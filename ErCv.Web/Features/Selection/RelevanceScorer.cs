using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErCv.Web.Features.Selection;

public class RelevanceScorer
{
    #region Constants

    public const int DefaultMaxSkills = 15;

    private const int MaxSkillPoints = 40;
    private const double SkillPointsPerMatch = 4.0;
    private const double BulletSkillHalfCredit = 2.0;
    private const double KeywordPointsPerMatch = 2.0;
    private const int MaxKeywordsEvaluated = 15;

    // Recency: how many years ago the role ended
    private const double RecencyPresent = 18.0;
    private const double RecencyWithinOneYear = 16.0;
    private const double RecencyWithinTwoYears = 13.0;
    private const double RecencyWithinThreeYears = 10.0;
    private const double RecencyWithinFiveYears = 7.0;
    private const double RecencyOlder = 4.0;
    private const double RecencyUnknown = 5.0;

    // Seniority fit: distance in levels between the role and the job (see MatchScoreConstants.SeniorityMap)
    private const double SeniorityExact = 10.0;
    private const double SeniorityOneLevel = 7.0;
    private const double SeniorityTwoLevels = 4.0;
    private const double SeniorityFar = 1.0;
    private const double SeniorityUnknown = 5.0;

    // Top skills: a skill the job asks for, plus a bonus per selected role that lists it
    private const double TopSkillJobMatch = 10.0;
    private const double TopSkillPerRole = 3.0;
    private const double TopSkillFiller = 0.5;

    #endregion

    #region Fields

    private readonly ILogger<RelevanceScorer> _logger;

    #endregion

    #region Constructors

    public RelevanceScorer(ILogger<RelevanceScorer>? logger = null)
    {
        _logger = logger ?? NullLogger<RelevanceScorer>.Instance;
    }

    #endregion

    #region Public Methods

    public List<RelevanceScore> ScoreExperiences(
        List<Experience> experiences,
        JobRequirements requirements)
    {
        return experiences
            .Select(exp => CalculateRelevanceScore(exp, requirements))
            .OrderByDescending(s => s.Score)
            .ToList();
    }

    public List<string> GetTopSkills(
        List<Experience> experiences,
        JobRequirements requirements,
        Skills allSkills,
        int maxSkills = DefaultMaxSkills)
    {
        var skillScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var allCVSkills = FlattenSkills(allSkills);

        foreach (var skill in allCVSkills)
        {
            double score = 0;

            if (SkillNormalizer.MatchesAny(skill, requirements.TechnicalSkills))
                score += TopSkillJobMatch;

            foreach (var exp in experiences)
            {
                if (exp.Skills?.Any(s => SkillNormalizer.FuzzyMatch(s, skill)) == true)
                    score += TopSkillPerRole;
            }

            if (score > 0 && !skillScores.ContainsKey(skill))
                skillScores[skill] = score;
        }

        if (skillScores.Count < maxSkills)
        {
            foreach (var skill in allCVSkills)
            {
                if (!skillScores.ContainsKey(skill) && skillScores.Count < maxSkills)
                    skillScores[skill] = TopSkillFiller;
            }
        }

        return skillScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(maxSkills)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public static List<string> FlattenSkills(Skills? skills)
    {
        var all = new List<string>();
        if (skills == null) return all;
        all.AddRange(skills.Languages ?? new());
        all.AddRange(skills.Frameworks ?? new());
        all.AddRange(skills.Databases ?? new());
        all.AddRange(skills.Tools ?? new());
        all.AddRange(skills.Cloud ?? new());
        all.AddRange(skills.Practices ?? new());
        return all;
    }

    #endregion

    #region Private Methods

    private RelevanceScore CalculateRelevanceScore(
        Experience experience,
        JobRequirements requirements)
    {
        double skillPoints = 0;
        var matchedSkills = new List<string>();
        if (experience.Skills != null && requirements.TechnicalSkills.Any())
        {
            foreach (var reqSkill in requirements.TechnicalSkills)
            {
                var match = experience.Skills.FirstOrDefault(s =>
                    SkillNormalizer.FuzzyMatch(s, reqSkill));

                if (match != null)
                {
                    skillPoints += SkillPointsPerMatch;
                    if (!SkillNormalizer.MatchesAny(match, matchedSkills))
                        matchedSkills.Add(match);
                }
            }
            var bulletText = string.Join(" ", experience.Bullets ?? new List<string>());
            foreach (var reqSkill in requirements.TechnicalSkills)
            {
                if (!SkillNormalizer.MatchesAny(reqSkill, matchedSkills) &&
                    SkillNormalizer.ContainsTerm(bulletText, reqSkill))
                {
                    skillPoints += BulletSkillHalfCredit;
                    matchedSkills.Add(reqSkill);
                }
            }

            skillPoints = Math.Min(skillPoints, MaxSkillPoints);
        }

        double keywordPoints = 0;
        var matchedKeywords = new List<string>();
        var fullBulletText = string.Join(" ", experience.Bullets ?? new List<string>()).ToLower();
        foreach (var keyword in requirements.Keywords.Take(MaxKeywordsEvaluated))
        {
            if (SkillNormalizer.ContainsTerm(fullBulletText, keyword))
            {
                keywordPoints += KeywordPointsPerMatch;
                matchedKeywords.Add(keyword);
            }
        }

        var parts = new List<ScorePart>
        {
            new() { Label = SelectionConstants.LabelMatchedSkills, Points = skillPoints, Detail = Join(matchedSkills) },
            new() { Label = SelectionConstants.LabelKeywords, Points = keywordPoints, Detail = Join(matchedKeywords) },
            new() { Label = SelectionConstants.LabelRecency, Points = CalculateRecencyScore(experience.EndDate), Detail = experience.EndDate },
            new() { Label = SelectionConstants.LabelSeniorityFit, Points = CalculateSeniorityScore(experience.Role, requirements.SeniorityLevel), Detail = experience.Role }
        };

        return new RelevanceScore
        {
            Experience = experience,
            Score = parts.Sum(p => p.Points),
            MatchedSkills = matchedSkills,
            Parts = parts
        };
    }

    private static string? Join(List<string> items) => items.Count == 0 ? null : string.Join(", ", items);

    private double CalculateRecencyScore(string? endDate)
    {
        if (string.IsNullOrWhiteSpace(endDate)) return RecencyUnknown;
        if (DateParser.IsPresent(endDate)) return RecencyPresent;

        if (DateParser.TryParse(endDate, out var year, out _))
        {
            var diff = DateTime.Now.Year - year;
            if (diff <= 1) return RecencyWithinOneYear;
            if (diff <= 2) return RecencyWithinTwoYears;
            if (diff <= 3) return RecencyWithinThreeYears;
            if (diff <= 5) return RecencyWithinFiveYears;
            return RecencyOlder;
        }

        _logger.LogWarning("Could not parse endDate '{EndDate}', using fallback score", endDate);

        return RecencyUnknown;
    }

    private double CalculateSeniorityScore(string? role, string? targetSeniority)
    {
        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(targetSeniority))
            return SeniorityUnknown;

        var roleLevel = MatchScoreConstants.DefaultLevel;
        var targetLevel = MatchScoreConstants.DefaultLevel;

        foreach (var kvp in MatchScoreConstants.SeniorityMap)
        {
            if (SkillNormalizer.ContainsTerm(role, kvp.Key)) roleLevel = kvp.Value;
            if (SkillNormalizer.ContainsTerm(targetSeniority, kvp.Key)) targetLevel = kvp.Value;
        }

        return Math.Abs(roleLevel - targetLevel) switch
        {
            0 => SeniorityExact, 1 => SeniorityOneLevel, 2 => SeniorityTwoLevels, _ => SeniorityFar
        };
    }

    #endregion
}

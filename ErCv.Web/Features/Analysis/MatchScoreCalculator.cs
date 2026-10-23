using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Analysis;

public class MatchScoreCalculator
{
    #region Public Methods

    /// <summary>The match of the CV as it stands: the skills and bullets it prints (EDIT-08).</summary>
    public MatchScore Calculate(GeneratedCV cv, JobRequirements requirements) =>
        Calculate(CvAsProfile.Of(cv), requirements);

    public MatchScore Calculate(CVMaster cv, JobRequirements requirements)
    {
        var allCVSkills = RelevanceScorer.FlattenSkills(cv.Skills);
        var totalSkills = Math.Max(requirements.TechnicalSkills.Count, 1);

        var matchedSkills = requirements.TechnicalSkills
            .Count(reqSkill => SkillNormalizer.MatchesAny(reqSkill, allCVSkills));

        var skillPercentage = (double)matchedSkills / totalSkills * MatchScoreConstants.MaxScore;
        var allBullets = cv.Experience
            .SelectMany(e => e.Bullets ?? new List<string>())
            .Select(b => b.ToLower())
            .ToList();

        var evaluatedKeywords = requirements.Keywords
            .Take(MatchScoreConstants.MaxKeywordEvaluation)
            .ToList();

        var totalKeywords = Math.Max(evaluatedKeywords.Count, 1);

        var matchedKeywords = evaluatedKeywords
            .Count(k => allBullets.Any(b => SkillNormalizer.ContainsTerm(b, k)));

        var keywordPercentage = Math.Min(
            (double)matchedKeywords / totalKeywords * MatchScoreConstants.MaxScore *
            MatchScoreConstants.KeywordBoostMultiplier,
            MatchScoreConstants.MaxScore);
        var seniorityScore = CalculateSeniorityMatch(cv, requirements);

        var overall =
            (skillPercentage * MatchScoreConstants.SkillWeight) +
            (keywordPercentage * MatchScoreConstants.KeywordWeight) +
            (seniorityScore * MatchScoreConstants.SeniorityWeight);

        overall = Math.Min(Math.Round(overall), MatchScoreConstants.MaxScore);
        var isLowMatch = overall < MatchScoreConstants.LowMatchThreshold;

        return new MatchScore
        {
            Overall = (int)overall,
            SkillsMatched = matchedSkills,
            TotalSkills = totalSkills,
            KeywordsMatched = matchedKeywords,
            TotalKeywords = totalKeywords,
            SeniorityMatch = seniorityScore >= MatchScoreConstants.CloseMatchScore,
            ColorClass = ResolveColorClass(overall),
            LowMatchWarning = isLowMatch
                ? MatchScoreConstants.LowMatchWarning
                : null
        };
    }

    #endregion

    #region Private Methods

    private double CalculateSeniorityMatch(CVMaster cv, JobRequirements requirements)
    {
        if (string.IsNullOrEmpty(requirements.SeniorityLevel))
            return MatchScoreConstants.DefaultSeniorityScore;

        var seniorityMap = MatchScoreConstants.SeniorityMap;

        var mostRecentRole = cv.Experience
            .OrderByDescending(e => DateParser.RecencyKey(e.StartDate, e.EndDate))
            .FirstOrDefault()
            ?.Role?.ToLower() ?? "";

        var cvLevel = MatchScoreConstants.DefaultLevel;
        var targetLevel = MatchScoreConstants.DefaultLevel;

        foreach (var kvp in seniorityMap)
        {
            if (SkillNormalizer.ContainsTerm(mostRecentRole, kvp.Key)) cvLevel = kvp.Value;
            if (SkillNormalizer.ContainsTerm(requirements.SeniorityLevel, kvp.Key))
                targetLevel = kvp.Value;
        }

        var diff = Math.Abs(cvLevel - targetLevel);
        return diff switch
        {
            0 => MatchScoreConstants.PerfectMatchScore,
            1 => MatchScoreConstants.CloseMatchScore,
            2 => MatchScoreConstants.PartialMatchScore,
            _ => MatchScoreConstants.LowMatchScore
        };
    }

    private string ResolveColorClass(double overall)
    {
        if (overall >= MatchScoreConstants.GreenThreshold) return MatchScoreConstants.GreenClass;
        if (overall >= MatchScoreConstants.YellowThreshold) return MatchScoreConstants.YellowClass;
        return MatchScoreConstants.RedClass;
    }

    #endregion
}

using ErCv.Web.Constants;
using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.Matching;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Selection;

public class CVSelector
{
    #region Constants

    private const int MaxExperiencesOnePage = 3;
    private const int MaxExperiencesTwoPages = 4;
    private const int MaxBulletsOnePage = 3;
    private const int MaxBulletsTwoPages = 4;
    private const int MaxProjectsOnePage = 1;
    private const int MaxProjectsTwoPages = 2;

    // A non-tech role keeps this share of its score when the job description is technical
    private const double NonTechRoleFactor = 0.4;

    // Bullet scoring: points per hit or per trait
    private const double BulletPointsPerMatchedSkill = 10.0;
    private const double BulletPointsPerJdSkill = 8.0;
    private const double BulletPointsPerKeyword = 3.0;
    private const double BulletPointsMetric = 6.0;
    private const double BulletPointsActionVerb = 4.0;
    private const int MaxKeywordsPerBullet = 10;

    private const int ShortBulletLength = 40;
    private const int LongBulletLength = 80;
    private const int VeryLongBulletLength = 120;
    private const double ShortBulletPenalty = -5.0;
    private const double LongBulletBonus = 2.0;
    private const double VeryLongBulletBonus = 1.0;

    // Project ranking: points per matching technology and per keyword in the description
    private const double ProjectPointsPerTech = 5.0;
    private const double ProjectPointsPerKeyword = 2.0;
    private const int MaxKeywordsPerProject = 10;

    private static readonly string[] StrongActionVerbs =
    {
        "built", "developed", "designed", "implemented", "architected",
        "led", "launched", "delivered", "reduced", "improved", "increased",
        "optimized", "automated", "migrated", "refactored", "scaled",
        "deployed", "integrated", "created", "established", "streamlined",
        "engineered", "shipped", "transformed", "drove", "accelerated"
    };

    #endregion

    #region Fields

    private readonly RelevanceScorer _scorer;
    private readonly PageConstraintHelper _pageConstraint;
    private readonly SummaryGenerator _summaryGenerator;

    #endregion

    #region Constructors

    /// <summary>Builds its own parts without logging, for callers outside the container (the collector, tests).</summary>
    public CVSelector()
        : this(new RelevanceScorer(), new PageConstraintHelper(), new SummaryGenerator())
    {
    }

    public CVSelector(RelevanceScorer scorer, PageConstraintHelper pageConstraint, SummaryGenerator summaryGenerator)
    {
        _scorer = scorer;
        _pageConstraint = pageConstraint;
        _summaryGenerator = summaryGenerator;
    }

    #endregion

    #region Public Methods

    public GeneratedCV SelectAndAdaptCV(
        CVMaster cvMaster,
        JobRequirements requirements,
        bool allowTwoPages = false,
        string language = ApiConstants.LanguageEnglish) =>
        SelectWithExplanation(cvMaster, requirements, allowTwoPages, language).Cv;

    /// <summary>Same selection as <see cref="SelectAndAdaptCV"/>, plus why each experience and bullet scored as it did.</summary>
    public (GeneratedCV Cv, SelectionExplanation Explanation) SelectWithExplanation(
        CVMaster cvMaster,
        JobRequirements requirements,
        bool allowTwoPages = false,
        string language = ApiConstants.LanguageEnglish)
    {
        var maxExperiences = allowTwoPages ? MaxExperiencesTwoPages : MaxExperiencesOnePage;
        var maxBulletsPerExp = allowTwoPages ? MaxBulletsTwoPages : MaxBulletsOnePage;

        var isTechJD = requirements.TechnicalSkills.Any();

        var scoredExperiences = _scorer.ScoreExperiences(cvMaster.Experience, requirements);

        if (isTechJD)
            scoredExperiences = PrioritizeTechExperiences(scoredExperiences, maxExperiences);

        var selected = SelectExperiences(
            scoredExperiences, requirements, maxExperiences, maxBulletsPerExp);
        var selectedExperiences = selected.Select(x => x.Experience).ToList();

        var topSkills = _scorer.GetTopSkills(
            selectedExperiences, requirements, cvMaster.Skills, maxSkills: RelevanceScorer.DefaultMaxSkills);

        var summary = _summaryGenerator.GenerateSummary(
            cvMaster, requirements, selectedExperiences, topSkills, language);

        var generatedCV = new GeneratedCV
        {
            Header = CvCloner.Clone(cvMaster.Header),
            Summary = summary,
            Education = cvMaster.Education.Select(CvCloner.Clone).ToList(),
            Experience = selectedExperiences,
            Skills = topSkills,
            Projects = new List<Project>(),
            Language = language
        };

        if (cvMaster.Projects?.Any() == true)
        {
            var maxProjects = allowTwoPages ? MaxProjectsTwoPages : MaxProjectsOnePage;
            var rankedProjects = RankProjects(cvMaster.Projects, requirements);

            foreach (var project in rankedProjects.Take(maxProjects))
            {
                if (_pageConstraint.CanFitProject(generatedCV, allowTwoPages))
                    generatedCV.Projects.Add(CvCloner.Clone(project));
            }
        }

        generatedCV = _pageConstraint.EnforcePageConstraint(generatedCV, allowTwoPages);
        return (generatedCV, BuildExplanation(generatedCV, selected.Select(x => x.Explanation)));
    }

    /// <summary>The measured page fit (MATCH-08) can trim after selection; this narrows the explanation to the final CV.</summary>
    public static SelectionExplanation FollowFinalCv(SelectionExplanation explanation, GeneratedCV finalCv) =>
        BuildExplanation(finalCv, explanation.Experiences);

    #endregion

    #region Private Methods

    // The page constraint can trim after selection, so the explanation follows the final CV.
    private static SelectionExplanation BuildExplanation(
        GeneratedCV finalCv, IEnumerable<ExperienceExplanation> explanations)
    {
        var byKey = new Dictionary<string, ExperienceExplanation>();
        foreach (var e in explanations) byKey.TryAdd(e.Key, e);

        var result = new SelectionExplanation();
        foreach (var exp in finalCv.Experience)
        {
            if (!byKey.TryGetValue(ExperienceExplanation.KeyOf(exp), out var explanation)) continue;

            foreach (var bullet in explanation.Bullets)
                bullet.Selected = bullet.Selected && exp.Bullets.Contains(bullet.Text);

            result.Experiences.Add(explanation);
        }
        return result;
    }

    private List<RelevanceScore> PrioritizeTechExperiences(
        List<RelevanceScore> scored, int maxNeeded)
    {
        var tech = scored.Where(s => IsTechExperience(s.Experience)).ToList();
        var nonTech = scored.Where(s => !IsTechExperience(s.Experience)).ToList();

        if (tech.Count >= maxNeeded) return tech;

        var needed = maxNeeded - tech.Count;
        var penalized = nonTech
            .Select(s => new RelevanceScore
            {
                Experience = s.Experience,
                Score = s.Score * NonTechRoleFactor,
                MatchedSkills = s.MatchedSkills,
                Parts = s.Parts.Append(new ScorePart
                {
                    Label = SelectionConstants.LabelNonTechPenalty,
                    Points = s.Score * NonTechRoleFactor - s.Score,
                    Detail = SelectionConstants.DetailNonTechPenalty
                }).ToList()
            })
            .Take(needed)
            .ToList();

        return tech.Concat(penalized).OrderByDescending(s => s.Score).ToList();
    }

    private bool IsTechExperience(Experience e) =>
        !string.IsNullOrEmpty(e.Role) &&
        SelectionConstants.TechRoleKeywords.Any(k => e.Role.Contains(k, StringComparison.OrdinalIgnoreCase));

    private List<(Experience Experience, ExperienceExplanation Explanation)> SelectExperiences(
        List<RelevanceScore> scored,
        JobRequirements requirements,
        int maxExperiences,
        int maxBulletsPerExp)
    {
        return scored
            .Take(maxExperiences)
            .Select(s => AdaptExperience(s, requirements, maxBulletsPerExp))
            .ToList();
    }

    private (Experience Experience, ExperienceExplanation Explanation) AdaptExperience(
        RelevanceScore relevance,
        JobRequirements requirements,
        int maxBullets = MaxBulletsOnePage)
    {
        var original = relevance.Experience;
        var bulletScores = ScoreBullets(
            original.Bullets ?? new List<string>(), relevance.MatchedSkills, requirements);

        var chosen = bulletScores
            .OrderByDescending(bs => bs.Score)
            .Take(maxBullets)
            .ToList();
        var selected = chosen
            .OrderBy(bs => bs.OriginalIndex)  // preserve original order
            .Select(bs => bs.Bullet)
            .ToList();

        var explanation = new ExperienceExplanation
        {
            Role = original.Role,
            Company = original.Company,
            StartDate = original.StartDate,
            EndDate = original.EndDate,
            Score = relevance.Score,
            Parts = relevance.Parts,
            Bullets = bulletScores.Select(bs => new BulletExplanation
            {
                Text = bs.Bullet,
                Score = bs.Score,
                Parts = bs.Parts,
                Selected = chosen.Contains(bs)
            }).ToList()
        };

        var experience = new Experience
        {
            Role = original.Role,
            Company = original.Company,
            Location = original.Location,
            StartDate = original.StartDate,
            EndDate = original.EndDate,
            Skills = (original.Skills ?? new List<string>()).ToList(),
            Bullets = selected
        };

        return (experience, explanation);
    }

    private List<BulletScore> ScoreBullets(
        List<string> bullets,
        List<string> matchedSkills,
        JobRequirements requirements)
    {
        var scores = new List<BulletScore>();

        for (int i = 0; i < bullets.Count; i++)
        {
            var bullet = bullets[i];
            var parts = new List<ScorePart>();

            var skillHits = matchedSkills.Where(skill => SkillNormalizer.ContainsSkill(bullet, skill)).ToList();
            AddPart(parts, SelectionConstants.LabelMatchedSkills, skillHits.Count * BulletPointsPerMatchedSkill, skillHits);

            var jdSkillHits = requirements.TechnicalSkills
                .Where(techSkill => SkillNormalizer.MatchesAny(techSkill, new[] { bullet })).ToList();
            AddPart(parts, SelectionConstants.LabelJdSkills, jdSkillHits.Count * BulletPointsPerJdSkill, jdSkillHits);

            var keywordHits = requirements.Keywords.Take(MaxKeywordsPerBullet)
                .Where(keyword => SkillNormalizer.ContainsTerm(bullet, keyword)).ToList();
            AddPart(parts, SelectionConstants.LabelKeywords, keywordHits.Count * BulletPointsPerKeyword, keywordHits);

            if (ATSConstants.Metrics().IsMatch(bullet))
                parts.Add(new ScorePart { Label = SelectionConstants.LabelMetric, Points = BulletPointsMetric, Detail = SelectionConstants.DetailMetric });

            var firstWord = bullet.Split(' ').FirstOrDefault()?.ToLower() ?? "";
            if (StrongActionVerbs.Any(v => firstWord.StartsWith(v, StringComparison.OrdinalIgnoreCase)))
                parts.Add(new ScorePart { Label = SelectionConstants.LabelActionVerb, Points = BulletPointsActionVerb, Detail = firstWord });

            var length = 0.0;
            if (bullet.Length < ShortBulletLength) length += ShortBulletPenalty;
            if (bullet.Length > LongBulletLength) length += LongBulletBonus;
            if (bullet.Length > VeryLongBulletLength) length += VeryLongBulletBonus;
            if (length != 0)
                parts.Add(new ScorePart { Label = SelectionConstants.LabelLength, Points = length, Detail = string.Format(SelectionConstants.DetailLength, bullet.Length) });

            scores.Add(new BulletScore
            {
                Bullet = bullet,
                Score = parts.Sum(p => p.Points),
                OriginalIndex = i,
                Parts = parts
            });
        }

        return scores;
    }

    private static void AddPart(List<ScorePart> parts, string label, double points, List<string> hits)
    {
        if (hits.Count > 0)
            parts.Add(new ScorePart { Label = label, Points = points, Detail = string.Join(", ", hits) });
    }

    private List<Project> RankProjects(List<Project> projects, JobRequirements requirements)
    {
        return projects
            .Select(p =>
            {
                double score = 0;

                foreach (var tech in p.Technologies ?? new List<string>())
                    if (SkillNormalizer.MatchesAny(tech, requirements.TechnicalSkills))
                        score += ProjectPointsPerTech;

                var text = (p.Description + " " +
                    string.Join(" ", p.Highlights ?? new List<string>())).ToLower();

                foreach (var kw in requirements.Keywords.Take(MaxKeywordsPerProject))
                    if (SkillNormalizer.ContainsTerm(text, kw)) score += ProjectPointsPerKeyword;

                return (Project: p, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Project)
            .ToList();
    }

    #endregion

    #region Nested Types

    private class BulletScore
    {
        public string Bullet { get; set; } = "";
        public double Score { get; set; }
        public int OriginalIndex { get; set; }
        public List<ScorePart> Parts { get; set; } = new();
    }

    #endregion
}

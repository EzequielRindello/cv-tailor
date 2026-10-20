using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class CVSelector
    {
        private readonly RelevanceScorer _scorer;
        private readonly PageConstraintHelper _pageConstraint;
        private readonly SummaryGenerator _summaryGenerator;

        private static readonly string[] TechRoleIndicators =
        {
            "developer", "engineer", "software", "programmer", "architect",
            "devops", "sre", "data", "backend", "frontend", "fullstack",
            "full stack", "full-stack", "tech", "cloud", "analyst",
            "consultant", "specialist"
        };
        private static readonly string[] StrongActionVerbs =
        {
            "built", "developed", "designed", "implemented", "architected",
            "led", "launched", "delivered", "reduced", "improved", "increased",
            "optimized", "automated", "migrated", "refactored", "scaled",
            "deployed", "integrated", "created", "established", "streamlined",
            "engineered", "shipped", "transformed", "drove", "accelerated"
        };

        public CVSelector(ILoggerFactory? loggerFactory = null)
        {
            _scorer = new RelevanceScorer(loggerFactory?.CreateLogger<RelevanceScorer>());
            _pageConstraint = new PageConstraintHelper();
            _summaryGenerator = new SummaryGenerator(loggerFactory?.CreateLogger<SummaryGenerator>());
        }

        public GeneratedCV SelectAndAdaptCV(
            CVMaster cvMaster,
            JobRequirements requirements,
            bool allowTwoPages = false)
        {
            var maxExperiences = allowTwoPages ? 4 : 3;
            var maxBulletsPerExp = allowTwoPages ? 4 : 3;

            var isTechJD = requirements.TechnicalSkills.Any();

            var scoredExperiences = _scorer.ScoreExperiences(cvMaster.Experience, requirements);

            if (isTechJD)
                scoredExperiences = PrioritizeTechExperiences(scoredExperiences, maxExperiences);

            var selectedExperiences = SelectExperiences(
                scoredExperiences, requirements, maxExperiences, maxBulletsPerExp);

            var topSkills = _scorer.GetTopSkills(
                selectedExperiences, requirements, cvMaster.Skills, maxSkills: 15);

            var summary = _summaryGenerator.GenerateSummary(
                cvMaster, requirements, selectedExperiences, topSkills);

            var generatedCV = new GeneratedCV
            {
                Header = CvCloner.Clone(cvMaster.Header),
                Summary = summary,
                Education = cvMaster.Education.Select(CvCloner.Clone).ToList(),
                Experience = selectedExperiences,
                Skills = topSkills,
                Projects = new List<Project>()
            };

            if (cvMaster.Projects?.Any() == true)
            {
                var maxProjects = allowTwoPages ? 2 : 1;
                var rankedProjects = RankProjects(cvMaster.Projects, requirements);

                foreach (var project in rankedProjects.Take(maxProjects))
                {
                    if (_pageConstraint.CanFitProject(generatedCV, allowTwoPages))
                        generatedCV.Projects.Add(CvCloner.Clone(project));
                }
            }

            generatedCV = _pageConstraint.EnforcePageConstraint(generatedCV, allowTwoPages);
            return generatedCV;
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
                    Score = s.Score * 0.4,
                    MatchedSkills = s.MatchedSkills
                })
                .Take(needed)
                .ToList();

            return tech.Concat(penalized).OrderByDescending(s => s.Score).ToList();
        }

        private bool IsTechExperience(Experience e) =>
            !string.IsNullOrEmpty(e.Role) &&
            TechRoleIndicators.Any(k => e.Role.Contains(k, StringComparison.OrdinalIgnoreCase));
        private List<Experience> SelectExperiences(
            List<RelevanceScore> scored,
            JobRequirements requirements,
            int maxExperiences,
            int maxBulletsPerExp)
        {
            return scored
                .Take(maxExperiences)
                .Select(s => AdaptExperience(s.Experience, s.MatchedSkills, requirements, maxBulletsPerExp))
                .ToList();
        }

        private Experience AdaptExperience(
            Experience original,
            List<string> matchedSkills,
            JobRequirements requirements,
            int maxBullets = 3)
        {
            var bulletScores = ScoreBullets(
                original.Bullets ?? new List<string>(), matchedSkills, requirements);

            var selected = bulletScores
                .OrderByDescending(bs => bs.Score)
                .Take(maxBullets)
                .OrderBy(bs => bs.OriginalIndex)  // preserve original order
                .Select(bs => bs.Bullet)
                .ToList();

            return new Experience
            {
                Role = original.Role,
                Company = original.Company,
                Location = original.Location,
                StartDate = original.StartDate,
                EndDate = original.EndDate,
                Skills = (original.Skills ?? new List<string>()).ToList(),
                Bullets = selected
            };
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
                double score = 0;
                foreach (var skill in matchedSkills)
                    if (bullet.Contains(skill, StringComparison.OrdinalIgnoreCase))
                        score += 10.0;
                foreach (var techSkill in requirements.TechnicalSkills)
                    if (SkillNormalizer.MatchesAny(techSkill, new[] { bullet }))
                        score += 8.0;
                foreach (var keyword in requirements.Keywords.Take(10))
                    if (bullet.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        score += 3.0;
                if (Regex.IsMatch(bullet, @"\d+[%x]|\d+[MK]\+?|[\d,]{3,}\+?"))
                    score += 6.0;
                var firstWord = bullet.Split(' ').FirstOrDefault()?.ToLower() ?? "";
                if (StrongActionVerbs.Any(v => firstWord.StartsWith(v, StringComparison.OrdinalIgnoreCase)))
                    score += 4.0;
                if (bullet.Length < 40) score -= 5.0;
                if (bullet.Length > 80) score += 2.0;
                if (bullet.Length > 120) score += 1.0;

                scores.Add(new BulletScore { Bullet = bullet, Score = score, OriginalIndex = i });
            }

            return scores;
        }
        private List<Project> RankProjects(List<Project> projects, JobRequirements requirements)
        {
            return projects
                .Select(p =>
                {
                    double score = 0;

                    foreach (var tech in p.Technologies ?? new List<string>())
                        if (SkillNormalizer.MatchesAny(tech, requirements.TechnicalSkills))
                            score += 5.0;

                    var text = (p.Description + " " +
                        string.Join(" ", p.Highlights ?? new List<string>())).ToLower();

                    foreach (var kw in requirements.Keywords.Take(10))
                        if (text.Contains(kw)) score += 2.0;

                    return (Project: p, Score: score);
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Project)
                .ToList();
        }

        private class BulletScore
        {
            public string Bullet { get; set; } = "";
            public double Score { get; set; }
            public int OriginalIndex { get; set; }
        }
    }
}

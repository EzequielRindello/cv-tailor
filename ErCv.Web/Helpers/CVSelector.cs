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
            bool allowTwoPages = false) =>
            SelectWithExplanation(cvMaster, requirements, allowTwoPages).Cv;

        /// <summary>Same selection as <see cref="SelectAndAdaptCV"/>, plus why each experience and bullet scored as it did.</summary>
        public (GeneratedCV Cv, SelectionExplanation Explanation) SelectWithExplanation(
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

            var selected = SelectExperiences(
                scoredExperiences, requirements, maxExperiences, maxBulletsPerExp);
            var selectedExperiences = selected.Select(x => x.Experience).ToList();

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
            return (generatedCV, BuildExplanation(generatedCV, selected.Select(x => x.Explanation)));
        }

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
                    Score = s.Score * 0.4,
                    MatchedSkills = s.MatchedSkills,
                    Parts = s.Parts.Append(new ScorePart
                    {
                        Label = "Non-tech role penalty",
                        Points = s.Score * 0.4 - s.Score,
                        Detail = "Tech job description, role is not a tech role"
                    }).ToList()
                })
                .Take(needed)
                .ToList();

            return tech.Concat(penalized).OrderByDescending(s => s.Score).ToList();
        }

        private bool IsTechExperience(Experience e) =>
            !string.IsNullOrEmpty(e.Role) &&
            TechRoleIndicators.Any(k => e.Role.Contains(k, StringComparison.OrdinalIgnoreCase));
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
            int maxBullets = 3)
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
                AddPart(parts, "Matched skills", skillHits.Count * 10.0, skillHits);

                var jdSkillHits = requirements.TechnicalSkills
                    .Where(techSkill => SkillNormalizer.MatchesAny(techSkill, new[] { bullet })).ToList();
                AddPart(parts, "JD skills", jdSkillHits.Count * 8.0, jdSkillHits);

                var keywordHits = requirements.Keywords.Take(10)
                    .Where(keyword => SkillNormalizer.ContainsTerm(bullet, keyword)).ToList();
                AddPart(parts, "Keywords", keywordHits.Count * 3.0, keywordHits);

                if (Regex.IsMatch(bullet, @"\d+[%x]|\d+[MK]\+?|[\d,]{3,}\+?"))
                    parts.Add(new ScorePart { Label = "Metric", Points = 6.0, Detail = "Contains a number or percentage" });

                var firstWord = bullet.Split(' ').FirstOrDefault()?.ToLower() ?? "";
                if (StrongActionVerbs.Any(v => firstWord.StartsWith(v, StringComparison.OrdinalIgnoreCase)))
                    parts.Add(new ScorePart { Label = "Action verb", Points = 4.0, Detail = firstWord });

                var length = 0.0;
                if (bullet.Length < 40) length -= 5.0;
                if (bullet.Length > 80) length += 2.0;
                if (bullet.Length > 120) length += 1.0;
                if (length != 0)
                    parts.Add(new ScorePart { Label = "Length", Points = length, Detail = $"{bullet.Length} characters" });

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
                            score += 5.0;

                    var text = (p.Description + " " +
                        string.Join(" ", p.Highlights ?? new List<string>())).ToLower();

                    foreach (var kw in requirements.Keywords.Take(10))
                        if (SkillNormalizer.ContainsTerm(text, kw)) score += 2.0;

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
            public List<ScorePart> Parts { get; set; } = new();
        }
    }
}

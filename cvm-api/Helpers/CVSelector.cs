using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class CVSelector
    {
        private readonly RelevanceScorer _scorer;
        private readonly PageConstraintHelper _pageConstraint;
        private readonly SummaryGenerator _summaryGenerator;

        public CVSelector()
        {
            _scorer = new RelevanceScorer();
            _pageConstraint = new PageConstraintHelper();
            _summaryGenerator = new SummaryGenerator();
        }

        public GeneratedCV SelectAndAdaptCV(
            CVMaster cvMaster,
            JobRequirements requirements,
            bool allowTwoPages = false)
        {
            var maxExperiences = allowTwoPages ? 4 : 3;
            var maxBulletsPerExp = allowTwoPages ? 4 : 3;

            // Score and rank experiences
            var scoredExperiences = _scorer.ScoreExperiences(cvMaster.Experience, requirements);

            // Select top N according to allowTwoPages
            var selectedExperiences = SelectExperiences(
                scoredExperiences, requirements, maxExperiences, maxBulletsPerExp);

            // Relevant Skills
            var topSkills = _scorer.GetTopSkills(
                selectedExperiences,
                requirements,
                cvMaster.Skills,
                maxSkills: 15);

            // Dynamic summary
            var summary = _summaryGenerator.GenerateSummary(
                cvMaster, requirements, selectedExperiences, topSkills);

            var generatedCV = new GeneratedCV
            {
                Header = cvMaster.Header,
                Summary = summary,
                Education = cvMaster.Education,
                Experience = selectedExperiences,
                Skills = topSkills,
                Projects = new List<Project>()
            };

            // Projects — only two pages
            if (cvMaster.Projects != null && cvMaster.Projects.Any())
            {
                var maxProjects = allowTwoPages ? 2 : 1;
                var rankedProjects = RankProjects(cvMaster.Projects, requirements);

                foreach (var project in rankedProjects.Take(maxProjects))
                {
                    if (_pageConstraint.CanFitProject(generatedCV, allowTwoPages))
                        generatedCV.Projects.Add(project);
                }
            }

            // Final page adjustment
            generatedCV = _pageConstraint.EnforcePageConstraint(generatedCV, allowTwoPages);

            return generatedCV;
        }

        private List<Experience> SelectExperiences(
            List<RelevanceScore> scoredExperiences,
            JobRequirements requirements,
            int maxExperiences,
            int maxBulletsPerExp)
        {
            var selected = new List<Experience>();

            foreach (var scored in scoredExperiences.Take(maxExperiences))
            {
                var adapted = AdaptExperience(scored.Experience, scored.MatchedSkills, requirements, maxBulletsPerExp);
                selected.Add(adapted);
            }

            return selected;
        }

        private Experience AdaptExperience(
            Experience original,
            List<string> matchedSkills,
            JobRequirements requirements,
            int maxBullets = 3)
        {
            var adapted = new Experience
            {
                Role = original.Role,
                Company = original.Company,
                Location = original.Location,
                StartDate = original.StartDate,
                EndDate = original.EndDate,
                Skills = original.Skills,
                Bullets = new List<string>()
            };

            var bulletScores = ScoreBullets(original.Bullets, matchedSkills, requirements);

            var selectedBullets = bulletScores
                .OrderByDescending(bs => bs.Score)
                .Take(maxBullets)
                .OrderBy(bs => bs.OriginalIndex)
                .Select(bs => bs.Bullet)
                .ToList();

            adapted.Bullets = selectedBullets;
            return adapted;
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

                foreach (var keyword in requirements.Keywords.Take(10))
                    if (bullet.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        score += 3.0;

                foreach (var techSkill in requirements.TechnicalSkills)
                    if (bullet.Contains(techSkill, StringComparison.OrdinalIgnoreCase))
                        score += 8.0;

                if (System.Text.RegularExpressions.Regex.IsMatch(bullet, @"\d+[%+]|\d+[MK]\+?|[\d,]+\+?"))
                    score += 5.0;

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
                        if (requirements.TechnicalSkills.Any(ts =>
                            tech.Contains(ts, StringComparison.OrdinalIgnoreCase) ||
                            ts.Contains(tech, StringComparison.OrdinalIgnoreCase)))
                            score += 5.0;

                    var projectText = (p.Description + " " + string.Join(" ", p.Highlights ?? new List<string>())).ToLower();
                    foreach (var keyword in requirements.Keywords.Take(10))
                        if (projectText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                            score += 2.0;

                    return (Project: p, Score: score);
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Project)
                .ToList();
        }

        private class BulletScore
        {
            public string Bullet { get; set; }
            public double Score { get; set; }
            public int OriginalIndex { get; set; }
        }
    }
}

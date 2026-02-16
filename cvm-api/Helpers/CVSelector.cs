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

        public GeneratedCV SelectAndAdaptCV(CVMaster cvMaster, JobRequirements requirements)
        {
            // Score and rank experiences
            var scoredExperiences = _scorer.ScoreExperiences(cvMaster.Experience, requirements);

            // Select top experiences that fit in one page
            var selectedExperiences = SelectExperiencesForOnePage(scoredExperiences, requirements);

            // Select relevant skills
            var topSkills = _scorer.GetTopSkills(
                selectedExperiences,
                requirements,
                cvMaster.Skills,
                maxSkills: 15);

            // Generate dynamic summary
            var summary = _summaryGenerator.GenerateSummary(
                cvMaster,
                requirements,
                selectedExperiences,
                topSkills);

            // Build generated CV
            var generatedCV = new GeneratedCV
            {
                Header = cvMaster.Header,
                Summary = summary,
                Education = cvMaster.Education,
                Experience = selectedExperiences,
                Skills = topSkills,
                Projects = new List<Project>()
            };

            // Optionally include top project if space allows
            if (cvMaster.Projects != null && cvMaster.Projects.Any())
            {
                var topProject = SelectRelevantProject(cvMaster.Projects, requirements);
                if (topProject != null && _pageConstraint.CanFitProject(generatedCV))
                {
                    generatedCV.Projects.Add(topProject);
                }
            }

            // Final adjustment to ensure one page
            generatedCV = _pageConstraint.EnforceOnePageConstraint(generatedCV);

            return generatedCV;
        }

        private List<Experience> SelectExperiencesForOnePage(
            List<RelevanceScore> scoredExperiences,
            JobRequirements requirements)
        {
            var selectedExperiences = new List<Experience>();

            // Start with top 3 most relevant experiences (was 4)
            var topExperiences = scoredExperiences.Take(3).ToList();

            foreach (var scored in topExperiences)
            {
                var adaptedExperience = AdaptExperience(scored.Experience, scored.MatchedSkills, requirements);
                selectedExperiences.Add(adaptedExperience);
            }

            return selectedExperiences;
        }

        private Experience AdaptExperience(
            Experience original,
            List<string> matchedSkills,
            JobRequirements requirements)
        {
            // Create a copy
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

            // Score and select best bullets
            var bulletScores = ScoreBullets(original.Bullets, matchedSkills, requirements);

            // Take top 3 bullets per experience (was 4)
            var selectedBullets = bulletScores
                .OrderByDescending(bs => bs.Score)
                .Take(3)
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

                // Score based on matched skills
                foreach (var skill in matchedSkills)
                {
                    if (bullet.Contains(skill, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10.0;
                    }
                }

                // Score based on keywords
                foreach (var keyword in requirements.Keywords.Take(10))
                {
                    if (bullet.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 3.0;
                    }
                }

                // Score based on technical skills
                foreach (var techSkill in requirements.TechnicalSkills)
                {
                    if (bullet.Contains(techSkill, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 8.0;
                    }
                }

                // Bonus for quantifiable results (numbers)
                if (System.Text.RegularExpressions.Regex.IsMatch(bullet, @"\d+[%+]|\d+[MK]\+?|[\d,]+\+?"))
                {
                    score += 5.0;
                }

                scores.Add(new BulletScore
                {
                    Bullet = bullet,
                    Score = score,
                    OriginalIndex = i
                });
            }

            return scores;
        }

        private Project SelectRelevantProject(List<Project> projects, JobRequirements requirements)
        {
            if (projects == null || !projects.Any())
                return null;

            double bestScore = 0;
            Project bestProject = null;

            foreach (var project in projects)
            {
                double score = 0;

                // Score based on technologies matching job requirements
                foreach (var tech in project.Technologies ?? new List<string>())
                {
                    if (requirements.TechnicalSkills.Any(ts => 
                        tech.Contains(ts, StringComparison.OrdinalIgnoreCase) ||
                        ts.Contains(tech, StringComparison.OrdinalIgnoreCase)))
                    {
                        score += 5.0;
                    }
                }

                // Score based on keywords in description
                var projectText = (project.Description + " " + string.Join(" ", project.Highlights ?? new List<string>())).ToLower();
                foreach (var keyword in requirements.Keywords.Take(10))
                {
                    if (projectText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 2.0;
                    }
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestProject = project;
                }
            }

            return bestProject;
        }

        private class BulletScore
        {
            public string Bullet { get; set; }
            public double Score { get; set; }
            public int OriginalIndex { get; set; }
        }
    }
}

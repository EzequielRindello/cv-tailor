using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class RelevanceScorer
    {
        public List<RelevanceScore> ScoreExperiences(
            List<Experience> experiences, 
            JobRequirements requirements)
        {
            var scores = new List<RelevanceScore>();

            foreach (var exp in experiences)
            {
                var score = CalculateRelevanceScore(exp, requirements);
                scores.Add(score);
            }

            return scores.OrderByDescending(s => s.Score).ToList();
        }

        private RelevanceScore CalculateRelevanceScore(
            Experience experience, 
            JobRequirements requirements)
        {
            double score = 0;
            var matchedSkills = new List<string>();

            // Score based on skill matching (40% weight)
            if (experience.Skills != null && requirements.TechnicalSkills.Any())
            {
                foreach (var reqSkill in requirements.TechnicalSkills)
                {
                    var match = experience.Skills.FirstOrDefault(s => 
                        s.Equals(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                        s.Contains(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                        reqSkill.Contains(s, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                    {
                        score += 4.0;
                        if (!matchedSkills.Contains(match))
                        {
                            matchedSkills.Add(match);
                        }
                    }
                }
            }

            // Score based on keyword presence in bullets (30% weight)
            var bulletText = string.Join(" ", experience.Bullets ?? new List<string>()).ToLower();
            
            foreach (var keyword in requirements.Keywords.Take(15))
            {
                if (bulletText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    score += 2.0;
                }
            }

            // Score based on recency (20% weight)
            if (experience.EndDate?.ToLower() == "present")
            {
                score += 20.0;
            }
            else
            {
                var recencyScore = CalculateRecencyScore(experience.EndDate);
                score += recencyScore;
            }

            // Score based on seniority match (10% weight)
            var seniorityScore = CalculateSeniorityScore(experience.Role, requirements.SeniorityLevel);
            score += seniorityScore;

            return new RelevanceScore
            {
                Experience = experience,
                Score = score,
                MatchedSkills = matchedSkills
            };
        }

        private double CalculateRecencyScore(string endDate)
        {
            if (string.IsNullOrWhiteSpace(endDate))
                return 5.0;

            try
            {
                // Try to parse the date
                var parts = endDate.Split(' ');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int year))
                {
                    var currentYear = DateTime.Now.Year;
                    var yearsDiff = currentYear - year;

                    if (yearsDiff <= 1) return 18.0;
                    if (yearsDiff <= 2) return 15.0;
                    if (yearsDiff <= 3) return 12.0;
                    if (yearsDiff <= 5) return 8.0;
                    return 5.0;
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return 5.0;
        }

        private double CalculateSeniorityScore(string role, string targetSeniority)
        {
            var roleNormalized = role.ToLower();
            var targetNormalized = targetSeniority.ToLower();

            var seniorityMap = new Dictionary<string, int>
            {
                { "intern", 1 },
                { "junior", 2 },
                { "entry", 2 },
                { "mid", 3 },
                { "senior", 4 },
                { "lead", 5 },
                { "principal", 6 },
                { "staff", 6 },
                { "architect", 6 }
            };

            var roleLevel = 3; // default mid-level
            var targetLevel = 3;

            foreach (var kvp in seniorityMap)
            {
                if (roleNormalized.Contains(kvp.Key))
                {
                    roleLevel = kvp.Value;
                }
                if (targetNormalized.Contains(kvp.Key))
                {
                    targetLevel = kvp.Value;
                }
            }

            // Perfect match
            if (roleLevel == targetLevel)
                return 10.0;

            // Close match (within 1 level)
            if (Math.Abs(roleLevel - targetLevel) == 1)
                return 7.0;

            // Partial match
            return 4.0;
        }

        public List<string> GetTopSkills(
            List<Experience> experiences, 
            JobRequirements requirements, 
            Skills allSkills, 
            int maxSkills = 15)
        {
            var skillScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // Flatten all skills from CV Master
            var allCVSkills = new List<string>();
            if (allSkills != null)
            {
                allCVSkills.AddRange(allSkills.Languages ?? new List<string>());
                allCVSkills.AddRange(allSkills.Frameworks ?? new List<string>());
                allCVSkills.AddRange(allSkills.Databases ?? new List<string>());
                allCVSkills.AddRange(allSkills.Tools ?? new List<string>());
                allCVSkills.AddRange(allSkills.Cloud ?? new List<string>());
                allCVSkills.AddRange(allSkills.Practices ?? new List<string>());
            }

            // Score skills based on job requirements
            foreach (var skill in allCVSkills)
            {
                double score = 0;

                // Check if mentioned in job requirements
                var isInRequirements = requirements.TechnicalSkills.Any(reqSkill =>
                    skill.Equals(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                    skill.Contains(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                    reqSkill.Contains(skill, StringComparison.OrdinalIgnoreCase));

                if (isInRequirements)
                {
                    score += 10.0;
                }

                // Check how many selected experiences use this skill
                foreach (var exp in experiences)
                {
                    if (exp.Skills?.Any(s => s.Equals(skill, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        score += 3.0;
                    }
                }

                if (score > 0 && !skillScores.ContainsKey(skill))
                {
                    skillScores[skill] = score;
                }
            }

            return skillScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(maxSkills)
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }
}

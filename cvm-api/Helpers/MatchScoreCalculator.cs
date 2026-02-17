using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class MatchScoreCalculator
    {
        public MatchScore Calculate(CVMaster cv, JobRequirements requirements)
        {
            // Skills Score
            var allCVSkills = FlattenSkills(cv.Skills);
            var totalSkills = Math.Max(requirements.TechnicalSkills.Count, 1);

            var matchedSkills = requirements.TechnicalSkills
                .Where(reqSkill => allCVSkills.Any(s =>
                    s.Equals(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                    s.Contains(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                    reqSkill.Contains(s, StringComparison.OrdinalIgnoreCase)))
                .Count();

            var skillPercentage = (double)matchedSkills / totalSkills * 100;

            // Keywords Score
            // Search for JD keywords in all CV bullet points
            var allBullets = cv.Experience
                .SelectMany(e => e.Bullets ?? new List<string>())
                .Select(b => b.ToLower())
                .ToList();

            var totalKeywords = Math.Max(requirements.Keywords.Take(15).Count(), 1);
            var matchedKeywords = requirements.Keywords
                .Take(15)
                .Count(k => allBullets.Any(b => b.Contains(k.ToLower())));

            // Normalize: We expect each keyword to appear ~2 times in bullet points
            var keywordPercentage = Math.Min((double)matchedKeywords / totalKeywords * 100 * 1.5, 100);

            // Seniority Score
            var seniorityScore = CalculateSeniorityMatch(cv, requirements);

            var overall = (skillPercentage * 0.5) + (keywordPercentage * 0.3) + (seniorityScore * 0.2);
            overall = Math.Min(Math.Round(overall), 100);

            return new MatchScore
            {
                Overall = (int)overall,
                SkillsMatched = matchedSkills,
                TotalSkills = totalSkills,
                KeywordsMatched = matchedKeywords,
                TotalKeywords = totalKeywords,
                SeniorityMatch = seniorityScore >= 70,
                ColorClass = overall >= 80 ? "green" : overall >= 60 ? "yellow" : "red"
            };
        }

        private double CalculateSeniorityMatch(CVMaster cv, JobRequirements requirements)
        {
            if (string.IsNullOrEmpty(requirements.SeniorityLevel))
                return 70;

            var seniorityMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "intern", 1 }, { "junior", 2 }, { "entry", 2 }, { "entry-level", 2 },
                { "mid", 3 }, { "mid-level", 3 }, { "senior", 4 }, { "lead", 5 },
                { "principal", 6 }, { "staff", 6 }, { "architect", 6 }
            };

            // Detect CV seniority from most recent role
            var mostRecentRole = cv.Experience
                .OrderByDescending(e => e.EndDate?.ToLower() == "present" ? "zzz" : e.EndDate ?? "")
                .FirstOrDefault()?.Role?.ToLower() ?? "";

            var cvLevel = 3;
            foreach (var kvp in seniorityMap)
                if (mostRecentRole.Contains(kvp.Key)) { cvLevel = kvp.Value; break; }

            var targetLevel = 3;
            foreach (var kvp in seniorityMap)
                if (requirements.SeniorityLevel.Contains(kvp.Key)) { targetLevel = kvp.Value; break; }

            var diff = Math.Abs(cvLevel - targetLevel);
            if (diff == 0) return 100;
            if (diff == 1) return 70;
            if (diff == 2) return 40;
            return 10;
        }

        public static List<string> FlattenSkills(Skills skills)
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
    }
}

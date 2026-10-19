using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class RelevanceScorer
    {
        private const int MaxSkillPoints = 40;
        private const double SkillPointsPerMatch = 4.0;
        private const double BulletSkillHalfCredit = 2.0;
        private const double KeywordPointsPerMatch = 2.0;
        private const int MaxKeywordsEvaluated = 15;

        public List<RelevanceScore> ScoreExperiences(
            List<Experience> experiences,
            JobRequirements requirements)
        {
            return experiences
                .Select(exp => CalculateRelevanceScore(exp, requirements))
                .OrderByDescending(s => s.Score)
                .ToList();
        }

        private RelevanceScore CalculateRelevanceScore(
            Experience experience,
            JobRequirements requirements)
        {
            double score = 0;
            var matchedSkills = new List<string>();
            if (experience.Skills != null && requirements.TechnicalSkills.Any())
            {
                foreach (var reqSkill in requirements.TechnicalSkills)
                {
                    var match = experience.Skills.FirstOrDefault(s =>
                        SkillNormalizer.FuzzyMatch(s, reqSkill));

                    if (match != null)
                    {
                        score += SkillPointsPerMatch;
                        if (!SkillNormalizer.MatchesAny(match, matchedSkills))
                            matchedSkills.Add(match);
                    }
                }
                var bulletText = string.Join(" ", experience.Bullets ?? new List<string>());
                foreach (var reqSkill in requirements.TechnicalSkills)
                {
                    if (!SkillNormalizer.MatchesAny(reqSkill, matchedSkills) &&
                        ContainsTerm(bulletText, reqSkill))
                    {
                        score += BulletSkillHalfCredit;
                        matchedSkills.Add(reqSkill);
                    }
                }

                score = Math.Min(score, MaxSkillPoints);
            }
            var fullBulletText = string.Join(" ", experience.Bullets ?? new List<string>()).ToLower();
            foreach (var keyword in requirements.Keywords.Take(MaxKeywordsEvaluated))
            {
                if (ContainsTerm(fullBulletText, keyword))
                    score += KeywordPointsPerMatch;
            }
            score += CalculateRecencyScore(experience.EndDate);
            score += CalculateSeniorityScore(experience.Role, requirements.SeniorityLevel);

            return new RelevanceScore
            {
                Experience = experience,
                Score = score,
                MatchedSkills = matchedSkills
            };
        }

        private double CalculateRecencyScore(string? endDate)
        {
            if (string.IsNullOrWhiteSpace(endDate)) return 5.0;
            if (IsPresent(endDate)) return 18.0;

            if (TryExtractYear(endDate, out var year))
            {
                var diff = DateTime.Now.Year - year;
                if (diff <= 1) return 16.0;
                if (diff <= 2) return 13.0;
                if (diff <= 3) return 10.0;
                if (diff <= 5) return 7.0;
                return 4.0;
            }

            Console.WriteLine($"[WARN] RelevanceScorer: could not parse endDate '{endDate}', using fallback score.");

            return 5.0;
        }

        private static bool IsPresent(string value) =>
            value.Trim().Equals("present", StringComparison.OrdinalIgnoreCase) ||
            value.Trim().Equals("current", StringComparison.OrdinalIgnoreCase) ||
            value.Trim().Equals("now", StringComparison.OrdinalIgnoreCase);

        private static bool TryExtractYear(string? value, out int year)
        {
            year = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var match = Regex.Match(value, @"\b(19|20)\d{2}\b");
            if (!match.Success) return false;

            if (!int.TryParse(match.Value, out year)) return false;

            var currentYear = DateTime.Now.Year;
            return year >= 1950 && year <= currentYear + 1;
        }

        private static bool ContainsTerm(string text, string term)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term)) return false;
            var pattern = $@"(?<![A-Za-z0-9]){Regex.Escape(term.Trim())}(?![A-Za-z0-9])";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        private double CalculateSeniorityScore(string? role, string? targetSeniority)
        {
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(targetSeniority))
                return 5.0;

            var seniorityMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "intern", 1 }, { "junior", 2 }, { "entry", 2 }, { "associate", 2 },
                { "mid", 3 }, { "mid-level", 3 },
                { "senior", 4 }, { "lead", 5 }, { "principal", 6 },
                { "staff", 6 }, { "architect", 6 }
            };

            var roleLevel = 3;
            var targetLevel = 3;

            foreach (var kvp in seniorityMap)
            {
                if (role.ToLower().Contains(kvp.Key)) roleLevel = kvp.Value;
                if (targetSeniority.ToLower().Contains(kvp.Key)) targetLevel = kvp.Value;
            }

            return Math.Abs(roleLevel - targetLevel) switch
            {
                0 => 10.0, 1 => 7.0, 2 => 4.0, _ => 1.0
            };
        }

        public List<string> GetTopSkills(
            List<Experience> experiences,
            JobRequirements requirements,
            Skills allSkills,
            int maxSkills = 15)
        {
            var skillScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var allCVSkills = FlattenSkills(allSkills);

            foreach (var skill in allCVSkills)
            {
                double score = 0;

                if (SkillNormalizer.MatchesAny(skill, requirements.TechnicalSkills))
                    score += 10.0;

                foreach (var exp in experiences)
                {
                    if (exp.Skills?.Any(s => SkillNormalizer.FuzzyMatch(s, skill)) == true)
                        score += 3.0;
                }

                if (score > 0 && !skillScores.ContainsKey(skill))
                    skillScores[skill] = score;
            }

            if (skillScores.Count < maxSkills)
            {
                foreach (var skill in allCVSkills)
                {
                    if (!skillScores.ContainsKey(skill) && skillScores.Count < maxSkills)
                        skillScores[skill] = 0.5;
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
    }
}

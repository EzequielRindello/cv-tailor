using ErCvApi.Constants;
using ErCvApi.Models;
using System;
using System.Collections.Generic;

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
                .Count(reqSkill => allCVSkills.Any(s =>
                    s.Equals(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                    s.Contains(reqSkill, StringComparison.OrdinalIgnoreCase) ||
                    reqSkill.Contains(s, StringComparison.OrdinalIgnoreCase)));

            var skillPercentage =
                (double)matchedSkills / totalSkills * 100;

            // Keywords Score
            var allBullets = cv.Experience
                .SelectMany(e => e.Bullets ?? new List<string>())
                .Select(b => b.ToLower())
                .ToList();

            var evaluatedKeywords = requirements.Keywords
                .Take(MatchScoreConstants.MaxKeywordEvaluation)
                .ToList();

            var totalKeywords = Math.Max(evaluatedKeywords.Count, 1);

            var matchedKeywords = evaluatedKeywords
                .Count(k => allBullets.Any(b => b.Contains(k.ToLower())));

            var keywordPercentage = Math.Min(
                (double)matchedKeywords / totalKeywords * 100 *
                MatchScoreConstants.KeywordBoostMultiplier,
                100);

            // Seniority Score
            var seniorityScore = CalculateSeniorityMatch(cv, requirements);

            var overall =
                (skillPercentage * MatchScoreConstants.SkillWeight) +
                (keywordPercentage * MatchScoreConstants.KeywordWeight) +
                (seniorityScore * MatchScoreConstants.SeniorityWeight);

            overall = Math.Min(Math.Round(overall), 100);

            return new MatchScore
            {
                Overall = (int)overall,
                SkillsMatched = matchedSkills,
                TotalSkills = totalSkills,
                KeywordsMatched = matchedKeywords,
                TotalKeywords = totalKeywords,
                SeniorityMatch = seniorityScore >=
                                 MatchScoreConstants.CloseMatchScore,
                ColorClass = ResolveColorClass(overall)
            };
        }

        private double CalculateSeniorityMatch(
            CVMaster cv,
            JobRequirements requirements)
        {
            if (string.IsNullOrEmpty(requirements.SeniorityLevel))
                return MatchScoreConstants.DefaultSeniorityScore;

            var seniorityMap = MatchScoreConstants.SeniorityMap;

            var mostRecentRole = cv.Experience
                .OrderByDescending(e =>
                    e.EndDate?.ToLower() ==
                    MatchScoreConstants.PresentKeyword
                        ? "zzz"
                        : e.EndDate ?? "")
                .FirstOrDefault()
                ?.Role?.ToLower() ?? "";

            var cvLevel = MatchScoreConstants.DefaultLevel;

            foreach (var kvp in seniorityMap)
            {
                if (mostRecentRole.Contains(kvp.Key))
                {
                    cvLevel = kvp.Value;
                    break;
                }
            }

            var targetLevel = MatchScoreConstants.DefaultLevel;

            foreach (var kvp in seniorityMap)
            {
                if (requirements.SeniorityLevel
                    .Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    targetLevel = kvp.Value;
                    break;
                }
            }

            var diff = Math.Abs(cvLevel - targetLevel);

            if (diff == 0) return MatchScoreConstants.PerfectMatchScore;
            if (diff == 1) return MatchScoreConstants.CloseMatchScore;
            if (diff == 2) return MatchScoreConstants.PartialMatchScore;

            return MatchScoreConstants.LowMatchScore;
        }

        private string ResolveColorClass(double overall)
        {
            if (overall >= MatchScoreConstants.GreenThreshold)
                return MatchScoreConstants.GreenClass;

            if (overall >= MatchScoreConstants.YellowThreshold)
                return MatchScoreConstants.YellowClass;

            return MatchScoreConstants.RedClass;
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
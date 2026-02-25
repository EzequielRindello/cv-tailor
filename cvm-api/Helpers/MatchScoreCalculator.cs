using ErCvApi.Constants;
using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class MatchScoreCalculator
    {
        public MatchScore Calculate(CVMaster cv, JobRequirements requirements)
        {
            // ── Skills Score ─────────────────────────────────────────────────
            var allCVSkills = RelevanceScorer.FlattenSkills(cv.Skills);
            var totalSkills = Math.Max(requirements.TechnicalSkills.Count, 1);

            var matchedSkills = requirements.TechnicalSkills
                .Count(reqSkill => SkillNormalizer.MatchesAny(reqSkill, allCVSkills));

            var skillPercentage = (double)matchedSkills / totalSkills * 100;

            // ── Keywords Score ───────────────────────────────────────────────
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

            // ── Seniority Score ──────────────────────────────────────────────
            var seniorityScore = CalculateSeniorityMatch(cv, requirements);

            var overall =
                (skillPercentage * MatchScoreConstants.SkillWeight) +
                (keywordPercentage * MatchScoreConstants.KeywordWeight) +
                (seniorityScore * MatchScoreConstants.SeniorityWeight);

            overall = Math.Min(Math.Round(overall), 100);

            // ── Role mismatch warning ────────────────────────────────────────
            // 40% threshold: scores below this indicate the JD is a poor fit
            // (e.g. iOS JD against a backend CV scores ~35% due to React Native nice-to-have)
            var isLowMatch = overall < 40;

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
                    ? "Low match detected — this JD may not align with your profile. The generated CV may not be competitive."
                    : null
            };
        }

        private double CalculateSeniorityMatch(CVMaster cv, JobRequirements requirements)
        {
            if (string.IsNullOrEmpty(requirements.SeniorityLevel))
                return MatchScoreConstants.DefaultSeniorityScore;

            var seniorityMap = MatchScoreConstants.SeniorityMap;

            var mostRecentRole = cv.Experience
                .OrderByDescending(e =>
                    e.EndDate?.ToLower() == MatchScoreConstants.PresentKeyword ? "zzz" : e.EndDate ?? "")
                .FirstOrDefault()
                ?.Role?.ToLower() ?? "";

            var cvLevel = MatchScoreConstants.DefaultLevel;
            var targetLevel = MatchScoreConstants.DefaultLevel;

            foreach (var kvp in seniorityMap)
            {
                if (mostRecentRole.Contains(kvp.Key)) cvLevel = kvp.Value;
                if (requirements.SeniorityLevel.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
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
    }
}

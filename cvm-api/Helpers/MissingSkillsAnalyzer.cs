using ErCvApi.Constants;
using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class MissingSkillsAnalyzer
    {
        private static readonly HashSet<string> NiceToHaveKeywords =
            new(MissingSkillsConstants.NiceToHaveKeywords, StringComparer.OrdinalIgnoreCase);

        public MissingSkillsAnalysis Analyze(CVMaster cv, JobRequirements requirements)
        {
            var allCVSkills = RelevanceScorer.FlattenSkills(cv.Skills);

            var allBulletsText = string.Join(" ",
                cv.Experience.SelectMany(e => e.Bullets ?? new List<string>())).ToLower();

            var requiredMissing = new List<string>();
            var niceToHaveMissing = new List<string>();

            foreach (var skill in requirements.TechnicalSkills)
            {
                // Fuzzy match against tagged skills
                var foundInSkills = SkillNormalizer.MatchesAny(skill, allCVSkills);

                // Fallback: plain contains in bullet text
                var foundInBullets = allBulletsText.Contains(skill.ToLower());

                if (!foundInSkills && !foundInBullets)
                {
                    if (NiceToHaveKeywords.Contains(skill) ||
                        SkillNormalizer.MatchesAny(skill, NiceToHaveKeywords))
                        niceToHaveMissing.Add(skill);
                    else
                        requiredMissing.Add(skill);
                }
            }

            var suggestions = BuildSuggestions(requiredMissing, niceToHaveMissing);

            var totalSkills = Math.Max(requirements.TechnicalSkills.Count, 1);
            var currentMatched = totalSkills - requiredMissing.Count - niceToHaveMissing.Count;
            var potentialMatched = currentMatched + requiredMissing.Count;

            var potentialIncrease = (int)Math.Round(
                (double)(potentialMatched - currentMatched) / totalSkills *
                MissingSkillsConstants.SkillWeightImpact);

            return new MissingSkillsAnalysis
            {
                RequiredMissing = requiredMissing.Take(MissingSkillsConstants.MaxRequiredMissing).ToList(),
                NiceToHaveMissing = niceToHaveMissing.Take(MissingSkillsConstants.MaxNiceToHaveMissing).ToList(),
                Suggestions = suggestions,
                PotentialScoreIncrease = Math.Min(potentialIncrease, MissingSkillsConstants.MaxPotentialIncrease)
            };
        }

        private List<string> BuildSuggestions(List<string> required, List<string> niceToHave)
        {
            var suggestions = new List<string>();

            foreach (var skill in required.Take(MissingSkillsConstants.MaxRequiredSuggestions))
                suggestions.Add(string.Format(MissingSkillsConstants.RequiredSuggestionTemplate, skill));

            foreach (var skill in niceToHave.Take(MissingSkillsConstants.MaxNiceToHaveSuggestions))
                suggestions.Add(string.Format(MissingSkillsConstants.NiceToHaveSuggestionTemplate, skill));

            if (required.Count > MissingSkillsConstants.MaxRequiredSuggestions)
                suggestions.Add(string.Format(
                    MissingSkillsConstants.ExtraRequiredSuggestionTemplate,
                    required.Count - MissingSkillsConstants.MaxRequiredSuggestions));

            return suggestions;
        }
    }
}

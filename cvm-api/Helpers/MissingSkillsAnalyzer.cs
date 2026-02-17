using ErCvApi.Models;

namespace ErCvApi.Helpers
{

    public class MissingSkillsAnalyzer
    {
        // Skills "nice to have"
        private static readonly HashSet<string> NiceToHaveKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "kubernetes", "terraform", "graphql", "redis", "kafka", "elasticsearch",
            "jenkins", "ansible", "prometheus", "grafana", "rabbitmq"
        };

        public MissingSkillsAnalysis Analyze(CVMaster cv, JobRequirements requirements)
        {
            var allCVSkills = MatchScoreCalculator.FlattenSkills(cv.Skills)
                .Select(s => s.ToLower())
                .ToHashSet();

            var allBulletsText = string.Join(" ",
                cv.Experience.SelectMany(e => e.Bullets ?? new List<string>())).ToLower();

            var requiredMissing = new List<string>();
            var niceToHaveMissing = new List<string>();

            foreach (var skill in requirements.TechnicalSkills)
            {
                var skillLower = skill.ToLower();

                // Skills or bullets
                var foundInSkills = allCVSkills.Any(s =>
                    s.Contains(skillLower) || skillLower.Contains(s));
                var foundInBullets = allBulletsText.Contains(skillLower);

                if (!foundInSkills && !foundInBullets)
                {
                    if (NiceToHaveKeywords.Contains(skill))
                        niceToHaveMissing.Add(skill);
                    else
                        requiredMissing.Add(skill);
                }
            }

            var suggestions = BuildSuggestions(requiredMissing, niceToHaveMissing);

            // Estimate how much the score would increase if the player had the missing skills
            var totalSkills = Math.Max(requirements.TechnicalSkills.Count, 1);
            var currentMatched = totalSkills - requiredMissing.Count - niceToHaveMissing.Count;
            var potentialMatched = currentMatched + requiredMissing.Count;
            var potentialIncrease = (int)Math.Round(
                (double)(potentialMatched - currentMatched) / totalSkills * 50); // 50% weight

            return new MissingSkillsAnalysis
            {
                RequiredMissing = requiredMissing.Take(7).ToList(),
                NiceToHaveMissing = niceToHaveMissing.Take(5).ToList(),
                Suggestions = suggestions,
                PotentialScoreIncrease = Math.Min(potentialIncrease, 40)
            };
        }

        private List<string> BuildSuggestions(List<string> required, List<string> niceToHave)
        {
            var suggestions = new List<string>();

            foreach (var skill in required.Take(5))
            {
                suggestions.Add($"Add '{skill}' to cv-master.json if you have experience with this technology");
            }

            foreach (var skill in niceToHave.Take(3))
            {
                suggestions.Add($"Mention '{skill}' in relevant project bullet points if you have used it");
            }

            if (required.Count > 5)
            {
                suggestions.Add($"There are {required.Count - 5} more required skills — review the full JSON");
            }

            return suggestions;
        }
    }
}

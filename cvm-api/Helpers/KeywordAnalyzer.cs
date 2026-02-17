using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class KeywordAnalyzer
    {
        private static readonly HashSet<string> SoftSkillWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "leadership", "communication", "teamwork", "collaboration", "mentoring",
            "agile", "scrum", "problem-solving", "adaptability", "initiative",
            "ownership", "proactive", "analytical", "organized", "detail"
        };

        private static readonly HashSet<string> TechWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "react", "angular", "vue", "node", "python", "java", "c#", "dotnet", "net",
            "sql", "nosql", "mongodb", "postgresql", "mysql", "redis", "docker",
            "kubernetes", "aws", "azure", "gcp", "git", "ci/cd", "devops", "api",
            "rest", "graphql", "microservices", "typescript", "javascript"
        };

        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "with", "you", "that", "this", "will", "from",
            "have", "has", "are", "our", "your", "their", "been", "more", "than",
            "such", "about", "into", "through", "should", "would", "could", "must",
            "what", "when", "where", "which", "who", "how", "they", "them", "then",
            "than", "also", "both", "each", "few", "more", "most", "other", "some"
        };

        public KeywordAnalysis Analyze(string jobDescription, CVMaster cv)
        {
            // Full CV text for search
            var cvText = BuildCVText(cv);

            // Extract all words from the JD frequently
            var wordFrequencies = ExtractWordFrequencies(jobDescription);

            var technical = new List<KeywordItem>();
            var softSkills = new List<KeywordItem>();
            var industry = new List<KeywordItem>();

            foreach (var kvp in wordFrequencies.OrderByDescending(x => x.Value))
            {
                var word = kvp.Key;
                var freq = kvp.Value;
                var present = cvText.Contains(word, StringComparison.OrdinalIgnoreCase);

                var item = new KeywordItem { Word = word, Frequency = freq, PresentInCV = present };

                if (TechWords.Contains(word))
                    technical.Add(item);
                else if (SoftSkillWords.Contains(word))
                    softSkills.Add(item);
                else if (word.Length > 4 && !StopWords.Contains(word))
                    industry.Add(item);
            }

            var presentList = wordFrequencies.Keys
                .Where(w => cvText.Contains(w, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var missingList = wordFrequencies.Keys
                .Where(w => !cvText.Contains(w, StringComparison.OrdinalIgnoreCase))
                .Where(w => TechWords.Contains(w) || SoftSkillWords.Contains(w))
                .Take(10)
                .ToList();

            var totalRelevant = Math.Max(technical.Count + softSkills.Count, 1);
            var presentRelevant = technical.Count(x => x.PresentInCV) + softSkills.Count(x => x.PresentInCV);
            var density = (double)presentRelevant / totalRelevant;

            return new KeywordAnalysis
            {
                TechnicalKeywords = technical.Take(10).ToList(),
                SoftSkillKeywords = softSkills.Take(8).ToList(),
                IndustryKeywords = industry.Take(8).ToList(),
                PresentInCV = presentList.Take(15).ToList(),
                MissingFromCV = missingList,
                KeywordDensity = Math.Round(density, 2),
                Suggestions = BuildSuggestions(missingList, technical, softSkills)
            };
        }

        private Dictionary<string, int> ExtractWordFrequencies(string text)
        {
            return Regex.Split(text.ToLower(), @"\W+")
                .Where(w => w.Length > 3 && !StopWords.Contains(w))
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private string BuildCVText(CVMaster cv)
        {
            var parts = new List<string>();
            parts.AddRange(MatchScoreCalculator.FlattenSkills(cv.Skills));
            foreach (var exp in cv.Experience)
            {
                parts.Add(exp.Role ?? "");
                parts.AddRange(exp.Bullets ?? new List<string>());
                parts.AddRange(exp.Skills ?? new List<string>());
            }
            return string.Join(" ", parts);
        }

        private List<string> BuildSuggestions(
            List<string> missing,
            List<KeywordItem> technical,
            List<KeywordItem> softSkills)
        {
            var suggestions = new List<string>();

            var missingTech = technical.Where(x => !x.PresentInCV).Take(3);
            foreach (var item in missingTech)
                suggestions.Add($"Add \"{item.Word}\" to the skills section or bullets if you have used it");

            var missingSoft = softSkills.Where(x => !x.PresentInCV).Take(2);
            foreach (var item in missingSoft)
                suggestions.Add($"Mention \"{item.Word}\" in a bullet point to reflect this skill");

            return suggestions;
        }
    }
}

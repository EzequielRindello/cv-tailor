using ErCvApi.Constants;
using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class KeywordAnalyzer
    {
        private static readonly HashSet<string> SoftSkillWords =
            new(KeywordAnalyzerConstants.SoftSkillWords, StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> TechWords =
            new(KeywordAnalyzerConstants.TechWords, StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> StopWords =
            new(KeywordAnalyzerConstants.StopWords, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Analyzes keyword presence against the GENERATED CV (not the master),
        /// so results reflect what will actually be in the output document.
        /// </summary>
        public KeywordAnalysis Analyze(string jobDescription, GeneratedCV generatedCV)
        {
            var cvText = BuildCVText(generatedCV);
            var wordFrequencies = ExtractWordFrequencies(jobDescription);

            var technical = new List<KeywordItem>();
            var softSkills = new List<KeywordItem>();
            var industry = new List<KeywordItem>();

            foreach (var kvp in wordFrequencies.OrderByDescending(x => x.Value))
            {
                var word = kvp.Key;
                var freq = kvp.Value;
                var present = cvText.Contains(word, StringComparison.OrdinalIgnoreCase);

                var item = new KeywordItem
                {
                    Word = word,
                    Frequency = freq,
                    PresentInCV = present
                };

                if (TechWords.Contains(word) ||
                    SkillNormalizer.MatchesAny(word, TechWords.ToList()))
                    technical.Add(item);
                else if (SoftSkillWords.Contains(word))
                    softSkills.Add(item);
                else if (word.Length > 4 && !StopWords.Contains(word))
                    industry.Add(item);
            }

            var presentList = wordFrequencies.Keys
                .Where(w => cvText.Contains(w, StringComparison.OrdinalIgnoreCase))
                .Take(KeywordAnalyzerConstants.MaxPresentKeywords)
                .ToList();

            var missingList = wordFrequencies.Keys
                .Where(w => !cvText.Contains(w, StringComparison.OrdinalIgnoreCase))
                .Where(w => TechWords.Contains(w) || SoftSkillWords.Contains(w))
                .Take(KeywordAnalyzerConstants.MaxMissingKeywords)
                .ToList();

            var totalRelevant = Math.Max(technical.Count + softSkills.Count, 1);
            var presentRelevant =
                technical.Count(x => x.PresentInCV) +
                softSkills.Count(x => x.PresentInCV);

            var density = (double)presentRelevant / totalRelevant;

            return new KeywordAnalysis
            {
                TechnicalKeywords = technical.Take(KeywordAnalyzerConstants.MaxTechnicalKeywords).ToList(),
                SoftSkillKeywords = softSkills.Take(KeywordAnalyzerConstants.MaxSoftSkillKeywords).ToList(),
                IndustryKeywords = industry.Take(KeywordAnalyzerConstants.MaxIndustryKeywords).ToList(),
                PresentInCV = presentList,
                MissingFromCV = missingList,
                KeywordDensity = Math.Round(density, 2),
                Suggestions = BuildSuggestions(missingList, technical, softSkills)
            };
        }

        private Dictionary<string, int> ExtractWordFrequencies(string text)
        {
            return Regex.Split(text.ToLower(), KeywordAnalyzerConstants.WordSplitPattern)
                .Where(w => w.Length > 3 && !StopWords.Contains(w))
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Builds CV text from the GENERATED CV — only experiences/skills that
        /// will actually appear in the output document.
        /// </summary>
        private string BuildCVText(GeneratedCV cv)
        {
            var parts = new List<string>();

            parts.AddRange(cv.Skills ?? new List<string>());

            if (!string.IsNullOrEmpty(cv.Summary))
                parts.Add(cv.Summary);

            foreach (var exp in cv.Experience ?? new List<Experience>())
            {
                parts.Add(exp.Role ?? "");
                parts.AddRange(exp.Bullets ?? new List<string>());
                parts.AddRange(exp.Skills ?? new List<string>());
            }

            foreach (var proj in cv.Projects ?? new List<Project>())
            {
                parts.Add(proj.Name ?? "");
                parts.Add(proj.Description ?? "");
                parts.AddRange(proj.Technologies ?? new List<string>());
            }

            return string.Join(" ", parts);
        }

        private List<string> BuildSuggestions(
            List<string> missing,
            List<KeywordItem> technical,
            List<KeywordItem> softSkills)
        {
            var suggestions = new List<string>();

            foreach (var item in technical.Where(x => !x.PresentInCV)
                .Take(KeywordAnalyzerConstants.MaxMissingTechSuggestions))
                suggestions.Add(string.Format(KeywordAnalyzerConstants.AddTechSuggestionTemplate, item.Word));

            foreach (var item in softSkills.Where(x => !x.PresentInCV)
                .Take(KeywordAnalyzerConstants.MaxMissingSoftSuggestions))
                suggestions.Add(string.Format(KeywordAnalyzerConstants.AddSoftSuggestionTemplate, item.Word));

            return suggestions;
        }
    }
}

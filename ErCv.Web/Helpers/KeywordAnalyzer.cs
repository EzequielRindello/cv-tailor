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

        public KeywordAnalysis Analyze(string jobDescription, GeneratedCV generatedCV)
        {
            var cvText = BuildCVText(generatedCV);
            var cvSkills = ExtractAllCVSkills(generatedCV);
            var wordFrequencies = ExtractWordFrequencies(jobDescription);

            var technical = new List<KeywordItem>();
            var softSkills = new List<KeywordItem>();
            var industry = new List<KeywordItem>();

            foreach (var kvp in wordFrequencies.OrderByDescending(x => x.Value))
            {
                var word = kvp.Key;
                var freq = kvp.Value;
                var presentInCV =
                    SkillNormalizer.MatchesAny(word, cvSkills) ||
                    ContainsTerm(cvText, word);

                var item = new KeywordItem { Word = word, Frequency = freq, PresentInCV = presentInCV };

                if (TechWords.Contains(word) || SkillNormalizer.MatchesAny(word, TechWords.ToList()))
                    technical.Add(item);
                else if (SoftSkillWords.Contains(word))
                    softSkills.Add(item);
                else if (word.Length > 4 && !StopWords.Contains(word))
                    industry.Add(item);
            }

            var presentList = wordFrequencies.Keys
                .Where(w => SkillNormalizer.MatchesAny(w, cvSkills) ||
                            ContainsTerm(cvText, w))
                .Take(KeywordAnalyzerConstants.MaxPresentKeywords).ToList();

            var missingList = wordFrequencies.Keys
                .Where(w => !SkillNormalizer.MatchesAny(w, cvSkills) &&
                            !ContainsTerm(cvText, w) &&
                            (TechWords.Contains(w) || SoftSkillWords.Contains(w)))
                .Take(KeywordAnalyzerConstants.MaxMissingKeywords).ToList();

            var totalRelevant = Math.Max(technical.Count + softSkills.Count, 1);
            var presentRelevant = technical.Count(x => x.PresentInCV) + softSkills.Count(x => x.PresentInCV);

            return new KeywordAnalysis
            {
                TechnicalKeywords = technical.Take(KeywordAnalyzerConstants.MaxTechnicalKeywords).ToList(),
                SoftSkillKeywords = softSkills.Take(KeywordAnalyzerConstants.MaxSoftSkillKeywords).ToList(),
                IndustryKeywords = industry.Take(KeywordAnalyzerConstants.MaxIndustryKeywords).ToList(),
                PresentInCV = presentList,
                MissingFromCV = missingList,
                KeywordDensity = Math.Round((double)presentRelevant / totalRelevant, 2),
                Suggestions = BuildSuggestions(technical, softSkills)
            };
        }

        private Dictionary<string, int> ExtractWordFrequencies(string text) =>
            Regex.Split(text.ToLower(), KeywordAnalyzerConstants.WordSplitPattern)
                .Where(w => w.Length > 3 && !StopWords.Contains(w))
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

        private string BuildCVText(GeneratedCV cv)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(cv.Summary)) parts.Add(cv.Summary);
            parts.AddRange(cv.Skills ?? new());
            foreach (var exp in cv.Experience ?? new())
            {
                parts.Add(exp.Role ?? "");
                parts.AddRange(exp.Bullets ?? new());
                parts.AddRange(exp.Skills ?? new());
            }
            foreach (var proj in cv.Projects ?? new())
            {
                parts.Add(proj.Name ?? "");
                parts.Add(proj.Description ?? "");
                parts.AddRange(proj.Technologies ?? new());
            }
            return string.Join(" ", parts);
        }

        private List<string> ExtractAllCVSkills(GeneratedCV cv)
        {
            var skills = new List<string>();
            skills.AddRange(cv.Skills ?? new());
            foreach (var exp in cv.Experience ?? new()) skills.AddRange(exp.Skills ?? new());
            foreach (var proj in cv.Projects ?? new()) skills.AddRange(proj.Technologies ?? new());
            return skills.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private List<string> BuildSuggestions(List<KeywordItem> technical, List<KeywordItem> softSkills)
        {
            var suggestions = new List<string>();
            foreach (var item in technical.Where(x => !x.PresentInCV).Take(KeywordAnalyzerConstants.MaxMissingTechSuggestions))
                suggestions.Add(string.Format(KeywordAnalyzerConstants.AddTechSuggestionTemplate, item.Word));
            foreach (var item in softSkills.Where(x => !x.PresentInCV).Take(KeywordAnalyzerConstants.MaxMissingSoftSuggestions))
                suggestions.Add(string.Format(KeywordAnalyzerConstants.AddSoftSuggestionTemplate, item.Word));
            return suggestions;
        }

        private static bool ContainsTerm(string text, string term)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term)) return false;
            var pattern = $@"(?<![A-Za-z0-9]){Regex.Escape(term.Trim())}(?![A-Za-z0-9])";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }
    }
}

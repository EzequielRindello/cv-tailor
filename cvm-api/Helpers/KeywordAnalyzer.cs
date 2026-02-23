using ErCvApi.Constants;
using ErCvApi.Models;
using System;
using System.Collections.Generic;
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

        public KeywordAnalysis Analyze(string jobDescription, CVMaster cv)
        {
            var cvText = BuildCVText(cv);
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

                if (TechWords.Contains(word))
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
                TechnicalKeywords = technical
                    .Take(KeywordAnalyzerConstants.MaxTechnicalKeywords)
                    .ToList(),

                SoftSkillKeywords = softSkills
                    .Take(KeywordAnalyzerConstants.MaxSoftSkillKeywords)
                    .ToList(),

                IndustryKeywords = industry
                    .Take(KeywordAnalyzerConstants.MaxIndustryKeywords)
                    .ToList(),

                PresentInCV = presentList,
                MissingFromCV = missingList,
                KeywordDensity = Math.Round(density, 2),
                Suggestions = BuildSuggestions(missingList, technical, softSkills)
            };
        }

        private Dictionary<string, int> ExtractWordFrequencies(string text)
        {
            return Regex.Split(
                    text.ToLower(),
                    KeywordAnalyzerConstants.WordSplitPattern)
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

            var missingTech = technical
                .Where(x => !x.PresentInCV)
                .Take(KeywordAnalyzerConstants.MaxMissingTechSuggestions);

            foreach (var item in missingTech)
            {
                suggestions.Add(string.Format(
                    KeywordAnalyzerConstants.AddTechSuggestionTemplate,
                    item.Word));
            }

            var missingSoft = softSkills
                .Where(x => !x.PresentInCV)
                .Take(KeywordAnalyzerConstants.MaxMissingSoftSuggestions);

            foreach (var item in missingSoft)
            {
                suggestions.Add(string.Format(
                    KeywordAnalyzerConstants.AddSoftSuggestionTemplate,
                    item.Word));
            }

            return suggestions;
        }
    }
}
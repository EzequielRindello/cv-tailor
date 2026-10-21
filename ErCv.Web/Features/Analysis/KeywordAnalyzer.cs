using ErCv.Web.Features.Matching;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Analysis;

public class KeywordAnalyzer
{
    #region Constants

    private static readonly HashSet<string> SoftSkillWords =
        new(KeywordAnalyzerConstants.SoftSkillWords, StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> StopWords =
        new(KeywordAnalyzerConstants.StopWords, StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Public Methods

    public KeywordAnalysis Analyze(string jobDescription, GeneratedCV generatedCV)
    {
        var cvText = BuildCVText(generatedCV);
        var cvSkills = ExtractAllCVSkills(generatedCV);

        // Technical keywords are the dictionary skills the job description names, the same ones the parser finds,
        // so a word like "next" in "the next generation" is never a technical keyword (F22)
        var technical = new List<KeywordItem>();
        var skillWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (skill, hits) in FindSkills(jobDescription))
        {
            technical.Add(new KeywordItem
            {
                Word = skill.Name,
                Frequency = hits.Count,
                PresentInCV = SkillNormalizer.MatchesAny(skill.Name, cvSkills) || SkillNormalizer.ContainsSkill(cvText, skill.Name)
            });
            foreach (var (start, length) in hits)
                skillWords.UnionWith(JobDescriptionConstants.WordSplit().Split(jobDescription.Substring(start, length).ToLower()));
        }

        // The words that name a skill are already counted as technical
        var wordFrequencies = ExtractWordFrequencies(jobDescription)
            .Where(kvp => !skillWords.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Fuzzy matching against the CV is the expensive part, so each word is checked once
        var presentInCv = wordFrequencies.Keys.ToDictionary(
            w => w,
            w => SkillNormalizer.MatchesAny(w, cvSkills) || SkillNormalizer.ContainsTerm(cvText, w));

        var softSkills = new List<KeywordItem>();
        var industry = new List<KeywordItem>();

        foreach (var kvp in wordFrequencies.OrderByDescending(x => x.Value))
        {
            var word = kvp.Key;
            var item = new KeywordItem { Word = word, Frequency = kvp.Value, PresentInCV = presentInCv[word] };

            if (SoftSkillWords.Contains(word))
                softSkills.Add(item);
            else if (word.Length >= KeywordAnalyzerConstants.MinIndustryWordLength && !StopWords.Contains(word))
                industry.Add(item);
        }

        var presentList = technical.Where(k => k.PresentInCV).Select(k => k.Word)
            .Concat(wordFrequencies.Keys.Where(w => presentInCv[w]))
            .Take(KeywordAnalyzerConstants.MaxPresentKeywords).ToList();

        var missingList = technical.Where(k => !k.PresentInCV).Select(k => k.Word)
            .Concat(softSkills.Where(k => !k.PresentInCV).Select(k => k.Word))
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
            KeywordDensity = Math.Round((double)presentRelevant / totalRelevant, KeywordAnalyzerConstants.DensityDecimals),
            Suggestions = BuildSuggestions(technical, softSkills)
        };
    }

    #endregion

    #region Private Methods

    // Most named first; on a tie, the one the job description names first
    private static List<(Skill Skill, List<(int Start, int Length)> Hits)> FindSkills(string jobDescription) =>
        SkillDictionary.All
            .Select(skill => (Skill: skill, Hits: SkillNormalizer.FindSkill(jobDescription, skill).ToList()))
            .Where(x => x.Hits.Count > 0)
            .OrderByDescending(x => x.Hits.Count)
            .ThenBy(x => x.Hits.Min(h => h.Start))
            .ToList();

    private Dictionary<string, int> ExtractWordFrequencies(string text) =>
        JobDescriptionConstants.WordSplit().Split(text.ToLower())
            .Where(w => w.Length >= KeywordAnalyzerConstants.MinWordLength && !StopWords.Contains(w))
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

    #endregion
}

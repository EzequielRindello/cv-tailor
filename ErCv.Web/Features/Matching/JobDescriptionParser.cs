using System.Text.RegularExpressions;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Matching;

public class JobDescriptionParser
{
    #region Constants

    private static readonly HashSet<string> CommonTechKeywords =
        new(JobDescriptionConstants.CommonTechKeywords, StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> CommonWords =
        new(JobDescriptionConstants.CommonWords, StringComparer.OrdinalIgnoreCase);

    // Built once: one pattern per seniority word, in list order, because the first unnegated match wins
    private static readonly (string Keyword, Regex Pattern)[] SeniorityPatterns =
        new HashSet<string>(JobDescriptionConstants.SeniorityKeywords, StringComparer.OrdinalIgnoreCase)
            .Select(k => (k, new Regex($@"\b{Regex.Escape(k)}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)))
            .ToArray();

    private static readonly HashSet<string> NeverSkills = new(StringComparer.OrdinalIgnoreCase)
    {
        "experience","familiarity","understanding","knowledge","proficiency",
        "ability","background","exposure","awareness","skill","skills",
        "strong","solid","deep","good","basic","advanced","proven",
        "excellent","nice","required","preferred","plus","bonus",
        "including","following","similar","relevant","related",
        "technology","technologies","tools","tool","stack","platform",
        "systems","system","solutions","services","applications",
        "backend","frontend","fullstack","full","stack",
        "engineer","engineering","developer","development","software",
        "technical","senior","junior","lead","principal","manager",
        "manage","monitor","cost","write","build","design","implement",
        "collaborate","work","team","teams","environment","practices",
        "responsibilities","requirements","qualifications","about",
        "company","role","position","candidate","ideal","looking",
        "seeking","hire","join","opportunity","what","will",
        "have","with","that","this","from","your",
        "library","libraries","framework","frameworks","methodology",
        // words that follow "N years of/in" without naming a technology
        "in","or","of","and","as","on","at","to","for","a","an","the","b2b","b2c",
    };

    #endregion

    #region Public Methods

    public JobRequirements Parse(string jobDescription)
    {
        if (string.IsNullOrWhiteSpace(jobDescription))
            return new JobRequirements
            {
                TechnicalSkills = new List<string>(),
                Keywords = new List<string>(),
                SeniorityLevel = JobDescriptionConstants.DefaultSeniority,
                RequiredExperience = new List<string>()
            };

        var normalizedText = jobDescription.ToLower();

        return new JobRequirements
        {
            TechnicalSkills = ExtractTechnicalSkills(jobDescription, normalizedText),
            Keywords = ExtractKeywords(normalizedText),
            SeniorityLevel = ExtractSeniorityLevel(normalizedText),
            RequiredExperience = ExtractRequiredExperience(normalizedText)
        };
    }

    #endregion

    #region Private Methods

    private List<string> ExtractTechnicalSkills(string originalText, string normalizedText)
    {
        var foundSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tech in CommonTechKeywords)
        {
            if (SkillNormalizer.ContainsTerm(normalizedText, tech))
                foundSkills.Add(tech);
        }
        var yearsMatches = JobDescriptionConstants.YearsWithTech().Matches(originalText);
        foreach (Match match in yearsMatches)
        {
            if (match.Groups.Count > 2)
            {
                var tech = match.Groups[2].Value.Trim();
                if (!string.IsNullOrWhiteSpace(tech)
                    && tech.Length >= JobDescriptionConstants.MinCapturedSkillLength
                    && tech.Length <= JobDescriptionConstants.MaxCapturedSkillLength
                    && !NeverSkills.Contains(tech) && !CommonWords.Contains(tech))
                    foundSkills.Add(tech);
            }
        }
        return foundSkills
            .Select(s => SkillNormalizer.Normalize(s))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private List<string> ExtractKeywords(string text) =>
        JobDescriptionConstants.WordSplit().Split(text)
            .Where(w => w.Length >= JobDescriptionConstants.MinKeywordLength && !CommonWords.Contains(w) && !NeverSkills.Contains(w))
            .GroupBy(w => w.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(JobDescriptionConstants.MaxKeywords)
            .Select(g => g.Key)
            .ToList();

    private string ExtractSeniorityLevel(string text)
    {
        foreach (var (keyword, pattern) in SeniorityPatterns)
        {
            foreach (Match match in pattern.Matches(text))
            {
                var start = Math.Max(0, match.Index - JobDescriptionConstants.NegationWindowChars);
                var context = text.Substring(start, match.Index - start).ToLower();
                if (!JobDescriptionConstants.SeniorityNegations.Any(n => context.Contains(n)))
                    return keyword.ToLower();
            }
        }

        var yearsMatch = JobDescriptionConstants.YearsOfExperience().Match(text);
        if (yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out int years))
        {
            if (years >= JobDescriptionConstants.SeniorYearsThreshold) return JobDescriptionConstants.SeniorityLevelSenior;
            if (years >= JobDescriptionConstants.MidLevelYearsThreshold) return JobDescriptionConstants.DefaultSeniority;
            if (years >= JobDescriptionConstants.JuniorYearsThreshold) return JobDescriptionConstants.SeniorityLevelJunior;
        }

        return JobDescriptionConstants.DefaultSeniority;
    }

    private List<string> ExtractRequiredExperience(string text)
    {
        var requirements = new List<string>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (JobDescriptionConstants.BulletLine().IsMatch(trimmed) ||
                trimmed.StartsWith(JobDescriptionConstants.ExperienceKeyword, StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains(JobDescriptionConstants.RequiredKeyword, StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains(JobDescriptionConstants.MustHaveKeyword, StringComparison.OrdinalIgnoreCase))
            {
                var cleaned = JobDescriptionConstants.BulletLine().Replace(trimmed, "").Trim();
                if (cleaned.Length >= JobDescriptionConstants.MinRequirementLength
                    && cleaned.Length <= JobDescriptionConstants.MaxRequirementLength)
                    requirements.Add(cleaned);
            }
        }
        return requirements.Take(JobDescriptionConstants.MaxRequirements).ToList();
    }

    #endregion
}

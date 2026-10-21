using System.Text.RegularExpressions;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Matching;

public class JobDescriptionParser
{
    #region Constants

    private static readonly HashSet<string> CommonWords =
        new(JobDescriptionConstants.CommonWords, StringComparer.OrdinalIgnoreCase);

    // Built once, in the order of SeniorityLevels: a level word as a whole word of the title
    private static readonly (string Level, Regex Pattern)[] TitleLevelPatterns =
        JobDescriptionConstants.SeniorityLevels
            .Select(l => (l.Level, new Regex($@"(?<![a-z0-9]){Alternation(l.Words)}(?![a-z0-9])", RegexOptions.Compiled)))
            .ToArray();

    private static readonly (string Level, Regex Pattern)[] NumberedLevelPatterns =
        JobDescriptionConstants.NumberedLevels
            .Select(l => (l.Level, new Regex($@"(?<![a-z0-9]){Alternation(JobDescriptionConstants.RoleNouns)}\s+{Alternation(l.Numerals)}(?![a-z0-9])", RegexOptions.Compiled)))
            .ToArray();

    // A level word that describes the role in the body: followed, within a few words, by a singular role noun
    private static readonly (string Level, Regex Pattern)[] BodyLevelPatterns =
        JobDescriptionConstants.SeniorityLevels
            .Select(l => (l.Level, new Regex(
                $@"(?<![a-z0-9-]){Alternation(l.Words)}\.?\s+(?:[a-z0-9+#./-]+\s+){{0,{JobDescriptionConstants.MaxWordsBeforeRoleNoun}}}?{Alternation(JobDescriptionConstants.RoleNouns)}(?![a-z0-9])",
                RegexOptions.Compiled)))
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
            TechnicalSkills = ExtractTechnicalSkills(jobDescription),
            Keywords = ExtractKeywords(normalizedText),
            SeniorityLevel = ExtractSeniorityLevel(normalizedText),
            RequiredExperience = ExtractRequiredExperience(normalizedText)
        };
    }

    #endregion

    #region Private Methods

    // Searched in the original text: some text forms only count capitalized ("Go", not "go ahead")
    private List<string> ExtractTechnicalSkills(string text)
    {
        var found = SkillDictionary.All
            .Where(skill => SkillNormalizer.FindSkill(text, skill).Any())
            .Select(skill => skill.Name)
            .ToList();

        // "5 years of go" names a skill even in lowercase, but only a word the dictionary knows ("5 years of building" does not)
        foreach (Match match in JobDescriptionConstants.YearsWithTech().Matches(text))
        {
            var skill = SkillDictionary.FindByTextForm(match.Groups[2].Value.TrimEnd('.'));
            if (skill != null && !found.Contains(skill.Name)) found.Add(skill.Name);
        }
        return found;
    }

    private List<string> ExtractKeywords(string text) =>
        JobDescriptionConstants.WordSplit().Split(text)
            .Where(w => w.Length >= JobDescriptionConstants.MinKeywordLength && !CommonWords.Contains(w) && !NeverSkills.Contains(w))
            .GroupBy(w => w.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(JobDescriptionConstants.MaxKeywords)
            .Select(g => g.Key)
            .ToList();

    /// <summary>
    /// The title (the first line) decides when it names a level. Otherwise the first level the body gives the role
    /// ("a senior backend engineer", not "lead the design" or "senior engineers"), then the years asked for.
    /// </summary>
    private string ExtractSeniorityLevel(string text)
    {
        var title = text.TrimStart().Split('\n', 2)[0].Trim();
        if (title.Length <= JobDescriptionConstants.MaxTitleLength && TitleLevel(title) is { } titleLevel)
            return titleLevel;

        var bodyLevel = BodyLevelPatterns
            .SelectMany(p => p.Pattern.Matches(text).Where(m => !IsNegated(text, m.Index)).Select(m => (p.Level, m.Index)))
            .OrderBy(x => x.Index)
            .Select(x => x.Level)
            .FirstOrDefault();
        if (bodyLevel != null) return bodyLevel;

        var yearsMatch = JobDescriptionConstants.YearsOfExperience().Match(text);
        if (yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out int years))
        {
            if (years >= JobDescriptionConstants.SeniorYearsThreshold) return JobDescriptionConstants.LevelSenior;
            if (years >= JobDescriptionConstants.MidLevelYearsThreshold) return JobDescriptionConstants.DefaultSeniority;
            if (years >= JobDescriptionConstants.JuniorYearsThreshold) return JobDescriptionConstants.LevelJunior;
        }

        return JobDescriptionConstants.DefaultSeniority;
    }

    private static string? TitleLevel(string title)
    {
        var t = " " + JobDescriptionConstants.TitlePunctuation().Replace(title, " ") + " ";
        foreach (var phrase in JobDescriptionConstants.NotLevelPhrases) t = t.Replace(phrase, " ");

        foreach (var (level, pattern) in TitleLevelPatterns)
            if (pattern.Matches(t).Any(m => !IsNegated(t, m.Index))) return level;
        foreach (var (level, pattern) in NumberedLevelPatterns)
            if (pattern.IsMatch(t)) return level;
        return null;
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

    private static bool IsNegated(string text, int index)
    {
        var start = Math.Max(0, index - JobDescriptionConstants.NegationWindowChars);
        var context = text.Substring(start, index - start);
        return JobDescriptionConstants.SeniorityNegations.Any(n => context.Contains(n));
    }

    private static string Alternation(IEnumerable<string> words) => $"(?:{string.Join("|", words.Select(Regex.Escape))})";

    #endregion
}

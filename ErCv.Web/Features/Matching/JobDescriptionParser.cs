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
        var skills = FindTechnicalSkills(jobDescription);

        return new JobRequirements
        {
            TechnicalSkills = skills.Select(s => s.Name).ToList(),
            NiceToHaveSkills = NiceToHaveSkills(jobDescription, skills),
            Keywords = ExtractKeywords(normalizedText),
            SeniorityLevel = ExtractSeniorityLevel(normalizedText),
            RequiredExperience = ExtractRequiredExperience(normalizedText)
        };
    }

    #endregion

    #region Private Methods

    // Searched in the original text: some text forms only count capitalized ("Go", not "go ahead")
    private static List<SkillMentions> FindTechnicalSkills(string text)
    {
        var found = SkillDictionary.All
            .Select(skill => new SkillMentions(skill.Name, SkillNormalizer.FindSkill(text, skill).Select(h => h.Start).ToList()))
            .Where(s => s.At.Count > 0)
            .ToList();

        // "5 years of go" names a skill even in lowercase, but only a word the dictionary knows ("5 years of building" does not)
        foreach (Match match in JobDescriptionConstants.YearsWithTech().Matches(text))
        {
            var skill = SkillDictionary.FindByTextForm(match.Groups[2].Value.TrimEnd('.'));
            if (skill == null) continue;
            var at = match.Groups[2].Index;
            if (found.Find(s => s.Name == skill.Name) is { } known) known.At.Add(at);
            else found.Add(new SkillMentions(skill.Name, new List<int> { at }));
        }
        return found;
    }

    /// <summary>The skills whose every mention is in an optional part of the post (see <see cref="OptionalSpans"/>).</summary>
    private static List<string> NiceToHaveSkills(string text, List<SkillMentions> skills)
    {
        var optional = OptionalSpans(text, skills.SelectMany(s => s.At).ToList());
        return skills
            .Where(s => s.At.All(at => optional.Any(span => at >= span.Start && at < span.End)))
            .Select(s => s.Name)
            .ToList();
    }

    /// <summary>
    /// The parts of the post that ask for something optional: the lines under a heading with an optional cue, up to
    /// the next heading, and elsewhere the lines, sentences or asides that carry one (see <see cref="OptionalParts"/>).
    /// A line that names a skill is a heading only when it opens a section about the company ("About MongoDB"),
    /// so "Kafka" alone on a line does not close a section.
    /// </summary>
    private static List<(int Start, int End)> OptionalSpans(string text, List<int> skillsAt)
    {
        var spans = new List<(int Start, int End)>();
        var underOptionalHeading = false;
        var lineStart = 0;
        foreach (var line in text.Split('\n'))
        {
            var lineEnd = lineStart + line.Length;
            var namesSkill = skillsAt.Any(at => at >= lineStart && at < lineEnd);

            var trimmed = line.Trim();
            if (IsHeading(trimmed) && (!namesSkill || StartsWithAny(trimmed, JobDescriptionConstants.CompanyHeadingPrefixes)))
                underOptionalHeading = HasOptionalCue(line);
            else if (underOptionalHeading)
                spans.Add((lineStart, lineEnd));
            else
                spans.AddRange(OptionalParts(line, lineStart));

            lineStart = lineEnd + 1;
        }
        return spans;
    }

    private static bool IsHeading(string line) =>
        line.Length > 0
        && !JobDescriptionConstants.BulletLine().IsMatch(line)
        && (line[^1] == JobDescriptionConstants.HeadingEnd
            || (!JobDescriptionConstants.SentenceEndChars.Contains(line[^1]) && IsShort(line)));

    /// <summary>
    /// The optional parts of a line outside a nice-to-have section. A lead-in with a cue ("Bonus: Go, Java or Rust;
    /// object storage at scale") covers the whole line. Otherwise each sentence: a cue covers the sentence ("Typescript
    /// or Rust is a plus"), a forward cue only what follows it ("AWS, GCP or Azure, ideally including access logs"), and
    /// a cue inside parentheses only the parentheses ("read code (we use a lot of TypeScript), cloud (GCP a plus)").
    /// </summary>
    private static IEnumerable<(int Start, int End)> OptionalParts(string line, int lineStart)
    {
        var leadIn = JobDescriptionConstants.BulletLine().Replace(line.Trim(), "").Split(JobDescriptionConstants.HeadingEnd, 2);
        if (leadIn.Length == 2 && IsShort(leadIn[0]) && HasOptionalCue(leadIn[0]))
        {
            yield return (lineStart, lineStart + line.Length);
            yield break;
        }

        var start = 0;
        foreach (var end in JobDescriptionConstants.SentenceBreak().Matches(line).Select(m => m.Index).Append(line.Length))
        {
            var sentence = line[start..end];
            var asides = JobDescriptionConstants.Parenthetical().Matches(sentence);
            // Blanked to the same length, so an offset in it is an offset in the sentence
            var outside = JobDescriptionConstants.Parenthetical().Replace(sentence, a => new string(' ', a.Length));

            if (ContainsAny(outside, JobDescriptionConstants.OptionalCues))
                yield return (lineStart + start, lineStart + end);
            else if (FirstForwardCue(outside) is var cue and >= 0)
                yield return (lineStart + start + cue, lineStart + end);

            foreach (Match aside in asides.Where(a => HasOptionalCue(a.Value)))
                yield return (lineStart + start + aside.Index, lineStart + start + aside.Index + aside.Length);
            start = end;
        }
    }

    private static int FirstForwardCue(string text) =>
        JobDescriptionConstants.ForwardOptionalCues
            .Select(cue => text.IndexOf(cue, StringComparison.OrdinalIgnoreCase))
            .Where(at => at >= 0)
            .DefaultIfEmpty(-1)
            .Min();

    private static bool HasOptionalCue(string text) =>
        ContainsAny(text, JobDescriptionConstants.OptionalCues) || ContainsAny(text, JobDescriptionConstants.ForwardOptionalCues);

    private static bool ContainsAny(string text, string[] cues) =>
        cues.Any(cue => text.Contains(cue, StringComparison.OrdinalIgnoreCase));

    private static bool StartsWithAny(string text, string[] prefixes) =>
        prefixes.Any(prefix => text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static bool IsShort(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= JobDescriptionConstants.MaxHeadingWords;

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

    #region Nested Types

    /// <summary>A skill the post names and where (character offsets in the job description).</summary>
    private sealed record SkillMentions(string Name, List<int> At);

    #endregion
}

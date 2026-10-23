using System.Globalization;
using System.Text.RegularExpressions;
using ErCv.Web.Features.Matching;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Profiles;

/// <summary>
/// Reads a draft profile out of a resume's text (PROF-05): contact details, then experience, education, skills and
/// projects, found by their section titles and dates. It is a heuristic: the visitor reviews the draft before saving.
/// Nothing is invented: every value is text of the resume, only dates are rewritten in the profile's format.
/// </summary>
public static partial class ResumeParser
{
    #region Constants

    // A month is read by its first three letters ("Sept." and "septiembre" are September)
    private const int MonthPrefixLength = 3;

    private const string MonthPattern =
        @"(?<![a-z])(?:jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec|ene|abr|ago|set|dic)[a-zé]*\.?";
    private const string YearPattern = @"(?:19|20)\d{2}";
    private const string DatePattern =
        $@"(?:{MonthPattern}\s*(?:de\s+)?{YearPattern}|\d{{1,2}}/{YearPattern}|{YearPattern}-\d{{1,2}}(?!\d)|{YearPattern})";
    private const string PresentPattern =
        @"(?:la actualidad|actualidad|presente|present|current|now|today|hoy|actual)(?![a-z])";

    [GeneratedRegex($@"(?<start>{DatePattern})\s*(?:[–\u2014-]|\s(?:to|a|al|hasta)\s)\s*(?<end>{DatePattern}|{PresentPattern})", RegexOptions.IgnoreCase)]
    private static partial Regex DateRange();

    [GeneratedRegex($@"(?<![\d/])(?:{DatePattern}|{PresentPattern})(?![\d/])", RegexOptions.IgnoreCase)]
    private static partial Regex SingleDate();

    [GeneratedRegex(PresentPattern, RegexOptions.IgnoreCase)]
    private static partial Regex Present();

    [GeneratedRegex($@"^(?<m>{MonthPattern})\s*(?:de\s+)?(?<y>{YearPattern})$|^(?<mm>\d{{1,2}})/(?<y>{YearPattern})$|^(?<y>{YearPattern})-(?<mm>\d{{1,2}})$|^(?<y>{YearPattern})$", RegexOptions.IgnoreCase)]
    private static partial Regex DateParts();

    [GeneratedRegex(@"[\w.+-]+@[\w-]+(?:\.[\w-]+)+")]
    private static partial Regex Email();

    [GeneratedRegex(@"\+?\d[\d\s().-]{5,}\d")]
    private static partial Regex Phone();

    [GeneratedRegex(@"(?:https?://)?(?:[a-z]{2,3}\.)?linkedin\.com/in/[\w%-]+", RegexOptions.IgnoreCase)]
    private static partial Regex LinkedIn();

    [GeneratedRegex(@"(?:https?://)?(?:www\.)?github\.com/[\w-]+", RegexOptions.IgnoreCase)]
    private static partial Regex GitHub();

    // Case-sensitive on purpose: a bare domain is written in lowercase, so "ASP.NET" is not a link
    [GeneratedRegex(@"https?://\S+|(?:www\.)?(?:github|gitlab|bitbucket)\.(?:com|org)/\S+|\b[a-z0-9-]+(?:\.[a-z0-9-]+)*\.(?:com|dev|io|app|net|org|me|ar|co)(?:/\S*)?")]
    private static partial Regex Link();

    [GeneratedRegex(@"^(?:https?://)?(?:www\.)?")]
    private static partial Regex Scheme();

    // "React 18/19", ".NET 9", "Bootstrap 5": the version is not part of the skill's name
    [GeneratedRegex(@"\s+v?[\d./+x]+$")]
    private static partial Regex VersionSuffix();

    // What is left of "(2019 - 2021)" once the dates are taken out
    [GeneratedRegex(@"\(\s*(?:\|\s*)?\)")]
    private static partial Regex EmptyParens();

    [GeneratedRegex(@" {3,}")]
    private static partial Regex WideSpaces();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    private static readonly Dictionary<string, ResumeSection> SectionByTitle = ResumeImportConstants.SectionTitles
        .SelectMany(s => s.Titles.Select(t => (Title: t, s.Section)))
        .ToDictionary(x => x.Title, x => x.Section, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, SkillGroup> GroupByLabel = ResumeImportConstants.SkillLabels
        .SelectMany(s => s.Labels.Select(l => (Label: l, s.Group)))
        .ToDictionary(x => x.Label, x => x.Group, StringComparer.OrdinalIgnoreCase);

    private static readonly (SkillGroup Group, HashSet<string> Names)[] KnownSkills =
    {
        (SkillGroup.Languages, Set(ResumeImportConstants.LanguageSkills)),
        (SkillGroup.Frameworks, Set(ResumeImportConstants.FrameworkSkills)),
        (SkillGroup.Databases, Set(ResumeImportConstants.DatabaseSkills)),
        (SkillGroup.Cloud, Set(ResumeImportConstants.CloudSkills)),
        (SkillGroup.Practices, Set(ResumeImportConstants.PracticeSkills))
    };

    private static readonly HashSet<string> Connectors = Set(ResumeImportConstants.ConnectorWords);

    private static readonly HashSet<string> SkippedSkillLabels = Set(ResumeImportConstants.SkippedSkillLabels);

    private static readonly char[] TrimChars = ResumeImportConstants.TrimSeparators.ToCharArray();

    private static readonly char[] TrimColumns = { ' ', '|' };

    #endregion

    #region Public Methods

    /// <summary>
    /// The draft profile in <paramref name="text"/>. <paramref name="linesWrap"/> is true for PDF text, where a long
    /// line goes on in the next one; a Word paragraph is already one line.
    /// </summary>
    public static CVMaster Parse(string text, bool linesWrap)
    {
        var (header, sections) = SplitSections(ReadLines(text, linesWrap));
        var profile = new CVMaster { Header = ReadHeader(header) };

        foreach (var (section, lines) in sections)
        {
            switch (section)
            {
                case ResumeSection.Experience: profile.Experience.AddRange(ReadExperience(lines)); break;
                case ResumeSection.Education: profile.Education.AddRange(ReadEducation(lines)); break;
                case ResumeSection.Skills: ReadSkills(lines, profile.Skills); break;
                case ResumeSection.Projects: profile.Projects.AddRange(ReadProjects(lines)); break;
            }
        }
        return profile;
    }

    /// <summary>A resume date in the profile's format: "Jan 2020", "2020" or "Present". Anything else is kept as written.</summary>
    public static string ProfileDate(string date)
    {
        var text = date.Trim();
        if (Present().IsMatch(text)) return ResumeImportConstants.PresentWord;

        var parts = DateParts().Match(text);
        if (!parts.Success) return text;

        var year = parts.Groups["y"].Value;
        int? month = parts.Groups["m"].Success ? MonthOf(parts.Groups["m"].Value)
            : parts.Groups["mm"].Success ? int.Parse(parts.Groups["mm"].Value, CultureInfo.InvariantCulture)
            : null;
        return month >= 1 && month <= ResumeImportConstants.ProfileMonthNames.Length
            ? $"{ResumeImportConstants.ProfileMonthNames[month.Value - 1]} {year}"
            : year;
    }

    #endregion

    #region Private Methods

    // Lines: trimmed, list items marked, letter-spaced titles joined, and in a PDF the wrapped lines put back together
    private static List<Line> ReadLines(string text, bool linesWrap)
    {
        var lines = new List<Line>();
        var nextIsItem = false;
        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            // The mark is a whitespace character to Trim, so it is looked for before anything is trimmed
            var wraps = raw.Contains(ResumeImportConstants.WrapMark);
            var line = Clean(raw.Replace(ResumeImportConstants.WrapMark, ' '));
            if (line.Length == 0) continue;

            var isItem = nextIsItem;
            nextIsItem = false;
            if (ResumeImportConstants.BulletChars.Contains(line[0]) && (line.Length == 1 || char.IsWhiteSpace(line[1])))
            {
                line = line[1..].Trim();
                isItem = true;
                // A marker on a line of its own belongs to the next line
                if (line.Length == 0) { nextIsItem = true; continue; }
            }

            if (linesWrap && !isItem && lines.Count > 0 && (Continues(lines[^1], line) || GoesOn(lines[^1].Text, line)))
                lines[^1] = lines[^1] with { Text = JoinWrapped(lines[^1].Text, line), Wraps = wraps };
            else
                lines.Add(new Line(line, isItem, wraps));
        }
        return lines;
    }

    private static string Clean(string raw)
    {
        var text = raw.Replace(' ', ' ').Replace("​", "").Trim();

        // "E X P E R I E N C E" (or "W O R K  E X P E R I E N C E", words two spaces apart)
        if (!text.Contains('\t') && IsLetterSpaced(text.Split(' ', StringSplitOptions.RemoveEmptyEntries)))
            text = string.Join(" ", text.Split("  ", StringSplitOptions.RemoveEmptyEntries).Select(w => w.Replace(" ", "")));

        // A tab or a wide run of spaces separates columns ("Acme<tab>Jan 2020 - Present")
        text = WideSpaces().Replace(text.Replace("\t", ResumeImportConstants.ColumnSeparator), ResumeImportConstants.ColumnSeparator);
        return Spaces().Replace(text, " ").Trim(TrimColumns);
    }

    // Single letters, with the odd kerned group ("S A M C A RT E R", "F O R M A C IÓN")
    private static bool IsLetterSpaced(string[] tokens) =>
        tokens.Count(t => t.Length == 1) >= ResumeImportConstants.MinLetterSpacedLetters
        && tokens.All(t => t.Length <= ResumeImportConstants.MaxKernedLetters)
        && tokens.Count(t => t.Length == 1) >= tokens.Length * ResumeImportConstants.MinSingleLetterShare;

    private static bool Continues(Line previous, string next) =>
        previous.Wraps && !IsSectionTitle(next) && !IsDateOnly(previous.Text) && !StartsAsContact(next)
        && !StartsWithLabel(next) && !DateRange().IsMatch(next) && !next.Contains(ResumeImportConstants.ColumnSeparator);

    // "Languages: Go, Python" starts a line of its own
    private static bool StartsWithLabel(string text) =>
        text.IndexOf(ResumeImportConstants.LabelEnd) is > 0 and var colon && colon <= ResumeImportConstants.MaxLabelLength
        && WordCount(text[..colon]) <= ResumeImportConstants.MaxSectionTitleWords;

    private static bool StartsAsContact(string text) =>
        Email().Match(text) is { Success: true, Index: 0 } || Link().Match(text) is { Success: true, Index: 0 };

    private static bool GoesOn(string previous, string next)
    {
        if (IsSectionTitle(previous) || IsSectionTitle(next) || IsDateOnly(next) || IsDateOnly(previous)) return false;
        if (ResumeImportConstants.OpenLineEnds.Contains(previous[^1]) || Connectors.Contains(LastWord(previous))) return true;

        var first = next[0];
        return (char.IsLower(first) || char.IsDigit(first) || ResumeImportConstants.ContinuationStarts.Contains(first)) && !StartsAsContact(next);
    }

    // "github.com/alexrivera-" + "dev/openmq" and "zero-" + "downtime" were cut at the hyphen
    private static string JoinWrapped(string previous, string next) =>
        previous[^1] == ResumeImportConstants.Hyphen && previous.Length > 1 && char.IsLetter(previous[^2]) && char.IsLower(next[0])
            ? previous + next
            : previous + " " + next;

    private static (List<Line> Header, List<(ResumeSection Section, List<Line> Lines)> Sections) SplitSections(List<Line> lines)
    {
        var header = new List<Line>();
        var sections = new List<(ResumeSection, List<Line>)>();
        var current = header;
        var kind = ResumeSection.Header;

        foreach (var line in lines)
        {
            var title = line.IsItem ? null : TitleOf(line.Text, kind);
            if (title == null)
            {
                current.Add(line);
                continue;
            }

            // A floated date can print above the title: the first entry's dates end up in the section before
            var carried = new List<Line>();
            if (kind is ResumeSection.Header or ResumeSection.Other && current.Count > 0 && IsDateRangeOnly(current[^1].Text))
            {
                carried.Add(current[^1]);
                current.RemoveAt(current.Count - 1);
            }

            current = carried;
            kind = title.Value;
            sections.Add((kind, current));
        }
        return (header, sections);
    }

    private static ResumeSection? TitleOf(string line, ResumeSection current)
    {
        if (SectionOf(line) is not { } section) return null;
        // Inside skills, "Languages:" is a label, not the spoken-languages section
        return current == ResumeSection.Skills && section == ResumeSection.Other && line.EndsWith(ResumeImportConstants.LabelEnd)
            ? null
            : section;
    }

    private static bool IsSectionTitle(string line) => SectionOf(line) != null;

    private static ResumeSection? SectionOf(string line)
    {
        if (WordCount(line) > ResumeImportConstants.MaxSectionTitleWords) return null;
        var key = Spaces().Replace(line.TrimEnd(ResumeImportConstants.LabelEnd).Replace(ResumeImportConstants.Ampersand, ResumeImportConstants.And), " ").Trim();
        if (SectionByTitle.TryGetValue(key, out var section)) return section;

        var and = key.IndexOf(ResumeImportConstants.And, StringComparison.OrdinalIgnoreCase);
        return and > 0 && SectionByTitle.TryGetValue(key[..and], out section) ? section : null;
    }

    private static Header ReadHeader(List<Line> lines)
    {
        var all = string.Join(ResumeImportConstants.ColumnSeparator, lines.Select(l => l.Text));
        var header = new Header
        {
            Email = Email().Match(all).Value,
            Phone = Phone().Matches(all).Select(m => m.Value.Trim())
                .FirstOrDefault(p => p.Count(char.IsDigit) >= ResumeImportConstants.MinPhoneDigits) ?? string.Empty,
            LinkedIn = Bare(LinkedIn().Match(all).Value),
            GitHub = Bare(GitHub().Match(all).Value)
        };

        // "Robin Quill | robin.quill@email.com": the name can share its line with the contact details
        var nameAt = lines.FindIndex(l => IsNameLike(FirstColumn(l.Text)));
        if (nameAt >= 0)
        {
            var name = FirstColumn(lines[nameAt].Text);
            // Letter-spaced names print one word per line ("A L E X" / "R I V E R A")
            if (WordCount(name) == 1 && nameAt + 1 < lines.Count && WordCount(lines[nameAt + 1].Text) == 1 && IsNameLike(lines[nameAt + 1].Text))
                name += " " + lines[nameAt + 1].Text;
            header.FullName = name.Any(char.IsLower) ? name : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant());
        }

        header.Location = lines
            .SelectMany((l, i) => Parts(l.Text).Skip(i == nameAt ? 1 : 0))
            .Select(WithoutContacts)
            .FirstOrDefault(p => p.Any(char.IsLetter) && WordCount(p) <= ResumeImportConstants.MaxLocationWords
                                 && (p.Contains(',') || ResumeImportConstants.RemoteWords.Any(w => p.Equals(w, StringComparison.OrdinalIgnoreCase))))
            ?? string.Empty;
        return header;
    }

    private static string FirstColumn(string line) => line.Split(ResumeImportConstants.ColumnSeparator)[0].Trim();

    private static string WithoutContacts(string part)
    {
        foreach (var pattern in new[] { Email(), Phone(), LinkedIn(), Link() }) part = pattern.Replace(part, " ");
        return part.Trim(TrimChars);
    }

    private static bool IsNameLike(string text) =>
        text.Any(char.IsLetter) && !text.Any(char.IsDigit) && !text.Contains('@') && !Link().IsMatch(text)
        && WordCount(text) <= ResumeImportConstants.MaxNameWords && !text.Contains(',') && !text.Contains('|');

    /// <summary>
    /// Experience entries are found by their date ranges. The role and company are the short lines next to the
    /// dates: on their line or after them ("Mar 2022 – Present" / role / company, as a PDF of this app reads), or
    /// above them when only those name a role ("Company" / "Role" / "2020 - 2022" / "• bullet").
    /// </summary>
    private static List<Experience> ReadExperience(List<Line> lines)
    {
        var anchors = lines.Select((l, i) => (l, i)).Where(x => EntryDates(x.l) != null).Select(x => x.i).ToList();

        // Each entry: where its head starts, where its bullets start, the head's lines and the dates
        var entries = new List<(int Start, int BodyStart, List<string> Head, Match Dates)>();
        var floor = 0;
        for (var k = 0; k < anchors.Count; k++)
        {
            var anchor = anchors[k];
            var next = k + 1 < anchors.Count ? anchors[k + 1] : lines.Count;
            var dates = EntryDates(lines[anchor])!;
            var head = new List<string>();
            var rest = Without(lines[anchor].Text, dates);
            if (rest.Length > 0) head.Add(rest);

            var forward = new List<string>();
            var bodyStart = anchor + 1;
            while (head.Count + forward.Count < ResumeImportConstants.MaxEntryHeadLines && bodyStart < next && IsHeadLine(lines[bodyStart]))
                forward.Add(lines[bodyStart++].Text);

            var backward = new List<string>();
            var start = anchor;
            while (backward.Count < ResumeImportConstants.MaxEntryHeadLines && start > floor && IsHeadLine(lines[start - 1]))
                backward.Insert(0, lines[--start].Text);

            var namedBelow = head.Concat(forward).Any(HasRoleWord);
            if (backward.Count > 0 && !namedBelow && (backward.Any(HasRoleWord) || head.Count + forward.Count == 0))
            {
                // Company and role above the dates; below them only the location, if any
                var locations = forward.TakeWhile(IsLocationLike).ToList();
                head.InsertRange(0, backward);
                head.AddRange(locations);
                bodyStart = anchor + 1 + locations.Count;
            }
            else
            {
                head.AddRange(forward);
                start = anchor;
            }

            entries.Add((start, bodyStart, head, dates));
            floor = bodyStart;
        }

        var experiences = new List<Experience>();
        if (entries.Count == 0)
        {
            // No dates anywhere: only entries that a role line and a company line open
            experiences.AddRange(DatelessEntries(lines).Skip(1).Select(d => NewExperience(d.Head!, null, d.Body)));
            return experiences;
        }

        for (var k = 0; k < entries.Count; k++)
        {
            var (_, bodyStart, head, dates) = entries[k];
            var bodyEnd = k + 1 < entries.Count ? entries[k + 1].Start : lines.Count;
            var parts = DatelessEntries(lines.Skip(bodyStart).Take(Math.Max(0, bodyEnd - bodyStart)).ToList());
            experiences.Add(NewExperience(head, dates, parts[0].Body));
            experiences.AddRange(parts.Skip(1).Select(d => NewExperience(d.Head!, null, d.Body)));
        }
        return experiences;
    }

    private static Experience NewExperience(List<string> head, Match? dates, List<Line> body)
    {
        var (role, company, location) = RoleCompanyLocation(head);
        return new Experience
        {
            Role = role,
            Company = company,
            Location = location,
            StartDate = dates == null ? string.Empty : ProfileDate(dates.Groups["start"].Value),
            EndDate = dates == null ? string.Empty : ProfileDate(dates.Groups["end"].Value),
            Bullets = body.Select(l => l.Text).ToList()
        };
    }

    /// <summary>
    /// Splits lines at the entries that have no dates: a short line with a job title followed by a short line
    /// without one ("Warehouse Assistant" / "LogiMart"). The first part (Head null) is what comes before them.
    /// </summary>
    private static List<(List<string>? Head, List<Line> Body)> DatelessEntries(List<Line> lines)
    {
        var parts = new List<(List<string>? Head, List<Line> Body)> { (null, new List<Line>()) };
        for (var i = 0; i < lines.Count; i++)
        {
            if (i + 1 < lines.Count && IsShortHead(lines[i]) && HasRoleWord(lines[i].Text) && IsShortHead(lines[i + 1]) && !HasRoleWord(lines[i + 1].Text))
            {
                parts.Add((new List<string> { lines[i].Text, lines[i + 1].Text }, new List<Line>()));
                i++;
                continue;
            }
            parts[^1].Body.Add(lines[i]);
        }
        return parts;
    }

    private static bool IsShortHead(Line line) =>
        IsHeadLine(line) && WordCount(line.Text) <= ResumeImportConstants.MaxDatelessHeadWords && !line.Text.EndsWith('.');

    // A date range on a short line: "Led the 2019 - 2021 migration of ..." in a bullet is not an entry's dates
    private static Match? EntryDates(Line line) =>
        !line.IsItem && DateRange().Match(line.Text) is { Success: true } m
                     && WordCount(Without(line.Text, m)) <= ResumeImportConstants.MaxHeadWords
            ? m
            : null;

    private static bool IsHeadLine(Line line) =>
        !line.IsItem && WordCount(line.Text) <= ResumeImportConstants.MaxHeadWords && !EndsSentence(line.Text) && !IsSectionTitle(line.Text);

    /// <summary>
    /// The role is the part with a job-title word ("Engineer", "Desarrollador"); the company is the first other
    /// part and the location the part that reads as a place. Parts are the head's lines, split at their columns
    /// and, on the role's line, at "at", a dash or " | " ("Acme - Senior Engineer").
    /// </summary>
    private static (string Role, string Company, string Location) RoleCompanyLocation(List<string> head)
    {
        var parts = head.SelectMany(h => h.Split(ResumeImportConstants.ColumnSeparator)).Select(p => p.Trim(TrimChars)).Where(p => p.Length > 0).ToList();
        if (parts.Count == 0) return (string.Empty, string.Empty, string.Empty);

        var roleAt = Math.Max(0, parts.FindIndex(HasRoleWord));
        var inRole = SplitOnce(parts[roleAt], ResumeImportConstants.RoleCompanyWords) ?? Parts(parts[roleAt]);
        if (inRole.Count == 1 && parts.Count == 1) inRole = parts[0].Split(", ").ToList();
        var roleInParts = Math.Max(0, inRole.FindIndex(HasRoleWord));

        var others = inRole.Where((_, i) => i != roleInParts).Concat(parts.Where((_, i) => i != roleAt)).ToList();
        var role = inRole[roleInParts];
        if (others.Count == 0) return (role, string.Empty, string.Empty);
        if (others.Count == 1)
        {
            // "Stripe, Remote · San Francisco, CA": the company and its place on one line
            var (company, place) = NameAndPlace(others[0], firstComma: true);
            return (role, company, place);
        }

        var location = others.LastOrDefault(IsLocationLike);
        if (location == null) return (role, others[0], string.Empty);
        return (role, others.First(o => !ReferenceEquals(o, location)), location);
    }

    private static bool HasRoleWord(string text) => ResumeImportConstants.RoleWords.Any(w => SkillNormalizer.ContainsTerm(text, w));

    private static bool IsLocationLike(string text) =>
        !HasRoleWord(text) && WordCount(text) <= ResumeImportConstants.MaxLocationWords
        && (text.Contains(',') || ResumeImportConstants.RemoteWords.Any(w => SkillNormalizer.ContainsTerm(text, w)));

    /// <summary>
    /// Education entries, one or two short lines each: the line with a school word is the institution (the rest of
    /// its columns the location), the one with a degree word the degree (the rest GPA and honors), and a date the
    /// graduation date. A second institution, degree or date, or an entry already complete, starts the next one.
    /// </summary>
    private static List<Education> ReadEducation(List<Line> lines)
    {
        var entries = new List<Education>();
        Education? current = null;
        foreach (var line in lines)
        {
            if (current != null && Labeled(line.Text, ResumeImportConstants.CourseLabels) is { } courses)
            {
                current.RelevantCourses.AddRange(Items(courses));
                continue;
            }
            if (current != null && Labeled(line.Text, ResumeImportConstants.GpaLabels) is { } gpa)
            {
                current.GPA = gpa;
                continue;
            }
            // A description under the entry ("Thesis on ...") is not a field of the profile
            var first = FirstColumn(line.Text);
            if (line.IsItem || EndsSentence(first) || WordCount(first) > ResumeImportConstants.MaxHeadWords) continue;

            var text = line.Text;
            string? date = null;
            if (EntryDates(line) is { } range)
            {
                date = ProfileDate(range.Groups["end"].Value);
                text = Without(text, range);
            }
            else if (SingleDate().Match(text) is { Success: true } single)
            {
                date = ProfileDate(single.Value);
                text = Without(text, single);
            }

            var parts = text.Split(ResumeImportConstants.ColumnSeparator).Select(p => p.Trim(TrimChars)).Where(p => p.Length > 0).ToList();
            var isInstitution = parts.Count > 0 && HasInstitutionWord(parts[0]);
            var isDegree = parts.Count > 0 && !isInstitution && HasDegreeWord(parts[0]);
            var complete = current is { Degree.Length: > 0, Institution.Length: > 0, GraduationDate.Length: > 0 };
            if (current == null || complete
                || (isInstitution && current.Institution.Length > 0) || (isDegree && current.Degree.Length > 0)
                || (date != null && current.GraduationDate.Length > 0))
                entries.Add(current = new Education());

            if (date != null) current.GraduationDate = date;
            if (parts.Count == 0) continue;

            var asInstitution = isInstitution || (!isDegree && current.Institution.Length == 0);
            if (asInstitution && current.Institution.Length == 0)
            {
                if (parts.Count == 1) (current.Institution, current.Location) = NameAndPlace(parts[0], firstComma: false);
                else (current.Institution, current.Location) = (parts[0], string.Join(", ", parts.Skip(1)));
            }
            else if (current.Degree.Length == 0)
            {
                ReadDegree(parts, current);
            }
        }
        return entries.Where(e => e.Degree.Length > 0 || e.Institution.Length > 0).ToList();
    }

    private static bool HasInstitutionWord(string text) => ResumeImportConstants.InstitutionWords.Any(w => SkillNormalizer.ContainsTerm(text, w));

    private static bool HasDegreeWord(string text) => ResumeImportConstants.DegreeWords.Any(w => SkillNormalizer.ContainsTerm(text, w));

    // "B.S. Computer Science | GPA: 3.7 | Magna Cum Laude"
    private static void ReadDegree(List<string> parts, Education entry)
    {
        entry.Degree = parts[0];
        foreach (var part in parts.Skip(1))
        {
            if (Labeled(part, ResumeImportConstants.GpaLabels) is { } gpa) entry.GPA = gpa;
            else entry.Honors = entry.Honors.Length == 0 ? part : $"{entry.Honors}; {part}";
        }
    }

    /// <summary>
    /// Skills lines, "Label: a, b, c" or just items. A known label picks the profile list; otherwise each item goes
    /// to the list it is known in, and to Tools when it is not known. "Interests:" and the like are skipped.
    /// </summary>
    private static void ReadSkills(List<Line> lines, Skills skills)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        SkillGroup? pendingGroup = null;
        var pendingSkip = false;
        foreach (var line in lines)
        {
            var text = line.Text;
            var group = pendingGroup;
            var skip = pendingSkip;
            (pendingGroup, pendingSkip) = (null, false);

            var colon = text.IndexOf(ResumeImportConstants.LabelEnd);
            if (colon >= 0 && colon <= ResumeImportConstants.MaxLabelLength)
            {
                var label = Spaces().Replace(text[..colon].Replace(ResumeImportConstants.Ampersand, ResumeImportConstants.And), " ").Trim();
                group = GroupByLabel.TryGetValue(label, out var labeled) ? labeled : null;
                skip = SkippedSkillLabels.Contains(label);
                text = text[(colon + 1)..];
                // "Languages:" on a line of its own labels the line below it
                if (text.Trim().Length == 0) { (pendingGroup, pendingSkip) = (group, skip); continue; }
            }
            if (skip) continue;

            foreach (var item in Items(text).Where(i => i.Length <= ResumeImportConstants.MaxSkillLength && seen.Add(i)))
                ListOf(skills, group ?? GroupOf(item)).Add(item);
        }
    }

    private static SkillGroup GroupOf(string item) =>
        Names(item).Select(KnownGroup).FirstOrDefault(g => g != null) ?? SkillGroup.Tools;

    private static SkillGroup? KnownGroup(string name)
    {
        foreach (var (group, names) in KnownSkills)
            if (names.Contains(name)) return group;
        return null;
    }

    private static bool IsKnownSkill(string item) =>
        Names(item).Any(n => KnownGroup(n) != null || SkillDictionary.Find(n) != null);

    // The forms an item is looked up by: as written, its dictionary name, without its version or its parentheses,
    // and each side of " / " (".NET 9 / ASP.NET Core")
    private static IEnumerable<string> Names(string item)
    {
        foreach (var side in item.Split(" / ").Prepend(item))
        {
            var bare = VersionSuffix().Replace(side.Split(" (")[0], "").Trim();
            foreach (var form in new[] { side, bare })
            {
                yield return form.ToLowerInvariant();
                yield return SkillNormalizer.Normalize(form);
            }
        }
    }

    private static List<string> ListOf(Skills skills, SkillGroup group) => group switch
    {
        SkillGroup.Languages => skills.Languages,
        SkillGroup.Frameworks => skills.Frameworks,
        SkillGroup.Databases => skills.Databases,
        SkillGroup.Cloud => skills.Cloud,
        SkillGroup.Practices => skills.Practices,
        _ => skills.Tools
    };

    /// <summary>
    /// Projects: a short line opens a project (after the previous one's technologies, bullets or a finished
    /// description). Its link is any address on it, a column of known skills its stack; the next lines are its
    /// description, "Technologies:" or a line of known skills its stack, and bullets its highlights.
    /// </summary>
    private static List<Project> ReadProjects(List<Line> lines)
    {
        var projects = new List<Project>();
        Project? current = null;
        var closed = true;
        foreach (var line in lines)
        {
            if (current != null && Labeled(line.Text, ResumeImportConstants.TechnologiesLabels) is { } stack)
            {
                current.Technologies.AddRange(Items(stack));
                closed = true;
                continue;
            }
            if (current != null && !closed && !line.IsItem && current.Technologies.Count == 0 && IsTechList(line.Text))
            {
                current.Technologies.AddRange(Items(line.Text));
                continue;
            }
            if (line.IsItem && current != null)
            {
                current.Highlights.Add(line.Text);
                closed = true;
                continue;
            }

            var opensProject = current == null || closed
                || (current.Description.EndsWith('.') && WordCount(line.Text) <= ResumeImportConstants.MaxProjectNameWords && !line.Text.EndsWith('.'));
            if (!opensProject)
            {
                current!.Description = $"{current.Description} {line.Text}".Trim();
                continue;
            }

            projects.Add(current = new Project());
            closed = false;
            foreach (var part in line.Text.Split(ResumeImportConstants.ColumnSeparator))
            {
                var text = part.Trim(TrimChars);
                if (Link().Match(text) is { Success: true } link && current.Link.Length == 0)
                {
                    current.Link = Bare(link.Value);
                    text = Without(text, link);
                }
                if (text.Length == 0) continue;
                if (current.Name.Length > 0 && IsTechList(text)) current.Technologies.AddRange(Items(text));
                else if (current.Name.Length > 0) continue;
                else if (NameAndStack(text) is { } split) (current.Name, current.Technologies) = split;
                else current.Name = text;
            }
        }
        return projects.Where(p => p.Name.Length > 0).ToList();
    }

    // "Temple Trips  .NET · EF Core · PostgreSQL": the name, then its stack on the same line
    private static (string Name, List<string> Stack)? NameAndStack(string text)
    {
        var items = Items(text).ToList();
        if (items.Count < 2 || !IsTechList(string.Join(", ", items.Skip(1)))) return null;

        var words = items[0].Split(' ');
        for (var n = 1; n < words.Length; n++)
        {
            var tail = string.Join(' ', words[^n..]);
            if (IsKnownSkill(tail)) return (string.Join(' ', words[..^n]), items.Skip(1).Prepend(tail).ToList());
        }
        return null;
    }

    private static bool IsTechList(string text)
    {
        var items = Items(text).ToList();
        return items.Count > 1 && items.Count(IsKnownSkill) >= items.Count * ResumeImportConstants.MinKnownTechShare;
    }

    /// <summary>The value after "Label:" when the line starts with one of the labels, otherwise null.</summary>
    private static string? Labeled(string text, string[] labels)
    {
        var colon = text.IndexOf(ResumeImportConstants.LabelEnd);
        if (colon < 0 || colon > ResumeImportConstants.MaxLabelLength) return null;
        var label = text[..colon].Trim();
        return labels.Any(l => label.Equals(l, StringComparison.OrdinalIgnoreCase)) ? text[(colon + 1)..].Trim() : null;
    }

    // Items of a list, split at commas, bullets and middle dots, but not inside parentheses: "PostgreSQL (indexing, raw SQL)"
    private static IEnumerable<string> Items(string text)
    {
        var start = 0;
        var depth = 0;
        for (var i = 0; i <= text.Length; i++)
        {
            if (i < text.Length)
            {
                if (text[i] == '(') depth++;
                else if (text[i] == ')') depth = Math.Max(0, depth - 1);
                if (depth > 0 || !ResumeImportConstants.ItemSeparators.Contains(text[i])) continue;
            }
            var item = text[start..i].Trim().TrimEnd('.');
            if (item.Length > 0) yield return item;
            start = i + 1;
        }
    }

    /// <summary>"Name, Place". <paramref name="firstComma"/> splits at the first comma ("Stripe, Remote · San Francisco, CA");
    /// otherwise the last two parts are the place ("University of California, Berkeley, Berkeley, CA").</summary>
    private static (string Name, string Place) NameAndPlace(string text, bool firstComma)
    {
        // "Brigham Young University – Idaho · Rexburg, Idaho, USA": a middle dot puts the place apart
        var dot = text.IndexOf(ResumeImportConstants.PlaceSeparator, StringComparison.Ordinal);
        if (!firstComma && dot > 0) return (text[..dot].Trim(), text[(dot + ResumeImportConstants.PlaceSeparator.Length)..].Trim());

        var parts = text.Split(", ");
        if (parts.Length == 1) return (text, string.Empty);
        var nameParts = firstComma || parts.Length < 3 ? 1 : parts.Length - 2;
        return (string.Join(", ", parts.Take(nameParts)), string.Join(", ", parts.Skip(nameParts)));
    }

    private static List<string>? SplitOnce(string text, string[] separators)
    {
        foreach (var separator in separators)
        {
            var at = text.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (at > 0) return new List<string> { text[..at].Trim(), text[(at + separator.Length)..].Trim() };
        }
        return null;
    }

    private static List<string> Parts(string text) =>
        text.Split(ResumeImportConstants.PartSeparators, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0).ToList()
            is { Count: > 0 } parts ? parts : new List<string> { text };

    // The text without the match, its columns kept: "Acme | Jan 2020 - Present | Remote" is "Acme | Remote"
    private static string Without(string text, Match match)
    {
        var joined = EmptyParens().Replace(text[..match.Index] + ResumeImportConstants.ColumnSeparator + text[(match.Index + match.Length)..], " ");
        return string.Join(ResumeImportConstants.ColumnSeparator, joined.Split('|').Select(p => p.Trim(TrimChars)).Where(p => p.Length > 0));
    }

    private static bool IsDateOnly(string text) =>
        IsDateRangeOnly(text) || (SingleDate().Match(text) is { Success: true } m && Without(text, m).Length == 0);

    private static bool IsDateRangeOnly(string text) => DateRange().Match(text) is { Success: true } m && Without(text, m).Length == 0;

    // "Inc." or "S.A." at the end of a company line is not the end of a sentence
    private static bool EndsSentence(string text)
    {
        if (!text.EndsWith('.')) return false;
        var last = LastWord(text).TrimEnd('.');
        return !(last.Length is > 0 and <= ResumeImportConstants.MaxAbbreviationLength && char.IsUpper(last[0]));
    }

    private static int? MonthOf(string name)
    {
        var prefix = name.ToLowerInvariant()[..Math.Min(MonthPrefixLength, name.Length)];
        var index = Array.FindIndex(ResumeImportConstants.MonthPrefixes, m => m.Contains(prefix));
        return index >= 0 ? index + 1 : null;
    }

    private static string Bare(string link) => Scheme().Replace(link, "").TrimEnd('/');

    private static string LastWord(string text) => text[(text.LastIndexOf(' ') + 1)..];

    private static int WordCount(string text) => text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    private static HashSet<string> Set(IEnumerable<string> words) => new(words, StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Nested Types

    /// <summary>A line of the resume. <c>Wraps</c>: the PDF line reached the right edge, so the next one goes on with it.</summary>
    private sealed record Line(string Text, bool IsItem, bool Wraps = false);

    #endregion
}

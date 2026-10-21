using ErCv.Web.Features.Analysis;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Profiles;

public enum HealthRule
{
    NoMetrics,
    WeakVerb,
    NoSkillTags,
    Dates
}

/// <summary>One thing worth improving in a profile. <see cref="Path"/> points at the field, like the validator's paths.</summary>
public record HealthFinding(HealthRule Rule, string Path, string Message);

/// <summary>
/// Advice for a profile that is already valid (PROF-04). Nothing here blocks generating; it points at what makes
/// a CV weaker. Each rule is independent, so a profile can be flagged by several.
/// </summary>
public static class ProfileHealthChecker
{
    #region Constants

    // Roles that overlap by no more than this many months are treated as a normal hand-over, not a mistake.
    private const int OverlapToleranceMonths = 1;

    /// <summary>Openers that describe a duty or a presence instead of something the person did.</summary>
    private static readonly HashSet<string> WeakVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "responsible", "worked", "helped", "assisted", "participated", "involved", "tasked",
        "handled", "did", "made", "used", "was", "were", "am", "duties", "various", "tried"
    };

    #endregion

    #region Public Methods

    public static List<HealthFinding> Check(CVMaster? profile)
    {
        var findings = new List<HealthFinding>();
        var experience = profile?.Experience ?? new List<Experience>();

        for (var i = 0; i < experience.Count; i++)
        {
            var x = experience[i];
            if (x == null) continue;

            if (x.Skills == null || x.Skills.All(string.IsNullOrWhiteSpace))
                findings.Add(new(HealthRule.NoSkillTags, $"experience[{i}].skills",
                    $"{Label(x)} has no skill tags, so it cannot be matched to a job description."));

            for (var b = 0; b < (x.Bullets?.Count ?? 0); b++)
            {
                var bullet = x.Bullets![b];
                if (string.IsNullOrWhiteSpace(bullet)) continue;

                if (!ATSConstants.Metrics().IsMatch(bullet))
                    findings.Add(new(HealthRule.NoMetrics, $"experience[{i}].bullets[{b}]",
                        $"{Label(x)}, bullet {b + 1}: no number or percentage. Add a result such as 'by 30%'."));

                var opener = FirstWord(bullet);
                if (WeakVerbs.Contains(opener))
                    findings.Add(new(HealthRule.WeakVerb, $"experience[{i}].bullets[{b}]",
                        $"{Label(x)}, bullet {b + 1}: starts with '{opener}'. Open with what you did, such as 'Built' or 'Reduced'."));
            }
        }

        CheckDates(experience, findings);
        return findings;
    }

    #endregion

    #region Private Methods

    private static void CheckDates(List<Experience> experience, List<HealthFinding> findings)
    {
        var ranges = new List<(int Index, int Start, int End)>();

        for (var i = 0; i < experience.Count; i++)
        {
            var x = experience[i];
            if (x == null) continue;

            var startOk = DateParser.IsPresent(x.StartDate) == false &&
                          DateParser.TryParseStrict(x.StartDate, out _, out _);
            if (!startOk)
            {
                findings.Add(new(HealthRule.Dates, $"experience[{i}].startDate",
                    $"{Label(x)}: the start date '{x.StartDate}' cannot be read."));
                continue;
            }

            var endBlank = string.IsNullOrWhiteSpace(x.EndDate);
            if (!endBlank && !DateParser.IsPresent(x.EndDate) && !DateParser.TryParseStrict(x.EndDate, out _, out _))
            {
                findings.Add(new(HealthRule.Dates, $"experience[{i}].endDate",
                    $"{Label(x)}: the end date '{x.EndDate}' cannot be read."));
                continue;
            }

            DateParser.TryGetMonthIndex(x.StartDate, isEnd: false, out var start);
            var end = endBlank
                ? DateParser.TryGetMonthIndex("Present", isEnd: true, out var now) ? now : start
                : DateParser.TryGetMonthIndex(x.EndDate, isEnd: true, out var parsed) ? parsed : start;

            if (end < start)
            {
                findings.Add(new(HealthRule.Dates, $"experience[{i}].endDate",
                    $"{Label(x)}: ends before it starts."));
                continue;
            }

            ranges.Add((i, start, end));
        }

        for (var a = 0; a < ranges.Count; a++)
            for (var b = a + 1; b < ranges.Count; b++)
            {
                var overlap = Math.Min(ranges[a].End, ranges[b].End) - Math.Max(ranges[a].Start, ranges[b].Start);
                if (overlap <= OverlapToleranceMonths) continue;

                findings.Add(new(HealthRule.Dates, $"experience[{ranges[b].Index}].startDate",
                    $"{Label(experience[ranges[a].Index])} and {Label(experience[ranges[b].Index])} overlap by {overlap} months. Fine if you held both at once."));
            }
    }

    private static string Label(Experience x)
    {
        var role = string.IsNullOrWhiteSpace(x.Role) ? "Role" : x.Role.Trim();
        return string.IsNullOrWhiteSpace(x.Company) ? role : $"{role} at {x.Company.Trim()}";
    }

    private static string FirstWord(string bullet)
    {
        var text = bullet.TrimStart(' ', '-', '*', '•', '\t');
        var end = 0;
        while (end < text.Length && char.IsLetter(text[end])) end++;
        return text[..end];
    }

    #endregion
}

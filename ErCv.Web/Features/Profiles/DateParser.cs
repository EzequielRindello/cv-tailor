using System.Text.RegularExpressions;

namespace ErCv.Web.Features.Profiles;

/// <summary>
/// Parses the free-form dates found in a CV profile ("Jun 2024", "June 2024", "2024-06", "06/2024", "2024",
/// "Present") into month indexes so roles can be ordered and experience can be measured.
/// </summary>
public static partial class DateParser
{
    #region Constants

    private const int MinYear = 1950;
    private const int MaxYearsAhead = 1;
    private const int MonthsPerYear = 12;
    private const int MonthPrefixLength = 3;

    private static readonly string[] PresentWords = { "present", "current", "now" };

    private static readonly string[] MonthPrefixes =
        { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };

    [GeneratedRegex(@"^(?<m>[A-Za-z]{3,9})\.?,?\s+(?<y>\d{4})$")]
    private static partial Regex MonthNameYear();

    [GeneratedRegex(@"^(?<y>\d{4})-(?<m>\d{1,2})$")]
    private static partial Regex YearMonth();

    [GeneratedRegex(@"^(?<m>\d{1,2})/(?<y>\d{4})$")]
    private static partial Regex MonthYear();

    [GeneratedRegex(@"^\d{4}$")]
    private static partial Regex YearOnly();

    [GeneratedRegex(@"\b(19|20)\d{2}\b")]
    private static partial Regex AnyYear();

    // "Today" for one async flow only, so the snapshot tests (TEST-08) can pin it without affecting other tests
    private static readonly AsyncLocal<DateTime?> PinnedNow = new();

    #endregion

    #region Properties

    /// <summary>The date "Present" roles end at and recency is measured from: the current date unless pinned.</summary>
    public static DateTime Now => PinnedNow.Value ?? DateTime.Now;

    #endregion

    #region Public Methods

    /// <summary>Pins <see cref="Now"/> for the calling async flow until the result is disposed (tests only).</summary>
    public static IDisposable PinNow(DateTime now)
    {
        PinnedNow.Value = now;
        return new Unpin();
    }

    public static bool IsPresent(string? value)
    {
        var v = value?.Trim();
        return v != null && PresentWords.Any(w => v.Equals(w, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Parses only the documented formats (no "any year in the text" fallback). Used to validate what a
    /// visitor types, where "13/2024" must be an error and not silently read as 2024.
    /// </summary>
    public static bool TryParseStrict(string? value, out int year, out int? month)
    {
        year = 0;
        month = null;
        if (string.IsNullOrWhiteSpace(value)) return false;
        var text = value.Trim();

        if (TryMatch(MonthNameYear(), text, out year, out month, nameMonth: true) ||
            TryMatch(YearMonth(), text, out year, out month, nameMonth: false) ||
            TryMatch(MonthYear(), text, out year, out month, nameMonth: false))
            return IsPlausible(year);

        if (YearOnly().IsMatch(text)) { year = int.Parse(text); month = null; return IsPlausible(year); }

        year = 0;
        return false;
    }

    /// <summary>Parses a date to its year, plus its month when the text gives one.</summary>
    public static bool TryParse(string? value, out int year, out int? month)
    {
        if (TryParseStrict(value, out year, out month)) return true;
        if (string.IsNullOrWhiteSpace(value)) return false;
        var text = value.Trim();

        // Tolerant fallback for text such as "Spring 2019": any plausible year in the string.
        var any = AnyYear().Match(text);
        if (any.Success) { year = int.Parse(any.Value); month = null; return IsPlausible(year); }

        year = 0;
        return false;
    }

    /// <summary>
    /// Month index (year * 12 + month) of a date. A year with no month is January for a start date and
    /// December for an end date. "Present" is the current month.
    /// </summary>
    public static bool TryGetMonthIndex(string? value, bool isEnd, out int monthIndex)
    {
        monthIndex = 0;
        if (IsPresent(value))
        {
            monthIndex = CurrentMonthIndex();
            return true;
        }
        if (!TryParse(value, out var year, out var month)) return false;

        monthIndex = year * MonthsPerYear + ((month ?? (isEnd ? MonthsPerYear : 1)) - 1);
        return true;
    }

    /// <summary>
    /// Ordering key for "most recent role": by end date, then start date. "Present" sorts above any date,
    /// an unreadable end date falls back to the start date, and a role with no readable dates sorts last.
    /// </summary>
    public static (int End, int Start) RecencyKey(string? start, string? end)
    {
        var startKey = TryGetMonthIndex(start, isEnd: false, out var s) ? s : int.MinValue;

        int endKey;
        if (IsPresent(end)) endKey = int.MaxValue;
        else if (TryGetMonthIndex(end, isEnd: true, out var e)) endKey = e;
        else endKey = startKey;

        return (endKey, startKey);
    }

    /// <summary>
    /// Whole years covered by the ranges. Overlapping roles are merged and gaps are not counted. A range with
    /// no readable start is ignored; one with no readable end is treated as ongoing.
    /// </summary>
    public static int YearsOfExperience(IEnumerable<(string? Start, string? End)> ranges)
    {
        var spans = new List<(int Start, int End)>();
        foreach (var (start, end) in ranges)
        {
            if (!TryGetMonthIndex(start, isEnd: false, out var s)) continue;
            var e = TryGetMonthIndex(end, isEnd: true, out var parsedEnd) ? parsedEnd : CurrentMonthIndex();
            if (e > s) spans.Add((s, e));
        }

        var months = 0;
        int? mergedStart = null;
        var mergedEnd = 0;
        foreach (var span in spans.OrderBy(x => x.Start))
        {
            if (mergedStart == null)
            {
                mergedStart = span.Start;
                mergedEnd = span.End;
            }
            else if (span.Start <= mergedEnd)
            {
                mergedEnd = Math.Max(mergedEnd, span.End);
            }
            else
            {
                months += mergedEnd - mergedStart.Value;
                mergedStart = span.Start;
                mergedEnd = span.End;
            }
        }
        if (mergedStart != null) months += mergedEnd - mergedStart.Value;

        return months / MonthsPerYear;
    }

    #endregion

    #region Private Methods

    private static int CurrentMonthIndex() => Now.Year * MonthsPerYear + (Now.Month - 1);

    private static bool IsPlausible(int year) => year >= MinYear && year <= Now.Year + MaxYearsAhead;

    private static bool TryMatch(Regex pattern, string text, out int year, out int? month, bool nameMonth)
    {
        year = 0;
        month = null;
        var m = pattern.Match(text);
        if (!m.Success) return false;

        var monthText = m.Groups["m"].Value;
        int monthNumber;
        if (nameMonth)
        {
            monthNumber = Array.IndexOf(MonthPrefixes, monthText[..MonthPrefixLength].ToLowerInvariant()) + 1;
            if (monthNumber == 0) return false;
        }
        else if (!int.TryParse(monthText, out monthNumber) || monthNumber < 1 || monthNumber > MonthsPerYear)
        {
            return false;
        }

        year = int.Parse(m.Groups["y"].Value);
        month = monthNumber;
        return true;
    }

    #endregion

    #region Nested Types

    private sealed class Unpin : IDisposable
    {
        public void Dispose() => PinnedNow.Value = null;
    }

    #endregion
}

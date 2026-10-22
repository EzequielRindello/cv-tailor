using System.Globalization;
using ErCv.Tests.Api;
using ErCv.Tests.Fixtures;
using ErCv.Tests.Support;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Models;
using ErCv.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Snapshots;

/// <summary>One host for every case: the snapshots only call the service, so a fresh app per case buys nothing.</summary>
public sealed class SnapshotHost : IDisposable
{
    public ApiApp App { get; } = new();

    public void Dispose() => App.Dispose();
}

/// <summary>
/// TEST-08: the generated CV and every score for each profile x job description (the hand-written ones and one real
/// post per role family), against approved JSON in Snapshots/approved. A scoring or selection change shows up as a
/// diff to review instead of silent drift. DOCX only, so the selection is the one before the measured PDF fit.
/// </summary>
// Same collection as the API tests: the host closes the shared Chrome when it stops
[Collection("Documents")]
[Trait("Category", "Snapshot")]
public class GenerationSnapshotTests : IClassFixture<SnapshotHost>
{
    // "Present" roles and recency are measured from this date, so the snapshots do not drift with the calendar
    private static readonly DateTime PinnedDate = new(2026, 10, 1);

    private readonly ApiApp _app;

    public GenerationSnapshotTests(SnapshotHost host)
    {
        _app = host.App;
    }

    public static TheoryData<string, string> Cases()
    {
        var data = new TheoryData<string, string>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var jd in FixtureLoader.AllJdNames)
            data.Add(profile, jd);
        return data;
    }

    private static string Name(string profile, string jd) =>
        $"{profile}__{jd.Replace(FixtureLoader.CorpusPrefix, "corpus-", StringComparison.Ordinal)}";

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Generation_matches_its_approved_snapshot(string profile, string jd) =>
        Snapshot.Verify(Name(profile, jd), await GenerateAsync(profile, jd));

    // F28: production runs on Linux, where .NET starts with the invariant culture, and a developer machine has its
    // own. "P0" printed "0%" on one and "0 %" on the other, so every case is generated again under other cultures
    // and must match the same approved snapshot.
    [Theory]
    [InlineData("")]        // invariant: the Linux CI runner and the container
    [InlineData("es-AR")]   // decimal comma, dot for thousands
    [InlineData("de-DE")]
    [InlineData("fr-FR")]   // a space before the percent sign
    public async Task Generation_does_not_depend_on_the_culture(string culture)
    {
        // Approving writes the snapshots once, from the run above
        if (Snapshot.Approving) return;

        // The culture flows with this async test only, so tests running in parallel keep theirs
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

        foreach (var row in Cases())
        {
            var (profile, jd) = ((string)row[0], (string)row[1]);
            Snapshot.Verify(Name(profile, jd), await GenerateAsync(profile, jd));
        }
    }

    private async Task<object> GenerateAsync(string profile, string jd)
    {
        using var _ = DateParser.PinNow(PinnedDate);
        var request = new GenerateRequest
        {
            // git checks the fixtures out with CRLF on Windows and LF on Linux; the snapshots must not depend on it
            JobDescription = FixtureLoader.LoadJd(jd).ReplaceLineEndings("\n"),
            Formats = new List<string> { "docx" },
            Template = CVTemplate.Harvard
        };

        try
        {
            var (_, analysis) = await _app.InScopeAsync(sp =>
                sp.GetRequiredService<CVGeneratorService>().BuildCvAsync(request, FixtureLoader.LoadProfile(profile)));

            return new
            {
                analysis.MatchScore,
                analysis.MissingSkills,
                analysis.KeywordAnalysis,
                analysis.ATSScore,
                analysis.GeneratedCV
            };
        }
        catch (ArgumentException ex)
        {
            // A rejected request (an empty job description) is an outcome worth pinning too
            return new { Rejected = ex.Message };
        }
    }

    [Fact]
    public void Every_approved_snapshot_belongs_to_a_current_case()
    {
        var names = Cases().Select(c => Name((string)c[0], (string)c[1]));

        Assert.Empty(Snapshot.Orphans(names));
    }
}

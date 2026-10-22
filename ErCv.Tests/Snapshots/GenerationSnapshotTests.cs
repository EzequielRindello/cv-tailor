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
    public async Task Generation_matches_its_approved_snapshot(string profile, string jd)
    {
        using var _ = DateParser.PinNow(PinnedDate);
        var request = new GenerateRequest
        {
            // git checks the fixtures out with CRLF on Windows and LF on Linux; the snapshots must not depend on it
            JobDescription = FixtureLoader.LoadJd(jd).ReplaceLineEndings("\n"),
            Formats = new List<string> { "docx" },
            Template = CVTemplate.Harvard
        };

        object snapshot;
        try
        {
            var (_, analysis) = await _app.InScopeAsync(sp =>
                sp.GetRequiredService<CVGeneratorService>().BuildCvAsync(request, FixtureLoader.LoadProfile(profile)));

            snapshot = new
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
            snapshot = new { Rejected = ex.Message };
        }

        Snapshot.Verify(Name(profile, jd), snapshot);
    }

    [Fact]
    public void Every_approved_snapshot_belongs_to_a_current_case()
    {
        var names = Cases().Select(c => Name((string)c[0], (string)c[1]));

        Assert.Empty(Snapshot.Orphans(names));
    }
}

using ErCv.Tests.Fixtures;
using ErCv.Web.Models;
using ErCv.Web.Services;

namespace ErCv.Tests.Api;

/// <summary>PROF-03: generating from a profile supplied by the caller keeps the TEST-03 promises.</summary>
// Same collection as the PDF tests: the app shuts the shared headless Chromium down when a host stops.
[Collection("Documents")]
[Trait("Category", "Api")]
public class CustomProfileTests
{
    public static TheoryData<string, string> Matrix()
    {
        var data = new TheoryData<string, string>();
        foreach (var profile in new[] { FixtureLoader.DemoProfileId, "cv-master", "cv-master-senior", "cv-master-junior" })
        foreach (var jd in FixtureLoader.JdNames.Where(n => n != "empty")) // an empty JD is rejected up front
            data.Add(profile, jd);
        return data;
    }

    private static GenerateRequest Request(string jd) => new()
    {
        JobDescription = FixtureLoader.LoadJd(jd),
        Formats = new List<string> { "docx" }
    };

    private static Task<(Dictionary<string, byte[]> Documents, AnalysisResult Analysis)> Build(
        ApiApp app, GenerateRequest request, CVMaster profile) =>
        app.InScopeAsync(sp => ((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!)
            .BuildCvAsync(request, profile));

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task Custom_profile_output_only_contains_what_the_profile_says(string profileId, string jd)
    {
        using var app = new ApiApp();
        var master = FixtureLoader.LoadProfile(profileId);

        var (_, analysis) = await Build(app, Request(jd), master);

        var masterBullets = master.Experience.SelectMany(e => e.Bullets).ToHashSet();
        Assert.All(analysis.GeneratedCV.Experience.SelectMany(e => e.Bullets),
            b => Assert.Contains(b, masterBullets));
        Assert.Equal(master.Header.FullName, analysis.GeneratedCV.Header.FullName);
        Assert.Equal(master.Header.Email, analysis.GeneratedCV.Header.Email);
        Assert.InRange(analysis.MatchScore.Overall, 0, 100);
        Assert.InRange(analysis.ATSScore.Overall, 0, 100);
    }

    [Fact]
    public async Task Custom_profile_is_not_mutated_by_generation()
    {
        using var app = new ApiApp();
        var master = FixtureLoader.LoadProfile("cv-master");
        var before = System.Text.Json.JsonSerializer.Serialize(master);

        await Build(app, Request("backend-go-kafka"), master);

        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(master));
    }

    [Fact]
    public async Task Custom_profile_matches_the_stored_profile_with_the_same_content()
    {
        using var app = new ApiApp();
        var master = FixtureLoader.LoadProfile(FixtureLoader.DemoProfileId);

        var (_, custom) = await Build(app, Request("backend-go-kafka"), master);
        var (_, stored) = await app.InScopeAsync(sp => ((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!)
            .BuildCvAsync(Request("backend-go-kafka")));

        Assert.Equal(stored.MatchScore.Overall, custom.MatchScore.Overall);
        Assert.Equal(stored.GeneratedCV.Skills, custom.GeneratedCV.Skills);
    }

    [Fact]
    public async Task Invalid_custom_profile_is_rejected_with_the_field_paths()
    {
        using var app = new ApiApp();
        var master = FixtureLoader.LoadProfile("cv-master");
        master.Header.Email = "nope";
        master.Experience[0].StartDate = "someday";

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => Build(app, Request("devops"), master));

        Assert.Contains("header.email", ex.Message);
        Assert.Contains("experience[0].startDate", ex.Message);
    }

    [Fact]
    public async Task Null_custom_profile_is_rejected()
    {
        using var app = new ApiApp();

        await Assert.ThrowsAsync<ArgumentException>(() => Build(app, Request("devops"), null!));
    }
}

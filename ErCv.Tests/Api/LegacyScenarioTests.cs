using System.Net;
using System.Text.Json;
using ErCv.Tests.Fixtures;

namespace ErCv.Tests.Api;

/// <summary>
/// TEST-09: TC-01 to TC-07 of the retired Node runner (testing/test-runner-2.0.1.js), end to end through the API
/// with the profile they were written for (the senior sample, Alex Rivera). The job descriptions are the runner's,
/// verbatim, in Fixtures/jds/legacy.
/// Three match floors are lower than the runner's (written against the 2.0.1 parser): since MATCH-07 the dictionary
/// also recognizes "distributed systems", "gitops" and "devops", which this profile does not list as skills, so the
/// share of matched skills is honestly lower. Every other assertion is the runner's.
/// </summary>
[Collection("Documents")]
[Trait("Category", "Api")]
public class LegacyScenarioTests
{
    private const string SeniorSampleProfile = "cv-master-senior";

    // Runner: 75 for TC-01 and TC-06 (now 72 and 70), 70 for TC-05 (now 69). See the summary.
    private const int BackendFloor = 70;
    private const int AliasesFloor = 65;

    private static async Task<JsonElement> GenerateAsync(
        string testCase, string template = "Harvard", string[]? formats = null, bool twoPages = false)
    {
        using var app = new ApiApp();
        var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(
            jd: FixtureLoader.LoadJd($"legacy/{testCase}"),
            formats: formats ?? new[] { "pdf" },
            template: template,
            twoPages: twoPages,
            profileId: SeniorSampleProfile));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ApiApp.JsonAsync(response);
    }

    private static int Overall(JsonElement r) => r.GetProperty("matchScore").GetProperty("overall").GetInt32();

    private static bool HasLowMatchWarning(JsonElement r) =>
        r.GetProperty("matchScore").TryGetProperty("lowMatchWarning", out var w) && w.ValueKind == JsonValueKind.String;

    private static string Summary(JsonElement r) => r.GetProperty("generatedCV").GetProperty("summary").GetString() ?? "";

    private static string[] Companies(JsonElement r) => r.GetProperty("generatedCV").GetProperty("experience")
        .EnumerateArray().Select(e => e.GetProperty("company").GetString() ?? "").ToArray();

    private static string[] Strings(JsonElement r, string section, string list) =>
        r.GetProperty(section).GetProperty(list).EnumerateArray().Select(s => s.GetString() ?? "").ToArray();

    private static string[] RequiredMissing(JsonElement r) => Strings(r, "missingSkills", "requiredMissing");

    // The runner's notInArray: no entry contains the word, ignoring case
    private static void NoneContains(IEnumerable<string> list, params string[] words)
    {
        foreach (var word in words)
            Assert.DoesNotContain(list, item => item.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TC01_perfect_match_senior_backend_go_kafka()
    {
        var r = await GenerateAsync("tc-01");

        Assert.True(Overall(r) >= BackendFloor, $"match {Overall(r)}");
        Assert.False(HasLowMatchWarning(r));
        Assert.True(r.GetProperty("matchScore").GetProperty("seniorityMatch").GetBoolean());
        Assert.Equal("Stripe", Companies(r)[0]);
        Assert.Contains("Senior", Summary(r));
        Assert.DoesNotContain("Junior", Summary(r));
        Assert.DoesNotContain(RequiredMissing(r), s => s.Equals("go", StringComparison.OrdinalIgnoreCase));
        NoneContains(RequiredMissing(r), "kafka", "experience", "strong");
        Assert.True(r.GetProperty("atsScore").GetProperty("overall").GetInt32() >= 70);
    }

    [Fact]
    public async Task TC02_good_match_frontend_react_typescript_on_modern()
    {
        var r = await GenerateAsync("tc-02", template: "Modern");

        Assert.True(Overall(r) >= 58, $"match {Overall(r)}");
        Assert.Equal("Airbnb", Companies(r)[0]);
        Assert.True(
            Summary(r).Contains("frontend", StringComparison.OrdinalIgnoreCase)
            || Summary(r).Contains("full stack", StringComparison.OrdinalIgnoreCase), Summary(r));
        Assert.DoesNotContain("backend", Summary(r), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Junior", Summary(r));
        NoneContains(RequiredMissing(r), "react", "typescript", "familiarity", "nice");
    }

    [Fact]
    public async Task TC03_partial_match_devops_platform()
    {
        var r = await GenerateAsync("tc-03", formats: new[] { "pdf", "docx" });

        Assert.InRange(Overall(r), 45, 75);
        Assert.False(HasLowMatchWarning(r));
        Assert.Equal("Stripe", Companies(r)[0]);
        NoneContains(RequiredMissing(r), "cost", "monitor", "manage");
    }

    [Fact]
    public async Task TC04_poor_match_ios_gets_the_low_match_warning_and_still_a_cv()
    {
        var r = await GenerateAsync("tc-04");

        Assert.True(Overall(r) <= 40, $"match {Overall(r)}");
        Assert.True(HasLowMatchWarning(r));
        NoneContains(RequiredMissing(r), "baas", "backend");
        Assert.NotEmpty(Companies(r));
    }

    [Fact]
    public async Task TC05_aliases_resolve_nodejs_reactjs_postgres_k8s()
    {
        var r = await GenerateAsync("tc-05");

        Assert.True(Overall(r) >= AliasesFloor, $"match {Overall(r)}");
        NoneContains(RequiredMissing(r), "nodejs", "reactjs", "postgres", "k8s", "familiarity");
        NoneContains(Strings(r, "keywordAnalysis", "suggestions"), "nodejs", "reactjs");
    }

    [Fact]
    public async Task TC06_two_page_mode_keeps_four_roles_and_a_project()
    {
        var r = await GenerateAsync("tc-06", twoPages: true);

        Assert.Equal(4, Companies(r).Length);
        Assert.NotEmpty(r.GetProperty("generatedCV").GetProperty("projects").EnumerateArray());
        Assert.True(Overall(r) >= BackendFloor, $"match {Overall(r)}");
    }

    [Fact]
    public async Task TC07_a_junior_post_does_not_make_the_senior_candidate_junior()
    {
        var r = await GenerateAsync("tc-07", template: "Minimalist");

        Assert.False(r.GetProperty("matchScore").GetProperty("seniorityMatch").GetBoolean());
        Assert.DoesNotContain("Junior Developer", Summary(r));
        Assert.Contains("Senior", Summary(r));
        NoneContains(RequiredMissing(r), "basic", "understanding", "write");
    }
}

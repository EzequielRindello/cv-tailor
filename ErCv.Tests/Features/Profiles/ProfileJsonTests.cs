using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Profiles;

namespace ErCv.Tests.Features.Profiles;

[Trait("Category", "Unit")]
public class ProfileJsonTests
{
    [Fact]
    public void Template_is_a_valid_profile()
    {
        var (profile, issues) = ProfileJson.Parse(ProfileJson.Template());

        Assert.NotNull(profile);
        Assert.Empty(issues);
    }

    [Theory]
    [InlineData("demo")]
    [InlineData("cv-master")]
    [InlineData("cv-master-senior")]
    public void Export_then_import_round_trips(string id)
    {
        var original = FixtureLoader.LoadProfile(id);

        var (imported, issues) = ProfileJson.Parse(ProfileJson.Serialize(original));

        Assert.Empty(issues);
        Assert.Equal(ProfileJson.Serialize(original), ProfileJson.Serialize(imported!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{ not json")]
    [InlineData("[1, 2]")]
    public void Unreadable_input_returns_a_file_issue_and_no_profile(string json)
    {
        var (profile, issues) = ProfileJson.Parse(json);

        Assert.Null(profile);
        Assert.Equal("file", Assert.Single(issues).Path);
    }

    [Fact]
    public void Valid_json_with_bad_fields_returns_field_level_issues()
    {
        var (profile, issues) = ProfileJson.Parse("""{ "header": { "fullName": "", "email": "nope" } }""");

        Assert.NotNull(profile);
        Assert.Contains(issues, i => i.Path == "header.fullName");
        Assert.Contains(issues, i => i.Path == "header.email");
    }

    [Fact]
    public void Keys_are_case_insensitive_and_comments_are_allowed()
    {
        var (profile, issues) = ProfileJson.Parse("""
            { // my profile
              "Header": { "FullName": "Ada", "Email": "ada@example.com" },
              "Experience": [ { "Role": "Eng", "Company": "AE", "StartDate": "Jan 2020",
                                "Bullets": ["Built a thing"], "Skills": ["C#"] } ] }
            """);

        Assert.Empty(issues);
        Assert.Equal("Ada", profile!.Header.FullName);
    }
}

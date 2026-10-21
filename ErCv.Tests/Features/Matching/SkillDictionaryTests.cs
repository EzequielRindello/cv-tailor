using ErCv.Web.Features.Matching;

namespace ErCv.Tests.Features.Matching;

/// <summary>MATCH-07: Data/skills.json is well formed, and every alias and text form does what the file says.</summary>
[Trait("Category", "Unit")]
public class SkillDictionaryTests
{
    private static readonly string[] CloudPlatforms = { "aws", "azure", "gcp" };

    private readonly JobDescriptionParser _parser = new();

    public static TheoryData<string, string> Aliases()
    {
        var data = new TheoryData<string, string>();
        foreach (var skill in SkillDictionary.All)
            foreach (var alias in skill.Aliases) data.Add(alias, skill.Name);
        return data;
    }

    public static TheoryData<string, string> TextForms()
    {
        var data = new TheoryData<string, string>();
        foreach (var skill in SkillDictionary.All)
            foreach (var form in skill.TextForms) data.Add(form, skill.Name);
        return data;
    }

    [Fact]
    public void Dictionary_loads_with_lowercase_unique_names()
    {
        var names = SkillDictionary.All.SelectMany(s => s.Aliases.Prepend(s.Name)).ToList();

        Assert.True(SkillDictionary.All.Count > 400);
        Assert.All(names, n => Assert.Equal(n.Trim().ToLowerInvariant(), n));
        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void A_text_form_belongs_to_one_skill_and_exceptions_are_lowercase()
    {
        var forms = SkillDictionary.All.SelectMany(s => s.TextForms).ToList();

        Assert.Equal(forms.Count, forms.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(SkillDictionary.All.SelectMany(s => s.Except), e => Assert.Equal(e.ToLowerInvariant(), e));
    }

    [Theory]
    [MemberData(nameof(Aliases))]
    public void Every_alias_normalizes_to_its_skill(string alias, string name)
    {
        Assert.Equal(name, SkillNormalizer.Normalize(alias));
        Assert.Equal(name, SkillNormalizer.Normalize(alias.ToUpperInvariant()));
        Assert.Equal(name, SkillNormalizer.Normalize(name));
        Assert.True(SkillNormalizer.FuzzyMatch(alias, name));
    }

    [Theory]
    [MemberData(nameof(TextForms))]
    public void Every_text_form_is_found_in_a_job_description(string form, string name)
    {
        Assert.Contains(name, _parser.Parse($"Our stack: {form} and more.").TechnicalSkills);
    }

    [Fact]
    public void A_skill_with_no_text_forms_is_only_a_profile_name()
    {
        var profileOnly = SkillDictionary.All.Where(s => s.TextForms.Count == 0).ToList();

        Assert.Contains(profileOnly, s => s.Name == "mvp");
        Assert.All(profileOnly, s => Assert.All(s.Aliases.Prepend(s.Name), n =>
            Assert.DoesNotContain(s.Name, _parser.Parse($"Our stack: {n} and more.").TechnicalSkills)));
    }

    // "AWS Lambda" in a profile must still count for a job that asks for AWS, so no alias drops the platform
    [Fact]
    public void An_alias_never_drops_the_cloud_platform_it_names()
    {
        foreach (var alias in SkillDictionary.All.SelectMany(s => s.Aliases))
            foreach (var platform in CloudPlatforms.Where(p => SkillNormalizer.ContainsSkill(alias, p)))
                Assert.True(SkillNormalizer.FuzzyMatch(platform, alias), $"{alias} loses {platform}");
    }

    [Theory]
    [InlineData("golang", "go")]
    [InlineData("Next", "next.js")]
    [InlineData("py", "python")]
    [InlineData("Rails", "ruby on rails")]
    [InlineData("Model Context Protocol", "mcp")]
    public void Find_accepts_a_name_or_alias_in_any_case(string nameOrAlias, string name)
    {
        Assert.Equal(name, SkillDictionary.Find(nameOrAlias)?.Name);
    }

    [Fact]
    public void Find_returns_null_for_an_unknown_skill()
    {
        Assert.Null(SkillDictionary.Find("blazor-ish"));
        Assert.Null(SkillDictionary.FindByTextForm("professional"));
    }
}

using ErCv.Web.Features.Analysis;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Analysis;

[Trait("Category", "Unit")]
public class KeywordAnalyzerTests
{
    private readonly KeywordAnalyzer _analyzer = new();

    [Fact]
    public void Tech_words_are_classified_as_technical_with_presence_flag()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "Built services in python" }) }, skills: new[] { "python" });

        var r = _analyzer.Analyze("We need python and docker experience with python", cv);

        var python = r.TechnicalKeywords.Single(k => k.Word == "python");
        var docker = r.TechnicalKeywords.Single(k => k.Word == "docker");
        Assert.Equal(2, python.Frequency);
        Assert.True(python.PresentInCV);
        Assert.False(docker.PresentInCV);
    }

    [Fact]
    public void Soft_skill_words_are_classified_as_soft_skills()
    {
        var cv = Generated();

        var r = _analyzer.Analyze("Strong leadership and communication", cv);

        Assert.Contains(r.SoftSkillKeywords, k => k.Word == "leadership");
        Assert.Contains(r.SoftSkillKeywords, k => k.Word == "communication");
    }

    [Fact]
    public void Other_long_non_stop_words_land_in_industry()
    {
        var r = _analyzer.Analyze("Payments logistics marketplace", Generated());

        Assert.Contains(r.IndustryKeywords, k => k.Word == "payments");
    }

    [Fact]
    public void Stop_words_and_short_words_are_dropped()
    {
        var r = _analyzer.Analyze("the and for you go to it", Generated());

        Assert.Empty(r.TechnicalKeywords);
        Assert.Empty(r.SoftSkillKeywords);
        Assert.Empty(r.IndustryKeywords);
    }

    [Fact]
    public void Density_is_ratio_of_present_relevant_keywords_rounded_to_two_decimals()
    {
        var cv = Generated(skills: new[] { "python" });

        var r = _analyzer.Analyze("python docker kafka", cv);

        Assert.Equal(0.33, r.KeywordDensity);
    }

    [Fact]
    public void Empty_jd_has_zero_density_and_no_suggestions()
    {
        var r = _analyzer.Analyze("", Generated());

        Assert.Equal(0, r.KeywordDensity);
        Assert.Empty(r.Suggestions);
    }

    [Fact]
    public void Missing_tech_keywords_produce_suggestions_capped_at_5()
    {
        var jd = "python java ruby rust kotlin swift scala elixir";

        var r = _analyzer.Analyze(jd, Generated());

        Assert.Equal(5, r.Suggestions.Count);
    }

    [Fact]
    public void MissingFromCV_only_lists_tech_or_soft_words_not_in_cv()
    {
        var cv = Generated(skills: new[] { "python" });

        var r = _analyzer.Analyze("python docker payments leadership", cv);

        Assert.Contains("docker", r.MissingFromCV);
        Assert.Contains("leadership", r.MissingFromCV);
        Assert.DoesNotContain("python", r.MissingFromCV);
        Assert.DoesNotContain("payments", r.MissingFromCV);
    }
}

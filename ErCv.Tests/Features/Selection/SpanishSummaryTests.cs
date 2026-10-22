using ErCv.Web.Constants;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Selection;

// EDIT-04: the summary phrasing in Spanish. The words come from the profile and the job, only the phrasing changes.
[Trait("Category", "Unit")]
public class SpanishSummaryTests
{
    private readonly SummaryGenerator _gen = new();

    private string Summary(CVMaster cv, JobRequirements req, params string[] topSkills) =>
        _gen.GenerateSummary(cv, req, cv.Experience, topSkills.ToList(), ApiConstants.LanguageSpanish);

    [Fact]
    public void States_title_years_and_skills_in_spanish()
    {
        var cv = Master(new[] { Exp(role: "Senior Backend Engineer", start: $"Jan {Year(6)}") });

        var s = Summary(cv, Req(skills: new[] { "go", "kafka", "postgresql" }), "Go", "Kafka", "PostgreSQL");

        Assert.StartsWith("Senior Backend Developer con 6+ años de experiencia, con foco en Go, Kafka y PostgreSQL", s);
        Assert.EndsWith(".", s);
        Assert.DoesNotContain(" with ", s);
    }

    [Theory]
    [InlineData("Istio", "Go e Istio")]
    [InlineData("Hibernate", "Go e Hibernate")]
    [InlineData("Hierarchy", "Go y Hierarchy")]
    [InlineData("Kafka", "Go y Kafka")]
    public void Uses_e_before_an_i_sound(string second, string expected)
    {
        var cv = Master(new[] { Exp(role: "Software Engineer", start: $"Jan {Year(3)}") });

        var s = Summary(cv, Req(skills: new[] { "go", second.ToLowerInvariant() }), "Go", second);

        Assert.Contains(expected, s);
    }

    [Fact]
    public void Translates_soft_skills_and_leaves_out_the_quoted_achievement()
    {
        var cv = Master(new[]
        {
            Exp(role: "Software Engineer", start: $"Jan {Year(4)}",
                bullets: new[] { "Led the team while reducing build times by 45%", "Mentored two engineers" })
        });

        var s = Summary(cv, Req(skills: new[] { "go" }, keywords: new[] { "leadership" }), "Go");

        Assert.Contains("Sólidas habilidades de", s);
        Assert.Contains("trabajo en equipo", s);
        Assert.DoesNotContain("reducing", s);
        Assert.DoesNotContain("Proven track record", s);
    }

    [Fact]
    public void English_stays_the_default()
    {
        var cv = Master(new[] { Exp(role: "Software Engineer", start: $"Jan {Year(4)}") });

        var s = _gen.GenerateSummary(cv, Req(skills: new[] { "go" }), cv.Experience, new List<string> { "Go" });

        Assert.Contains("with 4+ years of experience", s);
    }
}

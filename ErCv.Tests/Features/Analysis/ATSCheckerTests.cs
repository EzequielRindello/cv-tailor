using System.Globalization;
using ErCv.Web.Features.Analysis;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Analysis;

[Trait("Category", "Unit")]
public class ATSCheckerTests
{
    private readonly ATSChecker _checker = new();

    private const string GoodBullet = "Reduced API latency by 40% for 2M users across all regions using python";

    private static GeneratedCV Healthy() =>
        Generated(new[] { Exp(bullets: new[] { GoodBullet }, skills: new[] { "python" }) }, skills: new[] { "python" });

    private static JobRequirements PythonJd() => Req(skills: new[] { "python" });

    [Fact]
    public void Healthy_cv_scores_100_green_with_optimized_message()
    {
        var r = _checker.Check(Healthy(), PythonJd(), CVTemplate.Harvard);

        Assert.Equal(100, r.Overall);
        Assert.Equal("green", r.ColorClass);
        Assert.Empty(r.Issues);
        Assert.Contains("fully optimized", r.Recommendations.Single());
    }

    [Fact]
    public void Modern_template_costs_15_points()
    {
        var r = _checker.Check(Healthy(), PythonJd(), CVTemplate.Modern);

        Assert.Equal(85, r.Overall);
        Assert.Equal("green", r.ColorClass);
        Assert.Contains(r.Issues, i => i.Severity == "Medium" && i.Issue.Contains("Modern"));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    [InlineData("a@b")]
    public void Invalid_email_costs_5_points(string email)
    {
        var cv = Healthy();
        cv.Header.Email = email;

        Assert.Equal(95, _checker.Check(cv, PythonJd(), CVTemplate.Harvard).Overall);
    }

    [Fact]
    public void Short_phone_costs_5_points()
    {
        var cv = Healthy();
        cv.Header.Phone = "123";

        var r = _checker.Check(cv, PythonJd(), CVTemplate.Harvard);

        Assert.Equal(95, r.Overall);
        Assert.Contains(r.Issues, i => i.Severity == "Low");
    }

    [Fact]
    public void Missing_sections_cost_10_points_and_are_high_severity()
    {
        var cv = Healthy();
        cv.Education.Clear();

        var r = _checker.Check(cv, PythonJd(), CVTemplate.Harvard);

        Assert.Equal(90, r.Overall);
        Assert.Contains(r.Issues, i => i.Severity == "High");
    }

    [Theory]
    [InlineData(new[] { "python", "go", "rust" }, 20, "High")]          // 1/3 = 33% < 40%
    [InlineData(new[] { "python", "go" }, 10, "Medium")]                // 1/2 = 50% < 60%
    [InlineData(new[] { "python" }, 0, null)]                           // 100%
    public void Keyword_density_penalty_by_threshold(string[] jdSkills, int penalty, string? severity)
    {
        var r = _checker.Check(Healthy(), Req(skills: jdSkills), CVTemplate.Harvard);

        Assert.Equal(100 - penalty, r.Overall);
        if (severity != null)
            Assert.Contains(r.Issues, i => i.Severity == severity && i.Issue.Contains("keyword density"));
    }

    [Fact]
    public void Short_bullets_are_flagged()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "Too short 50%", GoodBullet }, skills: new[] { "python" }) },
            skills: new[] { "python" });

        var r = _checker.Check(cv, PythonJd(), CVTemplate.Harvard);

        Assert.Equal(95, r.Overall);
        Assert.Contains(r.Issues, i => i.Issue.StartsWith("1 bullet point(s)"));
    }

    [Fact]
    public void No_metrics_costs_5_and_low_ratio_costs_2()
    {
        var none = Generated(new[] { Exp(bullets: new[] { "Maintained internal tooling used by the whole engineering org python" }) },
            skills: new[] { "python" });
        var lowRatio = Generated(new[]
        {
            Exp(bullets: new[]
            {
                GoodBullet,
                "Maintained internal tooling used by the whole engineering org python",
                "Maintained other internal tooling used by the whole engineering org python"
            })
        }, skills: new[] { "python" });

        Assert.Equal(95, _checker.Check(none, PythonJd(), CVTemplate.Harvard).Overall);
        Assert.Equal(98, _checker.Check(lowRatio, PythonJd(), CVTemplate.Harvard).Overall);
    }

    [Fact]
    public void Issues_are_ordered_by_severity_descending()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "short" }) });
        cv.Header.Email = "bad";
        cv.Education.Clear();

        var r = _checker.Check(cv, Req(skills: new[] { "python" }), CVTemplate.Modern);

        var weights = r.Issues.Select(i => i.Severity switch { "Critical" => 4, "High" => 3, "Medium" => 2, _ => 1 }).ToList();
        Assert.Equal(weights.OrderByDescending(w => w), weights);
    }

    [Fact]
    public void Score_never_drops_below_zero_and_is_red_when_low()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "x" }) }, email: "bad", phone: "1", education: false);

        var r = _checker.Check(cv, Req(skills: new[] { "python", "go", "rust" }), CVTemplate.Modern);

        Assert.InRange(r.Overall, 0, 100);
        Assert.Equal("red", r.ColorClass);
    }

    [Fact]
    public void Recommendations_list_at_most_four_issues_plus_header()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "x" }) }, email: "bad", phone: "1", education: false);

        var r = _checker.Check(cv, Req(skills: new[] { "python", "go", "rust" }), CVTemplate.Modern);

        Assert.Equal(5, r.Recommendations.Count);
        Assert.EndsWith("improvement point(s) detected:", r.Recommendations[0]);
    }

    // F2 / MATCH-02: "go" must not be counted because the CV says "good".
    [Fact]
    public void Keyword_density_does_not_count_substrings()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "Good results delivered to the happy customers 50%" }) },
            skills: new[] { "python" });

        var r = _checker.Check(cv, Req(skills: new[] { "go" }), CVTemplate.Harvard);

        Assert.Contains(r.Issues, i => i.Issue.Contains("keyword density"));
    }

    [Fact]
    public void Keyword_density_counts_an_alias_of_the_skill()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "Built services in Golang serving 2M users every single day" }) },
            skills: new[] { "golang" });

        var r = _checker.Check(cv, Req(skills: new[] { "go" }), CVTemplate.Harvard);

        Assert.DoesNotContain(r.Issues, i => i.Issue.Contains("keyword density"));
    }

    // F28: the server's culture used to decide the text ("12%" on Windows, "12 %" on Linux, where production runs)
    [Theory]
    [InlineData("")]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    public void Keyword_density_issue_prints_a_whole_percent_whatever_the_culture(string culture)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        var cv = Generated(new[] { Exp(bullets: new[] { "Built services in Go serving 2M users every single day" }) },
            skills: new[] { "go" });

        // 1 of 8 skills present: 12.5%, rounded half to even like the "P0" format it replaced
        var r = _checker.Check(cv, Req(skills: new[] { "go", "rust", "kafka", "redis", "java", "scala", "elixir", "haskell" }),
            CVTemplate.Harvard);

        Assert.Contains(r.Issues, i => i.Issue == "Low keyword density (12%): ATS may reject the CV");
    }
}

using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Models;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class JdHighlightCardTests : BunitContext
{
    private static JdHighlight Data(params (string Text, string? Skill)[] segments) => new()
    {
        Segments = segments.Select(s => new JdSegment { Text = s.Text, Skill = s.Skill }).ToList(),
        Skills = new List<JdSkillInfo>
        {
            new()
            {
                Skill = "go", InProfile = true,
                Locations = new List<JdSkillLocation>
                {
                    new() { Section = "Experience", Label = "Engineer, Acme", Excerpt = "Built Go services" }
                }
            },
            new() { Skill = "kafka", InProfile = false },
            new() { Skill = "sql", InProfile = true }
        }
    };

    private IRenderedComponent<JdHighlightCard> Render(JdHighlight data) =>
        Render<JdHighlightCard>(p => p.Add(c => c.Data, data));

    [Fact]
    public void Matched_and_missing_skills_get_their_own_style_and_label()
    {
        var cut = Render(Data(("We need ", null), ("Go", "go"), (" and ", null), ("Kafka", "kafka")));

        var go = cut.Find("button.jd-term.matched");
        var kafka = cut.Find("button.jd-term.missing");

        Assert.Equal("Go, in your profile", go.GetAttribute("aria-label"));
        Assert.Equal("Kafka, not in your profile", kafka.GetAttribute("aria-label"));
    }

    [Fact]
    public void Job_description_text_is_encoded_and_never_becomes_markup()
    {
        var cut = Render(Data(("<script>alert('x')</script> ", null), ("Go", "go"), (" <img src=x onerror=alert(1)>", null)));

        Assert.Empty(cut.FindAll("script"));
        Assert.Empty(cut.FindAll("img"));
        Assert.Contains("&lt;script&gt;", cut.Find(".jd-text").InnerHtml);
        Assert.Contains("&lt;img", cut.Find(".jd-text").InnerHtml);
    }

    [Fact]
    public void Selecting_a_skill_lists_where_it_appears_and_selecting_again_closes_it()
    {
        var cut = Render(Data(("Go", "go")));

        cut.Find("button.jd-term").Click();

        Assert.Equal("true", cut.Find("button.jd-term").GetAttribute("aria-pressed"));
        Assert.Contains("Engineer, Acme", cut.Find(".jd-locations").TextContent);
        Assert.Contains("Built Go services", cut.Find(".jd-locations").TextContent);

        cut.Find("button.jd-term").Click();

        Assert.Empty(cut.FindAll(".jd-locations"));
        Assert.Equal("false", cut.Find("button.jd-term").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void A_missing_skill_explains_that_nothing_is_invented()
    {
        var cut = Render(Data(("Kafka", "kafka")));

        cut.Find("button.jd-term").Click();

        Assert.Contains("never adds skills you do not have", cut.Find(".jd-detail").TextContent);
    }

    [Fact]
    public void A_skill_in_the_profile_but_not_in_the_cv_says_it_was_left_out()
    {
        var cut = Render(Data(("SQL", "sql")));

        cut.Find("button.jd-term").Click();

        Assert.Contains("did not include it", cut.Find(".jd-detail").TextContent);
    }

    [Fact]
    public void Every_occurrence_is_a_button_and_selecting_one_marks_all_of_that_skill()
    {
        var cut = Render(Data(("Go", "go"), (" then ", null), ("go", "go"), (" and ", null), ("Kafka", "kafka")));

        cut.FindAll("button.jd-term")[0].Click();

        var pressed = cut.FindAll("button.jd-term").Select(b => b.GetAttribute("aria-pressed")).ToArray();
        Assert.Equal(new[] { "true", "true", "false" }, pressed);
    }

    [Fact]
    public void With_no_job_description_an_empty_state_is_shown()
    {
        var cut = Render(new JdHighlight());

        Assert.Contains("no job description", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(cut.FindAll("button.jd-term"));
    }
}

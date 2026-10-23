using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using ErCv.Tests.Support;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Components;

// EDIT-07: the editor offers what the CV left out, from the profile
[Trait("Category", "Component")]
public class CvEditorAddBackTests : BunitContext
{
    private int _changes;

    public CvEditorAddBackTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    private static CVMaster Profile()
    {
        var profile = Master(new[]
        {
            Exp(role: "Backend Engineer", bullets: new[] { "Built the billing API", "Cut latency by 40%" }),
            Exp(role: "Intern", company: "Old", start: "Jan 2015", end: "Dec 2015", bullets: new[] { "Fixed bugs", "Wrote tests" })
        }, languages: new[] { "Go", "Rust" });
        profile.Projects = new List<Project> { new() { Name = "Ledger" }, new() { Name = "CLI" } };
        return profile;
    }

    private static CvDraft Draft(bool withProfile = true)
    {
        var cv = Build.Generated(new[] { Exp(role: "Backend Engineer", bullets: new[] { "Built the billing API" }) }, skills: new[] { "Go" });
        cv.Projects = new List<Project> { new() { Name = "Ledger" } };
        return new CvDraft(cv, DocumentConstants.HarvardSectionOrder, withProfile ? Profile() : null);
    }

    private IRenderedComponent<CvEditor> Render(CvDraft draft, string? pageWarning = null) =>
        Render<CvEditor>(p => p
            .Add(c => c.Draft, draft)
            .Add(c => c.PageWarning, pageWarning)
            .Add(c => c.OnChanged, () => _changes++));

    [Fact]
    public void Nothing_is_offered_without_a_profile()
    {
        var cut = Render(Draft(withProfile: false));

        Assert.Empty(cut.FindAll(".cv-editor-leftout"));
    }

    [Fact]
    public void What_the_cv_left_out_is_listed_for_each_part()
    {
        var cut = Render(Draft());

        Assert.Contains("More from your profile (1)", cut.Find("#editor-leftout-0").TextContent);
        Assert.Contains("Other experience from your profile (1)", cut.Find("#editor-leftout-experiences").TextContent);
        Assert.Contains("Rust", cut.Find("#editor-leftout-skills").TextContent);
        Assert.Contains("CLI", cut.Find("#editor-leftout-projects").TextContent);
    }

    [Fact]
    public void Adding_a_bullet_shows_it_as_added_and_notifies()
    {
        var draft = Draft();
        var cut = Render(draft);

        cut.Find("#editor-leftout-0 button").Click();

        Assert.Equal(1, _changes);
        Assert.Equal(new[] { "Built the billing API", "Cut latency by 40%" }, draft.Build().Experience[0].Bullets);
        Assert.Contains("Added", cut.Find("#bullet-text-0-1").TextContent);
    }

    [Fact]
    public void Adding_a_skill_and_a_project_puts_them_in_the_cv()
    {
        var draft = Draft();
        var cut = Render(draft);

        cut.Find("#editor-leftout-skills button").Click();
        cut.Find("#editor-leftout-projects button").Click();

        Assert.Equal(new[] { "Go", "Rust" }, draft.Build().Skills);
        Assert.Equal(new[] { "Ledger", "CLI" }, draft.Build().Projects.Select(p => p.Name));
        Assert.Equal(2, _changes);
    }

    [Fact]
    public void An_added_experience_can_be_removed_again()
    {
        var draft = Draft();
        var cut = Render(draft);

        cut.Find("#editor-leftout-experiences button").Click();
        Assert.Equal(2, draft.Build().Experience.Count);

        cut.Find("#editor-remove-exp-1").Click();

        Assert.Single(draft.Build().Experience);
    }

    [Fact]
    public void The_page_warning_is_a_status_message_and_null_shows_nothing()
    {
        Assert.Empty(Render(Draft()).FindAll("#editor-page-warning"));

        var warning = Render(Draft(), "The CV now has 2 pages, over the 1-page limit.").Find("#editor-page-warning");

        Assert.Equal("status", warning.GetAttribute("role"));
        Assert.Contains("2 pages", warning.TextContent);
    }
}

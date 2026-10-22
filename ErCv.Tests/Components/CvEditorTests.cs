using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class CvEditorTests : BunitContext
{
    private readonly CvDraft _draft;
    private int _changes;

    public CvEditorTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", bullets: new[] { "Built the billing API", "Cut latency by 40%" }),
            Exp(role: "Developer", bullets: new[] { "Maintained the monolith" })
        }, skills: new[] { "Go", "Kafka" });
        cv.Summary = "Backend engineer.";
        cv.Projects = new List<Project> { new() { Name = "Ledger" } };
        _draft = new CvDraft(cv, DocumentConstants.HarvardSectionOrder);
    }

    private IRenderedComponent<CvEditor> Render(CVTemplate template = CVTemplate.Harvard) =>
        Render<CvEditor>(p => p
            .Add(c => c.Draft, _draft)
            .Add(c => c.Template, template)
            .Add(c => c.OnChanged, () => _changes++));

    private static void ClickButton(IRenderedComponent<CvEditor> cut, string text) =>
        cut.FindAll("button").Single(b => b.TextContent.Trim() == text).Click();

    [Fact]
    public void Unchecking_a_bullet_updates_the_draft_and_notifies_once()
    {
        var cut = Render();

        cut.Find("#bullet-0-1").Change(false);

        Assert.Equal(new[] { "Built the billing API" }, _draft.Build().Experience[0].Bullets);
        Assert.Equal(1, _changes);
    }

    [Fact]
    public void The_only_bullet_of_an_experience_cannot_be_unchecked()
    {
        var cut = Render();

        Assert.True(cut.Find("#bullet-1-0").HasAttribute("disabled"));
    }

    [Fact]
    public void Rewriting_a_bullet_shows_it_as_edited_and_can_be_undone()
    {
        var cut = Render();

        cut.Find("#bullet-edit-btn-0-0").Click();
        cut.Find("#bullet-edit-0-0").Input("Built and ran the billing API");
        ClickButton(cut, "Save");

        Assert.Contains("Built and ran the billing API", cut.Find("#bullet-text-0-0").TextContent);
        Assert.Contains("Edited", cut.Find("#bullet-text-0-0").TextContent);
        Assert.Single(_draft.Build().EditedBullets);

        cut.Find("[aria-label=\"Undo rewrite\"]").Click();

        Assert.DoesNotContain("Edited", cut.Find("#bullet-text-0-0").TextContent);
        Assert.Empty(_draft.Build().EditedBullets);
        Assert.Equal(2, _changes);
    }

    [Fact]
    public void An_empty_rewrite_shows_an_error_and_keeps_the_text()
    {
        var cut = Render();

        cut.Find("#bullet-edit-btn-0-0").Click();
        cut.Find("#bullet-edit-0-0").Input("   ");
        ClickButton(cut, "Save");

        Assert.Contains("cannot be empty", cut.Find("[role=alert]").TextContent);
        Assert.Equal("Built the billing API", _draft.Build().Experience[0].Bullets[0]);
        Assert.Equal(0, _changes);
    }

    [Fact]
    public void Cancel_leaves_the_bullet_as_it_was()
    {
        var cut = Render();

        cut.Find("#bullet-edit-btn-0-0").Click();
        cut.Find("#bullet-edit-0-0").Input("Something else");
        ClickButton(cut, "Cancel");

        Assert.Empty(cut.FindAll("#bullet-edit-0-0"));
        Assert.Equal("Built the billing API", _draft.Build().Experience[0].Bullets[0]);
    }

    [Fact]
    public void Moving_a_section_reorders_the_list_and_the_cv()
    {
        var cut = Render();

        cut.Find("#section-up-skills").Click();

        var names = cut.FindAll(".cv-editor-section-name").Select(n => n.TextContent).ToArray();
        Assert.Equal(new[] { "Summary", "Education", "Skills", "Experience", "Projects" }, names);
        Assert.Equal(new[] { "summary", "education", "skills", "experience", "projects" }, _draft.Build().SectionOrder);
    }

    [Fact]
    public void Summary_skills_and_projects_can_be_toggled()
    {
        var cut = Render();

        cut.Find("#editor-include-summary").Change(false);
        cut.FindAll(".cv-editor-chip")[0].Click();
        cut.Find("#editor-project-0").Change(false);

        var cv = _draft.Build();
        Assert.Equal(string.Empty, cv.Summary);
        Assert.Equal(new[] { "Kafka" }, cv.Skills);
        Assert.Empty(cv.Projects);
        Assert.Equal("false", cut.FindAll(".cv-editor-chip")[0].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Undo_all_is_enabled_only_after_a_change()
    {
        var cut = Render();
        var undoAll = () => cut.FindAll("button").Single(b => b.TextContent.Contains("Undo all changes"));

        Assert.True(undoAll().HasAttribute("disabled"));

        cut.FindAll(".cv-editor-chip")[1].Click();
        Assert.False(undoAll().HasAttribute("disabled"));

        undoAll().Click();
        Assert.False(_draft.IsChanged);
    }

    [Fact]
    public void Modern_explains_that_sections_stay_in_their_column()
    {
        Assert.Contains("within its own column", Render(CVTemplate.Modern).Markup);
        Assert.DoesNotContain("within its own column", Render().Markup);
    }
}

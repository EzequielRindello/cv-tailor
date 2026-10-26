using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Components;

// EDIT-09: the cover letter editor
[Trait("Category", "Component")]
public class CoverLetterEditorTests : BunitContext
{
    private static readonly DateTime Today = new(2026, 10, 23);
    private int _changes;
    private string? _company;

    private static CoverLetter Letter(string? bullet = null, string? company = null)
    {
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", company: "Acme", bullets: new[] { bullet ?? "Built the billing API" })
        }, skills: new[] { "Go" });
        cv.Summary = "Senior Backend Developer with 6+ years of experience.";
        return new CoverLetterGenerator().Generate(cv, Req(skills: new[] { "go" }), "Senior Backend Engineer", Today, company);
    }

    private IRenderedComponent<CoverLetterEditor> Render(CoverLetterDraft draft, string? pageWarning = null) =>
        Render<CoverLetterEditor>(p => p
            .Add(c => c.Draft, draft)
            .Add(c => c.PageWarning, pageWarning)
            .Add(c => c.OnChanged, () => _changes++)
            .Add(c => c.OnCompanyChanged, (string company) => _company = company));

    [Fact]
    public void Each_part_of_the_letter_has_a_field_with_its_text()
    {
        var draft = new CoverLetterDraft(Letter());

        var cut = Render(draft);

        Assert.Equal("Dear Hiring Manager,", cut.Find("#letter-text-0").GetAttribute("value"));
        Assert.Contains("Greeting", cut.Find("#letter-block-0").TextContent);
        Assert.Contains("About your role as Backend Engineer at Acme", cut.Find("#letter-block-2").TextContent);
        Assert.Equal(4, cut.FindAll(".letter-block").Count);
    }

    [Fact]
    public void Editing_a_part_changes_the_draft_marks_it_and_notifies()
    {
        var draft = new CoverLetterDraft(Letter());
        var cut = Render(draft);

        cut.Find("#letter-text-1").Change("My own opening.");

        Assert.Equal(1, _changes);
        Assert.Equal("My own opening.", draft.Build().Paragraphs[0].Text);
        Assert.Contains("Edited", cut.Find("#letter-block-1").TextContent);
    }

    [Fact]
    public void An_empty_part_is_refused_with_a_message()
    {
        var draft = new CoverLetterDraft(Letter());
        var cut = Render(draft);

        cut.Find("#letter-text-1").Change("   ");

        Assert.Equal(0, _changes);
        Assert.Contains("cannot be empty", cut.Find("#letter-block-1 .field-error").TextContent);
        Assert.False(draft.IsChanged);
    }

    [Fact]
    public void A_part_the_cv_made_outdated_says_so_and_can_take_the_new_text()
    {
        var draft = new CoverLetterDraft(Letter());
        draft.Edit(draft.Blocks[2].Key, "I led the billing team.");
        draft.Update(Letter("Rewrote the billing API"));
        var cut = Render(draft);

        Assert.Contains("The CV changed since you edited this", cut.Find("#letter-outdated-2").TextContent);
        cut.Find("#letter-outdated-2 button").Click();

        Assert.Contains("Rewrote the billing API", draft.Build().Paragraphs[1].Text);
        Assert.Empty(cut.FindAll("#letter-outdated-2"));
        Assert.Equal(1, _changes);
    }

    [Fact]
    public void Undo_my_edit_goes_back_to_the_generated_text()
    {
        var draft = new CoverLetterDraft(Letter());
        draft.Edit(draft.Blocks[1].Key, "My own opening.");
        var cut = Render(draft);

        cut.FindAll("#letter-block-1 button").Single(b => b.TextContent.Contains("Undo my edit")).Click();

        Assert.False(draft.IsChanged);
    }

    // EDIT-10: a letter the visitor made longer than a page says so
    [Fact]
    public void The_page_warning_is_a_status_message_and_null_shows_nothing()
    {
        Assert.Empty(Render(new CoverLetterDraft(Letter())).FindAll("#letter-page-warning"));

        var warning = Render(new CoverLetterDraft(Letter()), "The letter now has 2 pages.").Find("#letter-page-warning");

        Assert.Equal("status", warning.GetAttribute("role"));
        Assert.Contains("2 pages", warning.TextContent);
    }

    [Fact]
    public void The_company_field_shows_the_company_and_reports_a_change()
    {
        var cut = Render(new CoverLetterDraft(Letter(company: "Stripe")));

        Assert.Equal("Stripe", cut.Find("#letter-company").GetAttribute("value"));
        cut.Find("#letter-company").Change("Ramp");

        Assert.Equal("Ramp", _company);
    }
}

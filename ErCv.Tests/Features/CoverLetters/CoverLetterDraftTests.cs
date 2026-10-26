using ErCv.Web.Constants;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.CoverLetters;

// EDIT-09: what the visitor wrote in the letter is theirs; the rest keeps following the CV
[Trait("Category", "Unit")]
public class CoverLetterDraftTests
{
    private static readonly DateTime Today = new(2026, 10, 23);
    private readonly CoverLetterGenerator _generator = new();

    private static GeneratedCV Cv(params string[] firstBullets)
    {
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", company: "Acme", bullets: firstBullets.Length > 0 ? firstBullets : new[] { "Built the billing API" }),
            Exp(role: "Developer", company: "Initech", start: "Mar 2018", end: "Dec 2020", bullets: new[] { "Maintained the monolith" })
        }, skills: new[] { "Go" });
        cv.Summary = "Senior Backend Developer with 6+ years of experience.";
        return cv;
    }

    private CoverLetter Letter(GeneratedCV cv, string? company = null) =>
        _generator.Generate(cv, Req(skills: new[] { "go" }), "Senior Backend Engineer", Today, company);

    private static string KeyOfFirstExperience(CoverLetter letter) => letter.Paragraphs[1].Key;

    [Fact]
    public void The_paragraphs_are_keyed_by_what_they_are_about()
    {
        var letter = Letter(Cv());

        Assert.Equal(CoverLetterConstants.OpeningKey, letter.Paragraphs[0].Key);
        Assert.Equal("experience:Backend Engineer|Acme|Jan 2020|Present", letter.Paragraphs[1].Key);
        Assert.Equal("experience:Developer|Initech|Mar 2018|Dec 2020", letter.Paragraphs[2].Key);
        Assert.Equal(CoverLetterConstants.ClosingKey, letter.Paragraphs[3].Key);
    }

    [Fact]
    public void Without_edits_the_built_letter_is_the_generated_one()
    {
        var generated = Letter(Cv());
        var draft = new CoverLetterDraft(generated);

        var built = draft.Build();

        Assert.False(draft.IsChanged);
        Assert.Equal(generated.Salutation, built.Salutation);
        Assert.Equal(generated.Paragraphs.Select(p => p.Text), built.Paragraphs.Select(p => p.Text));
        Assert.All(built.Paragraphs, p => Assert.False(p.IsEdited));
    }

    [Fact]
    public void An_edit_is_kept_word_for_word_after_the_letter_is_written_again()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        var key = CoverLetterConstants.OpeningKey;

        Assert.True(draft.Edit(key, "  My own opening.  "));
        draft.Update(Letter(Cv("Rewrote the billing API")));

        Assert.Equal("My own opening.", draft.Build().Paragraphs[0].Text);
        Assert.True(draft.Build().Paragraphs[0].IsEdited);
    }

    [Fact]
    public void A_paragraph_that_was_not_edited_follows_the_new_cv()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        draft.Edit(CoverLetterConstants.OpeningKey, "My own opening.");

        draft.Update(Letter(Cv("Rewrote the billing API")));

        Assert.Contains("Rewrote the billing API", draft.Build().Paragraphs[1].Text);
    }

    [Fact]
    public void An_edited_paragraph_is_outdated_only_when_what_the_app_would_write_changed()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        var key = KeyOfFirstExperience(Letter(Cv()));
        draft.Edit(key, "I led the billing team.");

        draft.Update(Letter(Cv()));
        Assert.DoesNotContain(draft.Blocks, b => b.IsOutdated);

        draft.Update(Letter(Cv("Rewrote the billing API")));
        Assert.Contains(draft.Blocks, b => b.Key == key && b.IsOutdated);
        Assert.Equal("I led the billing team.", draft.Blocks.Single(b => b.Key == key).Text);
    }

    [Fact]
    public void Reverting_brings_back_the_generated_text()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        var key = KeyOfFirstExperience(Letter(Cv()));
        draft.Edit(key, "I led the billing team.");
        draft.Update(Letter(Cv("Rewrote the billing API")));

        draft.Revert(key);

        Assert.Contains("Rewrote the billing API", draft.Build().Paragraphs[1].Text);
        Assert.False(draft.IsChanged);
    }

    [Fact]
    public void Empty_too_long_and_unknown_edits_are_refused()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));

        Assert.False(draft.Edit(CoverLetterConstants.OpeningKey, "   "));
        Assert.False(draft.Edit(CoverLetterConstants.OpeningKey, new string('a', ApiConstants.MaxEditedLetterBlockLength + 1)));
        Assert.False(draft.Edit("experience:Nobody|Nowhere|x|y", "Text"));
        Assert.False(draft.IsChanged);
    }

    [Fact]
    public void An_edit_equal_to_the_generated_text_is_no_edit()
    {
        var generated = Letter(Cv());
        var draft = new CoverLetterDraft(generated);

        Assert.True(draft.Edit(CoverLetterConstants.OpeningKey, generated.Paragraphs[0].Text));

        Assert.False(draft.IsChanged);
    }

    [Fact]
    public void The_greeting_can_be_edited_and_survives_a_company_change()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        draft.Edit(CoverLetterDraft.SalutationKey, "Hello team,");

        draft.Update(Letter(Cv(), "Stripe"));

        Assert.Equal("Hello team,", draft.Build().Salutation);
        Assert.Equal("Stripe", draft.Build().Company);
        Assert.Contains("help the Stripe team", draft.Build().Paragraphs[^1].Text);
    }

    [Fact]
    public void A_paragraph_about_an_experience_no_longer_written_stays_before_the_closing_and_is_marked()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        var key = draft.Blocks.First(b => b.Key.StartsWith("experience:Developer")).Key;
        draft.Edit(key, "Maintained the monolith with pride.");

        var withoutIt = Cv();
        withoutIt.Experience.RemoveAt(1);
        draft.Update(Letter(withoutIt));

        var built = draft.Build();
        Assert.Equal("Maintained the monolith with pride.", built.Paragraphs[^2].Text);
        Assert.Equal(CoverLetterConstants.ClosingKey, built.Paragraphs[^1].Key);
        Assert.True(draft.Blocks.Single(b => b.Key == key).IsOutdated);
    }

    [Fact]
    public void Edits_are_not_in_the_generated_letter_the_draft_was_given()
    {
        var generated = Letter(Cv());
        var draft = new CoverLetterDraft(generated);

        draft.Edit(CoverLetterConstants.OpeningKey, "Mine.");
        draft.Build();

        Assert.NotEqual("Mine.", generated.Paragraphs[0].Text);
        Assert.False(generated.Paragraphs[0].IsEdited);
    }

    [Fact]
    public void Plain_text_and_the_renderer_use_the_edited_text()
    {
        var draft = new CoverLetterDraft(Letter(Cv()));
        draft.Edit(CoverLetterConstants.OpeningKey, "My own opening.");

        var text = CoverLetterText.ToPlainText(draft.Build());

        Assert.Contains("My own opening.", text);
    }
}

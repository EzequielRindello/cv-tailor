using ErCv.Web.Constants;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.CoverLetters;

[Trait("Category", "Unit")]
public class CoverLetterGeneratorTests
{
    private static readonly DateTime Today = new(2026, 10, 21);
    private readonly CoverLetterGenerator _generator = new();

    private static GeneratedCV Cv(string language = ApiConstants.LanguageEnglish)
    {
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", company: "Acme", start: "Jan 2021", end: "Present",
                bullets: new[] { "Built the billing API in Go.", "Cut p99 latency by 40%", "Ran the on-call rotation" }),
            Exp(role: "Developer", company: "Initech", start: "Mar 2018", end: "Dec 2020",
                bullets: new[] { "Maintained the C# monolith" }),
            Exp(role: "Intern", company: "Globex", start: "2017", end: "2017", bullets: new[] { "Wrote tests" })
        }, skills: new[] { "Go", "Kafka", "Istio", "Figma" });
        cv.Summary = "Senior Backend Developer with 6+ years of experience, specializing in Go and Kafka. Strong leadership skills.";
        cv.Language = language;
        return cv;
    }

    private CoverLetter Letter(GeneratedCV cv, string jd = "Senior Backend Engineer (Go)\n\nWe use Go, Kafka and Istio.") =>
        _generator.Generate(cv, Req(skills: new[] { "go", "kafka", "istio" }), jd, Today);

    [Fact]
    public void Has_an_opening_two_experience_paragraphs_and_a_closing()
    {
        var letter = Letter(Cv());

        Assert.Equal(4, letter.Paragraphs.Count);
        Assert.StartsWith("I am writing to apply for the Senior Backend Engineer (Go) position.", letter.Paragraphs[0].Text);
        Assert.StartsWith("In my role as Backend Engineer at Acme (Jan 2021 – Present)", letter.Paragraphs[1].Text);
        Assert.StartsWith("Before that, as Developer at Initech (Mar 2018 – Dec 2020)", letter.Paragraphs[2].Text);
        Assert.Contains("Thank you for your time", letter.Paragraphs[3].Text);
        Assert.DoesNotContain(letter.Paragraphs, p => p.Text.Contains("Globex"));
    }

    [Fact]
    public void The_opening_quotes_the_first_sentence_of_the_summary()
    {
        var opening = Letter(Cv()).Paragraphs[0].Text;

        Assert.Contains("Senior Backend Developer with 6+ years of experience, specializing in Go and Kafka.", opening);
        Assert.DoesNotContain("Strong leadership", opening);
    }

    [Fact]
    public void Quotes_two_bullets_per_experience_verbatim_as_sentences()
    {
        var paragraph = Letter(Cv()).Paragraphs[1].Text;

        Assert.Contains("Built the billing API in Go. Cut p99 latency by 40%. ", paragraph);
        Assert.DoesNotContain("on-call", paragraph);
        Assert.DoesNotContain("..", paragraph);
    }

    [Fact]
    public void Names_only_the_skills_that_are_on_the_cv_and_in_the_post()
    {
        var closing = Letter(Cv()).Paragraphs[^1].Text;

        Assert.StartsWith("The role asks for Go, Kafka and Istio, all of which are part of my experience.", closing);
        Assert.DoesNotContain("Figma", closing);
    }

    [Fact]
    public void Without_shared_skills_the_closing_has_no_skills_sentence()
    {
        var letter = _generator.Generate(Cv(), Req(skills: new[] { "rust" }), "Rust Engineer", Today);

        Assert.StartsWith("I would welcome the chance", letter.Paragraphs[^1].Text);
    }

    [Theory]
    [InlineData("About the role\n\nWe are hiring.")]
    [InlineData("")]
    [InlineData("We are a fast growing company looking for someone who loves building great products and who also enjoys writing")]
    public void Without_a_position_in_the_first_line_the_opening_is_generic(string jd)
    {
        var letter = _generator.Generate(Cv(), Req(), jd, Today);

        Assert.Null(letter.JobTitle);
        Assert.Equal(string.Empty, letter.Subject);
        Assert.StartsWith("I am writing to apply for this position.", letter.Paragraphs[0].Text);
    }

    [Fact]
    public void A_position_becomes_the_subject_line_and_the_date_is_written_out()
    {
        var letter = Letter(Cv());

        Assert.Equal("Application: Senior Backend Engineer (Go)", letter.Subject);
        Assert.Equal("October 21, 2026", letter.Date);
        Assert.Equal("Dear Hiring Manager,", letter.Salutation);
    }

    [Fact]
    public void A_cv_with_one_experience_gets_one_experience_paragraph()
    {
        var cv = Generated(new[] { Exp(role: "Engineer", company: "Acme", start: "2020", end: "", bullets: new[] { "Shipped things" }) });

        var letter = _generator.Generate(cv, Req(), "Software Engineer", Today);

        Assert.Equal(3, letter.Paragraphs.Count);
        Assert.Contains("In my role as Engineer at Acme (2020), my work included the following: Shipped things.", letter.Paragraphs[1].Text);
    }

    [Fact]
    public void Spanish_letters_use_spanish_wording_dates_and_conjunctions()
    {
        var letter = Letter(Cv(ApiConstants.LanguageSpanish), "Ingeniero de Software Backend Senior\n\nGo, Kafka e Istio.");

        Assert.Equal("Estimado equipo de selección:", letter.Salutation);
        Assert.Equal("21 de octubre de 2026", letter.Date);
        Assert.Equal("Postulación: Ingeniero de Software Backend Senior", letter.Subject);
        Assert.StartsWith("Les escribo para postularme al puesto de Ingeniero de Software Backend Senior.", letter.Paragraphs[0].Text);
        Assert.StartsWith("En mi rol de Backend Engineer en Acme (ene. 2021 – Actualidad)", letter.Paragraphs[1].Text);
        Assert.StartsWith("El puesto pide Go, Kafka e Istio, que forman parte de mi experiencia.", letter.Paragraphs[^1].Text);
        Assert.Equal("Saludos cordiales,", letter.Closing);
    }

    [Fact]
    public void The_letter_carries_the_cv_style_and_contact()
    {
        var cv = Cv();
        cv.Accent = ApiConstants.AccentTeal;
        cv.Fonts = ApiConstants.FontsMixed;

        var letter = Letter(cv);

        Assert.Equal(ApiConstants.AccentTeal, letter.Accent);
        Assert.Equal(ApiConstants.FontsMixed, letter.Fonts);
        Assert.Equal("t@x.com", letter.Header.Email);
        Assert.NotSame(cv.Header, letter.Header);
    }

    [Fact]
    public void Plain_text_has_the_letter_in_reading_order()
    {
        var text = CoverLetterText.ToPlainText(Letter(Cv()));

        var order = new[] { "Test User", "October 21, 2026", "Application:", "Dear Hiring Manager,", "In my role as", "Before that, as", "Sincerely," };
        var cursor = 0;
        foreach (var item in order)
        {
            var at = text.IndexOf(item, cursor, StringComparison.Ordinal);
            Assert.True(at >= 0, $"'{item}' missing or out of order");
            cursor = at + item.Length;
        }
    }

    // EDIT-09: the company is a fact between two pieces of fixed wording, in the salutation and the closing
    [Fact]
    public void A_company_is_named_in_the_salutation_and_the_closing()
    {
        var letter = _generator.Generate(Cv(), Req(skills: new[] { "go" }), "Senior Backend Engineer (Go)", Today, "Stripe");

        Assert.Equal("Dear Stripe team,", letter.Salutation);
        Assert.Equal("Stripe", letter.Company);
        var closing = letter.Paragraphs[^1];
        Assert.Contains("help the Stripe team. Thank you for your time and consideration.", closing.Text);
        Assert.Contains(closing.Parts, p => p.IsFact && p.Text == "Stripe");
    }

    [Fact]
    public void Without_a_company_the_wording_stays_generic()
    {
        var letter = Letter(Cv());

        Assert.Equal("Dear Hiring Manager,", letter.Salutation);
        Assert.Null(letter.Company);
        Assert.Contains("help your team", letter.Paragraphs[^1].Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_company_is_no_company(string? company)
    {
        var letter = _generator.Generate(Cv(), Req(), "Senior Backend Engineer", Today, company);

        Assert.Null(letter.Company);
        Assert.Equal("Dear Hiring Manager,", letter.Salutation);
    }

    [Fact]
    public void The_spanish_letter_names_the_company_too()
    {
        var letter = _generator.Generate(Cv(ApiConstants.LanguageSpanish), Req(), "Ingeniero Backend Senior", Today, "Mercado Libre");

        Assert.Equal("Estimado equipo de Mercado Libre:", letter.Salutation);
        Assert.Contains("aportar al equipo de Mercado Libre. Muchas gracias", letter.Paragraphs[^1].Text);
    }

    [Fact]
    public void A_name_that_ends_with_a_period_reads_well_in_both_languages()
    {
        var english = _generator.Generate(Cv(), Req(), "Backend Engineer", Today, "Acme Inc.");
        var spanish = _generator.Generate(Cv(ApiConstants.LanguageSpanish), Req(), "Ingeniero Backend", Today, "Acme Inc.");

        Assert.Equal("Dear Acme Inc. team,", english.Salutation);
        Assert.Equal("Estimado equipo de Acme Inc.:", spanish.Salutation);
    }
}

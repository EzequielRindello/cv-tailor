using DocumentFormat.OpenXml.Packaging;
using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using ErCv.Web.Services;
using UglyToad.PdfPig;
using static ErCv.Tests.Api.ApiApp;

namespace ErCv.Tests.Api;

// EDIT-03 through the service: written on request, follows the edited CV, PDF and DOCX on download
[Collection("Documents")]
[Trait("Category", "Api")]
public class CoverLetterApiTests
{
    private static GenerateRequest Request(bool letter) => new()
    {
        JobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
        Formats = new List<string> { "docx" },
        IncludeCoverLetter = letter
    };

    private static async Task<T> WithServiceAsync<T>(Func<CVGeneratorService, Task<T>> action)
    {
        using var app = new ApiApp();
        return await app.InScopeAsync(sp => action((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!));
    }

    [Fact]
    public async Task A_letter_is_written_only_when_asked_for()
    {
        var with = await WithServiceAsync(async s => (await s.BuildCvAsync(Request(true))).Analysis);
        var without = await WithServiceAsync(async s => (await s.BuildCvAsync(Request(false))).Analysis);

        Assert.NotNull(with.CoverLetter);
        Assert.Contains("Dear Hiring Manager,", with.CoverLetterHtml);
        Assert.DoesNotContain("{{", with.CoverLetterHtml);
        Assert.Null(without.CoverLetter);
        Assert.Null(without.CoverLetterHtml);
    }

    // EDIT-09: the visitor's company wins over the post's, and it is checked at the boundary
    [Fact]
    public async Task A_company_in_the_request_is_named_in_the_letter_and_returned()
    {
        var request = Request(true);
        request.Company = "  Stripe   Payments ";

        var analysis = await WithServiceAsync(async s => (await s.BuildCvAsync(request)).Analysis);

        Assert.Equal("Stripe Payments", analysis.Company);
        Assert.Equal("Dear Stripe Payments team,", analysis.CoverLetter!.Salutation);
        Assert.Contains("Dear Stripe Payments team,", analysis.CoverLetterHtml);
    }

    [Fact]
    public async Task Without_a_company_the_one_the_post_names_is_used_or_none()
    {
        var request = Request(true);
        request.JobDescription = "Backend Engineer\n\nAbout Stripe\nWe build payments. Go and Kafka.";

        var named = await WithServiceAsync(async s => (await s.BuildCvAsync(request)).Analysis);
        var none = await WithServiceAsync(async s => (await s.BuildCvAsync(Request(true))).Analysis);

        Assert.Equal("Stripe", named.Company);
        Assert.Equal("Dear Stripe team,", named.CoverLetter!.Salutation);
        Assert.Null(none.Company);
        Assert.Equal("Dear Hiring Manager,", none.CoverLetter!.Salutation);
    }

    [Theory]
    [InlineData("Line one\nLine two")]
    [InlineData("Bell\u0007")]
    public async Task A_company_that_is_not_one_line_is_refused(string company)
    {
        var request = Request(true);
        request.Company = company;

        await WithServiceAsync(async s =>
        {
            await Assert.ThrowsAsync<ArgumentException>(() => s.BuildCvAsync(request));
            return true;
        });
    }

    [Fact]
    public async Task A_company_that_is_too_long_is_refused()
    {
        var request = Request(true);
        request.Company = new string('a', ApiConstants.MaxCompanyLength + 1);

        await WithServiceAsync(async s =>
        {
            await Assert.ThrowsAsync<ArgumentException>(() => s.BuildCvAsync(request));
            return true;
        });
    }

    // EDIT-10: a letter made of blocks at their cap runs past one page, and is printed as written (never trimmed)
    [Fact]
    public Task A_letter_edited_to_the_cap_runs_to_several_pages_and_is_not_trimmed() => WithServiceAsync(async s =>
    {
        var (_, analysis) = await s.BuildCvAsync(Request(true));
        var draft = new CoverLetterDraft(analysis.CoverLetter!);
        var words = string.Join(" ", Enumerable.Repeat("delivered", ApiConstants.MaxEditedLetterBlockLength / 10));
        foreach (var block in draft.Blocks.Where(b => b.Key != CoverLetterDraft.SalutationKey))
            Assert.True(draft.Edit(block.Key, words));

        var pdf = await s.RenderCoverLetterAsync(draft.Build(), "pdf");
        var short1 = await s.RenderCoverLetterAsync(analysis.CoverLetter!, "pdf");

        Assert.True(DocumentRenderer.CountPages(pdf) > 1);
        Assert.Equal(1, DocumentRenderer.CountPages(short1));
        return true;
    });

    [Fact]
    public Task The_letter_follows_a_rewritten_bullet() => WithServiceAsync(async s =>
    {
        var (_, analysis) = await s.BuildCvAsync(Request(true));
        var draft = new CvDraft(analysis.GeneratedCV, DocumentConstants.HarvardSectionOrder);
        draft.EditBullet(0, 0, "Rewritten in the editor for this letter");

        var (letter, html) = s.BuildCoverLetter(draft.Build(), Request(true).JobDescription);

        Assert.Contains("Rewritten in the editor for this letter", letter.Paragraphs[1].Text);
        Assert.Contains("Rewritten in the editor for this letter", html);
        return true;
    });

    [Fact]
    public Task The_letter_downloads_as_a_one_page_pdf_and_a_docx() => WithServiceAsync(async s =>
    {
        var (_, analysis) = await s.BuildCvAsync(Request(true));
        var letter = analysis.CoverLetter!;

        var pdf = await s.RenderCoverLetterAsync(letter, ApiConstants.FormatPdf);
        var docx = await s.RenderCoverLetterAsync(letter, ApiConstants.FormatDocx);

        using (var doc = PdfDocument.Open(pdf))
        {
            Assert.Equal(1, doc.NumberOfPages);
            var text = Squash(string.Join(" ", doc.GetPages().Select(p => p.Text)));
            Assert.Contains(Squash(letter.Paragraphs[1].Text), text);
        }

        using var stream = new MemoryStream(docx);
        using var word = WordprocessingDocument.Open(stream, false);
        var body = word.MainDocumentPart!.Document!.Body!.InnerText;
        Assert.Contains(letter.Salutation, body);
        Assert.All(letter.Paragraphs, p => Assert.Contains(p.Text.Trim(), body));
        return true;
    });

    [Fact]
    public async Task The_api_returns_the_letter_text_when_asked_for()
    {
        using var app = new ApiApp();

        var response = await app.PostAsync("/api/cv/generate", new
        {
            jobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
            formats = new[] { "docx" },
            includeCoverLetter = true
        });

        var json = await JsonAsync(response);
        Assert.Contains("Dear Hiring Manager,", json.GetProperty("coverLetter").GetString());
    }
}

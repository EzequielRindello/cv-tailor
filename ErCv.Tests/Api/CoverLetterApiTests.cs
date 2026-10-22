using DocumentFormat.OpenXml.Packaging;
using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
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

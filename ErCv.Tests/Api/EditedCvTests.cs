using DocumentFormat.OpenXml.Packaging;
using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using ErCv.Web.Services;
using UglyToad.PdfPig;
using static ErCv.Tests.Api.ApiApp;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Api;

// EDIT-02 through the service: the preview of an edited CV, and its documents built on download
[Collection("Documents")]
[Trait("Category", "Api")]
public class EditedCvTests
{
    private const string Rewrite = "Rewritten by the user for this job";

    private static async Task WithServiceAsync(Func<CVGeneratorService, Task> action)
    {
        using var app = new ApiApp();
        await app.InScopeAsync(async sp =>
        {
            await action((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!);
            return true;
        });
    }

    private static async Task<GeneratedCV> GeneratedAsync(CVGeneratorService service)
    {
        var (_, analysis) = await service.BuildCvAsync(new GenerateRequest
        {
            JobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
            Formats = new List<string> { "docx" }
        });
        return analysis.GeneratedCV;
    }

    private static string PdfText(byte[] pdf)
    {
        using var doc = PdfDocument.Open(pdf);
        return Squash(string.Join(" ", doc.GetPages().Select(p => p.Text)));
    }

    [Fact]
    public Task The_preview_shows_a_rewritten_bullet_and_drops_an_excluded_one() => WithServiceAsync(async service =>
    {
        var draft = new CvDraft(await GeneratedAsync(service), DocumentConstants.HarvardSectionOrder);
        var excluded = draft.Experiences[0].Bullets[1].Text;
        draft.EditBullet(0, 0, Rewrite);
        draft.ToggleBullet(0, 1);

        var html = Squash(service.RenderPreviewHtml(draft.Build(), CVTemplate.Harvard));

        Assert.Contains(Squash(Rewrite), html);
        Assert.DoesNotContain(Squash(excluded), html);
    });

    [Fact]
    public Task The_edited_documents_hold_the_edits_and_report_pdf_pages() => WithServiceAsync(async service =>
    {
        var draft = new CvDraft(await GeneratedAsync(service), DocumentConstants.HarvardSectionOrder);
        draft.EditBullet(0, 0, Rewrite);
        var cv = draft.Build();

        var (pdf, pages) = await service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "pdf");
        var (docx, docxPages) = await service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "docx");

        using (var doc = PdfDocument.Open(pdf))
            Assert.Equal(doc.NumberOfPages, pages);
        Assert.Contains(Squash(Rewrite), PdfText(pdf));

        using var stream = new MemoryStream(docx);
        using var word = WordprocessingDocument.Open(stream, false);
        Assert.Contains(Rewrite, word.MainDocumentPart!.Document!.Body!.InnerText);
        Assert.Equal(0, docxPages);
    });

    // The user's choices are printed as they are: the edited PDF is never trimmed to fit
    [Fact]
    public Task An_edited_cv_that_runs_long_is_not_trimmed() => WithServiceAsync(async service =>
    {
        var bullets = Enumerable.Range(0, 8).Select(i => $"Bullet {i} " + string.Join(" ", Enumerable.Repeat("delivered", 25))).ToArray();
        var cv = Generated(Enumerable.Range(0, 6).Select(i => Exp(role: $"Role {i}", bullets: bullets)).ToArray());

        var (pdf, pages) = await service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "pdf");

        Assert.True(pages > 1);
        var text = PdfText(pdf);
        Assert.All(cv.Experience.SelectMany(e => e.Bullets), b => Assert.Contains(Squash(b), text));
    });

    [Fact]
    public Task Bad_input_is_refused_at_the_service() => WithServiceAsync(async service =>
    {
        var cv = Generated(new[] { Exp(bullets: new[] { new string('a', ApiConstants.MaxEditedBulletLength + 1) }) });

        Assert.Throws<ArgumentException>(() => service.RenderPreviewHtml(cv, CVTemplate.Harvard));
        await Assert.ThrowsAsync<ArgumentException>(() => service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "pdf"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.RenderEditedDocumentAsync(Generated(), CVTemplate.Harvard, "html"));
    });
}

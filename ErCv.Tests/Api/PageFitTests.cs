using DocumentFormat.OpenXml.Packaging;
using ErCv.Tests.Fixtures;
using ErCv.Web.Models;
using ErCv.Web.Services;
using UglyToad.PdfPig;
using static ErCv.Tests.Api.ApiApp;

namespace ErCv.Tests.Api;

// MATCH-08 through the service: Minimalist one page is the layout the character estimate gets wrong.
[Collection("Documents")]
[Trait("Category", "Api")]
public class PageFitTests
{
    private static GenerateRequest Request(params string[] formats) => new()
    {
        JobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
        ProfileId = "cv-master",
        Template = CVTemplate.Minimalist,
        AllowTwoPages = false,
        Formats = formats.ToList()
    };

    private static Task<(Dictionary<string, byte[]> Documents, AnalysisResult Analysis)> BuildAsync(
        ApiApp app, GenerateRequest request) =>
        app.InScopeAsync(sp => ((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!).BuildCvAsync(request));

    private static List<string> Bullets(GeneratedCV cv) => cv.Experience.SelectMany(e => e.Bullets).ToList();

    private static string DocxText(byte[] docx)
    {
        using var stream = new MemoryStream(docx);
        using var doc = WordprocessingDocument.Open(stream, false);
        return doc.MainDocumentPart!.Document!.Body!.InnerText;
    }

    [Fact]
    public async Task The_pdf_fits_on_one_page_and_every_output_shows_the_fitted_cv()
    {
        using var app = new ApiApp();

        var (documents, analysis) = await BuildAsync(app, Request("pdf", "docx"));

        using (var pdf = PdfDocument.Open(documents["pdf"]))
            Assert.Equal(1, pdf.NumberOfPages);

        var bullets = Bullets(analysis.GeneratedCV);
        var docx = Squash(DocxText(documents["docx"]));
        var preview = Squash(analysis.PreviewHtml ?? "");
        Assert.All(bullets, b => Assert.Contains(Squash(b), docx));
        Assert.All(bullets, b => Assert.Contains(Squash(b), preview));

        var explained = analysis.Explanation.Experiences.SelectMany(e => e.Bullets).Where(b => b.Selected).Select(b => b.Text);
        Assert.Equal(bullets.OrderBy(b => b), explained.OrderBy(b => b));
    }

    [Fact]
    public async Task The_measured_fit_trims_what_the_estimate_alone_keeps()
    {
        using var app = new ApiApp();

        var (_, estimated) = await BuildAsync(app, Request("docx"));
        var (documents, measured) = await BuildAsync(app, Request("pdf", "docx"));

        var trimmed = Bullets(estimated.GeneratedCV).Except(Bullets(measured.GeneratedCV)).ToList();
        Assert.NotEmpty(trimmed);

        var docx = Squash(DocxText(documents["docx"]));
        Assert.All(trimmed, b => Assert.DoesNotContain(Squash(b), docx));
    }
}

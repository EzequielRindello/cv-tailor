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

    // The measured fit only ever removes: with a PDF in the request the CV keeps a subset of what the
    // estimate alone keeps, and the DOCX shows exactly that subset. (Which cases trim depends on the
    // template's layout; the trimming itself is unit tested with a fake renderer in PageConstraintHelperTests.)
    [Theory]
    [InlineData(CVTemplate.Minimalist)]
    [InlineData(CVTemplate.Executive)]
    [InlineData(CVTemplate.Compact)]
    public async Task The_measured_fit_keeps_a_subset_of_the_estimate(CVTemplate template)
    {
        using var app = new ApiApp();
        var docxOnly = Request("docx");
        var withPdf = Request("pdf", "docx");
        docxOnly.Template = withPdf.Template = template;

        var (_, estimated) = await BuildAsync(app, docxOnly);
        var (documents, measured) = await BuildAsync(app, withPdf);

        Assert.Subset(Bullets(estimated.GeneratedCV).ToHashSet(), Bullets(measured.GeneratedCV).ToHashSet());
        using (var pdf = PdfDocument.Open(documents["pdf"]))
            Assert.Equal(1, pdf.NumberOfPages);

        var docx = Squash(DocxText(documents["docx"]));
        Assert.All(Bullets(estimated.GeneratedCV).Except(Bullets(measured.GeneratedCV)), b => Assert.DoesNotContain(Squash(b), docx));
    }
}

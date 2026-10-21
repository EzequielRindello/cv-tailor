using DocumentFormat.OpenXml.Packaging;
using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Models;
using UglyToad.PdfPig;
using static ErCv.Tests.Documents.DocumentSupport;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Documents;

// EDIT-04 (both languages in every output) and UX-10 (accent and fonts)
[Collection("Documents")]
[Trait("Category", "Documents")]
public class LanguageAndStyleTests
{
    public static TheoryData<CVTemplate> Templates()
    {
        var data = new TheoryData<CVTemplate>();
        foreach (var template in Enum.GetValues<CVTemplate>()) data.Add(template);
        return data;
    }

    private static GeneratedCV Cv(string language = ApiConstants.LanguageEnglish)
    {
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", company: "Acme", start: "Jan 2020", end: "Present", bullets: new[] { "Built the billing API" })
        }, skills: new[] { "Go", "Leadership" });
        cv.Summary = "Backend engineer.";
        cv.Education = new List<Education> { new() { Institution = "UTN", Degree = "B.S.", GraduationDate = "2017", GPA = "9" } };
        cv.Projects = new List<Project> { new() { Name = "Ledger", Technologies = new() { "Go" } } };
        cv.Language = language;
        return cv;
    }

    private static string DocxText(byte[] docx)
    {
        using var stream = new MemoryStream(docx);
        using var doc = WordprocessingDocument.Open(stream, false);
        return doc.MainDocumentPart!.Document!.Body!.InnerText;
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void Spanish_html_has_spanish_titles_dates_and_lang(CVTemplate template)
    {
        // Mustache writes "ó" as an entity, which the browser shows as the letter
        var html = System.Net.WebUtility.HtmlDecode(Renderer(template).RenderHtml(Cv(ApiConstants.LanguageSpanish)));

        Assert.Contains("<html lang=\"es\">", html);
        foreach (var title in ExpectedTitles(Cv(ApiConstants.LanguageSpanish), template))
            Assert.Contains($">{title}<", html);
        Assert.Contains("ene. 2020 – Actualidad", html);
        Assert.Contains("Tecnologías: Go", html);
        Assert.DoesNotContain(">Experience<", html);
        Assert.DoesNotContain("Present", html);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void English_html_keeps_english_titles_and_dates_as_written(CVTemplate template)
    {
        var html = Renderer(template).RenderHtml(Cv());

        Assert.Contains("<html lang=\"en\">", html);
        foreach (var title in ExpectedTitles(Cv(), template))
            Assert.Contains($">{title}<", html);
        Assert.Contains("Jan 2020 – Present", html);
    }

    [Fact]
    public void Spanish_docx_and_text_use_spanish_titles_labels_and_dates()
    {
        var cv = Cv(ApiConstants.LanguageSpanish);

        var docx = DocxText(Renderer(CVTemplate.Harvard).RenderDocx(cv));
        var text = CvTextRenderer.ToPlainText(cv);

        foreach (var output in new[] { docx, text })
        {
            Assert.Contains("RESUMEN PROFESIONAL", output);
            Assert.Contains("FORMACIÓN", output);
            Assert.Contains("EXPERIENCIA", output);
            Assert.Contains("HABILIDADES", output);
            Assert.Contains("PROYECTOS", output);
            Assert.Contains("ene. 2020 – Actualidad", output);
            Assert.Contains("Promedio: 9", output);
            Assert.DoesNotContain("EXPERIENCE", output);
        }
        Assert.Contains("## Experiencia", CvTextRenderer.ToMarkdown(cv));
    }

    [Fact]
    public async Task A_spanish_pdf_prints_spanish_titles()
    {
        var pdf = await Renderer(CVTemplate.Executive).RenderPdfAsync(Cv(ApiConstants.LanguageSpanish));

        using var doc = PdfDocument.Open(pdf);
        var text = Squash(string.Join(" ", doc.GetPages().Select(p => p.Text)));
        Assert.Contains(Squash("Perfil ejecutivo"), text);
        Assert.Contains(Squash("Experiencia"), text);
        Assert.Contains(Squash("Actualidad"), text);
    }

    [Fact]
    public void Known_accents_and_fonts_become_classes_and_anything_else_is_ignored()
    {
        var cv = Cv();
        cv.Accent = ApiConstants.AccentTeal;
        cv.Fonts = ApiConstants.FontsClean;
        Assert.Contains("<body class=\"accent-teal fonts-clean\">", Renderer(CVTemplate.Harvard).RenderHtml(cv));

        cv.Accent = "red; } body { display: none";
        cv.Fonts = ApiConstants.StyleDefault;
        var html = Renderer(CVTemplate.Harvard).RenderHtml(cv);
        Assert.Contains("<body class=\"\">", html);
        Assert.DoesNotContain("display: none", html);
    }

    [Fact]
    public void Every_accent_and_font_class_the_ui_offers_is_defined_in_the_shared_styles()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, TemplatePaths.TemplatesFolder, TemplatePaths.PartialsFolder, "base-styles.html"));

        foreach (var accent in ApiConstants.Accents.Where(a => a != ApiConstants.StyleDefault))
            Assert.Contains($".{DocumentConstants.AccentClassPrefix}{accent} ", css);
        foreach (var fonts in ApiConstants.FontPairings.Where(f => f != ApiConstants.StyleDefault))
            Assert.Contains($".{DocumentConstants.FontsClassPrefix}{fonts} ", css);
    }

    [Theory]
    [MemberData(nameof(Templates))]
    public void Every_template_uses_the_shared_partials(CVTemplate template)
    {
        var html = Renderer(template).RenderHtml(Cv());

        Assert.Contains("Shared base (UX-10)", html);
        Assert.Contains("class=\"cv-section cv-section-experience\"", html);
        Assert.Contains("class=\"cv-contact-item\"", html);
    }
}

using DocumentFormat.OpenXml.Packaging;
using ErCv.Web.Features.Documents;
using ErCv.Web.Models;
using static ErCv.Tests.Documents.DocumentSupport;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Documents;

// EDIT-01: the section order the user chose, in every template and in the DOCX
[Trait("Category", "Documents")]
public class SectionOrderTests
{
    private static GeneratedCV Cv(params string[] order)
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "Built the billing API" }) }, skills: new[] { "Go" });
        cv.Summary = "Backend engineer.";
        cv.Education = new List<Education> { new() { Institution = "UTN", Degree = "B.S." } };
        cv.Projects = new List<Project> { new() { Name = "Ledger", Description = "A ledger." } };
        cv.SectionOrder = order.ToList();
        return cv;
    }

    /// <summary>Where each title first appears in the text, so the test reads the order the document prints.</summary>
    private static List<string> TitleOrder(string text, params string[] titles) =>
        titles.Where(t => text.Contains(t, StringComparison.Ordinal))
            .OrderBy(t => text.IndexOf(t, StringComparison.Ordinal))
            .ToList();

    private static string DocxText(byte[] docx)
    {
        using var stream = new MemoryStream(docx);
        using var doc = WordprocessingDocument.Open(stream, false);
        return doc.MainDocumentPart!.Document!.Body!.InnerText;
    }

    [Fact]
    public void Resolving_drops_unknown_keys_and_repeats_and_appends_what_is_missing()
    {
        var order = DocumentRenderer.ResolveSectionOrder(
            new[] { "projects", "nope", "projects", "skills" }, DocumentConstants.HarvardSectionOrder);

        Assert.Equal(new[] { "projects", "skills", "summary", "education", "experience" }, order);
    }

    [Fact]
    public void Without_a_chosen_order_each_template_keeps_its_own()
    {
        Assert.Equal(
            new[] { ">Professional Summary<", ">Education<", ">Experience<", ">Skills<", ">Projects<" },
            TitleOrder(Renderer(CVTemplate.Harvard).RenderHtml(Cv()), ">Projects<", ">Skills<", ">Experience<", ">Education<", ">Professional Summary<"));
        Assert.Equal(
            new[] { ">Profile<", ">Experience<", ">Education<", ">Skills<", ">Projects<" },
            TitleOrder(Renderer(CVTemplate.Minimalist).RenderHtml(Cv()), ">Projects<", ">Skills<", ">Education<", ">Experience<", ">Profile<"));
    }

    [Theory]
    [InlineData(CVTemplate.Harvard)]
    [InlineData(CVTemplate.Minimalist)]
    public void Single_column_templates_print_the_chosen_order(CVTemplate template)
    {
        var html = Renderer(template).RenderHtml(Cv("projects", "experience", "skills", "education", "summary"));

        var summaryTitle = template == CVTemplate.Harvard ? "Professional Summary" : "Profile";
        Assert.Equal(
            new[] { ">Projects<", ">Experience<", ">Skills<", ">Education<", $">{summaryTitle}<" },
            TitleOrder(html, ">Projects<", ">Experience<", ">Skills<", ">Education<", $">{summaryTitle}<"));
    }

    [Fact]
    public void Modern_moves_a_section_only_within_its_column()
    {
        var html = Renderer(CVTemplate.Modern).RenderHtml(Cv("education", "projects", "experience", "skills", "summary"));

        var left = html[html.IndexOf("class=\"left-col\"", StringComparison.Ordinal)..html.IndexOf("class=\"right-col\"", StringComparison.Ordinal)];
        var right = html[html.IndexOf("class=\"right-col\"", StringComparison.Ordinal)..];
        Assert.Equal(new[] { ">Education<", ">Skills<" }, TitleOrder(left, ">Skills<", ">Education<"));
        Assert.Equal(new[] { ">Projects<", ">Experience<", ">Profile<" }, TitleOrder(right, ">Profile<", ">Experience<", ">Projects<"));
    }

    [Fact]
    public void Every_section_is_printed_once_whatever_the_order()
    {
        foreach (var template in Enum.GetValues<CVTemplate>())
        {
            var html = Renderer(template).RenderHtml(Cv("skills", "skills", "unknown"));

            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, "Built the billing API"));
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(html, @"A ledger\."));
        }
    }

    [Fact]
    public void The_docx_prints_the_chosen_order_and_harvard_order_by_default()
    {
        var renderer = Renderer(CVTemplate.Modern);

        Assert.Equal(
            new[] { "PROFESSIONAL SUMMARY", "EDUCATION", "EXPERIENCE", "SKILLS", "PROJECTS" },
            TitleOrder(DocxText(renderer.RenderDocx(Cv())), "PROFESSIONAL SUMMARY", "EDUCATION", "EXPERIENCE", "SKILLS", "PROJECTS"));
        Assert.Equal(
            new[] { "EXPERIENCE", "PROJECTS", "SKILLS", "PROFESSIONAL SUMMARY", "EDUCATION" },
            TitleOrder(DocxText(renderer.RenderDocx(Cv("experience", "projects", "skills"))), "PROFESSIONAL SUMMARY", "EDUCATION", "EXPERIENCE", "SKILLS", "PROJECTS"));
    }

    [Fact]
    public void The_text_export_prints_the_chosen_order()
    {
        var text = CvTextRenderer.ToPlainText(Cv("skills", "experience"));

        Assert.Equal(
            new[] { "SKILLS", "EXPERIENCE", "PROFESSIONAL SUMMARY", "EDUCATION", "PROJECTS" },
            TitleOrder(text, "PROFESSIONAL SUMMARY", "EDUCATION", "EXPERIENCE", "SKILLS", "PROJECTS"));
    }
}

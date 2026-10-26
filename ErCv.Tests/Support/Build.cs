using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using ErCv.Web.Models;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace ErCv.Tests.Support;

/// <summary>Small object builders so each test only states what it cares about.</summary>
public static class Build
{
    public static Experience Exp(
        string role = "Software Engineer",
        string end = "Present",
        string start = "Jan 2020",
        string[]? bullets = null,
        string[]? skills = null,
        string company = "Acme") => new()
    {
        Role = role,
        Company = company,
        StartDate = start,
        EndDate = end,
        Bullets = (bullets ?? Array.Empty<string>()).ToList(),
        Skills = (skills ?? Array.Empty<string>()).ToList()
    };

    public static JobRequirements Req(
        string[]? skills = null,
        string[]? keywords = null,
        string seniority = "mid-level",
        string[]? niceToHave = null,
        string title = "",
        string text = "") => new()
    {
        Title = title,
        Text = text,
        TechnicalSkills = (skills ?? Array.Empty<string>()).ToList(),
        NiceToHaveSkills = (niceToHave ?? Array.Empty<string>()).ToList(),
        Keywords = (keywords ?? Array.Empty<string>()).ToList(),
        SeniorityLevel = seniority
    };

    public static CVMaster Master(
        Experience[]? exps = null,
        string[]? languages = null,
        string[]? tools = null) => new()
    {
        Header = new Header { FullName = "Test User", Email = "t@x.com", Phone = "+1 555 123 4567" },
        Experience = (exps ?? Array.Empty<Experience>()).ToList(),
        Skills = new Skills
        {
            Languages = (languages ?? Array.Empty<string>()).ToList(),
            Tools = (tools ?? Array.Empty<string>()).ToList()
        }
    };

    public static GeneratedCV Generated(
        Experience[]? exps = null,
        string[]? skills = null,
        string email = "t@x.com",
        string phone = "+1 555 123 4567",
        bool education = true) => new()
    {
        Header = new Header { FullName = "Test User", Email = email, Phone = phone },
        Education = education ? new List<Education> { new() { Degree = "B.S." } } : new List<Education>(),
        Experience = (exps ?? Array.Empty<Experience>()).ToList(),
        Skills = (skills ?? Array.Empty<string>()).ToList()
    };

    public static string Year(int yearsAgo) => (DateTime.Now.Year - yearsAgo).ToString();

    /// <summary>A Word document with one paragraph per line; lines starting with "- " become numbered list items.</summary>
    public static byte[] ResumeDocx(IEnumerable<string> lines, string? link = null)
    {
        using var stream = new MemoryStream();
        using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var main = document.AddMainDocumentPart();
            main.Document = new W.Document(new W.Body());
            foreach (var line in lines)
            {
                var paragraph = main.Document.Body!.AppendChild(new W.Paragraph());
                var text = line;
                if (text.StartsWith("- "))
                {
                    paragraph.AppendChild(new W.ParagraphProperties(new W.NumberingProperties(new W.NumberingId { Val = 1 })));
                    text = text[2..];
                }
                var run = paragraph.AppendChild(new W.Run());
                var columns = text.Split('\t');
                for (var i = 0; i < columns.Length; i++)
                {
                    if (i > 0) run.AppendChild(new W.TabChar());
                    run.AppendChild(new W.Text(columns[i]) { Space = SpaceProcessingModeValues.Preserve });
                }
            }
            if (link != null) main.AddHyperlinkRelationship(new Uri(link), true);
            main.Document.Save();
        }
        return stream.ToArray();
    }

    /// <summary>A PDF with one blank page and, when given, a line of text on it (a scanner's watermark).</summary>
    public static byte[] ScannedPdf(string? watermark = null)
    {
        var builder = new UglyToad.PdfPig.Writer.PdfDocumentBuilder();
        var page = builder.AddPage(UglyToad.PdfPig.Content.PageSize.A4);
        if (watermark != null)
            page.AddText(watermark, 10, new UglyToad.PdfPig.Core.PdfPoint(50, 50),
                builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Helvetica));
        return builder.Build();
    }

    /// <summary>A PDF with a text layer: one line per line given, from the top of the page.</summary>
    public static byte[] ResumePdf(IEnumerable<string> lines)
    {
        var builder = new UglyToad.PdfPig.Writer.PdfDocumentBuilder();
        var page = builder.AddPage(UglyToad.PdfPig.Content.PageSize.A4);
        var font = builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Helvetica);
        var y = 800.0;
        foreach (var line in lines)
        {
            page.AddText(line, 11, new UglyToad.PdfPig.Core.PdfPoint(50, y), font);
            y -= 16;
        }
        return builder.Build();
    }
}

using System.IO.Compression;
using ErCv.Web.Features.Profiles;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Profiles;

[Trait("Category", "Unit")]
public class ResumeTextTests
{
    [Fact]
    public void Detects_pdf_and_word_by_their_first_bytes()
    {
        Assert.Equal(ResumeFormat.Pdf, ResumeText.Detect("%PDF-1.7 rest"u8.ToArray()));
        Assert.Equal(ResumeFormat.Docx, ResumeText.Detect(ResumeDocx(new[] { "Jane" })));
        Assert.Null(ResumeText.Detect("{\"header\":{}}"u8.ToArray()));
    }

    [Fact]
    public void Word_list_items_get_a_dash_and_tabs_become_columns()
    {
        var text = ResumeText.FromDocx(ResumeDocx(new[] { "Acme\tJan 2020 - Present", "- Built the API" }));

        Assert.Equal(new[] { "Acme | Jan 2020 - Present", "- Built the API" }, Lines(text));
    }

    [Fact]
    public void Linkedin_and_github_link_targets_come_first()
    {
        var text = ResumeText.FromDocx(ResumeDocx(new[] { "Jane Doe", "LinkedIn" }, "https://www.linkedin.com/in/jane-doe/"));

        Assert.Equal("https://www.linkedin.com/in/jane-doe/", Lines(text)[0]);
    }

    [Fact]
    public void A_word_file_that_expands_past_the_limit_is_refused_before_opening()
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            using var entry = zip.CreateEntry("word/document.xml", CompressionLevel.SmallestSize).Open();
            var zeros = new byte[1024 * 1024];
            for (var written = 0L; written <= ResumeImportConstants.MaxDocxUncompressedBytes; written += zeros.Length)
                entry.Write(zeros);
        }

        Assert.Throws<InvalidDataException>(() => ResumeText.FromDocx(stream.ToArray()));
    }

    private static string[] Lines(string text) =>
        text.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0).ToArray();
}

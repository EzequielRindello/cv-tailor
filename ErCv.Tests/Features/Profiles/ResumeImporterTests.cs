using ErCv.Web.Features.Profiles;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Profiles;

[Trait("Category", "Unit")]
public class ResumeImporterTests
{
    [Fact]
    public async Task A_word_resume_becomes_a_draft_profile()
    {
        var docx = ResumeDocx(new[]
        {
            "Jane Doe", "jane@doe.dev", "EXPERIENCE", "Engineer at Acme\t2020 - Present", "- Built the billing API"
        });

        var result = await ResumeImporter.ImportAsync(docx);

        Assert.Null(result.Error);
        Assert.Equal("Jane Doe", result.Profile!.Header.FullName);
        Assert.Equal("Built the billing API", Assert.Single(Assert.Single(result.Profile.Experience).Bullets));
    }

    [Fact]
    public async Task An_empty_file_is_refused()
    {
        var result = await ResumeImporter.ImportAsync(Array.Empty<byte>());

        Assert.Null(result.Profile);
        Assert.Equal("The file is empty.", result.Error);
    }

    [Fact]
    public async Task A_file_over_the_limit_is_refused_before_it_is_read()
    {
        Assert.Null(ResumeImporter.SizeError(ResumeImportConstants.MaxImportBytes));
        Assert.Equal("The file is larger than 4 MB.", ResumeImporter.SizeError(ResumeImportConstants.MaxImportBytes + 1));

        var result = await ResumeImporter.ImportAsync(new byte[ResumeImportConstants.MaxImportBytes + 1]);

        Assert.Equal("The file is larger than 4 MB.", result.Error);
    }

    [Fact]
    public async Task Only_pdf_and_word_files_are_read()
    {
        var result = await ResumeImporter.ImportAsync("{\"header\":{\"fullName\":\"Jane\"}}"u8.ToArray());

        Assert.Equal(ResumeImportConstants.WrongTypeMessage, result.Error);
    }

    [Theory]
    [InlineData("%PDF-1.7 this is not a pdf")]
    [InlineData("PK\u0003\u0004 this is not a zip")]
    public async Task A_damaged_file_gets_a_message_not_an_exception(string content)
    {
        var result = await ResumeImporter.ImportAsync(System.Text.Encoding.Latin1.GetBytes(content));

        Assert.Null(result.Profile);
        Assert.Equal("The file could not be read. Save it again as PDF or .docx and retry.", result.Error);
    }

    [Fact]
    public async Task A_document_without_resume_sections_is_refused()
    {
        var result = await ResumeImporter.ImportAsync(ResumeDocx(new[] { "Dear hiring manager,", "I am writing to apply." }));

        Assert.Equal("No resume sections were found (Experience, Education, Skills, Projects).", result.Error);
    }

    [Fact]
    public async Task A_word_file_with_no_text_is_refused()
    {
        var result = await ResumeImporter.ImportAsync(ResumeDocx(Array.Empty<string>()));

        Assert.Equal(ResumeImportConstants.NoTextMessage, result.Error);
    }

    [Fact]
    public async Task A_scanned_pdf_is_refused_before_it_is_parsed()
    {
        var result = await ResumeImporter.ImportAsync(ScannedPdf());

        Assert.Equal(ResumeImportConstants.ScannedPdfMessage, result.Error);
    }

    [Fact]
    public async Task A_pdf_with_only_a_scanner_watermark_is_refused()
    {
        var result = await ResumeImporter.ImportAsync(ScannedPdf("Scanned with CamScanner"));

        Assert.Equal(ResumeImportConstants.ScannedPdfMessage, result.Error);
    }

    [Fact]
    public async Task A_text_pdf_is_not_mistaken_for_a_scan()
    {
        var pdf = ResumePdf(new[]
        {
            "Jane Doe", "jane@doe.dev", "EXPERIENCE", "Platform Engineer at Acme Corporation 2020 - Present",
            "Built the billing API and the reporting pipeline", "SKILLS", "Languages: C#, SQL, TypeScript"
        });

        var result = await ResumeImporter.ImportAsync(pdf);

        Assert.NotEqual(ResumeImportConstants.ScannedPdfMessage, result.Error);
        Assert.Equal("Jane Doe", result.Profile?.Header.FullName);
    }

    [Theory]
    [InlineData("jane.pdf", 1000, null)]
    [InlineData("JANE.PDF", 1000, null)]
    [InlineData("jane.docx", 1000, null)]
    public void A_pdf_or_word_file_within_the_limit_can_go_on(string name, long size, string? expected) =>
        Assert.Equal(expected, ResumeImporter.UploadError(name, size));

    [Theory]
    [InlineData("jane.doc")]
    [InlineData("notes.txt")]
    [InlineData("resume")]
    public void Any_other_type_is_refused_by_its_name(string name) =>
        Assert.Equal(ResumeImportConstants.WrongTypeMessage, ResumeImporter.UploadError(name, 1000));

    [Fact]
    public void A_pdf_over_the_limit_gets_the_size_message() =>
        Assert.Equal(ResumeImporter.SizeError(ResumeImportConstants.MaxImportBytes + 1),
            ResumeImporter.UploadError("jane.pdf", ResumeImportConstants.MaxImportBytes + 1));
}

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

        Assert.Equal("Only PDF and Word (.docx) files can be read.", result.Error);
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

        Assert.Equal("No text was found in the file. A scanned resume (an image) cannot be read.", result.Error);
    }
}

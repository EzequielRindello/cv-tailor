using DocumentFormat.OpenXml.Packaging;
using Microsoft.Playwright;

namespace ErCv.E2E;

// EDIT-03: the cover letter option, its preview next to the CV, its downloads, and following the CV's edits
public class CoverLetterTests : E2ETestBase
{
    public CoverLetterTests(AppFixture app) : base(app)
    {
    }

    private ILocator LetterBody => Page.FrameLocator("iframe[title='Cover letter']").Locator("body");

    private async Task GenerateWithLetterAsync()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await GoToOptionsAsync();
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Also write a cover letter" }).CheckAsync();
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Without_the_option_there_is_no_letter_switch()
    {
        await GenerateAsync();

        await Assertions.Expect(Button("Cover letter", exact: true)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task The_letter_shows_next_to_the_cv_and_downloads_as_docx()
    {
        await GenerateWithLetterAsync();

        await Button("Cover letter", exact: true).ClickAsync();
        await Assertions.Expect(LetterBody).ToContainTextAsync("Dear Hiring Manager,");
        await Assertions.Expect(LetterBody).ToContainTextAsync("In my role as");

        var download = await Page.RunAndWaitForDownloadAsync(() => Button("Letter DOCX").ClickAsync());
        Assert.StartsWith("Cover_Letter_", download.SuggestedFilename);
        await using var stream = await download.CreateReadStreamAsync();
        using var memory = new MemoryStream();
        await stream!.CopyToAsync(memory);
        memory.Position = 0;
        using var doc = WordprocessingDocument.Open(memory, false);
        Assert.Contains("Dear Hiring Manager,", doc.MainDocumentPart!.Document!.Body!.InnerText);

        await Button("CV", exact: true).ClickAsync();
        await Assertions.Expect(Page.Locator("iframe[title='CV Preview']")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task The_letter_follows_a_rewritten_bullet()
    {
        const string rewrite = "Rewritten in the browser for the letter";
        await GenerateWithLetterAsync();

        await Button("Edit", exact: true).ClickAsync();
        await Page.Locator("#bullet-edit-btn-0-0").ClickAsync();
        await Page.Locator("#bullet-edit-0-0").FillAsync(rewrite);
        await Page.Locator(".bullet-editor-card").GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
        await Page.Locator(".bullet-editor-card").GetByRole(AriaRole.Button, new() { Name = "Done", Exact = true }).ClickAsync();

        await Button("Cover letter", exact: true).ClickAsync();
        await Assertions.Expect(LetterBody).ToContainTextAsync(rewrite);
    }
}

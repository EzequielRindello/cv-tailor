using System.Text.RegularExpressions;
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
        // The sample is a real post of a real company, and the letter names it (EDIT-09)
        await Assertions.Expect(LetterBody).ToContainTextAsync(new Regex(@"Dear .+ team,"));
        await Assertions.Expect(LetterBody).ToContainTextAsync("In my role as");

        var download = await Page.RunAndWaitForDownloadAsync(() => Button("Letter DOCX").ClickAsync());
        Assert.StartsWith("Cover_Letter_", download.SuggestedFilename);
        await using var stream = await download.CreateReadStreamAsync();
        using var memory = new MemoryStream();
        await stream!.CopyToAsync(memory);
        memory.Position = 0;
        using var doc = WordprocessingDocument.Open(memory, false);
        Assert.Matches(@"Dear .+ team,", doc.MainDocumentPart!.Document!.Body!.InnerText);

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

    // EDIT-09: what the visitor writes in the letter is theirs. It shows in the preview, survives an edit of the CV
    // (which marks it) and reaches the DOCX
    [Fact]
    public async Task An_edited_paragraph_stays_when_the_cv_changes_and_reaches_the_docx()
    {
        const string mine = "My own opening, written in the browser.";
        await GenerateWithLetterAsync();
        await Button("Cover letter", exact: true).ClickAsync();
        await Button("Edit", exact: true).ClickAsync();

        await Page.Locator("#letter-text-1").FillAsync(mine);
        await Page.Locator("#letter-text-1").BlurAsync();
        await Assertions.Expect(LetterBody).ToContainTextAsync(mine);

        // The CV changes: the opening is the visitor's, so it stays
        await Button("CV", exact: true).ClickAsync();
        await Page.Locator("#bullet-0-0").UncheckAsync();
        await Button("Cover letter", exact: true).ClickAsync();
        await Assertions.Expect(LetterBody).ToContainTextAsync(mine);

        var download = await Page.RunAndWaitForDownloadAsync(() => Button("Letter DOCX").ClickAsync());
        await using var stream = await download.CreateReadStreamAsync();
        using var memory = new MemoryStream();
        await stream!.CopyToAsync(memory);
        memory.Position = 0;
        using var doc = WordprocessingDocument.Open(memory, false);
        Assert.Contains(mine, doc.MainDocumentPart!.Document!.Body!.InnerText);
    }

    // EDIT-10: filling the letter past a page shows the page count, and nothing is cut
    [Fact]
    public async Task A_letter_made_longer_than_a_page_shows_the_page_count()
    {
        await GenerateWithLetterAsync();
        await Button("Cover letter", exact: true).ClickAsync();
        await Button("Edit", exact: true).ClickAsync();
        await Assertions.Expect(Page.Locator("#letter-page-warning")).ToHaveCountAsync(0);

        var longText = string.Join(" ", Enumerable.Repeat("delivered", 140));
        for (var i = 1; i <= 4; i++)
        {
            await Page.Locator($"#letter-text-{i}").FillAsync(longText);
            await Page.Locator($"#letter-text-{i}").BlurAsync();
        }

        await Assertions.Expect(Page.Locator("#letter-page-warning")).ToContainTextAsync("pages", new() { Timeout = 15000 });
    }

    // APP-09: the company of the tracker entry the result created is the letter's company, live
    [Fact]
    public async Task The_company_edited_in_the_tracker_updates_the_open_letter()
    {
        await GenerateWithLetterAsync();

        await Button("Back to composer").ClickAsync();
        await Button("History", exact: true).ClickAsync();
        await Page.Locator(".history-item").GetByRole(AriaRole.Button, new() { Name = "Details" }).ClickAsync();
        await Page.GetByLabel("Company", new() { Exact = true }).FillAsync("Globex");
        await Page.GetByLabel("Company", new() { Exact = true }).BlurAsync();
        await Assertions.Expect(Page.Locator(".history-title")).ToContainTextAsync("Globex");

        await Page.Locator("#back-to-results").ClickAsync();
        await Button("Cover letter", exact: true).ClickAsync();

        await Assertions.Expect(LetterBody).ToContainTextAsync("Dear Globex team,");
    }

    [Fact]
    public async Task Changing_the_company_in_the_letter_editor_rewrites_the_greeting()
    {
        await GenerateWithLetterAsync();
        await Button("Cover letter", exact: true).ClickAsync();
        await Button("Edit", exact: true).ClickAsync();

        await Page.Locator("#letter-company").FillAsync("Globex");
        await Page.Locator("#letter-company").BlurAsync();

        await Assertions.Expect(LetterBody).ToContainTextAsync("Dear Globex team,");
    }

    [Fact]
    public async Task Picking_a_sample_fills_the_company_and_typing_one_wins()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await Assertions.Expect(Page.Locator("#company")).Not.ToHaveValueAsync("");

        await Page.Locator("#company").FillAsync("Initech");
        await GoToOptionsAsync();
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Also write a cover letter" }).CheckAsync();
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        await Button("Cover letter", exact: true).ClickAsync();

        await Assertions.Expect(LetterBody).ToContainTextAsync("Dear Initech team,");
    }
}

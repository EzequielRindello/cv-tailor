using Microsoft.Playwright;

namespace ErCv.E2E;

/// <summary>A fresh browser context per test (own localStorage, own cookies) plus the steps every flow shares.</summary>
[Collection("E2E")]
[Trait("Category", "E2E")]
public abstract class E2ETestBase : IAsyncLifetime
{
    private readonly AppFixture _app;
    private IBrowserContext _context = null!;

    protected E2ETestBase(AppFixture app)
    {
        _app = app;
    }

    protected IPage Page { get; private set; } = null!;

    /// <summary>Override to test another viewport.</summary>
    protected virtual ViewportSize Viewport => new() { Width = 1280, Height = 900 };

    public async Task InitializeAsync()
    {
        _context = await _app.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = Viewport,
            AcceptDownloads = true,
            // Ctrl+Shift+V reads the clipboard
            Permissions = new[] { "clipboard-read", "clipboard-write" },
            BaseURL = _app.BaseUrl
        });
        Page = await _context.NewPageAsync();
        Page.SetDefaultTimeout(20_000);
    }

    public async Task DisposeAsync() => await _context.CloseAsync();

    protected async Task OpenHomeAsync()
    {
        await Page.GotoAsync("/");
        // The page is interactive once the stepper (rendered by Blazor, not by the static shell) is there
        await Page.GetByRole(AriaRole.Navigation, new() { Name = "Composer steps" }).WaitForAsync();
        // The keyboard shortcuts are registered a moment after the first render; on a slow runner a key pressed in between is lost
        await Page.Locator("html[data-shortcuts='ready']").WaitForAsync(new() { State = WaitForSelectorState.Attached });
    }

    /// <summary>Creates "My profile" the way a visitor does: import a JSON file on the profile page, then save it.</summary>
    protected async Task SaveMyProfileFromJsonAsync(string json)
    {
        await Page.GotoAsync("/profile");
        await Assertions.Expect(Page.Locator("#profile-save")).ToBeVisibleAsync();
        await Page.Locator("#profile-import").SetInputFilesAsync(new FilePayload
        {
            Name = "profile.json",
            MimeType = "application/json",
            Buffer = System.Text.Encoding.UTF8.GetBytes(json)
        });
        await Assertions.Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Imported" })).ToBeVisibleAsync();
        await Page.Locator("#profile-save").ClickAsync();
        await Assertions.Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Saved in this browser" })).ToBeVisibleAsync();
    }

    /// <summary>From the home page to step 2 with "My profile" chosen.</summary>
    protected async Task ChooseMyProfileAsync(string sample = "Backend")
    {
        await OpenHomeAsync();
        await PickSampleAsync(sample);
        await NextAsync();
        await Page.Locator("label[for='source-mine']").ClickAsync();
    }

    protected ILocator Button(string name, bool exact = false) =>
        Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = exact });

    protected ILocator JobDescription => Page.GetByRole(AriaRole.Textbox, new() { Name = "Job Description" });

    protected ILocator ResultsRegion => Page.GetByRole(AriaRole.Region, new() { Name = "CV results" });

    protected ILocator PreviewBody => Page.FrameLocator("iframe[title='CV Preview']").Locator("body");

    protected async Task PickSampleAsync(string name)
    {
        await Button(name, exact: true).ClickAsync();
        // The sample lands in the editor once Blazor has re-rendered
        await Assertions.Expect(JobDescription).Not.ToHaveValueAsync("");
    }

    protected async Task NextAsync()
    {
        await Button("Next").ClickAsync();
    }

    /// <summary>Moves from step 1 to step 3 and picks the formats (Word only by default: no PDF engine needed).</summary>
    protected async Task GoToOptionsAsync(bool pdf = false, bool word = true)
    {
        await NextAsync();
        await Assertions.Expect(Page.GetByRole(AriaRole.Radiogroup, new() { Name = "Resume Template" })).ToBeVisibleAsync();
        await NextAsync();

        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "PDF" }), pdf);
        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), word);
    }

    protected static async Task SetCheckedAsync(ILocator checkbox, bool wanted)
    {
        if (await checkbox.IsCheckedAsync() != wanted)
            await checkbox.SetCheckedAsync(wanted);
    }

    /// <summary>Sample JD, through the three steps, to the results workspace.</summary>
    protected async Task GenerateAsync(string sample = "Backend", bool pdf = false, bool word = true)
    {
        await OpenHomeAsync();
        await PickSampleAsync(sample);
        await GoToOptionsAsync(pdf, word);
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        // The preview is the first pane on every viewport; the insights tabs are not visible on small screens
        await Assertions.Expect(Page.Locator("iframe[title='CV Preview']")).ToBeVisibleAsync();
    }

    /// <summary>
    /// The results show before the history entry is written to localStorage, so a test that leaves the page right
    /// after generating waits for it first, or the entry is lost with the navigation.
    /// </summary>
    protected Task WaitForHistoryEntriesAsync(int count) =>
        Page.WaitForFunctionAsync("n => JSON.parse(localStorage.getItem('cv-generator-history') || '[]').length >= n", count);

    /// <summary>Clicks a template or page-option card by its label, like a user (the radio inside is visually hidden).</summary>
    protected async Task ChooseCardAsync(string name)
    {
        await Page.Locator("label.template-card, label.page-option-card").Filter(new() { HasText = name }).ClickAsync();
    }

    protected async Task<bool> HasHorizontalScrollAsync() =>
        await Page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth");
}

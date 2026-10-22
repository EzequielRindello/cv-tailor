using Microsoft.Playwright;

namespace ErCv.E2E;

/// <summary>
/// OPS-03: the README screenshots, taken from the running app so they can be refreshed with each release.
/// Off unless ERCV_SCREENSHOTS names the output folder (relative to the repository root), for example:
/// ERCV_SCREENSHOTS=docs/screenshots dotnet test ErCv.E2E --filter Category=Screenshots
/// </summary>
[Trait("Category", "Screenshots")]
public class ScreenshotTests : E2ETestBase
{
    private const string FolderVariable = "ERCV_SCREENSHOTS";

    // Lets the score gauges and the page transitions finish before a shot
    private const int SettleMilliseconds = 1500;

    // The sample the default (senior) profile matches best, so the shots show a typical result
    private const string Sample = "DevOps";

    public ScreenshotTests(AppFixture app) : base(app)
    {
    }

    protected override ViewportSize Viewport => new() { Width = 1440, Height = 900 };

    [Fact]
    public async Task Readme_screenshots()
    {
        var folder = Environment.GetEnvironmentVariable(FolderVariable);
        if (string.IsNullOrWhiteSpace(folder)) return;
        folder = Path.IsPathRooted(folder) ? folder : Path.Combine(RepoRoot(), folder);
        Directory.CreateDirectory(folder);

        await OpenHomeAsync();
        await PickSampleAsync(Sample);
        await ShotAsync(folder, "composer.png");

        await GoToOptionsAsync();
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        await ShotAsync(folder, "workspace.png");

        await Button("Edit", exact: true).ClickAsync();
        await Assertions.Expect(Page.Locator(".bullet-editor-card")).ToBeVisibleAsync();
        await ShotAsync(folder, "editor.png");
        await Button("Done", exact: true).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Switch to dark theme" }).ClickAsync();
        await Page.GetByRole(AriaRole.Tab, new() { Name = "Job" }).ClickAsync();
        await Page.Locator(".jd-term").First.ClickAsync();
        await Assertions.Expect(Page.Locator(".jd-detail-title")).ToBeVisibleAsync();
        await ShotAsync(folder, "job-post-dark.png");
    }

    private async Task ShotAsync(string folder, string file)
    {
        // Toasts cover the top right corner of every shot
        foreach (var close in await Page.Locator(".app-toast-close").AllAsync())
            await close.ClickAsync();
        // Focus moves (to the job description, to the results) scroll the page; the shots start at the top
        await Page.EvaluateAsync("() => window.scrollTo(0, 0)");
        await Page.WaitForTimeoutAsync(SettleMilliseconds);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(folder, file) });
    }

    private static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "er-cv-generator.sln"))) return dir.FullName;
        throw new DirectoryNotFoundException("Repository root (er-cv-generator.sln) not found above the test output");
    }
}

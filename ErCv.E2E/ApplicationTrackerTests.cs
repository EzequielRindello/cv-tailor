using Microsoft.Playwright;

namespace ErCv.E2E;

public class ApplicationTrackerTests : E2ETestBase
{
    public ApplicationTrackerTests(AppFixture app) : base(app)
    {
    }

    private async Task GenerateAndOpenHistoryAsync(string sample)
    {
        await OpenHomeAsync();
        await PickSampleAsync(sample);
        await GoToOptionsAsync();
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        await WaitForHistoryEntriesAsync(1);

        await OpenHomeAsync();
        await Button("History", exact: true).ClickAsync();
    }

    [Fact]
    public async Task Application_details_and_status_are_kept_after_a_reload_and_can_be_filtered()
    {
        await GenerateAndOpenHistoryAsync("Backend");
        await Page.Locator(".history-item").GetByRole(AriaRole.Button, new() { Name = "Details" }).ClickAsync();

        await Page.GetByLabel("Company", new() { Exact = true }).FillAsync("Acme");
        await Page.GetByLabel("Role", new() { Exact = true }).FillAsync("Platform Engineer");
        await Page.GetByLabel("Application status").SelectOptionAsync("Interview");
        await Page.GetByLabel("Notes").FillAsync("Phone screen on Monday");
        await Page.GetByLabel("Notes").BlurAsync();

        await Assertions.Expect(Page.Locator(".history-title")).ToHaveTextAsync("Platform Engineer at Acme");
        await Assertions.Expect(Page.Locator(".history-status")).ToHaveTextAsync("Interview");

        await Page.ReloadAsync();
        await Page.GetByRole(AriaRole.Navigation, new() { Name = "Composer steps" }).WaitForAsync();
        await Button("History", exact: true).ClickAsync();

        await Assertions.Expect(Page.Locator(".history-title")).ToHaveTextAsync("Platform Engineer at Acme");
        await Page.Locator(".history-item").GetByRole(AriaRole.Button, new() { Name = "Details" }).ClickAsync();
        await Assertions.Expect(Page.GetByLabel("Notes")).ToHaveValueAsync("Phone screen on Monday");

        await Page.Locator("#history-filter").SelectOptionAsync("Offer");
        await Assertions.Expect(Page.Locator(".history-item")).ToHaveCountAsync(0);
        await Assertions.Expect(Page.Locator(".history-no-match")).ToBeVisibleAsync();

        await Page.Locator("#history-filter").SelectOptionAsync("Interview");
        await Assertions.Expect(Page.Locator(".history-item")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task Entries_can_be_sorted_by_status()
    {
        await GenerateAndOpenHistoryAsync("Backend");
        await Button("Close", exact: true).ClickAsync();
        await PickSampleAsync("Junior");
        await GoToOptionsAsync();
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        await WaitForHistoryEntriesAsync(2);

        await OpenHomeAsync();
        await Button("History", exact: true).ClickAsync();
        await Assertions.Expect(Page.Locator(".history-item")).ToHaveCountAsync(2);

        // Mark the newest one as an offer, then sort: Saved comes before Offer
        await Page.Locator(".history-item").First.GetByRole(AriaRole.Button, new() { Name = "Details" }).ClickAsync();
        await Page.GetByLabel("Application status").SelectOptionAsync("Offer");
        await Page.Locator("#history-sort").SelectOptionAsync("Status");

        await Assertions.Expect(Page.Locator(".history-status").First).ToHaveTextAsync("Saved");
        await Assertions.Expect(Page.Locator(".history-status").Last).ToHaveTextAsync("Offer");
    }
}

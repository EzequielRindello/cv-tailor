using Microsoft.Playwright;

namespace ErCv.E2E;

public class MyProfileTests : E2ETestBase
{
    public MyProfileTests(AppFixture app) : base(app)
    {
    }

    private async Task OpenProfilePageAsync()
    {
        await Page.GotoAsync("/profile");
        await Assertions.Expect(Page.Locator("#profile-save")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Saving_an_incomplete_profile_shows_field_errors_and_saves_nothing()
    {
        await OpenProfilePageAsync();
        await Page.Locator("#profile-save").ClickAsync();

        await Assertions.Expect(Page.Locator("#p-fullName-err")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#p-email-err")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#exp-0-role-err")).ToBeVisibleAsync();
        // The summary at the top says where each problem is, in words
        await Assertions.Expect(Page.Locator("#profile-errors")).ToContainTextAsync("Header, full name: Full name is required.");
        await Assertions.Expect(Page.Locator("#profile-errors")).ToContainTextAsync("Experience 1, role");

        var saved = await Page.EvaluateAsync<string?>("() => localStorage.getItem('cv-generator-my-profile')");
        Assert.Null(saved);
    }

    [Fact]
    public async Task Created_profile_is_saved_in_the_browser_and_used_for_the_cv()
    {
        await OpenProfilePageAsync();

        await Page.Locator("#p-fullName").FillAsync("Zed Tester");
        await Page.Locator("#p-email").FillAsync("zed@example.com");
        await Page.Locator("#exp-0-role").FillAsync("Backend Engineer");
        await Page.Locator("#exp-0-company").FillAsync("Acme");
        await Page.Locator("#exp-0-start").FillAsync("Jan 2021");
        await Page.Locator("#exp-0-bullets-add").ClickAsync();
        await Page.Locator("#exp-0-bullets-0").FillAsync("Built C# services handling 2M requests a day");
        await Page.Locator("#exp-0-skills").FillAsync("C#, SQL");
        await Page.Locator("#profile-save").ClickAsync();
        await Assertions.Expect(Page.GetByRole(AriaRole.Status).Filter(new() { HasText = "Saved in this browser" })).ToBeVisibleAsync();

        var saved = await Page.EvaluateAsync<string>("() => localStorage.getItem('cv-generator-my-profile')");
        Assert.Contains("Zed Tester", saved);

        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();
        await Page.Locator("label[for='source-mine']").ClickAsync();
        await NextAsync();
        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
        await Button("Generate CV").ClickAsync();

        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        await Assertions.Expect(PreviewBody).ToContainTextAsync("Tester");
    }

    [Fact]
    public async Task My_profile_option_is_disabled_until_a_profile_is_saved()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();

        await Assertions.Expect(Page.Locator("#source-mine")).ToBeDisabledAsync();
        await Assertions.Expect(Page.Locator("#profile-edit-link")).ToHaveTextAsync("Create my profile");
    }
}

using System.Text;
using Microsoft.Playwright;

namespace ErCv.E2E;

public class ProfileImportTests : E2ETestBase
{
    public ProfileImportTests(AppFixture app) : base(app)
    {
    }

    private async Task OpenProfilePageAsync()
    {
        await Page.GotoAsync("/profile");
        await Assertions.Expect(Page.Locator("#profile-export")).ToBeVisibleAsync();
    }

    private static FilePayload Json(string text) =>
        new() { Name = "profile.json", MimeType = "application/json", Buffer = Encoding.UTF8.GetBytes(text) };

    [Fact]
    public async Task Step_two_has_no_import_export_or_template_buttons()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();

        await Assertions.Expect(Page.Locator("#source-demo")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#profile-edit-link")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#profile-import, #profile-export, #profile-template")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Exported_profile_can_be_edited_imported_saved_and_is_used_for_the_cv()
    {
        await OpenProfilePageAsync();
        await Page.Locator("#profile-use-demo").ClickAsync();
        await Assertions.Expect(Page.Locator("#p-fullName")).ToHaveValueAsync("Ezequiel Rindello");

        var download = await Page.RunAndWaitForDownloadAsync(() => Page.Locator("#profile-export").ClickAsync());
        await using var stream = await download.CreateReadStreamAsync();
        var exported = await new StreamReader(stream!).ReadToEndAsync();
        Assert.Contains("Rindello", exported);

        await SaveMyProfileFromJsonAsync(exported.Replace("Ezequiel Rindello", "Zed Tester"));
        await Assertions.Expect(Page.Locator("#p-fullName")).ToHaveValueAsync("Zed Tester");

        await ChooseMyProfileAsync();
        await NextAsync();
        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        await Assertions.Expect(PreviewBody).ToContainTextAsync("Tester");
    }

    [Fact]
    public async Task Invalid_file_is_rejected_with_field_level_errors()
    {
        await OpenProfilePageAsync();

        await Page.Locator("#profile-import").SetInputFilesAsync(Json("""{ "header": { "fullName": "", "email": "nope" } }"""));

        var errors = Page.Locator("#profile-import-errors");
        await Assertions.Expect(errors).ToContainTextAsync("header.fullName");
        await Assertions.Expect(errors).ToContainTextAsync("header.email");
        await Assertions.Expect(Page.Locator("#p-fullName")).ToHaveValueAsync("");
    }

    [Fact]
    public async Task Blank_template_downloads()
    {
        await OpenProfilePageAsync();

        var download = await Page.RunAndWaitForDownloadAsync(() => Page.Locator("#profile-template").ClickAsync());
        Assert.EndsWith(".json", download.SuggestedFilename);
    }
}

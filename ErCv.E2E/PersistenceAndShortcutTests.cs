using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace ErCv.E2E
{
    public class PersistenceAndShortcutTests : E2ETestBase
    {
        public PersistenceAndShortcutTests(AppFixture app) : base(app)
        {
        }

        private ILocator Radio(string name) => Page.GetByRole(AriaRole.Radio, new() { Name = name });

        [Fact]
        public async Task Settings_and_history_survive_a_reload()
        {
            await OpenHomeAsync();
            await PickSampleAsync("Backend");

            await NextAsync();
            await ChooseCardAsync("Modern");
            await NextAsync();
            await ChooseCardAsync("Up to 2 Pages");
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "PDF" }), false);
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
            await Button("Generate CV").ClickAsync();
            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();

            await OpenHomeAsync();

            // The job description itself is not kept, the settings are
            await PickSampleAsync("Junior");
            await NextAsync();
            await Assertions.Expect(Radio("Modern")).ToBeCheckedAsync();
            await NextAsync();
            await Assertions.Expect(Radio("Up to 2 Pages")).ToBeCheckedAsync();
            await Assertions.Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" })).ToBeCheckedAsync();
            await Assertions.Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = "PDF" })).Not.ToBeCheckedAsync();

            await Button("History", exact: true).ClickAsync();
            var entry = Page.Locator(".history-item");
            await Assertions.Expect(entry).ToHaveCountAsync(1);
            await Assertions.Expect(entry).ToContainTextAsync("Template: Modern");

            await entry.GetByRole(AriaRole.Button, new() { Name = "Re-open" }).ClickAsync();
            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Reopening_a_past_generation_restores_its_options_and_generates_again()
        {
            await OpenHomeAsync();
            await PickSampleAsync("Backend");
            await NextAsync();
            await ChooseCardAsync("Modern");
            await NextAsync();
            await ChooseCardAsync("Up to 2 Pages");
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "PDF" }), false);
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
            await Button("Generate CV").ClickAsync();
            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();

            // Come back, change everything, then re-open the old generation
            await OpenHomeAsync();
            await PickSampleAsync("Junior");
            await NextAsync();
            await ChooseCardAsync("Harvard");
            await NextAsync();
            await ChooseCardAsync("1 Page");
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), false);
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "PDF" }), true);

            await Button("History", exact: true).ClickAsync();
            await Page.Locator(".history-item").GetByRole(AriaRole.Button, new() { Name = "Re-open" }).ClickAsync();
            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
            await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Download DOCX" })).ToBeVisibleAsync();
            await Assertions.Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Download PDF" })).ToHaveCountAsync(0);

            await OpenHomeAsync();
            await PickSampleAsync("Backend");
            await NextAsync();
            await Assertions.Expect(Radio("Modern")).ToBeCheckedAsync();
            await NextAsync();
            await Assertions.Expect(Radio("Up to 2 Pages")).ToBeCheckedAsync();
        }

        [Fact]
        public async Task Reopening_an_entry_that_used_an_imported_profile_falls_back_to_the_demo_profile()
        {
            await OpenHomeAsync();
            await PickSampleAsync("Backend");
            await NextAsync();
            var profile = """{ "header": { "fullName": "Zed Tester", "email": "zed@example.com" }, "experience": [ { "role": "Eng", "company": "Acme", "startDate": "Jan 2020", "bullets": ["Built a thing"], "skills": ["C#"] } ] }""";
            await Page.Locator("#profile-import").SetInputFilesAsync(new FilePayload
            {
                Name = "p.json", MimeType = "application/json", Buffer = System.Text.Encoding.UTF8.GetBytes(profile)
            });
            await Assertions.Expect(Page.Locator("#profile-active")).ToBeVisibleAsync();
            await NextAsync();
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
            await Button("Generate CV").ClickAsync();
            await Assertions.Expect(PreviewBody).ToContainTextAsync("Tester");

            await OpenHomeAsync();
            await Button("History", exact: true).ClickAsync();
            await Page.Locator(".history-item").GetByRole(AriaRole.Button, new() { Name = "Re-open" }).ClickAsync();

            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
            await Assertions.Expect(PreviewBody).ToContainTextAsync("Rivera");
            await Assertions.Expect(Page.GetByText("imported profile was not kept")).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Ctrl_enter_with_an_empty_job_description_shows_the_inline_error()
        {
            await OpenHomeAsync();

            await Page.Keyboard.PressAsync("Control+Enter");

            await Assertions.Expect(Page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "Paste a job description" })).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Ctrl_shift_v_pastes_the_clipboard_and_ctrl_enter_generates()
        {
            await OpenHomeAsync();

            // Word only, so no PDF engine is involved
            await PickSampleAsync("Backend");
            await GoToOptionsAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Job description" }).ClickAsync();
            await JobDescription.FillAsync("");

            const string pasted = "Platform Engineer. Go, Kafka and Kubernetes on AWS.";
            await Page.EvaluateAsync("text => navigator.clipboard.writeText(text)", pasted);
            await Page.Locator("h1").ClickAsync();
            await Page.Keyboard.PressAsync("Control+Shift+V");

            await Assertions.Expect(JobDescription).ToHaveValueAsync(pasted);

            await Page.Keyboard.PressAsync("Control+Enter");
            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        }
    }
}

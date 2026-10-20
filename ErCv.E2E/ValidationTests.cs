using Microsoft.Playwright;

namespace ErCv.E2E
{
    public class ValidationTests : E2ETestBase
    {
        public ValidationTests(AppFixture app) : base(app)
        {
        }

        [Fact]
        public async Task Empty_job_description_shows_an_inline_error_and_focuses_the_field()
        {
            await OpenHomeAsync();

            await NextAsync();

            var error = Page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "Paste a job description" });
            await Assertions.Expect(error).ToBeVisibleAsync();
            await Assertions.Expect(JobDescription).ToHaveAttributeAsync("aria-invalid", "true");
            await Assertions.Expect(JobDescription).ToBeFocusedAsync();
            await Assertions.Expect(Page.Locator(".alert-danger")).ToHaveCountAsync(0);

            await JobDescription.PressSequentiallyAsync("Go engineer");
            await Assertions.Expect(error).ToHaveCountAsync(0);
        }

        [Fact]
        public async Task No_format_selected_shows_an_inline_error_instead_of_generating()
        {
            await OpenHomeAsync();
            await PickSampleAsync("Frontend");
            await GoToOptionsAsync(pdf: false, word: false);

            await Button("Generate CV").ClickAsync();

            var error = Page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "Select at least one format" });
            await Assertions.Expect(error).ToBeVisibleAsync();
            await Assertions.Expect(ResultsRegion).ToHaveCountAsync(0);

            await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }).CheckAsync();
            await Assertions.Expect(error).ToHaveCountAsync(0);
        }

        [Fact]
        public async Task Empty_history_shows_a_friendly_empty_state()
        {
            await OpenHomeAsync();

            await Button("History", exact: true).ClickAsync();

            await Assertions.Expect(Page.GetByText("No generations yet")).ToBeVisibleAsync();
            await Button("Create your first CV").ClickAsync();
            await Assertions.Expect(Page.GetByText("No generations yet")).ToHaveCountAsync(0);
        }
    }
}

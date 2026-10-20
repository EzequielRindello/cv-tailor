using System.Text;
using Microsoft.Playwright;

namespace ErCv.E2E
{
    public class ProfileHealthTests : E2ETestBase
    {
        public ProfileHealthTests(AppFixture app) : base(app)
        {
        }

        private const string WeakProfile = """
            {
              "header": { "fullName": "Zed Tester", "email": "zed@example.com" },
              "experience": [
                { "role": "Engineer", "company": "Acme", "startDate": "Jan 2020", "endDate": "Present",
                  "bullets": ["Responsible for the billing service"], "skills": [] }
              ]
            }
            """;

        [Fact]
        public async Task Checklist_is_shown_before_generating_and_lists_what_to_improve()
        {
            await OpenHomeAsync();
            await PickSampleAsync("Backend");
            await NextAsync();

            await Page.Locator("#profile-import").SetInputFilesAsync(new FilePayload
            {
                Name = "profile.json",
                MimeType = "application/json",
                Buffer = Encoding.UTF8.GetBytes(WeakProfile)
            });
            await Assertions.Expect(Page.Locator("#profile-active")).ToContainTextAsync("Zed Tester");
            await NextAsync();

            var card = Page.Locator("#profile-health");
            await Assertions.Expect(card).ToBeVisibleAsync();
            await Assertions.Expect(card.Locator("[data-rule='NoMetrics']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("warn"));
            await Assertions.Expect(card.Locator("[data-rule='WeakVerb']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("warn"));
            await Assertions.Expect(card.Locator("[data-rule='NoSkillTags']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("warn"));
            await Assertions.Expect(card.Locator("[data-rule='Dates']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("ok"));

            await card.Locator("[data-rule='WeakVerb'] summary").ClickAsync();
            await Assertions.Expect(card.Locator("[data-rule='WeakVerb'] li")).ToContainTextAsync("Responsible");

            // Advice only: the CV still generates
            await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
            await Button("Generate CV").ClickAsync();
            await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();
        }

        [Fact]
        public async Task Profile_editor_can_check_the_health_of_the_form()
        {
            await Page.GotoAsync("/profile");
            await Assertions.Expect(Page.Locator("#profile-health-check")).ToBeVisibleAsync();

            await Page.Locator("#profile-health-check").ClickAsync();

            await Assertions.Expect(Page.Locator("#profile-health [data-rule]")).ToHaveCountAsync(4);
        }
    }
}

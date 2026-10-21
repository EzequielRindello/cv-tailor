using Microsoft.Playwright;

namespace ErCv.E2E;

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
    public async Task Step_three_does_not_show_the_health_check()
    {
        await SaveMyProfileFromJsonAsync(WeakProfile);
        await ChooseMyProfileAsync();
        await NextAsync();

        await Assertions.Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" })).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator("#profile-health")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Editor_checklist_lists_what_to_improve_and_is_advice_only()
    {
        await SaveMyProfileFromJsonAsync(WeakProfile);

        await Page.Locator("#profile-health-check").ClickAsync();
        var card = Page.Locator("#profile-health");
        await Assertions.Expect(card).ToBeVisibleAsync();
        await Assertions.Expect(card.Locator("[data-rule='NoMetrics']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("warn"));
        await Assertions.Expect(card.Locator("[data-rule='WeakVerb']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("warn"));
        await Assertions.Expect(card.Locator("[data-rule='NoSkillTags']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("warn"));
        await Assertions.Expect(card.Locator("[data-rule='Dates']")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("ok"));
        await Assertions.Expect(card.Locator(".profile-health-badge")).ToHaveTextAsync("3 to improve");

        await card.Locator("[data-rule='WeakVerb'] summary").ClickAsync();
        await Assertions.Expect(card.Locator("[data-rule='WeakVerb'] li")).ToContainTextAsync("Responsible");

        // Advice only: the profile saved and the CV still generates from it
        await ChooseMyProfileAsync();
        await NextAsync();
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
        // The button takes you to the card: it is scrolled into view and holds the focus
        await Assertions.Expect(Page.Locator("#profile-health")).ToBeInViewportAsync();
        await Assertions.Expect(Page.Locator("#profile-health")).ToBeFocusedAsync();
    }
}

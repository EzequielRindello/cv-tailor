using System.Text.RegularExpressions;
using ErCv.Web.Features.Matching;
using Microsoft.Playwright;

namespace ErCv.E2E;

public class GenerateFlowTests : E2ETestBase
{
    public GenerateFlowTests(AppFixture app) : base(app)
    {
    }

    [Fact]
    public async Task Sample_jd_generates_a_preview_and_scores()
    {
        await GenerateAsync("Backend");

        await Assertions.Expect(PreviewBody).ToContainTextAsync("Rivera", new() { IgnoreCase = true });

        var match = Page.GetByRole(AriaRole.Img, new() { Name = "Match score" });
        await Assertions.Expect(match).ToBeVisibleAsync();
        Assert.Matches(@"Match score \d{1,3} out of 100", await match.GetAttributeAsync("aria-label") ?? "");

        await Page.GetByRole(AriaRole.Tab, new() { Name = "ATS" }).ClickAsync();
        await Assertions.Expect(Page.GetByRole(AriaRole.Img, new() { Name = "ATS score" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Tab, new() { Name = "Gaps" }).ClickAsync();
        await Assertions.Expect(Page.Locator(".missing-skills-card")).ToBeVisibleAsync();
    }

    // PROF-06: the senior sample is the first choice and the default; the creator's real profile is the second
    [Fact]
    public async Task The_senior_sample_is_the_default_and_the_creators_profile_can_be_chosen()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();

        var choices = Page.Locator(".profile-source input[type=radio]");
        await Assertions.Expect(choices.First).ToHaveIdAsync("source-cv-master-senior");
        await Assertions.Expect(choices.First).ToBeCheckedAsync();

        await Page.Locator("label[for='source-demo']").ClickAsync();
        await NextAsync();
        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);
        await Button("Generate CV").ClickAsync();

        await Assertions.Expect(PreviewBody).ToContainTextAsync("Rindello");
    }

    // APP-05: the samples are real posts, so the picker says whose they are, when they were posted and links back
    [Fact]
    public async Task Picking_a_sample_shows_where_and_when_it_was_posted_with_a_link_to_the_original()
    {
        var sample = SampleJobDescriptions.All.Single(s => s.Label == "Backend");

        await OpenHomeAsync();
        await PickSampleAsync("Backend");

        var source = Page.Locator(".sample-picker-source");
        await Assertions.Expect(source).ToContainTextAsync(sample.Company);
        await Assertions.Expect(source).ToContainTextAsync(sample.PostedAt.ToString("d MMM yyyy", System.Globalization.CultureInfo.InvariantCulture));
        var link = source.GetByRole(AriaRole.Link, new() { Name = "View the original post" });
        await Assertions.Expect(link).ToHaveAttributeAsync("href", sample.Url);
        await Assertions.Expect(link).ToHaveAttributeAsync("target", "_blank");
        await Assertions.Expect(JobDescription).ToHaveValueAsync(new Regex(Regex.Escape(sample.Title)));
    }

    [Fact]
    public async Task Why_tab_explains_each_role_and_bullet_with_its_score_parts()
    {
        await GenerateAsync("Backend");
        await Page.GetByRole(AriaRole.Tab, new() { Name = "Why" }).ClickAsync();

        var firstRole = Page.Locator(".why-item").First;
        await Assertions.Expect(firstRole).ToBeVisibleAsync();

        await firstRole.Locator("summary").ClickAsync();
        await Assertions.Expect(firstRole.Locator(".why-parts li").First).ToBeVisibleAsync();
        await Assertions.Expect(firstRole.Locator(".why-bullet.included").First).ToBeVisibleAsync();
        await Assertions.Expect(firstRole.Locator(".why-chip").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Why_tab_card_uses_the_same_surface_as_the_other_tabs_in_dark_mode()
    {
        await GenerateAsync("Backend");
        await Page.EvaluateAsync("() => document.documentElement.setAttribute('data-theme', 'dark')");
        // Colors are read from the computed style, so no transition may be halfway through the theme change
        await Page.AddStyleTagAsync(new() { Content = "*, *::before, *::after { transition: none !important; }" });

        await Page.GetByRole(AriaRole.Tab, new() { Name = "Keywords" }).ClickAsync();
        var reference = await Page.Locator(".keyword-card").EvaluateAsync<string>("e => getComputedStyle(e).backgroundColor");

        foreach (var (tab, card) in new[] { ("Why", ".why-card"), ("Job", ".jd-card") })
        {
            await Page.GetByRole(AriaRole.Tab, new() { Name = tab }).ClickAsync();
            var background = await Page.Locator(card).EvaluateAsync<string>("e => getComputedStyle(e).backgroundColor");
            Assert.True(reference == background, $"{tab}: card is {background}, keyword card is {reference}");
        }
    }

    [Fact]
    public async Task Job_tab_highlights_skills_and_shows_where_one_appears_in_the_cv()
    {
        await GenerateAsync("Backend");
        await Page.GetByRole(AriaRole.Tab, new() { Name = "Job" }).ClickAsync();

        await Assertions.Expect(Page.Locator(".jd-term.matched").First).ToBeVisibleAsync();

        var skill = Page.Locator(".jd-term.matched").First;
        await skill.ClickAsync();
        await Assertions.Expect(skill).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(Page.Locator(".jd-detail-title")).ToBeVisibleAsync();
        await Assertions.Expect(Page.Locator(".jd-locations li, .jd-note").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Generate_announces_the_result_and_moves_focus_to_the_results()
    {
        await GenerateAsync();

        await Assertions.Expect(Page.Locator("[aria-live=polite]").Filter(new() { HasTextRegex = new Regex("CV ready\\. Match score") }))
            .ToHaveCountAsync(1);
        await Assertions.Expect(ResultsRegion).ToBeFocusedAsync();
    }

    [Fact]
    public async Task Generating_does_not_download_anything_until_a_button_is_clicked()
    {
        var downloads = 0;
        Page.Download += (_, _) => downloads++;

        await GenerateAsync();
        await Page.WaitForTimeoutAsync(1000);
        Assert.Equal(0, downloads);

        var download = await Page.RunAndWaitForDownloadAsync(() => Button("Download DOCX").ClickAsync());
        Assert.EndsWith(".docx", download.SuggestedFilename);
        await Assertions.Expect(Page.Locator(".app-toast").Filter(new() { HasText = "Download started" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Toasts_appear_at_the_top_right_below_the_header()
    {
        await GenerateAsync();

        var toast = Page.Locator(".app-toast").Filter(new() { HasText = "CV ready" });
        await Assertions.Expect(toast).ToBeVisibleAsync();

        var box = (await toast.BoundingBoxAsync())!;
        var header = (await Page.Locator("#app-header").BoundingBoxAsync())!;
        var viewport = Page.ViewportSize!;
        Assert.True(box.Y >= header.Y + header.Height, "the toast overlaps the header");
        Assert.True(box.Y < viewport.Height / 3, "the toast is not at the top");
        Assert.True(viewport.Width - (box.X + box.Width) < 40, "the toast is not at the right edge");
    }

    [Fact]
    public async Task Progress_stages_are_shown_in_order_while_generating()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Data");
        await GoToOptionsAsync();

        await Page.EvaluateAsync(@"() => {
                window.__stages = [];
                new MutationObserver(() => {
                    const label = document.querySelector('.gen-step.current .gen-step-label')?.textContent;
                    if (label && window.__stages[window.__stages.length - 1] !== label) window.__stages.push(label);
                }).observe(document.body, { subtree: true, childList: true, attributes: true, characterData: true });
            }");

        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();

        var stages = await Page.EvaluateAsync<string[]>("window.__stages");
        var canonical = new[] { "Parsing", "Scoring", "Selecting", "Rendering" };

        Assert.True(stages.Length >= 2, $"Expected several stages, saw: {string.Join(", ", stages)}");
        Assert.All(stages, s => Assert.Contains(s, canonical));
        Assert.Equal(stages.OrderBy(s => Array.IndexOf(canonical, s)), stages);
    }

    [Fact]
    public async Task Back_to_the_composer_keeps_the_job_description_and_start_over_clears_it()
    {
        await GenerateAsync("DevOps");

        await Button("Back to composer").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Job description" }).ClickAsync();
        var devops = SampleJobDescriptions.All.Single(s => s.Label == "DevOps");
        await Assertions.Expect(JobDescription).ToHaveValueAsync(new Regex(Regex.Escape(devops.Title)));

        await Page.GetByRole(AriaRole.Button, new() { Name = "Options" }).ClickAsync();
        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();

        await Button("Start over").ClickAsync();
        await Assertions.Expect(JobDescription).ToHaveValueAsync("");
    }
}

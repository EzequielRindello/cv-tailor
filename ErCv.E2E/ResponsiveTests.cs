using Microsoft.Playwright;

namespace ErCv.E2E
{
    /// <summary>The same checks at each viewport; the concrete classes below only pick the size.</summary>
    public abstract class ResponsiveTestsBase : E2ETestBase
    {
        protected ResponsiveTestsBase(AppFixture app) : base(app)
        {
        }

        [Fact]
        public async Task Composer_has_no_horizontal_scroll_on_any_step()
        {
            await OpenHomeAsync();
            Assert.False(await HasHorizontalScrollAsync(), "hero and step 1");

            await PickSampleAsync("Full stack");
            Assert.False(await HasHorizontalScrollAsync(), "step 1 with a job description");

            await NextAsync();
            Assert.False(await HasHorizontalScrollAsync(), "step 2");

            await NextAsync();
            Assert.False(await HasHorizontalScrollAsync(), "step 3");

            await Button("History", exact: true).ClickAsync();
            Assert.False(await HasHorizontalScrollAsync(), "history panel");
        }

        [Fact]
        public async Task Results_have_no_horizontal_scroll_and_show_one_pane_at_a_time()
        {
            await GenerateAsync();
            Assert.False(await HasHorizontalScrollAsync(), "preview pane");

            // Below the desktop breakpoint the preview and the insights are separate panes
            var insightsTab = Page.GetByRole(AriaRole.Tab, new() { Name = "Match" });
            await Assertions.Expect(insightsTab).Not.ToBeVisibleAsync();

            await Button("Insights", exact: true).ClickAsync();
            await Assertions.Expect(insightsTab).ToBeVisibleAsync();
            Assert.False(await HasHorizontalScrollAsync(), "insights pane");

            foreach (var tab in new[] { "ATS", "Keywords", "Gaps" })
            {
                await Page.GetByRole(AriaRole.Tab, new() { Name = tab }).ClickAsync();
                Assert.False(await HasHorizontalScrollAsync(), $"{tab} tab");
            }

            await Page.GetByRole(AriaRole.Button, new() { Name = "Edit", Exact = true }).ClickAsync();
            await Assertions.Expect(Page.Locator(".bullet-editor-card")).ToBeVisibleAsync();
            Assert.False(await HasHorizontalScrollAsync(), "bullet editor");
        }

        [Fact]
        public async Task Actions_stay_reachable_without_overflowing_the_action_bar()
        {
            await GenerateAsync();

            var bar = Page.GetByRole(AriaRole.Toolbar, new() { Name = "CV actions" });
            foreach (var name in new[] { "Back to composer", "Download DOCX", "Edit", "Start over" })
                await Assertions.Expect(bar.GetByRole(AriaRole.Button, new() { Name = name, Exact = true })).ToBeVisibleAsync();
        }
    }

    public class Mobile390Tests : ResponsiveTestsBase
    {
        public Mobile390Tests(AppFixture app) : base(app)
        {
        }

        protected override ViewportSize Viewport => new() { Width = 390, Height = 844 };
    }

    public class Mobile360Tests : ResponsiveTestsBase
    {
        public Mobile360Tests(AppFixture app) : base(app)
        {
        }

        protected override ViewportSize Viewport => new() { Width = 360, Height = 740 };
    }

    public class Tablet768Tests : ResponsiveTestsBase
    {
        public Tablet768Tests(AppFixture app) : base(app)
        {
        }

        protected override ViewportSize Viewport => new() { Width = 768, Height = 1024 };
    }
}

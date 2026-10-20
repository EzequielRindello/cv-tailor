using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace ErCv.E2E
{
    public class AccessibilityTests : E2ETestBase
    {
        private static readonly string[] BlockingImpacts = { "serious", "critical" };

        public AccessibilityTests(AppFixture app) : base(app)
        {
        }

        private async Task AssertNoSeriousViolationsAsync(string where)
        {
            foreach (var theme in new[] { "light", "dark" })
            {
                await Page.EvaluateAsync("t => document.documentElement.setAttribute('data-theme', t)", theme);

                // Colors are read from the computed style, so no transition or animation may be halfway through a theme change
                await Page.AddStyleTagAsync(new()
                {
                    Content = "*, *::before, *::after { transition: none !important; animation: none !important; }"
                });

                // The CV preview is the generated document with its own template styles, not part of the app UI
                var results = await Page.RunAxe(
                    new AxeRunContext { Exclude = new List<AxeSelector> { new("iframe[title='CV Preview']") } },
                    new AxeRunOptions
                    {
                        RunOnly = new RunOnlyOptions
                        {
                            Type = "tag",
                            Values = new List<string> { "wcag2a", "wcag2aa", "wcag21a", "wcag21aa" }
                        }
                    });

                // Guards against a scan that silently checked nothing
                Assert.True(results.Passes.Length > 10, $"axe evaluated suspiciously few rules on {where} ({theme})");

                var blocking = results.Violations
                    .Where(v => BlockingImpacts.Contains(v.Impact))
                    .Select(v => $"{v.Id} ({v.Impact}): {v.Help} [{string.Join(" | ", v.Nodes.Select(n => n.Target.ToString()).Take(3))}]")
                    .ToList();

                Assert.True(blocking.Count == 0, $"{where} ({theme}) has accessibility violations:\n" + string.Join("\n", blocking));
            }
        }

        [Fact]
        public async Task Composer_steps_have_no_serious_violations()
        {
            await OpenHomeAsync();
            await AssertNoSeriousViolationsAsync("step 1");

            await PickSampleAsync("Backend");
            await AssertNoSeriousViolationsAsync("step 1 with a job description");

            await NextAsync();
            await AssertNoSeriousViolationsAsync("step 2");

            await NextAsync();
            await AssertNoSeriousViolationsAsync("step 3");
        }

        [Fact]
        public async Task Validation_errors_and_the_history_panel_have_no_serious_violations()
        {
            await OpenHomeAsync();

            await NextAsync();
            await Assertions.Expect(Page.GetByRole(AriaRole.Alert).Filter(new() { HasText = "Paste a job description" })).ToBeVisibleAsync();
            await AssertNoSeriousViolationsAsync("inline error on step 1");

            await Button("History", exact: true).ClickAsync();
            await AssertNoSeriousViolationsAsync("empty history");
        }

        [Fact]
        public async Task Results_workspace_has_no_serious_violations()
        {
            await GenerateAsync();
            await AssertNoSeriousViolationsAsync("results, match tab");

            foreach (var tab in new[] { "ATS", "Keywords", "Gaps" })
            {
                await Page.GetByRole(AriaRole.Tab, new() { Name = tab }).ClickAsync();
                await AssertNoSeriousViolationsAsync($"results, {tab} tab");
            }

            await Page.GetByRole(AriaRole.Tab, new() { Name = "Why" }).ClickAsync();
            await Page.Locator(".why-item > summary").First.ClickAsync();
            await AssertNoSeriousViolationsAsync("results, Why tab expanded");

            await Button("Edit", exact: true).ClickAsync();
            await Assertions.Expect(Page.Locator(".bullet-editor-card")).ToBeVisibleAsync();
            await AssertNoSeriousViolationsAsync("bullet editor");
        }

        [Fact]
        public async Task Toasts_have_no_serious_violations()
        {
            await GenerateAsync();
            await Button("Download DOCX").ClickAsync();
            await Assertions.Expect(Page.Locator(".app-toast")).ToBeVisibleAsync();
            await AssertNoSeriousViolationsAsync("download toast");
        }
    }
}

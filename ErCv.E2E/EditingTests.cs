using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Playwright;

namespace ErCv.E2E;

// EDIT-01 and EDIT-02: editing in the workspace, with the preview following every change
public class EditingTests : E2ETestBase
{
    private const string Rewrite = "Rewritten in the browser for this job";

    // More than the sample profile has to add back
    private const int MaxAddClicks = 60;

    public EditingTests(AppFixture app) : base(app)
    {
    }

    private ILocator Editor => Page.Locator(".bullet-editor-card");

    private async Task OpenEditorAsync()
    {
        await Button("Edit", exact: true).ClickAsync();
        await Assertions.Expect(Editor).ToBeVisibleAsync();
    }

    private async Task RewriteFirstBulletAsync()
    {
        await Page.Locator("#bullet-edit-btn-0-0").ClickAsync();
        await Page.Locator("#bullet-edit-0-0").FillAsync(Rewrite);
        await Editor.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();
    }

    [Fact]
    public async Task Unchecking_a_bullet_updates_the_preview_without_regenerating()
    {
        await GenerateAsync();
        await OpenEditorAsync();
        var firstBullet = (await Page.Locator("#bullet-text-0-0").InnerTextAsync()).Trim();
        await Assertions.Expect(PreviewBody).ToContainTextAsync(firstBullet);

        await Page.Locator("#bullet-0-0").UncheckAsync();

        await Assertions.Expect(PreviewBody).Not.ToContainTextAsync(firstBullet, new() { Timeout = 2000 });
        await Assertions.Expect(Button("Regenerate CV")).ToHaveCountAsync(0);
        await Assertions.Expect(Editor).ToBeVisibleAsync();
    }

    [Fact]
    public async Task A_rewritten_bullet_shows_in_the_preview_and_is_marked_as_edited()
    {
        await GenerateAsync();
        await OpenEditorAsync();

        await RewriteFirstBulletAsync();

        await Assertions.Expect(PreviewBody).ToContainTextAsync(Rewrite, new() { Timeout = 2000 });
        await Assertions.Expect(Page.Locator("#bullet-text-0-0")).ToContainTextAsync("Edited");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Undo rewrite" }).ClickAsync();
        await Assertions.Expect(PreviewBody).Not.ToContainTextAsync(Rewrite);
    }

    // EDIT-07: a skill the CV left out comes back from the profile and shows in the preview
    [Fact]
    public async Task A_skill_the_cv_left_out_can_be_added_back()
    {
        await GenerateAsync();
        await OpenEditorAsync();
        await Page.Locator("#editor-leftout-skills summary").ClickAsync();
        var add = Page.Locator("#editor-leftout-skills button").First;
        var skill = (await add.InnerTextAsync()).Trim();
        await Assertions.Expect(PreviewBody).Not.ToContainTextAsync(skill);

        await add.ClickAsync();

        await Assertions.Expect(PreviewBody).ToContainTextAsync(skill, new() { Timeout = 5000 });
        await Assertions.Expect(Page.Locator(".cv-editor-chip", new() { HasText = skill }).Locator(".bullet-added-badge")).ToBeVisibleAsync();
    }

    // EDIT-07: nothing is left out for the visitor; adding more than fits shows the page count instead
    [Fact]
    public async Task Adding_more_than_fits_shows_the_page_count_and_trims_nothing()
    {
        await GenerateAsync();
        await OpenEditorAsync();
        var warning = Page.Locator("#editor-page-warning");
        await Assertions.Expect(warning).ToHaveCountAsync(0);

        // Add back every experience, bullet and project that was left out (each click re-renders the lists)
        for (var i = 0; i < MaxAddClicks; i++)
        {
            var clicked = await Page.EvaluateAsync<bool>(
                "() => { const b = document.querySelector('.cv-editor-leftout li button'); if (!b) return false; b.click(); return true; }");
            if (!clicked) break;
            await Page.WaitForTimeoutAsync(150);
        }

        await Assertions.Expect(warning).ToContainTextAsync("pages", new() { Timeout = 15000 });
    }

    // EDIT-08: the scores follow the edited CV, after a short pause, and say so while they are being recomputed
    [Fact]
    public async Task The_match_follows_the_edited_cv()
    {
        await GenerateAsync();
        var skills = Page.Locator("[id^=insights-panel] .breakdown-item", new() { HasText = "Technical skills" });
        var before = (await skills.InnerTextAsync()).Trim();

        await OpenEditorAsync();
        var chips = Page.Locator(".cv-editor-chips").First.Locator(".cv-editor-chip.included");
        for (var i = await chips.CountAsync() - 1; i >= 0; i--)
            await chips.Nth(i).ClickAsync();
        await Editor.GetByRole(AriaRole.Button, new() { Name = "Done", Exact = true }).ClickAsync();

        var panel = Page.Locator("#insights-panel");
        await Assertions.Expect(panel).ToHaveAttributeAsync("aria-busy", "false", new() { Timeout = 10000 });
        await Assertions.Expect(skills).Not.ToHaveTextAsync(before, new() { Timeout = 10000 });
    }

    // Moving a section changes nothing the analysis reads, so no "Updating" state appears
    [Fact]
    public async Task Moving_a_section_does_not_recompute_the_scores()
    {
        await GenerateAsync();
        await OpenEditorAsync();

        await Page.Locator("#section-up-experience").ClickAsync();
        await Editor.GetByRole(AriaRole.Button, new() { Name = "Done", Exact = true }).ClickAsync();

        await Assertions.Expect(Page.Locator(".insights-updating")).ToHaveCountAsync(0);
        await Assertions.Expect(Page.Locator("#insights-panel")).ToHaveAttributeAsync("aria-busy", "false");
    }

    [Fact]
    public async Task Moving_a_section_reorders_the_preview()
    {
        await GenerateAsync();
        await OpenEditorAsync();

        // Harvard prints Education before Experience; one move up puts Experience first
        await Page.Locator("#section-up-experience").ClickAsync();

        // The frame reloads on the change, so this waits for the new order instead of reading it once
        var titles = Page.FrameLocator("iframe[title='CV Preview']").Locator(".cv-section-title");
        await Assertions.Expect(titles).ToContainTextAsync(new[]
        {
            new Regex("^Experience$", RegexOptions.IgnoreCase), new Regex("^Education$", RegexOptions.IgnoreCase)
        });
    }

    [Fact]
    public async Task The_preview_keeps_its_scroll_position_when_an_edit_reloads_it()
    {
        await GenerateAsync();
        await OpenEditorAsync();
        var frame = Page.Frames.Single(f => f.ParentFrame != null);
        // The marker tells the reloaded document apart from the one scrolled here. 100 px stays inside the scroll
        // range even after the edit makes the CV a little shorter.
        await frame.EvaluateAsync("() => { window.scrollTo(0, 100); window.__beforeEdit = true; }");
        await Page.WaitForFunctionAsync("() => document.querySelector(\"iframe[title='CV Preview']\").contentWindow.scrollY > 0");

        await Page.Locator(".cv-editor-chip").First.ClickAsync();

        await Page.WaitForFunctionAsync(
            "() => { const view = document.querySelector(\"iframe[title='CV Preview']\").contentWindow; return !view.__beforeEdit && view.scrollY >= 90; }");
    }

    [Fact]
    public async Task The_docx_downloaded_after_an_edit_holds_the_edit()
    {
        await GenerateAsync();
        await OpenEditorAsync();
        await RewriteFirstBulletAsync();
        await Assertions.Expect(PreviewBody).ToContainTextAsync(Rewrite);

        var download = await Page.RunAndWaitForDownloadAsync(() => Button("Download DOCX").ClickAsync());

        await using var stream = await download.CreateReadStreamAsync();
        using var memory = new MemoryStream();
        await stream!.CopyToAsync(memory);
        memory.Position = 0;
        using var doc = WordprocessingDocument.Open(memory, false);
        Assert.Contains(Rewrite, doc.MainDocumentPart!.Document!.Body!.InnerText);
    }

    [Fact]
    public async Task Edits_survive_closing_and_reopening_the_editor_and_undo_all_clears_them()
    {
        await GenerateAsync();
        await OpenEditorAsync();
        await RewriteFirstBulletAsync();

        await Editor.GetByRole(AriaRole.Button, new() { Name = "Done", Exact = true }).ClickAsync();
        await Assertions.Expect(Editor).ToHaveCountAsync(0);
        await Assertions.Expect(PreviewBody).ToContainTextAsync(Rewrite);

        await OpenEditorAsync();
        await Assertions.Expect(Page.Locator("#bullet-text-0-0")).ToContainTextAsync(Rewrite);

        await Editor.GetByRole(AriaRole.Button, new() { Name = "Undo all changes" }).ClickAsync();
        await Assertions.Expect(PreviewBody).Not.ToContainTextAsync(Rewrite);
    }
}

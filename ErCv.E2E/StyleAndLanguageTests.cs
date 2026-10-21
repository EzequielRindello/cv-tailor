using Microsoft.Playwright;

namespace ErCv.E2E;

// UX-10 (real thumbnails, accent and fonts), EDIT-05 (two new templates) and EDIT-04 (CV language)
public class StyleAndLanguageTests : E2ETestBase
{
    public StyleAndLanguageTests(AppFixture app) : base(app)
    {
    }

    private ILocator Thumbnails => Page.Locator(".template-preview iframe");

    [Fact]
    public async Task The_template_picker_shows_a_real_thumbnail_of_each_template()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();

        await Assertions.Expect(Page.Locator("label.template-card")).ToHaveCountAsync(5);
        await Assertions.Expect(Thumbnails).ToHaveCountAsync(5);
        var compact = Page.FrameLocator("iframe[title='Compact sample']");
        await Assertions.Expect(compact.Locator(".cv-section-title").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Picking_an_accent_restyles_the_thumbnails()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();

        await Page.Locator("label.accent-swatch").Filter(new() { HasText = "Teal" }).ClickAsync();

        await Assertions.Expect(Page.FrameLocator("iframe[title='Harvard sample']").Locator("body.accent-teal")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task A_spanish_executive_cv_has_spanish_titles_in_the_preview()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();
        await ChooseCardAsync("Executive");
        await NextAsync();
        await ChooseCardAsync("Español");
        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "PDF" }), false);
        await SetCheckedAsync(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Word" }), true);

        await Button("Generate CV").ClickAsync();
        await Assertions.Expect(ResultsRegion).ToBeVisibleAsync();

        await Assertions.Expect(PreviewBody).ToContainTextAsync("Perfil ejecutivo");
        await Assertions.Expect(PreviewBody).ToContainTextAsync("Experiencia");
        await Assertions.Expect(Page.FrameLocator("iframe[title='CV Preview']").Locator("html[lang=es]")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task The_style_choices_are_remembered_after_a_reload()
    {
        await OpenHomeAsync();
        await PickSampleAsync("Backend");
        await NextAsync();
        await Page.Locator("label.font-option").Filter(new() { HasText = "Clean" }).ClickAsync();
        await Assertions.Expect(Page.Locator("label.font-option.selected")).ToContainTextAsync("Clean");

        await Page.ReloadAsync();
        await PickSampleAsync("Backend");
        await NextAsync();

        await Assertions.Expect(Page.Locator("label.font-option.selected")).ToContainTextAsync("Clean");
    }
}

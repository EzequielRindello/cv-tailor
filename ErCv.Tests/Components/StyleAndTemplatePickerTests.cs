using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Constants;
using ErCv.Web.UI;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class StyleAndTemplatePickerTests : BunitContext
{
    [Fact]
    public void The_picker_lists_five_templates_with_a_real_thumbnail_each()
    {
        var thumbnails = TemplateSelector.Templates.ToDictionary(t => t.Key, t => $"<html><head></head><body>{t.Name} sample</body></html>");

        var cut = Render<TemplateSelector>(p => p
            .Add(c => c.Value, UiConstants.TemplateExecutive)
            .Add(c => c.Thumbnails, thumbnails));

        var frames = cut.FindAll(".template-preview iframe");
        Assert.Equal(5, cut.FindAll(".template-card").Count);
        Assert.Equal(5, frames.Count);
        Assert.All(frames, f => Assert.Equal("", f.GetAttribute("sandbox")));
        Assert.Contains("Compact sample", frames[3].GetAttribute("srcdoc"));
        Assert.Contains("padding:0.5in", frames[3].GetAttribute("srcdoc"));
        Assert.Contains("Executive", cut.Find(".template-card.selected").TextContent);
    }

    [Fact]
    public void Without_thumbnails_the_cards_still_show_their_names()
    {
        var cut = Render<TemplateSelector>(p => p.Add(c => c.Value, UiConstants.TemplateHarvard));

        Assert.Empty(cut.FindAll("iframe"));
        Assert.Equal(5, cut.FindAll(".template-name").Count);
    }

    [Fact]
    public void Picking_an_accent_and_fonts_reports_their_keys()
    {
        string? accent = null, fonts = null;
        var cut = Render<StyleOptions>(p => p
            .Add(c => c.AccentChanged, (string a) => accent = a)
            .Add(c => c.FontsChanged, (string f) => fonts = f));

        cut.Find($"input[name=cv-accent][value={ApiConstants.AccentBurgundy}]").Change(true);
        cut.Find($"input[name=cv-fonts][value={ApiConstants.FontsClean}]").Change(true);

        Assert.Equal(ApiConstants.AccentBurgundy, accent);
        Assert.Equal(ApiConstants.FontsClean, fonts);
    }

    [Fact]
    public void Every_accent_and_font_pairing_the_service_accepts_is_offered()
    {
        var cut = Render<StyleOptions>();

        var accents = cut.FindAll("input[name=cv-accent]").Select(i => i.GetAttribute("value")).ToArray();
        var fonts = cut.FindAll("input[name=cv-fonts]").Select(i => i.GetAttribute("value")).ToArray();
        Assert.Equal(ApiConstants.Accents, accents);
        Assert.Equal(ApiConstants.FontPairings, fonts);
    }

    [Fact]
    public void The_language_picker_reports_spanish()
    {
        string? language = null;
        var cut = Render<LanguageOptions>(p => p.Add(c => c.ValueChanged, (string l) => language = l));

        cut.Find($"input[name=cv-language][value={ApiConstants.LanguageSpanish}]").Change(true);

        Assert.Equal(ApiConstants.LanguageSpanish, language);
    }
}

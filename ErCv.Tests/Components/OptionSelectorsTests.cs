using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Constants;
using ErCv.Web.UI;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class OptionSelectorsTests : BunitContext
{
    private static Dictionary<string, bool> Formats(bool pdf, bool docx) =>
        new() { [ApiConstants.FormatPdf] = pdf, [ApiConstants.FormatDocx] = docx };

    [Fact]
    public void The_format_selector_reports_the_format_that_was_toggled()
    {
        var toggled = new List<string>();
        var cut = Render<FormatSelector>(p => p
            .Add(c => c.Selected, Formats(pdf: true, docx: false))
            .Add(c => c.OnToggle, (string key) => toggled.Add(key)));

        cut.FindAll(".format-checkbox")[1].Change(true);
        cut.FindAll(".format-checkbox")[0].Change(false);

        Assert.Equal(new[] { ApiConstants.FormatDocx, ApiConstants.FormatPdf }, toggled);
        Assert.Contains("PDF", cut.Find(".format-card.selected").TextContent);
    }

    [Fact]
    public void The_format_error_shows_only_while_no_format_is_selected()
    {
        var none = Render<FormatSelector>(p => p
            .Add(c => c.Selected, Formats(pdf: false, docx: false))
            .Add(c => c.Error, "Pick a format."));
        var one = Render<FormatSelector>(p => p
            .Add(c => c.Selected, Formats(pdf: false, docx: true))
            .Add(c => c.Error, "Pick a format."));

        Assert.Contains("Pick a format.", none.Find("[role=alert]").TextContent);
        Assert.Empty(one.FindAll("[role=alert]"));
    }

    [Fact]
    public void A_disabled_format_selector_disables_its_boxes_and_reports_nothing()
    {
        var toggled = new List<string>();
        var cut = Render<FormatSelector>(p => p
            .Add(c => c.Selected, Formats(pdf: true, docx: false))
            .Add(c => c.Disabled, true)
            .Add(c => c.OnToggle, (string key) => toggled.Add(key)));

        Assert.All(cut.FindAll(".format-checkbox"), b => Assert.True(b.HasAttribute("disabled")));

        cut.FindAll(".format-checkbox")[1].Change(true);
        Assert.Empty(toggled);
    }

    [Fact]
    public void The_page_options_report_one_or_two_pages()
    {
        var chosen = new List<bool>();
        var cut = Render<PageOptions>(p => p.Add(c => c.AllowTwoPages, false)
            .Add(c => c.AllowTwoPagesChanged, (bool v) => chosen.Add(v)));

        Assert.Contains("1 Page", cut.Find(".page-option-card.selected").TextContent);

        cut.FindAll("input[name=page-mode]")[1].Change(true);
        cut.FindAll("input[name=page-mode]")[0].Change(true);

        Assert.Equal(new[] { true, false }, chosen);
    }

    [Fact]
    public void Disabled_page_options_report_nothing()
    {
        var chosen = new List<bool>();
        var cut = Render<PageOptions>(p => p.Add(c => c.Disabled, true)
            .Add(c => c.AllowTwoPagesChanged, (bool v) => chosen.Add(v)));

        Assert.All(cut.FindAll("input[name=page-mode]"), r => Assert.True(r.HasAttribute("disabled")));
        Assert.All(cut.FindAll(".page-option-card"), c => Assert.Contains("disabled", c.ClassList));

        cut.FindAll("input[name=page-mode]")[1].Change(true);
        Assert.Empty(chosen);
    }

    [Fact]
    public void The_template_selector_reports_the_picked_template()
    {
        var picked = new List<string>();
        var cut = Render<TemplateSelector>(p => p.Add(c => c.Value, UiConstants.TemplateHarvard)
            .Add(c => c.ValueChanged, (string v) => picked.Add(v)));

        cut.Find($"input[value={UiConstants.TemplateCompact}]").Change(true);

        Assert.Equal(new[] { UiConstants.TemplateCompact }, picked);
    }

    [Fact]
    public void A_disabled_template_selector_reports_nothing()
    {
        var picked = new List<string>();
        var cut = Render<TemplateSelector>(p => p.Add(c => c.Value, UiConstants.TemplateHarvard)
            .Add(c => c.Disabled, true)
            .Add(c => c.ValueChanged, (string v) => picked.Add(v)));

        Assert.All(cut.FindAll("input[name=resume-template]"), r => Assert.True(r.HasAttribute("disabled")));

        cut.Find($"input[value={UiConstants.TemplateModern}]").Change(true);
        Assert.Empty(picked);
    }
}

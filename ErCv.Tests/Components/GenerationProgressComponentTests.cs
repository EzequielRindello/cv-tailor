using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Models;

namespace ErCv.Tests.Components;

// UX-11: the generation bar and its stepper
[Trait("Category", "Component")]
public class GenerationProgressComponentTests : BunitContext
{
    private IRenderedComponent<GenerationProgress> Render(GenerationStage stage, int percent, string? detail = null) =>
        Render<GenerationProgress>(p => p.Add(c => c.Value, new GenerationStatus(stage, percent, detail)));

    [Fact]
    public void The_bar_shows_the_percent_and_has_a_name()
    {
        var bar = Render(GenerationStage.Rendering, 55).Find("progress#gen-bar");

        Assert.Equal("55", bar.GetAttribute("value"));
        Assert.Equal("100", bar.GetAttribute("max"));
        Assert.Equal("Generating your CV", bar.GetAttribute("aria-label"));
    }

    [Fact]
    public void Earlier_steps_are_done_the_current_one_runs_and_the_rest_wait()
    {
        var steps = Render(GenerationStage.Rendering, 40).FindAll(".gen-step");

        Assert.Equal(new[] { "done", "done", "current", "todo" }, steps.Select(s => s.ClassList.Intersect(new[] { "done", "current", "todo" }).Single()));
        Assert.Equal(new[] { "Parsing", "Selecting", "Rendering", "Scoring" }, steps.Select(s => s.TextContent.Trim()));
    }

    [Fact]
    public void The_page_fit_detail_shows_while_rendering()
    {
        var cut = Render(GenerationStage.Rendering, 60, "Fitting the page (2 of 4)");

        Assert.Equal("Fitting the page (2 of 4)", cut.Find("#gen-detail").TextContent);
    }

    [Fact]
    public void Without_detail_there_is_no_detail_line_and_the_screen_reader_gets_the_step_and_percent()
    {
        var cut = Render(GenerationStage.Scoring, 100);

        Assert.Empty(cut.FindAll("#gen-detail"));
        Assert.Contains("Scoring, step 4 of 4, 100 percent", cut.Find(".visually-hidden").TextContent);
    }
}

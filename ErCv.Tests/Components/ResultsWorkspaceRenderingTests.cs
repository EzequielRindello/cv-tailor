using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Models;
using ErCv.Web.UI;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Components;

// UX-11: while a download is rendered, a bar shows and the download buttons wait
[Trait("Category", "Component")]
public class ResultsWorkspaceRenderingTests : BunitContext
{
    public ResultsWorkspaceRenderingTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new ToastService());
    }

    private IRenderedComponent<ResultsWorkspace> Render(string? renderingLabel) =>
        Render<ResultsWorkspace>(p => p
            .Add(c => c.Analysis, new AnalysisResult { PreviewHtml = "<p>cv</p>" })
            .Add(c => c.Documents, new Dictionary<string, byte[]> { ["pdf"] = new byte[] { 1 }, ["docx"] = new byte[] { 1 } })
            .Add(c => c.RenderingLabel, renderingLabel));

    [Fact]
    public void No_bar_and_enabled_downloads_when_nothing_is_rendering()
    {
        var cut = Render(null);

        Assert.Empty(cut.FindAll("#render-bar"));
        Assert.All(cut.FindAll("button[aria-label^='Download']"), b => Assert.False(b.HasAttribute("disabled")));
    }

    [Fact]
    public void A_bar_with_the_label_shows_and_the_downloads_wait_while_rendering()
    {
        var cut = Render("Rendering PDF...");

        var bar = cut.Find("progress#render-bar");
        Assert.Equal("Rendering PDF...", bar.GetAttribute("aria-label"));
        Assert.False(bar.HasAttribute("value"));
        Assert.All(cut.FindAll("button[aria-label^='Download']"), b => Assert.True(b.HasAttribute("disabled")));
    }
}

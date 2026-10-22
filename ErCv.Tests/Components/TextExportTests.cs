using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Documents;
using ErCv.Web.Models;
using ErCv.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class TextExportTests : BunitContext
{
    private readonly ToastService _toasts = new();
    private readonly GeneratedCV _cv = Generated(new[] { Exp(bullets: new[] { "Built the billing API" }) });

    public TextExportTests()
    {
        Services.AddSingleton(_toasts);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<TextExport> Render(bool disabled = false) =>
        Render<TextExport>(p => p.Add(c => c.Cv, _cv).Add(c => c.Disabled, disabled));

    [Fact]
    public void Copy_sends_the_plain_text_of_the_cv_to_the_clipboard()
    {
        JSInterop.Setup<bool>(UiConstants.JsCopyText, _ => true).SetResult(true);
        var cut = Render();

        cut.FindAll("button")[0].Click();

        var call = Assert.Single(JSInterop.Invocations, i => i.Identifier == UiConstants.JsCopyText);
        Assert.Equal(CvTextRenderer.ToPlainText(_cv), call.Arguments[0]);
        Assert.Contains(_toasts.Toasts, t => t.Kind == ToastKind.Success);
    }

    [Fact]
    public void A_refused_copy_points_to_the_text_download()
    {
        JSInterop.Setup<bool>(UiConstants.JsCopyText, _ => true).SetResult(false);
        var cut = Render();

        cut.FindAll("button")[0].Click();

        var toast = Assert.Single(_toasts.Toasts);
        Assert.Equal(ToastKind.Error, toast.Kind);
        Assert.Contains(".txt", toast.Text);
    }

    [Theory]
    [InlineData(1, ".md")]
    [InlineData(2, ".txt")]
    public void Each_download_button_saves_its_format(int button, string extension)
    {
        var cut = Render();

        cut.FindAll("button")[button].Click();

        var call = Assert.Single(JSInterop.Invocations, i => i.Identifier == UiConstants.JsDownloadFile);
        Assert.EndsWith(extension, (string)call.Arguments[0]!);
    }

    [Fact]
    public void Buttons_are_disabled_while_the_cv_is_regenerating()
    {
        var cut = Render(disabled: true);

        Assert.All(cut.FindAll("button"), b => Assert.True(b.HasAttribute("disabled")));
    }
}

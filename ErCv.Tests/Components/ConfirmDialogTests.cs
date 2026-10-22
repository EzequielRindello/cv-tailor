using Bunit;
using ErCv.Web.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class ConfirmDialogTests : BunitContext
{
    private static readonly RenderFragment Body = b => b.AddMarkupContent(0, "<p>Delete <strong>everything</strong>?</p>");

    // Not awaited: the task stays pending until the dialog is answered
    private static Task<bool> Ask(IRenderedComponent<ConfirmDialog> cut)
    {
        Task<bool> answer = Task.FromResult(false);
        cut.InvokeAsync(() => { answer = cut.Instance.AskAsync("Clear it?", Body, "Clear"); }).Wait();
        return answer;
    }

    [Fact]
    public void Nothing_is_shown_until_asked()
    {
        var cut = Render<ConfirmDialog>();

        Assert.Empty(cut.FindAll(".confirm-dialog"));
    }

    [Fact]
    public async Task Asking_shows_the_dialog_focuses_cancel_and_confirm_answers_true()
    {
        var cut = Render<ConfirmDialog>();

        var answer = Ask(cut);
        Assert.Equal("alertdialog", cut.Find(".confirm-dialog").GetAttribute("role"));
        Assert.Contains("Clear it?", cut.Find("#confirm-title").TextContent);
        Assert.Equal("everything", cut.Find("#confirm-body strong").TextContent);
        Assert.Equal("Clear", cut.Find("#confirm-ok").TextContent);
        JSInterop.VerifyFocusAsyncInvoke();

        cut.Find("#confirm-ok").Click();

        Assert.True(await answer);
        Assert.Empty(cut.FindAll(".confirm-dialog"));
    }

    [Fact]
    public async Task A_click_outside_the_dialog_answers_false()
    {
        var cut = Render<ConfirmDialog>();

        var answer = Ask(cut);
        cut.Find(".confirm-overlay").Click();

        Assert.False(await answer);
    }
}

using ErCv.Web.UI;

namespace ErCv.Tests.UI;

[Trait("Category", "Unit")]
public class ToastServiceTests
{
    [Fact]
    public void The_same_message_again_replaces_the_one_on_screen()
    {
        var toasts = new ToastService();

        toasts.Success("Application updated");
        var first = toasts.Toasts.Single().Id;
        toasts.Success("Application updated");

        var shown = Assert.Single(toasts.Toasts);
        Assert.NotEqual(first, shown.Id);
    }

    [Fact]
    public void Different_messages_stack_in_order()
    {
        var toasts = new ToastService();

        toasts.Success("Removed from history");
        toasts.Error("Could not download the PDF file");
        toasts.Success("History cleared");

        Assert.Equal(
            new[] { "Removed from history", "Could not download the PDF file", "History cleared" },
            toasts.Toasts.Select(t => t.Text));
    }

    [Fact]
    public void The_same_text_of_another_kind_is_a_different_toast()
    {
        var toasts = new ToastService();

        toasts.Info("Profile cleared");
        toasts.Success("Profile cleared");

        Assert.Equal(2, toasts.Toasts.Count);
    }
}

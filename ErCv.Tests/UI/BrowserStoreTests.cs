using ErCv.Web.Models;
using ErCv.Web.UI;
using Microsoft.JSInterop;

namespace ErCv.Tests.UI;

// A reload, a closed tab or a hot reload drops the circuit while a storage call is on its way (the browser never
// answers). The page must not crash on it: reads give the default, writes and removals do nothing.
[Trait("Category", "Unit")]
public class BrowserStoreTests
{
    private sealed class GoneJsRuntime : IJSRuntime
    {
        private readonly Exception _error;

        public GoneJsRuntime(Exception error) => _error = error;

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            ValueTask.FromException<TValue>(_error);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            ValueTask.FromException<TValue>(_error);
    }

    public static TheoryData<Exception> CircuitGone() => new()
    {
        new TaskCanceledException(),
        new JSDisconnectedException("The circuit is gone")
    };

    [Theory]
    [MemberData(nameof(CircuitGone))]
    public async Task Reads_give_the_defaults(Exception error)
    {
        var store = new BrowserStore(new GoneJsRuntime(error));

        var settings = await store.LoadSettingsAsync();

        Assert.Equal(new AppSettings().Template, settings.Template);
        Assert.Empty(await store.GetHistoryAsync());
        Assert.Null(await store.LoadMyProfileAsync());
    }

    [Theory]
    [MemberData(nameof(CircuitGone))]
    public async Task Writes_and_removals_do_nothing(Exception error)
    {
        var store = new BrowserStore(new GoneJsRuntime(error));

        await store.SaveSettingsAsync(new AppSettings());
        await store.SaveMyProfileAsync(new CVMaster());
        await store.ClearMyProfileAsync();
        await store.ClearHistoryAsync();
    }

    [Fact]
    public async Task Other_script_errors_still_surface()
    {
        var store = new BrowserStore(new GoneJsRuntime(new JSException("storage is broken")));

        await Assert.ThrowsAsync<JSException>(() => store.LoadSettingsAsync());
    }
}

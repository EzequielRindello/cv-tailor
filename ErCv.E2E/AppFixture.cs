using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;

namespace ErCv.E2E;

/// <summary>
/// One app process and one browser for every E2E test.
/// Set E2E_BASE_URL to run the tests against an app that is already running instead of starting one.
/// Set E2E_BROWSER_CHANNEL to pick another installed browser (default: chrome). No browser is downloaded.
/// </summary>
public sealed class AppFixture : IAsyncLifetime
{
    private Process? _app;
    private IPlaywright? _playwright;

    public string BaseUrl { get; private set; } = string.Empty;

    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable("E2E_BASE_URL");
        if (!string.IsNullOrWhiteSpace(external))
            BaseUrl = external.TrimEnd('/');
        else
            await StartAppAsync();

        _playwright = await Playwright.CreateAsync();
        Browser = await LaunchInstalledBrowserAsync(_playwright);
    }

    /// <summary>Uses a browser that is already installed (Chrome, then Edge), so nothing has to be downloaded.</summary>
    private static async Task<IBrowser> LaunchInstalledBrowserAsync(IPlaywright playwright)
    {
        var forced = Environment.GetEnvironmentVariable("E2E_BROWSER_CHANNEL");
        var channels = string.IsNullOrWhiteSpace(forced) ? new[] { "chrome", "msedge" } : new[] { forced };

        PlaywrightException? last = null;
        foreach (var channel in channels)
        {
            try
            {
                return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = channel, Headless = true });
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("is not found"))
            {
                last = ex;
            }
        }

        throw new InvalidOperationException(
            $"None of these browsers is installed: {string.Join(", ", channels)}. Install one or set E2E_BROWSER_CHANNEL.", last);
    }

    public async Task DisposeAsync()
    {
        if (Browser != null) await Browser.CloseAsync();
        _playwright?.Dispose();

        if (_app is { HasExited: false })
        {
            _app.Kill(entireProcessTree: true);
            await _app.WaitForExitAsync();
        }

        _app?.Dispose();
    }

    private async Task StartAppAsync()
    {
        var port = FreePort();
        BaseUrl = $"http://127.0.0.1:{port}";

        var webDll = Path.Combine(AppContext.BaseDirectory, "ErCv.Web.dll");
        if (!File.Exists(webDll))
            throw new FileNotFoundException($"ErCv.Web.dll was not copied next to the tests: {webDll}");

        var info = new ProcessStartInfo("dotnet")
        {
            // The content root (wwwroot, Data, Templates) is the web project folder
            WorkingDirectory = WebProjectDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        info.ArgumentList.Add(webDll);
        info.ArgumentList.Add("--urls");
        info.ArgumentList.Add(BaseUrl);
        info.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";

        _app = Process.Start(info) ?? throw new InvalidOperationException("Could not start the web app");

        // Drain the output so a full pipe cannot stall the app
        _app.OutputDataReceived += (_, _) => { };
        _app.ErrorDataReceived += (_, _) => { };
        _app.BeginOutputReadLine();
        _app.BeginErrorReadLine();

        await WaitUntilReadyAsync();
    }

    private async Task WaitUntilReadyAsync()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(90);

        while (DateTime.UtcNow < deadline)
        {
            if (_app is { HasExited: true })
                throw new InvalidOperationException($"The web app exited early with code {_app.ExitCode}");

            try
            {
                var response = await http.GetAsync(BaseUrl + "/");
                if (response.StatusCode == HttpStatusCode.OK) return;
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"The web app did not answer on {BaseUrl} within 90 seconds");
    }

    private static string WebProjectDirectory()
    {
        // Needed when the tests are built to a folder outside the repository (dotnet test -o ...)
        var configured = Environment.GetEnvironmentVariable("E2E_WEB_DIR");
        if (!string.IsNullOrWhiteSpace(configured)) return configured;

        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "ErCv.Web");
            if (File.Exists(Path.Combine(candidate, "ErCv.Web.csproj"))) return candidate;
        }

        throw new DirectoryNotFoundException("Could not find the ErCv.Web project folder above the test output");
    }

    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<AppFixture>
{
}

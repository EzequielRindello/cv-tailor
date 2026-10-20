using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ErCv.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Api
{
    /// <summary>
    /// A fresh app per test: the rate limiter and the session store are per app instance,
    /// so tests cannot hit each other's 60 requests/minute budget or share cookies.
    /// </summary>
    public sealed class ApiApp : IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory = new();

        public HttpClient Client { get; }

        /// <summary>Another browser: same app, its own cookie jar and therefore its own session.</summary>
        public HttpClient NewClient() => _factory.CreateClient();

        public ApiApp()
        {
            Client = _factory.CreateClient();
        }

        /// <summary>Runs <paramref name="action"/> against the app's services inside a DI scope (the services are scoped).</summary>
        public async Task<T> InScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
        {
            using var scope = _factory.Services.CreateScope();
            return await action(scope.ServiceProvider);
        }

        public static object GenerateBody(
            string? jd = null, string[]? formats = null, string template = "Harvard",
            bool twoPages = false, string? profileId = null) => new Dictionary<string, object?>
        {
            ["jobDescription"] = jd ?? FixtureLoader.LoadJd("backend-go-kafka"),
            ["formats"] = formats ?? new[] { "docx" },
            ["template"] = template,
            ["allowTwoPages"] = twoPages,
            ["profileId"] = profileId ?? "cv-master"
        };

        public Task<HttpResponseMessage> PostAsync(string url, object body) =>
            Client.PostAsJsonAsync(url, body);

        public static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(text).RootElement.Clone();
        }

        public static string Squash(string text) => Documents.DocumentSupport.Squash(text);

        public static string RandomText(int chars)
        {
            var bytes = new byte[chars * 3 / 4];
            new Random(42).NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static StringContent Raw(string json) => new(json, Encoding.UTF8, "application/json");

        public void Dispose()
        {
            Client.Dispose();
            _factory.Dispose();
        }
    }
}

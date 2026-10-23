using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErCv.Tests.Support;
using ErCv.Web.Configuration;
using ErCv.Web.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Api;

// APP-07: the job post import over the API, with its own per-IP rate limit. Same collection as the PDF tests:
// the app shuts the shared headless Chromium down when a host stops.
[Trait("Category", "Api")]
[Collection("Documents")]
public class JobPostImportApiTests
{
    private const string Route = "/api/cv/import-job-post";
    private const string GreenhouseLink = "https://boards.greenhouse.io/stripe/jobs/6524123";
    private const string RefusedLink = "https://evil.example/stripe/jobs/1";

    private static readonly string GreenhouseJob = JsonSerializer.Serialize(new
    {
        title = "Senior Backend Engineer",
        company_name = "Stripe",
        content = WebUtility.HtmlEncode("<p>Build payments.</p><ul><li>Go</li><li>Kafka</li></ul>")
    });

    private static readonly string LeverJob = JsonSerializer.Serialize(new
    {
        text = "Platform Engineer",
        description = "<div>Run our platform.</div>"
    });

    private static ApiApp App(FakeHttp http, int importPermits = 10) => new(configure: services =>
    {
        services.AddSingleton<IHttpClientFactory>(http);
        services.AddSingleton(Options(importPermits));
    });

    private static ErCvOptions Options(int importPermits) => new()
    {
        RateLimit = new ErCvOptions.RateLimitSettings { PermitLimit = 60, WindowSeconds = 60 },
        ImportRateLimit = new ErCvOptions.RateLimitSettings { PermitLimit = importPermits, WindowSeconds = 60 },
        Session = new ErCvOptions.SessionSettings { IdleTimeoutMinutes = 10 }
    };

    private static Task<HttpResponseMessage> Import(HttpClient client, string? link, string? forwardedFor = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Route) { Content = JsonContent.Create(new { link }) };
        if (forwardedFor != null) request.Headers.Add("X-Forwarded-For", forwardedFor);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task A_greenhouse_link_gives_the_post_and_only_the_board_api_is_called()
    {
        var http = FakeHttp.Json(GreenhouseJob);
        using var app = App(http);

        var response = await Import(app.Client, GreenhouseLink);
        var json = await ApiApp.JsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Senior Backend Engineer", json.GetProperty("title").GetString());
        Assert.Equal("Stripe", json.GetProperty("company").GetString());
        Assert.Equal("Greenhouse", json.GetProperty("source").GetString());
        Assert.StartsWith("Senior Backend Engineer", json.GetProperty("text").GetString());
        Assert.Contains("Build payments.", json.GetProperty("text").GetString());
        Assert.Equal("boards-api.greenhouse.io", Assert.Single(http.Requested).Host);
    }

    [Fact]
    public async Task A_lever_link_gives_the_post_without_a_company()
    {
        var http = FakeHttp.Json(LeverJob);
        using var app = App(http);

        var response = await Import(app.Client, "https://jobs.lever.co/netflix/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b");
        var json = await ApiApp.JsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Lever", json.GetProperty("source").GetString());
        Assert.Equal(string.Empty, json.GetProperty("company").GetString());
        Assert.Equal("api.lever.co", Assert.Single(http.Requested).Host);
    }

    [Theory]
    [InlineData(RefusedLink)]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("https://boards.greenhouse.io.evil.example/stripe/jobs/1")]
    [InlineData("")]
    [InlineData(null)]
    public async Task A_link_that_is_not_a_greenhouse_or_lever_post_is_refused_without_a_request(string? link)
    {
        var http = FakeHttp.Json(GreenhouseJob);
        using var app = App(http);

        var response = await Import(app.Client, link);
        var json = await ApiApp.JsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("error").GetString()));
        Assert.Empty(http.Requested);
    }

    [Fact]
    public async Task A_link_that_is_too_long_is_refused_without_a_request()
    {
        var http = FakeHttp.Json(GreenhouseJob);
        using var app = App(http);

        var response = await Import(app.Client, GreenhouseLink + "?" + new string('a', 3000));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(http.Requested);
    }

    [Fact]
    public async Task A_closed_post_is_a_404()
    {
        using var app = App(FakeHttp.Json("{}", HttpStatusCode.NotFound));

        var response = await Import(app.Client, GreenhouseLink);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task A_board_that_answers_an_error_is_a_502(HttpStatusCode status)
    {
        using var app = App(FakeHttp.Json("{}", status));

        var response = await Import(app.Client, GreenhouseLink);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task A_board_that_cannot_be_reached_is_a_502_that_leaks_no_exception_text()
    {
        using var app = App(FakeHttp.Throws(new HttpRequestException("secret socket detail")));

        var response = await Import(app.Client, GreenhouseLink);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.DoesNotContain("secret socket detail", body);
    }

    [Fact]
    public async Task An_answer_that_is_not_json_is_a_502()
    {
        using var app = App(FakeHttp.Json("<html>not json</html>"));

        var response = await Import(app.Client, GreenhouseLink);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    // The action's "import" policy replaces the convention's "api" one: the third request is the limited one
    [Fact]
    public async Task The_import_limit_applies_and_leaves_the_api_budget_alone()
    {
        using var app = App(FakeHttp.Json(GreenhouseJob), importPermits: 2);

        var first = await Import(app.Client, RefusedLink);
        var second = await Import(app.Client, RefusedLink);
        var third = await Import(app.Client, RefusedLink);
        var health = await app.Client.GetAsync("/api/cv/health");

        Assert.Equal(HttpStatusCode.BadRequest, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        // APP-08: the same error shape as everywhere else, and when to come back (the import window is 60 s)
        Assert.False(string.IsNullOrWhiteSpace((await ApiApp.JsonAsync(third)).GetProperty("error").GetString()));
        Assert.InRange(int.Parse(third.Headers.GetValues("Retry-After").Single()), 1, 60);
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
    }

    [Fact]
    public async Task The_limit_is_per_client_ip()
    {
        using var app = App(FakeHttp.Json(GreenhouseJob), importPermits: 1);

        await Import(app.Client, RefusedLink, forwardedFor: "203.0.113.1");
        var sameIp = await Import(app.Client, RefusedLink, forwardedFor: "203.0.113.1");
        var otherIp = await Import(app.Client, RefusedLink, forwardedFor: "203.0.113.2");

        Assert.Equal(HttpStatusCode.TooManyRequests, sameIp.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, otherIp.StatusCode);
    }

    // ForwardLimit 1: only the last hop (the proxy's own) is trusted, so a made-up first address buys nothing
    [Fact]
    public async Task A_spoofed_forwarded_for_does_not_give_a_fresh_limit()
    {
        using var app = App(FakeHttp.Json(GreenhouseJob), importPermits: 1);

        await Import(app.Client, RefusedLink, forwardedFor: "203.0.113.1");
        var spoofed = await Import(app.Client, RefusedLink, forwardedFor: "198.51.100.9, 203.0.113.1");

        Assert.Equal(HttpStatusCode.TooManyRequests, spoofed.StatusCode);
    }

    [Fact]
    public async Task The_endpoint_is_in_the_openapi_document()
    {
        using var app = App(FakeHttp.Json(GreenhouseJob));

        var document = await ApiApp.JsonAsync(await app.Client.GetAsync("/openapi/v1.json"));

        Assert.True(document.GetProperty("paths").TryGetProperty("/api/CV/import-job-post", out _));
    }
}

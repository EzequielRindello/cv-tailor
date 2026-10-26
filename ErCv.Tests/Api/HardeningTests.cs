using System.Globalization;
using System.Net;
using ErCv.Web.Configuration;
using ErCv.Web.Constants;
using ErCv.Web.Models;
using ErCv.Web.Services;
using Microsoft.Extensions.Configuration;

namespace ErCv.Tests.Api;

// Same collection as the PDF tests: the app shuts the shared headless Chromium down when a host stops.
[Collection("Documents")]
[Trait("Category", "Api")]
public class HardeningTests
{
    [Fact]
    public async Task Generate_rejects_a_job_description_over_the_limit_with_400()
    {
        using var app = new ApiApp();
        var jd = new string('a', ApiConstants.MaxJobDescriptionLength + 1);

        var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(jd: jd));
        var json = await ApiApp.JsonAsync(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("too long", json.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Generate_accepts_a_job_description_at_the_limit()
    {
        using var app = new ApiApp();
        const string prefix = "Senior backend engineer with Go and Kafka. ";
        var jd = prefix + new string('a', ApiConstants.MaxJobDescriptionLength - prefix.Length);

        var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(jd: jd));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // The UI calls the service directly (not through the API), so the limit must hold there too
    [Fact]
    public async Task BuildCv_rejects_a_job_description_over_the_limit()
    {
        using var app = new ApiApp();
        var request = new GenerateRequest
        {
            JobDescription = new string('a', ApiConstants.MaxJobDescriptionLength + 1),
            Formats = new List<string> { ApiConstants.FormatDocx }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => app.InScopeAsync(async sp =>
        {
            var service = (CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!;
            return await service.BuildCvAsync(request);
        }));
    }

    // F28: the limit in the message is formatted the same on every server, whatever its culture
    [Theory]
    [InlineData("")]
    [InlineData("de-DE")]
    public async Task The_length_error_names_the_limit_the_same_way_in_every_culture(string culture)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        using var app = new ApiApp();
        var request = new GenerateRequest
        {
            JobDescription = new string('a', ApiConstants.MaxJobDescriptionLength + 1),
            Formats = new List<string> { ApiConstants.FormatDocx }
        };

        var error = await Assert.ThrowsAsync<ArgumentException>(() => app.InScopeAsync(sp =>
            ((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!).BuildCvAsync(request)));

        Assert.Contains("maximum 100,000 characters", error.Message);
    }

    [Theory]
    [InlineData("/api/cv/health")]
    [InlineData("/")]
    public async Task Responses_carry_the_security_headers(string url)
    {
        using var app = new ApiApp();

        var response = await app.Client.GetAsync(url);

        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("SAMEORIGIN", Header(response, "X-Frame-Options"));
        Assert.Equal("strict-origin-when-cross-origin", Header(response, "Referrer-Policy"));
        Assert.Contains("frame-ancestors 'self'", Header(response, "Content-Security-Policy"));
        Assert.Contains("object-src 'none'", Header(response, "Content-Security-Policy"));
    }

    // UseExceptionHandler("/Error") in production needs a page at that route
    [Fact]
    public async Task Error_page_exists_and_shows_no_details()
    {
        using var app = new ApiApp();

        var response = await app.Client.GetAsync("/Error");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Something went wrong", html);
        Assert.DoesNotContain("Exception", html);
    }

    [Fact]
    public void Options_load_the_values_from_appsettings()
    {
        var options = ErCvOptions.Load(Config(new()
        {
            ["ErCv:RateLimit:PermitLimit"] = "60",
            ["ErCv:RateLimit:WindowSeconds"] = "60",
            ["ErCv:ImportRateLimit:PermitLimit"] = "10",
            ["ErCv:ImportRateLimit:WindowSeconds"] = "60",
            ["ErCv:Session:IdleTimeoutMinutes"] = "10"
        }));

        Assert.Equal(60, options.RateLimit.PermitLimit);
        Assert.Equal(60, options.RateLimit.WindowSeconds);
        Assert.Equal(10, options.ImportRateLimit.PermitLimit);
        Assert.Equal(60, options.ImportRateLimit.WindowSeconds);
        Assert.Equal(10, options.Session.IdleTimeoutMinutes);
    }

    [Fact]
    public void Options_fail_at_startup_when_the_section_is_missing_or_invalid()
    {
        Assert.Throws<InvalidOperationException>(() => ErCvOptions.Load(Config(new())));
        Assert.Throws<InvalidOperationException>(() => ErCvOptions.Load(Config(new()
        {
            ["ErCv:RateLimit:PermitLimit"] = "0",
            ["ErCv:RateLimit:WindowSeconds"] = "60",
            ["ErCv:ImportRateLimit:PermitLimit"] = "10",
            ["ErCv:ImportRateLimit:WindowSeconds"] = "60",
            ["ErCv:Session:IdleTimeoutMinutes"] = "10"
        })));
    }

    // APP-07: the import limit is required, positive, and not looser than the API's
    [Theory]
    [InlineData("0", "60")]
    [InlineData("10", "0")]
    [InlineData("61", "60")]
    [InlineData("2", "1")]
    public void The_import_limit_must_be_positive_and_stricter_than_the_api_limit(string permit, string window)
    {
        var config = Config(new()
        {
            ["ErCv:RateLimit:PermitLimit"] = "60",
            ["ErCv:RateLimit:WindowSeconds"] = "60",
            ["ErCv:ImportRateLimit:PermitLimit"] = permit,
            ["ErCv:ImportRateLimit:WindowSeconds"] = window,
            ["ErCv:Session:IdleTimeoutMinutes"] = "10"
        });

        Assert.Throws<InvalidOperationException>(() => ErCvOptions.Load(config));
    }

    private static string Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) ? string.Join(",", values)
        : response.Content.Headers.TryGetValues(name, out var content) ? string.Join(",", content)
        : string.Empty;

    private static IConfiguration Config(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}

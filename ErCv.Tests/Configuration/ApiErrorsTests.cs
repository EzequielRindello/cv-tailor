using System.Net;
using System.Text.Json;
using ErCv.Web.Configuration;
using ErCv.Web.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace ErCv.Tests.Configuration;

// OPS-06: in Production a body over the limit reached the API client as the UI's HTML error page. The API answers a
// ProblemDetails instead. TestServer does not enforce the body limit, so Kestrel's rejection is thrown by hand here.
[Trait("Category", "Unit")]
public class ApiErrorsTests
{
    private const string Secret = "secret internal detail";

    private static async Task<WebApplication> AppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.UseApiErrors();
        app.MapPost("/api/cv/generate", () =>
        {
            throw new BadHttpRequestException("Request body too large.", StatusCodes.Status413PayloadTooLarge);
        });
        app.MapGet("/api/cv/boom", () =>
        {
            throw new InvalidOperationException(Secret);
        });
        app.MapGet("/page", () =>
        {
            throw new InvalidOperationException(Secret);
        });
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task A_body_over_the_limit_is_a_413_problem()
    {
        await using var app = await AppAsync();

        var response = await app.GetTestClient().PostAsync("/api/cv/generate", new StringContent("{}"));
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(ApiConstants.ProblemJsonContentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ApiConstants.TitlePayloadTooLarge, problem.GetProperty("title").GetString());
        Assert.Equal(ApiConstants.ErrorRequestTooLarge, problem.GetProperty("error").GetString());
    }

    [Fact]
    public async Task An_unexpected_exception_is_a_500_problem_that_leaks_nothing()
    {
        await using var app = await AppAsync();

        var response = await app.GetTestClient().GetAsync("/api/cv/boom");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains(ApiConstants.ErrorUnexpected, body);
        Assert.DoesNotContain(Secret, body);
    }

    [Fact]
    public async Task A_page_outside_the_api_is_left_to_the_app_wide_handler()
    {
        await using var app = await AppAsync();

        // Nothing handles it here: TestServer hands the exception back to the caller
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.GetTestClient().GetAsync("/page"));
    }

    [Theory]
    [InlineData(StatusCodes.Status413PayloadTooLarge)]
    [InlineData(StatusCodes.Status400BadRequest)]
    public void Kestrel_rejections_keep_their_status_and_anything_else_is_a_500(int status)
    {
        Assert.Equal(status, ApiErrors.StatusOf(new BadHttpRequestException("rejected", status)));
        Assert.Equal(StatusCodes.Status500InternalServerError, ApiErrors.StatusOf(new InvalidOperationException()));
    }

    [Fact]
    public async Task Another_rejection_reads_as_a_bad_request()
    {
        var context = new DefaultHttpContext { Response = { StatusCode = StatusCodes.Status400BadRequest, Body = new MemoryStream() } };

        await ApiErrors.WriteProblemAsync(context);

        context.Response.Body.Position = 0;
        var problem = JsonDocument.Parse(context.Response.Body).RootElement;
        Assert.Equal(ApiConstants.TitleBadRequest, problem.GetProperty("title").GetString());
        Assert.Equal(ApiConstants.ErrorRequestUnreadable, problem.GetProperty("detail").GetString());
    }
}

using System.Net;
using System.Text.Json;
using ErCv.Tests.Support;
using ErCv.Web.Constants;
using ErCv.Web.Features.Matching;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErCv.Tests.Features.Matching;

// APP-04: a job post read from its Greenhouse or Lever link, through the board's public API only
[Trait("Category", "Unit")]
public class JobPostImporterTests
{
    private static readonly string GreenhouseJob = JsonSerializer.Serialize(new
    {
        title = "Senior Backend Engineer",
        company_name = "Stripe",
        content = WebUtility.HtmlEncode("<p>Build payments.</p><h3>Requirements</h3><ul><li>Go</li><li>Kafka</li></ul>")
    });

    private static readonly string LeverJob = JsonSerializer.Serialize(new
    {
        text = "Platform Engineer",
        description = "<div>Run our platform.</div>",
        lists = new[] { new { text = "Nice to have", content = "<li>Terraform</li>" } },
        additional = "<div>Remote friendly.</div>"
    });

    private static JobPostImporter Importer(FakeHttp http) => new(http, NullLogger<JobPostImporter>.Instance);

    [Theory]
    [InlineData("https://boards.greenhouse.io/stripe/jobs/6524123", "https://boards-api.greenhouse.io/v1/boards/stripe/jobs/6524123")]
    [InlineData("https://job-boards.greenhouse.io/stripe/jobs/6524123/", "https://boards-api.greenhouse.io/v1/boards/stripe/jobs/6524123")]
    [InlineData("boards.greenhouse.io/stripe/jobs/6524123", "https://boards-api.greenhouse.io/v1/boards/stripe/jobs/6524123")]
    [InlineData("http://boards.greenhouse.io/stripe/jobs/6524123?gh_src=x", "https://boards-api.greenhouse.io/v1/boards/stripe/jobs/6524123")]
    [InlineData("https://boards.greenhouse.io/embed/job_app?for=stripe&token=6524123", "https://boards-api.greenhouse.io/v1/boards/stripe/jobs/6524123")]
    [InlineData("https://jobs.lever.co/netflix/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b", "https://api.lever.co/v0/postings/netflix/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b")]
    [InlineData("https://jobs.lever.co/netflix/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b/apply", "https://api.lever.co/v0/postings/netflix/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b")]
    [InlineData("https://jobs.eu.lever.co/spotify/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b", "https://api.eu.lever.co/v0/postings/spotify/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b")]
    public void Job_links_are_read_as_their_board_api(string link, string api)
    {
        Assert.Equal(new Uri(api), JobPostImporter.ParseLink(link)?.Api);
    }

    // The link is never requested, so none of these can make the server call another host
    [Theory]
    [InlineData("https://evil.example/stripe/jobs/6524123")]
    [InlineData("https://boards.greenhouse.io.evil.example/stripe/jobs/6524123")]
    [InlineData("https://boards.greenhouse.io@evil.example/stripe/jobs/6524123")]
    [InlineData("https://user@boards.greenhouse.io/stripe/jobs/6524123")]
    [InlineData("https://boards.greenhouse.io:8443/stripe/jobs/6524123")]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://localhost/stripe/jobs/1")]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://boards.greenhouse.io/stripe%2F..%2Fadmin/jobs/1")]
    [InlineData("https://boards.greenhouse.io/stripe/jobs/abc")]
    [InlineData("https://boards.greenhouse.io/embed/job_app?for=../x&token=1")]
    [InlineData("https://jobs.lever.co/netflix/not-a-posting-id")]
    [InlineData("https://www.linkedin.com/jobs/view/123")]
    [InlineData("")]
    public void Other_links_are_refused(string link)
    {
        Assert.Null(JobPostImporter.ParseLink(link));
    }

    // "/../" is resolved before the path is read: whatever board it names, the request stays on the API host
    [Fact]
    public void Dot_segments_cannot_leave_the_board_api()
    {
        var api = JobPostImporter.ParseLink("https://boards.greenhouse.io/../../admin/jobs/1")!.Api;

        Assert.Equal("boards-api.greenhouse.io", api.Host);
        Assert.Equal("/v1/boards/admin/jobs/1", api.AbsolutePath);
    }

    [Fact]
    public void A_link_longer_than_the_limit_is_refused()
    {
        Assert.Null(JobPostImporter.ParseLink("https://boards.greenhouse.io/stripe/jobs/1?" + new string('a', JobPostImporter.MaxLinkLength)));
    }

    [Fact]
    public async Task A_greenhouse_post_becomes_title_then_plain_text()
    {
        var http = FakeHttp.Json(GreenhouseJob);

        var result = await Importer(http).ImportAsync("https://boards.greenhouse.io/stripe/jobs/6524123");

        Assert.Null(result.Error);
        Assert.Equal("https://boards-api.greenhouse.io/v1/boards/stripe/jobs/6524123", Assert.Single(http.Requested).ToString());
        Assert.Equal(("Senior Backend Engineer", "Stripe", "Greenhouse"), (result.Title, result.Company, result.Source));
        Assert.Equal("Senior Backend Engineer\n\nBuild payments.\n\nRequirements\n\n- Go\n- Kafka", result.Text);
    }

    [Fact]
    public async Task A_lever_post_keeps_its_lists_and_closing_text()
    {
        var result = await Importer(FakeHttp.Json(LeverJob)).ImportAsync("https://jobs.lever.co/acme/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b");

        Assert.Null(result.Error);
        Assert.StartsWith("Platform Engineer\n\nRun our platform.", result.Text);
        Assert.Contains("Nice to have\n- Terraform", result.Text);
        Assert.EndsWith("Remote friendly.", result.Text);
    }

    [Fact]
    public async Task The_parsed_post_finds_its_skills_and_sections()
    {
        var result = await Importer(FakeHttp.Json(GreenhouseJob)).ImportAsync("https://boards.greenhouse.io/stripe/jobs/6524123");

        var requirements = new JobDescriptionParser().Parse(result.Text!);

        Assert.Contains("go", requirements.TechnicalSkills);
        Assert.Equal("senior", requirements.SeniorityLevel);
    }

    [Fact]
    public async Task A_post_longer_than_the_editor_allows_is_cut()
    {
        var huge = JsonSerializer.Serialize(new { title = "Engineer", content = new string('a', ApiConstants.MaxJobDescriptionLength + 100) });

        var result = await Importer(FakeHttp.Json(huge)).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");

        Assert.True(result.Text!.Length <= ApiConstants.MaxJobDescriptionLength);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "That job post was not found on the board. It may have been closed.")]
    [InlineData(HttpStatusCode.InternalServerError, "The job board did not answer (HTTP 500). Try again, or paste the text.")]
    [InlineData(HttpStatusCode.Found, "The job board did not answer (HTTP 302). Try again, or paste the text.")]
    public async Task A_board_error_gets_a_message(HttpStatusCode status, string message)
    {
        var result = await Importer(FakeHttp.Json("{}", status)).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");

        Assert.Null(result.Text);
        Assert.Equal(message, result.Error);
    }

    [Fact]
    public async Task An_unreachable_board_or_a_timeout_gets_a_message()
    {
        var down = await Importer(FakeHttp.Throws(new HttpRequestException("refused"))).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");
        var slow = await Importer(FakeHttp.Throws(new TaskCanceledException("timeout"))).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");

        Assert.Equal("The job board could not be reached. Try again, or paste the text.", down.Error);
        Assert.Equal(down.Error, slow.Error);
    }

    [Fact]
    public async Task An_answer_that_is_not_json_or_too_large_is_refused()
    {
        var notJson = await Importer(FakeHttp.Json("<html>")).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");
        var huge = await Importer(FakeHttp.Json(new string(' ', JobPostImporter.MaxResponseBytes + 1))).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");

        Assert.Equal("The job board's answer could not be read. Paste the text instead.", notJson.Error);
        Assert.Equal("The job board's answer is too large to be a job post.", huge.Error);
    }

    [Fact]
    public async Task A_post_without_text_is_refused()
    {
        var result = await Importer(FakeHttp.Json("{\"title\":\"Engineer\",\"content\":\"\"}")).ImportAsync("https://boards.greenhouse.io/stripe/jobs/1");

        Assert.Equal("That job post has no text to read.", result.Error);
    }

    [Fact]
    public async Task A_refused_link_makes_no_request()
    {
        var http = FakeHttp.Json(GreenhouseJob);

        var result = await Importer(http).ImportAsync("https://evil.example/stripe/jobs/1");

        Assert.StartsWith("Only Greenhouse and Lever job links can be read", result.Error);
        Assert.Empty(http.Requested);
    }
}

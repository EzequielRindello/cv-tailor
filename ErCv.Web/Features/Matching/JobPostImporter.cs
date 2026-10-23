using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ErCv.Web.Constants;
using Microsoft.AspNetCore.WebUtilities;

namespace ErCv.Web.Features.Matching;

/// <summary>
/// Reads a job post from its Greenhouse or Lever link (APP-04). The link itself is never requested: the board and
/// the post id are read out of it, and only that board's public API, on a fixed host, is called. So a pasted link
/// cannot send the server anywhere else (no SSRF). One request, a timeout, a size cap, no redirects.
/// </summary>
public sealed partial class JobPostImporter
{
    #region Constants

    /// <summary>The named HttpClient (Program.cs): no redirects, <see cref="RequestTimeout"/>, the app's User-Agent.</summary>
    public const string HttpClientName = "job-posts";
    public const string UserAgent = "ErCvGenerator/1.0 (+https://github.com/EzequielRindello/er-cv-generator)";
    public const string JsonMediaType = "application/json";
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    // A post is a few KB of JSON; anything bigger is not a job post
    public const int MaxResponseBytes = 2 * 1024 * 1024;
    public const int MaxLinkLength = 2048;
    private const int ReadChunkBytes = 81920;

    // The UI is not rate limited: at most this many board requests at a time for the whole app
    private const int MaxConcurrentRequests = 2;

    private const string SourceGreenhouse = "Greenhouse";
    private const string SourceLever = "Lever";
    private const string GreenhouseApi = "https://boards-api.greenhouse.io/v1/boards/{0}/jobs/{1}";
    private const string LeverApi = "https://api.lever.co/v0/postings/{0}/{1}";
    private const string LeverEuApi = "https://api.eu.lever.co/v0/postings/{0}/{1}";
    private const string LeverEuHost = "jobs.eu.lever.co";
    private const string GreenhouseEmbedPath = "/embed/job_app";

    private static readonly HashSet<string> GreenhouseHosts =
        new(StringComparer.OrdinalIgnoreCase) { "boards.greenhouse.io", "job-boards.greenhouse.io" };

    private static readonly HashSet<string> LeverHosts =
        new(StringComparer.OrdinalIgnoreCase) { "jobs.lever.co", LeverEuHost };

    private static readonly SemaphoreSlim Requests = new(MaxConcurrentRequests);

    // boards.greenhouse.io/stripe/jobs/6524123
    [GeneratedRegex(@"^/(?<board>[A-Za-z0-9_-]{1,100})/jobs/(?<id>\d{1,20})/?$")]
    private static partial Regex GreenhousePath();

    // jobs.lever.co/netflix/1f2e3d4c-5b6a-7980-1a2b-3c4d5e6f7a8b (or .../apply)
    [GeneratedRegex(@"^/(?<board>[A-Za-z0-9_.-]{1,100})/(?<id>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})(?:/apply)?/?$")]
    private static partial Regex LeverPath();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{1,100}$")]
    private static partial Regex BoardName();

    [GeneratedRegex(@"^\d{1,20}$")]
    private static partial Regex GreenhouseId();

    #endregion

    #region Fields

    private readonly IHttpClientFactory _clients;
    private readonly ILogger<JobPostImporter> _logger;

    #endregion

    #region Constructors

    public JobPostImporter(IHttpClientFactory clients, ILogger<JobPostImporter> logger)
    {
        _clients = clients;
        _logger = logger;
    }

    #endregion

    #region Public Methods

    /// <summary>The board API address for a Greenhouse or Lever job link, or null for any other link.</summary>
    public static JobBoardRequest? ParseLink(string? link)
    {
        if (string.IsNullOrWhiteSpace(link) || link.Length > MaxLinkLength) return null;
        var text = link.Trim();
        if (!text.Contains("://")) text = Uri.UriSchemeHttps + Uri.SchemeDelimiter + text;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            || !uri.IsDefaultPort || uri.UserInfo.Length > 0) return null;

        if (GreenhouseHosts.Contains(uri.Host))
        {
            if (GreenhousePath().Match(uri.AbsolutePath) is { Success: true } page)
                return new(SourceGreenhouse, Api(GreenhouseApi, page.Groups["board"].Value, page.Groups["id"].Value));

            // The embedded form: boards.greenhouse.io/embed/job_app?for=stripe&token=6524123
            var query = QueryHelpers.ParseQuery(uri.Query);
            if (uri.AbsolutePath.TrimEnd('/').Equals(GreenhouseEmbedPath, StringComparison.OrdinalIgnoreCase)
                && query.TryGetValue("for", out var board) && BoardName().IsMatch(board.ToString())
                && query.TryGetValue("token", out var id) && GreenhouseId().IsMatch(id.ToString()))
                return new(SourceGreenhouse, Api(GreenhouseApi, board.ToString(), id.ToString()));
            return null;
        }

        if (LeverHosts.Contains(uri.Host) && LeverPath().Match(uri.AbsolutePath) is { Success: true } posting)
        {
            var api = uri.Host.Equals(LeverEuHost, StringComparison.OrdinalIgnoreCase) ? LeverEuApi : LeverApi;
            return new(SourceLever, Api(api, posting.Groups["board"].Value, posting.Groups["id"].Value));
        }
        return null;
    }

    /// <summary>The post's text ("Title", a blank line, the body), or the message for the visitor.</summary>
    public async Task<JobPostImportResult> ImportAsync(string? link, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(link)) return Failed("Paste the link to a job post first.", JobPostImportFailure.InvalidLink);
        if (ParseLink(link) is not { } post)
            return Failed("Only Greenhouse and Lever job links can be read (boards.greenhouse.io/company/jobs/123 or jobs.lever.co/company/id). For other boards, paste the text.", JobPostImportFailure.InvalidLink);

        await Requests.WaitAsync(ct);
        try
        {
            using var client = _clients.CreateClient(HttpClientName);
            using var response = await client.GetAsync(post.Api, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Failed("That job post was not found on the board. It may have been closed.", JobPostImportFailure.NotFound);
            if (response.StatusCode != HttpStatusCode.OK)
                return Failed($"The job board did not answer (HTTP {(int)response.StatusCode}). Try again, or paste the text.", JobPostImportFailure.BoardUnavailable);

            var json = await ReadLimitedAsync(response, ct);
            if (json == null) return Failed("The job board's answer is too large to be a job post.", JobPostImportFailure.Unreadable);

            var (title, company, body) = post.Source == SourceGreenhouse ? ReadGreenhouse(json) : ReadLever(json);
            if (body.Length == 0) return Failed("That job post has no text to read.", JobPostImportFailure.Unreadable);

            var text = HtmlText.Truncate($"{HtmlText.Normalize(title).Replace('\n', ' ')}\n\n{body}", ApiConstants.MaxJobDescriptionLength);
            return new JobPostImportResult(text, title, company, post.Source, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Job post request to {Host} failed", post.Api.Host);
            return Failed("The job board could not be reached. Try again, or paste the text.", JobPostImportFailure.BoardUnavailable);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Job post from {Host} is not the expected JSON", post.Api.Host);
            return Failed("The job board's answer could not be read. Paste the text instead.", JobPostImportFailure.Unreadable);
        }
        finally
        {
            Requests.Release();
        }
    }

    #endregion

    #region Private Methods

    private static Uri Api(string format, string board, string id) =>
        new(string.Format(format, Uri.EscapeDataString(board), Uri.EscapeDataString(id)));

    // The body, or null once it passes MaxResponseBytes (a Content-Length can be missing or wrong)
    private static async Task<string?> ReadLimitedAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.Content.Headers.ContentLength > MaxResponseBytes) return null;
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var buffer = new MemoryStream();
        var chunk = new byte[ReadChunkBytes];
        int read;
        while ((read = await stream.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > MaxResponseBytes) return null;
            buffer.Write(chunk, 0, read);
        }
        return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
    }

    // https://developers.greenhouse.io/job-board.html: the post's content is HTML that is itself HTML-escaped
    private static (string Title, string Company, string Body) ReadGreenhouse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var job = doc.RootElement;
        return (Str(job, "title"), Str(job, "company_name"), HtmlText.ToPlain(WebUtility.HtmlDecode(Str(job, "content"))));
    }

    // https://github.com/lever/postings-api: a description, then titled lists, then a closing paragraph
    private static (string Title, string Company, string Body) ReadLever(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var job = doc.RootElement;
        var parts = new List<string> { HtmlText.ToPlain(Str(job, "description")) };
        if (job.TryGetProperty("lists", out var lists) && lists.ValueKind == JsonValueKind.Array)
            foreach (var section in lists.EnumerateArray())
                parts.Add(HtmlText.ToPlain(Str(section, "text")) + "\n" + HtmlText.ToPlain("<ul>" + Str(section, "content") + "</ul>"));
        parts.Add(HtmlText.ToPlain(Str(job, "additional")));
        return (Str(job, "text"), string.Empty, HtmlText.Normalize(string.Join("\n\n", parts.Where(p => p.Length > 0))));
    }

    private static string Str(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    private static JobPostImportResult Failed(string error, JobPostImportFailure failure) => new(null, null, null, null, error, failure);

    #endregion
}

/// <summary>The board API address a job link was read as.</summary>
public sealed record JobBoardRequest(string Source, Uri Api);

/// <summary>The job description read from a link, or the message for the visitor.</summary>
public sealed record JobPostImportResult(
    string? Text, string? Title, string? Company, string? Source, string? Error,
    JobPostImportFailure Failure = JobPostImportFailure.None);

/// <summary>Why a link could not be read, so the API can answer with the right status (APP-07).</summary>
public enum JobPostImportFailure
{
    None,

    /// <summary>Blank, too long or not a Greenhouse or Lever job link: nothing was requested.</summary>
    InvalidLink,

    /// <summary>The board has no such post (closed).</summary>
    NotFound,

    /// <summary>The board did not answer, answered an error, or timed out.</summary>
    BoardUnavailable,

    /// <summary>The board answered something that is not a usable job post (too large, not JSON, no text).</summary>
    Unreadable
}

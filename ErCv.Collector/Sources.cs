using System.Globalization;
using System.Net;
using System.Text.Json;

namespace ErCv.Collector
{
    /// <summary>
    /// A public job-board API. Each one returns every published post of one company board in a single request,
    /// so a refresh is one request per board. See corpus/README.md for the terms of each.
    /// </summary>
    public interface IJobSource
    {
        string Name { get; }
        string DisplayName { get; }
        Uri BoardUrl(string board);
        IReadOnlyList<RawPosting> Parse(string json);
    }

    public static class JobSources
    {
        public static readonly IReadOnlyList<IJobSource> All = new IJobSource[]
        {
            new GreenhouseSource(), new LeverSource(), new AshbySource(), new WorkableSource()
        };

        public static IJobSource? Find(string name) =>
            All.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

        internal static string Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

        internal static DateTimeOffset? Date(string value) =>
            DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d) ? d : null;

        internal static string Location(params string?[] parts) =>
            string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()).Distinct());
    }

    /// <summary>https://docs.greenhouse.io/job-board.html</summary>
    public sealed class GreenhouseSource : IJobSource
    {
        public string Name => "greenhouse";
        public string DisplayName => "Greenhouse";

        public Uri BoardUrl(string board) =>
            new($"https://boards-api.greenhouse.io/v1/boards/{Uri.EscapeDataString(board)}/jobs?content=true");

        public IReadOnlyList<RawPosting> Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var list = new List<RawPosting>();
            if (!doc.RootElement.TryGetProperty("jobs", out var jobs)) return list;

            foreach (var job in jobs.EnumerateArray())
            {
                var location = job.TryGetProperty("location", out var loc) ? JobSources.Str(loc, "name") : "";
                // The content is HTML that is itself HTML-escaped
                var html = WebUtility.HtmlDecode(JobSources.Str(job, "content"));
                list.Add(new RawPosting(
                    job.GetProperty("id").ToString(),
                    JobSources.Str(job, "absolute_url"),
                    JobSources.Str(job, "company_name"),
                    JobSources.Str(job, "title"),
                    JobSources.Date(JobSources.Str(job, "first_published")),
                    location,
                    HtmlText.ToPlain(html)));
            }
            return list;
        }
    }

    /// <summary>https://github.com/lever/postings-api</summary>
    public sealed class LeverSource : IJobSource
    {
        public string Name => "lever";
        public string DisplayName => "Lever";

        public Uri BoardUrl(string board) => new($"https://api.lever.co/v0/postings/{Uri.EscapeDataString(board)}?mode=json");

        public IReadOnlyList<RawPosting> Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var list = new List<RawPosting>();
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return list;

            foreach (var job in doc.RootElement.EnumerateArray())
            {
                var parts = new List<string> { HtmlText.ToPlain(JobSources.Str(job, "description")) };
                if (job.TryGetProperty("lists", out var lists) && lists.ValueKind == JsonValueKind.Array)
                    foreach (var section in lists.EnumerateArray())
                        parts.Add(HtmlText.ToPlain(JobSources.Str(section, "text")) + "\n"
                            + HtmlText.ToPlain("<ul>" + JobSources.Str(section, "content") + "</ul>"));
                parts.Add(HtmlText.ToPlain(JobSources.Str(job, "additional")));

                DateTimeOffset? posted = job.TryGetProperty("createdAt", out var created) && created.TryGetInt64(out var ms)
                    ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : null;
                var categories = job.TryGetProperty("categories", out var c) ? c : default;
                var location = categories.ValueKind == JsonValueKind.Object ? JobSources.Str(categories, "location") : "";
                var workplace = JobSources.Str(job, "workplaceType");

                list.Add(new RawPosting(
                    JobSources.Str(job, "id"),
                    JobSources.Str(job, "hostedUrl"),
                    null,
                    JobSources.Str(job, "text"),
                    posted,
                    JobSources.Location(location, workplace is "remote" or "hybrid" ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(workplace) : null),
                    HtmlText.Normalize(string.Join("\n\n", parts.Where(p => p.Length > 0)))));
            }
            return list;
        }
    }

    /// <summary>https://developers.ashbyhq.com/docs/public-job-posting-api</summary>
    public sealed class AshbySource : IJobSource
    {
        public string Name => "ashby";
        public string DisplayName => "Ashby";

        public Uri BoardUrl(string board) => new($"https://api.ashbyhq.com/posting-api/job-board/{Uri.EscapeDataString(board)}");

        public IReadOnlyList<RawPosting> Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var list = new List<RawPosting>();
            if (!doc.RootElement.TryGetProperty("jobs", out var jobs)) return list;

            foreach (var job in jobs.EnumerateArray())
            {
                // Unlisted posts are meant to be reached by direct link only
                var listed = !job.TryGetProperty("isListed", out var l) || l.ValueKind != JsonValueKind.False;
                var html = JobSources.Str(job, "descriptionHtml");
                var remote = job.TryGetProperty("isRemote", out var r) && r.ValueKind == JsonValueKind.True;
                list.Add(new RawPosting(
                    JobSources.Str(job, "id"),
                    JobSources.Str(job, "jobUrl"),
                    null,
                    JobSources.Str(job, "title"),
                    JobSources.Date(JobSources.Str(job, "publishedAt")),
                    JobSources.Location(JobSources.Str(job, "location"), remote ? "Remote" : null),
                    html.Length > 0 ? HtmlText.ToPlain(html) : HtmlText.Normalize(JobSources.Str(job, "descriptionPlain")),
                    listed));
            }
            return list;
        }
    }

    /// <summary>https://help.workable.com/hc/en-us/articles/115012771647 (public endpoint, published jobs only)</summary>
    public sealed class WorkableSource : IJobSource
    {
        public string Name => "workable";
        public string DisplayName => "Workable";

        // www.workable.com/api/accounts/{x} answers with a redirect to this host
        public Uri BoardUrl(string board) =>
            new($"https://apply.workable.com/api/v1/widget/accounts/{Uri.EscapeDataString(board)}?details=true");

        public IReadOnlyList<RawPosting> Parse(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var list = new List<RawPosting>();
            if (!doc.RootElement.TryGetProperty("jobs", out var jobs)) return list;
            var company = JobSources.Str(doc.RootElement, "name");

            foreach (var job in jobs.EnumerateArray())
            {
                var html = string.Join("\n", new[] { "description", "requirements", "benefits" }
                    .Select(f => JobSources.Str(job, f)).Where(s => s.Length > 0));
                var remote = job.TryGetProperty("telecommuting", out var t) && t.ValueKind == JsonValueKind.True;
                var published = JobSources.Str(job, "published_on");
                list.Add(new RawPosting(
                    JobSources.Str(job, "shortcode"),
                    JobSources.Str(job, "url"),
                    company,
                    JobSources.Str(job, "title"),
                    JobSources.Date(published.Length > 0 ? published : JobSources.Str(job, "created_at")),
                    JobSources.Location(JobSources.Str(job, "city"), JobSources.Str(job, "country"), remote ? "Remote" : null),
                    HtmlText.ToPlain(html)));
            }
            return list;
        }
    }
}

using System.Net;
using System.Text.Json;
using ErCv.Collector;

namespace ErCv.Tests.Collector;

/// <summary>APP-05 collector against fake job-board APIs: no test touches the network.</summary>
[Trait("Category", "Unit")]
public sealed class CollectorTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "ercv-corpus-" + Guid.NewGuid().ToString("N"));
    private readonly FakeBoards _boards = new();
    private readonly CorpusStore _store;

    public CollectorTests()
    {
        _store = new CorpusStore(_dir);
        CorpusStore.WriteIfChanged(_store.ConfigPath, new CorpusConfig
        {
            WindowDays = 90,
            MaxPerBoard = 5,
            MinTextLength = 20,
            DelayMs = 1000,
            Boards = new()
            {
                new() { Source = "greenhouse", Board = "acme", Company = "Acme" },
                new() { Source = "lever", Board = "globex", Company = "Globex" },
                new() { Source = "ashby", Board = "initech", Company = "Initech" },
                new() { Source = "workable", Board = "umbrella", Company = "" }
            }
        });
        _boards.Greenhouse(("101", "Senior Backend Engineer", Now.AddDays(-3)), ("102", "Account Executive", Now.AddDays(-2)), ("103", "Frontend Engineer", Now.AddDays(-200)));
        _boards.Lever(("aaa-1", "Data Engineer", Now.AddDays(-10)));
        _boards.Ashby(("bbb-1", "Site Reliability Engineer", Now.AddDays(-5), true), ("bbb-2", "Hidden Engineer", Now.AddDays(-5), false));
        _boards.Workable(("W1", "Junior Software Developer", "2026-10-01"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    private Task<RefreshResult> RefreshAsync(DateTimeOffset? now = null) =>
        new Refresher(_store, new PoliteHttp(_boards, TimeSpan.FromMilliseconds(1000), _ => Task.CompletedTask)).RunAsync(now ?? Now);

    private Dictionary<string, string> Snapshot() =>
        Directory.EnumerateFiles(_dir, "*.json", SearchOption.AllDirectories).ToDictionary(f => f, File.ReadAllText);

    [Fact]
    public async Task Refresh_keeps_only_listed_software_posts_inside_the_window()
    {
        var result = await RefreshAsync();

        var ids = _store.LoadPostings().Select(p => p.Id).ToList();
        Assert.Equal(new[] { "ashby-initech-bbb-1", "greenhouse-acme-101", "lever-globex-aaa-1", "workable-umbrella-w1" }, ids);
        Assert.Equal(4, result.Added.Count);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Posting_files_carry_the_metadata_and_link_back()
    {
        await RefreshAsync();

        var p = _store.LoadPostings().Single(x => x.Source == "greenhouse");
        Assert.Equal("https://job-boards.greenhouse.io/acme/jobs/101", p.Url);
        Assert.Equal("Acme", p.Company);
        Assert.Equal("Senior Backend Engineer", p.Title);
        Assert.Equal(Now.AddDays(-3), p.PostedAt);
        Assert.Equal(Now, p.RetrievedAt);
        Assert.Equal("backend", p.RoleFamily);
        Assert.Equal("senior", p.Seniority);
        Assert.Equal("Remote, Spain", p.Location);
        Assert.Contains("- Go and Kafka", p.Text);
        Assert.DoesNotContain("<", p.Text);

        var w = _store.LoadPostings().Single(x => x.Source == "workable");
        Assert.Equal("Umbrella Corp", w.Company);
        Assert.Equal("junior", w.Seniority);
        Assert.Equal(new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero), w.PostedAt);
    }

    [Fact]
    public async Task Refresh_twice_changes_nothing_the_second_time()
    {
        await RefreshAsync();
        var first = Snapshot();

        var again = await RefreshAsync(Now.AddHours(6));

        Assert.False(again.Changed);
        Assert.Equal(4, again.Unchanged);
        Assert.Equal(first, Snapshot());
    }

    [Fact]
    public async Task Expired_posts_are_dropped_on_a_later_refresh()
    {
        await RefreshAsync();

        var later = await RefreshAsync(Now.AddDays(86));

        // 86 days later the window starts 4 days before the first refresh: only the 3-day-old post survives
        Assert.Equal(new[] { "ashby-initech-bbb-1", "lever-globex-aaa-1", "workable-umbrella-w1" }, later.Removed);
        Assert.All(_store.LoadPostings(), p => Assert.True(p.PostedAt >= Now.AddDays(86 - 90)));
        Assert.Empty(CorpusCheck.Problems(_store, null));
    }

    [Fact]
    public async Task A_post_closed_at_the_source_is_removed()
    {
        await RefreshAsync();
        _boards.Lever();

        var result = await RefreshAsync();

        Assert.Equal(new[] { "lever-globex-aaa-1" }, result.Removed);
        Assert.False(File.Exists(_store.PostingPath("lever-globex-aaa-1")));
    }

    [Fact]
    public async Task A_board_that_fails_keeps_its_posts_until_they_expire()
    {
        await RefreshAsync();
        _boards.Fail("api.lever.co", HttpStatusCode.ServiceUnavailable);

        var result = await RefreshAsync();

        Assert.Contains(result.Errors, e => e.StartsWith("lever/globex: HTTP 503"));
        Assert.True(File.Exists(_store.PostingPath("lever-globex-aaa-1")));
        Assert.False(result.Changed);
    }

    [Fact]
    public async Task An_edited_post_is_updated_and_gets_a_new_retrieval_date()
    {
        await RefreshAsync();
        _boards.Lever(("aaa-1", "Senior Data Engineer", Now.AddDays(-10)));

        var result = await RefreshAsync(Now.AddDays(1));

        Assert.Equal(new[] { "lever-globex-aaa-1" }, result.Updated);
        var p = _store.LoadPostings().Single(x => x.Id == "lever-globex-aaa-1");
        Assert.Equal("senior", p.Seniority);
        Assert.Equal(Now.AddDays(1), p.RetrievedAt);
    }

    [Fact]
    public async Task Remove_deletes_the_post_records_a_takedown_and_refresh_never_brings_it_back()
    {
        await RefreshAsync();
        var url = _store.LoadPostings().Single(p => p.Id == "greenhouse-acme-101").Url;

        Cli.Remove(_store, Path.Combine(_dir, "samples.json"), url, "requested by the employer", Now);
        var result = await RefreshAsync();

        Assert.DoesNotContain(_store.LoadPostings(), p => p.Id == "greenhouse-acme-101");
        Assert.DoesNotContain("greenhouse-acme-101", result.Added);
        var takedown = Assert.Single(_store.LoadTakedowns());
        Assert.Equal("greenhouse-acme-101", takedown.Id);
        Assert.Equal(url, takedown.Url);
        Assert.Equal("requested by the employer", takedown.Reason);
    }

    [Fact]
    public async Task A_board_takedown_drops_every_post_of_that_company()
    {
        await RefreshAsync();
        CorpusStore.WriteIfChanged(_store.TakedownsPath, new[] { new Takedown { Board = "ashby/initech", Reason = "opt-out", Date = "2026-10-20" } });
        _boards.Requests.Clear();

        var result = await RefreshAsync();

        Assert.Equal(new[] { "ashby-initech-bbb-1" }, result.Removed);
        Assert.DoesNotContain(_boards.Requests, r => r.RequestUri!.Host == "api.ashbyhq.com");
    }

    [Fact]
    public async Task Robots_txt_is_respected_and_every_request_says_who_it_is()
    {
        _boards.Robots("boards-api.greenhouse.io", "User-agent: *\nDisallow: /v1/\n");

        var result = await RefreshAsync();

        Assert.Contains(result.Errors, e => e.Contains("robots.txt of boards-api.greenhouse.io disallows"));
        Assert.DoesNotContain(_boards.Requests, r => r.RequestUri!.Host == "boards-api.greenhouse.io" && r.RequestUri.AbsolutePath.StartsWith("/v1/"));
        Assert.All(_boards.Requests, r => Assert.StartsWith(PoliteHttp.UserAgentProduct + "/", r.Headers.UserAgent.ToString()));
    }

    [Fact]
    public async Task Requests_to_one_host_are_spaced_by_the_delay()
    {
        var waits = new List<TimeSpan>();
        var http = new PoliteHttp(_boards, TimeSpan.FromSeconds(2), d => { waits.Add(d); return Task.CompletedTask; });

        await http.GetAsync(new Uri("https://api.lever.co/v0/postings/globex?mode=json"));
        await http.GetAsync(new Uri("https://api.lever.co/v0/postings/globex?mode=json"));

        // robots.txt, then two board requests: each after the first waits
        Assert.Equal(2, waits.Count);
        Assert.All(waits, w => Assert.InRange(w, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task Samples_are_picked_one_per_role_family_from_the_corpus()
    {
        await RefreshAsync();

        var samples = SamplePicker.Pick(_store.LoadPostings(), null, out var missing);

        Assert.Equal(new[] { "backend", "data", "devops", "junior" }, samples.Select(s => s.Id));
        Assert.Equal(new[] { "Frontend", "Full stack" }, missing);
        Assert.All(samples, s => Assert.StartsWith("https://", s.Url));
        Assert.All(samples, s => Assert.NotEqual(default, s.PostedAt));
        Assert.Empty(CorpusCheck.Problems(_store, samples));
    }

    /// <summary>Serves canned board responses and records every request.</summary>
    private sealed class FakeBoards : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode Status, string Body)> _routes = new();
        private readonly Dictionary<string, HttpStatusCode> _failing = new();

        public List<HttpRequestMessage> Requests { get; } = new();

        public void Greenhouse(params (string Id, string Title, DateTimeOffset Posted)[] jobs) =>
            _routes["https://boards-api.greenhouse.io/v1/boards/acme/jobs?content=true"] = (HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                jobs = jobs.Select(j => new
                {
                    id = long.Parse(j.Id),
                    title = j.Title,
                    absolute_url = $"https://job-boards.greenhouse.io/acme/jobs/{j.Id}",
                    first_published = j.Posted.ToString("o"),
                    updated_at = Now.ToString("o"),
                    company_name = "Acme Inc",
                    location = new { name = "Remote, Spain" },
                    content = WebUtility.HtmlEncode("<p>We build things.</p><h3>Requirements</h3><ul><li><p>Go and Kafka</p></li><li>PostgreSQL &amp; Redis</li></ul>")
                })
            }));

        public void Lever(params (string Id, string Title, DateTimeOffset Posted)[] jobs) =>
            _routes["https://api.lever.co/v0/postings/globex?mode=json"] = (HttpStatusCode.OK, JsonSerializer.Serialize(jobs.Select(j => new
            {
                id = j.Id,
                text = j.Title,
                hostedUrl = $"https://jobs.lever.co/globex/{j.Id}",
                createdAt = j.Posted.ToUnixTimeMilliseconds(),
                categories = new { location = "Berlin" },
                workplaceType = "hybrid",
                description = "<div>Pipelines on Spark and Airflow.</div>",
                lists = new[] { new { text = "What you bring", content = "<li>Python</li><li>SQL</li>" } },
                additional = "<div>Equal opportunity employer.</div>"
            })));

        public void Ashby(params (string Id, string Title, DateTimeOffset Posted, bool Listed)[] jobs) =>
            _routes["https://api.ashbyhq.com/posting-api/job-board/initech"] = (HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                jobs = jobs.Select(j => new
                {
                    id = j.Id,
                    title = j.Title,
                    jobUrl = $"https://jobs.ashbyhq.com/initech/{j.Id}",
                    publishedAt = j.Posted.ToString("o"),
                    isListed = j.Listed,
                    isRemote = true,
                    location = "New York",
                    descriptionHtml = "<p>Run Kubernetes and Terraform on AWS.</p>"
                })
            }));

        public void Workable(params (string Id, string Title, string Published)[] jobs) =>
            _routes["https://apply.workable.com/api/v1/widget/accounts/umbrella?details=true"] = (HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                name = "Umbrella Corp",
                jobs = jobs.Select(j => new
                {
                    shortcode = j.Id,
                    title = j.Title,
                    url = $"https://apply.workable.com/j/{j.Id}",
                    published_on = j.Published,
                    city = "Athens",
                    country = "Greece",
                    telecommuting = false,
                    description = "<p>JavaScript, React and Node.js with mentors.</p>"
                })
            }));

        public void Robots(string host, string body) => _routes[$"https://{host}/robots.txt"] = (HttpStatusCode.OK, body);

        public void Fail(string host, HttpStatusCode status) => _failing[host] = status;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var url = request.RequestUri!.ToString();
            if (request.RequestUri.AbsolutePath != "/robots.txt" && _failing.TryGetValue(request.RequestUri.Host, out var failing))
                return Task.FromResult(new HttpResponseMessage(failing));
            var (status, body) = _routes.TryGetValue(url, out var r) ? r : (HttpStatusCode.NotFound, "");
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}

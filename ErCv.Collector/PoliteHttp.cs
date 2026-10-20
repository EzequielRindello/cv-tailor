using System.Net;
using System.Net.Http.Headers;

namespace ErCv.Collector
{
    /// <summary>
    /// The only way the collector talks to the network: a clear User-Agent, robots.txt checked once per host,
    /// at most one request per host every <c>delay</c> (or the host's Crawl-delay, if longer), a timeout and a size cap.
    /// </summary>
    public sealed class PoliteHttp
    {
        public const string UserAgentProduct = "ErCvCorpusCollector";
        public const string UserAgent = UserAgentProduct + "/1.0 (+https://github.com/EzequielRindello/er-cv-generator)";
        public const long MaxResponseBytes = 64L * 1024 * 1024;

        private readonly HttpClient _client;
        private readonly TimeSpan _delay;
        private readonly Func<TimeSpan, Task> _wait;
        private readonly Dictionary<string, RobotsRules> _robots = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, DateTimeOffset> _lastRequest = new(StringComparer.OrdinalIgnoreCase);

        public PoliteHttp(HttpMessageHandler handler, TimeSpan delay, Func<TimeSpan, Task>? wait = null)
        {
            _client = new HttpClient(handler, disposeHandler: false) { Timeout = TimeSpan.FromSeconds(60) };
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _delay = delay;
            _wait = wait ?? (d => Task.Delay(d));
        }

        public static HttpMessageHandler DefaultHandler() => new SocketsHttpHandler
        {
            // A redirect could land on a host whose robots.txt was never read
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All
        };

        /// <summary>The body of a 200 response, or an error saying why there is none (robots.txt, status, size).</summary>
        public async Task<(string? Body, string? Error)> GetAsync(Uri url, CancellationToken ct = default)
        {
            var rules = await RobotsAsync(url, ct);
            if (!rules.Allows(url.PathAndQuery))
                return (null, $"robots.txt of {url.Host} disallows {url.AbsolutePath}");

            using var response = await SendAsync(url, rules.CrawlDelay, ct);
            if (response.StatusCode != HttpStatusCode.OK)
                return (null, $"HTTP {(int)response.StatusCode} from {url.Host}");
            if (response.Content.Headers.ContentLength > MaxResponseBytes)
                return (null, $"response from {url.Host} is larger than {MaxResponseBytes} bytes");

            var body = await response.Content.ReadAsStringAsync(ct);
            return body.Length > MaxResponseBytes ? (null, $"response from {url.Host} is too large") : (body, null);
        }

        private async Task<RobotsRules> RobotsAsync(Uri url, CancellationToken ct)
        {
            if (_robots.TryGetValue(url.Host, out var cached)) return cached;

            RobotsRules rules;
            try
            {
                using var response = await SendAsync(new Uri(url, "/robots.txt"), null, ct);
                var status = (int)response.StatusCode;
                // RFC 9309: a 4xx means there are no rules; a 5xx or no answer means stay away
                rules = status == 200 ? RobotsRules.Parse(await response.Content.ReadAsStringAsync(ct), UserAgentProduct)
                    : status is >= 400 and < 500 ? RobotsRules.AllowAll
                    : RobotsRules.DisallowAll;
            }
            catch (HttpRequestException)
            {
                rules = RobotsRules.DisallowAll;
            }
            _robots[url.Host] = rules;
            return rules;
        }

        private async Task<HttpResponseMessage> SendAsync(Uri url, TimeSpan? crawlDelay, CancellationToken ct)
        {
            var delay = crawlDelay is { } c && c > _delay ? c : _delay;
            if (_lastRequest.TryGetValue(url.Host, out var last))
            {
                var wait = last + delay - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero) await _wait(wait);
            }
            _lastRequest[url.Host] = DateTimeOffset.UtcNow;
            return await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        }
    }

    /// <summary>The robots.txt group that applies to the collector (its own name, else "*").</summary>
    public sealed class RobotsRules
    {
        public static readonly RobotsRules AllowAll = new(new List<(bool, string)>(), null);
        public static readonly RobotsRules DisallowAll = new(new List<(bool, string)> { (false, "/") }, null);

        private readonly List<(bool Allow, string Path)> _rules;

        private RobotsRules(List<(bool Allow, string Path)> rules, TimeSpan? crawlDelay)
        {
            _rules = rules;
            CrawlDelay = crawlDelay;
        }

        public TimeSpan? CrawlDelay { get; }

        public static RobotsRules Parse(string text, string product)
        {
            var groups = new List<Group>();
            Group? current = null;
            var lastWasAgent = false;

            foreach (var raw in text.Split('\n'))
            {
                var line = raw.Split('#')[0].Trim();
                var colon = line.IndexOf(':');
                if (colon <= 0) continue;
                var key = line[..colon].Trim().ToLowerInvariant();
                var value = line[(colon + 1)..].Trim();

                if (key == "user-agent")
                {
                    if (current == null || !lastWasAgent) groups.Add(current = new Group());
                    current.Agents.Add(value.ToLowerInvariant());
                    lastWasAgent = true;
                    continue;
                }
                lastWasAgent = false;
                if (current == null) continue;

                if (key is "allow" or "disallow" && value.Length > 0)
                    current.Rules.Add((key == "allow", value));
                else if (key == "crawl-delay" && double.TryParse(value, System.Globalization.NumberStyles.Float,
                             System.Globalization.CultureInfo.InvariantCulture, out var seconds))
                    current.Delay = TimeSpan.FromSeconds(seconds);
            }

            var name = product.ToLowerInvariant();
            var group = groups.FirstOrDefault(g => g.Agents.Any(a => a != "*" && name.StartsWith(a)))
                ?? groups.FirstOrDefault(g => g.Agents.Contains("*"));
            return group == null ? AllowAll : new RobotsRules(group.Rules, group.Delay);
        }

        private sealed class Group
        {
            public List<string> Agents { get; } = new();
            public List<(bool Allow, string Path)> Rules { get; } = new();
            public TimeSpan? Delay { get; set; }
        }

        /// <summary>Longest matching rule wins; Allow wins a tie. Supports the "*" and "$" wildcards.</summary>
        public bool Allows(string path)
        {
            var best = (Allow: true, Length: -1);
            foreach (var (allow, rule) in _rules)
            {
                if (!Matches(path, rule)) continue;
                if (rule.Length > best.Length || (rule.Length == best.Length && allow))
                    best = (allow, rule.Length);
            }
            return best.Allow;
        }

        private static bool Matches(string path, string rule)
        {
            var pattern = "^" + System.Text.RegularExpressions.Regex.Escape(rule).Replace(@"\*", ".*");
            if (pattern.EndsWith(@"\$")) pattern = pattern[..^2] + "$";
            return System.Text.RegularExpressions.Regex.IsMatch(path, pattern);
        }
    }
}

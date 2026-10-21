using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ErCv.Web.Features.Matching;

namespace ErCv.Collector;

/// <summary>
/// corpus/report.md: what recent posts ask for, set against the parser and its skill dictionary
/// (JobDescriptionConstants.CommonTechKeywords, the future MATCH-07 data file).
/// </summary>
public static partial class RelevanceReport
{
    #region Constants

    // Thresholds the report text quotes, so the numbers in the prose always match the tables
    private const int TopSkillCount = 30;
    private const int MinWatchListPosts = 2;
    private const int MinDiscoveredPosts = 3;
    private const int MaxDiscoveredTerms = 30;
    private const int MinWrongSkillPosts = 3;
    private const int ExcerptContextChars = 30;
    private const int TopFamiliesShown = 3;

    // A "N years" figure above this is a typo or not about experience ("50 years of history")
    private const int MaxPlausibleYears = 20;

    private const string DateFormat = "yyyy-MM-dd";
    private const string MonthFormat = "yyyy-MM";

    /// <summary>
    /// Technologies a reviewer would expect the parser to know. Used only to spot dictionary gaps;
    /// a term here that the dictionary already has is not reported. Capitalized entries are product names that are
    /// also ordinary words ("Combine", "Ray"), so they only count when written that way.
    /// </summary>
    internal static readonly string[] WatchList =
    {
        "llms", "large language models", "generative ai", "genai", "RAG", "retrieval-augmented generation", "agents", "agentic",
        "prompt engineering", "fine-tuning", "evals", "embeddings", "vector database", "Pinecone", "Weaviate", "pgvector",
        "langgraph", "llamaindex", "vllm", "Triton", "CUDA", "JAX", "Ray", "mlflow", "kubeflow", "onnx", "transformers",
        "Anthropic", "Claude", "Gemini", "MCP", "Copilot", "Cursor",
        "clickhouse", "trino", "Presto", "Iceberg", "delta lake", "duckdb", "Polars", "Dagster", "Prefect", "kinesis",
        "Glue", "Athena", "emr", "bigtable", "Druid", "Pinot", "elt", "etl", "data modeling", "data warehouse", "lakehouse",
        "debezium", "kafka streams", "protobuf", "thrift", "avro", "parquet",
        "nosql", "scylladb", "vitess", "timescaledb", "postgis", "planetscale", "Neon",
        "istio", "Envoy", "linkerd", "cilium", "ebpf", "Argo", "argo workflows", "karpenter", "crossplane", "kustomize",
        "bazel", "gradle", "maven", "buildkite", "Packer", "Consul", "otel", "slo", "slos", "sli", "on-call", "incident response",
        "chaos engineering", "finops", "load balancing", "cdn", "dns", "tcp/ip", "http/2", "networking",
        "redux", "zustand", "mobx", "rxjs", "react query", "tanstack", "webgl", "three.js", "d3", "webassembly", "wasm",
        "accessibility", "wcag", "seo", "web performance", "design systems", "swiftui", "uikit", "jetpack compose", "Combine",
        "coroutines", "kmp", "kotlin multiplatform",
        "jvm", "spring cloud", "grpc-web", "trpc", "zod", "deno", "Bun", "ocaml", "zig", "solidity", "web3", "blockchain",
        "soc 2", "iso 27001", "gdpr", "hipaa", "pci", "siem", "threat modeling", "penetration testing", "Okta", "auth0",
        "keycloak", "a/b testing", "experimentation", "feature flags", "launchdarkly", "amplitude", "mixpanel", "Segment",
        "Stripe", "Salesforce", "metabase", "superset", "dbt cloud", "snowpark", "bigquery ml",
        "distributed systems", "system design", "data structures", "algorithms", "concurrency", "multithreading",
        "object-oriented", "functional programming", "api design", "observability", "performance tuning", "scalability"
    };

    private static readonly string[] SeniorityTerms =
    {
        "intern", "internship", "new grad", "graduate", "early career", "junior", "jr", "entry level", "associate",
        "mid-level", "ii", "iii", "iv", "senior", "sr", "staff", "lead", "tech lead", "principal", "distinguished",
        "member of technical staff", "architect"
    };

    private static readonly string[] Levels = { "intern", "junior", "mid", "senior", "staff", "lead", "principal" };

    // Capitalised words and common English that look like products to the token pattern
    private static readonly HashSet<string> NotTech = new(StringComparer.OrdinalIgnoreCase)
    {
        "linkedin", "youtube", "iphone", "ipad", "macbook", "paypal", "mastercard", "americorps", "phd", "msc", "bsc", "llc", "inc",
        "glassdoor", "wellbeing", "fintech", "insurtech", "proptech", "edtech", "healthtech", "nyc"
    };

    [GeneratedRegex(@"\b(?:[A-Z][a-z0-9]+(?:[A-Z][A-Za-z0-9]*)+|[A-Za-z]+\.js)\b")]
    private static partial Regex ProductToken();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    [GeneratedRegex(@"[()\[\],/|:;.]")]
    private static partial Regex TitleSeparators();

    #endregion

    #region Public Methods

    public static string Build(IReadOnlyList<Posting> postings, CorpusConfig config, DateTimeOffset now)
    {
        var parser = new JobDescriptionParser();
        var posts = postings
            .OrderBy(p => p.PostedAt).ThenBy(p => p.Id, StringComparer.Ordinal)
            .Select(p => new Analyzed(p, parser.Parse(p.JobDescription), p.JobDescription.ToLowerInvariant()))
            .ToList();
        var sb = new StringBuilder();

        sb.AppendLine("# Job-post corpus: relevance report");
        sb.AppendLine();
        if (posts.Count == 0)
        {
            sb.AppendLine($"Generated {now.ToString(DateFormat)}. The corpus is empty.");
            return sb.ToString();
        }

        sb.AppendLine($"Generated {now.ToString(DateFormat)} from {posts.Count} posts published between {posts.Min(a => a.P.PostedAt).ToString(DateFormat)} and "
            + $"{posts.Max(a => a.P.PostedAt).ToString(DateFormat)} (freshness window {config.WindowDays} days). "
            + "Skills are what `JobDescriptionParser` extracts, so this report measures the parser as much as the market.");
        sb.AppendLine();
        sb.AppendLine("Regenerate: `dotnet run --project ErCv.Collector -- report`. Source posts: `corpus/postings/`.");
        sb.AppendLine();

        AppendCorpus(sb, posts);
        AppendSkillsOverTime(sb, posts);
        AppendMissing(sb, posts);
        AppendWrong(sb, posts);
        AppendUnused(sb, posts);
        AppendSeniority(sb, posts);
        return sb.ToString().Replace("\r\n", "\n");
    }

    internal static HashSet<string> DictionaryCanonical() =>
        JobDescriptionConstants.CommonTechKeywords.Select(SkillNormalizer.Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);

    internal static string ParserLevel(string level) => level switch
    {
        "intern" => "intern",
        "junior" or "jr." or "entry-level" or "entry level" or "associate" or "graduate" or "new grad" => "junior",
        "senior" or "sr." => "senior",
        "staff engineer" => "staff",
        "lead" or "tech lead" or "team lead" => "lead",
        "principal" or "distinguished" or "fellow" or "architect" => "principal",
        _ => "mid"
    };

    #endregion

    #region Private Methods

    private static void AppendCorpus(StringBuilder sb, List<Analyzed> posts)
    {
        sb.AppendLine("## Corpus");
        sb.AppendLine();
        Table(sb, "Source", posts.GroupBy(a => JobSources.Find(a.P.Source)?.DisplayName ?? a.P.Source));
        Table(sb, "Role family", posts.GroupBy(a => a.P.RoleFamily));
        Table(sb, "Seniority (from the title)", posts.GroupBy(a => a.P.Seniority), Levels);
        Table(sb, "Month posted", posts.GroupBy(a => a.Month), posts.Select(a => a.Month).Distinct().Order().ToArray());
        sb.AppendLine($"Companies: {posts.Select(a => a.P.Company).Distinct().Count()}.");
        sb.AppendLine();
    }

    private static void AppendSkillsOverTime(StringBuilder sb, List<Analyzed> posts)
    {
        var months = posts.Select(a => a.Month).Distinct().Order().ToList();
        var perMonth = months.ToDictionary(m => m, m => posts.Count(a => a.Month == m));
        var top = posts.SelectMany(a => a.Req.TechnicalSkills.Distinct())
            .GroupBy(s => s).OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal).Take(TopSkillCount).ToList();

        sb.AppendLine("## Skill frequency over time");
        sb.AppendLine();
        sb.AppendLine($"Share of the posts published each month that the parser finds the skill in (top {TopSkillCount} overall). "
            + "Months with few posts swing a lot; read the column counts first.");
        sb.AppendLine();
        sb.AppendLine($"| Skill | All posts | {string.Join(" | ", months.Select(m => $"{m} (n={perMonth[m]})"))} |");
        sb.AppendLine($"|---|---|{string.Concat(months.Select(_ => "---|"))}");
        foreach (var g in top)
        {
            var cells = months.Select(m => Pct(posts.Count(a => a.Month == m && a.Req.TechnicalSkills.Contains(g.Key)), perMonth[m]));
            sb.AppendLine($"| {Md(g.Key)} | {g.Count()} ({Pct(g.Count(), posts.Count)}) | {string.Join(" | ", cells)} |");
        }
        sb.AppendLine();
    }

    private static void AppendMissing(StringBuilder sb, List<Analyzed> posts)
    {
        var dictionary = DictionaryCanonical();
        bool Known(string term) => dictionary.Contains(SkillNormalizer.Normalize(term));

        var companies = posts.Select(a => a.P.Company.ToLowerInvariant().Replace(" ", "")).ToHashSet();
        var watched = WatchList.Distinct().Where(t => !Known(t) && !companies.Contains(t.ToLowerInvariant()))
            .Select(t => (Term: t, Posts: posts.Where(a => Mentions(a, t)).ToList()))
            .Where(x => x.Posts.Count >= MinWatchListPosts)
            .OrderByDescending(x => x.Posts.Count).ThenBy(x => x.Term, StringComparer.Ordinal)
            .ToList();

        sb.AppendLine("## Skills the parser misses");
        sb.AppendLine();
        sb.AppendLine("Terms that recent posts use but the skill dictionary does not have, so the parser never extracts them, "
            + "match scores ignore them and the gaps tab cannot mention them. Candidates for the MATCH-07 dictionary; "
            + "some are concepts rather than skills and a person should decide.");
        sb.AppendLine();
        sb.AppendLine("### Known technologies and practices not in the dictionary");
        sb.AppendLine();
        sb.AppendLine($"From a reviewer watch list (`RelevanceReport.WatchList`), in at least {MinWatchListPosts} posts.");
        sb.AppendLine();
        sb.AppendLine("| Term | Posts | Share | Families |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var (term, hits) in watched)
            sb.AppendLine($"| {Md(term)} | {hits.Count} | {Pct(hits.Count, posts.Count)} | {Families(hits)} |");
        sb.AppendLine();

        // Product-looking tokens (CamelCase, *.js) that are neither in the dictionary nor a company in the corpus
        var watchedTerms = WatchList.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var discovered = posts
            .SelectMany(a => ProductToken().Matches(a.P.Text).Select(m => m.Value).Distinct(StringComparer.OrdinalIgnoreCase).Select(t => (Token: t, Post: a)))
            .GroupBy(x => x.Token.ToLowerInvariant())
            .Where(g => !Known(g.Key) && !watchedTerms.Contains(g.Key) && !companies.Contains(g.Key) && !NotTech.Contains(g.Key))
            .Select(g => (Term: g.GroupBy(x => x.Token).OrderByDescending(v => v.Count()).ThenBy(v => v.Key, StringComparer.Ordinal).First().Key,
                Posts: g.Select(x => x.Post).Distinct().Count()))
            .Where(x => x.Posts >= MinDiscoveredPosts)
            .OrderByDescending(x => x.Posts).ThenBy(x => x.Term, StringComparer.Ordinal)
            .Take(MaxDiscoveredTerms)
            .ToList();

        sb.AppendLine("### Other product-looking terms (review by hand)");
        sb.AppendLine();
        sb.AppendLine($"CamelCase or `*.js` words in at least {MinDiscoveredPosts} posts that are not in the dictionary, the watch list or a company name in the corpus. "
            + "Expect some noise (other companies' products, brand names).");
        sb.AppendLine();
        sb.AppendLine(discovered.Count == 0 ? "None." : string.Join(", ", discovered.Select(d => $"{Md(d.Term)} ({d.Posts})")));
        sb.AppendLine();
    }

    private static bool Mentions(Analyzed a, string term) =>
        term.Any(char.IsUpper)
            ? SkillNormalizer.FindTerm(a.P.JobDescription, term).Any(m => string.CompareOrdinal(a.P.JobDescription, m.Start, term, 0, term.Length) == 0)
            : SkillNormalizer.ContainsTerm(a.Lower, term);

    private static void AppendWrong(StringBuilder sb, List<Analyzed> posts)
    {
        var dictionary = DictionaryCanonical();
        var forms = JobDescriptionConstants.CommonTechKeywords
            .GroupBy(SkillNormalizer.Normalize, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Append(g.Key).Distinct(StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);

        sb.AppendLine("## Skills the parser gets wrong");
        sb.AppendLine();
        sb.AppendLine("### Dictionary words that match ordinary English");
        sb.AppendLine();
        sb.AppendLine($"Skills found in at least {MinWrongSkillPosts} posts where, in most of those posts, every match is written in lowercase "
            + "(products are usually capitalized: \"Next.js\", \"Go\"). Lowercase is a hint, not proof: concepts such as microservices "
            + "are normally lowercase too, so read the example. The real false positives inflate match scores and highlight the wrong words.");
        sb.AppendLine();
        sb.AppendLine("| Skill | Posts | Only lowercase prose | Example |");
        sb.AppendLine("|---|---|---|---|");
        var rows = posts.SelectMany(a => a.Req.TechnicalSkills).Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(forms.ContainsKey)
            .Select(skill =>
            {
                var hits = posts.Where(a => a.Req.TechnicalSkills.Contains(skill)).ToList();
                var prose = hits.Where(a => ProseOnly(a.P.JobDescription, forms[skill])).ToList();
                return (Skill: skill, Hits: hits.Count, Prose: prose);
            })
            .Where(r => r.Hits >= MinWrongSkillPosts && r.Prose.Count * 2 >= r.Hits)
            .OrderByDescending(r => r.Prose.Count).ThenBy(r => r.Skill, StringComparer.Ordinal)
            .ToList();
        foreach (var (skill, hits, prose) in rows)
            sb.AppendLine($"| {Md(skill)} | {hits} | {prose.Count} | {Md(Excerpt(prose[0].P.JobDescription, forms[skill]))} |");
        if (rows.Count == 0) sb.AppendLine("| none | | | |");
        sb.AppendLine();

        // "N years of X" captures the word after the phrase, which is not always a technology
        var captured = posts.SelectMany(a => a.Req.TechnicalSkills.Where(s => !dictionary.Contains(s)))
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
        sb.AppendLine("### Words taken as skills from \"N years of ...\"");
        sb.AppendLine();
        sb.AppendLine("Not in the dictionary; the parser keeps the word that follows \"N years of\". Most are not skills.");
        sb.AppendLine();
        sb.AppendLine(captured.Count == 0 ? "None." : string.Join(", ", captured.Select(g => $"{Md(g.Key)} ({g.Count()})")));
        sb.AppendLine();
    }

    /// <summary>True when every place the skill matches is plain lowercase letters (a word in a sentence, not a product name).</summary>
    private static bool ProseOnly(string text, List<string> forms)
    {
        var matches = forms.Where(f => f.All(c => char.IsLetter(c) || c is ' ' or '-'))
            .SelectMany(f => SkillNormalizer.FindTerm(text, f)).ToList();
        var all = forms.SelectMany(f => SkillNormalizer.FindTerm(text, f)).Count();
        return matches.Count > 0 && matches.Count == all && matches.All(m => text.Substring(m.Start, m.Length).All(c => !char.IsUpper(c)));
    }

    private static string Excerpt(string text, List<string> forms)
    {
        var m = forms.SelectMany(f => SkillNormalizer.FindTerm(text, f)).OrderBy(x => x.Start).First();
        var start = Math.Max(0, m.Start - ExcerptContextChars);
        var end = Math.Min(text.Length, m.Start + m.Length + ExcerptContextChars);
        return "\"..." + Whitespace().Replace(text[start..end], " ").Trim() + "...\"";
    }

    private static void AppendUnused(StringBuilder sb, List<Analyzed> posts)
    {
        var counts = DictionaryCanonical().ToDictionary(s => s, s => posts.Count(a => a.Req.TechnicalSkills.Contains(s)));
        var unused = counts.Where(kv => kv.Value == 0).Select(kv => kv.Key).Order(StringComparer.Ordinal).ToList();
        var rare = counts.Where(kv => kv.Value == 1).Select(kv => kv.Key).Order(StringComparer.Ordinal).ToList();

        sb.AppendLine("## Dictionary skills nobody asks for");
        sb.AppendLine();
        sb.AppendLine($"{unused.Count} of the {counts.Count} distinct dictionary skills (after alias normalization) appear in none of the {posts.Count} posts, "
            + $"and {rare.Count} in only one. Not a reason to delete them on its own (the corpus is software roles at product companies), "
            + "but they cost nothing to find and the ones that look dated are candidates to drop or merge in MATCH-07.");
        sb.AppendLine();
        sb.AppendLine($"**In no post:** {string.Join(", ", unused.Select(Md))}");
        sb.AppendLine();
        sb.AppendLine($"**In one post:** {(rare.Count == 0 ? "none" : string.Join(", ", rare.Select(Md)))}");
        sb.AppendLine();
    }

    private static void AppendSeniority(StringBuilder sb, List<Analyzed> posts)
    {
        var parserKeywords = JobDescriptionConstants.SeniorityKeywords;
        sb.AppendLine("## Seniority wording");
        sb.AppendLine();
        sb.AppendLine("### Words in titles");
        sb.AppendLine();
        sb.AppendLine("| Title word | Posts | Parser keyword |");
        sb.AppendLine("|---|---|---|");
        foreach (var term in SeniorityTerms)
        {
            var n = posts.Count(a => TitleHas(a.P.Title, term));
            if (n == 0) continue;
            var keyword = parserKeywords.FirstOrDefault(k => k.TrimEnd('.') == term) ?? parserKeywords.FirstOrDefault(k => k.StartsWith(term + " "));
            var shown = keyword == null ? "**none**" : keyword.TrimEnd('.') == term ? Md(keyword) : $"only inside \"{Md(keyword)}\"";
            sb.AppendLine($"| {Md(term)} | {n} | {shown} |");
        }
        sb.AppendLine();

        sb.AppendLine("### Title level against the level the parser reads from the post");
        sb.AppendLine();
        sb.AppendLine("Rows: level from the title (the collector's classifier). Columns: `JobDescriptionParser` seniority on the full post. "
            + "The parser takes the first seniority word it meets, which in long posts is often a team mention rather than the role.");
        sb.AppendLine();
        sb.AppendLine($"| Title \\ Parser | {string.Join(" | ", Levels)} |");
        sb.AppendLine($"|---|{string.Concat(Levels.Select(_ => "---|"))}");
        foreach (var row in Levels)
        {
            var inRow = posts.Where(a => a.P.Seniority == row).ToList();
            if (inRow.Count == 0) continue;
            sb.AppendLine($"| {row} | {string.Join(" | ", Levels.Select(col => inRow.Count(a => ParserLevel(a.Req.SeniorityLevel) == col)))} |");
        }
        var agree = posts.Count(a => ParserLevel(a.Req.SeniorityLevel) == a.P.Seniority);
        sb.AppendLine();
        sb.AppendLine($"Agreement: {agree} of {posts.Count} ({Pct(agree, posts.Count)}).");
        sb.AppendLine();

        sb.AppendLine("### Years of experience asked for");
        sb.AppendLine();
        sb.AppendLine("First \"N years\" or \"N+ years\" in each post.");
        sb.AppendLine();
        var years = posts.Select(a => JobDescriptionConstants.YearsOfExperience().Match(a.Lower) is { Success: true } m && int.TryParse(m.Groups[1].Value, out var y) && y <= MaxPlausibleYears ? y : (int?)null).ToList();
        var buckets = new (string Label, Func<int?, bool> In)[]
        {
            ("not stated", y => y == null), ("0 to 1", y => y <= 1), ("2 to 3", y => y is >= 2 and <= 3),
            ("4 to 5", y => y is >= 4 and <= 5), ("6 to 7", y => y is >= 6 and <= 7), ("8 or more", y => y >= 8)
        };
        sb.AppendLine("| Years | Posts |");
        sb.AppendLine("|---|---|");
        foreach (var (label, test) in buckets) sb.AppendLine($"| {label} | {years.Count(test)} |");
        sb.AppendLine();
    }

    // The term part is built per call, so only the separators can be a [GeneratedRegex]
    private static bool TitleHas(string title, string term) =>
        Regex.IsMatch(" " + TitleSeparators().Replace(title.ToLowerInvariant(), " ") + " ", $@"(?<![a-z0-9]){Regex.Escape(term)}(?![a-z0-9])");

    private static void Table(StringBuilder sb, string title, IEnumerable<IGrouping<string, Analyzed>> groups, string[]? order = null)
    {
        var list = groups.ToList();
        var rows = order != null
            ? order.Select(k => (Key: k, Count: list.FirstOrDefault(g => g.Key == k)?.Count() ?? 0)).Where(r => r.Count > 0)
            : list.Select(g => (g.Key, Count: g.Count())).OrderByDescending(r => r.Count).ThenBy(r => r.Key, StringComparer.Ordinal);
        sb.AppendLine($"| {title} | Posts |");
        sb.AppendLine("|---|---|");
        foreach (var (key, count) in rows) sb.AppendLine($"| {Md(key)} | {count} |");
        sb.AppendLine();
    }

    private static string Families(IEnumerable<Analyzed> hits) =>
        string.Join(", ", hits.GroupBy(a => a.P.RoleFamily).OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal).Take(TopFamiliesShown).Select(g => $"{g.Key} {g.Count()}"));

    private static string Pct(int part, int whole) =>
        whole == 0 ? "-" : (100.0 * part / whole).ToString("0", CultureInfo.InvariantCulture) + "%";

    private static string Md(string s) => s.Replace("|", "\\|");

    #endregion

    #region Nested Types

    private sealed record Analyzed(Posting P, ErCv.Web.Models.JobRequirements Req, string Lower)
    {
        public string Month => P.PostedAt.ToString(MonthFormat, CultureInfo.InvariantCulture);
    }

    #endregion
}

using System.Text.Json;
using ErCv.Web.Constants;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Models;

namespace ErCv.Collector;

/// <summary>
/// dotnet run --project ErCv.Collector -- refresh | report | samples | check | remove &lt;id or url&gt; [--reason "..."]
/// Paths default to the repository this tool lives in (corpus/ and ErCv.Web/Data/sample-jobs.json).
/// </summary>
public static class Cli
{
    #region Constants

    private const string SolutionFile = "er-cv-generator.sln";
    private const string CorpusFolder = "corpus";
    private const string ReportFile = "report.md";
    private static readonly string[] DefaultSamplesPath = { "ErCv.Web", "Data", "sample-jobs.json" };

    private const string CorpusOption = "--corpus";
    private const string SamplesOption = "--samples";
    private const string ReasonOption = "--reason";
    private const string OptionPrefix = "--";
    private const string DefaultTakedownReason = "takedown request";
    private const string UrlPrefix = "http";
    private const string DateFormat = "yyyy-MM-dd";

    private const string Usage = """
        Usage: dotnet run --project ErCv.Collector -- <command>

          refresh            Fetch the boards in corpus/sources.json, drop expired, closed and taken-down posts,
                             add new ones, then re-pick the app's sample job descriptions. Safe to run again.
          report             Write corpus/report.md (skill frequency over time, parser gaps, seniority wording).
          samples            Re-pick the app's sample job descriptions from the posts already in the corpus
                             (no network). Run it after changing the parser or the skill dictionary.
          check              Validate the corpus against today's date and the freshness window.
          remove <id|url>    Delete a post and record a takedown so it is never collected again.
                             Options: --reason "<why>"

        Options: --corpus <dir>  --samples <file>
        """;

    #endregion

    #region Public Methods

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            Console.WriteLine(Usage);
            return args.Length == 0 ? 1 : 0;
        }

        var root = FindRepoRoot();
        var corpus = Option(args, CorpusOption) ?? Path.Combine(root ?? ".", CorpusFolder);
        var samplesPath = Option(args, SamplesOption) ?? Path.Combine(DefaultSamplesPath.Prepend(root ?? ".").ToArray());
        var store = new CorpusStore(Path.GetFullPath(corpus));
        var now = DateTimeOffset.UtcNow;

        switch (args[0])
        {
            case "refresh":
                return await RefreshAsync(store, samplesPath, now);
            case "report":
                var reportPath = Path.Combine(store.Root, ReportFile);
                File.WriteAllText(reportPath, RelevanceReport.Build(store.LoadPostings(), store.LoadConfig(), now));
                Console.WriteLine($"Wrote {reportPath}");
                return 0;
            case "samples":
                WriteSamples(store, samplesPath);
                return 0;
            case "check":
                return Check(store, samplesPath, now);
            case "remove" when args.Length > 1 && !args[1].StartsWith(OptionPrefix):
                return Remove(store, samplesPath, args[1], Option(args, ReasonOption) ?? DefaultTakedownReason, now);
            default:
                Console.Error.WriteLine(Usage);
                return 1;
        }
    }

    internal static int Remove(CorpusStore store, string samplesPath, string idOrUrl, string reason, DateTimeOffset now)
    {
        var takedown = new Takedown
        {
            Id = idOrUrl.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase) ? null : idOrUrl,
            Url = idOrUrl.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase) ? idOrUrl : null,
            Reason = reason,
            Date = now.ToString(DateFormat)
        };
        var postings = store.LoadPostings();
        var matches = postings.Where(takedown.Matches).ToList();
        foreach (var p in matches)
        {
            store.DeletePosting(p.Id);
            Console.WriteLine($"Removed {p.Id} ({p.Title}, {p.Company})");
        }

        // Recorded by id and url, so the post is not collected again under either
        var first = matches.FirstOrDefault();
        var takedowns = store.LoadTakedowns();
        takedowns.Add(takedown with { Id = first?.Id ?? takedown.Id, Url = first?.Url ?? takedown.Url });
        store.SaveTakedowns(takedowns);

        var left = store.LoadPostings();
        store.SaveManifest(Refresher.BuildManifest(left, matches.Count > 0 ? now : store.LoadManifest()?.RefreshedAt ?? now, store.LoadConfig().WindowDays));
        WriteSamples(store, samplesPath);
        if (matches.Count == 0) Console.WriteLine("No post matched; the takedown is recorded so it is never collected.");
        return 0;
    }

    #endregion

    #region Private Methods

    private static async Task<int> RefreshAsync(CorpusStore store, string samplesPath, DateTimeOffset now)
    {
        var config = store.LoadConfig();
        using var handler = PoliteHttp.DefaultHandler();
        var http = new PoliteHttp(handler, TimeSpan.FromMilliseconds(config.DelayMs));
        Console.WriteLine($"Refreshing {config.Boards.Count} boards into {store.Root} (window {config.WindowDays} days)...");

        var result = await new Refresher(store, http).RunAsync(now);
        foreach (var line in result.BoardSummary) Console.WriteLine("  " + line);
        foreach (var line in result.Errors) Console.WriteLine("  ! " + line);
        Console.WriteLine($"Added {result.Added.Count}, updated {result.Updated.Count}, removed {result.Removed.Count}, unchanged {result.Unchanged}.");

        WriteSamples(store, samplesPath);
        return 0;
    }

    private static int Check(CorpusStore store, string samplesPath, DateTimeOffset now)
    {
        var problems = CorpusCheck.Problems(store, ReadSamples(samplesPath), now);
        var count = store.LoadPostings().Count;
        if (problems.Count == 0)
        {
            Console.WriteLine($"OK: {count} postings, all inside the {store.LoadConfig().WindowDays}-day window as of {now:yyyy-MM-dd}, samples in sync.");
            return 0;
        }
        foreach (var p in problems) Console.WriteLine("  ! " + p);
        Console.WriteLine($"{problems.Count} problem(s). An expired post is fixed by running refresh.");
        return 1;
    }

    private static void WriteSamples(CorpusStore store, string samplesPath)
    {
        var samples = SamplePicker.Pick(store.LoadPostings(), LoadDemoProfile(samplesPath), out var missing);
        var changed = CorpusStore.WriteIfChanged(samplesPath, samples);
        Console.WriteLine($"Samples: {string.Join(", ", samples.Select(s => $"{s.Label} = {s.Company}"))}{(changed ? " (updated)" : " (unchanged)")}");
        if (missing.Count > 0) Console.WriteLine($"  ! no post for sample(s): {string.Join(", ", missing)}");
    }

    /// <summary>The profile the demo starts with, next to the samples file (ErCv.Web/Data); null when it is not there.</summary>
    public static CVMaster? LoadDemoProfile(string samplesPath)
    {
        var path = Path.Combine(Path.GetDirectoryName(samplesPath) ?? ".", ApiConstants.SeniorSampleProfileId + ".json");
        return File.Exists(path) ? ProfileJson.Parse(File.ReadAllText(path)).Profile : null;
    }

    private static List<SampleRecord>? ReadSamples(string path) =>
        File.Exists(path) ? JsonSerializer.Deserialize<List<SampleRecord>>(File.ReadAllText(path), CorpusStore.Json) : null;

    private static string? Option(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private static string? FindRepoRoot()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
                if (File.Exists(Path.Combine(dir.FullName, SolutionFile))) return dir.FullName;
        return null;
    }

    #endregion
}

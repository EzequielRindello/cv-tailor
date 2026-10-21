using System.Text.Json.Serialization;

namespace ErCv.Collector;

/// <summary>One committed job post: only the fields the app needs, plus where and when it came from.</summary>
public sealed record Posting
{
    public string Id { get; init; } = "";
    public string Source { get; init; } = "";
    public string Board { get; init; } = "";
    public string SourceId { get; init; } = "";
    public string Url { get; init; } = "";
    public string Company { get; init; } = "";
    public string Title { get; init; } = "";
    public DateTimeOffset PostedAt { get; init; }
    public DateTimeOffset RetrievedAt { get; init; }
    public string RoleFamily { get; init; } = "";
    public string Seniority { get; init; } = "";
    public string Location { get; init; } = "";
    public string Text { get; init; } = "";

    /// <summary>The text the app is given: the title, then the post body.</summary>
    [JsonIgnore]
    public string JobDescription => $"{Title}\n\n{Text}";

    /// <summary>Same post, ignoring when it was retrieved (so an unchanged post is not rewritten).</summary>
    public bool SameContentAs(Posting other) => this with { RetrievedAt = default } == other with { RetrievedAt = default };
}

/// <summary>What a source adapter returns for one listing, before filtering and classification.</summary>
public sealed record RawPosting(
    string SourceId,
    string Url,
    string? Company,
    string Title,
    DateTimeOffset? PostedAt,
    string Location,
    string Text,
    bool Listed = true);

public sealed record BoardConfig
{
    public string Source { get; init; } = "";
    public string Board { get; init; } = "";
    public string Company { get; init; } = "";
}

/// <summary>corpus/sources.json</summary>
public sealed record CorpusConfig
{
    public int WindowDays { get; init; } = 90;
    public int MaxPerBoard { get; init; } = 6;
    public int MaxTextLength { get; init; } = 15000;
    public int MinTextLength { get; init; } = 400;
    public int DelayMs { get; init; } = 1500;
    public List<BoardConfig> Boards { get; init; } = new();
}

/// <summary>corpus/takedowns.json: a matching posting is deleted and never collected again.</summary>
public sealed record Takedown
{
    public string? Id { get; init; }
    public string? Url { get; init; }
    /// <summary>"source/board" to drop a whole company board.</summary>
    public string? Board { get; init; }
    public string Reason { get; init; } = "";
    public string Date { get; init; } = "";

    public bool Matches(Posting p) =>
        (!string.IsNullOrEmpty(Id) && string.Equals(Id, p.Id, StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrEmpty(Url) && string.Equals(Url.TrimEnd('/'), p.Url.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
        || MatchesBoard(p.Source, p.Board);

    public bool MatchesBoard(string source, string board) =>
        !string.IsNullOrEmpty(Board) && string.Equals(Board, $"{source}/{board}", StringComparison.OrdinalIgnoreCase);
}

/// <summary>corpus/manifest.json. Only rewritten when the corpus changes, so a refresh with nothing new is a no-op.</summary>
public sealed record Manifest
{
    public DateTimeOffset RefreshedAt { get; init; }
    public int WindowDays { get; init; }
    public int Count { get; init; }
    public DateTimeOffset? OldestPostedAt { get; init; }
    public DateTimeOffset? NewestPostedAt { get; init; }
    public SortedDictionary<string, int> BySource { get; init; } = new();
    public SortedDictionary<string, int> ByRoleFamily { get; init; } = new();
}

/// <summary>One APP-03 sample as the web app reads it (ErCv.Web/Data/sample-jobs.json).</summary>
public sealed record SampleRecord
{
    public string Id { get; init; } = "";
    public string Label { get; init; } = "";
    public string PostingId { get; init; } = "";
    public string Source { get; init; } = "";
    public string Url { get; init; } = "";
    public string Company { get; init; } = "";
    public string Title { get; init; } = "";
    public string Location { get; init; } = "";
    public DateTimeOffset PostedAt { get; init; }
    public string Text { get; init; } = "";
}

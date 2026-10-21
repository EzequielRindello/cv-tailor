namespace ErCv.Collector;

public sealed record RefreshResult(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Updated,
    IReadOnlyList<string> Removed,
    int Unchanged,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> BoardSummary)
{
    public bool Changed => Added.Count + Updated.Count + Removed.Count > 0;
}

/// <summary>
/// One refresh: fetch every configured board, keep the newest software posts inside the freshness window,
/// drop what expired, closed or was taken down. Run twice against the same listings, the second run changes nothing.
/// </summary>
public sealed class Refresher
{
    #region Constants

    // A post dated this far in the future is still accepted (time zones); anything later is a bad date
    private const int FutureToleranceDays = 1;

    #endregion

    #region Fields

    private readonly CorpusStore _store;
    private readonly PoliteHttp _http;

    #endregion

    #region Constructors

    public Refresher(CorpusStore store, PoliteHttp http)
    {
        _store = store;
        _http = http;
    }

    #endregion

    #region Public Methods

    public async Task<RefreshResult> RunAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        now = Truncate(now);
        var config = _store.LoadConfig();
        var takedowns = _store.LoadTakedowns();
        var existing = _store.LoadPostings().ToDictionary(p => p.Id, StringComparer.Ordinal);
        var cutoff = now.AddDays(-config.WindowDays);

        var keep = new Dictionary<string, Posting>(StringComparer.Ordinal);
        var added = new List<string>();
        var updated = new List<string>();
        var errors = new List<string>();
        var summary = new List<string>();

        foreach (var board in config.Boards)
        {
            var label = $"{board.Source}/{board.Board}";
            var source = JobSources.Find(board.Source);
            if (source == null) { errors.Add($"{label}: unknown source"); continue; }
            if (takedowns.Any(t => t.MatchesBoard(board.Source, board.Board))) { summary.Add($"{label}: taken down, skipped"); continue; }

            var (body, error) = await _http.GetAsync(source.BoardUrl(board.Board), ct);
            IReadOnlyList<RawPosting>? raw = null;
            if (body != null)
            {
                try { raw = source.Parse(body); }
                catch (System.Text.Json.JsonException ex) { error = $"unreadable response ({ex.Message})"; }
            }

            if (raw == null)
            {
                // The source is down or refused: keep what is still in the window instead of wiping the board
                var carried = existing.Values
                    .Where(p => p.Source == board.Source && p.Board == board.Board && p.PostedAt >= cutoff && !takedowns.Any(t => t.Matches(p)))
                    .ToList();
                foreach (var p in carried) keep[p.Id] = p;
                errors.Add($"{label}: {error}; kept {carried.Count} existing");
                continue;
            }

            var candidates = raw
                .Select(r => ToPosting(r, board, config, now))
                .Where(p => p != null && p.PostedAt >= cutoff && !takedowns.Any(t => t.Matches(p)))
                .Select(p => p!)
                // The same role is often posted once per location with the same text: keep the newest
                .OrderByDescending(p => p.PostedAt).ThenBy(p => p.Id, StringComparer.Ordinal)
                .GroupBy(p => p.Title, StringComparer.OrdinalIgnoreCase).Select(g => g.First())
                .ToList();
            var picked = Pick(candidates, config.MaxPerBoard);
            summary.Add($"{label}: {raw.Count} listed, {candidates.Count} eligible, {picked.Count} kept");

            foreach (var p in picked)
            {
                if (existing.TryGetValue(p.Id, out var old))
                {
                    if (old.SameContentAs(p)) { keep[p.Id] = old; continue; }
                    updated.Add(p.Id);
                }
                else added.Add(p.Id);
                keep[p.Id] = p;
            }
        }

        var removed = existing.Keys.Where(id => !keep.ContainsKey(id)).OrderBy(id => id, StringComparer.Ordinal).ToList();
        foreach (var id in removed) _store.DeletePosting(id);
        foreach (var p in keep.Values) _store.SavePosting(p);

        var result = new RefreshResult(added, updated, removed, keep.Count - added.Count - updated.Count, errors, summary);
        var refreshedAt = result.Changed || _store.LoadManifest() is not { } m ? now : m.RefreshedAt;
        _store.SaveManifest(BuildManifest(keep.Values, refreshedAt, config.WindowDays));
        return result;
    }

    /// <summary>Null when the post is not a software role, has no publish date, is unlisted or too short to be useful.</summary>
    internal static Posting? ToPosting(RawPosting r, BoardConfig board, CorpusConfig config, DateTimeOffset now)
    {
        if (!r.Listed || r.PostedAt is not { } posted || posted > now.AddDays(FutureToleranceDays)) return null;
        if (string.IsNullOrWhiteSpace(r.Url) || string.IsNullOrWhiteSpace(r.SourceId)) return null;
        var title = HtmlText.Normalize(r.Title).Replace("\n", " ");
        var family = Classifier.RoleFamily(title);
        if (family == null) return null;
        var text = HtmlText.Truncate(r.Text, config.MaxTextLength);
        if (text.Length < config.MinTextLength) return null;

        return new Posting
        {
            Id = CorpusStore.MakeId(board.Source, board.Board, r.SourceId),
            Source = board.Source,
            Board = board.Board,
            SourceId = r.SourceId,
            Url = r.Url,
            Company = string.IsNullOrWhiteSpace(board.Company) ? r.Company ?? board.Board : board.Company,
            Title = title,
            PostedAt = Truncate(posted),
            RetrievedAt = now,
            RoleFamily = family,
            Seniority = Classifier.Seniority(title),
            Location = r.Location,
            Text = text
        };
    }

    /// <summary>Newest first, but spread over role families and seniorities so one team does not fill a board's quota.</summary>
    internal static List<Posting> Pick(List<Posting> candidates, int max)
    {
        var remaining = candidates.OrderByDescending(p => p.PostedAt).ThenBy(p => p.Id, StringComparer.Ordinal).ToList();
        var picked = new List<Posting>();
        while (picked.Count < max && remaining.Count > 0)
        {
            var next = remaining
                .OrderBy(c => picked.Count(p => p.RoleFamily == c.RoleFamily) + picked.Count(p => p.Seniority == c.Seniority))
                .First();
            picked.Add(next);
            remaining.Remove(next);
        }
        return picked;
    }

    public static Manifest BuildManifest(IEnumerable<Posting> postings, DateTimeOffset refreshedAt, int windowDays)
    {
        var list = postings.ToList();
        return new Manifest
        {
            RefreshedAt = refreshedAt,
            WindowDays = windowDays,
            Count = list.Count,
            OldestPostedAt = list.Count > 0 ? list.Min(p => p.PostedAt) : null,
            NewestPostedAt = list.Count > 0 ? list.Max(p => p.PostedAt) : null,
            BySource = new SortedDictionary<string, int>(list.GroupBy(p => p.Source).ToDictionary(g => g.Key, g => g.Count()), StringComparer.Ordinal),
            ByRoleFamily = new SortedDictionary<string, int>(list.GroupBy(p => p.RoleFamily).ToDictionary(g => g.Key, g => g.Count()), StringComparer.Ordinal)
        };
    }

    #endregion

    #region Private Methods

    private static DateTimeOffset Truncate(DateTimeOffset d)
    {
        var utc = d.ToUniversalTime();
        return new DateTimeOffset(utc.Ticks - utc.Ticks % TimeSpan.TicksPerSecond, TimeSpan.Zero);
    }

    #endregion
}

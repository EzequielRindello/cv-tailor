namespace ErCv.Collector;

/// <summary>The rules every committed corpus must follow. Used by the "check" command and by the unit tests.</summary>
public static class CorpusCheck
{
    #region Constants

    // A post may carry a posted date up to this far after the moment it was fetched (time zones, clock skew)
    private const int PostedAtToleranceDays = 1;
    private const string DateFormat = "yyyy-MM-dd";

    #endregion

    #region Public Methods

    /// <param name="asOf">The date the window is measured from: the last refresh (tests) or today (the command).</param>
    public static List<string> Problems(CorpusStore store, IReadOnlyList<SampleRecord>? samples, DateTimeOffset? asOf = null)
    {
        var problems = new List<string>();
        var config = store.LoadConfig();
        var manifest = store.LoadManifest();
        var postings = store.LoadPostings();
        var takedowns = store.LoadTakedowns();

        if (manifest == null) problems.Add("manifest.json is missing");
        else if (manifest.Count != postings.Count) problems.Add($"manifest.json says {manifest.Count} postings, the folder has {postings.Count}");

        var cutoff = (asOf ?? manifest?.RefreshedAt ?? DateTimeOffset.UtcNow).AddDays(-config.WindowDays);
        foreach (var p in postings)
        {
            var file = p.Id;
            if (JobSources.Find(p.Source) == null) problems.Add($"{file}: unknown source '{p.Source}'");
            if (!Uri.TryCreate(p.Url, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps) problems.Add($"{file}: url is not an https link");
            if (p.PostedAt == default) problems.Add($"{file}: no postedAt");
            if (p.RetrievedAt == default) problems.Add($"{file}: no retrievedAt");
            if (p.PostedAt > p.RetrievedAt.AddDays(PostedAtToleranceDays)) problems.Add($"{file}: postedAt is after retrievedAt");
            if (p.PostedAt < cutoff) problems.Add($"{file}: posted {p.PostedAt.ToString(DateFormat)}, older than the {config.WindowDays}-day window");
            foreach (var (name, value) in new[] { ("board", p.Board), ("sourceId", p.SourceId), ("company", p.Company), ("title", p.Title), ("roleFamily", p.RoleFamily), ("seniority", p.Seniority), ("text", p.Text) })
                if (string.IsNullOrWhiteSpace(value)) problems.Add($"{file}: no {name}");
            if (p.Text.Length > config.MaxTextLength) problems.Add($"{file}: text longer than {config.MaxTextLength}");
            if (takedowns.Any(t => t.Matches(p))) problems.Add($"{file}: matches a takedown request");
        }

        if (samples != null)
        {
            var byId = postings.ToDictionary(p => p.Id, StringComparer.Ordinal);
            foreach (var s in samples)
            {
                if (!byId.TryGetValue(s.PostingId, out var p)) { problems.Add($"sample {s.Id}: posting {s.PostingId} is not in the corpus"); continue; }
                if (s.PostedAt != p.PostedAt || s.Url != p.Url || s.Text != p.JobDescription || s.Company != p.Company)
                    problems.Add($"sample {s.Id}: out of date with {s.PostingId}, run refresh");
            }
        }
        return problems;
    }

    #endregion
}

namespace ErCv.Web.UI
{
    public enum HistorySort
    {
        Newest,
        Oldest,
        Status
    }

    /// <summary>Statuses, filtering and sorting for the application tracker (APP-01). Pure, so it can be tested without a browser.</summary>
    public static class ApplicationTracker
    {
        public const string DefaultStatus = "Saved";

        /// <summary>In pipeline order, which is also the order used when sorting by status.</summary>
        public static readonly string[] Statuses = { "Saved", "Applied", "Interview", "Offer", "Rejected" };

        /// <summary>An unknown or missing status (hand-edited storage) counts as the default.</summary>
        public static string Normalize(string? status) =>
            Statuses.FirstOrDefault(s => s.Equals(status?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? DefaultStatus;

        public static List<HistoryEntry> Apply(IEnumerable<HistoryEntry> entries, string? statusFilter, HistorySort sort)
        {
            var filter = string.IsNullOrEmpty(statusFilter) ? null : Normalize(statusFilter);
            var matching = entries.Where(e => filter == null || Normalize(e.Status) == filter);

            return (sort switch
            {
                HistorySort.Oldest => matching.OrderBy(e => e.Id),
                HistorySort.Status => matching.OrderBy(e => Array.IndexOf(Statuses, Normalize(e.Status))).ThenByDescending(e => e.Id),
                _ => matching.OrderByDescending(e => e.Id)
            }).ToList();
        }

        /// <summary>Entries per status, for the filter labels.</summary>
        public static Dictionary<string, int> CountByStatus(IEnumerable<HistoryEntry> entries)
        {
            var counts = Statuses.ToDictionary(s => s, _ => 0);
            foreach (var e in entries) counts[Normalize(e.Status)]++;
            return counts;
        }
    }
}

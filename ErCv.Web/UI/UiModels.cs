namespace ErCv.Web.UI
{
    public class AppSettings
    {
        public string Template { get; set; } = "harvard";
        public Dictionary<string, bool> SelectedFormats { get; set; } = new() { ["pdf"] = true, ["docx"] = false };
        public bool AllowTwoPages { get; set; }
        /// <summary>"demo" or "mine" (PROF-01).</summary>
        public string ProfileSource { get; set; } = "demo";
    }

    public class HistoryEntry
    {
        public long Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string GeneratedAt { get; set; } = string.Empty;
        public int? MatchScore { get; set; }
        public string Template { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;

        // APP-02: what is needed to run the same generation again. Null on entries saved before these existed.
        public string? TemplateKey { get; set; }
        public List<string>? Formats { get; set; }
        public bool? AllowTwoPages { get; set; }
        /// <summary>"demo", "mine" or "imported". An imported profile is not kept, so it cannot be restored.</summary>
        public string? ProfileSource { get; set; }
        public string? ProfileId { get; set; }
    }

    public record TemplateOption(string Key, string Name, string Description);

    public record FormatOption(string Key, string Name);
}

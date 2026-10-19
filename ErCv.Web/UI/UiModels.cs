namespace ErCv.Web.UI
{
    public class AppSettings
    {
        public string Template { get; set; } = "harvard";
        public Dictionary<string, bool> SelectedFormats { get; set; } = new() { ["pdf"] = true, ["docx"] = false };
        public bool AllowTwoPages { get; set; }
    }

    public class HistoryEntry
    {
        public long Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string GeneratedAt { get; set; } = string.Empty;
        public int? MatchScore { get; set; }
        public string Template { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
    }

    public record TemplateOption(string Key, string Name, string Description);

    public record FormatOption(string Key, string Name);
}

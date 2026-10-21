using ErCv.Web.Constants;

namespace ErCv.Web.UI;

public class AppSettings
{
    public string Template { get; set; } = UiConstants.TemplateHarvard;
    public Dictionary<string, bool> SelectedFormats { get; set; } = new() { [ApiConstants.FormatPdf] = true, [ApiConstants.FormatDocx] = false };
    public bool AllowTwoPages { get; set; }

    // EDIT-04 and UX-10: the CV's language and look
    public string Language { get; set; } = ApiConstants.LanguageEnglish;
    public string Accent { get; set; } = ApiConstants.StyleDefault;
    public string Fonts { get; set; } = ApiConstants.StyleDefault;

    /// <summary><see cref="UiConstants.ProfileSourceDemo"/> or <see cref="UiConstants.ProfileSourceMine"/> (PROF-01).</summary>
    public string ProfileSource { get; set; } = UiConstants.ProfileSourceDemo;
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
    /// <summary>"demo", "mine" or "imported" (see <see cref="UiConstants"/>). An imported profile is not kept, so it cannot be restored.</summary>
    public string? ProfileSource { get; set; }
    public string? ProfileId { get; set; }
    public string? Language { get; set; }
    public string? Accent { get; set; }
    public string? Fonts { get; set; }

    // APP-01: the application this CV was made for. Entries saved before the tracker read as "Saved" with no notes.
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = ApplicationTracker.DefaultStatus;
    public string Notes { get; set; } = string.Empty;
}

public record TemplateOption(string Key, string Name, string Description);

public record FormatOption(string Key, string Name);

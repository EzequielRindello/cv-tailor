using ErCv.Web.Constants;

namespace ErCv.Web.Models;

/// <summary>
/// A cover letter (EDIT-03). Every paragraph is a list of parts: fixed wording from the letter's phrases, or a fact
/// quoted from the CV or the job post. Keeping them apart is what lets the tests prove nothing was made up.
/// </summary>
public class CoverLetter
{
    public string Language { get; set; } = ApiConstants.LanguageEnglish;
    public string Accent { get; set; } = ApiConstants.StyleDefault;
    public string Fonts { get; set; } = ApiConstants.StyleDefault;
    public Header Header { get; set; } = new();
    public string Date { get; set; } = string.Empty;
    public string? JobTitle { get; set; }

    /// <summary>The company the letter is for, quoted in the salutation and the closing; null when unknown (EDIT-09).</summary>
    public string? Company { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Salutation { get; set; } = string.Empty;
    public List<CoverLetterParagraph> Paragraphs { get; set; } = new();
    public string Closing { get; set; } = string.Empty;
}

public class CoverLetterParagraph
{
    /// <summary>
    /// Names the paragraph's subject ("opening", "closing", or the experience it is about), so an edit stays with
    /// its paragraph when the letter is written again (EDIT-09).
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public List<CoverLetterPart> Parts { get; set; } = new();

    /// <summary>The visitor's own text for this paragraph (EDIT-09); null while it is the generated one.</summary>
    public string? EditedText { get; set; }

    public bool IsEdited => EditedText != null;

    /// <summary>What the app wrote from the parts, whether or not the visitor changed it.</summary>
    public string GeneratedText => string.Concat(Parts.Select(p => p.Text));

    /// <summary>What the letter says: the visitor's text if they edited this paragraph, else the generated one.</summary>
    public string Text => EditedText ?? GeneratedText;
}

/// <summary>A piece of a paragraph. <see cref="IsFact"/> parts are quoted verbatim from the CV or the job post.</summary>
public record CoverLetterPart(string Text, bool IsFact);

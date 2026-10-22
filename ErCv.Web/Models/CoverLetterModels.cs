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
    public string Subject { get; set; } = string.Empty;
    public string Salutation { get; set; } = string.Empty;
    public List<CoverLetterParagraph> Paragraphs { get; set; } = new();
    public string Closing { get; set; } = string.Empty;
}

public class CoverLetterParagraph
{
    public List<CoverLetterPart> Parts { get; set; } = new();

    public string Text => string.Concat(Parts.Select(p => p.Text));
}

/// <summary>A piece of a paragraph. <see cref="IsFact"/> parts are quoted verbatim from the CV or the job post.</summary>
public record CoverLetterPart(string Text, bool IsFact);

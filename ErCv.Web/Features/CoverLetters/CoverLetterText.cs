using System.Text;
using ErCv.Web.Features.Documents;
using ErCv.Web.Models;

namespace ErCv.Web.Features.CoverLetters;

/// <summary>The cover letter as plain text, for a "message to the hiring team" field (EDIT-03 with EDIT-06).</summary>
public static class CoverLetterText
{
    public static string ToPlainText(CoverLetter letter)
    {
        var text = new StringBuilder();
        text.AppendLine(letter.Header.FullName);
        text.AppendLine(string.Join(DocumentConstants.DetailSeparator,
            new[] { letter.Header.Email, letter.Header.Phone, letter.Header.Location, letter.Header.LinkedIn, letter.Header.GitHub }
                .Where(c => !string.IsNullOrWhiteSpace(c))));
        text.AppendLine();
        text.AppendLine(letter.Date);
        if (!string.IsNullOrEmpty(letter.Subject)) text.AppendLine(letter.Subject);
        text.AppendLine();
        text.AppendLine(letter.Salutation);

        foreach (var paragraph in letter.Paragraphs)
        {
            text.AppendLine();
            text.AppendLine(paragraph.Text.Trim());
        }

        text.AppendLine();
        text.AppendLine(letter.Closing);
        text.AppendLine(letter.Header.FullName);
        return text.ToString();
    }
}

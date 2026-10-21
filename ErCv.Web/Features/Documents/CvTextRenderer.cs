using System.Globalization;
using System.Text;
using ErCv.Web.Models;

namespace ErCv.Web.Features.Documents;

/// <summary>
/// The CV as plain text or Markdown (EDIT-06), for the paste-in fields of an applicant tracking system.
/// Same sections and order as the DOCX, with nothing the CV does not already hold.
/// </summary>
public static class CvTextRenderer
{
    #region Constants

    // A dash reads as a list item both in a plain text field and in Markdown
    private const string ListBullet = "- ";
    private const string MarkdownTitle = "# ";
    private const string MarkdownSection = "## ";
    private const string MarkdownEntry = "### ";

    // Two trailing spaces are a Markdown line break; without them the next line joins the same paragraph
    private const string MarkdownLineBreak = "  ";

    // Characters Markdown would read as formatting when they appear in CV text
    private static readonly HashSet<char> MarkdownSpecials = new("\\`*_[]#<>|");

    #endregion

    #region Public Methods

    public static string ToPlainText(GeneratedCV cv) => Render(cv, markdown: false);

    public static string ToMarkdown(GeneratedCV cv) => Render(cv, markdown: true);

    #endregion

    #region Private Methods

    private static string Render(GeneratedCV cv, bool markdown)
    {
        var text = new StringBuilder();
        string Esc(string? value) => markdown ? EscapeMarkdown(value ?? "") : value ?? "";
        string Broken(string line) => markdown ? line + MarkdownLineBreak : line;

        void Section(string title)
        {
            text.AppendLine();
            text.AppendLine(markdown ? MarkdownSection + TitleCase(title) : title);
        }

        var header = cv.Header ?? new Header();
        text.AppendLine(markdown ? MarkdownTitle + Esc(header.FullName) : header.FullName.ToUpperInvariant());
        var contact = new[] { header.Email, header.Phone, header.Location, header.LinkedIn, header.GitHub }
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(Esc);
        text.AppendLine(string.Join(DocumentConstants.DetailSeparator, contact));

        if (!string.IsNullOrWhiteSpace(cv.Summary))
        {
            Section(DocumentConstants.SectionSummary);
            text.AppendLine(Esc(cv.Summary));
        }

        var education = cv.Education ?? new List<Education>();
        if (education.Any())
        {
            Section(DocumentConstants.SectionEducation);
            foreach (var edu in education)
            {
                if (markdown) text.AppendLine();
                var place = Join(", ", Esc(edu.Institution), Esc(edu.Location));
                text.AppendLine(Broken(Join(DocumentConstants.DetailSeparator, markdown ? Bold(place) : place, Esc(edu.GraduationDate))));

                var gpa = string.IsNullOrWhiteSpace(edu.GPA) ? "" : DocumentConstants.GpaLabel + Esc(edu.GPA);
                var details = Join(DocumentConstants.DetailSeparator, Esc(edu.Degree), gpa, Esc(edu.Honors));
                if (details.Length > 0) text.AppendLine(details);
            }
        }

        var experience = cv.Experience ?? new List<Experience>();
        if (experience.Any())
        {
            Section(DocumentConstants.SectionExperience);
            foreach (var exp in experience)
            {
                var dates = Join(DocumentConstants.DateRangeSeparator, Esc(exp.StartDate), Esc(exp.EndDate));
                if (markdown)
                {
                    text.AppendLine();
                    text.AppendLine(MarkdownEntry + Esc(exp.Role));
                    text.AppendLine(Join(DocumentConstants.DetailSeparator, Italic(Join(", ", Esc(exp.Company), Esc(exp.Location))), dates));
                    text.AppendLine();
                }
                else
                {
                    text.AppendLine(Join(DocumentConstants.DetailSeparator, exp.Role, dates));
                    text.AppendLine(Join(", ", exp.Company, exp.Location));
                }

                foreach (var bullet in exp.Bullets ?? new List<string>())
                    text.AppendLine(ListBullet + Esc(bullet));
            }
        }

        if (cv.Skills?.Any() == true)
        {
            Section(DocumentConstants.SectionSkills);
            text.AppendLine(string.Join(", ", cv.Skills.Select(Esc)));
        }

        if (cv.Projects?.Any() == true)
        {
            Section(DocumentConstants.SectionProjects);
            foreach (var project in cv.Projects)
            {
                if (markdown) text.AppendLine();
                var name = markdown ? Bold(Esc(project.Name)) : project.Name;
                text.AppendLine(Broken(Join(DocumentConstants.DetailSeparator, name, Esc(project.Link))));
                if (!string.IsNullOrWhiteSpace(project.Description)) text.AppendLine(Broken(Esc(project.Description)));
                if (project.Technologies?.Any() == true)
                {
                    var technologies = DocumentConstants.TechnologiesLabel + string.Join(", ", project.Technologies.Select(Esc));
                    text.AppendLine(markdown ? Italic(technologies) : technologies);
                }
            }
        }

        return text.ToString();
    }

    /// <summary>Joins the parts that have text, so a missing location or end date leaves no stray separator.</summary>
    private static string Join(string separator, params string?[] parts) =>
        string.Join(separator, parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static string Bold(string text) => text.Length == 0 ? text : $"**{text}**";

    private static string Italic(string text) => text.Length == 0 ? text : $"*{text}*";

    private static string TitleCase(string title) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLowerInvariant());

    private static string EscapeMarkdown(string text)
    {
        var escaped = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (MarkdownSpecials.Contains(c)) escaped.Append('\\');
            escaped.Append(c);
        }
        return escaped.ToString();
    }

    #endregion
}

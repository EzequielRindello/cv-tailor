using ErCv.Web.Constants;
using ErCv.Web.Features.Profiles;

namespace ErCv.Web.Features.Documents;

/// <summary>
/// The words a CV prints around the profile's content, in one language (EDIT-04): section titles, skill
/// categories, labels and dates. The profile's own text (roles, bullets, skills) is never translated.
/// </summary>
public sealed record CvLabels(
    string Language,
    string Summary,
    string Profile,
    string ExecutiveSummary,
    string Education,
    string Experience,
    string Skills,
    string Projects,
    string Technologies,
    string Gpa,
    string Technical,
    string ProgrammingLanguages,
    string SoftSkills,
    string Present,
    IReadOnlyList<string>? MonthNames)
{
    public static readonly CvLabels English = new(
        ApiConstants.LanguageEnglish, "Professional Summary", "Profile", "Executive Profile",
        "Education", "Experience", "Skills", "Projects", "Technologies: ", "GPA: ",
        "Technical", "Languages", "Soft Skills", "Present", null);

    public static readonly CvLabels Spanish = new(
        ApiConstants.LanguageSpanish, "Resumen profesional", "Perfil", "Perfil ejecutivo",
        "Formación", "Experiencia", "Habilidades", "Proyectos", "Tecnologías: ", "Promedio: ",
        "Técnicas", "Lenguajes", "Habilidades blandas", "Actualidad",
        new[] { "ene.", "feb.", "mar.", "abr.", "may.", "jun.", "jul.", "ago.", "sept.", "oct.", "nov.", "dic." });

    public static CvLabels For(string? language) => language == ApiConstants.LanguageSpanish ? Spanish : English;

    /// <summary>
    /// A profile date in this language. English keeps the profile's text as written; Spanish rewrites the
    /// formats the date parser knows ("Jan 2020", "2020-01", "01/2020", "Present") and leaves anything else alone.
    /// </summary>
    public string FormatDate(string? raw)
    {
        var text = raw?.Trim() ?? string.Empty;
        if (MonthNames == null || text.Length == 0) return text;
        if (DateParser.IsPresent(text)) return Present;
        if (!DateParser.TryParseStrict(text, out var year, out var month)) return text;

        return month is { } m ? $"{MonthNames[m - 1]} {year}" : year.ToString();
    }

    /// <summary>"Jan 2020 – Present", leaving out whichever end is missing.</summary>
    public string DateRange(string? start, string? end) =>
        string.Join(DocumentConstants.DateRangeSeparator,
            new[] { FormatDate(start), FormatDate(end) }.Where(d => d.Length > 0));
}

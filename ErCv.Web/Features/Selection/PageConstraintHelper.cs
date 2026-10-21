using ErCv.Web.Models;

namespace ErCv.Web.Features.Selection;

public class PageConstraintHelper
{
    #region Constants

    private const int LinesPerPage = 52;
    private const int CharsPerLine = 95;          // ~10pt Times New Roman at Letter width

    private const int HeaderLines = 3;
    private const int SummaryMinLines = 2;
    private const int SectionTitleLines = 1;
    private const int EducationLinesPerEntry = 2;
    private const int ExperienceTitleLines = 2;   // role line + company line
    private const int SkillsSectionLines = 3;
    private const int ProjectLines = 3;
    private const int BulletMinLines = 1;
    private const int TwoPages = 2;

    // Trimming never goes below these, even if the CV still does not fit
    private const int MinBulletsPerExperience = 2;
    private const int MinExperiences = 2;
    private const int MinSkills = 8;

    // Skills are trimmed one at a time but the line count only changes every few, so it is recounted every N removals
    private const int SkillsRecountInterval = 5;

    #endregion

    #region Public Methods

    public GeneratedCV EnforcePageConstraint(GeneratedCV source, bool allowTwoPages = false)
    {
        var cv = CvCloner.Clone(source);
        var maxLines = allowTwoPages ? LinesPerPage * TwoPages : LinesPerPage;
        var current = CalculateTotalLines(cv);

        if (current <= maxLines) return cv;
        while (current > maxLines &&
               cv.Experience.Any(e => (e.Bullets?.Count ?? 0) > MinBulletsPerExperience))
        {
            var exp = cv.Experience
                .Where(e => (e.Bullets?.Count ?? 0) > MinBulletsPerExperience)
                .OrderByDescending(e => e.Bullets!.Count)
                .First();
            exp.Bullets!.RemoveAt(exp.Bullets.Count - 1);
            current = CalculateTotalLines(cv);
        }
        if (current > maxLines && cv.Projects.Any())
        {
            cv.Projects.Clear();
            current = CalculateTotalLines(cv);
        }
        while (current > maxLines && cv.Experience.Count > MinExperiences)
        {
            cv.Experience.RemoveAt(cv.Experience.Count - 1);
            current = CalculateTotalLines(cv);
        }
        while (current > maxLines && cv.Skills.Count > MinSkills)
        {
            cv.Skills.RemoveAt(cv.Skills.Count - 1);
            if (cv.Skills.Count % SkillsRecountInterval == 0)
                current = CalculateTotalLines(cv);
        }

        return cv;
    }

    public GeneratedCV EnforceOnePageConstraint(GeneratedCV cv) =>
        EnforcePageConstraint(cv, allowTwoPages: false);

    public bool CanFitProject(GeneratedCV cv, bool allowTwoPages = false)
    {
        var maxLines = allowTwoPages ? LinesPerPage * TwoPages : LinesPerPage;
        return (CalculateTotalLines(cv) + SectionTitleLines + ProjectLines) <= maxLines;
    }

    public int CalculateTotalLines(GeneratedCV cv)
    {
        int lines = HeaderLines;

        if (!string.IsNullOrEmpty(cv.Summary))
            lines += EstimateLines(cv.Summary, SummaryMinLines);
        lines += SectionTitleLines;
        lines += (cv.Education?.Count ?? 0) * EducationLinesPerEntry;
        lines += SectionTitleLines;
        foreach (var exp in cv.Experience ?? new List<Experience>())
        {
            lines += ExperienceTitleLines;
            foreach (var bullet in exp.Bullets ?? new List<string>())
                lines += EstimateLines(bullet, BulletMinLines);
        }
        if (cv.Skills?.Any() == true)
            lines += SkillsSectionLines;
        if (cv.Projects?.Any() == true)
        {
            lines += SectionTitleLines;
            foreach (var proj in cv.Projects)
                lines += EstimateLines(proj.Description ?? "", ProjectLines);
        }

        return lines;
    }

    #endregion

    #region Private Methods

    private int EstimateLines(string text, int minimumLines)
    {
        if (string.IsNullOrEmpty(text)) return minimumLines;
        var estimated = (int)Math.Ceiling((double)text.Length / CharsPerLine);
        return Math.Max(minimumLines, estimated);
    }

    #endregion
}

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
    private const int OnePage = 1;
    private const int TwoPages = 2;

    // Trimming never goes below these, even if the CV still does not fit
    private const int MinBulletsPerExperience = 2;
    private const int MinExperiences = 2;
    private const int MinSkills = 8;

    #endregion

    #region Public Methods

    /// <summary>First pass (F6): trims by the character estimate, before anything is rendered.</summary>
    public GeneratedCV EnforcePageConstraint(GeneratedCV source, bool allowTwoPages = false)
    {
        var cv = CvCloner.Clone(source);
        var maxLines = allowTwoPages ? LinesPerPage * TwoPages : LinesPerPage;

        while (CalculateTotalLines(cv) > maxLines && TrimOneStep(cv))
        {
        }

        return cv;
    }

    public GeneratedCV EnforceOnePageConstraint(GeneratedCV cv) =>
        EnforcePageConstraint(cv, allowTwoPages: false);

    /// <summary>
    /// Second pass (MATCH-08): renders the CV and counts the real pages. When it spills over, it keeps trimming in
    /// the same order as the estimate, binary searching the number of steps so only a few renders are needed.
    /// The render of the returned CV comes back too, so the caller does not render it again.
    /// </summary>
    public async Task<(GeneratedCV Cv, T Render)> FitMeasuredAsync<T>(
        GeneratedCV source, bool allowTwoPages, Func<GeneratedCV, Task<T>> render, Func<T, int> countPages,
        Action<int, int>? onRender = null)
    {
        var maxPages = allowTwoPages ? TwoPages : OnePage;
        var first = await render(source);
        if (countPages(first) <= maxPages)
        {
            onRender?.Invoke(1, 1);
            return (source, first);
        }

        var stepsAvailable = CountTrimSteps(source);
        if (stepsAvailable == 0)
        {
            onRender?.Invoke(1, 1);
            return (source, first);
        }

        // The first render plus the bisection: about log2 of the steps, so a progress bar knows how far it is (UX-11)
        var expectedRenders = 1 + (int)Math.Ceiling(Math.Log2(stepsAvailable + 1));
        var renders = 1;
        onRender?.Invoke(renders, expectedRenders);

        (GeneratedCV Cv, T Render)? fit = null;
        var mostTrimmed = (Cv: source, Render: first);
        var low = 1;
        var high = stepsAvailable;

        // Removing content never adds a page, so the smallest number of steps that fits is found by bisection
        while (low <= high)
        {
            var steps = low + (high - low) / 2;
            var candidate = Trimmed(source, steps);
            var rendered = await render(candidate);
            onRender?.Invoke(Math.Min(++renders, expectedRenders), expectedRenders);

            if (countPages(rendered) <= maxPages)
            {
                fit = (candidate, rendered);
                high = steps - 1;
            }
            else
            {
                if (steps == stepsAvailable) mostTrimmed = (candidate, rendered);
                low = steps + 1;
            }
        }

        // When nothing fits, the search has climbed to the last step, so the most trimmed CV is already rendered
        return fit ?? mostTrimmed;
    }

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

    /// <summary>
    /// Removes the least valuable piece left, in this order: the last bullet of the experience with the most,
    /// then the projects, then the oldest experience, then the last skill. False when nothing can go.
    /// </summary>
    private static bool TrimOneStep(GeneratedCV cv)
    {
        var withSpareBullets = cv.Experience
            .Where(e => (e.Bullets?.Count ?? 0) > MinBulletsPerExperience)
            .OrderByDescending(e => e.Bullets!.Count)
            .FirstOrDefault();
        if (withSpareBullets != null)
        {
            withSpareBullets.Bullets.RemoveAt(withSpareBullets.Bullets.Count - 1);
            return true;
        }

        if (cv.Projects.Any())
        {
            cv.Projects.Clear();
            return true;
        }

        if (cv.Experience.Count > MinExperiences)
        {
            cv.Experience.RemoveAt(cv.Experience.Count - 1);
            return true;
        }

        if (cv.Skills.Count > MinSkills)
        {
            cv.Skills.RemoveAt(cv.Skills.Count - 1);
            return true;
        }

        return false;
    }

    private static int CountTrimSteps(GeneratedCV source)
    {
        var cv = CvCloner.Clone(source);
        var steps = 0;
        while (TrimOneStep(cv)) steps++;
        return steps;
    }

    private static GeneratedCV Trimmed(GeneratedCV source, int steps)
    {
        var cv = CvCloner.Clone(source);
        for (var i = 0; i < steps && TrimOneStep(cv); i++)
        {
        }
        return cv;
    }

    private int EstimateLines(string text, int minimumLines)
    {
        if (string.IsNullOrEmpty(text)) return minimumLines;
        var estimated = (int)Math.Ceiling((double)text.Length / CharsPerLine);
        return Math.Max(minimumLines, estimated);
    }

    #endregion
}

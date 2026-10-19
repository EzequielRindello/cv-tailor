using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class PageConstraintHelper
    {
        private const int LinesPerPage = 52;
        private const int CharsPerLine = 95;          // ~10pt Times New Roman at Letter width

        private const int HeaderLines = 3;
        private const int SummaryMinLines = 2;
        private const int SectionTitleLines = 1;
        private const int EducationLinesPerEntry = 2;
        private const int ExperienceTitleLines = 2;   // role line + company line
        private const int SkillsSectionLines = 3;
        private const int ProjectLines = 3;

        public GeneratedCV EnforcePageConstraint(GeneratedCV cv, bool allowTwoPages = false)
        {
            var maxLines = allowTwoPages ? LinesPerPage * 2 : LinesPerPage;
            var current = CalculateTotalLines(cv);

            if (current <= maxLines) return cv;
            while (current > maxLines &&
                   cv.Experience.Any(e => (e.Bullets?.Count ?? 0) > 2))
            {
                var exp = cv.Experience
                    .Where(e => (e.Bullets?.Count ?? 0) > 2)
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
            while (current > maxLines && cv.Experience.Count > 2)
            {
                cv.Experience.RemoveAt(cv.Experience.Count - 1);
                current = CalculateTotalLines(cv);
            }
            while (current > maxLines && cv.Skills.Count > 8)
            {
                cv.Skills.RemoveAt(cv.Skills.Count - 1);
                if (cv.Skills.Count % 5 == 0)
                    current = CalculateTotalLines(cv);
            }

            return cv;
        }

        public GeneratedCV EnforceOnePageConstraint(GeneratedCV cv) =>
            EnforcePageConstraint(cv, allowTwoPages: false);

        public bool CanFitProject(GeneratedCV cv, bool allowTwoPages = false)
        {
            var maxLines = allowTwoPages ? LinesPerPage * 2 : LinesPerPage;
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
                    lines += EstimateLines(bullet, 1);
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
        private int EstimateLines(string text, int minimumLines)
        {
            if (string.IsNullOrEmpty(text)) return minimumLines;
            var estimated = (int)Math.Ceiling((double)text.Length / CharsPerLine);
            return Math.Max(minimumLines, estimated);
        }
    }
}

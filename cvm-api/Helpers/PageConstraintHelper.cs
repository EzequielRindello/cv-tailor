using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class PageConstraintHelper
    {
        private const int LinesPerPage = 48;
        private const int HeaderLines = 3;
        private const int SummaryLines = 3;
        private const int EducationLinesPerEntry = 2;
        private const int ExperienceTitleLines = 2;
        private const int BulletLines = 1;
        private const int SkillsSectionLines = 3;
        private const int ProjectLines = 4;

        public GeneratedCV EnforcePageConstraint(GeneratedCV cv, bool allowTwoPages = false)
        {
            var maxLines = allowTwoPages ? LinesPerPage * 2 : LinesPerPage;
            var currentLines = CalculateTotalLines(cv);

            if (currentLines <= maxLines) return cv;

            // 1. Reduce bullets (minimum 2 per experience)
            while (currentLines > maxLines && cv.Experience.Any(e => e.Bullets.Count > 2))
            {
                var expWithMost = cv.Experience.OrderByDescending(e => e.Bullets.Count).First();
                expWithMost.Bullets.RemoveAt(expWithMost.Bullets.Count - 1);
                currentLines--;
            }

            // 2.Remove last experience if more than 2 remain
            while (currentLines > maxLines && cv.Experience.Count > 2)
            {
                var lastExp = cv.Experience.Last();
                currentLines -= (ExperienceTitleLines + lastExp.Bullets.Count);
                cv.Experience.RemoveAt(cv.Experience.Count - 1);
            }

            // 3. Delete projects
            if (currentLines > maxLines && cv.Projects.Any())
            {
                currentLines -= (1 + cv.Projects.Count * ProjectLines);
                cv.Projects.Clear();
            }

            // 4. Reducir skills si hay más de 8
            while (currentLines > maxLines && cv.Skills.Count > 8)
            {
                cv.Skills.RemoveAt(cv.Skills.Count - 1);
                if (cv.Skills.Count % 5 == 0) currentLines--;
            }

            return cv;
        }

        // Backward compatibility with the previous method
        public GeneratedCV EnforceOnePageConstraint(GeneratedCV cv) =>
            EnforcePageConstraint(cv, allowTwoPages: false);

        public bool CanFitProject(GeneratedCV cv, bool allowTwoPages = false)
        {
            var maxLines = allowTwoPages ? LinesPerPage * 2 : LinesPerPage;
            return (CalculateTotalLines(cv) + ProjectLines) <= maxLines;
        }

        public int CalculateTotalLines(GeneratedCV cv)
        {
            int lines = HeaderLines;

            if (!string.IsNullOrEmpty(cv.Summary))
                lines += SummaryLines;

            lines += 1; // Education section title
            lines += cv.Education.Count * EducationLinesPerEntry;

            lines += 1; // Experience section title
            foreach (var exp in cv.Experience)
                lines += ExperienceTitleLines + exp.Bullets.Count * BulletLines;

            if (cv.Skills.Any())
                lines += SkillsSectionLines;

            if (cv.Projects.Any())
                lines += 1 + cv.Projects.Count * ProjectLines;

            return lines;
        }
    }
}

using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    public class PageConstraintHelper
    {
        private const int MaxLinesPerPage = 48;
        private const int HeaderLines = 3;
        private const int SummaryLines = 3;
        private const int EducationLinesPerEntry = 2;
        private const int ExperienceTitleLines = 2;
        private const int BulletLines = 1;
        private const int SkillsSectionLines = 3;
        private const int ProjectLines = 4;

        public GeneratedCV EnforceOnePageConstraint(GeneratedCV cv)
        {
            var currentLines = CalculateTotalLines(cv);

            if (currentLines <= MaxLinesPerPage)
            {
                return cv;
            }

            // Reduce bullets from experiences
            while (currentLines > MaxLinesPerPage && cv.Experience.Any(e => e.Bullets.Count > 2))
            {
                // Find experience with most bullets
                var expWithMostBullets = cv.Experience
                    .OrderByDescending(e => e.Bullets.Count)
                    .First();

                if (expWithMostBullets.Bullets.Count > 2)
                {
                    // Remove last bullet
                    expWithMostBullets.Bullets.RemoveAt(expWithMostBullets.Bullets.Count - 1);
                    currentLines--;
                }
            }

            // If still too long, remove last experience
            while (currentLines > MaxLinesPerPage && cv.Experience.Count > 2)
            {
                var lastExp = cv.Experience.Last();
                var linesInExp = ExperienceTitleLines + lastExp.Bullets.Count;
                cv.Experience.RemoveAt(cv.Experience.Count - 1);
                currentLines -= linesInExp;
            }

            // Remove projects if still too long
            if (currentLines > MaxLinesPerPage && cv.Projects.Any())
            {
                currentLines -= (cv.Projects.Count * ProjectLines);
                cv.Projects.Clear();
            }

            // Reduce skills if needed
            while (currentLines > MaxLinesPerPage && cv.Skills.Count > 8)
            {
                cv.Skills.RemoveAt(cv.Skills.Count - 1);
                if (cv.Skills.Count % 5 == 0) // Assume skills wrap every 5
                {
                    currentLines--;
                }
            }

            return cv;
        }

        public bool CanFitProject(GeneratedCV cv)
        {
            var currentLines = CalculateTotalLines(cv);
            return (currentLines + ProjectLines) <= MaxLinesPerPage;
        }

        private int CalculateTotalLines(GeneratedCV cv)
        {
            int lines = 0;

            // Header
            lines += HeaderLines;

            // Summary
            if (!string.IsNullOrEmpty(cv.Summary))
            {
                lines += SummaryLines;
            }

            // Education
            lines += 1; // Section title
            lines += cv.Education.Count * EducationLinesPerEntry;

            // Experience
            lines += 1; // Section title
            foreach (var exp in cv.Experience)
            {
                lines += ExperienceTitleLines;
                lines += exp.Bullets.Count * BulletLines;
            }

            // Skills
            if (cv.Skills.Any())
            {
                lines += SkillsSectionLines;
            }

            // Projects
            if (cv.Projects.Any())
            {
                lines += 1; // Section title
                lines += cv.Projects.Count * ProjectLines;
            }

            return lines;
        }
    }
}

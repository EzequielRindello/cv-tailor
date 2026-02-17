using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class ATSChecker
    {
        private static readonly HashSet<string> AtsFriendlyFonts = new(StringComparer.OrdinalIgnoreCase)
        {
            "times new roman", "arial", "calibri", "georgia", "helvetica", "garamond"
        };

        private static readonly List<string> StandardSections = new()
        {
            "education", "experience", "skills", "projects", "summary"
        };

        public ATSScore Check(GeneratedCV cv, JobRequirements requirements, CVTemplate template)
        {
            var score = 100;
            var issues = new List<ATSIssue>();

            // Font Check
            if (template == CVTemplate.Modern)
            {
                // Modern uses Helvetica/Arial - OK
            }
            else if (template == CVTemplate.Minimalist)
            {
                // Minimalist uses Helvetica Neue - OK
            }
            // Harvard uses Times New Roman - always OK

            if (!IsValidEmail(cv.Header?.Email))
            {
                score -= 5;
                issues.Add(new ATSIssue
                {
                    Severity = "Medium",
                    Issue = "Email not detected or invalid format",
                    Fix = "Use the format: name@domain.com"
                });
            }

            if (!IsValidPhone(cv.Header?.Phone))
            {
                score -= 5;
                issues.Add(new ATSIssue
                {
                    Severity = "Low",
                    Issue = "Phone format may not be parsed correctly by some ATS systems",
                    Fix = "Use format: +1 (234) 567-8900 or +54 11 1234-5678"
                });
            }

            // Keyword Density
            var cvText = BuildCVText(cv);
            var totalKeywords = Math.Max(requirements.TechnicalSkills.Count, 1);
            var matchedKeywords = requirements.TechnicalSkills
                .Count(s => cvText.Contains(s, StringComparison.OrdinalIgnoreCase));
            var density = (double)matchedKeywords / totalKeywords;

            if (density < 0.4)
            {
                score -= 20;
                issues.Add(new ATSIssue
                {
                    Severity = "High",
                    Issue = $"Low keyword density ({density:P0}) — ATS may reject the CV",
                    Fix = "Include more keywords from the job description in bullets and skills"
                });
            }
            else if (density < 0.6)
            {
                score -= 10;
                issues.Add(new ATSIssue
                {
                    Severity = "Medium",
                    Issue = $"Medium keyword density ({density:P0})",
                    Fix = "Add 2–3 more keywords from the job description to improve ranking"
                });
            }

            // Section Headings
            var hasExperience = cv.Experience?.Any() == true;
            var hasEducation = cv.Education?.Any() == true;
            var hasSkills = cv.Skills?.Any() == true;

            if (!hasExperience || !hasEducation || !hasSkills)
            {
                score -= 10;
                issues.Add(new ATSIssue
                {
                    Severity = "High",
                    Issue = "Missing required sections (Education / Experience / Skills)",
                    Fix = "Verify that cv-master.json contains data in all required sections"
                });
            }

            // Bullet Points
            var hasShortBullets = cv.Experience?
                .SelectMany(e => e.Bullets ?? new List<string>())
                .Any(b => b.Length < 30) == true;

            if (hasShortBullets)
            {
                score -= 5;
                issues.Add(new ATSIssue
                {
                    Severity = "Low",
                    Issue = "Some bullet points are too short (<30 characters)",
                    Fix = "Expand bullet points with more detail and context"
                });
            }

            // Quantifiable Results
            var hasMetrics = cv.Experience?
                .SelectMany(e => e.Bullets ?? new List<string>())
                .Any(b => Regex.IsMatch(b, @"\d+[%+]|\d+[MK]\+?|[\d,]+\+?")) == true;

            if (!hasMetrics)
            {
                score -= 5;
                issues.Add(new ATSIssue
                {
                    Severity = "Medium",
                    Issue = "No quantifiable achievements detected (numbers, percentages)",
                    Fix = "Add metrics to your bullets: 'Reduced X by 30%', 'Managed N users'"
                });
            }

            // Template-specific checks
            if (template == CVTemplate.Modern)
            {
                // A 2-column layout can confuse some basic ATS systems.
                score -= 5;
                issues.Add(new ATSIssue
                {
                    Severity = "Low",
                    Issue = "Modern template uses two columns — some basic ATS systems may parse it incorrectly",
                    Fix = "For strict ATS systems, use the Harvard (single-column) template"
                });
            }

            score = Math.Max(0, score);

            return new ATSScore
            {
                Overall = score,
                Issues = issues.OrderByDescending(i => GetSeverityWeight(i.Severity)).ToList(),
                Recommendations = BuildRecommendations(issues, score),
                ColorClass = score >= 85 ? "green" : score >= 65 ? "yellow" : "red"
            };
        }

        private bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private bool IsValidPhone(string phone) =>
            !string.IsNullOrEmpty(phone) && phone.Length >= 8;

        private string BuildCVText(GeneratedCV cv)
        {
            var parts = new List<string>();
            parts.AddRange(cv.Skills ?? new List<string>());
            foreach (var exp in cv.Experience ?? new List<Experience>())
            {
                parts.Add(exp.Role ?? "");
                parts.AddRange(exp.Bullets ?? new List<string>());
                parts.AddRange(exp.Skills ?? new List<string>());
            }
            return string.Join(" ", parts);
        }

        private int GetSeverityWeight(string severity) => severity switch
        {
            "Critical" => 4,
            "High" => 3,
            "Medium" => 2,
            "Low" => 1,
            _ => 0
        };

        private List<string> BuildRecommendations(List<ATSIssue> issues, int score)
        {
            var recs = new List<string>();

            if (!issues.Any())
            {
                recs.Add("✅ Your CV is fully optimized for ATS");
                return recs;
            }

            recs.Add($"{issues.Count} improvement point(s) detected:");
            foreach (var issue in issues.Take(4))
                recs.Add($"• {issue.Issue} → {issue.Fix}");

            return recs;
        }
    }
}

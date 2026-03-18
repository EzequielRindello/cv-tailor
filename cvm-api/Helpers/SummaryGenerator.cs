using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class SummaryGenerator
    {
        private static readonly string[] TechRoleKeywords =
        {
            "developer","engineer","software","programmer","architect",
            "devops","sre","data","backend","frontend","fullstack",
            "full stack","full-stack","cloud","platform","tech"
        };

        private static readonly Dictionary<string, string[]> RoleTypeSignals =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Data Engineer",       new[] { "etl","spark","airflow","dbt","pipeline","warehouse","databricks" } },
                { "ML Engineer",         new[] { "tensorflow","pytorch","machine learning","scikit","mlflow" } },
                { "DevOps Engineer",     new[] { "terraform","ansible","kubernetes","helm","argocd","infrastructure","sre" } },
                { "Frontend Developer",  new[] { "react","vue","angular","css","html","figma","tailwind","ui" } },
                { "Backend Developer",   new[] { "postgresql","mysql","rest api","grpc","kafka","rabbitmq","spring","node.js" } },
                { "Mobile Developer",    new[] { "swift","kotlin","flutter","react native","ios","android","xcode" } },
                { "Full Stack Developer",new[] { "fullstack","full stack","full-stack","end-to-end" } },
            };

        public string GenerateSummary(
            CVMaster cvMaster,
            JobRequirements requirements,
            List<Experience> selectedExperiences,
            List<string> topSkills)
        {
            var yearsOfExperience = CalculateTechYearsOfExperience(cvMaster.Experience);
            var candidateTitle = DeriveRoleFromCV(cvMaster.Experience, requirements);

            var keyTechSkills = topSkills
                .Where(s => requirements.TechnicalSkills.Any(ts => SkillNormalizer.FuzzyMatch(s, ts)))
                .Take(5).ToList();

            if (!keyTechSkills.Any()) keyTechSkills = topSkills.Take(4).ToList();

            var parts = new List<string>();
            parts.Add($"{candidateTitle} with {yearsOfExperience}+ years of experience");
            if (keyTechSkills.Any()) parts.Add($"specializing in {FormatSkillsList(keyTechSkills)}");

            var achievement = ExtractTopAchievement(selectedExperiences);
            if (!string.IsNullOrEmpty(achievement)) parts.Add($"Proven track record of {achievement}");

            var softSkills = ExtractSoftSkills(requirements, selectedExperiences);
            if (softSkills.Any()) parts.Add($"Strong {string.Join(" and ", softSkills.Take(2))} skills");

            var summary = string.Join(". ", parts) + ".";
            if (summary.Length > 300) summary = summary.Substring(0, 297) + "...";
            return summary;
        }

        private string DeriveRoleFromCV(List<Experience> allExperiences, JobRequirements requirements)
        {
            var mostRecentTech = allExperiences
                .Where(e => IsTechRole(e.Role))
                .OrderByDescending(e => e.EndDate?.ToLower() == "present" ? "zzz" : e.EndDate ?? "")
                .FirstOrDefault();

            var candidateSeniority = ExtractSeniorityFromRole(mostRecentTech?.Role ?? "");
            var roleType = DetectRoleTypeFromJD(requirements);

            return string.IsNullOrEmpty(candidateSeniority) ? roleType : $"{candidateSeniority} {roleType}";
        }

        private string ExtractSeniorityFromRole(string role)
        {
            var prefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "senior", "Senior" }, { "lead", "Lead" }, { "principal", "Principal" },
                { "staff", "Staff" },   { "junior", "Junior" }, { "associate", "Associate" },
                { "intern", "Intern" }, { "mid", "" }
            };
            foreach (var kvp in prefixes)
                if (role.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            return "";
        }

        private string DetectRoleTypeFromJD(JobRequirements requirements)
        {
            var allSignals = requirements.TechnicalSkills
                .Concat(requirements.Keywords)
                .Select(s => s.ToLower()).ToList();

            var scores = new Dictionary<string, int>();
            foreach (var kvp in RoleTypeSignals)
            {
                var score = kvp.Value.Count(signal => allSignals.Any(s => s.Contains(signal)));
                if (score > 0) scores[kvp.Key] = score;
            }

            if (!scores.Any()) return "Software Engineer";

            var hasFrontend = scores.ContainsKey("Frontend Developer");
            var hasBackend = scores.ContainsKey("Backend Developer");
            if (hasFrontend && hasBackend) return "Full Stack Developer";

            return scores.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        private int CalculateTechYearsOfExperience(List<Experience> allExperiences)
        {
            var pool = allExperiences.Where(e => IsTechRole(e.Role)).ToList();
            if (!pool.Any()) pool = allExperiences;

            var earliestYear = DateTime.Now.Year;
            foreach (var exp in pool)
            {
                if (TryExtractYear(exp.StartDate, out int year))
                    earliestYear = Math.Min(earliestYear, year);
                else if (!string.IsNullOrWhiteSpace(exp.StartDate))
                    Console.WriteLine($"[WARN] SummaryGenerator: could not parse startDate '{exp.StartDate}', ignoring entry for years calculation.");
            }
            return Math.Max(0, DateTime.Now.Year - earliestYear);
        }

        private static bool TryExtractYear(string? value, out int year)
        {
            year = 0;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var match = Regex.Match(value, @"\b(19|20)\d{2}\b");
            if (!match.Success) return false;
            if (!int.TryParse(match.Value, out year)) return false;

            var currentYear = DateTime.Now.Year;
            return year >= 1950 && year <= currentYear + 1;
        }

        private bool IsTechRole(string? role) =>
            !string.IsNullOrEmpty(role) &&
            TechRoleKeywords.Any(k => role.Contains(k, StringComparison.OrdinalIgnoreCase));

        private string ExtractTopAchievement(List<Experience> experiences)
        {
            var best = ""; var bestPct = 0;
            foreach (var exp in experiences)
                foreach (var bullet in exp.Bullets ?? new())
                {
                    var m = Regex.Match(bullet,
                        @"(improving|increasing|reducing|achieving|exceeding|delivering|cutting|boosting).*?(\d+)%",
                        RegexOptions.IgnoreCase);
                    if (m.Success && int.TryParse(m.Groups[2].Value, out int pct) && pct > bestPct)
                    { bestPct = pct; best = m.Value.ToLower(); }
                }
            return best;
        }

        private string FormatSkillsList(List<string> skills)
        {
            if (skills.Count == 1) return skills[0];
            if (skills.Count == 2) return $"{skills[0]} and {skills[1]}";
            return $"{string.Join(", ", skills.Take(skills.Count - 1))}, and {skills.Last()}";
        }

        private List<string> ExtractSoftSkills(JobRequirements requirements, List<Experience> experiences)
        {
            var map = new Dictionary<string, string>
            {
                { "team","team collaboration" }, { "leadership","leadership" },
                { "communication","communication" }, { "problem","problem-solving" },
                { "agile","agile methodologies" }, { "mentor","mentoring" },
                { "collaborat","cross-functional collaboration" }
            };
            var result = new List<string>();
            foreach (var kvp in map)
            {
                if (requirements.Keywords.Any(k => k.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) ||
                    experiences.Any(e => e.Bullets?.Any(b => b.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) == true))
                    result.Add(kvp.Value);
                if (result.Count >= 2) break;
            }
            return result;
        }
    }
}

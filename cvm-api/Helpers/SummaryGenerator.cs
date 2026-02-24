using ErCvApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErCvApi.Helpers
{
    public class SummaryGenerator
    {
        // Roles we consider "software/tech" — used to filter YoE calculation
        private static readonly HashSet<string> TechRoleKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "developer", "engineer", "software", "programmer", "architect",
            "devops", "sre", "data", "backend", "frontend", "fullstack",
            "full stack", "full-stack", "tech", "it ", "cloud", "analyst",
            "consultant", "specialist", "lead", "cto", "cio", "vp of"
        };

        public string GenerateSummary(
            CVMaster cvMaster,
            JobRequirements requirements,
            List<Experience> selectedExperiences,
            List<string> topSkills)
        {
            var yearsOfExperience = CalculateTechYearsOfExperience(cvMaster.Experience);
            var primaryRole = DeterminePrimaryRole(selectedExperiences, requirements);

            var keyTechSkills = topSkills
                .Where(s => IsRelevantForSummary(s, requirements))
                .Take(5)
                .ToList();

            var parts = new List<string>();

            // Opening: role + years
            var yearsText = yearsOfExperience > 0 ? $" with {yearsOfExperience}+ years of experience" : "";
            parts.Add($"{primaryRole}{yearsText}");

            // Specialization: top skills from the JD
            if (keyTechSkills.Any())
                parts.Add($"specializing in {FormatSkillsList(keyTechSkills)}");

            // Achievement: best quantifiable bullet
            var achievement = ExtractTopAchievement(selectedExperiences);
            if (!string.IsNullOrEmpty(achievement))
                parts.Add($"Proven track record of {achievement}");

            // Soft skills from JD context
            var softSkills = ExtractSoftSkills(requirements, selectedExperiences);
            if (softSkills.Any())
                parts.Add($"Strong {string.Join(" and ", softSkills.Take(2))} skills");

            var summary = string.Join(". ", parts) + ".";

            // Hard cap at 300 chars for one-liners
            if (summary.Length > 350)
                summary = summary.Substring(0, 347) + "...";

            return summary;
        }

        // ─── Years of Experience ─────────────────────────────────────────────
        // Only counts tech/software roles — excludes unrelated past jobs
        private int CalculateTechYearsOfExperience(List<Experience> allExperiences)
        {
            var techExperiences = allExperiences
                .Where(e => IsTechRole(e.Role))
                .ToList();

            if (!techExperiences.Any())
            {
                // Fallback: use all experiences if no tech ones found
                techExperiences = allExperiences;
            }

            var earliestStart = DateTime.Now;

            foreach (var exp in techExperiences)
            {
                if (TryParseDate(exp.StartDate, out var date))
                    if (date < earliestStart)
                        earliestStart = date;
            }

            var years = (int)Math.Floor((DateTime.Now - earliestStart).TotalDays / 365.25);
            return Math.Max(0, years);
        }

        private bool IsTechRole(string role)
        {
            if (string.IsNullOrEmpty(role)) return false;
            return TechRoleKeywords.Any(k => role.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryParseDate(string dateStr, out DateTime date)
        {
            date = DateTime.Now;
            if (string.IsNullOrWhiteSpace(dateStr)) return false;

            // "Jun 2025" / "Jan 2023" format
            var parts = dateStr.Trim().Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int year))
            {
                var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    {"jan",1},{"feb",2},{"mar",3},{"apr",4},{"may",5},{"jun",6},
                    {"jul",7},{"aug",8},{"sep",9},{"oct",10},{"nov",11},{"dec",12}
                };

                if (monthMap.TryGetValue(parts[0], out int month))
                {
                    date = new DateTime(year, month, 1);
                    return true;
                }

                // Just a year
                date = new DateTime(year, 1, 1);
                return true;
            }

            return false;
        }

        // ─── Primary Role ────────────────────────────────────────────────────
        // Maps to the actual role the JD is asking for — not a generic fallback
        private string DeterminePrimaryRole(
            List<Experience> selectedExperiences,
            JobRequirements requirements)
        {
            var seniority = requirements.SeniorityLevel?.ToLower() ?? "";
            var keywords = requirements.Keywords;
            var techSkills = requirements.TechnicalSkills;

            // Detect role type from JD keywords
            var roleType = DetectRoleTypeFromJD(keywords, techSkills);

            // Detect seniority prefix
            var seniorityPrefix = seniority switch
            {
                "senior" => "Senior ",
                "lead" => "Lead ",
                "principal" => "Principal ",
                "staff" => "Staff ",
                "junior" => "Junior ",
                "intern" => "Junior ",
                _ => ""
            };

            return seniorityPrefix + roleType;
        }

        private string DetectRoleTypeFromJD(List<string> keywords, List<string> techSkills)
        {
            var allContext = string.Join(" ", keywords).ToLower();
            var allTech = string.Join(" ", techSkills).ToLower();

            // Check for specific role types in order of specificity
            if (ContainsAny(allContext, "data engineer", "data pipeline", "etl", "spark", "airflow", "dbt"))
                return "Data Engineer";

            if (ContainsAny(allContext, "machine learning", "ml engineer", "ai engineer") ||
                ContainsAny(allTech, "tensorflow", "pytorch", "scikit"))
                return "Machine Learning Engineer";

            if (ContainsAny(allContext, "devops", "sre", "infrastructure", "platform engineer") ||
                ContainsAny(allTech, "kubernetes", "terraform", "ansible", "helm"))
                return "DevOps Engineer";

            if (ContainsAny(allContext, "mobile", "ios", "android", "react native", "flutter"))
                return "Mobile Developer";

            if (ContainsAny(allContext, "frontend", "front-end", "front end", "ui developer") &&
                !ContainsAny(allContext, "backend", "back-end", "fullstack", "full stack"))
                return "Frontend Developer";

            if (ContainsAny(allContext, "backend", "back-end", "back end") &&
                !ContainsAny(allContext, "frontend", "front-end", "fullstack", "full stack"))
                return "Backend Developer";

            if (ContainsAny(allContext, "fullstack", "full stack", "full-stack"))
                return "Full Stack Developer";

            // Fallback based on tech mix
            var hasFrontend = ContainsAny(allTech, "react", "angular", "vue", "next", "typescript");
            var hasBackend = ContainsAny(allTech, "node", "asp.net", ".net", "django", "spring", "express");
            var hasBoth = hasFrontend && hasBackend;

            if (hasBoth) return "Full Stack Developer";
            if (hasFrontend) return "Frontend Developer";
            if (hasBackend) return "Backend Developer";

            // Use the current role from selected experiences
            if (selectedExperiences.Any())
            {
                var role = selectedExperiences.First().Role;
                if (role.Contains("Developer", StringComparison.OrdinalIgnoreCase) ||
                    role.Contains("Engineer", StringComparison.OrdinalIgnoreCase))
                    return "Software Developer";
            }

            return "Software Developer";
        }

        private List<Experience> selectedExperiences = new(); // temp holder for fallback

        // Overload that stores experiences for DetectRoleTypeFromJD fallback
        private string DeterminePrimaryRole(
            List<Experience> experiences,
            JobRequirements requirements,
            bool _)
        {
            selectedExperiences = experiences;
            return DeterminePrimaryRole(experiences, requirements);
        }

        private bool ContainsAny(string text, params string[] terms)
            => terms.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));

        // ─── Achievement ─────────────────────────────────────────────────────
        // Pick the best achievement — varies by selecting highest % metric found
        private string ExtractTopAchievement(List<Experience> experiences)
        {
            var bestMatch = "";
            var bestValue = 0;

            foreach (var exp in experiences)
            {
                foreach (var bullet in exp.Bullets ?? new List<string>())
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        bullet,
                        @"(improving|increasing|reducing|achieving|exceeding|delivering|optimizing|cutting|saving|growing|boosting|scaling).*?(\d+)([%x])",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (match.Success && int.TryParse(match.Groups[2].Value, out int value))
                    {
                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestMatch = match.Value.ToLower();
                        }
                    }
                }
            }

            return bestMatch;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private bool IsRelevantForSummary(string skill, JobRequirements requirements)
        {
            return requirements.TechnicalSkills.Any(ts =>
                ts.Equals(skill, StringComparison.OrdinalIgnoreCase) ||
                skill.Contains(ts, StringComparison.OrdinalIgnoreCase));
        }

        private string FormatSkillsList(List<string> skills)
        {
            if (skills.Count == 1) return skills[0];
            if (skills.Count == 2) return $"{skills[0]} and {skills[1]}";
            return $"{string.Join(", ", skills.Take(skills.Count - 1))}, and {skills.Last()}";
        }

        private List<string> ExtractSoftSkills(
            JobRequirements requirements,
            List<Experience> experiences)
        {
            var softSkillKeywords = new Dictionary<string, string>
            {
                { "team", "team collaboration" },
                { "leadership", "leadership" },
                { "communication", "communication" },
                { "problem", "problem-solving" },
                { "agile", "agile methodologies" },
                { "mentor", "mentoring" },
                { "cross-functional", "cross-functional collaboration" },
                { "stakeholder", "stakeholder communication" },
                { "remote", "remote collaboration" },
            };

            var result = new List<string>();

            foreach (var kvp in softSkillKeywords)
            {
                if (requirements.Keywords.Any(k => k.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) ||
                    experiences.Any(e => e.Bullets?.Any(b =>
                        b.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) == true))
                {
                    result.Add(kvp.Value);
                }

                if (result.Count >= 2) break;
            }

            return result;
        }
    }
}

using ErCvApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ErCvApi.Helpers
{
    public class SummaryGenerator
    {
        public string GenerateSummary(
            CVMaster cvMaster,
            JobRequirements requirements,
            List<Experience> selectedExperiences,
            List<string> topSkills)
        {
            var yearsOfExperience = CalculateYearsOfExperience(cvMaster.Experience);
            var primaryRole = DeterminePrimaryRole(selectedExperiences, requirements);

            var keyTechSkills = topSkills
                .Where(s => IsRelevantForSummary(s, requirements))
                .Take(5)
                .ToList();

            var parts = new List<string>();

            parts.Add($"{primaryRole} with {yearsOfExperience}+ years of experience");

            if (keyTechSkills.Any())
                parts.Add($"specializing in {FormatSkillsList(keyTechSkills)}");

            var achievement = ExtractTopAchievement(selectedExperiences);
            if (!string.IsNullOrEmpty(achievement))
                parts.Add($"Proven track record of {achievement}");

            var softSkills = ExtractSoftSkills(requirements, selectedExperiences);
            if (softSkills.Any())
                parts.Add($"Strong {string.Join(" and ", softSkills.Take(2))} skills");

            var summary = string.Join(". ", parts) + ".";

            if (summary.Length > 300)
                summary = summary.Substring(0, 297) + "...";

            return summary;
        }

        private string ExtractTopAchievement(List<Experience> experiences)
        {
            foreach (var exp in experiences)
            {
                foreach (var bullet in exp.Bullets ?? new List<string>())
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        bullet,
                        @"(improving|increasing|reducing|achieving|exceeding|delivering).*?(\d+%)",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    if (match.Success)
                        return match.Value.ToLower();
                }
            }

            return string.Empty;
        }

        private int CalculateYearsOfExperience(List<Experience> allExperiences)
        {
            var earliestYear = DateTime.Now.Year;

            foreach (var exp in allExperiences)
            {
                if (!string.IsNullOrEmpty(exp.StartDate))
                {
                    var parts = exp.StartDate.Split(' ');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int year))
                        earliestYear = Math.Min(earliestYear, year);
                }
            }

            return DateTime.Now.Year - earliestYear;
        }

        private string DeterminePrimaryRole(
            List<Experience> selectedExperiences,
            JobRequirements requirements)
        {
            var seniority = requirements.SeniorityLevel?.ToLower() ?? "";

            if (seniority.Contains("senior")) return "Senior Software Developer";
            if (seniority.Contains("lead")) return "Lead Software Engineer";
            if (seniority.Contains("junior")) return "Junior Software Developer";

            if (selectedExperiences.Any())
            {
                var role = selectedExperiences.First().Role;

                if (role.Contains("Developer", StringComparison.OrdinalIgnoreCase) ||
                    role.Contains("Engineer", StringComparison.OrdinalIgnoreCase))
                    return "Software Developer";

                if (role.Contains("Specialist", StringComparison.OrdinalIgnoreCase) ||
                    role.Contains("Coordinator", StringComparison.OrdinalIgnoreCase))
                    return "Operations Specialist";

                if (role.Contains("Operator", StringComparison.OrdinalIgnoreCase) ||
                    role.Contains("Supervisor", StringComparison.OrdinalIgnoreCase))
                    return "Production Professional";
            }

            return "Software Developer";
        }

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

        private List<string> ExtractAdditionalKeywords(
            JobRequirements requirements,
            List<string> alreadyIncluded)
        {
            var importantKeywords = new[]
            {
                "microservices", "cloud", "CI/CD", "DevOps",
                "automation", "scalability", "performance", "architecture"
            };

            return importantKeywords
                .Where(k =>
                    requirements.Keywords.Any(r => r.Contains(k, StringComparison.OrdinalIgnoreCase)) &&
                    !alreadyIncluded.Any(a => a.Contains(k, StringComparison.OrdinalIgnoreCase)))
                .Take(3)
                .ToList();
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
                { "mentor", "mentoring" }
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

                if (result.Count >= 2)
                    break;
            }

            return result;
        }
    }
}

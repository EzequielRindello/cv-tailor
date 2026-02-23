using ErCvApi.Models;
using ErCvApi.Constants;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class JobDescriptionParser
    {
        private readonly HashSet<string> _seniorityKeywords =
            new(JobDescriptionConstants.SeniorityKeywords, StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _commonTechKeywords =
            new(JobDescriptionConstants.CommonTechKeywords, StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _commonWords =
            new(JobDescriptionConstants.CommonWords, StringComparer.OrdinalIgnoreCase);

        public JobRequirements Parse(string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                return new JobRequirements
                {
                    TechnicalSkills = new List<string>(),
                    Keywords = new List<string>(),
                    SeniorityLevel = JobDescriptionConstants.DefaultSeniority,
                    RequiredExperience = new List<string>()
                };
            }

            var normalizedText = jobDescription.ToLower();

            return new JobRequirements
            {
                TechnicalSkills = ExtractTechnicalSkills(normalizedText),
                Keywords = ExtractKeywords(normalizedText),
                SeniorityLevel = ExtractSeniorityLevel(normalizedText),
                RequiredExperience = ExtractRequiredExperience(normalizedText)
            };
        }

        private List<string> ExtractTechnicalSkills(string text)
        {
            var foundSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tech in _commonTechKeywords)
            {
                var pattern = $@"\b{Regex.Escape(tech)}\b";
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                {
                    foundSkills.Add(tech);
                }
            }

            var matches = Regex.Matches(
                text,
                JobDescriptionConstants.YearsWithTechPattern,
                RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 2)
                {
                    var tech = match.Groups[2].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(tech) && tech.Length > 1)
                    {
                        foundSkills.Add(tech);
                    }
                }
            }

            return foundSkills.ToList();
        }

        private List<string> ExtractKeywords(string text)
        {
            var words = Regex.Split(text, JobDescriptionConstants.WordSplitPattern)
                .Where(w => w.Length > 3 && !_commonWords.Contains(w))
                .GroupBy(w => w.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(30)
                .Select(g => g.Key)
                .ToList();

            return words;
        }

        private string ExtractSeniorityLevel(string text)
        {
            foreach (var keyword in _seniorityKeywords)
            {
                if (Regex.IsMatch(text, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                {
                    return keyword.ToLower();
                }
            }

            var yearsMatch = Regex.Match(
                text,
                JobDescriptionConstants.YearsOfExperiencePattern);

            if (yearsMatch.Success &&
                int.TryParse(yearsMatch.Groups[1].Value, out int years))
            {
                if (years >= 7) return "senior";
                if (years >= 4) return JobDescriptionConstants.DefaultSeniority;
                if (years >= 1) return "junior";
            }

            return JobDescriptionConstants.DefaultSeniority;
        }

        private List<string> ExtractRequiredExperience(string text)
        {
            var requirements = new List<string>();

            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (Regex.IsMatch(trimmed, JobDescriptionConstants.BulletLinePattern) ||
                    trimmed.StartsWith(JobDescriptionConstants.ExperienceKeyword, StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains(JobDescriptionConstants.RequiredKeyword, StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains(JobDescriptionConstants.MustHaveKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    var cleaned = Regex.Replace(trimmed, JobDescriptionConstants.BulletLinePattern, "").Trim();

                    if (cleaned.Length > 10 && cleaned.Length < 200)
                    {
                        requirements.Add(cleaned);
                    }
                }
            }

            return requirements.Take(10).ToList();
        }
    }
}
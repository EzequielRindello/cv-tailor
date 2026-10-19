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

        private static readonly HashSet<string> _neverSkills = new(StringComparer.OrdinalIgnoreCase)
        {
            "experience","familiarity","understanding","knowledge","proficiency",
            "ability","background","exposure","awareness","skill","skills",
            "strong","solid","deep","good","basic","advanced","proven",
            "excellent","nice","required","preferred","plus","bonus",
            "including","following","similar","relevant","related",
            "technology","technologies","tools","tool","stack","platform",
            "systems","system","solutions","services","applications",
            "backend","frontend","fullstack","full","stack",
            "engineer","engineering","developer","development","software",
            "technical","senior","junior","lead","principal","manager",
            "manage","monitor","cost","write","build","design","implement",
            "collaborate","work","team","teams","environment","practices",
            "responsibilities","requirements","qualifications","about",
            "company","role","position","candidate","ideal","looking",
            "seeking","hire","join","opportunity","what","will",
            "have","with","that","this","from","your",
            "library","libraries","framework","frameworks","methodology",
        };

        public JobRequirements Parse(string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(jobDescription))
                return new JobRequirements
                {
                    TechnicalSkills = new List<string>(),
                    Keywords = new List<string>(),
                    SeniorityLevel = JobDescriptionConstants.DefaultSeniority,
                    RequiredExperience = new List<string>()
                };

            var normalizedText = jobDescription.ToLower();

            return new JobRequirements
            {
                TechnicalSkills = ExtractTechnicalSkills(jobDescription, normalizedText),
                Keywords = ExtractKeywords(normalizedText),
                SeniorityLevel = ExtractSeniorityLevel(normalizedText),
                RequiredExperience = ExtractRequiredExperience(normalizedText)
            };
        }

        private List<string> ExtractTechnicalSkills(string originalText, string normalizedText)
        {
            var foundSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tech in _commonTechKeywords)
            {
                var pattern = $@"(?<![a-zA-Z]){Regex.Escape(tech)}(?![a-zA-Z])";
                if (Regex.IsMatch(normalizedText, pattern, RegexOptions.IgnoreCase))
                    foundSkills.Add(tech);
            }
            var yearsMatches = Regex.Matches(originalText, JobDescriptionConstants.YearsWithTechPattern, RegexOptions.IgnoreCase);
            foreach (Match match in yearsMatches)
            {
                if (match.Groups.Count > 2)
                {
                    var tech = match.Groups[2].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(tech) && tech.Length > 1 && tech.Length < 40
                        && !_neverSkills.Contains(tech) && !_commonWords.Contains(tech))
                        foundSkills.Add(tech);
                }
            }
            return foundSkills
                .Select(s => SkillNormalizer.Normalize(s))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<string> ExtractKeywords(string text) =>
            Regex.Split(text, JobDescriptionConstants.WordSplitPattern)
                .Where(w => w.Length > 3 && !_commonWords.Contains(w) && !_neverSkills.Contains(w))
                .GroupBy(w => w.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(30)
                .Select(g => g.Key)
                .ToList();

        private string ExtractSeniorityLevel(string text)
        {
            foreach (var keyword in _seniorityKeywords)
            {
                var pattern = $@"\b{Regex.Escape(keyword)}\b";
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    var start = Math.Max(0, match.Index - 60);
                    var context = text.Substring(start, match.Index - start).ToLower();
                    var negations = new[] { "no ", "not ", "without ", "don't ", "dont ", "non-", "doesn't require" };
                    if (!negations.Any(n => context.Contains(n)))
                        return keyword.ToLower();
                }
            }

            var yearsMatch = Regex.Match(text, JobDescriptionConstants.YearsOfExperiencePattern);
            if (yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out int years))
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
                        requirements.Add(cleaned);
                }
            }
            return requirements.Take(10).ToList();
        }
    }
}

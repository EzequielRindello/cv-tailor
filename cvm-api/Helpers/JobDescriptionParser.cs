using ErCvApi.Models;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class JobDescriptionParser
    {
        private readonly List<string> _seniorityKeywords = new()
        {
            "senior", "lead", "principal", "staff", "architect",
            "junior", "mid-level", "entry-level", "intern"
        };

        private readonly List<string> _commonTechKeywords = new()
        {
            "c#", "csharp", ".net", "asp.net", "dotnet", "react", "angular", "vue",
            "javascript", "typescript", "python", "java", "node.js", "sql", "nosql",
            "aws", "azure", "gcp", "docker", "kubernetes", "microservices",
            "api", "rest", "graphql", "mongodb", "postgresql", "mysql", "redis",
            "ci/cd", "devops", "agile", "scrum", "git", "jenkins", "terraform"
        };

        public JobRequirements Parse(string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                return new JobRequirements
                {
                    TechnicalSkills = new List<string>(),
                    Keywords = new List<string>(),
                    SeniorityLevel = "mid-level",
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

            // Extract years of experience with technologies
            var experiencePattern = @"(\d+)\+?\s*(?:years?|yrs?)\s*(?:of\s*)?(?:experience\s*)?(?:with\s*)?([a-z0-9\.\#\+\-]+)";
            var matches = Regex.Matches(text, experiencePattern, RegexOptions.IgnoreCase);
            
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
            var keywords = new List<string>();
            
            // Split by common separators and extract meaningful words
            var words = Regex.Split(text, @"\W+")
                .Where(w => w.Length > 3 && !IsCommonWord(w))
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

            // Check for years of experience
            var yearsMatch = Regex.Match(text, @"(\d+)\+?\s*(?:years?|yrs?)");
            if (yearsMatch.Success && int.TryParse(yearsMatch.Groups[1].Value, out int years))
            {
                if (years >= 7) return "senior";
                if (years >= 4) return "mid-level";
                if (years >= 1) return "junior";
            }

            return "mid-level";
        }

        private List<string> ExtractRequiredExperience(string text)
        {
            var requirements = new List<string>();
            
            // Look for bullet points or requirements sections
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                
                // Check if line looks like a requirement
                if (Regex.IsMatch(trimmed, @"^[\-\*\•\d\.]+\s*") || 
                    trimmed.StartsWith("experience", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("must have", StringComparison.OrdinalIgnoreCase))
                {
                    var cleaned = Regex.Replace(trimmed, @"^[\-\*\•\d\.]+\s*", "").Trim();
                    if (cleaned.Length > 10 && cleaned.Length < 200)
                    {
                        requirements.Add(cleaned);
                    }
                }
            }

            return requirements.Take(10).ToList();
        }

        private bool IsCommonWord(string word)
        {
            var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "and", "for", "with", "you", "that", "this", "will", "from",
                "have", "has", "are", "our", "your", "their", "been", "more", "than",
                "such", "about", "into", "through", "should", "would", "could", "must"
            };

            return commonWords.Contains(word);
        }
    }
}

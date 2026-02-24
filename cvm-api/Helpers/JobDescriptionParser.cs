using ErCvApi.Models;
using ErCvApi.Constants;
using System.Text.RegularExpressions;

namespace ErCvApi.Helpers
{
    public class JobDescriptionParser
    {
        private readonly HashSet<string> _seniorityKeywords =
            new(JobDescriptionConstants.SeniorityKeywords, StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _techKeywords =
            new(JobDescriptionConstants.CommonTechKeywords, StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _commonWords =
            new(JobDescriptionConstants.CommonWords, StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _negationWords =
            new(JobDescriptionConstants.NegationWords, StringComparer.OrdinalIgnoreCase);

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

            return new JobRequirements
            {
                TechnicalSkills = ExtractTechnicalSkills(jobDescription),
                Keywords = ExtractKeywords(jobDescription),
                SeniorityLevel = ExtractSeniorityLevel(jobDescription),
                RequiredExperience = ExtractRequiredExperience(jobDescription)
            };
        }

        // ─── Technical Skills ────────────────────────────────────────────────
        // Strategy: match against known list, then also extract unknown
        // capitalized/versioned tokens that look like tech (e.g. "Prisma", "tRPC")
        private List<string> ExtractTechnicalSkills(string text)
        {
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var textLower = text.ToLower();

            // 1. Match against the full known list
            foreach (var tech in _techKeywords)
            {
                // Build pattern: word boundary, but support dots and # in the middle
                var escaped = Regex.Escape(tech);
                // For multi-word techs like "asp.net core", simple contains check
                if (tech.Contains(' '))
                {
                    if (textLower.Contains(tech.ToLower()))
                        found.Add(NormalizeTechName(tech));
                }
                else
                {
                    var pattern = $@"(?<![a-z0-9\.\#\+]){escaped}(?![a-z0-9\.\#\+])";
                    if (Regex.IsMatch(textLower, pattern, RegexOptions.IgnoreCase))
                        found.Add(NormalizeTechName(tech));
                }
            }

            // 2. Extract "X years of/with Y" patterns to catch unlisted tech
            var yearsMatches = Regex.Matches(
                text,
                JobDescriptionConstants.YearsWithTechPattern,
                RegexOptions.IgnoreCase);

            foreach (Match match in yearsMatches)
            {
                if (match.Groups.Count > 2)
                {
                    var tech = match.Groups[2].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(tech) && tech.Length > 1 && !_commonWords.Contains(tech))
                        found.Add(NormalizeTechName(tech));
                }
            }

            // 3. Extract capitalized tokens that look like tech names
            // Pattern: standalone words of 2-20 chars starting with uppercase
            // OR all-uppercase acronyms of 2-8 chars
            // This catches things like "Prisma", "tRPC", "NestJS", "Supabase" etc.
            var tokenMatches = Regex.Matches(text, @"\b([A-Z][a-zA-Z0-9\.\#\+\-]{1,19}|[A-Z]{2,8})\b");
            foreach (Match match in tokenMatches)
            {
                var token = match.Value;
                // Skip if it looks like a regular sentence word or common stopword
                if (_commonWords.Contains(token.ToLower())) continue;
                if (token.Length < 2) continue;
                // Skip all-uppercase words that are not acronyms (too generic like "MUST", "WILL")
                if (token == token.ToUpper() && token.Length > 6) continue;
                // If it's already in found (case-insensitive), skip
                if (found.Any(f => f.Equals(token, StringComparison.OrdinalIgnoreCase))) continue;
                // Only add if it appears in a tech context (near tech section keywords)
                if (IsInTechContext(text, match.Index))
                    found.Add(token);
            }

            return found
                .OrderBy(s => s)
                .ToList();
        }

        // Check if a match position is near tech-indicating words
        private bool IsInTechContext(string text, int position)
        {
            var contextStart = Math.Max(0, position - 80);
            var contextEnd = Math.Min(text.Length, position + 80);
            var context = text.Substring(contextStart, contextEnd - contextStart).ToLower();

            var techContextWords = new[]
            {
                "experience", "proficient", "knowledge", "familiar", "skill",
                "using", "with", "stack", "tool", "technology", "framework",
                "library", "platform", "language", "database", "cloud",
                "work", "build", "develop", "design", "implement", "deploy"
            };

            return techContextWords.Any(w => context.Contains(w));
        }

        // Normalize common aliases to canonical names
        private string NormalizeTechName(string tech)
        {
            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "csharp", "C#" },
                { "c sharp", "C#" },
                { "dotnet", ".NET" },
                { "dot net", ".NET" },
                { "reactjs", "React" },
                { "react.js", "React" },
                { "nodejs", "Node.js" },
                { "node.js", "Node.js" },
                { "vuejs", "Vue" },
                { "vue.js", "Vue" },
                { "nextjs", "Next.js" },
                { "next.js", "Next.js" },
                { "nestjs", "NestJS" },
                { "nest.js", "NestJS" },
                { "angularjs", "Angular" },
                { "asp.net", "ASP.NET Core" },
                { "asp.net core", "ASP.NET Core" },
                { "postgres", "PostgreSQL" },
                { "mongo", "MongoDB" },
                { "k8s", "Kubernetes" },
                { "ts", "TypeScript" },
                { "js", "JavaScript" },
                { "golang", "Go" },
                { "sklearn", "scikit-learn" },
                { "ml", "Machine Learning" },
            };

            return aliases.TryGetValue(tech, out var canonical) ? canonical : tech;
        }

        // ─── Keywords ────────────────────────────────────────────────────────
        private List<string> ExtractKeywords(string text)
        {
            return Regex.Split(text.ToLower(), JobDescriptionConstants.WordSplitPattern)
                .Where(w => w.Length > 3 && !_commonWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(30)
                .Select(g => g.Key)
                .ToList();
        }

        // ─── Seniority ───────────────────────────────────────────────────────
        // Context-aware: ignores seniority keywords preceded by negation words
        private string ExtractSeniorityLevel(string text)
        {
            var textLower = text.ToLower();

            // Check each seniority keyword in priority order
            foreach (var keyword in JobDescriptionConstants.SeniorityKeywords)
            {
                var keywordLower = keyword.ToLower();
                var index = textLower.IndexOf(keywordLower, StringComparison.OrdinalIgnoreCase);

                while (index >= 0)
                {
                    // Check if this occurrence is preceded by a negation
                    if (!IsPrecededByNegation(textLower, index))
                        return NormalizeSeniority(keywordLower);

                    // Look for next occurrence
                    index = textLower.IndexOf(keywordLower, index + 1, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Fallback: infer from years of experience
            var yearsMatch = Regex.Match(
                textLower,
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

        private bool IsPrecededByNegation(string text, int keywordIndex)
        {
            // Look at the 50 chars before the keyword
            var start = Math.Max(0, keywordIndex - 50);
            var preceding = text.Substring(start, keywordIndex - start);

            var words = Regex.Split(preceding.Trim(), @"\s+");
            // Check last 4 words before keyword
            return words.TakeLast(4).Any(w => _negationWords.Contains(w));
        }

        private string NormalizeSeniority(string keyword)
        {
            if (keyword.StartsWith("sr")) return "senior";
            if (keyword.StartsWith("jr")) return "junior";
            if (keyword == "entry" || keyword == "entry level" || keyword == "entry-level") return "junior";
            if (keyword == "associate") return "junior";
            if (keyword == "intern" || keyword == "graduate" || keyword == "new grad") return "intern";
            if (keyword == "mid level" || keyword == "intermediate") return "mid-level";
            if (keyword == "tech lead" || keyword == "team lead") return "lead";
            if (keyword == "staff engineer") return "staff";
            return keyword;
        }

        // ─── Required Experience ─────────────────────────────────────────────
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

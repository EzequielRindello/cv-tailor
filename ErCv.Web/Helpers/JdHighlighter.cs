using ErCvApi.Models;

namespace ErCvApi.Helpers
{
    /// <summary>
    /// Marks where the skills the parser found appear in the job description, and where each one shows up in the
    /// generated CV (MATCH-05). Only splits text and reads the CV; it never changes either.
    /// </summary>
    public class JdHighlighter
    {
        private const int MaxExcerptLength = 140;

        // Short or everyday words that are also aliases of a skill ("next steps", "RN" for nurse) would light up
        // ordinary prose. These aliases are not highlighted; the canonical skill name always is.
        private static readonly HashSet<string> AmbiguousAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            "next", "nest", "elastic", "kube", "ml", "ef", "tf", "rn", "py"
        };

        public JdHighlight Build(
            string jobDescription, JobRequirements requirements, CVMaster profile, GeneratedCV generatedCv)
        {
            var result = new JdHighlight();
            if (string.IsNullOrEmpty(jobDescription)) return result;

            var spans = FindSpans(jobDescription, requirements.TechnicalSkills);

            var cursor = 0;
            foreach (var span in spans)
            {
                if (span.Start > cursor)
                    result.Segments.Add(new JdSegment { Text = jobDescription[cursor..span.Start] });

                result.Segments.Add(new JdSegment
                {
                    Text = jobDescription.Substring(span.Start, span.Length),
                    Skill = span.Skill
                });
                cursor = span.Start + span.Length;
            }
            if (cursor < jobDescription.Length)
                result.Segments.Add(new JdSegment { Text = jobDescription[cursor..] });

            foreach (var skill in spans.Select(s => s.Skill).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                result.Skills.Add(new JdSkillInfo
                {
                    Skill = skill,
                    InProfile = MissingSkillsAnalyzer.IsInProfile(profile, skill),
                    Locations = FindLocations(generatedCv, skill)
                });
            }

            return result;
        }

        // Longest match wins, so "react native" is one highlight and not "react" plus a leftover.
        private static List<(int Start, int Length, string Skill)> FindSpans(string text, IEnumerable<string> skills)
        {
            var candidates = skills
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(skill => SkillNormalizer.FindSkill(text, skill)
                    .Where(m => IsReliable(text.Substring(m.Start, m.Length), skill))
                    .Select(m => (m.Start, m.Length, Skill: skill)))
                .OrderBy(c => c.Start)
                .ThenByDescending(c => c.Length)
                .ToList();

            var spans = new List<(int Start, int Length, string Skill)>();
            var end = 0;
            foreach (var c in candidates)
            {
                if (c.Start < end) continue;
                spans.Add(c);
                end = c.Start + c.Length;
            }
            return spans;
        }

        private static bool IsReliable(string matched, string skill) =>
            matched.Equals(SkillNormalizer.Normalize(skill), StringComparison.OrdinalIgnoreCase) ||
            matched.Equals(skill, StringComparison.OrdinalIgnoreCase) ||
            !AmbiguousAliases.Contains(matched);

        private static List<JdSkillLocation> FindLocations(GeneratedCV cv, string skill)
        {
            var found = new List<JdSkillLocation>();

            if (cv.Skills.Any(s => SkillNormalizer.FuzzyMatch(s, skill)))
                found.Add(new JdSkillLocation { Section = "Skills", Label = "Skills list" });

            foreach (var exp in cv.Experience)
            {
                var label = $"{exp.Role}, {exp.Company}";

                foreach (var bullet in exp.Bullets.Where(b => SkillNormalizer.ContainsSkill(b, skill)))
                    found.Add(new JdSkillLocation { Section = "Experience", Label = label, Excerpt = Excerpt(bullet) });

                if (exp.Skills.Any(s => SkillNormalizer.FuzzyMatch(s, skill)))
                    found.Add(new JdSkillLocation { Section = "Experience", Label = label, Excerpt = "Listed in this role's skills" });
            }

            foreach (var project in cv.Projects)
            {
                var inText = SkillNormalizer.ContainsSkill(project.Description, skill) ||
                             project.Highlights.Any(h => SkillNormalizer.ContainsSkill(h, skill));
                var inTech = project.Technologies.Any(t => SkillNormalizer.FuzzyMatch(t, skill));
                if (inText || inTech)
                    found.Add(new JdSkillLocation { Section = "Projects", Label = project.Name });
            }

            if (SkillNormalizer.ContainsSkill(cv.Summary, skill))
                found.Add(new JdSkillLocation { Section = "Summary", Label = "Summary" });

            return found;
        }

        private static string Excerpt(string text) =>
            text.Length <= MaxExcerptLength ? text : text[..MaxExcerptLength].TrimEnd() + "...";
    }
}

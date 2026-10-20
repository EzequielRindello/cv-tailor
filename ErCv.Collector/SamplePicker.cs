using ErCvApi.Helpers;

namespace ErCv.Collector
{
    /// <summary>
    /// Picks the APP-03 samples from the corpus: one post per role family, the one the parser gets the most skills from
    /// (newest first on a tie), preferring posts short enough to read in the composer. "Junior" is the entry-level pick from any family.
    /// </summary>
    public static class SamplePicker
    {
        private const int ReadableLength = 8000;

        public static readonly (string Id, string Label, Func<Posting, bool> Fits)[] Slots =
        {
            ("backend", "Backend", p => p.RoleFamily == "backend" && !IsEntry(p)),
            ("frontend", "Frontend", p => p.RoleFamily == "frontend" && !IsEntry(p)),
            ("fullstack", "Full stack", p => p.RoleFamily == "fullstack" && !IsEntry(p)),
            ("data", "Data", p => p.RoleFamily == "data" && !IsEntry(p)),
            ("devops", "DevOps", p => p.RoleFamily == "devops" && !IsEntry(p)),
            ("junior", "Junior", p => p.Seniority == "junior"),
        };

        public static List<SampleRecord> Pick(IEnumerable<Posting> postings, out List<string> missing)
        {
            var parser = new JobDescriptionParser();
            var scored = postings
                .Select(p => (Posting: p, Skills: parser.Parse(p.JobDescription).TechnicalSkills.Count))
                .ToList();
            var used = new HashSet<string>(StringComparer.Ordinal);
            var samples = new List<SampleRecord>();
            missing = new List<string>();

            foreach (var (id, label, fits) in Slots)
            {
                var best = scored
                    .Where(s => fits(s.Posting) && !used.Contains(s.Posting.Id))
                    .OrderBy(s => s.Posting.Text.Length > ReadableLength ? 1 : 0)
                    .ThenByDescending(s => s.Skills)
                    .ThenByDescending(s => s.Posting.PostedAt)
                    .ThenBy(s => s.Posting.Id, StringComparer.Ordinal)
                    .Select(s => s.Posting)
                    .FirstOrDefault();
                if (best == null) { missing.Add(label); continue; }

                used.Add(best.Id);
                samples.Add(new SampleRecord
                {
                    Id = id,
                    Label = label,
                    PostingId = best.Id,
                    Source = JobSources.Find(best.Source)?.DisplayName ?? best.Source,
                    Url = best.Url,
                    Company = best.Company,
                    Title = best.Title,
                    Location = best.Location,
                    PostedAt = best.PostedAt,
                    Text = best.JobDescription
                });
            }
            return samples;
        }

        private static bool IsEntry(Posting p) => p.Seniority is "junior" or "intern";
    }
}

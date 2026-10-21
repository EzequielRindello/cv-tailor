using ErCv.Web.Features.Matching;

namespace ErCv.Collector;

/// <summary>
/// Picks the APP-03 samples from the corpus: one post per role family, the one the parser gets the most skills from
/// (newest first on a tie), preferring posts whose title names a clear level and that are short enough to read in the
/// composer. Programs (accelerators, fellowships) are never picked. "Junior" is the entry-level pick from any family.
/// </summary>
public static class SamplePicker
{
    #region Constants

    private const int ReadableLength = 8000;

    // A program is not a normal job to tailor a CV to (APP-06: an accelerator post was the Full stack sample)
    private static readonly string[] ProgramWords = { "accelerator", "fellowship", "residency", "apprenticeship", "bootcamp" };

    public static readonly (string Id, string Label, Func<Posting, bool> Fits)[] Slots =
    {
        ("backend", "Backend", p => p.RoleFamily == "backend" && !IsEntry(p)),
        ("frontend", "Frontend", p => p.RoleFamily == "frontend" && !IsEntry(p)),
        ("fullstack", "Full stack", p => p.RoleFamily == "fullstack" && !IsEntry(p)),
        ("data", "Data", p => p.RoleFamily == "data" && !IsEntry(p)),
        ("devops", "DevOps", p => p.RoleFamily == "devops" && !IsEntry(p)),
        ("junior", "Junior", p => p.Seniority == "junior"),
    };

    #endregion

    #region Public Methods

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
                .Where(s => fits(s.Posting) && !used.Contains(s.Posting.Id) && !IsProgram(s.Posting))
                .OrderBy(s => Classifier.NamesClearLevel(s.Posting.Title) ? 0 : 1)
                .ThenBy(s => s.Posting.Text.Length > ReadableLength ? 1 : 0)
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

    #endregion

    #region Private Methods

    private static bool IsEntry(Posting p) => p.Seniority is "junior" or "intern";

    private static bool IsProgram(Posting p) =>
        ProgramWords.Any(w => p.Title.Contains(w, StringComparison.OrdinalIgnoreCase));

    #endregion
}

using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.Matching;
using ErCv.Web.Models;

namespace ErCv.Collector;

/// <summary>
/// Picks the APP-03 samples from the corpus: one post per role family. It prefers posts at the level of the profile
/// the demo starts with (then any other level the title names), short enough to read in the composer and with enough
/// skills to show, and among those the one that profile matches best, then the one the parser gets the most skills
/// from, newest first on a tie.
/// Programs (accelerators, fellowships) are never picked. "Junior" is the entry-level pick from any family.
/// </summary>
public static class SamplePicker
{
    #region Constants

    private const int ReadableLength = 8000;

    // The level of the profile step 2 starts with (the senior sample, Data/cv-master-senior.json). F27: most samples
    // were Staff posts, so trying the demo gave a seniority mismatch and a weak match on 5 of 6 samples.
    private const string DemoProfileLevel = "senior";

    // A sample shows the parser at work (skills to match, to miss and to highlight). Without this floor the best match
    // was a post the parser found one skill in, a trivial 1 of 1.
    private const int MinSampleSkills = 6;

    private static readonly MatchScoreCalculator MatchScores = new();

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

    /// <param name="demoProfile">The profile the demo starts with; without it the match does not rank the posts.</param>
    public static List<SampleRecord> Pick(IEnumerable<Posting> postings, CVMaster? demoProfile, out List<string> missing)
    {
        var parser = new JobDescriptionParser();
        var scored = postings
            .Select(p => (Posting: p, Requirements: parser.Parse(p.JobDescription)))
            .Select(s => (
                s.Posting,
                Skills: s.Requirements.TechnicalSkills.Count,
                Match: demoProfile == null ? 0 : MatchScores.Calculate(demoProfile, s.Requirements).Overall))
            .ToList();
        var used = new HashSet<string>(StringComparer.Ordinal);
        var samples = new List<SampleRecord>();
        missing = new List<string>();

        foreach (var (id, label, fits) in Slots)
        {
            var best = scored
                .Where(s => fits(s.Posting) && !used.Contains(s.Posting.Id) && !IsProgram(s.Posting))
                .OrderBy(s => LevelPreference(s.Posting))
                .ThenBy(s => s.Posting.Text.Length > ReadableLength ? 1 : 0)
                .ThenBy(s => s.Skills >= MinSampleSkills ? 0 : 1)
                .ThenByDescending(s => s.Match)
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

    // The demo profile's level first, then any other level the title names, then titles that name none
    private static int LevelPreference(Posting p) =>
        p.Seniority == DemoProfileLevel ? 0 : Classifier.NamesClearLevel(p.Title) ? 1 : 2;

    private static bool IsProgram(Posting p) =>
        ProgramWords.Any(w => p.Title.Contains(w, StringComparison.OrdinalIgnoreCase));

    #endregion
}

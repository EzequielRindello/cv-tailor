using ErCv.Collector;
using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Selection;

namespace ErCv.Tests.Collector;

/// <summary>APP-05: the committed corpus, the samples taken from it and the parser running over real posts.</summary>
[Trait("Category", "Unit")]
public class CorpusTests
{
    private static readonly string[] SampleLabels = { "Backend", "Frontend", "Full stack", "Data", "DevOps", "Junior" };

    public static TheoryData<string> PostingIds => FixtureLoader.CorpusPostingIds();

    private static List<SampleRecord> CommittedSamples() =>
        SampleJobDescriptions.All.Select(s => new SampleRecord
        {
            Id = s.Id, Label = s.Label, PostingId = s.PostingId, Source = s.Source, Url = s.Url, Company = s.Company,
            Title = s.Title, Location = s.Location, PostedAt = s.PostedAt, Text = s.Text
        }).ToList();

    [Fact]
    public void Committed_corpus_follows_every_rule_as_of_its_last_refresh()
    {
        // Measured from manifest.refreshedAt, not today, so the build does not break while nobody refreshes
        Assert.Empty(CorpusCheck.Problems(FixtureLoader.Corpus, CommittedSamples()));
    }

    [Fact]
    public void No_posting_is_older_than_the_window()
    {
        var manifest = FixtureLoader.Corpus.LoadManifest()!;
        var cutoff = manifest.RefreshedAt.AddDays(-FixtureLoader.Corpus.LoadConfig().WindowDays);

        Assert.NotEmpty(FixtureLoader.CorpusPostings);
        Assert.All(FixtureLoader.CorpusPostings, p => Assert.True(p.PostedAt >= cutoff, $"{p.Id} posted {p.PostedAt:yyyy-MM-dd}"));
    }

    [Fact]
    public void Every_posting_has_a_date_and_a_source_link()
    {
        Assert.All(FixtureLoader.CorpusPostings, p =>
        {
            Assert.NotEqual(default, p.PostedAt);
            Assert.NotEqual(default, p.RetrievedAt);
            Assert.StartsWith("https://", p.Url);
        });
    }

    [Fact]
    public void Samples_are_one_per_role_family_and_each_is_a_dated_post_from_the_corpus()
    {
        var samples = SampleJobDescriptions.All;
        var byId = FixtureLoader.CorpusPostings.ToDictionary(p => p.Id);

        Assert.Equal(SampleLabels, samples.Select(s => s.Label));
        Assert.All(samples, s =>
        {
            Assert.True(byId.ContainsKey(s.PostingId), $"{s.Label}: {s.PostingId} not in the corpus");
            Assert.StartsWith("https://", s.Url);
            Assert.NotEqual(default, s.PostedAt);
            Assert.False(string.IsNullOrWhiteSpace(s.Company));
            Assert.Equal(byId[s.PostingId].JobDescription, s.Text);
        });
    }

    // APP-06: no program (accelerator, fellowship) and a title that names its level
    [Fact]
    public void Samples_are_normal_jobs_with_a_clear_level_in_the_title()
    {
        Assert.All(SampleJobDescriptions.All, s =>
        {
            Assert.DoesNotContain("accelerator", s.Title, StringComparison.OrdinalIgnoreCase);
            Assert.True(Classifier.NamesClearLevel(s.Title), s.Title);
        });
    }

    // F18 on the committed corpus: Next.js only where a post names it
    [Fact]
    public void Next_js_is_found_only_in_posts_that_name_it()
    {
        var parser = new JobDescriptionParser();

        Assert.All(FixtureLoader.CorpusPostings.Where(p => parser.Parse(p.JobDescription).TechnicalSkills.Contains("next.js")),
            p => Assert.True(p.JobDescription.Contains("next.js", StringComparison.OrdinalIgnoreCase)
                || p.JobDescription.Contains("nextjs", StringComparison.OrdinalIgnoreCase), p.Id));
    }

    // MATCH-09 acceptance: the level the parser reads agrees with the title level on more than 85% of the posts
    [Fact]
    public void Parser_agrees_with_the_title_level_on_most_of_the_corpus()
    {
        var parser = new JobDescriptionParser();
        var agree = FixtureLoader.CorpusPostings.Count(p => RelevanceReport.ParserLevel(parser.Parse(p.JobDescription).SeniorityLevel) == p.Seniority);

        Assert.True(agree > FixtureLoader.CorpusPostings.Count * 0.85, $"{agree} of {FixtureLoader.CorpusPostings.Count}");
    }

    // EDIT-09: a company read from a post is the post's own company, or nothing
    [Fact]
    public void The_company_read_from_a_real_post_is_its_company_or_nothing()
    {
        var parser = new JobDescriptionParser();
        var found = 0;

        foreach (var post in FixtureLoader.CorpusPostings)
        {
            var company = parser.Parse(post.JobDescription).Company;
            if (company == null) continue;

            found++;
            Assert.True(string.Equals(company, post.Company, StringComparison.OrdinalIgnoreCase), $"{post.Id}: read '{company}', the post is {post.Company}'s");
        }

        Assert.True(found > 0, "no post names its company in a heading");
    }

    // MATCH-10: the summary's role from the real posts. A mobile post reads Mobile; one stray mobile word does not
    private static string SummaryRole(Posting post)
    {
        var cv = ErCv.Tests.Support.Build.Master(new[] { ErCv.Tests.Support.Build.Exp(role: "Developer") });
        var requirements = new JobDescriptionParser().Parse(post.JobDescription);
        return new SummaryGenerator().GenerateSummary(cv, requirements, cv.Experience, new List<string>());
    }

    [Fact]
    public void Mobile_posts_read_as_mobile()
    {
        var mobile = FixtureLoader.CorpusPostings.Where(p => p.RoleFamily == "mobile").ToList();

        Assert.NotEmpty(mobile);
        Assert.All(mobile, p => Assert.StartsWith("Mobile Developer", SummaryRole(p)));
    }

    // F29: "a backend language such as Java, Kotlin" is not a mobile post. "backend" is repeated in the post (MATCH-11)
    // and "distributed systems" is a signal too, so it reads as the backend role it is
    [Fact]
    public void A_post_that_names_kotlin_once_reads_as_backend()
    {
        var post = FixtureLoader.CorpusPostings.SingleOrDefault(p => p.Id == "greenhouse-airbnb-8274152");
        if (post == null) return; // a corpus refresh can drop the post

        Assert.StartsWith("Backend Developer", SummaryRole(post));
    }

    [Fact]
    public void Samples_are_what_the_picker_chooses_from_the_committed_corpus()
    {
        var samplesPath = Path.Combine(FixtureLoader.RepoRoot(), "ErCv.Web", "Data", "sample-jobs.json");
        var picked = SamplePicker.Pick(FixtureLoader.CorpusPostings, Cli.LoadDemoProfile(samplesPath), out var missing);

        Assert.Empty(missing);
        Assert.Equal(picked.Select(s => s.PostingId), SampleJobDescriptions.All.Select(s => s.PostingId));
    }

    [Fact]
    public void Every_corpus_jd_fixture_is_a_real_post()
    {
        Assert.NotEmpty(FixtureLoader.CorpusJdNames);
        Assert.All(FixtureLoader.CorpusJdNames, n => Assert.False(string.IsNullOrWhiteSpace(FixtureLoader.LoadJd(n))));
    }

    [Fact]
    public void Post_text_has_no_em_dashes()
    {
        Assert.All(FixtureLoader.CorpusPostings, p => Assert.DoesNotContain('\u2014', p.Title + p.Text));
    }

    [Theory]
    [MemberData(nameof(PostingIds))]
    public void Parser_reads_a_real_post_without_junk(string id)
    {
        var post = FixtureLoader.CorpusPostings.Single(p => p.Id == id);

        var r = new JobDescriptionParser().Parse(post.JobDescription);

        Assert.Contains(r.SeniorityLevel, JobDescriptionConstants.SeniorityLevels.Select(l => l.Level));
        Assert.All(r.TechnicalSkills, s => Assert.Equal(s, SkillNormalizer.Normalize(s)));
        Assert.All(r.TechnicalSkills, s => Assert.NotNull(SkillDictionary.Find(s)));
        Assert.DoesNotContain(r.TechnicalSkills, s => s is "in" or "or" or "of" or "and" or "the" or "experience");
        Assert.Equal(r.TechnicalSkills.Count, r.TechnicalSkills.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.InRange(r.Keywords.Count, 1, 30);
        Assert.InRange(r.RequiredExperience.Count, 0, 10);
    }

    [Fact]
    public void Relevance_report_runs_on_the_committed_corpus()
    {
        var report = RelevanceReport.Build(FixtureLoader.CorpusPostings, FixtureLoader.Corpus.LoadConfig(), DateTimeOffset.UtcNow);

        foreach (var heading in new[] { "## Skill frequency over time", "## Skills the parser misses", "## Skills the parser gets wrong", "## Dictionary skills nobody asks for", "## Seniority wording" })
            Assert.Contains(heading, report);
        Assert.DoesNotContain('\u2014', report);
    }
}

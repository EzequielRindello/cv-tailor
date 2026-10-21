using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Selection;

[Trait("Category", "Unit")]
public class CVSelectorTests
{
    private readonly CVSelector _selector = new();
    private const string LongBullet = "Built a distributed service handling 2M requests per day with high availability";

    private static Experience Role(string role, string end, params string[] bullets) =>
        Exp(role: role, end: end, bullets: bullets, skills: new[] { "go" });

    [Theory]
    [InlineData(false, 3, 3)]
    [InlineData(true, 4, 4)]
    public void Limits_experiences_and_bullets_per_mode(bool twoPages, int maxExp, int maxBullets)
    {
        var bullets = Enumerable.Range(1, 8).Select(i => $"{LongBullet} number {i}").ToArray();
        var exps = Enumerable.Range(0, 6).Select(i => Role("Backend Engineer", "Present", bullets)).ToArray();
        var cv = Master(exps, languages: new[] { "go" });

        var result = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" }), twoPages);

        Assert.True(result.Experience.Count <= maxExp);
        Assert.All(result.Experience, e => Assert.True(e.Bullets.Count <= maxBullets));
    }

    [Fact]
    public void Tech_jd_prefers_tech_roles_over_non_tech()
    {
        var tech = Exp(role: "Backend Engineer", end: Year(6), skills: new[] { "go" },
            bullets: new[] { LongBullet });
        var sales = Exp(role: "Sales Rep", end: "Present", bullets: new[] { LongBullet });
        var cv = Master(new[] { sales, tech }, languages: new[] { "go" });

        var result = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" }));

        Assert.Equal("Backend Engineer", result.Experience[0].Role);
    }

    [Fact]
    public void Bullets_keep_their_original_relative_order()
    {
        var bullets = Enumerable.Range(1, 6).Select(i => $"{LongBullet} #{i}").ToArray();
        var cv = Master(new[] { Role("Backend Engineer", "Present", bullets) }, languages: new[] { "go" });

        var selected = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" })).Experience.Single().Bullets;

        var indexes = selected.Select(b => Array.IndexOf(bullets, b)).ToList();
        Assert.All(indexes, i => Assert.True(i >= 0));
        Assert.Equal(indexes.OrderBy(i => i), indexes);
    }

    [Fact]
    public void Bullets_with_metrics_and_skill_mentions_win_the_cut()
    {
        var weak = "Helped the team with various tasks around the office";
        var strong = "Built a Go service cutting latency by 40% for 2M users every single day";
        var cv = Master(new[] { Role("Backend Engineer", "Present", weak, weak + " 2", weak + " 3", strong) },
            languages: new[] { "go" });

        var selected = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" })).Experience.Single().Bullets;

        Assert.Contains(strong, selected);
    }

    [Fact]
    public void Header_and_education_come_from_the_master()
    {
        var cv = Master(new[] { Role("Engineer", "Present", LongBullet) });
        cv.Education = new List<Education> { new() { Degree = "B.S. CS" } };

        var result = _selector.SelectAndAdaptCV(cv, Req());

        Assert.Equal(cv.Header.Email, result.Header.Email);
        Assert.Equal("B.S. CS", result.Education.Single().Degree);
    }

    [Fact]
    public void Summary_is_generated_and_within_300_chars()
    {
        var cv = Master(new[] { Role("Engineer", "Present", LongBullet) }, languages: new[] { "go" });

        var result = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" }));

        Assert.False(string.IsNullOrWhiteSpace(result.Summary));
        Assert.True(result.Summary.Length <= 300);
    }

    [Fact]
    public void Projects_capped_per_mode_and_ranked_by_jd_fit()
    {
        var cv = Master(new[] { Role("Engineer", "Present", LongBullet) });
        cv.Projects = new List<Project>
        {
            new() { Name = "Unrelated", Description = "Knitting app", Technologies = new() { "swift" } },
            new() { Name = "Fit", Description = "Go service", Technologies = new() { "go" } },
            new() { Name = "Fit2", Description = "Go cli", Technologies = new() { "go" } }
        };
        var req = Req(skills: new[] { "go" });

        var onePage = _selector.SelectAndAdaptCV(cv, req, false);
        var twoPage = _selector.SelectAndAdaptCV(cv, req, true);

        Assert.Single(onePage.Projects);
        Assert.StartsWith("Fit", onePage.Projects[0].Name);
        Assert.Equal(2, twoPage.Projects.Count);
    }

    [Fact]
    public void Profile_without_projects_or_experience_bullets_does_not_crash()
    {
        var result = _selector.SelectAndAdaptCV(FixtureLoader.LoadProfile("cv-master-edge"),
            Req(skills: new[] { "php" }));

        Assert.Empty(result.Projects);
        Assert.NotEmpty(result.Experience);
    }

    [Fact]
    public void Generated_cv_shares_no_references_with_the_master()
    {
        var cv = Master(new[] { Role("Backend Engineer", "Present", LongBullet) }, languages: new[] { "go" });
        cv.Education = new List<Education> { new() { Degree = "B.S. CS", RelevantCourses = new() { "Algorithms" } } };
        cv.Projects = new List<Project> { new() { Name = "P", Description = "d", Technologies = new() { "go" } } };

        var result = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" }));

        Assert.NotSame(cv.Header, result.Header);
        Assert.NotSame(cv.Education, result.Education);
        Assert.NotSame(cv.Education[0], result.Education[0]);
        Assert.NotSame(cv.Education[0].RelevantCourses, result.Education[0].RelevantCourses);
        Assert.NotSame(cv.Experience[0].Skills, result.Experience[0].Skills);
        Assert.NotSame(cv.Experience[0].Bullets, result.Experience[0].Bullets);
        Assert.All(result.Projects, p => Assert.DoesNotContain(cv.Projects, m => ReferenceEquals(m, p)));
    }

    [Fact]
    public void Bullet_scoring_does_not_count_a_skill_inside_another_word()
    {
        // The three "related" bullets only contain "go" inside "good"; the real one names the Go skill.
        var related = "Improved the good team spirit and delivery rhythm by 40% across the whole department staff";
        var real = "Wrote the Go service that handles the payment retries for all merchants";
        var cv = Master(new[]
        {
            Exp(role: "Backend Engineer", skills: new[] { "go" },
                bullets: new[] { related, related + " 2", related + " 3", real })
        }, languages: new[] { "go" });

        var selected = _selector.SelectAndAdaptCV(cv, Req(skills: new[] { "go" })).Experience.Single().Bullets;

        Assert.Contains(real, selected);
    }

    [Fact]
    public void Project_ranking_uses_whole_word_keywords()
    {
        var cv = Master(new[] { Exp(role: "Engineer", bullets: new[] { LongBullet }) });
        cv.Projects = new List<Project>
        {
            new() { Name = "Substring", Description = "A goodwill campaign", Technologies = new() { "swift" } },
            new() { Name = "Whole", Description = "A good campaign", Technologies = new() { "swift" } }
        };

        var result = _selector.SelectAndAdaptCV(cv, Req(keywords: new[] { "good" }));

        Assert.Equal("Whole", result.Projects.Single().Name);
    }
}

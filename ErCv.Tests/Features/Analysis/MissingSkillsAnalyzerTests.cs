using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.Matching;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Analysis;

[Trait("Category", "Unit")]
public class MissingSkillsAnalyzerTests
{
    private readonly MissingSkillsAnalyzer _analyzer = new();

    [Fact]
    public void Skill_present_in_profile_skills_is_not_missing()
    {
        var cv = Master(languages: new[] { "go" });

        var r = _analyzer.Analyze(cv, Req(skills: new[] { "golang" }));

        Assert.Empty(r.RequiredMissing);
        Assert.Empty(r.NiceToHaveMissing);
    }

    [Fact]
    public void Skill_mentioned_only_in_a_bullet_is_not_missing()
    {
        var cv = Master(new[] { Exp(bullets: new[] { "Deployed services on Rust runtime" }) });

        var r = _analyzer.Analyze(cv, Req(skills: new[] { "rust" }));

        Assert.Empty(r.RequiredMissing);
    }

    [Fact]
    public void Missing_skills_split_by_what_the_post_marks_optional()
    {
        var cv = Master(languages: new[] { "python" });

        var r = _analyzer.Analyze(cv, Req(skills: new[] { "haskell", "kubernetes", "terraform" }, niceToHave: new[] { "terraform" }));

        Assert.Equal(new[] { "haskell", "kubernetes" }, r.RequiredMissing);
        Assert.Equal(new[] { "terraform" }, r.NiceToHaveMissing);
    }

    // MATCH-06: Kubernetes used to be nice to have in every post, from a fixed list of technologies
    [Fact]
    public void A_skill_the_post_does_not_mark_optional_is_required()
    {
        var r = _analyzer.Analyze(Master(languages: new[] { "python" }), Req(skills: new[] { "kubernetes", "kafka" }));

        Assert.Equal(new[] { "kubernetes", "kafka" }, r.RequiredMissing);
        Assert.Empty(r.NiceToHaveMissing);
    }

    // F25: in TC-01 GitOps is under "Nice to have" and RabbitMQ under "Requirements"
    [Fact]
    public void Tc01_follows_the_sections_of_the_post()
    {
        var requirements = new JobDescriptionParser().Parse(FixtureLoader.LoadJd("legacy/tc-01"));

        var r = _analyzer.Analyze(Master(languages: new[] { "python" }), requirements);

        Assert.Contains("rabbitmq", r.RequiredMissing);
        Assert.Contains("gitops", r.NiceToHaveMissing);
        Assert.DoesNotContain("gitops", r.RequiredMissing);
        Assert.DoesNotContain("rabbitmq", r.NiceToHaveMissing);
    }

    [Fact]
    public void Lists_are_capped_and_overflow_is_reported_in_suggestions()
    {
        var many = Enumerable.Range(0, 12).Select(i => $"zzskill{i}").ToArray();

        var r = _analyzer.Analyze(Master(), Req(skills: many));

        Assert.Equal(8, r.RequiredMissing.Count);
        Assert.Contains(r.Suggestions, s => s.Contains("7 more required skills"));
    }

    [Fact]
    public void Potential_increase_is_proportional_and_capped_at_40()
    {
        var cv = Master(languages: new[] { "a" });

        var half = _analyzer.Analyze(cv, Req(skills: new[] { "a", "zzmissing" }));
        var all = _analyzer.Analyze(Master(), Req(skills: new[] { "zz1", "zz2" }));

        Assert.Equal(25, half.PotentialScoreIncrease);
        Assert.Equal(40, all.PotentialScoreIncrease);
    }

    [Fact]
    public void No_requirements_means_nothing_missing_and_zero_increase()
    {
        var r = _analyzer.Analyze(Master(), Req());

        Assert.Empty(r.RequiredMissing);
        Assert.Empty(r.Suggestions);
        Assert.Equal(0, r.PotentialScoreIncrease);
    }
}

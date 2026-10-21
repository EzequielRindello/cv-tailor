using ErCv.Web.Features.Analysis;
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
    public void Missing_skills_split_into_required_and_nice_to_have()
    {
        var cv = Master(languages: new[] { "python" });

        var r = _analyzer.Analyze(cv, Req(skills: new[] { "haskell", "kubernetes", "terraform" }));

        Assert.Equal(new[] { "haskell" }, r.RequiredMissing);
        Assert.Equal(new[] { "kubernetes", "terraform" }, r.NiceToHaveMissing);
    }

    // "rust" used to match the nice-to-have entry "zero trust" through substring matching (F1).
    [Fact]
    public void Rust_is_required_not_nice_to_have()
    {
        var r = _analyzer.Analyze(Master(languages: new[] { "python" }), Req(skills: new[] { "rust" }));

        Assert.Equal(new[] { "rust" }, r.RequiredMissing);
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

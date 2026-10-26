using ErCv.Web.Features.Analysis;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Analysis;

// EDIT-08: match and gaps measure the CV as it stands, so leaving something out of the CV shows, and adding it back closes it
[Trait("Category", "Unit")]
public class CvMeasuredAnalysisTests
{
    private static CVMaster Profile() => Master(
        new[] { Exp(role: "Senior Engineer", bullets: new[] { "Built the billing API" }) },
        languages: new[] { "Go", "Kafka" });

    private static GeneratedCV Cv(params string[] skills) =>
        Generated(new[] { Exp(role: "Senior Engineer", bullets: new[] { "Built the billing API" }) }, skills: skills);

    private static JobRequirements Post() => Req(skills: new[] { "go", "kafka", "rust" }, seniority: "senior");

    [Fact]
    public void A_skill_the_profile_has_but_the_cv_left_out_is_a_gap_of_the_cv_not_of_the_person()
    {
        var gaps = new MissingSkillsAnalyzer().Analyze(Cv("Go"), Profile(), Post());

        Assert.Equal(new[] { "kafka", "rust" }, gaps.RequiredMissing);
        Assert.Equal(new[] { "kafka" }, gaps.InProfileNotInCv);
    }

    [Fact]
    public void Adding_the_skill_back_closes_the_gap_and_raises_the_match()
    {
        var calculator = new MatchScoreCalculator();
        var before = calculator.Calculate(Cv("Go"), Post());
        var after = calculator.Calculate(Cv("Go", "Kafka"), Post());
        var gaps = new MissingSkillsAnalyzer().Analyze(Cv("Go", "Kafka"), Profile(), Post());

        Assert.True(after.Overall > before.Overall);
        Assert.Equal(before.SkillsMatched + 1, after.SkillsMatched);
        Assert.Equal(new[] { "rust" }, gaps.RequiredMissing);
        Assert.Empty(gaps.InProfileNotInCv);
    }

    [Fact]
    public void The_cv_measure_is_never_above_the_profile_measure()
    {
        var calculator = new MatchScoreCalculator();

        var ofCv = calculator.Calculate(Cv("Go"), Post());
        var ofProfile = calculator.Calculate(Profile(), Post());

        Assert.True(ofCv.Overall <= ofProfile.Overall);
    }

    [Fact]
    public void A_cv_that_left_part_of_the_profile_out_carries_the_profile_match()
    {
        var calculator = new MatchScoreCalculator();

        var partial = calculator.Calculate(Cv("Go"), Profile(), Post());
        var whole = calculator.Calculate(Cv("Go", "Kafka"), Profile(), Post());

        Assert.Equal(calculator.Calculate(Profile(), Post()).Overall, partial.ProfileOverall);
        Assert.True(partial.ProfileOverall > partial.Overall);
        Assert.Equal(calculator.Calculate(Cv("Go"), Post()).Overall, partial.Overall);
        // Nothing left out: no second number
        Assert.Null(whole.ProfileOverall);
    }

    [Fact]
    public void The_cv_is_read_without_copying_anything_it_does_not_hold()
    {
        var profile = CvAsProfile.Of(Cv("Go", "SQL"));

        Assert.Equal(new[] { "Go", "SQL" }, profile.Skills.Languages);
        Assert.Empty(profile.Skills.Tools);
        Assert.Single(profile.Experience);
    }
}

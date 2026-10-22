using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Analysis;
using ErCv.Web.Models;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class ScoreCardsTests : BunitContext
{
    private static MatchScore Match(int overall, string color, string? warning = null) => new()
    {
        Overall = overall,
        ColorClass = color,
        SkillsMatched = 3,
        TotalSkills = 5,
        KeywordsMatched = 4,
        TotalKeywords = 10,
        SeniorityMatch = true,
        LowMatchWarning = warning
    };

    [Theory]
    [InlineData(85, "green", "Strong Match")]
    [InlineData(55, "yellow", "Moderate Match")]
    [InlineData(25, "red", "Weak Match")]
    public void The_match_card_shows_the_label_and_tone_of_each_color(int overall, string color, string label)
    {
        var cut = Render<MatchScoreCard>(p => p.Add(c => c.Data, Match(overall, color)));

        Assert.Equal(label, cut.Find($".match-badge.{color}").TextContent);
        Assert.Contains(color, cut.Find(".score-gauge").ClassList);
        Assert.Equal($"Match score {overall} out of 100", cut.Find(".score-gauge").GetAttribute("aria-label"));
    }

    [Fact]
    public void The_match_card_falls_back_to_the_moderate_tone_without_a_color()
    {
        var cut = Render<MatchScoreCard>(p => p.Add(c => c.Data, Match(50, string.Empty)));

        Assert.Equal("Moderate Match", cut.Find(".match-badge.yellow").TextContent);
    }

    [Fact]
    public void The_low_match_warning_is_shown_only_when_the_score_has_one()
    {
        var low = Render<MatchScoreCard>(p => p.Add(c => c.Data, Match(25, "red", MatchScoreConstants.LowMatchWarning)));
        var fine = Render<MatchScoreCard>(p => p.Add(c => c.Data, Match(85, "green")));

        Assert.Contains(MatchScoreConstants.LowMatchWarning, low.Find(".match-low-warning").TextContent);
        Assert.Empty(fine.FindAll(".match-low-warning"));
    }

    [Fact]
    public void The_breakdown_marks_what_falls_short()
    {
        var score = Match(40, "yellow");
        score.SeniorityMatch = false;

        var cut = Render<MatchScoreCard>(p => p.Add(c => c.Data, score));

        // 3/5 skills is the 60% the card accepts, 4/10 keywords is under its 50%
        var icons = cut.FindAll(".breakdown-icon").Select(i => i.ClassList.Contains("ok")).ToArray();
        Assert.Equal(new[] { true, false, false }, icons);
        Assert.Contains("Mismatch", cut.Markup);
    }

    [Theory]
    [InlineData(95, "green", "ATS Ready")]
    [InlineData(70, "yellow", "Needs Attention")]
    [InlineData(40, "red", "ATS Risk")]
    public void The_ats_card_shows_the_label_and_tone_of_each_color(int overall, string color, string label)
    {
        var cut = Render<ATSScoreCard>(p => p.Add(c => c.Data, new ATSScore { Overall = overall, ColorClass = color }));

        Assert.Equal(label, cut.Find($".ats-badge.{color}").TextContent);
        Assert.Contains(color, cut.Find(".score-gauge").ClassList);
    }

    [Fact]
    public void The_ats_card_lists_issues_or_says_the_cv_is_clear_and_folds_away()
    {
        var issues = new ATSScore
        {
            Overall = 70,
            ColorClass = "yellow",
            Issues = new() { new ATSIssue { Severity = "High", Issue = "No email", Fix = "Add an email" } }
        };

        var withIssues = Render<ATSScoreCard>(p => p.Add(c => c.Data, issues));
        var clear = Render<ATSScoreCard>(p => p.Add(c => c.Data, new ATSScore { Overall = 100, ColorClass = "green" }));

        Assert.Equal("High", withIssues.Find(".severity-badge.High").TextContent);
        Assert.Contains("Add an email", withIssues.Find(".issue-fix").TextContent);
        Assert.Contains("fully optimized", clear.Find(".ats-all-clear").TextContent);

        withIssues.Find(".ats-card-header").Click();
        Assert.Empty(withIssues.FindAll(".ats-issue-row"));
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(140, 100)]
    public void The_gauge_keeps_its_value_between_0_and_100(int value, int shown)
    {
        var cut = Render<ScoreGauge>(p => p.Add(c => c.Value, value).Add(c => c.Label, "Score"));

        Assert.Equal($"Score {shown} out of 100", cut.Find(".score-gauge").GetAttribute("aria-label"));
    }
}

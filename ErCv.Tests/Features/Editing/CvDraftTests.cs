using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Editing;

[Trait("Category", "Unit")]
public class CvDraftTests
{
    private static GeneratedCV Source()
    {
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", bullets: new[] { "Built the billing API", "Cut latency by 40%", "Ran on-call" }),
            Exp(role: "Developer", bullets: new[] { "Maintained the monolith" })
        }, skills: new[] { "Go", "Kafka", "SQL" });
        cv.Summary = "Backend engineer.";
        cv.Projects = new List<Project> { new() { Name = "Ledger" }, new() { Name = "CLI" } };
        return cv;
    }

    private static CvDraft Draft(GeneratedCV? source = null) =>
        new(source ?? Source(), DocumentConstants.HarvardSectionOrder);

    [Fact]
    public void Without_edits_the_built_cv_is_the_generated_one()
    {
        var source = Source();
        var draft = Draft(source);

        var cv = draft.Build();

        Assert.False(draft.IsChanged);
        Assert.Equal(source.Experience.Select(e => e.Bullets), cv.Experience.Select(e => e.Bullets));
        Assert.Equal(source.Skills, cv.Skills);
        Assert.Equal(source.Projects.Select(p => p.Name), cv.Projects.Select(p => p.Name));
        Assert.Equal(source.Summary, cv.Summary);
        Assert.Empty(cv.SectionOrder);
        Assert.Empty(cv.EditedBullets);
    }

    [Fact]
    public void An_excluded_bullet_leaves_the_cv_but_the_last_one_stays()
    {
        var draft = Draft();

        Assert.True(draft.ToggleBullet(0, 1));
        Assert.False(draft.ToggleBullet(1, 0));

        var cv = draft.Build();
        Assert.Equal(new[] { "Built the billing API", "Ran on-call" }, cv.Experience[0].Bullets);
        Assert.Equal(new[] { "Maintained the monolith" }, cv.Experience[1].Bullets);
        Assert.True(draft.IsChanged);
    }

    [Fact]
    public void A_rewritten_bullet_is_trimmed_used_and_flagged_with_its_original()
    {
        var draft = Draft();

        Assert.True(draft.EditBullet(0, 0, "  Built and ran the billing API in Go  "));

        var cv = draft.Build();
        Assert.Equal("Built and ran the billing API in Go", cv.Experience[0].Bullets[0]);
        var edit = Assert.Single(cv.EditedBullets);
        Assert.Equal(new BulletEdit("Built the billing API", "Built and ran the billing API in Go"), edit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_empty_rewrite_is_refused(string text)
    {
        var draft = Draft();

        Assert.False(draft.EditBullet(0, 0, text));
        Assert.Equal("Built the billing API", draft.Build().Experience[0].Bullets[0]);
    }

    [Fact]
    public void A_rewrite_over_the_length_limit_is_refused()
    {
        var draft = Draft();

        Assert.False(draft.EditBullet(0, 0, new string('a', ApiConstants.MaxEditedBulletLength + 1)));
        Assert.True(draft.EditBullet(0, 0, new string('a', ApiConstants.MaxEditedBulletLength)));
    }

    [Fact]
    public void Undoing_a_rewrite_or_typing_the_original_again_drops_the_flag()
    {
        var draft = Draft();

        draft.EditBullet(0, 0, "Something else");
        draft.RevertBullet(0, 0);
        Assert.Empty(draft.Build().EditedBullets);

        draft.EditBullet(0, 1, "Another text");
        draft.EditBullet(0, 1, "Cut latency by 40%");
        Assert.Empty(draft.Build().EditedBullets);
        Assert.False(draft.IsChanged);
    }

    [Fact]
    public void An_excluded_rewritten_bullet_is_not_listed_as_edited()
    {
        var draft = Draft();

        draft.EditBullet(0, 0, "Rewritten");
        draft.ToggleBullet(0, 0);

        Assert.Empty(draft.Build().EditedBullets);
    }

    [Fact]
    public void Bullets_can_be_reordered()
    {
        var draft = Draft();

        draft.MoveBullet(0, 2, 0);

        Assert.Equal(new[] { "Ran on-call", "Built the billing API", "Cut latency by 40%" }, draft.Build().Experience[0].Bullets);
        Assert.True(draft.IsChanged);
    }

    [Fact]
    public void The_summary_can_be_left_out_and_put_back()
    {
        var draft = Draft();

        draft.ToggleSummary();
        Assert.Equal(string.Empty, draft.Build().Summary);

        draft.ToggleSummary();
        Assert.Equal("Backend engineer.", draft.Build().Summary);
    }

    [Fact]
    public void A_cv_without_summary_cannot_include_one()
    {
        var source = Source();
        source.Summary = string.Empty;
        var draft = Draft(source);

        draft.ToggleSummary();

        Assert.False(draft.HasSummary);
        Assert.False(draft.IncludeSummary);
        Assert.Equal(string.Empty, draft.Build().Summary);
    }

    [Fact]
    public void Skills_and_projects_can_be_left_out_one_by_one()
    {
        var draft = Draft();

        draft.ToggleSkill(1);
        draft.ToggleProject(0);

        var cv = draft.Build();
        Assert.Equal(new[] { "Go", "SQL" }, cv.Skills);
        Assert.Equal(new[] { "CLI" }, cv.Projects.Select(p => p.Name));
    }

    [Fact]
    public void Moving_a_section_sets_the_order_and_moving_it_back_clears_it()
    {
        var draft = Draft();

        draft.MoveSection(3, 0);
        Assert.Equal(
            new[] { "skills", "summary", "education", "experience", "projects" },
            draft.Build().SectionOrder);

        draft.MoveSection(0, 3);
        Assert.Empty(draft.Build().SectionOrder);
        Assert.False(draft.IsChanged);
    }

    [Fact]
    public void The_section_list_starts_in_the_template_order()
    {
        var draft = new CvDraft(Source(), DocumentConstants.ModernSectionOrder);

        Assert.Equal(DocumentConstants.ModernSectionOrder, draft.SectionOrder);
    }

    [Fact]
    public void Reset_undoes_every_edit()
    {
        var draft = Draft();
        draft.EditBullet(0, 0, "Rewritten");
        draft.ToggleBullet(0, 1);
        draft.ToggleSkill(0);
        draft.ToggleProject(1);
        draft.ToggleSummary();
        draft.MoveSection(0, 4);

        draft.Reset();

        Assert.False(draft.IsChanged);
        Assert.Empty(draft.Build().EditedBullets);
        Assert.Empty(draft.Build().SectionOrder);
    }

    [Fact]
    public void Editing_never_changes_the_generated_cv()
    {
        var source = Source();
        var before = System.Text.Json.JsonSerializer.Serialize(source);
        var draft = Draft(source);

        draft.EditBullet(0, 0, "Rewritten");
        draft.ToggleBullet(0, 1);
        draft.ToggleSkill(0);
        draft.MoveSection(0, 1);
        draft.Build().Experience[0].Bullets.Add("added to the built copy");

        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(source));
        Assert.DoesNotContain("added to the built copy", draft.Build().Experience[0].Bullets);
    }

    // The no-fabrication rule with edits: every bullet is the profile's text, or a rewrite flagged with the original.
    [Fact]
    public void Every_bullet_is_a_profile_bullet_or_a_flagged_rewrite_of_one()
    {
        var source = Source();
        var draft = Draft(source);
        draft.EditBullet(0, 2, "Led the on-call rotation");
        draft.MoveBullet(0, 2, 0);
        draft.EditBullet(1, 0, "Kept the monolith healthy");

        var cv = draft.Build();
        var profileBullets = source.Experience.SelectMany(e => e.Bullets).ToHashSet();
        var edits = cv.EditedBullets.ToDictionary(e => e.Edited, e => e.Original);

        Assert.All(cv.Experience.SelectMany(e => e.Bullets), b =>
            Assert.True(profileBullets.Contains(b) || (edits.TryGetValue(b, out var original) && profileBullets.Contains(original)), b));
    }
}

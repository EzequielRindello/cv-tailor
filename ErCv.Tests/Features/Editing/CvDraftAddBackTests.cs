using System.Text.Json;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using ErCv.Tests.Support;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Editing;

// EDIT-07: what the selection or the page fit left out can be added back, and only from the profile
[Trait("Category", "Unit")]
public class CvDraftAddBackTests
{
    private static CVMaster Profile()
    {
        var profile = Master(new[]
        {
            Exp(role: "Backend Engineer", bullets: new[] { "Built the billing API", "Cut latency by 40%", "Ran on-call", "Wrote the runbook" }),
            Exp(role: "Developer", company: "Beta", start: "Jan 2017", end: "Dec 2019", bullets: new[] { "Maintained the monolith", "Migrated to Postgres" }),
            Exp(role: "Intern", company: "Old", start: "Jan 2015", end: "Dec 2015", bullets: new[] { "Fixed bugs", "Wrote tests" })
        }, languages: new[] { "Go", "Kafka", "SQL", "Rust" }, tools: new[] { "Docker" });
        profile.Projects = new List<Project> { new() { Name = "Ledger" }, new() { Name = "CLI" }, new() { Name = "Tracker" } };
        return profile;
    }

    // The CV kept the first and third bullet of the first job, the second job whole, and left the intern job out
    private static GeneratedCV Source()
    {
        var cv = Build.Generated(new[]
        {
            Exp(role: "Backend Engineer", bullets: new[] { "Built the billing API", "Ran on-call" }),
            Exp(role: "Developer", company: "Beta", start: "Jan 2017", end: "Dec 2019", bullets: new[] { "Maintained the monolith", "Migrated to Postgres" })
        }, skills: new[] { "Go", "Kafka", "SQL" });
        cv.Projects = new List<Project> { new() { Name = "Ledger" } };
        return cv;
    }

    private static CvDraft Draft(CVMaster? profile = null) =>
        new(Source(), DocumentConstants.HarvardSectionOrder, profile ?? Profile());

    [Fact]
    public void Without_a_profile_nothing_is_left_out()
    {
        var draft = new CvDraft(Source(), DocumentConstants.HarvardSectionOrder);

        Assert.Empty(draft.LeftOutExperiences);
        Assert.Empty(draft.LeftOutSkills);
        Assert.Empty(draft.LeftOutProjects);
        Assert.Empty(draft.LeftOutBullets(0));
        Assert.False(draft.HasAdditions);
    }

    [Fact]
    public void Left_out_items_are_what_the_profile_has_and_the_cv_does_not()
    {
        var draft = Draft();

        Assert.Equal("Intern", Assert.Single(draft.LeftOutExperiences).Role);
        Assert.Equal(new[] { "Rust", "Docker" }, draft.LeftOutSkills);
        Assert.Equal(new[] { "CLI", "Tracker" }, draft.LeftOutProjects.Select(p => p.Name));
        Assert.Equal(new[] { (1, "Cut latency by 40%"), (3, "Wrote the runbook") }, draft.LeftOutBullets(0));
        Assert.Empty(draft.LeftOutBullets(1));
    }

    [Fact]
    public void Skills_are_compared_ignoring_case()
    {
        var generated = Source();
        generated.Skills = new List<string> { "go", "KAFKA", "sql" };

        var draft = new CvDraft(generated, DocumentConstants.HarvardSectionOrder, Profile());

        Assert.Equal(new[] { "Rust", "Docker" }, draft.LeftOutSkills);
    }

    [Fact]
    public void An_added_bullet_goes_where_the_profile_has_it_and_is_not_listed_as_edited()
    {
        var draft = Draft();

        Assert.True(draft.AddBullet(0, 1));
        Assert.True(draft.AddBullet(0, 3));

        Assert.Equal(new[] { "Built the billing API", "Cut latency by 40%", "Ran on-call", "Wrote the runbook" },
            draft.Build().Experience[0].Bullets);
        Assert.Empty(draft.Build().EditedBullets);
        Assert.True(draft.HasAdditions);
        Assert.True(draft.IsChanged);
        Assert.Empty(draft.LeftOutBullets(0));
    }

    [Fact]
    public void A_bullet_that_is_not_left_out_cannot_be_added()
    {
        var draft = Draft();

        Assert.False(draft.AddBullet(0, 0));
        Assert.False(draft.AddBullet(0, 99));
    }

    [Fact]
    public void An_added_bullet_that_is_rewritten_is_listed_with_the_profile_text_as_its_original()
    {
        var draft = Draft();
        draft.AddBullet(0, 1);

        draft.EditBullet(0, 1, "Cut p99 latency by 40%");

        var edit = Assert.Single(draft.Build().EditedBullets);
        Assert.Equal(("Cut latency by 40%", "Cut p99 latency by 40%"), (edit.Original, edit.Edited));
    }

    [Fact]
    public void Turning_off_an_added_bullet_puts_it_back_in_the_left_out_list()
    {
        var draft = Draft();
        draft.AddBullet(0, 1);

        Assert.True(draft.ToggleBullet(0, 1));

        Assert.Equal(2, draft.Experiences[0].Bullets.Count);
        Assert.Contains(draft.LeftOutBullets(0), b => b.ProfileIndex == 1);
        Assert.False(draft.HasAdditions);
    }

    [Fact]
    public void An_added_experience_comes_in_profile_order_with_its_first_bullet()
    {
        var generated = Source();
        generated.Experience.RemoveAt(1);
        generated.Experience.Add(Exp(role: "Intern", company: "Old", start: "Jan 2015", end: "Dec 2015", bullets: new[] { "Fixed bugs" }));
        var draft = new CvDraft(generated, DocumentConstants.HarvardSectionOrder, Profile());

        Assert.True(draft.AddExperience(0));

        Assert.Equal(new[] { "Backend Engineer", "Developer", "Intern" }, draft.Build().Experience.Select(e => e.Role));
        Assert.Equal(new[] { "Maintained the monolith" }, draft.Build().Experience[1].Bullets);
        Assert.Contains(draft.LeftOutBullets(1), b => b.Text == "Migrated to Postgres");
    }

    [Fact]
    public void An_added_experience_with_no_later_one_goes_last_and_can_be_removed_but_a_generated_one_cannot()
    {
        var draft = Draft();

        Assert.True(draft.AddExperience(0));

        Assert.Equal("Intern", draft.Experiences[^1].Source.Role);
        Assert.False(draft.RemoveExperience(0));
        Assert.True(draft.RemoveExperience(2));
        Assert.Equal(2, draft.Experiences.Count);
        Assert.Single(draft.LeftOutExperiences);
    }

    [Fact]
    public void An_added_skill_and_project_leave_again_when_turned_off()
    {
        var draft = Draft();

        Assert.True(draft.AddSkill("rust"));
        Assert.True(draft.AddProject(0));
        Assert.Equal("Rust", draft.Build().Skills.Last());
        Assert.Equal("CLI", draft.Build().Projects.Last().Name);

        draft.ToggleSkill(draft.Skills.Count - 1);
        draft.ToggleProject(draft.Projects.Count - 1);

        Assert.Contains("Rust", draft.LeftOutSkills);
        Assert.Contains(draft.LeftOutProjects, p => p.Name == "CLI");
        Assert.False(draft.HasAdditions);
    }

    [Fact]
    public void Only_what_the_profile_has_can_be_added()
    {
        var draft = Draft();

        Assert.False(draft.AddSkill("COBOL"));
        Assert.False(draft.AddProject(99));
        Assert.False(draft.AddExperience(99));
        Assert.False(draft.AddSkill("Go"));
    }

    [Fact]
    public void Undo_all_drops_every_addition()
    {
        var draft = Draft();
        draft.AddBullet(0, 1);
        draft.AddExperience(0);
        draft.AddSkill("Rust");
        draft.AddProject(0);

        draft.Reset();

        Assert.False(draft.HasAdditions);
        Assert.False(draft.IsChanged);
        Assert.Equal(2, draft.Experiences.Count);
        Assert.Equal(3, draft.Skills.Count);
    }

    [Fact]
    public void Two_identical_bullets_are_counted_not_merged()
    {
        var profile = Master(new[] { Exp(bullets: new[] { "Fixed bugs", "Fixed bugs", "Wrote tests" }) });
        var generated = Build.Generated(new[] { Exp(bullets: new[] { "Fixed bugs" }) });
        var draft = new CvDraft(generated, DocumentConstants.HarvardSectionOrder, profile);

        Assert.Equal(new[] { (1, "Fixed bugs"), (2, "Wrote tests") }, draft.LeftOutBullets(0));
    }

    [Fact]
    public void Neither_the_generated_cv_nor_the_profile_is_changed_by_adding()
    {
        var profile = Profile();
        var generated = Source();
        var before = (JsonSerializer.Serialize(profile), JsonSerializer.Serialize(generated));
        var draft = new CvDraft(generated, DocumentConstants.HarvardSectionOrder, profile);

        draft.AddBullet(0, 1);
        draft.AddExperience(0);
        draft.AddSkill("Rust");
        draft.EditBullet(0, 1, "Changed");
        draft.Build();

        Assert.Equal(before, (JsonSerializer.Serialize(profile), JsonSerializer.Serialize(generated)));
    }
}

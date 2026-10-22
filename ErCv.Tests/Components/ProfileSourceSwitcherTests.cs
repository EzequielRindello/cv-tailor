using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Constants;
using ErCv.Web.UI;
using static ErCv.Web.Services.CVGeneratorService;

namespace ErCv.Tests.Components;

/// <summary>Step 2: the senior sample first, then the creator's profile, then the visitor's own.</summary>
[Trait("Category", "Component")]
public class ProfileSourceSwitcherTests : BunitContext
{
    private static readonly List<ProfileInfo> StoredProfiles = new()
    {
        new(ApiConstants.DefaultProfileId, ApiConstants.DefaultProfileName),
        new(ApiConstants.SeniorSampleProfileId, "senior")
    };

    [Fact]
    public void Stored_profiles_come_first_with_the_senior_sample_on_top_and_checked_by_default()
    {
        var cut = Render<ProfileSourceSwitcher>(p => p.Add(c => c.Profiles, StoredProfiles));

        var radios = cut.FindAll("input[type=radio]");
        Assert.Equal(new[] { "source-cv-master-senior", "source-demo", "source-mine" }, radios.Select(r => r.Id));
        Assert.True(radios[0].HasAttribute("checked"));
        Assert.Equal(UiConstants.SeniorSampleProfileLabel, cut.Find("label[for='source-cv-master-senior'] .profile-source-name").TextContent);
        Assert.Equal(UiConstants.CreatorProfileLabel, cut.Find("label[for='source-demo'] .profile-source-name").TextContent);
    }

    [Fact]
    public void Picking_a_stored_profile_reports_its_id_and_the_stored_source()
    {
        string? id = null, source = null;
        var cut = Render<ProfileSourceSwitcher>(p => p
            .Add(c => c.Profiles, StoredProfiles)
            .Add(c => c.Value, UiConstants.ProfileSourceMine)
            .Add(c => c.HasMine, true)
            .Add(c => c.ProfileIdChanged, (string v) => id = v)
            .Add(c => c.ValueChanged, (string v) => source = v));

        cut.Find("#source-demo").Change(true);

        Assert.Equal(ApiConstants.DefaultProfileId, id);
        Assert.Equal(UiConstants.ProfileSourceDemo, source);
    }

    [Fact]
    public void My_profile_is_disabled_until_one_is_saved()
    {
        var cut = Render<ProfileSourceSwitcher>(p => p.Add(c => c.Profiles, StoredProfiles));

        Assert.True(cut.Find("#source-mine").HasAttribute("disabled"));
    }

    [Fact]
    public void Without_a_saved_profile_its_card_is_empty_and_points_to_the_header()
    {
        var cut = Render<ProfileSourceSwitcher>(p => p.Add(c => c.Profiles, StoredProfiles));

        var card = cut.Find("#source-mine").ParentElement!;
        Assert.Contains("empty", card.ClassList);
        Assert.Contains("disabled", card.ClassList);
        Assert.Equal("Not created yet. Use Profile at the top.", card.QuerySelector(".profile-source-desc")!.TextContent);
        // Editing the profile lives in the header now, not in the card
        Assert.Empty(cut.FindAll("a"));
    }

    [Fact]
    public void Only_the_picked_card_is_marked_selected()
    {
        var cut = Render<ProfileSourceSwitcher>(p => p
            .Add(c => c.Profiles, StoredProfiles)
            .Add(c => c.Value, UiConstants.ProfileSourceMine)
            .Add(c => c.HasMine, true));

        var selected = cut.FindAll(".profile-source-card.selected");
        Assert.Single(selected);
        Assert.NotNull(selected[0].QuerySelector("#source-mine"));
        Assert.Equal("Saved in this browser", selected[0].QuerySelector(".profile-source-desc")!.TextContent);
        Assert.DoesNotContain("empty", selected[0].ClassList);
    }
}

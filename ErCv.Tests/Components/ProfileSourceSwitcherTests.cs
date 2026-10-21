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
        Assert.Equal(UiConstants.SeniorSampleProfileLabel, cut.Find("label[for='source-cv-master-senior']").TextContent);
        Assert.Equal(UiConstants.CreatorProfileLabel, cut.Find("label[for='source-demo']").TextContent);
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
        Assert.Equal("Create my profile", cut.Find("#profile-edit-link").TextContent);
    }
}

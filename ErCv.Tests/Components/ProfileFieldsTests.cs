using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.UI;
using Microsoft.AspNetCore.Components;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class ProfileFieldsTests : BunitContext
{
    [Fact]
    public void Field_without_error_has_a_label_and_no_invalid_state()
    {
        var cut = Render<ProfileField>(p => p.Add(c => c.Id, "f").Add(c => c.Label, "Role").Add(c => c.Value, "Eng"));

        Assert.Equal("f", cut.Find("label").GetAttribute("for"));
        Assert.Equal("Eng", cut.Find("input#f").GetAttribute("value"));
        Assert.Empty(cut.FindAll(".is-invalid"));
    }

    [Fact]
    public void Field_error_is_shown_and_linked_to_the_input()
    {
        var cut = Render<ProfileField>(p => p.Add(c => c.Id, "f").Add(c => c.Label, "Role").Add(c => c.Error, "Role is required."));

        var input = cut.Find("input#f");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("f-err", input.GetAttribute("aria-describedby"));
        Assert.Equal("Role is required.", cut.Find("#f-err").TextContent);
    }

    [Fact]
    public void Editing_a_field_reports_the_new_value()
    {
        string? seen = null;
        var cut = Render<ProfileField>(p => p
            .Add(c => c.Id, "f").Add(c => c.Label, "Role")
            .Add(c => c.ValueChanged, (string v) => seen = v));

        cut.Find("input#f").Change("Staff Engineer");

        Assert.Equal("Staff Engineer", seen);
    }

    [Fact]
    public void Inside_the_profile_page_a_field_reports_typing_then_the_change_and_the_blur()
    {
        var reports = new List<bool>();
        var cut = Render<CascadingValue<Action<bool>>>(p => p
            .Add(c => c.Name, UiConstants.ProfileEditedCascade)
            .Add(c => c.Value, (Action<bool>)reports.Add)
            .AddChildContent<ProfileField>(f => f.Add(c => c.Id, "f").Add(c => c.Label, "Role")));

        cut.Find("input#f").Input("Sta");
        cut.Find("input#f").Change("Staff");
        cut.Find("input#f").Blur();

        Assert.Equal(new[] { true, false, false }, reports);
    }

    [Fact]
    public void Adding_or_removing_a_list_item_is_reported_to_the_profile_page()
    {
        var reports = new List<bool>();
        var items = new List<string> { "One" };
        var cut = Render<CascadingValue<Action<bool>>>(p => p
            .Add(c => c.Name, UiConstants.ProfileEditedCascade)
            .Add(c => c.Value, (Action<bool>)reports.Add)
            .AddChildContent<ProfileListField>(f => f.Add(c => c.Id, "b").Add(c => c.Label, "Bullet").Add(c => c.Items, items)));

        cut.Find("#b-add").Click();
        cut.Find("[aria-label='Remove Bullet 1']").Click();

        Assert.Equal(new[] { string.Empty }, items);
        Assert.Equal(new[] { false, false }, reports);
    }

    [Fact]
    public void Tag_field_splits_on_commas_and_drops_blanks()
    {
        var values = new List<string> { "C#" };
        var cut = Render<ProfileTagField>(p => p.Add(c => c.Id, "t").Add(c => c.Label, "Skills").Add(c => c.Values, values));

        Assert.Equal("C#", cut.Find("input#t").GetAttribute("value"));

        cut.Find("input#t").Change(" Go ,, SQL , ");

        Assert.Equal(new[] { "Go", "SQL" }, values);
    }

    // The tag field passed the literal text "Error" down, so every skills field showed as invalid with that word
    [Fact]
    public void Tag_field_is_valid_unless_it_is_given_an_error()
    {
        var cut = Render<ProfileTagField>(p => p.Add(c => c.Id, "t").Add(c => c.Label, "Skills used").Add(c => c.Values, new List<string> { "C#" }));

        Assert.Empty(cut.FindAll(".is-invalid"));
        Assert.Empty(cut.FindAll(".profile-field-error"));

        cut.Render(p => p.Add(c => c.Error, "Skill 2 is empty."));

        Assert.Equal("Skill 2 is empty.", cut.Find("#t-err").TextContent);
    }

    [Fact]
    public void List_field_adds_edits_and_removes_items()
    {
        var items = new List<string> { "first" };
        var cut = Render<ProfileListField>(p => p.Add(c => c.Id, "b").Add(c => c.Label, "Bullet").Add(c => c.Items, items));

        cut.Find("#b-add").Click();
        Assert.Equal(2, items.Count);

        cut.Find("textarea#b-1").Change("second");
        Assert.Equal("second", items[1]);

        cut.Find("button[aria-label='Remove Bullet 1']").Click();
        Assert.Equal(new[] { "second" }, items);
    }

    [Fact]
    public void List_field_shows_the_error_of_the_item_that_has_one()
    {
        var cut = Render<ProfileListField>(p => p
            .Add(c => c.Id, "b").Add(c => c.Label, "Bullet")
            .Add(c => c.Items, new List<string> { "ok", "" })
            .Add(c => c.ErrorFor, (Func<int, string?>)(i => i == 1 ? "Bullet is empty." : null)));

        Assert.Equal("Bullet is empty.", Assert.Single(cut.FindAll(".profile-field-error")).TextContent);
    }
}

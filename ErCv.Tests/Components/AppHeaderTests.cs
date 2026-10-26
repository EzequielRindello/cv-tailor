using Bunit;
using Bunit.TestDoubles;
using ErCv.Web.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class AppHeaderTests : BunitContext
{
    public AppHeaderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void The_header_links_to_the_profile_page_with_a_name_that_survives_the_hidden_label()
    {
        var cut = Render<AppHeader>();

        var link = cut.Find("#header-profile-link");
        Assert.Equal("profile", link.GetAttribute("href"));
        Assert.Equal("Profile", link.GetAttribute("aria-label"));
        Assert.DoesNotContain("active", link.ClassList);
    }

    [Fact]
    public void The_wordmark_carries_the_product_name()
    {
        var cut = Render<AppHeader>();

        var wordmark = cut.Find(".app-wordmark");
        Assert.Equal("CV Tailor, home", wordmark.GetAttribute("aria-label"));
        Assert.Equal("Tailor", cut.Find(".app-wordmark-text").TextContent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("#samples")]
    public void The_wordmark_on_the_home_page_reloads_it(string current)
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo(current);
        var cut = Render<AppHeader>();

        cut.Find(".app-wordmark").Click();

        var last = navigation.History.First();
        Assert.Equal(navigation.BaseUri, last.Uri);
        Assert.True(last.Options.ForceLoad);
    }

    [Fact]
    public void The_wordmark_on_another_page_goes_home_without_a_reload()
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo("profile");
        var cut = Render<AppHeader>();

        cut.Find(".app-wordmark").Click();

        var last = navigation.History.First();
        Assert.Equal(navigation.BaseUri, last.Uri);
        Assert.False(last.Options.ForceLoad);
    }

    [Fact]
    public void On_the_profile_page_the_link_is_marked_active()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("profile");

        var cut = Render<AppHeader>();

        Assert.Contains("active", cut.Find("#header-profile-link").ClassList);
    }
}

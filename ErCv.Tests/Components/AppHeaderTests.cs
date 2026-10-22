using Bunit;
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
    public void On_the_profile_page_the_link_is_marked_active()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("profile");

        var cut = Render<AppHeader>();

        Assert.Contains("active", cut.Find("#header-profile-link").ClassList);
    }
}

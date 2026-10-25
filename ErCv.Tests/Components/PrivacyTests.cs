using Bunit;
using ErCv.Web.Components.Pages;
using ErCv.Web.Components.Shared;
using ErCv.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErCv.Tests.Components;

// OPS-05
[Trait("Category", "Component")]
public class PrivacyTests : BunitContext
{
    [Fact]
    public void The_privacy_page_states_the_configured_session_timeout()
    {
        Services.AddSingleton(new ErCvOptions { Session = new ErCvOptions.SessionSettings { IdleTimeoutMinutes = 7 } });

        var cut = Render<Privacy>();

        Assert.Contains("up to 7 minutes of inactivity", cut.Markup);
        Assert.Equal(3, cut.FindAll("section.card").Count);
        Assert.NotNull(cut.Find("a[href='/profile']"));
    }

    // The company typed in step 1 goes to the server and into the history; the API reads a post from a link too
    [Fact]
    public void The_privacy_page_names_the_company_and_the_api_import()
    {
        Services.AddSingleton(new ErCvOptions());

        var cut = Render<Privacy>();

        Assert.Equal(2, cut.FindAll("li").Count(li => li.TextContent.Contains("company name")));
        Assert.Contains("/api/cv/import-job-post", cut.Markup);
    }

    [Fact]
    public void The_footer_links_to_the_privacy_page()
    {
        var cut = Render<AppFooter>();

        Assert.Equal("Privacy", cut.Find("a[href='/privacy']").TextContent);
        Assert.DoesNotContain("don't store", cut.Markup);
    }
}

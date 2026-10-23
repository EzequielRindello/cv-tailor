using System.Text.Json;
using Bunit;
using ErCv.Tests.Support;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Matching;
using ErCv.Web.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class JobPostLinkTests : BunitContext
{
    private const string Link = "https://boards.greenhouse.io/stripe/jobs/6524123";

    private readonly ToastService _toasts = new();
    private string? _imported;

    public JobPostLinkTests()
    {
        var http = FakeHttp.Json(JsonSerializer.Serialize(new { title = "Backend Engineer", company_name = "Stripe", content = "<p>Go and Kafka.</p>" }));
        Services.AddSingleton(_toasts);
        Services.AddSingleton(new JobPostImporter(http, NullLogger<JobPostImporter>.Instance));
    }

    private IRenderedComponent<JobPostLink> Render() =>
        Render<JobPostLink>(p => p.Add(c => c.OnImport, (string text) => _imported = text));

    [Fact]
    public void Read_is_disabled_until_a_link_is_typed()
    {
        var cut = Render();
        Assert.True(cut.Find("#job-link-read").HasAttribute("disabled"));

        cut.Find("#job-link").Input(Link);

        Assert.False(cut.Find("#job-link-read").HasAttribute("disabled"));
    }

    [Fact]
    public void A_board_link_puts_the_post_in_the_editor()
    {
        var cut = Render();

        cut.Find("#job-link").Input(Link);
        cut.Find("#job-link-read").Click();

        cut.WaitForAssertion(() => Assert.Equal("Backend Engineer\n\nGo and Kafka.", _imported));
        Assert.Contains(_toasts.Toasts, t => t.Kind == ToastKind.Success && t.Text.Contains("Backend Engineer") && t.Text.Contains("Stripe"));
        Assert.Equal(string.Empty, cut.Find("#job-link").GetAttribute("value"));
    }

    // EDIT-09: the board's company name goes to the page, for the cover letter
    [Fact]
    public void The_company_the_board_names_goes_to_the_page()
    {
        string? company = null;
        var cut = Render<JobPostLink>(p => p
            .Add(c => c.OnImport, (string _) => { })
            .Add(c => c.OnCompany, (string name) => company = name));

        cut.Find("#job-link").Input(Link);
        cut.Find("#job-link-read").Click();

        cut.WaitForAssertion(() => Assert.Equal("Stripe", company));
    }

    [Fact]
    public void Enter_reads_the_link_too()
    {
        var cut = Render();

        cut.Find("#job-link").Input(Link);
        cut.Find("#job-link").KeyDown("Enter");

        cut.WaitForAssertion(() => Assert.NotNull(_imported));
    }

    [Fact]
    public void Another_link_shows_why_it_cannot_be_read()
    {
        var cut = Render();

        cut.Find("#job-link").Input("https://www.linkedin.com/jobs/view/123");
        cut.Find("#job-link-read").Click();

        cut.WaitForAssertion(() => Assert.Contains(_toasts.Toasts, t => t.Kind == ToastKind.Error && t.Text.StartsWith("Only Greenhouse and Lever")));
        Assert.Null(_imported);
    }
}

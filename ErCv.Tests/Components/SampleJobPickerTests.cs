using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.Features.Matching;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class SampleJobPickerTests : BunitContext
{
    private IRenderedComponent<SampleJobPicker> Render(string current) =>
        Render<SampleJobPicker>(p => p.Add(c => c.Current, current));

    [Fact]
    public void Shows_one_chip_per_sample()
    {
        var cut = Render("");

        Assert.Equal(SampleJobDescriptions.All.Select(s => s.Label), cut.FindAll("button.sample-chip").Select(b => b.TextContent.Trim()));
    }

    [Fact]
    public void The_active_sample_says_whose_post_it_is_and_links_to_the_original()
    {
        var sample = SampleJobDescriptions.All[0];

        var cut = Render(sample.Text);

        var source = cut.Find(".sample-picker-source");
        Assert.Contains(sample.Company, source.TextContent);
        Assert.Contains(sample.Source, source.TextContent);
        Assert.Contains(sample.PostedAt.ToString("d MMM yyyy", System.Globalization.CultureInfo.InvariantCulture), source.TextContent);
        var link = source.QuerySelector("a")!;
        Assert.Equal(sample.Url, link.GetAttribute("href"));
        Assert.Equal("noopener noreferrer", link.GetAttribute("rel"));
        Assert.Equal("true", cut.FindAll("button.sample-chip")[0].GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Without_an_active_sample_it_says_the_samples_are_real_posts()
    {
        var cut = Render("");

        Assert.Contains("real, recent post", cut.Find(".sample-picker-source").TextContent);
        Assert.Empty(cut.FindAll(".sample-picker-source a"));
    }

    [Fact]
    public void Hidden_once_the_user_types_their_own_job_description()
    {
        var cut = Render("My own job description");

        Assert.Empty(cut.FindAll(".sample-picker"));
    }
}

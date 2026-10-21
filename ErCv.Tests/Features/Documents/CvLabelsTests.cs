using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;

namespace ErCv.Tests.Features.Documents;

[Trait("Category", "Unit")]
public class CvLabelsTests
{
    [Theory]
    [InlineData("Jan 2020", "ene. 2020")]
    [InlineData("September 2019", "sept. 2019")]
    [InlineData("2021-03", "mar. 2021")]
    [InlineData("08/2022", "ago. 2022")]
    [InlineData("2018", "2018")]
    [InlineData("Present", "Actualidad")]
    [InlineData("current", "Actualidad")]
    [InlineData("Spring 2019", "Spring 2019")]
    [InlineData("", "")]
    public void Spanish_rewrites_the_dates_the_parser_knows_and_leaves_the_rest(string raw, string expected)
    {
        Assert.Equal(expected, CvLabels.Spanish.FormatDate(raw));
    }

    [Theory]
    [InlineData("Jan 2020")]
    [InlineData("2021-03")]
    [InlineData("Present")]
    [InlineData("Spring 2019")]
    public void English_keeps_the_profile_date_as_written(string raw)
    {
        Assert.Equal(raw, CvLabels.English.FormatDate(raw));
    }

    [Fact]
    public void A_date_range_leaves_out_a_missing_end()
    {
        Assert.Equal("ene. 2020 – Actualidad", CvLabels.Spanish.DateRange("Jan 2020", "Present"));
        Assert.Equal("Jan 2020", CvLabels.English.DateRange("Jan 2020", ""));
        Assert.Equal(string.Empty, CvLabels.English.DateRange(null, null));
    }

    [Fact]
    public void Unknown_languages_fall_back_to_english()
    {
        Assert.Same(CvLabels.English, CvLabels.For("fr"));
        Assert.Same(CvLabels.English, CvLabels.For(null));
        Assert.Same(CvLabels.Spanish, CvLabels.For(ApiConstants.LanguageSpanish));
    }

    [Fact]
    public void Both_languages_label_every_section()
    {
        foreach (var labels in new[] { CvLabels.English, CvLabels.Spanish })
        {
            Assert.All(
                new[] { labels.Summary, labels.Profile, labels.ExecutiveSummary, labels.Education, labels.Experience, labels.Skills, labels.Projects },
                l => Assert.False(string.IsNullOrWhiteSpace(l)));
        }
        Assert.Equal(12, CvLabels.Spanish.MonthNames!.Count);
    }
}

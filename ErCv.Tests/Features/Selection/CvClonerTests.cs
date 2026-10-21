using ErCv.Web.Features.Selection;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Selection;

[Trait("Category", "Unit")]
public class CvClonerTests
{
    [Fact]
    public void Clone_copies_every_value_and_shares_no_list_or_object()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "a", "b" }, skills: new[] { "go" }) }, skills: new[] { "go", "sql" });
        cv.Summary = "summary";
        cv.Education[0].RelevantCourses = new List<string> { "Algorithms" };
        cv.Projects = new List<Project> { new() { Name = "P", Technologies = new() { "go" }, Highlights = new() { "h" } } };

        var copy = CvCloner.Clone(cv);

        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(cv), System.Text.Json.JsonSerializer.Serialize(copy));
        Assert.NotSame(cv.Header, copy.Header);
        Assert.NotSame(cv.Education, copy.Education);
        Assert.NotSame(cv.Education[0].RelevantCourses, copy.Education[0].RelevantCourses);
        Assert.NotSame(cv.Experience[0], copy.Experience[0]);
        Assert.NotSame(cv.Experience[0].Bullets, copy.Experience[0].Bullets);
        Assert.NotSame(cv.Experience[0].Skills, copy.Experience[0].Skills);
        Assert.NotSame(cv.Skills, copy.Skills);
        Assert.NotSame(cv.Projects[0], copy.Projects[0]);
        Assert.NotSame(cv.Projects[0].Technologies, copy.Projects[0].Technologies);
    }

    [Fact]
    public void Editing_the_copy_does_not_change_the_original()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "a", "b" }) }, skills: new[] { "go" });

        var copy = CvCloner.Clone(cv);
        copy.Header.FullName = "CHANGED";
        copy.Experience[0].Bullets.Clear();
        copy.Skills.Clear();
        copy.Education.Clear();

        Assert.Equal("Test User", cv.Header.FullName);
        Assert.Equal(2, cv.Experience[0].Bullets.Count);
        Assert.Single(cv.Skills);
        Assert.Single(cv.Education);
    }

    [Fact]
    public void Clone_tolerates_null_collections()
    {
        var cv = new GeneratedCV { Header = null!, Education = null!, Experience = null!, Skills = null!, Projects = null! };

        var copy = CvCloner.Clone(cv);

        Assert.NotNull(copy.Header);
        Assert.Empty(copy.Experience);
        Assert.Empty(copy.Skills);
    }
}

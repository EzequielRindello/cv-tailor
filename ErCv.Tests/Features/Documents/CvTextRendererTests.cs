using ErCv.Tests.Documents;
using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Documents;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Documents;

[Trait("Category", "Unit")]
public class CvTextRendererTests
{
    private static GeneratedCV FullCv()
    {
        var cv = Generated(new[]
        {
            Exp(role: "Backend Engineer", company: "Acme", start: "Jan 2021", end: "Present",
                bullets: new[] { "Built the billing API in Go", "Cut p99 latency by 40%" }),
            Exp(role: "Developer", company: "Initech", start: "Mar 2018", end: "Dec 2020",
                bullets: new[] { "Maintained the C# monolith" })
        }, skills: new[] { "Go", "Kafka", "PostgreSQL" });

        cv.Header.Location = "Buenos Aires";
        cv.Header.LinkedIn = "linkedin.com/in/test";
        cv.Summary = "Backend engineer with seven years of experience.";
        cv.Education = new List<Education>
        {
            new() { Institution = "UTN", Location = "Buenos Aires", Degree = "B.S. Computer Science", GraduationDate = "2017", GPA = "9.1" }
        };
        cv.Experience[0].Location = "Remote";
        cv.Projects = new List<Project>
        {
            new() { Name = "Ledger", Link = "github.com/test/ledger", Description = "Double entry ledger.", Technologies = new() { "Go", "SQLite" } }
        };
        return cv;
    }

    [Fact]
    public void Plain_text_lists_every_section_in_document_order()
    {
        var text = CvTextRenderer.ToPlainText(FullCv());

        var order = new[]
        {
            "TEST USER", CvLabels.English.Summary.ToUpperInvariant(), CvLabels.English.Education.ToUpperInvariant(),
            CvLabels.English.Experience.ToUpperInvariant(), CvLabels.English.Skills.ToUpperInvariant(), CvLabels.English.Projects.ToUpperInvariant()
        };
        var cursor = 0;
        foreach (var item in order)
        {
            var at = text.IndexOf(item, cursor, StringComparison.Ordinal);
            Assert.True(at >= 0, $"'{item}' missing or out of order");
            cursor = at + item.Length;
        }
    }

    [Fact]
    public void Plain_text_has_the_contact_line_roles_dates_and_bullets()
    {
        var text = CvTextRenderer.ToPlainText(FullCv());

        Assert.Contains("t@x.com | +1 555 123 4567 | Buenos Aires | linkedin.com/in/test", text);
        Assert.Contains("Backend Engineer | Jan 2021 – Present", text);
        Assert.Contains("Acme, Remote", text);
        Assert.Contains("- Built the billing API in Go", text);
        Assert.Contains("UTN, Buenos Aires | 2017", text);
        Assert.Contains("B.S. Computer Science | GPA: 9.1", text);
        Assert.Contains("Go, Kafka, PostgreSQL", text);
        Assert.Contains("Ledger | github.com/test/ledger", text);
        Assert.Contains("Technologies: Go, SQLite", text);
    }

    [Fact]
    public void Plain_text_has_no_markdown_markers()
    {
        var text = CvTextRenderer.ToPlainText(FullCv());

        Assert.DoesNotContain("**", text);
        Assert.DoesNotContain("##", text);
        Assert.DoesNotContain("\\", text);
    }

    [Fact]
    public void Missing_fields_leave_no_stray_separators()
    {
        var cv = Generated(new[] { Exp(role: "Engineer", company: "Acme", start: "2020", end: "", bullets: new[] { "Did things" }) });
        cv.Header.Phone = "";

        var text = CvTextRenderer.ToPlainText(cv);

        Assert.Contains("t@x.com" + Environment.NewLine, text);
        Assert.Contains("Engineer | 2020" + Environment.NewLine, text);
        Assert.Contains("Acme" + Environment.NewLine, text);
        Assert.DoesNotContain("|  |", text);
        Assert.DoesNotContain(" | " + Environment.NewLine, text);
        Assert.DoesNotContain(", " + Environment.NewLine, text);
    }

    [Fact]
    public void Sections_without_content_are_left_out()
    {
        var cv = Generated(new[] { Exp(bullets: new[] { "One bullet" }) }, education: false);

        var text = CvTextRenderer.ToPlainText(cv);

        Assert.DoesNotContain(CvLabels.English.Summary.ToUpperInvariant(), text);
        Assert.DoesNotContain(CvLabels.English.Education.ToUpperInvariant(), text);
        Assert.DoesNotContain(CvLabels.English.Skills.ToUpperInvariant(), text);
        Assert.DoesNotContain(CvLabels.English.Projects.ToUpperInvariant(), text);
    }

    [Fact]
    public void Markdown_uses_headings_lists_and_emphasis()
    {
        var md = CvTextRenderer.ToMarkdown(FullCv());

        Assert.StartsWith("# Test User", md);
        Assert.Contains("## Professional Summary", md);
        Assert.Contains("## Experience", md);
        Assert.Contains("### Backend Engineer", md);
        Assert.Contains("*Acme, Remote* | Jan 2021 – Present", md);
        Assert.Contains("- Cut p99 latency by 40%", md);
        Assert.Contains("**UTN, Buenos Aires** | 2017  ", md);
        Assert.Contains("**Ledger** | github.com/test/ledger  ", md);
        Assert.Contains("*Technologies: Go, SQLite*", md);
    }

    [Fact]
    public void Markdown_escapes_characters_that_would_become_formatting()
    {
        var cv = Generated(new[] { Exp(role: "C# Developer", bullets: new[] { "Wrote *nix scripts_v2 in [bash]" }) });

        var md = CvTextRenderer.ToMarkdown(cv);

        Assert.Contains(@"### C\# Developer", md);
        Assert.Contains(@"- Wrote \*nix scripts\_v2 in \[bash\]", md);
    }

    // The text is only a different layout of the CV: every bullet and skill of every fixture CV is in it, verbatim.
    [Theory]
    [MemberData(nameof(FixtureLoader.Profiles), MemberType = typeof(FixtureLoader))]
    public void Plain_text_holds_every_bullet_and_skill_of_the_generated_cv(string profile)
    {
        foreach (var jd in FixtureLoader.AllJdNames)
        {
            var cv = DocumentSupport.Generate(profile, jd, twoPages: true);

            var text = CvTextRenderer.ToPlainText(cv);

            Assert.Contains(cv.Header.FullName.ToUpperInvariant(), text);
            Assert.All(cv.Experience.SelectMany(e => e.Bullets), b => Assert.Contains("- " + b, text));
            Assert.All(cv.Skills, s => Assert.Contains(s, text));
        }
    }
}

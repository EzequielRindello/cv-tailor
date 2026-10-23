using ErCv.Web.Features.Selection;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Features.Selection;

[Trait("Category", "Unit")]
public class SummaryGeneratorTests
{
    private readonly SummaryGenerator _gen = new();

    private string Summary(CVMaster cv, JobRequirements req, string[]? topSkills = null) =>
        _gen.GenerateSummary(cv, req, cv.Experience, (topSkills ?? Array.Empty<string>()).ToList());

    [Fact]
    public void Includes_candidate_seniority_not_jd_seniority()
    {
        var cv = Master(new[] { Exp(role: "Senior Backend Engineer", start: "Jan 2018") });

        var s = Summary(cv, Req(skills: new[] { "kafka" }, seniority: "junior"), new[] { "kafka" });

        Assert.StartsWith("Senior ", s);
        Assert.DoesNotContain("Junior", s);
    }

    [Fact]
    public void Years_of_experience_come_from_earliest_tech_role_start()
    {
        var cv = Master(new[]
        {
            Exp(role: "Software Engineer", start: $"Jan {Year(7)}"),
            Exp(role: "Warehouse Assistant", start: $"Jan {Year(20)}")
        });

        var s = Summary(cv, Req());

        Assert.Contains("with 7+ years of experience", s);
    }

    [Fact]
    public void Years_of_experience_do_not_count_gaps_between_roles()
    {
        var cv = Master(new[]
        {
            Exp(role: "Developer", start: "Jan 2010", end: "Jan 2013"),   // 3 years
            Exp(role: "Developer", start: "Jan 2020", end: "Jan 2022")    // 2 years, long gap before
        });

        Assert.Contains("with 5+ years of experience", Summary(cv, Req()));
    }

    [Fact]
    public void Years_of_experience_merge_overlapping_roles()
    {
        var cv = Master(new[]
        {
            Exp(role: "Developer", start: "Jan 2015", end: "Jan 2019"),
            Exp(role: "Engineer", start: "Jan 2017", end: "Jan 2020")
        });

        Assert.Contains("with 5+ years of experience", Summary(cv, Req()));
    }

    [Fact]
    public void Candidate_seniority_comes_from_the_most_recent_role_by_date()
    {
        // As strings "May 2020" sorts after "Dec 2023" and would pick the junior role (F3).
        var cv = Master(new[]
        {
            Exp(role: "Junior Developer", start: "Jan 2019", end: "May 2020"),
            Exp(role: "Senior Developer", start: "Jun 2020", end: "Dec 2023")
        });

        Assert.StartsWith("Senior ", Summary(cv, Req()));
    }

    // H10: the summary and the selector share one tech-role list, and a broad word like "specialist" is not on it
    [Fact]
    public void Years_of_experience_do_not_count_a_specialist_role_as_tech()
    {
        var cv = Master(new[]
        {
            Exp(role: "Software Engineer", start: $"Jan {Year(3)}"),
            Exp(role: "Certification Specialist", start: $"Jan {Year(6)}", end: $"Jan {Year(3)}")
        });

        Assert.Contains("with 3+ years of experience", Summary(cv, Req()));
    }

    [Fact]
    public void Specializing_in_continues_the_first_sentence()
    {
        var cv = Master(new[] { Exp(role: "Backend Engineer", start: $"Jan {Year(4)}") });

        var s = Summary(cv, Req(skills: new[] { "go" }), new[] { "go" });

        Assert.Contains("with 4+ years of experience, specializing in go.", s);
        Assert.DoesNotContain(". specializing", s);
    }

    [Fact]
    public void Falls_back_to_all_roles_when_none_is_tech()
    {
        var cv = Master(new[] { Exp(role: "Barista", start: $"Jan {Year(4)}") });

        Assert.Contains("4+ years", Summary(cv, Req()));
    }

    [Fact]
    public void Unparseable_start_dates_are_ignored_without_crashing()
    {
        var cv = Master(new[] { Exp(role: "Developer", start: "03/16"), Exp(role: "Developer", start: $"Jan {Year(3)}") });

        Assert.Contains("3+ years", Summary(cv, Req()));
    }

    [Theory]
    [InlineData(new[] { "spark", "airflow" }, "Data Engineer")]
    [InlineData(new[] { "terraform", "kubernetes" }, "DevOps Engineer")]
    [InlineData(new[] { "react", "css" }, "Frontend Developer")]
    [InlineData(new[] { "kafka", "postgresql" }, "Backend Developer")]
    [InlineData(new[] { "swift", "ios" }, "Mobile Developer")]
    [InlineData(new[] { "react", "css", "kafka", "postgresql" }, "Full Stack Developer")]
    [InlineData(new string[0], "Software Engineer")]
    public void Role_type_is_detected_from_jd_signals(string[] skills, string expected)
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        var s = Summary(cv, Req(skills: skills));

        Assert.StartsWith(expected, s);
    }

    // MATCH-10 (F29): the Airbnb Payments post says "a backend language such as Java, Kotlin"
    [Fact]
    public void A_single_mobile_signal_does_not_set_the_role()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        Assert.StartsWith("Software Engineer", Summary(cv, Req(skills: new[] { "kotlin", "java", "python" })));
    }

    [Theory]
    [InlineData("swift", "ios")]
    [InlineData("android", "kotlin")]
    public void A_real_mobile_post_still_reads_mobile(string first, string second)
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        Assert.StartsWith("Mobile Developer", Summary(cv, Req(skills: new[] { first, second })));
    }

    [Fact]
    public void Backend_signals_beat_one_stray_kotlin()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        var s = Summary(cv, Req(skills: new[] { "kotlin", "distributed systems" }, keywords: new[] { "backend" }));

        Assert.StartsWith("Backend Developer", s);
    }

    // MATCH-11: the Airbnb Payments shape. "backend" is repeated in the post but is not among the 30 most frequent words
    [Fact]
    public void A_role_name_the_post_repeats_is_a_signal_even_when_it_is_not_a_keyword()
    {
        var cv = Master(new[] { Exp(role: "Developer") });
        var text = "Software Engineer, Payments\n\nWe build backend services. Our backend language is Kotlin or Java.";

        var s = Summary(cv, Req(skills: new[] { "kotlin", "distributed systems" }, text: text));

        Assert.StartsWith("Backend Developer", s);
    }

    [Fact]
    public void A_role_name_mentioned_once_is_a_stray_mention()
    {
        var cv = Master(new[] { Exp(role: "Developer") });
        var text = "Software Engineer\n\nOur backend is Kotlin.";

        var s = Summary(cv, Req(skills: new[] { "kotlin", "distributed systems" }, text: text));

        Assert.StartsWith("Software Engineer", s);
    }

    [Fact]
    public void A_tie_falls_back_to_the_default_role()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        Assert.StartsWith("Software Engineer", Summary(cv, Req(skills: new[] { "spark", "airflow", "terraform", "kubernetes" })));
    }

    [Fact]
    public void Full_stack_needs_both_sides()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        Assert.StartsWith("Frontend Developer", Summary(cv, Req(skills: new[] { "react", "css", "html", "node.js" })));
    }

    [Theory]
    [InlineData("Senior Frontend Engineer", "Frontend Developer")]
    [InlineData("Staff Backend Engineer", "Backend Developer")]
    [InlineData("Senior Full-Stack Engineer", "Full Stack Developer")]
    [InlineData("Site Reliability Engineer", "DevOps Engineer")]
    public void The_title_decides_when_it_names_a_role(string title, string expected)
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        // The Coinbase shape: the body has two mobile signals, the title says otherwise
        var s = Summary(cv, Req(skills: new[] { "ios", "android" }, title: title));

        Assert.StartsWith(expected, s);
    }

    [Fact]
    public void Two_roles_in_the_title_pick_the_one_the_post_supports()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        var s = Summary(cv, Req(skills: new[] { "swift", "ios" }, title: "Backend / Mobile Engineer"));

        Assert.StartsWith("Mobile Developer", s);
    }

    // F29: "ui" (a frontend signal) used to match inside "build" and "guide", so a backend post read as full stack
    [Fact]
    public void Role_signals_match_whole_terms_only()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        var s = Summary(cv, Req(skills: new[] { "kafka", "postgresql" }, keywords: new[] { "build", "guide", "quickly" }));

        Assert.StartsWith("Backend Developer", s);
    }

    [Fact]
    public void Specializing_in_lists_jd_matching_top_skills()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        var s = Summary(cv, Req(skills: new[] { "go", "kafka" }), new[] { "go", "kafka", "git" });

        Assert.Contains("specializing in go and kafka", s);
    }

    [Fact]
    public void Specializing_in_falls_back_to_first_four_top_skills()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        var s = Summary(cv, Req(skills: new[] { "rust" }), new[] { "a1", "b2", "c3", "d4", "e5" });

        Assert.Contains("a1, b2, c3, and d4", s);
        Assert.DoesNotContain("e5", s);
    }

    [Fact]
    public void Top_achievement_percentage_is_quoted_when_present()
    {
        var cv = Master(new[]
        {
            Exp(role: "Developer", bullets: new[] { "Reduced build time by 30%", "Improving uptime to 99%" })
        });

        var s = Summary(cv, Req());

        Assert.Contains("Proven track record of", s);
        Assert.Contains("99%", s);
    }

    [Fact]
    public void Soft_skills_come_from_keywords_or_bullets_max_two()
    {
        var cv = Master(new[] { Exp(role: "Developer", bullets: new[] { "Led team standups and mentor juniors" }) });

        var s = Summary(cv, Req(keywords: new[] { "leadership", "communication" }));

        Assert.Contains("Strong ", s);
        Assert.Contains(" and ", s.Substring(s.IndexOf("Strong ", StringComparison.Ordinal)));
    }

    [Fact]
    public void Summary_is_capped_at_300_chars_with_ellipsis()
    {
        var cv = Master(new[] { Exp(role: "Developer", bullets: new[] { "Reduced costs by 50%" }) });
        var skills = Enumerable.Range(0, 5).Select(i => new string((char)('a' + i), 60)).ToArray();

        var s = Summary(cv, Req(skills: skills, keywords: new[] { "leadership" }), skills);

        Assert.Equal(300, s.Length);
        Assert.EndsWith("...", s);
    }

    [Fact]
    public void Ends_with_a_period_when_not_truncated()
    {
        var cv = Master(new[] { Exp(role: "Developer") });

        Assert.EndsWith(".", Summary(cv, Req()));
    }
}

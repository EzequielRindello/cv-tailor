using ErCv.Collector;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Collector;

[Trait("Category", "Unit")]
public class CollectorPartsTests
{
    [Theory]
    [InlineData("Senior Backend Engineer", "backend", "senior")]
    [InlineData("Staff Frontend Engineer", "frontend", "staff")]
    [InlineData("Full-Stack Engineer (Backend Leaning)", "fullstack", "mid")]
    [InlineData("Senior iOS Engineer, Crypto", "mobile", "senior")]
    [InlineData("Data Scientist, Intern", "data", "intern")]
    [InlineData("Site Reliability Engineer", "devops", "mid")]
    [InlineData("Staff Machine Learning Engineer, Radar", "ml", "staff")]
    [InlineData("Software Engineer, New Grad 2027", "software", "junior")]
    [InlineData("Principal Software Engineer I - Platform Billing", "devops", "principal")]
    [InlineData("Software Engineer I", "software", "junior")]
    [InlineData("Software Engineer III", "software", "senior")]
    [InlineData("Associate QA Engineer", "qa", "junior")]
    [InlineData("Sr. Security Engineer", "security", "senior")]
    public void Classifies_software_titles(string title, string family, string seniority)
    {
        Assert.Equal(family, Classifier.RoleFamily(title));
        Assert.Equal(seniority, Classifier.Seniority(title));
    }

    [Theory]
    [InlineData("Senior Backend Engineer", true)]
    [InlineData("Software Engineer, New Grad 2027", true)]
    [InlineData("Sr. Security Engineer", true)]
    [InlineData("Associate DevOps Engineer", false)]
    [InlineData("Software Engineer 3, Data Pipeline", false)]
    [InlineData("Member of Technical Staff", false)]
    public void A_clear_level_is_a_level_word_that_leaves_no_doubt(string title, bool expected)
    {
        Assert.Equal(expected, Classifier.NamesClearLevel(title));
    }

    // APP-06: an accelerator post with more skills used to win the Full stack sample
    [Fact]
    public void Sample_picker_skips_programs_and_prefers_a_clear_level()
    {
        static Posting Post(string id, string title, string text) => new()
        {
            Id = id, Source = "greenhouse", Url = "https://example.com/" + id, Company = "Acme", Title = title,
            PostedAt = DateTimeOffset.Parse("2026-10-01T00:00:00Z"), RoleFamily = "fullstack", Seniority = Classifier.Seniority(title), Text = text
        };
        var postings = new[]
        {
            Post("program", "Accelerator Program - Software Engineer (Full-stack)", "React, Node.js, Kafka, Redis, PostgreSQL and AWS."),
            Post("plain", "Full Stack Engineer", "React, Node.js and PostgreSQL."),
            Post("clear", "Senior Full Stack Engineer", "React.")
        };

        var sample = SamplePicker.Pick(postings, null, out _).Single(s => s.Id == "fullstack");

        Assert.Equal("clear", sample.PostingId);
    }

    // F27: the demo starts with the senior sample profile, so a Staff post was a seniority mismatch on first try
    [Fact]
    public void Sample_picker_prefers_the_demo_profiles_level_over_another_clear_level()
    {
        static Posting Post(string id, string title, string text) => new()
        {
            Id = id, Source = "greenhouse", Url = "https://example.com/" + id, Company = "Acme", Title = title,
            PostedAt = DateTimeOffset.Parse("2026-10-01T00:00:00Z"), RoleFamily = "backend", Seniority = Classifier.Seniority(title), Text = text
        };
        var postings = new[]
        {
            Post("staff", "Staff Backend Engineer", "Go, Kafka, Redis, PostgreSQL, Kubernetes and AWS."),
            Post("senior", "Senior Backend Engineer", "Go and PostgreSQL."),
            Post("plain", "Backend Engineer", "Go, Kafka, Redis, PostgreSQL, Kubernetes, AWS and Terraform.")
        };

        var sample = SamplePicker.Pick(postings, null, out _).Single(s => s.Id == "backend");

        Assert.Equal("senior", sample.PostingId);
    }

    // F27: among the posts at the demo's level, the one the demo profile fits best, not the one with the most skills
    [Fact]
    public void Sample_picker_takes_the_post_the_demo_profile_matches_best()
    {
        static Posting Post(string id, string text) => new()
        {
            Id = id, Source = "greenhouse", Url = "https://example.com/" + id, Company = "Acme", Title = "Senior Backend Engineer",
            PostedAt = DateTimeOffset.Parse("2026-10-01T00:00:00Z"), RoleFamily = "backend", Seniority = "senior", Text = text
        };
        var postings = new[]
        {
            Post("trivial", "Go."),
            Post("many-skills", "Rust, Elixir, Haskell, Scala, Erlang, OCaml and Clojure."),
            Post("fits", "Go, Kafka, PostgreSQL, Redis, Docker and Kubernetes.")
        };
        var profile = Master(new[] { Exp(role: "Senior Backend Engineer") },
            languages: new[] { "Go" }, tools: new[] { "Kafka", "PostgreSQL", "Redis", "Docker", "Kubernetes" });

        var sample = SamplePicker.Pick(postings, profile, out _).Single(s => s.Id == "backend");

        // "trivial" is a perfect match on its one skill, too few to make a sample
        Assert.Equal("fits", sample.PostingId);
    }

    [Theory]
    [InlineData("Account Executive - France")]
    [InlineData("Engineering Manager, Payments")]
    [InlineData("Solutions Engineer")]
    [InlineData("Product Designer")]
    [InlineData("Staff Chassis Engineer, Steering and Braking Actuation")]
    [InlineData("Technical Escalations Engineer II")]
    [InlineData("Wild Card")]
    public void Leaves_out_roles_that_are_not_individual_software_work(string title)
    {
        Assert.Null(Classifier.RoleFamily(title));
    }

    [Fact]
    public void Html_becomes_paragraphs_and_bullets_without_markup()
    {
        var text = HtmlText.ToPlain("<h3>About</h3><p>We&nbsp;build &amp; ship.</p><ul><li><p>Go</p></li><li>Kafka</li></ul><script>x()</script><p>Bye</p>");

        Assert.Equal("About\n\nWe build & ship.\n\n- Go\n- Kafka\n\nBye", text);
    }

    [Fact]
    public void Em_dashes_in_post_text_become_hyphens()
    {
        Assert.Equal("Lead AI Engineer - Enterprise", HtmlText.Normalize("Lead AI Engineer \u2014 Enterprise"));
    }

    [Fact]
    public void Truncate_cuts_at_a_paragraph_boundary()
    {
        var text = new string('a', 60) + "\n\n" + new string('b', 60);

        Assert.Equal(new string('a', 60), HtmlText.Truncate(text, 100));
        Assert.Equal(text, HtmlText.Truncate(text, 500));
    }

    [Fact]
    public void Robots_group_for_everyone_applies_with_longest_match_and_crawl_delay()
    {
        var rules = RobotsRules.Parse("User-agent: Googlebot\nDisallow: /\n\nUser-agent: *\nDisallow: /embed/\nAllow: /embed/ok\nCrawl-delay: 1.5\n", PoliteHttp.UserAgentProduct);

        Assert.True(rules.Allows("/v1/boards/acme/jobs?content=true"));
        Assert.False(rules.Allows("/embed/job_board"));
        Assert.True(rules.Allows("/embed/ok"));
        Assert.Equal(TimeSpan.FromSeconds(1.5), rules.CrawlDelay);
    }

    [Fact]
    public void Robots_group_naming_the_collector_wins_over_everyone()
    {
        var rules = RobotsRules.Parse("User-agent: *\nAllow: /\n\nUser-agent: ErCvCorpusCollector\nDisallow: /api/\n", PoliteHttp.UserAgentProduct);

        Assert.False(rules.Allows("/api/v1/x"));
        Assert.True(rules.Allows("/careers"));
    }

    [Fact]
    public void Posting_ids_are_stable_and_safe_as_file_names()
    {
        Assert.Equal("lever-globex-6d2c2e8e-1211", CorpusStore.MakeId("lever", "Globex", "6d2c2e8e-1211"));
        Assert.Equal("workable-a-b-w1", CorpusStore.MakeId("workable", "a.b", "W1"));
    }
}

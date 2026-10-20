using ErCv.Collector;

namespace ErCv.Tests.Collector
{
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
}

using ErCvApi.Helpers;
using ErCvApi.Models;
using static ErCv.Tests.Helpers.Build;

namespace ErCv.Tests.Helpers
{
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
        [InlineData(new[] { "react", "kafka" }, "Full Stack Developer")]
        [InlineData(new string[0], "Software Engineer")]
        public void Role_type_is_detected_from_jd_signals(string[] skills, string expected)
        {
            var cv = Master(new[] { Exp(role: "Developer") });

            var s = Summary(cv, Req(skills: skills));

            Assert.StartsWith(expected, s);
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
}

using ErCv.Tests.Fixtures;
using ErCvApi.Helpers;
using ErCvApi.Models;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class ProfileHealthCheckerTests
    {
        private static Experience Exp(string start = "Jan 2020", string end = "Dec 2020", string[]? bullets = null, string[]? skills = null) => new()
        {
            Role = "Engineer", Company = "Acme", StartDate = start, EndDate = end,
            Bullets = (bullets ?? new[] { "Reduced latency by 30%" }).ToList(),
            Skills = (skills ?? new[] { "C#" }).ToList()
        };

        private static CVMaster Profile(params Experience[] experience) => new() { Experience = experience.ToList() };

        private static HealthFinding[] Of(HealthRule rule, params Experience[] experience) =>
            ProfileHealthChecker.Check(Profile(experience)).Where(f => f.Rule == rule).ToArray();

        [Fact]
        public void A_strong_profile_has_no_findings()
        {
            Assert.Empty(ProfileHealthChecker.Check(Profile(Exp())));
        }

        [Fact]
        public void Null_or_empty_profile_has_no_findings()
        {
            Assert.Empty(ProfileHealthChecker.Check(null));
            Assert.Empty(ProfileHealthChecker.Check(new CVMaster()));
        }

        [Theory]
        [InlineData("Built internal tools", true)]
        [InlineData("Cut costs by 30%", false)]
        [InlineData("Served 2M requests a day", false)]
        [InlineData("Handled 10,000 tickets", false)]
        public void Bullets_without_a_metric_are_flagged(string bullet, bool flagged)
        {
            var found = Of(HealthRule.NoMetrics, Exp(bullets: new[] { bullet }));

            Assert.Equal(flagged, found.Length == 1);
            if (flagged) Assert.Equal("experience[0].bullets[0]", found[0].Path);
        }

        [Fact]
        public void Empty_bullets_are_left_to_the_validator()
        {
            Assert.Empty(Of(HealthRule.NoMetrics, Exp(bullets: new[] { "" })));
        }

        [Theory]
        [InlineData("Responsible for the billing API (40%)", true)]
        [InlineData("worked on search, 20% faster", true)]
        [InlineData("- Helped the team ship 3x faster", true)]
        [InlineData("Was part of a team that shipped 3 apps", true)]
        [InlineData("Built the billing API (40% faster)", false)]
        [InlineData("Workflow automation cut time by 20%", false)]
        public void Weak_openers_are_flagged_by_the_first_word_only(string bullet, bool flagged)
        {
            Assert.Equal(flagged, Of(HealthRule.WeakVerb, Exp(bullets: new[] { bullet })).Length == 1);
        }

        [Fact]
        public void An_experience_without_skill_tags_is_flagged()
        {
            Assert.Single(Of(HealthRule.NoSkillTags, Exp(skills: Array.Empty<string>())));
            Assert.Single(Of(HealthRule.NoSkillTags, Exp(skills: new[] { " " })));
            Assert.Empty(Of(HealthRule.NoSkillTags, Exp()));
        }

        [Theory]
        [InlineData("soon", "Dec 2020", "experience[0].startDate")]
        [InlineData("Jan 2020", "later", "experience[0].endDate")]
        [InlineData("13/2020", "Dec 2020", "experience[0].startDate")]
        public void Unreadable_dates_are_flagged(string start, string end, string path)
        {
            var found = Of(HealthRule.Dates, Exp(start, end));

            Assert.Equal(path, Assert.Single(found).Path);
        }

        [Fact]
        public void An_end_before_the_start_is_flagged()
        {
            var finding = Assert.Single(Of(HealthRule.Dates, Exp("Jun 2021", "Jan 2020")));

            Assert.Contains("ends before it starts", finding.Message);
        }

        [Fact]
        public void Overlapping_roles_are_flagged_once()
        {
            var found = Of(HealthRule.Dates, Exp("Jan 2020", "Dec 2021"), Exp("Jun 2021", "Present"));

            var finding = Assert.Single(found);
            Assert.Equal("experience[1].startDate", finding.Path);
            Assert.Contains("overlap", finding.Message);
        }

        [Fact]
        public void A_one_month_hand_over_is_not_an_overlap()
        {
            Assert.Empty(Of(HealthRule.Dates, Exp("Jan 2020", "Jun 2021"), Exp("Jun 2021", "Present")));
        }

        [Fact]
        public void Back_to_back_roles_do_not_overlap()
        {
            Assert.Empty(Of(HealthRule.Dates, Exp("Jan 2020", "Jun 2021"), Exp("Jul 2021", "Present")));
        }

        [Fact]
        public void A_blank_end_date_means_ongoing()
        {
            Assert.Single(Of(HealthRule.Dates, Exp("Jan 2020", ""), Exp("Jan 2021", "Dec 2021")));
        }

        [Fact]
        public void The_check_does_not_change_the_profile()
        {
            var profile = FixtureLoader.LoadProfile("cv-master");
            var before = ProfileJson.Serialize(profile);

            ProfileHealthChecker.Check(profile);

            Assert.Equal(before, ProfileJson.Serialize(profile));
        }
    }
}

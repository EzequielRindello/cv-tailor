using ErCv.Tests.Fixtures;
using ErCvApi.Helpers;
using ErCvApi.Models;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class ProfileValidatorTests
    {
        private static readonly ProfileValidator Validator = new();

        private static CVMaster ValidProfile() => new()
        {
            Header = new Header { FullName = "Ada Lovelace", Email = "ada@example.com" },
            Experience = new List<Experience>
            {
                new()
                {
                    Role = "Engineer", Company = "Analytical Engines", StartDate = "Jan 2020", EndDate = "Present",
                    Bullets = new List<string> { "Built a thing" }, Skills = new List<string> { "C#" }
                }
            }
        };

        private static string[] Paths(CVMaster profile) =>
            Validator.Validate(profile).Issues.Select(i => i.Path).ToArray();

        [Theory]
        [InlineData("cv-master")]
        [InlineData("cv-master-senior")]
        [InlineData("cv-master-junior")]
        public void Shipped_profiles_are_valid(string id)
        {
            Assert.True(Validator.Validate(FixtureLoader.LoadProfile(id)).IsValid);
        }

        [Fact]
        public void A_minimal_profile_is_valid()
        {
            Assert.True(Validator.Validate(ValidProfile()).IsValid);
        }

        [Fact]
        public void Null_profile_is_reported_instead_of_throwing()
        {
            Assert.Equal(new[] { "profile" }, Validator.Validate(null).Issues.Select(i => i.Path));
        }

        [Fact]
        public void Name_and_email_are_required()
        {
            var p = ValidProfile();
            p.Header.FullName = " ";
            p.Header.Email = "";

            Assert.Equal(new[] { "header.fullName", "header.email" }, Paths(p));
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("a@b")]
        [InlineData("a b@c.com")]
        public void Malformed_email_is_rejected(string email)
        {
            var p = ValidProfile();
            p.Header.Email = email;

            Assert.Equal(new[] { "header.email" }, Paths(p));
        }

        [Fact]
        public void At_least_one_experience_is_required()
        {
            var p = ValidProfile();
            p.Experience.Clear();

            Assert.Equal(new[] { "experience" }, Paths(p));
        }

        [Fact]
        public void Role_company_and_start_date_are_required_per_experience()
        {
            var p = ValidProfile();
            p.Experience.Add(new Experience());

            Assert.Equal(
                new[] { "experience[1].role", "experience[1].company", "experience[1].startDate" },
                Paths(p));
        }

        [Theory]
        [InlineData("Jun 2024")]
        [InlineData("June 2024")]
        [InlineData("2024-06")]
        [InlineData("06/2024")]
        [InlineData("2024")]
        public void Every_supported_date_format_is_accepted(string start)
        {
            var p = ValidProfile();
            p.Experience[0].StartDate = start;
            p.Experience[0].EndDate = "Present";

            Assert.True(Validator.Validate(p).IsValid);
        }

        [Theory]
        [InlineData("yesterday")]
        [InlineData("13/2024")]
        [InlineData("soon")]
        public void Unreadable_start_date_is_rejected(string start)
        {
            var p = ValidProfile();
            p.Experience[0].StartDate = start;

            Assert.Equal(new[] { "experience[0].startDate" }, Paths(p));
        }

        [Fact]
        public void Present_is_not_a_start_date()
        {
            var p = ValidProfile();
            p.Experience[0].StartDate = "Present";

            Assert.Equal(new[] { "experience[0].startDate" }, Paths(p));
        }

        [Fact]
        public void Empty_end_date_means_ongoing_and_is_valid()
        {
            var p = ValidProfile();
            p.Experience[0].EndDate = "";

            Assert.True(Validator.Validate(p).IsValid);
        }

        [Fact]
        public void Unreadable_end_date_is_rejected()
        {
            var p = ValidProfile();
            p.Experience[0].EndDate = "someday";

            Assert.Equal(new[] { "experience[0].endDate" }, Paths(p));
        }

        [Fact]
        public void End_before_start_is_rejected()
        {
            var p = ValidProfile();
            p.Experience[0].StartDate = "Dec 2023";
            p.Experience[0].EndDate = "May 2020";

            Assert.Equal(new[] { "experience[0].endDate" }, Paths(p));
        }

        [Fact]
        public void Same_month_start_and_end_is_valid()
        {
            var p = ValidProfile();
            p.Experience[0].StartDate = "May 2020";
            p.Experience[0].EndDate = "May 2020";

            Assert.True(Validator.Validate(p).IsValid);
        }

        [Fact]
        public void Empty_bullets_and_skills_are_reported_with_their_index()
        {
            var p = ValidProfile();
            p.Experience[0].Bullets = new List<string> { "ok", "  ", "" };
            p.Experience[0].Skills = new List<string> { "" };

            Assert.Equal(
                new[] { "experience[0].bullets[1]", "experience[0].bullets[2]", "experience[0].skills[0]" },
                Paths(p));
        }

        [Fact]
        public void Education_needs_degree_and_institution_and_a_readable_date()
        {
            var p = ValidProfile();
            p.Education.Add(new Education { GraduationDate = "never" });

            Assert.Equal(
                new[] { "education[0].degree", "education[0].institution", "education[0].graduationDate" },
                Paths(p));
        }

        [Fact]
        public void Education_date_is_optional()
        {
            var p = ValidProfile();
            p.Education.Add(new Education { Degree = "B.S.", Institution = "MIT" });

            Assert.True(Validator.Validate(p).IsValid);
        }

        [Fact]
        public void Projects_need_a_name_and_no_empty_highlights()
        {
            var p = ValidProfile();
            p.Projects.Add(new Project { Highlights = new List<string> { "" } });

            Assert.Equal(new[] { "projects[0].name", "projects[0].highlights[0]" }, Paths(p));
        }

        [Fact]
        public void The_edge_profile_is_reported_for_its_missing_dates()
        {
            var issues = Validator.Validate(FixtureLoader.LoadProfile("cv-master-edge")).Issues;

            Assert.Contains(issues, i => i.Path == "experience[2].startDate");
        }

        [Fact]
        public void Validation_does_not_change_the_profile()
        {
            var p = ValidProfile();
            p.Experience[0].Bullets = new List<string> { "", "kept" };
            var before = System.Text.Json.JsonSerializer.Serialize(p);

            Validator.Validate(p);

            Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(p));
        }
    }
}

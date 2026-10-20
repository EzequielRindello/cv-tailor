using ErCv.Tests.Fixtures;

namespace ErCv.Tests
{
    [Trait("Category", "Unit")]
    public class FixtureSmokeTests
    {
        public static TheoryData<string> Profiles => FixtureLoader.Profiles();
        public static TheoryData<string> Jds => FixtureLoader.Jds();

        [Theory]
        [MemberData(nameof(Profiles))]
        public void Profile_deserializes_with_header_name(string id)
        {
            var cv = FixtureLoader.LoadProfile(id);

            Assert.False(string.IsNullOrWhiteSpace(cv.Header.FullName));
            Assert.NotEmpty(cv.Experience);
        }

        [Fact]
        public void Edge_profile_has_no_projects_and_odd_dates()
        {
            var cv = FixtureLoader.LoadProfile("cv-master-edge");

            Assert.Empty(cv.Projects);
            Assert.Contains(cv.Experience, e => e.StartDate == "03/2016");
            Assert.Contains(cv.Experience, e => string.IsNullOrEmpty(e.EndDate));
        }

        [Theory]
        [MemberData(nameof(Jds))]
        public void Jd_file_loads(string name)
        {
            var jd = FixtureLoader.LoadJd(name);

            if (name == "empty") Assert.Equal(string.Empty, jd);
            else Assert.False(string.IsNullOrWhiteSpace(jd));
        }

        [Fact]
        public void Very_long_jd_is_at_least_50k_chars()
        {
            Assert.True(FixtureLoader.LoadJd("very-long").Length >= 50_000);
        }
    }
}

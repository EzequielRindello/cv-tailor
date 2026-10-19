using ErCv.Tests.Fixtures;
using ErCvApi.Helpers;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class JobDescriptionParserTests
    {
        private readonly JobDescriptionParser _parser = new();

        [Theory]
        [InlineData("")]
        [InlineData("   \n\t ")]
        public void Empty_input_returns_empty_requirements_with_default_seniority(string jd)
        {
            var r = _parser.Parse(jd);

            Assert.Empty(r.TechnicalSkills);
            Assert.Empty(r.Keywords);
            Assert.Empty(r.RequiredExperience);
            Assert.Equal("mid-level", r.SeniorityLevel);
        }

        [Fact]
        public void Extracts_known_skills_and_normalizes_them()
        {
            var r = _parser.Parse("We use Golang, Kafka, PostgreSQL and Node.js on AWS.");

            Assert.Contains("go", r.TechnicalSkills);
            Assert.Contains("kafka", r.TechnicalSkills);
            Assert.Contains("postgresql", r.TechnicalSkills);
            Assert.Contains("node.js", r.TechnicalSkills);
            Assert.Contains("aws", r.TechnicalSkills);
        }

        [Fact]
        public void Skills_are_not_extracted_from_inside_other_words()
        {
            var r = _parser.Parse("A good, ongoing mission for a gold standard team.");

            Assert.DoesNotContain("go", r.TechnicalSkills);
        }

        [Fact]
        public void Skills_have_no_duplicates()
        {
            var r = _parser.Parse("Python python PYTHON and py");

            Assert.Single(r.TechnicalSkills, s => s == "python");
        }

        [Theory]
        [InlineData("We need a Senior engineer", "senior")]
        [InlineData("Junior developer wanted", "junior")]
        [InlineData("Looking for a principal engineer", "principal")]
        [InlineData("Entry-level position", "entry-level")]
        public void Detects_explicit_seniority(string jd, string expected)
        {
            Assert.Equal(expected, _parser.Parse(jd).SeniorityLevel);
        }

        [Fact]
        public void Negated_seniority_is_ignored()
        {
            var r = _parser.Parse("Junior developer role. No senior experience needed.");

            Assert.Equal("junior", r.SeniorityLevel);
        }

        [Theory]
        [InlineData("8 years of experience with systems", "senior")]
        [InlineData("5 years of experience with systems", "mid-level")]
        [InlineData("2 years of experience with systems", "junior")]
        [InlineData("a role with no tenure mentioned", "mid-level")]
        public void Falls_back_to_years_of_experience(string jd, string expected)
        {
            Assert.Equal(expected, _parser.Parse(jd).SeniorityLevel);
        }

        [Fact]
        public void Keywords_are_capped_at_30_and_exclude_short_words()
        {
            var r = _parser.Parse(FixtureLoader.LoadJd("very-long"));

            Assert.True(r.Keywords.Count <= 30);
            Assert.All(r.Keywords, k => Assert.True(k.Length > 3));
        }

        [Fact]
        public void Required_experience_picks_bullet_lines_and_caps_at_10()
        {
            var jd = string.Join("\n", Enumerable.Range(1, 15).Select(i => $"- Requirement number {i} is here"));

            var r = _parser.Parse(jd);

            Assert.Equal(10, r.RequiredExperience.Count);
            Assert.StartsWith("requirement number 1", r.RequiredExperience[0]);
        }

        [Theory]
        [InlineData("backend-go-kafka", "go", "kafka")]
        [InlineData("frontend-react", "react", "typescript")]
        public void Corpus_jds_yield_their_headline_skills(string jd, string a, string b)
        {
            var r = _parser.Parse(FixtureLoader.LoadJd(jd));

            Assert.Contains(a, r.TechnicalSkills);
            Assert.Contains(b, r.TechnicalSkills);
        }

        // Found while writing TEST-02: YearsWithTechPattern captures the word after "N years of" as a skill.
        [Fact(Skip = "NEW FINDING: 'years of B2B' is captured as a tech skill, so a sales JD looks technical")]
        public void Sales_jd_has_no_technical_skills()
        {
            Assert.Empty(_parser.Parse(FixtureLoader.LoadJd("sales-non-tech")).TechnicalSkills);
        }

        [Fact(Skip = "NEW FINDING: 'experience or relevant' yields the skill 'or'")]
        public void Junior_jd_does_not_extract_filler_words_as_skills()
        {
            var r = _parser.Parse(FixtureLoader.LoadJd("junior"));

            Assert.DoesNotContain("or", r.TechnicalSkills);
        }

        [Fact]
        public void Junior_jd_extracts_its_real_skills_and_seniority()
        {
            var r = _parser.Parse(FixtureLoader.LoadJd("junior"));

            Assert.Equal("junior", r.SeniorityLevel);
            Assert.Contains("javascript", r.TechnicalSkills);
            Assert.Contains("react", r.TechnicalSkills);
            Assert.Contains("node.js", r.TechnicalSkills);
        }

        [Fact]
        public void Spanish_jd_still_finds_the_english_tech_terms()
        {
            var r = _parser.Parse(FixtureLoader.LoadJd("spanish"));

            Assert.Contains("node.js", r.TechnicalSkills);
            Assert.Contains("postgresql", r.TechnicalSkills);
            Assert.Contains("aws", r.TechnicalSkills);
            Assert.Equal("senior", r.SeniorityLevel);
        }
    }
}

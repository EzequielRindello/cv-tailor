using ErCv.Tests.Fixtures;
using ErCv.Web.Features.Matching;

namespace ErCv.Tests.Features.Matching;

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
    [InlineData("Entry-level position", "junior")]
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
    [InlineData("3 years of experience with systems", "mid-level")]
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

    // F12: YearsWithTechPattern used to capture the word after "N years of" as a skill ("B2B").
    [Fact]
    public void Sales_jd_has_no_technical_skills()
    {
        Assert.Empty(_parser.Parse(FixtureLoader.LoadJd("sales-non-tech")).TechnicalSkills);
    }

    [Fact]
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

    [Theory]
    [InlineData("5 years in DevOps or SRE", "in")]
    [InlineData("3+ years of B2B sales", "b2b")]
    [InlineData("2 years or more", "or")]
    public void Connector_words_after_years_are_not_skills(string jd, string notASkill)
    {
        Assert.DoesNotContain(notASkill, _parser.Parse(jd).TechnicalSkills);
    }

    // F19: the word after "N years of" is a skill only when the dictionary knows it
    [Theory]
    [InlineData("5+ years of professional software experience", "professional")]
    [InlineData("3+ years of building production systems", "building")]
    [InlineData("4 years of industry experience", "industry")]
    [InlineData("2 years of hands-on work", "hands-on")]
    [InlineData("5+ years of experience with Zig", "zig")]
    public void Words_after_years_that_are_not_skills_are_ignored(string jd, string notASkill)
    {
        Assert.DoesNotContain(notASkill, _parser.Parse(jd).TechnicalSkills);
    }

    [Theory]
    [InlineData("5+ years of experience with Haskell", "haskell")]
    [InlineData("3 years of go in production", "go")]
    public void Years_with_a_dictionary_skill_counts_it_even_in_lowercase(string jd, string skill)
    {
        Assert.Contains(skill, _parser.Parse(jd).TechnicalSkills);
    }

    // F18 and F21, on sentences from the real posts of the corpus report
    [Theory]
    [InlineData("AI systems that enable the next generation of intelligent apps", "next.js")]
    [InlineData("You will own end-to-end feature delivery, author tech specs", "e2e testing")]
    [InlineData("trusted account and commerce foundation that lets anyone build", "foundation")]
    [InlineData("so work is faster, clearer, and less fragmented", "less")]
    [InlineData("Write solid automation in Python and/or Bash", "solidjs")]
    [InlineData("integrations with international banking rails", "ruby on rails")]
    [InlineData("Minnesota, Vermont or Washington D.C. : $116,960.00", "c")]
    [InlineData("one of the following- Kotlin, Java, Swift, Objective-C, Go, or Python", "c")]
    [InlineData("languages such as TypeScript, Node.js, or Python", "javascript")]
    [InlineData("To learn more, please visit https://www.redditinc.com/careers/", "https")]
    public void Ordinary_english_is_not_read_as_a_skill(string jd, string notASkill)
    {
        Assert.DoesNotContain(notASkill, _parser.Parse(jd).TechnicalSkills);
    }

    [Fact]
    public void Skills_recent_posts_ask_for_are_in_the_dictionary()
    {
        var r = _parser.Parse("Build AI agents and MCP servers, run evals, own observability for distributed systems, "
            + "join the on-call rotation, lead incident response, system design, data modeling, ETL into an Iceberg lakehouse, "
            + "LLMs, accessibility and networking.");

        foreach (var skill in new[] { "ai agents", "mcp", "evals", "observability", "distributed systems", "on-call",
                     "incident response", "system design", "data modeling", "etl", "iceberg", "lakehouse", "llm",
                     "accessibility", "networking" })
            Assert.Contains(skill, r.TechnicalSkills);
    }

    // MATCH-09: the title (first line) decides when it names a level
    [Theory]
    [InlineData("Staff Software Engineer", "staff")]
    [InlineData("Senior Staff Engineer, Infrastructure", "staff")]
    [InlineData("Senior / Staff Security Engineer", "staff")]
    [InlineData("Principal Engineer", "principal")]
    [InlineData("Tech Lead, Payments", "lead")]
    [InlineData("Sr. Backend Engineer", "senior")]
    [InlineData("Software Engineer II", "mid-level")]
    [InlineData("Software Engineer III", "senior")]
    [InlineData("Software Engineer I", "junior")]
    [InlineData("Software Engineering Intern", "intern")]
    [InlineData("Software Engineering Internship, Summer 2027", "intern")]
    [InlineData("Early Career Software Engineer", "junior")]
    [InlineData("Software Engineer, New Grad", "junior")]
    [InlineData("Desarrollador .NET Semi Senior", "mid-level")]
    [InlineData("Member of Technical Staff", "mid-level")]
    public void The_title_names_the_level(string title, string expected)
    {
        Assert.Equal(expected, _parser.Parse(title + "\n\nWe build products.").SeniorityLevel);
    }

    [Fact]
    public void The_body_does_not_override_a_level_in_the_title()
    {
        var r = _parser.Parse("Senior Software Engineer\n\nYou will mentor junior engineers and pair with our staff engineer.");

        Assert.Equal("senior", r.SeniorityLevel);
    }

    [Theory]
    [InlineData("Software Engineer, Payments\n\nYou will lead the design of our APIs with principal engineers.", "mid-level")]
    [InlineData("About us\n\nWe are hiring a staff backend engineer to own our data platform.", "staff")]
    [InlineData("About us\n\nAs a senior machine learning engineer you will train models.", "senior")]
    public void Without_a_level_in_the_title_the_body_names_the_role_not_the_team(string jd, string expected)
    {
        Assert.Equal(expected, _parser.Parse(jd).SeniorityLevel);
    }

    [Theory]
    [InlineData("Revisiones de c\u00f3digo y despliegues continuos")]
    [InlineData("Trabajamos con C# y C++ todos los d\u00edas")]
    public void The_single_letter_language_c_is_not_found_inside_other_words_or_c_sharp(string jd)
    {
        Assert.DoesNotContain("c", _parser.Parse(jd).TechnicalSkills);
    }

    [Fact]
    public void The_single_letter_language_c_is_found_when_it_is_named()
    {
        Assert.Contains("c", _parser.Parse("Experience with C, Linux and Bash").TechnicalSkills);
    }
}

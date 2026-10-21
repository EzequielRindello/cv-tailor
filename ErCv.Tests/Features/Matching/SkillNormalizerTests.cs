using ErCv.Web.Features.Matching;

namespace ErCv.Tests.Features.Matching;

[Trait("Category", "Unit")]
public class SkillNormalizerTests
{
    [Theory]
    [InlineData("dotnet", ".net")]
    [InlineData("CSharp", "c#")]
    [InlineData("JS", "javascript")]
    [InlineData("ReactJS", "react")]
    [InlineData("golang", "go")]
    [InlineData("Postgres", "postgresql")]
    [InlineData("k8s", "kubernetes")]
    [InlineData("Amazon Web Services", "aws")]
    [InlineData("CI CD", "ci/cd")]
    [InlineData("micro-services", "microservices")]
    public void Alias_maps_to_canonical_form(string input, string expected)
    {
        Assert.Equal(expected, SkillNormalizer.Normalize(input));
    }

    [Fact]
    public void Unknown_skill_is_lowercased_and_trimmed()
    {
        Assert.Equal("haskell", SkillNormalizer.Normalize("  Haskell "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_normalizes_to_empty(string? input)
    {
        Assert.Equal("", SkillNormalizer.Normalize(input!));
    }

    [Theory]
    [InlineData("golang", "go")]
    [InlineData("Node", "NodeJS")]
    [InlineData("python", "PYTHON")]
    public void FuzzyMatch_true_for_aliases_and_case(string a, string b)
    {
        Assert.True(SkillNormalizer.FuzzyMatch(a, b));
    }

    [Theory]
    [InlineData("", "python")]
    [InlineData("python", " ")]
    [InlineData("python", "ruby")]
    public void FuzzyMatch_false_for_blank_or_unrelated(string a, string b)
    {
        Assert.False(SkillNormalizer.FuzzyMatch(a, b));
    }

    // F1 / MATCH-01: a skill must match as a whole token, never as a substring of another word.
    [Theory]
    [InlineData("go", "google")]
    [InlineData("go", "mongo")]
    [InlineData("go", "mongodb")]
    [InlineData("java", "javascript")]
    [InlineData("react", "react native")]
    [InlineData("c", "c#")]
    [InlineData("c", "c++")]
    [InlineData("sql", "nosql")]
    [InlineData("rust", "zero trust")]
    public void FuzzyMatch_rejects_substring_false_positives(string a, string b)
    {
        Assert.False(SkillNormalizer.FuzzyMatch(a, b));
        Assert.False(SkillNormalizer.FuzzyMatch(b, a));
    }

    [Theory]
    [InlineData("rest", "REST API Design")]
    [InlineData("aws", "AWS Lambda")]
    [InlineData("kafka", "Apache Kafka")]
    [InlineData("python", "Python 3")]
    public void FuzzyMatch_still_matches_a_skill_inside_a_longer_phrase(string a, string b)
    {
        Assert.True(SkillNormalizer.FuzzyMatch(a, b));
        Assert.True(SkillNormalizer.FuzzyMatch(b, a));
    }

    [Theory]
    [InlineData("We write Go services", "go", true)]
    [InlineData("Go, Rust and C#", "go", true)]
    [InlineData("Good results at google", "go", false)]
    [InlineData("Wrote C# and C++ code", "c", false)]
    [InlineData("Wrote C, C# and C++ code", "c", true)]
    [InlineData("Wrote C# code", "c#", true)]
    [InlineData("Built on .NET Core", ".net", true)]
    [InlineData("JavaScript only", "java", false)]
    [InlineData("", "go", false)]
    [InlineData("text", "", false)]
    public void ContainsTerm_matches_whole_tokens_only(string text, string term, bool expected)
    {
        Assert.Equal(expected, SkillNormalizer.ContainsTerm(text, term));
    }

    [Theory]
    [InlineData("Backend in Golang", "go", true)]
    [InlineData("Backend in Go", "golang", true)]
    [InlineData("Used k8s daily", "kubernetes", true)]
    [InlineData("Good and ongoing work", "go", false)]
    public void ContainsSkill_accepts_aliases_but_not_substrings(string text, string skill, bool expected)
    {
        Assert.Equal(expected, SkillNormalizer.ContainsSkill(text, skill));
    }

    // MATCH-07: a dot between letters, an address or an underscore joins one name
    [Theory]
    [InlineData("TypeScript, Node.js, or Python", "js", false)]
    [InlineData("contributions to llama.cpp", "cpp", false)]
    [InlineData("Learn more at grafana.com today", "grafana", false)]
    [InlineData("email candidate_accessibility@elastic.co", "accessibility", false)]
    [InlineData("We use Go.", "go", true)]
    [InlineData("CSS-in-JS tooling", "js", true)]
    public void ContainsTerm_does_not_split_dotted_names_or_addresses(string text, string term, bool expected)
    {
        Assert.Equal(expected, SkillNormalizer.ContainsTerm(text, term));
    }

    // F21: a single letter next to a hyphen, ampersand or abbreviation dot belongs to another word
    [Theory]
    [InlineData("Washington D.C., and Washington State", "c", false)]
    [InlineData("Kotlin, Swift, Objective-C, Go", "c", false)]
    [InlineData("MVVM-C architecture", "c", false)]
    [InlineData("our R&D strategy", "r", false)]
    [InlineData("C-level stakeholders", "c", false)]
    [InlineData("Java, Go, C, or C++", "c", true)]
    [InlineData("Rust, CUDA, or C (with a preference for Rust)", "c", true)]
    [InlineData("Typescript/Javascript, Go, Rust, C/C++ and Python", "c", true)]
    [InlineData("Python, R, JMP, or equivalent", "r", true)]
    public void A_single_letter_term_is_a_word_of_its_own(string text, string term, bool expected)
    {
        Assert.Equal(expected, SkillNormalizer.ContainsTerm(text, term));
    }

    // F18: a text form with a capital letter only counts written that way, and exception phrases never count
    [Theory]
    [InlineData("datasets go directly into training", "go", false)]
    [InlineData("sync with go-to-market on launches", "go", false)]
    [InlineData("Node, Python, or Go. You have built", "go", true)]
    [InlineData("shapes what we build next.", "next.js", false)]
    [InlineData("we use Next.js and Tailwind", "next.js", true)]
    [InlineData("propose a solid technical approach", "solidjs", false)]
    [InlineData("international banking rails", "ruby on rails", false)]
    [InlineData("from Sinatra toward Rails", "ruby on rails", true)]
    [InlineData("graduating in Spring 2027", "spring", false)]
    [InlineData("exposure to Spring / Spring Boot", "spring", true)]
    [InlineData("node health checks and draining", "node.js", false)]
    [InlineData("please visit https://www.example.com", "https", false)]
    [InlineData("a strong foundation in optimizing workloads", "foundation", false)]
    [InlineData("work is faster, clearer, and less fragmented", "less", false)]
    public void ContainsSkill_needs_the_written_form_and_skips_exceptions(string text, string skill, bool expected)
    {
        Assert.Equal(expected, SkillNormalizer.ContainsSkill(text, skill));
    }

    [Fact]
    public void FindSkill_returns_every_text_form_of_a_skill()
    {
        var hits = SkillNormalizer.FindSkill("Go services, a Golang CLI, and go-live support", "go").ToList();

        Assert.Equal(new[] { (0, 2), (15, 6) }, hits.OrderBy(h => h.Start));
    }

    [Fact]
    public void MatchesAny_uses_fuzzy_matching_over_the_collection()
    {
        Assert.True(SkillNormalizer.MatchesAny("golang", new[] { "ruby", "go" }));
        Assert.False(SkillNormalizer.MatchesAny("rust", new[] { "ruby", "go" }));
        Assert.False(SkillNormalizer.MatchesAny("rust", Array.Empty<string>()));
    }
}

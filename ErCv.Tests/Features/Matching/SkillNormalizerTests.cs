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
    public void Every_alias_maps_to_a_stable_canonical_form()
    {
        var aliases = (Dictionary<string, string>)typeof(SkillNormalizer)
            .GetField("Aliases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;

        Assert.True(aliases.Count > 50);
        foreach (var (alias, canonical) in aliases)
        {
            Assert.Equal(canonical, SkillNormalizer.Normalize(alias));
            Assert.Equal(canonical, SkillNormalizer.Normalize(alias.ToUpperInvariant()));
            // Canonical forms must not be re-aliased (no alias chains).
            Assert.Equal(canonical, SkillNormalizer.Normalize(canonical));
        }
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

    [Fact]
    public void MatchesAny_uses_fuzzy_matching_over_the_collection()
    {
        Assert.True(SkillNormalizer.MatchesAny("golang", new[] { "ruby", "go" }));
        Assert.False(SkillNormalizer.MatchesAny("rust", new[] { "ruby", "go" }));
        Assert.False(SkillNormalizer.MatchesAny("rust", Array.Empty<string>()));
    }
}

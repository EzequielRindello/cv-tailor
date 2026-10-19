using ErCvApi.Helpers;

namespace ErCv.Tests.Helpers
{
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

        // F1: substring Contains makes these match. Fixed by MATCH-01.
        [Theory(Skip = "MATCH-01: FuzzyMatch uses substring Contains (F1)")]
        [InlineData("go", "google")]
        [InlineData("go", "mongo")]
        [InlineData("java", "javascript")]
        public void FuzzyMatch_rejects_substring_false_positives(string a, string b)
        {
            Assert.False(SkillNormalizer.FuzzyMatch(a, b));
        }

        [Fact]
        public void F1_current_behavior_substring_matches()
        {
            // Documents the bug until MATCH-01. Delete this test when the Skip above is removed.
            Assert.True(SkillNormalizer.FuzzyMatch("java", "javascript"));
        }

        [Fact]
        public void MatchesAny_uses_fuzzy_matching_over_the_collection()
        {
            Assert.True(SkillNormalizer.MatchesAny("golang", new[] { "ruby", "go" }));
            Assert.False(SkillNormalizer.MatchesAny("rust", new[] { "ruby", "go" }));
            Assert.False(SkillNormalizer.MatchesAny("rust", Array.Empty<string>()));
        }
    }
}

using ErCvApi.Helpers;
using static ErCv.Tests.Helpers.Build;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class MatchScoreCalculatorTests
    {
        private readonly MatchScoreCalculator _calc = new();

        [Fact]
        public void Full_skill_keyword_and_seniority_match_scores_100_green()
        {
            var cv = Master(new[] { Exp(role: "Senior Engineer", bullets: new[] { "Worked with kafka pipelines" }) },
                languages: new[] { "go", "python" });
            var req = Req(skills: new[] { "go", "python" }, keywords: new[] { "kafka" }, seniority: "senior");

            var m = _calc.Calculate(cv, req);

            Assert.Equal(100, m.Overall);
            Assert.Equal("green", m.ColorClass);
            Assert.True(m.SeniorityMatch);
            Assert.Null(m.LowMatchWarning);
            Assert.Equal(2, m.SkillsMatched);
        }

        [Fact]
        public void No_match_is_red_with_low_match_warning()
        {
            var cv = Master(new[] { Exp(role: "Junior Dev", bullets: new[] { "Did things" }) }, languages: new[] { "php" });
            var req = Req(skills: new[] { "rust", "haskell" }, keywords: new[] { "kafka" }, seniority: "principal");

            var m = _calc.Calculate(cv, req);

            Assert.True(m.Overall < 40);
            Assert.Equal("red", m.ColorClass);
            Assert.NotNull(m.LowMatchWarning);
            Assert.False(m.SeniorityMatch);
        }

        [Fact]
        public void Empty_jd_requirements_do_not_divide_by_zero()
        {
            var m = _calc.Calculate(Master(new[] { Exp() }), Req());

            Assert.InRange(m.Overall, 0, 100);
            Assert.Equal(1, m.TotalSkills);
            Assert.Equal(1, m.TotalKeywords);
        }

        [Fact]
        public void Color_class_follows_thresholds()
        {
            // 1 of 2 skills (50% * 0.5 = 25) + seniority exact (100 * 0.2 = 20) + 0 keywords = 45 -> red.
            // Add keywords to land in yellow: all keywords matched -> 100 capped * 0.3 = 30 -> 75.
            var cv = Master(new[] { Exp(role: "Senior Engineer", bullets: new[] { "kafka work" }) }, languages: new[] { "go" });
            var req = Req(skills: new[] { "go", "rust" }, keywords: new[] { "kafka" }, seniority: "senior");

            var m = _calc.Calculate(cv, req);

            Assert.Equal(75, m.Overall);
            Assert.Equal("yellow", m.ColorClass);
        }

        [Fact]
        public void Keywords_beyond_the_first_15_are_ignored()
        {
            // Names are not substrings of each other (the calculator matches by substring).
            var keywords = Enumerable.Range(0, 30).Select(i => $"term{(char)('a' + i % 26)}{(char)('a' + i / 26)}z").ToArray();
            var cv = Master(new[] { Exp(bullets: new[] { $"{keywords[20]} {keywords[25]}" }) });

            var m = _calc.Calculate(cv, Req(keywords: keywords));

            Assert.Equal(15, m.TotalKeywords);
            Assert.Equal(0, m.KeywordsMatched);
        }

        [Theory]
        [InlineData("Senior Engineer", "senior", 100, true)]
        [InlineData("Lead Engineer", "senior", 70, true)]
        [InlineData("Mid Developer", "senior", 70, true)]
        [InlineData("Junior Developer", "senior", 40, false)]
        [InlineData("Intern", "senior", 10, false)]
        public void Seniority_distance_maps_to_score(string role, string target, int expectedScore, bool match)
        {
            // Isolate seniority: no skills, no keywords. Overall = 0 + 0 + score * 0.2, rounded.
            var cv = Master(new[] { Exp(role: role) });

            var m = _calc.Calculate(cv, Req(seniority: target));

            Assert.Equal((int)Math.Round(expectedScore * 0.2), m.Overall);
            Assert.Equal(match, m.SeniorityMatch);
        }

        [Fact]
        public void Overall_is_always_between_0_and_100()
        {
            var cv = Master(new[] { Exp(role: "Staff Engineer", bullets: new[] { "a b c d e" }) }, languages: new[] { "x" });
            var req = Req(skills: Enumerable.Range(0, 50).Select(i => $"s{i}").ToArray(),
                keywords: Enumerable.Range(0, 50).Select(i => $"k{i}").ToArray());

            Assert.InRange(_calc.Calculate(cv, req).Overall, 0, 100);
        }

        // F3 / MATCH-03: "May 2020" used to outrank "Dec 2023" because end dates were compared as strings.
        [Fact]
        public void Most_recent_role_is_chosen_by_date_not_by_string_order()
        {
            var older = Exp(role: "Intern", end: "May 2020");
            var newer = Exp(role: "Senior Engineer", end: "Dec 2023");

            var m = _calc.Calculate(Master(new[] { older, newer }), Req(seniority: "senior"));

            Assert.True(m.SeniorityMatch);
            Assert.Equal(20, m.Overall);
        }

        [Fact]
        public void Present_role_wins_as_most_recent()
        {
            var past = Exp(role: "Intern", end: "2020");
            var current = Exp(role: "Senior Engineer", end: "Present");

            var m = _calc.Calculate(Master(new[] { past, current }), Req(seniority: "senior"));

            Assert.True(m.SeniorityMatch);
        }

        [Fact]
        public void Keywords_match_whole_words_in_bullets_only()
        {
            var cv = Master(new[] { Exp(bullets: new[] { "Improved goodwill with partners" }) });

            var m = _calc.Calculate(cv, Req(keywords: new[] { "good", "partner" }));

            Assert.Equal(0, m.KeywordsMatched);
        }

        [Fact]
        public void Keyword_matches_when_the_whole_word_is_present()
        {
            var cv = Master(new[] { Exp(bullets: new[] { "Worked with partners on good outcomes" }) });

            var m = _calc.Calculate(cv, Req(keywords: new[] { "good", "partners" }));

            Assert.Equal(2, m.KeywordsMatched);
        }

        [Fact]
        public void Seniority_words_in_a_role_match_whole_words_only()
        {
            // "Internal" must not be read as "intern": that would be level 1 against a mid-level target (a mismatch).
            var cv = Master(new[] { Exp(role: "Internal Tools Engineer") });

            var m = _calc.Calculate(cv, Req(seniority: "mid-level"));

            Assert.True(m.SeniorityMatch);
            Assert.Equal(20, m.Overall);
        }
    }
}

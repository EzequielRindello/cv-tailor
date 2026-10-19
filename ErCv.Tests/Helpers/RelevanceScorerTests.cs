using ErCvApi.Helpers;
using ErCvApi.Models;
using static ErCv.Tests.Helpers.Build;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class RelevanceScorerTests
    {
        private readonly RelevanceScorer _scorer = new();

        private double Score(Experience e, JobRequirements r) =>
            _scorer.ScoreExperiences(new List<Experience> { e }, r).Single().Score;

        [Fact]
        public void Results_are_sorted_by_score_descending()
        {
            var weak = Exp(role: "Support", end: Year(10));
            var strong = Exp(skills: new[] { "python", "kafka" });
            var req = Req(skills: new[] { "python", "kafka" });

            var result = _scorer.ScoreExperiences(new List<Experience> { weak, strong }, req);

            Assert.Same(strong, result[0].Experience);
        }

        [Fact]
        public void Recency_tiers_decrease_with_age()
        {
            var req = Req(seniority: "mid-level");
            double Recency(string end) => Score(Exp(role: "Mid Dev", end: end), req);

            Assert.True(Recency("Present") > Recency(Year(1)));
            Assert.True(Recency(Year(1)) > Recency(Year(2)));
            Assert.True(Recency(Year(2)) > Recency(Year(3)));
            Assert.True(Recency(Year(3)) > Recency(Year(5)));
            Assert.True(Recency(Year(5)) > Recency(Year(9)));
        }

        [Theory]
        [InlineData("Present")]
        [InlineData("current")]
        [InlineData("Now")]
        public void Present_synonyms_score_as_present(string end)
        {
            var req = Req();
            Assert.Equal(Score(Exp(end: "Present"), req), Score(Exp(end: end), req));
        }

        [Fact]
        public void Unparseable_end_date_uses_fallback_not_a_crash()
        {
            var req = Req();
            Assert.Equal(Score(Exp(end: ""), req), Score(Exp(end: "someday"), req));
        }

        [Fact]
        public void Seniority_distance_lowers_the_score()
        {
            var req = Req(seniority: "senior");

            var exact = Score(Exp(role: "Senior Engineer"), req);
            var close = Score(Exp(role: "Lead Engineer"), req);
            var far = Score(Exp(role: "Junior Engineer"), req);
            var farthest = Score(Exp(role: "Intern"), req);

            Assert.True(exact > close);
            Assert.True(close > far);
            Assert.True(far > farthest);
        }

        [Fact]
        public void Skill_listed_on_experience_scores_more_than_skill_only_in_bullets()
        {
            var req = Req(skills: new[] { "kafka" });

            var listed = Score(Exp(skills: new[] { "kafka" }), req);
            var inBullet = Score(Exp(bullets: new[] { "Built pipelines with Kafka" }), req);
            var none = Score(Exp(), req);

            Assert.True(listed > inBullet);
            Assert.True(inBullet > none);
        }

        [Fact]
        public void Skill_points_are_capped_at_40()
        {
            var many = Enumerable.Range(0, 30).Select(i => $"skill{i}").ToArray();
            var req = Req(skills: many);

            var capped = Score(Exp(skills: many), req);
            var none = Score(Exp(), req);

            Assert.Equal(40.0, capped - none, precision: 6);
        }

        [Fact]
        public void Keyword_in_bullets_adds_points_only_as_whole_word()
        {
            var req = Req(keywords: new[] { "ship" });

            var whole = Score(Exp(bullets: new[] { "We ship weekly" }), req);
            var partial = Score(Exp(bullets: new[] { "Worked in a leadership role" }), req);

            Assert.True(whole > partial);
        }

        [Fact]
        public void Only_first_15_keywords_are_evaluated()
        {
            var keywords = Enumerable.Range(0, 20).Select(i => $"kw{i}").ToArray();
            var req = Req(keywords: keywords);
            var allIn = Exp(bullets: new[] { string.Join(" ", keywords) });

            var withNone = Score(Exp(bullets: new[] { "nothing relevant" }), req);

            Assert.Equal(15 * 2.0, Score(allIn, req) - withNone, precision: 6);
        }

        [Fact]
        public void GetTopSkills_prefers_jd_matches_and_respects_the_cap()
        {
            var skills = new Skills
            {
                Languages = new() { "python", "go" },
                Tools = new() { "git", "docker", "jira" }
            };
            var req = Req(skills: new[] { "go", "docker" });

            var top = _scorer.GetTopSkills(new List<Experience>(), req, skills, maxSkills: 3);

            Assert.Equal(3, top.Count);
            Assert.Contains("go", top.Take(2));
            Assert.Contains("docker", top.Take(2));
        }

        [Fact]
        public void GetTopSkills_fills_with_remaining_skills_when_few_match()
        {
            var skills = new Skills { Languages = new() { "a1", "b2", "c3" } };

            var top = _scorer.GetTopSkills(new List<Experience>(), Req(), skills, maxSkills: 15);

            Assert.Equal(3, top.Count);
        }

        [Fact]
        public void FlattenSkills_handles_null()
        {
            Assert.Empty(RelevanceScorer.FlattenSkills(null));
        }
    }
}

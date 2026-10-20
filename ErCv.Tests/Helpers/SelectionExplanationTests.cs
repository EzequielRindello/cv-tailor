using System.Text.Json;
using ErCv.Tests.Fixtures;
using ErCvApi.Helpers;
using ErCvApi.Models;

namespace ErCv.Tests.Helpers
{
    /// <summary>MATCH-04: the explanation must add up and must describe the CV that was actually produced.</summary>
    [Trait("Category", "Unit")]
    public class SelectionExplanationTests
    {
        private const double Tolerance = 1e-9;

        public static TheoryData<string, string, bool> Matrix()
        {
            var data = new TheoryData<string, string, bool>();
            foreach (var profile in FixtureLoader.ProfileIds)
            foreach (var jd in FixtureLoader.AllJdNames)
            foreach (var twoPages in new[] { false, true })
                data.Add(profile, jd, twoPages);
            return data;
        }

        private static (GeneratedCV Cv, SelectionExplanation Explanation) Run(string profile, string jd, bool twoPages)
        {
            var master = FixtureLoader.LoadProfile(profile);
            var req = new JobDescriptionParser().Parse(FixtureLoader.LoadJd(jd));
            return new CVSelector().SelectWithExplanation(master, req, twoPages);
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Experience_and_bullet_parts_add_up_to_their_score(string profile, string jd, bool twoPages)
        {
            var (_, explanation) = Run(profile, jd, twoPages);

            foreach (var exp in explanation.Experiences)
            {
                Assert.Equal(exp.Score, exp.Parts.Sum(p => p.Points), Tolerance);
                foreach (var bullet in exp.Bullets)
                    Assert.Equal(bullet.Score, bullet.Parts.Sum(p => p.Points), Tolerance);
            }
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Explanation_describes_the_final_cv(string profile, string jd, bool twoPages)
        {
            var (cv, explanation) = Run(profile, jd, twoPages);

            Assert.Equal(
                cv.Experience.Select(e => $"{e.Role}|{e.Company}|{e.StartDate}|{e.EndDate}"),
                explanation.Experiences.Select(e => e.Key));

            for (var i = 0; i < cv.Experience.Count; i++)
            {
                var selected = explanation.Experiences[i].Bullets.Where(b => b.Selected).Select(b => b.Text);
                Assert.Equal(cv.Experience[i].Bullets.OrderBy(b => b), selected.OrderBy(b => b));
            }
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Selected_bullets_never_score_below_dropped_ones_unless_trimmed_for_space(
            string profile, string jd, bool twoPages)
        {
            var (_, explanation) = Run(profile, jd, twoPages);

            foreach (var exp in explanation.Experiences)
            {
                var selected = exp.Bullets.Where(b => b.Selected).ToList();
                var dropped = exp.Bullets.Where(b => !b.Selected).ToList();
                if (selected.Count == 0 || dropped.Count == 0) continue;

                // The page constraint may trim the lowest-ranked bullets of a role, never a better one first.
                Assert.True(selected.Min(b => b.Score) >= dropped.Min(b => b.Score));
            }
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Explanation_does_not_change_the_selected_cv(string profile, string jd, bool twoPages)
        {
            var master = FixtureLoader.LoadProfile(profile);
            var req = new JobDescriptionParser().Parse(FixtureLoader.LoadJd(jd));

            var plain = new CVSelector().SelectAndAdaptCV(master, req, twoPages);
            var (explained, _) = new CVSelector().SelectWithExplanation(master, req, twoPages);

            Assert.Equal(JsonSerializer.Serialize(plain), JsonSerializer.Serialize(explained));
        }

        [Fact]
        public void A_bullet_with_a_metric_and_an_action_verb_shows_both_parts()
        {
            var master = Build.Master(new[]
            {
                Build.Exp(
                    bullets: new[] { "Reduced API latency by 40% across the payments platform using Go and Kafka" },
                    skills: new[] { "Go", "Kafka" })
            });
            var req = Build.Req(skills: new[] { "Go", "Kafka" });

            var (_, explanation) = new CVSelector().SelectWithExplanation(master, req);

            var bullet = Assert.Single(Assert.Single(explanation.Experiences).Bullets);
            Assert.Contains(bullet.Parts, p => p.Label == "Metric" && p.Points == 6.0);
            Assert.Contains(bullet.Parts, p => p.Label == "Action verb" && p.Points == 4.0);
            Assert.Contains(bullet.Parts, p => p.Label == "Matched skills" && p.Points > 0);
            Assert.True(bullet.Selected);
        }

        [Fact]
        public void Experience_parts_are_named_for_the_ui()
        {
            var master = Build.Master(new[] { Build.Exp(bullets: new[] { "Built things with Go" }, skills: new[] { "Go" }) });

            var (_, explanation) = new CVSelector().SelectWithExplanation(master, Build.Req(skills: new[] { "Go" }));

            Assert.Equal(
                new[] { "Matched skills", "Keywords", "Recency", "Seniority fit" },
                Assert.Single(explanation.Experiences).Parts.Select(p => p.Label));
        }
    }
}

using System.Text.Json;
using ErCv.Tests.Fixtures;
using ErCvApi.Helpers;
using ErCvApi.Models;

// Every test shares the same profile x JD x template x pages matrix, so some do not use every parameter.
#pragma warning disable xUnit1026

namespace ErCv.Tests.Invariants
{
    /// <summary>
    /// The product promise: the generated CV only ever contains what the master profile says.
    /// Every test runs for each profile x JD in the corpus x template x 1-page / 2-page.
    /// </summary>
    [Trait("Category", "Invariant")]
    public class InvariantTests
    {
        public static TheoryData<string, string, CVTemplate, bool> Matrix()
        {
            var data = new TheoryData<string, string, CVTemplate, bool>();
            foreach (var profile in FixtureLoader.ProfileIds)
            foreach (var jd in FixtureLoader.JdNames)
            foreach (var template in Enum.GetValues<CVTemplate>())
            foreach (var twoPages in new[] { false, true })
                data.Add(profile, jd, template, twoPages);
            return data;
        }

        private sealed record Run(
            CVMaster Master, GeneratedCV Generated, MatchScore Match, ATSScore Ats,
            MissingSkillsAnalysis Missing, KeywordAnalysis Keywords);

        private static Run Generate(string profile, string jdName, CVTemplate template, bool twoPages)
        {
            var master = FixtureLoader.LoadProfile(profile);
            var jd = FixtureLoader.LoadJd(jdName);
            var req = new JobDescriptionParser().Parse(jd);

            var match = new MatchScoreCalculator().Calculate(master, req);
            var missing = new MissingSkillsAnalyzer().Analyze(master, req);
            var generated = new CVSelector().SelectAndAdaptCV(master, req, twoPages);
            var keywords = new KeywordAnalyzer().Analyze(jd, generated);
            var ats = new ATSChecker().Check(generated, req, template);

            return new Run(master, generated, match, ats, missing, keywords);
        }

        private static string Key(Experience e) => $"{e.Role}|{e.Company}|{e.StartDate}|{e.EndDate}";

        private static string Json(object o) => JsonSerializer.Serialize(o);

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Every_bullet_exists_verbatim_in_the_same_master_experience(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var r = Generate(profile, jd, template, twoPages);

            foreach (var exp in r.Generated.Experience)
            {
                var source = r.Master.Experience.SingleOrDefault(m => Key(m) == Key(exp));
                Assert.NotNull(source);
                Assert.All(exp.Bullets, b => Assert.Contains(b, source!.Bullets));
            }
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Every_skill_project_and_education_entry_exists_in_the_master(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var r = Generate(profile, jd, template, twoPages);

            var masterSkills = RelevanceScorer.FlattenSkills(r.Master.Skills);
            Assert.All(r.Generated.Skills, s => Assert.Contains(s, masterSkills));

            var masterProjects = r.Master.Projects.Select(Json).ToList();
            Assert.All(r.Generated.Projects, p => Assert.Contains(Json(p), masterProjects));

            var masterEducation = r.Master.Education.Select(Json).ToList();
            Assert.All(r.Generated.Education, e => Assert.Contains(Json(e), masterEducation));
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Experience_entries_come_from_the_master_without_duplicates(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var r = Generate(profile, jd, template, twoPages);

            var keys = r.Generated.Experience.Select(Key).ToList();

            Assert.Equal(keys.Count, keys.Distinct().Count());
            Assert.All(r.Generated.Experience, e =>
                Assert.Contains(r.Master.Experience, m => Key(m) == Key(e)));
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Bullets_keep_their_original_relative_order_inside_each_role(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var r = Generate(profile, jd, template, twoPages);

            foreach (var exp in r.Generated.Experience)
            {
                var source = r.Master.Experience.Single(m => Key(m) == Key(exp));
                var positions = exp.Bullets.Select(b => source.Bullets.IndexOf(b)).ToList();

                Assert.Equal(positions.OrderBy(p => p), positions);
            }
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Contact_header_is_identical_to_the_master(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var r = Generate(profile, jd, template, twoPages);

            Assert.Equal(Json(r.Master.Header), Json(r.Generated.Header));
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void All_scores_stay_within_their_ranges(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var r = Generate(profile, jd, template, twoPages);

            Assert.InRange(r.Match.Overall, 0, 100);
            Assert.InRange(r.Ats.Overall, 0, 100);
            Assert.InRange(r.Keywords.KeywordDensity, 0.0, 1.0);
            Assert.InRange(r.Missing.PotentialScoreIncrease, 0, 40);
            Assert.InRange(r.Match.SkillsMatched, 0, r.Match.TotalSkills);
            Assert.InRange(r.Match.KeywordsMatched, 0, r.Match.TotalKeywords);
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Generation_does_not_mutate_the_master_profile(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var master = FixtureLoader.LoadProfile(profile);
            var before = Json(master);
            var req = new JobDescriptionParser().Parse(FixtureLoader.LoadJd(jd));

            new MatchScoreCalculator().Calculate(master, req);
            new MissingSkillsAnalyzer().Analyze(master, req);
            new CVSelector().SelectAndAdaptCV(master, req, twoPages);

            Assert.Equal(before, Json(master));
        }

        // F5 / ENG-02: Header and Education used to be shared references with the master.
        [Theory]
        [MemberData(nameof(Matrix))]
        public void Editing_the_generated_cv_never_touches_the_master(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var master = FixtureLoader.LoadProfile(profile);
            var before = Json(master);
            var req = new JobDescriptionParser().Parse(FixtureLoader.LoadJd(jd));

            var generated = new CVSelector().SelectAndAdaptCV(master, req, twoPages);
            generated.Header.FullName = "CHANGED";
            generated.Education.Clear();
            foreach (var e in generated.Experience)
            {
                e.Bullets.Clear();
                e.Skills.Clear();
            }
            generated = new PageConstraintHelper().EnforcePageConstraint(generated, twoPages);

            Assert.Equal(before, Json(master));
        }
    }
}

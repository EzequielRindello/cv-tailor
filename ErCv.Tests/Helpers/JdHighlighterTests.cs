using ErCv.Tests.Fixtures;
using ErCvApi.Helpers;
using ErCvApi.Models;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class JdHighlighterTests
    {
        private static readonly JdHighlighter Highlighter = new();

        private static JdHighlight Run(string jd, string[] skills, CVMaster? master = null, GeneratedCV? cv = null) =>
            Highlighter.Build(jd, Build.Req(skills: skills), master ?? Build.Master(), cv ?? Build.Generated());

        private static string[] Highlighted(JdHighlight h) =>
            h.Segments.Where(s => s.Skill != null).Select(s => s.Text).ToArray();

        public static TheoryData<string, string> Matrix()
        {
            var data = new TheoryData<string, string>();
            foreach (var profile in FixtureLoader.ProfileIds)
            foreach (var jd in FixtureLoader.JdNames)
                data.Add(profile, jd);
            return data;
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Segments_join_back_into_the_exact_job_description(string profile, string jdName)
        {
            var jd = FixtureLoader.LoadJd(jdName);
            var master = FixtureLoader.LoadProfile(profile);
            var req = new JobDescriptionParser().Parse(jd);
            var cv = new CVSelector().SelectAndAdaptCV(master, req);

            var result = Highlighter.Build(jd, req, master, cv);

            Assert.Equal(jd, string.Concat(result.Segments.Select(s => s.Text)));
            Assert.All(result.Segments, s => Assert.NotEmpty(s.Text));
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void Every_highlighted_skill_has_info(string profile, string jdName)
        {
            var jd = FixtureLoader.LoadJd(jdName);
            var master = FixtureLoader.LoadProfile(profile);
            var req = new JobDescriptionParser().Parse(jd);
            var cv = new CVSelector().SelectAndAdaptCV(master, req);

            var result = Highlighter.Build(jd, req, master, cv);

            var infoSkills = result.Skills.Select(s => s.Skill).ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.All(result.Segments.Where(s => s.Skill != null), s => Assert.Contains(s.Skill!, infoSkills));
            Assert.Equal(result.Skills.Count, infoSkills.Count);
        }

        [Fact]
        public void Empty_job_description_gives_an_empty_result()
        {
            var result = Run("", new[] { "go" });

            Assert.Empty(result.Segments);
            Assert.Empty(result.Skills);
        }

        [Fact]
        public void Whole_tokens_only_so_go_is_not_found_inside_google()
        {
            var result = Run("We use Google Cloud and Go daily, which is good.", new[] { "go" });

            Assert.Equal(new[] { "Go" }, Highlighted(result));
        }

        [Fact]
        public void The_longest_match_wins_over_a_shorter_one_inside_it()
        {
            var result = Run("Build apps with React Native and React.", new[] { "react", "react native" });

            Assert.Equal(new[] { "React Native", "React" }, Highlighted(result));
        }

        [Fact]
        public void Every_occurrence_is_highlighted_keeping_the_original_casing()
        {
            var result = Run("Kafka first. Also KAFKA, and kafka.", new[] { "kafka" });

            Assert.Equal(new[] { "Kafka", "KAFKA", "kafka" }, Highlighted(result));
        }

        [Fact]
        public void Aliases_are_highlighted_for_the_canonical_skill()
        {
            var result = Run("Experience with Golang and k8s.", new[] { "go", "kubernetes" });

            Assert.Equal(new[] { "Golang", "k8s" }, Highlighted(result));
            Assert.Equal("go", result.Segments.First(s => s.Text == "Golang").Skill);
        }

        [Fact]
        public void Everyday_words_that_are_aliases_are_not_highlighted()
        {
            var result = Run("Next steps: build with Next.js. RN experience is a plus.", new[] { "next.js", "react native" });

            Assert.Equal(new[] { "Next.js" }, Highlighted(result));
        }

        [Fact]
        public void Skills_in_the_profile_are_matched_and_the_rest_are_not()
        {
            var master = Build.Master(languages: new[] { "Go" });

            var result = Run("Go and Kafka required.", new[] { "go", "kafka" }, master);

            Assert.True(result.Skills.Single(s => s.Skill == "go").InProfile);
            Assert.False(result.Skills.Single(s => s.Skill == "kafka").InProfile);
        }

        [Fact]
        public void A_skill_only_mentioned_in_a_bullet_counts_as_in_the_profile()
        {
            var master = Build.Master(new[] { Build.Exp(bullets: new[] { "Streamed events with Kafka" }) });

            var result = Run("Kafka required.", new[] { "kafka" }, master);

            Assert.True(result.Skills.Single().InProfile);
        }

        [Fact]
        public void Locations_point_at_the_bullet_the_skills_list_and_the_role_tags()
        {
            var cv = Build.Generated(
                new[]
                {
                    Build.Exp(role: "Backend Engineer", company: "Acme",
                        bullets: new[] { "Built Go services handling 2M requests per day" },
                        skills: new[] { "Go" })
                },
                skills: new[] { "Go", "SQL" });

            var result = Run("Go required.", new[] { "go" }, cv: cv);

            var locations = result.Skills.Single().Locations;
            Assert.Contains(locations, l => l.Section == "Skills");
            Assert.Contains(locations, l => l.Section == "Experience" && l.Label == "Backend Engineer, Acme"
                                           && l.Excerpt!.StartsWith("Built Go services"));
            Assert.Contains(locations, l => l.Section == "Experience" && l.Excerpt == "Listed in this role's skills");
        }

        [Fact]
        public void Long_bullets_are_cut_in_the_excerpt()
        {
            var longBullet = "Built Go services " + new string('x', 400);
            var cv = Build.Generated(new[] { Build.Exp(bullets: new[] { longBullet }) });

            var result = Run("Go required.", new[] { "go" }, cv: cv);

            var excerpt = result.Skills.Single().Locations.Single().Excerpt!;
            Assert.True(excerpt.Length < longBullet.Length);
            Assert.EndsWith("...", excerpt);
        }

        [Fact]
        public void A_skill_in_the_profile_but_not_in_the_cv_has_no_locations()
        {
            var master = Build.Master(languages: new[] { "Go" });

            var result = Run("Go required.", new[] { "go" }, master, Build.Generated());

            var info = result.Skills.Single();
            Assert.True(info.InProfile);
            Assert.Empty(info.Locations);
        }

        [Fact]
        public void Markup_in_the_job_description_stays_plain_text()
        {
            var jd = "<script>alert('x')</script> Go required <img src=x onerror=alert(1)>";

            var result = Run(jd, new[] { "go" });

            Assert.Equal(jd, string.Concat(result.Segments.Select(s => s.Text)));
            Assert.Equal(new[] { "Go" }, Highlighted(result));
        }
    }
}

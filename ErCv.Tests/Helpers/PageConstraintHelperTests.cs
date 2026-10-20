using ErCvApi.Helpers;
using ErCvApi.Models;
using static ErCv.Tests.Helpers.Build;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class PageConstraintHelperTests
    {
        private readonly PageConstraintHelper _helper = new();

        private static string Bullet(int chars) => new('x', chars);

        /// <summary>A CV that is clearly longer than one page: many experiences with several long bullets.</summary>
        private static GeneratedCV Oversized(int experiences = 5, int bulletsEach = 6, int bulletChars = 190)
        {
            var exps = Enumerable.Range(0, experiences)
                .Select(i => Exp(role: $"Role {i}", bullets: Enumerable.Range(0, bulletsEach).Select(_ => Bullet(bulletChars)).ToArray()))
                .ToArray();
            var cv = Generated(exps, skills: Enumerable.Range(0, 15).Select(i => $"s{i}").ToArray());
            cv.Summary = Bullet(250);
            return cv;
        }

        [Fact]
        public void Line_count_grows_with_content()
        {
            var small = Generated(new[] { Exp(bullets: new[] { "short" }) });
            var big = Oversized();

            Assert.True(_helper.CalculateTotalLines(big) > _helper.CalculateTotalLines(small));
        }

        [Fact]
        public void Long_bullet_counts_as_multiple_lines()
        {
            var one = Generated(new[] { Exp(bullets: new[] { Bullet(90) }) });
            var three = Generated(new[] { Exp(bullets: new[] { Bullet(250) }) });

            Assert.Equal(2, _helper.CalculateTotalLines(three) - _helper.CalculateTotalLines(one));
        }

        [Fact]
        public void CV_that_already_fits_is_returned_unchanged()
        {
            var cv = Generated(new[] { Exp(bullets: new[] { "a", "b", "c" }) });

            var result = _helper.EnforcePageConstraint(cv);

            Assert.Equal(3, result.Experience.Single().Bullets.Count);
        }

        [Fact]
        public void Oversized_cv_is_trimmed_to_fit_one_page()
        {
            var result = _helper.EnforcePageConstraint(Oversized());

            Assert.True(_helper.CalculateTotalLines(result) <= 52);
        }

        [Fact]
        public void Two_page_mode_allows_roughly_double_the_content()
        {
            // Between one and two pages worth of content.
            Assert.InRange(_helper.CalculateTotalLines(Oversized(6, 4)), 53, 104);

            var onePage = _helper.EnforcePageConstraint(Oversized(6, 4), allowTwoPages: false);
            var twoPage = _helper.EnforcePageConstraint(Oversized(6, 4), allowTwoPages: true);

            Assert.True(_helper.CalculateTotalLines(onePage) <= 52);
            Assert.True(_helper.CalculateTotalLines(twoPage) > _helper.CalculateTotalLines(onePage));
        }

        [Fact]
        public void Bullets_are_trimmed_first_and_projects_survive_when_that_is_enough()
        {
            var cv = Oversized(experiences: 4, bulletsEach: 8);
            cv.Projects = new List<Project> { new() { Description = Bullet(100) } };

            var result = _helper.EnforcePageConstraint(cv);

            Assert.All(result.Experience, e => Assert.True(e.Bullets.Count >= 2));
            Assert.Equal(4, result.Experience.Count);
            Assert.Single(result.Projects);
        }

        [Fact]
        public void Projects_then_experiences_are_dropped_when_bullet_trimming_is_not_enough()
        {
            var cv = Oversized(experiences: 6, bulletsEach: 2, bulletChars: 400);
            cv.Projects = new List<Project> { new() { Description = Bullet(100) } };

            var result = _helper.EnforcePageConstraint(cv);

            Assert.Empty(result.Projects);
            Assert.True(result.Experience.Count < 6);
            Assert.All(result.Experience, e => Assert.Equal(2, e.Bullets.Count));
        }

        [Fact]
        public void Skills_are_trimmed_last_but_never_below_eight()
        {
            var cv = Oversized(experiences: 2, bulletsEach: 2, bulletChars: 900);
            cv.Skills = Enumerable.Range(0, 30).Select(i => $"s{i}").ToList();

            var result = _helper.EnforcePageConstraint(cv);

            Assert.Equal(2, result.Experience.Count);
            Assert.Equal(8, result.Skills.Count);
        }

        [Fact]
        public void Never_trims_below_two_experiences()
        {
            var cv = Oversized(experiences: 6, bulletsEach: 2, bulletChars: 400);

            var result = _helper.EnforcePageConstraint(cv);

            Assert.True(result.Experience.Count >= 2);
        }

        [Fact]
        public void CanFitProject_true_for_small_cv_false_for_full_one()
        {
            var small = Generated(new[] { Exp(bullets: new[] { "a" }) });
            var full = Oversized();

            Assert.True(_helper.CanFitProject(small));
            Assert.False(_helper.CanFitProject(full));
        }

        [Fact]
        public void EnforceOnePageConstraint_matches_non_two_page_enforcement()
        {
            var a = _helper.EnforceOnePageConstraint(Oversized());
            var b = _helper.EnforcePageConstraint(Oversized(), allowTwoPages: false);

            Assert.Equal(_helper.CalculateTotalLines(b), _helper.CalculateTotalLines(a));
        }

        [Fact]
        public void Null_collections_do_not_crash()
        {
            var cv = new GeneratedCV { Education = null!, Experience = null!, Skills = null! };

            var lines = _helper.CalculateTotalLines(cv);

            Assert.True(lines > 0);
        }

        [Fact]
        public void Enforcing_returns_a_new_cv_and_leaves_the_input_untouched()
        {
            var input = Oversized(experiences: 6, bulletsEach: 2, bulletChars: 400);
            input.Projects = new List<Project> { new() { Description = Bullet(100) } };
            var before = System.Text.Json.JsonSerializer.Serialize(input);

            var result = _helper.EnforcePageConstraint(input);

            Assert.NotSame(input, result);
            Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(input));
            Assert.True(result.Experience.Count < input.Experience.Count);
        }

        [Fact]
        public void A_cv_that_already_fits_is_still_returned_as_a_copy()
        {
            var input = Generated(new[] { Exp(bullets: new[] { "a", "b", "c" }) });

            var result = _helper.EnforcePageConstraint(input);

            Assert.NotSame(input, result);
            Assert.NotSame(input.Experience[0], result.Experience[0]);
            Assert.Equal(input.Experience[0].Bullets, result.Experience[0].Bullets);
        }
    }
}

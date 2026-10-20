using ErCvApi.Models;
using static ErCv.Tests.Documents.DocumentSupport;

namespace ErCv.Tests.Documents
{
    [Trait("Category", "Documents")]
    public class HtmlPreviewTests
    {
        public static TheoryData<string, string, CVTemplate, bool> Cases => ProfileJdTemplatePages();

        [Theory]
        [MemberData(nameof(Cases))]
        public void Preview_has_no_unrendered_mustache_tags(string profile, string jd, CVTemplate template, bool twoPages)
        {
            var html = Renderer(template).RenderHtmlPublic(Generate(profile, jd, twoPages));

            Assert.DoesNotContain("{{", html);
            Assert.DoesNotContain("}}", html);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Preview_contains_name_titles_and_every_bullet(string profile, string jd, CVTemplate template, bool twoPages)
        {
            var cv = Generate(profile, jd, twoPages);

            var html = Squash(Renderer(template).RenderHtmlPublic(cv));

            Assert.Contains(Squash(cv.Header.FullName), html);
            foreach (var title in ExpectedTitles(cv, template))
                Assert.Contains(Squash(title), html);
            foreach (var bullet in cv.Experience.SelectMany(e => e.Bullets))
                Assert.Contains(Squash(bullet), html);
        }
    }
}

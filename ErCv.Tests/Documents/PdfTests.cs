using System.Collections.Concurrent;
using ErCv.Tests.Fixtures;
using ErCvApi.Models;
using UglyToad.PdfPig;
using static ErCv.Tests.Documents.DocumentSupport;

namespace ErCv.Tests.Documents
{
    [Collection("Documents")]
    [Trait("Category", "Documents")]
    public class PdfTests
    {
        public static TheoryData<string, string, CVTemplate, bool> Cases => ProfileJdTemplatePages();

        public static TheoryData<string, string, CVTemplate, bool> PageCountCases =>
            Filter((t, two) => !(t == CVTemplate.Minimalist && !two));

        public static TheoryData<string, string, CVTemplate, bool> MinimalistOnePageCases =>
            Filter((t, two) => t == CVTemplate.Minimalist && !two);

        public static TheoryData<string, string, CVTemplate, bool> ExtractionCases =>
            Filter((t, _) => t != CVTemplate.Modern);

        public static TheoryData<string, string, CVTemplate, bool> ModernCases =>
            Filter((t, _) => t == CVTemplate.Modern);

        private static TheoryData<string, string, CVTemplate, bool> Filter(Func<CVTemplate, bool, bool> keep)
        {
            var data = new TheoryData<string, string, CVTemplate, bool>();
            foreach (var profile in FixtureLoader.ProfileIds)
            foreach (var jd in FixtureLoader.JdNames)
            foreach (var template in Enum.GetValues<CVTemplate>())
            foreach (var twoPages in new[] { false, true })
                if (keep(template, twoPages)) data.Add(profile, jd, template, twoPages);
            return data;
        }

        // Rendering dominates the run time, so each combination is rendered once and shared by the tests.
        private static readonly ConcurrentDictionary<string, Task<(byte[], GeneratedCV)>> Cache = new();

        private static Task<(byte[] Pdf, GeneratedCV Cv)> RenderAsync(
            string profile, string jd, CVTemplate template, bool twoPages) =>
            Cache.GetOrAdd($"{profile}|{jd}|{template}|{twoPages}", async _ =>
            {
                var cv = Generate(profile, jd, twoPages);
                return (await Renderer(template).RenderPdfAsync(cv), cv);
            });

        private static string ExtractedText(byte[] pdf)
        {
            using var doc = PdfDocument.Open(pdf);
            return Squash(string.Join(" ", doc.GetPages().Select(p => p.Text)));
        }

        private static async Task AssertPageCount(string profile, string jd, CVTemplate template, bool twoPages)
        {
            var (pdf, _) = await RenderAsync(profile, jd, template, twoPages);

            using var doc = PdfDocument.Open(pdf);
            if (twoPages) Assert.InRange(doc.NumberOfPages, 1, 2);
            else Assert.Equal(1, doc.NumberOfPages);
        }

        private static async Task AssertContainsNameTitlesAndBullets(
            string profile, string jd, CVTemplate template, bool twoPages)
        {
            var (pdf, cv) = await RenderAsync(profile, jd, template, twoPages);
            var text = ExtractedText(pdf);

            Assert.Contains(Squash(cv.Header.FullName), text);
            foreach (var title in ExpectedTitles(cv, template))
                Assert.Contains(Squash(title), text);
            foreach (var bullet in cv.Experience.SelectMany(e => e.Bullets))
                Assert.Contains(Squash(bullet), text);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public async Task Is_a_valid_pdf(string profile, string jd, CVTemplate template, bool twoPages)
        {
            var (pdf, _) = await RenderAsync(profile, jd, template, twoPages);

            Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdf, 0, 5));
            using var doc = PdfDocument.Open(pdf);
            Assert.True(doc.NumberOfPages >= 1);
        }

        [Theory]
        [MemberData(nameof(PageCountCases))]
        public Task Page_count_respects_the_selected_mode(string profile, string jd, CVTemplate template, bool twoPages) =>
            AssertPageCount(profile, jd, template, twoPages);

        // F6: page fit is estimated by character count, not measured. Minimalist lays out taller than the estimate.
        [Theory(Skip = "MATCH-08: Minimalist 1-page output spills onto a second page (F6)")]
        [MemberData(nameof(MinimalistOnePageCases))]
        public Task Minimalist_one_page_mode_stays_on_one_page(string profile, string jd, CVTemplate template, bool twoPages) =>
            AssertPageCount(profile, jd, template, twoPages);

        [Theory]
        [MemberData(nameof(ExtractionCases))]
        public Task Extracted_text_contains_name_titles_and_every_bullet(
            string profile, string jd, CVTemplate template, bool twoPages) =>
            AssertContainsNameTitlesAndBullets(profile, jd, template, twoPages);

        // F16: the Modern PDF loses characters at text-extraction level, e.g. "backoff reducing" becomes
        // "backoreducing". Root cause not confirmed (template layout or font ligatures).
        [Theory(Skip = "NEW FINDING F16: Modern PDF text extraction drops characters")]
        [MemberData(nameof(ModernCases))]
        public Task Modern_extracted_text_contains_name_titles_and_every_bullet(
            string profile, string jd, CVTemplate template, bool twoPages) =>
            AssertContainsNameTitlesAndBullets(profile, jd, template, twoPages);

        [Theory]
        [MemberData(nameof(ExtractionCases))]
        public async Task Extracted_text_follows_reading_order(string profile, string jd, CVTemplate template, bool twoPages)
        {
            var (pdf, cv) = await RenderAsync(profile, jd, template, twoPages);
            var text = ExtractedText(pdf);

            // Search forward from the previous hit: a title word such as "experience" can also occur in the summary.
            var cursor = 0;
            var sequence = new List<string> { cv.Header.FullName };
            sequence.AddRange(ExpectedTitles(cv, template));

            foreach (var item in sequence)
            {
                var at = text.IndexOf(Squash(item), cursor, StringComparison.Ordinal);
                Assert.True(at >= 0, $"'{item}' not found after position {cursor}");
                cursor = at + Squash(item).Length;
            }

            // Bullets must also appear in CV order (they are inside the sections, after the Experience title).
            var experienceTitleAt = text.IndexOf(Squash("Experience"), StringComparison.Ordinal);
            var bulletCursor = experienceTitleAt;
            foreach (var bullet in cv.Experience.SelectMany(e => e.Bullets))
            {
                var at = text.IndexOf(Squash(bullet), bulletCursor, StringComparison.Ordinal);
                Assert.True(at >= 0, $"bullet out of order or missing: {bullet}");
                bulletCursor = at + Squash(bullet).Length;
            }
        }
    }
}

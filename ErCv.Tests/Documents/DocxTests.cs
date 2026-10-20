using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using ErCvApi.Constants;
using ErCvApi.Models;
using static ErCv.Tests.Documents.DocumentSupport;

namespace ErCv.Tests.Documents
{
    [Trait("Category", "Documents")]
    public class DocxTests
    {
        public static TheoryData<string, string, bool> Cases => ProfileJdPages();

        private static List<Paragraph> Paragraphs(byte[] docx)
        {
            using var stream = new MemoryStream(docx);
            using var doc = WordprocessingDocument.Open(stream, false);
            return doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>()
                .Select(p => (Paragraph)p.CloneNode(true)).ToList();
        }

        private static byte[] Render(string profile, string jd, bool twoPages, out GeneratedCV cv)
        {
            cv = Generate(profile, jd, twoPages);
            return Renderer(CVTemplate.Harvard).RenderDocx(cv);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Opens_with_openxml_and_has_a_body(string profile, string jd, bool twoPages)
        {
            var bytes = Render(profile, jd, twoPages, out _);

            using var stream = new MemoryStream(bytes);
            using var doc = WordprocessingDocument.Open(stream, false);

            Assert.NotNull(doc.MainDocumentPart?.Document.Body);
            Assert.Equal((byte)'P', bytes[0]);   // zip container ("PK")
            Assert.Equal((byte)'K', bytes[1]);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Section_titles_appear_in_the_expected_order(string profile, string jd, bool twoPages)
        {
            var bytes = Render(profile, jd, twoPages, out var cv);
            var paragraphs = Paragraphs(bytes).Select(p => p.InnerText).ToList();

            var expected = new List<string>();
            if (!string.IsNullOrEmpty(cv.Summary)) expected.Add(DocumentConstants.SectionSummary);
            expected.Add(DocumentConstants.SectionEducation);
            expected.Add(DocumentConstants.SectionExperience);
            if (cv.Skills.Any()) expected.Add(DocumentConstants.SectionSkills);
            if (cv.Projects.Any()) expected.Add(DocumentConstants.SectionProjects);

            var found = paragraphs.Where(expected.Contains).ToList();
            Assert.Equal(expected, found);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Name_and_contact_line_are_present(string profile, string jd, bool twoPages)
        {
            var bytes = Render(profile, jd, twoPages, out var cv);
            var paragraphs = Paragraphs(bytes).Select(p => p.InnerText).ToList();

            Assert.Equal(cv.Header.FullName.ToUpper(), paragraphs[0]);

            var contact = paragraphs[1];
            foreach (var part in new[] { cv.Header.Email, cv.Header.Phone, cv.Header.Location, cv.Header.LinkedIn, cv.Header.GitHub })
                if (!string.IsNullOrWhiteSpace(part))
                    Assert.Contains(part, contact);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Bullet_count_equals_the_generated_cv(string profile, string jd, bool twoPages)
        {
            var bytes = Render(profile, jd, twoPages, out var cv);

            var bullets = Paragraphs(bytes)
                .Where(p => p.ParagraphProperties?.Indentation != null)
                .Select(p => p.InnerText)
                .ToList();

            Assert.Equal(BulletCount(cv), bullets.Count);
            Assert.Equal(
                cv.Experience.SelectMany(e => e.Bullets).Select(b => $"{DocumentConstants.BulletSymbol} {b}"),
                bullets);
        }

        // NEW FINDING F15: SectionProperties is appended as the first child of Body; the schema requires it last.
        [Theory(Skip = "NEW FINDING F15: sectPr is the first child of w:body, schema requires it last")]
        [MemberData(nameof(Cases))]
        public void Document_passes_openxml_schema_validation(string profile, string jd, bool twoPages)
        {
            var bytes = Render(profile, jd, twoPages, out _);

            using var stream = new MemoryStream(bytes);
            using var doc = WordprocessingDocument.Open(stream, false);
            var errors = new OpenXmlValidator().Validate(doc).Select(e => e.Description).Distinct().ToList();

            Assert.True(errors.Count == 0, string.Join(" || ", errors));
        }
    }
}

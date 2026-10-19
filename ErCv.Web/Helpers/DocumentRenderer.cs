using ErCvApi.Models;
using ErCvApi.Constants;
using Stubble.Core.Builders;
using PuppeteerSharp;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CvHeader = ErCvApi.Models.Header;
using PuppeteerSharp.Media;
using System.Threading;

namespace ErCvApi.Helpers
{
    public class DocumentRenderer
    {
        private readonly string _templatePath;
        private readonly StubbleBuilder _stubble;
        private static readonly SemaphoreSlim BrowserInitSemaphore = new(1, 1);
        private static Task<IBrowser>? _sharedBrowserTask;
        private static bool _chromiumReady;

        public DocumentRenderer(string templatePath)
        {
            _templatePath = templatePath;
            _stubble = new StubbleBuilder();
        }
        public async Task<byte[]> RenderPdfAsync(GeneratedCV cv)
        {
            var html = RenderHtml(cv);
            var browser = await GetSharedBrowserAsync();
            var page = await browser.NewPageAsync();

            try
            {
                await page.SetContentAsync(html);

                return await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.Letter,
                    MarginOptions = new MarginOptions
                    {
                        Top = DocumentConstants.PdfMarginTop,
                        Bottom = DocumentConstants.PdfMarginBottom,
                        Left = DocumentConstants.PdfMarginLeft,
                        Right = DocumentConstants.PdfMarginRight
                    },
                    PrintBackground = true
                });
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        private static async Task<IBrowser> GetSharedBrowserAsync()
        {
            if (_sharedBrowserTask != null)
            {
                var existing = await _sharedBrowserTask;
                if (!existing.IsClosed)
                    return existing;
            }

            await BrowserInitSemaphore.WaitAsync();
            try
            {
                if (_sharedBrowserTask != null)
                {
                    var cached = await _sharedBrowserTask;
                    if (!cached.IsClosed)
                        return cached;
                }

                var executablePath = Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

                if (string.IsNullOrWhiteSpace(executablePath) && !_chromiumReady)
                {
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _chromiumReady = true;
                }

                _sharedBrowserTask = Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = string.IsNullOrWhiteSpace(executablePath) ? null : executablePath,
                    Args = DocumentConstants.ChromiumArgs
                });

                return await _sharedBrowserTask;
            }
            finally
            {
                BrowserInitSemaphore.Release();
            }
        }
        public string RenderHtmlPublic(GeneratedCV cv) => RenderHtml(cv);
        public byte[] RenderDocx(GeneratedCV cv)
        {
            using var memoryStream = new MemoryStream();

            using (var document = WordprocessingDocument.Create(
                memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(new SectionProperties(
                    new PageMargin { Top = 720, Bottom = 720, Left = 1080, Right = 1080 }));

                AddHeader(body, cv.Header);

                if (!string.IsNullOrEmpty(cv.Summary))
                {
                    AddSectionTitle(body, DocumentConstants.SectionSummary);
                    AddParagraph(body, cv.Summary);
                }

                AddSectionTitle(body, DocumentConstants.SectionEducation);
                foreach (var edu in cv.Education ?? new List<Education>())
                    AddEducation(body, edu);

                AddSectionTitle(body, DocumentConstants.SectionExperience);
                foreach (var exp in cv.Experience ?? new List<Experience>())
                    AddExperience(body, exp);

                if (cv.Skills?.Any() == true)
                {
                    AddSectionTitle(body, DocumentConstants.SectionSkills);
                    AddSkills(body, cv.Skills);
                }

                if (cv.Projects?.Any() == true)
                {
                    AddSectionTitle(body, DocumentConstants.SectionProjects);
                    foreach (var project in cv.Projects)
                        AddProject(body, project);
                }

                mainPart.Document.Save();
            }

            return memoryStream.ToArray();
        }
        private string RenderHtml(GeneratedCV cv)
        {
            var template = File.ReadAllText(_templatePath);
            var skillCategories = BuildSkillCategories(cv.Skills);

            var data = new
            {
                FullName = cv.Header?.FullName ?? "",
                Email = cv.Header?.Email ?? "",
                Phone = cv.Header?.Phone ?? "",
                Location = cv.Header?.Location ?? "",
                LinkedIn = cv.Header?.LinkedIn ?? "",
                GitHub = cv.Header?.GitHub ?? "",
                HasSummary = !string.IsNullOrEmpty(cv.Summary),
                Summary = cv.Summary ?? "",
                Education = cv.Education ?? new List<Education>(),
                Experience = cv.Experience ?? new List<Experience>(),
                HasSkills = skillCategories.Any(),
                SkillCategories = skillCategories,
                HasProjects = cv.Projects != null && cv.Projects.Any(),
                Projects = (cv.Projects ?? new List<Project>()).Select(p => new
                {
                    p.Name,
                    p.Link,
                    p.Description,
                    Technologies = p.Technologies?.Any() == true
                        ? string.Join(", ", p.Technologies)
                        : (string?)null
                }).ToList()
            };

            return _stubble.Build().Render(template, data);
        }

        private List<object> BuildSkillCategories(List<string>? skills)
        {
            var categories = new List<object>();
            if (skills == null || !skills.Any()) return categories;

            var languageSet = new HashSet<string>(DocumentConstants.LanguageKeywords, StringComparer.OrdinalIgnoreCase);
            var softSet = new HashSet<string>(DocumentConstants.SoftSkillKeywords, StringComparer.OrdinalIgnoreCase);

            var technical = new List<string>();
            var languages = new List<string>();
            var softSkills = new List<string>();

            foreach (var skill in skills)
            {
                if (languageSet.Contains(skill)) languages.Add(skill);
                else if (softSet.Contains(skill)) softSkills.Add(skill);
                else technical.Add(skill);
            }

            if (technical.Any())
                categories.Add(new { CategoryName = DocumentConstants.CategoryTechnical, CategoryItems = string.Join(", ", technical) });
            if (languages.Any())
                categories.Add(new { CategoryName = DocumentConstants.CategoryLanguages, CategoryItems = string.Join(", ", languages) });
            if (softSkills.Any())
                categories.Add(new { CategoryName = DocumentConstants.CategorySoftSkills, CategoryItems = string.Join(", ", softSkills) });

            return categories;
        }
        private void AddHeader(Body body, CvHeader? header)
        {
            if (header == null) return;

            var namePara = body.AppendChild(new Paragraph());
            namePara.ParagraphProperties = new ParagraphProperties(
                new Justification { Val = JustificationValues.Center });
            var nameRun = namePara.AppendChild(new Run());
            nameRun.AppendChild(new RunProperties(new Bold(), new FontSize { Val = DocumentConstants.FontSizeName }));
            nameRun.AppendChild(new Text(header.FullName?.ToUpper() ?? ""));

            var contactParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(header.Email)) contactParts.Add(header.Email);
            if (!string.IsNullOrWhiteSpace(header.Phone)) contactParts.Add(header.Phone);
            if (!string.IsNullOrWhiteSpace(header.Location)) contactParts.Add(header.Location);
            if (!string.IsNullOrWhiteSpace(header.LinkedIn)) contactParts.Add(header.LinkedIn);
            if (!string.IsNullOrWhiteSpace(header.GitHub)) contactParts.Add(header.GitHub);

            var contactPara = body.AppendChild(new Paragraph());
            contactPara.ParagraphProperties = new ParagraphProperties(
                new Justification { Val = JustificationValues.Center });
            var cRun = contactPara.AppendChild(new Run());
            cRun.AppendChild(new RunProperties(new FontSize { Val = DocumentConstants.FontSizeContact }));
            cRun.AppendChild(new Text(string.Join($" {DocumentConstants.BulletSymbol} ", contactParts)));

            body.AppendChild(new Paragraph());
        }

        private void AddSectionTitle(Body body, string title)
        {
            var para = body.AppendChild(new Paragraph());
            para.ParagraphProperties = new ParagraphProperties(
                new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 6 }));
            var run = para.AppendChild(new Run());
            run.AppendChild(new RunProperties(new Bold(), new FontSize { Val = DocumentConstants.FontSizeSection }));
            run.AppendChild(new Text(title));
        }

        private void AddParagraph(Body body, string text)
        {
            body.AppendChild(new Paragraph()).AppendChild(new Run()).AppendChild(new Text(text));
        }

        private void AddEducation(Body body, Education edu)
        {
            var para = body.AppendChild(new Paragraph());
            var r1 = para.AppendChild(new Run());
            r1.AppendChild(new RunProperties(new Bold()));
            r1.AppendChild(new Text($"{edu.Institution}, {edu.Location}"));
            para.AppendChild(new Run()).AppendChild(new Text($"     {edu.GraduationDate}"));

            var details = edu.Degree ?? "";
            if (!string.IsNullOrWhiteSpace(edu.GPA)) details += $" | GPA: {edu.GPA}";
            if (!string.IsNullOrWhiteSpace(edu.Honors)) details += $" | {edu.Honors}";

            var detPara = body.AppendChild(new Paragraph());
            var r3 = detPara.AppendChild(new Run());
            r3.AppendChild(new RunProperties(new Italic()));
            r3.AppendChild(new Text(details));
        }

        private void AddExperience(Body body, Experience exp)
        {
            var para = body.AppendChild(new Paragraph());
            var r1 = para.AppendChild(new Run());
            r1.AppendChild(new RunProperties(new Bold()));
            r1.AppendChild(new Text(exp.Role ?? ""));
            para.AppendChild(new Run()).AppendChild(new Text($"     {exp.StartDate} – {exp.EndDate}"));

            var sub = body.AppendChild(new Paragraph());
            var r3 = sub.AppendChild(new Run());
            r3.AppendChild(new RunProperties(new Italic()));
            r3.AppendChild(new Text($"{exp.Company}, {exp.Location}"));

            foreach (var bullet in exp.Bullets ?? new List<string>())
            {
                var bp = body.AppendChild(new Paragraph());
                bp.AppendChild(new ParagraphProperties(
                    new Indentation { Left = DocumentConstants.IndentationLeft }));
                bp.AppendChild(new Run()).AppendChild(new Text($"{DocumentConstants.BulletSymbol} {bullet}"));
            }
        }

        private void AddSkills(Body body, List<string> skills)
        {
            body.AppendChild(new Paragraph())
                .AppendChild(new Run())
                .AppendChild(new Text(string.Join($" {DocumentConstants.BulletSymbol} ", skills)));
        }

        private void AddProject(Body body, Project project)
        {
            var para = body.AppendChild(new Paragraph());
            var r = para.AppendChild(new Run());
            r.AppendChild(new RunProperties(new Bold()));
            var title = project.Name ?? "";
            if (!string.IsNullOrWhiteSpace(project.Link)) title += $" | {project.Link}";
            r.AppendChild(new Text(title));

            if (!string.IsNullOrEmpty(project.Description))
                body.AppendChild(new Paragraph()).AppendChild(new Run()).AppendChild(new Text(project.Description));

            if (project.Technologies?.Any() == true)
            {
                var tp = body.AppendChild(new Paragraph());
                var tr = tp.AppendChild(new Run());
                tr.AppendChild(new RunProperties(new Italic()));
                tr.AppendChild(new Text($"Technologies: {string.Join(", ", project.Technologies)}"));
            }
        }
    }
}

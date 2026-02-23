using ErCvApi.Models;
using ErCvApi.Constants;
using Stubble.Core.Builders;
using PuppeteerSharp;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using CvHeader = ErCvApi.Models.Header;
using PuppeteerSharp.Media;

namespace ErCvApi.Helpers
{
    public class DocumentRenderer
    {
        private readonly string _templatePath;
        private readonly StubbleBuilder _stubble;

        public DocumentRenderer(string templatePath)
        {
            _templatePath = templatePath;
            _stubble = new StubbleBuilder();
        }

        public async Task<byte[]> RenderPdfAsync(GeneratedCV cv)
        {
            var html = RenderHtml(cv);

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = DocumentConstants.ChromiumArgs
            });

            var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
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

            await browser.CloseAsync();
            return pdfBytes;
        }

        public byte[] RenderDocx(GeneratedCV cv)
        {
            using var memoryStream = new MemoryStream();

            using (var document = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                AddHeader(body, cv.Header);

                AddSectionTitle(body, DocumentConstants.SectionEducation);
                foreach (var edu in cv.Education)
                    AddEducation(body, edu);

                AddSectionTitle(body, DocumentConstants.SectionExperience);
                foreach (var exp in cv.Experience)
                    AddExperience(body, exp);

                if (cv.Skills.Any())
                {
                    AddSectionTitle(body, DocumentConstants.SectionSkills);
                    AddSkills(body, cv.Skills);
                }

                if (cv.Projects.Any())
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

            var skillCategories = new List<object>();

            if (cv.Skills != null && cv.Skills.Any())
            {
                var technicalSkills = new List<string>();
                var languages = new List<string>();
                var softSkills = new List<string>();

                var languageKeywords = new HashSet<string>(
                    DocumentConstants.LanguageKeywords,
                    StringComparer.OrdinalIgnoreCase);

                var softSkillKeywords = new HashSet<string>(
                    DocumentConstants.SoftSkillKeywords,
                    StringComparer.OrdinalIgnoreCase);

                foreach (var skill in cv.Skills)
                {
                    if (languageKeywords.Contains(skill))
                        languages.Add(skill);
                    else if (softSkillKeywords.Contains(skill))
                        softSkills.Add(skill);
                    else
                        technicalSkills.Add(skill);
                }

                if (technicalSkills.Any())
                {
                    skillCategories.Add(new
                    {
                        CategoryName = DocumentConstants.CategoryTechnical,
                        CategoryItems = string.Join(", ", technicalSkills)
                    });
                }

                if (languages.Any())
                {
                    skillCategories.Add(new
                    {
                        CategoryName = DocumentConstants.CategoryLanguages,
                        CategoryItems = string.Join(", ", languages)
                    });
                }

                if (softSkills.Any())
                {
                    skillCategories.Add(new
                    {
                        CategoryName = DocumentConstants.CategorySoftSkills,
                        CategoryItems = string.Join(", ", softSkills)
                    });
                }
            }

            var data = new
            {
                FullName = cv.Header.FullName,
                Email = cv.Header.Email,
                Phone = cv.Header.Phone,
                Location = cv.Header.Location,
                LinkedIn = cv.Header.LinkedIn,
                GitHub = cv.Header.GitHub,
                HasSummary = !string.IsNullOrEmpty(cv.Summary),
                Summary = cv.Summary,
                Education = cv.Education,
                Experience = cv.Experience,
                HasSkills = skillCategories.Any(),
                SkillCategories = skillCategories,
                HasProjects = cv.Projects != null && cv.Projects.Any(),
                Projects = cv.Projects?.Select(p => new
                {
                    p.Name,
                    p.Link,
                    p.Description,
                    Technologies = p.Technologies != null && p.Technologies.Any()
                        ? string.Join(", ", p.Technologies)
                        : null
                })
            };

            var stubble = _stubble.Build();
            return stubble.Render(template, data);
        }

        private void AddHeader(Body body, CvHeader header)
        {
            var namePara = body.AppendChild(new Paragraph());
            var nameRun = namePara.AppendChild(new Run());
            var nameProps = nameRun.AppendChild(new RunProperties());
            nameProps.AppendChild(new Bold());
            nameProps.AppendChild(new FontSize { Val = DocumentConstants.FontSizeName });
            nameRun.AppendChild(new Text(header.FullName.ToUpper()));
            namePara.ParagraphProperties =
                new ParagraphProperties(new Justification { Val = JustificationValues.Center });

            var contactPara = body.AppendChild(new Paragraph());
            var contactRun = contactPara.AppendChild(new Run());
            var contactProps = contactRun.AppendChild(new RunProperties());
            contactProps.AppendChild(new FontSize { Val = DocumentConstants.FontSizeContact });

            var contactParts = new List<string>
            {
                header.Email,
                header.Phone,
                header.Location
            };

            if (!string.IsNullOrWhiteSpace(header.LinkedIn))
                contactParts.Add(header.LinkedIn);

            if (!string.IsNullOrWhiteSpace(header.GitHub))
                contactParts.Add(header.GitHub);

            var contactText = string.Join(
                $" {DocumentConstants.BulletSymbol} ",
                contactParts);

            contactRun.AppendChild(new Text(contactText));
            contactPara.ParagraphProperties =
                new ParagraphProperties(new Justification { Val = JustificationValues.Center });

            body.AppendChild(new Paragraph());
        }

        private void AddSectionTitle(Body body, string title)
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            var props = run.AppendChild(new RunProperties());
            props.AppendChild(new Bold());
            props.AppendChild(new FontSize { Val = DocumentConstants.FontSizeSection });
            run.AppendChild(new Text(title));

            para.ParagraphProperties = new ParagraphProperties(
                new ParagraphBorders(
                    new BottomBorder { Val = BorderValues.Single, Size = 6 }
                )
            );
        }

        private void AddEducation(Body body, Education edu)
        {
            var headerPara = body.AppendChild(new Paragraph());
            var run1 = headerPara.AppendChild(new Run());
            var props1 = run1.AppendChild(new RunProperties());
            props1.AppendChild(new Bold());
            run1.AppendChild(new Text($"{edu.Institution}, {edu.Location}"));

            headerPara.AppendChild(new Run())
                .AppendChild(new Text($"     {edu.GraduationDate}"));

            var detailsPara = body.AppendChild(new Paragraph());
            var run3 = detailsPara.AppendChild(new Run());
            var props3 = run3.AppendChild(new RunProperties());
            props3.AppendChild(new Italic());

            var details = edu.Degree;

            if (!string.IsNullOrWhiteSpace(edu.GPA))
                details += $" | GPA: {edu.GPA}";

            if (!string.IsNullOrWhiteSpace(edu.Honors))
                details += $" | {edu.Honors}";

            run3.AppendChild(new Text(details));
        }

        private void AddExperience(Body body, Experience exp)
        {
            var headerPara = body.AppendChild(new Paragraph());
            var run1 = headerPara.AppendChild(new Run());
            var props1 = run1.AppendChild(new RunProperties());
            props1.AppendChild(new Bold());
            run1.AppendChild(new Text(exp.Role));

            headerPara.AppendChild(new Run())
                .AppendChild(new Text($"     {exp.StartDate} – {exp.EndDate}"));

            var subPara = body.AppendChild(new Paragraph());
            var run3 = subPara.AppendChild(new Run());
            var props3 = run3.AppendChild(new RunProperties());
            props3.AppendChild(new Italic());
            run3.AppendChild(new Text($"{exp.Company}, {exp.Location}"));

            foreach (var bullet in exp.Bullets)
            {
                var bulletPara = body.AppendChild(new Paragraph());
                var bulletProps = bulletPara.AppendChild(new ParagraphProperties());
                bulletProps.AppendChild(
                    new Indentation { Left = DocumentConstants.IndentationLeft });

                bulletPara.AppendChild(new Run())
                    .AppendChild(new Text($"{DocumentConstants.BulletSymbol} {bullet}"));
            }
        }

        private void AddSkills(Body body, List<string> skills)
        {
            var para = body.AppendChild(new Paragraph());
            para.AppendChild(new Run())
                .AppendChild(new Text(
                    string.Join(
                        $" {DocumentConstants.BulletSymbol} ",
                        skills)));
        }

        private void AddProject(Body body, Project project)
        {
            var titlePara = body.AppendChild(new Paragraph());
            var titleRun = titlePara.AppendChild(new Run());
            var titleProps = titleRun.AppendChild(new RunProperties());
            titleProps.AppendChild(new Bold());

            var titleText = project.Name;
            if (!string.IsNullOrWhiteSpace(project.Link))
                titleText += $" | {project.Link}";

            titleRun.AppendChild(new Text(titleText));

            body.AppendChild(new Paragraph())
                .AppendChild(new Run())
                .AppendChild(new Text(project.Description));

            if (project.Technologies != null && project.Technologies.Any())
            {
                var techPara = body.AppendChild(new Paragraph());
                var techRun = techPara.AppendChild(new Run());
                var techProps = techRun.AppendChild(new RunProperties());
                techProps.AppendChild(new Italic());
                techRun.AppendChild(new Text(
                    $"Technologies: {string.Join(", ", project.Technologies)}"));
            }
        }
    }
}
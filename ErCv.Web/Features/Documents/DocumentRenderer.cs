using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ErCv.Web.Constants;
using ErCv.Web.Models;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Stubble.Core;
using Stubble.Core.Builders;
using CvHeader = ErCv.Web.Models.Header;

namespace ErCv.Web.Features.Documents;

public partial class DocumentRenderer
{
    #region Constants

    private static readonly HashSet<string> LanguageSet =
        new(DocumentConstants.LanguageKeywords, StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> SoftSkillSet =
        new(DocumentConstants.SoftSkillKeywords, StringComparer.OrdinalIgnoreCase);

    // Templates ship with the app and never change while it runs, so each one is read from disk once.
    // The Stubble renderer is thread safe and caches parsed templates, so one instance serves every request.
    private static readonly ConcurrentDictionary<string, string> TemplateCache = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> PartialsCache = new();
    private static readonly StubbleVisitorRenderer Stubble = new StubbleBuilder().Build();

    // Every page object of a PDF carries "/Type /Page" ("/Pages" is the page tree, so a letter may not follow).
    // Chromium writes these dictionaries uncompressed, so counting them is enough and needs no PDF library.
    [GeneratedRegex(@"/Type\s*/Page(?![A-Za-z])")]
    private static partial Regex PageObject();

    private static readonly SemaphoreSlim BrowserInitSemaphore = new(1, 1);
    private static readonly SemaphoreSlim PdfRenderSlots =
        new(DocumentConstants.MaxConcurrentPdfRenders, DocumentConstants.MaxConcurrentPdfRenders);

    #endregion

    #region Fields

    private static Task<IBrowser>? _sharedBrowserTask;
    private static bool _chromiumReady;
    private static string? _downloadedExecutablePath;

    private readonly string _templatePath;
    private readonly string _partialsFolder;
    private readonly CVTemplate _template;

    #endregion

    #region Constructors

    public DocumentRenderer(string templatePath, CVTemplate template = CVTemplate.Harvard)
    {
        _templatePath = templatePath;
        _partialsFolder = Path.Combine(Path.GetDirectoryName(templatePath) ?? string.Empty, TemplatePaths.PartialsFolder);
        _template = template;
    }

    #endregion

    #region Public Methods

    /// <summary>The renderer for a template: its file in <paramref name="templatesFolder"/> and the order it prints sections in.</summary>
    public static DocumentRenderer ForTemplate(string templatesFolder, CVTemplate template)
    {
        var file = template switch
        {
            CVTemplate.Modern => TemplatePaths.ModernTemplate,
            CVTemplate.Minimalist => TemplatePaths.MinimalistTemplate,
            CVTemplate.Compact => TemplatePaths.CompactTemplate,
            CVTemplate.Executive => TemplatePaths.ExecutiveTemplate,
            _ => TemplatePaths.HarvardTemplate
        };
        return new DocumentRenderer(Path.Combine(templatesFolder, file), template);
    }

    public static IReadOnlyList<string> DefaultSectionOrder(CVTemplate template) => template switch
    {
        CVTemplate.Modern => DocumentConstants.ModernSectionOrder,
        CVTemplate.Minimalist => DocumentConstants.MinimalistSectionOrder,
        CVTemplate.Compact => DocumentConstants.CompactSectionOrder,
        CVTemplate.Executive => DocumentConstants.ExecutiveSectionOrder,
        _ => DocumentConstants.HarvardSectionOrder
    };

    /// <summary>The title the template gives the summary: the formal one, the short one or the executive one.</summary>
    public static string SummaryTitle(CVTemplate template, CvLabels labels) => template switch
    {
        CVTemplate.Modern or CVTemplate.Minimalist => labels.Profile,
        CVTemplate.Executive => labels.ExecutiveSummary,
        _ => labels.Summary
    };

    /// <summary>
    /// The user's order with unknown keys and repeats dropped, then every section it left out in the default order,
    /// so a stale or partial order (an older history entry, an API call) still prints every section once.
    /// </summary>
    public static List<string> ResolveSectionOrder(IEnumerable<string>? chosen, IReadOnlyList<string> defaults)
    {
        var order = (chosen ?? Enumerable.Empty<string>()).Where(defaults.Contains).Distinct().ToList();
        order.AddRange(defaults.Where(key => !order.Contains(key)));
        return order;
    }

    public Task<byte[]> RenderPdfAsync(GeneratedCV cv) => RenderPdfAsync(RenderHtml(cv));

    /// <summary>Prints HTML already produced by <see cref="RenderHtml"/>, so a caller that also shows a preview renders it once.</summary>
    public async Task<byte[]> RenderPdfAsync(string html)
    {
        await PdfRenderSlots.WaitAsync();
        try
        {
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
        finally
        {
            PdfRenderSlots.Release();
        }
    }

    /// <summary>Number of pages in a PDF produced by <see cref="RenderPdfAsync(string)"/> (MATCH-08 page fit).</summary>
    public static int CountPages(byte[] pdf) => PageObject().Count(Encoding.Latin1.GetString(pdf));

    public static async Task ShutdownAsync()
    {
        if (_sharedBrowserTask == null)
            return;

        try
        {
            var browser = await _sharedBrowserTask;
            await browser.CloseAsync();
        }
        catch
        {
        }
    }

    public string RenderHtml(GeneratedCV cv)
    {
        var template = TemplateCache.GetOrAdd(_templatePath, File.ReadAllText);
        var partials = PartialsCache.GetOrAdd(_partialsFolder, LoadPartials);
        var labels = CvLabels.For(cv.Language);
        var skillCategories = BuildSkillCategories(cv.Skills, labels);
        var header = cv.Header ?? new CvHeader();

        var data = new
        {
            Lang = labels.Language,
            StyleClasses = StyleClasses(cv),
            Labels = labels,
            SummaryTitle = SummaryTitle(_template, labels),
            FullName = header.FullName ?? "",
            ContactItems = ContactItems(header),
            Sections = ResolveSectionOrder(cv.SectionOrder, DefaultSectionOrder(_template)).Select(key => new
            {
                IsSummary = key == DocumentConstants.SectionSummaryKey,
                IsEducation = key == DocumentConstants.SectionEducationKey,
                IsExperience = key == DocumentConstants.SectionExperienceKey,
                IsSkills = key == DocumentConstants.SectionSkillsKey,
                IsProjects = key == DocumentConstants.SectionProjectsKey
            }).ToList(),
            HasSummary = !string.IsNullOrEmpty(cv.Summary),
            Summary = cv.Summary ?? "",
            Education = (cv.Education ?? new List<Education>()).Select(e => new
            {
                Place = JoinPresent(", ", e.Institution, e.Location),
                e.Degree,
                GraduationDate = labels.FormatDate(e.GraduationDate)
            }).ToList(),
            Experience = (cv.Experience ?? new List<Experience>()).Select(e => new
            {
                e.Role,
                CompanyLine = JoinPresent(", ", e.Company, e.Location),
                DateRange = labels.DateRange(e.StartDate, e.EndDate),
                Bullets = e.Bullets ?? new List<string>()
            }).ToList(),
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

        return Stubble.Render(template, data, partials);
    }

    public byte[] RenderDocx(GeneratedCV cv)
    {
        using var memoryStream = new MemoryStream();

        using (var document = WordprocessingDocument.Create(
            memoryStream, WordprocessingDocumentType.Document))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            AddHeader(body, cv.Header);

            // The DOCX has one layout, so without a chosen order it uses Harvard's
            var labels = CvLabels.For(cv.Language);
            foreach (var section in ResolveSectionOrder(cv.SectionOrder, DocumentConstants.HarvardSectionOrder))
                AddSection(body, cv, section, labels);

            // The schema requires the section properties to be the last child of the body
            body.AppendChild(new SectionProperties(
                new PageMargin
                {
                    Top = DocumentConstants.DocxMarginVerticalTwips,
                    Bottom = DocumentConstants.DocxMarginVerticalTwips,
                    Left = DocumentConstants.DocxMarginHorizontalTwips,
                    Right = DocumentConstants.DocxMarginHorizontalTwips
                }));

            mainPart.Document.Save();
        }

        return memoryStream.ToArray();
    }

    #endregion

    #region Private Methods

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

            var executablePath = Environment.GetEnvironmentVariable(DocumentConstants.ExecutablePathVariable);

            if (string.IsNullOrWhiteSpace(executablePath) && !_chromiumReady)
            {
                var cachePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    DocumentConstants.ChromiumCacheAppFolder, DocumentConstants.ChromiumCacheFolder);
                var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = cachePath });
                var installed = await browserFetcher.DownloadAsync();
                _downloadedExecutablePath = installed.GetExecutablePath();
                _chromiumReady = true;
            }

            if (string.IsNullOrWhiteSpace(executablePath))
                executablePath = _downloadedExecutablePath;

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

    /// <summary>Every partial of the templates folder, by file name without the extension.</summary>
    private static Dictionary<string, string> LoadPartials(string folder) =>
        Directory.Exists(folder)
            ? Directory.GetFiles(folder, "*.html").ToDictionary(f => Path.GetFileNameWithoutExtension(f), File.ReadAllText)
            : new Dictionary<string, string>();

    /// <summary>Classes for the accent and font pairing, only for known keys, so nothing a request sends reaches the CSS.</summary>
    private static string StyleClasses(GeneratedCV cv)
    {
        var classes = new List<string>();
        if (cv.Accent != ApiConstants.StyleDefault && ApiConstants.Accents.Contains(cv.Accent))
            classes.Add(DocumentConstants.AccentClassPrefix + cv.Accent);
        if (cv.Fonts != ApiConstants.StyleDefault && ApiConstants.FontPairings.Contains(cv.Fonts))
            classes.Add(DocumentConstants.FontsClassPrefix + cv.Fonts);
        return string.Join(" ", classes);
    }

    private static List<object> ContactItems(CvHeader header)
    {
        var items = new List<object>();
        void Add(string? text, bool mail = false, bool web = false)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            items.Add(new { Text = text.Trim(), IsMail = mail, IsWeb = web, IsPlain = !mail && !web });
        }

        Add(header.Email, mail: true);
        Add(header.Phone);
        Add(header.Location);
        Add(header.LinkedIn, web: true);
        Add(header.GitHub, web: true);
        return items;
    }

    /// <summary>Joins the parts that have text, so a missing location leaves no stray comma.</summary>
    private static string JoinPresent(string separator, params string?[] parts) =>
        string.Join(separator, parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static List<object> BuildSkillCategories(List<string>? skills, CvLabels labels)
    {
        var categories = new List<object>();
        if (skills == null || !skills.Any()) return categories;

        var technical = new List<string>();
        var languages = new List<string>();
        var softSkills = new List<string>();

        foreach (var skill in skills)
        {
            if (LanguageSet.Contains(skill)) languages.Add(skill);
            else if (SoftSkillSet.Contains(skill)) softSkills.Add(skill);
            else technical.Add(skill);
        }

        if (technical.Any())
            categories.Add(new { CategoryName = labels.Technical, CategoryItems = string.Join(", ", technical) });
        if (languages.Any())
            categories.Add(new { CategoryName = labels.ProgrammingLanguages, CategoryItems = string.Join(", ", languages) });
        if (softSkills.Any())
            categories.Add(new { CategoryName = labels.SoftSkills, CategoryItems = string.Join(", ", softSkills) });

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

    private void AddSection(Body body, GeneratedCV cv, string section, CvLabels labels)
    {
        switch (section)
        {
            case DocumentConstants.SectionSummaryKey when !string.IsNullOrEmpty(cv.Summary):
                AddSectionTitle(body, labels.Summary);
                AddParagraph(body, cv.Summary);
                break;
            case DocumentConstants.SectionEducationKey:
                AddSectionTitle(body, labels.Education);
                foreach (var edu in cv.Education ?? new List<Education>())
                    AddEducation(body, edu, labels);
                break;
            case DocumentConstants.SectionExperienceKey:
                AddSectionTitle(body, labels.Experience);
                foreach (var exp in cv.Experience ?? new List<Experience>())
                    AddExperience(body, exp, labels);
                break;
            case DocumentConstants.SectionSkillsKey when cv.Skills?.Any() == true:
                AddSectionTitle(body, labels.Skills);
                AddSkills(body, cv.Skills);
                break;
            case DocumentConstants.SectionProjectsKey when cv.Projects?.Any() == true:
                AddSectionTitle(body, labels.Projects);
                foreach (var project in cv.Projects)
                    AddProject(body, project, labels);
                break;
        }
    }

    private void AddSectionTitle(Body body, string title)
    {
        var para = body.AppendChild(new Paragraph());
        para.ParagraphProperties = new ParagraphProperties(
            new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = DocumentConstants.SectionBorderSize }));
        var run = para.AppendChild(new Run());
        run.AppendChild(new RunProperties(new Bold(), new FontSize { Val = DocumentConstants.FontSizeSection }));
        run.AppendChild(new Text(title.ToUpperInvariant()));
    }

    private void AddParagraph(Body body, string text)
    {
        body.AppendChild(new Paragraph()).AppendChild(new Run()).AppendChild(new Text(text));
    }

    private void AddEducation(Body body, Education edu, CvLabels labels)
    {
        var para = body.AppendChild(new Paragraph());
        var r1 = para.AppendChild(new Run());
        r1.AppendChild(new RunProperties(new Bold()));
        r1.AppendChild(new Text(JoinPresent(", ", edu.Institution, edu.Location)));
        para.AppendChild(new Run()).AppendChild(new Text($"{DocumentConstants.DateGap}{labels.FormatDate(edu.GraduationDate)}"));

        var details = edu.Degree ?? "";
        if (!string.IsNullOrWhiteSpace(edu.GPA)) details += DocumentConstants.DetailSeparator + labels.Gpa + edu.GPA;
        if (!string.IsNullOrWhiteSpace(edu.Honors)) details += DocumentConstants.DetailSeparator + edu.Honors;

        var detPara = body.AppendChild(new Paragraph());
        var r3 = detPara.AppendChild(new Run());
        r3.AppendChild(new RunProperties(new Italic()));
        r3.AppendChild(new Text(details));
    }

    private void AddExperience(Body body, Experience exp, CvLabels labels)
    {
        var para = body.AppendChild(new Paragraph());
        var r1 = para.AppendChild(new Run());
        r1.AppendChild(new RunProperties(new Bold()));
        r1.AppendChild(new Text(exp.Role ?? ""));
        para.AppendChild(new Run()).AppendChild(new Text($"{DocumentConstants.DateGap}{labels.DateRange(exp.StartDate, exp.EndDate)}"));

        var sub = body.AppendChild(new Paragraph());
        var r3 = sub.AppendChild(new Run());
        r3.AppendChild(new RunProperties(new Italic()));
        r3.AppendChild(new Text(JoinPresent(", ", exp.Company, exp.Location)));

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

    private void AddProject(Body body, Project project, CvLabels labels)
    {
        var para = body.AppendChild(new Paragraph());
        var r = para.AppendChild(new Run());
        r.AppendChild(new RunProperties(new Bold()));
        var title = project.Name ?? "";
        if (!string.IsNullOrWhiteSpace(project.Link)) title += DocumentConstants.DetailSeparator + project.Link;
        r.AppendChild(new Text(title));

        if (!string.IsNullOrEmpty(project.Description))
            body.AppendChild(new Paragraph()).AppendChild(new Run()).AppendChild(new Text(project.Description));

        if (project.Technologies?.Any() == true)
        {
            var tp = body.AppendChild(new Paragraph());
            var tr = tp.AppendChild(new Run());
            tr.AppendChild(new RunProperties(new Italic()));
            tr.AppendChild(new Text($"{labels.Technologies}{string.Join(", ", project.Technologies)}"));
        }
    }

    #endregion
}

using System.Collections.Concurrent;
using System.Text.Json;
using ErCv.Web.Constants;
using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ErCv.Web.Services;

public class CVGeneratorService
{
    #region Constants

    private static readonly HashSet<string> SupportedFormats =
        new(StringComparer.OrdinalIgnoreCase) { ApiConstants.FormatPdf, ApiConstants.FormatDocx };

    private static readonly string AllowedFormatsText = string.Join(", ", SupportedFormats);

    private static readonly JsonSerializerOptions ProfileReadOptions = new() { PropertyNameCaseInsensitive = true };

    // UX-10 thumbnails: the sample CV per language and its HTML per look, built once (a few dozen small strings)
    private static readonly ConcurrentDictionary<string, Task<GeneratedCV>> SampleCvCache = new();
    private static readonly ConcurrentDictionary<string, string> SampleHtmlCache = new();

    #endregion

    #region Fields

    private readonly JobDescriptionParser _parser;
    private readonly CVSelector _selector;
    private readonly MatchScoreCalculator _matchScoreCalc;
    private readonly MissingSkillsAnalyzer _missingSkillsAnalyzer;
    private readonly KeywordAnalyzer _keywordAnalyzer;
    private readonly ATSChecker _atsChecker;
    private readonly ProfileValidator _profileValidator = new();
    private readonly JdHighlighter _jdHighlighter = new();
    private readonly PageConstraintHelper _pageConstraint = new();
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<CVGeneratorService> _logger;
    private readonly string _templatesFolder;

    #endregion

    #region Constructors

    public CVGeneratorService(IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        _env = env;
        _logger = loggerFactory.CreateLogger<CVGeneratorService>();
        _parser = new JobDescriptionParser();
        _selector = new CVSelector(loggerFactory);
        _matchScoreCalc = new MatchScoreCalculator();
        _missingSkillsAnalyzer = new MissingSkillsAnalyzer();
        _keywordAnalyzer = new KeywordAnalyzer();
        _atsChecker = new ATSChecker();
        _templatesFolder = Path.Combine(_env.ContentRootPath, TemplatePaths.TemplatesFolder);
    }

    #endregion

    #region Public Methods

    public async Task<IActionResult> GenerateAsync(GenerateRequest request, HttpContext context)
    {
        try
        {
            ValidateGenerateRequest(request);

            var (documents, analysis) = await GenerateCVInternalAsync(request);

            SaveDocumentsToSession(documents, context);
            SaveGeneratedCvToSession(analysis.GeneratedCV, context);

            return new OkObjectResult(BuildGenerateResponse(documents, analysis));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(StatusCodes.Status413PayloadTooLarge, ApiConstants.TitlePayloadTooLarge, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Problem(StatusCodes.Status400BadRequest, ApiConstants.TitleBadRequest, ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            return Problem(StatusCodes.Status400BadRequest, ApiConstants.TitleProfileNotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CV");
            return Problem(StatusCodes.Status500InternalServerError, ApiConstants.ErrorGeneratingCv);
        }
    }

    public async Task<IActionResult> RegenerateAsync(RegenerateRequest request, HttpContext context)
    {
        try
        {
            ValidateRegenerateRequest(request);

            var (documents, previewHtml) = await RegenerateCVInternalAsync(request);
            SaveDocumentsToSession(documents, context);

            return new OkObjectResult(new
            {
                success = true,
                files = BuildFileMetadata(documents),
                previewHtml
            });
        }
        catch (InvalidOperationException ex)
        {
            return Problem(StatusCodes.Status413PayloadTooLarge, ApiConstants.TitlePayloadTooLarge, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Problem(StatusCodes.Status400BadRequest, ApiConstants.TitleBadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating CV");
            return Problem(StatusCodes.Status500InternalServerError, ApiConstants.ErrorRegeneratingCv);
        }
    }

    public IActionResult Download(string format, HttpContext context)
    {
        try
        {
            var normalizedFormat = NormalizeAndValidateFormat(format);
            var fileBytes = context.Session.Get(ApiConstants.SessionCvPrefix + normalizedFormat);

            if (fileBytes == null || fileBytes.Length == 0)
                return Problem(StatusCodes.Status404NotFound, ApiConstants.TitleNotFound, ApiConstants.ErrorFileNotFound);

            return new FileContentResult(fileBytes, GetContentType(normalizedFormat))
            {
                FileDownloadName = BuildFileName(normalizedFormat)
            };
        }
        catch (ArgumentException ex)
        {
            return Problem(StatusCodes.Status400BadRequest, ApiConstants.TitleBadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return Problem(StatusCodes.Status500InternalServerError, ApiConstants.ErrorDownloadingFile);
        }
    }

    public List<ProfileInfo> ListProfiles()
    {
        var dataPath = Path.Combine(_env.ContentRootPath, TemplatePaths.DataFolder);
        return Directory.GetFiles(dataPath, ApiConstants.ProfileFilePattern)
            .Select(f =>
            {
                var id = Path.GetFileNameWithoutExtension(f);
                var displayName = id
                    .Replace(ApiConstants.ProfileIdPrefix, "")
                    .Replace(ApiConstants.DefaultProfileId, ApiConstants.DefaultProfileName);
                return new ProfileInfo(id, displayName);
            })
            .OrderBy(p => p.Id)
            .ToList();
    }

    public async Task<(Dictionary<string, byte[]> Documents, AnalysisResult Analysis)> BuildCvAsync(
        GenerateRequest request, IProgress<GenerationStage>? progress = null)
    {
        ValidateGenerateRequest(request);
        return await GenerateCVInternalAsync(request, progress);
    }

    /// <summary>
    /// Builds a CV from a profile the caller supplies (PROF-03) instead of a stored <c>profileId</c>.
    /// The profile is validated first; an invalid one raises <see cref="ArgumentException"/> listing the problems.
    /// </summary>
    public async Task<(Dictionary<string, byte[]> Documents, AnalysisResult Analysis)> BuildCvAsync(
        GenerateRequest request, CVMaster profile, IProgress<GenerationStage>? progress = null)
    {
        ValidateGenerateRequest(request);
        ValidateProfile(profile);
        return await GenerateCVInternalAsync(request, progress, profile);
    }

    /// <summary>EDIT-02: the preview of a CV being edited. HTML only, so it can follow every change.</summary>
    public string RenderPreviewHtml(GeneratedCV cv, CVTemplate template)
    {
        ValidateEditedCv(cv);
        return DocumentRenderer.ForTemplate(_templatesFolder, template).RenderHtml(cv);
    }

    /// <summary>
    /// EDIT-02: one document of an edited CV, rendered when the user downloads it. The CV is printed as the user left
    /// it, never trimmed: <c>Pages</c> lets the caller say when it runs past the page limit (0 for a DOCX).
    /// </summary>
    public async Task<(byte[] Bytes, int Pages)> RenderEditedDocumentAsync(GeneratedCV cv, CVTemplate template, string format)
    {
        ValidateEditedCv(cv);
        var normalized = NormalizeAndValidateFormat(format);
        var renderer = DocumentRenderer.ForTemplate(_templatesFolder, template);

        if (normalized == ApiConstants.FormatDocx) return (renderer.RenderDocx(cv), 0);

        var pdf = await renderer.RenderPdfAsync(renderer.RenderHtml(cv));
        return (pdf, DocumentRenderer.CountPages(pdf));
    }

    /// <summary>
    /// UX-10: a template as it really prints, filled with the made-up senior sample profile, for the template
    /// picker's thumbnails. It follows the chosen accent, fonts and language, so the picker shows the actual look.
    /// </summary>
    public async Task<string> RenderTemplateSampleAsync(CVTemplate template, string accent, string fonts, string language)
    {
        ValidateStyle(language, accent, fonts);

        var key = $"{template}|{accent}|{fonts}|{language}";
        if (SampleHtmlCache.TryGetValue(key, out var cached)) return cached;

        var sample = CvCloner.Clone(await SampleCvCache.GetOrAdd(language, SampleCvAsync));
        sample.Accent = accent;
        sample.Fonts = fonts;
        return SampleHtmlCache.GetOrAdd(key, _ => DocumentRenderer.ForTemplate(_templatesFolder, template).RenderHtml(sample));
    }

    public IActionResult GetProfiles()
    {
        try
        {
            var profiles = ListProfiles().Select(p => new { id = p.Id, name = p.Name }).ToList();

            return new OkObjectResult(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profiles");
            return Problem(StatusCodes.Status500InternalServerError, ApiConstants.ErrorLoadingProfiles);
        }
    }

    public IActionResult Health() =>
        new OkObjectResult(new { status = ApiConstants.StatusHealthy, timestamp = DateTime.UtcNow });

    /// <summary>Loads a stored profile so the visitor can export it (PROF-02).</summary>
    public Task<CVMaster> GetProfileAsync(string profileId) => LoadCVMasterAsync(profileId);

    public static string BuildFileName(string format) =>
        ApiConstants.FileNamePrefix + DateTime.Now.ToString(ApiConstants.FileNameDateFormat) + "." + format;

    #endregion

    #region Private Methods

    private async Task<(Dictionary<string, byte[]>, AnalysisResult)>
        GenerateCVInternalAsync(
            GenerateRequest request, IProgress<GenerationStage>? progress = null, CVMaster? profile = null)
    {
        await ReportAsync(progress, GenerationStage.Parsing);
        var cvMaster = profile ?? await LoadCVMasterAsync(request.ProfileId);
        var requirements = _parser.Parse(request.JobDescription);

        await ReportAsync(progress, GenerationStage.Scoring);
        var matchScore      = _matchScoreCalc.Calculate(cvMaster, requirements);
        var missingSkills   = _missingSkillsAnalyzer.Analyze(cvMaster, requirements);

        await ReportAsync(progress, GenerationStage.Selecting);
        var (selectedCV, explanation) = _selector.SelectWithExplanation(
            cvMaster, requirements, request.AllowTwoPages, request.Language);
        selectedCV.Accent = request.Accent;
        selectedCV.Fonts = request.Fonts;

        // The analysis reads the CV after the measured page fit, which can trim more than the selection did
        await ReportAsync(progress, GenerationStage.Rendering);
        var renderer = DocumentRenderer.ForTemplate(_templatesFolder, request.Template);
        var (generatedCV, previewHtml, documents) =
            await RenderDocumentsAsync(request.Formats, renderer, selectedCV, request.AllowTwoPages);
        explanation = CVSelector.FollowFinalCv(explanation, generatedCV);

        var keywordAnalysis = _keywordAnalyzer.Analyze(request.JobDescription, generatedCV);
        var atsScore        = _atsChecker.Check(generatedCV, requirements, request.Template);

        var analysis = new AnalysisResult
        {
            MatchScore      = matchScore,
            MissingSkills   = missingSkills,
            KeywordAnalysis = keywordAnalysis,
            ATSScore        = atsScore,
            GeneratedCV     = generatedCV,
            PreviewHtml     = previewHtml,
            Explanation     = explanation,
            JdHighlight     = _jdHighlighter.Build(request.JobDescription, requirements, cvMaster, generatedCV)
        };

        return (documents, analysis);
    }

    // Yielding after each report lets the UI paint the new stage before the next (synchronous) phase starts
    private static async Task ReportAsync(IProgress<GenerationStage>? progress, GenerationStage stage)
    {
        if (progress == null) return;

        progress.Report(stage);
        await Task.Yield();
    }

    private async Task<(Dictionary<string, byte[]> Documents, string PreviewHtml)> RegenerateCVInternalAsync(RegenerateRequest request)
    {
        var cv = CvCloner.Clone(request.GeneratedCV!);

        for (int i = 0; i < cv.Experience.Count; i++)
        {
            var key = ApiConstants.BulletSelectionKeyPrefix + i;
            if (request.BulletSelections.TryGetValue(key, out var selected))
            {
                var all = cv.Experience[i].Bullets;
                cv.Experience[i].Bullets = selected
                    .Where(idx => idx >= 0 && idx < all.Count)
                    .Select(idx => all[idx])
                    .ToList();
            }
        }

        cv = _pageConstraint.EnforcePageConstraint(cv, request.AllowTwoPages);

        var renderer = DocumentRenderer.ForTemplate(_templatesFolder, request.Template);
        var (_, previewHtml, documents) = await RenderDocumentsAsync(request.Formats, renderer, cv, request.AllowTwoPages);

        return (documents, previewHtml);
    }

    /// <summary>
    /// Renders the requested documents. With a PDF, the page count is measured on the real PDF and the CV trimmed
    /// further if it spills over (MATCH-08), so the preview, the PDF and the DOCX all show the fitted CV.
    /// </summary>
    private async Task<(GeneratedCV Cv, string Html, Dictionary<string, byte[]> Documents)> RenderDocumentsAsync(
        IEnumerable<string> formats, DocumentRenderer renderer, GeneratedCV cv, bool allowTwoPages)
    {
        var docs = new Dictionary<string, byte[]>();
        var normalized = formats
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(f => f.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        string html;
        if (normalized.Contains(ApiConstants.FormatPdf))
        {
            (cv, var fitted) = await _pageConstraint.FitMeasuredAsync(
                cv,
                allowTwoPages,
                async candidate =>
                {
                    var candidateHtml = renderer.RenderHtml(candidate);
                    return (Html: candidateHtml, Pdf: await renderer.RenderPdfAsync(candidateHtml));
                },
                rendered => DocumentRenderer.CountPages(rendered.Pdf));

            html = fitted.Html;
            docs[ApiConstants.FormatPdf] = fitted.Pdf;
        }
        else
        {
            html = renderer.RenderHtml(cv);
        }

        if (normalized.Contains(ApiConstants.FormatDocx))
            docs[ApiConstants.FormatDocx] = renderer.RenderDocx(cv);

        return (cv, html, docs);
    }

    private async Task<CVMaster> LoadCVMasterAsync(string profileId = ApiConstants.DefaultProfileId)
    {
        var safeId = Path.GetFileName(profileId);
        if (string.IsNullOrWhiteSpace(safeId)) safeId = ApiConstants.DefaultProfileId;

        var path = Path.Combine(_env.ContentRootPath, TemplatePaths.DataFolder, $"{safeId}.json");

        if (!File.Exists(path))
            throw new FileNotFoundException(string.Format(ApiConstants.ErrorProfileNotFound, safeId));

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<CVMaster>(json, ProfileReadOptions)
            ?? throw new InvalidOperationException(ApiConstants.ErrorProfileDeserialize);
    }

    private void ValidateGenerateRequest(GenerateRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.JobDescription))
            throw new ArgumentException(ApiConstants.ErrorJobDescriptionRequired);

        if (request.JobDescription.Length > ApiConstants.MaxJobDescriptionLength)
            throw new ArgumentException(string.Format(ApiConstants.ErrorJobDescriptionTooLong, ApiConstants.MaxJobDescriptionLength));

        if (request.Formats == null || !request.Formats.Any())
            throw new ArgumentException(ApiConstants.ErrorFormatsRequired);

        ValidateFormats(request.Formats);
        ValidateStyle(request.Language, request.Accent, request.Fonts);
    }

    private static void ValidateStyle(string? language, string? accent, string? fonts)
    {
        if (!ApiConstants.Languages.Contains(language))
            throw new ArgumentException($"Unsupported language: {language}. Allowed: {string.Join(", ", ApiConstants.Languages)}.");
        if (!ApiConstants.Accents.Contains(accent))
            throw new ArgumentException($"Unsupported accent: {accent}. Allowed: {string.Join(", ", ApiConstants.Accents)}.");
        if (!ApiConstants.FontPairings.Contains(fonts))
            throw new ArgumentException($"Unsupported fonts: {fonts}. Allowed: {string.Join(", ", ApiConstants.FontPairings)}.");
    }

    // The sample profile with no job description: what a thumbnail needs is a realistic amount of content
    private async Task<GeneratedCV> SampleCvAsync(string language)
    {
        var master = await LoadCVMasterAsync(ApiConstants.SeniorSampleProfileId);
        return _selector.SelectAndAdaptCV(master, new JobRequirements(), allowTwoPages: false, language);
    }

    private void ValidateProfile(CVMaster? profile)
    {
        var result = _profileValidator.Validate(profile);
        if (result.IsValid) return;

        var shown = result.Issues.Take(ApiConstants.MaxProfileIssuesShown).Select(i => $"{i.Path}: {i.Message}");
        var more = result.Issues.Count > ApiConstants.MaxProfileIssuesShown
            ? string.Format(ApiConstants.ErrorProfileMoreIssues, result.Issues.Count - ApiConstants.MaxProfileIssuesShown)
            : string.Empty;
        throw new ArgumentException(ApiConstants.ErrorProfileInvalid + string.Join(" ", shown) + more);
    }

    private void ValidateRegenerateRequest(RegenerateRequest request)
    {
        if (request?.GeneratedCV == null)
            throw new ArgumentException(ApiConstants.ErrorGeneratedCvRequired);

        if (request.Formats == null || !request.Formats.Any())
            throw new ArgumentException(ApiConstants.ErrorFormatsRequired);

        ValidateFormats(request.Formats);
    }

    private static void ValidateEditedCv(GeneratedCV? cv)
    {
        if (cv == null)
            throw new ArgumentException(ApiConstants.ErrorGeneratedCvRequired);

        if (cv.Experience.SelectMany(e => e.Bullets).Any(b => b.Length > ApiConstants.MaxEditedBulletLength))
            throw new ArgumentException($"A bullet can be at most {ApiConstants.MaxEditedBulletLength} characters.");
    }

    private void ValidateFormats(IEnumerable<string> formats)
    {
        var invalid = formats
            .Where(f => string.IsNullOrWhiteSpace(f) || !SupportedFormats.Contains(f.Trim()))
            .ToList();

        if (invalid.Any())
        {
            var invalidList = string.Join(", ", invalid.Select(f => string.IsNullOrWhiteSpace(f) ? "<empty>" : f));
            throw new ArgumentException(string.Format(ApiConstants.ErrorUnsupportedFormats, invalidList, AllowedFormatsText));
        }
    }

    private string NormalizeAndValidateFormat(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException(string.Format(ApiConstants.ErrorFormatRequired, AllowedFormatsText));

        var normalized = format.Trim().ToLowerInvariant();
        if (!SupportedFormats.Contains(normalized))
            throw new ArgumentException(string.Format(ApiConstants.ErrorUnsupportedFormat, format, AllowedFormatsText));

        return normalized;
    }

    private void SaveDocumentsToSession(Dictionary<string, byte[]> documents, HttpContext context)
    {
        var currentBytes = GetCurrentSessionDocumentBytes(context);

        foreach (var doc in documents)
        {
            if (doc.Value.Length > ApiConstants.MaxSessionFileSizeBytes)
                throw new InvalidOperationException(ApiConstants.ErrorSessionFileTooLarge);

            var projected = currentBytes + doc.Value.Length;
            if (projected > ApiConstants.MaxSessionTotalSizeBytes)
                throw new InvalidOperationException(ApiConstants.ErrorSessionStorageLimit);

            context.Session.Set(ApiConstants.SessionCvPrefix + doc.Key, doc.Value);
            currentBytes = projected;
        }

        _logger.LogInformation("Saved {Count} generated document(s) in session. Total session bytes: {Bytes}",
            documents.Count, currentBytes);
    }

    private static int GetCurrentSessionDocumentBytes(HttpContext context)
    {
        var total = 0;

        foreach (var format in SupportedFormats)
        {
            var bytes = context.Session.Get(ApiConstants.SessionCvPrefix + format);
            if (bytes != null) total += bytes.Length;
        }

        return total;
    }

    private void SaveGeneratedCvToSession(GeneratedCV cv, HttpContext context)
    {
        context.Session.SetString(ApiConstants.SessionGeneratedCv, JsonSerializer.Serialize(cv));
    }

    /// <summary>
    /// RFC 7807 error body. <paramref name="detail"/> is only ever a message written for the client, never an
    /// exception message. The "error" extension keeps the field existing API consumers read.
    /// </summary>
    private static ObjectResult Problem(int status, string title, string? detail = null)
    {
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        problem.Extensions["error"] = detail ?? title;
        return new ObjectResult(problem) { StatusCode = status };
    }

    private object BuildGenerateResponse(Dictionary<string, byte[]> documents, AnalysisResult analysis) =>
        new
        {
            success         = true,
            files           = BuildFileMetadata(documents),
            matchScore      = analysis.MatchScore,
            missingSkills   = analysis.MissingSkills,
            keywordAnalysis = analysis.KeywordAnalysis,
            atsScore        = analysis.ATSScore,
            generatedCV     = analysis.GeneratedCV,
            previewHtml     = analysis.PreviewHtml,   // FEATURE-102
            explanation     = analysis.Explanation
        };

    private List<object> BuildFileMetadata(Dictionary<string, byte[]> documents) =>
        documents.Select(doc => (object)new
        {
            format   = doc.Key,
            filename = BuildFileName(doc.Key),
            size     = doc.Value.Length
        }).ToList();

    private string GetContentType(string format) =>
        format.Equals(ApiConstants.FormatPdf, StringComparison.OrdinalIgnoreCase)
            ? ApiConstants.ContentTypePdf
            : ApiConstants.ContentTypeDocx;

    #endregion

    #region Nested Types

    public record ProfileInfo(string Id, string Name);

    #endregion
}

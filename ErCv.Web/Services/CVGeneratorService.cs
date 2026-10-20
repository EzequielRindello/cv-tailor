using ErCvApi.Models;
using ErCvApi.Helpers;
using ErCvApi.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ErCvApi.Services
{
    public class CVGeneratorService
    {
        private static readonly HashSet<string> SupportedFormats =
            new(StringComparer.OrdinalIgnoreCase) { ApiConstants.FormatPdf, ApiConstants.FormatDocx };

        private readonly JobDescriptionParser _parser;
        private readonly CVSelector _selector;
        private readonly MatchScoreCalculator _matchScoreCalc;
        private readonly MissingSkillsAnalyzer _missingSkillsAnalyzer;
        private readonly KeywordAnalyzer _keywordAnalyzer;
        private readonly ATSChecker _atsChecker;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CVGeneratorService> _logger;
        private readonly string _harvardTemplatePath;
        private readonly string _modernTemplatePath;
        private readonly string _minimalistTemplatePath;

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

            _harvardTemplatePath   = TemplatePath(TemplatePaths.HarvardTemplate);
            _modernTemplatePath    = TemplatePath(TemplatePaths.ModernTemplate);
            _minimalistTemplatePath= TemplatePath(TemplatePaths.MinimalistTemplate);
        }

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
                return Problem(StatusCodes.Status413PayloadTooLarge, "Payload too large", ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Problem(StatusCodes.Status400BadRequest, "Bad request", ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                return Problem(StatusCodes.Status400BadRequest, "Profile not found", ex.Message);
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
                return Problem(StatusCodes.Status413PayloadTooLarge, "Payload too large", ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Problem(StatusCodes.Status400BadRequest, "Bad request", ex.Message);
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
                    return Problem(StatusCodes.Status404NotFound, "Not found", ApiConstants.ErrorFileNotFound);

                return new FileContentResult(fileBytes, GetContentType(normalizedFormat))
                {
                    FileDownloadName = BuildFileName(normalizedFormat)
                };
            }
            catch (ArgumentException ex)
            {
                return Problem(StatusCodes.Status400BadRequest, "Bad request", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return Problem(StatusCodes.Status500InternalServerError, ApiConstants.ErrorDownloadingFile);
            }
        }
        public record ProfileInfo(string Id, string Name);

        public List<ProfileInfo> ListProfiles()
        {
            var dataPath = Path.Combine(_env.ContentRootPath, TemplatePaths.DataFolder);
            return Directory.GetFiles(dataPath, "cv-master*.json")
                .Select(f =>
                {
                    var id = Path.GetFileNameWithoutExtension(f);
                    var displayName = id
                        .Replace("cv-master-", "")
                        .Replace("cv-master", "Default");
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

        public async Task<(Dictionary<string, byte[]> Documents, string PreviewHtml)> RebuildCvAsync(RegenerateRequest request)
        {
            ValidateRegenerateRequest(request);
            return await RegenerateCVInternalAsync(request);
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
                return Problem(StatusCodes.Status500InternalServerError, "Error loading profiles");
            }
        }

        public IActionResult Health() =>
            new OkObjectResult(new { status = ApiConstants.StatusHealthy, timestamp = DateTime.UtcNow });

        private async Task<(Dictionary<string, byte[]>, AnalysisResult)>
            GenerateCVInternalAsync(GenerateRequest request, IProgress<GenerationStage>? progress = null)
        {
            await ReportAsync(progress, GenerationStage.Parsing);
            var cvMaster = await LoadCVMasterAsync(request.ProfileId);
            var requirements = _parser.Parse(request.JobDescription);

            await ReportAsync(progress, GenerationStage.Scoring);
            var matchScore      = _matchScoreCalc.Calculate(cvMaster, requirements);
            var missingSkills   = _missingSkillsAnalyzer.Analyze(cvMaster, requirements);

            await ReportAsync(progress, GenerationStage.Selecting);
            var generatedCV     = _selector.SelectAndAdaptCV(cvMaster, requirements, request.AllowTwoPages);
            var keywordAnalysis = _keywordAnalyzer.Analyze(request.JobDescription, generatedCV);

            await ReportAsync(progress, GenerationStage.Rendering);
            var atsScore        = _atsChecker.Check(generatedCV, requirements, request.Template);

            var templatePath = ResolveTemplatePath(request.Template);
            var renderer = new DocumentRenderer(templatePath);

            var documents = await RenderDocumentsAsync(request.Formats, renderer, generatedCV);
            var previewHtml = renderer.RenderHtmlPublic(generatedCV);

            var analysis = new AnalysisResult
            {
                MatchScore      = matchScore,
                MissingSkills   = missingSkills,
                KeywordAnalysis = keywordAnalysis,
                ATSScore        = atsScore,
                GeneratedCV     = generatedCV,
                PreviewHtml     = previewHtml
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
                var key = "exp" + i;
                if (request.BulletSelections.TryGetValue(key, out var selected))
                {
                    var all = cv.Experience[i].Bullets;
                    cv.Experience[i].Bullets = selected
                        .Where(idx => idx >= 0 && idx < all.Count)
                        .Select(idx => all[idx])
                        .ToList();
                }
            }

            cv = new PageConstraintHelper().EnforcePageConstraint(cv, request.AllowTwoPages);

            var renderer = new DocumentRenderer(ResolveTemplatePath(request.Template));
            var documents = await RenderDocumentsAsync(request.Formats, renderer, cv);
            var previewHtml = renderer.RenderHtmlPublic(cv);

            return (documents, previewHtml);
        }

        private async Task<Dictionary<string, byte[]>> RenderDocumentsAsync(
            IEnumerable<string> formats, DocumentRenderer renderer, GeneratedCV cv)
        {
            var docs = new Dictionary<string, byte[]>();
            var normalized = formats
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (normalized.Contains(ApiConstants.FormatPdf))
                docs[ApiConstants.FormatPdf] = await renderer.RenderPdfAsync(cv);

            if (normalized.Contains(ApiConstants.FormatDocx))
                docs[ApiConstants.FormatDocx] = renderer.RenderDocx(cv);

            return docs;
        }

        private string ResolveTemplatePath(CVTemplate template) => template switch
        {
            CVTemplate.Modern     => _modernTemplatePath,
            CVTemplate.Minimalist => _minimalistTemplatePath,
            _                     => _harvardTemplatePath
        };

        private string TemplatePath(string filename) =>
            Path.Combine(_env.ContentRootPath, TemplatePaths.TemplatesFolder, filename);
        private async Task<CVMaster> LoadCVMasterAsync(string profileId = "cv-master")
        {
            var safeId = Path.GetFileName(profileId);
            if (string.IsNullOrWhiteSpace(safeId)) safeId = "cv-master";

            var path = Path.Combine(_env.ContentRootPath, TemplatePaths.DataFolder, $"{safeId}.json");

            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"Profile '{safeId}' not found. Make sure '{safeId}.json' exists in the Data folder.");

            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<CVMaster>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to deserialize CV Master");
        }

        private void ValidateGenerateRequest(GenerateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.JobDescription))
                throw new ArgumentException(ApiConstants.ErrorJobDescriptionRequired);

            if (request.Formats == null || !request.Formats.Any())
                throw new ArgumentException(ApiConstants.ErrorFormatsRequired);

            ValidateFormats(request.Formats);
        }

        private void ValidateRegenerateRequest(RegenerateRequest request)
        {
            if (request?.GeneratedCV == null)
                throw new ArgumentException(ApiConstants.ErrorGeneratedCvRequired);

            if (request.Formats == null || !request.Formats.Any())
                throw new ArgumentException(ApiConstants.ErrorFormatsRequired);

            ValidateFormats(request.Formats);
        }

        private void ValidateFormats(IEnumerable<string> formats)
        {
            var invalid = formats
                .Where(f => string.IsNullOrWhiteSpace(f) || !SupportedFormats.Contains(f.Trim()))
                .ToList();

            if (invalid.Any())
            {
                var invalidList = string.Join(", ", invalid.Select(f => string.IsNullOrWhiteSpace(f) ? "<empty>" : f));
                throw new ArgumentException($"Unsupported format(s): {invalidList}. Allowed formats: pdf, docx.");
            }
        }

        private string NormalizeAndValidateFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
                throw new ArgumentException("Format is required. Allowed formats: pdf, docx.");

            var normalized = format.Trim().ToLowerInvariant();
            if (!SupportedFormats.Contains(normalized))
                throw new ArgumentException($"Unsupported format: {format}. Allowed formats: pdf, docx.");

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
                previewHtml     = analysis.PreviewHtml   // FEATURE-102
            };

        private List<object> BuildFileMetadata(Dictionary<string, byte[]> documents) =>
            documents.Select(doc => (object)new
            {
                format   = doc.Key,
                filename = BuildFileName(doc.Key),
                size     = doc.Value.Length
            }).ToList();

        public string BuildFileName(string format) =>
            ApiConstants.FileNamePrefix + DateTime.Now.ToString(ApiConstants.FileNameDateFormat) + "." + format;

        private string GetContentType(string format) =>
            format.Equals(ApiConstants.FormatPdf, StringComparison.OrdinalIgnoreCase)
                ? ApiConstants.ContentTypePdf
                : ApiConstants.ContentTypeDocx;
    }
}

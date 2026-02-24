using ErCvApi.Models;
using ErCvApi.Helpers;
using ErCvApi.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ErCvApi.Services
{
    public class CVGeneratorService
    {
        private readonly JobDescriptionParser _parser;
        private readonly CVSelector _selector;
        private readonly MatchScoreCalculator _matchScoreCalc;
        private readonly MissingSkillsAnalyzer _missingSkillsAnalyzer;
        private readonly KeywordAnalyzer _keywordAnalyzer;
        private readonly ATSChecker _atsChecker;
        private readonly IWebHostEnvironment _env;
        private readonly string _harvardTemplatePath;
        private readonly string _modernTemplatePath;
        private readonly string _minimalistTemplatePath;

        public CVGeneratorService(IWebHostEnvironment env)
        {
            _env = env;
            _parser = new JobDescriptionParser();
            _selector = new CVSelector();
            _matchScoreCalc = new MatchScoreCalculator();
            _missingSkillsAnalyzer = new MissingSkillsAnalyzer();
            _keywordAnalyzer = new KeywordAnalyzer();
            _atsChecker = new ATSChecker();

            _harvardTemplatePath   = TemplatePath(TemplatePaths.HarvardTemplate);
            _modernTemplatePath    = TemplatePath(TemplatePaths.ModernTemplate);
            _minimalistTemplatePath= TemplatePath(TemplatePaths.MinimalistTemplate);
        }

        // ─── Public endpoints ────────────────────────────────────────────────

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
            catch (Exception ex)
            {
                return new ObjectResult(new { error = ApiConstants.ErrorGeneratingCv, details = ex.Message })
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> RegenerateAsync(RegenerateRequest request, HttpContext context)
        {
            try
            {
                ValidateRegenerateRequest(request);

                var documents = await RegenerateCVInternalAsync(request);
                SaveDocumentsToSession(documents, context);

                return new OkObjectResult(new { success = true, files = BuildFileMetadata(documents) });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { error = ApiConstants.ErrorRegeneratingCv, details = ex.Message })
                { StatusCode = 500 };
            }
        }

        public IActionResult Download(string format, HttpContext context)
        {
            try
            {
                var fileBytes = context.Session.Get(ApiConstants.SessionCvPrefix + format);

                if (fileBytes == null || fileBytes.Length == 0)
                    return new NotFoundObjectResult(new { error = ApiConstants.ErrorFileNotFound });

                return new FileContentResult(fileBytes, GetContentType(format))
                {
                    FileDownloadName = BuildFileName(format)
                };
            }
            catch
            {
                return new ObjectResult(new { error = ApiConstants.ErrorDownloadingFile }) { StatusCode = 500 };
            }
        }

        /// <summary>
        /// FEATURE-202: Lists available CV profiles from the Data directory.
        /// Returns id (filename without extension) and display name.
        /// </summary>
        public IActionResult GetProfiles()
        {
            try
            {
                var dataPath = Path.Combine(_env.ContentRootPath, TemplatePaths.DataFolder);
                var profiles = Directory.GetFiles(dataPath, "cv-master*.json")
                    .Select(f =>
                    {
                        var id = Path.GetFileNameWithoutExtension(f);
                        var displayName = id
                            .Replace("cv-master-", "")
                            .Replace("cv-master", "Default");
                        return new { id, name = displayName };
                    })
                    .OrderBy(p => p.id)
                    .ToList();

                return new OkObjectResult(profiles);
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { error = "Error loading profiles", details = ex.Message })
                { StatusCode = 500 };
            }
        }

        public IActionResult Health() =>
            new OkObjectResult(new { status = ApiConstants.StatusHealthy, timestamp = DateTime.UtcNow });

        // ─── Internal ────────────────────────────────────────────────────────

        private async Task<(Dictionary<string, byte[]>, AnalysisResult)>
            GenerateCVInternalAsync(GenerateRequest request)
        {
            var cvMaster = await LoadCVMasterAsync(request.ProfileId);
            var requirements = _parser.Parse(request.JobDescription);

            var matchScore      = _matchScoreCalc.Calculate(cvMaster, requirements);
            var missingSkills   = _missingSkillsAnalyzer.Analyze(cvMaster, requirements);
            var generatedCV     = _selector.SelectAndAdaptCV(cvMaster, requirements, request.AllowTwoPages);

            // KeywordAnalyzer now uses GeneratedCV (not CVMaster) so results
            // reflect only what's actually in the output document
            var keywordAnalysis = _keywordAnalyzer.Analyze(request.JobDescription, generatedCV);
            var atsScore        = _atsChecker.Check(generatedCV, requirements, request.Template);

            var templatePath = ResolveTemplatePath(request.Template);
            var renderer = new DocumentRenderer(templatePath);

            var documents = await RenderDocumentsAsync(request.Formats, renderer, generatedCV);

            // Generate preview HTML for inline display (FEATURE-102)
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

        private async Task<Dictionary<string, byte[]>> RegenerateCVInternalAsync(RegenerateRequest request)
        {
            var cv = request.GeneratedCV;

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
            return await RenderDocumentsAsync(request.Formats, renderer, cv);
        }

        private async Task<Dictionary<string, byte[]>> RenderDocumentsAsync(
            IEnumerable<string> formats, DocumentRenderer renderer, GeneratedCV cv)
        {
            var docs = new Dictionary<string, byte[]>();

            if (formats.Contains(ApiConstants.FormatPdf, StringComparer.OrdinalIgnoreCase))
                docs[ApiConstants.FormatPdf] = await renderer.RenderPdfAsync(cv);

            if (formats.Contains(ApiConstants.FormatDocx, StringComparer.OrdinalIgnoreCase))
                docs[ApiConstants.FormatDocx] = renderer.RenderDocx(cv);

            return docs;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private string ResolveTemplatePath(CVTemplate template) => template switch
        {
            CVTemplate.Modern     => _modernTemplatePath,
            CVTemplate.Minimalist => _minimalistTemplatePath,
            _                     => _harvardTemplatePath
        };

        private string TemplatePath(string filename) =>
            Path.Combine(_env.ContentRootPath, TemplatePaths.TemplatesFolder, filename);

        /// <summary>FEATURE-202: Loads a named profile from Data/.</summary>
        private async Task<CVMaster> LoadCVMasterAsync(string profileId = "cv-master")
        {
            // Sanitize: strip path separators to prevent directory traversal
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
        }

        private void ValidateRegenerateRequest(RegenerateRequest request)
        {
            if (request?.GeneratedCV == null)
                throw new ArgumentException(ApiConstants.ErrorGeneratedCvRequired);

            if (request.Formats == null || !request.Formats.Any())
                throw new ArgumentException(ApiConstants.ErrorFormatsRequired);
        }

        private void SaveDocumentsToSession(Dictionary<string, byte[]> documents, HttpContext context)
        {
            foreach (var doc in documents)
                context.Session.Set(ApiConstants.SessionCvPrefix + doc.Key, doc.Value);
        }

        private void SaveGeneratedCvToSession(GeneratedCV cv, HttpContext context)
        {
            context.Session.SetString(ApiConstants.SessionGeneratedCv, JsonSerializer.Serialize(cv));
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

        private string BuildFileName(string format) =>
            ApiConstants.FileNamePrefix + DateTime.Now.ToString(ApiConstants.FileNameDateFormat) + "." + format;

        private string GetContentType(string format) =>
            format.Equals(ApiConstants.FormatPdf, StringComparison.OrdinalIgnoreCase)
                ? ApiConstants.ContentTypePdf
                : ApiConstants.ContentTypeDocx;
    }
}

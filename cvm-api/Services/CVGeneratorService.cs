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
        private readonly string _cvMasterPath;
        private readonly string _harvardTemplatePath;
        private readonly string _modernTemplatePath;
        private readonly string _minimalistTemplatePath;

        public CVGeneratorService(IWebHostEnvironment env)
        {
            _parser = new JobDescriptionParser();
            _selector = new CVSelector();
            _matchScoreCalc = new MatchScoreCalculator();
            _missingSkillsAnalyzer = new MissingSkillsAnalyzer();
            _keywordAnalyzer = new KeywordAnalyzer();
            _atsChecker = new ATSChecker();
            _cvMasterPath = Path.Combine( env.ContentRootPath,TemplatePaths.DataFolder,TemplatePaths.CvMasterFile);
            _harvardTemplatePath = Path.Combine( env.ContentRootPath,TemplatePaths.TemplatesFolder,TemplatePaths.HarvardTemplate);
            _modernTemplatePath = Path.Combine(env.ContentRootPath,TemplatePaths.TemplatesFolder,TemplatePaths.ModernTemplate);
            _minimalistTemplatePath = Path.Combine(env.ContentRootPath,TemplatePaths.TemplatesFolder,TemplatePaths.MinimalistTemplate);
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
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = ApiConstants.ErrorGeneratingCv,
                    details = ex.Message
                })
                { StatusCode = 500 };
            }
        }

        public async Task<IActionResult> RegenerateAsync(
            RegenerateRequest request,
            HttpContext context)
        {
            try
            {
                ValidateRegenerateRequest(request);

                var documents = await RegenerateCVInternalAsync(request);
                SaveDocumentsToSession(documents, context);

                return new OkObjectResult(new
                {
                    success = true,
                    files = BuildFileMetadata(documents)
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = ApiConstants.ErrorRegeneratingCv,
                    details = ex.Message
                })
                { StatusCode = 500 };
            }
        }

        public IActionResult Download(string format, HttpContext context)
        {
            try
            {
                var key = ApiConstants.SessionCvPrefix + format;
                var fileBytes = context.Session.Get(key);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return new NotFoundObjectResult(new { error = ApiConstants.ErrorFileNotFound });
                }

                var contentType = GetContentType(format);
                var filename = BuildFileName(format);

                return new FileContentResult(fileBytes, contentType)
                {
                    FileDownloadName = filename
                };
            }
            catch
            {
                return new ObjectResult(
                    new { error = ApiConstants.ErrorDownloadingFile })
                { StatusCode = 500 };
            }
        }

        public IActionResult Health()
        {
            return new OkObjectResult(new
            {
                status = ApiConstants.StatusHealthy,
                timestamp = DateTime.UtcNow
            });
        }

        private void ValidateGenerateRequest(GenerateRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.JobDescription))
            {
                throw new ArgumentException(ApiConstants.ErrorJobDescriptionRequired);
            }

            if (request.Formats == null || !request.Formats.Any())
            {
                throw new ArgumentException(ApiConstants.ErrorFormatsRequired);
            }
        }

        private void ValidateRegenerateRequest(RegenerateRequest request)
        {
            if (request?.GeneratedCV == null)
                throw new ArgumentException(ApiConstants.ErrorGeneratedCvRequired);

            if (request.Formats == null || !request.Formats.Any())
                throw new ArgumentException(ApiConstants.ErrorFormatsRequired);
        }

        private async Task<(Dictionary<string, byte[]>, AnalysisResult)>
            GenerateCVInternalAsync(GenerateRequest request)
        {
            var cvMaster = await LoadCVMasterAsync();
            var requirements = _parser.Parse(request.JobDescription);

            var matchScore = _matchScoreCalc.Calculate(cvMaster, requirements);
            var missingSkills =_missingSkillsAnalyzer.Analyze(cvMaster, requirements);
            var keywordAnalysis =_keywordAnalyzer.Analyze(request.JobDescription, cvMaster);
            var generatedCV =_selector.SelectAndAdaptCV(cvMaster, requirements, request.AllowTwoPages);
            var atsScore =_atsChecker.Check(generatedCV, requirements, request.Template);

            var templatePath = ResolveTemplatePath(request.Template);
            var renderer = new DocumentRenderer(templatePath);

            var documents = await RenderDocumentsAsync(
                request.Formats,
                renderer,
                generatedCV);

            var analysis = new AnalysisResult
            {
                MatchScore = matchScore,
                MissingSkills = missingSkills,
                KeywordAnalysis = keywordAnalysis,
                ATSScore = atsScore,
                GeneratedCV = generatedCV
            };

            return (documents, analysis);
        }

        private async Task<Dictionary<string, byte[]>> RegenerateCVInternalAsync(
            RegenerateRequest request)
        {
            var cv = request.GeneratedCV;

            for (int i = 0; i < cv.Experience.Count; i++)
            {
                var key = "exp" + i;

                if (request.BulletSelections.TryGetValue(key, out var selected))
                {
                    var allBullets = cv.Experience[i].Bullets;
                    cv.Experience[i].Bullets = selected
                        .Where(idx => idx >= 0 && idx < allBullets.Count)
                        .Select(idx => allBullets[idx])
                        .ToList();
                }
            }

            var pageHelper = new PageConstraintHelper();
            cv = pageHelper.EnforcePageConstraint(cv, request.AllowTwoPages);

            var templatePath = ResolveTemplatePath(request.Template);
            var renderer = new DocumentRenderer(templatePath);

            return await RenderDocumentsAsync(
                request.Formats,
                renderer,
                cv);
        }

        private async Task<Dictionary<string, byte[]>> RenderDocumentsAsync(IEnumerable<string> formats,DocumentRenderer renderer,GeneratedCV cv)
        {
            var documents = new Dictionary<string, byte[]>();

            if (formats.Contains(ApiConstants.FormatPdf,
                StringComparer.OrdinalIgnoreCase))
            {
                documents[ApiConstants.FormatPdf] =
                    await renderer.RenderPdfAsync(cv);
            }

            if (formats.Contains(ApiConstants.FormatDocx,
                StringComparer.OrdinalIgnoreCase))
            {
                documents[ApiConstants.FormatDocx] =
                    renderer.RenderDocx(cv);
            }

            return documents;
        }

        private string ResolveTemplatePath(CVTemplate template)
        {
            if (template == CVTemplate.Modern)
                return _modernTemplatePath;

            if (template == CVTemplate.Minimalist)
                return _minimalistTemplatePath;

            return _harvardTemplatePath;
        }

        private async Task<CVMaster> LoadCVMasterAsync()
        {
            var json = await File.ReadAllTextAsync(_cvMasterPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<CVMaster>(json, options)
                ?? throw new InvalidOperationException(
                    "Failed to load CV Master");
        }

        private void SaveDocumentsToSession(Dictionary<string, byte[]> documents,HttpContext context)
        {
            foreach (var doc in documents)
            {
                context.Session.Set(ApiConstants.SessionCvPrefix + doc.Key, doc.Value);
            }
        }

        private void SaveGeneratedCvToSession(
            GeneratedCV cv,
            HttpContext context)
        {
            var json = JsonSerializer.Serialize(cv);
            context.Session.SetString(
                ApiConstants.SessionGeneratedCv,
                json);
        }

        private object BuildGenerateResponse(Dictionary<string, byte[]> documents,AnalysisResult analysis)
        {
            return new
            {
                success = true,
                files = BuildFileMetadata(documents),
                matchScore = analysis.MatchScore,
                missingSkills = analysis.MissingSkills,
                keywordAnalysis = analysis.KeywordAnalysis,
                atsScore = analysis.ATSScore,
                generatedCV = analysis.GeneratedCV
            };
        }

        private List<object> BuildFileMetadata(
            Dictionary<string, byte[]> documents)
        {
            var list = new List<object>();

            foreach (var doc in documents)
            {
                list.Add(new
                {
                    format = doc.Key,
                    filename = BuildFileName(doc.Key),
                    size = doc.Value.Length
                });
            }

            return list;
        }

        private string BuildFileName(string format)
        {
            return ApiConstants.FileNamePrefix + DateTime.Now.ToString(ApiConstants.FileNameDateFormat) + "." + format;
        }

        private string GetContentType(string format)
        {
            if (format.Equals(ApiConstants.FormatPdf, StringComparison.OrdinalIgnoreCase))
                return ApiConstants.ContentTypePdf;

            return ApiConstants.ContentTypeDocx;
        }
    }
}
using ErCvApi.Models;
using ErCvApi.Helpers;
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

            _cvMasterPath = Path.Combine(env.ContentRootPath, "Data", "cv-master.json");
            _harvardTemplatePath = Path.Combine(env.ContentRootPath, "Templates", "harvard-template.html");
            _modernTemplatePath = Path.Combine(env.ContentRootPath, "Templates", "modern-template.html");
            _minimalistTemplatePath = Path.Combine(env.ContentRootPath, "Templates", "minimalist-template.html");
        }

        public async Task<(Dictionary<string, byte[]> Documents, AnalysisResult Analysis)> GenerateCVAsync(
            GenerateRequest request)
        {
            var cvMaster = await LoadCVMasterAsync();
            var requirements = _parser.Parse(request.JobDescription);

            var matchScore = _matchScoreCalc.Calculate(cvMaster, requirements);
            var missingSkills = _missingSkillsAnalyzer.Analyze(cvMaster, requirements);
            var keywordAnalysis = _keywordAnalyzer.Analyze(request.JobDescription, cvMaster);
            var generatedCV = _selector.SelectAndAdaptCV(cvMaster, requirements, request.AllowTwoPages);
            var atsScore = _atsChecker.Check(generatedCV, requirements, request.Template);

            // Render
            var templatePath = request.Template switch
            {
                CVTemplate.Modern => _modernTemplatePath,
                CVTemplate.Minimalist => _minimalistTemplatePath,
                _ => _harvardTemplatePath
            };

            var renderer = new DocumentRenderer(templatePath);
            var documents = new Dictionary<string, byte[]>();

            if (request.Formats.Contains("pdf", StringComparer.OrdinalIgnoreCase))
                documents["pdf"] = await renderer.RenderPdfAsync(generatedCV);

            if (request.Formats.Contains("docx", StringComparer.OrdinalIgnoreCase))
                documents["docx"] = renderer.RenderDocx(generatedCV);

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

        public async Task<Dictionary<string, byte[]>> RegenerateCVAsync(RegenerateRequest request)
        {
            var cv = request.GeneratedCV;

            // Apply user bullet selections
            for (int i = 0; i < cv.Experience.Count; i++)
            {
                var expKey = $"exp{i}";
                if (request.BulletSelections.TryGetValue(expKey, out var selectedIndices))
                {
                    var allBullets = cv.Experience[i].Bullets;
                    cv.Experience[i].Bullets = selectedIndices
                        .Where(idx => idx >= 0 && idx < allBullets.Count)
                        .Select(idx => allBullets[idx])
                        .ToList();
                }
            }

            // Re-check page constraint
            var pageHelper = new PageConstraintHelper();
            cv = pageHelper.EnforcePageConstraint(cv, request.AllowTwoPages);

            var templatePath = request.Template switch
            {
                CVTemplate.Modern => _modernTemplatePath,
                CVTemplate.Minimalist => _minimalistTemplatePath,
                _ => _harvardTemplatePath
            };

            var renderer = new DocumentRenderer(templatePath);
            var documents = new Dictionary<string, byte[]>();

            if (request.Formats.Contains("pdf", StringComparer.OrdinalIgnoreCase))
                documents["pdf"] = await renderer.RenderPdfAsync(cv);

            if (request.Formats.Contains("docx", StringComparer.OrdinalIgnoreCase))
                documents["docx"] = renderer.RenderDocx(cv);

            return documents;
        }

        public async Task<CVMaster> LoadCVMasterAsync()
        {
            var json = await File.ReadAllTextAsync(_cvMasterPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<CVMaster>(json, options)
                ?? throw new InvalidOperationException("Failed to load CV Master");
        }
    }
}

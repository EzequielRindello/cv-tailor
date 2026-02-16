using ErCvApi.Models;
using ErCvApi.Helpers;
using System.Text.Json;

namespace ErCvApi.Services
{
    public class CVGeneratorService
    {
        private readonly JobDescriptionParser _parser;
        private readonly CVSelector _selector;
        private readonly string _cvMasterPath;
        private readonly string _templatePath;

        public CVGeneratorService(IWebHostEnvironment env)
        {
            _parser = new JobDescriptionParser();
            _selector = new CVSelector();
            _cvMasterPath = Path.Combine(env.ContentRootPath, "Data", "cv-master.json");
            _templatePath = Path.Combine(env.ContentRootPath, "Templates", "harvard-template.html");
        }

        public async Task<Dictionary<string, byte[]>> GenerateCVAsync(GenerateRequest request)
        {
            // Load CV Master
            var cvMaster = await LoadCVMasterAsync();

            // Parse Job Description
            var requirements = _parser.Parse(request.JobDescription);

            // Select and adapt CV content
            var generatedCV = _selector.SelectAndAdaptCV(cvMaster, requirements);

            // Render documents
            var documents = new Dictionary<string, byte[]>();
            var renderer = new DocumentRenderer(_templatePath);

            if (request.Formats.Contains("pdf", StringComparer.OrdinalIgnoreCase))
            {
                var pdfBytes = await renderer.RenderPdfAsync(generatedCV);
                documents["pdf"] = pdfBytes;
            }

            if (request.Formats.Contains("docx", StringComparer.OrdinalIgnoreCase))
            {
                var docxBytes = renderer.RenderDocx(generatedCV);
                documents["docx"] = docxBytes;
            }

            return documents;
        }

        private async Task<CVMaster> LoadCVMasterAsync()
        {
            var json = await File.ReadAllTextAsync(_cvMasterPath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<CVMaster>(json, options)
                ?? throw new InvalidOperationException("Failed to load CV Master");
        }
    }
}

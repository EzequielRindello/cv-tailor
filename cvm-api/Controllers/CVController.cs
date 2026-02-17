using Microsoft.AspNetCore.Mvc;
using ErCvApi.Models;
using ErCvApi.Services;

namespace ErCvApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly CVGeneratorService _cvService;
        private readonly ILogger<CVController> _logger;

        public CVController(CVGeneratorService cvService, ILogger<CVController> logger)
        {
            _cvService = cvService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.JobDescription))
                    return BadRequest(new { error = "Job description is required" });

                if (request.Formats == null || !request.Formats.Any())
                    return BadRequest(new { error = "At least one format must be selected" });

                _logger.LogInformation(
                    "Generating CV | Formats: {F} | Template: {T} | TwoPages: {P}",
                    string.Join(", ", request.Formats), request.Template, request.AllowTwoPages);

                var (documents, analysis) = await _cvService.GenerateCVAsync(request);

                // Save files in session for download
                foreach (var doc in documents)
                    HttpContext.Session.Set($"cv_{doc.Key}", doc.Value);

                // Save the GeneratedCV in session
                var cvJson = System.Text.Json.JsonSerializer.Serialize(analysis.GeneratedCV);
                HttpContext.Session.SetString("cv_generated", cvJson);

                return Ok(new
                {
                    success = true,
                    files = documents.Select(d => new
                    {
                        format = d.Key,
                        filename = $"CV_{DateTime.Now:yyyyMMdd_HHmmss}.{d.Key}",
                        size = d.Value.Length
                    }).ToList(),

                    matchScore = analysis.MatchScore,
                    missingSkills = analysis.MissingSkills,
                    keywordAnalysis = analysis.KeywordAnalysis,
                    atsScore = analysis.ATSScore,
                    generatedCV = analysis.GeneratedCV
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CV");
                return StatusCode(500, new { error = "Error generating CV", details = ex.Message });
            }
        }

        [HttpPost("regenerate")]
        public async Task<IActionResult> Regenerate([FromBody] RegenerateRequest request)
        {
            try
            {
                if (request?.GeneratedCV == null)
                    return BadRequest(new { error = "GeneratedCV is required" });

                if (request.Formats == null || !request.Formats.Any())
                    return BadRequest(new { error = "At least one format must be selected" });

                var documents = await _cvService.RegenerateCVAsync(request);

                foreach (var doc in documents)
                    HttpContext.Session.Set($"cv_{doc.Key}", doc.Value);

                return Ok(new
                {
                    success = true,
                    files = documents.Select(d => new
                    {
                        format = d.Key,
                        filename = $"CV_{DateTime.Now:yyyyMMdd_HHmmss}.{d.Key}",
                        size = d.Value.Length
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating CV");
                return StatusCode(500, new { error = "Error regenerating CV", details = ex.Message });
            }
        }

        [HttpGet("download/{format}")]
        public IActionResult Download(string format)
        {
            try
            {
                var fileBytes = HttpContext.Session.Get($"cv_{format}");

                if (fileBytes == null || fileBytes.Length == 0)
                    return NotFound(new { error = "File not found. Generate a CV first." });

                var contentType = format.ToLower() == "pdf"
                    ? "application/pdf"
                    : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                var filename = $"CV_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
                return File(fileBytes, contentType, filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, new { error = "Error downloading file" });
            }
        }

        [HttpGet("health")]
        public IActionResult Health() =>
            Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}

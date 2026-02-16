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
                {
                    return BadRequest(new { error = "Job description is required" });
                }

                if (request.Formats == null || !request.Formats.Any())
                {
                    return BadRequest(new { error = "At least one format must be selected" });
                }

                _logger.LogInformation("Generating CV with formats: {Formats}", string.Join(", ", request.Formats));

                var documents = await _cvService.GenerateCVAsync(request);

                // Return file information for client to download
                var result = new
                {
                    success = true,
                    files = documents.Select(d => new
                    {
                        format = d.Key,
                        filename = $"CV_{DateTime.Now:yyyyMMdd_HHmmss}.{d.Key}",
                        size = d.Value.Length
                    }).ToList()
                };

                // Store files temporarily in memory for download
                foreach (var doc in documents)
                {
                    HttpContext.Session.Set($"cv_{doc.Key}", doc.Value);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CV");
                return StatusCode(500, new { error = "An error occurred while generating the CV", details = ex.Message });
            }
        }

        [HttpGet("download/{format}")]
        public IActionResult Download(string format)
        {
            try
            {
                var fileBytes = HttpContext.Session.Get($"cv_{format}");
                
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return NotFound(new { error = "File not found" });
                }

                var contentType = format.ToLower() == "pdf" 
                    ? "application/pdf" 
                    : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                var filename = $"CV_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";

                return File(fileBytes, contentType, filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, new { error = "An error occurred while downloading the file" });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}

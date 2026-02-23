using Microsoft.AspNetCore.Mvc;
using ErCvApi.Services;
using ErCvApi.Models;

namespace ErCvApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly CVGeneratorService _service;

        public CVController(CVGeneratorService service)
        {
            _service = service;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
        {
            return await _service.GenerateAsync(request, HttpContext);
        }

        [HttpPost("regenerate")]
        public async Task<IActionResult> Regenerate([FromBody] RegenerateRequest request)
        {
            return await _service.RegenerateAsync(request, HttpContext);
        }

        [HttpGet("download/{format}")]
        public IActionResult Download(string format)
        {
            return _service.Download(format, HttpContext);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return _service.Health();
        }
    }
}
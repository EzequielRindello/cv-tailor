using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ErCvApi.Services;
using ErCvApi.Models;

namespace ErCvApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class CVController : ControllerBase
    {
        private readonly CVGeneratorService _service;

        public CVController(CVGeneratorService service)
        {
            _service = service;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
            => await _service.GenerateAsync(request, HttpContext);

        [HttpPost("regenerate")]
        public async Task<IActionResult> Regenerate([FromBody] RegenerateRequest request)
            => await _service.RegenerateAsync(request, HttpContext);

        [HttpGet("download/{format}")]
        public IActionResult Download(string format)
            => _service.Download(format, HttpContext);
        [HttpGet("profiles")]
        public IActionResult GetProfiles()
            => _service.GetProfiles();

        [HttpGet("health")]
        public IActionResult Health()
            => _service.Health();
    }
}

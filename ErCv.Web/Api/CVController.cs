using ErCv.Web.Constants;
using ErCv.Web.Models;
using ErCv.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErCv.Web.Api;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
[RequestSizeLimit(ApiConstants.MaxRequestBodyBytes)]
public class CVController : ControllerBase
{
    #region Fields

    private readonly CVGeneratorService _service;

    #endregion

    #region Constructors

    public CVController(CVGeneratorService service)
    {
        _service = service;
    }

    #endregion

    #region Public Methods

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

    #endregion
}

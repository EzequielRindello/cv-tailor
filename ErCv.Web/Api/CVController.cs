using ErCv.Web.Constants;
using ErCv.Web.Models;
using ErCv.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ErCv.Web.Api;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
// An attribute, not MapControllers().RequireRateLimiting: a convention would win over an action's own policy
[EnableRateLimiting(ApiConstants.ApiRateLimitPolicy)]
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

    /// <summary>Reads a job post from its Greenhouse or Lever link. Counts against the stricter "import" rate limit, not "api".</summary>
    [HttpPost("import-job-post")]
    [EnableRateLimiting(ApiConstants.ImportRateLimitPolicy)]
    [RequestSizeLimit(ApiConstants.MaxImportRequestBytes)]
    [EndpointSummary("Read a job post from its Greenhouse or Lever link")]
    [ProducesResponseType<JobPostImportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ImportJobPost([FromBody] JobPostImportRequest request)
        => await _service.ImportJobPostAsync(request, HttpContext.RequestAborted);

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

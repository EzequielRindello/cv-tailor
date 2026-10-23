using System.Net;
using System.Threading.RateLimiting;
using ErCv.Web.Components;
using ErCv.Web.Configuration;
using ErCv.Web.Features.Analysis;
using ErCv.Web.Features.CoverLetters;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Features.Selection;
using ErCv.Web.Services;
using ErCv.Web.UI;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

const string ApiRateLimitPolicy = "api";
const string PortVariable = "PORT";
const string ErrorPath = "/Error";

var builder = WebApplication.CreateBuilder(args);
var settings = ErCvOptions.Load(builder.Configuration);
builder.Services.AddSingleton(settings);

// Render injects the port to bind to
var port = Environment.GetEnvironmentVariable(PortVariable);
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(ApiRateLimitPolicy, context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = settings.RateLimit.PermitLimit,
            Window = TimeSpan.FromSeconds(settings.RateLimit.WindowSeconds),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(settings.Session.IdleTimeoutMinutes);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Render's proxy is not on a fixed, known network. Trusting only the last hop (the one Render appends) is what
    // stops a client from spoofing X-Forwarded-For to get a fresh rate limit partition, so keep this at 1.
    options.ForwardLimit = 1;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
// ENG-01. The domain classes keep no per-request state, so one instance of each serves every request.
builder.Services.AddSingleton<JobDescriptionParser>();
builder.Services.AddSingleton<RelevanceScorer>();
builder.Services.AddSingleton<SummaryGenerator>();
builder.Services.AddSingleton<PageConstraintHelper>();
builder.Services.AddSingleton<CVSelector>();
builder.Services.AddSingleton<MatchScoreCalculator>();
builder.Services.AddSingleton<MissingSkillsAnalyzer>();
builder.Services.AddSingleton<KeywordAnalyzer>();
builder.Services.AddSingleton<ATSChecker>();
builder.Services.AddSingleton<JdHighlighter>();
builder.Services.AddSingleton<ProfileValidator>();
builder.Services.AddSingleton<CoverLetterGenerator>();
// APP-04: the only outbound requests the app makes, to the Greenhouse and Lever board APIs. A redirect is not
// followed, so the fixed API host is the only host ever reached.
builder.Services.AddHttpClient(JobPostImporter.HttpClientName, client =>
    {
        client.Timeout = JobPostImporter.RequestTimeout;
        client.DefaultRequestHeaders.UserAgent.ParseAdd(JobPostImporter.UserAgent);
        client.DefaultRequestHeaders.Accept.ParseAdd(JobPostImporter.JsonMediaType);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.All
    });
builder.Services.AddSingleton<JobPostImporter>();
builder.Services.AddScoped<CVGeneratorService>();
builder.Services.AddScoped<BrowserStore>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddHostedService<PdfEngineWarmUp>();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    // The OpenAPI document at /openapi/v1.json and its Scalar UI at /scalar, both Development only
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandlingPath = ErrorPath,
        CreateScopeForErrors = true,
        // Kestrel rejects an oversized body (RequestSizeLimit) with this exception; answer its 413, not a 500
        StatusCodeSelector = ex => ex is BadHttpRequestException badRequest
            ? badRequest.StatusCode
            : StatusCodes.Status500InternalServerError
    });
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseSession();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers().RequireRateLimiting(ApiRateLimitPolicy);
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStopping.Register(() => DocumentRenderer.ShutdownAsync().GetAwaiter().GetResult());

app.Run();

// Lets WebApplicationFactory<Program> reach the top-level entry point from the test project
public partial class Program { }

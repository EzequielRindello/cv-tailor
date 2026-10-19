using ErCv.Web.Components;
using ErCv.Web.UI;
using ErCvApi.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Render injects the port to bind to
var port = Environment.GetEnvironmentVariable("PORT");
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("api", context =>
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Render's proxy is not on a fixed, known network
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddScoped<CVGeneratorService>();
builder.Services.AddScoped<BrowserStore>();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseRateLimiter();
app.UseSession();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers().RequireRateLimiting("api");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

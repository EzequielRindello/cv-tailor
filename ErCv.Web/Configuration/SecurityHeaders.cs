namespace ErCv.Web.Configuration;

/// <summary>Response headers that harden every page and API response.</summary>
public static class SecurityHeaders
{
    #region Constants

    private const string ContentTypeOptions = "nosniff";
    private const string FrameOptions = "SAMEORIGIN";
    private const string ReferrerPolicy = "strict-origin-when-cross-origin";

    // Only directives that cannot break Blazor or the CV preview. The preview is a srcdoc iframe, which inherits
    // this policy, so framing is limited to the app's own origin instead of being blocked outright.
    private const string ContentSecurityPolicy = "frame-ancestors 'self'; object-src 'none'; base-uri 'self'";

    #endregion

    #region Public Methods

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = ContentTypeOptions;
            headers.XFrameOptions = FrameOptions;
            headers["Referrer-Policy"] = ReferrerPolicy;
            headers.ContentSecurityPolicy = ContentSecurityPolicy;

            await next();
        });

    #endregion
}

using System.Text.Json;
using ErCv.Web.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ErCv.Web.Configuration;

/// <summary>
/// Errors that escape the API's own handling, answered like every other API error: a ProblemDetails, never the HTML
/// error page the UI gets. The main one is a body over the endpoint's <c>RequestSizeLimit</c>, which Kestrel rejects
/// before the controller runs.
/// </summary>
public static class ApiErrors
{
    #region Constants

    private const string ApiPath = "/api";

    #endregion

    #region Public Methods

    /// <summary>Handles the errors of the <c>/api</c> routes; registered inside the app-wide handler, so it runs first.</summary>
    public static IApplicationBuilder UseApiErrors(this IApplicationBuilder app) =>
        app.UseWhen(context => context.Request.Path.StartsWithSegments(ApiPath), api =>
            api.UseExceptionHandler(new ExceptionHandlerOptions
            {
                StatusCodeSelector = StatusOf,
                ExceptionHandler = WriteProblemAsync
            }));

    /// <summary>Kestrel's own rejections keep their status (413 for a body over the limit); anything else is a 500.</summary>
    public static int StatusOf(Exception exception) =>
        exception is BadHttpRequestException badRequest ? badRequest.StatusCode : StatusCodes.Status500InternalServerError;

    /// <summary>Writes the problem for the status the handler set. The exception text never reaches the client.</summary>
    public static Task WriteProblemAsync(HttpContext context)
    {
        var status = context.Response.StatusCode;
        var (title, detail) = status switch
        {
            StatusCodes.Status413PayloadTooLarge => (ApiConstants.TitlePayloadTooLarge, ApiConstants.ErrorRequestTooLarge),
            StatusCodes.Status500InternalServerError => (ApiConstants.TitleServerError, ApiConstants.ErrorUnexpected),
            _ => (ApiConstants.TitleBadRequest, ApiConstants.ErrorRequestUnreadable)
        };

        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        problem.Extensions["error"] = detail;
        return context.Response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, ApiConstants.ProblemJsonContentType);
    }

    #endregion
}

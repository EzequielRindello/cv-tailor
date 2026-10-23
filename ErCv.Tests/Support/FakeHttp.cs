using System.Net;
using System.Text;

namespace ErCv.Tests.Support;

/// <summary>
/// Answers HTTP requests in memory (no network) and keeps every request it got, so a test can check which
/// addresses the app called.
/// </summary>
public sealed class FakeHttp : HttpMessageHandler, IHttpClientFactory
{
    #region Fields

    private readonly Func<HttpRequestMessage, HttpResponseMessage> _answer;

    #endregion

    #region Constructors

    public FakeHttp(Func<HttpRequestMessage, HttpResponseMessage> answer) => _answer = answer;

    #endregion

    #region Properties

    public List<Uri> Requested { get; } = new();

    #endregion

    #region Public Methods

    public static FakeHttp Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(_ => new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") });

    public static FakeHttp Throws(Exception error) => new(_ => throw error);

    public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

    #endregion

    #region Private Methods

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requested.Add(request.RequestUri!);
        return Task.FromResult(_answer(request));
    }

    #endregion
}

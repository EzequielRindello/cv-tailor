using ErCv.Web.Features.Documents;
using ErCv.Web.Models;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Documents;

// ENG-05. Same collection as the other PDF tests: they share the one Chrome this test closes and relaunches.
[Collection("Documents")]
[Trait("Category", "Documents")]
public class PdfEngineTests
{
    [Fact]
    public async Task A_failed_launch_reports_unavailable_and_the_next_pdf_launches_chrome_again()
    {
        var original = Environment.GetEnvironmentVariable(DocumentConstants.ExecutablePathVariable);
        await DocumentRenderer.ShutdownAsync();

        try
        {
            Environment.SetEnvironmentVariable(
                DocumentConstants.ExecutablePathVariable, Path.Combine(Path.GetTempPath(), "no-such-chrome", "chrome"));

            await Assert.ThrowsAnyAsync<Exception>(DocumentRenderer.WarmUpAsync);
            Assert.Equal(DocumentConstants.PdfEngineUnavailable, DocumentRenderer.PdfEngineStatus);
        }
        finally
        {
            Environment.SetEnvironmentVariable(DocumentConstants.ExecutablePathVariable, original);
        }

        var pdf = await DocumentSupport.Renderer(CVTemplate.Harvard).RenderPdfAsync(Generated());

        Assert.True(DocumentRenderer.CountPages(pdf) > 0);
        Assert.Equal(DocumentConstants.PdfEngineReady, DocumentRenderer.PdfEngineStatus);
    }
}

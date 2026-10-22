namespace ErCv.Web.Features.Documents;

/// <summary>
/// Launches Chrome in the background when the app starts (ENG-05), so the first visitor after a cold start on Render
/// does not wait for it. A failure is only logged: the first PDF tries again.
/// </summary>
public class PdfEngineWarmUp : BackgroundService
{
    #region Fields

    private readonly ILogger<PdfEngineWarmUp> _logger;

    #endregion

    #region Constructors

    public PdfEngineWarmUp(ILogger<PdfEngineWarmUp> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Private Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await DocumentRenderer.WarmUpAsync();
            _logger.LogInformation("PDF engine ready");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PDF engine warm-up failed, the first PDF will launch it again");
        }
    }

    #endregion
}

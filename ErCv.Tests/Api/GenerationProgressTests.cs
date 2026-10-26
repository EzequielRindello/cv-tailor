using ErCv.Tests.Fixtures;
using ErCv.Web.Models;
using ErCv.Web.Services;

namespace ErCv.Tests.Api;

// Same collection as the PDF tests: the app shuts the shared headless Chromium down when a host stops.
[Collection("Documents")]
[Trait("Category", "Api")]
public class GenerationProgressTests
{
    /// <summary>Synchronous on purpose: Progress&lt;T&gt; posts to the thread pool and would make the order racy.</summary>
    private sealed class RecordingProgress : IProgress<GenerationStatus>
    {
        public List<GenerationStatus> Reports { get; } = new();

        public List<GenerationStage> Stages => Reports.Select(r => r.Stage).Distinct().ToList();

        public void Report(GenerationStatus value) => Reports.Add(value);
    }

    private static GenerateRequest Request() => new()
    {
        JobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
        Formats = new List<string> { "docx" }
    };

    [Fact]
    public async Task BuildCv_reports_every_stage_once_in_order()
    {
        using var app = new ApiApp();
        var progress = new RecordingProgress();

        await app.InScopeAsync(async sp =>
        {
            var service = sp.GetService(typeof(CVGeneratorService)) as CVGeneratorService;
            return await service!.BuildCvAsync(Request(), progress);
        });

        Assert.Equal(
            new[] { GenerationStage.Parsing, GenerationStage.Selecting, GenerationStage.Rendering, GenerationStage.Scoring },
            progress.Stages);
    }

    // UX-11: the bar only moves forward, starts at 0 and ends at 100
    [Fact]
    public async Task BuildCv_reports_a_percent_that_only_grows_from_0_to_100()
    {
        using var app = new ApiApp();
        var progress = new RecordingProgress();

        await app.InScopeAsync(async sp =>
            await ((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!).BuildCvAsync(Request(), progress));

        var percents = progress.Reports.Select(r => r.Percent).ToList();
        Assert.Equal(0, percents[0]);
        Assert.Equal(100, percents[^1]);
        Assert.Equal(percents.OrderBy(p => p), percents);
    }

    // With a PDF, the page fit renders in Chrome: its renders are reported inside the rendering band with a label
    [Fact]
    public async Task BuildCv_with_a_pdf_reports_the_page_fit_inside_the_rendering_band()
    {
        using var app = new ApiApp();
        var progress = new RecordingProgress();
        var request = Request();
        request.Formats = new List<string> { "pdf" };

        await app.InScopeAsync(async sp =>
            await ((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!).BuildCvAsync(request, progress));

        var fitting = progress.Reports.Where(r => r.Detail != null).ToList();
        Assert.NotEmpty(fitting);
        Assert.All(fitting, r => Assert.Equal(GenerationStage.Rendering, r.Stage));
        Assert.All(fitting, r => Assert.InRange(r.Percent, 30, 90));
        Assert.StartsWith("Fitting the page (1 of ", fitting[0].Detail);
        Assert.Equal(fitting.Select(r => r.Percent).OrderBy(p => p), fitting.Select(r => r.Percent));
    }

    [Fact]
    public async Task BuildCv_without_a_progress_sink_still_works()
    {
        using var app = new ApiApp();

        var (documents, analysis) = await app.InScopeAsync(async sp =>
        {
            var service = sp.GetService(typeof(CVGeneratorService)) as CVGeneratorService;
            return await service!.BuildCvAsync(Request());
        });

        Assert.Contains("docx", documents.Keys);
        Assert.NotNull(analysis.PreviewHtml);
    }
}

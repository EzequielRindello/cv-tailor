using ErCv.Tests.Fixtures;
using ErCvApi.Models;
using ErCvApi.Services;

namespace ErCv.Tests.Api
{
    // Same collection as the PDF tests: the app shuts the shared headless Chromium down when a host stops.
    [Collection("Documents")]
    [Trait("Category", "Api")]
    public class GenerationProgressTests
    {
        /// <summary>Synchronous on purpose: Progress&lt;T&gt; posts to the thread pool and would make the order racy.</summary>
        private sealed class RecordingProgress : IProgress<GenerationStage>
        {
            public List<GenerationStage> Stages { get; } = new();

            public void Report(GenerationStage value) => Stages.Add(value);
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
                new[] { GenerationStage.Parsing, GenerationStage.Scoring, GenerationStage.Selecting, GenerationStage.Rendering },
                progress.Stages);
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
}

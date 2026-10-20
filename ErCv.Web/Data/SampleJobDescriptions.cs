using System.Text.Json;

namespace ErCv.Web.Data
{
    /// <summary>A real, recent job post from the corpus, with where and when it was published.</summary>
    public sealed record SampleJobDescription
    {
        public string Id { get; init; } = "";
        public string Label { get; init; } = "";
        public string PostingId { get; init; } = "";
        public string Source { get; init; } = "";
        public string Url { get; init; } = "";
        public string Company { get; init; } = "";
        public string Title { get; init; } = "";
        public string Location { get; init; } = "";
        public DateTimeOffset PostedAt { get; init; }
        public string Text { get; init; } = "";
    }

    /// <summary>
    /// Job descriptions a visitor can load with one click (APP-03). One per role family, picked from the
    /// job-post corpus (APP-05) by the collector's refresh command, which writes Data/sample-jobs.json.
    /// </summary>
    public static class SampleJobDescriptions
    {
        public const string ResourceName = "ErCv.Web.Data.sample-jobs.json";

        public static readonly IReadOnlyList<SampleJobDescription> All = Load();

        private static IReadOnlyList<SampleJobDescription> Load()
        {
            using var stream = typeof(SampleJobDescriptions).Assembly.GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing");
            return JsonSerializer.Deserialize<List<SampleJobDescription>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<SampleJobDescription>();
        }
    }
}

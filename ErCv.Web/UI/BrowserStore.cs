using System.Text.Json;
using ErCv.Web.UI;
using Microsoft.JSInterop;

namespace ErCv.Web.UI
{
    /// <summary>Settings and generation history kept in the visitor's browser (localStorage).</summary>
    public class BrowserStore
    {
        private const string SettingsKey = "cv-generator-settings";
        private const string HistoryKey = "cv-generator-history";
        private const int MaxHistoryEntries = 10;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IJSRuntime _js;

        public BrowserStore(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            var defaults = new AppSettings();
            var loaded = await ReadAsync<AppSettings>(SettingsKey);
            if (loaded == null) return defaults;

            foreach (var (key, value) in defaults.SelectedFormats)
                loaded.SelectedFormats.TryAdd(key, value);

            return loaded;
        }

        public Task SaveSettingsAsync(AppSettings settings) => WriteAsync(SettingsKey, settings);

        public async Task<List<HistoryEntry>> GetHistoryAsync() =>
            await ReadAsync<List<HistoryEntry>>(HistoryKey) ?? new();

        public async Task<List<HistoryEntry>> AddToHistoryAsync(string jobDescription, int? matchScore, string template)
        {
            var entry = new HistoryEntry
            {
                Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                JobTitle = ExtractJobTitle(jobDescription),
                GeneratedAt = DateTime.UtcNow.ToString("o"),
                MatchScore = matchScore,
                Template = template,
                JobDescription = jobDescription
            };

            var updated = (await GetHistoryAsync()).Prepend(entry).Take(MaxHistoryEntries).ToList();
            await WriteAsync(HistoryKey, updated);
            return updated;
        }

        public async Task<List<HistoryEntry>> RemoveFromHistoryAsync(long id)
        {
            var updated = (await GetHistoryAsync()).Where(e => e.Id != id).ToList();
            await WriteAsync(HistoryKey, updated);
            return updated;
        }

        public async Task ClearHistoryAsync() =>
            await _js.InvokeVoidAsync("cvApp.removeItem", HistoryKey);

        private static string ExtractJobTitle(string jobDescription)
        {
            var firstLine = (jobDescription ?? string.Empty)
                .Split('\n')
                .Select(l => l.Trim())
                .FirstOrDefault(l => l.Length > 0);

            var title = string.IsNullOrEmpty(firstLine) ? "Unknown role" : firstLine;
            return title.Length > 80 ? title[..80] : title;
        }

        private async Task<T?> ReadAsync<T>(string key)
        {
            try
            {
                var raw = await _js.InvokeAsync<string?>("cvApp.getItem", key);
                return string.IsNullOrEmpty(raw) ? default : JsonSerializer.Deserialize<T>(raw, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private async Task WriteAsync<T>(string key, T value) =>
            await _js.InvokeVoidAsync("cvApp.setItem", key, JsonSerializer.Serialize(value, JsonOptions));
    }
}

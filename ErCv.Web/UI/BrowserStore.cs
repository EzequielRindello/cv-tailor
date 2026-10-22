using System.Text.Json;
using ErCv.Web.Constants;
using ErCv.Web.Models;
using Microsoft.JSInterop;

namespace ErCv.Web.UI;

/// <summary>Settings and generation history kept in the visitor's browser (localStorage).</summary>
public class BrowserStore
{
    #region Constants

    private const string SettingsKey = "cv-generator-settings";
    private const string ProfileKey = "cv-generator-my-profile";
    private const string HistoryKey = "cv-generator-history";
    private const int MaxHistoryEntries = 50;
    private const int MaxJobTitleLength = 80;
    private const string UnknownJobTitle = "Unknown role";
    private const string TimestampFormat = "o";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    #endregion

    #region Fields


    private readonly IJSRuntime _js;

    #endregion

    #region Constructors


    public BrowserStore(IJSRuntime js)
    {
        _js = js;
    }

    #endregion

    #region Public Methods

    public async Task<AppSettings> LoadSettingsAsync()
    {
        var defaults = new AppSettings();
        var loaded = await ReadAsync<AppSettings>(SettingsKey);
        if (loaded == null) return defaults;

        foreach (var (key, value) in defaults.SelectedFormats)
            loaded.SelectedFormats.TryAdd(key, value);

        // Settings saved before these options existed, or edited by hand, fall back to the defaults
        if (!ApiConstants.Languages.Contains(loaded.Language)) loaded.Language = defaults.Language;
        if (!ApiConstants.Accents.Contains(loaded.Accent)) loaded.Accent = defaults.Accent;
        if (!ApiConstants.FontPairings.Contains(loaded.Fonts)) loaded.Fonts = defaults.Fonts;

        return loaded;
    }

    public Task SaveSettingsAsync(AppSettings settings) => WriteAsync(SettingsKey, settings);

    /// <summary>The visitor's own profile (PROF-01). It stays in this browser and is never sent to the server to be stored.</summary>
    public Task<CVMaster?> LoadMyProfileAsync() => ReadAsync<CVMaster>(ProfileKey);

    public Task SaveMyProfileAsync(CVMaster profile) => WriteAsync(ProfileKey, profile);

    public Task ClearMyProfileAsync() => RemoveAsync(ProfileKey);

    public async Task<List<HistoryEntry>> GetHistoryAsync() =>
        await ReadAsync<List<HistoryEntry>>(HistoryKey) ?? new();

    public async Task<List<HistoryEntry>> AddToHistoryAsync(
        string jobDescription, int? matchScore, string template, HistoryEntry? options = null)
    {
        var entry = new HistoryEntry
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            JobTitle = ExtractJobTitle(jobDescription),
            GeneratedAt = DateTime.UtcNow.ToString(TimestampFormat),
            MatchScore = matchScore,
            Template = template,
            JobDescription = jobDescription,
            TemplateKey = options?.TemplateKey,
            Formats = options?.Formats,
            AllowTwoPages = options?.AllowTwoPages,
            ProfileSource = options?.ProfileSource,
            ProfileId = options?.ProfileId,
            Language = options?.Language,
            Accent = options?.Accent,
            Fonts = options?.Fonts,
            IncludeCoverLetter = options?.IncludeCoverLetter
        };

        var updated = (await GetHistoryAsync()).Prepend(entry).Take(MaxHistoryEntries).ToList();
        await WriteAsync(HistoryKey, updated);
        return updated;
    }

    /// <summary>Saves edits to an entry's application details (APP-01). Keyed by id, so a stale copy cannot add an entry back.</summary>
    public async Task<List<HistoryEntry>> UpdateHistoryEntryAsync(HistoryEntry entry)
    {
        entry.Status = ApplicationTracker.Normalize(entry.Status);
        var history = await GetHistoryAsync();
        var index = history.FindIndex(e => e.Id == entry.Id);
        if (index < 0) return history;

        history[index] = entry;
        await WriteAsync(HistoryKey, history);
        return history;
    }

    public async Task<List<HistoryEntry>> RemoveFromHistoryAsync(long id)
    {
        var updated = (await GetHistoryAsync()).Where(e => e.Id != id).ToList();
        await WriteAsync(HistoryKey, updated);
        return updated;
    }

    public Task ClearHistoryAsync() => RemoveAsync(HistoryKey);

    #endregion

    #region Private Methods

    private static string ExtractJobTitle(string jobDescription)
    {
        var firstLine = (jobDescription ?? string.Empty)
            .Split('\n')
            .Select(l => l.Trim())
            .FirstOrDefault(l => l.Length > 0);

        var title = string.IsNullOrEmpty(firstLine) ? UnknownJobTitle : firstLine;
        return title.Length > MaxJobTitleLength ? title[..MaxJobTitleLength] : title;
    }

    private async Task<T?> ReadAsync<T>(string key)
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>(UiConstants.JsGetItem, key);
            return string.IsNullOrEmpty(raw) ? default : JsonSerializer.Deserialize<T>(raw, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
        catch (Exception ex) when (IsCircuitGone(ex))
        {
            return default;
        }
    }

    private async Task WriteAsync<T>(string key, T value)
    {
        try
        {
            await _js.InvokeVoidAsync(UiConstants.JsSetItem, key, JsonSerializer.Serialize(value, JsonOptions));
        }
        catch (Exception ex) when (IsCircuitGone(ex))
        {
        }
    }

    private async Task RemoveAsync(string key)
    {
        try
        {
            await _js.InvokeVoidAsync(UiConstants.JsRemoveItem, key);
        }
        catch (Exception ex) when (IsCircuitGone(ex))
        {
        }
    }

    // The tab was reloaded or closed (or hot reload restarted the circuit) while a call was on its way: the browser
    // will never answer, and there is no page left to show an error on, so storage just gives the default.
    private static bool IsCircuitGone(Exception ex) => ex is JSDisconnectedException or TaskCanceledException;

    #endregion
}

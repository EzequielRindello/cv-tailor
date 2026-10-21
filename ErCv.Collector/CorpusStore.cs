using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ErCv.Collector;

/// <summary>
/// The committed corpus folder: sources.json, takedowns.json, manifest.json and postings/*.json (one file per post).
/// Writes are deterministic and skipped when the content is unchanged, which is what makes a refresh idempotent.
/// </summary>
public sealed partial class CorpusStore
{
    #region Constants

    private const string PostingsFolder = "postings";
    private const string ConfigFile = "sources.json";
    private const string TakedownsFile = "takedowns.json";
    private const string ManifestFile = "manifest.json";
    private const string JsonExtension = ".json";
    private const string JsonSearchPattern = "*.json";
    private const string LineEnding = "\n";

    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        IndentSize = 2,
        NewLine = LineEnding,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex("[^a-z0-9-]+")]
    private static partial Regex NonIdCharacters();

    #endregion

    #region Constructors

    public CorpusStore(string root)
    {
        Root = root;
    }

    #endregion

    #region Properties

    public string Root { get; }
    public string PostingsDir => Path.Combine(Root, PostingsFolder);
    public string ConfigPath => Path.Combine(Root, ConfigFile);
    public string TakedownsPath => Path.Combine(Root, TakedownsFile);
    public string ManifestPath => Path.Combine(Root, ManifestFile);

    #endregion

    #region Public Methods

    public static string MakeId(string source, string board, string sourceId) =>
        NonIdCharacters().Replace($"{source}-{board}-{sourceId}".ToLowerInvariant(), "-").Trim('-');

    public CorpusConfig LoadConfig() => Read<CorpusConfig>(ConfigPath) ?? new CorpusConfig();

    public List<Takedown> LoadTakedowns() => Read<List<Takedown>>(TakedownsPath) ?? new List<Takedown>();

    public Manifest? LoadManifest() => Read<Manifest>(ManifestPath);

    public List<Posting> LoadPostings()
    {
        if (!Directory.Exists(PostingsDir)) return new List<Posting>();
        return Directory.EnumerateFiles(PostingsDir, JsonSearchPattern)
            .Select(f => Read<Posting>(f) is { } p && p.Id == Path.GetFileNameWithoutExtension(f)
                ? p : throw new InvalidDataException($"Unreadable posting, or its id is not its file name: {f}"))
            .OrderBy(p => p.Id, StringComparer.Ordinal)
            .ToList();
    }

    public string PostingPath(string id) => Path.Combine(PostingsDir, id + JsonExtension);

    public bool SavePosting(Posting posting)
    {
        Directory.CreateDirectory(PostingsDir);
        return WriteIfChanged(PostingPath(posting.Id), posting);
    }

    public bool DeletePosting(string id)
    {
        var path = PostingPath(id);
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    public bool SaveTakedowns(List<Takedown> takedowns) => WriteIfChanged(TakedownsPath, takedowns);

    public bool SaveManifest(Manifest manifest) => WriteIfChanged(ManifestPath, manifest);

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Json) + LineEnding;

    /// <summary>Writes only when the text differs (line endings ignored, so a CRLF checkout is not "changed").</summary>
    public static bool WriteIfChanged<T>(string path, T value)
    {
        var text = Serialize(value);
        if (File.Exists(path) && File.ReadAllText(path).Replace("\r\n", LineEnding) == text) return false;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
        return true;
    }

    #endregion

    #region Private Methods

    private static T? Read<T>(string path) =>
        File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), Json) : default;

    #endregion
}

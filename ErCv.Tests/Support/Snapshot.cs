using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErCv.Tests.Fixtures;

namespace ErCv.Tests.Support;

/// <summary>
/// Golden snapshots (TEST-08), with no library: each case is a JSON file under ErCv.Tests/Snapshots/approved.
/// A difference writes a .received.json next to it (git ignores those) and fails, so a scoring change shows up as a
/// diff to review. Set ERCV_SNAPSHOTS=approve to accept every current output and drop snapshots no case uses.
/// </summary>
public static class Snapshot
{
    public const string ModeVariable = "ERCV_SNAPSHOTS";
    public const string ApproveMode = "approve";

    private const string ApprovedSuffix = ".approved.json";
    private const string ReceivedSuffix = ".received.json";

    // Lines of context around the first difference in a failure message
    private const int ContextLines = 3;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // Accents and quotes stay readable in the diff instead of \u escapes
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Folder { get; } = Path.Combine(FixtureLoader.RepoRoot(), "ErCv.Tests", "Snapshots", "approved");

    public static bool Approving =>
        string.Equals(Environment.GetEnvironmentVariable(ModeVariable), ApproveMode, StringComparison.OrdinalIgnoreCase);

    /// <param name="folder">Where the files live; only the tests of this helper pass another one.</param>
    public static void Verify(string name, object value, string? folder = null)
    {
        folder ??= Folder;
        var approvedPath = Path.Combine(folder, name + ApprovedSuffix);
        var receivedPath = Path.Combine(folder, name + ReceivedSuffix);
        var received = JsonSerializer.Serialize(value, JsonOptions).ReplaceLineEndings("\n") + "\n";

        if (Approving)
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(approvedPath, received);
            File.Delete(receivedPath);
            return;
        }

        if (!File.Exists(approvedPath))
        {
            File.WriteAllText(receivedPath, received);
            Assert.Fail($"No approved snapshot for '{name}'. Review {receivedPath} and approve it with {ModeVariable}={ApproveMode}.");
        }

        // git may check the file out with CRLF line endings
        var approved = File.ReadAllText(approvedPath).ReplaceLineEndings("\n");
        if (approved == received)
        {
            File.Delete(receivedPath);
            return;
        }

        File.WriteAllText(receivedPath, received);
        Assert.Fail($"Snapshot '{name}' changed. {FirstDifference(approved, received)}\n" +
                    $"Compare {approvedPath} with {receivedPath}; if the change is intended, approve it with {ModeVariable}={ApproveMode}.");
    }

    /// <summary>Approved snapshots that no current case produces (a corpus refresh drops posts). Deleted when approving.</summary>
    public static List<string> Orphans(IEnumerable<string> expectedNames)
    {
        var expected = expectedNames.ToHashSet(StringComparer.Ordinal);
        var orphans = Directory.Exists(Folder)
            ? Directory.GetFiles(Folder, "*" + ApprovedSuffix)
                .Select(f => Path.GetFileName(f)[..^ApprovedSuffix.Length])
                .Where(n => !expected.Contains(n))
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList()
            : new List<string>();

        if (Approving)
        {
            foreach (var orphan in orphans)
                File.Delete(Path.Combine(Folder, orphan + ApprovedSuffix));
            return new List<string>();
        }

        return orphans;
    }

    private static string FirstDifference(string approved, string received)
    {
        var a = approved.Split('\n');
        var r = received.Split('\n');
        var line = 0;
        while (line < a.Length && line < r.Length && a[line] == r[line]) line++;

        var from = Math.Max(0, line - ContextLines);
        string Excerpt(string[] lines) => string.Join("\n", lines.Skip(from).Take(line - from + ContextLines + 1));
        return $"First difference at line {line + 1}.\nApproved:\n{Excerpt(a)}\nReceived:\n{Excerpt(r)}";
    }
}

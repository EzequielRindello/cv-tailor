using Xunit.Sdk;

namespace ErCv.Tests.Support;

[Trait("Category", "Unit")]
public class SnapshotTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "ercv-snapshot-tests", Guid.NewGuid().ToString("N"));

    public SnapshotTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    private string Approved => Path.Combine(_folder, "case.approved.json");
    private string Received => Path.Combine(_folder, "case.received.json");

    [Fact]
    public void A_matching_output_passes_and_leaves_no_received_file()
    {
        if (Snapshot.Approving) return;
        File.WriteAllText(Approved, "{\r\n  \"score\": 70\r\n}\r\n");

        Snapshot.Verify("case", new { Score = 70 }, _folder);

        Assert.False(File.Exists(Received));
    }

    [Fact]
    public void A_changed_output_fails_at_the_first_difference_and_writes_what_it_received()
    {
        if (Snapshot.Approving) return;
        File.WriteAllText(Approved, "{\n  \"score\": 70\n}\n");

        var failure = Assert.Throws<FailException>(() => Snapshot.Verify("case", new { Score = 72 }, _folder));

        Assert.Contains("line 2", failure.Message);
        Assert.Contains("\"score\": 72", File.ReadAllText(Received));
        Assert.Contains("\"score\": 70", File.ReadAllText(Approved));
    }

    [Fact]
    public void A_case_without_an_approved_file_fails_and_writes_what_it_received()
    {
        if (Snapshot.Approving) return;

        Assert.Throws<FailException>(() => Snapshot.Verify("case", new { Score = 70 }, _folder));

        Assert.True(File.Exists(Received));
        Assert.False(File.Exists(Approved));
    }
}

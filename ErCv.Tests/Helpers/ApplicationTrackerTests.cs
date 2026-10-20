using System.Text.Json;
using ErCv.Web.UI;

namespace ErCv.Tests.Helpers
{
    [Trait("Category", "Unit")]
    public class ApplicationTrackerTests
    {
        private static HistoryEntry Entry(long id, string? status = null) => new()
        {
            Id = id, JobTitle = $"Job {id}", Status = status ?? ApplicationTracker.DefaultStatus
        };

        private static long[] Ids(IEnumerable<HistoryEntry> entries) => entries.Select(e => e.Id).ToArray();

        private static readonly HistoryEntry[] Sample =
        {
            Entry(1, "Offer"), Entry(2, "Applied"), Entry(3, "Saved"), Entry(4, "Applied"), Entry(5, "Rejected"), Entry(6, "Interview")
        };

        [Theory]
        [InlineData("Applied", "Applied")]
        [InlineData("applied", "Applied")]
        [InlineData("  Offer ", "Offer")]
        [InlineData("Ghosted", "Saved")]
        [InlineData("", "Saved")]
        [InlineData(null, "Saved")]
        public void Status_is_normalized_to_a_known_one(string? input, string expected)
        {
            Assert.Equal(expected, ApplicationTracker.Normalize(input));
        }

        [Fact]
        public void Statuses_follow_the_pipeline_order()
        {
            Assert.Equal(new[] { "Saved", "Applied", "Interview", "Offer", "Rejected" }, ApplicationTracker.Statuses);
        }

        [Fact]
        public void New_entries_default_to_saved_with_no_details()
        {
            var entry = new HistoryEntry();

            Assert.Equal("Saved", entry.Status);
            Assert.Equal(string.Empty, entry.Company);
            Assert.Equal(string.Empty, entry.Notes);
        }

        [Fact]
        public void An_entry_saved_before_the_tracker_reads_as_saved()
        {
            var old = JsonSerializer.Deserialize<HistoryEntry>(
                """{"id":7,"jobTitle":"Old","generatedAt":"2026-01-01T00:00:00Z","template":"Harvard","jobDescription":"x"}""",
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

            Assert.Equal("Saved", old.Status);
            Assert.Equal(string.Empty, old.Company);
        }

        [Fact]
        public void Without_a_filter_every_entry_is_kept_newest_first()
        {
            Assert.Equal(new long[] { 6, 5, 4, 3, 2, 1 }, Ids(ApplicationTracker.Apply(Sample, null, HistorySort.Newest)));
            Assert.Equal(new long[] { 6, 5, 4, 3, 2, 1 }, Ids(ApplicationTracker.Apply(Sample, "", HistorySort.Newest)));
        }

        [Fact]
        public void Oldest_first_reverses_the_order()
        {
            Assert.Equal(new long[] { 1, 2, 3, 4, 5, 6 }, Ids(ApplicationTracker.Apply(Sample, null, HistorySort.Oldest)));
        }

        [Fact]
        public void Filter_keeps_only_the_chosen_status()
        {
            Assert.Equal(new long[] { 4, 2 }, Ids(ApplicationTracker.Apply(Sample, "Applied", HistorySort.Newest)));
            Assert.Empty(ApplicationTracker.Apply(Sample.Where(e => e.Id != 5), "Rejected", HistorySort.Newest));
        }

        [Fact]
        public void Filter_matches_an_entry_with_an_unknown_status_as_saved()
        {
            var entries = new[] { Entry(1, "Ghosted"), Entry(2, "Applied") };

            Assert.Equal(new long[] { 1 }, Ids(ApplicationTracker.Apply(entries, "Saved", HistorySort.Newest)));
        }

        [Fact]
        public void Sorting_by_status_follows_the_pipeline_then_newest_first()
        {
            Assert.Equal(new long[] { 3, 4, 2, 6, 1, 5 }, Ids(ApplicationTracker.Apply(Sample, null, HistorySort.Status)));
        }

        [Fact]
        public void Counts_cover_every_status_including_empty_ones()
        {
            var counts = ApplicationTracker.CountByStatus(Sample);

            Assert.Equal(2, counts["Applied"]);
            Assert.Equal(1, counts["Saved"]);
            Assert.Equal(5, counts.Count);
            Assert.Equal(0, ApplicationTracker.CountByStatus(Array.Empty<HistoryEntry>())["Offer"]);
        }

        [Fact]
        public void Apply_does_not_change_the_list_it_is_given()
        {
            var list = Sample.ToList();

            ApplicationTracker.Apply(list, "Applied", HistorySort.Status);

            Assert.Equal(Ids(Sample), Ids(list));
        }
    }
}

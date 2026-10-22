using Bunit;
using ErCv.Web.Components.Shared;
using ErCv.Web.UI;
using Microsoft.AspNetCore.Components.Web;

namespace ErCv.Tests.Components;

[Trait("Category", "Component")]
public class HistoryPanelTests : BunitContext
{
    private static List<HistoryEntry> Entries() => new()
    {
        new() { Id = 1, JobTitle = "Backend Engineer", GeneratedAt = "2026-01-01T00:00:00Z", Template = "Harvard", Status = "Applied" },
        new() { Id = 2, JobTitle = "Data Analyst", GeneratedAt = "2026-02-01T00:00:00Z", Template = "Modern", Status = "Saved" },
        new() { Id = 3, JobTitle = "SRE", GeneratedAt = "2026-03-01T00:00:00Z", Template = "Modern", Status = "Offer", Company = "Acme", Role = "Site Reliability Engineer" }
    };

    private IRenderedComponent<HistoryPanel> Render(List<HistoryEntry> entries, Action<HistoryEntry>? onUpdate = null) =>
        Render<HistoryPanel>(p => p
            .Add(c => c.Entries, entries)
            .Add(c => c.OnUpdate, (HistoryEntry e) => onUpdate?.Invoke(e)));

    private static string[] Titles(IRenderedComponent<HistoryPanel> cut) =>
        cut.FindAll(".history-title").Select(t => t.TextContent.Trim()).ToArray();

    [Fact]
    public void Entries_are_listed_newest_first_with_their_status()
    {
        var cut = Render(Entries());

        Assert.Equal(new[] { "Site Reliability Engineer at Acme", "Data Analyst", "Backend Engineer" }, Titles(cut));
        Assert.Equal(new[] { "Offer", "Saved", "Applied" }, cut.FindAll(".history-status").Select(s => s.TextContent).ToArray());
    }

    [Fact]
    public void Filter_shows_counts_and_only_the_chosen_status()
    {
        var cut = Render(Entries());

        Assert.Contains("Applied (1)", cut.Find("#history-filter").TextContent);
        Assert.Contains("All (3)", cut.Find("#history-filter").TextContent);

        cut.Find("#history-filter").Change("Applied");

        Assert.Equal(new[] { "Backend Engineer" }, Titles(cut));
    }

    [Fact]
    public void A_filter_with_no_match_says_so()
    {
        var cut = Render(Entries());

        cut.Find("#history-filter").Change("Rejected");

        Assert.Empty(cut.FindAll(".history-item"));
        Assert.Contains("No applications with this status", cut.Find(".history-no-match").TextContent);
    }

    [Fact]
    public void Sort_by_status_follows_the_pipeline()
    {
        var cut = Render(Entries());

        cut.Find("#history-sort").Change("Status");

        Assert.Equal(new[] { "Data Analyst", "Backend Engineer", "Site Reliability Engineer at Acme" }, Titles(cut));
    }

    [Fact]
    public void Sort_oldest_first()
    {
        var cut = Render(Entries());

        cut.Find("#history-sort").Change("Oldest");

        Assert.Equal("Backend Engineer", Titles(cut)[0]);
    }

    [Fact]
    public void Details_open_and_close_for_one_entry()
    {
        var cut = Render(Entries());
        Assert.Empty(cut.FindAll(".history-details"));

        cut.FindAll("button").First(b => b.TextContent == "Details").Click();
        Assert.Single(cut.FindAll(".history-details"));

        cut.FindAll("button").First(b => b.TextContent == "Details").Click();
        Assert.Empty(cut.FindAll(".history-details"));
    }

    [Fact]
    public void Editing_company_role_status_and_notes_reports_the_entry()
    {
        var entries = Entries();
        var updated = new List<HistoryEntry>();
        var cut = Render(entries, updated.Add);

        // The first row is entry 3 (newest)
        cut.FindAll("button").First(b => b.TextContent == "Details").Click();
        cut.Find("#app-company-3").Change("Globex");
        cut.Find("#app-role-3").Change("Platform Engineer");
        cut.Find("#app-status-3").Change("Interview");
        cut.Find("#app-notes-3").Change("Phone screen on Monday");

        Assert.Equal(4, updated.Count);
        var entry = entries.Single(e => e.Id == 3);
        Assert.Equal("Globex", entry.Company);
        Assert.Equal("Platform Engineer", entry.Role);
        Assert.Equal("Interview", entry.Status);
        Assert.Equal("Phone screen on Monday", entry.Notes);
        Assert.Equal("Platform Engineer at Globex", Titles(cut)[0]);
    }

    [Fact]
    public void An_unknown_status_value_is_stored_as_saved()
    {
        var entries = Entries();
        var cut = Render(entries);

        cut.FindAll("button").First(b => b.TextContent == "Details").Click();
        cut.Find("#app-status-3").Change("Ghosted");

        Assert.Equal("Saved", entries.Single(e => e.Id == 3).Status);
    }

    [Fact]
    public void Reopen_delete_and_clear_report_the_entry_the_id_and_the_clear()
    {
        var loaded = new List<HistoryEntry>();
        var deleted = new List<long>();
        var cleared = 0;
        var cut = Render<HistoryPanel>(p => p
            .Add(c => c.Entries, Entries())
            .Add(c => c.OnLoad, (HistoryEntry e) => loaded.Add(e))
            .Add(c => c.OnDelete, (long id) => deleted.Add(id))
            .Add(c => c.OnClear, () => cleared++));

        // Rows are newest first: entry 3, then 2, then 1. Delete and Clear go through the confirm dialog.
        cut.FindAll("button").Where(b => b.TextContent == "Re-open").ElementAt(1).Click();
        cut.FindAll(".history-delete-btn").ElementAt(2).Click();
        cut.Find("#confirm-ok").Click();
        cut.FindAll("button").Single(b => b.TextContent.Trim() == "Clear").Click();
        cut.Find("#confirm-ok").Click();

        Assert.Equal(2, Assert.Single(loaded).Id);
        Assert.Equal(1, Assert.Single(deleted));
        Assert.Equal(1, cleared);
    }

    [Fact]
    public void Delete_and_clear_do_nothing_until_confirmed_and_cancel_backs_out()
    {
        var deleted = new List<long>();
        var cleared = 0;
        var cut = Render<HistoryPanel>(p => p
            .Add(c => c.Entries, Entries())
            .Add(c => c.OnDelete, (long id) => deleted.Add(id))
            .Add(c => c.OnClear, () => cleared++));

        var delete = cut.FindAll(".history-delete-btn").First();
        Assert.Equal("Delete Site Reliability Engineer at Acme", delete.GetAttribute("aria-label"));
        delete.Click();
        Assert.Contains("Site Reliability Engineer at Acme", cut.Find("#confirm-body strong").TextContent);
        cut.Find("#confirm-cancel").Click();

        cut.FindAll("button").Single(b => b.TextContent.Trim() == "Clear").Click();
        Assert.Equal("all 3 CVs", cut.Find("#confirm-body strong").TextContent);
        cut.Find(".confirm-dialog").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        Assert.Empty(deleted);
        Assert.Equal(0, cleared);
        Assert.Empty(cut.FindAll(".confirm-dialog"));
    }

    [Fact]
    public void While_disabled_reopen_delete_and_clear_cannot_be_used()
    {
        var cut = Render<HistoryPanel>(p => p.Add(c => c.Entries, Entries()).Add(c => c.Disabled, true));

        var actions = cut.FindAll("button").Where(b => b.TextContent.Trim() is "Re-open" or "Clear")
            .Concat(cut.FindAll(".history-delete-btn")).ToList();
        Assert.Equal(7, actions.Count);
        Assert.All(actions, b => Assert.True(b.HasAttribute("disabled")));
    }

    [Fact]
    public void Clear_is_disabled_when_there_is_nothing_to_clear()
    {
        var cut = Render(new List<HistoryEntry>());

        Assert.True(cut.FindAll("button").Single(b => b.TextContent.Trim() == "Clear").HasAttribute("disabled"));
    }

    [Fact]
    public void Empty_history_shows_the_empty_state_and_no_controls()
    {
        var cut = Render(new List<HistoryEntry>());

        Assert.Contains("No generations yet", cut.Markup);
        Assert.Empty(cut.FindAll("#history-filter"));
    }
}

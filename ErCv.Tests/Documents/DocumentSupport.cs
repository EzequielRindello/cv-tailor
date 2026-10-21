using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Selection;
using ErCv.Web.Models;

namespace ErCv.Tests.Documents;

public static class DocumentSupport
{
    public static TheoryData<string, string, bool> ProfileJdPages()
    {
        var data = new TheoryData<string, string, bool>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var jd in FixtureLoader.JdNames)
        foreach (var twoPages in new[] { false, true })
            data.Add(profile, jd, twoPages);
        return data;
    }

    public static TheoryData<string, string, CVTemplate, bool> ProfileJdTemplatePages()
    {
        var data = new TheoryData<string, string, CVTemplate, bool>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var jd in FixtureLoader.JdNames)
        foreach (var template in Enum.GetValues<CVTemplate>())
        foreach (var twoPages in new[] { false, true })
            data.Add(profile, jd, template, twoPages);
        return data;
    }

    public static GeneratedCV Generate(string profile, string jd, bool twoPages)
    {
        var master = FixtureLoader.LoadProfile(profile);
        var req = new JobDescriptionParser().Parse(FixtureLoader.LoadJd(jd));
        return new CVSelector().SelectAndAdaptCV(master, req, twoPages);
    }

    public static DocumentRenderer Renderer(CVTemplate template) =>
        DocumentRenderer.ForTemplate(Path.Combine(AppContext.BaseDirectory, TemplatePaths.TemplatesFolder), template);

    public static int BulletCount(GeneratedCV cv) => cv.Experience.Sum(e => e.Bullets.Count);

    /// <summary>
    /// Titles the template prints for the sections this CV actually has, in its reading order: the CV's order or the
    /// template's own, in the CV's language. Modern reads its left column (skills, education) first.
    /// </summary>
    public static List<string> ExpectedTitles(GeneratedCV cv, CVTemplate template)
    {
        var labels = CvLabels.For(cv.Language);
        var order = DocumentRenderer.ResolveSectionOrder(cv.SectionOrder, DocumentRenderer.DefaultSectionOrder(template));
        if (template == CVTemplate.Modern)
            order = order.Where(LeftColumn.Contains).Concat(order.Where(k => !LeftColumn.Contains(k))).ToList();

        var titles = new List<string>();
        foreach (var key in order)
        {
            switch (key)
            {
                case DocumentConstants.SectionSummaryKey when !string.IsNullOrEmpty(cv.Summary):
                    titles.Add(DocumentRenderer.SummaryTitle(template, labels));
                    break;
                case DocumentConstants.SectionEducationKey:
                    titles.Add(labels.Education);
                    break;
                case DocumentConstants.SectionExperienceKey:
                    titles.Add(labels.Experience);
                    break;
                case DocumentConstants.SectionSkillsKey when cv.Skills.Any():
                    titles.Add(labels.Skills);
                    break;
                case DocumentConstants.SectionProjectsKey when cv.Projects.Any():
                    titles.Add(labels.Projects);
                    break;
            }
        }
        return titles;
    }

    private static readonly string[] LeftColumn = { DocumentConstants.SectionSkillsKey, DocumentConstants.SectionEducationKey };

    /// <summary>Lowercase letters and digits only, so line wraps, entities and punctuation do not matter.</summary>
    public static string Squash(string text) =>
        new(System.Net.WebUtility.HtmlDecode(text).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

[CollectionDefinition("Documents")]
public class DocumentsCollection : ICollectionFixture<ChromiumFixture> { }

/// <summary>Closes the shared headless Chromium when the document tests finish.</summary>
public class ChromiumFixture : IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => DocumentRenderer.ShutdownAsync();
}

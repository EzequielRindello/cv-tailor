using ErCv.Tests.Fixtures;
using ErCvApi.Constants;
using ErCvApi.Helpers;
using ErCvApi.Models;

namespace ErCv.Tests.Documents
{
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

        public static DocumentRenderer Renderer(CVTemplate template)
        {
            var file = template switch
            {
                CVTemplate.Modern => TemplatePaths.ModernTemplate,
                CVTemplate.Minimalist => TemplatePaths.MinimalistTemplate,
                _ => TemplatePaths.HarvardTemplate
            };
            return new DocumentRenderer(Path.Combine(AppContext.BaseDirectory, TemplatePaths.TemplatesFolder, file));
        }

        public static int BulletCount(GeneratedCV cv) => cv.Experience.Sum(e => e.Bullets.Count);

        /// <summary>Titles the template prints for the sections this CV actually has, in template reading order.</summary>
        public static List<string> ExpectedTitles(GeneratedCV cv, CVTemplate template)
        {
            var hasSummary = !string.IsNullOrEmpty(cv.Summary);
            var hasSkills = cv.Skills.Any();
            var hasProjects = cv.Projects.Any();

            var titles = new List<string>();
            switch (template)
            {
                case CVTemplate.Minimalist:
                    if (hasSummary) titles.Add("Profile");
                    titles.Add("Experience");
                    titles.Add("Education");
                    if (hasSkills) titles.Add("Skills");
                    break;
                case CVTemplate.Modern:
                    if (hasSkills) titles.Add("Skills");
                    titles.Add("Education");
                    if (hasSummary) titles.Add("Profile");
                    titles.Add("Experience");
                    break;
                default:
                    if (hasSummary) titles.Add("Professional Summary");
                    titles.Add("Education");
                    titles.Add("Experience");
                    if (hasSkills) titles.Add("Skills");
                    break;
            }
            if (hasProjects) titles.Add("Projects");
            return titles;
        }

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
}

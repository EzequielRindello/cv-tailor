using System.Text.Json;
using ErCv.Collector;
using ErCvApi.Models;

namespace ErCv.Tests.Fixtures
{
    public static class FixtureLoader
    {
        public static readonly string[] ProfileIds =
        {
            "cv-master",
            "cv-master-senior",
            "cv-master-junior",
            "cv-master-edge"
        };

        public static readonly string[] JdNames =
        {
            "backend-go-kafka",
            "frontend-react",
            "data-engineer",
            "devops",
            "sales-non-tech",
            "junior",
            "empty",
            "very-long",
            "spanish"
        };

        public const string CorpusPrefix = "corpus/";

        /// <summary>The committed job-post corpus (APP-05), read from the repository so a refresh is picked up without a rebuild.</summary>
        public static readonly CorpusStore Corpus = new(Path.Combine(RepoRoot(), "corpus"));

        public static readonly IReadOnlyList<Posting> CorpusPostings = Corpus.LoadPostings();

        /// <summary>One real post per role family (the newest), so the profile x JD matrices stay a reasonable size.</summary>
        public static readonly string[] CorpusJdNames = CorpusPostings
            .GroupBy(p => p.RoleFamily)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => CorpusPrefix + g.OrderByDescending(p => p.PostedAt).ThenBy(p => p.Id, StringComparer.Ordinal).First().Id)
            .ToArray();

        /// <summary>The hand-written edge cases plus the real posts.</summary>
        public static readonly string[] AllJdNames = JdNames.Concat(CorpusJdNames).ToArray();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static CVMaster LoadProfile(string id)
        {
            var json = File.ReadAllText(FixturePath("profiles", $"{id}.json"));
            return JsonSerializer.Deserialize<CVMaster>(json, JsonOptions)
                ?? throw new InvalidOperationException($"Failed to deserialize profile '{id}'");
        }

        public static string LoadJd(string name) =>
            name.StartsWith(CorpusPrefix, StringComparison.Ordinal)
                ? CorpusPostings.Single(p => p.Id == name[CorpusPrefix.Length..]).JobDescription
                : File.ReadAllText(FixturePath("jds", $"{name}.txt"));

        public static TheoryData<string> Profiles()
        {
            var data = new TheoryData<string>();
            foreach (var id in ProfileIds) data.Add(id);
            return data;
        }

        public static TheoryData<string> Jds()
        {
            var data = new TheoryData<string>();
            foreach (var name in AllJdNames) data.Add(name);
            return data;
        }

        public static TheoryData<string> CorpusPostingIds()
        {
            var data = new TheoryData<string>();
            foreach (var p in CorpusPostings) data.Add(p.Id);
            return data;
        }

        public static string RepoRoot()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
                if (File.Exists(Path.Combine(dir.FullName, "er-cv-generator.sln"))) return dir.FullName;
            throw new DirectoryNotFoundException("Repository root (er-cv-generator.sln) not found above the test output");
        }

        private static string FixturePath(string folder, string file) =>
            Path.Combine(AppContext.BaseDirectory, "Fixtures", folder, file);
    }
}

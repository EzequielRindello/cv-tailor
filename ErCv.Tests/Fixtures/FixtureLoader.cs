using System.Text.Json;
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
            File.ReadAllText(FixturePath("jds", $"{name}.txt"));

        public static TheoryData<string> Profiles()
        {
            var data = new TheoryData<string>();
            foreach (var id in ProfileIds) data.Add(id);
            return data;
        }

        public static TheoryData<string> Jds()
        {
            var data = new TheoryData<string>();
            foreach (var name in JdNames) data.Add(name);
            return data;
        }

        private static string FixturePath(string folder, string file) =>
            Path.Combine(AppContext.BaseDirectory, "Fixtures", folder, file);
    }
}

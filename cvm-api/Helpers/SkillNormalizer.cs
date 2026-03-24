namespace ErCvApi.Helpers
{
    public static class SkillNormalizer
    {
        private static readonly Dictionary<string, string> Aliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "dotnet",             ".net" },
                { "dot net",            ".net" },
                { ".net core",          ".net" },
                { "asp.net",            "asp.net core" },
                { "aspnet",             "asp.net core" },
                { "asp.net core",       "asp.net core" },
                { "c sharp",            "c#" },
                { "csharp",             "c#" },
                { "js",                 "javascript" },
                { "ecmascript",         "javascript" },
                { "ts",                 "typescript" },
                { "react.js",           "react" },
                { "reactjs",            "react" },
                { "nextjs",             "next.js" },
                { "next",               "next.js" },
                { "vue.js",             "vue" },
                { "vuejs",              "vue" },
                { "nuxt.js",            "nuxt" },
                { "nuxtjs",             "nuxt" },
                { "angularjs",          "angular" },
                { "angular.js",         "angular" },
                { "node",               "node.js" },
                { "nodejs",             "node.js" },
                { "nest",               "nestjs" },
                { "nest.js",            "nestjs" },
                { "py",                 "python" },
                { "golang",             "go" },
                { "postgres",           "postgresql" },
                { "mongo",              "mongodb" },
                { "mssql",              "sql server" },
                { "ms sql",             "sql server" },
                { "mysql server",       "mysql" },
                { "redis cache",        "redis" },
                { "elastic",            "elasticsearch" },
                { "opensearch",         "elasticsearch" },
                { "amazon web services","aws" },
                { "google cloud",       "gcp" },
                { "google cloud platform", "gcp" },
                { "microsoft azure",    "azure" },
                { "k8s",                "kubernetes" },
                { "kube",               "kubernetes" },
                { "docker container",   "docker" },
                { "docker compose",     "docker" },
                { "cicd",               "ci/cd" },
                { "ci cd",              "ci/cd" },
                { "continuous integration", "ci/cd" },
                { "continuous deployment",  "ci/cd" },
                { "continuous delivery",    "ci/cd" },
                { "github action",      "github actions" },
                { "gh actions",         "github actions" },
                { "tf",                 "terraform" },
                { "graph ql",           "graphql" },
                { "rest api",           "rest" },
                { "restful",            "rest" },
                { "restful api",        "rest" },
                { "grpc api",           "grpc" },
                { "ef core",            "entity framework" },
                { "ef",                 "entity framework" },
                { "type orm",           "typeorm" },
                { "tailwindcss",        "tailwind" },
                { "tailwind css",       "tailwind" },
                { "sass",               "scss" },
                { "rn",                 "react native" },
                { "ml",                 "machine learning" },
                { "artificial intelligence", "ai" },
                { "sklearn",            "scikit-learn" },
                { "test driven development", "tdd" },
                { "test-driven development", "tdd" },
                { "agile methodology",  "agile" },
                { "agile methodologies","agile" },
                { "scrum methodology",  "scrum" },
                { "micro services",     "microservices" },
                { "micro-services",     "microservices" },
            };
        public static string Normalize(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill)) return "";
            var trimmed = skill.Trim().ToLower();
            return Aliases.TryGetValue(trimmed, out var canonical) ? canonical : trimmed;
        }
        public static bool FuzzyMatch(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;

            var na = Normalize(a);
            var nb = Normalize(b);
            if (na == nb) return true;
            if (na.Contains(nb) || nb.Contains(na)) return true;

            return false;
        }
        public static bool MatchesAny(string skill, IEnumerable<string> collection)
            => collection.Any(c => FuzzyMatch(skill, c));
    }
}

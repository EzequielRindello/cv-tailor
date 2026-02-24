namespace ErCvApi.Helpers
{
    /// <summary>
    /// Centralizes all skill matching logic.
    /// All scorers and analyzers must use FuzzyMatch() instead of
    /// raw string comparisons so matching stays consistent across the app.
    /// </summary>
    public static class SkillNormalizer
    {
        // Canonical aliases — both directions covered so Normalize()
        // always returns the same canonical string regardless of input variant.
        private static readonly Dictionary<string, string> Aliases =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // .NET family
                { "dotnet",             ".net" },
                { "dot net",            ".net" },
                { ".net core",          ".net" },
                { "asp.net",            "asp.net core" },
                { "aspnet",             "asp.net core" },
                { "asp.net core",       "asp.net core" },
                { "c sharp",            "c#" },
                { "csharp",             "c#" },

                // JavaScript / TypeScript
                { "js",                 "javascript" },
                { "ecmascript",         "javascript" },
                { "ts",                 "typescript" },

                // React
                { "react.js",           "react" },
                { "reactjs",            "react" },

                // Next.js
                { "nextjs",             "next.js" },
                { "next",               "next.js" },

                // Vue
                { "vue.js",             "vue" },
                { "vuejs",              "vue" },

                // Nuxt
                { "nuxt.js",            "nuxt" },
                { "nuxtjs",             "nuxt" },

                // Angular
                { "angularjs",          "angular" },
                { "angular.js",         "angular" },

                // Node
                { "node",               "node.js" },
                { "nodejs",             "node.js" },

                // NestJS
                { "nest",               "nestjs" },
                { "nest.js",            "nestjs" },

                // Python
                { "py",                 "python" },

                // Go
                { "golang",             "go" },

                // Databases
                { "postgres",           "postgresql" },
                { "mongo",              "mongodb" },
                { "mssql",              "sql server" },
                { "ms sql",             "sql server" },
                { "mysql server",       "mysql" },

                // Redis
                { "redis cache",        "redis" },

                // Elasticsearch
                { "elastic",            "elasticsearch" },
                { "opensearch",         "elasticsearch" },

                // Cloud
                { "amazon web services","aws" },
                { "google cloud",       "gcp" },
                { "google cloud platform", "gcp" },
                { "microsoft azure",    "azure" },

                // Kubernetes
                { "k8s",                "kubernetes" },
                { "kube",               "kubernetes" },

                // Docker
                { "docker container",   "docker" },
                { "docker compose",     "docker" },

                // CI/CD
                { "cicd",               "ci/cd" },
                { "ci cd",              "ci/cd" },
                { "continuous integration", "ci/cd" },
                { "continuous deployment",  "ci/cd" },
                { "continuous delivery",    "ci/cd" },

                // GitHub Actions
                { "github action",      "github actions" },
                { "gh actions",         "github actions" },

                // Terraform
                { "tf",                 "terraform" },

                // GraphQL
                { "graph ql",           "graphql" },

                // REST
                { "rest api",           "rest" },
                { "restful",            "rest" },
                { "restful api",        "rest" },

                // gRPC
                { "grpc api",           "grpc" },

                // Entity Framework
                { "ef core",            "entity framework" },
                { "ef",                 "entity framework" },

                // TypeORM
                { "type orm",           "typeorm" },

                // Tailwind
                { "tailwindcss",        "tailwind" },
                { "tailwind css",       "tailwind" },

                // SCSS / Sass
                { "sass",               "scss" },

                // React Native
                { "rn",                 "react native" },

                // Machine Learning
                { "ml",                 "machine learning" },
                { "artificial intelligence", "ai" },

                // Scikit-learn
                { "sklearn",            "scikit-learn" },

                // TDD
                { "test driven development", "tdd" },
                { "test-driven development", "tdd" },

                // Agile
                { "agile methodology",  "agile" },
                { "agile methodologies","agile" },

                // Scrum
                { "scrum methodology",  "scrum" },

                // Microservices
                { "micro services",     "microservices" },
                { "micro-services",     "microservices" },
            };

        /// <summary>
        /// Returns the canonical lowercase form of a skill.
        /// "NodeJS" → "node.js", "TS" → "typescript", etc.
        /// </summary>
        public static string Normalize(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill)) return "";
            var trimmed = skill.Trim().ToLower();
            return Aliases.TryGetValue(trimmed, out var canonical) ? canonical : trimmed;
        }

        /// <summary>
        /// True if two skill strings refer to the same technology.
        /// Uses alias normalization first, then substring containment as fallback.
        /// </summary>
        public static bool FuzzyMatch(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;

            var na = Normalize(a);
            var nb = Normalize(b);

            // Exact match after normalization
            if (na == nb) return true;

            // Substring containment (handles "asp.net core" ↔ ".net", etc.)
            if (na.Contains(nb) || nb.Contains(na)) return true;

            return false;
        }

        /// <summary>
        /// True if a skill matches any item in a collection.
        /// </summary>
        public static bool MatchesAny(string skill, IEnumerable<string> collection)
            => collection.Any(c => FuzzyMatch(skill, c));
    }
}

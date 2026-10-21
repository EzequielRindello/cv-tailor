namespace ErCv.Web.Features.Matching;

public static class SkillNormalizer
{
    #region Constants

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

    // Phrases that look like an extension of another skill but are a different technology.
    private static readonly HashSet<string> DistinctProducts =
        new(StringComparer.OrdinalIgnoreCase) { "react native" };

    private static readonly Lazy<Dictionary<string, List<string>>> AliasesByCanonical = new(() =>
        Aliases.GroupBy(kvp => kvp.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key).ToList(), StringComparer.OrdinalIgnoreCase));

    #endregion

    #region Public Methods

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
        if (DistinctProducts.Contains(na) != DistinctProducts.Contains(nb)) return false;

        return ContainsTerm(na, nb) || ContainsTerm(nb, na);
    }

    /// <summary>
    /// True when <paramref name="term"/> appears in <paramref name="text"/> as a whole token, so "go" does not
    /// match "google" or "good", "java" does not match "javascript" and "c" does not match "c#" or "c++".
    /// </summary>
    public static bool ContainsTerm(string? text, string? term) => FindTerm(text, term).Any();

    /// <summary>Every place <paramref name="term"/> appears in <paramref name="text"/> as a whole token (see <see cref="ContainsTerm"/>).</summary>
    public static IEnumerable<(int Start, int Length)> FindTerm(string? text, string? term)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term)) yield break;

        term = term.Trim();
        var at = 0;
        while ((at = text.IndexOf(term, at, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            var end = at + term.Length;
            var startsToken = at == 0 || !char.IsLetterOrDigit(text[at - 1]);
            var endsToken = end >= text.Length || !(char.IsLetterOrDigit(text[end]) || text[end] == '#' || text[end] == '+');
            if (startsToken && endsToken)
            {
                yield return (at, term.Length);
                at = end;
            }
            else
            {
                at++;
            }
        }
    }

    /// <summary>Like <see cref="FindTerm"/>, but also finds every alias of the skill ("golang" for "go").</summary>
    public static IEnumerable<(int Start, int Length)> FindSkill(string? text, string? skill)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(skill)) return Enumerable.Empty<(int, int)>();

        var canonical = Normalize(skill);
        var terms = new List<string> { canonical };
        if (AliasesByCanonical.Value.TryGetValue(canonical, out var aliases)) terms.AddRange(aliases);

        return terms.SelectMany(t => FindTerm(text, t));
    }

    /// <summary>Like <see cref="ContainsTerm"/>, but also accepts any alias of the skill ("golang" for "go").</summary>
    public static bool ContainsSkill(string? text, string? skill)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(skill)) return false;

        var canonical = Normalize(skill);
        if (ContainsTerm(text, canonical)) return true;

        return AliasesByCanonical.Value.TryGetValue(canonical, out var aliases) &&
               aliases.Any(alias => ContainsTerm(text, alias));
    }
    public static bool MatchesAny(string skill, IEnumerable<string> collection)
        => collection.Any(c => FuzzyMatch(skill, c));

    #endregion
}

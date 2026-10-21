using System.Text.RegularExpressions;

namespace ErCv.Collector;

/// <summary>
/// Role family and seniority from the post title. The corpus only keeps individual-contributor software roles,
/// the people this app writes CVs for, so sales, design, management and hardware posts are left out.
/// </summary>
public static partial class Classifier
{
    #region Constants

    public const string Software = "software";

    // A title needs one of these to count as an engineering role
    private static readonly string[] EngineeringNouns =
    {
        "engineer", "developer", "programmer", "sre", "devops", "data scientist", "swe", "member of technical staff"
    };

    private static readonly string[] Excluded =
    {
        "manager", "director", "head of", "vp", "vice president", "chief", "recruit", "sales", "account",
        "solutions", "solution engineer", "support", "customer", "success", "partner", "marketing", "designer",
        "counsel", "legal", "finance", "accountant", "hardware", "mechanical", "electrical", "silicon", "asic",
        "fpga", "rf", "manufacturing", "process engineer", "facilities",
        "network engineer", "validation", "test technician", "field", "pre-sales", "presales", "deployment strategist",
        "physical design", "analog", "firmware", "embedded", "robotics", "controls", "optical", "thermal",
        "civil", "construction", "it engineer", "workplace", "evangelist", "advocate", "writer",
        "technical services", "escalation", "escalations", "it ops", "it systems", "gtm", "compliance", "grc",
        "enterprise engineer", "corporate", "leader", "logistics", "supply chain", "netsuite", "salesforce",
        "workday", "servicenow", "engagement", "developer relations", "chassis", "vehicle", "actuation", "powertrain"
    };

    // First match wins, so the more specific families come first ("full stack" before "frontend")
    private static readonly (string Family, string[] Terms)[] Families =
    {
        ("security", new[] { "security", "appsec", "detection and response", "threat detection", "offensive", "cryptography" }),
        ("qa", new[] { "qa", "quality", "test", "sdet", "test automation" }),
        ("mobile", new[] { "ios", "android", "mobile", "react native", "flutter" }),
        ("fullstack", new[] { "full stack", "full-stack", "fullstack" }),
        ("frontend", new[] { "frontend", "front end", "front-end", "ui engineer", "web engineer", "web developer", "web platform", "design engineer", "ux engineer" }),
        ("backend", new[] { "backend", "back end", "back-end", "api", "server", "distributed systems" }),
        ("ml", new[] { "machine learning", "ml", "ai", "llm", "applied scientist", "research engineer", "inference", "model" }),
        ("data", new[] { "data", "analytics", "etl" }),
        ("devops", new[] { "devops", "sre", "site reliability", "platform", "infrastructure", "cloud", "reliability", "production engineer", "kubernetes", "observability" }),
    };

    // Level words that leave no doubt. "Associate" and numbered levels ("Engineer 3") mean different things per company.
    private static readonly string[] ClearLevelWords =
    {
        "intern", "internship", "new grad", "graduate", "junior", "jr", "entry level", "entry-level", "early career",
        "senior", "sr", "staff", "lead", "principal", "distinguished"
    };

    [GeneratedRegex(@"\b(engineer|developer) i\b")]
    private static partial Regex NumberedJunior();

    [GeneratedRegex(@"\b(iii|iv)\b")]
    private static partial Regex NumberedSenior();

    [GeneratedRegex(@"[()\[\],/|:;]")]
    private static partial Regex TitlePunctuation();

    #endregion

    #region Public Methods

    /// <summary>The role family, or null when the title is not an individual-contributor software role.</summary>
    public static string? RoleFamily(string title)
    {
        var t = Prepare(title);
        if (!EngineeringNouns.Any(n => HasWord(t, n))) return null;
        if (Excluded.Any(x => HasWord(t, x))) return null;

        foreach (var (family, terms) in Families)
            if (terms.Any(term => HasWord(t, term))) return family;
        return Software;
    }

    /// <summary>True when the title names its level with a word that leaves no doubt ("Senior", "Junior", not "Associate").</summary>
    public static bool NamesClearLevel(string title)
    {
        var t = Prepare(title).Replace("member of technical staff", " ");
        return ClearLevelWords.Any(w => HasWord(t, w));
    }

    public static string Seniority(string title)
    {
        var t = Prepare(title);
        if (HasWord(t, "intern") || HasWord(t, "internship") || HasWord(t, "co-op")) return "intern";
        if (HasWord(t, "new grad") || HasWord(t, "graduate") || HasWord(t, "university grad") || HasWord(t, "junior")
            || HasWord(t, "jr") || HasWord(t, "entry level") || HasWord(t, "entry-level") || HasWord(t, "early career")
            || HasWord(t, "associate")) return "junior";
        if (HasWord(t, "principal") || HasWord(t, "distinguished") || HasWord(t, "fellow")) return "principal";
        if (HasWord(t, "staff") && !HasWord(t, "member of technical staff")) return "staff";
        if (HasWord(t, "lead")) return "lead";
        if (HasWord(t, "senior") || HasWord(t, "sr")) return "senior";
        // Numbered levels only count when there is no level word ("Principal Software Engineer I" is principal)
        if (NumberedJunior().IsMatch(t)) return "junior";
        if (NumberedSenior().IsMatch(t)) return "senior";
        return "mid";
    }

    #endregion

    #region Private Methods

    private static string Prepare(string title) =>
        " " + TitlePunctuation().Replace(title.ToLowerInvariant(), " ").Replace('.', ' ') + " ";

    private static bool HasWord(string text, string term) =>
        Regex.IsMatch(text, $@"(?<![a-z0-9]){Regex.Escape(term)}(?![a-z0-9])");

    #endregion
}

using System.Text.RegularExpressions;

namespace ErCv.Collector
{
    /// <summary>
    /// Role family and seniority from the post title. The corpus only keeps individual-contributor software roles,
    /// the people this app writes CVs for, so sales, design, management and hardware posts are left out.
    /// </summary>
    public static class Classifier
    {
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
            if (Regex.IsMatch(t, @"\b(engineer|developer) i\b")) return "junior";
            if (Regex.IsMatch(t, @"\b(iii|iv)\b")) return "senior";
            return "mid";
        }

        private static string Prepare(string title) =>
            " " + Regex.Replace(title.ToLowerInvariant(), @"[()\[\],/|:;]", " ").Replace('.', ' ') + " ";

        private static bool HasWord(string text, string term) =>
            Regex.IsMatch(text, $@"(?<![a-z0-9]){Regex.Escape(term)}(?![a-z0-9])");
    }
}

using System.Text.RegularExpressions;
using ErCv.Web.Constants;
using ErCv.Web.Features.Matching;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErCv.Web.Features.Selection;

public partial class SummaryGenerator
{
    #region Constants

    private const int MaxSummaryLength = 300;
    private const string TruncationSuffix = "...";
    private const int MaxKeyTechSkills = 5;
    private const int FallbackTopSkills = 4;
    private const int MaxSoftSkills = 2;
    private const string DefaultRoleType = "Software Engineer";
    private const string FrontendRoleType = "Frontend Developer";
    private const string BackendRoleType = "Backend Developer";
    private const string FullStackRoleType = "Full Stack Developer";
    private const string MobileRoleType = "Mobile Developer";
    private const string DataRoleType = "Data Engineer";
    private const string MlRoleType = "ML Engineer";
    private const string DevOpsRoleType = "DevOps Engineer";

    // The winning role needs this many distinct signals in the post, and this many more than the runner-up. One
    // stray mention (F29: Kotlin as "a backend language such as Java, Kotlin") must not set the role, and a tie
    // must not be decided by the order of RoleTypeSignals
    private const int MinRoleSignals = 2;
    private const int MinRoleLead = 1;

    // A role name that the post repeats is a signal too, even when it is not among the 30 most frequent words (the
    // company's own boilerplate can push it out). Once is a stray mention, so it takes this many (MATCH-11)
    private const int MinRoleNameMentions = 2;

    private static readonly Dictionary<string, string[]> RoleTypeSignals =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { DataRoleType,           new[] { "etl","spark","airflow","dbt","pipeline","warehouse","databricks" } },
            { MlRoleType,             new[] { "tensorflow","pytorch","machine learning","scikit","mlflow" } },
            { DevOpsRoleType,         new[] { "terraform","ansible","kubernetes","helm","argocd","infrastructure","sre" } },
            { FrontendRoleType,      new[] { "react","vue","angular","css","html","figma","tailwind","ui","frontend" } },
            { BackendRoleType,       new[] { "postgresql","mysql","rest api","grpc","kafka","rabbitmq","spring","node.js","backend","distributed systems" } },
            { MobileRoleType,         new[] { "swift","kotlin","flutter","react native","ios","android","xcode","mobile" } },
            { FullStackRoleType,     new[] { "fullstack","full stack","full-stack","end-to-end" } },
        };

    // When the title of the post names a role, the title decides (MATCH-10). Narrower than the signal lists on
    // purpose: no "platform", "api", "server", "data" or "model", which are ambiguous in a title
    private static readonly Dictionary<string, string[]> TitleRoleSignals =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { BackendRoleType,    new[] { "backend","back end","back-end" } },
            { FrontendRoleType,   new[] { "frontend","front end","front-end","ui engineer","web engineer","web developer" } },
            { FullStackRoleType,  new[] { "full stack","full-stack","fullstack" } },
            { MobileRoleType,     new[] { "mobile","ios","android","react native","flutter" } },
            { DevOpsRoleType,     new[] { "devops","sre","site reliability","infrastructure" } },
            { DataRoleType,       new[] { "data engineer","data engineering","analytics engineer" } },
            { MlRoleType,         new[] { "machine learning","ml","ai","llm" } },
        };

    // The words that name a role in the body of the post
    private static readonly Dictionary<string, string[]> RoleNameTerms =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { BackendRoleType,   new[] { "backend", "back-end", "back end" } },
            { FrontendRoleType,  new[] { "frontend", "front-end", "front end" } },
            { FullStackRoleType, new[] { "full stack", "full-stack", "fullstack" } },
            { MobileRoleType,    new[] { "mobile" } },
            { DevOpsRoleType,    new[] { "devops" } },
            { DataRoleType,      new[] { "data engineer", "data engineering" } },
        };

    // Checked in order; the first prefix found in the role wins ("mid" adds nothing to the title)
    private static readonly Dictionary<string, string> SeniorityPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "senior", "Senior" }, { "lead", "Lead" }, { "principal", "Principal" },
        { "staff", "Staff" },   { "junior", "Junior" }, { "associate", "Associate" },
        { "intern", "Intern" }, { "mid", "" }
    };

    // Word stem found in the job or the bullets, and the soft skill it is written as
    private static readonly Dictionary<string, string> SoftSkillStems = new()
    {
        { "team","team collaboration" }, { "leadership","leadership" },
        { "communication","communication" }, { "problem","problem-solving" },
        { "agile","agile methodologies" }, { "mentor","mentoring" },
        { "collaborat","cross-functional collaboration" }
    };

    // EDIT-04: the same soft skills written in Spanish. Role titles stay as they are: tech titles are usually
    // written in English in Spanish CVs too, and a translated title would have to pick a grammatical gender.
    private static readonly Dictionary<string, string> SpanishSoftSkills = new()
    {
        { "team collaboration", "trabajo en equipo" }, { "leadership", "liderazgo" },
        { "communication", "comunicación" }, { "problem-solving", "resolución de problemas" },
        { "agile methodologies", "metodologías ágiles" }, { "mentoring", "mentoría" },
        { "cross-functional collaboration", "colaboración entre equipos" }
    };

    [GeneratedRegex(@"(improving|increasing|reducing|achieving|exceeding|delivering|cutting|boosting).*?(\d+)%", RegexOptions.IgnoreCase)]
    private static partial Regex Achievement();

    #endregion

    #region Fields

    private readonly ILogger<SummaryGenerator> _logger;

    #endregion

    #region Constructors

    public SummaryGenerator(ILogger<SummaryGenerator>? logger = null)
    {
        _logger = logger ?? NullLogger<SummaryGenerator>.Instance;
    }

    #endregion

    #region Public Methods

    public string GenerateSummary(
        CVMaster cvMaster,
        JobRequirements requirements,
        List<Experience> selectedExperiences,
        List<string> topSkills,
        string language = ApiConstants.LanguageEnglish)
    {
        var spanish = language == ApiConstants.LanguageSpanish;
        var yearsOfExperience = CalculateTechYearsOfExperience(cvMaster.Experience);
        var candidateTitle = DeriveRoleFromCV(cvMaster.Experience, requirements);

        var keyTechSkills = topSkills
            .Where(s => requirements.TechnicalSkills.Any(ts => SkillNormalizer.FuzzyMatch(s, ts)))
            .Take(MaxKeyTechSkills).ToList();

        if (!keyTechSkills.Any()) keyTechSkills = topSkills.Take(FallbackTopSkills).ToList();

        // One sentence: "specializing in" continues the opening clause, it does not start a sentence of its own
        var parts = new List<string>();
        if (spanish)
        {
            parts.Add(keyTechSkills.Any()
                ? $"{candidateTitle} con {yearsOfExperience}+ años de experiencia, con foco en {FormatSpanishList(keyTechSkills)}"
                : $"{candidateTitle} con {yearsOfExperience}+ años de experiencia");

            // The achievement is quoted from a bullet, which is in the profile's language, so it is left out here
            var softSkills = ExtractSoftSkills(requirements, selectedExperiences)
                .Take(MaxSoftSkills)
                .Select(s => SpanishSoftSkills.TryGetValue(s, out var es) ? es : s)
                .ToList();
            if (softSkills.Any()) parts.Add($"Sólidas habilidades de {FormatSpanishList(softSkills)}");
        }
        else
        {
            parts.Add(keyTechSkills.Any()
                ? $"{candidateTitle} with {yearsOfExperience}+ years of experience, specializing in {FormatSkillsList(keyTechSkills)}"
                : $"{candidateTitle} with {yearsOfExperience}+ years of experience");

            var achievement = ExtractTopAchievement(selectedExperiences);
            if (!string.IsNullOrEmpty(achievement)) parts.Add($"Proven track record of {achievement}");

            var softSkills = ExtractSoftSkills(requirements, selectedExperiences);
            if (softSkills.Any()) parts.Add($"Strong {string.Join(" and ", softSkills.Take(MaxSoftSkills))} skills");
        }

        var summary = string.Join(". ", parts) + ".";
        if (summary.Length > MaxSummaryLength)
            summary = summary.Substring(0, MaxSummaryLength - TruncationSuffix.Length) + TruncationSuffix;
        return summary;
    }

    #endregion

    #region Private Methods

    private string DeriveRoleFromCV(List<Experience> allExperiences, JobRequirements requirements)
    {
        var mostRecentTech = allExperiences
            .Where(e => IsTechRole(e.Role))
            .OrderByDescending(e => DateParser.RecencyKey(e.StartDate, e.EndDate))
            .FirstOrDefault();

        var candidateSeniority = ExtractSeniorityFromRole(mostRecentTech?.Role ?? "");
        var roleType = DetectRoleTypeFromJD(requirements);

        return string.IsNullOrEmpty(candidateSeniority) ? roleType : $"{candidateSeniority} {roleType}";
    }

    private string ExtractSeniorityFromRole(string role)
    {
        foreach (var kvp in SeniorityPrefixes)
            if (role.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        return "";
    }

    private string DetectRoleTypeFromJD(JobRequirements requirements)
    {
        var allSignals = requirements.TechnicalSkills
            .Concat(requirements.Keywords)
            .Select(s => s.ToLower()).ToList();

        var scores = new Dictionary<string, int>();
        foreach (var kvp in RoleTypeSignals)
        {
            // Whole terms (F29): as a substring, the frontend signal "ui" matched keywords such as "build" and "guide",
            // so a backend post read as frontend
            var score = kvp.Value.Count(signal => allSignals.Any(s => SkillNormalizer.ContainsTerm(s, signal)));
            if (score > 0) scores[kvp.Key] = score;
        }

        foreach (var (role, terms) in RoleNameTerms)
        {
            var mentions = terms.Sum(term => SkillNormalizer.FindTerm(requirements.Text, term).Count());
            if (mentions >= MinRoleNameMentions) scores[role] = scores.GetValueOrDefault(role) + 1;
        }

        if (TitleRole(requirements.Title, scores) is { } titled) return titled;

        // Both sides of the stack, each with a real presence ("React" and "Kafka" alone are not a full stack)
        if (scores.GetValueOrDefault(FrontendRoleType) >= MinRoleSignals && scores.GetValueOrDefault(BackendRoleType) >= MinRoleSignals)
            return FullStackRoleType;

        var ranked = scores.OrderByDescending(kvp => kvp.Value).ToList();
        if (ranked.Count == 0 || ranked[0].Value < MinRoleSignals) return DefaultRoleType;
        if (ranked.Count > 1 && ranked[0].Value - ranked[1].Value < MinRoleLead) return DefaultRoleType;

        return ranked[0].Key;
    }

    // The role the title names, or null. With several in the title ("Mobile / Full Stack") the post's own signals
    // choose, and on a tie the one named first
    private static string? TitleRole(string title, Dictionary<string, int> scores)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;

        var text = JobDescriptionConstants.TitlePunctuation().Replace(title.ToLower(), " ");
        var named = new List<(string Role, int At)>();
        foreach (var kvp in TitleRoleSignals)
        {
            var at = kvp.Value
                .SelectMany(signal => SkillNormalizer.FindTerm(text, signal).Select(m => m.Start))
                .DefaultIfEmpty(-1)
                .Min();
            if (at >= 0) named.Add((kvp.Key, at));
        }

        return named.Count == 0
            ? null
            : named.OrderByDescending(x => scores.GetValueOrDefault(x.Role)).ThenBy(x => x.At).First().Role;
    }

    private int CalculateTechYearsOfExperience(List<Experience> allExperiences)
    {
        var pool = allExperiences.Where(e => IsTechRole(e.Role)).ToList();
        if (!pool.Any()) pool = allExperiences;

        foreach (var exp in pool)
        {
            if (!string.IsNullOrWhiteSpace(exp.StartDate) && !DateParser.TryParse(exp.StartDate, out _, out _))
                _logger.LogWarning("Could not parse startDate '{StartDate}', ignoring entry for years calculation", exp.StartDate);
        }

        return DateParser.YearsOfExperience(pool.Select(e => ((string?)e.StartDate, (string?)e.EndDate)));
    }

    private bool IsTechRole(string? role) =>
        !string.IsNullOrEmpty(role) &&
        SelectionConstants.TechRoleKeywords.Any(k => role.Contains(k, StringComparison.OrdinalIgnoreCase));

    private string ExtractTopAchievement(List<Experience> experiences)
    {
        var best = ""; var bestPct = 0;
        foreach (var exp in experiences)
            foreach (var bullet in exp.Bullets ?? new())
            {
                var m = Achievement().Match(bullet);
                if (m.Success && int.TryParse(m.Groups[2].Value, out int pct) && pct > bestPct)
                { bestPct = pct; best = m.Value.ToLower(); }
            }
        return best;
    }

    private string FormatSkillsList(List<string> skills)
    {
        if (skills.Count == 1) return skills[0];
        if (skills.Count == 2) return $"{skills[0]} and {skills[1]}";
        return $"{string.Join(", ", skills.Take(skills.Count - 1))}, and {skills.Last()}";
    }

    /// <summary>"A, B y C": Spanish lists have no comma before the last item.</summary>
    private static string FormatSpanishList(List<string> items)
    {
        if (items.Count == 1) return items[0];
        var last = items[^1];
        var conjunction = LanguagePatterns.StartsWithISound().IsMatch(last) ? "e" : "y";
        return $"{string.Join(", ", items.Take(items.Count - 1))} {conjunction} {last}";
    }

    private List<string> ExtractSoftSkills(JobRequirements requirements, List<Experience> experiences)
    {
        var result = new List<string>();
        foreach (var kvp in SoftSkillStems)
        {
            if (requirements.Keywords.Any(k => k.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) ||
                experiences.Any(e => e.Bullets?.Any(b => b.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase)) == true))
                result.Add(kvp.Value);
            if (result.Count >= MaxSoftSkills) break;
        }
        return result;
    }

    #endregion
}

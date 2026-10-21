using System.Text.RegularExpressions;
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

    private static readonly string[] TechRoleKeywords =
    {
        "developer","engineer","software","programmer","architect",
        "devops","sre","data","backend","frontend","fullstack",
        "full stack","full-stack","cloud","platform","tech"
    };

    private static readonly Dictionary<string, string[]> RoleTypeSignals =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Data Engineer",       new[] { "etl","spark","airflow","dbt","pipeline","warehouse","databricks" } },
            { "ML Engineer",         new[] { "tensorflow","pytorch","machine learning","scikit","mlflow" } },
            { "DevOps Engineer",     new[] { "terraform","ansible","kubernetes","helm","argocd","infrastructure","sre" } },
            { FrontendRoleType,      new[] { "react","vue","angular","css","html","figma","tailwind","ui" } },
            { BackendRoleType,       new[] { "postgresql","mysql","rest api","grpc","kafka","rabbitmq","spring","node.js" } },
            { "Mobile Developer",    new[] { "swift","kotlin","flutter","react native","ios","android","xcode" } },
            { FullStackRoleType,     new[] { "fullstack","full stack","full-stack","end-to-end" } },
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
        List<string> topSkills)
    {
        var yearsOfExperience = CalculateTechYearsOfExperience(cvMaster.Experience);
        var candidateTitle = DeriveRoleFromCV(cvMaster.Experience, requirements);

        var keyTechSkills = topSkills
            .Where(s => requirements.TechnicalSkills.Any(ts => SkillNormalizer.FuzzyMatch(s, ts)))
            .Take(MaxKeyTechSkills).ToList();

        if (!keyTechSkills.Any()) keyTechSkills = topSkills.Take(FallbackTopSkills).ToList();

        var parts = new List<string>();
        parts.Add($"{candidateTitle} with {yearsOfExperience}+ years of experience");
        if (keyTechSkills.Any()) parts.Add($"specializing in {FormatSkillsList(keyTechSkills)}");

        var achievement = ExtractTopAchievement(selectedExperiences);
        if (!string.IsNullOrEmpty(achievement)) parts.Add($"Proven track record of {achievement}");

        var softSkills = ExtractSoftSkills(requirements, selectedExperiences);
        if (softSkills.Any()) parts.Add($"Strong {string.Join(" and ", softSkills.Take(MaxSoftSkills))} skills");

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
            var score = kvp.Value.Count(signal => allSignals.Any(s => s.Contains(signal)));
            if (score > 0) scores[kvp.Key] = score;
        }

        if (!scores.Any()) return DefaultRoleType;

        var hasFrontend = scores.ContainsKey(FrontendRoleType);
        var hasBackend = scores.ContainsKey(BackendRoleType);
        if (hasFrontend && hasBackend) return FullStackRoleType;

        return scores.OrderByDescending(kvp => kvp.Value).First().Key;
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
        TechRoleKeywords.Any(k => role.Contains(k, StringComparison.OrdinalIgnoreCase));

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

using ErCv.Web.Models;

namespace ErCv.Web.Features.Analysis;

/// <summary>
/// The generated CV in the shape the match and gap analyzers read (EDIT-08): they measure the CV as it stands, so
/// they see only the skills and bullets it prints. Nothing is copied that the CV does not hold.
/// </summary>
public static class CvAsProfile
{
    public static CVMaster Of(GeneratedCV cv) => new()
    {
        Header = cv.Header,
        Education = cv.Education,
        Experience = cv.Experience,
        Skills = new Skills { Languages = cv.Skills },
        Projects = cv.Projects
    };
}

/// <summary>What an edit is analysed against: the post, what the parser read from it and the profile the CV came from.</summary>
public sealed record AnalysisContext(string JobDescription, JobRequirements Requirements, CVMaster Profile);

/// <summary>The scores that follow the CV, recomputed after an edit (EDIT-08).</summary>
public sealed record EditedAnalysis(
    MatchScore MatchScore, MissingSkillsAnalysis MissingSkills, KeywordAnalysis KeywordAnalysis,
    ATSScore ATSScore, JdHighlight JdHighlight);

namespace ErCv.Web.Features.Analysis;

public static class MatchScoreConstants
{
    #region Weights

    public const double SkillWeight = 0.5;
    public const double KeywordWeight = 0.3;
    public const double SeniorityWeight = 0.2;
    public const int MaxKeywordEvaluation = 15;
    public const double KeywordBoostMultiplier = 1.5;

    #endregion

    #region Scores

    public const int MaxScore = 100;
    public const int DefaultSeniorityScore = 70;
    public const int DefaultLevel = 3;
    public const int PerfectMatchScore = 100;
    public const int CloseMatchScore = 70;
    public const int PartialMatchScore = 40;
    public const int LowMatchScore = 10;

    // Below this overall score the CV is flagged as a poor fit for the job
    public const int LowMatchThreshold = 40;
    public const string LowMatchWarning =
        "Low match detected: this job description may not align with your profile, so the generated CV may not be competitive.";

    #endregion

    #region Colors

    public const string GreenClass = "green";
    public const string YellowClass = "yellow";
    public const string RedClass = "red";

    public const int GreenThreshold = 80;
    public const int YellowThreshold = 60;

    #endregion

    #region Seniority

    public static readonly Dictionary<string, int> SeniorityMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "intern", 1 },
            { "junior", 2 },
            { "entry", 2 },
            { "entry-level", 2 },
            { "associate", 2 },
            { "mid", 3 },
            { "mid-level", 3 },
            { "senior", 4 },
            { "lead", 5 },
            { "principal", 6 },
            { "staff", 6 },
            { "architect", 6 }
        };

    #endregion
}

using System.Text.RegularExpressions;

namespace ErCv.Web.Features.Analysis;

public static partial class ATSConstants
{
    #region Reference Lists

    public static readonly string[] AtsFriendlyFonts =
    {
        "times new roman",
        "arial",
        "calibri",
        "georgia",
        "helvetica",
        "garamond"
    };
    public static readonly string[] StandardSections =
    {
        "education",
        "experience",
        "skills",
        "projects",
        "summary"
    };

    #endregion

    #region Severities

    public const string SeverityCritical = "Critical";
    public const string SeverityHigh = "High";
    public const string SeverityMedium = "Medium";
    public const string SeverityLow = "Low";

    // Issues are listed most severe first
    public const int SeverityWeightCritical = 4;
    public const int SeverityWeightHigh = 3;
    public const int SeverityWeightMedium = 2;
    public const int SeverityWeightLow = 1;

    #endregion

    #region Scoring

    public const int MaxScore = 100;
    public const int PenaltyInvalidEmail = 5;
    public const int PenaltyInvalidPhone = 5;
    public const int PenaltyLowDensityHigh = 20;
    public const int PenaltyLowDensityMedium = 10;
    public const int PenaltyMissingSections = 10;
    public const int PenaltyShortBullets = 5;
    public const int PenaltyNoMetrics = 5;
    public const int PenaltyFewMetrics = PenaltyNoMetrics / 2;
    public const int PenaltyModernTemplate = 15;
    public const double DensityLowThreshold = 0.4;
    public const double DensityMediumThreshold = 0.6;
    public const double FewMetricsRatio = 0.4;
    public const int ShortBulletLength = 40;
    public const int MinPhoneLength = 8;
    public const int ScoreGreen = 85;
    public const int ScoreYellow = 65;
    public const int MaxRecommendations = 4;

    #endregion

    #region Patterns

    public const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string MetricsRegex = @"\d+[%x]|\d+[MK]\+?|[\d,]{3,}\+?";

    [GeneratedRegex(EmailRegex)]
    public static partial Regex Email();

    /// <summary>A number that reads as a result: "40%", "3x", "2M+", "1,200".</summary>
    [GeneratedRegex(MetricsRegex)]
    public static partial Regex Metrics();

    #endregion

    #region Messages

    public const string EmailInvalidIssue = "Email not detected or invalid format";
    public const string EmailInvalidFix = "Use the format: name@domain.com";

    public const string PhoneInvalidIssue = "Phone format may not be parsed correctly by some ATS systems";
    public const string PhoneInvalidFix = "Use format: +1 (234) 567-8900 or +54 9 341 279-3916";

    // {0} is a whole percent, formatted by the caller: "P0" would print "0 %" where the culture is invariant (Linux)
    public const string LowDensityIssue = "Low keyword density ({0}%): ATS may reject the CV";
    public const string LowDensityFix = "Include more keywords from the job description in bullets and skills section";

    public const string MediumDensityIssue = "Medium keyword density ({0}%): could be improved";
    public const string MediumDensityFix = "Add 2–3 more keywords from the job description to improve ATS ranking";

    public const string MissingSectionsIssue = "Missing required sections (Education / Experience / Skills)";
    public const string MissingSectionsFix = "Verify that cv-master.json contains data in all required sections";

    public const string ShortBulletIssue = "Some bullet points are too short";
    public const string ShortBulletCountIssue = "{0} bullet point(s) are too short (<{1} chars)";
    public const string ShortBulletFix = "Expand bullet points with context, tech used, and impact";

    public const string NoMetricsIssue = "No quantifiable achievements detected (numbers, percentages)";
    public const string NoMetricsFix = "Add metrics to bullets: 'Reduced X by 30%', 'Handled N requests/day'";

    public const string FewMetricsIssue = "Only {0}/{1} bullets have quantifiable results";
    public const string FewMetricsFix = "Add more numbers and percentages to your bullets";

    public const string ModernTemplateIssue =
        "Modern (two-column) template detected: major ATS systems (Workday, Taleo, Greenhouse) " +
        "linearize columns and may scramble your content";
    public const string ModernTemplateFix =
        "Use Harvard or Minimalist template when applying through ATS portals. " +
        "Modern is safe for direct email or PDF submissions only.";

    public const string OptimizedMessage = "Your CV is fully optimized for ATS";
    public const string RecommendationsHeader = "{0} improvement point(s) detected:";
    public const string RecommendationLine = "• {0} → {1}";

    #endregion
}

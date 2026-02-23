namespace ErCvApi.Constants
{
    public static class ATSConstants
    {
        // Fonts
        public static readonly string[] AtsFriendlyFonts =
        {
            "times new roman",
            "arial",
            "calibri",
            "georgia",
            "helvetica",
            "garamond"
        };

        // Sections
        public static readonly string[] StandardSections =
        {
            "education",
            "experience",
            "skills",
            "projects",
            "summary"
        };

        // Severities
        public const string SeverityCritical = "Critical";
        public const string SeverityHigh = "High";
        public const string SeverityMedium = "Medium";
        public const string SeverityLow = "Low";

        // Score penalties
        public const int PenaltyInvalidEmail = 5;
        public const int PenaltyInvalidPhone = 5;
        public const int PenaltyLowDensityHigh = 20;
        public const int PenaltyLowDensityMedium = 10;
        public const int PenaltyMissingSections = 10;
        public const int PenaltyShortBullets = 5;
        public const int PenaltyNoMetrics = 5;
        public const int PenaltyModernTemplate = 5;

        // Thresholds
        public const double DensityLowThreshold = 0.4;
        public const double DensityMediumThreshold = 0.6;
        public const int ShortBulletLength = 30;

        public const int ScoreGreen = 85;
        public const int ScoreYellow = 65;

        // Regex
        public const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const string MetricsRegex = @"\d+[%+]|\d+[MK]\+?|[\d,]+\+?";

        // Messages
        public const string EmailInvalidIssue = "Email not detected or invalid format";
        public const string EmailInvalidFix = "Use the format: name@domain.com";

        public const string PhoneInvalidIssue = "Phone format may not be parsed correctly by some ATS systems";
        public const string PhoneInvalidFix = "Use format: +1 (234) 567-8900 or +54 11 1234-5678";

        public const string MissingSectionsIssue = "Missing required sections (Education / Experience / Skills)";
        public const string MissingSectionsFix = "Verify that cv-master.json contains data in all required sections";

        public const string ShortBulletIssue = "Some bullet points are too short (<30 characters)";
        public const string ShortBulletFix = "Expand bullet points with more detail and context";

        public const string NoMetricsIssue = "No quantifiable achievements detected (numbers, percentages)";
        public const string NoMetricsFix = "Add metrics to your bullets: 'Reduced X by 30%', 'Managed N users'";

        public const string ModernTemplateIssue = "Modern template uses two columns — some basic ATS systems may parse it incorrectly";
        public const string ModernTemplateFix = "For strict ATS systems, use the Harvard (single-column) template";

        public const string OptimizedMessage = "Your CV is fully optimized for ATS";
    }
}
namespace ErCv.Web.Features.Documents;

public static class DocumentConstants
{
    #region Section Order

    // Keys of GeneratedCV.SectionOrder; the editor, the templates' Sections loop and the DOCX all use them
    public const string SectionSummaryKey = "summary";
    public const string SectionEducationKey = "education";
    public const string SectionExperienceKey = "experience";
    public const string SectionSkillsKey = "skills";
    public const string SectionProjectsKey = "projects";

    // The order each template prints when the user has not chosen one. The DOCX and the text export use Harvard's.
    // Modern has two columns (skills and education on the left), so a section only moves within its column.
    public static readonly IReadOnlyList<string> HarvardSectionOrder =
        new[] { SectionSummaryKey, SectionEducationKey, SectionExperienceKey, SectionSkillsKey, SectionProjectsKey };

    public static readonly IReadOnlyList<string> ModernSectionOrder =
        new[] { SectionSkillsKey, SectionEducationKey, SectionSummaryKey, SectionExperienceKey, SectionProjectsKey };

    public static readonly IReadOnlyList<string> MinimalistSectionOrder =
        new[] { SectionSummaryKey, SectionExperienceKey, SectionEducationKey, SectionSkillsKey, SectionProjectsKey };

    // Compact puts the skills right after the experience, where a dense one-page reader looks for them
    public static readonly IReadOnlyList<string> CompactSectionOrder =
        new[] { SectionSummaryKey, SectionExperienceKey, SectionSkillsKey, SectionEducationKey, SectionProjectsKey };

    public static readonly IReadOnlyList<string> ExecutiveSectionOrder =
        new[] { SectionSummaryKey, SectionExperienceKey, SectionEducationKey, SectionSkillsKey, SectionProjectsKey };

    #endregion

    #region Style Classes

    // Prefixes of the classes in Templates/partials/base-styles.html; the key comes from ApiConstants.Accents / FontPairings
    public const string AccentClassPrefix = "accent-";
    public const string FontsClassPrefix = "fonts-";

    #endregion

    #region DOCX Layout

    public const string FontSizeName = "32";       // 16pt
    public const string FontSizeSection = "22";    // 11pt
    public const string FontSizeContact = "18";    // 9pt
    public const string BulletSymbol = "•";
    public const string IndentationLeft = "360";
    public const int DocxMarginVerticalTwips = 720;      // 0.5in
    public const int DocxMarginHorizontalTwips = 1080;   // 0.75in
    public const uint SectionBorderSize = 6;             // eighths of a point
    public const string DateGap = "     ";
    public const string DetailSeparator = " | ";
    public const string DateRangeSeparator = " – ";

    #endregion

    #region PDF Layout

    public const string PdfMarginTop = "0.5in";
    public const string PdfMarginBottom = "0.5in";
    public const string PdfMarginLeft = "0.75in";
    public const string PdfMarginRight = "0.75in";

    #endregion

    #region Chromium

    public const string ExecutablePathVariable = "PUPPETEER_EXECUTABLE_PATH";
    public const string ChromiumCacheAppFolder = "er-cv-generator";
    public const string ChromiumCacheFolder = "chromium";

    // PDF engine states the health check reports (ENG-05)
    public const string PdfEngineReady = "ready";
    public const string PdfEngineStarting = "starting";
    public const string PdfEngineUnavailable = "unavailable";

    // Each PDF holds a Chrome page open. The UI calls the service directly (not through the rate-limited API),
    // so this cap is what keeps a burst of generations from exhausting the instance.
    public const int MaxConcurrentPdfRenders = 2;

    public static readonly string[] ChromiumArgs =
    {
        "--no-sandbox",
        "--disable-setuid-sandbox",
        "--disable-dev-shm-usage"
    };

    #endregion

    #region Skill Categories

    public static readonly string[] LanguageKeywords =
    {
        "C#", "JavaScript", "TypeScript", "Python", "Java", "SQL",
        "HTML", "CSS", "PHP", "Ruby", "Go", "Rust",
        "Kotlin", "Swift", "C++", "C", "Scala", "Dart", "Elixir",
        "R", "Perl", "Lua", "Groovy", "Shell", "Bash", "PowerShell",
        "PL/SQL", "T-SQL"
    };

    public static readonly string[] SoftSkillKeywords =
    {
        "Agile", "Scrum", "Kanban", "Leadership", "Communication",
        "Team Collaboration", "Cross-functional", "Problem Solving",
        "Time Management", "Adaptability", "Critical Thinking",
        "Training", "Mentoring", "Remote Collaboration",
        "Stakeholder Management", "Decision Making", "Ownership",
        "Pair Programming", "Code Review"
    };

    #endregion
}

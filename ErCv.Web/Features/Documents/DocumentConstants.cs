namespace ErCv.Web.Features.Documents;

public static class DocumentConstants
{
    #region Section Titles

    public const string SectionSummary = "PROFESSIONAL SUMMARY";
    public const string SectionEducation = "EDUCATION";
    public const string SectionExperience = "EXPERIENCE";
    public const string SectionSkills = "SKILLS";
    public const string SectionProjects = "PROJECTS";

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
    public const string TechnologiesLabel = "Technologies: ";
    public const string GpaLabel = "GPA: ";
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

    public const string CategoryTechnical = "Technical";
    public const string CategoryLanguages = "Languages";
    public const string CategorySoftSkills = "Soft Skills";

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

namespace ErCvApi.Constants
{
    public static class DocumentConstants
    {
        // Sections
        public const string SectionSummary = "PROFESSIONAL SUMMARY";
        public const string SectionEducation = "EDUCATION";
        public const string SectionExperience = "EXPERIENCE";
        public const string SectionSkills = "SKILLS";
        public const string SectionProjects = "PROJECTS";

        // Font sizes (half-points in OpenXML)
        public const string FontSizeName = "32";       // 16pt
        public const string FontSizeSection = "22";    // 11pt
        public const string FontSizeContact = "18";    // 9pt

        // Layout
        public const string BulletSymbol = "•";
        public const string IndentationLeft = "360";

        // PDF margins
        public const string PdfMarginTop = "0.5in";
        public const string PdfMarginBottom = "0.5in";
        public const string PdfMarginLeft = "0.75in";
        public const string PdfMarginRight = "0.75in";

        // Skill Categories
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

        // Chromium launch args
        public static readonly string[] ChromiumArgs =
        {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage"
        };
    }
}

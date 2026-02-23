namespace ErCvApi.Constants
{
    public static class DocumentConstants
    {
        // Sections
        public const string SectionEducation = "EDUCATION";
        public const string SectionExperience = "EXPERIENCE";
        public const string SectionSkills = "SKILLS";
        public const string SectionProjects = "PROJECTS";

        // Fonts
        public const string FontSizeName = "32";
        public const string FontSizeSection = "24";
        public const string FontSizeContact = "20";

        // Layout
        public const string BulletSymbol = "•";
        public const string IndentationLeft = "360";

        // PDF
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
            "Kotlin", "Swift", "C++", "C"
        };

        public static readonly string[] SoftSkillKeywords =
        {
            "Agile","Scrum","Leadership","Communication","Team Collaboration",
            "Problem Solving","Time Management","Adaptability","Critical Thinking",
            "Training","Mentoring","Remote Collaboration"
        };

        // Chromium
        public static readonly string[] ChromiumArgs =
        {
            "--no-sandbox",
            "--disable-setuid-sandbox"
        };
    }
}
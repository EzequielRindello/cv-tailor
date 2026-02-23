namespace ErCvApi.Constants
{
    public static class JobDescriptionConstants
    {
        // Default Values
        public const string DefaultSeniority = "mid-level";

        // Regex Patterns
        public const string YearsOfExperiencePattern = @"(\d+)\+?\s*(?:years?|yrs?)";
        public const string YearsWithTechPattern = @"(\d+)\+?\s*(?:years?|yrs?)\s*(?:of\s*)?(?:experience\s*)?(?:with\s*)?([a-z0-9\.\#\+\-]+)";
        public const string WordSplitPattern = @"\W+";
        public const string BulletLinePattern = @"^[\-\*\•\d\.]+\s*";

        // Requirement Keywords
        public const string ExperienceKeyword = "experience";
        public const string RequiredKeyword = "required";
        public const string MustHaveKeyword = "must have";

        // Seniority Keywords
        public static readonly string[] SeniorityKeywords =
        {
            "senior", "lead", "principal", "staff", "architect",
            "junior", "mid-level", "entry-level", "intern"
        };

        // Common Tech Keywords
        public static readonly string[] CommonTechKeywords =
        {
            "c#", "csharp", ".net", "asp.net", "dotnet", "react", "angular", "vue",
            "javascript", "typescript", "python", "java", "node.js", "sql", "nosql",
            "aws", "azure", "gcp", "docker", "kubernetes", "microservices",
            "api", "rest", "graphql", "mongodb", "postgresql", "mysql", "redis",
            "ci/cd", "devops", "agile", "scrum", "git", "jenkins", "terraform"
        };

        // Common Words (Stopwords)
        public static readonly string[] CommonWords =
        {
            "the", "and", "for", "with", "you", "that", "this", "will", "from",
            "have", "has", "are", "our", "your", "their", "been", "more", "than",
            "such", "about", "into", "through", "should", "would", "could", "must"
        };
    }
}
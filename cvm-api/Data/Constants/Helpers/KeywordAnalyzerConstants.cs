namespace ErCvApi.Constants
{
    public static class KeywordAnalyzerConstants
    {
        // Regex
        public const string WordSplitPattern = @"\W+";

        // Limits
        public const int MaxTechnicalKeywords = 10;
        public const int MaxSoftSkillKeywords = 8;
        public const int MaxIndustryKeywords = 8;
        public const int MaxPresentKeywords = 15;
        public const int MaxMissingKeywords = 10;

        // Suggestion Limits
        public const int MaxMissingTechSuggestions = 3;
        public const int MaxMissingSoftSuggestions = 2;

        // Suggestion Templates
        public const string AddTechSuggestionTemplate =
            "Add \"{0}\" to the skills section or bullets if you have used it";

        public const string AddSoftSuggestionTemplate =
            "Mention \"{0}\" in a bullet point to reflect this skill";

        // Soft Skills
        public static readonly string[] SoftSkillWords =
        {
            "leadership", "communication", "teamwork", "collaboration", "mentoring",
            "agile", "scrum", "problem-solving", "adaptability", "initiative",
            "ownership", "proactive", "analytical", "organized", "detail"
        };

        // Technical Words
        public static readonly string[] TechWords =
        {
            "react", "angular", "vue", "node", "python", "java", "c#", "dotnet", "net",
            "sql", "nosql", "mongodb", "postgresql", "mysql", "redis", "docker",
            "kubernetes", "aws", "azure", "gcp", "git", "ci/cd", "devops", "api",
            "rest", "graphql", "microservices", "typescript", "javascript"
        };

        // Stop Words
        public static readonly string[] StopWords =
        {
            "the", "and", "for", "with", "you", "that", "this", "will", "from",
            "have", "has", "are", "our", "your", "their", "been", "more", "than",
            "such", "about", "into", "through", "should", "would", "could", "must",
            "what", "when", "where", "which", "who", "how", "they", "them", "then",
            "also", "both", "each", "few", "most", "other", "some"
        };
    }
}
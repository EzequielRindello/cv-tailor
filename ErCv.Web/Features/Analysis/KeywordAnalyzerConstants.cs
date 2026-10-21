namespace ErCv.Web.Features.Analysis;

public static class KeywordAnalyzerConstants
{
    #region Limits

    // Shorter words are never counted ("and", "the"); "industry" keywords must be longer still
    public const int MinWordLength = 4;
    public const int MinIndustryWordLength = 5;
    public const int MaxTechnicalKeywords = 15;
    public const int MaxSoftSkillKeywords = 10;
    public const int MaxIndustryKeywords = 10;
    public const int MaxPresentKeywords = 20;
    public const int MaxMissingKeywords = 15;
    public const int MaxMissingTechSuggestions = 5;
    public const int MaxMissingSoftSuggestions = 3;
    public const int DensityDecimals = 2;

    #endregion

    #region Messages

    public const string AddTechSuggestionTemplate =
        "Add \"{0}\" to the skills section or bullets if you have used it";

    public const string AddSoftSuggestionTemplate =
        "Mention \"{0}\" in a bullet point to reflect this skill";

    #endregion

    #region Keyword Lists

    public static readonly string[] SoftSkillWords =
    {
        "leadership", "communication", "teamwork", "collaboration",
        "mentoring", "coaching", "stakeholder", "cross-functional",
        "interpersonal", "presentation", "negotiation", "facilitation",
        "partnership", "relationship", "alignment",
        "agile", "scrum", "kanban", "sprint", "retrospective",
        "problem-solving", "analytical", "critical thinking",
        "decision-making", "prioritization", "planning", "estimation",
        "ownership", "accountability", "initiative", "proactive",
        "adaptability", "flexibility", "organized", "detail-oriented",
        "self-motivated", "independent", "curious", "fast learner",
        "resilient", "reliable",
        "remote", "async", "distributed", "time zones", "written communication",
    };
    public static readonly string[] TechWords =
    {
        "react", "angular", "vue", "svelte", "next", "nuxt", "gatsby",
        "node", "express", "nestjs", "fastapi", "django", "flask", "rails",
        "spring", "laravel", "gin", "fiber",
        "python", "java", "javascript", "typescript", "csharp", "c#",
        "dotnet", "net", "go", "golang", "rust", "ruby", "php", "scala",
        "kotlin", "swift", "dart", "elixir",
        "sql", "nosql", "mongodb", "postgresql", "mysql", "redis",
        "elasticsearch", "dynamodb", "cassandra", "kafka", "rabbitmq",
        "snowflake", "databricks", "bigquery", "redshift",
        "aws", "azure", "gcp", "docker", "kubernetes", "k8s", "helm",
        "terraform", "ansible", "jenkins", "ci/cd", "cicd", "devops",
        "linux", "nginx", "serverless",
        "jest", "pytest", "cypress", "playwright", "selenium", "xunit",
        "tdd", "bdd",
        "api", "rest", "graphql", "grpc", "microservices", "event-driven",
        "mvc", "cqrs", "ddd", "clean architecture",
        "machine learning", "tensorflow", "pytorch", "llm", "openai",
        "git", "github", "gitlab", "postman", "swagger", "openapi",
        "vite", "webpack", "prisma", "typeorm",
    };
    public static readonly string[] StopWords =
    {
        "the", "and", "for", "with", "you", "that", "this", "will", "from",
        "have", "has", "are", "our", "your", "their", "been", "more", "than",
        "such", "about", "into", "through", "should", "would", "could", "must",
        "what", "when", "where", "which", "who", "how", "they", "them", "then",
        "also", "both", "each", "few", "most", "other", "some", "very", "just",
        "like", "well", "good", "great", "new", "high", "best", "able", "need",
        "make", "work", "role", "team", "build", "using", "help", "join", "want",
        "look", "plus", "take", "provide", "ensure", "maintain", "develop",
        "candidate", "position", "company", "opportunity", "environment",
    };

    #endregion
}

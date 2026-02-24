namespace ErCvApi.Constants
{
    public static class MissingSkillsConstants
    {
        // Limits
        public const int MaxRequiredMissing = 8;
        public const int MaxNiceToHaveMissing = 6;
        public const int MaxRequiredSuggestions = 5;
        public const int MaxNiceToHaveSuggestions = 3;

        // Score rules
        public const int SkillWeightImpact = 50;   // Matches MatchScore skill weight (50%)
        public const int MaxPotentialIncrease = 40;

        // Suggestion templates
        public const string RequiredSuggestionTemplate =
            "Add '{0}' to cv-master.json if you have experience with this technology";

        public const string NiceToHaveSuggestionTemplate =
            "Mention '{0}' in relevant project bullet points if you have used it";

        public const string ExtraRequiredSuggestionTemplate =
            "There are {0} more required skills — review the full JSON";

        // Nice-to-have keywords — advanced/ops tools typically listed as optional
        public static readonly string[] NiceToHaveKeywords =
        {
            // Advanced infra
            "kubernetes", "k8s", "helm", "terraform", "ansible", "pulumi",
            "argocd", "flux", "spinnaker", "nomad", "openshift",

            // Advanced data
            "kafka", "rabbitmq", "elasticsearch", "opensearch", "redis",
            "cassandra", "dynamodb", "neo4j", "clickhouse", "snowflake",
            "databricks", "spark", "flink", "dbt", "airflow",

            // Advanced patterns
            "graphql", "grpc", "event sourcing", "cqrs", "ddd",
            "hexagonal architecture", "clean architecture",

            // Observability
            "prometheus", "grafana", "datadog", "newrelic", "sentry",
            "opentelemetry", "jaeger", "splunk",

            // AI/ML (unless it's a ML role)
            "tensorflow", "pytorch", "scikit-learn", "machine learning",
            "llm", "langchain", "vertex ai", "sagemaker",

            // Advanced security
            "vault", "hashicorp vault", "zero trust", "sso", "saml", "oidc",

            // Misc advanced tools
            "istio", "envoy", "consul", "linkerd",
        };
    }
}

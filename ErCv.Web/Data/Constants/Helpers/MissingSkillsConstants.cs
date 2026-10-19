namespace ErCvApi.Constants
{
    public static class MissingSkillsConstants
    {
        public const int MaxRequiredMissing = 8;
        public const int MaxNiceToHaveMissing = 6;
        public const int MaxRequiredSuggestions = 5;
        public const int MaxNiceToHaveSuggestions = 3;
        public const int SkillWeightImpact = 50;   // Matches MatchScore skill weight (50%)
        public const int MaxPotentialIncrease = 40;
        public const string RequiredSuggestionTemplate =
            "Add '{0}' to cv-master.json if you have experience with this technology";

        public const string NiceToHaveSuggestionTemplate =
            "Mention '{0}' in relevant project bullet points if you have used it";

        public const string ExtraRequiredSuggestionTemplate =
            "There are {0} more required skills — review the full JSON";
        public static readonly string[] NiceToHaveKeywords =
        {
            "kubernetes", "k8s", "helm", "terraform", "ansible", "pulumi",
            "argocd", "flux", "spinnaker", "nomad", "openshift",
            "kafka", "rabbitmq", "elasticsearch", "opensearch", "redis",
            "cassandra", "dynamodb", "neo4j", "clickhouse", "snowflake",
            "databricks", "spark", "flink", "dbt", "airflow",
            "graphql", "grpc", "event sourcing", "cqrs", "ddd",
            "hexagonal architecture", "clean architecture",
            "prometheus", "grafana", "datadog", "newrelic", "sentry",
            "opentelemetry", "jaeger", "splunk",
            "tensorflow", "pytorch", "scikit-learn", "machine learning",
            "llm", "langchain", "vertex ai", "sagemaker",
            "vault", "hashicorp vault", "zero trust", "sso", "saml", "oidc",
            "istio", "envoy", "consul", "linkerd",
        };
    }
}

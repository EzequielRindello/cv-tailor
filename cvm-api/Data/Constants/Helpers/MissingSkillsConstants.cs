namespace ErCvApi.Constants
{
    public static class MissingSkillsConstants
    {
        // Limits
        public const int MaxRequiredMissing = 7;
        public const int MaxNiceToHaveMissing = 5;

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

        // Nice-to-have keywords
        public static readonly string[] NiceToHaveKeywords =
        {
            "kubernetes", "terraform", "graphql", "redis", "kafka",
            "elasticsearch", "jenkins", "ansible", "prometheus",
            "grafana", "rabbitmq"
        };
    }
}
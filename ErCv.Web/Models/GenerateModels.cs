namespace ErCvApi.Models
{
    public class GeneratedCV
    {
        public Header Header { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public List<Education> Education { get; set; } = new();
        public List<Experience> Experience { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public List<Project> Projects { get; set; } = new();
    }

    public class JobRequirements
    {
        public List<string> TechnicalSkills { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public string SeniorityLevel { get; set; } = string.Empty;
        public List<string> RequiredExperience { get; set; } = new();
    }

    public class RelevanceScore
    {
        public Experience Experience { get; set; } = new();
        public double Score { get; set; }
        public List<string> MatchedSkills { get; set; } = new();
    }
}
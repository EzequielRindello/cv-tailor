namespace ErCvApi.Models
{
    public class GeneratedCV
    {
        public Header Header { get; set; }
        public string Summary { get; set; }
        public List<Education> Education { get; set; }
        public List<Experience> Experience { get; set; }
        public List<string> Skills { get; set; }
        public List<Project> Projects { get; set; }
    }

    public class JobRequirements
    {
        public List<string> TechnicalSkills { get; set; }
        public List<string> Keywords { get; set; }
        public string SeniorityLevel { get; set; }
        public List<string> RequiredExperience { get; set; }
    }

    public class RelevanceScore
    {
        public Experience Experience { get; set; }
        public double Score { get; set; }
        public List<string> MatchedSkills { get; set; }
    }
}
namespace ErCvApi.Models
{
    public class MatchScore
    {
        public int Overall { get; set; }
        public int SkillsMatched { get; set; }
        public int TotalSkills { get; set; }
        public int KeywordsMatched { get; set; }
        public int TotalKeywords { get; set; }
        public bool SeniorityMatch { get; set; }
        public string ColorClass { get; set; } = string.Empty;
        public string? LowMatchWarning { get; set; }
    }

    public class MissingSkillsAnalysis
    {
        public List<string> RequiredMissing { get; set; } = new();
        public List<string> NiceToHaveMissing { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public int PotentialScoreIncrease { get; set; }
    }

    public class KeywordAnalysis
    {
        public List<KeywordItem> TechnicalKeywords { get; set; } = new();
        public List<KeywordItem> SoftSkillKeywords { get; set; } = new();
        public List<KeywordItem> IndustryKeywords { get; set; } = new();
        public List<string> PresentInCV { get; set; } = new();
        public List<string> MissingFromCV { get; set; } = new();
        public double KeywordDensity { get; set; }
        public List<string> Suggestions { get; set; } = new();
    }

    public class KeywordItem
    {
        public string Word { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public bool PresentInCV { get; set; }
    }

    public class ATSScore
    {
        public int Overall { get; set; }
        public List<ATSIssue> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string ColorClass { get; set; } = string.Empty;
    }

    public class ATSIssue
    {
        public string Severity { get; set; } = string.Empty;
        public string Issue { get; set; } = string.Empty;
        public string Fix { get; set; } = string.Empty;
    }

    public class RegenerateRequest
    {
        public GeneratedCV? GeneratedCV { get; set; }
        public Dictionary<string, List<int>> BulletSelections { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public CVTemplate Template { get; set; } = CVTemplate.Harvard;
        public bool AllowTwoPages { get; set; } = false;
    }

    /// <summary>The phases of generating a CV, reported in this order (UX-06).</summary>
    public enum GenerationStage
    {
        Parsing,
        Scoring,
        Selecting,
        Rendering
    }

    public enum CVTemplate
    {
        Harvard,
        Modern,
        Minimalist
    }

    public class GenerateRequest
    {
        public string JobDescription { get; set; } = string.Empty;
        public List<string> Formats { get; set; } = new();
        public CVTemplate Template { get; set; } = CVTemplate.Harvard;
        public bool AllowTwoPages { get; set; } = false;
        public string ProfileId { get; set; } = "cv-master";
    }

    public class AnalysisResult
    {
        public MatchScore MatchScore { get; set; } = new();
        public MissingSkillsAnalysis MissingSkills { get; set; } = new();
        public KeywordAnalysis KeywordAnalysis { get; set; } = new();
        public ATSScore ATSScore { get; set; } = new();
        public GeneratedCV GeneratedCV { get; set; } = new();
        public string? PreviewHtml { get; set; }
        public SelectionExplanation Explanation { get; set; } = new();
    }
}

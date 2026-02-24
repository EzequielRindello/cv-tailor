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
        public string ColorClass { get; set; }

        /// <summary>
        /// Non-null when Overall < 30 — warns the user that this JD is a poor fit.
        /// </summary>
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
        public string Word { get; set; }
        public int Frequency { get; set; }
        public bool PresentInCV { get; set; }
    }

    public class ATSScore
    {
        public int Overall { get; set; }
        public List<ATSIssue> Issues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string ColorClass { get; set; }
    }

    public class ATSIssue
    {
        public string Severity { get; set; }
        public string Issue { get; set; }
        public string Fix { get; set; }
    }

    public class RegenerateRequest
    {
        public GeneratedCV GeneratedCV { get; set; }
        public Dictionary<string, List<int>> BulletSelections { get; set; } = new();
        public List<string> Formats { get; set; } = new();
        public CVTemplate Template { get; set; } = CVTemplate.Harvard;
        public bool AllowTwoPages { get; set; } = false;
    }

    public enum CVTemplate
    {
        Harvard,
        Modern,
        Minimalist
    }

    public class GenerateRequest
    {
        public string JobDescription { get; set; }
        public List<string> Formats { get; set; }
        public CVTemplate Template { get; set; } = CVTemplate.Harvard;
        public bool AllowTwoPages { get; set; } = false;

        /// <summary>
        /// Profile to use from Data/. Defaults to "cv-master".
        /// Enables FEATURE-202 multi-profile support.
        /// </summary>
        public string ProfileId { get; set; } = "cv-master";
    }

    public class AnalysisResult
    {
        public MatchScore MatchScore { get; set; }
        public MissingSkillsAnalysis MissingSkills { get; set; }
        public KeywordAnalysis KeywordAnalysis { get; set; }
        public ATSScore ATSScore { get; set; }
        public GeneratedCV GeneratedCV { get; set; }

        /// <summary>
        /// Rendered HTML of the CV template — used for inline preview (FEATURE-102).
        /// </summary>
        public string? PreviewHtml { get; set; }
    }
}

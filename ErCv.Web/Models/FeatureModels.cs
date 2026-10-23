using ErCv.Web.Constants;

namespace ErCv.Web.Models;

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

    /// <summary>Missing from this CV but in the profile (EDIT-08): left out, so the visitor can add them back.</summary>
    public List<string> InProfileNotInCv { get; set; } = new();
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

/// <summary>The phases of generating a CV, reported in this order (UX-06). The scores are computed on the finished CV.</summary>
public enum GenerationStage
{
    Parsing,
    Selecting,
    Rendering,
    Scoring
}

/// <summary>Where a generation is (UX-11): its phase, how far along it is (0 to 100) and, while it renders, what it is doing.</summary>
public record GenerationStatus(GenerationStage Stage, int Percent, string? Detail = null);

public enum CVTemplate
{
    Harvard,
    Modern,
    Minimalist,
    Compact,
    Executive
}

public class GenerateRequest
{
    public string JobDescription { get; set; } = string.Empty;
    public List<string> Formats { get; set; } = new();
    public CVTemplate Template { get; set; } = CVTemplate.Harvard;
    public bool AllowTwoPages { get; set; } = false;
    public string ProfileId { get; set; } = ApiConstants.DefaultProfileId;
    public string Language { get; set; } = ApiConstants.LanguageEnglish;
    public string Accent { get; set; } = ApiConstants.StyleDefault;
    public string Fonts { get; set; } = ApiConstants.StyleDefault;

    /// <summary>EDIT-03: also write a cover letter from the same job post and profile.</summary>
    public bool IncludeCoverLetter { get; set; }

    /// <summary>EDIT-09: the company the cover letter is for. Empty: the one the post names, else none.</summary>
    public string? Company { get; set; }
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
    public JdHighlight JdHighlight { get; set; } = new();
    public CoverLetter? CoverLetter { get; set; }
    public string? CoverLetterHtml { get; set; }

    /// <summary>The company the cover letter names (the visitor's, else the post's), or null.</summary>
    public string? Company { get; set; }

    /// <summary>What the scores are recomputed against after an edit (EDIT-08); never part of an API response.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public ErCv.Web.Features.Analysis.AnalysisContext? Context { get; set; }
}

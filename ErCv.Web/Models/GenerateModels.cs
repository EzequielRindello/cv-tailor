namespace ErCv.Web.Models;

public class GeneratedCV
{
    public Header Header { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
    public List<Education> Education { get; set; } = new();
    public List<Experience> Experience { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public List<Project> Projects { get; set; } = new();

    /// <summary>Section keys (<c>DocumentConstants.Section*Key</c>) in the order the user chose; empty means the template's own order.</summary>
    public List<string> SectionOrder { get; set; } = new();

    /// <summary>Bullets the user rewrote in the editor (EDIT-01). Every other bullet is the profile's text.</summary>
    public List<BulletEdit> EditedBullets { get; set; } = new();
}

/// <summary>A bullet the user rewrote: the profile's text and what it says now.</summary>
public record BulletEdit(string Original, string Edited);

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
    public List<ScorePart> Parts { get; set; } = new();
}
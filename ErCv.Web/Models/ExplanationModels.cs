namespace ErCv.Web.Models;

/// <summary>One named contribution to a score. The parts of an explanation add up to its score (MATCH-04).</summary>
public class ScorePart
{
    public string Label { get; set; } = string.Empty;
    public double Points { get; set; }
    public string? Detail { get; set; }
}

public class BulletExplanation
{
    public string Text { get; set; } = string.Empty;
    public double Score { get; set; }
    public List<ScorePart> Parts { get; set; } = new();

    /// <summary>False when the bullet scored too low, or was trimmed to fit the page.</summary>
    public bool Selected { get; set; }
}

public class ExperienceExplanation
{
    public string Role { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
    public double Score { get; set; }
    public List<ScorePart> Parts { get; set; } = new();
    public List<BulletExplanation> Bullets { get; set; } = new();

    public static string KeyOf(Experience e) => $"{e.Role}|{e.Company}|{e.StartDate}|{e.EndDate}";

    public string Key => $"{Role}|{Company}|{StartDate}|{EndDate}";
}

/// <summary>Why each experience and bullet of a generated CV was chosen, in the order they appear in the CV.</summary>
public class SelectionExplanation
{
    public List<ExperienceExplanation> Experiences { get; set; } = new();
}

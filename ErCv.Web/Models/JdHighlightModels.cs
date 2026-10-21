namespace ErCv.Web.Models;

/// <summary>A stretch of the job description. <see cref="Skill"/> is set when the text is a skill the CV can be checked against.</summary>
public class JdSegment
{
    public string Text { get; set; } = string.Empty;
    public string? Skill { get; set; }
}

/// <summary>One place in the generated CV where a skill shows up, e.g. Section "Experience", Label "Backend Engineer, Acme".</summary>
public class JdSkillLocation
{
    public string Section { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
}

public class JdSkillInfo
{
    public string Skill { get; set; } = string.Empty;

    /// <summary>True when the profile has the skill (listed, or mentioned in a bullet).</summary>
    public bool InProfile { get; set; }

    public List<JdSkillLocation> Locations { get; set; } = new();
}

/// <summary>
/// The job description cut into segments, plus what is known about each skill found in it (MATCH-05).
/// Joining the segment texts gives back the job description exactly.
/// </summary>
public class JdHighlight
{
    public List<JdSegment> Segments { get; set; } = new();
    public List<JdSkillInfo> Skills { get; set; } = new();
}

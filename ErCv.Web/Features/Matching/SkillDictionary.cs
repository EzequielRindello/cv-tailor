using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErCv.Web.Features.Matching;

/// <summary>One skill of Data/skills.json: its canonical name, other names for it, and how it is found in text.</summary>
public sealed class Skill
{
    private IReadOnlyList<string>? _textForms;

    /// <summary>Canonical name, lowercase. What the parser reports and what every alias normalizes to.</summary>
    public string Name { get; init; } = "";

    /// <summary>Other names for the same skill ("golang" for "go"), lowercase.</summary>
    public List<string> Aliases { get; init; } = new();

    /// <summary>
    /// The forms searched in free text (job posts, bullets). Null means the name and every alias. A form with a capital
    /// letter only matches when written exactly that way ("Go", not "go ahead"). An empty list means the skill is only
    /// recognized as a profile skill name and never searched in text.
    /// </summary>
    public List<string>? Match { get; init; }

    /// <summary>Phrases, in any case, inside which a match does not count ("go-to-market" for "go").</summary>
    public List<string> Except { get; init; } = new();

    [JsonIgnore]
    public IReadOnlyList<string> TextForms => _textForms ??= Match ?? Aliases.Prepend(Name).ToList();
}

/// <summary>
/// The skill dictionary (MATCH-07): what the parser looks for in a job description and how skill names are normalized.
/// The data is Data/skills.json, embedded in the assembly and read once, so adding a skill or an alias needs no code change.
/// </summary>
public static class SkillDictionary
{
    #region Constants

    public const string ResourceName = "ErCv.Web.Data.skills.json";

    // Declared before All, which reads it during static initialization
    private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

    public static readonly IReadOnlyList<Skill> All = Load();

    private static readonly Dictionary<string, Skill> ByName = All
        .SelectMany(s => s.Aliases.Prepend(s.Name).Select(n => (Name: n, Skill: s)))
        .ToDictionary(x => x.Name, x => x.Skill, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, Skill> ByTextForm = All
        .SelectMany(s => s.TextForms.Select(f => (Form: f, Skill: s)))
        .ToDictionary(x => x.Form, x => x.Skill, StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Public Methods

    /// <summary>The skill with this name or alias (any case), or null when the dictionary does not know it.</summary>
    public static Skill? Find(string? nameOrAlias) =>
        nameOrAlias != null && ByName.TryGetValue(nameOrAlias.Trim(), out var skill) ? skill : null;

    /// <summary>The skill one of whose text forms is this word (any case), or null.</summary>
    public static Skill? FindByTextForm(string? word) =>
        word != null && ByTextForm.TryGetValue(word.Trim(), out var skill) ? skill : null;

    #endregion

    #region Private Methods

    private static IReadOnlyList<Skill> Load()
    {
        using var stream = typeof(SkillDictionary).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing");
        return JsonSerializer.Deserialize<List<Skill>>(stream, ReadOptions) ?? new List<Skill>();
    }

    #endregion
}

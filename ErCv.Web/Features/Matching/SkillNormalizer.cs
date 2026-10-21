namespace ErCv.Web.Features.Matching;

public static class SkillNormalizer
{
    #region Constants

    // Phrases that look like an extension of another skill but are a different technology.
    private static readonly HashSet<string> DistinctProducts =
        new(StringComparer.OrdinalIgnoreCase) { "react native" };

    #endregion

    #region Public Methods

    public static string Normalize(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill)) return "";
        var trimmed = skill.Trim().ToLower();
        return SkillDictionary.Find(trimmed)?.Name ?? trimmed;
    }

    public static bool FuzzyMatch(string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;

        var na = Normalize(a);
        var nb = Normalize(b);
        if (na == nb) return true;
        if (DistinctProducts.Contains(na) != DistinctProducts.Contains(nb)) return false;

        return ContainsTerm(na, nb) || ContainsTerm(nb, na);
    }

    /// <summary>
    /// True when <paramref name="term"/> appears in <paramref name="text"/> as a whole token, so "go" does not
    /// match "google" or "good", "java" does not match "javascript" and "c" does not match "c#" or "c++".
    /// </summary>
    public static bool ContainsTerm(string? text, string? term) => FindTerm(text, term).Any();

    /// <summary>Every place <paramref name="term"/> appears in <paramref name="text"/> as a whole token (see <see cref="ContainsTerm"/>).</summary>
    public static IEnumerable<(int Start, int Length)> FindTerm(string? text, string? term)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term)) yield break;

        term = term.Trim();
        var at = 0;
        while ((at = text.IndexOf(term, at, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            var end = at + term.Length;
            if (StartsToken(text, at, term) && EndsToken(text, end, term))
            {
                yield return (at, term.Length);
                at = end;
            }
            else
            {
                at++;
            }
        }
    }

    /// <summary>
    /// Every place the skill is written in <paramref name="text"/>, under any of its text forms ("golang" for "go").
    /// A skill the dictionary does not know is searched by its own name.
    /// </summary>
    public static IEnumerable<(int Start, int Length)> FindSkill(string? text, string? skill)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(skill)) return Enumerable.Empty<(int, int)>();

        var known = SkillDictionary.Find(skill);
        return known != null ? FindSkill(text, known) : FindTerm(text, Normalize(skill));
    }

    /// <summary>
    /// Every place a dictionary skill is written in <paramref name="text"/>: a text form with a capital letter only
    /// counts written that way, and a match inside one of the skill's exception phrases does not count.
    /// </summary>
    public static IEnumerable<(int Start, int Length)> FindSkill(string text, Skill skill)
    {
        foreach (var form in skill.TextForms)
        {
            var exactCase = form.Any(char.IsUpper);
            foreach (var hit in FindTerm(text, form))
            {
                if (exactCase && string.CompareOrdinal(text, hit.Start, form, 0, form.Length) != 0) continue;
                if (InsideException(text, hit, skill.Except)) continue;
                yield return hit;
            }
        }
    }

    /// <summary>True when the skill is written in <paramref name="text"/> (see <see cref="FindSkill(string?, string?)"/>).</summary>
    public static bool ContainsSkill(string? text, string? skill) => FindSkill(text, skill).Any();

    public static bool MatchesAny(string skill, IEnumerable<string> collection)
        => collection.Any(c => FuzzyMatch(skill, c));

    #endregion

    #region Private Methods

    private static bool StartsToken(string text, int at, string term)
    {
        if (at == 0) return true;
        var before = text[at - 1];
        if (char.IsLetterOrDigit(before) || before is '_' or '@') return false;
        // A dot between letters joins one name: "node.js" holds no "js", "D.C." no "c"
        if (before == '.' && at >= 2 && char.IsLetterOrDigit(text[at - 2])) return false;
        // A single letter after a hyphen or apostrophe ends a word ("Objective-C", "MVVM-C")
        return term.Length > 1 || before is not ('-' or '\'');
    }

    private static bool EndsToken(string text, int end, string term)
    {
        if (end >= text.Length) return true;
        var after = text[end];
        if (char.IsLetterOrDigit(after) || after is '#' or '+' or '_' or '@') return false;
        // "grafana.com" is an address, not the product
        if (after == '.' && end + 1 < text.Length && char.IsLetterOrDigit(text[end + 1])) return false;
        // "R&D", "C-level", "C's"
        return term.Length > 1 || after is not ('-' or '&' or '\'');
    }

    private static bool InsideException(string text, (int Start, int Length) hit, IReadOnlyList<string> phrases)
    {
        var hitEnd = hit.Start + hit.Length;
        foreach (var phrase in phrases)
        {
            // A phrase that contains the hit starts between hitEnd - phrase.Length and hit.Start
            var from = Math.Max(0, hitEnd - phrase.Length);
            var count = Math.Min(text.Length, hit.Start + phrase.Length) - from;
            if (count >= phrase.Length && text.IndexOf(phrase, from, count, StringComparison.OrdinalIgnoreCase) is var at
                && at >= 0 && at <= hit.Start)
                return true;
        }
        return false;
    }

    #endregion
}

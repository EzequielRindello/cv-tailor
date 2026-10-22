using System.Text.RegularExpressions;

namespace ErCv.Web.Constants;

/// <summary>Patterns of the CV languages (EDIT-04) that more than one feature writes with.</summary>
public static partial class LanguagePatterns
{
    // Spanish writes "e" instead of "y" before a word that starts with the sound /i/ ("Go e Istio"), not before "hie"
    [GeneratedRegex(@"^(i|hi(?!e))", RegexOptions.IgnoreCase)]
    public static partial Regex StartsWithISound();
}

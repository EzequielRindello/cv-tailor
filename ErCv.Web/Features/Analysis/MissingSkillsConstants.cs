namespace ErCv.Web.Features.Analysis;

public static class MissingSkillsConstants
{
    #region Limits

    public const int MaxRequiredMissing = 8;
    public const int MaxNiceToHaveMissing = 6;
    public const int MaxRequiredSuggestions = 5;
    public const int MaxNiceToHaveSuggestions = 3;
    public const int SkillWeightImpact = 50;   // Matches MatchScore skill weight (50%)
    public const int MaxPotentialIncrease = 40;

    #endregion

    #region Messages

    public const string RequiredSuggestionTemplate =
        "Add '{0}' to cv-master.json if you have experience with this technology";

    public const string NiceToHaveSuggestionTemplate =
        "Mention '{0}' in relevant project bullet points if you have used it";

    public const string ExtraRequiredSuggestionTemplate =
        "There are {0} more required skills: review the full JSON";

    #endregion
}

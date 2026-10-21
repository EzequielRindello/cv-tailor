namespace ErCv.Web.Features.Selection;

/// <summary>Labels of the score parts shown in the selection explanation (MATCH-04). Shared by the scorer and the selector.</summary>
public static class SelectionConstants
{
    #region Explanation Labels

    public const string LabelMatchedSkills = "Matched skills";
    public const string LabelKeywords = "Keywords";
    public const string LabelRecency = "Recency";
    public const string LabelSeniorityFit = "Seniority fit";
    public const string LabelJdSkills = "JD skills";
    public const string LabelMetric = "Metric";
    public const string LabelActionVerb = "Action verb";
    public const string LabelLength = "Length";
    public const string LabelNonTechPenalty = "Non-tech role penalty";

    public const string DetailMetric = "Contains a number or percentage";
    public const string DetailNonTechPenalty = "Tech job description, role is not a tech role";
    public const string DetailLength = "{0} characters";

    #endregion
}

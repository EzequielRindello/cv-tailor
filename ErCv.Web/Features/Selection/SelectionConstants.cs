namespace ErCv.Web.Features.Selection;

/// <summary>Values shared by the scorer, the selector and the summary generator.</summary>
public static class SelectionConstants
{
    #region Roles

    /// <summary>
    /// A role whose title holds one of these is a tech role: the selector keeps its full score for a technical job
    /// description and the summary counts its years. One list, so both agree on what a tech role is.
    /// </summary>
    public static readonly string[] TechRoleKeywords =
    {
        "developer", "engineer", "software", "programmer", "architect",
        "devops", "sre", "data", "backend", "frontend", "fullstack",
        "full stack", "full-stack", "tech", "cloud", "platform", "analyst",
        "consultant", "specialist"
    };

    #endregion

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

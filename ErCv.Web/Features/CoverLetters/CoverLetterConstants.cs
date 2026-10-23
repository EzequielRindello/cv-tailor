namespace ErCv.Web.Features.CoverLetters;

/// <summary>The fixed wording of a cover letter, per language (EDIT-03). Everything else in a letter is a quoted fact.</summary>
public static class CoverLetterConstants
{
    #region Structure

    // Bullets quoted per experience and experiences written about: "opening, two experience paragraphs, closing"
    public const int BulletsPerExperience = 2;
    public const int ExperiencesWritten = 2;
    public const int MaxSkillsNamed = 4;

    // Keys of the paragraphs (CoverLetterParagraph.Key); an experience paragraph adds the experience's own key
    public const string OpeningKey = "opening";
    public const string ClosingKey = "closing";
    public const string ExperienceKeyPrefix = "experience:";

    // The first line of a job post is used as the position only when it is short and names a role
    public const int MaxJobTitleLength = 100;

    public static readonly string[] RoleNouns =
    {
        "engineer", "developer", "programmer", "architect", "scientist", "analyst", "designer", "manager",
        "administrator", "consultant", "specialist", "lead", "intern", "executive", "director", "sre", "devops",
        "ingeniero", "ingeniera", "desarrollador", "desarrolladora", "programador", "programadora", "arquitecto",
        "arquitecta", "analista", "diseñador", "diseñadora", "gerente", "líder", "especialista", "pasante",
        "consultor", "consultora"
    };

    #endregion

    #region Phrases

    /// <summary>One language's wording. A trailing or leading space belongs to the phrase, not to the fact.</summary>
    public sealed record Phrases(
        string Salutation,
        string Subject,
        string ApplyForTitle,
        string ApplyForTitleEnd,
        string ApplyGeneric,
        string FirstRole,
        string NextRole,
        string RoleAt,
        string DatesOpen,
        string DatesCloseLead,
        string NoDatesLead,
        string SentenceEnd,
        string SkillsLead,
        string SkillsEnd,
        string ListSeparator,
        string ListLastSeparator,
        string ListLastSeparatorBeforeI,
        string ClosingParagraph,
        string SalutationCompanyLead,
        string SalutationCompanyEnd,
        string ClosingCompanyLead,
        string ClosingCompanyEnd,
        string SignOff,
        IReadOnlyList<string> MonthNames,
        string DateFormat);

    public static readonly Phrases English = new(
        Salutation: "Dear Hiring Manager,",
        Subject: "Application: ",
        ApplyForTitle: "I am writing to apply for the ",
        ApplyForTitleEnd: " position. ",
        ApplyGeneric: "I am writing to apply for this position. ",
        FirstRole: "In my role as ",
        NextRole: "Before that, as ",
        RoleAt: " at ",
        DatesOpen: " (",
        DatesCloseLead: "), my work included the following: ",
        NoDatesLead: ", my work included the following: ",
        SentenceEnd: ". ",
        SkillsLead: "The role asks for ",
        SkillsEnd: ", all of which are part of my experience.",
        ListSeparator: ", ",
        ListLastSeparator: " and ",
        ListLastSeparatorBeforeI: " and ",
        ClosingParagraph: "I would welcome the chance to discuss how my experience can help your team. Thank you for your time and consideration.",
        SalutationCompanyLead: "Dear ",
        SalutationCompanyEnd: " team,",
        ClosingCompanyLead: "I would welcome the chance to discuss how my experience can help the ",
        ClosingCompanyEnd: " team. Thank you for your time and consideration.",
        SignOff: "Sincerely,",
        MonthNames: new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" },
        DateFormat: "{1} {0}, {2}");

    public static readonly Phrases Spanish = new(
        Salutation: "Estimado equipo de selección:",
        Subject: "Postulación: ",
        ApplyForTitle: "Les escribo para postularme al puesto de ",
        ApplyForTitleEnd: ". ",
        ApplyGeneric: "Les escribo para postularme a este puesto. ",
        FirstRole: "En mi rol de ",
        NextRole: "Antes, como ",
        RoleAt: " en ",
        DatesOpen: " (",
        DatesCloseLead: "), mi trabajo incluyó lo siguiente: ",
        NoDatesLead: ", mi trabajo incluyó lo siguiente: ",
        SentenceEnd: ". ",
        SkillsLead: "El puesto pide ",
        SkillsEnd: ", que forman parte de mi experiencia.",
        ListSeparator: ", ",
        ListLastSeparator: " y ",
        ListLastSeparatorBeforeI: " e ",
        ClosingParagraph: "Me encantaría conversar sobre cómo mi experiencia puede aportar a su equipo. Muchas gracias por su tiempo y consideración.",
        SalutationCompanyLead: "Estimado equipo de ",
        SalutationCompanyEnd: ":",
        ClosingCompanyLead: "Me encantaría conversar sobre cómo mi experiencia puede aportar al equipo de ",
        ClosingCompanyEnd: ". Muchas gracias por su tiempo y consideración.",
        SignOff: "Saludos cordiales,",
        MonthNames: new[] { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" },
        DateFormat: "{0} de {1} de {2}");

    #endregion
}

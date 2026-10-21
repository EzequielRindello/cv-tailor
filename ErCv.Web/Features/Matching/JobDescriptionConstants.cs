using System.Text.RegularExpressions;

namespace ErCv.Web.Features.Matching;

public static partial class JobDescriptionConstants
{
    #region Parsing

    public const string YearsOfExperiencePattern = @"(\d+)\+?\s*(?:years?|yrs?)";
    public const string YearsWithTechPattern = @"(\d+)\+?\s*(?:years?|yrs?)\s*(?:of\s*)?(?:experience\s*)?(?:with\s*)?([a-z0-9\.\#\+\-\/]+)";
    public const string WordSplitPattern = @"\W+";
    public const string BulletLinePattern = @"^[\-\*\•\d\.]+\s*";
    public const string TitlePunctuationPattern = @"[()\[\],/|:;.]";
    public const string ExperienceKeyword = "experience";
    public const string RequiredKeyword = "required";
    public const string MustHaveKeyword = "must have";

    public const int MinKeywordLength = 4;
    public const int MaxKeywords = 30;
    public const int MinRequirementLength = 11;
    public const int MaxRequirementLength = 199;
    public const int MaxRequirements = 10;

    [GeneratedRegex(YearsOfExperiencePattern)]
    public static partial Regex YearsOfExperience();

    [GeneratedRegex(YearsWithTechPattern, RegexOptions.IgnoreCase)]
    public static partial Regex YearsWithTech();

    [GeneratedRegex(WordSplitPattern)]
    public static partial Regex WordSplit();

    [GeneratedRegex(BulletLinePattern)]
    public static partial Regex BulletLine();

    [GeneratedRegex(TitlePunctuationPattern)]
    public static partial Regex TitlePunctuation();

    #endregion

    #region Seniority

    public const string LevelIntern = "intern";
    public const string LevelJunior = "junior";
    public const string DefaultSeniority = "mid-level";
    public const string LevelSenior = "senior";
    public const string LevelStaff = "staff";
    public const string LevelLead = "lead";
    public const string LevelPrincipal = "principal";

    // The first line of a job description is read as its title when it is at most this long
    public const int MaxTitleLength = 120;

    // A level word followed by a role noun, with at most this many words between ("senior machine learning engineer")
    public const int MaxWordsBeforeRoleNoun = 3;

    // "N years" alone decides the seniority when no level is named
    public const int SeniorYearsThreshold = 7;
    public const int MidLevelYearsThreshold = 3;
    public const int JuniorYearsThreshold = 1;

    // How far back from a level word to look for a negation ("no senior experience needed")
    public const int NegationWindowChars = 60;

    public static readonly string[] SeniorityNegations =
    {
        "no ", "not ", "without ", "don't ", "dont ", "non-", "doesn't require"
    };

    /// <summary>
    /// The levels the parser reports and the words that name them. A title is checked in this order and the first
    /// level it names wins, so "Senior Staff Engineer" is staff and "Lead / Senior" is lead.
    /// </summary>
    public static readonly (string Level, string[] Words)[] SeniorityLevels =
    {
        (LevelIntern, new[] { "intern", "internship", "co-op" }),
        (LevelJunior, new[] { "junior", "jr", "entry-level", "entry level", "new grad", "graduate", "university grad", "early career", "associate" }),
        (LevelPrincipal, new[] { "principal", "distinguished", "fellow" }),
        (LevelStaff, new[] { "staff" }),
        (LevelLead, new[] { "lead", "tech lead", "team lead" }),
        (DefaultSeniority, new[] { "mid-level", "mid level", "intermediate", "semi senior", "semi-senior", "ssr" }),
        (LevelSenior, new[] { "senior", "sr" }),
    };

    /// <summary>Roman numerals after the role ("Software Engineer II"). They count only when the title names no level word.</summary>
    public static readonly (string Level, string[] Numerals)[] NumberedLevels =
    {
        (LevelJunior, new[] { "i" }),
        (DefaultSeniority, new[] { "ii" }),
        (LevelSenior, new[] { "iii", "iv" }),
    };

    // Phrases that hold a level word without naming a level
    public static readonly string[] NotLevelPhrases = { "member of technical staff" };

    // What a level word must be followed by in the body to describe the role, not the team ("senior engineers")
    public static readonly string[] RoleNouns =
    {
        "engineer", "developer", "programmer", "scientist", "architect", "sre", "analyst", "role", "position",
        "ingeniero", "ingeniera", "desarrollador", "desarrolladora", "programador", "programadora"
    };

    #endregion

    #region Keyword Lists

    public static readonly string[] CommonWords =
    {
        "the", "and", "for", "with", "you", "that", "this", "will", "from",
        "have", "has", "are", "our", "your", "their", "been", "more", "than",
        "such", "about", "into", "through", "should", "would", "could", "must",
        "what", "when", "where", "which", "who", "how", "they", "them", "then",
        "also", "both", "each", "few", "most", "other", "some", "very", "just",
        "like", "well", "good", "great", "new", "high", "best", "able", "need",
        "make", "work", "role", "team", "build", "using", "help", "join", "want",
        "look", "plus", "take", "provide", "ensure", "maintain", "develop",
        "candidate", "position", "company", "opportunity", "environment",
        "responsibilities", "requirements", "qualifications", "preferred",
        "required", "bonus", "ideal", "apply", "including", "following"
    };

    #endregion
}

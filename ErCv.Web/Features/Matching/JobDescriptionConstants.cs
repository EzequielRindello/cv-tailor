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

    #region Sections

    // A line without a bullet that names no skill is a section heading when it ends with ':' or is this short
    // and does not end like a sentence ("Why Discord?" is a heading)
    public const int MaxHeadingWords = 12;
    public const char HeadingEnd = ':';
    public const string SentenceEndChars = ".!";

    /// <summary>Headings of the company's own sections, which name a skill when the company is one ("About MongoDB").</summary>
    public static readonly string[] CompanyHeadingPrefixes = { "about ", "why ", "life at ", "acerca de ", "sobre " };

    // The company is read only from headings that name it ("About Stripe", "Life at Ramp", "Sobre Mercado Libre"),
    // never from the body: a wrong company is worse than none (EDIT-09)
    public static readonly string[] CompanyNamePrefixes = { "about ", "life at ", "acerca de ", "sobre " };

    // What follows those prefixes in the posts' own sections ("About the role", "About you"), not a company
    public static readonly string[] NotACompany =
    {
        "the", "this", "you", "us", "your", "our", "me", "el", "la", "los", "las", "este", "esta", "ti", "ustedes",
        "nosotros", "vos", "role", "team", "job", "position", "company", "opportunity", "puesto", "rol", "equipo", "empresa"
    };

    // A heading about a program or a section is not about the employer ("About the Binance Accelerator Program")
    public static readonly string[] NotAnEmployerWord =
    {
        "program", "programme", "accelerator", "internship", "fellowship", "benefits", "process", "culture", "values"
    };

    public const int MaxCompanyWords = 5;

    /// <summary>
    /// Words that mark something the post does not require. Under a heading with one of them ("Nice to have",
    /// "Bonus points", "Preferred qualifications") every skill is optional until the next heading; a sentence with
    /// one ("Kafka is a plus") makes its own skills optional. Taken from the headings of the job-post corpus.
    /// </summary>
    public static readonly string[] OptionalCues =
    {
        "nice to have", "nice-to-have", "bonus", "preferred", "desired", "desirable", "a plus", "added plus",
        "not required", "stand out", "helpful", "even better", "good to have", "great to have",
        "deseable", "valorable", "se valora", "un plus"
    };

    /// <summary>Cues that make optional only what follows them: in "AWS or GCP, ideally including access logs" the clouds stay required.</summary>
    public static readonly string[] ForwardOptionalCues = { "ideally", "preferably", "idealmente", "preferentemente" };

    public const string SentenceBreakPattern = @"(?<=[.!?;])\s+";

    public const string ParentheticalPattern = @"\([^()]*\)";

    [GeneratedRegex(SentenceBreakPattern)]
    public static partial Regex SentenceBreak();

    [GeneratedRegex(ParentheticalPattern)]
    public static partial Regex Parenthetical();

    #endregion
}

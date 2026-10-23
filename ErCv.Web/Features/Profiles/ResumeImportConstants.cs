using ErCv.Web.Features.Matching;

namespace ErCv.Web.Features.Profiles;

/// <summary>Limits and word lists for reading a profile out of an uploaded resume (PROF-05). English and Spanish.</summary>
public static class ResumeImportConstants
{
    #region Limits

    public const int BytesPerMegabyte = 1024 * 1024;

    /// <summary>Largest resume accepted, in MB. A text resume is well under 1 MB; this leaves room for a photo.</summary>
    public const int MaxImportMegabytes = 4;

    public const long MaxImportBytes = MaxImportMegabytes * BytesPerMegabyte;

    // A resume is one to three pages; the rest of a longer PDF is not read
    public const int MaxPdfPages = 6;

    // A one-page text resume has well over a thousand letters; a scan's watermark or page number has a few dozen.
    // Below this many letters in the pages that are read, a PDF is an image
    public const int MinPdfTextLetters = 100;

    // A Word file is a zip: refuse one that expands past this before opening it (zip bomb)
    public const long MaxDocxUncompressedBytes = 32 * 1024 * 1024;

    // A section title is a short line ("Work Experience", "Formación académica")
    public const int MaxSectionTitleWords = 4;

    // Next to an entry's dates: its role and its company
    public const int MaxEntryHeadLines = 2;

    // A project's name is a short line without a final period
    public const int MaxProjectNameWords = 8;

    // An entry's title or company line is short and does not end like a sentence
    public const int MaxHeadWords = 12;

    public const int MaxNameWords = 5;
    public const int MaxLocationWords = 6;
    public const int MinPhoneDigits = 7;

    // "Languages: C#, Java": a label is the text before a colon this close to the start of the line
    public const int MaxLabelLength = 40;
    public const int MaxSkillLength = 40;

    // Letter-spaced titles ("E X P E R I E N C E") have at least this many single letters, most of their tokens
    // are single letters and none is longer than a kerned group ("RT", "IÓN")
    public const int MinLetterSpacedLetters = 3;
    public const int MaxKernedLetters = 3;
    public const double MinSingleLetterShare = 0.6;

    // An entry without dates opens with a role line and a company line this short
    public const int MaxDatelessHeadWords = 6;

    // PDF layout: words whose baselines differ by less than this share of their height are on one line, a gap wider
    // than this many characters splits a line in columns, and a page is two columns when a vertical band in its
    // middle (away from the margins by this share of the width) is crossed by at most this share of its lines
    public const double SameLineTolerance = 0.5;
    public const double ColumnGapChars = 3;
    public const double GutterMargin = 0.2;
    public const double GutterStep = 2;
    public const double MaxGutterCrossing = 0.1;
    public const int MinColumnLines = 5;

    // A PDF line was wrapped when the next line's first word, plus this many characters (the space), did not fit
    // before the column's right edge
    public const double WrapSlackChars = 1;

    // ...and only within a paragraph: its baseline at most this much farther than the closest pair of lines on the
    // page. The margin between two list items can be one pixel (0.75pt on 10.5pt lines), hence the tight ratio
    public const double MaxParagraphLeading = 1.04;

    /// <summary>Ends a PDF line that was wrapped (it reached the right edge): the next line goes on with it.</summary>
    public const char WrapMark = '\u2028';

    // In a letter-spaced line, a gap this many times the usual gap between letters is a space between words
    public const double LetterSpacedWordGap = 1.5;

    // Reading a hostile file must not hold the visitor's circuit; at most this many files are read at a time
    public const int MaxReadSeconds = 15;
    public const int MaxConcurrentReads = 2;

    #endregion

    #region Text

    public const string ItemPrefix = "- ";

    /// <summary>The file types that can be read; the input's <c>accept</c> list and the extension check share them.</summary>
    public static readonly string[] AcceptedExtensions = { ".pdf", ".docx" };

    public const string Accept =
        ".pdf,.docx,application/pdf,application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public const string WrongTypeMessage = "Only PDF and Word (.docx) files can be read.";

    public const string ScannedPdfMessage =
        "This PDF has no text to read (a scanned resume is an image). Export a text PDF from your editor, or use the Word file.";

    public const string NoTextMessage =
        "No text was found in the file. A scanned resume (an image) cannot be read. Use a text PDF or a Word file with text.";

    /// <summary>What the limits tooltip tells the visitor, built from the limits above.</summary>
    public static readonly string[] LimitLines =
    {
        $"PDF or Word (.docx) only, up to {MaxImportMegabytes} MB.",
        $"Only the first {MaxPdfPages} pages of a PDF are read.",
        "A scanned resume (an image) has no text and is refused.",
        "In a PDF, bullets with no bullet mark can be joined into one.",
        "A skill at the end of a line can be cut in two.",
        "What is read is a draft: check it before saving."
    };

    /// <summary>Links worth keeping from a file's link targets: the ones the profile has a field for.</summary>
    public static readonly string[] ProfileLinkHosts = { "linkedin.com/in/", "github.com/" };

    /// <summary>Between the columns of a line: a Word tab, a wide gap in a PDF line, three or more spaces.</summary>
    public const string ColumnSeparator = " | ";
    public const string ListStylePrefix = "List";
    public const string PresentWord = "Present";
    public const char LabelEnd = ':';
    public const char Hyphen = '-';

    // "Skills & Interests" is read as "skills and interests", and then as "skills"
    public const string Ampersand = "&";
    public const string And = " and ";

    // A line that ends with "Inc." or "S.A." (a short capitalized word) does not end a sentence
    public const int MaxAbbreviationLength = 5;

    // An unlabeled line of a project is its stack when at least this share of its items are known skills
    public const double MinKnownTechShare = 0.5;

    /// <summary>A PDF line that starts with one of these goes on from the line before ("(ASP.NET Core, React)").</summary>
    public const string ContinuationStarts = "()[]/&+%";

    /// <summary>A PDF line that ends with one of these, or with a connector word, goes on in the next line.</summary>
    public const string OpenLineEnds = ",+&/-·|(";

    public static readonly string[] ConnectorWords =
    {
        "and", "or", "of", "the", "to", "a", "an", "in", "by", "with", "for", "from", "on", "at", "using", "into",
        "via", "as", "y", "o", "de", "del", "la", "el", "los", "las", "en", "con", "para", "por", "que"
    };

    /// <summary>Separators left around the text of an entry line once its dates are taken out.</summary>
    public const string TrimSeparators = " |·,;:-–\u2014•";

    public static readonly string[] RemoteWords = { "remote", "remoto", "hybrid", "híbrido" };

    /// <summary>Characters that start a list item in extracted text.</summary>
    public const string BulletChars = "-*•◦▪▫●○‣▸▹–·■□➢►▶→»✓✔◆◇❖";

    /// <summary>Separators between the parts of a contact or entry line ("Acme | Remote", "Role · Company").</summary>
    public static readonly string[] PartSeparators = { ColumnSeparator, " · ", " • ", " \u2014 ", " – ", " - " };

    public const string PlaceSeparator = " · ";

    /// <summary>Separators between the items of a skills line.</summary>
    public static readonly char[] ItemSeparators = { ',', ';', '•', '|', '·' };

    // Month names as their first three letters, English and Spanish, in calendar order
    public static readonly string[][] MonthPrefixes =
    {
        new[] { "jan", "ene" }, new[] { "feb" }, new[] { "mar" }, new[] { "apr", "abr" }, new[] { "may" },
        new[] { "jun" }, new[] { "jul" }, new[] { "aug", "ago" }, new[] { "sep", "set" }, new[] { "oct" },
        new[] { "nov" }, new[] { "dec", "dic" }
    };

    // How the profile writes a month ("Jan 2020"): the form the validator and the date parser read
    public static readonly string[] ProfileMonthNames =
        { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    #endregion

    #region Sections

    /// <summary>
    /// Section titles, compared with the line without a final ':' and with "&amp;" read as "and". A title that is not
    /// listed is read by its first part: "Skills and Interests" is skills, "Education &amp; Certifications" education.
    /// </summary>
    public static readonly (ResumeSection Section, string[] Titles)[] SectionTitles =
    {
        (ResumeSection.Experience, new[]
        {
            "experience", "work experience", "professional experience", "employment", "employment history",
            "work history", "career history", "relevant experience", "experiencia", "experiencia laboral",
            "experiencia profesional", "trayectoria", "trayectoria profesional"
        }),
        (ResumeSection.Education, new[]
        {
            "education", "academic background", "education and training", "formación", "formacion",
            "formación académica", "formacion academica", "educación", "educacion", "estudios"
        }),
        (ResumeSection.Skills, new[]
        {
            "skills", "technical skills", "core skills", "key skills", "core competencies", "technologies",
            "tech stack", "skills and tools", "habilidades", "habilidades técnicas", "conocimientos",
            "conocimientos técnicos", "tecnologías", "tecnologias", "aptitudes", "competencias"
        }),
        (ResumeSection.Projects, new[]
        {
            "projects", "personal projects", "side projects", "selected projects", "key projects",
            "proyectos", "proyectos personales"
        }),
        (ResumeSection.Other, new[]
        {
            "summary", "professional summary", "profile", "executive profile", "about", "about me", "objective",
            "resumen", "resumen profesional", "perfil", "perfil ejecutivo", "perfil profesional", "sobre mí",
            "acerca de mí", "certifications", "certificates", "licenses and certifications", "courses",
            "languages", "awards", "honors", "publications", "volunteering", "volunteer experience", "interests",
            "references", "certificaciones", "cursos", "idiomas", "premios", "intereses", "referencias",
            "voluntariado"
        })
    };

    #endregion

    #region Entries

    /// <summary>Words of a job title: an entry's line with one is the role, not the company.</summary>
    public static readonly string[] RoleWords = JobDescriptionConstants.RoleNouns.Concat(new[]
    {
        "manager", "lead", "consultant", "designer", "intern", "specialist", "director", "administrator",
        "technician", "coordinator", "head of", "officer", "tester", "qa", "devops", "founder", "freelance",
        "contractor", "trainee", "assistant", "analista", "líder", "lider", "pasante", "consultor", "consultora",
        "diseñador", "diseñadora", "gerente", "técnico", "técnica", "jefe", "becario"
    }).ToArray();

    /// <summary>Words of a school's name: an education line with one is the institution.</summary>
    public static readonly string[] InstitutionWords =
    {
        "university", "universidad", "college", "institute", "instituto", "school", "escuela", "academy",
        "academia", "facultad", "polytechnic", "politécnico", "tecnológico", "bootcamp", "utn", "uba", "mit"
    };

    /// <summary>Words of a degree: an education line with one is the degree, not the school.</summary>
    public static readonly string[] DegreeWords =
    {
        "bachelor", "bachelor's", "master", "master's", "b.s.", "b.s", "bs", "b.sc.", "bsc", "b.a.", "ba", "m.s.",
        "m.s", "ms", "msc", "m.sc.", "mba", "ph.d.", "phd", "doctorate", "degree", "diploma", "associate",
        "certificate", "licenciatura", "licenciado", "licenciada", "ingeniería", "ingenieria", "tecnicatura",
        "técnico", "maestría", "maestria", "doctorado", "grado", "título"
    };

    /// <summary>Between a role and its company on one line: "Senior Engineer at Acme".</summary>
    public static readonly string[] RoleCompanyWords = { " at ", " @ ", " en " };

    public static readonly string[] GpaLabels = { "gpa", "promedio" };

    public static readonly string[] CourseLabels = { "relevant courses", "coursework", "courses", "materias", "cursos" };

    public static readonly string[] TechnologiesLabels = { "technologies", "tech stack", "stack", "tecnologías", "tecnologias" };

    #endregion

    #region Skills

    /// <summary>Labels of a skills line ("Languages: C#, Java") and the profile list they fill.</summary>
    public static readonly (SkillGroup Group, string[] Labels)[] SkillLabels =
    {
        (SkillGroup.Languages, new[] { "languages", "programming languages", "programming", "lenguajes", "lenguajes de programación" }),
        (SkillGroup.Frameworks, new[] { "frameworks", "libraries", "frameworks and libraries", "frontend", "backend", "librerías" }),
        (SkillGroup.Databases, new[] { "databases", "database", "data", "bases de datos", "base de datos" }),
        (SkillGroup.Tools, new[] { "tools", "devops", "tooling", "devops and tools", "tools and devops", "ai tools", "testing", "herramientas" }),
        (SkillGroup.Cloud, new[] { "cloud", "cloud platforms", "nube", "cloud and devops" }),
        (SkillGroup.Practices, new[] { "practices", "methodologies", "soft skills", "metodologías", "prácticas", "habilidades blandas" })
    };

    /// <summary>Lines of a skills section that are not skills ("Interests: chess, cycling").</summary>
    public static readonly string[] SkippedSkillLabels =
        { "interests", "hobbies", "spoken languages", "certifications", "intereses", "idiomas", "certificaciones" };

    // An item of an unlabeled skills line goes to the list it is found in, otherwise to Tools
    public static readonly string[] LanguageSkills =
    {
        "c#", "java", "javascript", "typescript", "python", "go", "rust", "c++", "c", "php", "ruby", "kotlin",
        "swift", "scala", "sql", "html", "css", "bash", "dart", "r", "elixir", "haskell", "perl", "lua",
        "objective-c", "vb.net", "f#", "matlab", "powershell", "shell", "scss", "sass"
    };

    public static readonly string[] FrameworkSkills =
    {
        ".net", "asp.net", "asp.net core", "entity framework", "blazor", "react", "angular", "vue", "svelte",
        "next.js", "nuxt", "node.js", "express", "nestjs", "spring", "spring boot", "django", "flask", "fastapi",
        "rails", "laravel", "jquery", "bootstrap", "tailwind", "redux", "react native", "flutter", "xamarin",
        "graphql", "jest", "xunit", "nunit", "playwright", "cypress", "pandas", "numpy", "tensorflow", "pytorch",
        "ef core", "signalr", "fluentvalidation", "react router", "wpf", "vite"
    };

    public static readonly string[] DatabaseSkills =
    {
        "postgresql", "mysql", "sql server", "oracle", "sqlite", "mongodb", "redis", "dynamodb", "cassandra",
        "elasticsearch", "mariadb", "snowflake", "bigquery", "firestore", "cosmos db", "neo4j", "supabase"
    };

    public static readonly string[] CloudSkills =
    {
        "aws", "azure", "gcp", "google cloud", "heroku", "vercel", "netlify", "cloudflare", "digitalocean",
        "firebase", "render", "kubernetes", "terraform"
    };

    public static readonly string[] PracticeSkills =
    {
        "agile", "scrum", "kanban", "tdd", "bdd", "ci/cd", "code review", "clean architecture", "solid", "ddd",
        "microservices", "rest", "pair programming", "design patterns", "unit testing", "mentoring"
    };

    #endregion
}

public enum ResumeSection { Header, Experience, Education, Skills, Projects, Other }

public enum SkillGroup { Languages, Frameworks, Databases, Tools, Cloud, Practices }

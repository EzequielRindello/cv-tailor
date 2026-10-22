namespace ErCv.Web.Constants;

public static class ApiConstants
{
    #region Session

    public const string SessionCvPrefix = "cv_";
    public const string SessionGeneratedCv = "cv_generated";

    public const int MaxSessionFileSizeBytes = 10 * 1024 * 1024; // 10 MB per file
    public const int MaxSessionTotalSizeBytes = 20 * 1024 * 1024; // 20 MB per session

    #endregion

    #region Request Limits

    // The very-long fixture is 50k characters; real posts in the corpus are far shorter
    public const int MaxJobDescriptionLength = 100_000;

    // EDIT-01: a rewritten bullet; the editor's textarea and the service check share it
    public const int MaxEditedBulletLength = 600;
    public const long MaxRequestBodyBytes = 2 * 1024 * 1024;

    #endregion

    #region Document Style

    // EDIT-04: the language of the CV (headings, dates, summary). The profile's own text is never translated.
    public const string LanguageEnglish = "en";
    public const string LanguageSpanish = "es";
    public static readonly string[] Languages = { LanguageEnglish, LanguageSpanish };

    // UX-10: an accent color and a font pairing, as keys of classes in Templates/partials/base-styles.html.
    // "default" keeps the template's own look.
    public const string StyleDefault = "default";
    public const string AccentNavy = "navy";
    public const string AccentTeal = "teal";
    public const string AccentBurgundy = "burgundy";
    public const string AccentForest = "forest";
    public const string AccentSlate = "slate";
    public const string FontsClassic = "classic";
    public const string FontsClean = "clean";
    public const string FontsMixed = "mixed";
    public static readonly string[] Accents = { StyleDefault, AccentNavy, AccentTeal, AccentBurgundy, AccentForest, AccentSlate };
    public static readonly string[] FontPairings = { StyleDefault, FontsClassic, FontsClean, FontsMixed };

    #endregion

    #region Profiles

    // The creator's own profile (PROF-06); what the API uses when a request names no profile
    public const string DefaultProfileId = "cv-master";
    // A made-up senior profile, the UI's first choice, so a visitor sees what a full CV looks like
    public const string SeniorSampleProfileId = "cv-master-senior";
    public const string ProfileFilePattern = "cv-master*.json";
    public const string ProfileIdPrefix = "cv-master-";
    public const string DefaultProfileName = "Default";

    // Key of one experience in RegenerateRequest.BulletSelections (the API client writes it, the service reads it)
    public const string BulletSelectionKeyPrefix = "exp";

    #endregion

    #region Formats

    public const string FormatPdf = "pdf";
    public const string FormatDocx = "docx";

    // Text exports (EDIT-06) are built in the browser session from the generated CV, not stored like the documents
    public const string FormatMarkdown = "md";
    public const string FormatText = "txt";

    public const string ContentTypePdf = "application/pdf";
    public const string ContentTypeDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public const string FileNamePrefix = "CV_";
    public const string CoverLetterFileNamePrefix = "Cover_Letter_";
    public const string FileNameDateFormat = "yyyyMMdd_HHmmss";

    #endregion

    #region Problem Titles

    public const string TitleBadRequest = "Bad request";
    public const string TitlePayloadTooLarge = "Payload too large";
    public const string TitleProfileNotFound = "Profile not found";
    public const string TitleNotFound = "Not found";

    #endregion

    #region Messages

    public const string ErrorJobDescriptionRequired = "Job description is required";
    public const string ErrorJobDescriptionTooLong = "Job description is too long (maximum {0:N0} characters)";
    public const string ErrorFormatsRequired = "At least one format must be selected";
    public const string ErrorFormatRequired = "Format is required. Allowed formats: {0}.";
    public const string ErrorUnsupportedFormat = "Unsupported format: {0}. Allowed formats: {1}.";
    public const string ErrorUnsupportedFormats = "Unsupported format(s): {0}. Allowed formats: {1}.";
    public const string ErrorGeneratedCvRequired = "GeneratedCV is required";
    public const string ErrorGeneratingCv = "Error generating CV";
    public const string ErrorRegeneratingCv = "Error regenerating CV";
    public const string ErrorDownloadingFile = "Error downloading file";
    public const string ErrorLoadingProfiles = "Error loading profiles";
    public const string ErrorFileNotFound = "File not found. Generate a CV first.";
    public const string ErrorSessionFileTooLarge = "Generated file is too large for session storage.";
    public const string ErrorSessionStorageLimit = "Session storage limit exceeded for generated files.";
    public const string ErrorProfileNotFound = "Profile '{0}' not found. Make sure '{0}.json' exists in the Data folder.";
    public const string ErrorProfileDeserialize = "Failed to deserialize CV Master";
    public const string ErrorProfileInvalid = "Profile is not valid. ";
    public const string ErrorProfileMoreIssues = " (and {0} more)";

    // How many validation issues an invalid custom profile lists in its error before summarizing the rest
    public const int MaxProfileIssuesShown = 5;

    public const string StatusHealthy = "healthy";

    #endregion
}

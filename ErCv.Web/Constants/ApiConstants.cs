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
    public const long MaxRequestBodyBytes = 2 * 1024 * 1024;

    #endregion

    #region Profiles

    public const string DefaultProfileId = "cv-master";
    public const string ProfileFilePattern = "cv-master*.json";
    public const string ProfileIdPrefix = "cv-master-";
    public const string DefaultProfileName = "Default";

    // Key of one experience in RegenerateRequest.BulletSelections (BulletEditor writes it, the service reads it)
    public const string BulletSelectionKeyPrefix = "exp";

    #endregion

    #region Formats

    public const string FormatPdf = "pdf";
    public const string FormatDocx = "docx";

    public const string ContentTypePdf = "application/pdf";
    public const string ContentTypeDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public const string FileNamePrefix = "CV_";
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

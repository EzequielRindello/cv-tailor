namespace ErCvApi.Constants
{
    public static class ApiConstants
    {
        public const string SessionCvPrefix = "cv_";
        public const string SessionGeneratedCv = "cv_generated";

        public const string ErrorJobDescriptionRequired = "Job description is required";
        public const string ErrorFormatsRequired = "At least one format must be selected";
        public const string ErrorGeneratedCvRequired = "GeneratedCV is required";
        public const string ErrorGeneratingCv = "Error generating CV";
        public const string ErrorRegeneratingCv = "Error regenerating CV";
        public const string ErrorDownloadingFile = "Error downloading file";
        public const string ErrorFileNotFound = "File not found. Generate a CV first.";
        public const string ErrorSessionFileTooLarge = "Generated file is too large for session storage.";
        public const string ErrorSessionStorageLimit = "Session storage limit exceeded for generated files.";

        public const string StatusHealthy = "healthy";

        public const string FormatPdf = "pdf";
        public const string FormatDocx = "docx";

        public const string ContentTypePdf = "application/pdf";
        public const string ContentTypeDocx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public const string FileNamePrefix = "CV_";
        public const string FileNameDateFormat = "yyyyMMdd_HHmmss";

        public const int MaxSessionFileSizeBytes = 10 * 1024 * 1024; // 10 MB per file
        public const int MaxSessionTotalSizeBytes = 20 * 1024 * 1024; // 20 MB per session
    }
}
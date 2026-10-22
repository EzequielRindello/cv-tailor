namespace ErCv.Web.Constants;

public static class TemplatePaths
{
    public const string DataFolder = "Data";
    public const string TemplatesFolder = "Templates";

    public const string CvMasterFile = "cv-master.json";
    public const string HarvardTemplate = "harvard-template.html";
    public const string ModernTemplate = "modern-template.html";
    public const string MinimalistTemplate = "minimalist-template.html";
    public const string CompactTemplate = "compact-template.html";
    public const string ExecutiveTemplate = "executive-template.html";
    public const string CoverLetterTemplate = "cover-letter-template.html";

    // UX-10: Mustache partials every template shares ({{> name}} is the file name without .html)
    public const string PartialsFolder = "partials";
}
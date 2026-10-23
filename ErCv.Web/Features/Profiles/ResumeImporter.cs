using ErCv.Web.Models;

namespace ErCv.Web.Features.Profiles;

/// <summary>
/// Turns an uploaded resume (PDF or Word) into a draft profile (PROF-05). The file is checked here, so every
/// caller gets the same limits; reading runs off the caller's thread, a few files at a time, with a time limit.
/// </summary>
public static class ResumeImporter
{
    #region Constants

    private const int BytesPerMegabyte = 1024 * 1024;

    private static readonly SemaphoreSlim Readers = new(ResumeImportConstants.MaxConcurrentReads);

    #endregion

    #region Public Methods

    public static async Task<ResumeImportResult> ImportAsync(byte[] bytes)
    {
        if (bytes.Length == 0) return Failed("The file is empty.");
        if (SizeError(bytes.Length) is { } tooLarge) return Failed(tooLarge);

        var format = ResumeText.Detect(bytes);
        if (format == null) return Failed("Only PDF and Word (.docx) files can be read.");

        await Readers.WaitAsync();
        try
        {
            var reading = Task.Run(() => Read(bytes, format.Value));
            // A hostile file can keep the reader busy; the visitor gets an answer anyway
            var finished = await Task.WhenAny(reading, Task.Delay(TimeSpan.FromSeconds(ResumeImportConstants.MaxReadSeconds)));
            return finished == reading ? await reading : Failed("Reading the file took too long.");
        }
        finally
        {
            Readers.Release();
        }
    }

    /// <summary>The message for a file over the size limit, or null when it fits (checked before the upload is read).</summary>
    public static string? SizeError(long bytes) =>
        bytes > ResumeImportConstants.MaxImportBytes
            ? $"The file is larger than {ResumeImportConstants.MaxImportBytes / BytesPerMegabyte} MB."
            : null;

    public static int SkillCount(Skills skills) =>
        skills.Languages.Count + skills.Frameworks.Count + skills.Databases.Count +
        skills.Tools.Count + skills.Cloud.Count + skills.Practices.Count;

    #endregion

    #region Private Methods

    private static ResumeImportResult Read(byte[] bytes, ResumeFormat format)
    {
        string text;
        try
        {
            text = format == ResumeFormat.Pdf ? ResumeText.FromPdf(bytes) : ResumeText.FromDocx(bytes);
        }
        catch (Exception)
        {
            // The file's own fault (damaged, encrypted, not really a resume): nothing to log
            return Failed("The file could not be read. Save it again as PDF or .docx and retry.");
        }

        if (string.IsNullOrWhiteSpace(text))
            return Failed("No text was found in the file. A scanned resume (an image) cannot be read.");

        var profile = ResumeParser.Parse(text, linesWrap: format == ResumeFormat.Pdf);
        var found = profile.Experience.Count + profile.Education.Count + profile.Projects.Count + SkillCount(profile.Skills);
        if (found == 0 && string.IsNullOrEmpty(profile.Header.Email))
            return Failed("No resume sections were found (Experience, Education, Skills, Projects).");

        return new ResumeImportResult(profile, null);
    }

    private static ResumeImportResult Failed(string error) => new(null, error);

    #endregion
}

/// <summary>The draft profile, or the message for the visitor when the file could not be used.</summary>
public record ResumeImportResult(CVMaster? Profile, string? Error);

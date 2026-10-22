using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Features.Profiles;
using ErCv.Web.Models;
using static ErCv.Tests.Documents.DocumentSupport;

namespace ErCv.Tests.Documents;

// PROF-05: a CV this app printed, read back as a profile, gives back what it printed
[Collection("Documents")]
[Trait("Category", "Documents")]
public class ResumeRoundTripTests
{
    private const string Jd = "backend-go-kafka";

    public static TheoryData<string, CVTemplate, string> PdfCases()
    {
        var data = new TheoryData<string, CVTemplate, string>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var template in Enum.GetValues<CVTemplate>())
        foreach (var language in ApiConstants.Languages)
            data.Add(profile, template, language);
        return data;
    }

    public static TheoryData<string, string> DocxCases()
    {
        var data = new TheoryData<string, string>();
        foreach (var profile in FixtureLoader.ProfileIds)
        foreach (var language in ApiConstants.Languages)
            data.Add(profile, language);
        return data;
    }

    [Theory]
    [MemberData(nameof(PdfCases))]
    public async Task A_pdf_of_this_app_reads_back_as_its_profile(string profile, CVTemplate template, string language)
    {
        var cv = Cv(profile, language);
        var pdf = await Renderer(template).RenderPdfAsync(cv);

        var result = await ResumeImporter.ImportAsync(pdf);

        AssertSameCv(cv, result);
    }

    [Theory]
    [MemberData(nameof(DocxCases))]
    public async Task A_docx_of_this_app_reads_back_as_its_profile(string profile, string language)
    {
        var cv = Cv(profile, language);
        var docx = Renderer(CVTemplate.Harvard).RenderDocx(cv);

        var result = await ResumeImporter.ImportAsync(docx);

        AssertSameCv(cv, result);
    }

    private static GeneratedCV Cv(string profile, string language)
    {
        var cv = Generate(profile, Jd, twoPages: true);
        cv.Language = language;
        return cv;
    }

    private static void AssertSameCv(GeneratedCV cv, ResumeImportResult result)
    {
        Assert.Null(result.Error);
        var read = result.Profile!;

        Assert.Equal(cv.Header.FullName, read.Header.FullName, ignoreCase: true);
        Assert.Equal(cv.Header.Email, read.Header.Email);
        // The demo profile's phone is a placeholder ("55555"), too short to read as a phone number
        if (cv.Header.Phone.Count(char.IsDigit) >= ResumeImportConstants.MinPhoneDigits) Assert.Equal(cv.Header.Phone, read.Header.Phone);
        Assert.Equal(cv.Header.Location, read.Header.Location);
        Assert.Equal(cv.Header.LinkedIn, read.Header.LinkedIn);
        Assert.Equal(cv.Header.GitHub, read.Header.GitHub);

        Assert.Equal(cv.Experience.Count, read.Experience.Count);
        foreach (var (printed, back) in cv.Experience.Zip(read.Experience))
        {
            Assert.Equal(printed.Role, back.Role);
            // Where the company ends and its place starts is a guess; together they are the printed line
            Assert.Equal(Place(printed.Company, printed.Location), Place(back.Company, back.Location));
            // Dates come back in the profile's format: "03/2016" as "Mar 2016", "Current" as "Present"
            Assert.Equal(ProfileDate(printed.StartDate), back.StartDate);
            Assert.Equal(ProfileDate(printed.EndDate), back.EndDate);
            Assert.Equal(printed.Bullets, back.Bullets);
        }

        Assert.Equal(cv.Education.Count, read.Education.Count);
        foreach (var (printed, back) in cv.Education.Zip(read.Education))
        {
            Assert.Equal(printed.Degree, back.Degree);
            Assert.Equal(Place(printed.Institution, printed.Location), Place(back.Institution, back.Location));
            Assert.Equal(ProfileDate(printed.GraduationDate), back.GraduationDate);
        }

        // A PDF can cut a skill at the end of a line ("SQL" / "Server"): its words all come back, none is added
        Assert.Equal(Words(cv.Skills), Words(AllSkills(read.Skills)));

        Assert.Equal(cv.Projects.Count, read.Projects.Count);
        foreach (var (printed, back) in cv.Projects.Zip(read.Projects))
        {
            Assert.Equal(printed.Name, back.Name);
            Assert.Equal(printed.Link, back.Link);
            Assert.Equal(printed.Description, back.Description);
            Assert.Equal(printed.Technologies, back.Technologies);
        }
    }

    private static string ProfileDate(string date) => date.Length == 0 ? date : ResumeParser.ProfileDate(date);

    private static string Place(string name, string place) =>
        string.Join(", ", new[] { name, place }.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static IEnumerable<string> Words(IEnumerable<string> items) =>
        items.SelectMany(i => i.Split(' ', StringSplitOptions.RemoveEmptyEntries)).Distinct().OrderBy(w => w, StringComparer.Ordinal);

    private static IEnumerable<string> AllSkills(Skills s) =>
        s.Languages.Concat(s.Frameworks).Concat(s.Databases).Concat(s.Tools).Concat(s.Cloud).Concat(s.Practices);
}

using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Models;
using ErCv.Web.Services;

namespace ErCv.Tests.Api;

// EDIT-04 and UX-10 at the service boundary: the API and the UI get the same checks
[Collection("Documents")]
[Trait("Category", "Api")]
public class StyleOptionsApiTests
{
    private static GenerateRequest Request() => new()
    {
        JobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
        Formats = new List<string> { "docx" }
    };

    private static async Task<T> WithServiceAsync<T>(Func<CVGeneratorService, Task<T>> action)
    {
        using var app = new ApiApp();
        return await app.InScopeAsync(sp => action((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!));
    }

    [Fact]
    public async Task A_spanish_request_gives_a_spanish_cv()
    {
        var request = Request();
        request.Language = ApiConstants.LanguageSpanish;
        request.Accent = ApiConstants.AccentForest;
        request.Fonts = ApiConstants.FontsMixed;

        var analysis = await WithServiceAsync(async s => (await s.BuildCvAsync(request)).Analysis);

        Assert.Equal(ApiConstants.LanguageSpanish, analysis.GeneratedCV.Language);
        Assert.Equal(ApiConstants.AccentForest, analysis.GeneratedCV.Accent);
        Assert.Contains("años de experiencia", analysis.GeneratedCV.Summary);
        Assert.Contains("lang=\"es\"", analysis.PreviewHtml);
        Assert.Contains("accent-forest fonts-mixed", analysis.PreviewHtml);
    }

    [Theory]
    [InlineData("fr", ApiConstants.StyleDefault, ApiConstants.StyleDefault)]
    [InlineData(ApiConstants.LanguageEnglish, "red", ApiConstants.StyleDefault)]
    [InlineData(ApiConstants.LanguageEnglish, ApiConstants.StyleDefault, "comic")]
    public async Task Unknown_language_accent_or_fonts_are_a_bad_request(string language, string accent, string fonts)
    {
        var request = Request();
        request.Language = language;
        request.Accent = accent;
        request.Fonts = fonts;

        await Assert.ThrowsAsync<ArgumentException>(() => WithServiceAsync(async s => (await s.BuildCvAsync(request)).Analysis));
    }

    [Fact]
    public async Task Every_template_has_a_sample_that_follows_the_chosen_style()
    {
        foreach (var template in Enum.GetValues<CVTemplate>())
        {
            var html = await WithServiceAsync(s =>
                s.RenderTemplateSampleAsync(template, ApiConstants.AccentNavy, ApiConstants.FontsClassic, ApiConstants.LanguageSpanish));

            Assert.Contains("accent-navy fonts-classic", html);
            Assert.Contains(">Experiencia<", html);
            Assert.DoesNotContain("{{", html);
        }
    }
}

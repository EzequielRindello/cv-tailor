using DocumentFormat.OpenXml.Packaging;
using ErCv.Tests.Fixtures;
using ErCv.Web.Constants;
using ErCv.Web.Features.Documents;
using ErCv.Web.Features.Editing;
using ErCv.Web.Models;
using ErCv.Web.Services;
using UglyToad.PdfPig;
using static ErCv.Tests.Api.ApiApp;
using static ErCv.Tests.Support.Build;

namespace ErCv.Tests.Api;

// EDIT-02 through the service: the preview of an edited CV, and its documents built on download
[Collection("Documents")]
[Trait("Category", "Api")]
public class EditedCvTests
{
    private const string Rewrite = "Rewritten by the user for this job";

    // The debounce is 400 ms; recomputing the scores must be a small part of that, even for the longest post
    private const double MaxReanalysisMilliseconds = 150;

    private static async Task WithServiceAsync(Func<CVGeneratorService, Task> action)
    {
        using var app = new ApiApp();
        await app.InScopeAsync(async sp =>
        {
            await action((CVGeneratorService)sp.GetService(typeof(CVGeneratorService))!);
            return true;
        });
    }

    private static async Task<GeneratedCV> GeneratedAsync(CVGeneratorService service)
    {
        var (_, analysis) = await service.BuildCvAsync(new GenerateRequest
        {
            JobDescription = FixtureLoader.LoadJd("backend-go-kafka"),
            Formats = new List<string> { "docx" }
        });
        return analysis.GeneratedCV;
    }

    private static string PdfText(byte[] pdf)
    {
        using var doc = PdfDocument.Open(pdf);
        return Squash(string.Join(" ", doc.GetPages().Select(p => p.Text)));
    }

    [Fact]
    public Task The_preview_shows_a_rewritten_bullet_and_drops_an_excluded_one() => WithServiceAsync(async service =>
    {
        var draft = new CvDraft(await GeneratedAsync(service), DocumentConstants.HarvardSectionOrder);
        var excluded = draft.Experiences[0].Bullets[1].Text;
        draft.EditBullet(0, 0, Rewrite);
        draft.ToggleBullet(0, 1);

        var html = Squash(service.RenderPreviewHtml(draft.Build(), CVTemplate.Harvard));

        Assert.Contains(Squash(Rewrite), html);
        Assert.DoesNotContain(Squash(excluded), html);
    });

    [Fact]
    public Task The_edited_documents_hold_the_edits_and_report_pdf_pages() => WithServiceAsync(async service =>
    {
        var draft = new CvDraft(await GeneratedAsync(service), DocumentConstants.HarvardSectionOrder);
        draft.EditBullet(0, 0, Rewrite);
        var cv = draft.Build();

        var (pdf, pages) = await service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "pdf");
        var (docx, docxPages) = await service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "docx");

        using (var doc = PdfDocument.Open(pdf))
            Assert.Equal(doc.NumberOfPages, pages);
        Assert.Contains(Squash(Rewrite), PdfText(pdf));

        using var stream = new MemoryStream(docx);
        using var word = WordprocessingDocument.Open(stream, false);
        Assert.Contains(Rewrite, word.MainDocumentPart!.Document!.Body!.InnerText);
        Assert.Equal(0, docxPages);
    });

    // The user's choices are printed as they are: the edited PDF is never trimmed to fit
    [Fact]
    public Task An_edited_cv_that_runs_long_is_not_trimmed() => WithServiceAsync(async service =>
    {
        var bullets = Enumerable.Range(0, 8).Select(i => $"Bullet {i} " + string.Join(" ", Enumerable.Repeat("delivered", 25))).ToArray();
        var cv = Generated(Enumerable.Range(0, 6).Select(i => Exp(role: $"Role {i}", bullets: bullets)).ToArray());

        var (pdf, pages) = await service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "pdf");

        Assert.True(pages > 1);
        var text = PdfText(pdf);
        Assert.All(cv.Experience.SelectMany(e => e.Bullets), b => Assert.Contains(Squash(b), text));
    });

    // EDIT-07: a skill the CV left out, added back from the profile, is in the edited PDF; adding everything is not trimmed
    [Fact]
    public Task A_skill_added_back_is_in_the_edited_pdf_and_adding_everything_is_not_trimmed() => WithServiceAsync(async service =>
    {
        var profile = await service.GetProfileAsync(ApiConstants.SeniorSampleProfileId);
        var draft = new CvDraft(await GeneratedAsync(service), DocumentConstants.HarvardSectionOrder, profile);
        var skill = draft.LeftOutSkills.First();

        draft.AddSkill(skill);
        var (withSkill, _) = await service.RenderEditedDocumentAsync(draft.Build(), CVTemplate.Harvard, "pdf");

        Assert.Contains(Squash(skill), PdfText(withSkill));

        while (draft.LeftOutExperiences.Count > 0) draft.AddExperience(0);
        for (var e = 0; e < draft.Experiences.Count; e++)
            while (draft.LeftOutBullets(e).Count > 0) draft.AddBullet(e, draft.LeftOutBullets(e)[0].ProfileIndex);
        var everything = draft.Build();
        var (pdf, pages) = await service.RenderEditedDocumentAsync(everything, CVTemplate.Harvard, "pdf");

        Assert.True(pages > 1);
        Assert.All(everything.Experience.SelectMany(x => x.Bullets), b => Assert.Contains(Squash(b), PdfText(pdf)));
    });

    private static async Task<AnalysisResult> AnalysisOfAsync(CVGeneratorService service, string jd = "backend-go-kafka")
    {
        var (_, analysis) = await service.BuildCvAsync(new GenerateRequest
        {
            JobDescription = FixtureLoader.LoadJd(jd),
            Formats = new List<string> { "docx" }
        });
        return analysis;
    }

    // EDIT-08: with no edit, the recomputed scores are the ones the generation gave
    [Fact]
    public Task Scores_of_an_unedited_cv_are_the_generated_ones() => WithServiceAsync(async service =>
    {
        var analysis = await AnalysisOfAsync(service);

        var again = service.AnalyzeEdited(analysis.GeneratedCV, analysis.Context!, CVTemplate.Harvard);

        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(analysis.MatchScore), System.Text.Json.JsonSerializer.Serialize(again.MatchScore));
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(analysis.MissingSkills), System.Text.Json.JsonSerializer.Serialize(again.MissingSkills));
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(analysis.KeywordAnalysis), System.Text.Json.JsonSerializer.Serialize(again.KeywordAnalysis));
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(analysis.ATSScore), System.Text.Json.JsonSerializer.Serialize(again.ATSScore));
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(analysis.JdHighlight), System.Text.Json.JsonSerializer.Serialize(again.JdHighlight));
    });

    // EDIT-08: taking out the only bullet that names a post keyword takes the keyword out of the CV
    [Fact]
    public Task Leaving_out_a_bullet_with_a_post_keyword_makes_the_keyword_missing() => WithServiceAsync(async service =>
    {
        var analysis = await AnalysisOfAsync(service);
        var cv = analysis.GeneratedCV;
        cv.Experience[0].Bullets = cv.Experience[0].Bullets.Concat(new[] { "Shipped the Kafka consumers on schedule" }).ToList();
        var withBullet = service.AnalyzeEdited(cv, analysis.Context!, CVTemplate.Harvard);
        var kafkaWith = withBullet.KeywordAnalysis.TechnicalKeywords.FirstOrDefault(k => k.Word.Equals("kafka", StringComparison.OrdinalIgnoreCase));
        if (kafkaWith == null) return; // the post does not name Kafka as a technical keyword

        cv.Experience[0].Bullets.RemoveAt(cv.Experience[0].Bullets.Count - 1);
        cv.Skills = cv.Skills.Where(s => !s.Equals("kafka", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var e in cv.Experience) e.Bullets = e.Bullets.Where(b => !b.Contains("kafka", StringComparison.OrdinalIgnoreCase)).ToList();
        var without = service.AnalyzeEdited(cv, analysis.Context!, CVTemplate.Harvard);

        Assert.True(kafkaWith.PresentInCV);
        Assert.False(without.KeywordAnalysis.TechnicalKeywords.Single(k => k.Word.Equals("kafka", StringComparison.OrdinalIgnoreCase)).PresentInCV);
    });

    // EDIT-08: the edit loop must stay fast. The largest post of the corpus, one bullet rewrite, median of several runs.
    [Fact]
    public Task Reanalysing_an_edited_cv_is_fast_on_the_largest_post() => WithServiceAsync(async service =>
    {
        var name = FixtureLoader.CorpusJdNames.OrderByDescending(n => FixtureLoader.LoadJd(n).Length).First();
        var analysis = await AnalysisOfAsync(service, name);
        var cv = analysis.GeneratedCV;
        cv.Experience[0].Bullets[0] = Rewrite;
        service.AnalyzeEdited(cv, analysis.Context!, CVTemplate.Harvard); // warm-up

        var times = Enumerable.Range(0, 9).Select(_ =>
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            service.AnalyzeEdited(cv, analysis.Context!, CVTemplate.Harvard);
            return watch.Elapsed.TotalMilliseconds;
        }).OrderBy(t => t).ToList();

        Console.WriteLine($"AnalyzeEdited median {times[4]:F1} ms (post of {FixtureLoader.LoadJd(name).Length} characters)");
        Assert.True(times[4] < MaxReanalysisMilliseconds, $"median {times[4]:F1} ms");
    });

    [Fact]
    public Task Bad_input_is_refused_at_the_service() => WithServiceAsync(async service =>
    {
        var cv = Generated(new[] { Exp(bullets: new[] { new string('a', ApiConstants.MaxEditedBulletLength + 1) }) });

        Assert.Throws<ArgumentException>(() => service.RenderPreviewHtml(cv, CVTemplate.Harvard));
        await Assert.ThrowsAsync<ArgumentException>(() => service.RenderEditedDocumentAsync(cv, CVTemplate.Harvard, "pdf"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.RenderEditedDocumentAsync(Generated(), CVTemplate.Harvard, "html"));
    });
}

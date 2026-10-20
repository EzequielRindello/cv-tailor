using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErCv.Tests.Fixtures;

namespace ErCv.Tests.Api
{
    // Same collection as the PDF tests: the app shuts the shared headless Chromium down when a host stops.
    [Collection("Documents")]
    [Trait("Category", "Api")]
    public class CvApiTests
    {
        [Fact]
        public async Task Generate_returns_200_with_files_scores_and_preview()
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(formats: new[] { "pdf", "docx" }));
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(json.GetProperty("success").GetBoolean());

            var files = json.GetProperty("files").EnumerateArray().ToList();
            Assert.Equal(new[] { "docx", "pdf" }, files.Select(f => f.GetProperty("format").GetString()!).OrderBy(x => x));
            Assert.All(files, f => Assert.True(f.GetProperty("size").GetInt32() > 0));
            Assert.All(files, f => Assert.StartsWith("CV_", f.GetProperty("filename").GetString()));

            Assert.InRange(json.GetProperty("matchScore").GetProperty("overall").GetInt32(), 0, 100);
            Assert.InRange(json.GetProperty("atsScore").GetProperty("overall").GetInt32(), 0, 100);
            Assert.True(json.TryGetProperty("missingSkills", out _));
            Assert.True(json.TryGetProperty("keywordAnalysis", out _));
            Assert.NotEmpty(json.GetProperty("generatedCV").GetProperty("experience").EnumerateArray());
            Assert.Contains("<html", json.GetProperty("previewHtml").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("Harvard")]
        [InlineData("Modern")]
        [InlineData("Minimalist")]
        public async Task Generate_accepts_every_template(string template)
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(template: template));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Download_round_trips_the_generated_file_through_the_session()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody()));
            var size = generate.GetProperty("files")[0].GetProperty("size").GetInt32();
            var filename = generate.GetProperty("files")[0].GetProperty("filename").GetString();

            var download = await app.Client.GetAsync("/api/cv/download/docx");
            var bytes = await download.Content.ReadAsByteArrayAsync();

            Assert.Equal(HttpStatusCode.OK, download.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                download.Content.Headers.ContentType?.MediaType);
            Assert.Equal(filename, download.Content.Headers.ContentDisposition?.FileName);
            Assert.Equal(size, bytes.Length);
            Assert.Equal("PK", System.Text.Encoding.ASCII.GetString(bytes, 0, 2));
        }

        [Fact]
        public async Task Download_pdf_after_generating_pdf_returns_a_pdf()
        {
            using var app = new ApiApp();
            await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(formats: new[] { "pdf" }));

            var download = await app.Client.GetAsync("/api/cv/download/pdf");
            var bytes = await download.Content.ReadAsByteArrayAsync();

            Assert.Equal("application/pdf", download.Content.Headers.ContentType?.MediaType);
            Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(bytes, 0, 5));
        }

        [Fact]
        public async Task Download_is_404_when_the_session_is_empty()
        {
            using var app = new ApiApp();

            var response = await app.Client.GetAsync("/api/cv/download/docx");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Download_is_404_for_a_format_that_was_not_generated()
        {
            using var app = new ApiApp();
            await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(formats: new[] { "docx" }));

            var response = await app.Client.GetAsync("/api/cv/download/pdf");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Download_does_not_share_files_between_sessions()
        {
            using var app = new ApiApp();
            await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody());
            using var otherBrowser = app.NewClient();

            var mine = await app.Client.GetAsync("/api/cv/download/docx");
            var theirs = await otherBrowser.GetAsync("/api/cv/download/docx");

            Assert.Equal(HttpStatusCode.OK, mine.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, theirs.StatusCode);
        }

        [Fact]
        public async Task Download_rejects_an_unsupported_format_with_400()
        {
            using var app = new ApiApp();

            var response = await app.Client.GetAsync("/api/cv/download/exe");
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Unsupported format", json.GetProperty("error").GetString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Generate_rejects_an_empty_job_description_with_400(string jd)
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(jd: jd));
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Job description is required", json.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Generate_rejects_missing_formats_with_400()
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(formats: Array.Empty<string>()));
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("At least one format must be selected", json.GetProperty("error").GetString());
        }

        [Theory]
        [InlineData("txt")]
        [InlineData("")]
        public async Task Generate_rejects_unsupported_formats_with_400(string format)
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(formats: new[] { "docx", format }));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Generate_with_unknown_profile_is_not_a_success()
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(profileId: "does-not-exist"));

            Assert.False(response.IsSuccessStatusCode);
        }

        [Theory]
        [InlineData("../../appsettings")]
        [InlineData("..\\..\\appsettings")]
        [InlineData("../appsettings.json")]
        [InlineData("/etc/passwd")]
        [InlineData("C:\\Windows\\win.ini")]
        public async Task Path_traversal_profile_id_is_rejected_and_serves_nothing(string profileId)
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(profileId: profileId));
            var body = await response.Content.ReadAsStringAsync();

            Assert.False(response.IsSuccessStatusCode);
            Assert.DoesNotContain("AllowedHosts", body);
            Assert.DoesNotContain("\"Logging\"", body);
            Assert.DoesNotContain("[fonts]", body);
        }

        // F8 / ENG-03: an unknown profile used to answer 500 with the exception message.
        [Theory]
        [InlineData("does-not-exist")]
        [InlineData("../../appsettings")]
        public async Task Unknown_profile_id_answers_a_client_error_without_internal_details(string profileId)
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(profileId: profileId));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("not found", body);
            Assert.DoesNotContain("Exception", body);
            Assert.DoesNotContain("ErCv", body);
            Assert.DoesNotContain(":\\", body);
        }

        [Fact]
        public async Task Unexpected_errors_answer_500_without_exception_details()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody()));
            var cv = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(generate.GetProperty("generatedCV").GetRawText())!;

            // A null experience entry makes the renderer throw a NullReferenceException.
            var response = await app.Client.PostAsync("/api/cv/regenerate", ApiApp.Raw(JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["generatedCV"] = new Dictionary<string, object?>
                {
                    ["header"] = cv["header"], ["summary"] = "", ["education"] = cv["education"],
                    ["experience"] = new object?[] { null }, ["skills"] = cv["skills"], ["projects"] = cv["projects"]
                },
                ["formats"] = new[] { "docx" }
            })));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("Error regenerating CV", body);
            Assert.DoesNotContain("NullReference", body);
            Assert.DoesNotContain("Object reference", body);
            Assert.DoesNotContain("at ErCv", body);
            Assert.DoesNotContain("\"details\"", body);
            Assert.DoesNotContain("StackTrace", body);
        }

        [Fact]
        public async Task Client_errors_are_problem_details_and_keep_the_error_field()
        {
            using var app = new ApiApp();

            var response = await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody(jd: ""));
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(400, json.GetProperty("status").GetInt32());
            Assert.Equal("Bad request", json.GetProperty("title").GetString());
            Assert.Equal("Job description is required", json.GetProperty("detail").GetString());
            Assert.Equal("Job description is required", json.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Regenerate_respects_bullet_selection_and_order()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate",
                ApiApp.GenerateBody(jd: FixtureLoader.LoadJd("backend-go-kafka"))));
            var generated = generate.GetProperty("generatedCV");
            var bullets = generated.GetProperty("experience")[0].GetProperty("bullets")
                .EnumerateArray().Select(b => b.GetString()!).ToList();
            Assert.True(bullets.Count >= 3, "fixture must give the first experience at least 3 bullets");

            // Keep the last bullet and the first one, in that order.
            var body = new Dictionary<string, object?>
            {
                ["generatedCV"] = generated,
                ["bulletSelections"] = new Dictionary<string, int[]> { ["exp0"] = new[] { 2, 0 } },
                ["formats"] = new[] { "docx" },
                ["template"] = "Harvard",
                ["allowTwoPages"] = false
            };
            var response = await app.PostAsync("/api/cv/regenerate", body);
            var json = await ApiApp.JsonAsync(response);
            var html = ApiApp.Squash(json.GetProperty("previewHtml").GetString()!);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(json.GetProperty("success").GetBoolean());
            Assert.Contains(ApiApp.Squash(bullets[2]), html);
            Assert.Contains(ApiApp.Squash(bullets[0]), html);
            Assert.DoesNotContain(ApiApp.Squash(bullets[1]), html);
            Assert.True(html.IndexOf(ApiApp.Squash(bullets[2]), StringComparison.Ordinal)
                      < html.IndexOf(ApiApp.Squash(bullets[0]), StringComparison.Ordinal));
        }

        [Fact]
        public async Task Regenerate_ignores_out_of_range_selection_indexes()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody()));
            var generated = generate.GetProperty("generatedCV");
            var first = generated.GetProperty("experience")[0].GetProperty("bullets")[0].GetString()!;

            var response = await app.PostAsync("/api/cv/regenerate", new Dictionary<string, object?>
            {
                ["generatedCV"] = generated,
                ["bulletSelections"] = new Dictionary<string, int[]> { ["exp0"] = new[] { 0, 99, -1 } },
                ["formats"] = new[] { "docx" }
            });
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(ApiApp.Squash(first), ApiApp.Squash(json.GetProperty("previewHtml").GetString()!));
        }

        [Fact]
        public async Task Regenerate_replaces_the_downloadable_file()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody()));

            var response = await app.PostAsync("/api/cv/regenerate", new Dictionary<string, object?>
            {
                ["generatedCV"] = generate.GetProperty("generatedCV"),
                ["bulletSelections"] = new Dictionary<string, int[]>(),
                ["formats"] = new[] { "docx" }
            });
            var json = await ApiApp.JsonAsync(response);
            var download = await app.Client.GetAsync("/api/cv/download/docx");

            Assert.Equal(json.GetProperty("files")[0].GetProperty("size").GetInt32(),
                (await download.Content.ReadAsByteArrayAsync()).Length);
        }

        [Fact]
        public async Task Regenerate_rejects_missing_formats_with_400()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody()));

            var response = await app.PostAsync("/api/cv/regenerate", new Dictionary<string, object?>
            {
                ["generatedCV"] = generate.GetProperty("generatedCV"),
                ["formats"] = Array.Empty<string>()
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // NEW FINDING F17: RegenerateRequest.GeneratedCV defaults to new(), so the null check never fires and an
        // empty CV is rendered and stored instead of answering 400.
        [Fact(Skip = "NEW FINDING F17: regenerate without a generatedCV answers 200 with an empty document")]
        public async Task Regenerate_rejects_a_missing_cv_with_400()
        {
            using var app = new ApiApp();

            var response = await app.Client.PostAsync("/api/cv/regenerate", ApiApp.Raw("{\"formats\":[\"docx\"]}"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Session_file_larger_than_10MB_answers_413()
        {
            using var app = new ApiApp();
            var generate = await ApiApp.JsonAsync(await app.PostAsync("/api/cv/generate", ApiApp.GenerateBody()));
            var cv = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(generate.GetProperty("generatedCV").GetRawText())!;
            var experience = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(cv["experience"].GetRawText())!;

            // Incompressible text so the zipped DOCX still exceeds the 10 MB per-file limit.
            var huge = ApiApp.RandomText(9 * 1024 * 1024);
            foreach (var exp in experience) exp["bullets"] = new[] { huge, huge };
            var body = new Dictionary<string, object?>
            {
                ["generatedCV"] = new Dictionary<string, object?> { ["header"] = cv["header"], ["summary"] = "", ["education"] = cv["education"], ["experience"] = experience, ["skills"] = cv["skills"], ["projects"] = cv["projects"] },
                ["bulletSelections"] = new Dictionary<string, int[]>(),
                ["formats"] = new[] { "docx" }
            };

            var response = await app.PostAsync("/api/cv/regenerate", body);
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal((HttpStatusCode)413, response.StatusCode);
            Assert.Contains("too large", json.GetProperty("error").GetString());
        }

        [Fact]
        public async Task Rate_limit_answers_429_after_60_requests_per_minute()
        {
            using var app = new ApiApp();

            var statuses = new List<HttpStatusCode>();
            for (var i = 0; i < 61; i++)
                statuses.Add((await app.Client.GetAsync("/api/cv/health")).StatusCode);

            Assert.All(statuses.Take(60), s => Assert.Equal(HttpStatusCode.OK, s));
            Assert.Equal(HttpStatusCode.TooManyRequests, statuses[60]);
        }

        [Fact]
        public async Task Health_reports_healthy_with_a_timestamp()
        {
            using var app = new ApiApp();

            var response = await app.Client.GetAsync("/api/cv/health");
            var json = await ApiApp.JsonAsync(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("healthy", json.GetProperty("status").GetString());
            Assert.True(DateTime.TryParse(json.GetProperty("timestamp").GetString(), out _));
        }

        [Fact]
        public async Task Profiles_lists_the_default_profile_with_a_display_name()
        {
            using var app = new ApiApp();

            var response = await app.Client.GetAsync("/api/cv/profiles");
            var json = await ApiApp.JsonAsync(response);
            var profiles = json.EnumerateArray().ToList();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var def = profiles.Single(p => p.GetProperty("id").GetString() == "cv-master");
            Assert.Equal("Default", def.GetProperty("name").GetString());
        }
    }
}

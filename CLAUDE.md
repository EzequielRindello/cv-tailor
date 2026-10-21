# CLAUDE.md

Conventions for anyone (person or agent) changing code in this repo. Follow them for every change; when existing code
does not match, fix it as part of the change you are making in that file, not in an unrelated one.

Roadmap, versioning and PR naming live in `features.md`. Architecture overview: `PROJECT_OVERVIEW.md`.

## Commands

- Build: `dotnet build er-cv-generator.sln` (must stay at 0 warnings)
- Fast tests: `dotnet test ErCv.Tests --filter "Category=Component|Category=Unit"` (seconds)
- Full suite without browsers: `dotnet test ErCv.Tests --filter "Category!=E2E"` (about 2 min, PDF tests start Chrome)
- Browser tests: `dotnet test ErCv.E2E` (uses an installed Chrome or Edge, nothing is downloaded)
- Corpus tool: `dotnet run --project ErCv.Collector -- refresh | report | check`
- Style fixes the tooling can apply: `dotnet format style er-cv-generator.sln --diagnostics IDE0005 IDE0161`

## Folder structure

```
ErCv.Web/
  Api/              Controllers. No logic: call the service, return its result.
  Services/         CVGeneratorService: request validation, orchestration, session.
  Features/<Name>/  The domain, one folder per feature. Each feature keeps its classes and its <Name>Constants.cs together.
    Matching/       Parsing the job description, skill aliases and matching, sample job posts
    Analysis/       Scores and reports on the generated CV (match, ATS, keywords, gaps, highlights)
    Selection/      Choosing experiences, bullets, skills, projects and the summary; page fit
    Profiles/       Validating, checking and (de)serializing the visitor's profile; date parsing
    Documents/      HTML, PDF and DOCX rendering
  Constants/        Only constants used across features (ApiConstants, TemplatePaths)
  Configuration/    Options bound from appsettings.json and app-wide middleware (security headers)
  Models/           Plain data classes and records shared across layers
  UI/               Browser-side state helpers for Blazor (BrowserStore, ToastService, UiConstants)
  Components/       Razor components: Pages/, Shared/, Layout/
  Data/             Content files only (JSON). No C# here.
  Templates/        CV HTML templates (Mustache)
ErCv.Tests/         Mirrors ErCv.Web: Features/<Name>/, UI/, Components/, Api/, Documents/, plus Support/ (builders)
ErCv.Collector/     Offline CLI for the job-post corpus (flat, small)
ErCv.E2E/           Playwright browser tests
```

- A new domain class goes in the feature folder it belongs to; a new feature gets its own folder under `Features/`.
- Namespace = folder path (`ErCv.Web.Features.Matching`, `ErCv.Tests.Features.Analysis`). File-scoped namespaces
  only (`namespace X;`), usings outside the namespace. `.editorconfig` enforces both.
- Razor components get feature namespaces from `Components/_Imports.razor`; add a `@using` there, not per file.

## Regions

Every class with members of two or more of the categories below is split into regions, in this order. Leave out
the regions that would be empty. Classes with a single category, models/records and test classes have no regions.

C# classes:

1. `#region Constants` (`const`, `static readonly`, `[GeneratedRegex]` declarations)
2. `#region Fields`
3. `#region Constructors`
4. `#region Properties` (events too)
5. `#region Public Methods` (public and internal)
6. `#region Private Methods`
7. `#region Nested Types`

Razor `@code` blocks:

1. `#region Constants`
2. `#region Parameters` (`[Parameter]`, `[CascadingParameter]`, `[Inject]`)
3. `#region State` (private fields)
4. `#region Lifecycle` (`OnInitialized*`, `OnParametersSet*`, `OnAfterRender*`, `Dispose*`)
5. `#region Event Handlers` (methods the markup or JS calls)
6. `#region Helpers` (computed properties, formatting, private logic, `RenderFragment`s)
7. `#region Nested Types`

Constants classes longer than about 50 lines use topic regions instead (`Scoring`, `Messages`, `Keyword Lists`...).

## Magic numbers and hardcoded values

- Every threshold, weight, score, limit, length, count to take, timeout and format string has a name.
  Not named: `0`, `1`, `-1` used as identity, guard or increment, and the labels of a small lookup `switch` whose
  results are named.
- Where the constant lives, narrowest scope first:
  - used by one class only: `private const` in that class's Constants region;
  - the class's feature already has a `<Feature>Constants.cs` that holds its values or messages: put it there;
  - used by more than one feature: `Constants/ApiConstants.cs` (or a new cross-cutting file there).
- Text: a message used once stays inline next to the check that produces it (validator messages, Razor copy).
  Text used in more than one place (labels, keys, section names, JS function names, profile sources, template keys)
  is always a constant. JS interop names are in `UI/UiConstants.cs` and must match `wwwroot/js/app.js`.
- Values an operator may tune per deployment (rate limit, session timeout) go in the `ErCv` section of
  `appsettings.json`, bound by `Configuration/ErCvOptions.cs`, validated at startup. Values that tune the CV logic
  are constants, not settings.
- External API field names (Collector sources) stay inline in the parser that reads them.

## Performance rules

- Fixed regex patterns are `[GeneratedRegex]` (the class becomes `partial`). Declare the pattern next to its
  constant: in the owning class if private, in the `<Feature>Constants` class if shared
  (`ATSConstants.Metrics()`, `JobDescriptionConstants.YearsOfExperience()`). Razor components declare them in a
  `.razor.cs` partial (see `CVPreview.razor.cs`); the regex generator cannot see Razor-generated code.
  A pattern built from data (a list of words) is built once into a `static readonly Regex[]`, never per call.
- Lookup data (HashSets, dictionaries, keyword lists) is `static readonly`, built once. Never allocate it inside a
  method or a loop.
- `JsonSerializerOptions` is a `static readonly` field, never `new` per call.
- Nothing that ships with the app (templates, data files) is read from disk per request: cache it
  (`DocumentRenderer.TemplateCache`). Restart the app after editing a template.
- Render a CV's HTML once and reuse it for the preview and the PDF.
- Fuzzy or term matching against the CV is the expensive part of analysis: compute it once per word and reuse it.

## Security rules

- Validate at the service boundary (`CVGeneratorService`), not in the controller or the component, so the API and
  the Blazor UI get the same checks. Limits live in `ApiConstants` (`MaxJobDescriptionLength`, `MaxRequestBodyBytes`,
  session sizes) and in `ProfileJson.MaxImportBytes`.
- The API body limit is `[RequestSizeLimit]` on `CVController`. Kestrel enforces it (TestServer does not, so it is
  verified in the container); `Program.cs` maps the resulting `BadHttpRequestException` to its 413.
- Error responses go through `CVGeneratorService.Problem`: the client gets a message written for it, never
  `ex.Message` of an unexpected exception. Log the exception instead.
- CV templates use `{{ }}` only (HTML-escaped). Never `{{{ }}}`. Links are written as `https://{{Link}}`.
- The CV preview is an `<iframe srcdoc>` with `sandbox` and without `allow-scripts`. Keep it that way.
- PDF rendering goes through `DocumentRenderer.RenderPdfAsync`, which caps concurrent Chrome pages
  (`DocumentConstants.MaxConcurrentPdfRenders`). The UI path is not rate limited, so this cap is what protects
  the instance.
- `Configuration/SecurityHeaders.cs` sets nosniff, X-Frame-Options, Referrer-Policy and a CSP. The preview iframe
  inherits the CSP: test the preview after changing it.
- `ForwardedHeaders.ForwardLimit` stays 1 (only Render's own hop is trusted, so the rate limiter cannot be dodged
  with a spoofed X-Forwarded-For).
- The container runs as the non-root `app` user. Do not add `USER root` for runtime steps.
- No CDN assets: vendor libraries under `wwwroot/lib/`.
- Never add a download, install or new package without asking the repo owner first.

## Tests

- Every behavior change ships with a test in the mirrored folder; refactors must keep the full suite green.
- Unit tests: `[Trait("Category", "Unit")]`. API tests: `[Trait("Category", "Api")]` plus
  `[Collection("Documents")]` (they share the headless Chrome with the PDF tests).
- Builders for test data are in `ErCv.Tests/Support/Build.cs`.

## Writing

- No em dashes in anything you write (code comments, docs, commit messages, UI text). Use a period, comma, colon
  or parentheses.
- Comments explain why, not what. Match the density of the surrounding code.

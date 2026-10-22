# PROJECT_OVERVIEW

The technical reference: what the app does, how a request flows through it, where each piece lives and why it is built
this way. The README is the short tour; `features.md` is the roadmap with the reasoning behind each item; `CLAUDE.md`
holds the coding conventions.

## Purpose

Generate an ATS-friendly CV tailored to one job description, without inventing anything. The app selects, reorders,
and summarizes what the visitor's profile already says, then explains each choice and scores the result. The invariant
tests (`ErCv.Tests/Invariants`) hold it to that promise: every bullet, skill, company and date in the output exists in
the profile.

Target users: job seekers, mainly software engineers, who adapt one master CV to many posts.

## Tech stack

- **App:** ASP.NET Core 10 monolith. Blazor Server UI and a REST API in one project, deployed as one Docker service on
  Render.
- **UI:** Blazor Server components, Bootstrap 5.3.2 and Bootstrap Icons 1.13.1 (vendored under `wwwroot/lib`, no CDN).
- **Documents:** Mustache templates rendered with Stubble; PDF through PuppeteerSharp (headless Chrome); DOCX through
  the Open XML SDK; plain text and Markdown built in code.
- **API docs:** the built-in OpenAPI document and the Scalar reference page, Development only.
- **Tests:** xUnit, bUnit, Playwright (installed Chrome or Edge), coverlet; GitHub Actions CI.
- **No database, no account.** Profiles, history and settings live in the visitor's browser (see the Privacy page).

## Request flow

```
POST /api/cv/generate (the Blazor UI calls the same service method in-process)
  jobDescription      -> JobDescriptionParser   -> JobRequirements (skills, keywords, seniority, years)
  profile + reqs      -> MatchScoreCalculator    -> match score (skills 50%, keywords 30%, seniority 20%)
  profile + reqs      -> MissingSkillsAnalyzer   -> required / nice-to-have skills the profile lacks
  profile + reqs      -> CVSelector              -> GeneratedCV + the reasons for each choice
                           RelevanceScorer          experiences ranked by skills, keywords, recency, seniority
                           SummaryGenerator         summary from the profile's own role and years
                           PageConstraintHelper     1 or 2 pages: character estimate first
  GeneratedCV         -> DocumentRenderer        -> HTML preview, PDF (page count measured, CV trimmed until it fits), DOCX
  GeneratedCV + reqs  -> KeywordAnalyzer, ATSChecker, JdHighlighter -> keyword table, ATS issues, highlighted post
  (includeCoverLetter) -> CoverLetterGenerator   -> letter built from fixed sentences and facts of the CV
  -> { files[], generatedCV, matchScore, missingSkills, keywordAnalysis, atsScore, previewHtml, explanation, coverLetter }

POST /api/cv/regenerate
  generatedCV + bulletSelections -> page constraint -> DocumentRenderer -> { files[], previewHtml }

GET /api/cv/download/{pdf|docx}   the file from the visitor's session
GET /api/cv/profiles              stored profiles (the creator's and the senior sample)
GET /api/cv/health                status, timestamp and pdfEngine (ready, starting, unavailable)
```

In the UI the result opens in a workspace: preview, scores, the reasons, the highlighted job post, the cover letter
switch and an editor. Edits only re-render HTML (EDIT-02); documents are rendered when downloaded.

## Project structure

```
er-cv-generator/
├── ErCv.Web/
│   ├── Api/CVController.cs          endpoints only: call the service, return its result
│   ├── Services/                    CVGeneratorService: validation at the boundary, orchestration, session
│   ├── Features/                    the domain, one folder per feature, each with its <Feature>Constants.cs
│   │   ├── Matching/                JobDescriptionParser, SkillDictionary (Data/skills.json), SkillNormalizer, samples
│   │   ├── Analysis/                MatchScoreCalculator, MissingSkillsAnalyzer, KeywordAnalyzer, ATSChecker, JdHighlighter
│   │   ├── Selection/               CVSelector, RelevanceScorer, SummaryGenerator, PageConstraintHelper, CvCloner
│   │   ├── Profiles/                ProfileValidator, ProfileHealthChecker, ProfileJson, DateParser
│   │   ├── Documents/               DocumentRenderer (HTML, PDF, DOCX), CvLabels (EN/ES), CvTextRenderer, PdfEngineWarmUp
│   │   ├── Editing/                 CvDraft: the editable copy behind the editor
│   │   └── CoverLetters/            CoverLetterGenerator, CoverLetterText
│   ├── Constants/                   cross-feature constants only (ApiConstants, TemplatePaths)
│   ├── Configuration/               ErCvOptions (the "ErCv" section of appsettings.json), SecurityHeaders
│   ├── Models/                      CVMaster, GeneratedCV, requests, analysis results
│   ├── UI/                          BrowserStore (localStorage), ToastService, ApplicationTracker, UiConstants
│   ├── Components/                  Pages (Home, Profile, Privacy, Error), Shared (one component per UI piece), Layout
│   ├── Templates/                   harvard, modern, minimalist, compact, executive, cover-letter, partials/
│   ├── Data/                        content only: profiles, skills.json, sample-jobs.json
│   └── wwwroot/                     css/site.css (design tokens first), js/app.js (interop), lib/ (vendored)
├── ErCv.Tests/                      mirrors ErCv.Web, plus Invariants/, Snapshots/, Fixtures/, Support/
├── ErCv.E2E/                        Playwright browser tests (start the app themselves)
├── ErCv.Collector/                  offline CLI that keeps corpus/ fresh and picks the app's samples
├── corpus/                          real job posts (one JSON each) and report.md
├── .github/                         ci.yml, corpus-refresh.yml, dependabot.yml, scripts/coverage_summary.py
├── Dockerfile, render.yaml          the image (with Google Chrome) and the Render service
├── features.md                      roadmap, findings, decisions
└── CLAUDE.md                        coding conventions
```

## Design decisions

- **Deterministic, explainable heuristics instead of a model.** Every score is a weighted sum of named constants, and
  the UI shows why each experience, bullet, skill and project was chosen (MATCH-04). An optional AI layer is planned
  as M6 (parked until there is an API key to use), off by default, and may only reorder or suggest, never write facts.
- **Constants, not settings, for the CV logic.** Weights and thresholds are proven by the invariant tests, the corpus
  report and the generation snapshots; a deployment cannot change them (ENG-04 was dropped for this reason). Only
  hosting values (rate limit, session timeout) are settings.
- **Page fit is measured, not guessed.** With a PDF, the page count is read from the rendered file and the CV is
  trimmed until it fits (MATCH-08). The character estimate only runs first, to start close.
- **One render per CV.** The HTML is rendered once and reused for the preview and the PDF; templates and partials are
  cached for the life of the process.
- **The skill dictionary is data.** `Data/skills.json` lists skills and aliases, tuned on real posts; the parser only
  counts words the dictionary knows (MATCH-07).
- **Validation at the service boundary**, so the API and the Blazor UI get the same checks and limits.
- **Domain classes are stateless singletons** in DI (ENG-01); `CVGeneratorService` is scoped.

## UI

- `Components/Pages/Home.razor` holds the flow: a 3-step composer (job post, profile, options), generation progress,
  then the results workspace. It calls `CVGeneratorService` in-process; the API endpoints are for other clients.
- `Components/Shared/*.razor`: one component per piece (selectors, score cards, editor, preview, history, tracker,
  cover letter, toasts). Each is plain HTML plus a `@code` block with regions (see `CLAUDE.md`).
- The CV preview is an `<iframe srcdoc>` with `sandbox` and no scripts.
- `UI/BrowserStore.cs` keeps settings, history (up to 50 entries, with application status and notes) and the visitor's
  profile in `localStorage`, through `wwwroot/js/app.js`.
- Keyboard: `Ctrl/Cmd+Enter` generates, `Ctrl/Cmd+Shift+V` pastes the job post, `Alt+Arrow` moves a bullet.
- Accessibility: WCAG AA colors in light and dark, keyboard reordering, focus management after generation; the E2E
  suite runs axe on every screen.

### Restyling

- **Look and feel:** `wwwroot/css/site.css`. Its first block holds the design tokens (`--color-*`, `--spacing-*`,
  `--radius-*`, `--shadow-*`), redefined for dark mode; changing them restyles most of the app.
- **Markup:** each `.razor` file in `Components/Shared`. Keep the `@onclick`/`@bind` bindings.
- **Bootstrap:** override its variables in `site.css` or swap the build in `wwwroot/lib/bootstrap`.
- **CV templates** (the documents) are separate: `Templates/*.html` plus `Templates/partials/`. Accents and fonts are
  classes picked from a fixed list, so nothing from a request reaches template CSS.
- `dotnet watch --project ErCv.Web` reloads the browser on save. Restart after editing a template or partial: they are
  cached.

## Key data shapes

```jsonc
// A profile (Data/cv-master.json, or the visitor's own from My profile)
{
  "header": { "fullName", "email", "phone", "location", "linkedin", "github" },
  "education": [{ "degree", "institution", "location", "graduationDate", "gpa", "honors", "relevantCourses": [] }],
  "experience": [{
    "role": "Senior Software Engineer",
    "company": "Acme Corp",
    "startDate": "Jun 2021",
    "endDate": "Present",               // "Present", "Jun 2024", "2024-06", "06/2024" or "2024"
    "bullets": ["Did X resulting in Y%"],
    "skills": ["React", "PostgreSQL"]  // used for relevance, so be complete
  }],
  "skills": { "languages": [], "frameworks": [], "databases": [], "tools": [], "cloud": [], "practices": [] },
  "projects": [{ "name", "description", "technologies": [], "highlights": [], "link" }]
}

// POST /api/cv/generate
{
  "jobDescription": "...",              // required, up to 100,000 characters
  "formats": ["pdf", "docx"],
  "template": "Harvard",                // Harvard, Modern, Minimalist, Compact, Executive
  "allowTwoPages": false,
  "profileId": "cv-master",
  "language": "en",                     // "en" or "es": headings, dates and labels
  "accent": "default",                  // default, navy, teal, burgundy, forest, slate
  "fonts": "default",                   // default, classic, clean, mixed
  "includeCoverLetter": false
}

// POST /api/cv/regenerate
{
  "generatedCV": { },                   // the generatedCV of a generate response
  "bulletSelections": { "exp0": [0, 2], "exp1": [1] },
  "formats": ["pdf"],
  "template": "Harvard",
  "allowTwoPages": false
}
```

Errors are RFC 7807 problem details (`application/problem+json`) with an `error` field; a 500 never carries exception
text. The request body is limited to 2 MB (413 above it) and the API to 60 requests per minute per client IP (429).

## Testing

| Layer | Where | What it proves |
|---|---|---|
| Unit | `ErCv.Tests/Features`, `UI` | each class: parser, scorers, selection, dates, documents, tracker |
| Invariants | `ErCv.Tests/Invariants` | nothing is invented; order, bounds and limits hold for every profile x job post |
| Documents | `ErCv.Tests/Documents` | DOCX is valid, PDF text and page count, every template stays within its page limit |
| API | `ErCv.Tests/Api` | endpoints, validation, limits, sessions, rate limit, OpenAPI, the old Node scenarios |
| Snapshots | `ErCv.Tests/Snapshots` | the generated CV and every score per profile x job post, as approved JSON |
| Components | `ErCv.Tests/Components` | bUnit: editor, selectors, score cards, history, profile fields, privacy |
| Browser | `ErCv.E2E` | Playwright: the whole flow, downloads, editing, persistence, 360 to 768 px, axe in light and dark |

The job posts come from `corpus/` (real posts, refreshed by hand through a PR) and `ErCv.Tests/Fixtures/jds`. CI runs
the suite with line coverage of `Features/` and `Services/` (gate 95%), the browser tests and the Docker build.

## Security and hosting

- Security headers (nosniff, frame options, referrer policy, CSP), HSTS outside Development, a non-root container.
- Forwarded headers trust one hop (Render's), so a spoofed `X-Forwarded-For` cannot dodge the rate limiter.
- At most 2 PDF renders at a time: the Blazor UI is not rate limited, so this cap protects the instance.
- Chrome is launched in the background at startup and a failed launch is retried by the next PDF (ENG-05).
- Memory: capped at 512 MB (the free plan), 6 concurrent PDF generations peaked around 250 MiB.

## Open items

Tracked in `features.md`: the "Open items after" sections and the findings tables. The main ones: the parser does
not split required and nice-to-have skills by the post's sections (MATCH-06, F25, planned for M5.1), scores are not
recomputed after edits, the cover letter has no company name, and the first public deploy is M5.2 (OPS-06).

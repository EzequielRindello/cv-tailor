# ER CV Generator

ASP.NET Core monolith (.NET 10) that generates ATS-optimized, tailored resumes from a job description. The UI is Blazor Server and the REST API lives in the same project, so the whole app ships as a single deploy. It never fabricates content: it selects, reorders, and summarizes what's already in `cv-master.json`.

---

## Run locally

```
dotnet run --project ErCv.Web      # or: dotnet watch --project ErCv.Web   (hot reload while editing UI)
```

The app serves the UI at `/` and the API at `/api/cv/*` on `http://localhost:5000` (override with the `PORT` env var). PDF export needs Chrome/Chromium: locally PuppeteerSharp downloads it on first use, or set `PUPPETEER_EXECUTABLE_PATH` to an installed Chrome/Chromium.

With Docker (same image Render builds):

```
docker build -t er-cv-generator .
docker run --rm -m 512m -e PORT=10000 -p 10000:10000 er-cv-generator
```

## Deploy (Render)

One Docker web service, defined in `render.yaml` (`Dockerfile` at the repo root):

- Multi-stage build on the .NET 10 SDK, runtime image installs Google Chrome (Ubuntu's chromium package is a snap stub) and sets `PUPPETEER_EXECUTABLE_PATH`.
- Binds to the `PORT` Render provides; health check at `/api/cv/health`.
- Forwarded headers are enabled so the rate limiter sees the real client IP behind Render's proxy.
- Memory: with the container capped at 512 MB (the free plan limit), 6 concurrent PDF generations peaked around 250 MiB. Move to Starter if that ever stops being enough.

---

## Blazor UI

- `Components/Pages/Home.razor`: single page with the generate and details views.
- `Components/Shared/*.razor`: one component per UI piece (selectors, history, cards, bullet editor, preview).
- `UI/BrowserStore.cs`: template/format settings and generation history (top 10) persisted in the visitor's `localStorage` via JS interop.
- The page calls `CVGeneratorService` in-process (`BuildCvAsync` / `RebuildCvAsync`); the `/api/cv/*` controller endpoints are unchanged.
- Generated files are streamed to the browser through `wwwroot/js/app.js`.
- Keyboard shortcuts: `Ctrl/Cmd+Enter` generate, `Ctrl/Cmd+Shift+V` paste into the job description.
- CV preview renders the template HTML in an iframe (`srcdoc`).
- Bootstrap 5.3.2 and Bootstrap Icons 1.13.1 are vendored under `wwwroot/lib` (no CDN at runtime).

### Restyling the UI

Markup and style are separate, so a redesign does not touch the C# logic:

- **Look and feel**: `wwwroot/css/site.css`. The first block holds the design tokens as CSS variables (`--color-*`, `--spacing-*`, `--radius-*`, `--shadow-*`). Changing those restyles most of the app at once.
- **Markup**: each `.razor` file in `Components/Shared` is plain HTML plus `@code` for its parameters. Edit the HTML and class names freely; keep the `@onclick`/`@bind` bindings.
- **Layout and page flow**: `Components/Layout/MainLayout.razor` and `Components/Pages/Home.razor`.
- **Per-component CSS (optional)**: add `Foo.razor.css` next to `Foo.razor`; Blazor scopes it to that component automatically (CSS isolation), which is the closest equivalent to CSS modules.
- **Bootstrap**: override its variables in `site.css` (`:root { --bs-primary: ...; }`) or swap `wwwroot/lib/bootstrap` for another theme build.
- **Workflow**: run `dotnet watch --project ErCv.Web` and edit `.razor`/`.css` files; the browser updates on save.
- **CV templates** (the generated PDF/DOCX) are separate from the app UI: `ErCv.Web/Templates/*.html`.

---

## Architecture

```
POST /api/cv/generate
  JobDescription (string) → JobDescriptionParser → JobRequirements
  JobRequirements + CVMaster → RelevanceScorer → scored experiences
  scored experiences → CVSelector → GeneratedCV (top 3 exp, top 3 bullets each, top 15 skills)
  CVMaster + requirements → SummaryGenerator → professional summary
  GeneratedCV → PageConstraintHelper → enforced 1 or 2 page CV
  GeneratedCV → DocumentRenderer → PDF (PuppeteerSharp) + DOCX (OpenXML)
  GeneratedCV + requirements → [MatchScoreCalculator, MissingSkillsAnalyzer, KeywordAnalyzer, ATSChecker] → AnalysisResult
  → returns { generatedCV, files[], matchScore, missingSkills, keywordAnalysis, atsScore, previewHtml }

POST /api/cv/regenerate
  GeneratedCV + BulletSelections (user-edited) → PageConstraintHelper → DocumentRenderer
  → returns { files[] }

GET /api/cv/download/{pdf|docx}
  → returns file from session
```

---

## Project Structure

```
er-cv-generator/
├── ErCv.Web/
│   ├── Controllers/CVController.cs          # POST /generate, /regenerate, GET /download/:format
│   ├── Services/CVGeneratorService.cs       # orchestrates full flow
│   ├── Helpers/
│   │   ├── JobDescriptionParser.cs          # extracts TechnicalSkills, Keywords, SeniorityLevel
│   │   ├── RelevanceScorer.cs               # scores each experience: skills 40%, keywords 30%, recency 20%, seniority 10%
│   │   ├── CVSelector.cs                    # picks top experiences/bullets; deprioritizes non-tech roles for tech JDs
│   │   ├── SummaryGenerator.cs              # builds 2-3 line summary: role + tech YoE + skills + achievement + soft skills
│   │   ├── PageConstraintHelper.cs          # trims bullets → projects → experiences to enforce page limit
│   │   ├── DocumentRenderer.cs              # Mustache HTML → PDF via PuppeteerSharp, DOCX via OpenXML
│   │   ├── MatchScoreCalculator.cs          # overall % = skills(50%) + keywords(30%) + seniority(20%)
│   │   ├── MissingSkillsAnalyzer.cs         # required vs nice-to-have gaps with suggestions
│   │   ├── KeywordAnalyzer.cs               # frequency + present/missing per category
│   │   └── ATSChecker.cs                    # email, phone, keyword density, sections, bullet length, metrics, template
│   ├── Constants/
│   │   ├── JobDescriptionConstants.cs       # 250+ tech keywords, seniority keywords, negation words, stopwords
│   │   ├── KeywordAnalyzerConstants.cs      # soft skills, tech words for categorization, stopwords
│   │   ├── MatchScoreConstants.cs           # weights, seniority map, color thresholds
│   │   ├── MissingSkillsConstants.cs        # nice-to-have keyword list, limits, suggestion templates
│   │   ├── ATSConstants.cs                  # penalties, thresholds, issue messages
│   │   ├── DocumentConstants.cs             # section names, font sizes, language/soft-skill keyword lists
│   │   ├── ApiConstants.cs                  # session keys, error messages, content types
│   │   └── TemplatePaths.cs                 # file path constants for templates and data
│   ├── Models/
│   │   ├── CVMaster.cs                      # Header, Education, Experience, Skills, Project
│   │   ├── GenerateModels.cs                # GeneratedCV, JobRequirements, RelevanceScore
│   │   └── FeatureModels.cs                 # MatchScore, MissingSkillsAnalysis, KeywordAnalysis, ATSScore, enums
│   ├── Templates/
│   │   ├── harvard-template.html            # single-column, Times New Roman — best ATS compatibility
│   │   ├── minimalist-template.html         # single-column, Helvetica — clean modern look, ATS-safe
│   │   └── modern-template.html             # two-column grid — NOT recommended for ATS portals
│   ├── Data/cv-master.json                  # master CV source — all experiences, skills, projects
│   ├── Components/                          # Blazor Server UI (App, Routes, Layout, Pages/Home, Shared/*)
│   ├── UI/                                  # BrowserStore (localStorage) + UI models
│   ├── wwwroot/                             # css/site.css, js/app.js, lib/ (Bootstrap 5.3.2 + Icons 1.13.1, vendored)
│   └── Program.cs                           # Blazor + API + rate limiter + forwarded headers, binds to PORT
├── Dockerfile                               # build + runtime image with Chromium
└── render.yaml                              # Render web service (Docker)
```

---

## Key Data Shapes

```jsonc
// cv-master.json — the only file to customize
{
  "header": { "fullName", "email", "phone", "location", "linkedin", "github" },
  "education": [{ "degree", "institution", "location", "graduationDate", "gpa", "honors" }],
  "experience": [{
    "role": "Junior Software Developer",
    "company": "Acme Corp",
    "location": "Remote",
    "startDate": "Jun 2024",
    "endDate": "Present",             // use "Present" for current role
    "bullets": ["Did X resulting in Y%..."],
    "skills": ["React", "PostgreSQL"] // used for relevance scoring — be complete
  }],
  "skills": {
    "languages": [], "frameworks": [], "databases": [],
    "tools": [], "cloud": [], "practices": []
  },
  "projects": [{ "name", "description", "technologies": [], "highlights": [], "link" }]
}

// POST /api/cv/generate — request
{
  "jobDescription": "...",
  "formats": ["pdf", "docx"],
  "template": "Harvard",
  "allowTwoPages": false,
  "profileId": "cv-master"
}

// POST /api/cv/generate — response
{
  "success": true,
  "files": [{ "format", "filename", "size" }],
  "generatedCV": { "header", "summary", "education", "experience", "skills", "projects" },
  "matchScore": { "overall", "skillsMatched", "totalSkills", "keywordsMatched", "seniorityMatch", "colorClass" },
  "missingSkills": { "requiredMissing": [], "niceToHaveMissing": [], "suggestions": [], "potentialScoreIncrease" },
  "keywordAnalysis": { "technicalKeywords": [], "softSkillKeywords": [], "presentInCV": [], "missingFromCV": [] },
  "atsScore": { "overall", "issues": [{ "severity", "issue", "fix" }], "recommendations": [], "colorClass" },
  "previewHtml": "<html>...rendered cv...</html>"
}

// POST /api/cv/regenerate — request
{
  "generatedCV": { ... },             // full GeneratedCV object from generate response
  "bulletSelections": { "exp0": [0,2], "exp1": [1] }, // indices of selected bullets per experience
  "formats": ["pdf"],
  "template": "Harvard",
  "allowTwoPages": false
}
```

---
### version 2.0.1
---

## Testing Scripts

- `node testing/test-runner-2.0.1.js`
- `node testing/test-runner-2.0.1.js --tc TC-01`
- `node testing/test-runner-2.0.1.js --profile cv-master`
- `PROFILE_ID=cv-master node testing/test-runner-2.0.1.js`

The runner now supports profile override via CLI or env:
- CLI flag: `--profile`
- Env var: `PROFILE_ID`
- Default: `cv-master`

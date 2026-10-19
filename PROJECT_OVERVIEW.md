# PROJECT_OVERVIEW

## Project Overview
- **Purpose:** Generate ATS-oriented CVs tailored to a specific job description without inventing new experience content.
- **Target users:** Job seekers (especially software engineers) who want quick role-specific CV customization.
- **Core workflow:**
  - Parse job description into technical skills, keywords, and seniority intent.
  - Score and rank CV experiences/projects for relevance.
  - Build a constrained 1-page or 2-page CV output.
  - Render downloadable documents (PDF and DOCX).
  - Return diagnostics: match score, missing skills, keyword analysis, ATS score.

## Tech Stack
- **Languages:** C#, Razor, HTML, CSS, a little JavaScript (interop).
- **App:** ASP.NET Core 10 monolith: REST API + Blazor Server UI in one project, deployed as one Docker service on Render.
- **UI:** Blazor Server components + Bootstrap 5.
- **Document generation:**
  - PDF via PuppeteerSharp (Chromium).
  - DOCX via Open XML SDK.
  - Template rendering via Stubble (Mustache-style templates).
- **Data tools:** System.Text.Json.
- **Dev/runtime tooling:** Swagger/OpenAPI, Docker, .NET SDK.

## Architecture
- **High-level design:** Layered monolith: API + Blazor Server components in one project.
  - Controller layer: HTTP endpoints (`/generate`, `/regenerate`, `/download`, `/profiles`, `/health`).
  - Service orchestration layer: `CVGeneratorService` coordinates parsing, scoring, selection, rendering, and analysis.
  - Helper domain layer: focused classes for parser, scorer, selector, ATS checks, and constraints.
  - UI layer: Blazor Server page (`Components/Pages/Home.razor`) with feature cards and editor-driven regeneration.
- **Key design decisions:**
  - Keep CV source-of-truth in JSON profiles (`Data/cv-master*.json`) for deterministic generation.
  - Use heuristic ranking/scoring (transparent logic) instead of opaque model-only matching.
  - Use server-side rendering for export fidelity and keep generated files in session for immediate download.

## Key Features
- Job-description parsing for skills/keywords/seniority inference.
- Relevance-based experience and bullet selection.
- Skill normalization with alias/fuzzy matching.
- Match score with skills/keywords/seniority breakdown.
- Missing-skills analysis with improvement suggestions.
- Keyword presence/density analysis.
- ATS compatibility checker with actionable issue list.
- Template selection (Harvard, Modern, Minimalist).
- PDF/DOCX export and bullet-level regeneration flow.
- Multi-profile backend support (`/api/cv/profiles`, `profileId`).
- Frontend profile selector with automatic hide when only one profile exists.
- Inline HTML preview (`previewHtml`) rendered in-app for quick visual validation.
- Keyboard productivity shortcuts (`Ctrl/Cmd+Enter`, `Ctrl/Cmd+Shift+V`).
- Generation history in localStorage (top 10, quick reload, clear).
- Export/template/page settings persistence in localStorage.

## Technical Highlights
- **State management:**
  - Component state in `Home.razor` coordinates generation lifecycle, analysis payloads, bullet-edit regeneration, profile selection, and history panel state.
  - LocalStorage-backed persistence for export settings and generation history.
  - Deterministic UI flow: input -> generate -> analyze -> optional bullet edits -> regenerate.
- **API handling:**
  - Typed backend models for generation and analysis payloads.
  - Session-backed download pattern separates generation and retrieval endpoints.
  - UI and API share one origin, so no CORS is needed; the UI calls `CVGeneratorService` in-process.
- **Performance-oriented logic:**
  - Top-N limits for skills/keywords/experiences/projects to bound response size and rendering work.
  - Page-fit enforcement heuristics reduce overflow risk before document render.
  - Shared Puppeteer browser instance for PDF rendering to reduce launch overhead.
  - Reuse of parsed/selected structures across analysis outputs.
- **Patterns used:**
  - Service orchestration pattern (`CVGeneratorService`) for endpoint business flow.
  - Single-responsibility helper classes (parser/scorer/analyzers/renderers).
  - Constants-driven behavior for thresholds/weights/messages.
  - Template-based rendering for output format flexibility.

## Possible Improvements
- Return updated `previewHtml` from `/regenerate` to keep preview in sync after bullet edits.
- Cover letter generation flow (template-driven, no hallucinated content).
- Optional LLM-assisted matching with controlled fallback and budget guardrails.
- Replace direct helper instantiation with DI for easier testing and configurability.
- Add robust date parsing/normalization and structured validation errors.
- Add unit/integration tests for parser, scorer, selector, and rendering pipelines.
- Add production hardening: request size/rate limits, stronger session storage strategy, and telemetry.
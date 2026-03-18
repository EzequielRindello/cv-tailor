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
- **Languages:** C#, JavaScript, HTML, CSS.
- **Backend:** ASP.NET Core 8 Web API.
- **Frontend:** React 18 + React Bootstrap.
- **Document generation:**
  - PDF via PuppeteerSharp (Chromium).
  - DOCX via Open XML SDK.
  - Template rendering via Stubble (Mustache-style templates).
- **HTTP/data tools:** Axios (frontend), System.Text.Json (backend).
- **Dev/runtime tooling:** Swagger/OpenAPI, npm/react-scripts, .NET SDK.

## Architecture
- **High-level design:** Layered API + component-based SPA.
  - Controller layer: HTTP endpoints (`/generate`, `/regenerate`, `/download`, `/profiles`, `/health`).
  - Service orchestration layer: `CVGeneratorService` coordinates parsing, scoring, selection, rendering, and analysis.
  - Helper domain layer: focused classes for parser, scorer, selector, ATS checks, and constraints.
  - UI layer: single-page React app with feature cards and editor-driven regeneration.
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

## Technical Highlights
- **State management:**
  - React local state (`useState`) coordinates generation lifecycle, analysis payloads, and bullet-edit regeneration.
  - Deterministic UI flow: input -> generate -> analyze -> optional bullet edits -> regenerate.
- **API handling:**
  - Typed backend models for generation and analysis payloads.
  - Session-backed download pattern separates generation and retrieval endpoints.
  - CORS + credentials enabled for local multi-port frontend integration.
- **Performance-oriented logic:**
  - Top-N limits for skills/keywords/experiences/projects to bound response size and rendering work.
  - Page-fit enforcement heuristics reduce overflow risk before document render.
  - Reuse of parsed/selected structures across analysis outputs.
- **Patterns used:**
  - Service orchestration pattern (`CVGeneratorService`) for endpoint business flow.
  - Single-responsibility helper classes (parser/scorer/analyzers/renderers).
  - Constants-driven behavior for thresholds/weights/messages.
  - Template-based rendering for output format flexibility.

## Possible Improvements
- Frontend integration of backend-delivered HTML preview (`previewHtml`).
- Frontend profile selector to use existing backend multi-profile support.
- Replace direct helper instantiation with DI for easier testing and configurability.
- Add robust date parsing/normalization and structured validation errors.
- Add unit/integration tests for parser, scorer, selector, and rendering pipelines.
- Add production hardening: request size/rate limits, stronger session storage strategy, and telemetry.
# ER CV Generator: Features & Status

Living roadmap and status tracker for the project. It replaces the old `BACKLOG.md`.

## How to use this file

- Every feature has an **ID**, a **status**, **acceptance criteria** (checkboxes) and a **Verify** line that says how it is proven to work.
- Tick acceptance criteria as they are met. Update the status in both the item and the dashboard table.
- **A feature is only `Done` when it is implemented AND verified** (an automated test, or a manual check written down under "Verify").

| Status | Meaning |
|---|---|
| `Todo` | Not started |
| `In progress` | Being worked on |
| `Implemented` | Code is in, not verified yet |
| `Done` | Implemented and verified |
| `Parked` | Intentionally postponed |

Priority: **P0** (do first) to **P3** (nice to have). Effort: **S** (hours), **M** (1 to 2 days), **L** (several days).

### Ground rules for every feature

- The app never fabricates CV content. It selects, reorders and summarizes what already exists in the profile. AI features may only *suggest*, and the user has to accept.
- No em dashes in any generated text, copy, code comments or docs.
- New packages, fonts or any other download must be confirmed before installing.
- Every bug fix gets a regression test.

---

## Dashboard

| ID | Title | Epic | Priority | Effort | Status |
|---|---|---|---|---|---|
| TEST-01 | xUnit test project + fixtures + JD corpus | E1 Tests | P0 | M | Done |
| TEST-02 | Unit tests per helper | E1 Tests | P0 | L | Done |
| TEST-03 | Invariant tests (no fabrication, ordering, bounds) | E1 Tests | P0 | M | Done |
| TEST-04 | Document tests (DOCX structure, PDF pages + text extraction) | E1 Tests | P0 | M | Done |
| TEST-05 | API integration tests (WebApplicationFactory) | E1 Tests | P0 | M | Done |
| TEST-06 | Blazor component tests (bUnit) | E1 Tests | P1 | M | Todo |
| TEST-07 | End-to-end tests (Playwright) | E1 Tests | P1 | L | Todo |
| TEST-08 | Golden snapshot tests for generation output | E1 Tests | P2 | S | Todo |
| TEST-09 | Retire the Node scripts in `testing/` | E1 Tests | P2 | S | Todo |
| TEST-10 | Coverage report and target | E1 Tests | P2 | S | Todo |
| UX-01 | Design system: tokens, fonts, dark mode | E2 Redesign | P0 | M | Todo |
| UX-02 | New app shell and editorial hero | E2 Redesign | P0 | M | Todo |
| UX-03 | Guided 3-step composer | E2 Redesign | P0 | M | Todo |
| UX-04 | Results workspace (no auto-download) | E2 Redesign | P0 | L | Todo |
| UX-05 | Score visualizations | E2 Redesign | P1 | M | Todo |
| UX-06 | Generation progress and skeletons | E2 Redesign | P1 | S | Todo |
| UX-07 | Empty, error and toast states | E2 Redesign | P1 | S | Todo |
| UX-08 | Accessibility (WCAG AA, keyboard reorder) | E2 Redesign | P1 | M | Todo |
| UX-09 | Responsive pass | E2 Redesign | P1 | M | Todo |
| UX-10 | CV template visual polish | E2 Redesign | P2 | M | Todo |
| PROF-01 | In-browser profile editor | E3 Your profile | P1 | L | Todo |
| PROF-02 | Profile JSON import/export + validation | E3 Your profile | P1 | M | Todo |
| PROF-03 | Generate from a custom profile | E3 Your profile | P1 | S | Todo |
| PROF-04 | Profile health check | E3 Your profile | P2 | M | Todo |
| PROF-05 | Import from existing PDF/DOCX resume | E3 Your profile | P3 | L | Todo |
| MATCH-01 | Fix fuzzy-match false positives | E4 Matching | P0 | S | Done |
| MATCH-02 | Word-boundary ATS keyword density | E4 Matching | P0 | S | Done |
| MATCH-03 | Shared date parser + correct "most recent role" | E4 Matching | P0 | M | Done |
| MATCH-04 | Explainability: "why this was chosen" | E4 Matching | P1 | M | Todo |
| MATCH-05 | JD highlighter | E4 Matching | P1 | M | Todo |
| MATCH-06 | Required vs nice-to-have from JD sections | E4 Matching | P2 | M | Todo |
| MATCH-07 | Skill/alias dictionary as data file | E4 Matching | P2 | S | Todo |
| MATCH-08 | Page fit measured on the rendered PDF | E4 Matching | P1 | M | Todo |
| EDIT-01 | Inline editing in the workspace | E5 Editing | P2 | L | Todo |
| EDIT-02 | Live HTML preview, documents on download | E5 Editing | P2 | M | Todo |
| EDIT-03 | Cover letter generator | E5 Editing | P2 | M | Todo |
| EDIT-04 | CV output language (EN/ES) | E5 Editing | P2 | M | Todo |
| EDIT-05 | More templates | E5 Editing | P3 | M | Todo |
| EDIT-06 | Plain-text / Markdown export | E5 Editing | P3 | S | Todo |
| APP-01 | Application tracker | E6 Workflow | P2 | M | Todo |
| APP-02 | Re-open a past generation | E6 Workflow | P2 | S | Todo |
| APP-03 | Sample JD gallery | E6 Workflow | P0 | S | Todo |
| APP-04 | Import JD from URL (Greenhouse/Lever) | E6 Workflow | P3 | M | Todo |
| AI-01 | LLM options + feature flag | E7 AI | P3 | S | Parked |
| AI-02 | Semantic matching (indices only) | E7 AI | P3 | M | Parked |
| AI-03 | Bullet rewrite suggestions (user accepts) | E7 AI | P3 | M | Parked |
| AI-04 | Cover letter polish | E7 AI | P3 | S | Parked |
| AI-05 | Cost guardrails and caching | E7 AI | P3 | S | Parked |
| ENG-01 | DI for helpers + single namespace root | E8 Engineering | P2 | M | Todo |
| ENG-02 | Consolidate duplicated logic, immutable selection | E8 Engineering | P0 | M | Done |
| ENG-03 | ILogger + ProblemDetails (no leaked exceptions) | E8 Engineering | P0 | S | Done |
| ENG-04 | Options pattern for scoring weights | E8 Engineering | P3 | S | Todo |
| ENG-05 | Chromium readiness in health check + warm-up | E8 Engineering | P2 | S | Todo |
| ENG-06 | Modern OpenAPI (built-in + Scalar) | E8 Engineering | P3 | S | Todo |
| OPS-01 | GitHub Actions CI | E9 DevOps | P0 | M | Done |
| OPS-02 | Dependabot | E9 DevOps | P2 | S | Todo |
| OPS-03 | README rewrite for portfolio | E9 DevOps | P1 | M | Todo |
| OPS-04 | PROJECT_OVERVIEW refresh | E9 DevOps | P2 | S | Todo |
| OPS-05 | Privacy note | E9 DevOps | P2 | S | Todo |

---

## Milestones

1. **M1 Foundation**: TEST-01, TEST-02, TEST-03, TEST-04, TEST-05, MATCH-01, MATCH-02, MATCH-03, ENG-02, ENG-03, OPS-01. Fixes ship with their tests from day one.
2. **M2 Redesign**: UX-01 to UX-09, APP-03, TEST-07 (E2E written against the new UI).
3. **M3 Your data**: PROF-01 to PROF-04, MATCH-04, MATCH-05, APP-01, APP-02.
4. **M4 Output**: EDIT-01 to EDIT-06, MATCH-08, UX-10.
5. **M5 Polish & presentation**: OPS-02 to OPS-05, ENG-01, ENG-04, ENG-05, ENG-06, TEST-06, TEST-08, TEST-09, TEST-10.
6. **M6 Optional AI**: AI-01 to AI-05.

---

## Code review findings (v2.0.x)

Found during the roadmap review. Each one is covered by a feature below.

| # | Finding | Where | Covered by |
|---|---|---|---|
| F1 | `FuzzyMatch` uses substring `Contains`: `go` matches `google`/`mongo`, `java` matches `javascript`. Inflates match score and skill selection. | `Helpers/SkillNormalizer.cs:95` | MATCH-01 (fixed) |
| F2 | ATS keyword density uses raw `Contains` (`go` matches "good"). | `Helpers/ATSChecker.cs:75` | MATCH-02 (fixed) |
| F3 | "Most recent role" sorts `EndDate` strings lexically, so `"May 2020"` ranks above `"Dec 2023"`. | `Helpers/MatchScoreCalculator.cs:67`, `Helpers/SummaryGenerator.cs:59` | MATCH-03 (fixed) |
| F4 | `ContainsTerm` copied 3 times, seniority map duplicated, year parsing duplicated. | `RelevanceScorer`, `MissingSkillsAnalyzer`, `KeywordAnalyzer`, `MatchScoreConstants`, `SummaryGenerator` | ENG-02 (fixed) |
| F5 | Generated CV shares `Header`/`Education` references with the master; page constraint mutates in place (caller clones as a workaround). | `Helpers/CVSelector.cs:59`, `Helpers/PageConstraintHelper.cs`, `Components/Pages/Home.razor:300` | ENG-02 (fixed) |
| F6 | Page fit is estimated by character count, not measured; a "1-page" CV can spill over. | `Helpers/PageConstraintHelper.cs` | MATCH-08, TEST-04 |
| F7 | Generate auto-downloads files before the user sees the preview. | `Components/Pages/Home.razor:268` | UX-04 |
| F8 | 500 responses include `ex.Message`; logging uses `Console.WriteLine`. | `Services/CVGeneratorService.cs:63`, helpers | ENG-03 (fixed) |
| F9 | Mixed namespaces (`ErCvApi.*` and `ErCv.Web.*`); helpers built with `new` instead of DI. | Whole project | ENG-01 |
| F10 | Docs drift: legacy FIX-002 says low match is below 30%, code uses below 40%. Node runner expectations depend on a fixture in `testing/`. | `Helpers/MatchScoreCalculator.cs:43`, `testing/` | OPS-04, TEST-09 |
| F11 | Bullet reordering is drag-only, not keyboard accessible. | `Components/Shared/BulletEditor.razor` | UX-08 |

### New findings (from TEST-02 to TEST-05)

Found while writing the tests. All fixed at the end of M1, each with a regression test.

| # | Finding | Status |
|---|---|---|
| F12 | `YearsWithTechPattern` captured the word after "N years of/in" as a skill (`b2b`, `or`, `in`), and the single-letter skill `c` matched inside accented words ("código"). Parser now matches whole tokens and ignores connector words. | Fixed |
| F13 | Keyword, bullet, project and seniority matching used substring `Contains` (same class as F1/F2), e.g. `intern` in `Internal`. Now whole-token. | Fixed |
| F14 | `FuzzyMatch` misfiled skills as nice-to-have (`rust` matched `zero trust`). | Fixed with MATCH-01 |
| F15 | DOCX was schema-invalid: `SectionProperties` was the first child of `w:body`, the schema requires it last. | Fixed |
| F16 | Modern template PDF lost the last characters of long lines: the grid was `38% 62%` plus a 10pt gap, wider than the printable area, so Chromium clipped it. Now `38% 1fr`. | Fixed |
| F17 | `POST /api/cv/regenerate` without `generatedCV` answered 200 with an empty document (the property defaulted to `new()`). Now 400. | Fixed |

### Open items after M1

Nothing in M1 is blocked. These were deliberately left to the milestone that owns them:

| Item | Why it is not in M1 | Owner |
|---|---|---|
| F6: a Minimalist CV in 1-page mode can spill onto a second page (18 of 36 combinations). Test `Minimalist_one_page_mode_stays_on_one_page` is written and skipped. | Fixing it properly means measuring the rendered page instead of estimating from character counts. | MATCH-08 (M4) |
| OPS-01 E2E job | Needs Playwright tests. | TEST-07 (M2) |
| Coverage report and 80% target | Needs coverlet. | TEST-10 (M5) |
| MATCH-01 "snapshot diff reviewed" | Reviewed by hand over the 36 corpus combinations (only junk skills such as `in` and `or` stopped matching); the automatic snapshots do not exist yet. | TEST-08 (M5) |
| TEST-06 (bUnit) and TEST-07 (Playwright) | Not started on purpose: the components and selectors are redesigned in M2. Both need package downloads that have to be approved first. | M5 / M2 |
| Parser: `objective-c` also yields the skill `c` (a hyphen is not treated as part of a token). | Minor and ambiguous (`go-to-market` would be the opposite case). | Backlog |

---

## E1. Test foundation

Goal: tests that tell whether the app *actually works*, not just whether endpoints answer. Built first so every later feature can be marked `Done` with proof.

### TEST-01: xUnit test project, fixtures and JD corpus
**Status:** Done · **Priority:** P0 · **Effort:** M

Why: there is no `dotnet test` today. Everything else in this epic builds on it.

- [x] `ErCv.Tests` project (xUnit) added to `er-cv-generator.sln`, referencing `ErCv.Web`
- [x] Profile fixtures: default `cv-master`, the senior profile (moved from `testing/cv-master-senior.json`), a junior profile, and a minimal/edge profile (no projects, odd date formats)
- [x] JD corpus in `ErCv.Tests/Fixtures/jds/`: backend Go/Kafka, frontend React, data engineer, DevOps, non-tech (sales), junior, empty, very long (50k chars), Spanish-language JD
- [x] `dotnet test` runs green locally

Files: `ErCv.Tests/*`, `er-cv-generator.sln`
Verify: `dotnet test` passes.

### TEST-02: Unit tests per helper
**Status:** Done · **Priority:** P0 · **Effort:** L

- [x] `JobDescriptionParser`: skill extraction, seniority (including negation like "no senior experience needed"), years-of-experience fallback, empty input
- [x] `SkillNormalizer`: every alias maps correctly; false positives from F1 are rejected
- [x] `RelevanceScorer`: recency tiers, seniority distance, skill cap at 40
- [x] `CVSelector`: tech roles prioritized for tech JDs, bullet order preserved, max experiences/bullets per mode
- [x] `PageConstraintHelper`: trimming order (bullets, then projects, then experiences, then skills)
- [x] `MatchScoreCalculator`, `MissingSkillsAnalyzer`, `KeywordAnalyzer`, `ATSChecker`: scores, thresholds, color classes, issue ordering
- [x] `SummaryGenerator`: role type detection, years of experience, 300-char cap

Notes: the tests for F1, F2 and F3 were written first and skipped with the ticket id; they are active now that `MATCH-01`, `MATCH-02` and `MATCH-03` landed. The parser findings found while writing them (F12) are fixed too, see "New findings".

Files: `ErCv.Tests/Helpers/*`
Verify: `dotnet test --filter Category=Unit`.

### TEST-03: Invariant ("truth") tests
**Status:** Done · **Priority:** P0 · **Effort:** M

Why: these prove the core promise of the product. Run for every JD in the corpus x every template x 1-page and 2-page.

- [x] Every output bullet exists verbatim in the master profile (no fabrication)
- [x] Every output skill, project and education entry exists in the master
- [x] No duplicate experiences in the output
- [x] Bullets keep their original relative order inside each role
- [x] Contact header is identical to the master
- [x] Match and ATS scores stay within 0 to 100
- [x] Master profile object is not mutated by generation (F5)

Note: the "no mutation" invariant passes today for generation itself. The stricter aliasing check (editing the generated CV must not touch the master) was skipped until ENG-02 and is active now.

Files: `ErCv.Tests/Invariants/*`
Verify: theory tests over the corpus pass.

### TEST-04: Document tests
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] DOCX: opens with OpenXML, sections appear in the expected order, contact line present, bullet count equals the generated CV
- [x] PDF: valid `%PDF` header, page count is 1 in 1-page mode and at most 2 in 2-page mode, for every corpus JD (catches F6)
- [x] PDF text extraction (ATS simulation): extracted text contains the name, every section title and every selected bullet, in reading order
- [x] HTML preview contains no unrendered Mustache tags (`{{`)

Notes: DOCX and HTML tests run over profiles x corpus JDs (x templates for HTML); PDF tests over profiles x JDs x 3 templates x 1/2 pages. PDF tests take about 1.5 minutes, use `--filter Category=Documents` to run or exclude them. The only skipped group is the Minimalist 1-page spill (F6, MATCH-08, M4). The Modern text extraction (F16) and the DOCX schema validation (F15) are active: both caught real bugs that are fixed. Chromium is downloaded on first use if `PUPPETEER_EXECUTABLE_PATH` is not set.

Files: `ErCv.Tests/Documents/*`
Verify: tests pass locally and in CI (Chromium available).

### TEST-05: API integration tests
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] `POST /api/cv/generate` returns 200 with files, scores and preview
- [x] `POST /api/cv/regenerate` respects bullet selections and order
- [x] `GET /api/cv/download/{format}` session round-trip; 404 when the session is empty
- [x] 400 for empty JD, missing formats, unsupported formats
- [x] Path-traversal `profileId` (`../../appsettings`) is rejected
- [x] 413 when session size limits are exceeded
- [x] 429 after the rate limit (60 per minute)
- [x] `GET /api/cv/health` and `GET /api/cv/profiles`

Notes: each test boots its own app (the rate limiter and session store are per app). API tests share the `Documents` collection with the PDF tests because the app closes the shared Chromium on shutdown. `Program.cs` gets `public partial class Program { }` so `WebApplicationFactory<Program>` can find the entry point. Nothing is skipped: the unknown or traversal `profileId` case (F8, ENG-03) and the missing `generatedCV` case (F17) were skipped until fixed.

Files: `ErCv.Tests/Api/*`
Verify: tests pass with `WebApplicationFactory<Program>`.

### TEST-06: Blazor component tests (bUnit)
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] BulletEditor: at least 1 bullet always selected, reorder updates selections, emitted selections map is correct
- [ ] Template, format and page selectors emit the right values and respect `Disabled`
- [ ] HistoryPanel: load, delete, clear
- [ ] Score cards render every color state and the low-match warning

Files: `ErCv.Tests/Components/*`
Verify: tests pass.

### TEST-07: End-to-end tests (Playwright)
**Status:** Todo · **Priority:** P1 · **Effort:** L

- [ ] Paste a sample JD, generate, preview visible, scores visible
- [ ] Download PDF and DOCX; files are non-empty and valid
- [ ] Edit bullets, regenerate, preview updates
- [ ] Reload: settings and history persist
- [ ] Keyboard shortcuts (`Ctrl+Enter`, `Ctrl+Shift+V`)
- [ ] Mobile viewport (390px): no horizontal scroll
- [ ] axe accessibility scan has no serious violations

Files: `ErCv.E2E/*`
Verify: `dotnet test ErCv.E2E` against a running app; runs in CI.

### TEST-08: Golden snapshot tests
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Approved `GeneratedCV` + scores JSON per corpus JD
- [ ] Scoring changes show up as a reviewable diff instead of silent drift

Verify: snapshot tests pass; intentional changes are re-approved.

### TEST-09: Retire the Node scripts
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Port the cases from `test-runner-2.0.1.js`, `bug-regression-2.0.2.js` and `security-hardening-2.0.3.js` into TEST-02/05/08
- [ ] Delete `testing/` and update README

Verify: every old TC-xx has a matching .NET test.

### TEST-10: Coverage
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] coverlet report generated in CI
- [ ] 80% or more line coverage on `Helpers/` and `Services/`
- [ ] Coverage badge in README

---

## E2. UI/UX redesign: Editorial / refined

Direction: a premium document tool. Warm paper background, serif display headings, clean sans UI, one confident accent. Think Notion/Linear meets a print magazine. Use the `frontend-design` skill when implementing.

**Design spec**

| Token | Value |
|---|---|
| Paper (page bg) | `#FAF8F4` |
| Surface (cards) | `#FFFFFF` |
| Ink (text) | `#1C1B1A` |
| Muted text | `#6B6760` |
| Line / borders | `#E7E2D9` |
| Accent (emerald) | `#0F766E`, hover `#115E59`, soft `#E6F2F0` |
| Success / Warning / Danger | `#15803D` / `#B45309` / `#B42318` |
| Display font | Fraunces (serif) |
| UI font | Inter |
| Numbers / scores | JetBrains Mono |

Fonts are self-hosted under `wwwroot/lib/fonts` (no runtime CDN, same rule as Bootstrap).

### UX-01: Design system
**Status:** Todo · **Priority:** P0 · **Effort:** M

- [ ] Replace the token block in `wwwroot/css/site.css` with the spec above (colors, type scale, 4px spacing grid, radii, soft shadows, focus ring)
- [ ] Bootstrap variables mapped to the new tokens (`--bs-primary`, `--bs-body-font-family`, ...)
- [ ] Fonts self-hosted (woff2) with `font-display: swap`
- [ ] Dark mode token set (`prefers-color-scheme` + manual toggle)
- [ ] Per-component CSS moved to `*.razor.css` (CSS isolation) where it helps

Files: `wwwroot/css/site.css`, `wwwroot/lib/fonts/*`, `Components/**/*.razor.css`
Verify: visual check in light and dark; contrast check with axe (UX-08).

### UX-02: App shell and editorial hero
**Status:** Todo · **Priority:** P0 · **Effort:** M

- [ ] Top bar: wordmark, "Try demo", GitHub link, theme toggle
- [ ] Hero: serif headline, one-line value prop ("Tailored to every job. Nothing invented."), short 3-point "how it works"
- [ ] Footer with privacy note and version

Files: `Components/Layout/MainLayout.razor`, `Components/Shared/AppHeader.razor`
Verify: E2E screenshot; first view looks finished on desktop and mobile.

### UX-03: Guided 3-step composer
**Status:** Todo · **Priority:** P0 · **Effort:** M

- [ ] Step 1 Job description: large editor, sample JD picker (APP-03), live counter of characters and detected skills
- [ ] Step 2 Profile & template: big template cards with real thumbnails
- [ ] Step 3 Options: formats, 1 or 2 pages
- [ ] One primary CTA; shortcuts still work

Files: `Components/Pages/Home.razor`, `Components/Shared/*Selector.razor`, `JobDescriptionInput.razor`
Verify: E2E generate flow (TEST-07).

### UX-04: Results workspace
**Status:** Todo · **Priority:** P0 · **Effort:** L

- [ ] Left: preview at paper scale with zoom (fit width / 100%)
- [ ] Right: tabbed insights (Match, ATS, Keywords, Gaps)
- [ ] Sticky action bar: Download PDF, Download DOCX, Edit, Start over
- [ ] Generate no longer auto-downloads (F7); downloads happen on click
- [ ] Back to composer keeps the JD and settings

Files: `Components/Pages/Home.razor`, new `Components/Shared/ResultsWorkspace.razor`
Verify: E2E: generate without download prompt, then click to download.

### UX-05: Score visualizations
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] Radial gauges for Match and ATS (inline SVG, no chart library)
- [ ] Stacked breakdown bar for skills / keywords / seniority
- [ ] Missing skills as a chip cloud, required vs nice-to-have
- [ ] Count-up animation, disabled under `prefers-reduced-motion`

Files: `MatchScoreCard.razor`, `ATSScoreCard.razor`, `MissingSkillsCard.razor`, `KeywordAnalysisCard.razor`
Verify: bUnit render tests for each color state (TEST-06).

### UX-06: Generation progress
**Status:** Todo · **Priority:** P1 · **Effort:** S

- [ ] Step indicator: Parsing, Scoring, Selecting, Rendering (reported from the service via `IProgress<T>`)
- [ ] Skeleton placeholders for preview and cards

Verify: manual check plus E2E that the indicator appears and disappears.

### UX-07: Empty, error and toast states
**Status:** Todo · **Priority:** P1 · **Effort:** S

- [ ] Inline validation on the JD field instead of an alert box
- [ ] Toasts for success (download started, regenerated) and errors with a retry action
- [ ] Friendly empty states with a call to action

Verify: E2E for empty JD and no format selected.

### UX-08: Accessibility
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] WCAG AA contrast for all tokens in light and dark
- [ ] BulletEditor: move up/down buttons and `Alt+Arrow` keyboard reorder (F11)
- [ ] `aria-live` region announces generation results
- [ ] Visible focus on every interactive element; selectable cards are real buttons/radios
- [ ] axe scan in E2E has no serious violations

Verify: TEST-07 axe check + keyboard-only manual pass.

### UX-09: Responsive pass
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] Composer usable at 360px width
- [ ] Workspace stacks on mobile; preview becomes a tab
- [ ] No horizontal scroll at any breakpoint

Verify: E2E at 390px and 768px.

### UX-10: CV template polish
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] Shared typographic rhythm across Harvard, Modern and Minimalist
- [ ] Optional accent color and font pairing per template (kept ATS-safe)
- [ ] Real thumbnails generated from each template for the template picker

Files: `Templates/*.html`, `TemplateSelector.razor`
Verify: TEST-04 still passes for every template.

---

## E3. Bring your own profile

Why: today the only usable profile is the demo JSON in the repo. Letting visitors use their own data turns the project from a demo into a real tool.

### PROF-01: In-browser profile editor
**Status:** Todo · **Priority:** P1 · **Effort:** L

- [ ] Form-based editor: header, education, experience (bullets + skills tags), skills by category, projects
- [ ] Saved in localStorage only; never stored on the server
- [ ] Profile switcher: "Demo profile" and "My profile"

Files: new `Components/Pages/Profile.razor`, `UI/BrowserStore.cs`
Verify: bUnit tests for the editor; E2E create profile, generate, output uses it.

### PROF-02: Import/export + validation
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] Import JSON file; export current profile as JSON
- [ ] Validation with field-level errors (required fields, date formats, empty bullets)
- [ ] Downloadable blank template

Verify: unit tests for the validator; E2E import round-trip.

### PROF-03: Generate from a custom profile
**Status:** Todo · **Priority:** P1 · **Effort:** S

- [ ] `CVGeneratorService` overload that takes a `CVMaster` instead of a `profileId`
- [ ] Demo profile stays the default

Verify: TEST-03 invariants run against a custom profile.

### PROF-04: Profile health check
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] Flags bullets without metrics, weak opening verbs, experiences without skill tags, overlapping or invalid dates
- [ ] Shown as a checklist before generating

Verify: unit tests per rule.

### PROF-05: Import from an existing resume
**Status:** Todo · **Priority:** P3 · **Effort:** L

- [ ] Upload PDF/DOCX, heuristic section parsing into a draft profile the user reviews
- [ ] AI-assisted parsing when E7 is enabled

Verify: tests with sample resumes.

---

## E4. Matching quality and transparency

### MATCH-01: Fix fuzzy-match false positives
**Status:** Done · **Priority:** P0 · **Effort:** S

- [x] Replace substring `Contains` in `SkillNormalizer.FuzzyMatch` with alias equality + token-boundary matching (F1)
- [x] Regression tests: `go` vs `google`/`mongodb`, `java` vs `javascript`, `react` vs `react native`, `c` vs `c#`

Notes: `FuzzyMatch` now matches whole tokens (`rest` matches `REST API Design`, `go` does not match `google`), with `react native` kept distinct from `react`. New shared `SkillNormalizer.ContainsTerm` (token matcher, treats `#` and `+` as part of the token) and `ContainsSkill` (same, also accepting aliases such as `golang` for `go`). The old `Skip` tests for F1 are now active. Corpus impact reviewed by hand: the only matches that disappear are junk skills from F12 (`in`, `or`) that matched by substring.

Files: `Helpers/SkillNormalizer.cs`
Verify: TEST-02 SkillNormalizer tests; TEST-08 snapshot diff reviewed.

### MATCH-02: Word-boundary ATS density
**Status:** Done · **Priority:** P0 · **Effort:** S

- [x] `ATSChecker.EvaluateKeywordDensity` uses the shared word-boundary matcher (F2)

Verify: regression test with "good" not counting as `go`.

### MATCH-03: Shared date parser
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] `DateParser` handles `Jun 2024`, `June 2024`, `2024-06`, `06/2024`, `2024`, `Present`/`Current`/`Now`
- [x] "Most recent role" uses parsed dates (F3)
- [x] Years of experience computed from merged date ranges (gaps not counted)

Notes: months without a month name count as January for a start date and December for an end date; a missing or unreadable end date means ongoing; a role with no readable start is ignored for years. Years are floored whole years from merged ranges, so the demo profile summary now says `9+ years` instead of `10+` (there is a gap between the 2016 internship and 2017).

Files: new `Helpers/DateParser.cs`, `MatchScoreCalculator.cs`, `SummaryGenerator.cs`, `RelevanceScorer.cs`
Verify: unit tests per format; regression test for `May 2020` vs `Dec 2023`.

### MATCH-04: Explainability
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] Each selected experience and bullet exposes a score breakdown (matched skills, keywords, recency, metric bonus, verb bonus)
- [ ] Workspace shows "Why this was chosen" on hover/expand

Verify: unit test that breakdown parts sum to the score.

### MATCH-05: JD highlighter
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] JD rendered with matched skills in green and missing ones in red
- [ ] Click a highlighted skill to see where it appears in the CV

Verify: bUnit test for highlighting; HTML-encoded output (no XSS).

### MATCH-06: Required vs nice-to-have from JD sections
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] Parser detects sections ("Requirements", "Must have", "Nice to have", "Bonus", "Preferred")
- [ ] Missing-skills split uses the JD section instead of a fixed list

Verify: unit tests with corpus JDs.

### MATCH-07: Skill dictionary as data
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Aliases and tech keywords moved to `Data/skills.json`
- [ ] One test per alias

Verify: tests pass; adding an alias requires no code change.

### MATCH-08: Page fit measured on the rendered PDF
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] After rendering, count PDF pages (or measure DOM height via Puppeteer); trim and re-render until it fits (F6)
- [ ] Character estimate kept only as a first pass

Verify: TEST-04 page count assertion over the whole corpus.

---

## E5. Editing and output

### EDIT-01: Inline editing
**Status:** Todo · **Priority:** P2 · **Effort:** L

- [ ] Edit bullet text in the workspace; edited bullets are flagged as user-edited
- [ ] Toggle summary, projects and individual skills
- [ ] Reorder sections

Verify: bUnit + E2E.

### EDIT-02: Live HTML preview
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] Preview updates from HTML only while editing
- [ ] PDF/DOCX rendered on download, not on every change

Verify: E2E: edit updates preview without a render delay.

### EDIT-03: Cover letter generator
**Status:** Todo · **Priority:** P2 · **Effort:** M

Moved from the legacy FEATURE-301.

- [ ] "Also generate cover letter" option, same JD and profile
- [ ] Structure: opening, two experience paragraphs, closing
- [ ] Template-driven (Mustache), uses only CV data
- [ ] PDF and DOCX

Files: new `Helpers/CoverLetterGenerator.cs`, `Templates/cover-letter-template.html`
Verify: invariant test that every fact in the letter comes from the profile.

### EDIT-04: CV output language
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] English or Spanish section headings, dates and summary phrasing

Verify: document tests for both languages.

### EDIT-05: More templates
**Status:** Todo · **Priority:** P3 · **Effort:** M

- [ ] "Compact" and "Executive" templates on a shared partial

Verify: TEST-04 across new templates.

### EDIT-06: Plain-text / Markdown export
**Status:** Todo · **Priority:** P3 · **Effort:** S

- [ ] Copy-to-clipboard and `.md`/`.txt` download for ATS paste-in fields

Verify: unit test of the text renderer.

---

## E6. Application workflow

### APP-01: Application tracker
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] History entries gain company, role, status (Saved, Applied, Interview, Offer, Rejected), notes
- [ ] Filter and sort by status/date; stays in localStorage

Verify: bUnit + E2E.

### APP-02: Re-open a past generation
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] History stores template, formats, page mode and profile; "Re-open" restores and regenerates

Verify: E2E.

### APP-03: Sample JD gallery
**Status:** Todo · **Priority:** P0 · **Effort:** S

Why: portfolio visitors rarely have a JD at hand.

- [ ] 5 to 6 curated JDs (backend, frontend, full stack, data, DevOps, junior)
- [ ] One click fills the composer

Verify: E2E: pick sample, generate.

### APP-04: Import JD from URL
**Status:** Todo · **Priority:** P3 · **Effort:** M

- [ ] Greenhouse and Lever public job APIs only, allowlisted hosts (SSRF-safe), timeout and size limit

Verify: integration tests with mocked HTTP; disallowed hosts rejected.

---

## E7. AI assist (optional, off by default)

The app must work fully without an API key. AI only picks or suggests; it never writes content into the CV without the user accepting it.

### AI-01: LLM options + feature flag
**Status:** Parked · **Priority:** P3 · **Effort:** S

- [ ] `LLM` options section (`Enabled`, `Model`, timeouts); key from env var / user-secrets only
- [ ] UI hides AI toggles when no key is configured

### AI-02: Semantic matching
**Status:** Parked · **Priority:** P3 · **Effort:** M

Moved from the legacy FEATURE-203.

- [ ] LLM returns experience and bullet *indices* only (structured output)
- [ ] Automatic fallback to the heuristic scorer on error, timeout or invalid output
- [ ] Default to the current Haiku model (check the latest model ID at implementation time)

Verify: tests with a fake client; TEST-03 invariants still hold.

### AI-03: Bullet rewrite suggestions
**Status:** Parked · **Priority:** P3 · **Effort:** M

- [ ] Suggestions shown as a diff; user accepts or rejects each one
- [ ] Accepted rewrites are flagged as user-edited

### AI-04: Cover letter polish
**Status:** Parked · **Priority:** P3 · **Effort:** S

- [ ] Optional polish pass on top of EDIT-03, with a diff view

### AI-05: Cost guardrails
**Status:** Parked · **Priority:** P3 · **Effort:** S

- [ ] Per-session request cap, token usage logging, cache keyed by hash of JD + profile

---

## E8. Engineering quality

### ENG-01: DI + single namespace root
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] Helpers registered in DI; interfaces only where tests need seams
- [ ] All code under `ErCv.Web.*` (F9)

Verify: build + all tests pass.

### ENG-02: Consolidate duplicated logic
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] One shared term matcher (replaces 3 `ContainsTerm` copies)
- [x] One seniority map; one year/date parser (with MATCH-03) (F4)
- [x] Selection builds new objects; master profile is never mutated; page constraint returns a new CV (F5)

Notes: new `Helpers/CvCloner.cs` (deep copies). `CVSelector`, `PageConstraintHelper.EnforcePageConstraint` and `CVGeneratorService.RegenerateCVInternalAsync` now work on copies, so the clone workaround in `Home.razor` is gone. Shared matcher is `SkillNormalizer.ContainsTerm`; shared seniority map is `MatchScoreConstants.SeniorityMap` (now also has `associate`); dates and years go through `DateParser` (MATCH-03). The TEST-03 aliasing test is active.

Verify: TEST-03 "master not mutated" invariant.

### ENG-03: Logging and errors
**Status:** Done · **Priority:** P0 · **Effort:** S

- [x] `ILogger<T>` replaces `Console.WriteLine`
- [x] ProblemDetails responses; no exception text in 500 bodies (F8)

Notes: `CVGeneratorService` takes `ILoggerFactory` and hands loggers to `CVSelector`, `RelevanceScorer` and `SummaryGenerator` (optional parameters, so `new` still works until ENG-01 moves them to DI). Every error is an RFC 7807 `application/problem+json` body; the `error` extension keeps the field existing consumers read. An unknown or traversal `profileId` is now a 400 "Profile not found" instead of a 500.

Verify: TEST-05 asserts 500 bodies contain no exception details.

### ENG-04: Options pattern for weights
**Status:** Todo · **Priority:** P3 · **Effort:** S

- [ ] Scoring weights and limits configurable via `appsettings.json`

### ENG-05: Chromium readiness + warm-up
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Health check reports PDF engine readiness
- [ ] Browser launched at startup in the background to cut cold-start time on Render

Verify: TEST-05 health response; manual cold-start timing.

### ENG-06: Modern OpenAPI
**Status:** Todo · **Priority:** P3 · **Effort:** S

- [ ] Replace Swashbuckle 6.5 with built-in `MapOpenApi` + Scalar UI (or update Swashbuckle)

---

## E9. DevOps and portfolio presentation

### OPS-01: GitHub Actions CI
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] On push and PR: restore, build, unit + integration tests (coverage arrives with TEST-10)
- [x] Docker image build
- [ ] E2E job (Playwright), arrives with TEST-07
- [x] Status badge in README

Notes: `.github/workflows/ci.yml` runs the same commands that pass locally (restore, build and test in Release). PDF tests use the Chrome preinstalled on the runner through `PUPPETEER_EXECUTABLE_PATH`. Verified on PR #7: build and test (3389 passed, 6 skipped, about 2 minutes) and the Docker image build both green.

Verify: green run on a PR.

### OPS-02: Dependabot
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] NuGet, GitHub Actions and Docker ecosystems

### OPS-03: README rewrite
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] Hero screenshot / GIF, live demo link, badges (CI, coverage, .NET 10)
- [ ] Architecture diagram, "How matching works", testing strategy section

### OPS-04: PROJECT_OVERVIEW refresh
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Fix the drift (F10) and document the new features

### OPS-05: Privacy note
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Page or footer note: no CV data stored server-side; session files expire after 10 minutes; custom profiles live only in the browser

---

## Legacy (v2.0.x, from BACKLOG.md)

Shipped before this file existed. Kept for history.

| ID | Description | Status |
|---|---|---|
| FEATURE-101 | Fuzzy skill matching: `SkillNormalizer.cs` centralizes all matching | Done (legacy) |
| FEATURE-102 | CV preview HTML: `RenderHtmlPublic()` exposed, `previewHtml` in the response | Done (legacy) |
| FEATURE-102-FE | Inline CV preview in the UI (`CVPreview` + iframe `srcdoc`) | Done (legacy) |
| FEATURE-103 | Keyboard shortcuts: `Ctrl/Cmd+Enter` generate, `Ctrl/Cmd+Shift+V` paste | Done (legacy) |
| FEATURE-201 | Generation history (localStorage, top 10, reload JD, clear history) | Done (legacy) |
| FEATURE-202 | Multi CV master: `GET /api/cv/profiles`, `ProfileId` in `GenerateRequest` | Done (legacy) |
| FEATURE-202-FE | Profile selector in the UI sending `profileId` | Done (legacy) |
| FEATURE-302 | Page constraint fix: character-based estimate replaces fixed line counts | Done (legacy) |
| FEATURE-303 | Export/template/page settings persisted in localStorage | Done (legacy) |
| FIX-001 | `KeywordAnalyzer` uses `GeneratedCV` instead of `CVMaster` | Done (legacy) |
| FIX-002 | Role mismatch warning: `LowMatchWarning` on low overall score (code uses below 40%, see F10) | Done (legacy) |
| FIX-003 | Better bullet scoring: action-verb bonus, short-bullet penalty | Done (legacy) |
| FIX-004 | `SkillNormalizer` replaces scattered string comparisons | Done (legacy) |
| FIX-005 | Bullet reorder persists correctly on `/regenerate` | Done (legacy) |
| FIX-006 | Strict format validation (`pdf`, `docx`) with `400 BadRequest` | Done (legacy) |
| FIX-007 | Robust year parsing in scoring and summary, with warning logs | Done (legacy) |
| FIX-008 | Frontend `API_BASE_URL` fallback + clear connectivity message (pre-Blazor) | Done (legacy) |
| FIX-009 | Word-boundary matching to avoid false positives | Done (legacy) |
| FIX-010 | Session file size limits + controlled 413 response | Done (legacy) |
| FIX-011 | Initialized models to remove CS8618 warnings (clean nullable build) | Done (legacy) |
| FIX-012 | Security baseline: rate limiting, HTTPS redirection outside dev, explicit access posture | Done (legacy) |
| PERF-001 | Faster PDF rendering: shared lazy-init browser, page closed after each render | Done (legacy) |
| TEST-001 | Node test runner configurable by profile (`--profile` or `PROFILE_ID`) | Done (legacy) |
| ENH-001 | Preview refresh on `/regenerate` with updated `previewHtml` | Done (legacy) |
| FEATURE-301 | Cover letter generator | Moved to EDIT-03 |
| FEATURE-203 | LLM integration (Claude API) | Moved to AI-01 / AI-02 |

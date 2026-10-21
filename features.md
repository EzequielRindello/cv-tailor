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
| TEST-06 | Blazor component tests (bUnit) | E1 Tests | P1 | M | In progress |
| TEST-07 | End-to-end tests (Playwright) | E1 Tests | P1 | L | Done |
| TEST-08 | Golden snapshot tests for generation output | E1 Tests | P2 | S | Todo |
| TEST-09 | Retire the Node scripts in `testing/` | E1 Tests | P2 | S | Todo |
| TEST-10 | Coverage report and target | E1 Tests | P2 | S | Todo |
| UX-01 | Design system: tokens, fonts, dark mode | E2 Redesign | P0 | M | Done |
| UX-02 | New app shell and editorial hero | E2 Redesign | P0 | M | Done |
| UX-03 | Guided 3-step composer | E2 Redesign | P0 | M | Done |
| UX-04 | Results workspace (no auto-download) | E2 Redesign | P0 | L | Done |
| UX-05 | Score visualizations | E2 Redesign | P1 | M | Done |
| UX-06 | Generation progress and skeletons | E2 Redesign | P1 | S | Done |
| UX-07 | Empty, error and toast states | E2 Redesign | P1 | S | Done |
| UX-08 | Accessibility (WCAG AA, keyboard reorder) | E2 Redesign | P1 | M | Done |
| UX-09 | Responsive pass | E2 Redesign | P1 | M | Done |
| UX-10 | CV template visual polish | E2 Redesign | P2 | M | Todo |
| PROF-01 | In-browser profile editor | E3 Your profile | P1 | L | Done |
| PROF-02 | Profile JSON import/export + validation | E3 Your profile | P1 | M | Done |
| PROF-03 | Generate from a custom profile | E3 Your profile | P1 | S | Done |
| PROF-04 | Profile health check | E3 Your profile | P2 | M | Done |
| PROF-06 | The owner's real profile as the demo | E3 Your profile | P1 | S | Todo |
| PROF-05 | Import from existing PDF/DOCX resume | E3 Your profile | P3 | L | Todo |
| MATCH-01 | Fix fuzzy-match false positives | E4 Matching | P0 | S | Done |
| MATCH-02 | Word-boundary ATS keyword density | E4 Matching | P0 | S | Done |
| MATCH-03 | Shared date parser + correct "most recent role" | E4 Matching | P0 | M | Done |
| MATCH-04 | Explainability: "why this was chosen" | E4 Matching | P1 | M | Done |
| MATCH-05 | JD highlighter | E4 Matching | P1 | M | Done |
| MATCH-06 | Required vs nice-to-have from JD sections | E4 Matching | P2 | M | Todo |
| MATCH-07 | Skill/alias dictionary as data file, tuned on real posts | E4 Matching | P1 | M | Done |
| MATCH-08 | Page fit measured on the rendered PDF | E4 Matching | P1 | M | Todo |
| MATCH-09 | Seniority detection from the title and wording | E4 Matching | P1 | S | Done |
| EDIT-01 | Inline editing in the workspace | E5 Editing | P2 | L | Todo |
| EDIT-02 | Live HTML preview, documents on download | E5 Editing | P2 | M | Todo |
| EDIT-03 | Cover letter generator | E5 Editing | P2 | M | Todo |
| EDIT-04 | CV output language (EN/ES) | E5 Editing | P2 | M | Todo |
| EDIT-05 | More templates | E5 Editing | P3 | M | Todo |
| EDIT-06 | Plain-text / Markdown export | E5 Editing | P3 | S | Todo |
| APP-01 | Application tracker | E6 Workflow | P2 | M | Done |
| APP-02 | Re-open a past generation | E6 Workflow | P2 | S | Done |
| APP-03 | Sample JD gallery | E6 Workflow | P0 | S | Done |
| APP-04 | Import JD from URL (Greenhouse/Lever) | E6 Workflow | P3 | M | Todo |
| APP-05 | Real job-post corpus, dated and kept fresh | E6 Workflow | P1 | L | Done |
| APP-06 | Corpus follow-ups: sample picker rules, first Action run | E6 Workflow | P2 | S | In progress |
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

1. **M1 Foundation** (v2.0.2, released): TEST-01, TEST-02, TEST-03, TEST-04, TEST-05, MATCH-01, MATCH-02, MATCH-03, ENG-02, ENG-03, OPS-01. Fixes ship with their tests from day one.
2. **M2 Redesign** (v2.0.3, released): UX-01 to UX-09, APP-03, TEST-07 (E2E written against the new UI).
3. **M3 Your data** (v2.0.4, released): PROF-01 to PROF-04, MATCH-04, MATCH-05, APP-01, APP-02, APP-05.
3.2. **M3.2 Real-post fixes** (v2.0.4.1, before M4): MATCH-07, MATCH-09, APP-06, PROF-06. Fixes what the real-post report found (F18 to F21) so the parser and the demo match what employers write.
4. **M4 Output** (v2.0.5): EDIT-01 to EDIT-06, MATCH-08, UX-10.
5. **M5 Polish & presentation** (v2.0.6): OPS-02 to OPS-05, ENG-01, ENG-04, ENG-05, ENG-06, TEST-06, TEST-08, TEST-09, TEST-10.
6. **M6 Optional AI** (v2.0.7): AI-01 to AI-05.

### Versioning

One milestone is one version: `2.0.<N+1>` for milestone `M<N>`. The version before the roadmap was 2.0.1. A milestone added between two others takes a fourth number so nothing is renumbered (M3.2 is `2.0.4.1`, its branch `feature/m3-2-real-post-fixes`).

**PR names.** Branch `feature/m<N>-<short-name>` (for example `feature/m3-your-data`). PR title `v2.0.<N+1>: M<N> <name>` (for example `v2.0.4: M3 Your data`). A fix to something already released goes in its own small PR titled `fix: <what>` and does not change the version.

**When a milestone is ready to merge**, the same PR also:
1. Sets `<Version>` in `ErCv.Web/ErCv.Web.csproj` (the app footer shows it).
2. Updates "Current version" in `README.md`.
3. Marks the milestone as `released` in the list above and checks that every item in it is `Done` in the dashboard.
4. Updates the "Open items after" section: what was left to a later milestone, and which one owns it.

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
| F9 | Mixed namespaces (`ErCvApi.*` and `ErCv.Web.*`); helpers built with `new` instead of DI. | Whole project | Namespaces fixed in the system hardening (H8); DI is ENG-01 |
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

### New findings (from APP-05, real posts)

Found by the relevance report over 194 real posts (`corpus/report.md`). Not fixed in APP-05: they change scoring, so they belong to the matching work.

| # | Finding | Status |
|---|---|---|
| F18 | Dictionary entries that are ordinary English match prose: the alias `next` (Next.js) hits 40% of posts ("the next generation"), plus `end-to-end`, `foundation`, `less`, `solid`, `rails`. | Fixed with MATCH-07 (next.js in 77 posts before, 3 after) |
| F19 | `YearsWithTechPattern` still takes the word after "N years of" as a skill when it is not one: `professional`, `building`, `industry`, `hands-on`. | Fixed with MATCH-07 (only dictionary words count) |
| F20 | Seniority: "Staff ..." titles are not recognized (only the phrase "staff engineer" is), "II", "internship" and "early career" are not keywords, and the first level word in the text wins ("lead the design" reads as lead). The parser agrees with the title level in 46% of posts. | Fixed with MATCH-09 (91% agreement) |
| F21 | The single-letter skill `c` matches "Washington D.C." in pay-range text. | Fixed with MATCH-07 |
| F22 | `KeywordAnalyzer` keeps its own `TechWords` list, apart from the dictionary, and classifies single JD words with it: "next" (as in "the next generation") is still listed as a technical keyword in the keywords tab, because the alias `next` still normalizes to Next.js. | Fixed in M3.2 (MATCH-07 follow-up): technical keywords are the dictionary skills the job description names (phrases such as "machine learning" included), `KeywordAnalyzerConstants.TechWords` is gone, and the words of a skill are no longer repeated as industry keywords |

### System hardening (between M3 and M3.2)

A review of the C# code before M3.2, branch `feature/system-hardening`. No behavior change: every existing test passes (tests were only moved and renamed to the new namespaces), and the corpus report regenerates byte for byte. No version bump (not a milestone). The conventions it set are in `CLAUDE.md`.

| # | Finding | Status |
|---|---|---|
| H1 | Performance: every render read the HTML template from disk and rebuilt the Stubble renderer, and one generation rendered the HTML twice (PDF and preview). | Fixed: templates cached, one renderer, HTML rendered once |
| H2 | Performance: `KeywordAnalyzer` copied the tech word set into a new list for every JD word, and matched each word against the CV twice. | Fixed |
| H3 | Performance: the seniority check built a new `Regex` per keyword on every parse (21 patterns, more than the regex cache holds); parser lookup sets were rebuilt per instance. | Fixed: patterns and sets built once |
| H4 | Performance: `JsonSerializerOptions` and lookup dictionaries created per call; fixed regexes mixed three styles. | Fixed: static, all fixed patterns are `[GeneratedRegex]` |
| H5 | Security: no server-side length limit on the job description (API or UI), and a 30 MB default request body. | Fixed: 100,000 characters (`ApiConstants`), 2 MB body |
| H6 | Security: the UI calls the service directly, so it bypasses the rate limiter, and each generation opened a Chrome page with no cap. | Fixed: at most 2 PDF renders at a time |
| H7 | Security: no security headers or HSTS; `UseExceptionHandler("/Error")` pointed at a page that did not exist; the container ran as root with Chrome `--no-sandbox`. | Fixed: headers, HSTS, error page, non-root user |
| H8 | Structure: the whole domain sat in `Helpers/`, constants in `Data/Constants/Helpers/`, namespaces did not match folders. | Fixed: `Features/<Feature>/`, namespaces = folders, file-scoped |
| H9 | Magic numbers and repeated literals (scoring weights, limits, profile ids, JS interop names, template keys) inline across helpers and components. | Fixed: named constants, hosting values in `appsettings.json` |
| H10 | `CVSelector` and `SummaryGenerator` keep two slightly different "tech role" word lists. | Fixed with MATCH-07: one list, `SelectionConstants.TechRoleKeywords` (the union, so the summary now also counts analyst, consultant and specialist roles as tech years) |
| H11 | Swashbuckle 6.5.0 is old (Development only). | Open: ENG-06 (M5) replaces it |

### Open items after M3

Nothing in M3 is blocked. Left to M3.2 (v2.0.4.1, before M4):

| Item | Why it is not in M3 | Owner |
|---|---|---|
| F18, F19, F21: dictionary words that match ordinary English, junk after "N years of", `c` in "D.C." | They change scoring, so they need their own before/after review. | MATCH-07 (M3.2) |
| F20: seniority wording ("Staff", "II", internship) and 46% agreement with the title | Same reason. | MATCH-09 (M3.2) |
| Sample picker picks an accelerator post for Full stack | Needs a rule change, reviewed on its own. | APP-06 (M3.2) |
| Corpus refresh workflow never ran on GitHub | Needs the repo setting and a first look at the PR it opens. | APP-06 (M3.2) |
| TEST-08 snapshots over `FixtureLoader.CorpusJdNames` | Snapshots are not built yet. | TEST-08 (M5) |

### Open items after M1

Nothing in M1 is blocked. These were deliberately left to the milestone that owns them:

| Item | Why it is not in M1 | Owner |
|---|---|---|
| F6: a Minimalist CV in 1-page mode can spill onto a second page (18 of 36 combinations). Test `Minimalist_one_page_mode_stays_on_one_page` is written and skipped. | Fixing it properly means measuring the rendered page instead of estimating from character counts. | MATCH-08 (M4) |
| Coverage report and 80% target | Needs coverlet. | TEST-10 (M5) |
| MATCH-01 "snapshot diff reviewed" | Reviewed by hand over the 36 corpus combinations (only junk skills such as `in` and `or` stopped matching); the automatic snapshots do not exist yet. | TEST-08 (M5) |
| TEST-06 (bUnit) | Not started on purpose: the components were redesigned in M2 first. Needs the `bunit` package, which has to be approved before it is downloaded. | M5 |
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
**Status:** In progress (`bunit` 2.12.5 added in M3 with the JD highlighter tests; the checklist below is still to do, M5) · **Priority:** P1 · **Effort:** M

- [ ] BulletEditor: at least 1 bullet always selected, reorder updates selections, emitted selections map is correct
- [ ] Template, format and page selectors emit the right values and respect `Disabled`
- [ ] HistoryPanel: load, delete, clear
- [ ] Score cards render every color state and the low-match warning

Files: `ErCv.Tests/Components/*`
Verify: tests pass.

### TEST-07: End-to-end tests (Playwright)
**Status:** Done (26 tests passing locally; the CI job has not run on GitHub yet) · **Priority:** P1 · **Effort:** L

- [x] Paste a sample JD, generate, preview visible, scores visible
- [x] Download PDF and DOCX; files are non-empty and valid (PDF header, DOCX is a zip with `word/document.xml`)
- [x] Edit bullets, regenerate, preview updates
- [x] Reload: settings and history persist
- [x] Keyboard shortcuts (`Ctrl+Enter`, `Ctrl+Shift+V`)
- [x] Mobile viewport (390px, also 360 and 768): no horizontal scroll, one pane at a time
- [x] axe accessibility scan has no serious violations (wcag2a, wcag2aa, wcag21a, wcag21aa; light and dark; CV preview iframe excluded)

Also covered: nothing downloads until a button is clicked (F7), the progress stages appear in order, inline validation for an empty JD and for no format, the empty history state, focus and announcement after generating.

Files: `ErCv.E2E/*` (`AppFixture` starts the app on a free port and launches an installed Chrome or Edge, so no browser is downloaded)
Verify: `dotnet test ErCv.E2E`; runs in CI as its own job.

### TEST-08: Golden snapshot tests
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Approved `GeneratedCV` + scores JSON per corpus JD
- [ ] Scoring changes show up as a reviewable diff instead of silent drift
- [ ] Snapshots also cover the real posts from the APP-05 corpus (`FixtureLoader.CorpusJdNames`)

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
**Status:** Done (CSS isolation deliberately not done) · **Priority:** P0 · **Effort:** M

- [x] Replace the token block in `wwwroot/css/site.css` with the spec above (colors, type scale, 4px spacing grid, radii, soft shadows, focus ring)
- [x] Bootstrap variables mapped to the new tokens (`--bs-primary`, `--bs-body-font-family`, ...)
- [x] Fonts self-hosted (woff2) with `font-display: swap`
- [x] Dark mode token set (`prefers-color-scheme` + manual toggle)
- [x] Per-component CSS: kept in `site.css`, organised by component sections, because the tokens are shared everywhere; `*.razor.css` did not help

Files: `wwwroot/css/site.css`, `wwwroot/lib/fonts/*`, `Components/**/*.razor.css`
Verify: visual check in light and dark; contrast check with axe (UX-08, `AccessibilityTests`).

### UX-02: App shell and editorial hero
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] Top bar: wordmark, "Try demo" (scrolls to the sample picker, only on the composer), GitHub link, theme toggle
- [x] Hero: serif headline, one-line value prop ("Tailored to every job. Nothing invented."), short 3-point "how it works"
- [x] Footer with privacy note and version

Files: `Components/Layout/MainLayout.razor`, `Components/Shared/AppHeader.razor`
Verify: E2E screenshot; first view looks finished on desktop and mobile.

### UX-03: Guided 3-step composer
**Status:** Done (real thumbnails move to UX-10) · **Priority:** P0 · **Effort:** M

- [x] Step 1 Job description: large editor, sample JD picker (APP-03), live counter of characters and detected skills
- [x] Step 2 Profile & template: template cards (the existing SVG sketches; real thumbnails arrive with UX-10)
- [x] Step 3 Options: formats, 1 or 2 pages
- [x] One primary CTA (Next, then Generate on step 3); shortcuts still work from any step

Files: `Components/Pages/Home.razor`, `Components/Shared/*Selector.razor`, `JobDescriptionInput.razor`
Verify: E2E generate flow (TEST-07).

### UX-04: Results workspace
**Status:** Done · **Priority:** P0 · **Effort:** L

- [x] Left: preview at paper scale with zoom (fit width / 100%)
- [x] Right: tabbed insights (Match, ATS, Keywords, Gaps)
- [x] Sticky action bar: Download PDF, Download DOCX, Edit, Start over (one Download button per generated format; both formats are downloaded and validated in `DownloadTests`)
- [x] Generate no longer auto-downloads (F7); downloads happen on click (also after Regenerate)
- [x] Back to composer keeps the JD and settings

Files: `Components/Pages/Home.razor`, new `Components/Shared/ResultsWorkspace.razor`
Verify: E2E: generate without download prompt, then click to download.

### UX-05: Score visualizations
**Status:** Done (bUnit render tests wait for TEST-06) · **Priority:** P1 · **Effort:** M

- [x] Radial gauges for Match and ATS (inline SVG, no chart library): `ScoreGauge.razor`
- [x] Stacked breakdown bar for skills / keywords / seniority (segment width = weight, fill = points earned)
- [x] Missing skills as a chip cloud, required vs nice-to-have (solid vs outline, with a legend)
- [x] Count-up animation, disabled under `prefers-reduced-motion`

Files: `MatchScoreCard.razor`, `ATSScoreCard.razor`, `MissingSkillsCard.razor`, `KeywordAnalysisCard.razor`
Verify: bUnit render tests for each color state (TEST-06).

### UX-06: Generation progress
**Status:** Done (E2E check arrives with TEST-07) · **Priority:** P1 · **Effort:** S

- [x] Step indicator: Parsing, Scoring, Selecting, Rendering (reported from the service via `IProgress<GenerationStage>`, covered by `GenerationProgressTests`)
- [x] Skeleton placeholders for preview and cards (Generate now opens the workspace straight away; an error sends the user back to the composer)

Verify: manual check plus E2E that the indicator appears and disappears.

### UX-07: Empty, error and toast states
**Status:** Done (E2E arrives with TEST-07) · **Priority:** P1 · **Effort:** S

- [x] Inline validation on the JD field (and on the formats) instead of an alert box
- [x] Toasts for success (download started, regenerated) and errors with a retry action (`ToastService` + `ToastHost`)
- [x] Friendly empty states with a call to action (history, results)

Verify: E2E for empty JD and no format selected.

### UX-08: Accessibility
**Status:** Done (keyboard-only manual pass still welcome) · **Priority:** P1 · **Effort:** M

- [x] WCAG AA contrast for all tokens in light and dark (token pairs audited, plus a DOM scan of the composer and results views; control borders raised to 3:1)
- [x] BulletEditor: move up/down buttons and `Alt+Arrow` keyboard reorder (F11); focus follows the moved bullet and the move is announced
- [x] `aria-live` region announces generation results; focus moves to the results region
- [x] Visible focus on every interactive element; selectable cards are real radios / checkboxes; insights tabs follow the ARIA tabs pattern
- [x] axe scan in E2E has no serious violations (`AccessibilityTests`)

Verify: TEST-07 axe check + keyboard-only manual pass.

### UX-09: Responsive pass
**Status:** Done (E2E at 390px and 768px arrives with TEST-07) · **Priority:** P1 · **Effort:** M

- [x] Composer usable at 360px width
- [x] Workspace stacks on mobile; preview becomes a tab (Preview / Insights switch below 992px; the editor opens in the second pane)
- [x] No horizontal scroll at any breakpoint (measured at 360, 390, 768, 993 and 1024px across composer, history, results, every tab and the editor)

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
**Status:** Done · **Priority:** P1 · **Effort:** L

- [x] Form-based editor: header, education, experience (bullets + skills tags), skills by category, projects
- [x] Saved in localStorage only; never stored on the server
- [x] Profile switcher: "Demo profile" and "My profile"

Notes: page `/profile` (`Pages/Profile.razor`) with `ProfileField`, `ProfileTagField` and `ProfileListField`. Save validates with `ProfileValidator` and shows errors under each field; nothing is saved while there are errors. `BrowserStore` keeps it under `cv-generator-my-profile`. Step 2 of the generator has `ProfileSourceSwitcher` (choice kept in `AppSettings.ProfileSource`); "My profile" is disabled until one is saved. An imported profile (PROF-02) still wins over both. The page can start from the demo profile, import JSON, export JSON and download the template.

Verify: `ProfileFieldsTests` (bUnit); `MyProfileTests` E2E (field errors and nothing saved; create, save, generate, output uses it; switcher disabled); `AccessibilityTests` covers the editor.

### PROF-02: Import/export + validation
**Status:** Done · **Priority:** P1 · **Effort:** M

- [x] Import JSON file; export current profile as JSON
- [x] Validation with field-level errors (required fields, date formats, empty bullets)
- [x] Downloadable blank template

Notes: `ProfileTools.razor` (import, export, blank template) first sat in step 2 and was moved to the `/profile` editor so the three steps stay simple. Importing there fills the form; saving makes it "My profile". Files over 512 KB are refused. `ProfileJson` parses (case-insensitive keys, comments and trailing commas allowed), serializes and builds the template.

Verify: `ProfileValidatorTests` and `ProfileJsonTests` (incl. export/import round trip); `ProfileImportTests` E2E (export, edit, import, generate; invalid file; template).

### PROF-03: Generate from a custom profile
**Status:** Done · **Priority:** P1 · **Effort:** S

- [x] `CVGeneratorService` overload that takes a `CVMaster` instead of a `profileId`
- [x] Demo profile stays the default

Notes: `BuildCvAsync(request, profile, progress)` validates the profile with `ProfileValidator` and raises `ArgumentException` listing the first 5 field paths. The stored `profileId` path is unchanged. `DateParser.TryParseStrict` was added for the validator (the tolerant `TryParse` reads "13/2024" as 2024).

Verify: `CustomProfileTests` runs the no-fabrication, header, score-range and no-mutation checks against custom profiles.

### PROF-04: Profile health check
**Status:** Done · **Priority:** P2 · **Effort:** M

- [x] Flags bullets without metrics, weak opening verbs, experiences without skill tags, overlapping or invalid dates
- [x] Shown as a checklist in the profile editor (`/profile`), not in the composer steps

Notes: `ProfileHealthChecker.Check` returns `HealthFinding(rule, path, message)`. Metrics reuse `ATSConstants.MetricsRegex`, so a flagged bullet is one the ATS check would also not count. Weak verbs are judged on the first word only. Roles that overlap by one month or less are treated as a hand-over. `ProfileHealthCard` is a collapsible panel shown only on `/profile`, through "Check profile health" (it opens expanded there). It was removed from step 3 of the composer so the three steps stay simple. It is advice only and never blocks generating.

Verify: `ProfileHealthCheckerTests` (one group per rule); `ProfileHealthTests` E2E (checklist on `/profile` shows warnings and the CV still generates; step 3 has no checklist).

### PROF-06: The owner's real profile as the demo
**Status:** Todo · **Priority:** P1 · **Effort:** S

Why: the demo profile is a made-up person (Alex Rivera). The portfolio is stronger if the demo is the owner's own, real CV.

- [ ] The owner provides the profile (JSON, or typed in the `/profile` editor) and it replaces `Data/cv-master.json` as the demo profile
- [ ] Tests and fixtures that assert on "Rivera" or on its bullets are updated to the new demo, or keep a small fixture copy of the old one so edge cases stay covered
- [ ] The step 2 choice stays "Demo profile / My profile / Create my profile"; "Create my profile" opens the `/profile` editor (import, export and the blank template live there, not in the composer)

Notes: waiting for the owner to upload their profile. Do not invent it. Check the personal data in it (phone, email) before committing it to a public repo.

Verify: the E2E suite and the document matrices pass on the new demo profile.

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
**Status:** Done · **Priority:** P1 · **Effort:** M

- [x] Each selected experience and bullet exposes a score breakdown (matched skills, keywords, recency, metric bonus, verb bonus)
- [x] Workspace shows "Why this was chosen" on hover/expand

Notes: `CVSelector.SelectWithExplanation` returns the CV plus a `SelectionExplanation`; `SelectAndAdaptCV` now delegates to it, so the selection is unchanged (a test compares both). Scores are built as the sum of named `ScorePart`s, so the breakdown cannot drift from the score. The explanation follows the final CV after the page constraint, and lists the bullets that were left out with their scores. It is also in the `/api/cv/generate` response as `explanation`. UI: a "Why" tab with one expandable block per role (`SelectionExplanationCard`); after editing bullets the card reads the current CV, so included / left out stays correct.

Files: `Helpers/CVSelector.cs`, `Helpers/RelevanceScorer.cs`, `Models/ExplanationModels.cs`, `Components/Shared/SelectionExplanationCard.razor`
Verify: `SelectionExplanationTests` (parts sum to the score over profiles x corpus x 1/2 pages); E2E `Why_tab_explains_each_role_and_bullet_with_its_score_parts` and the axe scan of the Why tab.

### MATCH-05: JD highlighter
**Status:** Done · **Priority:** P1 · **Effort:** M

- [x] JD rendered with matched skills in green and missing ones in red
- [x] Click a highlighted skill to see where it appears in the CV

Notes: new "Job" tab (`JdHighlightCard`). `JdHighlighter` cuts the job description into segments (joining them gives the JD back exactly) and finds each skill the parser extracted, whole tokens only, longest match first (`React Native` is one highlight), with aliases (`Golang` for `go`). Aliases that are also everyday words (`next`, `RN`, `ml`...) are not highlighted. "Matched" means the profile lists the skill or a bullet mentions it (same rule as the Gaps tab, now shared through `MissingSkillsAnalyzer.IsInProfile`), so it does not depend on what this CV happened to include. Clicking a skill lists where it appears in the generated CV (skills list, role bullets and tags, projects, summary); a skill that is in the profile but not in this CV says so. Status is not color only: matched is underlined, missing is dashed. The tab strip now scrolls sideways if six tabs do not fit. `SkillNormalizer` gained `FindTerm` / `FindSkill` (positions); `ContainsTerm` is built on `FindTerm`.

Files: `Helpers/JdHighlighter.cs`, `Models/JdHighlightModels.cs`, `Components/Shared/JdHighlightCard.razor`, `Helpers/SkillNormalizer.cs`
Verify: `JdHighlighterTests` (lossless split over profiles x corpus, whole tokens, locations, markup stays text); `JdHighlightCardTests` (bUnit: HTML-encoded output, selection, labels); E2E `Job_tab_highlights_skills_and_shows_where_one_appears_in_the_cv` and the axe scan of the Job tab.

### MATCH-06: Required vs nice-to-have from JD sections
**Status:** Todo · **Priority:** P2 · **Effort:** M

- [ ] Parser detects sections ("Requirements", "Must have", "Nice to have", "Bonus", "Preferred")
- [ ] Missing-skills split uses the JD section instead of a fixed list

Verify: unit tests with corpus JDs.

### MATCH-07: Skill dictionary as data, tuned on real posts
**Status:** Done · **Priority:** P1 · **Effort:** M

- [x] Aliases and tech keywords moved to `Data/skills.json`
- [x] One test per alias
- [x] Ordinary English words stop matching as skills: `next`, `end-to-end`, `foundation`, `less`, `solid`, `rails` need context or a stricter match (F18)
- [x] The word after "N years of" is a skill only if it is in the dictionary: `professional`, `building`, `industry`, `hands-on` stop appearing (F19)
- [x] The single-letter skill `c` no longer matches "Washington D.C." or `objective-c` (F21)
- [x] Add the skills recent posts ask for and the dictionary lacks: AI agents/agentic, observability, distributed systems, on-call, incident response, system design, LLMs, MCP, evals, data modeling, ETL, lakehouse, Iceberg, accessibility, networking
- [x] Review the 205 dictionary skills no post asks for: keep the ones a real CV can list (they still help matching a profile), drop duplicates and typos; do not delete a skill only because it is rare in 194 posts

Notes: `corpus/report.md` is the evidence and the acceptance check: regenerate it after the change (`dotnet run --project ErCv.Collector -- report`) and the false positives above must be gone. The change moves scores, so review the corpus impact by hand as in MATCH-01.

Verify: unit tests with corpus posts (a post with "the next generation" has no `next` skill); adding an alias requires no code change; `CorpusTests` still pass; report before/after attached to the PR.

Done notes:
- `Data/skills.json` (embedded, read once by `Features/Matching/SkillDictionary.cs`) holds one entry per skill: `name` (canonical, lowercase), `aliases` (other names, normalized to `name`), optional `match` (the forms searched in a job post or a bullet; a form with a capital letter only counts written that way, `[]` means a profile skill name only) and optional `except` (phrases in which a match does not count, such as "go-to-market" for Go or "spring 2027" for Spring). `SkillNormalizer` and the parser read it; the hardcoded `CommonTechKeywords` and alias table are gone.
- Token boundaries are stricter for every match: a dot between letters, an `@` or an `_` joins one name ("Node.js" no longer yields `javascript`, "grafana.com" no `grafana`), and a single letter next to `-`, `&` or `'` belongs to another word ("Objective-C", "R&D", "C-level").
- F18: `next` and `foundation` are no longer searched in text (they stay profile names), `solid` and `rails` count only as SolidJS and Rails, `less` only as LESS, bare `end-to-end` is no longer a skill (e2e testing needs "e2e/end-to-end testing" or "tests"), Go only counts written "Go" or "golang". Other product names that are English words got the same capital-letter rule after reading their corpus matches (Spark, Helm, Node, Bootstrap, Snowflake, Swift, REST, Notion, Linear and more); `https` only counts as "HTTPS" (it matched URLs).
- F19: the word after "N years of" counts only when it is one of the dictionary's text forms ("3 years of go" still gives Go).
- New skills: the 15 in the list above plus generative AI, RAG, prompt engineering, fine-tuning, embeddings, LangGraph, MLflow, CUDA, JAX, ClickHouse, NoSQL, Parquet, data warehouse, ELT, A/B testing, threat modeling, SIEM, penetration testing, Istio, Envoy, Gradle, JVM, Jetpack Compose, SwiftUI, DNS, CDN, SLOs, concurrency, data structures, algorithms, API design, design systems, GitOps, infrastructure as code, WebRTC. 417 skills (397 before).
- Review of the unused skills: duplicates merged (`bigquery`, `oauth` and `gitlab` were listed twice; `cpp`, `css3`, `solid`, `ember.js`, `express.js`, `rails`, `argocd`, `newrelic`, `websocket`, `oauth2`, `ddd`, `xunit.net`, `subversion`, `apache spark`, `apache airflow`, `apache flink`, `oidc`, `vault`, `maui`, `site reliability` and `plsql` are now aliases of one skill), FaunaDB dropped (the service shut down), the rest kept. Aliases that would drop a cloud platform ("AWS Lambda" normalizing to plain `lambda`) are not allowed, so a profile listing "AWS Lambda" still counts for a job asking for AWS (`SkillDictionaryTests`).
- H10 fixed here: the selector and the summary share `SelectionConstants.TechRoleKeywords`.
- Before/after on the 194 posts (`corpus/report.md`): next.js 77 posts to 3, go 69 to 48, end-to-end 49 to 0, foundation 31 to 0, solid 31 to 0, less 20 to 0, c 13 to 8, javascript 35 to 31, https 7 to 0, words after "N years of" (professional 15, building 12, ...) to none. The new top skills are python, observability, AI agents, machine learning and distributed systems. The "ordinary English" table now lists concepts that are normally lowercase (observability, on-call, system design), which are real skills.
- Tests: `SkillDictionaryTests` (file well formed, one test per alias and per text form, profile-only skills, cloud platforms), new cases in `SkillNormalizerTests` and `JobDescriptionParserTests` with sentences from the corpus posts, `CorpusTests` (every parsed skill is in the dictionary, Next.js only where a post names it).
- Left out on purpose (still in the report's watch list): bare "agents" (support agents, "teams and their agents"; AI agents and agentic are in), vendor and model names (Claude, Copilot, Gemini, Salesforce) and concepts such as scalability and experimentation.
- F22, found while closing this item, fixed in the same milestone: the keywords tab takes its technical keywords from the dictionary (`KeywordAnalyzerTests`).

### MATCH-08: Page fit measured on the rendered PDF
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] After rendering, count PDF pages (or measure DOM height via Puppeteer); trim and re-render until it fits (F6)
- [ ] Character estimate kept only as a first pass

Verify: TEST-04 page count assertion over the whole corpus.

### MATCH-09: Seniority detection
**Status:** Done · **Priority:** P1 · **Effort:** S

- [x] "Staff", "Principal", "II/III", "Intern/Internship" and "Early career" are recognized as levels (F20)
- [x] The title decides when it names a level; the first level word in the body ("lead the design") only breaks ties
- [x] Agreement with the title level in the corpus goes from 46% to a target recorded in the PR (aim: above 85%)

Verify: unit tests per title pattern; `corpus/report.md` shows the new agreement figure.

Done notes:
- The first line is the title when it is at most 120 characters. Levels are checked in a fixed order (intern, junior, principal, staff, lead, mid, senior, the same order as the collector's classifier), so "Senior Staff Engineer" is staff; roman numerals after the role count only when no level word is named (I junior, II mid, III and IV senior). "Member of Technical Staff" is not staff. Spanish titles work too ("Ingeniero ... Senior", "Semi Senior" is mid).
- When the title names no level, the body counts only a level word followed within three words by a singular role noun ("a senior machine learning engineer"), so "lead the design" and "our senior engineers" no longer decide; then the years asked for; then mid-level. A level word in the body never overrides the title.
- The parser now reports one of seven levels (`intern`, `junior`, `mid-level`, `senior`, `staff`, `lead`, `principal`) instead of the matched word (`staff engineer`, `sr.`, `entry-level`); all of them are in `MatchScoreConstants.SeniorityMap`.
- "3+ years" now reads as mid-level (`MidLevelYearsThreshold` 4 to 3): 9 of the posts with a plain title (no level word) that the parser read as junior asked for 3+ years, and 3 years is mid on most career ladders.
- Agreement with the title level: 89 of 194 (46%) before, 176 of 194 (91%) after. The 18 left are titles without a level word where the body or the years asked for point to another level (8 read as junior, 9 as senior, 1 as staff).
- Tests: `JobDescriptionParserTests` (one case per title pattern, title over body, body role phrases), `CorpusTests.Parser_agrees_with_the_title_level_on_most_of_the_corpus` (more than 85%).

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
**Status:** Done · **Priority:** P2 · **Effort:** M

- [x] History entries gain company, role, status (Saved, Applied, Interview, Offer, Rejected), notes
- [x] Filter and sort by status/date; stays in localStorage

Notes: the tracker is the History panel. Each entry has a "Details" section (company, role, status, notes) saved to localStorage as you edit. The title reads "Role at Company" once filled in. Filter by status (with counts) and sort by newest, oldest or status (pipeline order). `ApplicationTracker` holds the statuses, filter and sort. Entries saved before this read as "Saved". The history cap went from 10 to 50 so a tracker is usable.

Verify: `ApplicationTrackerTests` (unit), `HistoryPanelTests` (bUnit), `ApplicationTrackerTests` E2E (details kept after reload, filter, sort) and the accessibility scan of the open panel.

### APP-02: Re-open a past generation
**Status:** Done · **Priority:** P2 · **Effort:** S

- [x] History stores template, formats, page mode and profile; "Re-open" restores and regenerates

Notes: `HistoryEntry` gained `TemplateKey`, `Formats`, `AllowTwoPages`, `ProfileSource` and `ProfileId`; entries saved before this keep working and re-open with the current options. The button was "Regenerate" and only reloaded the job description; it is now "Re-open" and generates straight away. An imported profile is memory only, and "My profile" may have been cleared, so in both cases the demo profile is used and a toast says so.

Verify: `PersistenceAndShortcutTests` E2E (options restored and the same formats come back; imported profile falls back to the demo).

### APP-03: Sample JD gallery
**Status:** Done · **Priority:** P0 · **Effort:** S

Why: portfolio visitors rarely have a JD at hand.

- [x] 6 curated JDs (backend, frontend, full stack, data, DevOps, junior) in `Data/SampleJobDescriptions.cs`
- [x] One click fills the composer (`SampleJobPicker.razor`)

Notes: since APP-05 the six samples are real posts picked from `corpus/` by the collector and stored in `Data/sample-jobs.json` (same labels); the hand-written texts are gone.

Verify: E2E: pick sample, generate.

### APP-04: Import JD from URL
**Status:** Todo · **Priority:** P3 · **Effort:** M

- [ ] Greenhouse and Lever public job APIs only, allowlisted hosts (SSRF-safe), timeout and size limit

Verify: integration tests with mocked HTTP; disallowed hosts rejected.

### APP-05: Real job-post corpus, dated and kept fresh
**Status:** Done · **Priority:** P1 · **Effort:** L

Why: the sample JDs (APP-03), the test corpus (`ErCv.Tests/Fixtures/jds`) and the skill dictionary behind the parser were all written by hand. They are plausible, but they are not what employers are posting now, so the demo, the tests and the scoring are tuned to guesses. Real, recent posts make the demo convincing, the tests honest, and show where the app drifts from the market (new tools, renamed skills, different seniority wording).

- [x] Collector (a console tool, not part of the web app) that gathers current postings from sources that allow it: the public job-board APIs of Greenhouse, Lever, Ashby and Workable, plus feeds that publish their data on purpose. No scraping of sites whose terms forbid it; respect `robots.txt`, rate limits and a clear User-Agent
- [x] One JSON file per posting with metadata: source, url, company, title, `postedAt`, `retrievedAt`, role family, seniority, location, and the clean text. Keep only what is needed and link back to the original post
- [x] Freshness window (for example 90 days) and a refresh command that drops expired posts and adds new ones
- [x] A GitHub Action runs the refresh command and opens a PR, so a person reviews every change (`.github/workflows/corpus-refresh.yml`, manual trigger only, cron commented out; its first run on GitHub is APP-06)
- [x] A relevance report with dates: skill frequency over time, skills the parser misses (`MATCH-07` dictionary), skills in the dictionary nobody asks for any more, and seniority wording, so the app can be adjusted to what recent posts actually say
- [x] APP-03 samples are picked from the corpus (one per role family) and show where and when they were posted
- [x] Test fixtures read the same corpus: the parser and scoring tests run over real posts, and `TEST-08` golden snapshots use them (TEST-08 is not built yet; its own checklist now says to snapshot `FixtureLoader.CorpusJdNames`)
- [x] Removal path: honor takedown requests and keep a short note of the licensing and terms of each source

Workflow: `corpus-refresh.yml` runs `refresh`, `report` and `check`, then opens a PR with `gh` (it never pushes to master). It was written but not run on GitHub yet. One-time repo setting needed: Settings > Actions > General > allow Actions to create pull requests. The weekly cron is commented out for the owner to enable.

Notes: the legal and terms-of-use review of each source is part of the work, not an afterthought. `APP-04` (import one JD from a URL) is related but separate: that one is the user's own action on one post, this one is a maintained dataset.
Recommended model for the implementation: Opus. It is a multi-part design task (source adapters, data model, scheduled automation, a report that has to be judged, and terms-of-use trade-offs), and a wrong call silently ships stale or unlicensed data.

What was built:
- `ErCv.Collector` (console, BCL only: `HttpClient` + `System.Text.Json`, no new packages). One adapter per source (`Sources.cs`), `PoliteHttp` (User-Agent `ErCvCorpusCollector/1.0 (+repo url)`, `robots.txt` per host as in RFC 9309, one request per host every 1.5 s or the host's `Crawl-delay`, no redirects, timeout and size cap), `Classifier` (role family and seniority from the title; only individual-contributor software roles are kept), `Refresher`, `SamplePicker`, `RelevanceReport`, `CorpusCheck`. It references `ErCv.Web` so the report and the sample picker use the real parser and dictionary.
- `corpus/`: `sources.json` (37 boards, window and limits), `postings/*.json` (one file per post, fields as listed above), `takedowns.json`, `manifest.json`, `report.md`, and `README.md` with the per-source terms note and the removal steps.
- Commands: `dotnet run --project ErCv.Collector -- refresh` (the single refresh command), `-- report`, `-- check`, `-- remove <id-or-url> --reason "..."`.
- Window: 90 days from the source's own publish date (Greenhouse `first_published`, Lever `createdAt`, Ashby `publishedAt`, Workable `published_on`). At most 6 posts per board (newest, spread over role families and seniority), text cut at 15,000 characters, em dashes in post text turned into hyphens. Posts that expire, close at the source or are taken down are removed; a board that cannot be read keeps its posts until they expire. Unchanged posts are not rewritten and `manifest.json` only changes when the corpus changes, which makes the refresh idempotent.
- APP-03: the six samples (labels unchanged: Backend, Frontend, Full stack, Data, DevOps, Junior) are written by `refresh` to `ErCv.Web/Data/sample-jobs.json`, embedded in the web app (so the Docker image does not need `corpus/`). The picker shows "Title at Company (location), posted d MMM yyyy on Source. View the original post" for the active sample.
- Tests: `FixtureLoader` reads `corpus/` from the repository; `CorpusJdNames` (the newest real post of each role family) join the hand-written JDs in the invariant, highlighter and selection-explanation matrices (`AllJdNames`). The hand-written JDs stay for their edge cases (empty, Spanish, very long, non-tech). The DOCX/PDF matrices still use the hand-written set only, to keep render time down.
- Removal: `remove` deletes the file and records the takedown by id and url; a whole company is a `board` takedown. A removed post stays in git history; rewriting history is the owner's call.

Sources (terms reviewed 2026-10-20, details in `corpus/README.md`): Greenhouse, Lever, Ashby and Workable public job-board APIs, all documented as public and unauthenticated, none disallowed by `robots.txt`. None publishes a license for third-party reuse of post text, hence the minimal fields, the link back, the small size and the takedown path. Ashby posts marked `isListed: false` are skipped. Not used: job sites whose terms forbid automated collection (LinkedIn, Indeed, Glassdoor), hosted job pages and Greenhouse `/embed/` (pages, not APIs; `/embed/` is disallowed), and other feeds (Remotive, RemoteOK, Hacker News "Who is hiring"), which were not evaluated because only the four ATS APIs were approved for download; each needs its own terms review. Boards dropped: `lever/plaid` (404) and `lever/mistral` (no posts).

Corpus on 2026-10-20: 194 posts from 35 companies (Greenhouse 114, Ashby 48, Lever 28, Workable 4), posted 2026-07-22 to 2026-10-20. Families: software 41, ml 32, devops 29, data 25, security 20, backend 13, mobile 11, frontend 10, full stack 9, qa 4.

Decisions taken when closing M3: the corpus (194 posts, 1.7 MB, with the terms note and the takedown path) stays committed in git, because the tests and the Docker-free samples need it and a takedown request is the only reason to rewrite history. The six samples stay as the picker chose them for the release; the picker rule is reviewed in APP-06.

Headline findings from `corpus/report.md` (not fixed here, see F18 to F21, owned by M3.2):
- The dictionary matches ordinary English: `next` (alias of Next.js) is "found" in 40% of posts, almost always "the next generation"; also `end-to-end` (25%), `foundation`, `less`, `solid`, `rails` ("banking rails").
- "N years of X" takes the next word as a skill: `professional` (15 posts), `building`, `industry`, `hands-on`, `working`.
- Missing from the dictionary although posts ask for them: AI agents and agentic (38% and 19%), observability (35%), distributed systems (25%), on-call, incident response, networking, system design, LLMs, MCP, evals, data modeling, ETL, lakehouse, Iceberg, accessibility.
- 205 of the 397 distinct dictionary skills appear in no post (for example most Azure services, jQuery, PHP, Laravel, Jest, Flutter, Entity Framework, .NET), 66 in only one.
- Seniority: titles say "Staff" 36 times, but the parser only knows the phrase "staff engineer"; "II", "internship" and "early career" are not keywords; the parser's level agrees with the title in 46% of posts (it takes the first level word in the text, often "lead the ...").

Verify: the refresh command run twice in a row: the second run reported "Added 0, updated 0, removed 0, unchanged 194" and every file under `corpus/` plus `sample-jobs.json` had the same hash before and after. `check` reports no post older than the window as of today. `CorpusTests` (unit): the corpus follows every rule as of its last refresh, no post is older than the window, every post, sample and corpus fixture has `postedAt` and an https source url, samples are what the picker chooses, the parser reads all 194 posts without junk, and the report runs on the committed corpus. `CollectorTests` (unit, fake HTTP, no network): idempotent refresh, expiry, closed posts, failing board, edits, takedown by url and by board, `robots.txt`, User-Agent, request spacing, sample picking. `SampleJobPickerTests` (bUnit) and the E2E `Picking_a_sample_shows_where_and_when_it_was_posted_with_a_link_to_the_original`; the full E2E suite (accessibility and responsive included) passes with the real samples.

### APP-06: Corpus follow-ups
**Status:** In progress · **Priority:** P2 · **Effort:** S

- [x] Sample picker skips posts that are not a normal job (accelerators, programs, fellowships) and prefers posts with a clear level in the title; the Full stack sample (an accelerator post) and the Junior sample (Associate DevOps) are the cases that triggered it
- [ ] Run the corpus refresh workflow once from the Actions tab and review the PR it opens; fix whatever the first real run shows
- [ ] Decide whether to enable the weekly cron (the user turns it on, see the workflow file)

Verify: `CorpusTests` samples test updated to the new rule; the first run's PR exists and `check` passes on it.

Progress notes:
- The picker leaves out titles with "accelerator", "fellowship", "residency", "apprenticeship" or "bootcamp" and prefers titles that name a clear level (`Classifier.NamesClearLevel`: junior, new grad, senior, staff and so on; not "Associate" or "Engineer 3"). New samples: Backend MongoDB (Senior), Frontend GitLab (Staff), Full stack GitLab (Staff Fullstack), Data Elastic (Lead Data Engineer), DevOps Databricks (Senior Production Engineer), Junior Palantir (New Grad). Tests: `CorpusTests.Samples_are_normal_jobs_with_a_clear_level_in_the_title`, `CollectorPartsTests.Sample_picker_skips_programs_and_prefers_a_clear_level`.
- New collector command `samples`: re-picks the samples from the committed corpus without the network (needed whenever the parser or the dictionary changes, since the picker ranks by parsed skills).
- Still open: the first run of the refresh workflow on GitHub (the repo owner allows Actions to create PRs, then runs it from the Actions tab) and the cron decision. Until a refresh runs, `check` fails on one post that aged out of the window (`lever-palantir-67110929...`, posted 2026-07-22).

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
- [x] E2E job (Playwright) in `ci.yml`, separate from the unit job (first run on GitHub pending)
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

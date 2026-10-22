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
| UX-10 | CV template visual polish | E2 Redesign | P2 | M | Done |
| PROF-01 | In-browser profile editor | E3 Your profile | P1 | L | Done |
| PROF-02 | Profile JSON import/export + validation | E3 Your profile | P1 | M | Done |
| PROF-03 | Generate from a custom profile | E3 Your profile | P1 | S | Done |
| PROF-04 | Profile health check | E3 Your profile | P2 | M | Done |
| PROF-06 | The owner's real profile as the demo | E3 Your profile | P1 | S | Done |
| PROF-05 | Import from existing PDF/DOCX resume | E3 Your profile | P3 | L | Todo |
| MATCH-01 | Fix fuzzy-match false positives | E4 Matching | P0 | S | Done |
| MATCH-02 | Word-boundary ATS keyword density | E4 Matching | P0 | S | Done |
| MATCH-03 | Shared date parser + correct "most recent role" | E4 Matching | P0 | M | Done |
| MATCH-04 | Explainability: "why this was chosen" | E4 Matching | P1 | M | Done |
| MATCH-05 | JD highlighter | E4 Matching | P1 | M | Done |
| MATCH-06 | Required vs nice-to-have from JD sections | E4 Matching | P2 | M | Todo |
| MATCH-07 | Skill/alias dictionary as data file, tuned on real posts | E4 Matching | P1 | M | Done |
| MATCH-08 | Page fit measured on the rendered PDF | E4 Matching | P1 | M | Done |
| MATCH-09 | Seniority detection from the title and wording | E4 Matching | P1 | S | Done |
| EDIT-01 | Inline editing in the workspace | E5 Editing | P2 | L | Done |
| EDIT-02 | Live HTML preview, documents on download | E5 Editing | P2 | M | Done |
| EDIT-03 | Cover letter generator | E5 Editing | P2 | M | Done |
| EDIT-04 | CV output language (EN/ES) | E5 Editing | P2 | M | Done |
| EDIT-05 | More templates | E5 Editing | P3 | M | Done |
| EDIT-06 | Plain-text / Markdown export | E5 Editing | P3 | S | Done |
| APP-01 | Application tracker | E6 Workflow | P2 | M | Done |
| APP-02 | Re-open a past generation | E6 Workflow | P2 | S | Done |
| APP-03 | Sample JD gallery | E6 Workflow | P0 | S | Done |
| APP-04 | Import JD from URL (Greenhouse/Lever) | E6 Workflow | P3 | M | Todo |
| APP-05 | Real job-post corpus, dated and kept fresh | E6 Workflow | P1 | L | Done |
| APP-06 | Corpus follow-ups: sample picker rules, first Action run | E6 Workflow | P2 | S | Done |
| AI-01 | LLM options + feature flag | E7 AI | P3 | S | Parked |
| AI-02 | Semantic matching (indices only) | E7 AI | P3 | M | Parked |
| AI-03 | Bullet rewrite suggestions (user accepts) | E7 AI | P3 | M | Parked |
| AI-04 | Cover letter polish | E7 AI | P3 | S | Parked |
| AI-05 | Cost guardrails and caching | E7 AI | P3 | S | Parked |
| ENG-01 | DI for helpers + single namespace root | E8 Engineering | P2 | M | Todo |
| ENG-02 | Consolidate duplicated logic, immutable selection | E8 Engineering | P0 | M | Done |
| ENG-03 | ILogger + ProblemDetails (no leaked exceptions) | E8 Engineering | P0 | S | Done |
| ENG-04 | Options pattern for scoring weights | E8 Engineering | P3 | S | Won't do |
| ENG-05 | Chromium readiness in health check + warm-up | E8 Engineering | P2 | S | Done |
| ENG-06 | Modern OpenAPI (built-in + Scalar) | E8 Engineering | P3 | S | Done |
| OPS-01 | GitHub Actions CI | E9 DevOps | P0 | M | Done |
| OPS-02 | Dependabot | E9 DevOps | P2 | S | Done |
| OPS-03 | README rewrite for portfolio | E9 DevOps | P1 | M | Todo |
| OPS-04 | PROJECT_OVERVIEW refresh | E9 DevOps | P2 | S | Todo |
| OPS-05 | Privacy note | E9 DevOps | P2 | S | Todo |
| OPS-06 | Deploy to Render + live demo link | E9 DevOps | P1 | M | Todo |

---

## Milestones

1. **M1 Foundation** (v2.0.2, released): TEST-01, TEST-02, TEST-03, TEST-04, TEST-05, MATCH-01, MATCH-02, MATCH-03, ENG-02, ENG-03, OPS-01. Fixes ship with their tests from day one.
2. **M2 Redesign** (v2.0.3, released): UX-01 to UX-09, APP-03, TEST-07 (E2E written against the new UI).
3. **M3 Your data** (v2.0.4, released): PROF-01 to PROF-04, MATCH-04, MATCH-05, APP-01, APP-02, APP-05.
3.2. **M3.2 Real-post fixes** (v2.0.4.1, released): MATCH-07, MATCH-09, APP-06, PROF-06. Fixes what the real-post report found (F18 to F21) so the parser and the demo match what employers write.
4. **M4 Output** (v2.0.5, released): EDIT-01 to EDIT-06, MATCH-08, UX-10.
5. **M5 Polish & presentation** (v2.0.6): OPS-02 to OPS-05, ENG-01, ENG-05, ENG-06, TEST-06, TEST-08, TEST-09, TEST-10. ENG-04 was dropped (Won't do, see the item). Nothing in M5 waits on M6.
5.1. **M5.1 Deploy** (v2.0.6.1): OPS-06. The first public deploy, and the live demo link the README gets with it.
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
| F6 | Page fit is estimated by character count, not measured; a "1-page" CV can spill over. | `Helpers/PageConstraintHelper.cs` | MATCH-08 (fixed) |
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

### Findings during M4

| # | Finding | Status |
|---|---|---|
| F23 | `BrowserStore` let a `TaskCanceledException` from JS interop escape `Home.OnInitializedAsync` when the circuit went away during the first storage reads (a reload, a closed tab, or hot reload while the app runs from the IDE). | Fixed: reads give the default and writes and removals do nothing when the circuit is gone (`JSDisconnectedException`, `TaskCanceledException`); other script errors still surface. `BrowserStoreTests` |

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
| H10 | `CVSelector` and `SummaryGenerator` keep two slightly different "tech role" word lists. | Fixed with MATCH-07: one list, `SelectionConstants.TechRoleKeywords`, the summary's stricter one plus nothing broad: the selector no longer treats analyst, consultant or specialist roles as tech (a union was tried first and made "Certification Specialist" count as tech years in the real demo profile) |
| H11 | Swashbuckle 6.5.0 is old (Development only). | Fixed with ENG-06 (M5): built-in OpenAPI + Scalar |

### Open items after M4

Nothing in M4 is blocked. Found while building it, not scheduled yet (each would be a new item):

| Item | Why it is not in M4 | Owner |
|---|---|---|
| The editor toggles what the generated CV holds; a project or skill the selection or the page fit left out cannot be added back | Needs the profile in the workspace and a rule for what still fits on the page. | New item (not scheduled) |
| Match, ATS and keyword scores are not recomputed after edits | The edit loop renders HTML only; the analysis would have to run again per change. | New item (not scheduled) |
| The cover letter has a generic salutation and no company name (the parser does not read the company), and its text cannot be edited, only the CV it quotes | A company field (the tracker has one) and letter editing are their own features. | New item (not scheduled) |
| Spanish keeps role titles as written and leaves out the summary's achievement sentence | Translating profile text is out of scope by design (the app never rewrites the profile). | By design |
| The suite without browsers takes about 7 minutes (five templates over the corpus) | The page count over the whole corpus is what proves MATCH-08. | TEST-10 (M5) can look at splitting it |
| TEST-08 snapshots over `FixtureLoader.CorpusJdNames` | Snapshots are not built yet. | TEST-08 (M5) |

### Open items after M3.2

Nothing in M3.2 is blocked. Left to a later milestone:

| Item | Why it is not in M3.2 | Owner |
|---|---|---|
| TEST-08 snapshots over `FixtureLoader.CorpusJdNames` | Snapshots are not built yet. | TEST-08 (M5) |

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
**Status:** Done (26 tests; the CI job runs on GitHub and is green, first checked on the M4 merge, PR #14) · **Priority:** P1 · **Effort:** L

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

Follow-up (during M4, owner's request):
- Toasts sit at the top right, below the header (`--header-height`), slide in from the right, and have no colored left border: a card with the icon in a soft tinted circle per kind.
- More actions confirm with a toast: CV ready, job description pasted with the shortcut, start over, tracker entry updated, removed from history, history cleared, and on the profile page: base profile loaded, profile cleared, imported, export and blank template downloads (with a retry on failure), and save blocked by validation errors (with the count). Picking a template, format or page mode has no toast: the selection shows on screen.
- The profile page's inline status line ("Saved in this browser", "Imported...", "Profile cleared") is replaced by the toast with the same text, so the message is not shown and announced twice.
- The same message again (a tracker field saved after another) replaces the toast on screen instead of stacking.
- Tests: `ToastServiceTests` (unit), E2E `Toasts_appear_at_the_top_right_below_the_header`; the existing E2E that wait for "Imported" and "Saved in this browser" now find the toast.

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
**Status:** Done · **Priority:** P2 · **Effort:** M

- [x] Shared typographic rhythm across Harvard, Modern and Minimalist
- [x] Optional accent color and font pairing per template (kept ATS-safe)
- [x] Real thumbnails generated from each template for the template picker

Files: `Templates/*.html`, `TemplateSelector.razor`
Verify: TEST-04 still passes for every template.

Done notes:
- `Templates/partials/`: `base-styles` (one body size and one spacing step as CSS variables, every margin a multiple of the step; the accent and font classes; the shared element styles), `contact`, one partial per section, and `sections` (the ordered loop). Every template includes them and only sets its header, its values of the scale and its details; Modern places the same section partials in its two columns. `DocumentRenderer` loads the partials once (`PartialsCache`). The markup is now the same everywhere (`cv-section`, `cv-entry`, `h2.cv-section-title`), which also fixed stray separators ("Acme, " with no location, "a@b.com •  • ").
- Minimalist was tightened (line height and section spacing): measured over the 95 profile x JD combinations in one-page mode, the measured fit (MATCH-08) now trims 5 of them by one bullet each, where it trimmed 57, by 2 or 3 bullets and usually the project. Harvard, Modern, Compact and Executive need no trim at all (Executive did, 21 of 95, until its body size and spacing were brought onto the same scale).
- Accent (navy, teal, burgundy, forest, slate) and fonts (classic serif, clean sans, mixed) are classes from a fixed list (`ApiConstants.Accents`, `FontPairings`; anything else is a 400, and the renderer ignores unknown keys), so nothing a request sends reaches the CSS. Accents only color titles, rules and markers, and are all dark; the text stays dark. Fonts are limited to what the PDF container ships (`fonts-liberation`: Times, Arial and Courier metrics), so the PDF looks the same locally and on Render. "Template's own" keeps each template as designed.
- Thumbnails: `CVGeneratorService.RenderTemplateSampleAsync` renders each template with the made-up senior sample profile in the chosen accent, fonts and language (cached); the picker shows it scaled down in a sandboxed iframe with no scripts. They follow every style change. The old hand-drawn SVGs are gone.
- Tests: `LanguageAndStyleTests` (classes only for known keys, every class the UI offers is defined in the shared styles, every template uses the partials), `StyleAndTemplatePickerTests` (bUnit: five cards with a real thumbnail each, picker without thumbnails, accent, fonts and language report their keys, every value the service accepts is offered), `StyleOptionsApiTests` (a sample for every template follows the style), E2E `StyleAndLanguageTests` (thumbnails, an accent restyles them, choices are remembered).

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
**Status:** Done · **Priority:** P1 · **Effort:** S

Why: the demo profile is a made-up person (Alex Rivera). The portfolio is stronger if the demo is the owner's own, real CV.

- [x] The owner provides the profile (JSON, or typed in the `/profile` editor) and it replaces `Data/cv-master.json` as the demo profile
- [x] Tests and fixtures that assert on "Rivera" or on its bullets are updated to the new demo, or keep a small fixture copy of the old one so edge cases stay covered
- [x] The step 2 choice stays "Demo profile / My profile / Create my profile"; "Create my profile" opens the `/profile` editor (import, export and the blank template live there, not in the composer)

Notes: waiting for the owner to upload their profile. Do not invent it. Check the personal data in it (phone, email) before committing it to a public repo.

Verify: the E2E suite and the document matrices pass on the new demo profile.

Done notes:
- `Data/cv-master.json` is now Ezequiel Rindello's profile, built from the owner's CV (`ezequiel-rindello-cv.pdf`, October 2026) and portfolio site; nothing was added that those do not say. Internal ticket numbers were left out. The phone is left empty on purpose (public repo); the email, LinkedIn and GitHub are the ones the portfolio already publishes.
- The work at Primero Systems is three entries, one per project (Chamber Mate, KOORUS, LabLink 360), same role and dates: the selector shows at most 3 bullets per experience, so one entry showed only 3 development bullets and filled the page with the non-tech jobs. The earlier non-tech jobs stay in the profile (a profile is a superset; the selector penalizes them for technical posts).
- Step 2 now reads "Senior sample profile / Creator's profile / My profile / Create my profile". The senior sample (`Data/cv-master-senior.json`, the earlier made-up demo Alex Rivera) is the first choice and the default, so a first visit shows what a full senior CV looks like; the owner's profile is "Creator's profile". The radios replace the old "CV Profile" dropdown (`ProfileSelector`, removed). The API still defaults to the creator's profile when a request names none. The profile page can start from either one.
- The phone in the creator's profile is the placeholder "55555" (the owner's choice, so the real number stays out of the public repo). It is shorter than the ATS check's minimum, so that CV shows the "phone" ATS issue.
- Alex Rivera stays as the `cv-master` test fixture with its edge cases; the real profile joins every profile matrix as `FixtureLoader.DemoProfileId`, read from `ErCv.Web/Data` (document, PDF, invariant, explanation, highlighter, validator and round-trip tests). E2E assertions now look for "Rindello".
- Fixed on the way: the summary read "...years of experience. specializing in ..."; "specializing in" now continues the first sentence (`SummaryGeneratorTests`).
- The profile health check flags most bullets for having no number: the source CV has none, and the profile does not invent them.
- Profile page (`/profile`), found while doing this: every comma-separated field (skills used, technologies, courses, the skills groups) was always red with the word "Error", because `ProfileTagField` passed the literal text `"Error"` instead of its `Error` parameter. Fixed, with a component test. Empty-item messages now say which item ("Skill 3 is empty. Remove the extra comma."), and the problem summary above the form lists each problem and where it is ("Experience 1, start date: ...") instead of only a count.
- Profile page layout: one toolbar for "Start from", the JSON file and clearing; section hints; entries with their role and company under the title; icon buttons to remove a bullet; dashed "Add experience" buttons; wider gutters; a save bar that stays at the bottom of the screen while the form scrolls, "Check profile health" scrolls to the health card and moves focus to it (`cvApp.revealElement`), and the destructive buttons (Clear my profile, Remove) are a red outline without the soft red fill that looked muddy in dark mode.

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
- H10 fixed here: the selector and the summary share `SelectionConstants.TechRoleKeywords` (without analyst, consultant and specialist).
- Before/after on the 194 posts (`corpus/report.md`): next.js 77 posts to 3, go 69 to 48, end-to-end 49 to 0, foundation 31 to 0, solid 31 to 0, less 20 to 0, c 13 to 8, javascript 35 to 31, https 7 to 0, words after "N years of" (professional 15, building 12, ...) to none. The new top skills are python, observability, AI agents, machine learning and distributed systems. The "ordinary English" table now lists concepts that are normally lowercase (observability, on-call, system design), which are real skills.
- Tests: `SkillDictionaryTests` (file well formed, one test per alias and per text form, profile-only skills, cloud platforms), new cases in `SkillNormalizerTests` and `JobDescriptionParserTests` with sentences from the corpus posts, `CorpusTests` (every parsed skill is in the dictionary, Next.js only where a post names it).
- Left out on purpose (still in the report's watch list): bare "agents" (support agents, "teams and their agents"; AI agents and agentic are in), vendor and model names (Claude, Copilot, Gemini, Salesforce) and concepts such as scalability and experimentation.
- F22, found while closing this item, fixed in the same milestone: the keywords tab takes its technical keywords from the dictionary (`KeywordAnalyzerTests`).

### MATCH-08: Page fit measured on the rendered PDF
**Status:** Done · **Priority:** P1 · **Effort:** M

- [x] After rendering, count PDF pages (or measure DOM height via Puppeteer); trim and re-render until it fits (F6)
- [x] Character estimate kept only as a first pass

Verify: TEST-04 page count assertion over the whole corpus.

Done notes:
- `PageConstraintHelper.FitMeasuredAsync` renders the CV and counts the real pages (`DocumentRenderer.CountPages`, the page objects of the PDF Chrome wrote). If it spills over it trims in the same order as the estimate (one step at a time: last bullet of the longest experience, projects, oldest experience, last skill) and binary searches the number of steps, so a spill costs about 4 or 5 renders. A CV that already fits costs nothing extra: the measured PDF is the one returned. The estimate and the measured fit share one trim step (`TrimOneStep`).
- `CVGeneratorService` fits right after selection, then runs the keyword and ATS analysis, the explanation (`CVSelector.FollowFinalCv`), the preview and the DOCX on the fitted CV, so every output shows the same CV. Regenerate goes through the same path. Without a PDF in the request there is nothing to measure and the estimate stands.
- Measured over 5 profiles x 17 JDs (the 9 hand-written ones and one real post per role family) x 3 templates x 2 page modes: only Minimalist in one-page mode spilled over (57 of 570). It loses 2 or 3 bullets, and usually the project, to fit. Harvard and Modern already fit with the estimate. Tightening Minimalist's spacing (UX-10) would keep more content.
- Tests: `PdfTests.Page_count_respects_the_selected_mode` now covers every template (the skipped Minimalist case is gone) over the whole matrix including the corpus; `Page_counter_agrees_with_a_pdf_reader` and `Page_counter_counts_every_page_of_a_long_pdf` check the counter against PdfPig; `PageConstraintHelperTests` (fake renderer: no extra render when it fits, smallest trim, few renders, two-page mode, nothing fits); `PageFitTests` (service: one-page PDF, preview, DOCX and explanation show the fitted CV, and the measured fit trims what the estimate keeps).
- The suite without browsers takes about 5 to 6 minutes now (the page count runs over the corpus, about 0.4 s per PDF).

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
**Status:** Done · **Priority:** P2 · **Effort:** L

- [x] Edit bullet text in the workspace; edited bullets are flagged as user-edited
- [x] Toggle summary, projects and individual skills
- [x] Reorder sections

Verify: bUnit + E2E.

Done notes:
- `Features/Editing/CvDraft.cs` holds the edits apart from the generated CV, so each one can be undone (one bullet, or "Undo all changes"): bullet text, bullets, summary, skills and projects in or out, bullet and section order. `Build()` gives the CV as edited. A rewrite is trimmed, cannot be empty or longer than `ApiConstants.MaxEditedBulletLength` (600, checked again in the service), and typing the profile's text again undoes it.
- No fabrication with edits: the app still writes nothing; a bullet the user rewrote is listed in `GeneratedCV.EditedBullets` with the profile's original (`BulletEdit`), shown with an "Edited" badge, and the "Why" tab explains it as the profile bullet it came from.
- Section order: `GeneratedCV.SectionOrder` (keys `DocumentConstants.Section*Key`). The templates print their sections in a `{{#Sections}}` loop; each template's own order is a constant (`HarvardSectionOrder`, `ModernSectionOrder`, `MinimalistSectionOrder`), so an empty order prints exactly as before. Modern keeps its two columns: a section moves within its column, and the editor says so. The DOCX and the text export follow the chosen order, Harvard's by default. `DocumentRenderer.ResolveSectionOrder` drops unknown keys and repeats and adds what is missing, so every section prints once. `DocumentRenderer.ForTemplate` now pairs each template file with its order (it replaces the per-template paths in the service and the tests).
- `Components/Shared/CvEditor.razor` replaces `BulletEditor.razor`, keeping drag and Alt+Up/Down for bullets and its focus handling. Rewrite opens a textarea (Ctrl+Enter saves, Escape cancels). Edits survive closing and reopening the editor; a new generation starts a new draft.
- Edits only reach the CV the user sees; the match, ATS and keyword scores stay those of the generated CV.
- Tests: `CvDraftTests` (unit, including every bullet being a profile bullet or a flagged rewrite of one, and the generated CV never changing), `SectionOrderTests` (HTML of each template, Modern columns, DOCX, text export, resolving), `CvEditorTests` (bUnit), E2E `EditingTests`.

### EDIT-02: Live HTML preview
**Status:** Done · **Priority:** P2 · **Effort:** M

- [x] Preview updates from HTML only while editing
- [x] PDF/DOCX rendered on download, not on every change

Verify: E2E: edit updates preview without a render delay.

Done notes:
- Every edit renders the preview HTML only (`CVGeneratorService.RenderPreviewHtml`), there is no "Regenerate" step any more. The PDF and DOCX of an edited CV are built when a download button is clicked (`RenderEditedDocumentAsync`) and kept until the next edit.
- The edited CV is printed as the user left it, never trimmed by the page fit (MATCH-08 would undo their choices). When an edited PDF runs past the page limit, a toast says how many pages it has.
- The preview keeps its scroll position across the reloads (`cvApp.keepScroll`, set up by the page; the sandboxed CV document still runs no script).
- The UI no longer calls `RebuildCvAsync` (removed); `POST /api/cv/regenerate` keeps its own path for API clients.
- Tests: `EditedCvTests` (service: preview, edited PDF and DOCX, not trimmed, input limits), E2E `EditingTests` (preview follows an unchecked bullet, a rewrite and a section move without regenerating, scroll kept, the DOCX downloaded after an edit holds it).

### EDIT-03: Cover letter generator
**Status:** Done · **Priority:** P2 · **Effort:** M

Moved from the legacy FEATURE-301.

- [x] "Also generate cover letter" option, same JD and profile
- [x] Structure: opening, two experience paragraphs, closing
- [x] Template-driven (Mustache), uses only CV data
- [x] PDF and DOCX

Files: `Features/CoverLetters/CoverLetterGenerator.cs`, `CoverLetterConstants.cs`, `CoverLetterText.cs`, `Templates/cover-letter-template.html`
Verify: invariant test that every fact in the letter comes from the profile.

Done notes:
- `GenerateRequest.IncludeCoverLetter` (step 3: "Also write a cover letter"). The letter is written from the CV as it stands: the opening names the position (the post's first line, only when it is short and names a role; otherwise "this position") and quotes the summary's first sentence; one paragraph each for the two most relevant experiences (role, company, dates and two bullets quoted verbatim); the closing names the skills that are both on the CV and in the post, then thanks the reader. English or Spanish (EDIT-04), the CV's accent and fonts (UX-10), on the shared partials.
- No fabrication: each paragraph is a list of parts, fixed wording (`CoverLetterConstants.Phrases`) or a fact. `CoverLetterInvariantTests` checks, for every profile x JD in the corpus x language, that every fact is the CV's (role, company, dates, a bullet, a skill, the summary's first sentence) or the post's position, and every other part is the letter's wording.
- In the workspace a "CV | Cover letter" switch sits over the preview; the letter has its own PDF, DOCX and copy buttons, rendered on download. It is written again after every edit, so a rewritten bullet shows in the letter too. The API returns its text as `coverLetter`.
- Tests: `CoverLetterGeneratorTests` (unit), `CoverLetterInvariantTests`, `CoverLetterApiTests` (only on request, follows an edit, one-page PDF and DOCX, API field), E2E `CoverLetterTests`.

### EDIT-04: CV output language
**Status:** Done · **Priority:** P2 · **Effort:** M

- [x] English or Spanish section headings, dates and summary phrasing

Verify: document tests for both languages.

Done notes:
- `GenerateRequest.Language` (`en` or `es`, `ApiConstants.Languages`; anything else is a 400) is kept on the CV (`GeneratedCV.Language`), so the preview, the PDF, the DOCX, the text export and an edited CV's downloads all follow it.
- `Features/Documents/CvLabels.cs` holds the words a CV prints around the profile's content: section titles (each template picks the formal, short or executive summary title), skill categories, "GPA"/"Promedio", "Technologies"/"Tecnologías", and dates. English keeps the profile's dates as written; Spanish rewrites the formats the date parser knows ("Jan 2020" is "ene. 2020", "Present" is "Actualidad") and leaves anything else ("Spring 2019") alone. The HTML gets `lang="es"`.
- The summary is phrased in Spanish ("... con 6+ años de experiencia, con foco en Go, Kafka y PostgreSQL", "y" becomes "e" before an /i/ sound). Role titles stay as they are (tech titles are usually English in Spanish CVs too, and a translated title would have to pick a grammatical gender). The "proven track record" sentence is left out in Spanish, because it quotes a bullet and the profile's text is never translated.
- The UI option is in step 3 next to the page options, and is remembered with the other settings and in the history (APP-02 restores it).
- Tests: `CvLabelsTests`, `SpanishSummaryTests` (unit), `LanguageAndStyleTests` (HTML of every template in both languages, DOCX and text in Spanish, a Spanish PDF), `StyleOptionsApiTests` (a Spanish request, unknown values refused), E2E `A_spanish_executive_cv_has_spanish_titles_in_the_preview`.

### EDIT-05: More templates
**Status:** Done · **Priority:** P3 · **Effort:** M

- [x] "Compact" and "Executive" templates on a shared partial

Verify: TEST-04 across new templates.

Done notes:
- Compact (`CVTemplate.Compact`): dense single column (9pt, a smaller spacing step, name and contact on one band, skills right after the experience) for a long history on one page. Executive (`CVTemplate.Executive`): serif, a large name over an accent rule, uppercase spaced titles, the profile set off by an accent bar. No small capitals: synthesized small caps broke the PDF text layer that an ATS reads.
- Both are built from the shared partials (UX-10) and get every TEST-04 check through `Enum.GetValues<CVTemplate>()`: valid PDF, page count over the whole corpus, every bullet and title in the extracted text, reading order, DOCX, HTML without Mustache tags. `DocumentSupport.ExpectedTitles` now derives the titles from each template's order and labels, in either language.

### EDIT-06: Plain-text / Markdown export
**Status:** Done · **Priority:** P3 · **Effort:** S

- [x] Copy-to-clipboard and `.md`/`.txt` download for ATS paste-in fields

Verify: unit test of the text renderer.

Done notes:
- `Features/Documents/CvTextRenderer.cs` writes the CV as plain text or Markdown: same sections and order as the DOCX, nothing that is not in the CV, missing fields leave no stray separator, Markdown escapes the characters it would read as formatting (`C#`, `*nix`). The DOCX now shares its separators and labels (`DocumentConstants.DetailSeparator`, `DateRangeSeparator`, `GpaLabel`).
- `Components/Shared/TextExport.razor` sits in the preview header (`CVPreview` takes an `Actions` fragment): "Copy text" (plain text, `cvApp.copyText`; a refused clipboard points to the .txt), ".md" and ".txt". Built in the circuit from the generated CV, so nothing is stored in the session or added to the API.
- Tests: `CvTextRendererTests` (unit: order, contact line, separators, empty sections, Markdown headings and escaping, every bullet and skill of every fixture CV over every JD), `TextExportTests` (bUnit: clipboard text, refused copy, both downloads, disabled while regenerating).

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
**Status:** Done · **Priority:** P2 · **Effort:** S

- [x] Sample picker skips posts that are not a normal job (accelerators, programs, fellowships) and prefers posts with a clear level in the title; the Full stack sample (an accelerator post) and the Junior sample (Associate DevOps) are the cases that triggered it
- [x] Run the corpus refresh workflow once from the Actions tab and review the PR it opens; fix whatever the first real run shows
- [x] Decide whether to enable the weekly cron (the user turns it on, see the workflow file)

Verify: `CorpusTests` samples test updated to the new rule; the first run's PR exists and `check` passes on it.

Progress notes:
- The picker leaves out titles with "accelerator", "fellowship", "residency", "apprenticeship" or "bootcamp" and prefers titles that name a clear level (`Classifier.NamesClearLevel`: junior, new grad, senior, staff and so on; not "Associate" or "Engineer 3"). New samples: Backend MongoDB (Senior), Frontend GitLab (Staff), Full stack GitLab (Staff Fullstack), Data Elastic (Lead Data Engineer), DevOps Databricks (Senior Production Engineer), Junior Palantir (New Grad). Tests: `CorpusTests.Samples_are_normal_jobs_with_a_clear_level_in_the_title`, `CollectorPartsTests.Sample_picker_skips_programs_and_prefers_a_clear_level`.
- New collector command `samples`: re-picks the samples from the committed corpus without the network (needed whenever the parser or the dictionary changes, since the picker ranks by parsed skills).
- First run on GitHub (2026-10-21, run 39670556900, PR #12): added 39, updated 14, removed 39, unchanged 141; `check` passes ("194 postings, all inside the 90-day window, samples in sync"), the expired Palantir post is gone, title level agreement 177 of 194 (91%). Two samples changed (Backend: Coinbase Staff, DevOps: Asana Senior). On the PR branch the full suite without browsers (9061 passed, 1 skipped) and the E2E suite (47) pass locally.
- What the first run showed: CI did not run on PR #12, because GitHub does not start `pull_request` workflows for a PR opened with `GITHUB_TOKEN`. Fixed: `ci.yml` also takes `workflow_dispatch`, and the refresh workflow (now with `actions: write`) starts it on its branch after opening the PR, so the checks show on the PR.
- Cron decision (2026-10-21, repo owner): no weekly cron. The refresh stays manual from the Actions tab; the schedule stays commented out in the workflow file. Run it before `check` fails on posts that age out of the 90-day window.

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
**Status:** Won't do (decided in M5) · **Priority:** P3 · **Effort:** S

- ~~Scoring weights and limits configurable via `appsettings.json`~~

Why not: `CLAUDE.md` keeps the values that tune the CV logic as constants and only the values an operator tunes per deployment (rate limit, session timeout) as settings. The weights are proven by the invariant tests, the corpus report and (TEST-08) the snapshots, all of which read the constants: a weight changed in `appsettings.json` on the server would change every score with none of those tests seeing it. There is no operator who needs to tune them, and a weight change is a code change reviewed with its diff.

### ENG-05: Chromium readiness + warm-up
**Status:** Done · **Priority:** P2 · **Effort:** S

- [x] Health check reports PDF engine readiness
- [x] Browser launched at startup in the background to cut cold-start time on Render

Notes: `GET /api/cv/health` adds `pdfEngine`: `ready`, `starting` (not launched yet, or launching) or `unavailable` (the last launch failed, or Chrome was closed). It still answers 200 in every state, so Render's health check does not take the instance out while Chrome starts. `Features/Documents/PdfEngineWarmUp` (a hosted service) launches the shared Chrome in the background at startup and only logs a failure. A failed launch is no longer cached: before, a faulted launch task was kept and every later PDF failed until a restart; now the next PDF launches again. API tests turn the warm-up off (`ApiApp`), so only the tests that render a PDF start Chrome. Cold-start timing is measured on the first deploy (OPS-06).

Verify: `CvApiTests` (health field, ready after the warm-up), `PdfEngineTests` (a failed launch reports unavailable, the next PDF relaunches).

### ENG-06: Modern OpenAPI
**Status:** Done · **Priority:** P3 · **Effort:** S

- [x] Replace Swashbuckle 6.5 with built-in `MapOpenApi` + Scalar UI (or update Swashbuckle)

Notes: `Microsoft.AspNetCore.OpenApi` 10.0.13 and `Scalar.AspNetCore` 2.18.2 replace `Swashbuckle.AspNetCore` 6.5.0 (H11). Development only, as before: the document is at `/openapi/v1.json` and the reference page at `/scalar`. Paths keep the declared controller name (`/api/CV/...`); routing ignores case, so `/api/cv/...` keeps working.

Verify: `CvApiTests.OpenApi_document_and_its_reference_page_are_served_in_development`.

---

## E9. DevOps and portfolio presentation

### OPS-01: GitHub Actions CI
**Status:** Done · **Priority:** P0 · **Effort:** M

- [x] On push and PR: restore, build, unit + integration tests (coverage arrives with TEST-10)
- [x] Docker image build
- [x] E2E job (Playwright) in `ci.yml`, separate from the unit job (green on GitHub since PR #14)
- [x] Status badge in README

Notes: `.github/workflows/ci.yml` runs the same commands that pass locally (restore, build and test in Release). PDF tests use the Chrome preinstalled on the runner through `PUPPETEER_EXECUTABLE_PATH`. Verified on PR #7: build and test (3389 passed, 6 skipped, about 2 minutes) and the Docker image build both green.

Verify: green run on a PR.

### OPS-02: Dependabot
**Status:** Done · **Priority:** P2 · **Effort:** S

- [x] NuGet, GitHub Actions and Docker ecosystems

Notes: `.github/dependabot.yml`, weekly, one grouped PR per ecosystem (NuGet covers every project of the solution). Each PR runs `ci.yml`.

### OPS-03: README rewrite
**Status:** Todo · **Priority:** P1 · **Effort:** M

- [ ] Hero screenshot / GIF, badges (CI, coverage, .NET 10). The live demo link moved to OPS-06 (M5.1): there is no deploy yet.
- [ ] Architecture diagram, "How matching works", testing strategy section

### OPS-04: PROJECT_OVERVIEW refresh
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Fix the drift (F10) and document the new features

### OPS-05: Privacy note
**Status:** Todo · **Priority:** P2 · **Effort:** S

- [ ] Page or footer note: no CV data stored server-side; session files expire after 10 minutes; custom profiles live only in the browser

### OPS-06: Deploy to Render + live demo link
**Status:** Todo (M5.1) · **Priority:** P1 · **Effort:** M

- [ ] First deploy from `render.yaml` (Docker, free plan), health check green
- [ ] Cold-start timing with and without the PDF warm-up (ENG-05)
- [ ] Live demo link and badge in the README (moved from OPS-03)
- [ ] Rate limit, security headers and the 413 body limit checked on the deployed instance (TestServer does not enforce the body limit)

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

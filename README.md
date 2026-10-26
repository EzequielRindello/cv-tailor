# CV Tailor

[![CI](https://github.com/EzequielRindello/cv-tailor/actions/workflows/ci.yml/badge.svg)](https://github.com/EzequielRindello/cv-tailor/actions/workflows/ci.yml)
[![Coverage](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FEzequielRindello%2Fcv-tailor%2Fmaster%2Fdocs%2Fcoverage-badge.json)](features.md#test-10-coverage)
[![Live demo](https://img.shields.io/badge/demo-cv--tailor--9z6e.onrender.com-0f766e)](https://cv-tailor-9z6e.onrender.com)
[![.NET 10](https://img.shields.io/badge/.NET-10-512bd4)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

**Tailored to every job. Nothing invented.** Paste a job description and get a CV built only from your real
experience: the app picks the roles, bullets, skills and projects that match the post, writes a summary from your own
profile, scores the result, explains every choice, and exports PDF, DOCX, Markdown or plain text, with a cover letter
if you want one.

It is a single ASP.NET Core 10 app (Blazor Server UI and REST API in one deploy) with no database and no account.
Your profile and history stay in your browser.

![The results workspace: CV preview next to the match score](docs/screenshots/workspace.png)

> **Live demo: [cv-tailor-9z6e.onrender.com](https://cv-tailor-9z6e.onrender.com)**. It runs on Render's free plan, so the first
> visit after a quiet spell has to wake it up first (Render's cold start); then it answers right away.

---

## What it does

- **Reads the job post.** A skill dictionary tuned on about 200 real posts (`Data/skills.json`) finds the skills,
  keywords, seniority and years asked for, and ignores ordinary English that looks like a skill ("the next
  generation" is not Next.js).
- **Selects, never writes.** Roles, bullets, skills and projects are ranked against the post and the best ones are
  kept. The summary is built from the profile's own title and years. Invariant tests check, for every profile and post,
  that every fact in the output exists in the profile.
- **Fits the page for real.** One or two pages, measured on the rendered PDF, not estimated.
- **Explains itself.** Why each item was chosen, a match score with its breakdown, missing skills, keyword coverage,
  an ATS check with fixes, and the job post with every skill highlighted and linked to where it appears in the CV.
- **Lets you finish it.** Edit the CV with a live preview: leave bullets out, rewrite or reorder them, move sections,
  add back what the selection left out, and watch the scores follow. Edit the cover letter too. Five templates, six
  accents, four font pairings, English or Spanish headings.
- **Tracks applications.** The last 50 CVs stay in your browser with their status, company and notes.

| Compose | Edit with a live preview |
|---|---|
| ![Composer](docs/screenshots/composer.png) | ![Editor](docs/screenshots/editor.png) |
| **A cover letter that names the company** | **The post, highlighted (dark mode)** |
| ![Cover letter editor](docs/screenshots/letter.png) | ![Job post tab](docs/screenshots/job-post-dark.png) |

---

## How matching works

1. **Parse.** `JobDescriptionParser` finds the dictionary skills (with aliases such as `k8s` for Kubernetes), the
   keywords, the seniority (from the title, or from the wording when the title names no level) and "N years of X"
   requirements.
2. **Score the match.** Skills count 50%, keywords 30% and seniority 20%. Under 40% the CV is flagged as a weak fit.
3. **Rank.** `RelevanceScorer` scores each role by matching skills, keywords, recency and seniority; `CVSelector`
   keeps the top roles and, inside each, the bullets with the most matching skills, metrics and strong verbs.
4. **Fit.** The selection starts from a character estimate, then the PDF is rendered and trimmed until its page count
   fits.
5. **Analyze.** Keywords, ATS rules and the highlighted post are computed on the final CV, so what you see is what
   you download.

Every weight and threshold is a named constant. There is no model in the loop: the same input gives the same CV.

---

## Architecture

```mermaid
flowchart LR
    UI["Blazor Server UI"] --> S
    API["REST API /api/cv/*"] --> S
    S["CVGeneratorService<br/>validation, orchestration"] --> M["Matching<br/>parser, skill dictionary"]
    S --> SEL["Selection<br/>ranking, summary, page fit"]
    S --> A["Analysis<br/>match, ATS, keywords, gaps"]
    S --> D["Documents<br/>HTML, PDF, DOCX, text"]
    S --> CL["Cover letters"]
    D --> CH["Headless Chrome<br/>(PuppeteerSharp)"]
    UI --> LS[("Browser localStorage<br/>profile, history, settings")]
```

The UI calls the service in-process; the API serves other clients. The domain lives in `ErCv.Web/Features/<Name>/`,
one folder per feature. [`PROJECT_OVERVIEW.md`](PROJECT_OVERVIEW.md) has the request flow, the project structure,
the data shapes and the design decisions; [`CLAUDE.md`](CLAUDE.md) has the coding conventions.

---

## Testing strategy

About 15,600 tests, most of them run over every profile and job post (hand-written cases plus one real post per role
family from the corpus).

| Layer | What it proves |
|---|---|
| Unit | each parser, scorer, selector and renderer on its own |
| Invariants | the product promise: nothing invented, order and limits respected |
| Documents | valid DOCX, PDF text and page count for every template |
| API | endpoints, validation, limits, sessions, rate limit |
| Snapshots | the generated CV and every score, as approved JSON: a scoring change is a diff to review |
| Components | bUnit tests of the editor, selectors, score cards and history |
| Browser | Playwright: the whole flow, downloads, editing, mobile widths, axe accessibility scans in light and dark |

CI runs everything on each push and PR, with line coverage of the domain and service (the job fails under 95%), plus
the browser tests and the Docker build.

```
dotnet test ErCv.Tests --filter "Category=Component|Category=Unit"   # seconds
dotnet test ErCv.Tests --filter "Category!=E2E"                      # the full suite, about 7 minutes (PDF page counts)
dotnet test ErCv.Tests --filter Category=Snapshot                    # approve a change with ERCV_SNAPSHOTS=approve
dotnet test ErCv.E2E                                                 # browser tests, uses an installed Chrome or Edge
```

The README screenshots come from the running app:
`ERCV_SCREENSHOTS=docs/screenshots dotnet test ErCv.E2E --filter Category=Screenshots`.

---

## Run it

```
dotnet run --project ErCv.Web          # http://localhost:5000, or set PORT
```

PDF export needs Chrome: set `PUPPETEER_EXECUTABLE_PATH` to an installed Chrome, or PuppeteerSharp downloads one on
first use. With Docker (the image Render builds, Chrome included):

```
docker build -t cv-tailor .
docker run --rm -m 512m -e PORT=10000 -p 10000:10000 cv-tailor
```

In Development the OpenAPI document is at `/openapi/v1.json` and its reference page at `/scalar`.

**Deploy:** one Docker web service on Render, defined in `render.yaml`, health check at `/api/cv/health`.

**Your own profile:** open *My profile*, start from a sample, from your resume (PDF or Word, read into a draft you
check) or from a JSON file, and save it. It stays in your browser.

---

## Job-post corpus

`corpus/` holds real software job posts from the public Greenhouse, Lever, Ashby and Workable job-board APIs, dated
and linked to the original. They feed the samples in the UI, the tests and `corpus/report.md` (skill trends, parser
gaps). A refresh is run by hand and reviewed as a PR:

```
dotnet run --project ErCv.Collector -- refresh | report | samples | check
```

Sources, terms and how to remove a post: [`corpus/README.md`](corpus/README.md).

---

## Privacy

No account, no database, no analytics. Your profile, history and settings stay in your browser; the server holds a
CV in memory only while it builds it. The details are on the app's Privacy page (`/privacy`).

---

## Roadmap and version

Current version: **2.0.6.3** (M5.3 Deploy). Each milestone ships as one version; the roadmap, the findings and the
reasoning behind each decision are in [`features.md`](features.md).

---

## License

[MIT](LICENSE). Security problems: see [`SECURITY.md`](SECURITY.md). Contributions: [`CONTRIBUTING.md`](CONTRIBUTING.md).

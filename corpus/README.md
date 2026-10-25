# Job-post corpus (APP-05)

Real, recent software job posts, one JSON file per post, collected from public job-board APIs. The app's sample job descriptions (APP-03) are picked from here, and the parser, scoring and invariant tests run over these posts.

The posts belong to the employers who published them. They are kept here only to test and demo the app, with the fewest fields that job needs and a link back to the original. They are not covered by the repository's license. See "Removal" below.

## Layout

| Path | What it is |
|---|---|
| `sources.json` | Boards to collect (source, board token, company name), the freshness window and the limits |
| `postings/<source>-<board>-<id>.json` | One post: `id`, `source`, `board`, `sourceId`, `url`, `company`, `title`, `postedAt`, `retrievedAt`, `roleFamily`, `seniority`, `location`, `text` |
| `takedowns.json` | Posts or whole boards that must never be collected again |
| `manifest.json` | Count, date range and the time of the last refresh that changed something |
| `report.md` | Relevance report (skill frequency over time, parser gaps, seniority wording) |
| `../ErCv.Web/Data/sample-jobs.json` | The six APP-03 samples, written by `refresh` and `samples` (embedded in the web app) |

## Commands

The collector is a console tool (`ErCv.Collector`), not part of the web app or its Docker image.

```
dotnet run --project ErCv.Collector -- refresh     # the refresh command: safe to run again
dotnet run --project ErCv.Collector -- report      # rewrite report.md from the committed posts
dotnet run --project ErCv.Collector -- samples     # re-pick the app samples from the committed posts (no network)
dotnet run --project ErCv.Collector -- check       # every rule below, measured from today
dotnet run --project ErCv.Collector -- remove <id-or-url> --reason "<why>"
```

`refresh` is run by hand and its result is reviewed in a PR like any other change. It:

1. Reads each board in `sources.json` with one request (all four APIs return every published post of a board in one response).
2. Keeps software roles only (individual contributors: no sales, management, design or hardware), listed, with a publish date inside the **90-day window**, at least 400 characters of text, and not in `takedowns.json`.
3. Keeps at most 6 posts per board: the newest, spread over role families and seniority. A role posted once per city with the same title is kept once.
4. Drops posts that expired, closed at the source, or were taken down. If a board cannot be read, its posts stay until they expire.
5. Leaves an unchanged post's file untouched (its `retrievedAt` is the first time that text was seen), re-picks the samples, and only rewrites `manifest.json` when something changed. Running it twice in a row changes nothing the second time.

`postedAt` is the source's own publish date: Greenhouse `first_published`, Lever `createdAt`, Ashby `publishedAt`, Workable `published_on`. A post without one is skipped.

Text is the post body as plain text: paragraphs and "- " bullets, no HTML, em dashes turned into hyphens (project rule), cut at a paragraph boundary after 15,000 characters. Nothing else is kept: no apply links, application questions, compensation fields, internal ids, departments or metadata.

## How the collector behaves

- User-Agent: `ErCvCorpusCollector/1.0 (+https://github.com/EzequielRindello/cv-tailor)`.
- `robots.txt` is read once per host before anything else. Rules for this agent, else for `*`, are honored (longest match wins). As in RFC 9309, a 4xx answer means no rules and a 5xx or no answer means the host is skipped.
- At most one request per host every 1.5 s, or the host's `Crawl-delay` if longer. Redirects are not followed (they could reach a host whose `robots.txt` was not read). 60 s timeout, 64 MB response cap.
- Only the documented public endpoints below, no pages are scraped, nothing is sent but GET requests.

## Sources and terms

Reviewed on 2026-10-20. None of these APIs publishes a license for third-party reuse of the post text; all four say the data is public and unauthenticated. That is why the corpus keeps little, links back, stays small and honors takedowns.

| Source | Endpoint | Terms and robots.txt | Used |
|---|---|---|---|
| Greenhouse Job Board API ([docs](https://docs.greenhouse.io/job-board.html)) | `boards-api.greenhouse.io/v1/boards/{board}/jobs?content=true` | Docs: "Job Board data is publicly available, so authentication is not required for any GET endpoints"; meant for building job boards. No rate limit published. `robots.txt` only disallows `/embed/`, which is not used. | Yes |
| Lever Postings API ([docs](https://github.com/lever/postings-api)) | `api.lever.co/v0/postings/{site}?mode=json` | Docs: published postings "are publicly viewable" and "may be scraped by third parties". `robots.txt` allows everything with `Crawl-delay: 1`. | Yes |
| Ashby Public Job Posting API ([docs](https://developers.ashbyhq.com/docs/public-job-posting-api)) | `api.ashbyhq.com/posting-api/job-board/{board}` | Docs: public, no key; posts with `isListed: false` are meant to be reached by direct link only, so they are skipped. `robots.txt` answers 401 (no rules). | Yes |
| Workable public jobs endpoint ([help article](https://help.workable.com/hc/en-us/articles/115012771647)) | `apply.workable.com/api/v1/widget/accounts/{account}?details=true` | The help article lists it as a public, tokenless endpoint for published jobs. `www.workable.com/api/accounts/...` redirects to this host. `apply.workable.com/robots.txt` disallows nothing (and signals `ai-train=no`; nothing here trains a model). The customer Terms do not address site visitors. | Yes, few posts (most accounts tried are small) |

Not used:

- **LinkedIn, Indeed, Glassdoor and other job sites' pages**: their terms forbid automated collection, and APP-05 rules out scraping.
- **Greenhouse `/embed/` pages and hosted job pages of any source**: pages, not APIs (and `/embed/` is disallowed by `robots.txt`). Only the JSON APIs are read; the hosted page is only linked.
- **Remote-job feeds (for example Remotive, RemoteOK) and Hacker News "Who is hiring"**: not evaluated for collection. Each has its own terms (attribution rules, user-written comments) and needs its own review before it is added.
- Boards dropped from `sources.json` while building the corpus: `lever/plaid` (404, the board moved) and `lever/mistral` (no posts). `greenhouse/dropbox` and `workable/skroutz` are kept but currently have no eligible posts.

## Removal

Anyone (an employer, a candidate, a source) can ask for a post to be removed, by GitHub issue or by contacting the repository owner. To honor it:

```
dotnet run --project ErCv.Collector -- remove <id-or-url> --reason "requested by <who>"
```

This deletes the file, records the takedown in `takedowns.json` (by id and url) so `refresh` never brings it back, and re-picks the samples if the post was one. To remove a whole company, add `{ "board": "<source>/<board>", "reason": "...", "date": "YYYY-MM-DD" }` to `takedowns.json` and delete the board from `sources.json`, then run `refresh`. Commit the result in a PR.

A removed post is still in the git history. If a request needs it gone from history too, that means rewriting history, which is the repository owner's decision.

# Security

## Reporting a problem

Please report a vulnerability privately, not in a public issue: use **Report a vulnerability** on the repository's
**Security** tab (GitHub private vulnerability reporting). Say what you found, how to reproduce it and what it lets
someone do. You will get an answer within a week.

Only the latest version on `master` (the one deployed as the live demo) gets fixes.

## What there is to protect

- There are no accounts and no database. A visitor's profile, history and settings live in their own browser's
  local storage.
- The server holds a job description, a profile and the generated files in memory only while it builds and serves a
  CV. An uploaded resume is read in memory and dropped. Nothing is written to disk.
- The only outbound requests are to the public Greenhouse and Lever job-board APIs, to read a post from a link the
  visitor pasted (fixed hosts, strict path pattern, no redirects).

## What the app already does

- Every request is validated in one place (`CVGeneratorService`), with size limits on the job description, the
  profile, the resume and the request body, and a per-IP rate limit on the API.
- CV templates only print HTML-escaped values; nothing from a request reaches the CSS.
- The CV preview is a sandboxed iframe without scripts, under a Content Security Policy.
- PDF rendering runs a capped number of headless Chrome pages.
- The container runs as a non-root user.

The full list of rules is in [`CLAUDE.md`](CLAUDE.md#security-rules).

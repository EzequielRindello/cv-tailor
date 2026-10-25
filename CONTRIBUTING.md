# Contributing

Thanks for looking. This is a personal portfolio project, so issues and ideas are welcome, and pull requests are
read, but there is no promise that one is merged.

## Before you open a pull request

- Open an issue first for anything bigger than a fix, so we can agree on the approach.
- Read [`CLAUDE.md`](CLAUDE.md). It holds the conventions every change follows: folder structure, regions, named
  constants, performance and security rules, tests. [`features.md`](features.md) has the roadmap and the reasoning
  behind each decision; [`PROJECT_OVERVIEW.md`](PROJECT_OVERVIEW.md) explains the architecture.
- No new package, download or outbound request without asking first.

## Checks

```
dotnet build er-cv-generator.sln                                     # must stay at 0 warnings
dotnet test ErCv.Tests --filter "Category=Component|Category=Unit"   # seconds
dotnet test ErCv.Tests --filter "Category!=E2E"                      # the full suite, PDF tests start Chrome
dotnet test ErCv.E2E                                                 # browser tests, uses an installed Chrome or Edge
```

- Every behavior change comes with a test in the mirrored folder of `ErCv.Tests`.
- A change to selection, scoring or parsing changes the generation snapshots: review the `.received.json` files
  and approve them with `ERCV_SNAPSHOTS=approve` in the same pull request.
- No em dashes in code, comments, docs or UI text.

CI runs the build, the full suite with line coverage (it fails under 95%), the browser tests and the Docker build on
every pull request.

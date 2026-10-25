## What and why

<!-- What changes, and the issue or features.md item it belongs to. -->

## Checklist (CLAUDE.md)

- [ ] `dotnet build er-cv-generator.sln` has 0 warnings
- [ ] The behavior change has a test in the mirrored folder of `ErCv.Tests`
- [ ] `dotnet test ErCv.Tests --filter "Category!=E2E"` passes
- [ ] Snapshots: any `.received.json` reviewed and approved with `ERCV_SNAPSHOTS=approve`, or none changed
- [ ] Browser tests pass if the UI changed (`dotnet test ErCv.E2E`)
- [ ] New thresholds, limits and repeated text are named constants
- [ ] No new package, download or outbound request (or the owner agreed)
- [ ] No em dashes
- [ ] `features.md` updated (checkboxes, Done notes) when an item changes

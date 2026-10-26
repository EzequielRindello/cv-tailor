# Release notes

Drafts for the GitHub releases, one file per version (OPS-07). One milestone is one version (see "Versioning" in
`features.md`). Nothing was tagged before 2026-10-24; each tag points at the merge commit that shipped its milestone,
so the history reads the same as the releases.

| Version | Milestone | Tag on | PR |
|---|---|---|---|
| v2.0.2 | M1 Foundation | `785e24a` | #7 |
| v2.0.3 | M2 Redesign | `0eb72f3` | #8 |
| v2.0.4 | M3 Your data | `47945b9` | #9 |
| v2.0.4.1 | M3.2 Real-post fixes | `b4f518c` | #11, #13 |
| v2.0.5 | M4 Output | `8594f56` | #14 |
| v2.0.6 | M5 Polish & presentation | `1c6b069` | #15 |
| v2.0.6.1 | M5.1 Smarter input | `68ecef6` | #18, #19 |
| v2.0.6.2 | M5.2 Pre-deploy polish | `f498026` (the M5.2 commit; it ships in the same PR as M5.3) | the M5.2/M5.3 PR |
| v2.0.6.3 | M5.3 Deploy | the merge commit of the M5.2/M5.3 PR | the M5.2/M5.3 PR |

## Creating them (owner)

With the GitHub CLI, from the repository root, once the M5.2/M5.3 PR is merged (replace `MERGE` with its commit):

```
git tag -a v2.0.2 785e24a -m "v2.0.2: M1 Foundation"
git tag -a v2.0.3 0eb72f3 -m "v2.0.3: M2 Redesign"
git tag -a v2.0.4 47945b9 -m "v2.0.4: M3 Your data"
git tag -a v2.0.4.1 b4f518c -m "v2.0.4.1: M3.2 Real-post fixes"
git tag -a v2.0.5 8594f56 -m "v2.0.5: M4 Output"
git tag -a v2.0.6 1c6b069 -m "v2.0.6: M5 Polish & presentation"
git tag -a v2.0.6.1 68ecef6 -m "v2.0.6.1: M5.1 Smarter input"
git tag -a v2.0.6.2 f498026 -m "v2.0.6.2: M5.2 Pre-deploy polish"
git tag -a v2.0.6.3 MERGE -m "v2.0.6.3: M5.3 Deploy"
git push origin --tags
```

Then one release per tag, oldest first so the last one is "Latest". The first line of each file is the title, the
rest is the body (`--notes-file` takes the whole file, so delete the first line from the body in the web UI, or
paste the body by hand):

```
gh release create v2.0.2 --title "v2.0.2: M1 Foundation" --notes-file docs/releases/v2.0.2.md
...
gh release create v2.0.6.3 --latest --title "v2.0.6.3: M5.3 Deploy" --notes-file docs/releases/v2.0.6.3.md
```

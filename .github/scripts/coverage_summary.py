"""TEST-10: line coverage of ErCv.Web/Features and ErCv.Web/Services from a coverlet cobertura report.

Usage: python .github/scripts/coverage_summary.py <coverage.cobertura.xml> [--min 80] [--badge FILE [--write-badge]]

Prints one row per feature folder and for Services, writes the same table to the GitHub job summary when it runs
in Actions, and exits 1 when the total is under --min. Standard library only.

OPS-07: --badge names the shields.io endpoint file the README badge reads (docs/coverage-badge.json). With
--write-badge it is written from this run; without it the run fails when the file is more than BADGE_TOLERANCE points
away from what was measured, so the badge in the README cannot drift from the real number.
"""

import argparse
import json
import os
import sys
import xml.etree.ElementTree as ET

WEB_FOLDER = "ErCv.Web/"

# How far the committed badge may be from the measured coverage (whole points) before CI asks to rewrite it
BADGE_TOLERANCE = 1

# shields.io colors by coverage, the first threshold reached wins
BADGE_COLORS = ((95, "brightgreen"), (90, "green"), (80, "yellow"), (0, "red"))


def area_of(filename):
    """'Features/Matching' or 'Services' for a file the target covers, None for anything else."""
    # coverlet writes paths relative to the project's <source> (ErCv.Web/); absolute ones are cut at ErCv.Web/
    path = filename.replace("\\", "/")
    start = path.find(WEB_FOLDER)
    relative = path[start + len(WEB_FOLDER):] if start >= 0 else path
    if relative.startswith("Features/"):
        parts = relative.split("/")
        return "/".join(parts[:2]) if len(parts) > 2 else None
    if relative.startswith("Services/"):
        return "Services"
    return None


def collect(report):
    # A file can appear in several <class> entries (partial and nested classes); a line counts once, covered if any
    # entry hit it
    covered = {}
    area_by_file = {}
    for cls in ET.parse(report).getroot().iter("class"):
        filename = cls.get("filename", "")
        area = area_of(filename)
        if area is None:
            continue
        area_by_file[filename] = area
        for line in cls.iter("line"):
            key = (filename, line.get("number"))
            covered[key] = covered.get(key, False) or int(line.get("hits", "0")) > 0

    areas = {}
    for (filename, _), hit in covered.items():
        total, hits = areas.get(area_by_file[filename], (0, 0))
        areas[area_by_file[filename]] = (total + 1, hits + (1 if hit else 0))
    return areas


def percent(hit, total):
    return 100.0 * hit / total if total else 100.0


def badge(overall):
    """The shields.io endpoint document for this coverage."""
    color = next(c for floor, c in BADGE_COLORS if overall >= floor)
    return {"schemaVersion": 1, "label": "coverage", "message": f"{overall:.0f}%", "color": color}


def check_badge(path, overall):
    """None when the committed badge is close enough to the measured coverage, else the error to print."""
    try:
        with open(path, encoding="utf-8") as f:
            shown = float(json.load(f)["message"].rstrip("%"))
    except (OSError, ValueError, KeyError) as error:
        return f"Cannot read the coverage badge {path}: {error}"
    if abs(shown - overall) > BADGE_TOLERANCE:
        return (f"The coverage badge says {shown:.0f}% but the run measured {overall:.1f}%. Run this script with "
                f"--badge {path} --write-badge on a fresh coverage report and commit the file.")
    return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("report")
    parser.add_argument("--min", type=float, default=0.0)
    parser.add_argument("--badge")
    parser.add_argument("--write-badge", action="store_true")
    args = parser.parse_args()

    areas = collect(args.report)
    total = sum(t for t, _ in areas.values())
    hit = sum(h for _, h in areas.values())
    overall = percent(hit, total)

    rows = ["| Area | Lines | Covered | Coverage |", "|---|---:|---:|---:|"]
    for area in sorted(areas):
        t, h = areas[area]
        rows.append(f"| {area} | {t} | {h} | {percent(h, t):.1f}% |")
    rows.append(f"| **Features + Services** | **{total}** | **{hit}** | **{overall:.1f}%** |")
    table = "\n".join(rows)

    print(table)
    print(f"coverage={overall:.0f}")

    summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary:
        with open(summary, "a", encoding="utf-8") as out:
            out.write(f"## Line coverage (minimum {args.min:.0f}%)\n\n{table}\n")

    if overall < args.min:
        print(f"Coverage {overall:.1f}% is under the {args.min:.0f}% minimum", file=sys.stderr)
        return 1

    if args.badge and args.write_badge:
        with open(args.badge, "w", encoding="utf-8", newline="\n") as out:
            json.dump(badge(overall), out)
            out.write("\n")
    elif args.badge:
        error = check_badge(args.badge, overall)
        if error:
            print(error, file=sys.stderr)
            return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())

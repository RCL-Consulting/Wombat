"""List every design/baseline/... path named in design/BRIEF.md and design/flows/*.md and check each exists.

A path is recognised as act-1..act-6/act-A/states followed by /<name>, with or without .png.
- name.png            -> exact file
- name ending in *    -> glob, must match >= 1 file
- name without .png   -> name.png must exist (a bare stem is how prose names a capture)
Also expands the BRIEF § 10 shorthand "`home--x`, `--y`" (a leading -- continues the previous page stem).
Usage: python check_baseline_paths.py [--verbose]
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the repo root
BASE = os.path.join(ROOT, "design", "baseline")
DOCS = [os.path.join(ROOT, "design", "BRIEF.md")] + sorted(glob.glob(os.path.join(ROOT, "design", "flows", "*.md")))

PATH_RE = re.compile(r"(?<![A-Za-z0-9_\-])((?:act-[1-6A])|states)/([A-Za-z0-9][A-Za-z0-9._\-]*(?:\*|…)?)")
# a backticked capture stem with no folder, e.g. `home--assessor-empty` or `--assessor-pending`, only in states lists
STEM_RE = re.compile(r"`((?:[a-z0-9][a-z0-9\-]*)?--[a-z0-9][a-z0-9\-<>]*\*?)(?:\.png)?`")
# a backticked step capture with no folder, e.g. `2.8-2` or `A.7.10-*`
STEP_RE = re.compile(r"`((?:A\.)?\d+(?:\.\d+)*-(?:\d+|\*))`")


def exists(folder, name):
    if name.endswith("…"):
        name = name[:-1] + "*"
    if name.endswith("*"):
        return len(glob.glob(os.path.join(BASE, folder, name))) > 0
    if name.endswith(".png"):
        return os.path.isfile(os.path.join(BASE, folder, name))
    # bare stem: name.png, or a unique prefix of a step capture (e.g. 2.8-2 -> 2.8-2-*.png)
    if os.path.isfile(os.path.join(BASE, folder, name + ".png")):
        return True
    return False


def step_exists(stem):
    act = "act-A" if stem.startswith("A.") else "act-" + stem.split(".")[0]
    pat = stem if stem.endswith("*") else stem + "-*"
    hits = glob.glob(os.path.join(BASE, act, pat + ".png")) if not stem.endswith("*") else glob.glob(os.path.join(BASE, act, pat + ".png"))
    # a step capture named <step>-<n>-<slug>.png; `2.8-2` must match 2.8-2-*.png
    return len(hits) > 0


def main():
    verbose = "--verbose" in sys.argv
    missing = []
    total = 0
    for doc in DOCS:
        rel = os.path.relpath(doc, ROOT)
        with open(doc, encoding="utf-8") as f:
            lines = f.readlines()
        for n, line in enumerate(lines, 1):
            for m in PATH_RE.finditer(line):
                folder, name = m.group(1), m.group(2).rstrip(".,;:)")
                if name.endswith(".png") is False and name.endswith(".pn"):
                    name += "g"
                total += 1
                if not exists(folder, name):
                    missing.append((rel, n, f"{folder}/{name}"))
                elif verbose:
                    print("ok", rel, n, f"{folder}/{name}")
            # the § 10 / held-list shorthand: page stems carried from the previous full stem on the line
            if "--" in line:
                prev_page = None
                for m in STEM_RE.finditer(line):
                    stem = m.group(1)
                    if "<" in stem:
                        continue  # a template such as review-detail--<state>
                    if stem.startswith("--"):
                        if prev_page is None:
                            total += 1
                            if not glob.glob(os.path.join(BASE, "states", "*" + stem + (".png" if not stem.endswith("*") else ".png"))):
                                missing.append((rel, n, f"states/*{stem} (orphan shorthand)"))
                            continue
                        stem = prev_page + stem
                    else:
                        prev_page = stem.split("--")[0]
                    if "/" in stem:
                        continue
                    total += 1
                    if not exists("states", stem):
                        missing.append((rel, n, f"states/{stem} (shorthand)"))
            for m in STEP_RE.finditer(line):
                stem = m.group(1)
                total += 1
                if not step_exists(stem):
                    missing.append((rel, n, f"step capture {stem} (shorthand)"))
    print(f"checked {total} path mentions in {len(DOCS)} files; missing {len(missing)}")
    for rel, n, p in missing:
        print(f"MISSING {rel}:{n}  {p}")


if __name__ == "__main__":
    main()


def bare_png_pass():
    """Bare state files with no folder, e.g. `my-reviews--detail.png` in an ATTACHED list."""
    bare = re.compile(r"(?<![A-Za-z0-9_\-/<])([a-z0-9][a-z0-9\-]*--[a-z0-9][a-z0-9\-]*\.png)")
    missing, total = [], 0
    for doc in DOCS:
        rel = os.path.relpath(doc, ROOT)
        for n, line in enumerate(open(doc, encoding="utf-8"), 1):
            for m in bare.finditer(line):
                total += 1
                if not os.path.isfile(os.path.join(BASE, "states", m.group(1))):
                    missing.append((rel, n, m.group(1)))
    print(f"bare state names: {total}; missing {len(missing)}")
    for rel, n, p in missing:
        print(f"MISSING-BARE {rel}:{n}  {p}")


bare_png_pass()

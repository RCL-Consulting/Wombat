"""List every design/baseline/... path named in design/BRIEF.md and design/flows/*.md and check each exists.

A path is recognised as act-1..act-6/act-A/states, or mail/pdf (T336), followed by /<name>, with or without .png.
- name.html/.txt/.pdf  -> exact file (the mail and PDF baselines keep these beside their PNGs)
- name.png            -> exact file
- name ending in *    -> glob, must match >= 1 file
- name without .png   -> name.png must exist, or, in an act folder, a step capture it prefixes (4.45-1 -> 4.45-1-*.png)
- name followed by <  -> a template such as review-detail--<state>.png; not checked
Also expands the BRIEF § 10 shorthand "`home--x`, `--y`" (a leading -- continues the previous page stem on the line,
including one named by a states/ path).

Not reported as missing:
- NOT_TAKEN: captures states.md names that were never taken, which BRIEF § 10 lists and the flows describe in words;
- a backticked `--x` that is a CSS custom property defined in app.css (or a family of them, `--space-*`) or a CLI flag
  (`--no-build`), not a capture.

Exits 1 if anything is missing, so it can gate a change.
Usage: python check_baseline_paths.py [--verbose]
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the repo root
BASE = os.path.join(ROOT, "design", "baseline")
DOCS = [os.path.join(ROOT, "design", "BRIEF.md")] + sorted(glob.glob(os.path.join(ROOT, "design", "flows", "*.md")))
APP_CSS = os.path.join(ROOT, "src", "Wombat.Web", "wwwroot", "app.css")

# Named in states.md, never captured; BRIEF § 10 says so, and F02, F05 and F15 describe them in words.
NOT_TAKEN = {"my-progress--december", "change-password--throttled", "epa-edit--local-reactivated"}
CLI_FLAGS = {"--no-build"}

PATH_RE = re.compile(r"(?<![A-Za-z0-9_\-])((?:act-[1-6A])|states|mail|pdf)/([A-Za-z0-9][A-Za-z0-9._\-]*(?:\*|…)?)")
# a backticked capture stem with no folder, e.g. `home--assessor-empty` or `--assessor-pending`, only in states lists
STEM_RE = re.compile(r"`((?:[a-z0-9][a-z0-9\-]*)?--[a-z0-9][a-z0-9\-<>]*\*?)(?:\.png)?`")
# a backticked step capture with no folder, e.g. `2.8-2` or `A.7.10-*`
STEP_RE = re.compile(r"`((?:A\.)?\d+(?:\.\d+)*-(?:\d+|\*))`")


def css_properties():
    try:
        with open(APP_CSS, encoding="utf-8") as f:
            return set(re.findall(r"^\s*(--[a-z0-9-]+)\s*:", f.read(), re.M))
    except OSError:
        return set()


CSS_PROPS = css_properties()


def is_not_a_capture(stem):
    """A backticked --x that names a CSS custom property, a family of them, or a CLI flag."""
    if stem in CLI_FLAGS or stem in CSS_PROPS:
        return True
    if stem.endswith("*"):
        return any(p.startswith(stem[:-1]) for p in CSS_PROPS)
    return False


def stem_of(name):
    return re.sub(r"\.png$", "", name)


def exists(folder, name):
    if name.endswith("…"):
        name = name[:-1] + "*"
    if name.endswith("*"):
        return len(glob.glob(os.path.join(BASE, folder, name))) > 0
    if name.endswith((".png", ".html", ".txt", ".pdf")):
        return os.path.isfile(os.path.join(BASE, folder, name))
    # bare stem: name.png, or a step capture it prefixes (e.g. act-4/4.45-1 -> 4.45-1-*.png)
    if os.path.isfile(os.path.join(BASE, folder, name + ".png")):
        return True
    if folder.startswith("act-") and re.fullmatch(r"(?:A\.)?\d+(?:\.\d+)*[a-z]?-\d+", name):
        return len(glob.glob(os.path.join(BASE, folder, name + "-*.png"))) > 0
    return False


def step_exists(stem):
    act = "act-A" if stem.startswith("A.") else "act-" + stem.split(".")[0]
    pat = stem if stem.endswith("*") else stem + "-*"
    # a step capture named <step>-<n>-<slug>.png; `2.8-2` must match 2.8-2-*.png
    return len(glob.glob(os.path.join(BASE, act, pat + ".png"))) > 0


def check_paths(verbose):
    missing, not_taken, total = [], [], 0
    for doc in DOCS:
        rel = os.path.relpath(doc, ROOT)
        with open(doc, encoding="utf-8") as f:
            lines = f.readlines()
        for n, line in enumerate(lines, 1):
            prev_page = None
            for m in PATH_RE.finditer(line):
                if m.end() < len(line) and line[m.end()] == "<":
                    continue  # a template such as states/review-detail--<state>.png
                folder, name = m.group(1), m.group(2).rstrip(".,;:)")
                if name.endswith(".png") is False and name.endswith(".pn"):
                    name += "g"
                if folder == "states" and "--" in name:
                    prev_page = name.split("--")[0]
                total += 1
                if folder == "states" and stem_of(name) in NOT_TAKEN:
                    not_taken.append((rel, n, f"{folder}/{name}"))
                elif not exists(folder, name):
                    missing.append((rel, n, f"{folder}/{name}"))
                elif verbose:
                    print("ok", rel, n, f"{folder}/{name}")
            # the § 10 / held-list shorthand: page stems carried from the previous full stem on the line
            if "--" in line:
                for m in STEM_RE.finditer(line):
                    stem = m.group(1)
                    if "<" in stem:
                        continue  # a template such as review-detail--<state>
                    if stem.startswith("--"):
                        if is_not_a_capture(stem):
                            continue
                        if prev_page is None:
                            total += 1
                            if not glob.glob(os.path.join(BASE, "states", "*" + stem + ".png")):
                                missing.append((rel, n, f"states/*{stem} (orphan shorthand)"))
                            continue
                        stem = prev_page + stem
                    else:
                        prev_page = stem.split("--")[0]
                    if "/" in stem:
                        continue
                    total += 1
                    if stem_of(stem) in NOT_TAKEN:
                        not_taken.append((rel, n, f"states/{stem} (shorthand)"))
                    elif not exists("states", stem):
                        missing.append((rel, n, f"states/{stem} (shorthand)"))
            for m in STEP_RE.finditer(line):
                stem = m.group(1)
                total += 1
                if not step_exists(stem):
                    missing.append((rel, n, f"step capture {stem} (shorthand)"))
    print(f"checked {total} path mentions in {len(DOCS)} files; missing {len(missing)}; "
          f"named but never taken (BRIEF § 10) {len(not_taken)}")
    for rel, n, p in missing:
        print(f"MISSING {rel}:{n}  {p}")
    if verbose:
        for rel, n, p in not_taken:
            print(f"NOT-TAKEN {rel}:{n}  {p}")
    return len(missing)


def bare_png_pass():
    """Bare state files with no folder, e.g. `my-reviews--detail.png` in an ATTACHED list."""
    bare = re.compile(r"(?<![A-Za-z0-9_\-/<])([a-z0-9][a-z0-9\-]*--[a-z0-9][a-z0-9\-]*\.png)")
    missing, total = [], 0
    for doc in DOCS:
        rel = os.path.relpath(doc, ROOT)
        with open(doc, encoding="utf-8") as f:
            for n, line in enumerate(f, 1):
                for m in bare.finditer(line):
                    total += 1
                    if stem_of(m.group(1)) in NOT_TAKEN:
                        continue
                    if not os.path.isfile(os.path.join(BASE, "states", m.group(1))):
                        missing.append((rel, n, m.group(1)))
    print(f"bare state names: {total}; missing {len(missing)}")
    for rel, n, p in missing:
        print(f"MISSING-BARE {rel}:{n}  {p}")
    return len(missing)


def main():
    verbose = "--verbose" in sys.argv
    failures = check_paths(verbose) + bare_png_pass()
    sys.exit(1 if failures else 0)


if __name__ == "__main__":
    main()

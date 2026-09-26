"""Re-paste each verbatim runbook step in design/flows/*.md from the runbook as it is now.

A flow quotes a step as a heading line "Step <id> — <label> (<file>.md:<line>)" followed by its Role, Route, Do and Expect
fields, each possibly continued on lines indented two spaces. This rewrites the heading's pointer to the step's current
line and replaces the four fields with the runbook's, leaving anything else in the flow (commentary after the fields,
the next step) as it was. Run check_verbatim_steps.py afterwards.
Usage: python sync_verbatim_steps.py [--dry-run]
"""
import glob
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the repo root
RB = os.path.join(ROOT, "execution", "knowledge", "scenario-paediatrics")
FLOWS = sorted(glob.glob(os.path.join(ROOT, "design", "flows", "*.md")))
FIELDS = ("Role", "Route", "Do", "Expect")
HEAD = re.compile(r"^(\s*(?:#+ *)?Step ([A-Z]?\.?[\d.]+[a-z]?) — .*?)(?: \(([a-z0-9\-]+\.md):(\d+)\))?\s*$")

runbook = {os.path.basename(f): open(f, encoding="utf-8").read().splitlines()
           for f in glob.glob(os.path.join(RB, "*.md"))}


def find_step(sid):
    for name, lines in runbook.items():
        for k, line in enumerate(lines):
            if re.match(rf"^#+ *Step {re.escape(sid)} — ", line):
                return name, k
    return None, None


def field_region(lines, start):
    """Indices [start, end) of the Role..Expect fields beginning at start, with their continuation lines."""
    i = start
    seen = False
    while i < len(lines):
        line = lines[i]
        if re.match(rf"^({'|'.join(FIELDS)}):", line):
            seen = True
            i += 1
            while i < len(lines) and lines[i].startswith("  ") and not re.match(r"^\s*(?:#+ *)?Step ", lines[i]):
                i += 1
            continue
        break
    return (start, i) if seen else (start, start)


def runbook_fields(name, k):
    lines = runbook[name]
    s, e = field_region(lines, k + 1)
    return lines[s:e]


changed_files = 0
for path in FLOWS:
    lines = open(path, encoding="utf-8").read().splitlines()
    out, i, touched = [], 0, 0
    while i < len(lines):
        m = HEAD.match(lines[i])
        if not m:
            out.append(lines[i]); i += 1; continue
        sid = m.group(2)
        name, k = find_step(sid)
        if name is None:
            print("NOSTEP", os.path.basename(path), sid)
            out.append(lines[i]); i += 1; continue
        heading = f"{m.group(1)} ({name}:{k + 1})" if m.group(3) else lines[i]
        s, e = field_region(lines, i + 1)
        new = runbook_fields(name, k)
        if heading != lines[i] or lines[s:e] != new:
            touched += 1
        out.append(heading)
        out.extend(new if e > s else [])
        i = e if e > s else i + 1
    if touched:
        changed_files += 1
        print(f"{os.path.basename(path)}: {touched} step(s) re-pasted")
        if "--dry-run" not in sys.argv:
            open(path, "w", encoding="utf-8").write("\n".join(out) + "\n")
print(f"{changed_files} file(s) {'would change' if '--dry-run' in sys.argv else 'changed'}")

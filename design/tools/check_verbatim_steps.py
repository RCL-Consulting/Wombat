import os
"""Each verbatim step block in a flow: its (file.md:LINE) points at the step's heading, and its Role/Route/Do/Expect equal
the runbook's (whitespace-normalised)."""
import glob, os, re
ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the repo root
RB = os.path.join(ROOT, "execution", "knowledge", "scenario-paediatrics")
FLOWS = sorted(glob.glob(os.path.join(ROOT, "design", "flows", "*.md")))
rb_lines = {os.path.basename(f): open(f, encoding="utf-8").read().splitlines() for f in glob.glob(os.path.join(RB, "*.md"))}

def runbook_step(fname, ln):
    lines = rb_lines[fname]
    out = []
    for l in lines[ln:]:
        if l.startswith("#") or l.startswith("Actual") or l.startswith("Status") or l.startswith("Capture") or l.startswith("Evidence") or l.startswith("Note"):
            break
        out.append(l)
    return out

def fields(block):
    txt = "\n".join(block)
    res = {}
    for k in ("Role", "Route", "Do", "Expect"):
        m = re.search(rf"^\s*{k}:(.*?)(?=^\s*(?:Role|Route|Do|Expect|Actual|Status|Captures?|Evidence):|\Z)", txt, re.S | re.M)
        res[k] = re.sub(r"\s+", " ", m.group(1)).strip() if m else None
    return res

total = bad = 0
for f in FLOWS:
    lines = open(f, encoding="utf-8").read().splitlines()
    for i, l in enumerate(lines):
        m = re.match(r"^\s*(?:#+ *)?Step ([A-Z]?\.?[\d.]+[a-z]?) — .*?(?:\(([a-z0-9\-]+\.md):(\d+)\))?\s*$", l)
        if not m:
            continue
        total += 1
        sid = m.group(1)
        if m.group(2):
            fname, ln = m.group(2), int(m.group(3))
        else:
            hit = [(fn, k + 1) for fn, ls in rb_lines.items() for k, x in enumerate(ls) if re.match(rf"^#+ *Step {re.escape(sid)} — ", x)]
            if not hit:
                print("NOSTEP", os.path.basename(f), sid); bad += 1; continue
            fname, ln = hit[0]
        if fname not in rb_lines:
            print("NOFILE", os.path.basename(f), sid, fname); bad += 1; continue
        head = rb_lines[fname][ln - 1]
        if f"Step {sid} " not in head + " ":
            print(f"HEADING {os.path.basename(f)} step {sid}: {fname}:{ln} is {head[:80]!r}"); bad += 1
            continue
        blk = []
        for l2 in lines[i + 1:]:
            if re.match(r"^\s*(?:#+ *)?Step [A-Z]?\.?[\d.]+[a-z]? — ", l2) or l2.startswith("```") or l2.startswith("## ") or re.match(r"^\[[a-z0-9\-]+\.md\]\s*$", l2) or l2.startswith("--- from") or l2.startswith("The workflow Step"):
                break
            blk.append(l2)
        mine, theirs = fields(blk), fields(runbook_step(fname, ln))
        for k in ("Role", "Route", "Do", "Expect"):
            if mine[k] != theirs[k]:
                bad += 1
                a, b = mine[k] or "", theirs[k] or ""
                j = next((x for x in range(min(len(a), len(b))) if a[x] != b[x]), min(len(a), len(b)))
                print(f"DIFF {os.path.basename(f)} step {sid} {k}: flow={a[max(0,j-40):j+60]!r} | runbook={b[max(0,j-40):j+60]!r}")
print(f"{total} verbatim steps; {bad} problems")

"""Completeness of design/BRIEF.md and design/flows/*.md against coverage.md.

1. Every page template in coverage.md § Pages is in some flow: BRIEF § 8's Pages column (expanding "(+`new`, `{Id}`)"
   and "/**" shorthands) and, separately, named in that flow's own file.
2. Every role heading in coverage.md § Journeys by role is served by some flow (BRIEF § 1's Flows column, and the flow
   file names the role).
3. Every group-3 task appears in BRIEF § 6 or § 7.
4. Every flow's "ask" block names ATTACHED and points to BRIEF.md.
5. Every runbook step id is in some flow file.
"""
import glob
import os
import re

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))  # the repo root
COV = os.path.join(ROOT, "execution", "knowledge", "scenario-paediatrics", "coverage.md")
BRIEF = os.path.join(ROOT, "design", "BRIEF.md")
FLOWS = sorted(glob.glob(os.path.join(ROOT, "design", "flows", "*.md")))
RUNBOOK = sorted(glob.glob(os.path.join(ROOT, "execution", "knowledge", "scenario-paediatrics", "act-*.md"))) + [
    os.path.join(ROOT, "execution", "knowledge", "scenario-paediatrics", "appendix-cross-cutting.md")]

cov = open(COV, encoding="utf-8").read()
brief = open(BRIEF, encoding="utf-8").read()
flow_text = {os.path.basename(f)[:2]: open(f, encoding="utf-8").read() for f in FLOWS}


def section(text, heading, level="## "):
    start = text.index("\n" + heading) + 1
    nxt = text.find("\n" + level, start + len(heading))
    return text[start: nxt if nxt != -1 else len(text)]


# ---- 1. templates
pages = section(cov, "## Pages")
templates = []
for line in pages.splitlines():
    if line.startswith("| `"):
        first = line.split("|")[1]
        templates += re.findall(r"`([^`]+)`", first)
print(f"coverage.md § Pages: {len(templates)} templates")

idx = section(brief, "## 8. The flow index")
brief_pages = {}
for line in idx.splitlines():
    m = re.match(r"\| (\d\d) \| `flows/", line)
    if not m:
        continue
    cells = line.split("|")
    pcell = cells[5]
    toks = []
    base = None
    for t in re.findall(r"`([^`]+)`|\(\+([^)]*)\)", pcell):
        if t[0]:
            if t[0].startswith("/"):
                base = t[0]
                toks.append(t[0])
            elif base:  # a shorthand like `logout-confirm` after /account/login
                toks.append(base.rsplit("/", 1)[0] + "/" + t[0])
        else:
            for s in re.findall(r"`([^`]+)`", t[1]):
                toks.append(base.rstrip("/") + "/" + s)
    brief_pages[m.group(1)] = toks


def norm(t):
    # collapse parameter names so /x/{Id} matches /x/{Id:int}
    return re.sub(r"\{([A-Za-z]+)(?::[a-z]+)?\}", lambda m: "{" + m.group(1).lower() + "}", t)


def covered_by_brief(t):
    hits = []
    nt = norm(t)
    for fl, toks in brief_pages.items():
        for tok in toks:
            ntok = norm(tok)
            if ntok == nt:
                hits.append(fl)
            elif ntok.endswith("/**") and nt.startswith(ntok[:-3]):
                hits.append(fl)
    return sorted(set(hits))


def covered_by_file(t):
    nt = norm(t)
    out = []
    for fl, txt in flow_text.items():
        toks = {norm(x) for x in re.findall(r"`(/[^`\s]*)`", txt)}
        # also route chains like `/a → /b` inside a Route: line
        for line in txt.splitlines():
            if line.startswith("Route:") or " → " in line:
                toks |= {norm(x) for x in re.findall(r"(/[A-Za-z0-9_\-{}:./]+)", line)}
        if nt in toks:
            out.append(fl)
    return sorted(out)


for t in templates:
    b = covered_by_brief(t)
    f = covered_by_file(t)
    if not b or not f:
        print(f"  TEMPLATE {t}: brief§8={b} files={f}")

# ---- 2. roles
jr = section(cov, "## Journeys by role")
roles = re.findall(r"^### (.+?)(?: \(|$)", jr, re.M)
print(f"coverage.md § Journeys by role: {roles}")
tbl = section(brief, "## 1. What Wombat is")
for r in roles:
    key = r.split(",")[0].strip()
    row = [l for l in tbl.splitlines() if l.startswith("| ") and key.split()[0] in l.split("|")[1]]
    print(f"  ROLE {r}: brief§1 row -> {row[0].split('|')[-2].strip() if row else 'MISSING'}")

# ---- 3. group 3
g3 = ["T299", "T306", "T308", "T311", "T314", "T316", "T321", "T322", "T323", "T324", "T325", "T326", "T327", "T328",
      "T330", "T331"]
s6 = section(brief, "## 6. Requirements")
s7 = section(brief, "## 7. Screens")
for t in g3:
    where = [n for n, s in (("§6", s6), ("§7", s7)) if t in s]
    if not where:
        print(f"  GROUP3 {t}: not in §6 or §7")
print("group 3 checked")
if "Coming soon" not in s7:
    print("  Coming soon placeholders not in §7")

# ---- 4. ask blocks
for fl, txt in flow_text.items():
    blocks = [m.group(2) for m in re.finditer(r"^(`{3,})[a-z]*\n(.*?)^\1[ \t]*$", txt, re.S | re.M)]
    asks = [b for b in blocks if re.search(r"^FLOW \d\d", b, re.M)]
    if not asks:
        print(f"  ASK {fl}: no fenced block holding FLOW NN")
        continue
    ask = asks[0]
    issues = []
    if "ATTACHED" not in ask:
        issues.append("no ATTACHED")
    if "BRIEF.md" not in ask and "BRIEF" not in ask:
        issues.append("no BRIEF.md pointer")
    if "WOMBAT CONSTRAINTS" not in ask:
        issues.append("no constraints digest")
    for k in ("GOAL", "AUDIENCE", "SCREENS", "STEPS", "STATES", "REQUIREMENTS", "QUESTIONS", "ASK"):
        if k not in ask:
            issues.append("no " + k)
    print(f"  ASK {fl}: {len(ask.splitlines())} lines; {'; '.join(issues) if issues else 'ok'}")

# ---- 5. steps
steps = []
for f in RUNBOOK:
    steps += re.findall(r"^#+ *Step ([A-Z]?\.?[\d.]+)", open(f, encoding="utf-8").read(), re.M)
alltxt = "\n".join(flow_text.values())
missing = [s for s in steps if not re.search(r"(?<![\d.])" + re.escape(s) + r"(?![\d])", alltxt)]
print(f"runbook steps: {len(steps)}; not named in any flow: {missing}")

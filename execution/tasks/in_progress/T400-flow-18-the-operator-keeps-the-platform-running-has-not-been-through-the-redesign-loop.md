---
id: T400
title: Flow 18, the operator keeps the platform running, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-11-11
started: 2026-11-11
---

# T400 — Flow 18, the operator keeps the platform running, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 18 is the eighteenth flow of the restructure (W-008, BRIEF § 8) and the last flow of
screens: the platform operator (devadmin) reads the Administrator's Home and the health page, creates and keeps
institutions, runs, pauses and reads the scheduled jobs, reads the audit trail and rebuilds progress; an institution's
administrator reads their own institution's audit trail. The health card is the operator's only warning that mail is not
configured (T157, T327).
**Surfaced:** 2026-11-11, after T398 (flow 17) closed; the operator said "flow 18 next".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T398 needed.

## Symptom

The operator's pages (`/`, the Administrator's Home; `/admin/institutions` (+`new`, `{Id:int}`), `/admin/jobs` (+`runs`),
`/admin/audit` (+`{Id:guid}`), `/admin/curriculum-progress`, `/admin/users/{UserId}`, `/placeholder/{Feature}`; also
`/health`) have not been designed for the restructure. Brief: `design/flows/18-platform-operations.md`, written before
flows 01–17; flows 01–17 changed the shell, the forms, the dialogs and the tables it inherits (flow 15 the rebuild page,
flow 12 the user page, flow 16 the 1150px stack), so the brief must be restated against what is there now. Known faults
it takes up: whatever the restated brief finds still open (T327, the System health stubs; the brief's Mode says the
System page must become a real health page or lose its nav item), and what the earlier replays left for this flow
(A.2.10 and A.6.1–A.6.2's known Gaps).

## Root cause

Not a defect with one cause: the pages predate the design system and the brief's requirements (§ Known problems).

## What to build

Run the loop as flow 17 ran it (`design/flows/17-activity-type-builder/`, BRIEF § 11 "Flow 16" and "Flow 17"):
1. **Restate the brief** in flow 17's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–17 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`. The brief says "He" of devadmin: use they/them.
2. **Stage** (`stage_upload.ps1 -Flow 18`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 18") on the Wombat design system (version 52), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** wireframes first (the brief's Mode), then fidelity; pick; a review of the boards against the code and
   the runbook; the operator's decisions; one correction round (its own canvas if over about 200 boards); check it
   item by item.
6. **Build** in waves (flow 17's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 17's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; sweep new tables 900–1100px against their container; after any width change, read every
   column of every list it reaches at 1280 for each role, and sweep 390–1440; re-take the flow's states. Start replay
   apps detached; act prompts forbid reading an earlier flow's `replay-pw.env`.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/18-platform-operations/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 18) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 18, the baseline is re-captured, and
  flow 18's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T398 (flow 17), T394 (flow 16); T157, T302, T327; BRIEF § 4, § 6–8, § 11; INFRASTRUCTURE.md, HANDOVER.md.

## Notes

- **2026-11-11.** Filed and started on the operator's word ("yes, flow 18 next"); master pushed first (`70657d6c`).
- **2026-11-11, steps 3–4 (ahead of step 1, as T398).** Canvas "Wombat 18": https://claude.ai/artifact/FE7WSRNTmSp88bvS4dZRGV,
  created from the Design type, empty boards; design system v52 (1794377277-5687) installed: `tokens.json`,
  `components/bundle.css` and the three fonts copied from the system, `tokens.css` by hand (flow 17's, header updated;
  its 54 properties match app.css's `:root`, `--shadow-raised` resolved). The mark copied in as
  `/_blob/b3b9ce265597292edc272896bd70a693`. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\c18`.
- **2026-11-11, steps 1–2** (one Opus agent, `337a6675`): brief restated in flow 17's layout (they/them throughout);
  `check_flow_completeness.py` exit 0; `check_verbatim_steps.py` 0 problems for flow 18; `check_baseline_paths.py`
  missing 0; staged 30 captures (all opened: payloads `[REDACTED]`, no link, token or password), published here into the
  canvas as `project/ref/baseline/…` (version 4); `round-1-ask.txt` (636 lines). Gone: the System stub and its nav item,
  the Maintenance card, ShortCode's required text, T302's Deactivate, the institution's and Curriculum progress's failure
  words, nine jobs (eight since T379), 4.38's failed entries. Remain: T327, T157, T324, T325/T384, T326, T286, T277, T328,
  T264, T323, T329's kind, T378, T114, T274; new: the jobs page's result below the table, "dispatched." for a finished
  run, Run now not refusing a disabled job, the jobs table wrapping at 1280, Back links beside the trail, the audit
  entry's h3 with no h2 and its empty state for an unknown id; the Api host's `/health` checks no database (unfiled).
  BRIEF § 8's row no longer lists `/placeholder/{Feature}`. **Waiting:** the operator's Q1–Q5 (before round 1), then
  Q6–Q12 (brief § 6). The old frame's `states/` captures are marked "read their bodies only".


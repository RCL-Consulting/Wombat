---
id: T364
title: Flow 08, committee panels and the schedule, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-12
started: 2026-10-12
---

# T364 — Flow 08, committee panels and the schedule, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 08 is the eighth flow of the restructure (W-008, BRIEF § 8): panels, what is due, and the
schedule of sittings, for the admins, the Coordinator and the committee.
**Surfaced:** 2026-10-12, flow 07 closing (T361); the operator said "flow 08".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T361 needed.

## Symptom

The panels pages (`/committee/panels`, `new`, `{PanelId}`), Decisions due, the committee reviews list and scheduling a
review have not been designed for the restructure. Brief: `design/flows/08-committee-calendar.md`. BRIEF § 8 marks the
flow F/W: its structural round may be a single proposal to confirm where the structure holds.

## What to build

Run the loop as flow 07 ran it (`design/flows/07-review-sitting/`, BRIEF § 11 "Flow 06" and "Flow 07"):
1. **Restate the brief** in flow 07's layout, from read-only audits of the code and the captures; check the pages table
   against the code, including what flow 07 already changed on `/committee/reviews` and `/committee/decisions-due`
   (D53: Not decided, no Deferred); run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 08`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** from the main app's Design page on the Wombat design system (version 19); the operator gives its link.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure, pick, fidelity; a review of the boards against the code and the runbook; the operator's
   decisions; one correction round; check it item by item.
6. **Build** in waves (flow 07's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (`design/flows/07-review-sitting/replay-brief.md`); keep capture
   names; `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit. (`c6740da1`; exit 0, baseline
  paths missing 0, verbatim 424 steps 0 problems)
- [x] The canvas is created, with the mark copied in before round 1 — its link. ("Wombat 08",
  https://claude.ai/artifact/Bvi2pBNN4WBWc4HqwnRRtQ, design system installed; mark `/_blob/d8e89f2bacfcd9a2e79781ca6f135313`, 2026-10-12)
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/08-committee-calendar/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 08) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 08, the baseline is re-captured, and
  flow 08's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T361 (flow 07), T362, T363, T309 (the scheduling preview's seat warning), T329; W-008; D53; BRIEF § 8, § 11.

## Notes

- **2026-10-12, steps 1–2.** Restated from two read-only audits (code at `b62997a5`; every capture opened). Staged
  with `stage_upload.ps1 -Flow 08` (87 files, 6.2 MB, nothing to crop); `design/flows/08-committee-calendar/round-1-ask.txt`
  is § 1 + § 8 as one paste, with `/_blob/MARK` until the mark is copied into the canvas. Waits for the operator's canvas link.
- **2026-10-12, round 1 picked: A** (two pages, the schedule a form page of its own at `/committee/reviews/new`), with
  B's open-review line when one trainee is filtered; the panels proposal confirmed (the seat picker, the panel named on
  its page). The operator said "proceed" on the recommendations: a Missed row of an ended semester schedules for the
  current semester (C5); both panel rules kept; both semester labels kept with one line on the schedule (D40);
  T309, T259, T260, T329, T338, T324, T272 and T280 folded into the build; pre-graduation help worded (C1). Round 1
  gave no step counts (C14 asks again). All in `design/flows/08-committee-calendar/round-2-ask.txt`.
- **2026-10-12 to 10-13, round 2 reviewed** from three sides (`951b855d`, `round-2-review.md`): ten must-fixes (the
  wrong semester EPAs drawn, the brief's fault, now corrected), F1–F6 put to the operator, who took every
  recommendation (2026-10-13): **F1 reverses Q7**, a Missed row schedules its own ended semester (an S2 review cannot
  decide an S1 EPA: `AgendaPlanner.cs:210`, `DecisionWindowRecords.cs:125`); Show; the browser's date input; type
  preselected; races tested, not replayed; the S2 line reworded. Round 3 is `round-3-ask.txt`.
- **2026-10-13, round 3 drawn and checked** (`round-3/`, `round-3-check.md`): F1–F6, M1–M10 and S1–S8 done; the design
  is final. F4's S1 default is drawn Entrustment-only; the code's `DefaultFor` makes it Annual progression (the build
  moves it). Next: step 6, the build.

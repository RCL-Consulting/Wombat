---
id: T342
title: Flow 03, a registrar asks for an assessment or logs one, has not been through the redesign loop
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-28
started: 2026-09-28
completed: 2026-09-30
---

# T342 — Flow 03, a registrar asks for an assessment or logs one, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 03 is the third flow of the restructure (W-008, BRIEF § 8), and the trainee's most frequent
job: filing an observation, 25 a semester.
**Surfaced:** 2026-09-28, flow 02 closing (T339).
**Model:** Opus. Design rounds, a review, a multi-lane build and an acceptance replay, as T335 and T339 needed.

## Symptom

`/activities/new`, the activity page, My activities and the inbox's trainee side still wear the pre-restructure
language inside the new shell. Their brief (`design/flows/03-trainee-files-activity.md`) predates the pilot's
restatement (BRIEF § 2.4): its ASK line is the old one.

## What to build

Run the loop as flow 02 ran it (`design/flows/02-sign-in-and-account/`, BRIEF § 11 "Flow 02"):
1. **Restate the brief** (flow 02's § 1 is the model): structure-first ASK; flow 03 is marked W, so round 1 asks for 2–3
   structural variations; check the pages table against the code; prefer new-shell `act-*` captures, opened and
   checked; run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 03`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** from the main app's Design page on the Wombat design system (version 7); the operator gives its link.
4. **Copy the mark** into it (`asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`) before round 1.
5. **Rounds:** structure, pick, fidelity; a four-sided review of the boards against the code and the runbook (one
   reviewer on the code, one on the cast); the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (`design/flows/02-sign-in-and-account/build-lanes.md` as template; contracts named in both lanes'
   briefs), integrate, four-sided review (`review-lanes.md`), one fix pass, squash; DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (`design/flows/02-sign-in-and-account/replay-workflow.js`); keep
   capture names; `check_baseline_paths.py`; check citations against the images.
8. **Re-sync the design system** and republish (uploads, files, the index last).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — `6df626e6` (with `check_verbatim_steps.py` and `check_baseline_paths.py` clean for flow 03; staged: 85 files, 2.69 MB, plus the two seed folders).
- [x] The canvas is created, with the mark copied in before round 1 — https://claude.ai/artifact/6Uar9yce4Dxx9iPwgE8JY5 (mark `/_blob/c1b21826d1dc3296fc39cb61b871981e`, 2026-09-29).
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/03-trainee-files-activity/` (A picked; round 2 reviewed from four sides; round 3 checked, `round-3-check.md`).
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 03) — `725237ee` (Domain 804, Application 3,565, Infrastructure 1,011, Architecture 49, Web 2,859, Integration 481).
- [x] The whole runbook replays on a fresh database with no regression from flow 03, and the baseline is re-captured,
  with `check_baseline_paths.py` at `missing 0` — the Actual lines (`3ae97b4b`…`bf84372a`: 325 steps, 254 with no gap;
  acts 3–A on `wombat_scenario_t342b` after a shutdown); 65 flow 03 states re-taken; three small defects the replay and
  states found fixed (`daaf6386`, `5fd5abcd`, `a341b44e`, Web 2,860), T348 filed; `check_baseline_paths` missing 0.
- [x] The design system is re-synced and republished — its version number: 10 (`b05f3862`; files as version 9, the index last; copy.svg uploaded, 59 icons). Lessons in BRIEF § 11 "Flow 03".

## Related

W-008, T335 (flow 01), T339 (flow 02), BRIEF § 11; flow 03's own held tasks per its brief.

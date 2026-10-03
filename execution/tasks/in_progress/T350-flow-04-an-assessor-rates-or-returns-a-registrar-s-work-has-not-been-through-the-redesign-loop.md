---
id: T350
title: Flow 04, an assessor rates or returns a registrar's work, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-30
started: 2026-09-30
---

# T350 — Flow 04, an assessor rates or returns a registrar's work, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 04 is the fourth flow of the restructure (W-008, BRIEF § 8), and the other half of flow
03: a registrar's request waits on the assessor's inbox, and nothing emails the assessor (T320).
**Surfaced:** 2026-09-30, flow 03 closing (T342).
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335, T339 and T342 needed.

## Symptom

The Assessor's Home, `/activities/inbox` and the assessor's side of the activity page carry flow 03's vocabulary only
where flow 03 reached them (the status card, the moves, the note panel); the dashboard, the inbox's structure and its
phone layout (it scrolls sideways at 390 where My activities stacks, A.7.2) have not been designed. Brief:
`design/flows/04-assessor-inbox.md`.

## What to build

Run the loop as flow 02 ran it (`design/flows/03-trainee-files-activity/`, BRIEF § 11 "Flow 02" and "Flow 03"):
1. **Restate the brief** (flow 03's § 1 is the model): structure-first ASK; flow 04 is marked W for the Assessor dashboard, fidelity for the activity page, so round 1 asks for 2–3
   structural variations; check the pages table against the code; prefer new-shell `act-*` captures, opened and
   checked; run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 04`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** from the main app's Design page on the Wombat design system (version 10); the operator gives its link.
4. **Copy the mark** into it (`asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`) before round 1.
5. **Rounds:** structure, pick, fidelity; a four-sided review of the boards against the code and the runbook (one
   reviewer on the code, one on the cast); the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (`design/flows/03-trainee-files-activity/build-lanes.md` as template; contracts named in both lanes'
   briefs), integrate, four-sided review (`review-lanes.md`), one fix pass, squash; DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (`design/flows/03-trainee-files-activity/replay-brief.md`, which keeps every password and writes per phase); keep
   capture names; `check_baseline_paths.py`; check citations against the images.
8. **Re-sync the design system** and republish (uploads, files, the index last).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — `30c29c31` (verbatim steps 0 problems, baseline paths missing 0; staged 87 files, 2.74 MB, plus flow 03's seven assessor boards in `design/upload/flow03-boards/`).
- [x] The canvas is created, with the mark copied in before round 1 — https://claude.ai/artifact/8JnYLZp6CTR1Mg38a5v7DX (mark `/_blob/89ddc910c53255a32bcbb84e2820bc45`, 2026-09-30).
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/04-assessor-inbox/` (A picked; round 2 reviewed from four sides, C1–C13, E1–E5 accepted; round 3 checked, `round-3-check.md`: all hold).
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 04) — `06aa51d7` (Domain 810, Application 3,581, Infrastructure 1,011, Architecture 50, Web 3,041, Integration 481; build-review.md: 0 high, 6 medium fixed or dropped; T351, T352 filed).
- [x] The whole runbook replays on a fresh database with no regression from flow 04, and the baseline is re-captured,
  with `check_baseline_paths.py` at `missing 0` — the Actual lines (`wombat_scenario_t350`, 2026-09-30 to 10-03: 325 steps,
  249 with no gap, every other gap an open task; 0 regression-t350, 0 new; `ef64eb8d`, `90cd0112`, `60854d97`, `4b7a04d0`,
  `9bc09e15`, `8f634de9`, `7c30795d` and the appendix's commit; dumps `scenario-t350-post-act{1..6,A}`).
- [ ] The design system is re-synced and republished — its version number.

## Related

W-008, T342 (flow 03), T320 (no email to the assessor), T343–T349; BRIEF § 11.

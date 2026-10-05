---
id: T358
title: Flow 06, programme oversight, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-04
started: 2026-10-04
---

# T358 — Flow 06, programme oversight, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 06 is the sixth flow of the restructure (W-008, BRIEF § 8): who is behind, and what has
stalled, for the committee member, the speciality and sub-speciality admins and the coordinator.
**Surfaced:** 2026-10-04, flow 05 closing (T355).
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335, T339, T342, T350 and T355 needed.

## Symptom

The oversight roles' Homes and the pages BRIEF § 8's flow 06 row lists have not been designed for the restructure.
Brief: `design/flows/06-programme-oversight.md`.

## What to build


Run the loop as flow 05 ran it (`design/flows/05-trainee-progress/`, BRIEF § 11 "Flow 04" and "Flow 05"):
1. **Restate the brief** (flow 03's § 1 is the model): structure-first ASK; flow 06 is marked as BRIEF § 8 says (check it), so round 1 asks for 2–3
   structural variations; check the pages table against the code; prefer new-shell `act-*` captures, opened and
   checked; run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 06`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** from the main app's Design page on the Wombat design system (version 15); the operator gives its link.
4. **Copy the mark** into it (`asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`) before round 1.
5. **Rounds:** structure, pick, fidelity; a four-sided review of the boards against the code and the runbook (one
   reviewer on the code, one on the cast); the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (`design/flows/05-trainee-progress/build-lanes.md` as template; contracts named in both lanes'
   briefs), integrate, four-sided review (`review-lanes.md`), one fix pass, squash; DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (`design/flows/05-trainee-progress/replay-brief.md`: every password
   kept, lines written per phase, every claim played, full pages shot from the top); keep capture names;
   `check_baseline_paths.py`; check citations against the images. Then re-take the flow's states on scratch copies,
   reusing a replay capture only after opening it (BRIEF § 11, flow 04).
8. **Re-sync the design system** and republish (uploads, files, the index last).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit. (`2f29c158`)
- [x] The canvas is created, with the mark copied in before round 1 — its link. (https://claude.ai/artifact/97WQWEuzMYHcKngPnNdHkV, `f2137a8c`)
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/06-…/`. (A; `round-2-review.md` E1–E6; `round-3-check.md`, 2026-10-05)
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 06) — the squash commit and counts. (`85d5a508`: Domain 814, Application 3763, Infrastructure 1018, Architecture 52, Web 3428, Integration 490; `build-review.md`.)
- [ ] The whole runbook replays on a fresh database with no regression from flow 06, the baseline is re-captured, and
  flow 06's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

W-008, T355 (flow 05), T298, T290, T309; BRIEF § 8 and § 11.

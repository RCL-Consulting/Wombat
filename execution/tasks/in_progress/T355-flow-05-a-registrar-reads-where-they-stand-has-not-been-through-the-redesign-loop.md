---
id: T355
title: Flow 05, a registrar reads where they stand, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-03
started: 2026-10-03
---

# T355 — Flow 05, a registrar reads where they stand, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 05 is the fifth flow of the restructure (W-008, BRIEF § 8): the registrar's own view of
where they stand, the other half of what flows 03 and 04 file and rate.
**Surfaced:** 2026-10-03, flow 04 closing (T350).
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335, T339, T342 and T350 needed.

## Symptom

The Trainee's Home, `/portfolio/progress` and `/activities/mine` read as flow 03 left them where it reached them (My
activities, the Home card); My progress and the Home's standing have not been designed. Brief:
`design/flows/05-trainee-progress.md`.

## What to build

Run the loop as flow 04 ran it (`design/flows/04-assessor-inbox/`, BRIEF § 11 "Flow 03" and "Flow 04"):
1. **Restate the brief** (flow 03's § 1 is the model): structure-first ASK; flow 05 is marked W (BRIEF § 8), so round 1 asks for 2–3
   structural variations; check the pages table against the code; prefer new-shell `act-*` captures, opened and
   checked; run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 05`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** from the main app's Design page on the Wombat design system (version 13); the operator gives its link.
4. **Copy the mark** into it (`asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`) before round 1.
5. **Rounds:** structure, pick, fidelity; a four-sided review of the boards against the code and the runbook (one
   reviewer on the code, one on the cast); the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (`design/flows/04-assessor-inbox/build-lanes.md` as template; contracts named in both lanes'
   briefs), integrate, four-sided review (`review-lanes.md`), one fix pass, squash; DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (`design/flows/04-assessor-inbox/replay-brief.md`: every password
   kept, lines written per phase, every claim played, full pages shot from the top); keep capture names;
   `check_baseline_paths.py`; check citations against the images. Then re-take the flow's states on scratch copies,
   reusing a replay capture only after opening it (BRIEF § 11, flow 04).
8. **Re-sync the design system** and republish (uploads, files, the index last).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit (to come; restated against `f913dda3`: ASK 05 ok, no step unnamed; verbatim steps 0 problems in flow 05; baseline paths missing 0; staged 90 files, 10.15 MB; flow 03's five trainee-facing boards named in § 4, not staged).
- [ ] The canvas is created, with the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/05-…/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 05) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 05, the baseline is re-captured, and
  flow 05's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

W-008, T350 (flow 04), T306 (B4: no training year shown), T323, T328, T311; BRIEF § 8 and § 11.

---
id: T361
title: Flow 07, the review sitting, has not been through the redesign loop
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-10-05
---

# T361 — Flow 07, the review sitting, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 07 is the seventh flow of the restructure (W-008, BRIEF § 8): the committee sits and ratifies.
**Surfaced:** 2026-10-05, flow 06 closing (T358).
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335, T339, T342, T350 and T355 needed.

## Symptom

The committee's review page, the evidence it reads and the pages BRIEF § 8's flow 07 row lists have not been designed for
the restructure.
Brief: `design/flows/07-review-sitting.md`.

## What to build


Run the loop as flow 06 ran it (`design/flows/06-programme-oversight/`, BRIEF § 11 "Flow 05" and "Flow 06"):
1. **Restate the brief** (flow 03's § 1 is the model): structure-first ASK; flow 07 is marked as BRIEF § 8 says (check it), so round 1 asks for 2–3
   structural variations; check the pages table against the code; prefer new-shell `act-*` captures, opened and
   checked; run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 07`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** from the main app's Design page on the Wombat design system (version 17); the operator gives its link.
4. **Copy the mark** into it (`asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`) before round 1.
5. **Rounds:** structure, pick, fidelity; a four-sided review of the boards against the code and the runbook (one
   reviewer on the code, one on the cast); the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (`design/flows/06-programme-oversight/build-lanes.md` as template; contracts named in both lanes'
   briefs), integrate, four-sided review (`review-lanes.md`), one fix pass, squash; DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (`design/flows/06-programme-oversight/replay-brief.md`: every password
   kept, lines written per phase, every claim played, full pages shot from the top); keep capture names;
   `check_baseline_paths.py`; check citations against the images. Then re-take the flow's states on scratch copies,
   reusing a replay capture only after opening it (BRIEF § 11, flow 04).
8. **Re-sync the design system** and republish (uploads, files, the index last).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with the mark copied in before round 1 — its link. (https://claude.ai/artifact/97WQWEuzMYHcKngPnNdHkV, `f2137a8c`)
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/07-…/`. (A; `round-2-review.md` E1–E6; `round-3-check.md`, 2026-10-05)
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 07) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 07, the baseline is re-captured, and
  flow 06's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines. (2026-10-05,
  `wombat_scenario_t358`: 332 steps, 266 no gap, every other gap a filed task; one regression, A.7.8a at 768 px, fixed
  in `94edf2b7` and re-checked; states re-taken on scratch copies, missing 0; T360 filed for two layout nits.)
- [ ] The design system is re-synced and republished — its version number. (Version 17, 1791219142-2815; `1fed3a9a`; lessons in BRIEF § 11 "Flow 06".)

## Related

W-008, T358 (flow 06), BRIEF § 8 and § 11; flow 07's page is the committee review (/committee/reviews/{ReviewId:int}), which flow 05's trajectory and standing panel already draw.

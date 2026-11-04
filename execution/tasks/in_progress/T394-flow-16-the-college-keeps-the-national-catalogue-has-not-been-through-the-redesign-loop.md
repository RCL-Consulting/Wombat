---
id: T394
title: Flow 16, the College keeps the national catalogue, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-11-04
started: 2026-11-04
---

# T394 — Flow 16, the College keeps the national catalogue, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 16 is the sixteenth flow of the restructure (W-008, BRIEF § 8): the College's administrator
keeps the specialities, sub-specialities, EPAs and curriculum versions, and the Administrator keeps the Colleges and the
entrustment scales. A target, its period or an EPA's pause is read live in every adopting institution, including closed
periods, so each page must say whose page it is and how far a change reaches before it is saved.
**Surfaced:** 2026-11-04, after T392 (flow 15) closed; the operator said "start the next flow".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T392 needed.

## Symptom

The catalogue pages as the College and the Administrator see them (21 templates: Colleges, specialities,
sub-specialities, entrustment scales, EPAs, curricula and a curriculum's items, `/access-denied`) have not been designed
for the restructure. Brief: `design/flows/16-college-catalogue.md`, written 2026-09-26, before flows 01–15; flow 15
rebuilt the EPA list, the curriculum list and the items page for an institution, so the brief must be restated against
what is there now. Known faults it takes up: T323's item-editor half (at 390px), T393 (CurriculumEdit and
EntrustmentScaleEdit print exception text), T291 (Back to colleges offered to the College), T324, T326, T331 (the
nav lights nothing on the College's specialities), and what T392's replay left unfiled for this flow: CurriculumEdit's
validator messages twice and unlinked, its 36–40px controls at 390px, no result after a create or a clone; the EPA
list's Create EPA under the long subtitle; three EPA list states on the old column widths (`epas-list--college`,
`--institution`, `--inactive`).

## Root cause

Not a defect with one cause: the pages predate the design system and the brief's requirements (§ Known problems).

## What to build

Run the loop as flow 15 ran it (`design/flows/15-institution-adopts-catalogue/`, BRIEF § 11 "Flow 14" and "Flow 15"):
1. **Restate the brief** in flow 15's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–15 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 16`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 16") on the Wombat design system (version 45), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** fidelity in two parts (the brief's Mode); pick; a review of the boards against the code and the runbook;
   the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 15's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 15's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; sweep new tables 900–1100px against their container; re-take the flow's states.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/16-college-catalogue/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 16) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 16, the baseline is re-captured, and
  flow 16's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T392 (flow 15), T387 (flow 14); T323, T393, T291, T324, T326, T331; BRIEF § 6, § 8, § 11; CUSTOMIZATION.md.

## Notes

- **2026-11-04.** Filed and started on the operator's word ("start the next flow"). The flow 14 scratch DBs
  (`wombat_scenario_t387_s10`, `_s20`, `_s21`, `_s22`, `_s515`) were dropped first; the replays' end-state DBs
  (`wombat_scenario_t290`…`_t392`) kept.
- **2026-11-04, steps 3–4 (ahead of step 1, as T392).** Canvas "Wombat 16": https://claude.ai/artifact/AqaH447KL91WCTbjQFJnWf,
  created from the Design type, empty boards; design system v45 (1793670418-6412) installed: `tokens.json`,
  `components/bundle.css` and the three fonts copied from the system, and `tokens.css` by hand (flow 15's, header
  updated; its 54 properties match app.css's `:root`, `--shadow-raised` resolved). The mark copied in as
  `/_blob/ce366cc30297c5a90255d5d0a88fa6b5`. Steps 1–2 run in one Opus agent. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\28969ed0-c979-487c-82e2-606d8331c3a3\scratchpad\c16`.

---
id: T392
title: Flow 15, an institution adopts the catalogue, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-11-02
started: 2026-11-02
---

# T392 — Flow 15, an institution adopts the catalogue, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 15 is the fifteenth flow of the restructure (W-008, BRIEF § 8): an institution's
administrator adopts a College curriculum version, reads the College's EPAs, items and ladders, adds an EPA and an item
of the institution's own, and moves to a newer version. Adoption gates admission and a version move rewrites a
registrar's progress, so each page must say whose rules these are and what a change will do.
**Surfaced:** 2026-11-02, after T387 (flow 14) closed; the operator said "lets do flow 15".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T387 needed.

## Symptom

The catalogue pages as an institution sees them (Curricula, EPAs, an EPA, Adoptions, a curriculum's items, the
read-only entrustment scales), the trainee profile's curriculum picker and the progress rebuild have not been designed
for the restructure. Brief: `design/flows/15-institution-adopts-catalogue.md`, written 2026-09-26, before flows 01–14.
Known faults it takes up include T304 (a superseded registrar's profile cannot be saved; a move shows zeros until a
rebuild), T301 (held-back versions), T291 (Edit offered on national EPAs; no own-EPA mark; Create EPA with nothing to
put it in) and T326 (the scales subtitle); the audits in step 1 list the rest, including what flows 01–14 already
changed.

## Root cause

Not a defect with one cause: the pages predate the design system and the brief's requirements (§ Known problems).

## What to build

Run the loop as flow 14 ran it (`design/flows/14-data-rights/`, BRIEF § 11 "Flow 13" and "Flow 14"):
1. **Restate the brief** in flow 14's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–14 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 15`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 15") on the Wombat design system (version 41), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure where the brief says wireframe first, fidelity otherwise; pick; fidelity; a review of the boards
   against the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 14's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 14's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [x] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link. (https://claude.ai/artifact/QvQL26tCCJh6S9zoLBywdZ)
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/15-institution-adopts-catalogue/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 15) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 15, the baseline is re-captured, and
  flow 15's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T387 (flow 14), T379 (flow 13), T304, T301, T291, T326; BRIEF § 6, § 8, § 11; CUSTOMIZATION.md.

## Notes
- **2026-11-02, steps 3–4 (ahead of step 1).** Canvas "Wombat 15": https://claude.ai/artifact/QvQL26tCCJh6S9zoLBywdZ,
  created from the Design type, empty boards; design system v41 (1793385917-2f3c) installed: `tokens.json`,
  `components/bundle.css` and the three fonts copied from the system, and `tokens.css` by hand (flow 14's, header
  updated; its 54 properties match app.css's `:root`, `--shadow-raised` resolved). The mark copied in as
  `/_blob/7d85109844af8d55420d07753d246843`. Steps 1–2 run in one Opus agent.
- **2026-11-02, steps 1–2** (one Opus agent, `1b0676a2`; checked here: `NavItems.cs:147`, the Administrator's
  Catalogue group has no Curriculum adoptions; `CurriculaList.razor:59` prints the College's `IsActive`, so KGK's
  superseded 11.1 reads "Active"): restated in flow 14's layout (923 lines). T304 done and its states dropped; T301,
  T291 (but item 4), T326, T323, T329, T272, T325 still true on these pages; "the scales are the College's" dropped
  (scales are platform-wide). Mode: wireframe first for Curricula with Adoptions (Q1) and the items page (Q14);
  fidelity for the EPA list and page, and a move on the profile; one proposal each for scales, the rebuild, My
  progress. § 6: Q1–Q17 with recommendations. New, unfiled: the picker offers the adopted version; adopting supersedes
  without asking; Curricula's Status; a superseded version readable only while it holds an own item (inferred); the
  Administrator's menu; "every institution" on a local EPA's deactivate; Back links and unnamed titles; an EPA's
  refusals at the top; `.Single` in the EPA list (inferred); h3 with no h2. Completeness exit 0; verbatim 0 problems
  in flow 15's 16 steps; baseline missing 0. Staged 122 files, none to crop. `round-1-ask.txt` carries the mark and
  asks the canvas to confirm the captures and save `project/f15r1.css`. **Next:** the operator's answers to Q1–Q17.

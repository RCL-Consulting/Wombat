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
- **2026-11-02, round 1** (17 boards and `f15r1.css` in `round-1/`, `ff17241b`; the operator sent the ask and said
  "done"; Q1–Q17 taken as recommended, to confirm; the canvas saved its stylesheet as a project file, but again says no
  capture arrived). Assessed in `round-1-assessment.md` (one Opus agent, `af8296c4`; checked here:
  `AdoptCurriculum.cs:54-69` supersedes with any other version, older included; `BadgeFor.cs:17` badge-draft is "not
  started or no longer in force"): picks A1 (with A2's "follows no version yet" line), B2 (with B1's 11.2 empty state
  and folded cells), C2's list with C1's page for KGK's own EPA, D1; E, F, G confirmed with corrections. 6 must-fixes
  (M1 gendered words; M2 P5 refuses an older version against Q7, so "cannot be undone" is false; M3 F's outline
  trigger against Q16; M4 the owner mark as badge-draft, not muted words; M5 the Steps board misses 1.10, 2.44, A.5.2,
  A.7.9, A.7.10, 6.13/6.29/6.31; M6 an item's EPA fixed on edit), 16 should (N1–N8 all duplicate existing classes),
  7 nits. Q18–Q21 for the operator. **Next:** the operator's answers, then round 2's ask.
- **2026-11-02, the operator took the picks and Q18–Q21 as recommended** ("yes"). Round 2's ask (`round-2-ask.txt`,
  768 lines, ~165 boards, a 74-state checklist R2-A01–R2-G01, M1–M6 and S1–S16 numbered, N1–N8 replaced by existing
  classes, `project/f15r2.css`; decisions in the brief's § 6 and the assessment's § 10; one Opus agent, `0ffb5eda`).
  Checked here: the item form's minimum is `RungPicker` (a select), not radios. PROPOSED in the ask, T1–T12; T4 (the
  item form pages serve the College and the Administrator too, changing 1.18, 6.30, A.7.10) most needs the operator's
  look. **Next:** the operator sends it with the twelve captures from `design/upload/baseline/`.
- **2026-11-02, round 2** (168 boards and `f15r2.css` in `round-2/`, `9c37e0ea`; the captures arrived this time,
  read from the connected upload folder, 7 viewed as images): reviewed by three Sonnet agents (code `50f85562` 2/14/8,
  cast `29fe0a08` 3/8/5, checklist 72/74, corrections 20/22; ds `245a8791` 1/8/7), merged by one Opus agent
  (`round-2-review.md`, `2780557d`: 6 must, 25 should, 18 nits). Checked here: `EpaEdit.razor:257` a create navigates
  with forceLoad and carries no result (M5). Musts: M1 `td.actions-cell` (242 cells); M2 A11's 11.2 count; M3 the
  College admin's and Administrator's item pages undrawn (T4, 6.30); M4 step 1.17 missing; M5/M6 a result and focus
  across a page change. F1–F9 for the operator.
- **2026-11-02, F1–F9 taken as recommended** ("yes"). Round 3's ask (`round-3-ask.txt`, 328 lines, one Opus agent,
  `5fe15328`): 41 boards redrawn at 1280 with their 390 and 768 twins (93 screens), 12 new (B21 superseded 11.1 items,
  B22–B24 Kruger's add, the list after, PAED-011's edit; B08/B17 at 1100 and 901); Spec and Steps redrawn whole; 11
  captures to reopen; `project/f15r3.css`. M1–M6 hold (242 exact); S11 partly wrong (only 6.17's Do names Deactivate).
  Assumed, marked NEW: A08's one-registrar line, B21's no-Add line, `CatalogueLinks`, `FieldRefusalException`, PAED-011's
  title, loading lines, "EPAs per page", the Administrator's A10/A11 wording. **Next:** the operator sends round 3.
- **2026-11-02, round 3** (111 boards and `f15r3.css` in `round-3/`, `7ee1206a`; the captures came from the connected
  folder). Checked item by item (`round-3-check.md`, one Sonnet agent, `e91f5c7f`, rendered in Chromium): 48 items, 45
  done, 2 partly (N6: D03's "Save profile and move…" lacks aria-haspopup, checked here; S17: a section misnamed in the
  Spec), 1 not done (N7: the trail differs from Breadcrumbs.razor; build from the component). For the operator: the
  901 px items table scrolls 10 px (move Window and weight to the item page); five 390 controls under 44 px (a
  page-scoped rule each, plus the College's "Add item"); KGK-001 last in the items table; the plus icon kept on Create
  EPA and Create curriculum; B22/B24's year selects from StageMinimaEditor. **Next:** the operator's word, then step 6.

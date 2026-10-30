---
id: T387
title: Flow 14, data rights, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-30
started: 2026-10-30
---

# T387 — Flow 14, data rights, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 14 is the fourteenth flow of the restructure (W-008, BRIEF § 8): a person makes a data-rights
request (export, access, correction, erasure) and staff decide it. These are legal rights (POPIA) and an erasure cannot
be undone, so each page must say exactly what happened and what will.
**Surfaced:** 2026-10-30, after T379 (flow 13) closed; the operator said "start flow 14".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T379 needed.

## Symptom

The data-rights pages (My data rights, the Data rights requests queue, a request's page) and the erasure's aftermath
have not been designed for the restructure. Brief: `design/flows/14-data-rights.md`, written 2026-09-26, before flows
01–13. Known faults it takes up include T316 (an approved rectification can never be applied or completed), T319 (a
decision notifies nobody; a refused requester never sees the note), T320's digest help, T318 (an opt-out nothing
reads), T324 (enum names), T264 (an erasure approved without a confirmation), T323, T328, T325 and T329's rule; the
audits in step 1 list the rest, including what flows 01–13 already changed.

## What to build

Run the loop as flow 13 ran it (`design/flows/13-graduation-and-exit/`, BRIEF § 11 "Flow 12" and "Flow 13"):
1. **Restate the brief** in flow 13's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–13 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 14`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 14") on the Wombat design system (version 37), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure where the brief says wireframe first, fidelity otherwise; pick; fidelity; a review of the boards
   against the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 13's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 13's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/14-data-rights/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 14) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 14, the baseline is re-captured, and
  flow 14's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T379 (flow 13), T376 (flow 12), T316, T319, T320, T318, T324, T264, T323, T328, T325, T329, T276, T258, T112;
BRIEF § 6, § 8, § 11; CUSTOMIZATION.md § 5.

## Notes
- **2026-10-30, steps 3–4 (ahead of step 1's commit).** Canvas "Wombat 14":
  https://claude.ai/artifact/FrvjJMZLwRd2Y3AMDEJeGs, created from the Design type, empty boards; design system v37
  (1793144981-e3ba) installed: `tokens.json`, `components/bundle.css`, the three fonts copied from the system, and
  `tokens.css` by hand (flow 13's, header updated; its 54 properties match app.css's `:root`, `--shadow-raised`
  resolved). The mark copied in as `/_blob/616538c681cf4475edf7c466864e408d`. Steps 1–2 run in one Opus agent.
- **2026-10-30, steps 1–2** (one Opus agent, `2bc97e30`; checked here: `DataRightsAuthorization.cs:29,32`
  ReviewRoles = Coordinator, RectificationRoles = SpecialityAdmin; `/admin/trainees/edit` is
  `AdministratorOrInstitutionalAdmin`, so the approving Coordinator cannot open the profile): restated in flow 13's
  layout; Mode F/W (wireframes first for My data rights and for a request's page with its correction; fidelity for the
  queue and the decision with the erasure dialog). Dropped as done: T239, T240, T234, T279, T258, the monospace
  textareas; content re-based on T379's replay (`deleted_user_0ae1d64b`). § 5: T316, T319, T318, T320's help
  sentence, T324, T264, T323, T328, T325, T329, T280, T276's request half, T112's wording. § 6: Q1–Q16 with
  recommendations. New, unfiled: UnderReview never set; the domain lets a decided request be withdrawn; a failed
  export PDF is silently dropped from the zip; refusals at the page top; Back to list repeats the trail; (inferred) an
  erased account in the Administrator's users list with an empty name. Completeness exit 0; verbatim 424/0; baseline
  missing 0. Staged 113 files, none to crop. `round-1-ask.txt` carries the mark's URL. **Next:** the operator's
  answers to Q1–Q16, then round 1.
- **2026-10-30, round 1** (18 boards in `round-1/`, `42366e44`; the operator sent the ask and said "done"; the boards
  follow Q1–Q16's recommendations, taken as recommended, to confirm): three structures for My data rights (A one page,
  B a form page, C cards), three for a request and its correction (A in its section, B a form page, C a status card),
  Queue V1/V2 and Decide V1/V2 at fidelity, NotFound-Kept, Spec, Steps. Assessment running (one Opus agent).

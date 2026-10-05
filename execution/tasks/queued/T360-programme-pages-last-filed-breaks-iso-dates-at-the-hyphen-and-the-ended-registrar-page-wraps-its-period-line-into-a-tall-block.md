---
id: T360
title: Programme pages: Last filed breaks ISO dates at the hyphen, and the ended registrar page wraps its period line into a tall block
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-10-05
---

# T360 — Programme pages: Last filed breaks ISO dates at the hyphen, and the ended registrar page wraps its period line into a tall block

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Presentation only; both read correctly, just untidily.
**Surfaced:** 2026-10-05, flow 06's states capture after the T358 replay.

## Symptom

- At 1280 px, Programme trainees' "Last filed" column breaks an ISO date at its hyphen ("2026-10-" over "05")
  (`design/baseline/states/programme-trainees--typical.png`).
- On an ended registrar's page (`/programme/trainees/4`, Dr du Plessis), the three-column cards wrap "no target (the
  programme ended part-way through) · 0 recorded" into a tall, narrow block
  (`design/baseline/states/programme-trainee-detail--ended.png`).

## What to build

- Keep a date whole: `white-space: nowrap` on the date (or the column a minimum width), as flow 05's index keeps its
  dates.
- Give the ended page's period lines room: one column for the ended cards below a sensible width, or the line under
  its heading rather than beside it.

## Verification

- [ ] Both captures re-taken, the date on one line and the period line readable — a states capture.
- [ ] `Design/` tests green (a `StylesheetRuleTests` rule for the date's nowrap).

## Related

T358 (flow 06), `ProgrammeTrainees.razor`, `EndedRecord.razor`.

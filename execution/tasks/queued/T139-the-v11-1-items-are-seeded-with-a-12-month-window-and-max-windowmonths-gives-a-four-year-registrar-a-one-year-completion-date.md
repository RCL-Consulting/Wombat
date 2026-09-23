---
id: T139
title: The v11.1 items are seeded with a 12-month window, and max(WindowMonths) gives a four-year registrar a one-year completion date
status: queued
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T139 — The v11.1 items are seeded with a 12-month window, and max(WindowMonths) gives a four-year registrar a one-year completion date

**Severity:** Low. Nothing enforces `WindowMonths` (D19), so no figure is wrong today; but the one place that reads
it produces a wrong date, and the seed misstates the College's expiry period.
**Surfaced:** 2026-09-23, during [T130]. Found by the domain critic of the design; not fixed because D19 keeps T130 off
`WindowMonths`.

## Symptom

- `PaediatricCatalogueSeeder.BuildCurriculumItem` sets `WindowMonths = 12` on all fifteen v11.1 items. The source's
  "expiry period if not practised" is **six months** for EPAs 1, 2, 4 and 5 ("Must be performed each semester (six
  months)"), twelve for the rest.
- `AdmitTrainee` defaults `ExpectedCompletionDate` to `ProgrammeStartDate + max(WindowMonths)` months, so a paediatric
  registrar admitted without an explicit completion date is expected to finish **one year** after starting a four-year
  programme. `DevUserSeeder` side-steps it with `AddYears(4)`.

## Root cause

`WindowMonths` has one reader, and it treats a per-item currency figure as a programme length.

## What to build

Decide what `WindowMonths` is. Either seed the College's expiry period into it (six for 1, 2, 4, 5) and stop
`AdmitTrainee` deriving a programme length from it, or delete it and give the curriculum an explicit programme length.
Nothing is live, so a re-seed is fine.

## Verification

- [ ] A paediatric admission with no completion date gets a four-year default — checked by an `AdmitTrainee` test
- [ ] The seed matches the source's expiry column — checked by a seed test against a hard-coded table

## Related

[T130] (D19, left alone there); `AdmitTrainee.cs:81-82,126`; `PaediatricCatalogueSeeder.BuildCurriculumItem`.

---
id: T197
title: The undated-date wording names an encounter date and then denies it, and progress shows an unmarked last encounter
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T197 — The undated-date wording names an encounter date and then denies it, and progress shows an unmarked last encounter

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The copy is wrong for drafts and awkward everywhere.
**Surfaced:** 2026-09-24, the T161 review.

## Symptom

- "Encounter date: X (filed; no encounter date)" names an encounter date and then denies it. The fallback date is the
  day the form was created (`ObservationDateResolver.cs:44`), not the filing day, so on a draft "filed" is wrong too.
- `MyProgress.razor` shows "Last encounter …" unmarked. `CurriculumItemProgress.LastObservedOn` does not record whether
  the date was stated; marking it needs a column, a migration and a progress rebuild.

## What to build

Change the wording once, in `EncounterDate.Label`, e.g. "not recorded (created yyyy-mm-dd)". Every surface picks it up.
Add the declared flag to the progress row, and mark "Last encounter".

## Verification

- [ ] Every surface shows the new wording. bUnit.
- [ ] An undated last encounter is marked on My progress. bUnit.

## Related

T161, T137, D28.

## Note, 2026-09-24 (T154 browser check)

The date refusal names the date as "the encounter date" whatever the field is: "Review period to: The encounter date
cannot be after today". Word it by the field's own label, or neutrally ("cannot be after today").


---
id: T219
title: My progress shows a trainee's last encounter unmarked when that date was never stated
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T219 — My progress shows a trainee's last encounter unmarked when that date was never stated

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is one surface T197's marking does not reach.
**Surfaced:** 2026-09-25, split from T197 (its second Verification item).

## What to build

`CurriculumItemProgress.LastObservedOn` records no "was it stated" flag and no link to the activity (`CreditApplier`).
Add the flag (a migration and a progress rebuild), and mark "Last encounter" on My progress with T197's wording ("not
recorded (created …)").

## Verification

- [x] An undated last encounter is marked on My progress. bUnit and browser.

## Related

T197, T161, D28.

---

## As built — 2026-09-25 (`79eabe8`)

`CurriculumItemProgress.LastObservedOnDeclared` records whether the last encounter's date was stated. It is set by the
credit path and the rebuild from what the completion writes, not from what the draft said. The access report exports
it too. My progress prints "Last encounter date: not recorded (created …)" through `EncounterDate.Label`.

The migration `T219_LastEncounterDeclared` empties the progress table, and the bootstrapper refills it.

Browser on dev (scripted Chrome, master `d6b2796`; `pg_dump` first, at `recovery/pre-g1-migrations.dump`):
- **The first boot** logged: "Curriculum progress was empty and 9 completions record credit, so it was rebuilt".
- **A stated date.** The PAED-006 card read "Last encounter date: 2026-08-15."
- **An undated date.** Activity 10 was set in SQL to `ObservedOnSource = CreatedOn`, then progress was emptied and
  refilled. The card read "Last encounter date: not recorded (created 2026-09-23)." After the values were restored and
  progress rebuilt, it read "2026-08-15." again.

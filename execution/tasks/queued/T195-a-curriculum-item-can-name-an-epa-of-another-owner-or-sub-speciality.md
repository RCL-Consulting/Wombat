---
id: T195
title: A curriculum item can name an EPA of another owner or sub-speciality
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Scope and catalogue integrity: a national item can point at a local EPA, and an item at any
sub-speciality's EPA.
**Surfaced:** 2026-09-24, the T158 review (finding 3, confirmed, out of scope).

## Symptom

`AddCurriculumItemCommandHandler` (`ManageCurriculumItems.cs:157-166`) and `UpdateCurriculumItemCommandHandler` (around
:207) never check the EPA's owner or sub-speciality against the item's.

## What to build

A national item may name only a national EPA of the curriculum's sub-speciality. A local item may name a national EPA
or the caller's own institution's local EPA. Refuse before any mutation, and make the EPA pickers offer exactly that.

## Verification

- [ ] Each disallowed combination is refused, and nothing is written. Handler tests.
- [ ] The picker offers only allowed EPAs. bUnit.

## Related

T158, T056, the catalogue memory (EPAs are national; curriculum rows are shared).

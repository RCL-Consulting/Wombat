---
id: T195
title: A curriculum item can name an EPA of another owner or sub-speciality
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T195 — A curriculum item can name an EPA of another owner or sub-speciality

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

- [x] Each disallowed combination is refused, and nothing is written. Handler tests.
- [x] The picker offers only allowed EPAs. bUnit.

## Related

T158, T056, the catalogue memory (EPAs are national; curriculum rows are shared).

---

## As built — 2026-09-25 (`4e61d8a`)

A national curriculum item names only a national EPA of the curriculum's sub-speciality, and a local item a national
EPA or the caller's institution's own. Add and Update refuse otherwise before any write, and the pickers use the same
predicate. The edit row says when its stored EPA is not one it can name.

**Browser:** not reachable with a permitted dev account. An InstitutionalAdmin cannot load the curriculum editor
(`GetCurriculumByIdQuery` requires `CanAccessCollege`), which predates T195 and is [T211], and dev has no seeded
CollegeAdmin. Covered by the handler matrix and `CurriculumItemsEpaPicker` bUnit (6/6). SQL confirms curriculum 2's
national EPAs are exactly PAED-001 to 015. **The browser check moves to [T211]**, which must make the local-item path
reachable.

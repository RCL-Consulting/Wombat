---
id: T291
title: Admin pages offer what they then refuse, found by the T159 replay: Edit on national EPAs, Back to colleges, and a first save's status
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T291 — Admin pages offer what they then refuse, found by the T159 replay: Edit on national EPAs, Back to colleges, and a first save's status

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T159 replay of Acts 1–2 on a fresh database (`wombat_t159`).

## Items

1. **`EpasList` offers Edit on national EPAs to an InstitutionalAdmin,** and the save is refused: "You do not have
   permission to update this EPA.". Offer Edit only where the command admits the caller (T211's rule).
2. **`SpecialitiesList` shows "Back to colleges" to a CollegeAdmin,** and `/admin/colleges` refuses them. Show it only to an
   Administrator.
3. **The first save of a new activity type shows no "Draft saved."** The page moves to `/{id}`, and
   `OnParametersSetAsync` clears the status. Keep the status across that first navigation.
4. **The InstitutionalAdmin dashboard's specialities card** counts the whole national catalogue (2 and 2 before KGK
   adopts anything). Count what the institution has adopted, and fix the stale comment.
5. **Raw ids in the activity types list.** An InstitutionalAdmin sees the Demo types' scope as `Speciality · #1`,
   because labels come only from the caller's own specialities. Label every scope.
6. **The builder preview's EPA picker** lists another institution's local EPA (the Demo `EPA-001`), although the real form
   does not. Use the same EPA rule as the form.

## Verification

- [ ] Each item, with bUnit or handler tests.

## Related

T159, T211, T261, T178.

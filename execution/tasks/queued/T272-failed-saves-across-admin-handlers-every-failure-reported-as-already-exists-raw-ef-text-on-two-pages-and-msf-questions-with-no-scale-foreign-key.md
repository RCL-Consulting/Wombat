---
id: T272
title: Failed saves across admin handlers: every failure reported as "already exists", raw EF text on two pages, and MSF questions with no scale foreign key
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T272 — Failed saves across admin handlers: every failure reported as "already exists", raw EF text on two pages, and MSF questions with no scale foreign key

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The admin sees the wrong reason, or database text.
**Surfaced:** 2026-09-25, the T254 review.

## Symptom

- **Every save failure reads "already exists".** Ten handlers report every failed save that way: CreateCollege,
  UpdateCollege, CloneCurriculum, CreateCurriculum, UpdateCurriculum, CreateEpa, CreateInstitution, UpdateInstitution
  and others. A grep for "already exists" in catch blocks finds them.
- **Raw EF text.** `SubSpecialityEdit.razor` and `EntrustmentScalesList.razor` show `exception.Message`, so a rethrown
  database exception reads "An error occurred while saving…".
- **An unchecked read-back.** A lookup after a failed save can itself fail, with no guard, in `UpdateEpa`
  (`CodeTakenAsync`), `ScheduleCommitteeReview` (`OpenBindingReviewAsync`), `StagePendingEntrustmentDecision` and
  `CurriculumAdminScope`.
- **No foreign key.** `MsfQuestion.ScaleId` has none, and `CreateMsfTemplate` stores it without checking the scale
  exists.

## What to build

- Map only the matching unique violation (T243's SQLSTATE helper and the constraint name) to "already exists", and
  rethrow the rest.
- Give pages a generic "could not be saved; try again" for database exceptions (a `RefusalText` case).
- Consider the central fix the review suggested: the audit pipeline notes per context that a save was refused, and
  drops pending changes whatever follows.
- Add an ON DELETE RESTRICT foreign key for `MsfQuestion.ScaleId` (a migration) and an existence check in create.

## Verification

- [ ] Each handler maps only its own violation. Tests with an interceptor.
- [ ] The MSF foreign key refuses deleting a scale in use. Postgres test.

## Related

T254, T243, T232, T201.

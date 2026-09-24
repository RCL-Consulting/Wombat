---
id: T145
title: The legacy FormEpaLink admin screen maps instruments to EPAs and restricts nothing
status: queued
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T145 — A second, inert "which instrument may assess this EPA" screen sits beside the real one

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing is wrong at runtime, but an administrator can configure it believing it restricts
something.
**Surfaced:** 2026-09-23, by [T122]'s design critique.

## Symptom

`/admin/forms/{id}` (`FormEdit.razor`) lets an administrator link an assessment form to EPAs, and says "EPA linked."
`FormEpaLink` (`src/Wombat.Domain/Forms/FormEpaLink.cs`) is written only by `LinkFormToEpaCommandHandler`
(`ManageAssessmentFormDetails.cs`) and read only for display (`FormMappings.cs`, `GetAssessmentForms.cs`,
`FormEdit.razor`, `FormsList.razor`). Nothing in the activity platform, the credit engine or any picker reads it.

Since [T122], the real EPA→instrument mapping is `CurriculumItem.PermittedToolsJson` against
`ActivityType.WbaToolKey`, edited on `/admin/curricula/{id}/items`. Two admin surfaces now answer the same question,
and only one of them is enforced.

## Root cause

`AssessmentForm` predates the schema-driven activity platform; the forms feature was never retired.

## What to build

Decide: retire the forms feature (and its tables), or label the screen plainly as not enforced and point at the
curriculum item editor. Retiring is preferable if nothing else reads `AssessmentForm`; check before deleting.

## Verification

- [ ] Either no admin screen links forms to EPAs, or the screen says in words that it restricts nothing and links to
      the curriculum item editor — browser check
- [ ] `grep -rn "FormEpaLink" src` shows no writer left, if retired

## Related

[T122] (the enforced mapping), [T091].

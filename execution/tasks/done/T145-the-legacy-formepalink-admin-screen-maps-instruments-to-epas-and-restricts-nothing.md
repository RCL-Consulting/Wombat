---
id: T145
title: The legacy FormEpaLink admin screen maps instruments to EPAs and restricts nothing
status: done
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
completed: 2026-09-25
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

- [x] Either no admin screen links forms to EPAs, or the screen says in words that it restricts nothing and links to
      the curriculum item editor — browser check
- [x] `grep -rn "FormEpaLink" src` shows no writer left, if retired

## Related

[T122] (the enforced mapping), [T091].

---

## As built — 2026-09-25 (`aa2b569`)

Nothing outside the forms admin feature read `AssessmentForm` or `FormEpaLink`, so the whole feature is retired:
- the Domain entities, the Application `Features/Forms`, and the Infrastructure configurations and DbSets;
- the `/admin/forms*` pages and their nav links;
- the seed data and tests.

The migration `T145_RetireAssessmentForms` drops `AssessmentForms`, `FormCriteria` and `FormEpaLinks`. Its Postgres test
migrates to exactly T145 and checks the parent rows are untouched. DOMAIN.md no longer describes the feature.

`grep -rn "FormEpaLink" src` finds only migrations.

Browser on dev (scripted Chrome, master `d6b2796`; `pg_dump` first, at `recovery/pre-g1-migrations.dump`):
- The three tables are gone.
- The sidebars of instadmin and collegeadmin have no Forms link.
- `/admin/forms`, `/admin/forms/new` and `/admin/forms/1` answer 404.

**Found:** a signed-in user's 404 is the browser's bare page, not the app's "Page not found". This is app-wide and older
than T145, filed as [T233].

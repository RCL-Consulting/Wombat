---
id: T142
title: Activity pages print raw user ids where people's names belong
status: done
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
completed: 2026-09-24
---

# T142 — Activity pages print raw user ids where people's names belong

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Cosmetic, but it is on the activity pages clinicians read and it looks broken.
**Surfaced:** 2026-09-23, observed by the [T130] staging run (OBSERVED on dev).

## Symptom

The activity History table's "Actor" column and the inbox "Subject" column print user GUIDs
(`24f37634-aac1-…`) instead of names.

## Root cause

Not yet traced. Likely the DTOs carry user ids and nothing resolves display names. [T130] added
`IUserAdministrationService.GetDisplayNamesAsync` for the committee dashboard; it is the obvious tool.

## What to build

Resolve names for the ids a page shows, in one query per page, and show the id only when no user exists.

## Verification

- [x] The History "Actor" and inbox "Subject" columns show names — checked by bUnit tests and in the browser

## Related

[T130] (`GetDisplayNamesAsync`); [T137] (the activity list shows too little about each row).

---

## As built — 2026-09-24

Scope (b): every clinician- and committee-facing page shows people by name. Names are resolved server-side, in one
batched lookup per request (`UserDisplayNames`), as DTO fields. Nothing is looked up in Razor or per row, and
`ActivityService`'s shared mapper stays lookup-free. The id shows only where no user or no name exists. Five pages keep
ids on purpose (DESIGN.md: audit, data rights, portfolio verification). The entrustment admin filter takes a name
(`TraineeFilter`: part of a name, or an exact id), applied after T183's scope, so it can only narrow.
`ReviewDetail` keeps the command's result after an action instead of re-reading it. The real lookup is tested on
PostgreSQL.

Browser on dev, 2026-09-24 (master `b0cb335`, the batch-C check): 
- The inbox Subject shows "Demo Trainee" on activities 20–28.
- Activity 22's history Actor shows names.
- Review 1, the reviews list, `/msf/campaigns` and the new-campaign picker show names, with no GUID anywhere.
- The entrustment filter reads "Filter by name". Narrowing could not be shown, because dev holds 0 decisions; bUnit and
  Postgres tests cover it.

[T106] item 4 is a duplicate of this task and is closed with it.

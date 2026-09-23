---
id: T142
title: Activity pages print raw user ids where people's names belong
status: queued
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T142 — Activity pages print raw user ids where people's names belong

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

- [ ] The History "Actor" and inbox "Subject" columns show names — checked by bUnit tests and in the browser

## Related

[T130] (`GetDisplayNamesAsync`); [T137] (the activity list shows too little about each row).

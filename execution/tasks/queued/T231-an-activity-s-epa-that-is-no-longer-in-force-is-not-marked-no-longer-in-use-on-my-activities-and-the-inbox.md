---
id: T231
title: An activity's EPA that is no longer in force is not marked "(no longer in use)" on My activities and the Inbox
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T231 — An activity's EPA that is no longer in force is not marked "(no longer in use)" on My activities and the Inbox

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The option list marks it; the lists do not.
**Surfaced:** 2026-09-25, the T196 review (item 9, out of the brief's scope).

## Symptom

T196 labels a stored EPA option that is not in force "(no longer in use)" in the form's picker (D48). `ActivityEpaLabel`,
which prints the EPA column on My activities and the Inbox, does not.

## What to build

Mark it the same way wherever `ActivityEpaLabel` is used, from the one in-force rule (`Epa.InForceAt` /
`CurriculumItemsInForce`), not a second one.

## Verification

- [ ] A deactivated EPA reads "(no longer in use)" on My activities and the Inbox. bUnit.

## Related

T196, D48, T220.

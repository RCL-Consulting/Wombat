---
id: T257
title: Choosing a panel member as its chair leaves them in the members list, so the save fails with "listed twice"
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T257 — Choosing a panel member as its chair leaves them in the members list, so the save fails with "listed twice"

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T237 review (out of scope 3).

## Symptom

On `PanelEdit.razor`, choosing as Chair someone already selected under Members or External members keeps them in that
list. The save then fails with "listed twice".

## What to build

When the chair changes, remove the new chair from the other two selections, and say so in a polite status line (or
leave them out of those pickers while they are the chair). bUnit.

## Verification

- [ ] Choosing a member as chair, then saving, succeeds. bUnit.

## Related

T237, T165.

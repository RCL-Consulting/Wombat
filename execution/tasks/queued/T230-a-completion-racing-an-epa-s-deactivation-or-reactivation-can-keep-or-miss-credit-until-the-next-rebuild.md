---
id: T230
title: A completion racing an EPA's deactivation or reactivation can keep or miss credit until the next rebuild
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T230 — A completion racing an EPA's deactivation or reactivation can keep or miss credit until the next rebuild

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The window is one request wide, and a rebuild corrects it.
**Surfaced:** 2026-09-25, the T196 review (finding 3). Documented in the `ResumedEpaCredit` remarks and CUSTOMIZATION.md,
not fixed.

## Symptom

- A completion that reads the EPA as inactive, and saves after a reactivation has read its candidates, stays uncredited
  until an Administrator rebuild.
- A completion that read the EPA as active just before a deactivation committed, but whose moment falls after
  `DeactivatedOn`, keeps live credit. A rebuild while the EPA is inactive removes it; reactivation restores it.

## What to build

Make the completion's save and the EPA's deactivation or reactivation conflict: for example, the credit path touches a
concurrency token the EPA's save also touches, or the credit reads the EPA under a lock. Then re-run the loser.

## Verification

- [ ] Each race, driven from two contexts in a Postgres test, ends with the credit a rebuild would write.

## Related

T196, D48, T158.

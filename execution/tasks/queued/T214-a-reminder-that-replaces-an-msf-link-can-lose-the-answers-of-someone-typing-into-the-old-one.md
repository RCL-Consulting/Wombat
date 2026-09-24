---
id: T214
title: A reminder that replaces an MSF link can lose the answers of someone typing into the old one
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A respondent who opened the old link, then took long enough that a reminder issued a new one, loses
their answers on submit.
**Surfaced:** 2026-09-24, the T206 review (finding 1, partly fixed: the reminder now spares a link mailed less than a
day ago).

## What to build

Add `MsfInvitations.PreviousTokenHash` (a migration). Set it when a reminder replaces the link, and accept it until the
last day to respond. Clear it when the respondent answers, the link is revoked or the invitation is anonymised.
`GetActiveInvitationByTokenAsync` checks both hashes. T163 (the invitation lookup) should land first or with it.

## Verification

- [ ] A response through the previous link, submitted after a reminder, is accepted once, and the new link is then
      used. Postgres test.

## Related

T206, T205, T163.

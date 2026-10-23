---
id: T377
title: A role given by hand is not recorded as manual, so the next SSO sign-in can take it away
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-23
---

# T377 — A role given by hand is not recorded as manual, so the next SSO sign-in can take it away

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** _High / Medium / Low, and why — this is what `priority` above is derived from._
**Surfaced:** 2026-10-23, by the flow 12 round 1 assessment (T376, `design/flows/12-people-admin/round-1-assessment.md` Q20).

## Symptom

`SsoGroupMapper` (`src/Wombat.Infrastructure/Identity/SsoGroupMapper.cs:120-130`) removes an SSO-derived role that no
longer matches unless a `UserRoleAssignments` row with `Source == RoleAssignmentSource.Manual` keeps it. Nothing in
`src/` writes such a row: Add role (`UserAdministrationService.cs:442`) and admission (`:423`) call only
`UserManager.AddToRoleAsync`. So an admin's hand-given role, on a person who also matches an SSO group for that role,
can be taken away at their next institutional sign-in. Observed by reading the code at `62f6ec41`; not replayed.

## Root cause

The manual-assignment row the mapper reads is never written (inferred: the write was planned with T027 and not wired).

## What to build

_The intended shape of the work. Enough that someone else could start it._

## Verification

_This repo's completion signal. Each item names how it will be checked — a command, a test,
an observation. "Code exists" is not a completion signal._

- [ ] _criterion_ — checked by _command or observation_

## Related

_Task ids, decisions (`D<n>` for product, `W-<n>` for process), files, commits._

## Notes

_Observed facts, inferences, and unknowns. Label them when the difference matters._

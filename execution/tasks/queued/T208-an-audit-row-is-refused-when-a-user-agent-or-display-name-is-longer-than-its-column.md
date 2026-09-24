---
id: T208
title: An audit row is refused when a User-Agent or display name is longer than its column
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

**Severity:** Low. With the header over 500 characters, the command still commits but leaves no audit row, and the
user sees EF's error.
**Surfaced:** 2026-09-24, the T201 review.

## Symptom

`AuditEntry.Create` (`src/Wombat.Domain/Audit/AuditEntry.cs:50-92`) shortens `errorMessage` but not `ActorUserAgent`
(`varchar(500)`) or `ActorDisplay` (`varchar(200)`). `HttpAuditContextProvider.cs:34-36` passes the raw header
through.

## What to build

Truncate both in `AuditEntry.Create`, as `errorMessage` is.

## Verification

- [ ] A 2,000-character User-Agent leaves a (truncated) audit row. Test.

## Related

T201, T122 (errorMessage truncation).

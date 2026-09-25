---
id: T275
title: Dev and the integration tests run on PostgreSQL 16 while production runs 18
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T275 — Dev and the integration tests run on PostgreSQL 16 while production runs 18

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low to Medium. A behaviour difference between majors (as T243 suspected for RESTRICT SQLSTATEs) would be
invisible to the whole suite.
**Surfaced:** 2026-09-25, the T243 review. Dev's server reads 16.10. A PostgreSQL 18 service exists on port 5433, but
the `wombat` user cannot log in there.

## What to build

Move dev to the 18 cluster: `pg_dump` from 16, restore into 18 (a 16 dump restores into 18), repoint the user-secret,
and retire the 16 service. Or run `ForeignKeySqlStatePostgresTests`, and ideally the whole Integration suite, against 18
whenever it is available. Read production's version with `deploy/verify/drift-check.sh` and record it in HANDOVER.md.
Fold the five handlers' own `UniqueViolation = "23505"` constants into `PostgresErrors`.

## Verification

- [ ] `SELECT version()` on dev reads 18.x, and the Integration suite passes there.

## Related

T243, T241, T227.

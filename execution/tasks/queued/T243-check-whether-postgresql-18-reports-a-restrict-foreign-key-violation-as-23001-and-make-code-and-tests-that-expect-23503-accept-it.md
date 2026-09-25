---
id: T243
title: Check whether PostgreSQL 18 reports a RESTRICT foreign-key violation as 23001, and make code and tests that expect 23503 accept it
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T243 — Check whether PostgreSQL 18 reports a RESTRICT foreign-key violation as 23001, and make code and tests that expect 23503 accept it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low to Medium if true. Production runs PostgreSQL 18.x, and a handler that translates 23503 into a refusal
would show a raw error there.
**Surfaced:** 2026-09-25, the T223 implementer, on a `postgres:18` container. Not reproduced: the dev server's tests
pass with 23503.

## Symptom (reported)

Four foreign-key tests expect SQLSTATE 23503, and PostgreSQL 18.6 reportedly returns 23001 (`restrict_violation`) for an
`ON DELETE RESTRICT` delete: `DecisionCadenceMigrationPostgresTests`, `DecisionPanel*PostgresTests` and
`CommitteeAgendaPostgresTests`.

## What to build

1. Establish the fact: `SELECT version()` on dev, and a RESTRICT delete on a current 18.x. Record which code each version
   gives.
2. If they differ, make every `PostgresException.SqlState == "23503"` check in `src` and `tests` accept both
   (`PostgresErrorCodes.ForeignKeyViolation` or `RestrictViolation`), in one helper.
3. Compare production's minor version with dev's (`deploy/verify/drift-check.sh`).

## Verification

- [ ] Both codes are handled wherever a foreign-key violation is translated. Tests.

## Related

T223, T232.

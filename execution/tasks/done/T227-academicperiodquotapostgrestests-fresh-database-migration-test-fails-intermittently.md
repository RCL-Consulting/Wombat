---
id: T227
title: AcademicPeriodQuotaPostgresTests' fresh-database migration test fails intermittently
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T227 — AcademicPeriodQuotaPostgresTests' fresh-database migration test fails intermittently

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A flaky test hides real failures.
**Surfaced:** 2026-09-25. `AcademicPeriodQuotaPostgresTests.MigrateAsync_OnAFreshDatabase_AppliesT130…` failed twice in
worktree runs during the T217 work and passed on the rerun.

## What to build

Find why (a shared container, ordering, or a timing assumption), and fix the cause. Do not add a retry.

## Verification

- [x] The Integration suite passes 10 runs in a row.

## Related

T130.

---

## As built — 2026-09-25 (`7684f3a`)

**The cause.** The test's catalog query rendered index definitions across every schema, through `pg_indexes` and
`pg_get_indexdef`. `pg_get_indexdef` and `pg_get_expr` lock the table they render, so another test class's uncommitted
`DROP SCHEMA` made the query wait and fail (55P03), or fail with "could not open relation" once the drop committed.

**The fix.** The query now filters to the test's own schema before rendering. A regression test holds a second schema's
drop open under `lock_timeout = 2s`.

**Mutation check.** With the old query back, the new test fails with 55P03.

ARCHITECTURE.md § Testing states the rule and the suite's real setup: a dev PostgreSQL, and one `it_<guid>` schema per
test, not Testcontainers. CLAUDE.md is corrected to match.

**Evidence.** The implementer's 10 full Integration runs in a row passed 148 of 148 each time, with no `it_` schema left
behind.

**Filed:** [T241] (pooled schema-admin connections, and unused package references).

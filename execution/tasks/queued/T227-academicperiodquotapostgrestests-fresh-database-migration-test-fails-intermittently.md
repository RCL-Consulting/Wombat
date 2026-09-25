---
id: T227
title: AcademicPeriodQuotaPostgresTests' fresh-database migration test fails intermittently
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
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

- [ ] The Integration suite passes 10 runs in a row.

## Related

T130.

---
id: T241
title: Integration test hygiene: schema-admin connections share the pool and can exhaust connections, and two package references are unused
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T241 — Integration test hygiene: schema-admin connections share the pool and can exhaust connections, and two package references are unused

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is a latent 53300 ("too many clients") under a busy machine, plus dead dependencies.
**Surfaced:** 2026-09-25, the T227 review.

## Symptom

- All 35 integration test classes open their `CREATE`/`DROP SCHEMA` connections with the pooled base connection string,
  and none calls `ClearPool`. Only the per-schema connection is unpooled. They share one pool per test process.
- `Testcontainers.PostgreSql` and `Respawn` are referenced in `Wombat.Integration.Tests.csproj`, and no file uses either.
  The `SSH.NET` security pin in `Directory.Packages.props` exists only because Testcontainers pulls it in.

## What to build

One shared helper for `ResolveBaseConnectionString`, the schema-admin connection (opened unpooled) and T227's catalog
helpers, used by every class. Remove the two unused references and, if nothing else needs it, the SSH.NET pin.

## Verification

- [ ] The Integration suite passes, and `grep -rn "new NpgsqlConnection(_baseConnectionString)" tests` finds nothing
      outside the helper.
- [ ] `dotnet list package --include-transitive` shows no Testcontainers or SSH.NET.

## Related

T227.

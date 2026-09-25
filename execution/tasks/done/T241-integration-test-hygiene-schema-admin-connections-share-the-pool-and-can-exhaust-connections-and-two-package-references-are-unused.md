---
id: T241
title: Integration test hygiene: schema-admin connections share the pool and can exhaust connections, and two package references are unused
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] The Integration suite passes, and `grep -rn "new NpgsqlConnection(_baseConnectionString)" tests` finds nothing
      outside the helper.
- [x] `dotnet list package --include-transitive` shows no Testcontainers or SSH.NET.

## Related

T227.

---

## As built — 2026-09-25 (`e9305b5`, `bfe2725`)

- **One shared helper.** `TestDatabase` and `TestSchemas` resolve the server, open schema-admin connections, name and
  drop `it_<guid>` schemas, and hold T227's catalog helpers. Every integration test class uses them.
- **Pooling.** A schema's connections are pooled until the schema is dropped, then cleared, so a run no longer uses up
  Windows' local ports.
- **The scan.** A test fails on any class that opens `new NpgsqlConnection(_baseConnectionString)` or builds its own
  `NpgsqlConnectionStringBuilder`.
- **Packages.** `Testcontainers.PostgreSql`, `Respawn` and the SSH.NET pin are removed.
- **Classes merged since.** The classes that landed after the lane (T247, T248, T253, T254) were converted at merge.

Master's Integration suite passed 290 of 290 on its first run, with no 53300 failures. HANDOVER.md and CLAUDE.md now say
how integration tests run.

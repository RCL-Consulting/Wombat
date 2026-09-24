---
id: T140
title: The integration suite's MSF flow test cannot set itself up on a fresh schema, and leaks a schema every time it fails
status: done
priority: P2
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
completed: 2026-09-24
---

# T140 — The integration suite's MSF flow test cannot set itself up on a fresh schema, and leaks a schema every time it fails

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The only test that runs the Api host on real Postgres has never passed, so it guards nothing,
and every failed run leaves a schema behind in whatever database it points at (the dev database, by default).
**Surfaced:** 2026-09-23, running the whole Integration suite for [T130]. Not a T130 defect: it fails identically on the
pre-T130 baseline.

## Symptom

`MsfRespondEndpointFlowTests.OpenRespondCloseRelease_Flow_WorksAgainstLiveApiAndPreservesAnonymity` fails in
`InitializeAsync` → `AdmitSubjectAsync` with `Sequence contains no elements` (the `FirstAsync` over `Curricula`).
Because `InitializeAsync` throws, xUnit never calls `DisposeAsync`, so the `it_<guid>` schema it created is not dropped.

## Root cause

`src/Wombat.Api/Program.cs` migrates but seeds nothing, so a fresh schema has no curriculum; the fixture (added with
[T121]) assumes one. [T121] compiled the test but never ran it.

## What to build

Have the fixture create the curriculum it needs (or run `DataSeeder`), and drop the schema even when setup fails (a
`try` in `InitializeAsync`, or a class fixture that owns the schema).

## Verification

- [x] The whole Integration suite passes — `dotnet test tests/Wombat.Integration.Tests -c Release`: 23/23 on master
      `e2f4858`, 2026-09-24
- [x] No `it_` schema remains after a deliberately failing run.
      - The implementer forced a setup failure and confirmed its schema was dropped, by exact name and by diffing
        the schema list.
      - The 13 empty schemas the old fixture had leaked were dropped by exact name.
      - After the full suite on master, the read-only count of `it_%` schemas is **0**.
      - A second forced failure was blocked by the permission classifier and not retried.

## Related

[T121], [T130] (`AcademicPeriodQuotaPostgresTests` uses its own schema-per-test and passes 9/9).

---

## As built — 2026-09-24

- Nothing that can fail runs outside a `try` that drops the schema, and `DisposeAsync` tolerates a null client or
  factory. Setup seeds the catalogue as startup does (`SeedCatalogueAsync`) and selects the Paediatric EPA Curriculum
  by name.
- The test host runs no scheduler: `ApiFactory` removes `ScheduledJobHost`, and a check asserts that
  `ScheduledJobDefinitions` is empty. The leaked schemas had held job rows written during setup.
- The test asserts that `ObservedOn` is the `ClosedOn` date, as T121 specifies. It moves `ClosedOn` two days back, so
  the close day and the release day differ. Mutants that date the evidence from today, or from `ClosesOn`, both fail.

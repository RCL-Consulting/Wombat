---
id: T140
title: The integration suite's MSF flow test cannot set itself up on a fresh schema, and leaks a schema every time it fails
status: queued
priority: P2
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T140 — The integration suite's MSF flow test cannot set itself up on a fresh schema, and leaks a schema every time it fails

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

- [ ] The whole Integration suite passes — `dotnet test tests/Wombat.Integration.Tests -c Release`
- [ ] No `it_` schema remains after a deliberately failing run — checked with `\dn it_*` in psql

## Related

[T121], [T130] (`AcademicPeriodQuotaPostgresTests` uses its own schema-per-test and passes 9/9).

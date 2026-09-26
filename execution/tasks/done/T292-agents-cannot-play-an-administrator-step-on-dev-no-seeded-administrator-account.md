---
id: T292
title: Agents cannot play an Administrator step on dev: no seeded Administrator account
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T292 — Agents cannot play an Administrator step on dev: no seeded Administrator account

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. About fifteen Administrator jobs in the GUI (colleges, institutions, entrustment scales, the audit
detail, the progress rebuild, scheduled jobs, SSO mappings, erasure approval) cannot be exercised by an agent on dev, so
the scenario runbook plays them with SQL stand-ins or not at all (T159, T204).
**Surfaced:** 2026-09-26, while scoping the full-coverage runbook rewrite (T293) for the GUI redesign. The operator
chose a dev-only Administrator account over playing those steps by hand.

## Symptom

`DevUserSeeder` seeds a Trainee, two CommitteeMembers, an Assessor, a Coordinator, an InstitutionalAdmin and a
CollegeAdmin, but no Administrator. The only Administrator on a dev database is `AdminSeeder`'s bootstrap account, whose
password is configuration an agent may not read. The T159 replay stood in for Steps 1.2, 1.3 and 1.11–1.12 with SQL
writes and borrowed accounts.

## Root cause

Deliberate at the time (T204: "verification must not depend on the administrator's credential"). That rule guarded the
bootstrap credential, which is the same kind of secret production uses; a dev-only account with a hardcoded password,
like the seven already seeded, does not expose it.

## What to build

- `DevUserSeeder` seeds `devadmin@wombat.local` with a hardcoded dev password beside the others, holding
  `Administrator` only, with no institution and no College (a global Administrator, as `AdminSeeder` makes one), and
  idempotent like the rest.
- It runs only where the rest of `DevUserSeeder` runs (`IHostEnvironment.IsDevelopment()`), so production never sees it.
- The seeder's summary and `HANDOFF`/runbook lists of dev accounts name it.

## Verification

- [x] A fresh dev boot creates `devadmin@wombat.local` holding exactly `Administrator`, with no institution or College —
  `DevUserSeederTests.AFreshDevBoot_SeedsDevAdmin_AsAGlobalAdministratorOnly_AndASecondBootCreatesNothing`.
- [x] A second boot creates nothing — the same test.
- [x] `DevUserSeeder` is still invoked only under `IsDevelopment()` — `Program.cs:693` read. No test pins the gate
  (`DemoSeedKeyPostgresTests` calls the seeder itself).
- [x] Signed in as the account on a dev database, `/admin/colleges` and `/admin/institutions` open — browser check,
  2026-09-26, on a fresh database (`wombat_scenario_t292`, `tools/scenario-replay.ps1`).
- [x] Infrastructure, Application and Architecture suites green.

## As built — 2026-09-26

- `DevUserSeeder.EnsureAdministratorAsync` seeds `devadmin@wombat.local` (Demo Administrator), holding `Administrator`
  only, with no institution, College or scope. It is the first thing `SeedAsync` does, so the early returns that wait
  for the demo curriculum and institution cannot skip it.
- `DevUserSeederTests` (real Identity over EF InMemory, the app's password rules, booted in `Program.cs`'s order): the
  account is a global Administrator only and a second boot changes no count; and with no demo data it is still seeded,
  alone. Mutation-checked: without the call both fail; with the call moved behind the early returns the second fails.
- Suites (Debug; the dev app held the Release output): Infrastructure 981/981, Application 3267/3267, Architecture
  45/45.
- Browser: on a fresh database the account signs in to the Administrator's dashboard and nav (Colleges, Institutions,
  Scheduled Jobs, SSO Mappings, Audit Log, Data Rights Requests, System), and `/admin/colleges` lists CPSA and the Demo
  College, `/admin/institutions` the Demo Institution.

## Related

T204, T159 (the SQL stand-ins), T293 (the runbook rewrite), T295 (the replay).

## Notes

- The password sits in source like the other seven dev passwords; it is not a secret, because the account exists only
  on a Development host.

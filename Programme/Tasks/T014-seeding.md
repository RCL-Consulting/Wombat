# T014 — Seeding & first-run bootstrap

**Phase:** 4 — Quality & ship
**Depends on:** T002, T003, T004
**Blocks:** T015, T016

## Goal

Make a fresh install ready to use. When the app starts against an empty database, it should create the admin, one institution, one speciality, one sub-speciality, one EPA, one curriculum, one assessment form, and a default entrustment scale. All seeded data is guarded so it runs exactly once.

## What to do

1. A `DataSeeder` service in `Wombat.Infrastructure/Persistence/Seeding/`. It is called from `Program.cs` during startup, after migrations have been applied. It uses `IApplicationDbContext` (via infra), not handlers, so it can bypass normal validation.
2. Sequence of seeds (each one checks "does this already exist? if yes, skip"):
   1. Roles — redundant with `RoleSeeder` from T002, but belt-and-braces.
   2. Administrator user from config — redundant with `AdminSeeder`, same reason.
   3. Default `EntrustmentScale` ("O-R Scale") with five levels:
      1. "Observation only"
      2. "Performs with direct supervision"
      3. "Performs with supervision nearby"
      4. "Performs independently; retrospective review"
      5. "Performs independently; can supervise others"
   4. Demo `Institution` ("Demo University Hospital").
   5. Demo `Speciality` ("Internal Medicine") under the demo institution.
   6. Demo `SubSpeciality` ("General Medicine") under the demo speciality.
   7. Demo `Epa` ("IM-001 — Admit a patient to a general medical ward") under the demo sub-speciality.
   8. Demo `AssessmentForm` ("Mini-CEX v1") linked to the demo EPA, using the default scale, with three criteria: "History-taking", "Physical examination", "Clinical reasoning" — all scale criteria.
   9. Demo `Curriculum` ("IM Core Curriculum v2026.1") under the demo sub-speciality, with one `CurriculumItem` referencing the demo EPA (RequiredCount=5, MinimumLevelOrder=4, WindowMonths=12).
3. A config flag `Wombat:Seed:DemoData` defaults to `true` in Development, `false` in Production. In Production, only the admin + role seeds run — the demo tree is skipped unless explicitly enabled.
4. A CLI mode: `dotnet Wombat.Web.dll --seed` runs migrations + seeds and exits. This is what gets called on a fresh server before the first real service start.
5. A CLI mode: `dotnet Wombat.Web.dll --migrate` runs migrations only and exits. This is what gets called on each deployment.
6. `Wombat.Web.dll --seed` and `--migrate` share the same startup path as the web host but short-circuit the `Run()` call. Pattern: detect the flag early in `Program.cs`, build the host, resolve the seeder / migrator service, execute, call `Environment.Exit(0)`.

## Files created

- `src/Wombat.Infrastructure/Persistence/Seeding/DataSeeder.cs`
- `src/Wombat.Infrastructure/Persistence/Seeding/Seeds/{EntrustmentScalesSeed,DemoTreeSeed}.cs`
- Modifications to `src/Wombat.Web/Program.cs` for CLI modes.
- `appsettings.Development.json` — `Wombat:Seed:DemoData = true`
- `appsettings.Production.json` — `Wombat:Seed:DemoData = false`

## Verification

- [ ] `dotnet build` clean.
- [ ] Drop the local DB, run `dotnet Wombat.Web.dll --migrate`, confirm tables exist.
- [ ] Run `dotnet Wombat.Web.dll --seed` in Development mode, confirm the demo tree exists end-to-end (curriculum visible, EPA visible, form visible, admin user exists).
- [ ] Run `dotnet Wombat.Web.dll --seed` twice — the second run does nothing (idempotent).
- [ ] Run the app with `ASPNETCORE_ENVIRONMENT=Production` and `DemoData=false`, confirm only roles + admin are seeded.

## Notes & gotchas

- Seed methods must be **idempotent**. Every seed checks for existing data before inserting. Use `AnyAsync` not `CountAsync` for performance.
- Do not use EF's `HasData` for anything other than truly static reference data (roles, scales). Demo data goes through the seeder because `HasData` creates migrations that are a pain to maintain.
- The `--seed` CLI mode must not start the web host. Otherwise systemd considers the unit "started" when it's really just running a one-shot command.
- Production mode skipping demo data means a real deployment has an admin user + roles but nothing else. The admin then uses the UI to create their institution, speciality, etc. This is the intended first-run experience.

# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T004 — EPAs, Curricula, assessment forms** (not started)

Next session: read `Tasks/T004-epas-curricula-assessment-forms.md` and execute it.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → T011 → T012 → T023 → T024 → T025 → T026 → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

Planning session on 2026-04-11, followed by T001 scaffold completion.

**Pass 1 — initial plan:**
- Created `README.md`, `PLAN.md` (v1), `DOMAIN.md` (v1), `ARCHITECTURE.md` (v1), `WORKFLOW.md`, `INFRASTRUCTURE.md`
- Created `Tasks/T001`–`T016` task files (T007–T009 wrote assessments as concrete typed aggregates — **wrong approach**; see pass 2).

**Pass 3 — builder scope refinement:**
- Rewrote `T019-activity-builder-ui.md` as a v1 scope with a visual form editor from day one, JSON-validated editors for the Workflow and Credit tabs, exactly ten field types, single-condition `show_if`, up/down reorder (no drag-drop), one-level sections (no repeatable or nested), and a full draft/publish lifecycle with immutable published versions and activity schema pinning.
- Added six follow-up task stubs as post-launch iterations, each small and independent:
  - `T019-b-drag-drop-reorder.md`
  - `T019-c-nested-and-repeatable-sections.md`
  - `T019-d-visual-workflow-editor.md`
  - `T019-e-visual-credit-rules-editor.md`
  - `T019-f-multi-condition-visibility.md`
  - `T019-g-schema-templates-and-copy.md`
- Updated `CUSTOMIZATION.md` with the explicit v1 field-type list (text, longtext, number, date, choice, multichoice, likert, procedure_ref, file, signature), the single-condition `show_if` constraint, the "sections are one level deep in v1" rule, and a "Builder scope — v1 and beyond" section that lists what ships in T019 versus each follow-up.
- Updated `PLAN.md` to list the follow-ups as deferred tasks under Phase 2 with an explicit "do not pull any of these into T019" instruction.

Rationale: a builder is non-negotiable from day one (the old Wombat already has one and replacing it with a JSON textarea would be a regression), but the polish features are the sinkhole. Shipping the engine plus a visual-form/JSON-workflow/JSON-credit editor meets the "match or slightly exceed the old Wombat builder" bar without expanding T019 into a six-month UI task.

**Pass 2 — no-code customization pivot:**
- Added `CUSTOMIZATION.md` describing the Activity platform (jsonb schema + workflow DSL + credit rules DSL).
- Added supersession banners to `T007`, `T008`, `T009`, and partial banner to `T011`.
- Rewrote `PLAN.md` with the new phase structure (Phases 0–7).
- Amended `DOMAIN.md` with the Activity-platform addendum.
- Amended `ARCHITECTURE.md` with the Activity-platform layer.
- Added new task files:
  - `T017-activity-platform-schema.md` — domain aggregates + DSLs + jsonb storage
  - `T018-activity-engine.md` — schema validator, workflow evaluator, credit applier, CQRS handlers
  - `T019-activity-builder-ui.md` — admin builder + runtime renderer
  - `T020-seed-activity-types.md` — 10 seeded types incl. Mini-CEX, DOPS, CbD, ACAT, STAR, procedure log, research, teaching, QI, journal club
  - `T021-multi-source-feedback.md` — hardcoded MSF with anonymity invariants
  - `T022-committee-decisions.md` — hardcoded committee (ARCP-equivalent) with immutable decisions
  - `T023-portfolio-pdf-export.md` — QuestPDF portfolio export
  - `T024-scheduled-nudges.md` — hosted-service scheduler + Cronos, 7 jobs
  - `T025-admin-audit-log.md` — append-only audit via MediatR pipeline behaviour
  - `T026-data-subject-rights.md` — POPIA/GDPR access/export/rectify/object/erase with retention exceptions
  - `T027-institutional-sso.md` — OIDC client for Entra/Google/Shibboleth-bridge, group→role mapping, break-glass

T001 completed:
- Removed the old root projects (`Wombat.Common`, `Wombat.Data`, old `Wombat.Application`, old `Wombat.Web`, old `Wombat.sln`) per task instructions.
- Scaffolded a new Visual Studio-friendly `Wombat.sln` with `src/` and `tests/` folders at the repo root.
- Added the five app projects: `Wombat.Domain`, `Wombat.Application`, `Wombat.Infrastructure`, `Wombat.Api`, `Wombat.Web`.
- Added the four test projects: `Wombat.Domain.Tests`, `Wombat.Application.Tests`, `Wombat.Architecture.Tests`, `Wombat.Integration.Tests`.
- Wired layer references to match `ARCHITECTURE.md`.
- Added root `Directory.Build.props`, `Directory.Packages.props`, and `.editorconfig`.
- Kept `.gitignore` in place and retained `Rewrite/` plus `ClinicAssist.NET_ref_DO_NOT_COMMIT/`.
- Set launch profiles for local verification on `http://localhost:5080` (Web) and `http://localhost:5090/health` (API).
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test Wombat.sln -c Release --no-build` passes; runners report no discovered tests yet.
  - `dotnet run --project src/Wombat.Api --no-build -c Release --launch-profile http` returns `ok` from `/health`.
  - `dotnet run --project src/Wombat.Web --no-build -c Release --launch-profile http` returns an HTML shell from `/`.

Reference for future tasks:
- Old Wombat runtime code has been removed from the root working tree as planned.
- The architecture reference remains `ClinicAssist.NET_ref_DO_NOT_COMMIT/`.
- Historical old-Wombat code can be recovered from git history if needed.

T002 completed:
- Added `WombatUser` and role constants in Domain.
- Added `IApplicationDbContext`, claims principal extensions, and claim-type constants in Application.
- Added `ApplicationDbContext : IdentityDbContext<WombatIdentityUser>`, identity scope entities, role/admin seeders, authorization policies, and a claims principal factory in Infrastructure.
- Wired Identity and authorization into `Wombat.Web/Program.cs` and startup seeding now runs on app boot.
- Added the initial migration `IdentityInitial` under `src/Wombat.Infrastructure/Persistence/Migrations/`.
- Added a unit test covering `ClaimsPrincipalExtensions.GetInstitutionId()`.
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj -c Release --no-build --filter GetInstitutionId` passes.
  - `dotnet ef database update --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext` succeeds against a fresh local verification database when `ConnectionStrings__DefaultConnection` is overridden to the local Postgres dev connection.
  - Running the app against that fresh verification database seeds all 9 roles and 1 `Administrator` bootstrap user.

Important environment note:
- The committed `appsettings.json` connection string remains a placeholder local default.
- Local Postgres verification on this machine used an environment override to a working dev connection and a fresh temporary database `wombat_t002_verify`.

T003 completed:
- Added `Institution`, `Speciality`, and `SubSpeciality` entities in Domain.
- Added EF configurations, `DataSeeder`, and the `InstitutionsInitial` migration in Infrastructure.
- Added the institution/speciality/sub-speciality query and command handlers plus validators in Application.
- Wired the Blazor host for authorization-aware routing, scoped MediatR dispatch from components, and startup data seeding.
- Added admin pages under `src/Wombat.Web/Components/Pages/Admin/Institutions/` for list/create/edit/deactivate flows across the hierarchy.
- Added two new application tests:
  - validator coverage for `CreateInstitutionCommandValidator`
  - in-memory handler coverage for `CreateInstitutionCommandHandler`
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj -c Release --no-build` passes with 3/3 tests green.
  - `dotnet ef migrations add InstitutionsInitial --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext --output-dir Persistence/Migrations` succeeds.
  - `dotnet ef database update --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext` succeeds against `wombat_t002_verify`.
  - `dotnet run --project src/Wombat.Web --no-build -c Release --launch-profile http` returns HTTP 200 from `/` after the migration.
- Verification caveat:
  - The admin UI is role-gated, but a full manual login walkthrough was not re-run in this session because local admin bootstrap credentials are not currently configured in committed settings and there is still no dedicated login UI task completed yet.

## Last known-good commit

`483738d` — Add identity and role model plumbing

## Open questions

- **Icon set.** ClinicAssist learned that the Bootstrap Icons web font is problematic. We need to pick an icon strategy before T010. Current leaning: inline SVGs from Lucide copied into a static folder. Decide at the start of T010.
- **Will there be a `Programme` layer above `SubSpeciality`?** DOMAIN.md mentions this as "maybe later". Defer; current plan assumes Curriculum belongs directly to SubSpeciality. Revisit at T004.
- **Schema DSL extensibility: repeatable sections.** Decided for v1: no repeatable sections. T020's QI seed uses three pre-numbered sub-sections (`pdsa_1`, `pdsa_2`, `pdsa_3`). Repeatable sections are T019-c.
- **Drag-and-drop library for the builder.** Decided for v1: no drag-drop; up/down buttons only. SortableJS comes in T019-b.
- **Cron timezone.** T024 says cron expressions run in the institution's timezone. Confirm institution tz is stored on `InstitutionBrand` in T023.
- **GitHub Actions.** Defer until post-T016. Phase 1 deploy is rsync + systemctl.

## Blockers

None.

## Session history

| Date | Session | Task | Outcome |
|---|---|---|---|
| 2026-04-11 | planning v1 | — | Created initial `Rewrite/` plan folder with T001–T016. |
| 2026-04-11 | planning v2 | — | No-code pivot: added CUSTOMIZATION.md, Activity platform (T017–T020), hardcoded features (T021–T022), cross-cutting ops (T023–T027). Superseded T007–T009. |
| 2026-04-11 | planning v3 | — | Builder scope refinement: rewrote T019 for honest v1 scope (visual form editor, JSON-validated workflow/credit, ten field types, single-condition show_if, up/down reorder, draft/publish lifecycle). Added T019-b through T019-g as deferred follow-ups. Ready for T001 to begin. |
| 2026-04-11 | implementation v1 | T001 | Replaced the old root solution with a new `Wombat.sln`, scaffolded `src/` and `tests/`, added central package management and editor config, and verified build/test/web/api startup locally. |
| 2026-04-11 | implementation v2 | T002 | Added ASP.NET Core Identity plumbing, role/admin seeding, claim-based authorization helpers, initial EF migration, and verified seeding against a fresh local Postgres database. |
| 2026-04-11 | implementation v3 | T003 | Added institution/speciality/sub-speciality domain slices, EF config and migration, admin Blazor maintenance pages, startup data seeding, and test coverage; build/test/migration/app-start verification passed, but a full authenticated manual walkthrough remains pending the later login UI work. |

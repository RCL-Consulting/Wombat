# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T019 — Activity builder UI + dynamic form renderer** (not started)

Next session: read `Tasks/T019-activity-builder-ui.md` and build on top of the T018 runtime now in Application + Infrastructure.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → T011 → T012 → T023 → T024 → T025 → T026 → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T010 completed:
- Added the shared web design system foundation for `Wombat.Web`:
  - replaced the default `wwwroot/app.css` with the full token-driven layout/form/table/card/alert/pager system from `DESIGN.md`
  - added Lucide SVG assets under `src/Wombat.Web/wwwroot/icons/`
  - added shared browser helpers in `wwwroot/wombat.js` and `wwwroot/js/dialog.js`
- Rebuilt the application shell:
  - rewrote `Components/Layout/MainLayout.razor` + `.razor.css`
  - created `Components/Layout/NavMenu.razor` + `.razor.css` with role-gated navigation and sign-out integration
  - updated routing so anonymous users are redirected to login and authenticated users see `/access-denied`
- Added the shared component library under `src/Wombat.Web/Components/Shared/`:
  - `Icon`, `PageHeader`, `Breadcrumbs`, `DataTable`, `FormField`, `FormActions`, `ConfirmDialog`, `PagerControls`, `StatePanel`, `Skeleton`, `Alert`, `PasswordToggleButton`, `RedirectToLogin`
- Reworked the account/error/home surfaces:
  - rewrote login, logout, register, profile, change-password, forgot-password, access-denied, error, not-found, and home pages to use the shared design system
- Rewrote the existing primitive admin pages to use the new page shell and components without changing MediatR handlers:
  - institutions/specialities/sub-specialities
  - EPAs
  - curricula + curriculum items
  - assessment forms
  - invitations
  - trainees
  - assessors
- Updated web/auth wiring:
  - added `ServerAuthenticationStateProvider`
  - enabled `UseStaticFiles()` / `UseRouting()`
  - set the identity access-denied path to `/access-denied`
  - added a fallback authorization policy requiring authenticated users
  - kept cookie auth POST endpoints aligned with the new account pages
- Added `tests/Wombat.Web.Tests/` with bUnit smoke coverage:
  - `DesignSystemSmokeTests`
  - `PageShapeSmokeTests`
  - `NavMenuAuthorizationTests`
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj -c Release --no-build --no-restore` passes with 16/16 tests green.
  - `dotnet test tests/Wombat.Architecture.Tests/Wombat.Architecture.Tests.csproj -c Release --no-build --no-restore` exits successfully, but the project still contains no discovered tests.
  - The T010 grep checks for legacy Bootstrap/table/icon/raw-hex patterns now return zero matches.
- Follow-up note:
  - `tests/Wombat.Architecture.Tests` still has no discovered tests, so architecture-boundary enforcement remains a Phase 7 coverage gap rather than something T010 validated.

T018 completed:
- Added the activity-runtime application slice under `src/Wombat.Application/Features/Activities/`:
  - DTOs for activity details, summaries, transitions, validation errors, and activity-type list items
  - service contracts: `IActivityService`, `ISchemaValidator`, `IWorkflowEvaluator`, `ICreditApplier`
  - commands: create draft, update draft, transition activity, rebuild curriculum progress
  - queries: get activity by id, list by subject, list actor inbox, list scoped activity types
- Added infrastructure implementations under `src/Wombat.Infrastructure/Activities/`:
  - `ActivityService` for orchestration and JSON merge/validation flow
  - `SchemaValidator` with required-field, numeric range, hidden-field, option, and scale checks
  - `WorkflowEvaluator` as pure actor-rule evaluation over `ClaimsPrincipal`
  - `CreditApplier` with curriculum-item matching and idempotent credit application
- Added `CurriculumItemProgress` in Domain plus EF configuration and the `CurriculumItemProgress` migration.
- Registered the new activity runtime services in Infrastructure DI.
- Added 21 application tests under `tests/Wombat.Application.Tests/Activities/` covering:
  - schema validation edge cases
  - workflow evaluator subject/creator/role/scope/all/any paths
  - deterministic evaluator behaviour over 1000 randomized runs
  - credit application match / non-match / minimum-level / idempotency cases
  - end-to-end create draft -> update -> transition -> progress update flow
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj -c Release --no-build` passes with 24/24 tests green when `DOTNET_CLI_HOME` is pointed at a writable workspace path.
  - `dotnet ef migrations add CurriculumItemProgress --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext --output-dir Persistence/Migrations --configuration Release --no-build` succeeds when `DOTNET_CLI_HOME` is pointed at a writable workspace path.
- Scope note:
  - Draft editing currently allows the subject or creator to edit non-terminal activities. If T019 surfaces a stronger per-state edit rule requirement, capture it as a focused follow-up instead of widening T018 retroactively.

T017 completed:
- Added the new activity-platform domain slice under `src/Wombat.Domain/Activities/`:
  - aggregates: `ActivityType`, `Activity`, `ActivityTransition`, `ActivityPermissionRule`
  - scope enum: `ActivityScope`
  - schema DSL + parser: `Schema/*`
  - workflow DSL + actor-rule grammar/parser: `Workflow/*`
  - credit-rules DSL + parser: `Credit/*`
- `ActivityType.PublishNewVersion(...)` now validates and normalizes schema/workflow/credit JSON before bumping the version.
- `Activity.ApplyTransition(...)` now parses the owning activity type's workflow, enforces that the requested transition is declared from the current state, updates state/data, and appends an `ActivityTransition` snapshot row.
- Wired EF Core for the activity platform:
  - added DbSets to `ApplicationDbContext`
  - added activity configurations under `src/Wombat.Infrastructure/Persistence/Configurations/Activities/`
  - mapped `SchemaJson`, `WorkflowJson`, `CreditRulesJson`, `DataJson`, and `SnapshotJson` as `jsonb`
  - added the composite dashboard index and GIN index in configuration
- Generated and committed the `ActivitiesPlatform` migration plus `Designer.cs`.
- Patched the migration to add the two required jsonb expression indexes:
  - `("DataJson"->>'epa_id')`
  - `("DataJson"->>'assessor_user_id')`
- Added 15 domain tests under `tests/Wombat.Domain.Tests/Activities/` covering:
  - schema parser success/failure cases
  - workflow parser success/failure cases
  - credit-rules parser success/failure cases
  - parser round-trips
  - `Activity.ApplyTransition(...)`
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Domain.Tests/Wombat.Domain.Tests.csproj -c Release --no-build` passes with 15/15 tests green when `DOTNET_CLI_HOME` is pointed at a writable workspace path.
  - `dotnet ef migrations add ActivitiesPlatform --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext --output-dir Persistence/Migrations --configuration Release --no-build` succeeds.
  - `dotnet ef database update --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext --configuration Release --no-build` succeeds against the local `wombat` Postgres database using the committed localhost connection string.
- Verification caveat:
  - The final task check "insert a sample `ActivityType` row and round-trip the stored jsonb payloads back through the parsers" was not automated in this session. The parsers are covered by unit tests and the migration was applied successfully to Postgres, but the explicit database-backed parser round-trip still remains available as a quick follow-up check if needed.

**Pass 4 — design-system pass (2026-04-12):**
- Created `DESIGN.md` as the canonical UI/design-system contract for the rewrite. Covers CSS custom properties, typography scale, layout grid, NavMenu, button system, table system, form system, card system, dashboard grid, alerts, skeleton loaders, pager, accessibility rules, icon strategy, page-level patterns (list/detail/form/dashboard/account), mandatory `app.css` section order, non-negotiables, and the task-graph mapping that shows T010 shipping the contract and T011/T019/all later UI tasks consuming it.
- Identity decision: port ClinicAssist's structure — token names, class names, spacing scale, layout grid — but use a distinct Wombat palette. Palette placeholder values in `DESIGN.md` currently mirror ClinicAssist; the `Wombat palette — TBD` block is the single place to change them before T010 executes.
- Rewrote `Tasks/T010-web-layout-auth.md` from a light "copy the pattern" task into an executable spec. Now depends on T002 and blocks both T011 and T019. Enumerates the 20 Lucide icons, gives the full `Icon.razor` / `MainLayout.razor` code shape, lists every shared component (`PageHeader`, `Breadcrumbs`, `DataTable<TItem>`, `FormField`, `FormActions`, `ConfirmDialog`, `PagerControls`, `StatePanel`, `Skeleton`, `Alert`, `PasswordToggleButton`), rewrites `Login.razor`, creates a new `tests/Wombat.Web.Tests/` project with bUnit smoke tests, and catalogs every primitive admin page (EpasList, CurriculaList, Institutions, Invitations, Trainees, Assessors, Profile, Register) that must be rewritten against the design system. Adds grep-based verification checks that forbid `class="table"`, raw hex outside `:root`, `.btn-outline-primary`, and `<i class="bi bi-*">`.
- Rewrote `Tasks/T011-role-dashboards.md`. Dependencies updated to T010 + T019 + T021 + T022 (not the superseded T008/T009). Adds a `DashboardCard.razor` shared component spec, new CSS classes (`.dashboard-metric*`, `.progress-bar*`, `.detail-card--emphasis`, `.detail-card--warning`, `.badge*`, `.status-dot*`), card-by-card wireframes for all seven role dashboards, a `DashboardRouter.razor` with explicit priority order matching DOMAIN.md, a role-switch cookie, a one-query-per-dashboard rule with `Get{Role}DashboardSummaryQuery` under `Wombat.Application/Features/Dashboards/`, a <200ms perf target, and bUnit test requirements. Notes that dashboards are the highest-visibility surface for T010 drift.
- Amended `Tasks/T019-activity-builder-ui.md`. Now depends on "T010 (web chrome + design system)". Added a top-of-file callout enumerating the exact classes and components the builder consumes, the candidate new classes it may need to add to `DESIGN.md` first (`.tab-bar`, `.tab-bar-tab`, `.builder-two-col`, `.field-type-icon`, `.state-diagram`), and the rule that no builder page may inline a `<style>` block. Added inline design-system cross-refs to the `ActivityTypesList`, `ActivityTypeEdit`, and Form-tab steps. Added five design-system compliance greps to the Verification section.
- Updated `Rewrite/README.md` document map to list `DESIGN.md`.
- Created `C:\Users\Renier\Wombat\CLAUDE.md` at the repo root, using ClinicAssist's CLAUDE.md section structure populated with Wombat rewrite facts: activity-platform pivot, `Rewrite/` task workflow, `current_state.md` handoff, reference folders, 9 roles, Linode + Caddy + systemd deploy, MediatR v12 cap, `DateOnly` for clinical dates, `IScopedSender` not `ISender`, `jsonb` storage, design-system non-negotiables (no Bootstrap Icons font, no `btn-outline-*` variants, no raw hex outside `:root`).

Rationale: the plan was solid on architecture but light on UI specifications — so the first Razor pages delivered in T003–T006 regressed to a primitive `<h1>` + `<table class="table">` shape far below the ClinicAssist reference. Fixing it retroactively in every task would drift. Fixing it once in `DESIGN.md` and having T010 ship the system-wide contract (before T011 and T019 consume it) is the cheap path.

T018 completed:
- See the "Last session notes" block above for full detail.

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

T004 completed:
- Added new domain slices for EPAs, entrustment scales/levels, curricula/curriculum items, and assessment forms/form criteria/form-EPA links.
- Added EF Core configurations, DbContext sets, and the `EpasAndCurricula` migration with updated model snapshot.
- Expanded `DataSeeder` so the demo tree now ensures a default `O-R Scale`, one sample EPA, and one sample curriculum with a seeded curriculum item under the demo sub-speciality.
- Added CQRS handlers, validators, and DTOs for:
  - EPA create/update/deactivate/list/get flows
  - curriculum create/update/list/get, item add/update/remove, and clone-as-new-version flows
  - assessment form create/update/deactivate/list/get, criterion add/remove, and EPA link/unlink flows
- Added two small global lookup queries for all specialities and all sub-specialities to support admin selectors.
- Added Blazor admin pages under:
  - `Admin/Epas/` for list + create/edit
  - `Admin/Curricula/` for list + create/edit + item management
  - `Admin/Forms/` for list + create/edit + criterion/EPA-link management
- Added tests for:
  - `Curriculum.CloneAsNewVersion()`
  - `CreateEpaCommandValidator`
  - `CreateCurriculumCommandValidator`
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj -c Release --no-build` passes with 6/6 tests green.
  - `dotnet ef migrations add EpasAndCurricula --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext --output-dir Persistence/Migrations --configuration Release --no-build` succeeds.
- Verification caveat:
  - The manual database-backed walkthrough from the task file (create EPA, form, curriculum, curriculum item and reload) was not run in this session because no fresh local verification database/connection override was configured for T004.

T005 completed:
- Added `Invitation` plus EF configuration and the `Invitations` migration, including first/last-name columns on `WombatIdentityUser` for invited registrations.
- Added `InvitationTokenService`, invitation CQRS handlers/queries, DTOs, and a logging `IEmailSender` stub.
- Added admin invitation issue/revoke UI plus account login/register/logout pages and route updates.
- Fixed several runtime integration gaps uncovered during real testing:
  - startup now runs `Database.MigrateAsync()` before seeding
  - all `EditForm`s now set `FormName`
  - auth cookie issuance moved to real `/account/login`, `/account/register`, and `/account/logout` POST endpoints because interactive component events cannot safely write auth headers
- Verified:
  - `dotnet build Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj -c Release --no-build --filter Invitations` passes with 5/5 tests green.
  - `dotnet ef migrations has-pending-model-changes --project src/Wombat.Infrastructure --startup-project src/Wombat.Web` reports no pending model changes.
  - Manual walkthrough passed on 2026-04-11: admin issued an invitation, invited user registered successfully, could log out and back in, and reusing the same invitation link was rejected.

T006 completed:
- Added concrete `TraineeProfile` and `AssessorProfile` domain entities, EF configurations, and the `Profiles` migration.
- Added an application-facing `IUserAdministrationService` plus an infrastructure implementation to load users, sync scope rows, promote `PendingTrainee` to `Trainee`, and update names.
- Added trainee CQRS for admission, update, deactivate, lookup, and pending-trainee listing.
- Added assessor CQRS for create/update and lookup/list flows, with a guard that only `Assessor` users can receive an assessor profile.
- Added account CQRS for self-service first/last-name edits.
- Added Blazor pages for:
  - `Admin/Trainees` pending admission + active-profile editing
  - `Admin/Assessors` list + edit
  - `Account/Profile`
- Follow-up fix during manual checking:
  - `Account/Profile` now saves through an explicit Blazor button + `EditContext` validation path instead of relying on form submit, because the original implementation could fall back to a plain browser POST/refresh on `/account/profile`.
- Updated home-page stubs so `PendingTrainee` sees onboarding copy and `Trainee` sees a dashboard placeholder after re-sign-in.
- Fixed a latent claims issue by having `WombatUserClaimsPrincipalFactory` load current scope rows from the database instead of relying on unloaded navigation collections.
- Extended `CurriculumDto` with speciality metadata because T006 admission/edit flows need curriculum-to-speciality resolution in the UI.
- Verified:
  - `dotnet build C:\Users\Renier\Wombat\Wombat.sln -c Release` passes with 0 warnings and 0 errors.
  - `dotnet test C:\Users\Renier\Wombat\tests\Wombat.Application.Tests\Wombat.Application.Tests.csproj -c Release --no-build --filter AdmitTrainee` passes with 2/2 tests green.
  - `dotnet ef migrations add Profiles --project src/Wombat.Infrastructure --startup-project src/Wombat.Web --context ApplicationDbContext --output-dir Persistence/Migrations --configuration Release --no-build` generated `20260411150759_Profiles` plus `Designer.cs` successfully.
- Verification caveat:
  - The manual end-to-end admin-admit / trainee-relogin walkthrough from the task file was not run in this session.

## Last known-good commit

`pending` — update after the T010 commit is created in this session

## Open questions

- **Icon set.** Decided in Pass 4: inline SVGs from Lucide, shipped via a shared `Icon.razor` component backed by `wwwroot/icons/*.svg` files. Bootstrap Icons font is forbidden. See `DESIGN.md` § Icons and T010 step on `Icon.razor`.
- **Wombat palette.** `DESIGN.md` currently carries ClinicAssist palette values as placeholders. Pick the distinct Wombat palette (primary, primary-hover, secondary, secondary-hover, sidebar gradient stops) before T010 implementation begins.
- **Will there be a `Programme` layer above `SubSpeciality`?** DOMAIN.md mentions this as "maybe later". Deferred; current plan still assumes Curriculum belongs directly to SubSpeciality. Revisit when T006/T017 make programme-level assignment pressure concrete.
- **Actor-rule grammar normal form.** T017 ships a deliberately tiny parser for `subject`, `creator`, `role:<name>`, `scope:<name>`, `+`, and `|`. T018 needs to decide whether the runtime evaluator treats mixed `+`/`|` expressions strictly left-to-right or whether it will reject ambiguous strings and require explicit future grammar expansion.
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
| 2026-04-11 | implementation v4 | T004 | Added EPAs, entrustment scales, curricula, curriculum items, assessment forms, admin CRUD pages, validators/tests, and the `EpasAndCurricula` migration; build and targeted application tests passed, but the manual DB walkthrough remains pending local connection setup. |
| 2026-04-11 | implementation v5 | T005 | Implemented invitation flow, invite/admin UI, registration/login/logout, invitation migration/tests, fixed runtime startup/auth integration issues discovered during manual exercise, and completed the full manual invite/register/login/reuse walkthrough. |
| 2026-04-11 | implementation v6 | T006 | Added trainee/assessor profiles, pending-trainee admission with role promotion, self-service profile editing, admin trainee/assessor Blazor pages, targeted admission tests, and the `Profiles` migration; then fixed the `/account/profile` save interaction after manual testing. Build/tests passed, but the full manual admit/relogin walkthrough remains pending. |
| 2026-04-11 | implementation v7 | T017 | Added the activity-platform aggregates, schema/workflow/credit DSL parsers, EF jsonb mappings, `ActivitiesPlatform` migration with json-path indexes, and 15 domain tests. Build, domain tests, and local Postgres migration update passed; explicit DB-backed parser round-trip remains optional follow-up verification. |
| 2026-04-11 | implementation v8 | T018 | Added activity-runtime application slice: DTOs, service contracts, commands/queries, infrastructure implementations (SchemaValidator, WorkflowEvaluator, CreditApplier), CurriculumItemProgress entity, migration, and 21 application tests (24 total green). |
| 2026-04-12 | planning v4 | — | Design-system pass: created `DESIGN.md`, rewrote T010/T011, amended T019, updated README.md/current_state.md, created root `CLAUDE.md`. Closes the GUI-spec gap between primitive HTML and the ClinicAssist reference. |

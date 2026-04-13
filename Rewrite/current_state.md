# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T012 — Email infrastructure** (next task after T011 on the critical path) — **Model: Sonnet**

T011 is now implemented. Next session: start `Tasks/T012-email-infrastructure.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → T012 → T023 → T024 → T025 → T026 → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T011 completed:
- Created the `DashboardCard.razor` shared component under `Components/Shared/` with Title, Icon, Href, Emphasis, Warning, Span, ChildContent parameters
- Added CSS classes to `app.css`: `.dashboard-metric*`, `.progress-bar*`, `.badge*` (5 state variants), `.status-dot*` (3 states), `.detail-card--emphasis`, `.detail-card--warning`, `.dashboard-card-body`, `.dashboard-card-footer`, `.list-unstyled`, `.muted`
- Updated `Rewrite/DESIGN.md` with new Badges, Status dots, and Dashboard widgets sections
- Added `DashboardThresholds` options class in `Application/Common/Options/` with `AssessorDueDays`, `CoordinatorStallDays`, `CommitteeCompletionPercent` (registered in `Infrastructure/DependencyInjection.cs`)
- Created 8 MediatR query+handler+DTO sets under `Application/Features/Dashboards/`:
  - `Trainee/` — curriculum progress (via CurriculumItemProgress), inbox, recent activities, upcoming deadlines (jsonb date scan)
  - `Assessor/` — pending request count, accepted activities with overdue check, recent decisions
  - `Coordinator/` — stalled requests (older than threshold), expiring invitations (within 3 days)
  - `SpecialityAdmin/` — pending review count, trainee counts (active/inactive), per-EPA coverage
  - `SubSpecialityAdmin/` — same shape as SpecialityAdmin, scoped to sub-speciality claims
  - `InstitutionalAdmin/` — user count by role (via IUserAdministrationService), speciality/sub-speciality counts
  - `CommitteeMember/` — trainees near completion threshold, programme progress overview
  - `Administrator/` — DB health check (lightweight count query), distinct user count
- Created 8 dashboard Razor pages under `Components/Pages/Dashboards/`, each with `@rendermode InteractiveServer`, `IScopedSender`, `StatePanel` + `DashboardCard` grid
- Replaced `Home.razor` with a DashboardRouter: resolves user's highest-priority role (via `DashboardPriority.Order`) and renders the corresponding dashboard inline; supports `wombat_preferred_dashboard_role` cookie override with switch links for multi-role users
- Added `Navigation/DashboardPriority.cs` with priority order constant, valid roles set, cookie name
- Added `/dashboard/switch/{role}` minimal API endpoint in `Program.cs` for role-switch cookie
- Registered `IHttpContextAccessor` in DI
- Tests:
  - 4 Application test classes (Trainee, Coordinator, InstitutionalAdmin, Administrator) covering 9 test cases — empty, partial, and full data scenarios
  - 2 bUnit test classes (DashboardCard: 8 tests, DashboardPriority: 6 tests) — card classes, emphasis, warning, spans, links, priority order
- Verified:
  - `dotnet build Wombat.sln -c Release` — all projects except Wombat.Domain.Tests (pre-existing NU1900 offline issue)
  - `dotnet test tests/Wombat.Application.Tests` — 42 passed
  - `dotnet test tests/Wombat.Web.Tests` — 33 passed
  - `grep '<style>' src/Wombat.Web/Components/Pages/Dashboards` — no inline styles
- Verification caveats:
  - Manual walkthrough (T011 acceptance script) not run — requires live DB + seeded users
  - Performance benchmark (200ms target) not run — requires seeded dataset with 100 trainees

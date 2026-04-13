# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T025 — Admin audit log (pipeline behaviour)** (next task on the critical path after T024) — **Model: Sonnet**

T024 is now implemented. Next session: start `Tasks/T025-audit-log.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → T025 → T026 → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T024 completed:
- Added Cronos 0.8.4 to `Directory.Packages.props` and both `Wombat.Application.csproj` / `Wombat.Infrastructure.csproj`
- Domain layer:
  - `ScheduledJobDefinition` entity (Id, Key unique, CronExpression, IsEnabled, Description)
  - `ScheduledJobRun` entity (Id, Key, StartedAt, FinishedAt?, Status enum, ErrorMessage?, TriggeredBy?)
  - `ScheduledJobRunStatus` enum (Running, Succeeded, Failed)
- Application layer:
  - `IScheduledJob` interface (Key, CronExpression, Description, ExecuteAsync)
  - `ScheduledJobContext` record (UtcNow, Logger)
  - `IScheduledJobRegistry` interface (Jobs list, GetByKey)
  - `IScheduledJobDispatcher` interface (DispatchNowAsync)
  - CQRS: `EnableScheduledJobCommand`, `DisableScheduledJobCommand`, `RunScheduledJobNowCommand`
  - Queries: `GetScheduledJobStatusQuery` (returns all definitions + last run + next run), `GetScheduledJobRunsQuery` (filtered run history)
  - DTOs: `ScheduledJobDto`, `ScheduledJobRunDto`
  - 4 new email templates: `DraftNudgeEmail`, `AssessorPendingNudgeEmail`, `MsfExpiryReminderEmail`, `CoordinatorDigestEmail`
- Infrastructure:
  - `ScheduledJobRegistry` — in-memory job registry with duplicate-key protection
  - `ScheduledJobDispatcher` — creates scoped DI, records ScheduledJobRun, executes job
  - `ScheduledJobHost` (BackgroundService) — 30-second tick loop, Cronos-based scheduling, per-key SemaphoreSlim overlap protection, startup seed/reconcile/catch-up
  - `ScheduledJobServiceCollectionExtensions` — `AddScheduledJob<TJob>()` extension
  - 8 jobs implemented:
    - `ActivityDraftNudgeJob` — daily 07:00, drafts older than 14 days, grouped by trainee
    - `AssessorPendingNudgeJob` — daily 09:00, activities with field:assessor_user_id actor waiting 5+ days, grouped by assessor
    - `MsfCampaignAutoCloseJob` — hourly, closes expired Open campaigns, anonymizes invitations
    - `MsfInvitationExpiryReminderJob` — daily 08:00, tokens expiring within 48h
    - `WeeklyCoordinatorDigestJob` — Monday 08:00, inactive trainees + MSF needing review + upcoming committee reviews
    - `PortfolioExportCleanupJob` — daily 02:00, deletes export records older than 90 days
    - `AuditLogRetentionJob` — daily 03:00, no-op placeholder until T025
    - `ScheduledJobRunRetentionJob` — daily 04:00, deletes run records older than 90 days
- EF Core:
  - `ScheduledJobDefinitionConfiguration` — unique index on Key
  - `ScheduledJobRunConfiguration` — indexes on Key, StartedAt, composite Key+StartedAt
  - DbSets added to `ApplicationDbContext`
  - Migration `20260413130000_Scheduling` with Designer + snapshot updated
- Web:
  - `ScheduledJobsList.razor` at `/admin/jobs` — DataTable with enable/disable toggle, "Run now" button, next run time
  - `ScheduledJobRunsList.razor` at `/admin/jobs/runs` — filtered run history with key/status/date filters
  - NavMenu: "Scheduled Jobs" link under Administrator section
- DI wired in `DependencyInjection.cs`: all 8 jobs registered, registry built from IScheduledJob services, ScheduledJobHost as hosted service
- Tests: 20 new tests (4 enable/disable, 3 status query, 4 runs query, 3 registry, 3 draft nudge job, 2 MSF auto-close, 1 export cleanup)
- Verification:
  - `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors)
  - `dotnet test tests/Wombat.Application.Tests` — 74 passed (was 54)
  - `dotnet test tests/Wombat.Web.Tests` — 33 passed
  - `dotnet test tests/Wombat.Domain.Tests` — 17 passed
- Verification caveats:
  - Live scheduler not tested — requires running app + PostgreSQL
  - Timezone-aware cron (per-institution) deferred — all crons evaluated in UTC
  - Email delivery not tested end-to-end — relies on T012 email infrastructure

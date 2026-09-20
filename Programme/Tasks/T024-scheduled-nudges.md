# T024 — Scheduled jobs (nudges, auto-close, digests)

**Phase:** 6 — Cross-cutting operations
**Depends on:** T012, T018, T021
**Blocks:** T016

## Goal

A dependable background-job subsystem that runs time-based work: remind trainees of stale drafts, nudge assessors sitting on pending activities, auto-close MSF campaigns past their `ClosesOn` date, send weekly digest emails, and anything else that needs to happen on a schedule rather than in response to a user action.

## Why not Hangfire / Quartz.NET

Both work, both are heavier than we need. Wombat runs on a single node (for now). A small hosted-service scheduler wired through `IHostedService` + a table-backed job log is enough, integrates cleanly with our DI, and avoids a second database schema to reason about. If the system later needs distributed scheduling, Quartz.NET drops in behind the same interface.

## What to do

1. **Job contracts** in `Wombat.Application/Scheduling/`:
   - `interface IScheduledJob` — `string Key { get; }`, `string CronExpression { get; }`, `Task ExecuteAsync(CancellationToken ct)`.
   - `ScheduledJobContext` record passed to handlers (clock, scope, logger).
2. **Scheduler host** in `Wombat.Infrastructure/Scheduling/`:
   - `ScheduledJobHost : BackgroundService` — every 30 seconds, iterates the registered jobs, uses `Cronos` (tiny library, MIT) to compute the next fire time, compares to the last successful run recorded in `ScheduledJobRun`, dispatches if due.
   - Each dispatch creates a fresh DI scope (`IServiceScopeFactory`), resolves the job, runs it, records success/failure + duration in `ScheduledJobRun`.
   - Overlap protection: a job already running is not dispatched again. Use a per-key in-memory `SemaphoreSlim`.
3. **Registry**: `IScheduledJobRegistry` — jobs register themselves via `services.AddScheduledJob<TJob>()` extension. Host reads the registry at startup.
4. **Persistence**:
   - `ScheduledJobDefinition` — Id, Key (unique), CronExpression, IsEnabled, Description. Seeded on startup from the registry so admins can toggle jobs off from the UI without a redeploy.
   - `ScheduledJobRun` — Id, Key, StartedAt, FinishedAt?, Status (`Running`, `Succeeded`, `Failed`), ErrorMessage?. Retained 90 days.
5. **Jobs to ship**:
   - `ActivityDraftNudgeJob` — daily at 07:00 local. Finds activities in `draft` state older than 14 days owned by Trainees, emails the owner a reminder.
   - `AssessorPendingNudgeJob` — daily at 09:00. Finds activities waiting on an assessor (state matches any "pending on assessor" transition per the workflow, older than 5 days), emails the assessor.
   - `MsfCampaignAutoCloseJob` — hourly. Transitions `Open` MSF campaigns past their `ClosesOn` date to `Closed`, runs aggregation.
   - `MsfInvitationExpiryReminderJob` — daily at 08:00. Emails respondents whose tokens expire in 48 hours and haven't been used.
   - `WeeklyCoordinatorDigestJob` — Monday 08:00. For each Coordinator, emails a summary: trainees at risk (no activities in 30 days), MSF campaigns needing review, committee reviews scheduled this week.
   - `PortfolioExportCleanupJob` — daily at 02:00. Deletes expired exports from `/opt/wombat/data/exports/` (see T023).
   - `AuditLogRetentionJob` — daily at 03:00. Archives audit rows older than the retention window (see T025).
6. **Admin UI** under `Wombat.Web/Components/Pages/Admin/Jobs/`:
   - `ScheduledJobsList.razor` — shows each job, next run time, last run status, enable toggle.
   - `ScheduledJobRunsList.razor` — run history with filters (key, status, date range).
   - "Run now" button for manual dispatch (requires Administrator role).
7. **CQRS**:
   - `EnableScheduledJobCommand`, `DisableScheduledJobCommand`, `RunScheduledJobNowCommand`.
   - `GetScheduledJobRunsQuery`, `GetScheduledJobStatusQuery`.
8. **Testing strategy**:
   - Each job is a small handler with its dependencies injected — unit-testable directly by calling `ExecuteAsync` with a fake `IClock` and in-memory DbContext.
   - The host itself is tested with a fake clock that ticks forward rapidly; assert jobs fire at their cron times.

## Files created

- `src/Wombat.Application/Scheduling/IScheduledJob.cs`
- `src/Wombat.Application/Scheduling/ScheduledJobContext.cs`
- `src/Wombat.Application/Scheduling/Jobs/**` (one file per job)
- `src/Wombat.Infrastructure/Scheduling/ScheduledJobHost.cs`
- `src/Wombat.Infrastructure/Scheduling/ScheduledJobRegistry.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/Scheduling/**`
- `src/Wombat.Infrastructure/Persistence/Migrations/*Scheduling.cs`
- `src/Wombat.Web/Components/Pages/Admin/Jobs/**`
- `tests/Wombat.Application.Tests/Scheduling/**`

## Verification

- [x] `dotnet build` clean (0 warnings, 0 errors).
- [x] `dotnet test` — 74 passed in Application tests (was 54; 20 new scheduling tests), 17 Domain, 33 Web.
- [ ] Manual: set `ActivityDraftNudgeJob` cron to `*/1 * * * *` for testing, confirm it fires within a minute, confirm email lands in MailHog, confirm a `ScheduledJobRun` row is written with `Succeeded`.
- [ ] Disable a job via the UI, confirm it stops firing. Re-enable, confirm it resumes.
- [ ] Run a job manually via "Run now", confirm it executes regardless of schedule.
- [ ] Kill the app mid-job, restart, confirm the partial run is marked `Failed` (startup reconciliation) and the next scheduled run behaves correctly.

## Notes & gotchas

- `Cronos` library is MIT, tiny, actively maintained. Use it. Don't write a cron parser.
- Cron expressions evaluated in the **institution's timezone**, not UTC. Store the zone on `InstitutionBrand` (see T023) and use it when computing next-fire times for per-institution jobs. Global jobs use the server timezone.
- Single-node scheduling means if the node is down, scheduled jobs are missed. The reconciler on startup should detect missed runs within a small grace window (e.g., 30 minutes) and catch them up. Beyond that, log and skip.
- Do not fire-and-forget email inside a job. Use the outbound email channel from T012 — the job enqueues, the worker sends.
- Nudge emails must be rate-limited: no more than one nudge per user per job per day, even if multiple objects are stale. The job groups by recipient and sends one digest email, not N.
- Document the "run now" button's audit trail: who dispatched what, when.
- Do not overload this subsystem with long-running computations. Jobs should complete in seconds. If a job needs to process thousands of rows, it schedules a queue of smaller work items and exits.

# T400 build contracts (flow 18)

The names two or more lanes share, and the ones each lane provides. Use them exactly; a lane that must differ says so in
its report. Each name says whether it exists (a path, at master `76cb7805` before § 0) or is new (on `t400`, written by
§ 0, or by the lane named). Lanes A, B and C run side by side in one wave (`build-lanes.md`); the runbook lane W follows
the merge and the sweep.

Flow 18 is six pages over four unrelated back ends (Home's health and the accounts, the institutions, the scheduled jobs
and their runs, the audit log), so its lanes are cut by page family, each taking its own Application and Infrastructure
work with it: A Home and the institutions, B the jobs and run history, C the audit log and an entry. What would otherwise
cross (the mail outcomes table, the schedule rule Home and the jobs page both judge by, the time helper every page prints
with, the names reader run history and the audit share, the address Home's "Its runs" opens) is § 0's. **No DTO
crosses a lane**: each lane owns its pages' DTOs and queries whole.

## § 0: on `t400` before any lane is cut, written by one Opus agent

- **The boards' text**: `design/flows/18-platform-operations/round-3/boards-text.md` (128 boards: Spec 1–3, Steps and the
  two check boards, then H, I, J, R, L, E, each at 1280 then its other widths) and `round-2/boards-text.md` (the 25
  standing ids with their twins, 50 screens: R2-I01, I02 (+390, 768), I03-390, I04, I05, I06, I17–I21; J11, J12; R03,
  R08, R10, R11; L14, L15; E01, E04, E05, E08, E10, E11; and R2-0-spec, R2-0-spec-2, R2-0-steps for reference only: round
  3's Spec is restated whole). One heading per board (`######## BOARD file`), the label strip's lines first; style,
  script, svg, head stripped; aria-labels, placeholders, values, `[checked]`, `[disabled]`, `[aria-disabled]`,
  `[selected]`, `[aria-haspopup]`, `[aria-current]`, `[aria-required]`, `[tabindex]`, `role` and alt text kept; a table
  row one `| a | b |` line, a `dt` opened `**`. Design data, read by every lane. A stdlib script in the integrator's
  scratchpad, run with `python -I` (flow 17's way).
- **The migration, and the one table it adds (Q18, K3; § 0 owns it, no lane adds another).**
  - NEW `src/Wombat.Domain/Email/EmailOutcome.cs`: `sealed class EmailOutcome` with `long Id`, `DateTime OccurredAt`
    (UTC), `EmailOutcomeKind Kind`, `string? Refusal`; `const int RefusalMaxLength = 2000`; factories
    `EmailOutcome.Sent(DateTime occurredAtUtc)` and `EmailOutcome.Dropped(DateTime occurredAtUtc, string? refusal)` (the
    refusal trimmed, cut to 2,000 characters, empty as null). NEW `enum EmailOutcomeKind { Sent, Dropped }`. No address,
    subject, body, reference or tag is a member, ever (Q18).
  - NEW `src/Wombat.Infrastructure/Persistence/Configurations/EmailOutcomeConfiguration.cs`: table `EmailOutcomes`, key
    `Id` (identity), `Kind` stored as the project's other enums are (read `ScheduledJobRunConfiguration` and its run
    status's mapping, and follow it), `Refusal` max 2000, an index on `OccurredAt` and one on (`Kind`, `OccurredAt`).
    Registered as the other configurations are (a `DbSet` on `ApplicationDbContext` only if its neighbours have one).
  - The migration `‹timestamp›_T400_EmailOutcomes` **generated** with `dotnet ef migrations add T400_EmailOutcomes
    --project src/Wombat.Infrastructure --startup-project src/Wombat.Web` (never `--no-build`; CLAUDE.md § EF Core
    migrations: the stale-output trap writes an empty migration), then **read before it is trusted**: it creates
    `EmailOutcomes` with both indexes and nothing else; its `.Designer.cs` is beside it and
    `ApplicationDbContextModelSnapshot.cs` gains the entity. If the tool cannot run, hand-write all three (the Designer
    copied from `20261030082058_T387_DataRightsFlow14.Designer.cs`, its `[Migration]` timestamp and class changed, the
    `BuildTargetModel` body and the snapshot updated), and say so. Then `dotnet ef migrations has-pending-model-changes`
    (same projects) exits 0 where the tool runs.
  - Tests: `Domain.Tests/Email/EmailOutcomeTests` (the factories; a 2,001-character refusal kept to 2,000; whitespace
    as null); **`Integration.Tests/Email/EmailOutcomePostgresTests`** (on PostgreSQL, through `MigrateAsync` on its own
    schema, as `ActivityTypeAdminScopePostgresTests` does, so a migration EF does not discover fails it: a Sent and a
    Dropped row written and read back, a 2,000-character refusal kept whole, the two indexes present in `pg_indexes`, and
    a read of the last 24 hours by `OccurredAt` returning only those rows).
- **What the mail path tells Home (Q2, Q18)**: NEW `src/Wombat.Application/Common/Email/IEmailHealth.cs`:
  `interface IEmailHealth { bool IsConfigured { get; } int QueueDepth { get; } }`, the in-process half only (whether
  `Email:SmtpHost` is set; the queue's depth in the host that draws Home). The 24 hours, the last sent and the last drop
  are read from `EmailOutcomes`, never kept in memory. Lane A implements and registers it; § 0 writes the interface only.
- **The schedule rule (K4, K5)**: NEW `src/Wombat.Application/Scheduling/JobSchedule.cs` (Application references Cronos
  already: `GetScheduledJobStatusQueryHandler.cs:1`), `public static class JobSchedule`:
  - `bool TryParse(string cron, out CronExpression? expression)` (no throw, no log).
  - `DateTime? NextAfter(CronExpression cron, DateTime? lastRunStartUtc, DateTime nowUtc)`: the next occurrence after the
    last run's start, or after `nowUtc − 1 day` for a job never run: **the host's rule**, `ScheduledJobHost.cs:110–117`,
    word for word in its effect.
  - `bool IsDue(CronExpression cron, DateTime? lastRunStartUtc, DateTime nowUtc)`: `NextAfter(...) <= nowUtc`.
  - `TimeSpan OverdueGrace` = 1 hour (Q3), and `bool IsOverdue(CronExpression cron, DateTime? lastRunStartUtc, bool
    lastRunRunning, DateTime nowUtc)`: false while the last run is Running (it holds its lock, `:80–85`, K4); else
    `NextAfter(...) + OverdueGrace < nowUtc`.
  - `ScheduledJobHost.IsDue` (`:97–120`) keeps its signature and its warning on a bad cron, and calls `TryParse` and
    `JobSchedule.IsDue`: § 0 changes nothing else in the host. Tests: `Application.Tests/Scheduling/JobScheduleTests`
    (each of the eight crons; a last run at 21:00 SAST and an hourly cron next at 22:00 SAST, R3-J03's K5 case; a job
    disabled past its time read as due at once; Running never overdue; one hour of grace exactly; a cron that does not
    parse); `ScheduledJobCompositionTests` and the jobs' own tests green.
- **The names reader (M3, Q15)**: `DataRightsPeople.NamesOfAsync(IApplicationDbContext dbContext, IUserAdministrationService
  users, IReadOnlyCollection<string> ids, CancellationToken ct) → IReadOnlyDictionary<string, string>`, NEW in
  `src/Wombat.Application/Features/DataRights/Queries/DataRightsPeople.cs` (additive: `ErasuresOfAsync` then
  `StaffNamesAsync`, `:69`, in one call; the class stays `internal`, which every Application handler reaches). An erased
  person reads as their pseudonym, a living one as the live "First Last", an account with no name as its id; the stored
  `ActorDisplay` is never consulted (it keeps the real name, `ErasureExecutor.cs:440–452`). Tests:
  `Application.Tests/DataRights/DataRightsPeopleNamesTests` (live, erased by old id, erased by pseudonym, nameless, an id
  twice). Lane B calls it for Ran by; lane C for Person and Who.
- **The time helper (K7, Q7)**: NEW `src/Wombat.Web/Components/Shared/SastTime.cs`, `public static class SastTime`, the one
  place Web turns a UTC moment into South African words (UTC+2, no daylight saving; the day by
  `ProgrammeCalendar.DateOf`, so the two never disagree; a `.razor` file never names `ProgrammeCalendar`):
  - `Minute(DateTime utc)` → "2026-11-10 21:39 SAST"; `Second(DateTime utc)` → "2026-11-10 21:39:41 SAST";
    `Clock(DateTime utc)` → "21:39" (the window line's "to ‹hh:mm› SAST" adds its own " SAST"); `Day(DateTime utc)` →
    "2026-11-10"; `StoredUtc(DateTime utc)` → "2026-11-10 17:01:41.861 UTC" (the entry's "Stored as …" line, T4);
    `Minute(DateTimeOffset)` for `ErrorPages`.
  - `FromSastInput(DateTime sastLocal) → DateTime` (UTC) and `ToSastInput(DateTime utc) → DateTime` (unspecified kind),
    for the audit filters' `datetime-local` (today tagged UTC, `AuditList.razor:185–186`).
  - `ErrorPages.TimeOf` (`Navigation/ErrorPages.cs:136`) and `ActivityListWords.DateOf` (`ActivityListWords.cs:94`) keep
    their public signatures and call it; their private offsets (`:51`, `:13`) go. The other private copies
    (`DataRightsRequestWords.cs:156`, `MyDataRightsWords.cs:224`) are not the design's to change and stay.
  - A day's UTC range for run history's From and To is the query's (lane B, `ProgrammeCalendar.EndOf`), not Web's.
  - Tests: `Web.Tests/Shared/SastTimeTests` (22:30 UTC on 2026-11-10 is "2026-11-11 00:30 SAST" and the day 2026-11-11;
    milliseconds; the input round trip); `ErrorPages`' and `ActivityListWords`' tests green unchanged.
- **Run history's address (T18, R3-H05, R3-R09)**: NEW `src/Wombat.Web/Components/Shared/JobLinks.cs`: `Jobs`
  (`/admin/jobs`), `Runs` (`/admin/jobs/runs`), `JobParameter` = `"job"`, `StatusParameter` = `"status"` (its values the
  run status's names: `Succeeded`, `Failed`, `Running`), `RunsOf(string? jobKey, string? status)` →
  `/admin/jobs/runs?job=entrustment-decision-expiry&status=Failed` (escaped; a null part left out). Home's "Its runs"
  (lane A) builds with it; run history (lane B) reads its query by these names (`[SupplyParameterFromQuery(Name = …)]`)
  and may add `from`, `to` and `page` of its own. Tests: `Web.Tests/Shared/JobLinksTests`.
- **`app.css`**: (1) **C1** (S9): `.institutions-page`, `.jobs-page` and `.audit-page` join each of the **seven** selector
  lists of "Curricula and the items stack to 1150px" (`app.css:7708–7770`: thead; tbody tr; tbody tr:last-child; td and
  tbody th, two selectors each; th[scope="row"]; td[data-label]::before; .btn), and `.job-runs-page` each of the seven
  of "The catalogue tables stack from 1024px down" (`:7633–7702`); each block's comment names them with S8's reason (a
  readability choice, not an overflow fix: R3-0-spec-3 § 10). No declaration is written; the canvas's written-out
  stand-in (`f18r3.css` C1) never ships. (2) After `/* end Flow 17: the builder */`, a section heading `/* ── Platform
  operations ── */` and three lane headings with their `/* end … */` lines and nothing between: "Flow 18: Home and the
  institutions (T400). Lane A.", "Flow 18: the jobs and run history (T400). Lane B.", "Flow 18: the audit log and an
  entry (T400). Lane C." (3) DESIGN.md: § app.css section order gains the section, the C1 line and one placeholder line
  per lane; § Table system's narrow-desktop stack paragraph names the four lists and S8's reason. `NarrowLayoutTests` pins
  C1 (each class in all seven lists of its block).
- **The baseline counts**: § 0 runs the six suites at master first and on `t400` after its commit, and writes both in
  `build-lanes.md` § "§ 0's counts".

## § 1: lane A (Home and the institutions) provides

- `IEmailHealth`'s implementation (NEW `src/Wombat.Infrastructure/Email/EmailHealth.cs`, a singleton: `IsConfigured`
  from `Email:SmtpHost`, `QueueDepth` from `EmailQueue.Reader.Count`), registered in `AddInfrastructure` for both hosts.
- The mail outcomes written (K3): `EmailWorker` records one `EmailOutcome` per mail it finishes with, at
  `SendWithRetryAsync`'s two returns (`EmailWorker.cs:150` Sent, `:160` Dropped with the redacted failure's message,
  `EmailLog.Redact`, `:154`), and Dropped **with no refusal** at `AbandonAsync` (`:104`) and the backoff wait's return
  (`:179`); never through `ReportAsync` (`:195–198` returns early for a mail with no `DeliveryKey`); a mail cut off in
  flight (`:136`, outcome null) records nothing, as it reports nothing. A recording that fails is logged and never
  throws (the next mail is sent). `LoggingEmailSender` records nothing. Either host writes, so Home reads both (K20).
- `IUserAdministrationService.CountAccountsAsync(DateTime nowUtc, CancellationToken)` → NEW `record AccountCount(int
  Accounts, int Locked)` (K2, Q4, T14): every account no `DataRightsErasureRecord` names, and among them those whose
  lockout is still running at `nowUtc` (an administrator's indefinite lock or a sign-in lockout); an erased account is in
  neither. A default interface implementation (as `GetDisplayNamesAsync`'s, so hand-built directories compile), and the
  real one in `UserAdministrationService` in one query. Read by lane A alone.
- `GetAdministratorDashboardSummaryQuery`'s new DTO (per part, K11: accounts, jobs, mail each read or not read; the
  moment the database failed), and the institutions' pages. Read by no other lane.
- `CatalogueLinks`' institution addresses (additive): `Institutions` (`/admin/institutions`), `NewInstitution`
  (`/admin/institutions/new`), `Institution(id)`, `InstitutionCreated(id)` (`/admin/institutions/7?created`, the existing
  `CreatedToken`, `CatalogueLinks.cs:38`; `Tokens`, `:55`, unchanged; K13).

## § 2: lane B (the jobs and run history) provides

- `IScheduledJobDispatcher.DispatchNowAsync` returning NEW `record ScheduledJobRunOutcome(ScheduledJobRunStatus Status,
  DateTime StartedAt, DateTime? FinishedAt, string? ErrorMessage)` (Application), and NEW `ScheduledJobRefusal`
  (Application, `src/Wombat.Application/Scheduling/ScheduledJobRefusal.cs`, an `InvalidOperationException` carrying its
  sentence) for both refusals: already running (the built sentence, moved from `ScheduledJobDispatcher.AlreadyRunning`,
  `:66–68`) and disabled (T22). `RunScheduledJobNowCommand` answers the outcome. Read by lane B alone.
- `ScheduledJobRunRetentionJob` also deletes `EmailOutcomes` older than 90 days (Q18), its Description "Deletes job run
  records and mail outcomes older than 90 days."; every job's Description without its UTC clause (T15).
- The scheduling DTOs and queries (status, runs with a page and a total). Read by no other lane.

## § 3: lane C (the audit log and an entry) provides

- `AuditPipelineBehavior`'s failure text (K10, P1): a refusal's words, else "The action failed.", the text to the log.
  A refusal is a `ValidationException`, or an `UnauthorizedAccessException` or `InvalidOperationException` **thrown by
  Wombat's own code** (its throw site in a `Wombat.` assembly; the fix pass applied the test to
  `UnauthorizedAccessException` too, the operator's F1, 2026-11-12: the framework throws it for file access with a path
  in its text, which stores "The action failed." and goes to the log whole).
  Every command's failed row changes with it, every flow's: lane C's tests and the review say so.
- The audit DTOs and queries (the resolved window; the Institution filter of three kinds, K15; names through § 0's
  reader; the institution's name per row; `ActorKind`, K8). Read by no other lane.

## § 4: words and constants already built and kept (lanes use them, do not reword)

- The institution's refusals (`UpdateInstitutionCommandValidator.cs:12–24`): "Type the institution's name. The record was
  not saved.", "Type a short code. The record was not saved.", "Keep the name to 200 characters. The record was not
  saved.", "Keep the short code to 32 characters. The record was not saved.", "Enter a valid email address, for example
  name@example.org."; the duplicate "An institution with the same name or short code already exists."
  (`UpdateInstitutionCommandHandler`'s `Duplicate`); `InstitutionEdit`'s `LoadFailed`, `SaveFailed`, `LoadingLine`,
  `HeadingBeforeTheRead` ("Institution"), `RecordSubtitle`; the deactivation question's body ("No new invitation can be
  issued for it until it is reactivated. Its people, their records and the invitations already sent are kept. You can
  reactivate it on this page.", `InstitutionEdit.razor:134`); `InvitationText.RequiredMarkLine` ("* marks a field you
  must fill in."), `InvitationText.Address`.
- "Job '‹key›' is already running, so it was not started again. Wait for that run to finish, then run it again if it is
  still needed." (`ScheduledJobDispatcher.cs:66–68`, moved into `ScheduledJobRefusal` word for word).
- `PagerControls`' words ("Showing 1–50 of 174", "Per page:", Previous, Next) and order (M2); `ConfirmDialog`'s action
  mode (opening on Cancel, `DangerAction`, `ReturnFocusId`, `BodyContent`); `StatePanel` (`LoadError`, `OnRetry`,
  `FocusAfterRetry`, `LoadingContent`); `ActionResult` (`FocusAfterRender`, `FocusOnLoad`); `DashboardFrame`,
  `DashboardCard` (`HeadingId`, `Span`, `Emphasis`), `.dashboard-card-footer` as `AssessorDashboard.razor:39–41` writes
  it; `DataTable` (`StackWide`, `Caption`, `Class`); `RowNames.Distinct`; `BadgeFor.JobRun`, `BadgeFor.AuditResult`,
  `BadgeFor.State`; `InFlight`; `PageFocus.FocusByIdAsync`; `WithoutTokens`; `WombatRoleLabels.For`;
  `.people-page`, `.record-page`, `.user-page`, `.user-section`, `.person-cell`, `.list-section-title`, `.detail-list`,
  `.code-block`, `.search-container`, `.search-grid`, `.filter-actions`, `.visually-hidden`.
- Flow 01's Access denied ("You cannot open this page", A.5.2, R2-I21), unchanged.

## Across lanes

- **Refusals by type, failures in fixed words** (T23, T329): a page shows a refusal (a typed refusal, a
  `ValidationException`'s messages, `UnauthorizedAccessException`, `InstitutionUpdateRefusals.IsRefusal`) in its own
  words, and anything else in the board's fixed failure sentence, logged. **No page of this flow calls `RefusalText.Of`
  or prints `exception.Message`** (`InstitutionsList.razor:63`, `InstitutionCreate.razor:83`, `InstitutionEdit.razor`'s
  three, `ScheduledJobsList.razor:117`, `:149`, `:180`, `ScheduledJobRunsList.razor:109`, `AuditList.razor:193`,
  `AuditDetail.razor:101` all go). `RefusalText.Of` itself is unchanged (T399 is the other pages').
- **Out of scope is Unavailable** (CLAUDE.md § Roles: 404, not 403, and never which cause): an institution (Q17, both
  roles, R2-I19/I20) and an audit entry (R2-E08) each read their page's own Unavailable, no id shown.
- **Times**: every time a person reads is `SastTime`'s; a day is the South African day; never `ToLocalTime()`,
  `DateTime.Now`, `DateTime.Today` or a hand-written offset on a page or in a handler; a handler's "now" is
  `TimeProvider`'s (registered, `DependencyInjection.cs:127`), so a test can pin it.
- **Names**: a person by the live "First Last" through the directory, an erased one by their pseudonym (§ 0's reader);
  "Demo Administrator" and "Scheduler" on run history; a user id only where the audit pages draw one (the filter, Who's
  User id).
- **The institution scope of the audit (T101) is not widened**: a row with no institution stays the Administrator's; a
  name, an institution's name or a filter is read only for the rows the caller may already read.
- **No new role check, route or nav item** (Q1). Titles: `build-lanes.md` § How.

## § 5: words on no board, and the NEW sentences beside what they replace (C5)

Rows 1–8 are round 3's NEW sentences, given here the "replaces" cell the check found missing (`round-3-check.md` C5,
T400's Notes: carried into the build). The fix pass appends the words the lanes ship that no board draws (rows 9 on),
from the lanes' reports and the pages review, as flow 17's § 5 did. Rows 9–34 are the pages review's 26 groups
(`build-review-pages.md`, the last section), in its order; rows 35 and 36 are the fix pass's own. Not NEW though the lanes
listed them: "Set by a Wombat administrator.", "Save changes" and " It has no contact email now." are flow 12's.

| # | Words | Where | Replaces |
|---|---|---|---|
| 1 | "‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST. The application log names each one." (the Failing mail line when the last drop has no refusal: a mail abandoned as the host stopped, or whose last try failed in the backoff wait; K3) | lane A, Home's mail line | nothing: no line says anything of mail today (`AdministratorDashboard.razor:19`'s "Email queue (T012)" stub, retired whole by T8) |
| 2 | "‹key›'s schedule cannot be read (cron ‹cron›), so the scheduler does not run it. The application log says why." (badge Failing; K4) | lane A, Home's jobs line | nothing: today an unreadable cron shows only "—" for its next run on Scheduled jobs (`GetScheduledJobStatusQueryHandler.cs:45–48`) and is a warning in the log (`ScheduledJobHost.cs:106`) |
| 3 | "Enabled ‹key›. It runs at the next check, within 30 seconds." (a job due as soon as it is enabled; K5) | lane B, the jobs page's result | "Job '‹key›' enabled." (`ScheduledJobsList.razor:141`), as T5's "Enabled ‹key›. Its next run is …" does for a job not yet due |
| 4 | "At the next check, within 30 seconds" (Next run of a job already due; K5) | lane B, the jobs list's Next run | the next occurrence after now (`GetScheduledJobStatusQueryHandler.cs:43`), which for such a job named a time the host would not keep |
| 5 | "Cut to its first 24 bits when stored." (beside an IPv4 address cut to /24; K9) | lane C, the entry's Who | nothing: today the address alone (`AuditDetail.razor:47`); round 1's 48-bit twin beside an IPv6 /48 |
| 6 | "The action failed." (a Failed row's stored text when its exception is no refusal; K10, P1) | lane C, `AuditPipelineBehavior` | the exception's own text (`errorMessage: ex.Message`, `AuditPipelineBehavior.cs:104`) |
| 7 | "1 run." and "1 action." (K19) | lane B and lane C, the count lines | nothing: the singulars of T17's "21 runs." and "166 actions.", which replace the pager's count alone |
| 8 | "Nothing sent since ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours." (a mail server set and nothing sent in the 90 days the table holds; Q18, R3-H12) | lane A, Home's mail line | nothing: no line says anything of mail today (as row 1) |
| — | "Deletes job run records and mail outcomes older than 90 days." (Q18; listed for completeness, its board is every R3-J) | lane B, the retention job's Description | "Deletes ScheduledJobRun records older than 90 days (daily at 04:00 UTC)." (`ScheduledJobRunRetentionJob.cs:20`), whose T15 form would have been "Deletes job run records older than 90 days." |
| 9 | "1 mail was dropped in the last 24 hours, the last at ‹time›…" (n = 1) | lane A, Home's mail line (`AdministratorHomeWords`) | R3-H04's "‹n› mails were dropped …" at n = 1 |
| 10 | "1 is locked and cannot sign in now." | lane A, Home's Accounts | R3-H10's "‹n› are locked …" at n = 1 |
| 11 | "‹n› waiting" in the Mail sentences | lane A, Home's mail line | "none waiting" (R3-H03, H12) when the queue holds mail |
| 12 | "‹key› has not run since it was due: the scheduler did not start it. The application log says why; Run now runs it." (no time) | lane A, Home's jobs line | R3-H07's "… due at ‹time› SAST: …" for a job with no next occurrence |
| 13 | "‹key› has not run yet." | lane A, Home's jobs line | R3-H02's "… has not run yet: its first run is ‹time› SAST." when no first run is known |
| 14 | "Two / Four … Ten jobs need you:" (a number in numerals above ten) | lane A, Home's jobs line | R3-H09's "Three jobs need you:" |
| 15 | "‹n› job last succeeded." and "All 1 job last succeeded." | lane A, Home's jobs line | R3-H01/H02's "7 jobs …" / "All 8 jobs …" at n = 1 |
| 16 | "account" (the singular label) | lane A, Home's Accounts | "accounts" (R3-H01) at 1 |
| 17 | "No institutions yet" and "Create the first institution to start configuring scope and onboarding." (kept from master; no board draws an empty list) | lane A, the institutions list (`InstitutionWords`) | nothing |
| 18 | "Could not deactivate the institution. Nothing has changed. Try again, or come back in a few minutes." | lane A, the record's Status section | nothing; the twin of R3-I22 for the Status section |
| 19 | "Could not reactivate the institution. …" | lane A, the record's Status section | nothing; as row 18 |
| 20 | "Could not save the institution. …" on the create page (R3-I22 draws it on the record) | lane A, Create institution | master's create-page failure text |
| 21 | "None: the schedule cannot be read" (Next run, an unreadable cron) | lane B, Scheduled jobs (`JobWords`) | "—" (K4's note; the Home sentence is row 2) |
| 22 | "Enabled ‹key›." (no next run known) | lane B, the jobs page's result | R3-J03's "Enabled ‹key›. Its next run is ‹time› SAST." |
| 23 | "‹key› could not be enabled." and "‹key› could not be started." | lane B, the jobs page's failure | nothing; twins of R3-J10's "could not be disabled" |
| 24 | "The filters are kept above. Clear them to see the one run." | lane B, Run history's no-match card | R3-R03's "… see all ‹n›." at n = 1 |
| 25 | Durations "1.2 s" and "2 min 5 s" (R3-R07 draws "7 ms") | lane B, Run history's Duration | the old page's Finished column |
| 26 | "Unknown" (a status that is none of the three) | lane B, both job pages | an enum name read through |
| 27 | "2,800 runs." (a thousands separator) | lane B, Run history's count line | extends R3-R01's "21 runs." |
| 28 | "Run now needs a signed-in person." (audit text only, never on a page) | lane B, `RunScheduledJobNowCommandHandler` | nothing |
| 29 | "Read from ‹From› SAST to ‹To› SAST. Turning a page keeps this window." (a To typed) | lane C, the audit log's window line (`AuditWords`) | R3-L01's "… to ‹hh:mm› SAST, when you pressed Show. …" |
| 30 | "Read from ‹From› SAST to ‹hh:mm› SAST, when you opened the page. …" (a From typed, the address opened) | lane C, the audit log's window line | R3-L's "… : the last 24 hours, when you opened the page. …" |
| 31 | " by ‹person›" in a Detail's name when two rows share action and second | lane C, the audit log's Detail | nothing; extends C7's "Login at ‹time›" |
| 32 | "None" for a missing address, subject type or subject id (R3-E02 draws it for User id only) | lane C, the entry | "—" (master) |
| 33 | The category "Other" (a value the label map does not know) | lane C, the audit log and the entry | an enum name |
| 34 | The entry's subtitle for an actor with no name ("‹action› by No signed-in person, …"), which composes two board phrases | lane C, the entry | nothing |
| 35 | "No scheduled job is stored yet: the scheduler stores them as it starts." (badge Not configured; no job definition stored, which only a host whose scheduler never started reaches) | the fix pass (N7), Home's jobs line | "All 0 jobs last succeeded." on an OK badge |
| 36 | "Only a Wombat administrator may read the system's health." and "Only a Wombat administrator may run a scheduled job." (audit text only, never on a page: the pages are the Administrator's) | the fix pass (N-3), the Home query and Run now | nothing: the handlers checked no role |

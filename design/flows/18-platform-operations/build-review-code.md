# T400 (flow 18) build: code review of § 0 and lanes A, B and C (Application and Infrastructure)

Reviewed: `git diff 367b3c10..HEAD -- src tests` at `07920cc9`, minus the Web pages (§ 0 `9f53214e`, lane A `e65df391` `7ee09a04`,
lane B `41afb119` `47dbb988`, lane C `b729a039` `1cba10a1` `02c38cfe`). Read against CLAUDE.md (Roles, InstitutionalAdmin
scope, EF Core migrations, jsonb), `build-lanes.md` § Reviews and the fix pass (P1), `build-contracts.md`, `build-s0-notes.md`.
Line numbers are the worktree's files at that head.

**Counts: 0 must, 2 should, 8 nit.**

Run (observed), `-c Release`, no `--no-build`: the full Integration suite against the real PostgreSQL, **541 passed, 0 failed, 0
skipped** (6 m 28 s). `EvidenceEpaMigrationPostgresTests.AfterTheMigration_TheSeedRefresherFindsThePatchedSeedTypesInSync` passed;
the earlier lane run's one failure under load **did not reproduce**. I also ran a scratch program (outside the repo) to check
P1's throw-site test against the framework's own `InvalidOperationException`s (below).

## What holds (checked, no finding)

- **Migration `20261111182215_T400_EmailOutcomes`.** The Designer is the previous Designer (`20261030082058_T387…`) with only the
  `[Migration]`/class name and the `EmailOutcome` entity block added (diffed whole); the snapshot's added block is byte-for-byte
  that block; `Up` creates exactly the table, its PK and the two indexes the configuration declares, `Down` drops the table;
  nothing else in the model changed. The PostgreSQL test reaches it through `MigrateAsync`, so a missing Designer would fail it
  (`EmailOutcomePostgresTests`, also asserts the three index names).
- **`EmailOutcome`.** No address, subject, body, reference or tag is a member; the refusal is trimmed, empty to null, cut to 2,000 (the
  column's width).
- **`EmailWorker` outcome recording.** Four points: sent (`:159`), dropped after the last attempt (`:171`, with `failure.Message`, which is
  `EmailLog.Redact`'s `RedactedException` message, so To and Cc, their IDN forms and bare local parts are replaced: the test pins
  `<[recipient]>` and `NotContain("nurse-1")`), abandoned at stop (`:117`) and abandoned while waiting to retry (`:191`), both with a
  null refusal. The cut-off mail records nothing. `RecordAsync` (`:236-253`) wraps scope creation, resolution and the save in one
  `try`, logs through `EmailLog.Redact`, `GetService` (not `Required`) so the no-recorder host records nothing and logs nothing.
  It is awaited after the send and after the "sent" log line, outside the send's `try`, so a failing recording can never count a sent
  mail as a failed attempt. `LoggingEmailSender` writes nothing (the recorder is registered only in the mail-server branch,
  `DependencyInjection.cs:107-110`). Both hosts register the worker through `AddInfrastructure`, and both write the same table.
- **`EmailQueue` without `SingleReader`.** The only reader is `EmailWorker` (`grep` of `Reader` across `src`: the worker's
  `ReadAllAsync`/`TryRead`, and `EmailHealth`'s `CanCount`/`Count`). One worker is registered per host, so the single-consumer
  assumption is still true; without the hint the channel is merely a little slower, with no ordering or safety change, and `CanCount`
  becomes true, which `EmailHealth.QueueDepth` needs. No concurrency hazard.
- **Home's query.** Probe, accounts, jobs and mail are each caught alone (`ReadPartAsync`, `:99-111`), a failure logged and the part null;
  cancellation is not swallowed. No Identity type crosses into Application (`AccountCount` is a record in the interface file; the
  default method uses `UserIdentityDetails`). The 24-hour drop window is `OccurredAt > now - 24h` (the PostgreSQL test pins "exactly 24
  hours ago: outside", "a millisecond inside"); the "since" window is `>= now - 90 days`, the same 90 the retention job deletes at
  (`< cutoff`), so the line says only what the table can hold. Mail not configured reads Not configured with the table unread (R3-H08).
- **`CountAccountsAsync`.** One statement (`GroupBy(_ => 1)` over `Users` with a `NOT EXISTS` on `DataRightsErasureRecord.UserId`): the erased
  are in neither figure; the lock is `LockoutEnd > now` which counts PostgreSQL's `infinity` (the test guards the stored value as
  `infinity`) and a running sign-in lockout, not a lapsed one; empty database answers 0/0 via `SingleOrDefaultAsync`. The test
  records the command text and asserts a single round trip. The default interface method ignores `nowUtc` and the erasure by its own
  doc; only hand-built directories reach it.
- **`JobSchedule` and the host.** `TryParse` is `CronExpression.TryParse` (five fields, as `Parse` was; empty and white space are false).
  `IsDue` is exactly the old inline rule (`next ≤ now`, base the last run's start or `now - 1 day`). `IsOverdue` is false while the last
  run runs, else `next + 1 h < now` (strict); a never-run job is judged by the same base, so a daily job never run is Overdue only
  once an hour past its slot (and the host runs it within 30 s). Every moment is UTC (Cronos requires it); Home `SpecifyKind`s the
  stored start. The host's `TickAsync` and `CatchUpMissedRunsAsync` are otherwise unchanged (the catch-up keeps its own parse and
  grace; it is not a second "due" rule, the tick runs a missed job anyway).
- **Run now.** `ScheduledJobDispatcher.DispatchNowAsync` takes the job's lock first (`TryEnter`, `using var held`), then, in a fresh scope,
  reads the stored definition `AsNoTracking`, and throws `ScheduledJobRefusal.Disabled` only for a stored definition with
  `IsEnabled == false` (none stored runs), all before the run row is added; the refusal is an `InvalidOperationException` thrown
  from a Wombat assembly, so P1 stores its sentence. `RunScheduledJobNowCommandHandler` reads the id from the principal's
  `NameIdentifier`, refuses a missing or blank id with `UnauthorizedAccessException` before anything is staged, and returns the
  outcome built from the saved run (`StartedAt`/`FinishedAt` UTC). `AuditPayloadSerializer` skips `ClaimsPrincipal` properties
  (`:34`), so the new `Principal` member does not enter the audit summary. The tests cover disabled, already running (under the
  lock, with the lock held the disabled job reads as running), no-id principal, unknown key (a failure, not a refusal), no stored
  definition (runs).
- **Run history.** `PageSizes` {10, 20, 50, 100}, anything else is 50; page clamped to `[1, lastPage]`; `From`'s start is
  `EndOf(from - 1 day)` (that day's 00:00 SAST = 22:00 UTC the day before) and `To`'s end is `EndOf(to)`, exclusive; the PostgreSQL
  test pins 00:00 SAST in and 22:00 UTC out over 72 hourly runs. `TriggeredBy` goes through the shared names reader (every id has an
  entry, so `names[id]` cannot miss).
- **Retention.** `EmailOutcomes` older than `now - 90 days` are removed in the same save as the runs; the PostgreSQL test pins both
  tables at the cutoff minute.
- **Audit list scope (T101).** An Administrator reads all; any other caller is pinned to `InstitutionId == their claim` (no claim, empty), the
  scope name is read only for them, and the Institution filter is applied only in the `else if` Administrator branch, so it can
  never be combined with, or widen, the scoped predicate (the PostgreSQL test sends `One(beta)` as alpha's administrator and gets
  alpha's single row). Names are read after the page's rows are selected, for those rows' ids only. The three filter kinds
  translate on PostgreSQL (`IS NULL`, `=`). The window is resolved once and returned (`from`, `to` as read). Entry: scope decided
  before any name or institution or speciality is read; `SummaryJson` is `null` unless Administrator (a narrowing: the old
  `AuditEntryDto` returned it to an institution's administrator); out of scope is `null` (404). The old DTO is deleted and nothing
  else referenced it (the solution builds).
- **`AuditFailureText` (P1).** Exactly `ValidationException` (its validators' messages, de-duplicated, in order, never the log form),
  `UnauthorizedAccessException`, and `InvalidOperationException` whose `TargetSite.DeclaringType.Assembly` is named `Wombat.*`; every
  other exception, a framework or EF `InvalidOperationException` included, stores "The action failed." and is logged whole with
  the entry id before the write. I checked the throw-site rule against the runtime (Release, scratch program): `int?.Value`
  (`System.ThrowHelper`, CoreLib), `First`/`Single` (`System.Linq.ThrowHelper`), a modified-collection enumerator, `Stack.Pop`
  all report a framework assembly; a Wombat `throw` in an `async` method reports the state machine's nested type, in the same
  assembly. An exception never thrown has no `TargetSite` and is not a refusal (safe default). The unit tests cover both outcomes.
  `AuditEntry.Create` still truncates to 2,000.
- **Two-constructor `AuditPipelineBehavior`.** The 2-argument constructor's parameters are a subset of the 3-argument one's, so the
  container takes the greedy one (logger injected) without ambiguity; the short one keeps the old tests compiling with a
  `NullLogger`. The refused-save path is unchanged, and the row's text is chosen once for both writes.
- **`DataRightsPeople.NamesOfAsync`.** One erasure read (by the id they had or by the pseudonym) and one directory read for the living;
  an erased person is the pseudonym; an unknown or nameless account is the id itself (so the dictionary has an entry for every
  id: no caller can `KeyNotFoundException`). `InternalsVisibleTo("Wombat.Application.Tests")` only, no wider; no other assembly
  reaches the internal class.
- **Create institution.** Administrator only, before anything is staged; short code then name checked with `AnyAsync` before the
  entity is added, so a pipeline failure row carries only the refusal; the typed `InstitutionUpdateRefusedException` is an
  `InvalidOperationException` from Wombat (stored as its sentence). The race path: only a unique violation (`PostgresErrors`,
  walking the chain) becomes the duplicate refusal, with the `DbUpdateException` as inner, so `IsRefusedSave` still discards the
  tracked insert before the audit write; any other fault is not mistaken for a duplicate. `AuditOnRefusedSavePostgresTests` is
  re-armed to make the clash between the check and the save with a real concurrent rename, so both tests still reach the unique index.
- **PostgreSQL tests can fail.** Each asserts exact values from rows it writes (counts, edges, the stored `infinity`, 22:00 UTC, the
  index names, the one-statement count); the migration test cannot pass without the migration being discovered.

## Findings

### S-1. Home still tries every part after the probe has failed (`GetAdministratorDashboardSummaryQuery.cs:82-90`)

When the probe (`:67-76`) throws, `databaseFailedAt` is set but the handler goes on to run the accounts, jobs and mail reads one after
another. Each is a full attempt against the same database. With a database that refuses at once they fail at once, but with one that
does not answer (a black-holed address, a stalled server) each waits out the connection timeout (Npgsql's default is 15 s), so
Home takes the probe's wait plus three more, about a minute, before it can say "Database: not answering", which is the one
moment Home should be quick. The design asks for each part to be caught alone (K11); it does not ask for the dependent parts to be
tried after the probe has said the database is gone.
Failure scenario: PostgreSQL's host stops dropping packets; an Administrator opens Home and sees the loading state for ~60 s.
Fix: when the probe failed, skip the three reads and leave `accounts`, `jobs` and the outcomes null (they read "not read" anyway). Only
the mail line's configuration half remains. (A part that fails on its own with a healthy probe is still caught alone.)

### S-2. `UnauthorizedAccessException` is a refusal whoever threw it (`AuditFailureText.cs:56-57`)

P1 as decided says "an `UnauthorizedAccessException`", unqualified, and the code does exactly that, so this is not a deviation. But the
framework throws the same type for file-system access, with a path in its text ("Access to the path '…' is denied."), and that text
would be stored in a failed row and shown to an institution's administrator, the case P1 exists to prevent for
`InvalidOperationException`. Today no command does file I/O (the only `File.`/`Directory.`/`Path.Combine` uses in Application and
Infrastructure are the seeders and the reference-data readers, none under a command), so there is no live exposure; it is a
latent asymmetry. Fix: apply the same `ThrownByWombat` test to `UnauthorizedAccessException` (every Wombat refusal of authority is
thrown by Wombat code), or accept and note it in P1's record.

### Nits

- **N-1. A date at the edge of `DateOnly` throws in Run history** (`GetScheduledJobRunsQueryHandler.cs:49,55`). `from=0001-01-01` makes
  `from.AddDays(-1)` throw `ArgumentOutOfRangeException`, and `to=9999-12-31` makes `EndOf` overflow. `RunHistoryAddress.Day` accepts
  both (any `yyyy-MM-dd`). The page catches it (`ScheduledJobRunsList.razor:219-226`) and shows the generic load failure with a log
  line, so no crash, but the address gets a misleading error. Clamp the two days (or compute the start as `EndOf(from) - 1 day`
  guarded by `DateOnly.MinValue`).
- **N-2. `GetScheduledJobStatusQueryHandler.cs:62`** passes `lastRun?.StartedAt` to `JobSchedule.NextAfter` as stored, while Home
  (`Judge`, `:172-174`) `SpecifyKind`s it to UTC first. Cronos throws on a non-UTC `DateTime`. Npgsql returns UTC, so production is
  safe; a test double or a provider that returns `Unspecified` would throw here and not on Home. Put the `SpecifyKind` in
  `JobSchedule.NextAfter` so the one rule is also the one guard.
- **N-3. Handlers that return system data take a principal and ignore it.** `GetAdministratorDashboardSummaryQuery(Principal)` (never read)
  and `RunScheduledJobNowCommand` now carries a `Principal` it only reads the id from; neither checks `IsAdministrator()`. The pages
  are gated (`[Authorize(Roles = "Administrator")]`, `Policy = "Administrator"`), and the same was true before T400, but the
  principal is there now, and CLAUDE.md's pattern is that the handler is the second lock. One line each.
- **N-4. The audit list does not clamp `Page` upward** (`ListAuditEntriesQueryHandler.cs:~38`): `Page = int.MaxValue` makes
  `(page - 1) * pageSize` overflow to a negative `OFFSET`, which PostgreSQL refuses. Pre-existing (Run history now clamps to the
  last page; the audit list returns an empty page for a page past the end). Clamp it the same way.
- **N-5. `AuditPipelineBehavior.cs:128` logs at Error for every non-refusal,** `OperationCanceledException` included, so a cancelled
  command (a circuit torn down mid-send) is a failed row ("The action failed.") and an Error line. Pre-existing as a row; the Error
  line is new. Log cancellation at Information, or skip the failure row for it.
- **N-6. The race path always names the short code** (`CreateInstitutionCommandHandler.cs:~80`). A concurrent create that collided on the *name*
  index is refused under Short code. Documented in the remarks; the PostgreSQL error carries the constraint name
  (`IX_Institutions_Name`), which the handler could read to place it, if the field matters.
- **N-7. `ScheduledJobRunRetentionJob` loads every expired row** (`ToListAsync` then `RemoveRange`) for both tables. With 90 days of
  outcomes this is a daily few thousand rows; `ExecuteDeleteAsync` would not load them. Pre-existing for the runs; the outcomes table
  inherits it.
- **N-8. Shutdown with the database down.** `RecordAsync` is uncancellable (`CancellationToken.None`), like `ReportAsync`; during
  `AbandonAsync` of a long queue each mail waits for a connection that will not come, bounded only by the host's shutdown timeout, and
  while the database is down a *live* mail's send is delayed by the recording's wait. It is logged and never throws, as the contract
  says; just note it as the cost of recording in the path.

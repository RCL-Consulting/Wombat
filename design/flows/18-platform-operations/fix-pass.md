# Flow 18 build: the fix pass (T400)

One Opus agent, 2026-11-12, on `t400` from `f26cd81c`, against `build-review.md` (M1, S1–S4) and every nit of
`build-review-code.md` (N-1–N-8) and `build-review-pages.md` (N1–N13). The operator decided F1: take the recommendation.

Commits: `9d072c56` (S1, N-3), `d1ea53fc` (S2, N-5), `00dc2cf8` (N-1, N-2, N-4, N-6, N-7, N-8), `1d54a6b9` (N7, N12),
`439546c5` (M1, S3), `9b393f43` (S4, N1, N2, F1's record, the NEW words).

| # | Finding | Done | Commit |
|---|---|---|---|
| M1 | A.2.9 expects the retention job's old line | Expect now "no old runs or mail outcomes to delete." (the job's sentence, `ScheduledJobRunRetentionJob.cs`); the T398 Actual keeps its quote, marked as pre-T400 words, on the same line count; the flow's own copy of the step (`18-platform-operations.md`) too. `JobRunsPostgresTests` now pins both log sentences. | `439546c5`, `9b393f43`, `00dc2cf8` |
| S1 | Home tries every part after the probe fails | With the probe failed, accounts, jobs and the mail table are not tried; they read "Not read: the database did not answer." The mail line with no server stays Not configured. The database-down test verifies the directory is never asked. | `9d072c56` |
| S2 | `UnauthorizedAccessException` kept whoever threw it (F1) | The throw-site test now applies to it: kept only when thrown from a `Wombat.` assembly; a framework one stores "The action failed." and is logged whole. Tests both ways (a Wombat throw kept; `File.ReadAllText` on a directory replaced, directly and through the pipeline; one never thrown replaced). P1 restated in `build-contracts.md` § 3 and recorded as decided in `build-lanes.md` P1 (DESIGN.md states no P1 text). | `d1ea53fc`, `9b393f43` |
| S3 | `states.md` says "The other 7" at A.2.2 | The row now reads "The other 6 last succeeded. weekly-coordinator-digest has not run yet: its first run is ‹YYYY-MM-DD› 10:00 SAST.", checked against `AdministratorHomeWords.Jobs`; a Web test renders exactly that shape. | `439546c5`, `1d54a6b9` |
| S4 | DESIGN.md has no Scheduled jobs or Run history page text | Two paragraphs before the audit log's: the schedule words, Last and Next run cells, the actions' names; the filters, the address (`?job=&status=&from=&to=&page=`, `size` when not 50), the h2 "Runs", the count line and each action's focus, the empty and no-match cards. | `9b393f43` |
| N-1 | Run history throws at the calendar's edges | `from = DateOnly.MinValue` and `to = DateOnly.MaxValue` bound nothing; tested with both and with the days next to them. | `00dc2cf8` |
| N-2 | Status handler passes an unkinded start to Cronos | `JobSchedule.NextAfter` takes an `Unspecified` moment as UTC, so every caller has the guard; tested. | `00dc2cf8` |
| N-3 | Home and Run now ignore the principal's role | Both handlers refuse anyone but the Administrator with `UnauthorizedAccessException` before reading or staging anything; tests for an institution's administrator and a coordinator. The two audit-only sentences are § 5 row 36. | `9d072c56` |
| N-4 | The audit list does not clamp `Page` upward | Clamped to the last page after the count, as Run history is; `int.MaxValue` and 5 both read page 4 of 4. | `00dc2cf8` |
| N-5 | Cancellation logged at Error | A cancelled command still stores "The action failed." but logs at Information, with no exception; tested. | `d1ea53fc` |
| N-6 | A raced name clash refused under Short code | After the unique index refuses, the handler reads again which field clashes (short code, then name, else short code; a failed read leaves short code). A PostgreSQL test races the name and gets Name; the existing short-code race now asserts ShortCode. | `00dc2cf8` |
| N-7 | Retention loads every expired row | `ExecuteDeleteAsync` on each table, the log sentences unchanged. The in-memory provider cannot run it, so the Application tests keep only the description; what is deleted (and the cutoff minute kept) is pinned on PostgreSQL. The two deletes are no longer one save; a failure between them leaves the outcomes for the next day. | `00dc2cf8` |
| N-8 | `RecordAsync` uncancellable | Still not cancelled by the host's stop, but bounded at 10 s (`EmailWorker.RecordTimeout`); a cut-off recording is logged like any other failure. | `00dc2cf8` |
| N1 | DESIGN.md says Edit has a visually hidden name | Now `aria-label="Edit ‹name›"`, as built. | `9b393f43` |
| N2 | The payload ceiling's reading | DESIGN.md says 28rem of content (`content-box`, 30rem with padding and border); the sweep's open point marked settled. | `9b393f43` |
| N3 | Disable acts at once | Fine as built (no board draws a dialog). No change. | — |
| N4 | Two command names break once | Fine as built (round 3 accepted it). No change. | — |
| N5 | Run history at 1025 with a long error | Fine as built; the reviewer's note (widen the 1024px stack, not the share, if a replay finds it unreadable) stands for the next replay. No change. | — |
| N6 | Lane W's open points | Fine as built. No change. | — |
| N7 | "All 0 jobs last succeeded." with no definitions | Home's jobs line reads "No scheduled job is stored yet: the scheduler stores them as it starts." on a Not configured badge (NEW, § 5 row 35); tested. | `1d54a6b9` |
| N8 | A page past the last | The audit log now reads the last page (N-4). Run history already did: its handler clamps to the last page (`GetScheduledJobRunsQueryHandler`, tested since lane B), so the review's Run history half does not occur. | `00dc2cf8` |
| N9 | Native `disabled` on the record's other actions in flight | Not changed: it is T234's in-flight rule, which DESIGN.md accepts; changing it would be a design change across every record page, not this flow's. | — |
| N10 | The audit subtitle appears after the first read | Not changed: the institution's name is not known before the read; accepted by the review. | — |
| N11 | `/admin/institutions/abc` is Page not found | Not changed: the route is `{Id:int}`, as every record route; only a hand-typed non-number reaches it, and A.5.5's claim is about numeric ids. | — |
| N12 | Two builders of one address, a hand-made millisecond format | Home's "Its runs" now calls `JobWords.FailedRunsOf`; `SastTime.Millisecond` is new and `AuditWords.Millisecond` uses it. | `1d54a6b9` |
| N13 | `FormFieldHelpTextLinkTests` guard lowered | Accepted by the review (the two hand-written pages are pinned by their own tests); no change. | — |
| NEW | The pages review's 26 word groups | Copied into `build-contracts.md` § 5 as rows 9–34, each with its "replaces" cell; rows 35–36 the fix pass's own. | `9b393f43` |

## Checks

- `dotnet build Wombat.sln -c Release`: 0 warnings, 0 errors.
- Suites, each by `dotnet test <csproj> -c Release` with its own build: Domain 907, Application 4560, Infrastructure 1039,
  Architecture 52, Web 4952, Integration 542 (PostgreSQL, 3 m 20 s); all passed, none skipped.
- `design/tools/check_verbatim_steps.py`: 425 verbatim steps, 0 problems. `design/tools/check_flow_completeness.py`: 332
  runbook steps, none unnamed.

## Departures

- `build-contracts.md`'s "No new role check" (across lanes, Q1) is about pages, routes and nav; N-3's handler checks
  repeat the pages' existing Administrator gate and add no role.

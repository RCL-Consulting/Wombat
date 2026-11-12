# ScheduledJobs

Scheduled jobs (`/admin/jobs`, `Pages/Admin/Jobs/ScheduledJobsList.razor`; its words `JobWords.cs`, the schedule in words `ScheduleWords.cs`; the read `GetScheduledJobStatusQuery`, `ScheduledJobDto`; the commands `RunScheduledJobNowCommand` (answering `ScheduledJobRunOutcome`), `DisableScheduledJobCommand`, `EnableScheduledJobCommand`; the dispatcher's refusal `ScheduledJobRefusal`; the host's rule `Wombat.Application/Scheduling/JobSchedule.cs`; the addresses `Components/Shared/JobLinks.cs`): **the eight jobs Wombat runs, each saying when it runs, how its last run went and when it runs next, with Run now and Disable or Enable.** Flow 18 (T400, 2026-11-12, `34b5077e`, with `4454e915`: Last run and Next run each one unit; boards R3-J01 to J10, R2-J11, J12; T1, T2, T5, T15, T16, T22, T23, T234, T239, T329, K4, K5, K7, S3, S4, S7, C1, N7) rebuilt it. Until then it showed the cron alone and times in UTC, a column headed Enabled, a run's error cut at 80 characters, the result under the table with the focus left on the button, Run now on a disabled job that ran it anyway, and a refusal's or failure's exception text.

## Who reads it

The Administrator alone (`[Authorize(Policy = "Administrator")]`; NavMenu's Scheduled jobs in the Platform group). Run history lights it (NavOwners). Home's System health card links here (AdministratorHome).

## The page

- **The header**: h1 and tab "Scheduled jobs"; "The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history."; **Run history** (outline, `clock`) the header action, which needs no data (R2-J11).
- **The column**: `<div class="people-page jobs-page">`, `aria-busy` while it reads; **the one result region (`ActionResult`) above the list** (T5).
- **The list**: a `DataTable` with `StackWide`, captioned "Scheduled jobs", no pager.
  - **Job**, the row header (S7): the key over its description in `.person-cell` ("entrustment-decision-expiry" over "Expires entrustment decisions past their expiry date and sends 30-day expiry reminders."), painted by `.people-page`'s row-header rules. Every description says what the job does, never when (T15).
  - **Schedule** in words in South African time (`ScheduleWords`, K7): "Daily at 09:00 SAST", "Hourly, on the hour", "Mondays at 10:00 SAST" (a UTC hour of 22 or 23 moves the day), over the cron as the scheduler reads it, `<code>` "cron 0 7 * * *, UTC"; any other cron that line alone (T2).
  - **Last run**: the South African day and the hour, one `span.nowrap` ("2026-11-12 05:30 SAST", `4454e915`: the replay found every Last run over two lines at 1280 beside a band), its status badge under it (Running `badge-accepted`, Succeeded `badge-completed`, Failed `badge-declined`, "Unknown" for a status that is none of the three, never an enum's name), and a failed run's first error line under that (`span.text-sm`, breaking anywhere, T1); "Not run yet" muted.
  - **Next run** by the host's own rule (`JobSchedule`, K5): the day and the hour as Last run's; "At the next check, within 30 seconds" for a job already due; "None while disabled" muted; "None: the schedule cannot be read" muted for a cron that does not parse (K4).
  - **Actions** (`.actions-cell`, an unlabelled column): **Run now** and **Disable** or **Enable**, outline and small, each named for its row (T239: "Run now: entrustment-decision-expiry", "Disable entrustment-decision-expiry").
- **Run now on a disabled job is unavailable, not disabled** (R3-J02, T22): it stays in the tab order with `aria-disabled="true"`, described by "Enable it to run it." in a `span.muted.text-sm` beside the buttons (a span, so the cell's row does not open), and a press sends nothing. A page read before someone else disabled the job still cannot run it: the dispatcher refuses a disabled job under the job's lock.
- **While a run goes** Run now reads "Running…" (`aria-disabled`), the row's other actions natively disabled, the pressed button keeping the focus (T16, T234).

## The answers (above the list, each taking the focus)

- **Done** (success): "Ran msf-campaign-auto-close: Succeeded at 2026-11-12 09:14 SAST."; "Disabled weekly-coordinator-digest. The scheduler will not run it until you enable it."; "Enabled weekly-coordinator-digest. Its next run is 2026-11-16 10:00 SAST." ("… It runs at the next check, within 30 seconds." for a job due at once, K5; "Enabled ‹key›." with no next run known).
- **A run that failed** (danger): "Ran entrustment-decision-expiry: Failed at ‹moment› SAST: ‹its error›." and **Its runs** (Run history filtered to that job's failed runs, with a visually hidden ": ‹key›, failed").
- **Refused** (warning, `role="alert"`; N7: the focus no longer stays on the button): the dispatcher's words, "Job 'audit-log-retention' is already running, so it was not started again. Wait for that run to finish, then run it again if it is still needed.", or a disabled job's, "Job 'weekly-coordinator-digest' is disabled, so it was not run. Enable it, then run it." (T22).
- **Failed** (danger, T23): "**‹key› could not be started.**" ("could not be disabled.", "could not be enabled.") then "Nothing has changed. Try again, or come back in a few minutes."; the cause to the log, never the exception's text (T329).

The page reads again after each answer, so a refused run on a job disabled meanwhile shows its row disabled (R3-J09).

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the scheduled jobs." over a skeleton card.
- **Load failed**: "Could not load the scheduled jobs. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), which focuses the h1 once it reads (S3).

## Narrow widths

**At 1150px and below the list stacks** (C1: `.jobs-page` in the seven selector lists of app.css's flow 16 shared 1150px query), a readability choice, not an overflow fix (S8): at 1151px a job's description ran over four lines beside short cells. Stacked, each job a block, its key and description first, then "Schedule:", "Last run:", "Next run:", then its actions, 44px. **While a row shows an error, Last run asks for 25% of the table** (`.jobs-page .clinic-table:has(td[data-label="Last run"] .text-sm)`, the build's sweep): a share, not a floor, so the error wraps inside it and the keys, descriptions and cron lines keep their width.

## Contrast

No new colour token. The description, cron line and muted cells `muted-text`, 5.09:1 on the surface; the badges are the five tints (Badge).

## Known gaps

- No page fault found re-syncing. The cron line is a bare `<code>` (tokens.json's Code style: app.css has no `code` rule, so it renders in the browser's monospace).

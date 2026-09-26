---
id: T327
title: The Administrator's operations pages misreport: System health is two stubs, the user count skips role-less accounts, and job-run history cuts at 200 unannounced
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T327 — The Administrator's operations pages misreport: System health is two stubs, the user count skips role-less accounts, and job-run history cuts at 200 unannounced

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The System health card cannot show a real failure. Mail and jobs read amber whether they work or not. The hazard the operator most needs to see, mail not configured (T157's note: with Email__SmtpHost unset every mail is logged, not sent), looks the same as working mail. Each line's state is colour alone, which fails WCAG 1.4.1. The user count and the run-history filter are Low: a wrong number and a filter that finds nothing.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-1.1a, F-A.6.2a, F-A.6.2c, F-A.6.2b, F-A.2.10b).

## Symptom

**In full:** The Administrator's operations pages misreport: System health is two hard-coded amber stubs with task ids and colour-only status, the user count skips accounts with no role, and job-run history matches only a whole key and stops at 200 without saying so.

- Steps 1.1 and A.6.2 (devadmin, `/`): System health lists 'Database connection' with a green dot, and 'Email queue (T012)' and 'Last nightly job (T024)' each with an amber dot. In the replay the mail sink delivered every mail and every job's last run Succeeded. The labels print internal task ids. Screenshots: design/baseline/act-1/1.1-1-devadmin-home.png, design/baseline/act-A/A.6.2-1-system-health.png.
- Each line is `<span class="status-dot ok|warn"></span>` followed by the label, with no text and no accessible name, so a screen reader hears 'Database connection', 'Email queue', 'Last nightly job' and no state (A.6.2).
- 'Users across institutions' reads '23 registered users' while AspNetUsers holds 25. Dr Molefe, a graduate who still signs in, and the erased account are not counted (A.6.2). The runbook's Expect says the card 'counts registered accounts'.
- Step A.2.10 (`/admin/jobs/runs`): the key 'weekly-coordinator-digest' lists its two runs, but 'digest' gives 'No runs found', although the box's placeholder is 'Filter by key...' (design/baseline/act-A/A.2.10-2-partial-key-nothing.png). The list shows at most the newest 200 runs, with no pager and no total. The replay's 23 runs could not show this, but runs are kept 90 days across nine jobs.

## Root cause

- `src/Wombat.Web/Components/Pages/Dashboards/AdministratorDashboard.razor:17-24`: the Email and job lines are literal markup, `<span class="status-dot warn">` plus '(T012)'/'(T024)' spans, and read no data. T012 and T024 shipped in June (PLAN.md:64-65), and nothing wired them in. `AdministratorDashboardSummaryDto` carries only `DatabaseHealthy` and `TotalUserCount`.
- The same file, lines 14, 18 and 22: the state is only the dot's colour class. `app.css:1012-1031` draws an empty 0.6rem circle, and DESIGN.md § Status dots (line 1635) presents the dot as the whole signal.
- Nothing records mail state that the dashboard could read. The queue is in-process (`EmailQueue.cs:7`, an unbounded Channel). `EmailWorker.ReportAsync` (`EmailWorker.cs:195`) reports outcomes only for mails that carry a DeliveryKey. When `Email:SmtpHost` is empty, `DependencyInjection.cs:97-106` registers `LoggingEmailSender` and no worker at all.
- `src/Wombat.Application/Features/Dashboards/Administrator/GetAdministratorDashboardSummaryQuery.cs:43-52` counts the union of `ListUsersInRoleAsync` over `WombatRoles.All`. An account with no role is never counted, and counting takes ten queries that load every user of every role.
- `GetScheduledJobRunsQueryHandler.cs:23` filters with `r.Key == request.Key`. At :45 it applies `.Take(200)` and returns no total. `ScheduledJobRunsList.razor:23` is a free-text box. No decision chose whole-key matching; A.2.10's Expect recorded what the code does.

## What to build

- **System health reads real state and names it in words.** Each line gets a label, the state as text ('OK', 'Failing', 'Not configured', …) and the dot beside it, `aria-hidden`, as a secondary cue. No task ids.
  - Database: as now, plus the word.
  - Mail: add an Application interface (for example `IEmailHealth`), implemented by an Infrastructure singleton that the mail path writes to. It holds:
    - whether mail is configured or only logged;
    - the current queue depth;
    - the time of the last mail sent and of the last mail dropped;
    - the number dropped in the last 24 hours.
  - The worker records every outcome, not only mails that carry a DeliveryKey. 'Not configured: mail is written to the log, not sent' is a warning, and a drop in the last 24 hours is a failure. The state is the Web host's queue; if the Api host sends mail too, say so on the card.
  - Scheduled jobs: read from `GetScheduledJobStatusQuery` (definitions plus each job's last run). The line reads 'All N jobs last succeeded', or names the failed, never-run and disabled jobs, and links to /admin/jobs. Consider 'overdue' too: an enabled job whose last run is older than its previous cron time, because the host was down.
  - DESIGN.md § Status dots says a dot is never the only signal (WCAG 1.4.1). The inline `style` on each `<li>` becomes a class.
- **The user count is one count query** through `IUserAdministrationService`, not ten role lists. Recommended: count every account except an erased one, and show locked accounts apart. The alternative is to keep the role union and label it 'accounts holding a role'. Which is intended is not recorded and needs the operator's confirmation. Either way, the label says what it counts.
- **Job-run history.** The key filter becomes a select of the job definitions, since the keys are a closed set of nine; if free text stays, it matches a part of a key. The list pages with DESIGN.md § Pager ('Showing 1–50 of N') instead of stopping at 200, and the query returns its total. Update A.2.10's Expect.

## Verification

- [ ] Application test with a fake clock covering the summary's mail state: it reports 'not configured' when SmtpHost is empty, OK after a delivery, and failing after a drop in the last 24 hours.
- [ ] Application test for the jobs state: 'all succeeded' when every last run succeeded, and a failed job named by its key.
- [ ] Application test that the user count includes an account with no role and leaves out an erased one (or, if the label route is chosen, that the label says 'holding a role').
- [ ] bUnit test on AdministratorDashboard: each System health line renders its state as text, the dot is aria-hidden, and no '(T0' text appears.
- [ ] Application test for GetScheduledJobRuns: the key filter returns a job's runs, and with 250 runs page 2 returns the rest along with a total of 250.
- [ ] Browser, replaying 1.1 and A.6.2 as devadmin: the database, mail and jobs lines read OK in words on the scenario database. With the mail sink stopped and a mail sent, the mail line reads failing. Replace design/baseline/act-A/A.6.2-1-system-health.png.
- [ ] Browser, replaying A.2.10: choosing the digest's key lists its two runs, and the pager's count shows. Update A.2.10's Expect and Actual.
- [ ] Browser at 390 px: the card has no horizontal scroll, and an accessibility-tree check hears each line's state.

## Related

T012 and T024 (the stubs' origin), T157 (Email__SmtpHost unset: mail logged, not sent), T251 and T283 (delivery outcomes and ScheduledJobMailTally), T277 (same class: an unannounced 500 cut on the access report), T142 and F-A.2.10a ('Triggered by' prints a user id; filed with the raw-id findings), DESIGN.md § Status dots and § Pager, runbook steps 1.1, A.6.2 and A.2.10.

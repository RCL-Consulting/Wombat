# Flow 18 round 1: assessment (T400)

Read-only, at HEAD `243ae294` (the boards are that commit). Canvas "Wombat 18"
(https://claude.ai/artifact/FE7WSRNTmSp88bvS4dZRGV), design system copied at version 1794377277-5687 (`canvas.json`,
`designSystems`). Boards: `design/flows/18-platform-operations/round-1/*.dc.html` (31: Spec, Steps, h-lines, wf-a, wf-b,
wf-c, eight `i-*`, eight `j-*`, nine `a-*`; every board's text and markup read), `canvas.json` and the round's
stylesheet `f18r1.css`. `W/` = `src/Wombat.Web/Components/Pages/`, `A/` = `src/Wombat.Application/Features/`, `I/` =
`src/Wombat.Infrastructure/`, `dash` = `W/Dashboards/AdministratorDashboard.razor`, `summary` =
`A/Dashboards/Administrator/GetAdministratorDashboardSummaryQuery.cs`, `jobs` = `W/Admin/Jobs/ScheduledJobsList.razor`,
`runs` = `W/Admin/Jobs/ScheduledJobRunsList.razor`, `host` = `I/Scheduling/ScheduledJobHost.cs`, `dispatcher` =
`I/Scheduling/ScheduledJobDispatcher.cs`, `inst` = `W/Admin/Institutions/InstitutionEdit.razor`, `audit` =
`W/Admin/Audit/AuditList.razor`, `entry` = `W/Admin/Audit/AuditDetail.razor`, `app.css` =
`src/Wombat.Web/wwwroot/app.css`. **Obs** = observed in the code, the runbook, a capture or the boards. **Inf** =
inferred. Brief sections are `design/flows/18-platform-operations.md`'s. Two captures were opened to check ids and rows
(`act-3/3.56-1`, `3.57-3`, `3.57-1`); the rest were read in the code.

**Q1–Q5 are decided** (2026-11-11, T400's Notes): Q1 Home's card holds everything, no System page or item; Q2 T327's mail
half, "Not configured: mail is written to the log, not sent." a warning; Q3 "All 8 jobs last succeeded." or the jobs
named; Q4 every account but an erased one, locked said apart, one query; Q5 the record headed by its name, Status read
out, Deactivate…/Reactivate… behind dialogs, no Back, a new list subtitle, T378. Q6–Q12 are open. The ask the canvas got
(`round-1-ask.txt:264–276`, "QUESTIONS THE DESIGN MUST ANSWER") lists all twelve as open with no answers, so the canvas
answered Q4 and Q5 against the decisions: flow 17's lesson repeated (§ 9). As in flows 16 and 17, a board that
contradicts a decision is a must-fix below unless it argues to reopen it.

## Summary

- **Pick A** (Home holds it all: a System health card of three lines in words, each with a badge, a sentence and a link,
  and an Accounts card; no System page, no nav item).
  - **Why A:** it is Q1 as decided. It costs nothing in the shell (A.5.13 holds), adds only R1 (and R2 if Q14 keeps a
    breakdown), and reaches a failed job in 2 presses at both widths (Steps, confirmed in § 6).
  - **B does not argue convincingly to reopen Q1.** Its reasoning concedes the cost: "it repeats Scheduled jobs' rows
    in a second place" and "brings back a nav item flow 01 dropped". Its one gain, "a monitoring habit can bookmark" it,
    is already true of `/`. It costs a press to read the lines (A.6.2: 1, against 0) and one more for a failed job (3,
    against 2), a route, a nav item, A.5.13's and 1.1's Expects, `NavMenuAuthorizationTests` and a DESIGN.md nav row.
    Its "Your account" link names no target (§ 6).
  - **C does not reopen anything** (no page; Q1 holds), but its "Needs you" block moves lines as their states change,
    and on dev, where mail is always Not configured, Mail is a standing first row: C's own note says people stop
    reading it. **Take from C: nothing structural.** A's "every line failing" illustration already lists each failing
    job under the one line (h-lines, "Three jobs need you:"), which is all C's per-problem rows add.
  - **Take from B: nothing.** B's mail figures table ("Mail server: not set · Waiting · Last sent · Last dropped ·
    Dropped in 24 hours") is the same DATA A's configured line prints as a sentence; Q2 decided the sentence.
- **The must-fixes** (§ 7):
  - **M1:** Q4 contradicted. The Accounts card counts 25 "accounts in all, whatever they hold", with "1 is erased" under
    it. Q4: every account but an erased one, so 24, with locked said apart. The role / no role / erased lines are not
    Q4's (Q14).
  - **M2:** Q5 contradicted twice. Reactivate "asks nothing" (Spec § 5, i-inactive, i-record-390, i-states); Q5 puts it
    behind a dialog, as Reactivate…. The list's subtitle is new words, not Q5's "The institutions that hold Wombat's
    people and records.".
  - **M3:** Q2 contradicted. The mail line reads "No mail server is configured (Email__SmtpHost is not set), so every
    mail is written to the application log and none is sent: invitations, reminders and digests reach no one." Q2's is
    "Not configured: mail is written to the log, not sent.".
  - **M4:** the Steps board. "Find their own account: 1 (the name in the top bar)" lands on `/account/profile`
    (`W/../Layout/MainLayout.razor:62`), not A.6.9's `/admin/users/{UserId}`; and 3.32, A.2.6, A.2.8, A.2.9 are marked
    as changing when their Expects quote no result words. Recounted: 14 of the 23 Expects change under A, not 18 (§ 6).
- **The DATA table misses the costly half** (S9): the dashboard query throws when the database is down, so Home's
  "Database Unreachable" state needs a handler change; Run now returns nothing, so its result's duration and failure
  need the run back; the jobs page reads no error, finish or runner for a last run; the audit rows carry an institution
  id, not a name; an erased account cannot be told from a locked one without the erasure record.
- **The 900px stack (R9) is not enough for three lists** (Inf, S8): by column minimums, Run history (~690px), the
  institutions list (~690–740px) and the audit log (~900px for Prof Mbatha, ~1,020px for the Administrator, so over at
  1280) overflow their containers above 900px. Flow 15–17's 1150px stack or breakable values, measured in round 2.
- **Screenshots: all 30 opened** (Spec § 1), and the canvas corrected the brief twice, rightly: A.2.10's runs are 10
  by hand and 11 by the scheduler (3.32, A.2.4, A.2.6, two at A.2.8, five at A.2.9: 10), and KGK's address at A.7.11 is
  `hod.paediatrics@…` (set at A.6.3, `appendix-cross-cutting.md:1260`). Both go into round 2's CONTENT.
- **Step count:** the 23 steps are right and the brief pastes each. Counts recomputed in § 6.

## 1. What was drawn

31 boards in five rows (`canvas.json` notes row0–row4).

| Row | Boards | State drawn |
|---|---|---|
| 0 · Spec, Steps | Spec, Steps | Screenshots opened (30 of 30) and where they differ from the brief; components; classes K1–K8 canvas, R1–R9 PROPOSED; DESIGN.md rules; PROPOSED; DATA; edges; accessibility; Q1–Q12 answered. Journeys per structure; the 23 Expects |
| 1 · Home | h-lines, wf-a, wf-b, wf-c | Every health line's state (as replayed; all healthy; a mail dropped; a job failed; never run; disabled; overdue; the database down; all failing) and Accounts at A.6.2 and 1.1; A, B (Home and System), C, each at 1280 and 390 with a failing case |
| 2 · Institutions | i-list, i-create, i-record, i-deactivate, i-inactive, i-states, i-list-390, i-record-390 | Two rows; created empty; saved; Deactivate… asked; inactive; one row, loading, load error, a 200-character name, the record loading and failed, Unavailable, Access denied, reactivated; 390 |
| 3 · Jobs | j-disabled, j-ran, j-illus, j-runs, j-runs-job, j-states, j-jobs-390, j-runs-390 | A.2.2, A.2.4, refused and failed; run history loaded, by job, Failed none, from a day, past 50, Running and a long error, no runs, Enable, Run now failed, Run now waiting, loading, load errors; 390 |
| 4 · Audit | a-log, a-fail, a-admin, a-entry-fail, a-entry-payload, a-entry-large, a-states, a-log-390, a-entry-390 | 3.55 and Failures only; 3.57 "Msf" across institutions; three entries; empty, paged, the way back, a submission, Unavailable, loading, load errors, an inactive institution; 390 |

- **Wireframes and fidelity as asked.** A, B and C are `.cv-wf` boxes with real words; screens 3–5 are fidelity on the
  shipped shell. Every board links `ds/wombat/tokens.css`, `components/bundle.css`, `f18r1.css` in that order (Obs).
- **f18r1.css holds no hex** (Obs). Inline `style` on boards is layout scaffolding (`margin: 0`, gaps): canvas only.
- **Gendered words:** none on a page. Two in board-only text: the bracket "the date before **his** programme started"
  (a-fail, a-states, a-log-390) and "**Her** account, her role" (a-states' edge caption) (N1).

**Not drawn in round 1** (round 2's, S10):
- **Everywhere:** 768; the 900–1150px sweep; Run history loading.
- **Home:** the database down with the Accounts card (what it reads then); C's and B's 390 failing for B.
- **Institutions:** Reactivate…'s dialog (M2); create's other refusals ("An institution with the same name or short
  code already exists.", a malformed address); create and Deactivate… at 390; a refused deactivation.
- **Jobs:** a refused Disable/Enable; A.2.1's plain state before any action (j-disabled stands in).
- **Audit:** the first arrival with no From (the 24-hour default, fixed at load, Q8); the Administrator's Failures only;
  an entry with a subject; 4.38's decision and revocation entries; Prof Mbatha's entry at 390.

## 2. Home: the three structures and the health lines

The health lines (h-lines) are shared by A, B and C. Right (Obs):
- each line a `li.health-line` in a `ul.health-list`: name, a badge with a word, a sentence, a link; no dot, no task id
  (T327; WCAG 1.4.1). The card titles are `h2.dashboard-card-title`, as `DashboardCard` renders them
  (`W/../Shared/DashboardCard.razor:121`);
- Q3's words: "All 8 jobs last succeeded."; a never-run job named with its first run ("weekly-coordinator-digest has
  not run yet: its first run is 2026-11-16 10:00 SAST", a Monday; `0 8 * * 1` UTC is 10:00 SAST); a disabled job
  named; a failed job with its error and "Its runs · Scheduled jobs";
- the database line, and "/health says Unhealthy" (`src/Wombat.Web/Program.cs:105–106`, `:192`).

**Against the code** (Obs unless marked):
- **The database-down state is a handler change, not drawn data.** `summary:31–41` catches the probe, but the ten
  `ListUsersInRoleAsync` calls after it (`:44–52`) are not caught, so with the database down the query throws and
  `DashboardFrame` replaces both cards with its load error (`dash:10`). The board's "Database Unreachable / Mail Not
  read / Scheduled jobs Not read" needs the handler to return a partial answer, and the Accounts card's state then
  drawn (S9).
- **Mail is readable with the database down.** The mail state is configuration and the in-process sender
  (`I/DependencyInjection.cs:100–109`; `I/Email/EmailQueue.cs:7`), not the database. "Mail: Not read: the database did
  not answer" is false; Mail reads as it would (S4).
- **Overdue's cause is wrong** (Inf, strong). The board: "was due at 05:00 SAST since: Wombat was not running then. It
  runs again at its next time, or now." But the host runs every enabled job whose next time after its last run has
  passed on each 30-second tick (`host:14`, `:57–95`, `:97–120`), and catches up at start (`:221–273`). So while Home is
  being read, a job more than an hour late means the scheduler did not start it (a tick erroring, `host:48–51`, or a
  run holding the job's lock, `host:80–85`), not that Wombat was down; and it will not wait for "its next time" (S5).
- **Q3's failed-job line needs the last run's error** (`ScheduledJobDto` has `LastRunAt` and `LastRunStatus` only,
  `A/Scheduling/ScheduledJobDto.cs:3–11`): DATA, cheap.
- **Q4's count needs the erasure record.** There is no erased flag: an erasure and an administrator's lock both write
  the indefinite lockout (`I/Identity/UserDeactivation.cs:3–13`). "None is locked" at A.6.2 is right only if the erased
  account is taken out first, which means reading approved erasures (data rights). DATA; "one query" becomes one query
  with a join (S9).
- **"Users across institutions" becomes "Accounts".** Allowed (Home's Administrator cards), but unmarked; 1.1's Expect
  names the card (§ 6).

**A, wireframe (wf-a).** Right: Home's frame kept ("Home", "Administrator · Semester 2, 2026", no action); health
spans two, Accounts one; health first at 390. Its reasoning has one false step: "the operator's own account its way in
from the name in the top bar (flow 12)". The top bar goes to `/account/profile` (`MainLayout.razor:62`), flow 02's page;
flow 12's own-account page is reached from Users (A.6.9) (M4).

**B, wireframe (wf-b).** Home: a verdict ("1 needs you Mail: Not configured. Database: OK. Scheduled jobs: OK.") and a
System link; System (PROPOSED): an h2 a part, each job's last and next run, Maintenance (Curriculum progress, a sentence
on /health, "Your account"). Its eight job times are right against A.2.10 (each `0 h` UTC + 2). Reject (Summary).

**C, wireframe (wf-c).** "Needs you" rows worst first, "Also checked: …" folding the rest; healthy is one sentence
("OK Wombat is healthy: the database answers, mail is sent, all 8 jobs last succeeded."), which replaces Q3's line.
Reject (Summary).

## 3. Institutions (screen 3)

Right (Obs): h1 the institution's name, tab "‹name› · Wombat", trail Home › Institutions › ‹name›, no Back (Q5);
Status its own `h2` section with Deactivate… (outline) while active; the dialog's title, body and "Deactivate" as built
(`inst:132–138`), opening on Cancel; "Institution saved." / "Institution deactivated." focused (`inst:122`, `:307`);
create's refusals under their fields, `aria-invalid`, `aria-describedby` with the refusal first, Name focused, "* marks a
field you must fill in." (T378); Edit a link named for its row (`.visually-hidden` suffix); Status a badge with its
word; Created a date (A.6.1's "creation time" changes).

**Against the code and the decisions:**
- **M2.** Reactivate asks nothing and reads "Reactivate" (no ellipsis); Q5 decided Reactivate… behind a
  `ConfirmDialog` opening on Cancel. The list subtitle, "The institutions Wombat serves. Create one, or open one to keep
  its record, deactivate it or reactivate it.", is not Q5's.
- **Q5's model is not what flow 16 built** (Obs). The brief cites "flow 16's College, speciality and sub-speciality
  records", but those keep the Active box and no Reactivate button (`W/Admin/Colleges/CollegeEdit.razor:24`,
  `CollegeWords.cs:71`). Q5 stands; the two Administrator records will differ unless the College's follow (Q16).
- **The record's subtitle**, "Short code KGK · Active · created 2026-11-10", is near flow 16's
  `RecordSubtitle` ("‹short code› · ‹status›", `CollegeWords.cs:58`) with a date added. Use flow 16's form (N2). Its
  Reactivated result is "‹College› reactivated. A new invitation can name it again." (`CollegeWords.cs:92`); the board's
  "Institution reactivated." may follow that pattern (N2).
- **Unavailable reaches flow 12's view** (S6). Today an unknown or out-of-scope id goes to `/not-found` for everyone
  (`inst:248`), and Prof Mbatha's Demo Institution id is Page not found by 1.23's Expect (`act-1-setup.md:709`, `:722`;
  `states.md` "Out of scope"). The brief forbids changing her view. Q17.
- **Unavailable's markup and words.** Drawn in `.system-panel` (Access denied's) with bracketed words "flow 14 owns
  them". DESIGN.md § Alerts: "Every Unavailable is drawn one way" (`DESIGN.md:2073`): `.detail-card .detail-card--empty`,
  no icon, the trail ending at the h1, the triplet "Institution unavailable" / "This institution does not exist, or you
  cannot open it." / Go to Institutions (S7).
- **The load error's words** are new: "The institutions could not be read. Nothing was changed. Try again in a
  moment." Every built page says "Could not load the ‹things›. Nothing has changed. Try again, or come back in a few
  minutes." (`inst:163`; `W/Admin/ActivityTypes/ActivityTypesListWords.cs:109`; six more) (S3). Same on every board
  (jobs, runs, audit, entry).
- **The field help is new and unmarked** ("Up to 200 characters.", "Up to 32 characters.", "Optional.") (N3). True
  against the brief's limits.
- **The h2 "Status" is followed by a label "Status"** (i-record, i-inactive): one is enough (N4).

## 4. Scheduled jobs and Run history (screen 4)

Right (Obs): the result above the list, focused (today after the table, `jobs:79–84`); a disabled job's Run now
`aria-disabled` with "Enable it to run it now." tied by `aria-describedby`; the schedules in words and SAST, each right
against its cron (`0 7`→09:00, `0 9`→11:00, `0 3`→05:00, `30 3`→05:30, `0 8`→10:00, `0 4`→06:00, Mondays 10:00, hourly);
times zoned; the toggle in Actions, named "Disable: ‹key›"; Run history a link; "Not run yet" for the digest at A.2.2;
the refusal word for word (`dispatcher:66–68`); run history's job select of the eight keys, Status, two SAST days, Show
and Clear, the count line focused, `PagerControls` "Showing 1–21 of 21" with `aria-disabled` ends; Ran by "Demo
Administrator" / "Scheduler"; A.2.10's 21 rows (Spec's 10 by hand / 11 by the scheduler is right; § Summary).

**Against the code:**
- **Run now's result words need the run back** (S9). `RunScheduledJobNowCommand` is `IRequest` with no answer
  (`A/Scheduling/Commands/RunScheduledJobNow/RunScheduledJobNowCommand.cs:8`); `DispatchNowAsync` returns `Task` and
  swallows a failure into the run row (`dispatcher:25`, `:54–60`). "ran and succeeded … took 68 ms" and "ran and
  failed … Its error" need the command to return the run (status, start, finish, error). Cheap, unlisted.
- **The jobs page reads no error, finish or runner** (`ScheduledJobDto.cs:3–11`; `GetScheduledJobStatusQueryHandler.cs:26–59`
  selects `StartedAt` and `Status`). j-illus's error under a failed job and "Started by the scheduler" are DATA, cheap,
  unlisted (S9).
- **"Ran by" by name is cheap, not new DATA:** `IUserAdministrationService.GetDisplayNamesAsync`
  (`A/../Common/Interfaces/IUserAdministrationService.cs:28–36`). **The descriptions' "(daily at 07:00 UTC)"** are not
  stored data: each job class's `Description`, rewritten at every start (`host:166–196`). A code change, listed as DATA
  (S9).
- **The run history total** needs a count and paging in `GetScheduledJobRunsQueryHandler` (`:43–45`, `Take(200)`); the
  job select makes `:23`'s whole-key match right as it stands. From and To as SAST days change `:31–41`.
- **The command does not refuse a disabled job** (brief § 5, "PROPOSED: refuse it"). The Spec's PROPOSED list omits it
  (S2).
- **Running's tint.** Spec § 2 maps Running to `badge-submitted` (info); `BadgeFor.JobRun` maps it to Accepted (amber,
  `W/../Shared/BadgeFor.cs:171`). Unmarked (N5). Overdue uses `badge-accepted` where `.badge-overdue` ships
  (`app.css:2820`) (N5).
- **Two link targets are filtered run histories** ("Its runs", "Run history, failed runs", h-lines and j-illus): the
  runs page's filters in the address, PROPOSED for the audit log only (S2).
- **The failed-run bracket can be filled.** A run left Running at a crash is failed at start with "Marked as failed
  during startup reconciliation — process likely crashed." (`host:198–219`): a real error to draw (N6).
- **Focus after Run now refused** moves to the alert (j-illus). Today a refusal leaves it on the button (`jobs:77–78`).
  With the result above the table the move is right; say so (N7).
- **PagerControls has a page-size select** ("Per page:", 10/20/50/100, default 20; `W/../Shared/PagerControls.razor:28–45`).
  The boards draw none and show 50 a page (N8).
- **Small words:** "Error: None" on every row (an empty cell reads better); "Running" in both Duration and Status;
  "0 runs that failed" over "No run failed"; "ExpiresOn", "ScheduledJobRun" in descriptions (T324's kin) (N9).

## 5. The audit log and an entry (screen 5)

Right (Obs): the filters, then Show and Clear filters (T328); Category by label; Result "Success"/"Failed" badges
(PROPOSED); a failed row's refusal under its badge (read today: `AuditEntryDto.ErrorMessage`,
`A/Audit/AuditEntryDto.cs:19`); the window stated and fixed (T277); the count line focused; the pager's focus on the h2
"Actions" (T264); Detail a link named for its row; the entry in `.user-page` with `h2` sections, Payload the
Administrator's alone, laid out one property a line in a focusable `role="region"`; the four failures and their SAST
times (each UTC + 2, brief § 1); Ndlovu's id, Naidoo's id, "::/48" and the payload's key order, against the captures
(Obs, `3.56-1`, `3.57-3`); 27 rows for "Msf" (19 KGK + 8 submissions); the first KGK rows match `3.57-1`'s.

**Against the code:**
- **The Administrator's table at 1280** (Inf, S8). Seven columns, When `.col-fit--wide` (nowrap from 1025px, ~185px),
  Action "TransitionActivityCommand" with no break opportunity (~200px), Category "Authentication" (~100px), Person,
  Institution, Result, Detail, 32px padding each: minimums sum to about 1,020px against the 974px container. T323 asks
  long values to wrap: let Action break (`overflow-wrap: anywhere`), or drop the Institution column (the entry and the
  filter carry it). Prof Mbatha's six columns, about 900px, overflow from about 1,210px down to the stack.
- **The Subject column and the actor's address are gone from the rows** (today `audit:73–110`), unmarked. Subject type
  stays a filter (N10).
- **The institution's name is DATA** (`AuditEntryDto.InstitutionId`, an int, `:15`; the entry prints raw ids today,
  `entry:60`, `:64`): cheap; the Institution filter is new on the query (`ListAuditEntriesQuery.cs:7–17`) (S9).
- **"MSF respondent (no account)"** on the rows and "No person: an MSF respondent answers by link, with no account" on
  the entry replace "system" (`audit:96–99`). Not every actor-less row is a respondent; say what any other reads, and
  mark both PROPOSED (S2).
- **The way back needs the resolved window.** The board keeps "the filters and page in the address"; with To empty
  ("up to when you press Show"), returning re-reads to now and the page shifts, which is T277. Carry the resolved To
  (and From's default) in the address, as the brief's Q8 says. The trail's crumb then needs that query: `PageHeader`'s
  `Trail` takes a custom crumb (`W/Admin/Institutions/SpecialityEdit.razor:28`), not NavOwners' fixed href (S2).
- **Unavailable** is `.system-panel` with bracketed words; the brief's Q9 gives them: "Audit entry unavailable" / "This
  entry does not exist, or you cannot open it." / Go to Audit log (S7). Today's empty card: `entry:20–21`.
- **The Subject bracket can be filled.** Neither capture shows a Subject card, and `entry` draws it only with a subject
  type: Ndlovu's create and Naidoo's transition store none. Draw "No subject" (N6). Naidoo's first name is David
  (`README.md:142`) (N6).
- **The payload region** is named by `aria-label="Payload"` under an h2 "Payload": use `aria-labelledby` (N11).
- **The audit pager** stays 50 a page; say whether it becomes `PagerControls` with its select (`audit:121–122` explains
  why it is not today) (N8).
- **The Detail name drops the date** ("Detail: Login at 20:24:10"; today "… at 2026-11-10 20:24:10", `audit:65–70`); a
  window over two days repeats times (N12).

## 6. The Spec and Steps boards

- **Spec § 1** first, as asked, and right (§ Summary).
- **Spec § 2–3, classes.** R1 (`.health-*`) and R8 are new and justified. Four duplicate shipped classes:
  - R3 `.list-count`: flow 17's count line is a plain `<p role="status" tabindex="-1">`
    (`W/Admin/ActivityTypes/ActivityTypesList.razor:103`); `.list-window` is `.muted .text-sm`;
  - R4 `.action-reason`: flow 17's `.move-reasons--beside` (`app.css:3274–3277`); `.job-desc` is today's
    `text-muted text-sm` (`jobs:36`); `.nw` is an unscoped global, against PART 2's own "each scoped to its page";
  - R6 `.payload-block`: a modifier of `.code-block` (`app.css:3333`), which already wraps (`pre-wrap`, `anywhere`);
  - R7 `.audit-entry-page .detail-list dd`: `.user-page .detail-list dd` already wraps (`app.css:6630–6633`);
  - R9's 44px rows: `.clinic-table--stack-wide .btn` is 44px from 900px on every page (`DESIGN.md:1137–1139`).
  The boards write `clinic-table--stack-wide` alone; `StackWide` writes both classes (`DESIGN.md:1136`) (N13).
- **Spec § 4.** "§ Table system (stacking): … StackWide (≤900 px)": see S8. "§ Page-level patterns · Unavailable":
  right in intent, wrong markup (S7).
- **Spec § 5 PROPOSED, omitted:** the Accounts card's title; Unavailable for an institution (S6); the respondent's
  words; the command's refusal of a disabled job; Running's tint; run history's filters in the address; the partial
  Home when the database is down (S2).
- **Spec § 9** answers Q4 and Q5 against the decisions (M1, M2). Its Q6–Q12 answers follow the brief's recommendations,
  with these departures for the operator: "Job run history" renamed "Run history", "Triggered by" "Ran by", Finished
  folded into Started + Duration (Q6); the entry leading with SAST, the stored UTC under it (Q7, which recommended UTC as
  the record); a Person filter (DATA) and an Institution column and filter (PROPOSED) that Q8 left out unless wanted;
  sections Event, Person, Subject, Payload (Q9 recommended What happened, Who, What it was done to, Payload); StackWide
  at 900 (Q11 allowed the 1150 stack).

**Steps § 1, recomputed.** A press is a click on a link or button; reading and scrolling are 0.

| Journey | Today | A 1280 / 390 | B 1280 / 390 | C 1280 / 390 |
|---|---|---|---|---|
| 1.1 read Home | 0 / 0 | 0 / 0 | 0 / 0 | 0 / 0 |
| A.6.2 read health and accounts | 0 / 0 (nothing true to read) | 0 / 0 | 1 / 1 | 0 / 0 |
| A failing mail line found | never (nothing says it) | 0 / 0 | 0 verdict, 1 line | 0 / 0 |
| A failed job found and run again | 2 / 3 (nav, Run now; Menu first at 390), found only by reading the badge | 2 / 2 | 3 / 3 | 2 / 2 |
| Curriculum progress | 2 / 3 | 2 / 3 | 2 / 3 | 2 / 3 |
| Their own account (A.6.9's page) | 3 + 1 / 4 + 1 (Users, "devadmin", Show, the row) | 3 + 1 / 4 + 1 | 3 + 1, or 2 if System's "Your account" is that page | 3 + 1 / 4 + 1 |

- The board's own-account row (1 / 2) counts the top bar's link, which lands on `/account/profile`, not A.6.9's page
  (M4). Every other row of the board stands.

**Steps § 2, the 23 Expects, read literally.**
- **Change under every structure (14):** 1.1 (the card's name; B also the menu); 1.6 ("headed 'Edit institution'", the
  tab, "Active is ticked"); 1.11 ("count of registered users"); 3.55 (From (UTC), "apply", "OK", "FAILED"); 3.56 (the
  cards, "display name", "Back to log"); 3.57 (From (UTC), "actor is 'system'"); 4.38 ("five OK rows", "the Event card
  and the Actor card"); A.2.1 ("cron schedule (UTC)", "the server's local time", "reads '—' for its last run and status"
  → "Not run yet"); A.2.2 (the result's words, "'—' for its next run"); A.2.4 ("dispatched."); A.2.10 (the "digest"
  sub-step, Filter → Show, "finish" (folded), "who triggered it", "scheduler", "No runs found", "a UTC day"); A.6.1
  ("creation time", "the Status box"); A.6.2 (the stubs, "registered accounts"); A.7.11 ("scrolls inside its container").
- **Hold (9):** 3.32 ("The job's last run updates."); A.2.6 and A.2.8 (log only); A.2.9 ("Each run reads Succeeded");
  6.38; A.5.2; A.5.13 (under A and C); A.6.9; A.6.10. The board marks 3.32, A.2.6, A.2.8 and A.2.9 as changing (M4). The
  brief's § 7 ("3.32 and A.2.2–A.2.9") over-lists the same way.
- **Missed by the board:** A.2.1's "—" → "Not run yet"; A.2.10's "finish"; 3.57's "system" on the rows; 4.38's "OK".
- **`states.md` rows that change:** 141 ("System health (with the job status), Users across institutions"); 888 ("An
  administrator's edit"); 899 (Page not found → Unavailable, if Q17 says so); 1086 ("No entries"); 1091 ("Audit entry
  not found"); 1095 ("Dispatched"); 1099 ("the full key" → the select); 1100 ("No runs" → "No run failed"). 1093 still
  reads "Nine jobs" (eight since T379): stale today (N14).
- **Searched:** "Apply filters", "Back to log", "Back to jobs", "Edit institution", "registered users", "dispatched",
  "When (UTC)", "Display name" (the brief § 7's list): each is in a step above or `states.md`.

## 7. Corrections

Severity: **M** must (round 2 cannot go out without it), **S** should, **N** nit.

### Wrong against the code, the runbook or a decision

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| M1 | h-lines (Accounts ×2), wf-a, wf-b, wf-c; Spec § 6, § 9 Q4 | **Q4 is contradicted.** "25 accounts in all, whatever they hold" counts the erased account. Draw "24 accounts" at A.6.2 with locked said apart ("None locked"), and "8 accounts" at 1.1 (9 with the bootstrap Administrator). The "22 hold a role · 2 hold no role · 1 is erased" lines are not Q4's: drop them unless Q14 keeps them. R2 goes with them. | M |
| M2 | i-inactive, i-record-390, i-states; i-list, i-list-390, i-states (subtitle); Spec § 5 | **Q5 is contradicted.** (a) Reactivate… asks first: a `ConfirmDialog` opening on Cancel, title "Reactivate ‹name›?", a body saying a new invitation can name it again, confirm "Reactivate" (PROPOSED words), its result focused; draw the dialog. The trigger reads "Reactivate…". (b) The list's subtitle is Q5's: "The institutions that hold Wombat's people and records." | M |
| M3 | h-lines (five frames), wf-a, wf-b, wf-c | **Q2 is contradicted.** The mail line's sentence is Q2's: "Not configured: mail is written to the log, not sent." (the badge "Not configured", then "Mail is written to the log, not sent."). The setting may follow as a second line, PROPOSED: "To send mail, set Email__SmtpHost on the server and restart Wombat." Drop "invitations, reminders and digests reach no one" or mark it. | M |
| M4 | Steps § 1, § 2; wf-a reasoning | **The Steps board.** (a) Their own account is A.6.9's page, reached from Users: 3 + 1 at 1280, 4 + 1 at 390, the same for A and C; the top bar's name goes to `/account/profile` (`MainLayout.razor:62`). (b) 3.32, A.2.6, A.2.8 and A.2.9 hold; 14 of 23 change under A and C, 15 under B. (c) Add A.2.1's "—", A.2.10's "finish", 3.57's "system", 4.38's "OK", and the `states.md` rows (§ 6). | M |
| S1 | wf-b, wf-c | B and C are not taken (Summary). Nothing to correct on them for round 2; drop their boards. | S |
| S2 | Spec § 5 | **PROPOSED, unlisted:** the Accounts card's title; Unavailable for an institution (S6); the respondent's words on rows and entry, and what any other actor-less row reads; the command refusing Run now on a disabled job (brief § 5); Running's tint; run history's filters in the address (h-lines' "Its runs", "Run history, failed runs"); the audit address carrying the resolved window, and the trail's crumb carrying it; Home's partial read when the database is down. | S |
| S3 | i-states, j-states, a-states (five load errors) | **The house load-error sentence:** "Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes." (likewise "the scheduled jobs", "the run history", "the audit log", "the audit entry", "the institution"); `inst:163` and six pages' words. | S |
| S4 | h-lines (the database down) | **Mail is read without the database** (configuration and the in-process sender): draw its real state, not "Not read". Draw the Accounts card in that state too. | S |
| S5 | h-lines (overdue, all failing) | **Overdue's words.** The host catches a missed run up within 30 s of a start and on every tick (`host:14`, `:57–120`, `:221–273`), so a job over an hour late while Home is read means the scheduler did not start it. "‹key› has not run since it was due at ‹time› SAST: the scheduler did not start it. The application log says why; Run now runs it." (PROPOSED). Drop "Wombat was not running then" and "It runs again at its next time". | S |
| S6 | i-states (Unavailable) | **Say whom Unavailable covers.** Today an unknown or out-of-scope institution is Page not found for both roles (`inst:248`), and 1.23's Expect says so for Prof Mbatha. Q17. | S |
| S7 | i-states, a-states | **Unavailable as DESIGN.md draws it** (`DESIGN.md:2034–2036`, `:2073–2078`): `.detail-card .detail-card--empty`, no icon, the trail ending at the h1; "Institution unavailable" / "This institution does not exist, or you cannot open it." / Go to Institutions; "Audit entry unavailable" / "This audit entry does not exist, or you cannot open it." / Go to Audit log. Not `.system-panel`, not brackets. | S |
| S8 | j-runs, i-list, a-log, a-admin; Spec R9, § 4 | **Widths** (Inf, by column minimums: 16px text, 32px padding a column, containers 974px at 1280 and 717px at 1025). Run history ~690px and the institutions list ~690–740px overflow between 901 and about 1,000–1,045px; the audit log ~900px (Prof Mbatha) and ~1,020px (the Administrator: over at 1280). Join the 1150px stack (DESIGN.md:1147–1157) or make the long values break (Action, the address, the key), and sweep 900–1100px, then read every column at 1280 for both roles (BRIEF § 11, flows 15–17). R9's 44px rule is shipped (`DESIGN.md:1137`): drop it. | S |
| S9 | Spec § 6 | **DATA, unlisted:** the dashboard query's partial answer when the database is down (`summary:44–52` throws); the erasure record for Q4 (`UserDeactivation.cs:3–13`); `RunScheduledJobNowCommand` returning the run (`RunScheduledJobNowCommand.cs:8`, `dispatcher:25`); `ScheduledJobDto` with the last run's error, finish and runner (`ScheduledJobDto.cs:3–11`); the run-history total and paging (`GetScheduledJobRunsQueryHandler.cs:43–45`); the institution's name per audit row and on the entry (`AuditEntryDto.cs:15`). **Mislabelled:** "Ran by" is an existing read (`GetDisplayNamesAsync`); the descriptions are code, rewritten at start (`host:166–196`). | S |
| S10 | (not drawn) | **The states § 1 lists** under "Not drawn": 768; Reactivate…'s dialog; create's duplicate and malformed refusals; the audit log's first arrival on the 24-hour default; the Administrator's Failures only; an entry with a subject; 4.38's entries; a refused Disable; run history loading; Home with the database down, whole. | S |
| S11 | i-list-390 (A.7.11) | **KGK's address at A.7.11 is `hod.paediatrics@kgk.wombat.local`** (A.6.3, `appendix-cross-cutting.md:1260`), as the Spec itself found. Draw it on the 390 board. | S |
| N1 | a-fail, a-states, a-log-390 brackets; a-states caption | "his programme", "Her account": they. | N |
| N2 | i-record, i-inactive, i-states | The record's subtitle as flow 16's: "KGK · Active" (`CollegeWords.cs:58`); the reactivated result as flow 16's: "Kgosi Kgari Teaching Hospital reactivated. A new invitation can name it again." (`CollegeWords.cs:92`). | N |
| N3 | i-create, i-record | "Up to 200 characters.", "Up to 32 characters.", "Optional." are new: mark PROPOSED. | N |
| N4 | i-record, i-inactive, i-record-390 | The h2 "Status" over a label "Status": one. | N |
| N5 | Spec § 2; j-states, j-illus | Running is amber today (`BadgeFor.cs:171`): keep it or mark the change; Overdue on `.badge-overdue` (`app.css:2820`), not `badge-accepted`. "Five tints" lists six classes. | N |
| N6 | j-illus, j-states; a-entry-*, a-states | Fill the brackets the code holds: the crash error "Marked as failed during startup reconciliation — process likely crashed." (`host:212`); "No subject" for Ndlovu's and Naidoo's entries; Dr Naidoo's first name, David (`README.md:142`); the refusals of the three bracketed failures (their steps' Actuals in act 3). | N |
| N7 | j-illus | Run now refused moves the focus to the alert; today it stays on the button (`jobs:77–78`). Right here; say so. | N |
| N8 | j-runs, j-states, a-log | `PagerControls` carries a page-size select (`PagerControls.razor:28–45`); draw it, or say the audit log keeps its fixed 50. | N |
| N9 | j-runs, j-states, j-* | "Error: None" on every row; "Running" twice in one row; "0 runs that failed" over "No run failed"; "ExpiresOn", "ScheduledJobRun" in descriptions. | N |
| N10 | a-log, a-fail, a-admin | The rows drop Subject and the address (`audit:73–110`); say so. | N |
| N11 | a-entry-payload, a-entry-large, a-states | The payload region: `aria-labelledby` its h2, not `aria-label`; its `<details>` summary in run history is 24px at 390: 44 (`.run-error summary`). | N |
| N12 | a-log, a-fail, a-admin | Detail's name keeps the date, as today (`audit:65–70`). | N |
| N13 | every list board; f18r1.css R3, R4, R6, R7 | `clinic-table--stack` with `--stack-wide`; drop the four duplicate classes (§ 6); scope `.nw`. Table captions as names ("Scheduled jobs", "Institutions"), not counts ("8 jobs"). | N |
| N14 | (runbook) | `states.md:1093` "Nine jobs": eight since T379. For the runbook lane. | N |

### Invented people or values

- **No invented person** (Obs). devadmin, Prof Mbatha, Mr Smit, Drs Ndlovu, Dlamini, Khumalo, Naidoo, Molefe and the
  institutions are the ask's.
- **Ids from the captures, not invented:** Ndlovu's `c22b8779-798b-4229-a0b8-b73a3d16999c` and Naidoo's
  `535c41ed-ffb9-449f-bab9-709da9a0ed77` (`3.56-1`, `3.57-3`); "::/48"; the payload's key order (jsonb's).
- **Times and durations** match the brief and the captures (each audit time UTC + 2; A.2.10's 21 runs, 7–102 ms).
- **Bracketed where nothing was given**, rightly; N6 fills what the code holds.

### PROPOSED items, with a recommendation each

| Item | Recommend |
|---|---|
| Health lines in words with badges; no dots | Accept (T327), with Q2's sentence (M3) and S5's overdue words |
| "Accounts" for "Users across institutions" | Accept; mark it (S2) |
| B's System page, route and nav item | Drop (Q1) |
| The record headed by its name; Status its own section; no Active box | Accept (Q5) |
| Reactivate asking nothing | Drop: Reactivate… asks (M2) |
| Create's subtitle "A new institution is active from the moment it is saved, so its people can be invited." | Accept |
| Created as a date | Accept |
| Result words: disabled, enabled (with the next run), ran and succeeded (with start and duration), ran and failed | Accept (Q10 open), with S9's command change |
| "Running…" while Run now waits | Accept |
| Schedule in words and SAST, no raw cron | Accept (Q7 open); keep the cron as a monospace detail, as the brief's Q7 recommends, for the operator who reads cron |
| "Run history" for "Job run history"; "Ran by"; Finished folded | Accept; A.2.10's Expect changes (§ 6) |
| Audit: Person filter (DATA) | Q8 is the operator's; recommend leaving it out until T286's name source lands (Q15) |
| Audit: Institution column and filter (PROPOSED) | The filter yes; the column no (S8) |
| Categories by label; Success / Failed; a failed row's refusal under its badge | Accept |
| The window fixed and stated; filters and page in the address | Accept, with the resolved To in the address (S2) |
| The entry: h2 sections, SAST with the stored UTC under it | Accept (Q7 open) |
| "MSF respondent (no account)" / "No person: …" | Accept for the submission; say what any other actor-less row reads (S2) |

### DATA items, with a recommendation each

| Page | Item | Recommend |
|---|---|---|
| Home | Mail configured, queue, last sent, last dropped, drops in 24 hours | Accept (Q2 decided; T327's `IEmailHealth`) |
| Home | Overdue from the cron | Accept; computed, no store |
| Home | The account count, erased taken out, locked apart | Accept (Q4); one query joining approved erasures (S9) |
| Home | The role / no-role breakdown | Drop unless Q14 keeps it |
| Home | A partial answer with the database down | Accept (unlisted, S9) |
| Home, Scheduled jobs | The last run's error, finish and runner | Accept (cheap, unlisted) |
| Scheduled jobs | Run now returning its run | Accept (cheap, unlisted) |
| Scheduled jobs | Descriptions without "(daily at …UTC)" | Not DATA: each job's `Description` (code) |
| Run history | Ran by a name | Cheap: `GetDisplayNamesAsync` |
| Run history | The total, paging, SAST days | Accept |
| Audit | The person by name on every row (T286) | Accept, read live by user id (Q15) |
| Audit | The institution's name | Accept (cheap: an id today) |
| Audit | Person and Institution filters | Institution yes; Person after T286 (Q15) |
| Runs | What a run did (T274) | Left room for; not now (Q12) |

### Access control

| # | Board(s) | Finding | Sev |
|---|---|---|---|
| A1 | i-states | Access denied for Prof Mbatha on `/admin/institutions` and `/admin/jobs`, flow 01's words, nothing lit: holds (A.5.2). | — |
| A2 | a-entry-fail, a-states | No Payload for Prof Mbatha; an out-of-scope entry reads as one that does not exist: holds (`entry:93`; `ListAuditEntriesQueryHandler.cs:26–42`). | — |
| A3 | a-admin | The Institution filter is the Administrator's alone; an InstitutionalAdmin's query is pinned to her institution (`ListAuditEntriesQueryHandler.cs:41`) and must ignore it. | — |
| A4 | j-disabled | Run now on a disabled job is refused by the page only; the command must refuse it too (brief § 5), or the page offers less than the command accepts. | S2 |
| A5 | i-states | Unavailable for an unknown and an unreadable institution alike, never "forbidden" (S6, Q17). | S6 |

### Accessibility and the 390 boards

- **Good (Obs):** one h1 a page and h2 below it (the entry's and the record's sections; Home's card titles); tables
  captioned; every badge a word; row actions named ("Run now: ‹key›", "Edit ‹name›", "Detail: ‹action› at ‹time›"); Run
  now `aria-disabled` with its reason tied; results focused (`.action-result tabindex="-1"`); the count line focused
  after Show; the pager's focus on the h2; the dialog opening on Cancel; create's refusals tied and focused; Previous
  and Next `aria-disabled` at the ends; the filters a `role="search"` form named; every list stacked at 390 with 44px
  actions; the payload region focusable and scrolling on its own.
- **To fix:** N11 (the payload's name, the error summary's 44px); S8 (tables between 901 and 1150, and the
  Administrator's audit table at 1280); N13 (captions as names).
- **Contrast:** badges are body text on tints (T322; `DESIGN.md:309–310`): no new pair. Not measured here: R1's text on
  the card at 390.
- **Times:** every time a person reads is SAST with the zone (Q7's recommendation); the entry adds the stored UTC.

### DESIGN.md and flows 01–17's patterns

- **§ Status dots** (`DESIGN.md:3322–3333`) is retired: Home was its only user (`dash:16–22`).
- **§ Dashboard page:** the Administrator's cards (health span 2 in words, Accounts).
- **§ Table system:** the four lists join the stack their columns need (S8), not 900px by default.
- **§ Pager:** run history on `PagerControls`; the audit pager's focus (T264).
- **§ Page-level patterns:** Unavailable as drawn one way (S7); no Back links (flows 12–17); the entry a `.user-page` of
  h2 sections (flow 12); the record's Status section (flow 13's end section).
- **§ Button system:** Deactivate… and Reactivate… each an outline trigger behind a `ConfirmDialog` (M2).
- **BRIEF § 11's lessons, checked:** no gendered word on a page (board text: N1); no shipped class duplicated as NEW:
  four are (§ 6); step coverage (M4); 1280 per role and the 900–1100 sweep (S8); the decided questions sent as open
  (§ 9).

## 8. New questions for the operator

Q1–Q5 are decided; Q6–Q12 stay as the brief asks them. These five are new; each has a recommendation.

13. **Q13. What makes the mail line fail.** The brief's Q2 says "a drop in 24 hours as a failure"; T400's note of the
    decision says "nothing sent in 24 hours a failure". The second would fail a quiet weekend with nothing to send.
    *Recommend:* a drop (a mail given up after its three tries) in the last 24 hours is Failing; no mail sent is not a
    state. The board draws the first.
14. **Q14. The Accounts card's breakdown.** Q4 counts every account but an erased one, locked said apart: "24 accounts"
    / "None locked" at A.6.2. The canvas adds "22 hold a role · 2 hold no role · 1 is erased". The no-role line is the
    graduates (Dr Molefe, Dr du Plessis), which an operator may want; it costs one more query.
    *Recommend:* Q4 as decided, one figure and the locked line; no breakdown. Users already finds the graduates.
15. **Q15. Where the audit's names come from (T286), given erasure.** The stored display is free text that an erasure
    does not touch (`I/Identity/ExternalLoginHandler.cs:637`: "the audit log outlives an erasure, which pseudonymises the
    account but not free text"). Writing a person's name into each row would keep an erased person's name in the log.
    The Spec's own edge case asks this (§ 7).
    *Recommend:* read the name live from the user id at display (`GetDisplayNamesAsync`), so an erased person reads by
    their pseudonym; the stored display only where a row has no user id. No migration. The Person filter (Q8) then
    matches the live name, so it waits for this.
16. **Q16. The College's records after Q5.** Q5 took "flow 16's College, speciality and sub-speciality records" as its
    model, but those keep the Active box and no Reactivate button (`CollegeEdit.razor:24`, `CollegeWords.cs:71`). Built
    as decided, the Administrator meets two patterns for the same act.
    *Recommend:* keep Q5 for institutions, and propose a task for the College, speciality and sub-speciality records to
    follow it (Reactivate… asking first, no box), outside this flow.
17. **Q17. Unavailable for an institution, for whom.** Flow 14's rule makes an unknown and an unreadable record one
    page. Today both are Page not found (`inst:248`); for Prof Mbatha that is flow 12's built view and 1.23's Expect,
    which the brief says flow 18 may not change.
    *Recommend:* one rule for both roles, Unavailable ("Institution unavailable"), and 1.23's Expect and `states.md`'s
    "Out of scope" row change with it. One page saying "does not exist, or you cannot open it" is what flow 14 settled;
    leaving her on Page not found keeps two answers for one route.

## 9. What round 2's ask must carry forward

- **The pick:** A, with M1–M4, S2–S11 and the nits in one pass. Draw every state the brief lists and § 1's "Not drawn",
  at 1280, 768 and 390, and the 900–1150px range for the four lists.
- **The decisions, stated as decided, in the ask's first lines and in QUESTIONS:** Q1–Q5 with their words (Q2's
  sentence, Q4's count, Q5's subtitle and Reactivate…), and the operator's answers to Q6–Q17. Round 1's ask listed
  Q1–Q5 as open (`round-1-ask.txt:264–270`) though they were decided before the send, and the canvas answered Q4 and Q5
  against them: flow 17's lesson, repeated. A candidate for BRIEF § 11: the ask is regenerated after the decisions,
  not before.
- **Correct the CONTENT:** A.2.10's 10 runs by hand and 11 by the scheduler; KGK's address at A.7.11
  (`hod.paediatrics@…`); A.6.2's count under Q4 (24, none locked); the overdue cause (S5).
- **Words round 1 settles, for round 2 to reuse:** Q2's mail sentence; Q3's "All 8 jobs last succeeded." and the
  never-run line; the house load-error sentence (S3); the Unavailable triplets (S7); the institution's dialog body
  (`inst:135`); "Institution saved." / "Institution deactivated."; the refusal "Job '‹key›' is already running, …"; the
  schedules in words.
- **The stylesheet:** `f18r2.css` as a project file; drop R2 (unless Q14), R3's `.list-count`, R4's `.action-reason` and
  `.job-desc`, R6 as a `.code-block` modifier, R7, R9; scope `.nw`.
- **The steps:** recount A's journeys with A.6.9's real page; list the 14 Expects and the `states.md` rows (§ 6).
- **Checked here, so the ask need not re-derive it:**
  - the dashboard query throws on the role lists when the database is down; mail state needs no database;
  - the host catches up missed runs on every 30-second tick, so overdue means the scheduler did not start a job;
  - Run now's command returns nothing and a failed run does not throw; the jobs query reads only a last run's start and
    status;
  - an erasure and an administrator's lock write the same lockout;
  - "Ran by" names come from an existing read; the jobs' descriptions are code;
  - the audit rows carry the institution as an id; Detail and Edit are links;
  - the College records keep the Active box.

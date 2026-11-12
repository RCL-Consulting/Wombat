# T400 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/17-activity-type-builder/replay-brief.md` (T398), with flow 17's lessons (BRIEF § 11: the app
started detached, no hand-back before the act's dump, the earlier flow's password file never read).
Flow 18 runs the same prompts as sequential agents, one per act (1, 2, 3, 4, 5, 6, A), then two for the states. The text
below is what each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in, and the integrator's header line
`D = 2026-11-12` (the replay's date, a Thursday) above it. What flow 18 changed, step by step, with each act's own browser
checks, and the states to re-take follow the text; each act's prompt carries its own part of the first. The filled
prompts are in `<G>` (below) as `act-1.txt` … `act-A.txt`, `states-1.txt` and `states-2.txt`, written by
`<G>\make_prompts.py` from this file.

```text
You are playing part of T400 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It is
played against flow 18 of the restructure (the operator keeps the platform running: the Administrator's Home with System
health's three lines, Database, Mail and Scheduled jobs, each its state as a word on a badge with its sentences, no dot
and no task id, Mail "Not configured" on a host with no mail server, the jobs judged by the scheduler's own rule, and
Accounts counting every account but an erased one, with "None is locked."; the institutions' list subtitled "The
institutions that hold Wombat's people and records.", Edit named for its row, Created a South African day; Create
institution with the mark line, each refusal under its field and the first refused field focused, landing on the record
by "‹name› created." with the token gone; the record headed and tabbed by its name, "KGK · Active" under it, no Back and
no Active box, Save never asking, a Status section of its own with Deactivate… or Reactivate… behind a dialog opening on
Cancel; Institution unavailable for an unknown or out-of-scope id, for both roles; Scheduled jobs with each schedule in
words in SAST over its cron, Last run with its badge or "Not run yet", Next run by the host's rule ("None while
disabled", "At the next check, within 30 seconds"), the result above the list and focused ("Ran ‹key›: Succeeded at …
SAST.", "Disabled ‹key›. …", "Enabled ‹key›. …"), Run now unavailable on a disabled job (aria-disabled, "Enable it to
run it.") and refused by the command too; Run history with a Job select, Status, From and To as South African days, Show
writing the address and focusing the count line, Ran by a name or "Scheduler", the pager at 50, no Back; the audit log
with Category by label, From and To in SAST, Institution for the Administrator alone, Show focusing the count line, a
window fixed, said and carried in the address, each person by name, Failed with its stored refusal under the badge; an
entry in h2 sections (What happened, Who, What it was done to, and the Administrator's Payload laid out one property a
line in its own scrolling region), the trail back to the same rows with the entry's Detail focused, Audit entry
unavailable; a failed audit row storing a refusal's words or "The action failed."; mail outcomes kept in a new table
that the retention job empties after 90 days; Institutions, Scheduled jobs and the audit log stacking to 1150 px, Run
history to 1024 px), which landed on master as 34b5077e with its fix pass (one migration, T400_EmailOutcomes). Flows
01-17 and T312 landed earlier (b347e11c, f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1,
49e7cf5e, 68327c9d, 69a17c70, e53d4ae7, c9d6fe13, 5cc10c66, e8eea1d2, c931d56f, de4cc718 with its column fixes a2878763
to 2c93b247, 84bcffeb with its replay fixes c9742a18 and 277e4a78).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub
  remote; the integrator commits and pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t400, published from master at e6f928c2 or later. The
  integrator started it with tools\scenario-replay.ps1.
- It runs in Development with no mail server. Mail is written to the log
  C:\dev\Wombat\.scenario-app\wombat_scenario_t400.log as "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it DETACHED, so it does not die with your shell (BRIEF § 11, flow
  17):
  Start-Process powershell -ArgumentList '-File','tools\scenario-replay.ps1','start','wombat_scenario_t400','5180' -WindowStyle Hidden
  then poll http://localhost:5180/account/login for 200. Start it once: two instances lock .scenario-app\bin. To stop
  one, read the command line of the process listening on the port and stop that process by its id; if the tool's stop
  prints "Nothing listens on 5180" (or 5181) while the app answers, do the same (BRIEF § 11, flow 14; T391).
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database, nor
  wombat_scenario_t400_sweep (the build sweep's, kept) or wombat_scenario_t398 (flow 17's end state, kept).
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t400_s515, start it detached on port
  5181, play the step there, then stop 5181; never change wombat_scenario_t400 for that step.

Read first:
- execution/knowledge/scenario-paediatrics/README.md: How to play, The step format, and the Screenshots rule.
- Then your act file, in full.

Browser:
- The Playwright library is installed at <PW>\node_modules (v1.56.1). Its helpers are in <PW>\lib.mjs: launch, login,
  logout and waitBlazor; BASE is http://localhost:5180 unless WB_BASE is set.
- Write your scripts under <G>\<DIR>\ and import by absolute file URL, e.g.
  import { launch, login, waitBlazor } from '<PWURL>/lib.mjs';
  import { chromium } from '<PWURL>/node_modules/playwright/index.mjs';
- Launch Chrome with channel 'chrome', headless.
- Signed-in pages are Blazor Interactive Server: wait for the circuit before acting (waitBlazor).
- The sign-in, sign-out, link, forgotten-password and change-password pages are static (form posts).
- The default viewport is 1280x800; a step that says 390 px uses 390x844, 768 px 768x844.

Accounts and passwords:
- Dev accounts (devadmin@wombat.local and the others): their dev-only passwords are constants in
  src/Wombat.Infrastructure/Identity/DevUserSeeder.cs. Read them in your script; never print them.
- Cast accounts registered during this replay:
  - Choose a strong password at replay time: 12+ characters, 4+ different characters, a digit, an upper-case and a
    lower-case letter, and a symbol.
  - Keep it ONLY in <G>\replay-pw.env as KEY=value lines (WB_PW_<SURNAME>=...), which later acts read into their
    scripts' environment. This is a new file for this replay; never read or copy any other replay's password file
    (g398's, g394's or older): the scripts you copy from sit beside one.
  - A step that changes a cast member's password loses no value: it first copies the old one to
    WB_PW_<SURNAME>_<ACT> (the act that changed it), then sets WB_PW_<SURNAME> to the new one, so WB_PW_<SURNAME> is
    always current and the earlier snapshots can still be signed in to (BRIEF § 11, flow 03). A state capture on a
    snapshot taken before the change uses the _<ACT> key.
  - Never print a password to the console or into your reply, and never write one into the runbook or any tracked file.
  - Never open pwd_DO_NOT_COMMIT.txt or recovery\.
- Registration and MSF links come from the log's Stub email lines. Never write a token into the runbook.

The clock: D is the replay's date, given in this prompt's header (D = 2026-11-12 unless the integrator changed it).
Record it in your Actual lines and compute every date from it. J is the latest 15 January on or before D. Dates in steps
are relative. Every time the flow 18 pages print is South African (SAST, UTC+2; the machine clock is UTC): quote them as
printed. If your act runs past 00:00 SAST, keep the header's D for what you typed before it, and say in your notes which
steps were played after midnight and what D they read (BRIEF § 11, flow 13).

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t400 <file.sql>
- If a step's Note prescribes an UPDATE (ageing a row) and the session refuses to run it, mark that step not-played,
  citing T337, and play on.
- A stored jsonb (an audit row's SummaryJson) is never compared with a page's text as a string: Postgres re-renders it
  (CLAUDE.md). Parse both sides.

A step that waits a real hour (acts 2 and 3) waits in short background waits (a few minutes each), playing
read-only steps meanwhile and saying so; never one long wait. Do not hand back during a wait: your act ends with its
dump, and only then do you report (BRIEF § 11, flow 17).

Write your Actual and Gap lines into the act file as you go, at the end of each phase, not only at the end of the act:
a replay can be killed part-way (BRIEF § 11, flow 03), and the lines already written are what survives. Every script
writes what it saw to a file under <G>\<DIR>\out\ as it goes (one file a step: the words, the focus, the measures).
If your act file already holds "T400 replay" Actual lines when you start, an earlier run was interrupted: do not replay
those steps. Write any step the out\ files show as played but the act file lacks from those files, without playing it
again; read the database to find where the state stands; and play on from the first step not yet played. Record what
only the lost run saw (a result's words, where the focus went) as not seen (BRIEF § 11, flow 15).

Your remit is EVERY step of your act, setup included: a step named elsewhere in this prompt is the flow's step inside
your act, never a limit (BRIEF § 11, flows 08 and 09).

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows, and against the log or SQL where the Expect says so.
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (<D>, T400 replay, wombat_scenario_t400): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-A.4.2a, T286 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see, write "Gap: none (T<n> no longer occurs: …)". Flow
     18 should close: T327 (System health's stubs and dots, the account count that skipped role-less accounts, run
     history's free-text key; F-1.1a, F-A.2.10b, F-A.6.2a-c), T326's institutions half (the subtitle promising a
     drill-down; F-1.6a, F-A.6.1a), T286's audit display (command rows by email; F-3.55a, F-3.56a), T277's audit half
     (the way back losing the filters; F-3.56b), T324's job-runs half (Triggered by a user id; F-A.2.10a) and T325's
     and T384's page half on these pages (a time with no zone, From read as a UTC day; F-A.2.10c). The runbook already
     says "closed by T400" where it expects them gone. Still open, keep citing them where they still fail: T286's
     other half (a password change writes no audit row, F-A.4.2a), T157 (the deploy), T274 (a run's outcome stored),
     T114 items 2-3, T399 (pages outside flows 17 and 18 print an exception's text through RefusalText.Of), T390 (a
     pager hidden when its size holds every row), T396, T401 (the College's records keep an Active box), and the other
     halves of T324, T325, T326 and T323.
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST", ‹n›, ‹next Monday›),
     fill it from what the app shows in your Actual line; a bracket is not a failure. A count the Expect gives "in the
     T398 replay" (166 actions, 174, 21 runs, 22) is that replay's: give yours and trace any difference to the steps
     that set it (a scheduler's hourly run more or less) before calling it a finding.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 18 from the code, and no
   replay has seen it yet: 1.1, 1.6, 1.11, 1.23, 3.55-3.57, 4.38, A.2.1, A.2.2, A.2.4, A.2.9, A.2.10, A.5.5, A.6.1,
   A.6.2 and A.7.11. Their old Actuals are T398's and predate flow 18 (their Gap lines say "The T398 Actual above
   predates T400"; A.2.9's Actual marks its "pre-T400 words"); replace them as usual. Two Routes changed: 1.23 and
   A.5.5 now stay at the institution's own address (no /not-found).
   - Flow 11's to 17's habits still hold: the mail's registration link is
     http://localhost:5180/account/register/start?token=… (open THAT; never /account/register?token=…); invitation Stub
     email lines are tagged `invitation` only; each admission sends a mail; Copy link needs the context's permissions
     ['clipboard-read', 'clipboard-write'] for http://localhost:5180 (README § The browser). Role changes and the lock go
     through a note panel or a dialog. Count stub mails from what the log shows.
   - The end of a programme: Mark complete… and Deactivate… check the last day on the press, before any dialog; play
     the refusal the step names, then the dialog, and quote the forecast and the result's count as the page prints them.
   - Verify: type and paste hashes exactly as the step says (upper case, spaces); quote the answer's dates and counts.
   - Data rights (flow 14): every decision sends a stub mail; count them from the log and quote each subject. The
     erasure is confirmed in a dialog ("Approve and erase…" opens it, "Erase and approve" confirms). A correction is
     recorded from the Correction section's fixed list, then Complete.
   - The catalogue (flows 15 and 16): adoption is on Curricula (Open for adoption; a superseding Adopt… asks first:
     quote the dialog's registrar count and words). An item is added, edited or read on its own page; the College's
     specialities are one page, its tree (/admin/colleges/{CollegeId}/specialities). A save, an add or a create returns
     with a one-shot token (?added=, ?saved=, ?back=, ?created, ?created=…, ?cloned=): quote the result, say where the
     focus is, check the page took the token out of the address, then reload and say the result is gone. A refusal sits
     under its field with the focus, the typed value kept; one that follows a confirmed action sits in the page's result
     region, focused. Every confirmation dialog opens with the focus on Cancel: say so. Find Colleges by short code,
     specialities, sub-specialities and scales by name, EPAs, curricula and items by code and version, never by a
     remembered id.
   - The activity types (flow 17): find a type by its Key or Name (SQL: "ActivityTypes"."Key"). The list's search
     applies on Show only; the builder's results sit above the tabs and take the focus; a refused save's summary takes
     the focus, its line naming the tab and the place.
   - Every page with header actions keeps them beside the h1 from 1024 px (Q20).
   - The operator's pages (flow 18): find an institution by its short code, a job by its key, a run by its job and
     start, an audit entry by its action, person and time (SQL: "Institutions"."ShortCode", "ScheduledJobRuns"."Key",
     "AuditEntries"), never by a remembered id. Run history and the audit log filter on Show only: typing or choosing
     alone changes nothing; quote the count line ("21 runs.", "166 actions.") and say it took the focus, and read the
     address Show wrote. Every result on these pages ("‹name› created.", "‹name› saved. …", "Ran ‹key›: …", "Disabled
     ‹key›. …", "Enabled ‹key›. …") sits above the form or the list and takes the focus: say so, and for a token
     (?created) check the page took it out of the address, then reload and say the result is gone. Home's System
     health: quote each line's name, its badge's word and its sentences in order, and say that no dot and no "(T0"
     is drawn. Times are quoted as printed, with "SAST".
   - If a step cannot be played as written, or its Expect disagrees with what the built page does, record that in its
     Gap ("Gap: runbook: ...") and play on as far as you sensibly can.
5. Capture the step's outcome screenshot(s) to design\baseline\<DIR>\.
   - KEEP THE FILE NAMES THAT ALREADY EXIST for the step when the state is the same (list the folder first:
     <step>-<n>-<slug>.png). Design briefs cite them by name.
   - Add a new file only for a state the step did not capture before.
   - Full page, taken from the top: scroll to (0, 0) first (the focus stays put), or the fixed sidebar and top bar are
     drawn part-way down the image. A states capture can then reuse the image instead of retaking it.
   - The folder is gitignored and local only.
   - Say in your notes which captures show a registration link.

Edit no file but your act file, your tmp dir and the baseline images.
Write SQL, scripts and scratch files under your act dir, never in the repo. A script re-run after its step has changed
the state is refused but still leaves audit rows and overwrites captures: re-run only a read, and say so if you did.

At the end of your act:
- Check its outcome state (the act file's closing SQL, if any).
- Snapshot it:
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t400 scenario-t400-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t400: the flow 18 build made it wrong. Flow 18 covers the Administrator's Home (/: System health's three
  lines, their badges and sentences, Accounts, the footers' buttons, the grid and the health list by width), the
  institutions at /admin/institutions, /admin/institutions/new and /admin/institutions/{Id} (both roles: the subtitle,
  the rows, Edit's name, the create page's refusals and focus, "‹name› created." and the token, the record by its name,
  the Status section, Deactivate… and Reactivate… and their dialogs, the save's result, Institution unavailable), the
  jobs at /admin/jobs (the schedule words, Last run and Next run, the results above the list, Run now unavailable and
  refused on a disabled job), run history at /admin/jobs/runs (the filters, Show and the address, the count line, Ran
  by, the pager, the stack to 1024 px), the audit log at /admin/audit and an entry at /admin/audit/{id} (the filters,
  the window line and the address, the names, Failed with its words, the sections, the payload region, the trail back,
  Audit entry unavailable), every failed audit row's stored text (a refusal's words or "The action failed."), the
  mail outcomes table and the retention job's log line, and each job's description (no UTC clause).
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T400 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 18 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual. Wherever it first shows Institutions, Scheduled jobs, Run history or the audit log at 1280, look at the
capture: a value squeezed to a few words a line beside an empty band, or a column of one-word pieces (a job's key broken
at its hyphens, a command name over three lines), is a finding even though nothing scrolls (BRIEF § 11, flows 15 and
16).
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t400-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know (ids,
the stub mail count, password keys added or changed, by name only), which captures show a registration link, the
measures your act's checks asked for, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\pw`
  (Playwright 1.56.1 and `lib.mjs`: launch, login, logout, waitBlazor and BASE; flows 14–17's replays used it, and it
  was there on 2026-11-12).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--dev-Wombat/e81867ab-c351-40bf-9d41-d1d61fe30f43/scratchpad/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\9c6f7097-96ff-4516-94b8-576d4f4e4e14\scratchpad\g400`
  (a new, empty `replay-pw.env` for this replay). T398's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\g398\act-*`
  (each act's `h.mjs`, or `common.mjs` in acts 4, 5 and A, holds the shared helpers: devPw, envPw, setEnvPw, shot, txt,
  go, the stub mail blocks); its states scripts (`holds.mjs`, `c.mjs`, `meas.mjs`, `meas-list.mjs`) in `g398\states`.
  Copy what helps into `<G>\<DIR>`, fixing paths (the lib.mjs import above, `<G>` for the password file), ids, database
  names (wombat_scenario_t400) and dates (compute every date from D), and rewriting the scripts of the steps flow 18
  changed (below) for its pages: Show on the audit log and run history, the Job select, the results above the lists,
  the record by its name and its Status section, Unavailable in place of Page not found. Never copy or read g398's
  `replay-pw.env` (or any older one).
- **Every act's browser checks for flow 18**, beyond its own (below): a result or a count line is checked for the focus
  by keyboard where a scripted change says it moved (BRIEF § 11, flow 14); a dialog opens with the focus on Cancel,
  Cancel first in tab order, Escape returning the focus to its trigger, and is captured open before it is cancelled or
  confirmed as the step says; a flow 18 page's table measured at a width reads each `.table-container`'s table
  `scrollWidth` against its `clientWidth`, the table's narrowest (`width: min-content`) against the container (the
  spare), and the page's own `scrollWidth`; at 390 every visible control in `main` is 44 px or more.
- **For the integrator, after the acts:** look at one 1280 capture of each flow 18 list (Institutions at A.6.1, Scheduled
  jobs at A.2.1, Run history at A.2.10, the audit log at 3.55 and 3.57) and Home at 1.1 and A.6.2, before the states
  pass (BRIEF § 11, flows 15 and 16). A width fix found mid-replay waits until the acts using the published app are done;
  republish between the states passes, read every column of the list it reaches at 1280 for each role that sees it (the
  Administrator; Prof Mbatha for the audit log and her record), sweep 390–1440, and re-take the captures the fix changes
  (BRIEF § 11, flows 16 and 17). Two states rows are taken in the acts (act 1's `institution-create--email-malformed`,
  act A's `home--administrator-job-disabled`): pass 1 checks them and removes their "Not yet captured (T400)" marks.

## What flow 18 changed, step by step

Each changed step's Gap line says "The T398 Actual above predates T400" (A.2.9's Actual marks its "pre-T400 words";
the fix pass, `439546c5`, rewrote its Expect after lane W, `88ee3636`). Seventeen steps changed: act 1 four, act 3
three, act 4 one, the appendix nine. **Acts 2, 5 and 6 hold no changed step**: no step of theirs opens a flow 18 page
(6.38's Curriculum progress is flow 15's). Unchanged steps on flow 18's pages, played as written and read for the new
words: 3.32, A.2.6, A.2.8 (Run now's result, which their T398 Actuals quote as "dispatched."), A.5.2 (Access denied on
the operator's pages, flow 01's, unchanged), A.5.13 (the menu), A.6.3 (Prof Mbatha's own record, on the rebuilt page),
A.6.9 and A.6.10 (/health). The audit pipeline's new failure text reaches every flow: a Failed row stores its refusal's
words, or "The action failed." for an exception no Wombat code threw as a refusal (P1, F1).

### Act 1
- **1.1** devadmin's Home: no header action, no switch; two cards. System health, three lines in order, each a word on
  a badge, no dot, no task id: Database OK, "The database answers."; Mail Not configured, "Mail is written to the log,
  not sent." and "To send mail, set Email__SmtpHost on the server and restart Wombat."; Scheduled jobs OK, "7 jobs last
  succeeded. weekly-coordinator-digest has not run yet: its first run is ‹next Monday› 10:00 SAST." (D = 2026-11-12, a
  Thursday: 2026-11-16), Scheduled jobs a footer button. Accounts 8, or 9 where the bootstrap Administrator exists,
  over "Every account Wombat holds but an erased one." and "None is locked.", Users a footer button. The grouped menu
  as before. (F-1.1a, T327 closed.)
- **1.6** Institutions subtitled "The institutions that hold Wombat's people and records.", the Demo Institution alone,
  Active, Edit; Create institution subtitled "A new institution is active from the moment it is saved, so its people
  can be invited.", the mark line, Name * ("Up to 200 characters."), Short code * ("Up to 32 characters."), Contact
  email ("Optional."), no Status; the empty Save's two refusals under their fields, each field marked and described by
  its refusal, Name focused; Save lands on the record headed and tabbed "Kgosi Kgari Teaching Hospital", "KGK · Active"
  under it, "Kgosi Kgari Teaching Hospital created." above the form, focused; Status Active, "A new invitation can name
  it.", Deactivate… in a section of its own; back by the trail's Institutions, KGK a second row, Created a day with no
  time. (F-1.6a, T326 closed.)
- **1.11** Active invitations "No active invitations"; Home's Accounts two higher than 1.1's (11 where 1.1 read 9).
- **1.23** Prof Mbatha's own record as flow 12 left it; `/admin/institutions/new` "You cannot open this page"; the Demo
  Institution's id stays at its address: "Institution unavailable" (heading and tab), "This institution does not exist,
  or you cannot open it.", Go to Home, the trail Home › Institution unavailable; an unknown id the same.

Act 1's checks:
- 1.1: Home at 1280 and its sweep (`sweep.md` § Home): every 25 px from 900 to 1150, each px from 1095 to 1120, and
  1151: the grid's tracks (one at 900, two from 901 to 1099, three from 1100), the health list's width and whether its
  lines lay out narrow (the badge beside the name, over the sentences: expected 1100–1112) or wide (expected from 1113,
  and up to 1099), each card's `scrollWidth` against its `clientWidth`, the page unscrolled. Also at 390, 640, 641 and
  768 (the list narrow at 390, 641 and 768, wide at 640), and the footers' buttons 44 px at 390.
- 1.6: the address after Save carries `?created` only until the page has read it: say it was taken out; reload, the
  result gone. The empty Save's refusals: each input with aria-invalid and its aria-describedby naming the refusal first,
  then the help. Institutions after the create (2 rows): every 25 px from 900 to 1150 plus 1025 and 1151 (stacked to
  1150, a table from 1151; `sweep.md` measured the smallest spare 180 px at 1151), and every column at 1280 (`sweep.md`:
  Name 279, Short code 116, Contact email 279, Status 95, Created 115, Actions 87; nothing over one line).
- 1.6, one states capture only this act can take (no snapshot holds the empty create page before KGK): before the real
  Save, type the name, `KGK` and `paeds-admin@` in Contact email and press Save. Expected: "Enter a valid email address,
  for example name@example.org." under Contact email, the field marked and focused, the other fields as typed, nothing
  sent (no KGK row by SQL; if a row was written, stop and report it). Capture it at 1280 from the top as
  design\baseline\states\institution-create--email-malformed.png, check it against that `states.md` row, then correct
  the address and Save as the step says, and say so in 1.6's Actual.
- 1.23: the Unavailable pages name no id; an unknown id (`/admin/institutions/99999`) reads the same; nothing in her
  menu lit.

### Act 3
- **3.55** Prof Mbatha's audit log subtitled "Every consequential action at Kgosi Kgari Teaching Hospital, newest
  first."; the last 24 hours on arrival; From (SAST) set to the act's sitting and Show: "‹n› actions." focused, the
  window line "Read from ‹From› SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window."; every
  row KGK's, 50 a page, each person by name, Success; Failures only "4 actions.", each Failed with its refusal under the
  badge (Ndlovu's two creates, Dlamini's submit, Khumalo's decline); "Msf" "19 actions."; no respondent or devadmin row.
  (F-3.55a, T286 closed.)
- **3.56** The entry: "Audit entry", trail Home › Audit log › Audit entry, subtitle "CreateActivityCommand by Sipho
  Ndlovu, ‹time› SAST · Failed"; What happened (SAST to the millisecond, "Stored as … UTC", Command, the action, Kgosi
  Kgari Teaching Hospital, Failed with "Date observed: The date cannot be after today (…).") ; Who (Sipho Ndlovu, the
  user id, ::/48 and "Cut to its first 48 bits when stored."); no Payload; the trail's Audit log back to Failures only,
  page 1, the entry's Detail focused; no Back to log. (F-3.56a, T286; F-3.56b, T277 closed.)
- **3.57** devadmin: the subtitle "Every consequential action across the institutions, and those with no institution,
  newest first.", Institution (All institutions, No institution, Demo Institution, Kgosi Kgari Teaching Hospital); "‹n›
  actions." (174 in T398's), each person by name over their institution; eight SubmitMsfResponseCommand rows "MSF
  respondent (no account)" over "No institution"; devadmin's RunScheduledJobNowCommand by Demo Administrator; the
  submission's Who ("No person: an MSF respondent answers by link, with no account.", User id None, ::/48); its Payload
  laid out, token and answers "[REDACTED]"; Dr Naidoo's principal "[PRINCIPAL]", data patch and note "[REDACTED]".

Act 3's checks:
- 3.32 (unchanged): quote Run now's result as the page prints it ("Ran assessor-pending-nudge: Succeeded at ‹…› SAST."
  above the list, focused) and the row's Last run and Next run in SAST; the old "dispatched." is gone.
- 3.55: typing in Action or choosing Result alone changes nothing until Show; the address Show writes (`shown=`, `from=`,
  `result=`, `action=`); a page turn (Next, if more than 50 rows) keeps the window line and focuses h2 "Actions"; Clear
  filters (once a filter is applied) focuses Category. The audit log at 1280 as Prof Mbatha, the act's window, every
  25 px from 900 to 1150 plus 1025 and 1151 (stacked to 1150; `sweep.md`'s smallest spare 144 px at 1151), then every
  column at 1280 with a failure on the page and with Failures only (`sweep.md`: When 173, Person 135, Category 131,
  Action 250, Result 194, Detail 88; a command name at most 2 lines; a name over 2 lines, or a key broken at each
  letter, is a finding). At 390 and 768: rows stacked, nothing sideways, controls 44 px at 390.
- 3.56: the entry at 390 and 768, nothing sideways; the Detail focused by keyboard after the trail back.
- 3.57: the same sweep and columns as devadmin (`sweep.md`: smallest spare 133 px at 1151; When 163, Person 166,
  Category 131, Action 228, Result 194, Detail 88). The payload region: `tabindex="0"`, role region, named by its h2;
  its content scrolls inside it (height capped at 28rem of content) and never sideways, the page unscrolled, at 1280,
  768 and 390.
- A Failed row's stored text by SQL for the four failures ("AuditEntries", Failures only): each a refusal's own words,
  none an exception's type or a stack line.

### Act 4
- **4.38** Prof Mbatha, RecordCommitteeDecisionCommand and Show: "5 actions.", five Success rows by Thandi Zulu, no
  Failed row; an entry: What happened, Who and What it was done to, no Payload; SummaryJson by SQL keeping ReviewId,
  Category and PresentUserIds, Rationale, Conditions and UndecidedNote "[REDACTED]"; RevokeEntrustmentDecisionCommand:
  "1 action.", its person Refilwe Mokoena, its stored Reason "[REDACTED]".

Act 4's checks: the count lines focused after Show; the singular "1 action."; the entry's trail back to the filtered rows.

### Appendix
- **A.2.1** Scheduled jobs subtitled "The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or
  read every run in Run history."; Job, Schedule, Last run, Next run, then the actions, no Enabled column; the key over
  its description; the schedule in words in SAST over "cron …, UTC"; Last run in SAST with its badge; Run now and
  Disable named for the job; "Not run yet" for a job never run; scheduled-job-run-retention "Deletes job run records
  and mail outcomes older than 90 days."; the eight keys; Run history a header action.
- **A.2.2** Disable: "Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it." above the
  list, focused; Next run "None while disabled", Enable, Run now unavailable with "Enable it to run it." beside it, a
  press sending nothing; Enable: "Enabled msf-campaign-auto-close. Its next run is ‹the next hour› SAST." (or "It runs
  at the next check, within 30 seconds." after a missed hour); both changes in the audit log.
- **A.2.4** "Ran weekly-coordinator-digest: Succeeded at ‹YYYY-MM-DD hh:mm› SAST." above the list, focused; the log line
  with one coordinator skipped as opted out; no digest greets Pieter.
- **A.2.9** (the fix pass) Each run Succeeded, one log line each; scheduled-job-run-retention "no old runs or mail
  outcomes to delete."
- **A.2.10** Run history subtitled "Every run of the eight jobs, newest first, kept for 90 days.", trail Home ›
  Scheduled jobs › Run history; Job a select of the eight keys, All jobs first; Status; From and To "a South African
  day"; Show. Newest first: job, start in SAST, duration, status badge, Ran by "Demo Administrator" or "Scheduler".
  Unfiltered "‹n› runs." and "Showing 1–‹n› of ‹n›" (the 10 by hand and the scheduler's: trace yours); the digest "2
  runs." with Clear filters; Failed: "No runs match these filters.", the card "No runs match", "The filters are kept
  above. Clear them to see all ‹n›." and Clear the filters; From today: today's runs. (F-A.2.10a, T324; F-A.2.10b,
  T327; F-A.2.10c closed.)
- **A.5.5** The Demo Institution's id, as Prof Mbatha: "Institution unavailable" at its own address (heading and tab),
  "This institution does not exist, or you cannot open it.", Go to Home, trail Home › Institution unavailable; an
  unknown id the same; the user half unchanged (User unavailable).
- **A.6.1** Institutions (subtitle, two rows each with its short code, contact email, Active, the day, Edit named for
  it); KGK's record by its name, "KGK · Active", no Back, the trail Home › Institutions › Kgosi Kgari Teaching
  Hospital; Name, Short code, Contact email, Cancel and Save; Save: "Kgosi Kgari Teaching Hospital saved. Its contact
  email is now paediatrics@kgk.wombat.local." focused; Status Active, "A new invitation can name it.", Deactivate… in
  a section of its own, no Active box. (F-A.6.1a, T326 closed.)
- **A.6.2** System health: Database OK, "The database answers."; Mail Not configured, "Mail is written to the log, not
  sent."; Scheduled jobs OK, "All 8 jobs last succeeded."; Accounts 24 (1.11's 11, the fourteen Act 2 registered, less
  Dr Ndlovu's erased account), "Every account Wombat holds but an erased one.", "None is locked."; Scheduled jobs and
  Users footer buttons. (F-A.6.2a-c, T327 closed.)
- **A.7.11** At 390: each job a stacked block (state, next run, Run now and Disable in view, each 44 px); Run history's
  filters and runs stacked under "‹n› runs." (A.2.10's plus the scheduler's since); Institutions' rows stacked, KGK's
  contact hod.paediatrics@kgk.wombat.local (A.6.3); nothing sideways.

The appendix's checks:
- A.2.1: Scheduled jobs at 1280 (8 rows, no error): every 25 px from 900 to 1150 plus 1025 and 1151 (stacked to 1150;
  `sweep.md`'s smallest spare 307 px at 1151), then every column at 1280 (`sweep.md`, no error: Job 367, Schedule 139,
  Last run 143, Next run 141, Actions 183; descriptions 2 lines, a schedule's words and its cron 2 lines each; a key
  broken at its hyphens, or Last run's day and hour apart, is a finding).
- A.2.2: Run now on the disabled job is `aria-disabled`, not `disabled`, still in the tab order, its reason as text
  beside it; a press (by keyboard too) writes no run (count "ScheduledJobRuns" for the key before and after) and no
  result. **One states capture only this act can take** (no snapshot holds the disabled job): after Disable, before
  Enable, open Home in a second page of the same context (a read; change nothing), capture it at 1280 from the top as
  design\baseline\states\home--administrator-job-disabled.png, check it against that `states.md` row (Scheduled jobs
  Disabled, "msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 6 last succeeded.
  weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST.") and say so in A.2.2's Actual.
- A.2.4, A.2.6, A.2.8, A.2.9: quote each Run now result as printed ("Ran ‹key›: Succeeded at … SAST."), focused, above
  the list; A.2.6 and A.2.8 are unchanged steps whose T398 Actuals quote "dispatched.".
- A.2.10: the address Show writes (`?job=…&status=…&from=…`); Clear filters and "Clear the filters" focus Job; the pager
  ("Showing 1–‹n› of ‹n›", Per page) and whether it is drawn when one page holds every row (T390, other pages'; cite it
  if hidden). Run history at 1280 (all jobs, then one job): every 25 px from 900 to 1100 plus 1024 and 1025 (stacked to
  1024, a table from 1025; `sweep.md`'s smallest spare 144 px all jobs and 178 px one job, at 1025), then every column
  at 1280 (`sweep.md`, no error on the page: expect Job one line for every key; "Demo Administrator" may take 2 lines).
- A.5.5: no id on either Unavailable page; Go to Home is the page's only action.
- A.6.1: Institutions' columns at 1280 as in act 1, now with A.6.1's two rows; the record: Deactivate… opens "Deactivate
  Kgosi Kgari Teaching Hospital?" with the focus on Cancel; quote its body ("No new invitation can be issued for it until
  it is reactivated. …"), Escape returns the focus to Deactivate…, then Cancel: NEVER confirm (the institution must stay
  active). The record at 390 and 768, nothing sideways.
- A.6.2: Home's badges' contrast (Not configured amber, OK green), each word 4.5:1 or more on its badge; the S2 band
  again at 1100, 1112 and 1113.
- A.6.3 (unchanged step, rebuilt page): any difference from T398's Actual is a finding (regression-t400 if it comes from
  the record).
- A.7.11: also at 768 (nothing sideways); at 390 every visible control in `main` 44 px, the jobs' and runs' blocks, and
  Home's footer buttons.

## Flow 18's states to re-take

Every `states.md` row of flow 18's pages: Home's Administrator rows (`:141`–`:150`), Institution administration
(`:891`–`:914`, the Administrator's and Prof Mbatha's record rows, on a page the build rewrote) and Platform operations'
audit, jobs and runs rows (`:1097`–`:1129`): 67 rows. Of them 28 are lane W's rewrites, 28 are marked "Not yet captured
(T400)" (\*), and 11 are unchanged rows on the rebuilt pages (three Narrow captures from the old frame, the record's and
the audit log's and an entry's, and Prof Mbatha's eight record rows, flow 12's). Two passes after the appendix: pass 1 the reads (rows reached at a step, read-only
on :5180 at the appendix's end, or by holding a read on a scratch copy); pass 2 the writes and the staged conditions, on
scratch copies of `scenario-t400-post-actA` only, never on wombat_scenario_t400.

**Pass 1** (42 rows; 7 \*):
- Home: `home--administrator` (at 1.1), `home--administrator-job-disabled`\* (act A takes it at A.2.2: check it).
- Institutions: `institutions-list--one`, `--two`, `--loading`, `--narrow`; `institution-edit--create`, `--invalid`,
  `institution-create--email-malformed`\* (act 1 takes it at 1.6: check it); `institution-edit--administrator`,
  `--saved`, `--not-found`, `--unavailable-admin`\* (typed, read-only on :5180), `--narrow`; Prof Mbatha's `--own`,
  `--short-code-empty`, `--email-malformed` (refused on the page, nothing sent; reload), `--loading`, `--load-error`.
- Audit: `audit-list--institution`, `--failures`, `--administrator`, `--no-institution`\*, `--empty`, `--loading`,
  `--narrow`; `audit-detail--failure`, `--payload`, `--large-payload`\* (the sweep found the largest payload a refused
  activity type save, 1,974 characters: find it by SQL), `--subject`\* (an entry whose subject columns are set: find
  one by SQL), `--erased`\*, `--not-found`, `--narrow`.
- Jobs: `scheduled-jobs-list--loaded`, `--disabled`, `--dispatched`, `--loading`, `--narrow`; runs:
  `scheduled-job-runs-list--loaded`, `--filtered`, `--empty`, `--narrow`.
- Reuse the acts' captures where they show the row's state exactly, from the top, at the row's width; else reach the
  state again on a scratch copy of the snapshot before it (a step inside act N: post-act(N-1), played up to the state).

**Pass 2** (25 rows; 21 \*), each on a scratch copy of `scenario-t400-post-actA`:
- Home: `home--administrator-mail-idle`\*, `--mail-ok`\*, `--mail-failing`\*, `--job-failed`\*, `--job-overdue`\*,
  `--all-failing`\*, `--database-down`\*, `--locked`\*.
- Institutions: `institutions-list--long-name`\*, `institution-create--duplicate`\*, `institution-edit--save-failed`\*,
  `--deactivated` (then Reactivate…'s dialog and result checked, no new file); Prof Mbatha's `--deactivate-refused` (at
  A.6.3: reuse act A's capture), `--email-cleared`, `--duplicate`.
- Jobs: `scheduled-jobs-list--running`\*, `--already-running`\*, `--run-failed`\*, `--last-failed`\*,
  `--disabled-refused`\*, `--disable-failed`\*; runs: `scheduled-job-runs-list--page-2`\*, `--running`\*,
  `--its-runs`\*, `--none`\*.

**How each staged condition is reached** (inferred from the code at `e6f928c2`; the states agent checks each by SQL or
by the page before capturing, and records any that does not hold as not reached):
- **A mail server** (mail-idle, mail-ok, mail-failing, all-failing): the scratch app started detached with the tool's
  third argument, `start <scratch db> 5181 smtp=keep`, which keeps Development's `localhost:25`. For mail-idle and
  mail-ok, first run a minimal SMTP sink on port 25 (a Node `net` server answering 220, 250 to EHLO/HELO, MAIL, RCPT,
  354 to DATA, 250 after the lone ".", 221 to QUIT; listening on both 127.0.0.1 and ::1, since "localhost" may resolve
  to either; no STARTTLS offered), written under the states dir, stopped by its own id afterwards. Nothing else may
  listen on 25 first (check). Mail-idle: Home before any mail is sent (a post-appendix copy holds no mail outcomes:
  the replay's mail went to the log). Mail-ok: Run now on weekly-coordinator-digest (it mails the coordinators), then
  Home. Mail-failing: stop the sink, Run now on the digest again, wait for the three tries (2 s and 4 s apart), then
  Home: the refusal is the connection's. With smtp=keep no Stub email lines are written.
- **A job that fails** (run-failed, job-failed, its-runs, all-failing): `ALTER TABLE "EntrustmentDecisions" RENAME TO
  "EntrustmentDecisions_off";` on the scratch, then Run now on entrustment-decision-expiry (it reads that table): the
  run fails with PostgreSQL's "relation does not exist". Capture the jobs page's result, Home's Failing line and its
  Its runs, then rename it back and Run now again to clear it. Never on wombat_scenario_t400.
- **A job overdue** (job-overdue, all-failing): the scheduler runs any missed job it knows within 30 seconds, so the
  stand-in is a job it does not know: insert into "ScheduledJobDefinitions" an enabled definition with a key no job
  registers (e.g. `replay-overdue-probe`, cron `0 * * * *`, a description) and one Succeeded "ScheduledJobRuns" row for
  it started three hours ago; Home judges it overdue by the same rule (`JobSchedule`) and the host never starts it.
  Read the columns from information_schema first; delete both rows after. Say in the report that this is a stand-in.
- **The database down**: on the scratch, with Home open, from a `sql` session on ANOTHER scratch database of the pass
  (never wombat_scenario_t400 or the sweep's): `ALTER DATABASE <scratch> ALLOW_CONNECTIONS false;` then
  `SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '<scratch>';`; read Home again inside the
  circuit (its nav link) within the minute; capture; then `ALTER DATABASE <scratch> ALLOW_CONNECTIONS true;` the same
  way. The role owns the scratch databases (the tool restores them with OWNER), which these statements need (inferred).
  Never stop the PostgreSQL server: the dev app and the other databases share it.
- **Holding a read** (save-failed, running, already-running, disable-failed, and pass 1's loading and load-error rows):
  `states.md` § Holding a read, on the scratch only, under two minutes, `ROLLBACK;` after. Run now waiting and Run now
  refused share one hold: Run now on a job in one session ("Running…", aria-disabled), then Run now on the same job in
  a second session: the job's lock is in the process, so the second is refused with "Job '‹key›' is already running, …"
  while the first waits on the database.
- **A failed last run** (last-failed): stop the scratch app, insert a Running "ScheduledJobRuns" row for a job (Status
  as the column stores Running: read a stored row and the enum `ScheduledJobRunStatus` first), start the app: the
  host's startup reconciliation marks it failed ("Marked as failed during startup reconciliation …").
- **A run still running; a long error** (runs `--running`): with the scratch app running, insert a Running row and a
  Failed row whose ErrorMessage is 2,000 characters (the column's limit; the sweep measured only 330).
- **Page 2 past 50** (runs `--page-2`): insert 60 Succeeded rows for msf-campaign-auto-close, an hour apart over the past
  days, each with a FinishedAt a second after its start and no TriggeredBy (the scheduler's).
- **No runs at all** (runs `--none`): on its own scratch, `UPDATE "ScheduledJobDefinitions" SET "IsEnabled" = false;`
  then `DELETE FROM "ScheduledJobRuns";` with the app running (a disabled job is never started), then Run history.
- **Locked accounts** (`--locked`): devadmin's Lock out… on three people's pages (their dialogs confirmed), then Home.
- **A stale page** (`--disabled-refused`): two browser contexts as devadmin on Scheduled jobs; Disable a job in one, then
  Run now on it in the other.
- One scratch database can serve a run of rows where each leaves it as the next starts: mail-idle, mail-ok, the failed
  job group, overdue, mail-failing, all-failing, then the database down, in that order; the runs' inserts on a second;
  the institutions' writes on a third; `--none` on its own. The scheduler on a scratch copy restored hours after the
  appendix may run due jobs at its first check (the hourly msf-campaign-auto-close, a nudge after 09:00 SAST): read the
  jobs' state before each capture.

# T364 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/07-review-sitting/replay-brief.md` (T361). Flow 08 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T364 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 08 of the restructure (the committee calendar: Decision panels and a panel's page with its seat
picker; Decisions due with its Trainee filter and trainee line, one action a row, By EPA in a fold, every Schedule on
the row's own period; the new schedule page /committee/reviews/new with its Agenda aside and "Already held" before
Create; Committee reviews without a Mode column), which landed on master as 622c5ad1. Flows 01-07 landed earlier
(b347e11c, f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t364, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t364.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t364 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t364_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t364 for that step.

Read first:
- execution/knowledge/scenario-paediatrics/README.md: How to play, The step format, and the Screenshots rule.
- Then your act file, in full.

Browser:
- The Playwright library is installed at <PW>\node_modules (v1.63). Its helpers are in <PW>\lib.mjs: launch, login and
  waitBlazor; BASE is http://localhost:5180.
- Write your scripts under <G>\<DIR>\ and import by absolute file URL, e.g.
  import { launch, login, waitBlazor } from '<PWURL>/lib.mjs';
  import { chromium } from '<PWURL>/node_modules/playwright/index.mjs';
- Launch Chrome with channel 'chrome', headless.
- Signed-in pages are Blazor Interactive Server: wait for the circuit before acting (waitBlazor).
- The sign-in, sign-out, link, forgotten-password and change-password pages are static (form posts).
- The default viewport is 1280x800; a step that says 390 px uses 390x844.

Accounts and passwords:
- Dev accounts (devadmin@wombat.local and the others): their dev-only passwords are constants in
  src/Wombat.Infrastructure/Identity/DevUserSeeder.cs. Read them in your script; never print them.
- Cast accounts registered during this replay:
  - Choose a strong password at replay time: 12+ characters, 4+ different characters, a digit, an upper-case and a
    lower-case letter, and a symbol.
  - Keep it ONLY in <G>\replay-pw.env as KEY=value lines (WB_PW_<SURNAME>=...), which later acts read into their
    scripts' environment. This is a new file for this replay; never read any other replay's password file.
  - A step that changes a cast member's password loses no value: it first copies the old one to
    WB_PW_<SURNAME>_<ACT> (the act that changed it), then sets WB_PW_<SURNAME> to the new one, so WB_PW_<SURNAME> is
    always current and the earlier snapshots can still be signed in to (BRIEF § 11, flow 03). A state capture on a
    snapshot taken before the change uses the _<ACT> key.
  - Never print a password to the console or into your reply, and never write one into the runbook or any tracked file.
  - Never open pwd_DO_NOT_COMMIT.txt or recovery\.
- Registration and MSF links come from the log's Stub email lines. Never write a token into the runbook.

The clock: D is the day your act is played (record it in your Actual lines; flow 08's replay starts 2026-10-13). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t364 <file.sql>
- If a step's Note prescribes an UPDATE (ageing a row) and the session refuses to run it, mark that step not-played,
  citing T337, and play on.

A step that waits a real hour (acts 2 and 3) waits in short background waits (a few minutes each), playing
read-only steps meanwhile and saying so; never one long wait.

Write your Actual and Gap lines into the act file as you go, at the end of each phase, not only at the end of the act:
a replay can be killed part-way (BRIEF § 11, flow 03), and the lines already written are what survives.

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows, and against the log or SQL where the Expect says so.
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (<D>, T364 replay, wombat_scenario_t364): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 08 should fix, for example, the held seat
     warned before Create (T309 item 1), Formative check-in as a Review type (T309 item 3), the holding review's own
     panel (T259), the panel's page named and its warning re-read (T260), "no committee routing" while loading (T329),
     the preview's failure text (T338, T272), the panel and schedule field messages (T324), the pager's unnamed select
     (T280, F-4.1a) and the focus after a page change on Decisions due (T264)), write "Gap: none (T<n> no longer occurs: …)".
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 08 from the code, and no
   replay has seen it yet: the steps 2.20-2.25, 2.32, 2.35, 2.37, 4.1-4.14, 4.33, 4.34, 4.37, 4.49, 4.51, 5.1, 5.2,
   5.29, A.1.10, A.1.14, A.7.5-A.7.8 and A.7.14. Their old Actuals describe the pages before flow 08; replace them as
   usual. Step 4.2's S1 agenda lines were written from the planner's code, not seen: check them closely.
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

At the end of your act:
- Check its outcome state (the act file's closing SQL, if any).
- Snapshot it:
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t364 scenario-t364-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t364: the flow 08 build made it wrong. Flow 08 covers Decision panels (/committee/panels), a panel's page
  (/committee/panels/new and /committee/panels/{id}: the seat picker, Chair, About this panel, Decides for and its
  lock, the can-no-longer-sit warning, the results), Decisions due (/committee/decisions-due: the filters and Show,
  the Trainee filter and line, the rows' one action and sentences, By EPA, the pager), Committee reviews
  (/committee/reviews: the list) and the schedule page (/committee/reviews/new: the form, Review type, the Agenda
  aside, Already held, Create, the refusals, Cancel), the "Formative check-in" label wherever it shows, the pager's
  label on every list, and CommitteeRefusedException's fixed failure words.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T364 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 08 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t364-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\842d7157-aa59-4fb6-9f09-284891ff7f72\scratchpad\replay\pw`
  (flow 02's installed helpers, reused).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--Users-Renier-Wombat/842d7157-aa59-4fb6-9f09-284891ff7f72/scratchpad/replay/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e59fcb9b-03d6-4b8e-bbd5-975292c9e3a2\scratchpad\g364`
  (a new `replay-pw.env` for this replay). T361's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e59fcb9b-03d6-4b8e-bbd5-975292c9e3a2\scratchpad\g361\act-*`:
  copy what helps into `<G>\<DIR>`, fixing paths, ids, database names and dates (compute every date from D), and
  rewriting the committee scripts of acts 2, 4 and 5 and the appendix for flow 08's pages (the seat picker's radios
  instead of multi-selects; scheduling on /committee/reviews/new, with Review type radios and no Formative only tick;
  Show on Decisions due's filters; By EPA in a fold).

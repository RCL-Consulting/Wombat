# T361 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/06-programme-oversight/replay-brief.md` (T358). Flow 07 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T361 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 07 of the restructure (the review sitting: the committee review page, its sitting bar, agenda,
evidence reader, decision form and dialogs; D53, no deferral: lines left unstaged are recorded "Not decided at this
sitting"), which landed on master as 44de180b. Flows 01-06 landed earlier (b347e11c, f50dffb2, 725237ee, 06aa51d7,
b020c942, 85d5a508).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t361, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t361.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t361 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t361_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t361 for that step.

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

The clock: D is the day your act is played (record it in your Actual lines; flow 07's replay starts 2026-10-12). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t361 <file.sql>
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
     Actual (<D>, T361 replay, wombat_scenario_t361): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 07 should fix, for example, the agenda table
     over Sign out (T356), the "(no longer in use)" mark (T273), the formative label (T309 item 2), the review half of
     T329 and of T260, T324's decision form, F-4.46a and F-A.7.13a/b), write "Gap: none (T<n> no longer occurs: …)".
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 07 from the code, and no
   replay has seen it yet. (Acts 4 and 5 and the appendix open with a banner: Actuals before T361 describe the page
   with deferral. Replace them as usual.)
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t361 scenario-t361-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t361: the flow 07 build made it wrong. Flow 07 covers the committee review page (/committee/reviews/{id}:
  the sitting bar, Start, Refresh, the agenda and its opened line, Stage and the foot form, Remove, the evidence reader,
  the decision form, Record, Ratify, Close, the appeals section), D53 (no Defer or Reinstate; Not decided at this
  sitting and the decision's optional sentence; the entrustment-only refusal), Decisions due's Not decided status and
  E3 rule, the formative label on the committee reviews list, the registrar page's review links and the PDF, the
  activity page's return link from a review's snapshot, and the shell's stacking under the chair's sitting bar.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T361 and not already filed.
- not-played: you could not play it (say why).
Wherever your act opens a review in progress as its chair, also check at 768 and 1280 px that the sitting bar never
paints over the top bar's Sign out (T356), and say so in that step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t361-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\842d7157-aa59-4fb6-9f09-284891ff7f72\scratchpad\replay\pw`
  (flow 02's installed helpers, reused).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--Users-Renier-Wombat/842d7157-aa59-4fb6-9f09-284891ff7f72/scratchpad/replay/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e59fcb9b-03d6-4b8e-bbd5-975292c9e3a2\scratchpad\g361`
  (a new `replay-pw.env` for this replay). T358's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\74d9a5be-052f-48f4-8cce-8475499dde63\scratchpad\g358\act-*`:
  copy what helps into `<G>\<DIR>`, fixing paths, ids and dates (compute every date from D), and rewriting act 4's and
  act 5's committee scripts for the new page (no Defer; dialogs on Start, Record, Ratify, Close and Remove).

# T342 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/02-sign-in-and-account/replay-workflow.js` (T339). Flow 02 ran its replay through the Workflow
tool. Flow 03 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T342 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 03 of the restructure (a registrar files an activity), which landed on master as 725237ee.
Flow 01, the shell, landed earlier as b347e11c, and flow 02, sign-in and account, as f50dffb2.
- The repo is C:\Users\Renier\Wombat, on branch master. Do not switch branches, do not commit, do not push.

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t342, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\Users\Renier\Wombat\.scenario-app\wombat_scenario_t342.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t342 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.

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
  - Never print a password to the console or into your reply, and never write one into the runbook or any tracked file.
  - Never open pwd_DO_NOT_COMMIT.txt or recovery\.
- Registration and MSF links come from the log's Stub email lines. Never write a token into the runbook.

The clock: D is the replay day (2026-09-29). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t342 <file.sql>
- If a step's Note prescribes an UPDATE (ageing a row) and the session refuses to run it, mark that step not-played,
  citing T337, and play on.

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows, and against the log or SQL where the Expect says so.
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (2026-09-29, T342 replay, wombat_scenario_t342): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 03 fixed T299's silent move, T323's narrow
     form, T329's crash, T325's history times and others), write "Gap: none (T<n> no longer occurs: …)".
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 03 from the code, and no
   replay has seen it yet.
   - If a step cannot be played as written, or its Expect disagrees with what the built page does, record that in its
     Gap ("Gap: runbook: ...") and play on as far as you sensibly can.
5. Capture the step's outcome screenshot(s) to design\baseline\<DIR>\.
   - KEEP THE FILE NAMES THAT ALREADY EXIST for the step when the state is the same (list the folder first:
     <step>-<n>-<slug>.png). Design briefs cite them by name.
   - Add a new file only for a state the step did not capture before.
   - The folder is gitignored and local only.
   - Say in your notes which captures show a registration link.

Edit no file but your act file, your tmp dir and the baseline images.

At the end of your act:
- Check its outcome state (the act file's closing SQL, if any).
- Snapshot it:
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t342 scenario-t342-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t342: the flow 03 build made it wrong. Flow 03 covers Log an activity, the activity page, My activities,
  the Activity inbox, Home's Needs you card, the form renderer (also in the builder's preview and the committee's
  reading), File it again, Save draft, the Cancel dialogs, the note panel, the history, the refusal summary, and the
  nominee and programme wording.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T342 and not already filed.
- not-played: you could not play it (say why).
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t342-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\842d7157-aa59-4fb6-9f09-284891ff7f72\scratchpad\replay\pw`
  (flow 02's installed helpers, reused).
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\6961d967-94bb-4e02-aece-0e817889fb63\scratchpad\g342`.

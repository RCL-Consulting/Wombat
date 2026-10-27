# T379 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/12-people-admin/replay-brief.md` (T376). Flow 13 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T379 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 13 of the restructure (graduation and exit: the former trainee's Home (her record card) and menu
(My progress, Export portfolio, My data rights); My authorisations admitting a graduate; My progress's "Your record:"
line under the ended notice; My activities' read-only line and note rows; Export portfolio, her own at
/portfolio/export and the staff export at /portfolio/export/{TraineeUserId} with its trail and "Portfolio
unavailable"; the Export action in the header of the trainee profile, the committee review page and the programme page;
Verify (a hash in any case or spacing, refusals under the field, the answer with first and last export in SAST, the
90-day cleanup job retired, so eight scheduled jobs); the trainee profile titled "‹name›'s programme" with its own "End
this programme" section, the last day checked on the press, the dialogs' and results' take-back counts, the account
section; Completed and closed paged with View; the graduation email linking My progress), which landed on master as
5cc10c66 (one migration, T379_PortfolioExportCleanupRetired), after T312 c9d6fe13 (an ended programme keeps the list
it ended with; migration T312_EndedCurriculumList). Flows 01-12 landed earlier (b347e11c, f50dffb2, 725237ee,
06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1, 49e7cf5e, 68327c9d, 69a17c70, e53d4ae7).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t379, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t379.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t379 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t379_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t379 for that step.

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

The clock: D is the day your act is played (record it in your Actual lines; flow 13's replay starts 2026-10-27). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t379 <file.sql>
- If a step's Note prescribes an UPDATE (ageing a row) and the session refuses to run it, mark that step not-played,
  citing T337, and play on.

A step that waits a real hour (acts 2 and 3) waits in short background waits (a few minutes each), playing
read-only steps meanwhile and saying so; never one long wait.

Write your Actual and Gap lines into the act file as you go, at the end of each phase, not only at the end of the act:
a replay can be killed part-way (BRIEF § 11, flow 03), and the lines already written are what survives.

Your remit is EVERY step of your act, setup included: a step named elsewhere in this prompt is the flow's step inside
your act, never a limit (BRIEF § 11, flows 08 and 09).

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows, and against the log or SQL where the Expect says so.
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (<D>, T379 replay, wombat_scenario_t379): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 13 should fix, for example, T311 (a graduate's
     Home, menu and STAR certificates), T314 (the staff export names no trainee), T313 and T287's verify item (a
     case-sensitive hash, the 90-day window), T312 (an ended record rewritten by a later catalogue change: 6.21, 6.40,
     6.41), and these pages' halves of T325, T329, T280 and T323; the runbook already says "closed by T379" or "by T312"
     where it expects them gone), write "Gap: none (T<n> no longer occurs: …)".
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 13 from the code, and no
   replay has seen it yet: 5.8-5.28, 2.30, 3.54b, 4.33, 6.17, 6.21, 6.34, 6.36, 6.40, 6.41, A.1.6, A.1.14, A.2.1,
   A.2.9, A.4.6, A.7.4, A.7.8a, A.7.9 and A.7.12. Their old Actuals describe the pages before flow 13; replace them as
   usual.
   - Flow 11's and 12's habits still hold: the mail's registration link is
     http://localhost:5180/account/register/start?token=… (open THAT; never /account/register?token=…); invitation Stub
     email lines are tagged `invitation` only; each admission sends a mail; Copy link needs the context's permissions
     ['clipboard-read', 'clipboard-write'] for http://localhost:5180 (README § The browser). Role changes and the lock go
     through a note panel or a dialog. Count stub mails from what the log shows.
   - The end of a programme: Mark complete… and Deactivate… check the last day on the press, before any dialog; play
     the refusal the step names, then the dialog, and quote the forecast and the result's count as the page prints them.
   - Verify: type and paste hashes exactly as the step says (upper case, spaces); quote the answer's dates and counts.
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t379 scenario-t379-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t379: the flow 13 build (or T312) made it wrong. Flow 13 covers the former trainee's Home and menu, My
  progress's record line, My authorisations for a graduate, My activities' notes, Export (own and staff) and its
  entry points, Portfolio unavailable, Verify, the scheduled jobs list, the trainee profile and its end, Completed and
  closed, and the graduation email; T312 covers an ended record against later catalogue changes (6.21, 6.40, 6.41).
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T379 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 13 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t379-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\842d7157-aa59-4fb6-9f09-284891ff7f72\scratchpad\replay\pw`
  (flow 02's installed helpers, reused).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--Users-Renier-Wombat/842d7157-aa59-4fb6-9f09-284891ff7f72/scratchpad/replay/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\67ce8168-e9a2-4dd0-acb1-eab0ccee0868\scratchpad\g379`
  (a new `replay-pw.env` for this replay). T376's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\67ce8168-e9a2-4dd0-acb1-eab0ccee0868\scratchpad\g376\act-*`:
  copy what helps into `<G>\<DIR>`, fixing paths, ids, database names and dates (compute every date from D), and
  rewriting Act 5's and the appendix's graduation, export, verify and end scripts for flow 13's pages (the profile's
  End this programme section, the staff export route, Verify's new answer). Never copy g376's replay-pw.env or read it.
- **Flow 13's browser checks** (build-review.md, T379's notes and the round-3 check), played where their step is:
  - My progress (5.20, 5.28): "Your record:" sits UNDER the ended notice; quote it.
  - The trainee profile (5.16, 5.17, 5.27, 2.30): a refused last day shows under the field, the field has the focus and
    aria-invalid; a cleared Programme start in Profile details is refused under that field (play it read-only only if
    the step does; otherwise check on the refusal the step plays). The dialogs' safe button is first in tab order.
  - Verify (5.13, 5.14, 5.23, A.7.12): "Exported 3 times" and the SAST times as the page prints them; an upper-case
    hash and a hash with spaces verify; at 390 the field is one line and nothing scrolls sideways.
  - At 901, 1000 and 1100 px: Completed and closed (5.17, 5.27, A.1.14) and My activities (5.28): nothing scrolls
    sideways, no word splits mid-word. Say what you measured.
  - At 390 (5.17, 5.21, 5.23, A.7.4, A.7.9): nothing scrolls sideways; the end section's triggers, Export PDF, Verify
    and View are at least 44 px tall; a dialog's safe button is first in tab order, the primary on top on screen.
  - Focus: an end's result, Export's result and Verify's answer take the focus after their action.
  - T312 (6.21, 6.40, 6.41): Molefe's record keeps PAED-012 and "15 of 15" during the pause and gains no KGK-001 card;
    du Plessis's gains none.
  - The derived values on the ended pages (R02, R04, R07: "0 recorded", the note rows' wording): quote what they read.

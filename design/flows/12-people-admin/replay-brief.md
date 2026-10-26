# T376 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/11-onboarding/replay-brief.md` (T373). Flow 12 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T376 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 12 of the restructure (the institution's people: Users /admin/users (role labels, not keys; a
filter; a pager only past one page; Locked out), the user page /admin/users/{UserId} in sections (Account, Roles with
Add a role… in a note panel and a Remove dialog per role, the scope per person, the Password part with Reset, the
lock with a confirmation dialog naming what waits, Reactivate, Institutional sign-in, and Assessor training at
?section=training with Record training), Assessors /admin/assessors (training status and date; /admin/assessors/edit is
now a redirect), her institution's record /admin/institutions/{Id} (Status as text, refusals in Wombat's words), SSO
group mappings /admin/sso/group-mappings (no provider: the not-set-up words, no form), and the Institutional admin's
Home (the waiting line, her institution's card, Entrustment decisions in her menu)), which landed on master as
e53d4ae7 (one migration, T376_AssessorProfileTrainingOnly), after its prerequisites T289 b8ed8fd6, T288 964a2c4e and
T333 5f9bb9bb. Flows 01-11 landed earlier (b347e11c, f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b,
622c5ad1, 49e7cf5e, 68327c9d, 69a17c70).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t376, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t376.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t376 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t376_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t376 for that step.

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

The clock: D is the day your act is played (record it in your Actual lines; flow 12's replay starts 2026-10-26). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t376 <file.sql>
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
     Actual (<D>, T376 replay, wombat_scenario_t376): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 12 should fix, for example, T264's users and
     SSO half (red in-row actions with no confirmation), T286's external-logins half, T326's assessors and SSO items
     (the empty Assessors body, the SSO page's words), T324's "The UserId field is required.", T289 (the assessor
     picker and the cross-institution move), T291 items 4 and 8, T323's email cut and "TraineeSystem-managed", and the
     flow 12 halves of T280, T325 and T329), write "Gap: none (T<n> no longer occurs: …)".
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 12 from the code, and no
   replay has seen it yet: 1.23, 2.12-2.15, 2.44, A.1.14, A.3.1, A.4.5, A.5.5, A.6.3, A.6.4, A.6.7, A.7.9 and A.7.14,
   and the crossing steps 1.6, 1.10, 1.21, 2.28, 5.10, 5.17, 2.37, 3.52, A.5.15, A.6.1 and A.6.9 (role labels for
   keys, "Lock out…", "Record training", "Entrustment decisions" in her menu). Their old Actuals describe the pages
   before flow 12; replace them as usual.
   - Flow 11's habits still hold: the mail's registration link is http://localhost:5180/account/register/start?token=…
     (open THAT; never /account/register?token=…); invitation Stub email lines are tagged `invitation` only, so match a
     link to its person by the address and the role words ("You are invited to Wombat as <role and scope>."); each
     admission sends a mail; Copy link needs the context's permissions ['clipboard-read', 'clipboard-write'] for
     http://localhost:5180 (README § The browser). Count stub mails from what the log shows.
   - Role changes and the lock now go through a note panel or a dialog: play the confirmation the step names, and
     check its words (the lock's dialog names what waits for the person).
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t376 scenario-t376-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t376: the flow 12 build made it wrong. Flow 12 covers Users, the user page (its sections, roles, scope,
  password, lock, reactivation, institutional sign-in, assessor training), Assessors, /admin/assessors/edit's
  redirect, her institution's record, SSO group mappings, and the Institutional admin's Home and menu.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T376 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 12 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t376-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\842d7157-aa59-4fb6-9f09-284891ff7f72\scratchpad\replay\pw`
  (flow 02's installed helpers, reused).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--Users-Renier-Wombat/842d7157-aa59-4fb6-9f09-284891ff7f72/scratchpad/replay/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\67ce8168-e9a2-4dd0-acb1-eab0ccee0868\scratchpad\g376`
  (a new `replay-pw.env` for this replay). T373's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\c239ab95-a81c-4974-9d82-6b782f1d9059\scratchpad\g373\act-*`:
  copy what helps into `<G>\<DIR>`, fixing paths, ids, database names and dates (compute every date from D), and
  rewriting the people-admin scripts (2.12-2.15, 2.44, A.4.5, A.6.4, A.6.7) for flow 12's pages (Add a role… in a note
  panel, the lock's dialog, Record training on the user page at ?section=training, not /admin/assessors/edit). Never
  copy g373's replay-pw.env or read it.
- **Flow 12's browser checks** (build-review.md, T376's notes and the round-3 check), played where their step is:
  - Narrow desktops (act 2, at 2.12 (Users, KGK's staff) and 2.15 (Assessors, five rows); the appendix at A.1.14): at
    901, 1000 and 1100 px wide, nothing scrolls sideways, and no word in Users' Name, Institution or Scope, or Assessors'
    Name, splits mid-word (an address breaks only after its @). Say what you measured.
  - At 390 (2.12, 2.13, 2.15, A.4.5, A.6.4, A.7.9): nothing scrolls sideways; Add a role…, the lock's and the Remove
    dialog's buttons, Record training and Reset are at least 44 px tall; a dialog's safe button is first in tab order,
    and the primary is on top on screen.
  - Focus: a result ("… added to …", "User locked out.", "Assessor training saved." or the page's words) has the focus
    after its action; a refusal's field has it; Escape in a dialog or the note panel returns it to the button that
    opened it; a rows-per-page change keeps it on the select (Users and Assessors).
  - One result at a time on the user page: Record training clears the page's result, and a page action clears the
    training section's.
  - Her own page (2.12) shows the Institutional sign-in section; devadmin's own (A.6.9) too.
  - Her menu has Entrustment decisions; Home has no Quick link for it.
  - "People 1 of 14 are locked out" (or whatever Home's waiting line says after A.6.4): quote what it reads.

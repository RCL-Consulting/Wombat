# T373 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/10-msf-campaign/replay-brief.md` (T369). Flow 11 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T373 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 11 of the restructure (onboarding: Invitations /admin/invitations (Delivery a state word with a
note row, not delivered first, "Last day", Revoke in a dialog, Expired, not used with Invite again), Invite a person
/admin/invitations/new (role radios, only the chosen role's fields, required marks, each refusal under its field, the
one-time link with Copy link), Register (the mail's link opens /account/register/start, which sets a cookie and lands on
/account/register with no token in the address; refusals under their field with the names kept; a card for each link
that cannot be used), the pending registrar's Home (the Awaiting admission card), Trainees /admin/trainees (Waiting to
be admitted, Current trainees with Programme start and Training year) and Admit <name> /admin/trainees/admit?userId=
(the curriculum read out, Programme start, the Training year, the result back on Trainees; the admission mail)), which
landed on master as 69a17c70 (one migration, T373_InvitationIssuedAsRole). Flows 01-10 landed earlier (b347e11c,
f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1, 49e7cf5e, 68327c9d).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t373, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t373.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t373 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t373_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t373 for that step.

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

The clock: D is the day your act is played (record it in your Actual lines; flow 11's replay starts 2026-10-20). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t373 <file.sql>
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
     Actual (<D>, T373 replay, wombat_scenario_t373): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 11 should fix, for example, the token left in
     the address bar (T315), a refused registration dropping the names (T317), the register half of T341, the
     invitations half of T264 (revoke and refused issue), the issue half of T286, the admission half of T306 (the
     training year), and the flow 11 halves of T323, T324, T280, T325, T328 and T329), write "Gap: none (T<n> no longer
     occurs: …)".
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 11 from the code, and no
   replay has seen it yet: Act 1's 1.5 and 1.7-1.11 and Act 2's 2.1-2.11, 2.16-2.19, 2.26-2.32 and 2.44, plus 2.13 and the
   Trainees wording in 5.16, 5.17, 5.27, 6.34, 6.36 and A.7.9. Their old Actuals describe the pages before flow 11;
   replace them as usual.
   - Registration links: the mail's link is http://localhost:5180/account/register/start?token=… ; open THAT (it sets
     the register cookie and redirects). Never open /account/register?token=…, which now shows "no invitation".
   - Invitation Stub email lines are tagged `invitation` only (no `role:` tag): match a link to its person by the
     address and the role words in the mail's text ("You are invited to Wombat as <role and scope>.").
   - Admissions now send a mail each ("You are admitted in Wombat"): stub counts after Act 2 are 5 higher than the old
     Actuals say. Count what the log shows.
   - Copy link: create the browser context with permissions ['clipboard-read', 'clipboard-write'] for origin
     http://localhost:5180 (README § The browser), or "Link copied." reads "Could not copy…".
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t373 scenario-t373-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t373: the flow 11 build made it wrong. Flow 11 covers Invitations and Invite a person (issue, resend,
  revoke, Invite again, the one-time link, the refusals), the invitation mail, /account/register/start and
  /account/register with its cards, the sign-out's return to Register, the pending registrar's Home, Trainees, Admit
  <name> and the admission mail, and the Users pages' invitation and admission lines.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T373 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 11 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t373-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\842d7157-aa59-4fb6-9f09-284891ff7f72\scratchpad\replay\pw`
  (flow 02's installed helpers, reused).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--Users-Renier-Wombat/842d7157-aa59-4fb6-9f09-284891ff7f72/scratchpad/replay/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\c239ab95-a81c-4974-9d82-6b782f1d9059\scratchpad\g373`
  (a new `replay-pw.env` for this replay). T369's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\c239ab95-a81c-4974-9d82-6b782f1d9059\scratchpad\g369\act-*`:
  copy what helps into `<G>\<DIR>`, fixing paths, ids, database names and dates (compute every date from D), and
  rewriting Act 1's and Act 2's invitation, registration and admission scripts for flow 11's pages (Invite a person at
  /admin/invitations/new, the start link, Admit <name> at /admin/trainees/admit?userId=). Never copy g369's
  replay-pw.env or read it.
- **Flow 11's browser checks** (build-review.md and the round-3 check), played where their step is:
  - Narrow desktops (act 2, at 2.7 (nine rows) and 2.29 (Current trainees)): at 901, 1000 and 1100 px wide, nothing
    scrolls sideways, and no word in Invitee, Delivery or Curriculum splits mid-word (an address breaks only after its
    @). Say what you measured.
  - At 390 (1.5, 2.7, 2.8, 2.17, 2.18, 2.29): nothing scrolls sideways; the header's Invite a person, the revoke dialog's
    two buttons and Admit's buttons are at least 44 px tall; the dialog's safe button ("Keep the invitation") is last on
    screen and first in tab order.
  - Focus: after an issue the result (the one-time link's region) has the focus; a refusal's field has it; Escape in
    the revoke dialog returns it to Revoke…; after Admit, the result on Trainees has it.
  - The token: after /account/register/start the address bar reads /account/register with no query, and the page
    source holds no token.
  - The one-time link is gone after any next action (2.6, 2.7, 2.17 show none).

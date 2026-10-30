# T387 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/13-graduation-and-exit/replay-brief.md` (T379). Flow 14 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T387 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 14 of the restructure (data rights: My data rights with its three sections, the four described
request types, Your requests with each row's answer in SAST, Withdraw, Download, and "Stop the regular reminder emails"
in place of the old preferences (no optional-processing box); the Data rights requests queue with Show, the requester's
name over the address, badges per status and the pseudonym over "Erased ‹day›" for an erased person; a request's page
titled "‹Type› request from ‹name›" with its decision note and counter, refusals under their field, "Decided by" a name,
the Correction section's fixed list of records (Account › Last name and the rest), "Record at least one correction
before completing the request.", Complete; the erasure's confirmation dialog, focused on Cancel, "Erase and approve",
the result sentence and the kept request; "Request unavailable" for a request that is not the caller's; the mails each
decision now sends (decided, rejected, being corrected, completed, and the erasure's own confirmation)), which landed on
master as e8eea1d2 (one migration, T387_DataRightsFlow14). Flows 01-13 and T312 landed earlier (b347e11c, f50dffb2,
725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1, 49e7cf5e, 68327c9d, 69a17c70, e53d4ae7, c9d6fe13,
5cc10c66).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t387, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t387.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t387 5180
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t387_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t387 for that step.

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

The clock: D is the day your act is played (record it in your Actual lines; flow 14's replay starts 2026-10-30). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t387 <file.sql>
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
     Actual (<D>, T387 replay, wombat_scenario_t387): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 14 should fix, for example, T316 (an approved
     rectification can never be applied or completed), T319 (a decision notifies nobody; a refused requester never sees
     the note), T320's digest help, T318 (an opt-out nothing reads), T324 (enum names on the pages), T264 (an erasure
     approved without a confirmation), T323, T328, T325 and T329's data-rights halves, T280, T276's request half and
     T112's wording; the runbook already says "closed by T387" where it expects them gone), write
     "Gap: none (T<n> no longer occurs: …)".
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 14 from the code, and no
   replay has seen it yet: A.1.1-A.1.14 (A.1.8a included), A.2.3-A.2.6, A.5.7, A.7.3, A.7.5 and A.7.14. Their old
   Actuals describe the pages before flow 14; replace them as usual.
   - Flow 11's, 12's and 13's habits still hold: the mail's registration link is
     http://localhost:5180/account/register/start?token=… (open THAT; never /account/register?token=…); invitation Stub
     email lines are tagged `invitation` only; each admission sends a mail; Copy link needs the context's permissions
     ['clipboard-read', 'clipboard-write'] for http://localhost:5180 (README § The browser). Role changes and the lock go
     through a note panel or a dialog. Count stub mails from what the log shows.
   - The end of a programme: Mark complete… and Deactivate… check the last day on the press, before any dialog; play
     the refusal the step names, then the dialog, and quote the forecast and the result's count as the page prints them.
   - Verify: type and paste hashes exactly as the step says (upper case, spaces); quote the answer's dates and counts.
   - Data rights (flow 14): every decision now sends a stub mail; count them from the log and quote each subject. The
     erasure is confirmed in a dialog ("Approve and erase…" opens it, "Erase and approve" confirms): note where the
     focus lands when it opens. A correction is recorded from the Correction section's fixed list, then Complete.
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t387 scenario-t387-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t387: the flow 14 build made it wrong. Flow 14 covers My data rights (requests, Your requests, the
  reminder emails setting), the Data rights requests queue, a request's page (decision, correction, Complete), the
  erasure's dialog and result, Request unavailable, the decision mails, and the shared confirmation dialog, whose action
  mode now puts the first focus on Cancel everywhere (so a dialog in any act whose first focus moved is flow 14's).
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T387 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 14 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t387-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\pw`
  (Playwright 1.56.1 and a rewritten `lib.mjs`, the same exports: flow 02's copy was lost with its temp folder).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--dev-Wombat/e81867ab-c351-40bf-9d41-d1d61fe30f43/scratchpad/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\g387`
  (a new `replay-pw.env` for this replay). T379's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\67ce8168-e9a2-4dd0-acb1-eab0ccee0868\scratchpad\g379\act-*`
  (each act's `h.mjs` holds the shared helpers: devPw, envPw, setEnvPw, shot, txt, go, the stub mail blocks): copy what
  helps into `<G>\<DIR>`, fixing paths (the lib.mjs import above), ids, database names and dates (compute every date
  from D), and rewriting the appendix's data-rights scripts for flow 14's pages. Never copy g379's replay-pw.env or read
  it.
- **Flow 14's browser checks**, played where their step is:
  - The erasure dialog (A.1.12) and every other action-mode confirmation an act plays: the first focus lands on Cancel,
    with a long forecast too; quote what the forecast lists.
  - The queue (A.1.3, A.1.8, A.1.12): the page-size select keeps the focus after a change; Show filters; at the defaults
    there is no Clear filters.
  - A request's page (A.1.3, A.1.8, A.1.8a, A.1.12): the decision note's counter counts while typing; a refusal sits
    under its field with the focus; no rule under a stacked row header at 900 px and below.
  - At 390, 768 and 1280 (A.7.3, A.7.5; A.1.1 and A.1.3 at 1280): nothing scrolls sideways; controls at least 44 px at
    390; Your requests stacks below 900 px and Download is full width below 641 px; the queue stacks from 1100 px.
  - The mails (A.1.3, A.1.8, A.1.8a, A.1.12, A.1.13): quote each Stub email's subject and recipient; the erasure's goes
    to the erased person's old address, with no link, after the commit.
  - My data rights' answers (A.1.5, A.1.6, A.1.9): quote each row's answer line as the page prints it.

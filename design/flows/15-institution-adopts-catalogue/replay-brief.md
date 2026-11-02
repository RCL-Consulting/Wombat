# T392 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/14-data-rights/replay-brief.md` (T387). Flow 15 runs the same prompts as sequential agents, one per act, then one for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in.

```text
You are playing part of T392 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It
is played against flow 15 of the restructure (an institution adopts the College's catalogue: Curricula carries adoption
by rows, "Curricula you train against" with each version's state (Adopted on, Superseded on, registrars still on it)
and "Open for adoption" with Adopt, a superseding adoption asking first in a dialog that counts the registrars left on
the old version and names the own items not carried over; the Administrator's institution section, chosen and read with
Show; /admin/adoptions redirecting to Curricula, and Curriculum adoptions gone from every menu; a curriculum's items as
a list titled "Items of ‹curriculum›", with Edit, Remove… and View per row, and an item page of its own to add, edit, or
read a national item as a record, a save or add returning to the list with a one-shot result token the page strips
from the address; the EPA list naming each EPA's owner, an EPA's page titled by the EPA ("KGK-001 — …"), a national EPA
a read-only record to an institution, Deactivate… asking first in words that fit a national or a local EPA, and no
Create EPA before anything is adopted; the trainee profile's move line under the picker and "Save profile and move…"
with its dialog; the rebuild's words; the scales' reader subtitle; refusals under their field; new and cloned versions
start held back), which landed on master as c931d56f (no migration). Flows 01-14 and T312 landed earlier (b347e11c,
f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1, 49e7cf5e, 68327c9d, 69a17c70, e53d4ae7, c9d6fe13,
5cc10c66, e8eea1d2).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t392, published from master. The integrator started it
  with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t392.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t392 5180
  If stop prints "Nothing listens on 5180" (or 5181) while the app answers, stop the process listening on that port by
  its id, after checking its command line (BRIEF § 11, flow 14).
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t392_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t392 for that step.

Read first:
- execution/knowledge/scenario-paediatrics/README.md: How to play, The step format, and the Screenshots rule.
- Then your act file, in full.

Browser:
- The Playwright library is installed at <PW>\node_modules (v1.56.1). Its helpers are in <PW>\lib.mjs: launch, login and
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

The clock: D is the day your act is played (record it in your Actual lines; flow 15's replay starts 2026-11-02). J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t392 <file.sql>
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
     Actual (<D>, T392 replay, wombat_scenario_t392): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 15 should fix, for example, T291's items 1 and 4
     and its 1.19 widening (Edit offered on national EPAs, KGK-001 unmarked, Create EPA before any adoption, the Demo
     EPA-001 opening as a form to KGK), T301 (a held-back version adoptable by request; a new version not held back),
     T304's remainder (a move saved with no question first), T326's scales half and the catalogue subtitles (one
     subtitle for both roles), T323's item-editor half (the edit row scrolling sideways at 390 px, A.7.10), T329's
     catalogue pages (a failed read or save printing an exception's text), T272 (CreateEpa's every failure read as a
     duplicate code; the rebuild printing the exception) and T325's AdoptedOn half; the runbook already says "closed by
     T392" where it expects them gone), write "Gap: none (T<n> no longer occurs: …)". T291's items 2, 3, 6 and 8,
     T264's scale Delete (6.8), T390 (a pager hidden when its size holds every row) and T393 (the curriculum and scale
     edit pages print an exception's text) are other flows' or open: keep citing them where they still fail.
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 15 from the code, and no
   replay has seen it yet: 1.10, 1.17-1.22, 2.44, 6.9, 6.13, 6.17, 6.22-6.26, 6.29-6.38, the Act 6 Outcome state, A.5.2,
   A.7.9 and A.7.10 (1.21 now presses View on PAED-001). Their old Actuals are T387's and predate flow 15 (their Gap
   lines say "The T387 Actual above predates T392"); replace them as usual.
   - Flow 11's to 14's habits still hold: the mail's registration link is
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
   - The catalogue (flow 15): adoption is on Curricula (Open for adoption; a superseding Adopt… asks first: quote the
     dialog's registrar count and words). An item is added, edited or read on its own page
     (/admin/curricula/{id}/items/new, …/items/{itemId}); a save or an add returns to the list with a one-shot token
     (?added=, ?saved=, ?back=, and ?created on a new EPA's page): quote the result, say where the focus is, check the
     page took the token out of the address, then reload and say the result is gone. A refusal sits under its field
     with the focus. Every confirmation dialog an act plays opens with the focus on Cancel: say so. Find EPAs,
     curricula and items by code and version, never by a remembered id.
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t392 scenario-t392-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t392: the flow 15 build made it wrong. Flow 15 covers Curricula (the institution's versions, Open for
  adoption, the adopt dialog and result, the Administrator's institution section and Show), the /admin/adoptions
  redirect and every menu's Curriculum group, a curriculum's items list and the item page (add, edit, record, Remove…,
  the result tokens), the EPA list and an EPA's page (owner, record, create, Deactivate…, refusals under the field),
  the curriculum page's held-back start and clone, the trainee profile's curriculum picker and move dialog, the
  progress rebuild page, the entrustment scales list's subtitle, and Home's and Invite's and Admit's adoption links.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T392 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 15 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual.
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t392-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\pw`
  (Playwright 1.56.1 and `lib.mjs`: launch, login, logout, waitBlazor and BASE; flow 14's session's copy, which flow 14's
  replay proved).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--dev-Wombat/e81867ab-c351-40bf-9d41-d1d61fe30f43/scratchpad/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\ecc5a9d5-2a6e-43d8-a55a-e55b7e48f8ab\scratchpad\g392`
  (a new, empty `replay-pw.env` for this replay). T387's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\g387\act-*`
  (each act's `h.mjs`, or `common.mjs` in acts 4 and 5, holds the shared helpers: devPw, envPw, setEnvPw, shot, txt, go,
  the stub mail blocks): copy what helps into `<G>\<DIR>`, fixing paths (the lib.mjs import above, `<G>` for the
  password file), ids, database names (wombat_scenario_t392) and dates (compute every date from D), and rewriting the
  catalogue scripts (acts 1 and 6, A.7.10) for flow 15's pages: adoption on Curricula, the item page, the EPA record.
  Never copy g387's replay-pw.env or read it.
- **Flow 15's browser checks**, played where their step is:
  - The focus after each action: the adoption's result (1.20, 6.32), the EPA's create (6.25, `?created`), the item's
    add and save (6.26, 6.30, 6.33, `?added=`, `?saved=`), Back from an item (1.21, `?back=`: the focus on that row),
    Deactivate… and its result (6.17, 6.23), the move (6.36), the rebuild (6.38). Check the focus by keyboard where a
    scripted change says it moved (BRIEF § 11, flow 14).
  - The one-shot tokens (1.21, 6.25, 6.26, 6.30, 6.33): the page strips the token from the address; a reload then says
    nothing of the result.
  - Every confirmation dialog (6.17 Deactivate…, 6.26's or 6.30's Remove… if played, 6.32 Adopt…, 6.36 Save profile
    and move…, 6.38 the rebuild): it opens with the focus on Cancel; quote its words and counts; Escape returns the
    focus to its trigger.
  - The items list at 390, 768, 901 and 1280 (A.7.10, and 1.18 or 6.30 at 1280): nothing scrolls sideways, the table
    stacks below its breakpoint, and the controls are at least 44 px at 390 (A.7.9, A.7.10).
  - No national EPA offers Edit to KGK (1.21, 6.22, 6.25), and a national item offers View, opening its read-only record
    (1.21); KGK-001 is marked as KGK's own and is the only row she may change (6.25).
  - The menus (1.10, A.5.2, A.7.9): no Curriculum adoptions; `/admin/adoptions` typed lands on Curricula.
  - The held-back start (6.13, 6.29, 6.31): a new or cloned version starts held back and is not offered for adoption
    until published.

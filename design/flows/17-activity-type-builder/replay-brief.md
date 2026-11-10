# T398 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/16-college-catalogue/replay-brief.md` (T394).
Flow 17 runs the same prompts as sequential agents, one per act (1, 2, 3, 4, 5, 6, A), then two for the states. The text
below is what each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in, and the integrator's header line
`D = 2026-11-10` (the replay's date) above it. What flow 17 changed, step by step, and the states to re-take follow the
text; each act's prompt carries its own part of the first. The filled prompts are in `<G>` (below) as `act-1.txt` …
`act-A.txt`, `states-1.txt` and `states-2.txt`.

```text
You are playing part of T398 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It is
played against flow 17 of the restructure (build and publish an activity type: the list searched on Show, with Clear
and a count line, Set by in words (the College's short code or the institution's name over the scope), Published
"Version n" and Draft "None" or "Saved D", the writer's own rows first and then one run by name, Edit or View named for
its row, the release lines on msf_cpsa and learner_feedback_cpsa, one button's Actions column, no match apart from the
empty catalogue, the rows stacking to 1024 px; the builder's real tabs (Details, Form, Workflow, Credit and, for a
writer, Publish; the arrow keys, Home and End move between them, Tab leaves them), a new type opening on Details, Scope
a read-out or a radio group, the owner line and the Versions strip, a section's or field's editor in place under its
row, a refused save's summary above the tabs with each line at its place and the tab's count ("Form 1 problem"), the
workflow and credit rules read in words above their JSON, the JSON laid out by Wombat after each save, Publish
unavailable with its reason as text, the Publish tab, Publish asking only on warnings (Q16) and keeping its tab (Q20),
Discard draft… asking first in two bodies (Q14), unticking Active on a published type asking first (Q15, Q17), a
system-managed type written by its release alone (T334, Q9; F1: Discard stays on its leftover draft), Unavailable for an
out-of-scope or unknown id, the preview's EPAs by the reader's scope; the tab bar and a count's word laid out by the
page's column, the moves table stacking to 1024 px), which landed on master as 84bcffeb (no migration). Flows 01-16 and
T312 landed earlier (b347e11c, f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1, 49e7cf5e,
68327c9d, 69a17c70, e53d4ae7, c9d6fe13, 5cc10c66, e8eea1d2, c931d56f, de4cc718 with its column fixes a2878763 to
2c93b247).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub
  remote; the integrator commits and pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t398, published from master at 399a99be or later. The
  integrator started it with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t398.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t398 5180
  If stop prints "Nothing listens on 5180" (or 5181) while the app answers, stop the process listening on that port by
  its id, after checking its command line (BRIEF § 11, flow 14; T391).
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database, nor
  wombat_scenario_t398_sweep (the build sweep's, kept).
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t398_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t398 for that step.

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
- The default viewport is 1280x800; a step that says 390 px uses 390x844.

Accounts and passwords:
- Dev accounts (devadmin@wombat.local and the others): their dev-only passwords are constants in
  src/Wombat.Infrastructure/Identity/DevUserSeeder.cs. Read them in your script; never print them.
- Cast accounts registered during this replay:
  - Choose a strong password at replay time: 12+ characters, 4+ different characters, a digit, an upper-case and a
    lower-case letter, and a symbol.
  - Keep it ONLY in <G>\replay-pw.env as KEY=value lines (WB_PW_<SURNAME>=...), which later acts read into their
    scripts' environment. This is a new file for this replay; never read any other replay's password file (g394's,
    g392's or older).
  - A step that changes a cast member's password loses no value: it first copies the old one to
    WB_PW_<SURNAME>_<ACT> (the act that changed it), then sets WB_PW_<SURNAME> to the new one, so WB_PW_<SURNAME> is
    always current and the earlier snapshots can still be signed in to (BRIEF § 11, flow 03). A state capture on a
    snapshot taken before the change uses the _<ACT> key.
  - Never print a password to the console or into your reply, and never write one into the runbook or any tracked file.
  - Never open pwd_DO_NOT_COMMIT.txt or recovery\.
- Registration and MSF links come from the log's Stub email lines. Never write a token into the runbook.

The clock: D is the replay's date, given in this prompt's header (D = 2026-11-10 unless the integrator changed it).
Record it in your Actual lines and compute every date from it. J is the latest 15 January on or before D. Dates in steps
are relative. If your act runs past 00:00 SAST (UTC+2; the machine clock is UTC), keep the header's D for what you typed
before it, and say in your notes which steps were played after midnight and what D they read (BRIEF § 11, flow 13).

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t398 <file.sql>
- If a step's Note prescribes an UPDATE (ageing a row) and the session refuses to run it, mark that step not-played,
  citing T337, and play on.

A step that waits a real hour (acts 2 and 3) waits in short background waits (a few minutes each), playing
read-only steps meanwhile and saying so; never one long wait.

Write your Actual and Gap lines into the act file as you go, at the end of each phase, not only at the end of the act:
a replay can be killed part-way (BRIEF § 11, flow 03), and the lines already written are what survives. Every script
writes what it saw to a file under <G>\<DIR>\out\ as it goes (one file a step: the words, the focus, the measures).
If your act file already holds "T398 replay" Actual lines when you start, an earlier run was interrupted: do not replay
those steps. Write any step the out\ files show as played but the act file lacks from those files, without playing it
again; read the database to find where the state stands; and play on from the first step not yet played. Record what
only the lost run saw (a result's words, where the focus went) as not seen (BRIEF § 11, flow 15).

Your remit is EVERY step of your act, setup included: a step named elsewhere in this prompt is the flow's step inside
your act, never a limit (BRIEF § 11, flows 08 and 09).

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows, and against the log or SQL where the Expect says so.
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (<D>, T398 replay, wombat_scenario_t398): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 17 should fix T271 (the field view naming the
     scale by its seed key, F-1.25b), T291's items 3 and 6 (no "Draft saved." after the first save, F-1.26c and
     F-6.14aa; the preview offering the Demo EPA-001, F-1.27a), T324's builder half (types and roles by enum name,
     F-1.27b), T326's search (a search that matches nothing read as the empty catalogue, F-1.31b), T334 (a
     system-managed type writable in the builder, F-6.14ab) and T280 (the builder's tabs as plain buttons, F-A.7.9a);
     the runbook already says "closed by T398" where it expects them gone), write "Gap: none (T<n> no longer occurs:
     …)". T291's items 7 and 8, T390 (a pager hidden when its size holds every row), T396 (the Q16 and Q19 guards
     check, then save) and T399 (pages outside the builder print an exception's text through RefusalText.Of) are other
     flows' or open: keep citing them where they still fail.
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 17 from the code, and no
   replay has seen it yet: 1.24-1.31, 6.14a, A.7.9 and A.7.14. Their old Actuals are T394's and predate flow 17 (their
   Gap lines say "The T394 Actual above predates T398"); replace them as usual. Several Routes changed: 1.25 leaves by
   the trail's Activity types and 1.31 comes back by it (no Back to list); 6.14a goes back to the list by the trail.
   - Flow 11's to 16's habits still hold: the mail's registration link is
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
     region, focused. Every confirmation dialog opens with the focus on Cancel: say so. Quote the reach sentences as the
     page prints them and say where a count differs from the step's. Find Colleges by short code, specialities,
     sub-specialities and scales by name, EPAs, curricula and items by code and version, never by a remembered id.
   - Every page with header actions keeps them beside the h1 from 1024 px (Q20).
   - The activity types (flow 17): find a type by its Key or Name (SQL: "ActivityTypes"."Key"), never by a remembered
     id. The list's search applies on Show only: typing alone changes nothing; quote the count line ("12 of 22 activity
     types match “cpsa”.") and say it took the focus; Clear puts the focus back in the search. The builder's results
     ("Draft saved.", "Published version 1.") sit above the tabs and take the focus, with no token in the address; a
     tab press does not clear them. A refused save's summary ("Nothing was saved." and "Everything you typed is kept.")
     takes the focus; quote its line, the words at the place (under the field or the JSON), and the tab's count
     ("Form 1 problem"), and check the place's input carries aria-invalid and aria-describedby naming those words.
     Pressing the summary's line opens the place (its editor, its key focused). Read the tabs by keyboard on one reader
     page (1.25) and one writer page (1.27): role "tablist"/"tab", aria-selected on the selected one only, ArrowLeft,
     ArrowRight, Home and End move between them, Tab leaves the bar (it must not trap the keyboard). Publish unavailable
     is aria-disabled (not disabled), still in the tab order, with its reason as text beside it.
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t398 scenario-t398-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t398: the flow 17 build made it wrong. Flow 17 covers /admin/activity-types (the subtitle by role, the
  search on Show with Clear and its count line, Set by, Published and Draft, the order, Edit or View named for its row,
  the release lines, no match apart from the empty catalogue, the Actions column, the stacking to 1024 px) and the
  builder at /admin/activity-types/new and /admin/activity-types/{id} (the tabs and their keyboard, Details first,
  Scope, the owner line and the Versions strip, the editors in place, the refusal summary with its places and the
  tab's count, the read in words and the JSON laid out, Publish unavailable with its reason, the Publish tab, the
  Publish, Discard and Active questions and their results, the system-managed notice, Unavailable, the preview's EPAs
  and its person-picker line, the tab bar and the moves table by width), and flow 03's filing page only as far as it
  reads a type's editor (one line in NewActivity).
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T398 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 17 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual. Wherever it first shows the activity-type list or a builder tab at 1280, look at the capture: a name
squeezed to a few words a line beside empty columns, or a column of one-word pieces, is a finding even though nothing
scrolls (BRIEF § 11, flows 15 and 16).
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t398-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know (ids,
the stub mail count, password keys added or changed, by name only), which captures show a registration link, and
anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\pw`
  (Playwright 1.56.1 and `lib.mjs`: launch, login, logout, waitBlazor and BASE; flows 14–16's replays used it, and it
  was there on 2026-11-10).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--dev-Wombat/e81867ab-c351-40bf-9d41-d1d61fe30f43/scratchpad/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\g398`
  (a new, empty `replay-pw.env` for this replay). T394's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\28969ed0-c979-487c-82e2-606d8331c3a3\scratchpad\g394\act-*`
  (each act's `h.mjs`, or `common.mjs`, holds the shared helpers: devPw, envPw, setEnvPw, shot, txt, go, the stub mail
  blocks): copy what helps into `<G>\<DIR>`, fixing paths (the lib.mjs import above, `<G>` for the password file), ids,
  database names (wombat_scenario_t398) and dates (compute every date from D), and rewriting the activity-type scripts
  (1.24–1.31, 6.14a, A.7.9, A.7.14) for flow 17's pages: the search on Show, the tabs as tabs, the editors in place,
  the summary and its places, the read in words, the trail in place of Back to list. Never copy g394's replay-pw.env or
  read it.
- **Flow 17's browser checks**, played where their step is:
  - The focus after each action: the count line after Show (1.24, 1.31) and the search after Clear (1.24); "Draft
    saved." on Details at the type's address with no token (1.26, 6.14a); each Edit's and Add field's editor with its
    key focused (1.27); the refused saves' summary (1.27, 1.28) and the summary's line opening Audience's editor, Field
    key focused (1.27); "Draft saved." after each corrected save (1.27, 1.28); "Published version 1." with Credit still
    selected, and still there after Details is pressed (1.30). Check the focus by keyboard where a scripted change says
    it moved (BRIEF § 11, flow 14).
  - The tabs (1.25 as a reader, four; 1.27 as a writer, five; A.7.9 at 390): role "tablist" and "tab", aria-selected,
    aria-controls naming a tabpanel; the arrow keys, Home and End move the selection; Tab from the selected tab leaves
    the bar for the panel or the next control (no trap). A refused tab's count is part of the tab's name ("Form 1
    problem"); below 46rem of column the word is visually hidden, the number shown.
  - The places (1.27, 1.28): Topic's and Audience's Field key inputs, and the Workflow JSON, carry aria-invalid and
    aria-describedby naming the words at the place; after the corrected save they carry neither.
  - The read in words (1.25, 1.28, 1.29): the workflow's states and "Moves, as saved", the credit rule's sentence, as
    the step quotes them; a reader sees no JSON (Q2); a writer's JSON is laid out one property a line after the save
    (Q13). A stored jsonb is never compared with the page's text: Postgres re-renders it (CLAUDE.md).
  - Unavailable, never Access denied (6.14a: Prof Mbatha on Dr Kruger's draft's address), no id shown, Activity types
    lit.
  - No Back to list anywhere; the trail's Activity types is the way back (1.25, 1.31, 6.14a).
  - **The list's sweep** (sweep.md; the fix pass's single-button Actions column was inferred, not measured): at 1.24
    and 1.31 as Prof Mbatha (22, then 23 rows) and at 6.14a as Dr Kruger (22, then 23): every 25 px from 900 to 1100
    plus 1025, each `.table-container`'s table `scrollWidth` against its `clientWidth`, and the page's own; record the
    smallest spare (sweep.md said 40-55 px before the fix, about 170-190 px expected after it). Then every column at
    1280 (sweep.md's table: width, most lines in a cell, the name's lines, one-word pieces broken, a band beside the
    widest line): the Actions column about 92 px, Set by's scope line and "Not published" / "Saved D" back to one line
    on most rows, no name past two lines. A table wider than its container, or a column of one-word pieces, is a
    regression.
  - **The builder's sweep**, at 900 to 1150 in 25 px steps plus 1115 and 1116, the card's and each `.table-container`'s
    `scrollWidth` against its `clientWidth`, the tab bar one row, each field row's buttons on one line beside or under
    a whole label (never over it), the page unscrolled: in act 1 on Mini-CEX (Paediatrics) as a reader, all four tabs
    (Details and Credit were never swept), and on KGK Teaching Session Log as a writer: Form with the real "Form 1
    problem" refusal standing (1.27), Workflow with "Workflow 1 problem" (1.28), Credit (1.29), and Details and Publish
    after the publish (1.30); also at 641 and 768 with each real refusal standing (the tab bar one row, nothing over).
    In act 6 at 6.14a as Dr Kruger: Multi-Source Feedback (Paediatrics) read-only (four tabs) and the new type's Details
    with Scope's radios. Record the smallest spare per table in the step's Actual.
  - At 390 (A.7.9): one row of five tabs, equal tracks, the selected one exposed; Show's choice of "Edit the form" and
    "Preview"; every control 44 px; nothing scrolls sideways.
  - Contrast (A.7.14): Publish's reason, muted text, 4.5:1 or more on its background; white on Publish 5.88:1 though it
    is aria-disabled (exempt).
  - One states capture only act 1 can take, since no snapshot holds KGK's unpublished draft: after 1.29, before 1.30's
    Publish, open /admin/activity-types in a second page of the same context (a read; change nothing), capture it at
    1280 from the top as design\baseline\states\activity-types-list--draft.png, check it against that `states.md` row
    ("the KGK row first, Published "Not published", Draft "Saved `D`"") and say so in 1.29's Actual.
- **For the integrator, after the acts:** look at one 1280 capture of the list for each of Prof Mbatha (1.31) and Dr
  Kruger (6.14a), and one of each builder tab (1.25, 1.27-1.30), before the states pass (BRIEF § 11, flows 15 and 16). A
  width fix found mid-replay waits until the acts using the published app are done; republish between the states
  passes, read every column of the list at 1280 for each role (an institution, the College, the Administrator), sweep
  390–1440, and re-take the captures the fix changes (BRIEF § 11, flow 16).

## What flow 17 changed, step by step

Each changed step's Gap line says "The T394 Actual above predates T398". Acts 2 to 5 hold no changed step: they see flow
17 only in the types a registrar is offered (2.42) and KGK's teaching log filed at 3.18 and 3.19, from the type 1.26–1.30
built: flow 03's filing page and summary are unchanged by the build.

### Act 1
- **1.24** The list (Prof Mbatha): the subtitle "The forms registrars file and assessors complete. You can change the
  types you set; the College sets its instruments." (Q12); `cpsa` and Show, "12 of 22 activity types match “cpsa”."
  focused (Q7); Set by "CPSA" over "Speciality · Paediatrics", Published "Version 1", Draft "None", Active, View named
  for its row; Clear back to "22 activity types.", the focus in the search; the Demo rows "DEMO-C" over "Speciality ·
  General Medicine"; the two release lines (Q9).
- **1.25** Mini-CEX (Paediatrics) read-only: the owner line "CPSA · Speciality · Paediatrics", four tabs opening on
  Form, T300's notice, the strip ("Version 1, `D`, Wombat's catalogue"); Form settings and rows by label (T19, T324);
  `overall_level`'s scale by its name (T271); the preview's 15 PAED EPAs (S8) and the person picker's line; Details and
  Versions as text; Workflow and Credit in words, no JSON (Q2); leave by the trail.
- **1.26** New activity type opens on Details (Q1) with Q14's line and the required line, Scope a read-out; the default
  draft; Save draft to the type's address, "Draft saved." focused (T291 item 3), the strip "Saved `D` by Nolwazi
  Mbatha", the header Save draft, Publish and Discard draft….
- **1.27** The editors in place under their row, key focused; Add field's editor opened at once (Q4, S13); the types by
  label; the User field's Names a; the duplicate key refused: the summary, its "Form:" line, the words at Topic and at
  Audience, "Form 1 problem" (Q6, M2); the line opening Audience's editor; "Draft saved."; the settings by label; the
  preview's EPAs.
- **1.28** The `field:` rule refused: the summary's "Workflow, move Cancel:" line, the words under the JSON, "Workflow 1
  problem"; "Draft saved."; the JSON laid out (Q13); the workflow in words.
- **1.29** "Credits nothing." and its sentence above the JSON laid out.
- **1.30** Publish from Credit: "Published version 1." focused, Credit kept (Q20); Discard draft… gone; Publish
  unavailable with "Save a draft to publish." as text (S5); Details' Key a read-out; the Publish tab's words and
  Versions; the result kept across a tab press.
- **1.31** By the trail: "23 activity types.", her own row first with Edit, the rest in one run by name (Q11); `KGK`
  and Show, "1 of 23 activity types match “KGK”." focused; the Note's `zzz` no-match, apart from the empty catalogue
  (T326).

### Act 6
- **6.14a** Dr Kruger's list: the College's subtitle, "22 activity types.", his ten rows first with Edit, msf_cpsa and
  learner_feedback_cpsa View with their release lines (T334, Q9), the Demo rows View; Multi-Source Feedback
  (Paediatrics) read-only under the release's notice, no Save draft, Discard draft… or Publish; New activity type on
  Details, Scope a required radio group (Speciality, Sub-speciality: `Paediatrics / Neonatology` and `Paediatrics /
  Paediatrics`), the owner line following it; Save draft, "Draft saved." focused, the strip "Saved `D` by Anton Kruger";
  his list "23 activity types."; Prof Mbatha's list still 23 without it, and its address Unavailable (R2-B09).

### Appendix
- **A.7.9** At 390, the builder: one row of five tabs, the selected one exposed (T280), Show's choice of "Edit the form"
  and "Preview" (Q5), every control 44 px.
- **A.7.14** Contrast: Publish unavailable (aria-disabled, exempt) and its reason as text, muted, 4.5:1.

## Flow 17's states to re-take

Every `states.md` row of the two pages (`:902`–`:936`, 35 rows): the 19 lane W rewrote and the 16 marked "Not yet
captured (T398)" (\*), in two passes after the appendix. Pass 1 takes the list and the reads (the rows reached at a
step, read-only, or by holding a read, and the two scratch Colleges'); pass 2 the builder's writes (the saves, the
refusals, the questions and their results, on scratch copies of the appendix's end).

**Pass 1** (16 rows):
- `activity-types-list--search`, `--draft` (act 1 takes it between 1.29 and 1.30; check it), `--all`, `--no-match`, `--loading`, `--narrow`, `--empty`\*,
  `--no-speciality`\*, `--load-error`\*.
- `activity-type-edit--no-scope`\* (the same scratch as `--no-speciality`), `--college-instrument`, `--not-found`,
  `--loading`, `--narrow`, `--unavailable`\*, `--load-error`\*.
- Also measured, no capture: the list's every column at 1280 and its 900–1100 sweep as devadmin (24 rows), and the
  builder at 900–1150 as devadmin on KGK Teaching Session Log and Mini-CEX (Paediatrics) (never swept; read-only on
  :5180).

**Pass 2** (19 rows):
- `activity-type-edit--new`, `--metadata`, `--user-field`, `--duplicate-key`, `--field-rule`, `--credit`,
  `--published` (each "At Step 1.2x": reuse act 1's capture where it shows the row's state exactly).
- `activity-type-edit--publish-warnings`, `--discarded`, `--key-taken`\*, `--details-refused`\*, `--section-refused`\*,
  `--publish-dialog`\*, `--discard-dialog`\*, `--discard-never-published`\*, `--deactivate-dialog`\* (with
  `--deactivated` and `--reactivated`), `--deactivate-publish`\*, `--active-only-draft`\* (the Active-only save, build
  review N6), `--release-draft`\* (F1: a draft staged on `msf_cpsa` by SQL).
- Checks the build left: a section refusal's key and title inputs carry aria-invalid and aria-describedby naming the
  words in the section's head (pages review S2); a real refusal's count at 641, 768 and 900–1150.

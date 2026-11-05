# T394 step 7 — the replay brief (one agent per act, in order)

Adapted from `design/flows/15-institution-adopts-catalogue/replay-brief.md` (T392).
Flow 16 runs the same prompts as sequential agents, one per act, then two for the states. The text below is what
each act's agent is given, with `<ACT>`, `<FILE>` and `<DIR>` filled in, and the integrator's header line
`D = 2026-11-05` (the replay's date) above it. What flow 16 changed, step by step, and the states to re-take follow
the text; each act's prompt carries its own part of the first.

```text
You are playing part of T394 step 7: the acceptance replay of Wombat's whole scenario runbook on a fresh database. It is
played against flow 16 of the restructure (the College keeps the national catalogue: one Specialities page per College,
a tree with a section per speciality, its sub-specialities and their default ladders, the default chosen at create, the
old sub-specialities address redirecting to its section; Colleges, a College's record, a speciality's and a
sub-speciality's pages titled by the record, with refusals under the field naming the holder, Deactivate… asking first
(D56) and no Back links; the College admin reading the entrustment scales list (D57), a scale's page with Delete… asking
first, Q18's refusals under the rung, and Up, Down and Remove only on a scale nothing uses (Q16, T395); a curriculum's
page in one column with Open for adoption, Hold back…, the clone and their one-shot results, its sub-speciality fixed
once it holds an item or has an adoption (Q19); the reach, how far a change goes, said in the items' warning, an item
page's subtitle, an EPA's standing line and its pause dialog (Q2, EV2); the College's EPA list without a College column;
Home's card with four links; header actions beside the h1 from 1024 px on every page (Q20); the catalogue tables
stacking from 1024 px, Curricula and the items from 1100 px), which landed on master as de4cc718 (no migration). Flows
01-15 and T312 landed earlier (b347e11c, f50dffb2, 725237ee, 06aa51d7, b020c942, 85d5a508, 44de180b, 622c5ad1, 49e7cf5e,
68327c9d, 69a17c70, e53d4ae7, c9d6fe13, 5cc10c66, e8eea1d2, c931d56f).
- The repo is C:\dev\Wombat on branch master. Do not switch branches, do not commit, do not push (the repo has a GitHub remote; the integrator pushes).

The replay app is ALREADY RUNNING.
- It is at http://localhost:5180, on database wombat_scenario_t394, published from master at de4cc718 or later. The
  integrator started it with tools\scenario-replay.ps1.
- It runs in Development. Mail is written to the log C:\dev\Wombat\.scenario-app\wombat_scenario_t394.log as
  "Stub email" lines, and emailed links point at :5180.
- The acts before yours have been played on it, so its state is theirs.
- If the app has died (no answer on :5180), restart it with:
  powershell -File tools\scenario-replay.ps1 start wombat_scenario_t394 5180
  If stop prints "Nothing listens on 5180" (or 5181) while the app answers, stop the process listening on that port by
  its id, after checking its command line (BRIEF § 11, flow 14).
- Never publish again, never create or drop a database, and never touch port 5080 (the dev app) or its database.
  One exception: Step A.5.15 plays on a scratch database. Restore the current state into it with
  powershell -File tools\scenario-replay.ps1 restore <your dump> wombat_scenario_t394_s515, start it on port 5181,
  play the step there, then stop 5181; never change wombat_scenario_t394 for that step.

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

The clock: D is the replay's date, given in this prompt's header (D = 2026-11-05 unless the integrator changed it).
Record it in your Actual lines and compute every date from it. J is the latest 15 January on or before D. Dates in steps
are relative. If your act runs past 00:00 SAST (UTC+2; the machine clock is UTC), keep the header's D for what you typed
before it, and say in your notes which steps were played after midnight and what D they read (BRIEF § 11, flow 13).

SQL checks:
- Write a .sql file under your tmp dir and run:
  powershell -File tools\scenario-replay.ps1 sql wombat_scenario_t394 <file.sql>
- If a step's Note prescribes an UPDATE (ageing a row) and the session refuses to run it, mark that step not-played,
  citing T337, and play on.

A step that waits a real hour (acts 2 and 3) waits in short background waits (a few minutes each), playing
read-only steps meanwhile and saying so; never one long wait.

Write your Actual and Gap lines into the act file as you go, at the end of each phase, not only at the end of the act:
a replay can be killed part-way (BRIEF § 11, flow 03), and the lines already written are what survives. Every script
writes what it saw to a file under <G>\<DIR>\out\ as it goes (one file a step: the words, the focus, the measures).
If your act file already holds "T394 replay" Actual lines when you start, an earlier run was interrupted: do not replay
those steps. Write any step the out\ files show as played but the act file lacks from those files, without playing it
again; read the database to find where the state stands; and play on from the first step not yet played. Record what
only the lost run saw (a result's words, where the focus went) as not seen (BRIEF § 11, flow 15).

Your remit is EVERY step of your act, setup included: a step named elsewhere in this prompt is the flow's step inside
your act, never a limit (BRIEF § 11, flows 08 and 09).

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows, and against the log or SQL where the Expect says so.
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (<D>, T394 replay, wombat_scenario_t394): <what you saw, concise; quote on-screen words where the
       Expect does>
   Replace its Gap line with "Gap: none" or the finding.
   - If the Expect fails because of a still-open task the old Gap line cites, keep citing that task (for example
     "Gap: F-2.34a, T317 (still: ...)").
   - If a task the old Gap line cites is now fixed by what you see (flow 16 should fix, for example, T291's item 2 (Back
     to colleges offered to the College, F-1.12a, F-6.11a), T324 on the College's pages (DataAnnotations' "The
     ShortCode field is required.", F-6.1a), T264 on the scales (Delete with no question, F-6.8a), T326's Neonatology
     empty state (F-6.13a), T393 (the curriculum and scale pages printing an exception's text), T272's College and
     speciality handlers (every refused save read as a duplicate), T301's "Active" box on a curriculum (now Open for
     adoption) and T331 (the menu lighting nothing on the College's specialities); the runbook already says "closed by
     T394" where it expects them gone), write "Gap: none (T<n> no longer occurs: …)". T291's items 3, 5, 6, 7 and 8,
     T390 (a pager hidden when its size holds every row) and T396 (the Q16 and Q19 guards check, then save) are other
     flows' or open: keep citing them where they still fail.
   - Where a step's Expect carries a bracket ([#n], [n] items, review #N, "As of hh:mm SAST"), fill it from what the
     app shows in your Actual line; a bracket is not a failure.
   - For a new finding write "Gap: new: <one sentence>".
   - Keep the file's style: lines under 120 characters, continuations indented two spaces.
   - Play every claim, restarts included: a restart (stop, then start) keeps the old log beside the new one, so it
     loses nothing. Copied scripts carry stale paths and date helpers; compute every date from D.
4. Do NOT edit Role, Route, Do, Expect or Note lines. The runbook was just rewritten for flow 16 from the code, and no
   replay has seen it yet: 1.2-1.4, 1.8, 1.12-1.18, 6.1-6.8, 6.10-6.14, 6.17, 6.23, 6.25, 6.26, 6.28-6.31 and A.7.10.
   Their old Actuals are T392's and predate flow 16 (their Gap lines say "The T392 Actual above predates T394");
   replace them as usual. Several Routes changed: 1.12, 6.3, 6.7 and 6.11 go through the College's tree; 1.13 types
   the old sub-specialities address, which now redirects; 1.15 reads the scales list the College was refused before.
   - Flow 11's to 15's habits still hold: the mail's registration link is
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
   - The College's catalogue (flow 16): the College's specialities are one page, its tree
     (/admin/colleges/{CollegeId}/specialities), with a section per speciality; a speciality or sub-speciality is
     created from the tree and its result comes back on the tree (?created=speciality-…, ?created=sub-…, ?saved=…), and
     a College, a scale and a curriculum are created onto their own page (?created) and a clone onto the new version's
     (?cloned=). The same token checks as flow 15's apply: quote the result, say where the focus is, check the address
     lost the token, reload and say the result is gone. A refusal sits under its field (a College's short code, a rung's
     label), focused, the typed value kept; a refusal that follows a confirmed action (6.8's delete) sits in the page's
     result region, focused. Quote the reach sentences ("Adopted by 1 institution, …; 3 registrars in training on it.",
     "No institution has adopted this version yet.", "PAED-006 is on 1 curriculum adopted by …") as the page prints
     them, and say where a count differs from the step's. Find Colleges by short code, specialities and
     sub-specialities by name, and scales by name, never by a remembered id.
   - Every page with header actions (Create …, Edit this version) keeps them beside the h1 from 1024 px (Q20). A 1280
     capture whose header differs from the old one only by that is the build; actions that overlap the title, clip or
     push the page sideways are a regression.
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
  powershell -File tools\scenario-replay.ps1 dump wombat_scenario_t394 scenario-t394-post-act<ACT>
- Leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t394: the flow 16 build made it wrong. Flow 16 covers Colleges and a College's record (create, refusals,
  Deactivate…, the result), /admin/specialities and the College's tree (sections, the tree's results, the old
  sub-specialities address's redirect), a speciality's and a sub-speciality's pages (create, default ladder,
  Deactivate…), the entrustment scales list (the College's read-only view, Delete… and its refusal) and a scale's page
  (Q18's refusals, Q16's line, the moves), the College's and the Administrator's EPA list and a national EPA's page
  (subtitle, reach line, pause dialog's reach), the College's Curricula and a curriculum's page (one column, Open for
  adoption, Hold back…, the clone, Q19's read-out, the results), the College's items and item page (the reach in the
  warning and the subtitle), the College admin's Home card and menu, the required-field line, every header's actions
  from 1024 px (Q20), and the catalogue tables' stacking (1024 px; Curricula and the items 1100 px).
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design, and the runbook line is wrong.
- new-defect: a defect not from T394 and not already filed.
- not-played: you could not play it (say why).
Wherever your act plays a flow 16 page at 390 or 768 px, also check that nothing scrolls sideways, and say so in that
step's Actual. Wherever it first shows a flow 16 table at 1280 (Colleges, the tree, the scales, the College's EPAs,
Curricula, the items), look at the capture: a title column squeezed to a few words a line beside empty columns is a
finding even though nothing scrolls (BRIEF § 11, flow 15).
Report blocking only if the app or the database is broken so that the next act cannot run.

YOUR ACT: <ACT>. Your file: execution/knowledge/scenario-paediatrics/<FILE>. Your act dir: <DIR>.
- Screenshots go in design\baseline\<DIR>, and scripts in <G>\<DIR>.
- Your snapshot name: scenario-t394-post-act<ACT>.

Return, as your final message: the act; steps_total; steps_passed (Gap: none); every Gap as step / kind / task /
one-line summary; the dump name; blocking yes or no; and notes. The notes cover anything the next act must know, which
captures show a registration link, and anything you could not check.
```

- `<PW>` = `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\e81867ab-c351-40bf-9d41-d1d61fe30f43\scratchpad\pw`
  (Playwright 1.56.1 and `lib.mjs`: launch, login, logout, waitBlazor and BASE; flow 15's replay used it).
- `<PWURL>` = `file:///C:/Users/Renier/AppData/Local/Temp/claude/c--dev-Wombat/e81867ab-c351-40bf-9d41-d1d61fe30f43/scratchpad/pw`.
- `<G>` = `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\28969ed0-c979-487c-82e2-606d8331c3a3\scratchpad\g394`
  (a new, empty `replay-pw.env` for this replay). T392's act scripts are in
  `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\ecc5a9d5-2a6e-43d8-a55a-e55b7e48f8ab\scratchpad\g392\act-*`
  (each act's `h.mjs`, or `common.mjs`, holds the shared helpers: devPw, envPw, setEnvPw, shot, txt, go, the stub mail
  blocks; act 6's `items.mjs` the item page's): copy what helps into `<G>\<DIR>`, fixing paths (the lib.mjs import
  above, `<G>` for the password file), ids, database names (wombat_scenario_t394) and dates (compute every date from D),
  and rewriting the catalogue scripts (acts 1 and 6, A.7.10) for flow 16's pages: the College's tree, the records titled
  by their name, the curriculum's own page, the reach lines. Never copy g392's replay-pw.env or read it.
- **Flow 16's browser checks**, played where their step is:
  - The focus after each action: the College's create refused under Short code and created (6.1, `?created`) and saved
    (6.2); the tree's results (6.3, 6.4, 6.7, 6.11, 6.12: `?created=`, `?saved=`); the scale's rung refused under its
    Label and created (6.5, `?created`) and saved (6.6); the delete refused at the list's top (6.8); the curriculum
    created (6.13, `?created`), cloned (6.29, `?cloned=`) and opened (6.31); the EPA saved (6.14, 6.28) and reactivated
    (6.23); the items' add and save (6.30). Check the focus by keyboard where a scripted change says it moved (BRIEF
    § 11, flow 14).
  - The one-shot tokens (6.1, 6.3, 6.4, 6.5, 6.7, 6.11, 6.12, 6.13, 6.28, 6.29, 6.30): the page strips the token from
    the address; a reload then says nothing of the result.
  - Every confirmation dialog (6.8 Delete…, 6.17 Deactivate… with its reach first): it opens with the focus on Cancel;
    quote its words and counts; Escape returns the focus to its trigger. 1.3, 1.14 and 6.11 offer Deactivate…: check it
    carries `aria-haspopup="dialog"`, and do not press it.
  - No Back links and no way to Colleges for the College admin (1.12, 1.13, 1.14, 6.11); his menu holds Entrustment
    scales (1.8, 6.10) and lights it on the list (1.15); the list offers him no Create, no actions and no link (1.15).
  - Q16 (1.4, 6.5, 6.6, 6.7): the ladder in use offers no Up, Down or Remove and says so above Add level; the CNSA
    scale offers them until 6.7 makes it Adult Neurology's default; reopen it after 6.7 and say whether they are gone.
  - Q19 (6.13, 6.29, 6.31, A.7.10): the Sub-speciality is a select on a version with no item and no adoption, and a
    read-out with its help once it holds items.
  - The 900-1100 sweep (BRIEF § 11, flow 15), in act 1 at 1.2 (Colleges), 1.12 (the tree), 1.4 (the scales, as
    devadmin), 1.16 (the College's EPAs), 1.17 (Curricula) and 1.18 (the items), and in act 6 at 6.1 (three Colleges)
    and 6.12 (the tree with two rows): every 25 px from 900 to 1100, plus 1101, 1125 and 1150 for Curricula and the
    items; at each width each `.table-container`'s table `scrollWidth` against its `clientWidth`, and the page's own.
    Record the smallest spare per table in the step's Actual; a table wider than its container is a regression.
  - At 390 (A.7.10): Home's four links, the stacked EPAs (no College cell), Curricula and the items, PAED-001's item
    page and 11.2's own page: nothing scrolls sideways, every control 44 px, Save on top of Cancel.
- **For the integrator, after the acts:** look at one 1280 capture of each flow 16 list (1.2, 1.4, 1.12, 1.16, 1.17,
  1.18) before the states pass (BRIEF § 11, flow 15). A fix found mid-replay waits until the acts using the published
  app are done; republish between the states passes, re-measure, and re-take the captures the fix changes.

## What flow 16 changed, step by step

Each changed step's Gap line says "The T392 Actual above predates T394". Acts 2 to 5 hold no changed step: they see flow
16 only in the header rule (Q20) and the shared required line.

### Act 1
- **1.2** Colleges (devadmin): the subtitle "The national Colleges. …", the caption "Every College", each row headed by
  its name, Created as the South African day, Edit and Specialities named for the row; Create college beside the h1.
- **1.3** The CPSA's record: titled "College of Paediatricians of South Africa" (h1 and tab), subtitled "CPSA · Active",
  no Back to colleges; the required line; the Status help (D56); Cancel, Deactivate… and Save. Leave by Cancel.
- **1.4** The scales: Create scale, Edit and Delete… on each row; the ladder's page titled by the scale, subtitled "A
  ladder shared by every College and institution.", Details and Levels; in use, so no Up, Down or Remove, and Q16's line
  above Add level.
- **1.8** Dr Kruger's Home: the card's new sentence and four links (Activity types added); Entrustment scales in his
  menu.
- **1.12** Specialities redirects to the CPSA's tree: one section, Paediatrics, with Edit and Create sub-speciality, its
  table "Sub-specialities of Paediatrics"; no Back to colleges (T291 item 2 closed); Specialities lit.
- **1.13** The old sub-specialities address redirects to the tree's Paediatrics section; the id comes from its Edit.
- **1.14** The sub-speciality's page: "Paediatrics", "Sub-speciality of Paediatrics", the trail, the required line, the
  default ladder and its help, the Status help; Cancel returns to the tree's section.
- **1.15** The College reads the scales list (D57; Access denied before): the reader's subtitle and caption, no Create,
  no actions, no link.
- **1.16** The College's EPAs: the new subtitle and caption, no College column (T3), Edit named by the EPA's full label.
- **1.17** Curricula: the College's subtitle and caption.
- **1.18** The items: Edit this version in the header (to the curriculum's page); the warning opens with the reach, "No
  institution has adopted this version yet."

### Act 6
- **6.1** Create college: subtitle and required line; the CPSA short code refused under Short code naming its holder;
  "College created." on the new record (`?created`); required messages in the label's words (T324, F-6.1a closed).
- **6.2** "College saved." on the record; the tree's subtitle keeps the name; the restart claim stands.
- **6.3** CNSA's Specialities is its empty tree; the create page's subtitle; "Speciality Neurology created." on the
  tree.
- **6.4** Create sub-speciality from the tree, its default ladder chosen at create; the result on the tree.
- **6.5** Create scale: the subtitle; the duplicate rung refused under the fourth rung's Label before anything is sent
  (Q18); "Entrustment scale created." on the scale's page; Up, Down and Remove offered (nothing uses it).
- **6.6** "Entrustment scale saved."; the moves still offered.
- **6.7** By the tree to Adult Neurology's page, titled by it; "Sub-speciality saved." on the tree; the scale is now in
  use (Q16).
- **6.8** Delete… asks first (T264, F-6.8a closed); Delete scale is refused at the list's top, focused, with Open Adult
  Neurology.
- **6.10** Home's four links; Entrustment scales in his menu; the two typed addresses still Access denied.
- **6.11** The tree; the speciality's page titled "Paediatrics"; "Speciality saved." on the tree; no way to Colleges
  (T291, F-6.11a closed).
- **6.12** Create sub-speciality offers three scales; "Sub-speciality Neonatology created." on the tree; reopened.
- **6.13** Create curriculum's subtitle; the new version's own page, its created result, Sub-speciality still a select
  (Q19), Open for adoption unticked with the reach, the clone's none intro; Curricula Held back; the items' Q14 empty
  state with Go to EPAs (T326, F-6.13a closed).
- **6.14** PAED-006: the subtitle, EV2's standing reach line and the required line.
- **6.17** The pause dialog opens with the reach; 11.1's items warning opens with it.
- **6.23** The reach line after the reactivation.
- **6.25**, **6.26** Prof Mbatha's create EPA and add item: the required line only (flow 15's otherwise).
- **6.28** The College's create EPA: subtitle, Sub-speciality help, required line; PAED-016's page with EV2's none line.
- **6.29** 11.1's items with the reach; Edit this version opens 11.1's page (Items (15), Sub-speciality read-out, Open
  for adoption ticked with the reach, Hold back…, the clone's intro, "11.1-copy"); the clone's result on 11.2's page
  (`?cloned=`), held back, its Sub-speciality fixed.
- **6.30** 11.2's items "No institution has adopted this version yet."; the add and edit pages' subtitles with the
  reach; the required line.
- **6.31** "Curriculum saved. It is open for adoption: an institution can adopt it now.", focused; Hold back… beside
  Save; the read-out stays.

### Appendix
- **A.7.10** At 390: Home's four links (44 px); the EPAs with no College cell; PAED-001's item page subtitled with
  11.2's reach, "no registrar in training on it" (Dr Ndlovu's erasure at A.1.12 ended the only one, build-review M1);
  11.2's own page by Edit this version, its Sub-speciality a read-out, Open for adoption 44 px with the reach, Save on
  top of Cancel, Cancel to Curricula.

## Flow 16's states to re-take

Every `states.md` row lane W marked "Not yet captured (T394)" (35) and every catalogue row it rewrote (53, and three
rows with no capture), in two passes after the appendix. Pass 1 takes the College records, the tree and its pages; pass
2 the scales, the EPAs, the curricula and the items.

**Pass 1** (Not yet captured marked \*):
- `home--college-admin`, `access-denied--signed-in`, and the Access denied row for a CollegeAdmin typing
  `/admin/entrustment-scales/new` or `/admin/colleges/{Id}`.
- `colleges-list--two`, `--loading`, `--load-error`\*.
- `college-edit--edit`, `--create`, `--invalid`, `--duplicate`, `--created`\*, `--saved`, `--deactivate-dialog`\*,
  `--deactivated`, `--not-found`, `--loading`\*, `--save-failed`\*.
- The `/admin/specialities` redirect row; `my-specialities-redirect--no-college`\*.
- `specialities-list--one`, `--empty`, `--other-college`, `--speciality-created`\*, `--sub-created`\*, `--sub-saved`\*,
  `--speciality-saved`\*, `--two`\*, `--loading`, `--load-error`\*, `--narrow`.
- `speciality-edit--edit`, `--create`, `--invalid`, `--duplicate`\*, `--deactivate-dialog`\*, `--deactivated`,
  `--not-found`, `--loading`\*, `--load-error`\*.
- The old sub-specialities address's redirect row; `sub-specialities-list--unavailable`\*.
- `sub-speciality-edit--edit`, `--create`, `--invalid`, `--no-default`, `--saved`, `--deactivate-dialog`\*,
  `--deactivated`, `--not-found`, `--loading`\*, `--load-error`\*.
- Also re-taken, rows lane W left as they were on pages flow 16 rebuilt: `colleges-list--three`,
  `colleges-list--narrow`, `college-edit--narrow`, `speciality-edit--narrow`, `sub-speciality-edit--narrow`.

**Pass 2:**
- `entrustment-scales-list--administrator`, `--college`\*, `--delete-dialog`\*, `--delete-refused`, `--deleted`.
- `entrustment-scale-edit--edit`, `--create`, `--duplicate`, `--created`\*, `--saved`, `--unknown`, `--loading`\*,
  `--load-error`\*.
- `epas-list--college`, `--no-college`\*; `epa-edit--edit`, `--deactivate-dialog`, `--create-national`,
  `--created-national`\*.
- `curricula-list--college`.
- `curriculum-edit--create`, `--invalid`, `--saved`, `--clone`, `--cloned`, `--opened`\*, `--hold-back-dialog`\*,
  `--held-back`\*, `--sub-speciality-fixed`\*, `--not-found`, `--loading`\*, `--load-error`\*, `--narrow`.
- `curriculum-items-edit--college`, `--no-items`, `--inactive`.
- `curriculum-item-page--add-national`, `--no-national-epa`\*, `--epa-held`\*, `--edit-national`, `--narrow`.
- Also re-taken: `entrustment-scales-list--read-only`, `--loading`, `--load-error`, `--narrow` and
  `entrustment-scale-edit--narrow` (pages flow 16 rebuilt), and flow 15's `epas-list--institution` and
  `epas-list--inactive`, left on the old column widths (T392's Notes).

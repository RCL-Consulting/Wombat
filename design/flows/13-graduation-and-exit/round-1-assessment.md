# Flow 13 round 1: assessment (T379 step 5)

Read-only, at HEAD `05b0f876`. Canvas "Wombat 13" (https://claude.ai/artifact/SujH2iB2snNe7VANE4SxdK), version
1793087531-266b, design system copied at version 1793082623-7a3a (`canvas.json`, `designSystems`). Boards:
`design/flows/13-graduation-and-exit/round-1/project/*.dc.html` (27, every board's text and markup read) and
`canvas.json`. `W/` = `src/Wombat.Web/Components/Pages/`, `P/` = `W/Portfolio/`, `Tr/` = `W/Admin/Trainees/`, `A/` =
`src/Wombat.Application/Features/`, `Nav/` = `src/Wombat.Web/Navigation/`, `Act4`, `Act5` =
`execution/knowledge/scenario-paediatrics/act-4-*.md` and `act-5-graduation.md`. **Obs** = observed in the code, the
runbook or the boards. **Inf** = inferred. Brief sections are `design/flows/13-graduation-and-exit.md`'s. The operator
took the brief's Q1–Q16 as recommended (2026-10-27, § 6); a board that contradicts one is a must-fix unless it argues to
reopen it, and then it is a question in § 8.

## Summary

- **The former trainee's Home and menu: pick S1-A** (one emphasised card headed by the end, then the record's four pages,
  each with a line on what it holds). It is Q1 as decided, and it is the cheapest: a STAR certificate and her export are
  each 2 + 0 from Home at 1280 and at 390 (Steps, recomputed).
  - **Correct it to Q2's menu (M1).** No board draws the decided menu. S1-A keeps the built one (Home, My progress, My
    data rights) and S1-B adds four links. Q2 is My progress, Export portfolio and My data rights.
  - **Reject S1-B.** Its four new personal links go beyond Q2. Home then only points at the menu, and at 390 every page
    of the record costs one press more (Menu).
  - **Reject S1-C.** Its primary header action "Export portfolio" contradicts Q1 ("No header action"). Three cards also
    say less than S1-A's four lines.
- **The staff export's entry points: pick S2-A** ("Export Lerato Molefe's portfolio" as an outline header action on the
  trainee profile, active and ended, and on the registrar's programme page; a link beside "Lerato Molefe's progress" on
  the committee review). It is Q4 as decided, 4 + 0 for Prof Mbatha and Mr Smit at 1280, 5 + 0 at 390.
  - **Correct it (M2).** Drop its link from the user page, which Q4 excludes.
  - **Reject S2-B.** Its Portfolio section is a new DATA read (count and last export). Its line "Every export of the
    same dates is the same file" is false once the record changes (5.23).
  - **Reject S2-C.** A new menu item and route (Portfolio exports) go against Q4. It lists current trainees as exportable
    with no training year for Dr Dlamini, and its sort says "by surname" but is not.
- **Export portfolio: pick V1** (the form as built, From on the programme start and To on today or the last day, on
  the South African calendar; the header names whose; the result names the file and Verify).
  - **Why V1:** it is the smallest change. Each of 5.9, 5.10, 5.12 and 5.23 is 2 + 0 in all three variations, and V1
    follows DESIGN.md § Form page, the result in an `ActionResult` above the form.
  - **Reject V2.** It adds a NEW whole-record card and a `details` for the same count.
  - **Reject V3.** Its "What the PDF holds" aside restates flow 20's PDF, which this flow may not change, and its
    receipt carries an unknown time ("[hh:mm]").
  - **Two decisions to reopen, both against the system:** the h1 (Q5's "Export your portfolio" breaks the nav-label rule,
    Q17) and where the result sits (Q5's "under the form" breaks § Form page, Q18).
- **Verify: pick V1** (the answer, an `<h2>` section focused after a press, above the field, which keeps what was
  typed; the help with each system's command).
  - **Why V1:** it alone draws every state asked: blank, verified with three exports, no match, not a hash, spaces only,
    the second file, check failed.
  - **Reject V2.** Its "You asked about …" echo duplicates what V1's kept field shows. It empties the field and does not
    draw spaces only, the second file or a failed check.
  - **Reject V3.** Its numbered steps and textarea are NEW for a paste an `<input>` already takes. Its answer under the
    button sits below the fold at 390.
  - **Words to settle:** the not-a-hash and verified words differ from Q6's (Q20).
- **Ending a programme: pick V1** ("End the programme", an `<h2>` section of its own under Profile details, with the
  last committee decision, the last day, and Mark complete… and Deactivate… as outline buttons, each in its
  ConfirmDialog). It is Q7 as decided, and the cheapest: 4 + 0 to complete and 4 + 1 to withdraw at 1280, the same as
  today.
  - **Reject V2.** Its radio adds a press (5 + 0 or 5 + 1), and its default (Completed, checked) points an irreversible
    end at one outcome.
  - **Reject V3.** It needs a new route and 6 presses.
  - **Correct the title (M3):** the h1 is "Lerato Molefe's programme" (Q7), not "Lerato Molefe", which is the user page's.
- **Completed and closed (S6) and the ended record pages (S7): confirm, with corrections.**
  - **S6:** the pager size (Q22: 20, not Q8's 50), View's name, the empty state marked NEW, and the training year at the
    end as DATA.
  - **S7:** PAED-010 and PAED-012 are drawn as yearly EPAs, but both are three a semester (M6). The record's links belong
    under the notice (M4, Q2, Q9). My activities keeps flow 03's columns.
- **The must-fixes** (§ 7):
  - **M1:** Q2's menu.
  - **M2:** no export link on the user page.
  - **M3:** the profile's title.
  - **M4:** the record's links under My progress's notice.
  - **M5:** the Steps board's 5.18 "unchanged" against Q12, and its missing A.2.1 and A.2.9 (Q16).
  - **M6:** PAED-010's and PAED-012's targets.
  - **M7:** Dr du Plessis's "last committee decision" line, against Q13 and Act 4.
- **The attachments:** no board says whether they arrived. There is no Assumptions note anywhere, and nothing names a
  capture. Re-attach § 4's key screenshots for round 2, name the folder, and ask round 2 to say whether they arrived
  (§ 11, flow 06).
- **S1-A at 1280 is not missing.** `canvas.json` maps `Main.dc.html` to "S1 · A one record card" (x 0, 1280 × 3304), and
  its board title reads "Screen 1 · Structure A — one record card, menu as built". The name is only the canvas's first
  board. Round 2 may rename it `S1-A.dc.html`.
- **The Spec and Steps boards were drawn**, as the ask required. Their counts recompute (§ 2–6), except V2's radio
  (N5).

## 1. What was drawn

27 boards in six rows (`canvas.json` notes row1–row6), each linking `ds/wombat/tokens.css` and
`components/bundle.css`. All boards share one helmet stylesheet, whose NEW classes are flow 13's (`.record-links`,
`.ended-line`, `.hash-value`, `.verify-answer`, `.cmd-list`, `.end-section`, `.receipt`) or board scaffolding (`.sheet`,
`.cap`, `.frame`, `.wf`, `.tag-*`, `.pv-*`, `.stack8/16/24`, `.row8`, `.col2`). Every class a board uses is in
`design/system/components/bundle.css` or in that helmet (Obs, script over all 27). Every board uses the copied mark
`/_blob/f4e05985da677926678d5d52de67cf4f`, and the signed-out bar draws it too. The files are LF.

| Row | Boards | State drawn |
|---|---|---|
| 1 · Home and menu (wireframes) | Main (= S1-A, 1280), S1-A-390, S1-B, S1-B-390, S1-C, S1-C-390 | 5.21 Molefe; 5.28 du Plessis (no STAR); a former trainee holding Assessor; neither role nor record ("No role assigned"); at 390 Molefe, the menu open, du Plessis |
| 2 · Staff entry points (wireframes) | S2-A, S2-B, S2-C, S2-390 | the profile active and ended; the user page's no-role section; the committee review (5.12); the registrar's programme page; the export's staff header; C's list page |
| 3 · Export | S3-V1, S3-V2, S3-V3, S3-390 | 5.9 before and after; 5.10 staff before; 5.12 staff after; 5.23 her own after the end; refused (Dr Dlamini, an unknown id); From after To (V1 only); 390 |
| 4 · Verify | S4-V1, S4-V2, S4-V3, S4-390 | blank; 5.13 verified (upper case, three exports); 5.14 no match; not a hash; spaces only, 5.23's second file and check failed (V1 only); A.7.12 at 390 |
| 5 · The end of a programme | S5-V1, S5-V2, S5-V3, S5-390 | 5.16 the story's day typed, refused; 5.17 the dialog, the ended record; 5.27 the Deactivate dialog, the withdrawn record; 390 |
| 6 · One proposal each; Spec, Steps | S6, S7, S67-390, Spec, Steps | closed list two, none, forty; My progress completed and withdrawn; the EPA page ended and paused; My activities after the end; 390 |

**Not drawn in round 1** (round 2's):
- **Home:**
  - loading;
  - the Assessor former trainee and "No role assigned" at 390;
  - a 60-character name.
- **Staff entry points:**
  - a caller the export refuses seeing no link (A11);
  - the Committee member's and the speciality admins' views;
  - the review page for an ended registrar;
  - Mr Smit on an ended registrar's programme page. Programme trainees lists current trainees only
    (`A/Programme/Trainees/ProgrammeRosterReader.cs:18–19, :57`), so after the end that page is reached only by its
    address (Inf).
- **Export:**
  - generating ("Generating...");
  - a failed export;
  - loading;
  - From after To in V2 and V3;
  - an export of 200 activities (heavy);
  - a staff caller who also oversees at another institution.
- **Verify:**
  - a 64-digit hash pasted with spaces and a line break;
  - a signed-in visitor (the page is static either way).
- **The end of a programme:**
  - an unknown id;
  - a load error;
  - a failed end ("nothing changed, the day kept");
  - each dialog's Cancel, with the focus back on its trigger;
  - an end recorded on the programme's first day;
  - a last day in a period's last month;
  - a profile moved before the end.
- **Completed and closed:** none and forty at 390; two people with one name.
- **The ended record:**
  - Dr du Plessis's EPA page;
  - the EPA page at 390 (A.7.4);
  - a record with 17 EPA cards (heavy);
  - My authorisations as a former trainee (Q3, drawn nowhere), with its Download and du Plessis's empty card.
- **768 px** anywhere (§ 11, flow 06).

## 2. The former trainee's Home and menu: S1-A, S1-B, S1-C

All three keep, rightly (Obs):
- no "Acting as" head and no subtitle for no role (DESIGN.md:556; `Nav/HomeFrame.cs:33–36` returns null for no role);
- "No role assigned" word for word for an account with neither a role nor a record (`W/Home.razor:72–74`);
- the end in the cast's words: "You completed your programme on 2026-10-26." and "Your programme ended on 2026-10-24."
  (T311; `TraineeHomeWords.Ended`, brief § 5);
- training year 4 and 2 when it ended;
- Dr du Plessis with no STAR.

- **S1-A, one record card.** The card "Your training record" (`.ended-line`, the kept-read-only line, then
  `.record-links`): My progress, My authorisations ("Your 15 STARs, each with its certificate to download."), My
  activities, Export portfolio.
  - **Dr du Plessis:** the STAR row is text, "No STAR was issued to you, so there is no certificate to download.", with
    no link. That fits Q1's "only when she holds any", since no link is offered.
  - **The Assessor former trainee:** her Assessor Home, with an info line in flow 04's `.other-role-line` naming the
    record and "Open My progress" (PROPOSED, unmarked; Q25).
  - **Counts (Steps, recomputed):** certificate 2 + 0, export 2 + 0, at 1280 and at 390.
- **S1-B, the record in the menu.** Four personal links (My progress, My authorisations, My activities, Export portfolio)
  for a record holder. Home is an info alert plus a "Your programme" card with a details list.
  - **Against Q2:** two of the four links are more than Q2 decided.
  - **Against Q1:** Home no longer lists the record. It says "Its pages are in the menu", which costs Menu at 390 (3 + 0).
- **S1-C, Home as an index.** "Export portfolio" as Home's primary header action, and three cards (Your programme, Your
  STARs, Your activities).
  - **Against Q1:** Q1 says no header action, and flow 10's pattern is a role's create action, not a record's.
  - **For the Assessor former trainee:** it adds a whole record card under her Assessor Home.

**Recommend S1-A**, with Q2's menu (M1).

**Its link names:** S1-A names each link by its page's title, with a line saying what the page holds. Q1 named them
"Your STAR certificates", "Your activities" and "Export your portfolio". Keeping the page titles keeps one name per page,
the h1 each link lands on (Q23).

**Correct in S1-A:**
- M1, the menu;
- the STAR row's label is a `<span>` styled like the links (an inline `font-weight` and the link's class); make it read
  as text (N6);
- `.record-links a.record-link` is 24 px with no 390 rule: 44 px at 390 (S14);
- the training year in each line is a DATA read (Home reads only claims today, `W/Home.razor:100–102`).

## 3. The staff export's entry points: S2-A, S2-B, S2-C

All three draw, rightly (Obs):
- the export's staff header "Export Lerato Molefe's portfolio" (T314) over "Kgosi Kgari Teaching Hospital · Paediatric
  EPA Curriculum (11.1) · …" (DATA, the gated query);
- the user page's subtitle and no-role sentence word for word as flow 12 built them (`W/Admin/Users/UserPageText.cs:21–22`;
  5.17's Expect);
- the committee review's subtitle as Act 5 records it ("Pre-graduation review · Paed Annual Review Panel · Training year
  4", `Act5:128`) with flow 07's `.review-progress-link`;
- Prof Mbatha's grouped menu (People, Curriculum, Reviews, Access and audit; `Nav/NavItems.cs:150–157`) and Mr Smit's
  flat eight (`:175–176`).

- **S2-A, named actions.** An outline header action on the profile and on the programme page; a link on the review's
  subtitle and on the user page. The export page lives under the page it came from, "Home › Trainees › Lerato Molefe ›
  Export portfolio".
  - **Counts (recomputed):**
    - 5.10: Trainees, her row, Export…, Export PDF: 4 + 0 (today 4 + 3).
    - 5.12: Committee reviews, the review, Export…, Export PDF: 4 + 0.
    - At 390, each is 5 + 0.
  - **Against Q4:** the user-page link is excluded by Q4 (M2).
- **S2-B, a Portfolio section.** "Exported 3 times, last on 2026-10-26. Every export of the same dates is the same
  file." (DATA) with "Export Lerato Molefe's portfolio…" on every page, the user page included.
  - **The count is right at 5.12–5.17:** 5.9, 5.10 and 5.12 each write a row, `A/Reporting/ExportPortfolio.cs:52–61`.
  - **The second sentence is false** after the end (5.23's file differs for the same dates).
  - **Against Q4:** the user page again.
- **S2-C, a list page.** "Portfolio exports", a new item under People (PROPOSED), lists the five registrars with
  Export… named for each.
  - **Against Q4:** a new route and menu item; the Coordinator's menu goes past the flat eight.
  - **Its rows:** Dr Dlamini reads "Current" with no year while Mahlangu and Ndlovu read "training year 1". The caption
    says "by surname" but the order is Molefe, du Plessis, Dlamini, Mahlangu, Ndlovu.

**Recommend S2-A without the user-page link.**

**Correct in S2-A:**
- M2, the user-page link;
- 5.10's header "Completed 2026-10-26": at 5.10 she is in training year 4 (S3's boards have it right; S2);
- the programme page at 5.12 reads "Programme completed 2026-10-26" before the end, and leaves out `RegistrarWords.Subtitle`'s
  lead: built, it is "Training year 4 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics" while she is
  current (`src/Wombat.Web/Components/Shared/Programme/RegistrarWords.cs:28–45`; S2);
- "[review period]" is "2026 S2" (`Act5:128`; S3);
- the header action's inline `white-space:nowrap` would push a 60-character name off the 390 row: drop it (S9);
- the trail: Q5 says "from the page it was opened from", which needs a return parameter, or the owner table that the
  Spec proposes. Settle which (Q27).

## 4. Export portfolio: V1, V2, V3

All three share what follows (Obs).
- **The defaults:** From opens on the programme start (2023-01-15) and To on today (2026-10-26), or the last day for a
  former trainee, on the South African calendar (T325). Built, it is the last twelve months on the server's
  `DateTime.Today`, `P/ExportPortfolio.razor:55–56`.
- **The trainee's menu:** Home, Log an activity, My activities, MSF reports, My committee reviews, Export portfolio, then
  My progress and My data rights (`Nav/NavItems.cs:180`, `:212–215`).
- **The file and its result:** portfolio-aba6ce7943c2.pdf (`Act5:339`). The result names the file and Verify (NEW
  words).
- **The staff header** names her (T314).
- **The refusal draws no form** (Q5), for Dr Dlamini typing Dr Molefe's id and for an unknown id alike. V1 keeps
  today's sentence, V2 draws flow 01's Page not found, and V3 its own page (PROPOSED words).

- **V1, the form as built.** From and To with help ("Your programme started on 2023-01-15.", "Today, on the South
  African calendar." / "Your last day in the programme."), a line under them, Export PDF. The result sits above the form
  in `ActionResult`, as DESIGN.md § Form page draws it (DESIGN.md:3809–3814). From after To is refused under From,
  `aria-invalid`, the typed day kept and focused.
- **V2, one press.** A "Your whole record" card with Export PDF, and "Export other dates" in a `details` (NEW). Every
  step is still 2 + 0, because V1's defaults are the whole record already.
- **V3, a receipt.** An aside "What the PDF holds", and after the export a receipt in the form's place: Trainee, Period,
  "Exported by You, 2026-10-26 [hh:mm] SAST", Check it.
  - **The aside restates flow 20's PDF:** its sections and "(15 on record)" would have to follow every change to the PDF.
  - **The time** is not in the export's result today.

**Recommend V1.**

**Correct in V1:**
- the refusal page (Q19): DESIGN.md's Unavailable pattern, not today's sentence under the page's own h1;
- "The same dates always give the same file." holds only while the record is unchanged: 5.23 shows the same dates
  giving a new file. Say "Exporting the same dates again gives the same file, until the record changes." (S4);
- the required marks on From and To are NEW. Today both dates are optional (`DateOnly?`, the validator compares only
  when both are set, `A/Reporting/ExportPortfolio.cs:20–23`). Mark it PROPOSED, and draw the refusal of an empty date
  (S11);
- the labels "From date" and "To date" are kept. The subtitle "A PDF of your portfolio for the dates you choose: …" and
  the result "Your portfolio exported: portfolio-aba6ce7943c2.pdf. …" are NEW; today they read "Generate a PDF export
  of the trainee's portfolio." and "Portfolio exported successfully. The download should start automatically." (S6);
- for the former trainee (5.23): Export portfolio lights itself under Q2, so the trail "Home › My progress › Export
  portfolio" goes (M1);
- draw generating, a failed export ("**The portfolio could not be exported just now.** Nothing was downloaded. …", the
  Spec's), and loading (the header and a skeleton, no Export PDF until the read is in), at 1280 and 390.

## 5. Verify: V1, V2, V3

All three share what follows (Obs).
- **The page:** static under the signed-out bar (brand and Sign in, `page--signed-out`), a GET form with `check=1`.
- **The field:** "Content hash (SHA-256)" kept; the help names certutil, Get-FileHash, shasum and sha256sum (T287).
- **The answer:** an `<h2>` "Export verified" (T280), focused after a press.
  - **First generated** "2026-10-26 22:51 SAST": 20:51 UTC at `Act5:427`, in South African time (A9).
  - **Exported** "3 times, the last on …": 5.9, 5.10 and 5.12.
  - **Trainee and First exported by** are both 2b5abc2f-…: she exported first at 5.9, so this is right.
  - **Filter and File:** "2023-01-15 to 2026-10-26" and portfolio-aba6ce7943c2.pdf.
- **Not a hash:** "This is not a SHA-256 hash: …" under the field (PROPOSED words).
- **No match:** today's two sentences and a third, NEW: "One changed byte changes the whole hash: …".

- **V1, the answer above the field, the value kept.** It draws every state but the heavy paste. The help is
  `.field-help` plus `.cmd-list`, both named by `aria-describedby`.
- **V2, the answer is the page.** "You asked about …", the answer, then "Check another file" with an empty field. Spaces
  only, the second file and a failed check are not drawn.
- **V3, three steps and a textarea.** The answer under the button. An `<input type="text">` already drops a pasted line
  break and the check strips spaces (the Spec), so the textarea buys nothing (Inf).

**Recommend V1.**

**Correct in V1:**
- **The shape and words against Q6.** Q6 decided a sentence ("Wombat produced this PDF. It was first generated on … and
  has been exported 3 times, most recently on …") and the not-a-hash words "That is not a SHA-256 hash: it has 64
  characters, …", where "it" reads as the value typed. The board's details list and its words are clearer (Q20).
- **"Nothing is wrong with the file on that account."** claims what was not checked. Keep today's "The export could not
  be checked just now. Try again in a few minutes." without "Please" (S5).
- **The subtitle** "Check whether a PDF was produced by Wombat, without signing in." is NEW. Today it reads "… by this
  system." (S6).
- **The answer's inline styles:** `border-left:4px solid var(--success-color)`, an empty spacer
  `<div style="height:24px">`, `style="margin:12px 0 0"` and the `<h2>`'s flex. Verify carries four inline styles today
  (T280, brief § 5), so each must be a named class, not copied (S9).
- **The input's `required`.** It is gone from the board's input; keep it (today `P/VerifyExport.razor:26`). The spaces
  refusal still covers what `required` lets through (N7).
- **The `check` flag:** a hidden input on the board, the button's name and value today (`:31`). Either sends `check=1`
  on a press. Keep the button's, so a link with `?hash=` alone is still told apart (N7).
- **Real values:**
  - the last export's time is not recorded (5.10's and 5.12's Actuals give none): say "[time] SAST, DATA, unreplayed";
  - 5.23's file is portfolio-344dd52bdded.pdf, generated 2026-10-26 22:56 SAST (20:56 UTC, `Act5:640–643`);
  - the genuine hash is aba6ce79…c995 (`Act5:340`) (S3).
- **`verify-card`** is on the wrapping `<div>` and on the `<form>`; use it once (N8).

## 6. The end of a programme: V1, V2, V3

All three share what follows (Obs).
- **Kept from flow 12:** Back to trainees gone (Q7); the summary as flow 12's `dl.detail-list` who line (Email, Status,
  Institution; Name dropped since the h1 names her); Profile details as an `<h2>`, Save profile with Cancel.
- **The last committee decision line (Q13, DATA).** It reads "Graduate (programme complete), Pre-graduation review,
  ratified [date]". Ratified at 5.6, so the date is 2026-10-26.
- **The help:** the last day's help, restated.
- **The dialogs.** Each is titled by the person, with Cancel first in the DOM. At 390, flow 08's `.dialog-card
  .form-actions` stacks them column-reverse, the action on top (bundle.css:6150–6167). The bodies are today's with
  "keeps the record, read-only, and can still export the portfolio" added (NEW).
- **The Deactivate dialog** adds "1 activity, observed 2026-10-25, stops counting." (DATA). The Mini-CEX is `D−1`, right.
- **The results** take the focus at the top. Deactivate's gains "1 activity observed after that day no longer counts."
  (NEW, DATA).
- **The ended record:** Status with its line, then Profile details read-only, plus a "Last day in the programme" row and
  a NEW section "The account" with "Open Lerato Molefe's user page".
- **The values:**
  - expected completion 2027-01-14 (`J+1y−1d`) and 2029-01-14 (`J+3y−1d`, `Act2:796`);
  - the story's day 2027-01-14;
  - the addresses molefe@ and duplessis@kgk.wombat.local (README § Cast).

- **V1, two named ends.** The "End the programme" section with the day and two `.role-row`s, each a line on what it
  does and an outline small button, Mark complete… and Deactivate….
  - **The day's check:** it is made on the press, before any dialog (PROPOSED). 5.16's Expect loses its dialog (Q21).
  - **Counts:** 5.17 4 + 0, 5.27 4 + 1 (at 390, 5 + 0 and 5 + 1), as today.
- **V2, a choice then one button.** Described radios (Completed: graduated / Left without completing: withdrew), then
  "End the programme…".
  - **The default:** the board checks Completed, so 5.17 is 4 + 0, not the Steps' 5 + 0 (N5).
  - **For withdrawal**, the radio is 5 + 1.
  - **A default on an irreversible choice** is what flow 08's choice-group avoids (Inf).
- **V3, a page of its own.** "End the programme…" in the header opens `/admin/trainees/end` (PROPOSED route), with the
  choice, the day, "Either way" and Continue….
  - **Counts:** 6 + 0 and 6 + 1.
  - **Its `<h2 style="font-size:1.1rem">`** is an inline style.

**Recommend V1.** It is Q7's section with two outline triggers, at today's counts. Accept its early check (Q21): the
command still refuses at confirm, with the same words under the field, since "today" can turn between the two (CLAUDE.md
§ EncounterDateGate: 22:30 UTC is already tomorrow).

**Correct in V1:**
- **M3, the title:** "Lerato Molefe's programme", its tab and its crumb.
- **M7, Dr du Plessis's decision line.**
- **The section's heading:** Q7 says "End this programme" (N1).
- **The ended record's order:** Q7 puts the outcome first. Today's Status sits under Email, and the added "Last day in
  the programme 2026-10-26 (graduated)" row repeats it (S10).
- **The refusal** "The last day cannot be after today (2026-10-26)." is NEW (today "The completion date …",
  `src/Wombat.Domain/Identity/TraineeProfile.cs:178`; 5.16's Expect quotes it) (S6).
- **The dialog titles:** "Mark Lerato Molefe's programme complete?" and "Deactivate Pieter du Plessis's profile?" are
  NEW (today "Mark this programme complete?" and "Deactivate this trainee profile?", `Tr/TraineeProfileEdit.razor:137–150`) (S6).
- **The Deactivate result:** "The last day …" is NEW for "Their last day …" (S6).
- **Mark complete's confirm** is drawn `btn-danger`. Today it is `DangerAction="false"`, a primary (`:148`). Mark it or
  keep it (Q24).
- **"The account" section** is PROPOSED and unmarked (Q26).
- **The page's classes.** The page sits in flow 12's `.user-page` and the end section in `.user-section` with `.role-rows`.
  That reuse gives 44 px and the stacked Save row at 390 (bundle.css, Flow 12 shared). Name it in the Spec as reused,
  or give the profile its own scope (S8).
- **The Save row's alignment.** It overrides `.form-actions`'s `justify-content: flex-end` twice: inline on the row, and
  `.end-section .form-actions` in the helmet. That is a board rule the build would copy (BRIEF § 11, flow 11) (S7).
- **Status "Active"** is drawn `badge badge-completed`; check the tint against flow 11's Active (N9).
- **Not found:** the Spec proposes Page not found drawn in place for an unknown id. DESIGN.md's Unavailable pattern
  (DESIGN.md:1825–1827), which flow 12 confirmed for "User unavailable" (C21), is one page for unknown and out of scope.
  Keep "Profile unavailable", with "This trainee profile does not exist, or you cannot open it." and "Go to Trainees"
  (S12).

## 7. Corrections

Severity: **M** must (round 2 cannot go out without it), **S** should, **N** nit.

### Wrong against the code or the runbook

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| M1 | Main, S1-A-390, S1-B, S1-B-390, S3-V1/V2/V3 5.23, S3-390, S7, S67-390 | **Q2's menu is drawn nowhere.** A record holder's personal links become My progress, Export portfolio and My data rights (PROPOSED `NavItems.PersonalFor`, today `Nav/NavItems.cs:212–215`). Export portfolio then lights itself, so 5.23's trail "Home › My progress › Export portfolio" goes. My authorisations and My activities stay owned by My progress. S1-B's four links go beyond Q2. | M |
| M2 | S2-A (user page) | **No export link on the user page** (Q4: "not on the user page (flow 12's, and Users is not a trainee list)"). | M |
| M3 | S5-V1, S5-V2, S5-V3 (record), S5-390, S2-A/B/C (profile) | **The profile's h1 is "Lerato Molefe's programme"** (Q7), with its tab and crumb. "Lerato Molefe" is the user page's h1 (flow 12), so two pages of one person would share a title. | M |
| M4 | S7, S67-390 | **The record's links sit under My progress's notice** (Q2: "a 'Your record' line under its notice"; Q9: "My progress's notice gains the record's links"), not in the Your programme card. With M1, the line links My authorisations (only with a STAR) and My activities; Export portfolio is in her menu. | M |
| M5 | Steps | **5.18 is not "unchanged".** Q12 folds T311 item 4 into this build (drop "your committee has ratified your final entrustment decisions", link My progress). **A.2.1 and A.2.9 change** with the cleanup job (Q16). **5.8, A.1.6 and A.4.6** cross these pages (brief § 7): list each with its new Expect or "unchanged". | M |
| M6 | S7, S67-390 | **PAED-010 and PAED-012 are drawn under "Once a year", "Target: 1 per academic year", periods "2026" and "2025".** Both are three a semester, six a year (Annexure B, `src/Wombat.Infrastructure/Persistence/Migrations/20260923082939_T130_AcademicPeriodQuota.cs:36, :38`; the seed's `observationsPerSemester: 3`), so they belong under "Each semester", with semester periods, like PAED-002. Their minimum in year 4 is 5 (order 6, right). | M |
| M7 | S5-V1, S5-V2, S5-V3 (5.27) | **"No committee decision has said Pieter du Plessis may graduate."** contradicts Q13, which reads the last ratified decision with no gate. Act 4 ratified his Annual progression review as "Satisfactory with Observations" (`Act4` Step 4.30): "The last committee decision: Satisfactory with Observations, Annual progression review, ratified 2026-10-26." The line about graduating is a gate's wording (T166's gating stays undecided). | M |
| S1 | S2-A, S2-B, S2-C, S3-V1/V2/V3, S3-390, S5-*, S6, S67-390 | **Prof Mbatha and Mr Smit are drawn with My progress** under the rule. `PersonalFor` gives it only to a holder of Trainee or of the record's claim (`Nav/NavItems.cs:212–215`); theirs is My data rights alone. | S |
| S2 | S2-A/B/C, S2-390 | **5.10's staff header reads "Completed 2026-10-26"**, but at 5.10 she is current, in training year 4 (S3 has it right). The programme page at 5.12 reads "Programme completed 2026-10-26" before 5.17, without `RegistrarWords.Subtitle`'s "Training year 4 · …" lead. | S |
| S3 | S2-A/B/C, S5-*, S4-*, S3-V3 | **Placeholders the runbook fills:** "[review period]" is 2026 S2; "[date]" for Molefe's ratification is 2026-10-26; 5.23's file is portfolio-344dd52bdded.pdf, 22:56 SAST; the hash is aba6ce79…c995. Only the last export's time is unrecorded (mark it DATA). | S |
| S4 | S3-V1, S3-V2, S3-V3, S2-B | **"The same dates always give the same file"** is false once the record changes (5.23: same dates, new file). | S |
| S5 | S4-V1 | **"Nothing is wrong with the file on that account."** asserts what a failed check did not establish. | S |
| S6 | S3, S4, S5, S6, S7 | **Built words changed without a NEW mark:** Export's subtitle and result; Verify's subtitle and failure; the last-day refusal; both dialog titles and their added sentences; Deactivate's result ("Their" → "The"); the last day's help; the closed list's caption, its row action and its aria-label ("Edit ‹name›" → "View ‹name›'s ended record"); My activities' subtitle. Changing them is allowed. Mark each NEW in the Spec's messages and give its Expect on the Steps board (flow 12's C8). | S |
| S7 | S5-V1 (all S5) | **The Save row's alignment is overridden twice:** inline `justify-content:flex-start` and `.end-section .form-actions`. Both are against `.form-actions`'s flex-end (bundle.css:605–613); either keep the system's or name the change in DESIGN.md. | S |
| S8 | S5-* | **The profile borrows flow 12's `.user-page`, `.user-section` and `.role-rows`.** Say so in the Spec, since the 44 px and stacked rows at 390 come from them, or give the profile its own scope. | S |
| S10 | S5-V1 (ended) | **The ended record's order:** Q7 puts the outcome first; drop the duplicate "Last day in the programme … (graduated)" row or the Status line. | S |
| S11 | S3-V1 | **From and To marked required** is a new refusal (both optional today). Mark it PROPOSED and draw an empty date refused. | S |
| S12 | Spec | **"An unknown id is Page not found drawn in place"** (the profile) is against DESIGN.md's Unavailable pattern and flow 12's C21. Keep "Profile unavailable", with one body for unknown and out of scope. | S |
| S13 | S7, S67-390 | **My activities is redrawn:** "Assessor" for "Who has it now", a NEW caption, and no "All activities (n)" heading (`W/Activities/MyActivities.razor:52–60`). Draw flow 03's table as built and change only the subtitle and the note row (both NEW, the row's reason DATA). **The EPA page:** draw flow 05's headings (Observations, Entrustment, Activities on this EPA, `P/EpaProgress.razor:82, :117, :166`), not "Each period". Keep "Open My authorisations" as flow 05's `.epa-auth-link` link, not a button. | S |
| N1 | S5-V1 | The section is "End the programme"; Q7 says "End this programme". | N |
| N2 | S6 | View's name "View Lerato Molefe's ended record"; Q8 says "View Lerato Molefe's record". Either, once. | N |
| N3 | S2-C | Dlamini "Current" with no year; the order does not follow its caption. (Not picked.) | N |
| N4 | S7 | PAED-012 is linked on her record. Today a paused EPA is not linked (6.21's Do). Under T312 it is: mark it PROPOSED with T312. | N |
| N5 | Steps, S5-V2 | V2's 5.17 is 4 + 0 with the board's default radio, not 5 + 0. | N |
| N6 | Main, S1-A-390 | The STAR row's label is a `<span>` with the link's class and an inline `font-weight`. Make it plain text. | N |
| N7 | S4-V1 | Keep `required` on the input and `check` on the button. | N |
| N8 | S4-* | `verify-card` twice (the `<div>` and the `<form>`). | N |
| N9 | S5-* | Status "Active" as `badge-completed`. | N |

### Invented people or values

- **No invented person** (Obs). Every name, address and role is the cast's (README § Cast). "[Name as stored]" stands
  for the account with neither a role nor a record, which no cast member reaches (brief § 3).
- **Invented or wrong values:**
  - PAED-010's and PAED-012's yearly targets (M6);
  - Dr du Plessis's decision line (M7);
  - 5.10's "Completed" (S2);
  - S2-C's Dlamini (N3).
- **Right:**
  - 15 STARs and training years 4 and 2;
  - 2023-01-15 and 2025-01-15;
  - 2027-01-14 and 2029-01-14;
  - the story's day 2027-01-14 (`J+1y−1d`);
  - the Mini-CEX on 2026-10-25 (`D−1`), rated 3b, David Naidoo;
  - 22:51 SAST from 20:51 UTC;
  - 3 exports at 5.13;
  - Trainee and first exporter both her id;
  - "3 current" (Dlamini, Mahlangu, Ndlovu) and "2 ended", the latest end first;
  - the minimum "5" in year 4 and "3b" in year 2 (the seed's stage levels).

### DATA claims (the build's new reads)

The Spec's DATA table is right as far as it goes:
- **Home:** how and when the programme ended, and the STAR count;
- **Export:** the name query gated as `MayReadAsync`, the programme start and the last day;
- **Verify:** the first generation, the count and the last export;
- **The end section:** the last ratified decision, and the activities a withdrawal takes back;
- **My progress:** the curriculum version it ended on (T312);
- **The closed list:** the training year at the end.

Add these:
- **Home (S1-A):** the training year when it ended, in My progress's line.
- **The staff export's trail:** her profile id from the user id the route carries (the crumb "Lerato Molefe" links
  `/admin/trainees/edit?id=`).
- **My activities' note row:** for a completed activity that credited nothing, whether it was observed after the last
  day (the reason in "… observed after your last day in the programme, 2026-10-24").
- **Deactivate's result:** the count of activities taken back. It is the same read as the dialog's, re-read after the
  save.

The profile's Institution and training year are not new: `TraineeProfileDto` carries both
(`Tr/PendingTraineesList.razor:183, :193`).

### PROPOSED items

Marked PROPOSED on the boards, and right to be:
- My authorisations admitting the record (Q3) and the EPA page's link to it;
- the owner table;
- the staff header's gated name query;
- Export's defaults;
- every Verify rule (any case, any whitespace, not-a-hash, first/count/last, the records kept, SAST);
- the end section's early check;
- the h1 by the person;
- the closed list's View, order and pager;
- T312's frozen record.

Add these, which are drawn but not marked:
- the Assessor former trainee's record line (Q25);
- "The account" section and its user-page link (Q26);
- Mark complete's danger confirm (Q24);
- the closed list's empty state (today the section is not drawn when empty, `Tr/PendingTraineesList.razor:213–216`);
- From and To required (S11);
- My activities' subtitle and note row;
- the export trail's return parameter, if Q27 chooses it.

### Access control and anonymity

| # | Board(s) | Finding | Sev |
|---|---|---|---|
| A1 | S4-* | **Verify names nobody** (Obs). Trainee and First exported by are ids, and the note says the issuing institution can say whose. "First exported by" holds one id more than today's "Exported by" and no name. Keep it. | — |
| A2 | S3-* | **The refusal draws no form and names nobody**, for an unknown id and another institution's alike. It must be one page whatever the cause (`A/Reporting/ExportPortfolio.cs:30–35`'s one message), in DESIGN.md's Unavailable shape (Q19). The gated name query must answer nothing, not a different nothing, for each cause. | — |
| A3 | S2-A | **The header action shows only where `MayReadAsync` admits the caller** (Q4, A11). Round 2 must draw a page where it is absent: a Committee member of another discipline on the review page, or an Assessor (who reaches none of these pages). | S |
| A4 | S2-A | **After the end,** Mr Smit reaches Dr Molefe's export only from the review page. Programme trainees lists current trainees only (`ProgrammeRosterReader.cs:57`). Say so on the Steps board. It is not a defect: the export stays reachable. | S |

### Accessibility and the 390 boards

- **Good (Obs):**
  - one h1, and `<h2>` for Verify's answer, the profile's sections and the dialog titles (T280);
  - the closed list captioned "Programmes that have ended, the latest end first. An ended record is read-only.", which
    does not repeat the heading;
  - its rows' `th scope="row"` with View named for the row (with RowNames' address when names repeat, the Spec's edge);
  - refusals under their field with `aria-invalid`, `aria-describedby` and `.input-validation-error`, the typed value
    kept;
  - each trigger `aria-haspopup="dialog"`, its dialog's safe button first in the DOM, stacked action-on-top at 390;
  - Verify a GET form that works with no script;
  - the hash and ids wrapping (`.hash-value`, `.receipt dd { overflow-wrap: anywhere }`), and the receipt one column
    at 390.
- **S9 (should):** inline styles that would reach a `.razor` file: Verify's answer (four), the header actions'
  `nowrap`, the dialogs' `max-width:32rem`, the Save row, V3's `<h2>`. Name each as a class.
- **S14 (should):** 44 px at 390 has no scope for:
  - Home's `.record-links` (24 px floor);
  - Export PDF;
  - Verify's button (A.7.12's Expect asks 44 px on the anonymous pages);
  - the closed list's View;
  - S7's record links (`.row8` is board-only).

  The bundle's 44 px rules are per page scope (`.user-page`, `.inv-page`, `.my-authorisations` …), so each page needs
  its own, named in the Spec.
- **S15 (should):** board utility classes (`.stack8/16/24`, `.row8`, `.col2`, `.export-period`, `.end-decision`) are
  not system classes. The Spec names `.export-period` but not the others; each must become a named class or go.
- **Round 2 must show:** the focus after each dialog's Cancel (back on its trigger); loading and load errors as named in
  STATES TO SHOW; 768 for the profile and the closed list.

### DESIGN.md and flows 01–12's patterns

The Spec names the rules each variation changes:
- § Dashboard page (no role);
- § The NavMenu's personal links;
- § Detail page and the title rule;
- § Form page;
- § Button system's destructive action;
- § Table system;
- § Account / auth page's static pages.

Round 2 must add text for:
- the end section's two actions as `.role-row`s (flow 12's role list put to a new use);
- the record links (`.record-links`), a new list pattern;
- whether Mark complete is a destructive action (Q24);
- the Unavailable pattern for the export and the profile (S12, Q19).

The CSS contract (Obs):
- **Colours are tokens:** no hex outside `tokens.css`.
- **No Bootstrap classes.**
- **Every class is defined,** in the bundle or the helmet.
- **Two board rules would break system rules if copied:** the Save row's flex-start (S7) and the inline styles (S9).

## 8. The operator's decisions still needed

Q1–Q16 were decided on 2026-10-27 (brief § 6), and the picks above follow them. Nothing in them is reopened except
where a board argues against one on a system rule (Q17, Q18, Q20, Q22, Q23). Each question below has a recommendation.

17. **Q17 (reopens Q5's header for her). Export's h1 for her own.** Drawn: "Export portfolio" on every board. Q5 said
    "Export your portfolio", but DESIGN.md makes a nav label its page's h1 and tab (`NavMenuAuthorizationTests`), and
    "Export portfolio" is a menu item (the Trainee's, and hers under Q2). *Recommend:* keep "Export portfolio" as her
    h1, with "your" in the subtitle and the result. The staff header stays "Export Lerato Molefe's portfolio", which is
    no menu item.
18. **Q18 (reopens Q5's placement). Where Export's result shows.** Q5 said under the form; V1 draws it above, as
    DESIGN.md § Form page does. *Recommend:* above, in `ActionResult`, focused, naming the file.
19. **Q19 (new). The export's refusal page.** Drawn: V1 today's sentence under "Export portfolio"; V2 Page not found;
    V3 "You cannot export this portfolio". *Recommend:* DESIGN.md's Unavailable pattern. The h1 is "Portfolio
    unavailable", with "This portfolio does not exist, or you cannot export it." and Go to Home, one page for every
    cause. The command's "You are not authorized to export this portfolio." stays its refusal, behind the page (A2).
20. **Q20 (reopens Q6's words). Verify's answer and not-a-hash.** Drawn: an `<h2>` "Export verified", "Wombat generated
    this PDF: its content is byte for byte an export Wombat recorded.", then a details list: First generated, Exported
    (n times, the last on …), Trainee, First exported by, Filter, File. The not-a-hash refusal reads "This is not a
    SHA-256 hash: it has 12 characters, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f). The 12 in a
    file's name are only its start." *Recommend:* the board's, which is clearer than Q6's sentence and fixes its
    misreading "it has 64 characters". Use Q6's help words for the field, adding the commands. The failure drops
    "Nothing is wrong with the file on that account." (S5).
21. **Q21 (new). The day's check before the dialog** (V1, PROPOSED). *Recommend:* yes. Mark complete… and Deactivate…
    check the day on the press and refuse under the field with no dialog. The command still refuses at confirm, with
    the same words in the same place. 5.16's Expect changes: no dialog for the story's day, and "The last day cannot be
    after today (2026-10-26)." under the field, focused, the day kept.
22. **Q22 (reopens Q8's pager). The closed list's page size.** Q8 said "50 (flow 08's)". Flow 08's pager starts at 20
    (flow 12's C14, `W/CommitteeDecisions/DecisionsDue.razor:338`), and flow 12 took 20 (its Q22). The board draws 20.
    *Recommend:* 20.
23. **Q23 (reopens Q1's link names). Home's four links.** Drawn: each page's title (My progress, My authorisations, My
    activities, Export portfolio), each with a line saying what it holds. Q1 named them for what they hold.
    *Recommend:* the page titles with the line, so a link reads as the h1 it lands on. Dr du Plessis's no-STAR line
    stays as text with no link (Q1's "only when she holds any").
24. **Q24 (new). Mark complete's confirm button.** Drawn: filled danger. Built: primary (`DangerAction="false"`), with
    Deactivate's danger. *Recommend:* as built. Mark complete is the programme's good end; the danger tint stays for
    the withdrawal. Both dialogs keep "cannot be undone".
25. **Q25 (new). A former trainee who holds a staff role** (S1-A's Assessor). Drawn: an info line on her Assessor Home
    naming the record, with Open My progress. *Recommend:* no line. Q1 keeps that role's Home, and under Q2 her
    personal links already carry My progress and Export portfolio.
26. **Q26 (new). The ended profile's "The account" section.** Drawn: "Lerato Molefe holds no role now, and can still
    sign in to read the record and export the portfolio." with "Open Lerato Molefe's user page". *Recommend:* keep it,
    PROPOSED. It is the way to the page where a staff role can be given (Q15 keeps re-admission out). Add "and download
    the STAR certificates" under Q3.
27. **Q27 (new). The staff export's trail.** Q5 said "the trail from the page it was opened from"; the Spec proposes a
    fixed owner (Trainees for the Institutional admin; Programme trainees for the Coordinator, the Committee member
    and both speciality admins). *Recommend:* the Spec's fixed owner, with no return parameter. Mr Smit's export opened
    from the review then reads "Home › Programme trainees › Lerato Molefe › Export portfolio", a crumb that opens her
    programme page, which reads an ended profile.

## 9. What round 2's ask must require

- **R1. The Spec and Steps boards.**
  - **The Spec board:**
    - every NEW and changed component with its classes (`.record-links`, `.ended-line`, the export's staff header, the
      Unavailable page for the export, `.verify-answer` with its `.receipt`, `.cmd-list`, `.hash-value`, the end
      section on `.role-rows`, the reused `.user-page` scope);
    - the 390 scope that gives each page 44 px (S14);
    - the DESIGN.md text each changes;
    - the tokens;
    - every DATA read in § 7;
    - every message by kind, the built words and the NEW ones marked (S6).
  - **The Steps board:**
    - each of the 26 Expects with its new words, plus 5.8, A.1.6, A.2.1, A.2.9 and A.4.6 (M5);
    - each journey's count at 1280 and 390: a former trainee downloading a STAR certificate from Home; exporting her
      portfolio; staff exporting a named registrar's; verifying a file; marking a programme complete; recording a
      withdrawal.
- **R2. The picks at fidelity:**
  - S1-A with Q2's menu;
  - S2-A without the user-page link;
  - Export V1;
  - Verify V1;
  - the end section V1;
  - S6 and S7 as corrected.
- **R3. The states not yet drawn** (§ 1), each at 1280 and 390, and 768 for the profile and the closed list. My
  authorisations as a former trainee, with Download and du Plessis's empty card (Q3).
- **R4. The corrections:**
  - M1–M7 (must);
  - S1–S15, A3 and A4 (should);
  - the nits as one pass.
- **R5. Real values:**
  - D = 2026-10-26, J = 2026-01-15;
  - the review "Lerato Molefe · 2026 S2", ratified 2026-10-26;
  - Dr du Plessis's last decision, Satisfactory with Observations, Annual progression review, ratified 2026-10-26;
  - the files portfolio-aba6ce7943c2.pdf (22:51 SAST, 3 exports) and portfolio-344dd52bdded.pdf (22:56 SAST, once);
  - the hash aba6ce79…c995;
  - PAED-010 and PAED-012 three a semester;
  - expected completion 2027-01-14 and 2029-01-14;
  - the Mini-CEX of 2026-10-25.
- **R6. Q1–Q16 drawn as decided, and Q17–Q27 as the operator decides.** Nothing of flows 01–12 is drawn as changed
  beyond what names the end or links the record. That covers My progress's and the EPA page's own layout (S13), My
  activities' table (S13), the user page, and Trainees' Waiting and Current lists.
- **R7. The attachments.** Re-attach § 4's twelve key screenshots and name the folder (`design/baseline/`). Ask round 2
  to say whether they arrived: round 1 does not say.

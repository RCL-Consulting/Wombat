# Act 5 — Year 4: graduation

**Scenario date:** late December 2026, a fortnight after Act 4's sitting and in Dr Molefe's last month of training: her
programme ends on 14 January 2027, the expected completion Act 2 recorded. The date is narrative only (see "The clock"
below).

**Who acts:**
- **Mr Pieter Smit** (Coordinator) schedules the final review and reproduces the portfolio export.
- **Dr Thandi Zulu** (CommitteeMember, chair) sits the Paed Annual Review Panel with **Dr David Naidoo**, **Dr Sarah
  Botha** and **Dr John van Rensburg**. Dr Naidoo also rates one last assessment as an Assessor.
- **Dr Lerato Molefe** (Trainee, then a former trainee) graduates.
- **Prof Nolwazi Mbatha** (InstitutionalAdmin) exports the portfolio and records the end of two programmes.
- **Dr Pieter du Plessis** (Trainee) leaves the programme without completing it.
- **An anonymous verifier** checks the portfolio PDF. This is a credentialing officer at the hospital that will employ
  Dr Molefe, and has no Wombat account.

**Why:** a programme ends in one of two ways, and Wombat has to record both. At the end of year 4 the committee judges
Dr Molefe against the exit rule, decides that she graduates, and entrusts her with every EPA at its exit level. Her
portfolio is what she takes with her, so anyone who receives it must be able to check that Wombat produced it. After
her programme ends she keeps a read-only record. A registrar who leaves part-way is recorded as leaving. An encounter
after his last day counts towards nothing, including one that had already counted.

**Starting state (what Acts 1–4 left):**
- KGK has adopted `Paediatric EPA Curriculum` 11.1. The Paed Annual Review Panel has Dr Zulu as chair and Drs Naidoo,
  Botha and van Rensburg as members, all active CommitteeMembers at KGK. Dr Naidoo also holds Assessor.
- The five registrars are admitted and current. Their programmes started on `J−3y` (Molefe), `J−2y` (Dlamini), `J−1y`
  (du Plessis) and `J` (Mahlangu and Ndlovu), Act 2's anchor: 15 January 2023 to 2026 on a replay in 2026.
- From Act 3, Dr Molefe holds six completed WBAs, on PAED-001, 010 and 012, and the MSF campaign that Mr Smit
  released, which covers PAED-010 and 012.
- Act 4 sat a review for each registrar, recorded and ratified each decision, and resolved Dr Mahlangu's appeal. No
  review is left open. Dr Molefe holds the three STARs Act 4 issued: PAED-001 and PAED-012 at `5`, their exit level,
  with no expiry, and PAED-010 at `4`, expiring 20 days after Act 4's sitting. Her other twelve EPAs are undecided: nine
  that Act 4 deferred, and PAED-008, 009 and 013, which it did not decide.
- To start here, restore `recovery/scenario-post-act4.dump`.

**The clock:**
- Every typed date is relative to the replay day `D`. The committee sits for the period that holds `D`.
- `J` is the 15 January Act 2 admitted from: the latest 15 January on or before `D`, if none has passed since Act 2
  was replayed. Dr Molefe's programme started on `J−3y` (15 January 2023 on a replay in 2026), and every export and
  evidence window below starts there.
- The story's graduation day, her expected completion `J+1y−1d` (14 January 2027 on a replay in 2026), is typed only
  once, in Step 5.16, to show that it is refused: it is always after `D`. Her last day is recorded as `D`.
- Act 2 wrote every programme start from `J`, so Dr Molefe reads training year 4. If a 15 January has passed since Act
  2 was replayed, she reads year 5. The curriculum sets no level for year 5, so each EPA's exit level stands in
  as its target (T139).
- D49 exempts the period a programme ends in only when it ends before that period's last month: June for semester 1,
  November for semester 2 and for the academic year (`QuotaWindow`). If `D`, or Dr du Plessis's `D−2`, falls in June,
  the semester keeps its full target and the year does not; in November or December, both keep it. Steps 5.20 and 5.28
  then read a target where they say "no target".

**Goal:**
1. Mr Smit schedules Dr Molefe's pre-graduation review, and the panel sits it, records the decision `Graduate` and
   ratifies it. She then holds an active STAR at its exit level on each of the 15 EPAs.
2. Dr Molefe, Prof Mbatha and Mr Smit each export her portfolio PDF, and the three files are identical byte for byte.
   An anonymous verifier can tell a genuine copy from a tampered one.
3. Her programme is marked complete on `D`. The Trainee role is removed and she is emailed. She keeps a read-only
   record and can still export it.
4. Dr du Plessis's programme ends on `D−2`, and an encounter he had after that day stops counting.

## Phase 5.A — The final review

### Step 5.1 — Mr Smit reads what Dr Molefe's committee still has to decide
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due
Do: Open Decisions due for the period holding `D`, set the status filter to every status, and read Dr Molefe's rows.
Expect: There is one row for each EPA due for her in the period, and each row gives its status in words:
  - an EPA that an Act 4 STAR decided reads Decided, and names the STAR and the review that issued it;
  - an EPA that Act 4 deferred reads Deferred, and says that a later sitting plans it again;
  - the others read Not scheduled, Due by year end or As opportunity allows.
  Every row that is not yet decided offers Schedule. KGK's other registrars are listed too, and nobody outside KGK is.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Menu Home, Programme trainees, Waiting for assessors,
  Decisions due, MSF campaigns, Committee reviews, Decision panels, Data rights requests, My data rights. Decisions
  due opened on 2026 S2; Every status: "75 of 75 decisions due in 2026 S2 shown." for "the 5 trainees you oversee at
  Kgosi Kgari Teaching Hospital" (Dlamini, Molefe, Mahlangu, du Plessis, Ndlovu; nobody else). Molefe's 15 rows:
  PAED-001, 010 and 012 Decided ("STAR #1/#2/#3, issued at review #1.", Open review); nine Deferred ("Deferred at
  review #1. A later sitting for 2026 S2 [or 2026] plans it again.", Open review and Schedule); PAED-008, 009 and 013
  As opportunity allows (Schedule). All 71 undecided rows of the 75 offer Schedule.
Gap: none

### Step 5.2 — Mr Smit schedules Dr Molefe's pre-graduation review
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Follow Schedule on one of Dr Molefe's outstanding rows. If Act 4 decided every EPA that is due, use Schedule review
  on Committee reviews instead. Then complete the form:
  - review type: Pre-graduation review;
  - evidence window: widened to start on her programme start, `J−3y`;
  - scheduled on: `D`;
  - Formative only: unticked.
  Create the review.
Expect: The form opens with the Paed Annual Review Panel, Dr Molefe and the period already filled in. The type offers
  Annual progression review and Pre-graduation review. Entrustment-only review is offered only in a semester-1 period.
  The agenda preview lists the EPAs that are due and names those Act 4 decided. In the same semester-2 period as Act 4:
  - it reads "12 EPAs will be on the agenda for …", with the period's label;
  - it names the nine EPAs Act 4 deferred as to be decided, or deferred with a reason, before the review is ratified;
  - it lists PAED-008, 009 and 013 as As opportunity allows;
  - it says "Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012."
  The new review reads Pre-graduation review, Summative and Scheduled, with the evidence window running from `J−3y` to
  the end of the period. Mr Smit is offered Start review, but leaves it to the chair.
Note: A second binding review for a period is refused only while the first one is open (scheduled, in progress or
  decided). Act 4's review of her has been ratified.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Schedule on Molefe's PAED-006 row opened
  /committee/reviews?panel=1&trainee=…&period=2026-2 with Paed Annual Review Panel, Lerato Molefe and "2026 S2 · 1 Jul
  to 31 Dec 2026" filled, window 2026-01-01 to 2026-12-31. Type offers Annual progression review and Pre-graduation
  review only; Formative unticked. Preview: "12 EPAs will be on the agenda for 2026 S2.", "PAED-002, PAED-003, …,
  PAED-014 and PAED-015 must be decided at this sitting, or deferred with a reason, before it is ratified.",
  PAED-008/009/013 "· As opportunity allows", "Already decided in this window, so not on the agenda: PAED-001,
  PAED-010 and PAED-012." With from 2023-01-15, scheduled 2026-10-05, Pre-graduation, Formative unticked, created
  review #7: Pre-graduation review, Summative, Scheduled, evidence window 2023-01-15 to 2026-12-31; Start review
  offered to Mr Smit, left unpressed.
Gap: none

### Step 5.3 — Dr Zulu starts the review and reads where Dr Molefe stands
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's pre-graduation review from Committee reviews and start it. Read the evidence snapshot, the agenda
  and "Entrustment against Annexure A".
Expect: The page says "Review started." and the review reads In progress.
  - The snapshot freezes her evidence observed since `J−3y`, grouped by EPA, including the released MSF
    campaign.
  - The standing reads training year 4, and "2 at or above · 1 below · 12 with no decision, of 15 EPAs". Its exit rule
    reads "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)", followed by "Not
    yet: …", which names every EPA but PAED-001 and PAED-012.
  - Under the rule: "For information only. Recording a Graduate decision or completing the programme does not check the
    exit rule."
  - Record decision is shown disabled, and names the closing agenda lines that are still to be staged or deferred.
  - Sampling warnings may name EPAs rated by fewer than three assessors. They are warnings and refuse nothing.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Menu Home, Programme trainees, Committee reviews, Decision
  panels, My data rights ("Acting as Committee member", Switch to Assessor). Opened review #7 from Committee reviews
  (Pre-graduation, Scheduled) and pressed Start review: "Review started.", State In progress. Snapshot frozen and
  grouped by EPA, the released MSF campaign (Default MSF #1, records #22 and #23) included. Standing: "In training
  year 4 on 2026-10-05", "2 at or above · 1 below · 12 with no decision, of 15 EPAs", "2 of 15 EPAs at their exit
  level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6). Not yet: PAED-002, …, PAED-015" (all but 001 and 012),
  then "For information only. Recording a Graduate decision or completing the programme does not check the exit rule."
  Record decision disabled, naming the nine closing lines. Sampling warnings on PAED-010 (1 assessor) and PAED-012
  (2), refusing nothing.
Gap: none

### Step 5.4 — Dr Zulu stages a STAR at its exit level on every EPA not yet there
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: For each EPA the standing names under "Not yet", stage a decision at its exit level. That is thirteen EPAs, every
  one but PAED-001 and PAED-012, which Act 4 decided at their exit level:
  - rung `5` on PAED-002 to 007 and PAED-010;
  - rung `4` on PAED-008, 009, 011, 013, 014 and 015.
  Use Stage on the EPA's agenda line. For PAED-010, which Act 4 decided in this window below its exit level, choose it
  in the staging form's EPA list instead. Issue each decision on `D` with no expiry, write a rationale,
  and tick the snapshot lines it rests on, choosing that EPA's own lines where it has some. Defer nothing.
Expect: The level list offers only the six rungs of the CPSA Paediatric Entrustment Scale v11.1 (`1`, `2`, `3a`, `3b`,
  `4`, `5`). Stage pending decision stays disabled until a line is ticked. A decision that rests on no line about its
  own EPA, or on a single line, gets a hint but is not refused. After each staging:
  - the page says "Pending entrustment decision staged.";
  - the EPA's line reads Staged, and an EPA that was not on the agenda gains a chair's line;
  - the decision is listed with its level, its issue date and the lines it rests on.
  Once every closing line is staged, Record decision is enabled.
Note: The exit levels are v11.1's final minima: nine EPAs at `5` and six at `4` (T166).
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Staged 13 decisions on review #7, all issued 2026-10-05 (the
  form's default) with no expiry: rung 5 on PAED-002 to 007 and 010, rung 4 on PAED-008, 009, 011, 013, 014 and 015.
  Twelve via Stage on the agenda line ("PAED-002's lines are listed first."); PAED-010 via the EPA list. The level
  list offered exactly 1, 2, 3a, 3b, 4, 5; Stage pending decision was disabled ("Name at least one item of the
  evidence snapshot.") until a line was ticked. The twelve without own lines rest on Default MSF #1 and got both hints
  ("Only one item is named. …" and "None of the named items is about PAED-002."), not refused; PAED-010 rests on its
  own DO #17 and MSF #22 (no hint). Each staging: "Pending entrustment decision staged."; every agenda line reads
  Staged; PAED-010 gained a line (agenda "13 EPAs for 2026 S2", "Already decided in this window, so not on the agenda:
  PAED-001 and PAED-012."); each decision is listed with its level, "issued 2026-10-05" and "Rests on …". Record
  decision then enabled.
Gap: none

### Step 5.5 — The panel records its decision: Graduate
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Record the committee's decision:
  - category: Graduate (programme complete);
  - rationale: every EPA is entrusted at its exit level, and the panel recommends completion;
  - conditions: none;
  - present: Dr Zulu, Dr Naidoo, Dr Botha and Dr van Rensburg.
Expect: The category list opens with nothing chosen and offers Graduate (programme complete) among the progression
  outcomes. Dr Zulu is ticked as present and cannot be unticked. The review then reads Decided (T250). The decision is
  headed "Graduate (programme complete)" and names the four members present. The page says that the staged decisions
  are now fixed and that ratifying issues exactly these. The staging form is gone, and only Dr Zulu is offered Ratify.
Note: Recording a Graduate decision checks nothing against the exit rule. T166 deferred that check until the College
  confirms the rule.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Category opened on "Select a category…" and offered
  Satisfactory Progress, Satisfactory with Observations, Inadequate Progress — Additional Training, Inadequate
  Progress — Repeat, Release from Training, Outcome Deferred and Graduate (programme complete). "Thandi Zulu (chair)"
  was ticked and disabled. Recorded Graduate with the rationale, no conditions, all four present: "Decision
  recorded.", State Decided, decision "Graduate (programme complete)", "Present: Thandi Zulu (chair), David Naidoo,
  Sarah Botha, John van Rensburg (external)". Pending list reads "Fixed when the committee's decision was recorded:
  ratifying issues exactly these. …"; the staging form is gone. Ratify offered to Dr Zulu; Dr Naidoo sees no Ratify,
  only "Only the panel's chair, Thandi Zulu, can ratify the committee's decision."
Gap: none

### Step 5.6 — Dr Zulu ratifies
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Ratify the decision.
Expect: The page says "Decision ratified." and the review reads Ratified. Every staged decision is issued as a STAR in
  the same action:
  - each agenda line reads Decided;
  - the list of pending decisions is empty;
  - Act 4's active STAR on PAED-010, which was re-decided today, is superseded.
  The standing's exit rule reads "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of
  6). Every EPA is at its exit level." Its year line reads "15 at or above · 0 below · 0 with no decision, of 15 EPAs".
Note: Only the panel's chair ratifies. There is no Administrator bypass (D46, T165).
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Ratify: "Decision ratified.", State Ratified. The 13 staged
  decisions were issued as STARs #6–#18 (PAED-010's is #18); every agenda line reads "Decided STAR #n."; the pending
  list reads "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
  one." Act 4's PAED-010 STAR #2 (level 4, expiry 2026-10-25) now has Status 4, superseded (SQL). Standing: "15 at or
  above · 0 below · 0 with no decision, of 15 EPAs" and "15 of 15 EPAs at their exit level by STAR decision (level 5:
  9 of 9 · level 4: 6 of 6). Every EPA is at its exit level." No mail (stub count 31 → 31).
Gap: none

### Step 5.7 — Prof Mbatha reads Dr Molefe's STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from her dashboard's quick links. Filter by the trainee name "Molefe", first with the
  status Active and then with All, applying the filters each time.
Expect: There are fifteen Active rows, one for each EPA from PAED-001 to PAED-015. Each row:
  - is at its EPA's exit level;
  - was issued on `D`, except PAED-001's and PAED-012's, which Act 4 issued;
  - has no expiry;
  - offers Download and Revoke.
  With All, Act 4's PAED-010 STAR, which Step 5.6 superseded, reads Superseded and offers Download only. She revokes
  nothing.
Note: If PAED-010's Act 4 expiry passed and the expiry job ran before Step 5.6, that STAR reads Expired instead, and
  ratifying superseded none.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Home's quick links: Invite a person, Users, Invitations,
  Curriculum adoptions, Entrustment decisions. Trainee "Molefe", Status Active, Apply filters: 15 rows, PAED-001 to
  PAED-015, level 5 on 001–007, 010, 012 and 4 on 008, 009, 011, 013–015; all issued 2026-10-05 (Act 4 was played on
  this replay's D too, so PAED-001's and 012's issue day is also D); Expires "—"; each with Download and Revoke.
  Status All: 16 rows; the extra is PAED-010 at 4, issued 2026-10-05, expires 2026-10-25, Superseded, Download only.
  Nothing revoked.
Gap: none

### Step 5.8 — Dr Molefe reads her final review and keeps a STAR certificate
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → / → /portfolio/authorisations
Do: Open My committee reviews and view the pre-graduation review. Then open My authorisations from her Home's card (its
  Open My authorisations), and download the certificate for PAED-001.
Expect: The review is listed as Pre-graduation, Ratified, Graduate (programme complete). Its detail shows:
  - the review type;
  - the decision and its rationale;
  - who was present;
  - an agenda in which each EPA reads Decided, with its STAR number.
  The Lodge appeal form is offered, and she lodges no appeal. My authorisations holds 15 cards, each with its level, its
  issue date and "No expiry". The certificate downloads as `star-certificate-PAED-001-<id>-<8 hex digits>.pdf`.
Note: My authorisations requires the Trainee role, so this is the last point at which she can download a certificate
  from that page (see Step 5.22).
Actual (2026-10-05, T358 replay, wombat_scenario_t358): My committee reviews lists two: "2026 S2 · 2023-01-15 to
  2026-12-31, Pre-graduation, Ratified, Graduate (programme complete)" and Act 4's annual one. View shows "Type:
  Pre-graduation review", "State: Ratified", the decision and rationale, "Present when this decision was taken: Thandi
  Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)", and an agenda of the 13 EPAs it decided,
  each "Decided STAR #n." (#6–#18). Lodge appeal form offered, not used. Home (Your targets "1 of 10" / "0 of 5",
  Furthest short with ISO "3 more by 2026-11-30", Needs you, Recent decisions, My authorisations "15 at or above") →
  Open My authorisations → /portfolio/authorisations: 15 cards, each with level, "Issued 5 October 2026" (a long date:
  this page is not a flow-05 surface), "No expiry" and Download certificate. PAED-001's downloaded as
  star-certificate-PAED-001-1-63a9f4ee.pdf (96,897 bytes).
Gap: none

## Phase 5.B — The portfolio and its verification

### Step 5.9 — Dr Molefe exports her portfolio
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/export
Do: Open Export portfolio from the menu. Set the from date to her programme start, `J−3y`, and the to date to `D`, and
  export the PDF.
Expect: The page opens with the last twelve months filled in. After the export it says "Portfolio exported
  successfully. The download should start automatically.", and a file named `portfolio-<12 hex digits>.pdf` downloads.
  Record the file name: Steps 5.10 and 5.12 must produce the same one.
Note: Every export in this phase is made on `D` with the same two dates. The PDF carries no generation time, so the
  same data gives the same bytes (T078).
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Export portfolio from the menu opened with From 2025-10-05 and
  To 2026-10-05 (the last twelve months). With 2023-01-15 to 2026-10-05, Export PDF: "Portfolio exported successfully.
  The download should start automatically." and portfolio-c47ccaad4d32.pdf downloaded (183,527 bytes; SHA-256
  c47ccaad…9f60, recorded for 5.10, 5.12 and 5.13).
Gap: none

### Step 5.10 — Prof Mbatha exports Dr Molefe's portfolio by its address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId} → /portfolio/export/{TraineeUserId}
Do: Find Dr Molefe in Users and open her. Take her user id from the page's address, then type the export address with
  that id. Set the dates to `J−3y` and `D`, and export.
Expect: The export succeeds as in Step 5.9. The downloaded file has the same name as Dr Molefe's and is identical to it
  byte for byte. Compare the two with `fc /b`, or by their SHA-256.
Note: No page links to the staff export, so this step reaches it by typing the address. Record that in `coverage.md`.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Users → filter "Molefe" → Manage opened
  /admin/users/49a8e028-a15a-4500-81ca-8d883f365212. Typed /portfolio/export/49a8e028-…: the page opened with
  2025-10-05 to 2026-10-05 and reads only "Generate a PDF export of the trainee's portfolio.", naming nobody, before
  and after. With 2023-01-15 to 2026-10-05: "Portfolio exported successfully. …" and portfolio-c47ccaad4d32.pdf,
  byte-identical to Dr Molefe's (cmp; the same SHA-256).
Gap: [F-5.10a, T314] (still) The staff export page never says whose portfolio it exports:
  /portfolio/export/{TraineeUserId} names no trainee before or after the export, so a wrong id exports another
  trainee's portfolio without warning.

### Step 5.11 — Prof Mbatha reads the portfolio
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: n/a
Do: Open the PDF from Step 5.10 and read it through.
Expect: Every page carries the same header and footer:
  - header: Kgosi Kgari Teaching Hospital, "Portfolio Export", Lerato Molefe, Paediatric EPA Curriculum, "Paediatrics —
    Paediatrics" (speciality and sub-speciality), and "Period: `J−3y` to `D`";
  - footer: "Page n of m | Generated by Wombat", with no generation time.
  The sections follow in this order:
  - a Summary, with activities by type and the number of committee reviews;
  - "Statements of Awarded Responsibility (STARs)", reading "15 active entrustment decisions on record." and giving each
    EPA's rung by its label;
  - "Progress per EPA", read on `D` in training year 4;
  - "Committee Decisions", listing each ratified review. The final one reads Pre-graduation review, Ratified, "Graduate
    (programme complete)", with its rationale and the four members present by name;
  - her activities by type, with rungs shown by their labels and people by their names;
  - the MSF campaign;
  - "Appendix — Audit Trail".
Note: In "Progress per EPA", the periods before the scenario's evidence (which starts in Act 3) read short. That comes
  from the scenario's data and is not a gap.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): The 11-page PDF carries on every page the header Kgosi Kgari
  Teaching Hospital, "Portfolio Export", Lerato Molefe, Paediatric EPA Curriculum, "Paediatrics — Paediatrics",
  "Period: 2023-01-15 to 2026-10-05", and the footer "Page n of 11 | Generated by Wombat"; no generation time
  (metadata dates fixed at 2000-01-01). Sections in order: Summary (activities by type, "Committee reviews: 2", "MSF
  reports: 1"); "Statements of Awarded Responsibility (STARs)", "15 active entrustment decisions on record.", rungs by
  label; "Progress per EPA" "on 5 October 2026 (training year 4)" (the PDF keeps long dates, D1); "Committee
  Decisions" with Act 4's review, then the Pre-graduation review, Ratified, "Graduate (programme complete)", its
  rationale and the four present by name; Activities by type; Multi-Source Feedback; "Appendix — Audit Trail".
  Rendered pages 1, 5 and 6 are the captures.
Gap: none

### Step 5.12 — Mr Smit reproduces the export
Role: Coordinator — Mr Pieter Smit
Route: /portfolio/export/{TraineeUserId}
Do: Type the export address with Dr Molefe's user id, which Prof Mbatha sends him. Set the same two dates, and export.
Expect: The export is allowed, because a coordinator of her institution oversees her (T079, T101). The file has the
  same name as those from Steps 5.9 and 5.10, and is identical to both byte for byte.
Note: No page that a coordinator can open links to a trainee's export. Her user id is otherwise only in an address he
  has seen: the `trainee=` part of the Schedule link he followed in Step 5.2.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Typed /portfolio/export/49a8e028-a15a-4500-81ca-8d883f365212
  as Mr Smit: admitted ("Generate a PDF export of the trainee's portfolio.", no name). With 2023-01-15 to 2026-10-05:
  "Portfolio exported successfully. …" and portfolio-c47ccaad4d32.pdf, byte-identical to Steps 5.9 and 5.10 (cmp).
Gap: [F-5.10a, T314] (still) The page names no trainee.

### Step 5.13 — A verifier checks the genuine PDF
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: In a private window, without signing in, open the verification page. Compute the SHA-256 of the PDF that Dr Molefe
  sent (`certutil -hashfile <file> SHA256`, or `sha256sum`, both of which print it in lower case), enter it and press
  Verify. Do the same for Mr Smit's copy.
Expect: The page works signed out, as a static page, under the signed-out bar: the brand and Sign in, and no menu.
  Verifying loads the page's own address with the hash in it. The
  page reads "Export verified" and shows:
  - when Wombat generated the file, in UTC;
  - the trainee and the exporter as user ids, never as names (DESIGN: the anonymous page must not disclose a name);
  - "Filter: `J−3y` to `D`";
  - the file name that all three downloads carry.
  Mr Smit's copy has the same SHA-256 and gets the same answer.
Note: The PDF prints no hash, though the field's placeholder says "from the PDF footer" (T287). The file name carries
  only the first 12 hex digits, and those alone match nothing. The check needs the file's whole SHA-256.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Signed out, /portfolio/verify opened under the signed-out bar
  (Wombat and Sign in, no menu) as a static GET form with the placeholder "Enter the hash from the PDF footer". Dr
  Molefe's SHA-256 (lower case) → /portfolio/verify?hash=c47ccaad…&check=1: "Export verified", "This PDF was generated
  by Wombat on 2026-10-05 14:43 UTC.", "Trainee ID: 49a8e028-…", "Exported by: 49a8e028-…" (ids only), "Filter:
  2023-01-15 to 2026-10-05", "File: portfolio-c47ccaad4d32.pdf". Mr Smit's copy got the same answer. The same hash in
  upper case read "No matching export found. …".
Gap: [F-5.13a, T313] (still) The verify page matches the hash case-sensitively: the genuine file's SHA-256 in upper
  case, as Windows' Get-FileHash prints it, reads "No matching export found". [F-5.13b, T287] (still) The placeholder
  says "Enter the hash from the PDF footer" though the footer prints none.

### Step 5.14 — The verifier checks a tampered copy
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: Change one byte in a copy of the PDF, compute the copy's SHA-256 and verify it. Then press Verify with only spaces
  in the field.
Expect: The tampered copy gets "No matching export found. This hash does not correspond to any PDF generated by this
  system." The press with only spaces gets "Enter the content hash to check." Neither leads to an error page.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): A copy with one byte changed (SHA-256 dbb89d20…) → "No
  matching export found. This hash does not correspond to any PDF generated by this system." Five spaces →
  ?hash=+++++&check=1: "Enter the content hash to check." Neither reached an error page; the page stayed signed out.
Gap: none

## Phase 5.C — The programme ends

### Step 5.15 — Dr Molefe leaves My progress open
Role: Trainee — Dr Lerato Molefe
Route: /account/login → /account/login/submit → / → /portfolio/progress
Do: In her own browser, sign in and open My progress. Leave the tab open through Step 5.19.
Expect: The page shows her programme as still running:
  - "This period", with "1 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in 2026";
  - Your EPAs, each EPA's count for the window containing `D` with its bar, and every STAR "At or above", at 5 or 4;
  - "Entrustment against Annexure A", "15 at or above · 0 below · 0 with no decision, of 15 EPAs", with the exit rule
    met as in Step 5.6: "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every
    EPA is at its exit level.";
  - the period before each window, and her rating trajectories, on each EPA's page (T355), not on My progress.
  Nothing says that her programme has ended.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Dr Molefe signed in in her own browser and opened My progress
  from the menu (the tab was held open through Step 5.19). "Training year 4 · Semester 2, 2026"; This period "1 of 10
  EPAs met this semester", "0 of 5 EPAs met in 2026", "Semester 2, 2026 ends on 2026-11-30."; Your EPAs: 15 rows with
  their bars (15), the window holding D ("3 of 3 this semester" / "Target met for Semester 2, 2026." on PAED-001, "3
  more by 2026-11-30" elsewhere), every STAR "At or above 5" or "At or above 4", each code a link to its page.
  "Entrustment against Annexure A": "15 at or above · 0 below · 0 with no decision, of 15 EPAs" and "15 of 15 EPAs at
  their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level." No
  trajectory chart and no period before on My progress (they are on the EPA pages). Nothing says her programme has
  ended.
Gap: none

### Step 5.16 — Prof Mbatha tries to record the story's graduation day
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Molefe's profile from Active profiles. Set the last day in the programme to the story's graduation day,
  `J+1y−1d`. Press Mark complete and confirm.
Expect: The last-day field opens on `D`. Among the rest, its help says that:
  - Mark complete records the graduation day, archives the profile and removes the Trainee role;
  - the day cannot be after today;
  - the day cannot be changed afterwards.
  The confirmation names `J+1y−1d` (as yyyy-MM-dd) as her graduation day. It says that the Trainee role will be
  removed, a graduation email will be sent, encounters after that day will count towards nothing, and none of this can
  be undone. Once
  confirmed, the request is refused with "The completion date cannot be after today (`D`)." (`D` shown as yyyy-MM-dd),
  which is read with the field. The profile is still Active.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Trainees → Edit Lerato Molefe (/admin/trainees/edit?id=2).
  "Last day in the programme" opened on 2026-10-05; its help reads "Mark complete records it as the graduation day,
  archives the profile and removes the Trainee role. … It cannot be after today, and it cannot be changed afterwards.
  …". Set to 2027-01-14, Mark complete: the dialog "Mark this programme complete?" asks "Record 2027-01-14 as Lerato
  Molefe's graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent.
  Encounters observed after that day will count towards nothing in this programme, including any already counted. The
  day cannot be changed afterwards, and this cannot be undone." Confirmed: "The completion date cannot be after today
  (2026-10-05)." (programme-end-refusal, in the field's aria-describedby); Status still Active.
Gap: none

### Step 5.17 — Prof Mbatha marks Dr Molefe's programme complete on `D`
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees/edit → /admin/trainees → /admin/users/{UserId}
Do: Set the last day to `D`, press Mark complete and confirm. Then go back to Trainees, and open Dr Molefe in Users.
Expect: The page says "Trainee marked complete. The Trainee role has been removed and a graduation email sent."
  - The summary reads Status Completed, Completed `D`.
  - The last-day field, Deactivate and Mark complete are gone. The page's subtitle reads "The record of a programme
    that has ended.", and the profile's details read as a list under "This programme has ended, so its record is
    archived and cannot be changed.": Curriculum Paediatric EPA Curriculum (11.1), Programme start date `J−3y`,
    Expected completion date `J+1y−1d`. There are no inputs and no Save profile; Back to trainees stays (T305).
  - On Trainees she is no longer under Active profiles. She is listed under Completed & closed profiles with the outcome
    "Completed `D`".
  - Her user page says "This user has no roles.", and its Add role offers no Trainee (T303).
  - No credit is taken back, because nothing of hers is observed after `D` (T281; Step 5.27 shows a take-back).
Note: The completion runs under the trainee credit lock (T281), so any completion of hers that is in flight is waited
  for first.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Last day 2026-10-05 → Mark complete; the dialog named
  2026-10-05; confirmed (14:44 UTC): "Trainee marked complete. The Trainee role has been removed and a graduation
  email sent." Subtitle "The record of a programme that has ended."; summary "Status: Completed", "Completed:
  2026-10-05". "Profile details" reads "This programme has ended, so its record is archived and cannot be changed."
  over Curriculum "Paediatric EPA Curriculum (11.1)", Programme start date 2023-01-15, Expected completion date
  2027-01-14, as text. No input or select in the page (Deactivate and Mark complete are only in the closed dialogs'
  markup); no Save profile; Back to trainees stays. Trainees: Active profiles lists Dlamini, du Plessis, Mahlangu and
  Ndlovu; "Completed & closed profiles" lists Lerato Molefe, "Completed 2026-10-05". /admin/users/49a8e028-…: "This
  user has no roles."; Add role offers no Trainee ("Trainee is not offered: …"). Nothing of hers is observed after D,
  so nothing was taken back.
Gap: none

### Step 5.18 — Dr Molefe is emailed
Role: System — the graduation email sent by Mark complete
Route: n/a
Do: Read the application log for the mail sent in Step 5.17.
Expect: There is one email to `molefe@kgk.wombat.local` with the subject "Congratulations on completing Paediatric EPA
  Curriculum". It congratulates her on completing the programme as of `D`, and says that her committee has ratified
  her final entrustment decisions and that her portfolio is available in Wombat.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): The log holds one mail after the completion, "Stub email
  b6310e0ea460 (tags: graduation)", subject "Congratulations on completing Paediatric EPA Curriculum": "Dear Lerato
  Molefe, Congratulations on completing your training programme, Paediatric EPA Curriculum, as of 2026-10-05. Your
  committee has ratified your final entrustment decisions and your portfolio of evidence is available in Wombat. …".
  The stub line never names the address; the completion wrote no other mail (stub count 31 → 32). The capture renders
  the log excerpt.
Gap: none

### Step 5.19 — Dr Molefe's open session ends
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /account/session-ended → /account/login
Do: Go back to the tab left open at Step 5.15 and wait up to a minute.
Expect: Removing her role changed her account's security stamp. The session check, which runs once a minute, ends the
  session: the tab reloads to the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Nothing she had open still acts as a Trainee.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): The tab held open since Step 5.15 reloaded 46 seconds after
  Mark complete to /account/login?error=SessionEnded&returnUrl=%2Fportfolio%2Fprogress, reading "Your session has
  ended. Sign in again." as an information notice (alert-info) above an empty sign-in form; nothing of the Trainee
  page remained.
Gap: none

## Phase 5.D — The graduate's record

### Step 5.20 — Dr Molefe signs in again and reads her record
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /account/login → /account/login/submit → /portfolio/progress
Do: Sign in from the page that Step 5.19 left her on. She is returned to My progress. Read it.
Expect: Her trainee record admits her to the page (T252, `TraineeOrFormerTrainee`).
  - An information notice reads "You completed your programme on `D`. This page is your record of it and is read-only:
    no target applies to you any more.", followed by D49's rule. Every date on the page is ISO, `YYYY-MM-DD` (T355, D1).
  - A "Your programme" card gives her start date, Completed `D`, and training year 4 as the year the programme ended.
  - Each EPA's card, its code a link to its page, lists its periods, newest first. The period holding `D` reads "no
    target (your programme ended part-way through) · n recorded", and the earlier periods read as met or short.
  - There are no progress bars.
  - The standing reads the year her programme ended in, "You completed your programme on `D`, in training year 4 …",
    with the exit rule met.
  - Her trajectories are on each EPA's page, which reads as her record does: every period listed, and no "n more by"
    (T355).
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Signing in from the session-ended page returned her to
  /portfolio/progress. Notice: "You completed your programme on 2026-10-05. This page is your record of it and is
  read-only: no target applies to you any more. A period your programme ended in before that period's last month has
  no target, and no period after it is listed." "Your programme": Started 2023-01-15, Completed 2026-10-05, "4 when
  your programme ended". Each EPA card's code links to its page (/portfolio/progress/2 …); periods newest first:
  "Semester 2, 2026 no target (your programme ended part-way through) · 3 recorded" (PAED-001), earlier periods "0 of
  3, 3 short". No progress bars; no long date anywhere. The standing reads "You completed your programme on
  2026-10-05, in training year 4, …", "15 at or above · 0 below · 0 with no decision" and "15 of 15 … Every EPA is at
  its exit level." PAED-001's page lists all eight periods the same way, "Training year 4: level 5, …", Entrustment 5
  "At or above", Issued 2026-10-05, "Exit level 5 · reached", the trajectory ("3 ratings in the 2026 academic year,
  …") and Activities on this EPA (three, 1 item each); no "more by".
Gap: none

### Step 5.21 — What her home page and menu offer a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /
Do: Open Home and read the menu.
Expect: The sidebar has no "Acting as" head, since she holds no role, and the menu offers Home, then My progress and My
  data rights under the rule, and nothing else (DESIGN § The NavMenu, D8, T252). The top bar names her, "Lerato Molefe",
  with Sign out. Home says "You completed your programme on `D`, so no target applies to you any more." and points her
  to My progress, read-only, as the trainee dashboard's Your targets card does for an ended programme (T252 As built:
  "Home says the same"; dates ISO, T355).
Note: The code shows one card, "Your training record", pointing to My progress (T335), but does not say that or when she
  completed her programme (T311).
Actual (2026-10-05, T358 replay, wombat_scenario_t358): No "Acting as" head; the menu is Home, My progress and My data
  rights; the top bar reads "Lerato Molefe" with Sign out. Home shows one card, "Your training record": "You hold no
  role at the moment. Your training record is kept, read-only: your progress in each period, and your portfolio to
  export." with "Open My progress →" (its only link). It does not say that or when she completed her programme.
Gap: [F-5.21a, T311] (still, as the Note records) A graduate's Home does not say "You completed your programme on
  2026-10-05, so no target applies to you any more." (T252 As built: "Home says the same"); its "Your training record"
  card names no completion and offers no export link although it mentions "your portfolio to export".

### Step 5.22 — What a graduate can no longer open
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /committee/my-reviews → /access-denied → /portfolio/authorisations → /access-denied → /msf/my-reports → /access-denied → /activities/new
Do: Type the addresses of My committee reviews, My authorisations and MSF reports in turn. Then type the address of the
  new-activity page.
Expect: Each of the first three reads "You cannot open this page", "Your account holds no role that opens this page."
  and "If you need it for your work, ask your institution's Wombat administrator.", with Go to Home: those pages still
  require the Trainee role (DESIGN § The NavMenu).
  Nothing offers her an activity to file: there is no menu link and no dashboard action. The new-activity page asks
  only that she is signed in, so it opens when she types its address, on Log an activity's instrument picker. She
  files nothing.
Note: No decision says whether a graduate may still file about herself. Nothing refuses her today: the page and
  `ActivityService.CreateDraftAsync` check no role. T281 decides only that an encounter after her last day credits
  nothing.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Typed /committee/my-reviews, /portfolio/authorisations and
  /msf/my-reports: each went to /access-denied?ReturnUrl=…, reading "You cannot open this page", "Your account holds
  no role that opens this page." and "If you need it for your work, ask your institution's Wombat administrator.",
  with Go to Home; the menu lights nothing. Neither the menu nor Home offers an activity. Typed /activities/new: it
  opens on Log an activity's instrument picker ("Choose what you are filing. Each opens its own form.", the KGK
  instruments). Nothing filed.
Gap: [F-5.22a, T311] (still) A graduate can no longer open My authorisations, the only page that downloads her STAR
  certificates, and no decision covers it. [F-5.22b, T153] (still) /activities/new admits a graduate with no role and
  offers her the KGK instruments to file about herself; no decision says whether she may, and nothing was filed.

### Step 5.23 — Dr Molefe exports her portfolio as a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/export → /portfolio/verify
Do: Type the export address, set the dates to `J−3y` and `D`, and export. Then verify the new file's SHA-256
  on the verification page.
Expect: The export page is not in her menu, but it admits her, because the portfolio is her own (`MayReadAsync`). The
  file has a new name. Its "Progress per EPA" now says "The trainee completed the programme on <`D`>", so the content
  differs from Steps 5.9 to 5.12. The new file verifies, with the same trainee, the same filter and its own
  file name. The earlier file still verifies too.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Export portfolio is not in her menu. Typed /portfolio/export:
  admitted. With 2023-01-15 to 2026-10-05: "Portfolio exported successfully. …" and a new file,
  portfolio-ed02ba05ee23.pdf (184,880 bytes). Its "Progress per EPA" opens "The trainee completed the programme on 5
  October 2026. Targets as on 5 October 2026 (training year 4). …". Verify: "Export verified", generated 2026-10-05
  14:46 UTC, the same Trainee ID and exporter (49a8e028-…), "Filter: 2023-01-15 to 2026-10-05", "File:
  portfolio-ed02ba05ee23.pdf". The earlier file still verifies (14:43 UTC, portfolio-c47ccaad4d32.pdf).
Gap: [F-5.23a, T311] (still) A graduate is offered no link to Export portfolio, although the page admits her, the
  graduation email says her portfolio "is available in Wombat" and Home's card mentions "your portfolio to export";
  she reaches it only by typing its address. The step's Expect describes this behaviour as it is.

## Phase 5.E — A registrar leaves

### Step 5.24 — Dr du Plessis asks for a Mini-CEX on a shift after his post ended
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) on PAED-002 with:
  - encounter date `D−1`;
  - Dr Naidoo as the assessor;
  - a clinical setting, a presenting problem and a case complexity.
  Submit it.
Expect: The EPA list offers PAED-002, because its tool list includes the Mini-CEX. There is no late-filing warning.
  Submit to David Naidoo reads "Submitted. It is now Requested. It is in David Naidoo's Activity inbox.", and the
  activity is in his inbox. No email is sent: Wombat mails nobody when an activity moves (Step 3.3).
Note: In the story, his registrar post ended on `D−2` when he resigned to move provinces, but HR tells Prof Mbatha only
  today. He worked `D−1` as a locum shift.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Log an activity's picker → Mini-CEX (Paediatrics)
  (/activities/new?type=mini_cex_cpsa): the EPA list offered PAED-001, 002, 003, 004, 006, 007, 008, 012 and 013, and
  the assessor list Botha, Khumalo, Naidoo, Patel and Zulu. With PAED-002, David Naidoo, date observed 2026-10-04,
  Ward, a presenting problem and Moderate, no late-filing notice showed; the moves were Submit to David Naidoo and
  Save draft. Submit → /activities/24: "Submitted. It is now Requested. It is in David Naidoo's Activity inbox."; Who
  has it now: "With David Naidoo since 2026-10-05 16:47 SAST.", Cancel request… offered; Credit "None until it is
  completed". No mail (stub count 32 → 32).
Gap: none

### Step 5.25 — Dr Naidoo rates it
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: He has acted as a Committee member since Step 4.5: choose Switch to Assessor in the sidebar, then open Dr du
  Plessis's Mini-CEX from Activity inbox and rate it `3b`, PAED-002's minimum in training year 2. Write what was done
  well, what to develop and the agreed plan, and complete it. Then choose Switch to Committee member in the sidebar.
Expect: Before the switch, his Committee member's Home carries, under the header, the line "1 activity waits for you
  in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · `D−1`, from Pieter du Plessis, waiting less than a day.",
  with Open it (T350). After it, Home's "Waiting for you" is badged "1 waiting", and the inbox lists the Mini-CEX as
  "Mini-CEX (Paediatrics) · PAED-002 · `D−1`", from Pieter du Plessis, Requested. He rates it on the rung picker. After
  Complete the result reads "Completed. Nothing else waits for you.", with Go to Home. The activity reads Completed and
  is read-only to both of them. It credits PAED-002 in the semester that holds `D−1` (checked in Step 5.26), and My
  activities reads it credited "1 item". Nobody is emailed.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): His Committee member Home (menu Home, Programme trainees,
  Committee reviews, Decision panels, My data rights) carries under the header, above the Registrars roster (4
  registrars; Molefe is gone), "1 activity waits for you in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 ·
  2026-10-04, from Pieter du Plessis, waiting less than a day." with Open it (→ /activities/24). Switch to Assessor:
  "You are now acting as Assessor." (menu Home, Activity inbox, My data rights); Home's "Waiting for you" is badged "1
  waiting". Inbox: "Mini-CEX (Paediatrics) · PAED-002 · 2026-10-04", from Pieter du Plessis, Requested, "Less than a
  day". Rated 3b on the rung picker (1, 2, 3a, 3b, 4, 5) with the three fields; Complete: "Completed. Nothing else
  waits for you."; "Done. You completed it on 2026-10-05 16:47 SAST. Rated 3b. Credited 1 item to PAED-002."; no field
  or move left for either (du Plessis's view adds "PAED-002: 1 of 3 this semester." and Open My progress). Switched
  back: "You are now acting as Committee member." Dr du Plessis's My activities reads it "Done, Completed, 1 item". No
  mail (32 → 32).
Gap: none

### Step 5.26 — Dr du Plessis sees it counted
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Open My progress, then PAED-002 from its index, and read its count for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX: its row reads "n of 3 this semester", and its page
  the same count, with "Last encounter `D−1`" and "Training year 2: level 3b, the minimum each encounter is judged
  against and your STAR's target." Record the count as n.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): My progress ("Training year 2 · Semester 2, 2026"): PAED-002
  "1 of 3 this semester", "2 more by 2026-11-30"; its code links to /portfolio/progress/3 (My activities' Credit "1
  item" links there too). PAED-002's page: Observations "1 of 3 this semester", "2 more by 2026-11-30.", "At the
  minimum level when observed: 1 of 1", "Last encounter 2026-10-04", "Semester 1, 2026: 0 of 3, 3 short", "Training
  year 2: level 3b, the minimum each encounter is judged against and your STAR's target."; Entrustment "No STAR yet.";
  the trajectory (1 rating, at the minimum) and Activities on this EPA (the Mini-CEX, 1 item). So n = 1.
Gap: none

### Step 5.27 — Prof Mbatha records his withdrawal
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Open Dr du Plessis's profile and set the last day in the programme to `D−2`. Press Deactivate and confirm.
Expect: The confirmation names `D−2` as his last day. It says that encounters after that day will count towards
  nothing, including any already counted, and that neither the day nor the profile can be changed back. After
  confirming:
  - the page says "Trainee profile deactivated. Their last day in the programme is recorded as `D−2`.";
  - the summary reads Status Inactive, "Left the programme: `D−2`", and the profile's details read-only, as on Step
    5.17, with no Save profile (T305);
  - on Trainees he is listed under Completed & closed profiles, with the outcome "Withdrawn `D−2`";
  - in the same save, the credit from the `D−1` Mini-CEX is taken back (T281);
  - he keeps the Trainee role and is not emailed.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Trainees → Edit Pieter du Plessis (/admin/trainees/edit?id=4),
  last day 2026-10-03, Deactivate: "Deactivate Pieter du Plessis's profile, with 2026-10-03 as their last day in the
  programme? Encounters observed after that day will count towards nothing in this programme, including any already
  counted. The day cannot be changed afterwards, and a deactivated profile cannot be made active again." Confirmed:
  "Trainee profile deactivated. Their last day in the programme is recorded as 2026-10-03."; "Status: Inactive", "Left
  the programme: 2026-10-03"; subtitle "The record of a programme that has ended."; "Profile details" reads "This
  programme has ended, so its record is archived and cannot be changed." over 11.1, 2025-01-15 and 2029-01-14 as text,
  with no visible input or button and no Save profile. Trainees: Active lists Dlamini, Mahlangu and Ndlovu; Completed
  & closed lists "Pieter du Plessis … Withdrawn 2026-10-03" and "Lerato Molefe … Completed 2026-10-05". In the same
  save his PAED-002 progress went (1 row → none) and #24's complete transition was re-stamped CreditedItemCount 1 → 0
  (SQL). His Users row still lists Trainee; no mail (32 → 32). Afterwards (flow 06, read as Mr Smit): Waiting for
  assessors still lists his two waiting requests (Portfolio and Logbook Review with Patel, Overdue, 8 days; CbD with
  Khumalo), each offering Send a reminder, and the Coordinator's Home lists them too; Programme trainees lists 3
  current registrars.
Gap: none

### Step 5.28 — Dr du Plessis's record after he left
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → /account/login/submit → / → /portfolio/progress → /activities/mine
Do: Sign in, then read Home, My progress and My activities.
Expect: Each page records that his programme ended on `D−2`, every date ISO (T355, D1):
  - Home's Your targets card says "Your programme ended on `D−2`, so no target applies to you any more. Your progress in
    each period is kept on My progress, read-only.", with no figures and no Furthest short, and ends with Open My
    progress. My authorisations reads "No STAR yet." alone.
  - My progress opens with "Your programme ended on `D−2`. This page is your record of it and is read-only…". Its "Your
    programme" card reads Ended `D−2`. PAED-002's period holding `D−2` reads "no target (your programme ended part-way
    through) · n−1 recorded".
  - My activities still lists the `D−1` Mini-CEX as Completed, its Credit now "None": the take-back's replay re-stamped
    its completion (`ProgrammeEndCredit`). The filing is kept, but it counts towards nothing.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Home (Acting as Trainee): Your targets reads "Your programme
  ended on 2026-10-03, so no target applies to you any more. Your progress in each period is kept on My progress,
  read-only.", no figures and no Furthest short, then Open My progress; Recent decisions lists the Mini-CEX "Credits
  nothing."; My authorisations reads "No STAR yet." with Open My authorisations. My progress: "Your programme ended on
  2026-10-03. This page is your record of it and is read-only: …"; "Your programme" Started 2025-01-15, Ended
  2026-10-03, "2 when your programme ended"; PAED-002 "Semester 2, 2026 no target (your programme ended part-way
  through) · 0 recorded" (n−1 = 0); no bars; no long date on Home or My progress. My activities: "Mini-CEX
  (Paediatrics) · PAED-002 · 2026-10-04, to David Naidoo, Done, Completed", Credit "None". /activities/24 reads "Rated
  3b. It counted towards no curriculum requirement.", Credit None, and "… or the encounter is dated after the
  trainee's programme ended. The record is kept. …".
Gap: none

### Step 5.29 — Neither ended programme can be put before the panel
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Open Schedule review, choose the Paed Annual Review Panel and read the trainee list.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu. A graduate and a registrar who has withdrawn are not
  current trainees (T238). He closes the form without scheduling anything.
Actual (2026-10-05, T358 replay, wombat_scenario_t358): Committee reviews → Schedule review → Paed Annual Review
  Panel: the Trainee list offers only Anele Dlamini, Nomsa Mahlangu and Sipho Ndlovu. He left the form without
  creating anything (still 7 reviews, SQL).
Gap: none

## Act 5 outcome state

Replay check (2026-10-05, T358 replay, wombat_scenario_t358): match on every query below. Molefe false, 2026-10-05,
  null; du Plessis false, null, 2026-10-03. Molefe holds no role and du Plessis holds Trainee. The current trainees
  are dlamini, mahlangu and ndlovu. 15 active STARs, nine at 5 and six at 4, each Order equal to MinimumLevelOrder.
  Review 7 reads 2, 4, 7, 4, with nothing left staged. The exports are portfolio-c47ccaad4d32.pdf ×3, then
  portfolio-ed02ba05ee23.pdf ×1. After the last day: #24 only (completed, 2026-10-04, 2026-10-03). His PAED-002
  progress has no row (n−1 = 0). The act wrote one mail, the graduation email. Snapshot: scenario-t358-post-act5.

- **Dr Molefe** is a former trainee:
  - her pre-graduation review is ratified with the decision Graduate;
  - she holds 15 active STARs, one for each EPA, each at its exit level;
  - her profile is completed on `D` and is inactive;
  - she holds no role, and sign-in still gives her the trainee record;
  - the graduation email is in the log;
  - four exports cover `J−3y` to `D`: three identical ones, before completion, by her, Prof Mbatha and Mr Smit, and
    one she made after completion.
- **Dr du Plessis** has withdrawn:
  - his profile is deactivated with the last day `D−2`;
  - he still holds the Trainee role;
  - his `D−1` Mini-CEX is completed and credits nothing.
- **Current KGK trainees:** Dr Dlamini, Dr Mahlangu and Dr Ndlovu.

Substitute the replay's dates for `D` in the comments.

```sql
-- Both programmes have ended: Molefe completed on D, du Plessis withdrawn on D−2.
SELECT u."Email", p."IsActive", p."CompletedOn", p."DeactivatedOn"
FROM "TraineeProfiles" p JOIN "AspNetUsers" u ON u."Id" = p."UserId"
WHERE u."Email" IN ('molefe@kgk.wombat.local', 'duplessis@kgk.wombat.local');
-- molefe: false, D, null · duplessis: false, null, D−2

-- Molefe holds no role; du Plessis still holds Trainee.
SELECT u."Email", r."Name"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
WHERE u."Email" IN ('molefe@kgk.wombat.local', 'duplessis@kgk.wombat.local');
-- molefe: one row, Name null · duplessis: Trainee

-- The current KGK trainees.
SELECT u."Email" FROM "TraineeProfiles" p JOIN "AspNetUsers" u ON u."Id" = p."UserId"
WHERE p."IsActive" AND u."Email" LIKE '%@kgk.wombat.local' ORDER BY 1;
-- dlamini, mahlangu, ndlovu

-- 15 active STARs (Status 1), one per EPA, each at or above its exit level.
SELECT e."Code", l."Label", l."Order", ci."MinimumLevelOrder"
FROM "EntrustmentDecisions" d
JOIN "AspNetUsers" u ON u."Id" = d."TraineeUserId"
JOIN "Epas" e ON e."Id" = d."EpaId"
JOIN "EntrustmentLevels" l ON l."Id" = d."AuthorisedLevelId"
JOIN "TraineeProfiles" p ON p."UserId" = u."Id"
JOIN "CurriculumItems" ci ON ci."CurriculumId" = p."CurriculumId" AND ci."EpaId" = d."EpaId"
  AND ci."OwningInstitutionId" IS NULL
WHERE u."Email" = 'molefe@kgk.wombat.local' AND d."Status" = 1
ORDER BY e."Code";
-- 15 rows, PAED-001 to PAED-015; "Order" >= "MinimumLevelOrder" on every row (label 5 on nine, 4 on six)

-- The final review: pre-graduation (ReviewType 2), Ratified (State 4), Graduate (Category 7), four present.
SELECT r."Id", r."ReviewType", r."State", c."Category", count(a."Id") AS present
FROM "CommitteeReviews" r
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
JOIN "CommitteeDecisions" c ON c."ReviewId" = r."Id"
LEFT JOIN "CommitteeDecisionAttendees" a ON a."DecisionId" = c."Id"
WHERE u."Email" = 'molefe@kgk.wombat.local' AND r."ReviewType" = 2
GROUP BY r."Id", r."ReviewType", r."State", c."Category";
-- one row: 2, 4, 7, 4. Nothing is left staged on it:
-- SELECT count(*) FROM "PendingEntrustmentDecisions" WHERE "ReviewId" = <that id>;  -- 0

-- The Act 5 exports: one file three times, then the graduate's own.
SELECT e."FileName", count(*) AS exports, min(e."ExportedOn") AS first
FROM "PortfolioExports" e JOIN "AspNetUsers" u ON u."Id" = e."TraineeUserId"
WHERE u."Email" = 'molefe@kgk.wombat.local' AND e."FilterFromDate" = DATE '<J−3y>'   -- 2023-01-15 on a replay in 2026
GROUP BY e."FileName" ORDER BY first;
-- two rows: the pre-completion file, 3 exports · the graduate's file, 1 export

-- du Plessis: the D−1 Mini-CEX is kept after his last day, and credits nothing.
SELECT a."Id", a."CurrentState", a."ObservedOn", p."DeactivatedOn"
FROM "Activities" a
JOIN "AspNetUsers" u ON u."Id" = a."SubjectUserId"
JOIN "TraineeProfiles" p ON p."UserId" = u."Id"
WHERE u."Email" = 'duplessis@kgk.wombat.local' AND a."ObservedOn" > p."DeactivatedOn"
  AND a."CurrentState" = 'completed';
-- one row: completed, D−1, D−2. Without the state filter, Act 3's cancelled DOPS (dated D, the day it was created)
-- and his submitted Portfolio and Logbook Review (D−1) are listed too: both are after D−2, and neither credits.
SELECT pr."AcademicYear", pr."Semester", pr."CountsSoFar"
FROM "CurriculumItemProgresses" pr
JOIN "CurriculumItems" ci ON ci."Id" = pr."CurriculumItemId"
JOIN "Epas" e ON e."Id" = ci."EpaId"
JOIN "AspNetUsers" u ON u."Id" = pr."TraineeUserId"
WHERE u."Email" = 'duplessis@kgk.wombat.local' AND e."Code" = 'PAED-002'
ORDER BY 1 DESC, 2 DESC;
-- the period holding D−1: n−1, the count before Step 5.24; with n−1 = 0, as in a replay where the Mini-CEX is his
-- only PAED-002 credit, the period has no row at all
```

## Handoff to Act 6

- Dump the database: `pg_dump -Fc` to `recovery/scenario-post-act5.dump`.
- **Act 6 starts with three current trainees** (Dlamini, Mahlangu and Ndlovu) and two ended programmes. Dr du Plessis's
  profile is inactive, so its record is archived and cannot be edited (T305), and any count of "active trainees on 11.1"
  leaves him out. Their records must survive the catalogue's maintenance:
  - Pausing an EPA must leave Dr Molefe's STAR on it standing, labelled as no longer in use.
  - The progress rebuild must reproduce what this act recorded: Dr Molefe's ended periods, and Dr du Plessis's `D−1`
    encounter crediting nothing.
- **Nothing later may schedule, credit or run an MSF campaign for Dr Molefe or Dr du Plessis**, because neither is a
  current trainee.
- **The appendix:**
  - Dr Molefe, with no role, and Dr du Plessis, withdrawn but still holding Trainee, both keep Data Rights in their
    menus.
  - Dr Ndlovu is the account erased there.

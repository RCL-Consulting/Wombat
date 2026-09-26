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
Do: Open Decisions Due for the period holding `D`, set the status filter to every status, and read Dr Molefe's rows.
Expect: There is one row for each EPA due for her in the period, and each row gives its status in words:
  - an EPA that an Act 4 STAR decided reads Decided, and names the STAR and the review that issued it;
  - an EPA that Act 4 deferred reads Deferred, and says that a later sitting plans it again;
  - the others read Not scheduled, Due by year end or As opportunity allows.
  Every row that is not yet decided offers Schedule. KGK's other registrars are listed too, and nobody outside KGK is.
Actual (2026-09-26, T295 replay, wombat_scenario): Period 2026 S2, Every status: "75 of 75 decisions due in 2026 S2
  shown." for "the 5 trainees you oversee at Kgosi Kgari Teaching Hospital" (Dlamini, Molefe, Mahlangu, du Plessis,
  Ndlovu; nobody else). Molefe has 15 rows: PAED-001, 010 and 012 Decided ("STAR #1/#2/#3, issued at review #1.", Open
  review); nine Deferred ("Deferred at review #1. A later sitting for 2026 S2 [or 2026] plans it again."); PAED-008, 009
  and 013 As opportunity allows. Every undecided row offers Schedule (the deferred ones Open review too).
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
Actual (2026-09-26, T295 replay, wombat_scenario): Schedule on Molefe's PAED-006 row opened
  /committee/reviews?panel=1&trainee=59ba7d01-…&period=2026-2 with Paed Annual Review Panel, Lerato Molefe and 2026 S2
  filled, window 2026-01-01 to 2026-12-31, scheduled 2026-09-26. Type offers Annual progression review and
  Pre-graduation review only. Preview: "12 EPAs will be on the agenda for 2026 S2.", "PAED-002, 003, 004, 005, 006, 007,
  011, 014 and 015 must be decided at this sitting, or deferred with a reason, before it is ratified.", PAED-008/009/013
  "· As opportunity allows", "Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012."
  With from 2023-01-15 and Pre-graduation, created review #7: Pre-graduation review, Summative, Scheduled, evidence
  window 2023-01-15 to 2026-12-31; Start review offered to Mr Smit, left unpressed.
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
Actual (2026-09-26, T295 replay, wombat_scenario): Opened review #7 from Committee reviews (Pre-graduation, Scheduled)
  and pressed Start review: "Review started.", State In progress. Snapshot (2023-01-15 to 2026-12-31) groups PAED-001
  (CbD #13, DOPS #14, Mini-CEX #12), PAED-010 (DO #17, MSF #22) and PAED-012 (CbD #16, Mini-CEX #15, MSF #23), plus "Not
  about a single EPA: Default MSF #1 … Released; responses 6". Standing: "In training year 4 on 26 September 2026", "2
  at or above · 1 below · 12 with no decision, of 15 EPAs", "2 of 15 EPAs at their exit level by STAR decision (level 5:
  2 of 9 · level 4: 0 of 6). Not yet: PAED-002 … PAED-015" (all but 001 and 012), then the "For information only …" line
  verbatim. Record decision disabled, its reason naming the nine Due lines. Sampling warnings on PAED-010 (1 assessor)
  and PAED-012 (2), refusing nothing. Agenda "12 EPAs for 2026 S2, 9 still to stage or defer".
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
Actual (2026-09-26, T295 replay, wombat_scenario): Staged 13 decisions on review #7, all issued 2026-09-26 with no
  expiry: rung 5 on PAED-002 to 007 and 010, rung 4 on PAED-008, 009, 011, 013, 014 and 015. Twelve via Stage on the
  agenda line (EPA preselected, "PAED-0nn's lines are listed first"); PAED-010 via the EPA list. The level list offered
  exactly 1, 2, 3a, 3b, 4, 5; Stage pending decision was disabled ("Name at least one item of the evidence snapshot.")
  until a line was ticked. The twelve without own lines rest on Default MSF #1 only and got both hints ("Only one item
  is named. The College's rule is that an entrustment decision is never taken from a single form." and "None of the
  named items is about PAED-002."), not refused; PAED-010 rests on its own DO #17 and MSF #22. Each staging: "Pending
  entrustment decision staged.", the line reads Staged ("A decision is staged below."), and the list shows e.g.
  "PAED-010 … 5 · issued 2026-09-26 … Rests on 2 items of the snapshot: …". PAED-010 gained a line (Origin 2; agenda "13
  EPAs for 2026 S2", "Already decided … PAED-001 and PAED-012."). Record decision then enabled.
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
Actual (2026-09-26, T295 replay, wombat_scenario): Category opened on "Select a category…" and offered Satisfactory
  Progress, Satisfactory with Observations, Inadequate Progress — Additional Training, Inadequate Progress — Repeat,
  Release from Training, Outcome Deferred and Graduate (programme complete). "Thandi Zulu (chair)" was ticked and
  disabled. Recorded Graduate with the rationale, no conditions, all four present: "Decision recorded.", State Decided,
  decision "Graduate (programme complete)", "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg
  (external)". Pending list reads "Fixed when the committee's decision was recorded: ratifying issues exactly these. …";
  no EPA select, Stage or Remove. Ratify offered to Dr Zulu; Dr Naidoo (second session) sees no Ratify, only "Only the
  panel's chair, Thandi Zulu, can ratify the committee's decision."
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
Actual (2026-09-26, T295 replay, wombat_scenario): Ratify: "Decision ratified.", State Ratified. The 13 staged decisions
  were issued as STARs #6–#18 in the same action (PAED-010's is #18); every agenda line reads Decided with its STAR
  number; the pending list reads "Nothing is pending: ratifying the review issued what was staged as STARs, and the
  agenda names each one." Act 4's PAED-010 STAR #2 (level 4, expiry 2026-10-16) now has Status 4, superseded (SQL).
  Standing: "15 at or above · 0 below · 0 with no decision, of 15 EPAs" and "15 of 15 EPAs at their exit level by STAR
  decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level."
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
Actual (2026-09-26, T295 replay, wombat_scenario): Dashboard quick links: Users, Invitations, Curriculum adoptions,
  Entrustment decisions. Trainee "Molefe", Status Active, Apply filters: 15 rows, PAED-001 to PAED-015, level 5 on
  001–007, 010, 012 and 4 on 008, 009, 011, 013–015, all Issued 2026-09-26 (Act 4's PAED-001 and 012 were also issued on
  D in this replay), Expires "—", each with Download and Revoke. Status All: 16 rows; the extra is PAED-010 at 4, issued
  2026-09-26, expires 2026-10-16, Superseded, Download only. Nothing revoked.
Gap: none

### Step 5.8 — Dr Molefe reads her final review and keeps a STAR certificate
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → / → /portfolio/authorisations
Do: Open My Committee Reviews and view the pre-graduation review. Then open My authorisations from her dashboard's card,
  and download the certificate for PAED-001.
Expect: The review is listed as Pre-graduation, Ratified, Graduate (programme complete). Its detail shows:
  - the review type;
  - the decision and its rationale;
  - who was present;
  - an agenda in which each EPA reads Decided, with its STAR number.
  The Lodge appeal form is offered, and she lodges no appeal. My authorisations holds 15 cards, each with its level, its
  issue date and "No expiry". The certificate downloads as `star-certificate-PAED-001-<id>-<8 hex digits>.pdf`.
Note: My authorisations requires the Trainee role, so this is the last point at which she can download a certificate
  from that page (see Step 5.22).
Actual (2026-09-26, T295 replay, wombat_scenario): My committee reviews lists two: "2026 S2 · 2023-01-15 to 2026-12-31,
  Pre-graduation, Ratified, Graduate (programme complete)" and Act 4's annual one. View shows Type "Pre-graduation
  review", State Ratified, the decision and rationale, "Present when this decision was taken: Thandi Zulu (chair), David
  Naidoo, Sarah Botha, John van Rensburg (external)", and an agenda of the 13 EPAs it decided, each "Decided, STAR #n."
  (#6–#18; PAED-001 and 012 were Act 4's, not on it). Lodge appeal form offered, not used. Dashboard's My authorisations
  card → /portfolio/authorisations: 15 cards, each with level, "Issued 26 September 2026", "No expiry" and Download
  certificate. PAED-001's downloaded as star-certificate-PAED-001-1-30777bea.pdf.
Gap: none

## Phase 5.B — The portfolio and its verification

### Step 5.9 — Dr Molefe exports her portfolio
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/export
Do: Open Export Portfolio from the menu. Set the from date to her programme start, `J−3y`, and the to date to `D`, and
  export the PDF.
Expect: The page opens with the last twelve months filled in. After the export it says "Portfolio exported
  successfully. The download should start automatically.", and a file named `portfolio-<12 hex digits>.pdf` downloads.
  Record the file name: Steps 5.10 and 5.12 must produce the same one.
Note: Every export in this phase is made on `D` with the same two dates. The PDF carries no generation time, so the
  same data gives the same bytes (T078).
Actual (2026-09-26, T295 replay, wombat_scenario): Export Portfolio from the menu opened with From 2025-09-26 and To
  2026-09-26 (the last twelve months). With 2023-01-15 to 2026-09-26, Export PDF: "Portfolio exported successfully. The
  download should start automatically." and portfolio-68a170344cf3.pdf downloaded (SHA-256 recorded for 5.10, 5.12 and
  5.13).
Gap: none

### Step 5.10 — Prof Mbatha exports Dr Molefe's portfolio by its address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId} → /portfolio/export/{TraineeUserId}
Do: Find Dr Molefe in Users and open her. Take her user id from the page's address, then type the export address with
  that id. Set the dates to `J−3y` and `D`, and export.
Expect: The export succeeds as in Step 5.9. The downloaded file has the same name as Dr Molefe's and is identical to it
  byte for byte. Compare the two with `fc /b`, or by their SHA-256.
Note: No page links to the staff export, so this step reaches it by typing the address. Record that in `coverage.md`.
Actual (2026-09-26, T295 replay, wombat_scenario): Users → filter "Molefe" → Manage opened
  /admin/users/59ba7d01-561f-4831-9f35-b85305e31354 (Status Active, Roles Trainee). Typed /portfolio/export/59ba7d01-…:
  the page opened with 2025-09-26 to 2026-09-26 and reads only "Generate a PDF export of the trainee's portfolio.",
  naming nobody. With 2023-01-15 to 2026-09-26: "Portfolio exported successfully. …" and portfolio-68a170344cf3.pdf,
  byte-identical to Dr Molefe's (cmp; the same SHA-256 68a17034…ec9c). coverage.md already lists this route as linked
  from no page.
Gap: [F-5.10a, T314] The staff export page never says whose portfolio it exports: /portfolio/export/{TraineeUserId} resolves
  no name before or after the export, so a member of staff who types or follows a wrong id exports another trainee's
  portfolio without being told (confirms the suspect "The staff export page does not name whose portfolio it exports").

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
Actual (2026-09-26, T295 replay, wombat_scenario): The 11-page PDF carries on every page the header Kgosi Kgari Teaching
  Hospital, "Portfolio Export", Lerato Molefe, Paediatric EPA Curriculum, "Paediatrics — Paediatrics", "Period:
  2023-01-15 to 2026-09-26", and the footer "Page n of 11 | Generated by Wombat"; no generation time anywhere (metadata
  dates fixed at 2000-01-01). Sections in order: Summary (8 activities by type, "Committee reviews: 2", "MSF reports:
  1"); "Statements of Awarded Responsibility (STARs)", "15 active entrustment decisions on record.", levels 5/4 by
  label; "Progress per EPA" "on 26 September 2026 (training year 4)"; "Committee Decisions" with Act 4's review and the
  Pre-graduation review, Ratified, "Graduate (programme complete)", its rationale and the four present by name;
  Activities by type (rungs by label, assessors by name); Multi-Source Feedback (Default MSF, Campaign #1); "Appendix —
  Audit Trail". Rendered pages 1 and 5 are the screenshots.
Gap: none

### Step 5.12 — Mr Smit reproduces the export
Role: Coordinator — Mr Pieter Smit
Route: /portfolio/export/{TraineeUserId}
Do: Type the export address with Dr Molefe's user id, which Prof Mbatha sends him. Set the same two dates, and export.
Expect: The export is allowed, because a coordinator of her institution oversees her (T079, T101). The file has the
  same name as those from Steps 5.9 and 5.10, and is identical to both byte for byte.
Note: No page that a coordinator can open links to a trainee's export. Her user id is otherwise only in an address he
  has seen: the `trainee=` part of the Schedule link he followed in Step 5.2.
Actual (2026-09-26, T295 replay, wombat_scenario): Typed /portfolio/export/59ba7d01-561f-4831-9f35-b85305e31354 as Mr
  Smit: admitted ("Generate a PDF export of the trainee's portfolio.", no name). With 2023-01-15 to 2026-09-26:
  "Portfolio exported successfully. …" and portfolio-68a170344cf3.pdf, byte-identical to Steps 5.9 and 5.10 (all three
  SHA-256 68a17034…ec9c; cmp). Her id came from the trainee= part of the Schedule link he followed in Step 5.2.
Gap: [F-5.10a, T314] also applies: the page names no trainee.

### Step 5.13 — A verifier checks the genuine PDF
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: In a private window, without signing in, open the verification page. Compute the SHA-256 of the PDF that Dr Molefe
  sent (`certutil -hashfile <file> SHA256`, or `sha256sum`, both of which print it in lower case), enter it and press
  Verify. Do the same for Mr Smit's copy.
Expect: The page works signed out, as a static page. Verifying loads the page's own address with the hash in it. The
  page reads "Export verified" and shows:
  - when Wombat generated the file, in UTC;
  - the trainee and the exporter as user ids, never as names (DESIGN: the anonymous page must not disclose a name);
  - "Filter: `J−3y` to `D`";
  - the file name that all three downloads carry.
  Mr Smit's copy has the same SHA-256 and gets the same answer.
Note: The PDF prints no hash, though the field's placeholder says "from the PDF footer" (T287). The file name carries
  only the first 12 hex digits, and those alone match nothing. The check needs the file's whole SHA-256.
Actual (2026-09-26, T295 replay, wombat_scenario): In a separate, signed-out browser context /portfolio/verify opened
  (menu shows only Sign in): "Verify portfolio export", field "Content hash (SHA-256)" with the placeholder "Enter the
  hash from the PDF footer". sha256sum of Dr Molefe's file (68a17034…ec9c, lower case) → Verify loaded
  /portfolio/verify?hash=68a17034…&check=1 reading "Export verified", "This PDF was generated by Wombat on 2026-09-26
  12:09 UTC.", "Trainee ID: 59ba7d01-…", "Exported by: 59ba7d01-…" (ids only, no name), "Filter: 2023-01-15 to
  2026-09-26", "File: portfolio-68a170344cf3.pdf". Mr Smit's copy has the same SHA-256, so the same answer, still naming
  export 1 (Dr Molefe, 12:09 UTC) as the exporter. The same hash typed in upper case (as PowerShell's Get-FileHash
  prints it) read "No matching export found. This hash does not correspond to any PDF generated by this system."
Gap: [F-5.13a, T313] The verify page matches the hash case-sensitively: the genuine file's SHA-256 in upper case, as Windows'
  Get-FileHash prints it, reads "No matching export found" (confirms the suspect "/portfolio/verify compares the hash
  case-sensitively"). [F-5.13b, T287] The placeholder still says "Enter the hash from the PDF footer" though the footer prints
  none (already on T287).

### Step 5.14 — The verifier checks a tampered copy
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: Change one byte in a copy of the PDF, compute the copy's SHA-256 and verify it. Then press Verify with only spaces
  in the field.
Expect: The tampered copy gets "No matching export found. This hash does not correspond to any PDF generated by this
  system." The press with only spaces gets "Enter the content hash to check." Neither leads to an error page.
Actual (2026-09-26, T295 replay, wombat_scenario): A copy with one byte changed (SHA-256 6a95a2c5…e9ea) →
  /portfolio/verify?hash=6a95a2c5…&check=1: "No matching export found. This hash does not correspond to any PDF
  generated by this system." Five spaces → ?hash=+++++&check=1: "Enter the content hash to check." Neither reached an
  error page; the page stayed signed out throughout.
Gap: none

## Phase 5.C — The programme ends

### Step 5.15 — Dr Molefe leaves My progress open
Role: Trainee — Dr Lerato Molefe
Route: /account/login → /account/login/submit → / → /portfolio/progress
Do: In her own browser, sign in and open My progress. Leave the tab open through Step 5.19.
Expect: The page shows her programme as still running:
  - "This period", and each EPA's target for it, with progress bars;
  - on each EPA's card, the period before it;
  - "Entrustment against Annexure A", with the exit rule met as in Step 5.6;
  - her rating trajectories.
  Nothing says that her programme has ended.
Actual (2026-09-26, T295 replay, wombat_scenario): Dr Molefe signed in in her own browser context (/ then My Progress,
  /portfolio/progress; the tab is left open). "This period" Semester 2, 2026 · July to November; "1 of 10 EPAs met this
  semester", "0 of 5 EPAs met in 2026", Training year 4; 15 cards with progress bars and targets (e.g. PAED-001 "3 of 3
  this semester", PAED-002 "0 of 3 … 3 more by 30 November 2026"), each with the period before ("Semester 1, 2026: 0 of
  3, 3 short", "2025 academic year: 0 of 1, 1 short"). "Entrustment against Annexure A": "15 at or above · 0 below · 0
  with no decision" and "15 of 15 … Every EPA is at its exit level." Rating trajectories for PAED-001, 010 and 012.
  Nothing says her programme has ended. The yearly cards say "Your training year changed on 14 Jan 2026", though her
  programme started on 15 January 2023.
Gap: none. [F-5.15a, not a defect: D17] "Your training year changed on 14 Jan 2026", a day before her anniversary:
  D17 counts the training year in whole 365-day blocks (`TraineeProfile.GetStage`), and 2024 was a leap year, as Act 2 §
  Dates records. An anniversary-based year would mean reopening D17.

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
Actual (2026-09-26, T295 replay, wombat_scenario): Trainees → Edit Lerato Molefe (/admin/trainees/edit?id=2). "Last day
  in the programme" opened on 2026-09-26; its help reads "Mark complete records it as the graduation day, archives the
  profile and removes the Trainee role. … It cannot be after today, and it cannot be changed afterwards. …". Set to
  2027-01-14 and pressed Mark complete: the dialog "Mark this programme complete?" asks "Record 2027-01-14 as Lerato
  Molefe's graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent.
  Encounters observed after that day will count towards nothing in this programme, including any already counted. The
  day cannot be changed afterwards, and this cannot be undone." Confirmed: the alert "The completion date cannot be
  after today (2026-09-26)." (id programme-end-refusal, in the field's aria-describedby); Status still Active.
Gap: none

### Step 5.17 — Prof Mbatha marks Dr Molefe's programme complete on `D`
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees/edit → /admin/trainees → /admin/users/{UserId}
Do: Set the last day to `D`, press Mark complete and confirm. Then go back to Trainees, and open Dr Molefe in Users.
Expect: The page says "Trainee marked complete. The Trainee role has been removed and a graduation email sent."
  - The summary reads Status Completed, Completed `D`.
  - The last-day field, Deactivate and Mark complete are gone.
  - On Trainees she is no longer under Active profiles. She is listed under Completed & closed profiles with the outcome
    "Completed `D`".
  - Her user page says "This user has no roles."
  - No credit is taken back, because nothing of hers is observed after `D` (T281; Step 5.27 shows a take-back).
Note: The completion runs under the trainee credit lock (T281), so any completion of hers that is in flight is waited
  for first.
Actual (2026-09-26, T295 replay, wombat_scenario): Last day 2026-09-26 → Mark complete; the dialog named 2026-09-26 as
  her graduation day; confirmed: "Trainee marked complete. The Trainee role has been removed and a graduation email
  sent." Summary Status Completed, "Completed: 2026-09-26"; the last-day field, Deactivate and Mark complete are gone,
  but Curriculum, Programme start date, Expected completion date and Save profile remain editable. Trainees: Active
  profiles lists Dlamini, du Plessis, Mahlangu and Ndlovu; "Completed & closed profiles" lists Lerato Molefe, "Completed
  2026-09-26". /admin/users/59ba7d01-…: "This user has no roles." (Status Active; Add role now offers Trainee). Progress
  unchanged (PAED-001 3, 010 1, 012 2 in 2026 S2): nothing taken back.
Gap: [F-5.17a, T305] A completed profile still offers Save profile, with the curriculum, programme start and expected
  completion editable. So a graduate's archived record, and the PDF built from it, can still be re-pinned or re-dated,
  although the help and the dialog say Mark complete "archives the profile". This confirms the suspect "A completed
  ('archived') profile still offers Save profile"; not pressed.

### Step 5.18 — Dr Molefe is emailed
Role: System — the graduation email sent by Mark complete
Route: n/a
Do: Read the application log for the mail sent in Step 5.17.
Expect: There is one email to `molefe@kgk.wombat.local` with the subject "Congratulations on completing Paediatric EPA
  Curriculum". It congratulates her on completing the programme as of `D`, and says that her committee has ratified
  her final entrustment decisions and that her portfolio is available in Wombat.
Actual (2026-09-26, T295 replay, wombat_scenario): One email, to molefe@kgk.wombat.local, 14:15:25 +0200, subject
  "Congratulations on completing Paediatric EPA Curriculum": "Congratulations on completing your training programme,
  Paediatric EPA Curriculum, as of 2026-09-26. Your committee has ratified your final entrustment decisions and your
  portfolio of evidence is available in Wombat." Read from this replay's SMTP sink (tmp/mail, file 90), where its app
  delivers mail, not from the log; no other mail was sent by the completion.
Gap: none

### Step 5.19 — Dr Molefe's open session ends
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /account/session-ended → /account/login
Do: Go back to the tab left open at Step 5.15 and wait up to a minute.
Expect: Removing her role changed her account's security stamp. The session check, which runs once a minute, ends the
  session: the tab reloads to the sign-in page, which says "Your session has ended. Please sign in again." Nothing
  she had open still acts as a Trainee.
Actual (2026-09-26, T295 replay, wombat_scenario): When I returned to the tab (about three minutes after the completion
  at 12:15 UTC) it had already reloaded to /account/login?error=SessionEnded&returnUrl=%2Fportfolio%2Fprogress, reading
  "Your session has ended. Please sign in again." above an empty sign-in form; nothing of the Trainee page remained.
Gap: none

## Phase 5.D — The graduate's record

### Step 5.20 — Dr Molefe signs in again and reads her record
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /account/login → /account/login/submit → /portfolio/progress
Do: Sign in from the page that Step 5.19 left her on. She is returned to My progress. Read it.
Expect: Her trainee record admits her to the page (T252, `TraineeOrFormerTrainee`).
  - An information notice reads "You completed your programme on <`D` as a long date>. This page is your record of it
    and is read-only: no target applies to you any more.", followed by D49's rule.
  - A "Your programme" card gives her start date, Completed `D`, and training year 4 as the year the programme ended.
  - Each EPA's card lists its periods, newest first. The period holding `D` reads "no target (your programme ended
    part-way through) · n recorded", and the earlier periods read as met or short.
  - There are no progress bars.
  - The standing reads the year her programme ended in, with the exit rule met.
  - Her trajectories are still shown.
Actual (2026-09-26, T295 replay, wombat_scenario): Signing in from the session-ended page returned her to
  /portfolio/progress. Notice: "You completed your programme on 26 September 2026. This page is your record of it and is
  read-only: no target applies to you any more. A period your programme ended in before that period's last month has no
  target, and no period after it is listed." "Your programme": Started 15 January 2023, Completed 26 September 2026,
  "Training year 4 when your programme ended". Each card lists its periods newest first: "Semester 2, 2026 no target
  (your programme ended part-way through) · 3 recorded" (PAED-001; 0 on the others), "2026 academic year no target …" on
  the yearly EPAs, earlier periods "0 of 3, 3 short". No progress bars. The standing reads "You completed your programme
  on 26 September 2026, in training year 4 …", "15 at or above …" and "15 of 15 … Every EPA is at its exit level."
  Trajectories still shown.
Gap: none

### Step 5.21 — What her home page and menu offer a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /
Do: Open Home and read the menu.
Expect: The menu offers Home, My Account, Data Rights, My Progress and Logout, and nothing else (DESIGN § The NavMenu,
  T252). Home says "You completed your programme on <`D` as a long date>, so no target applies to you any more." and
  points her to My progress, read-only, as the trainee dashboard's Curriculum targets card does for an ended programme
  (T252 As built: "Home says the same").
Note: T252 checked Home only on a withdrawn trainee, who keeps the Trainee role. For an account with no role, Home's
  code shows no dashboard, only the "No role assigned" card ("… An administrator can give you one."; DESIGN § Dashboard
  page), which does not mention her programme or My progress.
Actual (2026-09-26, T295 replay, wombat_scenario): Menu: Home, My Account, Data Rights, My Progress, Logout, nothing
  else (as decided). Home reads "Welcome, molefe@kgk.wombat.local", "No role assigned", "Your account holds no role at
  the moment, so there is nothing to show you here. An administrator can give you one." It does not say that she
  completed her programme, and it does not point to My progress.
Gap: [F-5.21a, T311] A graduate's Home shows the "No role assigned … An administrator can give you one." card instead of the
  decided "You completed your programme on 26 September 2026, so no target applies to you any more." with the pointer to
  My progress (T252 As built: "Home says the same"). This confirms the suspect "A graduate's Home says 'No role assigned
  …'".

### Step 5.22 — What a graduate can no longer open
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /committee/my-reviews → /access-denied → /portfolio/authorisations → /access-denied → /msf/my-reports → /access-denied → /activities/new
Do: Type the addresses of My Committee Reviews, My authorisations and MSF Reports in turn. Then type the address of the
  new-activity page.
Expect: Each of the first three shows Access denied: those pages still require the Trainee role (DESIGN § The NavMenu).
  Nothing offers her an activity to file: there is no menu link and no dashboard action. The new-activity page asks
  only that she is signed in, so it opens when she types its address. She files nothing.
Note: No decision says whether a graduate may still file about herself. Nothing refuses her today: the page and
  `ActivityService.CreateDraftAsync` check no role. T281 decides only that an encounter after her last day credits
  nothing.
Actual (2026-09-26, T295 replay, wombat_scenario): Typed /committee/my-reviews, /portfolio/authorisations and
  /msf/my-reports: each went to /access-denied?ReturnUrl=… "Access denied. You do not have permission to view this page.
  …". Neither the menu nor Home offers an activity. Typed /activities/new: it opens ("New activity", "Create a draft or
  submit an activity from the published catalogue.") with the type list, Case-Based Discussion (Paediatrics) to
  Reflective Exercise (Paediatrics) and KGK Teaching Session Log among them. Nothing filed.
Gap: [F-5.22a, T311] A graduate can no longer open My authorisations, the only page that downloads her STAR certificates.
  DESIGN § The NavMenu names only MSF Reports and My Committee Reviews as graduate pages that still require the role,
  and no decision covers My authorisations (confirms the suspect "A graduate loses My authorisations"). [F-5.22b, T153]
  /activities/new admits a graduate with no role and offers her the KGK instruments to file and submit about herself
  (confirms the suspect that a former trainee "can still open /activities/new"). Unsure: no decision says whether she
  may file, and nothing was filed.

### Step 5.23 — Dr Molefe exports her portfolio as a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/export → /portfolio/verify
Do: Type the export address, set the dates to `J−3y` and `D`, and export. Then verify the new file's SHA-256
  on the verification page.
Expect: The export page is not in her menu, but it admits her, because the portfolio is her own (`MayReadAsync`). The
  file has a new name. Its "Progress per EPA" now says "The trainee completed the programme on <`D`>", so the content
  differs from Steps 5.9 to 5.12. The new file verifies, with the same trainee, the same filter and its own
  file name. The earlier file still verifies too.
Actual (2026-09-26, T295 replay, wombat_scenario): Export Portfolio is not in her menu. Typed /portfolio/export:
  admitted. With 2023-01-15 to 2026-09-26: "Portfolio exported successfully. …" and a new file,
  portfolio-3db2ef8e7d2b.pdf (SHA-256 3db2ef8e…a41c). Its "Progress per EPA" opens "The trainee completed the programme
  on 26 September 2026. Targets as on 26 September 2026 (training year 4). A period the programme ended in before that
  period's last month has no target. …", and the 2026 periods read "no target (the programme ended part-way through), n
  recorded". Verify: "Export verified", generated 2026-09-26 12:17 UTC, the same Trainee ID and exporter (59ba7d01-…),
  "Filter: 2023-01-15 to 2026-09-26", "File: portfolio-3db2ef8e7d2b.pdf". The earlier file (68a17034…) still verifies
  (12:09 UTC, portfolio-68a170344cf3.pdf).
Gap: [F-5.23a, T311] A graduate is offered no link to Export Portfolio, although the page admits her and the graduation email
  says her portfolio "is available in Wombat". She reaches it only by typing its address (confirms the suspect "A
  graduate is offered no Export Portfolio link"). The step's Expect describes this behaviour as it is.

## Phase 5.E — A registrar leaves

### Step 5.24 — Dr du Plessis asks for a Mini-CEX on a shift after his post ended
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) on PAED-002 with:
  - encounter date `D−1`;
  - Dr Naidoo as the assessor;
  - a clinical setting, a presenting problem and a case complexity.
  Submit it.
Expect: The EPA list offers PAED-002, because its tool list includes the Mini-CEX. There is no late-filing warning. The
  activity reads Requested and appears in Dr Naidoo's inbox. No email is sent: Wombat mails nobody when an activity
  moves (Step 3.3).
Note: In the story, his registrar post ended on `D−2` when he resigned to move provinces, but HR tells Prof Mbatha only
  today. He worked `D−1` as a locum shift.
Actual (2026-09-26, T295 replay, wombat_scenario): New activity → Mini-CEX (Paediatrics): the EPA list offered PAED-001,
  002, 003, 004, 006, 007, 008, 012 and 013 (PAED-002 among them), and the assessor list offered Botha, Khumalo, Naidoo,
  Patel and Zulu. With PAED-002, David Naidoo, date observed 2026-09-25, Ward, a presenting problem and Moderate, the
  date's status line stayed empty (no late-filing warning). Submit → /activities/24: "Submitted. It is now Requested.",
  Encounter date 2026-09-25, history Create and Submit by Pieter du Plessis. No mail was sent: the sink still holds 90
  files.
Gap: none

### Step 5.25 — Dr Naidoo rates it
Role: Assessor — Dr David Naidoo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr du Plessis's Mini-CEX from the inbox and rate it `3b`, PAED-002's minimum in training year 2. Write what
  was done well, what to develop and the agreed plan, and complete it.
Expect: The activity reads Completed and is read-only to both of them. It credits PAED-002 in the semester that holds
  `D−1` (checked in Step 5.26), and My Activities reads it credited "1 item". Nobody is emailed.
Actual (2026-09-26, T295 replay, wombat_scenario): Dr Naidoo's inbox lists the Mini-CEX (Pieter du Plessis, PAED-002,
  2026-09-25, Requested), offered Complete, Decline and Discard changes. He rated it 3b, wrote what was done well, areas
  for development and the agreed plan, and pressed Complete. It reads Completed with no editable field or action left.
  The history row "Complete, Requested → Completed, David Naidoo, 2026-09-26 14:19" is credited "1 item", and Dr du
  Plessis's My Activities reads "Completed, 1 item". PAED-002's 2026 S2 progress row is now 1 (there was none before).
  No mail: the sink still holds 90 files.
Gap: none

### Step 5.26 — Dr du Plessis sees it counted
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress
Do: Open My progress and read PAED-002's card for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX, shown as the latest encounter. Record the count
  as n.
Actual (2026-09-26, T295 replay, wombat_scenario): My progress, Semester 2, 2026, training year 2: PAED-002 "1 of 3 this
  semester", "2 more by 30 November 2026. At the minimum level when observed: 1 of 1. Last encounter date: 2026-09-25.",
  "Minimum now 3b". So n = 1 (the D−1 Mini-CEX is his only credited PAED-002 encounter).
Gap: none

### Step 5.27 — Prof Mbatha records his withdrawal
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Open Dr du Plessis's profile and set the last day in the programme to `D−2`. Press Deactivate and confirm.
Expect: The confirmation names `D−2` as his last day. It says that encounters after that day will count towards
  nothing, including any already counted, and that neither the day nor the profile can be changed back. After
  confirming:
  - the page says "Trainee profile deactivated. Their last day in the programme is recorded as `D−2`.";
  - the summary reads Status Inactive, "Left the programme: `D−2`";
  - on Trainees he is listed under Completed & closed profiles, with the outcome "Withdrawn `D−2`";
  - in the same save, the credit from the `D−1` Mini-CEX is taken back (T281);
  - he keeps the Trainee role and is not emailed.
Actual (2026-09-26, T295 replay, wombat_scenario): Edit Pieter du Plessis (/admin/trainees/edit?id=4), last day
  2026-09-24, Deactivate: the dialog "Deactivate this trainee profile?" reads "… with 2026-09-24 as their last day in
  the programme? Encounters observed after that day will count towards nothing in this programme, including any already
  counted. The day cannot be changed afterwards, and a deactivated profile cannot be made active again." Confirmed:
  "Trainee profile deactivated. Their last day in the programme is recorded as 2026-09-24."; Status Inactive, "Left the
  programme: 2026-09-24" (Save profile still offered). Trainees: Active lists Dlamini, Mahlangu and Ndlovu; Completed &
  closed lists "Pieter du Plessis … Withdrawn 2026-09-24" and "Lerato Molefe … Completed 2026-09-26". In the same save,
  his PAED-002 2026 S2 progress row went (count 0) and #24's Complete transition was re-stamped CreditedItemCount 0. He
  keeps the Trainee role; no mail (90 files).
Gap: none

### Step 5.28 — Dr du Plessis's record after he left
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → /account/login/submit → / → /portfolio/progress → /activities/mine
Do: Sign in, then read Home, My progress and My Activities.
Expect: Each page records that his programme ended on `D−2`:
  - Home's Curriculum targets card says "Your programme ended on <`D−2` as a long date>, so no target applies to you any
    more.", and points to My progress.
  - My progress opens with "Your programme ended on <`D−2`>. This page is your record of it and is read-only…". Its
    "Your programme" card reads Ended `D−2`. PAED-002's period holding `D−2` reads "no target (your programme ended
    part-way through) · n−1 recorded".
  - My Activities still lists the `D−1` Mini-CEX as Completed, now credited "None": the take-back's replay re-stamped
    its completion (`ProgrammeEndCredit`). The filing is kept, but it counts towards nothing.
Actual (2026-09-26, T295 replay, wombat_scenario): Home (Viewing as Trainee): the Curriculum targets card reads "Your
  programme ended on 24 September 2026, so no target applies to you any more. Your progress in each period is kept on My
  progress, read-only." and links to /portfolio/progress; the card still offers Log an activity and Request an
  assessment. My progress: "Your programme ended on 24 September 2026. This page is your record of it and is read-only:
  no target applies to you any more. …"; "Your programme" reads Started 15 January 2025, Ended 24 September 2026,
  training year 2; PAED-002 "Semester 2, 2026 no target (your programme ended part-way through) · 0 recorded" (n−1 = 0);
  no bars. My Activities: the Mini-CEX, 2026-09-25, Completed, credited "None". Its page (/activities/24) warns "This
  activity is complete, but it counted towards no curriculum requirement. … or the encounter is dated after the
  trainee's programme ended. …", and its Complete row reads credit None.
Gap: none

### Step 5.29 — Neither ended programme can be put before the panel
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Open Schedule review, choose the Paed Annual Review Panel and read the trainee list.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu. A graduate and a registrar who has withdrawn are not
  current trainees (T238). He closes the form without scheduling anything.
Actual (2026-09-26, T295 replay, wombat_scenario): Committee reviews → Schedule review → Paed Annual Review Panel: the
  Trainee list offers only Anele Dlamini, Nomsa Mahlangu and Sipho Ndlovu (neither Dr Molefe nor Dr du Plessis). He
  closed the form with Schedule review; nothing created.
Gap: none

## Act 5 outcome state

Replay check (2026-09-26, T295, wombat_scenario): match, after two corrections to the SQL below. Molefe is false, 2026-09-26, null; du Plessis is false, null, 2026-09-24. Molefe holds no role and du Plessis holds Trainee. The current trainees are dlamini, mahlangu and ndlovu. There are 15 active STARs, nine at 5 and six at 4, each Order equal to MinimumLevelOrder. Review 7 reads 2, 4, 7, 4, with 0 pending. The exports are portfolio-68a170344cf3.pdf ×3, then portfolio-3db2ef8e7d2b.pdf ×1. The first correction: the after-the-last-day query also returned Act 3's cancelled DOPS #8 and submitted PLR #10, so it now filters on completed and returns #24 only. The second: his PAED-002 2026 S2 progress has no row (n−1 = 0).

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
  profile is inactive: a later step that edits it saves an ended profile, and any count of "active trainees on 11.1"
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

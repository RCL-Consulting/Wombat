# Act 5 — Year 4: graduation

**Actuals before T361.** An Actual dated before the T361 replay describes the review page with deferral, which D53
removed; the Expects are the page as built.

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
  with no expiry, and PAED-010 at `4`, expiring 20 days after Act 4's sitting. Her other twelve EPAs are undecided:
  Act 4 recorded each as "Not decided at this sitting" (D53), nine of them due there and PAED-008, 009 and 013
  optional.
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
- The standing is read on `D`; a committee review's subtitle reads the training year at the end of the review's
  period (T361, E6). The two agree on a replay in semester 2, whose period ends on 31 December, before the next 15
  January. On a replay in semester 1 between 1 and 14 January, the period ends on 30 June, after the 15 January, so the
  subtitle reads a year more than the standing.
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
Do: Open Decisions due for the period holding `D`. Set Trainee to Dr Molefe and Status to "Every status", and press
  Show. Then set Trainee back to "Every trainee" and press Show.
Expect: The line above the list reads "No open review holds `<P>`." with Schedule a review: Act 4's review of her is
  ratified. There is one row for each EPA due for her in the period ("15 of 75 decisions due in `<P>` shown: Lerato
  Molefe, every status."), and each row gives its status in words:
  - an EPA that an Act 4 STAR decided reads Decided, and names the STAR and the review that issued it;
  - an EPA due at Act 4's sitting and left unstaged there reads Not decided (amber), "Not decided at review #N. A later
    sitting for … plans it again." (T361, D53, E3);
  - the others read Not scheduled, Due by year end or As opportunity allows: PAED-008, 009 and 013, recorded as not
    decided at Act 4 but optional, read As opportunity allows while their window `<Y>` is open (E3).
  Every row that is not yet decided offers Schedule. While the filter is set only her rows are listed; with Every
  trainee, KGK's other registrars are listed too, and nobody outside KGK is.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Decisions due for Mr Smit on 2026 S2. Trainee Molefe, Every
  status, Show: "15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, every status." and "No open review holds 2026
  S2." with Schedule a review. Her 15 rows: PAED-001, 010, 012 "Decided STAR #1 [#2, #3], issued at review #1."
  ("Decided: nothing to schedule"); nine "Not decided at review #1. A later sitting for 2026 S2 [or 2026] plans it
  again." (Schedule); PAED-008, 009, 013 "As opportunity allows: Decided as opportunity allows: optional, never
  missed." (Schedule). Every trainee: "75 of 75 decisions due in 2026 S2 shown: every status.", "Decisions, page 1 of
  4", "Showing 1–20 of 75", focus on that heading (other pages not paged through). No sideways scroll at 390 or 768
  px.
Gap: none

### Step 5.2 — Mr Smit schedules Dr Molefe's pre-graduation review
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: Set Trainee to Dr Molefe again and press Show (Step 5.1 ended on Every trainee, where no trainee line shows). Press
  Schedule a review on Dr Molefe's line (or Schedule on one of her outstanding rows). Then complete the form:
  - review type: Pre-graduation review;
  - evidence window: From widened to her programme start, `J−3y`;
  - scheduled on: `D`.
  Create the review.
Expect: The form opens with the Paed Annual Review Panel, Dr Molefe and the period already filled in. Review type offers
  Annual progression review (chosen) and Pre-graduation review, then Formative check-in; Entrustment-only review is
  offered only in a semester-1 period. Pre-graduation review's help reads "The final review before the programme ends:
  it decides the trainee's progression, and whether they may graduate." (C1). The agenda preview lists the EPAs that
  are due and names those Act 4 decided. In the same semester-2 period as Act 4:
  - it reads "12 EPAs will be on the agenda for …", with the period's label;
  - it names the nine EPAs Act 4 did not decide that are due here: "PAED-002, PAED-003, PAED-004, PAED-005, PAED-006,
    PAED-007, PAED-011, PAED-014 and PAED-015 are due at this sitting." (T361, round 3 item 29);
  - it lists PAED-008, 009 and 013 as As opportunity allows;
  - it says "Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012."
  The new review's h1 reads "Lerato Molefe · `<P>`", its subtitle "Pre-graduation review · Paed Annual Review Panel ·
  Training year 4", and its sitting bar Scheduled, "12 EPAs on the agenda" and "Summative · evidence `J−3y` to
  `<Y>`-12-31" (ISO dates). Mr Smit is offered Start review… in the bar, but leaves it to the chair.
Note: A second binding review for a period is refused only while the first one is open (scheduled, in progress or
  decided). Act 4's review of her has been ratified.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Trainee Molefe, Show, then the line's Schedule a review opened
  /committee/reviews/new?panel=1&trainee=...&period=2026-2&return=...: Paed Annual Review Panel, "Lerato Molefe ·
  Training year 4", "2026 S2 · 1 Jul to 31 Dec 2026" filled; From 2026-01-01, To 2026-12-31, Scheduled on 2026-10-20.
  Review type offered Annual progression review (chosen), Pre-graduation review, Formative check-in (period 2026 S2,
  so no Entrustment-only; its help text and the S1 case were not re-read this replay). With From 2023-01-15 and
  Pre-graduation: "12 EPAs will be on the agenda for 2026 S2.", "PAED-002, PAED-003, PAED-004, PAED-005, PAED-006,
  PAED-007, PAED-011, PAED-014 and PAED-015 are due at this sitting.", PAED-008, 009, 013 "· As opportunity allows",
  "Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012." Create review opened
  /committee/reviews/7: h1 "Lerato Molefe · 2026 S2", subtitle "Pre-graduation review · Paed Annual Review Panel ·
  Training year 4", bar Scheduled, "12 EPAs on the agenda", "Summative · evidence 2023-01-15 to 2026-12-31", Refresh,
  Start review… (left unpressed). No sideways scroll at 390 or 768 px on the form. No mail (stub count 38 to 38).
Gap: none

### Step 5.3 — Dr Zulu starts the review and reads where Dr Molefe stands
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's pre-graduation review from Committee reviews and start it through its dialog. Read the evidence
  snapshot, the agenda and "Entrustment against Annexure A".
Expect: Before Start, the h1 reads "Lerato Molefe · `<P>`" with "Pre-graduation review" in the subtitle, and the bar
  "Summative · evidence `J−3y` to `<Y>`-12-31" (round 3 item 28). The Start dialog counts "the [n] items observed from
  `J−3y` to `<Y>`-12-31 (…)". The page then says "Review started." atop the agenda and the review reads In progress.
  - Above the agenda's caption "12 EPAs for `<P>` · none staged", it says "Already decided in this window, so not on
    the agenda: PAED-001, PAED-010 and PAED-012." (round 3 item 25).
  - The snapshot freezes her evidence observed since `J−3y`, grouped by EPA, including the released MSF
    campaign.
  - The standing reads training year 4, and "2 at or above · 1 below · 12 with no decision, of 15 EPAs". Its exit rule
    reads "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)", followed by "Not
    yet: …", which names every EPA but PAED-001 and PAED-012.
  - Under the rule: "For information only. Recording a Graduate decision or completing the programme does not check the
    exit rule."
  - Record decision… is held back only by the quorum (D53): unavailable, with the quorum line beside it, until a
    second member is ticked. It names no agenda line.
  - Sampling warnings may name EPAs rated by fewer than three assessors. They are warnings and refuse nothing.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Committee reviews (Zulu, Committee member) lists review #7
  Pre-graduation, Scheduled, Pending, above the Ratified annual one. Before Start: h1 "Lerato Molefe · 2026 S2",
  subtitle "Pre-graduation review · Paed Annual Review Panel · Training year 4", bar "Summative · evidence 2023-01-15
  to 2026-12-31". Start dialog: "Starting freezes the evidence: the 9 items observed from 2023-01-15 to 2026-12-31 (8
  activities and the MSF campaign report) become this review's snapshot ...", focus on Not yet. Started: "Review
  started." (focus there); bar In progress, "None staged", "Present: the chair · one more member needed". "Already
  decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012." above the caption "12 EPAs for 2026
  S2 · none staged". Snapshot "Frozen when the review started, 2026-10-20 · 9 items", by EPA (MSF records #22 under
  PAED-010, #23 under PAED-012), with "Default MSF #1 · Multi-source feedback. State: Released; responses 8; closed
  2026-10-20." Standing "In training year 4 on 2026-10-20", "2 at or above · 1 below · 12 with no decision, of 15
  EPAs"; "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6). Not yet: PAED-002 ...
  PAED-015" (all but 001 and 012), then "For information only. Recording a Graduate decision or completing the
  programme does not check the exit rule." Record decision… aria-disabled beside "Only the chair is ticked. A
  committee decision needs at least two panel members present: the chair and at least one other." Sampling warnings on
  PAED-010 (1 assessor) and PAED-012 (2), refusing nothing. T356: at 1280 and 768 px the sticky bar (56-110 px) never
  covers Sign out (14-42 px). No sideways scroll at 390 px.
Gap: none

### Step 5.4 — Dr Zulu stages a STAR at its exit level on every EPA not yet there
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: For each EPA the standing names under "Not yet", stage a decision at its exit level. That is thirteen EPAs, every
  one but PAED-001 and PAED-012, which Act 4 decided at their exit level:
  - rung `5` on PAED-002 to 007 and PAED-010;
  - rung `4` on PAED-008, 009, 011, 013, 014 and 015.
  Use Stage on each of the twelve agenda lines (PAED-002 to 009, 011, 013, 014 and 015), each resting on Default MSF #1
  alone, since none of them has a line of its own. For PAED-010, which Act 4 decided in this window below its exit
  level, use "Stage a decision on an EPA not on this agenda" at the agenda's foot, choosing it in its EPA list and
  ticking its own two lines. Issue each decision on `D` with no expiry and write a rationale.
Expect: The level list offers only the six rungs of the CPSA Paediatric Entrustment Scale v11.1 (`1`, `2`, `3a`, `3b`,
  `4`, `5`). Stage pending decision stays unavailable, with "Name at least one item of the evidence snapshot." beside
  it, until a line is ticked. A decision on Default MSF #1 alone gets both hints, "Only one item is named. The College's
  rule is that an entrustment decision is never taken from a single form." and "None of the named items is about
  PAED-002." (and so on), and is staged anyway. After each staging:
  - "Pending entrustment decision staged." sits under the EPA's line, with the focus there;
  - the line reads Staged, with its decision in words ("Level 5 · issued `D` · no expiry.") and "Rests on 1 item of
    the snapshot: Default MSF #1." with the hints;
  - PAED-010 gains a chair's line, reading Staged with "Added by the chair.", on its two lines [the Direct
    Observation #n and the MSF record #n].
  Once all thirteen are staged, the caption reads "13 EPAs for `<P>` · 13 staged" and "Already decided in this window,
  so not on the agenda: PAED-001 and PAED-012." (round 3 item 25). Record decision… is held back only by the quorum.
Note: The exit levels are v11.1's final minima: nine EPAs at `5` and six at `4` (T166).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Staged 13 decisions on review #7, all issued 2026-10-20, no
  expiry: rung 5 on PAED-002 to 007 and 010, rung 4 on PAED-008, 009, 011, 013, 014, 015. Each agenda line's form
  ("Stage a decision on PAED-002", "Year 4 target 5 · no STAR yet · 0 ratings in the review window · MSF: None
  released yet") offered Choose a rung…, 1, 2, 3a, 3b, 4, 5; Stage pending decision aria-disabled beside "Name at
  least one item of the evidence snapshot." until a line was ticked. The twelve rest on Default MSF #1 alone and got
  both hints ("Only one item is named. The College's rule is that an entrustment decision is never taken from a single
  form." and "None of the named items is about PAED-002."), staged anyway: "Pending entrustment decision staged.";
  "Staged", "Level 5 · issued 2026-10-20 · no expiry.", "Rests on 1 item of the snapshot: Default MSF #1.", Remove….
  PAED-010 via the foot's "Stage a decision on an EPA not on this agenda", ticking Direct Observation #17 and
  Multi-Source Feedback #22 (no hints): "Pending entrustment decision staged." with focus there, a line "Staged Added
  by the chair. Level 5 · issued 2026-10-20 · no expiry. Rests on 2 items". Caption "13 EPAs for 2026 S2 · 13 staged"
  under "Already decided in this window, so not on the agenda: PAED-001 and PAED-012." Record decision… held back only
  by the quorum. No sideways scroll at 390 px.
Gap: none

### Step 5.5 — The panel records its decision: Graduate
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Choose Go to the decision and record the committee's decision, confirming in the dialog:
  - category: Graduate (programme complete);
  - rationale: every EPA is entrusted at its exit level, and the panel recommends completion;
  - About the EPAs not decided: nothing (every line is staged);
  - conditions: none;
  - present: Dr Zulu, Dr Naidoo, Dr Botha and Dr van Rensburg.
Expect: The category list opens with nothing chosen and offers Graduate (programme complete) among the progression
  outcomes. Dr Zulu is ticked as present and cannot be unticked. Beside the available Record decision…: "Recording fixes
  the 13 staged decisions." The dialog, "Record Graduate (programme complete) for Lerato Molefe?", reads "Fixes 13
  staged decisions: …" naming each EPA at its level, and records none as not decided: it has no "Records … as not
  decided" line (E1). The review then reads Decided (T250), with "Decision recorded." in the Decision section, now above
  the agenda. The decision is headed "Graduate (programme complete)" and names the four members present. The agenda
  says "Fixed when the committee's decision was recorded: ratifying issues exactly these." The foot form and every
  Remove… are gone, the bar reads "Graduate (programme complete) · 13 decisions to issue", and only Dr Zulu is offered
  Ratify…; Dr Naidoo's static bar reads "Only the panel's chair, Thandi Zulu, can ratify the committee's decision."
Note: Recording a Graduate decision checks nothing against the exit rule. T166 deferred that check until the College
  confirms the rule.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Go to the decision: Category opened on "Select a category…"
  and offered Satisfactory Progress, Satisfactory with Observations, Inadequate Progress — Additional Training,
  Inadequate Progress — Repeat, Release from Training, Outcome Deferred and Graduate (programme complete); "Thandi
  Zulu (chair)" ticked and disabled. With the other three ticked: "4 present: the chair and 3 others. The quorum is
  met.", "Recording fixes the 13 staged decisions." Dialog "Record Graduate (programme complete) for Lerato Molefe?":
  "Fixes 13 staged decisions: PAED-002 at 5, ..., PAED-015 at 4.", "Present: Thandi Zulu (chair), Sarah Botha, David
  Naidoo, John van Rensburg (external)"; no "Records ... as not decided" line. "Decision recorded."; the Decision
  section (above Agenda) reads Graduate, the rationale and the four present; agenda "Fixed when the committee's
  decision was recorded: ratifying issues exactly these."; no foot form and no Remove…; bar Decided, "Graduate
  (programme complete) · 13 decisions to issue", Ratify… (Dr Zulu only). Dr Naidoo's bar: "Only the panel's chair,
  Thandi Zulu, can ratify the committee's decision.", "As of 14:23 SAST", Refresh ("No change.").
Gap: none

### Step 5.6 — Dr Zulu ratifies
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Choose Ratify… and confirm in the dialog.
Expect: The dialog, "Ratify and issue 13 STARs to Lerato Molefe?", lists the thirteen ("PAED-002 at 5, issued `D`" and
  so on) and names no EPA as not decided; its confirm is "Ratify and issue 13 STARs". The page says "Decision
  ratified." under the bar, and the review reads Ratified, "Graduate (programme complete) · 13 STARs issued". Every
  staged decision is issued as a STAR in the same action, STARs #6 to #18 (which EPA holds which number is read at the
  replay: [#n] each):
  - each agenda line reads Decided with its "STAR #n.";
  - the agenda says "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one.";
  - Act 4's active STAR on PAED-010 [#2], re-decided today as [#18], is superseded.
  The standing's exit rule reads "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of
  6). Every EPA is at its exit level." Its year line reads "15 at or above · 0 below · 0 with no decision, of 15 EPAs".
Note: Only the panel's chair ratifies. There is no Administrator bypass (D46, T165).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Ratify…: dialog "Ratify and issue 13 STARs to Lerato Molefe?"
  listing "PAED-002 at 5, issued 2026-10-20" ... "PAED-015 at 4, issued 2026-10-20", "Ratifying cannot be undone:
  nothing on this review can change afterwards.", Not yet and "Ratify and issue 13 STARs"; none named as not decided.
  "Decision ratified."; bar Ratified, "Graduate (programme complete) · 13 STARs issued". STARs PAED-002 #6, 003 #7,
  004 #8, 005 #9, 006 #10, 007 #11, 008 #12, 009 #13, 011 #14, 013 #15, 014 #16, 015 #17, 010 #18; each line "Decided
  STAR #n."; "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
  one." Act 4's PAED-010 STAR #2 superseded (Step 5.7). Standing "15 at or above · 0 below · 0 with no decision, of 15
  EPAs" and "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at
  its exit level." Dr Naidoo's Refresh then read Ratified. No mail (38 to 38).
Gap: none

### Step 5.7 — Prof Mbatha reads Dr Molefe's STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from the menu. Filter by the trainee name "Molefe", first with the
  status Active and then with All, pressing Show each time.
Expect: There are fifteen Active rows, captioned "… · 15 shown", one for each EPA from PAED-001 to PAED-015. Each row:
  - is at its EPA's exit level;
  - was issued on `D`, except PAED-001's and PAED-012's, which Act 4 issued;
  - has no expiry;
  - offers Download and Revoke.
  Newest issued first, then by STAR number descending: review #7's thirteen come first (T366, F1). With All, sixteen
  ("… · 16 shown"): Act 4's PAED-010 STAR, which Step 5.6 superseded, reads Superseded, offers Download only, and
  carries the line "Superseded by STAR #18, issued `D`." (no supersession date is stored; S4). She revokes nothing.
Note: If PAED-010's Act 4 expiry passed and the expiry job ran before Step 5.6, that STAR reads Expired instead, and
  ratifying superseded none.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home's quick links: Invite a person, Users, Invitations,
  Curriculum adoptions, Entrustment decisions. Trainee "Molefe", Active, Show: "Issued STARs (entrustment decisions) ·
  15 shown", focus back on Show; PAED-001 to 015 at their exit levels, review #7's thirteen first (STAR #18 down to
  #6), then PAED-012 and PAED-001 at review #1, all issued 2026-10-20 (Act 4 was played on D); Expires "—", Active,
  Download and Revoke. All: "· 16 shown"; the extra PAED-010 at 4, review #1, Expires 2026-11-09, Superseded, Download
  only, "Superseded by STAR #18, issued 2026-10-20." Nothing revoked. No sideways scroll at 390 or 768 px.
Gap: none

### Step 5.8 — Dr Molefe reads her final review and keeps a STAR certificate
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → /committee/my-reviews/{ReviewId:int} → / → /portfolio/authorisations
Do: Open My committee reviews and view the pre-graduation review. Then open My authorisations from her Home's card (its
  Open My authorisations), and download the certificate for PAED-001.
Expect: The pre-graduation review is listed above the Act 4 annual review (T366, F1), as Pre-graduation, Ratified,
  Graduate (programme complete). The two Views are named "View the `<P>` review before Paed Annual Review Panel,
  Pre-graduation" and "…, Annual progression" (M1). Her review's page, "`<P>` pre-graduation review", shows:
  - the review type, "Pre-graduation review";
  - the decision and its rationale;
  - who was present: "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)";
  - an agenda in which each of its thirteen EPAs reads Decided, with its STAR number, #6 to #18.
  The lodge block is offered, and she lodges no appeal. My authorisations holds 15 cards, each with its level, Issued
  `D` and "No expiry", and no "No longer in force" section: a superseded STAR is not listed. The certificate downloads
  as `star-certificate-PAED-001-<id>-<8 hex digits>.pdf`.
Note: My authorisations requires the Trainee role, so this is the last point at which she can download a certificate
  from that page (see Step 5.22).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Menu (Acting as Trainee) Home, Log an activity, My activities,
  MSF reports, My committee reviews, Export portfolio, My progress, My data rights. "Your committee reviews, newest
  first": Pre-graduation, Ratified, Graduate (programme complete) first, then Annual progression, Ratified,
  Satisfactory Progress; Views named "View the 2026 S2 review before Paed Annual Review Panel, Pre-graduation" and
  "..., Annual progression". Page "2026 S2 pre-graduation review": Type "Pre-graduation review", Ratified, "Graduate
  (programme complete)", the rationale, "Present: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van Rensburg
  (external)"; Appeal block with Lodge appeal (not used); agenda of 13, each "Decided STAR #n." (#6 to #18). Home's
  Open My authorisations: "In force", 15 cards, each Issued 2026-10-20 and "No expiry", no "No longer in force".
  PAED-001's certificate downloaded as star-certificate-PAED-001-1-28aaa21c.pdf (97,279 bytes, Active, issued 20
  October 2026). No sideways scroll at 390 or 768 px on the three pages.
Gap: T370 (known proposal: the panel's member order: Present lists Sarah Botha before David Naidoo; the members match)

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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Export portfolio from the menu opened with From 2025-10-20 and
  To 2026-10-20 (the last twelve months). With 2023-01-15 to 2026-10-20, Export PDF: "Portfolio exported successfully.
  The download should start automatically." and portfolio-146f1145d0de.pdf downloaded (185,212 bytes; SHA-256
  146f1145...ec405c, recorded for 5.10, 5.12 and 5.13).
Gap: none

### Step 5.10 — Prof Mbatha exports Dr Molefe's portfolio by its address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId} → /portfolio/export/{TraineeUserId}
Do: Find Dr Molefe in Users (type "Molefe" in Name or email and press Show) and open her. Take her user id from the
  path of the page's address (before any `?`: Manage adds `?back=`), then type the export address with that id. Set the dates to `J−3y` and `D`, and export.
Expect: The export succeeds as in Step 5.9. The downloaded file has the same name as Dr Molefe's and is identical to it
  byte for byte. Compare the two with `fc /b`, or by their SHA-256.
Note: No page links to the staff export, so this step reaches it by typing the address. Record that in `coverage.md`.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Users, filter "Molefe" (Lerato Molefe, Trainee, Active),
  Manage opened /admin/users/022fbd7d-.... Typed /portfolio/export/022fbd7d-...: the page opened with 2025-10-20 to
  2026-10-20 and reads only "Generate a PDF export of the trainee's portfolio.", naming nobody, before and after. With
  2023-01-15 to 2026-10-20: "Portfolio exported successfully. ..." and portfolio-146f1145d0de.pdf, byte-identical to
  Dr Molefe's (the same SHA-256).
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The 11-page PDF (read as text) carries on every page the
  header Kgosi Kgari Teaching Hospital, "Portfolio Export", Lerato Molefe, Paediatric EPA Curriculum, "Paediatrics --
  Paediatrics", "Period: 2023-01-15 to 2026-10-20", and the footer "Page n of 11 | Generated by Wombat"; no generation
  time (metadata dates fixed at 2000-01-01). Sections in order: Summary ("Total activities: 8", by type, "Committee
  reviews: 2", "MSF reports: 1"); "Statements of Awarded Responsibility (STARs)", "15 active entrustment decisions on
  record.", rungs 5 or 4; "Progress per EPA", "Targets as on the trainee's progress page on 20 October 2026 (training
  year 4)"; "Committee Decisions": Act 4's annual review, then "Type: Pre-graduation review", "Status: Ratified",
  "Decision: Graduate (programme complete)", its rationale, "Present: Thandi Zulu (chair), Sarah Botha, David Naidoo,
  John van Rensburg (external)", "Ratified: 2026-10-20"; Activities by type (people by name); Multi-Source Feedback
  (Default MSF, 8 responses, Consultant avg 4.0 from 3, Nurse avg 4.3 from 3, smaller groups not shown); "Appendix --
  Audit Trail".
Gap: none

### Step 5.12 — Mr Smit reproduces the export
Role: Coordinator — Mr Pieter Smit
Route: /portfolio/export/{TraineeUserId}
Do: Type the export address with Dr Molefe's user id, which Prof Mbatha sends him. Set the same two dates, and export.
Expect: The export is allowed, because a coordinator of her institution oversees her (T079, T101). The file has the
  same name as those from Steps 5.9 and 5.10, and is identical to both byte for byte.
Note: No page that a coordinator can open links to a trainee's export. Her user id is otherwise only in an address he
  has seen: the `trainee=` part of the Schedule link he followed in Step 5.2.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Typed /portfolio/export/022fbd7d-... as Mr Smit: admitted
  ("Generate a PDF export of the trainee's portfolio.", no name). With 2023-01-15 to 2026-10-20: "Portfolio exported
  successfully. ..." and portfolio-146f1145d0de.pdf, byte-identical to Steps 5.9 and 5.10 (the same SHA-256).
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signed out, /portfolio/verify opened under the signed-out bar
  (Wombat and Sign in, no menu), the field "Content hash (SHA-256)" with the placeholder "Enter the hash from the PDF
  footer". Dr Molefe's SHA-256 (lower case) went to /portfolio/verify?hash=146f1145d...&check=1: "Export verified",
  "This PDF was generated by Wombat on 2026-10-20 12:25 UTC.", "Trainee ID: 022fbd7d-...", "Exported by: 022fbd7d-..."
  (ids only), "Filter: 2023-01-15 to 2026-10-20", "File: portfolio-146f1145d0de.pdf". Mr Smit's copy (the same hash)
  got the same answer. The same hash in upper case read "No matching export found. This hash does not correspond to
  any PDF generated by this system."
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): A copy with one byte changed (SHA-256 12cc4688...) went to
  ?hash=...&check=1: "No matching export found. This hash does not correspond to any PDF generated by this system."
  Five spaces went to ?hash=+++++&check=1: "Enter the content hash to check." Neither reached an error page; the page
  stayed signed out.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Dr Molefe signed in in her own browser and opened My progress
  from the menu (a script held the tab open through Step 5.19). "Training year 4 · Semester 2, 2026"; This period "1
  of 10 EPAs met this semester", "0 of 5 EPAs met in 2026", "Semester 2, 2026 ends on 2026-11-30."; Your EPAs: 15 rows
  with each count for the window holding D ("3 of 3 this semester" / "Target met for Semester 2, 2026." on PAED-001,
  "3 more by 2026-11-30" and the like on the others), every STAR "At or above 5" or "At or above 4". "Entrustment
  against Annexure A": "15 at or above · 0 below · 0 with no decision, of 15 EPAs" and "15 of 15 EPAs at their exit
  level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every EPA is at its exit level." No trajectory and no
  period before on My progress. Nothing says her programme has ended.
Gap: none

### Step 5.16 — Prof Mbatha tries to record the story's graduation day
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Molefe's profile from Current trainees (Edit). Set the last day in the programme to the story's graduation day,
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Trainees (menu "Trainees"): "Waiting to be admitted" (Nobody
  is waiting to be admitted. A registrar appears here after registering from a Trainee invitation.) and "Current
  trainees", "5 current", columns Name, Curriculum and sub-speciality, Programme start (with "Training year n"),
  Expected completion, Actions; Lerato Molefe 2023-01-15, Training year 4, 2027-01-14, Edit
  (/admin/trainees/edit?id=2). "Last day in the programme" opened on 2026-10-20; its help reads "Mark complete records
  it as the graduation day, archives the profile and removes the Trainee role. Deactivate records it as the day the
  trainee left without completing, and also removes the Trainee role (D54). It cannot be after today, and it cannot be
  changed afterwards. ...". Set to 2027-01-14, Mark complete: "Mark this programme complete?" "Record 2027-01-14 as
  Lerato Molefe's graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent.
  Encounters observed after that day will count towards nothing in this programme, including any already counted. The
  day cannot be changed afterwards, and this cannot be undone." Confirmed: "The completion date cannot be after today
  (2026-10-20).", visible, the field's aria-describedby naming it (programme-end-refusal); Status still Active. The
  step was re-run once to check that the reconnect text in the DOM is hidden (it is): one extra refused attempt in the
  audit.
Gap: none

### Step 5.17 — Prof Mbatha marks Dr Molefe's programme complete on `D`
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees/edit → /admin/trainees → /admin/users → /admin/users/{UserId}
Do: Set the last day to `D`, press Mark complete and confirm. Then go back to Trainees, and open Dr Molefe in Users.
  On her page, press Add a role…, read it and press Cancel.
Expect: The page says "Trainee marked complete. The Trainee role has been removed and a graduation email sent."
  - The summary reads Status Completed, Completed `D`.
  - The last-day field, Deactivate and Mark complete are gone. The page's subtitle reads "The record of a programme
    that has ended.", and the profile's details read as a list under "This programme has ended, so its record is
    archived and cannot be changed.": Curriculum Paediatric EPA Curriculum (11.1), Programme start date `J−3y`,
    Expected completion date `J+1y−1d`. There are no inputs and no Save profile; Back to trainees stays (T305).
  - On Trainees she is no longer under Current trainees. She is listed under "Completed and closed" with the outcome
    "Completed `D`".
  - Her user page's Roles and scope says "**Lerato Molefe holds no role: they completed the programme on `D`.** They can
    still sign in to read their training record (My progress) and their data rights." (T376, S2, Q13; D54), and its Add
    a role… offers the six staff roles and no Trainee (T303). On Users her Roles cell reads "No role" over "A completed
    or withdrawn registrar keeps an account and its records."
  - No credit is taken back, because nothing of hers is observed after `D` (T281; Step 5.27 shows a take-back).
Note: The completion runs under the trainee credit lock (T281), so any completion of hers that is in flight is waited
  for first.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Last day 2026-10-20 (the default), Mark complete; the dialog
  named 2026-10-20; confirmed (12:28:02 UTC): "Trainee marked complete. The Trainee role has been removed and a
  graduation email sent." Subtitle "The record of a programme that has ended."; "Status: Completed", "Completed:
  2026-10-20"; "This programme has ended, so its record is archived and cannot be changed." over Curriculum
  "Paediatric EPA Curriculum (11.1)", Programme start date 2023-01-15, Expected completion date 2027-01-14, as text;
  no input and no button in the page body, no Save profile; Back to trainees stays. Trainees: "Current trainees" "4
  current" lists Dlamini, du Plessis, Mahlangu and Ndlovu; "Completed and closed" lists Lerato Molefe, "Completed
  2026-10-20". No sideways scroll at 390 or 768 px on Trainees. Her user page: "This user has no roles."; Add role
  offers InstitutionalAdmin to Assessor and "Trainee is not offered: a registrar becomes a trainee only when admitted,
  from Trainees with 'Admit…'." Nothing of hers is observed after D, so nothing was taken back.
Gap: none

### Step 5.18 — Dr Molefe is emailed
Role: System — the graduation email sent by Mark complete
Route: n/a
Do: Read the application log for the mail sent in Step 5.17.
Expect: There is one email to `molefe@kgk.wombat.local` with the subject "Congratulations on completing Paediatric EPA
  Curriculum". It congratulates her on completing the programme as of `D`, and says that her committee has ratified
  her final entrustment decisions and that her portfolio is available in Wombat.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The log holds one mail after the completion, "Stub email
  a6eab23980de (tags: graduation)", subject "Congratulations on completing Paediatric EPA Curriculum": "Dear Lerato
  Molefe, Congratulations on completing your training programme, Paediatric EPA Curriculum, as of 2026-10-20. Your
  committee has ratified your final entrustment decisions and your portfolio of evidence is available in Wombat. ...".
  The stub line prints no recipient, so the To was not checked; no other mail (stub count 38 to 39).
Gap: none

### Step 5.19 — Dr Molefe's open session ends
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /account/session-ended → /account/login
Do: Go back to the tab left open at Step 5.15 and wait up to a minute.
Expect: Removing her role changed her account's security stamp. The session check, which runs once a minute, ends the
  session: the tab reloads to the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Nothing she had open still acts as a Trainee.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The tab held open since Step 5.15 went to
  /account/login?error=SessionEnded&returnUrl=%2Fportfolio%2Fprogress 19 seconds after Mark complete (12:28:02 to
  12:28:21 UTC), reading "Your session has ended. Sign in again." above the sign-in form; nothing of the Trainee page
  remained.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signing in from the session-ended address returned her to
  /portfolio/progress. Notice: "You completed your programme on 2026-10-20. This page is your record of it and is
  read-only: no target applies to you any more. A period your programme ended in before that period's last month has
  no target, and no period after it is listed." "Your programme": Started 2023-01-15, Completed 2026-10-20, "4 when
  your programme ended". Each EPA card's code links to its page (/portfolio/progress/2 ...); periods newest first:
  "Semester 2, 2026 no target (your programme ended part-way through) · 3 recorded" (PAED-001), earlier periods "0 of
  3, 3 short". No progress bars (only the two hidden reconnect bars); no long date. The standing reads "You completed
  your programme on 2026-10-20, in training year 4, so the targets shown are that year's. ...", "15 at or above · 0
  below · 0 with no decision, of 15 EPAs" and "15 of 15 ... Every EPA is at its exit level." PAED-001's page lists all
  eight periods the same way, "Training year 4: level 5, ...", Entrustment 5 "At or above", Issued 2026-10-20, "Exit
  level 5 · reached", the rating trajectory ("3 ratings in the 2026 academic year, ...") and Activities on this EPA;
  no "more by", no long date.
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): No "Acting as" head; the menu is Home, My progress and My data
  rights; the top bar reads "Lerato Molefe" with Sign out. Home shows one card, "Your training record": "You hold no
  role at the moment. Your training record is kept, read-only: your progress in each period, and your portfolio to
  export." with "Open My progress →" (its only link). It does not say that or when she completed her programme.
Gap: [F-5.21a, T311] (still, as the Note records) A graduate's Home does not say "You completed your programme on
  2026-10-20, so no target applies to you any more." (T252 As built: "Home says the same"); its "Your training record"
  card names no completion and offers no export link although it mentions "your portfolio to export".

### Step 5.22 — What a graduate can no longer open
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /committee/my-reviews → /access-denied → /portfolio/authorisations → /access-denied → /msf/my-reports → /access-denied → /activities/new → /access-denied
Do: Type the addresses of My committee reviews, My authorisations and MSF reports in turn. Then type the address of the
  new-activity page.
Expect: Each of the first three reads "You cannot open this page", "Your account holds no role that opens this page."
  and "If you need it for your work, ask your institution's Wombat administrator.", with Go to Home: those pages still
  require the Trainee role (DESIGN § The NavMenu).
  Nothing offers her an activity to file: there is no menu link and no dashboard action, and the new-activity page
  reads the same "You cannot open this page" when she types its address. Only a registrar whose programme is running
  files (D55); the create refuses anyone else too.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Typed /committee/my-reviews, /portfolio/authorisations and
  /msf/my-reports: each went to /access-denied?ReturnUrl=..., reading "You cannot open this page", "Your account holds
  no role that opens this page." and "If you need it for your work, ask your institution's Wombat administrator.",
  with Go to Home. Neither the menu nor Home offers an activity. Typed /activities/new:
  /access-denied?ReturnUrl=%2Factivities%2Fnew with the same three lines and Go to Home (D55). Nothing filed.
Gap: [F-5.22a, T311] (still) A graduate can no longer open My authorisations, the only page that downloads her STAR
  certificates, and no decision covers it.

### Step 5.23 — Dr Molefe exports her portfolio as a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/export → /portfolio/verify
Do: Type the export address, set the dates to `J−3y` and `D`, and export. Then verify the new file's SHA-256
  on the verification page.
Expect: The export page is not in her menu, but it admits her, because the portfolio is her own (`MayReadAsync`). The
  file has a new name. Its "Progress per EPA" now says "The trainee completed the programme on <`D`>", so the content
  differs from Steps 5.9 to 5.12. The new file verifies, with the same trainee, the same filter and its own
  file name. The earlier file still verifies too.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Export portfolio is not in her menu. Typed /portfolio/export:
  admitted (defaults 2025-10-20 to 2026-10-20). With 2023-01-15 to 2026-10-20: "Portfolio exported successfully. ..."
  and a new file, portfolio-e06d16b86584.pdf (186,358 bytes), whose "Progress per EPA" reads "The trainee completed
  the programme on 20 October 2026." Verify: "Export verified", generated 2026-10-20 12:29 UTC, the same Trainee ID
  and exporter (022fbd7d-...), "Filter: 2023-01-15 to 2026-10-20", "File: portfolio-e06d16b86584.pdf". The earlier
  file still verifies (12:25 UTC, portfolio-146f1145d0de.pdf).
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Log an activity's picker, Mini-CEX (Paediatrics)
  (/activities/new?type=mini_cex_cpsa): the EPA list offered PAED-001, 002, 003, 004, 006, 007, 008, 012 and 013. With
  PAED-002, David Naidoo, date observed 2026-10-19 (D-1), Ward, a presenting problem and Moderate, no late-filing
  notice showed; the moves were Submit to David Naidoo and Save draft. Submit went to /activities/24: "Submitted. It
  is now Requested. It is in David Naidoo's Activity inbox."; "With David Naidoo since 2026-10-20 14:29 SAST.", Cancel
  request… offered; Credit "None until it is completed". No mail (stub count 39 to 39).
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
Actual (2026-10-20, T373 replay, wombat_scenario_t373): His Committee member Home (menu Home, Programme trainees,
  Committee reviews, Decision panels, My data rights) carries under the header "1 activity waits for you in the
  Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · 2026-10-19, from Pieter du Plessis, waiting less than a day."
  with Open it. Switch to Assessor: "You are now acting as Assessor."; Home's "Waiting for you" badged "1 waiting".
  Inbox: "Mini-CEX (Paediatrics) · PAED-002 · 2026-10-19", from Pieter du Plessis, Requested, "Less than a day". Rated
  3b on the rung picker with the three fields; Complete: "Completed. Nothing else waits for you.", Go to Home; "Done.
  You completed it on 2026-10-20 14:30 SAST.", "Rated 3b. Credited 1 item to PAED-002."; Credit "1 item"; no action
  left to either of them. Switched back: "You are now acting as Committee member." Dr du Plessis's My activities (with
  Log an activity) reads it "Done, Completed, 1 item"; his view adds "PAED-002: 1 of 3 this semester." and Open My
  progress. No mail (39 to 39).
Gap: none

### Step 5.26 — Dr du Plessis sees it counted
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Open My progress, then PAED-002 from its index, and read its count for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX: its row reads "n of 3 this semester", and its page
  the same count, with "Last encounter `D−1`" and "Training year 2: level 3b, the minimum each encounter is judged
  against and your STAR's target." Record the count as n.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): My progress ("Training year 2 · Semester 2, 2026"): PAED-002
  "1 of 3 this semester", "2 more by 2026-11-30". PAED-002's page: Observations "1 of 3 this semester", "2 more by
  2026-11-30.", "At the minimum level when observed: 1 of 1", "Last encounter 2026-10-19", "Semester 1, 2026: 0 of 3,
  3 short", "Training year 2: level 3b, the minimum each encounter is judged against and your STAR's target.";
  Entrustment "No STAR yet."; the trajectory (1 rating, at the minimum). So n = 1.
Gap: none

### Step 5.27 — Prof Mbatha records his withdrawal
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Open Dr du Plessis's profile from Current trainees (Edit) and set the last day in the programme to `D−2`. Press Deactivate and confirm.
Expect: The confirmation names `D−2` as his last day. It says that the Trainee role is removed and he keeps his own
  record, that encounters after that day will count towards nothing, including any already counted, and that neither
  the day nor the profile can be changed back. After confirming:
  - the page says "Trainee profile deactivated. Their last day in the programme is recorded as `D−2`, and the Trainee
    role has been removed.";
  - the summary reads Status Inactive, "Left the programme: `D−2`", and the profile's details read-only, as on Step
    5.17, with no Save profile (T305);
  - on Trainees he is listed under "Completed and closed", with the outcome "Withdrawn `D−2`";
  - in the same save, the credit from the `D−1` Mini-CEX is taken back (T281);
  - the Trainee role is taken away, as a completion takes it (T290), and he is not emailed.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Trainees ("Current trainees", 4 current), Edit Pieter du
  Plessis (/admin/trainees/edit?id=4), last day 2026-10-18 (D-2), Deactivate: "Deactivate this trainee profile?"
  "Deactivate Pieter du Plessis's profile, with 2026-10-18 as their last day in the programme? The Trainee role is
  removed; they keep their own record, read-only. Encounters observed after that day will count towards nothing in
  this programme, including any already counted. The day cannot be changed afterwards, and a deactivated profile
  cannot be made active again." Confirmed: "Trainee profile deactivated. Their last day in the programme is recorded
  as 2026-10-18, and the Trainee role has been removed."; "Status: Inactive", "Left the programme: 2026-10-18";
  subtitle "The record of a programme that has ended."; "This programme has ended, so its record is archived and
  cannot be changed." over 11.1, 2025-01-15 and 2029-01-14 as text, no input or button, no Save profile. Trainees:
  "Current trainees" "3 current" lists Dlamini, Mahlangu and Ndlovu; "Completed and closed" lists "Pieter du Plessis
  ... Withdrawn 2026-10-18" and "Lerato Molefe ... Completed 2026-10-20". SQL: before, his PAED-002 progress (2026 S2)
  read 1 and he held Trainee; after, no progress row (taken back in the same save) and no role (one row, Name null, as
  Molefe's). No mail (39 to 39).
Gap: none

### Step 5.28 — Dr du Plessis's record after he left
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /account/login → /account/login/submit → / → /portfolio/progress → /activities/mine
Do: Sign in, then read Home, My progress and My activities.
Expect: Each page records that his programme ended on `D−2`, every date ISO (T355, D1):
  - He holds no role (T290: withdrawal takes Trainee away, as completion does), so the phone bar shows no "Acting as"
    and the menu holds Home, My progress and My data rights. Home shows the former trainee's card, "Your training
    record", pointing to My progress, as Dr Molefe's does at Step 5.21 (T252; that it names no end day is T311).
  - My progress opens with "Your programme ended on `D−2`. This page is your record of it and is read-only…". Its "Your
    programme" card reads Ended `D−2`. PAED-002's period holding `D−2` reads "no target (your programme ended part-way
    through) · n−1 recorded".
  - My activities still lists the `D−1` Mini-CEX as Completed, its Credit now "None": the take-back's replay re-stamped
    its completion (`ProgrammeEndCredit`). The filing is kept, but it counts towards nothing.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signed in: Home at /, no "Acting as", menu Home, My progress
  and My data rights; Home's one card "Your training record": "You hold no role at the moment. Your training record is
  kept, read-only: ..." with "Open My progress →". My progress: "Your programme ended on 2026-10-18. This page is your
  record of it and is read-only: ..."; "Your programme" Started 2025-01-15, Ended 2026-10-18, "2 when your programme
  ended"; PAED-002 "Semester 2, 2026 no target (your programme ended part-way through) · 0 recorded" (n-1 = 0); no
  progress bars; no long date. My activities is not in the menu; typed /activities/mine opens, with no Log an
  activity: "Mini-CEX (Paediatrics) · PAED-002 · 2026-10-19, to David Naidoo, Done, Completed", Credit "None".
  /activities/24 reads "Rated 3b. It counted towards no curriculum requirement.", Credit None. Typed /activities/new:
  "You cannot open this page" (D55).
Gap: none

### Step 5.29 — Neither ended programme can be put before the panel
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews
Do: Press Schedule a review, choose the Paed Annual Review Panel and read the Trainee list. Then Cancel.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu, each with the training year. A graduate and a
  registrar who has withdrawn are not current trainees (T238). Cancel returns to Committee reviews; nothing is
  scheduled.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Committee reviews → Schedule a review → /committee/reviews/new
  → Paed Annual Review Panel: the Trainee list offers only "Anele Dlamini · Training year 3", "Nomsa Mahlangu ·
  Training year 1" and "Sipho Ndlovu · Training year 1" (Molefe and du Plessis are gone). Cancel returned to
  /committee/reviews; still 7 reviews listed. No sideways scroll at 390 px.
Gap: none

## Act 5 outcome state

Replay check (2026-10-20, T373 replay, wombat_scenario_t373): match on every query below. Molefe false, 2026-10-20,
  null; du Plessis false, null, 2026-10-18. Neither holds a role (one row each, Name null). The current trainees are
  dlamini, mahlangu and ndlovu. 15 active STARs, nine at 5 and six at 4, each Order at or above MinimumLevelOrder
  (the query returns the label, Order, and the minimum: 5/6/6 and 4/5/5). Review 7 reads 2, 4, 7, 4, with nothing left
  staged. The exports are portfolio-146f1145d0de.pdf x3, then portfolio-e06d16b86584.pdf x1. After the last day: #24
  only (completed, 2026-10-19, 2026-10-18). His PAED-002 progress has no row (n-1 = 0). The act wrote one mail, the
  graduation email (stub count 38 to 39). Snapshot: scenario-t373-post-act5.

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
  - he holds no role, and sign-in still gives him the trainee record (T290);
  - his `D−1` Mini-CEX is completed and credits nothing.
- **Current KGK trainees:** Dr Dlamini, Dr Mahlangu and Dr Ndlovu.

Substitute the replay's dates for `D` in the comments.

```sql
-- Both programmes have ended: Molefe completed on D, du Plessis withdrawn on D−2.
SELECT u."Email", p."IsActive", p."CompletedOn", p."DeactivatedOn"
FROM "TraineeProfiles" p JOIN "AspNetUsers" u ON u."Id" = p."UserId"
WHERE u."Email" IN ('molefe@kgk.wombat.local', 'duplessis@kgk.wombat.local');
-- molefe: false, D, null · duplessis: false, null, D−2

-- Neither holds a role: withdrawal takes Trainee away, as completion does (T290).
SELECT u."Email", r."Name"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r ON r."Id" = ur."RoleId"
WHERE u."Email" IN ('molefe@kgk.wombat.local', 'duplessis@kgk.wombat.local');
-- molefe: one row, Name null · duplessis: one row, Name null

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
  - Dr Molefe and Dr du Plessis, both with no role, keep Data Rights in their menus.
  - Dr Ndlovu is the account erased there.

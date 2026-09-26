# Act 4 — Month 12: annual review

**Scenario date.** Monday 7 December 2026, the week the year's reviews sit. This date is part of the story only. Every
date a step types is relative to the replay day `D` (README § The clock).

**Who acts.**
- Mr Smit (Coordinator), Dr Mokoena (SpecialityAdmin) and Dr Sithole (SubSpecialityAdmin) read what is due and schedule
  the reviews.
- The panel sits: Dr Zulu (chair), Dr Naidoo, Dr Botha and Dr van Rensburg (external).
- Prof Mbatha (InstitutionalAdmin) oversees. She schedules nothing here, and she never ratifies or hears an appeal:
  those belong to the panel (D46).
- Four registrars read their outcomes, and Dr Mahlangu appeals.

**Why.** The semester-2 sitting is the year's formal gate. It decides each registrar's progression for the year and
closes the year's entrustment decisions. Every EPA that closes at the sitting is either decided by a STAR or deferred
with a reason the registrar can read (T131); an EPA decided as opportunity allows may be left undecided. A STAR is a
Statement of Awarded Responsibility, the committee's formal entrustment of a registrar with an EPA at a level. It is
issued only when the panel's chair ratifies a decision taken by a quorum (T165).

**Starting state.** This is what Acts 1–3 leave. Where this act assumes a detail of Act 3, Act 3's outcome state is the
authority.
- **The catalogue.** KGK has adopted curriculum 11.1. Its 15 items carry Annexure B's cadence:
  - each semester for PAED-001, 002, 004, 005, 010 and 012;
  - once a year for the other nine;
  - as opportunity allows for PAED-008, 009 and 013;
  - the neonatal CCC for PAED-004 and 005. KGK has no neonatal panel, so these fall back to the general panel.
- **The panel.** `Paed Annual Review Panel` is Speciality-scoped to Paediatrics and is a General panel. Dr Zulu chairs
  it, Dr Naidoo and Dr Botha are members, and Dr van Rensburg is its external member. All four are active
  CommitteeMembers at KGK, and Zulu, Naidoo and Botha are also Assessors.
- **The registrars.** All five are admitted to curriculum 11.1 in the Paediatrics sub-speciality, with programme starts
  on 15 January 2023 (Molefe), 2024 (Dlamini), 2025 (du Plessis) and 2026 (Mahlangu, Ndlovu). On a replay in 2026 after
  14 January, they are in training years 4, 3, 2, 1 and 1.
- **Act 3's evidence** (its "Records this act creates" table). Every encounter is dated `D−20` to `D−1`, and the MSF
  records `D`, apart from one teaching session of du Plessis's dated `J−1y−56d` (20 November 2024 on a replay in 2026).
  - **Molefe:** six completed WBAs: PAED-001 three times (Dr Zulu's Mini-CEX at 5, Dr Naidoo's CBD at 5, Dr Patel's DOPS
    at 4), PAED-012 twice (Dr Botha at 5, Dr Khumalo at 5) and PAED-010 once (Dr Zulu's Direct Observation at 5). Also
    the MSF campaign Smit ran and released, whose records are on PAED-010 and PAED-012.
  - **Dlamini:** four completed WBAs: PAED-001 three times (Dr Naidoo's Mini-CEX at 4, Dr Zulu's CBD at 3b, Dr Botha's
    CCA at 4) and PAED-004 once (Dr Khumalo's Mini-CEX at 4). Nothing on PAED-002.
  - **du Plessis:** three teaching-session logs (`kgk_teaching_log`), a cancelled DOPS, a CBD still Requested and a
    portfolio review still Awaiting review. Nothing completed.
  - **Mahlangu:** a DOPS on PAED-002 (Dr Patel, 3a) and a Mini-CEX on PAED-004 that Dr Zulu completed at 3a after an
    eight-day stall.
  - **Ndlovu:** a Mini-CEX on PAED-002 that Dr Khumalo declined, the same encounter filed again and completed by Dr
    Botha at 3a, and a reflective exercise on PAED-001, discussed.
- **What does not exist yet.** No committee review, no STAR and no appeal.

**Goal.**
1. All five reviews are scheduled for the period, each with its agenda, and each is started, staged or deferred, decided
   by a quorum and ratified.
2. Molefe holds three STARs and Dlamini two. Mokoena then revokes one of Dlamini's, so the decisions-due page asks for
   it to be decided again.
3. Mahlangu appeals, and the chair remits the decision with a quorate replacement.
4. A formative check-in is held for Ndlovu and closed.
5. Every page the committee's work passes through is read by each role that uses it.

**The clock in this act.** `<Y>` is `D`'s year, and `<P>` is the semester holding `D`.
- **This file is written for a replay in semester 2** (1 July to 31 December), where `<P>` is `<Y> S2`, for example
  "2026 S2". Each agenda then holds 15 lines: the six semester EPAs and the six annual ones close, and PAED-008, 009 and
  013 are optional.
- **On a replay in semester 1**, `<P>` is `<Y> S1` and:
  - only the six semester EPAs close; the annual lines read "Due by year end", and decisions due reads them the same
    way;
  - the evidence window fills as 1 January to 30 June;
  - the Review type select also offers Entrustment-only review (keep Annual progression review);
  - each count of closing or deferred lines below falls to match.
- **The evidence window must hold Act 3's encounters.** Played in semester 2, as Act 3 is, the filled window (1 January
  to 31 December) holds all of them but du Plessis's `J−1y−56d` session, which predates his programme and stays out.

## Phase 4.A — What is due before anything is scheduled

### Step 4.1 — Mr Smit reads the decisions due
Role: Coordinator — Mr Pieter Smit
Route: / → /committee/decisions-due
Do: Open Decisions Due from the nav. Leave Period on `<P>` and Status on "Outstanding: not yet decided". Page through
  the list, then filter Status to "Not scheduled" and EPA to PAED-004.
Expect: The opening sentence names `<P>`, "5 trainees you oversee at Kgosi Kgari Teaching Hospital", and what Missed
  means. By EPA lists all 15 EPAs, each Due 5.
  - Twelve EPAs read To schedule 5.
  - PAED-008, 009 and 013 read Optional 5.
  The count reads "75 of 75 decisions due in `<P>` shown.", and the list pages 20 rows at a time.
  - Each semester EPA's row reads Not scheduled, window `<P>`, "No open review holds it, and `<P>` has not ended.".
  - An annual EPA's row reads the same with window `<Y>`.
  - The three optional EPAs read "As opportunity allows".
  Every Not scheduled row offers Schedule, named by trainee and EPA. The filters leave five PAED-004 rows, one per
  registrar; the By EPA card does not change. No Missed row exists.
Actual (2026-09-26, T295 replay, wombat_scenario): Sentence names 2026 S2, "the 5 trainees you oversee at Kgosi Kgari
  Teaching Hospital" and Missed. By EPA: Due 5 ×15; To schedule 5 ×12, Optional 5 for 008/009/013. "75 of 75 decisions
  due in 2026 S2 shown.", 20 a page. Rows Not scheduled, window 2026 S2 (semester) or 2026 (annual), "No open review
  holds it, and … has not ended."; optional "As opportunity allows". Schedule named "Schedule a review of <trainee> for
  <EPA> before Paed Annual Review Panel". Not scheduled + PAED-004: 5 rows; By EPA unchanged. No Missed row.
Gap: [F-4.1a, T280] The pager's page-size select has no accessible name: "Per page:" is a plain span, not a label
  (PagerControls.razor), so a screen reader announces an unnamed combobox. Seen here; it is the shared pager, so every
  paged list has it.

### Step 4.2 — Prof Mbatha reads the same period
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /committee/decisions-due
Do: Open Decisions Due from the nav. Look for an Institution filter, then switch Period to the previous semester and
  back to `<P>`.
Expect: No Institution filter is offered, because she reads her own institution. The same five registrars and 75 rows
  as Step 4.1. For the previous semester, `<Y> S1`, the statuses follow that period's windows:
  - every registrar's six semester EPAs read Missed ("`<Y> S1` has ended with nothing decided, deferred or on an open
    review's agenda."). Mahlangu and Ndlovu started on 15 January, inside the semester's first month, so their windows
    count too (D42);
  - the annual EPAs read "Due by year end";
  - PAED-008, 009 and 013 read "As opportunity allows", since an optional EPA is never missed.
Actual (2026-09-26, T295 replay, wombat_scenario): Filters are Period, Status and EPA only; no Institution filter. 2026
  S2: the same 5 registrars, "75 of 75". 2026 S1: By EPA reads Missed 5 on PAED-001/002/004/005/010/012 (so Mahlangu's
  and Ndlovu's count), rows "2026 S1 has ended with nothing decided, deferred or on an open review's agenda."; the nine
  annual rows "Due by year end" ("…due by the last sitting of 2026…"), 008/009/013 "As opportunity allows". Back on 2026
  S2: 75 of 75. Entrustment decisions (dashboard quick link): "No entrustment decisions".
Gap: none

### Step 4.3 — Dr Mokoena reads what is due in her speciality
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/decisions-due
Do: Sign in, read her nav, and open Decisions Due.
Expect:
  - **Nav:** Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue and Decisions Due.
  - **The page:** the same five registrars, all in Paediatrics, and 75 rows, each Schedule link offered.
  - **Not in her nav or on her dashboard:** Entrustment Decisions.
Actual (2026-09-26, T295 replay, wombat_scenario): Nav: Home, My Account, Data Rights, Programme Trainees, Decision
  Panels, Committee Reviews, STAR Review Queue, Decisions Due, Logout. Dashboard (Viewing as SpecialityAdmin): Pending
  reviews, Trainees in programme 5, Curriculum coverage; no Entrustment Decisions in nav or dashboard. Decisions Due:
  "the 5 trainees you oversee…", 75 of 75, every row with Schedule.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc297b4, the end-of-Act-4 snapshot): the dashboard's Pending
  reviews reads 2 "activities awaiting review", with no link: Dr du Plessis's requested CBD (activity 9) and his
  portfolio review awaiting review (10), the two open reviews SQL finds. Trainees in programme 5 "active / 0 inactive"
  and Curriculum coverage as before; no Entrustment Decisions.
Gap: none

### Step 4.4 — Dr Sithole reads what is due in his sub-speciality
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/decisions-due
Do: Sign in and open Decisions Due.
Expect: The same five registrars and 75 rows as Step 4.3: all five train in the Paediatrics sub-speciality he
  administers. His nav reads as Mokoena's.
Actual (2026-09-26, T295 replay, wombat_scenario): Nav identical to Mokoena's (Programme Trainees, Decision Panels,
  Committee Reviews, STAR Review Queue, Decisions Due). Decisions Due: "the 5 trainees you oversee at Kgosi Kgari
  Teaching Hospital", "75 of 75 decisions due in 2026 S2 shown.", Schedule on every row.
Gap: none

### Step 4.5 — Dr Naidoo is not offered what is due
Role: CommitteeMember — Dr David Naidoo
Route: /committee/decisions-due → /access-denied
Do: Look for Decisions Due in the nav, then type its address.
Expect: His nav has Decision Panels and Committee Reviews but no Decisions Due. The typed address lands on the
  access-denied page: the page is for the roles that schedule reviews.
Actual (2026-09-26, T295 replay, wombat_scenario): Nav: Activity Inbox, Recent Activities, Programme Trainees, Decision
  Panels, Committee Reviews; no Decisions Due. Committee Reviews: "No reviews yet. The reviews of the panels you sit on
  appear here once they are scheduled." Typed /committee/decisions-due → /access-denied?ReturnUrl=… "You do not have
  permission to view this page."
Gap: none

## Phase 4.B — Scheduling the five reviews

Since T182 a review can be scheduled by these roles:
- a Coordinator or InstitutionalAdmin of the panel's institution;
- a SpecialityAdmin or SubSpecialityAdmin, for a trainee in their own programme;
- an Administrator.
The trainee must be current and must train where the panel sits. A CommitteeMember schedules nothing, and neither does
anyone who also holds Trainee (T216).

### Step 4.6 — Mr Smit schedules Dr Molefe's review
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews from the nav and choose Schedule review. Fill in:
  - Panel: `Paed Annual Review Panel`.
  - Trainee: Dr Molefe.
  - Period: `<P>`, and leave the evidence window as it fills.
  - Scheduled on: `D`.
  - Review type: Annual progression review.
  - Formative only: unticked.
  Read the agenda preview, then choose Create review.
Expect:
  - **Panel:** the one panel is offered. The Trainee select, empty until a panel is chosen, then lists the five
    registrars.
  - **Period:** labelled "`<P>` · 1 Jul to 31 Dec `<Y>`". Choosing it fills the window as `<Y>`-01-01 to `<Y>`-12-31.
  - **Review type:** only "Annual progression review" and "Pre-graduation review", with the help "The semester-2 sitting
    decides the trainee's progression for the year." (T131 slice 5).
  - **The preview** reads "15 EPAs will be on the agenda for `<P>`." It then says that PAED-001, 002, 003, 004, 005,
    006, 007, 010, 011, 012, 014 and 015 "must be decided at this sitting, or deferred with a reason, before it is
    ratified". It lists PAED-008, 009 and 013 as "As opportunity allows".
  - **After Create review:** the page opens the new review. Its Review card reads:
    - Trainee Lerato Molefe;
    - Panel Paed Annual Review Panel;
    - Sits for `<P>`;
    - Evidence window `<Y>`-01-01 to `<Y>`-12-31;
    - Type Annual progression review;
    - Mode Summative;
    - State Scheduled.
  - **Start review** is offered to Smit, since the coordinators of the panel's institution may start a review. He leaves
    it for the chair.
  - **Evidence snapshot:** "No evidence has been frozen yet. Starting the review captures the current bundle."
Actual (2026-09-26, T295 replay, wombat_scenario): Played by the 4.E–4.I player after the 4.A–4.D player stopped (lock
  state reviews-schedule--preview-failed not captured). One panel offered; Trainee disabled until it was chosen, then
  the five registrars. Period "2026 S2 · 1 Jul to 31 Dec 2026", window 2026-01-01 to 2026-12-31; Review type Annual
  progression / Pre-graduation with the semester-2 help. Preview "15 EPAs will be on the agenda for 2026 S2.", the
  twelve closing EPAs named, 008/009/013 As opportunity allows. Created review #1: Lerato Molefe, Paed Annual Review
  Panel, Sits for 2026 S2, window 2026-01-01 to 2026-12-31, Annual progression review, Summative, Scheduled; Start
  review offered to Smit; "No evidence has been frozen yet. Starting the review captures the current bundle."
Gap: none

### Step 4.7 — Mr Smit schedules Dr Dlamini's and Dr du Plessis's reviews
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule the same kind of review for Dr Dlamini, then for Dr du Plessis: same panel, `<P>`, window as filled,
  Scheduled on `D`, Annual progression review.
Expect: Each opens as Scheduled with a 15-EPA preview like Step 4.6's. Committee Reviews lists three rows for `<P>`.
  Each row reads:
  - Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31";
  - Type Annual progression;
  - Mode Summative;
  - State Scheduled;
  - Decision Pending.
  Each row's Open is named by the review it opens.
Actual (2026-09-26, T295 replay, wombat_scenario): Reviews #2 (Anele Dlamini) and #3 (Pieter du Plessis) created, each
  opening Scheduled with the same 15-EPA preview (Period 2026-2, Scheduled on 2026-09-26, Annual progression review).
  Committee Reviews lists three rows, each "2026 S2 · 2026-01-01 to 2026-12-31", Annual progression, Summative,
  Scheduled, Pending; each Open named "Open the 2026 S2 review of <trainee> before Paed Annual Review Panel".
Gap: none

### Step 4.8 — A second review of Dr Molefe for the same period is refused
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Schedule another Annual progression review of Dr Molefe before the same panel for `<P>`.
Expect: Refused, and nothing is created. The refusal names the open review: "Review #N already puts this trainee before
  Paed Annual Review Panel for `<P>` (scheduled, scheduled `D`). A trainee has one binding review for each period …".
  The wording is a product decision (T131 slice 4). The list still holds three rows.
Actual (2026-09-26, T295 replay, wombat_scenario): The preview still read "15 EPAs will be on the agenda for 2026 S2."
  with no warning; Create review was refused in an alert: "Review #1 already puts this trainee before Paed Annual Review
  Panel for 2026 S2 (scheduled, scheduled 2026-09-26). A trainee has one binding review for each period before the
  panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another."
  Stayed on /committee/reviews; still three rows.
Gap: [F-4.8a, T309] The agenda preview gives no sign that the trainee already holds this period's binding seat: it reads "15
  EPAs will be on the agenda" and the refusal comes only on Create review. Unsure: no decision requires the preview to
  warn; it is a usability point.

### Step 4.9 — Dr Mokoena schedules Dr Mahlangu's review from what is due
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions Due, choose Schedule on Dr Mahlangu's PAED-001 row. Check the form it opens, set Scheduled on `D`, and
  create the review.
Expect:
  - **Decisions Due before she schedules:** Molefe's, Dlamini's and du Plessis's rows now read Scheduled, "On the agenda
    of review #N, which is still open.", each with Open review.
  - **The link opens the scheduling form already filled:** Panel `Paed Annual Review Panel`, Trainee Dr Mahlangu and
    Period `<P>`. The preview shows 15 EPAs. The query string fills only what the form offers her, so the link
    authorises nothing.
  - **The review opens as Scheduled,** and Start review is not offered to her. The Review card says: "Only the
    coordinators of the panel's institution, and those of its members who are active committee members there, can start
    this review." (T194)
Actual (2026-09-26, T295 replay, wombat_scenario): Before scheduling (EPA filter PAED-001): Molefe, Dlamini and du
  Plessis read Scheduled, "On the agenda of review #1/#2/#3, which is still open.", each with Open review; Mahlangu and
  Ndlovu Not scheduled with Schedule. By EPA: Scheduled 3, To schedule 2 (Optional 2 on 008/009/013). The Schedule link
  (/committee/reviews?panel=1&trainee=…&period=2026-2) opened the form filled: Paed Annual Review Panel, Nomsa Mahlangu,
  2026 S2, window 2026-01-01 to 2026-12-31, Scheduled on 2026-09-26; preview 15 EPAs. Created review #4, Scheduled; no
  Start review; the card reads "Only the coordinators of the panel's institution, and those of its members who are
  active committee members there, can start this review."
Gap: none

### Step 4.10 — Dr Sithole schedules Dr Ndlovu's review from what is due
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions Due, choose Schedule on one of Dr Ndlovu's rows. Set Scheduled on `D` and create the review.
Expect: As Step 4.9: the form is filled, the review opens as Scheduled, and Start is not offered. Decisions Due then
  reads Scheduled on all 75 rows, and By EPA reads Scheduled 5 and To schedule 0 for every EPA.
Actual (2026-09-26, T295 replay, wombat_scenario): From Ndlovu's PAED-002 Schedule link the form opened filled (Paed
  Annual Review Panel, Sipho Ndlovu, 2026 S2, Scheduled on 2026-09-26, Annual progression); created review #5,
  Scheduled, no Start review, the same "Only the coordinators …" line. Decisions Due: "75 of 75 decisions due in 2026 S2
  shown.", all 75 rows (4 pages) Scheduled; By EPA Scheduled 5, To schedule 0 on every EPA (Optional 0 on 008/009/013
  now they are on agendas).
Gap: none

### Step 4.11 — Dr Naidoo lists the panel's reviews
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews, then open Dr Dlamini's review.
Expect:
  - **The list:** its subtitle reads "Open existing committee reviews.", and no Schedule review button is offered. It
    lists the five reviews of the panel he sits on, all Scheduled, with Decision Pending.
  - **Dlamini's review:** Start review is offered to him, as a member who may sit on the panel. He leaves it.
Actual (2026-09-26, T295 replay, wombat_scenario): Committee Reviews subtitle "Open existing committee reviews.", no
  Schedule review button; five rows (Ndlovu, Mahlangu, du Plessis, Dlamini, Molefe), each Annual progression, Summative,
  Scheduled, Pending. Dlamini's review #2 offers him Start review; left unstarted.
Gap: none

### Step 4.12 — Prof Mbatha checks the schedule
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/decisions-due
Do: Read Committee Reviews, then Decisions Due.
Expect: Committee Reviews offers Schedule review and lists all five reviews. Decisions Due reads Scheduled on all 75
  rows. Each row's "Open review" opens the review holding it, and no row offers Schedule.
Actual (2026-09-26, T295 replay, wombat_scenario): Committee Reviews offers Schedule review and lists the five reviews.
  Decisions Due "75 of 75 decisions due in 2026 S2 shown."; all 75 rows Scheduled, each with Open review whose name
  ("Open review #N for <trainee>, <EPA>") matches the review it opens (checked all 75); no row offers Schedule.
Gap: none

### Step 4.13 — The panel cannot change what it decides while its reviews are open
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels → /committee/panels/{PanelId:int}
Do: Open the panel. In Decides for, choose the Neonatal team Clinical Competency Committee and save the committee.
Expect: Refused, and the panel stays a General panel. The refusal names the first open review: "Review #N (`<P>`,
  scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the
  panel sat as when it was scheduled …" (T131 slice 5). The select's help text says the same rule before she tries.
Note: If the save is accepted, set Decides for back to General panel before going on, and record the defect.
Actual (2026-09-26, T295 replay, wombat_scenario): Reached from Decision Panels' Edit. The Decides for help already says
  "It cannot change while a review before the panel is scheduled, in progress or awaiting ratification." Save committee
  with Neonatal team CCC refused: "Review #1 (2026 S2, scheduled) still sits before this panel. What a review decides,
  and its agenda, follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits
  as until its open reviews are ratified, or closed if formative." DB: DecisionBodyKey still null (General); the select
  keeps the unsaved choice until reload.
Gap: none

## Phase 4.C — Before the sitting

### Step 4.14 — Dr Botha checks who sits and who decides
Role: CommitteeMember — Dr Sarah Botha
Route: /committee/panels
Do: Open Decision Panels.
Expect:
  - **The list:** `Paed Annual Review Panel` with Scope Speciality, Decides for General panel and Members 4. There is no
    New panel button and no Edit column: she manages no panel (T239, T256).
  - **Who decides each EPA:** curriculum 11.1's EPAs are decided by `Paed Annual Review Panel`. PAED-004 and 005 fall
    back to it, because no panel covering the programme sits as the neonatal CCC.
Actual (2026-09-26, T295 replay, wombat_scenario): Played out of order, before 4.6-4.13 (see the handoff); it reads
  nothing those steps change. One row: Paed Annual Review Panel, Speciality, General panel, Members 4; no New panel, no
  Edit column, no controls in main. Who decides: PAED-004, PAED-005 → Paed Annual Review Panel, "No panel covering this
  programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides."; the other 13 → the
  same panel.
Gap: none

### Step 4.15 — Dr Zulu reads Dr Molefe's review before starting it
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews and Dr Molefe's review. Read every card before starting it.
Expect:
  - **Start review** is offered to her.
  - **Decision card:** "No decision has been recorded yet."
  - **Entrustment against Annexure A** reads training year 4 on `D`, and "0 at or above · 0 below · 15 with no decision,
    of 15 EPAs". The exit rule reads "0 of 15 EPAs at their exit level by STAR decision", names all fifteen as "Not
    yet", and says it is for information only (T166).
  - **Multi-source feedback by EPA** marks Covered, in the semester Act 3's campaign closed in, each EPA that campaign
    covered. The other cells read "None released" or "None released yet" (T168).
  - **Agenda** lists the 15 EPAs:
    - window `<P>` for the six semester EPAs and `<Y>` for the nine annual ones;
    - twelve Due, each with "Must be decided at this sitting, or deferred with a reason.";
    - three As opportunity allows;
    - Evidence 0 items each;
    - no Action column, because the review has not started.
  - **Evidence snapshot:** "No evidence has been frozen yet …"
  - **Rating trajectory by EPA** charts PAED-001, PAED-010 and PAED-012 on the ladder's rungs.
  - **Sampling concentration warnings,** above the cards, name PAED-010 (one rating) and PAED-012 (two), each with
    "Fewer than three distinct assessors across this EPA's evidence." PAED-001, rated by three assessors on three
    instruments, is not named. The card says her two MSF records are not counted, because no named assessor rated
    them (T135, D44).
  - **Pending entrustment decisions:** "No pending entrustment decisions have been staged for this review."
  - **Appeals:** "No appeal has been lodged against this review."
Actual (2026-09-26, T295 replay, wombat_scenario): Review #1: Start review offered; Decision "No decision has been
  recorded yet."; standing "In training year 4 on 26 September 2026", "0 at or above · 0 below · 15 with no decision, of
  15 EPAs", exit rule "0 of 15 EPAs at their exit level by STAR decision (level 5: 0 of 9 · level 4: 0 of 6)", all
  fifteen Not yet, "For information only." MSF: Semester 2 PAED-010 and 012 Covered (Default MSF #1, closed 26 Sept
  2026), other cells None released / None released yet. Agenda caption "15 EPAs for 2026 S2, 12 still to stage or
  defer", 4 columns (no Action), windows 2026 S2 ×6 and 2026 ×9, twelve Due with the must-decide line, three As
  opportunity allows, 0 items each. Snapshot not frozen; trajectory charts PAED-001/010/012; sampling warnings name
  PAED-010 (1 rating) and PAED-012 (2), not PAED-001, and "Not counted: 2 records … (multi-source feedback, for one)".
  No pending decisions; no appeal.
Gap: none

## Phase 4.D — Dr Molefe's sitting

### Step 4.16 — Dr Zulu starts the review
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the review.
Expect: "Review started." The state reads In progress.
  - **Evidence snapshot:** frozen "when the review started", grouped by EPA and then by instrument. Every state is
    listed, and each line links to its activity and shows its rating, encounter date and state. The MSF campaign's
    report sits under "Not about a single EPA", and its per-EPA records are listed under each EPA.
  - **Agenda:** its caption reads "15 EPAs for `<P>`, 12 still to stage or defer". Each line counts its snapshot items
    and offers Stage and Defer.
  - **Decision card:** a form with these fields:
    - Category, opening on "Select a category…";
    - Rationale;
    - Conditions;
    - Present, listing Thandi Zulu (chair) ticked and locked, David Naidoo, Sarah Botha and John van Rensburg
      (external).
  - **Record decision** is disabled. The line under it reads "Record decision: PAED-001, PAED-002, … and PAED-015 must
    be decided at this sitting. Stage a decision on each, or defer it with a reason." It names the twelve closing EPAs.
  - **The staging form** appears under Pending entrustment decisions.
Actual (2026-09-26, T295 replay, wombat_scenario): "Review started."; State In progress. Snapshot "Frozen when the
  review started, grouped by EPA and then by instrument. Every state is listed.": PAED-001 3 records (CBD #13 5, DOPS
  #14 4, Mini-CEX #12 5), PAED-010 2 (Direct Observation #17 5, MSF #22 Not recorded), PAED-012 3 (CBD #16, Mini-CEX
  #15, MSF #23), each linking /activities/<id>; Default MSF #1 (Released, responses 6) under "Not about a single EPA".
  Agenda caption "15 EPAs for 2026 S2, 12 still to stage or defer", Action column with Stage and Defer, PAED-001 3
  items. Decision form: Category "Select a category…", Rationale, Conditions, Present (Thandi Zulu (chair) ticked and
  disabled, Naidoo, Botha, John van Rensburg (external)). Record decision disabled, "Record decision: PAED-001, …
  PAED-014 and PAED-015 must be decided at this sitting. Stage a decision on each, or defer it with a reason." Staging
  form under Pending.
Gap: none

### Step 4.17 — Dr Zulu stages PAED-001 at level 5
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int} → /activities/{ActivityId:int} → /committee/reviews/{ReviewId:int}
Do: Open Dr Zulu's own PAED-001 Mini-CEX from the snapshot to re-read it, then come back to the review. Choose Stage on
  PAED-001's agenda line and fill in:
  - Authorised level `5`;
  - Issued on `D`, as filled;
  - no expiry;
  - the rationale "Leads resuscitation and emergency care without prompting; ready for unsupervised practice."
  Tick the Mini-CEX, then Dr Naidoo's CBD, and stage the pending decision.
Expect:
  - **The snapshot line** opens the Mini-CEX, Completed and read-only, rated 5.
  - **Choosing Stage** puts PAED-001 in the EPA select and moves the focus there.
  - **The level select** offers the rungs 1, 2, 3a, 3b, 4 and 5 under "CPSA Paediatric Entrustment Scale v11.1", with
    the help "Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on." (T076, T167)
  - **The evidence picker** lists PAED-001's three lines first. Stage pending decision stays disabled with "Name at
    least one item of the evidence snapshot." until a line is ticked (D38).
  - **With one line ticked,** the picker says "Only one item is named. The College's rule is that an entrustment
    decision is never taken from a single form." It still allows the stage.
  - **After staging:** "Pending entrustment decision staged." PAED-001's line reads Staged, "A decision is staged
    below.", and its Action cell reads "Staged below". The caption counts 11 still to stage or defer. The pending list
    shows PAED-001 at `5`, issued `D`, and "Rests on 2 items of the snapshot: …" naming both.
Actual (2026-09-26, T295 replay, wombat_scenario): Snapshot link opened /activities/12: Mini-CEX, State Completed,
  nothing editable, EPA PAED-001, assessor Thandi Zulu, supervision 5. Back on the review, PAED-001's Stage (named
  "Stage a decision on PAED-001") set the EPA select and moved focus to it (#pending-epa). Level select: 1, 2, 3a, 3b,
  4, 5 in optgroup "CPSA Paediatric Entrustment Scale v11.1", help "Rungs of CPSA Paediatric Entrustment Scale v11.1,
  the ladder this EPA is assessed on." Issued on filled 2026-09-26. Picker "PAED-001's lines are listed first."; Stage
  disabled with "Name at least one item of the evidence snapshot." until a tick; one tick showed "Only one item is
  named. The College's rule is …" and allowed staging. Staged with #12 and #13: "Pending entrustment decision staged.";
  PAED-001 Staged, "A decision is staged below.", Action "Staged below"; caption 11 still to stage or defer; pending
  "PAED-001 … 5 · issued 2026-09-26", "Rests on 2 items of the snapshot: Mini-CEX (Paediatrics) #12; Case-Based
  Discussion (Paediatrics) #13."
Gap: none

### Step 4.18 — An expiry before the issue date is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start staging PAED-010: choose its Stage, then level `4`, Issued on `D`, Expires on `D−1`, the rationale "Leads the
  ward round safely; one observation so far, to be confirmed at her final review." and both PAED-010 lines (the Direct
  Observation and the MSF record). Stage it.
Expect: Refused with "An expiry date must be after the issue date." Nothing is staged, the form keeps what was typed,
  and PAED-010 still reads Due.
Actual (2026-09-26, T295 replay, wombat_scenario): PAED-010's Stage set the EPA; level 4, Issued 2026-09-26, Expires
  2026-09-25, the rationale, #17 and MSF #22 ticked. Stage refused: "An expiry date must be after the issue date." The
  form kept every value; PAED-010 still Due with Stage/Defer; caption still 11.
Gap: none

### Step 4.19 — Dr Zulu stages PAED-010 with an expiry, and PAED-012
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Correct PAED-010's Expires on to `D+20` and stage it. Then stage PAED-012 at `5` with no expiry, the rationale
  "Breaks difficult news clearly and kindly; consistent across settings." and its two WBA lines. Remove the PAED-012
  decision, then stage it again the same way.
Expect: Both are staged. The pending list shows:
  - PAED-010 at `4`, "issued `D` · expires `D+20`";
  - PAED-012 at `5`.
  The caption counts 9 still to stage or defer.
  - **Remove** is named "Remove the staged decision on PAED-012". It answers "Pending entrustment decision removed.",
    PAED-012's line reads Due again, with Stage and Defer, and the caption counts 10.
  - **Staged again,** it reads Staged, and the caption counts 9.
Actual (2026-09-26, T295 replay, wombat_scenario): Expires corrected to 2026-10-16 (D+20): "Pending entrustment decision
  staged.", caption 10. PAED-012 at 5 (no expiry; the form had reset), #16 and #15: staged, caption 9. Pending list:
  PAED-001 "5 · issued 2026-09-26", PAED-010 "4 · issued 2026-09-26 · expires 2026-10-16", PAED-012 "5 · issued
  2026-09-26". Remove named "Remove the staged decision on PAED-012" (no confirmation dialog): "Pending entrustment
  decision removed.", PAED-012 Due with Stage/Defer, caption 10. Staged again: Staged, caption 9.
Gap: none

### Step 4.20 — A second decision on PAED-001 is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: In the staging form, choose PAED-001 from the EPA select, then fill in level `4`, a rationale and one line. Stage
  it.
Expect: Refused with "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage
  another." The pending list still holds exactly one PAED-001 decision, at `5`.
Actual (2026-09-26, T295 replay, wombat_scenario): EPA select PAED-001, level 4, a rationale and DOPS #14: refused with
  "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage another." The pending list
  still holds exactly one PAED-001 decision, at 5.
Gap: none

### Step 4.21 — Dr Zulu defers the other nine closing lines
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Defer on PAED-002 and submit an empty reason. Then give the reason "To be decided at her final review." and
  defer it. Reinstate PAED-002, then defer it again with the same reason. Defer the other eight lines still marked Due
  the same way.
Expect:
  - **The deferral form:** a group titled "Defer PAED-002" opens under the table with the focus in its reason box. Its
    help reads "Kept on the agenda line. The trainee sees it on their reviews page once the review is ratified."
  - **An empty reason** is refused in the form with "Say why the committee is deferring the decision." (T212).
  - **Deferring:** "PAED-002 deferred." The line reads Deferred with "Reason: To be decided at her final review." and
    offers Reinstate.
  - **Reinstating:** "PAED-002 reinstated: it is due at this sitting again." Stage and Defer return.
  - **After the last deferral:** the caption reads "15 EPAs for `<P>`" with nothing left to stage or defer. PAED-008,
    009 and 013 still read As opportunity allows. Record decision is enabled, beside "Recording fixes the entrustment
    decisions staged below and the agenda's deferrals: they cannot be changed afterwards."
Actual (2026-09-26, T295 replay, wombat_scenario): PAED-002's Defer (named "Defer PAED-002") opened a group "Defer
  PAED-002" under the table, focus in #deferral-reason, help "Kept on the agenda line. The trainee sees it on their
  reviews page once the review is ratified." Empty reason: "Say why the committee is deferring the decision." in the
  form. Deferred: "PAED-002 deferred.", Deferred, "Reason: To be decided at her final review.", Reinstate. Reinstate:
  "PAED-002 reinstated: it is due at this sitting again.", Stage and Defer back. PAED-002 to 007, 011, 014, 015
  deferred. Caption "15 EPAs for 2026 S2"; 008/009/013 As opportunity allows; Record decision enabled beside "Recording
  fixes the entrustment decisions staged below and the agenda's deferrals: they cannot be changed afterwards."
Gap: none

### Step 4.22 — Dr Naidoo reads the sitting without the chair's controls
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's review while it is in progress.
Expect: He sees everything the chair sees:
  - the snapshot;
  - the agenda with its Staged and Deferred badges and reasons;
  - the three staged decisions and their evidence;
  - the standing, MSF coverage, trajectory and sampling cards.
  He is offered none of the chair's controls (T213):
  - the agenda has four columns, with no Action column;
  - there is no staging form, no Remove and no decision form.
  The Review card says "Only the panel's chair, Thandi Zulu, can stage entrustment decisions, defer agenda lines and
  record the committee's decision." The Decision card says "No decision has been recorded yet."
Actual (2026-09-26, T295 replay, wombat_scenario): Naidoo sees the sampling, standing, MSF, snapshot, trajectory and
  appeals cards, the agenda with its Staged/Deferred badges and reasons, and the three staged decisions with "Rests on 2
  items …". Agenda columns EPA/Window/State/Evidence (no Action); no staging form, no Remove, no decision form (the only
  buttons are Sign out and the warnings' ×). Review card: "Only the panel's chair, Thandi Zulu, can stage entrustment
  decisions, defer agenda lines and record the committee's decision." Decision card: "No decision has been recorded
  yet."
Gap: none

### Step 4.23 — Recording without a second member present is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose the category Satisfactory Progress and write a rationale. Tick nobody besides herself under Present, and
  record the decision.
Expect: Refused with "A committee decision needs at least two panel members present: the chair and at least one
  other." (T165, D46). The review stays In progress, with nothing recorded.
Actual (2026-09-26, T295 replay, wombat_scenario): Satisfactory Progress, the rationale, only Thandi Zulu (chair)
  present: Record decision refused with "A committee decision needs at least two panel members present: the chair and at
  least one other." State stays In progress. Audit: RecordCommitteeDecisionCommand FAILED (PresentUserIds).
Gap: none

### Step 4.24 — Recording without a category is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Tick David Naidoo, Sarah Botha and John van Rensburg as present. Set Category back to "Select a category…" and
  record.
Expect: Refused with "This review decides the trainee's progression, so its decision records a progression category.
  Choose one." (T131 slice 5). Nothing is recorded.
Actual (2026-09-26, T295 replay, wombat_scenario): Naidoo, Botha and van Rensburg ticked, Category back to "Select a
  category…" (Record decision stays enabled): refused with "This review decides the trainee's progression, so its
  decision records a progression category. Choose one." Nothing recorded; State In progress.
Gap: none

### Step 4.25 — Dr Zulu records Dr Molefe's decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress, with the rationale "On track for completion; three EPAs entrusted, the rest to be
  decided at her final review." Keep all four present, and record the decision.
Expect: "Decision recorded." The state reads Decided.
  - **Decision card:** headed "Satisfactory Progress", with the rationale and a "Present:" line. It opens with "Thandi
    Zulu (chair)", ends with "John van Rensburg (external)", and names David Naidoo and Sarah Botha between them.
  - **Fixed:** the pending card now says the decisions were "Fixed when the committee's decision was recorded: ratifying
    issues exactly these." No Remove is offered.
  - **Agenda:** no Action column.
  - **Ratify** is offered, enabled.
  - **No chair-rated warning:** five assessors rated her lines, so "The chair rated all of this evidence" is not shown
    (D46 warns of that case and refuses nothing).
Actual (2026-09-26, T295 replay, wombat_scenario): "Decision recorded."; State Decided. Decision card "Satisfactory
  Progress", the rationale, "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)".
  Pending card "Fixed when the committee's decision was recorded: ratifying issues exactly these. Only a decision that
  no longer fits the trainee's curriculum can still be removed.", no Remove. Agenda EPA/Window/State/Evidence (no
  Action). Ratify offered, enabled. No "The chair rated all of this evidence" warning.
Gap: none

### Step 4.26 — Prof Mbatha reads the decided review, and is not offered Ratify
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Between the recording and the ratifying, open Dr Molefe's review from Committee Reviews.
Expect: The list reads Decided, Satisfactory Progress, for her review. The review shows the decision with its
  "Present:" line and the three staged decisions under "Fixed when the committee's decision was recorded …". It offers
  no Ratify, no Remove and no Action column. The Review card says "Only the panel's chair, Thandi Zulu, can ratify the
  committee's decision.": only the chair ratifies, with no administrator's bypass (D46).
Actual (2026-09-26, T295 replay, wombat_scenario): Committee Reviews row: Molefe, Decided, Satisfactory Progress. Review
  #1 shows the decision with "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)",
  the three staged decisions under "Fixed when the committee's decision was recorded …"; no Ratify, no Remove, no Action
  column (buttons: Sign out, ×). Review card: "Only the panel's chair, Thandi Zulu, can ratify the committee's
  decision."
Gap: none

### Step 4.27 — Dr Zulu ratifies, and the STARs are issued
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Ratify the decision.
Expect: "Decision ratified." The state reads Ratified, and Ratify is gone.
  - **Agenda:**
    - PAED-001, 010 and 012 read Decided, each with "STAR #n.";
    - the nine deferred lines keep their reasons;
    - PAED-008, 009 and 013 read Not decided, "The review was ratified without deciding it.".
  - **Pending:** "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one." (T212)
  - **Standing:** "2 at or above · 1 below · 12 with no decision, of 15 EPAs". PAED-001 and 012 at `5` meet year 4's
    target of `5`, and PAED-010 at `4` is below it. The exit rule reads "2 of 15 EPAs at their exit level by STAR
    decision".
  - **What ratifying locks:** nothing on the review can now be staged, deferred, removed or recorded.
Actual (2026-09-26, T295 replay, wombat_scenario): No confirmation dialog; "Decision ratified."; State Ratified, Ratify
  gone, no other control. Agenda: PAED-001 "Decided STAR #1.", PAED-010 "STAR #2.", PAED-012 "STAR #3."; nine Deferred
  with "Reason: To be decided at her final review."; 008/009/013 Not decided, "The review was ratified without deciding
  it." Pending: "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
  one." Standing "2 at or above · 1 below · 12 with no decision, of 15 EPAs"; exit rule "2 of 15 EPAs at their exit
  level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)". No email was sent (mail sink unchanged since Act 3).
Gap: [F-4.27a, T320] Ratifying issues three STARs and tells Dr Molefe nothing: no mail is sent (StarDecisionEmail, a "STAR
  reflection approved/declined" template, has no caller). Unsure: no product decision requires the notification; the
  suspect list raised it.

## Phase 4.E — The other four sittings

### Step 4.28 — Dr Zulu stages two decisions for Dr Dlamini
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Dlamini's review and start it. Stage, each on two finished PAED-001 lines (Dr Naidoo's Mini-CEX and Dr
  Botha's CCA):
  - PAED-001 at `4`, with the rationale "Manages the acutely ill child with the senior available on request.";
  - PAED-002 at `3b`, with the rationale "Manages common presentations with decisions checked by the senior on call."
  Defer the other ten closing lines with the reason "Not at a decision point this year."
Expect:
  - **Start:** "Review started." Her snapshot freezes her four Act 3 WBAs: three under PAED-001 and one under PAED-004.
  - **Staging PAED-002:** she has no PAED-002 line, so the picker says "None of the named items is about PAED-002." It
    still stages, since D38 lets any line of the snapshot ground a decision.
  - **The agenda:** PAED-001 and 002 read Staged, ten lines Deferred, and PAED-008, 009 and 013 As opportunity allows.
  - **Record decision** becomes enabled once the tenth line is deferred.
Actual (2026-09-26, T295 replay, wombat_scenario): Review #2 started: "Review started."; snapshot freezes her four WBAs:
  PAED-001 CBD #18 3b, CCA #19 4, Mini-CEX #1 4; PAED-004 Mini-CEX #20 4, all Completed (sampling warns on PAED-004
  only). PAED-001 staged at 4 on Mini-CEX #1 and CCA #19. Staging PAED-002 at 3b on the same two lines, the picker said
  "None of the named items is about PAED-002." and still staged ("Pending entrustment decision staged."). Ten lines
  deferred with "Not at a decision point this year."; agenda PAED-001/002 Staged, ten Deferred, 008/009/013 As
  opportunity allows, caption "15 EPAs for 2026 S2". Record decision stayed disabled through the ninth deferral and
  became enabled on the tenth.
Gap: none

### Step 4.29 — Dr Zulu records and ratifies Dr Dlamini's decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress with a rationale, tick David Naidoo and Sarah Botha as present, and record. Then
  ratify.
Expect:
  - **Recorded:** "Decision recorded.", then "Decision ratified.". The state reads Ratified.
  - **Present:** a "Present:" line naming Thandi Zulu (chair), David Naidoo and Sarah Botha.
  - **The agenda:** PAED-001 and 002 read Decided with their STAR numbers, and PAED-008, 009 and 013 Not decided.
  - **Standing:** "1 at or above · 1 below · 13 with no decision, of 15 EPAs". PAED-001 at `4` meets year 3's target of
    `4`, and PAED-002 at `3b` is below it.
Actual (2026-09-26, T295 replay, wombat_scenario): Satisfactory Progress with a rationale, Naidoo and Botha ticked:
  "Decision recorded.", "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha"; then "Decision ratified.", State
  Ratified. Agenda: PAED-001 "Decided STAR #4.", PAED-002 "Decided STAR #5.", 008/009/013 Not decided. Standing "1 at or
  above · 1 below · 13 with no decision, of 15 EPAs": PAED-001 4 against year 3's 4 At or above, PAED-002 3b against 4
  Below.
Gap: none

### Step 4.30 — Dr du Plessis's sitting: progress with an observation, no STAR
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr du Plessis's review and start it. Stage nothing, and defer all twelve closing lines with the reason "Not at
  a decision point this year." Record Satisfactory with Observations:
  - the rationale "Teaching logs are sound; no observed clinical assessment completed yet.";
  - the condition "At least two Mini-CEX before the next sitting.";
  - David Naidoo present.
  Then ratify.
Expect:
  - **The snapshot** holds his two teaching sessions logged this year, his CBD still Requested, his cancelled DOPS and
    his portfolio review still Awaiting review, each with its state (T138). The session logged for `J−1y−56d` is outside
    the window and not in it.
  - **Rating trajectory:** "No rating by a named assessor to chart for this trainee."
  - **Recorded:** the decision reads "Satisfactory with Observations", with "Conditions: At least two Mini-CEX before
    the next sitting." and "Present: Thandi Zulu (chair), David Naidoo".
  - **Ratified:** the state reads Ratified, and the pending card says "Nothing was staged when the review was ratified,
    so ratifying it issued no STAR."
Actual (2026-09-26, T295 replay, wombat_scenario): Review #3 started. Snapshot: PAED-002 CBD #9 Requested and DOPS #8
  Cancelled (encounter "not recorded (created 2026-09-26)"); PAED-004 KGK Teaching Session Log #6 Logged (2026-09-13);
  PAED-015 Teaching Session Log #5 Logged (2026-09-20) and Portfolio and Logbook Review #10 Awaiting review. The
  J−1y−56d session is absent. Trajectory "No rating by a named assessor to chart for this trainee." Twelve lines
  deferred; recorded "Satisfactory with Observations", "Conditions: At least two Mini-CEX before the next sitting.",
  "Present: Thandi Zulu (chair), David Naidoo"; ratified, State Ratified, pending card "Nothing was staged when the
  review was ratified, so ratifying it issued no STAR."
Gap: runbook corrected: the snapshot also holds his cancelled DOPS (every state is frozen, T138), which Expect left out;
  Expect now names it.

### Step 4.31 — Dr Mahlangu's sitting: additional training
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review and start it, and read her snapshot. Defer all twelve closing lines with the reason "Not
  enough observed evidence yet this year." Record Inadequate Progress — Additional Training:
  - the rationale "One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six
    months.";
  - Sarah Botha and John van Rensburg present.
  Then ratify.
Expect:
  - **The snapshot** lists her DOPS under PAED-002 and her Mini-CEX under PAED-004, both Completed at 3a.
  - **Sampling concentration warnings** name PAED-002 and PAED-004, one rating each.
  - **Ratified:** the state reads Ratified.
  - **Present:** "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)".
Actual (2026-09-26, T295 replay, wombat_scenario): Review #4 started. Snapshot: PAED-002 DOPS #11 3a Completed
  (2026-09-18), PAED-004 Mini-CEX #21 3a Completed (2026-09-23). Sampling warnings name PAED-002 and PAED-004, "1 rating
  · 1 assessor · 1 source" each. Twelve lines deferred with "Not enough observed evidence yet this year."; recorded
  "Inadequate Progress — Additional Training", "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg
  (external)"; "Decision ratified.", State Ratified.
Gap: none

### Step 4.32 — Dr Ndlovu's sitting: the outcome deferred
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Ndlovu's review and start it, and read his snapshot. Defer all twelve closing lines with the reason "Too
  early in the programme to decide." Record Outcome Deferred:
  - the rationale "One completed Mini-CEX and one reflection in his first semester; to be judged at the next
    sitting.";
  - Sarah Botha present.
  Then ratify.
Expect:
  - **Evidence snapshot:** his three Act 3 activities, each with its state: the Mini-CEX Dr Khumalo declined and the
    one Dr Botha completed at 3a, both under PAED-002, and the reflective exercise under PAED-001, discussed. A declined
    request is evidence too (T138).
  - **Trajectory:** PAED-002 alone, from Dr Botha's rating.
  - **Sampling:** the warnings card names PAED-002, one rating from one assessor.
  - **Ratified:** the review reads Ratified, with "Outcome Deferred".
Actual (2026-09-26, T295 replay, wombat_scenario): Review #5 started. Snapshot: PAED-001 Reflective Exercise #4 Unrated
  Discussed; PAED-002 Mini-CEX #2 Not recorded Declined and Mini-CEX #3 3a Completed ("2 records from 1 instrument").
  Trajectory PAED-002 alone (3a, 2026-09-06). Sampling names PAED-002, "1 rating · 1 assessor · 1 source". Twelve lines
  deferred with "Too early in the programme to decide."; recorded Outcome Deferred, "Present: Thandi Zulu (chair), Sarah
  Botha"; ratified, State Ratified, decision heading "Outcome Deferred".
Gap: none

## Phase 4.F — What the programme sees after the sitting

### Step 4.33 — Mr Smit reads the ratified schedule
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Read Committee Reviews, then open Dr Molefe's review.
Expect: All five read Ratified. The Decision column reads:
  - Satisfactory Progress for Molefe and Dlamini;
  - Satisfactory with Observations for du Plessis;
  - Inadequate Progress — Additional Training for Mahlangu;
  - Outcome Deferred for Ndlovu.
  No state or category prints an enum name (T250). Molefe's review shows its decision, agenda and STAR numbers, and
  offers Smit no action.
Actual (2026-09-26, T295 replay, wombat_scenario): Committee Reviews: all five Ratified; Decision column Satisfactory
  Progress (Molefe, Dlamini), Satisfactory with Observations (du Plessis), Inadequate Progress — Additional Training
  (Mahlangu), Outcome Deferred (Ndlovu); no enum name printed. Molefe's review #1: Ratified, Satisfactory Progress with
  its Present line, agenda STAR #1, #2, #3; no action offered (buttons Sign out and ×).
Gap: none

### Step 4.34 — Prof Mbatha reads what is due after the sitting
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/decisions-due
Do: Read `<P>` with the default filter, then with "Every status".
Expect:
  - **Count, default filter:** "70 of 75 decisions due in `<P>` shown."
  - **Every status:**
    - Decided on five rows, each "STAR #n, issued at review #N.": Molefe's PAED-001, 010 and 012, and Dlamini's PAED-001
      and 002;
    - Deferred on 55 rows, "Deferred at review #N. A later sitting for `<P>` plans it again.", or `<Y>` for an annual
      EPA;
    - As opportunity allows on 15.
  - **By EPA:** PAED-001 reads Due 5, Decided 2 and Deferred 3.
  - **Actions:** a deferred row offers Open review and Schedule, since no open review holds its seat now.
Actual (2026-09-26, T295 replay, wombat_scenario): Default filter: "70 of 75 decisions due in 2026 S2 shown." (55
  Deferred + 15 As opportunity allows). Every status: 75 of 75; Decided 5 (Molefe PAED-001/010/012 "STAR #1/#2/#3,
  issued at review #1.", Dlamini PAED-001/002 "STAR #4/#5, issued at review #2."), Deferred 55 ("Deferred at review #N.
  A later sitting for 2026 S2 plans it again." or "… for 2026 …" on annual EPAs), As opportunity allows 15. By EPA
  PAED-001: Due 5, Decided 2, Deferred 3. Every deferred row offers Open review and Schedule.
Gap: none

### Step 4.35 — Prof Mbatha reads the issued STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from her dashboard's quick links. Download Dr Molefe's PAED-010 certificate. Filter by
  trainee "Molefe" and apply.
Expect:
  - **The list:** five rows, all Active, issued `D`.
    - Molefe: PAED-001 `5`, PAED-010 `4` expiring `D+20`, PAED-012 `5`.
    - Dlamini: PAED-001 `4`, PAED-002 `3b`.
    - Expires reads "—" where none was set.
  - **The certificate PDF** names:
    - Kgosi Kgari Teaching Hospital, "Statement of Awarded Responsibility", Lerato Molefe, PAED-010 and level `4`;
    - Issued on `D` and Expires on `D+20`, Status Active;
    - the rationale, and the evidence summary of the two snapshot lines;
    - Panel Paed Annual Review Panel, the review number, and Chair Thandi Zulu.
  - **The filter** leaves Molefe's three rows.
Actual (2026-09-26, T295 replay, wombat_scenario): Dashboard quick link "Entrustment decisions" opens
  /admin/entrustment-decisions: five rows, all Active, issued 2026-09-26: Molefe PAED-001 5, PAED-010 4 expiring
  2026-10-16, PAED-012 5; Dlamini PAED-001 4, PAED-002 3b; Expires "—" where none. Download and Revoke named per
  trainee, EPA and issue date. PAED-010 certificate (star-certificate-PAED-010-2-….pdf): Kgosi Kgari Teaching Hospital,
  "Statement of Awarded Responsibility", Lerato Molefe, PAED-010, Authorised level 4, Issued 26 September 2026, Expires
  16 October 2026, Status Active, the rationale, evidence summary of #17 and MSF #22, Panel Paed Annual Review Panel,
  Committee review #1, Chair Thandi Zulu. Trainee "Molefe" + Apply filters leaves her three rows.
Gap: none

### Step 4.36 — Dr Mokoena revokes one of Dr Dlamini's STARs
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /admin/entrustment-decisions
Do: Type the page's address. Choose Revoke on Dr Dlamini's PAED-002 decision. Give the reason "Staged on evidence about
  PAED-001, not PAED-002; to be decided again." and confirm the revocation.
Expect:
  - **The list:** the same five rows as Step 4.35, since all five registrars are in her speciality.
  - **The dialog** names PAED-002 and Anele Dlamini, and says "Revocation is immediate and irreversible. The trainee is
    notified." Confirm revocation is enabled once a reason is typed.
  - **After confirming:** "Entrustment decision for PAED-002 revoked." The row reads Revoked, with no Revoke button.
  - **The trainee is notified,** as the dialog promises: the application log holds a mail to Dr Dlamini about the
    revocation.
Note: The page is linked from no nav item or dashboard card for her role (coverage.md). T260 (open): Confirm stays
  disabled until the reason box loses focus.
Actual (2026-09-26, T295 replay, wombat_scenario): Typed address; the same five rows. Revoke (named "Revoke Anele
  Dlamini's PAED-002 decision, issued 2026-09-26") opened an inline "Revoke entrustment decision" section, not a dialog:
  "… for PAED-002 — Managing common paediatric presentations held by Anele Dlamini. Revocation is immediate and
  irreversible. The trainee is notified." Confirm revocation stayed disabled while the reason was typed and enabled on
  blur (T260, open). Confirmed: "Entrustment decision for PAED-002 revoked."; row Revoked, only Download. No mail was
  sent: the SMTP sink holds nothing new and the log no mail line.
Gap: [F-4.36a, T319] The revocation promises "The trainee is notified." and notifies nobody: no mail to Dr Dlamini is sent
  (RevokeEntrustmentDecision revokes and saves only; no revocation template exists).

### Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Type the entrustment decisions page's address. Filter Status to Revoked and apply, then to Active and apply.
  Download the revoked certificate. Then open Decisions Due with "Every status".
Expect:
  - **Filtered:** Revoked leaves Dlamini's PAED-002, and Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", the date, who revoked it (Dr Mokoena, by name,
    T142) and the reason.
  - **Decisions Due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." It offers Schedule.
  - **By EPA:** PAED-002 reads Decided 0, Deferred 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."
Actual (2026-09-26, T295 replay, wombat_scenario): Status Revoked + Apply leaves Dlamini's PAED-002; Active leaves four
  rows. The revoked certificate (star-certificate-PAED-002-5-….pdf) reads Status Revoked, "REVOKED", "Revoked on 26
  September 2026 by f3e77734-04e5-4c77-bfc4-e27581bfeb6f" (Dr Mokoena's user id) and the reason. Decisions Due: default
  "71 of 75 decisions due in 2026 S2 shown."; Every status, Dlamini's PAED-002 "Revoked: re-decide", "STAR #5, issued
  for 2026 S2, was revoked. Schedule a review to decide it again.", with Open review and Schedule. By EPA PAED-002: Due
  5, Decided 0, Deferred 4, To schedule 1.
Gap: [F-4.37a, T324] The revoked certificate names the revoker by raw user id ("by f3e77734-…"), not "Refilwe Mokoena" as
  T142's naming rule and this step expect (EntrustmentCertificatePdfService prints RevokedByUserId).

### Step 4.38 — Prof Mbatha reads the sitting in the audit log
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Filter Action to RecordCommitteeDecisionCommand. Open one of the failed entries and one successful entry. Then
  filter to RevokeEntrustmentDecisionCommand.
Expect:
  - **The failed entries:** two FAILED rows by Dr Zulu, from Steps 4.23 and 4.24, whose errors name the quorum rule and
    the missing category. Each refusal wrote its row and changed nothing else: every check runs before the first change.
  - **The successful entries:** five OK rows, one per review.
  - **An entry's detail:** the Event card (with a failure's error) and the Actor card only. The Payload card is an
    Administrator's (Step 3.56), so she reads no summary. The stored summary, checked with SQL, keeps ReviewId,
    Category and PresentUserIds in the clear and holds Rationale and Conditions as "[REDACTED]" (T101).
  - **The revocation's entry:** its actor is Dr Mokoena. Its stored Reason is "[REDACTED]".
Actual (2026-09-26, T295 replay, wombat_scenario): Action RecordCommitteeDecisionCommand: seven rows by
  zulu@kgk.wombat.local, two FAILED (11:35:19, 11:35:51 UTC) and five OK. The first failure's error: "Validation failed:
  -- PresentUserIds: A committee decision needs at least two panel members present: the chair and at least one other.
  Severity: Error"; the second: "This review decides the trainee's progression, so its decision records a progression
  category. Choose one." Detail pages show only Event and Actor, no Payload. SQL: SummaryJson has category, reviewId and
  presentUserIds in the clear and rationale/conditions "[REDACTED]". RevokeEntrustmentDecisionCommand: one OK row, actor
  mokoena@kgk.wombat.local; stored reason "[REDACTED]".
Gap: runbook corrected: Expect had Prof Mbatha reading the summary, but the Payload card is an Administrator's (Step
  3.56); it now checks the stored summary with SQL. [F-4.38a, not a defect: T213 keeps the validator's full text in the audit row] The quorum failure's audit error is FluentValidation's raw
  text ("Validation failed: -- PresentUserIds: … Severity: Error"), where 3.56's and 4.24's failures read as plain
  sentences.

## Phase 4.G — The registrars read their outcomes

### Step 4.39 — Dr Molefe reads her STARs and downloads a certificate
Role: Trainee — Dr Lerato Molefe
Route: / → /portfolio/authorisations
Do: From the dashboard's My authorisations card, view her authorisations. Download the PAED-001 certificate.
Expect: Three cards, each issued on `D` with the committee's rationale: PAED-001 and PAED-012 at Authorised level `5`,
  and PAED-010 at `4`.
  - PAED-001 and 012 read "No expiry".
  - PAED-010 reads the date `D+20`, with the badge "Expires in 20 days".
  Each Download certificate is named by its EPA. The PAED-001 certificate downloads with Status Active and its evidence
  summary.
Actual (2026-09-26, T295 replay, wombat_scenario): Dashboard card "My authorisations" → View authorisations →
  /portfolio/authorisations: three cards issued 26 September 2026 with the committee's rationale: PAED-001 level 5 "No
  expiry", PAED-010 level 4 "16 October 2026" with "Expires in 20 days", PAED-012 level 5 "No expiry". Downloads named
  "Download certificate for PAED-001/010/012". PAED-001 certificate: Status Active, evidence summary of Mini-CEX #12 and
  CBD #13. At 390 px the cards stack with no horizontal scroll.
Gap: none

### Step 4.40 — Dr Molefe reads her progress against Annexure A
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/progress
Do: Open My Progress and read "Entrustment against Annexure A".
Expect: It says "You are in training year 4 on `D`." It then reads "2 at or above · 1 below · 12 with no decision, of
  15 EPAs". The table shows:
  - PAED-001 and 012 at `5`, each "At or above" the target of `5`, with its exit level Reached;
  - PAED-010 at `4`, "Below" the target of `5`, with its issue and expiry dates.
  The exit rule says it gates nothing. The page reads the same table as the committee's (T166).
Actual (2026-09-26, T295 replay, wombat_scenario): "You are in training year 4 on 26 September 2026." "2 at or above · 1
  below · 12 with no decision, of 15 EPAs". Table: PAED-001 5 "Issued 26 Sept 2026", At or above target 5, exit Reached;
  PAED-012 the same; PAED-010 4 "Issued 26 Sept 2026, expires 16 Oct 2026", Below target 5, exit Not yet. Exit rule "2
  of 15 … (level 5: 2 of 9 · level 4: 0 of 6)" with "For information only. Recording a Graduate decision or completing
  the programme does not check the exit rule." Same table and wording as the committee's card (Step 4.27).
Gap: none

### Step 4.41 — Dr Molefe reads her review, and not the committee's page
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → /committee/reviews/{ReviewId:int} → /access-denied
Do: Open My Committee Reviews and view her review. Then type the committee's own address for the same review.
Expect:
  - **The list:** one row, with Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31", Type Annual progression, State Ratified and
    Decision Satisfactory Progress. Its View is named by the review.
  - **The detail** shows Sits for, Evidence window, Type and State. Its decision is headed "Satisfactory Progress", with
    the rationale and "Present when this decision was taken: …" naming all four.
  - **The agenda** has three columns:
    - Decided with "STAR #n." on PAED-001, 010 and 012;
    - Deferred with "The committee's reason: To be decided at her final review." on nine;
    - "Not decided at this review." on PAED-008, 009 and 013.
  - **The committee's page** lands on access-denied.
Actual (2026-09-26, T295 replay, wombat_scenario): My Committee Reviews: one row, Paed Annual Review Panel, "2026 S2 ·
  2026-01-01 to 2026-12-31", Annual progression, Ratified, Satisfactory Progress; View named "View the 2026 S2 review
  before Paed Annual Review Panel". Detail: Sits for 2026 S2, Evidence window, Type Annual progression review, State
  Ratified; "Satisfactory Progress", the rationale, "Present when this decision was taken: Thandi Zulu (chair), David
  Naidoo, Sarah Botha, John van Rensburg (external)". Agenda EPA/Window/Outcome: Decided "STAR #1./#2./#3." on
  001/010/012; nine Deferred "The committee's reason: To be decided at her final review."; 008/009/013 "Not decided at
  this review." No horizontal scroll at 390 px. Typed /committee/reviews/1 → /access-denied.
Gap: none

### Step 4.42 — Dr Dlamini reads what stands after the revocation
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/authorisations → /committee/my-reviews
Do: View her authorisations, then her review.
Expect: One card, PAED-001 at `4`: the revoked PAED-002 is no longer listed. Her review reads Ratified, Satisfactory
  Progress. The agenda still reads Decided with STAR numbers on PAED-001 and 002, because it records what the sitting
  did.
Actual (2026-09-26, T295 replay, wombat_scenario): My authorisations: one card, PAED-001 level 4, issued 26 September
  2026, No expiry, with its rationale; the revoked PAED-002 is not listed. My Committee Reviews: one row, Ratified,
  Satisfactory Progress; its agenda still reads PAED-001 "Decided STAR #4." and PAED-002 "Decided STAR #5." No mail
  reached her for the ratification or the revocation.
Gap: none

## Phase 4.H — An appeal

### Step 4.43 — Dr Mahlangu lodges an appeal
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: View her ratified review and read it. Lodge an appeal with the reason "The single DOPS reflects the start of the
  year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration."
Expect:
  - **Before lodging:**
    - the decision reads "Inadequate Progress — Additional Training", with who was present;
    - the agenda shows twelve Deferred lines, each "The committee's reason: Not enough observed evidence yet this
      year.", and three "Not decided at this review.";
    - an Appeal reason form is offered.
  - **After lodging:** "Appeal lodged." The state reads Under appeal, the form is gone and the agenda stays in view.
Actual (2026-09-26, T295 replay, wombat_scenario): Her dashboard card's My authorisations reads "No active
  authorisations yet". The review reads "Inadequate Progress — Additional Training" with "Present when this decision was
  taken: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)"; twelve Deferred "The committee's reason: Not
  enough observed evidence yet this year." and three "Not decided at this review."; an Appeal reason form with Lodge
  appeal. Lodged: "Appeal lodged."; the detail's State reads Under appeal, the form is gone, the 15-line agenda stays.
  The list row above still reads Ratified. No mail was sent to the panel.
Gap: [F-4.43a, T308] After lodging, My Committee Reviews' list row still reads State Ratified while the detail below it reads
  Under appeal (MyReviews.razor replaces the selected review but does not reload the list).

### Step 4.44 — Dr Naidoo sees who hears the appeal
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review.
Expect: The list shows her review Under appeal. The Appeals card lists the appeal, with the date lodged, its reason and
  "(Open)". It offers him no form. It says "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu,
  and its external member, John van Rensburg." (T213, T237)
Actual (2026-09-26, T295 replay, wombat_scenario): The list shows Mahlangu's review Under appeal, Inadequate Progress —
  Additional Training. The Appeals card: "2026-09-26 The single DOPS reflects the start of the year, … I ask for
  reconsideration. (Open)" and "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its
  external member, John van Rensburg."; no form, no select (buttons Sign out and ×).
Gap: none

### Step 4.45 — Dr van Rensburg, the external member, is offered the appeal
Role: CommitteeMember (external) — Dr John van Rensburg
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews, then Dr Mahlangu's review. Read the resolve form, and press Resolve appeal without choosing
  an outcome. Then choose Remitted to see what it asks, and leave without resolving.
Expect:
  - **The list:** the panel's five reviews.
  - **The Appeals card** offers him the resolve form with an Outcome select opening on "Select an outcome…", then
    "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision" (T307, D51: there is no
    Upheld). Its help text says Dismissed leaves the decision in force, Remitted replaces it, and either closes the
    review.
  - **Resolve appeal with no outcome** says "Choose an outcome." under the select, and resolves nothing.
  - **Remitted** reveals:
    - Replacement category, opening on "Select a category…";
    - Replacement rationale;
    - Replacement conditions, optional;
    - Present, with both Thandi Zulu (chair) and himself ticked and locked.
  - **Nothing changes** until he submits.
Actual (2026-09-26, T295 replay, wombat_scenario): Committee Reviews lists the panel's five reviews. Mahlangu's Appeals
  card offers him the form: Outcome (Upheld/Dismissed/Remitted) and Resolve appeal. Outcome opens on Dismissed, with no
  empty option. Remitted reveals Replacement category ("Select a category…"), Replacement rationale and Present, with
  Thandi Zulu (chair) and John van Rensburg (external) ticked and disabled; there is no Replacement conditions field. He
  left without resolving; the appeal is still open (no outcome, not resolved) and the review Under appeal.
  Re-checked after T307 (2026-09-26, wombat_scenario_rc307a, a copy of the end-of-Act-3 snapshot with Steps 4.1 to 4.44
  replayed on it): Committee Reviews lists the panel's five reviews, Mahlangu's Under appeal. Her Appeals card offers him
  the form. Outcome opens on "Select an outcome…", then "Dismissed: the decision stands" and "Remitted: the appeal body
  replaces the decision"; there is no Upheld. Its help reads "Dismissed leaves the committee's decision in force.
  Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the
  review, and nothing reopens it." Resolve appeal with no outcome put "Choose an outcome." under the select, marked the
  select invalid (it names the message after its help) and moved the focus to it; nothing was sent (no
  ResolveAppealCommand audit row) and the appeal stayed open. Remitted revealed Replacement category ("Select a
  category…"), Replacement rationale, Replacement conditions (not marked required) and Present, with Thandi Zulu (chair)
  and John van Rensburg (external) ticked and disabled; Dismissed reveals none of them. He left without resolving: SQL
  reads the appeal open (no outcome) and the review Under appeal.
Gap: [F-4.45a, T307] fixed by T307 (d03732d): the Outcome opens on "Select an outcome…", names each outcome in words with
  help, and a submit without one is refused in the form with "Choose an outcome." and sends nothing. [F-4.45b, T307] fixed
  by T307 (d03732d): Remitted reveals an optional Replacement conditions box, and Step 4.47's conditions reach the
  replacement decision. [F-4.45c, T323] At 1280 px, once Remitted opens the form's second column, the Outcome select is 278 px
  wide and shows its choice clipped, "Remitted: the appeal body replac…" (`4.45-1`): T307's longer option wording does not
  fit the half-width column. The help under it wraps to five lines in the same column.

### Step 4.46 — A remit without a quorum is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: On Dr Mahlangu's review, choose Remitted, the replacement category Satisfactory with Observations and a
  rationale. Tick nobody besides herself as present, and resolve.
Expect: Resolve appeal is refused with "A committee decision needs at least two panel members present: the chair and
  at least one other." (T165: a remit records its own quorate sitting). The review stays Under appeal.
Actual (2026-09-26, T295 replay, wombat_scenario): Remitted, Satisfactory with Observations and the rationale, only
  Thandi Zulu (chair) ticked and locked (van Rensburg unticked for her): Resolve appeal refused with "A committee
  decision needs at least two panel members present: the chair and at least one other." The review stays Under appeal
  and the form keeps its values.
  Re-checked after T307 (2026-09-26, wombat_scenario_rc307a): the Outcome opened on "Select an outcome…" for her too.
  Remitted, Satisfactory with Observations and Step 4.47's rationale, with only Thandi Zulu (chair) ticked and disabled
  (van Rensburg unticked for her): Resolve appeal refused with "A committee decision needs at least two panel members
  present: the chair and at least one other." The review stays Under appeal, the form keeps its outcome, category and
  rationale, and the audit holds one FAILED ResolveAppealCommand. The refusal's alert is at the head of the page, about
  6,400 px above the Appeals card at 1280 px, and the focus stays on Resolve appeal, so nothing changes in view where she
  pressed it.
Gap: [F-4.46a, T299] The remit's refusal is shown only at the head of the review page, out of view of the Appeals card, which is
  the page's last card: a sighted chair who presses Resolve appeal sees nothing happen (`4.46-1` is taken after the
  refusal). A screen reader hears it, as DESIGN.md requires: the alert has role="alert" and the focus stays on the
  button. The Decision form's refusals (Steps 4.23, 4.24) land near their card, which is the page's first. Unsure: the
  rule is met; this is a usability point, older than T307.

### Step 4.47 — Dr Zulu remits the decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Keep Remitted, Satisfactory with Observations, and the rationale "The stalled Mini-CEX was the assessor's delay;
  progress is adequate, with observed assessments to follow." Write the Replacement conditions "Two observed Mini-CEX
  and one DOPS before the next review." Tick Sarah Botha as present, and resolve the appeal.
Expect: "Appeal resolved." The state reads Closed (T250).
  - **Decision card:** the replacement heads it, "Satisfactory with Observations" with "Conditions: Two observed
    Mini-CEX and one DOPS before the next review." (T307) and "Present: Thandi Zulu (chair), Sarah Botha". The first
    decision stays below it, marked "Replaced on appeal by the decision above.".
  - **Appeals card:** the appeal reads "(Remitted)".
  - **No STAR changed:** the review issued none.
Actual (2026-09-26, T295 replay, wombat_scenario): Sarah Botha ticked: "Appeal resolved."; State Closed. Decision card:
  "Satisfactory with Observations", the rationale, "Present: Thandi Zulu (chair), Sarah Botha"; below it "Inadequate
  Progress — Additional Training", "Replaced on appeal by the decision above.", with its first Present line. Appeals
  card: the appeal "(Remitted)"; no form left. STARs still 5 in total; the review issued none. No mail sent to Dr
  Mahlangu.
  Re-checked after T307 (2026-09-26, wombat_scenario_rc307a): kept Remitted, Satisfactory with Observations and the
  rationale, wrote the Replacement conditions "Two observed Mini-CEX and one DOPS before the next review." and ticked
  Sarah Botha: "Appeal resolved.", which takes the focus; State Closed. Decision card: "Satisfactory with Observations",
  the rationale, "Conditions: Two observed Mini-CEX and one DOPS before the next review." and "Present: Thandi Zulu
  (chair), Sarah Botha"; below it "Inadequate Progress — Additional Training", "Replaced on appeal by the decision
  above.", with its first Present line. Appeals card: the appeal "(Remitted)"; no form left. SQL: the appeal resolved
  with Outcome 3 (Remitted); the replacement (category 2) supersedes decision 4, holds the conditions and records 2
  present; the review is State 6 (Closed); STARs still 5; ResolveAppealCommand has one FAILED row (Step 4.46) and one OK.
Gap: none

### Step 4.48 — Dr Mahlangu reads the outcome
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: Open My Committee Reviews and view the review.
Expect: The row reads State Closed and Decision Satisfactory with Observations. The detail's current decision is the
  replacement, with "Present when this decision was taken: Thandi Zulu (chair), Sarah Botha". No appeal form is
  offered.
Actual (2026-09-26, T295 replay, wombat_scenario): My Committee Reviews row: State Closed, Decision Satisfactory with
  Observations. Detail: State Closed, "Satisfactory with Observations", the replacement rationale, "Present when this
  decision was taken: Thandi Zulu (chair), Sarah Botha"; the agenda's twelve deferrals and three not decided. No appeal
  form. The page says nothing of her appeal or its outcome: no "Remitted", no first decision, no "replaced on appeal"
  (the subtitle promises "appeal status").
  Re-checked after T307 (2026-09-26, wombat_scenario_rc307a): the row reads State Closed, Decision Satisfactory with
  Observations; the detail's current decision is the replacement, with its rationale and "Present when this decision
  was taken: Thandi Zulu (chair), Sarah Botha"; no appeal form. The page still says nothing of her appeal, its outcome
  or the first decision, and it does not show the replacement's conditions, which Step 4.47 now records.
Gap: [F-4.48a, T308] After her appeal is resolved, the trainee's review shows no trace of it: not her appeal, its outcome
  (Remitted), nor that the decision was replaced, although the page's subtitle reads "Ratified decisions, appeal status,
  and final outcomes." Unsure: no decision says what she must see; the committee's page shows it all. Re-checked after
  T307, still open. The remit's conditions, which T307 now records, are one more thing she does not see (T308 item 3:
  the trainee's detail prints no decision's Conditions).

## Phase 4.I — A formative check-in

### Step 4.49 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the panel for `<P>`, Scheduled on `D`, with Formative only ticked.
Expect:
  - **Accepted:** a formative review takes no binding seat for the period (T131 slice 4).
  - **The preview:** "A formative review carries no agenda: it decides no EPA."
  - **The review:** it opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled. It has
    no Decision, Agenda, Pending entrustment decisions or Appeals card.
  - **The list:** the review shows Mode Formative and Decision "—".
Actual (2026-09-26, T295 replay, wombat_scenario): Panel, Sipho Ndlovu, 2026 S2, Scheduled on 2026-09-26, Formative only
  ticked: the preview reads "A formative review carries no agenda: it decides no EPA." Created review #6 despite his
  ratified binding review: Mode "Formative (interim check-in, no binding decision)", State Scheduled, Start review
  offered to Smit; cards Review, Evidence snapshot, standing, MSF, trajectory; no Decision, Agenda, Pending or Appeals
  card. The list row reads Type Annual progression, Mode Formative, Scheduled, Decision "—"; its Open is named "Open the
  2026 S2 formative review of Sipho Ndlovu …".
Gap: [F-4.49a, T309] A formative check-in is recorded and shown as Type "Annual progression review": the Review type select
  stays offered (Annual progression / Pre-graduation) when Formative only is ticked, so the check-in carries a
  progression type it cannot decide. Unsure: the outcome SQL expects ReviewType 1 on it, so this may be by design.

### Step 4.50 — Dr Zulu starts and closes the check-in
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the formative review, read it, and close it.
Expect:
  - **Started:** "Review started." The snapshot freezes the same three activities as his annual review, and the standing
    and MSF coverage cards follow it. Close review is offered to her alone.
  - **Closed:** "Formative review closed." The state reads Closed, and nothing is offered.
  - **Other readers** are told "Only the panel's chair, Thandi Zulu, can close this review." while it is in progress.
Actual (2026-09-26, T295 replay, wombat_scenario): Start: "Review started.", In progress; the snapshot freezes the same
  three activities as review #5 (Reflective Exercise #4 Discussed; Mini-CEX #2 Declined, #3 3a Completed); standing
  (training year 1, "0 at or above · 0 below · 15 with no decision") and MSF cards follow it. Close review offered to
  her alone: Dr Naidoo and Mr Smit see "Only the panel's chair, Thandi Zulu, can close this review." and no button.
  Close (no confirmation): "Formative review closed.", State Closed, nothing offered.
Gap: none

### Step 4.51 — Dr Ndlovu reads both reviews
Role: Trainee — Dr Sipho Ndlovu
Route: /committee/my-reviews
Do: Open My Committee Reviews.
Expect: Two rows for `<P>`. The annual review reads Ratified, Outcome Deferred. The check-in reads Closed, with Decision
  "No binding decision".
Actual (2026-09-26, T295 replay, wombat_scenario): Two rows for 2026 S2: the check-in "Annual progression, Closed, No
  binding decision" (View named "View the 2026 S2 formative review before Paed Annual Review Panel") and the annual
  review "Annual progression, Ratified, Outcome Deferred". The trainee's list has no Mode column, so only the View's
  name and "No binding decision" mark the check-in (see F-4.49a).
Gap: none

## Outcome state

Replay check (2026-09-26, T295, wombat_scenario): match on all six queries (six reviews: four binding Ratified, Mahlangu's Closed, Ndlovu's formative Closed; 75 agenda lines, 55 Deferred, 5 Decided, 15 Not decided; six decisions with Molefe 4, Dlamini 3, du Plessis 2, Mahlangu 3 then 2, Ndlovu 2 present, the remit superseding decision 4; five STARs, Dlamini's PAED-002 Revoked, PAED-010 expiring 2026-10-16, 2 evidence links each; 0 pending, 1 remitted appeal; frozen lines Molefe 9, Dlamini 4, du Plessis 5, Mahlangu 2, Ndlovu 3 and 3); reached by one player for 4.6 to 4.51 after the previous player stopped at 4.6 (see Step 4.6).

These steps add, before the panel `Paed Annual Review Panel`, for the period `<P>`:
- **Six reviews.** Five are binding Annual progression reviews: four Ratified, and Mahlangu's Closed after its appeal
  was remitted. The sixth is Ndlovu's formative check-in, Closed.
- **75 agenda lines.** 55 Deferred, 5 Decided and 15 Not decided.
- **Six committee decisions, each with a quorum recorded.** Mahlangu's replacement supersedes her first.
- **Five STARs.** Molefe's three are Active. Of Dlamini's two, PAED-001 is Active and PAED-002 Revoked. Each STAR
  carries evidence links to its review's snapshot, and none is left staged.
- **One appeal,** Remitted.

Check them with psql against the replay's database:

```sql
-- 1. Six reviews. ReviewType 1 = Annual progression; State 4 = Ratified, 6 = Closed.
--    Expect dlamini, duplessis, molefe and ndlovu binding at 4; mahlangu binding at 6; ndlovu formative at 6.
SELECT u."Email", r."AcademicYear", r."Semester", r."ReviewType", r."IsFormative", r."State"
FROM "CommitteeReviews" r
JOIN "DecisionPanels" p ON p."Id" = r."PanelId"
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
WHERE p."Name" = 'Paed Annual Review Panel'
ORDER BY r."IsFormative", u."Email";

-- 2. Agenda lines by trainee and state (2 Deferred, 3 Decided, 4 Not decided). Expect 75 lines:
--    molefe 9/3/3, dlamini 10/2/3, duplessis 12/0/3, mahlangu 12/0/3, ndlovu 12/0/3.
SELECT u."Email", l."State", count(*) AS lines
FROM "CommitteeAgendaLines" l
JOIN "CommitteeReviews" r ON r."Id" = l."ReviewId"
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
WHERE u."Email" LIKE '%@kgk.wombat.local'
GROUP BY u."Email", l."State"
ORDER BY u."Email", l."State";

-- 3. Decisions and who sat. Category 1 = Satisfactory Progress, 2 = with Observations, 3 = Additional Training,
--    6 = Outcome Deferred. Expect six rows:
--    molefe 1 (4 present), dlamini 1 (3), duplessis 2 (2), mahlangu 3 (3), mahlangu 2 (2, superseding the other),
--    ndlovu 6 (2).
SELECT u."Email", d."Id", d."Category", d."SupersedesDecisionId", count(a."Id") AS present
FROM "CommitteeDecisions" d
JOIN "CommitteeReviews" r ON r."Id" = d."ReviewId"
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
LEFT JOIN "CommitteeDecisionAttendees" a ON a."DecisionId" = d."Id"
WHERE u."Email" LIKE '%@kgk.wombat.local'
GROUP BY u."Email", d."Id", d."Category", d."SupersedesDecisionId"
ORDER BY u."Email", d."Id";

-- 4. STARs. Status 1 = Active, 3 = Revoked. Expect five rows:
--    dlamini PAED-001 4 Active and PAED-002 3b Revoked;
--    molefe PAED-001 5, PAED-010 4 (expiring D+20) and PAED-012 5, all Active.
--    Each with evidence >= 1.
SELECT u."Email", e."Code", lv."Label", s."Status", s."ExpiresOn", count(k."Id") AS evidence
FROM "EntrustmentDecisions" s
JOIN "AspNetUsers" u ON u."Id" = s."TraineeUserId"
JOIN "Epas" e ON e."Id" = s."EpaId"
JOIN "EntrustmentLevels" lv ON lv."Id" = s."AuthorisedLevelId"
LEFT JOIN "EntrustmentEvidenceLinks" k ON k."DecisionId" = s."Id"
WHERE u."Email" LIKE '%@kgk.wombat.local'
GROUP BY s."Id", u."Email", e."Code", lv."Label", s."Status", s."ExpiresOn"
ORDER BY u."Email", e."Code";

-- 5. Nothing is left staged, and one appeal was remitted (Outcome 3). Expect 0 and 1.
SELECT (SELECT count(*) FROM "PendingEntrustmentDecisions") AS pending,
       (SELECT count(*) FROM "CommitteeAppeals" WHERE "Outcome" = 3 AND "ResolvedOn" IS NOT NULL) AS remitted;

-- 6. Frozen evidence per review. Expect ndlovu 3 on each of his two reviews, and more than 0 for the other four.
SELECT u."Email", r."IsFormative", count(i."Id") AS lines
FROM "CommitteeReviews" r
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
LEFT JOIN "CommitteeEvidenceItems" i ON i."ReviewId" = r."Id"
WHERE u."Email" LIKE '%@kgk.wombat.local'
GROUP BY r."Id", u."Email", r."IsFormative"
ORDER BY u."Email", r."IsFormative";
```

Then take the snapshot `recovery/scenario-post-act4.dump`.

## Handoff to Act 5

- **Molefe holds three Active STARs** from review #N for `<P>`: PAED-001 and PAED-012 at `5`, their exit level, with no
  expiry, and PAED-010 at `4`, below it, expiring on `D+20`.
- **Her nine deferred EPAs read Deferred on Decisions Due.** A later sitting for `<P>` plans them again.
  - Her ratified review holds no seat, so Act 5 can schedule her final Pre-graduation review for `<P>`.
  - Played for `<P>`, that review's agenda leaves off PAED-001, 010 and 012 as "Already decided in this window".
    Re-deciding PAED-010 at `5` stages it on the chair's own line and supersedes the Act 4 STAR when ratified, so the
    `D+20` expiry does not reach the appendix's expiry job on an Active STAR.
- **The panel is unchanged,** a General panel of four, and every review before it is ratified or closed. Nothing is left
  open for the appendix's erasure to withdraw.
- **Dlamini's PAED-002 reads "Revoked: re-decide".** The four other registrars' reviews, and Mahlangu's remitted appeal,
  stay as the record, for the portfolio PDF and for coverage.md. Dr du Plessis's programme ends in Act 5, and Act 6 has
  Dr Dlamini, Dr Mahlangu and Dr Ndlovu act again.

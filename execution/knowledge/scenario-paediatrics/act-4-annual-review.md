# Act 4 — Month 12: annual review

**Actuals before T361.** An Actual dated before the T361 replay describes the review page with deferral, which D53
removed; the Expects are the page as built.

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
closes the year's entrustment decisions. There is no deferral (D53): every EPA on the agenda that the chair does not
stage is recorded as "Not decided at this sitting" when the review is ratified, due and optional alike, with the
decision's one optional sentence about the EPAs not decided, which the registrar reads once it is ratified. A STAR is a
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
1. All five reviews are scheduled for the period, each with its agenda, and each is started, staged, decided by a quorum
   and ratified; what is not staged is recorded as not decided.
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
  - each count of lines due at this sitting below falls to match; the lines left unstaged are still recorded as not
    decided, but decisions due reads an annual one as "Due by year end", not "Not decided" (E3).
- **The training year in a review's subtitle** is read at the end of the review's period (T361, E6), not on `D` as the
  standing is. In semester 2 the two agree; on a replay between 1 and 14 January the subtitle reads a year more.
- **The evidence window must hold Act 3's encounters.** Played in semester 2, as Act 3 is, the filled window (1 January
  to 31 December) holds all of them but du Plessis's `J−1y−56d` session, which predates his programme and stays out.

## Phase 4.A — What is due before anything is scheduled

### Step 4.1 — Mr Smit reads the decisions due
Role: Coordinator — Mr Pieter Smit
Route: / → /committee/decisions-due
Do: Open Decisions due from the menu. Leave Period on `<P>` and Status on "Outstanding: not yet decided". Page through
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
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Menu Home, Programme trainees, Waiting for assessors, Decisions
  due, MSF campaigns, Committee reviews, Decision panels, Data rights requests, My data rights; Decisions due lit.
  Intro names 2026 S2, "the 5 trainees you oversee at Kgosi Kgari Teaching Hospital" and what Missed means. By EPA: Due
  5 on all 15; To schedule 5 on twelve, Optional 5 on 008/009/013. "75 of 75 decisions due in 2026 S2 shown.", "Showing
  1–20 of 75". 30 semester rows "No open review holds it, and 2026 S2 has not ended.", 30 annual rows "... and 2026 has
  not ended.", 15 "As opportunity allows". Schedule named "Schedule a review of <trainee> for <EPA> before Paed Annual
  Review Panel". Not scheduled + PAED-004: 5 rows, one per registrar; By EPA unchanged. No Missed row.
Gap: [F-4.1a, T280] (still) The pager's page-size select has no accessible name: no label, id or aria-label.

### Step 4.2 — Prof Mbatha reads the same period
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /committee/decisions-due
Do: Open Decisions due from the menu. Look for an Institution filter, then switch Period to the previous semester and
  back to `<P>`.
Expect: No Institution filter is offered, because she reads her own institution. The same five registrars and 75 rows
  as Step 4.1. For the previous semester, `<Y> S1`, the statuses follow that period's windows:
  - every registrar's six semester EPAs read Missed ("`<Y> S1` has ended with nothing decided or on an open review's
    agenda.", T361). Mahlangu and Ndlovu started on 15 January, inside the semester's first month, so their windows
    count too (D42);
  - the annual EPAs read "Due by year end";
  - PAED-008, 009 and 013 read "As opportunity allows", since an optional EPA is never missed.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Filters Period, Status and EPA; no Institution filter. 2026 S2:
  the same 5, "75 of 75". 2026 S1: "75 of 75 decisions due in 2026 S1 shown.", 30 rows Missed "2026 S1 has ended with
  nothing decided or on an open review's agenda." (By EPA Missed 5 on 001/002/004/005/010/012, so Mahlangu's and
  Ndlovu's count); 30 annual rows "Due by year end"; 15 "As opportunity allows". Back on 2026 S2: 75 of 75.
Gap: none

### Step 4.3 — Dr Mokoena reads what is due in her speciality
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/decisions-due
Do: Sign in, read her menu, and open Decisions due.
Expect:
  - **Menu:** Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and
    Entrustment decisions, then My data rights (T358, R2-Menus). Decisions due is lit on its page.
  - **Home's cards:** Waiting for assessors, Registrars and Targets by EPA, read for Paediatrics. Waiting for assessors
    holds Dr du Plessis's CBD and portfolio review, both still open from Act 3, the review Overdue; Registrars the five,
    badged "5 registrars".
  - **The page:** the same five registrars, all in Paediatrics, and 75 rows, each Schedule link offered.
  - **Entrustment decisions** is in the menu (Step A.7.7 opens it from there); no card on Home names it.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Home "Speciality admin · Semester 2, 2026". Menu Home,
  Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels, Entrustment decisions,
  My data rights; Decisions due lit on its page. Cards: Waiting for assessors "2 waiting, 1 overdue" (du Plessis's
  Portfolio and Logbook Review, With Mohammed Patel, Awaiting review, Overdue, "Waiting 8 days"; his CBD, With Fatima
  Khumalo, Requested), Registrars "5 registrars", Targets by EPA; no card names Entrustment decisions. Decisions due:
  the five, "75 of 75 decisions due in 2026 S2 shown.", Schedule on all 75 rows.
Gap: none

### Step 4.4 — Dr Sithole reads what is due in his sub-speciality
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/decisions-due
Do: Sign in and open Decisions due.
Expect: The same five registrars and 75 rows as Step 4.3: all five train in the Paediatrics sub-speciality Dr Sithole
  administers. Dr Sithole's menu and Home's cards read as Dr Mokoena's.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Home "Sub-speciality admin · Semester 2, 2026"; menu and the
  three cards as Mokoena's (Waiting for assessors "2 waiting, 1 overdue", Registrars "5 registrars", Targets by EPA; no
  Entrustment decisions card). Decisions due lit: "the 5 trainees you oversee at Kgosi Kgari Teaching Hospital", "75 of
  75 decisions due in 2026 S2 shown.", Schedule on every row.
Gap: none

### Step 4.5 — Dr Naidoo is not offered what is due
Role: CommitteeMember — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /committee/decisions-due → /access-denied
Do: He still acts as Assessor, his choice from Step 3.5: choose Switch to Committee member in the sidebar. Look for
  Decisions due in the menu, then type its address.
Expect: His menu has Committee reviews and Decision panels but no Decisions due. The typed address lands on the
  access-denied page, and nothing in his menu is lit there: the page is for the roles that schedule reviews.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Home "Assessor · Semester 2, 2026" (menu Home, Activity inbox,
  My data rights) with "Switch to Committee member" (/dashboard/switch/CommitteeMember). After it: "You are now acting
  as Committee member."; menu Home, Programme trainees, Committee reviews, Decision panels, My data rights; no
  Decisions due. Typed /committee/decisions-due -> /access-denied?ReturnUrl=..., "You cannot open this page", "None of
  your roles (Committee member, Assessor) opens this page."; no menu item lit.
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
Do: Open Committee reviews from the menu and choose Schedule review. Fill in:
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
  - **The preview** reads "15 EPAs will be on the agenda for `<P>`." It then says "PAED-001, PAED-002, PAED-003,
    PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this
    sitting." (T361, round 3 item 29: no deferral, D53). It lists PAED-008, 009 and 013 as "As opportunity allows".
  - **After Create review:** the page opens the new review (T361):
    - the h1 and the tab read "Lerato Molefe · `<P>`" ("… · Wombat" in the tab);
    - the subtitle reads "Annual progression review · Paed Annual Review Panel · Training year 4";
    - the sitting bar, under the title, reads Scheduled, "15 EPAs on the agenda" and "Summative · evidence
      `<Y>`-01-01 to `<Y>`-12-31".
  - **Start review…** is offered to Smit in the sitting bar, since the coordinators of the panel's institution may start
    a review (E2). He leaves it for the chair.
  - **Evidence snapshot:** "No evidence has been frozen yet. Starting the review captures the current bundle."
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Committee reviews "Schedule progression reviews and open
  existing ones.", "No reviews yet". The one panel; the Trainee select then lists the five. Period "2026 S2 · 1 Jul to
  31 Dec 2026", window 2026-01-01 to 2026-12-31. Review type Annual progression / Pre-graduation, help "The semester-2
  sitting decides the trainee's progression for the year." Preview "15 EPAs will be on the agenda for 2026 S2.",
  "PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and
  PAED-015 are due at this sitting.", 008/009/013 "As opportunity allows". Create review opened /committee/reviews/1:
  h1 "Lerato Molefe · 2026 S2", tab "Lerato Molefe · 2026 S2 · Wombat", subtitle "Annual progression review · Paed
  Annual Review Panel · Training year 4"; the bar Scheduled, "15 EPAs on the agenda", "Summative · evidence 2026-01-01
  to 2026-12-31", "As of 13:49 SAST", Refresh and Start review… (offered to Smit, left). "No evidence has been frozen
  yet. Starting the review captures the current bundle."
Gap: none

### Step 4.7 — Mr Smit schedules Dr Dlamini's and Dr du Plessis's reviews
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule the same kind of review for Dr Dlamini, then for Dr du Plessis: same panel, `<P>`, window as filled,
  Scheduled on `D`, Annual progression review.
Expect: Each opens as Scheduled with a 15-EPA preview like Step 4.6's. Committee reviews lists three rows for `<P>`.
  Each row reads:
  - Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31";
  - Type Annual progression;
  - Mode Summative;
  - State Scheduled;
  - Decision Pending.
  Each row's Open is named by the review it opens.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Reviews #2 (h1 "Anele Dlamini · 2026 S2", Training year 3) and
  #3 ("Pieter du Plessis · 2026 S2", Training year 2), each Scheduled after the same 15-EPA preview. The list: three
  rows "2026 S2 · 2026-01-01 to 2026-12-31", Annual progression, Summative, Scheduled, Pending; each Open named "Open
  the 2026 S2 review of <trainee> before Paed Annual Review Panel".
Gap: none

### Step 4.8 — A second review of Dr Molefe for the same period is refused
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Schedule another Annual progression review of Dr Molefe before the same panel for `<P>`.
Expect: Refused, and nothing is created. The refusal names the open review: "Review #N already puts this trainee before
  Paed Annual Review Panel for `<P>` (scheduled, scheduled `D`). A trainee has one binding review for each period …".
  The wording is a product decision (T131 slice 4). The list still holds three rows.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): The preview still read "15 EPAs will be on the agenda for 2026
  S2." with no warning. Create review refused: "Review #1 already puts this trainee before Paed Annual Review Panel for
  2026 S2 (scheduled, scheduled 2026-10-12). A trainee has one binding review for each period ...". Stayed on
  /committee/reviews; the list still three rows.
Gap: [F-4.8a, T309] (still) The agenda preview gives no sign that the trainee already holds this period's binding seat;
  the refusal comes only on Create review.

### Step 4.9 — Dr Mokoena schedules Dr Mahlangu's review from what is due
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions due, choose Schedule on Dr Mahlangu's PAED-001 row. Check the form it opens, set Scheduled on `D`, and
  create the review.
Expect:
  - **Decisions due before she schedules:** Molefe's, Dlamini's and du Plessis's rows now read Scheduled, "On the agenda
    of review #N, which is still open.", each with Open review.
  - **The link opens the scheduling form already filled:** Panel `Paed Annual Review Panel`, Trainee Dr Mahlangu and
    Period `<P>`. The preview shows 15 EPAs. The query string fills only what the form offers her, so the link
    authorises nothing.
  - **The review opens as Scheduled,** and Start review… is not offered to her. Her sitting bar, static under the title,
    says: "Only the coordinators of the panel's institution, and those of its members who are active committee members
    there, can start this review." (T194, E2), with "As of hh:mm SAST" and Refresh.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): EPA PAED-001: Molefe, Dlamini and du Plessis Scheduled, "On the
  agenda of review #1/#2/#3, which is still open.", each with Open review; By EPA Scheduled 3, To schedule 2. The
  Schedule link (?panel=1&trainee=...&period=2026-2) opened the form filled: Paed Annual Review Panel, Nomsa Mahlangu,
  2026 S2, window 2026-01-01 to 2026-12-31; preview 15 EPAs. Review #4 "Nomsa Mahlangu · 2026 S2", Scheduled; no Start
  review; her bar static (reader) under the title: "Only the coordinators of the panel's institution, and those of its
  members who are active committee members there, can start this review.", "As of 13:51 SAST" and Refresh.
Gap: none

### Step 4.10 — Dr Sithole schedules Dr Ndlovu's review from what is due
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions due, choose Schedule on one of Dr Ndlovu's rows. Set Scheduled on `D` and create the review.
Expect: As Step 4.9: the form is filled, the review opens as Scheduled, and Start review… is not offered: the sitting
  bar says who can start it. Decisions due then
  reads Scheduled on all 75 rows, and By EPA reads Scheduled 5 and To schedule 0 for every EPA.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Ndlovu's PAED-002 Schedule link opened the form filled (panel,
  Sipho Ndlovu, 2026 S2, Annual progression review); review #5 "Sipho Ndlovu · 2026 S2" Scheduled, no Start review, the
  same "Only the coordinators ..." sentence with "As of 13:52 SAST" and Refresh. Decisions due "75 of 75 ...": all 75
  rows Scheduled; By EPA Scheduled 5 and To schedule 0 on every EPA.
Gap: none

### Step 4.11 — Dr Naidoo lists the panel's reviews
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then open Dr Dlamini's review.
Expect:
  - **The list:** its subtitle reads "Open existing committee reviews.", and no Schedule review button is offered. It
    lists the five reviews of the panel he sits on, all Scheduled, with Decision Pending.
  - **Dlamini's review:** Start review… is offered to him in the sitting bar, as a member who may sit on the panel (E2).
    He leaves it.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Acting as Committee member (4.5). Subtitle "Open existing
  committee reviews.", no Schedule review; five rows (Ndlovu, Mahlangu, du Plessis, Dlamini, Molefe), Annual
  progression, Summative, Scheduled, Pending. Dlamini's review #2 offers him Start review… in a reader's bar (with "As
  of 13:52 SAST" and Refresh); left.
Gap: none

### Step 4.12 — Prof Mbatha checks the schedule
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/decisions-due
Do: Read Committee reviews, then Decisions due.
Expect: Committee reviews offers Schedule review and lists all five reviews. Decisions due reads Scheduled on all 75
  rows. Each row's "Open review" opens the review holding it, and no row offers Schedule.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Committee reviews offers Schedule review and lists the five.
  Decisions due "75 of 75 ...": all 75 rows Scheduled, each Open review named "Open review #N for <trainee>, <EPA>" and
  linking the review its row names (all 75 checked); no row offers Schedule.
Gap: none

### Step 4.13 — The panel cannot change what it decides while its reviews are open
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels → /committee/panels/{PanelId:int}
Do: Open the panel. In Decides for, choose the Neonatal team Clinical Competency Committee and save the committee.
Expect: Refused, and the panel stays a General panel. The refusal names the first open review: "Review #N (`<P>`,
  scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the
  panel sat as when it was scheduled …" (T131 slice 5). The select's help text says the same rule before she tries.
Note: If the save is accepted, set Decides for back to General panel before going on, and record the defect.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Decision panels -> "Edit Paed Annual Review Panel" ->
  /committee/panels/1. The College committee help ends "It cannot change while a review before the panel is scheduled,
  in progress or awaiting ratification." Save committee with the Neonatal team CCC refused: "Review #1 (2026 S2,
  scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the
  panel sat as when it was scheduled, ...". SQL: DecisionBodyKey still null (General).
Gap: none

## Phase 4.C — Before the sitting

### Step 4.14 — Dr Botha checks who sits and who decides
Role: CommitteeMember — Dr Sarah Botha
Route: /committee/panels
Do: Open Decision panels.
Expect:
  - **The list:** `Paed Annual Review Panel` with Scope Speciality, Decides for General panel and Members 4. There is no
    New panel button and no Edit column: she manages no panel (T239, T256).
  - **Who decides each EPA:** curriculum 11.1's EPAs are decided by `Paed Annual Review Panel`. PAED-004 and 005 fall
    back to it, because no panel covering the programme sits as the neonatal CCC.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Acting as Committee member; Decision panels lit. One row: Paed
  Annual Review Panel, Speciality, General panel, 4; no New panel, no Edit column, no control in main. Who decides each
  EPA: PAED-004, PAED-005 -> Paed Annual Review Panel, "No panel covering this programme sits as the Neonatal team
  Clinical Competency Committee, so the general panel decides."; the other 13 -> the same panel.
Gap: none

### Step 4.15 — Dr Zulu reads Dr Molefe's review before starting it
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews and Dr Molefe's review. Read every section before starting it.
Expect: The page is one column, the agenda its spine (T361, round 3):
  - **The header:** the h1 and the tab read "Lerato Molefe · `<P>`"; the subtitle "Annual progression review · Paed
    Annual Review Panel · Training year 4", with the link "Lerato Molefe's progress" to her registrar page under it
    (round 3 item 19). There is no Review card and no Pending card.
  - **The chair's sitting bar,** under the title: Scheduled, "15 EPAs on the agenda", "Summative · evidence
    `<Y>`-01-01 to `<Y>`-12-31", and Start review… (round 3 item 28).
  - **Decision:** "No decision has been recorded yet.", under the agenda.
  - **Entrustment against Annexure A** reads training year 4 on `D`, and "0 at or above · 0 below · 15 with no decision,
    of 15 EPAs". The exit rule reads "0 of 15 EPAs at their exit level by STAR decision", names all fifteen as "Not
    yet", and says it is for information only (T166). The names of PAED-001, PAED-010 and PAED-012 link to their charts
    on the page (T355).
  - **Multi-source feedback by EPA** marks Covered, in the semester Act 3's campaign closed in, each EPA that campaign
    covered. The other cells read "None released" or "None released yet" (T168).
  - **Agenda,** captioned "15 EPAs for `<P>`", lists the 15 EPAs, each its code over its title:
    - window `<P>` for PAED-001, 002, 004, 005, 010 and 012 and `<Y>` for the other nine (round 3 item 24);
    - twelve "Due at this sitting", with no sentence under the badge (D53);
    - three "As opportunity allows", each "Decided as opportunity allows: optional.";
    - Evidence 0 items each;
    - no Action column, because the review has not started.
  - **Evidence snapshot:** "No evidence has been frozen yet. Starting the review captures the current bundle."
  - **Rating trajectory by EPA** charts PAED-001, PAED-010 and PAED-012 on the ladder's rungs, each over the review's
    window and with no Today. Each chart's line names the trainee, the window and the assessors, PAED-001's reading
    "Lerato Molefe · 3 ratings in the review window, `<Y>`-01-01 to `<Y>`-12-31, from Thandi Zulu, David Naidoo and
    Mohammed Patel. 2 at the minimum, 1 below.", and the table of its ratings is under it (T355, T323).
  - **Before the committee decides,** the first section under the bar, not dismissible (T280): its "Sampling
    concentration warnings" read "PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors
    across this EPA's evidence." and "PAED-012 · 2 ratings · 2 assessors · 2 sources. Fewer than three distinct
    assessors across this EPA's evidence." (round 3 item 31). PAED-001, rated by three assessors on three instruments, is
    not named. "Not counted: 2 records in the review window with no rating by a named assessor …" says her two MSF
    records are not counted (T135, D44).
  - **Appeals:** "No appeal has been lodged against this review."
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Review #1: h1 and tab "Lerato Molefe · 2026 S2" ("… · Wombat"),
  subtitle "Annual progression review · Paed Annual Review Panel · Training year 4" with "Lerato Molefe's progress"
  (/programme/trainees/2); no Review or Pending card. The chair's bar (sticky): Scheduled, "15 EPAs on the agenda",
  "Summative · evidence 2026-01-01 to 2026-12-31", Start review…. Sections in order: Before the committee decides
  (first, no ×), Agenda, Decision ("No decision has been recorded yet."), Entrustment against Annexure A, MSF, Evidence
  snapshot, trajectory, Appeals. Standing "In training year 4 on 2026-10-12", "0 at or above · 0 below · 15 with no
  decision, of 15 EPAs", "0 of 15 EPAs at their exit level by STAR decision ...", all fifteen Not yet, "For information
  only. ..."; PAED-001, 010, 012 link #trajectory-2/11/13-h. MSF: Semester 2 PAED-010 and 012 Covered ("Default MSF #1,
  closed 2026-10-12"), the rest "None released" / "None released yet". Agenda caption "15 EPAs for 2026 S2", columns
  EPA/Window/State/Evidence (no Action), 2026 S2 x6 and 2026 x9, twelve "Due at this sitting" with no sentence, three
  "As opportunity allows" with "Decided as opportunity allows: optional.", 0 items each. "No evidence has been frozen
  yet. ..." Trajectory: PAED-001 "Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from Thandi
  Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.", PAED-010 and 012 likewise, no Today, each with
  its table. Sampling names PAED-010 (1 rating · 1 assessor · 1 source) and PAED-012 (2 · 2 · 2) in round 3's words,
  not PAED-001; "Not counted: 2 records ...". "No appeal has been lodged against this review." Enter on the PAED-010
  link focused its h3 at y 507, URL unchanged; 390 px no sideways scroll. T356: at 1280 and 768 px, scrolled through
  the page, the sticky bar sits at 56-110 px under the top bar and Sign out (14-42 px) is never covered.
Gap: none

## Phase 4.D — Dr Molefe's sitting

### Step 4.16 — Dr Zulu starts the review
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Start review… in the sitting bar, read the dialog, and start the review.
Expect:
  - **The dialog** is titled "Start Lerato Molefe's review for `<P>`?" and reads "Starting freezes the evidence: the 9
    items observed from `<Y>`-01-01 to `<Y>`-12-31 (8 activities and the MSF campaign report) become this review's
    snapshot, each in the state it is in now. Anything finished later is not added." (round 3 item 11). No sentence
    about a campaign left out: the campaign is released (T173). The focus is on its safe button, "Not yet"; the confirm
    is "Start review".
  - **Started:** "Review started." atop the agenda, with the focus there. The bar reads In progress, "None staged",
    "Present: the chair · one more member needed" and Go to the decision.
  - **Agenda:** its caption reads "15 EPAs for `<P>` · none staged". Each line counts its snapshot items and offers
    Stage only (D53).
  - **Evidence snapshot:** "Frozen when the review started, `D` · 9 items", an h3 per EPA ("PAED-001 — …"), its lines
    by instrument A–Z (round 3 item 30). Every state is listed, and each line links to its activity and shows its
    assessor, rating, encounter date and state. The MSF campaign's report sits under "Not about a single EPA", and its
    per-EPA records are listed under each EPA.
  - **Decision,** under the agenda: a form with these fields:
    - Category, opening on "Select a category…";
    - Rationale;
    - "About the EPAs not decided (optional)", with the help "The registrar sees this once the review is ratified.";
    - Conditions;
    - Present, listing Thandi Zulu (chair) ticked and locked, David Naidoo, Sarah Botha and John van Rensburg
      (external).
  - **Record decision…** is unavailable (aria-disabled, not natively disabled, round 3 item 16), with the quorum line
    beside it: "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and
    at least one other." It names no EPA: no line holds Record back (D53).
  - **Start** is also offered to a member who may sit, the Coordinator and the Administrator, never while the chair
    cannot act (E2).
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Dialog "Start Lerato Molefe's review for 2026 S2?": "Starting
  freezes the evidence: the 9 items observed from 2026-01-01 to 2026-12-31 (8 activities and the MSF campaign report)
  become this review's snapshot, each in the state it is in now. Anything finished later is not added."; no left-out
  sentence; focus on "Not yet"; confirm "Start review" (`4.16-2`). Then "Review started." atop the agenda, focused. Bar
  In progress, "None staged", "Present: the chair · one more member needed", Go to the decision. Caption "15 EPAs for
  2026 S2 · none staged"; each line counts its items (PAED-001 3, 010 2, 012 3, the rest 0) and offers Stage only.
  Snapshot "Frozen when the review started, 2026-10-12 · 9 items", h3 per EPA, lines by instrument A-Z (PAED-001: CBD
  #13 David Naidoo 5, DOPS #14 Mohammed Patel 4, Mini-CEX #12 Thandi Zulu 5), each linking its activity with assessor,
  rating, encounter and state; MSF #22/#23 "Not recorded", Recorded, under PAED-010/012; Default MSF #1 under "Not
  about a single EPA". Decision form: Category "Select a category…", Rationale, "About the EPAs not decided (optional)"
  with "The registrar sees this once the review is ratified.", Conditions, Present (Thandi Zulu (chair) ticked and
  locked, David Naidoo, Sarah Botha, John van Rensburg (external)). Record decision… aria-disabled (not disabled) with
  "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one
  other."; no EPA named. T356 in progress: at 1280 and 768 px the bar sticks at 56-110 px, Sign out (14-42) never
  covered.
Gap: none

### Step 4.17 — Dr Zulu stages PAED-001 at level 5
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int} → /activities/{ActivityId:int} → /committee/reviews/{ReviewId:int}
Do: Open Dr Zulu's own PAED-001 Mini-CEX from the evidence snapshot to re-read it, then come back to the review by its
  return link. Choose Stage on PAED-001's agenda line and fill in:
  - Authorised level `5`;
  - Issued on `D`, as filled;
  - no expiry;
  - the rationale "Leads resuscitation and emergency care without prompting; ready for unsupervised practice."
  Tick the Mini-CEX, then Dr Naidoo's CBD, and stage the pending decision.
Expect:
  - **The snapshot line** "Mini-CEX (Paediatrics) #12" opens the Mini-CEX at
    `/activities/{ActivityId}?from=review-{ReviewId}&at=snap-{itemId}`, read-only: its status card, badged Completed,
    reads "Done. You completed it on …" and "Rated 5. Credited 1 item to PAED-001.", and no move is offered (T342). Its
    only back link is "Back to Lerato Molefe · `<P>`" (round 3 item 9), which returns to the review at that snapshot
    line (`#snap-{itemId}`).
  - **Choosing Stage** opens PAED-001's line in place, under its row, headed "Stage a decision on PAED-001", with the
    focus on Authorised level. Its standing reads "Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF:
    None released yet" (round 3 item 26), with the link "Rating chart for PAED-001".
  - **The level select** offers the rungs 1, 2, 3a, 3b, 4 and 5 under "CPSA Paediatric Entrustment Scale v11.1", with
    the help "Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on." (T076, T167)
  - **"Evidence it rests on"** lists PAED-001's three lines first, by instrument A–Z: Case-Based Discussion
    (Paediatrics) #13, DOPS (Paediatrics) #14 and Mini-CEX (Paediatrics) #12, each with a tick and Read; the other
    lines sit behind "Show the other N snapshot lines". Read shows an activity in place of the form, and "Back to
    PAED-001" restores the form as typed (round 3 item 8). Stage pending decision stays unavailable with "Name at least
    one item of the evidence snapshot." beside it until a line is ticked (D38, round 3 item 16).
  - **With one line ticked,** the picker says "Only one item is named. The College's rule is that an entrustment
    decision is never taken from a single form." It still allows the stage.
  - **After staging:** "Pending entrustment decision staged." under PAED-001's line, with the focus there. The line reads
    Staged, "Level 5 · issued `D` · no expiry." and "Rests on 2 items of the snapshot: Mini-CEX (Paediatrics) #12;
    Case-Based Discussion (Paediatrics) #13." (round 3 item 30), with Remove… in its Action cell. The caption and the
    bar read "1 staged".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Snapshot line "Mini-CEX (Paediatrics) #12" ->
  /activities/12?from=review-1&at=snap-8: status card Completed, "Done. You completed it on 2026-10-12 12:37 SAST.",
  "Rated 5. Credited 1 item to PAED-001."; no control; its only back link "Back to Lerato Molefe · 2026 S2"
  (/committee/reviews/1#snap-8). Followed, it lands on the review with scrollY 0 and the focus on the h1, the snap-8
  row 7,440 px down (a typed /committee/reviews/1#snap-8 does scroll to it). Stage on PAED-001: "Stage a decision on
  PAED-001" under its row, focus on Authorised level; standing "Year 4 target 5 · no STAR yet · 3 ratings in the review
  window · MSF: None released yet", "Rating chart for PAED-001". Level "Choose a rung…", 1, 2, 3a, 3b, 4, 5 in the
  optgroup "CPSA Paediatric Entrustment Scale v11.1", help "Rungs of CPSA Paediatric Entrustment Scale v11.1, the
  ladder this EPA is assessed on."; Issued 2026-10-12. Evidence: CBD #13, DOPS #14, Mini-CEX #12 first, each with a
  tick and Read, then "Show the other 6 snapshot lines". Read swapped in the Mini-CEX (focus on its h3, `4.17-3`);
  "Back to PAED-001" restored level 5 and the rationale, focus back on Read. Stage pending decision aria-disabled with
  "Name at least one item of the evidence snapshot."; one tick: "Only one item is named. The College's rule is that an
  entrustment decision is never taken from a single form.", available. #12 then #13: "Pending entrustment decision
  staged." under the line, focused; line Staged, "Level 5 · issued 2026-10-12 · no expiry.", "Rests on 2 items of the
  snapshot: Mini-CEX (Paediatrics) #12; Case-Based Discussion (Paediatrics) #13.", Remove… (outline) in Action; caption
  and bar "1 staged".
Gap: new: the activity page's "Back to Lerato Molefe · 2026 S2" (/committee/reviews/1#snap-8) opens the review at its
  top with the focus on the h1, not at the snapshot line: the page draws after the navigation, so the fragment is never
  scrolled to.
  Fixed in `59bb72fd`: replayed on the republished app (2026-10-12), the link lands with #snap-8 in view and focused.

### Step 4.18 — An expiry before the issue date is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start staging PAED-010: choose its Stage, then level `4`, Issued on `D`, Expires on `D−1`, the rationale "Leads the
  ward round safely; one observation so far, to be confirmed at her final review." and both PAED-010 lines (the Direct
  Observation and the MSF record). Stage it.
Expect: PAED-010's open line lists its two lines first: the Direct Observation [#n] (Thandi Zulu, rated 5) and the MSF
  record #22 ("Not recorded"). Refused before anything is sent: "Not staged. An expiry date must be after the issue
  date." at the top of the line's form, with the focus there, and Expires on marked with "An expiry date must be after
  the issue date." (round 3 item 15). Nothing is staged, the form keeps what was typed, PAED-010 still reads Due at this
  sitting, and the bar still reads "1 staged".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): PAED-010's open line lists its two lines first: Direct
  Observation (Paediatrics) #17 · Thandi Zulu Rated 5, and Multi-Source Feedback (Paediatrics) #22 "Not recorded".
  Level 4, Issued 2026-10-12, Expires 2026-10-11, the rationale, both ticked: "Not staged. An expiry date must be after
  the issue date." at the top of the line's form, focused; Expires on aria-invalid, described by "An expiry date must
  be after the issue date.". Level, ticks and rationale kept; PAED-010 still "Due at this sitting"; bar "1 staged".
Gap: none

### Step 4.19 — Dr Zulu stages PAED-010 with an expiry, and PAED-012
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Correct PAED-010's Expires on to `D+20` and stage it. Then stage PAED-012 at `5` with no expiry, the rationale
  "Breaks difficult news clearly and kindly; consistent across settings." and its two WBA lines. Remove the PAED-012
  decision, confirming in its dialog, then stage it again the same way.
Expect: Both are staged, each with "Pending entrustment decision staged." under its line:
  - PAED-010 reads "Level 4 · issued `D` · expires `D+20`.";
  - PAED-012 reads "Level 5 · issued `D` · no expiry." on Mini-CEX (Paediatrics) #15 and Case-Based Discussion
    (Paediatrics) #16 (its Evidence 3 items).
  The bar reads "3 staged".
  - **Remove…** (an outline button, round 3 item 17) is named "Remove the staged decision on PAED-012". It opens a
    dialog titled "Remove the staged decision on PAED-012?", reading "Its level 5, its rationale and the 2 snapshot
    items it rests on are discarded. PAED-012 is due at this sitting again.", with the focus on its safe button "Keep
    it" (round 3 item 22), which would close it and change nothing. "Remove the decision" answers "Pending entrustment
    decision removed." under the line, PAED-012's line reads Due at this sitting with Stage, and the bar reads "2
    staged".
  - **Staged again,** it reads Staged, and the bar reads "3 staged".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Expires 2026-11-01 (D+20): staged, PAED-010 "Level 4 · issued
  2026-10-12 · expires 2026-11-01." on #17 and MSF #22. PAED-012 at 5 on Mini-CEX #15 and CBD #16: "Pending entrustment
  decision staged.", "Level 5 · issued 2026-10-12 · no expiry.", Evidence 3 items; bar "3 staged". Remove… named
  "Remove the staged decision on PAED-012" opened the dialog "Remove the staged decision on PAED-012?", "Its level 5,
  its rationale and the 2 snapshot items it rests on are discarded. PAED-012 is due at this sitting again.", focus on
  "Keep it" (`4.19-2`). "Remove the decision": "Pending entrustment decision removed." under the line, focused;
  PAED-012 "Due at this sitting" with Stage; bar "2 staged". Staged again: Staged, bar "3 staged".
Gap: none

### Step 4.20 — A second decision on PAED-001 is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: In "Stage a decision on an EPA not on this agenda", at the agenda's foot, choose PAED-001 from the EPA select, then
  fill in level `4`, a rationale and one line, DOPS (Paediatrics) #14 (rated 4, Mohammed Patel). Stage it.
Expect: Refused: "Not staged. An entrustment decision on PAED-001 is already staged at this review. Remove it to stage
  another." at the top of that form, with the focus there. PAED-001's line still holds exactly one decision, "Level 5
  · issued `D` · no expiry.", and the bar still reads "3 staged".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Foot form "Stage a decision on an EPA not on this agenda": EPA
  PAED-001, level 4, a rationale and DOPS #14: "Not staged. An entrustment decision on PAED-001 is already staged at
  this review. Remove it to stage another." at the top of that form, focused. PAED-001 still one decision, "Level 5 ·
  issued 2026-10-12 · no expiry."; bar "3 staged".
Gap: none

There is no Step 4.21: it deferred the other closing lines, and there is no deferral (D53, T361). The twelve lines
left unstaged are recorded as not decided at Step 4.25. The numbers after it are kept, so the captures and the flow
briefs that cite them still match.

### Step 4.22 — Dr Naidoo reads the sitting without the chair's controls
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's review while it is in progress, and press Refresh in its sitting bar.
Expect: Dr Naidoo sees everything the chair sees:
  - the snapshot;
  - the agenda with its Staged and "Due at this sitting" badges, and each staged decision and its evidence in its line;
  - the standing, MSF coverage, trajectory and "Before the committee decides" sections.
  Dr Naidoo is offered none of the chair's controls (T213):
  - the agenda has four columns, with no Action column;
  - there is no Stage, no Remove…, no foot form and no decision form.
  The sitting bar is a reader's, static under the title (E8): In progress, "3 staged", the sentence "Only the panel's
  chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision." (D53), and "As of hh:mm
  SAST" with Refresh. Refresh reads the review again in place, keeping the scroll and the focus, and says what changed:
  with nothing changed, "As of hh:mm SAST. No change." (round 3 item 21). Decision reads "No decision has been
  recorded yet."
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Naidoo (acting as Committee member) sees the sampling, agenda,
  Decision, standing, MSF, snapshot, trajectory and Appeals sections; PAED-001/010/012 Staged with their decisions and
  "Rests on ..." in their lines, the rest "Due at this sitting". Agenda EPA/Window/State/Evidence (no Action); no
  Stage, Remove…, foot form or decision form (no form control at all). His bar static (sitting-bar--reader): In
  progress, "3 staged", "Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the
  committee's decision.", "As of 13:56 SAST" and Refresh. Refresh: "As of 13:56 SAST. No change.", the focus kept on
  Refresh, scroll unchanged. "No decision has been recorded yet."
Gap: none

### Step 4.23 — Recording without a second member present is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Go to the decision in the sitting bar. Choose the category Satisfactory Progress and write a rationale. Tick
  nobody besides Dr Zulu under Present, and press Record decision….
Expect: Go to the decision moves the focus to the Decision heading. With only the chair ticked, Record decision… cannot
  be pressed to any effect: it is unavailable (aria-disabled, round 3 item 16), and the quorum line beside it reads
  "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one
  other." (T165, D46). Pressing it moves the focus to that line, opens no dialog and sends nothing, so the audit log
  gains no row (Step 4.38). The review stays In progress, with nothing recorded; the server's refusal stays only as
  the backstop.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Go to the decision moved the focus to the h2 "Decision".
  Satisfactory Progress and the rationale, only Thandi Zulu (chair) ticked (locked): Record decision… aria-disabled
  (.is-unavailable), the quorum line beside it "Only the chair is ticked. A committee decision needs at least two panel
  members present: the chair and at least one other." Pressed: the focus moved to that line, no dialog opened, nothing
  sent (Step 4.38 finds no FAILED row). Still In progress, bar "Present: the chair · one more member needed".
Gap: none

### Step 4.24 — Recording without a category is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Tick David Naidoo, Sarah Botha and John van Rensburg as present. Set Category back to "Select a category…" and
  press Record decision….
Expect: The quorum line reads "4 present: the chair and 3 others. The quorum is met." and the bar "4 present · quorum
  met"; Record decision… is available, with the hint "Recording fixes the 3 staged decisions, and records the 12 EPAs
  not staged as not decided." beside it. Pressed, it is refused before any dialog (T324): "Not recorded. This review
  decides the trainee's progression, so its decision records a progression category. Choose one." at the top of the
  Decision section, with the focus there, and Category marked "Choose a progression category." (T131 slice 5). Nothing
  is sent and nothing is recorded.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Naidoo, Botha and van Rensburg ticked: quorum line "4 present:
  the chair and 3 others. The quorum is met.", bar "4 present · quorum met"; Record decision… available with "Recording
  fixes the 3 staged decisions, and records the 12 EPAs not staged as not decided." beside it. Category back to "Select
  a category…" and pressed: no dialog; "Not recorded. This review decides the trainee's progression, so its decision
  records a progression category. Choose one." at the top of the Decision section, focused; Category aria-invalid with
  "Choose a progression category." Nothing recorded, In progress.
Gap: none (T324 no longer occurs: the category refusal comes before any dialog and nothing is sent)

### Step 4.25 — Dr Zulu records Dr Molefe's decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress, with the rationale "On track for completion; three EPAs entrusted, the rest to be
  decided at her final review." Write "To be decided at her final review." in About the EPAs not decided. Keep all
  four present, press Record decision…, and confirm.
Expect:
  - **The dialog** is titled "Record Satisfactory Progress for Lerato Molefe?" and reads "Fixes 3 staged decisions:
    PAED-001 at 5, PAED-010 at 4 (expires `D+20`), PAED-012 at 5.", "Records 12 EPAs as not decided: PAED-002,
    PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-008, PAED-009, PAED-011, PAED-013, PAED-014 and PAED-015."
    (E1), "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)." and "Afterwards only
    a decision that no longer fits the trainee's curriculum can be removed." The focus is on its safe button, "Not yet"
    (round 3 item 22); the confirm is "Record decision".
  - **Recorded:** "Decision recorded." in the Decision section, which now sits above the agenda, with the focus there.
    The bar reads Decided, "Satisfactory Progress · 3 decisions to issue" and Ratify….
  - **The decision:** headed "Satisfactory Progress", with the rationale, "About the EPAs not decided: To be decided at
    her final review." and a "Present:" line. It opens with "Thandi Zulu (chair)", ends with "John van Rensburg
    (external)", and names David Naidoo and Sarah Botha between them.
  - **Agenda:** "Fixed when the committee's decision was recorded: ratifying issues exactly these." above it. The three
    staged lines keep their decisions, with no Remove…; the other twelve, due and optional alike, read "Not decided at
    this sitting" with "To be decided at her final review." (E1). No Action column.
  - **No chair-rated warning:** five assessors rated her lines, so "The chair rated all of this evidence" is not shown
    (D46 warns of that case and refuses nothing).
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Dialog "Record Satisfactory Progress for Lerato Molefe?":
  "Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires 2026-11-01), PAED-012 at 5.", "Records 12 EPAs as
  not decided: PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-008, PAED-009, PAED-011, PAED-013,
  PAED-014 and PAED-015.", "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external).",
  "Afterwards only a decision that no longer fits the trainee's curriculum can be removed."; focus on "Not yet",
  confirm "Record decision" (`4.25-2`). Then "Decision recorded." in the Decision section, now above the agenda,
  focused. Bar Decided, "Satisfactory Progress · 3 decisions to issue", Ratify…. Decision "Satisfactory Progress", the
  rationale, "About the EPAs not decided: To be decided at her final review.", "Present: Thandi Zulu (chair), David
  Naidoo, Sarah Botha, John van Rensburg (external)". Agenda "Fixed when the committee's decision was recorded:
  ratifying issues exactly these."; the three staged lines keep their decisions, no Remove…; the other twelve
  (008/009/013 too) "Not decided at this sitting" with "To be decided at her final review."; no Action column. No "The
  chair rated all of this evidence". T356 decided: at 1280 and 768 px the bar sticks at 56-110 / 56-142 px, Sign out
  (14-42) never covered.
Gap: none

### Step 4.26 — Prof Mbatha reads the decided review, and is not offered Ratify
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Between the recording and the ratifying, open Dr Molefe's review from Committee reviews.
Expect: The list reads Decided, Satisfactory Progress, for Dr Molefe's review. The review shows the decision with its
  "Present:" line above the agenda, and the three staged decisions in their lines under "Fixed when the committee's
  decision was recorded …". It offers no Ratify…, no Remove… and no Action column, and the subtitle has no "Lerato
  Molefe's progress": that link is for the committee member, the coordinator and the speciality and sub-speciality
  admins (round 3 item 19). The sitting bar is a reader's, static: Decided, "Satisfactory Progress · 3 decisions to
  issue", "Only the panel's chair, Thandi Zulu, can ratify the committee's decision.", and "As of hh:mm SAST" with
  Refresh. Only the chair ratifies, with no administrator's bypass (D46).
Actual (2026-10-12, T361 replay, wombat_scenario_t361): List row: Molefe, Decided, Satisfactory Progress. Review #1:
  subtitle "Annual progression review · Paed Annual Review Panel · Training year 4" with no "Lerato Molefe's progress";
  decision with its "Present:" line above the agenda; the three under "Fixed when the committee's decision was
  recorded: ratifying issues exactly these."; no Ratify…, no Remove…, agenda EPA/Window/State/Evidence with no Action.
  Her bar a reader's, static: Decided, "Satisfactory Progress · 3 decisions to issue", "Only the panel's chair, Thandi
  Zulu, can ratify the committee's decision.", "As of 13:58 SAST", Refresh (her only control).
Gap: none

### Step 4.27 — Dr Zulu ratifies, and the STARs are issued
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Ratify… in the sitting bar, read the dialog, and confirm.
Expect:
  - **The dialog** is titled "Ratify and issue 3 STARs to Lerato Molefe?". It lists "PAED-001 at 5, issued `D`",
    "PAED-010 at 4, issued `D`, expires `D+20`" and "PAED-012 at 5, issued `D`", then "PAED-002 and 11 others were not
    decided at this sitting." (E1) and "Ratifying cannot be undone: nothing on this review can change afterwards." It
    says nothing about mail (T320). The focus is on "Not yet"; the confirm is "Ratify and issue 3 STARs".
  - **Ratified:** "Decision ratified." under the bar, with the focus there. The bar reads Ratified and "Satisfactory
    Progress · 3 STARs issued", and Ratify… is gone.
  - **Agenda:** "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one." above it (T212).
    - PAED-001, 010 and 012 read Decided, each with "STAR #n.";
    - the other twelve, due and optional alike, read "Not decided at this sitting" with "To be decided at her final
      review." (E1).
  - **Standing:** "2 at or above · 1 below · 12 with no decision, of 15 EPAs". PAED-001 and 012 at `5` meet year 4's
    target of `5`, and PAED-010 at `4` is below it. The exit rule reads "2 of 15 EPAs at their exit level by STAR
    decision".
  - **What ratifying locks:** nothing on the review can now be staged, removed or recorded.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Dialog "Ratify and issue 3 STARs to Lerato Molefe?": "PAED-001
  at 5, issued 2026-10-12", "PAED-010 at 4, issued 2026-10-12, expires 2026-11-01", "PAED-012 at 5, issued 2026-10-12",
  "PAED-002 and 11 others were not decided at this sitting.", "Ratifying cannot be undone: nothing on this review can
  change afterwards."; nothing about mail; focus "Not yet"; confirm "Ratify and issue 3 STARs" (`4.27-2`). "Decision
  ratified." under the bar, focused; bar Ratified, "Satisfactory Progress · 3 STARs issued", no control left on the
  page (no form, no button). "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda
  names each one." PAED-001 "Decided STAR #1.", PAED-010 "STAR #2.", PAED-012 "STAR #3."; the other twelve "Not decided
  at this sitting" with "To be decided at her final review.". "2 at or above · 1 below · 12 with no decision, of 15
  EPAs"; "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)". The log's Stub email
  count stayed 30.
Gap: [F-4.27a, T320] (still) Ratifying issues three STARs and tells Dr Molefe nothing: no mail is written.

## Phase 4.E — The other four sittings

### Step 4.28 — Dr Zulu stages two decisions for Dr Dlamini
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Dlamini's review and start it through its dialog. Stage, each on two finished PAED-001 lines (Dr Naidoo's
  Mini-CEX and Dr Botha's CCA):
  - PAED-001 at `4`, with the rationale "Manages the acutely ill child with the senior available on request.";
  - PAED-002 at `3b`, with the rationale "Manages common presentations with decisions checked by the senior on call."
  Defer nothing: there is no deferral (D53).
Expect:
  - **Start:** the dialog counts "the 4 items observed from `<Y>`-01-01 to `<Y>`-12-31 (4 activities)"; then "Review
    started." Dr Dlamini's snapshot freezes her four Act 3 WBAs: three under PAED-001 and one under PAED-004.
  - **Staging PAED-002:** Dr Dlamini has no PAED-002 line, so PAED-002's open line lists none of its own first, and the
    picker says "None of the named items is about PAED-002." It still stages, since D38 lets any line of the snapshot
    ground a decision; the staged line repeats the hint.
  - **The agenda:** PAED-001 and 002 read Staged; the other ten closing lines stay "Due at this sitting" and PAED-008,
    009 and 013 "As opportunity allows"; the bar reads "2 staged".
  - **Record decision…** is held back only by the quorum: it becomes available once a second member is ticked (Step
    4.29).
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Review #2: dialog counts "the 4 items observed from 2026-01-01
  to 2026-12-31 (4 activities)"; "Review started.". Snapshot PAED-001 CBD #18 Zulu 3b, CCA #19 Botha 4, Mini-CEX #1
  Naidoo 4 and PAED-004 Mini-CEX #20 Khumalo 4, all Completed. PAED-001 at 4 and PAED-002 at 3b, each on Mini-CEX #1
  and CCA #19. PAED-002's open line lists no line of its own (the snapshot's lines grouped by EPA, shown at once); the
  picker said "None of the named items is about PAED-002." and staged; the staged line repeats it. 001/002 Staged, the
  other ten closing lines "Due at this sitting", 008/009/013 "As opportunity allows"; bar "2 staged". Record decision…
  aria-disabled only on the quorum line ("Only the chair is ticked. ...").
Gap: none

### Step 4.29 — Dr Zulu records and ratifies Dr Dlamini's decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress with a rationale, write "Not at a decision point this year." in About the EPAs not
  decided, tick David Naidoo and Sarah Botha as present, and record, confirming in the dialog. Then ratify, confirming in
  its dialog.
Expect:
  - **The Record dialog** reads "Fixes 2 staged decisions: PAED-001 at 4, PAED-002 at 3b." and "Records 13 EPAs as not
    decided: PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-008, PAED-009, PAED-010, PAED-011, PAED-012,
    PAED-013, PAED-014 and PAED-015." (E1).
  - **Recorded:** "Decision recorded.", then, after the Ratify dialog ("Ratify and issue 2 STARs to Anele Dlamini?",
    "PAED-003 and 12 others were not decided at this sitting."), "Decision ratified.". The state reads Ratified.
  - **Present:** a "Present:" line naming Thandi Zulu (chair), David Naidoo and Sarah Botha.
  - **The agenda:** PAED-001 and 002 read Decided with their STAR numbers; the other thirteen read "Not decided at this
    sitting" with "Not at a decision point this year."
  - **Standing:** "1 at or above · 1 below · 13 with no decision, of 15 EPAs". PAED-001 at `4` meets year 3's target of
    `4`, and PAED-002 at `3b` is below it.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Naidoo and Botha ticked: "3 present: the chair and 2 others.
  The quorum is met.". Record dialog "Fixes 2 staged decisions: PAED-001 at 4, PAED-002 at 3b." and "Records 13 EPAs as
  not decided: PAED-003, ... PAED-013, PAED-014 and PAED-015."; "Decision recorded."; Ratify dialog "Ratify and issue 2
  STARs to Anele Dlamini?", "PAED-003 and 12 others were not decided at this sitting."; "Decision ratified.", Ratified.
  "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha". PAED-001 "Decided STAR #4.", PAED-002 "Decided STAR #5.";
  the other thirteen "Not decided at this sitting" with "Not at a decision point this year.". "1 at or above · 1 below
  · 13 with no decision, of 15 EPAs": PAED-001 4 vs target 4, PAED-002 3b vs 4.
Gap: none

### Step 4.30 — Dr du Plessis's sitting: progress with an observation, no STAR
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr du Plessis's review and start it through its dialog. Stage nothing. Record Satisfactory with Observations,
  confirming in the dialog:
  - the rationale "Teaching logs are sound; no observed clinical assessment completed yet.";
  - About the EPAs not decided: "Not at a decision point this year.";
  - the condition "At least two Mini-CEX before the next sitting.";
  - David Naidoo present.
  Then ratify, confirming in its dialog.
Expect:
  - **The snapshot** holds his two teaching sessions logged this year, his CBD still Requested, his cancelled DOPS and
    his portfolio review still Awaiting review, each with its state (T138). The session logged for `J−1y−56d` is outside
    the window and not in it.
  - **Rating trajectory:** "No rating by a named assessor to chart in this review's window."
  - **Record decision…** is available once David Naidoo is ticked, with nothing staged (D53): its dialog reads "Fixes no
    staged decision." and "Records 15 EPAs as not decided: PAED-001, PAED-002, … and PAED-015."
  - **Recorded:** the decision reads "Satisfactory with Observations", with "About the EPAs not decided: Not at a
    decision point this year.", "Conditions: At least two Mini-CEX before the next sitting." and "Present: Thandi Zulu
    (chair), David Naidoo".
  - **Ratified:** the Ratify dialog is titled "Ratify Pieter du Plessis's review?", reads "PAED-001 and 14 others were
    not decided at this sitting." and confirms with "Ratify the review". The state reads Ratified, the bar
    "Satisfactory with Observations · no STAR issued", and the agenda says "Nothing was staged when the review was
    ratified, so ratifying it issued no STAR." All fifteen lines read "Not decided at this sitting" with "Not at a
    decision point this year."
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Review #3 started (dialog "the 5 items ... (5 activities)").
  Snapshot: PAED-002 CBD #9 Fatima Khumalo Requested and DOPS #8 Cancelled; PAED-004 Teaching Session Log #6 Logged;
  PAED-015 Log #5 Logged and Portfolio and Logbook Review #10 Awaiting review; #7 (J−1y−56d) not in it. "No rating by a
  named assessor to chart in this review's window." David Naidoo ticked: Record available, hint "Recording records the
  15 EPAs not staged as not decided."; dialog "Fixes no staged decision." and "Records 15 EPAs as not decided:
  PAED-001, PAED-002, ... and PAED-015.". Decision "Satisfactory with Observations", "About the EPAs not decided: Not
  at a decision point this year.", "Conditions: At least two Mini-CEX before the next sitting.", "Present: Thandi Zulu
  (chair), David Naidoo". Ratify dialog "Ratify Pieter du Plessis's review?", "PAED-001 and 14 others were not decided
  at this sitting.", confirm "Ratify the review". Ratified; bar "Satisfactory with Observations · no STAR issued";
  "Nothing was staged when the review was ratified, so ratifying it issued no STAR."; all fifteen "Not decided at this
  sitting" with "Not at a decision point this year."
Gap: none

### Step 4.31 — Dr Mahlangu's sitting: additional training
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review and start it through its dialog, and read the snapshot. Stage nothing. Record Inadequate
  Progress — Additional Training, confirming in the dialog:
  - the rationale "One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six
    months.";
  - About the EPAs not decided: "Not enough observed evidence yet this year.";
  - Sarah Botha and John van Rensburg present.
  Then ratify, confirming in its dialog.
Expect:
  - **The snapshot** lists Dr Mahlangu's DOPS under PAED-002 and Mini-CEX under PAED-004, both Completed at 3a.
  - **Sampling concentration warnings** name PAED-002 and PAED-004, one rating each.
  - **Ratified:** the state reads Ratified. All fifteen lines read "Not decided at this sitting" with "Not enough
    observed evidence yet this year." (D53, E1).
  - **Present:** "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Review #4 started ("the 2 items ... (2 activities)"). Snapshot
  PAED-002 DOPS #11 Mohammed Patel 3a and PAED-004 Mini-CEX #21 Thandi Zulu 3a, both Completed. Sampling names PAED-002
  and PAED-004, "1 rating · 1 assessor · 1 source". Record dialog "Fixes no staged decision.", "Records 15 EPAs as not
  decided: ...". "Inadequate Progress — Additional Training", "About the EPAs not decided: Not enough observed evidence
  yet this year.", "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)"; "Decision ratified.",
  Ratified, "no STAR issued"; all fifteen "Not decided at this sitting" with "Not enough observed evidence yet this
  year."
Gap: none

### Step 4.32 — Dr Ndlovu's sitting: the outcome deferred
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Ndlovu's review and start it through its dialog, and read the snapshot. Stage nothing. Record Outcome
  Deferred, the progression category (unrelated to D53's end of agenda deferral), confirming in the dialog:
  - the rationale "One completed Mini-CEX and one reflection in his first semester; to be judged at the next
    sitting.";
  - About the EPAs not decided: "Too early in the programme to decide.";
  - Sarah Botha present.
  Then ratify, confirming in its dialog.
Expect:
  - **The agenda, once ratified:** all fifteen lines read "Not decided at this sitting" with "Too early in the programme
    to decide."
  - **Evidence snapshot:** his three Act 3 activities, each with its state: the Mini-CEX Dr Khumalo declined and the
    one Dr Botha completed at 3a, both under PAED-002, and the reflective exercise under PAED-001, discussed. A declined
    request is evidence too (T138).
  - **Trajectory:** PAED-002 alone, from Dr Botha's rating.
  - **Sampling:** "Before the committee decides" names PAED-002, one rating from one assessor.
  - **Ratified:** the review reads Ratified, with "Outcome Deferred".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Review #5 started ("the 3 items ... (3 activities)"). Snapshot:
  PAED-001 Reflective Exercise #4 Unrated Discussed; PAED-002 Mini-CEX #2 Fatima Khumalo Declined and #3 Sarah Botha 3a
  Completed. Trajectory PAED-002 alone, "Sipho Ndlovu · 1 rating in the review window, 2026-01-01 to 2026-12-31, from
  Sarah Botha. At the minimum." Sampling names PAED-002, "1 rating · 1 assessor · 1 source". "Outcome Deferred", "About
  the EPAs not decided: Too early in the programme to decide.", "Present: Thandi Zulu (chair), Sarah Botha"; Ratified,
  "Outcome Deferred · no STAR issued"; all fifteen "Not decided at this sitting" with "Too early in the programme to
  decide."
Gap: none

## Phase 4.F — What the programme sees after the sitting

### Step 4.33 — Mr Smit reads the ratified schedule
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Read Committee reviews, then open Dr Molefe's review.
Expect: All five read Ratified. The Decision column reads:
  - Satisfactory Progress for Molefe and Dlamini;
  - Satisfactory with Observations for du Plessis;
  - Inadequate Progress — Additional Training for Mahlangu;
  - Outcome Deferred for Ndlovu.
  No state or category prints an enum name (T250). Molefe's review shows its decision, agenda and STAR numbers, and
  offers Smit no action.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): All five Ratified; Decision Satisfactory Progress (Molefe,
  Dlamini), Satisfactory with Observations (du Plessis), Inadequate Progress — Additional Training (Mahlangu), Outcome
  Deferred (Ndlovu); no enum name. Review #1: bar Ratified, "Satisfactory Progress · 3 STARs issued", no Refresh;
  "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)"; STAR #1, #2, #3 in the
  agenda; no control offered to Smit (only the closed dialogs' buttons in the DOM).
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
    - Not decided on 55 rows, its badge amber, "Not decided at review #N. A later sitting for `<P>` plans it again.",
      or `<Y>` for an annual EPA (T361, D53, E3: each was due at that sitting);
    - As opportunity allows on 15: an optional EPA left unstaged is recorded as not decided at its review, but its
      window `<Y>` has not ended, so it still reads "Decided as opportunity allows: optional, never missed." (E3).
  - **By EPA:** PAED-001 reads Due 5, Decided 2 and Not decided 3.
  - **Actions:** a Not decided row offers Open review and Schedule, since no open review holds its seat now.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Default: "70 of 75 decisions due in 2026 S2 shown." (55 Not
  decided, 15 As opportunity allows). Every status, 75 of 75: Decided 5 ("STAR #1/#2/#3, issued at review #1." for
  Molefe 001/010/012, "STAR #4/#5, issued at review #2." for Dlamini 001/002); Not decided 55, badge amber
  (badge-accepted, the warning colours), "Not decided at review #N. A later sitting for 2026 S2 plans it again." or
  "... for 2026 plans it again."; As opportunity allows 15, "Decided as opportunity allows: optional, never missed.".
  By EPA columns Due, Decided, Scheduled, Not decided, To schedule, Missed, Optional; PAED-001 Due 5, Decided 2, Not
  decided 3. Every Not decided row offers Open review and Schedule.
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
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Home's "Entrustment decisions" link ->
  /admin/entrustment-decisions (the menu marks it current): five rows, all Active, issued 2026-10-12: Molefe PAED-001
  5, PAED-010 4 expiring 2026-11-01, PAED-012 5; Dlamini PAED-001 4, PAED-002 3b; Expires "—" where none. PAED-010
  certificate (star-certificate-PAED-010-2-....pdf, 97,695 bytes): Kgosi Kgari Teaching Hospital, "Statement of Awarded
  Responsibility", Lerato Molefe, PAED-010, Authorised level 4, Issued 12 October 2026, Expires 1 November 2026, Status
  Active, the rationale, #17 and MSF #22, Panel Paed Annual Review Panel, Committee review #1, Chair Thandi Zulu.
  Trainee "Molefe" + Apply filters leaves her three rows.
Gap: none

### Step 4.36 — Dr Mokoena revokes one of Dr Dlamini's STARs
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /admin/entrustment-decisions
Do: Open Entrustment decisions from the menu. Choose Revoke on Dr Dlamini's PAED-002 decision. Give the reason "Staged
  on evidence about PAED-001, not PAED-002; to be decided again." and confirm the revocation.
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail: a list her menu offers (T358, R2-Menus m5;
    as Step A.7.7).
  - **The list:** the same five rows as Step 4.35, since all five registrars are in her speciality.
  - **The dialog** names PAED-002 and Anele Dlamini, and says "Revocation is immediate and irreversible. The trainee is
    notified." Confirm revocation is enabled once a reason is typed.
  - **After confirming:** "Entrustment decision for PAED-002 revoked." The row reads Revoked, with no Revoke button.
  - **The trainee is notified,** as the dialog promises: the application log holds a mail to Dr Dlamini about the
    revocation.
Note: Until T358 the page was linked from no nav item or dashboard card for her role, and was reached by typing its
  address. T260 (open): Confirm stays disabled until the reason box loses focus.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): From the menu's Entrustment decisions: lit, no breadcrumb
  trail; the same five rows. "Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-12" opened an inline "Revoke
  entrustment decision" section (no dialog role): "... PAED-002 — Managing common paediatric presentations held by
  Anele Dlamini. Revocation is immediate and irreversible. The trainee is notified." Confirm revocation stayed disabled
  while the reason was typed and enabled when the box lost focus. "Entrustment decision for PAED-002 revoked."; row
  Revoked, only Download. The Stub email count stayed 30.
Gap: [F-4.36a, T319] (still) "The trainee is notified." and nobody is: no mail to Dr Dlamini. T260 (still, its
  entrustment-decisions half, as the Note says): Confirm revocation enables only when the reason box loses focus.

### Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Open Entrustment decisions from the menu. Filter Status to Revoked and apply, then to Active and apply. Download
  the revoked certificate. Then open Decisions due with "Every status".
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail (T358, R2-Menus m5).
  - **Filtered:** Revoked leaves Dlamini's PAED-002, and Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", the date, who revoked it (Dr Mokoena, by name,
    T142) and the reason.
  - **Decisions due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." It offers Schedule.
  - **By EPA:** PAED-002 reads Decided 0, Not decided 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."
Actual (2026-10-12, T361 replay, wombat_scenario_t361): From the menu's Entrustment decisions: lit, no trail. Status
  Revoked + Apply filters leaves Dlamini's PAED-002; Active leaves four. The revoked certificate
  (star-certificate-PAED-002-5-....pdf): Status Revoked, "REVOKED", "Revoked on 12 October 2026 by b6965f65-..." (a
  user id) and the reason. Decisions due: default "71 of 75 decisions due in 2026 S2 shown."; Dlamini's PAED-002
  "Revoked: re-decide", "STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again.", Open review
  and Schedule. By EPA PAED-002: Due 5, Decided 0, Not decided 4, To schedule 1.
Gap: [F-4.37a, T324] (still) The revoked certificate names the revoker by raw user id, not "Refilwe Mokoena".

### Step 4.38 — Prof Mbatha reads the sitting in the audit log
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Filter Action to RecordCommitteeDecisionCommand. Open one successful entry. Then filter to
  RevokeEntrustmentDecisionCommand.
Expect:
  - **No failed entry:** Steps 4.23 and 4.24 are refused on the page before anything is sent (round 3 item 16, T324), so
    no FAILED row is written; the handler's quorum and category refusals stay as the backstop (T361).
  - **The successful entries:** five OK rows by Dr Zulu, one per review.
  - **An entry's detail:** the Event card and the Actor card only. The Payload card is an Administrator's (Step 3.56),
    so she reads no summary. The stored summary, checked with SQL, keeps ReviewId, Category and PresentUserIds in the
    clear and holds Rationale, Conditions and UndecidedNote as "[REDACTED]" (T101, T361).
  - **The revocation's entry:** its actor is Dr Mokoena. Its stored Reason is "[REDACTED]".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Audit log, Action RecordCommitteeDecisionCommand: five rows,
  all OK, by zulu@kgk.wombat.local, none FAILED (4.23 and 4.24 sent nothing). An entry's detail shows Event and Actor
  only, no Payload. SQL: each SummaryJson keeps category, reviewId and presentUserIds (4, 3, 2, 3, 2 ids) and holds
  rationale, conditions and undecidedNote as "[REDACTED]". RevokeEntrustmentDecisionCommand: one OK row by
  mokoena@kgk.wombat.local, stored reason "[REDACTED]".
Gap: none

## Phase 4.G — The registrars read their outcomes

### Step 4.39 — Dr Molefe reads her STARs and downloads a certificate
Role: Trainee — Dr Lerato Molefe
Route: / → /portfolio/authorisations
Do: From her Home's My authorisations card, Open My authorisations. Download the PAED-001 certificate.
Expect: Home's My authorisations card reads "2 at or above · 1 below · 12 with no decision, of 15 EPAs" and lists one
  STAR, "PAED-010 — Leading and operating within a clinical team", a link to its EPA's page, with "4, below training
  year 4's level of 5 · expires `D+20`" under it; its footer is Open My authorisations (T355). My authorisations holds
  three cards, each issued on `D` with the committee's rationale: PAED-001 and PAED-012 at Authorised level `5`, and
  PAED-010 at `4`.
  - PAED-001 and 012 read "No expiry".
  - PAED-010 reads the date `D+20`, with the badge "Expires in 20 days".
  Each Download certificate is named by its EPA. The PAED-001 certificate downloads with Status Active and its evidence
  summary.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Home's My authorisations card: "2 at or above · 1 below · 12
  with no decision, of 15 EPAs", one STAR "PAED-010 — Leading and operating within a clinical team" linking
  /portfolio/progress/11, "4, below training year 4's level of 5 · expires 2026-11-01" under it, footer "Open My
  authorisations" (`4.39-3`). It opens /portfolio/authorisations: three cards issued 12 October 2026 with the
  rationale: PAED-001 5 "No expiry", PAED-010 4 "1 November 2026" with "Expires in 20 days", PAED-012 5 "No expiry".
  Downloads named "Download certificate for PAED-001/010/012". PAED-001 certificate
  (star-certificate-PAED-001-1-....pdf): Status Active, evidence Mini-CEX #12 and CBD #13. No horizontal scroll at 390
  px.
Gap: none

### Step 4.40 — Dr Molefe reads her progress against Annexure A
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/progress
Do: Open My progress and read its index's STAR column and "Entrustment against Annexure A".
Expect: The index's STAR column, headed "STAR against training year 4", reads "At or above" with 5 for PAED-001 and
  PAED-012, "Below" with "4 · expires `D+20`" for PAED-010, and "No decision" for the other twelve (T355). Entrustment
  against Annexure A says "You are in training year 4 on `D`." It then reads "2 at or above · 1 below · 12 with no
  decision, of 15 EPAs" and "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)". The
  table shows:
  - PAED-001 and 012 at `5`, each "At or above" the target of `5`, with its exit level Reached;
  - PAED-010 at `4`, "Below" the target of `5`, with its issue and expiry dates.
  Each EPA's name links to its page, and each Latest rating, "4 · Encounter <date>" and the like, is a link to its
  activity (T355). The exit rule says it gates nothing. The page reads the same table as the committee's (T166).
Actual (2026-10-12, T361 replay, wombat_scenario_t361): My progress (lit): index STAR column "STAR against training
  year 4": PAED-001 and PAED-012 "At or above 5", PAED-010 "Below 4 · expires 2026-11-01", the others "No decision".
  "You are in training year 4 on 2026-10-12." "2 at or above · 1 below · 12 with no decision, of 15 EPAs", "2 of 15
  EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)". PAED-001 5 "Issued 2026-10-12", At or
  above target 5, exit Reached; PAED-012 the same; PAED-010 4 "Issued 2026-10-12, expires 2026-11-01", Below target 5,
  Not yet. Names link /portfolio/progress/<EpaId>; Latest rating "4 · Encounter 2026-10-03" -> /activities/14,
  PAED-010's -> /activities/17, PAED-012's -> /activities/16. "For information only. ..."; the same table as the
  committee's. EPA pages: PAED-010 4, Below, Issued, Expires 2026-11-01, "5 · not yet"; PAED-001 5, At or above, "5 ·
  reached"; no horizontal scroll (`4.40-2`, `4.40-3`).
Gap: none

### Step 4.41 — Dr Molefe reads her review, and not the committee's page
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → /committee/reviews/{ReviewId:int} → /access-denied
Do: Open My committee reviews and view her review. Then type the committee's own address for the same review.
Expect:
  - **The list:** one row, with Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31", Type Annual progression, State Ratified and
    Decision Satisfactory Progress. Its View is named by the review.
  - **The detail** shows Sits for, Evidence window, Type and State. Its decision is headed "Satisfactory Progress", with
    the rationale and "Present when this decision was taken: …" naming all four.
  - **The agenda** has three columns:
    - Decided with "STAR #n." on PAED-001, 010 and 012;
    - "Not decided at this sitting" with the committee's sentence "To be decided at her final review." on the other
      twelve, due and optional alike: no reason, and no Deferred (T361, D53, E1).
  - **The committee's page** lands on access-denied.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): One row: Paed Annual Review Panel, "2026 S2 · 2026-01-01 to
  2026-12-31", Annual progression, Ratified, Satisfactory Progress; View named "View the 2026 S2 review before Paed
  Annual Review Panel". Detail: Sits for, Evidence window, Type "Annual progression review", State Ratified;
  "Satisfactory Progress", the rationale, "Present when this decision was taken: Thandi Zulu (chair), David Naidoo,
  Sarah Botha, John van Rensburg (external)". Agenda EPA/Window/Outcome: "Decided" "STAR #1./#2./#3." on 001/010/012;
  the other twelve, 008/009/013 too, "Not decided at this sitting" with "To be decided at her final review."; no
  Deferred, no reason label. An Appeal reason form follows. No horizontal scroll at 390 px. Typed /committee/reviews/1
  -> /access-denied, "Your role (Trainee) does not open this page."
Gap: none

### Step 4.42 — Dr Dlamini reads what stands after the revocation
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/authorisations → /committee/my-reviews
Do: From her Home's My authorisations card, Open My authorisations. Then open her review.
Expect: One card, PAED-001 at `4`: the revoked PAED-002 is no longer listed. Her review reads Ratified, Satisfactory
  Progress. The agenda still reads Decided with STAR numbers on PAED-001 and 002, because it records what the sitting
  did.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Home's Open My authorisations: one card, PAED-001 level 4, No
  expiry, with its rationale; PAED-002 not listed. My committee reviews: Ratified, Satisfactory Progress, "Present when
  this decision was taken: Thandi Zulu (chair), David Naidoo, Sarah Botha"; the agenda still reads PAED-001 "Decided
  STAR #4." and PAED-002 "Decided STAR #5."
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
    - the agenda shows all fifteen lines "Not decided at this sitting", each with "Not enough observed evidence yet
      this year." (T361, D53, E1);
    - an Appeal reason form is offered.
  - **After lodging:** "Appeal lodged." The state reads Under appeal, the form is gone and the agenda stays in view.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): "Inadequate Progress — Additional Training" with "Present when
  this decision was taken: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)"; all fifteen lines "Not
  decided at this sitting" with "Not enough observed evidence yet this year." (no Deferred); Appeal reason with Lodge
  appeal. "Appeal lodged."; "State: Under appeal", the form gone, the 15-line agenda kept. The list row above still
  reads Ratified. No mail (30).
Gap: [F-4.43a, T308] (still) After lodging, the list row still reads State Ratified while the detail below it reads
  Under appeal.

### Step 4.44 — Dr Naidoo sees who hears the appeal
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review.
Expect: The list shows her review Under appeal. The Appeals card lists the appeal, with the date lodged, its reason and
  "(Open)". It offers him no form. It says "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu,
  and its external member, John van Rensburg." (T213, T237)
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Acting as Committee member. The list shows Mahlangu's review
  Under appeal, Inadequate Progress — Additional Training. Appeals: "Only the appeal body can resolve the appeal: the
  panel's chair, Thandi Zulu, and its external member, John van Rensburg." and "2026-10-12 The single DOPS ... I ask
  for reconsideration. (Open)"; no form, no control (only the closed dialogs' buttons in the DOM).
Gap: none

### Step 4.45 — Dr van Rensburg, the external member, is offered the appeal
Role: CommitteeMember (external) — Dr John van Rensburg
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then Dr Mahlangu's review. Read the resolve form, and press Resolve appeal without choosing
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
Actual (2026-10-12, T361 replay, wombat_scenario_t361): "Acting as Committee member" (no other role). Committee reviews
  lists the five, Mahlangu's Under appeal. Outcome opens on "Select an outcome…", then "Dismissed: the decision stands"
  and "Remitted: the appeal body replaces the decision"; no Upheld; help "Dismissed leaves the committee's decision in
  force. Remitted replaces it ... Either closes the review, and nothing reopens it." Resolve appeal with none: "Choose
  an outcome.", aria-invalid, focus on the select. Remitted reveals Replacement category ("Select a category…"),
  Replacement rationale, Replacement conditions (optional) and Present, Thandi Zulu (chair) and John van Rensburg
  (external) ticked and locked. Left unresolved. With Remitted chosen the Outcome select is 292 px wide and its text
  304 px: it reads "Remitted: the appeal body replaces the" (`4.45-2`).
Gap: [F-4.45c, T323] (still) At 1280 px, once Remitted opens the second column, the Outcome select is too narrow and
  its choice, "Remitted: the appeal body replaces the decision", is clipped (`4.45-2`).

### Step 4.46 — A remit without a quorum is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: On Dr Mahlangu's review, choose Remitted, the replacement category Satisfactory with Observations and a
  rationale. Tick nobody besides herself as present, and resolve.
Expect: Resolve appeal is refused with "A committee decision needs at least two panel members present: the chair and
  at least one other." (T165: a remit records its own quorate sitting), in the Appeals section beside the form, with
  the focus on it (T361, round 3 item 15). The review stays Under appeal.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Outcome opened on "Select an outcome…". Remitted, Satisfactory
  with Observations and 4.47's rationale, only Thandi Zulu (chair) ticked and locked (van Rensburg is unticked for
  her): refused "A committee decision needs at least two panel members present: the chair and at least one other." at
  the top of the Appeals section (y 7,695, its heading at 7,656), beside the form, with the focus on it (`4.46-1`). Bar
  still Under appeal; outcome, category and rationale kept.
Gap: none (F-4.46a, T299 no longer occurs: the remit's refusal sits in the Appeals section beside the form, focused,
  not at the head of the page)

### Step 4.47 — Dr Zulu remits the decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Keep Remitted, Satisfactory with Observations, and the rationale "The stalled Mini-CEX was the assessor's delay;
  progress is adequate, with observed assessments to follow." Write the Replacement conditions "Two observed Mini-CEX
  and one DOPS before the next review." Tick Sarah Botha as present, and resolve the appeal.
Expect: "Appeal resolved." The state reads Closed (T250).
  - **Decision:** the replacement heads it, "Satisfactory with Observations" with "About the EPAs not decided: Not
    enough observed evidence yet this year." (carried from the first decision, T361, E5), "Conditions: Two observed
    Mini-CEX and one DOPS before the next review." (T307) and "Present: Thandi Zulu (chair), Sarah Botha". The first
    decision stays below it, marked "Replaced on appeal by the decision above.".
  - **Appeals card:** the appeal reads "(Remitted)".
  - **No STAR changed:** the review issued none.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Conditions and Sarah Botha ticked: "Appeal resolved." in the
  Appeals section; bar Closed, "Satisfactory with Observations · no STAR issued". Decision: "Satisfactory with
  Observations", the rationale, "About the EPAs not decided: Not enough observed evidence yet this year.", "Conditions:
  Two observed Mini-CEX and one DOPS before the next review.", "Present: Thandi Zulu (chair), Sarah Botha"; below it
  "Inadequate Progress — Additional Training", "Replaced on appeal by the decision above.", with its first Present
  line. Appeals: "(Remitted)", no form. SQL: appeal 1 Outcome 3, resolved; decision 6 (category 2, the note carried,
  the conditions) supersedes 4; review State 6; STARs still 5.
Gap: none

### Step 4.48 — Dr Mahlangu reads the outcome
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: Open My committee reviews and view the review.
Expect: The row reads State Closed and Decision Satisfactory with Observations. The detail's current decision is the
  replacement, with "Present when this decision was taken: Thandi Zulu (chair), Sarah Botha". The agenda's fifteen lines
  still read "Not decided at this sitting" with "Not enough observed evidence yet this year.": the replacement carries
  the first decision's sentence (T361, E5). No appeal form is offered.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Row: State Closed, Decision Satisfactory with Observations.
  Detail: "State: Closed", "Satisfactory with Observations", the replacement rationale, "Present when this decision was
  taken: Thandi Zulu (chair), Sarah Botha"; all fifteen agenda lines "Not decided at this sitting" with "Not enough
  observed evidence yet this year."; no appeal form. Nothing of her appeal, its outcome, the first decision or the
  Conditions.
Gap: [F-4.48a, T308] (still) The trainee's review shows no trace of her appeal, its outcome (Remitted), the decision it
  replaced or the replacement's conditions, though the subtitle reads "Ratified decisions, appeal status, and final
  outcomes."

## Phase 4.I — A formative check-in

### Step 4.49 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the panel for `<P>`, Scheduled on `D`, with Formative only ticked.
Expect:
  - **Accepted:** a formative review takes no binding seat for the period (T131 slice 4).
  - **The preview:** "A formative review carries no agenda: it decides no EPA."
  - **The review:** its h1 reads "Sipho Ndlovu · `<P>`" and its subtitle "Formative check-in · Paed Annual Review
    Panel · Training year 1" (T309 item 2). The sitting bar reads Scheduled and "Formative: an interim check-in, no
    binding decision · evidence `<Y>`-01-01 to `<Y>`-12-31" (round 3 item 28), with Start review… for Smit. It has no
    Decision, Agenda or Appeals section: only the evidence snapshot, the standing, MSF coverage and the trajectories.
  - **The list:** the review shows Mode Formative and Decision "—".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Panel, Sipho Ndlovu, 2026 S2, Scheduled on 2026-10-12,
  Formative only ticked: the preview reads "A formative review carries no agenda: it decides no EPA."; the Review type
  select stays offered (Annual progression review / Pre-graduation review). Accepted beside his binding review: review
  #6, h1 "Sipho Ndlovu · 2026 S2", subtitle "Formative check-in · Paed Annual Review Panel · Training year 1". Bar
  Scheduled, "Formative: an interim check-in, no binding decision · evidence 2026-01-01 to 2026-12-31", "As of 14:07
  SAST", Refresh and Start review… for Smit. Sections: Before the committee decides, Evidence snapshot, standing, MSF,
  trajectory; no Decision, Agenda or Appeals. List row: Type Formative, Mode Formative, Scheduled, Decision "—"; Open
  "Open the 2026 S2 formative review of Sipho Ndlovu ...".
Gap: [F-4.49a, T309] (still, item 3) With Formative only ticked the Review type select stays offered. (T309 item 2 no
  longer occurs: the check-in reads "Formative check-in" in the subtitle and "Formative" in the list.)

### Step 4.50 — Dr Zulu starts and closes the check-in
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the formative review through its dialog, read it, and close it through its dialog.
Expect:
  - **The type:** the subtitle reads "Formative check-in · Paed Annual Review Panel · Training year 1" (T309 item 2).
  - **Started:** "Review started." under the sitting bar, with the focus there. The bar reads In progress, "Formative: an
    interim check-in, no binding decision" and Close review…. The snapshot freezes the same three activities as Dr
    Ndlovu's annual review, and the standing and MSF coverage sections follow it. Close review… is offered to the chair
    alone.
  - **Closed:** the dialog is titled "Close Sipho Ndlovu's formative check-in?" and reads "It records no decision and
    issues no STAR. Nothing on it can change once it is closed.", with the focus on "Not yet"; its confirm is "Close
    review". Then "Formative review closed." with the focus on it; the bar reads Closed and "Formative check-in · no
    decision recorded", and nothing is offered.
  - **Other readers** (Dr Naidoo, Mr Smit) read "Only the panel's chair, Thandi Zulu, can close this review." in a
    static bar while it is in progress, with "As of hh:mm SAST" and Refresh, and no button to close it.
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Subtitle "Formative check-in · Paed Annual Review Panel ·
  Training year 1". Start dialog ("the 3 items ... (3 activities)"), then "Review started." under the bar, focused; bar
  In progress, "Formative: an interim check-in, no binding decision", Close review…. The snapshot freezes the same
  three activities as review #5 (#4 Discussed; #2 Declined, #3 3a Completed); standing ("In training year 1 on
  2026-10-12") and MSF follow it. Close review… offered to her alone: Naidoo and Smit read a static bar, "Only the
  panel's chair, Thandi Zulu, can close this review.", "As of 14:07 SAST", Refresh, no close button. Dialog "Close
  Sipho Ndlovu's formative check-in?", "It records no decision and issues no STAR. Nothing on it can change once it is
  closed.", focus "Not yet", confirm "Close review" (`4.50-4`). "Formative review closed.", focused; bar Closed,
  "Formative check-in · no decision recorded"; nothing offered. T356 in progress: at 1280 and 768 px the bar sticks at
  56-110 / 56-168 px, Sign out (14-42) never covered.
Gap: none

### Step 4.51 — Dr Ndlovu reads both reviews
Role: Trainee — Dr Sipho Ndlovu
Route: /committee/my-reviews
Do: Open My committee reviews.
Expect: Two rows for `<P>`. The annual review reads Annual progression, Ratified, Outcome Deferred. The check-in reads
  Type "Formative" (T309 item 2), State Closed, with Decision "No binding decision"; its detail's Type reads "Formative
  check-in".
Actual (2026-10-12, T361 replay, wombat_scenario_t361): Two rows for 2026 S2: the annual review "Annual progression,
  Ratified, Outcome Deferred" and the check-in "Formative, Closed, No binding decision" (View "View the 2026 S2
  formative review before Paed Annual Review Panel"); the check-in's detail "Type: Formative check-in", "State:
  Closed".
Gap: none (T309 item 2 no longer occurs: the check-in reads Formative)

## Outcome state

Replay check (2026-10-12, T361 replay, wombat_scenario_t361): match on all six queries (six reviews: four binding
  Ratified, Mahlangu's Closed, Ndlovu's formative Closed, stored with ReviewType 1; 75 agenda lines, 5 Decided and 70
  Not decided: molefe 3/12, dlamini 2/13, the other three 0/15; six decisions, each with a note, Molefe 4, Dlamini 3,
  du Plessis 2, Mahlangu 3 then 2, Ndlovu 2 present, decision 6 superseding 4; five STARs, Dlamini's PAED-002 Revoked,
  PAED-010 expiring 2026-11-01, 2 evidence links each; 0 pending, 1 remitted appeal; frozen lines Molefe 9, Dlamini 4,
  du Plessis 5, Mahlangu 2, Ndlovu 3 and 3). The act wrote no mail (the log's Stub email count stayed 30). Snapshot
  recovery/scenario-t361-post-act4.dump.

These steps add, before the panel `Paed Annual Review Panel`, for the period `<P>`:
- **Six reviews.** Five are binding Annual progression reviews: four Ratified, and Mahlangu's Closed after its appeal
  was remitted. The sixth is Ndlovu's formative check-in, Closed.
- **75 agenda lines.** 5 Decided and 70 Not decided: every line left unstaged, due and optional alike, is recorded as
  not decided when its review is ratified (T361, D53, E1). The five decisions taken at the sittings each carry a sentence
  about the EPAs not decided, and Mahlangu's replacement carries her first decision's (E5).
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

-- 2. Agenda lines by trainee and state (3 Decided, 4 Not decided; there is no state 2 since D53). Expect 75 lines:
--    molefe 3/12, dlamini 2/13, duplessis 0/15, mahlangu 0/15, ndlovu 0/15 (Decided/Not decided).
SELECT u."Email", l."State", count(*) AS lines
FROM "CommitteeAgendaLines" l
JOIN "CommitteeReviews" r ON r."Id" = l."ReviewId"
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
WHERE u."Email" LIKE '%@kgk.wombat.local'
GROUP BY u."Email", l."State"
ORDER BY u."Email", l."State";

-- 3. Decisions and who sat. Category 1 = Satisfactory Progress, 2 = with Observations, 3 = Additional Training,
--    6 = Outcome Deferred. Expect six rows, each with a note (T361, D53; the remit's carried, E5):
--    molefe 1 (4 present), dlamini 1 (3), duplessis 2 (2), mahlangu 3 (3), mahlangu 2 (2, superseding the other),
--    ndlovu 6 (2).
SELECT u."Email", d."Id", d."Category", d."SupersedesDecisionId", d."UndecidedNote" IS NOT NULL AS note,
       count(a."Id") AS present
FROM "CommitteeDecisions" d
JOIN "CommitteeReviews" r ON r."Id" = d."ReviewId"
JOIN "AspNetUsers" u ON u."Id" = r."TraineeUserId"
LEFT JOIN "CommitteeDecisionAttendees" a ON a."DecisionId" = d."Id"
WHERE u."Email" LIKE '%@kgk.wombat.local'
GROUP BY u."Email", d."Id", d."Category", d."SupersedesDecisionId", d."UndecidedNote"
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
- **Her nine closing EPAs left unstaged read Not decided on Decisions due** (E3); PAED-008, 009 and 013, recorded as
  not decided too, still read As opportunity allows there. A later sitting for `<P>` plans them again.
  - Her ratified review holds no seat, so Act 5 can schedule her final Pre-graduation review for `<P>`.
  - Played for `<P>`, that review's agenda leaves off PAED-001, 010 and 012 as "Already decided in this window".
    Re-deciding PAED-010 at `5` stages it on the chair's own line and supersedes the Act 4 STAR when ratified, so the
    `D+20` expiry does not reach the appendix's expiry job on an Active STAR.
- **The panel is unchanged,** a General panel of four, and every review before it is ratified or closed. Nothing is left
  open for the appendix's erasure to withdraw.
- **Dlamini's PAED-002 reads "Revoked: re-decide".** The four other registrars' reviews, and Mahlangu's remitted appeal,
  stay as the record, for the portfolio PDF and for coverage.md. Dr du Plessis's programme ends in Act 5, and Act 6 has
  Dr Dlamini, Dr Mahlangu and Dr Ndlovu act again.

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
Do: Open Decisions due from the menu. Leave Period on `<P>` and Status on "Outstanding: not yet decided", and open By
  EPA. Page through the list with Next. Then set Status to "Not scheduled" and EPA to PAED-004, and press Show.
Expect: The opening sentence names `<P>`, "5 trainees you oversee at Kgosi Kgari Teaching Hospital", and what Missed
  means. Period's help reads "The College's semester 2 ends in November; its window runs to 31 December, so December's
  work counts." By EPA ("Counts for the 15 EPAs due in `<P>`") lists all 15 EPAs, each Due 5.
  - Twelve EPAs read To schedule 5.
  - PAED-008, 009 and 013 read Optional 5.
  The count reads "75 of 75 decisions due in `<P>` shown.", and the list pages 20 rows at a time.
  - Each semester EPA's row (PAED-001, 002, 004, 005, 010, 012) reads Not scheduled, window `<P>`, "No open review
    holds it, and `<P>` has not ended.".
  - Each annual EPA's row (PAED-003, 006, 007, 011, 014, 015) reads the same with window `<Y>`: "… and `<Y>` has not
    ended.".
  - The three optional EPAs read "As opportunity allows".
  Every Not scheduled row offers one action, Schedule, named by trainee, EPA and panel ("Schedule a review of … for
  PAED-001 before Paed Annual Review Panel"). After Next the focus is on the heading, which reads "Decisions, page 2 of
  4" (T264). The filters leave five PAED-004 rows, one per registrar ("5 of 75 decisions due in `<P>` shown: Not
  scheduled, PAED-004."), with Clear filters offered; By EPA does not change. No Missed row exists.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Smit's menu Home, Programme trainees, Waiting for assessors,
  Decisions due, MSF campaigns, Committee reviews, Decision panels, Data rights requests, My data rights; Decisions
  due lit. Opening sentence "The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at
  Kgosi Kgari Teaching Hospital, ..." with what Missed means. Period's help "The College's semester 2 ends in
  November; its window runs to 31 December, so December's work counts." By EPA "Counts for the 15 EPAs due in 2026
  S2": Due 5 on all 15; To schedule 5 on twelve, Optional 5 on 008/009/013. "75 of 75 decisions due in 2026 S2
  shown.", 20 rows a page (page size 100 shows all 75): 30 semester rows "No open review holds it, and 2026 S2 has not
  ended.", 30 annual "... and 2026 has not ended.", 15 "As opportunity allows"; one action per row, Schedule, named
  "Schedule a review of Anele Dlamini for PAED-001 before Paed Annual Review Panel". Next: focus on h2 "Decisions,
  page 2 of 4". Not scheduled + PAED-004 + Show: "5 of 75 decisions due in 2026 S2 shown: Not scheduled, PAED-004.",
  five rows, Clear filters; By EPA unchanged. No Missed row. No sideways scroll at 1280.
Gap: none

### Step 4.2 — Prof Mbatha reads the same period
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /committee/decisions-due → /committee/reviews/new → /committee/decisions-due
Do: Open Decisions due from the menu. Look for an Institution filter. Switch Period to the previous semester, `<Y>` S1,
  and press Show, then open By EPA. Press Schedule on one Missed row and read the form it opens, then Cancel without
  creating. Back on Decisions due, switch Period to `<P>` and press Show.
Expect: No Institution filter is offered, because she reads her own institution. The same five registrars and 75 rows
  as Step 4.1. For `<Y>` S1 the statuses follow that period's windows:
  - every registrar's six semester EPAs read Missed ("`<Y>` S1 has ended with nothing decided or on an open review's
    agenda.", T361). Mahlangu and Ndlovu started on 15 January, inside the semester's first month, so their windows
    count too (D42);
  - the annual EPAs read "Due by year end", so PAED-003 and 006 are never Missed;
  - PAED-008, 009 and 013 read "As opportunity allows", since an optional EPA is never missed;
  - By EPA reads Missed 5 on the six semester EPAs.
  A Missed row's Schedule opens Schedule a review filled with the panel, that registrar and the row's own period (F1),
  "`<Y>` S1 · 1 Jan to 30 Jun `<Y>`", the window `<Y>`-01-01 to `<Y>`-06-30. Period's help begins "`<Y>` S1 has ended. A
  review can still sit for it: sittings are often held after the period ends." Review type offers Annual progression
  review (chosen), Pre-graduation review and Entrustment-only review, then Formative check-in. The Agenda reads `<Y>`
  S1's plan as the planner gives it: "PAED-001, PAED-002, PAED-004, PAED-005, PAED-010 and PAED-012 are due at this
  sitting.", the six annual EPAs "Due by year end" and PAED-008, 009 and 013 "As opportunity allows". Cancel returns to
  Decisions due on `<Y>` S1. Nothing is created.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): No Institution filter (filters Period, Trainee, Status, EPA,
  Decisions per page). 2026 S2: the same five, 75 of 75. 2026 S1 + Show: "75 of 75 decisions due in 2026 S1 shown.";
  30 Missed "2026 S1 has ended with nothing decided or on an open review's agenda." (six per registrar, Mahlangu and
  Ndlovu too); 30 "Due by year end" (PAED-003 and 006 among them); 15 "As opportunity allows"; By EPA Missed 5 on
  001/002/004/005/010/012. Dlamini's PAED-001 Missed row's Schedule opened /committee/reviews/new filled: Paed Annual
  Review Panel, Anele Dlamini, "2026 S1 · 1 Jan to 30 Jun 2026", window 2026-01-01 to 2026-06-30, Scheduled on
  2026-10-26, Annual progression review chosen; Period help begins "2026 S1 has ended. A review can still sit for it:
  sittings are often held after the period ends."; types Annual progression, Pre-graduation, Entrustment-only,
  Formative check-in. Agenda "15 EPAs will be on the agenda for 2026 S1.", "PAED-001, PAED-002, PAED-004, PAED-005,
  PAED-010 and PAED-012 are due at this sitting.", then nine lines "Due by year end": the six annual EPAs and also
  PAED-008, 009 and 013. Cancel returned to /committee/decisions-due?period=2026-1 (75 of 75); back on S2: 75 of 75.
  Nothing created. No sideways scroll at 1280.
Gap: T365 (still: the schedule page's S1 Agenda lists the optional PAED-008, 009 and 013 as "Due by year end", where
  Decisions due reads "As opportunity allows")

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
  - **The page:** the same five registrars, all in Paediatrics, and 75 rows, each with one action, Schedule.
  - **Entrustment decisions** is in the menu (Step A.7.7 opens it from there); no card on Home names it.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Home "Speciality admin · Semester 2, 2026". Menu Home,
  Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels, Entrustment decisions,
  My data rights; Decisions due lit on its page. Cards Waiting for assessors "2 waiting, 1 overdue" (du Plessis's
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Home "Sub-speciality admin · Semester 2, 2026"; menu and the
  three cards as Mokoena's ("2 waiting, 1 overdue", "5 registrars", Targets by EPA; no Entrustment decisions card).
  Decisions due lit: "the 5 trainees you oversee at Kgosi Kgari Teaching Hospital", "75 of 75 decisions due in 2026 S2
  shown.", Schedule on all 75 rows.
Gap: none

### Step 4.5 — Dr Naidoo is not offered what is due
Role: CommitteeMember — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /committee/decisions-due → /access-denied
Do: He still acts as Assessor, his choice from Step 3.5: choose Switch to Committee member in the sidebar. Look for
  Decisions due in the menu, then type its address.
Expect: His menu has Committee reviews and Decision panels but no Decisions due. The typed address lands on the
  access-denied page, and nothing in his menu is lit there: the page is for the roles that schedule reviews.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Home "Assessor · Semester 2, 2026" (menu Home, Activity inbox,
  My data rights) with "Switch to Committee member". After it: "You are now acting as Committee member."; menu Home,
  Programme trainees, Committee reviews, Decision panels, My data rights; no Decisions due. Typed
  /committee/decisions-due -> /access-denied?ReturnUrl=..., "You cannot open this page", "None of your roles
  (Committee member, Assessor) opens this page."; no menu item lit.
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
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews from the menu and press Schedule a review. Fill in:
  - Panel: `Paed Annual Review Panel`.
  - Trainee: Dr Molefe.
  - Period: `<P>`, and leave the evidence window as it fills.
  - Scheduled on: `D`, as it starts.
  - Review type: Annual progression review, as it starts.
  Read the Agenda, then press Create review.
Expect:
  - **The page:** Schedule a review is a page of its own, under the trail Home › Committee reviews › Schedule a review
    (T364, Q1).
  - **Panel:** the one panel is offered. Trainee is disabled until a panel is chosen ("Choose a panel first. The list
    holds the trainees at its institution you can schedule."), then lists the five registrars, each with the training
    year ("Lerato Molefe · Training year 4").
  - **Period:** labelled "`<P>` · 1 Jul to 31 Dec `<Y>`". Choosing it fills the window as `<Y>`-01-01 to `<Y>`-12-31
    and adds "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts."
  - **Review type:** radios, "Annual progression review" (chosen) and "Pre-graduation review", with the help "The
    semester-2 sitting decides the trainee's progression for the year." (T131 slice 5), then Formative check-in (T309
    item 3).
  - **The Agenda** reads "15 EPAs will be on the agenda for `<P>`." It then says "PAED-001, PAED-002, PAED-003,
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Committee reviews "Schedule progression reviews and open
  existing ones.", "No reviews yet". Schedule a review opened /committee/reviews/new, tab "Schedule a review ·
  Wombat", trail Home > Committee reviews > Schedule a review. Trainee disabled ("Choose a panel first. The list holds
  the trainees at its institution you can schedule.") until the one panel is chosen, then the five, each "<name> ·
  Training year n" (Lerato Molefe · Training year 4). Period "2026 S2 · 1 Jul to 31 Dec 2026" filled 2026-01-01 to
  2026-12-31 and added "The College's semester 2 ends in November; its window runs to 31 December, so December's work
  counts."; Scheduled on 2026-10-26. Review type radios: Annual progression review (chosen) with "The semester-2
  sitting decides the trainee's progression for the year.", Pre-graduation review, Formative check-in. Agenda "15 EPAs
  will be on the agenda for 2026 S2.", "PAED-001, PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007,
  PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this sitting.", PAED-008/009/013 "As opportunity
  allows". No sideways scroll at 1280, 768 or 390. Create review opened /committee/reviews/1: h1 "Lerato Molefe · 2026
  S2", tab "Lerato Molefe · 2026 S2 · Wombat", subtitle "Annual progression review · Paed Annual Review Panel ·
  Training year 4" with "Lerato Molefe's progress"; bar Scheduled, "15 EPAs on the agenda", "Summative · evidence
  2026-01-01 to 2026-12-31", "As of 22:28 SAST", Refresh, Start review… (offered to Smit, left). "No evidence has been
  frozen yet. Starting the review captures the current bundle."
Gap: none

### Step 4.7 — Mr Smit schedules Dr Dlamini's and Dr du Plessis's reviews
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: From Schedule a review, schedule the same kind of review for Dr Dlamini, then for Dr du Plessis: same panel, `<P>`,
  window as filled, Scheduled on `D`, Annual progression review.
Expect: Each opens as Scheduled with a 15-EPA preview like Step 4.6's. Committee reviews, captioned "Committee reviews
  at Kgosi Kgari Teaching Hospital, newest scheduled first" under "The College's semester 2 ends in November; …",
  lists three rows for `<P>`. Each row reads:
  - Period "`<P>`" over its window, "`<Y>`-01-01 to `<Y>`-12-31";
  - Type Annual progression;
  - State Scheduled, a grey badge, as on Decisions due;
  - Decision Pending.
  There is no Mode column (C8). Each row's Open is named by the review it opens.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Reviews #2 ("Anele Dlamini · 2026 S2", Training year 3) and #3
  ("Pieter du Plessis · 2026 S2", Training year 2), each Scheduled after "15 EPAs will be on the agenda for 2026 S2.".
  The list under the College's semester-2 line and the caption "Committee reviews at Kgosi Kgari Teaching Hospital,
  newest scheduled first": columns Trainee, Panel, Period, Type, State, Decision, Actions (no Mode); three rows "2026
  S2 / 2026-01-01 to 2026-12-31", Annual progression, Scheduled (badge-draft, grey), Pending; each Open named "Open
  the 2026 S2 review of <trainee> before Paed Annual Review Panel". No sideways scroll.
Gap: none

### Step 4.8 — A second review of Dr Molefe for the same period is refused
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new
Do: Press Schedule a review and choose the panel, Dr Molefe and `<P>`, as for another Annual progression review. Read
  the Agenda before Create.
Expect: Before Create, the Agenda says the seat is held (T309 item 1): "Already held. Review #N already puts Lerato
  Molefe before Paed Annual Review Panel for `<P>` (scheduled on `D`). A trainee has one binding review for each period
  …", then "A formative check-in takes no seat: choose Formative check-in under Review type to schedule one." Create
  review is not offered: "Create review is not offered: review #N already holds `<P>` for Lerato Molefe.", beside Cancel
  and Open review #N. The wording is a product decision (T131 slice 4; T364, C7). Nothing is created; the list still
  holds three rows.
Note: The race, a seat taken from a second tab after the page read it and refused at Create in the same sentence, is a
  scratch state in states.md, not played here (F5).
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Panel, Molefe and 2026 S2: before Create the Agenda reads
  "Already held. Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on
  2026-10-26). A trainee has one binding review for each period before the panels that decide the same EPAs, until it
  is ratified: open that review, or ratify it before scheduling another.", "A formative check-in takes no seat: choose
  Formative check-in under Review type to schedule one." and "Create review is not offered: review #1 already holds
  2026 S2 for Lerato Molefe."; Cancel and Open review #1 (/committee/reviews/1), no Create review. No sideways scroll
  at 390. The list still three rows.
Gap: none

### Step 4.9 — Dr Mokoena schedules Dr Mahlangu's review from what is due
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: On Decisions due, set Trainee to Dr Molefe, press Show and read the line above the list. Then set Trainee to Dr
  Mahlangu, press Show, and press Schedule a review on the line (or Schedule on her PAED-001 row). Check the form it
  opens, set Scheduled on `D`, and create the review.
Expect:
  - **Decisions due before she schedules:** Molefe's, Dlamini's and du Plessis's rows read Scheduled, "On the agenda of
    review #N before Paed Annual Review Panel, which is still open." (T259), each with Open review #N.
  - **The trainee line** (C2): with Dr Molefe filtered, "Review #N (annual progression) holds `<P>` before Paed Annual
    Review Panel, scheduled on `D`." with Open review #N; with Dr Mahlangu, "No open review holds `<P>`." with Schedule
    a review.
  - **The link opens the schedule page already filled:** Panel `Paed Annual Review Panel`, Trainee Dr Mahlangu and
    Period `<P>`. The preview shows 15 EPAs. The query string fills only what the form offers her, so the link
    authorises nothing.
  - **The review opens as Scheduled,** and Start review… is not offered to her. Her sitting bar, static under the title,
    says: "Only the coordinators of the panel's institution, and those of its members who are active committee members
    there, can start this review." (T194, E2), with "As of hh:mm SAST" and Refresh.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Decisions due, Every status + PAED-001: Dlamini, du Plessis
  and Molefe Scheduled, "On the agenda of review #2/#3/#1 before Paed Annual Review Panel, which is still open.", each
  with Open review #n; Mahlangu and Ndlovu Not scheduled; By EPA PAED-001 Due 5, Scheduled 3, To schedule 2. Trainee
  Molefe: "Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-26."
  with Open review #1. Trainee Mahlangu: her Schedule control (named "Schedule a review of Nomsa Mahlangu ...") opened
  /committee/reviews/new?panel=1&trainee=...&period=2026-2&return=... filled: Paed Annual Review Panel, Nomsa Mahlangu
  · Training year 1, 2026 S2 · 1 Jul to 31 Dec 2026, 2026-01-01 to 2026-12-31, Scheduled on 2026-10-26; preview 15
  EPAs. Review #4 "Nomsa Mahlangu · 2026 S2", Scheduled; no Start review…; her bar static (sitting-bar--reader): "Only
  the coordinators of the panel's institution, and those of its members who are active committee members there, can
  start this review.", "As of 22:28 SAST", Refresh. The line "No open review holds 2026 S2." was read on Ndlovu's line
  (Step 4.10): Mahlangu's line text was not printed before it changed, and the same read of her afterwards shows
  "Review #4 (annual progression) holds 2026 S2 ...".
Gap: none

### Step 4.10 — Dr Sithole schedules Dr Ndlovu's review from what is due
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: On Decisions due, press Schedule on one of Dr Ndlovu's rows. Set Scheduled on `D` and create the review. Then go
  back to Decisions due and open By EPA.
Expect: As Step 4.9: the form is filled, the review opens as Scheduled, and Start review… is not offered: the sitting
  bar says who can start it. Decisions due then reads Scheduled on all 75 rows, and By EPA reads Scheduled 5 and To
  schedule 0 for every EPA.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Trainee Ndlovu: "No open review holds 2026 S2." with Schedule
  a review ("Schedule a review of Sipho Ndlovu for 2026 S2 before Paed Annual Review Panel"), which opened the form
  filled (panel, Sipho Ndlovu · Training year 1, 2026 S2, window 2026-01-01 to 2026-12-31, Scheduled on 2026-10-26,
  Annual progression). Review #5 "Sipho Ndlovu · 2026 S2" Scheduled, no Start review, the same "Only the coordinators
  ..." sentence, "As of 22:28 SAST", Refresh. Decisions due, Every status: "75 of 75 decisions due in 2026 S2 shown:
  every status.", all 75 Scheduled; By EPA Due 5, Scheduled 5, To schedule 0 on every EPA.
Gap: none

### Step 4.11 — Dr Naidoo lists the panel's reviews
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then open Dr Dlamini's review.
Expect:
  - **The list:** its subtitle reads "Open existing committee reviews.", and no Schedule a review is offered. It
    lists the five reviews of the panel he sits on, all Scheduled, with Decision Pending.
  - **Dlamini's review:** Start review… is offered to him in the sitting bar, as a member who may sit on the panel (E2).
    He leaves it.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Acting as Committee member (kept from 4.5). Subtitle "Open
  existing committee reviews.", no Schedule a review; five rows (Ndlovu, Mahlangu, du Plessis, Dlamini, Molefe),
  Annual progression, Scheduled, Pending. Dlamini's review #2 offers him Start review… in a reader's bar
  (sitting-bar--reader sitting-bar--starts, "As of 22:29 SAST", Refresh); left.
Gap: none

### Step 4.12 — Prof Mbatha checks the schedule
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/decisions-due
Do: Read Committee reviews, then Decisions due. Set Trainee to Dr Ndlovu and press Show.
Expect: Committee reviews offers Schedule a review and lists all five reviews. Decisions due reads Scheduled on all 75
  rows, each with Open review #N, which opens the review holding it; no row offers Schedule. With Dr Ndlovu filtered,
  the line reads "Review #N (annual progression) holds `<P>` before Paed Annual Review Panel, scheduled on `D`." with
  Open review #N.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Committee reviews offers Schedule a review and lists the five.
  Decisions due, Every status: 75 rows, all Scheduled, each with an Open review link that opens the review its
  sentence names (75 of 75 checked), no row offers Schedule. Trainee Ndlovu: "Review #5 (annual progression) holds
  2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-26." with Open review #5.
Gap: none

### Step 4.13 — The panel cannot change what it decides while its reviews are open
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels → /committee/panels/{PanelId:int}
Do: Open the panel from Decision panels and read Decides for.
Expect: Decides for reads "General panel", with no select and no Save committee: the lock is said first (F5), "It
  cannot change now. Review #N (`<P>`, scheduled) still sits before this panel. What a review decides, and its agenda,
  follow the College committee the panel sat as when it was scheduled …" (T131 slice 5), with Open review #N, the
  first review still open before the panel. About this panel's Open reviews names the same review, linked, "and 4
  more".
Note: The race, a review scheduled from a second tab while the select is open, refused at Save committee and said
  beside the select, is a scratch state in states.md, not played here (F5).
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Decision panels -> "Edit Paed Annual Review Panel" ->
  /committee/panels/1 (tab "Paed Annual Review Panel · Wombat"). About this panel: Scope Speciality: Paediatrics,
  Institution, Open reviews "Review #1 (2026 S2, scheduled) and 4 more" (linked /committee/reviews/1). Who sits on it:
  Chair select (Thandi Zulu), seats Member, Member, External member. Decides for "General panel", no select and no
  Save committee (only select the Chair, only button Save panel): "It cannot change now. Review #1 (2026 S2,
  scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the
  panel sat as when it was scheduled, so the panel keeps what it sits as until its open reviews are ratified, or
  closed if formative." with Open review #1. No sideways scroll at 768 or 390 on the list or the panel page.
Gap: none

## Phase 4.C — Before the sitting

### Step 4.14 — Dr Botha checks who sits and who decides
Role: CommitteeMember — Dr Sarah Botha
Route: /committee/panels
Do: Open Decision panels.
Expect:
  - **The list:** `Paed Annual Review Panel` with Scope "Speciality · Paediatrics", Decides for General panel and
    Members naming the four, the chair first. There is no New panel button and no Edit column: she manages no panel
    (T239, T256).
  - **Who decides each EPA:** curriculum 11.1's EPAs are decided by `Paed Annual Review Panel`. PAED-004 and 005 fall
    back to it, because no panel covering the programme sits as the neonatal CCC.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Acting as Committee member; Decision panels lit. One row: Paed
  Annual Review Panel, Speciality · Paediatrics, General panel, "4: Thandi Zulu (chair), Sarah Botha, David Naidoo,
  John van Rensburg (external)"; no New panel, no Edit, no control in main; typed /committee/panels/1 ->
  /access-denied ("None of your roles (Committee member, Assessor) opens this page."). Who decides each EPA: PAED-004,
  PAED-005 -> Paed Annual Review Panel, "No panel covering this programme sits as the Neonatal team Clinical
  Competency Committee, so the general panel decides."; PAED-001 to PAED-003, PAED-006 to PAED-015 -> the same panel.
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Review #1: h1 and tab "Lerato Molefe · 2026 S2" ("... ·
  Wombat"), subtitle "Annual progression review · Paed Annual Review Panel · Training year 4" with "Lerato Molefe's
  progress" (/programme/trainees/2); no Review or Pending card. The chair's bar (sticky): Scheduled, "15 EPAs on the
  agenda", "Summative · evidence 2026-01-01 to 2026-12-31", Start review…. Sections: The sitting, Before the committee
  decides (first), Agenda, Decision ("No decision has been recorded yet."), Entrustment against Annexure A, MSF,
  Evidence snapshot, trajectory, Appeals. "In training year 4 on 2026-10-26", "0 at or above · 0 below · 15 with no
  decision, of 15 EPAs", "0 of 15 EPAs at their exit level by STAR decision ...", all fifteen Not yet, "For
  information only. ..."; PAED-001, 010, 012 link #trajectory-2/11/13-h. MSF: Semester 2 PAED-010 and 012 Covered
  ("Default MSF #1, closed 2026-10-26"), the rest "None released" / "None released yet". Agenda "15 EPAs for 2026 S2",
  EPA/Window/State/Evidence (no Action), 2026 S2 x6 and 2026 x9, twelve "Due at this sitting" with no sentence, three
  "As opportunity allows" with "Decided as opportunity allows: optional.", 0 items each. "No evidence has been frozen
  yet. ..." Trajectory: PAED-001 "Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from
  Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below.", no Today. Sampling names PAED-010 (1
  rating · 1 assessor · 1 source) and PAED-012 (2 · 2 · 2), not PAED-001; "Not counted: 2 records ...". "No appeal has
  been lodged against this review." Enter on the PAED-010 link focused its h3 at y 507, URL unchanged; 390 px no
  sideways scroll. T356: at 1280 the sticky bar sits at 56-110 and at 768 at 56-170, Sign out (14-42) never covered.
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Dialog "Start Lerato Molefe's review for 2026 S2?": "Starting
  freezes the evidence: the 9 items observed from 2026-01-01 to 2026-12-31 (8 activities and the MSF campaign report)
  become this review's snapshot, each in the state it is in now. Anything finished later is not added."; no left-out
  sentence; focus on "Not yet"; confirm "Start review". Then "Review started." atop the agenda, focused. Bar In
  progress, "None staged", "Present: the chair · one more member needed", Go to the decision. Caption "15 EPAs for
  2026 S2 · none staged"; each line counts its items (PAED-001 3, 010 2, 012 3, the rest 0) and offers Stage only.
  Snapshot "Frozen when the review started, 2026-10-26 · 9 items", h3 per EPA, lines by instrument A-Z (PAED-001: CBD
  #13 David Naidoo 5, DOPS #14 Mohammed Patel 4, Mini-CEX #12 Thandi Zulu 5), each with assessor, rating, encounter,
  state; MSF #22/#23 "Not recorded", Recorded, under PAED-010/012; "Default MSF #1 · Multi-source feedback. State:
  Released; responses 8; closed 2026-10-26." under "Not about a single EPA". Decision form: Category "Select a
  category…", Rationale, "About the EPAs not decided (optional)" with "The registrar sees this once the review is
  ratified.", Conditions, Present (Thandi Zulu (chair) ticked and locked, Sarah Botha, David Naidoo, John van Rensburg
  (external)). Record decision… aria-disabled (not disabled) with "Only the chair is ticked. A committee decision
  needs at least two panel members present: the chair and at least one other."; no EPA named. T356: Sign out (14-42)
  never covered at 1280 or 768.
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Snapshot "Mini-CEX (Paediatrics) #12" ->
  /activities/12?from=review-1&at=snap-8: Completed, "Done. You completed it on 2026-10-26 21:08 SAST.", "Rated 5.
  Credited 1 item to PAED-001."; no move; only back link "Back to Lerato Molefe · 2026 S2"
  (/committee/reviews/1#snap-8), which lands with scrollY 6,843, the row at y 489 and focused. Stage on PAED-001:
  "Stage a decision on PAED-001" under its row, focus on Authorised level; "Year 4 target 5 · no STAR yet · 3 ratings
  in the review window · MSF: None released yet", "Rating chart for PAED-001". Level "Choose a rung…", 1, 2, 3a, 3b,
  4, 5 in the optgroup "CPSA Paediatric Entrustment Scale v11.1", help "Rungs of CPSA Paediatric Entrustment Scale
  v11.1, the ladder this EPA is assessed on."; Issued 2026-10-26. Evidence CBD #13, DOPS #14, Mini-CEX #12 first, each
  with tick and Read, then "Show the other 6 snapshot lines". Read swapped in the Mini-CEX (focus on its h3); "Back to
  PAED-001" restored level 5 and rationale, focus on Read. Stage pending decision aria-disabled with "Name at least
  one item of the evidence snapshot."; one tick: "Only one item is named. The College's rule is that an entrustment
  decision is never taken from a single form.", available. #12 and #13: "Pending entrustment decision staged."
  focused; line Staged, "Level 5 · issued 2026-10-26 · no expiry.", "Rests on 2 items of the snapshot: Mini-CEX
  (Paediatrics) #12; Case-Based Discussion (Paediatrics) #13.", Remove… ; caption and bar "1 staged".
Gap: none

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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): PAED-010's open line lists its two lines first: Direct
  Observation (Paediatrics) #17 · Thandi Zulu Rated 5, and Multi-Source Feedback (Paediatrics) #22 "Not recorded".
  Level 4, Issued 2026-10-26, Expires 2026-10-25 (D-1), the rationale, both ticked: "Not staged. An expiry date must
  be after the issue date." at the top of the line's form, focused; Expires on aria-invalid, described by "An expiry
  date must be after the issue date.". Level, ticks and rationale kept; PAED-010 still "Due at this sitting" with 2
  items; bar "1 staged".
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Expires 2026-11-15 (D+20): "Pending entrustment decision
  staged.", PAED-010 "Level 4 · issued 2026-10-26 · expires 2026-11-15." on #17 and MSF #22. PAED-012 at 5 on Mini-CEX
  #15 and CBD #16: staged, "Level 5 · issued 2026-10-26 · no expiry.", 3 items; bar "3 staged". Remove… (named "Remove
  the staged decision on PAED-012") opened "Remove the staged decision on PAED-012?", "Its level 5, its rationale and
  the 2 snapshot items it rests on are discarded. PAED-012 is due at this sitting again.", focus "Keep it". "Remove
  the decision": "Pending entrustment decision removed." focused; PAED-012 "Due at this sitting" with Stage; bar "2
  staged". Staged again: Staged, bar "3 staged".
Gap: none

### Step 4.20 — A second decision on PAED-001 is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: In "Stage a decision on an EPA not on this agenda", at the agenda's foot, choose PAED-001 from the EPA select, then
  fill in level `4`, a rationale and one line, DOPS (Paediatrics) #14 (rated 4, Mohammed Patel). Stage it.
Expect: Refused: "Not staged. An entrustment decision on PAED-001 is already staged at this review. Remove it to stage
  another." at the top of that form, with the focus there. PAED-001's line still holds exactly one decision, "Level 5
  · issued `D` · no expiry.", and the bar still reads "3 staged".
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Foot form "Stage a decision on an EPA not on this agenda":
  PAED-001, level 4, a rationale, one snapshot line: "Not staged. An entrustment decision on PAED-001 is already
  staged at this review. Remove it to stage another." at the top of that form, focused. PAED-001 still one decision,
  "Level 5 · issued 2026-10-26 · no expiry."; bar "3 staged".
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Naidoo (acting as Committee member) sees the sampling, agenda,
  Decision, standing, MSF, snapshot, trajectory and Appeals sections; PAED-001/010/012 Staged with their decisions and
  "Rests on ..." in their lines, the rest "Due at this sitting". Agenda EPA/Window/State/Evidence (no Action); no
  Stage, Remove…, foot form or decision form (no form control; his only button Refresh). Bar static
  (sitting-bar--reader): In progress, "3 staged", "Only the panel's chair, Thandi Zulu, can stage entrustment
  decisions and record the committee's decision.", "As of 22:32 SAST", Refresh. Refresh (page at the top, scroll 0):
  "As of 22:32 SAST. No change.", focus kept on Refresh. Decision reads "No decision has been recorded yet."
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Go to the decision moved the focus to h2 "Decision".
  Satisfactory Progress and the rationale, only Thandi Zulu (chair, locked): Record decision… aria-disabled, the
  quorum line "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and
  at least one other." Pressed: focus moved to that line, no dialog, nothing sent (Step 4.38 finds no FAILED row).
  Still In progress, bar "Present: the chair · one more member needed".
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Botha, Naidoo, van Rensburg ticked: "4 present: the chair and
  3 others. The quorum is met.", bar "4 present · quorum met"; Record decision… available with "Recording fixes the 3
  staged decisions, and records the 12 EPAs not staged as not decided." Category back to "Select a category…" and
  pressed: no dialog; "Not recorded. This review decides the trainee's progression, so its decision records a
  progression category. Choose one." at the top of Decision, focused; Category aria-invalid with "Choose a progression
  category." Nothing recorded, In progress.
Gap: none

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
    (E1), "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)" and "Afterwards only
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Dialog "Record Satisfactory Progress for Lerato Molefe?":
  "Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires 2026-11-15), PAED-012 at 5.", "Records 12 EPAs as
  not decided: PAED-002, PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-008, PAED-009, PAED-011, PAED-013,
  PAED-014 and PAED-015.", "Present: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van Rensburg (external)",
  "Afterwards only a decision that no longer fits the trainee's curriculum can be removed."; focus "Not yet", confirm
  "Record decision". "Decision recorded." in Decision, now above the agenda, focused. Bar Decided, "Satisfactory
  Progress · 3 decisions to issue", Ratify…. Decision "Satisfactory Progress", the rationale, "About the EPAs not
  decided: To be decided at her final review.", "Present: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van
  Rensburg (external)". "Fixed when the committee's decision was recorded: ratifying issues exactly these."; three
  staged lines keep their decisions, no Remove…; the other twelve (008/009/013 too) "Not decided at this sitting" with
  "To be decided at her final review."; no Action column. No "The chair rated all of this evidence". T356: Sign out
  never covered at 1280 or 768.
Gap: runbook, T370 (known proposal): the Present lists read "Thandi Zulu (chair), Sarah Botha, David Naidoo, John van
  Rensburg (external)", the panel's member order as Decision panels lists them; the Expect quotes Naidoo before Botha

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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): List row: Molefe, Decided, Satisfactory Progress. Review #1:
  subtitle "Annual progression review · Paed Annual Review Panel · Training year 4", no "Lerato Molefe's progress";
  decision with "Present:" above the agenda; "Fixed when the committee's decision was recorded: ratifying issues
  exactly these."; no Ratify…, no Remove…, agenda EPA/Window/State/Evidence. Bar a reader's, static: Decided,
  "Satisfactory Progress · 3 decisions to issue", "Only the panel's chair, Thandi Zulu, can ratify the committee's
  decision.", "As of 22:33 SAST", Refresh (her only control).
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Dialog "Ratify and issue 3 STARs to Lerato Molefe?": "PAED-001
  at 5, issued 2026-10-26", "PAED-010 at 4, issued 2026-10-26, expires 2026-11-15", "PAED-012 at 5, issued
  2026-10-26", "PAED-002 and 11 others were not decided at this sitting.", "Ratifying cannot be undone: nothing on
  this review can change afterwards."; nothing about mail; focus "Not yet"; confirm "Ratify and issue 3 STARs".
  "Decision ratified." focused; bar Ratified, "Satisfactory Progress · 3 STARs issued", no control left. "Nothing is
  pending: ratifying the review issued what was staged as STARs, and the agenda names each one." PAED-001 "Decided
  STAR #1."; the other lines "Not decided at this sitting" with "To be decided at her final review."; "2 at or above ·
  1 below · 12 with no decision, of 15 EPAs"; "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 ·
  level 4: 0 of 6)". No form control left. Stub email count stayed 38.
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Review #2: dialog counts "the 4 items observed from 2026-01-01
  to 2026-12-31 (4 activities)"; "Review started.". Snapshot PAED-001 CBD #18 Zulu 3b, Clinical Case Analysis #19
  Botha 4, Mini-CEX #1 Naidoo 4 and PAED-004 Mini-CEX #20 Khumalo 4, all Completed. PAED-001 at 4 and PAED-002 at 3b,
  each on Mini-CEX #1 and CCA #19. PAED-002's open line lists the snapshot's lines grouped by EPA with none of its own
  first; the picker said "None of the named items is about PAED-002." and staged; the staged line repeats it. 001/002
  Staged, the other ten closing lines "Due at this sitting", 008/009/013 "As opportunity allows"; bar "2 staged".
  Record decision… aria-disabled only on the quorum line ("Only the chair is ticked. ...").
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Naidoo and Botha ticked: "3 present: the chair and 2 others.
  The quorum is met.", hint "Recording fixes the 2 staged decisions, and records the 13 EPAs not staged as not
  decided.". Record dialog "Fixes 2 staged decisions: PAED-001 at 4, PAED-002 at 3b." and "Records 13 EPAs as not
  decided: PAED-003, ... PAED-013, PAED-014 and PAED-015."; "Decision recorded."; Ratify dialog "Ratify and issue 2
  STARs to Anele Dlamini?", "PAED-003 and 12 others were not decided at this sitting."; "Decision ratified.",
  Ratified. "Present: Thandi Zulu (chair), Sarah Botha, David Naidoo". PAED-001 "Decided STAR #4.", PAED-002 "Decided
  STAR #5."; the other thirteen "Not decided at this sitting" with "Not at a decision point this year.". "1 at or
  above · 1 below · 13 with no decision, of 15 EPAs": PAED-001 4 vs target 4, PAED-002 3b vs 4.
Gap: runbook, T370 (known proposal): the Present line reads "Thandi Zulu (chair), Sarah Botha, David Naidoo", the
  panel's member order; the Expect quotes Naidoo before Botha

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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Review #3 started (dialog "the 5 items ... (5 activities)").
  Snapshot: PAED-002 CBD #9 Fatima Khumalo Requested and DOPS #8 Cancelled; PAED-004 Teaching Session Log #6 Logged;
  PAED-015 Log #5 Logged and Portfolio and Logbook Review #10 Awaiting review; #7 (J-1y-56d) not in it. "No rating by
  a named assessor to chart in this review's window." David Naidoo ticked: Record available, "Recording records the 15
  EPAs not staged as not decided."; dialog "Fixes no staged decision." and "Records 15 EPAs as not decided: PAED-001,
  PAED-002, ... and PAED-015.". Decision "Satisfactory with Observations", "About the EPAs not decided: Not at a
  decision point this year.", "Conditions: At least two Mini-CEX before the next sitting.", "Present: Thandi Zulu
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Review #4 started ("the 2 items ... (2 activities)"). Snapshot
  PAED-002 DOPS #11 Mohammed Patel 3a and PAED-004 Mini-CEX #21 Thandi Zulu 3a, both Completed. Sampling names
  PAED-002 and PAED-004, "1 rating · 1 assessor · 1 source". Record dialog "Fixes no staged decision.", "Records 15
  EPAs as not decided: ...". "Inadequate Progress — Additional Training", "About the EPAs not decided: Not enough
  observed evidence yet this year.", "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)"; Ratify
  dialog "Ratify Nomsa Mahlangu's review?"; "Decision ratified.", Ratified, "no STAR issued"; all fifteen "Not decided
  at this sitting" with "Not enough observed evidence yet this year."
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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Review #5 started ("the 3 items ... (3 activities)").
  Snapshot: PAED-001 Reflective Exercise #4 Unrated Discussed; PAED-002 Mini-CEX #2 Fatima Khumalo Declined and #3
  Sarah Botha 3a Completed. Trajectory PAED-002 alone, "Sipho Ndlovu · 1 rating in the review window, 2026-01-01 to
  2026-12-31, from Sarah Botha. At the minimum." Sampling names PAED-002, "1 rating · 1 assessor · 1 source". "Outcome
  Deferred", "About the EPAs not decided: Too early in the programme to decide.", "Present: Thandi Zulu (chair), Sarah
  Botha"; Ratified, "Outcome Deferred · no STAR issued"; all fifteen "Not decided at this sitting" with "Too early in
  the programme to decide."
Gap: none

## Phase 4.F — What the programme sees after the sitting

### Step 4.33 — Mr Smit reads the ratified schedule
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Read Committee reviews, then open Dr Molefe's review.
Expect: All five read Ratified, Type Annual progression; there is no Mode column (C8). The Decision column reads:
  - Satisfactory Progress for Molefe and Dlamini;
  - Satisfactory with Observations for du Plessis;
  - Inadequate Progress — Additional Training for Mahlangu;
  - Outcome Deferred for Ndlovu.
  No state or category prints an enum name (T250). Molefe's review shows its decision, agenda and STAR numbers, and
  offers Smit no Ratify or Start, only the header's "Export Lerato Molefe's portfolio" (T379, Step 5.12).
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Committee reviews: columns Trainee, Panel, Period, Type,
  State, Decision, Actions (no Mode); all five Ratified, Annual progression; Decision Satisfactory Progress (Molefe,
  Dlamini), Satisfactory with Observations (du Plessis), Inadequate Progress — Additional Training (Mahlangu), Outcome
  Deferred (Ndlovu); no enum name. Review #1: bar Ratified, "Satisfactory Progress · 3 STARs issued"; "Present: Thandi
  Zulu (chair), Sarah Botha, David Naidoo, John van Rensburg (external)"; STAR #1, #2, #3 in the agenda; no control
  offered to Smit (only the closed dialogs' buttons in the DOM).
Gap: none

### Step 4.34 — Prof Mbatha reads what is due after the sitting
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/decisions-due
Do: Read `<P>` with the default filter. Then set Status to "Every status", press Show, and open By EPA.
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
  - **Actions:** each row has one (C3). A Not decided row offers only Schedule, since no open review holds its seat now;
    its review is a link in its sentence. A Decided row offers none: "Decided: nothing to schedule".
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Default: "70 of 75 decisions due in 2026 S2 shown." Every
  status: "75 of 75 decisions due in 2026 S2 shown: every status.": Decided 5 ("STAR #1/#2/#3, issued at review #1."
  for Molefe 001/010/012, "STAR #4/#5, issued at review #2." for Dlamini 001/002), each "Decided: nothing to
  schedule", no action; Not decided 55, badge amber (badge-accepted), "Not decided at review #N. A later sitting for
  2026 S2 plans it again." or "... for 2026 plans it again.", the review a link in the sentence, one action Schedule;
  As opportunity allows 15, "Decided as opportunity allows: optional, never missed." (grey). By EPA: PAED-001 Due 5,
  Decided 2, Not decided 3.
Gap: none

### Step 4.35 — Prof Mbatha reads the issued STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from the menu, under Reviews. Download Dr Molefe's PAED-010 certificate. Filter by
  trainee "Molefe" and press Show.
Expect:
  - **The page:** lit in her menu, with no trail: a list her menu offers since T376 (flow 12, F10; Home's Quick links,
    its only way in until then, are gone). Its subtitle reads "Issued Statements of Awarded Responsibility across the
    programme."
  - **The list:** captioned "Issued STARs (entrustment decisions) · 5 shown"; five rows, all Active, issued `D`, newest
    issued first and then by STAR number, descending (one day's ties; T366, F1):
    - Dlamini: PAED-002 `3b`, PAED-001 `4`.
    - Molefe: PAED-012 `5`, PAED-010 `4` expiring `D+20`, PAED-001 `5`.
    - Each row is headed by its trainee (the row's header), its EPA in the next column. Every date is ISO.
    - Expires reads "—" where none was set.
    - Each Issued carries "review #n" under its date, a link to the review named "Open review #n" (T366, S7, M9).
    - Each row offers Download and Revoke, named by the decision: "Download the certificate of Lerato Molefe's PAED-010
      decision, issued `D`" (T239).
  - **The certificate PDF** names:
    - Kgosi Kgari Teaching Hospital, "Statement of Awarded Responsibility", Lerato Molefe, PAED-010 and level `4`;
    - Issued on `D` and Expires on `D+20`, Status Active;
    - the rationale, and the evidence summary of the two snapshot lines;
    - Panel Paed Annual Review Panel, the review number, and Chair Thandi Zulu.
  - **The filter** applies on Show (T366, F9) and leaves Molefe's three rows: the caption "… · 3 shown", a status line
    "3 shown" for a screen reader (S11), and Clear filters beside Show.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Mbatha's Home holds People, Curriculum and Your institution,
  no Quick links and no link to Entrustment decisions. Her menu (group Reviews: Decisions due, Committee reviews,
  Decision panels, Entrustment decisions) -> Entrustment decisions, lit, no trail, tab "Entrustment decisions ·
  Wombat", subtitle "Issued Statements of Awarded Responsibility across the programme." Caption "Issued STARs
  (entrustment decisions) · 5 shown"; columns Trainee, EPA, Level, Issued, Expires, Status, Actions; five rows, the
  trainee the row header, all Active, issued 2026-10-26, in order Dlamini PAED-002 3b, PAED-001 4, Molefe PAED-012 5,
  PAED-010 4 expiring 2026-11-15, PAED-001 5; Expires "—" where none. Each Issued carries "review #n", a link named
  "Open review #n" (24 px tall at 1280, 44 at 390). Each row offers Download and Revoke, named "Download the
  certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-26" / "Revoke ...". PAED-010 certificate
  (star-certificate-PAED-010-2-....pdf, 97,585 bytes): Kgosi Kgari Teaching Hospital, "Statement of Awarded
  Responsibility", Lerato Molefe, PAED-010, Authorised level 4, Issued on 26 October 2026, Expires on 15 November
  2026, Status Active, the rationale, Direct Observation #17 and MSF #22, Panel Paed Annual Review Panel, Committee
  review #1, Chair Thandi Zulu. Trainee "Molefe" typed: caption still "5 shown" until Show; after Show "· 3 shown",
  the visually hidden status "3 shown", Molefe's three rows, Clear filters beside Show. No sideways scroll at 1280,
  768 or 390.
Gap: none

### Step 4.36 — Dr Mokoena revokes one of Dr Dlamini's STARs
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /admin/entrustment-decisions
Do: Open Entrustment decisions from the menu. Choose Revoke on Dr Dlamini's PAED-002 decision. Press Revoke the STAR
  once with the reason blank. Then give the reason "Staged on evidence about PAED-001, not PAED-002; to be decided
  again." and press Revoke the STAR.
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail: a list her menu offers (T358, R2-Menus m5;
    as Step A.7.7).
  - **The list:** the same five rows as Step 4.35, since all five registrars are in her speciality.
  - **The dialog** is titled "Revoke Anele Dlamini's PAED-002 STAR?". It names the EPA, level `3b` and issued `D`, and
    says "Revocation is immediate and cannot be undone." It says Anele Dlamini is sent an email saying the STAR was
    revoked and by whom, "unless Anele Dlamini's account is deactivated", and that the reason is not in the email
    (T367; nothing claims delivery, S6). The focus is in Revocation reason, and Revoke the STAR, beside Cancel, is
    enabled from the start (T260).
  - **Pressed blank:** "Give a reason for the revocation." under the box; the dialog stays open, with the focus in the
    box, and nothing is sent (F3).
  - **After revoking:** "Anele Dlamini's PAED-002 STAR is revoked." above the table, with the focus. The row reads
    Revoked, offers Download only, and carries the line "Revoked `D` at hh:mm SAST by Refilwe Mokoena: Staged on
    evidence about PAED-001, not PAED-002; to be decided again.", hh:mm the stored RevokedOn on the South African clock
    (S4, C22).
  - **Dr Dlamini is emailed:** the application log holds a mail to her, "Entrustment decision revoked: PAED-002", naming
    Refilwe Mokoena and linking My authorisations, without the reason (T367).
Note: Until T358 the page was linked from no nav item or dashboard card for her role, and was reached by typing its
  address. Until T366 the revocation was an inline section whose Confirm revocation stayed disabled until the reason
  box lost focus (T260), and which said "The trainee is notified." though nothing was sent (T319).
Actual (2026-10-26, T376 replay, wombat_scenario_t376): From the menu's Entrustment decisions: lit, no trail; the same
  five rows. "Revoke Anele Dlamini's PAED-002 decision, issued 2026-10-26" opened the dialog "Revoke Anele Dlamini's
  PAED-002 STAR?": "PAED-002 — Managing common paediatric presentations, level 3b, issued 2026-10-26. Revocation is
  immediate and cannot be undone.", "Anele Dlamini is sent an email saying the STAR was revoked and by whom, with a
  link to My authorisations, unless Anele Dlamini's account is deactivated. The reason is not in the email: Anele
  Dlamini reads it on My authorisations and on the certificate.", and a further line, "The EPA reads 'Revoked:
  re-decide' on Decisions due, and Anele Dlamini's review agenda keeps its line." Focus in Revocation reason ("0 of
  1,000 characters"); Revoke the STAR (btn-danger) beside Cancel, enabled from the start. Pressed blank: "Give a
  reason for the revocation." under the box (aria-invalid, described by it), dialog open, focus in the box, Stub count
  unchanged (38). The counter counted as typed ("69 of 1,000 characters"). Revoked: "Anele Dlamini's PAED-002 STAR is
  revoked." above the table, focused; row Revoked, Download only, with the line "Revoked 2026-10-26 at 22:37 SAST by
  Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again." The log gained one Stub
  email (39), tags "entrustment, revoked", subject "Entrustment decision revoked: PAED-002", body naming Refilwe
  Mokoena, linking /portfolio/authorisations, without the reason.
Gap: none (F-4.36a, T319's revocation half no longer occurs: the dialog promises the email only unless the account is
  deactivated, and the revocation mails Dr Dlamini (T367); T260's entrustment half no longer occurs: Revoke the STAR
  is enabled from the start and a blank reason is refused under the box)

### Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Open Entrustment decisions from the menu. Set Status to Revoked and press Show, then to Active and press Show.
  Download the revoked certificate. Then open Decisions due, read the count, set Status to "Every status", press Show,
  and open By EPA.
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail (T358, R2-Menus m5).
  - **Filtered:** Revoked leaves Dlamini's PAED-002, with its line "Revoked `D` at hh:mm SAST by Refilwe Mokoena: …";
    Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", "Revoked on `D` by Refilwe Mokoena" (the
    South African day, ISO, T366 G6; who revoked it by name, T142, T324) and the reason.
  - **Decisions due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." Its one action is Schedule (C3).
  - **By EPA:** PAED-002 reads Decided 0, Not decided 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."
Actual (2026-10-26, T376 replay, wombat_scenario_t376): From the menu's Entrustment decisions: lit, no trail. Status
  options All, Active, Expired, Revoked, Superseded. Revoked + Show: Dlamini's PAED-002 with "Revoked 2026-10-26 at
  22:37 SAST by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again."; Active +
  Show: 4 rows. The revoked certificate (star-certificate-PAED-002-5-....pdf, 98,599 bytes): Status Revoked,
  "REVOKED", "Revoked on 2026-10-26 by Refilwe Mokoena", "Reason: Staged on evidence about PAED-001, ...". Decisions
  due: default "71 of 75 decisions due in 2026 S2 shown."; Every status + PAED-002: Dlamini's PAED-002 "Revoked:
  re-decide", "STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again." with one action,
  Schedule ("Schedule a review of Anele Dlamini for PAED-002 before Paed Annual Review Panel"). By EPA PAED-002: Due
  5, Decided 0, Not decided 4, To schedule 1.
Gap: none (F-4.37a, T324's certificate half no longer occurs: the revoked certificate names "Refilwe Mokoena", not a
  user id, on the ISO day)

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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Audit log, Action RecordCommitteeDecisionCommand: five rows,
  all OK, by zulu@kgk.wombat.local, none FAILED. An entry's detail shows Event and Actor only, no Payload. SQL: each
  SummaryJson keeps category, reviewId and presentUserIds (4, 3, 2, 3, 2 ids) and holds rationale, conditions and
  undecidedNote as "[REDACTED]". RevokeEntrustmentDecisionCommand: one OK row by mokoena@kgk.wombat.local, stored
  reason "[REDACTED]".
Gap: none

## Phase 4.G — The registrars read their outcomes

### Step 4.39 — Dr Molefe reads her STARs and downloads a certificate
Role: Trainee — Dr Lerato Molefe
Route: / → /portfolio/authorisations
Do: From her Home's My authorisations card, Open My authorisations. Download the PAED-001 certificate.
Expect: Home's My authorisations card reads "2 at or above · 1 below · 12 with no decision, of 15 EPAs" and lists one
  STAR, "PAED-010 — Leading and operating within a clinical team", a link to its EPA's page, with "4, below training
  year 4's level of 5 · expires `D+20`" under it; its footer is Open My authorisations (T355). My authorisations, under
  the trail Home › My progress › My authorisations, is subtitled "The Statements of Awarded Responsibility (STAR) the
  committee has issued you, and their certificates." In force holds three cards, by EPA code, each with its Authorised
  level, Issued `D` and the committee's rationale: PAED-001 and PAED-012 at `5`, and PAED-010 at `4`.
  - PAED-001 and 012 read "No expiry".
  - PAED-010 reads the date `D+20`, with the badge "Expires in 20 days", counted on the South African calendar (T325).
  Each Download certificate is named by its EPA: "Download certificate for PAED-001", "… PAED-010" and "… PAED-012"
  (F4). There is no "No longer in force" section. The PAED-001 certificate downloads with Status Active and its
  evidence summary.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Home's My authorisations card: "2 at or above · 1 below · 12
  with no decision, of 15 EPAs", one STAR "PAED-010 — Leading and operating within a clinical team" (link
  /portfolio/progress/11), "4, below training year 4's level of 5 · expires 2026-11-15", footer "Open My
  authorisations". It opens /portfolio/authorisations, trail Home > My progress > My authorisations, subtitle "The
  Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates." In force: three
  cards by EPA code, each with Authorised level, Issued 2026-10-26 and the rationale: PAED-001 5 "No expiry", PAED-010
  4 "2026-11-15" with the badge "Expires in 20 days", PAED-012 5 "No expiry". Downloads named "Download certificate
  for PAED-001/010/012". No "No longer in force" section. PAED-001 certificate (star-certificate-PAED-001-1-....pdf,
  96,959 bytes): Status Active, evidence Mini-CEX #12 and CBD #13. No sideways scroll at 390.
Gap: none (played at 22:4x SAST, 20:4x UTC, when the UTC and South African days agree, so T325's clock is not told
  apart here)

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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): My progress (lit): the index's STAR column "STAR against
  training year 4": PAED-001 and PAED-012 "At or above 5", PAED-010 "Below 4 · expires 2026-11-15", the others "No
  decision". "You are in training year 4 on 2026-10-26." "2 at or above · 1 below · 12 with no decision, of 15 EPAs",
  "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)". PAED-001 5 "Issued
  2026-10-26", At or above target 5, exit Reached; PAED-012 the same; PAED-010 4 "Issued 2026-10-26, expires
  2026-11-15", Below target 5, Not yet. Names link /portfolio/progress/<EpaId>; Latest rating "4 · Encounter
  2026-10-17" -> /activities/14, PAED-010's -> /activities/17, PAED-012's -> /activities/16. "For information only.
  ..."; the same table as the committee's. EPA pages: PAED-010 4, Below, Expires 2026-11-15, "5 · not yet"; PAED-001
  5, At or above, "5 · reached". No sideways scroll at 768 or 390.
Gap: none

### Step 4.41 — Dr Molefe reads her review, and not the committee's page
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → /committee/my-reviews/{ReviewId:int} → /committee/reviews/{ReviewId:int} → /access-denied
Do: Open My committee reviews and view her review. Then type the committee's own address for the same review.
Expect:
  - **The list,** captioned "Your committee reviews, newest first": one row, its header the Period "`<P>`" with the
    window "`<Y>`-01-01 to `<Y>`-12-31" on its own line under it (T212; the row's header, T366 D11), Type Annual
    progression, State Ratified (a badge) and Decision Satisfactory Progress. Its View is a
    link named by the review, "View the `<P>` review before Paed Annual Review Panel", to her review's page (T366, F8).
  - **Her review's page:** the h1 and the tab "`<P>` annual progression review", under the trail Home › My committee
    reviews › `<P>` annual progression review, the panel as its subtitle. It shows Panel, Sits for, Evidence window,
    Type "Annual progression review" and State Ratified.
  - **Decisions:** "Satisfactory Progress", with the rationale, "About the EPAs not decided: To be decided at her final
    review." and "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)".
  - **Appeal:** the lodge block is offered, and she lodges nothing.
  - **The agenda** has three columns:
    - Decided with "STAR #n." on PAED-001, 010 and 012;
    - "Not decided at this sitting" with the committee's sentence "To be decided at her final review." on the other
      twelve, due and optional alike: no reason, and no Deferred (T361, D53, E1).
  - **The committee's page** lands on access-denied, as before.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): My committee reviews, caption "Your committee reviews, newest
  first": one row, its header the Period "2026 S2" with "2026-01-01 to 2026-12-31" on its own line, Annual
  progression, Ratified, Satisfactory Progress; View "View the 2026 S2 review before Paed Annual Review Panel" ->
  /committee/my-reviews/1. There: h1 and tab "2026 S2 annual progression review", trail Home > My committee reviews >
  2026 S2 annual progression review, subtitle Paed Annual Review Panel; Panel, Sits for 2026 S2, Evidence window, Type
  "Annual progression review", State Ratified. Decisions: "Satisfactory Progress", the rationale, "About the EPAs not
  decided: To be decided at her final review.", "Present: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van
  Rensburg (external)". Appeal: the lodge block (What happens next, Why you are appealing, "0 of 4,000 characters",
  Lodge appeal), unused. Agenda EPA/Window/Outcome: "Decided" "STAR #1./#2./#3." on 001/010/012; the other twelve "Not
  decided at this sitting" with "To be decided at her final review."; no Deferred. No sideways scroll at 390. Typed
  /committee/reviews/1 -> /access-denied, "Your role (Trainee) does not open this page."
Gap: runbook, T370 (known proposal): the Present line reads "Thandi Zulu (chair), Sarah Botha, David Naidoo, John van
  Rensburg (external)", the panel's member order; the Expect quotes Naidoo before Botha

### Step 4.42 — Dr Dlamini reads what stands after the revocation
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/authorisations → /committee/my-reviews → /committee/my-reviews/{ReviewId:int}
Do: From her Home's My authorisations card, Open My authorisations. Then open My committee reviews and view her review.
Expect: In force holds one card, PAED-001 at `4`, "No expiry". No longer in force, a table, holds the revoked PAED-002
  (T366, F2; T367): Level `3b`, Status Revoked, Since `D`, Why "Revoked by Refilwe Mokoena: Staged on evidence about
  PAED-001, not PAED-002; to be decided again.", and Download, named "Download certificate for PAED-002 (revoked)"
  (F4). Her review's page reads Ratified, and its decision Satisfactory Progress with its rationale and "Present:
  Thandi Zulu (chair), David Naidoo, Sarah Botha". The agenda still reads Decided with "STAR #4." on PAED-001 and "STAR
  #5." on PAED-002, because it records what the sitting did.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Home's card "1 at or above · 0 below · 14 with no decision, of
  15 EPAs" -> Open My authorisations: In force one card, PAED-001 level 4, "No expiry", its rationale. No longer in
  force, captioned "STARs that are no longer in force: revoked, or past their expiry": PAED-002, Level 3b, Status
  Revoked, Since 2026-10-26, Why "Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be
  decided again.", Download named "Download certificate for PAED-002 (revoked)". No sideways scroll at 768 or 390. Her
  review /committee/my-reviews/2: Ratified, "Satisfactory Progress", its rationale, "Present: Thandi Zulu (chair),
  Sarah Botha, David Naidoo"; agenda PAED-001 and PAED-002 Decided (STAR #4 and #5).
Gap: runbook, T370 (known proposal): the Present line reads "Thandi Zulu (chair), Sarah Botha, David Naidoo", the
  panel's member order; the Expect quotes Naidoo before Botha

## Phase 4.H — An appeal

### Step 4.43 — Dr Mahlangu lodges an appeal
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews → /committee/my-reviews/{ReviewId:int} → /committee/my-reviews
Do: View her ratified review and read it. Press Lodge appeal with the reason blank. Then give the reason "The single
  DOPS reflects the start of the year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I
  ask for reconsideration.", press Lodge appeal, and lodge it in the dialog. Then go back to My committee reviews.
Expect:
  - **Before lodging:**
    - the decision reads "Inadequate Progress — Additional Training", with its rationale and "Present: Thandi Zulu
      (chair), Sarah Botha, John van Rensburg (external)";
    - the agenda shows all fifteen lines "Not decided at this sitting", each with "Not enough observed evidence yet
      this year." (T361, D53, E1);
    - the Appeal section offers the lodge block: What happens next, naming the appeal body ("Your appeal is heard by the
      panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).") and every reader of the
      reason; then Why you are appealing, with "Up to 4,000 characters." and a counter; and Lodge appeal.
  - **Pressed blank:** "Give your reason for appealing." under the field, which takes the focus; no dialog opens and
    nothing is sent.
  - **The dialog** is titled "Lodge your appeal?", against "the `<P>` annual progression review's decision, Inadequate
    Progress — Additional Training", with the focus on Keep editing. Keep editing would close it and return the focus
    to Lodge appeal, the reason kept (F6).
  - **After lodging:** "Appeal lodged." heads the Appeal section, with the focus. The state reads Under appeal. Her
    entry, "Your appeal", reads Open, "Lodged `D`" and "Heard by Thandi Zulu (chair) and John van Rensburg
    (external).", with her reason. The form is gone and the agenda stays in view.
  - **The list:** her row reads Under appeal (T308).
Actual (2026-10-26, T376 replay, wombat_scenario_t376): /committee/my-reviews/4: "Inadequate Progress — Additional
  Training", its rationale, "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)"; all fifteen
  lines "Not decided at this sitting" with "Not enough observed evidence yet this year."; Appeal: What happens next,
  "Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external).", who
  reads the reason, Why you are appealing with "Up to 4,000 characters." and "0 of 4,000 characters", Lodge appeal.
  Pressed blank: "Give your reason for appealing." under the field (aria-invalid), focus in it, no dialog. The counter
  read "40 of 4,000 characters" after 40 typed keys, before any blur, and "153 of 4,000" at the end; no console error
  and no CSP violation. Dialog "Lodge your appeal?": "Against the 2026 S2 annual progression review's decision,
  Inadequate Progress — Additional Training.", the appeal body, "Either outcome is final.", who reads it; focus on
  Keep editing. Escape closed it with the focus on Lodge appeal, the reason kept; Keep editing did the same. Lodge
  appeal: "Appeal lodged." heads Appeal, focused; State Under appeal; "Your appeal", Open, "Lodged 2026-10-26", "Heard
  by Thandi Zulu (chair) and John van Rensburg (external).", her reason; form gone, 15-line agenda kept. No sideways
  scroll at 390 or 768. Back on My committee reviews the row reads Under appeal. No mail (Stub count 39).
Gap: none (F-4.43a, T308 no longer occurs: her review is a page of its own, read again after the lodge, and the list
  row reads Under appeal)

### Step 4.44 — Dr Naidoo sees who hears the appeal
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review, and press Go to the appeal.
Expect: The list shows her review Under appeal. The sitting bar carries Go to the appeal (T366, F5); pressed, it moves
  the focus to the Appeals heading. The Appeals section lists the appeal, "Appeal by Nomsa Mahlangu", Open, with "Lodged
  `D`", "Heard by Thandi Zulu (chair) and John van Rensburg (external)." and its reason. It offers him no form. After
  the entry it says "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external
  member, John van Rensburg." (T213, T237)
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Acting as Committee member. The list shows Mahlangu's review
  Under appeal. The bar (a reader's) reads Under appeal, "Inadequate Progress — Additional Training · no STAR issued"
  and Go to the appeal; pressed, the focus moves to h2 "Appeals" (at 1280 it scrolled 0 -> 7,129 px, the heading
  landing at 491 px; at 390 0 -> 13,033 px, at 447 px). Appeals: "Appeal by Nomsa Mahlangu", Open, "Lodged
  2026-10-26", "Heard by Thandi Zulu (chair) and John van Rensburg (external).", the reason, then "Only the appeal
  body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg."; no form,
  his only control Go to the appeal. No sideways scroll at 1280 or 390.
Gap: none

### Step 4.45 — Dr van Rensburg, the external member, is offered the appeal
Role: CommitteeMember (external) — Dr John van Rensburg
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then Dr Mahlangu's review. Read the resolve form, and press Resolve appeal without choosing
  an outcome. Then choose Remitted to see what it asks, and leave without resolving.
Expect:
  - **The list:** the panel's five reviews.
  - **The Appeals section** offers him the resolve form after the entry. Its Outcome is a group of two radios under the
    legend "Outcome", neither chosen: "Dismissed: the decision stands" and "Remitted: the appeal body replaces the
    decision" (T307, D51: there is no Upheld), each read whole at every width (T323). Its help text says Dismissed
    leaves the decision in force, Remitted replaces it, and either closes the review.
  - **Resolve appeal with no outcome** says "Choose an outcome." under the group, with the focus on its first radio,
    and resolves nothing.
  - **Remitted** reveals:
    - Replacement category, opening on "Select a category…";
    - Replacement rationale;
    - Replacement conditions, optional;
    - Present, with both Thandi Zulu (chair) and himself ticked and locked;
    - beside Resolve appeal, the quorum line "2 present: the chair and 1 other. The quorum is met."
  - **Nothing changes** until he submits.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Acting as Committee member (his only role). Committee reviews
  lists the five, Mahlangu's Under appeal. The Appeals section offers the resolve form after the entry: Outcome, a
  group of two radios under the legend "Outcome", neither chosen: "Dismissed: the decision stands" and "Remitted: the
  appeal body replaces the decision"; no Upheld; help "Dismissed leaves the committee's decision in force. Remitted
  replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review,
  and nothing reopens it." Resolve appeal with none: "Choose an outcome." under the group, focus on the first radio
  (Dismissed), nothing resolved. Remitted reveals Replacement category ("Select a category…"), Replacement rationale,
  Replacement conditions (not required) and Present, Thandi Zulu (chair) and John van Rensburg (external) ticked and
  locked, and beside Resolve appeal "2 present: the chair and 1 other. The quorum is met." Each radio's label row is
  24 px tall at 1280 and 768 and 44/48 px at 390, the labels whole (none clipped); no sideways scroll at any width.
  Left unresolved (appeal still open).
Gap: none (F-4.45c, T323 no longer occurs: Outcome is two radios, each read whole at every width)

### Step 4.46 — A remit without a quorum is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: On Dr Mahlangu's review, choose Remitted, the replacement category Satisfactory with Observations and a
  rationale. Tick nobody besides herself as present, read the quorum line, and press Resolve appeal.
Expect: Before pressing, the quorum line reads "Only the chair is ticked. A committee decision needs at least two panel
  members present: the chair and at least one other." Resolve appeal is refused on the page, and nothing is sent
  (T366, M8): "Resolve appeal was refused: A committee decision needs at least two panel members present: the chair
  and at least one other." after the form's actions, in the Appeals section, with the focus on it (T165: a remit
  records its own quorate sitting; T361, round 3 item 15). What she chose and typed is kept, and the review stays
  Under appeal.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Remitted, Satisfactory with Observations and 4.47's rationale;
  for her only Thandi Zulu (chair) is ticked and locked, van Rensburg unticked. The quorum line before pressing: "Only
  the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one
  other." Resolve appeal: no dialog, "Resolve appeal was refused: A committee decision needs at least two panel
  members present: the chair and at least one other." after the form's actions in the Appeals section, focused;
  nothing sent (the audit log holds only the later successful ResolveAppealCommand). Outcome, category and rationale
  kept; bar still Under appeal.
Gap: none

### Step 4.47 — Dr Zulu remits the decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Keep Remitted, Satisfactory with Observations, and the rationale "The stalled Mini-CEX was the assessor's delay;
  progress is adequate, with observed assessments to follow." Write the Replacement conditions "Two observed Mini-CEX
  and one DOPS before the next review." Tick Sarah Botha as present, press Resolve appeal, and remit it in the dialog.
Expect:
  - **The dialog** is titled "Remit Nomsa Mahlangu's appeal?" and reads "Satisfactory with Observations replaces
    Inadequate Progress — Additional Training.", "Present: Thandi Zulu (chair), Sarah Botha" and "This closes the
    `<P>` review. Nothing reopens it, and no STAR changes.", with the focus on Go back (F6). Its confirm is Remit the
    appeal.
  - **Remitted:** "Appeal resolved." heads the Appeals section, with the focus. The state reads Closed (T250).
  - **Decision:** the replacement heads it, "Satisfactory with Observations" with "About the EPAs not decided: Not
    enough observed evidence yet this year." (carried from the first decision, T361, E5), "Conditions: Two observed
    Mini-CEX and one DOPS before the next review." (T307) and "Present: Thandi Zulu (chair), Sarah Botha". The first
    decision stays below it, marked "Replaced on appeal by the decision above.".
  - **Appeals:** the entry reads Remitted, "Lodged `D` · Resolved `D` by Thandi Zulu" and "Remitted: the appeal body
    replaced the decision. The replacement decision is above."; no form.
  - **No STAR changed:** the review issued none.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Conditions written and Sarah Botha ticked: "2 present: the
  chair and 1 other. The quorum is met." Dialog "Remit Nomsa Mahlangu's appeal?": "Satisfactory with Observations
  replaces Inadequate Progress — Additional Training.", "Present: Thandi Zulu (chair), Sarah Botha", "This closes the
  2026 S2 review. Nothing reopens it, and no STAR changes."; focus on Go back; confirm "Remit the appeal". "Appeal
  resolved." heading the Appeals section has the focus. Bar Closed, "Satisfactory with Observations · no STAR issued",
  no control. Decision: "Satisfactory with Observations", the rationale, "About the EPAs not decided: Not enough
  observed evidence yet this year.", "Conditions: Two observed Mini-CEX and one DOPS before the next review.",
  "Present: Thandi Zulu (chair), Sarah Botha"; below it "Inadequate Progress — Additional Training", "Replaced on
  appeal by the decision above.", its first Present line. Appeals: Remitted, "Lodged 2026-10-26 · Resolved 2026-10-26
  by Thandi Zulu", "Remitted: the appeal body replaced the decision. The replacement decision is above."; no form. SQL
  (the closing queries): decision 6 supersedes 4; review State 6; one appeal remitted and resolved; STARs still 5.
Gap: none

### Step 4.48 — Dr Mahlangu reads the outcome
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews → /committee/my-reviews/{ReviewId:int}
Do: Open My committee reviews and view the review.
Expect: The row reads State Closed and Decision Satisfactory with Observations. Her review's page reads Closed, and its
  Decisions holds both decisions, newest first (T308):
  - the replacement, "Satisfactory with Observations", with its rationale, "About the EPAs not decided: Not enough
    observed evidence yet this year.", "Conditions: Two observed Mini-CEX and one DOPS before the next review." and
    "Present: Thandi Zulu (chair), Sarah Botha";
  - the first, "Inadequate Progress — Additional Training", marked "Replaced on appeal by the decision above.", with
    its rationale and "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)".
  Her Appeal section holds "Your appeal", Remitted, "Lodged `D` · Resolved `D` by Thandi Zulu", "Remitted: the appeal
  body replaced the decision. The decision above is theirs, and it is final." and her reason, then "This review is
  closed and cannot be appealed again." No appeal form is offered. The agenda's fifteen lines still read "Not decided at
  this sitting" with "Not enough observed evidence yet this year.": the replacement carries the first decision's
  sentence (T361, E5).
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Row: State Closed, Decision Satisfactory with Observations.
  /committee/my-reviews/4: State Closed; Decisions, newest first: "Satisfactory with Observations", its rationale,
  "About the EPAs not decided: Not enough observed evidence yet this year.", "Conditions: Two observed Mini-CEX and
  one DOPS before the next review.", "Present: Thandi Zulu (chair), Sarah Botha"; then "Inadequate Progress —
  Additional Training", "Replaced on appeal by the decision above.", its rationale and "Present: Thandi Zulu (chair),
  Sarah Botha, John van Rensburg (external)". Appeal: "Your appeal", Remitted, "Lodged 2026-10-26 · Resolved
  2026-10-26 by Thandi Zulu", "Remitted: the appeal body replaced the decision. The decision above is theirs, and it
  is final.", her reason, "This review is closed and cannot be appealed again."; no appeal form. The fifteen lines
  "Not decided at this sitting" with "Not enough observed evidence yet this year." No sideways scroll at 768 or 390.
Gap: none (F-4.48a, T308 no longer occurs: her page shows her appeal and its outcome, both decisions, the replaced one
  marked, and the replacement's conditions)

## Phase 4.I — A formative check-in

### Step 4.49 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: Press Schedule a review and schedule a review of Dr Ndlovu before the panel for `<P>`, Scheduled on `D`, with
  Review type Formative check-in; create it. Then open Decisions due, set Trainee to Dr Ndlovu and press Show.
Expect:
  - **Accepted:** a formative review takes no binding seat for the period (T131 slice 4). Formative check-in's help
    reads "Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress
    check-ins. It takes no seat for the period." (T309 item 3).
  - **The preview:** "A formative review carries no agenda: it decides no EPA."
  - **The review:** its h1 reads "Sipho Ndlovu · `<P>`" and its subtitle "Formative check-in · Paed Annual Review
    Panel · Training year 1" (T309 item 2). The sitting bar reads Scheduled and "Formative: an interim check-in, no
    binding decision · evidence `<Y>`-01-01 to `<Y>`-12-31" (round 3 item 28), with Start review… for Smit. It has no
    Decision, Agenda or Appeals section: only the evidence snapshot, the standing, MSF coverage and the trajectories.
  - **The list:** the review's Type reads "Formative check-in" and its Decision "—", read as "None"; there is no Mode
    column (C8).
  - **Decisions due's line:** "No open review holds `<P>`." with Schedule a review, and "Formative check-in #N is open:
    it takes no seat." with Open check-in #N.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Schedule a review: Paed Annual Review Panel, Sipho Ndlovu ·
  Training year 1, 2026 S2, Scheduled on 2026-10-26; before the type changed the Agenda read the 15-EPA preview (no
  "Already held": his binding review is Ratified). Review type Formative check-in (a radio): its help "Closes without
  a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for
  the period." The Agenda then "A formative review carries no agenda: it decides no EPA."; no sideways scroll at 390.
  Created review #6: h1 "Sipho Ndlovu · 2026 S2", subtitle "Formative check-in · Paed Annual Review Panel · Training
  year 1" with "Sipho Ndlovu's progress". Bar Scheduled, "Formative: an interim check-in, no binding decision ·
  evidence 2026-01-01 to 2026-12-31", "As of 22:42 SAST", Refresh, Start review… for Smit. Sections: The sitting,
  Before the committee decides, Evidence snapshot, standing, MSF, trajectory; no Decision, Agenda or Appeals. List (no
  Mode column): Type "Formative check-in", Scheduled, Decision "— None", Open "Open the 2026 S2 formative review of
  Sipho Ndlovu before Paed Annual Review Panel". Decisions due, Trainee Ndlovu: "No open review holds 2026 S2." with
  Schedule a review, and "Formative check-in #6 is open: it takes no seat." with Open check-in #6
  (/committee/reviews/6).
Gap: none

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
Actual (2026-10-26, T376 replay, wombat_scenario_t376): Subtitle "Formative check-in · Paed Annual Review Panel ·
  Training year 1". Start dialog ("the 3 items ... (3 activities)"); "Review started." under the bar, focused; bar In
  progress, "Formative: an interim check-in, no binding decision", Close review…. Snapshot the same three activities
  as review #5 (#4 Discussed; #2 Declined, #3 3a Completed); standing ("In training year 1 on 2026-10-26") and MSF
  follow it. Close review… is the chair's alone: Naidoo and Smit read a static bar, "Only the panel's chair, Thandi
  Zulu, can close this review.", "As of 22:43 SAST", Refresh, no close button. Dialog "Close Sipho Ndlovu's formative
  check-in?", "It records no decision and issues no STAR. Nothing on it can change once it is closed.", focus "Not
  yet", confirm "Close review". "Formative review closed.", focused; bar Closed, "Formative check-in · no decision
  recorded"; nothing offered. T356: Sign out (14-42) never covered at 1280 or 768.
Gap: none

### Step 4.51 — Dr Ndlovu reads both reviews
Role: Trainee — Dr Sipho Ndlovu
Route: /committee/my-reviews → /committee/my-reviews/{ReviewId:int} → /committee/my-reviews/{ReviewId:int}
Do: Open My committee reviews, and view each review.
Expect: Two rows for `<P>`, the check-in first: newest first, by scheduled date and then review number (T366, F1). The
  check-in reads Type "Formative check-in" (T309 item 2; T364, C8), State Closed, with Decision "No binding decision".
  The annual review reads Annual progression, Ratified, Outcome Deferred. The check-in's page, "`<P>` formative
  check-in", reads Type "Formative check-in" and State Closed, and under Decisions "No binding decision. A formative
  check-in records no decision and issues no STAR."; it has no Appeal section and no agenda (S13). The annual review's
  page, "`<P>` annual progression review", reads Outcome Deferred and offers the lodge block, unused.
Actual (2026-10-26, T376 replay, wombat_scenario_t376): My committee reviews: two rows for 2026 S2, the check-in
  first: "Formative check-in", Closed, "No binding decision", View "View the 2026 S2 formative review before Paed
  Annual Review Panel" (/committee/my-reviews/6); then Annual progression, Ratified, Outcome Deferred
  (/committee/my-reviews/5). The check-in's page "2026 S2 formative check-in" (trail Home > My committee reviews >
  2026 S2 formative check-in): Type "Formative check-in", State Closed; Decisions "No binding decision. A formative
  check-in records no decision and issues no STAR."; no Appeal section, no agenda; no sideways scroll at 390. The
  annual review's page "2026 S2 annual progression review": Outcome Deferred with its rationale, "About the EPAs not
  decided: Too early in the programme to decide.", "Present: Thandi Zulu (chair), Sarah Botha", the lodge block
  unused, the 15-line agenda; no sideways scroll at 390.
Gap: none

## Outcome state

Replay check (2026-10-26, T376 replay, wombat_scenario_t376): match on all six queries (six reviews: four binding
  Ratified, Mahlangu's Closed, Ndlovu's formative Closed, stored with ReviewType 1; 75 agenda lines, 5 Decided and 70
  Not decided: molefe 3/12, dlamini 2/13, the other three 0/15; six decisions, each with a note, Molefe 4, Dlamini 3,
  du Plessis 2, Mahlangu 3 then 2, Ndlovu 2 present, decision 6 superseding 4; five STARs, Dlamini's PAED-002 Revoked,
  PAED-010 expiring 2026-11-15, 2 evidence links each; 0 pending, 1 remitted appeal; frozen lines Molefe 9, Dlamini 4,
  du Plessis 5, Mahlangu 2, Ndlovu 3 and 3). The act wrote one mail, Step 4.36's revocation mail to Dr Dlamini (the
  log's Stub email count went from 38 to 39). Snapshot recovery/scenario-t376-post-act4.dump.

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

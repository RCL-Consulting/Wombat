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
Actual:
Gap:

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
Actual:
Gap:

### Step 4.3 — Dr Mokoena reads what is due in her speciality
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/decisions-due
Do: Sign in, read her nav, and open Decisions Due.
Expect:
  - **Nav:** Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue and Decisions Due.
  - **The page:** the same five registrars, all in Paediatrics, and 75 rows, each Schedule link offered.
  - **Not in her nav or on her dashboard:** Entrustment Decisions.
Actual:
Gap:

### Step 4.4 — Dr Sithole reads what is due in his sub-speciality
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/decisions-due
Do: Sign in and open Decisions Due.
Expect: The same five registrars and 75 rows as Step 4.3: all five train in the Paediatrics sub-speciality he
  administers. His nav reads as Mokoena's.
Actual:
Gap:

### Step 4.5 — Dr Naidoo is not offered what is due
Role: CommitteeMember — Dr David Naidoo
Route: /committee/decisions-due → /access-denied
Do: Look for Decisions Due in the nav, then type its address.
Expect: His nav has Decision Panels and Committee Reviews but no Decisions Due. The typed address lands on the
  access-denied page: the page is for the roles that schedule reviews.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step 4.8 — A second review of Dr Molefe for the same period is refused
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Schedule another Annual progression review of Dr Molefe before the same panel for `<P>`.
Expect: Refused, and nothing is created. The refusal names the open review: "Review #N already puts this trainee before
  Paed Annual Review Panel for `<P>` (scheduled, scheduled `D`). A trainee has one binding review for each period …".
  The wording is a product decision (T131 slice 4). The list still holds three rows.
Actual:
Gap:

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
Actual:
Gap:

### Step 4.10 — Dr Sithole schedules Dr Ndlovu's review from what is due
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions Due, choose Schedule on one of Dr Ndlovu's rows. Set Scheduled on `D` and create the review.
Expect: As Step 4.9: the form is filled, the review opens as Scheduled, and Start is not offered. Decisions Due then
  reads Scheduled on all 75 rows, and By EPA reads Scheduled 5 and To schedule 0 for every EPA.
Actual:
Gap:

### Step 4.11 — Dr Naidoo lists the panel's reviews
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews, then open Dr Dlamini's review.
Expect:
  - **The list:** its subtitle reads "Open existing committee reviews.", and no Schedule review button is offered. It
    lists the five reviews of the panel he sits on, all Scheduled, with Decision Pending.
  - **Dlamini's review:** Start review is offered to him, as a member who may sit on the panel. He leaves it.
Actual:
Gap:

### Step 4.12 — Prof Mbatha checks the schedule
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/decisions-due
Do: Read Committee Reviews, then Decisions Due.
Expect: Committee Reviews offers Schedule review and lists all five reviews. Decisions Due reads Scheduled on all 75
  rows. Each row's "Open review" opens the review holding it, and no row offers Schedule.
Actual:
Gap:

### Step 4.13 — The panel cannot change what it decides while its reviews are open
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels → /committee/panels/{PanelId:int}
Do: Open the panel. In Decides for, choose the Neonatal team Clinical Competency Committee and save the committee.
Expect: Refused, and the panel stays a General panel. The refusal names the first open review: "Review #N (`<P>`,
  scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the
  panel sat as when it was scheduled …" (T131 slice 5). The select's help text says the same rule before she tries.
Note: If the save is accepted, set Decides for back to General panel before going on, and record the defect.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step 4.18 — An expiry before the issue date is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start staging PAED-010: choose its Stage, then level `4`, Issued on `D`, Expires on `D−1`, the rationale "Leads the
  ward round safely; one observation so far, to be confirmed at her final review." and both PAED-010 lines (the Direct
  Observation and the MSF record). Stage it.
Expect: Refused with "An expiry date must be after the issue date." Nothing is staged, the form keeps what was typed,
  and PAED-010 still reads Due.
Actual:
Gap:

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
Actual:
Gap:

### Step 4.20 — A second decision on PAED-001 is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: In the staging form, choose PAED-001 from the EPA select, then fill in level `4`, a rationale and one line. Stage
  it.
Expect: Refused with "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage
  another." The pending list still holds exactly one PAED-001 decision, at `5`.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step 4.23 — Recording without a second member present is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose the category Satisfactory Progress and write a rationale. Tick nobody besides herself under Present, and
  record the decision.
Expect: Refused with "A committee decision needs at least two panel members present: the chair and at least one
  other." (T165, D46). The review stays In progress, with nothing recorded.
Actual:
Gap:

### Step 4.24 — Recording without a category is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Tick David Naidoo, Sarah Botha and John van Rensburg as present. Set Category back to "Select a category…" and
  record.
Expect: Refused with "This review decides the trainee's progression, so its decision records a progression category.
  Choose one." (T131 slice 5). Nothing is recorded.
Actual:
Gap:

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
Actual:
Gap:

### Step 4.26 — Prof Mbatha reads the decided review, and is not offered Ratify
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Between the recording and the ratifying, open Dr Molefe's review from Committee Reviews.
Expect: The list reads Decided, Satisfactory Progress, for her review. The review shows the decision with its
  "Present:" line and the three staged decisions under "Fixed when the committee's decision was recorded …". It offers
  no Ratify, no Remove and no Action column. The Review card says "Only the panel's chair, Thandi Zulu, can ratify the
  committee's decision.": only the chair ratifies, with no administrator's bypass (D46).
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
  - **The snapshot** holds his two teaching sessions logged this year, his CBD still Requested and his portfolio review
    still Awaiting review, each with its state (T138). The session logged for `J−1y−56d` is outside the window and not
    in it.
  - **Rating trajectory:** "No rating by a named assessor to chart for this trainee."
  - **Recorded:** the decision reads "Satisfactory with Observations", with "Conditions: At least two Mini-CEX before
    the next sitting." and "Present: Thandi Zulu (chair), David Naidoo".
  - **Ratified:** the state reads Ratified, and the pending card says "Nothing was staged when the review was ratified,
    so ratifying it issued no STAR."
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step 4.38 — Prof Mbatha reads the sitting in the audit log
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Filter Action to RecordCommitteeDecisionCommand. Open one of the failed entries and one successful entry. Then
  filter to RevokeEntrustmentDecisionCommand.
Expect:
  - **The failed entries:** two FAILED rows by Dr Zulu, from Steps 4.23 and 4.24, whose errors name the quorum rule and
    the missing category. Each refusal wrote its row and changed nothing else: every check runs before the first change.
  - **The successful entries:** five OK rows, one per review.
  - **An entry's summary:** it keeps ReviewId, Category and PresentUserIds in the clear, and shows Rationale and
    Conditions as "[REDACTED]" (T101).
  - **The revocation's entry:** its Reason is "[REDACTED]", and its actor is Dr Mokoena.
Actual:
Gap:

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
Actual:
Gap:

### Step 4.40 — Dr Molefe reads her progress against Annexure A
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/progress
Do: Open My Progress and read "Entrustment against Annexure A".
Expect: It says "You are in training year 4 on `D`." It then reads "2 at or above · 1 below · 12 with no decision, of
  15 EPAs". The table shows:
  - PAED-001 and 012 at `5`, each "At or above" the target of `5`, with its exit level Reached;
  - PAED-010 at `4`, "Below" the target of `5`, with its issue and expiry dates.
  The exit rule says it gates nothing. The page reads the same table as the committee's (T166).
Actual:
Gap:

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
Actual:
Gap:

### Step 4.42 — Dr Dlamini reads what stands after the revocation
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/authorisations → /committee/my-reviews
Do: View her authorisations, then her review.
Expect: One card, PAED-001 at `4`: the revoked PAED-002 is no longer listed. Her review reads Ratified, Satisfactory
  Progress. The agenda still reads Decided with STAR numbers on PAED-001 and 002, because it records what the sitting
  did.
Actual:
Gap:

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
Actual:
Gap:

### Step 4.44 — Dr Naidoo sees who hears the appeal
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review.
Expect: The list shows her review Under appeal. The Appeals card lists the appeal, with the date lodged, its reason and
  "(Open)". It offers him no form. It says "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu,
  and its external member, John van Rensburg." (T213, T237)
Actual:
Gap:

### Step 4.45 — Dr van Rensburg, the external member, is offered the appeal
Role: CommitteeMember (external) — Dr John van Rensburg
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews, then Dr Mahlangu's review. Read the resolve form, choose Remitted to see what it asks, and
  leave without resolving.
Expect:
  - **The list:** the panel's five reviews.
  - **The Appeals card** offers him the resolve form with an Outcome select: Upheld, Dismissed or Remitted.
  - **Remitted** reveals:
    - Replacement category, opening on "Select a category…";
    - Replacement rationale;
    - Present, with both Thandi Zulu (chair) and himself ticked and locked.
  - **Nothing changes** until he submits.
Actual:
Gap:

### Step 4.46 — A remit without a quorum is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: On Dr Mahlangu's review, choose Remitted, the replacement category Satisfactory with Observations and a
  rationale. Tick nobody besides herself as present, and resolve.
Expect: Resolve appeal is refused with "A committee decision needs at least two panel members present: the chair and
  at least one other." (T165: a remit records its own quorate sitting). The review stays Under appeal.
Actual:
Gap:

### Step 4.47 — Dr Zulu remits the decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Keep Remitted, Satisfactory with Observations, and the rationale "The stalled Mini-CEX was the assessor's delay;
  progress is adequate, with observed assessments to follow." Tick Sarah Botha as present, and resolve the appeal.
Expect: "Appeal resolved." The state reads Closed (T250).
  - **Decision card:** the replacement heads it, "Satisfactory with Observations" with "Present: Thandi Zulu (chair),
    Sarah Botha". The first decision stays below it, marked "Replaced on appeal by the decision above.".
  - **Appeals card:** the appeal reads "(Remitted)".
  - **No STAR changed:** the review issued none.
Actual:
Gap:

### Step 4.48 — Dr Mahlangu reads the outcome
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: Open My Committee Reviews and view the review.
Expect: The row reads State Closed and Decision Satisfactory with Observations. The detail's current decision is the
  replacement, with "Present when this decision was taken: Thandi Zulu (chair), Sarah Botha". No appeal form is
  offered.
Actual:
Gap:

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
Actual:
Gap:

### Step 4.50 — Dr Zulu starts and closes the check-in
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the formative review, read it, and close it.
Expect:
  - **Started:** "Review started." The snapshot freezes the same three activities as his annual review, and the standing
    and MSF coverage cards follow it. Close review is offered to her alone.
  - **Closed:** "Formative review closed." The state reads Closed, and nothing is offered.
  - **Other readers** are told "Only the panel's chair, Thandi Zulu, can close this review." while it is in progress.
Actual:
Gap:

### Step 4.51 — Dr Ndlovu reads both reviews
Role: Trainee — Dr Sipho Ndlovu
Route: /committee/my-reviews
Do: Open My Committee Reviews.
Expect: Two rows for `<P>`. The annual review reads Ratified, Outcome Deferred. The check-in reads Closed, with Decision
  "No binding decision".
Actual:
Gap:

## Outcome state

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

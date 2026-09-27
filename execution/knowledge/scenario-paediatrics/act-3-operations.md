# Act 3 — Months 1–6: operational rhythm

**Scenario date.** Monday 2 February to Tuesday 30 June 2026: the first semester of Dr Mahlangu's and Dr Ndlovu's first
year. The dates are narrative only. The steps are played on one replay day, `D`, and every date they type is written
relative to it (see "The clock in this act").

**Who acts.** The five registrars file workplace-based assessments. The five consultants rate, decline, return and
complete them. Mr Smit runs an MSF campaign for Dr Molefe and watches for stalled work. Seven colleagues are asked to
answer that campaign anonymously, and six do. devadmin runs the assessor nudge. Every staff role reads its dashboard,
and Prof Mbatha and devadmin read the audit trail.

**Why.** Acts 1 and 2 built the skeleton. This act runs the loop the product exists for: file, rate, complete, credit.
It also runs the loop's detours: a refusal, a decline, a return, an abandoned draft, a stall and a withdrawn campaign.
Credit (`CreditApplier`) is what fills the progress pages and the staff dashboards. The evidence it leaves is what Act 4's
committee decides on.

**Starting state (after Act 2).**
- KGK has adopted `Paediatric EPA Curriculum` 11.1. The ten hand-filed `*_cpsa` instruments and Act 1's `KGK Teaching
  Session Log` (`kgk_teaching_log`) are published. `msf_cpsa` and `learner_feedback_cpsa` are written by the system and
  offered to nobody.
- Everyone in the cast is registered and has their roles:
  - Dr Zulu, Dr Naidoo and Dr Botha hold CommitteeMember and Assessor.
  - Dr Patel and Dr Khumalo hold Assessor.
  - Dr van Rensburg holds CommitteeMember only.
  - Mr Smit is the Coordinator.
  - Dr Mokoena is the SpecialityAdmin and Dr Sithole the SubSpecialityAdmin, both for Paediatrics.
- The five registrars are admitted to curriculum 11.1 at KGK. Their programmes started on `J−3y` (Molefe), `J−2y`
  (Dlamini), `J−1y` (du Plessis) and `J` (Mahlangu and Ndlovu), Act 2's anchor: `J` is the latest 15 January on or
  before `D`. On a replay in 2026 those are 15 January 2023, 2024, 2025 and 2026.
- The review panel exists. No activity, MSF questionnaire, MSF campaign or curriculum progress row exists.

**Goal.**
1. A Mini-CEX goes from draft to requested to completed, credits PAED-001 for Dr Dlamini, and shows on My Activities
   and My progress.
2. Filing is refused for a future date and for a date before the programme started. A late filing is warned about and
   recorded. The EPA and assessor pickers offer only what the write path accepts.
3. A request is declined and filed again. A reflection is returned and submitted again.
4. A self-logged record (KGK's teaching log) and a DOPS are filed, and a draft is abandoned.
5. Stalled work reaches the coordinator, and the assessors are nudged.
6. One MSF campaign runs from questionnaire to release and coverage, and one is withdrawn.
7. Dr Molefe and Dr Dlamini hold enough completed evidence for Act 4 to stage STARs.
8. Every staff dashboard reads real figures, and the audit trail records the act.

## The clock in this act

- **`D` is the day the act is played**, in one sitting. Every encounter date typed is `D−1` to `D−20`, apart from
  three: `D+1` and `J−1d`, which must be refused (Steps 3.8 and 3.9), and `J−1y−56d`, a teaching session from before
  Dr du Plessis's programme, which is accepted (Step 3.19).
- **The expectations are written for `D` between 21 July and 30 November.** In that window `J` is 15 January of `D`'s
  year, and:
  - every encounter falls in the semester containing `D`, semester 2 (July to November, nominal end 30 November). The
    previous semester is semester 1. The names below are for a replay in 2026 ("Semester 2, 2026"); in a later year,
    read `D`'s year wherever 2026 is written.
  - The training years read 4 (Dr Molefe), 3 (Dr Dlamini), 2 (Dr du Plessis) and 1 (Dr Mahlangu and Dr Ndlovu), since
    Act 2 wrote every programme start from `J`.
  - No page shows December's "academic year ended" notice.
  - Outside the window, read the semester names off the page, and record a Gap only where a count differs.
- **Two waits are real:**
  - The MSF campaign page counts a link as not delivered one hour after it was sent (Step 3.43).
  - Stalled work needs a request left untouched for more than seven days. Step 3.30 says how to age it in SQL when the
    act is played in one sitting.
- **The nudge job also runs on its own**, daily at 09:00 UTC. If the sitting crosses that time, its mail may arrive
  before Step 3.32.

## Records this act creates

| Ref | Registrar | Instrument | EPA | Named | Encounter | Step | Ends Act 3 as |
|---|---|---|---|---|---|---|---|
| R1 | Dlamini | Mini-CEX | PAED-001 | Naidoo | D−10 | 3.1–3.5 | Completed, rung 4 |
| R2 | Ndlovu | Mini-CEX | PAED-002 | Khumalo | D−20 | 3.8–3.11 | Declined |
| R3 | Ndlovu | Mini-CEX | PAED-002 | Botha | D−20 | 3.12–3.13 | Completed, 3a |
| R4 | Ndlovu | Reflective Exercise | PAED-001 | Botha | D−20 | 3.14–3.17 | Discussed |
| R5–R7 | du Plessis | KGK Teaching Session Log | PAED-015, 004, 015 | — | D−6, D−13, J−1y−56d | 3.18–3.19 | Logged |
| R8 | du Plessis | DOPS | PAED-002 | — | — | 3.20 | Cancelled |
| R9 | du Plessis | CBD | PAED-002 | Khumalo | D−5 | 3.21 | Requested (open) |
| R10 | du Plessis | Portfolio and Logbook Review | PAED-015 | Patel | D−1 | 3.22 | Awaiting review (open) |
| R11 | Mahlangu | DOPS | PAED-002 | Patel | D−8 | 3.23–3.24 | Completed, 3a |
| R12–R17 | Molefe | six WBAs | PAED-001 ×3, 012 ×2, 010 | five consultants | D−12 to D−4 | 3.25–3.26 | Completed |
| R18–R20 | Dlamini | CBD, CCA, Mini-CEX | PAED-001 ×2, 004 | Zulu, Botha, Khumalo | D−9, D−6, D−2 | 3.27–3.28 | Completed |
| R21 | Mahlangu | Mini-CEX | PAED-004 | Zulu | D−3 | 3.29–3.33 | Completed, 3a, after a stall |
| R22–R23 | Molefe | Multi-Source Feedback (system) | PAED-010, 012 | — | D (the close) | 3.46 | Recorded |

## Phase 3.A — Dr Dlamini's Mini-CEX, from draft to credit

### Step 3.1 — Dr Dlamini starts a Mini-CEX and saves it as a draft
Role: Trainee — Dr Anele Dlamini
Route: / → /activities/new
Do: From the dashboard, log an activity and choose Mini-CEX (Paediatrics). Fill in the EPA PAED-001, the assessor Dr
  David Naidoo, the date observed `D−10`, the setting Emergency unit and the complexity Moderate. Leave the presenting
  problem empty, and save the draft.
Expect: The type select offers eleven types, and no name appears twice: the ten hand-filed `*_cpsa` instruments and KGK
  Teaching Session Log. Multi-Source Feedback and Learner Feedback are not offered (T162). The form has three sections:
  Request, Entrustment and Feedback. Only Request can be filled in; the other two are locked. The EPA picker offers the
  nine EPAs whose list names the Mini-CEX: PAED-001 to 004, 006 to 008, 012 and 013. It does not offer PAED-010 (T122).
  The Assessor picker offers exactly KGK's active Assessors, by name and address: Dr Botha, Dr Khumalo, Dr Naidoo,
  Dr Patel and Dr Zulu. It never offers Dr Dlamini or another registrar, nor Prof Mbatha, Mr Smit, Dr Mokoena,
  Dr Sithole or Dr van Rensburg (T102). No lateness warning shows for `D−10`. Saving opens the activity's page, State:
  Draft, with "Draft saved. It has not been submitted." (T127).
Actual (2026-09-26, T295 replay, wombat_scenario): Type select offers the 11 types once each, no MSF or Learner
  Feedback. Request/Entrustment/Feedback, the last two disabled. EPA picker 001-004, 006-008, 012, 013; Assessor picker
  Botha, Khumalo, Naidoo, Patel, Zulu by name and address. No warning for 2026-09-16. Saved as activity 1, State: Draft,
  "Draft saved. It has not been submitted."
Gap: none

### Step 3.2 — Dr Dlamini reopens the draft, and a submit with a field missing is refused
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Find the draft in My activities and open it. Press Submit without filling in the presenting problem.
Expect: My activities lists the draft as Mini-CEX (Paediatrics), PAED-001 with its title, encounter date `D−10`, Draft,
  credited "—". On the draft's page, the Request fields are open. The page offers Submit and Cancel, and Discard changes
  stays disabled until something is typed. The history has one row: Create (Draft → Draft), by Dr Dlamini. The submit
  is refused with "Presenting problem: A value is required.", the field is marked (T263), and the activity stays a
  draft.
Actual (2026-09-26, T295 replay, wombat_scenario): My Activities: one row, Mini-CEX (Paediatrics), PAED-001 with its
  title, 2026-09-16, Draft, credited "—". Page offers Submit and Cancel; Discard changes disabled; history one row,
  Create Draft → Draft, Anele Dlamini. Submit refused: alert "Presenting problem: A value is required.", field [invalid];
  DB still draft, one transition.
Gap: none

### Step 3.3 — Dr Dlamini completes the request and submits it
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Type the presenting problem "Two-year-old with bronchiolitis and rising work of breathing" and submit.
Expect: State: Requested. The page is now read-only to her, and Cancel is the only action. The history adds Submit
  (Draft → Requested). It carries no lateness note, because ten days is on time (D15). No email is sent: Wombat mails
  nobody when an activity moves, so Dr Naidoo learns of the request from his inbox.
Note: The AssessmentRequested, AssessmentCompleted and AssessmentDeclined email templates exist, but nothing in the
  product sends them.
Actual (2026-09-26, T295 replay, wombat_scenario): State: Requested; every field disabled; Cancel the only action.
  History adds Submit Draft → Requested, Anele Dlamini, note "—". No mail reached the sink and no "Stub email" in the
  log.
Gap: none

### Step 3.4 — An assessor who was not named cannot open the request
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Look for Dr Dlamini's Mini-CEX in the inbox, then open its address directly, using the id from Step 3.1.
Expect: The inbox reads "Inbox clear", because no request names him yet. The activity's page reads "Activity
  unavailable", exactly as it does for an id that does not exist, so walking ids discloses nothing (T101).
Actual (2026-09-26, T295 replay, wombat_scenario): Inbox: "Inbox clear. There are no activities waiting for your
  action." /activities/1 and /activities/99999 both read "Activity unavailable. The requested activity could not be
  loaded.", text identical.
Gap: none

### Step 3.5 — Dr Naidoo rates the Mini-CEX and completes it
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Dlamini's Mini-CEX in it.
  Rate the supervision the encounter required at rung 4, write what was done well, the areas for development and the
  agreed plan, and complete it.
Expect: The inbox row names Anele Dlamini, PAED-001, `D−10` and Requested. On the page, only Entrustment and Feedback
  can be filled in, and Complete and Decline are offered. The rating offers the six rungs by the College's labels, 1, 2,
  3a, 3b, 4 and 5 (D32). After Complete, the state is Completed and the page is read-only. The history's Complete row
  reads Requested → Completed, by David Naidoo, credited "1 item". Year 3's minimum on PAED-001 is rung 4, so the
  encounter counts at the minimum.
Note: Dr Naidoo also holds CommitteeMember, so his sessions open acting as a Committee member, whose menu has no
  Activity inbox. The switch is stored with his account, so he acts as Assessor until Step 4.5 switches back.
Actual (2026-09-26, T295 replay, wombat_scenario): Home opened "Viewing as CommitteeMember". Inbox row: Mini-CEX,
  Anele Dlamini, PAED-001, 2026-09-16, Requested. Request disabled; Entrustment and Feedback open; Complete and Decline.
  Rating options 1, 2, 3a, 3b, 4, 5. After Complete: Completed, nothing enabled; history Complete Requested → Completed,
  David Naidoo, "1 item". Progress row PAED-001 2026 S2: count 1, at minimum 1.
Gap: none

### Step 3.6 — Dr Dlamini sees the completed Mini-CEX in My activities
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Open My activities, then the Mini-CEX.
Expect: The row reads Completed, credited "1 item". The page is read-only. It shows rung 4 and Dr Naidoo's feedback, and
  offers no action. The history holds Create, Submit and Complete, with Dr Naidoo as the actor of the Complete.
Actual (2026-09-26, T295 replay, wombat_scenario): Row: Mini-CEX, PAED-001, 2026-09-16, Completed, "1 item". Page: every
  field disabled, rating 4 and Dr Naidoo's three feedback texts shown, no action button. History Create, Submit,
  Complete (David Naidoo, "1 item").
Gap: none

### Step 3.7 — Dr Dlamini's progress counts the Mini-CEX against this semester's target
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress
Do: Open My progress.
Expect: The "This period" card reads:
  - "Semester 2, 2026 · July to November";
  - semester targets "0 of 10 EPAs met this semester" and yearly targets "0 of 5 EPAs met in 2026";
  - training year 3;
  - multi-source feedback "0 of 15 EPAs covered…".
  The PAED-001 card reads "1 of 3 this semester", with a bar, then "2 more by 30 November 2026. At the minimum level when
  observed: 1 of 1. Last encounter date: `D−10`." Below that come "Target: 3 per semester (6 a year). Minimum now 4."
  and "Semester 1, 2026: 0 of 3, 3 short". The Entrustment section shows no STAR yet. The trajectory charts PAED-001:
  1 observation from 1 distinct assessor.
Note: A figure is always "n of m" for a named window, never a lifetime total (T130, DESIGN.md § progress figures).
Actual (2026-09-26, T295 replay, wombat_scenario): This period: "Semester 2, 2026 · July to November", "0 of 10 EPAs met
  this semester", "0 of 5 EPAs met in 2026", training year 3, MSF "0 of 15 EPAs covered…". PAED-001 "1 of 3 this
  semester", "2 more by 30 November 2026. At the minimum level when observed: 1 of 1. Last encounter date: 2026-09-16.",
  "Target: 3 per semester (6 a year). Minimum now 4.", "Semester 1, 2026: 0 of 3, 3 short". No STAR (15 with no
  decision). Trajectory PAED-001: "1 observation from 1 distinct assessor." Her yearly cards also carry "Your training
  year changed on 14 Jan 2026" (365-day blocks from 2024-01-15 cross the leap day, as for Molefe).
Gap: none

## Phase 3.B — Filing rules, a decline and a re-filing

### Step 3.8 — A future encounter date is refused
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new
Do: Start a Mini-CEX (Paediatrics) for an encounter on the ward. Fill in the EPA PAED-002, the assessor Dr Fatima
  Khumalo, the setting Ward, the presenting problem "Nine-month-old with gastroenteritis and moderate dehydration" and
  the complexity Low. By mistake, type the date observed as `D+1`. Submit.
Expect: The submit is refused with "Nothing was saved. Date observed: The date cannot be after today (`D`)." The date
  field is marked and names the alert, and everything typed is kept (T160, T263). No activity exists.
Actual (2026-09-26, T295 replay, wombat_scenario): Alert "Nothing was saved. Date observed: The date cannot be after
  today (2026-09-26)." Date field aria-invalid, aria-describedby includes activity-refusal; every typed value kept.
  Still only activity 1 in the database.
Gap: none

### Step 3.9 — A date before the programme started is refused
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new
Do: Change the date observed to `J−1d`, the day before his programme started (2026-01-14 on a replay in 2026), and
  submit again.
Expect: As the date is typed, the field is marked, and below it reads "This date is before the trainee's programme
  started (`J`), and will not be accepted." The submit is refused with "Nothing was saved. Date observed: The date
  cannot be before the trainee's programme started (`J`)." A Mini-CEX can credit, so its date is held to the programme
  start (T160, T192). No activity exists.
Actual (2026-09-26, T295 replay, wombat_scenario): Typing 2026-01-14 marked the field and showed "This date is before
  the trainee's programme started (2026-01-15), and will not be accepted." (the 3.8 alert stayed until the submit).
  Submit refused: "Nothing was saved. Date observed: The date cannot be before the trainee's programme started
  (2026-01-15)." No activity created.
Gap: none

### Step 3.10 — A late filing is warned about and recorded, never refused
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new → /activities/{ActivityId:int}
Do: Change the date observed to `D−20`, the day the encounter happened, and submit.
Expect: No lateness warning shows yet. The field still carries the last refusal's mark, which stays until the next action
  (T263), and a refused date is never called fileable. Step 3.12's fresh form shows the warning. The submit succeeds:
  "Submitted. It is now Requested." In the history, the Submit row's time carries "Filed 20 days after the encounter"
  (D15, T160).
Actual (2026-09-26, T295 replay, wombat_scenario): Typing 2026-09-06 cleared the hint and showed no lateness warning;
  the field stayed aria-invalid. Submit opened activity 2 (R2) with "Submitted. It is now Requested."; the Submit row's
  time reads "2026-09-26 12:18 Filed 20 days after the encounter".
Gap: none

### Step 3.11 — Dr Khumalo declines the request, with a reason
Role: Assessor — Dr Fatima Khumalo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Ndlovu's Mini-CEX. Press Decline and apply it with the note left empty. Then apply it with the note "I was
  not on the ward that day; Dr Botha observed this encounter. Please send it to her."
Expect: Decline opens a transition note. Applied with the note empty, it is refused with "Decline requires a note."
  Applied with the note, the state is Declined. The page is read-only, with no action left. The history's Decline row
  shows her note, credited "—". Her inbox no longer lists the request.
Actual (2026-09-26, T295 replay, wombat_scenario): Inbox row: Sipho Ndlovu, PAED-002, 2026-09-06, Requested. Decline
  opened "Transition note" with Cancel and Apply. Empty Apply: alert "Decline requires a note.", still Requested, and the
  note panel closed, so Decline had to be pressed again. With the note: State Declined, nothing enabled; Decline row
  Requested → Declined, Fatima Khumalo, "—", her note. Her inbox: "Inbox clear".
Gap: [F-3.11a, T299] A refused note-carrying move closes the note panel, so the refusal's alert names a note field that is no
  longer on screen, and a note typed for a move refused for another reason is thrown away.
  (ActivityWorkflowActions.ConfirmSelectedTransitionAsync calls ClearSelection whatever the outcome.) Low severity.

### Step 3.12 — Dr Ndlovu reads the decline and files the encounter again, naming Dr Botha
Role: Trainee — Dr Sipho Ndlovu
Route: / → /activities/{ActivityId:int} → /activities/new → /activities/{ActivityId:int}
Do: Open the declined Mini-CEX from the dashboard's Recent activities. Then file the same encounter as a new Mini-CEX,
  `D−20`, PAED-002, naming Dr Sarah Botha, and submit.
Expect: Recent activities lists the Mini-CEX with a red Declined badge. The Activity inbox card does not list it, as the
  inbox it opens does not: nothing can move a declined request on (T297). No mail tells him of the decline (T320), so
  Recent activities, while it is among his five newest, and My activities are where he finds it. Its page reads
  State: Declined, offers no action, and shows Dr Khumalo's note in the history. The re-filing is a new activity. As
  `D−20` is typed, the field warns: "This encounter was 20 days ago. It can still be filed, but a filing more than 14
  days after the encounter is recorded as late." Its own Submit row records "Filed 20 days after the encounter". It
  ends Requested, and the Activity inbox card lists it, as the inbox does: he may still cancel it.
Note: Declined is a dead end by design: the seed's workflow has no move out of it. The declined record is kept.
Actual (2026-09-26, T295 replay, wombat_scenario): Home's Activity inbox card: "Mini-CEX (Paediatrics) Declined",
  linking to /activities/2. That page: State: Declined, nothing enabled, Dr Khumalo's note in the Decline row. The new
  form warned "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the
  encounter is recorded as late." Submitted as activity 3 (R3), Requested, Submit row "Filed 20 days after the
  encounter". Afterwards /activities/inbox lists only activity 3, while the Home card lists 3 (Requested) and 2 (Declined).
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): after the decline, Home's Activity inbox card read "No
  pending items." and /activities/inbox "Inbox clear"; Recent activities listed the Mini-CEX with a red Declined badge,
  linking to /activities/2, which reads State: Declined, offers no action and shows Dr Khumalo's note. The new form
  warned as `D−20` was typed; activity 3 ended Requested with "Filed 20 days after the encounter", and the card then
  listed only "Mini-CEX (Paediatrics) Requested", the inbox's one row.
Gap: [F-3.12a, T297] fixed by T297 (7bf8ea7): the card lists what the inbox lists, and the Expect (rewritten by T297)
  finds the decline on Recent activities. [F-3.12b, T280] The Activity inbox card is itself a link to /activities/inbox that
  wraps its row links and "Open inbox →" (My authorisations wraps "View authorisations →" the same way): a link nested
  in a link, announced as one link (DashboardCard's Href, since T011; not new with T297).

### Step 3.13 — Dr Botha completes Dr Ndlovu's Mini-CEX
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Ndlovu's re-filed Mini-CEX
  in it. Rate it 3a with feedback and complete it. Then choose Switch to Committee member in the sidebar.
Expect: The Mini-CEX is Completed, credited "1 item". Year 1's minimum on PAED-002 is 3a, so it counts at the minimum.
  The declined request stays Declined and credits nothing. Each switch lands on Home with its one-time info alert, "You
  are now acting as Assessor." and then "You are now acting as Committee member." (Step 2.34), and the sidebar ends
  reading "Acting as Committee member".
Note: Dr Botha also holds CommitteeMember, so her sessions open acting as a Committee member, whose menu has no Activity
  inbox, and in-app navigation never switches the role (R2-Rules § 1). The switch is stored with her account, so she
  switches to rate and back after. Every step that sends her or Dr Zulu to rate does the same, which keeps the frames the
  later steps read: Step 3.33's first "Acting as Committee member", Step 4.14's Decision panels and Step A.5.11's head.
Actual (2026-09-26, T295 replay, wombat_scenario): Botha's inbox held only activity 3. Rated 3a with feedback:
  Completed, Complete row by Sarah Botha, "1 item". Progress: Ndlovu PAED-002 2026 S2 count 1, at minimum 1. Activity 2
  stays declined, and no progress row names it.
Gap: none

## Phase 3.C — A reflection returned for more detail

### Step 3.14 — Dr Ndlovu submits a reflective exercise to Dr Botha
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Reflective Exercise (Paediatrics) on a critical incident. Fill in the EPA PAED-001, the supervisor
  Dr Botha and the date of the incident `D−20`. For what happened, write "A three-year-old in septic shock waited
  90 minutes for antibiotics after triage". Add the analysis, what he learned and a one-line plan, then submit.
Expect: The EPA picker offers only PAED-001, 003, 008 and 014, the EPAs whose list names the reflective exercise (D45).
  The form has no Entrustment section, because the exercise is unrated (D6), and its Discussion section is locked. No
  lateness warning shows for `D−20`: a reflective exercise credits nothing, so its filing is late for nobody (T160).
  The submit reads "Submitted. It is now Awaiting discussion.", and the Submit row carries no lateness note.
Actual (2026-09-26, T295 replay, wombat_scenario): EPA picker PAED-001, 003, 008, 014. Sections Reflection (EPA,
  supervisor, date, "Reflecting on" = A critical incident, what happened, what went well/differently, what I learned,
  what I will do differently) and Discussion (disabled); no Entrustment. No warning for 2026-09-06. Activity 4 (R4):
  "Submitted. It is now Awaiting discussion."; Submit row Draft → Awaiting discussion, no lateness note.
Gap: none

### Step 3.15 — Dr Botha returns the reflection for more detail
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Dr Ndlovu's reflection from Activity inbox. Return it with the
  note "Please say what you would do differently at triage, and what you will read before your next take." Then choose
  Switch to Committee member in the sidebar.
Expect: The inbox lists the reflection as Awaiting discussion. The page offers Record Discussion and Return, and only the
  Discussion field is open. Return asks for a note. Once it is returned, the state is Draft and the page offers
  Dr Botha nothing. The history's Return row (Awaiting discussion → Draft) carries her note.
Actual (2026-09-26, T295 replay, wombat_scenario): Inbox row: Reflective Exercise, Sipho Ndlovu, PAED-001, 2026-09-06,
  Awaiting discussion. Enabled: Record Discussion, Return and the Discussion field only. Return opened a transition
  note; applied: State Draft, nothing enabled for her; Return row Awaiting discussion → Draft, Sarah Botha, her note.
Gap: none

### Step 3.16 — Dr Ndlovu finds the returned reflection in his inbox and submits it again
Role: Trainee — Dr Sipho Ndlovu
Route: / → /activities/inbox → /activities/{ActivityId:int}
Do: From the dashboard's Activity inbox card, open the inbox, then the reflection. Expand "What I will do differently":
  "Start the sepsis bundle at triage; read the paediatric sepsis guideline before my next take". Submit again.
Expect: The dashboard card lists the reflection as Draft. The inbox page lists it and nothing else of his. His fields are
  open again, with Submit and Cancel. After the submit, the state is Awaiting discussion. The history reads Create,
  Submit, Return and Submit, and neither Submit row carries a lateness note. Home's Activity inbox card then lists the
  reflection as Awaiting discussion, as the inbox does (he may still cancel it), and not the declined Mini-CEX (T297).
Note: A re-submission after a return is not a new filing (`Workflow.LeftInitialStateLeadingOn`, `ActivityService.IsTheFiling`).
  On this type nothing shows it, because a reflective exercise records no lateness at all.
Actual (2026-09-26, T295 replay, wombat_scenario): Home card: Reflective Exercise Draft and Mini-CEX Declined. Inbox:
  the reflection only. His eight Reflection fields open, with Submit and Cancel. Re-submitted: Awaiting discussion;
  history Create, Submit, Return, Submit, no lateness note. Afterwards the inbox still lists the reflection (he may
  cancel it), but the Home card lists only the Declined Mini-CEX.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): Home's card listed only "Reflective Exercise (Paediatrics)
  Draft", not the Declined Mini-CEX (which Recent activities still shows), and "Open inbox →" opened /activities/inbox
  with that one row. Re-submitted: Awaiting discussion; history Create, Submit, Return, Submit, no lateness note. The
  card and the inbox then each listed the reflection alone, Awaiting discussion.
Gap: [F-3.16a, T297] fixed by T297 (7bf8ea7).

### Step 3.17 — Dr Botha records the discussion
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open the re-submitted reflection from Activity inbox, write the
  discussion notes, and record the discussion. Then choose Switch to Committee member in the sidebar.
Expect: The state is Discussed, finished and read-only to both of them. The Record Discussion row is credited "—":
  the exercise credits nothing (D7), and no "counted towards no curriculum requirement" banner shows (T108). Dr Ndlovu's
  progress does not change.
Actual (2026-09-26, T295 replay, wombat_scenario): State Discussed; nothing enabled for Botha or for Ndlovu. Record
  Discussion row Awaiting discussion → Discussed, Sarah Botha, "—"; no credit banner. Ndlovu's My Progress PAED-001
  still "0 of 3 this semester"; no progress row for the reflection.
Gap: none

## Phase 3.D — Logs, a DOPS and work left open

### Step 3.18 — Dr du Plessis logs a teaching session
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: Choose KGK Teaching Session Log. Fill in the topic "Recognising the sick child: a triage refresher", the EPA
  PAED-015, the date delivered `D−6`, the audience Interns and the learning objectives. Submit.
Expect: The form is Act 1's: one section, Teaching session, with six fields, the first five required and Supervising
  consultant optional, and no rating. The EPA picker offers all 15 PAED EPAs: the type is not a College instrument, so
  no tool list binds it (D21). The submit reads "Submitted. It is now Logged." The Log row is credited "—", and no
  lateness is recorded.
Note: No seeded CPSA instrument is a procedure log; the generic `procedure_log` belongs to the Demo speciality. KGK's
  own self-logged type stands in for procedure-style logging.
Actual (2026-09-26, T295 replay, wombat_scenario): One section, Teaching session: Topic, EPA, Date delivered, Audience,
  Learning objectives (required) and Supervising consultant (optional); no rating. EPA picker all 15. Activity 5 (R5):
  "Submitted. It is now Logged."; Log row Draft → Logged, "—", no lateness note.
Gap: none

### Step 3.19 — Dr du Plessis logs two more sessions, one from before his programme
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: Log two more sessions:
  - "Neonatal jaundice for fourth-year students": PAED-004, `D−13`, Medical students.
  - "Oral rehydration for nursing staff", taught as a medical officer before admission: PAED-015, `J−1y−56d`
    (20 November 2024 on a replay in 2026), Nursing staff.
Expect: Both sessions are Logged. `J−1y−56d` is before his programme started (`J−1y`), and it is accepted with no
  hint. A type that credits nothing is held only to "not after today" (T160).
Actual (2026-09-26, T295 replay, wombat_scenario): Activity 6 (R6, PAED-004, 2026-09-13, Medical students) and
  activity 7 (R7, PAED-015, 2024-11-20, Nursing staff) both "Submitted. It is now Logged."; no hint or warning shown
  for either date.
Gap: none

### Step 3.20 — Dr du Plessis abandons a draft started on the wrong instrument
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int} → /activities/mine
Do: Start a DOPS (Paediatrics) by mistake. Choose only the EPA PAED-002 and save the draft. Then cancel it from its page.
Expect: The draft saves with its other required fields empty, because saving a draft checks formats only (T105). Cancel
  asks for no note and moves the draft to Cancelled, with nothing offered after it. My activities lists it as Cancelled,
  credited "—", beside the three Logged sessions.
Actual (2026-09-26, T295 replay, wombat_scenario): Activity 8 (R8) saved with only the EPA: "Draft saved. It has not
  been submitted." Cancel asked for no note and moved it to Cancelled, nothing enabled; history Create, Cancel
  (Draft → Cancelled). My Activities: DOPS PAED-002 "not recorded (created 2026-09-26)" Cancelled "—", then the three
  Logged sessions (2026-09-20, 2026-09-13, 2024-11-20), all "—".
Gap: none

### Step 3.21 — Dr du Plessis asks Dr Khumalo for a CBD
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Case-Based Discussion (Paediatrics). Fill in the EPA PAED-002, the assessor Dr Khumalo, the date `D−5`, the
  case discussed (a toddler with a first febrile seizure) and the focus Clinical reasoning. Submit.
Expect: The CBD ends Requested. It stays that way to the end of the act; Step 3.51 reads it from Dr Khumalo's side.
Actual (2026-09-26, T295 replay, wombat_scenario): CBD EPA picker 001-005, 008, 009, 011-015; focus options include
  Clinical reasoning. Activity 9 (R9), Khumalo, 2026-09-21: "Submitted. It is now Requested."
Gap: none

### Step 3.22 — Dr du Plessis asks Dr Patel to review his portfolio
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Portfolio and Logbook Review (Paediatrics). Fill in the EPA PAED-015, the reviewer Dr Patel, the review
  period `D−60` to `D−1`, and the note for the reviewer "Three teaching sessions logged; please review them with my
  logbook". Leave the export's file name empty, since it is optional. Submit.
Expect: The EPA picker offers PAED-015 only, the one EPA whose list names the portfolio review (D45). The form has no
  rating, and its Review section is locked. The submit reads "Submitted. It is now Awaiting review." The encounter date
  is the period's last day, `D−1`. The review stays Awaiting review to the end of the act.
Actual (2026-09-26, T295 replay, wombat_scenario): EPA picker PAED-015 only. Sections Review request (open) and Review
  (disabled: date, evidence checkboxes, comments, actions); no rating. Period 2026-07-28 to 2026-09-25, export file name
  empty. Activity 10 (R10): "Submitted. It is now Awaiting review."; Encounter date 2026-09-25; Cancel offered to him.
Gap: none

### Step 3.23 — Dr Mahlangu files a DOPS, and the EPA picker follows the tool lists
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int}
Do: File a DOPS (Paediatrics) for a lumbar puncture on a four-month-old with suspected meningitis. First look for
  PAED-010 in the EPA picker. Then choose PAED-002, the assessor Dr Patel, the date `D−8`, the procedure "Lumbar
  puncture", the setting Ward and the complexity Moderate. Submit.
Expect: The EPA picker offers the eight EPAs whose list names the DOPS, PAED-001 to PAED-008. It does not offer
  PAED-010, whose list is Direct observation and MSF only (T122); the old runbook filed this DOPS there. The DOPS ends
  Requested.
Actual (2026-09-26, T295 replay, wombat_scenario): DOPS EPA picker PAED-001 to 008, no PAED-010. Filed PAED-002, Patel,
  2026-09-18, "Lumbar puncture", Ward, Moderate: activity 11 (R11), "Submitted. It is now Requested."
Gap: none

### Step 3.24 — Dr Patel completes Dr Mahlangu's DOPS
Role: Assessor — Dr Mohammed Patel
Route: / → /activities/inbox → /activities/{ActivityId:int}
Do: From the dashboard, open the inbox, then the DOPS. Rate it 3a with feedback, and complete it.
Expect: Home (Assessor view): "Waiting for your rating" is badged 2 and lists the same two rows as the inbox, oldest
  first: Pieter du Plessis's portfolio review (Awaiting review), then Nomsa Mahlangu's DOPS
  (Requested) (T297). The inbox holds those two rows. After Complete, the DOPS is Completed, credited "1 item". Year 1's
  minimum on PAED-002 is 3a, so it counts at the minimum. The portfolio review stays in his inbox, Home's card is then
  badged 1, and Recent decisions lists the DOPS as Completed.
Actual (2026-09-26, T295 replay, wombat_scenario): Home (Assessor view) read "Pending requests 0 assessments awaiting
  review" and "No decisions yet." while the inbox held two rows: Nomsa Mahlangu's DOPS (Requested) and Pieter du
  Plessis's portfolio review (Awaiting review). DOPS rated 3a: Completed, Mohammed Patel, "1 item"; progress Mahlangu
  PAED-002 2026 S2 count 1, at minimum 1. The inbox then held only the portfolio review.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): Home (Assessor view) read "2 assessments awaiting review",
  and Awaiting your review listed "Portfolio and Logbook Review (Paediatrics) — Pieter du Plessis" (Awaiting review),
  then "DOPS (Paediatrics) — Nomsa Mahlangu" (Requested), each linking to its activity. "Review inbox →" opened the
  inbox holding the same two rows (newest first there). DOPS 11 rated 3a: Completed, "1 item". Home then read "1
  assessment awaiting review" (the portfolio review), and Recent decisions "DOPS (Paediatrics) — Nomsa Mahlangu",
  Completed, in green. The "Awaiting review" badge wraps onto two lines beside the long type name (T328's class).
Gap: [F-3.24a, T297] fixed by T297 (7bf8ea7).

## Phase 3.E — Evidence for the annual review

### Step 3.25 — Dr Molefe files six workplace-based assessments
Role: Trainee — Dr Lerato Molefe
Route: /activities/new → /activities/{ActivityId:int} → /activities/mine
Do: File and submit each of these, filling in every Request field. The clinical context below is the presenting
  problem, the procedure, the case discussed or the context:
  - Mini-CEX, PAED-001, Dr Zulu, `D−12`, Emergency unit, complexity High: status epilepticus in a five-year-old.
  - CBD, PAED-001, Dr Naidoo, `D−11`, focus Escalation and safety: a febrile neutropenic child.
  - DOPS, PAED-001, Dr Patel, `D−9`, Emergency unit, complexity High: intraosseous access in a shocked infant.
  - Mini-CEX, PAED-012, Dr Botha, `D−7`, Ward, complexity Moderate: telling parents of a new diagnosis of type 1
    diabetes.
  - CBD, PAED-012, Dr Khumalo, `D−5`, focus Ethics and consent: a transfusion refused on religious grounds.
  - Direct Observation, PAED-010, Dr Zulu, `D−4`, activity observed Ward round: leading the Monday ward round.
Expect: Each ends Requested, with no lateness warning, since all six are within 14 days. My activities lists the six as
  Requested, credited "—". The Direct Observation's EPA picker offers PAED-010, because its list names Direct
  observation. The Mini-CEX and DOPS pickers do not offer it.
Actual (2026-09-26, T295 replay, wombat_scenario): Activities 12-17 (R12 Mini-CEX 001 Zulu, R13 CBD 001 Naidoo, R14
  DOPS 001 Patel, R15 Mini-CEX 012 Botha, R16 CBD 012 Khumalo, R17 Direct Observation 010 Zulu), each "Submitted. It is
  now Requested.", no warning. PAED-010 offered only by the Direct Observation picker. My Activities: six rows,
  Requested, "—". MSF Reports: "No released reports" (state captured).
Gap: none

### Step 3.26 — The consultants complete Dr Molefe's six
Role: Assessor — Dr Thandi Zulu, Dr David Naidoo, Dr Mohammed Patel, Dr Sarah Botha and Dr Fatima Khumalo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Each consultant opens their own requests from Activity inbox and completes them with feedback, rating:
  - Dr Zulu: rung 5 on the PAED-001 Mini-CEX and rung 5 on the PAED-010 observation;
  - Dr Naidoo: rung 5;
  - Dr Patel: rung 4;
  - Dr Botha: rung 5;
  - Dr Khumalo: rung 5.
  Dr Zulu and Dr Botha act as Committee members: each first chooses Switch to Assessor in the sidebar, and after rating
  chooses Switch to Committee member (Step 3.13). Dr Naidoo still acts as Assessor, his choice from Step 3.5.
Expect: Each inbox holds only that assessor's own requests. All six end Completed, each credited "1 item". Year 4's
  minimum is rung 5 on PAED-001, PAED-010 and PAED-012. So Dr Patel's rung 4 counts towards the target, but not at the
  minimum.
Actual (2026-09-26, T295 replay, wombat_scenario): Inboxes held only each assessor's own: Zulu 12 and 17; Naidoo 13;
  Patel 14 plus du Plessis's portfolio review 10; Botha 15; Khumalo 16 plus du Plessis's CBD 9. All six Completed, "1
  item" each. Molefe's progress 2026 S2: PAED-001 3 counted, 2 at minimum; PAED-010 1/1; PAED-012 2/2.
Gap: none

### Step 3.27 — Dr Dlamini files three more
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/{ActivityId:int}
Do: File and submit:
  - CBD, PAED-001, Dr Zulu, `D−9`, focus Management plan: diabetic ketoacidosis in a nine-year-old.
  - Clinical Case Analysis, PAED-001, Dr Botha, `D−6`, case "Anaphylaxis, 7-year-old, emergency unit", documents
    reviewed Admission notes and Progress notes, setting Emergency unit, and the reasoning discussed: why adrenaline
    was repeated before the second-line drugs.
  - Mini-CEX, PAED-004, Dr Khumalo, `D−2`, Neonatal unit, complexity Moderate: a term neonate with respiratory
    distress.
Expect: Each ends Requested.
Actual (2026-09-26, T295 replay, wombat_scenario): Activity 18 (R18, CBD 001 Zulu, 2026-09-17), 19 (R19, CCA 001 Botha,
  2026-09-20; the CCA's date is labelled "Date of the review" and its documents "Documentation reviewed") and 20 (R20,
  Mini-CEX 004 Khumalo, 2026-09-24): each "Submitted. It is now Requested.", no warning.
Gap: none

### Step 3.28 — The consultants complete Dr Dlamini's three
Role: Assessor — Dr Thandi Zulu, Dr Sarah Botha and Dr Fatima Khumalo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Each completes their own from Activity inbox with feedback, rating: Dr Zulu 3b on the CBD, Dr Botha 4 on the CCA,
  and Dr Khumalo 4 on the Mini-CEX. Dr Zulu and Dr Botha first choose Switch to Assessor in the sidebar, and after
  rating choose Switch to Committee member (Step 3.13).
Expect: All three end Completed, each credited "1 item". Dr Dlamini's PAED-001 now holds three encounters this semester.
  Two are at year 3's minimum of rung 4; the CBD at 3b counts towards the target only. Her PAED-004 holds one, at the
  minimum.
Actual (2026-09-26, T295 replay, wombat_scenario): 18 (Zulu, 3b), 19 (Botha, 4) and 20 (Khumalo, 4) each Completed,
  "1 item". Dlamini's progress 2026 S2: PAED-001 3 counted, 2 at minimum; PAED-004 1 counted, 1 at minimum.
  Khumalo's inbox still holds du Plessis's CBD (9).
Gap: none

## Phase 3.F — Stalled work

### Step 3.29 — Dr Mahlangu asks Dr Zulu for a Mini-CEX, which is left unrated
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics): PAED-004, Dr Zulu, `D−3`, Neonatal unit, "Term neonate with jaundice on day three",
  complexity Low. Submit. Dr Zulu does not act on it.
Expect: The Mini-CEX ends Requested.
Actual (2026-09-26, T295 replay, wombat_scenario): Activity 21 (R21) opened with "Submitted. It is now Requested.",
  State: Requested, encounter 2026-09-23, PAED-004, assessor Thandi Zulu. The EPA picker offered the nine Mini-CEX EPAs.
Gap: none

### Step 3.30 — Mr Smit's dashboard shows the stalled requests
Role: Coordinator — Mr Pieter Smit
Route: /
Do: Once Dr Mahlangu's Mini-CEX and Dr du Plessis's portfolio review have waited more than seven days, open the
  dashboard.
Expect: "Stalled requests" is marked as a warning and lists both, oldest first, each by type, trainee and the day it
  last moved:
  - Portfolio and Logbook Review (Paediatrics) — Pieter du Plessis
  - Mini-CEX (Paediatrics) — Nomsa Mahlangu
  Each row's type links to the activity's page, `/activities/{id}` (T297). "Invitations nearing expiry" reads "No
  invitations expiring soon." The Quick action card offers "Start an MSF campaign".
Note: To play this in one sitting, age both requests by eight days (the statement touches 2 rows):
  `UPDATE "Activities" a SET "UpdatedOn" = a."UpdatedOn" - interval '8 days' FROM "ActivityTypes" t, "AspNetUsers" u
  WHERE t."Id" = a."ActivityTypeId" AND u."Id" = a."SubjectUserId" AND ((t."Key" = 'mini_cex_cpsa' AND u."Email" =
  'mahlangu@kgk.wombat.local' AND a."CurrentState" = 'requested') OR (t."Key" = 'portfolio_review_cpsa' AND u."Email" =
  'duplessis@kgk.wombat.local'));`
  The card lists what awaits a reviewer, read from each activity's pinned workflow (`ActivityWaiting`, T297), the
  predicate the nudge of Step 3.32 reads too: the Mini-CEX waits in `requested`, the portfolio review in `submitted`.
  The two wait different times: the nudge mails after five days, and the card lists a request only once it is untouched
  for `DashboardThresholds:CoordinatorStallDays` (seven), so a request mailed about on its sixth day reaches the card
  only after its seventh. The eight days' ageing puts both past both. If it lists the portfolio review alone, that is a
  Gap.
Actual (2026-09-26, T295 replay, wombat_scenario): The ageing SQL touched 2 rows (activities 10 and 21). "Stalled
  requests" is a detail-card--warning with one row only, "Portfolio and Logbook Review (Paediatrics) — Pieter du Plessis
  · 18 Sept", no link. Mahlangu's Mini-CEX (21, requested) is missing. Invitations: "No invitations expiring soon.";
  Quick action "Start an MSF campaign".
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): the ageing SQL touched 2 rows (10 and 21). "Stalled
  requests" (detail-card--warning) listed both, oldest first: "Portfolio and Logbook Review (Paediatrics) — Pieter du
  Plessis · 18 Sept", then "Mini-CEX (Paediatrics) — Nomsa Mahlangu · 18 Sept". Each type links to /activities/10 and
  /activities/21 (named "… for <trainee>"), and each opens read-only for him. Invitations "No invitations expiring
  soon."; Quick action "Start an MSF campaign". Step 3.32's run then mailed Dr Zulu about this same Mini-CEX.
Gap: [F-3.30a, T297] fixed by T297 (7bf8ea7): the card lists the Mini-CEX the nudge mails about, and each row links.
  [F-3.30b, T328] The row's date is not kept on one line: beside "Portfolio and Logbook Review (Paediatrics) — Pieter du
  Plessis" at 1280 px it breaks into "18" over "Sept".

### Step 3.31 — Mr Smit looks for a page of stalled requests
Role: Coordinator — Mr Pieter Smit
Route: / → /not-found
Do: Look in the menu for a page of stalled requests. Then type the address the menu once linked,
  `/placeholder/stalled-activities`.
Expect: The menu offers none: Home, Decisions due, MSF campaigns, Committee reviews and Data rights requests, then My
  data rights. The nav links to no unbuilt page (DESIGN.md § The NavMenu); stalled work is flow 06's. Only Home's
  "Stalled requests" card lists it, and no page chases a stalled request: no reminder and no reassignment. The old
  address is "Page not found" with status 404: the placeholder page went with the stubs (T335, flow 01).
Actual (2026-09-26, T295 replay, wombat_scenario): Stalled Activities reads "This page is not built yet." with a
  "Coming soon" card; nothing on it chases a request. `/placeholder/stalled-work` is "Page not found", HTTP 404.
Gap: none

### Step 3.32 — The daily nudge reminds the assessors
Role: Administrator — devadmin@wombat.local
Route: /admin/jobs
Do: Run `assessor-pending-nudge` now.
Expect: The job's last run updates. The application log holds two stub emails, "Activities awaiting your assessment".
  The log names no address (T282); each greets its assessor by first name:
  - "Hi Thandi", listing Mini-CEX (Paediatrics) from Nomsa Mahlangu, waiting 8 days;
  - "Hi Mohammed", listing Portfolio and Logbook Review (Paediatrics) from Pieter du Plessis, waiting 8 days.
  The run's summary line reads "assessors nudged 2 (activities 2)" and skips nobody. Nothing else has waited five days,
  so nobody else is nudged.
Note: The job reads each activity's pinned workflow and nudges on anything untouched for five days in a state whose
  next move belongs to a `field:` nominee (`AssessorPendingNudgeJob`). D50 decides whom it skips.
Actual (2026-09-26, T295 replay, wombat_scenario): Last run moved 11:03 → 12:43, Succeeded. Log: "assessors nudged 2
  (activities 2); nominees skipped: … 0" and "sent 2, not delivered 0", no address. This replay's app has a local SMTP
  sink, so the two "Activities awaiting your assessment" mails landed there, not in the log: "Hi Thandi" (Mini-CEX from
  Nomsa Mahlangu, waiting 8 days) and "Hi Mohammed" (Portfolio and Logbook Review from Pieter du Plessis, 8 days).
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3), which moved the job onto the cards' shared predicate: with
  no mail server, the log held the two "Activities awaiting your assessment" stub emails, "Hi Mohammed" (Portfolio and
  Logbook Review from Pieter du Plessis, waiting 8 days) and "Hi Thandi" (Mini-CEX from Nomsa Mahlangu, 8 days), no
  address, and "assessors nudged 2 (activities 2)", nobody skipped: the two activities Step 3.30's card lists.
Gap: none

### Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her sessions open acting as a Committee member; choose Switch to Assessor in the sidebar. From "Waiting for your
  rating", open Dr Mahlangu's Mini-CEX, rate it 3a with feedback, and complete it.
Expect: The sidebar first reads "Acting as Committee member", with "Switch to Assessor" under it, and Home "Committee
  member · Semester N, YYYY" under its heading. After the switch Home reads "You are now acting as Assessor." in an info
  alert under the header, and "Assessor · Semester N, YYYY"; the sidebar reads "Acting as Assessor" over Home and
  Activity inbox, and "Waiting for your rating" is badged 1: this Mini-CEX.
  The card lists it, "Mini-CEX (Paediatrics) — Nomsa Mahlangu", badged Overdue: it has waited past the
  assessor's seven days since Step 3.30 aged it. After Complete, the Mini-CEX is Completed, credited "1 item". Year 1's
  minimum on PAED-004 is 3a. Nothing of Dr Mahlangu's is stalled any more.
Note: The card counts what her inbox lists, less her own portfolio (`ActivityWaiting`, T297), so it cannot read 0
  beside an inbox holding one. If it does, that is a Gap.
Actual (2026-09-26, T295 replay, wombat_scenario): Home read "Viewing as CommitteeMember", "You also act as Assessor.
  Switch view: Assessor". After the switch, "Viewing as Assessor", but Pending requests read "0 assessments awaiting
  review" while Review inbox listed activity 21 (Requested). Rated 3a with feedback, Complete: State Completed, the
  history's Complete row credited "1 item". Mahlangu's PAED-004 2026 S2 reads 1 counted, 1 at minimum.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): Home first read "Viewing as CommitteeMember" and "You also
  act as Assessor. Switch view: Assessor"; after the switch, "Viewing as Assessor", Pending requests "1 assessment
  awaiting review", and Awaiting your review (now a warning card) listed "Mini-CEX (Paediatrics) — Nomsa Mahlangu"
  badged Overdue. "Review inbox →" held that one row, Requested, updated 2026-09-18. Completed at 3a, "1 item"; Home
  then read 0 and "Nothing is awaiting your review.", and Mr Smit's Stalled requests listed only the portfolio review.
Gap: [F-3.33a, T297] fixed by T297 (7bf8ea7).

## Phase 3.G — Multi-source feedback for Dr Molefe

### Step 3.34 — Mr Smit creates the MSF questionnaire
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/new
Do: Open MSF campaigns and start a new campaign. The Template select is empty, so add the Quick template as it stands:
  kind Multi-source feedback, name "Default MSF", with its scale question and its comment question.
Expect: The list reads "No MSF campaigns", with MSF coverage and New campaign in its header. The create page shows a
  Quick template card and a Create campaign card. Adding the template reads "Template created.", and the Template
  select now offers Default MSF.
Actual (2026-09-26, T295 replay, wombat_scenario): The list read "No MSF campaigns" with MSF coverage and New campaign
  in its header. The create page showed Quick template (kind Multi-source feedback, "Default MSF", its scale and comment
  questions) and Create campaign, the Template select holding only "Select template". Add template: "Template created.",
  and the select now offers Default MSF.
Gap: none

### Step 3.35 — Mr Smit creates a campaign for Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int}
Do: Fill in the campaign:
  - trainee Dr Lerato Molefe and template Default MSF;
  - opens on `D` and closes on `D+14`, the defaults;
  - minimum responses 5, minimum category responses 2 and minimum reporting categories 2;
  - evidence for PAED-010 and PAED-012.
  Create it.
Expect: The Trainee select offers KGK's five current trainees by name and address, and nobody else. Once a trainee is
  chosen, "Evidence for these EPAs" lists all 15 EPAs of her curriculum, since MSF is on every one (D37). The help
  beside the reporting categories says that two is the College's answer (D11). Creating it opens the campaign: Draft,
  Lerato Molefe, Default MSF (Multi-source feedback), window `D` to `D+14`, and "Nobody has been invited yet." Open
  campaign is shown disabled, with "Open campaign: add at least one invitee first…" below it (T225).
Actual (2026-09-26, T295 replay, wombat_scenario): Trainee select: the five KGK trainees by name and address; evidence:
  PAED-001 to 015; help "Two is the College's answer…"; minimums typed 5/2/2 (defaults 8/3/2). Campaign 1: Draft, Lerato
  Molefe, Default MSF (Multi-source feedback), 2026-09-26 to 2026-10-10, "Nobody has been invited yet.", Open campaign
  disabled with "Open campaign: add at least one invitee first. …".
Gap: none

### Step 3.36 — Mr Smit invites seven colleagues, and removes one added under the wrong group
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Add each colleague by address and respondent group:
  - Sister Grace Mokwena, `grace.mokwena@kgk.wombat.local`, as a Nurse;
  - Sister Palesa Tau, `palesa.tau@kgk.wombat.local`, by mistake as an Allied health professional;
  - the registrars Dr Kagiso Motsepe, `kagiso.motsepe@kgk.wombat.local`, and Dr Lindiwe Khoza,
    `lindiwe.khoza@kgk.wombat.local`, as Peer doctors;
  - Dr Khumalo, `khumalo@kgk.wombat.local`, and Dr Botha, `botha@kgk.wombat.local`, as Consultants;
  - the physiotherapist Ms Naledi Sebego, `naledi.sebego@kgk.wombat.local`, as an Allied health professional.
  Then remove Sister Tau's row and add her again as a Nurse.
Expect: Each add reads "Invitee added." and clears the address for the next one. The group select offers Peer doctor,
  Consultant, Nurse, Allied health professional and Other; it offers no Patient and no Learner. Remove asks "Remove this
  invitee?", naming the address and group. Confirmed, it says she "has been removed from this campaign, and will not be
  emailed a link when it opens." In the end the counts read Peer doctor 2, Consultant 2, Nurse 2, Allied health
  professional 1 and All groups 7, with no Responded column. "Addresses invited" lists the seven, each with Remove, and
  Open campaign is enabled.
Actual (2026-09-26, T295 replay, wombat_scenario): All eight adds read "Invitee added." and cleared the address; groups
  offered: the five expected. Remove asked "Remove this invitee?" naming "palesa.tau@… (Allied health professional)",
  then "…has been removed from this campaign, and will not be emailed a link when it opens." End: 2/2/2/1, All groups 7,
  no Responded column, seven addresses each with Remove, Open campaign enabled.
Gap: none

### Step 3.37 — Mr Smit opens the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Open the campaign.
Expect: The page reads "Campaign opened; links are being sent." and State: Open. The note names `D+14` as the
  respondents' last day, and says the campaign can be closed sooner from its report. "7 links are still being sent.
  Reload this page to see whether they were delivered." The invitees are now counted only, with a Responded column
  reading 0, and no address is listed (T217, T247). The actions are Withdraw campaign and View report. The application
  log holds seven "Feedback request: Lerato Molefe (Default MSF, …)" emails, each with a `/msf/respond` link.
Actual (2026-09-26, T295 replay, wombat_scenario): "Campaign opened; links are being sent.", State Open, last day
  2026-10-10, close sooner from the report; "7 links are still being sent. Reload…"; counts only, Responded 0, no
  address; Withdraw campaign and View report. Seven "Feedback request: Lerato Molefe (Default MSF, …)" mails reached
  this replay's SMTP sink, each with a `/msf/respond?token=` link on localhost:5080 (the app's base URL; opened on
  5180).
Gap: none

### Step 3.38 — Mr Smit withdraws a campaign started in error
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int} → /msf/campaigns
Do: Create a second campaign, for Dr Pieter du Plessis: Default MSF, the defaults, evidence for PAED-015. Then
  Dr Mokoena says his MSF belongs to next semester, so withdraw it from the campaign list.
Expect: The list shows two rows, each with links named by its state (T225):
  - Lerato Molefe, Open: "Manage", "View report" and Withdraw;
  - Pieter du Plessis, Draft: "Manage" and Withdraw.
  Withdraw asks "Withdraw this campaign?", names "Pieter du Plessis (Default MSF, closing `D+14`)", and says withdrawing
  cannot be undone. Confirmed, it reads "The campaign for Pieter du Plessis (…) has been withdrawn. Its respondents'
  links no longer work, and their email addresses have been removed." The row then reads Withdrawn, with only "View
  campaign".
Actual (2026-09-26, T295 replay, wombat_scenario): Campaign 2 created for du Plessis (defaults 8/3/2, PAED-015). The
  list read Pieter du Plessis, Draft: Manage, Withdraw; Lerato Molefe, Open, 7 invitations: Manage, View report,
  Withdraw. The dialog: "Withdraw this campaign?", "…Pieter du Plessis (Default MSF, closing 2026-10-10)?…withdrawing
  cannot be undone." Confirmed: the quoted "has been withdrawn…" message; the row reads Withdrawn with only View
  campaign.
Gap: none

### Step 3.39 — Three respondents answer from their links
Role: Anonymous — Sister Grace Mokwena, Sister Palesa Tau and Dr Kagiso Motsepe (MSF respondents)
Route: /msf/respond
Do: Each respondent opens their own link from the log, in a browser signed in to nothing, and answers:
  - Sister Mokwena: Above expectations, "Calm and kind with parents at night; hands over clearly".
  - Sister Tau: Well above expectations, "Leads neonatal resuscitations and explains her decisions to the nurses".
  - Dr Motsepe: Meets expectations, "A supportive senior; could delegate more".
Expect: The page reads "Feedback on Lerato Molefe", Default MSF, and last day to respond `D+14`. It says their name and
  address are never shown to her, and that she sees the feedback only after it has closed and been released, grouped by
  respondent role. The rating offers five points, from Well below expectations to Well above expectations (D47), and is
  required. The comment is optional, with help warning that it may reach her word for word. Each submit ends on "Thank
  you" and "Your feedback on Lerato Molefe has been recorded."
Note: The stub emails name no address (T282), and a link's group decides where its answer is counted. Match each link
  to its invitee before handing it out: its `token=` begins with the `TokenSelector` of its row in
  `SELECT "RespondentEmail", "RespondentCategory", "TokenSelector" FROM "MsfInvitations"
  WHERE "CampaignId" = <the campaign's id>;`.
Actual (2026-09-26, T295 replay, wombat_scenario): Signed out, links matched by TokenSelector. "Feedback on Lerato
  Molefe", "Default MSF · Last day to respond: 2026-10-10", the anonymity text, five required points (an empty submit
  got the browser's "Please select one of these options."), the comment's word-for-word warning. All three: "Thank you",
  "Your feedback on Lerato Molefe has been recorded."; MsfResponses 3.
Gap: none

### Step 3.40 — A used link is refused
Role: Anonymous — Dr Kagiso Motsepe (MSF respondent)
Route: /msf/respond
Do: Open the same link again.
Expect: The page reads "Feedback link already used" and shows no questionnaire.
Actual (2026-09-26, T295 replay, wombat_scenario): HTTP 410, "Feedback link already used", "…a response was submitted
  through it, and each link takes one response."; no questionnaire (no rating control).
Gap: none

### Step 3.41 — Mr Smit reads the report while the campaign is open
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/reports/{CampaignId:int}
Do: Open Dr Molefe's report from the campaign list.
Expect: The report reads State Open, total responses 3, minimum responses 5, minimum category responses 2, and
  reporting categories 1 of 2 required. "Evidence for" lists PAED-010 and PAED-012, and says one record per EPA is
  written when the report is released. A Nurse card gives the rating's average, 4.50 from 2 responses, and both
  comments. A Peer doctor card reads "Insufficient responses in this category. Results are suppressed." A group that
  has not answered has no card. Close campaign is offered; Release is not.
Actual (2026-09-26, T295 replay, wombat_scenario): /msf/reports/1: State Open, Total responses 3, minimums 5 and 2,
  "Reporting categories: 1 of 2 required". Evidence for PAED-010 and PAED-012, "One evidence record per EPA is written…
  when the report is released." Nurse: "Average: 4.50 from 2 responses." with both comments; Peer doctor: "Insufficient
  responses in this category. Results are suppressed."; no other card. Only button: Close campaign.
Gap: none

### Step 3.42 — Three more respondents answer, and one never does
Role: Anonymous — Dr Fatima Khumalo, Dr Sarah Botha and Ms Naledi Sebego (MSF respondents)
Route: /msf/respond
Do: The three answer; Dr Khoza does not open her link.
  - Dr Khumalo: Above expectations, "Sound judgement in clinic; teaches the interns well".
  - Dr Botha: Above expectations, "Ready to run a ward; should document escalation decisions more fully".
  - Ms Sebego: Above expectations, "Refers early and listens to the therapists".
Expect: Each submit ends on "Thank you" and "Your feedback on Lerato Molefe has been recorded." Six of the seven links
  have now been used; Step 3.43 reads the counts.
Actual (2026-09-26, T295 replay, wombat_scenario): Signed out; Khumalo, Botha and Sebego each answered Above
  expectations with their comments, and each submit read "Thank you", "Your feedback on Lerato Molefe has been
  recorded." Khoza's link was not opened. Six of seven links used.
Gap: none

### Step 3.43 — Mr Smit resends the undelivered link
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: At least an hour after the open, open the campaign and resend.
Expect: The counts read Responded: Peer doctor 1, Consultant 2, Nurse 2 and Allied health professional 1, and All
  groups 7 invited, 6 responded. A warning reads "1 link was not delivered. Resend sends each of these respondents a new
  link; this page never says who they are.", with a "Resend 1 link" button. After the resend, the page reads "1 new link
  is being sent." and "1 link is still being sent…", and the warning is gone. The log holds one more "Feedback request:
  Lerato Molefe …" stub email. It names no address; it is Dr Khoza's, the only link not answered.
Note: With `Email__SmtpHost` unset, nothing reports a delivery, so an hour after the open every unanswered link reads as
  not delivered (INFRASTRUCTURE.md § After T251). Where the mail reaches a server that accepts it (a local SMTP sink),
  every link is reported sent and none ever reads as not delivered, so stand in for a dropped mail on Dr Khoza's link;
  the warning then shows at once: `UPDATE "MsfInvitations" SET "SentOn" = NULL, "DeliveryFailedOn" = now() WHERE
  "CampaignId" = <the campaign's id> AND "RespondentEmail" = 'lindiwe.khoza@kgk.wombat.local' AND "RespondedOn" IS
  NULL;`
Actual (2026-09-26, T295 replay, wombat_scenario): Responded 1/2/2/1, All groups 7 / 6, and no warning: the sink had
  reported all seven sent. After the Note's UPDATE (1 row), the quoted "1 link was not delivered…" and "Resend 1 link".
  Resend: "1 new link is being sent." and "1 link is still being sent. Reload…", warning gone; one more "Feedback
  request: Lerato Molefe …" mail, to Dr Khoza.
Gap: runbook corrected: with a mail server that accepts the mail (this replay's SMTP sink) the step could not be played,
  since no link ever reads as not delivered; the Note now gives the one-row stand-in used here (a dropped mail).

### Step 3.44 — Mr Smit closes the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns/{CampaignId:int}
Do: On the report, close the campaign. Then open its campaign page.
Expect: The report reads "Campaign closed and anonymised for review." and State: Under review:
  - total responses 6; reporting categories 2 of 2 required;
  - the Nurse card (4.50 from 2) and the Consultant card (4.00 from 2) show their comments;
  - Peer doctor and Allied health professional are suppressed.
  Release to trainee is enabled. Beside it are the narrative and the optional "Supervision level this feedback
  supports", which offers Not stated and the six rungs (D10). The campaign page says the campaign is closed to
  responses and every respondent's address has been removed. It offers Withdraw campaign and "Review and release".
Actual (2026-09-26, T295 replay, wombat_scenario): "Campaign closed and anonymised for review.", Under review, total 6,
  2 of 2 categories; Consultant 4.00 and Nurse 4.50 (2 each) with comments, Peer doctor and Allied suppressed; Release
  to trainee enabled beside the narrative and Not stated/1/2/3a/3b/4/5. Campaign page: "Closed to responses, and every
  respondent's email address has been removed…", Withdraw campaign and Review and release.
Gap: none

### Step 3.45 — A link opened after the close is refused
Role: Anonymous — Dr Lindiwe Khoza (MSF respondent)
Route: /msf/respond
Do: Open the resent link.
Expect: The page reads "Feedback request closed" and shows no questionnaire.
Actual (2026-09-26, T295 replay, wombat_scenario): Signed out, the resent link: HTTP 410, "Feedback request closed",
  "This feedback request has closed and is no longer accepting responses."; no questionnaire.
Gap: none

### Step 3.46 — Mr Smit releases the report to Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns
Do: Write the narrative "Consistently strong feedback from nursing and consultant colleagues; keep documenting
  escalation decisions". Leave the supervision level Not stated, since he is not a clinician. Release.
Expect: The report reads "Report released to the trainee." and State Released. The actions card shows the narrative
  and "Not stated" as stored, with no form (T246). "Evidence for" lists PAED-010 and PAED-012, with the time they were
  recorded on the portfolio. In the list, the row reads Released, with "View campaign" and "View report" and no
  Withdraw.
Actual (2026-09-26, T295 replay, wombat_scenario): Release (no confirmation dialog): "Report released to the trainee.",
  State Released; the actions card shows the narrative and "Supervision level this feedback supports: Not stated" as
  text, with no form. Evidence for PAED-010 and PAED-012, "Recorded on the portfolio: 2026-09-26 10:53 UTC". List row:
  Released, 7 invitations, 6 responses, View campaign and View report, no Withdraw.
Gap: none

### Step 3.47 — Dr Molefe reads her released report
Role: Trainee — Dr Lerato Molefe
Route: /msf/my-reports → /msf/my-reports/{CampaignId:int}
Do: Open MSF reports, then view the report.
Expect: One row: Default MSF (Multi-source feedback), released just now, 6 responses. The report shows the narrative
  and two groups only, Nurse (4.50) and Consultant (4.00), each with its comments. It shows nothing of the peer doctor
  or the physiotherapist, not even that they answered, and no group's count (T249).
Actual (2026-09-26, T295 replay, wombat_scenario): One row: Default MSF / Multi-source feedback, Released "2026-09-26
  12:53" (the report said 10:53 UTC), Responses 6. The report: the narrative, Consultant ("…professional performance.:
  4.00") and Nurse (".: 4.50"), each with both comments; nothing of Peer doctor or Allied health, no group count.
Gap: [F-3.47a, T270] The released report prints each scale question as "<question>.: <average>", a stray colon after the
  question's own full stop. [F-3.47b, T325] Unsure: the one release is 12:53 unlabelled on the trainee's list and 10:53 UTC on
  the coordinator's report (and /admin/jobs shows local times beside UTC schedules); no rule in DESIGN.md says which.

### Step 3.48 — Dr Molefe's record and progress show her evidence and the feedback
Role: Trainee — Dr Lerato Molefe
Route: /activities/mine → /portfolio/progress
Do: Open My activities, then My progress.
Expect: My activities holds her six Completed WBAs and two Multi-Source Feedback (Paediatrics) rows. The MSF rows are
  PAED-010 and PAED-012, encounter date `D` (the day the campaign closed), Recorded, credited "—" (D8). My progress
  reads:
  - semester targets "1 of 10 EPAs met this semester", and training year 4;
  - multi-source feedback "2 of 15 EPAs covered by a released campaign that closed this semester. MSF is tracked on its
    own and counts towards no target.";
  - PAED-001: "3 of 3 this semester", "Target met for Semester 2, 2026. At the minimum level when observed: 2 of 3.",
    minimum now 5;
  - PAED-012 "2 of 3" and PAED-010 "1 of 3", each with "MSF in Semester 2, 2026: covered by a released campaign that
    closed on …".
  The trajectory charts PAED-001 (3 observations from 3 distinct assessors), PAED-010 and PAED-012, and plots no MSF
  point (D36).
Actual (2026-09-26, T295 replay, wombat_scenario): My Activities: six Completed WBAs and two MSF rows (PAED-010, 012,
  2026-09-26, Recorded, "—"). My Progress: every quoted line as expected, MSF "…closed on 26 September 2026"; trajectory
  PAED-001 (3 from 3 assessors), 010, 012, no MSF point. Yearly EPAs add "Your training year changed on 14 Jan 2026".
Gap: none. [F-3.48a, not a defect: D17] My Progress says her training year changed on 14 Jan 2026, a day before her
  anniversary: D17 counts the training year in whole 365-day blocks, and 2024 was a leap year (Act 2 § Dates says so).

### Step 3.49 — Mr Smit reads the programme's MSF coverage
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/coverage
Do: Open MSF coverage from the campaign list.
Expect: One programme: "Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital".
  - Semester 1, 2026 reads "5 trainees, whose programme had started by 30 June 2026". Semester 2, 2026 reads
    "5 trainees, whose programme had started by 31 December 2026".
  - By EPA, in Semester 2, 2026: PAED-010 and PAED-012 read "1 of 5 trainees covered", in bold. Every other cell reads
    "0 of 5 trainees covered".
  - By trainee: Lerato Molefe reads "2 of 15 EPAs covered" in Semester 2, 2026; everyone else reads 0 of 15.
  There is no badge, tint or bar, because coverage is not a target (D8, D9). The withdrawn campaign counts for nothing.
Actual (2026-09-26, T295 replay, wombat_scenario): One programme, "Paediatric EPA Curriculum 11.1 at Kgosi Kgari
  Teaching Hospital"; S1 "5 trainees, whose programme had started by 30 June 2026", S2 "…by 31 December 2026". S2
  PAED-010 and PAED-012 "1 of 5 trainees covered" in <strong>, every other cell 0 of 5; Lerato Molefe S2 "2 of 15 EPAs
  covered", the rest 0 of 15. No badge, tint or bar; nothing from the withdrawn campaign. No horizontal scroll at 390
  px.
Gap: none

## Phase 3.H — Dashboards with data

### Step 3.50 — Dr Dlamini's dashboard
Role: Trainee — Dr Anele Dlamini
Route: /
Do: Open the dashboard and read each card.
Expect:
  - Curriculum targets, "Semester 2, 2026 · July to November": "1 / 10" semester targets met and "0 / 5" yearly targets
    met (2026). Below them are the five EPAs furthest short, largest shortfall first: PAED-002, PAED-003, PAED-005,
    PAED-010 and PAED-012, each "0 of 3 this semester".
  - Activity inbox: "No pending items."
  - Recent activities: her four WBAs, each Completed, in green.
  - Upcoming deadlines: "No deadlines in the next 14 days."
  - My authorisations links to its page, and the header offers "Log an activity" (T335: the Actions card is gone).
Actual (2026-09-26, T295 replay, wombat_scenario): Every card as expected: 1 / 10 and 0 / 5, PAED-002, 003, 005, 010,
  012 "0 of 3 this semester"; "No pending items."; activities 20, 19, 18, 1 green; "No deadlines…"; links to
  /portfolio/authorisations and /activities/new. State capture: /msf/my-reports/1 reads "The selected report is not
  available to the current trainee." twice (a danger alert, and in place of her list).
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): every card as before, 1 / 10 and 0 / 5 with PAED-002, 003,
  005, 010 and 012 "0 of 3 this semester"; Activity inbox "No pending items." beside an inbox reading "Inbox clear";
  Recent activities 20, 19, 18 and 1, Completed, in green; "No deadlines in the next 14 days.".
Gap: [F-3.50a, T270] My MSF Reports with another trainee's campaign id shows the refusal twice, as a danger alert and as the
  list's load error, and hides her (empty) list, where states.md expects "her empty list, nothing selected".

### Step 3.51 — Dr Khumalo's dashboard and menu
Role: Assessor — Dr Fatima Khumalo
Route: / → /activities/inbox
Do: Read the dashboard, then follow "Open inbox →". Then read the menu.
Expect:
  - Waiting for your rating, badged 1: "Case-Based Discussion (Paediatrics) — Pieter du Plessis", Requested (T297,
    T335). The inbox lists it as Requested.
  - Recent decisions, the activities she moved last, newest first: Mini-CEX (Paediatrics) — Anele Dlamini, Completed,
    in green; Case-Based Discussion (Paediatrics) — Lerato Molefe, Completed; Mini-CEX (Paediatrics) — Sipho Ndlovu,
    Declined, in red.
  - The inbox lights Activity inbox in the menu: Home and Activity inbox, then My data rights. There is no Recent
    activities: the flow 01 pick dropped it, with its placeholder.
Note: "Waiting for your rating" is read as in Step 3.33, from her inbox: 0 beside an inbox holding the CBD is a Gap.
  Until T297 it was "Accepted, needing action"; until T335 it was split across Pending requests and Awaiting your
  review.
Actual (2026-09-26, T295 replay, wombat_scenario): Pending requests "0 assessments awaiting review", while Review inbox
  lists du Plessis's CBD (9), Requested. "No accepted assessments pending action." Recent decisions: Dlamini Mini-CEX
  and Molefe CBD Completed (green), Ndlovu Mini-CEX Declined (red). Recent Activities is "Coming soon".
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): Pending requests "1 assessment awaiting review"; Awaiting
  your review "Case-Based Discussion (Paediatrics) — Pieter du Plessis", Requested (filed on `D`, so not overdue);
  "Review inbox →" listed that CBD alone, Requested. Recent decisions, newest first: Mini-CEX (Paediatrics) — Anele
  Dlamini and Case-Based Discussion (Paediatrics) — Lerato Molefe, Completed in green, then Mini-CEX (Paediatrics) —
  Sipho Ndlovu, Declined in red. Recent Activities is "Coming soon".
Gap: [F-3.51a, T297] fixed by T297 (7bf8ea7).

### Step 3.52 — Dr Zulu's committee dashboard
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → /
Do: Choose Switch to Committee member in the sidebar and read the dashboard and the menu.
Expect:
  - Home opens acting as Assessor, her choice from Step 3.33, kept with her account. After the switch an info alert
    reads "You are now acting as Committee member.", the sidebar "Acting as Committee member", and Home's subtitle
    "Committee member · Semester N, YYYY".
  - Targets this period, "Semester 2, 2026 · July to November": KGK's five current trainees, fewest met first.
    Pieter du Plessis, Nomsa Mahlangu and Sipho Ndlovu read "semester 0/10 · yearly 0/5". Anele Dlamini and Lerato
    Molefe read "semester 1/10 · yearly 0/5".
  - Targets met by EPA: PAED-001 (3 per semester) reads "2 of 5 met", and every other EPA "0 of 5 met".
  - Neither card holds a link.
  - Her menu is Home, Committee reviews and Decision panels, then My data rights. Programme trainees is flow 06's, and
    the menu offers no page before it is built.
Note: The card reads the trainees in the member's sub-speciality scopes (`GetCommitteeMemberDashboardSummaryQuery`).
  Dr Zulu's invitation carried none; her Paediatrics sub-speciality came with her assessor profile (Step 2.14).
Actual (2026-09-26, T295 replay, wombat_scenario): Landed on Assessor (3.33's switch persists); after the switch:
  Ndlovu, Mahlangu, du Plessis "semester 0/10 · yearly 0/5", then Dlamini and Molefe "semester 1/10 · yearly 0/5";
  PAED-001 "2 of 5 met", 14 others "0 of 5 met"; no link in either card. Programme Trainees is "Coming soon".
Gap: none

### Step 3.53 — Dr Mokoena's dashboard
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the dashboard and the menu.
Expect:
  - Pending reviews: "2 activities awaiting review", Dr du Plessis's Requested CBD and his portfolio review awaiting
    review. The card has no link (T297).
  - Trainees in programme: "5 active / 0 inactive".
  - Curriculum coverage — Semester 2, 2026: the 15 EPAs with their targets. PAED-001 reads "2 of 5 met", and the rest
    "0 of 5 met", the same figures as Dr Zulu's.
  - Her menu is Home, Decisions due, Committee reviews and Decision panels, then My data rights. The STAR review queue
    is flow 09's and Programme trainees flow 06's; the menu offers neither before it is built.
Note: The count is the programme's backlog awaiting a reviewer at KGK, read from each activity's pinned workflow
  (`ActivityWaiting`, T297): a request waits in `requested`, a portfolio review in `submitted`. It used to link to
  `/activities/inbox`, which lists only what she can move, and read "Inbox clear" beside the count; no page lists the
  backlog yet, so the card links nowhere.
Actual (2026-09-26, T295 replay, wombat_scenario): Pending reviews "1 activities in review" (the portfolio review; the
  Requested CBD not counted); "5 active / 0 inactive"; PAED-001 "2 of 5 met", 14 others "0 of 5 met". Review queue →
  /activities/inbox: "Inbox clear". STAR Review Queue is "Coming soon".
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): Pending reviews "2 activities awaiting review" (the
  Requested CBD and the portfolio review) in a card with no link; nothing on the page links to /activities/inbox, and
  her nav does not offer it. "5 active / 0 inactive"; PAED-001 "2 of 5 met", 14 others "0 of 5 met". On a post-actA
  copy (wombat_scenario_rc3a), after two of its three were completed or cancelled, the tile read "1 activity awaiting
  review".
Gap: [F-3.53a, T297] fixed by T297 (7bf8ea7): the Requested CBD is counted, the label has a singular, and the card no
  longer opens an inbox reading "Inbox clear" (its capture 3.53-2 has no step now).

### Step 3.54 — Dr Sithole's dashboard
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /
Do: Read the dashboard.
Expect: The same three cards and figures as Dr Mokoena's: "2 activities awaiting review" with no link, "5 active / 0
  inactive", and PAED-001 "2 of 5 met". His menu reads as hers, under "Acting as Sub-speciality admin".
Actual (2026-09-26, T295 replay, wombat_scenario): "Viewing as SubSpecialityAdmin": "1 activities in review" with
  Review queue → /activities/inbox, "5 active / 0 inactive", PAED-001 "2 of 5 met" and 14 "0 of 5 met". Nav: Programme
  Trainees and STAR Review Queue placeholders, Decision Panels, Committee Reviews, Decisions Due.
  Re-checked after T297 (2026-09-26, wombat_scenario_rc3): "Viewing as SubSpecialityAdmin": "2 activities awaiting
  review" with no link, "5 active / 0 inactive", PAED-001 "2 of 5 met" and 14 "0 of 5 met"; the nav as before.
Gap: [F-3.54a, T297] fixed by T297 (7bf8ea7).

## Phase 3.I — The audit trail

### Step 3.55 — Prof Mbatha reads KGK's audit log
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit
Do: Open Audit log, which shows the last 24 hours by default. Set From (UTC) to the start of this act's sitting, so
  that no earlier act's rows are counted, and apply. Filter by Result: Failures only. Then clear that and filter by
  Action "Msf".
Expect:
  - Every row is KGK's, newest first, 50 to a page. The rows include CreateActivityCommand and
    TransitionActivityCommand by each registrar and consultant, and Mr Smit's MSF commands, each with its actor and OK.
  - Failures only leaves four rows, each FAILED: Dr Ndlovu's two refused creates (the future date and the date before
    his programme started), Dr Dlamini's refused submit, and Dr Khumalo's decline without a note.
  - "Msf" leaves Mr Smit's seventeen commands: the template, two campaigns, eight invitations, one removal, the open,
    the withdrawal, the resend, the close and the release.
  - No respondent's submission and no devadmin row is listed, because those carry no institution (T101, T205).
Actual (2026-09-26, T295 replay, wombat_scenario): From (UTC) 2026-09-26T10:13: 161 KGK rows over four pages of 50,
  newest first (81 Login/Logout, 21+2 CreateActivityCommand, 38+2 TransitionActivityCommand, 17 MSF), none from a
  respondent or devadmin. Failures only: the four FAILED rows (Ndlovu ×2, Dlamini, Khumalo). "Msf": Smit's 17. The actor
  column is the account's email on command and Logout rows, and the person's name on Login rows.
Gap: [F-3.55a, T286] The audit log names the actor of every command (and Logout) by email, but of a Login by display name, so
  one person reads two ways in one list; Step 3.56's "display name" is the email too.

### Step 3.56 — Prof Mbatha opens one entry
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Under Failures only, open Dr Ndlovu's future-date create.
Expect: The Event card shows the time to the millisecond, category Command, action CreateActivityCommand, and result
  Failed with "Date observed: The date cannot be after today (…)." The Actor card shows Dr Ndlovu's user id, display
  name and IP address. There is no Payload card: only an Administrator reads the raw JSON. "Back to log" returns to the
  list.
Actual (2026-09-26, T295 replay, wombat_scenario): Event: "2026-09-26 10:17:57.627", Command, CreateActivityCommand,
  Failed, "Date observed: The date cannot be after today (2026-09-26)." Actor: user id, "Display name
  ndlovu@kgk.wombat.local", IP "::/48". Only Event and Actor cards, no Payload. Back to log returns to /admin/audit with
  the filters cleared (last 24 hours, All).
Gap: [F-3.56a, T286] The Actor card's "Display name" is Dr Ndlovu's email, not his name (see F-3.55a). [F-3.56b, T277] Back to log
  drops the From and Failures-only filters, so the reader lands on an unfiltered page.

### Step 3.57 — devadmin reads the log across institutions
Role: Administrator — devadmin@wombat.local
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Open Audit log, with From (UTC) set to the start of this act's sitting as in Step 3.55. Open one respondent's
  submission (SubmitMsfResponseCommand), then one of Dr Naidoo's TransitionActivityCommand rows.
Expect: The log holds every institution's rows and the rows with no institution. Among them are six
  SubmitMsfResponseCommand rows whose actor is "system", and devadmin's own RunScheduledJobNowCommand. The submission's
  detail names no user and no display name, only a truncated address. Its payload shows the token and the answers as
  "[REDACTED]" (T101, T205). Dr Naidoo's payload shows the principal as "[PRINCIPAL]", and the data patch and note as
  "[REDACTED]".
Actual (2026-09-26, T295 replay, wombat_scenario): From 10:13 UTC: 172 rows over four pages, Next enabled, among them
  six SubmitMsfResponseCommand by "system" and devadmin's RunScheduledJobNowCommand. A submission's detail: User ID "—",
  Display name "—", IP "::/48"; payload {"token": "[REDACTED]", "answers": "[REDACTED]"}. Naidoo's complete of 13:
  principal "[PRINCIPAL]", note and dataPatchJson "[REDACTED]", activityId, actorUserId and transitionKey in clear.
Gap: none

## Act 3 outcome state

Replay check (2026-09-26, T295, wombat_scenario): match on all five queries (23 activities in the 12 groups; the 8 progress rows; recorded 15, late 2; MSF Released 1 and Withdrawn 1, 6 responses, 7 invitations with 0 addresses, 1 template; failures CreateActivityCommand 2 and TransitionActivityCommand 2 since 10:13 UTC), and mail was 2 nudges, 7 invitations and 1 resent link, none about an activity's move; the one difference is in how it was reached: Khoza's link was marked dropped by SQL (Step 3.43), since this replay's SMTP sink reports every mail sent.

- **23 activities**:
  - 13 WBAs Completed;
  - 2 system-written MSF records;
  - 3 teaching sessions Logged;
  - 1 Declined, 1 Discussed and 1 Cancelled;
  - two left open: Dr du Plessis's CBD (Requested, Dr Khumalo) and his portfolio review (Awaiting review, Dr Patel,
    aged).
- **8 curriculum progress rows**, all in the semester containing `D`.
- **15 filings that record their lateness.** Two of them are late: Dr Ndlovu's two Mini-CEX filings, 20 days each.
  Neither of the reflective exercise's two submissions records one.
- **MSF:**
  - one questionnaire, Default MSF;
  - Dr Molefe's campaign Released, with 6 responses from 7 invitations whose addresses were removed at the close;
  - Dr du Plessis's draft Withdrawn.
- **The audit trail:** four failed commands in this act.
- **Mail:** the MSF invitations (seven, plus one resent) and two nudge digests. Nothing about any activity's moves.

```sql
-- 1. Activities by instrument and state: 23 in 12 groups
SELECT t."Key", a."CurrentState", count(*)
FROM "Activities" a JOIN "ActivityTypes" t ON t."Id" = a."ActivityTypeId"
GROUP BY 1, 2 ORDER BY 1, 2;
-- cbd_cpsa completed 3 · cbd_cpsa requested 1 · cca_cpsa completed 1 · direct_observation_cpsa completed 1
-- dops_cpsa cancelled 1 · dops_cpsa completed 2 · kgk_teaching_log logged 3 · mini_cex_cpsa completed 6
-- mini_cex_cpsa declined 1 · msf_cpsa recorded 2 · portfolio_review_cpsa submitted 1
-- reflective_exercise_cpsa discussed 1

-- 2. Credit: 8 rows, each in the academic year and semester containing D (2026, 2 in the window)
SELECT u."Email", e."Code", p."AcademicYear", p."Semester", p."CountsSoFar", p."MinimumLevelReachedCount"
FROM "CurriculumItemProgresses" p
JOIN "CurriculumItems" i ON i."Id" = p."CurriculumItemId"
JOIN "Epas" e ON e."Id" = i."EpaId"
JOIN "AspNetUsers" u ON u."Id" = p."TraineeUserId"
ORDER BY 1, 2;
-- dlamini PAED-001 3 2 · dlamini PAED-004 1 1 · mahlangu PAED-002 1 1 · mahlangu PAED-004 1 1
-- molefe PAED-001 3 2 · molefe PAED-010 1 1 · molefe PAED-012 2 2 · ndlovu PAED-002 1 1

-- 3. Lateness (D15, T160): recorded on every filing of a type that can credit, late on two
SELECT count(*) FILTER (WHERE "DaysAfterEncounter" IS NOT NULL) AS recorded,
       count(*) FILTER (WHERE "DaysAfterEncounter" > 14) AS late
FROM "ActivityTransitions";
-- recorded 15, late 2

-- 4. MSF
SELECT "State", count(*) FROM "MsfCampaigns" GROUP BY 1 ORDER BY 1;   -- 4 (Released) 1 · 5 (Withdrawn) 1
SELECT count(*) FROM "MsfResponses";                                   -- 6
SELECT count(*), count("RespondentEmail") FROM "MsfInvitations";       -- 7, 0
SELECT count(*) FROM "MsfTemplates";                                   -- 1

-- 5. The act's refused commands
SELECT "Action", count(*) FROM "AuditEntries"
WHERE NOT "Success" AND "OccurredAt" >= '<the sitting''s start, UTC>'
GROUP BY 1 ORDER BY 1;
-- CreateActivityCommand 2 · TransitionActivityCommand 2
```

Snapshot the database to `recovery/scenario-post-act3.dump`.

## Handoff to Act 4

Act 4's committee sits on this evidence. Every encounter above is dated inside the semester containing `D`, and so is
the MSF campaign's close. A review whose period is that semester therefore freezes all of it, and D38 requires each
staged STAR to name at least one item of that snapshot.

- **Dr Molefe (year 4)** is the case for STARs:
  - PAED-001: three completed WBAs from three assessors, two at year 4's minimum (rung 5). The target is met.
  - PAED-012: two completed WBAs, both at rung 5.
  - PAED-010: one Direct Observation at rung 5.
  - A released MSF campaign covering PAED-010 and PAED-012, with no level stated.
- **Dr Dlamini (year 3)** can support at least one STAR:
  - PAED-001: three completed WBAs, two at year 3's minimum (rung 4). The target is met.
  - PAED-004: one completed WBA, at rung 4.
- **Dr du Plessis (year 2)** has credited nothing. He has three logged teaching sessions, a CBD still Requested, and a
  portfolio review still Awaiting review. He is the natural case for deferral.
- **Dr Mahlangu (year 1)** has PAED-002 and PAED-004 once each, at the minimum. The PAED-004 Mini-CEX stalled for eight
  days before it was rated.
- **Dr Ndlovu (year 1)** has PAED-002 once, a declined request that credited nothing, and a discussed reflection on
  PAED-001.
- Dr Patel and Dr Khumalo have not acted on the two open requests. Act 4 may leave them open or close them; nothing
  here depends on either.

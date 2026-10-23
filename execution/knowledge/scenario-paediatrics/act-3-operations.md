# Act 3 — Months 1–6: operational rhythm

**Scenario date.** Monday 2 February to Tuesday 30 June 2026: the first semester of Dr Mahlangu's and Dr Ndlovu's first
year. The dates are narrative only. The steps are played on one replay day, `D`, and every date they type is written
relative to it (see "The clock in this act").

**Who acts.** The five registrars file workplace-based assessments. The five consultants rate, decline, return and
complete them. Mr Smit runs an MSF campaign for Dr Molefe and watches for stalled work. Nine colleagues are asked to
answer that campaign anonymously, and eight do. devadmin runs the assessor nudge. Every staff role reads its dashboard,
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
Route: / → /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: From the dashboard, log an activity and choose Mini-CEX (Paediatrics) in the picker. Fill in the EPA PAED-001, the
  assessor Dr David Naidoo, the date observed `D−10`, the setting Emergency unit and the complexity Moderate. Leave the
  presenting problem empty, and save the draft.
Expect: The picker offers eleven types, and no name appears twice: the ten hand-filed `*_cpsa` instruments and KGK
  Teaching Session Log, in Step 2.42's three groups. Multi-Source Feedback and Learner Feedback are not offered (T162).
  Mini-CEX (Paediatrics), under Rated by an assessor, opens its form, which has three sections: Request, Entrustment and
  Feedback. Only Request can be filled in. Entrustment and Feedback are locked, and once Dr Naidoo is named each reads
  "David Naidoo fills this in"; the primary button then reads Submit to David Naidoo. The EPA picker offers the nine
  EPAs whose list names the Mini-CEX: PAED-001 to 004, 006 to 008, 012 and 013. It does not offer PAED-010 (T122). The
  Assessor picker offers exactly KGK's active Assessors, by name and address: Dr Botha, Dr Khumalo, Dr Naidoo, Dr Patel
  and Dr Zulu. It never offers Dr Dlamini or another registrar, nor Prof Mbatha, Mr Smit, Dr Mokoena, Dr Sithole or Dr
  van Rensburg (T102). No lateness warning shows for `D−10`. Save draft opens the activity's page with the focused
  result "Draft saved. It has not been submitted. It is in nobody's inbox until you submit it." (T127). Its status card,
  "Who has it now", is badged Draft and reads "With you. Not submitted yet."; there is no "State:" line and no summary
  card (T342).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): From Home ("Trainee · Semester 2, 2026") Log an activity:
  three groups, Rated by an assessor (7 types), Discussed or reviewed, not rated (3) and Logged by you (1), eleven
  names once each, no Multi-Source Feedback or Learner Feedback. Mini-CEX (Paediatrics) opened Request ("You fill this
  in"), Entrustment and Feedback locked, "David Naidoo fills this in" once he was named, the button then Submit to
  David Naidoo. EPA picker PAED-001 to 004, 006 to 008, 012, 013 (no PAED-010); Assessor picker Botha, Khumalo,
  Naidoo, Patel, Zulu by name and address, nobody else. No warning for 2026-10-10. Save draft opened activity 1 with
  the focused "Draft saved. It has not been submitted. It is in nobody's inbox until you submit it."; status card "Who
  has it now", Draft, "With you. Not submitted yet."; no "State:" line, no summary card.
Gap: none

### Step 3.2 — Dr Dlamini reopens the draft, and a submit with a field missing is refused
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Find the draft in My activities and open it. Press Submit without filling in the presenting problem.
Expect: My activities opens with Needs you, badged 1: the draft, linked as "Mini-CEX (Paediatrics) · PAED-001 · `D−10`"
  with "to David Naidoo" under it, badged Draft, and "Not submitted yet. It is in nobody's inbox until you submit it."
  All activities (1) lists it again: Who has it now "You", State Draft, Credit "—". On the draft's page the Request
  fields are open, and the status card reads "With you. Not submitted yet." and "Finish the request and submit it. It is
  in nobody's inbox until you submit it." The actions read Submit to David Naidoo, Save draft, Discard changes (marked
  unavailable, with "Nothing to discard yet." beside it, until something is typed; T350) and, last and quiet, Cancel
  this draft…. Above them: "When you submit: it goes
  to David Naidoo's Activity inbox and stays Requested until David Naidoo acts on it." The history has one row: Create,
  — → Draft, by Anele Dlamini. The submit is refused: the summary "Not submitted. It is still a draft. Fix the field
  below and submit again." takes the focus, and its line "Presenting problem: A value is required." links to the field,
  which is marked and reads "A value is required." (T263). The activity stays a draft.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): My activities: Needs you badged 1, "Mini-CEX (Paediatrics) ·
  PAED-001 · 2026-10-10, to David Naidoo", Draft, "Not submitted yet. It is in nobody's inbox until you submit it.";
  All activities (1): You, Draft, "—". The draft's page: status "With you. Not submitted yet." / "Finish the request
  and submit it. It is in nobody's inbox until you submit it."; actions Submit to David Naidoo, Save draft, Discard
  changes (aria-disabled, "Nothing to discard yet."), Cancel this draft…; "When you submit: it goes to David Naidoo's
  Activity inbox and stays Requested until David Naidoo acts on it."; history one row, Create, — → Draft, Anele
  Dlamini. Submit refused: the focused summary "Not submitted. It is still a draft. Fix the field below and submit
  again." with "Presenting problem: A value is required." linking to #presenting_problem-in; the field aria-invalid,
  "A value is required.", described by the summary. Still Draft.
Gap: none

### Step 3.3 — Dr Dlamini completes the request and submits it
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Type the presenting problem "Two-year-old with bronchiolitis and rising work of breathing" and submit.
Expect: The focused result reads "Submitted. It is now Requested. It is in David Naidoo's Activity inbox." The status
  card, badged Requested, reads "With David Naidoo since …" and "Nothing for you to do. You can cancel the request until
  David Naidoo acts on it.", and its one action is Cancel request…, quiet. The page is read-only to her: Request reads
  "Filled in by Anele Dlamini, `D`", and Entrustment and Feedback are locked, "David Naidoo fills this in". The history
  adds Submit, Draft → Requested. It carries no lateness note, because ten days is on time (D15). No email is sent:
  Wombat mails nobody when an activity moves, so Dr Naidoo learns of the request from his inbox.
Note: The AssessmentRequested, AssessmentCompleted and AssessmentDeclined email templates exist, but nothing in the
  product sends them.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): The focused "Submitted. It is now Requested. It is in David
  Naidoo's Activity inbox." Status Requested, "With David Naidoo since 2026-10-20 12:31 SAST.", "Nothing for you to
  do. You can cancel the request until David Naidoo acts on it.", one action Cancel request…. Request "Filled in by
  Anele Dlamini, 2026-10-20"; Entrustment and Feedback "David Naidoo fills this in". History adds Submit, Draft →
  Requested, no lateness line. No stub email (23 before, 23 after).
Gap: none

### Step 3.4 — An assessor who was not named cannot open the request
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Look for Dr Dlamini's Mini-CEX in the inbox, then open its address directly, using the id from Step 3.1.
Expect: The inbox's subtitle reads "What waits for you to rate, review or discuss." Its section Waiting for you reads
  "Inbox clear" and "Nothing is waiting for you.", because no request names him yet, and its section Decided by you
  reads "No decisions yet." (T350). The activity's page is headed "Activity unavailable" and reads "This activity does
  not exist, or you cannot open it.", with Go to Activity inbox, the list his acting role opens activities from (T350).
  It reads exactly so for an id that does not exist, so walking ids discloses nothing (T101, C7).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Inbox: "What waits for you to rate, review or discuss.";
  Waiting for you "Inbox clear" / "Nothing is waiting for you."; Decided by you "No decisions yet." /activities/1 and
  /activities/99999 both "Activity unavailable" / "This activity does not exist, or you cannot open it." with Go to
  Activity inbox; main text and tab title identical.
Gap: none

### Step 3.5 — Dr Naidoo rates the Mini-CEX and completes it
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Dlamini's Mini-CEX in it.
  Rate the supervision the encounter required at rung 4, write what was done well, the areas for development and the
  agreed plan, and complete it.
Expect: The inbox's row is its own link, "Mini-CEX (Paediatrics) · PAED-001 · `D−10`" with "from Anele Dlamini" under
  it; the EPA cell "PAED-001 — Providing paediatric emergency care to children"; Requested; Waiting "Less than a day"
  over "since … SAST". The row has no Open button, and the table no Subject, Encounter date or Updated column (T350).
  On the page, the status card reads "Your move. Anele Dlamini asked you on …" and "Complete it, or decline it with a
  note Anele Dlamini will read." Request is read-only, "Filled in by Anele Dlamini, `D`", and its Assessor reads "David
  Naidoo", with no email; only Entrustment and Feedback can be filled in. The actions are Complete, Discard changes
  (greyed, "Nothing to discard yet." beside it) and Decline. The rating is the rung picker: six radios labelled by the
  College's rungs, 1, 2, 3a, 3b, 4 and 5 (D32), with "What each rung means" open under them; choosing 4 marks it
  "chosen" and puts its descriptor, "Unsupervised practice. …", under the row (T350). After Complete, the focused
  result reads "Completed. Nothing else waits for you.", with Go to Home after it; the status card reads "Done. You
  completed it on …" and "Rated 4. Credited 1 item to PAED-001.", and the page is read-only. The history's Complete row
  reads Requested → Completed, by David Naidoo, credited "1 item". Year 3's minimum on PAED-001 is rung 4, so the
  encounter counts at the minimum; nothing on the page says the minimum (Q7), so that is the tester's check.
Note: Dr Naidoo also holds CommitteeMember, so his sessions open acting as a Committee member, whose menu has no
  Activity inbox. The switch is stored with his account, so he acts as Assessor until Step 4.5 switches back.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home "Committee member · Semester 2, 2026" with "1 activity
  waits for you in the Activity inbox: …, waiting less than a day." and Open it; Switch to Assessor: "You are now
  acting as Assessor.", menu Home, Activity inbox, My data rights. Inbox columns Activity, EPA, State, Waiting; the
  row's one link "Mini-CEX (Paediatrics) · PAED-001 · 2026-10-10" with "from Anele Dlamini", EPA "PAED-001 — Providing
  paediatric emergency care to children", Requested, "Less than a day" over "since 2026-10-20 12:31 SAST"; no Open
  button. Status "Your move. Anele Dlamini asked you on 2026-10-20 12:31 SAST." / "Complete it, or decline it with a
  note Anele Dlamini will read."; Request read-only, Assessor "David Naidoo" (no address); only the six radios and
  three Feedback fields enabled; Complete, Discard changes (aria-disabled, "Nothing to discard yet."), Decline. Radios
  1, 2, 3a, 3b, 4, 5, "What each rung means" open; 4 marked "chosen" with "Unsupervised practice. …" under the row.
  After Complete: the focused "Completed. Nothing else waits for you.", Go to Home; "Done. You completed it on
  2026-10-20 12:32 SAST." / "Rated 4. Credited 1 item to PAED-001."; read-only; history Complete, Requested →
  Completed, David Naidoo, "1 item". Nothing shows the minimum.
Gap: none

### Step 3.6 — Dr Dlamini sees the completed Mini-CEX in My activities
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int} → /portfolio/progress/{EpaId:int}
Do: Open My activities, then the Mini-CEX. Then follow its status card's Open My progress.
Expect: Needs you is gone. Under All activities the row reads Who has it now "Done", State Completed, and Credit "1
  item", a link to PAED-001's page under My progress named "1 item to PAED-001, in My progress" (T355). The page is
  read-only. Its status card, badged Completed, reads "Done. David Naidoo completed it on …" and "Rated 4. Credited 1
  item to PAED-001. PAED-001: 1 of 3 this semester.", with Open My progress, a link named "Open My progress at PAED-001"
  (T355). Entrustment shows rung 4 chosen, and Feedback Dr Naidoo's three texts, "Filled in by David Naidoo, `D`". No
  move is offered. The history holds Create, Submit and Complete, with Dr Naidoo as the actor of the Complete. Open My
  progress lands on PAED-001's page, `/portfolio/progress/{EpaId}`, whose h1, "PAED-001 — Providing paediatric emergency
  care to children", takes the focus; the menu's My progress is lit.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): My activities: no Needs you; All activities (1): Done,
  Completed, Credit "1 item", a link to /portfolio/progress/2 named "1 item to PAED-001, in My progress". The page
  read-only, no move; status Completed, "Done. David Naidoo completed it on 2026-10-20 12:32 SAST.", "Rated 4.
  Credited 1 item to PAED-001. PAED-001: 1 of 3 this semester.", Open My progress (aria-label "Open My progress at
  PAED-001"); Entrustment 4 "chosen", Feedback's three texts "Filled in by David Naidoo, 2026-10-20"; history Create,
  Submit, Complete (David Naidoo). Open My progress landed on /portfolio/progress/2, focus on its h1 "PAED-001 —
  Providing paediatric emergency care to children"; the menu's My progress lit (aria-current).
Gap: none

### Step 3.7 — Dr Dlamini's progress counts the Mini-CEX against this semester's target
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Open My progress from the menu, then PAED-001 from its index.
Expect: My progress's subtitle reads "Training year 3 · Semester 2, 2026". The "This period" card reads:
  - "0 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in 2026";
  - "Semester 2, 2026 ends on 2026-11-30.";
  - "Training year 3 — it sets the minimum level each encounter is judged against.";
  - "Multi-source feedback: 0 of 15 EPAs covered by a released campaign that closed this semester. …".
  Under Your EPAs, PAED-001's row in "Each semester · 10 EPAs" reads "PAED-001 — Providing paediatric emergency care to
  children" (its link), "1 of 3 this semester" with a bar and "2 more by 2026-11-30", "Decided each semester", and STAR
  "No decision". No chart is drawn on My progress (T355). PAED-001's page, `/portfolio/progress/{EpaId}`, is headed with
  the EPA's name, "3 a semester · Decided each semester · Exit level 5" under it. Observations reads "1 of 3 this
  semester" with a bar, "2 more by 2026-11-30.", "At the minimum level when observed: 1 of 1", "Last encounter `D−10`",
  "Semester 1, 2026: 0 of 3, 3 short" and "Training year 3: level 4, the minimum each encounter is judged against and
  your STAR's target." Entrustment reads "No STAR yet." The rating trajectory reads "1 rating so far, from David Naidoo.
  At the minimum.", its chart marking Today, and its table one row: `D−10`, 4, "At or above (4, training year 3)", the
  Mini-CEX, David Naidoo. Activities on this EPA lists the Mini-CEX, Completed, Credit "1 item".
Note: A figure is always "n of m" for a named window, never a lifetime total (T130, DESIGN.md § progress figures).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): My progress: "Training year 3 · Semester 2, 2026"; "0 of 10"
  EPAs met this semester, "0 of 5" EPAs met in 2026, "Semester 2, 2026 ends on 2026-11-30.", "Training year 3 — it
  sets the minimum level …", "Multi-source feedback: 0 of 15 EPAs covered by a released campaign that closed this
  semester. …". "Each semester · 10 EPAs": PAED-001 "1 of 3 this semester", "2 more by 2026-11-30", "Decided each
  semester", "No decision". No svg on My progress. /portfolio/progress/2: "3 a semester · Decided each semester · Exit
  level 5"; "1 of 3 this semester", "2 more by 2026-11-30.", "At the minimum level when observed: 1 of 1", "Last
  encounter 2026-10-10", "Semester 1, 2026: 0 of 3, 3 short", "Training year 3: level 4, the minimum …"; "No STAR
  yet."; "1 rating so far, from David Naidoo. At the minimum.", chart with Today; one row 2026-10-10, 4, "At or above
  (4, training year 3)", the Mini-CEX, David Naidoo; Activities: the Mini-CEX, Completed, "1 item".
Gap: none

## Phase 3.B — Filing rules, a decline and a re-filing

### Step 3.8 — A future encounter date is refused
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new → /activities/new?type=mini_cex_cpsa
Do: Start a Mini-CEX (Paediatrics) for an encounter on the ward, from Log an activity's picker. Fill in the EPA
  PAED-002, the assessor Dr Fatima Khumalo, the setting Ward, the presenting problem "Nine-month-old with
  gastroenteritis and moderate dehydration" and the complexity Low. By mistake, type the date observed as `D+1`. Submit.
Expect: The submit is refused. The summary "Nothing was saved. Everything you typed is kept below." takes the focus, and
  its line "Date observed: The date cannot be after today (`D`)." links to the date field. The field is marked, reads
  "The date cannot be after today (`D`).", and names the summary; everything typed is kept (T160, T263, C8). No activity
  exists.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Every field filled, date 2026-10-21. Submit to Fatima Khumalo
  refused: the focused summary "Nothing was saved. Everything you typed is kept below." with "Date observed: The date
  cannot be after today (2026-10-20)." linking to #observed_on-in; the field aria-invalid with that message, described
  by the summary; every value kept. No activity created (the next filed is id 2).
Gap: none

### Step 3.9 — A date before the programme started is refused
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new?type=mini_cex_cpsa
Do: Change the date observed to `J−1d`, the day before his programme started (2026-01-14 on a replay in 2026), and
  submit again.
Expect: Once the date is entered, the field is marked, and below it reads "This date is before your programme started
  (`J`), and will not be accepted.": worded to him, the registrar (C12). The submit is refused: "Nothing was saved.
  Everything you typed is kept below.", with the line "Date observed: The date cannot be before your programme started
  (`J`)." A Mini-CEX can credit, so its date is held to the programme start (T160, T192). No activity exists. An
  assessor or the committee reading such a form reads "the trainee's programme" instead.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Typing 2026-01-14 marked the field: "This date is before your
  programme started (2026-01-15), and will not be accepted." Submit refused: the focused "Nothing was saved.
  Everything you typed is kept below." with "Date observed: The date cannot be before your programme started
  (2026-01-15)." No activity created. Until that submit the summary kept the previous refusal's line (flow 03's 3.10,
  adopted).
Gap: none

### Step 3.10 — A late filing is warned about and recorded, never refused
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: Change the date observed to `D−20`, the day the encounter happened, and submit.
Expect: Changing the date drops the last refusal's mark at once (T263), and the field warns: "This encounter was 20 days
  ago. It can still be filed, but a filing more than 14 days after the encounter is recorded as late." The line above
  the actions adds "Filed today, 20 days after the encounter: it will be recorded as late." Submit to Fatima Khumalo
  succeeds: "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox." The status card adds "Filed 20
  days after the encounter: recorded as late.", About's Filed reads "`D`, 20 days after the encounter (late)", and in
  the history the Submit row's time carries "Filed 20 days after the encounter" (D15, T160).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Typing 2026-09-30 dropped the mark at once (aria-invalid gone)
  and warned "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the
  encounter is recorded as late."; the check line "Filed today, 20 days after the encounter: it will be recorded as
  late." Submit to Fatima Khumalo: activity 2 (R2), "Submitted. It is now Requested. It is in Fatima Khumalo's
  Activity inbox."; status adds "Filed 20 days after the encounter: recorded as late."; About's Filed "2026-10-20, 20
  days after the encounter (late)"; the Submit row "Filed 20 days after the encounter".
Gap: none (the summary kept until the next submit is flow 03's 3.10, adopted in STATE's open questions)

### Step 3.11 — Dr Khumalo declines the request, with a reason
Role: Assessor — Dr Fatima Khumalo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Ndlovu's Mini-CEX. Press Decline and send it with the reason left empty. Then send it with the reason "I was
  not on the ward that day; Dr Botha observed this encounter. Please send it to her."
Expect: The status card reads "Your move. Sipho Ndlovu asked you on …". Decline opens the note panel under the actions,
  "Decline this request", and moves the focus into "Note for Sipho Ndlovu", marked required, with "Sipho Ndlovu reads
  it on the activity's page. It is kept with the activity's history." under it. Sent empty with Decline with this note,
  it is refused: the panel stays open, with the note as typed, and its summary reads "Not declined. It is still
  Requested." and "Note for Sipho Ndlovu: Decline requires a note."; the activity stays Requested (T350). Keep the
  request would close the panel and hand the focus back to Decline. Sent with the reason, the result reads "Declined.
  Nothing else waits for you.", with Go to Home after it. The status card, badged Declined, reads "Closed. You declined
  it on …", quotes her reason, and reads "It credits nothing, and nothing more can happen to it." The page is
  read-only, with no move left. The history's Decline row, Requested → Declined, is credited "—", with her note on a
  row of its own under it. Her inbox's Waiting for you reads "Inbox clear", and Decided by you ("1 decision") holds the
  request, Declined, decided "… SAST", Credit "—".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Status "Your move. Sipho Ndlovu asked you on 2026-10-20 12:33
  SAST." Decline (aria-expanded) opened "Decline this request", focus in "Note for Sipho Ndlovu *" (required), help
  "Sipho Ndlovu reads it on the activity's page. It is kept with the activity's history." Empty: refused, panel open,
  the focused "Not declined. It is still Requested." / "Note for Sipho Ndlovu: Decline requires a note." (link
  #note-in); still Requested. Keep the request closed the panel, focus on Decline. With the reason: the focused
  "Declined. Nothing else waits for you.", Go to Home; status Declined, "Closed. You declined it on 2026-10-20 12:33
  SAST.", her reason, "It credits nothing, and nothing more can happen to it."; nothing enabled; history Decline,
  Requested → Declined, "—", her note on its own row. Inbox: "Inbox clear"; Decided by you "1 decision": the Mini-CEX,
  Declined, "2026-10-20 12:33 SAST", "—".
Gap: none

### Step 3.12 — Dr Ndlovu reads the decline and files the encounter again, naming Dr Botha
Role: Trainee — Dr Sipho Ndlovu
Route: / → /activities/{ActivityId:int} → /activities/new?from={ActivityId} → /activities/{ActivityId:int} → /
Do: Open the declined Mini-CEX from Home's Recent decisions. Then file the same encounter again from its page, File it
  again, to someone else, naming Dr Sarah Botha, and submit. Then read Home again.
Expect: Home's Recent decisions holds one row: the link "Mini-CEX (Paediatrics) · PAED-002 · `D−20`", "to Fatima
  Khumalo" on its second line, a red Declined badge and `D`, and under it "File it again, to someone else", a link of
  its own named "File it again, to someone else: Mini-CEX (Paediatrics) · PAED-002 · `D−20`" (T355, E5). No card is one
  link around its rows (T280). Home's Needs you card reads "Nothing needs you. Requests you have filed are in My
  activities.": a declined request has no move left (T297). No mail tells him of the decline (T320), so Recent decisions
  and My activities are where he finds it. Its page's status card, badged Declined, reads "Closed. Fatima Khumalo
  declined it on …", quotes her reason, and reads "It credits nothing, and nothing more can happen to it. To be assessed
  on this encounter, file it again and name someone else.", with File it again, to someone else. That opens Log an
  activity on the Mini-CEX, under the notice "Copied from your request to Fatima Khumalo, which was declined. The EPA,
  date and request are as you filed them. Name someone else, then submit. Nothing is saved until you do." Everything is
  copied but the assessor, which is empty, and the date field shows at once "This encounter was 20 days ago. It can
  still be filed, but a filing more than 14 days after the encounter is recorded as late." The button reads Submit until
  Dr Botha is named, then Submit to Sarah Botha. The re-filing is a new activity: "Submitted. It is now Requested. It is
  in Sarah Botha's Activity inbox." Its own Submit row records "Filed 20 days after the encounter". The two now share
  type, EPA and date, so each name adds its assessor: "Mini-CEX (Paediatrics) · PAED-002 · `D−20` · Fatima Khumalo" and
  "… · Sarah Botha" (E7). Needs you lists neither: the new request is with Dr Botha, and he may cancel it from its page.
  Back on Home, Recent decisions still lists only the Declined row: the new request is Requested, which is no decision,
  until Step 3.13 completes it. Your targets' Furthest short reads PAED-001 to PAED-005, each "0 of 3 this semester · 3
  more by 2026-11-30".
Note: Declined is a dead end by design: the seed's workflow has no move out of it. The declined record is kept.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home: Needs you "Nothing needs you. Requests you have filed
  are in My activities."; Recent decisions one row, "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-30" with "to Fatima
  Khumalo", badge-declined, 2026-10-20, and its own link "File it again, to someone else" (aria-label "File it again,
  to someone else: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-30", /activities/new?from=2); no card is a link. Its
  page: "Closed. Fatima Khumalo declined it on 2026-10-20 12:33 SAST.", her reason, "… To be assessed on this
  encounter, file it again and name someone else.", File it again, to someone else. That opened the form under "Copied
  from your request to Fatima Khumalo, which was declined. … Nothing is saved until you do.", everything copied,
  Assessor empty, the late warning at once, Submit then Submit to Sarah Botha. Activity 3 (R3): "Submitted. It is now
  Requested. It is in Sarah Botha's Activity inbox.", heading "… · 2026-09-30 · Sarah Botha", "Filed 20 days after the
  encounter". My activities "… · Sarah Botha" (Requested) and "… · Fatima Khumalo" (Closed, Declined). Home after:
  "Nothing needs you."; Recent decisions only the Declined row ("… · Fatima Khumalo"); Furthest short PAED-001 to 005,
  each "0 of 3 this semester · 3 more by 2026-11-30".
Gap: none

### Step 3.13 — Dr Botha completes Dr Ndlovu's Mini-CEX
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Ndlovu's re-filed Mini-CEX
  in it. Rate it 3a with feedback and complete it. Then choose Switch to Committee member in the sidebar.
Expect: The inbox lists the re-filed Mini-CEX alone under Waiting for you. She rates it on the rung picker, choosing
  3a. After Complete the result reads "Completed. Nothing else waits for you." (T350). The Mini-CEX is Completed,
  credited "1 item". Year 1's minimum on PAED-002 is 3a, so it counts at the minimum. The declined request stays
  Declined and credits nothing. Each switch lands on Home with its one-time info alert, "You
  are now acting as Assessor." and then "You are now acting as Committee member." (Step 2.34), and the sidebar ends
  reading "Acting as Committee member".
Note: Dr Botha also holds CommitteeMember, so her sessions open acting as a Committee member, whose menu has no Activity
  inbox, and in-app navigation never switches the role (R2-Rules § 1). The switch is stored with her account, so she
  switches to rate and back after. Every step that sends her or Dr Zulu to rate does the same, which keeps the frames the
  later steps read: Step 3.33's first "Acting as Committee member", Step 4.14's Decision panels and Step A.5.11's head.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Switch to Assessor: "You are now acting as Assessor."; the
  inbox held activity 3 alone (Sipho Ndlovu, PAED-002, Requested). Rated 3a with feedback: the focused "Completed.
  Nothing else waits for you.", Go to Home; "Rated 3a. Credited 1 item to PAED-002.". Activity 2 stays Declined.
  Switch to Committee member: "You are now acting as Committee member.", sidebar "Acting as Committee member".
Gap: none

## Phase 3.C — A reflection returned for more detail

### Step 3.14 — Dr Ndlovu submits a reflective exercise to Dr Botha
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new → /activities/new?type=reflective_exercise_cpsa → /activities/{ActivityId:int}
Do: File a Reflective Exercise (Paediatrics) on a critical incident. Fill in the EPA PAED-001, the supervisor
  Dr Botha and the date of the incident `D−20`. For what happened, write "A three-year-old in septic shock waited
  90 minutes for antibiotics after triage". Add the analysis, what he learned and a one-line plan, then submit.
Expect: He finds it under Discussed or reviewed, not rated. The EPA picker offers only PAED-001, 003, 008 and 014, the
  EPAs whose list names the reflective exercise (D45). Reflection's fields follow the seed: EPA, Supervisor or mentor,
  Date of the case or incident, Reflecting on (Critical incident), then the four answers "What happened", "What went
  well, and what could have gone differently", "What I learned" and "What I will do differently". The form has no
  Entrustment section, because the exercise is unrated (D6). Discussion is locked, "Sarah Botha fills this in" once she
  is named. No lateness warning shows for `D−20`: a reflective exercise credits nothing, so its filing is late for
  nobody (T160). Submit to Sarah Botha reads "Submitted. It is now Awaiting discussion. It is in Sarah Botha's Activity
  inbox.", and the Submit row carries no lateness note.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Reflective Exercise (Paediatrics) under "Discussed or
  reviewed, not rated". EPA picker PAED-001, 003, 008, 014. Fields EPA, Supervisor or mentor, Date of the case or
  incident, Reflecting on (A challenging case / A critical incident), What happened, What went well, and what could
  have gone differently, What I learned, What I will do differently; no Entrustment; Discussion "Sarah Botha fills
  this in" once named. No warning for 2026-09-30, no "Filed today" line. Activity 4 (R4): "Submitted. It is now
  Awaiting discussion. It is in Sarah Botha's Activity inbox."; Credit "None: a reflective exercise credits nothing";
  no lateness note.
Gap: none

### Step 3.15 — Dr Botha returns the reflection for more detail
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Dr Ndlovu's reflection from Activity inbox. Return it with the
  note "Please say what you would do differently at triage, and what you will read before your next take." Then choose
  Switch to Committee member in the sidebar.
Expect: The inbox lists the reflection under Waiting for you, badged Awaiting discussion. The page offers Record
  discussion, Discard changes (greyed, "Nothing to discard yet." beside it) and Return, and only the Discussion field is
  open. Its status card reads "Your move. Sipho Ndlovu asked you on …" and "Record the discussion, or return it with a
  note Sipho Ndlovu will read." Return opens the note panel, "Return this reflection", with the focus in "Note for Sipho
  Ndlovu", marked required; Return with this note (primary) sends it, and Keep the reflection would close the panel
  (T350). Once it is returned, the result reads "Returned to Sipho Ndlovu. Nothing else waits for you.", with Go to Home after
  it; the status card reads "With Sipho Ndlovu. Sarah Botha returned it on …" and "It is in nobody's inbox until Sipho
  Ndlovu submits it.", quoting her note, and the page offers Dr Botha nothing. The history's Return row (Awaiting
  discussion → Draft) carries her note.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Acting as Assessor. Inbox: the reflection, from Sipho Ndlovu,
  Awaiting discussion. Status "Your move. Sipho Ndlovu asked you on 2026-10-20 12:34 SAST." / "Record the discussion,
  or return it with a note Sipho Ndlovu will read."; Record discussion, Discard changes (aria-disabled), Return; only
  discussion_notes open. Return: "Return this reflection", focus in the required "Note for Sipho Ndlovu *"; Keep the
  reflection closed it (focus on Return). Return with this note: the focused "Returned to Sipho Ndlovu. Nothing else
  waits for you.", Go to Home; status Draft, "With Sipho Ndlovu. Sarah Botha returned it on 2026-10-20 12:34 SAST.",
  her note, "It is in nobody's inbox until Sipho Ndlovu submits it."; nothing enabled; Return row Awaiting discussion
  → Draft with her note. Switched back to Committee member.
Gap: none

### Step 3.16 — Dr Ndlovu finds the returned reflection in his inbox and submits it again
Role: Trainee — Dr Sipho Ndlovu
Route: / → /activities/{ActivityId:int} → /activities/inbox
Do: From the dashboard's Needs you card, open the reflection. Expand "What I will do differently": "Start the sepsis
  bundle at triage; read the paediatric sepsis guideline before my next take". Submit again. Then type the Activity
  inbox's address.
Expect: Needs you, badged 1, lists the reflection alone, linked as "Reflective Exercise (Paediatrics) · PAED-001 ·
  `D−20`" with "with Sarah Botha" under it, badged Draft: "Returned to you by Sarah Botha on `D`. Change it and submit
  again." It does not list the declined Mini-CEX (T297). On its page the status card reads "With you. Sarah Botha
  returned it on …", quotes her note, and reads "Change your reflection and submit it again." His fields are open again,
  with Submit to Sarah Botha, Save draft, Discard changes ("Nothing to discard yet." beside it until he types) and
  Cancel this draft…. The submit reads "Submitted. It is now Awaiting discussion. It is in Sarah Botha's Activity
  inbox." The history reads Create, Submit, Return and Submit, and neither Submit row carries a lateness note. Needs you
  no longer lists it. The Activity inbox is not his: under its subtitle, "What waits for you to rate, review or
  discuss." (T350), it reads "Nothing here is yours to act on." and "This inbox holds work that assessors rate,
  discuss or review. Your drafts and work returned to you are under My activities, in Needs you.", with Open My
  activities (T342).
Note: A re-submission after a return is not a new filing (`Workflow.LeftInitialStateLeadingOn`, `ActivityService.IsTheFiling`).
  On this type nothing shows it, because a reflective exercise records no lateness at all.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home's Needs you badged 1: "Reflective Exercise (Paediatrics)
  · PAED-001 · 2026-09-30", "with Sarah Botha", Draft, "Returned to you by Sarah Botha on 2026-10-20. Change it and
  submit again."; not the declined Mini-CEX. The page: "With you. Sarah Botha returned it on 2026-10-20 12:34 SAST.",
  her note, "Change your reflection and submit it again."; his eight fields open; Submit to Sarah Botha, Save draft,
  Discard changes, Cancel this draft…. Submit: "Submitted. It is now Awaiting discussion. It is in Sarah Botha's
  Activity inbox."; history Create, Submit, Return, Submit, no lateness note. Home: "Nothing needs you. …"
  /activities/inbox: "What waits for you to rate, review or discuss.", "Nothing here is yours to act on." / "This
  inbox holds work that assessors rate, discuss or review. …", with Open My activities.
Gap: none

### Step 3.17 — Dr Botha records the discussion
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open the re-submitted reflection from Activity inbox, write the
  discussion notes, and record the discussion. Then choose Switch to Committee member in the sidebar.
Expect: The result reads "Discussed. Nothing else waits for you.", with Go to Home after it (T350). The state is
  Discussed, finished and read-only to both of them. The Record discussion row is credited "—":
  the exercise credits nothing (D7), and no "counted towards no curriculum requirement" banner shows (T108). Dr Ndlovu's
  progress does not change.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Inbox held the reflection alone. Record discussion: the
  focused "Discussed. Nothing else waits for you.", Go to Home; status Discussed, "Done. You recorded the discussion
  on 2026-10-20 12:34 SAST." / "A reflective exercise credits nothing. Nothing more happens to it."; history Record
  discussion, Awaiting discussion → Discussed, "—"; no banner. Switched back to Committee member. Ndlovu's view
  read-only, no banner; his PAED-001 "0 of 3", PAED-002 "1 of 3".
Gap: none

## Phase 3.D — Logs, a DOPS and work left open

### Step 3.18 — Dr du Plessis logs a teaching session
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=kgk_teaching_log → /activities/{ActivityId:int}
Do: Choose KGK Teaching Session Log, under Logged by you. Fill in the topic "Recognising the sick child: a triage
  refresher", the EPA PAED-015, the date delivered `D−6`, the audience Interns and the learning objectives. Log it.
Expect: The form is Act 1's: one section, Teaching session, with six fields, the first five required and Supervising
  consultant optional, and no rating. The EPA picker offers all 15 PAED EPAs: the type is not a College instrument, so
  no tool list binds it (D21). The primary button reads Log, since the Supervising consultant receives nothing, with
  Save draft after it, and above them "When you log it: it is Logged at once, and credits nothing. Nobody else acts on
  it." The result reads "Logged." The status card, badged Logged, reads "Done. Logged on …" and "A KGK teaching session
  log credits nothing, and nobody else acts on it." The history's Log row reads Draft → Logged, credited "—", and no
  lateness is recorded.
Note: No seeded CPSA instrument is a procedure log; the generic `procedure_log` belongs to the Demo speciality. KGK's
  own self-logged type stands in for procedure-style logging.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): KGK Teaching Session Log under "Logged by you" (1 type). One
  section, Teaching session: Topic, EPA, Date delivered, Audience, Learning objectives (each required) and Supervising
  consultant (optional); no rating. EPA picker all 15. Buttons Log, Save draft; above them "When you log it: it is
  Logged at once, and credits nothing. Nobody else acts on it." Activity 5 (R5, 2026-10-14): "Logged."; status Logged,
  "Done. Logged on 2026-10-20 12:35 SAST." / "A KGK teaching session log credits nothing, and nobody else acts on
  it."; Log row Draft → Logged, "—"; no lateness recorded.
Gap: none

### Step 3.19 — Dr du Plessis logs two more sessions, one from before his programme
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=kgk_teaching_log → /activities/{ActivityId:int}
Do: Log two more sessions:
  - "Neonatal jaundice for fourth-year students": PAED-004, `D−13`, Medical students.
  - "Oral rehydration for nursing staff", taught as a medical officer before admission: PAED-015, `J−1y−56d`
    (20 November 2024 on a replay in 2026), Nursing staff.
Expect: Each reads "Logged.". `J−1y−56d` is before his programme started (`J−1y`), and it is accepted with no hint. A
  type that credits nothing is held only to "not after today" (T160).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Activity 6 (R6, PAED-004, 2026-10-07, Medical students):
  "Logged." with no hint. Activity 7 (R7, PAED-015, Nursing staff) dated 2024-11-20 (J−1y−56d, before his 2025-01-15
  start): the date field showed no mark and no message (aria-invalid absent), "Logged.", encounter 2024-11-20.
Gap: none

### Step 3.20 — Dr du Plessis abandons a draft started on the wrong instrument
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=dops_cpsa → /activities/{ActivityId:int} → /activities/mine
Do: Start a DOPS (Paediatrics) by mistake. Choose only the EPA PAED-002 and save the draft. Then cancel it from its page.
Expect: The draft saves with its other required fields empty, because saving a draft checks formats only (T105): "Draft
  saved. It has not been submitted. It is in nobody's inbox until you submit it." Cancel this draft…, last among the
  actions, opens "Cancel this draft?": "A cancelled draft cannot be reopened, and it credits nothing.", with Keep the
  draft and Cancel draft. It asks for no note. Cancel draft reads "Cancelled."; the page's subtitle reads "Pieter du
  Plessis's draft, cancelled", the status card, badged Cancelled, reads "Closed. You cancelled it on …" and "It was never
  submitted, and it credits nothing.", and nothing is offered after it. My activities has no Needs you. All activities lists the DOPS as Cancelled, Who has it now "Closed", Credit "—",
  beside the three Logged sessions, each "Done".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): DOPS (Paediatrics) with only PAED-002: activity 8 (R8), the
  focused "Draft saved. It has not been submitted. It is in nobody's inbox until you submit it.", heading "DOPS
  (Paediatrics) · PAED-002 · no date"; My activities then Needs you badged 1, the DOPS draft. Cancel this draft…, last
  after Submit, Save draft and Discard changes, opened "Cancel this draft?" / "A cancelled draft cannot be reopened,
  and it credits nothing.", Keep the draft (focused) and Cancel draft, no note field. Cancel draft: the focused
  "Cancelled.", subtitle "Pieter du Plessis's draft, cancelled", status Cancelled, "Closed. You cancelled it on
  2026-10-20 12:35 SAST." / "It was never submitted, and it credits nothing.", nothing enabled. My activities: no
  Needs you; the DOPS Closed, Cancelled, "—", then the three sessions Done, Logged, "—".
Gap: none

### Step 3.21 — Dr du Plessis asks Dr Khumalo for a CBD
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=cbd_cpsa → /activities/{ActivityId:int}
Do: File a Case-Based Discussion (Paediatrics). Fill in the EPA PAED-002, the assessor Dr Khumalo, the date `D−5`, the
  case discussed (a toddler with a first febrile seizure) and the focus Clinical reasoning. Submit.
Expect: Submit to Fatima Khumalo reads "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox." The
  CBD ends Requested. It stays that way to the end of the act; Step 3.51 reads it from Dr Khumalo's side.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): CBD EPA picker PAED-001 to 005, 008, 009, 011 to 015. Activity
  9 (R9), PAED-002, Khumalo, 2026-10-15, Clinical reasoning: "Submitted. It is now Requested. It is in Fatima
  Khumalo's Activity inbox."
Gap: none

### Step 3.22 — Dr du Plessis asks Dr Patel to review his portfolio
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=portfolio_review_cpsa → /activities/{ActivityId:int}
Do: File a Portfolio and Logbook Review (Paediatrics). Fill in the EPA PAED-015, the reviewer Dr Patel, the review
  period `D−60` to `D−1`, and the note for the reviewer "Three teaching sessions logged; please review them with my
  logbook". Leave the export's file name empty, since it is optional. Submit.
Expect: The EPA picker offers PAED-015 only, the one EPA whose list names the portfolio review (D45). Review request's
  fields follow the seed: EPA, Reviewer, Review period from, Review period to, Portfolio export (file name) and Notes
  for your reviewer, the last two optional. The form has no rating, and its Review section is locked, "Mohammed Patel
  fills this in" once he is named. Submit to Mohammed Patel reads "Submitted. It is now Awaiting review. It is in
  Mohammed Patel's Activity inbox." The status card reads "With Mohammed Patel since …", with Cancel request…. About
  reads Encounter `D−1`, the period's last day, and Credit "None: a portfolio and logbook review credits nothing". The
  review stays Awaiting review to the end of the act.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): EPA picker PAED-015 only. Fields EPA, Reviewer, Review period
  from, Review period to, Portfolio export (file name) and Notes for your reviewer (the last two optional); Review
  "Mohammed Patel fills this in"; no rating. Period 2026-08-21 to 2026-10-19, file name empty. Activity 10 (R10):
  "Submitted. It is now Awaiting review. It is in Mohammed Patel's Activity inbox."; status "With Mohammed Patel since
  2026-10-20 12:35 SAST." with Cancel request…; About Encounter 2026-10-19, Credit "None: a portfolio and logbook
  review credits nothing".
Gap: none

### Step 3.23 — Dr Mahlangu files a DOPS, and the EPA picker follows the tool lists
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/new?type=dops_cpsa → /activities/{ActivityId:int}
Do: File a DOPS (Paediatrics) for a lumbar puncture on a four-month-old with suspected meningitis. First look for
  PAED-010 in the EPA picker. Then choose PAED-002, the assessor Dr Patel, the date `D−8`, the procedure "Lumbar
  puncture", the setting Ward and the complexity Moderate. Submit.
Expect: Request's fields follow the seed: EPA, Assessor, Date observed, Procedure, Clinical setting and Procedure
  complexity. The EPA picker offers the eight EPAs whose list names the DOPS, PAED-001 to PAED-008. It does not offer
  PAED-010, whose list is Direct observation and MSF only (T122); the old runbook filed this DOPS there. Submit to
  Mohammed Patel reads "Submitted. It is now Requested. It is in Mohammed Patel's Activity inbox." The DOPS ends
  Requested.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Request's fields EPA, Assessor, Date observed, Procedure,
  Clinical setting, Procedure complexity. DOPS EPA picker PAED-001 to 008, no PAED-010. Filed PAED-002, Patel,
  2026-10-12, "Lumbar puncture", Ward, Moderate: activity 11 (R11), "Submitted. It is now Requested. It is in Mohammed
  Patel's Activity inbox."
Gap: none

### Step 3.24 — Dr Patel completes Dr Mahlangu's DOPS
Role: Assessor — Dr Mohammed Patel
Route: / → /activities/inbox → /activities/{ActivityId:int}
Do: From the dashboard, open the inbox, then the DOPS. Rate it 3a with feedback, and complete it.
Expect: Home (Assessor view): "Waiting for you" is badged "2 waiting", with the rule line "Oldest first. Overdue once
  it has waited 7 days.", and lists the same two rows as the inbox, in the same order, oldest first (T297, T350):
  "Portfolio and Logbook Review (Paediatrics) · PAED-015 · `D−1`", from Pieter du Plessis, Awaiting review, "Waiting
  less than a day"; then "DOPS (Paediatrics) · PAED-002 · `D−8`", from Nomsa Mahlangu, Requested. The inbox's Waiting
  for you holds those two rows, in that order. He rates the DOPS 3a on the rung picker. After Complete the result
  reads "Completed. 1 more waits for you.", and under it the review's row, Open the next and Back to Activity inbox
  (T350). The DOPS is Completed, credited "1 item". Year 1's minimum on PAED-002 is 3a, so it counts at the minimum.
  The portfolio review stays in his inbox, Home's card is then badged "1 waiting", and Recent decisions lists "DOPS
  (Paediatrics) · PAED-002 · `D−8`", from Nomsa Mahlangu, Completed, dated `D`.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home ("Assessor · Semester 2, 2026"): Waiting for you "2
  waiting", "Oldest first. Overdue once it has waited 7 days.", "Portfolio and Logbook Review (Paediatrics) · PAED-015
  · 2026-10-19", from Pieter du Plessis, Awaiting review, "Waiting less than a day", then "DOPS (Paediatrics) ·
  PAED-002 · 2026-10-12", from Nomsa Mahlangu, Requested; the inbox the same two in that order. DOPS rated 3a: the
  focused "Completed. 1 more waits for you.", under it the review's row, Open the next ("Open the next: Portfolio and
  Logbook Review (Paediatrics) · PAED-015 · 2026-10-19, from Pieter du Plessis") and Back to Activity inbox; "Rated
  3a. Credited 1 item to PAED-002.", Complete row "1 item". Home then "1 waiting" and Recent decisions "DOPS
  (Paediatrics) · PAED-002 · 2026-10-12", from Nomsa Mahlangu, Completed, 2026-10-20, with All your decisions.
Gap: none

## Phase 3.E — Evidence for the annual review

### Step 3.25 — Dr Molefe files six workplace-based assessments
Role: Trainee — Dr Lerato Molefe
Route: /activities/new → /activities/new?type=… → /activities/{ActivityId:int} → /activities/mine
Do: File and submit each of these, filling in every Request field. The clinical context below is the presenting
  problem, the procedure, the case discussed or the context:
  - Mini-CEX, PAED-001, Dr Zulu, `D−12`, Emergency unit, complexity High: status epilepticus in a five-year-old.
  - CBD, PAED-001, Dr Naidoo, `D−11`, focus Escalation and safety: a febrile neutropenic child.
  - DOPS, PAED-001, Dr Patel, `D−9`, Emergency unit, complexity High: intraosseous access in a shocked infant.
  - Mini-CEX, PAED-012, Dr Botha, `D−7`, Ward, complexity Moderate: telling parents of a new diagnosis of type 1
    diabetes.
  - CBD, PAED-012, Dr Khumalo, `D−5`, focus Ethics and consent: a transfusion refused on religious grounds.
  - Direct Observation, PAED-010, Dr Zulu, `D−4`, activity observed Ward round: leading the Monday ward round.
Expect: Each ends Requested, with no lateness warning, since all six are within 14 days, and each result adds its
  assessor's inbox: "It is in Thandi Zulu's Activity inbox." and so on. My activities has no Needs you: all six are with
  their assessors. All activities lists the six, newest encounter first, as Requested, credit "—", Who has it now naming
  each assessor: Thandi Zulu, Fatima Khumalo, Sarah Botha, Mohammed Patel, David Naidoo and Thandi Zulu. The Direct
  Observation's EPA picker offers PAED-010, because its list names Direct observation. The Mini-CEX and DOPS pickers do
  not offer it.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Activities 12 to 17 (R12 Mini-CEX 001 Zulu 2026-10-08, R13 CBD
  001 Naidoo 10-08, R14 DOPS 001 Patel 10-10, R15 Mini-CEX 012 Botha 10-12, R16 CBD 012 Khumalo 10-14, R17 Direct
  Observation 010 Zulu 10-15), each "Submitted. It is now Requested. It is in <assessor>'s Activity inbox." with its
  own name, no warning. PAED-010 offered only by the Direct Observation picker. My activities: no Needs you; All
  activities (6), newest encounter first, each Requested, "—", Who has it now Thandi Zulu, Fatima Khumalo, Sarah
  Botha, Mohammed Patel, David Naidoo, Thandi Zulu.
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
Expect: Each inbox's Waiting for you holds only that assessor's own requests. Each rates on the rung picker. After each
  Complete the result says what is left: Dr Zulu reads "Completed. 1 more waits for you." after her first, with the
  second's row and Open the next, and "Completed. Nothing else waits for you." after her second; Dr Patel reads
  "Completed. 1 more waits for you.", with Dr du Plessis's portfolio review as the next row; Dr Khumalo reads the same,
  with Dr du Plessis's CBD (Step 3.21) still waiting; Dr Naidoo and Dr Botha read "Completed. Nothing else waits for
  you.", with Go to Home (T350). All six end Completed, each credited "1 item". Year 4's
  minimum is rung 5 on PAED-001, PAED-010 and PAED-012. So Dr Patel's rung 4 counts towards the target, but not at the
  minimum.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Each inbox's Waiting for you held only that assessor's own:
  Zulu 12 and 17; Naidoo 13 (acting as Assessor since Step 3.5); Patel du Plessis's review 10 and 14; Botha 15;
  Khumalo du Plessis's CBD 9 and 16. Each rated on the rung picker. Zulu after 12: "Completed. 1 more waits for you.",
  the observation's row, Open the next and Back to Activity inbox; after 17 "Completed. Nothing else waits for you.",
  Go to Home. Patel and Khumalo "Completed. 1 more waits for you." with the review and the CBD as the next row; Naidoo
  and Botha "Completed. Nothing else waits for you.", Go to Home. All six "Credited 1 item" (Zulu 5, 5; Naidoo 5;
  Patel 4; Botha 5; Khumalo 5); Zulu and Botha switched to Assessor and back, each with its info alert.
Gap: none

### Step 3.27 — Dr Dlamini files three more
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/new?type=… → /activities/{ActivityId:int}
Do: File and submit:
  - CBD, PAED-001, Dr Zulu, `D−9`, focus Management plan: diabetic ketoacidosis in a nine-year-old.
  - Clinical Case Analysis, PAED-001, Dr Botha, `D−6`, case "Anaphylaxis, 7-year-old, emergency unit", documents
    reviewed Admission notes and Progress notes, setting Emergency unit, and the reasoning discussed: why adrenaline
    was repeated before the second-line drugs.
  - Mini-CEX, PAED-004, Dr Khumalo, `D−2`, Neonatal unit, complexity Moderate: a term neonate with respiratory
    distress.
Expect: Each ends Requested, its result adding its assessor's inbox: Thandi Zulu's, Sarah Botha's and Fatima Khumalo's.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Activity 18 (R18, CBD 001 Zulu, 2026-10-11), 19 (R19, CCA 001
  Botha, 2026-10-14, documents admission_notes and progress_notes stored) and 20 (R20, Mini-CEX 004 Khumalo,
  2026-10-18): each "Submitted. It is now Requested." adding "It is in Thandi Zulu's Activity inbox.", "…Sarah
  Botha's…" and "…Fatima Khumalo's…"; no lateness warning.
Gap: none

### Step 3.28 — The consultants complete Dr Dlamini's three
Role: Assessor — Dr Thandi Zulu, Dr Sarah Botha and Dr Fatima Khumalo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Each completes their own from Activity inbox with feedback, rating: Dr Zulu 3b on the CBD, Dr Botha 4 on the CCA,
  and Dr Khumalo 4 on the Mini-CEX. Dr Zulu and Dr Botha first choose Switch to Assessor in the sidebar, and after
  rating choose Switch to Committee member (Step 3.13).
Expect: Each rates on the rung picker. Dr Khumalo, who also holds Dr du Plessis's CBD, reads "Completed. 1 more waits
  for you.", with the CBD's row as the next; Dr Zulu and Dr Botha read "Completed. Nothing else waits for you." (T350).
  All three end Completed, each credited "1 item". Dr Dlamini's PAED-001 now holds three encounters this semester.
  Two are at year 3's minimum of rung 4; the CBD at 3b counts towards the target only. Her PAED-004 holds one, at the
  minimum.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): 18 (Zulu, 3b) and 19 (Botha, 4): "Completed. Nothing else
  waits for you.", Go to Home; 20 (Khumalo, 4): "Completed. 1 more waits for you." with du Plessis's CBD as the next
  row. Each "Credited 1 item"; each inbox held only its own; Zulu and Botha switched and back. (SQL at the act's end:
  Dlamini PAED-001 3 counted, 2 at minimum; PAED-004 1, 1.)
Gap: none

## Phase 3.F — Stalled work

### Step 3.29 — Dr Mahlangu asks Dr Zulu for a Mini-CEX, which is left unrated
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics): PAED-004, Dr Zulu, `D−3`, Neonatal unit, "Term neonate with jaundice on day three",
  complexity Low. Submit. Dr Zulu does not act on it.
Expect: Submit to Thandi Zulu reads "Submitted. It is now Requested. It is in Thandi Zulu's Activity inbox." The
  Mini-CEX ends Requested.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Mini-CEX EPA picker the nine Mini-CEX EPAs. Activity 21 (R21,
  PAED-004, Zulu, 2026-10-17, Neonatal unit, Low): "Submitted. It is now Requested. It is in Thandi Zulu's Activity
  inbox."
Gap: none

### Step 3.30 — Mr Smit's dashboard shows what waits for an assessor
Role: Coordinator — Mr Pieter Smit
Route: /
Do: Once Dr Mahlangu's Mini-CEX and Dr du Plessis's portfolio review have waited more than seven days, open the
  dashboard.
Expect: Home's "Waiting for assessors" is in the warning stripe, badged "3 waiting, 2 overdue", under the rule line
  "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5." (T358, Q3). It lists every request
  whose next move names one person, oldest first, each row its link ("Type · EPA · date", then "from <registrar>"), its
  state's badge with Overdue beside it, "With <assessor>" on its own line, then the wait (E6):
  - "Portfolio and Logbook Review (Paediatrics) · PAED-015 · `D−1`", from Pieter du Plessis: Awaiting review, Overdue,
    "With Mohammed Patel", "Waiting 8 days";
  - "Mini-CEX (Paediatrics) · PAED-004 · `D−3`", from Nomsa Mahlangu: Requested, Overdue, "With Thandi Zulu", "Waiting 8
    days";
  - "Case-Based Discussion (Paediatrics) · PAED-002 · `D−5`", from Pieter du Plessis: Requested, "With Fatima Khumalo",
    "Waiting less than a day" (filed in this sitting, Step 3.21, and not aged).
  Each link opens the activity's page, `/activities/{id}` (T297), and no date on the card is written "26 Sept" (T325).
  Its foot is Open Waiting for assessors. "Nothing filed in 30 days" reads "Every current registrar has filed something
  in the last 30 days." and "Invitations nearing expiry" "No invitations expiring soon.". "Start an MSF campaign" is
  Home's header action; there is no Quick action card.
Note: To play this in one sitting, age both requests by eight days (the statement touches 2 rows):
  `UPDATE "Activities" a SET "UpdatedOn" = a."UpdatedOn" - interval '8 days' FROM "ActivityTypes" t, "AspNetUsers" u
  WHERE t."Id" = a."ActivityTypeId" AND u."Id" = a."SubjectUserId" AND ((t."Key" = 'mini_cex_cpsa' AND u."Email" =
  'mahlangu@kgk.wombat.local' AND a."CurrentState" = 'requested') OR (t."Key" = 'portfolio_review_cpsa' AND u."Email" =
  'duplessis@kgk.wombat.local'));`
  Then age their history rows, each one's Create and Submit, by the same eight days (4 rows), so the status card's
  "asked you on …", read from the last move, agrees with the inbox row's "since …", read from `UpdatedOn` (T350, E5):
  `UPDATE "ActivityTransitions" h SET "OccurredOn" = h."OccurredOn" - interval '8 days' FROM "Activities" a,
  "ActivityTypes" t, "AspNetUsers" u WHERE h."ActivityId" = a."Id" AND t."Id" = a."ActivityTypeId" AND u."Id" =
  a."SubjectUserId" AND ((t."Key" = 'mini_cex_cpsa' AND u."Email" = 'mahlangu@kgk.wombat.local' AND a."CurrentState" =
  'requested') OR (t."Key" = 'portfolio_review_cpsa' AND u."Email" = 'duplessis@kgk.wombat.local'));`
  The ageing leaves each Submit dated before its encounter (the Mini-CEX submitted `D−8` for a `D−3` encounter, the
  review filed `D−8` for a period to `D−1`), and the review's Review request then reads "Filled in by Pieter du Plessis,
  `D−8`". Both follow from the ageing, not from the product.
  The card is Waiting for assessors' first five, read from each activity's pinned workflow (`ActivityWaiting`, T297),
  the predicate the nudge of Step 3.32 reads too, and held by one named person (a `field:` nominee, E3): the Mini-CEX
  waits in `requested`, the portfolio review in `submitted`. A request is listed from its first moment and is Overdue
  once it has waited `DashboardThresholds:AssessorDueDays` (seven); the nudge mails after
  `DashboardThresholds:AssessorNudgeDays` (five). The eight days' ageing puts both past both. If either aged request is
  not marked Overdue, that is a Gap.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Both of the Note's UPDATEs ran after the 09:00 UTC scheduled
  nudge (which nudged 0): "UPDATE 2" (activities 10 and 21) and "UPDATE 4". Home "Coordinator · Semester 2, 2026",
  header action Start an MSF campaign (/msf/campaigns/new), no Quick action card. "Waiting for assessors" in the
  warning stripe, badged "3 waiting, 2 overdue", "Oldest first. Overdue once it has waited 7 days. Its assessor is
  emailed after 5.": "Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-10-19", from Pieter du Plessis,
  Awaiting review, Overdue, "With Mohammed Patel", "Waiting 8 days"; "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-17",
  from Nomsa Mahlangu, Requested, Overdue, "With Thandi Zulu", "Waiting 8 days"; "Case-Based Discussion (Paediatrics)
  · PAED-002 · 2026-10-15", from Pieter du Plessis, Requested, "With Fatima Khumalo", "Waiting less than a day"; links
  /activities/10, 21 and 9; no "Sept"; foot Open Waiting for assessors. "Nothing filed in 30 days": "Every current
  registrar has filed something in the last 30 days."; "Invitations nearing expiry": "No invitations expiring soon."
Gap: none

### Step 3.31 — Mr Smit sends Dr Zulu a reminder from Waiting for assessors
Role: Coordinator — Mr Pieter Smit
Route: / → /programme/waiting → /not-found
Do: Open Waiting for assessors from the menu. On the Mini-CEX's row choose Send a reminder, read the dialog, then choose
  Send the reminder. Then type the address the menu once linked, `/placeholder/stalled-activities`.
Expect: The menu reads Home, Programme trainees, Waiting for assessors, Decisions due, MSF campaigns, Committee reviews,
  Decision panels and Data rights requests, then My data rights, and Waiting for assessors is lit on its page (T358,
  Q7). The page is headed "Waiting for assessors", its subtitle "Requests at Kgosi Kgari Teaching Hospital whose next
  move names an assessor, supervisor or reviewer, read as Coordinator. Your own requests are not listed." (E4), over the
  filters Waiting (All) and With (Anyone), and Show. The list's heading reads "3 waiting, 2 overdue", its rule line
  "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5. Waiting counts from the last move:
  any save restarts it.", and the table's columns are Activity, With, State and Waiting: Step 3.30's three rows in its
  order, each Waiting cell "8 days" or "Less than a day" over "since … SAST", then Send a reminder.
  On the Mini-CEX's row Send a reminder opens a dialog titled "Send Thandi Zulu a reminder?", which says "Thandi Zulu
  gets one email, "Activities awaiting your assessment", listing this request: Mini-CEX (Paediatrics) from Nomsa
  Mahlangu — waiting 8 days. It moves nothing: the request stays Requested, its wait is not restarted, and Nomsa
  Mahlangu is not told." and opens on Don't send (C4). After Send the reminder, above the table and with the focus:
  "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · `D−3`, from Nomsa Mahlangu, waiting 8
  days. The request is still Requested; its wait is unchanged." The list is read again: the Mini-CEX's row reads
  "Reminded `D` by Pieter Smit" where its button was, still Requested, Overdue and "8 days"; the other two keep their
  buttons.
  The log holds one stub mail (tags reminder, assessor-reminder) "Activities awaiting your assessment", "Hi Thandi",
  listing "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days". Nomsa Mahlangu is sent nothing. No page offers
  Reassign (C5). The old address is "Page not found" with status 404: the placeholder page went with the stubs (T335,
  flow 01).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Menu Home, Programme trainees, Waiting for assessors (lit),
  Decisions due, MSF campaigns, Committee reviews, Decision panels, Data rights requests, then My data rights.
  /programme/waiting: subtitle "Requests at Kgosi Kgari Teaching Hospital whose next move names an assessor,
  supervisor or reviewer, read as Coordinator. Your own requests are not listed."; filters Waiting (All / Overdue
  only), With (Anyone, Fatima Khumalo, Mohammed Patel, Thandi Zulu), Show; "3 waiting, 2 overdue" with the full rule
  line; columns Activity, With, State, Waiting; Step 3.30's three rows, "8 days" over "since 2026-10-12 12:35 SAST",
  "8 days" over "since 2026-10-12 12:48 SAST", "Less than a day", each with Send a reminder. The Mini-CEX's dialog
  "Send Thandi Zulu a reminder?" with the body as written, focus on Don't send. Send the reminder: the focused
  "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · 2026-10-17, from Nomsa Mahlangu, waiting
  8 days. The request is still Requested; its wait is unchanged."; the row "Reminded 2026-10-20 by Pieter Smit", still
  Requested, Overdue, "8 days"; the other two keep theirs. One stub mail (tags reminder, assessor-reminder, 23 → 24)
  "Activities awaiting your assessment", "Hi Thandi", "Mini-CEX (Paediatrics) from Nomsa Mahlangu - waiting 8 days"
  (the log writes the em dash as "-"); nothing to Nomsa Mahlangu. No Reassign. /placeholder/stalled-activities: "Page
  not found", "There is no page at this address. …", Go to Home.
Gap: none

### Step 3.32 — The daily nudge reminds the assessors
Role: Administrator — devadmin@wombat.local
Route: /admin/jobs
Do: Run `assessor-pending-nudge` now.
Expect: The job's last run updates. The application log then holds three stub emails "Activities awaiting your
  assessment", naming no address (T282), each greeting its assessor by first name: Step 3.31's reminder (tags reminder,
  assessor-reminder), "Hi Thandi", and the nudge's two (tags nudge, assessor-pending):
  - "Hi Thandi", listing Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days;
  - "Hi Mohammed", listing Portfolio and Logbook Review (Paediatrics) from Pieter du Plessis — waiting 8 days.
  The nudge still mails a request reminded today: a reminder moves nothing (T358, C4). The run's summary line reads
  "assessors nudged 2 (activities 2)" and skips nobody. Nothing else has waited five days, so nobody else is nudged: Dr
  du Plessis's CBD has waited less than a day.
Note: The job reads each activity's pinned workflow and nudges on anything untouched for five days in a state whose
  next move belongs to a `field:` nominee (`AssessorPendingNudgeJob`). D50 decides whom it skips.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Run now on assessor-pending-nudge ("Nudges assessors … (daily
  at 09:00 UTC)"): "Job 'assessor-pending-nudge' dispatched."; Last run moved from 2026-10-20 11:02 (the scheduled
  run, SAST) to 12:49, Succeeded. The log gained two stub emails (tags nudge, assessor-pending, 24 → 26) "Activities
  awaiting your assessment", naming no address: "Hi Mohammed", Portfolio and Logbook Review (Paediatrics) from Pieter
  du Plessis, waiting 8 days; "Hi Thandi", Mini-CEX (Paediatrics) from Nomsa Mahlangu, waiting 8 days (reminded today,
  still nudged). Summary line "AssessorPendingNudgeJob: assessors nudged 2 (activities 2); nominees skipped: no such
  account 0, deactivated 0, opted out of digest emails 0, no email address 0." The CBD was not nudged.
Gap: none

### Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her sessions open acting as a Committee member; choose Switch to Assessor in the sidebar. From "Waiting for you",
  open Dr Mahlangu's Mini-CEX (the Activity inbox lists it too), rate it 3a with feedback, and complete it.
Expect: The sidebar first reads "Acting as Committee member", with "Switch to Assessor" under it, and Home "Committee
  member · Semester N, YYYY" under its heading. Under the header, above the committee cards (Registrars and Targets by
  EPA), a warning line reads "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics)
  · PAED-004 · `D−3`, from Nomsa Mahlangu, waiting 8 days.", with Open it (T350). After the switch Home reads "You are
  now acting as Assessor." in an info alert under the header, and "Assessor · Semester N, YYYY", with no such line; the
  sidebar reads "Acting as Assessor" over Home and Activity inbox, and "Waiting for you", in the warning stripe, is
  badged "1 waiting, 1 overdue": this Mini-CEX, "Mini-CEX (Paediatrics) · PAED-004 · `D−3`", from Nomsa Mahlangu,
  Requested with Overdue beside it, "Waiting 8 days": it has waited past the assessor's seven days since Step 3.30 aged
  it. The inbox lists it with "8 days" over "since `D−8` … SAST"; the page's status card reads "Your move. Nomsa
  Mahlangu asked you on `D−8` … SAST.", the same moment. Step 3.31's reminder has not moved it. Dr Zulu rates it on the
  rung picker. After Complete the result reads "Completed. Nothing else waits for you.", with Go to Home, and Home then
  reads "Nothing is waiting for you." The Mini-CEX is Completed, credited "1 item". Year 1's minimum on PAED-004 is 3a.
  Nothing of Dr Mahlangu's waits for an assessor any more: the Mini-CEX has left Waiting for assessors and Mr Smit's
  Home card.
Note: The card and the line count what her inbox lists, less her own portfolio (`ActivityWaiting`, T297), so neither
  can read 0 beside an inbox holding one; if one does, that is a Gap. The line's Open it is a second way in: it opens
  the activity with no switch, the trail Home › the activity with nothing lit in the menu (T350). The step keeps the
  switch, which later steps' frames depend on.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Home first "Committee member · Semester 2, 2026", sidebar
  "Acting as Committee member" with Switch to Assessor; above Registrars and Targets by EPA the warning line
  (alert-warning other-role-line) "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX
  (Paediatrics) · PAED-004 · 2026-10-17, from Nomsa Mahlangu, waiting 8 days." with Open it. Open it (tried first):
  /activities/21 with no switch, trail Home › the activity, nothing lit; status "Your move. Nomsa Mahlangu asked you
  on 2026-10-12 12:48 SAST.". After the switch: "You are now acting as Assessor." (alert-info), "Assessor · Semester
  2, 2026", no line; "Acting as Assessor" over Home and Activity inbox; Waiting for you (detail-card--warning) "1
  waiting, 1 overdue": the Mini-CEX, Requested, Overdue, "Waiting 8 days". Inbox: Overdue, "8 days" over "since
  2026-10-12 12:48 SAST", the status card the same moment. Rated 3a: "Completed. Nothing else waits for you.", Go to
  Home; "Rated 3a. Credited 1 item to PAED-004."; Home then "Nothing is waiting for you." Mr Smit's card then "2
  waiting, 1 overdue" (the review and the CBD), and With no longer offers Thandi Zulu.
Gap: none

## Phase 3.G — Multi-source feedback for Dr Molefe

### Step 3.34 — Mr Smit creates the MSF questionnaire
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/new
Do: Open MSF campaigns and start a new campaign. No questionnaire exists yet, so add the one the page offers as it
  stands: kind Multi-source feedback, name "Default MSF", with its scale question and its comment question.
Expect:
  - **The list:** "MSF campaigns", "Create and review multi-source feedback campaigns.", with New campaign and then MSF
    coverage in its header; "No MSF campaigns" / "Create the first MSF campaign to start respondent outreach."
  - **The create page** is "New MSF campaign" (its tab too), under the trail Home › MSF campaigns › New MSF campaign
    with MSF campaigns lit. Its subtitle says creating it makes a draft and nobody is emailed until it opens. Four
    cards, each an h2: Questionnaire, Trainee and window, Thresholds and Evidence.
  - **No questionnaire yet (T369, C1):** the Template select reads "No template yet", with "Write the first one below;
    it is chosen here once it is added."; the fold "Write a new questionnaire" is open, pre-filled (Multi-source
    feedback, "Default MSF", "Rates the trainee's overall professional performance.", "What should the trainee keep
    doing or improve?"). Create campaign is aria-disabled, with "Create campaign: write a questionnaire first." beside
    it.
  - **Add template:** "**Template created.** Default MSF is chosen as this campaign's template.", focused. The fold
    closes, the select reads "Default MSF (Multi-source feedback)", chosen, with its two questions under it, and Create
    campaign is offered.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): MSF campaigns (from the menu): "MSF campaigns", "Create and
  review multi-source feedback campaigns.", header New campaign then MSF coverage; "No MSF campaigns" / "Create the
  first MSF campaign to start respondent outreach." New campaign: h1 and tab "New MSF campaign", trail Home › MSF
  campaigns › New MSF campaign, MSF campaigns lit; subtitle "… Creating it makes a draft; nobody is emailed until you
  open it."; four h2 cards Questionnaire, Trainee and window, Thresholds, Evidence. Template select "No template yet"
  (disabled) with "Write the first one below; it is chosen here once it is added."; the fold "Write a new
  questionnaire" open, pre-filled (Multi-source feedback, "Default MSF", "Rates the trainee's overall professional
  performance.", "What should the trainee keep doing or improve?"); Create campaign aria-disabled with "Create
  campaign: write a questionnaire first." Add template: the focused "Template created. Default MSF is chosen as this
  campaign's template."; the fold closed; the select "Default MSF (Multi-source feedback)", chosen, its two questions
  under it; Create campaign a submit, offered. No CSP violation.
Gap: none

### Step 3.35 — Mr Smit creates a campaign for Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int}
Do: Fill in the campaign:
  - trainee Dr Lerato Molefe, with template Default MSF as it is already chosen;
  - opens on `D` and closes on `D+14`, the defaults;
  - Minimum responses 5; Minimum responses in a respondent group 3 and Minimum respondent groups reporting 2, the
    defaults;
  - evidence for PAED-010 and PAED-012.
  Create it.
Expect:
  - **The form:** the Trainee select offers KGK's five current trainees by name and address, and nobody else. The
    thresholds open on 8 / 3 / 2. "Minimum responses in a respondent group" says a group with fewer is suppressed and
    asks at least 3 for multi-source feedback (T369, F2); the help beside "Minimum respondent groups reporting" says
    that two is the College's answer (D11). The window's help says both days are on the South African calendar (T325).
  - **Evidence:** once a trainee is chosen, "Evidence for these EPAs" lists all 15 EPAs of her curriculum, since MSF is
    on every one (D37), with a count over them: "2 of 15 ticked" once the two are ticked.
  - **Created:** the campaign's own page, "MSF campaign for Lerato Molefe", subtitle "Default MSF · Multi-source
    feedback · `D` to `D+14`". Its status card: Draft, "Add invitees, then open the campaign to send anonymous response
    links.", "Nobody has been invited yet." Open campaign is aria-disabled, with "Open campaign: add at least one
    invitee first. …" beside it (T225), and Withdraw campaign… is offered. About this campaign reads Trainee "Lerato
    Molefe, training year 4", Template "Default MSF (Multi-source feedback)", Thresholds "5 responses; 3 in a respondent
    group; 2 respondent groups reporting" and "Evidence for PAED-010 and PAED-012", each EPA by code and title.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Trainee select: the five KGK trainees by name and address,
  nobody else. Defaults 8 / 3 / 2, 2026-10-20 to 2026-11-03. Help: group minimum "A respondent group with fewer is
  suppressed: … At least 3 for multi-source feedback."; groups "… Two is the College's answer: eight peer doctors
  alone are not multi-source feedback."; Opens "Today, on the South African calendar.", Closes "The respondents' last
  day to answer, on the South African calendar." Once Molefe was chosen, "Evidence for these EPAs" listed PAED-001 to
  015, "0 of 15 ticked", then "2 of 15 ticked" with PAED-010 and 012. Typed 5 (3 and 2 kept). D1: each section title
  stands 16 px above its grid, and Evidence's 16 px above its fieldset, at 1280 and at 390 (h2 margin-bottom 16px);
  nothing scrolls sideways at 390. Created campaign 1: "MSF campaign for Lerato Molefe", "Default MSF · Multi-source
  feedback · 2026-10-20 to 2026-11-03"; Draft, "Add invitees, then open the campaign to send anonymous response
  links.", "Nobody has been invited yet."; Open campaign aria-disabled with "Open campaign: add at least one invitee
  first. …"; Withdraw campaign… offered. About: "Lerato Molefe, training year 4", "Default MSF (Multi-source
  feedback)", "5 responses; 3 in a respondent group; 2 respondent groups reporting", "Evidence for PAED-010 and
  PAED-012" with each EPA by code and title.
Gap: none

### Step 3.36 — Mr Smit invites nine colleagues, and removes one added under the wrong group
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Add each colleague by address and respondent group, in this order:
  - Sister Grace Mokwena, `grace.mokwena@kgk.wombat.local`, as a Nurse;
  - Sister Palesa Tau, `palesa.tau@kgk.wombat.local`, by mistake as an Allied health professional;
  - the registrars Dr Kagiso Motsepe, `kagiso.motsepe@kgk.wombat.local`, and Dr Lindiwe Khoza,
    `lindiwe.khoza@kgk.wombat.local`, as Peer doctors;
  - Dr Khumalo, `khumalo@kgk.wombat.local`, and Dr Botha, `botha@kgk.wombat.local`, as Consultants;
  - the physiotherapist Ms Naledi Sebego, `naledi.sebego@kgk.wombat.local`, as an Allied health professional;
  - Sister Thandi Nkosi, `nkosi@kgk.wombat.local`, as a Nurse;
  - Dr Johan van Wyk, `vanwyk@kgk.wombat.local`, as a Consultant.
  Then remove Sister Tau's row and add her again as a Nurse.
Expect:
  - **The add form** ("Add an invitee", "* marks a field you must fill in."): Respondent email, and a Respondent group
    select offering Peer doctor, Consultant, Nurse, Allied health professional and Other, in that order, Peer doctor
    chosen; it offers no Patient and no Learner. The group is kept from one add to the next.
  - **Each add** reads "Invitee added." alone (T369, F11), clears the address and puts the focus back in it.
  - **The addresses:** "Addresses invited", listed only while the campaign is a draft. Under a count ("9 addresses"),
    each address whole over its group, with a Remove… beside it named for its address and group ("Remove
    palesa.tau@kgk.wombat.local (Allied health professional)").
  - **Remove** asks "Remove this invitee?", naming the address and group; its buttons are Keep the invitee and Remove
    invitee. Confirmed, the result above the rows, focused, says "palesa.tau@kgk.wombat.local (Allied health
    professional) has been removed from this campaign, and will not be emailed a link when it opens."
  - **In the end** the counts, captioned that once the campaign opens the page never lists who was invited, read Peer
    doctor 2, Consultant 3, Nurse 3, Allied health professional 1 and All groups 9, with no Responded column. "9
    addresses" lists the nine. Open campaign is offered, with "Opening emails each of the 9 invitees a link to respond,
    with `D+14` as their last day." beside it.
Note: The journey is 12 presses and 18 fields: nine adds, Remove…, Remove invitee and one more add; ten addresses, and
  eight group changes: on seven of nine adds (Khoza's and Botha's keep the one before) and the last (Consultant to Nurse).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Add form "Add an invitee", "* marks a field you must fill
  in.", Respondent email and Respondent group (Peer doctor*, Consultant, Nurse, Allied health professional, Other; no
  Patient, no Learner). Each of the ten adds read "Invitee added." alone, cleared the address and put the focus back
  in it; the group was kept between adds (Khoza's and Botha's needed no change). "9 addresses", each address over its
  group, Remove… named "Remove palesa.tau@kgk.wombat.local (Allied health professional)" and so on. Remove: "Remove
  this invitee?", "Remove palesa.tau@kgk.wombat.local (Allied health professional) from this campaign? …", Keep the
  invitee (focused) and Remove invitee; Escape closed it with the focus back on that row's Remove…. Confirmed: the
  result above the rows, focused (role status), "palesa.tau@kgk.wombat.local (Allied health professional) has been
  removed from this campaign, and will not be emailed a link when it opens." Re-added as Nurse. End: counts captioned
  "… Once the campaign opens, this page never lists who was invited …": Peer doctor 2, Consultant 3, Nurse 3, Allied
  health professional 1, All groups 9, no Responded column; "9 addresses" lists the nine; Open campaign offered with
  "Opening emails each of the 9 invitees a link to respond, with 2026-11-03 as their last day."
Gap: none

### Step 3.37 — Mr Smit opens the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Open the campaign.
Expect: The page reads "Campaign opened; links are being sent.", focused. The status card reads Open and "0 of 9
  invitees have responded." Its note names `D+14` as the respondents' last day, says the campaign closes by itself once
  that day has passed, and that Close campaign on its report closes it sooner. Under "Invitees by respondent group" the
  invitees are now counted only, with a Responded column reading 0, and no address is listed (T217, T247). Beside it,
  the Links part reads "9 links are still being sent. Reload this page to see whether they were delivered." The
  actions are View report and Withdraw campaign…. The application log holds nine "Feedback request: Lerato Molefe
  (Default MSF, …)" emails, each with a `/msf/respond` link.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): At 10:51 UTC (12:51 SAST): the focused "Campaign opened; links
  are being sent."; Open, "0 of 9 invitees have responded."; the note "Each respondent is emailed a link, which gives
  2026-11-03 as their last day to respond. The campaign closes by itself once that day has passed; to close it sooner,
  use Close campaign on its report." "Invitees by respondent group", counts only with Responded 0 (2/3/3/1, All groups
  9), no address; Links "9 links are still being sent. Reload this page to see whether they were delivered."; actions
  View report and Withdraw campaign…. The log gained nine stub emails "Feedback request: Lerato Molefe (Default MSF,
  2026-10-20 to 2026-11-03)" (tags msf-invite, campaign:1; 26 → 35), each with a /msf/respond?token= link on :5180.
Gap: none

### Step 3.38 — Mr Smit withdraws a campaign started in error
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int} → /msf/campaigns
Do: Create a second campaign, for Dr Pieter du Plessis: Default MSF, the defaults, evidence for PAED-015. Then
  Dr Mokoena says his MSF belongs to next semester, so withdraw it from the campaign list.
Expect: The list, "Campaigns (2)", is captioned "MSF campaigns, newest first. Counted by respondent group: this list
  never shows who was invited or who responded." Each row is headed by its trainee, with its links named by its state
  (T225) and by the campaign:
  - Pieter du Plessis first: Default MSF (Multi-source feedback), Draft, Invitations 0, Responses "—" (it never opened,
    T270), closing `D+14`: "Manage" and "Withdraw…";
  - then Lerato Molefe: Open, Invitations 9, Responses 0: "Manage", "View report" and "Withdraw…".
  Withdraw asks "Withdraw this campaign?", names "Pieter du Plessis (Default MSF, closing `D+14`)", and says withdrawing
  cannot be undone; its buttons are Keep the campaign and Withdraw campaign. Confirmed, it reads "The campaign for
  Pieter du Plessis (…) has been withdrawn. Its respondents' links no longer work, and their email addresses have been
  removed.", focused. The row then reads Withdrawn, with only "View campaign".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Campaign 2 created for du Plessis (Default MSF, 8/3/2,
  2026-10-20 to 2026-11-03, PAED-015). The list, "Campaigns (2)", captioned "MSF campaigns, newest first. Counted by
  respondent group: this list never shows who was invited or who responded.": Pieter du Plessis, Default MSF /
  Multi-source feedback, Draft, 0, "—", 2026-11-03, Manage and Withdraw…; Lerato Molefe, Open, 9, 0: Manage, View
  report, Withdraw…; each link named "<label>: the campaign for <trainee> (Default MSF, closing 2026-11-03)". D1:
  "Campaigns (2)" stands 17 px above its table at 1280 and at 390; at 390 the rows stack (tr grid, td block) and
  nothing scrolls sideways. Withdraw: "Withdraw this campaign?", "Withdraw the campaign for Pieter du Plessis (Default
  MSF, closing 2026-11-03)? … withdrawing cannot be undone.", Keep the campaign (focused) and Withdraw campaign;
  Escape returned the focus to that row's Withdraw…. Confirmed: the focused (role status) "The campaign for Pieter du
  Plessis (Default MSF, closing 2026-11-03) has been withdrawn. Its respondents' links no longer work, and their email
  addresses have been removed."; the row Withdrawn with only View campaign.
Gap: none

### Step 3.39 — Three respondents answer from their links
Role: Anonymous — Sister Grace Mokwena, Sister Palesa Tau and Dr Kagiso Motsepe (MSF respondents)
Route: /msf/respond
Do: Each respondent opens their own link from the log, in a browser signed in to nothing, and answers:
  - Sister Mokwena: Above expectations, "Calm and kind with parents at night; hands over clearly".
  - Sister Tau: Well above expectations, "Leads neonatal resuscitations and explains her decisions to the nurses".
  - Dr Motsepe: Meets expectations, "A supportive senior; could delegate more".
Expect: The card's one h1 is "Feedback on Lerato Molefe" (T280), its tab the same, over "Default MSF · Last day to
  respond: `D+14`" and the line asking for multi-source feedback on a trainee they have worked with. "Who sees your
  feedback" comes before the rating: their name and address are never shown to her, and she sees the feedback only
  after it has closed and been released, grouped by respondent role. The rating offers five points, from Well below
  expectations to Well above expectations (D47), and is required. The comment is marked "(optional)", with help that
  reads "Up to 4,000 characters." and warns that it may reach her word for word. Each submit ends on the h1 "Thank you"
  and a success Alert, "Your feedback on Lerato Molefe has been recorded."
Note: The stub emails name no address (T282), and a link's group decides where its answer is counted. Match each link
  to its invitee before handing it out: its `token=` begins with the `TokenSelector` of its row in
  `SELECT "RespondentEmail", "RespondentCategory", "TokenSelector" FROM "MsfInvitations"
  WHERE "CampaignId" = <the campaign's id>;`.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signed out, a fresh browser context each, each link matched to
  its invitee by TokenSelector. One h1 "Feedback on Lerato Molefe", tab "Feedback on Lerato Molefe · Wombat", "Default
  MSF · Last day to respond: 2026-11-03", the line asking for multi-source feedback; "Who sees your feedback" before
  the rating ("Your name and email address are never shown to Lerato Molefe. … grouped by respondent role."); five
  required points Well below expectations to Well above expectations; the comment "(optional)", "Up to 4,000
  characters. Your comments may be shown to Lerato Molefe word for word, …". Mokwena Above, Tau Well above, Motsepe
  Meets: each the h1 "Thank you" and the success alert "Your feedback on Lerato Molefe has been recorded." No CSP
  violation in the console. The captures show the questionnaire and thank-you pages, no registration link and no
  token.
Gap: none (the missing <h1> no longer occurs: one h1 in each state)

### Step 3.40 — A used link is refused
Role: Anonymous — Dr Kagiso Motsepe (MSF respondent)
Route: /msf/respond
Do: Open the same link again.
Expect: The card's h1 reads "Feedback link already used", with an info Alert saying the link has already been used
  and each link takes one response. No questionnaire is shown.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): HTTP 410, one h1 "Feedback link already used", an info alert
  "This feedback link has already been used: a response was submitted through it, and each link takes one response.";
  no questionnaire (no radio).
Gap: none

### Step 3.41 — Mr Smit reads the report while the campaign is open
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/reports/{CampaignId:int}
Do: Open Dr Molefe's report from the campaign list.
Expect: "MSF report for Lerato Molefe", subtitle "Default MSF · Multi-source feedback · `D` to `D+14`" with "Open its
  campaign page". The status card reads Open, "Still taking responses, until `D+14`.", and "The responses are read once
  the campaign has closed and every address has been removed." (T369, F1). Its checklist reads "3 responses, of the 5
  needed.", "0 respondent groups reporting, of the 2 needed." (no group has 3 yet) and "A respondent group reports once
  3 of it have responded; a group with fewer is suppressed." "Evidence for" lists PAED-010 and PAED-012, and says one
  record per EPA is written when the report is released. No group card and no suppressed line are shown, whatever has
  come back. Close campaign… is offered, with its help; Release is not.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): From the list (whose "Campaigns (2)" stands 17 px above its
  table) View report: /msf/reports/1, "MSF report for Lerato Molefe", "Default MSF · Multi-source feedback ·
  2026-10-20 to 2026-11-03" with "Open its campaign page". Status Open, "Still taking responses, until 2026-11-03.",
  "The responses are read once the campaign has closed and every address has been removed."; "3 responses, of the 5
  needed.", "0 respondent groups reporting, of the 2 needed.", "A respondent group reports once 3 of it have
  responded; a group with fewer is suppressed." Evidence for PAED-010 and PAED-012, "One evidence record per EPA is
  written to the trainee's portfolio when the report is released." No group card, no suppressed line. Close campaign…
  with "Release is offered once the campaign has closed. It closes by itself once 2026-11-03 has passed; close it
  sooner here."; no Release button. No CSP violation.
Gap: none

### Step 3.42 — Five more respondents answer, and one never does
Role: Anonymous — Dr Fatima Khumalo, Dr Sarah Botha, Ms Naledi Sebego, Sister Thandi Nkosi and Dr Johan van Wyk (MSF respondents)
Route: /msf/respond
Do: The five answer, each from their own link as in Step 3.39; Dr Khoza does not open hers.
  - Dr Khumalo: Above expectations, "Sound judgement in clinic; teaches the interns well".
  - Dr Botha: Above expectations, "Ready to run a ward; should document escalation decisions more fully".
  - Ms Sebego: Above expectations, "Refers early and listens to the therapists".
  - Sister Nkosi: Above expectations, "Escalates early and keeps the nursing notes in step".
  - Dr van Wyk: Above expectations, "Thorough on ward rounds; presents concisely".
Expect: Each submit ends on the h1 "Thank you" and "Your feedback on Lerato Molefe has been recorded." Eight of the
  nine links have now been used; Step 3.43 reads the counts.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signed out; Khumalo, Botha, Sebego, Nkosi and van Wyk each
  answered Above expectations with their comments, each submit the h1 "Thank you" and "Your feedback on Lerato Molefe
  has been recorded."; no CSP violation. Khoza's link was not opened.
Gap: none

### Step 3.43 — Mr Smit resends the undelivered link
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: At least an hour after the open, open the campaign and resend.
Expect: The status card reads "8 of 9 invitees have responded." The counts read Invited and Responded: Peer doctor 2
  and 1, Consultant 3 and 3, Nurse 3 and 3, Allied health professional 1 and 1, and All groups 9 and 8. In the Links
  part, a warning reads "1 link was not delivered. Resend sends each of these respondents a new link; this page never
  says who they are.", with the "Resend 1 link" button inside it, and "A link nothing was heard of for an hour counts as
  not delivered." under it. After the resend, the Links part reads "1 new link is being sent.", focused, and "1 link is
  still being sent. Reload this page to see whether it was delivered.", and the warning is gone. The log holds one more
  "Feedback request: Lerato Molefe …" stub email. It names no address; it is Dr Khoza's, the only link not answered.
Note: With `Email__SmtpHost` unset, nothing reports a delivery, so an hour after the open every unanswered link reads as
  not delivered (INFRASTRUCTURE.md § After T251). Where the mail reaches a server that accepts it (a local SMTP sink),
  every link is reported sent and none ever reads as not delivered, so stand in for a dropped mail on Dr Khoza's link;
  the warning then shows at once: `UPDATE "MsfInvitations" SET "SentOn" = NULL, "DeliveryFailedOn" = now(),
  "DeliveryLinkSelector" = "TokenSelector" WHERE "CampaignId" = <the campaign's id> AND "RespondentEmail" =
  'lindiwe.khoza@kgk.wombat.local' AND "RespondedOn" IS NULL;` (a failure counts only against the current link,
  `MsfInvitation.LinkNotDelivered`; T369's states pass).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): At 11:51 UTC (13:51 SAST), an hour after the open at 10:51 (no
  stand-in needed: the log sender reports no delivery; Steps 3.50 to 3.54c were played in the wait, kept in short
  background waits of under ten minutes, with read-only looks at the Links part, which read "1 link is
  still being sent." until the hour passed): "8 of 9 invitees have responded."; Invited / Responded Peer doctor 2 / 1,
  Consultant 3 / 3, Nurse 3 / 3, Allied health professional 1 / 1, All groups 9 / 8. The warning "1 link was not
  delivered. Resend sends each of these respondents a new link; this page never says who they are." with "Resend 1
  link" inside it, and "A link nothing was heard of for an hour counts as not delivered." under it. Resend: the
  focused "1 new link is being sent." and "1 link is still being sent. Reload this page to see whether it was
  delivered."; the warning gone. One more "Feedback request: Lerato Molefe (Default MSF, …)" stub email (35 → 36),
  naming no address: Khoza's.
Gap: none

### Step 3.44 — Mr Smit closes the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns/{CampaignId:int}
Do: On the report, press Close campaign… and confirm. Then open its campaign page.
Expect:
  - **The dialog** (A5): "Close this campaign now?", "Close the campaign for Lerato Molefe (Default MSF, closing
    `D+14`) before its last day?", then "It takes no more responses: the 1 link not yet used stops working.", "Every
    respondent's email address is removed, so no link can be resent.", "The report keeps the 8 responses it has, for
    you to review and release." and "Closing cannot be undone."; its buttons Keep it open and Close campaign.
  - **Closed:** "Campaign closed and anonymised for review. Every respondent's email address has been removed.",
    focused. The status card reads Under review, "Ready to release: every threshold is met.", with "8 responses, of
    the 5 needed." and "2 respondent groups reporting, of the 2 needed." met.
  - **By respondent group**, "As Lerato Molefe will read it once released." and how ratings are averaged, then the
    groups that reported, in the groups' order: "Consultant · 4.00 average rating, from 3 responses" above "Nurse · 4.33
    average rating, from 3 responses". Each card gives the comment question once, "What should the trainee keep doing
    or improve?", and its three comments.
  - **The suppressed line:** "Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so
    their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered."
    Named, never counted.
  - **Release to Lerato Molefe**, after the groups: what a release does (she reads the narrative and the groups shown;
    "2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012."; she is sent an email
    with a link to MSF reports; it cannot be undone), then the Narrative with its counter, "0 of 4,000 characters", and
    the optional "Supervision level this feedback supports", which offers Not stated and the six rungs (D10). Release
    to trainee… is offered.
  - **The campaign page:** Under review, "Closed to responses, and every respondent's email address has been
    removed.", the counts by group with their Responded column (9 and 8), and About's "Closed to responses: on `D`". It
    offers "Review and release" and Withdraw campaign….
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Close campaign… opened "Close this campaign now?", "Close the
  campaign for Lerato Molefe (Default MSF, closing 2026-11-03) before its last day? It takes no more responses: the 1
  link not yet used stops working. Every respondent's email address is removed, so no link can be resent. The report
  keeps the 8 responses it has, for you to review and release. Closing cannot be undone.", Keep it open (focused) and
  Close campaign; Escape returned the focus to Close campaign…. Confirmed: the focused (role status) "Campaign closed
  and anonymised for review. Every respondent's email address has been removed."; Under review, "Ready to release:
  every threshold is met.", "8 responses, of the 5 needed.", "2 respondent groups reporting, of the 2 needed."; By
  respondent group "As Lerato Molefe will read it once released." and how ratings are averaged, "Consultant · 4.00
  average rating, from 3 responses" above "Nurse · 4.33 average rating, from 3 responses", each with the comment
  question once and its three comments; "Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses
  each, so their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they
  answered." Release to Lerato Molefe: what a release does ("2 evidence records are written to Lerato Molefe's
  portfolio, for PAED-010 and PAED-012.", the email, "It cannot be undone."), Narrative "0 of 4,000 characters",
  Supervision level Not stated, 1, 2, 3a, 3b, 4, 5; Release to trainee… offered. The campaign page: Under review,
  "Closed to responses, and every respondent's email address has been removed.", counts 9 / 8, About "Closed to
  responses on 2026-10-20"; Review and release and Withdraw campaign…. No CSP violation.
Gap: none

### Step 3.45 — A link opened after the close is refused
Role: Anonymous — Dr Lindiwe Khoza (MSF respondent)
Route: /msf/respond
Do: Open the resent link.
Expect: The card's h1 reads "Feedback request closed", with a warning Alert saying the request has closed and is no
  longer accepting responses. No questionnaire is shown.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Signed out, the resent link (the last invitation stub): HTTP
  410, one h1 "Feedback request closed", a warning alert "This feedback request has closed and is no longer accepting
  responses."; no questionnaire.
Gap: none

### Step 3.46 — Mr Smit releases the report to Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns
Do: Write the narrative "Consistently strong feedback from nursing and consultant colleagues; keep documenting
  escalation decisions". Leave the supervision level Not stated, since he is not a clinician. Press Release to
  trainee… and confirm.
Expect:
  - **The counter** reads "106 of 4,000 characters" once the narrative is typed.
  - **The dialog** (A5): "Release this report to Lerato Molefe?": she can read it on MSF reports, the narrative and the
    ratings and comments of the Consultant and Nurse groups, nothing of the other groups; "2 evidence records are
    written to Lerato Molefe's portfolio: PAED-010 and PAED-012."; "Supervision level: Not stated."; "Lerato Molefe is
    sent an email saying a report has been released, with a link to MSF reports. The email holds none of the
    feedback."; and that releasing cannot be undone. Its buttons are Not yet and Release to Lerato Molefe.
  - **Released:** "Report released to Lerato Molefe.", focused. The status card reads Released, "The report has been
    released to the trainee.", and one line: "`D` hh:mm SAST · 8 responses, with 5 needed · 2 respondent groups
    reporting, with 2 needed" (T325). "Evidence for" lists PAED-010 and PAED-012, "Recorded on the portfolio: `D` hh:mm
    SAST". The groups read "As Lerato Molefe reads it.", the Consultant card first, and the suppressed line without its
    last sentence. "As released" shows the narrative and Supervision level "Not stated" as stored, with no form (T246).
  - **The mail:** the log holds one more stub email, "MSF report released: Default MSF": "Your multi-source feedback
    report, Default MSF, was released to you on `D` by Pieter Smit.", that it holds none of the feedback, and the link
    to `/msf/my-reports` (T369, Q8). The stub line prints no recipient (T282); it is Dr Molefe's.
  - **The list:** the row reads Released, Invitations 9, Responses 8, with "View campaign" and "View report" and no
    Withdraw.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Counter "106 of 4,000 characters"; level left Not stated.
  Release to trainee… opened "Release this report to Lerato Molefe?" (she reads the narrative and the Consultant and
  Nurse groups, nothing of the others; "2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and
  PAED-012."; "Supervision level: Not stated."; the email "holds none of the feedback"; "Releasing cannot be undone:
  …"), Not yet (focused) and Release to Lerato Molefe; Escape returned the focus to Release to trainee…. Confirmed:
  the focused "Report released to Lerato Molefe."; Released, "The report has been released to the trainee.",
  "2026-10-20 13:52 SAST · 8 responses, with 5 needed · 2 respondent groups reporting, with 2 needed"; Evidence for
  PAED-010 and PAED-012, "Recorded on the portfolio: 2026-10-20 13:52 SAST"; "As Lerato Molefe reads it.", Consultant
  first, the suppressed line without its last sentence; "As released": the narrative and Supervision level "Not
  stated" as text, no form control. One stub email (tags msf, released; 36 → 37) "MSF report released: Default MSF":
  "Your multi-source feedback report, Default MSF, was released to you on 2026-10-20 by Pieter Smit.", "Read it on MSF
  reports. This email holds none of the feedback.", http://localhost:5180/msf/my-reports (no recipient printed, T282).
  List row: Released, 9, 8, View campaign and View report, no Withdraw.
Gap: none

### Step 3.47 — Dr Molefe reads her released report
Role: Trainee — Dr Lerato Molefe
Route: /msf/my-reports → /msf/my-reports/{CampaignId:int}
Do: Open MSF reports, then view the report.
Expect:
  - **MSF reports:** "Released aggregate feedback only. Individual responses are never shown."; "Your released
    reports", a table captioned "Your released MSF reports, newest first", with the columns Report and Released and no
    Responses (T369, F3). One row: Default MSF over Multi-source feedback, Released "`D` hh:mm SAST", the same moment the
    coordinator's report gives (T325), and View, named "View the Default MSF report, released `D`".
  - **Her report** is a page of its own at `/msf/my-reports/{CampaignId:int}`: h1 and tab "Default MSF, released `D`",
    subtitle "Multi-source feedback · Released `D` hh:mm SAST", under the trail Home › MSF reports › Default MSF,
    released `D`, with MSF reports lit.
  - **Its sections:** Narrative, the coordinator's words. "What your colleagues said": "Grouped by respondent role; no
    one is named.", how ratings are averaged, and that comments are word for word; then two groups only, "Consultant ·
    4.00 average rating" first and "Nurse · 4.33 average rating", each with the comment question once and its three
    comments, and no stray ".:" (T270). "On your portfolio": "Recorded as evidence on `D` hh:mm SAST:", with PAED-010
    and PAED-012, each a link to its page under My progress.
  - **Nothing of the others:** nothing of the peer doctors or the physiotherapist, not even that they answered, and no
    group's count (T249).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): MSF reports: "Released aggregate feedback only. Individual
  responses are never shown."; "Your released reports" (17 px above its table at 1280 and at 390, D1), caption "Your
  released MSF reports, newest first", columns Report and Released (and a hidden Actions header), no Responses; one
  row Default MSF / Multi-source feedback, "2026-10-20 13:52 SAST" (the coordinator's moment), View named "View the
  Default MSF report, released 2026-10-20". /msf/my-reports/1: h1 and tab "Default MSF, released 2026-10-20",
  "Multi-source feedback · Released 2026-10-20 13:52 SAST", trail Home › MSF reports › Default MSF, released
  2026-10-20, MSF reports lit. Narrative; "What your colleagues said": "Grouped by respondent role; no one is named.",
  the averaging, "Comments are word for word."; "Consultant · 4.00 average rating" then "Nurse · 4.33 average rating",
  each the question once and three comments, no ".:"; "On your portfolio": "Recorded as evidence on 2026-10-20 13:52
  SAST:" PAED-010 and PAED-012, links to /portfolio/progress/11 and 13. Nothing of Peer doctor or Allied health, no
  group count. Nothing scrolls sideways at 390 on the list or the report.
Gap: none (T270 and T325 no longer occur: no stray ".:", and one SAST moment on both sides)

### Step 3.48 — Dr Molefe's record and progress show her evidence and the feedback
Role: Trainee — Dr Lerato Molefe
Route: /activities/mine → /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Open My activities, then My progress, then PAED-001, PAED-010 and PAED-012 from its index.
Expect: My activities holds her six Completed WBAs and two Multi-Source Feedback (Paediatrics) rows. The MSF rows are
  PAED-010 and PAED-012, encounter date `D` (the day the campaign closed), Done, Recorded, credit "—" as text (D8). Each
  WBA's Credit, "1 item", is a link to its EPA's page (T355). My progress reads:
  - "1 of 10" over "EPAs met this semester", and training year 4;
  - "Multi-source feedback: 2 of 15 EPAs covered by a released campaign that closed this semester. MSF is tracked on its
    own and counts towards no target.";
  - PAED-001's row: "3 of 3 this semester", "Target met for Semester 2, 2026.";
  - PAED-012's row "2 of 3 this semester" and PAED-010's "1 of 3 this semester", no row carrying an MSF line.
  PAED-001's page reads "At the minimum level when observed: 2 of 3" and "Training year 4: level 5, the minimum each
  encounter is judged against and your STAR's target."; its trajectory reads "3 ratings in the 2026 academic year, from
  Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below." The pages of PAED-010 and PAED-012 each read
  "MSF in Semester 2, 2026: covered by a released campaign that closed on `D`." first among their MSF lines, chart their
  ratings, and say under the chart "Multi-source feedback is not plotted.": no chart plots an MSF point (D36).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): My activities: six Completed WBAs, each Credit "1 item" a link
  named "1 item to PAED-0nn, in My progress", and two Multi-Source Feedback (Paediatrics) rows, PAED-012 and PAED-010,
  2026-10-20, Done, Recorded, "—" as text. My progress: "Training year 4 · Semester 2, 2026", "1 of 10" EPAs met this
  semester, "Multi-source feedback: 2 of 15 EPAs covered by a released campaign that closed this semester. MSF is
  tracked on its own and counts towards no target."; PAED-001 "3 of 3 this semester", "Target met for Semester 2,
  2026."; PAED-012 "2 of 3" and PAED-010 "1 of 3"; no row carries an MSF line; no chart. PAED-001: "At the minimum
  level when observed: 2 of 3", "Training year 4: level 5, …"; "3 ratings in the 2026 academic year, from Thandi Zulu,
  David Naidoo and Mohammed Patel. 2 at the minimum, 1 below." PAED-010 and PAED-012 each "MSF in Semester 2, 2026:
  covered by a released campaign that closed on 2026-10-20." first among their MSF lines, chart their 1 and 2 ratings,
  "Multi-source feedback is not plotted." under the chart; Activities list the MSF record, Recorded, "—".
Gap: none

### Step 3.49 — Mr Smit reads the programme's MSF coverage
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/coverage
Do: Open MSF coverage from the campaign list.
Expect: "MSF coverage", under the trail Home › MSF campaigns › MSF coverage, with no Back to campaigns (T369); its
  subtitle ends "Coverage is not a target." One programme: "Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching
  Hospital".
  - Under it, a line for each semester: "Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026"
    and "Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026".
  - By EPA, in Semester 2, 2026: PAED-010 and PAED-012 read "1 of 5 trainees covered", in bold. Every other cell reads
    "0 of 5 trainees covered".
  - By trainee: Lerato Molefe reads "2 of 15 EPAs covered" in Semester 2, 2026; everyone else reads 0 of 15.
  There is no badge, tint or bar, because coverage is not a target (D8, D9). The withdrawn campaign counts for nothing.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): From the list's MSF coverage: h1 and tab "MSF coverage", trail
  Home › MSF campaigns › MSF coverage, no Back to campaigns; subtitle ends "Coverage is not a target." One programme
  "Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital"; "Semester 1, 2026: 5 trainees, whose programme
  had started by 30 June 2026", "Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026". By
  EPA, Semester 2, 2026: PAED-010 and PAED-012 "1 of 5 trainees covered" in bold (td.is-covered, font-weight 700, no
  background), every other cell "0 of 5 trainees covered". By trainee: Lerato Molefe "2 of 15 EPAs covered" (bold) in
  Semester 2; everyone else 0 of 15. No badge, tint or bar; nothing from the withdrawn campaign. Nothing scrolls
  sideways at 390.
Gap: none

## Phase 3.H — Dashboards with data

### Step 3.50 — Dr Dlamini's dashboard
Role: Trainee — Dr Anele Dlamini
Route: /
Do: Open the dashboard and read each card.
Expect:
  - Your targets: "Training year 3 — it sets the minimum level each encounter is judged against.", then "1 of 10" over
    "EPAs met this semester" and "0 of 5" over "EPAs met in 2026". Under Furthest short, largest shortfall first, are
    PAED-002, PAED-003, PAED-005, PAED-010 and PAED-012, each its own link with "0 of 3 this semester · 3 more by
    2026-11-30" under it; then Open My progress.
  - Needs you: "Nothing needs you. Requests you have filed are in My activities.", with Open My activities (T342).
  - Recent decisions: her four WBAs, newest decision first, each Completed in green on `D`: PAED-004's, to Fatima
    Khumalo, reading "PAED-004: 1 of 3 this semester.", then the three PAED-001 rows, each "PAED-001: 3 of 3 this
    semester, met.". Each row is its own link, and the card wraps none of them (T280).
  - My authorisations: "No STAR yet. When the committee issues one, it shows here against training year 3's level.",
    with Open My authorisations. The header offers "Log an activity" (T335: the Actions card is gone), and there is no
    Upcoming deadlines card (T355).
Note: The state capture taken here, before the MSF release (`states.md`, `my-msf-reports--foreign`): Dr Dlamini types
  `/msf/my-reports/<Dr Molefe's campaign id>` and lands on her own list, `/msf/my-reports?unavailable=true`, with one
  info Alert, focused, "That report could not be found. Your released reports are listed below.", above "No released
  reports" (T369, M5, M11; T270). An id that is hers but not released, or no campaign at all, lands the same way.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in Step 3.43's hour's wait (after 3.42). Your targets:
  "Training year 3 — it sets the minimum level …", "1 of 10" EPAs met this semester, "0 of 5" EPAs met in 2026;
  Furthest short PAED-002, 003, 005, 010, 012, each its own link with "0 of 3 this semester · 3 more by 2026-11-30",
  then Open My progress. Needs you "Nothing needs you. Requests you have filed are in My activities." with Open My
  activities. Recent decisions, newest first, each its own link, Completed on 2026-10-20: "Mini-CEX (Paediatrics) ·
  PAED-004 · 2026-10-18", to Fatima Khumalo, "PAED-004: 1 of 3 this semester."; then the CCA, CBD and Mini-CEX on
  PAED-001, each "PAED-001: 3 of 3 this semester, met."; no nested link, no card a link. My authorisations "No STAR
  yet. When the committee issues one, it shows here against training year 3's level." with Open My authorisations;
  header Log an activity; no Upcoming deadlines. State capture (before the release): /msf/my-reports/1 landed on
  /msf/my-reports?unavailable=true with one info alert, focused, "That report could not be found. Your released
  reports are listed below.", above "No released reports".
Gap: none (F-3.50a, T270, no longer occurs: one focused alert above her own empty list)

### Step 3.51 — Dr Khumalo's dashboard and menu
Role: Assessor — Dr Fatima Khumalo
Route: / → /activities/inbox
Do: Read the dashboard, then follow "Open Activity inbox". Then read the menu.
Expect:
  - Waiting for you, badged "1 waiting": "Case-Based Discussion (Paediatrics) · PAED-002 · `D−5`", from Pieter du
    Plessis, Requested, "Waiting less than a day" (T297, T335, T350). The inbox's Waiting for you lists it alone, as
    Requested, "Less than a day" over "since … SAST".
  - Recent decisions, the activities she moved last, newest first, each linked by its full name with its registrar,
    its decision's badge and its day: "Mini-CEX (Paediatrics) · PAED-004 · `D−2`", from Anele Dlamini, Completed, in
    green; "Case-Based Discussion (Paediatrics) · PAED-012 · `D−5`", from Lerato Molefe, Completed; "Mini-CEX
    (Paediatrics) · PAED-002 · `D−20`", from Sipho Ndlovu, Declined, in red; its foot "All your decisions". The inbox's
    Decided by you, "3 decisions", lists the same three, each with its moment (SAST) and its credit.
  - The inbox lights Activity inbox in the menu: Home and Activity inbox, then My data rights. There is no Recent
    activities: the flow 01 pick dropped it, with its placeholder.
Note: "Waiting for you" is read as in Step 3.33, from her inbox: no badge beside an inbox holding the CBD is a Gap.
  Until T297 it was "Accepted, needing action"; until T335 it was split across Pending requests and Awaiting your
  review; until T350 it was "Waiting for your rating".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in Step 3.43's wait. Waiting for you "1 waiting":
  "Case-Based Discussion (Paediatrics) · PAED-002 · 2026-10-15", from Pieter du Plessis, Requested, "Waiting less than
  a day". Recent decisions, newest first: "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-18", from Anele Dlamini,
  Completed; "Case-Based Discussion (Paediatrics) · PAED-012 · 2026-10-15", from Lerato Molefe, Completed; "Mini-CEX
  (Paediatrics) · PAED-002 · 2026-09-30", from Sipho Ndlovu, Declined; foot "All your decisions". Open Activity inbox:
  the CBD alone, Requested, "Less than a day" over "since 2026-10-20 12:35 SAST"; Decided by you "3 decisions", the
  same three with their SAST moments and credit ("1 item", "1 item", "—"). Menu Home, Activity inbox (lit), My data
  rights; no Recent activities.
Gap: none

### Step 3.52 — Dr Zulu's committee dashboard
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → /
Do: Choose Switch to Committee member in the sidebar and read the dashboard and the menu.
Expect:
  - Home opens acting as Assessor, Dr Zulu's choice from Step 3.33, kept with the account. After the switch an info
    alert reads "You are now acting as Committee member.", the sidebar "Acting as Committee member", and Home's subtitle
    "Committee member · Semester N, YYYY".
  - "Registrars", badged "5 registrars", fewest met first, then by surname: Pieter du Plessis, Nomsa Mahlangu and Sipho
    Ndlovu, each "0 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in YYYY"; then Anele Dlamini and
    Lerato Molefe, each "1 of 10" and "0 of 5" (T298 at each tie).
  - "Targets by EPA", fewest registrars met first, then by code: PAED-002 to PAED-015, each "0 of 5", then PAED-001 ("3
    per semester") last, "2 of 5" over "registrars met this semester".
  - Each registrar's name is a link to the registrar's page, and each EPA's name a link to Programme trainees filtered
    Short on it, its figure in words hidden after it. No card is one link around its rows (T280).
  - The menu is Home, Programme trainees, Committee reviews and Decision panels, then My data rights.
Note: The cards read the current registrars at Dr Zulu's institution, as Committee member (`ProgrammeScope`, E4; T290),
  not the member's sub-speciality scopes: Dr Zulu's invitation carried none, and Dr Zulu's Paediatrics sub-speciality
  came with Assessor at Step 2.13 (since T376 Add a role writes the role's scope; until then the assessor profile did).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in Step 3.43's wait. Home opened "Assessor · Semester
  2, 2026" (3.33's choice kept). After the switch: "You are now acting as Committee member.", sidebar "Acting as
  Committee member", "Committee member · Semester 2, 2026". "Registrars" "5 registrars": Pieter du Plessis, Nomsa
  Mahlangu, Sipho Ndlovu, each "0 of 10" / "0 of 5", then Anele Dlamini and Lerato Molefe, each "1 of 10" / "0 of 5".
  "Targets by EPA": PAED-002 to PAED-015 each "0 of 5", then PAED-001 ("3 per semester") last, "2 of 5" registrars met
  this semester. Names link to /programme/trainees/{ProfileId}, EPAs to /programme/trainees?short=<EpaId> with the
  figure in words after; no card wraps a link. Menu Home, Programme trainees, Committee reviews, Decision panels, then
  My data rights.
Gap: none

### Step 3.53 — Dr Mokoena's dashboard
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the dashboard and the menu.
Expect:
  - "Waiting for assessors", in the warning stripe, badged "2 waiting, 1 overdue": Dr du Plessis's portfolio review
    (Awaiting review, Overdue, "With Mohammed Patel", "Waiting 8 days"), then the CBD (Requested, "With Fatima Khumalo",
    "Waiting less than a day"), each a link to its activity, with Open Waiting for assessors (T358, Q3).
  - "Registrars" and "Targets by EPA" read as Step 3.52's, for Paediatrics: the same five, badged "5 registrars", and
    PAED-001 "2 of 5" last.
  - No Pending reviews, no Trainees in programme and no "inactive" (Q10).
  - The menu is Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and
    Entrustment decisions, then My data rights. The STAR review queue is not restored: Entrustment decisions takes its
    place.
Note: The card is Waiting for assessors' first five, the page's own read (T358, E4): each activity's pinned workflow
  (`ActivityWaiting`, T297) says it awaits a reviewer, and a `field:` nominee holds it (E3): a request waits in
  `requested`, a portfolio review in `submitted`. Until T358 it was Pending reviews, a count that linked nowhere.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in Step 3.43's wait. "Speciality admin · Semester 2,
  2026". "Waiting for assessors" (detail-card--warning) "2 waiting, 1 overdue": the portfolio review (Awaiting review,
  Overdue, "With Mohammed Patel", "Waiting 8 days"), then the CBD (Requested, "With Fatima Khumalo", "Waiting less
  than a day"), links /activities/10 and 9, Open Waiting for assessors. Registrars "5 registrars" and Targets by EPA
  as Step 3.52's, PAED-001 "2 of 5" last. No Pending reviews, no Trainees in programme, no "inactive". Menu Home,
  Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels, Entrustment decisions,
  then My data rights.
Gap: none

### Step 3.54 — Dr Sithole's dashboard
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /
Do: Read the dashboard.
Expect: The same three cards and figures as Dr Mokoena's, read for Paediatrics: "2 waiting, 1 overdue", Registrars
  badged "5 registrars", and PAED-001 "2 of 5" last on Targets by EPA. Dr Sithole's menu reads as Dr Mokoena's, under
  "Acting as Sub-speciality admin".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in Step 3.43's wait. "Acting as Sub-speciality admin",
  "Sub-speciality admin · Semester 2, 2026": Waiting for assessors "2 waiting, 1 overdue" with the same two rows,
  Registrars "5 registrars" in the same order, PAED-001 "2 of 5" last; the menu reads as Dr Mokoena's.
Gap: none

### Step 3.54a — Dr Zulu reads who is short on PAED-002
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /programme/trainees
Do: On Home, choose PAED-002 on Targets by EPA. Then choose Clear filters.
Expect: Programme trainees opens with Short on set to PAED-002 and Programme trainees lit (T358, Q1). Its subtitle reads
  "Current registrars at Kgosi Kgari Teaching Hospital, read as Committee member · Semester N, YYYY". The list's
  heading, with the focus, reads "5 of 5 current registrars are short on PAED-002", its rule line "PAED-002 — <its
  title>, 3 per semester. Furthest from its target first, then fewest EPAs met, then by surname.", and its columns are
  Registrar, Training year, PAED-002, This semester, In YYYY and Last filed. Pieter du Plessis, Anele Dlamini and Lerato
  Molefe read "0 of 3 this semester" over "3 more by <the semester's last day, ISO>", then Nomsa Mahlangu and Sipho
  Ndlovu "1 of 3 this semester" over "2 more by …". Each registrar's name is a link to the registrar's page.
  Clear filters shows the whole list, headed "5 current registrars", under "Fewest met first, then by surname. Semester
  N, YYYY ends on <its last day>.", its columns Registrar, Training year, This semester, In YYYY, Furthest short and
  Last filed. Furthest short reads "PAED-002, PAED-003, PAED-005" for Anele Dlamini and "PAED-001, PAED-003, PAED-005"
  for Nomsa Mahlangu, each over "0 of 3 each this semester" (D11). Nothing on the page reads "n / m", a percentage or
  the bare word "year" (Q10).
Note: The EPA's link carries the filter in the address (`?short=<EpaId>`, D3), so Back and a shared link show the same
  list. Played after Step 3.54, on its figures.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Played in the wait, after 3.54. PAED-002 on Targets by EPA
  opened /programme/trainees?short=3, Short on PAED-002, Programme trainees lit. Subtitle "Current registrars at Kgosi
  Kgari Teaching Hospital, read as Committee member · Semester 2, 2026". The focused heading "5 of 5 current
  registrars are short on PAED-002", "PAED-002 — Managing common paediatric presentations, 3 per semester. Furthest
  from its target first, then fewest EPAs met, then by surname."; columns Registrar, Training year, PAED-002, This
  semester, In 2026, Last filed; du Plessis, Dlamini, Molefe "0 of 3 this semester" over "3 more by 2026-11-30", then
  Mahlangu and Ndlovu "1 of 3" over "2 more by 2026-11-30"; names link to /programme/trainees/{ProfileId}. Clear
  filters: the focused "5 current registrars", "Fewest met first, then by surname. Semester 2, 2026 ends on
  2026-11-30.", columns Registrar, Training year, This semester, In 2026, Furthest short, Last filed; Dlamini
  "PAED-002, PAED-003, PAED-005", Mahlangu "PAED-001, PAED-003, PAED-005", each "0 of 3 each this semester". No "n /
  m", no percentage; "year" only in Training year.
Gap: none

### Step 3.54b — Dr Zulu opens Dr Mahlangu's page
Role: CommitteeMember — Dr Thandi Zulu
Route: /programme/trainees → /programme/trainees/{ProfileId:int}
Do: On Programme trainees, choose Nomsa Mahlangu, and read the page to its foot.
Expect: The registrar page's h1 reads "Nomsa Mahlangu", its tab "Nomsa Mahlangu · Wombat", its subtitle "Training year 1
  · Semester N, YYYY · Kgosi Kgari Teaching Hospital, Paediatrics"; the trail reads Home › Programme trainees › Nomsa
  Mahlangu, and Programme trainees is lit (T358, C2, C9). Its sections, in order, each headed:
  - This period: "0 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in YYYY", then "Semester N, YYYY
    ends on <its last day>. Training year 1 sets the minimum level each encounter is judged against.";
  - EPAs: the index, in code order, each EPA's name as text, not a link (review 7), among them PAED-001 "0 of 3 this
    semester", and PAED-002 and PAED-004 "1 of 3 this semester";
  - Entrustment against Annexure A: the standing panel, PAED-004's latest rating "3a · Encounter `D−3`" and PAED-002's
    "3a · Encounter `D−8`"; each EPA charted on this page names a link to its chart below;
  - Rating trajectories: a chart for each EPA rated in the YYYY academic year (PAED-002's DOPS and PAED-004's Mini-CEX,
    both 3a), each headed by its own h3, with no Today rule (D8);
  - Waiting for assessors: "Nothing of Nomsa Mahlangu's waits for an assessor.";
  - Committee reviews: "No review is scheduled for Nomsa Mahlangu."
  Dr Zulu, a Committee member, is offered no Send a reminder. No word on the page names Nomsa Mahlangu by a pronoun
  (round-3-check 1).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): /programme/trainees/5: h1 "Nomsa Mahlangu" (focused), tab
  "Nomsa Mahlangu · Wombat", "Training year 1 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics"; trail
  Home › Programme trainees › Nomsa Mahlangu, Programme trainees lit. Sections: This period ("0 of 10", "0 of 5",
  "Semester 2, 2026 ends on 2026-11-30. Training year 1 sets the minimum level …"); EPAs in code order, names as text,
  PAED-001 "0 of 3", PAED-002 and 004 "1 of 3 this semester"; Entrustment against Annexure A, PAED-002's "3a ·
  Encounter 2026-10-12" and PAED-004's "3a · Encounter 2026-10-17", linking to #trajectory-3 and #trajectory-5; Rating
  trajectories, an h3 and chart each (DOPS 3a Patel, Mini-CEX 3a Zulu), no Today rule; "Nothing of Nomsa Mahlangu's
  waits for an assessor."; "No review is scheduled for Nomsa Mahlangu." No button (no Send a reminder); no pronoun.
Gap: none

### Step 3.54c — Dr Mokoena filters Waiting for assessors
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /programme/waiting
Do: Open Waiting for assessors from the menu. Choose With: Mohammed Patel and, before Show, look at the list; then Show.
  Then choose Waiting: Overdue only and With: Fatima Khumalo, and Show. Last, choose Clear filters.
Expect: The subtitle reads "Requests in Paediatrics whose next move names an assessor, supervisor or reviewer, read as
  Speciality admin. Your own requests are not listed." (E4), and the heading "2 waiting, 1 overdue": Step 3.53's two
  rows, each with Send a reminder. The With filter offers Anyone, Fatima Khumalo and Mohammed Patel, each waiting row's
  assessor by surname. Nothing on the page changes until Show (round 3 item 31). After Show the address carries the
  filter (`?with=<id>`, D3), Clear filters is offered, and the heading takes the focus: "1 waiting, 1 overdue, with
  Mohammed Patel", over the portfolio review alone. After the second Show the heading reads "0 of 2 waiting", and under
  it "No request matches these filters.", "Overdue only, with Fatima Khumalo." and Clear filters (review 33). Clear
  filters returns the whole list, "2 waiting, 1 overdue". No reminder is sent.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Subtitle "Requests in Paediatrics whose next move names an
  assessor, supervisor or reviewer, read as Speciality admin. Your own requests are not listed."; "2 waiting, 1
  overdue", the two rows each with Send a reminder; With offers Anyone, Fatima Khumalo, Mohammed Patel. Choosing
  Mohammed Patel changed nothing until Show; then ?with=<id>, Clear filters, the focused "1 waiting, 1 overdue, with
  Mohammed Patel" over the review alone. Overdue only with Fatima Khumalo: ?show=overdue&with=<id>, the focused "0 of
  2 waiting", "No request matches these filters.", "Overdue only, with Fatima Khumalo." and Clear filters. Clear
  filters: /programme/waiting, "2 waiting, 1 overdue". No reminder sent (one ActivityReminders row at the end).
Gap: none

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
  - "Msf" leaves Mr Smit's nineteen commands: the template, two campaigns, ten invitations, one removal, the open,
    the withdrawal, the resend, the close and the release.
  - No respondent's submission and no devadmin row is listed, because those carry no institution (T101, T205).
Actual (2026-10-20, T373 replay, wombat_scenario_t373): From (UTC) 2026-10-20T10:30, the sitting's start: 166 KGK rows
  (59 Login, 24 Logout, 23 CreateActivityCommand, 40 TransitionActivityCommand, 19 MSF, 1
  SendActivityReminderCommand), newest first, each with its actor and OK; none from a respondent or devadmin. Failures
  only: four FAILED (Khumalo's decline, Ndlovu's two creates, Dlamini's submit). "Msf": Mr Smit's 19 (template, two
  campaigns, ten invitations, one removal, open, withdrawal, resend, close, release). The actor column is the email on
  command rows and the person's name on Login rows.
Gap: known T286 (still: F-3.55a, the audit log names a command's actor by email but a Login's by display name)

### Step 3.56 — Prof Mbatha opens one entry
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Under Failures only, open Dr Ndlovu's future-date create.
Expect: The Event card shows the time to the millisecond, category Command, action CreateActivityCommand, and result
  Failed with "Date observed: The date cannot be after today (…)." The Actor card shows Dr Ndlovu's user id, display
  name and IP address. There is no Payload card: only an Administrator reads the raw JSON. "Back to log" returns to the
  list.
Actual (2026-10-20, T373 replay, wombat_scenario_t373): Event: "When (UTC) 2026-10-21 10:33:12.644", Command,
  CreateActivityCommand, Failed, "Date observed: The date cannot be after today (2026-10-20)." Actor: user id,
  "Display name ndlovu@kgk.wombat.local", IP "::/48". Only Event and Actor, no Payload. Back to log returns to
  /admin/audit with From and Result empty.
Gap: known T286 (still: F-3.56a, the Actor card's "Display name" is Dr Ndlovu's email); known T277 (still: F-3.56b,
  Back to log drops the From and Failures-only filters)

### Step 3.57 — devadmin reads the log across institutions
Role: Administrator — devadmin@wombat.local
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Open Audit log, with From (UTC) set to the start of this act's sitting as in Step 3.55. Open one respondent's
  submission (SubmitMsfResponseCommand), then one of Dr Naidoo's TransitionActivityCommand rows.
Expect: The log holds every institution's rows and the rows with no institution. Among them are eight
  SubmitMsfResponseCommand rows whose actor is "system", and devadmin's own RunScheduledJobNowCommand. The submission's
  detail names no user and no display name, only a truncated address. Its payload shows the token and the answers as
  "[REDACTED]" (T101, T205). Dr Naidoo's payload shows the principal as "[PRINCIPAL]", and the data patch and note as
  "[REDACTED]".
Actual (2026-10-20, T373 replay, wombat_scenario_t373): From 10:30 UTC: 179 rows (the KGK rows of Step 3.55 plus eight
  SubmitMsfResponseCommand by "system", one RunScheduledJobNowCommand by devadmin and devadmin's sign-ins). A
  submission's detail: User ID "—", Display name "—", IP "::/48"; payload {"token": "[REDACTED]", "answers":
  "[REDACTED]"}. Naidoo's complete of 13: principal "[PRINCIPAL]", note and dataPatchJson "[REDACTED]", activityId,
  actorUserId and transitionKey in clear.
Gap: none

## Act 3 outcome state

Replay check (2026-10-20, T373 replay, wombat_scenario_t373): match on all five queries (23 activities in the 12
  groups; the 8 progress rows; recorded 15, late 2; MSF Released 1 and Withdrawn 1, 8 responses, 9 invitations with 0
  addresses, 1 template; ActivityReminders 1; failures CreateActivityCommand 2 and TransitionActivityCommand 2 since
  10:30 UTC). Mail in the act was 14 stub emails (23 before, 37 after): 9 invitations, 1 resent link, the release
  mail, Mr Smit's reminder to Dr Zulu and the two nudge digests (Step 3.30's ageing ran after the 09:00 UTC scheduled
  nudge, which nudged 0), none about an activity's move. Steps 3.50 to 3.54c were played in Step 3.43's hour's wait.
  No correction by SQL was needed.

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
  - Dr Molefe's campaign Released, with 8 responses from 9 invitations whose addresses were removed at the close;
  - Dr du Plessis's draft Withdrawn.
- **The audit trail:** four failed commands in this act.
- **Mail:** the MSF invitations (nine, plus one resent), the release mail to Dr Molefe (Step 3.46), Mr Smit's one
  reminder to Dr Zulu (Step 3.31) and two nudge digests. Nothing about any activity's moves.
- **Reminders:** one row in `ActivityReminders`, the Mini-CEX's, sent by Mr Smit to Dr Zulu on `D` (T358).

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
SELECT count(*) FROM "MsfResponses";                                   -- 8
SELECT count(*), count("RespondentEmail") FROM "MsfInvitations";       -- 9, 0
SELECT count(*) FROM "MsfTemplates";                                   -- 1
SELECT count(*) FROM "ActivityReminders";                              -- 1 (Step 3.31)

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
  portfolio review still Awaiting review. He is the natural case for a sitting that stages nothing and issues no STAR.
- **Dr Mahlangu (year 1)** has PAED-002 and PAED-004 once each, at the minimum. The PAED-004 Mini-CEX stalled for eight
  days before it was rated.
- **Dr Ndlovu (year 1)** has PAED-002 once, a declined request that credited nothing, and a discussed reflection on
  PAED-001.
- Dr Patel and Dr Khumalo have not acted on the two open requests. Act 4 may leave them open or close them; nothing
  here depends on either.

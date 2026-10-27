# Appendix — cross-cutting

**Actuals before T361.** An Actual dated before the T361 replay describes the review page with deferral, which D53
removed; the Expects are the page as built.

**Scenario date.** None of its own. The appendix is played on the replay day `D`, after Act 6, and every check in it
reads the real clock (README § The clock). In the story it is early 2027: Dr Molefe has graduated, Dr du Plessis has
left the programme, and the other three registrars are still in it.

**Who acts.** devadmin (the Administrator); Prof Mbatha (InstitutionalAdmin); Mr Smit (Coordinator); Dr Mokoena
(SpecialityAdmin); Dr Sithole (SubSpecialityAdmin); Dr Kruger (CollegeAdmin); Dr Zulu and Dr Botha (CommitteeMember and
Assessor); Dr Patel and Dr Khumalo (Assessor); Dr Dlamini, Dr Mahlangu and Dr Ndlovu (Trainee); Dr Molefe and Dr du
Plessis (former trainees); and anonymous visitors.

**Why.** Some of Wombat belongs to no act: a person's rights over their own data, the jobs that run on their own, the
institutional sign-in, each person's own account, the pages a wrong address or a wrong role lands on, the platform's own
pages, and whether all of it works on a phone, from a keyboard and through a screen reader. This appendix plays them.

**Starting state.** Everything Act 6 left (`recovery/scenario-post-act6.dump`):
- KGK and its cast, each with the roles the README gives, and no review left open (Act 4, Act 5).
- Dr Molefe is a former trainee: no role, and a trainee record (Act 5).
- Dr du Plessis left the programme in Act 5: his profile ended on that act's `D−2`, and he holds no role (T290).
- Dr Dlamini, Dr Mahlangu and Dr Ndlovu are current trainees, Dr Ndlovu on curriculum 11.2 since Act 6, each with the
  activities of Act 3 and the reviews Act 4 ratified.
- Dr Molefe's MSF campaign was released in Act 3, and her portfolio PDF exported and verified in Act 5.
- No data-rights request, no SSO provider and no SSO group mapping exists yet. All eight scheduled jobs are enabled.
  Only the scheduler has run them, but for Act 3's one run of `assessor-pending-nudge` by hand (Step 3.32).

The replay host needs:
- `Wombat__PseudonymSalt` set, for A.1's erasure;
- `Email:SmtpHost` empty, so each mail's body is written to the application log. `appsettings.Development.json` names
  `localhost`, so start the app with `--Email:SmtpHost=` (an empty environment variable does not survive on Windows);
- `Sso__Providers` empty.

Note the time the appendix starts, as `<appendix start>`, for the outcome state's SQL.

The replay itself needs a second browser (or a private window) for the steps where a session ends elsewhere, a screen
reader (NVDA, or Windows Narrator), and a browser's device mode at 390 × 844 px.

**Goal.** Every page and endpoint listed here is played, including each refusal and each empty state a person meets on
the way. The appendix ends with:
- one account erased;
- four data-rights requests, each in its final state;
- two passwords changed, both recorded in `pwd_DO_NOT_COMMIT.txt`;
- the few other changes the outcome state lists, and everything else as the appendix found it.

**Order.** The erasure (A.1.12) is destructive, and nothing after it needs Dr Ndlovu. A.6 locks Dr Patel out and
reactivates him before A.7 needs him.

## A.1 — Data rights

### Step A.1.1 — Dr Dlamini reads her data rights and sets a preference
Role: Trainee — Dr Anele Dlamini
Route: / → /account/data-rights
Do: Open My data rights, under the rule at the foot of her menu. Read the page, tick "Opt out of optional processing",
  and save her preferences.
Expect: The page has three parts: processing preferences, a request form, and "Your requests", which is empty ("No
  requests"). The digest opt-out's help names the three reminders it stops (the weekly coordinator digest, the 14-day
  draft reminder and the 5-day assessment reminder), and says that email about one particular thing is still sent
  (T240). Saving says "Processing preferences saved." and the focus moves to that message (T234).
Note: Nothing in the product reads the optional-processing flag (reported).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): D = 2026-10-28 (the clock passed midnight SAST at the start of
  this act; the first save of the preference was made at 23:59 on 2026-10-27, so it was pressed again after midnight,
  an idempotent save). The menu reads "Acting as Trainee": Home, Log an activity, My activities, MSF reports, My
  committee reviews, Export portfolio, My progress, then My data rights. Opened, it is lit, tab "My data rights ·
  Wombat". Three parts: Processing preferences, Submit a request, and Your requests reading "No requests / You have
  not submitted any data rights requests yet." The digest help names the weekly coordinator digest, the 14-day draft
  reminder and the reminder of activities that have waited more than 5 days, and says email about one particular thing
  is still sent. Ticked the optional-processing opt-out and saved: "Processing preferences saved." took the focus.
Gap: none (the Note's unread flag is F-A.1.1a, T318, still confirmed only from code)

### Step A.1.2 — Dr Dlamini requests an export of her data
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights
Do: Choose the request type Export and submit it with no reason. Then submit it again with the reason "A copy of my
  training record for my own files."
Expect: The form offers four types: Access, Export, Rectification and Erasure. With no reason the request is refused
  ("Please provide a reason for your request.") and nothing is sent. The second submit says the request was submitted
  and that she will be notified when it is reviewed. "Your requests" lists it: today, Export, Submitted, with a
  Withdraw button named for it and no Download.
Note: The confirmation promises a notification that nothing sends (reported).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Four types offered (Access, Export, Rectification, Erasure,
  each with its gloss). Export with no reason: "Please provide a reason for your request.", no row written, the focus
  left on Submit request. With the reason: "Request submitted. You will be notified when it is reviewed." took the
  focus; Your requests lists "2026-10-27 22:01 | Export | Submitted" (made at 00:01 SAST on 2026-10-28) with "Withdraw
  the Export request made 2026-10-27 22:01" and no Download. No mail.
Gap: F-A.1.2a, T319 (still: the promised notification is never sent). F-A.1.2c, T325 (still: the time prints in UTC,
  22:01 at 00:01 SAST, and the day with it).

### Step A.1.3 — Mr Smit approves the export from KGK's queue
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Open Data rights requests from the menu. Narrow it to type Export and status Submitted, apply the filters, and
  review Dr Dlamini's request. Press Approve with no decision note. Then approve it with the note "Identity confirmed;
  export released to the data subject."
Expect: The queue holds only KGK's requests (T112), and the filters change it only when they are applied. On the
  request's page Data rights requests stays lit, and the trail reads Home › Data rights requests › the request's id
  once it has loaded (R2-Rules § 3; DESIGN.md's owner table).
  - The row names the requester by the address she signs in with, and its Review action is named for the row (T239).
  - The detail page shows the requester, her user id, when it was submitted, its type, its status and her reason.
  - Approving with no note is refused: "A decision note is required."
  - Once approved, the request reads Completed at once, because an export is built when it is downloaded. A Decision
    card appears (decided by, as a user id; decided on; the note; completed on), and Approve and Reject are gone.
Note: A Coordinator decides only their own institution's requests (T112). Which role should hold this power is still
  open (T112 § Still open).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Data rights requests from the menu, lit (nav-link active). The
  queue held only Dlamini's KGK request. Choosing Erasure changed nothing until Apply filters, then "No requests / No
  data rights requests match the current filters." Export + Submitted listed her row: her address, Export, Submitted
  and "Review the Export request from dlamini@kgk.wombat.local, 2026-10-27 22:01". On request 01a17f61-... the menu
  item stays lit and the trail reads Home > Data rights requests > its id. It shows requester, user id, submitted,
  type, status and reason. Approve with no note: "A decision note is required." With the note: "Request approved."
  took the focus; Status Completed; a Decision card (Decided by 43ad0cb7-..., Smit's id; decided on; the note;
  completed on 22:01:54 UTC); no Approve or Reject. No mail. The filters still offer "UnderReview" and "Objection". At
  1280 px the Request card (right edge 599 px) is still run past by its DDs (right edge 634 px: the address, the user
  id, the date, Export, Completed, the reason).
Gap: F-A.1.3b, T324 (still: raw "UnderReview" and the unsubmittable "Objection" in the filters). F-A.1.3c, T323
  (still: the Request card is run past by its DDs by 35 px).

### Step A.1.4 — Mr Smit cannot download what he approved
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights/download/{id:guid}
Do: Open the download address of the request he has just approved. Its id is in the detail page's address.
Expect: Page not found, with status 404. Nothing on the page says that the request exists. The reviewer approves; only
  the data subject, or a global Administrator, collects the bundle (T112).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): The download address of request 01a17f61-... answered 404 at
  the typed address, tab "Page not found · Wombat": "Page not found", "There is no page at this address.", "Check the
  address, or start again from Home." and Go to Home. Nothing on the page names the request or an export.
Gap: none

### Step A.1.5 — Dr Dlamini downloads her export
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights → /account/data-rights/download/{id:guid}
Do: Reload My data rights and download the completed request.
Expect: The row reads Completed and offers a Download named for the request.
  - The browser saves `data-export-<date>-<time>.zip`.
  - It holds `data-export.json`: schema version 2, with her profile, every activity she is the subject or author of,
    her committee reviews, the MSF campaigns about her, her progress by semester, her newest 500 audit entries and her
    portfolio exports.
  - It also holds `portfolio-summary.pdf`.
  - No email is sent.
Note: The audit section stops at 500 entries without saying so (T277).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): My data rights from the menu: "2026-10-27 22:01 | Export |
  Completed" with "Download the export of the Export request made 2026-10-27 22:01". Fetched with her session through
  Playwright's request context (README, Downloads): 200 application/zip, attachment data-export-20261027-220208.zip
  (138,513 bytes). It holds data-export.json (schemaVersion 2: profile, 5 activities, 1 committee review, 0 MSF
  campaigns, 3 progress rows, 50 audit entries, 0 portfolio exports) and portfolio-summary.pdf (143,066 bytes). No
  mail.
Gap: none

### Step A.1.6 — Dr du Plessis asks for access, then withdraws the request
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /account/data-rights
Do: Submit an Access request with the reason "Everything held about me." Then withdraw it from "Your requests".
Expect: He signs in to Home with no role: the menu reads Home, My progress, Export portfolio and My data rights, and
  Home is the former trainee's card as at Step 5.28, "Your programme ended on `D−2`." with the record's four rows, STARs
  plain text (T379, R3-H02). The request lists as Submitted, with a Withdraw named for its type and time. Once
  withdrawn, the page says "Request withdrawn.", the focus moves to that message, and the row reads Withdrawn and offers
  nothing.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): He signs in to Home with no role: the menu reads Home, My
  progress, Export portfolio and My data rights (no "Acting as"). Home is the former trainee's card "Your training
  record": "Your programme ended on 2026-10-25." (that was D−3 on this replay day, the runbook's D−2 being Act 5's
  day) and "Your record is kept, read-only: no target applies to you any more.", then four rows: My progress ("Every
  period of your programme, to training year 2, and your standing against Annexure A."), STARs as plain text ("No STAR
  was issued to you, so there is no certificate to download."), My activities and Export portfolio ("A PDF of your
  record. Anyone holding it can check it on Verify portfolio export."). At 390 px the three links are 44 px and
  nothing scrolls sideways. Access with the reason: "Request submitted. You will be notified when it is reviewed."
  took the focus; the row "2026-10-27 22:03 | Access | Submitted" with "Withdraw the Access request made 2026-10-27
  22:03". Withdraw acted at once (no dialog): "Request withdrawn." took the focus; the row reads Withdrawn and its
  actions cell is empty.
Gap: none (T311 no longer occurs: a graduate's Home, menu and STARs)

### Step A.1.7 — Dr Mahlangu asks for a correction
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Submit a Rectification request with the reason "My programme start is recorded as 15 January 2026; I started work
  on 19 January."
Expect: The request lists as Submitted, with a Withdraw.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Rectification with the reason: "Request submitted. You will be
  notified when it is reviewed." took the focus; the row "2026-10-27 22:03 | Rectification | Submitted" with "Withdraw
  the Rectification request made 2026-10-27 22:03".
Gap: none

### Step A.1.8 — Mr Smit reads the queue and rejects the correction
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid} → /admin/data-rights/{Id:guid}
Do: Clear both filters and apply them. Open Dr du Plessis's withdrawn request, then Dr Mahlangu's. Reject hers with the
  note "The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct."
Expect: The queue lists all three KGK requests, newest first, each with its status badge.
  - The withdrawn request's page shows the request and offers no decision.
  - Dr Mahlangu's reads Rejected once rejected, with its Decision card, and the focus moves to "Request rejected."
Note: Had he approved it, it would stay Approved for good: no page applies or completes a rectification (T112 § Still
  open; reported).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Both filters set to All and applied: three KGK rows, newest
  first: Mahlangu, Rectification, badge-submitted (Rejected once decided below); du Plessis, Access, Withdrawn
  (badge-draft); Dlamini, Export, Completed (badge-completed). The withdrawn request's page (01a17f63-7af4-...) shows
  its Request card (Status Withdrawn) and no Action card, no button. Mahlangu's (01a17f63-8c25-...), rejected with the
  note: "Request rejected." took the focus; Status Rejected, a Decision card (Smit's id, 22:03:39 UTC, the note), no
  buttons left. No mail.
Gap: none (the Note's stranded Approved rectification is from code and not played, since the step rejects)

### Step A.1.9 — Dr Mahlangu reads the decision
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Reload My data rights and read her request.
Expect: The request reads Rejected and offers nothing: no Withdraw, no Download. The page shows neither the decision
  note nor who decided, and no email told her of the decision.
Note: The confirmation at A.1.7 promised a notification (reported at A.1.2).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Reloaded, Your requests lists "2026-10-27 22:03 |
  Rectification | Rejected" with nothing in Actions (no Withdraw, no Download). The page shows neither the decision
  note nor who decided, and the log holds no mail about any data-rights request.
Gap: F-A.1.9a, T319 (still: a rejected requester is never told why, though A.1.7's confirmation promised a
  notification; the Expect records the page as it stands)

### Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: Press Schedule a review and schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period
  holding `D`, Scheduled on `D`, with Review type Formative check-in. Note the review's id from its address. Then open Decisions
  due, set Trainee to Dr Ndlovu and press Show.
Expect: The review opens as at Step 4.49: the subtitle "Formative check-in · Paed Annual Review Panel · Training year
  1", and the sitting bar Scheduled with "Formative: an interim check-in, no binding decision · evidence …" (T361). It
  is open, so the erasure at A.1.12 has a review to end. Decisions due's line reads "Formative check-in #N is open: it
  takes no seat." with Open check-in #N, as at Step 4.49.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Committee reviews -> Schedule a review
  (/committee/reviews/new): Panel, Trainee (disabled until a panel is chosen, then Anele Dlamini · Training year 3,
  Nomsa Mahlangu · Training year 1 and Sipho Ndlovu · Training year 1), Period ("2026 S2 · 1 Jul to 31 Dec 2026"
  chosen, which filled the window 2026-01-01 to 2026-12-31), Scheduled on 2026-10-28 (the default), Review type as
  three radios. With Annual: "16 EPAs will be on the agenda for 2026 S2." Formative check-in ticked: "A formative
  review carries no agenda: it decides no EPA." Create review opened review #8, h1 "Sipho Ndlovu · 2026 S2", subtitle
  "Formative check-in · Paed Annual Review Panel · Training year 1" with "Sipho Ndlovu's progress" and, new in the
  header, "Export Sipho Ndlovu's portfolio" (T379); the sitting bar reads Scheduled, "Formative: an interim check-in,
  no binding decision · evidence 2026-01-01 to 2026-12-31", "As of 00:04 SAST", Refresh and Start review... Decisions
  due (Period 2026 S2, Trainee Sipho Ndlovu, Show; the focus on the list heading "Decisions"): "16 of 46 decisions due
  in 2026 S2 shown: Sipho Ndlovu, outstanding.", "No open review holds 2026 S2." with Schedule a review, then
  "Formative check-in #8 is open: it takes no seat." with "Open check-in #8"; the rows each carry one Schedule action.
Gap: none

### Step A.1.11 — Dr Ndlovu requests erasure
Role: Trainee — Dr Sipho Ndlovu
Route: /account/data-rights
Do: Submit an Erasure request with the reason "I am leaving the programme and want my personal data removed." Stay
  signed in in this browser.
Expect: The request is accepted although a review of him is scheduled: no review's state refuses an erasure request
  (T258). It lists as Submitted, with a Withdraw.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Erasure with the reason, accepted although review #8 of him is
  Scheduled: "Request submitted. You will be notified when it is reviewed." took the focus; the row "2026-10-27 22:04
  | Erasure | Submitted" with "Withdraw the Erasure request made 2026-10-27 22:04". His browser was left on My data
  rights.
Gap: none

### Step A.1.12 — devadmin approves the erasure
Role: Administrator — devadmin
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Narrow the queue to Erasure and apply the filter, review Dr Ndlovu's request, and approve it with the note "Identity
  confirmed. Programme records are kept under a pseudonym."
Expect: The Administrator's queue holds every institution's requests.
  - Once approved, the request reads Completed at once, with its Decision card. The approval and the erasure are one
    transaction (T258).
  - No confirmation is asked before this irreversible action (noted on T264).
  - The queue and the request still name the requester by the address he signed in with: the erasure keeps the request
    as it was submitted (`ErasureExecutor`).
Note: Without `Wombat__PseudonymSalt` the approval is refused ("PseudonymSalt is not configured. Cannot execute erasure
  without it.") and the request stays Submitted, ready to approve again (T084).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Played in a second browser, so his stayed open. The
  Administrator's queue listed all four requests (KGK's are the only ones); Erasure applied, it held his alone.
  Approve with the note, no dialog in between: "Request approved." took the focus; Status Completed and a Decision
  card (devadmin's id abd4ed66-..., 22:04:50 UTC, the note, completed 22:04:50). The account became
  deleted_user_0ae1d64b (SQL: pseudonym equal to the user name, no email, no names, no password, no institution,
  lockout infinity; 0 roles, 0 logins, 0 activities on the original id); review #8 is Withdrawn (state 7, with its
  reason). The queue and the request still read ndlovu@kgk.wombat.local.
Gap: F-A.1.12a, T276 (still: the kept request names the erased person by his address, as the Expect records; approving
  the irreversible erasure asks nothing first, noted on T264)

### Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Sign in again." (T279), an
  information notice. Signing in is refused in the words an unknown address gets, "Invalid email or password.", with
  no mention of an erasure (T156). No email is sent to him.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): His tab, left on My data rights, went by itself 51 s after the
  approval to /account/login?error=SessionEnded&returnUrl=%2Faccount%2Fdata-rights: an information notice
  (alert-info), "Your session has ended. Sign in again." His old address and password: ?error=Refused, tab "Error:
  Sign in · Wombat", "Invalid email or password.", nothing of an erasure. No mail.
Gap: none

### Step A.1.14 — What the erasure left, as KGK sees it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or current trainees, and no row names him (T026, T258).
  - Users reads "People 14", with no pseudonym among them. Dr du Plessis's and Dr Molefe's Roles cells read "No role"
    over "A completed or withdrawn registrar keeps an account and its records.", each with Scope Paediatrics /
    Paediatrics and Status Active (T376, S2, U07).
  - Committee reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review as it was; the closed check-in, Type "Formative check-in", State Closed and Decision "—" (read as "None");
    and A.1.10's check-in, Type "Formative check-in", State Withdrawn and Decision "None: the review was withdrawn".
  - The check-in's sitting bar reads Withdrawn and "Withdrawn on `D`. Withdrawn because the trainee's personal data
    was erased at their request. Nothing more is decided at this review." (T258, T361), and the page offers no action:
    no Start, no Refresh.
  - Trainees' "Completed and closed" reads "3 ended" and lists his ended programme under the pseudonym, as Withdrawn
    on the erasure day (T379, F7, R3-C07): the latest end first, so his row (`deleted_user_…`, Paediatric EPA
    Curriculum (11.2) over Paediatrics, `J` to `D` over "Training year 1 when it ended", Withdrawn `D`, View named
    "View deleted_user_…'s record") stands first, before Lerato Molefe's (Completed `D`, the earlier profile) and
    Pieter du Plessis's (Withdrawn on Act 5's `D−2`). It shows no address, and no row names him.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.
Note: T276 (open) lists what an erasure does not yet end: an activity about him still in its workflow can be completed
  and credit the pseudonym, and user ids inside activity data are not rewritten.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Users (her page, lit) reads "People 14" with no Ndlovu and no
  pseudonym. Dr du Plessis's and Dr Molefe's Roles cells read "No role" over "A completed or withdrawn registrar keeps
  an account and its records.", each with Scope "Paediatrics / Paediatrics" and Status Active. Measured at 901, 1000
  and 1100 px wide: nothing scrolls sideways (the page 901, 1000, 1100 in the same; the table 593, 692 and 792 px,
  equal to its container), and no word splits mid-word (the only break found is "Sub-" / "speciality admin" after the
  hyphen, in Roles, at 901 and 1000). Trainees: "Waiting to be admitted: Nobody is waiting to be admitted."; "Current
  trainees, 2 current": Dlamini and Mahlangu, each with Edit; "Completed and closed, 3 ended", the latest end first:
  deleted_user_0ae1d64b (Paediatric EPA Curriculum (11.2) over Paediatrics, "2026-01-15 to 2026-10-28 / Training year
  1 when it ended", Withdrawn 2026-10-28, "View deleted_user_0ae1d64b's record"), Lerato Molefe (11.1, 2023-01-15 to
  2026-10-27, Training year 4 when it ended, Completed 2026-10-27) and Pieter du Plessis (11.1, 2025-01-15 to
  2026-10-25, Training year 2, Withdrawn 2026-10-25); no address, no Ndlovu. At 901, 1000 and 1100 px the Trainees
  page does not scroll sideways (page = viewport) and no word splits mid-word. Committee reviews lists three
  deleted_user_0ae1d64b rows: A.1.10's check-in (Formative check-in, Withdrawn, "None: the review was withdrawn", #8),
  the closed check-in (#6, Decision "— None") and Act 4's ratified review (Annual progression, Outcome Deferred).
  Review #8's sitting bar reads Withdrawn and "Withdrawn on 2026-10-28. Withdrawn because the trainee's personal data
  was erased at their request. Nothing more is decided at this review."; Evidence snapshot "No evidence was frozen:
  the review was withdrawn before it started."; no visible control (no Start, no Refresh). In SQL his profile (6) is
  inactive, deactivated 2026-10-28, under the pseudonym. Its header still offers "Export deleted_user_0ae1d64b's
  portfolio" (/portfolio/export/deleted_user_0ae1d64b), which opens "Portfolio unavailable / This portfolio does not
  exist, or you cannot export it." with Go to Home.
Gap: new: the erased person's review page offers an Export action in its header that leads only to "Portfolio
  unavailable" (the page and the refusal are right by T379; the offer is a dead end). The withdrawn review still draws
  the live Annexure A card, of 17 EPAs on 11.2, and a sampling warning, noted as before.

## A.2 — Scheduled jobs

### Step A.2.1 — devadmin reads the scheduled jobs
Role: Administrator — devadmin
Route: / → /admin/jobs
Do: Open Scheduled jobs from the menu and read every row.
Expect: Each row shows the job's description and its cron schedule (UTC), with its last run, that run's status badge
  and its next run in the server's local time. Each has Disable and Run now, both named for the job (T239). A job that
  has never run reads "—" for its last run and status. Run history is linked from the header. The eight jobs, by key
  (T379 retired `portfolio-export-cleanup` with its definition and runs, so a genuine PDF keeps verifying):
  - activity-draft-nudge
  - assessor-pending-nudge
  - audit-log-retention
  - entrustment-decision-expiry
  - msf-campaign-auto-close
  - msf-invitation-expiry-reminder
  - scheduled-job-run-retention
  - weekly-coordinator-digest
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Scheduled jobs from the Platform group, lit, tab "Scheduled
  jobs · Wombat". Columns Job, Schedule, Last run, Status, Next run, Enabled, Actions: the EIGHT jobs by key in the
  runbook's order (activity-draft-nudge, assessor-pending-nudge, audit-log-retention, entrustment-decision-expiry,
  msf-campaign-auto-close, msf-invitation-expiry-reminder, scheduled-job-run-retention, weekly-coordinator-digest),
  each with its description, cron (activity-draft-nudge 0 7 * * *, last run 2026-10-27 19:55, next 2026-10-28 09:00
  local), "Disable <key>" and "Run now: <key>"; no portfolio-export-cleanup. Seven had run (the scheduler's catch-up
  at 19:55 UTC, msf-campaign-auto-close hourly, assessor-pending-nudge by hand in Act 3) and read a Succeeded badge;
  weekly-coordinator-digest had never run and reads "—" for its last run and status, with next run 2026-11-02 10:00.
  Run history is a header action.
Gap: none (T313's ninth job no longer occurs: the 90-day cleanup is retired, so eight scheduled jobs)

### Step A.2.2 — devadmin disables a job and enables it again
Role: Administrator — devadmin
Route: /admin/jobs
Do: Disable msf-campaign-auto-close, try to run it now, then enable it again.
Expect: Disabling says "Job 'msf-campaign-auto-close' disabled." The row then reads "—" for its next run, its toggle
  says Enable, and its Run now is unavailable. Once enabled again, its next run is the next hour. The scheduler does not
  run a disabled job. The audit log records both changes.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Disable: "Job 'msf-campaign-auto-close' disabled." took the
  focus; the row reads Next run "—", its toggle "Enable msf-campaign-auto-close", and its Run now is disabled (a
  forced press did nothing; the alert stayed the disable's). Enable: "Job 'msf-campaign-auto-close' enabled.", next
  run 2026-10-28 01:00 (the next hour), Run now available again.
Gap: none

### Step A.2.3 — Mr Smit opts out of digest emails
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."
Actual (2026-10-28, T379 replay, wombat_scenario_t379): My data rights from the menu; "Opt out of digest emails" was
  clear, ticked and saved: "Processing preferences saved." took the focus, and the box stays ticked.
Gap: none

### Step A.2.4 — devadmin runs the weekly digest, and Mr Smit is skipped
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the application log.
Expect: "Job 'weekly-coordinator-digest' dispatched."; the row's last run is now, and it reads Succeeded. The log holds
  "WeeklyCoordinatorDigestJob: digests sent n; coordinators skipped: …", with at least one counted as "opted out of
  digest emails". No "Your weekly Wombat digest" mail greets Pieter.
Note: Each mail is logged as "Stub email <reference> … not sent", with its subject and text and no address (T282). The
  Demo Institution's dev coordinator may be sent a digest; ignore it.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Job 'weekly-coordinator-digest' dispatched." took the focus;
  the row's last run is 00:07, Succeeded. The log reads "WeeklyCoordinatorDigestJob: digests sent 1; coordinators
  skipped: deactivated 0, opted out of digest emails 1, no email address 0, holds Trainee 0, no institution 0." Its
  one stub mail "Your weekly Wombat digest" greets "Hi Demo," (the dev coordinator); none greets Pieter.
Gap: none

### Step A.2.5 — Mr Smit opts back in
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Untick "Opt out of digest emails" and save.
Expect: "Processing preferences saved."
Actual (2026-10-28, T379 replay, wombat_scenario_t379): The digest opt-out opened ticked; unticked and saved:
  "Processing preferences saved.", the box now clear.
Gap: none

### Step A.2.6 — devadmin runs the digest again, and Mr Smit gets it
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the log.
Expect: A stub mail "Your weekly Wombat digest" begins "Hi Pieter,". It covers KGK only (T117). With nothing to list
  it says "No items requiring attention this week." Otherwise it lists:
  - under "Trainees at risk", any current KGK trainee who has filed nothing in the last 30 days by the real clock
    (T284): a draft is not a filing, and a registrar admitted less than 30 days ago is not listed (T358, E5); never
    Dr Molefe or Dr du Plessis, whose programmes have ended, or Dr Ndlovu, who has been erased;
  - any MSF campaign of his waiting on its review;
  - any committee review scheduled in the coming week.
Note: A second log line accounts for the earlier run's mail. On dev, where no mail worker reports, it counts that mail
  as "not reported before the job ran again" (T283).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Job 'weekly-coordinator-digest' dispatched.", Succeeded
  (00:07). The log reads "WeeklyCoordinatorDigestJob mail of the run started 2026-10-27 22:07:20Z: sent 0, not
  delivered 0, not reported before the job ran again 1." and "digests sent 2; coordinators skipped: ... opted out of
  digest emails 0 ...". The stub "Your weekly Wombat digest" reads "Hi Pieter, Here is your weekly summary: No items
  requiring attention this week.": KGK only, no trainee listed under "Trainees at risk", no campaign waits, no review
  is scheduled in the coming week (review #8 is withdrawn). The Demo coordinator's digest was ignored.
Gap: none

### Step A.2.7 — Dr Mahlangu leaves a draft and a request waiting
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int} → /activities/new →
  /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int} → /activities/mine
Do: File two Mini-CEX (Paediatrics) activities, each with every request field filled, an EPA the list offers, and Dr
  Khumalo as assessor. Then open My activities.
  - The first: the encounter on `D−3`, then Save draft.
  - The second: the encounter on `D−2`, then Submit to Fatima Khumalo.
Expect: The first reads "Draft saved. It has not been submitted. It is in nobody's inbox until you submit it." The
  second reads "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox." My activities' Needs you
  lists the first and not the second. No email is sent: Wombat mails nobody when an activity moves (Step 3.3).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Log an activity opened the picker ("Rated by an assessor", 7
  types); Mini-CEX (Paediatrics) opened /activities/new?type=mini_cex_cpsa, its EPA list offering 9. Both with
  PAED-002, Khumalo as assessor, Ward, a presenting problem and complexity Moderate; the button read "Submit to Fatima
  Khumalo". Activity 26 (encounter 2026-10-25, D-3), Save draft: "Draft saved. It has not been submitted. It is in
  nobody's inbox until you submit it." took the focus; Who has it now "Draft / With you. Not submitted yet." Activity
  27 (encounter 2026-10-26, D-2), Submit to Fatima Khumalo: "Submitted. It is now Requested. It is in Fatima Khumalo's
  Activity inbox." took the focus. My activities' Needs you (1) lists 26 alone, as Draft; All activities (4) lists 27
  as with Fatima Khumalo, Requested. Khumalo's inbox, Waiting for you "2 waiting", lists du Plessis's CBD (since
  2026-10-27 21:29 SAST) then "Mini-CEX (Paediatrics) · PAED-002 · 2026-10-26 ... from Nomsa Mahlangu ... Requested".
  No mail.
Gap: none

### Step A.2.8 — devadmin runs the two reminders against aged work
Role: Administrator — devadmin
Route: /admin/jobs
Do: Age the draft by 15 days and the request by 6 (see the Note). Then run activity-draft-nudge and
  assessor-pending-nudge now, and read the log.
Expect: The log holds a stub mail "You have draft activities waiting" beginning "Hi Nomsa,", listing "Mini-CEX
  (Paediatrics) — draft for 15 days". It also holds "Activities awaiting your assessment", beginning "Hi Fatima,",
  which lists "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 6 days". Each job logs one line counting whom it
  reminded and whom it skipped, and why (T151, T240).
Note: This is a stand-in for elapsed time, since both jobs read the real clock. Run
  `UPDATE "Activities" SET "UpdatedOn" = "UpdatedOn" - interval '15 days' WHERE "Id" = <the draft>;` and the same with
  `interval '6 days'` for the request. Both ids are in the activities' addresses.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Stand-in as the Note prescribes: UpdatedOn -15 days on
  activity 26 (now 2026-10-13) and -6 days on 27 (now 2026-10-22). Both jobs dispatched and Succeeded (00:08). Stub
  mails: "You have draft activities waiting", "Hi Nomsa, ... Mini-CEX (Paediatrics) - draft for 15 days"; and
  "Activities awaiting your assessment", "Hi Fatima, ... Mini-CEX (Paediatrics) from Nomsa Mahlangu - waiting 6 days"
  (the log prints the dash as "-"; "Hi Mohammed," was also nudged for du Plessis's portfolio review, "waiting 8
  days"). Logs: "ActivityDraftNudgeJob: trainees reminded 1 (drafts 1); trainees skipped: no such account 0,
  deactivated 0, opted out of digest emails 0, no email address 0." and "AssessorPendingNudgeJob: assessors nudged 2
  (activities 2); nominees skipped: ... 0 ...".
Gap: none

### Step A.2.9 — devadmin runs the other five jobs
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run each of the remaining jobs now, one at a time, and read each one's log line.
Expect: Each run reads Succeeded, and each job logs one line:
  - entrustment-decision-expiry: how many decisions it expired and how many expiring within 30 days it reminded. A
    reminder mail is "Entrustment decision expiring soon: PAED-0nn", sent at most once a day per decision.
  - msf-campaign-auto-close: "no expired campaigns found.", since Act 3's campaign was closed and released.
  - msf-invitation-expiry-reminder: "no respondent is due a reminder."
  - audit-log-retention: "complete. Total archived: 0.", since nothing is two years old.
  - scheduled-job-run-retention: "no old runs to delete."
Actual (2026-10-28, T379 replay, wombat_scenario_t379): The five remaining jobs, run now one at a time, each read
  Succeeded (00:08 to 00:09), with one log line each: "EntrustmentDecisionExpiryJob: expired 0 decisions (sent 0),
  reminded 0 expiring soon (sent 0)."; "MsfCampaignAutoCloseJob: no expired campaigns found.";
  "MsfInvitationExpiryReminderJob: no respondent is due a reminder."; "AuditLogRetentionJob: complete. Total archived:
  0."; "ScheduledJobRunRetentionJob: no old runs to delete." There is no PortfolioExportCleanupJob line and no sixth
  job. No mail.
Gap: none (T313 no longer occurs: portfolio-export-cleanup is retired with its definition and runs)

### Step A.2.10 — devadmin reads the run history
Role: Administrator — devadmin
Route: /admin/jobs → /admin/jobs/runs
Do: Open Run history. Filter it:
  - by the key "weekly-coordinator-digest";
  - by the key "digest";
  - by the status Failed;
  - from today.
Expect: The runs are listed newest first, each with its key, start, finish, duration, status badge, who triggered it
  and any error. A run by hand names the person who ran it, devadmin, by name, never by user id (DESIGN.md § Page
  shapes, T142); one the scheduler ran reads "scheduler". Filters apply only when Filter is pressed.
  - The full key shows the digest's runs, the two by hand among them, and no other job's.
  - "digest" alone matches nothing: the filter wants the whole key.
  - Failed shows "No runs found".
  - From today shows today's runs, those of this appendix among them. The date is read as a UTC day.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Run history, from the jobs header (trail Home > Scheduled jobs
  > Job run history), lists 22 runs newest first: Key, Started, Finished, Duration, Status, Triggered by, Error. The
  scheduler's read "scheduler"; the ones by hand read abd4ed66-e9ad-4187-b87f-c3c481d05c30, devadmin's user id. Typing
  a key changed nothing until Filter. "weekly-coordinator-digest": its two runs by hand, no other job. "digest": "No
  runs found". Status Failed: "No runs found". From 2026-10-28 (today on the SAST calendar, 00:10 SAST): "No runs
  found", since the date is read as a UTC day and every run so far is on 2026-10-27 UTC; from 2026-10-27: 22, today's
  runs, the appendix's among them. The key's placeholder reads "Filter by key...".
Gap: F-A.2.10a, T324 (still: "Triggered by" prints devadmin's user id, not his name as the Expect says). F-A.2.10b,
  T327 (still: the placeholder invites a part, and only the whole key matches). F-A.2.10c, new (kin to
  T325: the From date is read as a UTC day, so until 02:00 SAST "from today" on the SAST date finds nothing).

## A.3 — Institutional sign-in (SSO)

### Step A.3.1 — Prof Mbatha opens SSO mappings with no provider configured
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/sso/group-mappings
Do: Open SSO mappings from the menu.
Expect: "SSO mappings", subtitled "Which of your institution's sign-in groups give which Wombat role. A change applies at
  each person's next sign-in." One card, headed "Institutional sign-in is not set up", reads "Institutional sign-in is
  not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them if your hospital signs
  staff in through its own system." and "Until then, your people sign in with an email and a password, and there is
  nothing to change on this page." It names no setting and no restart (T326, Q11). There is no form, no Wombat role
  select, no list of mappings and no Delete…: nothing on the page can be changed, and nothing on it offers Trainee or
  Pending trainee (T333). At 390 px nothing scrolls sideways.
Note: The form is drawn only once a provider is set up for KGK, which the replay never has. T333's browser check, that
  the Wombat role select offers neither Trainee nor Pending trainee (nor, to her, Institutional admin, Q19), is played
  on a scratch instance with a provider (`states.md`, `group-mappings--provider-form`): its options are Speciality
  admin, Sub-speciality admin, Coordinator, Committee member and Assessor, under "Staff roles only. Trainee and Pending
  trainee are never mapped: a registrar becomes a trainee only when admitted.". T288 (a mapping only for the
  provider's own institution) is the handlers' and unit-tested.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): SSO mappings from the "Access and audit" group, lit; tab "SSO
  mappings · Wombat". The page reads "SSO mappings", "Which of your institution's sign-in groups give which Wombat
  role. A change applies at each person's next sign-in.", and one card, headed "Institutional sign-in is not set up":
  "Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them
  if your hospital signs staff in through its own system." and "Until then, your people sign in with an email and a
  password, and there is nothing to change on this page." It names no setting and no restart. There is no form, no
  Wombat role select, no list of mappings and no visible Delete: the only controls in the DOM are in a closed,
  invisible "Delete this mapping?" dialog (Keep the mapping, Delete) that nothing can open, and nothing on the page
  offers Trainee or Pending trainee. At 390 px nothing scrolls sideways (390 in 390).
Gap: none (the closed dialog is not visible)

### Step A.3.2 — The sign-in page offers no institutional sign-in
Role: Anonymous — a KGK consultant
Route: /account/login → /account/sso-challenge/{providerKey} → /account/login
Do: Look for an institutional sign-in button. Sign in with a wrong password. Then open the challenge address for a
  provider called `kgk`.
Expect: Under the line "Work-based assessment for specialist training." and the heading "Sign in", the page offers an
  email, a password with its Show toggle, Remember me and the "Forgotten your password?" link. There is no "or"
  divider and no "Sign in with …" button. The wrong password reads "Invalid email or password.", which takes the
  focus, and points to no institutional button (T156); the tab's title starts "Error:". The challenge for a provider
  that is not configured returns to the sign-in page, which says "That institution's sign-in is not set up in Wombat.
  Sign in with your email and password."
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Signed out, the sign-in page reads "Work-based assessment for
  specialist training." and "Sign in", then Email, Password with Show ("Show password"), Remember me, Sign in and
  "Forgotten your password?", with no "or" divider and no "Sign in with ..." button. A wrong password for
  zulu@kgk.wombat.local: ?error=Refused, tab "Error: Sign in · Wombat", "Invalid email or password." (#login-error,
  alert-danger) took the focus, naming no institutional button. /account/sso-challenge/kgk returned to
  /account/login?error=SsoUnknownProvider: "That institution's sign-in is not set up in Wombat. Sign in with your
  email and password."
Gap: none

### Step A.3.3 — The callback and link pages with no institutional sign-in in progress
Role: Anonymous — a KGK consultant
Route: /account/sso-callback → /account/login → /account/link-external
Do: Open the callback address, then the link-your-account page, directly.
Expect: The callback returns to the sign-in page, which says "Your institution's sign-in did not complete. Sign in with
  your email and password." (the words of a page with no institution's button: none is configured). The link page is
  headed "Institutional sign-in expired" and says "Your institutional sign-in has expired. Start again from the
  sign-in page." It offers Back to sign in and no password field (T149).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /account/sso-callback returned to
  /account/login?error=ExternalLoginUnavailable: "Your institution's sign-in did not complete. Sign in with your email
  and password." /account/link-external: tab and heading "Institutional sign-in expired", "Your institutional sign-in
  has expired. Start again from the sign-in page." and only Back to sign in, with no password field. At 390 px nothing
  scrolls sideways (390 in 390).
Gap: none

## A.4 — Account self-service

### Step A.4.1 — Dr Botha reviews and edits her account
Role: CommitteeMember — Dr Sarah Botha
Route: / → /account/profile → /account/profile/submit → /account/profile → /account/profile/submit → /account/profile
Do: Open My account from her name in the top bar. Clear her last name and save. Then type a single space as her last
  name and save. Then put "Botha" back and save.
Expect: On My account her name in the top bar is the current page (underlined), and nothing in the menu is lit. The
  Account card shows her address as text, her institution (Kgosi Kgari Teaching Hospital) and both her roles, one per
  line ("Committee member", "Assessor"); How you sign in shows Password with Change password. The cleared last name is
  stopped by the browser's own required-field check, and nothing is sent. The space is sent and refused: the page
  reloads with "Your name was not saved. Enter your last name." in the Your name card, which takes the focus. Last
  name is marked, empty, and names its own message, "Enter your last name."; First name reads "Sarah"; the tab reads
  "Error: My account · Wombat"; nothing is saved. Restored, Save name reloads the page with "Name saved.", which takes
  the focus (T234), and the top bar's account row names her as saved, "Sarah Botha".
Actual (2026-10-28, T379 replay, wombat_scenario_t379): My account from "Sarah Botha" in the top bar: the name carries
  aria-current, underlined, and nothing in the menu is lit; tab "My account · Wombat". Account card:
  botha@kgk.wombat.local as text, Kgosi Kgari Teaching Hospital, and Roles "Committee member" and "Assessor" one per
  line; How you sign in shows Password, "Your Wombat password.", Change password. Last name cleared: the browser's own
  check ("Please fill out this field."), nothing posted. A space: posted, back at ?error=LastNameMissing, tab "Error:
  My account · Wombat", "Your name was not saved. Enter your last name." in the Your name card took the focus; Last
  name is aria-invalid, empty, and names "Enter your last name."; First name reads Sarah; nothing saved. "Botha":
  ?status=saved, "Name saved." took the focus; the top bar names "Sarah Botha".
Gap: none

### Step A.4.2 — Dr Khumalo changes her password, getting it wrong first
Role: Assessor — Dr Fatima Khumalo
Route: /account/profile → /account/change-password → /account/profile
Do: Open My account (the name in the top bar), then its Change password. Try four times, and record the password (d)
  sets in `pwd_DO_NOT_COMMIT.txt`:
  - (a) a wrong current password;
  - (b) the right current password, with a new password and a confirmation that differ;
  - (c) the right current password, with a new one of 8 different lower-case letters;
  - (d) the right current password, with a new one that meets every rule.
Expect: The page, "Change password" with "Choose a new password for signing in to Wombat.", lists the six rules under
  New password before anything is typed (Step 2.9's), and each field has its Show toggle: "Show current password",
  "Show new password", "Show confirm new password". Each refusal reloads the page with empty fields. The tab's title
  starts "Error:", and "Your password was not changed." and the reason stand in the form's card above its buttons,
  taking the focus; the field the refusal concerns is marked and names it (T265, T193).
  - (a) reads "Incorrect password."; Current password is marked.
  - (b) reads "The password confirmation does not match."; Confirm new password is marked.
  - (c) reads "The new password needs:" and "At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.",
    "A symbol, such as ! or #.", in the six rules' order. It says nothing of lower-case letters, a rule it keeps. New
    password says "The new password does not meet the rules below."
  - (d) lands on My account with "Password updated." under its header, which takes the focus, and she stays signed in
    in this browser.
Note: Before this step, sign Dr Khumalo in on a second browser and leave it on her Activity inbox. Five wrong current
  passwords in a row would lock the account for 15 minutes and sign her out: the sign-in page would say "Your current
  password was entered incorrectly too many times, so your account is locked for 15 minutes and you have been signed
  out. Wait 15 minutes, then sign in again." (states.md, Account and sign-in).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): A second browser was signed in as Khumalo on her Activity
  inbox first. From My account's Change password (tab "Change password · Wombat", trail Home > My account > Change
  password): "Choose a new password for signing in to Wombat.", the six rules under New password before anything is
  typed, and toggles "Show current password", "Show new password", "Show confirm new password". Each refusal came back
  by a full load with all three fields empty, tab "Error: Change password · Wombat", and "Your password was not
  changed." with the reason in the form's card above Cancel and Change password, taking the focus; the field concerned
  is aria-invalid and names it. (a) ?error=PasswordMismatch, "Incorrect password.", Current password marked. (b)
  ?error=ConfirmationMismatch, "The password confirmation does not match.", Confirm marked. (c) "qwertyui": "The new
  password needs:" "At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.", "A symbol, such as ! or
  #."; nothing on lower case; New password names "The new password does not meet the rules below." (d)
  ?status=password-updated, "Password updated." took the focus; she stays signed in. Recorded in the replay's password
  file (WB_PW_KHUMALO is the new one; the old one kept as WB_PW_KHUMALO_A). The audit holds no row for the change or
  the wrong current password (SQL: none).
Gap: F-A.4.2a, T286 (still: a password change and a wrong current password write no audit row; the Expect does not
  ask)

### Step A.4.3 — Dr Khumalo's other session ends
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Sign in again."
  (T279), an information notice. The old password is refused, and "Invalid email or password." takes the notice's
  place. The new one brings her back to her Activity inbox: the return address survives the refusal.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): The second browser's inbox left 54 s after the change, by a
  full load, for /account/login?error=SessionEnded&returnUrl=%2Factivities%2Finbox: the information notice "Your
  session has ended. Sign in again." The old password: ?error=Refused, "Invalid email or password." in its place. The
  new one brought her back to /activities/inbox (Mahlangu's Mini-CEX, 6 days since 2026-10-22 00:08 SAST, and du
  Plessis's CBD, less than a day, both Requested).
Gap: none

### Step A.4.4 — Dr du Plessis has forgotten his password
Role: Anonymous — Dr Pieter du Plessis
Route: /account/login → /account/forgot-password → /account/login
Do: Follow "Forgotten your password?" from the sign-in page.
Expect: A "Forgotten password" page: "A Wombat administrator can set a new password for you." then three steps ("Ask
  your Wombat administrator for a new password: the one at your institution or, if your account belongs to no
  institution, the platform's administrator. They give it to you themselves; Wombat does not email it." "Sign in with
  it." "Choose your own password on My account, under Change password."), and Back to sign in. There is no field, and
  no email is sent.
Note: Wombat sends no reset email: an administrator sets the password (A.4.5), and nothing uses the
  `PasswordResetEmail` template (not played).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Forgotten your password?" led to /account/forgot-password,
  tab "Forgotten password · Wombat": "Forgotten password", "A Wombat administrator can set a new password for you.",
  then the three steps word for word as the Expect quotes them, and Back to sign in, which returned to /account/login.
  No field; no mail.
Gap: none

### Step A.4.5 — Prof Mbatha resets his password
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Under Sign-in and access, in Password, set a new password of 10 characters, then one
  that meets every rule. Record the second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: Roles and scope reads "**Pieter du Plessis holds no role: they left the programme on `D−2`.** They can still sign
  in to read their training record (My progress) and their data rights." (T376, S2, Q13; D54), in place of "This user
  has no roles.". His Lockout lists only "They are sent no activity reminders or weekly digests.".
  - **Password** says "Sets a new password directly. The user is not emailed; share the password securely
    out-of-band." Its field has the Show toggle ("Show new password") and the six rules under it. Reset password is
    always offered, never disabled.
  - **The 10-character password** is refused in Password, under its heading, not at the page's top: "The password was
    not reset. The new password needs:" and the rules it breaks, in the six rules' order. The field is marked "The new
    password does not meet the rules below." and cleared.
  - **The second** is accepted. In Password, focused: "**Password reset for Pieter du Plessis.** Share the new password
    with Pieter du Plessis out-of-band." The field is cleared.
  - The audit log records the reset with the password redacted (T101).
Note: Resetting a password is 1 press and 1 field (the password, Reset password), at 1280 and at 390.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Users -> "Manage Pieter du Plessis" (tab "Pieter du Plessis ·
  Wombat", trail Home > Users > Pieter du Plessis, Users lit). Roles and scope reads "Pieter du Plessis holds no role:
  they left the programme on 2026-10-25. They can still sign in to read their training record (My progress) and their
  data rights.", with Add a role… and "Trainee is not offered: a registrar becomes a trainee only when admitted, from
  Trainees with 'Admit…'." His Lockout lists only "They are sent no activity reminders or weekly digests." and the
  records sentence. No Pending invitations card. Password: its field with "Show new password" and the six rules under
  it; Reset password is enabled while the field is empty. "shortpass1": under the Password heading, not at the page's
  top, "The password was not reset. The new password needs: At least 12 characters. An upper-case letter. A symbol,
  such as ! or #." took the focus; the field emptied, aria-invalid and naming "The new password does not meet the
  rules below." A password meeting every rule: "Password reset for Pieter du Plessis. Share the new password with
  Pieter du Plessis out-of-band." (the first sentence bold) took the focus; the field cleared. The audit holds
  ResetUserPasswordCommand twice false and twice true, all with the password [REDACTED]: the step's script was run a
  second time by mistake (a re-run of a state-changing script, which refused and reset again, leaving two more audit
  rows); the first admin-set password was superseded by the second, and both are recorded (WB_PW_DUPLESSIS_ADMINSET is
  the latest; WB_PW_DUPLESSIS_A the original). At 390 px nothing scrolls sideways and Reset password, Add a role…,
  Lock out… and the Show toggle are each 44 px tall.
Gap: none

### Step A.4.6 — Dr du Plessis signs in with it and chooses his own
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /account/login → / → /account/profile → /account/change-password → /account/profile
Do: Sign in with the password Prof Mbatha set. Then change it, from My account (the name in the top bar), to one of
  his own. Record it in `pwd_DO_NOT_COMMIT.txt`.
Expect: He lands on Home, with the former trainee's card "Your training record" as at Step 5.28 (T290: he holds no
  role; T379, R3-H02): "Your programme ended on `D−2`.", My progress, STARs as plain text with "No STAR was issued to
  you, so there is no certificate to download.", My activities and Export portfolio; his menu reads Home, My progress,
  Export portfolio and My data rights. The change lands on My account with "Password updated."
Actual (2026-10-28, T379 replay, wombat_scenario_t379): With the password Prof Mbatha set he lands on /, holding no
  role: the menu reads Home, My progress, Export portfolio and My data rights (no "Acting as"). Home's one card, "Your
  training record": "Your programme ended on 2026-10-25." (D-3 on this day), "Your record is kept, read-only: no
  target applies to you any more.", then My progress, STARs as plain text ("No STAR was issued to you, so there is no
  certificate to download."), My activities and Export portfolio (My progress, My activities and Export portfolio are
  the links; 44 px at 390 px, the card 16 to 374, nothing scrolls sideways, 390 in 390). My account (his name in the
  top bar) -> Change password, to his own: ?status=password-updated, "Password updated." took the focus. Recorded in
  the replay's password file (WB_PW_DUPLESSIS is his own; WB_PW_DUPLESSIS_A the original; WB_PW_DUPLESSIS_ADMINSET the
  one Prof Mbatha set).
Gap: none (T311 no longer occurs: the former trainee's Home, menu and STARs)

### Step A.4.7 — Mr Smit signs out through the confirmation page
Role: Coordinator — Mr Pieter Smit
Route: /account/logout-confirm → / → /account/logout → / → /account/logout-confirm → /account/logout/submit → /account/login → /msf/campaigns → /account/login → /msf/campaigns
Do: Open the sign-out confirmation by its address and press Cancel. Type `/account/logout` and press Cancel. Open the
  confirmation again and sign out. Then open MSF campaigns by its address.
Expect: A "Sign out" page, with no nav: "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends
  your session in this browser.", with Cancel and Sign out. Cancel returns him to his dashboard, still signed in. A
  typed GET /account/logout draws the same page and signs nobody out (T317). Sign out lands on the sign-in page with
  "You have signed out." MSF campaigns then asks him to sign in, and after signing in he is brought back to it, the
  list as Step 3.46 left it (Pieter du Plessis's Withdrawn, Dr Molefe's Released). The audit log records a Logout.
Note: Only the error page links to the confirmation page: its account row's Sign out is a link there, since a form
  drawn on the error page could carry a refused token. Everywhere else the account row's Sign out, in the top bar or
  the phone menu's foot, is a form that signs out at once.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /account/logout-confirm, tab "Sign out · Wombat", with no nav:
  "Sign out", "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends your session in this
  browser.", Cancel and Sign out; at 390 px nothing scrolls sideways. Cancel (a link to /) returned him to Home, still
  signed in. A typed GET /account/logout drew the same page at that address (200), and its Cancel returned him to /
  signed in. Sign out landed on /account/login?error=SignedOut: "You have signed out." (information notice).
  /msf/campaigns then went to /account/login?ReturnUrl=%2Fmsf%2Fcampaigns, and signing in brought him back to MSF
  campaigns (lit), "Campaigns (2)": Pieter du Plessis, Default MSF, Withdrawn, 0 invitations, "—", closes 2026-11-10,
  View campaign; then Lerato Molefe, Released, 9 invited, 8 responded, closes 2026-11-10, View campaign and View
  report, each action named for the campaign. The audit holds one Logout.
Gap: none

## A.5 — System pages

### Step A.5.1 — A registrar opens an administrator's pages
Role: Trainee — Dr Anele Dlamini
Route: /admin/users → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Users page, then that of Scheduled jobs.
Expect: Each time, "You cannot open this page": "Your role (Trainee) does not open this page." and "If you need it for
  your work, ask your institution's Wombat administrator.", with Go to Home, which takes her home. Nothing of the page
  she asked for is shown or named, and no switch of role is offered (T335, D6).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /admin/users went to
  /access-denied?ReturnUrl=%2Fadmin%2Fusers, tab "You cannot open this page · Wombat": "You cannot open this page",
  "Your role (Trainee) does not open this page.", "If you need it for your work, ask your institution's Wombat
  administrator." and Go to Home, which took her to /. /admin/jobs gave the same. Nothing of either page is named; no
  switch is offered; nothing is lit. At 390 px nothing scrolls sideways.
Gap: none

### Step A.5.2 — Prof Mbatha opens the Administrator's own pages
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled jobs.
Expect: For both, "You cannot open this page": "Your role (Institutional admin) does not open this page." and "If you
  need it for your work, ask the platform administrator.", with Go to Home. They are the Administrator's alone, her menu
  offers neither, and nothing in it is lit on the refusal.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Her menu (People, Curriculum, Reviews, Access and audit)
  offers neither page. /admin/institutions and /admin/jobs each went to /access-denied (ReturnUrl kept): "Your role
  (Institutional admin) does not open this page.", "If you need it for your work, ask the platform administrator." and
  Go to Home; nothing in the menu is lit.
Gap: none

### Step A.5.3 — A forged dashboard switch
Role: Trainee — Dr Anele Dlamini
Route: /dashboard/switch/{role} → /
Do: Open `/dashboard/switch/Administrator`.
Expect: She is back on her own dashboard, still acting as a Trainee. A switch to a role she does not hold changes
  nothing and says nothing (no "You are now acting as" alert), and the sidebar offers her no switch: she holds one role.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /dashboard/switch/Administrator redirected to /, "Trainee ·
  Semester 2, 2026", Home lit; no alert, and the menu ("Acting as Trainee": Home to My data rights) offers no switch.
Gap: none

### Step A.5.4 — Addresses that do not exist
Role: Trainee — Dr Anele Dlamini
Route: /not-found → /not-found → /account/login → /not-found
Do: Type the following addresses:
  - `/portfolio/cv`;
  - `/placeholder/recent-activities`, which the Assessor's menu once linked;
  - then, signed out, `/portfolio/cv` again.
Expect: For each, the "Page not found" page: "There is no page at this address." and "Check the address, or start
  again from Home.", with Go to Home; the page does not repeat the address (T335). The address stays as typed, and the
  status is 404 (the browser's network panel) (T233).
  - `/placeholder/recent-activities` is Page not found too: the placeholder page went with the stubs (T335, flow 01).
  - Signed out, the unknown address first asks her to sign in, then shows Page not found.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Signed in, /portfolio/cv and /placeholder/recent-activities
  each answered 404 at the typed address, tab "Page not found · Wombat": "There is no page at this address.", "Check
  the address, or start again from Home." and Go to Home, with no address repeated, nothing lit. At 390 px nothing
  scrolls sideways. Signed out, /portfolio/cv went to /account/login?ReturnUrl=%2Fportfolio%2Fcv; signing in there
  gave Page not found at /portfolio/cv, 404.
Gap: none

### Step A.5.5 — Another institution's records, by id
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not "You cannot open this page". The user shows "User unavailable" ("The
  user could not be found or is outside your scope."), headed and tabbed "User", under Home › Users › User, with Go to
  Users and no Back to users (T376, P23). Neither page confirms that the record exists (CLAUDE.md: 404, not 403).
Note: The Demo Institution stands in for another institution only here. Its ids come from
  `SELECT "Id" FROM "Institutions" WHERE "Name" = 'Demo Institution'` and from `"AspNetUsers"`. On a host without the
  dev accounts, play the institution half only.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /admin/institutions/1 (the Demo Institution) moved to
  /not-found, "Page not found", as /admin/institutions/999 does. The dev trainee's /admin/users/57999f24-... stays at
  its address, tab "User · Wombat", trail Home > Users > User, Users lit: the heading "User", "User unavailable / The
  user could not be found or is outside your scope." and Go to Users, with no Back to users; the same as an unknown id
  (00000000-...-0001). Neither reveals that the record exists.
Gap: none

### Step A.5.6 — Another registrar's activity, by id
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Open the address of Dr Mahlangu's submitted Mini-CEX from A.2.7.
Expect: The page is headed "Activity unavailable" and reads "This activity does not exist, or you cannot open it.", with
  Go to My activities: the same page an id that does not exist shows, so nothing of the activity is shown and nothing
  tells her it exists (T101, C7).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /activities/27 (Dr Mahlangu's requested Mini-CEX) stays at its
  address, tab "Activity unavailable · Wombat", trail Home > My activities > Activity unavailable (My activities lit):
  the heading "Activity unavailable", "This activity does not exist, or you cannot open it." and Go to My activities.
  /activities/99999 reads word for word the same; nothing of the activity is shown.
Gap: none

### Step A.5.7 — A data-rights request that is not his, by id
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): /admin/data-rights/01a17f63-8c25-7c6e-8724-8593f3d7a145
  (Mahlangu's id, which opens her Rejected request) with its last digit changed to 6, tab "Data rights request ·
  Wombat", trail Home > Data rights requests > Data rights request: the alert "You are not authorized to access this
  data-rights request." above "Not found / The data rights request was not found.", and Back to list. No other
  institution holds a request.
Gap: none (F-A.5.7a is not a defect: T112 makes an unknown and a foreign id read alike)

### Step A.5.8 — The error page
Role: Trainee — Dr Anele Dlamini
Route: /Error
Do: Type `/Error`.
Expect: Typed, no request failed: "Nothing went wrong" and "This is Wombat's error page, opened directly. No request
  failed, so there is nothing to report.", with Go to Home, no reference and no Try again. Signed out it reads the same,
  with no sign-in first: the page is open to everyone, and static (T321, T335).
Note: Outside Development a failed page load is answered here with status 500 (T321): "Something went wrong", "Wombat
  could not finish this request. Try again. If it keeps happening, send this reference to your institution's Wombat
  administrator." (signed out: "… to whoever sent you the link, or to your Wombat administrator."), the reference (the
  request's 32-character trace id, also in the log with the failure) and the time in SAST, then Try again (the failed
  address) and Go to Home. On dev the developer exception page shows instead, and a failure inside a page shows the
  error bar ("This page no longer responds; …"); `Hosting/ErrorPageFlowTests` plays the failure (states.md § System
  pages).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Signed in, /Error: status 200, tab "Nothing went wrong ·
  Wombat", "Nothing went wrong", "This is Wombat's error page, opened directly. No request failed, so there is nothing
  to report." and Go to Home; no reference and no Try again. Signed out (a fresh browser) it reads the same at /Error
  under the signed-out bar (Sign in), with no sign-in first. At 390 px nothing scrolls sideways.
Gap: none

### Step A.5.9 — Dr Patel's menu offers nothing unbuilt
Role: Assessor — Dr Mohammed Patel
Route: /
Do: Read the menu.
Expect: Home and Activity inbox, then My data rights. There is no Recent activities: the flow 01 pick dropped it, and
  the nav links to no page that is not built (DESIGN.md § The NavMenu).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Acting as Assessor"; the menu reads Home and Activity inbox,
  then My data rights. No Recent activities, and no link to an unbuilt page.
Gap: none

### Step A.5.10 — Mr Smit reads what waits for an assessor
Role: Coordinator — Mr Pieter Smit
Route: / → /programme/waiting
Do: Read the dashboard's "Waiting for assessors" card, then open Waiting for assessors from its foot.
Expect: The card, in the warning stripe, is badged "3 waiting, 1 overdue", and lists, oldest first, each row a link to
  its activity's page (T297):
  - Dr du Plessis's portfolio review, Awaiting review, Overdue, "With Mohammed Patel", "Waiting 8 days": Dr du Plessis
    withdrew at Step 5.27, and the request still waits (round-3-check, r6);
  - Dr Mahlangu's Mini-CEX of Step A.2.7, encounter `D−2`, Requested, not overdue, "With Fatima Khumalo", "Waiting 6
    days" (aged at Step A.2.8);
  - Dr du Plessis's CBD of Step 3.21, Requested, "With Fatima Khumalo".
  Waiting for assessors lists the same three in that order under "3 waiting, 1 overdue", each with Send a reminder (none
  is sent here). Nothing offers Reassign (C5).
Note: Dr Mahlangu's request from A.2.7 has waited 6 days, under the 7 that make a request overdue, so it is listed
  without Overdue.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Home's "Waiting for assessors" card (detail-card--warning, the
  warning stripe) is badged "3 waiting, 1 overdue", "Oldest first. Overdue once it has waited 7 days. Its assessor is
  emailed after 5.", and lists, each row a link to its activity: "Portfolio and Logbook Review (Paediatrics) ·
  PAED-015 · 2026-10-26, from Pieter du Plessis" (/activities/11), Awaiting review and Overdue, "With Mohammed Patel",
  "Waiting 8 days"; "Mini-CEX (Paediatrics) · PAED-002 · 2026-10-26, from Nomsa Mahlangu" (/activities/27; encounter
  D-2), Requested, no Overdue, "With Fatima Khumalo", "Waiting 6 days"; "Case-Based Discussion (Paediatrics) ·
  PAED-002 · 2026-10-22, from Pieter du Plessis" (/activities/9), Requested, "With Fatima Khumalo", "Waiting less than
  a day". Open Waiting for assessors at its foot opened /programme/waiting (lit, tab "Waiting for assessors ·
  Wombat"): "3 waiting, 1 overdue" over the same three in that order (8 days since 2026-10-19 21:30 SAST; 6 days since
  2026-10-22 00:08; less than a day since 2026-10-27 21:29), each with Send a reminder; none was sent. No Reassign
  anywhere. No mail.
Gap: none

### Step A.5.11 — Dr Botha's menu offers Programme trainees
Role: CommitteeMember — Dr Sarah Botha
Route: /
Do: Read the menu.
Expect: "Acting as Committee member", with "Switch to Assessor" under it, over Home, Programme trainees, Committee
  reviews and Decision panels, then My data rights. There is no Waiting for assessors: chasing work is not a Committee
  member's (T358, Q7).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Acting as Committee member", with "Switch to Assessor" under
  it, over Home, Programme trainees, Committee reviews and Decision panels, then My data rights. No Waiting for
  assessors.
Gap: none

### Step A.5.12 — Dr Mokoena's menu offers no STAR review queue
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the menu.
Expect: Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and
  Entrustment decisions, then My data rights (T358, R2-Menus). The STAR review queue is not restored: Entrustment
  decisions, the programme's register of STARs, takes its place (DESIGN.md § The NavMenu).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Acting as Speciality admin", no switch, over Home, Programme
  trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and Entrustment decisions, then
  My data rights. No STAR review queue.
Gap: none

### Step A.5.13 — devadmin's menu has no System page
Role: Administrator — devadmin
Route: /
Do: Read the menu.
Expect: The grouped menu of Step 1.1, with no System item: the flow 01 pick dropped it, with its placeholder.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): "Acting as Administrator", no switch, over Home; Platform:
  Scheduled jobs, Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users,
  Invitations; Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee
  reviews, Decision panels; then My data rights. No System item.
Gap: none

### Step A.5.14 — Registrar pages Dr Zulu may not read, by address
Role: CommitteeMember — Dr Thandi Zulu
Route: /programme/trainees/{ProfileId:int} → /programme/trainees/{ProfileId:int}
Do: Type the registrar page's address with the trainee profile id of the dev trainee `trainee@wombat.local`, at the Demo
  Institution; then with Dr Ndlovu's, whose account was erased in § A.1.
Expect: Each is flow 01's Page not found, word for word: the h1 "Page not found", the tab "Page not found · Wombat",
  "There is no page at this address.", "Check the address, or start again from Home." and Go to Home, with no trail and
  nothing in the menu lit. Neither page confirms that the profile exists (T358, C2; CLAUDE.md: 404, not 403), and
  nothing of either registrar is read.
Note: The ids: `SELECT p."Id" FROM "TraineeProfiles" p JOIN "AspNetUsers" u ON u."Id" = p."UserId" WHERE u."Email" =
  'trainee@wombat.local';` and, for the erased account's profile, which no account holds, `SELECT p."Id" FROM
  "TraineeProfiles" p LEFT JOIN "AspNetUsers" u ON u."Id" = p."UserId" WHERE u."Id" IS NULL;`. On a host without the dev
  accounts, play the erased half only. The corpus holds no earlier profile of a current registrar, so a profile that is
  not the registrar's current one is covered by `GetProgrammeTraineeQueryTests` instead (round-3-check 3).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Ids by the Note's SQL: the dev trainee's profile 1, the erased
  account's profile 6. /programme/trainees/1 and /programme/trainees/6 each stay at the typed address and show flow
  01's page word for word: tab "Page not found · Wombat", the h1 "Page not found", "There is no page at this
  address.", "Check the address, or start again from Home." and Go to Home; no trail, nothing in the menu lit, and
  nothing of either registrar. (Both answer status 200: the page is drawn inside the route, and the Expect does not
  ask for 404.) For contrast, /programme/trainees/3 opens Anele Dlamini's page, Programme trainees lit, with "Export
  Anele Dlamini's portfolio" in its header.
Gap: none

### Step A.5.15 — A reminder Wombat cannot send
Role: Coordinator and Administrator — Mr Pieter Smit and devadmin
Route: /programme/waiting → /admin/users/{UserId} → /programme/waiting → /admin/users/{UserId} → /programme/waiting
Do: On a scratch database (the Note), Mr Smit opens Waiting for assessors and leaves it open. devadmin locks Dr Khumalo
  out on Dr Khumalo's account page (Lock out…, then Lock out Fatima Khumalo in its dialog). Mr Smit, on the list as
  read before the lock, chooses Send a reminder on Dr du Plessis's CBD, then Send the reminder.
  devadmin then reactivates Dr Khumalo (Reactivate user), and Dr Patel's email address is cleared (the Note). Mr Smit,
  on the list as read before, chooses Send a reminder on the portfolio review, then Send the reminder.
Expect: The first answer stands above the table and takes the focus: "Not sent. Fatima Khumalo's account is deactivated,
  so Wombat sends Fatima Khumalo no email. The request still waits." The list is read again, and both of Dr Khumalo's
  rows read "No reminder: Fatima Khumalo's account is deactivated." where the button was (E3).
  The second, in the same place and with the focus: "Not sent. Mohammed Patel has no email address in Wombat. Ask your
  institutional admin to add one.", and the portfolio review's row then reads "No reminder: Mohammed Patel has no email
  address in Wombat.".
  Neither request moves: each keeps its state and its wait, and no "Reminded …" line is recorded. The log gains no
  reminder mail. A reminder is never refused because an assessor opted out of digest emails (E1).
Note: Scratch only: a lock and a cleared address are changes the story does not make (states.md § Three ways a state is
  reached, 3), on a copy of the replay's database as it stands here. No page edits an account's address, so clear Dr
  Patel's after Mr Smit's list has been read: `UPDATE "AspNetUsers" SET "Email" = NULL, "NormalizedEmail" = NULL WHERE
  "Email" = 'patel@kgk.wombat.local';`. The flow 06 boards play the second refusal on Dr Zulu's Mini-CEX, which Step
  3.33 completed; the portfolio review is the request still with a named assessor whose address can be cleared.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Played on a scratch copy: the state dumped as
  scenario-t379-pre-a515-A, restored into wombat_scenario_t379_s515 and started on :5181; wombat_scenario_t379
  untouched; 5181 stopped after. Mr Smit's Waiting for assessors was read first (three rows, each with Send a
  reminder). devadmin locked Dr Khumalo out on her page (Lock out…, whose dialog "Lock out Fatima Khumalo?" named her
  2 waiting requests; Lock out Fatima Khumalo): "Fatima Khumalo is locked out. Any open session ends within a minute."
  took the focus. On the stale list, Send a reminder on du Plessis's CBD opened "Send Fatima Khumalo a reminder?" (one
  email, "Activities awaiting your assessment", "It moves nothing ..."); Send the reminder: "Not sent. Fatima
  Khumalo's account is deactivated, so Wombat sends Fatima Khumalo no email. The request still waits." above the
  table, with the focus; the list read again, both Khumalo rows read "No reminder: Fatima Khumalo's account is
  deactivated." where the button was. devadmin reactivated her (Reactivate user, no dialog: "Fatima Khumalo is
  reactivated and can sign in again." took the focus); Patel's address cleared by the Note's UPDATE. Send a reminder
  on the portfolio review, Send the reminder: "Not sent. Mohammed Patel has no email address in Wombat. Ask your
  institutional admin to add one." in the same place, with the focus; that row then reads "No reminder: Mohammed Patel
  has no email address in Wombat.", and Khumalo's rows offer Send a reminder again. SQL: ActivityReminders still 1
  row, on both the scratch and the replay database; activities 9, 11 and 27 keep their UpdatedOn. The scratch log
  gained no Stub email.
Gap: none (E1, the opt-out, is not exercised: nobody here opted out)

### Step A.5.16 — Mr Smit looks for registrars with nothing filed
Role: Coordinator — Mr Pieter Smit
Route: / → /programme/trainees
Do: On Home, choose Open in Programme trainees under "Nothing filed in 30 days".
Expect: Home's "Nothing filed in 30 days" reads "Every current registrar has filed something in the last 30 days.": the
  two current registrars, Dr Dlamini and Dr Mahlangu, were admitted in this sitting, and a registrar admitted less than
  30 days ago is not listed (E5). Programme trainees opens with Nothing filed in 30 days ticked and Programme trainees lit. Nobody
  matches, so the heading reads "0 of 2 current registrars", and under it "No registrar matches these filters.",
  "Nothing filed in 30 days." and Clear filters (review 33).
  A registrar is listed only once admitted at least 30 days before and with nothing filed since `D−30`: a draft is not
  filed, and a recorded MSF is. The heading then takes the count's own form, singular or plural: "1 of 2 current
  registrars has filed nothing in 30 days", "2 of 2 current registrars have filed nothing in 30 days" (D9).
Note: Dr Molefe has completed, Dr du Plessis has withdrawn and Dr Ndlovu has been erased, so two registrars are current.
  A replay played over more than 30 days lists those with nothing filed since `D−30`, and its heading reads the count's
  form above.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Home's "Nothing filed in 30 days" reads "Every current
  registrar has filed something in the last 30 days." under "Current registrars with nothing filed (a draft is not
  filed) in the last 30 days. A registrar admitted less than 30 days ago is not listed." Open in Programme trainees
  opened /programme/trainees?filed=true, Programme trainees lit, "Nothing filed in 30 days" ticked; the heading reads
  "0 of 2 current registrars", and under it "No registrar matches these filters.", "Nothing filed in 30 days." and
  Clear filters. The singular and plural headings are not reachable on this day (the Note).
Gap: none

## A.6 — Platform operations

### Step A.6.1 — devadmin maintains the institution record
Role: Administrator — devadmin
Route: / → /admin/institutions → /admin/institutions/{Id:int}
Do: Open Institutions from the nav. Read the list, open KGK, set its contact email to
  `paediatrics@kgk.wombat.local`, and save.
Expect: The list holds the Demo Institution and Kgosi Kgari Teaching Hospital. Each row shows its short code, contact
  email, status (Active) and creation time, with an Edit named for it; Create institution heads the page. Saving says
  "Institution saved." The Administrator is offered the Status box and Deactivate.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Institutions from the Organisations group, lit, subtitled
  "Maintain institution records and drill into their speciality structure.", with Create institution in the header.
  Rows: Demo Institution (DEMO, admin@demo.local, Active, 2026-10-27 19:55) and Kgosi Kgari Teaching Hospital (KGK,
  paeds-admin@kgk.wombat.local, Active, 2026-10-27 19:58), each with "Edit <name>". KGK's page, "Edit institution"
  (trail Home > Institutions > Edit institution), offers Name, Short code, Contact email, Status with the Active box
  and its help ("An inactive institution cannot be named on a new invitation. Its people, their records and the
  invitations already sent are kept."), then Cancel, Deactivate and Save. Contact email set to
  paediatrics@kgk.wombat.local and saved: "Institution saved." took the focus. At 390 px nothing scrolls sideways.
Gap: F-A.6.1a, T326 (still: the subtitle promises a speciality drill-down that no row offers)

### Step A.6.2 — devadmin reads the system health card
Role: Administrator — devadmin
Route: /
Do: Read the dashboard's System health and Users across institutions cards.
Expect: Database connection is green. The card's other two lines, Email queue and Last nightly job, are amber whatever
  their state, and each carries a task id: they are stubs, tied neither to the mail queue nor to Scheduled jobs
  (reported). The users card counts registered accounts.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Home, "Administrator · Semester 2, 2026". System health:
  Database connection with status-dot ok, and "Email queue (T012)" and "Last nightly job (T024)" each with status-dot
  warn, although every job last Succeeded. Users across institutions reads "22 registered users"; AspNetUsers holds 25
  rows, 22 of them with a role. Each dot is an empty span with no text or label.
Gap: F-A.6.2a, T327 (still: two stub lines, as the Expect records). F-A.6.2b, T327 (still: the count is of role
  holders, leaving out Dr Molefe, Dr du Plessis and the erased account). F-A.6.2c, T327 (still: each state is a colour
  alone).

### Step A.6.3 — Prof Mbatha edits her own institution
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/institutions/{Id:int} → /
Do: From Home, press Open Kgosi Kgari Teaching Hospital. Set its contact email to `hod.paediatrics@kgk.wombat.local`
  and press Save changes. Read Status, then press Cancel.
Expect: The contact email starts at `paediatrics@kgk.wombat.local`, A.6.1's save. Above the form, focused: "**Kgosi Kgari
  Teaching Hospital saved.** Its contact email is now hod.paediatrics@kgk.wombat.local." (T376, I02). Status reads
  "Active", read out, over "Set by a Wombat administrator.": there is no Active box and no Deactivate, because an
  institution's state is the Administrator's alone (T302). There is no Back link; Cancel leads to `/`, whose Your
  institution card reads Contact email hod.paediatrics@kgk.wombat.local. SQL:
  `SELECT "Id","ContactEmail","IsActive" FROM "Institutions" WHERE "ShortCode"='KGK'` gives
  `2|hod.paediatrics@kgk.wombat.local|t`.
Note: Correcting the contact email is 2 presses and 1 field from Home (Open Kgosi Kgari Teaching Hospital, the email,
  Save changes), at 1280 and at 390.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Home's "Your institution" card (Name, Short code, Contact
  email paediatrics@kgk.wombat.local, Status "Active" over "Set by a Wombat administrator.") ends with "Open Kgosi
  Kgari Teaching Hospital" (/admin/institutions/2), the only way on (Home has no Quick links). It opened tab and
  heading "Kgosi Kgari Teaching Hospital", trail Home > Kgosi Kgari Teaching Hospital, nothing in the menu lit, "Your
  institution's name, short code and contact email." with Name, Short code, Contact email (starting at
  paediatrics@kgk.wombat.local), Status as text ("Active", then "Set by a Wombat administrator."), Cancel and Save
  changes: no box, no Deactivate, no Back link (the trail's Home and Cancel lead to /). Contact email set to
  hod.paediatrics@kgk.wombat.local and Save changes pressed: "Kgosi Kgari Teaching Hospital saved. Its contact email
  is now hod.paediatrics@kgk.wombat.local." (the first sentence bold) took the focus, in #institution-result. Cancel
  led to /, whose Your institution card reads Contact email hod.paediatrics@kgk.wombat.local. SQL gives
  2|hod.paediatrics@kgk.wombat.local|t. At 390 px nothing scrolls sideways and Save changes is 44 px tall.
Gap: none

### Step A.6.4 — Prof Mbatha locks Dr Patel out
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: With Dr Patel signed in on another browser, open his account and read Lockout, under Sign-in and access. Press
  Lock out…, read the dialog and press Keep access. Then press Lock out… again and Lock out Mohammed Patel.
Expect: Before the lock, Lockout reads "Locking out the user prevents sign-in immediately. Until they are reactivated:",
  then only what applies to him, an Assessor (T376, Q17):
  - "They are sent no activity reminders or weekly digests.";
  - "They cannot be named as an activity's assessor.";
  then "Their records stay as they are, and a review or feedback campaign already under way can still be finished.
  Reactivating clears the lockout." (T284), and an outline Lock out…, named "Lock out Mohammed Patel". There is no
  Pending invitations card.
  - **The dialog** (T264, Q5), "Lock out Mohammed Patel?": "Mohammed Patel can no longer sign in, and any open session
    ends within a minute. Until they are reactivated:", the same two lines, then a warning: "**1 request is waiting for
    Mohammed Patel**, since `D−8` hh:mm SAST. It stays with them; it does not move to anyone else, and no reminder is
    sent about it.", over its row "Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting
    since `D−8` hh:mm SAST · overdue" (F6); then the records sentence. Keep access, which has the focus, and Lock out
    Mohammed Patel.
  - **Keep access** closes it and locks nothing; the focus returns to Lock out….
  - **Once locked**, in Lockout, focused: "**Mohammed Patel is locked out.** Any open session ends within a minute." His
    Status reads the Locked out badge, with its lock icon; Lockout reads "This user is currently locked out and cannot
    sign in." and offers Reactivate user. On Users his Status reads Locked out.
Note: Locking out and reactivating is 3 presses (Lock out…, Lock out Mohammed Patel, Reactivate user), at 1280 and at 390.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Dr Patel was first signed in on a second browser, on his
  Activity inbox. Users -> "Manage Mohammed Patel": Status Active. Lockout reads "Locking out the user prevents
  sign-in immediately. Until they are reactivated:", then only "They are sent no activity reminders or weekly
  digests." and "They cannot be named as an activity's assessor.", then "Their records stay as they are, and a review
  or feedback campaign already under way can still be finished. Reactivating clears the lockout." and an outline Lock
  out…, named "Lock out Mohammed Patel"; no Pending invitations card. The dialog "Lock out Mohammed Patel?": "Mohammed
  Patel can no longer sign in, and any open session ends within a minute. Until they are reactivated:", the same two
  lines, a warning "1 request is waiting for Mohammed Patel, since 2026-10-19 21:30 SAST. It stays with them; it does
  not move to anyone else, and no reminder is sent about it." over its row "Pieter du Plessis · Portfolio and Logbook
  Review (Paediatrics) · PAED-015 · waiting since 2026-10-19 21:30 SAST · overdue", then the records sentence, Keep
  access (the focus) and Lock out Mohammed Patel (btn-danger). Tab goes Keep access, Lock out Mohammed Patel. Escape
  closed it and Keep access closed it, locking nothing (Status still Active), each returning the focus to Lock out….
  At 390 px nothing scrolls sideways; the dialog's two buttons are 44 px (36 px at 1280), Lock out Mohammed Patel on
  top (y761) over Keep access (y817); the Remove dialog ("Remove Assessor from Mohammed Patel?", naming the same
  waiting request) has Keep the role first in the tab order and focused, Remove Assessor on top, 44 px each, and
  Escape returned the focus to Remove…. Lock out Mohammed Patel: "Mohammed Patel is locked out. Any open session ends
  within a minute." took the focus, in Lockout; Status reads the "Locked out" badge (badge-declined, with its lock
  icon); Lockout reads "This user is currently locked out and cannot sign in." and offers Reactivate user. Users reads
  his Status "Locked out". Home (Prof Mbatha's), read after the lock, has no waiting line: People reads "14 accounts."
  with Open Users and no locked-out count.
Gap: none

### Step A.6.5 — Dr Patel's session ends, and he cannot sign in
Role: Assessor — Dr Mohammed Patel
Route: /account/session-ended → /account/login
Do: Wait up to a minute on the page he had open, then sign in.
Expect: The tab leaves for the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Signing in is refused: "Invalid email or password." (T287).
Note: A locked account, locked by an administrator or by wrong passwords, is refused in a wrong password's words, so the
  page says nothing about which accounts are locked (T287, in T339).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): The second browser's inbox left 46 s after the lock for
  /account/login?error=SessionEnded&returnUrl=%2Factivities%2Finbox: the information notice "Your session has ended.
  Sign in again." His own password: ?error=Refused, tab "Error: Sign in · Wombat", "Invalid email or password.",
  nothing about a lock.
Gap: none

### Step A.6.6 — Dr Patel cannot be named as an assessor while locked
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/new?type=mini_cex_cpsa
Do: Start a Mini-CEX (Paediatrics) and open its assessor list. Then leave without saving.
Expect: KGK's other assessors are listed (Dr Zulu, Dr Naidoo, Dr Botha and Dr Khumalo), but not Dr Patel, and never
  herself (T102). Nothing is saved.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Log an activity, Mini-CEX (Paediatrics)
  (/activities/new?type=mini_cex_cpsa): the Assessor list offers Sarah Botha, Fatima Khumalo, David Naidoo and Thandi
  Zulu (each with the address), not Dr Patel and not herself. Left by Home without saving; nothing was written.
Gap: none

### Step A.6.7 — Prof Mbatha reactivates Dr Patel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users/{UserId}
Do: Press Reactivate user.
Expect: One press, no dialog (Q5). In Lockout, focused: "Mohammed Patel is reactivated and can sign in again." His Status
  reads Active, and Lockout lists his two lines again with Lock out…. Roles and scope lists Assessor with Remove….
Actual (2026-10-28, T379 replay, wombat_scenario_t379): His page reloaded with Status "Locked out". Its Assessor
  training section (button "Edit training": he holds a record, In training) opens a form of four radios and
  Qualifications; Save training, values unchanged: "Training saved for Mohammed Patel: In training." took the focus.
  Reactivate user, no dialog: "Mohammed Patel is reactivated and can sign in again." took the focus, in Lockout, and
  replaced the training result (one result at a time); Status reads Active, Lockout lists his two lines again with
  Lock out…, and Roles and scope lists Assessor with Remove…. Then, to play one result at a time both ways: Lock out
  Mohammed Patel again ("Mohammed Patel is locked out. ..." took the focus), and Save training over it: the lock
  result was replaced by "Training saved for Mohammed Patel: In training." (focused), one result on the page;
  Reactivate user once more left him Active. These extra presses add two SetUserLockout rows to the audit (four in
  all).
Gap: none

### Step A.6.8 — Dr Patel signs in again
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard. "Waiting for you", in the warning stripe, is badged "1 waiting, 1 overdue"
  and lists Dr du Plessis's Portfolio and Logbook Review, from Pieter du Plessis, Awaiting review with Overdue beside
  it, "Waiting 8 days", as his inbox lists it, "8 days" over "since `D−8` … SAST" (T297, T335, T350). Recent decisions
  lists the three he decided on `D`. Dr Dlamini's assessor list names him again (checked at A.7.1).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Signed in afresh he lands on /, "Assessor · Semester 2, 2026".
  "Waiting for you" (detail-card--warning, left border rgb(154, 84, 0)), badged "1 waiting, 1 overdue", lists
  "Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-10-26", from Pieter du Plessis, Awaiting review and
  Overdue, "Waiting 8 days", with Open Activity inbox; the inbox lists it "8 days" over "since 2026-10-19 21:30 SAST".
  Recent decisions lists Dr Dlamini's Mini-CEX (PAED-012), Dr Molefe's DOPS (PAED-001) and Dr Mahlangu's DOPS
  (PAED-002), each Completed 2026-10-27 (the day before this replay's D, the day Acts 3 to 6 were played), with All
  your decisions.
Gap: none

### Step A.6.9 — devadmin opens his own account
Role: Administrator — devadmin
Route: /admin/users → /admin/users/{UserId}
Do: Find his own account in Users (type "devadmin" in Name or email and press Show) and open it.
Expect: The page's subtitle reads "Your own account: your roles and how you sign in." A note says it is his own
  account, so he cannot change its roles, lockout or password here, and another administrator can change his roles or
  lockout. It links to Change password on his own account. No Remove…, Add a role…, Password or Lockout is offered
  (T278), and no Pending invitations card (T376, Q9). Sign-in and access holds only Institutional sign-in: "No
  institutional sign-in is linked. Demo Administrator signs in with an email and a password." On Users his row reads Institution "Global (no institution)",
  Roles Administrator and Scope "—" (U14).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): Users, searched for "devadmin" (Name or email, Show): "Demo
  Administrator devadmin@wombat.local | Global (no institution) | Administrator | — | Active | Manage" (columns Name,
  Institution, Roles, Scope, Status, Actions). Manage opens "Demo Administrator", subtitle "Your own account: your
  roles and how you sign in.", with the note "This is your own account, so you cannot change its roles, lockout or
  password here. Another administrator can change your roles or lockout. Change your password from your account." and
  its link "Change your password" (/account/change-password). Roles and scope lists Administrator with no Remove… and
  no Add a role…; Sign-in and access holds only Institutional sign-in: "No institutional sign-in is linked. Demo
  Administrator signs in with an email and a password." No Password, Lockout or Pending invitations card, and no
  visible button.
Gap: none

### Step A.6.10 — The health endpoint
Role: Anonymous — the uptime monitor
Route: /health
Do: Request `/health` without signing in.
Expect: Status 200 with the body "Healthy". The check includes the database (T097), and the body stays terse because
  the endpoint is public.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): A request without cookies: HTTP 200, text/plain, body
  "Healthy", Cache-Control "no-store, no-cache", with the CSP and nosniff headers.
Gap: none

## A.7 — Narrow viewports and accessibility

At 390 × 844 px, every page meets the rules of DESIGN.md § Layout grid and § Table system:
- The phone bar shows the brand, "Acting as" and the role (none without a role), and Menu. Menu opens the menu over the
  page: the role head and its switch, the list with 44px rows, and at its foot the person's name and Sign out. Close
  folds it, and following a link folds it too.
- A page under a list shows its trail as one 44px link back to its parent.
- The page keeps a 16px gutter and never scrolls sideways; a wide table scrolls inside its own container.
- Cards and dashboard tiles stack in one column, and forms stack their fields.
- Every button and link is at least 24px tall (T086).

Each step below names only what is particular to its pages.

### Step A.7.1 — Dr Dlamini files a Mini-CEX with the keyboard alone
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: At desktop width, using only Tab, Shift+Tab, the arrow keys, Space and Enter, file a Mini-CEX (Paediatrics):
  - choose the type in the picker and press Submit with the form empty;
  - on the draft that opens, choose an EPA the list offers, Dr Patel as assessor, the encounter on `D−20`, the setting
    Ward, a presenting problem and the complexity Moderate;
  - then submit it.
Expect: Focus follows the page's order. On the picker, Tab reaches Mini-CEX (Paediatrics) under Rated by an assessor,
  and Enter opens its form. There the order is the seed's: EPA, Assessor, Date observed, Clinical setting, Presenting
  problem, Case complexity, then Submit and Save draft. Each control is labelled, and says it is required. Its help text
  is read with it (T193).
  - The focus ring is visible on every control, and never on the page heading (T048).
  - Only EPAs whose tool list names the Mini-CEX are offered (T122).
  - The empty Submit keeps the activity as a draft and opens it: the summary "Saved as a draft, but not submitted.
    Fix the 6 fields below and submit again." takes the focus, each of its lines a link to its field. The fields it
    names are marked (T127, T263, C8), and the focus is never left on the page body (T234).
  - The button reads Submit until Dr Patel is named, then Submit to Mohammed Patel.
  - Typing `D−20` announces the late-filing warning ("This encounter was 20 days ago. It can still be filed, …") from
    its live region (D15, T160), and on the draft the line above the actions adds "Filed today, 20 days after the
    encounter: it will be recorded as late."
  - Submitted, it reads "Submitted. It is now Requested. It is in Mohammed Patel's Activity inbox.", and its history
    records the filing as 20 days after the encounter.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 1280 px, keyboard only. Tab to Log an activity in the menu,
  Enter: the picker, the focus on the h1 "Log an activity" with no ring. Tab walks the "Rated by an assessor" links
  (Case-Based Discussion ... DOPS, then Mini-CEX (Paediatrics)), each with the 2px rgb(45,108,223) ring; Enter opened
  /activities/new?type=mini_cex_cpsa, the focus again on the h1, no ring. Tab: "Choose another type", then EPA,
  Assessor, Date observed (three segments and Chrome's picker), Clinical setting, Presenting problem, Case complexity,
  Submit, Save draft. Each control is required, its label "<label> *"; Assessor's and Date observed's help is in
  aria-describedby; every control shows the ring. The EPA list offers 9 (PAED-001 to 004, 006 to 008, 012, 013). The
  empty Submit opened activity 28 as a Draft: "Saved as a draft, but not submitted. Fix the 6 fields below and submit
  again." took the focus, each of its six lines a link to its field (#epa_id-in ...); the six fields are aria-invalid
  and name it. On the draft, by keyboard: PAED-002; the button read Submit until Dr Patel was named, then "Submit to
  Mohammed Patel"; 2026-10-08 typed put "This encounter was 20 days ago. It can still be filed, but a filing more than
  14 days after the encounter is recorded as late." in its status region (also on a fresh form); Ward, a presenting
  problem, Moderate; the line above the actions ends "Filed today, 20 days after the encounter: it will be recorded as
  late." Submit to Mohammed Patel: "Submitted. It is now Requested. It is in Mohammed Patel's Activity inbox." took
  the focus; history "Submit Draft -> Requested ... Filed 20 days after the encounter". No mail.
Gap: none

### Step A.7.2 — Dr Patel completes it on his phone
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: At 390 px, open Dr Dlamini's Mini-CEX from Activity inbox. Rate the supervision at `3b`, write the three
  feedback fields, and complete it.
Expect: Home's "Waiting for you" is badged "2 waiting, 1 overdue". The inbox's rows stack, nothing scrolling sideways:
  the review first, then "Mini-CEX (Paediatrics) · PAED-002 · `D−20`", from Anele Dlamini, each row its own link named
  for it, with no Open button (T350). On the activity, her request is read-only to him and folded: "Request", "Filled in
  by Anele Dlamini, `D`" and Show, reading Hide while open. Entrustment comes first, the rung picker's six radios
  abreast, then the three feedback fields, all fitting the width. Complete, Discard changes and Decline stack, each 44
  px tall, reachable without scrolling sideways, and About sits under them. The history folds into "All N moves", the
  same moves as stacked blocks (A7). Once completed, the result reads "Completed. 1 more waits for you.", with the
  review's row, Open the next and Back to Activity inbox; the completed page folds nothing, and it is read-only to both
  of them. Nobody is emailed (Step 3.3).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar: Wombat, "Acting as Assessor", Menu. Menu
  opens Home, Activity inbox and My data rights (44 px rows), and at its foot "Mohammed Patel" and Sign out; Close
  folds it, and following Activity inbox folds it too. Home: "Waiting for you" badged "2 waiting, 1 overdue" (du
  Plessis's review, Overdue, 8 days; Dr Dlamini's Mini-CEX, less than a day). The inbox: nothing scrolls sideways (390
  in 390); its rows stack as grid blocks (17 to 373), the review first, then "Mini-CEX (Paediatrics) · PAED-002 ·
  2026-10-08, from Anele Dlamini", each row one link named for it, no Open button; the pager's Previous and Next are
  44 px. On activity 28 the trail is one 44 px "Activity inbox" link; Who has it now reads "Your move. Anele Dlamini
  asked you on 2026-10-28 00:27 SAST." Her request is read-only and folded: "Request", "Filled in by Anele Dlamini,
  2026-10-28" and Show, reading Hide while open (the fields inside fit, 49 to 341). Entrustment comes next: six rung
  radios abreast (1, 2, 3a, 3b, 4, 5, 33 to 357), "What each rung means" folded; then the three feedback fields (33 to
  357). Complete, Discard changes and Decline stack full width (16 to 374), each 44 px; About sits under them. History
  folds into "All 2 moves", opened as stacked blocks. Rated 3b, wrote the three feedback fields, Complete: "Completed.
  1 more waits for you." took the focus, then the review's row (Overdue, "Waiting 8 days, since 2026-10-19 21:30
  SAST."), Open the next and Back to Activity inbox (16 to 374, 44 px each); the completed page reads "Done. You
  completed it on 2026-10-28 00:28 SAST. Rated 3b. Credited 1 item to PAED-002.", folds nothing but the history (open,
  "All 3 moves"), with no field or move left (0 enabled controls; Dr Dlamini's view of it is read-only too). Nothing
  scrolls sideways. No mail.
Gap: none

### Step A.7.3 — Dr Dlamini on her phone
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /portfolio/progress/{EpaId:int} → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress and one EPA's page from its index, My activities and My data
  rights from the menu.
Expect: The dashboard's cards stack, each Furthest short row's figure under its name. On My progress, This period, the
  index and the Entrustment panel stack: each EPA's link is a 44 px block, every cell but the link labelled by its
  column, and each Latest rating link in the panel is 44 px tall (T328); nothing scrolls sideways. The EPA's page draws
  its chart at 322 px, never scaled, with its table stacked under it (T355, T323). My activities' rows stack, each cell
  but the link and the state labelled by its column (T342), its Credit link 44 px tall, and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar reads "Acting as Trainee"; the menu's rows
  are 44 px, with Anele Dlamini and Sign out at its foot, and following a link folds it. Home's cards stack at 16 to
  374 (Your targets, Needs you, Recent decisions, My authorisations); each Furthest short row's figure sits under its
  name ("PAED-003 ..." y434, "0 of 3 this semester · 3 more by 2026-11-30" y498), each name a 48 px link. My progress:
  This period, the two index tables ("Each semester · 10 EPAs", "Once a year · 6 EPAs") and the Entrustment panel
  stack as grid rows, 356 px in 358, each EPA link a 48 px block (72 px where the name wraps), every other cell
  labelled by its column ("Semester 2, 2026: ", "Committee decides: ", "STAR against training year 3: "; the panel's
  "Year 3 target: ", "STAR decision: " ...); each Latest rating link ("4 · Encounter 2026-10-21 — open this PAED-001
  rating") is 48 px tall; no chart on the page; nothing sideways. PAED-001's page from the index (trail one 44 px "My
  progress" link): Observations, Entrustment, Rating trajectory and Activities on this EPA stack; the chart is drawn
  at 322 px (viewBox 322, labels 12 px), never scaled, in the region "Rating chart for PAED-001" (324 px wide,
  scrollWidth 324 = clientWidth), with "The ratings, oldest first" stacked under it. My activities: nothing needs her;
  All activities (6) stack each row as a 324 px block, "Who has it now:" and "Credit:" labelling their cells, the link
  and the state unlabelled; each Credit link ("1 item to PAED-012, in My progress") is 44 px; the table fits (356 in
  358). My data rights: the cards stack; Your requests (367 px) scrolls in its 358 px container, its Download in
  reach.
Gap: none

### Step A.7.4 — Dr Molefe's record on her phone
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: / → /portfolio/progress
Do: At 390 px, sign in, read Home, and open My progress from the menu.
Expect: Home's card says that she completed her programme on `D` and links the record's four pages, as at Step 5.21
  (T252, T379), fitting the width. The phone bar shows no "Acting as", since she holds no role, and the menu holds
  Home, My progress, Export portfolio and My data rights, then her name and Sign out at its foot (R3-H08). My progress
  shows her read-only record of past periods (T252), its cards fitting the width; her charts are on each EPA's page,
  not on My progress (T355).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px, signed in as Dr Molefe (completed 2026-10-27, the
  day before this replay's D). The bar shows Wombat and Menu, with no "Acting as" (56 px). The menu holds Home, My
  progress, Export portfolio and My data rights (44 px rows), with Lerato Molefe and Sign out at its foot (44 px).
  Home shows one card, "Your training record": "You completed your programme on 2026-10-27." and "Your record is kept,
  read-only: no target applies to you any more.", then the record's four pages as 44 px links with a line each: My
  progress ("Every period of your programme, to training year 4, and your standing against Annexure A."), My
  authorisations ("Your 15 STARs in force, each with its certificate to download."), My activities and Export
  portfolio ("A PDF of your record. Anyone holding it can check it on Verify portfolio export."); the card spans 16 to
  374. My progress has no controls: "Your record of the programme you ended: what your curriculum held you to in each
  period.", the notice "You completed your programme on 2026-10-27. This page is your record of it and is read-only:
  no target applies to you any more. A period your programme ended in before that period's last month has no target,
  and no period after it is listed." and, UNDER it, "Your record:" with the links My authorisations and My activities
  (44 px, 16 to 159 and 183 to 290); her programme card (Started 2023-01-15, Completed 2026-10-27, Training year 4
  when your programme ended) and each EPA's periods listed ("Semester 2, 2026: no target (your programme ended
  part-way through) · 3 recorded" for PAED-001); cards at 16 to 374, the Annexure A panel stacked (356 in 358, rating
  links 48 px, EPA links 44 px); no chart on the page. PAED-001's page draws her chart at 322 px, never scaled, in its
  324 px region, with its table stacked under it ("Exit level 5 · reached"). My activities reads "Read-only: your
  programme ended on 2026-10-27, so nothing new can be filed.", All activities (8) stacked (356 in 358); Export
  portfolio's "Export PDF" is 44 px, from date and to date fields above it ("Your programme started on 2023-01-15.",
  "Your last day in the programme."). At 901, 1000 and 1100 px My activities does not scroll sideways (page =
  viewport; table 593, 692 and 792 px in an equal container), and no word breaks mid-word, but a date in the Activity
  link breaks after its hyphen (2026-10-18 at 901 px, 2026-10-23 at 1000 px). Nothing scrolls sideways at 390 px.
Gap: new (minor): at 901 and 1000 px a date inside My activities' link breaks after its hyphen (2026-10-18 at 901
  px, 2026-10-23 at 1000 px). T311 no longer occurs: Home's card names her completion and its day, and the menu offers
  Export portfolio.

### Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue. On Decisions
  due, set Status to "Every status", press Show, and open By EPA.
Expect: Home's header action, Start an MSF campaign, is a row of its own below the header's rule, 44px tall and the page's
  width, and the cards stack in one column, each row's link a 44px block (T358, R2-Home k6). MSF campaigns stacks its rows
  (T323): Pieter du Plessis's first, then Dr Molefe's, each a block with every field shown under its column's name, each
  action a 44px block; nothing scrolls sideways. The campaign page keeps its gutter (T226), and its invitee table (counts by
  respondent group: the page never lists an address) fits its card. Decisions due's filters stack, with Show and Clear
  filters each 44px tall; By EPA's fold summary is 44px, and opened it reads one block per EPA; each decision is a block, its
  one action a 44px button the block's width; the pager's buttons are 44px (T364). The queue's filters stack above its table.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar reads "Acting as Coordinator" (50 px).
  Home's header action, Start an MSF campaign (/msf/campaigns/new), is a row of its own under the header's rule (y155;
  the header ends with its 2px rule), the page's width (16 to 374) and 44 px (the first read, taken before the card
  loaded, missed it; a second read, after 0.8 s, found it); the cards stack in one track (16 to 374): Waiting for
  assessors (badged "3 waiting, 1 overdue"), Nothing filed in 30 days and Invitations nearing expiry. MSF campaigns:
  the table (356 in 358) stacks a grid block per row: Pieter du Plessis first (Withdrawn, 0 invitations, "—"), then
  Lerato Molefe (Released, 9, 8), each action named for its campaign ("View campaign: the campaign for Lerato Molefe
  (Default MSF, closing 2026-11-10)"; her View report too); nothing scrolls sideways. Campaign 1 keeps its gutter
  (cards 16 to 374); its invitee table counts by respondent group ("Counted by respondent group. This page never lists
  who was invited or which of them responded."; Peer doctor 2/1, Consultant 3/3, Nurse 3/3, Allied health professional
  1/1, All groups 9/8) and fits its card (319 in 321); no "@" on the page. Decisions due (Status set to "Every
  status", Show; the focus went to the list heading "Decisions, page 1 of 2"): Period, Trainee, Status and EPA stack
  (41 to 349, each 44 px), Show 44 px and the page's width, and Clear filters (shown once a filter is off its default)
  44 px under it; "Counts for the 15 EPAs due in 2026 S2" (By EPA) is a fold whose summary is 44 px (17 to 373);
  opened it reads "Hide" and one 330 px block per EPA; each decision is a grid block (30 to 360), its one action
  ("Schedule a review of Anele Dlamini for ...") a 44 px button the block's width (33 to 357); the "review #n" links
  are 27 px at the least; the pager's Previous, Next and "Decisions per page" select are 44 px; Next put the focus on
  "Decisions, page 2 of 2". The queue (Data rights requests): Type and Status stack above the table (623 px, scrolls
  in its container), with Apply filters in the page header above them (y 141, the filters 280 and 374). Nothing
  scrolls sideways on any of them.
Gap: F-A.7.5b, T328 (still: Apply filters comes before the filters it applies)

### Step A.7.6 — Dr Zulu on her phone
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee reviews and Dr Molefe's final review.
Expect: Home's Registrars and Targets by EPA cards are one column: each registrar's name a 44px block with the training
  year and the figures under it, each EPA's name a 44px block with its cadence and its "n of 2" under it (T358, round 3
  item 21). Committee reviews stacks, a block per review, as at Step A.7.8. The review's cards stack. The evidence
  tables scroll inside their containers. Each EPA's trajectory chart is a card of its own (its section is not a card
  around it), drawn at 322 px, never scaled, its rungs and months 12 px, and fits its named region, "Rating chart for PAED-001" and the like (E1), with its table stacked under it. Nothing
  scrolls sideways (T166, T355).
  Review #7, Dr Molefe's pre-graduation review, ratified (T361, round 3): the sitting bar under the title, static
  below 641 px; the decision above the agenda; the agenda as stacked lines (a container query on its container, below
  760 px), each keeping its table roles and each cell labelled by its column; "Lerato Molefe's progress" under the
  title a 44 px target (round 3 item 20; a line's "Rating chart for …" and the chart's "Back to …'s line", the other
  links item 20 sizes, are drawn only while a line is open, so the Note checks them with the review in progress);
  the charts at 322 px. Nothing scrolls sideways. Checked at 768 px as well: the agenda still stacks, and nothing
  scrolls sideways.
Note: The board R2-Steps also draws 44 px row actions and dialogs at the foot of the screen at 390: those belong to a
  review in progress, which review #7 no longer is. states.md's "Narrow, in progress" row checks them at Steps
  4.16–4.19 and 4.24–4.25, with the chart links.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar's "Acting as Committee member" wraps and
  the bar grows to 59 px; the menu's head adds "Switch to Assessor" (44 px rows). Home: Registrars ("2 registrars")
  and Targets by EPA are one column (16 to 374), each registrar's name a 44 px block with the training year ("Training
  year 1") and figures under it, each EPA's name a 48 px block with its cadence and "0 of 2" under it. Committee
  reviews: the table (356 px in 358) stacks a grid block per review. Review #7 (Dr Molefe's pre-graduation review),
  trail one 44 px link: under the h1, "Lerato Molefe's progress" is a 44 px target (16 to 166); the sitting bar under
  it is static (position static at 390 and 768), "Ratified / Graduate (programme complete) · 13 STARs issued"; the
  Decision card (y960) above the Agenda (y1206). The agenda is stacked lines (display block, 306 px in 308) that keep
  their roles (table, row, rowheader, cell), each cell but the EPA labelled ("Window: ", "State: ", "Evidence: "). The
  cards stack at 16 to 374; the snapshot's evidence tables stack as grid rows (306 in 308), the curriculum-by-semester
  table (784 px) scrolls in its container. Rating trajectory: each chart drawn at 322 px (viewBox 322, 12 px text),
  never scaled, in its region "Rating chart for PAED-001" (and 010, 012), 324 px with scrollWidth 324 = clientWidth.
  Nothing scrolls sideways. At 768 px the agenda still stacks (display block, roles kept), the sitting bar is static
  under the title (282 to 744), nothing scrolls sideways (768). "Lerato Molefe's progress" leads, after graduation, to
  /programme/trainees/2: "Lerato Molefe", "Training year 4 · Programme completed 2026-10-27 · ...", the header action
  "Export Lerato Molefe's portfolio" and "Lerato Molefe completed the programme on 2026-10-27, part-way through
  Semester 2, 2026. No target applies after that; what follows is the record as it stood then, read-only." No link on
  the review page is under 24 px at 390 or 768.
Gap: none

### Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open the dashboard, then Decision panels, Decisions due and Entrustment decisions from the menu.
Expect: Home stacks Waiting for assessors, badged "3 waiting, 1 overdue", Registrars, badged "2 registrars", and Targets
  by EPA, KGK-001 first among the EPAs neither registrar has met, by code (T358). Decision panels' list and Who
  decides each EPA stack, a block per panel and per row of EPAs (T364), and Decisions due reads as at Step A.7.5.
  Entrustment decisions is in the menu, lit on its page, with no trail: it is a list the menu offers (R2-Menus m5).
  It is captioned "Issued STARs (entrustment decisions) · 18 shown" (M10), and its eighteen rows stack, each a block:
  the trainee, then EPA, Level, Issued with its "review #n" link (44 px), Expires, Status and its actions (44 px). The
  revoked and superseded rows' lines wrap under them, never cut (C22). Nothing scrolls sideways.
Note: Until T358 no menu item led to Entrustment decisions for a speciality admin, and the page lit nothing, under the
  trail Home › Entrustment decisions.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar reads "Acting as" over "Speciality admin"
  (50 px). Home stacks, in one track at 16 to 374, Waiting for assessors (the warning stripe, "3 waiting, 1 overdue":
  du Plessis's review Overdue 8 days, Mahlangu's Mini-CEX 6 days, du Plessis's CBD), Registrars ("2 registrars",
  Mahlangu then Dlamini, each name a 44 px block) and Targets by EPA (KGK-001 first, then PAED-002, PAED-003,
  PAED-004, by code, each "0 of 2"). The menu: Home, Programme trainees, Waiting for assessors, Decisions due,
  Committee reviews, Decision panels, Entrustment decisions, My data rights, then her name and Sign out (44 px rows).
  Decision panels: the list and "Who decides each EPA" (three tables) stack as grid blocks, 356 px in 358. Decisions
  due reads as at A.7.5 (filters stacked at 44 px, the By EPA fold 44 px, the pager 44 px, Next to "Decisions, page 2
  of 2"). Entrustment decisions from the menu: lit on its page, no trail; the caption reads "Issued STARs (entrustment
  decisions) · 18 shown"; the table (356 px in 358) stacks 18 blocks, each action (Download, Revoke; 34 in all) at
  least 44 px; the revoked row's line ("Revoked 2026-10-27 at 23:06 SAST by Refilwe Mokoena: Staged on evidence about
  PAED-001, not PAED-002; to be decided again.") wraps under its row and the superseded one ("Superseded by STAR #18,
  issued 2026-10-27.") sits under its own, neither cut. Nothing scrolls sideways (390 in 390).
Gap: none

### Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due
Do: At 390 px, open his dashboard, then Committee reviews and Decisions due from the menu.
Expect: As for Dr Mokoena, scoped to Dr Sithole's sub-speciality: Home stacks the same three cards. The folded bar reads
  "Acting as" over "Sub-speciality admin", in two lines: the bar grows, and the role is never cut. Dr Sithole's menu
  reads as Dr Mokoena's (A.5.12), each row 44px. Committee reviews: Schedule a review is a 44px row of its own under the
  header, and each review a block (the trainee, then Panel, Period over its window, Type, State, Decision, and Open
  44px). Decisions due reads as at Step A.7.5.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The folded bar reads "Acting as" over
  "Sub-speciality admin" on two lines, and the bar grows to 59 px; nothing is cut. Home stacks the same three cards as
  Dr Mokoena's (Waiting for assessors "3 waiting, 1 overdue", Registrars "2 registrars", Targets by EPA from KGK-001),
  each 16 to 374. His menu reads as hers, each row 44 px. Committee reviews: the table (356 px in 358) stacks a grid
  block per review (the first trainee deleted_user_0ae1d64b); Decisions due reads as at A.7.5 (filters 44 px, the
  pager 44 px, Next to "Decisions, page 2 of 2"). Nothing scrolls sideways.
Gap: none

### Step A.7.8a — Programme trainees, a registrar's page and Waiting for assessors on a phone and a tablet
Role: CommitteeMember and Coordinator — Dr Thandi Zulu and Mr Pieter Smit
Route: /programme/trainees → /programme/trainees/{ProfileId:int} → /programme/waiting → /
Do: At 390 px, Dr Zulu opens Programme trainees from the menu, then Dr Mahlangu's page; Mr Smit opens Waiting for
  assessors from the menu. Then each opens the same pages, and Home, at 768 px.
Expect: At 390 px both lists' tables stack, a block per row: each registrar's name and each activity's link a 44px
  block, every other cell under its column's name. The filter bar's fields stack, and Show and Clear filters are each
  44px tall and the row's width; so is each Send a reminder (round 3 items 22, 25). On Dr Mahlangu's page the sections
  stack, the trail is one 44px link back to Programme trainees, the header's "Export Nomsa Mahlangu's portfolio" is a
  44px row (T379), Dr Mahlangu's Mini-CEX of Step A.2.7 is listed under
  Waiting for assessors with no Send a reminder for Dr Zulu, and each trajectory chart is drawn at 322 px, never scaled,
  in its own named region.
  At 768 px Home's Registrars and Targets by EPA rows are one column for Dr Zulu, each figure under its name, and Mr
  Smit's cards stack in one column (round 3 item 21; the grid has one track at 900 px and below). Nothing scrolls
  sideways at either width.
Note: The flow 06 boards number this step A.7.9; Prof Mbatha's phone step already holds that number, so it is A.7.8a,
  after the speciality admins' (as Step 6.14a was placed).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. Dr Zulu's Programme trainees (from the menu): the
  filter fields stack (41 to 349), Show is 44 px and the row's width, and with a filter set (training year 1) Clear
  filters (a link) is too (41 to 349, 44 px); the table stacks a grid block per row (17 to 373), each name a 44 px
  link, every other cell under its column's name ("Training year:", "This semester:", "In 2026:", "Furthest short:",
  "Last filed:"). Dr Mahlangu's page (/programme/trainees/5): the trail is one 44 px "Programme trainees" link; the
  header's "Export Nomsa Mahlangu's portfolio" (/portfolio/export/<her id>) is a row of its own, 16 to 374 and 44 px
  (T379); the sections stack at 16 to 374; Waiting for assessors lists her Mini-CEX (PAED-002, 2026-10-26, "With
  Fatima Khumalo", "Waiting 6 days") with no Send a reminder (0 on the page); her two charts are drawn at 322 px,
  never scaled, in the regions "Rating chart for PAED-002" and "... PAED-004" (324 px, no scroll). Mr Smit's Waiting
  for assessors: the two filters stack, Show 44 px at 41 to 349; each row a grid block ("With:", "State:",
  "Waiting:"), the activity link a block 33 to 357, and each Send a reminder 44 px and the row's width (33 to 357). At
  768 px: both lists stack a grid block per row in a 460 px container (283 to 743), no sideways scroll, the export
  link 36 px; Dr Zulu's Home holds Registrars and Targets by EPA in one 462 px track, each figure under its name
  ("Training year 1" under "Nomsa Mahlangu"; "0 of 2" under KGK-001); Mr Smit's cards stack in one track. Nothing
  scrolls sideways at either width.
Gap: none

### Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, read Home, then open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK
  Teaching Session Log in the builder.
Expect: Nothing scrolls sideways on any of them (T323).
  - **Home's** three cards stack; People's counts and each card's footer link are 44 px tall, and a long contact address
    wraps (M7).
  - **Users** stacks: the filter's fields one under another, Show and Clear full width; People a block per person, the
    name over the address, then Roles, Scope, Status and Manage, 44 px (T376; until T376 the table scrolled 807 px
    in its box).
  - **Dr Dlamini's account** stacks its account line and sections: Roles and scope reads "Trainee" and "System-managed"
    apart (F-2.28b), with Add a role…; Sign-in and access's Password, Institutional sign-in and Lockout one under another,
    the password field's Show toggle 44 px tall, its buttons wrapping rather than overflowing. Her Lockout lists two
    lines: the trainee line and the reminders line. There is no Back to users.
  - **Trainees:** Current trainees is stacked, a block per row with its Edit 44 px, and does not scroll sideways (T373,
    S16); Completed and closed is stacked too, a block per ended programme, the latest end first, with its View 44 px
    (T379, T323).
  - **Dr Dlamini's profile**, "Anele Dlamini's programme", has no Back to trainees and offers "Export Anele Dlamini's
    portfolio" in its header (T379). It stacks three fields plus the last day and two triggers: Curriculum, Programme
    start date and Expected completion date, then End this programme's last day with Mark complete… and Deactivate….
  - **The builder's** editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
    stays usable.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px: no sideways scroll on Home, Users, the user page,
  Trainees, the profile or the builder (390 in 390 on each). The bar reads "Acting as Institutional admin"; the menu's
  rows are 44 px (Home, Invitations, Trainees, Assessors, Users, Curriculum adoptions, Curricula, EPAs, Activity
  types, Entrustment scales, Decisions due, Committee reviews, Decision panels, Entrustment decisions, SSO mappings,
  Audit log, My data rights, then her name and Sign out). Home: the three cards stack at 16 to 374; Invite a person,
  each People count (58 px tall, two to a row, Trainee on a row of its own) and each card's footer link (Open Users,
  Open Curriculum adoptions, Open Kgosi Kgari Teaching Hospital) are 44 px or more; the contact address breaks after
  its @ ("hod.paediatrics@" over "kgk.wombat.local"). Users: the filter's Name or email, Role and Status stack (41 to
  349, 44 px), Show and Clear full width, 44 px; People stacks a grid block per person (17 to 373, 356 in 358): the
  name over the address, then "Roles:", "Scope:", "Status:" and Manage (44 px, named for the person); no table
  scrolls. Dr Dlamini's account (trail one 44 px "Users" link; no Back to users): the account line and sections stack
  at 16 to 374; Roles and scope reads "Trainee" and "System-managed" on lines of their own (y555, y587), with Add a
  role… (41 to 179, 44 px) and "Trainee is not offered: ..."; Sign-in and access has Password (the field, the Show
  toggle 44 px at 263 to 349, the six rules, Reset password 44 px and the field's width), Institutional sign-in and
  Lockout one under another; her Lockout lists two lines (the trainee line, "They are not a current trainee. ...", and
  the reminders line), with Lock out… 44 px. Trainees ("Admit registrars who are waiting, and keep current trainees'
  programmes."): "Waiting to be admitted" holds "Nobody is waiting to be admitted."; "Current trainees, 2 current" is
  stacked, a grid block per row (17 to 373, 356 in 358) with each cell under its column's name ("Programme start:",
  "Expected completion:"), and "Edit Anele Dlamini's profile" 44 px; "Completed and closed, 3 ended" is stacked too, a
  grid block per ended programme (the latest end first: deleted_user_0ae1d64b, Lerato Molefe, Pieter du Plessis), with
  "Programme:" and "Outcome:" under their names and "View <name>'s record" 44 px (T379, T323). Her profile
  (/admin/trainees/edit?id=3), titled "Anele Dlamini's programme", has no Back to trainees (the trail is one 44 px
  "Trainees" link) and offers "Export Anele Dlamini's portfolio" in its header, 16 to 374 and 44 px (T379). It stacks
  Curriculum, Programme start date and Expected completion date (41 to 349, 44 px), Cancel and Save profile, then its
  own "End this programme" section: "The last committee decision: Satisfactory Progress, Annual progression review,
  ratified 2026-10-27.", Last day in the programme (a date, today by default), "A graduation: archives the profile,
  removes the Trainee role and sends Anele Dlamini a graduation email." over Mark complete… (44 px) and "A withdrawal:
  archives the profile and removes the Trainee role. No email is sent." over Deactivate… (44 px). With the last day
  cleared, Mark complete… was refused on the press, before any dialog: "Choose the last day." under the field, which
  took the focus and is aria-invalid, naming it. With today as the last day, Mark complete… opened "Mark Anele
  Dlamini's programme complete?" ("Record 2026-10-28 as Anele Dlamini's graduation day? ... a graduation email is sent
  to dlamini@kgk.wombat.local. ... The day cannot be changed afterwards, and this cannot be undone."): Cancel, the
  safe button, first in the tab order and focused, Mark complete (btn-primary) on top of it on screen (y574 over
  y630), 44 px each, no sideways scroll; Escape closed it and returned the focus to Mark complete…; Deactivate… opened
  "Deactivate Anele Dlamini's profile?" the same way (Cancel first and focused, Deactivate btn-danger on top, y562
  over y618, 44 px). Nothing was confirmed: her programme is unchanged. The builder (/admin/activity-types/23, "Edit
  KGK Teaching Session Log"): the editor and Live preview stack (the preview at 49 to 341, y 4331); the tab bar wraps
  Credit to a second row, each tab 42 px, plain buttons with no role="tab", aria-selected or aria-pressed.
Gap: F-A.7.9a, T280 (still: the builder's tabs expose no selected state).

### Step A.7.10 — Dr Kruger on his phone
Role: CollegeAdmin — Dr Anton Kruger
Route: / → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: At 390 px, open EPAs, then Curricula and the items of Paediatric EPA Curriculum 11.2.
Expect: The EPA list and the items table scroll inside their containers (DESIGN.md § Table system). Each item's row
  actions stay reachable, and the item editor's inputs keep usable widths (T198, T226).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar reads "Acting as College admin"; Home shows
  the National catalogue card; the menu reads Home, Specialities, EPAs, Curricula, Activity types, My data rights (44
  px rows). EPAs: 16 rows, the table (736 px) scrolling in its container. Curricula: 3 rows (Neonatology EPA
  Curriculum 1.0, Paediatric EPA Curriculum 11.2 and 11.1; 885 px, in its container). The items of 11.2
  (/admin/curricula/4/items): 16 rows, the table (772 px) in a 308 px container; the row actions are named "Edit
  PAED-001" and "Remove PAED-001". Edit PAED-001 opens the editor inside the table as a colspan=9 row 772 px wide: its
  inputs are 358 px (the EPA 740), so each half is reached by scrolling the table sideways. Cancel closed it; nothing
  was saved. The page never scrolls sideways.
Gap: F-A.7.10a, T323 (still: the item editor sits in the table's scroll container and must be scrolled sideways)

### Step A.7.11 — devadmin on his phone
Role: Administrator — devadmin
Route: / → /admin/jobs → /admin/jobs/runs → /admin/institutions
Do: At 390 px, open Scheduled jobs, Run history and Institutions.
Expect: The jobs table scrolls inside its container, with each Run now and toggle reachable. The run-history filters
  stack.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px. The bar reads "Acting as Administrator"; the menu's
  rows are 44 px. Scheduled jobs: the table (655 px) scrolls in its container; each row's "Disable <key>" (28 px) and
  "Run now: <key>" (47 px) are in reach. Run history: the trail is one 44 px "Scheduled jobs" link; Key, Status, From
  and To stack (41 to 349), Filter under them (y713), the table (693 px) under that, scrolling in its container.
  Institutions: 2 rows (KGK with hod.paediatrics@kgk.wombat.local, Active); the table (681 px) scrolls in its
  container, each Edit in reach. Nothing scrolls sideways.
Gap: none

### Step A.7.12 — The anonymous pages on a phone
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's second portfolio PDF, the
  file from Step 5.23, by its hash.
Expect: The sign-in card fits the width. Every field, toggle, button and link on the sign-in and forgot pages is
  44 px tall; Sign in is full width; the h1 is 1.5rem. The verify page's answer for Dr Molefe's newest export (Step
  5.23) fits the width (T379, R3-V07 at 390): "Export verified." over its details in one column, First generated
  `D` hh:mm SAST, the Trainee and First exported by ids wrapping in the answer, the hash kept on one line in its field
  (a text field scrolls; it does not wrap).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 390 px, signed out. Sign-in: the card spans 41 to 349;
  Email, Password, its Show toggle, Sign in and "Forgotten your password?" are each 44 px tall; Remember me's box is
  20 px in its row; Sign in is full width (41 to 349); the h1 is 24 px (1.5rem). Forgotten password: the h1 24 px,
  Back to sign in 44 px. Verify ("Check whether a PDF was produced by Wombat, without signing in."; the field and
  Verify are each 44 px, one text input line, 292 px wide): an empty Verify is stopped by the browser; "abc123" is
  refused under the field (aria-invalid, the focus on it): "This is not a SHA-256 hash: it has 6 characters, and a
  SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f)."; 64 zeros answers "No matching export found. This hash
  does not correspond to any PDF generated by this system. One changed byte changes the whole hash: check that it was
  computed from the file you were sent.", which takes the focus. Dr Molefe's second export (Step 5.23,
  portfolio-9dd929531b7f.pdf), its hash typed in UPPER CASE with a space after every 8 digits (the field keeps it on
  one line and scrolls, 649 in 290, rather than wrapping): ?hash=...&check=1, "Export verified. Wombat generated this
  PDF: its content is byte for byte an export Wombat recorded." over one column of details (68 to 357): First
  generated 2026-10-27 23:26 SAST, Exported "Once", Trainee and First exported by (both her user id, each on one line
  in the 289 px column, free to break anywhere), Filter 2023-01-15 to 2026-10-27, File portfolio-9dd929531b7f.pdf, and
  "People are shown by their Wombat user ids, never by name. ..."; the answer took the focus. The first export's hash
  (lower case with one space) answers Exported "3 times, the last on 2026-10-27 23:22 SAST" (First generated
  2026-10-27 23:20 SAST, portfolio-9ac3aea9a795.pdf). Nothing scrolls sideways (390 in 390).
Gap: none (T313 and T287's verify item no longer occur: an upper-case hash with spaces verifies and the answer carries
  the SAST times and the export count)

### Step A.7.13 — Dr Zulu hears a populated review with a screen reader
Role: CommitteeMember — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: With NVDA (or Narrator) on, open Dr Molefe's final review (review #7). Walk it by headings, then by tables, then by
  regions.
Expect: The h1, "Lerato Molefe · `<P>`", is announced on arrival. The sections are h2s, in the page's order: "The
  sitting" (visually hidden), "Before the committee decides", "Decision" (above the agenda, the review being ratified),
  "Agenda", "Entrustment against Annexure A", "Multi-source feedback by EPA", "Evidence snapshot", "Rating trajectory by
  EPA" and "Appeals" (T361, T280). Below them, h3s: "Sampling concentration warnings", the decision's "Graduate
  (programme complete)", each EPA group of the snapshot and "Not about a single EPA", and each EPA's chart.
  - Every section is a region named by its heading.
  - Each trajectory chart is announced by its EPA, and a table follows it that a screen reader reads, with one row per
    observation (T085).
  - The agenda and the evidence tables announce their column headers (stacked lines keep their table roles), and every
    row action is named for its row (T239).
  - Every badge is read as its words.
  - Nothing announces itself on arrival: on this page only a result, the decision form's quorum line and Refresh's
    status are live (round 3 item 21), and a ratified review shows none of them. Nothing is dismissible, and no
    control is unnamed (no "×", T280).
Actual (2026-10-28, T379 replay, wombat_scenario_t379): No screen reader can be driven here, so the accessibility tree
  and DOM of review 7 were read in its place, at 1280 px, opened from Committee reviews. The focus arrives on the h1
  "Lerato Molefe · 2026 S2". The h2s, in order: "The sitting" (visually hidden), "Before the committee decides",
  "Decision", "Agenda", "Entrustment against Annexure A", "Multi-source feedback by EPA", "Evidence snapshot", "Rating
  trajectory by EPA", "Appeals"; each is a section named by its h2. h3s: Sampling concentration warnings, "Graduate
  (programme complete)", each snapshot EPA group (PAED-001, PAED-010, PAED-012) and "Not about a single EPA", and each
  EPA's chart. Each chart sits in a region named for its EPA, "Rating chart for PAED-001" (010, 012), as role=img
  "Chart of the 3 ratings in the table below." (1, 2), followed by "The ratings, oldest first", one row per
  observation (3, 1, 2). All 9 tables have column headers and a row header per row (the agenda "13 EPAs for 2026 S2":
  4 columns, 13 row headers); every row action is named for its row. Badges are plain words (Ratified, Decided, At or
  above). The live regions are an empty status, an empty polite region and the hidden error bar, so nothing announces
  on arrival; no dismiss button is visible and no control is unnamed (0 unnamed).
Gap: none

### Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on Committee reviews (each state a badge since T364, Scheduled grey there and on Decisions due)
    and on a committee review opened from it (Dr Molefe's review 7);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (the dialog's Lock out Mohammed Patel: open Lock out… on Dr Patel's page, read it,
    then Keep access; T376, S22) and success (Publish in the builder, not pressed; with no draft it is disabled, and a
    disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
Note: Since 2026-09-27 (T335, flow 01; T322), `Design/ContrastTests` computes every pair from app.css's tokens, and
  DESIGN.md § Design tokens lists them. The badges and the alerts are body text on their tints (11.2:1 or more), with
  the state's colour on the edge. White (`--on-fill`) is 5.95:1 on the danger button and 5.88:1 on the success one. The
  focus ring is 4.61:1 on the page background, and an input's border 3.60:1 there. The nav's controls take
  `--nav-focus-ring`, 14.07:1 on the gradient's start.
Actual (2026-10-28, T379 replay, wombat_scenario_t379): At 1280 px, WCAG ratios computed from the rendered colours (no
  axe in this browser). Sign-in card: #333 on white 12.63; the tagline #686F77 5.09; link #0B5CAB 6.70; Sign in, white
  on #2D6CDF, 4.86; input border #7B848D 3.80; the Show toggle pressed (aria-pressed true), white on #2D6CDF, 4.86.
  The nav gradient (#052767 to #3A0647): items #D7D7D7 9.78 and 11.25, group labels 8.00 and 9.21, the current item
  (white on the .32 fill) 5.27 and 6.08. Muted on the page (#F8F9FA) 4.83. Committee reviews' state badges (each state
  a badge; 8 rows): "Withdrawn" #333 on #FFF5F5 11.81 (edge #B8322A 5.95); "Ratified" and "Closed" #333 on #E8F5E9
  11.23 (edge 5.88). No review is Scheduled now (review #8 is withdrawn), so Scheduled's grey (badge-draft) was
  measured where it shows: Decisions due's "As opportunity allows", #333 on #F1F3F5 11.36 (edge 3.80); Decisions due's
  "Decided" 11.23, "Revoked: re-decide" 11.81 (edge 5.95), "Not decided" #333 on #FFF8E1 11.89 (edge #9A5400 5.77).
  Review 7's badges: "Ratified", "Decided" and "At or above" 11.23 (edge 5.88). "Name saved." #333 on #E8F5E9 11.23
  (edge 5.88); "Your password was not changed. The password confirmation does not match." #333 on #FFF5F5 11.81 (edge
  5.95), nothing changed. The dialog's Lock out Mohammed Patel (btn-danger, opened on Dr Patel's page, then Keep
  access), white on #B8322A, 5.95; its Keep access #2D6CDF on white 4.86 (border 4.86); Publish, white on #1A7340,
  5.88 (disabled with no draft); primary 4.86. The focus ring #2D6CDF: 4.86 on white, 4.61 on the page. An invalid
  input's border 5.95.
Gap: none (a Scheduled review's own badge was not on screen: it takes badge-draft, measured above)

## Outcome state

Replay check (2026-10-28, T379 replay, wombat_scenario_t379): match. With <appendix start> = 2026-10-28 00:00:00+02
  (A.1.1's first save was made at 23:59 on 2026-10-27, A.1.2 began 00:01): the four KGK requests (0,5,1) (1,3,1)
  (2,4,1) (4,4,1); one erasure (pseudonym equal to the user name, no email, name or password, no institution, lockout
  infinity) with 0 roles, 0 logins and 0 activities on the original id; the profile inactive since 2026-10-28; reviews
  (false,4) (true,6) (true,7,true); eight jobs enabled, each run once by devadmin and the digest twice, 0 failed
  today; 0 SSO mappings; Dlamini opted out of optional processing only, Patel not locked, Smit not opted out; KGK
  hod.paediatrics@kgk.wombat.local and active; audit Submit 4, Withdraw 1, Approve 2, Reject 1, RunScheduledJobNow 9,
  SetUserLockout 4 (the Expect's 2 and the two of A.6.7's one-result-at-a-time pair), ResetUserPassword two false and
  two true (the step's script was run twice by mistake at A.4.5; the Expect's one of each, plus one more of each; the
  password [REDACTED]); 0 unredacted submits. Also as stated: Mahlangu has draft 26 and requested 27 (encounters D-3
  and D-2); Dlamini's 28 is completed with DaysAfterEncounter 20; ActivityReminders still 1 row (A.5.15 played on the
  scratch copy wombat_scenario_t379_s515 only, now stopped). Dr Khumalo's password was changed once (A.4.2) and Dr du
  Plessis's three times (A.4.5 by Prof Mbatha, twice, A.4.6 by himself). Dr Patel's training was saved twice with its
  values unchanged (A.6.7), and Prof Mbatha saved her name unchanged once (A.7.14). The log: 6 Stub emails since the
  app's restart in Act 6 (the digests three, the two nudges three), 0 before the restart in this log. Snapshot
  recovery/scenario-t379-post-actA.dump; the pre-A.5.15 state is recovery/scenario-t379-pre-a515-A.dump.

The appendix changed shared state as follows:
- Dr Ndlovu's account is erased, and the formative check-in scheduled for him at A.1.10 is withdrawn.
- KGK has four data-rights requests, each in its final state.
- Dr Dlamini has opted out of optional processing.
- Dr Mahlangu has one draft and one waiting Mini-CEX, aged by A.2.8.
- Dr Dlamini has one more completed Mini-CEX, filed 20 days after its encounter.
- Dr Khumalo's and Dr du Plessis's passwords have changed (recorded in `pwd_DO_NOT_COMMIT.txt`).
- KGK's contact email is Prof Mbatha's choice.

Everything else is as the appendix found it: every job enabled, Dr Patel active, Mr Smit opted in to digests, KGK
active, and no SSO mapping. Replace `<appendix start>` with the time noted before A.1.1.

```sql
-- The four KGK requests. Type: 0 Access, 1 Rectification, 2 Export, 4 Erasure.
-- Status: 3 Rejected, 4 Completed, 5 Withdrawn.
SELECT "Type", "Status", count(*)
FROM "DataRightsRequests"
WHERE "InstitutionId" = (SELECT "Id" FROM "Institutions" WHERE "Name" = 'Kgosi Kgari Teaching Hospital')
GROUP BY "Type", "Status" ORDER BY 1, 2;
-- Expect (0, 5, 1), (1, 3, 1), (2, 4, 1), (4, 4, 1).

-- One erasure. The account keeps its id under the pseudonym, and nothing on it names him or signs him in.
SELECT e."Pseudonym" = u."UserName" AS same_pseudonym, u."Email", u."FirstName", u."LastName",
       u."PasswordHash" IS NULL AS no_password, u."InstitutionId", u."LockoutEnd"
FROM "DataRightsErasureRecords" e JOIN "AspNetUsers" u ON u."Id" = e."UserId";
-- Expect one row: true, null, '', '', true, null, and the far-future lockout.
SELECT count(*) FROM "AspNetUserRoles" r JOIN "DataRightsErasureRecords" e ON e."UserId" = r."UserId";   -- 0
SELECT count(*) FROM "AspNetUserLogins" l JOIN "DataRightsErasureRecords" e ON e."UserId" = l."UserId";  -- 0

-- Nothing refers to his original id; his profile ended on the erasure day, under the pseudonym.
SELECT count(*) FROM "Activities" a JOIN "DataRightsErasureRecords" e
  ON a."SubjectUserId" = e."UserId" OR a."CreatedByUserId" = e."UserId";                                -- 0
SELECT p."IsActive", p."DeactivatedOn"
FROM "TraineeProfiles" p JOIN "DataRightsErasureRecords" e ON p."UserId" = e."Pseudonym";              -- false, D

-- His reviews under the pseudonym. State: 4 Ratified, 6 Final (Act 4's closed check-in), 7 Withdrawn.
SELECT r."IsFormative", r."State", r."WithdrawalReason" IS NOT NULL AS has_reason
FROM "CommitteeReviews" r JOIN "DataRightsErasureRecords" e ON r."TraineeUserId" = e."Pseudonym"
ORDER BY r."Id";
-- Act 4's annual review (false, 4) and check-in (true, 6), then A.1.10's check-in (true, 7, true).

-- Every job enabled, each run by hand in the appendix, and none failed today.
SELECT "Key", "IsEnabled" FROM "ScheduledJobDefinitions" ORDER BY "Key";                             -- eight, all true
SELECT r."Key", count(*) FROM "ScheduledJobRuns" r JOIN "AspNetUsers" u ON u."Id" = r."TriggeredBy"
WHERE u."Email" = 'devadmin@wombat.local' AND r."StartedAt" >= '<appendix start>'
GROUP BY r."Key" ORDER BY 1;                                                   -- eight keys; the digest twice
SELECT count(*) FROM "ScheduledJobRuns" WHERE "Status" = 2 AND "StartedAt" >= current_date;            -- 0

SELECT count(*) FROM "SsoGroupRoleMappings";                                                           -- 0

SELECT "Email", "LockoutEnd", "OptOutOfDigestEmails", "OptOutOfOptionalProcessing" FROM "AspNetUsers"
WHERE "Email" IN ('dlamini@kgk.wombat.local', 'patel@kgk.wombat.local', 'smit@kgk.wombat.local') ORDER BY 1;
-- dlamini: opted out of optional processing only; patel: no lockout; smit: not opted out of digests.
SELECT "ContactEmail", "IsActive" FROM "Institutions" WHERE "Name" = 'Kgosi Kgari Teaching Hospital';
-- 'hod.paediatrics@kgk.wombat.local', true

-- The audit trail of the appendix's commands, with the data subjects' own words redacted. Earlier acts ran some of
-- these commands too (Step 3.32 ran a job by hand), so only the appendix's rows are counted.
SELECT "Action", "Success", count(*) FROM "AuditEntries"
WHERE "OccurredAt" >= '<appendix start>'
  AND "Action" IN ('SubmitDataRightsRequestCommand', 'WithdrawDataRightsRequestCommand',
    'ApproveDataRightsRequestCommand', 'RejectDataRightsRequestCommand', 'RunScheduledJobNowCommand',
    'SetUserLockoutCommand', 'ResetUserPasswordCommand')
GROUP BY 1, 2 ORDER BY 1, 2;
-- Submit 4, Withdraw 1, Approve 2, Reject 1, RunScheduledJobNow 10, SetUserLockout 2 (all true);
-- ResetUserPassword one false (the 10-character password) and one true.
SELECT count(*) FROM "AuditEntries"
WHERE "Action" = 'SubmitDataRightsRequestCommand' AND "SummaryJson"::text NOT LIKE '%[REDACTED]%';     -- 0
```

## Handoff

Nothing follows the appendix. Snapshot the database to `recovery/scenario-post-appendix.dump`. That snapshot, with the
screenshots under `design/baseline/appendix/`, is the end state of a full replay (T295). `coverage.md` lists what this
appendix could not play:
- the institutional sign-in against a real identity provider;
- linking an account to it;
- adding and deleting SSO group mappings, and T288's cross-institution mapping, which need a configured provider;
- self-service password reset;
- applying and completing a rectification, which no page offers;
- the error page as a failure reaches it, which it does outside Development only;
- the five features the nav once linked as "Coming soon".

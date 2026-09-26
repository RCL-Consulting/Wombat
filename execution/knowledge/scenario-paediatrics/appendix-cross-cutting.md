# Appendix — cross-cutting

**Scenario date.** None of its own. The appendix is played on the replay day `D`, after Act 6, and every check in it
reads the real clock (README § The clock). In the story it is early 2027: Dr Molefe has graduated, Dr du Plessis has
left the programme, and the other three registrars are still in it.

**Who acts.** devadmin (the Administrator); Prof Mbatha (InstitutionalAdmin); Mr Smit (Coordinator); Dr Mokoena
(SpecialityAdmin); Dr Sithole (SubSpecialityAdmin); Dr Kruger (CollegeAdmin); Dr Zulu and Dr Botha (CommitteeMember and
Assessor); Dr Patel and Dr Khumalo (Assessor); Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu (Trainee); Dr
Molefe (former trainee); and anonymous visitors.

**Why.** Some of Wombat belongs to no act: a person's rights over their own data, the jobs that run on their own, the
institutional sign-in, each person's own account, the pages a wrong address or a wrong role lands on, the platform's own
pages, and whether all of it works on a phone, from a keyboard and through a screen reader. This appendix plays them.

**Starting state.** Everything Act 6 left (`recovery/scenario-post-act6.dump`):
- KGK and its cast, each with the roles the README gives, and no review left open (Act 4, Act 5).
- Dr Molefe is a former trainee: no role, and a trainee record (Act 5).
- Dr du Plessis left the programme in Act 5: his profile ended on that act's `D−2`, and he still holds the Trainee role.
- Dr Dlamini, Dr Mahlangu and Dr Ndlovu are current trainees, Dr Ndlovu on curriculum 11.2 since Act 6, each with the
  activities of Act 3 and the reviews Act 4 ratified.
- Dr Molefe's MSF campaign was released in Act 3, and her portfolio PDF exported and verified in Act 5.
- No data-rights request, no SSO provider and no SSO group mapping exists yet. All nine scheduled jobs are enabled. Only
  the scheduler has run them, but for Act 3's one run of `assessor-pending-nudge` by hand (Step 3.32).

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
Do: Open Data Rights from the nav. Read the page, tick "Opt out of optional processing", and save her preferences.
Expect: The page has three parts: processing preferences, a request form, and "Your requests", which is empty ("No
  requests"). The digest opt-out's help names the three reminders it stops (the weekly coordinator digest, the 14-day
  draft reminder and the 5-day assessment reminder), and says that email about one particular thing is still sent
  (T240). Saving says "Processing preferences saved." and the focus moves to that message (T234).
Note: Nothing in the product reads the optional-processing flag (reported).
Actual (2026-09-26, T295 replay, wombat_scenario): Data Rights opened from the nav: three parts, Processing preferences,
  Submit a request, and Your requests reading "No requests / You have not submitted any data rights requests yet." The
  digest help names the weekly coordinator digest, the 14-day draft reminder and the 5-day assessment reminder, and says
  email about one particular thing is still sent. Ticked the optional-processing opt-out and saved: "Processing
  preferences saved." (a status region, which took the focus); AspNetUsers.OptOutOfOptionalProcessing is now true for
  her.
Gap: [F-A.1.1a, T318] The Note's unread flag is confirmed from code, since the replay cannot observe the absence of a reader.
  OptOutOfOptionalProcessing is referenced only by its own read and write path (GetObjectionFlags, UpdateObjectionFlags,
  ObjectionFlagService), DataRights.razor and ErasureExecutor. So the page offers a preference that changes nothing.

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
Actual (2026-09-26, T295 replay, wombat_scenario): Four types offered (Access, Export, Rectification, Erasure). Export
  with no reason: alert "Please provide a reason for your request.", no row written. With the reason: "Request
  submitted. You will be notified when it is reviewed."; Your requests lists 2026-09-26 13:14, Export, Submitted, with
  "Withdraw the Export request made 2026-09-26 13:14" and no Download (request 01a0ddda-…, KGK). No mail reached the
  sink.
Gap: [F-A.1.2a, T319] The confirmation promises a notification that nothing sends. This confirms the Note; no mail followed
  the decision at A.1.3 either. [F-A.1.2b, T318] The refusal alert showed beside A.1.1's stale "Processing preferences saved."
  status. [F-A.1.2c, T325] The request's time prints in UTC with no zone: 13:14 at 15:14 SAST, raw RequestedOn in
  DataRights.razor:108. So between 22:00 and 24:00 UTC it names the previous day.

### Step A.1.3 — Mr Smit approves the export from KGK's queue
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Open Data Rights Requests from the nav. Narrow it to type Export and status Submitted, apply the filters, and
  review Dr Dlamini's request. Press Approve with no decision note. Then approve it with the note "Identity confirmed;
  export released to the data subject."
Expect: The queue holds only KGK's requests (T112), and the filters change it only when they are applied.
  - The row names the requester by the address she signs in with, and its Review action is named for the row (T239).
  - The detail page shows the requester, her user id, when it was submitted, its type, its status and her reason.
  - Approving with no note is refused: "A decision note is required."
  - Once approved, the request reads Completed at once, because an export is built when it is downloaded. A Decision
    card appears (decided by, as a user id; decided on; the note; completed on), and Approve and Reject are gone.
Note: A Coordinator decides only their own institution's requests (T112). Which role should hold this power is still
  open (T112 § Still open).
Actual (2026-09-26, T295 replay, wombat_scenario): The queue held only Dlamini's KGK request. Choosing Erasure changed
  nothing until Apply filters, then showed "No requests". Export + Submitted listed the row (her address, Export,
  Submitted badge, "Review the Export request from dlamini@kgk.wombat.local, 2026-09-26 13:14"). The detail showed
  requester, user id, submitted, type, status and reason. Approve with no note: "A decision note is required." With the
  note: "Request approved." (took the focus), Status Completed, Decision card (Decided by 59291d05-… = Smit's id,
  decided on, note, completed on 13:16:32), no Approve/Reject. No mail.
Gap: [F-A.1.3b, T324] The queue's Status filter prints the raw enum "UnderReview". Its Type filter offers "Objection", which
  no one can submit (the form offers four types). [F-A.1.3c, T323] At 1280 px the Request card is about 315 px wide and its
  values spill past its right edge (address, user id). Once the Decision card sits beside it, they are clipped
  ("dlamini@kgk.wombat.loc", "225e1615-c4fb-4852-b056"; the same on A.1.12's erasure). "Decided by" as a user id is
  deliberate: DESIGN.md § Page shapes keeps ids on the data-rights pages.

### Step A.1.4 — Mr Smit cannot download what he approved
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights/download/{id:guid}
Do: Open the download address of the request he has just approved. Its id is in the detail page's address.
Expect: Page not found, with status 404. Nothing on the page says that the request exists. The reviewer approves; only
  the data subject, or a global Administrator, collects the bundle (T112).
Actual (2026-09-26, T295 replay, wombat_scenario): The download address of request 01a0ddda-… answered HTTP 404: "Page
  not found / The page you asked for does not exist or has moved. / Nothing to show / Check the address or return to the
  dashboard.", with Back to home. Nothing on the page names the request.
Gap: none

### Step A.1.5 — Dr Dlamini downloads her export
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights → /account/data-rights/download/{id:guid}
Do: Reload Data Rights and download the completed request.
Expect: The row reads Completed and offers a Download named for the request.
  - The browser saves `data-export-<date>-<time>.zip`.
  - It holds `data-export.json`: schema version 2, with her profile, every activity she is the subject or author of,
    her committee reviews, the MSF campaigns about her, her progress by semester, her newest 500 audit entries and her
    portfolio exports.
  - It also holds `portfolio-summary.pdf`.
  - No email is sent.
Note: The audit section stops at 500 entries without saying so (T277).
Actual (2026-09-26, T295 replay, wombat_scenario): The row reads 2026-09-26 13:14, Export, Completed, with "Download the
  export of the Export request made 2026-09-26 13:14". The download answered 200 application/zip, attachment
  data-export-20260926-131859.zip (139,560 bytes). It held data-export.json (schemaVersion 2: profile, 5 activities, 1
  committee review, 0 MSF campaigns, 3 progress rows, 34 audit entries, 0 portfolio exports; each count matches the
  database) and portfolio-summary.pdf (144 KB). No mail.
Gap: none. The browser's download manager extension (IDM) intercepted the click, which answered 204 in the page. So the
  zip was fetched with the same session through Playwright's request context. This is an environment artefact, not the
  app.

### Step A.1.6 — Dr du Plessis asks for access, then withdraws the request
Role: Trainee — Dr Pieter du Plessis
Route: /account/data-rights
Do: Submit an Access request with the reason "Everything held about me." Then withdraw it from "Your requests".
Expect: The request lists as Submitted, with a Withdraw named for its type and time. Once withdrawn, the page says
  "Request withdrawn.", the focus moves to that message, and the row reads Withdrawn and offers nothing.
Actual (2026-09-26, T295 replay, wombat_scenario): Access with the reason: "Request submitted. You will be notified when
  it is reviewed."; the row 2026-09-26 13:20, Access, Submitted, with "Withdraw the Access request made 2026-09-26
  13:20". Withdraw acted at once (no dialog): "Request withdrawn." took the focus; the row reads Withdrawn and its
  Actions cell is empty.
Gap: none

### Step A.1.7 — Dr Mahlangu asks for a correction
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Submit a Rectification request with the reason "My programme start is recorded as 15 January 2026; I started work
  on 19 January."
Expect: The request lists as Submitted, with a Withdraw.
Actual (2026-09-26, T295 replay, wombat_scenario): Rectification with the reason: "Request submitted. You will be
  notified when it is reviewed."; the row 2026-09-26 13:22, Rectification, Submitted, with "Withdraw the Rectification
  request made 2026-09-26 13:22".
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
Actual (2026-09-26, T295 replay, wombat_scenario): After both filters were set to All and applied, the queue listed
  three KGK requests, newest first. Mahlangu, Rectification, badge-submitted; du Plessis, Access, Withdrawn
  (badge-draft); Dlamini, Export, Completed. The withdrawn request's page shows the request (Status Withdrawn) with no
  Action card and no decision. Mahlangu's, rejected with the note: "Request rejected." took the focus. Status Rejected,
  Decision card (decided by Smit's id, 13:23:15, the note), no buttons left. No mail.
Gap: none. The Note's stranded Approved rectification is confirmed only from code: ApplyRectification and
  CompleteRectificationRequest have no caller outside their own files and ApproveDataRightsRequest.cs. It is not played,
  since the step rejects.

### Step A.1.9 — Dr Mahlangu reads the decision
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Reload Data Rights and read her request.
Expect: The request reads Rejected and offers nothing: no Withdraw, no Download. The page shows neither the decision
  note nor who decided, and no email told her of the decision.
Note: The confirmation at A.1.7 promised a notification (reported at A.1.2).
Actual (2026-09-26, T295 replay, wombat_scenario): Your requests lists 2026-09-26 13:22, Rectification, Rejected, with
  nothing in Actions (no Withdraw, no Download). The page shows neither the decision note nor who decided. The mail sink
  still holds 90 files, so nothing was sent.
Gap: [F-A.1.9a, T319] The Expect is met, but a rejected requester is never told why: the note is not on her page and no mail
  carries it. The confirmation at A.1.7 promised a notification (F-A.1.2a). This is unsure as a defect: the runbook
  records it as expected, and no decision says a requester sees the note.

### Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period holding `D`, Scheduled on `D`,
  with Formative only ticked. Note the review's id from its address.
Expect: The review opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled, as at Step
  4.49. It is open, so the erasure at A.1.12 has a review to end.
Actual (2026-09-26, T295 replay, wombat_scenario): The trainee picker offered Dlamini, Mahlangu and Ndlovu. Paed Annual
  Review Panel, Sipho Ndlovu, 2026 S2 (the default), Scheduled on 2026-09-26, Formative only ticked: the preview read "A
  formative review carries no agenda: it decides no EPA." Create review opened review #8: Type Annual progression
  review, Mode "Formative (interim check-in, no binding decision)", State Scheduled, Start review offered. His standing
  counts 17 EPAs on 11.2.
Gap: none (the Type "Annual progression review" on a check-in is F-4.49a)

### Step A.1.11 — Dr Ndlovu requests erasure
Role: Trainee — Dr Sipho Ndlovu
Route: /account/data-rights
Do: Submit an Erasure request with the reason "I am leaving the programme and want my personal data removed." Stay
  signed in in this browser.
Expect: The request is accepted although a review of him is scheduled: no review's state refuses an erasure request
  (T258). It lists as Submitted, with a Withdraw.
Actual (2026-09-26, T295 replay, wombat_scenario): Erasure with the reason was accepted although review #8 of him is
  Scheduled: "Request submitted. You will be notified when it is reviewed."; the row 2026-09-26 13:26, Erasure,
  Submitted, with "Withdraw the Erasure request made 2026-09-26 13:26". His tab was left open on Data Rights.
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
Actual (2026-09-26, T295 replay, wombat_scenario): Played in a second page of the same browser, so Ndlovu's circuit
  stayed open. The Administrator's queue listed all four requests (KGK's are the only ones); filtered to Erasure it held
  Ndlovu's. Approve with the note, with no dialog in between: "Request approved.", Status Completed, and a Decision card
  (devadmin's id, 13:27:34, the note, completed 13:27:34). The account became deleted_user_622063bc, with no email and a
  lockout of infinity. An erasure record lists the audit_log, committee_decision and ratified_assessment_record
  categories as retained. Review #8 is Withdrawn. The queue and request still read ndlovu@kgk.wombat.local.
Gap: [F-A.1.12a, T276] After the erasure, the request keeps RequesterDisplayName = the erased person's email. So the
  Administrator's and KGK's queues still name him by address, and the suspect "Erasure keeps the data-rights request's
  RequesterDisplayName" is confirmed. The step expects this, citing ErasureExecutor. Unsure: a compliance record may
  need it, but it is the one place the erased address survives. Approving an irreversible erasure asks no confirmation
  (already noted on T264).

### Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Please sign in again."
  (T279). Signing in is refused in the words an unknown address gets, "Invalid email or password.", with no mention
  of an erasure (T156). No email is sent to him.
Actual (2026-09-26, T295 replay, wombat_scenario): His tab, left on Data Rights, went by itself to
  /account/login?error=SessionEnded&returnUrl=%2Faccount%2Fdata-rights 58 s after the approval (13:27:34 to 13:28:32
  UTC). The page read "Your session has ended. Please sign in again." Signing in with his old address and password:
  ?error=Refused, "Invalid email or password.", with no mention of an erasure. No mail (the sink still holds 90).
Gap: none. The session-ended route is a full load of /account/session-ended. The browser's cookie jar held his own
  cookie, which was put back after the Administrator's page closed.

### Step A.1.14 — What the erasure left, as KGK sees it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or trainees (T026, T258).
  - Committee Reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review and closed check-in as they were, and A.1.10's check-in as Withdrawn.
  - The check-in reads Withdrawn, dated today, with the reason "Withdrawn because the trainee's personal data was erased
    at their request. Nothing more is decided at this review.", and offers no action.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.
Note: T276 (open) lists what an erasure does not yet end: an activity about him still in its workflow can be completed
  and credit the pseudonym, and user ids inside activity data are not rewritten.
Actual (2026-09-26, T295 replay, wombat_scenario): Users lists 14 KGK accounts with no Ndlovu and no pseudonym. Trainees
  shows Dlamini and Mahlangu active and du Plessis and Molefe ended, with no Ndlovu. Committee Reviews lists three
  deleted_user_622063bc rows: Act 4's ratified review (Outcome Deferred), the closed check-in, and A.1.10's check-in as
  Formative, Withdrawn, "None: the review was withdrawn". Review #8 reads State Withdrawn, "2026-09-26. Withdrawn
  because the trainee's personal data was erased at their request. Nothing more is decided at this review.", with no
  action. In SQL, his activities 2–4, one progress row, reviews 5, 6 and 8 and profile 6 (inactive, deactivated
  2026-09-26) all carry the pseudonym.
Gap: none. The withdrawn review still renders the live "Entrustment against Annexure A" card for the pseudonym ("In
  training year 1 on 26 September 2026 … 17 with no decision, of 17 EPAs"). It also shows the sampling-concentration
  warning, although it froze no evidence. This is noted, not a finding.

## A.2 — Scheduled jobs

### Step A.2.1 — devadmin reads the scheduled jobs
Role: Administrator — devadmin
Route: / → /admin/jobs
Do: Open Scheduled Jobs from the nav and read every row.
Expect: Each row shows the job's description and its cron schedule (UTC), with its last run, that run's status badge
  and its next run in the server's local time. Each has Disable and Run now, both named for the job (T239). A job that
  has never run reads "—" for its last run and status. Run history is linked from the header. The nine jobs, by key:
  - activity-draft-nudge
  - assessor-pending-nudge
  - audit-log-retention
  - entrustment-decision-expiry
  - msf-campaign-auto-close
  - msf-invitation-expiry-reminder
  - portfolio-export-cleanup
  - scheduled-job-run-retention
  - weekly-coordinator-digest
Actual (2026-09-26, T295 replay, wombat_scenario): Scheduled Jobs, from the nav, lists the nine jobs by key in the
  runbook's order, each with its description, cron in <code>, last run, a Succeeded status, next run in server local
  time (activity-draft-nudge: 0 7 * * *, next 2026-09-27 09:00), "Disable <key>" and "Run now: <key>". Run history is
  linked from the header. weekly-coordinator-digest has never run: "—" for last run and status, next 2026-09-28 10:00.
  The other daily jobs last ran at 11:03, the scheduler's catch-up at app start; assessor-pending-nudge at 12:43, Step
  3.32's run by hand; msf-campaign-auto-close at 15:00, hourly.
Gap: none

### Step A.2.2 — devadmin disables a job and enables it again
Role: Administrator — devadmin
Route: /admin/jobs
Do: Disable msf-campaign-auto-close, try to run it now, then enable it again.
Expect: Disabling says "Job 'msf-campaign-auto-close' disabled." The row then reads "—" for its next run, its toggle
  says Enable, and its Run now is unavailable. Once enabled again, its next run is the next hour. The scheduler does not
  run a disabled job. The audit log records both changes.
Actual (2026-09-26, T295 replay, wombat_scenario): Disable: "Job 'msf-campaign-auto-close' disabled." took the focus.
  The row reads Next run "—", and its toggle reads "Enable msf-campaign-auto-close". Its Run now is disabled, so a press
  (dispatched in script) does nothing. Enable: "Job 'msf-campaign-auto-close' enabled.", next run 2026-09-26 16:00 (the
  next hour), Run now available again. The audit has DisableScheduledJobCommand and EnableScheduledJobCommand by
  devadmin, both successful. No scheduled run fell in the 27 s it was disabled.
Gap: none

### Step A.2.3 — Mr Smit opts out of digest emails
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."
Actual (2026-09-26, T295 replay, wombat_scenario): Data Rights from the nav, "Opt out of digest emails" ticked and
  saved: "Processing preferences saved."; the box stays ticked.
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
Actual (2026-09-26, T295 replay, wombat_scenario): "Job 'weekly-coordinator-digest' dispatched."; the row's last run is
  2026-09-26 15:33 (local), Succeeded. The log shows "WeeklyCoordinatorDigestJob: digests sent 1; coordinators skipped:
  deactivated 0, opted out of digest emails 1, no email address 0, holds Trainee 0, no institution 0." and "…mail of the
  run started 2026-09-26 13:33:22Z: sent 1, not delivered 0." The one mail went to coordinator@wombat.local ("Hi
  Demo,"); none greets Pieter.
Gap: none. This replay host runs Email:SmtpHost=localhost into an SMTP sink, so mail bodies are .eml files in the sink,
  not "Stub email" log lines. It is the environment, not a runbook error.

### Step A.2.5 — Mr Smit opts back in
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Untick "Opt out of digest emails" and save.
Expect: "Processing preferences saved."
Actual (2026-09-26, T295 replay, wombat_scenario): The digest opt-out opened ticked; unticked and saved: "Processing
  preferences saved.", box now clear.
Gap: none

### Step A.2.6 — devadmin runs the digest again, and Mr Smit gets it
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the log.
Expect: A stub mail "Your weekly Wombat digest" begins "Hi Pieter,". It covers KGK only (T117). With nothing to list
  it says "No items requiring attention this week." Otherwise it lists:
  - under "Trainees at risk", any current KGK trainee who has filed nothing in the last 30 days by the real clock
    (T284): never Dr Molefe or Dr du Plessis, whose programmes have ended, or Dr Ndlovu, who has been erased;
  - any MSF campaign of his waiting on its review;
  - any committee review scheduled in the coming week.
Note: A second log line accounts for the earlier run's mail. On dev, where no mail worker reports, it counts that mail
  as "not reported before the job ran again" (T283).
Actual (2026-09-26, T295 replay, wombat_scenario): "Job 'weekly-coordinator-digest' dispatched." at 15:34, Succeeded.
  The log shows "digests sent 2; coordinators skipped: … opted out of digest emails 0 …" and "mail of the run started
  2026-09-26 13:34:21Z: sent 2, not delivered 0". The sink holds "Your weekly Wombat digest" to smit@kgk.wombat.local,
  "Hi Pieter, … No items requiring attention this week." KGK only: Dlamini and Mahlangu filed today, no MSF campaign
  waits and review #8 is withdrawn. The Demo coordinator's digest was ignored.
Gap: none. The Note's "not reported before the job ran again" line does not appear, because here the SMTP sink reports
  each mail. Each run's own accounting line reads sent n, not delivered 0.

### Step A.2.7 — Dr Mahlangu leaves a draft and a request waiting
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int} → /activities/new → /activities/{ActivityId:int}
Do: File two Mini-CEX (Paediatrics) activities, each with every request field filled, an EPA the list offers, and Dr
  Khumalo as assessor.
  - The first: the encounter on `D−3`, then Save draft.
  - The second: the encounter on `D−2`, then Submit.
Expect: The first reads "Draft saved. It has not been submitted." The second reads "Submitted. It is now Requested.",
  and is in Dr Khumalo's inbox. No email is sent: Wombat mails nobody when an activity moves (Step 3.3).
Actual (2026-09-26, T295 replay, wombat_scenario): Both Mini-CEX (Paediatrics) have PAED-002, Khumalo as assessor, a
  setting, a presenting problem and complexity Moderate. Activity 26 (encounter 2026-09-23), Save draft: "Draft saved.
  It has not been submitted." Activity 27 (encounter 2026-09-24), Submit: "Submitted. It is now Requested." Khumalo's
  inbox lists it: Mini-CEX (Paediatrics), Nomsa Mahlangu, PAED-002, 2026-09-24, Requested, beside du Plessis's CBD
  (activity 9, Requested since 12:26). No mail: the sink holds only the three digests.
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
Actual (2026-09-26, T295 replay, wombat_scenario): Stand-in as the Note prescribes: UpdatedOn − 15 days on activity 26,
  the draft (now 2026-09-11), and − 6 days on activity 27, the request (now 2026-09-20). Both jobs dispatched and
  Succeeded. The sink holds "You have draft activities waiting" to Mahlangu: "Hi Nomsa, … Mini-CEX (Paediatrics) — draft
  for 15 days", and "Activities awaiting your assessment" to Khumalo: "Hi Fatima, … Mini-CEX (Paediatrics) from Nomsa
  Mahlangu — waiting 6 days". Patel was also nudged about du Plessis's Portfolio and Logbook Review (activity 10,
  back-dated in Act 3, waiting 8 days). Logs: "ActivityDraftNudgeJob: trainees reminded 1 (drafts 1); trainees skipped:
  no such account 0, deactivated 0, opted out of digest emails 0, no email address 0." "AssessorPendingNudgeJob:
  assessors nudged 2 (activities 2); nominees skipped: … 0 …". Each has a "mail of the run … sent n, not delivered 0"
  line.
Gap: none

### Step A.2.9 — devadmin runs the other six jobs
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run each of the remaining jobs now, one at a time, and read each one's log line.
Expect: Each run reads Succeeded, and each job logs one line:
  - entrustment-decision-expiry: how many decisions it expired and how many expiring within 30 days it reminded. A
    reminder mail is "Entrustment decision expiring soon: PAED-0nn", sent at most once a day per decision.
  - msf-campaign-auto-close: "no expired campaigns found.", since Act 3's campaign was closed and released.
  - msf-invitation-expiry-reminder: "no respondent is due a reminder."
  - audit-log-retention: "complete. Total archived: 0.", since nothing is two years old.
  - portfolio-export-cleanup: "no expired exports found.", since Act 5's exports are days old.
  - scheduled-job-run-retention: "no old runs to delete."
Note: portfolio-export-cleanup deletes the export records that `/portfolio/verify` checks, so after 90 days a
  graduate's PDF no longer verifies (reported).
Actual (2026-09-26, T295 replay, wombat_scenario): Each of the six, run now one at a time, reads Succeeded (15:37), with
  one log line each: "EntrustmentDecisionExpiryJob: expired 0 decisions (sent 0), reminded 0 expiring soon (sent 0)."
  (the only decision expiring within 30 days, #2, is Superseded); "MsfCampaignAutoCloseJob: no expired campaigns
  found."; "MsfInvitationExpiryReminderJob: no respondent is due a reminder."; "AuditLogRetentionJob: complete. Total
  archived: 0."; "PortfolioExportCleanupJob: no expired exports found." (Act 5's 4 exports are from today);
  "ScheduledJobRunRetentionJob: no old runs to delete." No mail.
Gap: [F-A.2.9a, T313] The Note's defect is confirmed from code, since the replay cannot age an export:
  PortfolioExportCleanupJob removes PortfolioExport rows over 90 days old, and VerifyExportQuery finds a PDF only by
  those rows (ContentHash). So a graduate's PDF stops verifying 90 days after export.

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
Actual (2026-09-26, T295 replay, wombat_scenario): Run history (from the jobs header) lists 23 runs newest first, each
  with key, started and finished (local time), duration, status badge, triggered by, and error. 12 read "scheduler"; the
  11 by hand read 52f88768-253d-4437-ab88-a11e7c6bb42a, devadmin's user id. Typing a key changed nothing until Filter.
  "weekly-coordinator-digest": its two runs by hand (15:33:22, 15:34:21), no other job. "digest": "No runs found".
  Status Failed: "No runs found". From 2026-09-26: all 23 (every run is today's).
Gap: [F-A.2.10a, T324] "Triggered by" prints a raw user id (ScheduledJobRunsList.razor:70, run.TriggeredBy). DESIGN.md § Page
  shapes (T142) names a person by name; only the audit, data-rights and verify pages keep ids. This confirms the suspect
  "Pages identify people by raw user id" for the job runs. Runbook corrected: the Expect had written the id in as
  expected, and now says what DESIGN.md decided. [F-A.2.10b, T327] "digest" matching nothing confirms the suspect that the key
  filter wants a whole key, though its placeholder "Filter by key..." invites a part. Unsure: the Expect records this as
  intended. The silent 200-run cap is not observable with 23 runs.

## A.3 — Institutional sign-in (SSO)

### Step A.3.1 — Prof Mbatha opens SSO Mappings with no provider configured
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/sso/group-mappings
Do: Open SSO Mappings from the nav.
Expect: A card says that no SSO providers are configured, and to add them to the `Sso:Providers` section of the
  application settings and restart. No form for adding a mapping is offered. Current mappings is empty ("No group
  mappings"), although its empty state still says to add a mapping above. Nothing on the page can be changed.
Note: T288 (open, P2): once a provider exists, an InstitutionalAdmin can map another institution's provider groups, and
  the mapper grants those roles. It is checked when a provider is configured.
Actual (2026-09-26, T295 replay, wombat_scenario): SSO Mappings from the nav: "No SSO providers are configured. Add
  providers to the Sso:Providers section in application settings and restart." No add form, no input, no button on the
  page. Current mappings shows "No group mappings / Add a mapping above to link external groups to Wombat roles." At 390
  px there is no horizontal scroll.
Gap: [F-A.3.1a, T326] The empty state says "Add a mapping above" where no form is shown. This confirms the suspect "the SSO
  mappings page's empty state says to 'Add a mapping above'" (GroupMappings.razor EmptyBody). The step expects the stale
  line. The card also tells an InstitutionalAdmin to edit application settings and restart, which only the operator can
  do.

### Step A.3.2 — The sign-in page offers no institutional sign-in
Role: Anonymous — a KGK consultant
Route: /account/login → /account/sso-challenge/{providerKey} → /account/login
Do: Look for an institutional sign-in button. Sign in with a wrong password. Then open the challenge address for a
  provider called `kgk`.
Expect: The page offers only an email, a password, Remember me and the "Forgotten your password?" link. There is no
  "or" divider and no "Sign in with …" button. The wrong password reads "Invalid email or password." and points to no
  institutional button (T156). The challenge for a provider that is not configured returns to the sign-in page, which
  says "Unknown SSO provider."
Actual (2026-09-26, T295 replay, wombat_scenario): Signed out, the sign-in page offers only Email, Password (with Show),
  Remember me, Sign in and "Forgotten your password? Reset it.", with no "or" divider and no "Sign in with …" button. A
  wrong password for zulu@kgk.wombat.local gives ?error=Refused, "Invalid email or password.", naming no institutional
  button. /account/sso-challenge/kgk returns to /account/login?error=SsoUnknownProvider: "Unknown SSO provider."
Gap: none

### Step A.3.3 — The callback and link pages with no institutional sign-in in progress
Role: Anonymous — a KGK consultant
Route: /account/sso-callback → /account/login → /account/link-external
Do: Open the callback address, then the link-your-account page, directly.
Expect: The callback returns to the sign-in page, which says "External login information was not available." The link
  page says "Your institutional sign-in has expired. Start again from the sign-in page." It offers Back to sign in and
  no password field (T149).
Actual (2026-09-26, T295 replay, wombat_scenario): /account/sso-callback returned to
  /account/login?error=ExternalLoginUnavailable: "External login information was not available." /account/link-external
  shows "Link your account" with the alert "Your institutional sign-in has expired. Start again from the sign-in page."
  and only a "Back to sign in" link, no password field. At 390 px there is no horizontal scroll.
Gap: none

## A.4 — Account self-service

### Step A.4.1 — Dr Botha reviews and edits her account
Role: CommitteeMember — Dr Sarah Botha
Route: / → /account/profile
Do: Open My Account from her address in the top row. Clear her last name and save. Then put "Botha" back and save.
Expect: The account summary shows her address, both her roles (CommitteeMember and Assessor) and a Change password
  link. The email field cannot be edited. The cleared last name is refused, with a required-field message beside it,
  and nothing is saved. Restored, the page says "Profile saved." and the focus moves to that message (T234).
Actual (2026-09-26, T295 replay, wombat_scenario): My Account from her address in the top row: the Account summary reads
  Email botha@kgk.wombat.local, Roles "CommitteeMember, Assessor", and a Change password link; the Email field is
  disabled. Last name cleared and saved: a summary list and a message beside the field, both "The LastName field is
  required."; the field is marked invalid, the focus stays on Save profile, and the database still holds Sarah Botha.
  "Botha" restored and saved: "Profile saved.", which takes the focus.
Gap: [F-A.4.1a, T324] The required-field message names the field by its code name, "The LastName field is required." (the
  DataAnnotations default on ProfileFormModel.LastName, Profile.razor:160, with no ErrorMessage), where the label reads
  "Last name". The same default serves FirstName.

### Step A.4.2 — Dr Khumalo changes her password, getting it wrong first
Role: Assessor — Dr Fatima Khumalo
Route: /account/profile → /account/change-password
Do: From My Account's Change password, try four times, and record the password (d) sets in `pwd_DO_NOT_COMMIT.txt`:
  - (a) a wrong current password;
  - (b) the right current password, with a new password and a confirmation that differ;
  - (c) the right current password, with a new one of 8 different lower-case letters;
  - (d) the right current password, with a new one that meets every rule.
Expect: Each refusal reloads the page with empty fields. The tab's title starts "Error:", the refusal takes the focus,
  and each field names the refusal (T265, T193).
  - (a) reads "Incorrect password."
  - (b) reads "The password confirmation does not match."
  - (c) gives one sentence for each rule it breaks: at least 12 characters, a digit, an upper-case letter and a
    symbol. It says nothing of lower-case letters, a rule it keeps.
  - (d) reads "Password updated.", and she stays signed in in this browser.
Note: Before this step, sign Dr Khumalo in on a second browser and leave it on her Activity Inbox. Five wrong current
  passwords in a row would lock the account for 15 minutes ("Too many incorrect passwords, so the account is locked
  for a few minutes. Please try again later.").
Actual (2026-09-26, T295 replay, wombat_scenario): A second browser (a separate Playwright context) was signed in as
  Khumalo on her Activity Inbox first. From My Account's Change password, each refusal came back by a full load with
  every field empty, the tab titled "Error: Change password", the focus on the alert and each field's aria-describedby
  naming it. (a) ?error=PasswordMismatch, "Incorrect password." (b) "The password confirmation does not match." (c)
  "qwertyui": "Passwords must be at least 12 characters. Passwords must have at least one non alphanumeric character.
  Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z')." Nothing on
  lower case. (d) "Password updated.", which takes the focus; she stays signed in here (the inbox loads after the
  minute). Recorded in pwd_DO_NOT_COMMIT.txt (the A.4 password).
Gap: [F-A.4.2a, T286] Unsure: nothing in the audit log records the change or the wrong current password. The endpoint
  (Program.cs, ChangePasswordOutcome.SubmitPath) is not a MediatR command, so it writes no audit row, while sign-in
  writes Login and LoginFailed and an administrator's reset writes ResetUserPasswordCommand (T101). The Expect says
  nothing of an audit row, so this may be deliberate.

### Step A.4.3 — Dr Khumalo's other session ends
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Please sign in
  again." (T279). The old password is refused ("Invalid email or password."). The new one brings her back to her
  Activity Inbox.
Actual (2026-09-26, T295 replay, wombat_scenario): The second browser's inbox left 36 s after the change, through
  /account/session-ended to /account/login?error=SessionEnded&returnUrl=%2Factivities%2Finbox: "Your session has ended.
  Please sign in again." The old password: ?error=Refused, "Invalid email or password." The new one brought her back to
  /activities/inbox (du Plessis's CBD and Mahlangu's Mini-CEX, both Requested).
Gap: none

### Step A.4.4 — Dr du Plessis has forgotten his password
Role: Anonymous — Dr Pieter du Plessis
Route: /account/login → /account/forgot-password → /account/login
Do: Follow "Forgotten your password? Reset it" from the sign-in page.
Expect: A "Reset password" page, saying exactly: "Password reset is not wired yet in the rewrite. Ask an administrator
  to issue a new invitation or reset the account directly." It offers Back to sign in, and nothing else: no field, and
  no email is sent.
Note: This is a stub. Self-service reset is not built, and nothing uses the `PasswordResetEmail` template (not
  played).
Actual (2026-09-26, T295 replay, wombat_scenario): "Forgotten your password? Reset it" led to /account/forgot-password,
  titled "Reset password", reading exactly "Password reset is not wired yet in the rewrite. Ask an administrator to
  issue a new invitation or reset the account directly." with one link, Back to sign in, which returned to
  /account/login. No field; the mail sink stayed at 96 files.
Gap: [F-A.4.4a, T287] This confirms the suspect "The forgot-password stub shows developer wording": a registrar is told that
  reset "is not wired yet in the rewrite" (ForgotPassword.razor:10). The step quotes it as the stub's text; the words,
  not the stub, are the finding.

### Step A.4.5 — Prof Mbatha resets his password
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Set a new password of 10 characters, then one that meets every rule. Record the
  second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: The Reset password card says it sets a password directly and that the user is not emailed. The 10-character
  password is refused with the rules it breaks. The second is accepted, and the field is cleared. The audit log
  records the reset with the password redacted (T101).
Note: The form enables Reset password at 8 characters; Identity requires 12 (reported).
Actual (2026-09-26, T295 replay, wombat_scenario): Users → "Manage Pieter du Plessis". The Reset password card: "Sets a
  new password directly. The user is not emailed; share the password securely out-of-band." The button stays disabled to
  7 characters and enables at 8 (minlength="8"). "shortpass1" (10): the page-top alert, which takes the focus,
  "Passwords must be at least 12 characters.; Passwords must have at least one non alphanumeric character.; Passwords
  must have at least one uppercase ('A'-'Z')."; the field is emptied. A 16-character password that meets every rule:
  "Password reset. Share the new password with the user out-of-band.", the field cleared. The audit holds
  ResetUserPasswordCommand false then true, each with newPassword "[REDACTED]". The password is recorded in
  pwd_DO_NOT_COMMIT.txt.
Gap: [F-A.4.5a, T317] This confirms the suspect "The administrator's Reset password form enables its button at 8 characters":
  Identity wants 12, so a short password is refused only after the press, as the Note reports. [F-A.4.5b, T317] The refusal
  joins Identity's sentences with "; " after their own full stops ("12 characters.; Passwords …"); the change-password
  page joins the same sentences with spaces. [F-A.4.5c, T323] At 1280 px the Account summary card clips his address at its
  right edge ("duplessis@kgk.wombat.l"), the overflow F-A.1.3c saw on the data-rights request.

### Step A.4.6 — Dr du Plessis signs in with it and chooses his own
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → / → /account/profile → /account/change-password
Do: Sign in with the password Prof Mbatha set. Then change it, from My Account, to one of his own. Record it in
  `pwd_DO_NOT_COMMIT.txt`.
Expect: He lands on his Trainee dashboard, which still says that his programme ended (Step 5.28). The change reads
  "Password updated."
Actual (2026-09-26, T295 replay, wombat_scenario): Signed in with the password Prof Mbatha set, he lands on / "Viewing
  as Trainee", whose Curriculum targets card says "Your programme ended on 24 September 2026, so no target applies to
  you any more. Your progress in each period is kept on My progress, read-only." My Account → Change password, from the
  set password to his own: "Password updated.". At 390 px the change-password page has no horizontal scroll (scrollWidth
  390). Recorded in pwd_DO_NOT_COMMIT.txt (the A.4 password).
Gap: none

### Step A.4.7 — Mr Smit signs out through the confirmation page
Role: Coordinator — Mr Pieter Smit
Route: /account/logout-confirm → / → /account/logout-confirm → /account/logout → /account/login → /msf/campaigns → /account/login → /msf/campaigns
Do: Open the sign-out confirmation by its address and press Cancel. Open it again and sign out. Then open MSF
  Campaigns by its address.
Expect: A "Sign out" page, with no nav, says to use its button to end the session. Cancel returns him to his
  dashboard, still signed in. Sign out lands on the sign-in page. MSF Campaigns then asks him to sign in, and after
  signing in he is brought back to it. The audit log records a Logout.
Note: No page links to the confirmation page: the nav's Logout and the top row's Sign out sign out at once.
Actual (2026-09-26, T295 replay, wombat_scenario): /account/logout-confirm, titled "Sign out", with no nav or top row:
  heading "Sign out", "Use the button below to end the current session.", Cancel and Sign out. Cancel returned him to /
  "Viewing as Coordinator", still signed in. At 390 px, no horizontal scroll. Sign out landed on /account/login.
  /msf/campaigns then went to /account/login?ReturnUrl=%2Fmsf%2Fcampaigns, and signing in brought him back to MSF
  Campaigns. The audit holds Logout (smit@kgk.wombat.local, 13:53:55 UTC) and then Login.
Gap: [F-A.4.7a, T317] This confirms the suspect "/account/logout-confirm is linked from nowhere": no page or component refers
  to it (grep), and the nav's Logout and the top row's Sign out post to /account/logout at once, as the Note says.
  Unsure: the Note records it as known, so it may be a page kept on purpose.

## A.5 — System pages

### Step A.5.1 — A registrar opens an administrator's pages
Role: Trainee — Dr Anele Dlamini
Route: /admin/users → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Users page, then that of Scheduled Jobs.
Expect: Each time, the "Access denied" page: she does not have permission to view the page, her role does not allow
  access to the area, and "Back to home" takes her home. Nothing of the page she asked for is shown.
Actual (2026-09-26, T295 replay, wombat_scenario): /admin/users went to /access-denied?ReturnUrl=%2Fadmin%2Fusers,
  titled "Access denied": "You do not have permission to view this page." and "Your current role does not allow access
  to this area. Return to the dashboard or sign in with a different account.", with "Back to home" (/), which took her
  home. /admin/jobs gave the same page. Nothing of either page was shown. At 390 px, no horizontal scroll.
Gap: none

### Step A.5.2 — Prof Mbatha opens the Administrator's own pages
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled Jobs.
Expect: Access denied for both. They are the Administrator's alone, and her nav offers neither.
Actual (2026-09-26, T295 replay, wombat_scenario): /admin/institutions and /admin/jobs each went to /access-denied
  (ReturnUrl kept) with the Access denied page. Her nav lists 17 links (Home to Decisions Due, among them Users, SSO
  Mappings and Audit Log) and neither Institutions nor Scheduled Jobs.
Gap: none

### Step A.5.3 — A forged dashboard switch
Role: Trainee — Dr Anele Dlamini
Route: /dashboard/switch/{role} → /
Do: Open `/dashboard/switch/Administrator`.
Expect: She is back on her own dashboard, viewing it as a Trainee. A switch to a role she does not hold changes
  nothing, and no other view is offered to her.
Actual (2026-09-26, T295 replay, wombat_scenario): /dashboard/switch/Administrator redirected to /, "Welcome,
  dlamini@kgk.wombat.local", "Viewing as Trainee", with her Curriculum targets card; no "Switch view" line or other view
  is offered. The endpoint did write wombat_preferred_dashboard_role=Administrator (it checks only that the name is a
  role, Program.cs:652), and Home ignores a role she does not hold.
Gap: none

### Step A.5.4 — Addresses that do not exist
Role: Trainee — Dr Anele Dlamini
Route: /not-found → /placeholder/{Feature} → /account/login → /not-found
Do: Type the following addresses:
  - `/portfolio/cv`;
  - `/placeholder/reports`;
  - then, signed out, `/portfolio/cv` again.
Expect: For each, the "Page not found" page: the page asked for does not exist or has moved, and "Back to home" is
  offered. The address stays as typed, and the status is 404 (the browser's network panel) (T233).
  - `/placeholder/reports` is Page not found too, not a "Coming soon" (T178).
  - Signed out, the unknown address first asks her to sign in, then shows Page not found.
Actual (2026-09-26, T295 replay, wombat_scenario): Signed in, /portfolio/cv and /placeholder/reports each answered 404
  at the typed address with "Page not found": "The page you asked for does not exist or has moved.", "Nothing to show /
  Check the address or return to the dashboard." and Back to home; /placeholder/reports is not a Coming soon. At 390 px,
  no horizontal scroll. Signed out, /portfolio/cv went to /account/login?ReturnUrl=%2Fportfolio%2Fcv, and signing in
  there gave the same Page not found at /portfolio/cv, 404. (/not-found typed signed out is the page itself, 200,
  captured as the signed-out state.)
Gap: none

### Step A.5.5 — Another institution's records, by id
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not Access denied. The user shows "User unavailable" ("The user could not
  be found or is outside your scope."). Neither page confirms that the record exists (CLAUDE.md: 404, not 403).
Note: The Demo Institution stands in for another institution only here. Its ids come from
  `SELECT "Id" FROM "Institutions" WHERE "Name" = 'Demo Institution'` and from `"AspNetUsers"`. On a host without the
  dev accounts, play the institution half only.
Actual (2026-09-26, T295 replay, wombat_scenario): /admin/institutions/1 (the Demo Institution) moved to /not-found,
  "Page not found", as /admin/institutions/999 does. The dev trainee's /admin/users/2dadddb8-… stays at its address,
  titled "User", with the alert "User unavailable / The user could not be found or is outside your scope." and Back to
  users, the same as an unknown id (00000000-…-0001). Neither reveals that the record exists.
Gap: none

### Step A.5.6 — Another registrar's activity, by id
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Open the address of Dr Mahlangu's submitted Mini-CEX from A.2.7.
Expect: "Activity unavailable": the requested activity could not be loaded. Nothing of it is shown, and nothing on the
  page speaks of permission.
Actual (2026-09-26, T295 replay, wombat_scenario): /activities/27 (Dr Mahlangu's requested Mini-CEX) stays at its
  address, titled "Activity", and shows only "Activity unavailable / The requested activity could not be loaded."
  Nothing of the activity, and no word of permission.
Gap: none

### Step A.5.7 — A data-rights request that is not his, by id
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).
Actual (2026-09-26, T295 replay, wombat_scenario): /admin/data-rights/01a0dde1-8ee6-79a8-ba4b-955f41042d17 (Mahlangu's
  id, last digit 6 changed to 7), titled "Data rights request": the alert "You are not authorized to access this
  data-rights request." above the empty state "Not found / The data rights request was not found.", and Back to list.
  Her real id (…d16) opens her Rectification request. No other institution holds a request, so the other-institution
  half could not be typed (as the previous part noted).
Gap: [F-A.5.7a, not a defect: T112 makes an unknown id and another institution's read alike] Unsure: the page tells him he is "not authorized" for an id that exists nowhere, and then that it was
  "Not found". The two read the same for unknown and foreign ids as T112 decided, so nothing leaks, but the refusal's
  words claim a record he may not see. The Expect quotes them as decided, hence unsure.

### Step A.5.8 — The error page
Role: Trainee — Dr Anele Dlamini
Route: /Error
Do: Type `/Error`.
Expect: "Something went wrong": an unexpected error interrupted the request. If it keeps happening, she is to contact
  her administrator, quoting the request ID the page gives. Signed out, `/Error` asks her to sign in first.
Note: No failure leads here. No exception handler is configured, so an unhandled exception gives the developer page on
  dev, a bare 500 in production, and the "An unhandled error has occurred" banner inside a page (reported; that path is
  not played).
Actual (2026-09-26, T295 replay, wombat_scenario): Signed out, /Error went to /account/login?ReturnUrl=%2FError; signed
  in there, /Error shows "Something went wrong", "An unexpected error interrupted the current request." and the alert
  "If this keeps happening, please contact your administrator." with no request ID on screen. The prerendered HTML
  carries "Request ID: 00-05f81b4e…-00", which the interactive render removes. At 390 px, no horizontal scroll.
Gap: [F-A.5.8a, T321] The request ID is gone by the time the page can be read. Error.razor takes it from Activity.Current or
  the HttpContext, both null once the circuit renders, so the prerender's ID is replaced by nothing, and the page asks
  her to contact an administrator with nothing to quote. The suspect "No exception handler is configured" cannot be
  observed here: no failure leads to this page, as the Note says.

### Step A.5.9 — Dr Patel's Recent Activities
Role: Assessor — Dr Mohammed Patel
Route: /placeholder/{Feature}
Do: Open Recent Activities from the nav.
Expect: A page headed "Recent Activities" says it is not built yet. Its "Coming soon" card says the menu links to it so
  that the role's navigation is complete. Nothing else is offered.
Actual (2026-09-26, T295 replay, wombat_scenario): Recent Activities, from the nav, is /placeholder/recent-activities,
  titled and headed "Recent Activities": "This page is not built yet." and the card "Coming soon / The menu links here
  so that your role's navigation is complete. The page itself is still to be built." No link or button in the page.
Gap: none

### Step A.5.10 — Mr Smit's Stalled Activities
Role: Coordinator — Mr Pieter Smit
Route: / → /placeholder/{Feature}
Do: Read the dashboard's "Stalled requests" card, then open Stalled Activities from the nav.
Expect: Whatever the card lists, no row links anywhere, so there is no page to act on a stalled request from (Step
  3.30). Stalled Activities is the "Coming soon" stub, headed "Stalled Activities".
Note: Dr Mahlangu's request from A.2.7 has waited 6 days, under the card's 7, so it is not listed.
Actual (2026-09-26, T295 replay, wombat_scenario): The Coordinator dashboard's "Stalled requests" card (a warning card)
  lists one row, "Portfolio and Logbook Review (Paediatrics) — Pieter du Plessis, 18 Sept" (activity 10, submitted),
  with no link in the card. Mahlangu's requested Mini-CEX (activity 27, 6 days) is not listed. Stalled Activities from
  the nav is /placeholder/stalled-activities, headed "Stalled Activities", the Coming soon stub.
Gap: none. The suspect "the Stalled requests card matches only activities in a state keyed 'submitted'" is not
  observable here: the only waiting request, activity 27, is under the 7 days either way. The one row listed is the one
  open activity in "submitted", which fits the suspect, and the code (GetCoordinatorDashboardSummaryQuery.cs:44) filters
  on "submitted" alone.

### Step A.5.11 — Dr Botha's Programme Trainees
Role: CommitteeMember — Dr Sarah Botha
Route: /placeholder/{Feature}
Do: Open Programme Trainees from the nav.
Expect: The "Coming soon" stub, headed "Programme Trainees".
Actual (2026-09-26, T295 replay, wombat_scenario): Programme Trainees from the nav is /placeholder/programme-trainees,
  titled and headed "Programme Trainees", with "This page is not built yet." and the Coming soon card.
Gap: none

### Step A.5.12 — Dr Mokoena's Programme Trainees and STAR Review Queue
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /placeholder/{Feature} → /placeholder/{Feature}
Do: Open Programme Trainees, then STAR Review Queue, from the nav.
Expect: Both are the "Coming soon" stub, each under its own heading.
Actual (2026-09-26, T295 replay, wombat_scenario): Programme Trainees (/placeholder/programme-trainees) and STAR Review
  Queue (/placeholder/star-review-queue), both from her nav, are each the Coming soon stub, titled and headed "Programme
  Trainees" and "STAR Review Queue".
Gap: none

### Step A.5.13 — devadmin's System page
Role: Administrator — devadmin
Route: /placeholder/{Feature}
Do: Open System from the nav.
Expect: The "Coming soon" stub, headed "System".
Actual (2026-09-26, T295 replay, wombat_scenario): System from the nav is /placeholder/system, titled and headed
  "System", the Coming soon stub.
Gap: none

## A.6 — Platform operations

### Step A.6.1 — devadmin maintains the institution record
Role: Administrator — devadmin
Route: / → /admin/institutions → /admin/institutions/{Id:int}
Do: Open Institutions from the dashboard's Maintenance card. Read the list, open KGK, set its contact email to
  `paediatrics@kgk.wombat.local`, and save.
Expect: The list holds the Demo Institution and Kgosi Kgari Teaching Hospital. Each row shows its short code, contact
  email, status (Active) and creation time, with an Edit named for it; Create institution heads the page. Saving says
  "Institution saved." The Administrator is offered the Status box and Deactivate.
Actual (2026-09-26, T295 replay, wombat_scenario): Institutions, from the dashboard's Maintenance card, headed "Maintain
  institution records and drill into their speciality structure." with Create institution, lists Demo Institution (DEMO,
  admin@demo.local, Active, 2026-09-26 11:03) and Kgosi Kgari Teaching Hospital (KGK, paeds-admin@kgk.wombat.local,
  Active, 11:13), each with "Edit <name>". KGK's edit page offers Name, Short code, Contact email, the Status box
  (Active, ticked), Cancel, Deactivate and Save. Contact email set to paediatrics@kgk.wombat.local and saved:
  "Institution saved.", which takes the focus. At 390 px, no horizontal scroll.
Gap: [F-A.6.1a, T326] The list's subtitle promises to "drill into their speciality structure", but a row offers only Edit, and
  specialities are the College's, not an institution's (the national catalogue).

### Step A.6.2 — devadmin reads the system health card
Role: Administrator — devadmin
Route: /
Do: Read the dashboard's System health and Users across institutions cards.
Expect: Database connection is green. The card's other two lines, Email queue and Last nightly job, are amber whatever
  their state, and each carries a task id: they are stubs, tied neither to the mail queue nor to Scheduled Jobs
  (reported). The users card counts registered accounts.
Actual (2026-09-26, T295 replay, wombat_scenario): System health lists Database connection with a green dot (status-dot
  ok), and "Email queue (T012)" and "Last nightly job (T024)" each with an amber dot (status-dot warn), although the
  mail sink delivers and every job last Succeeded. Users across institutions reads "23 registered users"; AspNetUsers
  holds 25 rows.
Gap: [F-A.6.2a, T327] This confirms the suspect "Two of the three lines on the Administrator dashboard's System health card
  are stubs": both are hard-coded amber (AdministratorDashboard.razor), labelled with internal task ids, as the Expect
  reports. [F-A.6.2b, T327] Unsure: "registered users" counts accounts that hold a role (GetAdministratorDashboardSummaryQuery
  unions WombatRoles.All), so Dr Molefe, a graduate who still signs in, and the erased account are left out: 23 of 25.
  The Expect's "counts registered accounts" would read 24 or 25. [F-A.6.2c, T327] Each line's state is a coloured dot alone
  (an empty span, no text or label), so a screen reader hears three names and no status (WCAG 1.4.1).

### Step A.6.3 — Prof Mbatha edits her own institution
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /access-denied
Do: Open KGK's page by its address (no nav link leads there). Set its contact email to
  `hod.paediatrics@kgk.wombat.local` and save. Press Deactivate. Untick Active and save. Then press Back to
  institutions.
Expect: The contact email saves ("Institution saved."). Deactivate is refused: "Only global administrators may
  deactivate institutions." Unticking Active is refused as Deactivate is, and KGK stays Active. Back to institutions
  and Cancel lead to the Institutions list, which is the Administrator's alone, so she lands on Access denied
  (reported).
Note: The code lets her save KGK inactive through the Status box (reported). If it does, devadmin ticks Active on KGK
  again before going on, since an inactive KGK can issue no invitations.
Actual (2026-09-26, T295 replay, wombat_scenario): KGK's page by its address. Contact email set to
  hod.paediatrics@kgk.wombat.local and saved: "Institution saved." Deactivate: the alert "Only global administrators may
  deactivate institutions." (DeactivateInstitutionCommand, audited as failed); KGK stays Active. Active unticked and
  saved: "Institution saved.", and the database holds KGK IsActive false. Back to institutions: /admin/institutions
  renders Access denied in place, drawn inside a second copy of the layout (two sidebars, two top rows, two Sign out
  buttons). As the Note directs, devadmin then ticked Active on KGK and saved ("Institution saved."); KGK is Active with
  Prof Mbatha's contact email.
Gap: [F-A.6.3a, T302] This confirms the suspect "An InstitutionalAdmin can deactivate her own institution by unticking Active
  and saving": UpdateInstitutionCommand wrote IsActive = false for her, while Deactivate refuses her. [F-A.6.3b, T291] This
  confirms the suspect "'Back to institutions' and 'Cancel' send an InstitutionalAdmin to a page that refuses her".
  [F-A.6.3c, T321] Access denied reached by an in-app link is nested in a second MainLayout: Routes.razor wraps the
  AccessDenied component in a LayoutView of MainLayout inside AuthorizeRouteView, which already applies the layout. A
  typed address goes to /access-denied and is drawn once (A.5.1), so this shows on every link to a page the role may not
  open.

### Step A.6.4 — Prof Mbatha locks Dr Patel out
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: With Dr Patel signed in on another browser, open his account, read the Lockout card, and lock him out.
Expect: Once locked, his status reads "Locked out" and the card offers Reactivate user. The focus moves to the result.
  No confirmation is asked first (T264). Before the lock, the card lists what it does until he is reactivated, and says
  his records stay as they are (T284):
  - he sits on no committee panel;
  - he is not a current trainee;
  - he is sent no reminders or digests;
  - he cannot be named as an activity's assessor.
Actual (2026-09-26, T295 replay, wombat_scenario): Dr Patel was first signed in on a second browser (a separate context)
  on his Activity Inbox. Users → "Manage Mohammed Patel": Status Active. The Lockout card reads "Locking out the user
  prevents sign-in immediately. Until they are reactivated:" then: no committee panel; not a current trainee (with what
  that leaves out); no activity reminders or weekly digests; cannot be named as an activity's assessor; then "Their
  records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating
  clears the lockout." Lock out user, pressed at 14:04:37 UTC, asked nothing first: "User locked out." takes the focus,
  Status reads "Locked out", and the card offers only Reactivate user.
Gap: none

### Step A.6.5 — Dr Patel's session ends, and he cannot sign in
Role: Assessor — Dr Mohammed Patel
Route: /account/session-ended → /account/login
Do: Wait up to a minute on the page he had open, then sign in.
Expect: The tab leaves for the sign-in page, which says "Your session has ended. Please sign in again." Signing in is
  refused: "Too many failed sign-in attempts. Please try again later or reset your password."
Note: An administrator's lock gets the brute-force lockout's words. Whether a locked-out person is told otherwise was
  left open (T156 review).
Actual (2026-09-26, T295 replay, wombat_scenario): The second browser's inbox left 23 s after the lock, through
  /account/session-ended to /account/login?error=SessionEnded&returnUrl=%2Factivities%2Finbox: "Your session has ended.
  Please sign in again." Signing in with his own password: ?error=LockedOut, "Too many failed sign-in attempts. Please
  try again later or reset your password."
Gap: [F-A.6.5a, T287] This confirms the suspect "An administrator's lock gets the brute-force lockout text": a man locked out
  by his head of department is told he failed too many sign-ins, and pointed to a reset that is the A.4.4 stub. The Note
  records the wording as left open in the T156 review.

### Step A.6.6 — Dr Patel cannot be named as an assessor while locked
Role: Trainee — Dr Anele Dlamini
Route: /activities/new
Do: Start a Mini-CEX (Paediatrics) and open its assessor list. Then leave without saving.
Expect: KGK's other assessors are listed (Dr Zulu, Dr Naidoo, Dr Botha and Dr Khumalo), but not Dr Patel, and never
  herself (T102). Nothing is saved.
Actual (2026-09-26, T295 replay, wombat_scenario): New activity, Mini-CEX (Paediatrics) chosen: the Assessor list offers
  Sarah Botha, Fatima Khumalo, David Naidoo and Thandi Zulu (each with the address), not Dr Patel and not herself. Left
  by Home without saving; Activities still ends at id 27. At 390 px the form stacks with no horizontal scroll.
Gap: none

### Step A.6.7 — Prof Mbatha reactivates Dr Patel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users/{UserId}
Do: Reactivate Dr Patel.
Expect: His status reads Active, the card offers Lock out user again, and the focus moves to the result.
Actual (2026-09-26, T295 replay, wombat_scenario): His page opened with Status "Locked out". Reactivate user, asked
  nothing first: "User reactivated." takes the focus, Status reads Active, and the card offers Lock out user again.
Gap: none

### Step A.6.8 — Dr Patel signs in again
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard, and Dr Dlamini's assessor list names him again (checked at A.7.1).
Actual (2026-09-26, T295 replay, wombat_scenario): On the second browser, still on
  ?error=LockedOut&returnUrl=%2Factivities%2Finbox, his own password signed him in and returned him to his Activity
  Inbox (du Plessis's Portfolio and Logbook Review, Awaiting review). Signed in afresh from /account/login he lands on /
  "Viewing as Assessor". Its Pending requests card reads "0 assessments awaiting review" while his inbox lists that
  review. Dlamini's assessor list is left to A.7.1, as the Expect says.
Gap: [F-A.6.8a, T297] The Assessor dashboard's Pending requests counts only activities in a state keyed "requested"
  (GetAssessorDashboardSummaryQuery.cs:41 and :81). A Portfolio and Logbook Review waits in "submitted" ("Awaiting
  review"), so Dr Patel reads 0 pending with one in his inbox. It is the Coordinator card's suspect (A.5.10) in reverse.

### Step A.6.9 — devadmin opens his own account
Role: Administrator — devadmin
Route: /admin/users → /admin/users/{UserId}
Do: Find his own account in Users and open it.
Expect: A note says it is his own account, so he cannot change its roles, lockout or password here, and another
  administrator can change his roles or lockout. It links to Change password on his own account. No Remove, Add role,
  Reset password or Lockout is offered (T278).
Actual (2026-09-26, T295 replay, wombat_scenario): Users, searched for "devadmin", lists "Demo Administrator,
  devadmin@wombat.local, Global Administrator, Active". Manage opens a page headed "Demo Administrator", "Your own
  account's summary, roles and pending invitations.", with the note "This is your own account, so you cannot change its
  roles, lockout or password here. Another administrator can change your roles or lockout." and a link "Change your
  password" (/account/change-password) "from your account." The cards are Account summary (Global (no institution),
  Active), Roles (Administrator, no Remove, no Add role) and Pending invitations. No Reset password or Lockout card.
Gap: none

### Step A.6.10 — The health endpoint
Role: Anonymous — the uptime monitor
Route: /health
Do: Request `/health` without signing in.
Expect: Status 200 with the body "Healthy". The check includes the database (T097), and the body stays terse because
  the endpoint is public.
Actual (2026-09-26, T295 replay, wombat_scenario): curl without cookies: HTTP 200, text/plain, body "Healthy" (7 bytes),
  no-store, with the CSP and nosniff headers. The check is AddDbContextCheck<ApplicationDbContext>("database")
  (Program.cs:88-89), mapped AllowAnonymous.
Gap: none

## A.7 — Narrow viewports and accessibility

At 390 × 844 px, every page meets the rules of DESIGN.md § Layout grid and § Table system:
- The nav folds behind the menu toggle.
- The page keeps a 16px gutter and never scrolls sideways; a wide table scrolls inside its own container.
- Cards and dashboard tiles stack in one column, and forms stack their fields.
- Every button and link is at least 24px tall (T086).
- The top row's address and Sign out stay reachable.

Each step below names only what is particular to its pages.

### Step A.7.1 — Dr Dlamini files a Mini-CEX with the keyboard alone
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/{ActivityId:int}
Do: At desktop width, using only Tab, Shift+Tab, the arrow keys, Space and Enter, file a Mini-CEX (Paediatrics):
  - choose the type and press Submit with the form empty;
  - on the draft that opens, choose an EPA the list offers, Dr Patel as assessor, the encounter on `D−20`, the setting
    Ward, a presenting problem and the complexity Moderate;
  - then submit it.
Expect: Focus follows the page's order: activity type, the request fields, Save draft, Submit. Each control is
  labelled, and says it is required. Its help text is read with it (T193).
  - The focus ring is visible on every control, and never on the page heading (T048).
  - Only EPAs whose tool list names the Mini-CEX are offered (T122).
  - The empty Submit keeps the activity as a draft and opens it: "Saved as a draft, but not submitted: … Fix the fields
    below and submit again." The fields the refusal names are marked (T127, T263), and the focus is never left on the
    page body (T234).
  - Typing `D−20` announces the late-filing warning ("This encounter was 20 days ago. It can still be filed, …") from
    its live region (D15, T160).
  - Submitted, it reads Requested, and its history records the filing as 20 days after the encounter.
Actual (2026-09-26, T295 replay, wombat_scenario): At 1280 px, keyboard only. /activities/new: focus arrives on the h1
  with no ring; Tab order is Activity type, EPA, Assessor, Date observed (three segments and the picker), Clinical
  setting, Presenting problem, Case complexity, Save draft, Submit. Every label reads "<label>* required"; Assessor's
  and Date observed's help is in aria-describedby; every control shows the 2px #3498db ring. The EPA list offers 9
  (PAED-001 to 004, 006 to 008, 012, 013), exactly the curriculum items whose tools name mini_cex. The empty Submit
  opened activity 28 as a Draft under a role=alert notice "Saved as a draft, but not submitted: EPA: A value is
  required. [...] Case complexity: A value is required. Fix the fields below and submit again."; the six request fields
  are aria-invalid and name it; focus was on BODY. On the draft: PAED-002, Dr Patel (listed again after A.6.7),
  2026-09-06, Ward, a presenting problem, Moderate, then Submit: it reads Requested; its history reads "Submit Draft ->
  Requested ... Filed 20 days after the encounter" (DaysAfterEncounter 20). Focus was on BODY again, and no result
  message was shown. Typing the date on the draft showed no warning. On a fresh /activities/new form (nothing saved),
  the status region read "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after
  the encounter is recorded as late." No mail sent.
Gap: [F-A.7.1a, T299] The focus is left on the page body twice. First, after the empty Submit opens the draft: the h1 renders
  only once the activity loads, so FocusOnNavigate's h1 selector finds nothing. Second, after the successful Submit: the
  pressed button leaves the page on reload, and HandleTransitionAsync moves no focus and shows no result, against T234's
  rule. [F-A.7.1b, T299] On the draft, the late-filing warning never appears for a date that the empty Submit named.
  ActivityForm hides it while the field is in RefusedFieldKeys (T263), so the date was typed and filed 20 days late with
  no warning (D15). Unsure: T263 made the hiding deliberate for a refused date value; whether it should also cover a
  date the refusal only called missing is a product call.

### Step A.7.2 — Dr Patel completes it on his phone
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: At 390 px, open Dr Dlamini's Mini-CEX from the Activity Inbox. Rate the supervision at `3b`, write the three
  feedback fields, and complete it.
Expect: The inbox lists it with an action named for it. On the activity, her request is read-only to him, and the
  six-rung ladder and the feedback fields fit the width. Complete and Decline are reachable without scrolling
  sideways. Once completed, it reads Completed and is read-only to both of them. Nobody is emailed (Step 3.3).
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home (captured) reads 0 in Pending requests although
  activity 28 waits on him (F-A.6.8a again). The inbox lists it first, "Mini-CEX (Paediatrics) / Anele Dlamini /
  PAED-002 / 2026-09-06 / Requested", with a link named "Open Mini-CEX (Paediatrics) for Anele Dlamini, PAED-002,
  encounter date 2026-09-06". The table (722 px) scrolls inside .table-container (356 px), and the page never scrolls
  sideways. On activity 28, the six request fields are read-only to him and the rating and feedback fields are writable.
  The ladder offers 1, 2, 3a, 3b, 4, 5; Complete and Decline sit at x 142 to 334, within the width. There is a 16 px
  gutter; the inputs are 193 px wide, which cuts off the EPA and assessor values. Rated 3b, wrote the three feedback
  fields, and completed it: "State: Completed", every field read-only, and no action but Sign out. History: "Complete
  Requested -> Completed Mohammed Patel ... 1 item". The DB shows completed, overall_level 4 (3b), CreditedItemCount 1.
  The mail sink is still 96 files. The focus was on BODY after Complete (as F-A.7.1a).
Gap: [F-A.7.2a, T328] The top row's account-address link is 21 px tall on every page at 390 px, against this section's rule
  "Every button and link is at least 24px tall (T086)". T086 named this very link and lifted only .btn. Unsure: it
  stands clear of other targets, so WCAG 2.5.8's spacing exception may excuse it. [F-A.7.2b, T323] The activity form sits
  three cards deep (detail card, form card, section fieldset), so at 390 px its inputs are 193 px wide in a 343 px
  column. The chosen EPA ("PAED-002 - Managing...") and assessor ("Mohammed Patel (patel...") are cut off in their
  selects. It fits the width, as the Expect asks, but it wastes half of it.

### Step A.7.3 — Dr Dlamini on her phone
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My Progress, My Activities and Data Rights from the folded nav.
Expect: The dashboard's cards stack. On My Progress, each EPA's figures and trajectory fit the width. The tables of My
  Activities and "Your requests" scroll inside their containers, and each row's action stays reachable.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, the sidebar is a top bar whose menu toggle ("Toggle
  navigation menu", the template's checkbox) opens the ten trainee links, each 48 px tall. The nav folds again after
  each navigation. Home: the six cards stack in one column with a 16 px gutter; Curriculum targets reads "1 / 10
  semester targets met, 0 / 6 yearly". My Progress (from the nav): no sideways scroll; the four trajectory charts are
  293 px wide in a 341 px column, and its one wide table (514 px) scrolls in .table-container. My Activities: 6 rows;
  the table (652 px) scrolls inside its container, and each row's action is named ("View Mini-CEX (Paediatrics),
  PAED-012, encounter date 2026-09-26"). Data Rights: the preferences and request cards stack; "Your requests" (1 row,
  Export, Completed) is 365 px in a 341 px container, and its Download stays reachable. The top row's address and Sign
  out stay in view on every page. Captured the nav folded and open, Home, My Progress, My Activities and Data Rights.
Gap: [F-A.7.3a, T328] On the dashboard's Recent activities card (and Dr Patel's Recent decisions), a status badge stretches
  into a tall pill wherever the link beside it wraps to two lines, as for "Clinical Case Analysis (Paediatrics)".
  [F-A.7.3b, T328] Form controls do not inherit the body font. On every page, a .form-control textarea renders in the
  browser's monospace and a select in Arial, not DESIGN.md's Segoe UI family: Data Rights' Reason is monospace, and so
  are the activity form's feedback fields.

### Step A.7.4 — Dr Molefe's record on her phone
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: / → /portfolio/progress
Do: At 390 px, sign in, read Home, and open My Progress from the folded nav.
Expect: Home says that she completed her programme and points her to My progress, as at Step 5.21 (T252). My Progress
  shows her read-only record of past periods (T252), its cards fitting the width.
Note: The code shows an account with no role the "No role assigned" card instead, which tells her an administrator can
  give her one (`Home.razor`; reported).
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, she signs in to Home. Her nav holds Home, My Account, Data
  Rights, My Progress and Logout. Home shows a single card: "No role assigned / Your account holds no role at the
  moment, so there is nothing to show you here. An administrator can give you one." It neither says she completed her
  programme nor points to My progress. My Progress (from the folded nav) opens read-only with no controls. Its notice
  reads "You completed your programme on 26 September 2026. This page is your record of it and is read-only: no target
  applies to you any more..." Its programme card reads Started 15 January 2023, Completed 26 September 2026, then the
  per-semester and per-EPA cards. All 20 cards stack at the 16 px gutter; the three trajectory tables fit (274 px), and
  the one wide table (539 px) scrolls in its container. The page never scrolls sideways.
Gap: [F-A.7.4a, T311] The reviewer's suspect is confirmed. Home shows a former trainee (no role, with a trainee record) the
  default "No role assigned" card, which says an administrator can give her a role. T252 and Step 5.21 decided that Home
  says she completed her programme and points to My progress (Home.razor's default case). My Progress itself meets the
  Expect.

### Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions Due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its invitee table (counts by respondent group: the page never
  lists an address) scrolls within its card. Decisions Due's summary scrolls sideways, and a keyboard can scroll it, as
  a labelled, focusable region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home's three cards stack at the 16 px gutter. His folded
  nav holds Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due and Stalled Activities. MSF Campaigns:
  2 rows (du Plessis Withdrawn; Molefe Released, 7 invited and 6 responded); the table (697 px) scrolls in its
  container; each action is named ("View report: the campaign for Lerato Molefe (Default MSF, closing 2026-10-10)").
  Campaign 1 keeps its gutter, with the card at 16 to 359. Its Invitees table counts by group (Peer doctor 2/1,
  Consultant 2/2, Nurse 2/2, Allied health professional 1/1, All groups 7/6), and at 316 px it scrolls inside its 291 px
  card. Decisions Due: its summary is a .table-container with role=region, tabindex=0 and the name "By EPA" (685 px in
  291 px); focused, ArrowRight scrolled it from 0 to 200 px; its focus ring is the browser default (auto 1px), not
  --focus-ring. The second table (598 px) scrolls in its container. Data rights queue: the Type and Status filters stack
  in a card above the 4-row table (628 px, scrolls in its container), with the pager below; "Apply filters" sits in the
  page header, above both. The page never scrolls sideways. The erased requester still reads ndlovu@kgk.wombat.local
  (F-A.1.12a).
Gap: runbook corrected: the Expect called the campaign page's table an "address table", but a released campaign counts
  invitees by respondent group and the page says it never lists who was invited. The Expect now says "invitee table
  (counts by respondent group...)". [F-A.7.5a, T328] Decisions Due's summary region (.table-container[tabindex="0"]) has no
  :focus-visible rule, so a keyboard user sees the browser's thin default ring, not --focus-ring (DESIGN.md:
  ":focus-visible uses --focus-ring"). [F-A.7.5b, T328] The data-rights queue's "Apply filters" button is a page-header
  action, so it comes before the Type and Status filters it applies, in both reading and Tab order. At 390 px it sits
  above them.

### Step A.7.6 — Dr Zulu on her phone
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee Reviews and Dr Molefe's final review.
Expect: The review's cards stack. The evidence tables scroll inside their containers, and each EPA's trajectory chart
  fits the width (T166).
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home ("Viewing as CommitteeMember") shows two stacked
  cards. Her folded nav holds Activity Inbox, Recent Activities, Programme Trainees, Decision Panels and Committee
  Reviews. Committee Reviews lists 8 reviews; its table (902 px) scrolls in its container. Dr Molefe's final review is
  review 7 (Pre-graduation, Ratified). Its 17 cards stack at 16 to 359, with nested cards at 41 to 334, and the page
  never scrolls sideways. Its six evidence, entrustment and agenda tables (464 to 807 px) each scroll inside a 257 to
  291 px container. The three trajectory charts (PAED-001, PAED-010, PAED-012) are 259 px wide and fit. The chart SVGs
  have viewBox 600x200, scaled to 274 px, so their axis labels ("1", "3a", the dates) render about 6 px tall.
Gap: [F-A.7.6a, T323] The trajectory charts fit the width only by scaling down. The 600x200 viewBox drawn at about 260 px
  shrinks the 11px axis labels (levels 1 to 5 and the dates) to about 5 to 6 px on a phone, which cannot be read. The
  table beside each chart is visually hidden, for screen readers only, so a sighted phone user has no legible scale. The
  same component draws My Progress's charts (293 px).

### Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open her dashboard, Decision Panels, Decisions Due and, by its address, Entrustment decisions.
Expect: The dashboard's coverage cards stack. The panels list and the decisions list scroll inside their containers
  (T226).
Note: No nav item leads to Entrustment decisions for her role; only the InstitutionalAdmin dashboard links to it.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home ("Viewing as SpecialityAdmin") stacks its three
  cards: Pending reviews 1, Trainees in programme "2 active / 3 inactive" (consistent with the profiles), and Curriculum
  coverage for Semester 2, 2026, whose 16 items (KGK-001 and PAED-001 to 015) stack one per row. Each item's "0 of 2
  met" wraps to three lines in its narrow right column. Her nav holds Programme Trainees and STAR Review Queue
  (placeholders), Decision Panels, Committee Reviews and Decisions Due, with no Entrustment decisions item, as the Note
  says. Decision Panels: the list table (445 px) scrolls in its container, and the two member tables fit (291 px).
  Decisions Due is as at A.7.5: a focusable "By EPA" region (685 px in 291 px), and a second table scrolling in its
  container. /admin/entrustment-decisions, opened by address: 18 rows; the table (783 px) scrolls in its container, with
  its Trainee and Status filters above. No page scrolls sideways, and the gutter is 16 px throughout.
Gap: none

### Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due → /placeholder/{Feature} → /placeholder/{Feature}
Do: At 390 px, open his dashboard, Committee Reviews and Decisions Due, then Programme Trainees and STAR Review Queue
  from the folded nav.
Expect: As for Dr Mokoena, scoped to his sub-speciality. His nav offers the same two "Coming soon" stubs as hers
  (A.5.12), and each fits the width.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home ("Viewing as SubSpecialityAdmin") matches Dr
  Mokoena's: three stacked cards (Pending reviews 1, "2 active / 3 inactive", and coverage of Semester 2, 2026), since
  the Paediatrics sub-speciality holds every KGK trainee. Committee Reviews: 8 rows; the table (902 px) scrolls in its
  container. Decisions Due: the focusable "By EPA" region (685 px in 291 px), and a second table scrolling in its
  container. His nav offers the same two stubs as hers. Programme Trainees and STAR Review Queue each show one card,
  "This page is not built yet. Coming soon. The menu links here so that your role's navigation is complete. The page
  itself is still to be built.", which fits the width. No page scrolls sideways.
Gap: none

### Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK Teaching Session
  Log in the builder.
Expect: The user page's cards stack, and its buttons wrap rather than overflow. The trainee profile form stacks its
  fields. The builder's editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
  stays usable.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home's three cards stack. Users (14 rows): the filter is
  293 px wide, and the table (833 px) scrolls in its container. Dr Dlamini's account (/admin/users/225e1615-...): five
  cards stack at 16 to 359 (Account summary, Roles, Reset password, Lockout, Pending invitations). Every button stays
  inside the width: Back to users, Remove, Add role, Reset password and Lock out user each sit on a line of their own,
  right-aligned. The password field's "Show" button is 23 px tall, and the role's Remove button touches the role name
  "Trainee" with no gap. Trainees: the two tables (512 and 486 px) scroll in their containers. Dr Dlamini's profile
  (/admin/trainees/edit?id=3) stacks its four fields in one column, each 293 px wide. The builder
  (/admin/activity-types/23, "Edit KGK Teaching Session Log"): the editor and Live preview cards stack in one column
  (both at 49 to 326; the preview starts at y 4251). The tab bar wraps to two rows (Metadata, Form, Workflow; then
  Credit), each tab 33 px tall. No page scrolls sideways.
Gap: [F-A.7.9a, T280] The builder's tab bar marks the active tab only by the .is-active class. Its tabs are plain buttons with
  no role="tab", aria-selected, aria-pressed or aria-current, so a screen reader cannot tell which of Metadata, Form,
  Workflow and Credit is showing (WCAG 4.1.2). DESIGN.md's .tab-bar entry says nothing on this. [F-A.7.9b, T328] The Reset
  password field's "Show" toggle is 23 px tall, under this section's 24px rule (T086). F-A.7.2a's 21 px account-address
  link recurs on every page here.

### Step A.7.10 — Dr Kruger on his phone
Role: CollegeAdmin — Dr Anton Kruger
Route: / → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: At 390 px, open EPAs, then Curricula and the items of Paediatric EPA Curriculum 11.2.
Expect: The EPA list and the items table scroll inside their containers (DESIGN.md § Table system). Each item's row
  actions stay reachable, and the item editor's inputs keep usable widths (T198, T226).
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home shows one card. EPAs: 16 rows; the table (750 px)
  scrolls in its container. Curricula: 3 rows (Neonatology EPA Curriculum 1.0, Paediatric EPA Curriculum 11.2 and 11.1);
  the table (907 px) scrolls in its container. The items of 11.2 (/admin/curricula/4/items): 16 rows; the table (796 px)
  scrolls inside a 291 px container, and each row's actions are named ("Edit PAED-001", "Remove PAED-001") and reachable
  by scrolling it. Pressing Edit PAED-001 opens the item editor inside the table, as a row spanning all 9 columns (td
  colspan=9, 796 px). Its inputs are 370 px wide, two to a row, and the container scrolls 490 px sideways to show it, so
  each field and its help text is cut off at the screen's edge. Cancel ("Cancel editing PAED-001") closed it, and
  nothing was saved. The page never scrolls sideways.
Gap: [F-A.7.10a, T323] At 390 px, the curriculum item editor is not usable without sideways scrolling. It renders as a colspan
  row inside the items table's scroll container, so it takes the table's 796 px width. Its two-column form (370 px
  inputs) and help text spill off the phone's 291 px column, and the user must scroll the table sideways to read and
  fill each half. The Expect (T198, T226: inputs keep usable widths) holds for the input widths, but not for the editor
  as a whole. T198 and T226 were never checked with the editor open at 390 px.

### Step A.7.11 — devadmin on his phone
Role: Administrator — devadmin
Route: / → /admin/jobs → /admin/jobs/runs → /admin/institutions
Do: At 390 px, open Scheduled Jobs, Run history and Institutions.
Expect: The jobs table scrolls inside its container, with each Run now and toggle reachable. The run-history filters
  stack.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, Home's three cards stack. Scheduled Jobs: 9 rows; the
  table (661 px) scrolls in its container. Each row's "Disable <key>" toggle (28 px) and "Run now: <key>" (38 px) are
  reachable by scrolling it. Run history (from the jobs page): its Key, Status, From and To filters stack one per row (x
  41 to 334), with Filter below them and the 24-row table under that (712 px, scrolls in its container). Institutions: 2
  rows (Demo Institution; Kgosi Kgari Teaching Hospital, hod.paediatrics@kgk.wombat.local, Active); the table (699 px)
  scrolls in its container, and each Edit is reachable. No page scrolls sideways, and the gutter is 16 px.
Gap: none

### Step A.7.12 — The anonymous pages on a phone
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's portfolio PDF from Act 5 by
  its hash.
Expect: The sign-in card fits the width, and its fields and buttons are easy to tap. The verify page's result fits
  the width.
Actual (2026-09-26, T295 replay, wombat_scenario): At 390 px, signed out. Sign-in: the card spans 24 to 366, with Email
  and Password 260 x 44 px, Sign in 78 x 33 px, and the "Reset it" link inline in its sentence. The password field's
  "Show" toggle is 45 x 23 px and sits at the field's top edge, not centred. Forgot password: the same card, with the
  info alert "Password reset is not wired yet in the rewrite. Ask an administrator..." (A.4.4's reported wording) and
  "Back to sign in" (46 px). Verify: typed Dr Molefe's hash 68a17034...ec9c and pressed Verify. It loaded
  /portfolio/verify?hash=...&check=1 with the card "Export verified. This PDF was generated by Wombat on 2026-09-26
  12:09 UTC. Trainee ID ... Exported by ... Filter: 2023-01-15 to 2026-09-26. File: portfolio-68a170344cf3.pdf", which
  fits the width with the ids wrapping. No page scrolls sideways.
Gap: [F-A.7.12a, T328] On the sign-in card, the password field's "Show" toggle is 23 px tall (as F-A.7.9b) and pinned to the
  top edge of the 44 px field instead of centred in it. The user page's toggle is centred.

### Step A.7.13 — Dr Zulu hears a populated review with a screen reader
Role: CommitteeMember — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: With NVDA (or Narrator) on, open Dr Molefe's final review. Walk it by headings, then by tables, then by regions.
Expect: The page heading is announced on arrival. Each card is a heading ("Review", "Decision", "Evidence snapshot",
  "Rating trajectory by EPA", "Appeals", "Entrustment against Annexure A", "Multi-source feedback by EPA", "Agenda"),
  and each EPA group within a card is a heading below it.
  - Each trajectory chart is announced by its EPA, and a table follows it that a screen reader reads, with one row per
    observation (T085).
  - Evidence tables announce their column headers, and every row action is named for its row (T239).
  - Every badge is read as its words.
  - No control is unnamed.
Note: The headings go from level 1 to level 3 with no level 2 (noted by T085; cosmetic).
Actual (2026-09-26, T295 replay, wombat_scenario): No screen reader can be driven here, so Chromium's accessibility tree
  for review 7 (Dr Molefe's pre-graduation review) was read in NVDA's place, at 1280 px. The focus arrives on the h1
  "Committee review". Headings: the h1, then h3s for Sampling concentration warnings, Review, Decision, Entrustment
  against Annexure A, Multi-source feedback by EPA, Agenda, Pending entrustment decisions, Evidence snapshot, Rating
  trajectory by EPA and Appeals (no h2, as the Note says). The Decision card has an h4 "Graduate (programme complete)";
  Evidence snapshot has an h4 per EPA (PAED-001, 010, 012 and "Not about a single EPA"), and so does Rating trajectory
  (001, 010, 012). All 9 tables have a caption and column headers, and the evidence rows have row headers per
  instrument. Each record link is named for its row ("Case-Based Discussion (Paediatrics) #13"). Each chart is role=img
  "Rating trajectory for PAED-001" (by its code), followed by a table captioned the same with one row per observation
  (3, 1 and 2). Badges are plain text ("At or above", "Completed", "Reached"). Regions: only 3 of the 9
  section.detail-card elements are named (Multi-source feedback by EPA, Agenda, Evidence snapshot). The Sampling
  warnings' dismiss button's only name is "×".
Gap: [F-A.7.13a, T280] A control is effectively unnamed: the "Sampling concentration warnings" dismiss button is <button
  class="btn btn-outline btn-xs">×</button> with no aria-label, so a screen reader announces "times button" or
  "multiplication sign button". [F-A.7.13b, T280] Walking by regions reaches only 3 of the page's 9 cards. Review, Decision,
  Entrustment against Annexure A, Pending entrustment decisions, Rating trajectory by EPA and Appeals are sections with
  no aria-labelledby, so they are not landmarks, while their three siblings are. Recorded from the accessibility tree,
  not from NVDA or Narrator.

### Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient;
  - muted text on the page background;
  - the status badges on a committee review opened from Committee Reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My Account unchanged: "Profile saved.") and a danger alert (Change password with a
    confirmation that differs, which checks no password and changes nothing);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
Note: Calculated from app.css's tokens, several pairs fall short (reported):
  - the semantic colours as text on their own tints (the Completed, Accepted and Declined badges, and the success,
    warning and danger alerts);
  - white on the danger and success buttons;
  - the focus ring on the page background;
  - input borders.
Actual (2026-09-26, T295 replay, wombat_scenario): At 1280 px, WCAG ratios were computed from the rendered colours in
  the page (no axe in this browser). Passing: the sign-in card's text is #333 on white, 12.63:1; its muted line is
  5.09:1; its link #0b5cab is 6.70:1; white on primary (#2d6cdf) is 4.86:1. The nav's white is 14.07 and 16.19:1 on the
  gradient's stops, and the active item (a 37% white overlay) is 4.51:1. Muted #686f77 is 4.83:1 on the page background
  (#f8f9fa) and 5.09:1 on white. The focus ring #3498db is 3.15:1 on white. Failing: the success alert "Profile saved."
  (#27ae60 on #e8f5e9, 2.55:1; focus moved to its result region); the danger alert "The password confirmation does not
  match." (#e74c3c on #fff5f5, 3.57:1; nothing changed); Lock out user (white on #e74c3c, 3.82:1; not pressed); success,
  white on #27ae60, 2.87:1 (Publish is disabled with no draft, so exempt there). Also the focus ring on the page
  background (2.99:1) and the input border #ced4da (1.49:1 on white, 1.42:1 on the page). Committee Reviews shows states
  as plain text (#333 on white, 12.63:1). Review 7's badges: "Decided" (.badge-completed) is 2.55:1; the standing badges
  are 11.2 to 12.0:1. Measured in a detached test element: .badge-accepted and .alert-warning 2.42:1, .badge-declined
  3.57:1, .validation-message 3.82:1.
Gap: runbook corrected: the Do named "the status badges on Committee Reviews", but that list shows each state as plain
  text; the badges are on a review's page. The Do and Route now take them from review 7. [F-A.7.14a, T322] Contrast below WCAG
  2.1 AA, confirming the Note's calculation by measurement. Semantic text on its own tint fails: success and completed
  2.55:1, warning and accepted 2.42:1, danger and declined 3.57:1. White on .btn-danger is 3.82:1 and on .btn-success
  2.87:1; .validation-message on white is 3.82:1. The --focus-ring on the page background is 2.99:1 (under 3:1), and the
  input border #ced4da is 1.49:1 (under 3:1).

## Outcome state

Replay check (2026-09-26, T295, wombat_scenario): match. With <appendix start> = 2026-09-26 15:10:00+02 (between Act 6's last command at 15:06:40 and A.1.1's at 15:13:22), every query returns what it expects: the four KGK requests (0,5,1) (1,3,1) (2,4,1) (4,4,1); one erasure (pseudonym, no email, name or password, no institution, lockout infinity) with 0 roles, 0 logins and 0 activities on the original id; the profile inactive since 2026-09-26; reviews (false,4) (true,6) (true,7,true); nine jobs enabled, each run once by devadmin and the digest twice, 0 failed today; 0 SSO mappings; Dlamini opted out of optional processing only, Patel not locked, Smit not opted out; KGK hod.paediatrics@kgk.wombat.local and active; audit Submit 4, Withdraw 1, Approve 2, Reject 1, RunScheduledJobNow 10, SetUserLockout 2, ResetUserPassword one false and one true; 0 unredacted submits. Also as stated: Mahlangu has one draft and one requested Mini-CEX; Dlamini's activity 28 is completed with DaysAfterEncounter 20.

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
SELECT "Key", "IsEnabled" FROM "ScheduledJobDefinitions" ORDER BY "Key";                              -- nine, all true
SELECT r."Key", count(*) FROM "ScheduledJobRuns" r JOIN "AspNetUsers" u ON u."Id" = r."TriggeredBy"
WHERE u."Email" = 'devadmin@wombat.local' AND r."StartedAt" >= '<appendix start>'
GROUP BY r."Key" ORDER BY 1;                                                    -- nine keys; the digest twice
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
-- ResetUserPassword one false (the 10-character password) and one true. If A.6.3 saved KGK inactive, devadmin's
-- repair adds nothing here.
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
- the error page as a failure reaches it;
- the five "Coming soon" features.

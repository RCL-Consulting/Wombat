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
Do: Open My data rights, under the rule at the foot of her menu. Read the page, tick "Opt out of optional processing",
  and save her preferences.
Expect: The page has three parts: processing preferences, a request form, and "Your requests", which is empty ("No
  requests"). The digest opt-out's help names the three reminders it stops (the weekly coordinator digest, the 14-day
  draft reminder and the 5-day assessment reminder), and says that email about one particular thing is still sent
  (T240). Saving says "Processing preferences saved." and the focus moves to that message (T234).
Note: Nothing in the product reads the optional-processing flag (reported).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): The menu reads "Acting as Trainee"; My data rights stands last,
  after My progress. Opened, it is lit and the tab reads "My data rights · Wombat". Three parts: Processing
  preferences, Submit a request, and Your requests reading "No requests / You have not submitted any data rights
  requests yet." The digest help names the weekly coordinator digest, the 14-day draft reminder and the 5-day
  assessment reminder, and says email about one particular thing is still sent. Ticked the optional-processing opt-out
  and saved: "Processing preferences saved." took the focus.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Four types offered (Access, Export, Rectification, Erasure,
  each with its gloss). Export with no reason: "Please provide a reason for your request.", no row written, the focus
  left on Submit request. With the reason: "Request submitted. You will be notified when it is reviewed." took the
  focus; Your requests lists "2026-10-03 17:25 | Export | Submitted" (made at 19:25 SAST) with "Withdraw the Export
  request made 2026-10-03 17:25" and no Download. No mail.
Gap: F-A.1.2a, T319 (still: the promised notification is never sent). F-A.1.2c, T325 (still: the time prints in UTC,
  17:25 at 19:25 SAST).

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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Data rights requests from the menu, lit. The queue held only
  Dlamini's KGK request. Choosing Erasure changed nothing until Apply filters, then "No requests / No data rights
  requests match the current filters." Export + Submitted listed her row: her address, Export, Submitted and "Review
  the Export request from dlamini@kgk.wombat.local, 2026-10-03 17:25". On request 01a102cc-… the menu item stays lit
  and the trail reads Home › Data rights requests › its id. It shows requester, user id, submitted, type, status and
  reason. Approve with no note: "A decision note is required." With the note: "Request approved." took the focus;
  Status Completed; a Decision card (Decided by 0a2db915-…, Smit's id; decided on; the note; completed on 17:25:35); no
  Approve or Reject. No mail. The filters still offer "UnderReview" and "Objection". At 1280 px the Request card clips
  "dlamini@kgk.wombat.loc" and the user id.
Gap: F-A.1.3b, T324 (still: raw "UnderReview" and the unsubmittable "Objection" in the filters). F-A.1.3c, T323 (still:
  the Request card clips the address and the user id).

### Step A.1.4 — Mr Smit cannot download what he approved
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights/download/{id:guid}
Do: Open the download address of the request he has just approved. Its id is in the detail page's address.
Expect: Page not found, with status 404. Nothing on the page says that the request exists. The reviewer approves; only
  the data subject, or a global Administrator, collects the bundle (T112).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): The download address of request 01a102cc-… answered 404 at the
  typed address, tab "Page not found · Wombat": "Page not found", "There is no page at this address.", "Check the
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): My data rights from the menu: "2026-10-03 17:25 | Export |
  Completed" with "Download the export of the Export request made 2026-10-03 17:25". Fetched with her session through
  Playwright's request context (README § Downloads): 200 application/zip, attachment data-export-20261003-172550.zip
  (137,208 bytes). It holds data-export.json (schemaVersion 2: profile, 5 activities, 1 committee review, 0 MSF
  campaigns, 3 progress rows, 32 audit entries, 0 portfolio exports) and portfolio-summary.pdf (142,334 bytes). No
  mail.
Gap: none

### Step A.1.6 — Dr du Plessis asks for access, then withdraws the request
Role: Trainee — Dr Pieter du Plessis
Route: /account/data-rights
Do: Submit an Access request with the reason "Everything held about me." Then withdraw it from "Your requests".
Expect: The request lists as Submitted, with a Withdraw named for its type and time. Once withdrawn, the page says
  "Request withdrawn.", the focus moves to that message, and the row reads Withdrawn and offers nothing.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Access with the reason: "Request submitted. You will be
  notified when it is reviewed." took the focus; the row "2026-10-03 17:26 | Access | Submitted" with "Withdraw the
  Access request made 2026-10-03 17:26". Withdraw acted at once (no dialog): "Request withdrawn." took the focus; the
  row reads Withdrawn and its actions cell is empty.
Gap: none

### Step A.1.7 — Dr Mahlangu asks for a correction
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Submit a Rectification request with the reason "My programme start is recorded as 15 January 2026; I started work
  on 19 January."
Expect: The request lists as Submitted, with a Withdraw.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Rectification with the reason: "Request submitted. You will be
  notified when it is reviewed." took the focus; the row "2026-10-03 17:26 | Rectification | Submitted" with "Withdraw
  the Rectification request made 2026-10-03 17:26".
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Both filters set to All and applied: three KGK rows, newest
  first: Mahlangu, Rectification, badge-submitted; du Plessis, Access, Withdrawn (badge-draft); Dlamini, Export,
  Completed (badge-completed). The withdrawn request's page shows its Request card (Status Withdrawn) and no Action
  card. Mahlangu's, rejected with the note: "Request rejected." took the focus; Status Rejected, a Decision card
  (Smit's id, 17:26:16, the note), no buttons left. No mail.
Gap: none (the Note's stranded Approved rectification is from code and not played, since the step rejects)

### Step A.1.9 — Dr Mahlangu reads the decision
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Reload My data rights and read her request.
Expect: The request reads Rejected and offers nothing: no Withdraw, no Download. The page shows neither the decision
  note nor who decided, and no email told her of the decision.
Note: The confirmation at A.1.7 promised a notification (reported at A.1.2).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Reloaded, Your requests lists "2026-10-03 17:26 | Rectification
  | Rejected" with nothing in Actions (no Withdraw, no Download). The page shows neither the decision note nor who
  decided, and the log holds no mail about any data-rights request.
Gap: F-A.1.9a, T319 (still: a rejected requester is never told why, though A.1.7's confirmation promised a
  notification; the Expect records the page as it stands)

### Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period holding `D`, Scheduled on `D`,
  with Formative only ticked. Note the review's id from its address.
Expect: The review opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled, as at Step
  4.49. It is open, so the erasure at A.1.12 has a review to end.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Committee reviews → Schedule review: the trainee picker,
  enabled once the panel is chosen, offered Anele Dlamini, Nomsa Mahlangu and Sipho Ndlovu. Paed Annual Review Panel,
  Sipho Ndlovu, 2026 S2 (the default, "1 Jul to 31 Dec 2026"), Scheduled on 2026-10-03, Formative only ticked: "A
  formative review carries no agenda: it decides no EPA." Create review opened review #8: Type Annual progression
  review, Mode "Formative (interim check-in, no binding decision)", State Scheduled, Start review offered; trail Home ›
  Committee reviews › Committee review.
Gap: none (the Type "Annual progression review" on a check-in is F-4.49a)

### Step A.1.11 — Dr Ndlovu requests erasure
Role: Trainee — Dr Sipho Ndlovu
Route: /account/data-rights
Do: Submit an Erasure request with the reason "I am leaving the programme and want my personal data removed." Stay
  signed in in this browser.
Expect: The request is accepted although a review of him is scheduled: no review's state refuses an erasure request
  (T258). It lists as Submitted, with a Withdraw.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Erasure with the reason, accepted although review #8 of him is
  Scheduled: "Request submitted. You will be notified when it is reviewed." took the focus; the row "2026-10-03 17:27 |
  Erasure | Submitted" with "Withdraw the Erasure request made 2026-10-03 17:27". His browser was left on My data
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Played in a second browser, so his stayed open. The
  Administrator's queue listed all four requests (KGK's are the only ones); Erasure applied, it held his alone. Approve
  with the note, no dialog in between: "Request approved." took the focus; Status Completed and a Decision card
  (devadmin's id 7f569340-…, 17:27:19, the note, completed 17:27:19). The account became deleted_user_93f30125 (SQL: no
  email, no password, lockout infinity); review #8 is Withdrawn. The queue and the request still read
  ndlovu@kgk.wombat.local.
Gap: F-A.1.12a, T276 (still: the kept request names the erased person by his address, as the Expect records; approving
  the irreversible erasure asks nothing first, noted on T264)

### Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Sign in again." (T279), an
  information notice. Signing in is refused in the words an unknown address gets, "Invalid email or password.", with
  no mention of an erasure (T156). No email is sent to him.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): His tab, left on My data rights, went by itself 51 s after the
  approval to /account/login?error=SessionEnded&returnUrl=%2Faccount%2Fdata-rights: an information notice (alert-info),
  "Your session has ended. Sign in again." His old address and password: ?error=Refused, tab "Error: Sign in · Wombat",
  "Invalid email or password.", nothing of an erasure. No mail.
Gap: none

### Step A.1.14 — What the erasure left, as KGK sees it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or trainees (T026, T258).
  - Committee reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review and closed check-in as they were, and A.1.10's check-in as Withdrawn.
  - The check-in reads Withdrawn, dated today, with the reason "Withdrawn because the trainee's personal data was erased
    at their request. Nothing more is decided at this review.", and offers no action.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.
Note: T276 (open) lists what an erasure does not yet end: an activity about him still in its workflow can be completed
  and credit the pseudonym, and user ids inside activity data are not rewritten.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Users lists 14 KGK accounts, with no Ndlovu and no pseudonym.
  Trainees: active Dlamini and Mahlangu; completed and closed du Plessis (Withdrawn 2026-10-01) and Molefe (Completed
  2026-10-03); no Ndlovu. Committee reviews lists three deleted_user_93f30125 rows: Act 4's ratified review (Outcome
  Deferred), the closed formative check-in (#6), and A.1.10's check-in as Formative, Withdrawn, "None: the review was
  withdrawn" (#8). Review #8 reads State Withdrawn, "2026-10-03. Withdrawn because the trainee's personal data was
  erased at their request. Nothing more is decided at this review.", Evidence snapshot "No evidence was frozen: the
  review was withdrawn before it started.", and offers no action but the warnings' ×. In SQL his profile is inactive,
  deactivated 2026-10-03, under the pseudonym, and no activity refers to his original id.
Gap: none (the withdrawn review still draws the live Annexure A card and a sampling warning, noted as before)

## A.2 — Scheduled jobs

### Step A.2.1 — devadmin reads the scheduled jobs
Role: Administrator — devadmin
Route: / → /admin/jobs
Do: Open Scheduled jobs from the menu and read every row.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Scheduled jobs from the Platform group, lit, tab "Scheduled
  jobs · Wombat". Columns Job, Schedule, Last run, Status, Next run, Enabled, Actions: the nine jobs by key in the
  runbook's order, each with its description, cron (activity-draft-nudge 0 7 * * *, last run 2026-10-03 16:56, next
  2026-10-04 09:00 local), a Succeeded badge, "Disable <key>" and "Run now: <key>". weekly-coordinator-digest had never
  run (the replay day is a Saturday): its last run and status read "—", its next run 2026-10-05 10:00. Run history is a
  header action.
Gap: none

### Step A.2.2 — devadmin disables a job and enables it again
Role: Administrator — devadmin
Route: /admin/jobs
Do: Disable msf-campaign-auto-close, try to run it now, then enable it again.
Expect: Disabling says "Job 'msf-campaign-auto-close' disabled." The row then reads "—" for its next run, its toggle
  says Enable, and its Run now is unavailable. Once enabled again, its next run is the next hour. The scheduler does not
  run a disabled job. The audit log records both changes.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Disable: "Job 'msf-campaign-auto-close' disabled." took the
  focus; the row reads Next run "—", its toggle "Enable msf-campaign-auto-close", and its Run now is disabled (a forced
  press did nothing). Enable: "Job 'msf-campaign-auto-close' enabled.", next run 2026-10-03 20:00 (the next hour), Run
  now available again. The audit holds DisableScheduledJobCommand and EnableScheduledJobCommand, both successful.
Gap: none

### Step A.2.3 — Mr Smit opts out of digest emails
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."
Actual (2026-10-03, T350 replay, wombat_scenario_t350): My data rights from the menu; "Opt out of digest emails" was
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Job 'weekly-coordinator-digest' dispatched." took the focus;
  the row's last run is 19:29, Succeeded. The log reads "WeeklyCoordinatorDigestJob: digests sent 1; coordinators
  skipped: deactivated 0, opted out of digest emails 1, no email address 0, holds Trainee 0, no institution 0." Its one
  stub mail "Your weekly Wombat digest" greets "Hi Demo," (the dev coordinator); none greets Pieter.
Gap: none

### Step A.2.5 — Mr Smit opts back in
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Untick "Opt out of digest emails" and save.
Expect: "Processing preferences saved."
Actual (2026-10-03, T350 replay, wombat_scenario_t350): The digest opt-out opened ticked; unticked and saved:
  "Processing preferences saved.", the box now clear.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Job 'weekly-coordinator-digest' dispatched.", Succeeded
  (19:29). The log reads "WeeklyCoordinatorDigestJob mail of the run started 2026-10-03 17:29:11Z: sent 0, not
  delivered 0, not reported before the job ran again 1." and "digests sent 2; coordinators skipped: … opted out of
  digest emails 0 …". The stub "Your weekly Wombat digest" reads "Hi Pieter, Here is your weekly summary: No items
  requiring attention this week.": KGK only, no campaign waits, review #8 is withdrawn, and Dlamini and Mahlangu have
  filed today. The Demo coordinator's digest was ignored.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Log an activity opened the picker ("Rated by an assessor", 7
  types); Mini-CEX (Paediatrics) opened /activities/new?type=mini_cex_cpsa, its EPA list offering 9. Both with
  PAED-002, Khumalo as assessor, Ward, a presenting problem and complexity Moderate; the button read "Submit to Fatima
  Khumalo". Activity 27 (encounter 2026-09-30), Save draft: "Draft saved. It has not been submitted. It is in nobody's
  inbox until you submit it." took the focus; Who has it now "Draft / With you. Not submitted yet." Activity 28
  (encounter 2026-10-01), Submit to Fatima Khumalo: "Submitted. It is now Requested. It is in Fatima Khumalo's Activity
  inbox." took the focus. My activities' Needs you (1) lists 27 alone, as Draft; All activities (4) lists 28 as with
  Fatima Khumalo, Requested. Khumalo's inbox, Waiting for you "2 waiting", lists du Plessis's CBD then "Mini-CEX
  (Paediatrics) · PAED-002 · 2026-10-01 … from Nomsa Mahlangu … Requested". No mail.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Stand-in as the Note prescribes: UpdatedOn −15 days on activity
  27 (now 2026-09-18) and −6 days on 28 (now 2026-09-27). Both jobs dispatched and Succeeded (19:30). Stub mails: "You
  have draft activities waiting", "Hi Nomsa, … Mini-CEX (Paediatrics) - draft for 15 days"; and "Activities awaiting
  your assessment", "Hi Fatima, … Mini-CEX (Paediatrics) from Nomsa Mahlangu - waiting 6 days" (the log prints the dash
  as "-"; Dr Patel was also nudged for du Plessis's review, waiting 8 days). Logs: "ActivityDraftNudgeJob: trainees
  reminded 1 (drafts 1); trainees skipped: no such account 0, deactivated 0, opted out of digest emails 0, no email
  address 0." and "AssessorPendingNudgeJob: assessors nudged 2 (activities 2); nominees skipped: … 0 …".
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Each of the six, run now one at a time, reads Succeeded
  (19:30), with one log line each: "EntrustmentDecisionExpiryJob: expired 0 decisions (sent 0), reminded 0 expiring
  soon (sent 0)."; "MsfCampaignAutoCloseJob: no expired campaigns found."; "MsfInvitationExpiryReminderJob: no
  respondent is due a reminder."; "AuditLogRetentionJob: complete. Total archived: 0."; "PortfolioExportCleanupJob: no
  expired exports found."; "ScheduledJobRunRetentionJob: no old runs to delete." No mail.
Gap: none (the Note's 90-day verify loss is F-A.2.9a, T313, not replayed)

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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Run history, from the jobs header (trail Home › Scheduled jobs
  › Job run history), lists 33 runs newest first: Key, Started, Finished, Duration, Status, Triggered by, Error. The
  scheduler's read "scheduler"; the ones by hand read 7f569340-94ac-4686-b4ad-e35173193d84, devadmin's user id. Typing
  a key changed nothing until Filter. "weekly-coordinator-digest": its two runs by hand, no other job (the scheduler
  has not run it this week). "digest": "No runs found". Status Failed: "No runs found". From 2026-10-03: 22, today's
  runs. The key's placeholder reads "Filter by key...".
Gap: F-A.2.10a, T324 (still: "Triggered by" prints devadmin's user id, not his name as the Expect says). F-A.2.10b,
  T327 (still: the placeholder "Filter by key..." invites a part, and only the whole key matches).

## A.3 — Institutional sign-in (SSO)

### Step A.3.1 — Prof Mbatha opens SSO mappings with no provider configured
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/sso/group-mappings
Do: Open SSO mappings from the menu.
Expect: A card says that no SSO providers are configured, and to add them to the `Sso:Providers` section of the
  application settings and restart. No form for adding a mapping is offered. Current mappings is empty ("No group
  mappings"), although its empty state still says to add a mapping above. Nothing on the page can be changed.
Note: T288 (open, P2): once a provider exists, an InstitutionalAdmin can map another institution's provider groups, and
  the mapper grants those roles. It is checked when a provider is configured.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): SSO mappings from the "Access and audit" group, lit: "No SSO
  providers are configured. Add providers to the Sso:Providers section in application settings and restart." No input
  and no button on the page. Current mappings: "No group mappings / Add a mapping above to link external groups to
  Wombat roles." At 390 px nothing scrolls sideways.
Gap: F-A.3.1a, T326 (still: the empty state says "Add a mapping above" where no form is shown, as the Expect records)

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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Signed out, the sign-in page reads "Work-based assessment for
  specialist training." and "Sign in", then Email, Password with Show ("Show password"), Remember me, Sign in and
  "Forgotten your password?", with no "or" divider and no "Sign in with …" button. A wrong password for
  zulu@kgk.wombat.local: ?error=Refused, tab "Error: Sign in · Wombat", "Invalid email or password." (#login-error)
  took the focus, naming no institutional button. /account/sso-challenge/kgk returned to
  /account/login?error=SsoUnknownProvider: "That institution's sign-in is not set up in Wombat. Sign in with your email
  and password."
Gap: none

### Step A.3.3 — The callback and link pages with no institutional sign-in in progress
Role: Anonymous — a KGK consultant
Route: /account/sso-callback → /account/login → /account/link-external
Do: Open the callback address, then the link-your-account page, directly.
Expect: The callback returns to the sign-in page, which says "Your institution's sign-in did not complete. Sign in with
  your email and password." (the words of a page with no institution's button: none is configured). The link page is
  headed "Institutional sign-in expired" and says "Your institutional sign-in has expired. Start again from the
  sign-in page." It offers Back to sign in and no password field (T149).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /account/sso-callback returned to
  /account/login?error=ExternalLoginUnavailable: "Your institution's sign-in did not complete. Sign in with your email
  and password." /account/link-external: tab and heading "Institutional sign-in expired", "Your institutional sign-in
  has expired. Start again from the sign-in page." and only Back to sign in, with no password field. At 390 px nothing
  scrolls sideways.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): My account from "Sarah Botha" in the top bar: the name carries
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): A second browser was signed in as Khumalo on her Activity inbox
  first. From My account's Change password (tab "Change password · Wombat", trail Home › My account › Change password):
  "Choose a new password for signing in to Wombat.", the six rules under New password before anything is typed, and
  toggles "Show current password", "Show new password", "Show confirm new password". Each refusal came back by a full
  load with all three fields empty, tab "Error: Change password · Wombat", and "Your password was not changed." with
  the reason in the form's card above Cancel and Change password, taking the focus; the field concerned is aria-invalid
  and names it. (a) ?error=PasswordMismatch, "Incorrect password.", Current password marked. (b)
  ?error=ConfirmationMismatch, "The password confirmation does not match.", Confirm marked. (c) "qwertyui": "The new
  password needs:" "At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.", "A symbol, such as ! or
  #."; nothing on lower case; New password names "The new password does not meet the rules below." (d)
  ?status=password-updated, "Password updated." took the focus; she stays signed in. Recorded in the replay's password
  file (WB_PW_KHUMALO, the old one kept as WB_PW_KHUMALO_A). The audit holds no row for the change or the wrong current
  password.
Gap: F-A.4.2a, T286 (still: a password change and a wrong current password write no audit row; the Expect does not ask)

### Step A.4.3 — Dr Khumalo's other session ends
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Sign in again."
  (T279), an information notice. The old password is refused, and "Invalid email or password." takes the notice's
  place. The new one brings her back to her Activity inbox: the return address survives the refusal.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): The second browser's inbox left 54 s after the change, by a
  full load, for /account/login?error=SessionEnded&returnUrl=%2Factivities%2Finbox: the information notice "Your
  session has ended. Sign in again." The old password: ?error=Refused, "Invalid email or password." in its place. The
  new one brought her back to /activities/inbox (Mahlangu's Mini-CEX and du Plessis's CBD, both Requested).
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Forgotten your password?" led to /account/forgot-password, tab
  "Forgotten password · Wombat": "Forgotten password", "A Wombat administrator can set a new password for you.", then
  the three steps word for word as the Expect quotes them, and Back to sign in, which returned to /account/login. No
  field; no mail.
Gap: none

### Step A.4.5 — Prof Mbatha resets his password
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Set a new password of 10 characters, then one that meets every rule. Record the
  second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: The Reset password card says it sets a password directly and that the user is not emailed. Its field has the
  Show toggle ("Show new password") and the six rules under it, and Reset password is enabled once anything is typed.
  The 10-character password is refused with "The password was not reset. The new password needs:" and the rules it
  breaks, in the six rules' order. The second is accepted, and the field is cleared. The audit log records the reset
  with the password redacted (T101).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Users → "Manage Pieter du Plessis". The Reset password card:
  "Sets a new password directly. The user is not emailed; share the password securely out-of-band.", its field with
  "Show new password" and the six rules under it; Reset password is disabled while empty and enabled after one
  character. "shortpass1": "The password was not reset. The new password needs: At least 12 characters. An upper-case
  letter. A symbol, such as ! or #." took the focus, the field emptied, aria-invalid and naming "The new password does
  not meet the rules below." A 17-character password meeting every rule (typed at a person's pace): "Password reset.
  Share the new password with the user out-of-band.", the field cleared. The audit holds ResetUserPasswordCommand once
  false and once true. Roles reads "TraineeSystem-managed"; the Add role list names raw role keys
  ("InstitutionalAdmin", "SubSpecialityAdmin").
Gap: F-2.28b, T323 (still: "TraineeSystem-managed").

### Step A.4.6 — Dr du Plessis signs in with it and chooses his own
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → / → /account/profile → /account/change-password → /account/profile
Do: Sign in with the password Prof Mbatha set. Then change it, from My account (the name in the top bar), to one of
  his own. Record it in `pwd_DO_NOT_COMMIT.txt`.
Expect: He lands on his Trainee dashboard, which still says that his programme ended (Step 5.28). The change lands on
  My account with "Password updated."
Actual (2026-10-03, T350 replay, wombat_scenario_t350): With the password Prof Mbatha set he lands on /, "Trainee ·
  Semester 2, 2026". Curriculum targets reads "Your programme ended on 1 October 2026, so no target applies to you any
  more. Your progress in each period is kept on My progress, read-only."; Needs you reads "Nothing needs you. Requests
  you have filed are in My activities." My account (his name in the top bar) → Change password, to his own: "Password
  updated." took the focus. At 390 px nothing scrolls sideways. Recorded in the replay's password file
  (WB_PW_DUPLESSIS; the original kept as WB_PW_DUPLESSIS_A, the one Prof Mbatha set as WB_PW_DUPLESSIS_ADMINSET).
Gap: none

### Step A.4.7 — Mr Smit signs out through the confirmation page
Role: Coordinator — Mr Pieter Smit
Route: /account/logout-confirm → / → /account/logout → / → /account/logout-confirm → /account/logout/submit → /account/login → /msf/campaigns → /account/login → /msf/campaigns
Do: Open the sign-out confirmation by its address and press Cancel. Type `/account/logout` and press Cancel. Open the
  confirmation again and sign out. Then open MSF campaigns by its address.
Expect: A "Sign out" page, with no nav: "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends
  your session in this browser.", with Cancel and Sign out. Cancel returns him to his dashboard, still signed in. A
  typed GET /account/logout draws the same page and signs nobody out (T317). Sign out lands on the sign-in page with
  "You have signed out." MSF campaigns then asks him to sign in, and after signing in he is brought back to it. The
  audit log records a Logout.
Note: Only the error page links to the confirmation page: its account row's Sign out is a link there, since a form
  drawn on the error page could carry a refused token. Everywhere else the account row's Sign out, in the top bar or
  the phone menu's foot, is a form that signs out at once.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /account/logout-confirm, tab "Sign out · Wombat", with no nav:
  "Sign out", "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends your session in this
  browser.", Cancel and Sign out; at 390 px nothing scrolls sideways. Cancel returned him to /, "Coordinator · Semester
  2, 2026", still signed in. A typed GET /account/logout drew the same page at that address (200), and its Cancel
  returned him to / signed in. Sign out landed on /account/login?error=SignedOut: "You have signed out." /msf/campaigns
  then went to /account/login?ReturnUrl=%2Fmsf%2Fcampaigns, and signing in brought him back to MSF campaigns. The audit
  holds one Logout.
Gap: none

## A.5 — System pages

### Step A.5.1 — A registrar opens an administrator's pages
Role: Trainee — Dr Anele Dlamini
Route: /admin/users → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Users page, then that of Scheduled jobs.
Expect: Each time, "You cannot open this page": "Your role (Trainee) does not open this page." and "If you need it for
  your work, ask your institution's Wombat administrator.", with Go to Home, which takes her home. Nothing of the page
  she asked for is shown or named, and no switch of role is offered (T335, D6).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /admin/users went to /access-denied?ReturnUrl=%2Fadmin%2Fusers,
  tab "You cannot open this page · Wombat": "You cannot open this page", "Your role (Trainee) does not open this
  page.", "If you need it for your work, ask your institution's Wombat administrator." and Go to Home, which took her
  to /. /admin/jobs gave the same. Nothing of either page is named; no switch is offered; nothing is lit. At 390 px
  nothing scrolls sideways.
Gap: none

### Step A.5.2 — Prof Mbatha opens the Administrator's own pages
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled jobs.
Expect: For both, "You cannot open this page": "Your role (Institutional admin) does not open this page." and "If you
  need it for your work, ask the platform administrator.", with Go to Home. They are the Administrator's alone, her menu
  offers neither, and nothing in it is lit on the refusal.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Her menu (People, Curriculum, Reviews, Access and audit) offers
  neither page. /admin/institutions and /admin/jobs each went to /access-denied (ReturnUrl kept): "Your role
  (Institutional admin) does not open this page.", "If you need it for your work, ask the platform administrator." and
  Go to Home; nothing in the menu is lit.
Gap: none

### Step A.5.3 — A forged dashboard switch
Role: Trainee — Dr Anele Dlamini
Route: /dashboard/switch/{role} → /
Do: Open `/dashboard/switch/Administrator`.
Expect: She is back on her own dashboard, still acting as a Trainee. A switch to a role she does not hold changes
  nothing and says nothing (no "You are now acting as" alert), and the sidebar offers her no switch: she holds one role.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /dashboard/switch/Administrator redirected to /, "Trainee ·
  Semester 2, 2026", the head "Acting as Trainee"; no alert, and no switch in the menu.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Signed in, /portfolio/cv and /placeholder/recent-activities
  each answered 404 at the typed address, tab "Page not found · Wombat": "There is no page at this address.", "Check
  the address, or start again from Home." and Go to Home, with no address repeated. At 390 px nothing scrolls sideways.
  Signed out, /portfolio/cv went to /account/login?ReturnUrl=%2Fportfolio%2Fcv; signing in there gave Page not found at
  /portfolio/cv, 404.
Gap: none

### Step A.5.5 — Another institution's records, by id
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not "You cannot open this page". The user shows "User unavailable" ("The
  user could not be found or is outside your scope."). Neither page confirms that the record exists (CLAUDE.md: 404, not
  403).
Note: The Demo Institution stands in for another institution only here. Its ids come from
  `SELECT "Id" FROM "Institutions" WHERE "Name" = 'Demo Institution'` and from `"AspNetUsers"`. On a host without the
  dev accounts, play the institution half only.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /admin/institutions/1 (the Demo Institution) moved to
  /not-found, "Page not found", as /admin/institutions/999 does. The dev trainee's /admin/users/caedc7eb-… stays at its
  address, tab "User · Wombat", trail Home › Users › User: "User unavailable / The user could not be found or is
  outside your scope." and Back to users, the same as an unknown id (00000000-…-0001). Neither reveals that the record
  exists.
Gap: none

### Step A.5.6 — Another registrar's activity, by id
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Open the address of Dr Mahlangu's submitted Mini-CEX from A.2.7.
Expect: The page is headed "Activity unavailable" and reads "This activity does not exist, or you cannot open it.", with
  Go to My activities: the same page an id that does not exist shows, so nothing of the activity is shown and nothing
  tells her it exists (T101, C7).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /activities/28 (Dr Mahlangu's requested Mini-CEX) stays at its
  address, tab "Activity unavailable · Wombat", trail Home › My activities › Activity unavailable (My activities lit):
  the heading "Activity unavailable", "This activity does not exist, or you cannot open it." and Go to My activities
  (/activities/mine). /activities/99999 reads word for word the same; nothing of the activity is shown.
Gap: none

### Step A.5.7 — A data-rights request that is not his, by id
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): /admin/data-rights/01a102cd-441c-7af9-8716-ce643e7fdc37
  (Mahlangu's id, last digit 6 changed to 7), tab "Data rights request · Wombat", trail Home › Data rights requests ›
  Data rights request: the alert "You are not authorized to access this data-rights request." above "Not found / The
  data rights request was not found.", and Back to list. Her real id opens her Rejected request. No other institution
  holds a request.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Signed in, /Error: status 200, tab "Nothing went wrong ·
  Wombat", "Nothing went wrong", "This is Wombat's error page, opened directly. No request failed, so there is nothing
  to report." and Go to Home; no reference and no Try again. Signed out (a fresh browser) it reads the same at /Error
  under the signed-out bar, with no sign-in first. At 390 px nothing scrolls sideways.
Gap: none

### Step A.5.9 — Dr Patel's menu offers nothing unbuilt
Role: Assessor — Dr Mohammed Patel
Route: /
Do: Read the menu.
Expect: Home and Activity inbox, then My data rights. There is no Recent activities: the flow 01 pick dropped it, and
  the nav links to no page that is not built (DESIGN.md § The NavMenu).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Acting as Assessor"; the menu reads Home and Activity inbox,
  then My data rights. No Recent activities, and no link to an unbuilt page.
Gap: none

### Step A.5.10 — Mr Smit's stalled requests
Role: Coordinator — Mr Pieter Smit
Route: /
Do: Read the dashboard's "Stalled requests" card, then look for a stalled-work page in the menu.
Expect: The card lists Dr du Plessis's portfolio review, still awaiting review since Step 3.30 aged it, and its row
  links to the activity's page (T297). No page chases a stalled request: no reminder and no reassignment. The menu
  offers no stalled-work page: it is flow 06's, and the nav links to no page that is not built.
Note: Dr Mahlangu's request from A.2.7 has waited 6 days, under the card's 7, so it is not listed.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): No stand-in was needed: Act 3 had aged Dr du Plessis's
  portfolio review (activity 10, UpdatedOn 2026-09-25). The "Stalled requests" card lists "Portfolio and Logbook Review
  (Paediatrics) — Pieter du Plessis 25 Sept", its type a link to /activities/10. Mahlangu's Mini-CEX 28 (6 days) is not
  listed. The menu reads Home, Decisions due, MSF campaigns, Committee reviews, Data rights requests, My data rights:
  no stalled-work page.
Gap: none

### Step A.5.11 — Dr Botha's menu offers no Programme trainees
Role: CommitteeMember — Dr Sarah Botha
Route: /
Do: Read the menu.
Expect: "Acting as Committee member", with "Switch to Assessor" under it, over Home, Committee reviews and Decision
  panels, then My data rights. Programme trainees is flow 06's, and not offered before it is built.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Acting as Committee member", with "Switch to Assessor" under
  it, over Home, Committee reviews and Decision panels, then My data rights. No Programme trainees.
Gap: none

### Step A.5.12 — Dr Mokoena's menu offers no Programme trainees or STAR review queue
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the menu.
Expect: Home, Decisions due, Committee reviews and Decision panels, then My data rights. Programme trainees is flow
  06's and the STAR review queue flow 09's; the menu offers neither before it is built.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Acting as Speciality admin", no switch, over Home, Decisions
  due, Committee reviews and Decision panels, then My data rights. Neither Programme trainees nor STAR review queue.
Gap: none

### Step A.5.13 — devadmin's menu has no System page
Role: Administrator — devadmin
Route: /
Do: Read the menu.
Expect: The grouped menu of Step 1.1, with no System item: the flow 01 pick dropped it, with its placeholder.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): "Acting as Administrator", no switch, over Home; Platform:
  Scheduled jobs, Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users,
  Invitations; Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee
  reviews, Decision panels; then My data rights. No System item.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Institutions from the Organisations group, lit, subtitled
  "Maintain institution records and drill into their speciality structure.", with Create institution in the header.
  Rows: Demo Institution (DEMO, admin@demo.local, Active, 2026-09-30 16:04) and Kgosi Kgari Teaching Hospital (KGK,
  paeds-admin@kgk.wombat.local, Active, 16:06), each with "Edit <name>". KGK's page, "Edit institution" (trail Home ›
  Institutions › Edit institution), offers Name, Short code, Contact email, Status with the Active box and its help,
  then Cancel, Deactivate and Save. Contact email set to paediatrics@kgk.wombat.local and saved: "Institution saved."
  took the focus. At 390 px nothing scrolls sideways.
Gap: F-A.6.1a, T326 (still: the subtitle promises a speciality drill-down that no row offers)

### Step A.6.2 — devadmin reads the system health card
Role: Administrator — devadmin
Route: /
Do: Read the dashboard's System health and Users across institutions cards.
Expect: Database connection is green. The card's other two lines, Email queue and Last nightly job, are amber whatever
  their state, and each carries a task id: they are stubs, tied neither to the mail queue nor to Scheduled jobs
  (reported). The users card counts registered accounts.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Home, "Administrator · Semester 2, 2026". System health:
  Database connection with status-dot ok, and "Email queue (T012)" and "Last nightly job (T024)" each with status-dot
  warn, although every job last Succeeded. Users across institutions reads "23 registered users"; AspNetUsers holds 25
  rows, 23 of them with a role. Each dot is an empty span with no text or label.
Gap: F-A.6.2a, T327 (still: two stub lines, as the Expect records). F-A.6.2b, T327 (still: the count is of role
  holders, leaving out Dr Molefe and the erased account). F-A.6.2c, T327 (still: each state is a colour alone).

### Step A.6.3 — Prof Mbatha edits her own institution
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /
Do: Open KGK's page by its address (no nav link leads there). Set its contact email to
  `hod.paediatrics@kgk.wombat.local` and save. Read Status, then look at Back to home and Cancel.
Expect: "Institution saved." takes the focus. Status reads "Active" as text, with "Set by a global administrator."
  beneath: there is no Active box and no Deactivate, because an institution's state is the Administrator's alone
  (T302). "Back to home" and Cancel both lead to `/`. SQL:
  `SELECT "Id","ContactEmail","IsActive" FROM "Institutions" WHERE "ShortCode"='KGK'` gives
  `2|hod.paediatrics@kgk.wombat.local|t`.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): KGK's page by its address: "Edit institution", trail Home ›
  Edit institution, nothing lit, headed by "Back to home". It offers Name, Short code, Contact email, Status as text
  ("Active", then "Set by a global administrator."), Cancel and Save: no box, no Deactivate. Contact email set to
  hod.paediatrics@kgk.wombat.local and saved: "Institution saved." took the focus. Back to home and Cancel each lead to
  / (Cancel followed: "Institutional admin · Semester 2, 2026"). SQL gives 2|hod.paediatrics@kgk.wombat.local|t.
Gap: none

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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Dr Patel was first signed in on a second browser, on his
  Activity inbox. Users → "Manage Mohammed Patel": Status Active. The Lockout card reads "Locking out the user prevents
  sign-in immediately. Until they are reactivated:", then: no committee panel; not a current trainee (with what that
  leaves out); no activity reminders or weekly digests; cannot be named as an activity's assessor; then "Their records
  stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the
  lockout." Lock out user, no dialog: "User locked out." took the focus, Status reads "Locked out", and the card offers
  only Reactivate user.
Gap: none

### Step A.6.5 — Dr Patel's session ends, and he cannot sign in
Role: Assessor — Dr Mohammed Patel
Route: /account/session-ended → /account/login
Do: Wait up to a minute on the page he had open, then sign in.
Expect: The tab leaves for the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Signing in is refused: "Invalid email or password." (T287).
Note: A locked account, locked by an administrator or by wrong passwords, is refused in a wrong password's words, so the
  page says nothing about which accounts are locked (T287, in T339).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): The second browser's inbox left 55 s after the lock for
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Log an activity, Mini-CEX (Paediatrics)
  (/activities/new?type=mini_cex_cpsa): the Assessor list offers Sarah Botha, Fatima Khumalo, David Naidoo and Thandi
  Zulu (each with the address), not Dr Patel and not herself. Left by Home without saving; nothing was written.
Gap: none

### Step A.6.7 — Prof Mbatha reactivates Dr Patel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users/{UserId}
Do: Reactivate Dr Patel.
Expect: His status reads Active, the card offers Lock out user again, and the focus moves to the result.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): His page reloaded with Status "Locked out". Reactivate user, no
  dialog: "User reactivated." took the focus, Status reads Active, and the card offers Lock out user again. Roles:
  Assessor with Remove.
Gap: none

### Step A.6.8 — Dr Patel signs in again
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard. "Waiting for you", in the warning stripe, is badged "1 waiting, 1 overdue"
  and lists Dr du Plessis's Portfolio and Logbook Review, from Pieter du Plessis, Awaiting review with Overdue beside
  it, "Waiting 8 days", as his inbox lists it, "8 days" over "since `D−8` … SAST" (T297, T335, T350). Recent decisions
  lists the three he decided on `D`. Dr Dlamini's assessor list names him again (checked at A.7.1).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Signed in afresh he lands on /, "Assessor · Semester 2, 2026".
  "Waiting for you", in the warning stripe, badged "1 waiting, 1 overdue", lists "Portfolio and Logbook Review
  (Paediatrics) · PAED-015 · 2026-10-02", from Pieter du Plessis, Awaiting review and Overdue, "Waiting 8 days", linked
  to /activities/10, with Open Activity inbox; the inbox lists it "8 days" over "since 2026-09-25 17:22 SAST". Recent
  decisions lists Dr Dlamini's Mini-CEX 26, Dr Molefe's DOPS 14 and Dr Mahlangu's DOPS 11, each Completed 2026-10-03,
  with All your decisions.
Gap: none

### Step A.6.9 — devadmin opens his own account
Role: Administrator — devadmin
Route: /admin/users → /admin/users/{UserId}
Do: Find his own account in Users and open it.
Expect: A note says it is his own account, so he cannot change its roles, lockout or password here, and another
  administrator can change his roles or lockout. It links to Change password on his own account. No Remove, Add role,
  Reset password or Lockout is offered (T278).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): Users, searched for "devadmin": "Demo Administrator |
  devadmin@wombat.local | Global | Administrator | Active | Manage". Manage opens "Demo Administrator", "Your own
  account's summary, roles and pending invitations.", with the note "This is your own account, so you cannot change its
  roles, lockout or password here. Another administrator can change your roles or lockout. Change your password from
  your account.", its link to /account/change-password. Cards: Account summary (Global (no institution), Active), Roles
  (Administrator, no Remove, no Add role) and Pending invitations. No Reset password or Lockout card, and no button.
Gap: none

### Step A.6.10 — The health endpoint
Role: Anonymous — the uptime monitor
Route: /health
Do: Request `/health` without signing in.
Expect: Status 200 with the body "Healthy". The check includes the database (T097), and the body stays terse because
  the endpoint is public.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): A request without cookies: HTTP 200, text/plain, body
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 1280 px, keyboard only. Tab to Log an activity, Enter: the
  picker, the focus on the h1 with no ring. Tab walks the "Rated by an assessor" links (Case-Based Discussion … DOPS,
  then Mini-CEX (Paediatrics)), each with the 2px #2D6CDF ring; Enter opened /activities/new?type=mini_cex_cpsa, the
  focus again on the h1, no ring. Tab: "Choose another type", then EPA, Assessor, Date observed (three segments and
  Chrome's picker), Clinical setting, Presenting problem, Case complexity, Submit, Save draft. Each control is
  required, its label "<label> *"; Assessor's and Date observed's help is in aria-describedby; every control shows the
  ring. The EPA list offers 9 (PAED-001 to 004, 006 to 008, 012, 013). The empty Submit opened activity 29 as a Draft:
  "Saved as a draft, but not submitted. Fix the 6 fields below and submit again." took the focus, each of its six lines
  a link to its field (#epa_id-in …); the six fields are aria-invalid and name it. On the draft, by keyboard: PAED-002;
  the button read Submit until Dr Patel (listed again after A.6.7) was named, then "Submit to Mohammed Patel";
  2026-09-13 typed put "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the
  encounter is recorded as late." in its status region (also on a fresh form); Ward, a presenting problem, Moderate;
  the line above the actions ends "Filed today, 20 days after the encounter: it will be recorded as late." Submit to
  Mohammed Patel: "Submitted. It is now Requested. It is in Mohammed Patel's Activity inbox." took the focus; history
  "Submit Draft → Requested … Filed 20 days after the encounter". No mail. Typed at 40 ms a key, the presenting problem
  kept every character.
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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar: Wombat, "Acting as Assessor", Menu. Menu
  opens Home, Activity inbox and My data rights (44 px rows), and at its foot "Mohammed Patel" and Sign out; Close
  folds it, and following Activity inbox folds it too. Home: "Waiting for you" badged "2 waiting, 1 overdue" (du
  Plessis's review, Overdue; Dr Dlamini's Mini-CEX, Requested). The inbox: nothing scrolls sideways (390 in 390); its
  rows stack as blocks (17 to 373), the review first, then "Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13, from Anele
  Dlamini", each row one link named for it, no Open button; the pager's Previous and Next are 44 px. On activity 29 the
  trail is one 44 px "Activity inbox" link; Who has it now reads "Your move. Anele Dlamini asked you on 2026-10-03
  19:39 SAST." Her request is read-only and folded: "Request", "Filled in by Anele Dlamini, 2026-10-03" and Show,
  reading Hide while open (the fields inside fit, 49 to 341). Entrustment comes next, his first section: six rung
  radios abreast (1, 2, 3a, 3b, 4, 5, each 51 px, 33 to 357), "What each rung means" folded; then the three feedback
  fields (33 to 357). Complete, Discard changes ("Nothing to discard yet." under it) and Decline stack full width (16
  to 374), each 44 px; About sits under them. History folds into "All 2 moves", opened as stacked blocks. Rated 3b (its
  descriptor under the row), wrote the three feedback fields, Complete: "Completed. 1 more waits for you." took the
  focus, then the review's row (Overdue, "Waiting 8 days, since 2026-09-25 17:22 SAST."), Open the next and Back to
  Activity inbox (each 44 px, full width). The completed page shows the request open with nothing folded but the
  history (open), "Rated 3b. Credited 1 item to PAED-002.", no field or move left for either of them. Nothing scrolls
  sideways. No mail.
Gap: none (T323's sideways scroll and narrow inputs no longer occur: the inbox stacks, and the request fold and About
  fit at 390)

### Step A.7.3 — Dr Dlamini on her phone
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress, My activities and My data rights from the menu.
Expect: The dashboard's cards stack. On My progress, each EPA's figures and trajectory fit the width. My activities'
  rows stack, each cell but the link and the state labelled by its column (T342), and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar reads "Acting as Trainee"; the menu's eight
  rows are 44 px, with Anele Dlamini and Sign out at its foot. Home's cards stack at 16 to 374. My progress (the menu
  folds on the way): four trajectory charts 308 px wide, the Annexure A table (500 px) scrolling in its 358 px
  container, nothing sideways; its rating links ("4 — open this PAED-001 rating") are 21 px tall. My activities:
  nothing needs her, so no Needs you card; All activities (6) stack each row as a block 324 px wide, "Who has it now:"
  and "Credit:" labelling their cells, the link (45 px, named "Mini-CEX (Paediatrics) · PAED-012 · 2026-10-03, to
  Mohammed Patel") and the state unlabelled; the table fits (356 px in 358). My data rights: the cards stack; Your
  requests (367 px) scrolls in its 358 px container, its Download in reach.
Gap: F-A.7.3a, T328 (still: the rating links in the entrustment-against-Annexure-A table are 21 px tall, under T086's
  24 px, on My progress and on a committee review).

### Step A.7.4 — Dr Molefe's record on her phone
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: / → /portfolio/progress
Do: At 390 px, sign in, read Home, and open My progress from the menu.
Expect: Home says that she completed her programme and points her to My progress, as at Step 5.21 (T252). The phone
  bar shows no "Acting as", since she holds no role, and the menu holds Home, My progress and My data rights, then her
  name and Sign out at its foot. My progress shows her read-only record of past periods (T252), its cards fitting the
  width.
Note: The code shows one card, "Your training record", pointing to My progress (T335), but does not say that or when she
  completed her programme (T311).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar shows Wombat and Menu, with no "Acting as".
  The menu holds Home, My progress and My data rights, with Lerato Molefe and Sign out at its foot (44 px). Home shows
  one card, "Your training record": "You hold no role at the moment. Your training record is kept, read-only: your
  progress in each period, and your portfolio to export." and "Open My progress →". My progress has no controls: "You
  completed your programme on 3 October 2026. This page is your record of it and is read-only: …"; Started 15 January
  2023, Completed 3 October 2026; cards at 16 to 374, its 524 px table scrolling in its container, three charts 308 px
  wide; nothing scrolls sideways.
Gap: F-A.7.4a, T311 (still: Home points her to My progress but does not say that she completed her programme)

### Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its invitee table (counts by respondent group: the page never
  lists an address) scrolls within its card. Decisions due's summary scrolls sideways, and a keyboard can scroll it, as
  a labelled, focusable region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar reads "Acting as Coordinator"; Home's cards
  stack. MSF campaigns: 2 rows (du Plessis Withdrawn; Molefe Released, 7 invited, 6 responded); the table (683 px)
  scrolls in its container; each action is named ("View campaign: the campaign for Lerato Molefe (Default MSF, closing
  2026-10-17)"). Campaign 1 keeps its gutter (card 16 to 374); its invitee table counts by group ("This page never
  lists who was invited or which of them responded.") and fits its card (306 px in 308); no address on the page.
  Decisions due: its summary is a focusable region labelled by its heading "By EPA" (656 px in 306); ArrowRight
  scrolled it 120 px; its ring is 2px #2D6CDF. The data-rights queue: Type and Status stack above the table (623 px,
  scrolls in its container), with Apply filters in the page header above them (y 141, the filters 272 and 366). Nothing
  scrolls sideways.
Gap: F-A.7.5b, T328 (still: Apply filters comes before the filters it applies).

### Step A.7.6 — Dr Zulu on her phone
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee reviews and Dr Molefe's final review.
Expect: The review's cards stack. The evidence tables scroll inside their containers, and each EPA's trajectory chart
  fits the width (T166).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar's "Acting as Committee member" wraps and the
  bar grows to 59 px; the menu's head adds "Switch to Assessor". Committee reviews: the table (875 px) scrolls in its
  container. Review 7 (Dr Molefe's pre-graduation review): cards at 16 to 374 with nested cards at 41 to 349; its six
  tables (454 to 784 px) each scroll inside a 274 to 308 px container; the three trajectory charts are 274 px wide and
  fit, and nothing scrolls sideways.
Gap: F-A.7.6a, T323 (still: the charts fit only by scaling, their labels about 5 px on a phone). The 21 px rating links
  are A.7.3's finding.

### Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open her dashboard, Decision panels, Decisions due and, by its address, Entrustment decisions.
Expect: The dashboard's coverage cards stack. The panels list and the decisions list scroll inside their containers
  (T226).
Note: No menu item leads to Entrustment decisions for her role; only the InstitutionalAdmin dashboard links to it. On
  it nothing is lit, and the trail reads Home › Entrustment decisions.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar reads "Acting as" over "Speciality admin".
  Home stacks Pending reviews (3 activities awaiting review), Trainees in programme "2 active / 3 inactive" and
  Curriculum coverage for Semester 2, 2026, at 16 to 374. The menu: Home, Decisions due, Committee reviews, Decision
  panels, My data rights (44 px rows), no Entrustment decisions. Decision panels: the list (436 px) scrolls in its
  container and the member tables fit (306 in 308). Decisions due as at A.7.5. /admin/entrustment-decisions by its
  address: 18 rows, the table (764 px) scrolling in its container; nothing is lit; the trail reads Home › Entrustment
  decisions at 1280 px and is one 44 px Home link at 390. Nothing scrolls sideways.
Gap: none

### Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due
Do: At 390 px, open his dashboard, then Committee reviews and Decisions due from the menu.
Expect: As for Dr Mokoena, scoped to his sub-speciality. The folded bar reads "Acting as" over "Sub-speciality admin",
  in two lines: the bar grows, and the role is never cut. His menu reads as hers (A.5.12), each row 44px.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The folded bar reads "Acting as" over
  "Sub-speciality admin" on two lines, and the bar grows to 59 px; nothing is cut. Home matches Dr Mokoena's (3, "2
  active / 3 inactive", the coverage). His menu reads as hers, each row 44 px. Committee reviews: the table (875 px)
  scrolls in its container. Decisions due: the focusable summary region (656 px in 306). Nothing scrolls sideways.
Gap: none

### Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK Teaching Session
  Log in the builder.
Expect: The user page's cards stack, and its buttons wrap rather than overflow. The trainee profile form stacks its
  fields. The builder's editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
  stays usable.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar reads "Acting as Institutional admin"; the
  menu's rows are 44 px. Users: the table (807 px) scrolls in its container. Dr Dlamini's account: the cards stack at
  16 to 374; Back to users, Add role, Show (44 px), Reset password and Lock out user all sit inside 16 to 349. Roles
  reads "TraineeSystem-managed". Trainees: the two tables (501 and 475 px) scroll in their containers. Her profile
  (/admin/trainees/edit?id=3) stacks its four fields, 41 to 349. The builder (/admin/activity-types/23, "Edit KGK
  Teaching Session Log"): the editor and Live preview stack (the preview at 49 to 341, y 4083); the tab bar wraps
  Credit to a second row, each tab 42 px, plain buttons with no role="tab", aria-selected or aria-pressed. Nothing
  scrolls sideways.
Gap: F-2.28b, T323 (still: "TraineeSystem-managed"). F-A.7.9a, T280 (still: the builder's tabs expose no selected
  state).

### Step A.7.10 — Dr Kruger on his phone
Role: CollegeAdmin — Dr Anton Kruger
Route: / → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: At 390 px, open EPAs, then Curricula and the items of Paediatric EPA Curriculum 11.2.
Expect: The EPA list and the items table scroll inside their containers (DESIGN.md § Table system). Each item's row
  actions stay reachable, and the item editor's inputs keep usable widths (T198, T226).
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar reads "Acting as College admin"; Home shows
  the National catalogue card; the menu reads Home, Specialities, EPAs, Curricula, Activity types, My data rights.
  EPAs: 16 rows, the table (736 px) scrolling in its container. Curricula: 3 rows (885 px, in its container). The items
  of 11.2 (/admin/curricula/4/items): 16 rows, the table (772 px) in a 308 px container; the row actions are named
  "Edit PAED-001" and "Remove PAED-001". Edit PAED-001 opens the editor inside the table as a colspan=9 row 772 px
  wide: its inputs are 358 px (the EPA 740), so each half is reached by scrolling the table sideways. "Cancel editing
  PAED-001" closed it; nothing was saved. The page never scrolls sideways.
Gap: F-A.7.10a, T323 (still: the item editor sits in the table's scroll container and must be scrolled sideways)

### Step A.7.11 — devadmin on his phone
Role: Administrator — devadmin
Route: / → /admin/jobs → /admin/jobs/runs → /admin/institutions
Do: At 390 px, open Scheduled jobs, Run history and Institutions.
Expect: The jobs table scrolls inside its container, with each Run now and toggle reachable. The run-history filters
  stack.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px. The bar reads "Acting as Administrator"; the menu's
  rows are 44 px. Scheduled jobs: the table (655 px) scrolls in its container; each row's "Disable <key>" (28 px) and
  "Run now: <key>" are in reach. Run history: the trail is one 44 px "Scheduled jobs" link; Key, Status, From and To
  stack (41 to 349), Filter under them, the table (695 px) under that, scrolling in its container. Institutions: 2 rows
  (KGK with hod.paediatrics@kgk.wombat.local, Active); the table (681 px) scrolls in its container, each Edit in reach.
  Nothing scrolls sideways.
Gap: none

### Step A.7.12 — The anonymous pages on a phone
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's portfolio PDF from Act 5 by
  its hash.
Expect: The sign-in card fits the width. Every field, toggle, button and link on the sign-in and forgot pages is
  44 px tall; Sign in is full width; the h1 is 1.5rem. The verify page's result fits the width.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 390 px, signed out. Sign-in: the card spans 41 to 349;
  Email, Password, its Show toggle, Sign in and "Forgotten your password?" are each 44 px tall; Remember me's label
  fills a 44 px row (its box 20 px); Sign in is full width (41 to 349); the h1 is 24 px (1.5rem). Forgotten password:
  the h1 24 px, Back to sign in 44 px. Verify: Dr Molefe's newest export hash (c52d1b16…), Verify: ?hash=…&check=1,
  "Export verified / This PDF was generated by Wombat on 2026-10-03 16:59 UTC. / Trainee ID … / Exported by … / Filter:
  2023-01-15 to 2026-10-03 / File: portfolio-c52d1b16cf57.pdf", fitting the width. Nothing scrolls sideways.
Gap: none

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
Actual (2026-10-03, T350 replay, wombat_scenario_t350): No screen reader can be driven here, so the accessibility tree
  and DOM of review 7 were read in its place, at 1280 px, opened from Committee reviews. The focus arrives on the h1
  "Committee review". Headings: the h1, then h3s Sampling concentration warnings, Review, Decision (h4 "Graduate
  (programme complete)"), Entrustment against Annexure A, Multi-source feedback by EPA, Agenda, Pending entrustment
  decisions, Evidence snapshot (an h4 per EPA and "Not about a single EPA"), Rating trajectory by EPA (an h4 per EPA)
  and Appeals; no h2. All 9 tables have a caption and column headers; the evidence rows have row headers. Each chart is
  role=img "Rating trajectory for PAED-001" (010, 012), followed by a visually hidden table captioned the same, one row
  per observation (3, 1, 2). Badges are plain words. Only Multi-source feedback by EPA, Agenda and Evidence snapshot
  are named regions. The warnings' dismiss button's only name is "×".
Gap: F-A.7.13a, T280 (still: the "×" button has no name). F-A.7.13b, T280 (still: only 3 of the cards are regions).

### Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on a committee review opened from Committee reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
Note: Since 2026-09-27 (T335, flow 01; T322), `Design/ContrastTests` computes every pair from app.css's tokens, and
  DESIGN.md § Design tokens lists them. The badges and the alerts are body text on their tints (11.2:1 or more), with
  the state's colour on the edge. White (`--on-fill`) is 5.95:1 on the danger button and 5.88:1 on the success one. The
  focus ring is 4.61:1 on the page background, and an input's border 3.60:1 there. The nav's controls take
  `--nav-focus-ring`, 14.07:1 on the gradient's start.
Actual (2026-10-03, T350 replay, wombat_scenario_t350): At 1280 px, WCAG ratios computed from the rendered colours (no
  axe in this browser). Sign-in card: #333 on white 12.63; the tagline #686F77 5.09; link #0B5CAB 6.70; Sign in, white
  on #2D6CDF, 4.86; input border #7B848D 3.80; the Show toggle pressed (aria-pressed true), white on #2D6CDF, 4.86. The
  nav gradient (#052767 to #3A0647): items #D7D7D7 9.78 and 11.25, group labels 8.00 and 9.21, the current item (white
  on the .32 fill) 5.27 and 6.08. Muted on the page (#F8F9FA) 4.83. Committee reviews shows states as plain text;
  review 7's badges: "No decision" #333 on #F1F3F5 11.36, "At or above" and "Decided" #333 on #E8F5E9 11.23. "Name
  saved." #333 on #E8F5E9 11.23 (edge 5.88); "Your password was not changed. The password confirmation does not match."
  #333 on #FFF5F5 11.81 (edge 5.95), nothing changed. Lock out user, white on #B8322A, 5.95 (not pressed); Publish,
  white on #1A7340, 5.88 (disabled with no draft); primary 4.86. The focus ring #2D6CDF: 4.86 on white, 4.61 on the
  page. An invalid input's border 5.95.
Gap: none

## Outcome state

Replay check (2026-10-03, T350 replay, wombat_scenario_t350): match. With <appendix start> = 2026-10-03 19:24:00+02
  (A.1.1 began 19:24:37): the four KGK requests (0,5,1) (1,3,1) (2,4,1) (4,4,1); one erasure (pseudonym, no email, name
  or password, no institution, lockout infinity) with 0 roles, 0 logins and 0 activities on the original id; the
  profile inactive since 2026-10-03; reviews (false,4) (true,6) (true,7,true); nine jobs enabled, each run once by
  devadmin and the digest twice, 0 failed today; 0 SSO mappings; Dlamini opted out of optional processing only, Patel
  not locked, Smit not opted out; KGK hod.paediatrics@kgk.wombat.local and active; audit Submit 4, Withdraw 1, Approve
  2, Reject 1, RunScheduledJobNow 10, SetUserLockout 2, ResetUserPassword one false and one true (the password
  [REDACTED]); 0 unredacted submits. Also as stated: Mahlangu has draft 27 and requested 28; Dlamini's 29 is completed
  with DaysAfterEncounter 20. Snapshot recovery/scenario-t350-post-actA.dump.

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

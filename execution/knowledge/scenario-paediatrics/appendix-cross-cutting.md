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
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step A.1.4 — Mr Smit cannot download what he approved
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights/download/{id:guid}
Do: Open the download address of the request he has just approved. Its id is in the detail page's address.
Expect: Page not found, with status 404. Nothing on the page says that the request exists. The reviewer approves; only
  the data subject, or a global Administrator, collects the bundle (T112).
Actual:
Gap:

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
Actual:
Gap:

### Step A.1.6 — Dr du Plessis asks for access, then withdraws the request
Role: Trainee — Dr Pieter du Plessis
Route: /account/data-rights
Do: Submit an Access request with the reason "Everything held about me." Then withdraw it from "Your requests".
Expect: The request lists as Submitted, with a Withdraw named for its type and time. Once withdrawn, the page says
  "Request withdrawn.", the focus moves to that message, and the row reads Withdrawn and offers nothing.
Actual:
Gap:

### Step A.1.7 — Dr Mahlangu asks for a correction
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Submit a Rectification request with the reason "My programme start is recorded as 15 January 2026; I started work
  on 19 January."
Expect: The request lists as Submitted, with a Withdraw.
Actual:
Gap:

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
Actual:
Gap:

### Step A.1.9 — Dr Mahlangu reads the decision
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Reload Data Rights and read her request.
Expect: The request reads Rejected and offers nothing: no Withdraw, no Download. The page shows neither the decision
  note nor who decided, and no email told her of the decision.
Note: The confirmation at A.1.7 promised a notification (reported at A.1.2).
Actual:
Gap:

### Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period holding `D`, Scheduled on `D`,
  with Formative only ticked. Note the review's id from its address.
Expect: The review opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled, as at Step
  4.49. It is open, so the erasure at A.1.12 has a review to end.
Actual:
Gap:

### Step A.1.11 — Dr Ndlovu requests erasure
Role: Trainee — Dr Sipho Ndlovu
Route: /account/data-rights
Do: Submit an Erasure request with the reason "I am leaving the programme and want my personal data removed." Stay
  signed in in this browser.
Expect: The request is accepted although a review of him is scheduled: no review's state refuses an erasure request
  (T258). It lists as Submitted, with a Withdraw.
Actual:
Gap:

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
Actual:
Gap:

### Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Please sign in again."
  (T279). Signing in is refused in the words an unknown address gets, "Invalid email or password.", with no mention
  of an erasure (T156). No email is sent to him.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step A.2.2 — devadmin disables a job and enables it again
Role: Administrator — devadmin
Route: /admin/jobs
Do: Disable msf-campaign-auto-close, try to run it now, then enable it again.
Expect: Disabling says "Job 'msf-campaign-auto-close' disabled." The row then reads "—" for its next run, its toggle
  says Enable, and its Run now is unavailable. Once enabled again, its next run is the next hour. The scheduler does not
  run a disabled job. The audit log records both changes.
Actual:
Gap:

### Step A.2.3 — Mr Smit opts out of digest emails
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."
Actual:
Gap:

### Step A.2.4 — devadmin runs the weekly digest, and Mr Smit is skipped
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the application log.
Expect: "Job 'weekly-coordinator-digest' dispatched."; the row's last run is now, and it reads Succeeded. The log holds
  "WeeklyCoordinatorDigestJob: digests sent n; coordinators skipped: …", with at least one counted as "opted out of
  digest emails". No "Your weekly Wombat digest" mail greets Pieter.
Note: Each mail is logged as "Stub email <reference> … not sent", with its subject and text and no address (T282). The
  Demo Institution's dev coordinator may be sent a digest; ignore it.
Actual:
Gap:

### Step A.2.5 — Mr Smit opts back in
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Untick "Opt out of digest emails" and save.
Expect: "Processing preferences saved."
Actual:
Gap:

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
Actual:
Gap:

### Step A.2.7 — Dr Mahlangu leaves a draft and a request waiting
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int} → /activities/new → /activities/{ActivityId:int}
Do: File two Mini-CEX (Paediatrics) activities, each with every request field filled, an EPA the list offers, and Dr
  Khumalo as assessor.
  - The first: the encounter on `D−3`, then Save draft.
  - The second: the encounter on `D−2`, then Submit.
Expect: The first reads "Draft saved. It has not been submitted." The second reads "Submitted. It is now Requested.",
  and is in Dr Khumalo's inbox. No email is sent: Wombat mails nobody when an activity moves (Step 3.3).
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step A.2.10 — devadmin reads the run history
Role: Administrator — devadmin
Route: /admin/jobs → /admin/jobs/runs
Do: Open Run history. Filter it:
  - by the key "weekly-coordinator-digest";
  - by the key "digest";
  - by the status Failed;
  - from today.
Expect: The runs are listed newest first, each with its key, start, finish, duration, status badge, who triggered it
  and any error. A run by hand names devadmin's user id; one the scheduler ran reads "scheduler". Filters apply only
  when Filter is pressed.
  - The full key shows the digest's runs, the two by hand among them, and no other job's.
  - "digest" alone matches nothing: the filter wants the whole key.
  - Failed shows "No runs found".
  - From today shows today's runs, those of this appendix among them. The date is read as a UTC day.
Actual:
Gap:

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
Actual:
Gap:

### Step A.3.2 — The sign-in page offers no institutional sign-in
Role: Anonymous — a KGK consultant
Route: /account/login → /account/sso-challenge/{providerKey} → /account/login
Do: Look for an institutional sign-in button. Sign in with a wrong password. Then open the challenge address for a
  provider called `kgk`.
Expect: The page offers only an email, a password, Remember me and the "Forgotten your password?" link. There is no
  "or" divider and no "Sign in with …" button. The wrong password reads "Invalid email or password." and points to no
  institutional button (T156). The challenge for a provider that is not configured returns to the sign-in page, which
  says "Unknown SSO provider."
Actual:
Gap:

### Step A.3.3 — The callback and link pages with no institutional sign-in in progress
Role: Anonymous — a KGK consultant
Route: /account/sso-callback → /account/login → /account/link-external
Do: Open the callback address, then the link-your-account page, directly.
Expect: The callback returns to the sign-in page, which says "External login information was not available." The link
  page says "Your institutional sign-in has expired. Start again from the sign-in page." It offers Back to sign in and
  no password field (T149).
Actual:
Gap:

## A.4 — Account self-service

### Step A.4.1 — Dr Botha reviews and edits her account
Role: CommitteeMember — Dr Sarah Botha
Route: / → /account/profile
Do: Open My Account from her address in the top row. Clear her last name and save. Then put "Botha" back and save.
Expect: The account summary shows her address, both her roles (CommitteeMember and Assessor) and a Change password
  link. The email field cannot be edited. The cleared last name is refused, with a required-field message beside it,
  and nothing is saved. Restored, the page says "Profile saved." and the focus moves to that message (T234).
Actual:
Gap:

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
Actual:
Gap:

### Step A.4.3 — Dr Khumalo's other session ends
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Please sign in
  again." (T279). The old password is refused ("Invalid email or password."). The new one brings her back to her
  Activity Inbox.
Actual:
Gap:

### Step A.4.4 — Dr du Plessis has forgotten his password
Role: Anonymous — Dr Pieter du Plessis
Route: /account/login → /account/forgot-password → /account/login
Do: Follow "Forgotten your password? Reset it" from the sign-in page.
Expect: A "Reset password" page, saying exactly: "Password reset is not wired yet in the rewrite. Ask an administrator
  to issue a new invitation or reset the account directly." It offers Back to sign in, and nothing else: no field, and
  no email is sent.
Note: This is a stub. Self-service reset is not built, and nothing uses the `PasswordResetEmail` template (not
  played).
Actual:
Gap:

### Step A.4.5 — Prof Mbatha resets his password
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Set a new password of 10 characters, then one that meets every rule. Record the
  second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: The Reset password card says it sets a password directly and that the user is not emailed. The 10-character
  password is refused with the rules it breaks. The second is accepted, and the field is cleared. The audit log
  records the reset with the password redacted (T101).
Note: The form enables Reset password at 8 characters; Identity requires 12 (reported).
Actual:
Gap:

### Step A.4.6 — Dr du Plessis signs in with it and chooses his own
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → / → /account/profile → /account/change-password
Do: Sign in with the password Prof Mbatha set. Then change it, from My Account, to one of his own. Record it in
  `pwd_DO_NOT_COMMIT.txt`.
Expect: He lands on his Trainee dashboard, which still says that his programme ended (Step 5.28). The change reads
  "Password updated."
Actual:
Gap:

### Step A.4.7 — Mr Smit signs out through the confirmation page
Role: Coordinator — Mr Pieter Smit
Route: /account/logout-confirm → / → /account/logout-confirm → /account/logout → /account/login → /msf/campaigns → /account/login → /msf/campaigns
Do: Open the sign-out confirmation by its address and press Cancel. Open it again and sign out. Then open MSF
  Campaigns by its address.
Expect: A "Sign out" page, with no nav, says to use its button to end the session. Cancel returns him to his
  dashboard, still signed in. Sign out lands on the sign-in page. MSF Campaigns then asks him to sign in, and after
  signing in he is brought back to it. The audit log records a Logout.
Note: No page links to the confirmation page: the nav's Logout and the top row's Sign out sign out at once.
Actual:
Gap:

## A.5 — System pages

### Step A.5.1 — A registrar opens an administrator's pages
Role: Trainee — Dr Anele Dlamini
Route: /admin/users → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Users page, then that of Scheduled Jobs.
Expect: Each time, the "Access denied" page: she does not have permission to view the page, her role does not allow
  access to the area, and "Back to home" takes her home. Nothing of the page she asked for is shown.
Actual:
Gap:

### Step A.5.2 — Prof Mbatha opens the Administrator's own pages
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled Jobs.
Expect: Access denied for both. They are the Administrator's alone, and her nav offers neither.
Actual:
Gap:

### Step A.5.3 — A forged dashboard switch
Role: Trainee — Dr Anele Dlamini
Route: /dashboard/switch/{role} → /
Do: Open `/dashboard/switch/Administrator`.
Expect: She is back on her own dashboard, viewing it as a Trainee. A switch to a role she does not hold changes
  nothing, and no other view is offered to her.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step A.5.6 — Another registrar's activity, by id
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Open the address of Dr Mahlangu's submitted Mini-CEX from A.2.7.
Expect: "Activity unavailable": the requested activity could not be loaded. Nothing of it is shown, and nothing on the
  page speaks of permission.
Actual:
Gap:

### Step A.5.7 — A data-rights request that is not his, by id
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).
Actual:
Gap:

### Step A.5.8 — The error page
Role: Trainee — Dr Anele Dlamini
Route: /Error
Do: Type `/Error`.
Expect: "Something went wrong": an unexpected error interrupted the request. If it keeps happening, she is to contact
  her administrator, quoting the request ID the page gives. Signed out, `/Error` asks her to sign in first.
Note: No failure leads here. No exception handler is configured, so an unhandled exception gives the developer page on
  dev, a bare 500 in production, and the "An unhandled error has occurred" banner inside a page (reported; that path is
  not played).
Actual:
Gap:

### Step A.5.9 — Dr Patel's Recent Activities
Role: Assessor — Dr Mohammed Patel
Route: /placeholder/{Feature}
Do: Open Recent Activities from the nav.
Expect: A page headed "Recent Activities" says it is not built yet. Its "Coming soon" card says the menu links to it so
  that the role's navigation is complete. Nothing else is offered.
Actual:
Gap:

### Step A.5.10 — Mr Smit's Stalled Activities
Role: Coordinator — Mr Pieter Smit
Route: / → /placeholder/{Feature}
Do: Read the dashboard's "Stalled requests" card, then open Stalled Activities from the nav.
Expect: Whatever the card lists, no row links anywhere, so there is no page to act on a stalled request from (Step
  3.30). Stalled Activities is the "Coming soon" stub, headed "Stalled Activities".
Note: Dr Mahlangu's request from A.2.7 has waited 6 days, under the card's 7, so it is not listed.
Actual:
Gap:

### Step A.5.11 — Dr Botha's Programme Trainees
Role: CommitteeMember — Dr Sarah Botha
Route: /placeholder/{Feature}
Do: Open Programme Trainees from the nav.
Expect: The "Coming soon" stub, headed "Programme Trainees".
Actual:
Gap:

### Step A.5.12 — Dr Mokoena's Programme Trainees and STAR Review Queue
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /placeholder/{Feature} → /placeholder/{Feature}
Do: Open Programme Trainees, then STAR Review Queue, from the nav.
Expect: Both are the "Coming soon" stub, each under its own heading.
Actual:
Gap:

### Step A.5.13 — devadmin's System page
Role: Administrator — devadmin
Route: /placeholder/{Feature}
Do: Open System from the nav.
Expect: The "Coming soon" stub, headed "System".
Actual:
Gap:

## A.6 — Platform operations

### Step A.6.1 — devadmin maintains the institution record
Role: Administrator — devadmin
Route: / → /admin/institutions → /admin/institutions/{Id:int}
Do: Open Institutions from the dashboard's Maintenance card. Read the list, open KGK, set its contact email to
  `paediatrics@kgk.wombat.local`, and save.
Expect: The list holds the Demo Institution and Kgosi Kgari Teaching Hospital. Each row shows its short code, contact
  email, status (Active) and creation time, with an Edit named for it; Create institution heads the page. Saving says
  "Institution saved." The Administrator is offered the Status box and Deactivate.
Actual:
Gap:

### Step A.6.2 — devadmin reads the system health card
Role: Administrator — devadmin
Route: /
Do: Read the dashboard's System health and Users across institutions cards.
Expect: Database connection is green. The card's other two lines, Email queue and Last nightly job, are amber whatever
  their state, and each carries a task id: they are stubs, tied neither to the mail queue nor to Scheduled Jobs
  (reported). The users card counts registered accounts.
Actual:
Gap:

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
Actual:
Gap:

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
Actual:
Gap:

### Step A.6.5 — Dr Patel's session ends, and he cannot sign in
Role: Assessor — Dr Mohammed Patel
Route: /account/session-ended → /account/login
Do: Wait up to a minute on the page he had open, then sign in.
Expect: The tab leaves for the sign-in page, which says "Your session has ended. Please sign in again." Signing in is
  refused: "Too many failed sign-in attempts. Please try again later or reset your password."
Note: An administrator's lock gets the brute-force lockout's words. Whether a locked-out person is told otherwise was
  left open (T156 review).
Actual:
Gap:

### Step A.6.6 — Dr Patel cannot be named as an assessor while locked
Role: Trainee — Dr Anele Dlamini
Route: /activities/new
Do: Start a Mini-CEX (Paediatrics) and open its assessor list. Then leave without saving.
Expect: KGK's other assessors are listed (Dr Zulu, Dr Naidoo, Dr Botha and Dr Khumalo), but not Dr Patel, and never
  herself (T102). Nothing is saved.
Actual:
Gap:

### Step A.6.7 — Prof Mbatha reactivates Dr Patel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users/{UserId}
Do: Reactivate Dr Patel.
Expect: His status reads Active, the card offers Lock out user again, and the focus moves to the result.
Actual:
Gap:

### Step A.6.8 — Dr Patel signs in again
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard, and Dr Dlamini's assessor list names him again (checked at A.7.1).
Actual:
Gap:

### Step A.6.9 — devadmin opens his own account
Role: Administrator — devadmin
Route: /admin/users → /admin/users/{UserId}
Do: Find his own account in Users and open it.
Expect: A note says it is his own account, so he cannot change its roles, lockout or password here, and another
  administrator can change his roles or lockout. It links to Change password on his own account. No Remove, Add role,
  Reset password or Lockout is offered (T278).
Actual:
Gap:

### Step A.6.10 — The health endpoint
Role: Anonymous — the uptime monitor
Route: /health
Do: Request `/health` without signing in.
Expect: Status 200 with the body "Healthy". The check includes the database (T097), and the body stays terse because
  the endpoint is public.
Actual:
Gap:

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
Actual:
Gap:

### Step A.7.2 — Dr Patel completes it on his phone
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: At 390 px, open Dr Dlamini's Mini-CEX from the Activity Inbox. Rate the supervision at `3b`, write the three
  feedback fields, and complete it.
Expect: The inbox lists it with an action named for it. On the activity, her request is read-only to him, and the
  six-rung ladder and the feedback fields fit the width. Complete and Decline are reachable without scrolling
  sideways. Once completed, it reads Completed and is read-only to both of them. Nobody is emailed (Step 3.3).
Actual:
Gap:

### Step A.7.3 — Dr Dlamini on her phone
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My Progress, My Activities and Data Rights from the folded nav.
Expect: The dashboard's cards stack. On My Progress, each EPA's figures and trajectory fit the width. The tables of My
  Activities and "Your requests" scroll inside their containers, and each row's action stays reachable.
Actual:
Gap:

### Step A.7.4 — Dr Molefe's record on her phone
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: / → /portfolio/progress
Do: At 390 px, sign in, read Home, and open My Progress from the folded nav.
Expect: Home says that she completed her programme and points her to My progress, as at Step 5.21 (T252). My Progress
  shows her read-only record of past periods (T252), its cards fitting the width.
Note: The code shows an account with no role the "No role assigned" card instead, which tells her an administrator can
  give her one (`Home.razor`; reported).
Actual:
Gap:

### Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions Due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its address table scrolls within its card. Decisions Due's
  summary scrolls sideways, and a keyboard can scroll it, as a labelled, focusable region (DESIGN.md's decisions-due
  contract). The queue's filters stack above its table.
Actual:
Gap:

### Step A.7.6 — Dr Zulu on her phone
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee Reviews and Dr Molefe's final review.
Expect: The review's cards stack. The evidence tables scroll inside their containers, and each EPA's trajectory chart
  fits the width (T166).
Actual:
Gap:

### Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open her dashboard, Decision Panels, Decisions Due and, by its address, Entrustment decisions.
Expect: The dashboard's coverage cards stack. The panels list and the decisions list scroll inside their containers
  (T226).
Note: No nav item leads to Entrustment decisions for her role; only the InstitutionalAdmin dashboard links to it.
Actual:
Gap:

### Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due → /placeholder/{Feature} → /placeholder/{Feature}
Do: At 390 px, open his dashboard, Committee Reviews and Decisions Due, then Programme Trainees and STAR Review Queue
  from the folded nav.
Expect: As for Dr Mokoena, scoped to his sub-speciality. His nav offers the same two "Coming soon" stubs as hers
  (A.5.12), and each fits the width.
Actual:
Gap:

### Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK Teaching Session
  Log in the builder.
Expect: The user page's cards stack, and its buttons wrap rather than overflow. The trainee profile form stacks its
  fields. The builder's editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
  stays usable.
Actual:
Gap:

### Step A.7.10 — Dr Kruger on his phone
Role: CollegeAdmin — Dr Anton Kruger
Route: / → /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items
Do: At 390 px, open EPAs, then Curricula and the items of Paediatric EPA Curriculum 11.2.
Expect: The EPA list and the items table scroll inside their containers (DESIGN.md § Table system). Each item's row
  actions stay reachable, and the item editor's inputs keep usable widths (T198, T226).
Actual:
Gap:

### Step A.7.11 — devadmin on his phone
Role: Administrator — devadmin
Route: / → /admin/jobs → /admin/jobs/runs → /admin/institutions
Do: At 390 px, open Scheduled Jobs, Run history and Institutions.
Expect: The jobs table scrolls inside its container, with each Run now and toggle reachable. The run-history filters
  stack.
Actual:
Gap:

### Step A.7.12 — The anonymous pages on a phone
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's portfolio PDF from Act 5 by
  its hash.
Expect: The sign-in card fits the width, and its fields and buttons are easy to tap. The verify page's result fits
  the width.
Actual:
Gap:

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
Actual:
Gap:

### Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient;
  - muted text on the page background;
  - the status badges on Committee Reviews;
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
Actual:
Gap:

## Outcome state

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

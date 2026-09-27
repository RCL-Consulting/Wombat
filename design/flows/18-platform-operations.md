# F18 — The operator keeps the platform running

**Who and why.** devadmin, the platform operator, comes to Wombat rarely. He comes to see whether the database, mail
and jobs are working; to create and keep institutions; to run, pause and read the scheduled jobs; to read the audit
trail; and to rebuild progress. Prof Mbatha reads her own institution's audit trail. An uptime monitor polls `/health`.

| | |
|---|---|
| **Mode** | **Wireframe first** for the Administrator dashboard and the System page, which must become a real health page or lose its nav item. **Fidelity** for institutions, jobs and audit (BRIEF § 2.3 step 3). |
| **People** | Administrator devadmin (display name "Demo Administrator", Step A.6.9). InstitutionalAdmin Prof Nolwazi Mbatha reads the audit log and is refused the rest (Step A.5.2). An uptime monitor, anonymous (Step A.6.10). |
| **Frequency and stakes** | Rare, for the operator. The stakes are medium: the health card is his only warning that mail is not configured. With `Email__SmtpHost` unset, every mail is logged and not sent (T157, T327). |
| **Pages** | `/`, `/admin/institutions`, `/admin/institutions/new`, `/admin/institutions/{Id:int}`, `/admin/jobs`, `/admin/jobs/runs`, `/admin/audit`, `/admin/audit/{Id:guid}`, `/admin/curriculum-progress`, `/admin/users/{UserId}`, `/placeholder/{Feature}`; also `/health`, the public health check (`coverage.md` § Pages, § Endpoints) |
| **Runbook steps** | 23. `act-1-setup.md`: 1.1, 1.6, 1.11. `act-3-operations.md`: 3.32, 3.55, 3.56, 3.57. `act-4-annual-review.md`: 4.38. `act-6-catalogue.md`: 6.38. `appendix-cross-cutting.md`: A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.2, A.5.13, A.6.1, A.6.2, A.6.9, A.6.10, A.7.11. |
| **Held** | Nothing. T302 (group 1) landed in 41be531, and the Administrator's institution page was re-captured on 2026-09-26 (§ 5.3). |

## 1. Before you paste

1. **T302 has landed** (41be531). Deactivate and Reactivate are separate Administrator-only commands, but the page
   keeps an Active box for the Administrator: Save sends a changed box as the state's own command, unticking asks
   first, and an active institution also offers Deactivate, an outline button behind a dialog naming it (T264). There
   is no Reactivate button: an Administrator ticks Active and saves ("Institution saved and reactivated."). Everyone
   else reads Status as text.
   - The institution-edit images in § 5.3 were re-captured on 2026-09-26; attach them.
2. **Open every image before you upload it** (BRIEF § 3.3).
   - Opened for this brief, showing no link or password:
     - `act-A/A.6.2-1-system-health.png`, `A.2.10-2-partial-key-nothing.png`;
     - `states/scheduled-jobs-list--dispatched.png`, `states/scheduled-job-runs-list--loaded.png`;
     - `states/audit-list--failures.png`, `states/audit-detail--payload.png`, `states/placeholder--system.png`;
     - `act-3/3.32-1-nudge-run.png`, `act-1/1.6-1-institutions-one.png`, `1.6-2-kgk-created.png`,
       `1.11-1-invitations-spent.png`.
   - The payload capture shows a user id and `[REDACTED]` values. These are scenario data (BRIEF § 1).
   - `act-1/1.11-1-invitations-spent.png` is an invitations capture. It shows "No active invitations" and no link.
3. **Some full-page captures show the sidebar part-way down the page.** In `states/scheduled-jobs-list--dispatched.png`
   and `act-3/3.32-1-nudge-run.png`, the sidebar and the top row appear again about 400 px down, over a job row. That is
   how the full-page capture stitched a fixed element, not the layout (observed; the cause is an inference). Say so in
   the chat.
4. **The nav is F01's.** The Administrator sees 20 links (BRIEF § 4). Whether the operations pages form a group is
   decided in F01. Here, decide only whether System keeps its item (question 1).

## 2. The ask (paste into Claude Design)

Paste everything inside the fence, then attach § 5.1.

````text
Design the platform operator's pages for Wombat, a work-based assessment tool for specialist registrars. It is 6
screens, each at desktop 1280×800 and phone 390×844. Do the dashboard and the System page as wireframes first, and the
institutions, jobs and audit pages at fidelity. The operator comes rarely. He comes to learn whether the platform is
healthy (database, mail, scheduled jobs), to create and keep institutions, to run, pause and read scheduled jobs,
to read the audit trail, and to rebuild curriculum progress. An institution's administrator reads her own
institution's audit trail. Use the Wombat design system set up earlier in this project: its tokens, type, Lucide
icons and components. The constraints below are binding.

FLOW 18 — Keep the platform running
GOAL: Let the operator do six things:
  1. see the platform's health and its user counts on arrival, in words;
  2. create institutions and keep their records;
  3. run, pause and resume scheduled jobs, and read their run history;
  4. read the audit trail across institutions, payloads included (an institution's admin reads her own institution's,
     without payloads);
  5. rebuild every trainee's curriculum progress;
  6. find the maintenance links.
  An uptime monitor checks /health, which answers "Healthy" or "Unhealthy" in plain text and is not a page to design.
  What is wrong today:
  - System health is three coloured dots. "Database connection" is real. "Email queue (T012)" and "Last nightly job
    (T024)" are hard-coded amber, read no data, and print internal task ids.
  - "23 registered users" skips the accounts that hold no role; there are 25.
  - Run history's key filter matches only a whole key, and the list stops at 200 runs without saying so.
  - "Triggered by" prints a user id.
  - The nav's System item opens a "Coming soon" stub.
AUDIENCE: The Administrator, devadmin ("Demo Administrator"), the platform operator. He is technical and comes rarely,
  so a page must say plainly what is wrong and where to act. Also an InstitutionalAdmin, Prof Nolwazi Mbatha, for the
  audit log only. Desktop 1280×800 and phone 390×844.
CONTENT:
  - The nine scheduled jobs: activity-draft-nudge, assessor-pending-nudge, audit-log-retention,
    entrustment-decision-expiry, msf-campaign-auto-close, msf-invitation-expiry-reminder, portfolio-export-cleanup,
    scheduled-job-run-retention and weekly-coordinator-digest.
    - Each has a description, a cron schedule, its last run and that run's status, its next run, Disable or Enable, and
      Run now.
    - A run has a key, start, finish, duration, status, who triggered it (a person, or the scheduler) and any error.
  - An institution has a name, a short code, a contact email, a status and when it was created.
  - An audit entry has a time (UTC, to the millisecond), a category, an action (a command name such as
    TransitionActivityCommand), an actor (user id, name, truncated IP), a subject, and a result (OK or FAILED, with the
    error).
    - Only an Administrator reads its payload, in which secrets appear as "[REDACTED]" and the principal as
      "[PRINCIPAL]".
    - Filters: Category, Action, Actor user ID, Subject type, From (UTC), To (UTC), Result. The pages are 50 rows long.
  - Rebuilding progress re-credits every completed activity against today's curriculum, in one transaction, after a
    confirmation. It reports five figures.
SCREENS, in order:
  1. / — the Administrator dashboard:
     - health in words (database, mail, scheduled jobs), each line saying what is wrong and, where a page exists,
       linking to where to act;
     - user counts that say what they count;
     - the maintenance links: Activity types, Users, Institutions, Curriculum progress;
     - the same at 390 px.
  2. System (/placeholder/system today): either T327's real health (database, mail, jobs) plus the maintenance links, or
     no page and no nav item. Show both options.
  3. /admin/institutions, /new and /{id} — institutions:
     - the list with one institution and with two;
     - create, and create with required fields empty;
     - the Administrator's edit, where Deactivate or Reactivate is a separate command with a confirmation naming the
       institution, not a checkbox;
     - saved, and deactivated.
  4. /admin/jobs and /admin/jobs/runs — scheduled jobs:
     - the nine jobs, a job disabled, and a job dispatched by Run now;
     - the run history: loaded; filtered by a select of the 9 job keys; empty ("No runs found"); and paged ("Showing
       1–50 of N").
  5. /admin/audit and /admin/audit/{id} — the audit log:
     - the log as one institution's admin sees it, with failures only, as the Administrator sees it (every
       institution), and empty;
     - an entry: a failure, one with its payload, and not found.
  6. /admin/users/{id} — his own account: a note that another administrator must change his roles or lockout, and a
     link to Change password; no Remove, Add role, Reset password or Lockout.
STEPS: 1.1, 1.6, 1.11, 3.32, 3.55, 3.56, 3.57, 4.38, 6.38, A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.2,
  A.5.13, A.6.1, A.6.2, A.6.9, A.6.10, A.7.11, pasted verbatim at the end.
STATES TO SHOW:
  - Loading: a skeleton under a header that shows at once.
  - Load error: the alert, with no empty state under it.
  - Refused: an institution saved with required fields empty.
  - Unavailable: Run now on a disabled job. Say why.
  - Access denied, for an InstitutionalAdmin typing /admin/institutions or /admin/jobs.
  - Not found: an audit entry outside her institution.
  - Narrow.
  - Also design these, which no replay reaches:
    - a Failed run badge, with its error;
    - Run now refused while that job is running;
    - each health line when it fails: the database down, mail not configured, a mail dropped in the last 24 hours, a
      job failed, never run, disabled or overdue.
  - Institutions and scheduled jobs always hold rows, so they have no empty state. Audit and run history do.
  - Data volumes:
    - the jobs list is always 9;
    - run history runs from 0 to thousands, since runs are kept 90 days across 9 jobs, some hourly;
    - the audit log from 0 to hundreds a day (172 rows in one act).
REQUIREMENTS FROM KNOWN DEFECTS:
  - System health (T327):
    - Each line has a label and its state in words ("OK", "Failing", "Not configured", …). A coloured dot may sit
      beside it as a secondary cue, hidden from screen readers (WCAG 1.4.1). No task ids.
    - Mail says whether mail is configured or only written to the log, the queue depth, the last mail sent and the last
      dropped, and how many were dropped in the last 24 hours. "Not configured" is a warning; a drop in 24 hours is a
      failure.
    - Jobs reads "All 9 jobs last succeeded", or names the failed, never-run, disabled (and perhaps overdue) jobs, and
      links to Scheduled jobs.
  - User counts (T327): the count says what it counts. Today it shows 23 while 25 accounts exist. The recommendation is
    every account except an erased one, with locked accounts shown apart; the operator confirms this.
  - Run history (T327): the key filter is a select of the 9 job keys, or matches part of a key. The list pages as
    "Showing 1–50 of N" and never stops silently.
  - Times (T325): every time a person reads is South African time, with the zone shown ("2026-09-26 15:14 SAST"). A
    schedule is described in the same zone. Today "daily at 07:00 UTC" sits beside a next run of "2026-09-27 09:00"
    with no zone. The audit log may keep its stored UTC, labelled, as it does now ("When (UTC)").
  - Names (T324):
    - "Triggered by" names the person, or "Scheduler". Today it prints a user id.
    - Required-field messages name the field by its label ("Short code"), not by its code name ("The ShortCode field
      is required.").
    - Home greets by name, not by email (observed, not filed).
  - T326: the Institutions subtitle says what the page does. It promises "drill into their speciality structure", but
    each row offers only Edit.
  - T302 (landed): Deactivate and Reactivate are separate Administrator commands. Today the page sends them from an
    Active box on Save and from a Deactivate button; the design may show Status with Deactivate or Reactivate as its
    own actions instead. An InstitutionalAdmin sees Status as text.
  - Destructive actions (T264):
    - Deactivate asks first in a dialog that names the institution. Its trigger is an outline button, and it is not
      offered again once the institution is inactive.
    - The audit pager keeps the focus when the page changes.
  - The audit list's time window and filters (T277):
    - The default window ("last 24 hours") is fixed when the page loads or filters are applied, so paging neither
      repeats nor misses a row.
    - "Back to log" keeps the filters.
  - T286: the audit log names a person one way. Today a command row shows the email and a Login row shows the name, and
    the Actor card's "Display name" is the email.
  - T328: Apply filters comes after the filters and before the table, in reading and Tab order. Today it sits in the
    page header.
  - Long values wrap (T323, at 1280 px): today the audit entry's Action is cut off ("TransitionActivityComman"), and a
    user id is long.
  - Contrast (T322): the Succeeded badge (2.55:1) and the FAILED badge (3.57:1) are below 4.5:1.
  - T114: an audit payload can be large, because an activity-type save carries whole schema documents. The payload
    block must handle it: monospace, wrapped or scrolled in its own region.
  - T274: after T274, a mailing job's log line counts its per-recipient failures. Leave room for a run's outcome (sent,
    skipped, failed) if the design shows one.
QUESTIONS THE DESIGN MUST ANSWER:
  1. System: a page of its own (health, jobs, mail, database, and the maintenance links now on the dashboard), or folded
     into the dashboard with the nav item dropped?
  2. Should /admin/curriculum-progress, which only the dashboard's Maintenance card links today, be reachable from
     System?
  3. What does the operator need on a phone? Today, at 390 px, each wide table scrolls inside its own box, and the
     run-history filters stack.
CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side behaviour beyond a
  ≤10-line script module; no live validation; the phone nav toggle is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence.
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token or a NEW token with its value; spacing on the scale xs 4, sm 8, md 16, lg 24, xl 32,
  2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, primary action),
  Breadcrumbs, DataTable (.clinic-table in .table-container, PagerControls), FormField / FormActions (.form-container,
  .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid), StatePanel (loading / empty / error),
  Skeleton, Alert (success / info / warning / danger), ActionResult, ConfirmDialog (native <dialog>), badges (five tints),
  Icon, TrajectoryChart (hand-drawn SVG, no chart library).
Design these framework states explicitly (no source file shows them): the active nav item; field validation (invalid
  border plus a stripe, not colour alone; message under the field; the summary); the reconnect dialog (rejoining,
  retrying, failed, paused, resume-failed); the in-app error bar; access denied; not found; session ended.
Content: people by name, states and types by label, times in South African time with the zone shown; an out-of-scope
  record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (sidebar included); targets ≥ 24 px; focus
  moves to an action's result; every page works at 390 px with no sideways scroll.
Viewports: 1280×800 and 390×844.
ASK: 2–3 variations. Do the dashboard and System as wireframes first, and institutions, jobs and audit at full
  fidelity. Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a
  variation breaks. The rules most at risk are these:
  - § Status dots: the dot is today the whole signal, and must not be;
  - § Pager: "Showing 1–20 of 137", through PagerControls;
  - § Dashboard layout grid.
  Flag edge cases: every health line failing, no runs, thousands of runs, and a long payload. Then review the result
  for accessibility against WCAG 2.1 AA.
ATTACHED: the screenshots below (paths under design/baseline/), in this order:
  1. act-A/A.6.2-1-system-health.png
  2. states/home--administrator.png
  3. states/scheduled-jobs-list--loaded.png
  4. states/scheduled-jobs-list--dispatched.png
  5. act-A/A.2.10-2-partial-key-nothing.png
  6. states/audit-list--failures.png
  7. states/audit-detail--payload.png
  8. states/placeholder--system.png
  9. act-3/3.32-1-nudge-run.png
  10. states/scheduled-job-runs-list--loaded.png
  11. act-1/1.6-1-institutions-one.png
  More state captures follow when you ask for a state.

RUNBOOK STEPS, verbatim (Role / Route / Do / Expect). Where an Expect describes today's behaviour and a requirement
above says otherwise, the requirement wins:
- A.6.2's Expect describes today's stub health lines; they are replaced;
- A.2.1's "next run in the server's local time" becomes SAST, labelled;
- A.2.10's "'digest' alone matches nothing" and "never by user id" become a select and a name;
- 1.6 and A.6.1's Active box may become Status with Deactivate or Reactivate (T302 kept the box; see § 1);
- A.5.13's "Coming soon" stub becomes a page or goes;
- 1.1's nav list loses System if the page is dropped, and its "Welcome, devadmin@wombat.local" greets him by name.
A.2.8 ages two rows by SQL, as a stand-in for time passing; nothing about that is on screen.

[act-1-setup.md]
### Step 1.1 — The Administrator signs in
Role: Administrator — the platform operator (`devadmin@wombat.local`)
Route: /account/login → /
Do: Sign in with `devadmin@wombat.local` and the dev password that `DevUserSeeder` gives it.
Expect: Home is headed "Home", with "Administrator · Semester N, YYYY" under it, naming the current semester, and its
  header offers no action. No switch is offered, in the sidebar or on Home, since devadmin holds one role. The dashboard
  has two cards:
  - System health, which shows the database connection as healthy;
  - Users across institutions, which counts the seeded accounts (8, or 9 where the bootstrap Administrator exists).
  There is no Maintenance card: Curriculum progress is linked from the Curricula page's header (T335).
  The sidebar reads "Acting as Administrator", with no switch, over his menu, grouped: Home; Platform: Scheduled jobs,
  Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users, Invitations;
  Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision
  panels; then My data rights under a rule (DESIGN.md § The NavMenu). Home is lit. The top bar names him "Demo
  Administrator", with Sign out beside it.

[act-1-setup.md]
### Step 1.6 — Create Kgosi Kgari Teaching Hospital
Role: Administrator — the platform operator
Route: /admin/institutions → /admin/institutions/new → /admin/institutions/{Id:int}
Do: Open Institutions and read the list. Then press Create institution and enter:
  - Name `Kgosi Kgari Teaching Hospital`;
  - Short code `KGK`;
  - Contact email `paeds-admin@kgk.wombat.local`.
  Save. Record KGK's id as `{InstitutionId}`.
Expect: The list holds one row, the `Demo Institution` (`DEMO`), Active, offering Edit only. The create form asks for
  Name, Short code and Contact email, and has no Status field. Save lands on KGK's own record, headed "Edit
  institution", with the browser tab reading "Edit institution · Wombat" (T190). There, Active is ticked and
  Deactivate is offered. Back on the list, KGK is a second row: Active, and created today.

[act-1-setup.md]
### Step 1.11 — Both invitations are spent
Role: Administrator — the platform operator
Route: /admin/invitations → /
Do: Reload the invitations list, then go Home.
Expect: Active invitations reads "No active invitations", because a used invitation leaves the list. The dashboard's
  count of registered users is two higher than in Step 1.1.

[act-3-operations.md]
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

[act-3-operations.md]
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

[act-3-operations.md]
### Step 3.56 — Prof Mbatha opens one entry
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Under Failures only, open Dr Ndlovu's future-date create.
Expect: The Event card shows the time to the millisecond, category Command, action CreateActivityCommand, and result
  Failed with "Date observed: The date cannot be after today (…)." The Actor card shows Dr Ndlovu's user id, display
  name and IP address. There is no Payload card: only an Administrator reads the raw JSON. "Back to log" returns to the
  list.

[act-3-operations.md]
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

[act-4-annual-review.md]
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

[act-6-catalogue.md]
### Step 6.38 — devadmin rebuilds curriculum progress
Role: Administrator — devadmin
Route: /admin/curricula → /admin/curriculum-progress
Do: From Curricula in the menu, open Curriculum progress from the header and read it. Press Rebuild progress, and
  confirm.
Expect:
  - **The way in.** Curricula offers the Administrator an outline "Curriculum progress" in its header, where Home's
    Maintenance card used to link it (T335, flow 01). On the page Curricula stays lit, and the trail reads Home ›
    Curricula › Curriculum progress.
  - **The page.** It says that a rebuild credits every completed activity again against today's curriculum, judges only
    whether an EPA was active as of each completion, and runs as one transaction.
  - **The question.** The button asks first: "Rebuild curriculum progress?".
  - **The result.** The page says progress was rebuilt, with five figures: activities re-read, curriculum items
    credited, semester tallies written, stale tallies removed, and completions re-stamped. Stale tallies removed is at
    least `t` from Step 6.35.

[appendix-cross-cutting.md]
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

[appendix-cross-cutting.md]
### Step A.2.2 — devadmin disables a job and enables it again
Role: Administrator — devadmin
Route: /admin/jobs
Do: Disable msf-campaign-auto-close, try to run it now, then enable it again.
Expect: Disabling says "Job 'msf-campaign-auto-close' disabled." The row then reads "—" for its next run, its toggle
  says Enable, and its Run now is unavailable. Once enabled again, its next run is the next hour. The scheduler does not
  run a disabled job. The audit log records both changes.

[appendix-cross-cutting.md]
### Step A.2.4 — devadmin runs the weekly digest, and Mr Smit is skipped
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the application log.
Expect: "Job 'weekly-coordinator-digest' dispatched."; the row's last run is now, and it reads Succeeded. The log holds
  "WeeklyCoordinatorDigestJob: digests sent n; coordinators skipped: …", with at least one counted as "opted out of
  digest emails". No "Your weekly Wombat digest" mail greets Pieter.

[appendix-cross-cutting.md]
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

[appendix-cross-cutting.md]
### Step A.2.8 — devadmin runs the two reminders against aged work
Role: Administrator — devadmin
Route: /admin/jobs
Do: Age the draft by 15 days and the request by 6 (see the Note). Then run activity-draft-nudge and
  assessor-pending-nudge now, and read the log.
Expect: The log holds a stub mail "You have draft activities waiting" beginning "Hi Nomsa,", listing "Mini-CEX
  (Paediatrics) — draft for 15 days". It also holds "Activities awaiting your assessment", beginning "Hi Fatima,",
  which lists "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 6 days". Each job logs one line counting whom it
  reminded and whom it skipped, and why (T151, T240).

[appendix-cross-cutting.md]
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

[appendix-cross-cutting.md]
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

[appendix-cross-cutting.md]
### Step A.5.2 — Prof Mbatha opens the Administrator's own pages
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled jobs.
Expect: For both, "You cannot open this page": "Your role (Institutional admin) does not open this page." and "If you
  need it for your work, ask the platform administrator.", with Go to Home. They are the Administrator's alone, her menu
  offers neither, and nothing in it is lit on the refusal.

[appendix-cross-cutting.md]
### Step A.5.13 — devadmin's System page
Role: Administrator — devadmin
Route: /
Do: Read the menu.
Expect: The grouped menu of Step 1.1, with no System item: the flow 01 pick dropped it, with its placeholder.

[appendix-cross-cutting.md]
### Step A.6.1 — devadmin maintains the institution record
Role: Administrator — devadmin
Route: / → /admin/institutions → /admin/institutions/{Id:int}
Do: Open Institutions from the nav. Read the list, open KGK, set its contact email to
  `paediatrics@kgk.wombat.local`, and save.
Expect: The list holds the Demo Institution and Kgosi Kgari Teaching Hospital. Each row shows its short code, contact
  email, status (Active) and creation time, with an Edit named for it; Create institution heads the page. Saving says
  "Institution saved." The Administrator is offered the Status box and Deactivate.

[appendix-cross-cutting.md]
### Step A.6.2 — devadmin reads the system health card
Role: Administrator — devadmin
Route: /
Do: Read the dashboard's System health and Users across institutions cards.
Expect: Database connection is green. The card's other two lines, Email queue and Last nightly job, are amber whatever
  their state, and each carries a task id: they are stubs, tied neither to the mail queue nor to Scheduled jobs
  (reported). The users card counts registered accounts.

[appendix-cross-cutting.md]
### Step A.6.9 — devadmin opens his own account
Role: Administrator — devadmin
Route: /admin/users → /admin/users/{UserId}
Do: Find his own account in Users and open it.
Expect: A note says it is his own account, so he cannot change its roles, lockout or password here, and another
  administrator can change his roles or lockout. It links to Change password on his own account. No Remove, Add role,
  Reset password or Lockout is offered (T278).

[appendix-cross-cutting.md]
### Step A.6.10 — The health endpoint
Role: Anonymous — the uptime monitor
Route: /health
Do: Request `/health` without signing in.
Expect: Status 200 with the body "Healthy". The check includes the database (T097), and the body stays terse because
  the endpoint is public.

[appendix-cross-cutting.md]
### Step A.7.11 — devadmin on his phone
Role: Administrator — devadmin
Route: / → /admin/jobs → /admin/jobs/runs → /admin/institutions
Do: At 390 px, open Scheduled jobs, Run history and Institutions.
Expect: The jobs table scrolls inside its container, with each Run now and toggle reachable. The run-history filters
  stack.
````

## 3. The journey

| # | Step | Page template | Who | What they do | What they must be able to see |
|---|---|---|---|---|---|
| 1 | 1.1 | `/account/login` → `/` | devadmin | Signs in | Health, the user count and the maintenance links; his nav |
| 2 | 1.6 | `/admin/institutions` → `/new` → `/{Id:int}` | devadmin | Reads the list and creates KGK | One institution; a create form with no Status field; the saved record, and KGK as a second row |
| 3 | 1.11 | `/admin/invitations` → `/` | devadmin | Goes home after two registrations | The user count, two higher |
| 4 | 3.32 | `/admin/jobs` | devadmin | Runs assessor-pending-nudge now | The last run updating; that the job was dispatched |
| 5 | 3.55 | `/admin/audit` | Prof Mbatha | Sets From, filters Failures only, then Action "Msf" | Only KGK's rows, newest first, 50 to a page; four failures; Mr Smit's 17 MSF commands |
| 6 | 3.56 | `/admin/audit` → `/admin/audit/{Id:guid}` | Prof Mbatha | Opens a failed create | The time to the millisecond, the command, "Failed" with its error, and the actor. No payload. Back to the filtered list. |
| 7 | 3.57 | `/admin/audit` → `/{Id:guid}` | devadmin | Reads every institution's rows and opens two payloads | Rows with no institution ("system"); payloads with `[REDACTED]` and `[PRINCIPAL]` |
| 8 | 4.38 | `/admin/audit` → `/{Id:guid}` | Prof Mbatha | Filters to a committee command and opens a failed and a successful entry | Two failures with readable errors; five OK rows; no payload |
| 9 | 6.38 | `/` → `/admin/curriculum-progress` | devadmin | Rebuilds progress, and confirms | What a rebuild does; "Rebuild curriculum progress?"; five figures |
| 10 | A.2.1 | `/` → `/admin/jobs` | devadmin | Reads every job | Each job's description, schedule, last run, status, next run, and its two row-named actions |
| 11 | A.2.2 | `/admin/jobs` | devadmin | Disables msf-campaign-auto-close, then enables it | "disabled"; next run "—"; Run now unavailable; then "enabled" |
| 12 | A.2.4 | `/admin/jobs` | devadmin | Runs weekly-coordinator-digest | "dispatched"; last run now, Succeeded |
| 13 | A.2.6 | `/admin/jobs` | devadmin | Runs it again | The same |
| 14 | A.2.8 | `/admin/jobs` | devadmin | Runs the two reminders | Both dispatched and Succeeded (the mail itself is in the log) |
| 15 | A.2.9 | `/admin/jobs` | devadmin | Runs the other six | Each Succeeded |
| 16 | A.2.10 | `/admin/jobs` → `/admin/jobs/runs` | devadmin | Filters by key, by "digest", by Failed, and from today | Newest first; who triggered each run, by name; a key filter that finds; "No runs found" |
| 17 | A.5.2 | `/admin/institutions`, `/admin/jobs` → `/access-denied` | Prof Mbatha | Types both addresses | Access denied, twice; neither page in her nav |
| 18 | A.5.13 | `/placeholder/{Feature}` | devadmin | Opens System | Today a "Coming soon" stub; after this flow, a page or no item |
| 19 | A.6.1 | `/` → `/admin/institutions` → `/{Id:int}` | devadmin | Changes KGK's contact email | Each row's code, email, status and creation time; "Institution saved." |
| 20 | A.6.2 | `/` | devadmin | Reads health and the user count | Each line's state in words; a count that says what it counts |
| 21 | A.6.9 | `/admin/users` → `/admin/users/{UserId}` | devadmin | Opens his own account | Why he cannot change his own roles, lockout or password here; the Change password link |
| 22 | A.6.10 | `/health` | uptime monitor | Requests it signed out | 200 "Healthy", plain text (not designed) |
| 23 | A.7.11 | `/`, `/admin/jobs`, `/admin/jobs/runs`, `/admin/institutions` at 390 px | devadmin | Does the same on a phone | Every Run now and toggle reachable; the filters stacked; no sideways scroll |

## 4. States to design

From `states.md` § Home and the role dashboards (rows 114, 148), § System pages (167), § Institution administration
(583–590, 593, 595, 653) and § Platform operations (690–708). The institution states that are Mbatha's own (`--own`,
`--deactivate-refused`, `--not-found`) belong to F12.

| Page | State | Capture (under `design/baseline/`) | Held | How it is reached |
|---|---|---|---|---|
| `/` | Administrator | `states/home--administrator.png` | — | At Step 1.1 |
| | Narrow | `states/home--narrow-administrator.png` | — | At Step A.7.11 |
| `/placeholder/{Feature}` | System | `states/placeholder--system.png` | — | At Step A.5.13 |
| `/admin/institutions` | One | `states/institutions-list--one.png` | — | At Step 1.6, before the create |
| | Two | `states/institutions-list--two.png` | — | At Step A.6.1 |
| | Loading | `states/institutions-list--loading.png` | — | Hold a read (`states.md` § Holding a read) |
| | Narrow | `states/institutions-list--narrow.png` | — | At Step A.7.11 |
| `/admin/institutions/new` | Create | `states/institution-edit--create.png` | — | At Step 1.6, before typing |
| | Required fields empty | `states/institution-edit--invalid.png` | — | At Step 1.6, Save with the form empty |
| `/admin/institutions/{Id:int}` | An administrator's edit | `states/institution-edit--administrator.png` | T302 (re-captured) | At Step 1.6, after Save |
| | Saved | `states/institution-edit--saved.png` | T302 (re-captured) | At Step A.6.1 |
| | Deactivated | `states/institution-edit--deactivated.png` | T302 (re-captured) | On a scratch database: Deactivate on the Demo Institution, confirmed in the dialog |
| | Narrow | `states/institution-edit--narrow.png` | T302 (re-captured) | At Step A.6.1, at 390 px |
| `/admin/jobs` | Nine jobs | `states/scheduled-jobs-list--loaded.png` | — | At Step A.2.1 |
| | A job disabled | `states/scheduled-jobs-list--disabled.png` | — | At Step A.2.2 |
| | Dispatched | `states/scheduled-jobs-list--dispatched.png` | — | At Step A.2.4 |
| | Loading | `states/scheduled-jobs-list--loading.png` | — | Hold a read |
| | Narrow | `states/scheduled-jobs-list--narrow.png` | — | At Step A.7.11 |
| `/admin/jobs/runs` | The history | `states/scheduled-job-runs-list--loaded.png` | — | At Step A.2.10 |
| | One job | `states/scheduled-job-runs-list--filtered.png` | — | At Step A.2.10, the full key |
| | No runs | `states/scheduled-job-runs-list--empty.png` | — | At Step A.2.10, status Failed |
| | Narrow | `states/scheduled-job-runs-list--narrow.png` | — | At Step A.7.11 |
| `/admin/audit` | An institution's log | `states/audit-list--institution.png` | — | At Step 3.55, From set |
| | Failures only | `states/audit-list--failures.png` | — | At Step 3.55 |
| | Every institution | `states/audit-list--administrator.png` | — | At Step 3.57, Next enabled |
| | No entries | `states/audit-list--empty.png` | — | At Step 3.55, Action `NoSuchCommand` |
| | Loading | `states/audit-list--loading.png` | — | Hold a read |
| | Narrow | `states/audit-list--narrow.png` | — | At Step 3.55, at 390 px |
| `/admin/audit/{Id:guid}` | A failure | `states/audit-detail--failure.png` | — | At Step 3.56 |
| | With its payload | `states/audit-detail--payload.png` | — | At Step 3.57 |
| | Not among hers | `states/audit-detail--not-found.png` | — | After Step 3.57, typed by Prof Mbatha: "Audit entry not found" |
| | Narrow | `states/audit-detail--narrow.png` | — | At Step 3.56, at 390 px |
| `/admin/users/{UserId}` | An administrator's own | `states/user-detail--administrator-own.png` | — | At Step A.6.9 |

**States no replay reaches, which the design must still draw:**
- A Failed run badge, and Run now refused while the job runs (`states.md:789`).
- `/health` reporting Unhealthy (`coverage.md:147`). It is plain text and not designed, but the System page's database
  line must show the same failure in words.
- Each health line failing (T327): database down; mail not configured; a mail dropped in 24 hours; a job failed, never
  run, disabled or overdue. This has no capture, because the lines are stubs today.
- A deactivated institution offering a Reactivate action, and no Deactivate (T302, T264). The replay now reaches a
  deactivated institution (`states/institution-edit--deactivated.png`): it offers no Deactivate, and an unticked Active
  box is its only way back; there is no Reactivate button.
- Run history paged past 50 (T327). This has no capture, since the replay had 23 runs.

**Curriculum progress** (Step 6.38) is in this flow's journey. Its five state captures
(`states/curriculum-progress-rebuild--default`, `--dialog`, `--result`, `--failed` and `--narrow`) are briefed in F15
(BRIEF § 8). Add them here if the answer to question 2 puts the page under System.

## 5. Attach

Every path below is relative to `design/baseline/` and was checked with `ls` on 2026-09-26.

### 5.1 Attach these first

1. `act-A/A.6.2-1-system-health.png`
2. `states/home--administrator.png`
3. `states/scheduled-jobs-list--loaded.png`
4. `states/scheduled-jobs-list--dispatched.png`
5. `act-A/A.2.10-2-partial-key-nothing.png`
6. `states/audit-list--failures.png`
7. `states/audit-detail--payload.png`
8. `states/placeholder--system.png`
9. `act-3/3.32-1-nudge-run.png`
10. `states/scheduled-job-runs-list--loaded.png`
11. `act-1/1.6-1-institutions-one.png`

### 5.2 Add when the chat asks for a state (not held)

**States:**
- `states/home--narrow-administrator.png`
- `states/institutions-list--two.png`, `--loading.png` and `--narrow.png`
- `states/institution-edit--create.png` and `--invalid.png`
- `states/scheduled-jobs-list--disabled.png`, `--loading.png` and `--narrow.png`
- `states/scheduled-job-runs-list--filtered.png`, `--empty.png` and `--narrow.png`
- `states/audit-list--institution.png`, `--administrator.png`, `--empty.png`, `--loading.png` and `--narrow.png`
- `states/audit-detail--failure.png`, `--not-found.png` and `--narrow.png`
- `states/user-detail--administrator-own.png`

**Step captures:**
- act-1: `act-1/1.1-1-devadmin-home.png`, `1.6-3-institutions-two.png`, `1.11-2-devadmin-home-11.png`
- act-3: `act-3/3.55-1-audit-kgk-from-sitting.png`, `3.55-2-audit-failures-only.png`, `3.55-3-audit-msf-actions.png`,
  `3.56-1-audit-failure-detail.png`, `3.57-1-audit-all-institutions.png`, `3.57-2-msf-submission-detail.png`,
  `3.57-3-naidoo-transition-payload.png`
- act-4: `act-4/4.38-1-mbatha-quorum-failure-audit-entry.png`, `4.38-2-mbatha-revoke-audit-entry.png`
- act-6: `act-6/6.38-1-curriculum-progress-page.png`, `6.38-2-rebuild-dialog.png`, `6.38-3-rebuilt.png`
- act-A, the jobs and their history: `act-A/A.2.1-1-scheduled-jobs.png`, `A.2.2-1-auto-close-disabled.png`,
  `A.2.2-2-auto-close-enabled.png`, `A.2.4-1-digest-dispatched.png`, `A.2.6-1-digest-dispatched-again.png`,
  `A.2.8-1-nudges-dispatched.png`, `A.2.9-1-six-jobs-run.png`, `A.2.10-1-digest-key.png`, `A.2.10-3-from-today.png`,
  `A.2.10-4-failed-none.png`
- act-A, the rest: `act-A/A.5.2-1-institutions-access-denied.png`, `A.5.2-2-jobs-access-denied.png`,
  `A.5.13-1-system.png`, `A.6.1-1-institutions-list.png`, `A.6.9-1-own-account.png`, `A.7.11-1-devadmin-home.png`,
  `A.7.11-2-devadmin-jobs.png`, `A.7.11-3-devadmin-runs.png`, `A.7.11-4-devadmin-institutions.png`

### 5.3 Re-captured after T302 (formerly held)

Re-captured on 2026-09-26 after T302 landed (41be531); attach them:
- `states/institution-edit--administrator.png` and `act-1/1.6-2-kgk-created.png` (a copy of the end-of-Act-1 snapshot:
  the Active box ticked with its help, Cancel, Deactivate and Save);
- `states/institution-edit--saved.png`, `act-A/A.6.1-2-kgk-saved.png` and `states/institution-edit--narrow.png` (Step
  A.6.1 on a copy of the end-of-Act-6 snapshot);
- `states/institution-edit--deactivated.png` (the Demo Institution on the same copy: "Institution deactivated.", the
  box unticked, no Deactivate).

`act-A/A.6.3-5-devadmin-reactivates-kgk.png` is no longer this flow's: Step A.6.3 no longer has devadmin reactivate KGK,
and the file now holds Prof Mbatha's Home (flow 12; BRIEF § 10).

## 6. Known problems this design must solve

| Task | Lane | What it means for the design | Evidence |
|---|---|---|---|
| T327 | queued | Health is read from real state and named in words: database; mail (configured or logged, queue depth, last sent, last dropped, drops in 24 h); jobs ("All N jobs last succeeded", or the failed, never-run and disabled jobs by name, linking to `/admin/jobs`). The dot is a secondary cue, `aria-hidden`. No task ids. The user count says what it counts. Run history has a key select and a pager with a total. (A2, A13) | `act-A/A.6.2-1-system-health.png`; `AdministratorDashboard.razor:11-26`; `act-A/A.2.10-2-partial-key-nothing.png` |
| T325 | queued | One clock, labelled SAST, across the jobs and run history; schedules in the same zone. The audit may keep labelled UTC. (A9) | `act-3/3.32-1-nudge-run.png`, `states/scheduled-jobs-list--loaded.png` |
| T324 | queued | "Triggered by" is a name (`ScheduledJobRunsList.razor:70`). Required-field messages name the field ("Short code"). Home greets by name (observed; not filed, BRIEF § 6 A8). | `states/scheduled-job-runs-list--loaded.png`, `states/institution-edit--invalid.png` |
| T326 | queued | The Institutions subtitle drops the speciality drill-down that T091 removed. (A10) | `act-1/1.6-1-institutions-one.png`, `states/institutions-list--two.png` |
| T302 | landed (41be531), P2 | Deactivate and Reactivate are separate Administrator commands. The page still sends them from the Active box on Save (and from Deactivate); the design decides whether the box gives way to Status with the two actions. | `states/institution-edit--administrator.png` (re-captured 2026-09-26); `InstitutionEdit.razor` |
| T264 | queued | Deactivate confirms in a ConfirmDialog naming the institution, and is not offered on an inactive record: on this page, landed with T302. The audit pager keeps the focus. (A5) | `states/institution-edit--deactivated.png` (re-captured 2026-09-26) |
| T277 | queued | The audit window is fixed at load or apply, and paging is stable. "Back to log" keeps the filters (F-3.56b). | Steps 3.55, 3.56 |
| T286 | queued | One name per person in the audit log and on its Actor card. | Steps 3.55, 3.56; `states/audit-list--failures.png` |
| T328 | queued | Apply filters after the filters (`AuditList.razor:8-15`). (A4) | `states/audit-list--failures.png` |
| T323 | queued | Long values wrap at 1280: the audit entry's Action is cut off in the Event card (observed, not filed). At 390, wide tables scroll inside their own box (A.7.11). (A3) | `states/audit-detail--payload.png` |
| T322 | queued, P2 | The Succeeded (2.55:1) and FAILED (3.57:1) badges. (A1) | Step A.7.14 Actual |
| T329 | queued, P2 | Each list shows a skeleton under its header while loading, and an alert with no empty state on a load error. (A6) | `states/institutions-list--loading.png`, `states/scheduled-jobs-list--loading.png`, `states/audit-list--loading.png` |
| T114 | queued | The payload card takes a large payload (the ~2 KB bound is a comment, not a mechanism). | `states/audit-detail--payload.png` |
| T274 | queued | A run may one day carry an outcome count (sent, skipped, failed). The inference is to leave room for it; today only the application log has it. | Steps A.2.4–A.2.9 (the log lines) |
| B7 | — | System: design it or drop the nav item (`NavMenu.razor:132`). The stub reads "This page is not built yet." | `states/placeholder--system.png` |

## 7. Questions the design must answer

Write each answer as a sentence in the Claude Design chat (BRIEF § 2.3).

1. **System: a page of its own, or folded into the dashboard with the nav item dropped?**
   - A page would hold health, jobs, mail, the database and the maintenance links now on the dashboard.
   - Dropping it changes `NavMenu.razor:132`, DESIGN.md's nav table and `NavMenuAuthorizationTests`, which parses that
     table (BRIEF § 4).
2. **Should `/admin/curriculum-progress` be reachable from System?**
   - Today only the dashboard's Maintenance card links it (`AdministratorDashboard.razor:47`).
   - It is Administrator-only and safe to run more than once (Step 6.38 Note).
3. **What does the operator need on a phone (A.7.11)?**
   - Today every page works at 390 px by scrolling each wide table inside its own box: jobs 661 px, run history 712 px,
     institutions 699 px.
   - Say whether a phone needs the full tables, or only health and Run now.

## 8. Acceptance

After Claude Code builds it (BRIEF § 9):

1. **Replay the steps** on a fresh database, as BRIEF § 9 says: 1.1, 1.6, 1.11, 3.32, 3.55, 3.56, 3.57, 4.38, 6.38,
   A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.2, A.5.13, A.6.1, A.6.2, A.6.9, A.6.10 and A.7.11.
   - Play the acts in order up to each step. The appendix is played after Act 6.
   - Also check a health line failing: stop the mail sink and send a mail (T327 § Verification).
2. **Update the Expects** whose wording changes, in the same task (BRIEF § 9 item 7):
   - 1.1: the health lines, and the nav if System goes;
   - 1.6 and A.6.1: Status, Deactivate and Reactivate, if the design replaces T302's Active box;
   - 1.11: the user count's label;
   - A.2.1: times in SAST;
   - A.2.10: the key select, and Triggered by as a name;
   - A.5.13: a System page, or no item;
   - A.6.2: health in words.
3. **Run the tests.** `tests/Wombat.Web.Tests` must be green without `--no-build`, including:
   - a bUnit test on `AdministratorDashboard` showing each line's state as text, with no "(T0" (T327);
   - `Navigation/NavMenuAuthorizationTests`, which also pins the placeholder items, if the System item changes;
   - `Scenario/ScenarioRulesTests` (T294's guard), if A.5.13's route changes;
   - `DashboardLinkAuthorizationTests`;
   - `Design/DefinedClassTests`.

   If the status-dot rule or the pager changes, amend DESIGN.md § Status dots and § Pager in the same task.
4. **Re-capture** the 33 states in § 4 and this flow's step captures into `design/baseline/`, and compare them with the
   chosen artboards.
5. **Check it in a browser** at 1280 and 390, as devadmin and, for the audit log, as Prof Mbatha. Request `/health`
   signed out.

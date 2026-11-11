# F18 The operator keeps the platform running

**Who and why:** devadmin ("Demo Administrator"), the platform operator, comes rarely: to see whether the database,
mail and the scheduled jobs work (Home's System health card), to create and keep institutions (1.6, A.6.1), to run,
pause and read the jobs (3.32, A.2.1–A.2.10), to read the audit trail across institutions (3.57), to rebuild progress
(6.38) and to open their own account (A.6.9). Prof Nolwazi Mbatha (InstitutionalAdmin, KGK) reads their own
institution's audit trail (3.55, 3.56, 4.38) and is refused the operator's other pages (A.5.2). An uptime monitor
requests `/health` (A.6.10). No flow has redesigned the operator's pages: flow 01 gave them the shell and Home's frame
and deleted the System placeholder and the Maintenance card (`b347e11c`), flow 12 the institution's record as its own
administrator reads it, the create page's own route and the user page (`e53d4ae7`, T376), flow 15 the curriculum-progress
page (`c931d56f`, T392); the System health card, Institutions, Scheduled jobs, Job run history, the Audit log and an
audit entry are as they were before the shell (their last changes T266, T239, T234). Every step capture is from the
T398 replay (2026-11-10), on the shell as built; the `states/` captures of jobs, runs, institutions and audit are from
2026-09-26, on the old frame.

Restated 2026-11-11 for T400 (the brief of 2026-09-26 predates flows 01–17: it called devadmin "he", and this brief
says "they" of every person; it counted nine jobs, eight since T379 retired `portfolio-export-cleanup`; it listed
`/placeholder/{Feature}`, deleted by T335 with the System nav item (`coverage.md:18`, `:215`); its Home "greets by
email", gone since flow 01 ("Home"); its Maintenance card, gone since T335, Curriculum progress now under Curricula
(T392); its T324 "The ShortCode field is required.", gone since T376 (1.6-4); its T322 badge contrast, done; its 4.38
"two failures with readable errors", gone since T361 (no FAILED row is written); its § 1 note on the sidebar stitched
part-way down a full-page capture, which the new captures do not show: the sidebar now ends at 800 px; its T302 "the
design may show Status with Deactivate or Reactivate", still open (Q5); its CONSTRAINTS digest, pre-flow 02). Paths to
screenshots are under `design/baseline/` (gitignored). Code is cited at `70657d6c`; `W/` is
`src/Wombat.Web/Components/Pages/`, `A/` is `src/Wombat.Application/Features/`, `I/` is `src/Wombat.Infrastructure/`,
`Nav/` is `src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **W** in BRIEF.md § 8, and it stays W for Home and System: whether System is a page with a nav item or Home's card alone (Q1), what health reads and needs (Q2–Q4) are structural. Institutions, jobs and audit are list and record pages whose shapes are expected to hold (BRIEF § 8's F sense), drawn as one proposal each. |
| Frequency and stakes | Rare for the operator. Medium stakes: the health card is the operator's only warning that mail is not configured; with `Email__SmtpHost` unset every mail is logged, not sent, and nothing says so (T157, T327). A deactivated institution can be named on no new invitation. The audit log is the record an auditor reads. |
| Mode | **Wireframe first for the Administrator's Home and the System page** (BRIEF.md § 2.3 step 4; the old row's "§ 2.3 step 3" is that step, renumbered), which must become a real health page or lose its nav item for good (Q1). **Fidelity for institutions, jobs and audit**, one proposal each in round 1 at fidelity. Then the pick; then fidelity with every state; one review round from the boards' text; the operator's decisions; one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768, every new or changed table swept 900–1100 px against its container (§ 11, flows 06 and 15), and every column of every list read at 1280 for each role that sees it, then 390–1440 (§ 11, flow 16) |
| Variations | 2–3 structures for Home and System (wireframes); one fidelity proposal for screens 3–5; the chosen structure at fidelity after the pick |
| People | Administrator `devadmin` ("Demo Administrator", Global, one role). InstitutionalAdmin Prof Nolwazi Mbatha (Kgosi Kgari Teaching Hospital, KGK), the audit log only. An uptime monitor, anonymous. Seen in the rows: Mr Pieter Smit (Coordinator, KGK), Dr Sipho Ndlovu, Dr Anele Dlamini, Dr Fatima Khumalo, Dr Naidoo, Dr Thandi Zulu, Dr Refilwe Mokoena. `D` is the replay day: 2026-11-10 in the T398 replay |
| Runbook steps | 23, pasted verbatim in § 8: 1.1, 1.6, 1.11 (`act-1-setup.md`); 3.32, 3.55, 3.56, 3.57 (`act-3-operations.md`); 4.38 (`act-4-annual-review.md`); 6.38 (`act-6-catalogue.md`); A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.2, A.5.13, A.6.1, A.6.2, A.6.9, A.6.10, A.7.11 (`appendix-cross-cutting.md`). 3.32 and A.2.4–A.2.9 check mail in the application log; only their Run now is this flow's. 6.38 and A.6.9 are flows 15's and 12's pages, reached from here |
| Pages (`coverage.md` templates) | **`/`** (`W/Home.razor`, 104 lines, `@page` at `:1`; the Administrator's case `:27–29` renders `W/Dashboards/AdministratorDashboard.razor`, 42 lines, `[Authorize(Roles = "Administrator")]` `:4`, `DashboardFrame` with `OnRetry` `:10`, System health `:11–26`, Users across institutions `:28–36`). **`/admin/institutions`** (`W/Admin/Institutions/InstitutionsList.razor`, 66, `@page` `:1`, policy `Administrator` `:2`). **`/admin/institutions/new`** (`W/Admin/Institutions/InstitutionCreate.razor`, 106, `@page` `:1`, `Administrator` `:2`; its own page since T376). **`/admin/institutions/{Id:int}`** (`W/Admin/Institutions/InstitutionEdit.razor`, 571, `@page` `:1`, `AdministratorOrInstitutionalAdmin` `:2`; their own administrator's view flow 12's, the Administrator's "kept as built", `:13–19`). **`/admin/jobs`** (`W/Admin/Jobs/ScheduledJobsList.razor`, 188, `@page` `:1`, `Administrator` `:2`). **`/admin/jobs/runs`** (`W/Admin/Jobs/ScheduledJobRunsList.razor`, 122, `@page` `:1`, `Administrator` `:2`). **`/admin/audit`** (`W/Admin/Audit/AuditList.razor`, 211, `@page` `:1`, `AdministratorOrInstitutionalAdmin` `:2`). **`/admin/audit/{Id:guid}`** (`W/Admin/Audit/AuditDetail.razor`, 108, `@page` `:1`, the same policy `:2`). **`/admin/curriculum-progress`** (`W/Admin/CurriculumProgress/CurriculumProgressRebuild.razor`, 134, `@page` `:1`, `Administrator` `:2`; flow 15's). **`/admin/users/{UserId}`** (`W/Admin/Users/UserDetail.razor`, 1,279, `@page` `:1`; the own-account note `:60–63`, flow 12's). **`/placeholder/{Feature}`: no file**; T335 deleted it with the System item, and its address is Page not found (`coverage.md:18`, `:215`); BRIEF § 8's row for flow 18 still names it. **`/health`** (`src/Wombat.Web/Program.cs:105–106`, `AddDbContextCheck<ApplicationDbContext>("database")`; `:192`, `MapHealthChecks("/health").AllowAnonymous()`); the Api host's `/health` answers "ok" and checks no database (`src/Wombat.Api/Program.cs:26`). No page was renamed or moved since 2026-09-26 but InstitutionCreate (split from InstitutionEdit by T376). |
| Menus (`Nav/NavItems.cs`) | Institutions (`/admin/institutions`, icon `building`, `:116`), Scheduled jobs (`/admin/jobs`, `clock`, `:124`), Audit log (`/admin/audit`, `history`, `:125`). The Administrator's groups: Platform (Scheduled jobs, Audit log, SSO mappings, Data rights requests, `:142`), Organisations (Institutions, Colleges, `:143`). The InstitutionalAdmin's "Access and audit" (SSO mappings, Audit log, `:156`). No System item (`:72`). Owners (`Nav/NavOwners.cs`): InstitutionEdit and InstitutionCreate light Institutions for the Administrator (`:147`, `:149`); Curriculum progress lights Curricula (`:161`); the user page Users (`:164`); an audit entry Audit log (`:173`); Job run history Scheduled jobs (`:174`). |
| Components drawn at runtime | **Home:** `PageHeader` "Home" / "Administrator · Semester 2, 2026", no action (flow 01); `DashboardFrame` (loading, load error with Try again); `DashboardCard` "System health" (`settings`, `Span="2"`): three `li.list-row`, each `span.status-dot ok|err` (the database, `:16`) or a literal `status-dot warn` with "Email queue (T012)" / "Last nightly job (T024)" (`:19`, `:22`); `DashboardCard` "Users across institutions" (`users`, `Emphasis`): `.dashboard-metric` "22" / "registered users" (`:31–34`). Data: `GetAdministratorDashboardSummaryQuery` (`A/Dashboards/Administrator/GetAdministratorDashboardSummaryQuery.cs`): a count of institutions as the database probe (`:35–46`), the union of `ListUsersInRoleAsync` over `WombatRoles.All`, ten queries (`:48–57`); the DTO is `DatabaseHealthy`, `TotalUserCount` only. **Institutions:** `PageHeader` "Institutions" / "Maintain institution records and drill into their speciality structure." (`:8`), Create institution (`btn-primary`, `plus`); `StatePanel` with `LoadError` = `exception.Message` (`:17`, `:63`), no `OnRetry`, an empty state "No institutions yet" that cannot occur; `DataTable` "Institutions" with no `Stack` (`:20`): Name, Short code, Contact email, Status (text), Created (`CreatedOn.ToLocalTime()`, no zone, `:37`), Edit named "Edit ‹name›" (`:41`). **Create:** `PageHeader` "Create institution" / "Maintain the institution record used for scoping and onboarding." with "Back to institutions" (`:16–20`); `EditForm` with `DataAnnotationsValidator` and `ValidationMessage`s (`:30–47`), the required mark as `FormField Required` ("Name*"), no "* marks …" line, no `aria-describedby`, no focus on a refusal (T378); Save lands on the record (`forceLoad`, `:79`). **The Administrator's record:** "Edit institution" (`:212`), the same subtitle (`:214–216`), "Back to institutions" (`:187–188`); `.record-page`, `ActionResult` (`:30–43`); `StatePanel` with fixed words and Try again (`LoadFailed`, `:161–163`, `:45`); Name, Short code (the required mark line, refusals under their fields, focused), Contact email; Status, an Active `InputCheckbox` with its help (`:86–97`); Cancel, Deactivate (outline, only while stored active, `:114–117`), Save; `ConfirmDialog` "Deactivate ‹name›?" (`:132–138`), opened by Deactivate and by Save with Active unticked; results "Institution saved." (`:361`), "Institution saved and reactivated." (`:357`), "Institution deactivated." (`:546`); an unknown or out-of-scope id goes to `/not-found` (`:102–105`). **Scheduled jobs:** `PageHeader` "Scheduled jobs" / "View and manage background jobs. Toggle jobs on or off, trigger manual runs, and view run history." with Run history (`btn-outline`, `clock`, `:8–15`); `StatePanel` `ex.Message` (`:17`, `:117`); `DataTable` "Scheduled jobs", no `Stack` (`:20`): Job (key strong, description muted), Schedule (`<code>` cron, `:38`), Last run and Next run (`ToLocalTime()`, `:39`, `:50`), Status (`BadgeFor.JobRun`, `:43`), Enabled (the Disable / Enable button, `:56–60`), Run now (disabled while `!IsEnabled`, `:64`); the `ActionResult` **after** the table (`:79–84`): "Job '‹key›' disabled." / "enabled." / "dispatched." (`:136`, `:141`, `:173`), a refusal its exception's text (`:149`, `:180`). Commands: `RunScheduledJobNowCommand` (`A/Scheduling/Commands/RunScheduledJobNow/`, no `IsEnabled` check), `ScheduledJobDispatcher.DispatchNowAsync` awaits the whole run (`I/Scheduling/ScheduledJobDispatcher.cs:25–63`), `AlreadyRunning` (`:66–68`). Query: `GetScheduledJobStatusQueryHandler` (last run per key, next occurrence in UTC). **Job run history:** `PageHeader` "Job run history" / "View the execution history of scheduled jobs." with "Back to jobs" (`:7–14`); `.search-container`: Job key (free text bound per keystroke, applied on Filter, placeholder "Filter by key...", `:23`), Status (All, Succeeded, Failed, Running, `:27–32`), From, To (`type="date"`, `:36`, `:40`), Filter (`:44`); `StatePanel` `ex.Message`, empty "No runs found" / "No job runs match the selected filters." (`:48`); `DataTable` "Job runs", no `Stack`: Key, Started, Finished (`ToLocalTime()`, `:66–67`), Duration ("7ms", "1.2s"), Status badge, Triggered by (`run.TriggeredBy ?? "scheduler"`, the user id, `:70`), Error (cut at 80 with the rest in `title`, `:74`). Query: `GetScheduledJobRunsQueryHandler` whole-key match (`:44`), From and To as UTC days (`:54`, `:60`), `.Take(200)` with no total (`:66`). **Audit log:** `PageHeader` "Audit log" / "Append-only record of every consequential action. Default view: last 24 hours." with **Apply filters** in its actions (`:8–15`); `.search-container`: Category (the enum's names, `:25–27`), Action ("e.g. CreateInstitutionCommand"), Actor user ID, Subject type ("e.g. CommitteeReview"), From (UTC), To (UTC) (`datetime-local`), Result (All, Success only, Failures only) (`:19–60`); `StatePanel` `ex.Message` (`:62`, `:193`), empty "No entries" / "No audit entries match the current filters."; `DataTable` "Audit entries", no `Stack`: When (UTC) (`<code>`), Category, Action, Actor (the stored display over the address; "system" with none), Subject (type over id), Result (`OK` / `FAILED` badge, `:113`), Detail named by `RowNames.Distinct` (`:65–70`, `:117`); a hand-built pager, "Showing 1–50 of 174", ← Previous / Page N / Next → (`disabled`, not `aria-disabled`), not `PagerControls` (`:125–134`); `LoadAsync` nulls the result, so a page turn drops the focus (`:165`). Query: `ListAuditEntriesQueryHandler` recomputes `UtcNow − 24h` per request (`:20–21`), Action `Contains` (`:51`), the institution scope (`:26–41`). **An entry:** `PageHeader` "Audit entry" / "Full audit record for a single action." with "Back to log" to a bare `/admin/audit` (`:8–15`); `StatePanel` (`RefusalText.Of`, `:101`) whose empty state is "Audit entry not found" / "No audit entry with this id is among those you can view." (`:20–21`), not Unavailable; `.details-grid` of `detail-card`s with **h3** Event (When (UTC) to the ms, Category, Action in `<code>`, Result "Success" / "Failed" over the error), Actor (User ID, Display name, IP address), Subject (Type, ID, Institution ID, Speciality ID; drawn only with a subject type) (`:24–69`); Payload (raw JSON) in `pre.code-block` for `IsInRole(Administrator)` (`:71–77`, `:93`). **Curriculum progress** and **the user page**: as flows 15 and 12 built them. |
| Held | None. Every step capture is from the T398 replay (2026-11-10) on the shell as built, except `act-3/3.32-0-jobs-before.png` (2026-10-13) and `act-4/4.38-2-…`, `4.38-3-…` (2026-10-05), which show failures the step no longer writes: do not attach them. The `states/` captures of Home's narrow frame (2026-09-27), the institutions list, jobs, runs and audit (2026-09-26) are on the old frame (a Title Case sidebar, an email in the top bar): read their bodies only. `states/institution-edit--*` (2026-10-27) and `curriculum-progress-rebuild--*` (2026-11-02) are on the shell |
| Depends on | Flow 01, built (`b347e11c`): the shell, the menus, Home's frame and `DashboardFrame`, Access denied, not found, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again. Flow 06: filters applied with Show. Flow 08: the pager's focus on the list's heading. Flow 11 (`69a17c70`): refusals under their field, focused; the required mark; `StackWide`. Flow 12 (`e53d4ae7`): the institution's record for its own administrator; a destructive action confirmed, its trigger an outline button; a detail page in one column of `<h2>` sections; the user page. Flow 14 (`e8eea1d2`): `ConfirmDialog` in action mode opens on Cancel; a page's own Unavailable. Flow 15 (`c931d56f`): Curriculum progress under Curricula; fixed failure words; the 1024px stack. Flow 16 (`de4cc718`): no Back links beside the trail; one Status group on a record (the College's, a speciality's); the 1150px stack, `.col-fit--wide`. Flow 17 (`84bcffeb`): an action unavailable with `aria-disabled` and its reason as text; JSON laid out one property a line; a search's count line taking the focus. The design system is **version 52** |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 18`. Open each screenshot (BRIEF § 3.3). No
   capture here is an invitation, issued, resent or reset capture; the audit payloads show `[REDACTED]` for the MSF
   token and answers and for a note and data patch, `[PRINCIPAL]` for the principal, and user ids; the run history shows
   devadmin's user id; the lists show scenario addresses (`paeds-admin@kgk.wombat.local`, `hod.paediatrics@…`). None
   shows a password, a token or a one-time link (opened 2026-11-11).
2. **The canvas** is "Wombat 18" (https://claude.ai/artifact/FE7WSRNTmSp88bvS4dZRGV) on the Wombat design system
   **v52**, with `tokens.css` written by hand (§ 11, flow 09) and the mark copied in before round 1 (§ 2.3 step 4):
   `/_blob/b3b9ce265597292edc272896bd70a693`, already in `round-1-ask.txt`.
3. **Captures reach the canvas only as its project files** (§ 11, flows 16 and 17: round 1 of flow 17 saw none of the
   staged files as attachments): copy the 30 staged captures into the canvas's project under `project/ref/baseline/`,
   keeping their paths, before the send. The ask tells the canvas to say first, on its Spec board, which it opened.
   Then **send § 1 and § 8 as ONE message** (`design/flows/18-platform-operations/round-1-ask.txt`). Read
   `project/canvas.json`'s version before and after: a send can draw nothing. The ask tells the canvas to save its
   stylesheet as a project file (§ 11, flow 14).
4. **Answer § 6's operator questions first**; Q1–Q5 are answered before round 1 (they decide what the structures draw
   and what the fidelity proposals may show as built). A question whose answer rests on an open task names it, and the
   task lands before the build when it changes data or rules (§ 11, flow 05). Pick a structure and say why in a
   sentence.
5. **Review the fidelity round from the boards' text** (`Artifact read`), as
   `design/flows/17-activity-type-builder/round-2-review.md` did: one reviewer on the code (and the ask), one on the cast
   and the runbook, one on the design system. Put the decisions to the operator, then ask for one correction round (its
   own canvas if over about 200 boards, § 11, flow 16).
6. **Record every round** in `design/flows/18-platform-operations/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 18 — The operator keeps the platform running: health on Home, institutions, scheduled jobs, the audit trail

This is flow 18 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where they stand),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns), 11 (invitation to admission), 12 (the institution's people), 13 (graduation
  and exit), 14 (data rights), 15 (an institution adopts the catalogue), 16 (the College keeps the national catalogue)
  and 17 (the activity-type builder) are designed and built; every page here sits inside the shell. No flow has
  redesigned the operator's pages: Home's frame is flow 01's, the institution's record is flow 12's for an
  institution's administrator (the Administrator's view of it is kept as built), the curriculum-progress page is flow
  15's, the user page is flow 12's; the System health card, Institutions, Scheduled jobs, Job run history, the Audit log
  and an audit entry are as they were before the shell, inside it.
  This round draws 2–3 STRUCTURES as WIREFRAMES for the Administrator's Home and System (screens 1 and 2), each with its
  reasoning and its step count, and ONE proposal at fidelity for each of screens 3–5 (institutions, scheduled jobs and
  their run history, the audit log and an entry), whose page shapes (a list page and a record page) are expected to
  hold. I pick before fidelity.
  The runbook steps follow below in this same message. The brand mark is at the URL on the next line: use it, never a
  drawn disc.
  /_blob/b3b9ce265597292edc272896bd70a693
  Every person, institution, job, key, count, time and id comes from this message and the steps below; invent none.
  Where a value is not given, write a bracket ‹…›.

THE SCREENSHOTS AND THE STYLESHEET:
  - The screenshots listed under ATTACHED are this canvas's own PROJECT FILES, under project/ref/baseline/ (the same
    names as design/upload/baseline/, copies of design/baseline/); read them from there. SAY ON YOUR SPEC BOARD,
    FIRST, BEFORE DRAWING, which of them you opened (list them) and which you could not read, said plainly. If any did
    not open, draw nothing from memory in its place: write a bracket (flows 06, 12, 13, 16 and 17's lesson). The words
    in them are today's pages, defects included; they are not the target.
  - Write the round's shared stylesheet as a PROJECT FILE, project/f18r1.css (write it as a file of the project, NOT as
    an asset, so the saved round holds it). Every board's head links ds/wombat/tokens.css,
    ds/wombat/components/bundle.css, then f18r1.css, in that order, and no /_blob/ stylesheet; a board's <style> holds
    body{margin:0} at most. Each NEW rule in f18r1.css names its row on the Spec's class table.

GOAL:
  The platform operator (devadmin, "Demo Administrator") comes rarely. They must be able to:
    - learn on arrival, in words, whether the platform is healthy: the database, mail (sent, or only written to the
      log because no mail server is configured), and the scheduled jobs; each line saying what is wrong and linking to
      where to act; and how many accounts there are, with the count saying what it counts;
    - create an institution and keep its record (name, short code, contact email; deactivate it, and reactivate it);
    - read the eight scheduled jobs, run one now, pause and resume one, and read the run history: who ran each run, by
      name, whether it failed and why, filtered by job and by day, paged rather than cut off;
    - read the audit trail across every institution, an entry's payload included, and come back to the rows they left;
    - find Curriculum progress (the rebuild) and their own account.
  An institution's administrator (Prof Nolwazi Mbatha, KGK) reads their own institution's audit trail, without
  payloads, and is refused the operator's other pages.
  An uptime monitor requests /health, which answers "Healthy" or "Unhealthy" in plain text and is not a page to design.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-11-10):
  - System health is three dots and labels. "Database connection" is real. "Email queue (T012)" and "Last nightly job
    (T024)" are hard-coded amber dots that read nothing and print internal task ids; every job had last succeeded
    (A.6.2-1). Each state is the dot's colour alone: an empty span, no text, no name. Nothing on any page says that mail
    is not configured; with no mail server set, every mail is written to the log and none is sent.
  - "22 registered users" counts the accounts holding a role; 25 accounts exist (two graduates hold no role, and one
    account is erased) (A.6.2-1).
  - Scheduled jobs: at 1280 the times wrap ("2026-11-" over "10 17:00") and Run now wraps to two lines; the schedule is a
    raw cron ("0 7 * * *") beside a description saying "daily at 07:00 UTC", while Last run and Next run are the
    server's local time with no zone (A.2.1-1). The result of Disable, Enable or Run now ("Job
    'msf-campaign-auto-close' disabled.") renders below the eight-row table, about 1,100 px down (A.2.2-1). Run now
    says "dispatched." though the run has already finished when it shows. The toggle sits in a column headed Enabled.
  - Job run history: "Triggered by" prints devadmin's user id (deaa0728-…) for every run by hand (A.2.10-4); the Job key
    box says "Filter by key..." but matches only a whole key, so "digest" finds nothing (A.2.10-2); the list stops at
    200 runs with no pager and no total; From is read as a UTC day. A "Back to jobs" link sits beside the trail.
  - The audit log: Apply filters is in the page header, before the filters in reading and Tab order (3.55-2). The
    actor is an email on a command row and a name on a Login row ("smit@kgk.wombat.local", "Pieter Smit") (3.57-1). The
    categories are code names ("DataRights", "ActivityType"); the filters ask for a command name and a user id. The
    default window ("last 24 hours") is recomputed on every page turn; Back to log drops the filters (3.56-2: the
    unfiltered 325 rows). The pager drops the focus.
  - An audit entry: the Action is cut off at 1280 ("TransitionActivityComman", 3.57-3); "Display name" is the email
    (3.56-1); the payload is one unbroken line of JSON (3.57-3); the cards are h3 under the h1; an unknown or
    out-of-scope id is an empty-state card, not flow 14's Unavailable.
  - Institutions: the subtitle promises a speciality drill-down that no row offers (1.6-1). The create page and the
    Administrator's record keep "Back to institutions" beside the trail, are headed "Create institution" / "Edit
    institution" with "Maintain the institution record used for scoping and onboarding.", and the record keeps an
    Active box beside a Deactivate button (A.6.1-2). The create form's refusals are not tied to their fields and take no
    focus (1.6-4).
  - At 390 the jobs, run-history and institutions tables scroll inside their boxes: Status, Next run and the two buttons
    of a job, and an institution's Status, Created and Edit, are out of view (A.7.11-3, -4, -5).
  - A failed read of Institutions, Scheduled jobs, Job run history or the Audit log prints the exception's text, with
    no Try again (code).

AUDIENCE:
  - Administrator: devadmin, "Demo Administrator", the platform operator. Technical, comes rarely, so a page must say
    plainly what is wrong and where to act. Global (no institution). Holds one role.
  - InstitutionalAdmin: Prof Nolwazi Mbatha, Kgosi Kgari Teaching Hospital (KGK), for the audit log only.
  - An uptime monitor, anonymous, for /health.
  - Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Who opens what. Home's System health and Users cards, Institutions and its create page, Scheduled jobs, Job run
    history and Curriculum progress are the Administrator's alone; an InstitutionalAdmin typing /admin/institutions or
    /admin/jobs reads Access denied ("You cannot open this page": "Your role (Institutional admin) does not open this
    page." "If you need it for your work, ask the platform administrator.", Go to Home). An institution's record admits
    their own institution's administrator too (flow 12 built that view). The audit log and an entry admit both: an
    InstitutionalAdmin reads only rows stamped with their institution (rows with no institution, such as an MSF
    respondent's submission or devadmin's own commands, are the Administrator's), and an entry outside their scope reads
    as not found; only the Administrator reads an entry's payload.
  - The menu (flow 01's; do not change it): Administrator: Home; Platform: Scheduled jobs, Audit log, SSO mappings, Data
    rights requests; Organisations: Institutions, Colleges; People: Users, Invitations; Catalogue: EPAs, Curricula,
    Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision panels; then My data rights.
    No System item: flow 01 dropped it with the "Coming soon" placeholder page, whose address is now Page not found.
    InstitutionalAdmin: "Access and audit": SSO mappings, Audit log. Job run history lights Scheduled jobs; an audit
    entry lights Audit log; Curriculum progress lights Curricula (its way in is Curricula's header, flow 15).
  - Health today: the database line is a count query that either answers or not. Mail: with no mail server configured
    (Email__SmtpHost unset) every mail is written to the application log and none is sent, and no mail worker runs;
    with one, mails go through an in-process queue with three tries each; the queue is lost if the app stops. Nothing
    records how many mails were sent or dropped overall (only an MSF link's, an invitation's and a job run's own mails
    are tallied). Jobs: each job's last run (start, status) and next run are read for the Scheduled jobs page.
  - The eight scheduled jobs (key, cron in UTC, description as stored):
      activity-draft-nudge, 0 7 * * *, "Reminds trainees about draft activities older than 14 days (daily at 07:00 UTC)."
      assessor-pending-nudge, 0 9 * * *, "Nudges assessors about activities waiting for their assessment for more than 5
        days (daily at 09:00 UTC)."
      audit-log-retention, 0 3 * * *, "Archives audit log entries older than 2 years into the archive table (daily at
        03:00 UTC)."
      entrustment-decision-expiry, 30 3 * * *, "Expires entrustment decisions past their ExpiresOn date and sends 30-day
        expiry reminders (daily at 03:30 UTC)."
      msf-campaign-auto-close, 0 * * * *, "Closes open MSF campaigns whose window has ended and anonymises their
        respondents (hourly)."
      msf-invitation-expiry-reminder, 0 8 * * *, "Reminds MSF respondents who have not responded, once, two days before
        their last day to respond (daily at 08:00 UTC)."
      scheduled-job-run-retention, 0 4 * * *, "Deletes ScheduledJobRun records older than 90 days (daily at 04:00 UTC)."
      weekly-coordinator-digest, 0 8 * * 1, "Sends weekly digest emails to Coordinators on Mondays at 08:00 UTC."
  - A job: Disable stops the scheduler running it and leaves Next run "—"; Enable resumes it. Run now runs it at once
    and the page waits for the run to end; a second Run now while it runs is refused: "Job '‹key›' is already running,
    so it was not started again. Wait for that run to finish, then run it again if it is still needed." Run now is
    offered only on an enabled job (the page's rule; the command itself does not check). Both changes and every run by
    hand are audited.
  - A run: key, started, finished, duration, status (Running, Succeeded, Failed), who ran it (an Administrator's user
    id today, or nobody for the scheduler), and a failed run's error (up to 2,000 characters). What a run did ("assessors
    nudged 2 (activities 2)") is written to the application log only, never stored with the run. Runs are kept 90 days.
  - An institution: name (required, up to 200), short code (required, up to 32), contact email (optional), active or
    not, created on. Refusals: "Type the institution's name. The record was not saved.", "Type a short code. The record
    was not saved.". Only an Administrator creates one, deactivates or reactivates one; deactivating asks first:
    "Deactivate ‹name›?" / "No new invitation can be issued for it until it is reactivated. Its people, their records
    and the invitations already sent are kept. You can reactivate it on this page." Results: "Institution saved.",
    "Institution deactivated.", "Institution saved and reactivated.". An inactive institution cannot be named on a new
    invitation.
  - An audit entry: when (stored in UTC, to the millisecond), category (Command, Authentication, Permission,
    Committee, Msf, Export, DataRights, Job, ActivityType), action (a command or event name: CreateActivityCommand,
    Login), actor (user id, a display name, the address truncated to ::/48), subject (type and id), institution, result
    (Success or Failed, with the refusal's words), and a payload (JSON) in which secrets read "[REDACTED]" and the
    signed-in person "[PRINCIPAL]". Filters today: Category, Action (matches part of a name: "Msf" finds the MSF
    commands), Actor user ID, Subject type, From (UTC), To (UTC), Result (All, Success only, Failures only); 50 rows a
    page, newest first; with no From or To, the last 24 hours.
  - Curriculum progress (flow 15's page; keep it): what a rebuild does, Rebuild progress asking "Rebuild curriculum
    progress?", the result "Curriculum progress was rebuilt." over five figures, a failure in fixed words.
  - The operator's own account (flow 12's page; keep it): "Your own account: your roles and how you sign in.", the note
    that another administrator changes their roles or lockout, and Change your password.

CONTENT (real scenario values from the T398 replay, D = 2026-11-10):
  - Home at A.6.2: "Administrator · Semester 2, 2026"; 25 accounts, 22 holding a role (Dr Lerato Molefe and Dr Pieter
    du Plessis have graduated and hold none; Dr Sipho Ndlovu's account is erased); no account locked at that moment.
  - Jobs at A.2.1 (times as the page prints them, the server's local time, which was SAST): seven last ran 2026-11-10
    17:00 (the scheduler's catch-up), assessor-pending-nudge 19:15, msf-campaign-auto-close 21:00, each Succeeded;
    weekly-coordinator-digest never ran ("—"), next 2026-11-16 10:00. activity-draft-nudge next 2026-11-11 09:00.
  - Run history at A.2.10: 21 runs, all Succeeded; 11 by hand (devadmin), 10 by the scheduler; durations 4 ms to 102 ms.
    The digest's two runs by hand: 21:39:40 (68 ms) and 21:39:58 (25 ms).
  - Institutions: Demo Institution (DEMO, admin@demo.local, Active, created 2026-11-10 17:00); Kgosi Kgari Teaching
    Hospital (KGK, paeds-admin@kgk.wombat.local, then paediatrics@kgk.wombat.local, Active, created 2026-11-10 17:17),
    id 2.
  - Audit at 3.55 (Prof Mbatha, From 2026-11-10 16:58 UTC): 166 KGK rows (58 Login, 25 Logout, 23
    CreateActivityCommand, 40 TransitionActivityCommand, 19 MSF, 1 SendActivityReminderCommand); Failures only: four
    (Dr Fatima Khumalo's decline without a note 17:02:02; Dr Sipho Ndlovu's two refused creates 17:01:45 and 17:01:41;
    Dr Anele Dlamini's refused submit 17:00:06). At 3.57 (devadmin, From 17:00 UTC): 174 rows, eight
    SubmitMsfResponseCommand by "system". Ndlovu's entry: "Date observed: The date cannot be after today (2026-11-10)."
  - Payloads (devadmin): {"token": "[REDACTED]", "answers": "[REDACTED]"}; Dr Naidoo's complete of activity 13:
    {"note": "[REDACTED]", "principal": "[PRINCIPAL]", "activityId": 13, "actorUserId": "535c41ed-…",
    "dataPatchJson": "[REDACTED]", "transitionKey": "complete"}. An activity type's save carries whole schema documents
    (several KB).
  - Volumes: jobs always 8; runs 0 to thousands (90 days, one job hourly: about 2,800 runs held at once); the audit log 0 to
    hundreds a day (174 rows in one act's sitting).

WHAT FLOW 18 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell, the menus and Access denied (flow 01); Home's frame ("Home", the role and semester, no header
    action for the Administrator; flow 01); the institution's record as their own administrator reads it (flow 12);
    Curriculum progress (flow 15) and the user page (flow 12) beyond a link to them; the rules above (who opens what,
    the audit scope, the payload the Administrator's alone, Deactivate asking first).
  May, for these pages: Home's Administrator cards (what health says and how), System (a page, or none), the
    institutions list's columns and subtitle, the create page and the Administrator's view of the record (Status as a
    box or as a read-out with its own actions), the jobs list's columns, times and where its results show, run history's
    filters and pager, the audit log's filters, their order, the window and the way back, an entry's layout and payload,
    the states (loading, load error, empty, Unavailable) and every page at 390. Mark PROPOSED any rule change (a new
    route, a new nav item, a new action, a new refusal, a new confirmation, data a page does not read today): those are
    mine. The runbook's Expects change with the words.
  Say which of these each structure changes, and why.

SCREENS, in order:
  1. / — the Administrator's Home (WIREFRAMES, 2–3 structures): health in words (the database, mail, the jobs), each
     line naming what is wrong and linking where to act; the account count saying what it counts; every health line
     healthy (A.6.2), and each failing (below); 390.
  2. System — either a page of its own (health in detail: database, mail, each job's line; Curriculum progress and the
     other maintenance links) with its nav item under Platform (PROPOSED), or no page and no item, Home's card holding
     it all. Draw both as structures (Q1).
  3. /admin/institutions, /admin/institutions/new, /admin/institutions/{Id:int} (FIDELITY): the list with one row and
     with two; create, and create saved empty (refusals at their fields); the Administrator's record: saved; Deactivate…
     asked and done; an inactive institution and its way back (Q5); loading, load error, 390.
  4. /admin/jobs and /admin/jobs/runs (FIDELITY): the eight jobs; one disabled (Run now unavailable, with its reason in
     text); one just run (its result where it can be read); Run now refused while running; a failed last run with its
     error; the run history loaded, filtered by job, filtered to Failed (none), from a day, paged past 50, empty;
     loading, load error, 390.
  5. /admin/audit and /admin/audit/{Id:guid} (FIDELITY): Prof Mbatha's KGK rows from a time; Failures only; the
     Administrator's rows across institutions (rows with no institution among them); empty; paged; an entry: a failure
     (Prof Mbatha, no payload), a submission with its payload, a large payload (an activity type's save), not found or
     not theirs; back to the rows they left; loading, load error, 390.

STEPS: 1.1, 1.6, 1.11, 3.32, 3.55, 3.56, 3.57, 4.38, 6.38, A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.2,
  A.5.13, A.6.1, A.6.2, A.6.9, A.6.10, A.7.11 (23). Pasted verbatim below (Role / Route / Do / Expect). In them, D is the
  day the story is replayed. Where an Expect quotes today's wording or layout (A.6.2's stub lines, A.2.1's "server's
  local time", A.2.10's whole-key filter, A.7.11's tables scrolling in their boxes), that is today's page, not a
  requirement on the new one. 3.32, A.2.4, A.2.6, A.2.8 and A.2.9 check mail in the application log: only their Run now
  and its result are this flow's. A.6.9 and 6.38 are flows 12's and 15's pages: draw them only as reached from here. The
  steps say "he" and "his" of devadmin; this flow says "they".

STATES TO SHOW:
  Loading for every page (the header at once and a skeleton; no action that needs the data until it has loaded); a load
  error (a sentence and Try again, never an exception's text, and no empty state under it); Unavailable for an unknown
  institution, an unknown audit entry and one outside Prof Mbatha's institution (flow 14's, never "forbidden"); Access
  denied for Prof Mbatha on /admin/institutions and /admin/jobs (flow 01's: reuse it); empty: the run history and the
  audit log (institutions and jobs always hold rows); refused: an institution saved empty, Run now while that job runs;
  the results ("Institution saved.", "Institution deactivated.", a job disabled, enabled, run), each taking the focus;
  Deactivate…'s question opening on Cancel; 390 for every page, nothing sideways.
  Health, each line as it would read: all healthy; the database unreachable; mail not configured (written to the log,
  not sent); a mail dropped in the last 24 hours; a job failed (its error), never run, disabled, overdue (its last run
  older than its last scheduled time: the app was down).
  None of these is reached by a replay unless marked: a failed run, Run now refused, every failing health line, run
  history past 50, a large payload. Label each "Illustration".
  Data volumes: none (no runs; no audit rows in the window), typical (A.2.10's 21 runs; 3.55's 166 rows), heavy
  (thousands of runs; 174 rows in one sitting; a payload of several KB; an error of 2,000 characters).

REQUIREMENTS FROM KNOWN DEFECTS:
  - T327 (queued): each health line has a label and its state in words ("OK", "Not configured", "Failing", …); a dot, if
    kept, is a secondary cue, hidden from screen readers (WCAG 1.4.1); no task ids. Mail says whether mail is sent or
    only logged and, once recorded (DATA), the queue, the last sent, the last dropped and the drops in 24 hours: "Not
    configured" is a warning, a drop in 24 hours a failure. Jobs reads "All 8 jobs last succeeded" or names the failed,
    never-run, disabled (and overdue) jobs, linking to Scheduled jobs. The account count says what it counts (Q4). Run
    history's job filter is a select of the eight keys (or matches part of a key); the list pages ("Showing 1–50 of N",
    PagerControls) and never stops silently.
  - T157 (queued, the deploy): with Email__SmtpHost unset, mail is logged, not sent, and nothing says so. The mail line
    is the operator's warning.
  - T324 (queued): "Triggered by" names the person ("Demo Administrator"), or "Scheduler". Audit categories by label
    ("Data rights", "Activity type").
  - T325 (queued) and T384 (proposed): every time a person reads is South African time with the zone shown
    ("2026-11-10 21:39 SAST"); a schedule in the same zone and in words ("Daily at 09:00 SAST"); From and To are South
    African days. The audit record may keep its stored UTC, labelled (Q7).
  - T326 (queued): the Institutions subtitle says what the page does.
  - T286 (queued): the audit log names a person one way, by name, on every row and on the entry (DATA: the stored display
    is the email on command rows).
  - T277 (queued): the audit window is fixed when the page loads or the filters are applied, so paging neither repeats
    nor misses a row; the way back from an entry returns to the same rows and filters.
  - T328 (queued): Apply filters (or Show) after the filters, before the table, in reading and Tab order.
  - T264 (queued): the audit pager keeps the focus (DESIGN.md § Pager: the list's heading). Deactivate… asks first, names
    the institution, is an outline button and is not offered on an inactive one (built, T302; keep).
  - T323 (queued): long values wrap at 1280 (an entry's Action; a user id; a 2,000-character error); at 390 nothing
    scrolls sideways and every action is reachable (Q11).
  - T329's kind: a failed read says so in fixed words with Try again, never the exception's text (the four lists today).
  - T378 (proposed): the create form's refusals under their fields, described by them and focused, as the record's are
    (flow 11/12's form system), with "* marks a field you must fill in.".
  - T114 (queued): a payload can be several KB; the payload block handles it (monospace, laid out one property a line,
    wrapped or scrolled in its own region).
  - T274 (queued): a run may one day carry what it did (sent, skipped, failed); leave room for it if the design shows a
    run's outcome. Today it is in the log only (DATA).
  - The header: no Back link beside the trail (flows 12–17): Back to jobs, Back to log, Back to institutions go.
  - Headings: one h1, then h2 (an entry's Event, Actor, Subject and Payload are h3 under the h1 today).
  - Run now's result: above the list, focused, and in true words (the run has finished when it shows).

QUESTIONS THE DESIGN MUST ANSWER:
  1. (Mine) System: a page of its own under Platform, or Home's card holding it all and no nav item? Draw both.
  2. (Mine) Mail health: what the mail line reads, and what new data it needs.
  3. (Mine) Job health: which jobs the line names (failed, never run, disabled, overdue) and in what words.
  4. (Mine) The account count: what it counts and how it says so.
  5. (Mine) The Administrator's institution record: the Active box kept, or Status read out with Deactivate… and
     Reactivate… as actions; its heading and subtitle.
  6. Run history: the job filter, the pager, Triggered by, the days.
  7. (Mine) Times: SAST everywhere, the audit record too?
  8. The audit log: the filters (by label, by name rather than by id), their order, the window, the way back.
  9. An entry: its layout, the payload's block, Unavailable.
  10. Run now and Disable: the words of the result, where it shows, and a disabled job's Run now.
  11. On a phone: which lists stack, and what the operator needs there.
  12. (Mine) What the build folds in.

ASK:
  - Round 1: 2–3 STRUCTURES as wireframes for screens 1 and 2 (Home and System), each with its reasoning and its step
    count (1.1, A.6.2 and a failing mail line found and acted on, at 1280 and at 390), at 1280 and 390 px; and ONE
    proposal at fidelity for each of screens 3, 4 and 5 in every state above, at 1280 and 390 px. With them, two boards
    of their own: a Spec board (FIRST, which screenshots you opened and which you could not; then every component used,
    NEW marked; the DESIGN.md rules each changes; everything PROPOSED; the data a page would need that it does not read
    today, marked DATA) and a Steps board (each structure's journeys counted, and which of the 23 Expects it would
    change). Stop and wait for my pick.
  - After the pick: the chosen structure at full fidelity with every state above, at 1280 and 390 px, every
    confirmation, and where the focus lands after each action, with a Spec board (every NEW and changed component, its
    classes, the DESIGN.md text each changes, the tokens, the DATA, and every message by kind with its words) and a
    Steps board.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a structure
    changes (§ Status dots: the dot is the whole signal today and must not be; § Pager; § Dashboard layout grid and
    § Dashboard page; § Table system's stacking; § Page-level patterns' List page, Detail page, Unavailable and
    Breadcrumbs; § Button system's destructive action; § Form system's refusals).
  - Flag edge cases: every health line failing at once; the database down (Home cannot read anything else); no runs;
    thousands of runs; a run still Running; a 2,000-character error; a payload of several KB; an audit row with no
    actor ("system"); an entry with no subject; an institution with no contact email; a 200-character institution name;
    an InstitutionalAdmin whose institution is inactive reading the log.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, a state never
    colour alone, every row action named for its row ("Run now: assessor-pending-nudge", "Edit Kgosi Kgari Teaching
    Hospital", "Detail: CreateActivityCommand at …"), focus after every action and confirmation and page turn, refusals
    tied to their fields (aria-describedby), targets ≥ 24 px (44 at 390).

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5; restated after the flow 01 pilot, 2026-09-27)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  the error page, /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side
  behaviour beyond a small script module; no live validation; the phone menu is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence (Source Sans 3 and Fraunces, OFL, ship).
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token (the design system's tokens.json) or a NEW token with its value; prefer the spacing scale
  xs 4, sm 8, md 16, lg 24, xl 32, 2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
The shell is designed and built (flow 01): design the page body inside it, not a new frame. The sidebar shows the
  acting role's navigation (one role at a time; access is the union of the roles held), grouped above eight links, with
  My progress and My data rights under a rule; the top bar holds the person's name and Sign out; below 641 px a phone
  bar with a CSS-only menu. A new page names its owning list (its nav item lights, its breadcrumb follows it) and its
  title "<Page> · Wombat", the same words as its h1 and its nav label, in sentence case.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, the trail from the
  owning list, an optional icon, header actions), DataTable (.clinic-table in .table-container, PagerControls),
  FormField / FormActions (.form-container, .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid,
  loading and load error), StatePanel (loading / empty / error), Skeleton, Alert (success / info / warning / danger,
  text on the tint), ActionResult, ConfirmDialog (native <dialog>), badges (five tints), Icon, TrajectoryChart
  (hand-drawn SVG, no chart library), ReferenceBlock.
  Since flow 02 also: the auth card, PasswordField with its Show toggle and PasswordRules. Since flow 03:
  RefusalSummary, DataTable's stacked rows (Stack). Since flow 05: ISO dates, the training year's wording, the ended
  record (R5). Since flow 06: the role Homes; filters applied with Show. Since flow 07: the container-query stack.
  Since flow 08: one action a row, a choice of a few kinds as radios (choice-group), a form page of its own, the
  pager's focus on the list's heading. Since flow 09: a page of its own per list row; a dialog with a field. Since flow
  10: a create page of its own reached from Home's header action. Since flow 11: the list is the page and its create
  form a page of its own; DataTable's StackWide (stacked from 900 px down); a role chosen from described radios; the
  required mark; only the chosen role's fields drawn; refusals under their field, focused. Since flow 12: a detail
  page in one column of <h2> sections with no details-grid (the user page); a form in a note panel opened in place
  (Add a role…); a destructive action confirmed, its trigger an outline button. Since flow 13: a record card naming
  an end and linking the record (FormerTraineeRecordCard); a page's header action named for the person; an Alert with
  its first sentence bold over a details list (Verify's answer); an end section of its own apart from Save. Since
  flow 14: a ConfirmDialog in action mode opens with the focus on Cancel; a page's own Unavailable state for a record
  that does not exist or is not the caller's; a textarea's character counter; the refusal first in aria-describedby.
  Since flow 15: the owner mark in words under a title (.muted .text-sm); "Set by the College." where a row offers no
  change; a one-shot result token that survives a page change and leaves the address; a list stacked to 1024 px when
  its columns need it; an item's page of its own with its record view; "Open" / "Held back".
  Since flow 16: a College's tree on one page, an <h2> per speciality over its table; no Back link beside the trail;
  Deactivate… asking first, naming its target and saying only what it does; Remove on an unsaved rung an outline
  button that asks nothing; Unavailable for a list or record typed with another's id; a list stacked to 1150 px with
  one-word columns kept whole (.col-fit--wide).
  Since flow 17: a real tabs pattern (role "tab", aria-selected, arrow keys); a search applied with Show and Clear, its
  count line ("12 of 22 … match …") taking the focus; the caller's own rows first; a version strip; an editor opened in
  place under its row; rules read in words above the JSON that stays their editor, the JSON laid out by Wombat one
  property a line; an action unavailable with aria-disabled and its reason as text beside it.
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Designed by flow 02: session ended.
  Still to design where a flow meets them: field validation (invalid border plus a stripe, not colour alone; message
  under the field; the summary).
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. A date is ISO (YYYY-MM-DD) on every screen.

ATTACHED (as project files under project/ref/baseline/; all on the shell as built, 2026-11-10, unless marked):
  act-A/A.6.2-1-system-health.png, act-A/A.7.11-1-home-390.png, act-A/A.2.1-1-scheduled-jobs.png,
  act-A/A.2.2-1-auto-close-disabled.png, act-A/A.2.10-1-digest-key.png, act-A/A.2.10-2-partial-key-nothing.png,
  act-A/A.2.10-4-from-today.png, act-A/A.7.11-3-jobs-390.png, act-A/A.7.11-4-runs-390.png,
  act-1/1.6-1-institutions-one.png, act-1/1.6-4-create-empty-save.png, act-A/A.6.1-2-kgk-saved.png,
  act-A/A.7.11-5-institutions-390.png, act-3/3.55-2-audit-failures.png, act-3/3.56-1-audit-entry.png,
  act-3/3.57-3-naidoo-transition-detail.png, act-3/3.57-1-devadmin-audit.png (17 key); and act-A/A.2.4-1-digest-dispatched.png,
  act-A/A.2.10-3-failed-none.png, act-1/1.6-2-kgk-created.png, act-3/3.56-2-back-to-log.png,
  act-3/3.57-2-msf-submission-detail.png, act-6/6.38-2-curriculum-progress-page.png, act-6/6.38-3-rebuild-dialog.png,
  act-6/6.38-4-rebuilt.png, act-A/A.5.2-1-institutions-access-denied.png, act-A/A.6.9-1-own-account.png,
  states/institution-edit--deactivated.png (2026-10-27, on the shell), states/scheduled-jobs-list--loading.png and
  states/audit-detail--not-found.png (2026-09-26, the old frame: a Title Case sidebar, an email in the top bar; read
  their bodies only). 30 in all. They show scenario names, addresses, ids and redacted payloads, which are the pages'
  own record; none shows a password, a token or a one-time link.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 1.1 | `/account/login` → `/` | devadmin | Home, "Administrator · Semester 2, 2026"; System health and the account count. **The two stub lines with task ids; each state a colour alone (T327)** |
| 1.6 | `/admin/institutions` → `/admin/institutions/new` → `/admin/institutions/{Id:int}` | devadmin | One row; the create form with no Status; the empty save refused under Name and Short code; KGK's record, then a second row. **The subtitle's drill-down (T326); the create refusals not tied to their fields (T378); "Edit institution" and Back to institutions** |
| 1.11 | `/admin/invitations` → `/` | devadmin | The account count two higher (9 → 11). **"registered users" counts role holders (T327)** |
| 3.32 | `/admin/jobs` | devadmin | Run now on assessor-pending-nudge; its last run 19:15, Succeeded. **"dispatched." below the table; unzoned times (T325)** |
| 3.55 | `/admin/audit` | Prof Mbatha | 166 KGK rows from the sitting; Failures only, four; "Msf", 19. **Apply filters before the filters (T328); email or name by row (T286); UTC typed by hand** |
| 3.56 | `/admin/audit` → `/admin/audit/{Id:guid}` | Prof Mbatha | Ndlovu's refused create: the ms time, Failed and its words; the actor; no payload. **"Display name" is the email (T286); Back to log drops the filters (T277)** |
| 3.57 | `/admin/audit` → `/admin/audit/{Id:guid}` | devadmin | 174 rows across institutions; "system" rows; two payloads, redacted. **The Action cut off at 1280 (T323); the payload one line (T114)** |
| 4.38 | `/admin/audit` → `/admin/audit/{Id:guid}` | Prof Mbatha | Five OK RecordCommitteeDecisionCommand rows; Event and Actor only; one revocation |
| 6.38 | `/admin/curricula` → `/admin/curriculum-progress` | devadmin | Flow 15's page: the question, the result and five figures. No fault left (Gap none) |
| A.2.1 | `/` → `/admin/jobs` | devadmin | Eight jobs, each with Disable and Run now named for it. **Raw cron beside "UTC" descriptions and unzoned times; dates and Run now wrap at 1280** |
| A.2.2 | `/admin/jobs` | devadmin | Disabled: Next run "—", Enable, Run now unavailable; enabled again. **The result 1,100 px down; Run now's reason not said** |
| A.2.4, A.2.6 | `/admin/jobs` | devadmin | The digest run, Succeeded (the mail is in the log) |
| A.2.8, A.2.9 | `/admin/jobs` | devadmin | Two, then five, jobs run, each Succeeded (the lines are in the log) |
| A.2.10 | `/admin/jobs` → `/admin/jobs/runs` | devadmin | 21 runs newest first; the digest's two by hand; Failed, none; from today. **Triggered by a user id (T324); "digest" finds nothing (T327); no pager; From a UTC day (T384); Back to jobs** |
| A.5.2 | `/admin/institutions`, `/admin/jobs` → `/access-denied` | Prof Mbatha | Access denied twice, nothing lit (flow 01's; Gap none) |
| A.5.13 | `/` | devadmin | The menu, no System item (unless Q1 builds one) |
| A.6.1 | `/` → `/admin/institutions` → `/admin/institutions/{Id:int}` | devadmin | Two rows; KGK's email changed; "Institution saved."; the Active box and Deactivate. **The subtitle (T326); Back to institutions** |
| A.6.2 | `/` | devadmin | Health in words; a count that says what it counts. **The stubs; 22 of 25 accounts; colour alone (T327)** |
| A.6.9 | `/admin/users` → `/admin/users/{UserId}` | devadmin | Flow 12's own-account page (Gap none) |
| A.6.10 | `/health` | uptime monitor | 200, "Healthy", text/plain, no-store (not designed) |
| A.7.11 | `/`, `/admin/jobs`, `/admin/jobs/runs`, `/admin/institutions` at 390 | devadmin | Every Run now and toggle reachable; the filters stacked; nothing sideways. **Each table scrolls in its box, its actions and states out of view (T323)** |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in
`execution/knowledge/scenario-paediatrics/states.md` (the line numbers below are its rows on 2026-11-11). The step
captures are from the T398 replay (2026-11-10), on the shell as built; `states/` captures dated 2026-09-26 or -27 are on
the old frame (read their bodies only).

| Page | State | Screenshot | Who | Reached at (`states.md` line) |
|---|---|---|---|---|
| `/` | The Administrator's Home; at 390 | `act-A/A.6.2-1-system-health.png` (`act-1/1.1-1-devadmin-home.png`, `states/home--administrator.png`), `act-A/A.7.11-1-home-390.png` (`states/home--narrow-administrator.png`) | devadmin | Steps 1.1, A.6.2, A.7.11 (:141, :196) |
| `/` | The count two higher | `act-1/1.11-2-devadmin-home-11.png` | devadmin | Step 1.11 |
| System | Not built: Page not found since T335 | none | devadmin | Step A.5.13 (`act-A/A.5.13-1-devadmin-menu.png`) |
| `/admin/institutions` | One; two; loading; at 390 (**Status, Created and Edit scrolled off**) | `act-1/1.6-1-institutions-one.png` (`states/institutions-list--one.png`), `act-1/1.6-3-institutions-two.png`, `act-A/A.6.1-1-institutions-list.png` (`states/institutions-list--two.png`), `states/institutions-list--loading.png`, `act-A/A.7.11-5-institutions-390.png` (`states/institutions-list--narrow.png`) | devadmin | Steps 1.6, A.6.1, A.7.11; held read (:882–:885) |
| `/admin/institutions/new` | Create; saved empty (**refusals not described, no focus: T378**) | `states/institution-edit--create.png`, `act-1/1.6-4-create-empty-save.png` (`states/institution-edit--invalid.png`) | devadmin | Step 1.6 (:886, :887) |
| `/admin/institutions/{Id:int}` | The Administrator's record; saved; deactivated (**no Reactivate; the box the way back**); at 390 | `act-1/1.6-2-kgk-created.png` (`states/institution-edit--administrator.png`), `act-A/A.6.1-2-kgk-saved.png` (`states/institution-edit--saved.png`), `states/institution-edit--deactivated.png`, `states/institution-edit--narrow.png` | devadmin | Steps 1.6, A.6.1; scratch (:888, :889, :898, :900) |
| `/admin/jobs` | Eight jobs; one disabled (**the result below the table**); run now; loading; at 390 (**Status, Next run and both buttons scrolled off**) | `act-A/A.2.1-1-scheduled-jobs.png` (`states/scheduled-jobs-list--loaded.png`), `act-A/A.2.2-1-auto-close-disabled.png` (`states/scheduled-jobs-list--disabled.png`), `act-A/A.2.2-2-auto-close-enabled.png`, `act-3/3.32-1-nudge-run.png`, `act-A/A.2.4-1-digest-dispatched.png` (`states/scheduled-jobs-list--dispatched.png`), `act-A/A.2.6-1-…`, `act-A/A.2.8-1-…`, `act-A/A.2.9-1-…`, `states/scheduled-jobs-list--loading.png`, `act-A/A.7.11-3-jobs-390.png` (`states/scheduled-jobs-list--narrow.png`) | devadmin | Steps 3.32, A.2.1–A.2.9, A.7.11; held read (:1093–:1097) |
| `/admin/jobs/runs` | All, from today (**Triggered by a user id**); the full key; "digest" (**nothing**); Failed (none); at 390 | `act-A/A.2.10-4-from-today.png` (`states/scheduled-job-runs-list--loaded.png`), `act-A/A.2.10-1-digest-key.png` (`states/scheduled-job-runs-list--filtered.png`), `act-A/A.2.10-2-partial-key-nothing.png`, `act-A/A.2.10-3-failed-none.png` (`states/scheduled-job-runs-list--empty.png`), `act-A/A.7.11-4-runs-390.png` (`states/scheduled-job-runs-list--narrow.png`) | devadmin | Steps A.2.10, A.7.11 (:1098–:1101) |
| `/admin/audit` | Hers from the sitting; Failures only (**Apply filters first**); "Msf"; back from an entry (**filters dropped**); the Administrator's (**email or name by row**); empty; loading; at 390 | `act-3/3.55-1-audit-from-sitting.png` (`states/audit-list--institution.png`), `act-3/3.55-2-audit-failures.png` (`states/audit-list--failures.png`), `act-3/3.55-3-audit-msf.png`, `act-3/3.56-2-back-to-log.png`, `act-3/3.57-1-devadmin-audit.png` (`states/audit-list--administrator.png`), `act-3/3.57-0-devadmin-failures.png`, `act-3/3.57-0-devadmin-msf.png`, `act-4/4.38-1-mbatha-record-decision-entries.png`, `states/audit-list--empty.png`, `states/audit-list--loading.png`, `states/audit-list--narrow.png` | Prof Mbatha, devadmin | Steps 3.55–3.57, 4.38; held read (:1083–:1088) |
| `/admin/audit/{Id:guid}` | A failure (**"Display name" the email**); a submission's payload; Dr Naidoo's payload (**the Action cut off; one line**); a decision's and a revocation's; not hers (**an empty state, old frame**); at 390 | `act-3/3.56-1-audit-entry.png` (`states/audit-detail--failure.png`), `act-3/3.57-2-msf-submission-detail.png`, `act-3/3.57-3-naidoo-transition-detail.png` (`states/audit-detail--payload.png`), `act-4/4.38-4-mbatha-record-decision-ok-entry.png`, `act-4/4.38-5-mbatha-revoke-audit-entry.png`, `states/audit-detail--not-found.png`, `states/audit-detail--narrow.png` | Prof Mbatha, devadmin | Steps 3.56, 3.57, 4.38; typed (:1089–:1092) |
| `/admin/curriculum-progress` | Flow 15's: the page, the question, rebuilt, failed, at 390 | `act-6/6.38-2-curriculum-progress-page.png`, `act-6/6.38-3-rebuild-dialog.png`, `act-6/6.38-4-rebuilt.png`, `states/curriculum-progress-rebuild--failed.png`, `--narrow.png` | devadmin | Step 6.38 (:1113–:1117) |
| `/admin/users/{UserId}` | Flow 12's own account | `act-A/A.6.9-1-own-account.png` (`states/user-detail--administrator-own.png`) | devadmin | Step A.6.9 (:1011) |
| `/access-denied` | Prof Mbatha on the operator's pages | `act-A/A.5.2-1-institutions-access-denied.png`, `act-A/A.5.2-2-jobs-access-denied.png` | Prof Mbatha | Step A.5.2 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Never built: every health line in words, and each failing (the database down, mail not configured, a mail dropped, a
  job failed, never run, disabled, overdue); the account count saying what it counts; a System page (Q1); run history
  paged past 50 and its job select; Triggered by a name; SAST on the jobs and runs; the audit filters after their
  fields and in the address; a payload laid out; Unavailable for an institution, an audit entry; Try again on the four
  lists; Reactivate… (Q5).
- Unplayed by the story: a failed run (every run Succeeded); Run now refused while running (the dispatcher's lock; no
  step races it); a run still Running; run history over 200 (the replay held 21); a large payload (an activity type's
  save is in the log at 1.26–1.30 but no step opens it); the database unreachable (`/health` Unhealthy needs PostgreSQL
  stopped, `coverage.md:248`); mail not configured is how dev runs, so every replay is in that state and nothing shows
  it; a load error on any of these lists.

**Belongs to other flows:** Prof Mbatha's own institution record (`states/institution-edit--own`, `--deactivate-refused`,
`--not-found`, `--load-error`, `--duplicate`, `--email-*`, `--short-code-empty`; flow 12); Curriculum progress's design
(flow 15, kept); the user page (flow 12, kept); the mails 3.32 and A.2.4–A.2.9 write (flow 19); Access denied (flow 01).

## 4. Attach

Paths are relative to `design/baseline/`. Every file below was opened on 2026-11-11 for this brief. None shows a
password, a token or a one-time link: the payloads read `[REDACTED]` and `[PRINCIPAL]`; the ids, addresses and names
are the scenario's own record.

**Key screenshots (attach these first, with § 1):**
1. `act-A/A.6.2-1-system-health.png`: Home, the two stub lines with task ids, "22 registered users"
2. `act-A/A.7.11-1-home-390.png`: Home at 390
3. `act-A/A.2.1-1-scheduled-jobs.png`: eight jobs, raw cron, wrapped times and Run now
4. `act-A/A.2.2-1-auto-close-disabled.png`: a job disabled; the result below the table
5. `act-A/A.2.10-1-digest-key.png`: the digest's two runs, Triggered by a user id
6. `act-A/A.2.10-2-partial-key-nothing.png`: "digest" finds nothing
7. `act-A/A.2.10-4-from-today.png`: 21 runs, eleven by a user id
8. `act-A/A.7.11-3-jobs-390.png`: jobs at 390, Status onwards scrolled off
9. `act-A/A.7.11-4-runs-390.png`: run history at 390
10. `act-1/1.6-1-institutions-one.png`: the list, its subtitle
11. `act-1/1.6-4-create-empty-save.png`: the create page refused empty
12. `act-A/A.6.1-2-kgk-saved.png`: the Administrator's record, the Active box, "Institution saved."
13. `act-A/A.7.11-5-institutions-390.png`: institutions at 390, Status onwards scrolled off
14. `act-3/3.55-2-audit-failures.png`: Prof Mbatha's Failures only; Apply filters in the header
15. `act-3/3.56-1-audit-entry.png`: an entry, "Display name" the email
16. `act-3/3.57-3-naidoo-transition-detail.png`: a payload on one line; the Action cut off
17. `act-3/3.57-1-devadmin-audit.png`: the Administrator's 174 rows, email and name by row

**As the chat asks:** `act-A/A.2.4-1-digest-dispatched.png`, `act-A/A.2.10-3-failed-none.png`,
`act-1/1.6-2-kgk-created.png`, `act-3/3.56-2-back-to-log.png`, `act-3/3.57-2-msf-submission-detail.png`,
`act-6/6.38-2-curriculum-progress-page.png`, `act-6/6.38-3-rebuild-dialog.png`, `act-6/6.38-4-rebuilt.png`,
`act-A/A.5.2-1-institutions-access-denied.png`, `act-A/A.6.9-1-own-account.png`,
`states/institution-edit--deactivated.png` (2026-10-27, on the shell), and (old frame, read their bodies only)
`states/scheduled-jobs-list--loading.png`, `states/audit-detail--not-found.png`.

That is 17 key files and 13 more: 30.

**Do not attach:** the duplicates of a step capture (`act-1/1.1-1-devadmin-home.png`, `act-A/A.5.13-1-devadmin-menu.png`, `states/home--administrator.png`, `states/home--narrow-administrator.png`, `states/institutions-list--one.png`, `states/institutions-list--two.png`, `states/institutions-list--narrow.png`, `states/institution-edit--create.png`, `states/institution-edit--invalid.png`, `states/institution-edit--administrator.png`, `states/institution-edit--saved.png`, `states/scheduled-jobs-list--loaded.png`, `states/scheduled-jobs-list--disabled.png`, `states/scheduled-jobs-list--dispatched.png`, `states/scheduled-jobs-list--narrow.png`, `states/scheduled-job-runs-list--loaded.png`, `states/scheduled-job-runs-list--filtered.png`, `states/scheduled-job-runs-list--empty.png`, `states/scheduled-job-runs-list--narrow.png`, `states/audit-list--institution.png`, `states/audit-list--failures.png`, `states/audit-list--administrator.png`, `states/audit-detail--failure.png`, `states/audit-detail--payload.png`, `states/user-detail--administrator-own.png`); the step captures the key ones cover, not opened for this brief (`act-1/1.6-3-institutions-two.png`, `act-1/1.11-1-invitations-spent.png`, `act-1/1.11-2-devadmin-home-11.png`, `act-3/3.32-1-nudge-run.png`, `act-3/3.55-1-audit-from-sitting.png`, `act-3/3.55-3-audit-msf.png`, `act-3/3.57-0-devadmin-failures.png`, `act-3/3.57-0-devadmin-msf.png`, `act-4/4.38-1-mbatha-record-decision-entries.png`, `act-4/4.38-4-mbatha-record-decision-ok-entry.png`, `act-4/4.38-5-mbatha-revoke-audit-entry.png`, `act-6/6.38-1-curricula-header.png`, `act-A/A.2.2-2-auto-close-enabled.png`, `act-A/A.2.6-1-digest-dispatched-again.png`, `act-A/A.2.8-1-nudges-dispatched.png`, `act-A/A.2.9-1-six-jobs-run.png`, `act-A/A.5.2-2-jobs-access-denied.png`, `act-A/A.5.2-3-adoptions-typed-curricula.png`, `act-A/A.6.1-1-institutions-list.png`, `act-A/A.6.9-0-users-search.png`, `act-A/A.7.11-2-menu-open-390.png`); stale captures of failures no longer written (`act-3/3.32-0-jobs-before.png`, `act-4/4.38-2-mbatha-quorum-failure-audit-entry.png`, `act-4/4.38-3-mbatha-category-failure-audit-entry.png`); other old-frame states not opened (`states/institutions-list--loading.png`, `states/audit-list--empty.png`, `states/audit-list--loading.png`, `states/audit-list--narrow.png`, `states/audit-detail--narrow.png`, `states/institution-edit--narrow.png`, `states/curriculum-progress-rebuild--default.png`, `states/curriculum-progress-rebuild--dialog.png`, `states/curriculum-progress-rebuild--result.png`, `states/curriculum-progress-rebuild--failed.png`, `states/curriculum-progress-rebuild--narrow.png`); other flows' states (`states/institution-edit--own.png`, `states/institution-edit--deactivate-refused.png`, `states/institution-edit--not-found.png`, `states/institution-edit--load-error.png`, `states/institution-edit--loading.png`, `states/institution-edit--duplicate.png`, `states/institution-edit--email-cleared.png`, `states/institution-edit--email-malformed.png`, `states/institution-edit--short-code-empty.png`).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `70657d6c`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T327** (P3, queued) | Q1–Q4, Q6. **Still true, all four parts.** The Email and job lines are literal markup with a `warn` dot and "(T012)" / "(T024)" (`AdministratorDashboard.razor:19`, `:22`; observed A.6.2-1, 1.1's and A.6.2's Gaps F-1.1a, F-A.6.2a, 2026-11-10, every job last Succeeded). Each state is an empty span's colour (`:16–22`; observed F-A.6.2c); DESIGN.md § Status dots (`:3322–3333`) still calls the dot the signal. The count is role holders, 22 of 25 (`GetAdministratorDashboardSummaryQuery.cs:48–57`; observed F-A.6.2b). Run history matches a whole key (`GetScheduledJobRunsQueryHandler.cs:44`; observed F-A.2.10b) and stops at 200 with no total (`:66`, code). Nothing records mail outcomes overall: the queue is an in-process `Channel` (`I/Email/EmailQueue.cs:7`); with no SMTP host no worker runs (`I/DependencyInjection.cs:100–109`); the worker reports only to a mail's own observers (DATA, T327's `IEmailHealth`). | `act-A/A.6.2-1…`, `act-A/A.2.10-2…` |
| **T157** (P1, queued; its pre-deploy note) | Q2. **Still true (code):** with `Email__SmtpHost` unset, `LoggingEmailSender` writes each mail, body and links included, to the log, silently (`DependencyInjection.cs:100–104`); `Program.cs` does not refuse to start in Production; no page says so. Every replay runs this way, so the state is today's dev. | — (code) |
| **T324** (P3, queued; its job-runs and audit halves) | Q6, Q8. **Still true:** Triggered by prints the user id (`ScheduledJobRunsList.razor:70`; observed A.2.10-1, -4: `deaa0728-…`, F-A.2.10a); the audit categories are enum names (`AuditList.razor:25–27`, code). **Gone:** "The ShortCode field is required." (T376: "Type a short code. The record was not saved.", observed 1.6-4). | `act-A/A.2.10-4…` |
| **T325** (P3, queued) and **T384** (proposed) | Q7. **Still true:** jobs and runs print `ToLocalTime()` with no zone (`ScheduledJobsList.razor:39`, `:50`; `ScheduledJobRunsList.razor:66–67`; `InstitutionsList.razor:37`) beside descriptions in UTC (observed A.2.1-1: "daily at 07:00 UTC", next "2026-11-11 09:00"); From and To are UTC days (`GetScheduledJobRunsQueryHandler.cs:54`, `:60`; F-A.2.10c, not played at 21:42). The audit log is stored UTC, labelled, typed in UTC (`AuditList.razor:44–49`, `:74`). | `act-A/A.2.1-1…` |
| **T326** (P3, queued; the institutions half) | **Still true:** "Maintain institution records and drill into their speciality structure." (`InstitutionsList.razor:8`; observed 1.6-1, F-1.6a, F-A.6.1a). The create page and the record share "Maintain the institution record used for scoping and onboarding." (`InstitutionCreate.razor:16`, `InstitutionEdit.razor:214–216`). | `act-1/1.6-1…` |
| **T302** (done, `41be531`) and **T264** (P2, queued; the institution part done) | Q5. **Built; keep:** Deactivate separate, an outline button, only on an institution stored active, behind a `ConfirmDialog` naming it, gone once done, its result focused (observed `states/institution-edit--deactivated.png`, 2026-10-27). **Still a choice:** the Active box stays the only way back (no Reactivate button; Save with the box ticked: "Institution saved and reactivated.", `:357`), and Save with the box unticked opens the same dialog (`:128–129`). **Still true (T264):** the audit pager drops the focus (`AuditList.razor:165`, code). | `states/institution-edit--deactivated.png`, `act-A/A.6.1-2…` |
| **T277** (P3, queued; the audit half) | Q8. **Still true:** the window is `UtcNow − 24h` per request (`ListAuditEntriesQueryHandler.cs:20–21`, code); Back to log goes to a bare `/admin/audit` (`AuditDetail.razor:10`; observed 3.56-2, F-3.56b: 325 rows, the filters empty). | `act-3/3.56-2…` |
| **T286** (P3, queued; the audit display) | Q8. **Still true:** command and Logout rows show the email, Login rows the name (observed 3.55's Actual, F-3.55a; 3.57-1); the entry's "Display name" is the email (observed 3.56-1, F-3.56a). The stored display comes from `ClaimTypes.Name` (DATA: one name source, T286). | `act-3/3.57-1…`, `act-3/3.56-1…` |
| **T328** (P3, queued; the audit half) | Q8. **Still true:** Apply filters is in `PageHeader`'s actions, before the filters (`AuditList.razor:8–15`; observed 3.55-2). | `act-3/3.55-2…` |
| **T323** (P3, queued) | Q11. **Still true:** an entry's Action cut at 1280 ("TransitionActivityComman", observed 3.57-3; "SubmitMsfResponseCommand" just fits, 3.57-2); at 390 the jobs (655 px), run-history (696 px) and institutions (681 px) tables scroll in their boxes, Status, Next run, the toggle and Run now, and an institution's Status, Created and Edit out of view (observed A.7.11-3, -4, -5; A.7.11's Expect accepts it today). None of the four lists has `Stack` or `StackWide`. | `act-A/A.7.11-3…`, `-4…`, `-5…` |
| **T329's kind** (P2, queued) | **Still true (code):** Institutions, Scheduled jobs, Job run history and the Audit log set `LoadError` to `exception.Message`, with no `OnRetry` (`InstitutionsList.razor:63`, `ScheduledJobsList.razor:117`, `ScheduledJobRunsList.razor:109`, `AuditList.razor:193`); a job action's refusal is its exception's text (`ScheduledJobsList.razor:149`, `:180`). **Gone:** the institution record (fixed words and Try again, T376), Curriculum progress (T392), Home (`DashboardFrame`, flow 01). | — (code) |
| **T378** (proposed) | **Still true:** the create page refuses with plain `ValidationMessage`s, no `aria-describedby`, the focus left on Save, no "* marks …" line, "Name*" (`InstitutionCreate.razor:30–47`; observed 1.6-4), unlike the record (observed 1.6-2: the mark line, refusals described). | `act-1/1.6-4…` |
| **T114** (P3, queued; item 1) | Q9. **Still true:** the payload is a `pre.code-block` of the stored text, one unbroken line in jsonb's key order (observed 3.57-3); nothing bounds it (an activity type's save carries whole schema documents). | `act-3/3.57-3…` |
| **T274** (P3, queued) | A run's outcome (sent, skipped, failed) is in the application log only (A.2.4–A.2.9's Actuals); `ScheduledJobRun` holds key, start, finish, status, error and who (`src/Wombat.Domain/Scheduling/ScheduledJobRun.cs`). Leave room; storing it is DATA. | Steps A.2.4–A.2.9 |
| **T322** (done) | **Gone:** the Succeeded and FAILED badges' contrast (the state tints, `BadgeFor.JobRun`, `BadgeFor.AuditResult`; T322, T335). Keep. | — |
| **New: the jobs' result below the table** (observed; unfiled) | `ActionResult` is after the `StatePanel` (`ScheduledJobsList.razor:79–84`): "Job 'msf-campaign-auto-close' disabled." at about 1,100 px on an 800 px screen (observed A.2.2-1), focused but out of the first view. | `act-A/A.2.2-1…` |
| **New: "dispatched." for a run already done** (code) | The page awaits the whole run (`ScheduledJobDispatcher.cs:25–63`), then says "dispatched." (`ScheduledJobsList.razor:173`); a failed run still reads "dispatched." in green, its failure only in the Status badge (inferred). | — (code) |
| **New: Run now on a disabled job** (code) | The page disables it (`:64`, a `disabled` button with no reason given); the command does not refuse a disabled job (`RunScheduledJobNowCommandHandler`, no `IsEnabled` check). A rule (PROPOSED: refuse it). | `act-A/A.2.2-1…` |
| **New: the jobs table at 1280** (observed; unfiled) | Last run and Next run wrap as "2026-11-" / "10 17:00", the cron "30 3 * *" / "*", Run now to two lines (observed A.2.1-1); the toggle sits under a column headed "Enabled". | `act-A/A.2.1-1…` |
| **New: Back links beside the trail** (observed) | "Back to jobs" (`ScheduledJobRunsList.razor:9–12`), "Back to log" (`AuditDetail.razor:10`), "Back to institutions" (`InstitutionCreate.razor:18`, `InstitutionEdit.razor:187–188`), which flows 12–17 dropped for the trail. | `act-A/A.2.10-1…`, `act-3/3.56-1…`, `act-A/A.6.1-2…` |
| **New: an entry's headings and not found** (code; old-frame capture) | h1 then h3 Event, Actor, Subject, Payload (`AuditDetail.razor:26`, `:43`, `:54`, `:74`), no h2; an unknown or out-of-scope id is `StatePanel`'s empty card "Audit entry not found" (`:20–21`), not flow 14's Unavailable; the subject's institution and speciality are raw ids (`:60`, `:64`). | `states/audit-detail--not-found.png` |
| **New: the audit filters ask for code** (code) | Action wants a command name ("e.g. CreateInstitutionCommand"), Actor a user id, Subject type a type name, From and To a UTC `datetime-local` (`AuditList.razor:33–49`); the pager is hand-built, `disabled` at its ends, not `PagerControls` (`:125–134`). | `act-3/3.55-2…` |
| **New: the Api host's `/health` checks nothing** (code) | `src/Wombat.Api/Program.cs:26` answers "ok" whatever the database; the Web host's checks it (T097). Not a page; noted for Q12. | — (code) |
| **Gone since the old brief** | System's "Coming soon" stub and its nav item (T335; `coverage.md:215`); Home's "Welcome, ‹email›" (flow 01); the Maintenance card (T335; Curriculum progress under Curricula, T392, 6.38 Gap none); nine jobs (eight, T379); 4.38's two failures (T361); the own account's controls (flow 12, T376; A.6.9 Gap none); the stitched sidebar in full-page captures (the new captures end the sidebar at 800 px). | 6.38, A.6.9 |

**Known Gaps the T398 replay left for this flow** (2026-11-10): 1.1 F-1.1a (T327); 1.6 F-1.6a (T326); 3.55 F-3.55a
(T286); 3.56 F-3.56a (T286), F-3.56b (T277); A.2.10 F-A.2.10a (T324), F-A.2.10b (T327), F-A.2.10c (T325, T384); A.6.1
F-A.6.1a (T326); A.6.2 F-A.6.2a, F-A.6.2b, F-A.6.2c (T327). All other steps: Gap none.

## 6. Questions for the operator

Each with a recommendation. The canvas answers Q6, Q8–Q11 in its proposals; Q1–Q5, Q7 and Q12 are the operator's, and
**Q1–Q5 are answered before round 1** (they decide what the structures draw and what the fidelity proposals may show
as built).

1. **Q1. (The operator's; the old brief's question 1; must be answered before round 1) System: a page, or Home's card
   alone?** Today: no page, no nav item (T335), Home's card three lines. *Recommendation:* round 1 draws both: (a)
   Home's System health card holds everything, a line per part in words, each linking where to act (Scheduled jobs; for
   mail, nothing to link: the words say what to set), and no System item (A.5.13 holds); (b) Home keeps a one-line
   summary per part and a System page under Platform (PROPOSED route and item) holds the detail and the maintenance
   links (Curriculum progress, Scheduled jobs, Audit log). Lean (a): the operator lands on Home, three parts fit one
   card, and (b) adds a route, a nav item, a DESIGN.md nav-table row and the tests that parse it for no new data.
2. **Q2. (The operator's; must be answered before round 1) Mail health and its data.** *Recommendation:* fold T327's
   mail half in: an `IEmailHealth` the mail path writes to (configured or logged; the queue's depth; the last sent and
   the last dropped; drops in 24 hours), with the line reading "Not configured: mail is written to the log, not sent."
   as a warning and a drop in 24 hours as a failure (PROPOSED words). Leave T157's startup refusal in Production to
   T157 (the deploy's). The state is the Web host's only; say so if the Api host ever sends mail.
3. **Q3. (The operator's; must be answered before round 1) Job health.** *Recommendation:* "All 8 jobs last
   succeeded." or the jobs named by state: failed (last run Failed), never run, disabled, and overdue (enabled, its last
   run older than its last scheduled time, plus a grace of an hour; PROPOSED), each by key, linking to Scheduled jobs.
   A Running last run is not a failure.
4. **Q4. (The operator's; T327's open choice; must be answered before round 1) The account count.** *Recommendation:*
   every account but an erased one, with locked accounts said apart: "24 accounts" / "0 locked" on the T398 replay at
   A.6.2 (inferred: 25 rows less Dr Ndlovu's erased one), one count query through `IUserAdministrationService`, not ten
   role lists. 1.11's Expect ("two higher") holds either way.
5. **Q5. (The operator's; T302's open half; must be answered before round 1) The Administrator's institution record.**
   *Recommendation:* as flow 16's College, speciality and sub-speciality records: headed by the institution's name (as
   Prof Mbatha's view already is; "Edit institution" goes), Status read out with Deactivate… while active and
   Reactivate… while inactive, each an outline button behind a `ConfirmDialog` opening on Cancel (Reactivate… PROPOSED
   words), no Active box, no Back to institutions; the create page's refusals as the record's (T378). The list's
   subtitle "The institutions that hold Wombat's people and records." (PROPOSED). 1.6 and A.6.1's Expects change.
6. **Q6. (Canvas; T327, T324, T384) Run history.** *Recommendation:* a Job select of the eight keys ("All jobs" first),
   Status, From and To as South African days, applied with Show (flow 06); Triggered by "Demo Administrator" or
   "Scheduler"; `PagerControls`, "Showing 1–50 of N", the focus on the list's heading after a turn; the error whole,
   wrapped, not cut at 80 with a tooltip. No Back to jobs.
7. **Q7. (The operator's; T325) Times.** *Recommendation:* every time a person reads in SAST with the zone
   ("2026-11-10 21:39 SAST"), the jobs' and runs' included; a schedule in words in SAST ("Daily at 09:00 SAST",
   "Hourly", "Mondays at 10:00 SAST"), the cron as a monospace detail; the audit list and its filters in SAST too, the
   entry keeping the stored UTC to the millisecond as its record, labelled ("2026-11-10 17:01:41.861 UTC").
8. **Q8. (Canvas; T277, T286, T328, T324) The audit log.** *Recommendation:* the filters, then Show and Clear after
   them (flow 06/17), carried in the address with the resolved window, so an entry's way back (the trail's Audit log)
   returns to the same rows; Category by label; the actor by name on every row (DATA: T286's one name source); the
   window fixed at load or Show; `PagerControls`-like words with the focus on the list's heading. Actor and Subject
   filters by name rather than id are DATA (PROPOSED), left out unless you want them.
9. **Q9. (Canvas; T114, T323) An entry.** *Recommendation:* one column of `<h2>` sections (What happened, Who, What it
   was done to, Payload) as flow 12's user page; the Action and long ids wrapping; the payload laid out one property a
   line (as flow 17's JSON), in its own scrolling region with a max height; Unavailable ("Audit entry unavailable" /
   "This entry does not exist, or you cannot open it.", flow 14's words; PROPOSED) for an unknown or out-of-scope id.
10. **Q10. (Canvas) Run now and Disable.** *Recommendation:* the result above the list, focused, in true words: "Ran
    assessor-pending-nudge: Succeeded at 2026-11-10 21:15 SAST." / "… Failed: ‹error›." (PROPOSED); "Disabled
    msf-campaign-auto-close. The scheduler will not run it until you enable it."; Run now unavailable on a disabled job
    (`aria-disabled`, its reason as text: "Enable it to run it."), and the command refusing it too (PROPOSED rule).
11. **Q11. (Canvas; T323) The phone.** *Recommendation:* the health card first; every list stacked (`StackWide`, or the
    1150px stack where its columns need it): a block per job with its state, next run and both buttons at 44 px; a
    block per run, per institution and per audit entry; nothing in a scrolling box. A.7.11's Expect changes.
12. **Q12. (The operator's) What the build folds in.** *Recommendation:* T327 whole (health in words, mail and jobs
    state, the count, run history's select and pager); T324's job-runs and audit-category halves; T325's page half for
    these pages and T384; T326's institutions half; T277's audit half; T286's audit display (if the name source is
    small: else its own task before the build); T328's audit half; T264's audit pager; T323 for these pages; T329's kind
    on the four lists; T378; T114 item 1 (the payload's display, not its bound); the new faults above (the jobs' result,
    "dispatched.", Run now on a disabled job, the 1280 wraps, the Back links, the entry's headings and Unavailable).
    Leave T157 (the deploy), T274 (a run's outcome stored), T114 items 2–3, and the Api's `/health` (file it) as their
    own.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are the 23 in § 8; § 9): every Expect must hold,
  the refusals' exact words included. **Rewrite the steps that change:** 1.1 and A.6.2 (health in words, the count's
  label; the nav if Q1 builds System), 1.6 and A.6.1 (Q5's record, the subtitle, no Back), 1.11 (the count's label),
  3.32 and A.2.2–A.2.9 (Q10's result words and place), 3.55–3.57 and 4.38 (Q8's filters, Show, the names, the way
  back; Q7's zone), A.2.1 (Q7's times and schedules), A.2.10 (Q6's select, names, pager, days), A.5.13 (Q1), A.7.11
  (Q11's stacks). Search the whole runbook (BRIEF.md § 9 item 7) for the words that change ("Apply filters", "Back to
  log", "Back to jobs", "Edit institution", "registered users", "dispatched", "When (UTC)", "Display name").
  Record passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** no "(T0" and no colour-only state on Home; the mail line reading "Not configured" on dev
  (which has no SMTP host), and failing with the mail sink stopped and a mail sent on a configured scratch host (T327's
  Verification); a job made to fail on a scratch database reads Failed on Home and in run history with its error; run
  history past 50 pages; Triggered by a name; no user id on a page that is not the audit's; every time zoned; the audit
  way back keeps the rows; nothing scrolls sideways at 390 or 768, every list stacked, every control 44 px at 390; each
  new table swept 900–1100px against its container and every column read at 1280 as devadmin and Prof Mbatha; no
  exception's text on any failed read.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include a bUnit test on `AdministratorDashboard` showing each line's state as text, no
  "(T0", the dot `aria-hidden` (T327); Application tests for the mail state (with a fake clock), the jobs state and the
  count; `GetScheduledJobRuns` paging with a total; `Navigation/NavMenuAuthorizationTests` if Q1 adds System;
  `DashboardLinkAuthorizationTests`; `Hosting/PageTitleTests`; `Accessibility/RowNamesTests`, `ActionFocusTests`;
  `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`, `TableColumnClassTests`, `ContrastTests`; the
  scenario guard in `Scenario/`. Amend DESIGN.md § Status dots, § Pager, § Dashboard page and the pages' § Page-level
  patterns with the banner.
- **Re-capture** the states in § 3 (every old-frame `states/` capture of jobs, runs, institutions and audit, and the new
  ones on scratch copies, as `states.md` says) and the steps' own captures, keeping their names;
  `check_baseline_paths.py` at missing 0. Compare them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as devadmin and, for the audit log, as Prof Mbatha. Request `/health` signed
  out.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (`act-1-setup.md`, `act-3-operations.md`,
`act-4-annual-review.md`, `act-6-catalogue.md`, `appendix-cross-cutting.md`; Role, Route, Do and Expect only), as the
T398 replay left them (2026-11-10; the Gaps it recorded on these steps are in § 5). `D` is the replay day (README § How
to play). The steps say "he" and "his" of devadmin; this brief says "they".

```text
Step 1.1 — The Administrator signs in (act-1-setup.md)
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

Step 1.6 — Create Kgosi Kgari Teaching Hospital (act-1-setup.md)
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
  Deactivate is offered. Back on the list, KGK is a second row: Active, and created today. An empty Save reads "Type the
  institution's name. The record was not saved." under Name and "Type a short code. The record was not saved." under
  Short code.

Step 1.11 — Both invitations are spent (act-1-setup.md)
Role: Administrator — the platform operator
Route: /admin/invitations → /
Do: Reload the invitations list, then go Home.
Expect: Active invitations reads "No active invitations", because a used invitation leaves the list. The dashboard's
  count of registered users is two higher than in Step 1.1.

Step 3.32 — The daily nudge reminds the assessors (act-3-operations.md)
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

Step 3.55 — Prof Mbatha reads KGK's audit log (act-3-operations.md)
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

Step 3.56 — Prof Mbatha opens one entry (act-3-operations.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Under Failures only, open Dr Ndlovu's future-date create.
Expect: The Event card shows the time to the millisecond, category Command, action CreateActivityCommand, and result
  Failed with "Date observed: The date cannot be after today (…)." The Actor card shows Dr Ndlovu's user id, display
  name and IP address. There is no Payload card: only an Administrator reads the raw JSON. "Back to log" returns to the
  list.

Step 3.57 — devadmin reads the log across institutions (act-3-operations.md)
Role: Administrator — devadmin@wombat.local
Route: /admin/audit → /admin/audit/{Id:guid}
Do: Open Audit log, with From (UTC) set to the start of this act's sitting as in Step 3.55. Open one respondent's
  submission (SubmitMsfResponseCommand), then one of Dr Naidoo's TransitionActivityCommand rows.
Expect: The log holds every institution's rows and the rows with no institution. Among them are eight
  SubmitMsfResponseCommand rows whose actor is "system", and devadmin's own RunScheduledJobNowCommand. The submission's
  detail names no user and no display name, only a truncated address. Its payload shows the token and the answers as
  "[REDACTED]" (T101, T205). Dr Naidoo's payload shows the principal as "[PRINCIPAL]", and the data patch and note as
  "[REDACTED]".

Step 4.38 — Prof Mbatha reads the sitting in the audit log (act-4-annual-review.md)
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

Step 6.38 — devadmin rebuilds curriculum progress (act-6-catalogue.md)
Role: Administrator — devadmin
Route: /admin/curricula → /admin/curriculum-progress
Do: From Curricula in the menu, open Curriculum progress from the header and read it. Press Rebuild progress, and
  confirm.
Expect:
  - **The way in.** Curricula offers the Administrator an outline "Curriculum progress" in its header, where Home's
    Maintenance card used to link it (T335, flow 01), over its two sections, All curricula (each version's Status Open
    or Held back) and An institution's curricula (T392, Q18). On the page Curricula stays lit, and the trail reads Home
    › Curricula › Curriculum progress.
  - **The page.** It says that a rebuild credits every completed activity again against today's curriculum, judges only
    whether an EPA was active as of each completion, and runs as one transaction.
  - **The question.** The button asks first: "Rebuild curriculum progress?".
  - **The result.** "Curriculum progress was rebuilt.", in bold, takes the focus, with five figures: activities
    re-read, curriculum items credited, semester tallies written, stale tallies removed, and completions re-stamped.
    Stale tallies removed is 0: the move left nothing stale (T304).

Step A.2.1 — devadmin reads the scheduled jobs (appendix-cross-cutting.md)
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

Step A.2.2 — devadmin disables a job and enables it again (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /admin/jobs
Do: Disable msf-campaign-auto-close, try to run it now, then enable it again.
Expect: Disabling says "Job 'msf-campaign-auto-close' disabled." The row then reads "—" for its next run, its toggle
  says Enable, and its Run now is unavailable. Once enabled again, its next run is the next hour. The scheduler does not
  run a disabled job. The audit log records both changes.

Step A.2.4 — devadmin runs the weekly digest, and Mr Smit is skipped (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the application log.
Expect: "Job 'weekly-coordinator-digest' dispatched."; the row's last run is now, and it reads Succeeded. The log holds
  "WeeklyCoordinatorDigestJob: digests sent n; coordinators skipped: …", with at least one counted as "opted out of
  digest emails". No "Your weekly Wombat digest" mail greets Pieter.

Step A.2.6 — devadmin runs the digest again, and Mr Smit gets it (appendix-cross-cutting.md)
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

Step A.2.8 — devadmin runs the two reminders against aged work (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /admin/jobs
Do: Age the draft by 15 days and the request by 6 (see the Note). Then run activity-draft-nudge and
  assessor-pending-nudge now, and read the log.
Expect: The log holds a stub mail "You have draft activities waiting" beginning "Hi Nomsa,", listing "Mini-CEX
  (Paediatrics) — draft for 15 days". It also holds "Activities awaiting your assessment", beginning "Hi Fatima,",
  which lists "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 6 days". Each job logs one line counting whom it
  reminded and whom it skipped, and why (T151, T240).

Step A.2.9 — devadmin runs the other five jobs (appendix-cross-cutting.md)
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

Step A.2.10 — devadmin reads the run history (appendix-cross-cutting.md)
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

Step A.5.2 — Prof Mbatha opens the Administrator's own pages (appendix-cross-cutting.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled jobs.
Expect: For both, "You cannot open this page": "Your role (Institutional admin) does not open this page." and "If you
  need it for your work, ask the platform administrator.", with Go to Home. They are the Administrator's alone, her menu
  offers neither, and nothing in it is lit on the refusal.

Step A.5.13 — devadmin's menu has no System page (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /
Do: Read the menu.
Expect: The grouped menu of Step 1.1, with no System item: the flow 01 pick dropped it, with its placeholder.

Step A.6.1 — devadmin maintains the institution record (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: / → /admin/institutions → /admin/institutions/{Id:int}
Do: Open Institutions from the nav. Read the list, open KGK, set its contact email to
  `paediatrics@kgk.wombat.local`, and save.
Expect: The list holds the Demo Institution and Kgosi Kgari Teaching Hospital. Each row shows its short code, contact
  email, status (Active) and creation time, with an Edit named for it; Create institution heads the page. Saving says
  "Institution saved." The Administrator is offered the Status box and Deactivate.

Step A.6.2 — devadmin reads the system health card (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /
Do: Read the dashboard's System health and Users across institutions cards.
Expect: Database connection is green. The card's other two lines, Email queue and Last nightly job, are amber whatever
  their state, and each carries a task id: they are stubs, tied neither to the mail queue nor to Scheduled jobs
  (reported). The users card counts registered accounts.

Step A.6.9 — devadmin opens his own account (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /admin/users → /admin/users/{UserId}
Do: Find his own account in Users (type "devadmin" in Name or email and press Show) and open it.
Expect: The page's subtitle reads "Your own account: your roles and how you sign in." A note says it is his own
  account, so he cannot change its roles, lockout or password here, and another administrator can change his roles or
  lockout. It links to Change password on his own account. No Remove…, Add a role…, Password or Lockout is offered
  (T278), and no Pending invitations card (T376, Q9). Sign-in and access holds only Institutional sign-in: "No
  institutional sign-in is linked. Demo Administrator signs in with an email and a password." On Users his row reads Institution "Global (no institution)",
  Roles Administrator and Scope "—" (U14).

Step A.6.10 — The health endpoint (appendix-cross-cutting.md)
Role: Anonymous — the uptime monitor
Route: /health
Do: Request `/health` without signing in.
Expect: Status 200 with the body "Healthy". The check includes the database (T097), and the body stays terse because
  the endpoint is public.

Step A.7.11 — devadmin on his phone (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: / → /admin/jobs → /admin/jobs/runs → /admin/institutions
Do: At 390 px, open Scheduled jobs, Run history and Institutions.
Expect: The jobs table scrolls inside its container, with each Run now and toggle reachable. The run-history filters
  stack.
```

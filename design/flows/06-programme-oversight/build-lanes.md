# T358 step 6 — rules common to every build lane (flow 06, watching the programme)

Adapted from `design/flows/05-trainee-progress/build-lanes.md` (T355). You are one lane of the build of flow 06 of
Wombat's GUI restructure, task T358 (`execution/tasks/in_progress/T358-*.md`). The design is final: Claude Design canvas
round 3 (version `1791182119-de2f`, `round-3-check.md`). The lane plan is at the end of this file; the names two or more
lanes share are in `build-contracts.md`, beside it.

Master is at `4d470f4b` or later: T290's committee item and T298's surname tie-break are merged (`14dac868`), so the
committee card already reads the institution's current registrars, ties by surname. Flow 06 builds on that read; it does
not write it again.

## Read first (in your worktree)
- `design/flows/06-programme-oversight.md` — the brief: the cast, the 20 runbook steps, what is wrong today, and § 1's
  **fence** ("WHAT FLOW 06 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT").
- `design/flows/06-programme-oversight/round-2-ask.txt` — the pick (A, "Two lists and a record"), Q1–Q10 answered, and
  corrections C1–C11. `round-3-ask.txt` — the operator's E1–E6 and the review's corrections by number (1, 3, 6–33).
- `design/flows/06-programme-oversight/round-2-review.md` — the 33 findings with file:line evidence; `round-3-check.md` —
  the six things **the build carries** (no pronouns in any shipped word; the ended page from flow 05's ended cards; the
  steps; the registrar page's loading keeps trail and header; no inline styles; board detail) and the settled reading of
  r6 (a withdrawn registrar's request is listed and remindable like any other).
- `design/flows/06-programme-oversight/round-3/project/` — the accepted boards. The substance is in the five component
  boards, `R2-Home`, `R2-Trainees`, `R2-Registrar`, `R2-Waiting` and `R2-Menus`: each draws every state from the
  `renderVals` script at its foot, which holds each state's data and words (the `R2-*-1280`/`-390` and `R3-768`
  wrappers only pick states and a width).
  - `R2-Spec.dc.html` is the contract: round 3 item by item, the NEW and changed components and their classes, the
    DESIGN.md rule texts ready to paste, the colour pairs, the targets, and the accessibility review.
  - `R2-Steps.dc.html` lists every runbook Expect that changes and the new steps 3.55–3.57, A.5.14–A.5.16 and A.7.9.
  - `flow06-r2.css` part 3, "NEW for flow 06", is the canvas's proposal for the five new classes. It is a picture, not
    code to copy: rebuild each rule in `app.css` on the spacing scale. Parts 1 and 2 (tokens, the canvas frame `.r2-*`)
    never ship. Its part-3 comment lists the built classes the boards reuse: reuse them, do not redefine them.
  - A bracketed figure on a board (`[date]`, `[n]`, `[assessor]`) is read from the data; a board caption is never a
    shipped word.
- `design/flows/06-programme-oversight/build-contracts.md` — the names you provide or rely on. Use them exactly.
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS: § The NavMenu (the nav table, the
  flat limit, the owner table), § Dashboard layout grid (the staff target cards, the progress figures, the progressbar
  rule), § Page-level patterns "List page" and "Dashboard page". CLAUDE.md — conventions and footguns, § Roles
  (T056's scope-aware powers), § EF Core migrations.
- Flow 04's and flow 05's `build-contracts.md` — the vocabulary flow 06 reuses: `ActivitySummaryDto`'s waiting fields
  (`IsOverdue`, `WaitedDays`, `SubjectName`, `DisplayName`, `Holder`, `NomineeName`), `WaitingForYou`, `WaitingWords`,
  `WaitingList`, `ActivityLink`, `ActivityRowNames.Waiting`, `.needs-you-row--overdue`, `.waited-cell`, `.card-empty`,
  `.waiting-more`; `ProgressWords`, `QuotaText.Iso`, `.progress-row-link`, `.count-figure`/`.count-meta`,
  `.clinic-table--index`, `.section-error`, `EpaProgressTable`, `EntrustmentStandingPanel.EpaHref`, `TrajectoryChart`.

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Use
  absolute paths for every command.
  - Never touch `C:\dev\Wombat` itself (master).
  - Never merge, push or switch branch. The repo has a remote (GitHub); no lane pushes to it, and the integrator does
    not either.
- Commit in your worktree as you go.
  - Write each message to a file and use `git commit -F <file>`.
  - End every message with the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns (the plan below). `app.css` and
  `execution/architecture/DESIGN.md` are shared by section: each lane writes only under its own heading. If you must
  touch a file another lane owns, keep the change minimal and name it in your report. A wave-1 lane that changes a shape
  the Web project reads keeps the Web project compiling with the least change there; wave 2 rewrites the page.

## Never
- Touch the apps running on ports 5080 and 5180, or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any admin credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves; each works in its own throwaway `it_<guid>` schema, which is expected.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Upgrade MediatR (12.x is the ceiling), add Bootstrap/MudBlazor/Radzen/jQuery, put raw hex colours outside `:root`,
  inline `style=` in shipped markup (skeleton sizes are classes, round-3-check 5), or use `<i class="bi">`.
- Load anything from a third-party host at runtime (the CSP allows only 'self'); no inline scripts or `onclick`. No
  live filtering: a list's filters change nothing until Show (Spec § List page, the NEW pattern).
- Write a figure as "n / m", a percentage or a lifetime total; put the bare word "year" beside a training year and an
  academic year; print "inactive", "Trainees in programme" or "Pending reviews" anywhere (Q10); print a role's key
  ("CommitteeMember") where its label goes; print a date in any form but ISO on these pages (T325; flow 05's D1).
- Use a pronoun in any shipped word (round-3-check 1): the person's name, "the registrar" or "the assessor".
- Draw "Reassign", or any button that does nothing (C5). Count or plot multi-source feedback (D8, D36).
- Change what the fence keeps (brief § 1): the shell and Home's frame (but the Coordinator's header action, Spec 26);
  the other-role line's words, place and Open it; flow 04's list vocabulary ("Waiting 8 days", the Overdue badge,
  rows named per row) and `WaitingWords`' existing members; flow 05's progress words and pages, which admit only the
  registrar (My progress changes only by extracting its ended view, word for word); the bodies of Decisions due,
  Decision panels, a committee review and Entrustment decisions.
- List a request whose next move belongs to a role rather than a named person (E3), the reader's own requests (E4), or
  read the programme by the union of the roles held (E4): every new read is scoped by the role it is read as.
- Refuse a reminder because the assessor opted out of digest emails (E1), or let a reminder move the request: it never
  touches `Activity.UpdatedOn`, its state or its transitions, and the registrar is not told (C4c).
- Change a seed, the credit engine, or any migration but the one A0 writes (below).
- Print an exception's text on a page: load errors use the fixed words, and the failure goes to the log (T329, T272).
- Name a test helper `StyleSheet.cs` (it is the same file as `Design/Stylesheet.cs` on Windows).

**One command and one migration, unlike flow 05.** Flow 06 adds `SendActivityReminderCommand` and one migration (the
reminder table, and the admission day, decision D1 in `build-contracts.md`). Their rules:
- The migration is lane A0's alone, written once. If `dotnet ef migrations add` cannot run, hand-write it **with its
  `.Designer.cs`** (`[Migration]` and `[DbContext]` attributes; copy the latest Designer, `20260927084116_T335_…`,
  change the timestamp, the class name and `BuildTargetModel`) and update `ApplicationDbContextModelSnapshot.cs` to
  the final model (CLAUDE.md § EF Core migrations). Without the Designer, `MigrateAsync` silently skips it. Read the
  generated migration before trusting it: a stale build writes an empty one.
- `AuditPipelineBehavior` commits a failed handler's staged rows (the "known trap": a handler that stages and then throws
  has its staging committed with the failure row; only a refused save is discarded, T201). So the command validates
  everything first (scope, still waiting, the same state and `UpdatedOn` it was read with, the same nominee, no reminder
  yet today, the recipient's account), returns a refusal as a result and never by throwing, and stages its one row only
  once nothing can refuse it.

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density. This codebase comments *why*, with task ids: cite
  T358 and the reviews' ids, e.g. "(T358, E3)", "(T358, C4e)", "(T358, round 3 item 21)", "(T358, review 12)".
- Application handlers go through `IApplicationDbContext`, never EF types beyond its `DbSet` and the query operators.
  Web components use `IScopedSender` and DTOs, never Domain types in `.razor`. A handler that reads `Set<Activity>()`
  must put its rows through `WhereReadableBy` (`ActivityReadBoundaryTests`; add every new DTO that carries activity
  rows to its `ActivityDataDtoNames`) and read no literal state key (`DashboardStateLiteralTests`). A read about a
  trainee goes through `ProgrammeScope` (A0) or `TraineeScopeResolver.MayReadAsync`; an out-of-scope or unknown id
  reads as not found (null), never a refusal.
- "Today" is `QuotaCalendar.Today(TimeProvider)` on the South African calendar, and a timestamp's day is
  `ProgrammeCalendar.DateOf(utc)`; never `DateTime.UtcNow`, `DateTime.Today` or `DateOnly.FromDateTime(UtcNow)` (T325).
  Times shown are SAST with the zone (`ActivityMoments.When`); dates are ISO (`QuotaText.Iso`).
- Straight apostrophes in app strings; every sentence that counts has its singular ("1 of 2 current registrars has").
- Change a test deliberately, never loosen it to pass. Keep the Design tests (`tests/Wombat.Web.Tests/Design/*`)
  truthful: `DefinedClassTests` fails a class a page uses that `app.css` does not define, so a class is defined by the
  lane that owns it (`build-contracts.md` § CSS, by owner) and used by the others only after the merge that brings it.
- Each lane adds its classes under its own `/* Flow 06: … (T358) */` heading in `app.css`, and its DESIGN.md rule under
  the section the plan names; the integrator makes the account one after the merge.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`: the integrator's
  runbook lane does both after merging. T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green; A0 adds the
  three routes to `coverage.md` § Pages only, if the guard asks. Name in your report every runbook line whose Expect
  your change alters.
- Before finishing:
  - `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release` clean, with no new warnings.
  - Then all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web, Integration
    (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message is returned to the integrator, not shown to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and anything another lane or the integrator must know: the contracts you provide or rely on
  (exact type, member and parameter names, and any way they differ from `build-contracts.md`), files you touched outside
  your list, runbook steps whose Expect your change alters.

## The lane plan

Six lanes in three waves, then the runbook lane. Worktrees under `C:\dev\Wombat\.claude\worktrees\<branch>`.
Integration branch **`t358`**, cut from master; each wave's lanes are cut from `t358` and merged back into it
(`Merge lane <X> (<branch>)`), and the whole is squashed into one commit on master. Nothing is pushed. Ownership is
disjoint: a file is in one lane's list (`app.css` and DESIGN.md by section), and a test file goes with the code it tests
unless named.

**Why a wave 0, and why the Homes' reads are lane B's** (refining the integrator's sketch, from the code):
- Both wave-1 reads scope by the acting role, so the scope is one piece both build on; the reminder table and the
  admission day are one migration, which both wave-1 lanes need compiled (A1 writes reminders, A2 reads admission);
  and lane B's menu items name the three new pages' types (`NavItem.Page`), which must exist before wave 2. So a short
  lane A0 lays all three down first.
- The four dashboard handlers each feed cards from both wave-1 reads (the admins' Home holds Waiting for assessors,
  A1's, beside Registrars and Targets by EPA, A2's; the Coordinator's holds Waiting and Nothing filed). Two lanes
  editing one handler in parallel would collide, so wave 1 provides readers and lane B composes the Homes, Application
  and Web together. Wave 1 keeps today's dashboards compiling with the least change.
- `ReminderAction` and `WaitingList`'s staff reading are drawn on two wave-2 pages (the waiting list, D; the registrar
  page, C), so they are wave 1's, with the command they drive (A1), as flow 05 put the block links in A2.

### Wave 0 (one lane, before wave 1): the base

**Lane A0 "base"** (`t358-base`). The acting role's scope, the persistence, and the three pages' addresses.
- Scope:
  - NEW `ProgrammeScope` (Application): which role a programme page reads as, what that role's scope is (the
    institution for a Committee member or a Coordinator; the institution and the speciality's sub-specialities for a
    Speciality admin; the institution and the sub-specialities for a Sub-speciality admin), its name for the subtitle,
    and the two narrowings (trainee profiles; activities by their institution and speciality stamps). Null for someone
    acting as a trainee (`TraineeScopeResolver.ActsAsTrainee`, T185), for a role not held or not one of the four, and
    for a caller with no institution. Never the union (E4).
  - NEW Web helper `ProgrammeReadAs.RoleFor` (decision D2): the acting role when the page admits it, else the first
    admitted role held in `DashboardPriority.Order`.
  - The persistence: NEW `Domain.Activities.ActivityReminder` and its configuration (table `ActivityReminders`, unique
    `(ActivityId, SentOnDay)`, no navigation to `Activity` so a save never touches it); `TraineeProfile.AdmittedOn`
    (D1), set by `AdmitTrainee` and `DevUserSeeder` from the South African day; ONE migration
    `<timestamp>_T358_ActivityRemindersAndAdmission` with its Designer and the snapshot, backfilling `AdmittedOn` to
    `ProgrammeStartDate`.
  - The three routed pages as shells (title, `PageHeader` with its title, nothing else): `/programme/trainees`,
    `/programme/trainees/{ProfileId:int}`, `/programme/waiting`, with their `[Authorize(Roles = …)]`; each in
    `NavOwners.Table` as `[]` with the comment "until lane B gives it its owner (T358)", so `ActiveNavItemTests` holds.
    Not in any menu: the nav never offers an unbuilt page.
- Files owned: `src/Wombat.Application/Features/Programme/ProgrammeScope.cs` (new),
  `src/Wombat.Web/Navigation/ProgrammeReadAs.cs` (new), `src/Wombat.Domain/Activities/ActivityReminder.cs` (new),
  `src/Wombat.Domain/Identity/TraineeProfile.cs` (`AdmittedOn` only),
  `src/Wombat.Infrastructure/Persistence/Configurations/Activities/ActivityReminderConfiguration.cs` (new),
  `…/Persistence/Configurations/TraineeProfileConfiguration.cs`, `…/Persistence/ApplicationDbContext.cs` (the
  `DbSet`), the migration, its Designer and the snapshot, `src/Wombat.Application/Features/Trainees/AdmitTrainee.cs`,
  `src/Wombat.Infrastructure/Identity/DevUserSeeder.cs`; `src/Wombat.Web/Components/Pages/Programme/ProgrammeTrainees.razor`,
  `ProgrammeTraineeDetail.razor`, `WaitingForAssessors.razor` (new shells); `src/Wombat.Web/Navigation/NavOwners.cs`
  (the three `[]` rows only); `execution/knowledge/scenario-paediatrics/coverage.md` (§ Pages, the three routes' rows,
  only if T294's guard asks).
- Tests: Application — NEW `Features/Programme/ProgrammeScopeTests.cs` (each role's scope and name; a Speciality admin
  who also holds Committee member reads the speciality when acting as Speciality admin and the institution when acting
  as Committee member; a role not held, a trainee rung, no institution: null; `Profiles` and `Activities` never reach
  another institution, an un-stamped activity, or another speciality), `AdmitTrainee`'s tests (the day set, from the
  clock). Infrastructure — the configuration (the unique index, the table). Integration — the migration applies on
  Postgres; two reminders on one activity and one South African day are refused by the index, on two days accepted.
  Web — `ProgrammeReadAsTests`; the shells render their h1 and refuse the roles their attribute leaves out
  (`PageAccess`).
- Provides: § A0 of `build-contracts.md`. Consumes: nothing new.

After wave 0: the integrator merges A0 into `t358`, runs the six suites, and cuts wave 1 from it.

### Wave 1 (parallel): the reads, the reminder and the words

**Lane A1 "waiting"** (`t358-waiting`). What waits for a named assessor, the reminder end to end, and their words.
- Scope:
  - NEW `WaitingForAssessorsReader` and `ListWaitingForAssessorsQuery` (Q3, E3, E4, C3, C7, C11; review 12): the
    activities in the scope that await a reviewer by their pinned workflow (`ActivityWaiting.LoadAwaitingReviewerAsync`,
    the predicate today's card and the nudge read), whose holder is one named person (`ActivityHolders.Resolve` is
    `Person`: a `field:` nominee), whose subject is not the reader, through `WhereReadableBy`; oldest first by
    `UpdatedOn`, then id; overdue and the whole-day wait exactly as `WaitingForYou` (flow 04's `AssessorDueDays`); each
    row's holder named, its last reminder and whether one was sent today, and whether its nominee cannot be reminded
    (from the recipient port, so the deactivated row says so before anyone presses, E3's w9); the With filter's names
    from every row's nominee; the Waiting and With filters and a subject filter (the registrar page's section); a page.
    One query for Home's card and the page (E4).
  - The reminder (Q4; C4; E1; review 1–3):
    - NEW Application port `IReminderRecipients` and its Infrastructure implementation over `ReminderRecipientPolicy`'s
      projection and `UserDeactivation` (the policy stays internal and the digests' rule unchanged): a reminder about
      one request refuses a missing account, a deactivated one and a missing address, never an opt-out (E1).
    - NEW `SendActivityReminderCommand` (the audit-pipeline rules above): the four refusals (deactivated, no email
      address, no such account, moved meanwhile), plus "already reminded today" (the same-day block for every staff
      member, settled in round 3) and not found (D7); records `ActivityReminder` (who, when, the South African day,
      whom), then hands one mail to `IEmailSender`. Never reads `Activity` tracked; never sets `UpdatedOn`.
    - The mail: `AssessorPendingNudgeEmail.BuildReminder`, the nudge's template for one request, its own tags
      (`reminder`, `assessor-reminder`); the days phrase "waiting less than a day" / "waiting 1 day" / "waiting 8 days"
      for both mails (review 3).
    - `ErasureExecutor` pseudonymises `ActivityReminder.SentByUserId` and `AssessorUserId` (review 1), as it does
      `ActivityTransition.ActorUserId`.
  - The setting (review 11; round 3 item 11): `DashboardThresholds.AssessorNudgeDays` (5) read by
    `AssessorPendingNudgeJob` (its cutoff and its `Description`) and by `DataRights.razor`'s help text (its words
    unchanged, E1); `CoordinatorStallDays` removed from the class, `appsettings*.json` and the Coordinator handler (its
    stalled read now uses `AssessorDueDays`, compile-only: lane B rewrites the handler).
  - `PostgresErrors.UniqueViolation` and `IsUniqueViolation(Exception?)`, beside the foreign-key pair (D6).
  - Web, the words and two components:
    - `Shared/Activities/WaitingWords` gains the staff reading's members (E6: "With Mohammed Patel" on its own line,
      flow 04's "Waiting 8 days" kept), the rule lines with the nudge's sentence and the page's T351 sentence, the
      overflow for Waiting for assessors, the filtered and no-match headings, the scope subtitle, the empty words. Its
      existing members are unchanged (the fence).
    - `Shared/Activities/WaitingList` gains the staff reading (`WithNominee`) and a row action slot (`RowAction`), the
      flow 04 rows otherwise byte-equal.
    - NEW `Shared/Programme/ReminderWords` and NEW `Shared/Programme/ReminderAction.razor` (C4; round 3 items 22, 23,
      29): the Waiting cell's tail (the last "Reminded … by …" line, then the button, "No reminder: …", or nothing), the
      `ConfirmDialog` opening on Don't send, in flight ("Sending…", Don't send disabled), the command sent, the result
      handed to the host (`OnResult`), which shows it in its own `ActionResult` and moves the focus there.
  - DESIGN.md: § Page-level patterns "List page", the NEW pattern "filters applied with Show" (Spec's text, round 3
    item 31), which lanes C and D follow.
  - CSS (`/* Flow 06: the reminder and the filter bar (T358) */`): `.reminder-action`, `.filter-actions`, and their
    rules below 641 px (44 px, full width; round 3 items 22, 25).
- Files owned: `src/Wombat.Application/Features/Programme/Waiting/*` (new), `…/Programme/Commands/SendActivityReminder/*`
  (new), `src/Wombat.Application/Common/Interfaces/IReminderRecipients.cs` (new),
  `…/Common/Options/DashboardThresholds.cs`, `…/Common/Persistence/PostgresErrors.cs`,
  `…/Common/Email/Templates/AssessorPendingNudgeEmail.cs`, `…/Features/Activities/Dtos/ActivityDtos.cs` (the init
  members in the contracts), `…/Features/Dashboards/Coordinator/*` (compile-only);
  `src/Wombat.Infrastructure/Scheduling/ReminderRecipients.cs` (new), `Scheduling/Jobs/AssessorPendingNudgeJob.cs`,
  `…/DataRights/ErasureExecutor.cs` (the reminder rows only), the Infrastructure DI registration (one line);
  `src/Wombat.Web/appsettings*.json` (the setting), `Components/Shared/Activities/WaitingWords.cs`, `WaitingList.razor`,
  `Components/Shared/Programme/ReminderWords.cs`, `ReminderAction.razor` (new), `Components/Pages/Profile/DataRights.razor`
  (the help text's number only); `app.css` (its section); `execution/architecture/DESIGN.md` (the List page pattern).
- Review findings: 1, 2, 3, 4, 5, 10 (the reads' half), 11, 12, 22, 23, 25, 29, 31 (the select's name). Rounds: Q3, Q4,
  C3, C4, C7, C11; E1, E3, E4, E6; the settled same-day rule.
- Tests: Application — NEW `Features/Programme/WaitingForAssessorsTests` (E3: a role-held review, an MSF release and
  an empty nominee field are never listed; E4: a Speciality admin who also holds Committee member reads the speciality
  only; the reader's own request never; another institution's never; oldest first; overdue at 7 × 24 h; the With
  names from every row; the filters; the deactivated nominee marked; the last reminder and today's; paging), NEW
  `SendActivityReminderCommandTests` (each outcome; the opt-out still sent, E1; a second sender the same South African
  day refused, the next day sent; `UpdatedOn`, state and transitions unchanged; nothing staged on a refusal; the mail's
  subject, tags and line). Infrastructure — `ReminderRecipientsTests`, `AssessorPendingNudgeJobTests` (the setting; the
  days phrase), `ErasureExecutorTests` (the reminder's two ids). Architecture — `ActivityReadBoundaryTests` with
  `WaitingForAssessorsDto`. Integration — the Postgres read of the waiting list beside `DashboardWaitingPostgresTests`,
  and the command's lost race (the unique index) answering "already reminded today". Web — `WaitingWordsTests` (the new
  members; the old byte-equal), `WaitingListTests` (the staff reading; flow 04's rows unchanged), NEW
  `Programme/ReminderWordsTests`, `Programme/ReminderActionTests` (every `w2`–`w8` and `r2d`/`r2r`/`r2x` state's words
  and focus hand-off; the button named for the assessor and the activity, "(1 of 2)" when two read the same), a
  `StylesheetRuleTests.Flow06` rule for the 44 px below 641 px.
- Provides: § Reads [A1], § The command [A1], § Words [A1] and the two components. Consumes: A0.

**Lane A2 "roster"** (`t358-roster`). The programme's registrars, one registrar, the EPAs' coverage, filing, their words.
- Scope:
  - NEW `ProgrammeRosterReader` and `ListProgrammeTraineesQuery` (Q1; C6, C7; E5; review 9, 16): the scope's current
    registrars (`ProgrammeScope.Profiles`, then `TraineeScopeResolver.KeepCurrentAsync`, T238); per registrar the
    training year, this semester's and this year's "n of m" (the `QuotaProgressCalculator` tallies My progress reads),
    the three EPAs furthest from this window's target (D11), the Short on EPA's tally, last filed; an exempt registrar
    listed last with why, in words (computed: started part-way, or not started, review 9); order fewest met first, then
    surname, first name, id (T298); the Short on, training-year and Nothing filed filters; a page.
  - NEW `FilingMoments` (E5): when an activity was filed (left its draft: the moment `ActivityTransition
    .DaysAfterEncounter`'s filing is judged by, `Workflow.LeftInitialStateLeadingOn`, or its create when it is itself the
    filing; a recorded MSF counts); "Nothing filed in 30 days" is a current registrar admitted at least 30 days ago with
    nothing filed in the last 30 (the later of admission and today − 30, so a registrar admitted last week is not
    listed). `WeeklyCoordinatorDigestJob`'s inactive list reads the same rule (E5).
  - "Targets by EPA" (Q5; review 13): `CurriculumCoverageReader` orders the EPAs fewest registrars met first, then code
    (KGK-001 leads at the story's end), and its trainees by share, then surname (T298, moved from the committee handler
    so every caller agrees); `EpaTargetCoverage` gains the owning institution's name. Nothing else of the reader moves.
  - NEW `GetProgrammeTraineeQuery` (C2; review 6): one registrar by trainee-profile id, for the registrar page: only the
    user's preferred profile (`TraineeScopeResolver.PreferredProfiles`), only in the reading role's scope, only while
    an account holds that user id (an erased profile is a pseudonym no account holds); else null ("Page not found").
    An ended profile opens (read-only, r6). The page then sends flow 05's queries by the returned user id.
  - NEW `ListReviewsForProgrammeTraineeQuery` (C9): that registrar's reviews, any state, that the reader may open
    (`CommitteeReviewReadAccess.ReadableAsync`, the review page's own ladder), newest scheduled first.
  - Web, the words (one class per surface):
    - NEW `Shared/Programme/ProgrammeWords`: the roster's figures and captions, the exempt badge and reason, Furthest
      short's line, Last filed, the subtitle ("Current registrars at Kgosi Kgari Teaching Hospital, read as Committee
      member · Semester 2, 2026"), the headings that count the answer and their rule lines, the no-match sentence and
      "what was asked", the empty state, the page's loading and error words, Home's Registrars card and Nothing filed card
      words (rule, rows' meta, badge in words, "3 more in Programme trainees.", empties).
    - NEW `Shared/Programme/TargetsByEpaWords`: the programme figure "2 of 5" over "registrars met this semester" /
      "registrars met in 2026", the one exemption wording, the rule line, the cadence line with KGK-001's owner and "every
      registrar has met it", the link's hidden tail, the empty.
    - NEW `Shared/Programme/RegistrarWords`: the registrar page's subtitle, section headings, section errors, empties
      ("Nothing of Nomsa Mahlangu's waits for an assessor.", "No review is scheduled for Nomsa Mahlangu."), the page
      error ("Programme trainee"), loading, and the ended record's staff words (round-3-check 2; review 8).
    - NEW `Shared/Programme/ProgrammeLinks`: the three addresses and the filter query names.
  - Keep `CommitteeMemberDashboard.razor`, `SpecialityAdminDashboard.razor` and `SubSpecialityAdminDashboard.razor`
    compiling with the least change if a DTO they read moves; lane B rewrites them.
- Files owned: `src/Wombat.Application/Features/Programme/Trainees/*` (new), `…/Programme/Filing/FilingMoments.cs`
  (new), `…/Features/Curricula/Quota/CurriculumCoverage.cs`, `…/Features/CommitteeDecisions/ListReviewsForProgrammeTrainee.cs`
  (new); `src/Wombat.Infrastructure/Scheduling/Jobs/WeeklyCoordinatorDigestJob.cs`;
  `src/Wombat.Web/Components/Shared/Programme/ProgrammeWords.cs`, `TargetsByEpaWords.cs`, `RegistrarWords.cs`,
  `ProgrammeLinks.cs` (new). Minimal compile-only edits: the three dashboards named above, and the committee handler
  only if the reader's new order makes its own sort redundant.
- Review findings: 6, 8 (words), 9, 13, 16, 19 (the moments the tests use), 20, 23, 28 (the reused captions), 31 (count
  badges in words), 33 (one no-match pattern, "Registrars" and its line). Rounds: Q1, Q2, Q5, Q10; C2, C6, C7, C9; E5.
- Tests: Application — NEW `Features/Programme/ProgrammeTraineesTests` (the cast at 3.52: Mahlangu's and Ndlovu's
  PAED-002 "1 of 3"; Furthest short Dlamini PAED-002, 003, 005, Mahlangu PAED-001, 003, 005; Short on PAED-002's order
  du Plessis, Dlamini, Molefe, then Mahlangu, Ndlovu; ties by surname at 2.33; an exempt registrar last with its reason;
  an ended, erased or locked registrar not current; another institution's never; each acting role's scope), NEW
  `FilingMomentsTests` (a draft is not filed; a request is filed when sent; a type born requested at its create; a
  return and re-submission is one filing; an MSF record counts; a registrar admitted 10 days ago with nothing is not
  listed, 31 days ago is), NEW `GetProgrammeTraineeQueryTests` (preferred profile only: an older profile's id is null;
  out of scope, unknown, erased: null; ended opens), NEW `ListReviewsForProgrammeTraineeQueryTests` (a member reads only
  their panels' reviews; a Coordinator the institution's), `CurriculumCoverageTests` (the order; T298's tie pinned).
  Infrastructure — `WeeklyCoordinatorDigestJobTests` (E5). Integration — the roster's Postgres read and the filing rule.
  Web — `Programme/ProgrammeWordsTests`, `TargetsByEpaWordsTests`, `RegistrarWordsTests` (every phrase the boards give
  for its surface; singular and plural; no pronoun).
- Provides: § Reads [A2] and § Words [A2]. Consumes: A0.

After wave 1: the integrator merges A1 and A2 into `t358`, runs the six suites, and corrects `build-contracts.md` to
what was built (its "As built after wave 1" section) before wave 2 starts.

### Wave 2 (parallel): the pages

**Lane B "homes"** (`t358-homes`). The four Homes, Application and Web; the menus and the owner table.
- Scope:
  - The four dashboard reads (Application), each one read behind `DashboardFrame` (a failure is Home's one load error):
    - Committee member: Registrars (the first five of `ProgrammeRosterReader`'s rows, its count) and Targets by EPA.
    - Speciality admin and Sub-speciality admin: Waiting for assessors (A1's read, first five, as that role), Registrars,
      Targets by EPA. "Pending reviews", "Trainees in programme" and every "inactive" go (Q3, Q10).
    - Coordinator: Waiting for assessors, Nothing filed in 30 days (A2's rule, its first five), Invitations nearing
      expiry (the role by its label, the expiry three South African days ahead, ISO; the scope as today).
  - The four Homes (Web; R2-Home c1–c11, a1–a7, k1–k6): the cards in the board's order and spans, each a section
    named by its title (`HeadingId`), its count badge in words; every row its own link (a registrar's name to the
    registrar page, an EPA's name to Programme trainees filtered Short on it, an activity's to its page; an EPA every
    registrar has met is text); "n more in <list>." past five; every preview card keeps its foot while its list is in the
    menu, empty included (round 3 item 27); Targets by EPA has no foot. The Coordinator's "Quick action" card goes and
    "Start an MSF campaign" is Home's header action (`HomeFrame.ActionFor`, Q6; review 26). Loading and load error
    stay the frame's (Try again answers into its result region, round 3 item 33).
  - `EpaTargetCoverageList` changed (one title "Targets by EPA" on every Home; the figure in its own column over its
    caption; the bar `aria-hidden` under its words; the row a `.coverage-row`); NEW `RegistrarRoster` (the Registrars
    card's body and the Nothing filed card's rows); `DashboardCard` gains a badge tone (D10).
  - The menus (`NavItems`; R2-Menus; Q7; C10; E2): Programme trainees for the four roles; Waiting for assessors for
    both speciality admins and the Coordinator; Entrustment decisions for both speciality admins; Decision panels for
    the Coordinator; the flat limit counts the role's links and Home only, never the personal links (E2), with its
    comment and `For`. The owner table (`NavOwners`; round 3 item 10): the registrar page under Programme trainees for
    the four roles; an activity under Waiting for assessors for both admins and the Coordinator; Entrustment decisions
    leaves the table (it is an item now); the three A0 `[]` rows go. The decision panel's row is unchanged (D4).
  - DESIGN.md (Spec § DESIGN.md text): § The NavMenu (the flat limit, the nav table, "What the nav never holds", the
    owner table); § Dashboard page (header actions; the link rule; "Guard the eager reads"); § Dashboard layout grid
    (the staff target cards; the progress figures' programme form and the one exemption wording; the progressbar rule's
    `aria-hidden` exception).
  - CSS (`/* Flow 06: the oversight Homes (T358) */`): `.roster-row`, `.roster-exempt`, `.coverage-row`, and their one
    column at `max-width: 900px` (round 3 item 21); the old `.progress-row-head` uses on these cards go.
- Files owned: `src/Wombat.Application/Features/Dashboards/{CommitteeMember,SpecialityAdmin,SubSpecialityAdmin,Coordinator}/*`;
  `src/Wombat.Web/Components/Pages/Dashboards/CommitteeMemberDashboard.razor`, `SpecialityAdminDashboard.razor`,
  `SubSpecialityAdminDashboard.razor`, `CoordinatorDashboard.razor`, `Components/Shared/EpaTargetCoverageList.razor`,
  `Components/Shared/Programme/RegistrarRoster.razor` (new), `Components/Shared/DashboardCard.razor` (the tone),
  `src/Wombat.Web/Navigation/HomeFrame.cs`, `NavItems.cs`, `NavOwners.cs`; `app.css` (its section);
  `execution/architecture/DESIGN.md` (the sections above); tests under `tests/Wombat.Application.Tests/Features/Dashboards/`
  (the four roles'), `tests/Wombat.Web.Tests/Dashboards/*` (`HomeFrameTests`, `WaitingCardsTests`, `DashboardCardTests`,
  NEW `CommitteeHomeTests`, `ProgrammeAdminHomeTests`, `CoordinatorHomeTests`, `RegistrarRosterTests`),
  `Progress/QuotaProgressRenderingTests.cs` (the list), `Navigation/*` (`NavMenuAuthorizationTests`,
  `ActiveNavItemTests`, `DashboardLinkAuthorizationTests`, `PageAccess.cs`), `Design/NarrowLayoutTests.cs`.
- Review findings: 10 (owners), 13, 14 (the Homes' counts in tests), 18, 21, 26, 27, 28 (the Homes), 31 (badges), 32,
  33 (the Homes). Rounds: Q1, Q3, Q5, Q6, Q7, Q10; C10; E2, E3 (the Coordinator's card), E6 (the rows).
- Tests: bUnit for every `R2-Home` state (typical, flow 04's line one waiting and one overdue, all zeros in surname
  order, the external member, one exempt, nobody to show, the story's end with KGK-001 first, heavy with "3 more in
  Programme trainees." and "9 more wait in Waiting for assessors.", loading, load error; the admins' and the
  Sub-speciality admin's; the Coordinator's, with the header action from the start and the invitation "· Trainee",
  "expires 2026-10-07"); each row a link of its own, no card a link around them; no "inactive", no slash figure, no
  "Pending reviews"; the menus of the four roles and their DESIGN.md table (`DesignMdNavTable_MatchesTheMenu…`); the
  Coordinator's eight role links flat (E2); the owners (the registrar page lights Programme trainees with the trail
  Home › Programme trainees › <name>; an activity lights Waiting for assessors for the three; Entrustment decisions lights
  itself with no trail); every Home link admits its role (`DashboardLinkAuthorizationTests`).
  `StylesheetRuleTests.Flow06Homes` for the 900 px collapse.
- Consumes: A0's pages and scope; A1's reader, `WaitingWords`' staff members and `WaitingList.WithNominee`; A2's
  readers, `ProgrammeWords`, `TargetsByEpaWords`, `ProgrammeLinks`. Provides: the card ids, the nav items' names.

**Lane C "trainees"** (`t358-trainees`). Programme trainees and the registrar page.
- Scope:
  - `ProgrammeTrainees.razor` (`/programme/trainees`; R2-Trainees t1–t10): header "Programme trainees" and the scope's
    subtitle; the filter bar (Short on, Training year, Nothing filed in 30 days; Show; Clear filters once any is set)
    applied with Show and carried in the address (A1's DESIGN.md pattern; D3); the list's heading counts the answer and
    takes the focus after Show and after a page turn; the rule line; the table `clinic-table--stack clinic-table--index`
    with its caption, the registrar a `th scope="row"` link, each stacked cell labelled; its three column sets (the
    roster, Short on, Nothing filed); the exempt row; no match ("No registrar matches these filters.", what was asked,
    Clear filters); empty (no form drawn); loading (the filters stand, skeleton rows, the status); load error (fixed
    words, Try again into the result region); `PagerControls`, 20 a page, labelled.
  - `ProgrammeTraineeDetail.razor` (`/programme/trainees/{ProfileId:int}`; R2-Registrar r1–r8; C2, C8, C9): `GetProgrammeTraineeQuery` first; null
    is flow 01's Page not found, word for word. h1 the registrar's name as stored, tab "<Name> · Wombat", subtitle
    `RegistrarWords.Subtitle`. Sections in C9's order, each a `section` named by its `h2.epa-section-title`, each read
    and failing apart in its own sentence with its own Try again (review 30): This period (flow 05's figures and ends
    line, staff words); EPAs (`EpaProgressTable` with its new names-as-text parameter); Entrustment against Annexure A
    (`EntrustmentStandingPanel` whole, `Self` false, `EpaHref` to `#trajectory-<EpaId>` for each EPA charted here, else
    text); Rating trajectories (`TrajectoryChart` per rated EPA over the academic year containing today, h3, D8); Waiting
    for assessors (A1's read with the subject filter, `WaitingList.WithNominee` and `ReminderAction` as `RowAction` for a
    role that may remind; its result alert in the section, taking the focus); Committee reviews (each a link to the
    review it opens, or the empty line). Loading keeps the trail and the header (round-3-check 4); the page error's h1 is
    "Programme trainee". An ended registrar (r6): subtitle "Programme ended <date>", the ended record in place of This
    period and EPAs, the standing as on the last day, the waiting requests still remindable (round-3-check).
  - The ended record extracted (review 8; round-3-check 2): `MyProgress.razor`'s private `EndedView`/`EndedItemCard`
    become NEW `Shared/Progress/EndedRecord.razor`, My progress drawing it word for word (byte-equal tests), the
    registrar page with `RegistrarWords`' staff words.
  - `EpaProgressTable` gains `EpaNamesAsText` (review 7). `TrajectoryWords.Summary` with a subject name outside a review
    window, if it does not already write "Nomsa Mahlangu · 1 rating in the 2026 academic year, …".
  - CSS (`/* Flow 06: the registrar page (T358) */`): `.registrar-stack` and its margin resets (round 3 item 30).
- Files owned: `src/Wombat.Web/Components/Pages/Programme/ProgrammeTrainees.razor`, `ProgrammeTraineeDetail.razor`;
  `Components/Shared/Progress/EndedRecord.razor` (new), `EpaProgressTable.razor`, `Pages/Portfolio/MyProgress.razor` (the
  extraction only), `Components/Shared/TrajectoryWords.cs` (the summary only, if needed); `app.css` (its section);
  tests NEW `tests/Wombat.Web.Tests/Programme/ProgrammeTraineesPageTests.cs`, `ProgrammeTraineeDetailTests.cs`,
  `Progress/MyProgressTests.cs` and `Progress/EpaProgressTableTests.cs` (their changed halves), `Charts/*` only where
  the summary changes.
- Review findings: 6 (the page), 7, 8, 24, 30, 31 (the Filed checkbox's label once), 33 (the lists). Rounds: Q1, Q2;
  C2, C8, C9; round-3-check 2 and 4.
- Tests: bUnit for every `R2-Trainees` state (typical, Short on PAED-002 with the focus on its heading, Nothing filed,
  no match, empty with no form, the Sub-speciality admin's subtitle, heavy with the pager, exempt, loading, load error)
  and every `R2-Registrar` state (typical at 3.56's figures; no rating with Send a reminder; the dialog, result and
  refusal in the section; with STARs; each section's own error and Try again; loading with trail and header; the page
  error; ended; not found); nothing changes until Show; the address carries the filters and reading it sets them; the
  standing's names move to the in-page charts; no pronoun on either page; My progress's ended view byte-equal.
- Consumes: A0's page shells and `ProgrammeReadAs`; A1's waiting read, `WaitingList`, `ReminderAction`,
  `ReminderWords`, `.filter-actions`, the List page pattern; A2's queries and words; flow 05's queries and components.
  Provides: the section ids, `EndedRecord`, `EpaProgressTable.EpaNamesAsText`.

**Lane D "waiting page"** (`t358-waiting-page`). Waiting for assessors.
- Scope:
  - `WaitingForAssessors.razor` (`/programme/waiting`; R2-Waiting w1–w15): header "Waiting for assessors" and the scope's
    subtitle (E4); the filter bar (Waiting: All, Overdue only; With: Anyone, then each row's nominee by name; Show; Clear
    filters), applied with Show, carried in the address; the heading counts the answer in flow 04's words ("3 waiting, 2
    overdue", "1 waiting, 1 overdue, with Mohammed Patel"), takes the focus after Show and a page turn; the rule line
    with T351's sentence; `DataTable` `Stack` with its caption, columns Activity (`ActivityLink`, "from <registrar>",
    two that read the same told apart by `ActivityRowNames.Waiting`, "(1 of 2)"), With, State (the badge and Overdue),
    Waiting (`.waited-cell`: "8 days" over "since … SAST", then `ReminderAction`); the result or refusal above the table
    in an `ActionResult` taking the focus, the list read again after a send (the row's button giving way to the line) and
    after "moved meanwhile" (the row gone, the heading recounted); no match, empty (no form), loading, load error;
    `PagerControls`, 20 a page.
- Files owned: `src/Wombat.Web/Components/Pages/Programme/WaitingForAssessors.razor`; tests NEW
  `tests/Wombat.Web.Tests/Programme/WaitingForAssessorsPageTests.cs`, `Accessibility/RowNamesTests.cs` (the page's rows).
- Review findings: 17's page moments, 25 (as used), 29, 31 ("Waiting" and Show apart), 33. Rounds: Q3, Q4; C3, C4, C11;
  E3, E4, E6.
- Tests: bUnit for every `R2-Waiting` state (typical at 3.30; dialog; in flight; result with the focus; another reader
  the same day with no button; the next day with the line and the button; each refusal with the row unchanged; moved
  meanwhile recounted; heavy with the deactivated row and the "(1 of 2)" pair; With filtered; no match; the Sub-speciality
  admin's; empty; loading; load error); nothing changes until Show; no Send a reminder for a role that may not remind.
- Consumes: A0's shell and `ProgrammeReadAs`; A1's query, words, `ReminderAction`, `.filter-actions`, the pattern.
  Provides: the page's ids (`waiting-h`).

### After wave 2 (the integrator, then its own lanes)
- Merge B, C and D into `t358` (B last: its owner rows and menus name the pages C and D fill); run the six suites; make
  `app.css`'s four flow-06 sections and DESIGN.md's flow-06 rules one account, with the banner naming flow 06.
- **Runbook-and-docs lane** (`t358-runbook`, as `t355-runbook`), from the built code:
  - Every Expect `R2-Steps` lists, in the act files: 2.32, 2.33, 2.35, 2.37, 2.38 (act 2); 3.30, 3.31 (the Do changes:
    Mr Smit sends the reminder there), 3.32 (three stub mails), 3.33 (its "Targets this period" Actual noted), 3.52,
    3.53, 3.54 (act 3); 4.3, 4.4 (act 4: the menus and cards only; their Decisions due clauses stay); A.2.8, A.5.10,
    A.5.11, A.5.12, A.7.5–A.7.8 (the appendix). New steps in play order, routes as their `@page`, no query string:
    3.55, 3.56, 3.57 after 3.54; A.5.14 (other institution and erased only; no earlier profile exists in the corpus,
    round-3-check 3), A.5.15 (refusals on a scratch database, devadmin then Mr Smit), A.5.16 (the no-match heading when
    nobody matches; singular and plural), A.7.9. Dated Actual lines are left as written.
  - `coverage.md`: the three pages in § Pages and § Journeys by role (the four roles' entries), § Reached only by
    address loses Entrustment decisions for the speciality admins and Decision panels for the Coordinator; the gaps
    closed (T290 at 2.37, T298 at 2.33 and 3.52, T280 and T325 at 3.30, B7, B8).
  - `states.md` § Home and the role dashboards: the Homes' new states; rows for Programme trainees', the registrar
    page's and Waiting for assessors' states, and the reminder's.
  - DESIGN.md's banner lists flow 06; `design/flows/06-programme-oversight.md` and any other flow brief that quotes a
    changed Expect (flow 08's Decision panels, flow 09's Entrustment decisions, flow 19's reminder mails) follow.
- **The build's review**: three read-only reviewers on Sonnet (`review-lanes.md`): code, data and authorization; the
  pages, the design system and accessibility; the runbook and the cast. The stopping line agreed first;
  `build-review.md`; one fix pass (`t358-fix`); then the squash onto master as one T358 commit, with the six suites'
  counts in its message. Not pushed.
- The integrator records in the squash which task symptoms the build closes (T280's stalled rows, T325's Coordinator
  dates, T290's external member, T298's ties, T328's rows on these Homes, B7, B8) for the replay (step 7) to confirm
  before any task is closed. Reassign is filed as its own task (Q4, C5) if it is not already.

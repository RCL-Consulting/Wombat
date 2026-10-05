# T358 step 6 — the contracts flow 06's lanes build on

Fixed before wave 0, so the lanes can run in parallel, as T355's were. Each wave builds these names exactly; a lane that
must differ says so in its report, and the integrator corrects this file to the code before the next wave starts (§ As
built). Namespaces are under `Wombat.Application.Features` unless given; Web paths under `src/Wombat.Web/Components/`.
Who provides each is in brackets: A0 "base", A1 "waiting", A2 "roster", B "homes", C "trainees", D "waiting page"
(`build-lanes.md` § The lane plan). Words are quoted as the boards give them for the cast (D = 2026-10-04).

## Decisions the design leaves to the code (the lanes build the recommendation unless the operator says otherwise)

- **D1. The admission day.** E5 starts Nothing filed's 30 days "at the later of admission and today − 30", and Step
  2.32's board is empty because the registrars "were admitted this week". No column holds an admission day
  (`TraineeProfile` has `ProgrammeStartDate`, 2026-01-15 for the whole cast, D42). **Recommended:** `TraineeProfile
  .AdmittedOn` (`DateOnly`, not null), written by `AdmitTrainee` and `DevUserSeeder` as `QuotaCalendar.Today(clock)`,
  backfilled to `ProgrammeStartDate` by the one migration. The alternative, reading admission as the programme's start,
  lists all five registrars at 2.32 and contradicts the board.
- **D2. The role a page reads as.** The pages admit four roles (three for Waiting for assessors) and scope by one (E4).
  **Recommended:** `ProgrammeReadAs.RoleFor`: the acting role when the page admits it, else the first admitted role
  held in `DashboardPriority.Order` (a Committee member acting as Assessor who types the address reads as Committee
  member). The subtitle always names the role read as.
- **D3. Filters in the address.** Programme trainees: `?short=<EpaId>&year=<n>&filed=true`; Waiting for assessors:
  `?show=overdue&with=<userId>`. Show calls `NavigationManager.NavigateTo` with the query; the page reads them with
  `[SupplyParameterFromQuery]` and reads again in `OnParametersSetAsync`; the form is a real `<form>` (`@onsubmit`) so
  Enter is Show. The page number is not in the address. Clear filters navigates to the bare route.
- **D4. Decision panels' owner row.** The Spec adds the Coordinator to "A decision panel", but `PanelEdit` does not admit
  him (Step 2.32: the panel form is "You cannot open this page") and an owner may be named only for a role the page
  admits (`ActiveNavItemTests`). **Recommended:** the row stays as built; the Coordinator reads panels on the list,
  which lights itself.
- **D5. Who the pages admit.** As drawn: Programme trainees and the registrar page `CommitteeMember, SpecialityAdmin,
  SubSpecialityAdmin, Coordinator`; Waiting for assessors `SpecialityAdmin, SubSpecialityAdmin, Coordinator`. Not the
  Administrator or the Institutional admin (no menu offers them; flow 12's and 18's to decide).
- **D6. The same-day race.** The command checks for today's reminder before staging; two senders in the same instant
  meet the unique index. **Recommended:** the handler catches a unique violation
  (`PostgresErrors.IsUniqueViolation`), detaches its staged row (`Set<ActivityReminder>().Entry(row).State =
  Detached`, so the audit write does not resend it), and returns `RemindedToday`. The mail is handed to `IEmailSender`
  only after the save.
- **D7. An activity the reader cannot see, or no such id.** `NotFound`, worded "Not sent. This request is no longer on
  your list." and the list read again; never the moved-meanwhile sentence, which would tell its state.
- **D8. The registrar page's charts.** Over the academic year containing today (`TrajectoryWindow.AcademicYearOf`), or
  the ended programme's last day; no "Today" rule (drawn without one, "as the review page draws them"); each chart an
  h3 under the section's h2, its card `id="trajectory-<EpaId>"`.
- **D9. A heading for several filters.** One filter: its own form ("5 of 5 current registrars are short on PAED-002",
  "1 of 5 current registrars has filed nothing in 30 days", "2 of 5 current registrars are in training year 4"). More
  than one: "2 of 5 current registrars match these filters", with what was asked in the rule line. No match: "0 of 5
  current registrars".
- **D10. The count badge's tone.** `DashboardCard` gains `BadgeTone` (default `BadgeState.Submitted`, as today):
  Waiting for assessors keeps Submitted; Registrars and Nothing filed pass `BadgeState.Draft`, as the board draws.
- **D11. Furthest short.** The registrar's in-force items whose window target applies and is not met, by shortfall
  (target − count) largest first, then semester before yearly, then code; three at most. Its line under the codes: "0
  of 3 each this semester" when the three share one figure and window, else each "PAED-008: 0 of 1 in 2026", joined by
  " · ". A registrar with every target met reads "Every target met".

## A0 "base"

- **`Programme.ProgrammeScope`** (static): `RosterRoles` = `[CommitteeMember, SpecialityAdmin, SubSpecialityAdmin,
  Coordinator]`, `WaitingRoles` = `[SpecialityAdmin, SubSpecialityAdmin, Coordinator]` (`IReadOnlyList<string>` of
  `WombatRoles` keys);
  - `ResolveAsync(IApplicationDbContext dbContext, ClaimsPrincipal principal, string actingRole, CancellationToken
    cancellationToken) : Task<ProgrammeScopeDto?>`: null as `build-lanes.md` § A0 says.
  - `Profiles(IApplicationDbContext dbContext, ProgrammeScopeDto scope) : IQueryable<TraineeProfile>`: at the scope's
    institution, curriculum sub-speciality in scope; every profile, ended included (callers keep the current).
  - `Activities(IQueryable<Activity> activities, ProgrammeScopeDto scope) : IQueryable<Activity>`: `InstitutionId` stamp
    equal, and the `SpecialityId` / `SubSpecialityId` stamp in scope for the two admins (a null stamp is nobody's).
- `Programme.ProgrammeScopeDto(string ActingRole, ProgrammeScopeKind Kind, string Name, int InstitutionId,
  IReadOnlyList<int> SpecialityIds, IReadOnlyList<int> SubSpecialityIds)`; `enum ProgrammeScopeKind { Institution,
  Speciality, SubSpeciality }`. `Name`: "Kgosi Kgari Teaching Hospital", or "Paediatrics" (names joined by ", " for
  several).
- **Web `Navigation.ProgrammeReadAs`**: `RoleFor(ActingRole acting, IReadOnlyList<string> admits) : string?` (D2).
- **`Domain.Activities.ActivityReminder`**: `int Id`, `int ActivityId`, `string SentByUserId`, `string AssessorUserId`,
  `DateTime SentOn` (UTC), `DateOnly SentOnDay` (`ProgrammeCalendar.DateOf(SentOn)`). No navigation property. Table
  `ActivityReminders`; ids `HasMaxLength(450)`; FK to `Activities` cascade; unique index `(ActivityId, SentOnDay)`; index
  `(ActivityId, SentOn)`.
- **`TraineeProfile.AdmittedOn`** (`DateOnly`, D1).
- Migration `<timestamp>_T358_ActivityRemindersAndAdmission` (+ `.Designer.cs`, snapshot).
- Page types (namespace `Wombat.Web.Components.Pages.Programme`): `ProgrammeTrainees` (`/programme/trainees`),
  `ProgrammeTraineeDetail` (`/programme/trainees/{ProfileId:int}`, `[Parameter] public int ProfileId`),
  `WaitingForAssessors` (`/programme/waiting`); the attributes of D5.

## Reads [A1]

- **`ActivitySummaryDto` gains** (init members, defaults for every other producer): `ActivityReminderDto? LastReminder`,
  `bool RemindedToday`, `ReminderOutcome? CannotRemind` (`Deactivated`, `NoEmail` or `NoAccount` for the nominee as read;
  null when a reminder may be sent). `Holder` (`Person`, its `Name` filled), `NomineeName`, `SubjectName`,
  `DisplayName`, `IsOverdue`, `WaitedDays` are filled as `WaitingForYou` fills them.
- `Programme.Waiting.ActivityReminderDto(DateTime SentOn, DateOnly SentOnDay, string SentByName)`.
- **The reader** `Programme.Waiting.WaitingForAssessorsReader.ReadAsync(IApplicationDbContext dbContext,
  IUserAdministrationService users, IReminderRecipients recipients, TimeProvider clock, DashboardThresholds thresholds,
  ClaimsPrincipal principal, ProgrammeScopeDto scope, WaitingForAssessorsFilter filter, int page, int pageSize,
  CancellationToken cancellationToken) : Task<WaitingForAssessorsDto>`.
  - `WaitingForAssessorsFilter(bool OverdueOnly = false, string? WithUserId = null, string? SubjectUserId = null)`.
  - `WaitingForAssessorsDto(ProgrammeScopeDto Scope, IReadOnlyList<ActivitySummaryDto> Items, int MatchCount, int
    MatchOverdueCount, int TotalCount, int TotalOverdueCount, IReadOnlyList<NomineeOptionDto> Nominees, int DueDays, int
    NudgeDays, int Page, int PageSize, bool MayRemind)`: `Items` the page of the match; `Total*` before any filter but
    the subject's; `Nominees` from every row, by surname; `MayRemind` when the scope's role is a `WaitingRoles` role.
  - `NomineeOptionDto(string UserId, string Name)`.
- **The query** `Programme.Waiting.ListWaitingForAssessorsQuery(ClaimsPrincipal Principal, string ActingRole, bool
  OverdueOnly = false, string? WithUserId = null, string? SubjectUserId = null, int Page = 1, int PageSize = 20) :
  IRequest<WaitingForAssessorsDto?>`: admits a `WaitingRoles` role; with `SubjectUserId` set, any `RosterRoles` role
  (the registrar page's section; a Committee member reads it with `MayRemind` false). Null when the scope is.
  `ListWaitingForAssessorsQueryHandler.HomeRows = 5` is what the Homes ask for.
- **The setting**: `Common.Options.DashboardThresholds.AssessorNudgeDays` (int, 5); `CoordinatorStallDays` is gone.
  `appsettings.json` `DashboardThresholds:AssessorNudgeDays`.

## The command [A1]

- `Programme.Commands.SendActivityReminder.SendActivityReminderCommand(ClaimsPrincipal Principal, string ActingRole, int
  ActivityId, string ExpectedState, DateTime ExpectedUpdatedOn) : IRequest<SendActivityReminderResult>` (the page
  passes the row's `CurrentState` and `UpdatedOn` as read).
- `SendActivityReminderResult(ReminderOutcome Outcome, string? AssessorName, string? ActivityName, string?
  SubjectName, int? WaitedDays, string? CurrentStateLabel, DateTime? MovedOn, ActivityReminderDto? Reminder)`.
- `enum ReminderOutcome { Sent, RemindedToday, Deactivated, NoEmail, NoAccount, MovedMeanwhile, NotFound }`. Moved
  meanwhile: no longer waiting, a different state or `UpdatedOn`, or another nominee (`CurrentStateLabel`, `MovedOn` set).
- **The port** `Common.Interfaces.IReminderRecipients`: `LoadAsync(IEnumerable<string> userIds, CancellationToken
  cancellationToken) : Task<IReadOnlyDictionary<string, ReminderRecipientDto>>` (an id naming no account is absent);
  `Common.Interfaces.ReminderRecipientDto(string UserId, string? Email, string FirstName, string LastName, bool
  IsDeactivated)`; `Programme.Waiting.ReminderRecipientRules.RefusalFor(ReminderRecipientDto? recipient) :
  ReminderOutcome?` (null: may be sent; NoAccount, Deactivated, NoEmail in that order; never the opt-out, E1).
  Infrastructure `Scheduling.ReminderRecipients : IReminderRecipients` (internal, scoped).
- **The mail** `Common.Email.Templates.AssessorPendingNudgeEmail.BuildReminder(string toEmail, string firstName, string
  activityTypeName, string traineeName, int daysWaiting) : EmailMessage`: subject "Activities awaiting your assessment",
  the nudge's body with one line "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days", tags `["reminder",
  "assessor-reminder"]`. `AssessorPendingNudgeEmail.WaitingPhrase(int days)` → "waiting less than a day", "waiting 1
  day", "waiting 8 days", used by both builds.
- `Common.Persistence.PostgresErrors.UniqueViolation` ("23505") and `IsUniqueViolation(Exception? exception)`.

## Words [A1]

- **`Shared/Activities/WaitingWords`** (namespace `Wombat.Web.Components.Shared.Activities`) gains:
  `With(ActivitySummaryDto item)` → "With Mohammed Patel"; `StaffRuleLine(int dueDays, int nudgeDays)` → "Oldest first.
  Overdue once it has waited 7 days. Its assessor is emailed after 5."; `PageRuleLine(dueDays, nudgeDays)` → the same
  then " Waiting counts from the last move: any save restarts it."; `StaffCount(WaitingForAssessorsDto)` → "3 waiting, 2
  overdue" (the match), with ", with Mohammed Patel" when filtered by With; `NoMatchHeading(dto)` → "0 of 2 waiting";
  `StaffMore(int beyond)` → "1 more waits in Waiting for assessors." / "9 more wait in Waiting for assessors.";
  `StaffSubtitle(ProgrammeScopeDto)` → "Requests at Kgosi Kgari Teaching Hospital whose next move names an assessor,
  supervisor or reviewer, read as Coordinator. Your own requests are not listed." ("in Paediatrics" for the admins);
  `NothingWaitingCard` → "Nothing is waiting for an assessor."; `NothingWaitingTitle` → "Nothing is waiting",
  `NothingWaitingBody(scope)` → "Nothing at Kgosi Kgari Teaching Hospital is waiting for an assessor, supervisor or
  reviewer."; `NoMatch` → "No request matches these filters."; `Asked(overdueOnly, withName)` → "Overdue only, with
  Fatima Khumalo."; `TableCaption(withName)` → "Requests waiting for a named assessor, oldest first" / "Requests waiting
  for Mohammed Patel, oldest first"; `PageLoading` → "Loading Waiting for assessors."; `PageLoadFailed` → "Could not load
  Waiting for assessors. Nothing has changed. Try again, or come back in a few minutes."; `OpenPage` → "Open Waiting for
  assessors". The filters' words: `ShowLabel` "Waiting", `ShowAll` "All", `ShowOverdue` "Overdue only", `WithLabel`
  "With", `WithAnyone` "Anyone".
- **`WaitingList`** gains `[Parameter] public bool WithNominee { get; set; }` ("With X" as its own `p.needs-you-why`
  above "Waiting 8 days") and `[Parameter] public RenderFragment<ActivitySummaryDto>? RowAction { get; set; }` (after the
  why-lines, inside the `li`).
- **`Shared/Programme/ReminderWords`** (namespace `Wombat.Web.Components.Shared.Programme`): `Button` "Send a reminder";
  `ButtonName(ActivitySummaryDto item, string? linkName)` → "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) ·
  PAED-004 · 2026-10-01, from Nomsa Mahlangu" (the link's own name when `ActivityRowNames.Waiting` gave one, "(1 of 2)"
  included); `DialogTitle(item)` → "Send Thandi Zulu a reminder?"; `DialogMail(item)` → "Thandi Zulu gets one email,
  "Activities awaiting your assessment", listing this request: Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8
  days."; `DialogMovesNothing(item)` → "It moves nothing: the request stays Requested, its wait is not restarted, and Nomsa
  Mahlangu is not told."; `Cancel` "Don't send"; `Confirm` "Send the reminder"; `Sending` "Sending…";
  `Result(SendActivityReminderResult)` → "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 ·
  2026-10-01, from Nomsa Mahlangu, waiting 8 days. The request is still Requested; its wait is unchanged." or "Not sent. "
  and: Deactivated "Fatima Khumalo's account is deactivated, so Wombat sends Fatima Khumalo no email. The request still
  waits."; NoEmail "Thandi Zulu has no email address in Wombat. Ask your institutional admin to add one."; NoAccount
  "Wombat has no account for the person this request names; it was erased or deleted, so there is nobody to email. The
  request still waits."; MovedMeanwhile "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01 moved at 2026-10-04 08:12 SAST:
  it is now Completed and waits for nobody." (D7's NotFound; RemindedToday "Pieter Smit reminded Thandi Zulu today
  already."); `IsRefusal(result)`; `Reminded(ActivityReminderDto)` → "Reminded 2026-10-04 by Pieter Smit";
  `CannotRemind(item)` → "No reminder: Fatima Khumalo's account is deactivated." / "No reminder: Thandi Zulu has no email
  address in Wombat." / "No reminder: the person this request names has no account.".
- **`Shared/Programme/ReminderAction.razor`**: `[Parameter, EditorRequired] ActivitySummaryDto Item`, `[Parameter,
  EditorRequired] string ActingRole`, `[Parameter] string? LinkName`, `[Parameter] EventCallback<SendActivityReminderResult>
  OnResult`. Renders `span` "Reminded …" (the last, if any), then `button.btn.btn-outline.btn-sm.reminder-action` named
  `ReminderWords.ButtonName` unless `RemindedToday` or `CannotRemind` (then the `CannotRemind` line); the dialog
  (`ConfirmDialog`, its safe button focused on open); while sending the confirm reads "Sending…" with `aria-disabled` and
  keeps the focus, Don't send disabled. It does not draw the result: the host does, in its `ActionResult`, and moves the
  focus there.

## Reads [A2]

- **The roster** `Programme.Trainees.ProgrammeRosterReader.ReadAsync(IApplicationDbContext dbContext,
  IUserAdministrationService users, ProgrammeScopeDto scope, ProgrammeTraineesFilter filter, DateOnly asOf,
  CancellationToken cancellationToken) : Task<ProgrammeRosterRead>` (every match, unpaged: Home takes five).
  - `ProgrammeTraineesFilter(int? ShortOnEpaId = null, int? TrainingYear = null, bool NothingFiled = false)`.
  - `ProgrammeRosterRead(ProgrammeScopeDto Scope, string CurrentSemesterName, DateOnly CurrentSemesterEnd, int
    CurrentCount, int ExemptCount, IReadOnlyList<ProgrammeTraineeRowDto> Rows, IReadOnlyList<EpaFilterOptionDto> Epas,
    IReadOnlyList<int> TrainingYears, EpaFilterOptionDto? ShortOn, CurriculumCoverage Coverage)`: `CurrentCount` every
    current registrar in scope (the "of 5"); `Rows` the match in its order; `Coverage` the scope's Targets by EPA.
  - `ProgrammeTraineeRowDto(int ProfileId, string TraineeUserId, string Name, int? TrainingYear, int SemesterMet, int
    SemesterApplying, int YearMet, int YearApplying, ProgrammeExemption? Exemption, IReadOnlyList<EpaShortfallDto>
    FurthestShort, EpaShortfallDto? ShortOn, DateOnly? LastFiledOn)`.
  - `EpaShortfallDto(int EpaId, string EpaCode, QuotaPeriod QuotaPeriod, int Count, int Target)`, `int Short`.
  - `EpaFilterOptionDto(int EpaId, string EpaCode, string EpaTitle, QuotaPeriod QuotaPeriod, int Target)`.
  - `enum ProgrammeExemption { StartedPartWay, NotStarted }`.
- **The query** `Programme.Trainees.ListProgrammeTraineesQuery(ClaimsPrincipal Principal, string ActingRole, int?
  ShortOnEpaId = null, int? TrainingYear = null, bool NothingFiled = false, int Page = 1, int PageSize = 20, DateOnly?
  AsOf = null) : IRequest<ProgrammeTraineesDto?>`; `ProgrammeTraineesDto(ProgrammeRosterRead Read,
  IReadOnlyList<ProgrammeTraineeRowDto> Page, int MatchCount, int PageNumber, int PageSize)`. Null when the scope is.
- **Filing** `Programme.Filing.FilingMoments.LastFiledAsync(IApplicationDbContext dbContext, IReadOnlyCollection<string>
  traineeUserIds, CancellationToken cancellationToken) : Task<IReadOnlyDictionary<string, DateTime>>` (UTC; absent: never
  filed); `FilingMoments.NothingFiled(DateOnly admittedOn, DateTime? lastFiledOn, DateOnly today) : bool` (admitted on or
  before today − 30, and no filing on or after today − 30 by the South African day); `FilingMoments.WindowDays = 30`.
- **Targets by EPA**: `Curricula.Quota.CurriculumCoverage.Epas` in the new order; `EpaTargetCoverage` gains `string?
  OwningInstitutionName { get; init; }`.
- **One registrar** `Programme.Trainees.GetProgrammeTraineeQuery(ClaimsPrincipal Principal, string ActingRole, int
  ProfileId, DateOnly? AsOf = null) : IRequest<ProgrammeTraineeDto?>`; `ProgrammeTraineeDto(int ProfileId, string
  TraineeUserId, string Name, int? TrainingYear, string InstitutionName, string SubSpecialityName, string
  CurrentSemesterName, ProgrammeEndDto? Ended, ProgrammeScopeDto Scope)`.
- **Reviews** `CommitteeDecisions.ListReviewsForProgrammeTraineeQuery(ClaimsPrincipal Principal, string TraineeUserId) :
  IRequest<IReadOnlyList<CommitteeReviewListItemDto>>`.

## Words [A2]

Static classes in `Shared/Programme/` (namespace `Wombat.Web.Components.Shared.Programme`), dates ISO.
- **`ProgrammeLinks`**: `Trainees` "/programme/trainees"; `Trainee(int profileId)` → "/programme/trainees/7"; `Waiting`
  "/programme/waiting"; `ShortOn(int epaId)` → "/programme/trainees?short=2"; `NothingFiled` →
  "/programme/trainees?filed=true"; the query names `ShortKey` "short", `YearKey` "year", `FiledKey` "filed", `ShowKey`
  "show", `ShowOverdueValue` "overdue", `WithKey` "with".
- **`ProgrammeWords`**: `Subtitle(ProgrammeScopeDto, string semester)` → "Current registrars at Kgosi Kgari Teaching
  Hospital, read as Committee member · Semester 2, 2026" / "Current registrars in Paediatrics, read as Sub-speciality
  admin · Semester 2, 2026"; `SemesterFigure(row)` → ("0 of 10", "EPAs met this semester"); `YearFigure(row)` → ("0 of
  5", "EPAs met in 2026"); `TrainingYear(row)` → "Training year 3"; `Exempt` "Exempt this period"; `ExemptReason
  (ProgrammeExemption)` → "Started part-way through the period" / "Starts on 2027-01-15"; `FurthestShortCodes(row)` →
  "PAED-001, PAED-002, PAED-003", `FurthestShortLine(row)` → "0 of 3 each this semester" (D11); `ShortOnCell(row)` →
  ("1 of 3 this semester", "2 more by 2026-11-30"); `LastFiled(row)` → "2026-09-12" / "Nothing filed yet";
  `RosterLine` "Fewest met first, then by surname."; `ListHeading(ProgrammeRosterRead read, ProgrammeTraineesFilter filter, int matchCount)` (D9) → "5 current
  registrars", "5 of 5 current registrars are short on PAED-002", "1 of 5 current registrars has filed nothing in 30
  days", "0 of 5 current registrars"; `ListRule(read, filter)` → "Fewest met first, then by surname. Semester 2, 2026
  ends on 2026-11-30." (exempt: "Fewest met first, then by surname. 1 registrar exempt this period, not counted, listed
  last."), Short on: "PAED-002 — Managing common paediatric presentations, 3 per semester. Furthest from its target first,
  then fewest EPAs met, then by surname.", Nothing filed: "Nothing filed (a draft is not filed) in the last 30 days, or
  since admission if that is later; a recorded MSF counts. Longest without first."; `Caption(filter, epa)` → "Current
  registrars, fewest EPAs met first" / "Current registrars short on PAED-002, furthest from its target first" / "Current
  registrars with nothing filed in 30 days"; `NoMatch` "No registrar matches these filters."; `Asked(filter, epa)` →
  "Short on PAED-001, training year 4, nothing filed in 30 days."; `EmptyTitle` "No current registrars", `EmptyBody` "They
  appear here once they are admitted to the programme."; `Loading` "Loading Programme trainees."; `LoadFailed` "Could not
  load Programme trainees. Nothing has changed. Try again, or come back in a few minutes."; the filter words
  `ShortOnLabel` "Short on", `AnyEpa` "Any EPA", `YearLabel` "Training year", `AnyYear` "Any", `FiledLabel` "Nothing filed
  in 30 days", `Show` "Show", `ClearFilters` "Clear filters"; Home: `Badge(int n)` → "5 registrars" / "1 registrar";
  `More(int beyond)` → "3 more in Programme trainees."; `RosterEmpty` "No current registrars. They appear here once they
  are admitted to the programme."; `OpenTrainees` "Open Programme trainees"; `FiledRule` "Current registrars with nothing
  filed (a draft is not filed) in the last 30 days, or since admission if that is later."; `FiledRowMeta(row)` → "Last
  filed 2026-09-12 · Training year 2"; `FiledEmpty` "Every current registrar has filed something in the last 30 days.";
  `OpenFiled` "Open in Programme trainees".
- **`TargetsByEpaWords`**: `Figure(EpaTargetCoverage)` → ("2 of 5", "registrars met this semester" / "registrars met in
  2026"), all exempt → ("All exempt", "this period"); `Rule(CurriculumCoverage)` → "Fewest registrars met first. Semester
  2, 2026 ends on 2026-11-30." with "1 registrar exempt this period, not counted. " before it (the one wording);
  `Cadence(epa)` → "3 per semester", "1 per academic year", " · Kgosi Kgari Teaching Hospital's own", " · every registrar
  has met it"; `IsMetByAll(epa)`; `LinkTail(epa)` → ": 2 of 5 registrars met this semester. Show the registrars short on
  it." (visually hidden); `Empty` "No targets this period: there is no current registrar.".
- **`RegistrarWords`**: `Subtitle(ProgrammeTraineeDto)` → "Training year 1 · Semester 2, 2026 · Kgosi Kgari Teaching
  Hospital, Paediatrics" / "Training year 2 · Programme ended 2026-10-02 · Kgosi Kgari Teaching Hospital, Paediatrics";
  `Tab(name)` → "Nomsa Mahlangu · Wombat"; `ErrorHeading` "Programme trainee"; `Loading` "Loading this registrar's
  progress."; `LoadFailed` "Could not load this registrar's progress. Nothing has changed. Try again, or come back in a
  few minutes."; the section headings `PeriodHeading` "This period", `EpasHeading` "EPAs", `StandingHeading` "Entrustment
  against Annexure A", `TrajectoriesHeading` "Rating trajectories", `WaitingHeading` "Waiting for assessors",
  `ReviewsHeading` "Committee reviews"; the section errors "Could not load this period. …", "Could not load the EPAs. …",
  "Could not load the standing. …", "Could not load the rating trajectories. …", "Could not load what waits for an
  assessor. …", "Could not load the committee reviews. …" (each ending "Nothing has changed. Try again, or come back in a
  few minutes."); `PeriodLine(summary)` → "Semester 2, 2026 ends on 2026-11-30. Training year 1 sets the minimum level
  each encounter is judged against."; `NoRatings(window)` → "No ratings yet in the 2026 academic year." / ended "No
  ratings in the 2026 academic year."; `NothingWaiting(name)` → "Nothing of Nomsa Mahlangu's waits for an assessor.";
  `NoReview(name)` → "No review is scheduled for Nomsa Mahlangu."; the ended record's staff words: `EndedNotice(name,
  ended, semester)` → "Pieter du Plessis's programme ended on 2026-10-02, part-way through Semester 2, 2026. No target
  applies after that; what follows is the record as it stood then, read-only.", `EndedWhen` "when the programme ended".

## The pages' fixed words and ids

- **Homes [B]:** cards, in order — Committee member: "Registrars" (`h2#card-registrars`, `users`, span 3), "Targets by
  EPA" (`h2#card-epa-targets`, `book`, span 3). Speciality and Sub-speciality admin: "Waiting for assessors"
  (`h2#card-waiting-assessors`, `clock`, span 3, the warning stripe when any is overdue, else emphasis), Registrars,
  Targets by EPA. Coordinator: Waiting for assessors (span 2), "Nothing filed in 30 days" (`h2#card-nothing-filed`,
  `clipboard-list`), "Invitations nearing expiry" (`h2#card-invitations`, `calendar`). Feet: "Open Waiting for
  assessors", "Open Programme trainees", "Open in Programme trainees"; none on Targets by EPA or Invitations. The
  Coordinator's header action `HomeAction("Start an MSF campaign", "/msf/campaigns/new", "plus")`. The invitation row
  "expiring@kgk.wombat.local · Trainee" and "expires 2026-10-07", its rule "Expiring in the next 3 days. Only an
  institutional admin can resend an invitation." (with rows only); `ExpiringInvitationItem` gains `string
  TargetRoleLabel`. A registrar's name links `ProgrammeLinks.Trainee`; an EPA's `ProgrammeLinks.ShortOn` with
  `TargetsByEpaWords.LinkTail` hidden after it.
- **`RegistrarRoster` [B]** (`Shared/Programme/RegistrarRoster.razor`): `[Parameter, EditorRequired]
  IReadOnlyList<ProgrammeTraineeRowDto> Rows`, `[Parameter] int Total`, `[Parameter] bool FiledMeta` (the Nothing filed
  card's rows: name and `FiledRowMeta`, no figures); `ul.list-unstyled` of `li.roster-row`, the name
  `a.progress-row-link` over `p.progress-row-meta`, each figure a `.dashboard-metric` of `.count-figure` and
  `.dashboard-metric-label`; exempt: `.roster-exempt` with the badge and the reason; then `p.waiting-more`.
- **The menus [B]:** `NavItems.ProgrammeTrainees` = `new("/programme/trainees", "Programme trainees", "users",
  typeof(ProgrammeTrainees))`; `NavItems.WaitingForAssessors` = `new("/programme/waiting", "Waiting for assessors",
  "clock", typeof(WaitingForAssessors))`; `NavItems.EntrustmentDecisions` = `new("/admin/entrustment-decisions",
  "Entrustment decisions", "award", typeof(EntrustmentDecisionsPage))`. ByRole: CommitteeMember [ProgrammeTrainees,
  CommitteeReviews, DecisionPanels]; SpecialityAdmin and SubSpecialityAdmin [ProgrammeTrainees, WaitingForAssessors,
  DecisionsDue, CommitteeReviews, DecisionPanels, EntrustmentDecisions]; Coordinator [ProgrammeTrainees,
  WaitingForAssessors, DecisionsDue, MsfCampaigns, CommitteeReviews, DecisionPanels, DataRightsRequests].
  `NavItems.FlatLimit` (8) counts Home and the role's links; `For` leaves the personal links out of the count (E2).
- **Owners [B]:** `[typeof(ProgrammeTraineeDetail)] = [new([.. ProgrammeScope.RosterRoles], NavItems.ProgrammeTrainees)]`;
  `[typeof(ActivityView)]` adds `new([SpecialityAdmin, SubSpecialityAdmin, Coordinator], NavItems.WaitingForAssessors)`
  after the Assessor's and the Trainee's rows; Entrustment decisions' `[]` row goes; `PanelEdit` unchanged (D4).
- **Programme trainees [C]:** title and h1 "Programme trainees"; the form `form.search-container` (`aria-label="Filter
  Programme trainees"`) with `#f-epa`, `#f-year`, `#f-filed`, then `div.search-field.filter-actions` (Show
  `btn-primary`, Clear filters `btn-outline` once a filter is set); `section.list-section` with `h2#trainees-h.list-section-title`
  (`tabindex="-1"`, the heading words); `p.needs-you-rule`; the table `clinic-table--stack clinic-table--index`, columns
  "Registrar", "Training year", "This semester", "In 2026", "Furthest short", "Last filed" (Short on: "Registrar",
  "Training year", "<code>", "This semester", "In 2026", "Last filed"; Nothing filed: "Registrar", "Training year", "Last
  filed", "This semester", "In 2026"); figure cells `.count-figure` over `.count-meta`; `PagerControls` labelled "Current
  registrars, pages"; the status `p.visually-hidden[role=status]` "Loading Programme trainees." always present.
- **The registrar page [C]:** h1 the name; `div.registrar-stack` of sections `h2#period-h`, `#epas-h`, `#standing-h`,
  `#trajectories-h`, `#waiting-h`, `#reviews-h`, each `h2.epa-section-title` (`tabindex="-1"` on those a Try again
  answers into); the charts `TrajectoryChart` with `HeadingLevel="3"` in cards `id="trajectory-<EpaId>"`;
  `EntrustmentStandingPanel` `EpaHref = epa => charted.Contains(epa.EpaId) ? $"#trajectory-{epa.EpaId}" : null`;
  `EpaProgressTable` `EpaNamesAsText="true"`; not found is flow 01's "Page not found" / "There is no page at this
  address." / "Check the address, or start again from Home." / Go to Home.
- **`EndedRecord` [C]** (`Shared/Progress/EndedRecord.razor`): `[Parameter, EditorRequired]
  TraineeCurriculumProgressSummaryDto Summary`, `[Parameter, EditorRequired] ProgrammeEndDto Ended`, `[Parameter]
  MsfCoverageDto? MsfCoverage`, `[Parameter] bool MsfFailed`, `[Parameter] string? SubjectName` (null: My progress's
  own words, byte-equal; set: `RegistrarWords`' staff words and no EPA-page links). `EpaProgressTable` gains `[Parameter]
  public bool EpaNamesAsText { get; set; }`.
- **Waiting for assessors [D]:** title and h1 "Waiting for assessors"; the form (`aria-label="Filter Waiting for
  assessors"`) with `#f-show` ("Waiting") and `#f-with` ("With"), then `.filter-actions`; `section.list-section` with
  `h2#waiting-h.list-section-title` (`tabindex="-1"`); `p.needs-you-rule` (`WaitingWords.PageRuleLine`); `DataTable`
  `Stack`, columns "Activity", "With" (`data-label`), "State" (`data-label`), "Waiting" (`td.waited-cell`, `b` the wait,
  `span` the since, then `ReminderAction`); the result `ActionResult` above the table; `PagerControls` labelled "Waiting,
  pages"; the status "Loading Waiting for assessors." always present.

## CSS, by owner

Each lane defines its own classes under its own `/* Flow 06: … (T358) */` heading; a class another lane uses arrives
with the merge. Built classes reused, never redefined (Spec, round 3 item 28): `.list-unstyled`, `.progress-row-link`,
`.progress-row-meta`, `.needs-you-rule`, `.needs-you`, `.needs-you-row`, `.needs-you-row--overdue`, `.needs-you-why`,
`.needs-you-badges`, `.dashboard-metric`, `.dashboard-metric-label`, `.count-figure`, `.count-meta`, `.waited-cell`,
`.waiting-more`, `.card-empty`, `.section-error`, `.dashboard-card-skeleton`, `.clinic-table--stack`,
`.clinic-table--index`, `.list-section`, `.list-section-title`, `.search-container`, `.search-grid`, `.search-field`,
`.period-card`, `.period-ends`, `.epa-section-title`, `.index-section`, `.standing-panel`, `.trajectory-*`.
- [A1] `.reminder-action`; `.filter-actions`; below 641 px `.reminder-action` and `.filter-actions .btn` 2.75rem and
  full width.
- [B] `.roster-row`, `.roster-row > .roster-exempt`, `.coverage-row`; at `max-width: 900px` both grids one column.
- [C] `.registrar-stack`, and `.registrar-stack > .index-section`, `> .standing-panel`, `> .list-section` margin 0.
- [A0], [A2], [D]: none.

Never shipped: the canvas's `.r2-*`, and every class round 3 dropped (`.waiting-with`, `.with-cell-note`, `.card-scope`,
`.figure-cell`, `.short-cell`, `.roster-figure*`, `.coverage-epa`/`-cadence`/`-figure`/`-value`/`-label`,
`.roster-list`, `.coverage-list`, `.review-links`, `.filter-check`).

## As built after wave 0

(The integrator fills this in after merging A0, from the code, before wave 1 starts.)

## As built after wave 1

(The integrator fills this in after merging A1 and A2, from the code, before wave 2 starts.)

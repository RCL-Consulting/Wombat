# T355 step 6 — the contracts flow 05's lanes build on

Fixed before wave 1, so the lanes can run in parallel, as T350's were. Wave 1 builds these names exactly; a lane that must
differ says so in its report, and the integrator corrects this file to the code before wave 2 starts (§ As built after
wave 1). Namespaces are under `Wombat.Application.Features` unless given; Web paths under `src/Wombat.Web/Components/`.
Who provides each is in brackets: A1 "counts", A2 "decisions", B "home", C "progress", D "epa" (`build-lanes.md` § The
lane plan). Words are quoted as the boards give them for the cast (D = 2026-10-03).

## Two decisions the design leaves to the code (the operator's; the lanes build the recommendation unless told otherwise)

- **D1. One date form, and `QuotaText`'s long date.** The boards write every date ISO (T325): "2 more by 2026-11-30",
  "Your programme ended on 2026-10-01", "Issued 2026-10-03", MSF's "closed on 2026-10-03" (C4), the part-way Alert's
  "2026-08-18" (C2). The built words take their dates from `QuotaText.LongDate` ("30 November 2026") and `ShortDate`
  ("3 Oct 2026"), which the portfolio PDF (`EpaProgressSectionComponent`) and the coordinator's MSF coverage text
  (`MsfProgrammeCoverageText`) share. **Recommended:** `QuotaText` gains `Iso(DateOnly)` ("yyyy-MM-dd") and
  `ProgrammeEnded` reads ISO (its callers are all Web: Home, My progress, the panel); every flow-05 surface and
  `MsfCoverageText` (the EPA page, This period, the committee's MSF section) write ISO; the PDF and the coordinator's text
  keep `LongDate` (outside the fence; a printed record, flow 20's). So a screen never shows two forms, and the ended view's
  notice (flow 13's page) reads ISO too. The alternative, `LongDate` itself becoming ISO, changes the PDF as well.
- **D2. The committee page's chart window.** `ReviewDetail` reads every rating of the trainee today (no From/To). The
  design draws the committee's chart over "the review window, 2026-01-01 to 2026-12-31", with no "Today" (R3-T-committee;
  4.15's new Expect: "3 ratings in the review window"). **Recommended, as drawn:** the committee page passes the review
  period's From/To to `GetEpaTrajectoryForTraineeQuery`, so a rating outside the period is not charted there (it stays on
  the review's evidence lists, which are unchanged). Flow 07 places the chart; flow 05 only gives it its window.

## Reads [A1]

- **The counts' new fields** (C10). `Curricula.Quota.TraineeCurriculumProgressDto` gains init members, left at their
  defaults by every other producer:
  - `QuotaPeriod? DecisionCadence` (`CurriculumItem.DecisionCadence`; null: no cadence, KGK-001) and
    `bool DecisionIsOpportunistic` ("Decided as opportunity allows").
  - `string? OwningInstitutionName`: the institution whose own item it is, null for the College's; with
    `bool IsLocal => OwningInstitutionName is not null`.
  - `int? ExitLevelOrder`, `string? ExitLevelLabel`: the EPA's exit level as the standing reads it.
  - `bool EpaInForce` (default `true`).
- **The paused group** (Spec § 6). `TraineeCurriculumProgressSummaryDto` gains
  `IReadOnlyList<PausedItemDto> Paused { get; init; } = []`, with
  `Curricula.Quota.PausedItemDto(int CurriculumItemId, int EpaId, string EpaCode, string EpaTitle, QuotaPeriod
  QuotaPeriod)`: the profile's items (national, or its own institution's) whose EPA is not in force, by code. Counted
  nowhere: `SemesterTargetsApplying`, `Items` and every figure stay in-force only.
- **One EPA** (round 1, correction 2). `Curricula.GetEpaProgressForTrainee.GetEpaProgressForTraineeQuery(ClaimsPrincipal
  Principal, int EpaId, DateOnly? AsOf = null) : IRequest<EpaProgressDto?>`, for the caller's own record (their
  `NameIdentifier`, their preferred profile, `TraineeScopeResolver.PreferredProfiles`).
  - `Curricula.GetEpaProgressForTrainee.EpaProgressDto(TraineeCurriculumProgressDto Item, DateOnly AsOf, DateOnly
    ProgrammeStartDate, int? TraineeStage, bool IsAfterTeachingYear, ProgrammeEndDto? Ended)`. `Item` is the
    item for that EPA, read by the same code as My progress's rows (so the two cannot disagree), even when paused
    (`Item.EpaInForce` false); with `Item.Periods` filled when the programme has ended (every period, read-only, R5).
  - Null when the caller has no profile, or the EPA is no item of their profile's curriculum (another institution's
    local item included), or the id is unknown: the page says "Page not found".
- **The trajectory** (C10; E2). `Activities.Queries.GetEpaTrajectoryForTrainee`:
  - The query gains `int? EpaId = null` after `To`: only that EPA's ratings.
  - `EpaTrajectoryDto` gains init members: `int? ExitLevelOrder`, `string? ExitLevelLabel`,
    `IReadOnlyList<TrajectoryMinimumStepDto> MinimumSteps` (default `[]`), `DateOnly? WindowFrom`, `DateOnly? WindowTo`
    (the window read, the request's From/To).
  - `TrajectoryMinimumStepDto(DateOnly From, int? TrainingYear, int MinimumOrder, string MinimumLabel)`: the item's
    minimum from that day, stepping where `TraineeProfile.StageOn` changes inside the window (14 January for the cast).
  - `TrajectoryPointDto` gains init members: `string AssessorName` (as stored), `string ActivityName` (the row's name,
    "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-21", the assessor added where two share it, E7 over the subject's whole
    list), `int? TrainingYear` (at the encounter), `string? MinimumLabel` (the minimum then), `TrajectoryAgainstMinimum
    AgainstMinimum`, `string? OtherScaleName` (the point's own ladder when `OffLadder`, C11's slot).
  - `enum TrajectoryAgainstMinimum { AtOrAbove, Below, NotComparable, NotGated }`, computed live with
    `EntrustmentLevelComparer.Compare(rating, ratedScaleId, item.GetMinimumLevelForStage(StageOn(start, observedOn)),
    item.ScaleId)` (E2): `ScaleMismatch` is NotComparable; an item with no minimum is NotGated.
- **The window.** `Curricula.Quota.TrajectoryWindow.AcademicYearOf(DateOnly day) : (DateOnly From, DateOnly To)`:
  1 January to 31 December of `day`'s academic year (December counts into it, D40). The EPA page passes today's, or the
  ended programme's last day's.
- **Dates (D1).** `Curricula.Quota.QuotaText.Iso(DateOnly date) : string` ("2026-11-30"); `QuotaText.ProgrammeEnded`
  reads "You completed your programme on 2026-06-30" / "Your programme ended on 2026-10-01". `LongDate` and `ShortDate`
  are unchanged for their other callers.

## Reads [A2]

- **Recent decisions** (B1; E6; note 1). `Activities.Services.DecidedOnYours.ReadAsync(IApplicationDbContext dbContext,
  IUserAdministrationService users, ClaimsPrincipal principal, int take, DateOnly today, CancellationToken
  cancellationToken) : Task<IReadOnlyList<ActivitySummaryDto>>`.
  - An item is an activity whose subject is the caller, whose latest transition (by `OccurredOn`, ties to the later id)
    was made by someone else, and that is finished in its pinned workflow or has no move left; never one whose latest
    transition is its create, never a `SystemManaged` type's; newest first by `DecidedOn`, then `Id`; the first `take`.
  - Each `ActivitySummaryDto` as flow 04's decided rows fill it (`CurrentState`, `CurrentStateLabel`, `IsFinished`,
    `DecidedOn`, `CreditedItemCount` by T108, `DisplayName` and the nominee line by E7 over the subject's whole list,
    `EpaId`, `EpaCode`, `EpaInForce`), plus `CountLine`.
- **The count a decision made** (C5; E5).
  - `Curricula.Quota.EpaCountLineDto(int EpaId, string EpaCode, QuotaPeriod QuotaPeriod, QuotaWindowDto Window, bool
    IsCurrentWindow)`: the trainee's in-force item for that EPA, its window containing the encounter date, tallied as on
    today; `IsCurrentWindow` when that window contains today.
  - `Curricula.Quota.EpaCountLines.ReadAsync(IApplicationDbContext dbContext, string traineeUserId,
    IReadOnlyCollection<(int EpaId, DateOnly ObservedOn)> keys, DateOnly today, CancellationToken cancellationToken) :
    Task<IReadOnlyDictionary<(int EpaId, DateOnly ObservedOn), EpaCountLineDto>>`: no entry for an EPA that is no
    in-force item of the trainee's curriculum.
  - `ActivitySummaryDto` gains `EpaCountLineDto? CountLine { get; init; }`, set by `DecidedOnYours` only.
  - `Activities.Queries.GetActivityCountLine.GetActivityCountLineQuery(ClaimsPrincipal Principal, int ActivityId) :
    IRequest<EpaCountLineDto?>`: null unless the caller is the activity's subject and its EPA has an entry.
- **The standing** (note 3). `EntrustmentDecisions.EntrustmentStandingReader.ReadAsync(IApplicationDbContext dbContext,
  string traineeUserId, DateOnly asOf, bool withLatestRatings, CancellationToken cancellationToken) :
  Task<EntrustmentStandingDto?>`, the handler's body after its scope check; `withLatestRatings: false` leaves every
  `EpaStandingDto.LatestRating` null and reads no activity.
- **Home** (R1; Q2; Q3). `Dashboards.Trainee.GetTraineeDashboardSummaryQuery(ClaimsPrincipal Principal, DateOnly? AsOf
  = null)` returns `TraineeDashboardSummaryDto(TraineeCurriculumProgressSummaryDto? CurriculumTargets,
  IReadOnlyList<ActivitySummaryDto> NeedsYou, IReadOnlyList<ActivitySummaryDto> RecentDecisions, EntrustmentStandingDto?
  Standing, bool IsPendingTrainee)`. `RecentDecisions` is `DecidedOnYours`' first five
  (`GetTraineeDashboardSummaryQueryHandler.RecentDecisionsListed = 5`); `Standing` is the reader in summary mode, as on
  `AsOf` (or the programme's last day once ended), null for a pending trainee or no profile. `RecentActivityItem`,
  `UpcomingDeadlineItem` and their two members are gone. "Today" from `TimeProvider` (T325).
- **The activities on one EPA** (C10). `ListActivitiesBySubjectQuery` gains `int? EpaId { get; init; }`: with it, only the
  subject's activities stamped with that EPA and finished in their pinned workflow (a Declined or cancelled request is
  never listed; MSF rows are), newest encounter first, paged and named as without it (E7 over the whole list). The EPA
  page asks for page 1 at `MaxPageSize` and draws no pager.

## Words [A1]

Static classes in `Shared/Progress/` (namespace `Wombat.Web.Components.Shared.Progress`) unless given. Every built
sentence the boards keep is moved word for word (its test asserts it byte-equal to today's), dates ISO (D1).
- **`ProgressLinks`**: `Epa(int epaId)` → "/portfolio/progress/2"; `OpenAt(string epaCode)` → "Open My progress at
  PAED-001"; `CreditTo(string label, string epaCode)` → "1 item to PAED-001, in My progress".
- **`ProgressWords`**:
  - `Figure(TraineeCurriculumProgressDto item)` → "1 of 3 this semester", "0 of 1 in 2026"; `WindowWords(item)` → "this
    semester", "in 2026".
  - `CountMeta(item, DateOnly asOf)` → "2 more by 2026-11-30", "Target met for Semester 2, 2026.", "Target met for the
    2026 academic year.", December "3 more; encounters in December still count towards Semester 2, 2026." / "1 more;
    encounters in December still count towards the 2026 academic year."; waived (one line, no bar) "No target this
    semester · 2 recorded · targets start with semester 1, 2027", "No target in 2026 · 0 recorded · targets start with
    the 2027 academic year"; not started "No target yet · targets start with semester 1, 2027"; the ended forms as built.
    `HasBar(item)`: false for every waived, not-started and ended cell.
  - `ShortRow(item, asOf)` (Home): "0 of 3 this semester · 3 more by 2026-11-30"; December "0 of 3 this semester · 3
    more; encounters in December still count towards Semester 2, 2026." (C3).
  - `AtMinimum(QuotaWindowDto window)` → "At the minimum level when observed: 1 of 1" (null when Count is 0);
    `LastEncounter(window)` → "Last encounter 2026-09-23" (`EncounterDate.Label`'s mark when undeclared; null when none).
  - `PreviousLine(QuotaWindowDto)` → "Semester 1, 2026: 0 of 3, 3 short", "2025 academic year: 0 of 1, 1 short" (as
    built, O2); `EndedPeriodLine(QuotaWindowDto, DateOnly today)` as built.
  - This period: `SemesterFigure(summary)` → ("1 of 10", "EPAs met this semester"), `YearFigure(summary)` → ("0 of 5",
    "EPAs met in 2026"); `TargetsLine(met, applying, when)` as built ("none apply yet"); `EndsLine(summary)` →
    "Semester 2, 2026 ends on 2026-11-30.", December "Semester 2, 2026 counts encounters observed in December.";
    `TrainingYearLine(int stage)` → "Training year 4 — it sets the minimum level each encounter is judged against."
    (Home and This period, T306); `Subtitle(summary)` → "Training year 4 · Semester 2, 2026".
  - Alerts: `DecemberNotice(summary)`, `StartNotice(summary)` (part-way and not started, C2, word for word from
    `MyProgress.razor:94-129` with ISO dates; null when neither applies).
  - `GroupCaption(QuotaPeriod kind, int count)` → "Each semester · 10 EPAs", "Once a year · 7 EPAs";
    `PausedCaption(int count)` → "No longer in use · 1 EPA"; `WindowHeading(item)` → "Semester 2, 2026", "2026 academic
    year".
  - `Cadence(item)` → "Decided each semester", "Decided once a year", "Decided as opportunity allows", or null (notes 8,
    9); `NoCadence` → "No decision cadence" (visually hidden beside the dash).
  - `PausedRow` → "Paused by the College. It is not a target while it is paused. Its ratings and the credit it had earned
    are kept, and what is completed on it meanwhile is credited if it is restored." (N6).
  - `LocalBadge(item)` → "Kgosi Kgari Teaching Hospital's own".
- **`EpaPageWords`**: `Title(code, title)` → "PAED-012 — Communicating with …"; `Tab(code, title, inForce)` → the h1's
  words with " (no longer in use)" when paused, then " · Wombat" (C5); `LevelLine(item, stage)` → "Training year 4: level
  5, the minimum each encounter is judged against and your STAR's target."; KGK-001 (no per-year map) "Minimum 3a";
  `ExitLine(EpaStandingDto)` → "Exit level 5 · reached" / "· not yet"; `Issued(StandingDecisionDto)` → "Issued
  2026-10-03" (and ", expires 2026-10-23" when it does); `PausedEntrustment(code)` → "While PAED-012 is paused it is not
  in your standing against Annexure A."; `NoStar` → "No STAR yet."; `NoActivity` → "No activity on this EPA yet.";
  `Loading` → "Loading this EPA", `LoadingStatus` → "Loading this EPA.", `ErrorHeading` → "EPA", `LoadError` → "Could not
  load this EPA. Nothing has changed. Try again, or come back in a few minutes."
- **`Shared/TrajectoryWords`** (namespace `Wombat.Web.Components.Shared`): `Summary(EpaTrajectoryDto, bool reviewWindow,
  string? subjectName)` → "3 ratings in the 2026 academic year, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at
  the minimum, 1 below.", "2 ratings in the 2026 academic year, from Sarah Botha and Mohammed Patel. 1 on another scale, 1
  below.", "1 rating so far, from David Naidoo. At the minimum.", the committee's "Lerato Molefe · 3 ratings in the review
  window, 2026-01-01 to 2026-12-31, from …" (C11; more than three assessors are counted, never "distinct assessors");
  `NotPlotted` → "Multi-source feedback is not plotted."; `RegionName(code)` → "Rating chart for PAED-001"; `SvgName(n)` →
  "Chart of the 3 ratings in the table below."; `Key` → "How to read the chart" and its four lines (Spec § 1);
  `NoRating` → "No rating yet."; `AgainstMinimum(TrajectoryPointDto)` → "At or above (5, training year 4)" (the board's
  "At it (…)", widened by note 14 to the panel's "at or above", since it also covers a rating above), "Below (5, training
  year 4)", "Not on the ladder: counts towards the number, not the level" (another scale), "No minimum" (not gated);
  `RatingCell(point)` → "4", or for another scale the rating's label on its own ladder, "Independent on O-R Scale"
  (C11's `[rating] on [scale]` slot, filled from the activity, `round-3-check.md`).
- **`EpaLabel`**: unchanged words; the marker's span is `span.paused-mark` (it was `span.muted`).

## Words [A2]

- **`Shared/Progress/CountLineWords`** (E5, E6; note 2):
  - `Line(EpaCountLineDto)` → current "PAED-001: 1 of 3 this semester." / met "PAED-001: 3 of 3 this semester, met."; an
    older window "PAED-001, Semester 1, 2026: 3 of 3, met." / "…: 1 of 3, 2 short."; yearly "PAED-008: 1 of 1 in 2026,
    met."; waived "PAED-002: no target this semester · 2 recorded.".
  - `ForDecision(ActivitySummaryDto item)` (Home's row): the paused sentence "Its credit to PAED-012 waits while the EPA
    is paused." when `EpaInForce` is false **and** it credited nothing (note 2); "Credits nothing." when it credited
    nothing; else `Line(CountLine)`; null for a decline (it has its link) and for no EPA.
  - `ForCard(EpaCountLineDto?)` (the completed card, appended after the built credit sentence): `Line`, or null.
- **`Pages/Dashboards/TraineeHomeWords`** (namespace `Wombat.Web.Components.Pages.Dashboards`): `AllMet(summary)` → "Every
  target is met for Semester 2, 2026 and the 2026 academic year." (one kind only: "… for Semester 2, 2026.", note 15);
  `PartWay(summary)` → "No semester target this semester: you started part-way through it. Semester targets begin with
  Semester 1, 2027."; `NotStarted(summary)` → "Your programme starts on 2027-01-15."; `Ended(ProgrammeEndDto)` → "Your
  programme ended on 2026-10-01, so no target applies to you any more. Your progress in each period is kept on My
  progress, read-only."; `NoCurriculum`, `NoEpaInUse`, `NoDecisions` ("No decisions yet."); `NoStar(int? stage, bool
  ended)` → "No STAR yet. When the committee issues one, it shows here against training year 3's level." / ended "No STAR
  yet."; `StarRow(EpaStandingDto, int targetYear)` → "4, below training year 4's level of 5 · expires 2026-10-23";
  `StarRows(EntrustmentStandingDto, DateOnly today)` → the rows below their level or expiring within 30 days, by code;
  `FileAgainName(ActivitySummaryDto)` → "File it again, to someone else: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13".
- **`Shared/StandingWords`**: `YearLine(EntrustmentStandingDto)` → "2 at or above · 1 below · 12 with no decision, of 15
  EPAs" (byte-equal to the panel's private `YearLine` today).

## The pages' fixed words and ids

- **Home [B]:** card titles "Your targets" (`h2#card-targets`, icon `target`), "Needs you" (as built), "Recent decisions"
  (`h2#card-decisions`), "My authorisations" (`h2#card-stars`, icon `award`); "Furthest short" (`h3.progress-group-label`)
  over `ul` of `li.progress-row.progress-row--link`, each `a.progress-row-link` to `ProgressLinks.Epa`, named "Code —
  Title", the bar's `aria-label` "PAED-002: 0 of 3 this semester"; feet "Open My progress" (`/portfolio/progress`) and
  "Open My authorisations" (`/portfolio/authorisations`); Recent decisions has no foot. Loading: `DashboardFrame`'s
  "Loading your Home." as built.
- **The ways in [B]:** the completed card's body is the built sentences then `CountLineWords.ForCard` ("Rated 4. Credited
  1 item to PAED-001. PAED-001: 1 of 3 this semester."); `ActivityStatusView` gains `string? ActionAriaLabel`; "Open My
  progress" `href` is `ProgressLinks.Epa(line.EpaId)` with `aria-label` `ProgressLinks.OpenAt(code)` when a line came
  back, else `/portfolio/progress` and no `aria-label`. My activities' Credit cell: `a.credit-link` "1 item" with
  `aria-label` `ProgressLinks.CreditTo`, when `CreditedItemCount > 0` and `EpaId` is set.
- **My progress [C]:** title "My progress", subtitle `ProgressWords.Subtitle`; the status "Loading My progress." (always
  present, `p.visually-hidden[role=status]`); load error "Could not load your progress. Nothing has changed. Try again,
  or come back in a few minutes."; `section.period-card` with `h2#period-h` "This period" (`tabindex="-1"`);
  `section.index-section` with `h2#index-h` "Your EPAs"; columns EPA, the window, "Committee decides", "STAR against
  training year 4" ("Not loaded" when the standing failed); the paused table's columns EPA and "Why it is not counted";
  `section.standing-panel` with `h2#standing-h` "Entrustment against Annexure A" (`tabindex="-1"`), its error "Could not
  load your standing. Nothing has changed. Try again, or come back in a few minutes."; MSF's error line
  "Multi-source feedback: Could not load MSF coverage. Your counts above are not affected." (`p.section-error`).
- **The standing panel [C]:** gains `[Parameter] public Func<EpaStandingDto, string?>? EpaHref { get; set; }` (null: the
  EPA's name is text); the EPA cell is `th scope="row"` with `a.epa-link`; the rating link `a.standing-rating-link`
  "4 · Encounter 2026-09-24" with `span.standing-rating-meta` "Direct observation; below the target" under it; every
  other cell labelled by its column below 641 px (`DataTable` `Stack`). My progress passes
  `epa => ProgressLinks.Epa(epa.EpaId)`; the committee page `epa => $"#trajectory-{epa.EpaId}"` (wired by the integrator
  after wave 2).
- **The EPA page [D]:** route `/portfolio/progress/{EpaId:int}`, page type `Pages.Portfolio.EpaProgress`; sections
  `h2#obs-h` "Observations", `h2#ent-h` "Entrustment", "Rating trajectory" (the chart's own heading, its paused mark in
  `span.paused-mark`), `h2#acts-h` "Activities on this EPA"; "Open My authorisations" `a.epa-auth-link` only for a
  holder of the Trainee role; not found is flow 01's "Page not found" / "There is no page at this address." / "Check
  the address, or start again from Home." / Go to Home. Owner: `NavOwners.OwnerFor(typeof(EpaProgress), any role or
  null)` is `NavItems.MyProgress`; the trail Home › My progress › <code>.
- **The chart [D]:** `div.trajectory-figure` (`role="region"`, `aria-label` `TrajectoryWords.RegionName`,
  `tabindex="0"`) holding `svg.trajectory-chart.trajectory-chart--sized.trajectory-chart--wide` (900) and
  `…--narrow` (326); its card `id="trajectory-<EpaId>"`; the table `table.clinic-table.trajectory-table` (Stack) with
  Encounter as `th scope="row"`.

## CSS, by owner

Each lane defines its own classes under its own `/* Flow 05: … (T355) */` heading; a class another lane uses arrives with
the merge. Built classes reused, not redefined: `.decided-list`, `.decided-row`, `.decided-meta`, `.decided-date`,
`.card-empty` (flow 04), `.progress-row`, `.progress-row-meta`, `.progress-bar`, `.dashboard-metric-row`,
`.details-list`, `.details-grid`, `.clinic-table--stack`, `.list-section`, `.activity-link`, `.activity-link-to`.
- [A1] `.paused-mark`.
- [A2] `.progress-row-link`, `.epa-link`, `.standing-rating-link`, `.activity-block-link`, `.credit-link`,
  `.epa-auth-link`, `.decided-note` (and `.decided-note a`), the 44 px block-link rule below 641 px over those and
  `.decided-row .activity-link`; `.detail-card--empty .btn` at 2.75rem below 641 px.
- [B] `.progress-row--link`, `.progress-group-label`, `.targets-line`, `.targets-note`, `.stars-summary`.
- [C] `.period-card`, `.period-ends`, `.period-lines`, `.period-line-term`, `.section-error`, `.index-section`,
  `.clinic-table .index-caption`, `.clinic-table--index`, `.count-cell`, `.count-figure`, `.count-meta`,
  `.verdict-cell`, `.verdict-level`, `.cadence-none`, `.clinic-table--index tr.is-paused`, `.standing-panel`,
  `.standing-rating-meta`.
- [D] `.details-grid--reverse`, `.epa-section-title`, `.epa-figure-row`, `.epa-lines`, `.epa-level-line`, `.epa-stack`,
  `.epa-page-alert`, `.star-level`, `.activity-cell-paused`, `.trajectory-figure`, `.trajectory-card`,
  `.trajectory-head`, `.trajectory-chart--sized`, `--wide`, `--narrow`, `.trajectory-chart-band`, `-below`, `-minimum`,
  `-exit`, `-rung`, `-lane`, `-today`, `-band-label`, `-note`, `-line-label`, `-minimum-label`, `-dots`, `-hollow`,
  `.trajectory-key` and its swatches, `.trajectory-table`, `.trajectory-msf`; and the edited originals of note 6.

Never shipped: `.standing-summary` (no board uses it), `.progress-row--waived` (C13), the canvas's `.r2-*`.

## As built after wave 1

(The integrator fills this in after merging A1 and A2, from the code, before wave 2 starts.)

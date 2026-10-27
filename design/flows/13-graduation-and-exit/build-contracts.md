# T379 build contracts (flow 13)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report. Each name says
whether it exists (a path, at master `3443d894`, which holds T312's `c9d6fe13`) or is new (on `t379`, written by § 0).
Lanes R, E, V and P run side by side in one wave (`build-lanes.md`).

## § 0: on `t379` before any lane is cut, written by the integrator's planner

- **`design/flows/13-graduation-and-exit/round-3/boards-text.md`**: the text of every `R3-*` board (192: the three Spec
  and two Steps boards, then H, X, E, V, P, C and R, each at its widths; style, script, svg, head and helmet blocks
  stripped; aria-labels, placeholders and input values kept as `[aria-label: …]`, `[placeholder: …]`, `[value: …]`), one
  heading per board with its board title, its caption and chips quoted after `>` as annotation, in the canvas's order
  (`canvas.json`, page `r3`, top to bottom then left to right). A table's rows are one `| a | b |` line each. Design data,
  read by every lane.
- **The one migration** (T313, Q16; lane V removes the job itself): `20261027115106_T379_PortfolioExportCleanupRetired`
  (`.cs` and `.Designer.cs`, made by `dotnet ef` and read: no model change, so the snapshot is unchanged and the generated
  `Up` was empty; it now deletes the `ScheduledJobRuns` and the `ScheduledJobDefinitions` row of `portfolio-export-cleanup`,
  which `ScheduledJobHost.SeedDefinitionsAsync` wrote and never removes; `Down` writes nothing back). **No lane adds a
  migration.**
- **The end commands' result** (F2, M2; lane P builds the result and the forecast on it):
  - `ProgrammeEndResult(int TakenBack)` (NEW, `src/Wombat.Application/Features/Trainees/ProgrammeEndResult.cs`).
  - `CompleteTraineeProfileCommand(int Id, DateOnly CompletedOn, ClaimsPrincipal Principal) : IRequest<ProgrammeEndResult>`
    and `DeactivateTraineeProfileCommand(int Id, DateOnly DeactivatedOn, ClaimsPrincipal Principal) :
    IRequest<ProgrammeEndResult>` (were `IRequest`): each returns the count it took back.
  - `ProgrammeEndCredit.CountTakenBackAsync(IQueryable<Activity>, string traineeUserId, DateOnly endedOn,
    CancellationToken) : Task<int>` (was `TakesCreditBackAsync : Task<bool>`; `internal`): the activities about the
    trainee observed after the day that record they credited. `SaveAsync(…, int takenBack, …)` replays when it is above
    zero. Lane P's `GetTakeBackForecastQuery` calls `CountTakenBackAsync` for the day typed, so the dialog's forecast and
    the command's result count by one rule.
  - `TraineeProfileEdit.razor`'s `EndAsync` takes `IRequest<ProgrammeEndResult>` (the page still ignores the count: lane
    P's). Tests: `CreditAfterProgrammeEndTests` (1 taken back by a withdrawal and by a late completion, 0 otherwise),
    `EpaCreditLockTests` (the handlers resolved as `IRequestHandler<…, ProgrammeEndResult>`), `TraineeProfileEndDateTests`'
    fake sender.
- **The end, read once** (S4, S8, M4; lanes R, E and P): `EndedProgrammes.Of(TraineeProfile) : EndedProgrammeDto?`
  (NEW member, `src/Wombat.Application/Features/Users/EndedProgrammes.cs`): null while the programme runs or for an end
  Wombat did not record; otherwise `EndedProgrammeDto(DateOnly EndedOn, bool Completed)` with NEW
  `int? TrainingYearAtEnd { get; init; }` (`TraineeProfile.StageOn` on the last day). An erased person's profile reads as
  a withdrawal on the erasure day. `LatestForAsync` (flow 12's) leaves `TrainingYearAtEnd` null; flow 12's pages read none.
  Test: `EndedCurriculumListTests.AnEndedProgramme_ReadsItsEndAndTheTrainingYearItEndedIn_AndARunningOneNone`.
- **The record's STARs** (F6, S5, S8; lane R's Home and record line, lane P's account sentence):
  `RecordStars.ReadAsync(IApplicationDbContext, ClaimsPrincipal, string traineeUserId, DateOnly asOf, CancellationToken) :
  Task<RecordStarsDto>` and `RecordStarsDto(bool AnyIssued, int InForce)` (NEW,
  `src/Wombat.Application/Features/EntrustmentDecisions/RecordStars.cs`). `AnyIssued`: any `EntrustmentDecision` of the
  person, in force or not (F6). `InForce`: the standing's own count (`EntrustmentStandingReader.ReadAsync` on the
  preferred profile, no ratings), so an ended record's STARs are read as the programme left them (T312). No scope check:
  the caller's. Test: in `EndedCurriculumListTests.HerStars_AreReadAsTheyStoodOnTheLastDay` (3 in force, any issued; a
  registrar with none).
- **`MayExport`** (F1, M1): `public bool MayExport { get; init; }` (NEW) on `CommitteeReviewDetailDto`
  (`Features/CommitteeDecisions/CommitteeDecisionDtos.cs`) and `ProgrammeTraineeDto`
  (`Features/Programme/Trainees/ProgrammeTraineeDtos.cs`), which lane E's handlers fill, and on `TraineeProfileDto`
  (`Features/Trainees/TraineeProfileDto.cs`), which lane P's fills: `TraineeScopeResolver.MayReadAsync(db, caller,
  trainee user id)`, the export command's own check. False wherever a handler does not fill it (the commands answering
  with the review, the list reads).
- **`PortfolioLinks`** (NEW, `src/Wombat.Web/Components/Shared/PortfolioLinks.cs`), so no page writes one by hand:
  `const string MyProgress`, `MyAuthorisations`, `MyActivities`, `Export` (`/portfolio/export`), `Verify`
  (`/portfolio/verify`: a plain `<a href>`, S13), `Trainees`, `ProgrammeTrainees`; `ExportFor(string traineeUserId)`
  (`/portfolio/export/{escaped id}`), `TraineeProfile(int profileId)` (`/admin/trainees/edit?id=`),
  `ProgrammeTrainee(int profileId)` (`/programme/trainees/{id}`). `PortfolioSharedTests` holds each against the menu and
  the page that takes it, and the staff route against whichever page lane E gives it.
- **`PortfolioWords`** (NEW, `src/Wombat.Web/Components/Shared/PortfolioWords.cs`): `ExportAction(name)` "Export Lerato
  Molefe's portfolio" (the header action on three pages, and the staff export's h1 and tab, Q5), `ProgrammeOf(name)`
  "Lerato Molefe's programme" (the profile's h1, tab and crumb, lane P; the staff export's middle crumb, lane E, M4),
  `Possessive(name)` ("Pieter du Plessis's").
- **`ExportPortfolioAction`** (NEW, `src/Wombat.Web/Components/Shared/ExportPortfolioAction.razor`): the header action,
  `<a class="btn btn-outline" href="@PortfolioLinks.ExportFor(id)"><Icon Name="download" />Export ‹name›'s portfolio</a>`,
  drawn only when `MayExport` (EditorRequired: `TraineeUserId`, `TraineeName`, `MayExport`). A page puts it in
  `PageHeader`'s `Actions`, which draws `.actions-cell`. Lanes E (review page, programme page) and P (the profile) mount it;
  neither changes it but by a default-valued parameter.
- **The record's personal links** (Q2, N3): `NavItems.PersonalFor` gives a holder of the `trainee_record` claim who does
  not hold Trainee `[MyProgress, ExportPortfolio, MyDataRights]`; a holder of Trainee keeps `[MyProgress, MyDataRights]`
  (Export portfolio is among the Trainee's own links); anyone else `[MyDataRights]`. The item is the Trainee's own
  `NavItems.ExportPortfolio`, so on `/portfolio/export` it lights itself and `TrailTo` gives no trail (F4) with no change
  to `NavOwners`. DESIGN.md § The NavMenu's personal-links table has the row. Tests: `NavMenuAuthorizationTests` (the
  graduate's menu, A.7.4; the graduate acting as Assessor, H03; the Coordinator with a record; NEW
  `ATraineeActingAsAnotherRole_IsNotOfferedExportPortfolio_AsAPersonalLink`). **Owner rows are the lanes'**: lane R's
  `MyAuthorisations` and `MyActivities`, lane E's staff export component.
- **The withdrawal's badge** (F10): `BadgeFor.ProgrammeOutcome(bool completed)` (NEW, `Components/Shared/BadgeFor.cs`):
  `badge-completed` or NEW `badge-withdrawn`; app.css `.badge-withdrawn` under `/* ── Badges ── */` (the neutral tint under
  its own name); DESIGN.md § Badges amended. Tests: `BadgeForStatusTableTests`, `DefinedClassTests`,
  `StylesheetRuleTests.Flow13Shared`.
- **The `.details-list` value column** (round-3-check build note 2: R03 and R06 17px sideways at 901): `.details-list > div`
  is `minmax(8rem, max-content) minmax(0, 1fr)` (was `… 1fr`), flow 05's component, under Detail / stack lists. Test:
  `StylesheetRuleTests.Flow13Shared`.
- **`app.css`, `/* ── Graduation and exit ── */`** at the file's end, after flow 12: one comment saying what is shared and
  where, then the four lane headings, each with its `/* end … */` line and nothing between: "Flow 13: the record (T379)"
  (R), "Flow 13: the export (T379)" (E), "Flow 13: verify (T379)" (V), "Flow 13: the programme's end (T379)" (P). DESIGN.md
  § app.css section order has the heading and one placeholder line per lane (each lane replaces its own) and the sentence
  under the block.
- **A Release-build fix found on the way** (not flow 13's): `CreditAfterProgrammeEndTests:294` and
  `EndedCurriculumListTests:437,467` named a tuple's elements in `Be((EndRecorded: true, …))`, CS8123, which Release's
  `TreatWarningsAsErrors` makes an error: T312 landed them building in Debug. The expected tuples are unnamed now, their
  names in the reason.
- Suites on `t379` at § 0: Domain 835, Application 4144, Infrastructure 1034, Architecture 52, Web 4158, Integration 511.

## § 1: lane R provides

- `GetFormerTraineeHomeQuery` and its DTO (lane R names them), `FormerTraineeRecordCard`: Home's no-role branch. Nothing
  another lane reads.
- **Owners:** `NavOwners`' `MyAuthorisations` row becomes `UnderAPersonalLink` → My progress; a NEW `MyActivities` row the
  same (a Trainee keeps the item lit, `Lit` reads the menu first). `ActiveNavItemTests` rows.
- **My authorisations' policy:** `TraineeOrFormerTrainee` (Q3), the one role check this build adds.

## § 2: lane E provides

- `GetExportSubjectQuery` and its DTO (Spec 2's row; lane E names the members): read before the form, null for every
  refused cause, `ProfileId` the preferred profile. Nothing another lane reads.
- **The staff export's component** for `/portfolio/export/{TraineeUserId}` (S2), its name lane E's, and its `NavOwners` rows:
  Trainees for the Institutional admin, Programme trainees for `ProgrammeScope.RosterRoles`. `PortfolioSharedTests`
  holds the route.
- `MayExport` filled on `CommitteeReviewDetailDto` (by `GetCommitteeReviewByIdQuery` only) and `ProgrammeTraineeDto`.

## § 3: lane V provides

- Nothing another lane reads. The retired job is gone from the registry (A.2.1's eight jobs); § 0's migration removes its
  stored rows. Verify's words are lane V's.

## § 4: lane P provides

- `TraineeProfileDto`'s new members beside § 0's `MayExport` (the end's reading, the last ratified decision, the account
  sentence's DATA), `GetTakeBackForecastQuery`, `ListEndedProfilesQuery`, the last-ratified-decision reader: lane P's
  alone.
- The profile's h1 through `PortfolioWords.ProgrammeOf`, which lane E's staff trail repeats as its middle crumb and links
  by `PortfolioLinks.TraineeProfile(ProfileId)`: the two must read the same.

## § 5: words and constants already built and kept (lanes use them, do not reword)

- Spec 3's built words: "The start date must be before or equal to the end date." (`ExportPortfolioCommandValidator`),
  "Enter the content hash to check.", "No matching export found. This hash does not correspond to any PDF generated by
  this system." (Verify), "Trainee marked complete. The Trainee role has been removed and a graduation email sent.",
  "Leave this empty to derive it from the curriculum window.", "Could not load your Home. …", "No active authorisations
  yet", "Left the programme: 2026-10-24", and the subtitles S7 names: "Update curriculum and completion details for this
  trainee.", "The record of a programme that has ended.", "Your record of the programme you ended: what your curriculum
  held you to in each period.", "Everything you have filed or logged, newest encounter first."
- The export command's refusal "You are not authorized to export this portfolio." (`ExportPortfolio.cs`): behind the
  Unavailable page, unchanged.
- T305's ended-record line "This programme has ended, so its record is archived and cannot be changed." and the
  profile's read-only details; flow 12's no-role sentence on the user page ("‹name› holds no role: they completed the
  programme on ‹date›. …"), read there, not here.
- `TraineeHomeWords.Ended` (the Trainee's Home's ended line), "No role assigned" and its sentence (Home, H04).
- `TraineeAdministrationRules.TraineeNoteOnTraineePages` and `DemandTraineeAdministration` (the T290 note, asked before any
  read).
- `StatePanel`'s Try again and `LoadingContent`, `ActionResult` (`Id`, `FocusOnLoad`, `FocusAfterRender`), `ConfirmDialog`
  (`CancelLabel`, `DangerAction`, `BodyContent`), `DataTable` (`StackWide`, `Caption`), `PagerControls` as built (rendered
  only when `TotalCount > PageSize`), `DashboardFrame`, `DashboardCard`, `PageHeader` (`Trail`, `Actions`, `Page`,
  `CurrentCrumb`), `FormField`, `FormActions`, `Alert` (`Role=""` for a standing notice), `Icon`, `InFlight`,
  `RowNames.Distinct`, `PageFocus.FocusByIdAsync`, `RefusalText.Of`, `ProgressLinks`, `EndedRecord` (unchanged, its
  golden-markup test held), `EntrustmentStandingPanel`, `PeopleLinks.User` (the ended record's "Open ‹name›'s user page").

## Across lanes

- **Typed refusals:** a lane that needs a refusal's reason on the page writes its own exception on the pattern of
  `TraineeAdmissionRefusedException` (derived from `InvalidOperationException`, a reason enum), or maps a
  `ValidationException`'s property name; never the message's text. No shared base.
- **Who reads what:** every staff read keeps its scope check; an out-of-scope or unknown id reads as not found (one body:
  "Profile unavailable", "Portfolio unavailable"), never a refusal that leaks it. `MayExport` and the subject query use the
  command's own `MayReadAsync`, so a page never offers what the command refuses, and an Administrator is never refused.
- **Dates and times:** ISO dates; a South African day from `ProgrammeCalendar.DateOf(utc)` or `QuotaCalendar.Today`; a
  moment "2026-10-26 22:51 SAST" from `ActivityMoments.When`. No `ToLocalTime()`, no UTC on a page.
- **Titles** (each tab "{h1} · Wombat"): Home; "Export portfolio" (own, Q17); "Export ‹name›'s portfolio" (staff); "Portfolio
  unavailable"; "Verify portfolio export"; "‹name›'s programme" (the profile), "Trainee profile" before the read and on
  failure (P14, P15), "Profile unavailable" (P12, P13); "Trainees"; "My progress", "My authorisations", "My activities".
- **Owners:** § 0 changes the menu only (Export portfolio for a record holder). Lane R: `MyAuthorisations` and
  `MyActivities` under My progress. Lane E: the staff export's component under Trainees and Programme trainees. The profile
  keeps its row (Trainees, Institutional admin).
- **No new role check** but Q3's (lane R). Export keeps its bare `[Authorize]`; Verify stays `[AllowAnonymous]` and
  static.

## § 6: words on no board, marked NEW (the fix pass)

The sentences the build ships that no round 3 board draws, each with why there is none. Each lane lists its own in its
report; the integrator fills this table after the wave. Round 4, if any, sees these.

| Words | Where | NEW because |
|---|---|---|
| "Your record of the programme stays in Wombat, read-only. Sign in to read it on My progress, to export your portfolio, and to download the certificate of any STAR issued to you." then the link "Open My progress" (a `.btn` link in the HTML, "Open My progress: ‹url›" in the text), for "Your committee has ratified your final entrustment decisions and your portfolio of evidence is available in Wombat." | `GraduationEmail` (lane P) | Q12: no board draws an email (flow 19's) |
| "The profile could not be saved just now. Nothing has changed. Try again, or come back in a few minutes." | the trainee profile's Profile details, a save that failed for a reason that is not a refusal (lane P) | T329: no board draws a failed save |
| "None of your STARs is in force now; My authorisations lists them." | Home's card (lane R) | Spec 3 PROPOSED, drawn on no board (S8) |
| "Your 1 STAR in force, with its certificate to download." | Home's card, one STAR in force (lane R) | the singular of H01's line; no board draws one STAR |
| "Every period of your programme, and your standing against Annexure A." | Home's card, My progress's line when no training year is known (lane R) | H01's line needs a training year; no board draws none |
| "No STAR was issued to you, so there is no certificate to download." | My authorisations' empty card, for a reader holding no Trainee role (lane R) | Spec 3's Empty row; R09 keeps the built words |
| "This is not a SHA-256 hash: it has 1 character, and a SHA-256 hash has 64 hexadecimal digits (0 to 9, a to f)." | Verify, a one-character entry (lane V) | the singular of S10's first refusal |
| "‹n› times, the last on ‹date› at ‹hh:mm› SAST" for n = 2 | Verify's answer (lane V) | the boards draw "Once" and "3 times" only |
| Verify's not-found third sentence (`VerifyExportWords.NotFoundRest`) | Verify, not found (lane V) | marked NEW in its code comment; check against V04 |

# T387 build contracts (flow 14)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report. Each name says whether
it exists (a path, at master `c47bc6e0`) or is new (on `t387`, written by § 0). Lanes D, K, M and P run side by side in
one wave (`build-lanes.md`); the runbook lane W follows the merge.

## § 0: on `t387` before any lane is cut, written by the integrator's planner

- **`design/flows/14-data-rights/round-3/boards-text.md`**: the text of every `R3-*` board (149: the four Spec and two
  Steps boards, then M, Q, D, C, K and U, each at its widths), one heading per board (`######## BOARD file — canvas title`),
  the `anno` chip quoted after `>`; style, script, svg, head and helmet stripped; aria-labels, placeholders, input values,
  `[checked]`, `[disabled]`, `[aria-disabled]` and alt text kept; a table row one `| a | b |` line, a `dt` opened `**`.
  Board order is `canvas.json`'s page `r3`, top to bottom then left to right. Design data, read by every lane.
- **The one migration** `20261030082058_T387_DataRightsFlow14` (`.cs` and `.Designer.cs` by `dotnet ef`, read and edited
  by hand; snapshot updated by the tool): drops `AspNetUsers.OptOutOfOptionalProcessing` (T318, Q6) and
  `DataRightsRectifications.TargetId`; adds `DataRightsRectifications.AppliedByUserId` (varchar 450, required); makes
  `AppliedOn` required (a SQL step fills any null first); moves any `DataRightsRequests.Status = 1` (UnderReview) to 0;
  maps `xmin` on `DataRightsRequests` as the concurrency token with **no DDL** (EF's generated `AddColumn` removed by hand,
  as T121's was). **No lane adds a migration.**
- **Domain** (`src/Wombat.Domain/DataRights/`):
  - `DataRightsRequestStatus` without `UnderReview`; the members keep their stored numbers (`Submitted = 0, Approved = 2,
    Rejected = 3, Completed = 4, Withdrawn = 5`). `DataRightsRequest.Review()` is gone (Q8).
  - `Approve`, `Reject` and `Withdraw` accept only `Submitted` (Q14). NEW `bool IsOpen` (Submitted or Approved) and
    `WithdrawOnErasure()` (accepts an open request: what an erasure of the requester withdraws, F3).
  - `DataRightsRectification`: no `TargetId`; NEW `string AppliedByUserId`; `DateTime AppliedOn` (required);
    `Create(Guid requestId, string targetType, string fromValueJson, string toValueJson, string appliedByUserId, DateTime
    utcNow)`. The record is always the requester's (the request says whose).
- **Who may act** (Q1, Q21, F9; `src/Wombat.Application/Features/DataRights/DataRightsAuthorization.cs`): one role set,
  `ReviewRoles = [Coordinator]` at the request's institution, plus the global Administrator; `RectificationRoles` and the
  Speciality admin are gone.
  - `CanReview(p, r)` = in scope and **not the requester**, the Administrator included.
  - `CanRectify(p, r)` = `CanReview(p, r)`.
  - NEW `CanRead(p, r)` = in scope or the requester (a reviewer reads their own request).
  - NEW `OwnRequest(p, r) : DataRightsOwnRequest` (`No`, `AnotherCoordinatorDecides`, `AnotherAdministratorDecides`).
  - NEW `CanDownload(p, r)` = Administrator or requester (`DemandExportAccess` uses it).
  - Tests: `DataRightsAuthorizationTests` (the fold, the own-request refusals, the Administrator's own).
- **The page DTOs** (`DataRightsDtos.cs`; positional members first, the rest `init`):
  - `DataRightsRequestSummaryDto(Guid Id, string RequesterName, DateTime RequestedOn, Type, Status)` + `RequesterEmail`,
    `DateOnly? ErasedOn`, `InstitutionName` (was `RequesterDisplayName`, the stored name). `PagedDataRightsResult(Items,
    TotalCount, Page, PageSize)` + `bool EveryInstitution` (§ 0 fills it) and `string? InstitutionName` (the scoped
    caller's; null with `EveryInstitution` false is a Coordinator set to no institution, S4).
  - `MyDataRightsDto(string? InstitutionName, IReadOnlyList<MyDataRightsRequestDto> Requests)`;
    `MyDataRightsRequestDto(Guid Id, DateTime RequestedOn, Type, Status, DateTime? DecidedOn, string? DecisionNote,
    DateTime? CompletedOn, IReadOnlyList<string> CorrectedRecords)` (keys of `DataRightsRecords`). Never who decided (Q5).
  - `DataRightsRequestDetailDto(Id, RequesterUserId, RequesterName, RequestedOn, Type, Status, Reason, DecisionNote,
    DecidedByUserId, DecidedOn, CompletedOn)` + `RequesterEmail`, `InstitutionName`, `DateOnly? ErasedOn`,
    `DecidedByName`, `IReadOnlyList<DataRightsCorrectionDto> Corrections`, and the flags **§ 0 fills**: `MayDecide`
    (`CanReview` and Submitted), `MayRecord` (`CanRectify`, a Rectification, Approved), `MayDownload` (`CanDownload`),
    `OwnRequest`; and `ProgrammeStartRecordDto? ProgrammeStart` (`int ProfileId, bool Ended, bool MayOpen`), `string?
    NameFromSignIn` (lane K fills both).
  - `DataRightsCorrectionDto(Guid Id, string TargetType, string FromValue, string ToValue, string AppliedByUserId, string
    AppliedByName, DateTime AppliedOn)`: the values as typed (the JSON strings read back).
  - `ErasureOutcome(string Pseudonym, int ReviewsWithdrawn, int CampaignsWithdrawn, int RequestsWithdrawn, int
    ProfilesEnded)`; `ApproveDataRightsResult(DataRightsRequestDto Request, ErasureOutcome? Erasure)`.
  - `ErasureForecastDto(string PersonName, IReadOnlyList<string> StaffRoleKeys, IReadOnlyList<ErasureForecastReview>
    OpenReviews, int OpenReviewsElsewhere, IReadOnlyList<ErasureForecastCampaign> OpenCampaigns,
    IReadOnlyList<ErasureForecastRequest> OtherOpenRequests, string? ProgrammeName, IReadOnlyList<ErasureForecastReview>
    KeptReviews)`; `ErasureForecastReview(int Id, string TypeLabel, string PanelName, CommitteeReviewState State)`;
    `ErasureForecastCampaign(int Id, string Name, MsfCampaignState State)`; `ErasureForecastRequest(Guid Id, Type,
    DateTime RequestedOn)`. `DataRightsRequestDto` is unchanged: what Submit, Reject and Complete answer.
- **The requests** (`Commands/`, `Queries/`):
  - `ApproveDataRightsRequestCommand : IRequest<ApproveDataRightsResult>` (§ 0 returns the erasure's outcome).
  - `ApplyRectificationCommand(Guid RequestId, string TargetType, [Redact] string FromValue, [Redact] string ToValue,
    ClaimsPrincipal Principal) : IRequest<DataRightsCorrectionDto>`: plain strings in, stored as
    `JsonSerializer.Serialize(string)`; the recorder is the caller; refuses with `NotAnApprovedRectification` (§ 0).
  - `UpdateObjectionFlagsCommand(bool OptOutOfDigestEmails, ClaimsPrincipal Principal) : IRequest<ReminderEmailsSaved>`,
    NEW `ReminderEmailsSaved(bool OptOutOfDigestEmails, bool Changed)` (N3, read against the stored state);
    `ObjectionFlagsDto(bool OptOutOfDigestEmails)`; `IObjectionFlagUpdater.UpdateAsync` returns it.
  - `GetMyDataRightsRequestsQuery : IRequest<MyDataRightsDto>`; `GetDataRightsRequestByIdQuery :
    IRequest<DataRightsRequestDetailDto?>` (**null for an unknown and a foreign id alike**, S14; never throws for either);
    `ListDataRightsRequestsQuery` pages at 20 by default (Q18).
  - NEW `GetErasureForecastQuery(Guid RequestId, ClaimsPrincipal Principal) : IRequest<ErasureForecastDto?>` with its
    validator; **its handler is lane D's**. No page sends it until lane P's dialog.
- **The typed refusal** (NEW `DataRightsRefusedException(DataRightsRefusal Reason, string message, Exception? inner)`,
  derived from `InvalidOperationException`): `Withdrawn`, `DecidedMeanwhile`, `LastAdministrator`, `ErasureNotConfigured`,
  `PersonChanged`, `NotAnApprovedRectification`, `NoCorrectionRecorded`. The pages map the reason, never the message.
- **The records** (NEW `DataRightsRecords`, Application): `AccountFirstName` "Account.FirstName", `AccountLastName`,
  `ProgrammeStart` "TraineeProfile.ProgrammeStart", `Other(name)` "Other:‹name›", `OtherNameMaxLength = 194`, `Fixed`,
  `IsValid`, `OtherName`, `Label` ("Account › Last name", "Other › ‹name›").
- **The labels** (NEW `DataRightsLabels`, Application): `Type`, `Status`, `OfferedTypes` (the four, not Objection), and
  `Statuses` (the filter's order: Submitted, Approved, Completed, Rejected, Withdrawn). NEW `DataRightsWords` (Web,
  `Components/Shared/`): `Type`, `Status`, `When(utc)` "2026-10-28 00:01 SAST" (`ActivityMoments.When`), `Day`,
  `DayOf(utc)`, `Title(type, name)` "Erasure request from Sipho Ndlovu", `Record(key)`, `QueueLink`, `MineLink`,
  `RequestLink(id)`, `DownloadLink(id)`.
- **The badge** (Q22): `BadgeFor.DataRightsRequest`: Submitted blue, Approved `badge-accepted`, Completed green, Rejected
  red, Withdrawn `badge-withdrawn`. `BadgeForStatusTableTests` and DESIGN.md § Badges amended.
- **The mail** (Q5, M3, F4): NEW `DataRightsDecisionEmail.Build(string toEmail, DataRightsRequestType type,
  DataRightsNoticeOutcome outcome, DateOnly onDay, string myDataRightsUrl)` with `DataRightsNoticeOutcome` `Approved`,
  `Rejected`, `Completed`, `Erased` (no link); NEW `AccountNotices.SendToAddressAsync(string? address, string userId,
  string kind, Func<string, EmailMessage> build) : Task<bool>` (logs the id and kind, never the address); the class
  comment names the no-decider exception. Kinds: `data-rights-approved`, `-rejected`, `-completed`, `-erased`.
- **The erasure's request rewrite** (F3, Q11; `ErasureExecutor`, tracked rows, before the save that precedes the
  account's update): every request of the person gets the pseudonym as `RequesterDisplayName`; `DecidedByUserId` and
  `DataRightsRectification.AppliedByUserId` the pseudonym where the person decided or recorded; the person's other open
  requests `WithdrawOnErasure()`, counted. `ExecuteAsync` returns `ErasureOutcome`, counted where each was ended.
  `ErasureCoverageTests` no longer excludes `DecidedByUserId`. `OptOutOfOptionalProcessing` is gone from the executor.
- **The concurrency token** (F8): `xmin` on `DataRightsRequestConfiguration`, as `CommitteeReviewConfiguration`'s.
- **Pages touched only to compile** (each lane rebuilds its own): `Profile/DataRights.razor` (the optional box gone, the
  new DTOs), `RequestsList.razor` (`RequesterName`, 20 a page), `RequestDetail.razor` (`DataRightsRequestDetailDto?`,
  Decide while `MayDecide`, a re-read after a decision). `ActionFocusTests`, `ActiveNavItemTests` and
  `DataRightsDigestOptOutTests` follow the new shapes; `DataRightsQueryHandlerTests` holds the null read.
- **`app.css`, `/* ── Data rights ── */`** at the file's end: one comment saying what is shared, `.request-id`,
  `.data-rights-table tbody th[scope="row"]`, then the two page lanes' headings with their `/* end … */` lines and nothing
  between: "Flow 14: my data rights (T387)" (M), "Flow 14: the requests (T387)" (P). The **list selects' 44px** below
  641px (`.search-container .form-select`, `.pager .form-select`, M4) is a system rule beside `.filter-actions`' phone
  rule under Programme. DESIGN.md § app.css section order has the heading, the shared line and one placeholder per page
  lane, and the sentence under the block. Test: `StylesheetRuleTests.Flow14Shared`.
- **Shared tests**: `DataRightsFlow14SharedTests` (the narrowed rules, `WithdrawOnErasure`, the stored numbers, the
  records, the mail, the address overload).
- Suites on `t387` at § 0: see `build-lanes.md` § "§ 0's counts".

## § 1: lane D (decide and erase) provides

- `GetErasureForecastQueryHandler` (F5: named at the caller's institution, counted elsewhere; the Administrator's names
  all; gated as the command; null otherwise). Nothing else another lane reads but the refusals below.
- Approve and Reject throw `DataRightsRefusedException` with `Withdrawn` (checked first, and on a lost race after a
  re-read), `DecidedMeanwhile`, `LastAdministrator` (Erasure only, before `ExecuteAsync`), `ErasureNotConfigured` (the
  log names `Wombat__PseudonymSalt`; the message does not), `PersonChanged` (the executor's, typed). Lane P maps each.
- The mails after the commit: approve (`Approved`; an Erasure `Erased` to the address read with `GetByIdAsync` before
  `ExecuteAsync`, through `SendToAddressAsync`), reject (`Rejected`).

## § 2: lane K (corrections and reads) provides

- The reads' members § 0 leaves: the live name and email (`GetContactsAsync`, F1, S1), the pseudonym and `ErasedOn`
  (from `DataRightsErasureRecord`, N5) for an erased requester, `InstitutionName` (row, detail, `MyDataRightsDto`, the
  result's scope), `DecidedByName`, `Corrections` with `AppliedByName`, `CorrectedRecords`, `ProgrammeStart` (the
  preferred profile, `EndedProgrammes.Of`, may-open: Administrator or an Institutional admin of its institution) and
  `NameFromSignIn` (`GetSignInMethodsAsync`, the first institutional sign-in's `DisplayName`).
- Submit stores "First Last" read with `GetByIdAsync` (F1) and refuses Objection (Q8); Withdraw maps a decided request
  or a lost race to `DataRightsRefusedException(DecidedMeanwhile)`.
- Apply: `DataRightsRecords.IsValid` in the validator (Other's 194), values not blank after trim; Complete:
  `NoCorrectionRecorded` ("Record at least one correction before completing the request."), `NotAnApprovedRectification`;
  the `Completed` mail after the commit (PROPOSED: the boards draw it, C06).

## § 3: lane M (My data rights) provides

- Nothing another lane reads. The requester's words (`MyDataRightsWords`, its own), its owner and title unchanged.

## § 4: lane P (the queue and a request's page) provides

- Nothing another lane reads. Its words class (`DataRightsRequestWords` or its own name), the erasure dialog, the Unavailable
  state (Data rights requests lit, Q17), and `NavOwners`' row unchanged (`RequestDetail` under Data rights requests).

## § 5: words and constants already built and kept (lanes use them, do not reword)

- "Request submitted. You will be notified when it is reviewed.", "Request withdrawn.", "Please provide a reason for your
  request.", "A decision note is required.", "Download the export of the Export request made ‹date› ‹time› SAST" (now in
  SAST), `ErasureExecutor.PersonChanged`'s sentence (D11 draws it), `DataRightsAuthorization.RefusalMessage` (behind every
  out-of-scope command, never on a page), the digest help's first sentence and its day count from
  `DashboardThresholds.AssessorNudgeDays` (flow 06), `TraineeProfileWords.EndedLine` (flow 13), `WombatRoleLabels.For`
  (flow 12), `CommitteeDecisionWording`'s type labels.
- `StatePanel` (`LoadError`, `OnRetry`, `FocusAfterRetry`), `ActionResult` (`FocusOnLoad`, `FocusAfterRender`),
  `ConfirmDialog` (`BodyContent`, `DangerAction`, `CancelLabel`, `InitialFocusId`, `ReturnFocusId`), `DataTable`
  (`StackWide`, `Caption`), `PagerControls` (`PageSizeLabel`, rendered only when `TotalCount > PageSize`, its focus on the
  list's heading), `PageHeader` (`Trail`, `CurrentCrumb`, `Page`), `FormField`, `FormActions`, `Alert` (`Role=""` for a
  standing notice), `Icon`, `InFlight`, `RowNames.Distinct`, `PageFocus.FocusByIdAsync`, `.user-page`, `.user-section`,
  `.person-cell`, `.choice-group`, `.clinic-table--stack(-wide)`, `.search-container`/`.search-field`/`.filter-actions`
  (flow 06's filter pattern, `DecisionsDue.razor:44`).

## Across lanes

- **Typed refusals only**: a page maps `DataRightsRefusedException.Reason` or a `ValidationException`'s property name to its
  words; never `ex.Message`. Any other failure is a fixed sentence and goes to the log (T329).
- **Who reads what**: an out-of-scope or unknown id reads as null (the page's Request unavailable); the commands keep
  `DemandReviewAccess`/`DemandRectificationAccess` and throw `UnauthorizedAccessException(RefusalMessage)` for both.
- **Names**: every sentence naming the requester uses `RequesterName` (live, or the pseudonym once erased); the Requester
  cell is `.person-cell` (the name over the address, or the pseudonym over "Erased ‹day›"). Names are read once per page by
  its query, never in Razor.
- **Times**: `DataRightsWords.When` ("2026-10-28 00:01 SAST", to the minute, Q9) on all three pages; days by
  `DataRightsWords.DayOf`. No `ToLocalTime()`, no UTC on a page; the export's JSON keeps UTC (flow 20's).
- **Titles** (each tab "{h1} · Wombat"): "My data rights"; "Data rights requests"; a request "‹Type› request from ‹name›"
  (`DataRightsWords.Title`), "Data rights request" while loading and on a failed load, "Request unavailable" for null.
- **No new role check, no new route.** The pages keep their policies (`[Authorize]`; `Roles = "Administrator,Coordinator"`).

## § 6: words on no board, marked NEW (the fix pass)

| Words | Where | NEW because |
|---|---|---|
| `DataRightsDecisionEmail`'s three subjects and its sentences ("Your Export request was approved on 2026-10-28.", "Read the answer, and download your data, on My data rights.", the Rectification's "It is being corrected. You will be emailed again when it is complete; read the answer on My data rights.", the rejection's, the completion's "Read what was corrected on My data rights.", the erasure's two) and the link "Open My data rights" | the decision notice (§ 0) | no board draws a mail (Q5) |
| "Choose the request type." | `MyDataRightsWords.TypeRequired` (lane M) | no radio is chosen as the page opens (R3-M01), so a refusal is needed |
| "Give your reason in 4,000 characters or fewer." | `MyDataRightsWords.ReasonTooLong` (lane M) | the appeal lodge's words; no board draws a long reason refused |
| "The request could not be withdrawn just now. Nothing has changed. Try again, or come back in a few minutes." and "The reminder emails could not be saved just now. …" | `WithdrawFailed`, `SaveFailed` (lane M) | M16's shape for the two other actions (T329) |
| "This request was decided before it could be withdrawn. Nothing was withdrawn." | `DecidedBeforeWithdrawn` (lane M) | approved by the operator (top of `build-lanes.md`) |
| "Nobody has made a data rights request yet." / "Nobody at ‹institution› has made a data rights request yet." | `DataRightsRequestWords.NoneBody` (lane P) | no board draws an empty unfiltered queue but S4's |
| "This request is no longer waiting for a correction. Nothing was recorded." | `NoLongerApproved` (lane P) | a record or Complete refused on a request no longer approved (the page reads it again) |
| "What the erasure would end could not be read just now. Nothing has changed. Try again, or come back in a few minutes." | `ForecastFailed` (lane P) | the forecast's failure; no dialog opens |
| "The request could not be completed just now. Nothing has changed. …" | `CompleteFailed` (lane P) | C10's shape for Complete request |
| The dialog's singulars ("The role ‹X› and its scope.", "1 MSF campaign not yet released: ‹n›.", "1 other open data rights request: … It is withdrawn.") and the result's "‹n› programmes ended today" | `DialogEndsLines`, `Erased` (lane P) | every count has its singular |
| "This person's personal data was erased on ‹day›. The request is kept under the pseudonym." | `ErasedNotice` on a request that is not the Erasure (lane P) | F3's class; K02's other rows need it |
| "This request was withdrawn before it was decided. There is nothing to decide." | `WithdrawnLine` for an erased person (lane P) | the erasure withdrew it, so the person did not (K02's Access) |
| Decide's help for Access and Rectification; the Access and default approval results | `DecideHelp`, `Approved` (lane P) | follow D01 and C01 |
| "You cannot decide your own request: another Coordinator, or the Wombat administrator, decides it." | `OwnRequestLine` for a requester with no institution (lane P) | S4's neighbour |
| "Give your note in 4,000 characters or fewer." and the counter "‹n› of 4,000 characters" under the decision note | `DataRightsRequestWords.NoteTooLong`, `NoteCounter` (the fix pass, review P4) | until the review a long note was refused as missing |
| "Erased with the person's data on ‹day›." in place of the reason and each correction's values on an erased person's requests | `DataRightsLabels.ErasedText` (the fix pass, review S1; the operator's decision of 2026-10-30) | no board draws an erased person's reason or corrections |

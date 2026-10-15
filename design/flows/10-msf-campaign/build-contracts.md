# T369 build contracts (flow 10)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report. Each name says
whether it exists (file:line at `e825e2e2`) or is new. Lanes K and R run in wave 1, lane T in wave 2 (`build-lanes.md`).

## § 0: on `t369` before any lane is cut, written by the integrator

- **`design/flows/10-msf-campaign/round-3/boards-text.md`**: the text of every `R3-*` board (style, script and helmet
  blocks stripped), one heading per board, as flow 09's `round-3/boards-text.md`. Design data, read by every lane.
- **`MsfLinks`** (NEW, `src/Wombat.Web/Components/Shared/MsfLinks.cs`, beside `ReviewLinks.cs`): the MSF routes, so no
  page writes one by hand.
  - `const string Campaigns = "/msf/campaigns"`, `NewCampaign = "/msf/campaigns/new"`, `Coverage = "/msf/coverage"`,
    `MyReports = "/msf/my-reports"`.
  - `static string Campaign(int campaignId)` = `/msf/campaigns/{id}`; `Report(int campaignId)` = `/msf/reports/{id}`;
    `MyReport(int campaignId)` = `/msf/my-reports/{id}` (invariant culture, as `ReviewLinks.MyReview`,
    `ReviewLinks.cs:29`).
  - Today's literals it replaces: `CampaignEdit.razor:385`, `:860`; `CampaignReport.razor:105`, `:248`;
    `CampaignsList.razor:17`, `:21`, `:78`, `:82`; `MyMsfReports.razor:44`. `NavItems.cs:93`, `:102` and
    `HomeFrame.cs:52` stay as they are.
- **`MsfSubjectStanding`** (NEW record, `src/Wombat.Application/Features/MultiSourceFeedback/MsfCampaignRules.cs`):
  `public sealed record MsfSubjectStanding(bool IsCurrent, DateOnly? EndedOn)`.
  - `IsCurrent`: `TraineeScopeResolver.ResolveCurrentAsync(…) is not null` (exists, `TraineeScopeResolver.cs:210`), the
    rule create, invite and open already apply (`MsfCampaignRules.EnsureCampaignTakesNewWorkAsync`, `:108`).
  - `EndedOn`: the subject's most recent `TraineeProfile.EndedOn` (exists, `TraineeProfile.cs:64`) when no profile is
    running; null while one is, or when there is none. A locked trainee is not current and has no `EndedOn` (K13).
- **`MsfCampaignRules.SubjectStandingAsync`** (NEW): `static Task<MsfSubjectStanding> SubjectStandingAsync(
  IApplicationDbContext dbContext, IUserAdministrationService users, string subjectUserId, CancellationToken
  cancellationToken)`. Called by `GetMsfCampaignSetup` (lane K) and `GetCampaignAggregateReport` (lane R), and by
  `ReleaseMsfCampaign` (lane R) for F4. One Application test (current, locked, ended).
- **`MsfCampaignText.ProgrammeEnded(string subjectName, DateOnly endedOn)`** (NEW, `MsfCampaignText.cs`): K14's and
  R10's sentence, "{name}'s programme ended on {yyyy-MM-dd}. A release still records the feedback as evidence on
  {name}'s record, but {name} can no longer open MSF reports to read it." (No pronoun, build-lanes § Never.) Lane K's campaign page and lane R's report both show it.
- **`app.css`, `/* Flow 10: the MSF pages, shared (T369) */`**: the classes two lanes use, rebuilt from `R3-msf.css` on
  the spacing scale:
  - `.msf-page` (the page column; 24 px between sections; it holds the open dialog), `.msf-page--measure` (60rem),
    `.msf-header` (`PageHeader`'s actions stack at 390), `.section-title` and `.section-title--h3` (a card's h2 at
    1.1rem, its h3 at 1rem; `.list-section-title`'s scale, `app.css:2596`), `.msf-stack-card`, `.msf-centre`;
  - the status card, on `.detail-card` with `--emphasis` or `.activity-status--others/--done/--declined` (exist,
    `app.css:1951`, `:2216–2224`): `.status-card`, `.status-head`, `.status-line`, `.status-note`, `.status-actions`;
  - list rows: `.trainee-cell` (a name alone), `.kind-line` (the template over its kind), `.nowrap`;
  - `.msf-inline-link` (a 24 px link in running text, 44 px at 390; flow 08's rule);
  - the dialogs at 390 (M10): `.msf-page dialog .form-actions { flex-direction: column-reverse; }` and
    `.msf-page dialog .btn { min-height: 2.75rem; width: 100%; }` below 641 px. `ConfirmDialog` renders its buttons in
    `FormActions` (`ConfirmDialog.razor:69–76`), safe button first in the DOM, so the action shows on top and the safe
    button is last on screen and first in tab order.
  - Not shipped: `.skeleton-stack` (use `StatePanel`'s `SkeletonCount`/`SkeletonHeight` or `Skeleton`'s `Count`,
    `Width`, `Height`, which exist), and the canvas-only rules.

## § 1: lane R provides, lane T mounts (wave 2)

- **`MsfGroupCards`** (NEW, `Components/Pages/MultiSourceFeedback/MsfGroupCards.razor`): the cards of the groups that
  cleared the threshold, in the enum's order (Peer doctor, Consultant, Nurse, Allied health professional, Other; M2), as
  `MsfAggregationService` already orders them.
  - `[Parameter, EditorRequired] IReadOnlyList<MsfCategoryAggregateDto> Categories` (exists, `MsfDtos.cs:50`); the
    component shows only those with `IsSuppressed` false.
  - `[Parameter] bool ForTrainee`: false, the coordinator's card ("Consultant · 4.33 average rating, from 3 responses");
    true, hers ("Consultant · 4.33 average rating": no count, T249).
  - `[Parameter] string HeadingLevel` = `"h3"`: the group's heading.
  - Each card a `section.detail-card.group-card`, its header the group and its average (`.group-avg`; RB's one line),
    the question once (`.group-question`, never "Prompt.: 4.00", T270), the comments verbatim (`ul.group-comments`).
    No scale question: the group's name alone; several: the name, then each question's average under it (S7).
  - The average to two decimals, invariant culture.
  - It renders nothing about a suppressed group. The suppressed line and the intro ("As Lerato Molefe reads it. …") are
    the page's, not the component's.
- **`MsfReportText`** (NEW, `Components/Pages/MultiSourceFeedback/MsfReportText.cs`), lane R's words, of which lane T
  uses: `Responses(int count)` ("1 response" / "3 responses"), and `RatingScaleNote` ("Ratings are averaged from 1, Well
  below expectations, to 5, Well above expectations.").
- **`MsfCampaignAggregateReportDto`** (exists, `MsfDtos.cs:108`) gains, as `init` properties filled by
  `GetCampaignAggregateReport` for every reader: `string? SubjectName`, `DateOnly OpensOn`, `DateOnly ClosesOn`,
  `int InvitedTotal`, `DateTime? ClosedOn`, `string? ClosedByUserId`, `DateTime? ReleasedOn`, `bool SubjectIsCurrent`,
  `DateOnly? SubjectEndedOn`. `EvidenceRecordedOn` (`:123`), `TeachingContextCount` (`:126`) and `IsSubjectsCopy`
  (`:127`) exist. Lane T reads `TemplateName`, `Kind`, `ReleasedOn`, `CoordinatorNarrative`, `Categories`,
  `CoveredEpas`, `EvidenceRecordedOn`, `TeachingContextCount`, `IsSubjectsCopy` and `State`.
  - `Categories` is empty while the campaign has not closed (F1), whoever reads it.
- **`MsfCoveredEpaDto`** (exists, `MsfDtos.cs:63`) gains `bool WillBeRecorded` (an `init` property, default false): before
  release, whether the release would record this EPA (`MsfCampaignRules.CoverableEpaIdsAsync`, `MsfCampaignRules.cs:39`,
  the predicate `ReleaseMsfCampaign.cs:213–217` drops by); on a released campaign it equals `Recorded`. Lane K's setup
  fills the list with `WillBeRecorded` left false (no K board reads it).
- **`MsfScaleAggregateDto`** (exists, `MsfDtos.cs:41`) becomes `(double Average, int ResponseCount)`: `Distribution`
  goes (F2, check item 3). Its one constructor call (`MsfAggregationService.cs:72`) and two test fixtures
  (`CampaignReportActionsTests.cs:542`, `SubjectReadsOwnReportTests.cs:187`) are lane R's to mend.

## § 2: the DTO additions, each lane's own

- **Lane K, `MsfCampaignSummaryDto`** (exists, `MsfDtos.cs:17`): `public bool HasOpened { get; init; }` (the entity's
  `OpenedOn` is not null, `MsfCampaign.cs:55`), filled by `ListMsfCampaignsForCoordinator`. The list shows Responses "—"
  when false.
- **Lane K, `MsfCampaignSetupDto`** (exists, `GetMsfCampaignSetup.cs:29`), as `init` properties: `int MinimumResponses`,
  `int MinimumCategoryResponses`, `int MinimumRespondentCategories`, `IReadOnlyList<MsfCoveredEpaDto> CoveredEpas`,
  `int? TrainingYear` (`TraineeProfile.StageOn`, `TraineeProfile.cs:196`, on today's South African date), `bool
  HasOpened`, `DateTime? ClosedOn`, `bool SubjectIsCurrent`, `DateOnly? SubjectEndedOn` (from § 0). The page's
  `ShowsResponses` (`CampaignEdit.razor:573`) reads `HasOpened`, not the state (T270).
- **Lane T, `MsfReleasedReportDto`** (NEW, `MsfDtos.cs`): `(int CampaignId, string TemplateName, MsfTemplateKind Kind,
  DateTime ReleasedOn)`, newest first, released only. `ListMsfCampaignsForTraineeQuery` (exists,
  `ListMsfCampaignsForTrainee.cs`) returns `IReadOnlyList<MsfReleasedReportDto>` in place of `MsfCampaignSummaryDto`, so
  no count reaches her page (F3). Its only page caller is `MyMsfReports.razor:114`.

## § 3: constants, words and the migration

- **The group floor (F2), lane K:** `public const int MinimumCategoryResponsesFloor = 3;` (NEW, on `MsfCampaign`,
  `src/Wombat.Domain/MultiSourceFeedback/MsfCampaign.cs`; the entity's default stays 3, `:13`). Used by
  `CreateMsfCampaignCommandValidator` (`CreateMsfCampaign.cs:36`, today `GreaterThanOrEqualTo(1)`) and by the create
  form's `[Range(MsfCampaign.MinimumCategoryResponsesFloor, 100, ErrorMessage = "Enter a number from 3 to 100.")]`, for
  both kinds.
- **F5, lane R:** `public string? ClosedByUserId { get; set; }` (NEW, on `MsfCampaign`), and `Close(string?
  closedByUserId, DateTime utcNow)` in place of `Close(DateTime utcNow)` (`MsfCampaign.cs:140`); callers
  `CloseMsfCampaign.cs:40` (the caller's id) and `MsfCampaignAutoCloseJob.cs:180` (null). Stored by the one migration,
  `T369_MsfCampaignClosedBy` (NEW), lane R's alone.
- **The release mail, lane R:** `MsfReportReleasedEmail` (NEW, `src/Wombat.Application/Common/Email/Templates/`, a static
  `Build` as `EntrustmentDecisionRevokedEmail.cs:10`), sent through `AccountNotices.SendAsync` (exists,
  `AccountNotices.cs:25`) after the release's save, its link `AccountNotices.LinkTo("/msf/my-reports")`; that path equals
  `MsfLinks.MyReports` (a Web test says so). Not sent when `MsfSubjectStanding.IsCurrent` is false (F4).
- **Words already built and kept** (lanes use them, do not reword): `MsfCampaignText.Describe`, `WithdrawTitle`,
  `WithdrawConfirmLabel`, `WithdrawBody`, `Withdrawn`, `RowLinkName`, `RemoveInviteeTitle`, `RemoveInviteeBody`,
  `InviteeRemoved`, `LinksNotDelivered`, `ResendLinksLabel`, `LinksResent`, `LinksBeingSent`, `CampaignOpened`
  (`MsfCampaignText.cs:15–136`); `MsfRespondentCategories.Describe` (`MsfRespondentCategories.cs:13`);
  `BadgeFor.MsfCampaign` (`BadgeFor.cs:117`); `MsfCampaign.OnlyOpenCanBeClosed` (`MsfCampaign.cs:221`).

## Across lanes

- **Classes that exist and are reused as given:** `.required-mark` (`app.css:1561`), `.choice-group` (`:4698`),
  `.review-progress-link` (`:4223`), `.submit-check` (`:1634`), `.actions-cell` (`:442`), `.move-reasons` (`:1681`),
  `.error-summary` (`:1270`), `.clinic-table--stack-wide` (`:4112`), `.epa-lines` (`:3716`), `.scale-choices` (`:663`),
  `.alert-row` (`:1255`), `.dialog-body` (`:4575`), `.detail-card--empty-compact` (`:1430`), `.details-list` (`:971`).
- **Components used as built:** `ConfirmDialog` (its `CancelLabel`, `DangerAction`, `BodyContent`; no new parameter),
  `ActionResult` (`Id`, `FocusOnLoad`), `StatePanel` (`OnRetry`, `FocusAfterRetry`), `DataTable` (`Stack`, `Caption`),
  `RefusalSummary` (C4, C8), `Skeleton`, `PageHeader`, `FormField` (`Required`), `Alert`, `PageFocus.FocusByIdAsync`,
  `RowNames.Distinct`, and `./js/review-appeal.js`'s `count(textarea, counter, limit)` for the narrative's counter.
- **Dates and times:** ISO dates; a South African day from `ProgrammeCalendar.DateOf(utc)` (`ProgrammeCalendar.cs:23`)
  or `QuotaCalendar.Today(TimeProvider)` (`QuotaProgress.cs:16`); a moment "2026-10-14 16:34 SAST" from
  `ActivityMoments.When` (`ActivityPageModel.cs:910`). The release time reads the same on the report and her pages.
- **Titles:** "MSF campaigns"; "New MSF campaign"; "MSF campaign for {name}"; "MSF report for {name}"; "MSF reports";
  "{template}, released {yyyy-MM-dd}" (her page); "MSF coverage"; `/msf/respond`'s card h1 per state ("Feedback on
  {name}", "Thank you", "Feedback link already used", …). Each tab "{h1} · Wombat".
- **Owners:** `CampaignEdit`, `CampaignReport`, `ProgrammeCoverage` stay MSF campaigns for the Coordinator
  (`NavOwners.cs:130–132`); NEW `[typeof(MyMsfReport)] = [new([WombatRoles.Trainee], NavItems.MsfReports)]` (lane T);
  `MsfRespond` stays outside the rule (`NavOwners.cs:185`). No menu item changes (Q7).
- **No new role check.** Every page keeps its `[Authorize]`; `MyMsfReport` keeps `MyMsfReports.razor:3`'s roles and
  shows a report only when `IsSubjectsCopy` is true and it is Released.

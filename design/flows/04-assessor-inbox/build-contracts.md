# T350 step 6 — the contracts flow 04's lanes build on

Fixed before wave 1, so the lanes can run in parallel (T342 wrote its contracts after wave 1, from the code; `aa4cb848`).
Wave 1 builds these names exactly; a lane that must differ says so in its report, and the integrator corrects this file
to the code before wave 2 starts. Namespaces are under `Wombat.Application.Features.Activities` unless given; Web paths
under `src/Wombat.Web/Components/`. Who provides each is in brackets: A1 "waiting", A2 "words", B "lists", C "page",
D "form" (`build-lanes.md` § The lane plan).

## Reads [A1]
- **Waiting for you** (note 5; E5; Spec § 7).
  `Queries.ListWaitingForYou.ListWaitingForYouQuery(ClaimsPrincipal Principal) : IRequest<WaitingForYouDto>`.
  - `Dtos.WaitingForYouDto(IReadOnlyList<ActivitySummaryDto> Items, int OverdueCount, int DueDays)`, with
    `int Count => Items.Count` and `ActivitySummaryDto? Oldest => Items.FirstOrDefault()`.
  - `Items`: every activity a move of which, one that leads on, the caller may make now by the arms that are not the
    author's (`ActorArms.NotAuthor`), less the caller's own subject rows; oldest first by `UpdatedOn`, then `Id`. All of
    them: Home takes the first five, the inbox all, the way on the first after removing the one just moved, the
    other-role line the count, `OverdueCount` and `Oldest`.
  - `DueDays` is `DashboardThresholds.AssessorDueDays` (7), for the rule line's number (E1).
  - The shared reader: `Services.WaitingForYou.ReadAsync(IApplicationDbContext dbContext, IWorkflowEvaluator
    workflowEvaluator, IUserAdministrationService users, TimeProvider clock, DashboardThresholds thresholds,
    ClaimsPrincipal principal, CancellationToken cancellationToken) : Task<WaitingForYouDto>`. The query and the
    dashboard summary both call it, so Home and the inbox cannot disagree (T297).
  - `ListActivitiesByActorInboxQuery` is deleted; nothing sends it.
- **Decided by you** (Q1; note 6).
  `Queries.ListDecidedByYou.ListDecidedByYouQuery(ClaimsPrincipal Principal, int Page = 1, int PageSize = 20) :
  IRequest<ActivityListPageDto>`, flow 03's page shape (`Items`, `Page`, `PageSize`, `TotalCount`, `PageCount`).
  - An item is an activity the caller moved last (the latest transition by `OccurredOn`, ties to the later id) that is
    finished or has no move left in its pinned workflow; the caller's own subject rows left out; newest first by
    `DecidedOn`, then `Id`. Page and size clamped; a page past the end serves the last page.
  - Each item is an `ActivitySummaryDto`: `CurrentState`/`CurrentStateLabel` are the state the decision left it in,
    `CreditedItemCount` the latest credit (T108), `DisplayName`, `SubjectName`, and `DecidedOn`, `IsFinished` set.
  - The shared reader: `Services.DecidedByYou.ReadAsync(IApplicationDbContext dbContext, IUserAdministrationService
    users, ClaimsPrincipal principal, int page, int pageSize, CancellationToken cancellationToken) :
    Task<ActivityListPageDto>`.
- **Row fields.** `Dtos.ActivitySummaryDto` gains init members, left at their defaults by every other query:
  - `bool IsOverdue`: `now − UpdatedOn ≥ DueDays × 24 h`.
  - `int? WaitedDays`: `(int)(now − UpdatedOn).TotalDays`, whole days rounded down, as the nudge counts; 0 is under a
    day. Null outside the waiting read.
  - `DateTime? DecidedOn` (UTC) and `bool IsFinished`: the decided read's.
  - `DisplayName` ("Type · EPA · date") through `ActivityRowDetails.ResolveAsync` with `sharesTheRest: false`, and
    `SubjectName`, as flow 03 fills them. `UpdatedOn` is the "since" (note 10: the one clock).
- **Home.** `Dashboards.Assessor.GetAssessorDashboardSummaryQuery(ClaimsPrincipal Principal)` returns
  `AssessorDashboardSummaryDto(WaitingForYouDto Waiting, ActivityListPageDto Decisions)`: all the waiting rows, and the
  decided read's page 1 at size 5. `AwaitingReviewItem`, `RecentDecisionItem` and `PendingRequestCount` are gone.

## The waiting row [A1]
- **`Shared/Activities/WaitingList.razor`** (C6; R1): `[Parameter, EditorRequired] IReadOnlyList<ActivitySummaryDto>
  Items`, `[Parameter] bool WithSince` (the way on's "Waiting 8 days, since 2026-09-22 08:06 SAST."),
  `[Parameter] IReadOnlyDictionary<int, string>? Names`. Renders `ul.needs-you.stack-list`, each
  `li.needs-you-row` (plus `needs-you-row--overdue` when `IsOverdue`) holding `<ActivityLink FromSubject="true">`, then
  `span.needs-you-badges` (the state's `BadgeFor.ActivityState` badge, then `span.badge.badge-overdue` "Overdue" beside
  it, never in its place), then `p.needs-you-why`. The inbox's State cell uses the same `.needs-you-badges` span (the
  board's `.waiting-badges` is a slip).
- **`ActivityLink`** gains `[Parameter] public bool FromSubject { get; set; }`: the second line is
  `ActivityListWords.FromLine(item)`, "from <SubjectName>", in place of the nominee line (R2).
- **Names on a waiting list:** `ActivityRowNames.Waiting(IEnumerable<ActivitySummaryDto>)`: the link's words; two
  that read the same add ", <state label>", then ", waiting since <yyyy-MM-dd HH:mm> SAST", then "(1 of 2)", and only
  those carry an `aria-label` (C11; note 9). `ActivityRowNames.For`'s tie-breaker reads SAST (note 8).
- **`BadgeFor.Overdue`** is `"badge-overdue"`, defined in `app.css` and admitted by `DefinedClassTests` (note 14).
- **`Shared/Activities/WaitingWords`** (static; every phrase, one place):
  - `Waited(ActivitySummaryDto)`: "Waiting less than a day", "Waiting 1 day", "Waiting 8 days".
  - `WaitedCell(…)`: "Less than a day", "1 day", "8 days". `Since(…)`: "since 2026-09-22 08:55 SAST".
  - `Count(WaitingForYouDto)`: "2 waiting", "2 waiting, 1 overdue".
  - `RuleLine(int dueDays)`: "Oldest first. Overdue once it has waited 7 days." `DecidedRule`: "Newest first. Everything
    you completed, declined, discussed or signed off." `DecidedCount(int total)`: "1 decision", "45 decisions".
  - `More(int beyondFive)`: "1 more waits in the Activity inbox.", "20 more wait in the Activity inbox." (E5).
  - `MoreForYou(int left)`: "1 more waits for you.", "2 more wait for you.", "Nothing else waits for you."
  - `OtherRoleLine(WaitingForYouDto)` (E4; the name and registrar from `Oldest`, the wait lower-cased):
    - one, overdue: "1 activity waits for you in the Activity inbox, and it is overdue: <name>, from <registrar>,
      waiting 8 days."
    - several: "3 activities wait for you in the Activity inbox; 1 is overdue. The oldest: <name>, from <registrar>,
      waiting 8 days." (the "; n is/are overdue" clause only when any is)
    - one, not overdue: "1 activity waits for you in the Activity inbox: <name>, from <registrar>, waiting less than a
      day."
  - `OtherRoleAction(WaitingForYouDto)`: "Open it" for one, "Open the oldest" for several.

## Words [A2]
- `WorkflowTransition.LabelFor`: sentence case, "Complete", "Decline", "Return", "Record discussion", "Sign off". The
  server's refusal follows: "Decline requires a note.", "Return requires a note."
- `FilingWords.Running`: "Completing…", "Declining…", "Returning…", "Recording discussion…", "Signing off…"; the bar's
  hidden status is the label less its ellipsis, with a full stop ("Recording discussion.").
- `RefusalWords.NotDone`: "Not declined.", "Not returned.", "Not discussed.", "Not signed off.", "Not completed.".
  `RefusalWords.ForNote(ActivityActionDto action, string currentStateLabel) : string`: "Not declined. It is still
  Requested.", "Not returned. It is still Awaiting discussion.", "Not returned. It is still Awaiting review."
- `Shared/Activities/NotePanelWords` (static), `part` being `ActivityPageModel.AuthorPart`'s first word ("request",
  "reflection", "review"):
  - `Heading(ActivityActionDto action, string part)`: "Decline this request", "Return this reflection", "Return this
    review".
  - `Keep(string part)`: "Keep the request", "Keep the reflection", "Keep the review".
  - `Label(string subjectName)`: "Note for Sipho Ndlovu". `Send(ActivityActionDto action)`: "Decline with this note",
    "Return with this note". `Help(string subjectName)`: "Sipho Ndlovu reads it on the activity's page. It is kept with
    the activity's history."
  - The send is `btn-danger` only for a move that ends the activity (`TargetIsFinal`), else `btn-primary`.
- CSS primitives: `.only-wide` and `.only-narrow`, one shown each side of `max-width: 640.98px`; `.fold-show` and
  `.fold-hide`, one hidden per `details[open] > summary` (C9).

## The pages' fixed words and ids
- **Home [B]:** card titles "Waiting for you" (`h2#card-waiting`) and "Recent decisions" (`h2#card-decisions`); the
  waiting card's badge is `WaitingWords.Count`, absent when nothing waits; "Nothing is waiting for you.", "No decisions
  yet." (`p.card-empty`); overflow `p.waiting-more`; feet "Open Activity inbox" (`/activities/inbox`) and "All your
  decisions" (`/activities/inbox#decided-h`); the status "Loading your Home."
- **The other-role line [B]:** `section.alert.alert-warning|alert-info.other-role-line`, `aria-label="Waiting for you
  in the Activity inbox"`, its action a link to `/activities/{Oldest.Id}` with `aria-label="<action>: <name>, from
  <registrar>"`. Placed in `Home.razor` between `PageHeader` and the role's dashboard, as `<OtherRoleLine />`.
- **The inbox [B]:** subtitle "What waits for you to rate, review or discuss."; sections `h2#waiting-h` "Waiting for
  you" and `h2#decided-h` "Decided by you", both `tabindex="-1"`; columns Activity, EPA, State, Waiting (`td.epa-cell`,
  `td.waited-cell` with `<b>` and `<span>`, `data-label` on EPA and Waiting only) and Activity, Decision, Decided,
  Credit; "Inbox clear" / "Nothing is waiting for you."; "No decisions yet."; the status "Loading the Activity inbox.";
  `PagerControls` as built, `aria-disabled` at the ends.
- **The activity page [C]:** `#activity-result` (the result's sentence and `MoreForYou` only); `section.way-on`
  `aria-label="The next activity waiting for you"`, its buttons "Open the next" (`aria-label="Open the next: <name>,
  from <registrar>"`, primary) and "Back to <owner label>" / "Back to Home"; nothing left: `p.way-on-none` with "Go to
  Home". Note panel `#note-panel` (`aria-labelledby="note-panel-title"`), `#note-in`, `#note-help`, `#note-msg`,
  `#note-summary` (its link to `#note-in`). Discard's reason `li#why-discard` "Nothing to discard yet." in
  `ul.move-reasons.move-reasons--beside`. Activity unavailable: "Go to <owner label>" by `NavOwners.OwnerFor(typeof
  (ActivityView), acting role)`, else "Go to Home".
- **The form [D]:** `ActivityForm` gains `[Parameter] public bool FoldFilledSections { get; set; }`: the reader has a
  section to fill, so each filled read-only section also renders as the phone's fold. C passes it wherever
  `ActivityView` draws the form (and forwards it through `ActivityDetail`), D builds it; nothing else changes in the form's parameters.
  - The picker: `fieldset.rung-picker` `aria-describedby="<fieldKey>-help"`; radios `name="<fieldKey>"`, the first
    `id="<fieldKey>-in"` (flow 03's refusal link target), the rest `id="<fieldKey>-in-<order>"`; each
    `aria-describedby="rung-desc-<order>"`, a visually hidden span outside the `<details>`; "chosen" `span.rung-check`
    `aria-hidden="true"`. The stored value stays the rung's Order (flow 03: 5 is the rung labelled "4").
  - The twins: every id inside the `.only-narrow` copy (the rung legend, a folded section, About) is suffixed `-narrow`.

## CSS, by owner
Each lane defines its own classes under its own `/* Flow 04: … (T350) */` heading; a class another lane uses arrives
with the merge. [A1] `.badge-overdue`, `.needs-you-row--overdue`, `.needs-you-badges`. [A2] `.only-wide`,
`.only-narrow`, `.fold-show`, `.fold-hide`. [B] `.waiting-more`, `.card-empty`, `.decided-list`, `.decided-row`,
`.decided-meta`, `.decided-date`, `.waited-cell`, `.epa-cell`, `.other-role-line`. [C] `.way-on`, `.way-on-none`,
`.move-reasons--beside`. [D] `.rung-picker`, `.rung-choice`, `.rung-legend`, `.rung-legend-marker`, `.rung-legend-list`,
`.section-fold`, `.section-fold-head`, `.section-fold-show`, `.section-fold-owner`, `.section-fold-body`. The overdue
card uses the built `.detail-card--warning` in place of `--emphasis`, never both (nit T15).

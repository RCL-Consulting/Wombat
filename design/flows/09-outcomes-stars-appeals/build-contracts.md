# T366 build contracts (flow 09)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report.

## § 0: already on `t366`, written by the integrator

- `ReviewLinks.MyReviews` = `"/committee/my-reviews"`.
- `ReviewLinks.MyReview(int reviewId)` = `"/committee/my-reviews/{reviewId}"`: her review's page (F8). Lane T's page
  routes it; lane T's list links it; lane S's register never does (its Issued links the committee's address,
  `ReviewLinks.Review`, and only where `IssuingReviewReadable`).

## § 1: lane T provides, lane C mounts (wave 2)

- **`DecisionHistory`** (`Components/Pages/CommitteeDecisions/Review/DecisionHistory.razor`): the decision history,
  lifted from `DecisionForm`'s read branch (`DecisionForm.razor:142–185` at `01b4c1ed`), newest first.
  - `[Parameter, EditorRequired] CommitteeReviewDetailDto Review`.
  - `DecisionForm` renders it for its read branch, so the committee's page shows it with no change of lane C's.
  - Classes: `.decision-history`, `.decision-record`, `.decision-replaced`, `.decision-note`.
- **`AppealEntry`** (`Components/Pages/CommitteeDecisions/Review/AppealEntry.razor`): one appeal, the same on both pages.
  - `[Parameter, EditorRequired] CommitteeAppealDto Appeal`.
  - `[Parameter, EditorRequired] CommitteeReviewDetailDto Review` (the appeal body, for "Heard by …" while Open).
  - `[Parameter] bool ForTrainee`: her page heads it as hers; the committee's "Appeal by {LodgedByName}".
  - Its badge from `BadgeFor.Appeal(CommitteeAppealOutcome?)`: Open amber (`badge-accepted`), Dismissed and Remitted
    green (`badge-completed`).
  - "Show the whole reason" only when the clamped reason overflows, measured after render by the page's script module
    (S10); CSS carries `-webkit-line-clamp: 6` and `line-clamp: 6`.
  - The list wrapper is `ul.appeal-list`; each entry an `li.appeal-entry`.
- **`CommitteeAppealDto`** gains `public string? LodgedByName { get; init; }` and `public string? ResolvedByName { get;
  init; }`, filled by `GetCommitteeReviewById` for every reader (T308). Lane C reads them; it does not fill them.

## § 2: lane S changes `ConfirmDialog` (additive), lanes T and C use it as built

- `[Parameter] string? InitialFocusId`: the id of an element in `BodyContent` that takes the first focus in place of the
  safe button (the revoke's reason box). Null keeps today's behaviour.
- `[Parameter] Func<Task<bool>>? OnConfirmClosesWhen`: when set, the confirm button runs it in place of `OnConfirm`, and
  the dialog closes only when it returns true. A caller that refuses returns false and shows its refusal inside
  `BodyContent`, under the field, with what was typed kept (M4, M5, F3). Null keeps today's behaviour (close after
  `OnConfirm`).
- Lodge (lane T), Remit and Dismiss (lane C) set neither: their refusals land on the page. Keep editing and Go back
  return the focus to the opener, as the native `<dialog>` already does (F6); a lane that finds otherwise says so.

## Across lanes

- **Names are looked up once per page**, by the query that serves it (`UserDisplayNames`), never in Razor or a shared
  mapper. `CommitteeDecisionMappings.ToDetailDto` and `EntrustmentDecisionMappings.ToDto` stay lookup-free.
- **Dates and times:** ISO dates; a South African day from `ProgrammeCalendar.DateOf(utc)`; a time "hh:mm SAST" from
  `ActivityMoments.When`.
- **`EntrustmentDecisionDto`** is lane S's to extend (`IssuingReviewReadable`, the successor's id and `IssuedOn` for the
  superseded line); `RevokedByName` already exists (T367) and the register's query fills it too.
- **No new role check.** Every page keeps its policy and its handler's scope check; her review's page (lane T) is the
  one new read, own-trainee and Ratified-or-later in the handler (F8).

# T361 build contracts (flow 07)

The names two or more lanes share. Use them exactly; a lane that must differ says so in its report, and the integrator
corrects this file ("As built after wave 1") before wave 2.

## § A — lane A "no deferral" provides

- `CommitteeAgendaLineState`: `Due = 1`, `Decided = 3`, `NotDecided = 4`, `DecidedElsewhere = 5`. `Deferred = 2` is gone.
- `CommitteeDecision.UndecidedNote` (`string?`, trimmed, empty stored as null; length as Conditions).
- `RecordCommitteeDecisionCommand(int ReviewId, CommitteeDecisionCategory? Category, string Rationale, string? Conditions,
  string? UndecidedNote, IReadOnlyList<string> PresentUserIds, ClaimsPrincipal Principal)`: `UndecidedNote` is new,
  after `Conditions`, `[property: Redact]`.
- `CommitteeDecisionDto.UndecidedNote` (`string?`).
- `CommitteeReview.NothingStagedOnEntrustmentOnly` (the sentence, a `const string`): "This review decides entrustment
  only, so stage at least one decision before recording it." Record refuses with it (E4).
- On the agenda DTO: `RecordBlockedReason` (`string?`) carries **only** E4's sentence now (the quorum stays
  `CommitteeReviewDetailDto.PanelShortfall` plus the page's ticks). `BlocksRatify`, `OutstandingClosingLines`,
  `RatifyBlockedReason` are gone.
- On the agenda line DTO: `ReadsNotDecided` (`bool`): the line's state is NotDecided, or the review is Decided and the
  line is unstaged. The page prints "Not decided at this sitting" and the decision's `UndecidedNote` under it.
  `DeferralReason` and the Deferred state are gone; `IsClosing` stays.
- Decisions due: the status member formerly `Deferred` is `NotDecided` (label "Not decided"), with E3's rule.
- `CommitteeDecisionWording.ReviewTypeLabel(...)`: "Formative check-in" for a formative review (T309 item 2), and the DTO
  the review page reads carries it (name it in the report).
- The who-may-act sentence for a member during the sitting: "Only the panel's chair, Thandi Zulu, can stage entrustment
  decisions and record the committee's decision." (name its member in the report).

## § R — lane R "reads, return link, stacking" provides

- `CommitteeReviewDetailDto.TraineeProfileId` (`int?`): the trainee's preferred profile id while the trainee trains at
  the panel's institution; null otherwise.
- `CommitteeReviewDetailDto.TrainingYear` (`int?`): the training year at the end of the review's period (E6).
- `GetStartPreviewQuery(int ReviewId, ClaimsPrincipal Principal) : IRequest<StartPreviewDto?>`;
  `StartPreviewDto(int ItemCount, int ActivityCount, int CampaignReportCount, DateOnly WindowFrom, DateOnly WindowTo,
  int CampaignsLeftOut)`. Null when the caller may not read the review or it is not Scheduled.
- `StartCommitteeReview` refuses with `ChairCannotAct`'s sentence while the chair cannot act.
- `EpaSamplingConcentrationDto.EpaInForce`, `PendingEntrustmentDecisionDto.EpaInForce` (`bool`).
- Web `ReviewLinks`: `ReviewLinks.Review(int reviewId)` → `/committee/reviews/{id}`;
  `ReviewLinks.SnapshotLine(int reviewId, int snapshotItemId)` → `/committee/reviews/{id}#snap-{itemId}`;
  `ReviewLinks.ActivityFromReview(int activityId, int reviewId, int snapshotItemId)` →
  `/activities/{activityId}?from=review-{reviewId}&at=snap-{snapshotItemId}`. The review page gives each snapshot line
  `id="snap-{snapshotItemId}"`.

## § CSS, by owner

- Lane R: the `.sidebar` z-index rule; `html { scroll-padding-top }`; the `--sitting-bar-h` custom property (set by
  `js/sticky-offset.js`, observing `.sitting-bar:not(.sitting-bar--reader)`).
- Lane P: `.sitting-bar`, `.sitting-bar--reader`, `.sitting-bar-state`, `.sitting-bar-count`, `.sitting-bar-sentence`,
  `.sitting-bar-as-of`, `.sitting-bar-actions` (sticky, `top: 56px`, `z-index: 1` at 641 px and wider, chair's only);
  `.agenda-form`, `.agenda-form-row`, `.agenda-result-row`, `.agenda-form-standing`, `.agenda-line`,
  `.agenda-line.is-editing`; `.snapshot-pick`, `.snapshot-pick-line`; `.evidence-reader`; `.chart-jump`, `.chart-back`;
  `.quorum-line`; `scroll-margin-top` on the page's focus targets. Reuses `tr.is-editing`, `.btn-outline`,
  `.is-unavailable`.

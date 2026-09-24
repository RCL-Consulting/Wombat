using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record DecisionPanelMemberInput(string UserId, DecisionPanelMemberRole Role);

public sealed record DecisionPanelMemberDto(
    int Id,
    string UserId,
    DecisionPanelMemberRole Role);

public sealed record DecisionPanelSummaryDto(
    int Id,
    string Name,
    DecisionPanelScope Scope,
    int? InstitutionId,
    int? SpecialityId,
    int MemberCount);

public sealed record DecisionPanelDetailDto(
    int Id,
    string Name,
    DecisionPanelScope Scope,
    int? InstitutionId,
    int? SpecialityId,
    IReadOnlyList<DecisionPanelMemberDto> Members);

public sealed record CommitteeReviewListItemDto(
    int Id,
    string TraineeUserId,
    int PanelId,
    string PanelName,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    CommitteeReviewState State,
    CommitteeDecisionCategory? CurrentDecisionCategory,
    DateTime? RatifiedOn,
    bool IsFormative = false,
    CommitteeReviewType ReviewType = CommitteeReviewType.AnnualProgression)
{
    /// <summary>
    /// Whose review it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// schedule's Trainee column (T142). Filled by <c>ListReviewsForPanelQuery</c> in one lookup for the page. Null from
    /// the trainee's own list and the chair's, which do not show it, and from scheduling.
    /// </summary>
    public string? TraineeName { get; init; }
}

public sealed record CommitteeDecisionDto(
    int Id,
    CommitteeDecisionCategory Category,
    string Rationale,
    string? Conditions,
    DateTime DecidedOn,
    string DecidedByChairUserId,
    int? SupersedesDecisionId);

public sealed record CommitteeAppealDto(
    int Id,
    DateTime LodgedOn,
    string LodgedByUserId,
    string Reason,
    DateTime? ResolvedOn,
    string? ResolvedByUserId,
    CommitteeAppealOutcome? Outcome);

/// <summary>
/// One frozen line of a review's evidence snapshot. The parameters from <paramref name="EpaId" /> on are T167's and are
/// null on a line frozen before it, and on an MSF campaign's line, which reports across EPAs.
/// </summary>
/// <param name="EpaId">The EPA the activity is evidence for (its stamped <c>Activity.EpaId</c>), or null.</param>
/// <param name="EpaCode">That EPA's code when the review started.</param>
/// <param name="EpaTitle">That EPA's title when the review started.</param>
/// <param name="InstrumentKey">The type's <c>WbaToolKey</c>, or null when it declares none.</param>
/// <param name="InstrumentName">The instrument by name, else the activity type's name.</param>
/// <param name="IsRatedInstrument">
/// Whether the pinned version rates the trainee at all. False is an unrated instrument; true with no
/// <paramref name="RatingLabel" /> is a rating nobody recorded.
/// </param>
/// <param name="RatingOrder">The rung as the stored ordinal.</param>
/// <param name="RatingLabel">The rung as a clinician reads it ("3a"), else the bare ordinal.</param>
/// <param name="ObservedOn">When the encounter happened.</param>
/// <param name="ObservedOnDeclared">
/// False when nobody stated an encounter date and <paramref name="ObservedOn" /> is only the filing day (T161).
/// </param>
/// <param name="SourceState">The activity's workflow state, or the campaign's state, when the review started.</param>
/// <param name="SourceFinished">
/// Whether that was finished work: a terminal state of the activity's pinned workflow (D44), or a released campaign. Null
/// on a line frozen before T131. The page says when every item a staged decision names was unfinished; nothing refuses it.
/// </param>
public sealed record CommitteeEvidenceDto(
    int Id,
    CommitteeEvidenceSourceType SourceType,
    int? ActivityId,
    int? MsfCampaignId,
    int? SupervisorReportId,
    string SourceLabel,
    string Summary,
    DateTime? SourceRecordedOn,
    int? EpaId = null,
    string? EpaCode = null,
    string? EpaTitle = null,
    string? InstrumentKey = null,
    string? InstrumentName = null,
    bool? IsRatedInstrument = null,
    int? RatingOrder = null,
    string? RatingLabel = null,
    DateOnly? ObservedOn = null,
    bool? ObservedOnDeclared = null,
    string? SourceState = null,
    bool? SourceFinished = null)
{
    /// <summary>
    /// An activity line frozen before T167, which recorded none of the columns above: every activity line frozen since
    /// carries its encounter date. The page lists these as they were written rather than under an EPA they never named.
    /// </summary>
    public bool FrozenBeforeLinesNamedTheirEpa => SourceType == CommitteeEvidenceSourceType.Activity && ObservedOn is null;

    /// <summary>
    /// Whether a staged entrustment decision may name this line as evidence it rests on: the predicate the staging
    /// handler enforces, so the page's picker offers exactly those lines (D38, T131).
    /// </summary>
    public bool CanGroundADecision => Wombat.Application.Features.EntrustmentDecisions.StagedEvidence.CanGround(SourceType);
}

public sealed record CommitteeReviewDetailDto(
    int Id,
    string TraineeUserId,
    int PanelId,
    string PanelName,
    DateOnly ReviewPeriodFrom,
    DateOnly ReviewPeriodTo,
    DateOnly ScheduledOn,
    CommitteeReviewState State,
    DateTime? StartedOn,
    string? StartedByUserId,
    DateTime? RatifiedOn,
    string? RatifiedByUserId,
    DateTime? FinalizedOn,
    IReadOnlyList<CommitteeDecisionDto> Decisions,
    IReadOnlyList<CommitteeAppealDto> Appeals,
    IReadOnlyList<CommitteeEvidenceDto> EvidenceItems,
    bool IsFormative = false,
    CommitteeReviewType ReviewType = CommitteeReviewType.AnnualProgression)
{
    /// <summary>
    /// Whose review it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// review page's Trainee (T142). Filled by <c>GetCommitteeReviewByIdQuery</c>. Null from the commands that answer
    /// with the review: <c>CommitteeDecisionMappings.ToDetailDto</c> is shared with six commands and does no lookup, so
    /// the review page reads the review back through the query after each action.
    /// </summary>
    public string? TraineeName { get; init; }
}

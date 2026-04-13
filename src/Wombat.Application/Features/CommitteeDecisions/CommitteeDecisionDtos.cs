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
    DateTime? RatifiedOn);

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

public sealed record CommitteeEvidenceDto(
    int Id,
    CommitteeEvidenceSourceType SourceType,
    int? ActivityId,
    int? MsfCampaignId,
    int? SupervisorReportId,
    string SourceLabel,
    string Summary,
    DateTime? SourceRecordedOn);

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
    IReadOnlyList<CommitteeEvidenceDto> EvidenceItems);

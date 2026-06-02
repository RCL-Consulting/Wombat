using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

internal static class CommitteeDecisionMappings
{
    public static CommitteeReviewDetailDto ToDetailDto(this CommitteeReview review)
        => new(
            review.Id,
            review.TraineeUserId,
            review.PanelId,
            review.Panel.Name,
            review.ReviewPeriodFrom,
            review.ReviewPeriodTo,
            review.ScheduledOn,
            review.State,
            review.StartedOn,
            review.StartedByUserId,
            review.RatifiedOn,
            review.RatifiedByUserId,
            review.FinalizedOn,
            review.Decisions
                .OrderByDescending(decision => decision.DecidedOn)
                .ThenByDescending(decision => decision.Id)
                .Select(decision => new CommitteeDecisionDto(
                    decision.Id,
                    decision.Category,
                    decision.Rationale,
                    decision.Conditions,
                    decision.DecidedOn,
                    decision.DecidedByChairUserId,
                    decision.SupersedesDecisionId))
                .ToArray(),
            review.Appeals
                .OrderByDescending(appeal => appeal.LodgedOn)
                .ThenByDescending(appeal => appeal.Id)
                .Select(appeal => new CommitteeAppealDto(
                    appeal.Id,
                    appeal.LodgedOn,
                    appeal.LodgedByUserId,
                    appeal.Reason,
                    appeal.ResolvedOn,
                    appeal.ResolvedByUserId,
                    appeal.Outcome))
                .ToArray(),
            review.EvidenceItems
                .OrderBy(item => item.SourceType)
                .ThenBy(item => item.SourceRecordedOn)
                .ThenBy(item => item.Id)
                .Select(item => new CommitteeEvidenceDto(
                    item.Id,
                    item.SourceType,
                    item.ActivityId,
                    item.MsfCampaignId,
                    item.SupervisorReportId,
                    item.SourceLabel,
                    item.Summary,
                    item.SourceRecordedOn))
                .ToArray(),
            review.IsFormative,
            review.ReviewType);
}

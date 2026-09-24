using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

internal static class EntrustmentDecisionMappings
{
    public static EntrustmentDecisionDto ToDto(this EntrustmentDecision decision)
        => new(
            decision.Id,
            decision.TraineeUserId,
            decision.EpaId,
            decision.Epa?.Code ?? string.Empty,
            decision.Epa?.Title ?? string.Empty,
            decision.AuthorisedLevelId,
            decision.AuthorisedLevel?.Label ?? string.Empty,
            decision.AuthorisedLevel?.Order ?? 0,
            decision.IssuedOn,
            decision.ExpiresOn,
            decision.IssuedByCommitteeReviewId,
            decision.IssuedByChairUserId,
            decision.Rationale,
            decision.Status,
            decision.RevokedOn,
            decision.RevokedByUserId,
            decision.RevocationReason,
            decision.SupersededByDecisionId,
            decision.EvidenceLinks
                .OrderBy(link => link.SourceType)
                .ThenBy(link => link.SourceRecordedOn)
                .ThenBy(link => link.Id)
                .Select(link => new EntrustmentEvidenceLinkDto(
                    link.Id,
                    link.SourceType,
                    link.ActivityId,
                    link.MsfCampaignId,
                    link.CommitteeReviewId,
                    link.SourceLabel,
                    link.Summary,
                    link.SourceRecordedOn,
                    link.CommitteeEvidenceId))
                .ToArray());

    public static PendingEntrustmentDecisionDto ToDto(this PendingEntrustmentDecision pending)
        => new(
            pending.Id,
            pending.ReviewId,
            pending.EpaId,
            pending.Epa?.Code ?? string.Empty,
            pending.Epa?.Title ?? string.Empty,
            pending.AuthorisedLevelId,
            pending.AuthorisedLevel?.Label ?? string.Empty,
            pending.IssuedOn,
            pending.ExpiresOn,
            pending.Rationale,
            pending.EvidenceItemIds,
            pending.StagedOn,
            pending.StagedByUserId);
}

using System.Text.Json;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

internal static class EntrustmentDecisionMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
                    link.SourceRecordedOn))
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
            DeserializeEvidenceLinks(pending.EvidenceLinksJson),
            pending.StagedOn,
            pending.StagedByUserId);

    public static IReadOnlyList<EntrustmentEvidenceLinkInput> DeserializeEvidenceLinks(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<EntrustmentEvidenceLinkInput>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<EntrustmentEvidenceLinkInput>>(json, JsonOptions)
                ?? new List<EntrustmentEvidenceLinkInput>();
        }
        catch (JsonException)
        {
            return Array.Empty<EntrustmentEvidenceLinkInput>();
        }
    }

    public static string SerializeEvidenceLinks(IReadOnlyList<EntrustmentEvidenceLinkInput> links)
        => JsonSerializer.Serialize(links ?? Array.Empty<EntrustmentEvidenceLinkInput>(), JsonOptions);
}

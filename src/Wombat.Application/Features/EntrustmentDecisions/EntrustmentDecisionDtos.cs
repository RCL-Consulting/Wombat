using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record EntrustmentEvidenceLinkInput(
    EntrustmentEvidenceSourceType SourceType,
    int? ActivityId,
    int? MsfCampaignId,
    int? CommitteeReviewId,
    string SourceLabel,
    string Summary,
    DateTime? SourceRecordedOn);

public sealed record EntrustmentEvidenceLinkDto(
    int Id,
    EntrustmentEvidenceSourceType SourceType,
    int? ActivityId,
    int? MsfCampaignId,
    int? CommitteeReviewId,
    string SourceLabel,
    string Summary,
    DateTime? SourceRecordedOn);

public sealed record EntrustmentDecisionDto(
    int Id,
    string TraineeUserId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    int AuthorisedLevelId,
    string AuthorisedLevelLabel,
    int AuthorisedLevelOrder,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    int IssuedByCommitteeReviewId,
    string IssuedByChairUserId,
    string Rationale,
    EntrustmentDecisionStatus Status,
    DateTime? RevokedOn,
    string? RevokedByUserId,
    string? RevocationReason,
    int? SupersededByDecisionId,
    IReadOnlyList<EntrustmentEvidenceLinkDto> EvidenceLinks)
{
    /// <summary>
    /// Whose decision it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// admin list's Trainee column and its revoke confirmation (T142). Filled by
    /// <c>ListEntrustmentDecisionsForAdminQuery</c> in one lookup for the page. Null from every other producer:
    /// <c>EntrustmentDecisionMappings.ToDto</c> is shared by six handlers and does no lookup.
    /// </summary>
    public string? TraineeName { get; init; }
}

public sealed record PendingEntrustmentDecisionDto(
    int Id,
    int ReviewId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    int AuthorisedLevelId,
    string AuthorisedLevelLabel,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    string Rationale,
    IReadOnlyList<EntrustmentEvidenceLinkInput> EvidenceLinks,
    DateTime StagedOn,
    string StagedByUserId);

public sealed record StagePendingEntrustmentDecisionInput(
    int EpaId,
    int AuthorisedLevelId,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    string Rationale,
    IReadOnlyList<EntrustmentEvidenceLinkInput> EvidenceLinks);

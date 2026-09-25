using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>One item of evidence a STAR rests on, as the issuing review's snapshot line said it. (T131)</summary>
/// <param name="CommitteeEvidenceId">The snapshot row it was copied from; null only on a link written before T131.</param>
public sealed record EntrustmentEvidenceLinkDto(
    int Id,
    EntrustmentEvidenceSourceType SourceType,
    int? ActivityId,
    int? MsfCampaignId,
    int? CommitteeReviewId,
    string SourceLabel,
    string Summary,
    DateTime? SourceRecordedOn,
    int? CommitteeEvidenceId = null);

/// <param name="EpaInForce">
/// Whether the decision's EPA is in force now (T255, D48): <c>Epa.IsActive</c>, the flag the EPA picker labels by
/// (<see cref="Wombat.Application.Features.Epas.EpaOptionLabel" />), so the admin list and My authorisations mark
/// "(no longer in use)" on exactly the EPA My activities marks. Judged now, not at issue: a STAR on an EPA deactivated
/// since stays what it is, and the mark says the EPA it authorises is no longer in use.
/// </param>
public sealed record EntrustmentDecisionDto(
    int Id,
    string TraineeUserId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    bool EpaInForce,
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

/// <summary>An entrustment decision staged at a review, before ratification issues it.</summary>
/// <param name="EvidenceItemIds">
/// The ids of the review's snapshot lines (<c>CommitteeReviewDetailDto.EvidenceItems</c>) it rests on (D38, T131).
/// </param>
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
    IReadOnlyList<int> EvidenceItemIds,
    DateTime StagedOn,
    string StagedByUserId)
{
    /// <summary>
    /// Why ratifying would refuse this staged decision now, it no longer fitting the trainee's curriculum (T167), or null
    /// when it still fits. Filled by <c>ListPendingEntrustmentDecisionsForReviewQuery</c>: on a decided review this is the
    /// only staged decision the chair may still remove (T165).
    /// </summary>
    public string? NoLongerFits { get; init; }
}

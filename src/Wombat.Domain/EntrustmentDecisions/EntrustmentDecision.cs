using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Domain.EntrustmentDecisions;

public sealed class EntrustmentDecision
{
    private EntrustmentDecision()
    {
    }

    public int Id { get; private set; }
    public string TraineeUserId { get; private set; } = string.Empty;
    public int EpaId { get; private set; }
    public int AuthorisedLevelId { get; private set; }
    public DateOnly IssuedOn { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public int IssuedByCommitteeReviewId { get; private set; }
    public string IssuedByChairUserId { get; private set; } = string.Empty;
    public string Rationale { get; private set; } = string.Empty;
    public EntrustmentDecisionStatus Status { get; private set; } = EntrustmentDecisionStatus.Active;
    public DateTime? RevokedOn { get; private set; }
    public string? RevokedByUserId { get; private set; }
    public string? RevocationReason { get; private set; }
    public int? SupersededByDecisionId { get; private set; }
    public DateOnly? LastExpiryReminderSentOn { get; private set; }

    public Epa Epa { get; private set; } = null!;
    public EntrustmentLevel AuthorisedLevel { get; private set; } = null!;
    public CommitteeReview IssuedByCommitteeReview { get; private set; } = null!;

    private readonly List<EntrustmentEvidenceLink> _evidenceLinks = new();
    public IReadOnlyCollection<EntrustmentEvidenceLink> EvidenceLinks => _evidenceLinks;

    public static EntrustmentDecision Issue(
        string traineeUserId,
        int epaId,
        int authorisedLevelId,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        int committeeReviewId,
        string chairUserId,
        string rationale,
        IEnumerable<EntrustmentEvidenceLink> evidenceLinks)
    {
        if (string.IsNullOrWhiteSpace(traineeUserId))
        {
            throw new InvalidOperationException("A trainee user id is required to issue an entrustment decision.");
        }

        if (epaId <= 0)
        {
            throw new InvalidOperationException("A valid EPA is required to issue an entrustment decision.");
        }

        if (authorisedLevelId <= 0)
        {
            throw new InvalidOperationException("A valid entrustment level is required.");
        }

        if (committeeReviewId <= 0)
        {
            throw new InvalidOperationException("A valid committee review is required.");
        }

        if (string.IsNullOrWhiteSpace(chairUserId))
        {
            throw new InvalidOperationException("The issuing chair user is required.");
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            throw new InvalidOperationException("An entrustment decision rationale is required.");
        }

        if (expiresOn.HasValue && expiresOn.Value <= issuedOn)
        {
            throw new InvalidOperationException("An entrustment decision expiry date must be after the issue date.");
        }

        var decision = new EntrustmentDecision
        {
            TraineeUserId = traineeUserId.Trim(),
            EpaId = epaId,
            AuthorisedLevelId = authorisedLevelId,
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            IssuedByCommitteeReviewId = committeeReviewId,
            IssuedByChairUserId = chairUserId.Trim(),
            Rationale = rationale.Trim(),
            Status = EntrustmentDecisionStatus.Active
        };

        foreach (var link in evidenceLinks ?? Array.Empty<EntrustmentEvidenceLink>())
        {
            decision._evidenceLinks.Add(link);
        }

        return decision;
    }

    public void Revoke(string reason, string actorUserId, DateTime utcNow)
    {
        if (Status != EntrustmentDecisionStatus.Active)
        {
            throw new InvalidOperationException("Only active entrustment decisions can be revoked.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("A revocation reason is required.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The revoking user is required.");
        }

        Status = EntrustmentDecisionStatus.Revoked;
        RevokedOn = utcNow;
        RevokedByUserId = actorUserId.Trim();
        RevocationReason = reason.Trim();
    }

    public void MarkExpired(DateTime utcNow)
    {
        if (Status != EntrustmentDecisionStatus.Active)
        {
            throw new InvalidOperationException("Only active entrustment decisions can be marked expired.");
        }

        if (!ExpiresOn.HasValue)
        {
            throw new InvalidOperationException("An entrustment decision without an expiry date cannot expire.");
        }

        if (ExpiresOn.Value >= DateOnly.FromDateTime(utcNow))
        {
            throw new InvalidOperationException("An entrustment decision cannot be marked expired before its expiry date.");
        }

        Status = EntrustmentDecisionStatus.Expired;
    }

    public void SupersedeBy(int newDecisionId)
    {
        if (Status != EntrustmentDecisionStatus.Active)
        {
            throw new InvalidOperationException("Only active entrustment decisions can be superseded.");
        }

        if (newDecisionId <= 0)
        {
            throw new InvalidOperationException("A valid superseding decision id is required.");
        }

        Status = EntrustmentDecisionStatus.Superseded;
        SupersededByDecisionId = newDecisionId;
    }

    public void RecordExpiryReminderSent(DateOnly sentOn)
    {
        LastExpiryReminderSentOn = sentOn;
    }

    public void Amend()
        => throw new InvalidOperationException("Entrustment decisions are immutable. Revoke and reissue instead.");
}

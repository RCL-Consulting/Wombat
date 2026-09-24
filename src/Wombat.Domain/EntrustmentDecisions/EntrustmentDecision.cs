using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Domain.EntrustmentDecisions;

public sealed class EntrustmentDecision
{
    /// <summary>The refusal for a STAR that names no evidence (D38).</summary>
    public const string EvidenceRequired =
        "An entrustment decision must rest on at least one item of the committee's evidence snapshot.";

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

    /// <summary>
    /// The STAR that replaced this one on its EPA, set with <see cref="SupersededByDecisionId" /> by
    /// <see cref="SupersedeBy" />. A navigation, so that a ratification can issue the new STAR and supersede the old one
    /// in one save: the new STAR has no id until it is stored.
    /// </summary>
    public EntrustmentDecision? SupersededByDecision { get; private set; }

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

        // D38 (T131): the committee's decision draws on the assessment evidence, "never from a single form" and never
        // from none. Every link is built from a line of the issuing review's frozen snapshot.
        var links = (evidenceLinks ?? Array.Empty<EntrustmentEvidenceLink>()).ToList();
        if (links.Count == 0)
        {
            throw new InvalidOperationException(EvidenceRequired);
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

        decision._evidenceLinks.AddRange(links);

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

    /// <summary>
    /// Marks this STAR superseded by <paramref name="successor" />, which may not be stored yet: the foreign key follows
    /// the navigation when both are saved together.
    /// </summary>
    public void SupersedeBy(EntrustmentDecision successor)
    {
        ArgumentNullException.ThrowIfNull(successor);

        if (Status != EntrustmentDecisionStatus.Active)
        {
            throw new InvalidOperationException("Only active entrustment decisions can be superseded.");
        }

        if (ReferenceEquals(successor, this) || (successor.Id > 0 && successor.Id == Id))
        {
            throw new InvalidOperationException("An entrustment decision cannot supersede itself.");
        }

        if (successor.TraineeUserId != TraineeUserId || successor.EpaId != EpaId)
        {
            throw new InvalidOperationException("An entrustment decision is superseded only by one for the same trainee and EPA.");
        }

        Status = EntrustmentDecisionStatus.Superseded;
        SupersededByDecision = successor;
        if (successor.Id > 0)
        {
            SupersededByDecisionId = successor.Id;
        }
    }

    public void RecordExpiryReminderSent(DateOnly sentOn)
    {
        LastExpiryReminderSentOn = sentOn;
    }

    public void Amend()
        => throw new InvalidOperationException("Entrustment decisions are immutable. Revoke and reissue instead.");
}

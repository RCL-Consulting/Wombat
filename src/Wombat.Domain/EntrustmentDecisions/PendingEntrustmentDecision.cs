using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Domain.EntrustmentDecisions;

/// <summary>
/// Chair-staged entrustment decision attached to a committee review prior to ratification.
/// Cleared from the table on ratification — the ratify handler materialises each pending
/// row into an <see cref="EntrustmentDecision"/> atomically.
/// </summary>
public sealed class PendingEntrustmentDecision
{
    private PendingEntrustmentDecision()
    {
    }

    public int Id { get; private set; }
    public int ReviewId { get; private set; }
    public int EpaId { get; private set; }
    public int AuthorisedLevelId { get; private set; }
    public DateOnly IssuedOn { get; private set; }
    public DateOnly? ExpiresOn { get; private set; }
    public string Rationale { get; private set; } = string.Empty;
    public string EvidenceLinksJson { get; private set; } = "[]";
    public DateTime StagedOn { get; private set; }
    public string StagedByUserId { get; private set; } = string.Empty;

    public CommitteeReview Review { get; private set; } = null!;
    public Epa Epa { get; private set; } = null!;
    public EntrustmentLevel AuthorisedLevel { get; private set; } = null!;

    public static PendingEntrustmentDecision Stage(
        int reviewId,
        int epaId,
        int authorisedLevelId,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        string rationale,
        string evidenceLinksJson,
        string actorUserId,
        DateTime utcNow)
    {
        if (reviewId <= 0)
        {
            throw new InvalidOperationException("A valid committee review is required to stage a pending decision.");
        }

        if (epaId <= 0)
        {
            throw new InvalidOperationException("A valid EPA is required to stage a pending decision.");
        }

        if (authorisedLevelId <= 0)
        {
            throw new InvalidOperationException("A valid entrustment level is required to stage a pending decision.");
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            throw new InvalidOperationException("A rationale is required to stage a pending decision.");
        }

        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            throw new InvalidOperationException("The staging user is required.");
        }

        if (expiresOn.HasValue && expiresOn.Value <= issuedOn)
        {
            throw new InvalidOperationException("An expiry date must be after the issue date.");
        }

        return new PendingEntrustmentDecision
        {
            ReviewId = reviewId,
            EpaId = epaId,
            AuthorisedLevelId = authorisedLevelId,
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            Rationale = rationale.Trim(),
            EvidenceLinksJson = string.IsNullOrWhiteSpace(evidenceLinksJson) ? "[]" : evidenceLinksJson,
            StagedOn = utcNow,
            StagedByUserId = actorUserId.Trim()
        };
    }

    public void Update(
        int authorisedLevelId,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        string rationale,
        string evidenceLinksJson)
    {
        if (authorisedLevelId <= 0)
        {
            throw new InvalidOperationException("A valid entrustment level is required.");
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            throw new InvalidOperationException("A rationale is required.");
        }

        if (expiresOn.HasValue && expiresOn.Value <= issuedOn)
        {
            throw new InvalidOperationException("An expiry date must be after the issue date.");
        }

        AuthorisedLevelId = authorisedLevelId;
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        Rationale = rationale.Trim();
        EvidenceLinksJson = string.IsNullOrWhiteSpace(evidenceLinksJson) ? "[]" : evidenceLinksJson;
    }
}

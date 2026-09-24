using System.Text.Json;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Domain.EntrustmentDecisions;

/// <summary>
/// Chair-staged entrustment decision attached to a committee review prior to ratification.
/// Cleared from the table on ratification — the ratify handler materialises each pending
/// row into an <see cref="EntrustmentDecision"/> atomically.
/// </summary>
/// <remarks>
/// <para>
/// A staged decision names the lines of its review's frozen evidence snapshot it rests on
/// (<see cref="EvidenceItemIds" />), and names at least one (D38, T131). The College's own words: the decision is taken
/// by the Clinical Competency Committee "drawing on the standard assessment information sources ... never by a single
/// assessor and never from a single form". Before T131 every STAR staged through the page carried no evidence at all.
/// </para>
/// <para>
/// Ids only, never a copy of what the lines say. The lines are the review's snapshot, frozen when it started and never
/// rewritten, so the ratify handler builds each STAR's evidence links from the rows themselves
/// (<see cref="EntrustmentEvidenceLink.FromSnapshot" />). Until T131 the chair's own description of each link was
/// stored here as JSON and copied onto the STAR as typed, so a link could say anything.
/// </para>
/// <para>
/// A review holds at most one staged decision per EPA: the table is unique on (review, EPA), and staging a second is
/// refused by name. Ratifying issues one STAR per staged decision and supersedes the trainee's active one on its EPA, so
/// two on one EPA would leave the review's own two STARs to supersede each other in an order nobody chose.
/// </para>
/// </remarks>
public sealed class PendingEntrustmentDecision
{
    /// <summary>The refusal for a decision that names no evidence (D38).</summary>
    public const string EvidenceRequired =
        "An entrustment decision must name at least one item of the review's evidence snapshot it rests on.";

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

    /// <summary>
    /// The ids of the <see cref="CommitteeEvidence" /> lines of this review's snapshot the decision rests on, as a JSON
    /// array. Read it through <see cref="EvidenceItemIds" />.
    /// </summary>
    public string EvidenceItemIdsJson { get; private set; } = "[]";

    public DateTime StagedOn { get; private set; }
    public string StagedByUserId { get; private set; } = string.Empty;

    public CommitteeReview Review { get; private set; } = null!;
    public Epa Epa { get; private set; } = null!;
    public EntrustmentLevel AuthorisedLevel { get; private set; } = null!;

    /// <summary>
    /// The snapshot lines the decision rests on. Empty when the stored value is empty or unreadable, which the ratify
    /// handler refuses exactly as it refuses a decision that names nothing.
    /// </summary>
    public IReadOnlyList<int> EvidenceItemIds
    {
        get
        {
            if (string.IsNullOrWhiteSpace(EvidenceItemIdsJson))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<int[]>(EvidenceItemIdsJson) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }
    }

    public static PendingEntrustmentDecision Stage(
        int reviewId,
        int epaId,
        int authorisedLevelId,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        string rationale,
        IReadOnlyCollection<int> evidenceItemIds,
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

        var evidenceJson = SerializeEvidence(evidenceItemIds);

        return new PendingEntrustmentDecision
        {
            ReviewId = reviewId,
            EpaId = epaId,
            AuthorisedLevelId = authorisedLevelId,
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            Rationale = rationale.Trim(),
            EvidenceItemIdsJson = evidenceJson,
            StagedOn = utcNow,
            StagedByUserId = actorUserId.Trim()
        };
    }

    public void Update(
        int authorisedLevelId,
        DateOnly issuedOn,
        DateOnly? expiresOn,
        string rationale,
        IReadOnlyCollection<int> evidenceItemIds)
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

        // Validated before the first assignment, so a refusal leaves the tracked row as it was.
        var evidenceJson = SerializeEvidence(evidenceItemIds);

        AuthorisedLevelId = authorisedLevelId;
        IssuedOn = issuedOn;
        ExpiresOn = expiresOn;
        Rationale = rationale.Trim();
        EvidenceItemIdsJson = evidenceJson;
    }

    /// <summary>
    /// The ids as stored, after refusing an empty list (D38), an id that names no stored line, and an id named twice.
    /// Which review's snapshot each id belongs to is the staging handler's check: this entity holds no snapshot.
    /// </summary>
    private static string SerializeEvidence(IReadOnlyCollection<int>? evidenceItemIds)
    {
        if (evidenceItemIds is null || evidenceItemIds.Count == 0)
        {
            throw new InvalidOperationException(EvidenceRequired);
        }

        if (evidenceItemIds.Any(id => id <= 0))
        {
            throw new InvalidOperationException("Each named evidence item must be a stored line of the review's evidence snapshot.");
        }

        if (evidenceItemIds.Distinct().Count() != evidenceItemIds.Count)
        {
            throw new InvalidOperationException("Name each item of the evidence snapshot once.");
        }

        return JsonSerializer.Serialize(evidenceItemIds.ToArray());
    }
}

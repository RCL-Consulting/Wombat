using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// What a staged entrustment decision rests on: lines of its own review's frozen evidence snapshot, at least one, none
/// of them a supervisor report. The one rule staging enforces, ratifying enforces again, and the committee page's
/// evidence picker lists. (D38, T131)
/// </summary>
/// <remarks>
/// <para>
/// D38(a), as adopted: <i>"The summative entrustment decision for an EPA is taken by the Clinical Competency Committee,
/// drawing on the standard assessment information sources ... never by a single assessor and never from a single
/// form."</i> A decision therefore names at least one line of the snapshot the committee weighed. The line may be about
/// any EPA; that the named lines are about the decision's own EPA, or that there is more than one, is shown on the page
/// as a hint and never enforced (the design's Decision 8, operator question O1).
/// </para>
/// <para>
/// A supervisor report line cannot be named, because <see cref="EntrustmentEvidenceLink" /> holds an activity or an MSF
/// campaign and nothing else. The picker leaves those lines out (<see cref="CanGround" />), so it offers nothing the
/// gate refuses.
/// </para>
/// </remarks>
public static class StagedEvidence
{
    /// <summary>The refusal for a decision that names no evidence.</summary>
    public const string NoneNamed = "Name at least one item of the evidence snapshot.";

    /// <summary>
    /// The one refusal for a named id that is not a line of this review's snapshot, whether it names another review's
    /// line or nothing at all: the reply never says which, so it cannot be used to learn what other reviews hold.
    /// </summary>
    public const string NotInThisSnapshot =
        "Every item named must be a line of this review's evidence snapshot.";

    /// <summary>The refusal for a supervisor report line.</summary>
    public const string SupervisorReportNamed =
        "A supervisor report line cannot ground an entrustment decision. Name the activities or MSF campaigns it rests on.";

    /// <summary>Whether a snapshot line of this source may be named as the evidence a decision rests on.</summary>
    public static bool CanGround(CommitteeEvidenceSourceType sourceType)
        => sourceType is CommitteeEvidenceSourceType.Activity or CommitteeEvidenceSourceType.MsfCampaign;

    /// <summary>
    /// The lines of <paramref name="review" />'s snapshot these ids name, refusing an empty list, an id that is not a
    /// line of this snapshot, and a supervisor report line. Reads only; the review's
    /// <see cref="CommitteeReview.EvidenceItems" /> must be loaded.
    /// </summary>
    /// <exception cref="InvalidOperationException">The ids are refused.</exception>
    public static IReadOnlyList<CommitteeEvidence> Demand(CommitteeReview review, IReadOnlyCollection<int>? evidenceItemIds)
    {
        ArgumentNullException.ThrowIfNull(review);

        var refusal = RefusalFor(review, evidenceItemIds, out var lines);
        if (refusal is not null)
        {
            throw new InvalidOperationException(refusal);
        }

        return lines;
    }

    /// <summary>
    /// Refuses to ratify while any staged decision names no evidence, or names an id that is no longer a line of this
    /// review's snapshot a decision may rest on, naming each such decision by its EPA. Returns each staged decision's
    /// lines by its id. Reads only: the ratify handler runs it before its first mutation. The review's
    /// <see cref="CommitteeReview.EvidenceItems" /> must be loaded.
    /// </summary>
    /// <exception cref="InvalidOperationException">At least one staged decision rests on no evidence.</exception>
    public static async Task<IReadOnlyDictionary<int, IReadOnlyList<CommitteeEvidence>>> DemandGroundedAsync(
        IApplicationDbContext dbContext,
        CommitteeReview review,
        IReadOnlyCollection<PendingEntrustmentDecision> staged,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(review);
        ArgumentNullException.ThrowIfNull(staged);

        var grounded = new Dictionary<int, IReadOnlyList<CommitteeEvidence>>();
        var ungrounded = new List<int>();
        foreach (var decision in staged)
        {
            if (RefusalFor(review, decision.EvidenceItemIds, out var lines) is null)
            {
                grounded[decision.Id] = lines;
            }
            else
            {
                ungrounded.Add(decision.EpaId);
            }
        }

        if (ungrounded.Count == 0)
        {
            return grounded;
        }

        var epaIds = ungrounded.Distinct().ToArray();
        var codes = await dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, epa => epa.Code, cancellationToken);
        var named = ungrounded
            .Select(epaId => codes.TryGetValue(epaId, out var code) ? code : $"EPA {epaId}")
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();

        throw new InvalidOperationException(
            $"This review cannot be ratified: {named.Length} staged entrustment " +
            $"{(named.Length == 1 ? "decision names" : "decisions name")} no evidence from the review's snapshot " +
            $"({string.Join(", ", named)}). A decision is taken on the assessment evidence, never on none. " +
            UngroundedOnceRecorded);
    }

    /// <summary>
    /// What a chair can do about a staged decision that rests on no evidence once the committee's decision is recorded:
    /// nothing on the page. Only a decided review is ratified, and its staged decisions are fixed with the decision (D46),
    /// so it can be neither removed nor staged again; and staging never stores one, so it was written some other way.
    /// Said by the ratify refusal and by the review page beside the decision. Before T213 both told the chair to remove
    /// it and stage it again. (T213)
    /// </summary>
    public const string UngroundedOnceRecorded =
        "Staging never stores such a decision, and the staged decisions are fixed once the committee's decision is " +
        "recorded, so ask an administrator to look into it.";

    private static string? RefusalFor(
        CommitteeReview review,
        IReadOnlyCollection<int>? evidenceItemIds,
        out IReadOnlyList<CommitteeEvidence> lines)
    {
        lines = [];

        if (evidenceItemIds is null || evidenceItemIds.Count == 0)
        {
            return NoneNamed;
        }

        var snapshot = review.EvidenceItems.ToDictionary(line => line.Id);
        var resolved = new List<CommitteeEvidence>(evidenceItemIds.Count);
        foreach (var id in evidenceItemIds.Distinct())
        {
            if (!snapshot.TryGetValue(id, out var line))
            {
                return NotInThisSnapshot;
            }

            resolved.Add(line);
        }

        if (resolved.Any(line => !CanGround(line.SourceType)))
        {
            return SupervisorReportNamed;
        }

        lines = resolved;
        return null;
    }
}

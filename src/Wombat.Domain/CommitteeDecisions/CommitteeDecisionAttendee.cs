namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// A panel member recorded as present when one committee decision was taken. (T165)
/// </summary>
/// <remarks>
/// <para>
/// The College's summative entrustment decision is a committee's, "never by a single assessor". Before T165 the record
/// held only the chair who recorded and the chair who ratified, so nothing could show that anyone else took part. Whoever
/// records a decision records who sat, at the review or at an appeal that remits it; ratifying requires that the current
/// decision's attendance holds a quorum (<see cref="CommitteeReview.QuorumShortfall" />).
/// </para>
/// <para>
/// It belongs to the decision, not to the review. A review can hold more than one decision (an appeal that is remitted
/// adds a replacement), and each is taken by whoever sat for it; attendance kept on the review would credit a later
/// decision to an earlier sitting.
/// </para>
/// <para>
/// A snapshot, like the evidence: <see cref="Role" /> is the member's role on the panel at the sitting, and neither
/// column follows a later change to the panel's members. There is no foreign key to <see cref="DecisionPanelMember" />
/// for the same reason; a member removed from the panel afterwards was still there when the decision was taken.
/// </para>
/// </remarks>
public sealed class CommitteeDecisionAttendee
{
    public int Id { get; set; }
    public int DecisionId { get; set; }
    public string UserId { get; set; } = string.Empty;

    /// <summary>The member's role on the panel when the decision was recorded.</summary>
    public DecisionPanelMemberRole Role { get; set; }

    public CommitteeDecision Decision { get; set; } = null!;
}

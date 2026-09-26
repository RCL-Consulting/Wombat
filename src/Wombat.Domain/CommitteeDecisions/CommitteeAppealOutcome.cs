namespace Wombat.Domain.CommitteeDecisions;

/// <summary>
/// What the appeal body did with an appeal, and so what became of the appealed decision (T307, D51). Either one closes the
/// review for good.
/// </summary>
/// <remarks>
/// There were three until T307. The third, <c>Upheld</c> (1), did exactly what <see cref="Dismissed" /> does, leaving the
/// appealed decision in force, under a name that says the appeal succeeded. An appeal that succeeds replaces the decision,
/// which is <see cref="Remitted" />. The T307 migration rewrote stored 1s as <see cref="Dismissed" />, what the engine had
/// done with them, and <c>CK_CommitteeAppeals_Outcome</c> refuses 1 from then on. The values keep their numbers.
/// </remarks>
public enum CommitteeAppealOutcome
{
    /// <summary>The appealed decision stands.</summary>
    Dismissed = 2,

    /// <summary>
    /// The appeal body replaces the appealed decision with its own, taken by a quorate sitting it records (T165), with a
    /// category on a progression review and none on an entrustment-only one (T131 slice 5), and its own conditions.
    /// </summary>
    Remitted = 3
}

using Wombat.Domain.Curricula;

namespace Wombat.Domain.CommitteeDecisions;

public sealed class DecisionPanel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DecisionPanelScope Scope { get; set; }

    /// <summary>
    /// The institution that runs the panel, whatever its scope: a Speciality-scoped panel still sits at one hospital,
    /// because the speciality it covers is a national catalogue entry (T091). Required (T182): a panel reviews only
    /// trainees at its own institution, so a panel without one could review nobody.
    /// </summary>
    public int InstitutionId { get; set; }

    public int? SpecialityId { get; set; }

    /// <summary>
    /// The <see cref="Curricula.DecisionBody" /> this panel sits as, by key, or null for a general panel (T131, Decision
    /// 2). A panel carrying <c>neonatal</c> is the institution's neonatal Clinical Competency Committee: it takes the
    /// decisions on the EPAs whose curriculum item names that body (EPAs 4 and 5), and a general panel takes them only
    /// where no such panel covers the trainee.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The EPA's tag is national and this mapping is local, so committee structure stays a per-institution arrangement;
    /// if the College answers that it is national (§ 3F question 6), only this column moves. Which panel an item goes to
    /// is <c>DecisionRouting</c>'s one predicate.
    /// </para>
    /// <para>
    /// A restricting foreign key to the national vocabulary, and unique per (institution, speciality, body) with a null
    /// speciality counted as one value: an institution has at most one institution-wide panel sitting as a body, and at
    /// most one per speciality, so an item routes to exactly one panel where any carries its body. Only an
    /// InstitutionalAdmin or a global Administrator sets it: a SpecialityAdmin can create a panel for their own
    /// speciality, and the tag would let them take that body's EPAs from the institution's own panels.
    /// </para>
    /// </remarks>
    public string? DecisionBodyKey { get; set; }

    public DateTime CreatedOn { get; set; }

    public DecisionBody? DecisionBody { get; set; }
    public ICollection<DecisionPanelMember> Members { get; set; } = [];
    public ICollection<CommitteeReview> Reviews { get; set; } = [];
}

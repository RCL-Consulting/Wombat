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
    public DateTime CreatedOn { get; set; }

    public ICollection<DecisionPanelMember> Members { get; set; } = [];
    public ICollection<CommitteeReview> Reviews { get; set; } = [];
}

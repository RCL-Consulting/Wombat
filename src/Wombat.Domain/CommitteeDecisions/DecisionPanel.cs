namespace Wombat.Domain.CommitteeDecisions;

public sealed class DecisionPanel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DecisionPanelScope Scope { get; set; }
    public int? InstitutionId { get; set; }
    public int? SpecialityId { get; set; }
    public DateTime CreatedOn { get; set; }

    public ICollection<DecisionPanelMember> Members { get; set; } = [];
    public ICollection<CommitteeReview> Reviews { get; set; } = [];
}

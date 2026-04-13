namespace Wombat.Domain.CommitteeDecisions;

public sealed class DecisionPanelMember
{
    public int Id { get; set; }
    public int PanelId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DecisionPanelMemberRole Role { get; set; }

    public DecisionPanel Panel { get; set; } = null!;
}

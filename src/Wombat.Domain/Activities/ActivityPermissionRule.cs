namespace Wombat.Domain.Activities;

public sealed class ActivityPermissionRule
{
    public int Id { get; set; }
    public int ActivityTypeId { get; set; }
    public string TransitionKey { get; set; } = string.Empty;
    public string ActorRuleJson { get; set; } = string.Empty;
    public string? FieldRequirementJson { get; set; }

    public ActivityType ActivityType { get; set; } = null!;
}

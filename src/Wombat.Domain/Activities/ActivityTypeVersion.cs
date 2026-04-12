namespace Wombat.Domain.Activities;

public sealed class ActivityTypeVersion
{
    public int Id { get; set; }
    public int ActivityTypeId { get; set; }
    public int Version { get; set; }
    public string SchemaJson { get; set; } = string.Empty;
    public string WorkflowJson { get; set; } = string.Empty;
    public string CreditRulesJson { get; set; } = string.Empty;
    public string DisplayFieldsJson { get; set; } = "[]";
    public string PublishedByUserId { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }

    public ActivityType ActivityType { get; set; } = null!;
}

using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Activities;

public sealed class ActivityType
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityScope Scope { get; set; }
    public int? ScopeId { get; set; }
    public string SchemaJson { get; set; } = string.Empty;
    public string WorkflowJson { get; set; } = string.Empty;
    public string CreditRulesJson { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string OwnerUserId { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public ICollection<ActivityPermissionRule> PermissionRules { get; set; } = [];
    public ICollection<Activity> Activities { get; set; } = [];

    public void PublishNewVersion(string newSchemaJson, string newWorkflowJson, string newCreditRulesJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newSchemaJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(newWorkflowJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(newCreditRulesJson);

        SchemaJson = FormSchemaParser.Serialize(FormSchemaParser.Parse(newSchemaJson));
        WorkflowJson = WorkflowParser.Serialize(WorkflowParser.Parse(newWorkflowJson));
        CreditRulesJson = CreditRulesParser.Serialize(CreditRulesParser.Parse(newCreditRulesJson));
        Version++;
    }
}

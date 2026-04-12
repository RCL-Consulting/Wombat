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
    public string? SchemaJson { get; set; }
    public string? WorkflowJson { get; set; }
    public string? CreditRulesJson { get; set; }
    public string DisplayFieldsJson { get; set; } = "[]";
    public string? StagingSchemaJson { get; set; }
    public string? StagingWorkflowJson { get; set; }
    public string? StagingCreditRulesJson { get; set; }
    public string? StagingDisplayFieldsJson { get; set; }
    public string? StagingUpdatedByUserId { get; set; }
    public DateTime? StagingUpdatedOn { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; } = true;
    public string OwnerUserId { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public ICollection<ActivityPermissionRule> PermissionRules { get; set; } = [];
    public ICollection<Activity> Activities { get; set; } = [];
    public ICollection<ActivityTypeVersion> Versions { get; set; } = [];

    public bool HasDraft =>
        !string.IsNullOrWhiteSpace(StagingSchemaJson) &&
        !string.IsNullOrWhiteSpace(StagingWorkflowJson) &&
        !string.IsNullOrWhiteSpace(StagingCreditRulesJson);

    public void SaveDraft(
        string schemaJson,
        string workflowJson,
        string creditRulesJson,
        string displayFieldsJson,
        string actorUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(workflowJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(creditRulesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayFieldsJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);

        StagingSchemaJson = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));
        StagingWorkflowJson = WorkflowParser.Serialize(WorkflowParser.Parse(workflowJson));
        StagingCreditRulesJson = CreditRulesParser.Serialize(CreditRulesParser.Parse(creditRulesJson));
        StagingDisplayFieldsJson = NormalizeDisplayFieldsJson(displayFieldsJson);
        StagingUpdatedByUserId = actorUserId.Trim();
        StagingUpdatedOn = DateTime.UtcNow;
    }

    public ActivityTypeVersion PublishDraft(string actorUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);

        if (!HasDraft)
        {
            throw new InvalidOperationException("A saved draft is required before publishing.");
        }

        SchemaJson = StagingSchemaJson;
        WorkflowJson = StagingWorkflowJson;
        CreditRulesJson = StagingCreditRulesJson;
        DisplayFieldsJson = StagingDisplayFieldsJson ?? "[]";
        Version++;

        var publishedVersion = new ActivityTypeVersion
        {
            ActivityTypeId = Id,
            Version = Version,
            SchemaJson = SchemaJson!,
            WorkflowJson = WorkflowJson!,
            CreditRulesJson = CreditRulesJson!,
            DisplayFieldsJson = DisplayFieldsJson,
            PublishedByUserId = actorUserId.Trim(),
            PublishedOn = DateTime.UtcNow
        };

        Versions.Add(publishedVersion);
        DiscardDraft();

        return publishedVersion;
    }

    public void DiscardDraft()
    {
        StagingSchemaJson = null;
        StagingWorkflowJson = null;
        StagingCreditRulesJson = null;
        StagingDisplayFieldsJson = null;
        StagingUpdatedByUserId = null;
        StagingUpdatedOn = null;
    }

    private static string NormalizeDisplayFieldsJson(string displayFieldsJson)
    {
        using var document = System.Text.Json.JsonDocument.Parse(displayFieldsJson);
        if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            throw new InvalidOperationException("Display fields must be a JSON array.");
        }

        var values = document.RootElement.EnumerateArray()
            .Select(element =>
            {
                if (element.ValueKind != System.Text.Json.JsonValueKind.String)
                {
                    throw new InvalidOperationException("Display field entries must be strings.");
                }

                var value = element.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("Display field entries must not be empty.");
                }

                return value;
            })
            .Distinct(StringComparer.Ordinal)
            .Take(3)
            .ToArray();

        return System.Text.Json.JsonSerializer.Serialize(values);
    }
}

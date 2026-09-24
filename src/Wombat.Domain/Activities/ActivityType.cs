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

    /// <summary>
    /// Which College-named workplace-based assessment instrument this type is, as a
    /// <see cref="Wombat.Domain.Epas.WbaTool" /> key, or null when it is not a recognised instrument (T122).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A curriculum item's <c>PermittedToolsJson</c> is checked against this key when an activity of this type is
    /// filed and submitted. Null is unrestricted (D21): a type nobody has declared an instrument credits any EPA
    /// on the subject's curriculum, exactly as before T122. That is true of every builder-made type until an
    /// administrator chooses an instrument for it, and of the seeds that are not instruments at all.
    /// </para>
    /// <para>
    /// <b>Live and unversioned, like <see cref="Name" /> and <see cref="Scope" />.</b> It is not staged, not copied
    /// into <see cref="ActivityTypeVersion" />, and never touched by the seed refresher. The builder writes it on
    /// SAVE, not on publish, and discarding the draft does not undo it. Because the builder's only save path saves a
    /// draft, changing it on a SEEDED type parks a draft, and the seed refresher skips a type with a draft in flight
    /// until it is published or discarded. For an UNCHANGED credit target, changing it changes what the author of a draft
    /// may still hand on while nobody else has acted; it changes nothing once someone else has acted, and nothing for a
    /// completed activity, because credit never re-checks (D20). A CHANGED target is always checked against the current
    /// key.
    /// </para>
    /// </remarks>
    public string? WbaToolKey { get; set; }

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

        // Every payload is parsed before any of them is assigned (T122). A save that assigned the schema and then
        // threw on the workflow left a tracked entity half-written, and the audit pipeline's catch saves the
        // request's DbContext, so the half-written draft was COMMITTED under a failed command.
        var schema = FormSchemaParser.Parse(schemaJson);
        var workflow = WorkflowParser.Parse(workflowJson);

        // T102: the nominee fields must be unambiguous before anything is assigned. See ActorFieldRules.
        ActorFieldRules.EnsurePublishable(schema, workflow);

        var stagingSchemaJson = FormSchemaParser.Serialize(schema);
        var stagingWorkflowJson = WorkflowParser.Serialize(workflow);
        var stagingCreditRulesJson = CreditRulesParser.Serialize(CreditRulesParser.Parse(creditRulesJson));
        var stagingDisplayFieldsJson = NormalizeDisplayFieldsJson(displayFieldsJson);

        StagingSchemaJson = stagingSchemaJson;
        StagingWorkflowJson = stagingWorkflowJson;
        StagingCreditRulesJson = stagingCreditRulesJson;
        StagingDisplayFieldsJson = stagingDisplayFieldsJson;
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

        // T102: also here, because a draft staged before the rule existed never met it, and a publish is the step that
        // makes it what new activities pin to. Nothing is assigned until it passes.
        ActorFieldRules.EnsurePublishable(
            FormSchemaParser.Parse(StagingSchemaJson!),
            WorkflowParser.Parse(StagingWorkflowJson!));

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

    /// <summary>
    /// Canonicalises a display-fields array the way <see cref="SaveDraft"/> stores it.
    /// </summary>
    /// <remarks>
    /// Public because the seed refresher (T103) has to compare a stored value against a desired one
    /// in the same form. <c>DisplayFieldsJson</c> is a <c>jsonb</c> column, so the bytes PostgreSQL
    /// hands back are its own rendering, never the bytes that were written — only a canonical-to-
    /// canonical comparison is stable across a round trip through the database.
    /// </remarks>
    public static string NormalizeDisplayFieldsJson(string displayFieldsJson)
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

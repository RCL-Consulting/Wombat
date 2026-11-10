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
    /// may still hand on while nobody else has acted; it changes nothing once someone else has acted, and nothing about a
    /// completed activity's credit, because credit never re-checks (D20). A CHANGED target is always checked against the
    /// current key.
    /// </para>
    /// <para>
    /// <b>It also names the type's evidence category</b> (T144): the source label on the trajectory and what the
    /// committee sampling report counts as one source. That reading is live too, so changing the key, from the save
    /// onward and whether or not the draft is then discarded, relabels every activity of the type, completed ones
    /// included, and can move a sampling report's source count. See <c>RatedActivityTypes.Classify</c>.
    /// </para>
    /// </remarks>
    public string? WbaToolKey { get; set; }

    /// <summary>
    /// Whether only the system writes activities of this type (T162). <c>msf_cpsa</c> is the one such type: its rows are
    /// written by a released multi-source feedback campaign, one per EPA the campaign covers, and nobody files one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A system-managed type is left out of the type picker (<c>ListActivityTypesQuery</c>) and refused by
    /// <c>ActivityService.CreateDraftAsync</c>, the one path a person creates an activity through, before anything is
    /// written. The system's own path, <c>ActivityService.StageCompletedAsync</c>, does not read it. Before T162 a trainee
    /// could file an <c>msf_cpsa</c> draft that nobody could ever write or record, so it sat in their list for good.
    /// </para>
    /// <para>
    /// <b>Owned by the seed catalogue, not the builder.</b> The seeders write it from the type's
    /// <c>ActivityTypeSeedEntry</c> on create, and the T162 migration stamped it on existing databases. The builder
    /// neither shows nor writes it, so a builder-made type is never system-managed and a save through the builder keeps
    /// whatever the type already had. Like <see cref="WbaToolKey" />, it is live and unversioned.
    /// </para>
    /// </remarks>
    public bool SystemManaged { get; set; }

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
        // T398 (P2): all three are parsed and both cross-checks run whatever the others say, and every problem is
        // refused at once, so a draft wrong on two tabs names both on one save.
        var (schema, workflow, creditRules) = ParseAll(schemaJson, workflowJson, creditRulesJson);

        var stagingSchemaJson = FormSchemaParser.Serialize(schema);
        var stagingWorkflowJson = WorkflowParser.Serialize(workflow);
        var stagingCreditRulesJson = CreditRulesParser.Serialize(creditRules);
        var stagingDisplayFieldsJson = NormalizeDisplayFieldsJson(displayFieldsJson);

        StagingSchemaJson = stagingSchemaJson;
        StagingWorkflowJson = stagingWorkflowJson;
        StagingCreditRulesJson = stagingCreditRulesJson;
        StagingDisplayFieldsJson = stagingDisplayFieldsJson;
        StagingUpdatedByUserId = actorUserId.Trim();
        StagingUpdatedOn = DateTime.UtcNow;
    }

    /// <summary>
    /// The saved draft's schema, after all three payloads are parsed and cross-checked as <see cref="PublishDraft" /> will
    /// judge them, assigning nothing: every problem at once, as the aggregate (P2). For a publish that asks a question of
    /// the parsed schema first (a scale it binds), so a draft wrong on a tab is refused in the same words as at
    /// <see cref="PublishDraft" />, never as a bare parse exception (T398, the fix pass, code review N-3).
    /// </summary>
    public FormSchema CheckDraft()
    {
        if (!HasDraft)
        {
            throw new InvalidOperationException("A saved draft is required before publishing.");
        }

        return ParseAll(StagingSchemaJson!, StagingWorkflowJson!, StagingCreditRulesJson!).Schema;
    }

    public ActivityTypeVersion PublishDraft(string actorUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);

        if (!HasDraft)
        {
            throw new InvalidOperationException("A saved draft is required before publishing.");
        }

        // T102 and T137: also here, because a draft staged before either rule existed never met it, and a publish is the
        // step that makes it what new activities pin to. Nothing is assigned until it passes; every problem at once (P2).
        ParseAll(StagingSchemaJson!, StagingWorkflowJson!, StagingCreditRulesJson!);

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

    /// <summary>
    /// The three payloads parsed, after the two checks across them (T102's <see cref="ActorFieldRules" />, T137's
    /// <see cref="EvidenceEpa" />); or every problem any of the five found, at once, as one
    /// <see cref="ActivityTypeDraftRefusedException" /> (T398, flow 17, P2: "every tab at once", the operator's
    /// 2026-11-10).
    /// </summary>
    /// <remarks>
    /// A cross-check runs only where the payloads it reads parsed: a schema that does not parse cannot be judged against
    /// the workflow. Before T398 the first payload to fail stopped the save, so a draft wrong in its form and its
    /// workflow named only the form, and the workflow's refusal appeared on the next save. The parsers themselves still
    /// throw their own types; only the draft's refusal is the aggregate, its message their sentences joined.
    /// </remarks>
    private static (FormSchema Schema, Workflow.Workflow Workflow, CreditRules CreditRules) ParseAll(
        string schemaJson,
        string workflowJson,
        string creditRulesJson)
    {
        var places = new List<RefusalPlace>();

        var schema = Collect(() => FormSchemaParser.Parse(schemaJson), places);
        var workflow = Collect(() => WorkflowParser.Parse(workflowJson), places);
        var creditRules = Collect(() => CreditRulesParser.Parse(creditRulesJson), places);

        if (schema is not null && workflow is not null)
        {
            places.AddRange(ActorFieldRules.PublishProblems(schema, workflow));
        }

        if (schema is not null && creditRules is not null)
        {
            places.AddRange(EvidenceEpa.CreditProblems(schema, creditRules));
        }

        if (places.Count > 0)
        {
            throw new ActivityTypeDraftRefusedException(places);
        }

        return (schema!, workflow!, creditRules!);
    }

    private static T? Collect<T>(Func<T> parse, List<RefusalPlace> places)
        where T : class
    {
        try
        {
            return parse();
        }
        catch (Exception exception) when (exception is IActivityTypeRefusal refusal)
        {
            places.AddRange(refusal.Places);
            return null;
        }
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

using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T102. <see cref="ActivityType.SaveDraft" /> and <see cref="ActivityType.PublishDraft" /> both run
/// <see cref="ActorFieldRules.EnsurePublishable" />, and both run it before they assign anything.
/// </summary>
/// <remarks>
/// <para>
/// Both, because a draft staged before T102 never met the rule, and publishing is the step that makes a schema what new
/// activities pin to. Before assigning, because of the audit trap: <c>AuditPipelineBehavior</c> saves the request's
/// DbContext from its catch, so a refused save or publish that had already written a staging or published column would
/// COMMIT that write under a failed command. The snapshot comparison below is what would show it.
/// </para>
/// <para>
/// The refusals use the shapes the rule exists for, each on its own: a <c>field:</c> rule naming no field (the
/// scenario-paediatrics title-only shortcut), one naming a text field (the escalation itself), a duplicated key, and
/// a user field with its own options.
/// </para>
/// </remarks>
public sealed class ActivityTypePublishCheckTests
{
    private const string Actor = "builder-admin";
    private const string DisplayFieldsJson = "[\"title\"]";

    private const string CreditRulesJson = """{ "counts_for": [] }""";

    private const string NomineeSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "title", "type": "text", "label": "Title", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall", "type": "number", "label": "Overall" }
              ]
            }
          ]
        }
        """;

    private const string NomineeWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "request", "from": "draft", "to": "requested", "actor": "subject" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    /// <summary>The Mini-CEX workflow beside a schema with only a title: every rule names a field that is not there.</summary>
    private const string TitleOnlySchemaJson = """
        { "version": 1, "sections": [ { "key": "main", "title": "Main", "fields": [ { "key": "title", "type": "text", "label": "Title" } ] } ] }
        """;

    /// <summary>Each unpublishable (schema, workflow) pair, with a fragment the refusal must contain.</summary>
    public static TheoryData<string, string, string> UnpublishablePairs() => new()
    {
        { TitleOnlySchemaJson, NomineeWorkflowJson, "the form has no field 'assessor_user_id'" },
        {
            NomineeSchemaJson.Replace("\"type\": \"user\"", "\"type\": \"text\"", StringComparison.Ordinal),
            NomineeWorkflowJson,
            "'assessor_user_id' is not a User field"
        },
        {
            NomineeSchemaJson.Replace(
                "{ \"key\": \"overall\", \"type\": \"number\", \"label\": \"Overall\" }",
                "{ \"key\": \"overall\", \"type\": \"number\", \"label\": \"Overall\" }, { \"key\": \"title\", \"type\": \"text\", \"label\": \"Title again\" }",
                StringComparison.Ordinal),
            NomineeWorkflowJson,
            "Field key 'title' is used by 2 fields"
        },
        {
            NomineeSchemaJson.Replace(
                "\"label\": \"Assessor\", \"required\": true",
                "\"label\": \"Assessor\", \"required\": true, \"options\": [\"user-1\"]",
                StringComparison.Ordinal),
            NomineeWorkflowJson,
            "User field 'assessor_user_id' declares its own options"
        }
    };

    // ---- SaveDraft ----------------------------------------------------------------------------

    /// <summary>
    /// The fixtures differ from the valid pair only in the one respect under test, so this control is what shows each
    /// refusal is about that respect and not something else in the fixture.
    /// </summary>
    [Fact]
    public void SaveDraft_AValidNomineeType_IsStaged()
    {
        var type = PublishedType();

        type.SaveDraft(NomineeSchemaJson, NomineeWorkflowJson, CreditRulesJson, DisplayFieldsJson, Actor);

        Assert.True(type.HasDraft);
        Assert.Equal(FormSchemaParser.Serialize(FormSchemaParser.Parse(NomineeSchemaJson)), type.StagingSchemaJson);
        Assert.Equal(WorkflowParser.Serialize(WorkflowParser.Parse(NomineeWorkflowJson)), type.StagingWorkflowJson);
    }

    [Theory]
    [MemberData(nameof(UnpublishablePairs))]
    public void SaveDraft_AnUnpublishableType_IsRefusedAndLeavesAnExistingDraftExactlyAsItWas(
        string schemaJson,
        string workflowJson,
        string expectedMessage)
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        var exception = Assert.Throws<SchemaParseException>(
            () => type.SaveDraft(schemaJson, workflowJson, CreditRulesJson, "[\"overall\"]", "someone-else"));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, Snapshot.Of(type));
    }

    /// <summary>
    /// A first save that is refused leaves the type with no draft at all — not an orphaned staging schema the builder
    /// would load beside nothing.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnpublishablePairs))]
    public void SaveDraft_AnUnpublishableType_IsRefusedAndLeavesEveryStagingColumnNull(
        string schemaJson,
        string workflowJson,
        string expectedMessage)
    {
        var type = PublishedType();
        var before = Snapshot.Of(type);

        var exception = Assert.Throws<SchemaParseException>(
            () => type.SaveDraft(schemaJson, workflowJson, CreditRulesJson, DisplayFieldsJson, Actor));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
        Assert.False(type.HasDraft);
        Assert.Null(type.StagingSchemaJson);
        Assert.Null(type.StagingWorkflowJson);
        Assert.Null(type.StagingCreditRulesJson);
        Assert.Null(type.StagingDisplayFieldsJson);
        Assert.Null(type.StagingUpdatedByUserId);
        Assert.Null(type.StagingUpdatedOn);
        Assert.Equal(before, Snapshot.Of(type));
    }

    // ---- PublishDraft -------------------------------------------------------------------------

    /// <summary>
    /// A draft staged before T102 — here written straight onto the staging columns, as a pre-T102 save would have
    /// left it — never met the rule. Publishing it is refused, and nothing moves: no published column, no version
    /// bump, no version row, and the draft stays so the author can repair it.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnpublishablePairs))]
    public void PublishDraft_ADraftStagedBeforeTheRule_IsRefusedAndChangesNothing(
        string schemaJson,
        string workflowJson,
        string expectedMessage)
    {
        var type = PublishedType();
        StageWithoutTheCheck(type, schemaJson, workflowJson);
        var before = Snapshot.Of(type);

        var exception = Assert.Throws<SchemaParseException>(() => type.PublishDraft("someone-else"));

        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, Snapshot.Of(type));
        Assert.True(type.HasDraft);
    }

    /// <summary>
    /// The check judges what is being published, not what it replaces. A type whose published version predates the
    /// rule (and fails it) can be repaired: its in-flight activities stay pinned to the old version, and new ones pin
    /// to the repair.
    /// </summary>
    [Fact]
    public void PublishDraft_AValidDraftOverAPublishedVersionThatFailsTheRule_IsPublished()
    {
        var type = PublishedType();
        type.SchemaJson = FormSchemaParser.Serialize(FormSchemaParser.Parse(TitleOnlySchemaJson));
        type.WorkflowJson = WorkflowParser.Serialize(WorkflowParser.Parse(NomineeWorkflowJson));
        var versionBefore = type.Version;

        type.SaveDraft(NomineeSchemaJson, NomineeWorkflowJson, CreditRulesJson, DisplayFieldsJson, Actor);
        var published = type.PublishDraft(Actor);

        Assert.Equal(versionBefore + 1, type.Version);
        Assert.Equal(type.Version, published.Version);
        Assert.Equal(FormSchemaParser.Serialize(FormSchemaParser.Parse(NomineeSchemaJson)), type.SchemaJson);
        Assert.False(type.HasDraft);
    }

    /// <summary>A refused publish does not poison the entity: the repaired draft publishes.</summary>
    [Fact]
    public void PublishDraft_AfterARefusal_ARepairedDraftPublishes()
    {
        var type = PublishedType();
        StageWithoutTheCheck(type, TitleOnlySchemaJson, NomineeWorkflowJson);
        var versionBefore = type.Version;

        Assert.Throws<SchemaParseException>(() => type.PublishDraft(Actor));

        type.SaveDraft(NomineeSchemaJson, NomineeWorkflowJson, CreditRulesJson, DisplayFieldsJson, Actor);
        type.PublishDraft(Actor);

        Assert.Equal(versionBefore + 1, type.Version);
        Assert.Equal(versionBefore + 1, type.Versions.Max(version => version.Version));
    }

    // ---- fixtures -----------------------------------------------------------------------------

    private static ActivityType PublishedType()
    {
        var type = new ActivityType
        {
            Id = 11,
            Key = "publish_check_probe",
            Name = "Publish check probe",
            Scope = ActivityScope.Global,
            OwnerUserId = Actor,
            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        type.SaveDraft(
            ActivityTestData.ValidSchemaJson,
            ActivityTestData.ValidWorkflowJson,
            ActivityTestData.ValidCreditRulesJson,
            DisplayFieldsJson,
            Actor);
        type.PublishDraft(Actor);

        Assert.Equal(1, type.Version);
        return type;
    }

    private static ActivityType TypeWithExistingDraft()
    {
        var type = PublishedType();
        type.SaveDraft(NomineeSchemaJson, NomineeWorkflowJson, CreditRulesJson, DisplayFieldsJson, Actor);

        // Pinned so a rewrite by the refused save would be visible even within one clock tick.
        type.StagingUpdatedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

        Assert.True(type.HasDraft);
        return type;
    }

    /// <summary>What a pre-T102 SaveDraft stored: the canonical payloads, never checked against the rule.</summary>
    private static void StageWithoutTheCheck(ActivityType type, string schemaJson, string workflowJson)
    {
        type.StagingSchemaJson = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));
        type.StagingWorkflowJson = WorkflowParser.Serialize(WorkflowParser.Parse(workflowJson));
        type.StagingCreditRulesJson = """{"counts_for":[]}""";
        type.StagingDisplayFieldsJson = "[\"title\"]";
        type.StagingUpdatedByUserId = "pre-t102-author";
        type.StagingUpdatedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
    }

    private sealed record Snapshot(
        string? StagingSchemaJson,
        string? StagingWorkflowJson,
        string? StagingCreditRulesJson,
        string? StagingDisplayFieldsJson,
        string? StagingUpdatedByUserId,
        DateTime? StagingUpdatedOn,
        string? SchemaJson,
        string? WorkflowJson,
        string? CreditRulesJson,
        string DisplayFieldsJson,
        int Version,
        int VersionRows)
    {
        public static Snapshot Of(ActivityType type) => new(
            type.StagingSchemaJson,
            type.StagingWorkflowJson,
            type.StagingCreditRulesJson,
            type.StagingDisplayFieldsJson,
            type.StagingUpdatedByUserId,
            type.StagingUpdatedOn,
            type.SchemaJson,
            type.WorkflowJson,
            type.CreditRulesJson,
            type.DisplayFieldsJson,
            type.Version,
            type.Versions.Count);
    }
}

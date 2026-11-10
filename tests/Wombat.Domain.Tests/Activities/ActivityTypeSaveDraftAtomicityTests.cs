using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T122. <see cref="ActivityType.SaveDraft" /> either writes every staging column or none of them.
/// </summary>
/// <remarks>
/// <para>
/// The audit trap: <c>AuditWriter</c> shares the request's DbContext, and <c>AuditPipelineBehavior</c> saves it from its
/// catch. So a handler that mutates a tracked entity and then throws COMMITS the mutation under a failed command.
/// </para>
/// <para>
/// Before T122, <c>SaveDraft</c> assigned <c>StagingSchemaJson</c> and only then parsed the workflow. A builder save with
/// a good schema and a broken workflow therefore left the draft half-written: the new form beside the old workflow and
/// credit rules, a combination nobody authored, which the next publish would have shipped. The fix parses all four
/// payloads into locals first and assigns at the end. These tests pin that for each payload that can fail, on a type that
/// already holds a draft (where a partial write is visible) and on one that does not.
/// </para>
/// </remarks>
public sealed class ActivityTypeSaveDraftAtomicityTests
{
    private const string Actor = "builder-admin";
    private const string ValidDisplayFieldsJson = "[\"title\"]";

    /// <summary>
    /// A valid schema that canonicalises to something DIFFERENT from <see cref="ActivityTestData.ValidSchemaJson" />.
    /// The failing saves send this one, so a schema assignment that ran before the failure would show.
    /// </summary>
    private const string ReplacementSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "encounter",
              "title": "Encounter",
              "fields": [
                { "key": "title", "type": "text", "label": "Title", "required": true },
                { "key": "setting", "type": "text", "label": "Setting", "required": false }
              ]
            }
          ]
        }
        """;

    private const string ReplacementWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "complete", "from": "draft", "to": "completed", "actor": "subject" }
          ]
        }
        """;

    private const string ReplacementCreditRulesJson = """{ "counts_for": [] }""";

    /// <summary>Malformed JSON, and well-formed JSON the parser rejects (an undeclared initial state).</summary>
    public static TheoryData<string> MalformedWorkflows() => new()
    {
        "{ \"version\": 1, \"initial_state\": ",
        """
        {
          "version": 1,
          "initial_state": "nowhere",
          "states": [ { "key": "draft", "label": "Draft" } ],
          "transitions": []
        }
        """
    };

    /// <summary>
    /// The case the fix exists for: an existing draft, a valid replacement schema, and a workflow that does not parse.
    /// Every staging column, including who saved it and when, must be exactly what it was.
    /// </summary>
    [Theory]
    [MemberData(nameof(MalformedWorkflows))]
    public void AMalformedWorkflowLeavesAnExistingDraftExactlyAsItWas(string malformedWorkflowJson)
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            ReplacementSchemaJson,
            malformedWorkflowJson,
            ReplacementCreditRulesJson,
            "[\"setting\"]",
            "someone-else"));

        Assert.Equal(before, Snapshot.Of(type));
        Assert.True(type.HasDraft);
    }

    /// <summary>The third payload failing must not leave the first two written.</summary>
    [Fact]
    public void MalformedCreditRulesLeaveAnExistingDraftExactlyAsItWas()
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            ReplacementSchemaJson,
            ReplacementWorkflowJson,
            "{ \"counts_for\": ",
            "[\"setting\"]",
            "someone-else"));

        Assert.Equal(before, Snapshot.Of(type));
    }

    /// <summary>The last payload failing must not leave the first three written.</summary>
    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("[1]")]
    [InlineData("[\"  \"]")]
    public void MalformedDisplayFieldsLeaveAnExistingDraftExactlyAsItWas(string malformedDisplayFieldsJson)
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        Assert.ThrowsAny<Exception>(() => type.SaveDraft(
            ReplacementSchemaJson,
            ReplacementWorkflowJson,
            ReplacementCreditRulesJson,
            malformedDisplayFieldsJson,
            "someone-else"));

        Assert.Equal(before, Snapshot.Of(type));
    }

    /// <summary>A malformed schema, the first payload, fails before anything could have been written.</summary>
    [Fact]
    public void AMalformedSchemaLeavesAnExistingDraftExactlyAsItWas()
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            "{ \"version\": 1, \"sections\": [] }",
            ReplacementWorkflowJson,
            ReplacementCreditRulesJson,
            "[\"setting\"]",
            "someone-else"));

        Assert.Equal(before, Snapshot.Of(type));
    }

    /// <summary>
    /// A type with no draft stays without one. A half-written first save would set <c>StagingSchemaJson</c> alone, which
    /// <see cref="ActivityType.HasDraft" /> reports as no draft, but the builder would still load the orphaned schema.
    /// </summary>
    [Fact]
    public void AMalformedWorkflowOnATypeWithNoDraftLeavesEveryStagingColumnNull()
    {
        var type = PublishedType();

        Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            ReplacementSchemaJson,
            "{ \"version\": 1, \"initial_state\": ",
            ReplacementCreditRulesJson,
            ValidDisplayFieldsJson,
            Actor));

        Assert.False(type.HasDraft);
        Assert.Equal(new Snapshot(null, null, null, null, null, null, type.SchemaJson, type.WorkflowJson, type.CreditRulesJson, type.DisplayFieldsJson, type.Version), Snapshot.Of(type));
    }

    /// <summary>
    /// The control. The same replacement payloads, all valid, DO change every staging column. Without this the
    /// atomicity tests above could pass vacuously if the replacement canonicalised to the stored value.
    /// </summary>
    [Fact]
    public void AValidReplacementChangesEveryStagedPayload()
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        type.SaveDraft(
            ReplacementSchemaJson,
            ReplacementWorkflowJson,
            ReplacementCreditRulesJson,
            "[\"setting\"]",
            "someone-else");

        Assert.NotEqual(before.StagingSchemaJson, type.StagingSchemaJson);
        Assert.NotEqual(before.StagingWorkflowJson, type.StagingWorkflowJson);
        Assert.NotEqual(before.StagingCreditRulesJson, type.StagingCreditRulesJson);
        Assert.NotEqual(before.StagingDisplayFieldsJson, type.StagingDisplayFieldsJson);
        Assert.Equal("someone-else", type.StagingUpdatedByUserId);
        Assert.NotEqual(before.StagingUpdatedOn, type.StagingUpdatedOn);

        // SaveDraft never touches the published columns; only PublishDraft does.
        Assert.Equal(before.SchemaJson, type.SchemaJson);
        Assert.Equal(before.WorkflowJson, type.WorkflowJson);
        Assert.Equal(before.CreditRulesJson, type.CreditRulesJson);
        Assert.Equal(before.DisplayFieldsJson, type.DisplayFieldsJson);
        Assert.Equal(before.Version, type.Version);
    }

    /// <summary>
    /// A failed save does not poison the entity: the next valid save goes through and writes all four payloads.
    /// </summary>
    [Fact]
    public void AValidSaveAfterAFailedOneWritesEverything()
    {
        var type = TypeWithExistingDraft();

        Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            ReplacementSchemaJson, "not json", ReplacementCreditRulesJson, "[\"setting\"]", "someone-else"));

        type.SaveDraft(ReplacementSchemaJson, ReplacementWorkflowJson, ReplacementCreditRulesJson, "[\"setting\"]", "someone-else");

        Assert.Equal(FormSchemaParser.Serialize(FormSchemaParser.Parse(ReplacementSchemaJson)), type.StagingSchemaJson);
        Assert.Equal(WorkflowParser.Serialize(WorkflowParser.Parse(ReplacementWorkflowJson)), type.StagingWorkflowJson);
        Assert.Equal(CreditRulesParser.Serialize(CreditRulesParser.Parse(ReplacementCreditRulesJson)), type.StagingCreditRulesJson);
        Assert.Equal("[\"setting\"]", type.StagingDisplayFieldsJson);
        Assert.Equal("someone-else", type.StagingUpdatedByUserId);
    }

    // ---- T398 (flow 17, P2: "every tab at once") -------------------------------------------------

    /// <summary>
    /// A draft wrong on its Form and its Workflow names both on one save: one refusal, every place, its message their
    /// sentences joined, and nothing assigned. Before T398 the schema's refusal stopped the save and the workflow's
    /// appeared on the next.
    /// </summary>
    [Fact]
    public void ADraftWrongOnTwoTabs_IsRefusedOnceNamingBoth_AndAssignsNothing()
    {
        var type = TypeWithExistingDraft();
        var before = Snapshot.Of(type);

        var refused = Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            EmptySectionSchemaJson, BadActorWorkflowJson, ReplacementCreditRulesJson, "[\"setting\"]", "someone-else"));

        Assert.Equal([RefusalTab.Form, RefusalTab.Workflow], refused.Places.Select(place => place.Tab));
        Assert.Equal((RefusalKind.EmptySection, "more"), (refused.Places[0].Kind, refused.Places[0].SectionKey));
        Assert.Equal("cancel", refused.Places[1].MoveKey);
        Assert.Equal(
            "Section 'more' must contain at least one field. Actor rule grammar supports only 'subject', 'creator', " +
            "'role:<name>', 'scope:<name>', 'field:<name>', '+' and '|'.",
            refused.Message);
        Assert.Equal(before, Snapshot.Of(type));
    }

    /// <summary>
    /// Both cross-checks run once their payloads parse, whatever the other found: a <c>field:</c> rule naming a text field
    /// (Workflow) and credit reading another EPA field (Credit) are two places on one save.
    /// </summary>
    [Fact]
    public void BothCrossChecks_AreReportedTogether()
    {
        var type = PublishedType();

        var refused = Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            ActivityTestData.ValidSchemaJson,
            ActivityTestData.ValidWorkflowJson.Replace("\"role:Assessor\"", "\"field:title\"", StringComparison.Ordinal),
            ActivityTestData.ValidCreditRulesJson.Replace("\"epa_field\": \"epa_id\"", "\"epa_field\": \"score\"", StringComparison.Ordinal),
            ValidDisplayFieldsJson,
            "someone-else"));

        Assert.Equal(
            [(RefusalTab.Workflow, RefusalKind.FieldRuleNotUserField), (RefusalTab.Credit, RefusalKind.CreditEpaFieldDisagrees)],
            refused.Places.Select(place => (place.Tab, place.Kind)));
        Assert.Equal("complete", refused.Places[0].MoveKey);
        Assert.Equal(1, refused.Places[1].DirectiveNumber);
        Assert.False(type.HasDraft);
    }

    /// <summary>
    /// A cross-check runs only where the payloads it reads parsed: a schema that does not parse is one place, and the
    /// workflow's <c>field:</c> rule is not judged against a form nobody can read.
    /// </summary>
    [Fact]
    public void ACrossCheck_IsNotRunAgainstAPayloadThatDidNotParse()
    {
        var type = PublishedType();

        var refused = Assert.Throws<ActivityTypeDraftRefusedException>(() => type.SaveDraft(
            "{ \"version\": 1,",
            ActivityTestData.ValidWorkflowJson.Replace("\"role:Assessor\"", "\"field:ghost\"", StringComparison.Ordinal),
            ActivityTestData.ValidCreditRulesJson,
            ValidDisplayFieldsJson,
            "someone-else"));

        var place = Assert.Single(refused.Places);
        Assert.Equal((RefusalTab.Form, RefusalKind.MalformedJson, 1L), (place.Tab, place.Kind, place.JsonLine));
    }

    /// <summary>PublishDraft refuses every problem of a staged draft at once too (P2), changing nothing.</summary>
    [Fact]
    public void PublishDraft_ADraftStagedWrongOnTwoTabs_NamesBoth()
    {
        var type = PublishedType();
        type.StagingSchemaJson = EmptySectionSchemaJson;
        type.StagingWorkflowJson = BadActorWorkflowJson;
        type.StagingCreditRulesJson = ReplacementCreditRulesJson;
        type.StagingDisplayFieldsJson = "[]";
        var before = Snapshot.Of(type);

        var refused = Assert.Throws<ActivityTypeDraftRefusedException>(() => type.PublishDraft(Actor));

        Assert.Equal(2, refused.Places.Count);
        Assert.Equal(before, Snapshot.Of(type));
    }

    /// <summary>
    /// The fix pass (N-3): CheckDraft judges a staged draft as PublishDraft will, every place at once, and assigns nothing;
    /// a draft that passes gives back its schema for the publish's scale question.
    /// </summary>
    [Fact]
    public void CheckDraft_RefusesAsPublishDraftDoes_AndAssignsNothing()
    {
        var type = PublishedType();
        type.StagingSchemaJson = EmptySectionSchemaJson;
        type.StagingWorkflowJson = BadActorWorkflowJson;
        type.StagingCreditRulesJson = ReplacementCreditRulesJson;
        type.StagingDisplayFieldsJson = "[]";
        var before = Snapshot.Of(type);

        var refused = Assert.Throws<ActivityTypeDraftRefusedException>(() => type.CheckDraft());

        Assert.Equal(2, refused.Places.Count);
        Assert.Equal(before, Snapshot.Of(type));

        var good = TypeWithExistingDraft();
        Assert.NotEmpty(good.CheckDraft().Sections);
    }

    private const string EmptySectionSchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "encounter", "title": "Encounter", "fields": [ { "key": "title", "type": "text", "label": "Title" } ] },
            { "key": "more", "title": "More", "fields": [] }
          ]
        }
        """;

    private const string BadActorWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "done", "actor": "subject" },
            { "key": "cancel", "from": "draft", "to": "done", "actor": "nobody" }
          ]
        }
        """;

    private static ActivityType PublishedType()
    {
        var type = new ActivityType
        {
            Id = 7,
            Key = "atomicity_probe",
            Name = "Atomicity probe",
            Scope = ActivityScope.Global,
            OwnerUserId = Actor,
            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            WbaToolKey = "mini_cex"
        };

        type.SaveDraft(
            ActivityTestData.ValidSchemaJson,
            ActivityTestData.ValidWorkflowJson,
            ActivityTestData.ValidCreditRulesJson,
            ValidDisplayFieldsJson,
            Actor);
        type.PublishDraft(Actor);

        return type;
    }

    private static ActivityType TypeWithExistingDraft()
    {
        var type = PublishedType();

        type.SaveDraft(
            ActivityTestData.ValidSchemaJson,
            ActivityTestData.ValidWorkflowJson,
            ActivityTestData.ValidCreditRulesJson,
            ValidDisplayFieldsJson,
            Actor);

        // Pinned so a rewrite by the failing save would be visible even within one clock tick.
        type.StagingUpdatedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);

        Assert.True(type.HasDraft);
        return type;
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
        int Version)
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
            type.Version);
    }
}

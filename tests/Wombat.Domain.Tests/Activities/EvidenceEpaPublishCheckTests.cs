using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T137. The EPA an activity is stamped with and the EPA its credit lands on are one field:
/// <see cref="ActivityType.SaveDraft" /> and <see cref="ActivityType.PublishDraft" /> both refuse a type whose credit reads
/// its EPA from anywhere but the schema's <c>evidence_epa_field</c>, and both refuse before they assign anything.
/// </summary>
/// <remarks>
/// Before anything is assigned because of the audit trap: <c>AuditPipelineBehavior</c> saves the request's DbContext from
/// its catch, so a refused save that had already written a staging column would commit it under a failed command. The
/// snapshot comparisons are what would show that.
/// </remarks>
public sealed class EvidenceEpaPublishCheckTests
{
    private const string Actor = "builder-admin";
    private const string DisplayFieldsJson = "[\"epa_id\"]";

    private const string SchemaJson = """
        {
          "version": 1,
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "title", "type": "text", "label": "Title", "required": false },
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "second_epa", "type": "epa", "label": "Another EPA", "required": false },
                { "key": "overall", "type": "number", "label": "Overall", "required": false }
              ]
            }
          ]
        }
        """;

    private static readonly string SchemaWithoutPointerJson =
        SchemaJson.Replace("\"evidence_epa_field\": \"epa_id\",", string.Empty, StringComparison.Ordinal);

    private const string CreditsFromThePointer = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    private const string CreditsFromAnotherEpaField = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "second_epa" }, "amount": 1 } ] }
        """;

    /// <summary>The second directive disagrees; one bad directive among good ones is still refused.</summary>
    private const string OneOfTwoDirectivesDisagrees = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 },
            { "curriculum_item_match": { "epa_field": "second_epa" }, "amount": 1 }
          ]
        }
        """;

    private const string CreditsNothing = """{ "counts_for": [] }""";

    private const string CreditsALiteralItem = """
        { "counts_for": [ { "curriculum_item_match": { "curriculum_item_id": 42 }, "amount": 1 } ] }
        """;

    private const string CreditsAnItemField = """
        { "counts_for": [ { "curriculum_item_match": { "curriculum_item_field": "item_ref" }, "amount": 1 } ] }
        """;

    /// <summary>
    /// The item wins whenever it resolves (<c>CreditTargetResolver.DescribeTarget</c>), so the <c>epa_field</c> beside
    /// it, though it names the pointer, does not say what was credited.
    /// </summary>
    private const string CreditsALiteralItemBesideThePointer = """
        { "counts_for": [ { "curriculum_item_match": { "curriculum_item_id": 42, "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    private const string CreditsAnItemFieldBesideThePointer = """
        { "counts_for": [ { "curriculum_item_match": { "curriculum_item_field": "item_ref", "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    /// <summary>One directive credits the picked EPA, another a fixed item whose EPA may be any other.</summary>
    private const string CreditsThePointerAndALiteralItem = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 },
            { "curriculum_item_match": { "curriculum_item_id": 42 }, "amount": 1 }
          ]
        }
        """;

    /// <summary>An EPA id held in a <c>number</c> field: the pointer may not name it, so credit may not read it.</summary>
    private const string CreditsFromANumberField = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "overall" }, "amount": 1 } ] }
        """;

    private const string CreditsFromAMissingField = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "no_such_field" }, "amount": 1 } ] }
        """;

    // ---- the rule itself ------------------------------------------------------------------------

    [Theory]
    [InlineData(CreditsFromThePointer)]
    [InlineData(CreditsNothing)]
    public void EnsureCreditAgrees_PointerAndCreditThroughItOrNothing_Passes(string creditRulesJson)
    {
        EvidenceEpa.EnsureCreditAgrees(FormSchemaParser.Parse(SchemaJson), CreditRulesParser.Parse(creditRulesJson));
    }

    /// <summary>
    /// A type about no EPA that credits a fixed item, an item field, or nothing, needs no pointer: journal club, a
    /// procedure log. Its rows stamp no EPA, so they cannot contradict what they credit.
    /// </summary>
    [Theory]
    [InlineData(CreditsNothing)]
    [InlineData(CreditsALiteralItem)]
    [InlineData(CreditsAnItemField)]
    public void EnsureCreditAgrees_NoPointerAndNoEpaFieldInCredit_Passes(string creditRulesJson)
    {
        EvidenceEpa.EnsureCreditAgrees(FormSchemaParser.Parse(SchemaWithoutPointerJson), CreditRulesParser.Parse(creditRulesJson));
    }

    [Fact]
    public void EnsureCreditAgrees_CreditReadingAnotherEpaField_IsRefusedNamingBothFields()
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(SchemaJson),
            CreditRulesParser.Parse(CreditsFromAnotherEpaField)));

        Assert.Contains("Credit rule 1 reads its EPA from field 'second_epa'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("the form's EPA field is 'epa_id'", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureCreditAgrees_OneDisagreeingDirectiveAmongAgreeingOnes_IsRefusedNamingIt()
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(SchemaJson),
            CreditRulesParser.Parse(OneOfTwoDirectivesDisagrees)));

        Assert.Contains("Credit rule 2", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Credit rule 1", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A type that credits by EPA must say which field its activities are about, or it would credit an EPA while every
    /// activity it produced stamped none, and each reader of the column would leave its completions out.
    /// </summary>
    [Fact]
    public void EnsureCreditAgrees_CreditReadingAnEpaFieldWithNoPointer_IsRefused()
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(SchemaWithoutPointerJson),
            CreditRulesParser.Parse(CreditsFromThePointer)));

        Assert.Contains("the form does not say which field carries the activity's EPA", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Set evidence_epa_field to 'epa_id'", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// With the pointer declared, a directive that targets an item is refused, alone or among agreeing directives: the
    /// row is stamped with the EPA the trainee picked while credit lands on an item whose EPA nobody picked.
    /// </summary>
    [Theory]
    [InlineData(CreditsALiteralItem, 1, "credits curriculum item 42")]
    [InlineData(CreditsAnItemField, 1, "credits the curriculum item in field 'item_ref'")]
    [InlineData(CreditsThePointerAndALiteralItem, 2, "credits curriculum item 42")]
    public void EnsureCreditAgrees_PointerBesideAnItemTargetedDirective_IsRefused(
        string creditRulesJson,
        int refusedRule,
        string expected)
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(SchemaJson),
            CreditRulesParser.Parse(creditRulesJson)));

        Assert.Contains($"Credit rule {refusedRule} {expected}", exception.Message, StringComparison.Ordinal);
        Assert.Contains("filed against the EPA in 'epa_id'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Credit through epa_field 'epa_id' instead", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A directive naming an item AND an EPA field is refused with or without the pointer, even when its
    /// <c>epa_field</c> IS the pointer: the item wins whenever it resolves, so the EPA field says nothing.
    /// </summary>
    [Theory]
    [InlineData(CreditsALiteralItemBesideThePointer, true, "names both curriculum item 42 and epa_field 'epa_id'")]
    [InlineData(CreditsAnItemFieldBesideThePointer, true, "names both the curriculum item in field 'item_ref' and epa_field 'epa_id'")]
    [InlineData(CreditsALiteralItemBesideThePointer, false, "names both curriculum item 42 and epa_field 'epa_id'")]
    [InlineData(CreditsAnItemFieldBesideThePointer, false, "names both the curriculum item in field 'item_ref' and epa_field 'epa_id'")]
    public void EnsureCreditAgrees_ADirectiveNamingAnItemAndAnEpaField_IsRefused(
        string creditRulesJson,
        bool withPointer,
        string expected)
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(withPointer ? SchemaJson : SchemaWithoutPointerJson),
            CreditRulesParser.Parse(creditRulesJson)));

        Assert.Contains($"Credit rule 1 {expected}", exception.Message, StringComparison.Ordinal);
        Assert.Contains(
            withPointer ? "Credit through epa_field 'epa_id' alone." : "Name one of them.",
            exception.Message,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Credit from a field that is not an <c>epa</c> field is refused saying so, not by suggesting a pointer the
    /// schema parser would then refuse ("must point at an 'epa' field").
    /// </summary>
    [Fact]
    public void EnsureCreditAgrees_CreditReadingANumberField_IsRefusedSayingItIsNotAnEpaField()
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(SchemaWithoutPointerJson),
            CreditRulesParser.Parse(CreditsFromANumberField)));

        Assert.Contains("Credit rule 1 reads its EPA from field 'overall', which is a 'Number' field", exception.Message, StringComparison.Ordinal);
        Assert.Contains("must be an 'epa' field", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Set evidence_epa_field", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EnsureCreditAgrees_CreditReadingAMissingField_IsRefusedSayingSo()
    {
        var exception = Assert.Throws<CreditRulesParseException>(() => EvidenceEpa.EnsureCreditAgrees(
            FormSchemaParser.Parse(SchemaWithoutPointerJson),
            CreditRulesParser.Parse(CreditsFromAMissingField)));

        Assert.Contains("field 'no_such_field', which is not a field of the form", exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("Set evidence_epa_field", exception.Message, StringComparison.Ordinal);
    }

    // ---- SaveDraft --------------------------------------------------------------------------------

    [Fact]
    public void SaveDraft_AgreeingCreditAndPointer_IsStaged()
    {
        var type = PublishedType();

        type.SaveDraft(SchemaJson, ActivityTestData.ValidWorkflowJson, CreditsFromThePointer, DisplayFieldsJson, Actor);

        Assert.True(type.HasDraft);
        Assert.Equal("epa_id", FormSchemaParser.Parse(type.StagingSchemaJson!).EvidenceEpaField);
    }

    [Theory]
    [MemberData(nameof(Disagreements))]
    public void SaveDraft_Disagreement_IsRefusedAndLeavesTheTypeExactlyAsItWas(string schemaJson, string creditRulesJson)
    {
        var type = PublishedType();
        type.SaveDraft(SchemaJson, ActivityTestData.ValidWorkflowJson, CreditsFromThePointer, DisplayFieldsJson, Actor);
        type.StagingUpdatedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var before = Snapshot.Of(type);

        Assert.Throws<CreditRulesParseException>(
            () => type.SaveDraft(schemaJson, ActivityTestData.ValidWorkflowJson, creditRulesJson, DisplayFieldsJson, "someone-else"));

        Assert.Equal(before, Snapshot.Of(type));
    }

    // ---- PublishDraft -----------------------------------------------------------------------------

    /// <summary>
    /// A draft staged before the rule, written straight onto the staging columns as a pre-T137 save would have left
    /// it, never met it. Publishing it is refused and nothing moves: no published column, no version, no version row.
    /// </summary>
    [Theory]
    [MemberData(nameof(Disagreements))]
    public void PublishDraft_ADisagreeingDraftStagedBeforeTheRule_IsRefusedAndChangesNothing(string schemaJson, string creditRulesJson)
    {
        var type = PublishedType();
        type.StagingSchemaJson = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));
        type.StagingWorkflowJson = WorkflowParser.Serialize(WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson));
        type.StagingCreditRulesJson = CreditRulesParser.Serialize(CreditRulesParser.Parse(creditRulesJson));
        type.StagingDisplayFieldsJson = DisplayFieldsJson;
        type.StagingUpdatedByUserId = "pre-t137-author";
        type.StagingUpdatedOn = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var before = Snapshot.Of(type);

        Assert.Throws<CreditRulesParseException>(() => type.PublishDraft(Actor));

        Assert.Equal(before, Snapshot.Of(type));
        Assert.True(type.HasDraft);
    }

    [Fact]
    public void PublishDraft_AnAgreeingDraft_IsPublished()
    {
        var type = PublishedType();
        var versionBefore = type.Version;

        type.SaveDraft(SchemaJson, ActivityTestData.ValidWorkflowJson, CreditsFromThePointer, DisplayFieldsJson, Actor);
        type.PublishDraft(Actor);

        Assert.Equal(versionBefore + 1, type.Version);
        Assert.Equal("epa_id", FormSchemaParser.Parse(type.SchemaJson!).EvidenceEpaField);
        Assert.Equal("epa_id", FormSchemaParser.Parse(type.Versions.Single(version => version.Version == type.Version).SchemaJson).EvidenceEpaField);
    }

    public static TheoryData<string, string> Disagreements() => new()
    {
        { SchemaJson, CreditsFromAnotherEpaField },
        { SchemaJson, OneOfTwoDirectivesDisagrees },
        { SchemaWithoutPointerJson, CreditsFromThePointer },
        { SchemaJson, CreditsThePointerAndALiteralItem },
        { SchemaJson, CreditsAnItemFieldBesideThePointer }
    };

    // ---- fixtures ---------------------------------------------------------------------------------

    private static ActivityType PublishedType()
    {
        var type = new ActivityType
        {
            Id = 12,
            Key = "evidence_epa_probe",
            Name = "Evidence EPA probe",
            Scope = ActivityScope.Global,
            OwnerUserId = Actor,
            CreatedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        type.SaveDraft(
            ActivityTestData.ValidSchemaJson,
            ActivityTestData.ValidWorkflowJson,
            ActivityTestData.ValidCreditRulesJson,
            "[\"title\"]",
            Actor);
        type.PublishDraft(Actor);

        Assert.Equal(1, type.Version);
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

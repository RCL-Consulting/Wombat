using System.Text.Json;
using FluentAssertions;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// The round-trip guard for the seed corpus (T103).
/// </summary>
/// <remarks>
/// <para>
/// <c>ActivityType.SaveDraft</c> stores <c>Serialize(Parse(json))</c>, never the bytes it was
/// handed. A DSL property that has a <c>Parse</c> half but no <c>Serialize</c> half is therefore
/// dropped at publish, silently — the seed file still reads correctly on disk and the published
/// version quietly loses it. Worse, once <see cref="Wombat.Infrastructure.Persistence.ActivityTypeSeedRefresher"/>
/// compares canonical-to-canonical, the loss becomes invisible forever: both sides drop the same
/// property, so the refresher reports "in sync".
/// </para>
/// <para>
/// Two properties are asserted here, per DSL, per seed folder:
/// <list type="number">
/// <item><b>Fixed point</b> — <c>canon(canon(x)) == canon(x)</c>, byte for byte. This is what the
/// refresher's idempotency rests on.</item>
/// <item><b>No loss</b> — every property present in the raw file survives into the canonical form
/// with the same value. Canonical may add properties (it materialises the omitted
/// <c>"required": false</c> default); it may never drop one.</item>
/// </list>
/// </para>
/// </remarks>
public sealed class SeedRoundTripTests
{
    public static TheoryData<string> SeedDirectories
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var directory in EnumerateSeedDirectories())
            {
                data.Add(Path.GetFileName(directory));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_CanonicalisationIsAFixedPoint(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "schema.json");
        var once = FormSchemaParser.Serialize(FormSchemaParser.Parse(raw));
        var twice = FormSchemaParser.Serialize(FormSchemaParser.Parse(once));

        twice.Should().Be(once, "schema.json for '{0}' must canonicalise to a fixed point", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Workflow_CanonicalisationIsAFixedPoint(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "workflow.json");
        var once = WorkflowParser.Serialize(WorkflowParser.Parse(raw));
        var twice = WorkflowParser.Serialize(WorkflowParser.Parse(once));

        twice.Should().Be(once, "workflow.json for '{0}' must canonicalise to a fixed point", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Credit_CanonicalisationIsAFixedPoint(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "credit.json");
        var once = CreditRulesParser.Serialize(CreditRulesParser.Parse(raw));
        var twice = CreditRulesParser.Serialize(CreditRulesParser.Parse(once));

        twice.Should().Be(once, "credit.json for '{0}' must canonicalise to a fixed point", seedKey);
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_RoundTripDropsNothingOnDisk(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "schema.json");
        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(raw));

        AssertNothingLost(raw, canonical, $"{seedKey}/schema.json");
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Workflow_RoundTripDropsNothingOnDisk(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "workflow.json");
        var canonical = WorkflowParser.Serialize(WorkflowParser.Parse(raw));

        AssertNothingLost(raw, canonical, $"{seedKey}/workflow.json");
    }

    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Credit_RoundTripDropsNothingOnDisk(string seedKey)
    {
        var raw = ReadSeedFile(seedKey, "credit.json");
        var canonical = CreditRulesParser.Serialize(CreditRulesParser.Parse(raw));

        AssertNothingLost(raw, canonical, $"{seedKey}/credit.json");
    }

    /// <summary>
    /// T070 added <c>editable_by</c> at three levels. Naming them explicitly means a future
    /// Serialize that forgets one fails here by name, not by an opaque corpus-wide diff.
    /// </summary>
    [Fact]
    public void EditableBy_SurvivesTheRoundTripAtSectionFieldAndStateLevel()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "assessor",
                  "title": "Assessor",
                  "editable_by": "field:assessor_user_id",
                  "fields": [
                    { "key": "overall", "type": "number", "label": "Overall", "editable_by": "field:assessor_user_id" }
                  ]
                }
              ]
            }
            """;

        const string workflowJson = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" }
              ],
              "transitions": [
                { "key": "request", "from": "draft", "to": "accepted", "actor": "subject" }
              ]
            }
            """;

        var canonicalSchema = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));
        var canonicalWorkflow = WorkflowParser.Serialize(WorkflowParser.Parse(workflowJson));

        canonicalSchema.Should().Contain("\"editable_by\":\"field:assessor_user_id\"");
        AssertNothingLost(schemaJson, canonicalSchema, "editable_by schema fixture");

        canonicalWorkflow.Should().Contain("\"editable_by\":\"field:assessor_user_id\"");
        AssertNothingLost(workflowJson, canonicalWorkflow, "editable_by workflow fixture");
    }

    /// <summary>
    /// T119 added a root <c>observation_date_field</c> pointer at the field that records when the
    /// encounter happened. Named explicitly for the same reason <c>editable_by</c> is: a Serialize
    /// that forgets it fails here by name rather than as an opaque corpus-wide diff.
    /// </summary>
    /// <remarks>
    /// This particular loss is invisible in the product rather than noisy. An activity whose pinned
    /// schema carries no pointer falls back to the audit clock, so it silently dates itself by when
    /// the paperwork was filed instead of when the encounter happened — which is precisely the
    /// defect T119 exists to remove, quietly re-created by a missing Serialize half.
    /// </remarks>
    [Fact]
    public void ObservationDateField_SurvivesParseSerializeParse()
    {
        const string schemaJson = """
            {
              "version": 1,
              "observation_date_field": "observed_on",
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "observed_on", "type": "date", "label": "Date observed", "required": true }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().Contain("\"observation_date_field\":\"observed_on\"");
        FormSchemaParser.Parse(canonical).ObservationDateField.Should().Be("observed_on");
        AssertNothingLost(schemaJson, canonical, "observation_date_field fixture");
    }

    /// <summary>
    /// The mirror. <c>reflective_note</c> and <c>qi_project</c> deliberately declare no pointer — a
    /// reflection and a months-long QI project have no single encounter date — so canonicalisation
    /// must not invent one and make them assert a date they do not hold.
    /// </summary>
    [Fact]
    public void ObservationDateField_StaysAbsentWhenTheSchemaDeclaresNone()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "reflection",
                  "title": "Reflection",
                  "fields": [
                    { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().NotContain("observation_date_field");
        FormSchemaParser.Parse(canonical).ObservationDateField.Should().BeNull();
    }

    /// <summary>
    /// The no-loss assertion is only worth having if it actually fails when a property is dropped.
    /// </summary>
    [Fact]
    public void NothingLostAssertion_DetectsADroppedProperty()
    {
        var losses = FindLosses(
            """{"key":"a","editable_by":"subject"}""",
            """{"key":"a"}""");

        losses.Should().ContainSingle().Which.Should().Contain("editable_by");
    }

    [Fact]
    public void NothingLostAssertion_DetectsAChangedValue()
    {
        var losses = FindLosses(
            """{"fields":[{"key":"a","required":true}]}""",
            """{"fields":[{"key":"a","required":false}]}""");

        losses.Should().ContainSingle().Which.Should().Contain("required");
    }

    [Fact]
    public void NothingLostAssertion_AllowsAnAddedDefault()
    {
        var losses = FindLosses(
            """{"key":"a"}""",
            """{"key":"a","required":false}""");

        losses.Should().BeEmpty();
    }

    /// <summary>
    /// T126 added a root <c>rated_level_field</c> pointer at the scale field carrying THE entrustment
    /// rating. Named explicitly for the same reason <c>observation_date_field</c> is: a Serialize that
    /// forgets it fails here by name rather than as an opaque corpus-wide diff.
    /// </summary>
    /// <remarks>
    /// This loss is silent in exactly the way T126 exists to stop. An activity whose pinned schema
    /// carries no pointer cannot say which ladder its ordinal sits on, so a five-rung "4" plotted
    /// against the six-rung CPSA axis draws as rung "3b" and looks entirely normal.
    /// </remarks>
    [Fact]
    public void RatedLevelField_SurvivesParseSerializeParse()
    {
        const string schemaJson = """
            {
              "version": 1,
              "rated_level_field": "overall_level",
              "sections": [
                {
                  "key": "assessment",
                  "title": "Assessment",
                  "fields": [
                    { "key": "overall_level", "type": "scale", "label": "Overall", "options": ["1", "2", "3"], "scale_key": "O-R Scale" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().Contain("\"rated_level_field\":\"overall_level\"");
        FormSchemaParser.Parse(canonical).RatedLevelField.Should().Be("overall_level");
        AssertNothingLost(schemaJson, canonical, "rated_level_field fixture");
    }

    /// <summary>
    /// The mirror. Six seeds rate nothing — a reflection, a procedure log, a journal club — so
    /// canonicalisation must not invent a rating they do not assert.
    /// </summary>
    [Fact]
    public void RatedLevelField_StaysAbsentWhenTheSchemaDeclaresNone()
    {
        const string schemaJson = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "reflection",
                  "title": "Reflection",
                  "fields": [
                    { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
                  ]
                }
              ]
            }
            """;

        var canonical = FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson));

        canonical.Should().NotContain("rated_level_field");
        FormSchemaParser.Parse(canonical).RatedLevelField.Should().BeNull();
    }

    /// <summary>
    /// The corpus-wide invariant, and the one that will catch the next seed rather than this one:
    /// a schema that carries a scale field declares which of them is the rating, and a schema that
    /// carries none declares nothing.
    /// </summary>
    /// <remarks>
    /// The parser already refuses a pointer naming a missing or non-scale field, so what is left to
    /// assert is the omission — which the parser cannot catch, because "declares no rating" is a
    /// legitimate state. Ten more v11.1 tools are queued in T120; each carries a scale field, and each
    /// will fail here if it forgets the pointer. Before T126 the only thing that knew was the credit
    /// rules' <c>minimum_level_field</c>, so a tool that credited nothing had no stated rated field.
    /// </remarks>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));

        var scaleFields = schema.Sections
            .SelectMany(section => section.Fields)
            .Where(field => field.Type == FieldType.Scale)
            .Select(field => field.Key)
            .ToArray();

        if (scaleFields.Length == 0)
        {
            schema.RatedLevelField.Should().BeNull(
                "'{0}' declares no scale field, so it rates nothing and must not claim to", seedKey);
            return;
        }

        schema.RatedLevelField.Should().NotBeNull(
            "'{0}' declares {1} scale field(s) ({2}) and nothing else can say which is the entrustment " +
            "rating — that is the gap T126 closed",
            seedKey, scaleFields.Length, string.Join(", ", scaleFields));

        scaleFields.Should().Contain(schema.RatedLevelField!);
    }

    /// <summary>
    /// The pointer and the credit directive answer different questions, and today they agree across the
    /// whole corpus. Pinned so that the day they diverge is a deliberate decision with a failing test
    /// behind it, rather than a discovery.
    /// </summary>
    /// <remarks>
    /// They are NOT the same question. <c>minimum_level_field</c> names the field a particular directive
    /// gates on, which could legitimately be one of the five or six component scales that <c>dops</c>,
    /// <c>acat</c>, <c>mini_cex</c> and <c>cbd</c> each declare. That they coincide everywhere today is
    /// why the credit rules could stand in for a rated field at all — and why the substitution looked
    /// safe enough to survive until T126.
    /// </remarks>
    [Theory]
    [MemberData(nameof(SeedDirectories))]
    public void Schema_RatedFieldAgreesWithTheCreditDirective(string seedKey)
    {
        var schema = FormSchemaParser.Parse(ReadSeedFile(seedKey, "schema.json"));
        var creditJson = ReadSeedFile(seedKey, "credit.json");

        using var document = JsonDocument.Parse(creditJson);
        var minimumLevelFields = new List<string>();
        CollectMinimumLevelFields(document.RootElement, minimumLevelFields);

        foreach (var field in minimumLevelFields.Distinct(StringComparer.Ordinal))
        {
            schema.RatedLevelField.Should().Be(field,
                "'{0}' credits on '{1}', and while the two questions differ the corpus has never disagreed",
                seedKey, field);
        }
    }

    private static void CollectMinimumLevelFields(JsonElement element, List<string> into)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("minimum_level_field") && property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            into.Add(value);
                        }
                    }

                    CollectMinimumLevelFields(property.Value, into);
                }

                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    CollectMinimumLevelFields(item, into);
                }

                break;
        }
    }

    private static void AssertNothingLost(string rawJson, string canonicalJson, string context)
    {
        var losses = FindLosses(rawJson, canonicalJson);

        losses.Should().BeEmpty(
            "{0} must survive Parse+Serialize intact — a missing Serialize half is dropped silently at publish. Losses: {1}",
            context,
            string.Join("; ", losses));
    }

    private static IReadOnlyList<string> FindLosses(string rawJson, string canonicalJson)
    {
        using var raw = JsonDocument.Parse(rawJson);
        using var canonical = JsonDocument.Parse(canonicalJson);

        var losses = new List<string>();
        Compare(raw.RootElement, canonical.RootElement, "$", losses);
        return losses;
    }

    private static void Compare(JsonElement raw, JsonElement canonical, string path, List<string> losses)
    {
        // WorkflowParser.Serialize collapses a single-element `from` array to a bare string. That
        // is a documented equivalence in the DSL, not a loss.
        if (raw.ValueKind == JsonValueKind.Array &&
            canonical.ValueKind != JsonValueKind.Array &&
            raw.GetArrayLength() == 1)
        {
            Compare(raw[0], canonical, path + "[0]", losses);
            return;
        }

        if (raw.ValueKind != canonical.ValueKind)
        {
            losses.Add($"{path}: raw is {raw.ValueKind}, canonical is {canonical.ValueKind}");
            return;
        }

        switch (raw.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in raw.EnumerateObject())
                {
                    if (!canonical.TryGetProperty(property.Name, out var canonicalValue))
                    {
                        losses.Add($"{path}.{property.Name}: present on disk, missing after Parse+Serialize");
                        continue;
                    }

                    Compare(property.Value, canonicalValue, $"{path}.{property.Name}", losses);
                }

                break;

            case JsonValueKind.Array:
                if (raw.GetArrayLength() != canonical.GetArrayLength())
                {
                    losses.Add($"{path}: raw has {raw.GetArrayLength()} entries, canonical has {canonical.GetArrayLength()}");
                    break;
                }

                for (var index = 0; index < raw.GetArrayLength(); index++)
                {
                    Compare(raw[index], canonical[index], $"{path}[{index}]", losses);
                }

                break;

            case JsonValueKind.String:
                // The parsers trim strings, which is a deliberate normalisation rather than a loss.
                var rawText = raw.GetString()?.Trim();
                if (!string.Equals(rawText, canonical.GetString(), StringComparison.Ordinal))
                {
                    losses.Add($"{path}: '{rawText}' became '{canonical.GetString()}'");
                }

                break;

            case JsonValueKind.Number:
                if (raw.GetDecimal() != canonical.GetDecimal())
                {
                    losses.Add($"{path}: {raw.GetRawText()} became {canonical.GetRawText()}");
                }

                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                break;
        }
    }

    private static string ReadSeedFile(string seedKey, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seedKey, fileName));

    private static IEnumerable<string> EnumerateSeedDirectories()
        => Directory.EnumerateDirectories(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds"));
}

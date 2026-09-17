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

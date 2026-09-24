using Wombat.Domain.Activities.Schema;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T137's root pointer, <c>evidence_epa_field</c>: the <c>epa</c> field naming the EPA an activity is evidence for.
/// </summary>
/// <remarks>
/// Written to the same shape as the T119 and T126 pointer tests in <see cref="FormSchemaParserTests" />, including the
/// two refusals, so the three pointers cannot drift apart. The Serialize half is the one CLAUDE.md names: without it,
/// SaveDraft stores Serialize(Parse(json)) and every publish drops the pointer silently.
/// </remarks>
public sealed class FormSchemaParserEvidenceEpaTests
{
    private const string SchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "rated_level_field": "overall",
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true },
                { "key": "overall", "type": "scale", "label": "Overall", "required": false, "options": ["1", "2", "3"], "scale_key": "O-R Scale" },
                { "key": "feedback", "type": "longtext", "label": "Feedback", "required": false }
              ]
            }
          ]
        }
        """;

    [Fact]
    public void Parse_EvidenceEpaField_IsRead()
    {
        var schema = FormSchemaParser.Parse(SchemaJson);

        Assert.Equal("epa_id", schema.EvidenceEpaField);
    }

    [Fact]
    public void Parse_EvidenceEpaField_IsTrimmed()
    {
        var json = SchemaJson.Replace("\"evidence_epa_field\": \"epa_id\"", "\"evidence_epa_field\": \"  epa_id  \"", StringComparison.Ordinal);

        Assert.Equal("epa_id", FormSchemaParser.Parse(json).EvidenceEpaField);
    }

    [Fact]
    public void Serialize_EvidenceEpaField_RoundTripsModuloWhitespace()
    {
        var serialized = FormSchemaParser.Serialize(FormSchemaParser.Parse(SchemaJson));

        Assert.Equal(ActivityTestData.NormalizeJson(SchemaJson), ActivityTestData.NormalizeJson(serialized));
    }

    [Fact]
    public void Serialize_EvidenceEpaField_SurvivesASecondRoundTrip()
    {
        var once = FormSchemaParser.Serialize(FormSchemaParser.Parse(SchemaJson));
        var twice = FormSchemaParser.Serialize(FormSchemaParser.Parse(once));

        Assert.Equal(once, twice);
        Assert.Equal("epa_id", FormSchemaParser.Parse(twice).EvidenceEpaField);
    }

    /// <summary>
    /// Five of the nineteen seeds are about no single EPA. Canonicalisation must not invent one for them.
    /// </summary>
    [Fact]
    public void Serialize_WithoutEvidenceEpaField_EmitsNoProperty()
    {
        var json = SchemaJson.Replace("\"evidence_epa_field\": \"epa_id\",", string.Empty, StringComparison.Ordinal);

        var serialized = FormSchemaParser.Serialize(FormSchemaParser.Parse(json));

        Assert.DoesNotContain("evidence_epa_field", serialized, StringComparison.Ordinal);
        Assert.Null(FormSchemaParser.Parse(serialized).EvidenceEpaField);
    }

    [Fact]
    public void Parse_EvidenceEpaFieldNamingNoField_ThrowsSchemaParseException()
    {
        var json = SchemaJson.Replace("\"evidence_epa_field\": \"epa_id\"", "\"evidence_epa_field\": \"nope\"", StringComparison.Ordinal);

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
        Assert.Contains("evidence_epa_field 'nope' does not match any field", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A pointer at a field that holds no EPA id would stamp nothing on every activity and fail nowhere.
    /// </summary>
    [Fact]
    public void Parse_EvidenceEpaFieldPointingAtANonEpaField_ThrowsSchemaParseException()
    {
        var json = SchemaJson.Replace("\"evidence_epa_field\": \"epa_id\"", "\"evidence_epa_field\": \"feedback\"", StringComparison.Ordinal);

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
        Assert.Contains("must point at an 'epa' field", exception.Message, StringComparison.Ordinal);
    }
}

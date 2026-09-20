using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

public sealed class FormSchemaParserTests
{
    [Fact]
    public void Parse_ValidSchema_ReturnsSchema()
    {
        var schema = FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson);

        Assert.Equal(1, schema.Version);
        Assert.Single(schema.Sections);
        Assert.Equal("details", schema.Sections[0].Key);
        Assert.Equal(FieldType.Text, schema.Sections[0].Fields[0].Type);
    }

    [Fact]
    public void Parse_MissingFieldKey_Throws()
    {
        const string json = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "type": "text", "label": "Title" }
                  ]
                }
              ]
            }
            """;

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));

        Assert.Contains("key", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_UnknownFieldType_Throws()
    {
        const string json = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "title", "type": "alien", "label": "Title" }
                  ]
                }
              ]
            }
            """;

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));

        Assert.Contains("Unknown field type", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_InvalidSectionShape_Throws()
    {
        const string json = """
            {
              "version": 1,
              "sections": {
                "key": "details"
              }
            }
            """;

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));

        Assert.Contains("sections must be an array", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_RoundTripsModuloWhitespace()
    {
        var parsed = FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson);
        var serialized = FormSchemaParser.Serialize(parsed);

        Assert.Equal(
            ActivityTestData.NormalizeJson(ActivityTestData.ValidSchemaJson),
            ActivityTestData.NormalizeJson(serialized));
    }

    [Fact]
    public void Parse_EditableBy_OnSectionAndField_ReturnsActorRules()
    {
        var schema = FormSchemaParser.Parse(EditableBySchemaJson);

        Assert.Null(schema.Sections[0].EditableBy);
        Assert.Null(schema.Sections[0].Fields[0].EditableBy);

        var sectionRule = Assert.IsType<FieldUserActorRule>(schema.Sections[1].EditableBy);
        Assert.Equal("assessor_user_id", sectionRule.Field);

        Assert.Null(schema.Sections[1].Fields[0].EditableBy);

        var fieldRule = Assert.IsType<CombinedActorRule>(schema.Sections[1].Fields[1].EditableBy);
        Assert.Equal(ActorRuleCombinationKind.Any, fieldRule.CombinationKind);
        Assert.Equal(2, fieldRule.Rules.Count);
    }

    [Fact]
    public void Serialize_EditableBy_RoundTripsModuloWhitespace()
    {
        var parsed = FormSchemaParser.Parse(EditableBySchemaJson);
        var serialized = FormSchemaParser.Serialize(parsed);

        Assert.Equal(
            ActivityTestData.NormalizeJson(EditableBySchemaJson),
            ActivityTestData.NormalizeJson(serialized));
    }

    [Fact]
    public void Serialize_WithoutEditableBy_EmitsNoProperty()
    {
        var parsed = FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson);
        var serialized = FormSchemaParser.Serialize(parsed);

        Assert.DoesNotContain("editable_by", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_UnknownEditableByToken_ThrowsSchemaParseException()
    {
        const string json = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "editable_by": "assessor",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title" }
                  ]
                }
              ]
            }
            """;

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));

        Assert.Contains("editable_by", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_UnknownEditableByTokenOnField_ThrowsSchemaParseException()
    {
        const string json = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title", "editable_by": "assessor" }
                  ]
                }
              ]
            }
            """;

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));

        Assert.Contains("editable_by", exception.Message, StringComparison.Ordinal);
    }

    private const string EditableBySchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                {
                  "key": "epa_id",
                  "type": "epa",
                  "label": "EPA",
                  "required": true
                }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                {
                  "key": "overall_level",
                  "type": "scale",
                  "label": "Overall level",
                  "required": false
                },
                {
                  "key": "strengths",
                  "type": "longtext",
                  "label": "Strengths",
                  "required": false,
                  "editable_by": "field:assessor_user_id|role:Assessor"
                }
              ]
            }
          ]
        }
        """;

    // ---- Root pointers: observation_date_field (T119) and rated_level_field (T126) ----------------
    //
    // T119 shipped both refusals below and neither was ever tested: before T126 this file contained no
    // occurrence of "observ" at all. They are written here rather than left for the task that owns them
    // because T126 adds the second pointer with the same validation, and an untested guard on one of a
    // matched pair is how the pair drifts.

    private const string TwoPointerSchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "rated_level_field": "overall",
          "sections": [
            {
              "key": "encounter",
              "title": "Encounter",
              "fields": [
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true },
                { "key": "technique", "type": "scale", "label": "Technique", "required": false, "options": ["1", "2", "3"], "scale_key": "O-R Scale" },
                { "key": "overall", "type": "scale", "label": "Overall", "required": false, "options": ["1", "2", "3"], "scale_key": "O-R Scale" },
                { "key": "feedback", "type": "longtext", "label": "Feedback", "required": false }
              ]
            }
          ]
        }
        """;

    [Fact]
    public void Parse_RatedLevelField_IsRead()
    {
        var schema = FormSchemaParser.Parse(TwoPointerSchemaJson);

        Assert.Equal("overall", schema.RatedLevelField);
        Assert.Equal("observed_on", schema.ObservationDateField);
    }

    [Fact]
    public void Serialize_RatedLevelField_RoundTripsModuloWhitespace()
    {
        var parsed = FormSchemaParser.Parse(TwoPointerSchemaJson);
        var serialized = FormSchemaParser.Serialize(parsed);

        Assert.Equal(
            ActivityTestData.NormalizeJson(TwoPointerSchemaJson),
            ActivityTestData.NormalizeJson(serialized));
    }

    /// <summary>
    /// The trap CLAUDE.md names: a Parse half with no Serialize half is dropped at publish with no
    /// error, because ActivityType.SaveDraft stores Serialize(Parse(json)) rather than what it was given.
    /// </summary>
    [Fact]
    public void Serialize_RatedLevelField_SurvivesASecondRoundTrip()
    {
        var once = FormSchemaParser.Serialize(FormSchemaParser.Parse(TwoPointerSchemaJson));
        var twice = FormSchemaParser.Serialize(FormSchemaParser.Parse(once));

        Assert.Equal(once, twice);
        Assert.Equal("overall", FormSchemaParser.Parse(twice).RatedLevelField);
    }

    /// <summary>
    /// Six of the fourteen seeds rate nothing. Canonicalisation must not invent a rating for them.
    /// </summary>
    [Fact]
    public void Serialize_WithoutRatedLevelField_EmitsNoProperty()
    {
        var parsed = FormSchemaParser.Parse(ActivityTestData.ValidSchemaJson);
        var serialized = FormSchemaParser.Serialize(parsed);

        Assert.DoesNotContain("rated_level_field", serialized, StringComparison.Ordinal);
        Assert.Null(FormSchemaParser.Parse(serialized).RatedLevelField);
    }

    [Fact]
    public void Parse_RatedLevelFieldNamingNoField_ThrowsSchemaParseException()
    {
        var json = TwoPointerSchemaJson.Replace("\"rated_level_field\": \"overall\"", "\"rated_level_field\": \"nope\"", StringComparison.Ordinal);

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
        Assert.Contains("does not match any field", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// A pointer at a field that carries no ladder is the defect this validation exists for: it would
    /// otherwise surface as a trajectory point that silently never resolves a scale.
    /// </summary>
    [Fact]
    public void Parse_RatedLevelFieldPointingAtANonScaleField_ThrowsSchemaParseException()
    {
        var json = TwoPointerSchemaJson.Replace("\"rated_level_field\": \"overall\"", "\"rated_level_field\": \"feedback\"", StringComparison.Ordinal);

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
        Assert.Contains("must point at a 'scale' field", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_ObservationDateFieldNamingNoField_ThrowsSchemaParseException()
    {
        var json = TwoPointerSchemaJson.Replace("\"observation_date_field\": \"observed_on\"", "\"observation_date_field\": \"nope\"", StringComparison.Ordinal);

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
        Assert.Contains("does not match any field", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_ObservationDateFieldPointingAtANonDateField_ThrowsSchemaParseException()
    {
        var json = TwoPointerSchemaJson.Replace("\"observation_date_field\": \"observed_on\"", "\"observation_date_field\": \"feedback\"", StringComparison.Ordinal);

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
        Assert.Contains("must point at a 'date' field", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The root allow-list is a hard gate: an unknown root key fails the whole parse, which on a fresh
    /// database means the seeders cannot run. Worth pinning, since adding a property means editing it.
    /// </summary>
    [Fact]
    public void Parse_UnknownRootProperty_ThrowsSchemaParseException()
    {
        var json = TwoPointerSchemaJson.Replace("\"version\": 1,", "\"version\": 1, \"rated_field\": \"overall\",", StringComparison.Ordinal);

        Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));
    }
}

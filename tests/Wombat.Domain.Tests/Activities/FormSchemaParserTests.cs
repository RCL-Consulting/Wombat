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
}

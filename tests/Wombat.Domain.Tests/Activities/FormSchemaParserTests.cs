using Wombat.Domain.Activities.Schema;

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
}

using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities.Schema;
using Wombat.Infrastructure.Activities;

namespace Wombat.Application.Tests.Activities;

public sealed class SchemaValidatorTests
{
    private readonly SchemaValidator _validator = new();

    [Fact]
    public void Validate_WhenRequiredFieldMissingOnSubmit_ReturnsRequiredError()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title", "required": true }
                  ]
                }
              ]
            }
            """);

        var errors = _validator.Validate(schema, "{}", SchemaValidationMode.Submit);

        errors.Should().ContainSingle(error => error.FieldKey == "title" && error.Rule == "required");
    }

    [Fact]
    public void Validate_WhenNumericValueViolatesMinMax_ReturnsRangeErrors()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    {
                      "key": "score",
                      "type": "number",
                      "label": "Score",
                      "required": true,
                      "validation": { "min": 1, "max": 5 }
                    }
                  ]
                }
              ]
            }
            """);

        var lowErrors = _validator.Validate(schema, """{ "score": 0 }""", SchemaValidationMode.Submit);
        var highErrors = _validator.Validate(schema, """{ "score": 6 }""", SchemaValidationMode.Submit);

        lowErrors.Should().ContainSingle(error => error.FieldKey == "score" && error.Rule == "min");
        highErrors.Should().ContainSingle(error => error.FieldKey == "score" && error.Rule == "max");
    }

    [Fact]
    public void Validate_WhenFieldIsHidden_DoesNotTreatItAsRequired()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    { "key": "kind", "type": "choice", "label": "Kind", "required": true, "options": ["basic", "advanced"] },
                    {
                      "key": "details",
                      "type": "text",
                      "label": "Details",
                      "required": true,
                      "show_if": { "field": "kind", "operator": "equals", "value": "advanced" }
                    }
                  ]
                }
              ]
            }
            """);

        var errors = _validator.Validate(schema, """{ "kind": "basic" }""", SchemaValidationMode.Submit);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenScaleValueFallsOutsideDeclaredScale_ReturnsScaleError()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "details",
                  "title": "Details",
                  "fields": [
                    {
                      "key": "entrustment",
                      "type": "scale",
                      "label": "Entrustment",
                      "required": true,
                      "options": ["1", "2", "3", "4", "5"]
                    }
                  ]
                }
              ]
            }
            """);

        var errors = _validator.Validate(schema, """{ "entrustment": 7 }""", SchemaValidationMode.Submit);

        errors.Should().ContainSingle(error => error.FieldKey == "entrustment" && error.Rule == "scale");
    }
}

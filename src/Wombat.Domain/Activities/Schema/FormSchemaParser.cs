using System.Text.Json;

namespace Wombat.Domain.Activities.Schema;

public static class FormSchemaParser
{
    public static FormSchema Parse(string schemaJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaJson);

        try
        {
            using var document = JsonDocument.Parse(schemaJson);
            return ParseSchema(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new SchemaParseException($"Schema JSON is malformed: {exception.Message}");
        }
    }

    public static string Serialize(FormSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteNumber("version", schema.Version);
        writer.WritePropertyName("sections");
        writer.WriteStartArray();

        foreach (var section in schema.Sections)
        {
            writer.WriteStartObject();
            writer.WriteString("key", section.Key);
            writer.WriteString("title", section.Title);

            if (section.ShowIf is not null)
            {
                WriteVisibilityCondition(writer, "show_if", section.ShowIf);
            }

            writer.WritePropertyName("fields");
            writer.WriteStartArray();

            foreach (var field in section.Fields)
            {
                writer.WriteStartObject();
                writer.WriteString("key", field.Key);
                writer.WriteString("type", ToJsonValue(field.Type));
                writer.WriteString("label", field.Label);

                if (!string.IsNullOrWhiteSpace(field.HelpText))
                {
                    writer.WriteString("help_text", field.HelpText);
                }

                writer.WriteBoolean("required", field.Required);

                if (field.Options.Count > 0)
                {
                    writer.WritePropertyName("options");
                    writer.WriteStartArray();
                    foreach (var option in field.Options)
                    {
                        writer.WriteStringValue(option);
                    }

                    writer.WriteEndArray();
                }

                if (!string.IsNullOrWhiteSpace(field.CatalogueKey))
                {
                    writer.WriteString("catalogue", field.CatalogueKey);
                }

                if (!string.IsNullOrWhiteSpace(field.ScaleKey))
                {
                    writer.WriteString("scale_key", field.ScaleKey);
                }

                if (field.Validation is not null)
                {
                    writer.WritePropertyName("validation");
                    writer.WriteStartObject();

                    if (field.Validation.Min is not null)
                    {
                        writer.WriteNumber("min", field.Validation.Min.Value);
                    }

                    if (field.Validation.Max is not null)
                    {
                        writer.WriteNumber("max", field.Validation.Max.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(field.Validation.Regex))
                    {
                        writer.WriteString("regex", field.Validation.Regex);
                    }

                    if (field.Validation.MinLength is not null)
                    {
                        writer.WriteNumber("min_length", field.Validation.MinLength.Value);
                    }

                    if (field.Validation.MaxLength is not null)
                    {
                        writer.WriteNumber("max_length", field.Validation.MaxLength.Value);
                    }

                    writer.WriteEndObject();
                }

                if (field.ShowIf is not null)
                {
                    WriteVisibilityCondition(writer, "show_if", field.ShowIf);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static FormSchema ParseSchema(JsonElement root)
    {
        EnsureObject(root, "Schema root must be an object.");
        EnsureAllowedProperties(root, ["version", "sections"], "schema");

        var version = GetRequiredInt(root, "version");
        var sectionsElement = GetRequiredProperty(root, "sections");
        EnsureArray(sectionsElement, "Schema sections must be an array.");

        var sections = sectionsElement
            .EnumerateArray()
            .Select(ParseSection)
            .ToList();

        if (sections.Count == 0)
        {
            throw new SchemaParseException("Schema must contain at least one section.");
        }

        return new FormSchema(version, sections);
    }

    private static FormSection ParseSection(JsonElement element)
    {
        EnsureObject(element, "Section must be an object.");
        EnsureAllowedProperties(element, ["key", "title", "show_if", "fields"], "section");

        var key = GetRequiredString(element, "key");
        var title = GetRequiredString(element, "title");
        var fieldsElement = GetRequiredProperty(element, "fields");
        EnsureArray(fieldsElement, $"Section '{key}' fields must be an array.");

        var fields = fieldsElement
            .EnumerateArray()
            .Select(ParseField)
            .ToList();

        if (fields.Count == 0)
        {
            throw new SchemaParseException($"Section '{key}' must contain at least one field.");
        }

        return new FormSection(key, title, ParseOptionalVisibilityCondition(element, "show_if"), fields);
    }

    private static FormField ParseField(JsonElement element)
    {
        EnsureObject(element, "Field must be an object.");
        EnsureAllowedProperties(
            element,
            ["key", "type", "label", "help_text", "required", "options", "catalogue", "scale_key", "validation", "show_if"],
            "field");

        return new FormField(
            GetRequiredString(element, "key"),
            ParseFieldType(GetRequiredString(element, "type")),
            GetRequiredString(element, "label"),
            GetOptionalTrimmedString(element, "help_text"),
            GetBooleanOrDefault(element, "required"),
            ParseOptionalStringArray(element, "options"),
            GetOptionalTrimmedString(element, "catalogue"),
            GetOptionalTrimmedString(element, "scale_key"),
            ParseOptionalValidation(element, "validation"),
            ParseOptionalVisibilityCondition(element, "show_if"));
    }

    private static VisibilityCondition? ParseOptionalVisibilityCondition(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var conditionElement))
        {
            return null;
        }

        EnsureObject(conditionElement, $"'{propertyName}' must be an object.");
        EnsureAllowedProperties(conditionElement, ["field", "operator", "value"], propertyName);

        return new VisibilityCondition(
            GetRequiredString(conditionElement, "field"),
            GetRequiredString(conditionElement, "operator"),
            GetOptionalTrimmedString(conditionElement, "value"));
    }

    private static FieldValidation? ParseOptionalValidation(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var validationElement))
        {
            return null;
        }

        EnsureObject(validationElement, $"'{propertyName}' must be an object.");
        EnsureAllowedProperties(validationElement, ["min", "max", "regex", "min_length", "max_length"], propertyName);

        return new FieldValidation(
            GetOptionalDecimal(validationElement, "min"),
            GetOptionalDecimal(validationElement, "max"),
            GetOptionalTrimmedString(validationElement, "regex"),
            GetOptionalInt(validationElement, "min_length"),
            GetOptionalInt(validationElement, "max_length"));
    }

    private static IReadOnlyList<string> ParseOptionalStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var arrayElement))
        {
            return [];
        }

        EnsureArray(arrayElement, $"'{propertyName}' must be an array.");

        return arrayElement
            .EnumerateArray()
            .Select(item =>
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    throw new SchemaParseException($"'{propertyName}' entries must be strings.");
                }

                var value = item.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new SchemaParseException($"'{propertyName}' entries must not be empty.");
                }

                return value;
            })
            .ToList();
    }

    private static FieldType ParseFieldType(string value)
    {
        return value switch
        {
            "text" => FieldType.Text,
            "longtext" => FieldType.LongText,
            "number" => FieldType.Number,
            "date" => FieldType.Date,
            "datetime" => FieldType.DateTime,
            "choice" => FieldType.Choice,
            "multichoice" => FieldType.MultiChoice,
            "scale" => FieldType.Scale,
            "user" => FieldType.User,
            "epa" => FieldType.Epa,
            "file" => FieldType.File,
            "checkbox" => FieldType.Checkbox,
            "rating" => FieldType.Rating,
            "markdown" => FieldType.Markdown,
            "likert" => FieldType.Likert,
            "procedure_ref" => FieldType.ProcedureRef,
            "signature" => FieldType.Signature,
            _ => throw new SchemaParseException($"Unknown field type '{value}'.")
        };
    }

    private static string ToJsonValue(FieldType value)
    {
        return value switch
        {
            FieldType.Text => "text",
            FieldType.LongText => "longtext",
            FieldType.Number => "number",
            FieldType.Date => "date",
            FieldType.DateTime => "datetime",
            FieldType.Choice => "choice",
            FieldType.MultiChoice => "multichoice",
            FieldType.Scale => "scale",
            FieldType.User => "user",
            FieldType.Epa => "epa",
            FieldType.File => "file",
            FieldType.Checkbox => "checkbox",
            FieldType.Rating => "rating",
            FieldType.Markdown => "markdown",
            FieldType.Likert => "likert",
            FieldType.ProcedureRef => "procedure_ref",
            FieldType.Signature => "signature",
            _ => throw new SchemaParseException($"Unsupported field type '{value}'.")
        };
    }

    private static void WriteVisibilityCondition(Utf8JsonWriter writer, string propertyName, VisibilityCondition condition)
    {
        writer.WritePropertyName(propertyName);
        writer.WriteStartObject();
        writer.WriteString("field", condition.Field);
        writer.WriteString("operator", condition.Operator);

        if (condition.Value is not null)
        {
            writer.WriteString("value", condition.Value);
        }

        writer.WriteEndObject();
    }

    private static void EnsureAllowedProperties(JsonElement element, IEnumerable<string> allowedProperties, string subject)
    {
        var allowed = allowedProperties.ToHashSet(StringComparer.Ordinal);

        foreach (var property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                throw new SchemaParseException($"Unknown property '{property.Name}' in {subject}.");
            }
        }
    }

    private static JsonElement GetRequiredProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            throw new SchemaParseException($"Required property '{propertyName}' is missing.");
        }

        return property;
    }

    private static string GetRequiredString(JsonElement element, string propertyName)
    {
        var property = GetRequiredProperty(element, propertyName);

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new SchemaParseException($"Property '{propertyName}' must be a string.");
        }

        var value = property.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new SchemaParseException($"Property '{propertyName}' must not be empty.");
        }

        return value;
    }

    private static string? GetOptionalTrimmedString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind is JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new SchemaParseException($"Property '{propertyName}' must be a string.");
        }

        var value = property.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool GetBooleanOrDefault(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind != JsonValueKind.True && property.ValueKind != JsonValueKind.False)
        {
            throw new SchemaParseException($"Property '{propertyName}' must be a boolean.");
        }

        return property.GetBoolean();
    }

    private static int GetRequiredInt(JsonElement element, string propertyName)
    {
        var property = GetRequiredProperty(element, propertyName);

        if (!property.TryGetInt32(out var value))
        {
            throw new SchemaParseException($"Property '{propertyName}' must be an integer.");
        }

        return value;
    }

    private static int? GetOptionalInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (!property.TryGetInt32(out var value))
        {
            throw new SchemaParseException($"Property '{propertyName}' must be an integer.");
        }

        return value;
    }

    private static decimal? GetOptionalDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (!property.TryGetDecimal(out var value))
        {
            throw new SchemaParseException($"Property '{propertyName}' must be numeric.");
        }

        return value;
    }

    private static void EnsureObject(JsonElement element, string message)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new SchemaParseException(message);
        }
    }

    private static void EnsureArray(JsonElement element, string message)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            throw new SchemaParseException(message);
        }
    }
}

using System.Text.Json;

namespace Wombat.Domain.Activities.Credit;

public static class CreditRulesParser
{
    public static CreditRules Parse(string creditRulesJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(creditRulesJson);

        try
        {
            using var document = JsonDocument.Parse(creditRulesJson);
            return ParseRules(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new CreditRulesParseException($"Credit rules JSON is malformed: {exception.Message}");
        }
    }

    public static string Serialize(CreditRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WritePropertyName("counts_for");
        writer.WriteStartArray();

        foreach (var directive in rules.CountsFor)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("curriculum_item_match");
            writer.WriteStartObject();

            if (!string.IsNullOrWhiteSpace(directive.CurriculumItemMatchRule.EpaField))
            {
                writer.WriteString("epa_field", directive.CurriculumItemMatchRule.EpaField);
            }

            if (directive.CurriculumItemMatchRule.CurriculumItemId is not null)
            {
                writer.WriteNumber("curriculum_item_id", directive.CurriculumItemMatchRule.CurriculumItemId.Value);
            }

            if (!string.IsNullOrWhiteSpace(directive.CurriculumItemMatchRule.CurriculumItemField))
            {
                writer.WriteString("curriculum_item_field", directive.CurriculumItemMatchRule.CurriculumItemField);
            }

            writer.WriteEndObject();
            writer.WriteNumber("amount", directive.Amount);

            if (!string.IsNullOrWhiteSpace(directive.MinimumLevelField))
            {
                writer.WriteString("minimum_level_field", directive.MinimumLevelField);
            }

            if (!string.IsNullOrWhiteSpace(directive.MinimumLevelFixed))
            {
                writer.WriteString("minimum_level_fixed", directive.MinimumLevelFixed);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static CreditRules ParseRules(JsonElement root)
    {
        EnsureObject(root, "Credit rules root must be an object.");
        EnsureAllowedProperties(root, ["counts_for"], "credit rules");

        var directives = GetRequiredArray(root, "counts_for", "Property 'counts_for' must be an array.")
            .EnumerateArray()
            .Select(ParseDirective)
            .ToList();

        return new CreditRules(directives);
    }

    private static CreditDirective ParseDirective(JsonElement element)
    {
        EnsureObject(element, "Credit directive must be an object.");
        EnsureAllowedProperties(
            element,
            ["curriculum_item_match", "amount", "minimum_level_field", "minimum_level_fixed"],
            "credit directive");

        return new CreditDirective(
            ParseCurriculumItemMatchRule(GetRequiredProperty(element, "curriculum_item_match")),
            GetRequiredInt(element, "amount"),
            GetOptionalTrimmedString(element, "minimum_level_field"),
            GetOptionalTrimmedString(element, "minimum_level_fixed"));
    }

    private static CurriculumItemMatchRule ParseCurriculumItemMatchRule(JsonElement element)
    {
        EnsureObject(element, "Property 'curriculum_item_match' must be an object.");
        EnsureAllowedProperties(
            element,
            ["epa_field", "curriculum_item_id", "curriculum_item_field"],
            "curriculum_item_match");

        return new CurriculumItemMatchRule(
            GetOptionalTrimmedString(element, "epa_field"),
            GetOptionalInt(element, "curriculum_item_id"),
            GetOptionalTrimmedString(element, "curriculum_item_field"));
    }

    private static void EnsureAllowedProperties(JsonElement element, IEnumerable<string> allowedProperties, string subject)
    {
        var allowed = allowedProperties.ToHashSet(StringComparer.Ordinal);

        foreach (var property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                throw new CreditRulesParseException($"Unknown property '{property.Name}' in {subject}.");
            }
        }
    }

    private static JsonElement GetRequiredProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            throw new CreditRulesParseException($"Required property '{propertyName}' is missing.");
        }

        return property;
    }

    private static JsonElement GetRequiredArray(JsonElement element, string propertyName, string errorMessage)
    {
        var property = GetRequiredProperty(element, propertyName);
        if (property.ValueKind != JsonValueKind.Array)
        {
            throw new CreditRulesParseException(errorMessage);
        }

        return property;
    }

    private static int GetRequiredInt(JsonElement element, string propertyName)
    {
        var property = GetRequiredProperty(element, propertyName);
        if (!property.TryGetInt32(out var value))
        {
            throw new CreditRulesParseException($"Property '{propertyName}' must be an integer.");
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
            throw new CreditRulesParseException($"Property '{propertyName}' must be an integer.");
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
            throw new CreditRulesParseException($"Property '{propertyName}' must be a string.");
        }

        var value = property.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void EnsureObject(JsonElement element, string message)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new CreditRulesParseException(message);
        }
    }
}

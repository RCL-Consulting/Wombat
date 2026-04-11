using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Infrastructure.Activities;

public sealed class SchemaValidator : ISchemaValidator
{
    public IReadOnlyList<ActivityValidationErrorDto> Validate(
        FormSchema schema,
        string dataJson,
        SchemaValidationMode mode,
        IReadOnlyCollection<string>? additionallyRequiredFieldKeys = null)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataJson);

        using var document = JsonDocument.Parse(dataJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            return [new ActivityValidationErrorDto(null, "Activity data must be a JSON object.", "invalid_object")];
        }

        var requiredFieldKeys = additionallyRequiredFieldKeys ?? [];
        var errors = new List<ActivityValidationErrorDto>();

        foreach (var section in schema.Sections)
        {
            if (!IsVisible(section.ShowIf, document.RootElement))
            {
                continue;
            }

            foreach (var field in section.Fields)
            {
                ValidateField(field, document.RootElement, mode, requiredFieldKeys, errors);
            }
        }

        return errors;
    }

    private static void ValidateField(
        FormField field,
        JsonElement root,
        SchemaValidationMode mode,
        IReadOnlyCollection<string> additionallyRequiredFieldKeys,
        ICollection<ActivityValidationErrorDto> errors)
    {
        if (!IsVisible(field.ShowIf, root))
        {
            return;
        }

        var isRequired = (mode == SchemaValidationMode.Submit && field.Required) ||
                         additionallyRequiredFieldKeys.Contains(field.Key, StringComparer.Ordinal);

        var hasValue = root.TryGetProperty(field.Key, out var value) && value.ValueKind != JsonValueKind.Null;
        if (!hasValue || IsEmptyValue(value))
        {
            if (isRequired)
            {
                errors.Add(new ActivityValidationErrorDto(field.Key, "A value is required.", "required"));
            }

            return;
        }

        switch (field.Type)
        {
            case FieldType.Text:
            case FieldType.LongText:
            case FieldType.Markdown:
            case FieldType.User:
            case FieldType.File:
                ValidateStringField(field, value, errors);
                break;
            case FieldType.Number:
            case FieldType.Rating:
            case FieldType.Scale:
                ValidateNumericField(field, value, errors);
                break;
            case FieldType.Date:
                ValidateDateField(field, value, errors);
                break;
            case FieldType.DateTime:
                ValidateDateTimeField(field, value, errors);
                break;
            case FieldType.Choice:
                ValidateChoiceField(field, value, errors);
                break;
            case FieldType.MultiChoice:
                ValidateMultiChoiceField(field, value, errors);
                break;
            case FieldType.Epa:
                ValidateIntegerLikeField(field, value, errors);
                break;
            case FieldType.Checkbox:
                if (value.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
                {
                    errors.Add(new ActivityValidationErrorDto(field.Key, "A boolean value is required.", "type"));
                }

                break;
            default:
                errors.Add(new ActivityValidationErrorDto(field.Key, $"Unsupported field type '{field.Type}'.", "unsupported_type"));
                break;
        }
    }

    private static void ValidateStringField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "A string value is required.", "type"));
            return;
        }

        var stringValue = value.GetString() ?? string.Empty;
        if (field.Validation?.MinLength is not null && stringValue.Length < field.Validation.MinLength.Value)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, $"Value must be at least {field.Validation.MinLength.Value} characters long.", "min_length"));
        }

        if (field.Validation?.MaxLength is not null && stringValue.Length > field.Validation.MaxLength.Value)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, $"Value must be at most {field.Validation.MaxLength.Value} characters long.", "max_length"));
        }

        if (!string.IsNullOrWhiteSpace(field.Validation?.Regex) && !Regex.IsMatch(stringValue, field.Validation.Regex))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "Value does not match the required format.", "regex"));
        }
    }

    private static void ValidateNumericField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (!TryGetDecimal(value, out var numberValue))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "A numeric value is required.", "type"));
            return;
        }

        if (field.Validation?.Min is not null && numberValue < field.Validation.Min.Value)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, $"Value must be at least {field.Validation.Min.Value}.", "min"));
        }

        if (field.Validation?.Max is not null && numberValue > field.Validation.Max.Value)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, $"Value must be at most {field.Validation.Max.Value}.", "max"));
        }

        if (field.Options.Count > 0 && !field.Options.Contains(numberValue.ToString(CultureInfo.InvariantCulture), StringComparer.Ordinal))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "Value is outside the declared scale.", "scale"));
        }
    }

    private static void ValidateDateField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (value.ValueKind != JsonValueKind.String || !DateOnly.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "A valid date is required.", "type"));
        }
    }

    private static void ValidateDateTimeField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (value.ValueKind != JsonValueKind.String || !DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "A valid datetime is required.", "type"));
        }
    }

    private static void ValidateChoiceField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "A string value is required.", "type"));
            return;
        }

        var selected = value.GetString() ?? string.Empty;
        if (field.Options.Count > 0 && !field.Options.Contains(selected, StringComparer.Ordinal))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "Value must be one of the declared options.", "options"));
        }
    }

    private static void ValidateMultiChoiceField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "An array value is required.", "type"));
            return;
        }

        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                errors.Add(new ActivityValidationErrorDto(field.Key, "All selected values must be strings.", "type"));
                return;
            }

            var selected = item.GetString() ?? string.Empty;
            if (field.Options.Count > 0 && !field.Options.Contains(selected, StringComparer.Ordinal))
            {
                errors.Add(new ActivityValidationErrorDto(field.Key, "Value must be one of the declared options.", "options"));
            }
        }
    }

    private static void ValidateIntegerLikeField(FormField field, JsonElement value, ICollection<ActivityValidationErrorDto> errors)
    {
        if (!TryGetInt32(value, out _))
        {
            errors.Add(new ActivityValidationErrorDto(field.Key, "An integer value is required.", "type"));
        }
    }

    private static bool IsVisible(VisibilityCondition? condition, JsonElement root)
    {
        if (condition is null)
        {
            return true;
        }

        var hasValue = root.TryGetProperty(condition.Field, out var value) && value.ValueKind != JsonValueKind.Null;
        var stringValue = hasValue ? ToComparableString(value) : null;

        return condition.Operator switch
        {
            "equals" or "eq" or "==" => string.Equals(stringValue, condition.Value, StringComparison.Ordinal),
            "not_equals" or "neq" or "!=" => !string.Equals(stringValue, condition.Value, StringComparison.Ordinal),
            "exists" => hasValue,
            "not_exists" or "missing" => !hasValue,
            "truthy" => hasValue && !string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase) && !string.Equals(stringValue, "0", StringComparison.Ordinal),
            "falsy" => !hasValue || string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(stringValue, "0", StringComparison.Ordinal),
            _ => true
        };
    }

    private static string? ToComparableString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.Array => string.Join(",", value.EnumerateArray().Select(ToComparableString)),
            _ => value.GetRawText()
        };
    }

    private static bool IsEmptyValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()),
            JsonValueKind.Array => value.GetArrayLength() == 0,
            _ => false
        };
    }

    private static bool TryGetDecimal(JsonElement value, out decimal result)
    {
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out result))
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.String &&
            decimal.TryParse(value.GetString(), CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        result = default;
        return false;
    }

    private static bool TryGetInt32(JsonElement value, out int result)
    {
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out result))
        {
            return true;
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), CultureInfo.InvariantCulture, out result))
        {
            return true;
        }

        result = default;
        return false;
    }
}

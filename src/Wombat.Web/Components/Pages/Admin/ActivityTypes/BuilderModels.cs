using System.Text.Json;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

internal sealed class BuilderSchemaModel
{
    public List<BuilderSectionModel> Sections { get; } = [];

    public static BuilderSchemaModel Parse(string schemaJson)
    {
        var schema = FormSchemaParser.Parse(schemaJson);
        var model = new BuilderSchemaModel();

        foreach (var section in schema.Sections)
        {
            model.Sections.Add(new BuilderSectionModel
            {
                Key = section.Key,
                Title = section.Title,
                ShowIfField = section.ShowIf?.Field,
                ShowIfOperator = section.ShowIf?.Operator,
                ShowIfValue = section.ShowIf?.Value,
                Fields = section.Fields.Select(field => new BuilderFieldModel
                {
                    Key = field.Key,
                    Type = field.Type,
                    Label = field.Label,
                    HelpText = field.HelpText,
                    Required = field.Required,
                    OptionsText = string.Join(Environment.NewLine, field.Options),
                    CatalogueKey = field.CatalogueKey,
                    ScaleKey = field.ScaleKey,
                    Min = field.Validation?.Min?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Max = field.Validation?.Max?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    Regex = field.Validation?.Regex,
                    MinLength = field.Validation?.MinLength?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    MaxLength = field.Validation?.MaxLength?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ShowIfField = field.ShowIf?.Field,
                    ShowIfOperator = field.ShowIf?.Operator,
                    ShowIfValue = field.ShowIf?.Value
                }).ToList()
            });
        }

        return model;
    }

    public string ToJson()
    {
        var schema = new FormSchema(
            1,
            Sections.Select(section => new FormSection(
                NormalizeKey(section.Key, "section"),
                section.Title.Trim(),
                BuildVisibility(section.ShowIfField, section.ShowIfOperator, section.ShowIfValue),
                section.Fields.Select(field => new FormField(
                    NormalizeKey(field.Key, "field"),
                    field.Type,
                    field.Label.Trim(),
                    NullIfWhiteSpace(field.HelpText),
                    field.Required,
                    ParseOptions(field.OptionsText),
                    NullIfWhiteSpace(field.CatalogueKey),
                    NullIfWhiteSpace(field.ScaleKey),
                    BuildValidation(field),
                    BuildVisibility(field.ShowIfField, field.ShowIfOperator, field.ShowIfValue)))
                .ToList()))
            .ToList());

        return FormSchemaParser.Serialize(schema);
    }

    public static IReadOnlyList<string> GetPublishWarnings(string? publishedSchemaJson, string draftSchemaJson)
    {
        if (string.IsNullOrWhiteSpace(publishedSchemaJson))
        {
            return [];
        }

        var published = FormSchemaParser.Parse(publishedSchemaJson);
        var draft = FormSchemaParser.Parse(draftSchemaJson);
        var warnings = new List<string>();

        foreach (var publishedSection in published.Sections)
        {
            var draftSection = draft.Sections.SingleOrDefault(section => section.Key == publishedSection.Key);
            if (draftSection is null)
            {
                warnings.Add($"Section '{publishedSection.Title}' will be removed.");
                continue;
            }

            foreach (var publishedField in publishedSection.Fields)
            {
                var draftField = draftSection.Fields.SingleOrDefault(field => field.Key == publishedField.Key);
                if (draftField is null)
                {
                    warnings.Add($"Field '{publishedField.Label}' will be removed.");
                    continue;
                }

                if (draftField.Type != publishedField.Type)
                {
                    warnings.Add($"Field '{publishedField.Label}' changes type from '{publishedField.Type}' to '{draftField.Type}'.");
                }

                if (!publishedField.Required && draftField.Required)
                {
                    warnings.Add($"Field '{publishedField.Label}' becomes required.");
                }
            }
        }

        return warnings;
    }

    private static VisibilityCondition? BuildVisibility(string? field, string? @operator, string? value)
    {
        if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(@operator))
        {
            return null;
        }

        return new VisibilityCondition(field.Trim(), @operator.Trim(), NullIfWhiteSpace(value));
    }

    private static FieldValidation? BuildValidation(BuilderFieldModel field)
    {
        var hasValue =
            !string.IsNullOrWhiteSpace(field.Min) ||
            !string.IsNullOrWhiteSpace(field.Max) ||
            !string.IsNullOrWhiteSpace(field.Regex) ||
            !string.IsNullOrWhiteSpace(field.MinLength) ||
            !string.IsNullOrWhiteSpace(field.MaxLength);

        if (!hasValue)
        {
            return null;
        }

        return new FieldValidation(
            ParseDecimal(field.Min),
            ParseDecimal(field.Max),
            NullIfWhiteSpace(field.Regex),
            ParseInt(field.MinLength),
            ParseInt(field.MaxLength));
    }

    private static IReadOnlyList<string> ParseOptions(string? optionsText)
    {
        if (string.IsNullOrWhiteSpace(optionsText))
        {
            return [];
        }

        return optionsText
            .Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    internal static string BuildDisplayFieldsJson(BuilderSchemaModel schema)
    {
        var keys = schema.Sections
            .SelectMany(section => section.Fields)
            .Take(3)
            .Select(field => field.Key)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return JsonSerializer.Serialize(keys);
    }

    private static string NormalizeKey(string? value, string subject)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{subject} key is required.");
        }

        return value.Trim();
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? ParseDecimal(string? value)
        => decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static int? ParseInt(string? value)
        => int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
}

internal sealed class BuilderSectionModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? ShowIfField { get; set; }
    public string? ShowIfOperator { get; set; }
    public string? ShowIfValue { get; set; }
    public List<BuilderFieldModel> Fields { get; set; } = [];
}

internal sealed class BuilderFieldModel
{
    public string Key { get; set; } = string.Empty;
    public FieldType Type { get; set; } = FieldType.Text;
    public string Label { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public bool Required { get; set; }
    public string? OptionsText { get; set; }
    public string? CatalogueKey { get; set; }
    public string? ScaleKey { get; set; }
    public string? Min { get; set; }
    public string? Max { get; set; }
    public string? Regex { get; set; }
    public string? MinLength { get; set; }
    public string? MaxLength { get; set; }
    public string? ShowIfField { get; set; }
    public string? ShowIfOperator { get; set; }
    public string? ShowIfValue { get; set; }
}

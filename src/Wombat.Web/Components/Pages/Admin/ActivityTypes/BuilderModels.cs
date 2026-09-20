using System.Text.Json;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

internal sealed class BuilderSchemaModel
{
    public List<BuilderSectionModel> Sections { get; } = [];

    /// <summary>
    /// The root pointer at the field recording when the encounter happened (T119), and the one at the
    /// field carrying the entrustment rating (T126).
    /// </summary>
    /// <remarks>
    /// Carried through the builder round-trip, for the same reason <see cref="BuilderSectionModel.EditableBy" />
    /// is. T119 and T126 both added a root property and neither extended this model, so
    /// <c>ToJson</c> called the two-argument <c>FormSchema</c> constructor and every draft an operator
    /// saved from the Form tab silently erased both (T133). The loss was invisible: the JSON parsed,
    /// the builder rendered, the publish succeeded, and the type quietly stopped dating its activities
    /// by the encounter and stopped saying which ladder it rated on.
    /// </remarks>
    public string? ObservationDateField { get; set; }

    /// <inheritdoc cref="ObservationDateField" />
    public string? RatedLevelField { get; set; }

    public static BuilderSchemaModel Parse(string schemaJson)
    {
        var schema = FormSchemaParser.Parse(schemaJson);
        var model = new BuilderSchemaModel
        {
            ObservationDateField = schema.ObservationDateField,
            RatedLevelField = schema.RatedLevelField
        };

        foreach (var section in schema.Sections)
        {
            model.Sections.Add(new BuilderSectionModel
            {
                Key = section.Key,
                Title = section.Title,
                ShowIfField = section.ShowIf?.Field,
                ShowIfOperator = section.ShowIf?.Operator,
                ShowIfValue = section.ShowIf?.Value,
                EditableBy = section.EditableBy,
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
                    ShowIfValue = field.ShowIf?.Value,
                    EditableBy = field.EditableBy
                }).ToList()
            });
        }

        return model;
    }

    public string ToJson()
    {
        var sections = BuildSections();

        // A pointer whose target no longer exists is DROPPED rather than emitted, and only then.
        // FormSchemaParser refuses an orphaned pointer, ActivityType.SaveDraft runs it, and the Form
        // tab is the only schema-authoring path there is -- so emitting one after the admin deleted
        // its field would make the type permanently unsaveable through the only door. Dropping it is
        // not the silent loss this task fixes: that dropped EVERY pointer on EVERY save. This drops
        // one whose field is gone, and GetPublishWarnings says so. (T133)
        var schema = new FormSchema(
            1,
            sections,
            PointerIfStillValid(sections, ObservationDateField, FieldType.Date),
            PointerIfStillValid(sections, RatedLevelField, FieldType.Scale));

        return FormSchemaParser.Serialize(schema);
    }

    /// <summary>
    /// The pointer when it still names a field of the right type in this draft, otherwise null.
    /// </summary>
    private static string? PointerIfStillValid(
        IReadOnlyList<FormSection> sections,
        string? pointer,
        FieldType requiredType)
    {
        if (string.IsNullOrWhiteSpace(pointer))
        {
            return null;
        }

        var target = sections
            .SelectMany(section => section.Fields)
            .FirstOrDefault(field => string.Equals(field.Key, pointer, StringComparison.Ordinal));

        return target?.Type == requiredType ? pointer : null;
    }

    private List<FormSection> BuildSections()
    {
        return Sections.Select(section => new FormSection(
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
                    BuildVisibility(field.ShowIfField, field.ShowIfOperator, field.ShowIfValue),
                    field.EditableBy))
                .ToList(),
                section.EditableBy))
            .ToList();
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

        // The root pointers, which this diff never mentioned. They are the two settings whose loss is
        // silent by construction -- nothing on the form changes, nothing refuses, and the feature just
        // stops. An operator who deletes a pointed-at field sees the field removal warned above and
        // had no way to know the pointer went with it. (T133)
        AddPointerWarning(warnings, "encounter date", published.ObservationDateField, draft.ObservationDateField);
        AddPointerWarning(warnings, "entrustment rating", published.RatedLevelField, draft.RatedLevelField);

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

    private static void AddPointerWarning(List<string> warnings, string what, string? published, string? draft)
    {
        if (string.Equals(published, draft, StringComparison.Ordinal))
        {
            return;
        }

        if (published is not null && draft is null)
        {
            warnings.Add($"This form will stop recording which field carries the {what}.");
            return;
        }

        if (published is null)
        {
            warnings.Add($"Field '{draft}' will become the {what} for this form.");
            return;
        }

        warnings.Add($"The {what} moves from field '{published}' to '{draft}'.");
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

    /// <summary>
    /// Carried through the builder round-trip verbatim. The builder has no editor for it yet
    /// (T070 step 1 is the DSL only), so preserving the parsed rule is what stops a save from
    /// silently dropping an <c>editable_by</c> authored in the raw JSON.
    /// </summary>
    public ActorRule? EditableBy { get; set; }
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

    /// <summary>
    /// Carried through the builder round-trip verbatim; see <see cref="BuilderSectionModel.EditableBy"/>.
    /// </summary>
    public ActorRule? EditableBy { get; set; }
}

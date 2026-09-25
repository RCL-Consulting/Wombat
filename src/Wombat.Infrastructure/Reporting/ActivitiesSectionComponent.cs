using System.Globalization;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Reporting;

internal static class ActivitiesSectionComponent
{
    /// <param name="workflows">
    /// Each activity's pinned workflow, by pin, or null where it has none that parses: what names its state (T220). A pin
    /// missing from it is treated as null, so the state is printed by its key.
    /// </param>
    /// <param name="references">
    /// What the activities' EPA, person and campaign fields name (T199). Required (T199 review): a caller that left it out
    /// would print every person as "Unknown person" and every EPA by its id. <see cref="PortfolioFieldReferences.Empty" />
    /// is for a caller that knows its activities hold none.
    /// </param>
    public static void Compose(
        IContainer container,
        Dictionary<string, List<Activity>> activitiesByType,
        Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> schemaVersions,
        IReadOnlyDictionary<(int ActivityTypeId, int Version), Workflow?> workflows,
        EntrustmentRungLookup rungLabels,
        PortfolioFieldReferences references)
    {
        ArgumentNullException.ThrowIfNull(references);

        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Activities").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            foreach (var (typeName, activities) in activitiesByType.OrderBy(pair => pair.Key))
            {
                column.Item().Element(e => ComposeTypeGroup(e, typeName, activities, schemaVersions, workflows, rungLabels, references));
            }
        });
    }

    private static void ComposeTypeGroup(
        IContainer container,
        string typeName,
        List<Activity> activities,
        Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> schemaVersions,
        IReadOnlyDictionary<(int ActivityTypeId, int Version), Workflow?> workflows,
        EntrustmentRungLookup rungLabels,
        PortfolioFieldReferences references)
    {
        container.Column(column =>
        {
            column.Spacing(6);

            column.Item().PaddingTop(6).Text(text =>
            {
                text.Span(typeName).Bold().FontSize(11);
                text.Span($"  ({activities.Count})").FontSize(9).FontColor(Colors.Grey.Darken1);
            });

            foreach (var activity in activities)
            {
                column.Item().Element(e => ComposeActivity(e, activity, schemaVersions, workflows, rungLabels, references));
            }
        });
    }

    private static void ComposeActivity(
        IContainer container,
        Activity activity,
        Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> schemaVersions,
        IReadOnlyDictionary<(int ActivityTypeId, int Version), Workflow?> workflows,
        EntrustmentRungLookup rungLabels,
        PortfolioFieldReferences references)
    {
        // T220: the state as the activity's page names it, from the pinned workflow.
        var stateLabel = PinnedWorkflows.StateLabel(
            workflows.GetValueOrDefault((activity.ActivityTypeId, activity.SchemaVersion)), activity.CurrentState);

        container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
        {
            column.Spacing(3);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span($"#{activity.Id}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span($"  State: {stateLabel}").FontSize(8);
                });
                // The encounter date, not the filing date — the column the PDF is also filtered and
                // sorted on, so a reader cannot be shown a row whose printed date sits outside the
                // period on the cover page. (T119)
                //
                // Where nobody stated one (ObservedOnSource.CreatedOn) the date is only the day it was created,
                // and the line says so in the wording every other surface uses (T161, D28). Sized to its
                // text: the qualified date does not fit the fixed 100pt the bare one did.
                row.AutoItem().AlignRight().Text(EncounterDateText(activity))
                    .FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            var key = (activity.ActivityTypeId, activity.SchemaVersion);
            if (schemaVersions.TryGetValue(key, out var version))
            {
                RenderDataFromSchema(column, version.SchemaJson, activity.DataJson, rungLabels, references, activity.ActivityType?.Key);
            }
            else
            {
                RenderDataRaw(column, activity.DataJson);
            }
        });
    }

    /// <summary>
    /// The date printed on an activity's header line: its encounter date, marked when undated (T161), and named as the
    /// encounter's (T197). Beside "#31  State: Completed", a bare "not recorded (created …)" reads as though the
    /// activity were not recorded.
    /// </summary>
    private static string EncounterDateText(Activity activity)
        => "Encounter date: " +
           EncounterDate.Label(activity.ObservedOn, activity.ObservedOnSource == ObservationDateSource.Declared);

    private static void RenderDataFromSchema(
        ColumnDescriptor column,
        string schemaJson,
        string dataJson,
        EntrustmentRungLookup rungLabels,
        PortfolioFieldReferences references,
        string? activityTypeKey)
    {
        FormSchema? schema;
        try
        {
            schema = FormSchemaParser.Parse(schemaJson);
        }
        catch
        {
            RenderDataRaw(column, dataJson);
            return;
        }

        JsonElement data;
        try
        {
            data = JsonDocument.Parse(dataJson).RootElement;
        }
        catch
        {
            return;
        }

        foreach (var section in schema.Sections)
        {
            column.Item().PaddingTop(3).Text(section.Title).FontSize(9).Bold();

            foreach (var field in section.Fields)
            {
                var value = FieldValueText(field, data, rungLabels, references, activityTypeKey);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                column.Item().PaddingLeft(8).Text(text =>
                {
                    text.Span($"{field.Label}: ").FontSize(8).Bold();
                    text.Span(value).FontSize(8);
                });
            }
        }
    }

    private static void RenderDataRaw(ColumnDescriptor column, string dataJson)
    {
        try
        {
            var data = JsonDocument.Parse(dataJson).RootElement;
            if (data.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var property in data.EnumerateObject())
            {
                var value = FormatJsonValue(property.Value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                column.Item().PaddingLeft(8).Text(text =>
                {
                    text.Span($"{property.Name}: ").FontSize(8).Bold();
                    text.Span(value).FontSize(8);
                });
            }
        }
        catch
        {
            // Malformed JSON — skip
        }
    }

    /// <summary>
    /// A field's stored value as the page prints it: a scale's ordinal as its rung, a chosen option by its label,
    /// several by theirs, anything else as stored (T100, T191).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A Scale field holds the ordinal, which is the comparison key and not the rung, so the rung the College prints comes
    /// first (T100). It is read from the stored value, whatever JSON kind carries it: the form stores <c>"3"</c>, other
    /// writers <c>3</c>, and the two must print alike. Only when no rung resolves does the option's label, and then the
    /// ordinal itself, stand in, which is the pre-T100 rendering, not a blank.
    /// </para>
    /// <para>
    /// The form shows an option's label and the stored key never; printing the key here would make the export the one
    /// place a committee reads <c>admission_notes</c>. A value no option of the pinned version declares prints as stored.
    /// </para>
    /// <para>
    /// A field storing an id prints as what the id names: an EPA by its code and title, a person by name, a campaign's
    /// evidence record's campaign as the MSF section heads it (T199). There is no form without the references (T199
    /// review), so no caller can print those ids by leaving them out.
    /// </para>
    /// </remarks>
    internal static string? FieldValueText(
        FormField field,
        JsonElement data,
        EntrustmentRungLookup rungLabels,
        PortfolioFieldReferences references,
        string? activityTypeKey)
    {
        if (!data.TryGetProperty(field.Key, out var element))
        {
            return null;
        }

        if (PortfolioFieldReferences.IsReference(field, activityTypeKey))
        {
            return PortfolioFieldReferences.StoredText(element) is { } stored
                ? references.Label(field, activityTypeKey, stored)
                : null;
        }

        if (field.Type == FieldType.Scale &&
            element.ValueKind is JsonValueKind.Number or JsonValueKind.String &&
            int.TryParse(FormatJsonValue(element), NumberStyles.Integer, CultureInfo.InvariantCulture, out var order))
        {
            var ordinal = order.ToString(CultureInfo.InvariantCulture);
            return rungLabels.Find(rungLabels.ResolveScaleKey(field.ScaleKey), order) ?? field.LabelFor(ordinal);
        }

        if (field.Options.Count == 0)
        {
            return FormatJsonValue(element);
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => field.LabelFor(element.GetString() ?? string.Empty),
            JsonValueKind.Array => string.Join(
                ", ",
                element.EnumerateArray().Select(item => item.ValueKind == JsonValueKind.String
                    ? field.LabelFor(item.GetString() ?? string.Empty)
                    : item.GetRawText())),
            _ => FormatJsonValue(element)
        };
    }

    private static string? FormatJsonValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number => element.GetRawText(),
        JsonValueKind.True => "Yes",
        JsonValueKind.False => "No",
        JsonValueKind.Array => string.Join(", ", element.EnumerateArray().Select(e => e.GetString() ?? e.GetRawText())),
        JsonValueKind.Null => null,
        _ => element.GetRawText()
    };
}

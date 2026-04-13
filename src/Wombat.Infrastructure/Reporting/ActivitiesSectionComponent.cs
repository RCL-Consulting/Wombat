using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Infrastructure.Reporting;

internal static class ActivitiesSectionComponent
{
    public static void Compose(
        IContainer container,
        Dictionary<string, List<Activity>> activitiesByType,
        Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> schemaVersions)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Activities").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            foreach (var (typeName, activities) in activitiesByType.OrderBy(pair => pair.Key))
            {
                column.Item().Element(e => ComposeTypeGroup(e, typeName, activities, schemaVersions));
            }
        });
    }

    private static void ComposeTypeGroup(
        IContainer container,
        string typeName,
        List<Activity> activities,
        Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> schemaVersions)
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
                column.Item().Element(e => ComposeActivity(e, activity, schemaVersions));
            }
        });
    }

    private static void ComposeActivity(
        IContainer container,
        Activity activity,
        Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> schemaVersions)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
        {
            column.Spacing(3);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span($"#{activity.Id}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span($"  State: {activity.CurrentState}").FontSize(8);
                });
                row.ConstantItem(100).AlignRight().Text(
                    activity.CreatedOn.ToString("yyyy-MM-dd")).FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            var key = (activity.ActivityTypeId, activity.SchemaVersion);
            if (schemaVersions.TryGetValue(key, out var version))
            {
                RenderDataFromSchema(column, version.SchemaJson, activity.DataJson);
            }
            else
            {
                RenderDataRaw(column, activity.DataJson);
            }
        });
    }

    private static void RenderDataFromSchema(ColumnDescriptor column, string schemaJson, string dataJson)
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
                var value = GetFieldValue(data, field.Key);
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

    private static string? GetFieldValue(JsonElement data, string fieldKey)
    {
        if (!data.TryGetProperty(fieldKey, out var element))
        {
            return null;
        }

        return FormatJsonValue(element);
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

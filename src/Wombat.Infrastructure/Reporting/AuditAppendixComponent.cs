using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Wombat.Infrastructure.Reporting;

internal static class AuditAppendixComponent
{
    public static void Compose(IContainer container, List<AuditEntry> entries)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Appendix — Audit Trail").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);   // Activity type
                    columns.ConstantColumn(40);   // ID
                    columns.RelativeColumn(1);    // From
                    columns.RelativeColumn(1);    // To
                    columns.RelativeColumn(1);    // Action
                    columns.ConstantColumn(110);  // Date
                });

                table.Header(header =>
                {
                    var headerStyle = TextStyle.Default.FontSize(8).Bold();

                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Padding(3).Text("Type").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Padding(3).Text("ID").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Padding(3).Text("From").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Padding(3).Text("To").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Padding(3).Text("Action").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1)
                        .Padding(3).Text("Date").Style(headerStyle);
                });

                var cellStyle = TextStyle.Default.FontSize(7);

                foreach (var entry in entries)
                {
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                        .Padding(2).Text(entry.ActivityTypeName).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                        .Padding(2).Text(entry.ActivityId.ToString()).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                        .Padding(2).Text(entry.FromState).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                        .Padding(2).Text(entry.ToState).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                        .Padding(2).Text(entry.TransitionKey).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                        .Padding(2).Text(entry.OccurredOn.ToString("yyyy-MM-dd HH:mm")).Style(cellStyle);
                }
            });
        });
    }
}

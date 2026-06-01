using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Infrastructure.Reporting;

/// <summary>
/// Renders the trainee's active Statements of Awarded Responsibility (STARs) — the entrustment
/// decisions issued by committee reviews. This is the headline outcome of a graduation portfolio
/// and was previously omitted from the export entirely (F-5-2 / T077).
/// </summary>
internal static class EntrustmentSummaryComponent
{
    public static void Compose(IContainer container, List<EntrustmentDecision> decisions)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Statements of Awarded Responsibility (STARs)").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
            column.Item().Text($"{decisions.Count} active entrustment decision{(decisions.Count == 1 ? string.Empty : "s")} on record.")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(65);   // EPA code
                    columns.RelativeColumn();      // EPA title
                    columns.ConstantColumn(120);   // Authorised level
                    columns.ConstantColumn(70);    // Issued
                    columns.ConstantColumn(70);    // Expires
                });

                table.Header(header =>
                {
                    var headerStyle = TextStyle.Default.FontSize(8).Bold();
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("EPA").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Title").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Authorised level").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Issued").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Expires").Style(headerStyle);
                });

                var cellStyle = TextStyle.Default.FontSize(8);

                foreach (var decision in decisions)
                {
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(decision.Epa.Code).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(decision.Epa.Title).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(2)
                        .Text($"{decision.AuthorisedLevel.Order}. {decision.AuthorisedLevel.Label}").Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(decision.IssuedOn.ToString("yyyy-MM-dd")).Style(cellStyle);
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(2)
                        .Text(decision.ExpiresOn.HasValue ? decision.ExpiresOn.Value.ToString("yyyy-MM-dd") : "—").Style(cellStyle);
                }
            });
        });
    }
}

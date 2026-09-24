using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Wombat.Infrastructure.Reporting;

internal static class SummaryPageComponent
{
    public static void Compose(IContainer container, PortfolioData data)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Summary").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            column.Item().Text(text =>
            {
                text.Span("Total activities: ").FontSize(10);
                text.Span(data.Activities.Count.ToString()).FontSize(10).Bold();
            });

            if (data.TypeSummaries.Count > 0)
            {
                column.Item().PaddingTop(4).Text("Activities by type:").FontSize(10).Bold();

                foreach (var summary in data.TypeSummaries)
                {
                    // "Complete" is a terminal state of the activity's pinned workflow (T169, D44), counted when the data
                    // was loaded: a discussed reflective exercise or a recorded MSF row is complete, a draft or a declined
                    // request is not. Printed even when it is none, so a reader can tell "none finished" from "not said".
                    column.Item().PaddingLeft(12).Text(text =>
                    {
                        text.Span($"{summary.TypeName}: ").FontSize(9);
                        text.Span($"{summary.Total} total, ").FontSize(9);
                        var complete = text.Span($"{summary.Complete} complete").FontSize(9);
                        if (summary.Complete > 0)
                        {
                            complete.FontColor(Colors.Green.Darken2);
                        }
                    });
                }
            }

            if (data.CommitteeReviews.Count > 0)
            {
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("Committee reviews: ").FontSize(10);
                    text.Span(data.CommitteeReviews.Count.ToString()).FontSize(10).Bold();
                });
            }

            if (data.MsfReports.Count > 0)
            {
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("MSF reports: ").FontSize(10);
                    text.Span(data.MsfReports.Count.ToString()).FontSize(10).Bold();
                });
            }

            column.Item().Height(10);
        });
    }
}

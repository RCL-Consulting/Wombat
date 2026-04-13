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

            if (data.ActivitiesByType.Count > 0)
            {
                column.Item().PaddingTop(4).Text("Activities by type:").FontSize(10).Bold();

                foreach (var (typeName, activities) in data.ActivitiesByType.OrderBy(pair => pair.Key))
                {
                    var completedCount = activities.Count(activity =>
                        string.Equals(activity.CurrentState, "completed", StringComparison.OrdinalIgnoreCase));

                    column.Item().PaddingLeft(12).Text(text =>
                    {
                        text.Span($"{typeName}: ").FontSize(9);
                        text.Span($"{activities.Count} total").FontSize(9);
                        if (completedCount > 0)
                        {
                            text.Span($" ({completedCount} completed)").FontSize(9).FontColor(Colors.Green.Darken2);
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

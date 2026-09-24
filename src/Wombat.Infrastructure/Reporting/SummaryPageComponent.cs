using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;

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

            foreach (var (label, count) in FeedbackReportCounts(data.MsfReports))
            {
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span($"{label}: ").FontSize(10);
                    text.Span(count.ToString(System.Globalization.CultureInfo.InvariantCulture)).FontSize(10).Bold();
                });
            }

            column.Item().Height(10);
        });
    }

    /// <summary>
    /// The released feedback reports, counted per kind: multi-source feedback and learner feedback run on one aggregate
    /// but are two instruments, and a learner-feedback report is not an MSF report (T164). A kind with none is left out.
    /// </summary>
    internal static IReadOnlyList<(string Label, int Count)> FeedbackReportCounts(
        IReadOnlyCollection<MsfCampaignAggregateReportDto> reports)
    {
        var msf = reports.Count(report => report.Kind == MsfTemplateKind.Msf);
        var learnerFeedback = reports.Count(report => report.Kind == MsfTemplateKind.LearnerFeedback);

        return new[] { ("MSF reports", msf), ("Learner feedback reports", learnerFeedback) }
            .Where(entry => entry.Item2 > 0)
            .ToArray();
    }
}

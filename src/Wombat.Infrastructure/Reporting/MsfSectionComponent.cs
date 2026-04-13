using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Reporting;

internal static class MsfSectionComponent
{
    public static void Compose(IContainer container, List<MsfCampaignAggregateReportDto> reports)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Multi-Source Feedback").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            column.Item().PaddingBottom(4).Text(
                "This section contains aggregate reports only. Individual respondent data is never included to protect anonymity.")
                .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);

            foreach (var report in reports)
            {
                column.Item().Element(e => ComposeReport(e, report));
            }
        });
    }

    private static void ComposeReport(IContainer container, MsfCampaignAggregateReportDto report)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(4);

            column.Item().Text(text =>
            {
                text.Span(report.TemplateName).FontSize(10).Bold();
                text.Span($"  (Campaign #{report.CampaignId})").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            column.Item().Text(text =>
            {
                text.Span("Total responses: ").FontSize(9);
                text.Span(report.TotalResponses.ToString()).FontSize(9).Bold();
            });

            if (!string.IsNullOrWhiteSpace(report.CoordinatorNarrative))
            {
                column.Item().PaddingTop(4).Text("Coordinator narrative:").FontSize(9).Bold();
                column.Item().PaddingLeft(8).Text(report.CoordinatorNarrative).FontSize(9);
            }

            foreach (var category in report.Categories)
            {
                column.Item().Element(e => ComposeCategory(e, category));
            }
        });
    }

    private static void ComposeCategory(IContainer container, MsfCategoryAggregateDto category)
    {
        container.PaddingTop(6).Column(column =>
        {
            column.Spacing(3);

            column.Item().Text(text =>
            {
                text.Span($"{category.Category}: ").FontSize(9).Bold();
                text.Span($"{category.ResponseCount} responses").FontSize(9);
            });

            if (category.IsSuppressed)
            {
                column.Item().PaddingLeft(8).Text(
                    "Below minimum threshold — data suppressed to protect anonymity.")
                    .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            foreach (var question in category.Questions)
            {
                column.Item().PaddingLeft(8).Element(e => ComposeQuestion(e, question));
            }
        });
    }

    private static void ComposeQuestion(IContainer container, MsfQuestionAggregateDto question)
    {
        container.Column(column =>
        {
            column.Spacing(2);

            if (question.Scale is not null)
            {
                column.Item().Text(text =>
                {
                    text.Span($"{question.Prompt}: ").FontSize(8);
                    text.Span($"Avg {question.Scale.Average:F1}").FontSize(8).Bold();
                    text.Span($" ({question.Scale.ResponseCount} ratings)").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            }
            else if (question.Comments.Count > 0)
            {
                column.Item().Text($"{question.Prompt}:").FontSize(8).Bold();
                foreach (var comment in question.Comments)
                {
                    column.Item().PaddingLeft(8).Text($"— {comment}").FontSize(8);
                }
            }
        });
    }
}

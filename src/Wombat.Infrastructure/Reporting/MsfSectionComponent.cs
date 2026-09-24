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

            column.Item().Text(SectionTitle(reports)).Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
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

    /// <summary>What the section holds: multi-source feedback, learner feedback (T164), or both.</summary>
    private static string SectionTitle(IReadOnlyCollection<MsfCampaignAggregateReportDto> reports)
    {
        var hasMsf = reports.Any(report => report.Kind == MsfTemplateKind.Msf);
        var hasLearnerFeedback = reports.Any(report => report.Kind == MsfTemplateKind.LearnerFeedback);

        return (hasMsf, hasLearnerFeedback) switch
        {
            (true, true) => "Multi-Source and Learner Feedback",
            (false, true) => "Learner Feedback",
            _ => "Multi-Source Feedback"
        };
    }

    /// <summary>
    /// An EPA code, saying so when the campaign declared it but recorded no evidence for it. It gives no
    /// reason: a missing evidence row does not show why it is missing (T186).
    /// </summary>
    private static string DescribeCoveredEpa(MsfCoveredEpaDto epa)
        => epa.Recorded ? epa.Code : $"{epa.Code} (not recorded)";

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

            // T164: learner feedback is run on the same aggregate but is another instrument, and the section prints both.
            column.Item().Text(MsfEvidenceKinds.Describe(report.Kind)).FontSize(8).FontColor(Colors.Grey.Darken1);

            column.Item().Text(text =>
            {
                text.Span("Total responses: ").FontSize(9);
                text.Span(report.TotalResponses.ToString()).FontSize(9).Bold();
            });

            // T164: EPA 15's "at least two teaching contexts" is counted here, and reported rather than judged. The count
            // only: the portfolio is the trainee's, and a context's name beside a handful of answers can say which learner
            // wrote which. The evidence records printed with it carry the count alone for the same reason.
            if (report.TeachingContextCount is int contextCount)
            {
                column.Item().Text(text =>
                {
                    text.Span("Teaching contexts that responded: ").FontSize(9);
                    text.Span(contextCount.ToString(System.Globalization.CultureInfo.InvariantCulture)).FontSize(9).Bold();
                });
            }

            // T121: what the campaign was declared evidence for. Printed here as well as on the
            // per-EPA activity records in the activities section, because those are separate entries
            // several pages away and nothing else says they came from this campaign.
            if (report.CoveredEpas.Count > 0)
            {
                column.Item().Text(text =>
                {
                    text.Span("Evidence for: ").FontSize(9);
                    text.Span(string.Join(", ", report.CoveredEpas.Select(DescribeCoveredEpa))).FontSize(9).Bold();
                });
            }

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

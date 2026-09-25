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
                text.Span(report.TotalResponses.ToString(System.Globalization.CultureInfo.InvariantCulture)).FontSize(9).Bold();
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

            // T249: a group below the category threshold is left out, label and count, as the trainee's web copy
            // (/msf/my-reports) leaves it out: an exact count under a group's name is what the threshold hides. One line,
            // printed once, says why the groups printed add up to less than the total, and names none it left out. Unlike
            // the web copy, the PDF prints each printed group's count, so the total less those counts is still the hidden
            // groups' count together, and one hidden group's own. That says at most who took part, never what anyone
            // answered, and it is accepted (DESIGN.md, "The trainee's copies of a released report"; T249 review).
            foreach (var category in report.Categories.Where(category => !category.IsSuppressed))
            {
                column.Item().Element(e => ComposeCategory(e, category));
            }

            if (report.Categories.Any(category => category.IsSuppressed))
            {
                column.Item().PaddingTop(4).Text(SuppressedGroupsLine(report.MinimumCategoryResponses))
                    .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
            }
        });
    }

    /// <summary>
    /// Why a report's groups add up to less than its total: some were below the category threshold. Says how many a group
    /// needed, never which group or how many it had (T249).
    /// </summary>
    private static string SuppressedGroupsLine(int minimumCategoryResponses)
        => $"Respondent groups with fewer than {Counted(minimumCategoryResponses, "response", "responses")} are not " +
           "shown, to protect the respondents' anonymity.";

    /// <summary>A count with its noun, singular at one: "1 response", "2 responses" (T249).</summary>
    private static string Counted(int count, string one, string many)
        => $"{count.ToString(System.Globalization.CultureInfo.InvariantCulture)} {(count == 1 ? one : many)}";

    /// <summary>A group that cleared the category threshold: its label, its count and its answers.</summary>
    private static void ComposeCategory(IContainer container, MsfCategoryAggregateDto category)
    {
        container.PaddingTop(6).Column(column =>
        {
            column.Spacing(3);

            column.Item().Text(text =>
            {
                // By the group's label, never its key (T225): "Peer doctor", not "PeerDoctor".
                text.Span($"{MsfRespondentCategories.Describe(category.Category)}: ").FontSize(9).Bold();
                text.Span(Counted(category.ResponseCount, "response", "responses")).FontSize(9);
            });

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
                    // Invariant, as the section's other numbers: "4.0" on any host, so an export is the same bytes (T078).
                    text.Span($"Avg {question.Scale.Average.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}")
                        .FontSize(8).Bold();
                    text.Span($" ({Counted(question.Scale.ResponseCount, "rating", "ratings")})").FontSize(8).FontColor(Colors.Grey.Darken1);
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

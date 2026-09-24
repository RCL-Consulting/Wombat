using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Reporting;

internal static class CommitteeSectionComponent
{
    public static void Compose(
        IContainer container,
        List<CommitteeReview> reviews,
        IReadOnlyDictionary<string, string> attendeeNames)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Committee Decisions").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            foreach (var review in reviews)
            {
                column.Item().Element(e => ComposeReview(e, review, attendeeNames));
            }
        });
    }

    /// <summary>
    /// Who was recorded as present when the review's current decision was taken, chair first, each by name ("Thandi Zulu
    /// (chair), Priya Naidoo"), or null when nobody was recorded. (T165)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The current decision's own sitting, printed beside that decision: a decision an appeal remitted was taken by
    /// whoever sat for the appeal, not by the review's first sitting.
    /// </para>
    /// <para>
    /// A name the map does not hold prints as the id, which is what a reader is shown for a person with no name on record
    /// elsewhere (T142); a blank would read as nobody.
    /// </para>
    /// </remarks>
    internal static string? PresentLine(CommitteeReview review, IReadOnlyDictionary<string, string> attendeeNames)
    {
        var attendees = review.GetCurrentDecision()?.Attendees;
        if (attendees is null || attendees.Count == 0)
        {
            return null;
        }

        return string.Join(", ", attendees
            .OrderBy(attendee => attendee.Role)
            .ThenBy(attendee => attendee.UserId, StringComparer.Ordinal)
            .Select(attendee =>
            {
                var name = attendeeNames.TryGetValue(attendee.UserId, out var found) ? found : attendee.UserId;
                return attendee.Role switch
                {
                    DecisionPanelMemberRole.Chair => $"{name} (chair)",
                    DecisionPanelMemberRole.External => $"{name} (external)",
                    _ => name
                };
            }));
    }

    private static void ComposeReview(IContainer container, CommitteeReview review, IReadOnlyDictionary<string, string> attendeeNames)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(column =>
        {
            column.Spacing(4);

            column.Item().Text(text =>
            {
                text.Span("Review Period: ").FontSize(9).Bold();
                text.Span($"{review.ReviewPeriodFrom:yyyy-MM-dd} to {review.ReviewPeriodTo:yyyy-MM-dd}").FontSize(9);
            });

            column.Item().Text(text =>
            {
                text.Span("Scheduled: ").FontSize(9).Bold();
                text.Span(review.ScheduledOn.ToString("yyyy-MM-dd")).FontSize(9);
            });

            column.Item().Text(text =>
            {
                text.Span("Panel: ").FontSize(9).Bold();
                text.Span(review.Panel.Name).FontSize(9);
            });

            column.Item().Text(text =>
            {
                text.Span("Status: ").FontSize(9).Bold();
                text.Span(review.State.ToString()).FontSize(9);
            });

            var currentDecision = review.GetCurrentDecision();
            if (currentDecision is not null)
            {
                column.Item().PaddingTop(4).Text(text =>
                {
                    text.Span("Decision: ").FontSize(9).Bold();
                    text.Span(FormatCategory(currentDecision.Category)).FontSize(9);
                });

                column.Item().Text(text =>
                {
                    text.Span("Rationale: ").FontSize(9).Bold();
                    text.Span(currentDecision.Rationale).FontSize(9);
                });

                if (!string.IsNullOrWhiteSpace(currentDecision.Conditions))
                {
                    column.Item().Text(text =>
                    {
                        text.Span("Conditions: ").FontSize(9).Bold();
                        text.Span(currentDecision.Conditions).FontSize(9);
                    });
                }
            }

            if (PresentLine(review, attendeeNames) is { } present)
            {
                column.Item().Text(text =>
                {
                    text.Span("Present: ").FontSize(9).Bold();
                    text.Span(present).FontSize(9);
                });
            }

            if (review.RatifiedOn.HasValue)
            {
                column.Item().Text(text =>
                {
                    text.Span("Ratified: ").FontSize(9).Bold();
                    text.Span(review.RatifiedOn.Value.ToString("yyyy-MM-dd")).FontSize(9);
                });
            }
        });
    }

    private static string FormatCategory(CommitteeDecisionCategory category) => category switch
    {
        CommitteeDecisionCategory.SatisfactoryProgress => "Satisfactory Progress",
        CommitteeDecisionCategory.SatisfactoryWithObservations => "Satisfactory with Observations",
        CommitteeDecisionCategory.InadequateProgressAdditionalTraining => "Inadequate Progress — Additional Training",
        CommitteeDecisionCategory.InadequateProgressRepeat => "Inadequate Progress — Repeat",
        CommitteeDecisionCategory.ReleaseFromTraining => "Release from Training",
        CommitteeDecisionCategory.OutcomeDeferred => "Outcome Deferred",
        CommitteeDecisionCategory.Graduate => "Graduate (programme complete)",
        _ => category.ToString()
    };
}

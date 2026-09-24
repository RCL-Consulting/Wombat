using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Reporting;

/// <summary>
/// "Progress per EPA" (T169): for each EPA of the trainee's curriculum, the target and the count in every period the
/// export covers, and the rated observations the entrustment trajectory draws for it. The figures are the progress
/// page's, and so are its distinctions: a closed period is met or short, a period still running is a count so far with
/// what is still to do by when, and a period the trainee was not held to says why.
/// </summary>
/// <remarks>
/// One row per EPA, with the figures as labelled lines in the right-hand cell. [T166]'s entrustment standing and
/// [T168]'s MSF coverage are each one more line in <see cref="ComposeDetails" />.
/// </remarks>
internal static class EpaProgressSectionComponent
{
    public static void Compose(IContainer container, PortfolioEpaProgress progress)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Text("Progress per EPA").Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
            column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);

            column.Item().Text(Introduction(progress)).FontSize(8).FontColor(Colors.Grey.Darken1);

            if (progress.Rows.Count == 0)
            {
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(130);   // EPA
                    columns.RelativeColumn();      // Targets and evidence
                });

                table.Header(header =>
                {
                    var headerStyle = TextStyle.Default.FontSize(8).Bold();
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("EPA").Style(headerStyle);
                    header.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Targets and evidence").Style(headerStyle);
                });

                foreach (var row in progress.Rows)
                {
                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(3).Column(epa =>
                    {
                        epa.Item().Text(row.EpaCode).FontSize(8).Bold();
                        epa.Item().Text(row.EpaTitle).FontSize(7).FontColor(Colors.Grey.Darken1);
                    });

                    table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3).Padding(3)
                        .Element(cell => ComposeDetails(cell, row, progress));
                }
            });
        });
    }

    /// <summary>What the section reads, for which programme and on which day, before any figure.</summary>
    internal static string Introduction(PortfolioEpaProgress progress)
    {
        const string ratings = "Rated observations are those the trainee's entrustment trajectory draws within this " +
                               "export's dates: finished, and rated by a named assessor.";

        if (progress.Targets is null)
        {
            return $"The trainee has no training programme, so no targets apply. {ratings}";
        }

        var day = QuotaText.LongDate(progress.AsOf);
        var stage = progress.Targets.TraineeStage is { } year ? $" (training year {year})" : string.Empty;
        var programme = progress.Programme switch
        {
            { State: PortfolioProgrammeState.Completed, CompletedOn: { } completedOn } =>
                $"The trainee completed the programme on {QuotaText.LongDate(completedOn)}. Targets as on {day}{stage}.",
            { State: PortfolioProgrammeState.Inactive } =>
                $"The trainee's programme is inactive: it was ended without being completed, on a day Wombat does not " +
                $"record. Targets as on {day}{stage}, so periods after it ended are listed too.",
            _ => $"Targets as on the trainee's progress page on {day}{stage}."
        };
        var reach = progress.FromDate is { } from
            ? $"back to the one containing {QuotaText.LongDate(from)}"
            : "back to the start of the programme";

        // The counts are the progress page's: every encounter credited in a whole period, whoever may read it. The
        // activity list below is cut on this export's dates and on the reader's scope, so the two can differ, and the
        // sentence says so rather than leaving a reader to find the gap. The counts are as they stand today, so a period
        // that has not ended is a count so far, not a result.
        return $"{programme} Each period's count is every encounter credited in that whole semester or academic year, " +
               $"newest period first, {reach}, so it can include encounters this export does not list. A period that " +
               $"has not ended by {QuotaText.LongDate(progress.Today)} shows its count so far. {ratings}";
    }

    private static void ComposeDetails(IContainer container, PortfolioEpaProgressRow row, PortfolioEpaProgress progress)
    {
        container.Column(lines =>
        {
            lines.Spacing(1);

            foreach (var (label, text) in DetailLines(row, progress))
            {
                lines.Item().Text(span =>
                {
                    span.Span($"{label}: ").FontSize(8).Bold();
                    span.Span(text).FontSize(8);
                });
            }
        });
    }

    /// <summary>
    /// A row's labelled lines, in print order: the target, one line per period (or a line saying the export's dates
    /// cover none), and the rated observations. A row with no target says why instead of the first two.
    /// </summary>
    internal static IReadOnlyList<(string Label, string Text)> DetailLines(
        PortfolioEpaProgressRow row, PortfolioEpaProgress progress)
    {
        var lines = new List<(string Label, string Text)>();

        if (row.Item is { } item)
        {
            lines.Add(("Target", TargetLine(item, progress.Targets?.TraineeStage)));
            if (row.Periods.Count == 0)
            {
                lines.Add(("Periods", "none within this export's dates"));
            }

            lines.AddRange(row.Periods.Select(period =>
                (period.Name, PeriodLine(period, progress.Today, progress.Programme.IsActive))));
        }
        else
        {
            lines.Add(("Target", NoTargetLine(row.NoTarget)));
        }

        lines.Add(("Rated observations", RatingsLine(row.Ratings)));
        return lines;
    }

    /// <summary>"3 per semester (6 a year), minimum 3a in training year 2": the progress page's target line.</summary>
    internal static string TargetLine(TraineeCurriculumProgressDto item, int? trainingYear)
    {
        var perYear = item.IsPerSemester ? $" ({item.Target * 2} a year)" : string.Empty;
        var when = trainingYear is { } year ? $" in training year {year}" : string.Empty;
        return $"{QuotaText.TargetPhrase(item.QuotaPeriod, item.Target)}{perYear}, minimum {item.EffectiveMinimumLevelLabel}{when}";
    }

    /// <summary>Why a row with rated evidence has no target.</summary>
    internal static string NoTargetLine(PortfolioNoTargetReason? reason) => reason switch
    {
        PortfolioNoTargetReason.NoProgramme => "none: the trainee has no training programme",
        PortfolioNoTargetReason.EpaDeactivated => "none: this EPA has been deactivated, so it is no longer a target",
        _ => "none: this EPA is not in the trainee's curriculum"
    };

    /// <summary>
    /// One period, in the progress page's words (<c>MyProgress</c>). A period that ended before
    /// <paramref name="today" /> is met or short (<c>PreviousLine</c>). One still running is a count so far and, while
    /// the programme is in progress, what is still to do by when: the College's last day, or in December, when the
    /// College's year is over but encounters still count (D40). A waived period (D14) and one before the programme
    /// started say so.
    /// </summary>
    /// <param name="programmeActive">
    /// False for a completed or deactivated programme: the trainee owes nothing more, so a running period is its count
    /// so far and nothing else.
    /// </param>
    internal static string PeriodLine(QuotaWindowDto period, DateOnly today, bool programmeActive)
    {
        switch (period.Status)
        {
            case QuotaWindowStatus.NotStarted:
                return period.FirstCountedName is { } first
                    ? $"no target yet; targets start with {first}"
                    : "no target yet";

            case QuotaWindowStatus.ExemptPartialPeriod:
                return $"no target (started part-way through), {period.Count} recorded";

            default:
                var atMinimum = period.Count > 0
                    ? $"; {period.MinimumLevelReachedCount} at the minimum level when observed"
                    : string.Empty;
                return $"{period.Count} of {period.Target}{Standing(period, today, programmeActive)}{atMinimum}";
        }
    }

    private static string Standing(QuotaWindowDto period, DateOnly today, bool programmeActive)
    {
        // The window's last counted day: 31 December when it ends in semester 2, a month after the College's own last
        // day, because December encounters count towards it (D40).
        var lastCountedDay = AcademicPeriod.Containing(period.NominalEnd).End;
        if (today > lastCountedDay)
        {
            return period.IsMet ? ", met" : $", {period.Shortfall} short";
        }

        if (period.IsMet)
        {
            return " so far, met";
        }

        if (!programmeActive)
        {
            return " so far";
        }

        return today > period.NominalEnd
            ? $" so far; {period.Shortfall} more, and encounters in December still count towards {period.Name}"
            : $" so far; {period.Shortfall} more by {QuotaText.LongDate(period.NominalEnd)}";
    }

    /// <summary>"4 from 2 assessors; latest 3a, 2026-08-12", or "none in this export".</summary>
    internal static string RatingsLine(PortfolioEpaRatings ratings)
    {
        if (ratings.Observations == 0)
        {
            return "none in this export";
        }

        var assessors = ratings.DistinctAssessors == 1 ? "1 assessor" : $"{ratings.DistinctAssessors} assessors";
        // An off-ladder point was rated on a different scale from this EPA's, so its label is not a rung here (T123 D30).
        var scale = ratings.LatestOffLadder ? " (on a different scale)" : string.Empty;
        return $"{ratings.Observations} from {assessors}; latest {ratings.LatestLabel}{scale}, {ratings.LatestObservedOn}";
    }
}

using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>
/// The words, addresses and actions of the decisions-due page (T131 slice 6; T364, flow 08). Sentences are built here in
/// C#, because Razor drops a space standing alone before an expression and runs sentences together.
/// </summary>
public static class DecisionsDueText
{
    // ---- The page's fixed words (R2-3 boards) ------------------------------------------------------------------------

    /// <summary>The subtitle: no "each semester", since the annual EPAs are decided once a year (T364, S4).</summary>
    public const string Subtitle = "What the committee must decide, per registrar and per EPA, and where each decision stands.";

    /// <summary>Period's help (T364, F6): why semester 2's window runs to 31 December; the schedule page's words, one copy.</summary>
    public const string PeriodHelp = ScheduleWords.Semester2WindowNote;

    /// <summary>Said while the page reads (C11), in the page's one status line.</summary>
    public const string Loading = "Loading Decisions due.";

    /// <summary>An unexpected failure, in fixed words (T338, T329); the failure itself goes to the log.</summary>
    public const string LoadFailed =
        "Could not load Decisions due. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>An Administrator's institution list could not be read (T338): the page says so, with Try again.</summary>
    public const string InstitutionsLoadFailed =
        "Could not load the institutions to choose from. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>An Administrator belongs to no institution, so the opening says the page waits for one.</summary>
    public const string AdministratorIntro =
        "An administrator belongs to no institution, so the decisions due are shown for the one chosen above.";

    public const string ChooseInstitutionTitle = "Choose an institution";

    public const string ChooseInstitutionBody = "Choose an institution above, then Show, to read what its committees must decide.";

    public const string ByEpaHeading = "By EPA";

    public const string ByEpaRule = "The filters above do not narrow these counts.";

    public const string ListHeading = "Decisions";

    public const string NoMatch = "No decision matches these filters.";

    public const string Show = "Show";

    public const string ClearFilters = "Clear filters";

    public const string EveryTrainee = "Every trainee";

    public const string EveryEpa = "Every EPA";

    public const string OutstandingOption = "Outstanding: not yet decided";

    public const string EveryStatusOption = "Every status";

    /// <summary>The pager's page-size label (T280; C9).</summary>
    public const string PageSizeLabel = "Decisions per page";

    /// <summary>The pager's landmark name.</summary>
    public const string PagerLabel = "Decisions, pages";

    // ---- The address: the filters applied with Show (DESIGN.md § List page; T364, F2) --------------------------------

    public const string Route = "/committee/decisions-due";

    public const string InstitutionKey = "institution";

    public const string PeriodKey = "period";

    public const string TraineeKey = "trainee";

    public const string StatusKey = "status";

    public const string EpaKey = "epa";

    /// <summary>The Status filter's default: every row nothing yet decides.</summary>
    public const string OutstandingValue = "outstanding";

    /// <summary>The Status filter's "Every status".</summary>
    public const string EveryStatusValue = "all";

    /// <summary>
    /// The page's address with what was asked as its query, each part left out when it is the default or not given:
    /// <c>/committee/decisions-due?period=2026-1&amp;trainee=…&amp;status=Missed&amp;epa=4</c>. What Show navigates to,
    /// what Clear filters keeps (the institution and the period: what is read, not how it is narrowed), and what a
    /// Schedule link's <c>return=</c> carries so Cancel comes back with the filters (C4).
    /// </summary>
    public static string Address(int? institutionId, string? periodKey, string? traineeUserId, string? status, int? epaId)
    {
        var parts = new List<string>(5);
        if (institutionId is int institution)
        {
            parts.Add(string.Create(CultureInfo.InvariantCulture, $"{InstitutionKey}={institution}"));
        }

        if (!string.IsNullOrEmpty(periodKey))
        {
            parts.Add($"{PeriodKey}={Uri.EscapeDataString(periodKey)}");
        }

        if (!string.IsNullOrEmpty(traineeUserId))
        {
            parts.Add($"{TraineeKey}={Uri.EscapeDataString(traineeUserId)}");
        }

        if (!string.IsNullOrEmpty(status) && !string.Equals(status, OutstandingValue, StringComparison.Ordinal))
        {
            parts.Add($"{StatusKey}={Uri.EscapeDataString(status)}");
        }

        if (epaId is int epa)
        {
            parts.Add(string.Create(CultureInfo.InvariantCulture, $"{EpaKey}={epa}"));
        }

        return parts.Count == 0 ? Route : $"{Route}?{string.Join('&', parts)}";
    }

    // ---- Statuses ------------------------------------------------------------------------------------------------------

    /// <summary>The status filter's options, what still needs someone to act first and what is settled last (S4: kept).</summary>
    public static IReadOnlyList<EntrustmentDecisionDueStatus> FilterOrder { get; } =
    [
        EntrustmentDecisionDueStatus.Missed,
        EntrustmentDecisionDueStatus.NotScheduled,
        EntrustmentDecisionDueStatus.Revoked,
        EntrustmentDecisionDueStatus.DueByYearEnd,
        EntrustmentDecisionDueStatus.NotDecided,
        EntrustmentDecisionDueStatus.Scheduled,
        EntrustmentDecisionDueStatus.PartialPeriod,
        EntrustmentDecisionDueStatus.AsOpportunityAllows,
        EntrustmentDecisionDueStatus.Decided
    ];

    /// <summary>A status as its badge says it.</summary>
    public static string StatusLabel(EntrustmentDecisionDueStatus status) => status switch
    {
        EntrustmentDecisionDueStatus.Decided => "Decided",
        EntrustmentDecisionDueStatus.Scheduled => "Scheduled",
        EntrustmentDecisionDueStatus.NotDecided => "Not decided",
        EntrustmentDecisionDueStatus.Revoked => "Revoked: re-decide",
        EntrustmentDecisionDueStatus.DueByYearEnd => "Due by year end",
        EntrustmentDecisionDueStatus.PartialPeriod => "Partial period",
        EntrustmentDecisionDueStatus.AsOpportunityAllows => "As opportunity allows",
        EntrustmentDecisionDueStatus.NotScheduled => "Not scheduled",
        EntrustmentDecisionDueStatus.Missed => "Missed",
        _ => status.ToString()
    };

    /// <summary>
    /// A status sentence, the one review it names apart so the page can link it (C3: "Not decided at review #1." with
    /// "review #1" a link where the caller may open it).
    /// </summary>
    /// <param name="Before">The words before the review.</param>
    /// <param name="ReviewId">The review named, or null when the sentence names none to link.</param>
    /// <param name="ReviewText">"review #1", or empty.</param>
    /// <param name="After">The words after it.</param>
    /// <param name="MayOpen">Whether the caller may open that review: the page links it only then.</param>
    public sealed record DueSentence(string Before, int? ReviewId, string ReviewText, string After, bool MayOpen)
    {
        /// <summary>The sentence as one string.</summary>
        public string Text => Before + ReviewText + After;

        /// <summary>A sentence that links nothing.</summary>
        public static DueSentence Plain(string text) => new(text, null, string.Empty, string.Empty, false);
    }

    /// <summary>What a status means for the trainee, said under its badge, with the review it names apart.</summary>
    public static DueSentence Sentence(EntrustmentDecisionDueDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var star = item.EntrustmentDecisionId is int starId ? $"STAR #{starId}" : "The decision";
        return item.Status switch
        {
            EntrustmentDecisionDueStatus.Decided => item.ReviewId is int decidedAt
                ? Named(item, $"{star}, issued at ", decidedAt, ".")
                : DueSentence.Plain($"{star} stands for {item.WindowLabel}."),
            // T259: the review's own panel.
            EntrustmentDecisionDueStatus.Scheduled => item.ReviewId is int scheduledAt
                ? Named(item, "On the agenda of ", scheduledAt,
                    (item.ReviewPanelName is { } panel ? $" before {panel}" : string.Empty) + ", which is still open.")
                : DueSentence.Plain("On the agenda of an open review."),
            // T361, D53: a ratified review left it so; there is nothing to reinstate.
            EntrustmentDecisionDueStatus.NotDecided => item.ReviewId is int notDecidedAt
                ? Named(item, "Not decided at ", notDecidedAt,
                    $". A later sitting for {item.WindowLabel} plans it again." + HoldingSentence(item))
                : DueSentence.Plain($"Not decided. A later sitting for {item.WindowLabel} plans it again." + HoldingSentence(item)),
            EntrustmentDecisionDueStatus.Revoked => DueSentence.Plain(item.HoldingReviewId is null
                ? $"{star}, issued for {item.WindowLabel}, was revoked. Schedule a review to decide it again."
                : $"{star}, issued for {item.WindowLabel}, was revoked." + HoldingSentence(item)),
            EntrustmentDecisionDueStatus.DueByYearEnd => DueSentence.Plain(
                $"Decided once a year: due by the last sitting of {item.WindowLabel}, and not missed before the year ends." +
                HoldingSentence(item)),
            EntrustmentDecisionDueStatus.PartialPeriod => DueSentence.Plain(
                "The trainee joined the window part-way through: optional, never missed." + HoldingSentence(item)),
            EntrustmentDecisionDueStatus.AsOpportunityAllows => DueSentence.Plain(
                "Decided as opportunity allows: optional, never missed." + HoldingSentence(item)),
            // M2: an annual EPA's window is the year, so its row reads "2026 has not ended".
            EntrustmentDecisionDueStatus.NotScheduled => DueSentence.Plain(item.HoldingReviewId is null
                ? $"No open review holds it, and {item.WindowLabel} has not ended."
                : $"{item.WindowLabel} has not ended, and no open review's agenda holds it." + HoldingSentence(item)),
            EntrustmentDecisionDueStatus.Missed => DueSentence.Plain(
                $"{item.WindowLabel} has ended with nothing decided or on an open review's agenda." + HoldingSentence(item)),
            _ => DueSentence.Plain(string.Empty)
        };
    }

    /// <summary>The status sentence as one string.</summary>
    public static string Detail(EntrustmentDecisionDueDto item) => Sentence(item).Text;

    private static DueSentence Named(EntrustmentDecisionDueDto item, string before, int reviewId, string after)
        => new(before, reviewId, $"review #{reviewId}", after, item.MayOpenReview);

    /// <summary>
    /// Where a row whose seat an open review already holds is to be decided, in the scheduling refusal's terms: a second
    /// binding review for the period would be refused, so the one open is where it goes. It names the panel the holding
    /// review sits before (T259). Empty where none holds it.
    /// </summary>
    private static string HoldingSentence(EntrustmentDecisionDueDto item)
        => item.HoldingReviewId is int holding
            ? $" Review #{holding} is open for {item.SchedulePeriodLabel} before {item.HoldingPanelName ?? item.SchedulePanelName}: " +
              "decide it there, or ratify that review before scheduling another."
            : string.Empty;

    // ---- A row's one action (C3) -------------------------------------------------------------------------------------

    /// <summary>What a row's action cell holds: one link, or a note saying why there is none.</summary>
    public enum RowActionKind
    {
        Note = 0,
        OpenReview = 1,
        Schedule = 2
    }

    /// <summary>A row's one action (C3): Open review #n, Schedule, or a note saying why there is none.</summary>
    public sealed record RowAction(RowActionKind Kind, int? ReviewId, string Text);

    /// <summary>
    /// The row's one action, always one (C3): a decided row says it needs nothing; a scheduled row opens its review; a row
    /// whose seat an open review holds opens that review, since Schedule would be refused; any other row schedules, or says
    /// why it cannot.
    /// </summary>
    public static RowAction ActionFor(EntrustmentDecisionDueDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Status == EntrustmentDecisionDueStatus.Decided)
        {
            return new RowAction(RowActionKind.Note, null, "Decided: nothing to schedule");
        }

        if (item.Status == EntrustmentDecisionDueStatus.Scheduled)
        {
            return item is { ReviewId: int reviewId, MayOpenReview: true }
                ? new RowAction(RowActionKind.OpenReview, reviewId, $"Open review #{reviewId}")
                : new RowAction(RowActionKind.Note, null, "Scheduled: nothing to schedule");
        }

        if (item.HoldingReviewId is int holdingId)
        {
            return item.MayOpenHoldingReview
                ? new RowAction(RowActionKind.OpenReview, holdingId, $"Open review #{holdingId}")
                : new RowAction(RowActionKind.Note, null, $"Review #{holdingId} holds it: nothing to schedule");
        }

        return item.SchedulePanelId is null
            ? new RowAction(RowActionKind.Note, null, "No panel you can schedule before decides it.")
            : new RowAction(RowActionKind.Schedule, null, "Schedule");
    }

    /// <summary>
    /// What a row's Schedule link's accessible name ends with: ", for 2026 S1" when the period has ended (F1), since the
    /// form then opens on a period already over; nothing otherwise. The name before it is the trainee, the EPA and the
    /// panel ("Schedule a review of Anele Dlamini for PAED-001 before Paed Annual Review Panel").
    /// </summary>
    public static string EndedPeriod(EntrustmentDecisionDueDto item, bool periodHasEnded)
        => periodHasEnded ? $", for {item.SchedulePeriodLabel}" : string.Empty;

    // ---- The opening, the count, the caption ---------------------------------------------------------------------------

    /// <summary>The opening sentence: whose decisions, where, for which period, and what "missed" means.</summary>
    public static string Intro(EntrustmentDecisionsDueDto result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var trainees = result.TraineeCount == 1 ? "1 trainee" : $"{result.TraineeCount} trainees";
        return $"The EPAs due for an entrustment decision in {result.PeriodLabel} for the {trainees} you oversee at " +
               $"{result.InstitutionName}, by each EPA's decision cadence: every semester, or once a year. " +
               "Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda.";
    }

    /// <summary>By EPA's fold: "Counts for the 15 EPAs due in 2026 S2".</summary>
    public static string ByEpaFold(EntrustmentDecisionsDueDto result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var count = result.ByEpa.Count;
        return $"Counts for the {count} EPA{(count == 1 ? string.Empty : "s")} due in {result.PeriodLabel}";
    }

    /// <summary>By EPA's caption: what the last three columns count.</summary>
    public static string ByEpaCaption(EntrustmentDecisionsDueDto result)
        => $"How many trainees stand where on each EPA due in {result.PeriodLabel}. To schedule: not scheduled, due by year " +
           "end, or revoked. Optional: a partial period, or decided as opportunity allows.";

    /// <summary>By EPA's scroll region's name.</summary>
    public static string ByEpaRegion(EntrustmentDecisionsDueDto result) => $"By EPA, {result.PeriodLabel}, scrollable";

    /// <summary>What the Decisions heading adds while the list has more than one page: ", page 2 of 4" (C9, T264).</summary>
    public static string PageOf(int page, int pages) => $", page {page} of {pages}";

    /// <summary>What was asked, as the page reads it back: the trainee's name and the EPA's code where they are set.</summary>
    /// <param name="TraineeName">The filtered trainee's name, or null for every trainee.</param>
    /// <param name="Status"><see cref="OutstandingValue" />, <see cref="EveryStatusValue" />, or a status's name.</param>
    /// <param name="EpaCode">The filtered EPA's code, or null for every EPA.</param>
    public sealed record Asked(string? TraineeName, string Status, string? EpaCode)
    {
        /// <summary>Whether anything narrows the list beyond the default: Clear filters is offered then.</summary>
        public bool IsFiltered => TraineeName is not null || EpaCode is not null ||
                                  !string.Equals(Status, OutstandingValue, StringComparison.Ordinal);
    }

    /// <summary>
    /// The count under the Decisions heading, a status region: "75 of 75 decisions due in 2026 S2 shown.", then what was
    /// asked where anything narrows it ("15 of 75 … shown: Lerato Molefe, every status."; "5 of 75 … shown: Not scheduled,
    /// PAED-004."). Nothing matching reads the bare count; the card under it says what was asked.
    /// </summary>
    public static string Count(EntrustmentDecisionsDueDto result, int shown, Asked asked)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(asked);

        var total = result.Items.Count;
        var count = $"{shown} of {total} decision{(total == 1 ? string.Empty : "s")} due in {result.PeriodLabel} shown";
        var parts = AskedParts(asked);
        return shown == 0 || parts.Count == 0 ? $"{count}." : $"{count}: {string.Join(", ", parts)}.";
    }

    /// <summary>
    /// The list's caption: "Decisions due in 2026 S2, by trainee then EPA"; "Decisions due in 2026 S2: Not scheduled,
    /// PAED-004"; with a trainee, "Lerato Molefe's decisions due in 2026 S2, every status, by EPA".
    /// </summary>
    public static string Caption(EntrustmentDecisionsDueDto result, Asked asked)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(asked);

        if (asked.TraineeName is { } name)
        {
            var epa = asked.EpaCode is { } code ? $", {code}" : string.Empty;
            return $"{name}'s decisions due in {result.PeriodLabel}, {StatusWords(asked.Status)}{epa}, by EPA";
        }

        var parts = AskedParts(asked);
        return parts.Count == 0
            ? $"Decisions due in {result.PeriodLabel}, by trainee then EPA"
            : $"Decisions due in {result.PeriodLabel}: {string.Join(", ", parts)}";
    }

    /// <summary>What was asked, under "No decision matches these filters.": "Missed only, every trainee, every EPA."</summary>
    public static string AskedSentence(Asked asked)
    {
        ArgumentNullException.ThrowIfNull(asked);

        var status = asked.Status switch
        {
            OutstandingValue => "Not yet decided",
            EveryStatusValue => "Every status",
            _ => Enum.TryParse<EntrustmentDecisionDueStatus>(asked.Status, out var parsed) ? $"{StatusLabel(parsed)} only" : asked.Status
        };
        return $"{status}, {asked.TraineeName ?? "every trainee"}, {asked.EpaCode ?? "every EPA"}.";
    }

    private static List<string> AskedParts(Asked asked)
    {
        var parts = new List<string>(3);
        if (asked.TraineeName is { } name)
        {
            parts.Add(name);
        }

        if (asked.TraineeName is not null || !string.Equals(asked.Status, OutstandingValue, StringComparison.Ordinal))
        {
            parts.Add(StatusWords(asked.Status));
        }

        if (asked.EpaCode is { } code)
        {
            parts.Add(code);
        }

        return parts;
    }

    private static string StatusWords(string status) => status switch
    {
        OutstandingValue => "outstanding",
        EveryStatusValue => "every status",
        _ => Enum.TryParse<EntrustmentDecisionDueStatus>(status, out var parsed) ? StatusLabel(parsed) : status
    };

    // ---- The trainee line (C2) -----------------------------------------------------------------------------------------

    /// <summary>
    /// A seat held: "Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on
    /// 2026-10-12." The type as a list column says it, in lower case inside the sentence.
    /// </summary>
    public static string HeldLine(DecisionsDueSeatLineDto line, string periodLabel)
    {
        ArgumentNullException.ThrowIfNull(line);

        var type = line.HoldingReviewTypeLabel is { } label ? $" ({label.ToLowerInvariant()})" : string.Empty;
        var on = line.HoldingScheduledOn is DateOnly date
            ? $", scheduled on {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : string.Empty;
        return $"Review #{line.HoldingReviewId}{type} holds {periodLabel} before {line.HoldingPanelName ?? line.PanelName}{on}.";
    }

    /// <summary>
    /// A seat free: "No open review holds 2026 S2.", naming the panel when the trainee has more than one line ("No open
    /// review before Neonatal CCC holds 2026 S2.").
    /// </summary>
    public static string FreeLine(DecisionsDueSeatLineDto line, string periodLabel, bool namePanel)
        => namePanel ? $"No open review before {line.PanelName} holds {periodLabel}." : $"No open review holds {periodLabel}.";

    /// <summary>The free line's link: "Schedule a review", or "Schedule a review before Neonatal CCC" beside another line.</summary>
    public static string FreeLineLink(DecisionsDueSeatLineDto line, bool namePanel)
        => namePanel ? $"Schedule a review before {line.PanelName}" : "Schedule a review";

    /// <summary>The free line's link's accessible name: its words first (S7), then whose and for when.</summary>
    public static string FreeLineLabel(DecisionsDueSeatLineDto line, string traineeName, string periodLabel, bool namePanel)
        => namePanel
            ? $"Schedule a review before {line.PanelName} of {traineeName} for {periodLabel}"
            : $"Schedule a review of {traineeName} for {periodLabel} before {line.PanelName}";

    /// <summary>The held line's link's accessible name: "Open review #1 of Lerato Molefe".</summary>
    public static string HeldLineLabel(int reviewId, string traineeName) => $"Open review #{reviewId} of {traineeName}";

    /// <summary>An open check-in: "Formative check-in #6 is open: it takes no seat."</summary>
    public static string CheckInLine(int reviewId) => $"Formative check-in #{reviewId} is open: it takes no seat.";

    /// <summary>The check-in's link: "Open check-in #6".</summary>
    public static string CheckInLink(int reviewId) => $"Open check-in #{reviewId}";

    /// <summary>The check-in link's accessible name, its words inside it (S7): "Open check-in #6 of Sipho Ndlovu".</summary>
    public static string CheckInLabel(int reviewId, string traineeName) => $"Open check-in #{reviewId} of {traineeName}";
}

using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>
/// The words of the schedule page (<c>/committee/reviews/new</c>) and Committee reviews (T364, flow 08; round 3's boards
/// R2-4 and R2-5), in one place, so a test reads the same sentence the page prints. Sentences are built here in C#, because
/// Razor drops a space standing alone before an expression and runs sentences together.
/// </summary>
public static class ScheduleWords
{
    /// <summary>The schedule page's heading, its tab's stem and its crumb, and Committee reviews' header action (S7).</summary>
    public const string Title = "Schedule a review";

    /// <summary>The schedule page's subtitle.</summary>
    public const string Subtitle = "Put a trainee before a panel for a period. The review opens once it is created.";

    /// <summary>
    /// Said wherever a semester-2 period is shown (T364, Q9 as F6 words it): the College's semester ends in November, and
    /// its window holds December (D13), so December's work counts. Not a rule about when a review may sit. Here and not
    /// in Application, whose committee feature writes no month of its own (<c>TheCommitteeFeature_WritesNoCalendarOfItsOwn</c>).
    /// </summary>
    public const string Semester2WindowNote =
        "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.";

    /// <summary>The schedule page while it reads what the caller may schedule (C11).</summary>
    public const string Loading = "Loading what you can schedule.";

    /// <summary>The schedule page's load failure (C11, T338): never an exception's text.</summary>
    public const string LoadFailure =
        "Could not load what you can schedule. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>Committee reviews while it reads (C11).</summary>
    public const string ListLoading = "Loading Committee reviews.";

    /// <summary>Committee reviews' load failure (C11, T338).</summary>
    public const string ListLoadFailure =
        "Could not load Committee reviews. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>Committee reviews' table caption: "Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first".</summary>
    public static string ListCaption(IEnumerable<CommitteeReviewListItemDto> reviews)
    {
        ArgumentNullException.ThrowIfNull(reviews);

        // The institution is named only when every row is at one (an Administrator's list may hold several).
        var institutions = reviews.Select(review => review.InstitutionName).Distinct(StringComparer.Ordinal).ToArray();
        return institutions is [{ } only] && !string.IsNullOrWhiteSpace(only)
            ? $"Committee reviews at {only}, newest scheduled first"
            : "Committee reviews, newest scheduled first";
    }

    // ---- The fields --------------------------------------------------------------------------------------------------

    /// <summary>Panel's help while panels are offered.</summary>
    public const string PanelHelp = "Only the panels with a trainee you can schedule are listed.";

    /// <summary>Panel's help when none is (T194): not a fault to report.</summary>
    public const string NoPanelHelp =
        "No panel has a trainee you can schedule a review for. A panel is listed once a trainee it reviews is in a " +
        "programme you oversee.";

    /// <summary>Trainee's help until a panel is chosen (C4); the select is disabled then.</summary>
    public const string TraineeWaitsForPanel =
        "Choose a panel first. The list holds the trainees at its institution you can schedule.";

    /// <summary>Trainee's help once a panel is chosen (5.29).</summary>
    public const string TraineeHelp =
        "Current registrars at the panel's institution whose programme you oversee, with their training year.";

    /// <summary>Trainee's help when the chosen panel offers no one.</summary>
    public const string NoTraineeHelp = "This panel has no trainees you can schedule a review for.";

    /// <summary>"Lerato Molefe · Training year 4"; the name alone before the programme starts.</summary>
    public static string TraineeOption(SchedulableTraineeDto trainee)
    {
        ArgumentNullException.ThrowIfNull(trainee);
        return trainee.TrainingYear is int year
            ? string.Create(CultureInfo.InvariantCulture, $"{trainee.DisplayName} · Training year {year}")
            : trainee.DisplayName;
    }

    /// <summary>Scheduled on's help.</summary>
    public const string ScheduledOnHelp = "The day the review is put on the schedule.";

    // ---- The refusals (C7, T324) ---------------------------------------------------------------------------------------

    /// <summary>The refusal summary's title.</summary>
    public const string NotCreated = "The review was not created.";

    /// <summary>The refusal summary's text.</summary>
    public const string KeptBelow = "Everything you chose is kept below.";

    public const string ChoosePanel = "Choose a panel.";
    public const string ChooseTrainee = "Choose a trainee.";
    public const string ChoosePeriod = "Choose the period the review sits for.";
    public const string ChooseWindowStart = "Choose the day the window starts.";
    public const string ChooseWindowEnd = "Choose the day the window ends.";
    public const string ChooseScheduledOn = "Choose the day the review is scheduled on.";

    /// <summary>Create failed for a reason nobody wrote words for (T338): never the exception's text.</summary>
    public const string CreateFailed =
        "The review could not be created just now. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- The Agenda aside --------------------------------------------------------------------------------------------

    /// <summary>The aside before a panel, a trainee and a period are chosen.</summary>
    public const string ChooseToPreview =
        "Choose a panel, a trainee and a period to see which EPAs the review will be there to decide.";

    /// <summary>The aside while the seat and the agenda are read, between a choice and the answers (T364, build review D2).</summary>
    public const string ReadingAgenda = "Reading the agenda.";

    /// <summary>A formative check-in's aside.</summary>
    public const string FormativeNoAgenda = "A formative review carries no agenda: it decides no EPA.";

    /// <summary>The aside's lead when the panel decides nothing on the trainee's curriculum (S1).</summary>
    public const string NoEpaOnAgenda = "No EPA will be on the agenda.";

    /// <summary>The preview's failure (C6, T338, T272): never the exception's text.</summary>
    public const string PreviewFailed = "The agenda could not be read just now.";

    /// <summary>Said with the failure: the preview is advice, and Create stays offered.</summary>
    public const string PreviewFailedNote = "The review can still be created: its agenda is planned when you press Create review.";

    /// <summary>Already held's lead (Q3).</summary>
    public const string AlreadyHeld = "Already held.";

    /// <summary>Said with Already held: a check-in may still be scheduled.</summary>
    public const string CheckInTakesNoSeat =
        "A formative check-in takes no seat: choose Formative check-in under Review type to schedule one.";

    /// <summary>"Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe."</summary>
    public static string CreateNotOffered(SchedulingSeatDto seat)
    {
        ArgumentNullException.ThrowIfNull(seat);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"Create review is not offered: review #{seat.ReviewId} already holds {seat.PeriodLabel} for {seat.TraineeName}.");
    }

    /// <summary>"Open review #1": the visible words of the link to the review in the way.</summary>
    public static string OpenReview(int reviewId) => string.Create(CultureInfo.InvariantCulture, $"Open review #{reviewId}");

    /// <summary>"Open review #1 of Lerato Molefe": its name, the visible words first (S7).</summary>
    public static string OpenReviewOf(SchedulingSeatDto seat)
    {
        ArgumentNullException.ThrowIfNull(seat);
        return $"{OpenReview(seat.ReviewId)} of {seat.TraineeName}";
    }
}

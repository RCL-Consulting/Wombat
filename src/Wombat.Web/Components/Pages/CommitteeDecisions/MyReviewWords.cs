using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>
/// The words of the trainee's two pages (T366, flow 09; boards R3-1, R3-2): My committee reviews and her review's page.
/// The appeal's words are <see cref="Review.AppealWords" />'.
/// </summary>
public static class MyReviewWords
{
    // ─── My committee reviews ────────────────────────────────────────────────────

    public const string ListTitle = "My committee reviews";
    public const string ListSubtitle = "Ratified decisions, appeal status, and final outcomes.";
    public const string ListCaption = "Your committee reviews, newest first";
    public const string ListLoading = "Loading your committee reviews.";

    /// <summary>A failed read (T329): fixed words, never the exception's text, and Try again beside them.</summary>
    public const string ListFailed =
        "Could not load your committee reviews. Nothing has changed. Try again, or come back in a few minutes.";

    public const string EmptyTitle = "No decisions yet";
    public const string EmptyBody = "Ratified committee outcomes will appear here.";

    // ─── Her review's page ───────────────────────────────────────────────────────

    /// <summary>The h1, tab and crumb while the review is read, or when it failed (S12): flow 07's.</summary>
    public const string LoadingHeading = Review.ReviewWords.LoadingHeading;

    public const string Loading = "Loading your review.";

    /// <summary>Flow 07's fixed sentence (T329).</summary>
    public const string LoadFailed = Review.ReviewWords.LoadFailed;

    /// <summary>
    /// The h1, tab and crumb (the title rule): the period and the review's type, "2026 S2 annual progression review",
    /// "2026 S2 formative check-in" (C6).
    /// </summary>
    public static string Heading(CommitteeReviewDetailDto review)
    {
        var type = review.ReviewTypeLabel;
        return type.Length == 0
            ? review.PeriodLabel
            : $"{review.PeriodLabel} {char.ToLower(type[0], CultureInfo.InvariantCulture)}{type[1..]}";
    }

    public const string AboutLabel = "About this review";
    public const string DecisionsHeading = "Decisions";
    public const string AgendaHeading = "Agenda";

    /// <summary>A formative check-in's Decisions (S13, NEW).</summary>
    public const string FormativeNoDecision = "No binding decision. A formative check-in records no decision and issues no STAR.";

    /// <summary>"2026-01-01 to 2026-12-31".</summary>
    public static string Window(DateOnly from, DateOnly to)
        => $"{from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} to {to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";
}

using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Pages.MultiSourceFeedback;

/// <summary>
/// The words of her MSF reports (T369, flow 10, lane T; boards R3-M1 to R3-M11): the list, <c>/msf/my-reports</c>, and her
/// own report, <c>/msf/my-reports/{id}</c>. Whole strings, because Razor drops a space standing alone before an expression;
/// every date ISO and every moment "yyyy-MM-dd hh:mm SAST" (T325). Where the coordinator's report says the same thing, the
/// words are <see cref="MsfReportText" />'s.
/// </summary>
public static class MyMsfReportText
{
    // ─── The list (M1, M2, M5–M7, M11) ───────────────────────────────────────

    public const string ListSubtitle = "Released aggregate feedback only. Individual responses are never shown.";

    public const string ReportsHeading = "Your released reports";

    public const string ReportsCaption = "Your released MSF reports, newest first";

    public const string ListLoading = "Loading your MSF reports.";

    /// <summary>A failed read (M7, T329): fixed words, never the exception's text, which goes to the log.</summary>
    public const string ListFailed =
        "Could not load your MSF reports. Nothing has changed. Try again, or come back in a few minutes.";

    public const string EmptyTitle = "No released reports";

    public const string EmptyBody = "Released multi-source feedback will appear here.";

    /// <summary>
    /// An id that is not one of her released reports (not hers, not released, or unknown), said once above her list (M5,
    /// M11; T270). Never for a failed read, which is the report page's load error (M10).
    /// </summary>
    public const string NotFound = "That report could not be found. Your released reports are listed below.";

    /// <summary>
    /// What a row's View names (T239), after "View ": "the Default MSF report, released 2026-10-14".
    /// </summary>
    public static string RowName(MsfReleasedReportDto report)
        => $"the {report.TemplateName} report, released {ActivityMoments.Date(report.ReleasedOn)}";

    /// <summary>The time a report was released, as the coordinator's report prints it: "2026-10-14 16:34 SAST" (T325).</summary>
    public static string Released(DateTime releasedOn) => ActivityMoments.When(releasedOn);

    // ─── Her report (M3, M4, M8–M10) ─────────────────────────────────────────

    /// <summary>Her report's h1, tab and crumb (S6): "Default MSF, released 2026-10-14".</summary>
    public static string Heading(string templateName, DateTime releasedOn)
        => $"{templateName}, released {ActivityMoments.Date(releasedOn)}";

    /// <summary>"Multi-source feedback · Released 2026-10-14 16:34 SAST".</summary>
    public static string Subtitle(MsfTemplateKind kind, DateTime releasedOn)
        => $"{MsfEvidenceKinds.Describe(kind)} · Released {ActivityMoments.When(releasedOn)}";

    public const string NarrativeHeading = "Narrative";

    public const string GroupsHeading = "What your colleagues said";

    /// <summary>The groups' intro, her copy: no one named, how the ratings are averaged, and that comments are verbatim.</summary>
    public static string GroupsIntro(MsfTemplateKind kind)
        => kind == MsfTemplateKind.LearnerFeedback
            ? $"From the learners you taught; no one is named. {MsfReportText.RatingScaleNote} Comments are word for word."
            : $"Grouped by respondent role; no one is named. {MsfReportText.RatingScaleNote} Comments are word for word.";

    public const string PortfolioHeading = "On your portfolio";

    /// <summary>"Recorded as evidence on 2026-10-14 16:34 SAST:", before the EPAs it was recorded for.</summary>
    public static string RecordedOn(DateTime? recordedOn)
        => recordedOn is DateTime moment ? $"Recorded as evidence on {ActivityMoments.When(moment)}:" : "Recorded as evidence:";

    /// <summary>An EPA on her portfolio, as the coordinator's report lists it: "PAED-010 — Leading and operating …".</summary>
    public static string EpaLine(MsfCoveredEpaDto epa) => $"{epa.Code} — {epa.Title}";

    /// <summary>
    /// The release recorded nothing: the campaign named no EPA, or every one it named had left her curriculum (round 2's
    /// R8c; M4).
    /// </summary>
    public const string NoEvidence = "This report records no evidence on your portfolio.";
}

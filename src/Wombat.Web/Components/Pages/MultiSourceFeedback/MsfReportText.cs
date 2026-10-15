using System.Globalization;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Pages.MultiSourceFeedback;

/// <summary>
/// The MSF report's words (T369, flow 10, round 3's R boards; build-contracts § 1): the coordinator's report page, its Close
/// and Release dialogs, and the group cards her own report mounts too (<see cref="Responses" />,
/// <see cref="RatingScaleNote" />). Whole strings, because Razor drops a space standing alone before an expression; every
/// count that counts has its singular (T270); every date ISO, every moment "yyyy-MM-dd hh:mm SAST" (T325).
/// </summary>
/// <remarks>
/// The registrar is named, never "she" or "her": where the boards wrote a pronoun (M5's ended programme, R6b's "no longer
/// on her curriculum"), the sentence names the trainee instead, as <see cref="MsfCampaignText.ProgrammeEnded" /> does
/// (T369, build review D3).
/// </remarks>
public static class MsfReportText
{
    /// <summary>The narrative's limit, as <c>ReleaseMsfCampaignCommandValidator</c> holds it.</summary>
    public const int NarrativeLimit = 4000;

    // ─── Header and states ───────────────────────────────────────────────────

    /// <summary>The h1, the tab and the crumb while the report is read or failed (R13, R14).</summary>
    public const string LoadingHeading = "MSF report";

    public static string Heading(string subjectName) => $"MSF report for {subjectName}";

    public const string Loading = "Loading the report.";

    /// <summary>A failed read (R14, T329): a fixed sentence, never the exception's text, which goes to the log.</summary>
    public const string LoadFailed = "Could not load this report. Nothing has changed. Try again, or come back in a few minutes.";

    public const string UnavailableHeading = "Report unavailable";

    /// <summary>R12: the campaign page's sentence for its own unavailable state, worded for the report (S17).</summary>
    public const string UnavailableBody = "The selected report could not be loaded.";

    public const string GoToCampaigns = "Go to MSF campaigns";

    /// <summary>"Default MSF · Multi-source feedback · 2026-10-14 to 2026-10-28".</summary>
    public static string Subtitle(string templateName, MsfTemplateKind kind, DateOnly opensOn, DateOnly closesOn)
        => $"{templateName} · {MsfEvidenceKinds.Describe(kind)} · {Day(opensOn)} to {Day(closesOn)}";

    /// <summary>F9: alone on its line under the subtitle.</summary>
    public const string CampaignPageLink = "Open its campaign page";

    // ─── The status card ─────────────────────────────────────────────────────

    /// <summary>The status card's h2: what the state means now (F7).</summary>
    public static string StatusLine(MsfCampaignAggregateReportDto report)
        => report.State switch
        {
            MsfCampaignState.Draft => "Not open yet: nobody has been asked for feedback.",
            MsfCampaignState.Open => $"Still taking responses, until {Day(report.ClosesOn)}.",
            MsfCampaignState.Released => "The report has been released to the trainee.",
            MsfCampaignState.Withdrawn => "Withdrawn: this report is never released to the trainee.",
            _ => report.ReadyForRelease ? "Ready to release: every threshold is met." : "Not ready to release."
        };

    /// <summary>F1: while Draft or Open, the report shows no group's results, and says why.</summary>
    public const string ReadOnceClosed =
        "The responses are read once the campaign has closed and every address has been removed.";

    /// <summary>F5: a closed campaign whose closer is null was closed by the hourly job.</summary>
    public static string ClosedByItself(DateTime closedOn, DateOnly closesOn)
        => $"Closed by itself on {ActivityMoments.Date(closedOn)}, once {Day(closesOn)} had passed.";

    /// <summary>"1 response" / "3 responses" (T270).</summary>
    public static string Responses(int count)
        => count == 1 ? "1 response" : $"{count.ToString(CultureInfo.InvariantCulture)} responses";

    /// <summary>"1 respondent group" / "2 respondent groups" (T270).</summary>
    public static string RespondentGroups(int count)
        => count == 1 ? "1 respondent group" : $"{count.ToString(CultureInfo.InvariantCulture)} respondent groups";

    /// <summary>The checklist's first line: "3 responses, of the 5 needed."</summary>
    public static string ResponsesNeeded(int total, int minimum)
        => $"{Responses(total)}, of the {minimum.ToString(CultureInfo.InvariantCulture)} needed.";

    /// <summary>The checklist's second line: "0 respondent groups reporting, of the 2 needed."</summary>
    public static string GroupsNeeded(int surviving, int minimum)
        => $"{RespondentGroups(surviving)} reporting, of the {minimum.ToString(CultureInfo.InvariantCulture)} needed.";

    /// <summary>The checklist's rule line: "A respondent group reports once 3 of it have responded; …".</summary>
    public static string GroupRule(int minimumInGroup)
        => minimumInGroup == 1
            ? "A respondent group reports once 1 of it has responded; a group with fewer is suppressed."
            : $"A respondent group reports once {minimumInGroup.ToString(CultureInfo.InvariantCulture)} of it have " +
              "responded; a group with fewer is suppressed.";

    /// <summary>
    /// R7's released one-liner, its parts: "2026-10-14 16:34 SAST", "8 responses, with 5 needed", "2 respondent groups
    /// reporting, with 2 needed". The page sets them apart with a separator only it draws.
    /// </summary>
    public static IReadOnlyList<string> ReleasedLine(MsfCampaignAggregateReportDto report)
        =>
        [
            report.ReleasedOn is DateTime releasedOn ? ActivityMoments.When(releasedOn) : string.Empty,
            $"{Responses(report.TotalResponses)}, with {report.MinimumResponses.ToString(CultureInfo.InvariantCulture)} needed",
            $"{RespondentGroups(report.SurvivingCategoryCount)} reporting, with " +
            $"{report.MinimumRespondentCategories.ToString(CultureInfo.InvariantCulture)} needed"
        ];

    // ─── Evidence for ────────────────────────────────────────────────────────

    public const string EvidenceHeading = "Evidence for";

    public static string EpaLine(MsfCoveredEpaDto epa) => $"{epa.Code} — {epa.Title}";

    public const string NoEpa =
        "This campaign names no EPA, so releasing it will record no evidence on the trainee's portfolio.";

    public const string OneRecordPerEpa =
        "One evidence record per EPA is written to the trainee's portfolio when the report is released.";

    /// <summary>
    /// R6b: before release, the EPAs the release will drop. "PAED-012 is no longer on Lerato Molefe's curriculum and will not
    /// be recorded."
    /// </summary>
    public static string WillNotBeRecorded(string subjectName, IReadOnlyList<string> codes)
        => codes.Count == 1
            ? $"{codes[0]} is no longer on {subjectName}'s curriculum and will not be recorded."
            : $"{Join(codes)} are no longer on {subjectName}'s curriculum and will not be recorded.";

    /// <summary>R7b: a released EPA whose record was not written.</summary>
    public const string NotRecordedSuffix = ": not recorded";

    public static string RecordedOn(DateTime recordedOn) => $"Recorded on the portfolio: {ActivityMoments.When(recordedOn)}";

    public const string NothingRecorded = "No evidence record was written to the portfolio.";

    // ─── Learner feedback's contexts (T164) ──────────────────────────────────

    public static string TeachingContexts(int count, IReadOnlyList<string>? names)
        => names is { Count: > 0 }
            ? $"Teaching contexts that responded: {count.ToString(CultureInfo.InvariantCulture)} ({string.Join(", ", names)})"
            : $"Teaching contexts that responded: {count.ToString(CultureInfo.InvariantCulture)}";

    public const string TeachingContextsNote =
        "EPA 15 asks for feedback from learners in at least two teaching contexts before entrustment. This count is for the " +
        "committee to weigh; it does not hold the release. The trainee and the committee are told how many contexts " +
        "responded, never their names.";

    // ─── The status card's Close (Open) ──────────────────────────────────────

    public const string CloseLabel = "Close campaign…";

    public static string CloseHelp(DateOnly closesOn)
        => $"Release is offered once the campaign has closed. It closes by itself once {Day(closesOn)} has passed; close " +
           "it sooner here.";

    // ─── By respondent group ─────────────────────────────────────────────────

    public const string GroupsHeading = "By respondent group";

    public const string RatingScaleNote =
        "Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.";

    /// <summary>
    /// The groups' intro: as she reads it once released, as she reads it, or, once her programme has ended, as she would
    /// have read it (M5).
    /// </summary>
    public static string GroupsIntro(string subjectName, MsfCampaignState state, bool subjectIsCurrent)
        => (subjectIsCurrent
               ? state == MsfCampaignState.Released
                   ? $"As {subjectName} reads it. "
                   : $"As {subjectName} will read it once released. "
               : $"As {subjectName} would have read it. ") + RatingScaleNote;

    /// <summary>"Suppressed: Peer doctor, Allied health professional." The bold half of the suppressed line.</summary>
    public static string SuppressedGroups(IReadOnlyList<MsfCategoryAggregateDto> suppressed)
        => $"Suppressed: {string.Join(", ", suppressed.Select(group => MsfRespondentCategories.Describe(group.Category)))}.";

    /// <summary>
    /// The rest of the suppressed line: why, and (before release) that she sees nothing of them. Named, never counted:
    /// nobody sees a suppressed group's count here.
    /// </summary>
    public static string SuppressedWhy(
        IReadOnlyList<MsfCategoryAggregateDto> suppressed, int minimumInGroup, string subjectName, bool released)
    {
        var fewer = $"Fewer than {Responses(minimumInGroup)}";
        if (suppressed.Count == 1)
        {
            var what = suppressed[0].ResponseCount == 1 ? "its rating and comment are" : "its ratings and comments are";
            return $"{fewer}, so {what} not shown here." +
                   (released ? string.Empty : $" {subjectName} sees nothing of it, not even that it answered.");
        }

        return $"{fewer} each, so their ratings and comments are not shown here." +
               (released ? string.Empty : $" {subjectName} sees nothing of them, not even that they answered.");
    }

    // ─── The release section ─────────────────────────────────────────────────

    public static string ReleaseHeading(string subjectName) => $"Release to {subjectName}";

    public const string WhenYouRelease = "When you release:";

    /// <summary>The release section's first line, or M5's once her programme has ended.</summary>
    public static string WhoReads(string subjectName, bool subjectIsCurrent)
        => subjectIsCurrent
            ? $"{subjectName} can read your narrative and the groups shown above on MSF reports."
            : NoLongerOpens(subjectName);

    /// <summary>M5's line, in the release section and the dialog.</summary>
    public static string NoLongerOpens(string subjectName)
        => $"{subjectName} can no longer open MSF reports; the report is kept with the evidence on {subjectName}'s record.";

    /// <summary>
    /// The release section's records line: "2 evidence records are written to Lerato Molefe's portfolio, for PAED-010
    /// and PAED-012.", with R6b's dropped EPAs after it. Null when the campaign names no EPA (R9: the line drops).
    /// </summary>
    public static string? RecordsLine(string subjectName, IReadOnlyList<MsfCoveredEpaDto> epas)
    {
        if (epas.Count == 0)
        {
            return null;
        }

        var recorded = epas.Where(epa => epa.WillBeRecorded).Select(epa => epa.Code).ToList();
        var dropped = epas.Where(epa => !epa.WillBeRecorded).Select(epa => epa.Code).ToList();
        if (recorded.Count == 0)
        {
            return $"No evidence record is written: {Join(dropped)} {(dropped.Count == 1 ? "is" : "are")} no longer on " +
                   $"{subjectName}'s curriculum.";
        }

        var line = $"{Records(recorded.Count)} written to {subjectName}'s portfolio, for {Join(recorded)}";
        return dropped.Count == 0
            ? $"{line}."
            : $"{line}; {Join(dropped)} {(dropped.Count == 1 ? "is" : "are")} no longer on {subjectName}'s curriculum and " +
              $"{(dropped.Count == 1 ? "is" : "are")} not recorded.";
    }

    /// <summary>The mail line (Q8), or F4's once the trainee's programme has ended.</summary>
    public static string MailLine(string subjectName, bool subjectIsCurrent)
        => subjectIsCurrent
            ? $"{subjectName} is sent an email saying a report has been released, with a link to MSF reports."
            : NoMail(subjectName);

    /// <summary>F4: no release mail once the trainee's programme has ended.</summary>
    public static string NoMail(string subjectName) => $"No email is sent: {subjectName} can no longer open MSF reports.";

    public const string CannotBeUndone = "It cannot be undone.";

    public const string NarrativeLabel = "Narrative";

    /// <summary>The narrative's counter, "106 of 4,000 characters", as the browser's script counts it.</summary>
    public static string Counter(int length)
        => $"{length.ToString("N0", CultureInfo.InvariantCulture)} of {NarrativeLimit.ToString("N0", CultureInfo.InvariantCulture)} characters";

    /// <summary>A narrative over the limit, refused under the field (DESIGN.md § Form system: no maxlength).</summary>
    public const string NarrativeTooLong = "Write the narrative in 4,000 characters or fewer.";

    public const string LevelLabel = "Supervision level this feedback supports";

    public const string LevelHelp =
        "Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed by the " +
        "aggregate above — never an average of respondent scores.";

    public const string NotStated = "Not stated";

    public const string ReleaseLabel = "Release to trainee…";

    /// <summary>The reason's name for the disabled Release, bold before the reason.</summary>
    public const string ReleaseReasonLead = "Release to trainee:";

    /// <summary>Before the link, in the help under Release and at the end of every blocked reason (S15, check item 2).</summary>
    public const string NeverReleasedLead = "If it should never be released, ";

    public const string WithdrawLinkText = "withdraw it on its campaign page";

    /// <summary>
    /// Which release gate is closed (R8a, R8b): both are computed in one place, the aggregation service, and this only
    /// phrases what it decided.
    /// </summary>
    public static string BlockedReason(MsfCampaignAggregateReportDto report)
        => report.TotalResponses < report.MinimumResponses
            ? $"Release needs {Responses(report.MinimumResponses)}; " +
              (report.TotalResponses == 1
                  ? "1 has come back."
                  : $"{report.TotalResponses.ToString(CultureInfo.InvariantCulture)} have come back.")
            : $"Release needs {RespondentGroups(report.MinimumRespondentCategories)} to report; " +
              $"{report.SurvivingCategoryCount.ToString(CultureInfo.InvariantCulture)} cleared the " +
              $"{report.MinimumCategoryResponses.ToString(CultureInfo.InvariantCulture)}-response threshold. " +
              (report.Kind == MsfTemplateKind.LearnerFeedback
                  ? "Below the threshold, the learners' answers are suppressed to keep them anonymous."
                  : "Feedback that survives from too few groups is not multi-source feedback.");

    // ─── The dialogs ─────────────────────────────────────────────────────────

    public const string CloseTitle = "Close this campaign now?";

    public static string CloseLead(string subjectName, string templateName, DateOnly closesOn)
        => $"Close the campaign for {MsfCampaignText.Describe(subjectName, templateName, closesOn)} before its last day?";

    /// <summary>R2: the links not yet used, a count from InvitedTotal less the responses, never whose.</summary>
    public static string CloseUnusedLinks(int unused)
        => unused switch
        {
            <= 0 => "It takes no more responses: every link has been used.",
            1 => "It takes no more responses: the 1 link not yet used stops working.",
            _ => $"It takes no more responses: the {unused.ToString(CultureInfo.InvariantCulture)} links not yet used stop working."
        };

    public const string CloseAddresses = "Every respondent's email address is removed, so no link can be resent.";

    public static string CloseKeeps(int responses)
        => responses == 0
            ? "The report has no response to keep."
            : $"The report keeps the {Responses(responses)} it has, for you to review and release.";

    public const string CloseFinal = "Closing cannot be undone.";

    public const string KeepItOpen = "Keep it open";

    public const string CloseConfirm = "Close campaign";

    /// <summary>R3: Close's result, its first sentence bold.</summary>
    public const string Closed = "Campaign closed and anonymised for review.";

    public const string ClosedDetail = " Every respondent's email address has been removed.";

    /// <summary>R16: a refused close, bold before the refusal in its own words.</summary>
    public const string NotClosed = "Not closed.";

    public static string ReleaseTitle(string subjectName) => $"Release this report to {subjectName}?";

    /// <summary>R6a: who reads it and what of it.</summary>
    public static string DialogWhoReads(string subjectName, IReadOnlyList<MsfCategoryAggregateDto> shown)
        => $"{subjectName} can read it on MSF reports: your narrative, and the ratings and comments of the " +
           $"{Join(shown.Select(group => MsfRespondentCategories.Describe(group.Category)).ToList())} " +
           $"{(shown.Count == 1 ? "group" : "groups")}. Nothing of the other groups is shown, not even that they answered.";

    /// <summary>R6a–R6c: "2 evidence records are written to Lerato Molefe's portfolio: PAED-010 and PAED-012."</summary>
    public static string DialogRecords(string subjectName, IReadOnlyList<MsfCoveredEpaDto> epas)
    {
        var recorded = epas.Where(epa => epa.WillBeRecorded).Select(epa => epa.Code).ToList();
        var dropped = epas.Where(epa => !epa.WillBeRecorded).Select(epa => epa.Code).ToList();
        var line = recorded.Count == 0
            ? "No evidence is recorded."
            : $"{Records(recorded.Count)} written to {subjectName}'s portfolio: {Join(recorded)}.";

        return dropped.Count == 0 ? line : $"{line} {Join(dropped)}: not recorded, no longer on {subjectName}'s curriculum.";
    }

    public static string DialogLevel(string levelLabel) => $"Supervision level: {levelLabel}.";

    public static string DialogMail(string subjectName, bool subjectIsCurrent)
        => subjectIsCurrent
            ? $"{subjectName} is sent an email saying a report has been released, with a link to MSF reports. The email " +
              "holds none of the feedback."
            : NoMail(subjectName);

    public static string ReleaseFinal(bool hasLevel)
        => hasLevel
            ? "Releasing cannot be undone: the narrative and the level cannot be changed afterwards."
            : "Releasing cannot be undone: the narrative cannot be changed afterwards.";

    public const string NotYet = "Not yet";

    public static string ReleaseConfirm(string subjectName) => $"Release to {subjectName}";

    /// <summary>R7: the release's result.</summary>
    public static string Released(string subjectName) => $"Report released to {subjectName}.";

    /// <summary>R17: a refused release, bold before the refusal.</summary>
    public const string NotReleased = "Not released.";

    public const string StillUnderReview = " It is still Under review.";

    /// <summary>
    /// An action that failed for a reason that is not a refusal (T329): fixed words, the failure to the log. The campaign
    /// pages' one sentence, not a second wording of it (T369, build review R2).
    /// </summary>
    public const string ActionFailed = MsfCampaignText.ActionFailed;

    // ─── As released ─────────────────────────────────────────────────────────

    public const string StoredHeading = "As released";

    public const string StoredLevel = "Supervision level";

    public const string NotGiven = "Not given";

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string Records(int count)
        => count == 1 ? "1 evidence record is" : $"{count.ToString(CultureInfo.InvariantCulture)} evidence records are";

    /// <summary>"A", "A and B", "A, B and C".</summary>
    internal static string Join(IReadOnlyList<string> items)
        => items.Count switch
        {
            0 => string.Empty,
            1 => items[0],
            _ => $"{string.Join(", ", items.Take(items.Count - 1))} and {items[^1]}"
        };

    private static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

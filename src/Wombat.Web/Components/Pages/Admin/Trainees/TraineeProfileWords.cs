using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.Trainees;

/// <summary>
/// The words of a trainee's profile, "‹name›'s programme" (T379, flow 13, boards P01–P25), in one place, so the page and its
/// tests read the same sentences. Straight apostrophes; the boards' "…" only on the two end triggers. No pronoun for the
/// trainee: their name, or "their" (N10).
/// </summary>
public static class TraineeProfileWords
{
    /// <summary>A date as every page prints one: ISO, in the invariant culture.</summary>
    public static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    // ── The page ───────────────────────────────────────────────────────────────

    /// <summary>The h1 and tab before the read, when it failed, and for the Trainee note (P14, P15).</summary>
    public const string Heading = "Trainee profile";

    public const string UnavailableTitle = "Profile unavailable";

    public const string UnavailableBody = "This trainee profile does not exist, or you cannot open it.";

    public const string GoToTrainees = "Go to Trainees";

    public const string Loading = "Loading this trainee profile.";

    public const string LoadFailed = "Could not load this trainee profile. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A running programme's subtitle (S7, built).</summary>
    public const string ActiveSubtitle = "Update curriculum and completion details for this trainee.";

    /// <summary>An ended record's subtitle (built).</summary>
    public const string EndedSubtitle = "The record of a programme that has ended.";

    public const string SummaryName = "Trainee summary";

    /// <summary>T305's ended-record line, built.</summary>
    public const string EndedLine = "This programme has ended, so its record is archived and cannot be changed.";

    public const string DetailsHeading = "Profile details";

    public const string CompletionHelp = "Leave this empty to derive it from the curriculum window.";

    public const string NotSet = "Not set";

    /// <summary>
    /// A save that failed for a reason that is not a refusal (T329): logged, and said in these fixed words. NEW: no board
    /// draws a failed save.
    /// </summary>
    public const string SaveFailed = "The profile could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.";

    // ── Status (N6, F10) ───────────────────────────────────────────────────────

    public const string Active = "Active";

    public const string Completed = "Completed";

    public const string Inactive = "Inactive";

    /// <summary>The built "Left the programme: 2026-10-24", after Inactive's badge.</summary>
    public static string LeftTheProgramme(DateOnly day) => $"Left the programme: {Iso(day)}";

    // ── End this programme ─────────────────────────────────────────────────────

    public const string EndHeading = "End this programme";

    /// <summary>
    /// Q13: "The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26."; an
    /// entrustment-only review has no category, so its type leads.
    /// </summary>
    public static string LastDecision(LastRatifiedDecisionDto decision)
        => decision.Category is { } category
            ? $"The last committee decision: {category}, {decision.ReviewType}, ratified {Iso(decision.RatifiedOn)}."
            : $"The last committee decision: {decision.ReviewType}, ratified {Iso(decision.RatifiedOn)}.";

    public const string LastDayLabel = "Last day in the programme";

    /// <summary>The last day's help (NEW, for T209's "Mark complete records it as the graduation day, …").</summary>
    public const string LastDayHelp =
        "The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and " +
        "it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last " +
        "month, and encounters observed after it count towards nothing in this programme, even those already counted.";

    /// <summary>P23: the day emptied, refused on the press (NEW, PROPOSED).</summary>
    public const string ChooseTheLastDay = "Choose the last day.";

    /// <summary>
    /// Why the day typed cannot be recorded, checked when Mark complete… or Deactivate… is pressed, before any dialog (Q21,
    /// S3): none typed (P23), or the rule both ends are held to (<see cref="TraineeProfile.EndDayRefusal" />: P02, P22).
    /// Null when the dialog may open.
    /// </summary>
    public static string? LastDayRefusal(DateOnly? typed, DateOnly programmeStart, DateOnly today)
        => typed is { } day ? TraineeProfile.EndDayRefusal(day, programmeStart, today) : ChooseTheLastDay;

    /// <summary>Mark complete…'s note (N13).</summary>
    public static string GraduationNote(string name)
        => $"A graduation: archives the profile, removes the Trainee role and sends {name} a graduation email.";

    /// <summary>Deactivate…'s note (N13).</summary>
    public const string WithdrawalNote = "A withdrawal: archives the profile and removes the Trainee role. No email is sent.";

    public const string MarkCompleteTrigger = "Mark complete…";

    public const string DeactivateTrigger = "Deactivate…";

    /// <summary>P06: an end that failed for a reason that is not a refusal, its first sentence bold.</summary>
    public const string EndFailedLead = "The programme could not be ended just now.";

    public const string EndFailedRest = "Nothing has changed. Try again, or come back in a few minutes.";

    // ── The dialogs (P03, P09, P16, P17, P19, P20) ───────────────────────────────

    public static string CompleteTitle(string name) => $"Mark {PortfolioWords.Possessive(name)} programme complete?";

    public const string CompleteConfirm = "Mark complete";

    /// <summary>P03's first paragraph, up to the address, which the page writes with a break after its @.</summary>
    public static string CompleteRecord(string name, DateOnly day)
        => $"Record {Iso(day)} as {PortfolioWords.Possessive(name)} graduation day? The profile is archived, the Trainee " +
           "role is removed and a graduation email is sent to ";

    /// <summary>P03's first paragraph for an account with no address (never in the scenario): no address is named.</summary>
    public static string CompleteRecordNoAddress(string name, DateOnly day)
        => $"Record {Iso(day)} as {PortfolioWords.Possessive(name)} graduation day? The profile is archived, the Trainee " +
           "role is removed and a graduation email is sent.";

    /// <summary>P03's second paragraph: what stops counting, the forecast (M2) when it is not 0, and what the graduate keeps.</summary>
    public static string CompleteAfter(string name, DateOnly day, TakeBackForecastDto? forecast)
        => $"{AfterTheDay(day, forecast)} {name} keeps the record, read-only, and can still export the portfolio.";

    public const string CompleteCannotUndo = "The day cannot be changed afterwards, and this cannot be undone.";

    public static string DeactivateTitle(string name) => $"Deactivate {PortfolioWords.Possessive(name)} profile?";

    public const string DeactivateConfirm = "Deactivate";

    public static string DeactivateRecord(string name, DateOnly day)
        => $"Deactivate {PortfolioWords.Possessive(name)} profile, with {Iso(day)} as their last day in the programme? The " +
           "Trainee role is removed; they keep their own record, read-only.";

    public static string DeactivateAfter(DateOnly day, TakeBackForecastDto? forecast) => AfterTheDay(day, forecast);

    public const string DeactivateCannotUndo =
        "The day cannot be changed afterwards, and a deactivated profile cannot be made active again. This cannot be undone.";

    /// <summary>
    /// "Encounters observed after that day will count towards nothing in this programme, including any already counted",
    /// then M2's forecast when it is not 0: ": 1 activity, observed 2026-10-25, will stop counting." or ": 3 activities,
    /// observed after 2026-10-24, will stop counting."
    /// </summary>
    private static string AfterTheDay(DateOnly day, TakeBackForecastDto? forecast)
    {
        const string lead = "Encounters observed after that day will count towards nothing in this programme, including any already counted";
        return forecast switch
        {
            { Count: 1, ObservedOn: { } observed } => $"{lead}: 1 activity, observed {Iso(observed)}, will stop counting.",
            { Count: > 0 } => $"{lead}: {forecast.Count} activities, observed after {Iso(day)}, will stop counting.",
            _ => $"{lead}."
        };
    }

    // ── The results (P05, P11, P21) ────────────────────────────────────────────

    public const string CompletedLead = "Trainee marked complete.";

    public const string CompletedRest = "The Trainee role has been removed and a graduation email sent.";

    public const string DeactivatedLead = "Trainee profile deactivated.";

    public static string DeactivatedRest(DateOnly day)
        => $"The last day in the programme is recorded as {Iso(day)}, and the Trainee role has been removed.";

    /// <summary>M2: what the end took back, from the command's count; null for 0, which says nothing.</summary>
    public static string? TakenBack(int count) => count switch
    {
        <= 0 => null,
        1 => "1 activity observed after that day no longer counts.",
        _ => $"{count} activities observed after that day no longer count."
    };

    // ── The account (F5, S5) ───────────────────────────────────────────────────

    public const string AccountHeading = "The account";

    /// <summary>
    /// The account sentence, or null for none: a locked account shows only the user page's link (P25). A person who holds a
    /// role now (P24, PROPOSED) is named by it; otherwise the sentence says what holding no role still allows, the STAR
    /// certificates only when one was issued (P05, P11).
    /// </summary>
    public static string? AccountSentence(string name, TraineeAccountDto account)
    {
        if (account.IsLocked)
        {
            return null;
        }

        if (account.Roles.Count > 0)
        {
            return $"{name} now holds {Join(WombatRoleLabels.InOrder(account.Roles).Select(WombatRoleLabels.For).ToList())}. " +
                   "They can still read the record and export the portfolio.";
        }

        return account.AnyStarIssued
            ? $"{name} holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates."
            : $"{name} holds no role now, and can still sign in to read the record and export the portfolio.";
    }

    public static string OpenUserPage(string name) => $"Open {PortfolioWords.Possessive(name)} user page";

    /// <summary>"Assessor", "Assessor and Coordinator", "Assessor, Coordinator and Committee member".</summary>
    private static string Join(IReadOnlyList<string> labels) => labels.Count switch
    {
        0 => string.Empty,
        1 => labels[0],
        _ => $"{string.Join(", ", labels.Take(labels.Count - 1))} and {labels[^1]}"
    };
}

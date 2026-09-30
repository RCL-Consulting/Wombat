using System.Globalization;
using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// Every phrase that says what waits on an assessor and what they decided (T350, flow 04; R3-Spec § 1, § 7): how long a
/// row has waited, the counts, the rule lines, the overflow, the result's tail and the other-role line. One place, so the
/// Assessor's Home, the Activity inbox, the activity page's way on and the other-role line cannot word one thing two ways
/// (flow 03's C11 rule, as <see cref="ActivityListWords" /> holds it for Needs you).
/// </summary>
/// <remarks>
/// A row's wait is <see cref="ActivitySummaryDto.WaitedDays" />, whole days since <c>UpdatedOn</c> rounded down, as the
/// assessor nudge counts them (Spec § 7); 0 is under a day. Moments are South African time with the zone
/// (<see cref="ActivityMoments.When" />, note 8).
/// </remarks>
public static class WaitingWords
{
    /// <summary>The rule over "Decided by you" (R2; Q1): what it holds and in what order.</summary>
    public const string DecidedRule = "Newest first. Everything you completed, declined, discussed or signed off.";

    /// <summary>A row's wait (R1): "Waiting less than a day", "Waiting 1 day", "Waiting 8 days".</summary>
    public static string Waited(ActivitySummaryDto item) => $"Waiting {Days(item, "less than a day")}";

    /// <summary>The inbox's Waiting cell (R2): "Less than a day", "1 day", "8 days".</summary>
    public static string WaitedCell(ActivitySummaryDto item) => Days(item, "Less than a day");

    /// <summary>Since when, under the inbox's cell and on the way on: "since 2026-09-22 08:55 SAST".</summary>
    public static string Since(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"since {ActivityMoments.When(item.UpdatedOn)}";
    }

    /// <summary>The way on's row (<c>WaitingList.WithSince</c>): "Waiting 8 days, since 2026-09-22 08:06 SAST."</summary>
    public static string WaitedSince(ActivitySummaryDto item) => $"{Waited(item)}, {Since(item)}.";

    /// <summary>
    /// The count in words, the waiting card's and section's badge (R1, R2; note 13): "2 waiting", "2 waiting, 1 overdue".
    /// </summary>
    public static string Count(WaitingForYouDto waiting)
    {
        ArgumentNullException.ThrowIfNull(waiting);
        return waiting.OverdueCount > 0
            ? $"{waiting.Count} waiting, {waiting.OverdueCount} overdue"
            : $"{waiting.Count} waiting";
    }

    /// <summary>
    /// The rule over "Waiting for you" (round 2, E1), its number <c>DashboardThresholds.AssessorDueDays</c>
    /// (<see cref="WaitingForYouDto.DueDays" />): "Oldest first. Overdue once it has waited 7 days."
    /// </summary>
    public static string RuleLine(int dueDays)
        => $"Oldest first. Overdue once it has waited {DayCount(dueDays)}.";

    /// <summary>The count over "Decided by you": "1 decision", "45 decisions".</summary>
    public static string DecidedCount(int total)
        => total == 1 ? "1 decision" : $"{total.ToString(CultureInfo.InvariantCulture)} decisions";

    /// <summary>
    /// Home's overflow line, above "Open Activity inbox" (round 2, E5): "1 more waits in the Activity inbox.", "20 more
    /// wait in the Activity inbox.".
    /// </summary>
    public static string More(int beyondFive)
        => beyondFive == 1
            ? "1 more waits in the Activity inbox."
            : $"{beyondFive.ToString(CultureInfo.InvariantCulture)} more wait in the Activity inbox.";

    /// <summary>
    /// The result's first sentence after a Return made as someone other than its author (T350 build review, D5):
    /// "Returned to Sipho Ndlovu.". The act and whose it now is, where the target state's "It is now Draft." named no act
    /// and read as the assessor's own draft; the status card below still says Draft. Before <see cref="MoreForYou" />,
    /// round 2's C1 pattern: "Returned to Sipho Ndlovu. Nothing else waits for you.".
    /// </summary>
    public static string ReturnedTo(string registrar) => $"Returned to {registrar}.";

    /// <summary>The move whose result <see cref="ReturnedTo" /> words: the CPSA reflection's, audit's and review's.</summary>
    public const string ReturnTransitionKey = "return";

    /// <summary>
    /// The result's tail after a move (round 1, Q6; C1), what is left once the activity just moved is gone: "1 more waits
    /// for you.", "2 more wait for you.", "Nothing else waits for you.".
    /// </summary>
    public static string MoreForYou(int left) => left switch
    {
        <= 0 => "Nothing else waits for you.",
        1 => "1 more waits for you.",
        _ => $"{left.ToString(CultureInfo.InvariantCulture)} more wait for you."
    };

    /// <summary>
    /// The other-role line on Home (note 7; Q3; round 2, E4, C12), for a holder of Assessor acting in another role, worded
    /// from <see cref="WaitingForYouDto.Oldest" />: its name, its registrar, and its wait lower-cased. Null when nothing
    /// waits: then there is no line.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>One, overdue: "1 activity waits for you in the Activity inbox, and it is overdue: &lt;name&gt;, from
    /// &lt;registrar&gt;, waiting 8 days."</item>
    /// <item>Several: "3 activities wait for you in the Activity inbox; 1 is overdue. The oldest: &lt;name&gt;, from
    /// &lt;registrar&gt;, waiting 8 days.", the "; n is/are overdue" clause only when any is.</item>
    /// <item>One, not overdue: "1 activity waits for you in the Activity inbox: &lt;name&gt;, from &lt;registrar&gt;,
    /// waiting less than a day."</item>
    /// </list>
    /// It names no role (E4): the read counts every arm that is not the author's, not only the assessor's.
    /// </remarks>
    public static string? OtherRoleLine(WaitingForYouDto waiting)
    {
        ArgumentNullException.ThrowIfNull(waiting);
        if (waiting.Oldest is not { } oldest)
        {
            return null;
        }

        var row = $"{ActivityRowNames.WaitingLinkWords(oldest)}, {LowerFirst(Waited(oldest))}.";
        if (waiting.Count == 1)
        {
            return oldest.IsOverdue
                ? $"1 activity waits for you in the Activity inbox, and it is overdue: {row}"
                : $"1 activity waits for you in the Activity inbox: {row}";
        }

        var overdue = waiting.OverdueCount switch
        {
            0 => string.Empty,
            1 => "; 1 is overdue",
            var count => $"; {count.ToString(CultureInfo.InvariantCulture)} are overdue"
        };
        return $"{waiting.Count.ToString(CultureInfo.InvariantCulture)} activities wait for you in the Activity inbox{overdue}. The oldest: {row}";
    }

    /// <summary>The other-role line's action (E4): "Open it" for one, "Open the oldest" for several.</summary>
    public static string OtherRoleAction(WaitingForYouDto waiting)
    {
        ArgumentNullException.ThrowIfNull(waiting);
        return waiting.Count == 1 ? "Open it" : "Open the oldest";
    }

    private static string Days(ActivitySummaryDto item, string underADay)
    {
        ArgumentNullException.ThrowIfNull(item);
        var days = item.WaitedDays ?? 0;
        return days < 1 ? underADay : DayCount(days);
    }

    private static string DayCount(int days)
        => days == 1 ? "1 day" : $"{days.ToString(CultureInfo.InvariantCulture)} days";

    private static string LowerFirst(string words)
        => words.Length == 0 ? words : char.ToLowerInvariant(words[0]) + words[1..];
}

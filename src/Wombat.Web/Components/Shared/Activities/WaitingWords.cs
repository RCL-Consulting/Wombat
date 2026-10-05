using System.Globalization;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Identity;

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

    // ---- The staff reading: Waiting for assessors (T358, flow 06; E3, E4, E6) ----------------------------------------
    //
    // The same rows read by a member of staff, about someone else's assessor: whom each waits with on a line of its own,
    // then flow 04's "Waiting 8 days" unchanged (E6). Nothing above this line changes for it (the fence). No third-person
    // pronoun anywhere (round 3 check 1): the person's name, "the registrar" or "the assessor"; the reader is "you".

    /// <summary>The staff row's line above its wait (E6): "With Mohammed Patel".</summary>
    public static string With(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"With {item.Holder?.Name ?? item.NomineeName}";
    }

    /// <summary>
    /// The rule over Home's Waiting for assessors card and the registrar page's section (round 3 item 11): "Oldest first.
    /// Overdue once it has waited 7 days. Its assessor is emailed after 5.", both numbers the settings'
    /// (<see cref="WaitingForAssessorsDto.DueDays" />, <see cref="WaitingForAssessorsDto.NudgeDays" />).
    /// </summary>
    public static string StaffRuleLine(int dueDays, int nudgeDays)
        => $"{RuleLine(dueDays)} Its assessor is emailed after {nudgeDays.ToString(CultureInfo.InvariantCulture)}.";

    /// <summary>
    /// The page's rule line: the staff rule, then T351's sentence, "Waiting counts from the last move: any save restarts
    /// it."
    /// </summary>
    public static string PageRuleLine(int dueDays, int nudgeDays)
        => $"{StaffRuleLine(dueDays, nudgeDays)} Waiting counts from the last move: any save restarts it.";

    /// <summary>
    /// The page's heading and the card's badge, the match counted in flow 04's words: "3 waiting, 2 overdue"; "1 waiting, 1
    /// overdue, with Mohammed Patel" when the read was asked for one nominee (<see cref="WaitingForAssessorsDto.Filter" />).
    /// </summary>
    public static string StaffCount(WaitingForAssessorsDto waiting)
    {
        ArgumentNullException.ThrowIfNull(waiting);
        var count = waiting.MatchOverdueCount > 0
            ? $"{waiting.MatchCount.ToString(CultureInfo.InvariantCulture)} waiting, {waiting.MatchOverdueCount.ToString(CultureInfo.InvariantCulture)} overdue"
            : $"{waiting.MatchCount.ToString(CultureInfo.InvariantCulture)} waiting";
        return NomineeNameOf(waiting, waiting.Filter.WithUserId) is { } name ? $"{count}, with {name}" : count;
    }

    /// <summary>The heading when the filters leave nothing (D9's pattern): "0 of 2 waiting".</summary>
    public static string NoMatchHeading(WaitingForAssessorsDto waiting)
    {
        ArgumentNullException.ThrowIfNull(waiting);
        return $"0 of {waiting.TotalCount.ToString(CultureInfo.InvariantCulture)} waiting";
    }

    /// <summary>
    /// Home's overflow past five rows (R2-Home): "1 more waits in Waiting for assessors.", "9 more wait in Waiting for
    /// assessors.".
    /// </summary>
    public static string StaffMore(int beyond)
        => beyond == 1
            ? "1 more waits in Waiting for assessors."
            : $"{beyond.ToString(CultureInfo.InvariantCulture)} more wait in Waiting for assessors.";

    /// <summary>
    /// The page's subtitle (E4): what was read, as which role. "Requests at Kgosi Kgari Teaching Hospital whose next move
    /// names an assessor, supervisor or reviewer, read as Coordinator. Your own requests are not listed."; "in
    /// Paediatrics" for the two admins.
    /// </summary>
    public static string StaffSubtitle(ProgrammeScopeDto scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        return $"Requests {Where(scope)} whose next move names an assessor, supervisor or reviewer, read as " +
               $"{WombatRoleLabels.For(scope.ActingRole)}. Your own requests are not listed.";
    }

    /// <summary>Home's Waiting for assessors card when nothing waits.</summary>
    public const string NothingWaitingCard = "Nothing is waiting for an assessor.";

    /// <summary>The page's empty state's title (w12).</summary>
    public const string NothingWaitingTitle = "Nothing is waiting";

    /// <summary>
    /// The page's empty state's words (w12): "Nothing at Kgosi Kgari Teaching Hospital is waiting for an assessor,
    /// supervisor or reviewer."; "Nothing in Paediatrics …" for the two admins.
    /// </summary>
    public static string NothingWaitingBody(ProgrammeScopeDto scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        return $"Nothing {Where(scope)} is waiting for an assessor, supervisor or reviewer.";
    }

    /// <summary>The no-match title, one pattern with Programme trainees' (review 33).</summary>
    public const string NoMatch = "No request matches these filters.";

    /// <summary>
    /// What was asked, under the no-match title: "Overdue only, with Fatima Khumalo.", "Overdue only.", "With Fatima
    /// Khumalo."; empty when nothing was.
    /// </summary>
    public static string Asked(bool overdueOnly, string? withName)
        => (overdueOnly, string.IsNullOrWhiteSpace(withName)) switch
        {
            (true, false) => $"Overdue only, with {withName}.",
            (true, true) => "Overdue only.",
            (false, false) => $"With {withName}.",
            _ => string.Empty
        };

    /// <summary>
    /// The table's caption: "Requests waiting for a named assessor, oldest first"; "Requests waiting for Mohammed Patel,
    /// oldest first" when one nominee is asked for.
    /// </summary>
    public static string TableCaption(string? withName)
        => string.IsNullOrWhiteSpace(withName)
            ? "Requests waiting for a named assessor, oldest first"
            : $"Requests waiting for {withName}, oldest first";

    /// <summary>The page's status while it loads, present from the first render.</summary>
    public const string PageLoading = "Loading Waiting for assessors.";

    /// <summary>The page's load error: fixed words, never the exception's (T329).</summary>
    public const string PageLoadFailed =
        "Could not load Waiting for assessors. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The card's foot, and the way to the page.</summary>
    public const string OpenPage = "Open Waiting for assessors";

    /// <summary>The Waiting filter's label: not "Show", which is the button's (review 31).</summary>
    public const string ShowLabel = "Waiting";

    /// <summary>The Waiting filter's first option.</summary>
    public const string ShowAll = "All";

    /// <summary>The Waiting filter's second option.</summary>
    public const string ShowOverdue = "Overdue only";

    /// <summary>The With filter's label.</summary>
    public const string WithLabel = "With";

    /// <summary>The With filter's first option, before each nominee by name.</summary>
    public const string WithAnyone = "Anyone";

    /// <summary>
    /// The name of the nominee <paramref name="withUserId" /> names, from the With filter's own list; null for none, or for
    /// an id the read does not list.
    /// </summary>
    public static string? NomineeNameOf(WaitingForAssessorsDto waiting, string? withUserId)
    {
        ArgumentNullException.ThrowIfNull(waiting);
        return withUserId is null
            ? null
            : waiting.Nominees.FirstOrDefault(nominee => string.Equals(nominee.UserId, withUserId, StringComparison.Ordinal))?.Name;
    }

    /// <summary>"at Kgosi Kgari Teaching Hospital" for the whole institution, "in Paediatrics" for a speciality's.</summary>
    private static string Where(ProgrammeScopeDto scope)
        => scope.Kind == ProgrammeScopeKind.Institution ? $"at {scope.Name}" : $"in {scope.Name}";

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

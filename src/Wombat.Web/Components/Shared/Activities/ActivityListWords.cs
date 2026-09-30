using System.Globalization;
using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// The words My activities and Needs you print for an activity row (T342, flow 03; R3-C-Mine, R3-Spec § 1 "Needs you"):
/// the link's second line, who has it now, and why a Needs you row is there. One place, so Home's card and My activities
/// cannot word one row two ways (C11).
/// </summary>
public static class ActivityListWords
{
    private static readonly TimeSpan SouthAfricanOffset = TimeSpan.FromHours(2);

    /// <summary>
    /// The link's second line: "to David Naidoo" for work that goes to its nominee (a rated request), "with Sarah Botha" for
    /// work talked over with them (a reflection, a portfolio review: <see cref="ActivityTypeShape.DiscussedOrReviewed" />).
    /// Null when there is no nominee, and when the name already ends with the nominee (E7,
    /// <see cref="ActivitySummaryDto.DisplayNameHasNominee" />), which a second line would repeat.
    /// </summary>
    /// <remarks>
    /// Decided from the type's shape, not from the nominee field's label: the shape is read from the pinned form by the
    /// rule the picker's groups use (<c>ActivityTypeShapes</c>), while a label is the College's words and differs per
    /// seed ("Assessor", "Supervisor or mentor", "Reviewer").
    /// </remarks>
    public static string? NomineeLine(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.DisplayNameHasNominee || string.IsNullOrWhiteSpace(item.NomineeName))
        {
            return null;
        }

        return item.Shape == ActivityTypeShape.DiscussedOrReviewed
            ? $"with {item.NomineeName}"
            : $"to {item.NomineeName}";
    }

    /// <summary>
    /// The assessor's link's second line (T350, R2; note 12): "from Anele Dlamini", the registrar the activity is about, in
    /// place of the nominee line, who on a waiting or a decided list is the reader. Null when the query named no subject.
    /// </summary>
    public static string? FromLine(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return string.IsNullOrWhiteSpace(item.SubjectName) ? null : $"from {item.SubjectName}";
    }

    /// <summary>The link's words: the activity's name, or its type's where the query named none.</summary>
    public static string NameOf(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return string.IsNullOrWhiteSpace(item.DisplayName) ? item.ActivityTypeName : item.DisplayName;
    }

    /// <summary>
    /// The "Who has it now" cell (R3-C-Mine): "You", the holder's name, "Waiting for Awaiting review." for a move a role
    /// holds with nobody named, "Done" and "Closed". An em dash when the query said nothing.
    /// </summary>
    public static string WhoHasIt(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Holder switch
        {
            null => "—",
            { IsViewer: true } => "You",
            { Kind: ActivityHolderKind.Done } => "Done",
            { Kind: ActivityHolderKind.Closed } => "Closed",
            { Kind: ActivityHolderKind.Waiting } => $"Waiting for {item.CurrentStateLabel}.",
            { Name: { } name } when !string.IsNullOrWhiteSpace(name) => name,
            _ => $"Waiting for {item.CurrentStateLabel}."
        };
    }

    /// <summary>
    /// Why a row is in Needs you (R3-Spec § 1): returned, by whom and on which day, or not submitted yet. A draft is not
    /// private (C2), so it never says nobody can see it.
    /// </summary>
    public static string NeedsYouWhy(ActivitySummaryDto item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Returned is { } returned
            ? $"Returned to you by {returned.ByName} on {DateOf(returned.ReturnedOn)}. Change it and submit again."
            : "Not submitted yet. It is in nobody's inbox until you submit it.";
    }

    /// <summary>A count as a screen reader hears it beside a badge that shows the figure alone (A16): "1 item", "2 items".</summary>
    public static string ItemCount(int count) => count == 1 ? "1 item" : $"{count} items";

    /// <summary>A UTC moment's day on the South African calendar, "2026-09-29".</summary>
    public static string DateOf(DateTime utc)
    {
        var moment = new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc));
        return moment.ToOffset(SouthAfricanOffset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}

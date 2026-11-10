using System.Globalization;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

/// <summary>
/// The read in words' own words (T398, flow 17, lane B, Q2, Q3): what <see cref="WorkflowReadout" />,
/// <see cref="CreditReadout" /> and <see cref="VersionsTable" /> print beyond what the read DTOs carry (the states' line,
/// a move's From and Note, "Credits nothing." and its line, the versions' heading, day and line). The DTOs' words (a
/// state's label, who moves it, what it checks, a credit sentence, a publisher's name) are the editor query's (lane A);
/// a page never parses a DSL. Boards R2-B03, B04, B05, R2-D02, D03, R3-D05, R2-E03, E06, R3-E01.
/// </summary>
public static class ActivityTypeReadWords
{
    /// <summary>
    /// The states on one line (R2-B04, R2-D02): each by its label, the initial one "where a new one starts", a terminal
    /// one "final", joined by " · ": "Draft, where a new one starts · Logged, final · Cancelled".
    /// </summary>
    public static string States(IEnumerable<ActivityTypeStateReadDto> states)
        => string.Join(" · ", states.Select(State));

    /// <summary>One state: "Draft, where a new one starts", "Completed, final", "Requested".</summary>
    public static string State(ActivityTypeStateReadDto state)
    {
        var words = state.Label;
        if (state.IsInitial)
        {
            words += ", where a new one starts";
        }

        if (state.IsTerminal)
        {
            words += ", final";
        }

        return words;
    }

    /// <summary>The moves table's caption (R2-B04), not repeating the panel's heading.</summary>
    public const string MovesCaption = "Moves, as saved";

    public const string MoveColumn = "Move";

    public const string FromColumn = "From";

    public const string ToColumn = "To";

    public const string WhoColumn = "Who moves it";

    public const string ChecksColumn = "Checks";

    public const string NoteColumn = "Note";

    /// <summary>
    /// The states a move leaves from (R2-B04: "Draft or Requested"): one alone, two joined by "or", more as "Draft,
    /// Requested or Returned".
    /// </summary>
    public static string From(IReadOnlyList<string> labels) => labels.Count switch
    {
        0 => string.Empty,
        1 => labels[0],
        _ => $"{string.Join(", ", labels.Take(labels.Count - 1))} or {labels[^1]}"
    };

    /// <summary>Whether a move asks for a note (R2-B04): "Required" or "No".</summary>
    public static string Note(bool requiresNote) => requiresNote ? "Required" : "No";


    /// <summary>The credit rules that credit nothing (R2-D03, R3-D04).</summary>
    public const string CreditsNothing = "Credits nothing.";

    /// <summary>"Credits nothing."'s line (R2-D03).</summary>
    public const string CreditsNothingLine =
        "An activity of this type counts towards no curriculum item, and its encounter date may not be after today.";

    /// <summary>The versions' heading (R2-B03 at h2 on a reader's Details, R2-E03 at h3 on the Publish tab).</summary>
    public const string VersionsHeading = "Versions";

    /// <summary>The versions table's caption (R2-B03).</summary>
    public const string VersionsCaption = "Published versions";

    public const string VersionColumn = "Version";

    public const string PublishedColumn = "Published";

    public const string ByColumn = "By";

    /// <summary>No version yet (R3-E01).</summary>
    public const string NotPublishedYet = "Not published yet.";

    /// <summary>Q3's line under the versions (R2-B03, R2-E03, R3-E01).</summary>
    public const string VersionsStay = "Activities filed under a version stay on it.";

    /// <summary>The day a version was published: the South African day, ISO (Q3, N13).</summary>
    public static string PublishedOn(DateTime publishedOnUtc)
        => ProgrammeCalendar.DateOf(publishedOnUtc).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>A version's number, the row header (R2-B03): "1".</summary>
    public static string Version(int version) => version.ToString(CultureInfo.InvariantCulture);
}

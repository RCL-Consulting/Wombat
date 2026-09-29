using System.Security.Claims;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetProgrammeStartForTrainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Web.Services;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// The one wording of a late filing (T160, D15), the day a form filed now would be filed on, and which forms can file
/// late at all.
/// </summary>
/// <remarks>
/// A filing's history row records how many days after the encounter it was filed, late or not
/// (<c>ActivityTransition.DaysAfterEncounter</c>), for a type that can credit. Only a late one is called out: "Filed 3
/// days after the encounter" on every submission would bury the one that matters.
/// </remarks>
public static class FilingLateness
{
    /// <summary>
    /// Today on the South African calendar, where the encounter date is typed and the write path takes "today".
    /// </summary>
    public static DateOnly Today() => Today(TimeProvider.System);

    /// <inheritdoc cref="Today()" />
    /// <remarks>
    /// Not the UTC date: between 22:00 and 24:00 UTC it is already tomorrow in South Africa, and a form that counted on
    /// the UTC date would call a filing a day less late than its history row will record.
    /// </remarks>
    public static DateOnly Today(TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return ProgrammeCalendar.DateOf(clock.GetUtcNow().UtcDateTime);
    }

    /// <summary>
    /// Today, when this viewer's next move on an existing activity could be its filing; otherwise null, so its form shows
    /// no lateness warning.
    /// </summary>
    /// <param name="activity">The activity as loaded, with its history.</param>
    /// <param name="actions">The moves the viewer may take from here, as the server listed them for the page.</param>
    /// <param name="viewer">The person looking at the page.</param>
    /// <remarks>
    /// <para>
    /// The server's rule (<c>ActivityService.IsTheFiling</c>), asked of the same rows: the filing is the author's first
    /// move out of the workflow's initial state that leads on. So the viewer must be the author (the subject or the
    /// creator), the activity must still be in its initial state and never have left it by such a move (a draft a
    /// supervisor returned was filed already), and one of the viewer's own moves from here must be such a move.
    /// </para>
    /// <para>
    /// The last condition is what keeps a type born in <c>requested</c> quiet: its create was the filing, and the
    /// author's only move there is the withdrawal. Which move the author presses is not known yet, so a draft's author
    /// who may both submit and cancel still sees the warning; it is only ever a warning. An unreadable workflow shows
    /// none.
    /// </para>
    /// </remarks>
    public static DateOnly? FiledOnFor(ActivityDto activity, IEnumerable<ActivityActionDto> actions, ClaimsPrincipal viewer)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(viewer);

        var viewerId = viewer.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(viewerId) ||
            !(string.Equals(viewerId, activity.SubjectUserId, StringComparison.Ordinal) ||
              string.Equals(viewerId, activity.CreatedByUserId, StringComparison.Ordinal)))
        {
            return null;
        }

        Workflow workflow;
        try
        {
            workflow = WorkflowParser.Parse(activity.WorkflowJson);
        }
        catch (Exception exception) when (exception is WorkflowParseException or ArgumentException)
        {
            return null;
        }

        if (!string.Equals(activity.CurrentState, workflow.InitialState, StringComparison.Ordinal) ||
            activity.Transitions.Any(row => workflow.LeftInitialStateLeadingOn(row.FromState, row.ToState, row.TransitionKey)))
        {
            return null;
        }

        var filingMoves = workflow.TransitionsLeadingOn(workflow.InitialState)
            .Select(transition => transition.Key)
            .ToHashSet(StringComparer.Ordinal);

        return actions.Any(action => filingMoves.Contains(action.TransitionKey)) ? Today() : null;
    }

    /// <summary>
    /// The subject's programme start, which the write path bounds a crediting type's encounter date by (T192), as
    /// <see cref="GetProgrammeStartForTraineeQuery" /> reads it for this viewer. Null when the subject holds no profile,
    /// when the viewer may not read it, or when it cannot be read.
    /// </summary>
    /// <remarks>
    /// A page asks once, where it passes <c>FiledOn</c>, and hands the answer to <c>ActivityForm.ProgrammeStartsOn</c>. A
    /// read that fails is no hint rather than a failed page: the hint is advisory, and the server's refusal still comes.
    /// It is logged as a warning, though: "no hint" is also what a trainee with no profile sees, so without the log a
    /// broken read (a query failing on the database, a handler missing) would pass for that and hide the hint everywhere.
    /// </remarks>
    public static async Task<DateOnly?> ProgrammeStartAsync(
        IScopedSender sender,
        string? subjectUserId,
        ClaimsPrincipal viewer,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(viewer);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(subjectUserId))
        {
            return null;
        }

        try
        {
            return await sender.Send(new GetProgrammeStartForTraineeQuery(subjectUserId, viewer));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "The programme start of trainee {SubjectUserId} could not be read, so the activity form shows no " +
                "pre-programme date hint. The server still refuses such a date.",
                subjectUserId);
            return null;
        }
    }

    /// <summary>
    /// Whether a form pinned to these credit rules warns of a late filing at all: only when they can credit
    /// (<see cref="EncounterDatePolicy.CanCredit" />), the predicate the server records a filing's lateness by. A form
    /// that warned where nothing is recorded would tell the author their filing "is recorded as late" untruly.
    /// </summary>
    /// <remarks>
    /// Rules the page cannot read warn of nothing, as the EPA picker's narrowing falls back to none on them
    /// (<c>CreditRuleFields</c>): the warning is advisory, and the server is the authority on what it records.
    /// </remarks>
    public static bool WarnsFor(string? creditRulesJson)
    {
        try
        {
            return EncounterDatePolicy.CanCredit(creditRulesJson);
        }
        catch (CreditRulesParseException)
        {
            return false;
        }
    }

    /// <summary>
    /// "Filed 15 days after the encounter" for a late filing; null for anything else.
    /// </summary>
    /// <remarks>
    /// Reads the record alone. The record is written only for a type that can credit (<see cref="WarnsFor" />'s
    /// predicate, asked by the server), so a type that credits nothing never shows it.
    /// </remarks>
    public static string? Label(int? daysAfterEncounter)
        => daysAfterEncounter is int days && EncounterDatePolicy.IsLateFiling(days)
            ? $"Filed {days} days after the encounter"
            : null;

    /// <summary>
    /// How many days late a filing on <paramref name="filedOn" /> would be, for the check line of Log an activity and of the
    /// author's draft (T342; the build review's D6): the encounter date as it stands in <paramref name="dataJson" />, under
    /// the schema's <paramref name="dateKey" />; null when it is not late, not a date, or before the programme start, which
    /// is refused rather than late (T160, T192). The caller asks only for a type that can credit (<see cref="WarnsFor" />).
    /// </summary>
    public static int? DaysLate(string? dataJson, string? dateKey, DateOnly filedOn, DateOnly? programmeStartsOn)
    {
        if (string.IsNullOrWhiteSpace(dateKey) || string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        try
        {
            if (System.Text.Json.Nodes.JsonNode.Parse(dataJson) is not System.Text.Json.Nodes.JsonObject data ||
                !data.TryGetPropertyValue(dateKey, out var node) ||
                node is not System.Text.Json.Nodes.JsonValue value ||
                !value.TryGetValue<string>(out var text) ||
                !DateOnly.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var encounteredOn) ||
                (programmeStartsOn is DateOnly start && encounteredOn < start))
            {
                return null;
            }

            var days = EncounterDatePolicy.DaysAfterEncounter(encounteredOn, filedOn);
            return EncounterDatePolicy.IsLateFiling(days) ? days : null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }
}

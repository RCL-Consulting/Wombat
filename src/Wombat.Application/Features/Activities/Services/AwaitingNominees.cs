using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Which activities wait on a named nominee, and on whom (T376, flow 12, F6): the one reading the nightly assessor nudge
/// (<c>AssessorPendingNudgeJob</c>) and the user page's waiting requests (<c>GetRequestsWaitingForUserQuery</c>) share, so
/// the dialog that warns "1 request is waiting for Mohammed Patel" counts the requests the nudge would mail him about.
/// </summary>
/// <remarks>
/// <para>
/// An activity waits on a nominee when it awaits a reviewer by the workflow it is PINNED to
/// (<see cref="ActivityWaiting.AwaitsReviewer" />, T297, read through <see cref="ActivityWaiting.LoadAwaitingReviewerAsync" />),
/// and the first move out of its state with a <c>field:</c> arm names someone in that field of its data. A move for a role
/// (a SpecialityAdmin's review of a teaching session) awaits a reviewer too, but names nobody.
/// </para>
/// <para>
/// Until T376 the nudge job parsed each pin's workflow itself and extracted the field on its own; the rule is unchanged,
/// only moved here: the first such move's first direct <c>field:</c> arm, its value a string in the data.
/// </para>
/// </remarks>
public static class AwaitingNominees
{
    /// <summary>
    /// The activities among <paramref name="activities" /> that wait on a nominee, each with whom, oldest first (by
    /// <see cref="Activity.UpdatedOn" />, then id). Loaded untracked, with each row's type.
    /// </summary>
    /// <param name="activities">
    /// Where to look: the nudge passes the activities older than its cutoff, the user page a coarse filter for one person.
    /// </param>
    public static async Task<IReadOnlyList<AwaitingNominee>> LoadAsync(
        IApplicationDbContext dbContext,
        IQueryable<Activity> activities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(activities);

        var awaiting = await ActivityWaiting.LoadAwaitingReviewerAsync(dbContext, cancellationToken);
        var candidates = await awaiting.Narrow(activities.AsNoTracking())
            .Include(activity => activity.ActivityType)
            .OrderBy(activity => activity.UpdatedOn)
            .ThenBy(activity => activity.Id)
            .ToListAsync(cancellationToken);

        return candidates
            .Where(activity => awaiting.Waits(activity.ActivityTypeId, activity.SchemaVersion, activity.CurrentState))
            .Select(activity => (Activity: activity, Nominee: NomineeOf(
                awaiting.WorkflowOf(activity.ActivityTypeId, activity.SchemaVersion), activity.CurrentState, activity.DataJson)))
            .Where(row => !string.IsNullOrWhiteSpace(row.Nominee))
            .Select(row => new AwaitingNominee(row.Activity, row.Nominee!))
            .ToList();
    }

    /// <summary>
    /// The activities among <paramref name="activities" /> that wait on <paramref name="userId" />: a coarse filter in SQL
    /// (the data's text holds the id), then <see cref="LoadAsync" />'s exact rule.
    /// </summary>
    public static async Task<IReadOnlyList<AwaitingNominee>> LoadForAsync(
        IApplicationDbContext dbContext,
        IQueryable<Activity> activities,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // The stored JSON holds an id verbatim only while it needs no escaping (WaitingStates.NarrowToCaller's rule); any
        // other id is looked for on every row.
        var verbatim = userId.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or '@');
        var narrowed = verbatim
            // DataJson is jsonb: read as text to search it (Convert.ToString is PostgreSQL's ::text; InMemory's identity).
            ? activities.Where(activity => Convert.ToString(activity.DataJson)!.Contains(userId))
            : activities;

        return (await LoadAsync(dbContext, narrowed, cancellationToken))
            .Where(row => string.Equals(row.NomineeUserId, userId, StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>
    /// Whom an activity in <paramref name="stateKey" />, pinned to <paramref name="workflow" />, waits on: the value of the
    /// field the first move out of the state with a <c>field:</c> arm names; null when it awaits no reviewer or names
    /// nobody.
    /// </summary>
    public static string? NomineeOf(Workflow? workflow, string stateKey, string dataJson)
    {
        if (workflow is null || !ActivityWaiting.AwaitsReviewer(workflow, stateKey))
        {
            return null;
        }

        var move = workflow.Transitions
            .Where(transition => transition.From.Contains(stateKey, StringComparer.Ordinal))
            .FirstOrDefault(transition => HasFieldUserActor(transition.Actor));

        return move is not null && FieldNameOf(move.Actor) is { } field ? FieldValue(dataJson, field) : null;
    }

    private static bool HasFieldUserActor(ActorRule rule) => rule switch
    {
        FieldUserActorRule => true,
        CombinedActorRule combined => combined.Rules.Any(HasFieldUserActor),
        _ => false
    };

    private static string? FieldNameOf(ActorRule rule) => rule switch
    {
        FieldUserActorRule field => field.Field,
        CombinedActorRule combined => combined.Rules.OfType<FieldUserActorRule>().FirstOrDefault()?.Field,
        _ => null
    };

    private static string? FieldValue(string dataJson, string fieldName)
    {
        try
        {
            using var document = JsonDocument.Parse(dataJson);
            return document.RootElement.TryGetProperty(fieldName, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>An activity that waits on a nominee, and who that is (<see cref="AwaitingNominees" />).</summary>
public sealed record AwaitingNominee(Activity Activity, string NomineeUserId);

using System.Text.Json;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Who has an activity now, whom it names as its nominee, and whether it came back to its author: the one reading of
/// each that My activities, Needs you and the activity's status card share (T342, B6, B7). Pure: every answer is read
/// from the activity's PINNED workflow, its data and its newest recorded move, never from a state's key.
/// </summary>
/// <remarks>
/// <para>
/// <b>Who has it</b> (<see cref="Resolve" />) is told from the moves out of the state that lead on
/// (<see cref="Workflow.TransitionsLeadingOn" />), as Needs you and the Activity inbox are (<see cref="ActivityWaiting" />):
/// a withdrawal hands the activity to no one, so a requested Mini-CEX its registrar may still cancel is with its assessor.
/// </para>
/// <list type="bullet">
/// <item><c>terminal: true</c>: <see cref="ActivityHolderKind.Done" />, whatever else the state declares.</item>
/// <item>
/// No move leads on (a declined or cancelled request, a dead end the CPSA workflows keep non-terminal so that no credit
/// fires): <see cref="ActivityHolderKind.Closed" />.
/// </item>
/// <item>Every arm of every such move is the author's (<see cref="ActivityWaiting.IsAuthorArm" />): <see cref="ActivityHolderKind.Author" />.</item>
/// <item>
/// Otherwise the other arms decide, and the author's are set aside: when each is a <c>field:</c> arm and all of them name
/// the same user in the data, <see cref="ActivityHolderKind.Person" />; else (a <c>role:</c> or <c>scope:</c> arm, or an
/// empty field) <see cref="ActivityHolderKind.Waiting" />, which the page words "Waiting for &lt;state label&gt;.".
/// </item>
/// </list>
/// <para>
/// A pin with no workflow, or a state its workflow does not declare, is <see cref="ActivityHolderKind.Waiting" />: the
/// page can still name the state, and must not claim a person it cannot read.
/// </para>
/// </remarks>
public static class ActivityHolders
{
    /// <summary>
    /// Who has the activity now. <see cref="ActivityHolderDto.Name" /> is left null: the caller looks up every name its
    /// page shows in one call (<c>UserDisplayNames</c>) and fills it (<see cref="WithName" />).
    /// </summary>
    /// <param name="since">When it entered its state: its newest recorded move.</param>
    /// <param name="callerUserId">Who is asking, for <see cref="ActivityHolderDto.IsViewer" />; null for no one.</param>
    public static ActivityHolderDto Resolve(
        Workflow? workflow,
        string currentState,
        string subjectUserId,
        string createdByUserId,
        string? dataJson,
        DateTime? since,
        string? callerUserId)
    {
        ArgumentNullException.ThrowIfNull(currentState);

        var state = workflow?.States.FirstOrDefault(candidate => string.Equals(candidate.Key, currentState, StringComparison.Ordinal));
        if (workflow is null || state is null)
        {
            return new ActivityHolderDto(ActivityHolderKind.Waiting, null, null, false, since);
        }

        if (state.Terminal)
        {
            return new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, since);
        }

        var arms = workflow.TransitionsLeadingOn(currentState).SelectMany(transition => ArmsOf(transition.Actor)).ToList();
        if (arms.Count == 0)
        {
            return new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, since);
        }

        var others = arms.Where(arm => !ActivityWaiting.IsAuthorArm(arm)).ToList();
        if (others.Count == 0)
        {
            var isAuthor = callerUserId is not null &&
                           (string.Equals(callerUserId, subjectUserId, StringComparison.Ordinal) ||
                            string.Equals(callerUserId, createdByUserId, StringComparison.Ordinal));
            return new ActivityHolderDto(ActivityHolderKind.Author, subjectUserId, null, isAuthor, since);
        }

        var named = others
            .Select(arm => NamedField(arm) is { } field ? ReadUserField(dataJson, field) : null)
            .ToList();
        if (named.All(userId => userId is not null) &&
            named.Distinct(StringComparer.Ordinal).Count() == 1)
        {
            var userId = named[0]!;
            return new ActivityHolderDto(
                ActivityHolderKind.Person, userId, null, string.Equals(callerUserId, userId, StringComparison.Ordinal), since);
        }

        return new ActivityHolderDto(ActivityHolderKind.Waiting, null, null, false, since);
    }

    /// <summary>The holder with its user's name filled in; unchanged for a holder that is no user.</summary>
    public static ActivityHolderDto WithName(ActivityHolderDto holder, Func<string, string> nameOf)
    {
        ArgumentNullException.ThrowIfNull(holder);
        ArgumentNullException.ThrowIfNull(nameOf);

        return holder.UserId is { } userId ? holder with { Name = nameOf(userId) } : holder;
    }

    /// <summary>
    /// The user field that names the activity's nominee, the person it goes to or is discussed with (T342, B7): the field
    /// a <c>field:</c> arm of the next move that leads on names; else, for a state whose next move is the author's or that
    /// is finished, the first field any move's rule names, in declaration order. Null for a workflow that names no one in
    /// a field: a log's Supervising consultant is a user field that receives nothing, so it is no one's nominee (C3).
    /// </summary>
    public static string? NomineeField(Workflow? workflow, string currentState)
    {
        if (workflow is null)
        {
            return null;
        }

        return workflow.TransitionsLeadingOn(currentState).SelectMany(transition => FieldsNamedBy(transition.Actor)).FirstOrDefault()
               ?? workflow.Transitions.SelectMany(transition => FieldsNamedBy(transition.Actor)).FirstOrDefault();
    }

    /// <summary>
    /// The return, when the activity's newest recorded move entered the workflow's initial state from another state and
    /// was made by someone other than its author: a supervisor sent it back (T342, B6). Null otherwise, and for no
    /// workflow or no move. <see cref="ActivityReturnDto.ByName" /> is the mover's id until the caller fills the name.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The create row runs from the initial state to itself, so a new draft is never returned. The newest move decides: a
    /// returned reflection that is submitted again and returned a second time is returned by the second.
    /// </para>
    /// <para>
    /// The author's own move back is no return (T342, R2): a declined reflective note she takes back to draft
    /// (<c>revise</c>, <c>subject</c>) was not sent back to her. So the mover must be neither the subject nor the creator,
    /// and the move the workflow records under its key must not be the author's alone
    /// (<see cref="ActivityWaiting.IsAuthorArm" />). The second holds when the author also holds another role.
    /// </para>
    /// </remarks>
    public static ActivityReturnDto? ReturnOf(
        Workflow? workflow,
        string currentState,
        string subjectUserId,
        string createdByUserId,
        ActivityLastMove? lastMove)
    {
        if (workflow is null || lastMove is null ||
            !string.Equals(currentState, workflow.InitialState, StringComparison.Ordinal) ||
            !string.Equals(lastMove.ToState, workflow.InitialState, StringComparison.Ordinal) ||
            string.Equals(lastMove.FromState, workflow.InitialState, StringComparison.Ordinal) ||
            string.Equals(lastMove.ActorUserId, subjectUserId, StringComparison.Ordinal) ||
            string.Equals(lastMove.ActorUserId, createdByUserId, StringComparison.Ordinal))
        {
            return null;
        }

        var recorded = lastMove.TransitionKey is { } key
            ? workflow.Transitions.FirstOrDefault(transition =>
                string.Equals(transition.Key, key, StringComparison.Ordinal) &&
                transition.From.Contains(lastMove.FromState, StringComparer.Ordinal) &&
                string.Equals(transition.To, lastMove.ToState, StringComparison.Ordinal))
            : null;
        if (recorded is not null && ActivityWaiting.IsAuthorArm(recorded.Actor))
        {
            return null;
        }

        return new ActivityReturnDto(lastMove.ActorUserId, lastMove.ActorUserId, lastMove.OccurredOn, lastMove.Note);
    }

    /// <summary>
    /// A user field's value in the activity's data: the id it holds, or null when the field is absent, empty or not a
    /// string. Total: data that does not parse names no one.
    /// </summary>
    public static string? ReadUserField(string? dataJson, string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(dataJson) || string.IsNullOrWhiteSpace(fieldKey))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   document.RootElement.TryGetProperty(fieldKey, out var value) &&
                   value.ValueKind == JsonValueKind.String &&
                   !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The arms of a rule: a disjunction's, flattened; any other rule is one arm.</summary>
    private static IEnumerable<ActorRule> ArmsOf(ActorRule rule)
        => rule is CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any
            ? any.Rules.SelectMany(ArmsOf)
            : [rule];

    /// <summary>The field an arm names a person by: a <c>field:</c> arm, or a conjunction holding one; else null.</summary>
    private static string? NamedField(ActorRule arm) => arm switch
    {
        FieldUserActorRule field => field.Field,
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.Select(NamedField).FirstOrDefault(field => field is not null),
        _ => null
    };

    /// <summary>Every field a rule's <c>field:</c> arms name, in the order the rule declares them.</summary>
    private static IEnumerable<string> FieldsNamedBy(ActorRule rule) => rule switch
    {
        FieldUserActorRule field => [field.Field],
        CombinedActorRule combined => combined.Rules.SelectMany(FieldsNamedBy),
        _ => []
    };
}

/// <summary>An activity's newest recorded move: what <see cref="ActivityHolders" /> reads "since" and "returned" from.</summary>
/// <param name="TransitionKey">The move's key as recorded, for whose move it was (T342, R2); null when not read.</param>
public sealed record ActivityLastMove(
    string FromState,
    string ToState,
    string ActorUserId,
    DateTime OccurredOn,
    string? Note,
    string? TransitionKey = null);

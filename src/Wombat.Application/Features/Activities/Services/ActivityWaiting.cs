using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Whether an activity is waiting, and on whom, read from the workflow version it is PINNED to: the one reading the
/// Activity Inbox, the dashboards' waiting cards and the assessor nudge share (T297), as <see cref="ActivityCompletion" />
/// is the one reading of "finished" (T203).
/// </summary>
/// <remarks>
/// <para>
/// Two questions, each answered from the workflow and never from a state's key:
/// </para>
/// <list type="bullet">
/// <item>
/// <b>Actionable by the caller</b> (<see cref="LoadActionableAsync" />): some move out of the activity's state that leads
/// on is one the caller may make, by the act gate (<see cref="IWorkflowEvaluator" />). By the arms of the rule that are
/// not the author's, less the caller's own subject rows, <see cref="WaitingForYou" /> lists these, oldest first: the
/// Activity inbox, the Assessor's Home and the activity page's way on, so a card cannot disagree with the page it links to
/// (T350, note 5); by the author's arms, Needs you lists them (T342, B6).
/// </item>
/// <item>
/// <b>Awaiting a reviewer</b> (<see cref="AwaitsReviewer" />): the state is not terminal, and some move out of it belongs
/// to someone other than the activity's author, because its actor has a <c>field:</c> or <c>role:</c> arm. The
/// Coordinator's stalled card and the programme admins' pending tile count these, and the assessor nudge mails about the
/// ones whose move names a nominee in a field.
/// </item>
/// </list>
/// <para>
/// Until T297 five cards read state keys (<c>requested</c>, <c>accepted</c>, <c>submitted</c>, <c>in_review</c>,
/// <c>declined</c>, <c>cancelled</c>, <c>draft</c>). Every rated CPSA instrument waits in <c>requested</c>, a portfolio
/// review in <c>submitted</c>, and no CPSA workflow has <c>accepted</c>, so each card disagreed with the page it opened:
/// an assessor read "0 assessments awaiting review" beside a full inbox. <c>DashboardStateLiteralTests</c> keeps literal
/// states out of the dashboards' queries.
/// </para>
/// <para>
/// A pin resolves to its published version's workflow (<see cref="ActivityTypeVersion" />) and to nothing else: that is
/// the version a move on the activity is judged against (<c>ActivityService</c>, which refuses a move on a pin with no
/// version row). So a pin with no version row, or whose workflow no longer parses, waits on nobody. This is deliberately
/// not <see cref="PinnedWorkflows.LoadAsync" />, which falls back to the type's own columns to name a state or say it is
/// finished: a card must not offer work that no move could do.
/// </para>
/// <para>
/// Each reader narrows in SQL first, to the types and states that can wait at all (<see cref="WaitingStates.Narrow" />),
/// then decides each row exactly from its own pin. Not by the caller's read scope: the list read rule
/// (<c>ActivityReadScope.WhereReadableBy</c>) is deliberately narrower than the act gate and omits the <c>field:</c> arm
/// that names an assessor, so it would empty an assessor's inbox. A reader that must be confined to what the caller may
/// read (the Coordinator's card) applies it itself.
/// </para>
/// <para>
/// The actionable read narrows further, to the rows the caller could reach at all (<see cref="WaitingStates.NarrowToCaller" />):
/// a coarse superset of the act gate's arms, so that a trainee's Home reads her own rows and the rows naming her, not
/// every open activity at every institution (T297 review). The act gate still decides each row that survives.
/// </para>
/// </remarks>
public static class ActivityWaiting
{
    /// <summary>
    /// The activities in <paramref name="activities" /> that <paramref name="principal" /> can move now, most recently
    /// updated first, each with the pinned workflow that says so. Each reader orders them its own way: Needs you as they
    /// come, <see cref="WaitingForYou" /> oldest first (T350, note 5).
    /// </summary>
    /// <param name="activities">
    /// Where to look: <c>Set&lt;Activity&gt;()</c>, passed by the handler so that the read boundary's scan
    /// (<c>ActivityReadBoundaryTests</c>) sees the handler read activity rows. Loaded untracked, with each row's type: the
    /// act gate reads the type's scope (<c>scope:global</c>).
    /// </param>
    /// <param name="withTransitions">
    /// Whether each row is loaded with its transitions: the inbox's credit column reads them; the act gate and the cards
    /// do not, so they leave them in the database.
    /// </param>
    /// <param name="arms">
    /// Which arms of each move's actor rule may admit the caller (T342, B6): <see cref="ActorArms.Author" /> for Needs
    /// you, the caller's own drafts and returned work; <see cref="ActorArms.NotAuthor" /> for the Activity inbox, the work
    /// that waits on the caller as someone else's assessor, reviewer or admin; <see cref="ActorArms.All" /> for both.
    /// </param>
    public static async Task<IReadOnlyList<ActionableActivity>> LoadActionableAsync(
        IQueryable<Activity> activities,
        IApplicationDbContext dbContext,
        IWorkflowEvaluator workflowEvaluator,
        ClaimsPrincipal principal,
        bool withTransitions = false,
        CancellationToken cancellationToken = default,
        ActorArms arms = ActorArms.All)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(workflowEvaluator);
        ArgumentNullException.ThrowIfNull(principal);

        var movable = await LoadMovableAsync(dbContext, cancellationToken);

        IQueryable<Activity> query = movable.NarrowToCaller(activities.AsNoTracking(), principal, arms)
            .Include(activity => activity.ActivityType);
        if (withTransitions)
        {
            query = query.Include(activity => activity.Transitions);
        }

        var candidates = await query
            .OrderByDescending(activity => activity.UpdatedOn)
            .ThenByDescending(activity => activity.Id)
            .ToListAsync(cancellationToken);

        return candidates
            .Select(activity => (Activity: activity, Workflow: movable.WorkflowOf(activity.ActivityTypeId, activity.SchemaVersion)))
            .Where(row => row.Workflow is { } workflow && IsActionableBy(workflow, row.Activity, principal, workflowEvaluator, arms))
            .Select(row => new ActionableActivity(row.Activity, row.Workflow!))
            .ToList();
    }

    /// <summary>
    /// Whether some move out of the activity's state that leads on (<see cref="Workflow.TransitionsLeadingOn" />), in
    /// <paramref name="workflow" />, is one the caller may make by the <paramref name="arms" /> asked for.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only the moves that lead on (T342, B6). A withdrawal is not work waiting on anyone: a requested Mini-CEX whose
    /// registrar may only cancel it (cancel goes into a dead end) is with its assessor, not with her, so it is neither in
    /// her Needs you nor in her inbox (Step 3.12), while the assessor's Complete keeps it in his. Until T342 any move
    /// counted, so every request a trainee had sent sat in her own Activity inbox beside the work waiting on her.
    /// </para>
    /// <para>
    /// The arms are cut from the rule before the act gate judges it (<see cref="RestrictTo(Workflow, ActorArms)" />), so
    /// the gate still decides each arm that is left, exactly as it decides the move.
    /// </para>
    /// </remarks>
    public static bool IsActionableBy(
        Workflow workflow,
        Activity activity,
        ClaimsPrincipal principal,
        IWorkflowEvaluator workflowEvaluator,
        ActorArms arms = ActorArms.All)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(activity);

        var leadingOn = workflow.TransitionsLeadingOn(activity.CurrentState).Select(transition => transition.Key).ToList();
        if (leadingOn.Count == 0)
        {
            return false;
        }

        var judged = RestrictTo(workflow, arms);
        return leadingOn.Any(key => workflowEvaluator.Evaluate(judged, activity, key, principal).Allowed);
    }

    /// <summary>
    /// <paramref name="workflow" /> with every move's actor rule cut to the <paramref name="arms" /> asked for; the
    /// workflow itself for <see cref="ActorArms.All" />.
    /// </summary>
    public static Workflow RestrictTo(Workflow workflow, ActorArms arms)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        return arms == ActorArms.All
            ? workflow
            : workflow with
            {
                Transitions = workflow.Transitions
                    .Select(transition => transition with { Actor = RestrictTo(transition.Actor, arms) })
                    .ToList()
            };
    }

    /// <summary>
    /// An actor rule cut to the <paramref name="arms" /> asked for (T342, B6). A disjunction is cut arm by arm; any other
    /// rule is kept or dropped whole, by whether it is the author's (<see cref="IsAuthorArm" />). A dropped rule admits
    /// nobody: an empty disjunction, which both the act gate and <see cref="ReachOf" /> read as no one.
    /// </summary>
    public static ActorRule RestrictTo(ActorRule rule, ActorArms arms)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return arms switch
        {
            ActorArms.All => rule,
            _ when rule is CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any
                => new CombinedActorRule(
                    ActorRuleCombinationKind.Any,
                    any.Rules.Select(child => RestrictTo(child, arms)).Where(child => !IsNobody(child)).ToList()),
            ActorArms.Author => IsAuthorArm(rule) ? rule : Nobody,
            _ => IsAuthorArm(rule) ? Nobody : rule
        };
    }

    /// <summary>
    /// Whether an arm of an actor rule is the activity's author's: <c>subject</c>, <c>creator</c>, or a conjunction
    /// confined to one of them (<c>subject+scope:institution</c> is still the author, only narrowed). A disjunction is
    /// the author's when each of its arms is.
    /// </summary>
    public static bool IsAuthorArm(ActorRule rule) => rule switch
    {
        SubjectUserActorRule or CreatorUserActorRule => true,
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.Any(IsAuthorArm),
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any => any.Rules.Count > 0 && any.Rules.All(IsAuthorArm),
        _ => false
    };

    /// <summary>The rule no one matches: a disjunction of nothing.</summary>
    private static readonly ActorRule Nobody = new CombinedActorRule(ActorRuleCombinationKind.Any, []);

    private static bool IsNobody(ActorRule rule)
        => rule is CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any, Rules.Count: 0 };

    /// <summary>
    /// Whether an activity pinned to <paramref name="workflow" /> and in <paramref name="stateKey" /> awaits a reviewer: the
    /// state is declared and not terminal, and some move out of it belongs to someone other than the author
    /// (<see cref="BelongsToSomeoneOtherThanTheAuthor" />). False for no workflow.
    /// </summary>
    /// <remarks>
    /// A requested Mini-CEX awaits its assessor, a portfolio review in <c>submitted</c> its supervisor, a submitted
    /// teaching session its SpecialityAdmin. A draft does not (only its author moves it), nor a declined or cancelled
    /// request (no move left), nor finished work.
    /// </remarks>
    public static bool AwaitsReviewer(Workflow? workflow, string stateKey)
    {
        if (workflow is null)
        {
            return false;
        }

        var state = workflow.States.FirstOrDefault(candidate => string.Equals(candidate.Key, stateKey, StringComparison.Ordinal));
        return state is { Terminal: false } &&
               workflow.Transitions.Any(transition =>
                   transition.From.Contains(stateKey, StringComparer.Ordinal) &&
                   BelongsToSomeoneOtherThanTheAuthor(transition.Actor));
    }

    /// <summary>
    /// Whether an actor rule has a <c>field:</c> or a <c>role:</c> arm: a move that someone other than the activity's
    /// author (its <c>subject</c> or <c>creator</c>) makes, such as the assessor a field names or a programme's admin.
    /// </summary>
    public static bool BelongsToSomeoneOtherThanTheAuthor(ActorRule rule) => rule switch
    {
        FieldUserActorRule or NamedRoleActorRule => true,
        CombinedActorRule combined => combined.Rules.Any(BelongsToSomeoneOtherThanTheAuthor),
        _ => false
    };

    /// <summary>
    /// Which rows an actor rule could admit the caller on, as far as the columns SQL reads can tell: a superset of what the
    /// act gate (<c>ActorRuleMatcher</c>) admits, arm by arm, so narrowing by it never drops a row the gate would keep.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>subject</c>, <c>creator</c>: the rows whose subject, or creator, is the caller.</item>
    /// <item><c>field:</c>: the rows whose data names the caller (<see cref="CallerReach.NamedInData" />).</item>
    /// <item><c>role:</c>: every row when the caller holds the role, none when not; the gate asks nothing of the row.</item>
    /// <item>
    /// <c>scope:global</c>: every row, since it asks only whether the type is global. Every other known scope conjoins the
    /// activity's institution stamp with the caller's (the speciality arms add a speciality), so it reaches the rows
    /// stamped to the caller's institution, and none for a caller with no institution. An unknown scope admits nobody.
    /// </item>
    /// <item><c>a|b</c>: what either reaches. <c>a+b</c>: nothing when an arm reaches nothing, else its narrowest arm's.</item>
    /// </list>
    /// </remarks>
    public static CallerReach ReachOf(ActorRule rule, ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(principal);

        return rule switch
        {
            SubjectUserActorRule => CallerReach.Subject,
            CreatorUserActorRule => CallerReach.Creator,
            FieldUserActorRule => CallerReach.NamedInData,
            // The instance IsInRole, as the gate calls it.
            NamedRoleActorRule role => principal.IsInRole(role.Role) ? CallerReach.AnyRow : CallerReach.None,
            ScopeMatchActorRule { Scope: "global" } => CallerReach.AnyRow,
            ScopeMatchActorRule { Scope: "institution" or "speciality" or "subspeciality" or "sub_speciality" }
                => principal.GetInstitutionId() is null ? CallerReach.None : CallerReach.CallersInstitution,
            CombinedActorRule { CombinationKind: ActorRuleCombinationKind.Any } any
                => any.Rules.Aggregate(CallerReach.None, (reach, child) => reach | ReachOf(child, principal)),
            CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all
                => Conjoin(all.Rules.Select(child => ReachOf(child, principal)).ToList()),
            _ => CallerReach.None
        };
    }

    /// <summary>
    /// A conjunction admits no more rows than any one of its arms: none when some arm reaches none, else the first arm
    /// confined to some rows (every row when each arm reaches every row, as an empty conjunction does).
    /// </summary>
    private static CallerReach Conjoin(IReadOnlyList<CallerReach> arms)
        => arms.Any(arm => arm == CallerReach.None)
            ? CallerReach.None
            : arms.FirstOrDefault(arm => !arm.HasFlag(CallerReach.AnyRow), CallerReach.AnyRow);

    /// <summary>
    /// The published workflows, and the states of each that have a move out of them: every state in which an activity
    /// could be actionable by someone.
    /// </summary>
    public static Task<WaitingStates> LoadMovableAsync(IApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        => LoadAsync(dbContext, static (workflow, stateKey) => workflow.HasOutgoingTransition(stateKey), cancellationToken);

    /// <summary>The published workflows, and the states of each that await a reviewer (<see cref="AwaitsReviewer" />).</summary>
    public static Task<WaitingStates> LoadAwaitingReviewerAsync(IApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        => LoadAsync(dbContext, static (workflow, stateKey) => AwaitsReviewer(workflow, stateKey), cancellationToken);

    /// <remarks>
    /// Every published version's workflow, read in one query and parsed once: the version table holds one row per publish,
    /// so it is small beside the activities it decides.
    /// </remarks>
    private static async Task<WaitingStates> LoadAsync(
        IApplicationDbContext dbContext,
        Func<Workflow, string, bool> waits,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var versions = await dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Select(version => new { version.ActivityTypeId, version.Version, version.WorkflowJson })
            .ToListAsync(cancellationToken);

        var workflows = new Dictionary<(int ActivityTypeId, int Version), Workflow>();
        foreach (var version in versions)
        {
            if (!workflows.ContainsKey((version.ActivityTypeId, version.Version)) &&
                PinnedWorkflows.TryParse(version.WorkflowJson) is { } workflow)
            {
                workflows[(version.ActivityTypeId, version.Version)] = workflow;
            }
        }

        return new WaitingStates(workflows, waits);
    }
}

/// <summary>An activity the caller can move now, with the pinned workflow that says so, which also names its state (T220).</summary>
public sealed record ActionableActivity(Activity Activity, Workflow Workflow);

/// <summary>
/// Which arms of a move's actor rule the actionable read lets admit the caller (T342, B6): the author's own
/// (<c>subject</c>, <c>creator</c>), everyone else's (<c>field:</c>, <c>role:</c>, <c>scope:</c>), or both.
/// </summary>
public enum ActorArms
{
    /// <summary>Every arm: what the caller may move, whoever they are to the activity.</summary>
    All = 0,

    /// <summary>The author's arms only: the caller's own drafts and the work returned to them. Needs you.</summary>
    Author = 1,

    /// <summary>Every arm but the author's: the work that waits on the caller for someone else. The Activity inbox.</summary>
    NotAuthor = 2
}

/// <summary>
/// The rows an actor rule could admit a caller on, told from the columns alone (<see cref="ActivityWaiting.ReachOf" />).
/// </summary>
[Flags]
public enum CallerReach
{
    /// <summary>No row: the caller can never make the move.</summary>
    None = 0,

    /// <summary>Any row: a role the caller holds, or <c>scope:global</c>, which asks only about the type.</summary>
    AnyRow = 1,

    /// <summary>The rows whose subject is the caller.</summary>
    Subject = 2,

    /// <summary>The rows the caller created.</summary>
    Creator = 4,

    /// <summary>The rows whose data holds the caller's id: a <c>field:</c> arm naming them.</summary>
    NamedInData = 8,

    /// <summary>The rows stamped to the caller's institution: a <c>scope:</c> arm other than <c>global</c>.</summary>
    CallersInstitution = 16
}

/// <summary>
/// The published workflow of every pin, and which of their states wait in the sense <see cref="ActivityWaiting" /> loaded
/// it for: a coarse filter for SQL (<see cref="Narrow" />) and the exact answer per pin (<see cref="Waits" />).
/// </summary>
public sealed class WaitingStates
{
    private readonly IReadOnlyDictionary<(int ActivityTypeId, int Version), Workflow> _workflows;
    private readonly HashSet<(int ActivityTypeId, int Version, string State)> _waiting;
    private readonly int[] _typeIds;
    private readonly string[] _stateKeys;

    internal WaitingStates(
        IReadOnlyDictionary<(int ActivityTypeId, int Version), Workflow> workflows,
        Func<Workflow, string, bool> waits)
    {
        _workflows = workflows;
        _waiting = workflows
            .SelectMany(pair => pair.Value.States
                .Where(state => waits(pair.Value, state.Key))
                .Select(state => (pair.Key.ActivityTypeId, pair.Key.Version, state.Key)))
            .ToHashSet();
        _typeIds = _waiting.Select(entry => entry.ActivityTypeId).Distinct().ToArray();
        _stateKeys = _waiting.Select(entry => entry.State).Distinct(StringComparer.Ordinal).ToArray();
    }

    /// <summary>The published workflow of a pin, or null when it has no version row or its workflow does not parse.</summary>
    public Workflow? WorkflowOf(int activityTypeId, int version)
        => _workflows.TryGetValue((activityTypeId, version), out var workflow) ? workflow : null;

    /// <summary>Whether an activity on this pin, in this state, waits.</summary>
    public bool Waits(int activityTypeId, int version, string stateKey)
        => _waiting.Contains((activityTypeId, version, stateKey));

    /// <summary>
    /// The activities that could wait, in SQL: of a type with a waiting state in some version, in a state that waits in
    /// some version. A superset; <see cref="Waits" /> decides each row by its own pin.
    /// </summary>
    public IQueryable<Activity> Narrow(IQueryable<Activity> activities)
    {
        ArgumentNullException.ThrowIfNull(activities);

        var typeIds = _typeIds;
        var stateKeys = _stateKeys;
        return activities.Where(activity => typeIds.Contains(activity.ActivityTypeId) && stateKeys.Contains(activity.CurrentState));
    }

    /// <summary>
    /// The activities that could wait (<see cref="Narrow" />) and that the caller could reach at all: for each waiting
    /// state, what every move out of it could admit the caller on (<see cref="ActivityWaiting.ReachOf" />). A superset of
    /// what the act gate admits, which decides each row that survives.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each arm is a list of types and a list of states, as <see cref="Narrow" /> is, so it too may keep a row its own pin
    /// would not: cheap, and still a superset. Until the T297 review the actionable read loaded every open activity at
    /// every institution, with its transitions, and ran the act gate on each, for every trainee's and assessor's Home.
    /// </para>
    /// <para>
    /// A <c>field:</c> arm is a search of the data's text for the caller's id. The stored JSON holds an id verbatim only
    /// while it needs no escaping, so an id with anything but ASCII letters, digits, <c>-</c>, <c>_</c>, <c>.</c> and
    /// <c>@</c> is reached on every row, as a held role is. A caller with no id is admitted by no <c>subject</c>,
    /// <c>creator</c> or <c>field:</c> arm, as in the gate.
    /// </para>
    /// <para>
    /// Only the moves that lead on are asked (<see cref="Workflow.TransitionsLeadingOn" />), each cut to the
    /// <paramref name="arms" /> asked for, as the act gate is asked of them (<see cref="ActivityWaiting.IsActionableBy" />,
    /// T342, B6).
    /// </para>
    /// </remarks>
    public IQueryable<Activity> NarrowToCaller(IQueryable<Activity> activities, ClaimsPrincipal principal, ActorArms arms = ActorArms.All)
    {
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(principal);

        var callerId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var institutionId = principal.GetInstitutionId();
        var idIsVerbatimInJson = callerId is not null &&
                                 callerId.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or '@');

        var reaches = _waiting
            .Select(entry => (entry.ActivityTypeId, entry.State, Reach: Effective(_workflows[(entry.ActivityTypeId, entry.Version)]
                .TransitionsLeadingOn(entry.State)
                .Aggregate(CallerReach.None, (reach, transition)
                    => reach | ActivityWaiting.ReachOf(ActivityWaiting.RestrictTo(transition.Actor, arms), principal)))))
            .ToList();

        CallerReach Effective(CallerReach reach)
        {
            if (callerId is null)
            {
                return reach & ~(CallerReach.Subject | CallerReach.Creator | CallerReach.NamedInData);
            }

            return !idIsVerbatimInJson && reach.HasFlag(CallerReach.NamedInData) ? reach | CallerReach.AnyRow : reach;
        }

        (int[] TypeIds, string[] States) Arm(CallerReach arm)
        {
            var rows = reaches.Where(row => row.Reach.HasFlag(arm)).ToList();
            return (rows.Select(row => row.ActivityTypeId).Distinct().ToArray(),
                rows.Select(row => row.State).Distinct(StringComparer.Ordinal).ToArray());
        }

        var (anyRowTypes, anyRowStates) = Arm(CallerReach.AnyRow);
        var (subjectTypes, subjectStates) = Arm(CallerReach.Subject);
        var (creatorTypes, creatorStates) = Arm(CallerReach.Creator);
        var (namedTypes, namedStates) = Arm(CallerReach.NamedInData);
        var (institutionTypes, institutionStates) = Arm(CallerReach.CallersInstitution);
        var id = callerId ?? string.Empty;
        var hasInstitution = institutionId.HasValue;

        return Narrow(activities).Where(activity =>
            (anyRowTypes.Contains(activity.ActivityTypeId) && anyRowStates.Contains(activity.CurrentState)) ||
            (subjectTypes.Contains(activity.ActivityTypeId) && subjectStates.Contains(activity.CurrentState) &&
             activity.SubjectUserId == id) ||
            (creatorTypes.Contains(activity.ActivityTypeId) && creatorStates.Contains(activity.CurrentState) &&
             activity.CreatedByUserId == id) ||
            // DataJson is jsonb: read as text to search it (Convert.ToString is PostgreSQL's ::text; InMemory's identity).
            (namedTypes.Contains(activity.ActivityTypeId) && namedStates.Contains(activity.CurrentState) &&
             Convert.ToString(activity.DataJson)!.Contains(id)) ||
            (hasInstitution && institutionTypes.Contains(activity.ActivityTypeId) && institutionStates.Contains(activity.CurrentState) &&
             activity.InstitutionId == institutionId));
    }
}

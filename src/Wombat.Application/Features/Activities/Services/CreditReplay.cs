using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Which stored activities a replay of credit evaluates, and with what: the rebuild (<c>RebuildCurriculumProgress</c>) and
/// a reactivated EPA's resumed credit (<c>ResumedEpaCredit</c>, T196) ask the same question the same way.
/// </summary>
internal static class CreditReplay
{
    /// <summary>
    /// The pinned version's credit rules and schema, as the live path passes them, when the activity sits in a terminal
    /// state of its pinned workflow and those rules declare credit; otherwise null.
    /// </summary>
    /// <remarks>
    /// The same <c>counts_for</c> gate the live path checks BEFORE calling the applier (<c>ActivityService</c>). Checking it
    /// here rather than reading a zero out of the result is what keeps the transition's stamp three-valued: a reflective
    /// note, journal club, procedure log, QI project, research output or teaching session declares an empty
    /// <c>counts_for</c>, was never evaluated, and must stay null rather than be flagged as having credited nothing. The
    /// schema travels with the rules because it binds the achieved ordinal to its ladder (T109), and pinning means a
    /// replay reaches the same binding the original completion did.
    /// </remarks>
    public static ActivityType? PinnedCreditingType(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);

        var pinnedVersion = activity.ActivityType.Versions.SingleOrDefault(version => version.Version == activity.SchemaVersion);
        if (pinnedVersion is null)
        {
            return null;
        }

        var workflow = WorkflowParser.Parse(pinnedVersion.WorkflowJson);
        var state = workflow.States.SingleOrDefault(candidate =>
            string.Equals(candidate.Key, activity.CurrentState, StringComparison.Ordinal));

        if (state is null || !state.Terminal || !DeclaresCredit(pinnedVersion.CreditRulesJson))
        {
            return null;
        }

        return new ActivityType
        {
            CreditRulesJson = pinnedVersion.CreditRulesJson,
            SchemaJson = pinnedVersion.SchemaJson
        };
    }

    /// <summary>
    /// Of the given versions, the ones <see cref="PinnedCreditingType" /> could accept, and every state their workflows end
    /// in: a filter a query can apply, so a replay that wants only crediting completions need not load the rest (T196
    /// review). The versions' rules and workflows are jsonb a query cannot read, and the versions table is small, so it
    /// is worked out here in memory.
    /// </summary>
    /// <remarks>
    /// A superset, not the answer: a state terminal in one crediting version need not be terminal in another. The caller
    /// still asks <see cref="PinnedCreditingType" /> of every activity the filter lets through.
    /// </remarks>
    public static CreditingVersionFilter CreditingVersions(IEnumerable<(int Id, string WorkflowJson, string CreditRulesJson)> versions)
    {
        ArgumentNullException.ThrowIfNull(versions);

        var versionIds = new List<int>();
        var terminalStates = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (id, workflowJson, creditRulesJson) in versions)
        {
            if (!DeclaresCredit(creditRulesJson))
            {
                continue;
            }

            versionIds.Add(id);
            terminalStates.UnionWith(WorkflowParser.Parse(workflowJson).States
                .Where(state => state.Terminal)
                .Select(state => state.Key));
        }

        return new CreditingVersionFilter(versionIds, terminalStates.Order(StringComparer.Ordinal).ToList());
    }

    /// <summary>
    /// The replay order: filing order, not encounter order. The tallies are order-independent, but
    /// <c>LastActivityId</c> is not: it means "the completion that most recently moved this row", and the live path
    /// writes it in the order transitions happened.
    /// </summary>
    public static IReadOnlyList<Activity> InFilingOrder(IEnumerable<Activity> activities)
        => activities
            .OrderBy(activity => activity.Transitions.Count == 0
                ? activity.CreatedOn
                : activity.Transitions.Max(transition => transition.OccurredOn))
            .ThenBy(activity => activity.Id)
            .ToList();

    /// <summary>
    /// The transition credit is recorded against: the newest, selected by the expression <c>CreditApplier</c> builds its
    /// dedupe key from, so the stamp and the credit describe the same move.
    /// </summary>
    public static ActivityTransition? CreditedTransition(Activity activity)
        => activity.Transitions
            .OrderByDescending(transition => transition.OccurredOn)
            .FirstOrDefault();

    private static bool DeclaresCredit(string creditRulesJson)
        => !string.IsNullOrWhiteSpace(creditRulesJson) &&
           CreditRulesParser.Parse(creditRulesJson).CountsFor.Count > 0;
}

/// <summary>
/// What <see cref="CreditReplay.CreditingVersions" /> found: the ids of the versions whose rules declare credit, and the
/// states any of them ends in.
/// </summary>
internal sealed record CreditingVersionFilter(IReadOnlyList<int> VersionIds, IReadOnlyList<string> TerminalStates);

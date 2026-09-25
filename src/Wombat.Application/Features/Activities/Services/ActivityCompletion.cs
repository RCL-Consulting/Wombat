using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Whether an activity is finished: its state is a terminal state of its PINNED workflow, the point where credit fires
/// (<c>ActivityService.CreditSubjectIfTerminal</c>, D44). The one answer the committee sampling report, the
/// entrustment trajectory (<see cref="RatedEvidenceProfile" />, T135), the portfolio export's summary (T169) and the
/// assessor and trainee dashboards (T203) share.
/// </summary>
/// <remarks>
/// <para>
/// Not the literal <c>completed</c>. That is where the assessor-rated seeds finish, but <c>reflective_exercise_cpsa</c>
/// finishes in <c>discussed</c>, <c>clinical_audit_cpsa</c> and <c>portfolio_review_cpsa</c> in <c>signed_off</c>,
/// <c>msf_cpsa</c> in <c>recorded</c>, <c>procedure_log</c> and <c>journal_club</c> in
/// <c>logged</c>, <c>qi_project</c> in <c>reviewed</c>, <c>reflective_note</c> in <c>approved</c>,
/// <c>research_output</c> in <c>verified</c> and <c>teaching_session</c> in <c>accepted</c>. The portfolio summary
/// counted only the literal, so every one of those printed as never completed (T169).
/// </para>
/// <para>
/// Not "any state with no way out" either. Every current seed makes <c>declined</c> and <c>cancelled</c> non-terminal
/// dead ends on purpose, so a refused or withdrawn request is not finished work. A version that marks them terminal
/// finishes there, exactly as it credits there.
/// </para>
/// <para>
/// A type with no workflow, or whose stored workflow no longer parses, is taken to finish in
/// <see cref="NoWorkflowFinishedState" />. States are keys, compared exactly.
/// </para>
/// </remarks>
public static class ActivityCompletion
{
    /// <summary>The state a type with no workflow is taken to finish in.</summary>
    public const string NoWorkflowFinishedState = "completed";

    /// <summary>The states in which an activity pinned to <paramref name="workflow" /> is finished.</summary>
    public static IReadOnlySet<string> FinishedStates(Workflow? workflow)
        => workflow is null
            ? new HashSet<string>(StringComparer.Ordinal) { NoWorkflowFinishedState }
            : workflow.States
                .Where(state => state.Terminal)
                .Select(state => state.Key)
                .ToHashSet(StringComparer.Ordinal);

    /// <summary>The states in which an activity pinned to this stored workflow is finished. Total: never throws.</summary>
    public static IReadOnlySet<string> FinishedStates(string? workflowJson)
        => FinishedStates(TryParseWorkflow(workflowJson));

    /// <summary>
    /// The finished states of every pin asked for, one entry per pin, always: the workflow of the pinned
    /// <see cref="ActivityTypeVersion" /> when that row exists, else the type's own columns (a type whose version rows
    /// were never written), as <see cref="RatedEvidenceProfiles" /> and the portfolio export resolve a pin. A pin whose
    /// type is gone reads as a type with no workflow. Each distinct pin is parsed once.
    /// </summary>
    /// <remarks>
    /// The pin is resolved by <see cref="PinnedWorkflows.LoadAsync" />, the resolution the surfaces that print a state's
    /// label share (T220), so an activity's finished states and the words for its state come from the same workflow.
    /// </remarks>
    public static async Task<IReadOnlyDictionary<(int ActivityTypeId, int Version), IReadOnlySet<string>>> LoadFinishedStatesAsync(
        IApplicationDbContext dbContext,
        IEnumerable<(int ActivityTypeId, int Version)> pins,
        CancellationToken cancellationToken = default)
    {
        var workflows = await PinnedWorkflows.LoadAsync(dbContext, pins, cancellationToken);
        return workflows.ToDictionary(pair => pair.Key, pair => FinishedStates(pair.Value));
    }

    /// <summary>
    /// A stored workflow, parsed, or null when there is none or it no longer parses (<see cref="PinnedWorkflows.TryParse" />).
    /// </summary>
    internal static Workflow? TryParseWorkflow(string? workflowJson) => PinnedWorkflows.TryParse(workflowJson);
}

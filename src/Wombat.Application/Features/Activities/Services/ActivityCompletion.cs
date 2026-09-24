using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Whether an activity is finished: its state is a terminal state of its PINNED workflow, the point where credit fires
/// (<c>ActivityService.PlanCreditIfTerminalAsync</c>, D44). The one answer the committee sampling report, the
/// entrustment trajectory (<see cref="RatedEvidenceProfile" />, T135) and the portfolio export's summary (T169) share.
/// </summary>
/// <remarks>
/// <para>
/// Not the literal <c>completed</c>. That is where the assessor-rated seeds finish, but <c>reflective_exercise_cpsa</c>
/// finishes in <c>discussed</c>, <c>msf_cpsa</c> in <c>recorded</c>, <c>procedure_log</c> and <c>journal_club</c> in
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
    /// A stored workflow, parsed, or null when there is none or it no longer parses. Total: a stored workflow that no
    /// longer parses is a defect, but not one a report or a chart should fail over.
    /// </summary>
    internal static Workflow? TryParseWorkflow(string? workflowJson)
    {
        if (string.IsNullOrWhiteSpace(workflowJson))
        {
            return null;
        }

        try
        {
            return WorkflowParser.Parse(workflowJson);
        }
        catch (Exception)
        {
            // Deliberately broad, as CreditRuleFields is: the parser raises its own exception for the shapes it
            // checks, and System.Text.Json raises InvalidOperationException for a value of the wrong kind.
            return null;
        }
    }
}

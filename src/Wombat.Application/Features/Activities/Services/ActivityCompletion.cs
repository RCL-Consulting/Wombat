using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Whether an activity is finished: its state is a terminal state of its PINNED workflow, the point where credit fires
/// (<c>ActivityService.PlanCreditIfTerminalAsync</c>, D44). The one answer the committee sampling report, the
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
    /// Matched in memory over a set already bounded by the caller's activities, because <c>Activity</c> carries no
    /// foreign key to its version (<c>ActivityConfiguration</c>).
    /// </remarks>
    public static async Task<IReadOnlyDictionary<(int ActivityTypeId, int Version), IReadOnlySet<string>>> LoadFinishedStatesAsync(
        IApplicationDbContext dbContext,
        IEnumerable<(int ActivityTypeId, int Version)> pins,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(pins);

        var wanted = pins.Distinct().ToArray();
        var finished = new Dictionary<(int ActivityTypeId, int Version), IReadOnlySet<string>>(wanted.Length);
        if (wanted.Length == 0)
        {
            return finished;
        }

        var typeIds = wanted.Select(pin => pin.ActivityTypeId).Distinct().ToArray();
        var versionNumbers = wanted.Select(pin => pin.Version).Distinct().ToArray();

        var typeWorkflows = await dbContext.Set<ActivityType>()
            .AsNoTracking()
            .Where(type => typeIds.Contains(type.Id))
            .Select(type => new { type.Id, type.WorkflowJson })
            .ToDictionaryAsync(type => type.Id, type => type.WorkflowJson, cancellationToken);

        var versions = await dbContext.Set<ActivityTypeVersion>()
            .AsNoTracking()
            .Where(version => typeIds.Contains(version.ActivityTypeId) && versionNumbers.Contains(version.Version))
            .Select(version => new { version.ActivityTypeId, version.Version, version.WorkflowJson })
            .ToListAsync(cancellationToken);

        var versionWorkflowByPin = versions
            .GroupBy(version => (version.ActivityTypeId, version.Version))
            .ToDictionary(group => group.Key, group => group.First().WorkflowJson);

        foreach (var pin in wanted)
        {
            finished[pin] = versionWorkflowByPin.TryGetValue(pin, out var versionWorkflow)
                ? FinishedStates(versionWorkflow)
                : FinishedStates(typeWorkflows.GetValueOrDefault(pin.ActivityTypeId));
        }

        return finished;
    }

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

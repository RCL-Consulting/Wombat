using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// The workflow each activity is pinned to, and the words it gives the activity's state and recorded moves: the one
/// resolution of a pin that the finished-state readers (<see cref="ActivityCompletion" />) and every surface that prints
/// a state or a move share (T220).
/// </summary>
/// <remarks>
/// <para>
/// A pin is the (type, version) an activity was filed against. It resolves to the pinned <see cref="ActivityTypeVersion" />'s
/// workflow when that row exists, else to the type's own columns (a type whose version rows were never written), as
/// <see cref="RatedEvidenceProfiles" /> and the portfolio export resolve it. A pin whose type is gone, and a stored
/// workflow that no longer parses, resolve to null: total, because a list or a report should not fail over a defect it
/// can still show.
/// </para>
/// <para>
/// A state is shown by its label in the pinned version (<see cref="Workflow.StateLabel" />) and a recorded move by the
/// name its button had (<see cref="Workflow.TransitionLabel" />). The stored key is shown only where the pinned version
/// cannot name it: a state or move it does not declare, or no workflow at all. So a page, the refusal beside it
/// (T189) and a notice on it name a state alike: a submitted clinical audit is "Awaiting supervisor" everywhere.
/// </para>
/// </remarks>
public static class PinnedWorkflows
{
    /// <summary>
    /// The workflow of every pin asked for, one entry per pin, always: null for a pin whose type is gone or whose stored
    /// workflow is missing or no longer parses. Each distinct pin is parsed once.
    /// </summary>
    /// <remarks>
    /// Matched in memory over a set already bounded by the caller's activities, because <c>Activity</c> carries no
    /// foreign key to its version (<c>ActivityConfiguration</c>).
    /// </remarks>
    public static async Task<IReadOnlyDictionary<(int ActivityTypeId, int Version), Workflow?>> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<(int ActivityTypeId, int Version)> pins,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(pins);

        var wanted = pins.Distinct().ToArray();
        var workflows = new Dictionary<(int ActivityTypeId, int Version), Workflow?>(wanted.Length);
        if (wanted.Length == 0)
        {
            return workflows;
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
            workflows[pin] = TryParse(versionWorkflowByPin.TryGetValue(pin, out var versionWorkflow)
                ? versionWorkflow
                : typeWorkflows.GetValueOrDefault(pin.ActivityTypeId));
        }

        return workflows;
    }

    /// <summary>
    /// A stored workflow, parsed, or null when there is none or it no longer parses. Total: a stored workflow that no
    /// longer parses is a defect, but not one a report or a chart should fail over.
    /// </summary>
    public static Workflow? TryParse(string? workflowJson)
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

    /// <summary>
    /// A state as a person reads it: its label in <paramref name="workflow" />, or the stored key when the workflow does
    /// not declare it or there is none.
    /// </summary>
    public static string StateLabel(Workflow? workflow, string stateKey)
        => workflow is null ? stateKey : workflow.StateLabel(stateKey);

    /// <summary>
    /// A recorded move as a person reads it: the name its button had in <paramref name="workflow" />, or the stored key
    /// when the workflow does not declare it or there is none.
    /// </summary>
    public static string TransitionLabel(Workflow? workflow, string transitionKey)
        => workflow is null ? transitionKey : workflow.TransitionLabel(transitionKey);
}

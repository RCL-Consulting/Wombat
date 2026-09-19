using System.Text.Json;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Activities;

public sealed class Activity
{
    public int Id { get; set; }
    public int ActivityTypeId { get; set; }
    public int SchemaVersion { get; set; }
    public string SubjectUserId { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public int? EpaId { get; set; }
    public int? CurriculumItemId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }

    /// <summary>
    /// Where this activity sits organisationally, stamped at creation from the subject's
    /// <see cref="Wombat.Domain.Identity.TraineeProfile" />. (T101)
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are the activity's OWN scope, not its <see cref="ActivityType" />'s. The type's
    /// <c>Scope</c>/<c>ScopeId</c> say which programme may *offer* the tool; these say which
    /// programme the assessment is *about*. They diverge in practice — a trainee in one speciality
    /// filing a tool published by another — and before T101 the actor grammar's <c>scope:</c> rules
    /// resolved against the type, so a SpecialityAdmin overseeing the trainee did not match while an
    /// admin of the tool's speciality did. Oversight follows the trainee, so it resolves from here.
    /// </para>
    /// <para>
    /// Stamped rather than derived so that read authorization is a column comparison rather than a
    /// three-table join, and so that a trainee transferring institutions does not retroactively move
    /// the visibility of assessments written about them elsewhere.
    /// </para>
    /// <para>
    /// Null when the subject had no trainee profile at creation — a self-logged activity by a user
    /// who is not an admitted trainee. Null never matches a <c>scope:</c> rule and never satisfies a
    /// scoped read, so an unstamped activity is readable only by the people named on it. That is the
    /// safe direction: it withholds oversight rather than granting it.
    /// </para>
    /// </remarks>
    public int? InstitutionId { get; set; }

    /// <inheritdoc cref="InstitutionId" />
    public int? SpecialityId { get; set; }

    /// <inheritdoc cref="InstitutionId" />
    public int? SubSpecialityId { get; set; }

    public ActivityType ActivityType { get; set; } = null!;
    public ICollection<ActivityTransition> Transitions { get; set; } = [];

    /// <summary>
    /// Applies a declared transition and records it. Returns the recorded
    /// <see cref="ActivityTransition" /> so the caller can stamp the outcome of anything that runs
    /// as a consequence of the move — credit, for one (T108) — onto the very row it belongs to,
    /// rather than fishing the newest one back out of the collection.
    /// </summary>
    public ActivityTransition ApplyTransition(Workflow.Workflow workflow, string transitionKey, string actorUserId, string newDataJson, string? note)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentException.ThrowIfNullOrWhiteSpace(transitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(newDataJson);

        using var snapshotDocument = JsonDocument.Parse(newDataJson);
        var normalizedDataJson = JsonUtilities.Normalize(snapshotDocument.RootElement);
        var transition = workflow.Transitions.SingleOrDefault(candidate =>
            string.Equals(candidate.Key, transitionKey, StringComparison.Ordinal) &&
            candidate.From.Contains(CurrentState, StringComparer.Ordinal));

        if (transition is null)
        {
            throw new InvalidOperationException(
                $"Transition '{transitionKey}' is not declared for state '{CurrentState}'.");
        }

        var previousState = CurrentState;
        CurrentState = transition.To;
        DataJson = normalizedDataJson;
        UpdatedOn = DateTime.UtcNow;

        var record = new ActivityTransition
        {
            FromState = previousState,
            ToState = transition.To,
            TransitionKey = transitionKey,
            ActorUserId = actorUserId.Trim(),
            OccurredOn = UpdatedOn,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            SnapshotJson = normalizedDataJson
        };

        Transitions.Add(record);
        return record;
    }
}

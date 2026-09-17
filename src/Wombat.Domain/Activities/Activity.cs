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

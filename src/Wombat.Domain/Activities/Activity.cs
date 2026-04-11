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

    public void ApplyTransition(string transitionKey, string actorUserId, string newDataJson, string? note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transitionKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(newDataJson);

        if (ActivityType is null)
        {
            throw new InvalidOperationException("ActivityType must be loaded before applying a transition.");
        }

        using var snapshotDocument = JsonDocument.Parse(newDataJson);
        var normalizedDataJson = JsonUtilities.Normalize(snapshotDocument.RootElement);
        var workflow = WorkflowParser.Parse(ActivityType.WorkflowJson);
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

        Transitions.Add(new ActivityTransition
        {
            FromState = previousState,
            ToState = transition.To,
            TransitionKey = transitionKey,
            ActorUserId = actorUserId.Trim(),
            OccurredOn = UpdatedOn,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            SnapshotJson = normalizedDataJson
        });
    }
}

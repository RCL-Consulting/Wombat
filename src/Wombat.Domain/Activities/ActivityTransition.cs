namespace Wombat.Domain.Activities;

public sealed class ActivityTransition
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string TransitionKey { get; set; } = string.Empty;
    public string ActorUserId { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public string? Note { get; set; }
    public string SnapshotJson { get; set; } = "{}";

    public Activity Activity { get; set; } = null!;
}

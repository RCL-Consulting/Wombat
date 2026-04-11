namespace Wombat.Domain.Activities.Workflow;

public sealed record WorkflowTransition(
    string Key,
    IReadOnlyList<string> From,
    string To,
    ActorRule Actor,
    bool RequiresNote,
    IReadOnlyList<string> RequiresFields);

namespace Wombat.Domain.Activities.Workflow;

public sealed record Workflow(
    int Version,
    string InitialState,
    IReadOnlyList<WorkflowState> States,
    IReadOnlyList<WorkflowTransition> Transitions);

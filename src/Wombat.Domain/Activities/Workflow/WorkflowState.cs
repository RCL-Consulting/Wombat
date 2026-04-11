namespace Wombat.Domain.Activities.Workflow;

public sealed record WorkflowState(
    string Key,
    string Label,
    bool Terminal);

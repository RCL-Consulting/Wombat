namespace Wombat.Domain.Activities.Workflow;

public sealed class WorkflowParseException : Exception
{
    public WorkflowParseException(string message)
        : base(message)
    {
    }
}

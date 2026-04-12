using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

public sealed class ActivityApplyTransitionTests
{
    [Fact]
    public void ApplyTransition_DeclaredTransition_UpdatesStateAndRecordsEvent()
    {
        var activity = new Activity
        {
            CurrentState = "draft",
            DataJson = "{}"
        };

        activity.ApplyTransition(
            WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson),
            "submit",
            "user-1",
            """{ "title": "Case" }""",
            "Sending for review");

        Assert.Equal("submitted", activity.CurrentState);
        Assert.Equal("""{"title":"Case"}""", activity.DataJson);
        Assert.Single(activity.Transitions);
        Assert.Equal("draft", activity.Transitions.Single().FromState);
        Assert.Equal("submitted", activity.Transitions.Single().ToState);
        Assert.Equal("user-1", activity.Transitions.Single().ActorUserId);
        Assert.Equal("Sending for review", activity.Transitions.Single().Note);
        Assert.Equal(activity.DataJson, activity.Transitions.Single().SnapshotJson);
    }

    [Fact]
    public void ApplyTransition_UndeclaredTransition_Throws()
    {
        var activity = new Activity
        {
            CurrentState = "draft",
            DataJson = "{}"
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            activity.ApplyTransition(
                WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson),
                "complete",
                "user-1",
                "{}",
                null));

        Assert.Contains("not declared", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}

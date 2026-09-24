using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T122. <see cref="Workflow.CanReachTerminal" /> is how the EPA→tool gate tells a move toward credit from a move away
/// from it.
/// </summary>
/// <remarks>
/// The first cut of the gate exempted only true dead ends. T122's adversarial review found the gap: a builder-made
/// <c>recall</c> back into the draft phase, or a <c>cancel</c> into a state that reopens only into the draft, was gated
/// like a submission. A trainee could not pull back a request whose EPA had since been closed, while the assessor's
/// unchanged completion still credited it. Reachability avoiding the state the move LEFT is one half of the test that
/// separates a withdrawal from a handover; the other half is whether the mover loses write access to the EPA.
/// </remarks>
public sealed class WorkflowReachabilityTests
{
    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "withdrawn", "label": "Withdrawn" },
            { "key": "on_hold", "label": "On hold" },
            { "key": "limbo_a", "label": "Limbo A" },
            { "key": "limbo_b", "label": "Limbo B" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject" },
            { "key": "recall", "from": "requested", "to": "draft", "actor": "subject" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "role:Assessor" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "role:Assessor" },
            { "key": "withdraw", "from": "requested", "to": "withdrawn", "actor": "subject" },
            { "key": "reopen", "from": "withdrawn", "to": "draft", "actor": "subject" },
            { "key": "hold", "from": "requested", "to": "on_hold", "actor": "role:Assessor" },
            { "key": "resume", "from": "on_hold", "to": "requested", "actor": "role:Assessor" },
            { "key": "stray", "from": "requested", "to": "limbo_a", "actor": "role:Assessor" },
            { "key": "loop", "from": "limbo_a", "to": "limbo_b", "actor": "role:Assessor" },
            { "key": "loop_back", "from": "limbo_b", "to": "limbo_a", "actor": "role:Assessor" }
          ]
        }
        """;

    [Theory]
    [InlineData("draft", true)]
    [InlineData("requested", true)]
    [InlineData("completed", true)]
    [InlineData("declined", false)]
    [InlineData("withdrawn", true)]
    [InlineData("on_hold", true)]
    [InlineData("limbo_a", false)]
    [InlineData("limbo_b", false)]
    [InlineData("nowhere", false)]
    public void WhetherCreditCanStillBeReachedAtAll(string state, bool expected)
    {
        Assert.Equal(expected, WorkflowParser.Parse(WorkflowJson).CanReachTerminal(state));
    }

    /// <summary>
    /// Avoiding a state models a move out of it: here a move out of <c>draft</c>. <c>withdrawn</c> reaches credit only
    /// back through <c>draft</c>. <c>on_hold</c> reaches it through <c>requested</c>. <c>draft</c> itself never counts,
    /// because it IS the avoided state.
    /// </summary>
    [Theory]
    [InlineData("draft", false)]
    [InlineData("requested", true)]
    [InlineData("completed", true)]
    [InlineData("withdrawn", false)]
    [InlineData("on_hold", true)]
    [InlineData("limbo_a", false)]
    public void WhetherCreditCanBeReachedWithoutPassingBackThroughTheDraftPhase(string state, bool expected)
    {
        Assert.Equal(expected, WorkflowParser.Parse(WorkflowJson).CanReachTerminal(state, avoidingState: "draft"));
    }

    /// <summary>A move out of <c>requested</c> into <c>withdrawn</c> or <c>on_hold</c> can only come back through it.</summary>
    [Theory]
    [InlineData("withdrawn", false)]
    [InlineData("on_hold", false)]
    [InlineData("completed", true)]
    public void WhetherCreditCanBeReachedWithoutPassingBackThroughRequested(string state, bool expected)
    {
        Assert.Equal(expected, WorkflowParser.Parse(WorkflowJson).CanReachTerminal(state, avoidingState: "requested"));
    }

    /// <summary>
    /// T148: the moves out of a state that lead on. Out of <c>requested</c> only <c>complete</c> does: <c>decline</c> and
    /// <c>stray</c> end nowhere, and <c>recall</c>, <c>withdraw</c> and <c>hold</c> reach credit only back through
    /// <c>requested</c>. A move straight into a terminal state counts; a withdrawal that can be reopened does not.
    /// </summary>
    [Theory]
    [InlineData("draft", new[] { "submit" })]
    [InlineData("requested", new[] { "complete" })]
    [InlineData("withdrawn", new[] { "reopen" })]
    [InlineData("declined", new string[0])]
    [InlineData("nowhere", new string[0])]
    public void TheMovesOutOfAStateThatLeadOn_InDeclarationOrder(string state, string[] expected)
    {
        var moves = WorkflowParser.Parse(WorkflowJson).TransitionsLeadingOn(state).Select(transition => transition.Key);

        Assert.Equal(expected, moves);
    }

    /// <summary>A self-transition leads nowhere new, and a move declared first is listed first.</summary>
    [Fact]
    public void ASelfTransitionDoesNotLeadOn_AndTheOrderIsTheWorkflows()
    {
        var workflow = WorkflowParser.Parse("""
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "done", "label": "Done", "terminal": true },
                { "key": "review", "label": "Review" }
              ],
              "transitions": [
                { "key": "save", "from": "draft", "to": "draft", "actor": "subject" },
                { "key": "finish", "from": "draft", "to": "done", "actor": "subject" },
                { "key": "send", "from": "draft", "to": "review", "actor": "subject" },
                { "key": "approve", "from": "review", "to": "done", "actor": "role:Assessor" }
              ]
            }
            """);

        Assert.Equal(["finish", "send"], workflow.TransitionsLeadingOn("draft").Select(transition => transition.Key));
    }

    /// <summary>A cycle with no exit terminates and reports what it is: a dead end in two states rather than one.</summary>
    [Fact]
    public void ACycleWithNoWayOutIsADeadEnd_AndTheSearchTerminates()
    {
        var workflow = WorkflowParser.Parse(WorkflowJson);

        Assert.True(workflow.HasOutgoingTransition("limbo_a"));
        Assert.False(workflow.CanReachTerminal("limbo_a"));
    }
}

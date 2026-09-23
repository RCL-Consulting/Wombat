using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T122. <see cref="Workflow.HasOutgoingTransition" /> is the one implementation of "dead end" for field-write
/// authorization.
/// </summary>
/// <remarks>
/// <para>
/// Field-write authorization treats a non-terminal state with no way out as final, so nobody can keep editing a
/// declined request. (The EPA→tool gate uses the stricter reachability test, <see cref="Workflow.CanReachTerminal" />,
/// pinned in <c>WorkflowReachabilityTests</c>.)
/// </para>
/// <para>
/// The CPSA workflows are the case that matters. They declare <c>declined</c> and <c>cancelled</c> NON-terminal on
/// purpose, because a terminal state fires credit, and credit on a refused or withdrawn request would be wrong. So
/// "terminal" cannot stand in for "dead end": if this method ever reported a CPSA dead end as live, a trainee would be
/// handed an editable form on a dead activity.
/// </para>
/// </remarks>
public sealed class WorkflowDeadEndTests
{
    /// <summary>The shape of <c>mini_cex_cpsa</c>, <c>dops_cpsa</c>, <c>cbd_cpsa</c> and <c>direct_observation_cpsa</c>.</summary>
    private const string CpsaWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    [Theory]
    [InlineData("draft", true)]
    [InlineData("requested", true)]
    [InlineData("completed", false)]
    [InlineData("declined", false)]
    [InlineData("cancelled", false)]
    public void EachCpsaStateReportsWhetherAnyTransitionLeavesIt(string state, bool expected)
    {
        var workflow = WorkflowParser.Parse(CpsaWorkflowJson);

        Assert.Equal(expected, workflow.HasOutgoingTransition(state));
    }

    /// <summary>
    /// The dead ends are exactly the two refusal states: non-terminal and with no way out. <c>completed</c> has no way
    /// out either but is terminal, which is where credit fires, so it is not a dead end.
    /// </summary>
    [Fact]
    public void TheCpsaDeadEndsAreDeclinedAndCancelled()
    {
        var workflow = WorkflowParser.Parse(CpsaWorkflowJson);

        var deadEnds = workflow.States
            .Where(state => !state.Terminal && !workflow.HasOutgoingTransition(state.Key))
            .Select(state => state.Key)
            .ToArray();

        Assert.Equal(new[] { "declined", "cancelled" }, deadEnds);
    }

    /// <summary>
    /// A transition with an array <c>from</c> leaves every state in the array, not just the first. The CPSA
    /// <c>cancel</c> is written that way, and <c>serialize</c> collapses a one-element array to a string, so both
    /// spellings must count. Here <c>requested</c> is left ONLY through the second element of an array.
    /// </summary>
    [Fact]
    public void AStateLeftOnlyThroughTheSecondElementOfAnArrayFromIsNotADeadEnd()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "requested", "label": "Requested" },
                { "key": "cancelled", "label": "Cancelled", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "requested", "actor": "subject" },
                { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject" }
              ]
            }
            """;

        var workflow = WorkflowParser.Parse(json);

        Assert.True(workflow.HasOutgoingTransition("requested"));
        Assert.True(workflow.HasOutgoingTransition("draft"));
        Assert.False(workflow.HasOutgoingTransition("cancelled"));
    }

    /// <summary>
    /// A workflow that lets a trainee resubmit after a decline turns <c>declined</c> into a live state, so field-write
    /// authorization must keep it editable for the author who has to fix and resubmit.
    /// </summary>
    [Fact]
    public void AResubmittableDeclinedStateIsNotADeadEnd()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "requested", "label": "Requested" },
                { "key": "completed", "label": "Completed", "terminal": true },
                { "key": "declined", "label": "Declined" }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "requested", "actor": "subject" },
                { "key": "complete", "from": "requested", "to": "completed", "actor": "role:Assessor" },
                { "key": "decline", "from": "requested", "to": "declined", "actor": "role:Assessor" },
                { "key": "resubmit", "from": "declined", "to": "requested", "actor": "subject" }
              ]
            }
            """;

        var workflow = WorkflowParser.Parse(json);

        Assert.True(workflow.HasOutgoingTransition("declined"));
        Assert.DoesNotContain(
            workflow.States,
            state => !state.Terminal && !workflow.HasOutgoingTransition(state.Key));
    }

    /// <summary>The legacy three-state shape has no dead ends: every non-terminal state leads on.</summary>
    [Fact]
    public void TheLegacyLinearWorkflowHasNoDeadEnds()
    {
        var workflow = WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson);

        Assert.True(workflow.HasOutgoingTransition("draft"));
        Assert.True(workflow.HasOutgoingTransition("submitted"));
        Assert.False(workflow.HasOutgoingTransition("completed"));
        Assert.DoesNotContain(
            workflow.States,
            state => !state.Terminal && !workflow.HasOutgoingTransition(state.Key));
    }

    /// <summary>
    /// State keys are ordinal, as the parser declares them. A differently cased or unknown key names no state, so no
    /// transition leaves it.
    /// </summary>
    [Theory]
    [InlineData("Draft")]
    [InlineData("REQUESTED")]
    [InlineData("")]
    [InlineData("nowhere")]
    public void AKeyThatNamesNoDeclaredStateHasNoOutgoingTransition(string state)
    {
        var workflow = WorkflowParser.Parse(CpsaWorkflowJson);

        Assert.False(workflow.HasOutgoingTransition(state));
    }

    /// <summary>
    /// The answer survives a canonical round trip, which is what <c>SaveDraft</c> stores and what the runtime reads
    /// back. The serializer rewrites a one-element <c>from</c> array as a string, so this also covers that spelling.
    /// </summary>
    [Fact]
    public void TheAnswerIsUnchangedByASerializeParseRoundTrip()
    {
        var original = WorkflowParser.Parse(CpsaWorkflowJson);
        var roundTripped = WorkflowParser.Parse(WorkflowParser.Serialize(original));

        foreach (var state in original.States)
        {
            Assert.Equal(original.HasOutgoingTransition(state.Key), roundTripped.HasOutgoingTransition(state.Key));
        }
    }
}

using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T189: <see cref="Workflow.StateLabel" /> and <see cref="WorkflowTransition.LabelFor" /> are where a workflow's keys
/// become the words a person sees: a refusal names a move as its button does, and a state by its declared label.
/// </summary>
public sealed class WorkflowLabelTests
{
    private static readonly Workflow ClinicalAuditShape = WorkflowParser.Parse("""
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Awaiting supervisor" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "sign_off", "from": "submitted", "to": "signed_off", "actor": "role:Assessor" }
          ]
        }
        """);

    [Theory]
    [InlineData("draft", "Draft")]
    [InlineData("submitted", "Awaiting supervisor")]
    [InlineData("signed_off", "Signed off")]
    public void ADeclaredState_IsNamedByItsLabel_NotByItsKey(string stateKey, string label)
        => Assert.Equal(label, ClinicalAuditShape.StateLabel(stateKey));

    [Fact]
    public void AStateTheWorkflowDoesNotDeclare_IsNamedByItsKey()
        // An activity whose stored state its pinned version does not declare still gets a refusal that says something.
        => Assert.Equal("archived", ClinicalAuditShape.StateLabel("archived"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AStateWithABlankLabel_IsNamedByItsKey(string blank)
    {
        // The parser refuses a blank label; a workflow built in code does not pass through it.
        var workflow = new Workflow(1, "draft", [new WorkflowState("draft", blank, false, null)], []);

        Assert.Equal("draft", workflow.StateLabel("draft"));
    }

    [Fact]
    public void AStateLabel_IsTrimmed()
    {
        var workflow = new Workflow(1, "draft", [new WorkflowState("draft", "  Draft ", false, null)], []);

        Assert.Equal("Draft", workflow.StateLabel("draft"));
    }

    [Theory]
    [InlineData("submit", "Submit")]
    [InlineData("sign_off", "Sign Off")]
    [InlineData("request-changes", "Request Changes")]
    [InlineData("record_discussion", "Record Discussion")]
    public void AMove_IsNamedFromItsKey_InWords_InTitleCase(string transitionKey, string label)
        => Assert.Equal(label, WorkflowTransition.LabelFor(transitionKey));

    [Theory]
    [InlineData("_")]
    [InlineData("-_-")]
    public void AMoveWhoseKeyIsOnlySeparators_IsNamedByItsKey(string transitionKey)
        => Assert.Equal(transitionKey, WorkflowTransition.LabelFor(transitionKey));

    [Theory]
    [InlineData("submit", "Submit")]
    [InlineData("sign_off", "Sign Off")]
    public void ARecordedMoveTheWorkflowDeclares_IsNamedAsItsButtonWas(string transitionKey, string label)
    {
        // T220: the history names a move as its button named it and a refusal of it did (T189).
        Assert.Equal(label, ClinicalAuditShape.TransitionLabel(transitionKey));
        Assert.Equal(WorkflowTransition.LabelFor(transitionKey), ClinicalAuditShape.TransitionLabel(transitionKey));
    }

    [Fact]
    public void TheCreateRow_IsNamedInWords_ThoughNoWorkflowDeclaresIt()
        // ActivityService writes it on every create, from the initial state to itself.
        => Assert.Equal("Create", ClinicalAuditShape.TransitionLabel(Workflow.CreateTransitionKey));

    [Fact]
    public void ARecordedMoveTheWorkflowDoesNotDeclare_IsNamedByItsKey()
        // A row the pinned version cannot account for is shown as it was stored, not dressed up as a move it knows.
        => Assert.Equal("complete_legacy", ClinicalAuditShape.TransitionLabel("complete_legacy"));

    /// <summary>
    /// T342: every move key the shipped seeds declare, as an instruction in running text. The runbook lane found the page
    /// reading "Record discussion it" from the label; <see cref="WorkflowTransition.ImperativeFor" /> is the phrase.
    /// </summary>
    [Theory]
    [InlineData("complete", "complete it")]
    [InlineData("decline", "decline it")]
    [InlineData("return", "return it")]
    [InlineData("accept", "accept it")]
    [InlineData("approve", "approve it")]
    [InlineData("verify", "verify it")]
    [InlineData("reject", "reject it")]
    [InlineData("review", "review it")]
    [InlineData("record", "record it")]
    [InlineData("record_discussion", "record the discussion")]
    [InlineData("sign_off", "sign it off")]
    [InlineData("Sign-Off", "sign it off")]
    public void AMove_ReadsAsAnInstructionAboutTheActivity(string key, string phrase)
        => Assert.Equal(phrase, WorkflowTransition.ImperativeFor(key));

    [Fact]
    public void AKeyOfSeparatorsOnly_HasNoPhrase()
        => Assert.Null(WorkflowTransition.ImperativeFor("__"));
}

using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

public sealed class WorkflowParserTests
{
    [Fact]
    public void Parse_ValidWorkflow_ReturnsWorkflow()
    {
        var workflow = WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson);

        Assert.Equal("draft", workflow.InitialState);
        Assert.Equal(2, workflow.Transitions.Count);
        Assert.IsType<SubjectUserActorRule>(workflow.Transitions[0].Actor);
    }

    [Fact]
    public void Parse_MissingInitialState_Throws()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "missing",
              "states": [
                { "key": "draft", "label": "Draft" }
              ],
              "transitions": []
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        Assert.Contains("Initial state", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_TransitionToUndeclaredState_Throws()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" }
              ]
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        Assert.Contains("undeclared target state", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_UnreachableState_Throws()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "submitted", "label": "Submitted" },
                { "key": "orphan", "label": "Orphan" }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" }
              ]
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        Assert.Contains("unreachable states", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_RoundTripsModuloWhitespace()
    {
        var parsed = WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson);
        var serialized = WorkflowParser.Serialize(parsed);

        Assert.Equal(
            ActivityTestData.NormalizeJson(ActivityTestData.ValidWorkflowJson),
            ActivityTestData.NormalizeJson(serialized));
    }

    [Fact]
    public void Parse_EditableBy_OnState_ReturnsActorRule()
    {
        var workflow = WorkflowParser.Parse(EditableByWorkflowJson);

        Assert.Null(workflow.States[0].EditableBy);

        var stateRule = Assert.IsType<FieldUserActorRule>(workflow.States[1].EditableBy);
        Assert.Equal("assessor_user_id", stateRule.Field);

        Assert.Null(workflow.States[2].EditableBy);
    }

    [Fact]
    public void Serialize_EditableBy_RoundTripsModuloWhitespace()
    {
        var parsed = WorkflowParser.Parse(EditableByWorkflowJson);
        var serialized = WorkflowParser.Serialize(parsed);

        Assert.Equal(
            ActivityTestData.NormalizeJson(EditableByWorkflowJson),
            ActivityTestData.NormalizeJson(serialized));
    }

    [Fact]
    public void Serialize_WithoutEditableBy_EmitsNoProperty()
    {
        var parsed = WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson);
        var serialized = WorkflowParser.Serialize(parsed);

        Assert.DoesNotContain("editable_by", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_UnknownEditableByToken_Throws()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft", "editable_by": "assessor" }
              ],
              "transitions": []
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        Assert.Contains("Actor rule grammar", exception.Message, StringComparison.Ordinal);
    }

    private const string EditableByWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "request", "from": "draft", "to": "requested", "actor": "subject" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;
}

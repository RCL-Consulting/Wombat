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
}

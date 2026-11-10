using Wombat.Domain.Activities;
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

    /// <summary>
    /// T398 (Q13, S11): laid out for the builder's page, one property a line, two spaces, a line feed, in the compact
    /// form's key order; it parses to what the compact text parses to, and the one-argument overload stays compact.
    /// </summary>
    [Fact]
    public void Serialize_Indented_IsOnePropertyALine_InTheCompactOrder_AndParsesAlike()
    {
        var parsed = WorkflowParser.Parse(ActivityTestData.ValidWorkflowJson);
        var compact = WorkflowParser.Serialize(parsed);
        var indented = WorkflowParser.Serialize(parsed, indented: true);

        Assert.DoesNotContain('\n', compact);
        Assert.DoesNotContain('\r', indented);
        Assert.StartsWith("{\n  \"version\": 1,\n  \"initial_state\": \"draft\",\n  \"states\": [\n    {\n      \"key\": ", indented);
        Assert.All(
            indented.Split('\n').Where(line => line.Contains("\": ", StringComparison.Ordinal)),
            line => Assert.Equal(2, line.Split("\": ").Length));
        Assert.Equal(compact, WorkflowParser.Serialize(WorkflowParser.Parse(indented)));
        Assert.Equal(ActivityTestData.NormalizeJson(compact), ActivityTestData.NormalizeJson(indented));
    }

    /// <summary>T398 (Q6): a refusal of one move is at that move, its sentence the message.</summary>
    [Fact]
    public void Parse_ARefusalOfAMove_IsAtThatMove()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "done", "actor": "subject" },
                { "key": "cancel", "from": "draft", "to": "done", "actor": "nobody" }
              ]
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalTab.Workflow, RefusalKind.Other, "cancel"), (place.Tab, place.Kind, place.MoveKey));
        Assert.Equal(exception.Message, place.Sentence);
    }

    [Fact]
    public void Parse_AnUndeclaredState_IsAtTheMoveNamingIt()
    {
        const string json = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [ { "key": "draft", "label": "Draft" } ],
              "transitions": [ { "key": "submit", "from": "draft", "to": "nowhere", "actor": "subject" } ]
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        Assert.Equal("submit", Assert.Single(exception.Places).MoveKey);
    }

    /// <summary>T398 (Q6, S11): a text that is not JSON is at the line the reader stopped on, counted from 1.</summary>
    [Fact]
    public void Parse_MalformedJson_IsAtItsLine()
    {
        var exception = Assert.Throws<WorkflowParseException>(
            () => WorkflowParser.Parse("{\n  \"version\": 1,\n  \"states\": [ ,\n}"));

        var place = Assert.Single(exception.Places);
        Assert.Equal((RefusalTab.Workflow, RefusalKind.MalformedJson, 3L), (place.Tab, place.Kind, place.JsonLine));
        Assert.StartsWith("Workflow JSON is malformed: ", place.Sentence, StringComparison.Ordinal);
    }

    /// <summary>A refusal its thrower placed nowhere is the Workflow tab's alone, its sentence the message (T398, Q6).</summary>
    [Fact]
    public void Parse_ARefusalWithNoPlace_IsTheTabsAlone()
    {
        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse("[]"));

        var place = Assert.Single(exception.Places);
        Assert.Equal(
            (RefusalTab.Workflow, RefusalKind.Other, "Workflow root must be an object."),
            (place.Tab, place.Kind, place.Sentence));
        Assert.Null(place.MoveKey);
    }

    [Fact]
    public void Parse_Validation_ReadsEachValue_AndDefaultsToAll()
    {
        var workflow = WorkflowParser.Parse(ValidationWorkflowJson);

        Assert.Equal(
            [
                ("submit", TransitionValidation.Owned),
                ("complete", TransitionValidation.All),
                ("cancel", TransitionValidation.Draft),
                ("reopen", TransitionValidation.All)
            ],
            workflow.Transitions.Select(transition => (transition.Key, transition.Validation)).ToArray());
    }

    [Fact]
    public void Serialize_Validation_IsAlwaysWritten_TheDefaultIncluded()
    {
        // A stored workflow says how each move is checked, so nobody has to know the default to read it (T105).
        var serialized = WorkflowParser.Serialize(WorkflowParser.Parse(ValidationWorkflowJson));

        using var document = System.Text.Json.JsonDocument.Parse(serialized);
        Assert.Equal(
            new[] { "owned", "all", "draft", "all" },
            document.RootElement.GetProperty("transitions").EnumerateArray()
                .Select(transition => transition.GetProperty("validation").GetString() ?? string.Empty)
                .ToArray());
    }

    [Theory]
    [InlineData("\"none\"")]
    [InlineData("\"Draft\"")]
    [InlineData("\"submit\"")]
    [InlineData("true")]
    [InlineData("1")]
    public void Parse_AValidationThatIsNotOneOfTheThree_Throws(string value)
    {
        var json = $$"""
            {
              "version": 1,
              "initial_state": "draft",
              "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
              "transitions": [ { "key": "finish", "from": "draft", "to": "done", "actor": "subject", "validation": {{value}} } ]
            }
            """;

        var exception = Assert.Throws<WorkflowParseException>(() => WorkflowParser.Parse(json));

        Assert.Contains("finish", exception.Message, StringComparison.Ordinal);
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

    private const string ValidationWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "role:Assessor", "validation": "all" },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject", "validation": "draft" },
            { "key": "reopen", "from": "cancelled", "to": "draft", "actor": "subject" }
          ]
        }
        """;

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
            { "key": "request", "from": "draft", "to": "requested", "actor": "subject", "validation": "draft" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
          ]
        }
        """;
}

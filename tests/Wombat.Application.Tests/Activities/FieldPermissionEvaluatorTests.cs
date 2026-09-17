using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Activities;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T070. The writable set is the conjunction of the state rule and the field rule (falling back to
/// the section rule). Every declaration is optional and defaults to subject|creator.
/// </summary>
public sealed class FieldPermissionEvaluatorTests
{
    private readonly FieldPermissionEvaluator _evaluator = new();

    /// <summary>
    /// THE BACKWARDS-COMPATIBILITY CONTRACT. Every ActivityTypeVersion already published to the live
    /// database declares no editable_by anywhere. If this test fails, those rows have silently
    /// changed behaviour without a republish.
    /// </summary>
    [Fact]
    public void GetWritableFieldKeys_WithNothingDeclared_GivesSubjectAndCreatorEveryFieldAndOthersNone()
    {
        var schema = FormSchemaParser.Parse(LegacySchemaJson);
        var workflow = WorkflowParser.Parse(LegacyWorkflowJson);
        var activity = CreateActivity("open");

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("trainee-1"))
            .Should().BeEquivalentTo(["title", "epa_id", "score"]);

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("coordinator-1"))
            .Should().BeEquivalentTo(["title", "epa_id", "score"]);

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("stranger-1"))
            .Should().BeEmpty();
    }

    [Fact]
    public void GetWritableFieldKeys_InRequestedState_GivesTheBoundAssessorExactlyTheirOwnFields()
    {
        var schema = FormSchemaParser.Parse(WbaSchemaJson);
        var workflow = WorkflowParser.Parse(WbaWorkflowJson);
        var activity = CreateActivity("requested", AssessorBoundDataJson);

        var writable = _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("assessor-1"));

        writable.Should().BeEquivalentTo(["overall_level", "strengths", "improvements", "plan"]);

        // The request section keeps the default rule. That is what stops an authorised assessor
        // rewriting epa_id or assessor_user_id through a complete patch and redirecting credit.
        writable.Should().NotContain("epa_id");
        writable.Should().NotContain("assessor_user_id");
    }

    [Fact]
    public void GetWritableFieldKeys_GivesTheTraineeTheRequestFieldsInDraftAndNothingOnceRequested()
    {
        var schema = FormSchemaParser.Parse(WbaSchemaJson);
        var workflow = WorkflowParser.Parse(WbaWorkflowJson);

        _evaluator
            .GetWritableFieldKeys(schema, workflow, CreateActivity("draft", AssessorBoundDataJson), CreatePrincipal("trainee-1"))
            .Should().BeEquivalentTo(["epa_id", "assessor_user_id", "case_summary"]);

        // requested is assessor-owned at the state level, so the state gate closes for the trainee
        // even though the request section rule would still admit them.
        _evaluator
            .GetWritableFieldKeys(schema, workflow, CreateActivity("requested", AssessorBoundDataJson), CreatePrincipal("trainee-1"))
            .Should().BeEmpty();
    }

    [Fact]
    public void GetWritableFieldKeys_InATerminalState_IsEmptyForEveryone()
    {
        var schema = FormSchemaParser.Parse(WbaSchemaJson);
        var workflow = WorkflowParser.Parse(WbaWorkflowJson);
        var activity = CreateActivity("completed", AssessorBoundDataJson);

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("assessor-1")).Should().BeEmpty();
        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("trainee-1")).Should().BeEmpty();
    }

    /// <summary>
    /// procedure_log and journal_club declare a terminal initial state. Without the create-time
    /// exception they would be uncreatable: the state gate returns an empty writable set and the
    /// submitted data is stripped to nothing.
    /// </summary>
    [Fact]
    public void GetWritableFieldKeys_WithIgnoreStateGate_StillReturnsFieldsInATerminalInitialState()
    {
        var schema = FormSchemaParser.Parse(LegacySchemaJson);
        var workflow = WorkflowParser.Parse(TerminalInitialStateWorkflowJson);
        var activity = CreateActivity("logged");

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("trainee-1"))
            .Should().BeEmpty();

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("trainee-1"), ignoreStateGate: true)
            .Should().BeEquivalentTo(["title", "epa_id", "score"]);
    }

    [Fact]
    public void GetWritableFieldKeys_PrefersTheFieldRuleOverTheSectionRule()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "assessment",
                  "title": "Entrustment",
                  "editable_by": "field:assessor_user_id",
                  "fields": [
                    { "key": "overall_level", "type": "number", "label": "Level" },
                    { "key": "trainee_reflection", "type": "longtext", "label": "Reflection", "editable_by": "subject" }
                  ]
                }
              ]
            }
            """);
        var workflow = WorkflowParser.Parse(WbaWorkflowJson);
        var activity = CreateActivity("requested", AssessorBoundDataJson);

        _evaluator.GetWritableFieldKeys(schema, workflow, activity, CreatePrincipal("assessor-1"))
            .Should().BeEquivalentTo(["overall_level"]);
    }

    /// <summary>
    /// An activity pinned to a version whose workflow no longer declares its state is writable by
    /// nobody: there is no rule to read, so denying is the only safe reading.
    /// </summary>
    [Fact]
    public void GetWritableFieldKeys_WhenTheCurrentStateIsNotDeclared_IsEmpty()
    {
        var schema = FormSchemaParser.Parse(LegacySchemaJson);
        var workflow = WorkflowParser.Parse(LegacyWorkflowJson);

        _evaluator.GetWritableFieldKeys(schema, workflow, CreateActivity("retired_state"), CreatePrincipal("trainee-1"))
            .Should().BeEmpty();
    }

    private const string LegacySchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                { "key": "title", "type": "text", "label": "Title" },
                { "key": "epa_id", "type": "epa", "label": "EPA" },
                { "key": "score", "type": "number", "label": "Score" }
              ]
            }
          ]
        }
        """;

    /// <summary>
    /// A dead end is as final as a terminal state. The CPSA workflows declare declined/cancelled
    /// NON-terminal on purpose — marking them terminal would fire CreditApplier on a refused or
    /// withdrawn request — so the terminal flag alone does not close the surface. Without this the
    /// subject is handed a live form on a dead activity and no action can consume it.
    /// </summary>
    [Fact]
    public void GetWritableFieldKeys_InANonTerminalStateWithNoWayOut_IsWritableByNobody()
    {
        var schema = FormSchemaParser.Parse(WbaSchemaJson);
        var workflow = WorkflowParser.Parse(DeadEndWorkflowJson);

        _evaluator.GetWritableFieldKeys(schema, workflow, CreateActivity("declined"), CreatePrincipal("trainee-1"))
            .Should().BeEmpty();

        _evaluator.GetWritableFieldKeys(schema, workflow, CreateActivity("declined"), CreatePrincipal("assessor-1"))
            .Should().BeEmpty();

        // The state it can still leave is unaffected.
        _evaluator.GetWritableFieldKeys(schema, workflow, CreateActivity("draft"), CreatePrincipal("trainee-1"))
            .Should().NotBeEmpty();
    }

    private const string DeadEndWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    private const string LegacyWorkflowJson = """
        {
          "version": 1,
          "initial_state": "open",
          "states": [
            { "key": "open", "label": "Open" },
            { "key": "closed", "label": "Closed", "terminal": true }
          ],
          "transitions": [
            { "key": "close", "from": "open", "to": "closed", "actor": "subject" }
          ]
        }
        """;

    private const string TerminalInitialStateWorkflowJson = """
        {
          "version": 1,
          "initial_state": "logged",
          "states": [
            { "key": "logged", "label": "Logged", "terminal": true }
          ],
          "transitions": []
        }
        """;

    private const string WbaSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA" },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                { "key": "case_summary", "type": "longtext", "label": "Case summary" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level" }
              ]
            },
            {
              "key": "feedback",
              "title": "Feedback",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "strengths", "type": "longtext", "label": "Strengths" },
                { "key": "improvements", "type": "longtext", "label": "Improvements" },
                { "key": "plan", "type": "longtext", "label": "Plan" }
              ]
            }
          ]
        }
        """;

    private const string WbaWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    private const string AssessorBoundDataJson = """{ "epa_id": 5000, "assessor_user_id": "assessor-1" }""";

    private static Activity CreateActivity(string currentState, string dataJson = "{}")
        => new()
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CurrentState = currentState,
            DataJson = dataJson,
            ActivityType = new ActivityType
            {
                Scope = ActivityScope.Institution,
                ScopeId = 10
            }
        };

    private static ClaimsPrincipal CreatePrincipal(string userId, int? institutionId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T342, flow 03 (B8, B2, C3, E4): the Log page's instrument picker groups, and a move's words: whom it hands the
/// activity to and the sentence its result reads. Over the shipped seeds and the KGK teaching log built in the runbook's
/// Act 1 (Steps 1.26 to 1.29), which are the eleven instruments a KGK registrar is offered (Step 2.42).
/// </summary>
public sealed class FilingPickerAndMoveWordsTests
{
    // ---- the picker's groups (B8) ----------------------------------------------------------------------------------

    [Theory]
    [InlineData("cbd_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("chart_stimulated_recall_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("cca_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("direct_observation_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("dops_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("mini_cex_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("rca_cpsa", ActivityTypeShape.Rated, false)]
    [InlineData("clinical_audit_cpsa", ActivityTypeShape.DiscussedOrReviewed, true)]
    [InlineData("portfolio_review_cpsa", ActivityTypeShape.DiscussedOrReviewed, true)]
    [InlineData("reflective_exercise_cpsa", ActivityTypeShape.DiscussedOrReviewed, true)]
    public void EachKgkInstrument_FallsInItsGroup(string seed, ActivityTypeShape shape, bool creditsNothing)
    {
        ActivityTypeShapes.Of(ReadSeed(seed, "schema.json"), ReadSeed(seed, "workflow.json"), ReadSeed(seed, "credit.json"))
            .Should().Be((shape, creditsNothing));
    }

    [Fact]
    public void TheKgkTeachingLog_IsLoggedByYou_AndCreditsNothing()
    {
        ActivityTypeShapes.Of(TeachingLogSchema, TeachingLogWorkflow, """{"counts_for": []}""")
            .Should().Be((ActivityTypeShape.LoggedByYou, true));
    }

    [Fact]
    public void TheElevenInstrumentsOfStep242_AreSevenRated_ThreeDiscussedOrReviewed_AndOneLogged()
    {
        var shapes = KgkInstruments
            .Select(seed => ActivityTypeShapes.Of(
                ReadSeed(seed, "schema.json"), ReadSeed(seed, "workflow.json"), ReadSeed(seed, "credit.json")).Shape)
            .Append(ActivityTypeShapes.Of(TeachingLogSchema, TeachingLogWorkflow, """{"counts_for": []}""").Shape)
            .ToList();

        shapes.Should().HaveCount(11);
        shapes.Count(shape => shape == ActivityTypeShape.Rated).Should().Be(7);
        shapes.Count(shape => shape == ActivityTypeShape.DiscussedOrReviewed).Should().Be(3);
        shapes.Count(shape => shape == ActivityTypeShape.LoggedByYou).Should().Be(1);
    }

    [Fact]
    public void ACancelIntoADeadEnd_DoesNotMakeATypeLoggedByYou()
    {
        // Every seeded type lets its author cancel into `cancelled`, a dead end that is deliberately not terminal. Were
        // "a move by the author into a state no move leaves" the test, the reflection and the audit would be "logged".
        var workflow = WorkflowParser.Parse(ReadSeed("reflective_exercise_cpsa", "workflow.json"));
        workflow.HasOutgoingTransition("cancelled").Should().BeFalse("guard: the seed's cancel ends in a dead end");
        workflow.States.Single(state => state.Key == "cancelled").Terminal.Should().BeFalse();

        ActivityTypeShapes.Of(FormSchemaParser.Parse(ReadSeed("reflective_exercise_cpsa", "schema.json")), workflow)
            .Should().Be(ActivityTypeShape.DiscussedOrReviewed);
    }

    [Theory]
    [InlineData("procedure_log")]
    [InlineData("journal_club")]
    public void ATypeBornTerminal_IsLoggedByYou(string seed)
    {
        // The create is the whole record (T127): nobody else ever acts on it.
        ActivityTypeShapes.Of(ReadSeed(seed, "schema.json"), ReadSeed(seed, "workflow.json"), ReadSeed(seed, "credit.json"))
            .Shape.Should().Be(ActivityTypeShape.LoggedByYou);
    }

    [Fact]
    public void AMoveStraightToTerminalThatOnlyARoleMayTake_IsNotLoggedByYou()
    {
        // msf_cpsa's `record` is the staff's (role:Coordinator|role:Administrator), and its form is rated; without the
        // scale it would still not be the author's log.
        const string workflow = """
            {
              "version": 1, "initial_state": "draft",
              "states": [ { "key": "draft", "label": "Draft" }, { "key": "recorded", "label": "Recorded", "terminal": true } ],
              "transitions": [ { "key": "record", "from": "draft", "to": "recorded", "actor": "role:Coordinator" } ]
            }
            """;

        ActivityTypeShapes.Of(TeachingLogSchema, workflow, """{"counts_for": []}""").Shape
            .Should().Be(ActivityTypeShape.DiscussedOrReviewed);
    }

    [Fact]
    public void AnUnreadablePublishedVersion_IsDiscussedOrReviewed_AndCreditsNothing()
    {
        ActivityTypeShapes.Of("{ not json", null, null).Should().Be((ActivityTypeShape.DiscussedOrReviewed, true));
    }

    // ---- whom a move hands the activity to (B2, C3) ----------------------------------------------------------------

    [Theory]
    [InlineData("mini_cex_cpsa", "submit", "assessor_user_id")]
    [InlineData("cbd_cpsa", "submit", "assessor_user_id")]
    [InlineData("reflective_exercise_cpsa", "submit", "assessor_user_id")]
    [InlineData("portfolio_review_cpsa", "submit", "assessor_user_id")]
    [InlineData("clinical_audit_cpsa", "submit", "assessor_user_id")]
    [InlineData("mini_cex_cpsa", "cancel", null)]
    [InlineData("mini_cex_cpsa", "complete", null)]
    [InlineData("mini_cex_cpsa", "decline", null)]
    [InlineData("reflective_exercise_cpsa", "record_discussion", null)]
    public void AMoveHandsTheActivityOn_OnlyToWhomItsNextMoveBelongs(string seed, string move, string? field)
    {
        var workflow = WorkflowParser.Parse(ReadSeed(seed, "workflow.json"));
        var schema = FormSchemaParser.Parse(ReadSeed(seed, "schema.json"));

        MoveHandOff.NomineeFieldFor(workflow, schema, move).Should().Be(field);
    }

    [Fact]
    public void TheTeachingLogsLog_HandsItToNobody_ThoughItHasAUserField()
    {
        // Its Supervising consultant is a user field that receives nothing: the button is "Log", not "Log to …".
        var workflow = WorkflowParser.Parse(TeachingLogWorkflow);
        var schema = FormSchemaParser.Parse(TeachingLogSchema);
        schema.Sections.SelectMany(section => section.Fields).Should()
            .Contain(field => field.Key == "supervisor_user_id" && field.Type == FieldType.User, "guard");

        MoveHandOff.NomineeFieldFor(workflow, schema, "log").Should().BeNull();
        MoveHandOff.NomineeFieldFor(workflow, schema, "cancel").Should().BeNull();
    }

    [Fact]
    public void ANextMoveHeldByARole_HandsItToNobodyByName()
    {
        // The generic fallback (the Demo types): "Waiting for <state label>", no name on the button.
        const string workflow = """
            {
              "version": 1, "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" }, { "key": "submitted", "label": "Awaiting approval" },
                { "key": "approved", "label": "Approved", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject|creator" },
                { "key": "approve", "from": "submitted", "to": "approved", "actor": "role:Coordinator+scope:institution" }
              ]
            }
            """;

        MoveHandOff.NomineeFieldFor(WorkflowParser.Parse(workflow), FormSchemaParser.Parse(TeachingLogSchema), "submit")
            .Should().BeNull();
    }

    [Fact]
    public void AnUndeclaredMove_HandsItToNobody()
    {
        var workflow = WorkflowParser.Parse(ReadSeed("mini_cex_cpsa", "workflow.json"));
        var schema = FormSchemaParser.Parse(ReadSeed("mini_cex_cpsa", "schema.json"));

        MoveHandOff.NomineeFieldFor(workflow, schema, "no_such_move").Should().BeNull();
        MoveHandOff.NomineeFieldFor(workflow, schema, "submit", fromState: "requested").Should().BeNull();
    }

    // ---- the result sentence (E4) ----------------------------------------------------------------------------------

    [Theory]
    [InlineData("mini_cex_cpsa", "submit", "draft", "Submitted. It is now Requested.", "Requested", false)]
    [InlineData("reflective_exercise_cpsa", "submit", "draft", "Submitted. It is now Awaiting discussion.", "Awaiting discussion", false)]
    [InlineData("portfolio_review_cpsa", "submit", "draft", "Submitted. It is now Awaiting review.", "Awaiting review", false)]
    [InlineData("mini_cex_cpsa", "cancel", "draft", "Cancelled.", "Cancelled", false)]
    [InlineData("mini_cex_cpsa", "cancel", "requested", "Cancelled.", "Cancelled", false)]
    [InlineData("mini_cex_cpsa", "decline", "requested", "Declined.", "Declined", false)]
    [InlineData("mini_cex_cpsa", "complete", "requested", "Completed.", "Completed", true)]
    [InlineData("reflective_exercise_cpsa", "record_discussion", "submitted", "Discussed.", "Discussed", true)]
    [InlineData("reflective_exercise_cpsa", "return", "submitted", "It is now Draft.", "Draft", false)]
    // R5: a submit into a state labelled Submitted says so once.
    [InlineData("teaching_session", "submit", "draft", "Submitted.", "Submitted", false)]
    [InlineData("qi_project", "submit", "draft", "Submitted.", "Submitted", false)]
    [InlineData("research_output", "submit", "draft", "Submitted.", "Submitted", false)]
    [InlineData("reflective_note", "submit", "draft", "Submitted.", "Submitted", false)]
    [InlineData("reflective_note", "submit", "declined", "Submitted.", "Submitted", false)]
    public void TheResultSentence_ComesFromTheTargetStatesLabel(
        string seed, string move, string from, string sentence, string targetLabel, bool terminal)
    {
        var outcome = MoveOutcome.For(WorkflowParser.Parse(ReadSeed(seed, "workflow.json")), move, from);

        outcome.Should().NotBeNull();
        outcome!.ResultSentence.Should().Be(sentence);
        outcome.TargetStateLabel.Should().Be(targetLabel);
        outcome.TargetIsTerminal.Should().Be(terminal);
    }

    [Fact]
    public void TheTeachingLogsLog_ReadsLogged()
    {
        var outcome = MoveOutcome.For(WorkflowParser.Parse(TeachingLogWorkflow), "log")!;

        outcome.ResultSentence.Should().Be("Logged.");
        outcome.TargetIsTerminal.Should().BeTrue();
        outcome.TargetIsFinal.Should().BeTrue();
        new ActivityActionDto("log", false).Label.Should().Be("Log", "the button is the move's own label");
    }

    [Fact]
    public void AMoveTheWorkflowDoesNotDeclare_HasNoOutcome()
        => MoveOutcome.For(WorkflowParser.Parse(TeachingLogWorkflow), "submit").Should().BeNull();

    // ---- fixtures --------------------------------------------------------------------------------------------------

    private static readonly string[] KgkInstruments =
    [
        "cbd_cpsa", "chart_stimulated_recall_cpsa", "clinical_audit_cpsa", "cca_cpsa", "direct_observation_cpsa",
        "dops_cpsa", "mini_cex_cpsa", "portfolio_review_cpsa", "rca_cpsa", "reflective_exercise_cpsa"
    ];

    /// <summary>The KGK Teaching Session Log's form, as Step 1.27 builds it.</summary>
    internal const string TeachingLogSchema = """
        {
          "version": 1,
          "observation_date_field": "delivered_on",
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "session",
              "title": "Teaching session",
              "fields": [
                { "key": "topic", "type": "text", "label": "Topic", "required": true },
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "delivered_on", "type": "date", "label": "Date delivered", "required": true },
                {
                  "key": "audience", "type": "choice", "label": "Audience", "required": true,
                  "options": [
                    { "value": "Interns", "label": "Interns" },
                    { "value": "Medical students", "label": "Medical students" },
                    { "value": "Nursing staff", "label": "Nursing staff" },
                    { "value": "Registrars", "label": "Registrars" }
                  ]
                },
                { "key": "objectives", "type": "longtext", "label": "Learning objectives", "required": true },
                { "key": "supervisor_user_id", "type": "user", "label": "Supervising consultant" }
              ]
            }
          ]
        }
        """;

    /// <summary>The KGK Teaching Session Log's workflow, word for word from Step 1.28.</summary>
    internal const string TeachingLogWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "logged", "label": "Logged", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" },
            { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" }
          ]
        }
        """;

    private static string ReadSeed(string folder, string file)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", folder, file));
}

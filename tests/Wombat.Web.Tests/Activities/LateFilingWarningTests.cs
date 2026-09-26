using System.Globalization;
using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.GetProgrammeStartForTrainee;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.Design;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T160, D15: the encounter date warns, without refusing, while a date more than fourteen days before the filing is
/// typed; and an activity's history says which filing was late. Only on a type whose pinned credit rules can credit
/// (<see cref="EncounterDatePolicy.CanCredit" />): the server records no lateness for any other, so the form must not
/// say it will.
/// </summary>
/// <remarks>
/// T192: a date before the subject's programme started is refused on such a type, so it is never called late ("It can
/// still be filed"); the form says it will not be accepted instead, from the start the page passes it.
/// </remarks>
public sealed class LateFilingWarningTests : TestContext
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";

    private const string WarningSelector = "#observed_on-filing-notice .field-warning";

    // The pointer names observed_on. A second date field shows that only the encounter date warns.
    private const string SchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "observed_on", "type": "date", "label": "Date observed" },
                { "key": "follow_up_on", "type": "date", "label": "Follow-up" }
              ]
            }
          ]
        }
        """;

    /// <summary>Credit rules that can credit. Every form here carries them unless a test says otherwise.</summary>
    private const string CreditingRules = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    /// <summary>The rules of a research output, a journal club or a reflective exercise.</summary>
    private const string NonCreditingRules = """{ "counts_for": [] }""";

    private static readonly DateOnly FiledOn = new(2026, 9, 24);

    /// <summary>Well over fourteen days before <see cref="FiledOn" />, so a date just before it would also be late.</summary>
    private static readonly DateOnly ProgrammeStart = new(2026, 1, 1);

    private const string HintSelector = "#observed_on-filing-notice .validation-message";

    public LateFilingWarningTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddScoped<ActivityNotices>();
    }

    // ---- the form --------------------------------------------------------------------------------------------------

    [Fact]
    public void TypingADateFifteenDaysBack_ShowsTheWarning()
    {
        var cut = RenderForm(FiledOn);

        // The live region is there, empty, before anything is typed: a screen reader announces content added to a region
        // that already exists, and often not a region inserted together with its content.
        var region = cut.Find("#observed_on-filing-notice");
        region.GetAttribute("role").Should().Be("status");
        region.TextContent.Trim().Should().BeEmpty();

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-15)));

        var warning = cut.Find(WarningSelector);
        warning.TextContent.Should().Contain("15 days ago").And.Contain("recorded as late");
        cut.Find("#observed_on").GetAttribute("aria-describedby").Should().Be("observed_on-filing-notice");
        cut.Find("#observed_on-filing-notice").GetAttribute("role").Should().Be("status");
    }

    [Fact]
    public void TypingADateExactlyFourteenDaysBack_ShowsNoWarning()
    {
        var cut = RenderForm(FiledOn);

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-14)));

        cut.FindAll(WarningSelector).Should().BeEmpty();
    }

    [Fact]
    public void TheWarningFollowsTheDateAsItIsRetyped()
    {
        var cut = RenderForm(FiledOn);

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-30)));
        cut.FindAll(WarningSelector).Should().ContainSingle();

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-2)));
        cut.FindAll(WarningSelector).Should().BeEmpty();
    }

    [Fact]
    public void OnlyTheEncounterDateWarns()
    {
        var cut = RenderForm(FiledOn);

        cut.Find("#follow_up_on").Input(Iso(FiledOn.AddDays(-30)));

        cut.FindAll(".field-warning").Should().BeEmpty();
        cut.Find("#follow_up_on").HasAttribute("aria-describedby").Should().BeFalse();
    }

    [Fact]
    public void AFormThatIsNotTheFiling_NeverWarns()
    {
        // The builder preview, and an activity's page for anyone but its author before the filing.
        var cut = RenderForm(filedOn: null, dataJson: $$"""{ "observed_on": "{{Iso(FiledOn.AddDays(-30))}}" }""");

        cut.FindAll("#observed_on-filing-notice").Should().BeEmpty();
        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    [Fact]
    public void ALockedEncounterDate_DoesNotWarn()
    {
        var cut = RenderForm(
            FiledOn,
            dataJson: $$"""{ "observed_on": "{{Iso(FiledOn.AddDays(-30))}}" }""",
            writableFieldKeys: new HashSet<string>(["follow_up_on"], StringComparer.Ordinal));

        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    [Fact]
    public void AFormOfATypeThatCreditsNothing_NeverWarns()
    {
        // Its filing records no lateness, so "is recorded as late" would be untrue. No live region either: nothing will
        // ever be announced in it.
        var cut = RenderForm(FiledOn, creditRulesJson: NonCreditingRules);

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-30)));

        cut.FindAll("#observed_on-filing-notice").Should().BeEmpty();
        cut.FindAll(".field-warning").Should().BeEmpty();
        cut.Find("#observed_on").HasAttribute("aria-describedby").Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("""{ "counts_for": "all" }""")]
    public void AFormWithNoRulesOrUnreadableOnes_DoesNotWarn_AndStillRenders(string? creditRulesJson)
    {
        // The warning is advisory and the server is the authority: rules the page cannot read warn of nothing, and must
        // not take the form down with them.
        var cut = RenderForm(FiledOn, creditRulesJson: creditRulesJson);

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-30)));

        cut.FindAll(".field-warning").Should().BeEmpty();
        cut.FindAll(".alert").Should().BeEmpty("guard: the form rendered rather than failing to load");
    }

    // ---- T192: a date before the programme started -----------------------------------------------------------------

    [Fact]
    public void ADateBeforeTheProgrammeStarted_SaysItWillNotBeAccepted_AndIsNotCalledLate()
    {
        var cut = RenderForm(FiledOn, programmeStartsOn: ProgrammeStart);

        cut.Find("#observed_on").Input(Iso(ProgrammeStart.AddDays(-1)));

        cut.Find(HintSelector).TextContent.Should()
            .Contain("before the trainee's programme started (2026-01-01)").And.Contain("will not be accepted");
        cut.FindAll(WarningSelector).Should().BeEmpty("the server refuses it, so it cannot 'still be filed'");
        cut.Find("#observed_on").GetAttribute("aria-describedby").Should().Be("observed_on-filing-notice");

        // A predicted refusal marks the value itself as wrong, not only a note beside it.
        cut.Find("#observed_on").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#observed_on").ClassList.Should().Contain("input-validation-error");
        InvalidFieldStyleTests.ShowsInvalid(cut.Find("#observed_on")).Should().BeTrue("the input shows it, not only the hint (T236)");
    }

    [Fact]
    public void TheDayTheProgrammeStarted_IsAccepted_SoOnlyItsLatenessIsWarnedOf()
    {
        // The server's bound is "not before" the start: the day itself is accepted, and it is 266 days before the filing.
        var cut = RenderForm(FiledOn, programmeStartsOn: ProgrammeStart);

        cut.Find("#observed_on").Input(Iso(ProgrammeStart));

        cut.FindAll(HintSelector).Should().BeEmpty();
        cut.Find(WarningSelector).TextContent.Should().Contain("266 days ago");

        // A late filing is accepted, so the value is not marked invalid.
        cut.Find("#observed_on").HasAttribute("aria-invalid").Should().BeFalse();
        cut.Find("#observed_on").ClassList.Should().NotContain("input-validation-error");
        InvalidFieldStyleTests.ShowsInvalid(cut.Find("#observed_on")).Should().BeFalse();
    }

    [Fact]
    public void TheHintFollowsTheDateAsItIsRetyped()
    {
        var cut = RenderForm(FiledOn, programmeStartsOn: ProgrammeStart);

        cut.Find("#observed_on").Input(Iso(ProgrammeStart.AddDays(-10)));
        cut.FindAll(HintSelector).Should().ContainSingle();

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-2)));
        cut.FindAll(HintSelector).Should().BeEmpty();
        cut.FindAll(WarningSelector).Should().BeEmpty();
        cut.Find("#observed_on").HasAttribute("aria-invalid").Should().BeFalse();
    }

    [Fact]
    public void WithNoKnownStart_APreProgrammeDate_IsWarnedOfAsLate_AsBefore()
    {
        // No profile, or a viewer who may not read it: the page passes nothing. With no profile the server does not bound
        // the date (EncounterDateGate), so lateness is the true thing to say.
        var cut = RenderForm(FiledOn, programmeStartsOn: null);

        cut.Find("#observed_on").Input(Iso(ProgrammeStart.AddDays(-1)));

        cut.FindAll(HintSelector).Should().BeEmpty();
        cut.Find(WarningSelector).TextContent.Should().Contain("days ago");
    }

    [Fact]
    public void ATypeThatCreditsNothing_MayPrecedeTheProgramme_AndIsNotHinted()
    {
        // A research output or reflective exercise may be dated from before admission: the server does not bound it.
        var cut = RenderForm(FiledOn, creditRulesJson: NonCreditingRules, programmeStartsOn: ProgrammeStart);

        cut.Find("#observed_on").Input(Iso(ProgrammeStart.AddDays(-1)));

        cut.FindAll("#observed_on-filing-notice").Should().BeEmpty();
        cut.FindAll(".validation-message").Should().BeEmpty();
    }

    [Fact]
    public void AFormThatIsNotTheFiling_IsNotHinted()
    {
        var cut = RenderForm(
            filedOn: null,
            dataJson: $$"""{ "observed_on": "{{Iso(ProgrammeStart.AddDays(-1))}}" }""",
            programmeStartsOn: ProgrammeStart);

        cut.FindAll("#observed_on-filing-notice").Should().BeEmpty();
        cut.FindAll(".validation-message").Should().BeEmpty();
    }

    [Fact]
    public void OnlyTheEncounterDateIsHinted()
    {
        var cut = RenderForm(FiledOn, programmeStartsOn: ProgrammeStart);

        cut.Find("#follow_up_on").Input(Iso(ProgrammeStart.AddDays(-1)));

        cut.FindAll(".validation-message").Should().BeEmpty();
    }

    // ---- the create page -------------------------------------------------------------------------------------------

    [Fact]
    public void TheCreatePage_WarnsAgainstToday_WhileTheDateIsTyped()
    {
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddSingleton<IScopedSender>(new CreatePageSender());

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);
        cut.Find("#activity-type").Change("2");
        cut.WaitForState(() => cut.FindAll("#observed_on").Count == 1);

        cut.Find("#observed_on").Input(Iso(FilingLateness.Today().AddDays(-14)));
        cut.FindAll(WarningSelector).Should().BeEmpty();

        cut.Find("#observed_on").Input(Iso(FilingLateness.Today().AddDays(-15)));
        cut.Find(WarningSelector).TextContent.Should().Contain("15 days ago");
    }

    [Fact]
    public void TheCreatePage_HintsAtTheAuthorsProgrammeStart_InPlaceOfTheLatenessWarning()
    {
        // The page asks for the author's start once and hands it to the form: the author is the subject here.
        var startedOn = FilingLateness.Today().AddDays(-100);
        var sender = new CreatePageSender(startedOn);
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);
        cut.Find("#activity-type").Change("2");
        cut.WaitForState(() => cut.FindAll("#observed_on").Count == 1);

        cut.Find("#observed_on").Input(Iso(startedOn.AddDays(-1)));
        cut.Find(HintSelector).TextContent.Should().Contain($"programme started ({Iso(startedOn)})");
        cut.FindAll(WarningSelector).Should().BeEmpty();

        cut.Find("#observed_on").Input(Iso(startedOn));
        cut.FindAll(HintSelector).Should().BeEmpty();
        cut.Find(WarningSelector).TextContent.Should().Contain("100 days ago");

        sender.ProgrammeStartAskedFor.Should().Equal(TraineeId);
    }

    [Fact]
    public void TheCreatePage_LogsAStartThatCannotBeRead()
    {
        var logger = new CapturingLogger<NewActivity>();
        Services.AddSingleton<ILogger<NewActivity>>(logger);
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        // No start given: the fake refuses the read, as a failing query would.
        Services.AddSingleton<IScopedSender>(new CreatePageSender());

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);

        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Exception is NotSupportedException &&
            entry.Message.Contains(TraineeId));
    }

    // ---- the activity's page ---------------------------------------------------------------------------------------

    [Fact]
    public void TheHistory_SaysWhichFilingWasLate()
    {
        var cut = RenderPage(Detail(state: "requested", submitDaysAfterEncounter: 15, editable: []));

        cut.Markup.Should().Contain("Filed 15 days after the encounter");
    }

    [Fact]
    public void TheHistory_SaysNothingOfAFilingOnTime()
    {
        var cut = RenderPage(Detail(state: "requested", submitDaysAfterEncounter: 14, editable: []));

        cut.Markup.Should().NotContain("days after the encounter");
    }

    [Fact]
    public void TheAuthorsDraft_WarnsAgainstToday()
    {
        var detail = Detail(
            state: "draft",
            submitDaysAfterEncounter: null,
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-15));

        var cut = RenderPage(detail);

        cut.Find(WarningSelector).TextContent.Should().Contain("15 days ago");
    }

    [Fact]
    public void TheAuthorsDraft_HintsAtTheProgrammeStart_InPlaceOfTheLatenessWarning()
    {
        var startedOn = FilingLateness.Today().AddDays(-100);
        var detail = Detail(
            state: "draft",
            submitDaysAfterEncounter: null,
            editable: ["observed_on"],
            observedOn: startedOn.AddDays(-1));
        var sender = new FakeSender(detail, startedOn);

        var cut = RenderPage(sender);

        // The stored date is handed on at the submit, where the server judges it whether it changed or not.
        cut.Find(HintSelector).TextContent.Should().Contain($"programme started ({Iso(startedOn)})");
        cut.FindAll(WarningSelector).Should().BeEmpty();
        sender.ProgrammeStartAskedFor.Should().Equal(TraineeId);
    }

    [Fact]
    public void SomeoneElsesView_DoesNotAskForTheProgrammeStart()
    {
        // Only a page that may be the filing uses it, so no other view reads it.
        var detail = Detail(
            state: "draft",
            submitDaysAfterEncounter: null,
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-200),
            subjectId: "someone-else");
        var sender = new FakeSender(detail, FilingLateness.Today().AddDays(-100));

        var cut = RenderPage(sender);

        cut.FindAll(".validation-message").Should().BeEmpty();
        sender.ProgrammeStartAskedFor.Should().BeEmpty();
    }

    [Fact]
    public void AStartThatCannotBeRead_IsLogged_AndTheAuthorsDraftStillWarnsOfLateness()
    {
        // No hint is also what a trainee with no profile sees, so a read that fails has to be told apart somewhere, or a
        // broken query would hide the hint on every page and look like nothing at all.
        var logger = new CapturingLogger<ActivityView>();
        Services.AddSingleton<ILogger<ActivityView>>(logger);
        var detail = Detail(
            state: "draft",
            submitDaysAfterEncounter: null,
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-15));

        // No start given: the fake refuses the read, as a failing query would.
        var cut = RenderPage(new FakeSender(detail));

        cut.Find(WarningSelector).TextContent.Should().Contain("15 days ago");
        cut.FindAll(HintSelector).Should().BeEmpty();
        logger.Entries.Should().ContainSingle(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Exception is NotSupportedException &&
            entry.Message.Contains(TraineeId));
    }

    [Fact]
    public void TheAuthorsDraft_OfATypeThatCreditsNothing_DoesNotWarn()
    {
        var detail = Detail(
            state: "draft",
            submitDaysAfterEncounter: null,
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-15),
            creditRulesJson: NonCreditingRules);

        var cut = RenderPage(detail);

        cut.Find("#observed_on").HasAttribute("disabled").Should().BeFalse("guard: the date can still be typed in");
        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    [Fact]
    public void SomeoneElsesView_OfTheSameDraft_DoesNotWarn()
    {
        var detail = Detail(
            state: "draft",
            submitDaysAfterEncounter: null,
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-15),
            subjectId: "someone-else");

        var cut = RenderPage(detail);

        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    [Fact]
    public void ADraftASupervisorReturned_WasFiledAlready_AndDoesNotWarn()
    {
        // The re-submission is not a second filing (the server records nothing on it), so warning that it will be
        // "recorded as late" would be untrue.
        var created = new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc);
        var detail = DetailFor(
            ReturnableWorkflow,
            state: "draft",
            transitions:
            [
                Row(1, "draft", "draft", "create", TraineeId, created),
                Row(2, "draft", "submitted", "submit", TraineeId, created.AddHours(1), daysAfterEncounter: 3),
                Row(3, "submitted", "draft", "return", AssessorId, created.AddDays(15))
            ],
            actions: ["submit", "cancel"],
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-20));

        var cut = RenderPage(detail);

        cut.Find("#observed_on").HasAttribute("disabled").Should().BeFalse("guard: the date can still be typed in");
        cut.FindAll("#observed_on-filing-notice").Should().BeEmpty();
        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    [Fact]
    public void ATypeBornInRequested_WasFiledByItsCreate_AndDoesNotWarn()
    {
        // The generic mini_cex: the create was the filing, and the author's one move left is the withdrawal. The date is
        // still writable in `requested`, where no editable_by is declared.
        const string requestBornWorkflow = """
            {
              "version": 1,
              "initial_state": "requested",
              "states": [
                { "key": "requested", "label": "Requested" },
                { "key": "completed", "label": "Completed", "terminal": true },
                { "key": "cancelled", "label": "Cancelled" }
              ],
              "transitions": [
                { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" },
                { "key": "cancel", "from": "requested", "to": "cancelled", "actor": "subject|creator" }
              ]
            }
            """;
        var created = new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc);
        var detail = DetailFor(
            requestBornWorkflow,
            state: "requested",
            transitions: [Row(1, "requested", "requested", "create", TraineeId, created, daysAfterEncounter: 20)],
            actions: ["cancel"],
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-20));

        var cut = RenderPage(detail);

        cut.Find("#observed_on").HasAttribute("disabled").Should().BeFalse("guard: the date can still be typed in");
        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    [Fact]
    public void AMoveFromAnotherState_IsNotTheFiling_EvenUnderTheSameKey()
    {
        // The server's rule is a move out of the INITIAL state. A builder-made workflow may reuse a key elsewhere: here
        // `submit` out of `paused` only goes back to the draft, and nothing has been filed yet.
        const string pausableWorkflow = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "paused", "label": "Paused" },
                { "key": "requested", "label": "Requested" },
                { "key": "completed", "label": "Completed", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
                { "key": "pause", "from": "draft", "to": "paused", "actor": "subject|creator" },
                { "key": "submit", "from": "paused", "to": "draft", "actor": "subject|creator" },
                { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
              ]
            }
            """;
        var created = new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc);
        var detail = DetailFor(
            pausableWorkflow,
            state: "paused",
            transitions:
            [
                Row(1, "draft", "draft", "create", TraineeId, created),
                Row(2, "draft", "paused", "pause", TraineeId, created.AddHours(1))
            ],
            actions: ["submit"],
            editable: ["observed_on"],
            observedOn: FilingLateness.Today().AddDays(-20));

        var cut = RenderPage(detail);

        cut.Find("#observed_on").HasAttribute("disabled").Should().BeFalse("guard: the date can still be typed in");
        cut.FindAll(".field-warning").Should().BeEmpty();
    }

    // ---- today -----------------------------------------------------------------------------------------------------

    [Fact]
    public void Today_IsTheSouthAfricanDate()
    {
        // 22:30 UTC on the 24th is 00:30 on the 25th in South Africa, the calendar the write path counts lateness on.
        var lateEvening = new FixedClock(new DateTimeOffset(2026, 9, 24, 22, 30, 0, TimeSpan.Zero));

        FilingLateness.Today(lateEvening).Should().Be(new DateOnly(2026, 9, 25));
    }

    // ---- helpers ---------------------------------------------------------------------------------------------------

    /// <summary><c>reflective_exercise_cpsa</c>'s shape: a supervisor may return a submission to the draft.</summary>
    private const string ReturnableWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" },
            { "key": "discussed", "label": "Discussed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject|creator" },
            { "key": "record_discussion", "from": "submitted", "to": "discussed", "actor": "field:assessor_user_id" },
            { "key": "return", "from": "submitted", "to": "draft", "actor": "field:assessor_user_id" },
            { "key": "cancel", "from": ["draft", "submitted"], "to": "cancelled", "actor": "subject|creator" }
          ]
        }
        """;

    private static ActivityTransitionDto Row(
        int id,
        string from,
        string to,
        string key,
        string actorId,
        DateTime occurredOn,
        int? daysAfterEncounter = null)
        // The labels are the keys in words: these tests read the lateness line, not the labels (T220 tests those).
        => new(id, from, to, key, WorkflowTransition.LabelFor(from), WorkflowTransition.LabelFor(to), WorkflowTransition.LabelFor(key),
            actorId, occurredOn, null, "{}", null, null, daysAfterEncounter);

    private static ActivityDetailDto DetailFor(
        string workflowJson,
        string state,
        IReadOnlyList<ActivityTransitionDto> transitions,
        IReadOnlyList<string> actions,
        IReadOnlyList<string> editable,
        DateOnly observedOn)
    {
        var created = transitions[0].OccurredOn;
        var activity = new ActivityDto(
            11,
            2,
            "reflective_exercise_cpsa",
            "Reflective exercise",
            null,
            1,
            SchemaJson,
            workflowJson,
            "[]",
            CreditingRules,
            TraineeId,
            1,
            TraineeId,
            state,
            PinnedWorkflows.StateLabel(PinnedWorkflows.TryParse(workflowJson), state),
            $$"""{ "observed_on": "{{Iso(observedOn)}}" }""",
            null,
            null,
            observedOn,
            true,
            created,
            transitions[^1].OccurredOn,
            transitions);

        return new ActivityDetailDto(
            activity,
            editable,
            actions.Select(action => new ActivityActionDto(action, false)).ToList());
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private IRenderedComponent<ActivityForm> RenderForm(
        DateOnly? filedOn,
        string dataJson = "{}",
        IReadOnlySet<string>? writableFieldKeys = null,
        string? creditRulesJson = CreditingRules,
        DateOnly? programmeStartsOn = null)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, SchemaJson)
            .Add(component => component.DataJson, dataJson)
            .Add(component => component.CreditRulesJson, creditRulesJson)
            .Add(component => component.EditableFieldKeys, writableFieldKeys)
            .Add(component => component.FiledOn, filedOn)
            .Add(component => component.ProgrammeStartsOn, programmeStartsOn)
            .Add(component => component.DataJsonChanged, EventCallback.Factory.Create<string>(this, _ => { })));

    private IRenderedComponent<ActivityView> RenderPage(ActivityDetailDto detail) => RenderPage(new FakeSender(detail));

    private IRenderedComponent<ActivityView> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 11));
        cut.WaitForState(() => cut.Markup.Contains("Activity details"));

        return cut;
    }

    private static ActivityDetailDto Detail(
        string state,
        int? submitDaysAfterEncounter,
        IReadOnlyList<string> editable,
        DateOnly? observedOn = null,
        string subjectId = TraineeId,
        string creditRulesJson = CreditingRules)
    {
        const string workflowJson = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "requested", "label": "Requested" },
                { "key": "completed", "label": "Completed", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
                { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
              ]
            }
            """;

        var created = new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc);
        List<ActivityTransitionDto> transitions =
        [
            new(1, "draft", "draft", "create", "Draft", "Draft", "Create", subjectId, created, null, "{}", null, null, null)
        ];

        if (state != "draft")
        {
            transitions.Add(new ActivityTransitionDto(
                2, "draft", "requested", "submit", "Draft", "Requested", "Submit", subjectId, created.AddHours(1), null, "{}", null, null,
                submitDaysAfterEncounter));
        }

        var dataJson = observedOn is DateOnly date ? $$"""{ "observed_on": "{{Iso(date)}}" }""" : "{}";

        var activity = new ActivityDto(
            11,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            "mini_cex",
            1,
            SchemaJson,
            workflowJson,
            "[]",
            creditRulesJson,
            subjectId,
            1,
            subjectId,
            state,
            PinnedWorkflows.StateLabel(PinnedWorkflows.TryParse(workflowJson), state),
            dataJson,
            null,
            null,
            observedOn ?? DateOnly.FromDateTime(created),
            observedOn is not null,
            created,
            created.AddHours(1),
            transitions);

        IReadOnlyList<ActivityActionDto> actions = state == "draft" ? [new ActivityActionDto("submit", false)] : [];
        return new ActivityDetailDto(activity, editable, actions);
    }

    /// <summary>
    /// The create page's reads: one type, its editor with the dated schema, and the author's programme start when a test
    /// gives one (T192). Without one it refuses that read as an unhandled request, which the page takes as no start.
    /// </summary>
    private sealed class CreatePageSender(DateOnly? programmeStart = null) : IScopedSender
    {
        public List<string> ProgrammeStartAskedFor { get; } = [];

        private const string DraftBornWorkflow = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "completed", "label": "Completed", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "completed", "actor": "subject|creator" }
              ]
            }
            """;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    IReadOnlyList<ActivityTypeListItemDto> types =
                        [new ActivityTypeListItemDto(2, "mini_cex_cpsa", "Mini-CEX", ActivityScope.Global, null, 1, true)];
                    return Task.FromResult((TResponse)(object)types);

                case GetActivityTypeEditorQuery:
                    var editor = new ActivityTypeEditorDto(
                        2, "mini_cex_cpsa", "Mini-CEX", null, ActivityScope.Global, null, true, "mini_cex", 1, false,
                        SchemaJson, DraftBornWorkflow, CreditingRules, "[]",
                        SchemaJson, DraftBornWorkflow, CreditingRules, "[]",
                        "admin-1", null, null, [], false, [], null);
                    return Task.FromResult((TResponse)(object)editor);

                case GetProgrammeStartForTraineeQuery query when programmeStart is not null:
                    ProgrammeStartAskedFor.Add(query.TraineeUserId);
                    return Task.FromResult((TResponse)(object)programmeStart);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    /// <summary>What a page logged, so a test can see a failure the page otherwise shows only as nothing.</summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, exception, formatter(state, exception)));
    }

    /// <summary>
    /// The activity page's read, and the subject's programme start when a test gives one (T192). Without one it refuses
    /// that read, which the page takes as no start.
    /// </summary>
    private sealed class FakeSender(ActivityDetailDto detail, DateOnly? programmeStart = null) : IScopedSender
    {
        public List<string> ProgrammeStartAskedFor { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    return Task.FromResult((TResponse)(object)detail);

                case GetProgrammeStartForTraineeQuery query:
                    ProgrammeStartAskedFor.Add(query.TraineeUserId);
                    return programmeStart is null
                        ? throw new NotSupportedException("No programme start in this test.")
                        : Task.FromResult((TResponse)(object)programmeStart);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

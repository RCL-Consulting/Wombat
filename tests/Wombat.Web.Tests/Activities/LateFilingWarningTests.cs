using System.Globalization;
using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T160, D15: the encounter date warns, without refusing, while a date more than fourteen days before the filing is
/// typed; and an activity's history says which filing was late. Only on a type whose pinned credit rules can credit
/// (<see cref="EncounterDatePolicy.CanCredit" />): the server records no lateness for any other, so the form must not
/// say it will.
/// </summary>
public sealed class LateFilingWarningTests : TestContext
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";

    private const string WarningSelector = "#observed_on-late-filing .field-warning";

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
        var region = cut.Find("#observed_on-late-filing");
        region.GetAttribute("role").Should().Be("status");
        region.TextContent.Trim().Should().BeEmpty();

        cut.Find("#observed_on").Input(Iso(FiledOn.AddDays(-15)));

        var warning = cut.Find(WarningSelector);
        warning.TextContent.Should().Contain("15 days ago").And.Contain("recorded as late");
        cut.Find("#observed_on").GetAttribute("aria-describedby").Should().Be("observed_on-late-filing");
        cut.Find("#observed_on-late-filing").GetAttribute("role").Should().Be("status");
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

        cut.FindAll("#observed_on-late-filing").Should().BeEmpty();
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

        cut.FindAll("#observed_on-late-filing").Should().BeEmpty();
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
        cut.FindAll("#observed_on-late-filing").Should().BeEmpty();
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
        => new(id, from, to, key, actorId, occurredOn, null, "{}", null, null, daysAfterEncounter);

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
        string? creditRulesJson = CreditingRules)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(component => component.SchemaJson, SchemaJson)
            .Add(component => component.DataJson, dataJson)
            .Add(component => component.CreditRulesJson, creditRulesJson)
            .Add(component => component.EditableFieldKeys, writableFieldKeys)
            .Add(component => component.FiledOn, filedOn)
            .Add(component => component.DataJsonChanged, EventCallback.Factory.Create<string>(this, _ => { })));

    private IRenderedComponent<ActivityView> RenderPage(ActivityDetailDto detail)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(detail));

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
            new(1, "draft", "draft", "create", subjectId, created, null, "{}", null, null, null)
        ];

        if (state != "draft")
        {
            transitions.Add(new ActivityTransitionDto(
                2, "draft", "requested", "submit", subjectId, created.AddHours(1), null, "{}", null, null, submitDaysAfterEncounter));
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

    /// <summary>The create page's two reads: one type, and its editor with the dated schema.</summary>
    private sealed class CreatePageSender : IScopedSender
    {
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
                        "admin-1", null, null, []);
                    return Task.FromResult((TResponse)(object)editor);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly ActivityDetailDto _detail;

        public FakeSender(ActivityDetailDto detail) => _detail = detail;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetActivityByIdQuery
                ? Task.FromResult((TResponse)(object)_detail)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

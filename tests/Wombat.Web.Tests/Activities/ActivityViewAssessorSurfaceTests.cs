using System.Security.Claims;
using System.Text.Json;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 7: the assessor's editable surface on the activity detail page.
/// </summary>
/// <remarks>
/// The symptom this task exists for is that an assessor had no way to enter a rating: the page was
/// read-only and the transition carried a literal <c>null</c> in the patch slot. The load-bearing
/// assertion here is <see cref="AssessorEdits_RideTheTransitionAsAPatchOfTheirOwnKeys" /> — the
/// captured <see cref="TransitionActivityCommand" /> must carry the assessor's values, and only
/// theirs, so <c>CreditApplier</c> grades what the assessor entered.
/// </remarks>
public sealed class ActivityViewAssessorSurfaceTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "text", "label": "EPA" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Overall level" },
                { "key": "strengths", "type": "longtext", "label": "Strengths" }
              ]
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    private const string CreditRulesJson = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 }
          ]
        }
        """;

    // What the trainee filed: the request keys, and a rating the assessor has not entered yet.
    private const string StoredDataJson = """{"epa_id":"3","assessor_user_id":"assessor-1"}""";

    public ActivityViewAssessorSurfaceTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("assessor@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "assessor-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void BoundAssessor_GetsAnEditableSurface_WithTheTraineesRequestStillLocked()
    {
        var cut = RenderPage(new FakeSender(Detail(Assessor())));

        cut.Find("#overall_level").HasAttribute("disabled").Should().BeFalse();
        cut.Find("#strengths").HasAttribute("disabled").Should().BeFalse();
        cut.Find("#epa_id").HasAttribute("disabled").Should()
            .BeTrue("the assessor must not be able to redirect which EPA gets credited");

        cut.Markup.Should().Contain("alert-info");
        cut.Find("#discard-changes").Should().NotBeNull();
    }

    [Fact]
    public void NonBoundUser_StaysFullyReadOnly_WithNoEditingAffordance()
    {
        // An empty writable set is what the server returns for a user the state's rule excludes.
        var cut = RenderPage(new FakeSender(Detail(NoOne())));

        cut.FindAll("input, textarea, select")
            .Where(element => !element.HasAttribute("disabled"))
            .Should().BeEmpty("a non-bound user's view of an activity is read-only");

        cut.FindAll("#discard-changes").Should().BeEmpty();
        cut.Markup.Should().NotContain("alert-info");
    }

    [Fact]
    public void AnActorWithNoWritableFields_CanStillAct_AndSendsNoPatch()
    {
        // The trainee looking at their own submitted WBA: the assessor owns every editable field in
        // `requested`, but `cancel` is still theirs. Read-only and actionable are independent.
        var sender = new FakeSender(Detail(NoOne(), availableActions: [new ActivityActionDto("cancel", false)]));
        var cut = RenderPage(sender);

        cut.FindAll("#discard-changes").Should().BeEmpty();

        ClickAction(cut, "Cancel");

        var command = sender.Transitions.Should().ContainSingle().Subject;
        command.TransitionKey.Should().Be("cancel");
        command.DataPatchJson.Should().BeNull("an actor who owns no field has nothing to patch");
    }

    [Fact]
    public void AssessorEdits_RideTheTransitionAsAPatchOfTheirOwnKeys()
    {
        var sender = new FakeSender(Detail(Assessor()), Detail(NoOne(), state: "completed"));
        var cut = RenderPage(sender);

        cut.Find("#overall_level").Input("5");
        cut.Find("#strengths").Input("Clear structured handover.");

        ClickAction(cut, "Complete");

        var command = sender.Transitions.Should().ContainSingle().Subject;
        command.TransitionKey.Should().Be("complete");
        command.ActorUserId.Should().Be("assessor-1");
        command.Note.Should().BeNull();

        command.DataPatchJson.Should().NotBeNull("the whole point of T070 is that the rating rides the transition");

        using var patch = JsonDocument.Parse(command.DataPatchJson!);
        patch.RootElement.GetProperty("overall_level").GetString().Should().Be("5");
        patch.RootElement.GetProperty("strengths").GetString().Should().Be("Clear structured handover.");

        // Only the assessor's own keys: a patch naming epa_id or assessor_user_id would be rejected
        // by MergeWritableKeys, and an untouched writable key is not worth sending.
        patch.RootElement.EnumerateObject().Select(property => property.Name)
            .Should().BeEquivalentTo(["overall_level", "strengths"]);
    }

    [Fact]
    public void AfterATransition_ThePageReloadsAndTheSurfaceFollowsTheNewState()
    {
        var sender = new FakeSender(Detail(Assessor()), Detail(NoOne(), state: "completed"));
        var cut = RenderPage(sender);

        cut.Find("#overall_level").Input("5");
        ClickAction(cut, "Complete");

        cut.WaitForState(() => cut.FindAll("#discard-changes").Count == 0);

        sender.LoadCount.Should().Be(2, "the writable set and the action list belong to the new state");
        cut.Markup.Should().Contain("State: completed");
    }

    [Fact]
    public void ARejectedTransition_ShowsTheErrorAndKeepsWhatTheAssessorTyped()
    {
        var sender = new FakeSender(Detail(Assessor()))
        {
            TransitionFailure = new InvalidOperationException("strengths: A value is required")
        };
        var cut = RenderPage(sender);

        cut.Find("#overall_level").Input("5");
        ClickAction(cut, "Complete");

        cut.WaitForState(() => cut.Markup.Contains("A value is required"));

        cut.Find("#overall_level").GetAttribute("value").Should()
            .Be("5", "a rejected transition must not discard the assessor's work");
    }

    [Fact]
    public void Discard_RevertsTheFormToWhatIsStored()
    {
        var cut = RenderPage(new FakeSender(Detail(Assessor())));

        cut.Find("#discard-changes").HasAttribute("disabled").Should().BeTrue("nothing has changed yet");

        cut.Find("#overall_level").Input("5");
        cut.Find("#discard-changes").HasAttribute("disabled").Should().BeFalse();

        cut.Find("#discard-changes").Click();

        cut.Find("#overall_level").GetAttribute("value").Should().BeEmpty();
        cut.Find("#discard-changes").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void RetypingTheStoredValue_LeavesNothingToSubmit()
    {
        var sender = new FakeSender(Detail(Assessor(), dataJson: """{"epa_id":"3","assessor_user_id":"assessor-1","overall_level":"4"}"""));
        var cut = RenderPage(sender);

        cut.Find("#overall_level").Input("5");
        cut.Find("#overall_level").Input("4");

        cut.Find("#discard-changes").HasAttribute("disabled").Should()
            .BeTrue("the diff the server would receive is empty");

        ClickAction(cut, "Complete");

        sender.Transitions.Should().ContainSingle().Which.DataPatchJson.Should().BeNull();
    }

    [Fact]
    public void TheHistoryCard_RendersTheTransitionNote_WhichNothingUsedToShow()
    {
        var transitions = new[]
        {
            new ActivityTransitionDto(1, "draft", "requested", "submit", "trainee-1", new DateTime(2026, 9, 16, 8, 0, 0, DateTimeKind.Utc), null, "{}", null, null),
            new ActivityTransitionDto(2, "requested", "declined", "decline", "assessor-1", new DateTime(2026, 9, 17, 9, 30, 0, DateTimeKind.Utc), "Wrong patient encounter.", "{}", null, null)
        };

        var cut = RenderPage(new FakeSender(Detail(NoOne(), transitions: transitions)));

        cut.Markup.Should().Contain("Wrong patient encounter.");
        cut.Markup.Should().Contain("decline");
        cut.Markup.Should().Contain("requested → declined");
    }

    [Fact]
    public void TheHistoryCard_HasAnEmptyState()
    {
        var cut = RenderPage(new FakeSender(Detail(NoOne())));

        cut.Markup.Should().Contain("No workflow actions have been recorded yet.");
    }

    private IRenderedComponent<ActivityView> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 7));
        cut.WaitForState(() => cut.Markup.Contains("Activity details"));

        return cut;
    }

    private static void ClickAction(IRenderedComponent<ActivityView> cut, string label)
        => cut.FindAll("button")
            .First(button => string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal))
            .Click();

    private static IReadOnlyList<string> Assessor() => ["overall_level", "strengths"];

    private static IReadOnlyList<string> NoOne() => [];

    private static ActivityDetailDto Detail(
        IReadOnlyList<string> editableFieldKeys,
        string state = "requested",
        string dataJson = StoredDataJson,
        IReadOnlyList<ActivityTransitionDto>? transitions = null,
        IReadOnlyList<ActivityActionDto>? availableActions = null,
        string creditRulesJson = CreditRulesJson)
    {
        var activity = new ActivityDto(
            7,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            1,
            SchemaJson,
            WorkflowJson,
            "[]",
            creditRulesJson,
            "trainee-1",
            "trainee-1",
            state,
            dataJson,
            null,
            null,
            new DateTime(2026, 9, 16, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 16, 8, 0, 0, DateTimeKind.Utc),
            transitions ?? []);

        var actions = availableActions
            ?? (string.Equals(state, "requested", StringComparison.Ordinal) && editableFieldKeys.Count > 0
                ? [new ActivityActionDto("complete", false)]
                : []);

        return new ActivityDetailDto(activity, editableFieldKeys, actions);
    }

    /// <summary>
    /// Answers the two requests the page makes. Each <c>GetActivityByIdQuery</c> takes the next
    /// supplied detail, so a test can model what the server returns once the transition has landed.
    /// </summary>
    private sealed class FakeSender : IScopedSender
    {
        private readonly ActivityDetailDto[] _details;

        public FakeSender(params ActivityDetailDto[] details)
        {
            _details = details;
        }

        public List<TransitionActivityCommand> Transitions { get; } = [];

        public Exception? TransitionFailure { get; set; }

        public int LoadCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    var detail = _details[Math.Min(LoadCount, _details.Length - 1)];
                    LoadCount++;
                    return Task.FromResult((TResponse)(object)detail);

                case TransitionActivityCommand command:
                    Transitions.Add(command);
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    return Task.FromResult((TResponse)(object)_details[^1].Activity);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

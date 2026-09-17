using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T108: a completed activity that credited nothing says so, where a clinician will see it.
/// </summary>
/// <remarks>
/// The banner sits above the details grid, OUTSIDE the editable/read-only branch. That matters: a
/// completed activity has no writable fields and no available actions, so it always renders the
/// read-only branch — which is exactly the activity this is meant to warn about.
/// </remarks>
public sealed class ActivityViewCreditSignalTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "Details", "fields": [ { "key": "epa_id", "type": "text", "label": "EPA" } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "complete", "from": "requested", "to": "completed", "actor": "subject" }
          ]
        }
        """;

    public ActivityViewCreditSignalTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void ACompletionThatCreditedNothing_IsFlagged()
    {
        var cut = RenderPage(Completion(creditedItemCount: 0));

        cut.Markup.Should().Contain("alert-warning");
        cut.Markup.Should().Contain("counted towards no curriculum requirement");
        cut.Markup.Should().Contain("None", "the history row spells out the same thing");
    }

    [Fact]
    public void ACompletionThatCredited_IsNotFlagged()
    {
        var cut = RenderPage(Completion(creditedItemCount: 2));

        cut.Markup.Should().NotContain("alert-warning");
        cut.Markup.Should().Contain("2 items");
    }

    [Fact]
    public void ATypeThatCreditsNothingByDesign_IsNeverFlagged()
    {
        // A reflective note, a journal club entry, a procedure log: credit was never evaluated, so the
        // stamp is null and there is nothing to warn about. This is also the state of every
        // transition recorded before the outcome started being stamped.
        var cut = RenderPage(Completion(creditedItemCount: null));

        cut.Markup.Should().NotContain("alert-warning");
        cut.Markup.Should().NotContain("counted towards no curriculum requirement");
    }

    private IRenderedComponent<ActivityView> RenderPage(ActivityDetailDto detail)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(detail));

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 11));
        cut.WaitForState(() => cut.Markup.Contains("Activity details"));

        return cut;
    }

    private static ActivityDetailDto Completion(int? creditedItemCount)
    {
        var transition = new ActivityTransitionDto(
            1,
            "requested",
            "completed",
            "complete",
            "assessor-1",
            new DateTime(2026, 9, 17, 9, 30, 0, DateTimeKind.Utc),
            null,
            "{}",
            creditedItemCount);

        var activity = new ActivityDto(
            11,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            1,
            SchemaJson,
            WorkflowJson,
            "[]",
            """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""",
            "trainee-1",
            "trainee-1",
            "completed",
            """{"epa_id":"17"}""",
            null,
            null,
            new DateTime(2026, 9, 17, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 17, 9, 30, 0, DateTimeKind.Utc),
            [transition]);

        // A completed activity: nothing writable, nothing to do. The read-only branch.
        return new ActivityDetailDto(activity, [], []);
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

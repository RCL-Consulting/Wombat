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
/// Since T342 (flow 03, C13) the warning is a <c>.field-warning</c> under the About card's Credit, outside the form's
/// editable/read-only branch: a completed activity has no writable fields and no available actions, so it always renders
/// the read-only branch — which is exactly the activity this is meant to warn about.
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
        Services.AddScoped<ActivityNotices>();
    }

    [Fact]
    public void ACompletionThatCreditedNothing_IsFlagged()
    {
        var cut = RenderPage(Completion(creditedItemCount: 0));

        var warning = CreditWarning(cut);
        warning.HasAttribute("role").Should().BeFalse(
            "it is standing page content, there on every visit, not news to announce (T193)");
        cut.Markup.Should().Contain("counted towards no curriculum requirement");
        cut.Markup.Should().Contain("or was not in use at the time",
            "a completion against a deactivated EPA credits nothing too (T158), and the warning is how it is explained");
        warning.TextContent.Should().Contain("or the encounter is dated after the trainee's programme ended.",
            "an encounter after the programme's last day credits nothing on it (T281), and this warning is how it is explained");
        cut.Markup.Should().Contain("If it should have counted, raise it with the programme administrator.",
            "zero credit on a retired EPA is intended, so the warning must not call every such record a fault");
        cut.Markup.Should().NotContain("so it can be credited correctly");
        cut.Markup.Should().Contain("None", "the history row spells out the same thing");
    }

    [Fact]
    public void ACompletionThatCredited_IsNotFlagged()
    {
        var cut = RenderPage(Completion(creditedItemCount: 2));

        cut.FindAll(".field-warning").Should().BeEmpty();
        cut.Markup.Should().Contain("2 items");
    }

    [Fact]
    public void ATypeThatCreditsNothingByDesign_IsNeverFlagged()
    {
        // A reflective note, a journal club entry, a procedure log: credit was never evaluated, so the
        // stamp is null and there is nothing to warn about. This is also the state of every
        // transition recorded before the outcome started being stamped.
        var cut = RenderPage(Completion(creditedItemCount: null));

        cut.FindAll(".field-warning").Should().BeEmpty();
        cut.Markup.Should().NotContain("counted towards no curriculum requirement");
    }

    [Fact]
    public void ACompletionRefusedAcrossScales_IsFlagged()
    {
        // T109. The encounter counted towards volume, so the T108 "credited nothing" banner is silent —
        // and without this one the refusal of the supervision level would appear nowhere at all, leaving
        // an unexplained shortfall on the progress page as the only clue.
        var cut = RenderPage(Completion(creditedItemCount: 1, creditScaleMismatchCount: 1));

        CreditWarning(cut).HasAttribute("role").Should().BeFalse("standing page content (T193)");
        cut.Markup.Should().Contain("different entrustment scale");
        cut.Markup.Should().NotContain("counted towards no curriculum requirement",
            "volume did count — this is a different failure from T108's");
    }

    [Fact]
    public void ACompletionCreditedOnTheSameScale_IsNotFlagged()
    {
        var cut = RenderPage(Completion(creditedItemCount: 1, creditScaleMismatchCount: 0));

        cut.FindAll(".field-warning").Should().BeEmpty();
        cut.Markup.Should().NotContain("different entrustment scale");
    }

    [Fact]
    public void ACompletionWhereCreditWasNeverEvaluated_IsNotFlaggedAcrossScales()
    {
        // Null means "not evaluated", which is the permanent state of every pre-T109 row. It is not a
        // warning, and treating it as one would light the banner on the entire existing history.
        var cut = RenderPage(Completion(creditedItemCount: null, creditScaleMismatchCount: null));

        cut.Markup.Should().NotContain("different entrustment scale");
    }

    // C13: the warning sits under About's Credit value.
    private static AngleSharp.Dom.IElement CreditWarning(IRenderedComponent<ActivityView> cut)
    {
        var credit = cut.FindAll(".activity-about .details-list > div")
            .Single(row => row.QuerySelector("dt")!.TextContent.Trim() == "Credit");
        return credit.QuerySelectorAll("dd .field-warning").Should().ContainSingle().Subject;
    }

    [Fact]
    public void ACompletionOnAPausedEpa_SaysItsCreditWaits()
    {
        // D48, T196: an EPA deactivated after the completion pauses its credit; About says so (C13). The card's sentence
        // keys on the credit and the pause together (T355, note 2): one that credited nothing waits; one credited before
        // the pause says what it credited, as Home's Recent decisions row does.
        var detail = Completion(creditedItemCount: 0) with { EpaCode = "PAED-001", EpaTitle = "Emergency care", EpaInForce = false };
        var cut = RenderPage(detail);

        cut.Find(".activity-status-body").TextContent.Should().Contain("Its credit to PAED-001 waits while the EPA is paused.");
    }

    [Fact]
    public void ACompletionCreditedBeforeItsEpaWasPaused_IsFlaggedInAbout_AndTheCardSaysWhatItCredited()
    {
        var cut = RenderPage(Completion(creditedItemCount: 1) with { EpaCode = "PAED-001", EpaTitle = "Emergency care", EpaInForce = false });

        CreditWarning(cut).TextContent.Trim().Should().Be("This activity's EPA is paused: its credit waits.");
        cut.Find(".activity-status-body").TextContent.Should().Contain("Credited 1 item to PAED-001.")
            .And.NotContain("waits", "note 2: a completion credited before the pause does not read as waiting");

        // T231: About names the paused EPA as every other EPA surface does, with the marker in its own muted span.
        var epa = cut.FindAll(".activity-about .details-list > div")
            .Single(row => row.QuerySelector("dt")!.TextContent.Trim() == "EPA");
        epa.QuerySelector("dd .muted")!.TextContent.Should().Be("(no longer in use)");
        epa.QuerySelector("dd")!.TextContent.Should().Contain("PAED-001 — Emergency care (no longer in use)");
    }

    private IRenderedComponent<ActivityView> RenderPage(ActivityDetailDto detail)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(detail));

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 11));
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));

        return cut;
    }

    private static ActivityDetailDto Completion(int? creditedItemCount, int? creditScaleMismatchCount = null)
    {
        var transition = new ActivityTransitionDto(
            1,
            "requested",
            "completed",
            "complete",
            "Requested",
            "Completed",
            "Complete",
            "assessor-1",
            new DateTime(2026, 9, 17, 9, 30, 0, DateTimeKind.Utc),
            null,
            "{}",
            creditedItemCount,
            creditScaleMismatchCount,
            null);

        var activity = new ActivityDto(
            11,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            "mini_cex",
            1,
            SchemaJson,
            WorkflowJson,
            "[]",
            """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""",
            "trainee-1",
            1,
            "trainee-1",
            "completed",
            "Completed",
            """{"epa_id":"17"}""",
            null,
            null,
            new DateOnly(2026, 9, 17),
            true,
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

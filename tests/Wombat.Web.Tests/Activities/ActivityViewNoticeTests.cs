using System.Security.Claims;
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
/// T127: <c>/activities/{id}</c> shows, once, the notice <c>/activities/new</c> left for the activity it just created.
/// </summary>
/// <remarks>
/// The notice is how a refused submit says that the draft exists and what to fix, so it has to reach the page. It must
/// also not outlive what it describes: it is taken on arrival, so a later load does not show it again, and it goes as
/// soon as the actor makes another move, whose own outcome is what the page then reports.
/// </remarks>
public sealed class ActivityViewNoticeTests : TestContext
{
    private const int ActivityId = 7;

    private const string NoticeText =
        "Saved as a draft, but not submitted: setting: Setting is required. Fix the fields below and submit again.";

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
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "completed", "actor": "subject|creator", "validation": "owned" }
          ]
        }
        """;

    private readonly ViewSender _sender = new();

    public ActivityViewNoticeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddScoped<ActivityNotices>();
    }

    private ActivityNotices Notices => Services.GetRequiredService<ActivityNotices>();

    [Fact]
    public void APostedNotice_IsShownAboveTheActivity_AsTheKindItWasPostedAs()
    {
        Notices.Post(ActivityId, "warning", NoticeText);

        var cut = RenderPage();

        cut.Find(".alert-warning").TextContent.Trim().Should().Be(NoticeText);
        cut.Markup.IndexOf(NoticeText, StringComparison.Ordinal).Should().BeLessThan(
            cut.Markup.IndexOf("details-grid", StringComparison.Ordinal),
            "\"Fix the fields below\" has to be above the fields");
    }

    [Fact]
    public void ARefusedSubmit_IsAnAlert_SoItIsReadAsThePageArrives()
    {
        // T193. The notice arrives with the page, already filled. A status region that arrives filled is often not read;
        // an alert is, and this one is a refusal the author has to act on.
        Notices.Post(ActivityId, "warning", NoticeText);

        var cut = RenderPage();

        cut.Find(".alert-warning").GetAttribute("role").Should().Be("alert");
    }

    [Fact]
    public void ASuccessfulSubmit_IsAStatus()
    {
        Notices.Post(ActivityId, "success", "Submitted. It is now Requested.");

        var cut = RenderPage();

        cut.Find(".alert-success").GetAttribute("role").Should().Be("status");
    }

    [Fact]
    public void ANoticeForAnActivityThatDoesNotLoad_IsTakenButNotShown()
    {
        // Null is "no such activity" and "not yours" alike (T101). Nothing is said about it beyond the empty state.
        Notices.Post(ActivityId, "success", "Submitted. It is now Requested.");
        _sender.Missing = true;

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, ActivityId));
        cut.WaitForState(() => cut.Markup.Contains("Activity unavailable"));

        cut.Markup.Should().NotContain("Submitted. It is now Requested.");
        Notices.Take(ActivityId).Should().BeNull("it was taken, so it cannot surface on a later visit");
    }

    [Fact]
    public void LoadingAnotherActivity_ClearsTheNoticeAtOnce()
    {
        // The page is reused when only the id in the address changes. While the next activity loads, the notice about
        // the last one must not sit above it.
        Notices.Post(ActivityId, "warning", NoticeText);
        var cut = RenderPage();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _sender.LoadGate = release.Task;

        cut.SetParametersAndRender(parameters => parameters.Add(page => page.ActivityId, ActivityId + 1));

        cut.Markup.Should().NotContain(NoticeText, "the load of the next activity is still running");
        release.SetResult();
        cut.WaitForState(() => _sender.Loads == 2 && cut.Markup.Contains("Activity details"));
        cut.Markup.Should().NotContain(NoticeText);
    }

    [Fact]
    public void TheNoticeIsShownOnce_ALaterLoadDoesNotRepeatIt()
    {
        Notices.Post(ActivityId, "success", "Submitted. It is now Requested.");

        var cut = RenderPage();
        cut.Markup.Should().Contain("Submitted. It is now Requested.");
        Notices.Take(ActivityId).Should().BeNull("arriving took it");

        cut.SetParametersAndRender(parameters => parameters.Add(page => page.ActivityId, ActivityId));
        cut.WaitForState(() => _sender.Loads == 2);

        cut.Markup.Should().NotContain("Submitted. It is now Requested.");
    }

    [Fact]
    public void TypingDoesNotClearTheNotice()
    {
        // "Fix the fields below" has to stay in view while the fields are fixed. Only a load or a move replaces it.
        Notices.Post(ActivityId, "warning", NoticeText);
        var cut = RenderPage();

        cut.Find("#epa_id").Input("12");

        cut.Find(".alert-warning").TextContent.Trim().Should().Be(NoticeText);
    }

    [Fact]
    public void AnotherActivitysNotice_IsNeitherShownNorTaken()
    {
        Notices.Post(ActivityId + 1, "success", "Draft saved. It has not been submitted.");

        var cut = RenderPage();

        cut.Markup.Should().NotContain("Draft saved.");
        Notices.Take(ActivityId + 1).Should().NotBeNull("it is still waiting for its own activity");
    }

    [Fact]
    public void ANewMove_ClearsTheNotice_SoARefusalStandsAlone()
    {
        Notices.Post(ActivityId, "warning", NoticeText);
        _sender.TransitionFailure = new InvalidOperationException("EPA: A value is required.");
        var cut = RenderPage();

        Click(cut, "Submit");
        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Should().Contain("EPA: A value is required."));

        cut.FindAll(".alert-warning").Should().BeEmpty("the notice described the page as it arrived, not this refusal");
    }

    [Fact]
    public void ANewMove_ClearsTheNotice_WhenTheMoveSucceedsToo()
    {
        Notices.Post(ActivityId, "warning", NoticeText);
        var cut = RenderPage();

        Click(cut, "Submit");
        cut.WaitForState(() => _sender.Transitions == 1 && _sender.Loads == 2);

        cut.Markup.Should().NotContain(NoticeText, "a stale \"not submitted\" must not sit above a submit that worked");
    }

    private IRenderedComponent<ActivityView> RenderPage()
    {
        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, ActivityId));
        cut.WaitForState(() => cut.Markup.Contains("Activity details"));

        return cut;
    }

    private static void Click(IRenderedComponent<ActivityView> cut, string label)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label).Click();

    private sealed class ViewSender : IScopedSender
    {
        public int Loads { get; private set; }

        public int Transitions { get; private set; }

        public Exception? TransitionFailure { get; set; }

        /// <summary>When set, a load answers only once this completes, so a test can look at the page mid-load.</summary>
        public Task? LoadGate { get; set; }

        /// <summary>When set, a load answers null: no such activity, or not the reader's.</summary>
        public bool Missing { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    Loads++;
                    if (Missing)
                    {
                        return Task.FromResult((TResponse)(object)null!);
                    }

                    return LoadGate is null
                        ? Task.FromResult((TResponse)(object)Detail())
                        : (Task<TResponse>)(object)LoadedOnceReleasedAsync(LoadGate);

                case TransitionActivityCommand:
                    Transitions++;
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    return Task.FromResult((TResponse)(object)Detail().Activity);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private static async Task<ActivityDetailDto?> LoadedOnceReleasedAsync(Task gate)
        {
            await gate;
            return Detail();
        }

        private static ActivityDetailDto Detail()
        {
            var activity = new ActivityDto(
                ActivityId,
                2,
                "mini_cex_cpsa",
                "Mini-CEX (CPSA)",
                "mini_cex",
                1,
                SchemaJson,
                WorkflowJson,
                "[]",
                """{ "counts_for": [] }""",
                "trainee-1",
                7,
                "trainee-1",
                "draft",
                "{}",
                null,
                null,
                new DateOnly(2026, 9, 24),
                true,
                new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
                []);

            return new ActivityDetailDto(activity, ["epa_id"], [new ActivityActionDto("submit", false)]);
        }
    }
}

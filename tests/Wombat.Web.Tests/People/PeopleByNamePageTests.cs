using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using EntrustmentDecisionsPage = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;

namespace Wombat.Web.Tests.People;

/// <summary>
/// The committee, entrustment and MSF pages name the trainee they are about. Each printed a user id in a
/// <c>&lt;code&gt;</c> cell. (T142)
/// </summary>
/// <remarks>
/// Each page prints the name its query resolved: nothing here looks a name up, and each fake answers with a DTO whose id
/// and name differ, so a page that printed the id again would show it.
/// </remarks>
public sealed class PeopleByNamePageTests : TestContext
{
    private readonly RecordingSender _sender = new();

    public PeopleByNamePageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("staff@test");
        auth.SetRoles("Administrator");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheEntrustmentDecisionList_NamesEachTrainee_AndSoDoesItsRevokeConfirmation()
    {
        _sender.On<ListEntrustmentDecisionsForAdminQuery>(_ => new[]
        {
            Decision(1, "trainee-1", "Thandi Nkosi"),
            Decision(2, "departed-trainee", "departed-trainee")
        });

        var cut = RenderComponent<EntrustmentDecisionsPage>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);

        Column(cut, "Trainee").Should().Equal("Thandi Nkosi", "departed-trainee");

        cut.FindAll("button").First(button => button.TextContent.Trim() == "Revoke").Click();

        cut.Find("section h3 + p").TextContent.Should().Contain("held by Thandi Nkosi.");
        cut.Markup.Should().NotContain("trainee-1");
    }

    /// <summary>
    /// The list shows names and no ids, so its trainee filter asks for a name. It asked for a user id, which after T142
    /// nothing on the page gives.
    /// </summary>
    [Fact]
    public void TheEntrustmentDecisionList_FiltersByWhatItShows_AName()
    {
        _sender.On<ListEntrustmentDecisionsForAdminQuery>(_ => new[] { Decision(1, "trainee-1", "Thandi Nkosi") });

        var cut = RenderComponent<EntrustmentDecisionsPage>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        cut.Find("label[for=filter-trainee]").TextContent.Should().NotContain("id");
        cut.Find("#filter-trainee").GetAttribute("placeholder").Should().Be("Filter by name");

        cut.Find("#filter-trainee").Input("thandi");
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Apply filters").Click();

        _sender.Received.OfType<ListEntrustmentDecisionsForAdminQuery>().Last().TraineeFilter.Should().Be("thandi");
    }

    [Fact]
    public void TheReviewSchedule_NamesEachTrainee()
    {
        _sender
            .On<GetCommitteeReviewsAccessQuery>(_ => new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null))
            .On<ListDecisionPanelsQuery>(_ => Array.Empty<DecisionPanelSummaryDto>())
            .On<ListReviewsForPanelQuery>(_ => new[] { ListItem(30, "trainee-1", "Thandi Nkosi") });

        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        Column(cut, "Trainee").Should().Equal("Thandi Nkosi");
        cut.Markup.Should().NotContain("trainee-1");
    }

    [Fact]
    public void TheCampaignList_NamesEachSubject()
    {
        _sender.On<ListMsfCampaignsForCoordinatorQuery>(_ => new[] { Campaign(5, "trainee-1", "Thandi Nkosi") });

        var cut = RenderComponent<CampaignsList>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        Column(cut, "Subject").Should().Equal("Thandi Nkosi");
        cut.Markup.Should().NotContain("trainee-1");
    }

    [Fact]
    public void TheReviewPage_NamesTheTrainee()
    {
        AnswerTheReviewPage(Review(CommitteeReviewState.Scheduled) with { TraineeName = "Thandi Nkosi" });

        var cut = RenderReview();

        Trainee(cut).Should().Be("Thandi Nkosi");
    }

    /// <summary>
    /// The commands answer with the review as the shared mapper builds it, which names nobody. The name the page loaded
    /// with carries over, so it does not vanish the moment the panel acts on the review.
    /// </summary>
    [Fact]
    public void TheReviewPage_StillNamesTheTrainee_AfterAnAction()
    {
        AnswerTheReviewPage(Review(CommitteeReviewState.Scheduled) with { TraineeName = "Thandi Nkosi" });
        _sender.On<StartCommitteeReviewCommand>(_ => Review(CommitteeReviewState.InProgress));

        var cut = RenderReview();
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Start review").Click();
        cut.WaitForState(() => cut.Markup.Contains("Review started."));

        Trainee(cut).Should().Be("Thandi Nkosi");
        cut.Markup.Should().NotContain("trainee-1");
    }

    /// <summary>
    /// An action that succeeded is shown as succeeded, with the state its command returned, whatever a later read of
    /// the review would have done. The page once read the review back after the success message: when that read failed
    /// it showed success and an error together, and kept offering "Start review" on a review already started.
    /// </summary>
    [Fact]
    public void TheReviewPage_AfterAnAction_ShowsTheCommandsResult_WithoutReadingTheReviewAgain()
    {
        var reads = 0;
        _sender
            .On<GetCommitteeReviewByIdQuery>(_ => ++reads == 1
                ? Review(CommitteeReviewState.Scheduled) with { TraineeName = "Thandi Nkosi" }
                : throw new InvalidOperationException("The database went away."));
        AnswerTheReviewPageExceptTheReview();
        _sender.On<StartCommitteeReviewCommand>(_ => Review(CommitteeReviewState.InProgress));

        var cut = RenderReview();
        cut.FindAll("button").First(button => button.TextContent.Trim() == "Start review").Click();
        cut.WaitForState(() => cut.Markup.Contains("Review started."));

        cut.FindAll(".alert-danger").Should().BeEmpty("the action succeeded");
        cut.FindAll("button").Select(button => button.TextContent.Trim()).Should().NotContain(
            "Start review", "the review is in progress now, as the command said");
        Trainee(cut).Should().Be("Thandi Nkosi");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------------

    private void AnswerTheReviewPage(CommitteeReviewDetailDto review)
    {
        _sender.On<GetCommitteeReviewByIdQuery>(_ => review);
        AnswerTheReviewPageExceptTheReview();
    }

    private void AnswerTheReviewPageExceptTheReview()
    {
        _sender
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null)
            .On<ListStarEpaOptionsForReviewQuery>(_ => Array.Empty<StarEpaOptionDto>())
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>());
    }

    private IRenderedComponent<ReviewDetail> RenderReview()
    {
        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));
        return cut;
    }

    private static string Trainee(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("dt").Single(term => term.TextContent.Trim() == "Trainee").NextElementSibling!.TextContent.Trim();

    /// <summary>The text of one column, found by its header, so a reordered table cannot pass by accident.</summary>
    private static IReadOnlyList<string> Column<T>(IRenderedComponent<T> cut, string header)
        where T : Microsoft.AspNetCore.Components.IComponent
    {
        var index = cut.FindAll("thead th").Select(cell => cell.TextContent.Trim()).ToList().IndexOf(header);
        index.Should().BeGreaterThanOrEqualTo(0, "the table must have a '{0}' column", header);

        return cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll("td")[index].TextContent.Trim()).ToList();
    }

    private static EntrustmentDecisionDto Decision(int id, string traineeUserId, string traineeName) => new(
        id,
        traineeUserId,
        7,
        "PAED-001",
        "Resuscitate a critically ill child",
        3,
        "Level 3",
        3,
        new DateOnly(2026, 4, 1),
        null,
        30,
        "chair-1",
        "Ratified at the annual review.",
        EntrustmentDecisionStatus.Active,
        null,
        null,
        null,
        null,
        [])
    {
        TraineeName = traineeName
    };

    private static CommitteeReviewListItemDto ListItem(int id, string traineeUserId, string traineeName) => new(
        id,
        traineeUserId,
        20,
        "Paediatrics CCC",
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 6, 30),
        new DateOnly(2026, 7, 2),
        CommitteeReviewState.Scheduled,
        null,
        null)
    {
        AcademicYear = 2026,
        Semester = 1,
        TraineeName = traineeName
    };

    private static MsfCampaignSummaryDto Campaign(int id, string subjectUserId, string subjectName) => new(
        id,
        subjectUserId,
        "Annual MSF",
        new DateOnly(2026, 9, 1),
        new DateOnly(2026, 9, 15),
        5,
        3,
        MsfCampaignState.Open,
        8,
        2,
        null)
    {
        SubjectName = subjectName
    };

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state) => new(
        30,
        "trainee-1",
        20,
        "Paediatrics CCC",
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 6, 30),
        new DateOnly(2026, 7, 2),
        state,
        null,
        null,
        null,
        null,
        null,
        [],
        [],
        [])
        {
            AcademicYear = 2026,
            Semester = 1,
            // The reader works on the panel, so is offered Start (T194).
            CallerMayStart = true
        };

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)Answer(request)!);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Answer(request);
            return Task.CompletedTask;
        }

        private object? Answer(object request)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}

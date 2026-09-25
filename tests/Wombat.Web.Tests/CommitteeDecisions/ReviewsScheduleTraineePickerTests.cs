using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The scheduling page offers, for the chosen panel, exactly the trainees the scheduling handler would accept, and
/// schedules only one it offered. (T182)
/// </summary>
/// <remarks>
/// Before T182 the trainee was a free-text user id, so the page would send any id at all and the handler checked none.
/// The handler is the gate now; the page asks it who passes (<see cref="ListSchedulableTraineesQuery" />) and offers
/// that list, which follows the panel because a panel reviews its own institution's trainees.
/// </remarks>
public sealed class ReviewsScheduleTraineePickerTests : TestContext
{
    private const string CoordinatorUserId = "coordinator-a";
    private const int PanelA = 10;
    private const int PanelB = 20;

    private readonly RecordingSender _sender = new();

    public ReviewsScheduleTraineePickerTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{CoordinatorUserId}@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId));

        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<ListDecisionPanelsQuery>(_ => new[]
            {
                new DecisionPanelSummaryDto(PanelA, "A's annual review panel", DecisionPanelScope.Institution, 1, null, 3),
                new DecisionPanelSummaryDto(PanelB, "A's second panel", DecisionPanelScope.Institution, 1, null, 3)
            })
            .On<GetCommitteeReviewsAccessQuery>(_ => new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null))
            .On<ListReviewsForPanelQuery>(_ => Array.Empty<CommitteeReviewListItemDto>())
            .On<ListSchedulableTraineesQuery>(query => query.PanelId switch
            {
                PanelA => new[] { new SchedulableTraineeDto("paeds-a", "Palesa Paeds") },
                PanelB => new[] { new SchedulableTraineeDto("neo-a", "Nandi Neonatal") },
                _ => Array.Empty<SchedulableTraineeDto>()
            })
            .On<ScheduleCommitteeReviewCommand>(command => new CommitteeReviewListItemDto(
                77, command.TraineeUserId, command.PanelId, "A's annual review panel",
                command.ReviewPeriodFrom, command.ReviewPeriodTo, command.ScheduledOn,
                CommitteeReviewState.Scheduled, null, null)
        {
            AcademicYear = 2026,
            Semester = 1
        });
    }

    [Fact]
    public void TheTrainee_IsAPicker_NotAFreeTextId_AndWaitsForAPanel()
    {
        var cut = RenderForm();

        cut.FindAll("input#review-trainee").Should().BeEmpty("the trainee is chosen from the handler's list, never typed");
        var trainee = cut.Find("select#review-trainee");
        trainee.HasAttribute("disabled").Should().BeTrue();
        OptionValues(cut).Should().Equal(string.Empty);
        cut.Markup.Should().Contain("Choose a panel first.");
        _sender.Received.OfType<ListSchedulableTraineesQuery>().Should().BeEmpty();
    }

    [Fact]
    public void ChoosingAPanel_OffersExactlyTheTraineesTheHandlerAccepts_AskedAsTheSignedInCaller()
    {
        var cut = RenderForm();

        cut.Find("#review-panel").Change(PanelA.ToString());
        cut.WaitForState(() => OptionValues(cut).Count > 1);

        OptionValues(cut).Should().Equal(string.Empty, "paeds-a");
        cut.Find("select#review-trainee option[value='paeds-a']").TextContent.Should().Be("Palesa Paeds");
        cut.Find("select#review-trainee").HasAttribute("disabled").Should().BeFalse();

        var query = _sender.Received.OfType<ListSchedulableTraineesQuery>().Should().ContainSingle().Which;
        query.PanelId.Should().Be(PanelA);
        CallerOf(query.Principal).Should().Be(CoordinatorUserId);
    }

    [Fact]
    public void ChangingThePanel_ReplacesTheList_AndDropsTheChosenTrainee()
    {
        var cut = RenderForm();
        cut.Find("#review-panel").Change(PanelA.ToString());
        cut.WaitForState(() => OptionValues(cut).Contains("paeds-a"));
        cut.Find("#review-trainee").Change("paeds-a");

        cut.Find("#review-panel").Change(PanelB.ToString());
        cut.WaitForState(() => OptionValues(cut).Contains("neo-a"));

        OptionValues(cut).Should().Equal(string.Empty, "neo-a");
        cut.Find("form").Submit();
        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().BeEmpty(
            "the trainee chosen for panel A is not carried over to panel B");
    }

    [Fact]
    public void APanelWithNoOneToOffer_SaysSo()
    {
        _sender.On<ListSchedulableTraineesQuery>(_ => Array.Empty<SchedulableTraineeDto>());
        var cut = RenderForm();

        cut.Find("#review-panel").Change(PanelA.ToString());
        cut.WaitForState(() => cut.Markup.Contains("This panel has no trainees you can schedule a review for."));

        OptionValues(cut).Should().Equal(string.Empty);
    }

    [Fact]
    public void AValueThePickerNeverOffered_IsNotSent()
    {
        // The select's value arrives from the browser and can be edited there. The handler would refuse it anyway;
        // the page does not send it.
        var cut = RenderForm();
        cut.Find("#review-panel").Change(PanelA.ToString());
        cut.WaitForState(() => OptionValues(cut).Contains("paeds-a"));

        cut.Find("#review-trainee").Change("paeds-b");
        cut.Find("form").Submit();

        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().BeEmpty();
        cut.Markup.Should().Contain("Choose a trainee from the list.");
    }

    [Fact]
    public void AnOfferedTrainee_IsScheduledOnTheChosenPanel()
    {
        var cut = RenderForm();
        cut.Find("#review-panel").Change(PanelA.ToString());
        cut.WaitForState(() => OptionValues(cut).Contains("paeds-a"));

        cut.Find("#review-trainee").Change("paeds-a");
        cut.Find("form").Submit();

        var command = _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().ContainSingle().Which;
        command.TraineeUserId.Should().Be("paeds-a");
        command.PanelId.Should().Be(PanelA);
        CallerOf(command.Principal).Should().Be(CoordinatorUserId);
    }

    private IRenderedComponent<ReviewsSchedule> RenderForm()
    {
        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.Markup.Contains("No reviews yet"));
        cut.Find("button.btn-primary").Click();
        cut.WaitForState(() => cut.FindAll("#review-panel").Count == 1);
        return cut;
    }

    private static string? CallerOf(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static IReadOnlyList<string> OptionValues(IRenderedComponent<ReviewsSchedule> cut)
        => cut.FindAll("select#review-trainee option")
            .Select(option => option.GetAttribute("value") ?? string.Empty)
            .ToArray();

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

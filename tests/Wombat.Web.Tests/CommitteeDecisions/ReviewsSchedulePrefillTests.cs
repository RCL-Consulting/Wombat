using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The decisions-due page's Schedule link opens the scheduling form filled with the panel, the trainee and the period
/// (T131 slice 6). Each is taken only where the page offers it: the link is a convenience, never an authorization.
/// </summary>
public sealed class ReviewsSchedulePrefillTests : TestContext
{
    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;

    private readonly PrefillSender _sender = new();
    private readonly CommitteeReviewPeriodOptionDto _earliest = CommitteeReviewPeriods.Around(QuotaCalendar.Today())[0];

    public ReviewsSchedulePrefillTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-a"));

        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void TheLink_OpensTheFormWithThePanelTraineeAndPeriod_AndPreviewsTheAgenda()
    {
        Navigate(GeneralPanel, "paeds-a", _earliest.Key);

        var cut = RenderComponent<ReviewsSchedule>();

        cut.Find("#review-panel").GetAttribute("value").Should().Be(GeneralPanel.ToString());
        cut.Find("#review-trainee").GetAttribute("value").Should().Be("paeds-a");
        cut.Find("#review-period").GetAttribute("value").Should().Be(_earliest.Key);
        cut.Find("#review-from").GetAttribute("value").Should().Be(_earliest.WindowFrom.ToString("yyyy-MM-dd"));
        var preview = _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Should().ContainSingle().Subject;
        (preview.PanelId, preview.TraineeUserId, preview.AcademicYear, preview.Semester)
            .Should().Be((GeneralPanel, "paeds-a", _earliest.AcademicYear, _earliest.Semester));
    }

    [Fact]
    public void BeforeACollegeCommittee_TheReviewTypeIsItsDefault()
    {
        Navigate(NeonatalPanel, "paeds-a", _earliest.Key);

        var cut = RenderComponent<ReviewsSchedule>();

        cut.Find("#review-type").GetAttribute("value").Should().Be(nameof(CommitteeReviewType.EntrustmentOnly));
    }

    [Fact]
    public void ATraineeThePanelDoesNotOffer_IsNotChosen()
    {
        Navigate(GeneralPanel, "someone-else", _earliest.Key);

        var cut = RenderComponent<ReviewsSchedule>();

        cut.Find("#review-panel").GetAttribute("value").Should().Be(GeneralPanel.ToString());
        cut.Find("#review-trainee").GetAttribute("value").Should().BeNullOrEmpty();
        _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Should().BeEmpty();
    }

    [Fact]
    public void APeriodNotOffered_LeavesTheCurrentOne()
    {
        Navigate(GeneralPanel, "paeds-a", "1999-1");

        var cut = RenderComponent<ReviewsSchedule>();

        cut.Find("#review-period").GetAttribute("value").Should().Be(CommitteeReviewPeriods.Current(QuotaCalendar.Today()).Key);
    }

    [Fact]
    public void APanelNotListed_LeavesTheFormClosed()
    {
        Navigate(99, "paeds-a", _earliest.Key);

        var cut = RenderComponent<ReviewsSchedule>();

        cut.FindAll("#review-panel").Should().BeEmpty();
        _sender.Received.OfType<ListSchedulableTraineesQuery>().Should().BeEmpty();
    }

    private void Navigate(int panelId, string traineeUserId, string periodKey)
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(
            $"/committee/reviews?panel={panelId}&trainee={Uri.EscapeDataString(traineeUserId)}&period={periodKey}");
    }

    /// <summary>Answers the scheduling page's queries, and records every request.</summary>
    private sealed class PrefillSender : IScopedSender
    {
        public List<object> Received { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            object answer = request switch
            {
                ListDecisionPanelsQuery => new[]
                {
                    new DecisionPanelSummaryDto(GeneralPanel, "General CCC", DecisionPanelScope.Institution, 1, null, 3),
                    new DecisionPanelSummaryDto(
                        NeonatalPanel, "Neonatal CCC", DecisionPanelScope.Institution, 1, null, 2, "neonatal",
                        "Neonatal team Clinical Competency Committee")
                },
                ListReviewsForPanelQuery => Array.Empty<CommitteeReviewListItemDto>(),
                ListSchedulableTraineesQuery => new[] { new SchedulableTraineeDto("paeds-a", "Palesa Paeds") },
                PreviewCommitteeAgendaQuery query => new CommitteeAgendaPreviewDto(
                    query.AcademicYear, query.Semester, $"{query.AcademicYear} S{query.Semester}", true, [], [], []),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)answer);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

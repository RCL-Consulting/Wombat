using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Decisions due's Schedule opens the schedule page (T364, Q1: <c>/committee/reviews/new</c>) filled with the panel, the
/// trainee and the row's own period (T131 slice 6; F1). Each is taken only where the page offers it: the link is a
/// convenience, never an authorization. Cancel goes back where the link came from, read only through
/// <c>ReviewLinks.ScheduleReturn</c> (C4).
/// </summary>
public sealed class ReviewsSchedulePrefillTests : TestContext
{
    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;

    /// <summary>2026-10-13 10:00 in South Africa: 2026 S2, and 2026 S1 has ended.</summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 13, 8, 0, 0, TimeSpan.Zero);

    private readonly PrefillSender _sender = new();

    public ReviewsSchedulePrefillTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-a"));

        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(Now));
    }

    [Fact]
    public void TheLink_OpensTheFormWithThePanelTraineeAndPeriod_AndPreviewsTheAgenda()
    {
        Navigate(GeneralPanel, "paeds-a", "2026-2");

        var cut = RenderForm();

        cut.Find("#review-panel").GetAttribute("value").Should().Be(GeneralPanel.ToString());
        cut.Find("#review-trainee").GetAttribute("value").Should().Be("paeds-a");
        cut.Find("#review-period").GetAttribute("value").Should().Be("2026-2");
        cut.Find("#review-from").GetAttribute("value").Should().Be("2026-01-01");
        cut.Find("#review-to").GetAttribute("value").Should().Be("2026-12-31");
        var preview = _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Should().ContainSingle().Subject;
        (preview.PanelId, preview.TraineeUserId, preview.AcademicYear, preview.Semester)
            .Should().Be((GeneralPanel, "paeds-a", 2026, 2));
        var seat = _sender.Received.OfType<GetSchedulingSeatQuery>().Should().ContainSingle().Subject;
        (seat.PanelId, seat.TraineeUserId, seat.AcademicYear, seat.Semester).Should().Be((GeneralPanel, "paeds-a", 2026, 2));
    }

    [Fact]
    public void AnEndedPeriod_IsTaken_AndSaysSoUnderPeriod()
    {
        // F1: a Missed row of 2026 S1 opens the form on its own ended semester, the only review that clears it.
        Navigate(GeneralPanel, "paeds-a", "2026-1");

        var cut = RenderForm();

        cut.Find("#review-period").GetAttribute("value").Should().Be("2026-1");
        cut.Find("#review-period-help").TextContent.Should().StartWith(
            "2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends. The semester " +
            "the committee decides for.");
        cut.FindAll("#review-period-note").Should().BeEmpty("semester 2's window line is said only for semester 2 (F6)");
        _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Single().Semester.Should().Be(1);
    }

    [Fact]
    public void ASemester2Period_SaysItsWindow_AndThePeriodNamesIt()
    {
        Navigate(GeneralPanel, "paeds-a", "2026-2");

        var cut = RenderForm();

        cut.Find("#review-period-help").TextContent.Should().NotContain("has ended");
        cut.Find("#review-period-note").TextContent.Should().Be(
            "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.");
        cut.Find("#review-period").GetAttribute("aria-describedby").Should().Be("review-period-help review-period-note");
    }

    [Fact]
    public void BeforeACollegeCommittee_TheReviewTypeIsItsOneBindingType_Preselected()
    {
        Navigate(NeonatalPanel, "paeds-a", "2026-2");

        var cut = RenderForm();

        TypeValues(cut).Should().Equal(nameof(CommitteeReviewType.EntrustmentOnly), ScheduleReview.FormativeKey);
        cut.Find("#review-type-EntrustmentOnly").HasAttribute("checked").Should().BeTrue("F4: preselected");
        cut.Find("#review-type-EntrustmentOnly-help").TextContent.Should().Contain(
            "Neonatal CCC sits as the Neonatal team Clinical Competency Committee, which decides entrustment only");
    }

    [Fact]
    public void ATraineeThePanelDoesNotOffer_IsNotChosen()
    {
        Navigate(GeneralPanel, "someone-else", "2026-2");

        var cut = RenderForm();

        cut.Find("#review-panel").GetAttribute("value").Should().Be(GeneralPanel.ToString());
        cut.Find("#review-trainee").GetAttribute("value").Should().BeNullOrEmpty();
        _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Should().BeEmpty();
        _sender.Received.OfType<GetSchedulingSeatQuery>().Should().BeEmpty();
    }

    [Fact]
    public void APeriodNotOffered_LeavesThePeriodEmpty()
    {
        Navigate(GeneralPanel, "paeds-a", "1999-1");

        var cut = RenderForm();

        cut.Find("#review-period").GetAttribute("value").Should().BeNullOrEmpty("C4: nothing chosen for the reader");
        cut.Find("#review-from").GetAttribute("value").Should().BeNullOrEmpty();
        _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Should().BeEmpty();
    }

    [Fact]
    public void APanelNotListed_LeavesThePanelEmpty_AndTheTraineeWaiting()
    {
        Navigate(99, "paeds-a", "2026-2");

        var cut = RenderForm();

        cut.Find("#review-panel").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("#review-trainee").HasAttribute("disabled").Should().BeTrue();
        _sender.Received.OfType<ListSchedulableTraineesQuery>().Should().BeEmpty();
    }

    [Theory]
    [InlineData("/committee/decisions-due?trainee=paeds-a&status=all", "/committee/decisions-due?trainee=paeds-a&status=all")]
    [InlineData("/committee/reviews", "/committee/reviews")]
    [InlineData(null, "/committee/reviews")]
    [InlineData("https://elsewhere.example/committee/decisions-due", "/committee/reviews")]
    [InlineData("//elsewhere.example/x", "/committee/reviews")]
    [InlineData("/admin/users", "/committee/reviews")]
    public void Cancel_GoesBackWhereTheLinkCameFrom_AndNowhereElse(string? returnTo, string expected)
    {
        // C4: Decisions due with its filters, or Committee reviews; a forged return= goes to Committee reviews.
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            ReviewLinks.Schedule(GeneralPanel, "paeds-a", "2026-2", returnTo));

        var cut = RenderForm();

        cut.FindAll(".form-actions a").Single(link => link.TextContent.Trim() == "Cancel")
            .GetAttribute("href").Should().Be(expected);
    }

    private IRenderedComponent<ScheduleReview> RenderForm()
    {
        var cut = RenderComponent<ScheduleReview>();
        cut.WaitForState(() => cut.FindAll("#review-panel").Count == 1);
        return cut;
    }

    private static IReadOnlyList<string> TypeValues(IRenderedComponent<ScheduleReview> cut)
        => cut.FindAll("input[name='review-type']").Select(radio => radio.GetAttribute("value") ?? string.Empty).ToArray();

    private void Navigate(int panelId, string traineeUserId, string periodKey)
        => Services.GetRequiredService<NavigationManager>().NavigateTo(ReviewLinks.Schedule(panelId, traineeUserId, periodKey, null));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Answers the schedule page's queries, and records every request.</summary>
    private sealed class PrefillSender : IScopedSender
    {
        public List<object> Received { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);

            object? answer = request switch
            {
                ListDecisionPanelsQuery => new[]
                {
                    new DecisionPanelSummaryDto(GeneralPanel, "General CCC", DecisionPanelScope.Institution, 1, null, 3),
                    new DecisionPanelSummaryDto(
                        NeonatalPanel, "Neonatal CCC", DecisionPanelScope.Institution, 1, null, 2, "neonatal",
                        "Neonatal team Clinical Competency Committee")
                },
                GetCommitteeReviewsAccessQuery => new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null),
                ListSchedulableTraineesQuery => new[] { new SchedulableTraineeDto("paeds-a", "Palesa Paeds") },
                GetSchedulingSeatQuery => null,
                PreviewCommitteeAgendaQuery query => new CommitteeAgendaPreviewDto(
                    query.AcademicYear, query.Semester, $"{query.AcademicYear} S{query.Semester}", true, [], [], []),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

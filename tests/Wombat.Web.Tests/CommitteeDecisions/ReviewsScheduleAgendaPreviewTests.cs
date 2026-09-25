using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The scheduling form asks for the period the review sits for (T131, Decision 4): choosing it fills the evidence window,
/// which stays editable, and the form previews the agenda the planner will give the review.
/// </summary>
public sealed partial class ReviewsScheduleAgendaPreviewTests : TestContext
{
    private const int Panel = 10;

    private readonly RecordingSender _sender = new();
    private readonly CommitteeReviewPeriodOptionDto _current = CommitteeReviewPeriods.Current(QuotaCalendar.Today());

    public ReviewsScheduleAgendaPreviewTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-a"));

        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<ListDecisionPanelsQuery>(_ => new[]
            {
                new DecisionPanelSummaryDto(Panel, "General CCC", DecisionPanelScope.Institution, 1, null, 3)
            })
            .On<GetCommitteeReviewsAccessQuery>(_ => new CommitteeReviewsAccessDto(MaySchedule: true, TraineeNote: null))
            .On<ListReviewsForPanelQuery>(_ => Array.Empty<CommitteeReviewListItemDto>())
            .On<ListSchedulableTraineesQuery>(_ => new[] { new SchedulableTraineeDto("paeds-a", "Palesa Paeds") })
            .On<PreviewCommitteeAgendaQuery>(query => Preview(query.AcademicYear, query.Semester))
            .On<ScheduleCommitteeReviewCommand>(command => new CommitteeReviewListItemDto(
                77, command.TraineeUserId, command.PanelId, "General CCC",
                command.ReviewPeriodFrom, command.ReviewPeriodTo, command.ScheduledOn,
                CommitteeReviewState.Scheduled, null, null)
            {
                AcademicYear = command.AcademicYear,
                Semester = command.Semester
            });
    }

    [Fact]
    public void ThePeriod_DefaultsToTheCurrentOne_AndFillsTheEvidenceWindow()
    {
        var cut = RenderForm();

        cut.Find("#review-period").GetAttribute("value").Should().Be(_current.Key);
        cut.Find("#review-from").GetAttribute("value").Should().Be(_current.WindowFrom.ToString("yyyy-MM-dd"));
        cut.Find("#review-to").GetAttribute("value").Should().Be(_current.WindowTo.ToString("yyyy-MM-dd"));
        cut.FindAll("#review-period option").Select(option => option.GetAttribute("value"))
            .Should().Equal(CommitteeReviewPeriods.Around(QuotaCalendar.Today()).Select(period => period.Key));
    }

    [Fact]
    public void ChoosingAnotherPeriod_FillsItsWindow_WhichStaysEditable()
    {
        var cut = RenderForm();
        var earliest = CommitteeReviewPeriods.Around(QuotaCalendar.Today())[0];

        cut.Find("#review-period").Change(earliest.Key);

        cut.Find("#review-from").GetAttribute("value").Should().Be(earliest.WindowFrom.ToString("yyyy-MM-dd"));
        cut.Find("#review-to").GetAttribute("value").Should().Be(earliest.WindowTo.ToString("yyyy-MM-dd"));

        cut.Find("#review-from").Change(earliest.WindowFrom.AddDays(14).ToString("yyyy-MM-dd"));
        cut.Find("#review-from").GetAttribute("value").Should().Be(earliest.WindowFrom.AddDays(14).ToString("yyyy-MM-dd"));
    }

    [Fact]
    public void ThePreview_IsAskedForThePanelTraineeAndPeriod_AndSaysWhatTheReviewWillDecide()
    {
        var cut = RenderForm();

        ChoosePanelAndTrainee(cut);

        var query = _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Last();
        (query.TraineeUserId, query.PanelId, query.AcademicYear, query.Semester)
            .Should().Be(("paeds-a", Panel, _current.AcademicYear, _current.Semester));

        var preview = Text(cut.Find("#agenda-preview"));
        var period = $"{_current.AcademicYear} S{_current.Semester}";
        preview.Should().Contain($"2 EPAs will be on the agenda for {period}.")
            .And.Contain("PAED-001 must be decided at this sitting, or deferred with a reason, before it is ratified.")
            .And.Contain("PAED-003 — Paediatric ward round · Due by year end")
            .And.Contain("PAED-004 and PAED-005 are decided by Neonatal CCC: schedule them separately.")
            .And.Contain("Already decided in this window, so not on the agenda: PAED-006.");
    }

    [Fact]
    public void EpasAStarAlreadyDecided_AreNamedAlreadyDecidedInThisWindow_AndAreNotOnTheAgenda()
    {
        // T215: the planner leaves off an EPA a STAR already decided in its window, whether or not an agenda line records
        // that STAR, and the preview names it.
        _sender.On<PreviewCommitteeAgendaQuery>(query => Preview(query.AcademicYear, query.Semester) with
        {
            Lines = [Line(3, "PAED-003", "Paediatric ward round", closing: false, CommitteeAgendaLineStatus.DueByYearEnd, $"{query.AcademicYear}")],
            DecidedInWindow = ["PAED-001", "PAED-002", "PAED-006"]
        });
        var cut = RenderForm();

        ChoosePanelAndTrainee(cut);

        var preview = Text(cut.Find("#agenda-preview"));
        preview.Should().Contain($"1 EPA will be on the agenda for {_current.AcademicYear} S{_current.Semester}.")
            .And.Contain("Already decided in this window, so not on the agenda: PAED-001, PAED-002 and PAED-006.")
            .And.NotContain("must be decided at this sitting");
        cut.FindAll("#agenda-preview li").Select(item => Text(item)).Should().ContainSingle()
            .Which.Should().StartWith("PAED-003");
        Text(cut.Find("#agenda-preview-summary")).Should().NotContain("Already decided", "only the opening sentences are live");
    }

    [Fact]
    public void WhereAStarAlreadyDecidedEveryEpaDue_TheLiveSentenceSaysSo_NotThatNoneIsDue()
    {
        // T215 review: the note naming them is outside the live region, so the live sentence must not say the period asks
        // nothing of this panel.
        _sender.On<PreviewCommitteeAgendaQuery>(query => Preview(query.AcademicYear, query.Semester) with
        {
            Lines = [],
            DecidedInWindow = ["PAED-001", "PAED-002"]
        });
        var cut = RenderForm();
        var period = $"{_current.AcademicYear} S{_current.Semester}";

        ChoosePanelAndTrainee(cut, until: "already decided in its window");

        Text(cut.Find("#agenda-preview-summary")).Should().Be(
            $"Every EPA this panel decides that is due for {period} is already decided in its window, so the review will " +
            "have nothing on its agenda.");
        Text(cut.Find("#agenda-preview")).Should().Contain(
            "Already decided in this window, so not on the agenda: PAED-001 and PAED-002.");

        CommitteeAgendaText.PreviewSentences(new CommitteeAgendaPreviewDto(2026, 2, "2026 S2", true, [], [], []))
            .Should().Equal(["No EPA this panel decides is due for 2026 S2."], "with nothing decided, none is due");
    }

    [Fact]
    public void OnlyThePreviewsSummary_IsALiveRegion()
    {
        // A screen reader hears what changed when a choice changes the preview, not every optional line and note again.
        var cut = RenderForm();
        ChoosePanelAndTrainee(cut);

        var live = cut.FindAll("[role=status]").Should().ContainSingle().Subject;
        live.Id.Should().Be("agenda-preview-summary");
        Text(live).Should().Contain("will be on the agenda").And.Contain("must be decided at this sitting")
            .And.NotContain("Due by year end").And.NotContain("schedule them separately");
        cut.Find("#agenda-preview").HasAttribute("role").Should().BeFalse();
    }

    [Fact]
    public void AFormativeReview_PreviewsNoAgenda()
    {
        var cut = RenderForm();
        ChoosePanelAndTrainee(cut);

        cut.Find("#review-formative").Change(true);

        Text(cut.Find("#agenda-preview")).Should().Be("A formative review carries no agenda: it decides no EPA.");
    }

    [Fact]
    public void Scheduling_SendsTheChosenPeriod()
    {
        var cut = RenderForm();
        var earliest = CommitteeReviewPeriods.Around(QuotaCalendar.Today())[0];
        ChoosePanelAndTrainee(cut);
        cut.Find("#review-period").Change(earliest.Key);

        cut.Find("#review-panel").Closest("form")!.Submit();

        var command = _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().ContainSingle().Subject;
        (command.AcademicYear, command.Semester).Should().Be((earliest.AcademicYear, earliest.Semester));
        (command.ReviewPeriodFrom, command.ReviewPeriodTo).Should().Be((earliest.WindowFrom, earliest.WindowTo));
    }

    // ---- Fixture ------------------------------------------------------------------------------------------------------

    private IRenderedComponent<ReviewsSchedule> RenderForm()
    {
        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.Markup.Contains("No reviews yet"));
        cut.Find("button.btn-primary").Click();
        cut.WaitForState(() => cut.FindAll("#review-period").Count == 1);
        return cut;
    }

    private static void ChoosePanelAndTrainee(IRenderedComponent<ReviewsSchedule> cut, string until = "will be on the agenda")
    {
        cut.Find("#review-panel").Change(Panel.ToString());
        cut.WaitForState(() => cut.FindAll("select#review-trainee option[value='paeds-a']").Count == 1);
        cut.Find("#review-trainee").Change("paeds-a");
        cut.WaitForState(() => cut.Find("#agenda-preview").TextContent.Contains(until));
    }

    private static CommitteeAgendaPreviewDto Preview(int year, int semester)
    {
        var label = $"{year} S{semester}";
        return new CommitteeAgendaPreviewDto(
            year,
            semester,
            label,
            true,
            [
                Line(1, "PAED-001", "Acute admission", closing: true, CommitteeAgendaLineStatus.Due, label),
                Line(3, "PAED-003", "Paediatric ward round", closing: false, CommitteeAgendaLineStatus.DueByYearEnd, $"{year}")
            ],
            [
                new CommitteeAgendaElsewhereDto(4, "PAED-004", "Newborn", 11, "Neonatal CCC", label, CommitteeAgendaElsewhereStatus.NotYetDecided),
                new CommitteeAgendaElsewhereDto(5, "PAED-005", "Sick newborn", 11, "Neonatal CCC", label, CommitteeAgendaElsewhereStatus.NotYetDecided)
            ],
            ["PAED-006"]);
    }

    private static CommitteeAgendaLineDto Line(
        int epaId, string code, string title, bool closing, CommitteeAgendaLineStatus status, string window)
        => new(0, epaId, code, title, CommitteeAgendaLineOrigin.Cadence, 2026, 1, window, closing, false,
            CommitteeAgendaLineState.Due, status, false, false, null, null, 0);

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

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

using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// T131 slice 5: the pages say what an entrustment-only review decides. The review page asks it for no progression
/// category, in the decision or a remitted appeal, and says why; the trainee's reviews call its decision what it is,
/// never "Pending"; and the scheduling form offers only the types the handler accepts for the panel and period.
/// </summary>
public sealed partial class EntrustmentOnlyReviewPageTests : TestContext
{
    private const string NeonatalCcc = "Neonatal team Clinical Competency Committee";

    private readonly RecordingSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public EntrustmentOnlyReviewPageTests()
    {
        _auth = this.AddTestAuthorization();
        SignInAs("chair-1", "CommitteeMember");
        Services.AddSingleton<IScopedSender>(_sender);
    }

    // ─── The review page ─────────────────────────────────────────────────────

    [Fact]
    public void AnEntrustmentOnlyReview_AsksForNoCategory_SaysWhy_AndRecordsNone()
    {
        var cut = RenderReview(Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.InProgress));

        Text(cut.Find("#review-type")).Should().Be("Entrustment-only review");
        cut.FindAll("#decision-category").Should().BeEmpty();
        Text(cut.Find("#entrustment-only-note")).Should().Be(CommitteeDecisionWording.EntrustmentOnlyForThePanel);

        cut.Find("#decision-rationale").Change("PAED-004 entrusted at 3a on the evidence named.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision").Click();
        cut.WaitForState(() => _sender.Received.OfType<RecordCommitteeDecisionCommand>().Any());

        _sender.Received.OfType<RecordCommitteeDecisionCommand>().Single().Category.Should().BeNull();
        cut.WaitForState(() => cut.Markup.Contains("Decision recorded."));
        Text(cut.Find("#decision-41 h4")).Should().Be("Entrustment decisions only");
    }

    [Fact]
    public void AProgressionReview_StillAsksForTheCategory_InWords_AndSendsIt()
    {
        var cut = RenderReview(Review(CommitteeReviewType.AnnualProgression, CommitteeReviewState.InProgress));

        Text(cut.Find("#review-type")).Should().Be("Annual progression review");
        cut.FindAll("#entrustment-only-note").Should().BeEmpty();
        var first = cut.FindAll("#decision-category option").First();
        first.GetAttribute("value").Should().BeEmpty("the page never shows a category the chair did not choose");
        first.TextContent.Trim().Should().Be("Select a category…");
        cut.FindAll("#decision-category option").Skip(1).Select(option => option.TextContent.Trim())
            .Should().StartWith("Satisfactory Progress").And.Contain("Inadequate Progress — Additional Training");

        cut.Find("#decision-category").Change(CommitteeDecisionCategory.OutcomeDeferred.ToString());
        cut.Find("#decision-rationale").Change("Deferred to the next sitting.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision").Click();
        cut.WaitForState(() => _sender.Received.OfType<RecordCommitteeDecisionCommand>().Any());

        _sender.Received.OfType<RecordCommitteeDecisionCommand>().Single().Category.Should().Be(CommitteeDecisionCategory.OutcomeDeferred);
        cut.WaitForState(() => cut.Markup.Contains("Decision recorded."));
        Text(cut.Find("#decision-41 h4")).Should().Be("Outcome Deferred");
    }

    [Fact]
    public void AProgressionReview_LeftWithNoCategoryChosen_SendsNone_AndShowsTheHandlersRefusal()
    {
        var cut = RenderReview(Review(CommitteeReviewType.AnnualProgression, CommitteeReviewState.InProgress));
        _sender.On<RecordCommitteeDecisionCommand>(recorded => recorded.Category is null
            ? throw new InvalidOperationException(CommitteeReview.ProgressionNeedsACategory)
            : (object?)null);

        cut.Find("#decision-rationale").Change("On track.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision").Click();
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);

        _sender.Received.OfType<RecordCommitteeDecisionCommand>().Single().Category.Should().BeNull();
        Text(cut.Find(".alert-danger")).Should().Be(CommitteeReview.ProgressionNeedsACategory);
        cut.Markup.Should().NotContain("Decision recorded.");
    }

    [Fact]
    public void AnEntrustmentOnlyReview_WithNothingOnItsAgenda_ShowsRecordDisabled_WithTheReason()
    {
        var agenda = new CommitteeAgendaDto(30, 2026, 1, "2026 S1", false, [], []) { DecidesProgression = false };
        var cut = RenderReview(Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.InProgress) with { Agenda = agenda });

        var record = cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision");
        record.HasAttribute("disabled").Should().BeTrue();
        record.GetAttribute("aria-describedby").Should().Be("record-reason");
        Text(cut.Find("#record-reason")).Should().Be($"Record decision: {CommitteeReview.NothingOnTheAgenda}");
    }

    [Fact]
    public void ARatifiedEntrustmentOnlyReview_SaysItsDecisionIsTheStarsItIssued_NotTheOnesStagedBelow()
    {
        var cut = RenderReview(Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.Ratified));

        Text(cut.Find("#entrustment-only-note")).Should().Be(
            "This review decided entrustment only. Its decision is the entrustment decisions it issued when it was ratified, " +
            "each named on its agenda, and it records no progression category.");
    }

    [Fact]
    public void AnEntrustmentOnlyReview_IsNotWarnedAboutTheSittingOrder_ThatAProgressionDecisionIs()
    {
        // O8's warning is that a progression decision is being taken before another panel's entrustment decisions. The
        // same agenda on a progression review warns (ReviewDetailAgendaTests); an entrustment-only review takes none.
        var agenda = new CommitteeAgendaDto(30, 2026, 1, "2026 S1", false, [], [])
        {
            RoutedElsewhere =
            [
                new CommitteeAgendaElsewhereDto(4, "PAED-004", "Newborn", 11, "Neonatal CCC", "2026 S1", CommitteeAgendaElsewhereStatus.NotYetDecided)
            ]
        };

        var entrustmentOnly = RenderReview(Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.InProgress) with { Agenda = agenda });
        entrustmentOnly.FindAll("#sitting-order-warning").Should().BeEmpty();
        entrustmentOnly.FindAll("#agenda-elsewhere-4").Should().ContainSingle("what another panel decides is still listed");
    }

    [Fact]
    public void RemittingAnEntrustmentOnlyReview_AsksForNoCategory_AndSendsNone()
    {
        SignInAs("external-1", "CommitteeMember");
        var cut = RenderReview(Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.UnderAppeal));

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());
        cut.FindAll("#appeal-category").Should().BeEmpty();
        Text(cut.Find("#remit-entrustment-only-note")).Should().Be(
            "This review decides entrustment only, so the replacement decision records no progression category, and it " +
            "changes no entrustment decision: the STARs this review issued stand. To change one, revoke it and re-decide the " +
            "EPA at a new review.");
        cut.Find("#appeal-rationale").Change("Re-read on appeal: the level stands.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Resolve appeal").Click();
        cut.WaitForState(() => _sender.Received.OfType<ResolveAppealCommand>().Any());

        var resolved = _sender.Received.OfType<ResolveAppealCommand>().Single();
        resolved.Outcome.Should().Be(CommitteeAppealOutcome.Remitted);
        resolved.RemittedCategory.Should().BeNull();
    }

    [Fact]
    public void RemittingAProgressionReview_AsksForTheReplacementCategory_WithNoneChosenForTheChair()
    {
        SignInAs("external-1", "CommitteeMember");
        var cut = RenderReview(Review(CommitteeReviewType.AnnualProgression, CommitteeReviewState.UnderAppeal));

        cut.Find("#appeal-outcome").Change(CommitteeAppealOutcome.Remitted.ToString());

        var first = cut.FindAll("#appeal-category option").First();
        first.GetAttribute("value").Should().BeEmpty();
        first.TextContent.Trim().Should().Be("Select a category…");
        cut.FindAll("#remit-entrustment-only-note").Should().BeEmpty();

        cut.Find("#appeal-category").Change(CommitteeDecisionCategory.InadequateProgressAdditionalTraining.ToString());
        cut.Find("#appeal-rationale").Change("Re-review in three months.");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Resolve appeal").Click();
        cut.WaitForState(() => _sender.Received.OfType<ResolveAppealCommand>().Any());

        _sender.Received.OfType<ResolveAppealCommand>().Single().RemittedCategory
            .Should().Be(CommitteeDecisionCategory.InadequateProgressAdditionalTraining);
    }

    // ─── The trainee's reviews ───────────────────────────────────────────────

    [Fact]
    public void TheTrainee_ReadsTheirEntrustmentOnlyReview_AsDecidingEntrustmentOnly_NeverAsPending()
    {
        SignInAs("trainee-1", WombatRoles.Trainee);
        var review = Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.Ratified) with
        {
            Agenda = new CommitteeAgendaDto(30, 2026, 1, "2026 S1", false,
                [
                    new CommitteeAgendaLineDto(1, 4, "PAED-004", "Newborn", CommitteeAgendaLineOrigin.Cadence, 2026, 1, "2026 S1",
                        true, false, CommitteeAgendaLineState.Decided, CommitteeAgendaLineStatus.Decided, false, false, null, 91, 1)
                ],
                [])
        };
        _sender
            .On<ListReviewsForTraineeQuery>(_ => new[] { ListItem(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.Ratified, null) })
            .On<GetCommitteeReviewByIdQuery>(_ => review);

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Trim() == "View"));

        var row = Text(cut.FindAll("tbody tr").Single());
        row.Should().Contain("Entrustment only").And.Contain("Entrustment decisions only").And.NotContain("Pending");

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "View").Click();
        cut.WaitForState(() => cut.FindAll("#current-decision-outcome").Count == 1);

        Text(cut.Find("#my-review-type")).Should().Be("Type: Entrustment-only review");
        Text(cut.Find("#current-decision-outcome")).Should().Be("Entrustment decisions only");
        Text(cut.Find("#entrustment-only-note")).Should().Be(
            $"{CommitteeDecisionWording.EntrustmentOnlyForTheTrainee} {CommitteeDecisionWording.EntrustmentOnlySeeTheAgenda}");
    }

    [Fact]
    public void TheTrainee_IsToldARemittedEntrustmentOnlyReviewChangedNoStar()
    {
        SignInAs("trainee-1", WombatRoles.Trainee);
        var review = Review(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.Final) with
        {
            Decisions = [Decision(42, null) with { SupersedesDecisionId = 41 }, Decision(41, null)]
        };
        _sender
            .On<ListReviewsForTraineeQuery>(_ => new[] { ListItem(CommitteeReviewType.EntrustmentOnly, CommitteeReviewState.Final, null) })
            .On<GetCommitteeReviewByIdQuery>(_ => review);

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Trim() == "View"));
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "View").Click();
        cut.WaitForState(() => cut.FindAll("#current-decision-outcome").Count == 1);

        Text(cut.Find("#entrustment-only-note")).Should().Be(
            "This review decided entrustment only, so it records no progression outcome. The appeal was remitted: this " +
            "decision is the appeal body's, and the entrustment decisions this review issued stand unless one is revoked.");
    }

    [Fact]
    public void TheTrainee_ReadsAProgressionReviewsCategory_InWords()
    {
        SignInAs("trainee-1", WombatRoles.Trainee);
        _sender
            .On<ListReviewsForTraineeQuery>(_ => new[]
            {
                ListItem(CommitteeReviewType.AnnualProgression, CommitteeReviewState.Ratified, CommitteeDecisionCategory.SatisfactoryWithObservations)
            })
            .On<GetCommitteeReviewByIdQuery>(_ => Review(CommitteeReviewType.AnnualProgression, CommitteeReviewState.Ratified));

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Trim() == "View"));

        Text(cut.FindAll("tbody tr").Single()).Should().Contain("Annual progression").And.Contain("Satisfactory with Observations");
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "View").Click();
        cut.WaitForState(() => cut.FindAll("#current-decision-outcome").Count == 1);
        cut.FindAll("#entrustment-only-note").Should().BeEmpty();
    }

    // ─── Scheduling ──────────────────────────────────────────────────────────

    [Fact]
    public void BeforeTheNeonatalCcc_TheFormOffersOnlyAnEntrustmentOnlyReview_SaysWhy_AndSchedulesOne()
    {
        var cut = RenderSchedule();

        ChoosePanelAndTrainee(cut, NeonatalPanel);

        TypeOptions(cut).Should().Equal(CommitteeReviewType.EntrustmentOnly.ToString());
        Text(cut.Find("#review-type-help")).Should().Be(
            $"Neonatal CCC sits as the {NeonatalCcc}, which decides entrustment only: a review before it records no progression category.");
        cut.Find("#review-type").GetAttribute("aria-describedby").Should().Be("review-type-help");

        cut.Find("#review-panel").Closest("form")!.Submit();

        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().ContainSingle()
            .Which.ReviewType.Should().Be(CommitteeReviewType.EntrustmentOnly);
    }

    [Fact]
    public void AGeneralPanel_OffersEntrustmentOnlyInSemester1_AndNotInSemester2_WhereTheChoiceFallsBackToProgression()
    {
        var periods = CommitteeReviewPeriods.Around(QuotaCalendar.Today());
        var semester1 = periods.First(period => period.Semester == 1);
        var semester2 = periods.First(period => period.Semester == 2);
        var cut = RenderSchedule();
        ChoosePanelAndTrainee(cut, GeneralPanel);

        cut.Find("#review-period").Change(semester1.Key);
        TypeOptions(cut).Should().Equal(
            nameof(CommitteeReviewType.AnnualProgression), nameof(CommitteeReviewType.PreGraduation), nameof(CommitteeReviewType.EntrustmentOnly));
        Text(cut.Find("#review-type-help")).Should().Be(
            "Choose Entrustment-only review when this sitting decides STARs and not the trainee's progression: it records no " +
            "progression category. The semester-2 sitting always decides progression.");
        cut.Find("#review-type").Change(nameof(CommitteeReviewType.EntrustmentOnly));

        cut.Find("#review-period").Change(semester2.Key);
        TypeOptions(cut).Should().Equal(nameof(CommitteeReviewType.AnnualProgression), nameof(CommitteeReviewType.PreGraduation));
        cut.Find("#review-panel").Closest("form")!.Submit();

        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().ContainSingle()
            .Which.ReviewType.Should().Be(CommitteeReviewType.AnnualProgression, "the entrustment-only choice is not a semester-2 type");
    }

    [Fact]
    public void ChoosingAnotherPanel_TakesThatPanelsDefaultType()
    {
        var cut = RenderSchedule();

        ChoosePanelAndTrainee(cut, NeonatalPanel);
        cut.Find("#review-type").GetAttribute("value").Should().Be(nameof(CommitteeReviewType.EntrustmentOnly));

        ChoosePanelAndTrainee(cut, GeneralPanel);
        cut.Find("#review-type").GetAttribute("value").Should().Be(nameof(CommitteeReviewType.AnnualProgression));
    }

    [Fact]
    public void ThePreview_SaysFirstWhenThePanelDecidesNothingOnTheTraineesCurriculum()
    {
        // The scheduling handler refuses an entrustment-only review there, with the same reason (picker = gate).
        var preview = new CommitteeAgendaPreviewDto(2026, 1, "2026 S1", true, [], [], []) { PanelDecidesAnything = false };

        CommitteeAgendaText.PreviewSentences(preview).Should().Equal(
            "This panel decides no EPA on this trainee's curriculum, so an entrustment-only review before it would have nothing to decide.");
        CommitteeAgendaText.PreviewSentences(preview with { PanelDecidesAnything = true }).Should().Equal(
            "No EPA this panel decides is due for 2026 S1.");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private const int GeneralPanel = 10;
    private const int NeonatalPanel = 11;

    private void SignInAs(string userId, string role)
    {
        _auth.SetAuthorized($"{userId}@test");
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private IRenderedComponent<ReviewDetail> RenderReview(CommitteeReviewDetailDto review)
    {
        _sender
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => Array.Empty<StarEpaOptionDto>())
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>())
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null)
            .On<GetCommitteeAgendaQuery>(_ => review.Agenda)
            // The command's answer, as the shared mapper builds it: the decision it recorded, with the category it was sent.
            .On<RecordCommitteeDecisionCommand>(recorded => review with
            {
                State = CommitteeReviewState.Decided,
                Decisions = [Decision(41, recorded.Category)]
            })
            .On<ResolveAppealCommand>(_ => review with { State = CommitteeReviewState.Final });

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));
        return cut;
    }

    private IRenderedComponent<ReviewsSchedule> RenderSchedule()
    {
        SignInAs("coordinator-a", WombatRoles.Coordinator);
        _sender
            .On<ListDecisionPanelsQuery>(_ => new[]
            {
                new DecisionPanelSummaryDto(GeneralPanel, "General CCC", DecisionPanelScope.Institution, 1, null, 3),
                new DecisionPanelSummaryDto(NeonatalPanel, "Neonatal CCC", DecisionPanelScope.Institution, 1, null, 2, "neonatal", NeonatalCcc)
            })
            .On<ListReviewsForPanelQuery>(_ => Array.Empty<CommitteeReviewListItemDto>())
            .On<ListSchedulableTraineesQuery>(_ => new[] { new SchedulableTraineeDto("paeds-a", "Palesa Paeds") })
            .On<PreviewCommitteeAgendaQuery>(query => new CommitteeAgendaPreviewDto(
                query.AcademicYear, query.Semester, $"{query.AcademicYear} S{query.Semester}", true, [], [], []))
            .On<ScheduleCommitteeReviewCommand>(command => new CommitteeReviewListItemDto(
                77, command.TraineeUserId, command.PanelId, "A panel", command.ReviewPeriodFrom, command.ReviewPeriodTo,
                command.ScheduledOn, CommitteeReviewState.Scheduled, null, null, ReviewType: command.ReviewType ?? CommitteeReviewType.AnnualProgression)
            {
                AcademicYear = command.AcademicYear,
                Semester = command.Semester
            });

        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.Markup.Contains("No reviews yet"));
        cut.Find("button.btn-primary").Click();
        cut.WaitForState(() => cut.FindAll("#review-period").Count == 1);
        return cut;
    }

    private static void ChoosePanelAndTrainee(IRenderedComponent<ReviewsSchedule> cut, int panelId)
    {
        cut.Find("#review-panel").Change(panelId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        cut.WaitForState(() => cut.FindAll("select#review-trainee option[value='paeds-a']").Count == 1);
        cut.Find("#review-trainee").Change("paeds-a");
    }

    private static IReadOnlyList<string?> TypeOptions(IRenderedComponent<ReviewsSchedule> cut)
        => cut.FindAll("#review-type option").Select(option => option.GetAttribute("value")).ToArray();

    private static CommitteeReviewListItemDto ListItem(
        CommitteeReviewType type, CommitteeReviewState state, CommitteeDecisionCategory? category)
        => new(30, "trainee-1", 20, "Neonatal CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2),
            state, category, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), ReviewType: type)
        {
            AcademicYear = 2026,
            Semester = 1
        };

    private static CommitteeDecisionDto Decision(int id, CommitteeDecisionCategory? category)
        => new(id, category, $"Decision {id}.", null, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null)
        {
            Attendees =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu" },
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { Name = "Priya Naidoo" }
            ]
        };

    private static CommitteeReviewDetailDto Review(CommitteeReviewType type, CommitteeReviewState state)
        => new CommitteeReviewDetailDto(
            30, "trainee-1", 20, "Neonatal CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2),
            state, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null, null, null,
            state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress
                ? []
                : [Decision(41, type == CommitteeReviewType.EntrustmentOnly ? null : CommitteeDecisionCategory.SatisfactoryWithObservations)],
            [], [], ReviewType: type)
        {
            AcademicYear = 2026,
            Semester = 1,
            // The page offers the chair's controls by what the query says the caller may do (T213); these tests act as the
            // chair.
            CallerChairs = true,
            CallerResolvesAppeals = true,
            TraineeName = "Lerato Molefe",
            PanelMembers =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu", MaySit = true },
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { Name = "Priya Naidoo", MaySit = true },
                new CommitteePersonDto("external-1", DecisionPanelMemberRole.External) { Name = "Anna Botha", MaySit = true }
            ]
        };

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

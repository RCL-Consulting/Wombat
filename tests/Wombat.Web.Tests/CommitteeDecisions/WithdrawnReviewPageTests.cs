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
/// T258 review: a withdrawn review's pages read as withdrawn. Nothing more is decided at it, so its Decision card does not
/// say a decision is coming, a decision recorded and then withdrawn is said never to have been ratified, an
/// entrustment-only review is not told of decisions "staged below", and the review list says it was withdrawn, formative
/// or not.
/// </summary>
public sealed partial class WithdrawnReviewPageTests : TestContext
{
    private static readonly DateOnly WithdrawnDay = new(2026, 9, 25);

    private readonly RecordingSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public WithdrawnReviewPageTests()
    {
        _auth = this.AddTestAuthorization();
        SignInAs("chair-1", WombatRoles.CommitteeMember);
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void AProgressionReviewDecidedAndThenWithdrawn_KeepsItsCategory_AndSaysItWasNeverRatified()
    {
        var cut = RenderReview(Withdrawn(CommitteeReviewType.AnnualProgression, CommitteeDecisionCategory.SatisfactoryWithObservations));

        Text(cut.Find("#decision-41 h4")).Should().Be("Satisfactory with Observations", "the record of what the panel recorded");
        Text(cut.Find("#withdrawn-decision-note")).Should().Be(
            "Recorded, and never ratified: the review was withdrawn before it was, so this decision took no effect.");
        cut.FindAll("#no-decision-note").Should().BeEmpty();
        cut.FindAll("button").Select(button => button.TextContent.Trim()).Should().NotContain(["Ratify", "Record decision"]);
    }

    [Fact]
    public void AReviewWithdrawnBeforeAnyDecision_SaysNoneWasRecordedBeforeIt_NotThatOneIsComing()
    {
        var cut = RenderReview(Withdrawn(CommitteeReviewType.AnnualProgression, category: null) with { Decisions = [] });

        Text(cut.Find("#no-decision-note")).Should().Be("No decision was recorded before the review was withdrawn.");
        cut.FindAll("#withdrawn-decision-note").Should().BeEmpty();
    }

    [Fact]
    public void AWithdrawnEntrustmentOnlyReview_IsToldItIssuedNothing_NotOfDecisionsStagedBelow()
    {
        var cut = RenderReview(Withdrawn(CommitteeReviewType.EntrustmentOnly, category: null));

        Text(cut.Find("#entrustment-only-note")).Should().Be(
            "This review was to decide entrustment only. It was withdrawn before it was ratified, so it issued no entrustment " +
            "decision, and it records no progression category.");
        Text(cut.Find("#withdrawn-decision-note")).Should().Be(CommitteeDecisionWording.WithdrawnDecisionNote);
    }

    [Fact]
    public void AnOpenReviewsRecordedDecision_IsNotSaidToBeWithdrawn()
    {
        var cut = RenderReview(Withdrawn(CommitteeReviewType.AnnualProgression, CommitteeDecisionCategory.SatisfactoryProgress) with
        {
            State = CommitteeReviewState.Decided,
            WithdrawnOn = null,
            WithdrawalReason = null
        });

        cut.FindAll("#withdrawn-decision-note").Should().BeEmpty();
        cut.FindAll("#review-withdrawn").Should().BeEmpty();
    }

    [Fact]
    public void TheReviewList_SaysAWithdrawnFormativeReviewWasWithdrawn_NotADash()
    {
        SignInAs("coordinator-a", WombatRoles.Coordinator);
        _sender
            .On<GetCommitteeReviewsAccessQuery>(_ => new CommitteeReviewsAccessDto(MaySchedule: false, TraineeNote: null))
            .On<ListReviewsForPanelQuery>(_ => new[]
            {
                ListItem(30, CommitteeReviewState.Withdrawn, formative: true),
                ListItem(31, CommitteeReviewState.InProgress, formative: true),
                ListItem(32, CommitteeReviewState.Withdrawn, formative: false)
            });

        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 3);

        cut.FindAll("tbody tr").Select(row => row.Children[6].TextContent.Trim()).Should().Equal(
            CommitteeDecisionWording.WithdrawnOutcome, "—", CommitteeDecisionWording.WithdrawnOutcome);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

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
            .On<GetCommitteeAgendaQuery>(_ => review.Agenda);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));
        return cut;
    }

    /// <summary>A review decided, with this category, and then withdrawn by an erasure.</summary>
    private static CommitteeReviewDetailDto Withdrawn(CommitteeReviewType type, CommitteeDecisionCategory? category)
        => new CommitteeReviewDetailDto(
            30, "deleted_user_1a2b3c4d", 20, "Neonatal CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2), CommitteeReviewState.Withdrawn, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc),
            "chair-1", null, null, null,
            [new CommitteeDecisionDto(41, category, "Decision 41.", null, new DateTime(2026, 7, 3, 9, 0, 0, DateTimeKind.Utc), "chair-1", null)],
            [], [], ReviewType: type)
        {
            AcademicYear = 2026,
            Semester = 1,
            CallerChairs = true,
            CallerResolvesAppeals = true,
            TraineeName = "deleted_user_1a2b3c4d",
            WithdrawnOn = WithdrawnDay,
            WithdrawalReason = CommitteeReview.WithdrawnTraineeErased,
            PanelMembers =
            [
                new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu", MaySit = true },
                new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { Name = "Priya Naidoo", MaySit = true }
            ]
        };

    private static CommitteeReviewListItemDto ListItem(int id, CommitteeReviewState state, bool formative)
        => new(id, "deleted_user_1a2b3c4d", 20, "Neonatal CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2), state, CurrentDecisionCategory: null, RatifiedOn: null, IsFormative: formative)
        {
            AcademicYear = 2026,
            Semester = 1
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

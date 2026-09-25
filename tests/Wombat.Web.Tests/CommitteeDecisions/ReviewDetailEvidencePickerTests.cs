using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The staging form names the lines of the frozen snapshot a decision rests on: grouped as the snapshot card groups
/// them, the chosen EPA's lines first, never a supervisor report, and Stage stays disabled, with its reason said, until
/// one is ticked (D38, T131).
/// </summary>
public sealed class ReviewDetailEvidencePickerTests : TestContext
{
    private const string ReasonId = "pending-evidence-reason";

    public ReviewDetailEvidencePickerTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles("CommitteeMember");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
    }

    [Fact]
    public void ThePicker_OffersEveryLineADecisionMayRestOn_GroupedByEpa_AndNoSupervisorReport()
    {
        var (cut, _) = RenderPage();

        Legends(cut).Should().Equal("PAED-001 — Acute admission", "PAED-002 — Ward round", "Not about a single EPA");
        CheckboxIds(cut).Should().BeEquivalentTo(new[] { "pending-evidence-1", "pending-evidence-2", "pending-evidence-3" });
        cut.FindAll("#pending-evidence-4").Should().BeEmpty("a supervisor report cannot ground a decision");
        cut.Find("label[for=pending-evidence-1]").TextContent.Should().Contain("Mini-CEX #101").And.Contain("3a · 2026-02-10 · completed");
    }

    [Fact]
    public void ChoosingAnEpa_ListsItsLinesFirst_AsAHint_WithoutHidingTheOthers()
    {
        var (cut, _) = RenderPage();

        cut.Find("#pending-epa").Change("8");

        Legends(cut).Should().Equal("PAED-002 — Ward round", "PAED-001 — Acute admission", "Not about a single EPA");
        cut.Find("#pending-evidence-help").TextContent.Should().Contain("PAED-002's lines are listed first.");
        CheckboxIds(cut).Should().HaveCount(3);
    }

    [Fact]
    public void Stage_IsDisabledUntilAnItemIsTicked_AndSaysWhy()
    {
        var (cut, _) = RenderPage();

        var stage = StageButton(cut);
        stage.HasAttribute("disabled").Should().BeTrue();
        stage.GetAttribute("aria-describedby").Should().Be(ReasonId);
        cut.Find($"#{ReasonId}").TextContent.Should().Be("Stage pending decision: Name at least one item of the evidence snapshot.");

        cut.Find("#pending-evidence-1").Change(true);

        StageButton(cut).HasAttribute("disabled").Should().BeFalse();
        StageButton(cut).HasAttribute("aria-describedby").Should().BeFalse();
        cut.FindAll($"#{ReasonId}").Should().BeEmpty();

        cut.Find("#pending-evidence-1").Change(false);
        StageButton(cut).HasAttribute("disabled").Should().BeTrue("unticking the only item takes the evidence away again");
    }

    [Fact]
    public void OneNamedItem_OrNoneAboutTheChosenEpa_IsSaid_NeverRefused()
    {
        var (cut, _) = RenderPage();
        cut.Find("#pending-epa").Change("8");

        cut.Find("#pending-evidence-1").Change(true);

        var hints = Hints(cut);
        hints.Should().Contain(hint => hint.Contains("never taken from a single form"));
        hints.Should().Contain("None of the named items is about PAED-002.");
        StageButton(cut).HasAttribute("disabled").Should().BeFalse();

        cut.Find("#pending-evidence-2").Change(true);

        Hints(cut).Should().BeEmpty("two items are named, one of them about PAED-002");
    }

    /// <summary>
    /// A requested form nobody filled in is in the snapshot on purpose (T138), and D38 as adopted lets any line ground a
    /// decision. So naming only unfinished forms is said, never refused; and a line frozen before T131, which does not
    /// record whether it was finished, is not counted either way. (T131 review)
    /// </summary>
    [Fact]
    public void NamingOnlyUnfinishedForms_IsSaid_NeverRefused_AndALineThatDoesNotSayIsNotCounted()
    {
        var (cut, _) = RenderPage(evidence:
        [
            Line(1, 101, 7, "PAED-001", "Acute admission"),
            Unfinished(Line(2, 102, 7, "PAED-001", "Acute admission"), "requested"),
            Unfinished(Line(3, 103, 7, "PAED-001", "Acute admission"), "draft"),
            Line(4, 104, 7, "PAED-001", "Acute admission") with { SourceFinished = null }
        ]);
        cut.Find("#pending-epa").Change("7");

        cut.Find("#pending-evidence-2").Change(true);
        cut.Find("#pending-evidence-3").Change(true);

        Hints(cut).Should().Equal(ReviewDetail.NoFinishedItemHint);
        StageButton(cut).HasAttribute("disabled").Should().BeFalse("it is said, not refused");

        cut.Find("#pending-evidence-1").Change(true);
        Hints(cut).Should().BeEmpty("one named item was finished");

        cut.Find("#pending-evidence-1").Change(false);
        cut.Find("#pending-evidence-4").Change(true);
        Hints(cut).Should().BeEmpty("a line frozen before T131 does not say whether it was finished");
    }

    [Fact]
    public void Staging_SendsTheTickedLines()
    {
        var (cut, sender) = RenderPage();
        cut.Find("#pending-epa").Change("7");
        cut.Find("#pending-level").Change("13");
        cut.Find("#pending-rationale").Change("Consistent across the window.");
        cut.Find("#pending-evidence-1").Change(true);
        cut.Find("#pending-evidence-3").Change(true);

        PendingForm(cut).Submit();

        cut.WaitForState(() => sender.Staged is not null);
        sender.Staged!.EvidenceItemIds.Should().BeEquivalentTo(new[] { 1, 3 });
        sender.Staged.EpaId.Should().Be(7);
    }

    [Fact]
    public void AStagedDecision_SaysWhatItRestsOn()
    {
        var pending = new PendingEntrustmentDecisionDto(
            90, 30, 7, "PAED-001", "Acute admission", 13, "3a", new DateOnly(2026, 7, 2), null, "Consistent.",
            [1, 3], new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1");

        var (cut, _) = RenderPage(pending);

        cut.Markup.Should().Contain("Rests on 2 items of the snapshot: Mini-CEX #101; Annual MSF #50.");
    }

    [Fact]
    public void ASnapshotWithNothingADecisionCanRestOn_SaysSo_AndStageStaysDisabled()
    {
        var (cut, _) = RenderPage(evidence: [Report(4)]);

        cut.Markup.Should().Contain("The evidence snapshot holds no line a decision can rest on");
        StageButton(cut).HasAttribute("disabled").Should().BeTrue();
    }

    // ---- Fixture -------------------------------------------------------------------------------------------------

    private static AngleSharp.Dom.IElement PendingForm(IRenderedComponent<ReviewDetail> cut)
        => cut.Find("#pending-epa").Closest("form")!;

    private static AngleSharp.Dom.IElement StageButton(IRenderedComponent<ReviewDetail> cut)
        => PendingForm(cut).QuerySelectorAll("button[type=submit]").Single();

    private static IReadOnlyList<string> Legends(IRenderedComponent<ReviewDetail> cut)
        => PendingForm(cut).QuerySelectorAll("fieldset fieldset > legend")
            .Select(legend => legend.TextContent.Trim())
            .ToArray();

    private static IReadOnlyList<string> CheckboxIds(IRenderedComponent<ReviewDetail> cut)
        => PendingForm(cut).QuerySelectorAll("input[type=checkbox]")
            .Select(input => input.Id ?? string.Empty)
            .ToArray();

    private static IReadOnlyList<string> Hints(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("#pending-evidence-hints .field-warning").Select(hint => hint.TextContent.Trim()).ToArray();

    private (IRenderedComponent<ReviewDetail> Cut, FakeSender Sender) RenderPage(
        PendingEntrustmentDecisionDto? pending = null,
        IReadOnlyList<CommitteeEvidenceDto>? evidence = null)
    {
        var review = Review(evidence ?? [Line(1, 101, 7, "PAED-001", "Acute admission"), Line(2, 102, 8, "PAED-002", "Ward round"), Campaign(3, 50), Report(4)]);
        var sender = new FakeSender(review, pending is null ? [] : [pending]);
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, review.Id));
        cut.WaitForState(() => cut.Markup.Contains("Evidence it rests on"));
        return (cut, sender);
    }

    private static CommitteeEvidenceDto Line(int id, int activityId, int epaId, string code, string title)
        => new(
            id,
            CommitteeEvidenceSourceType.Activity,
            activityId,
            null,
            null,
            $"Mini-CEX #{activityId}",
            "State: completed.",
            new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            EpaId: epaId,
            EpaCode: code,
            EpaTitle: title,
            InstrumentName: "Mini-CEX",
            IsRatedInstrument: true,
            RatingLabel: "3a",
            ObservedOn: new DateOnly(2026, 2, 10),
            ObservedOnDeclared: true,
            SourceState: "completed",
            SourceFinished: true);

    private static CommitteeEvidenceDto Unfinished(CommitteeEvidenceDto line, string state)
        => line with { SourceState = state, RatingLabel = null, Summary = $"State: {state}.", SourceFinished = false };

    private static CommitteeEvidenceDto Campaign(int id, int campaignId)
        => new(
            id,
            CommitteeEvidenceSourceType.MsfCampaign,
            null,
            campaignId,
            null,
            $"Annual MSF #{campaignId}",
            "State: Released; responses 8.",
            new DateTime(2026, 5, 2, 8, 0, 0, DateTimeKind.Utc),
            SourceState: "Released",
            SourceFinished: true);

    private static CommitteeEvidenceDto Report(int id)
        => new(
            id,
            CommitteeEvidenceSourceType.SupervisorReport,
            null,
            null,
            3,
            "Supervisor report #3",
            "Rotation report.",
            new DateTime(2026, 5, 3, 8, 0, 0, DateTimeKind.Utc));

    private static CommitteeReviewDetailDto Review(IReadOnlyList<CommitteeEvidenceDto> evidence)
        => new(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            CommitteeReviewState.InProgress,
            new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc),
            "chair-1",
            null,
            null,
            null,
            [],
            [],
            evidence)
        {
            AcademicYear = 2026,
            Semester = 1,
            // The page offers the chair's controls by what the query says the caller may do (T213); these tests act as the
            // chair.
            CallerChairs = true,
            CallerResolvesAppeals = true
        };

    private sealed class FakeSender : IScopedSender
    {
        private static readonly IReadOnlyList<StarEpaOptionDto> StarEpas =
        [
            new StarEpaOptionDto(7, "PAED-001", "Acute admission", 1, "CPSA ladder"),
            new StarEpaOptionDto(8, "PAED-002", "Ward round", 1, "CPSA ladder")
        ];

        private static readonly IReadOnlyList<EntrustmentScaleDto> Scales =
        [
            new EntrustmentScaleDto(1, "CPSA ladder", null,
            [
                new EntrustmentLevelDto(11, 1, "1", null),
                new EntrustmentLevelDto(12, 2, "2", null),
                new EntrustmentLevelDto(13, 3, "3a", null)
            ])
        ];

        private readonly CommitteeReviewDetailDto _review;
        private readonly IReadOnlyList<PendingEntrustmentDecisionDto> _pending;

        public FakeSender(CommitteeReviewDetailDto review, IReadOnlyList<PendingEntrustmentDecisionDto> pending)
        {
            _review = review;
            _pending = pending;
        }

        public StagePendingEntrustmentDecisionCommand? Staged { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? response = request switch
            {
                GetCommitteeReviewByIdQuery => _review,
                ListPendingEntrustmentDecisionsForReviewQuery => _pending,
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                ListStarEpaOptionsForReviewQuery => StarEpas,
                GetEntrustmentScalesListQuery => Scales,
                StagePendingEntrustmentDecisionCommand stage => Stage(stage),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private PendingEntrustmentDecisionDto Stage(StagePendingEntrustmentDecisionCommand command)
        {
            Staged = command;
            return new PendingEntrustmentDecisionDto(
                91, command.ReviewId, command.EpaId, "PAED-001", "Acute admission", command.AuthorisedLevelId, "3a",
                command.IssuedOn, command.ExpiresOn, command.Rationale, command.EvidenceItemIds, DateTime.UtcNow, "chair-1");
        }
    }
}

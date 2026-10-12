using System.Security.Claims;
using System.Text.RegularExpressions;
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
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// A review says how many feedback campaigns closed in its window and are not in its snapshot, whether still awaiting
/// release or released after it started, and says nothing when there are none. (T173)
/// </summary>
/// <remarks>
/// The snapshot holds released campaigns only (T138), so a campaign still under review at Start is in no review's
/// evidence, even once released. The page tells the panel it exists, as a live count beside the frozen list and never
/// as a row in it, and tells the chair at the Start button.
/// </remarks>
public sealed partial class ReviewDetailMsfOutsideSnapshotTests : TestContext
{
    private const string NotYetReleased = "not yet released";
    private const string ReleasedAfterStart = "released after this review started";
    private const string StartedEmpty = "The review window held no evidence when this review started.";

    public ReviewDetailMsfOutsideSnapshotTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles("CommitteeMember");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));

        // The Start dialog opens and closes, and a result takes the focus (T361).
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled)]
    [InlineData(CommitteeReviewState.InProgress)]
    public void NothingOutsideTheSnapshot_SaysNothing(CommitteeReviewState state)
    {
        var (cut, text) = Render(state, MsfCampaignsOutsideSnapshotDto.None);

        text.Should().NotContain(NotYetReleased);
        text.Replace(StartDialogText(cut), string.Empty, StringComparison.Ordinal).Should().NotContain("feedback campaign");
    }

    [Fact]
    public void OneCampaign_IsCountedInTheSingular()
    {
        var (_, text) = Render(CommitteeReviewState.InProgress, new MsfCampaignsOutsideSnapshotDto(1, 0));

        text.Should().Contain("1 feedback campaign that closed in this window is not yet released.");
        text.Should().Contain("The snapshot holds only the campaigns released before the review started, so that campaign is not in it.");
    }

    [Fact]
    public void SeveralCampaigns_AreCountedInThePlural()
    {
        var (_, text) = Render(CommitteeReviewState.InProgress, new MsfCampaignsOutsideSnapshotDto(2, 0));

        text.Should().Contain("2 feedback campaigns that closed in this window are not yet released.");
        text.Should().Contain("The snapshot holds only the campaigns released before the review started, so those campaigns are not in it.");
    }

    [Fact]
    public void ACampaignReleasedAfterStart_IsStillReported()
    {
        var (_, text) = Render(CommitteeReviewState.InProgress, new MsfCampaignsOutsideSnapshotDto(0, 1));

        text.Should().Contain("1 feedback campaign that closed in this window was released after this review started.");
        text.Should().Contain("so that campaign is not in it.");
        text.Should().NotContain(NotYetReleased);
    }

    [Fact]
    public void BothKinds_AreReportedTogether()
    {
        var (cut, text) = Render(CommitteeReviewState.Decided, new MsfCampaignsOutsideSnapshotDto(1, 2));

        text.Should().Contain("1 feedback campaign that closed in this window is not yet released.");
        text.Should().Contain("2 feedback campaigns that closed in this window were released after this review started.");
        text.Should().Contain("so those campaigns are not in it.", "three campaigns are missing, not one");
        cut.FindAll(".alert").Should().ContainSingle(alert => alert.TextContent.Contains(ReleasedAfterStart));
    }

    [Fact]
    public void BeforeStart_TheChairIsToldStartingLeavesThemOut()
    {
        var (_, text) = Render(CommitteeReviewState.Scheduled, new MsfCampaignsOutsideSnapshotDto(2, 0));

        text.Should().Contain("2 feedback campaigns that closed in this window are not yet released.");
        text.Should().Contain("Starting the review now leaves those campaigns out of the snapshot.");
        text.Should().NotContain("released before the review started", "there is no snapshot before Start");
    }

    /// <summary>
    /// The Evidence snapshot card can be far below Start on a narrow screen, so the warning is also said where Start is
    /// confirmed: since T361 (round 3 item 11) in the Start dialog, from the preview of what Start freezes and leaves out.
    /// </summary>
    [Theory]
    [InlineData(1, "Starting now leaves out 1 feedback campaign that closed in this window and is not yet released.")]
    [InlineData(3, "Starting now leaves out 3 feedback campaigns that closed in this window and are not yet released.")]
    public void BeforeStart_TheStartDialogCarriesTheWarning(int awaiting, string expected)
    {
        var (cut, _) = Render(CommitteeReviewState.Scheduled, new MsfCampaignsOutsideSnapshotDto(awaiting, 0), leftOut: awaiting);

        cut.FindAll("button").Single(button => button.Closest("dialog") is null && button.TextContent.Trim() == "Start review…").Click();

        StartDialogParagraphs(cut).Should().HaveCount(2).And.EndWith(expected);
    }

    [Fact]
    public void BeforeStart_WithNothingAwaiting_TheStartDialogSaysNothingOfIt()
    {
        var (cut, _) = Render(CommitteeReviewState.Scheduled, MsfCampaignsOutsideSnapshotDto.None);

        cut.FindAll("button").Single(button => button.Closest("dialog") is null && button.TextContent.Trim() == "Start review…").Click();

        StartDialogParagraphs(cut).Should().ContainSingle().Which.Should().NotContain("feedback campaign");
    }

    [Fact]
    public void TheNotice_IsNotEvidence()
    {
        // A started review whose snapshot is empty is still empty with a campaign missing: the notice sits beside the
        // list, never in it.
        var (cut, text) = Render(CommitteeReviewState.InProgress, new MsfCampaignsOutsideSnapshotDto(1, 1));

        text.Should().Contain(StartedEmpty);
        cut.FindAll("section.detail-card li").Should().NotContain(item =>
            item.TextContent.Contains(NotYetReleased) || item.TextContent.Contains(ReleasedAfterStart));
        cut.FindAll(".alert").Should().ContainSingle(alert => alert.TextContent.Contains(NotYetReleased));
    }

    [Fact]
    public void TheCount_IsAskedForThisReviewAsTheSignedInUser()
    {
        var sender = new FakeSender(Review(CommitteeReviewState.InProgress), new MsfCampaignsOutsideSnapshotDto(1, 0));
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        var query = sender.CountQueries.Should().ContainSingle().Subject;
        query.ReviewId.Should().Be(30);
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("chair-1");
    }

    /// <summary>
    /// Start freezes the snapshot the counts are measured against, so the page asks again. Here the one campaign
    /// awaiting release when the page loaded was released before the chair clicked, and Start took it: the notice must
    /// not go on saying it is missing.
    /// </summary>
    [Fact]
    public void Starting_ReadsTheCountAgain()
    {
        var sender = new FakeSender(Review(CommitteeReviewState.Scheduled), new MsfCampaignsOutsideSnapshotDto(1, 0))
        {
            AfterStart = (Review(CommitteeReviewState.InProgress), MsfCampaignsOutsideSnapshotDto.None)
        };
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));
        cut.Markup.Should().Contain(NotYetReleased, "the setup: one campaign is awaiting release at load");

        // T361 (round 3 item 22): Start review… opens the dialog, whose Start review starts it.
        cut.FindAll("button").Single(button => button.Closest("dialog") is null && button.TextContent.Trim() == "Start review…").Click();
        cut.FindAll("dialog button").Single(button => button.TextContent.Trim() == "Start review").Click();
        cut.WaitForState(() => cut.Markup.Contains("Review started."));

        sender.CountQueries.Should().HaveCount(2);
        cut.Markup.Should().NotContain(NotYetReleased);
        cut.FindAll(".alert").Should().NotContain(alert => alert.TextContent.Contains("feedback campaign"));
    }

    private (IRenderedComponent<ReviewDetail> Cut, string Text) Render(
        CommitteeReviewState state,
        MsfCampaignsOutsideSnapshotDto outsideSnapshot,
        int leftOut = 0)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(Review(state), outsideSnapshot) { LeftOut = leftOut });

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        // Text, not markup: Razor's line breaks are layout.
        var text = string.Join(" ", cut.Nodes.Select(node => node.TextContent));
        return (cut, Whitespace().Replace(text, " "));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private static IElement StartDialog(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("dialog").Single(dialog => dialog.QuerySelector("h2")!.TextContent.StartsWith("Start ", StringComparison.Ordinal));

    private static IReadOnlyList<string> StartDialogParagraphs(IRenderedComponent<ReviewDetail> cut)
        => StartDialog(cut).QuerySelectorAll("p").Select(p => Whitespace().Replace(p.TextContent, " ").Trim()).ToArray();

    private static string StartDialogText(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("dialog").Count == 0 ? string.Empty : Whitespace().Replace(StartDialog(cut).TextContent, " ");

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state)
    {
        var started = state != CommitteeReviewState.Scheduled;

        return new CommitteeReviewDetailDto(
            30,
            "trainee-1",
            20,
            "Paediatrics CCC",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            new DateOnly(2026, 7, 2),
            state,
            started ? new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc) : null,
            started ? "chair-1" : null,
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
    }

    private sealed class FakeSender : IScopedSender
    {
        private CommitteeReviewDetailDto _review;
        private MsfCampaignsOutsideSnapshotDto _outsideSnapshot;

        public FakeSender(CommitteeReviewDetailDto review, MsfCampaignsOutsideSnapshotDto outsideSnapshot)
        {
            _review = review;
            _outsideSnapshot = outsideSnapshot;
        }

        /// <summary>What the review and the counts are once Start has run, when a test starts the review.</summary>
        public (CommitteeReviewDetailDto Review, MsfCampaignsOutsideSnapshotDto OutsideSnapshot)? AfterStart { get; init; }

        public List<CountMsfCampaignsOutsideSnapshotQuery> CountQueries { get; } = [];

        /// <summary>The campaigns Start would leave out, as the Start dialog's preview reads them (T361, T173).</summary>
        public int LeftOut { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is CountMsfCampaignsOutsideSnapshotQuery countQuery)
            {
                CountQueries.Add(countQuery);
            }

            if (request is StartCommitteeReviewCommand)
            {
                var afterStart = AfterStart ?? throw new NotSupportedException("This test does not start the review.");
                (_review, _outsideSnapshot) = afterStart;
            }

            object? response = request switch
            {
                GetCommitteeReviewByIdQuery or StartCommitteeReviewCommand => _review,
                ListPendingEntrustmentDecisionsForReviewQuery => Array.Empty<PendingEntrustmentDecisionDto>(),
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => _outsideSnapshot,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                GetEntrustmentStandingForTraineeQuery => null,
                GetMsfCoverageForTraineeQuery => null,
                ListStarEpaOptionsForReviewQuery => Array.Empty<StarEpaOptionDto>(),
                GetEntrustmentScalesListQuery => Array.Empty<EntrustmentScaleDto>(),
                GetStartPreviewQuery => new StartPreviewDto(8, 8, 0, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), LeftOut),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

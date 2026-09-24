using System.Security.Claims;
using System.Text.RegularExpressions;
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
/// Each sentence the sampling report puts on the review page states one true thing (T135).
/// </summary>
/// <remarks>
/// The incomplete banner used to blame every gap on read scope. Since T135 two different things make a report
/// incomplete — rows this account may not read, and rows nobody can read — and a third count, rows with no assessor's
/// rating by design, makes it neither. A sentence that is true of one is false of the others, so each renders only
/// when its own count is non-zero.
/// </remarks>
public sealed partial class ReviewDetailSamplingSentenceTests : TestContext
{
    private const string Incomplete = "Sampling figures are incomplete";
    private const string OutOfScope = "outside what your account may read";
    private const string CouldNotBeRead = "could not be read";
    private const string NotCounted = "Not counted:";
    private const string AskAPanelMember = "Ask a panel member with the full record";

    public ReviewDetailSamplingSentenceTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles("CommitteeMember");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
    }

    [Fact]
    public void WithheldRows_AreBlamedOnReadScope_AndOnlyOnReadScope()
    {
        var text = RenderText(Report(withheld: 3, unreadable: 0, unattributed: 0));

        text.Should().Contain(Incomplete);
        text.Should().Contain("3 rated observations in this review window are " + OutOfScope);
        text.Should().Contain(AskAPanelMember + " before you rely on the sampling check.");
        text.Should().NotContain(CouldNotBeRead);
        text.Should().Contain("The concentration check on this page ran without them.");
        text.Should().Contain("that could be counted in the review window");
    }

    [Fact]
    public void UnreadableRows_AreSaidToBeUnreadable_NotOutOfScope()
    {
        var text = RenderText(Report(withheld: 0, unreadable: 1, unattributed: 0));

        text.Should().Contain(Incomplete);
        text.Should().Contain("1 rated observation in this review window " + CouldNotBeRead +
                              ": the EPA, the rating or the assessor is missing or malformed.");
        text.Should().NotContain(OutOfScope, "nothing here is outside this account's read scope");
        text.Should().NotContain(AskAPanelMember, "no panel member has a fuller record of a row nobody can read");
        text.Should().Contain("The concentration check on this page ran without it.");
    }

    [Fact]
    public void BothCauses_EachGetTheirOwnSentence()
    {
        var text = RenderText(Report(withheld: 1, unreadable: 2, unattributed: 0));

        text.Should().Contain("1 rated observation in this review window is " + OutOfScope);
        text.Should().Contain("2 rated observations in this review window " + CouldNotBeRead);
        text.Should().Contain("ran without them", "three rows are missing between the two causes");
    }

    [Fact]
    public void UnattributedRows_LeaveTheReportComplete_AndAreSaidNotToBeCounted()
    {
        var text = RenderText(Report(withheld: 0, unreadable: 0, unattributed: 2));

        text.Should().NotContain(Incomplete, "an MSF record is not missing evidence");
        text.Should().Contain("4 rated observations from 2 distinct assessors in the review window.");
        text.Should().NotContain("that could be counted");
        text.Should().Contain(NotCounted + " 2 records in the review window with no rating by a named assessor");
        text.Should().Contain("the form names nobody for the rating (multi-source feedback, for one), or the rating or the assessor was left blank where the form allows it.");
        text.Should().Contain("The figures above count only ratings a named assessor gave.");
    }

    [Fact]
    public void WithNothingUnattributed_ThereIsNoNotCountedSentence()
    {
        var text = RenderText(Report(withheld: 0, unreadable: 0, unattributed: 0));

        text.Should().NotContain(Incomplete);
        text.Should().NotContain(NotCounted);
    }

    /// <summary>
    /// Nothing counted, some of it unreadable: the banner stands alone. There are no figures to summarise, so no
    /// summary line claims any, and its sentences speak of the check rather than of numbers on the page.
    /// </summary>
    [Fact]
    public void AnIncompleteReportWithNoWarning_ShowsOnlyTheBanner()
    {
        var text = RenderText(Report(withheld: 0, unreadable: 2, unattributed: 1, total: 0, anyWarning: false));

        text.Should().Contain(Incomplete);
        text.Should().Contain("Treat the absence of a concentration warning as unknown rather than clean.");
        text.Should().NotContain("distinct assessor");
        text.Should().NotContain(NotCounted);
    }

    /// <summary>
    /// The trajectory card draws only ratings by a named assessor. Its empty state must not say the trainee has no
    /// rated observations beside a banner counting some it could not read.
    /// </summary>
    [Fact]
    public void AnEmptyTrajectory_SaysWhatItCouldNotChart_NotThatNothingIsOnFile()
    {
        var text = RenderText(Report(withheld: 0, unreadable: 2, unattributed: 0, total: 0, anyWarning: false));

        text.Should().Contain("No rating by a named assessor to chart for this trainee.");
        text.Should().NotContain("No rated observations on file");
    }

    private string RenderText(SamplingConcentrationReportDto report)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(Review(), report));

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        // Text, not markup: the renderer encodes the apostrophe in "assessor's", and Razor's line breaks are layout.
        var text = string.Join(" ", cut.Nodes.Select(node => node.TextContent));
        return Whitespace().Replace(text, " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>By default a report that warns, so the figures and the unattributed note have somewhere to render.</summary>
    private static SamplingConcentrationReportDto Report(
        int withheld,
        int unreadable,
        int unattributed,
        int total = 4,
        bool anyWarning = true)
        => new(
            ReviewId: 30,
            TotalRatedActivities: total,
            DistinctAssessorCount: total == 0 ? 0 : 2,
            AnyWarning: anyWarning,
            PerEpa: anyWarning
                ? [new EpaSamplingConcentrationDto(7, "EPA-07", "Emergency triage", 4, 2, 2, "assessor-a", 3, true, false, true)]
                : [],
            WithheldRatedActivities: withheld,
            UnreadableRatedActivities: unreadable,
            UnattributedRatedActivities: unattributed);

    private static CommitteeReviewDetailDto Review()
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
            [])
        {
            AcademicYear = 2026,
            Semester = 1
        };

    private sealed class FakeSender : IScopedSender
    {
        private readonly CommitteeReviewDetailDto _review;
        private readonly SamplingConcentrationReportDto _report;

        public FakeSender(CommitteeReviewDetailDto review, SamplingConcentrationReportDto report)
        {
            _review = review;
            _report = report;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? response = request switch
            {
                GetCommitteeReviewByIdQuery => _review,
                ListPendingEntrustmentDecisionsForReviewQuery => Array.Empty<PendingEntrustmentDecisionDto>(),
                GetSamplingConcentrationWarningsQuery => _report,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                GetEntrustmentStandingForTraineeQuery => null,
                GetMsfCoverageForTraineeQuery => null,
                ListStarEpaOptionsForReviewQuery => Array.Empty<StarEpaOptionDto>(),
                GetEntrustmentScalesListQuery => Array.Empty<EntrustmentScaleDto>(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

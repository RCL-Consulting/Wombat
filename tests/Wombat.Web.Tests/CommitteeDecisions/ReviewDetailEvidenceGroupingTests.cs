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
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The review page groups the frozen evidence snapshot by EPA and then by instrument, and its STAR picker offers the
/// trainee's curriculum and each EPA's own ladder (T167).
/// </summary>
public sealed class ReviewDetailEvidenceGroupingTests : TestContext
{
    public ReviewDetailEvidenceGroupingTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles("CommitteeMember");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
    }

    [Fact]
    public void TheSnapshot_IsGroupedByEpaInCodeOrder_ThenTheLinesAboutNoSingleEpa_ThenTheOldOnes()
    {
        var cut = RenderPage(Review(
            Line(1, 101, 8, "PAED-002", "Ward round", "CCA", rating: "3b"),
            Line(2, 102, 7, "PAED-001", "Acute admission", "Mini-CEX", rating: "3a"),
            Line(3, 103, 7, "PAED-001", "Acute admission", "MSF", rating: null, isRated: true),
            Campaign(4, 50),
            Legacy(5, 104)));

        Headings(cut).Should().Equal(
            "PAED-001 — Acute admission",
            "PAED-002 — Ward round",
            "Not about a single EPA",
            "Frozen before lines named their EPA");
    }

    [Fact]
    public void WithinAnEpa_TheLinesAreGroupedByInstrument_EachNamingItsRatingEncounterAndState()
    {
        var cut = RenderPage(Review(
            Line(1, 101, 7, "PAED-001", "Acute admission", "Mini-CEX", rating: "3a", observedOn: new DateOnly(2026, 2, 1)),
            Line(2, 102, 7, "PAED-001", "Acute admission", "Reflective exercise", rating: null, isRated: false),
            Line(3, 103, 7, "PAED-001", "Acute admission", "Mini-CEX", rating: null, isRated: true, state: "declined",
                observedOn: new DateOnly(2026, 3, 1))));

        var article = EpaArticle(cut, 7);
        article.QuerySelectorAll("tbody th[scope=rowgroup]").Select(header => header.TextContent.Trim())
            .Should().Equal("Mini-CEX", "Reflective exercise");

        var miniCex = article.QuerySelectorAll("tbody")[0].QuerySelectorAll("tr");
        miniCex.Should().HaveCount(2);
        miniCex[0].QuerySelector("th")!.GetAttribute("rowspan").Should().Be("2");
        Cells(miniCex[0]).Should().Equal("Mini-CEX (Paediatrics) #101", "3a", "2026-02-01", "completed");
        Cells(miniCex[1]).Should().Equal("Mini-CEX (Paediatrics) #103", "Not recorded", "2026-03-01", "declined");

        var reflective = article.QuerySelectorAll("tbody")[1].QuerySelector("tr")!;
        Cells(reflective)[1].Should().Be("Unrated");

        article.QuerySelector("a")!.GetAttribute("href").Should().Be("/activities/101");
        article.QuerySelector("caption")!.TextContent.Should().Be("3 records from 2 instruments");
    }

    [Fact]
    public void AnUndatedLine_SaysItsDateIsTheFilingDay()
    {
        var cut = RenderPage(Review(
            Line(1, 101, 7, "PAED-001", "Acute admission", "Mini-CEX", rating: "3a", declared: false)));

        EpaArticle(cut, 7).TextContent.Should().Contain("2026-02-10 (filed; no encounter date)");
    }

    [Fact]
    public void AnMsfCampaignLine_IsListedWithItsSummary_NotUnderAnEpa()
    {
        var cut = RenderPage(Review(
            Line(1, 101, 7, "PAED-001", "Acute admission", "MSF", rating: null, isRated: true),
            Campaign(2, 50)));

        var across = cut.Find("article[aria-labelledby=evidence-across-epas]");
        across.TextContent.Should().Contain("Annual MSF #50").And.Contain("Evidence recorded for PAED-001, one activity each.");
        EpaArticle(cut, 7).TextContent.Should().NotContain("Annual MSF #50");
        across.TextContent.Should().Contain("its per-EPA records are listed under each EPA above.")
            .And.NotContain("An activity here names no EPA.", "every activity line here names its EPA");
    }

    /// <summary>
    /// A review frozen before T167 has no EPA cards: its campaign's per-EPA records are among the old lines, and the
    /// card must not send the panel looking for EPA cards above that do not exist.
    /// </summary>
    [Fact]
    public void OnAReviewFrozenBeforeLinesNamedTheirEpa_TheCampaignCardPointsAtTheOldLines()
    {
        var cut = RenderPage(Review(Legacy(1, 101), Campaign(2, 50)));

        Headings(cut).Should().Equal("Not about a single EPA", "Frozen before lines named their EPA");
        var across = cut.Find("article[aria-labelledby=evidence-across-epas]");
        across.TextContent.Should().Contain("its per-EPA records are among the lines below")
            .And.NotContain("under each EPA above")
            .And.NotContain("An activity here names no EPA.");
    }

    [Fact]
    public void AnActivityThatNamesNoEpa_IsSaidToNameNone_AndNoCampaignIsMentioned()
    {
        var cut = RenderPage(Review(Line(1, 101, 7, "PAED-001", "Acute admission", "Mini-CEX", rating: "3a") with
        {
            EpaId = null,
            EpaCode = null,
            EpaTitle = null
        }));

        var across = cut.Find("article[aria-labelledby=evidence-across-epas]");
        across.TextContent.Should().Contain("An activity here names no EPA.")
            .And.NotContain("multi-source feedback");
    }

    [Fact]
    public void TheStarPicker_OffersTheCurriculumOptions_AndEachEpasOwnLadder()
    {
        var options = new[]
        {
            new StarEpaOptionDto(7, "PAED-001", "Acute admission", 1, "CPSA ladder"),
            new StarEpaOptionDto(8, "PAED-002", "Ward round", null, null)
        };
        var cut = RenderPage(Review(), options);

        cut.FindAll("#pending-epa option").Select(option => option.TextContent)
            .Should().Equal("Select an EPA…", "PAED-001 — Acute admission", "PAED-002 — Ward round");
        cut.FindAll("#pending-level option").Select(option => option.TextContent)
            .Should().Equal(["Select an EPA first…"], "the ladder is the chosen EPA's");

        cut.Find("#pending-epa").Change("7");
        cut.FindAll("#pending-level optgroup").Select(group => group.GetAttribute("label")).Should().Equal("CPSA ladder");
        cut.FindAll("#pending-level option").Select(option => option.TextContent)
            .Should().Equal("Select a level…", "1", "2", "3a");

        cut.Find("#pending-epa").Change("8");
        cut.FindAll("#pending-level optgroup").Select(group => group.GetAttribute("label"))
            .Should().Equal(["CPSA ladder", "Old ladder"], "an EPA on no ladder may take any scale's rung");
    }

    [Fact]
    public void ATraineeWhoseCurriculumListsNoEpa_IsToldSo()
    {
        var cut = RenderPage(Review(), []);

        cut.Markup.Should().Contain("This trainee's curriculum lists no EPA, so no entrustment decision can be staged.");
    }

    // ---- Fixture -------------------------------------------------------------------------------------------------

    private static IReadOnlyList<string> Headings(IRenderedComponent<ReviewDetail> cut)
        => cut.Find("section[aria-labelledby=evidence-snapshot-heading]")
            .QuerySelectorAll("article h4")
            .Select(heading => heading.TextContent.Trim())
            .ToArray();

    private static IElement EpaArticle(IRenderedComponent<ReviewDetail> cut, int epaId)
        => cut.Find($"article[aria-labelledby=evidence-epa-{epaId}]");

    private static IReadOnlyList<string> Cells(IElement row)
        => row.QuerySelectorAll("td").Select(cell => cell.TextContent.Trim()).ToArray();

    private static CommitteeEvidenceDto Line(
        int id,
        int activityId,
        int epaId,
        string epaCode,
        string epaTitle,
        string instrument,
        string? rating,
        bool isRated = true,
        string state = "completed",
        DateOnly? observedOn = null,
        bool declared = true)
        => new(
            id,
            CommitteeEvidenceSourceType.Activity,
            activityId,
            null,
            null,
            $"{instrument} (Paediatrics) #{activityId}",
            $"State: {state}.",
            new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            EpaId: epaId,
            EpaCode: epaCode,
            EpaTitle: epaTitle,
            InstrumentName: instrument,
            IsRatedInstrument: isRated,
            RatingLabel: rating,
            ObservedOn: observedOn ?? new DateOnly(2026, 2, 10),
            ObservedOnDeclared: declared,
            SourceState: state);

    private static CommitteeEvidenceDto Campaign(int id, int campaignId)
        => new(
            id,
            CommitteeEvidenceSourceType.MsfCampaign,
            null,
            campaignId,
            null,
            $"Annual MSF #{campaignId}",
            "State: Released; responses 8; closed 2026-02-10. Evidence recorded for PAED-001, one activity each.",
            new DateTime(2026, 2, 12, 8, 0, 0, DateTimeKind.Utc),
            SourceState: "Released");

    /// <summary>A line frozen before T167: no EPA, instrument, rating or encounter date.</summary>
    private static CommitteeEvidenceDto Legacy(int id, int activityId)
        => new(
            id,
            CommitteeEvidenceSourceType.Activity,
            activityId,
            null,
            null,
            $"Mini-CEX #{activityId}",
            "State: completed; created 2026-02-01; updated 2026-02-02 08:00 UTC.",
            new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc));

    private IRenderedComponent<ReviewDetail> RenderPage(
        CommitteeReviewDetailDto review,
        IReadOnlyList<StarEpaOptionDto>? starEpas = null)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(review, starEpas ?? []));

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, review.Id));
        cut.WaitForState(() => cut.Markup.Contains("Evidence snapshot"));

        return cut;
    }

    private static CommitteeReviewDetailDto Review(params CommitteeEvidenceDto[] evidence)
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
            evidence);

    private sealed class FakeSender : IScopedSender
    {
        private static readonly IReadOnlyList<EntrustmentScaleDto> Scales =
        [
            new EntrustmentScaleDto(1, "CPSA ladder", null,
            [
                new EntrustmentLevelDto(11, 1, "1", null),
                new EntrustmentLevelDto(12, 2, "2", null),
                new EntrustmentLevelDto(13, 3, "3a", null)
            ]),
            new EntrustmentScaleDto(2, "Old ladder", null,
            [
                new EntrustmentLevelDto(21, 1, "Observe", null)
            ])
        ];

        private readonly CommitteeReviewDetailDto _review;
        private readonly IReadOnlyList<StarEpaOptionDto> _starEpas;

        public FakeSender(CommitteeReviewDetailDto review, IReadOnlyList<StarEpaOptionDto> starEpas)
        {
            _review = review;
            _starEpas = starEpas;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? response = request switch
            {
                GetCommitteeReviewByIdQuery => _review,
                ListPendingEntrustmentDecisionsForReviewQuery => Array.Empty<PendingEntrustmentDecisionDto>(),
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                GetEntrustmentStandingForTraineeQuery => null,
                ListStarEpaOptionsForReviewQuery => _starEpas,
                GetEntrustmentScalesListQuery => Scales,
                GetMsfCoverageForTraineeQuery => null,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

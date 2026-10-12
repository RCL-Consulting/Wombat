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
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The staging form names the lines of the frozen snapshot a decision rests on: grouped as the snapshot groups them, the
/// chosen EPA's lines first and the others behind "Show the other N snapshot lines", never a supervisor report; and Stage
/// pending decision stays unavailable, with its reason said, until one is ticked (D38, T131). Since T361 (round 3 items
/// 16, 20, 30) the form is the agenda's foot form, "Stage a decision on an EPA not on this agenda" (an open line's form is
/// the same component, with its line's EPA), and Stage is aria-disabled, never natively disabled.
/// </summary>
public sealed class ReviewDetailEvidencePickerTests : TestContext
{
    private const string ReasonId = "stage-0-reason";

    public ReviewDetailEvidencePickerTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles("CommitteeMember");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ThePicker_OffersEveryLineADecisionMayRestOn_GroupedByEpa_AndNoSupervisorReport()
    {
        var (cut, _) = RenderPage();

        Groups(cut).Should().Equal("PAED-001 — Acute admission", "PAED-002 — Ward round", "Not about a single EPA");
        CheckboxIds(cut).Should().BeEquivalentTo(new[] { "stage-0-evidence-1", "stage-0-evidence-2", "stage-0-evidence-3" });
        cut.FindAll("#stage-0-evidence-4").Should().BeEmpty("a supervisor report cannot ground a decision");
        // The state by its frozen label (T220).
        cut.Find("label[for=stage-0-evidence-1]").TextContent.Should().Contain("Mini-CEX #101").And.Contain("Rated 3a · 2026-02-10 · Completed");
    }

    [Fact]
    public void ThePicker_IsOneFieldAmongTheForms()
    {
        // T188. The picker is one field of the staging form, so a form group: its legend reads as a label, like Rationale's
        // beside it, not as a section title. Its groups by EPA are labels inside it, not fieldsets of their own (T361,
        // round 3 item 30).
        var (cut, _) = RenderPage();

        var picker = Picker(cut);
        picker.ClassList.Should().Contain(["form-group", "full-width", "snapshot-pick"]);
        picker.ParentElement!.ClassList.Should().Contain("form-grid", "it sits among the form's fields");
        picker.QuerySelectorAll("fieldset").Should().BeEmpty();
    }

    [Fact]
    public void ThePickersLegend_IsMarkedRequired()
    {
        // T188 review. Read as a label beside "Rationale *", the legend said nothing of what Stage waits for: at least one
        // line (D38; StagePendingEntrustmentDecision refuses none). It carries the mark FormField gives a required field.
        // Its help paragraph went with round 3 (item 30): the boards draw none.
        var (cut, _) = RenderPage();

        var legend = Picker(cut).Children.First(child => child.LocalName == "legend");
        legend.QuerySelector("span[aria-hidden=true]")!.TextContent.Should().Be("*");
        legend.QuerySelector("span.visually-hidden")!.TextContent.Should().Be("required");
    }

    /// <summary>
    /// An activity whose type names no EPA is listed under "Not about a single EPA", and its detail is its state by the
    /// label frozen with it, never the key (T220 review): a submitted clinical audit reads "Awaiting supervisor".
    /// </summary>
    [Fact]
    public void ALineAboutNoSingleEpa_NamesItsStateByItsLabel()
    {
        var noEpa = Line(5, 105, 7, "PAED-001", "Acute admission") with
        {
            EpaId = null,
            EpaCode = null,
            EpaTitle = null,
            SourceLabel = "Clinical Audit #105",
            SourceState = "submitted",
            SourceStateLabel = "Awaiting supervisor",
            SourceFinished = false
        };

        var (cut, _) = RenderPage(evidence: [Line(1, 101, 7, "PAED-001", "Acute admission"), noEpa]);

        Groups(cut).Should().Equal("PAED-001 — Acute admission", "Not about a single EPA");
        var label = cut.Find("label[for=stage-0-evidence-5]").TextContent;
        label.Should().Contain("Clinical Audit #105").And.Contain("Awaiting supervisor");
        label.Should().NotContain("submitted");
    }

    [Fact]
    public void EachLabel_ReadsWithASpaceBeforeItsDetail()
    {
        // T212: the space before the detail stood alone before an @if, where Razor drops whitespace, so a label read
        // "Mini-CEX #101· 3a · …". Checked on the label's own text, not with Contain, which passed it. Round 3 item 30's
        // words: "Rated 3a", and a campaign report by its state alone.
        var (cut, _) = RenderPage();

        LabelText(cut, 1).Should().Be("Mini-CEX #101 Rated 3a · 2026-02-10 · Completed", "the state by its frozen label (T220)");
        LabelText(cut, 3).Should().Be("Annual MSF #50 Released");
    }

    [Fact]
    public void ChoosingAnEpa_ListsItsLinesFirst_AndTheOthersBehindAButton_WithoutDroppingThem()
    {
        // T361 (round 3 items 20, 30): the chosen EPA's lines first; the others behind a button with aria-expanded.
        var (cut, _) = RenderPage();

        cut.Find("#stage-0-epa").Change("8");

        CheckboxIds(cut).Should().Equal("stage-0-evidence-2");
        var others = cut.Find("#stage-0-others");
        others.TextContent.Trim().Should().Be("Show the other 2 snapshot lines");
        others.GetAttribute("aria-expanded").Should().Be("false");

        others.Click();

        Groups(cut).Should().Equal("PAED-001 — Acute admission", "Not about a single EPA");
        CheckboxIds(cut).Should().HaveCount(3);
    }

    [Fact]
    public void Stage_IsUnavailableUntilAnItemIsTicked_AndSaysWhy()
    {
        // T361 (round 3 item 16): aria-disabled and .is-unavailable, never natively disabled; its reason beside it.
        var (cut, _) = RenderPage();

        var stage = StageButton(cut);
        stage.HasAttribute("disabled").Should().BeFalse();
        stage.GetAttribute("aria-disabled").Should().Be("true");
        stage.ClassList.Should().Contain("is-unavailable");
        stage.GetAttribute("aria-describedby").Should().Be(ReasonId);
        cut.Find($"#{ReasonId}").TextContent.Should().Be("Name at least one item of the evidence snapshot.");

        cut.Find("#stage-0-evidence-1").Change(true);

        StageButton(cut).HasAttribute("aria-disabled").Should().BeFalse();
        StageButton(cut).HasAttribute("aria-describedby").Should().BeFalse();
        cut.FindAll($"#{ReasonId}").Should().BeEmpty();

        cut.Find("#stage-0-evidence-1").Change(false);
        StageButton(cut).GetAttribute("aria-disabled").Should().Be("true", "unticking the only item takes the evidence away again");
    }

    [Fact]
    public void OneNamedItem_OrNoneAboutTheChosenEpa_IsSaid_NeverRefused()
    {
        var (cut, _) = RenderPage();
        cut.Find("#stage-0-epa").Change("8");
        cut.Find("#stage-0-others").Click();

        cut.Find("#stage-0-evidence-1").Change(true);

        var hints = Hints(cut);
        hints.Should().Contain(hint => hint.Contains("never taken from a single form"));
        hints.Should().Contain("None of the named items is about PAED-002.");
        StageButton(cut).HasAttribute("aria-disabled").Should().BeFalse();

        cut.Find("#stage-0-evidence-2").Change(true);

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
        cut.Find("#stage-0-epa").Change("7");

        cut.Find("#stage-0-evidence-2").Change(true);
        cut.Find("#stage-0-evidence-3").Change(true);

        Hints(cut).Should().Equal(Wombat.Web.Components.Pages.CommitteeDecisions.Review.ReviewWords.NoFinishedItem);
        StageButton(cut).HasAttribute("aria-disabled").Should().BeFalse("it is said, not refused");

        cut.Find("#stage-0-evidence-1").Change(true);
        Hints(cut).Should().BeEmpty("one named item was finished");

        cut.Find("#stage-0-evidence-1").Change(false);
        cut.Find("#stage-0-evidence-4").Change(true);
        Hints(cut).Should().BeEmpty("a line frozen before T131 does not say whether it was finished");
    }

    [Fact]
    public void Staging_SendsTheTickedLines()
    {
        var (cut, sender) = RenderPage();
        cut.Find("#stage-0-epa").Change("7");
        cut.Find("#stage-0-level").Change("13");
        cut.Find("#stage-0-rationale").Change("Consistent across the window.");
        cut.Find("#stage-0-evidence-1").Change(true);
        cut.Find("#stage-0-others").Click();
        cut.Find("#stage-0-evidence-3").Change(true);

        StageButton(cut).Click();

        cut.WaitForState(() => sender.Staged is not null);
        sender.Staged!.EvidenceItemIds.Should().Equal(1, 3);
        sender.Staged.EpaId.Should().Be(7);

        // The staged decisions are read again once the decision is staged (T212), so its line says what it rests on.
        cut.WaitForState(() => cut.Markup.Contains("Pending entrustment decision staged."));
        Text(cut.Find("#agenda-line-7")).Should().Contain("Rests on 2 items of the snapshot: Mini-CEX #101; Annual MSF #50.");
    }

    [Fact]
    public void AStagedDecision_SaysWhatItRestsOn()
    {
        var pending = new PendingEntrustmentDecisionDto(
            90, 30, 7, "PAED-001", "Acute admission", 13, "3a", new DateOnly(2026, 7, 2), null, "Consistent.",
            [1, 3], new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1");

        var (cut, _) = RenderPage(pending);

        Text(cut.Find("#agenda-line-7")).Should().Contain("Rests on 2 items of the snapshot: Mini-CEX #101; Annual MSF #50.");
    }

    [Fact]
    public void ASnapshotWithNothingADecisionCanRestOn_SaysSo_AndStageStaysUnavailable()
    {
        var (cut, _) = RenderPage(evidence: [Report(4)]);

        cut.Markup.Should().Contain("The evidence snapshot holds no line a decision can rest on");
        StageButton(cut).GetAttribute("aria-disabled").Should().Be("true");
    }

    // ---- Fixture -------------------------------------------------------------------------------------------------

    private static AngleSharp.Dom.IElement Picker(IRenderedComponent<ReviewDetail> cut)
        => cut.Find("#agenda-foot").QuerySelector("fieldset.snapshot-pick")!;

    private static AngleSharp.Dom.IElement StageButton(IRenderedComponent<ReviewDetail> cut)
        => cut.Find("#stage-0-submit");

    private static IReadOnlyList<string> Groups(IRenderedComponent<ReviewDetail> cut)
        => Picker(cut).QuerySelectorAll(".snapshot-pick-group").Select(group => group.TextContent.Trim()).ToArray();

    private static IReadOnlyList<string> CheckboxIds(IRenderedComponent<ReviewDetail> cut)
        => Picker(cut).QuerySelectorAll("input[type=checkbox]").Select(input => input.Id ?? string.Empty).ToArray();

    private static string Text(AngleSharp.Dom.IElement element)
        => System.Text.RegularExpressions.Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>The label's text as a reader gets it: runs of whitespace as one space, and no space where none is.</summary>
    private static string LabelText(IRenderedComponent<ReviewDetail> cut, int evidenceId)
        => Text(cut.Find($"label[for=stage-0-evidence-{evidenceId}]"));

    private static IReadOnlyList<string> Hints(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("#stage-0-hints .field-warning").Select(hint => hint.TextContent.Trim()).ToArray();

    private (IRenderedComponent<ReviewDetail> Cut, FakeSender Sender) RenderPage(
        PendingEntrustmentDecisionDto? pending = null,
        IReadOnlyList<CommitteeEvidenceDto>? evidence = null)
    {
        var review = Review(evidence ?? [Line(1, 101, 7, "PAED-001", "Acute admission"), Line(2, 102, 8, "PAED-002", "Ward round"), Campaign(3, 50), Report(4)]);
        var sender = new FakeSender(review, pending is null ? [] : [pending]);
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, review.Id));
        cut.WaitForState(() => cut.FindAll("#agenda-foot").Count > 0);
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
            SourceFinished: true,
            SourceStateLabel: "Completed");

    private static CommitteeEvidenceDto Unfinished(CommitteeEvidenceDto line, string state)
        => line with { SourceState = state, SourceStateLabel = state, RatingLabel = null, Summary = $"State: {state}.", SourceFinished = false };

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
            SourceFinished: true,
            SourceStateLabel: "Released");

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
            CallerResolvesAppeals = true,
            // A line each for the two EPAs, so a staged decision shows in its line (T361, round 3 item 30).
            Agenda = new CommitteeAgendaDto(30, 2026, 1, "2026 S1", false,
            [
                new CommitteeAgendaLineDto(107, 7, "PAED-001", "Acute admission", CommitteeAgendaLineOrigin.Cadence, 2026, 1,
                    "2026 S1", true, false, CommitteeAgendaLineState.Due, CommitteeAgendaLineStatus.Due, false, false, null, 1),
                new CommitteeAgendaLineDto(108, 8, "PAED-002", "Ward round", CommitteeAgendaLineOrigin.Cadence, 2026, 1,
                    "2026 S1", true, false, CommitteeAgendaLineState.Due, CommitteeAgendaLineStatus.Due, false, false, null, 1)
            ], [])
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
                ListPendingEntrustmentDecisionsForReviewQuery => _staged is null ? _pending : _pending.Append(_staged).ToArray(),
                GetSamplingConcentrationWarningsQuery => null,
                CountMsfCampaignsOutsideSnapshotQuery => MsfCampaignsOutsideSnapshotDto.None,
                GetEpaTrajectoryForTraineeQuery => Array.Empty<EpaTrajectoryDto>(),
                ListStarEpaOptionsForReviewQuery => StarEpas,
                GetEntrustmentScalesListQuery => Scales,
                StagePendingEntrustmentDecisionCommand stage => Stage(stage),
                GetEntrustmentStandingForTraineeQuery => null,
                GetMsfCoverageForTraineeQuery => null,
                GetStartPreviewQuery => null,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private PendingEntrustmentDecisionDto? _staged;

        private PendingEntrustmentDecisionDto Stage(StagePendingEntrustmentDecisionCommand command)
        {
            Staged = command;
            return _staged = new PendingEntrustmentDecisionDto(
                91, command.ReviewId, command.EpaId, "PAED-001", "Acute admission", command.AuthorisedLevelId, "3a",
                command.IssuedOn, command.ExpiresOn, command.Rationale, command.EvidenceItemIds, DateTime.UtcNow, "chair-1");
        }
    }
}

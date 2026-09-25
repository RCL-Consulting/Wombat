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
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The review page's agenda (T131 slice 4): above the staging form, each line with its window, a state badge, the
/// evidence about it and its action; Ratify disabled with the closing lines still outstanding named (T107); what another
/// panel still owes, read-only, and a warning on the progression decision while it does (O8).
/// </summary>
public sealed partial class ReviewDetailAgendaTests : TestContext
{
    private readonly RecordingSender _sender = new();

    public ReviewDetailAgendaTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("chair@test");
        auth.SetRoles(WombatRoles.CommitteeMember);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "chair-1"));
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void TheAgenda_ListsEachLine_WithItsWindow_ItsStateBadge_AndTheEvidenceAboutIt()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null)),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Rotation moved.")));

        var rows = cut.FindAll("#agenda-heading ~ .table-container tbody tr");
        rows.Should().HaveCount(4);
        Row(cut, 1).QuerySelector(".badge")!.ClassList.Should().Contain("badge-draft");
        Text(Row(cut, 1)).Should().Contain("PAED-001 — EPA 1").And.Contain("2026 S1").And.Contain("Due")
            .And.Contain("Must be decided at this sitting, or deferred with a reason.").And.Contain("2 items");
        Row(cut, 2).QuerySelector(".badge")!.ClassList.Should().Contain("badge-submitted");
        Text(Row(cut, 3)).Should().Contain("2026").And.Contain("Due by year end");
        Row(cut, 4).QuerySelector(".badge")!.ClassList.Should().Contain("badge-accepted");
        Text(Row(cut, 4)).Should().Contain("Reason: Rotation moved.");

        var caption = cut.Find("#agenda-heading ~ .table-container caption").TextContent;
        caption.Should().Be("4 EPAs for 2026 S1, 1 still to stage or defer");

        // The card sits above the staging form.
        var cards = cut.FindAll("section.detail-card h3").Select(heading => heading.TextContent.Trim()).ToList();
        cards.IndexOf("Agenda").Should().BeLessThan(cards.IndexOf("Pending entrustment decisions"));
    }

    [Fact]
    public void EachLine_OffersWhatItsStateAllows()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")));

        Buttons(Row(cut, 1)).Should().Equal("Stage", "Defer");
        Buttons(Row(cut, 2)).Should().BeEmpty("a staged line is removed from the staged list below, not here");
        Buttons(Row(cut, 4)).Should().Equal("Reinstate");
    }

    [Fact]
    public void Stage_ChoosesTheLinesEpaInTheStagingForm()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001"), Due(2, "PAED-002")));

        ButtonIn(Row(cut, 2), "Stage").Click();

        cut.Find("#pending-epa").GetAttribute("value").Should().Be("2");
    }

    [Fact]
    public void Defer_AsksWhy_SendsTheReason_AndReadsTheAgendaAgain()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));
        _sender.On<DeferAgendaLineCommand>(_ => Unit.Value)
            .On<GetCommitteeAgendaQuery>(_ => Agenda(Line(1, "PAED-001", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Not observed.")));

        ButtonIn(Row(cut, 1), "Defer").Click();
        // The deferral's own fieldset: the decision form above it has one too, for who was present (T165).
        cut.Find("#deferral-reason").Closest("fieldset")!.QuerySelector("legend")!.TextContent.Should().Contain("Defer PAED-001");
        cut.Find("#deferral-reason").Change("Not observed.");
        cut.Find("#deferral-reason").Closest("form")!.Submit();

        cut.WaitForState(() => cut.Markup.Contains("PAED-001 deferred."));
        var command = _sender.Received.OfType<DeferAgendaLineCommand>().Should().ContainSingle().Subject;
        command.LineId.Should().Be(101);
        command.Reason.Should().Be("Not observed.");
        _sender.Received.OfType<GetCommitteeAgendaQuery>().Should().ContainSingle();
        Text(Row(cut, 1)).Should().Contain("Deferred").And.Contain("Reason: Not observed.");
        cut.FindAll("#deferral-reason").Should().BeEmpty("the form closes once the line is deferred");
    }

    [Fact]
    public void Defer_MovesTheFocusToTheReason_AsStageMovesItToTheEpa()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        ButtonIn(Row(cut, 1), "Defer").Click();

        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<Microsoft.AspNetCore.Components.ElementReference>()
            .Which.ShouldBeElementReferenceTo(cut.Find("#deferral-reason")));
    }

    [Fact]
    public void Reinstate_SendsTheCommand()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")));
        _sender.On<ReinstateAgendaLineCommand>(_ => Unit.Value).On<GetCommitteeAgendaQuery>(_ => Agenda(Due(4, "PAED-004")));

        ButtonIn(Row(cut, 4), "Reinstate").Click();

        cut.WaitForState(() => cut.Markup.Contains("PAED-004 reinstated"));
        _sender.Received.OfType<ReinstateAgendaLineCommand>().Single().LineId.Should().Be(104);
    }

    [Fact]
    public void OnceTheDecisionIsRecorded_OnlyALineThatBlocksRatify_OffersAnything_AndThatIsDefer()
    {
        // The agenda is fixed with the recorded decision (T165). The one line still open is a closing line whose staged
        // decision was removed afterwards because it no longer fits the curriculum: deferring it is the way through.
        var cut = Render(CommitteeReviewState.Decided, Agenda(
            Due(1, "PAED-001"),
            Staged(2, "PAED-002"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null)),
            Line(4, "PAED-004", CommitteeAgendaLineStatus.Deferred, CommitteeAgendaLineState.Deferred, reason: "Later.")));

        Buttons(Row(cut, 1)).Should().Equal("Defer");
        Buttons(Row(cut, 2)).Should().BeEmpty();
        Buttons(Row(cut, 3)).Should().BeEmpty("an optional line is not deferred after the decision is recorded");
        Buttons(Row(cut, 4)).Should().BeEmpty("a deferral is not taken back after the decision is recorded");
    }

    [Fact]
    public void Record_IsDisabled_WhileAClosingLineIsNeitherStagedNorDeferred_AndSaysWhich()
    {
        // The decision fixes the agenda (T165), so it is recorded only once every closing line is staged or deferred.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001"), Staged(2, "PAED-002")), seatsAQuorum: true);

        var record = RecordButton(cut);
        record.HasAttribute("disabled").Should().BeTrue();
        record.GetAttribute("aria-describedby").Should().Be("record-reason");
        Text(cut.Find("#record-reason")).Should().Be(
            "Record decision: PAED-001 must be decided at this sitting. Stage a decision on it, or defer it with a reason.");
    }

    [Fact]
    public void Record_IsOffered_OnceEveryClosingLineIsStagedOrDeferred()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(
            Staged(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null))),
            seatsAQuorum: true);

        var record = RecordButton(cut);
        record.HasAttribute("disabled").Should().BeFalse();
        record.GetAttribute("type").Should().Be("submit");
        cut.FindAll("#record-reason").Should().BeEmpty();
    }

    [Fact]
    public void Ratify_IsDisabled_WhileAClosingLineIsNeitherStagedNorDeferred_AndSaysWhich()
    {
        var cut = Render(CommitteeReviewState.Decided, Agenda(Due(1, "PAED-001"), Due(5, "PAED-005"), Staged(2, "PAED-002")));

        var ratify = RatifyButton(cut);
        ratify.HasAttribute("disabled").Should().BeTrue();
        ratify.GetAttribute("aria-describedby").Should().Be("ratify-reason");
        cut.Find("#ratify-reason").TextContent.Should().Be(
            "Ratify: PAED-001 and PAED-005 must be decided at this sitting. Stage a decision on each, or defer it with a reason.");
    }

    [Fact]
    public void Ratify_IsEnabled_OnceEveryClosingLineIsStagedOrDeferred()
    {
        var cut = Render(CommitteeReviewState.Decided, Agenda(
            Staged(1, "PAED-001"),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.DueByYearEnd, CommitteeAgendaLineState.Due, window: (2026, null))));

        RatifyButton(cut).HasAttribute("disabled").Should().BeFalse();
        cut.FindAll("#ratify-reason").Should().BeEmpty();
    }

    [Fact]
    public void WhatAnotherPanelDecides_IsListedReadOnly_AndTheProgressionDecisionWarnsWhileItIsUndecided()
    {
        var agenda = Agenda(Due(1, "PAED-001")) with
        {
            RoutedElsewhere =
            [
                new CommitteeAgendaElsewhereDto(4, "PAED-004", "Newborn", 11, "Neonatal CCC", "2026 S1", CommitteeAgendaElsewhereStatus.NotYetDecided),
                new CommitteeAgendaElsewhereDto(5, "PAED-005", "Sick newborn", 11, "Neonatal CCC", "2026 S1", CommitteeAgendaElsewhereStatus.Missed)
            ]
        };

        var cut = Render(CommitteeReviewState.InProgress, agenda);

        Text(cut.Find("#agenda-elsewhere-4")).Should().Contain("PAED-004 — Newborn").And.Contain("Neonatal CCC · 2026 S1").And.Contain("Not yet decided");
        cut.Find("#agenda-elsewhere-5 .badge").ClassList.Should().Contain("badge-declined");
        cut.Find("#agenda-elsewhere-5 .badge").TextContent.Should().Be("Missed");
        cut.Find("#agenda-elsewhere-4").QuerySelectorAll("button").Should().BeEmpty();
        cut.Find("#sitting-order-warning").TextContent.Should().Be(
            "PAED-004 and PAED-005 are decided by Neonatal CCC and not yet decided for 2026 S1. A progression decision " +
            "recorded now is taken without them.");
    }

    [Fact]
    public void WhatAStarAlreadyDecidedInTheWindow_IsNamedInTheAgendaCard_InThePreviewsWords()
    {
        // T215: the planner left these off because a STAR already decided their window; the card says so, as the
        // scheduling preview did, so the chair is not left wondering where PAED-001 went.
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(2, "PAED-002")) with { DecidedInWindow = ["PAED-001", "PAED-006"] });

        var note = cut.Find("#agenda-decided-in-window");
        note.TextContent.Should().Be("Already decided in this window, so not on the agenda: PAED-001 and PAED-006.");
        note.ClassList.Should().Contain("muted");
        note.Closest("section")!.QuerySelector("h3")!.TextContent.Should().Be("Agenda");
        cut.FindAll("#agenda-line-1").Should().BeEmpty();
    }

    [Fact]
    public void AnAgendaWithNothingAlreadyDecided_SaysNothingOfIt()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda(Due(1, "PAED-001")));

        cut.FindAll("#agenda-decided-in-window").Should().BeEmpty();
        cut.Markup.Should().NotContain("Already decided");
    }

    [Fact]
    public void AReviewWithNothingOnItsAgenda_SaysWhatStagingDoes()
    {
        var cut = Render(CommitteeReviewState.InProgress, Agenda());

        Text(cut.Find("#agenda-heading").ParentElement!).Should().Contain(
            "Nothing is on this review's agenda. A decision can still be staged on any EPA of the trainee's curriculum that routes to this panel, and staging it adds it.");
        RatifyButtonOrNull(cut).Should().BeNull("the review is not decided yet");
    }

    [Fact]
    public void AScheduledReviewWithNothingOnItsAgenda_SaysStartAddsOnlyWhatIsNotAlreadyDecided()
    {
        // T215 review: beside "Already decided in this window", the empty agenda must not promise that Start adds them.
        var cut = Render(CommitteeReviewState.Scheduled, Agenda() with { DecidedInWindow = ["PAED-001", "PAED-002"] });

        var card = Text(cut.Find("#agenda-heading").ParentElement!);
        card.Should().Contain(
                "Nothing is on the agenda yet. Starting the review adds every EPA routed to this panel that is due in its " +
                "period and not already decided in its window.")
            .And.Contain("Already decided in this window, so not on the agenda: PAED-001 and PAED-002.");
    }

    [Fact]
    public void AFormativeReview_ShowsNoAgenda()
    {
        var cut = Render(CommitteeReviewState.InProgress, null, formative: true);

        cut.FindAll("#agenda-heading").Should().BeEmpty();
    }

    // ---- Fixture ------------------------------------------------------------------------------------------------------

    /// <param name="seatsAQuorum">
    /// The panel's chair and one member may both sit (T165), so the panel is no reason to refuse Record; otherwise the
    /// review names no panel member.
    /// </param>
    private IRenderedComponent<ReviewDetail> Render(
        CommitteeReviewState state, CommitteeAgendaDto? agenda, bool formative = false, bool seatsAQuorum = false)
    {
        var review = new CommitteeReviewDetailDto(
            30, "trainee-1", 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 2),
            state, new DateTime(2026, 7, 2, 9, 0, 0, DateTimeKind.Utc), "chair-1", null, null, null, [], [],
            [
                Evidence(501, 1), Evidence(502, 1), Evidence(503, 2)
            ],
            IsFormative: formative)
        {
            AcademicYear = 2026,
            Semester = 1,
            // The page offers the chair's controls by what the query says the caller may do (T213); these tests act as the
            // chair.
            CallerChairs = true,
            CallerResolvesAppeals = true,
            Agenda = agenda,
            PanelMembers = seatsAQuorum
                ?
                [
                    new CommitteePersonDto("chair-1", DecisionPanelMemberRole.Chair) { MaySit = true },
                    new CommitteePersonDto("member-1", DecisionPanelMemberRole.Member) { MaySit = true }
                ]
                : []
        };

        _sender
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => Array.Empty<PendingEntrustmentDecisionDto>())
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => new[]
            {
                new StarEpaOptionDto(1, "PAED-001", "EPA 1", null, null),
                new StarEpaOptionDto(2, "PAED-002", "EPA 2", null, null)
            })
            .On<GetEntrustmentScalesListQuery>(_ => Array.Empty<EntrustmentScaleDto>())
            .On<GetMsfCoverageForTraineeQuery>(_ => null);

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 30));
        cut.WaitForState(() => cut.Markup.Contains("Committee review") && cut.Markup.Contains("Evidence snapshot"));
        return cut;
    }

    private static CommitteeAgendaDto Agenda(params CommitteeAgendaLineDto[] lines)
        => new(30, 2026, 1, "2026 S1", false, lines, []);

    private static CommitteeAgendaLineDto Due(int epaId, string code)
        => Line(epaId, code, CommitteeAgendaLineStatus.Due, CommitteeAgendaLineState.Due, closing: true, blocks: true);

    private static CommitteeAgendaLineDto Staged(int epaId, string code)
        => Line(epaId, code, CommitteeAgendaLineStatus.Staged, CommitteeAgendaLineState.Due, closing: true, staged: true);

    private static CommitteeAgendaLineDto Line(
        int epaId,
        string code,
        CommitteeAgendaLineStatus status,
        CommitteeAgendaLineState state,
        (int Year, int? Semester)? window = null,
        bool closing = false,
        bool staged = false,
        bool blocks = false,
        string? reason = null)
    {
        var (year, semester) = window ?? (2026, 1);
        return new CommitteeAgendaLineDto(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, year, semester,
            semester is int s ? $"{year} S{s}" : $"{year}", closing, false, state, status, staged, blocks, reason, null,
            epaId == 1 ? 2 : epaId == 2 ? 1 : 0);
    }

    private static CommitteeEvidenceDto Evidence(int id, int epaId)
        => new(
            id, CommitteeEvidenceSourceType.Activity, id, null, null, $"Mini-CEX #{id}", "State: completed.",
            new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc),
            EpaId: epaId, EpaCode: $"PAED-00{epaId}", EpaTitle: $"EPA {epaId}", InstrumentName: "Mini-CEX",
            ObservedOn: new DateOnly(2026, 2, 10), ObservedOnDeclared: true, SourceState: "completed", SourceFinished: true);

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<ReviewDetail> cut, int epaId) => cut.Find($"#agenda-line-{epaId}");

    private static IReadOnlyList<string> Buttons(AngleSharp.Dom.IElement row)
        => row.QuerySelectorAll("button").Select(button => button.TextContent.Trim()).ToArray();

    private static AngleSharp.Dom.IElement ButtonIn(AngleSharp.Dom.IElement row, string label)
        => row.QuerySelectorAll("button").Single(button => button.TextContent.Trim() == label);

    private static AngleSharp.Dom.IElement RecordButton(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Record decision");

    private static AngleSharp.Dom.IElement RatifyButton(IRenderedComponent<ReviewDetail> cut)
        => RatifyButtonOrNull(cut) ?? throw new InvalidOperationException("No Ratify button.");

    private static AngleSharp.Dom.IElement? RatifyButtonOrNull(IRenderedComponent<ReviewDetail> cut)
        => cut.FindAll("button").SingleOrDefault(button => button.TextContent.Trim() == "Ratify");

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

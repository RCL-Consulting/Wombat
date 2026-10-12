using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The trainee's own reviews page shows a ratified review's agenda: each EPA the review was there to decide and what the
/// committee did with it (T131 slice 4, O6). A line left unstaged reads "Not decided at this sitting" with the decision's
/// one sentence about such lines, and no reason or Deferred: there is no deferral (T361, D53). The decision form tells the
/// chair the registrar sees the sentence once the review is ratified; this is where.
/// </summary>
public sealed partial class MyReviewsAgendaTests : TestContext
{
    private const string TraineeId = "trainee-1";

    private readonly RecordingSender _sender = new();

    public MyReviewsAgendaTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void ARatifiedReview_ShowsEachEpasOutcome_AndTheSentenceUnderEveryLineNotDecided()
    {
        var cut = Open(CommitteeReviewState.Ratified, Agenda(
            Line(1, "PAED-001", CommitteeAgendaLineStatus.Decided, CommitteeAgendaLineState.Decided, starId: 7),
            Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided),
            Line(3, "PAED-003", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided, window: (2026, null))),
            undecidedNote: "To be decided at her final review.");

        Text(cut.Find("#my-review-agenda-heading").ParentElement!).Should().Contain(
            "The EPAs this review was there to decide for 2026 S2, and what the committee did with each.");

        var decided = cut.Find("#my-agenda-line-1");
        decided.QuerySelector(".badge")!.ClassList.Should().Contain("badge-completed");
        Text(decided).Should().Contain("PAED-001 — EPA 1").And.Contain("2026 S2").And.Contain("Decided").And.Contain("STAR #7.");

        // T361, D53 and E1: closing (PAED-002) and optional (PAED-003) alike.
        foreach (var id in new[] { "#my-agenda-line-2", "#my-agenda-line-3" })
        {
            var notDecided = cut.Find(id);
            notDecided.QuerySelector(".badge")!.ClassList.Should().Contain("badge-accepted");
            Text(notDecided.QuerySelector(".badge")!).Should().Be("Not decided at this sitting");
            Text(notDecided).Should().Contain("To be decided at her final review.");
        }

        Text(cut.Find("#my-agenda-line-3")).Should().Contain("2026");
        Page(cut).Should().NotContain("Deferred").And.NotContain("reason");

        cut.FindAll("#my-review-agenda-heading ~ .table-container button").Should().BeEmpty("the trainee's view is read-only");
    }

    [Fact]
    public void AReviewWithNoAgenda_ShowsNoAgendaCard()
    {
        var cut = Open(CommitteeReviewState.Ratified, Agenda());

        cut.FindAll("#my-review-agenda-heading").Should().BeEmpty();
    }

    [Fact]
    public void LodgingAnAppeal_KeepsTheAgendaInView()
    {
        var cut = Open(CommitteeReviewState.Ratified, Agenda(
            Line(2, "PAED-002", CommitteeAgendaLineStatus.NotDecided, CommitteeAgendaLineState.NotDecided)),
            undecidedNote: "Later.");
        _sender.On<LodgeAppealCommand>(_ => Review(CommitteeReviewState.UnderAppeal, agenda: null, undecidedNote: "Later."));

        cut.Find("#trainee-appeal-reason").Change("The rating ignored my logbook.");
        cut.Find("#trainee-appeal-reason").Closest("form")!.Submit();

        cut.WaitForState(() => cut.Markup.Contains("Appeal lodged."));
        Text(cut.Find("#my-agenda-line-2")).Should().Contain("Not decided at this sitting").And.Contain("Later.");
    }

    // ---- Fixture ------------------------------------------------------------------------------------------------------

    private IRenderedComponent<MyReviews> Open(CommitteeReviewState state, CommitteeAgendaDto agenda, string? undecidedNote = null)
    {
        _sender
            .On<ListReviewsForTraineeQuery>(_ => new[]
            {
                new CommitteeReviewListItemDto(
                    30, TraineeId, 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
                    new DateOnly(2027, 1, 8), state, null, null)
                {
                    AcademicYear = 2026,
                    Semester = 2
                }
            })
            .On<GetCommitteeReviewByIdQuery>(_ => Review(state, agenda, undecidedNote));

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Trim() == "View"));
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "View").Click();
        cut.WaitForState(() => cut.Markup.Contains("Review detail"));
        return cut;
    }

    private static CommitteeReviewDetailDto Review(CommitteeReviewState state, CommitteeAgendaDto? agenda, string? undecidedNote = null)
        => new(
            30, TraineeId, 20, "General CCC", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), new DateOnly(2027, 1, 8),
            state, new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc), "chair-1", new DateTime(2027, 1, 8, 12, 0, 0, DateTimeKind.Utc),
            "chair-1", null,
            [
                new CommitteeDecisionDto(
                    41, CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null,
                    new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc), "chair-1", null) { UndecidedNote = undecidedNote }
            ],
            [], [])
        {
            AcademicYear = 2026,
            Semester = 2,
            Agenda = agenda
        };

    private static CommitteeAgendaDto Agenda(params CommitteeAgendaLineDto[] lines)
        => new(30, 2026, 2, "2026 S2", false, lines, []);

    private static CommitteeAgendaLineDto Line(
        int epaId,
        string code,
        CommitteeAgendaLineStatus status,
        CommitteeAgendaLineState state,
        (int Year, int? Semester)? window = null,
        int? starId = null)
    {
        var (year, semester) = window ?? (2026, 2);
        return new CommitteeAgendaLineDto(
            100 + epaId, epaId, code, $"EPA {epaId}", CommitteeAgendaLineOrigin.Cadence, year, semester,
            semester is int s ? $"{year} S{s}" : $"{year}", true, false, state, status, false,
            status == CommitteeAgendaLineStatus.NotDecided, starId, 0);
    }

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    private static string Page(IRenderedComponent<MyReviews> cut) => Whitespace().Replace(cut.Find("#my-review-agenda-heading").ParentElement!.TextContent, " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

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
            => _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}

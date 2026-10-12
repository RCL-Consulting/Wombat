using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
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
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// The committee review page as round 3 draws it (T361, flow 07), on the story's cast: Lerato Molefe's 2026 S2 annual
/// review before the Paed Annual Review Panel, chaired by Thandi Zulu. The page's reads answer with the review a test
/// gives; every request is recorded, and one nobody registered is refused, so a section that reads it fails apart.
/// </summary>
public abstract partial class ReviewPageTestBase : WombatTestContext
{
    /// <summary>14:02 SAST on 2026-10-05, Act 4's day D.</summary>
    protected static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 2, 0, TimeSpan.Zero);

    protected const int ReviewId = 30;
    protected const string Chair = "zulu";
    protected const string Naidoo = "naidoo";
    protected const string Botha = "botha";
    protected const string VanRensburg = "vanrensburg";
    protected const string Mbatha = "mbatha";

    protected readonly TestAuthorizationContext Auth;
    protected readonly RecordingSender Sender = new();
    protected readonly FixedClock Clock = new(Now);

    protected ReviewPageTestBase()
    {
        Auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(Sender);
        Services.AddSingleton<TimeProvider>(Clock);
        JSInterop.Mode = JSRuntimeMode.Loose;
        SignInAs(Chair);
    }

    protected void SignInAs(string userId, string role = WombatRoles.CommitteeMember)
    {
        Auth.SetAuthorized($"{userId}@test");
        Auth.SetRoles(role);
        Auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    // ─── The cast's review ───────────────────────────────────────────────────────

    protected static CommitteePersonDto Person(string userId, DecisionPanelMemberRole role, string name, bool maySit = true)
        => new(userId, role) { Name = name, MaySit = maySit };

    protected static IReadOnlyList<CommitteePersonDto> Panel() =>
    [
        Person(Chair, DecisionPanelMemberRole.Chair, "Thandi Zulu"),
        Person(Naidoo, DecisionPanelMemberRole.Member, "David Naidoo"),
        Person(Botha, DecisionPanelMemberRole.Member, "Sarah Botha"),
        Person(VanRensburg, DecisionPanelMemberRole.External, "John van Rensburg")
    ];

    /// <summary>An agenda line: PAED-001, 2026 S2, closing, due.</summary>
    protected static CommitteeAgendaLineDto Line(
        int epaId,
        string title,
        CommitteeAgendaLineStatus status = CommitteeAgendaLineStatus.Due,
        CommitteeAgendaLineState state = CommitteeAgendaLineState.Due,
        bool annual = false,
        bool staged = false,
        bool readsNotDecided = false,
        int? starId = null,
        int evidence = 0,
        CommitteeAgendaLineOrigin origin = CommitteeAgendaLineOrigin.Cadence)
        => new(
            100 + epaId, epaId, Code(epaId), title, origin, 2026, annual ? null : 2, annual ? "2026" : "2026 S2",
            !annual, false, state, status, staged, readsNotDecided, starId, evidence);

    protected static string Code(int epaId) => $"PAED-{epaId:000}";

    /// <summary>Molefe's agenda at Act 4, cut to four lines: PAED-001, 002 and 010 due at this sitting, PAED-008 optional.</summary>
    protected static IReadOnlyList<CommitteeAgendaLineDto> Lines() =>
    [
        Line(1, "Providing paediatric emergency care to children", evidence: 3),
        Line(2, "Managing common paediatric presentations"),
        Line(8, "Evaluating and managing neurodevelopmental and behavioural presentations in children",
            CommitteeAgendaLineStatus.AsOpportunityAllows, annual: true),
        Line(10, "Leading and operating within a clinical team", evidence: 2)
    ];

    protected static CommitteeAgendaDto Agenda(IReadOnlyList<CommitteeAgendaLineDto>? lines = null, bool decidesProgression = true)
        => new(ReviewId, 2026, 2, "2026 S2", false, lines ?? Lines(), [])
        {
            DecidesProgression = decidesProgression
        };

    /// <summary>A snapshot line about an EPA.</summary>
    protected static CommitteeEvidenceDto Evidence(
        int id,
        int epaId,
        string instrument,
        string assessorId,
        string assessor,
        string? rating,
        DateOnly observedOn,
        string state = "Completed",
        bool? finished = true)
        => new(
            id, CommitteeEvidenceSourceType.Activity, id, null, null, $"{instrument} #{id}", $"State: {state}.",
            new DateTime(2026, 9, 30, 8, 0, 0, DateTimeKind.Utc),
            EpaId: epaId, EpaCode: Code(epaId), EpaTitle: Lines().FirstOrDefault(line => line.EpaId == epaId)?.EpaTitle ?? $"EPA {epaId}",
            InstrumentName: instrument, IsRatedInstrument: true, RatingOrder: rating is null ? null : 5, RatingLabel: rating,
            ObservedOn: observedOn, ObservedOnDeclared: true, SourceState: state.ToLowerInvariant(), SourceFinished: finished,
            AssessorUserId: assessorId, SourceStateLabel: state)
        {
            AssessorName = assessor
        };

    /// <summary>Molefe's snapshot at Act 4, cut: PAED-001's three, PAED-010's two, and the campaign report.</summary>
    protected static IReadOnlyList<CommitteeEvidenceDto> Snapshot() =>
    [
        Evidence(12, 1, "Mini-CEX (Paediatrics)", Chair, "Thandi Zulu", "5", new DateOnly(2026, 9, 23)),
        Evidence(13, 1, "Case-Based Discussion (Paediatrics)", Naidoo, "David Naidoo", "5", new DateOnly(2026, 9, 24)),
        Evidence(14, 1, "DOPS (Paediatrics)", "patel", "Mohammed Patel", "4", new DateOnly(2026, 9, 26)),
        Evidence(21, 10, "Direct Observation", Chair, "Thandi Zulu", "5", new DateOnly(2026, 10, 1)),
        Evidence(22, 10, "MSF record", "msf", "Default MSF", null, new DateOnly(2026, 10, 2)),
        new(
            1, CommitteeEvidenceSourceType.MsfCampaign, null, 1, null, "Default MSF #1", "Released · 6 responses",
            new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), SourceState: "released", SourceFinished: true,
            SourceStateLabel: "Released")
    ];

    protected static CommitteeDecisionDto Decision(
        CommitteeDecisionCategory? category = CommitteeDecisionCategory.SatisfactoryProgress,
        string? undecidedNote = "Not at a decision point this year.")
        => new(41, category, "On track for completion.", null, Now.UtcDateTime, Chair, null)
        {
            UndecidedNote = undecidedNote,
            Attendees =
            [
                new CommitteePersonDto(Chair, DecisionPanelMemberRole.Chair) { Name = "Thandi Zulu" },
                new CommitteePersonDto(Naidoo, DecisionPanelMemberRole.Member) { Name = "David Naidoo" }
            ]
        };

    /// <summary>
    /// The review, read by the chair unless <paramref name="callerChairs" /> says otherwise; summative, annual progression.
    /// </summary>
    protected static CommitteeReviewDetailDto Review(
        CommitteeReviewState state = CommitteeReviewState.InProgress,
        bool callerChairs = true,
        CommitteeAgendaDto? agenda = null,
        IReadOnlyList<CommitteeDecisionDto>? decisions = null,
        CommitteeReviewType type = CommitteeReviewType.AnnualProgression)
        => new CommitteeReviewDetailDto(
            ReviewId,
            "molefe",
            20,
            "Paed Annual Review Panel",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 10, 1),
            state,
            state == CommitteeReviewState.Scheduled ? null : new DateTime(2026, 10, 5, 7, 0, 0, DateTimeKind.Utc),
            state == CommitteeReviewState.Scheduled ? null : Chair,
            null,
            null,
            null,
            decisions ?? (state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress ? [] : [Decision()]),
            [],
            state == CommitteeReviewState.Scheduled ? [] : Snapshot(),
            ReviewType: type)
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeName = "Lerato Molefe",
            TraineeProfileId = 7,
            TrainingYear = 4,
            CallerChairs = callerChairs,
            CallerMayStart = state == CommitteeReviewState.Scheduled,
            Agenda = agenda ?? Agenda(decidesProgression: type != CommitteeReviewType.EntrustmentOnly),
            PanelMembers = Panel()
        };

    protected static PendingEntrustmentDecisionDto Pending(
        int epaId, string level, IReadOnlyList<int> evidence, DateOnly? expires = null, int id = 0)
        => new(
            id == 0 ? 900 + epaId : id, ReviewId, epaId, Code(epaId), Lines().FirstOrDefault(line => line.EpaId == epaId)?.EpaTitle ?? "An EPA",
            50 + epaId, level, new DateOnly(2026, 10, 5), expires, "Ready.", evidence, Now.UtcDateTime, Chair);

    protected static IReadOnlyList<StarEpaOptionDto> StarEpas() =>
    [
        .. Lines().Select(line => new StarEpaOptionDto(line.EpaId, line.EpaCode, line.EpaTitle, 3, "CPSA Paediatric Entrustment Scale v11.1"))
    ];

    protected static EntrustmentScaleDto Scale() => new(3, "CPSA Paediatric Entrustment Scale v11.1", null,
    [
        new EntrustmentLevelDto(31, 1, "1", null),
        new EntrustmentLevelDto(34, 4, "3b", null),
        new EntrustmentLevelDto(35, 5, "4", null),
        new EntrustmentLevelDto(36, 6, "5", null)
    ]);

    /// <summary>Registers every read the page makes, with the answers a test may override first.</summary>
    protected IRenderedComponent<ReviewDetail> Render(
        CommitteeReviewDetailDto review, IReadOnlyList<PendingEntrustmentDecisionDto>? pending = null)
    {
        Sender
            .On<GetCommitteeReviewByIdQuery>(_ => review)
            .On<GetCommitteeAgendaQuery>(_ => review.Agenda)
            .On<ListPendingEntrustmentDecisionsForReviewQuery>(_ => pending ?? [])
            .On<GetSamplingConcentrationWarningsQuery>(_ => null)
            .On<CountMsfCampaignsOutsideSnapshotQuery>(_ => MsfCampaignsOutsideSnapshotDto.None)
            .On<GetEpaTrajectoryForTraineeQuery>(_ => Array.Empty<EpaTrajectoryDto>())
            .On<ListStarEpaOptionsForReviewQuery>(_ => StarEpas())
            .On<GetEntrustmentScalesListQuery>(_ => new[] { Scale() })
            .On<GetEntrustmentStandingForTraineeQuery>(_ => null)
            .On<GetMsfCoverageForTraineeQuery>(_ => null)
            .On<GetStartPreviewQuery>(_ => new StartPreviewDto(9, 8, 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 0));

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, ReviewId));
        cut.WaitForState(() => cut.FindAll("#sitting-bar").Count > 0 || cut.Markup.Contains("could not"));
        return cut;
    }

    // ─── Reading the page ────────────────────────────────────────────────────────

    protected static string Text(IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    protected static string Page(IRenderedFragment cut)
        => Whitespace().Replace(string.Join(" ", cut.Nodes.Select(node => node.TextContent)), " ");

    /// <summary>The page's buttons by their words, the dialogs' left out.</summary>
    protected static IReadOnlyList<string> Buttons(IRenderedFragment cut)
        => cut.FindAll("button").Where(button => button.Closest("dialog") is null).Select(button => Text(button)).ToArray();

    protected static IElement Button(IRenderedFragment cut, string words)
        => cut.FindAll("button").Single(button => button.Closest("dialog") is null && Text(button) == words);

    /// <summary>A dialog's button, by its words, in the dialog titled <paramref name="title" />.</summary>
    protected static IElement DialogButton(IRenderedFragment cut, string title, string words)
        => Dialog(cut, title).QuerySelectorAll("button").Single(button => Text(button) == words);

    protected static IElement Dialog(IRenderedFragment cut, string title)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == title);

    protected IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "wombat.focusById")
            .Select(invocation => (string)invocation.Arguments[0]!)
            .ToArray();

    protected int ElementFocusCalls()
        => JSInterop.Invocations.Count(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus");

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Answers what a test registers (the first registration wins), records every request, refuses the rest.</summary>
    protected sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Requests { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers.TryAdd(typeof(TRequest), request => answer((TRequest)request));
            return this;
        }

        /// <summary>Replaces an answer already registered.</summary>
        public RecordingSender Now<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public IReadOnlyList<TRequest> Sent<TRequest>() => Requests.OfType<TRequest>().ToArray();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)Answer(request)!);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Answer(request);
            return Task.CompletedTask;
        }

        private object? Answer(object request)
        {
            Requests.Add(request);
            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }

    protected sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Value { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Value;
    }
}

using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T161, D28: wherever a page shows an activity's date as its encounter date, a date nobody stated is marked as not
/// recorded, with the day the activity was created, in the wording the activity lists use (T137, T197,
/// <see cref="EncounterDate.Label" />).
/// </summary>
/// <remarks>
/// The seeds that reach this are <c>reflective_note</c> and <c>qi_project</c>, which declare no date field, and any
/// builder-made type without one. The chart component's own table is tested in <c>TrajectoryChartTests</c>; the committee
/// review page test here shows that the page hands the flag to it (the trajectories left My progress for the EPA pages in
/// T355).
/// </remarks>
public sealed class UndatedEncounterDateTests : TestContext
{
    public UndatedEncounterDateTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddScoped<ActivityNotices>();
    }

    [Fact]
    public void TheActivityPage_SaysAnUndatedActivitysDateWasNotRecorded_AndGivesTheDayItWasCreated()
    {
        var cut = RenderActivityView(Reflection(new DateOnly(2026, 3, 20), declared: false));

        // T342: About's Encounter row; its dd carries the id.
        Text(cut.Find("#activity-encounter-date")).Should().Be("not recorded (created 2026-03-20)");
        cut.Find("#activity-encounter-date").PreviousElementSibling!.TextContent.Trim().Should().Be("Encounter");
    }

    [Fact]
    public void TheActivityPage_ShowsAStatedEncounterDateBare()
    {
        var cut = RenderActivityView(Reflection(new DateOnly(2026, 3, 20), declared: true));

        Text(cut.Find("#activity-encounter-date")).Should().Be("2026-03-20");
    }

    /// <summary>
    /// The committee draws the same chart from the same query, so it must be told the same thing: a committee member
    /// weighing progression is the reader the filing day most misleads. The page hands the chart the query's points whole
    /// (T355); a private mapping that dropped the flag is what this catches.
    /// </summary>
    [Fact]
    public void TheCommitteeReviewPage_MarksAnUndatedObservationOnTheTrajectory()
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(new Dictionary<Type, object?>
        {
            [typeof(GetCommitteeReviewByIdQuery)] = new CommitteeReviewDetailDto(
                5,
                "trainee-1",
                1,
                "Annual review panel",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                new DateOnly(2027, 1, 15),
                CommitteeReviewState.InProgress,
                new DateTime(2027, 1, 15, 8, 0, 0, DateTimeKind.Utc),
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
        },
            [typeof(ListPendingEntrustmentDecisionsForReviewQuery)] = (IReadOnlyList<PendingEntrustmentDecisionDto>)[],
            [typeof(GetSamplingConcentrationWarningsQuery)] = null,
            [typeof(CountMsfCampaignsOutsideSnapshotQuery)] = MsfCampaignsOutsideSnapshotDto.None,
            [typeof(GetEpaTrajectoryForTraineeQuery)] = (IReadOnlyList<EpaTrajectoryDto>)[OneDatedOneUndated()],
            [typeof(GetEntrustmentStandingForTraineeQuery)] = null,
            [typeof(GetMsfCoverageForTraineeQuery)] = null,
            [typeof(ListStarEpaOptionsForReviewQuery)] = (IReadOnlyList<StarEpaOptionDto>)[],
            [typeof(GetEntrustmentScalesListQuery)] = (IReadOnlyList<EntrustmentScaleDto>)[]
        }));

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 5));

        TrajectoryTableDates(cut).Should().Equal("2026-01-15", "not recorded (created 2026-03-20)");
    }

    /// <summary>One EPA's trajectory: a rating with a stated encounter date, then one sitting on the day it was created.</summary>
    private static EpaTrajectoryDto OneDatedOneUndated() => new(
        7, "EPA-07", "Emergency triage", true, null, null, [],
        [
            new(1, new DateOnly(2026, 1, 15), ObservedOnDeclared: true, 2, "2", "Direct observation", "assessor-a"),
            new(2, new DateOnly(2026, 3, 20), ObservedOnDeclared: false, 3, "3", "Direct observation", "assessor-a")
        ]);

    /// <summary>The Encounter column of the chart's table, each row's header, once both observations are drawn (T355).</summary>
    private static IEnumerable<string> TrajectoryTableDates(IRenderedFragment cut)
    {
        cut.WaitForState(() => cut.FindAll("table.trajectory-table tbody tr").Count == 2);

        return cut.FindAll("table.trajectory-table tbody tr").Select(row => row.QuerySelector("th")!.TextContent).ToArray();
    }

    private IRenderedComponent<ActivityView> RenderActivityView(ActivityDetailDto detail)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(new Dictionary<Type, object?>
        {
            [typeof(GetActivityByIdQuery)] = detail
        }));

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 31));
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));

        return cut;
    }

    /// <summary>A completed reflective note, filed on 20 March: read-only, nothing to do.</summary>
    private static ActivityDetailDto Reflection(DateOnly observedOn, bool declared)
    {
        var activity = new ActivityDto(
            31,
            4,
            "reflective_note",
            "Reflective note",
            null,
            1,
            """
            {
              "version": 1,
              "sections": [
                { "key": "s", "title": "Reflection", "fields": [ { "key": "what_happened", "type": "longtext", "label": "What happened" } ] }
              ]
            }
            """,
            """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "completed", "label": "Completed", "terminal": true }
              ],
              "transitions": [
                { "key": "complete", "from": "draft", "to": "completed", "actor": "subject" }
              ]
            }
            """,
            "[]",
            """{ "counts_for": [] }""",
            "trainee-1",
            1,
            "trainee-1",
            "completed",
            "Completed",
            """{"what_happened":"A long night on the ward."}""",
            null,
            null,
            observedOn,
            declared,
            new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
            []);

        return new ActivityDetailDto(activity, [], []);
    }

    /// <summary>An element's text with Razor's source indentation collapsed to single spaces.</summary>
    private static string Text(AngleSharp.Dom.IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        private readonly IReadOnlyDictionary<Type, object?> _answers;

        public FakeSender(IReadOnlyDictionary<Type, object?> answers) => _answers = answers;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => _answers.TryGetValue(request.GetType(), out var answer)
                ? Task.FromResult((TResponse)answer!)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

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
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Services;
using EntrustmentDecisionsAdmin = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;

namespace Wombat.Web.Tests.Epas;

/// <summary>
/// T255: every page that names an EPA a trainee has ratings or a STAR for marks one that is no longer in force "(no longer
/// in use)", as My activities and the Inbox do (T231): My progress's paused group, the rating trajectory headings on an EPA's page and on the committee
/// review page, the admin's entrustment decisions list, and My authorisations.
/// </summary>
/// <remarks>
/// Each marked label is compared with the EPA picker's own text (<see cref="EpaOptionLabel.For" />), not a copy of it, so a
/// change to the picker's wording that a page does not follow fails here. The marker is its own span.paused-mark (T355, C13), and an EPA in
/// force has none.
/// </remarks>
public sealed class EpaNoLongerInUseMarkTests : TestContext
{
    private const string TraineeUserId = "trainee-1";

    private static readonly string Deactivated = EpaOptionLabel.For("PAED-006", "Manage a sick neonate", inForce: false);
    private const string InForce = "PAED-001 — Take a history";

    /// <summary>
    /// T355, C13: the mark is its own span.paused-mark, the one class every row, heading and chart heading marks a paused
    /// EPA with, in the EPA picker's words; an EPA in force carries no span at all.
    /// </summary>
    [Fact]
    public void EpaLabel_MarksAPausedEpa_WithThePausedMark_AndAnEpaInForceWithNone()
    {
        var paused = RenderComponent<Wombat.Web.Components.Shared.EpaLabel>(parameters => parameters
            .Add(label => label.Code, "PAED-006")
            .Add(label => label.Title, "Manage a sick neonate")
            .Add(label => label.InForce, false));
        var inForce = RenderComponent<Wombat.Web.Components.Shared.EpaLabel>(parameters => parameters
            .Add(label => label.Code, "PAED-001")
            .Add(label => label.Title, "Take a history")
            .Add(label => label.InForce, true));

        Regex.Replace(paused.Markup, @"\s+", " ").Trim().Should().Be(
            $"PAED-006 — Manage a sick neonate <span class=\"paused-mark\">{EpaOptionLabel.NoLongerInUse}</span>");
        paused.FindAll("span.muted").Should().BeEmpty("the mark is no longer the generic muted span");
        inForce.Markup.Trim().Should().Be(InForce);
    }

    /// <summary>
    /// T355 (Spec § 6, C13): My progress draws no trajectory now (Q5); it lists a paused EPA in its "No longer in use"
    /// group, marked in the picker's words, and an EPA in force unmarked in the index above it.
    /// </summary>
    [Fact]
    public void MyProgress_MarksAnEpaThatIsNoLongerInForce_InItsPausedGroup()
    {
        SignIn(WombatRoles.Trainee);
        var window = new QuotaWindowDto(
            "Semester 2, 2026", "July to November", new DateOnly(2026, 7, 1), new DateOnly(2026, 11, 30), Wombat.Domain.Curricula.QuotaWindowStatus.Counting,
            1, 3, IsMet: false, Shortfall: 2, PercentOfTarget: 33, MinimumLevelReachedCount: 1, LastObservedOn: null,
            LastObservedOnDeclared: false, FirstCountedName: null, FirstCountedOn: null);
        var summary = new TraineeCurriculumProgressSummaryDto(
            new DateOnly(2026, 10, 3), new DateOnly(2025, 1, 15), 2, "Semester 2, 2026", "July to November", new DateOnly(2026, 11, 30),
            false, 0, 1, 0, 0, true, false, false, null, null,
            [new TraineeCurriculumProgressDto(101, 1, "PAED-001", "Take a history", Wombat.Domain.Curricula.QuotaPeriod.Semester, 3, window, null, 4, "3b", null)])
        {
            Paused = [new PausedItemDto(106, 6, "PAED-006", "Manage a sick neonate", Wombat.Domain.Curricula.QuotaPeriod.Semester)]
        };
        Answer(new Dictionary<Type, object?>
        {
            [typeof(GetCurriculumProgressForTraineeQuery)] = summary,
            [typeof(GetEntrustmentStandingForTraineeQuery)] = null,
            [typeof(GetMsfCoverageForTraineeQuery)] = null
        });

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Your EPAs"));

        var names = cut.FindAll("section.index-section tbody th a.epa-link");
        Texts(names).Should().Equal(InForce, Deactivated);
        MarkersIn(names).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    /// <summary>
    /// The committee reads the same chart from the same query, so it is told the same thing. (The trajectories left My
    /// progress for the EPA pages in T355, whose chart heading <c>EpaProgressPageTests</c> holds.)
    /// </summary>
    [Fact]
    public void TheCommitteeReviewPage_MarksATrajectoryWhoseEpaIsNoLongerInForce()
    {
        SignIn(WombatRoles.CommitteeMember, "member-1");
        Answer(new Dictionary<Type, object?>
        {
            [typeof(GetCommitteeReviewByIdQuery)] = Review(),
            [typeof(ListPendingEntrustmentDecisionsForReviewQuery)] = (IReadOnlyList<PendingEntrustmentDecisionDto>)[],
            [typeof(GetSamplingConcentrationWarningsQuery)] = null,
            [typeof(CountMsfCampaignsOutsideSnapshotQuery)] = MsfCampaignsOutsideSnapshotDto.None,
            [typeof(GetEpaTrajectoryForTraineeQuery)] = Trajectories(),
            [typeof(GetEntrustmentStandingForTraineeQuery)] = null,
            [typeof(GetMsfCoverageForTraineeQuery)] = null,
            [typeof(ListStarEpaOptionsForReviewQuery)] = (IReadOnlyList<StarEpaOptionDto>)[],
            [typeof(GetEntrustmentScalesListQuery)] = (IReadOnlyList<EntrustmentScaleDto>)[]
        });

        var cut = RenderComponent<ReviewDetail>(parameters => parameters.Add(page => page.ReviewId, 5));
        cut.WaitForState(() => cut.Markup.Contains("Rating trajectory by EPA"));

        var headings = cut.FindAll("section.trajectory-card h4");
        Texts(headings).Should().Equal(InForce, Deactivated);
        MarkersIn(headings).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    [Fact]
    public void TheAdminDecisionsList_MarksAStarWhoseEpaIsNoLongerInForce()
    {
        SignIn(WombatRoles.InstitutionalAdmin, "admin-1");
        Answer(new Dictionary<Type, object?>
        {
            [typeof(ListEntrustmentDecisionsForAdminQuery)] = Decisions()
        });

        var cut = RenderComponent<EntrustmentDecisionsAdmin>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);

        var index = cut.FindAll("thead th").Select(cell => cell.TextContent.Trim()).ToList().IndexOf("EPA");
        var cells = cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll("td")[index]).ToList();
        Texts(cells).Should().Equal(InForce, Deactivated);
        MarkersIn(cells).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    /// <summary>The revoke confirmation names the EPA too, and says the same of it as its row.</summary>
    [Fact]
    public void TheAdminRevokeConfirmation_MarksAStarWhoseEpaIsNoLongerInForce()
    {
        SignIn(WombatRoles.InstitutionalAdmin, "admin-1");
        Answer(new Dictionary<Type, object?>
        {
            [typeof(ListEntrustmentDecisionsForAdminQuery)] = Decisions()
        });

        var cut = RenderComponent<EntrustmentDecisionsAdmin>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 2);

        cut.FindAll("tbody tr").ElementAt(1).QuerySelectorAll("button").Single(button => button.TextContent.Trim() == "Revoke").Click();

        var named = cut.Find(".detail-card strong");
        Text(named).Should().Be(Deactivated);
        MarkersIn([named]).Should().Equal([EpaOptionLabel.NoLongerInUse]);
    }

    [Fact]
    public void MyAuthorisations_MarksAStarWhoseEpaIsNoLongerInForce()
    {
        SignIn(WombatRoles.Trainee);
        Answer(new Dictionary<Type, object?>
        {
            [typeof(GetActiveDecisionsForTraineeQuery)] = Decisions()
        });

        var cut = RenderComponent<MyAuthorisations>();
        cut.WaitForState(() => cut.FindAll(".detail-card h3").Count == 2);

        var headings = cut.FindAll(".detail-card h3");
        Texts(headings).Should().Equal(InForce, Deactivated);
        MarkersIn(headings).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    // ---- helpers ----------------------------------------------------------------------------------------------------

    private void SignIn(string role, string userId = TraineeUserId)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{userId}@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void Answer(IReadOnlyDictionary<Type, object?> answers)
        => Services.AddSingleton<IScopedSender>(new FakeSender(answers));

    /// <summary>Two EPAs with a rating each: PAED-001, in force, and PAED-006, deactivated since.</summary>
    private static IReadOnlyList<EpaTrajectoryDto> Trajectories() =>
    [
        Trajectory(5001, "PAED-001", "Take a history", inForce: true),
        Trajectory(5006, "PAED-006", "Manage a sick neonate", inForce: false)
    ];

    private static EpaTrajectoryDto Trajectory(int epaId, string code, string title, bool inForce) => new(
        epaId, code, title, inForce, null, null, [],
        [new(epaId, new DateOnly(2026, 3, 10), ObservedOnDeclared: true, 3, "3", "Direct observation", "assessor-a")]);

    /// <summary>Two active STARs: on PAED-001, in force, and on PAED-006, deactivated since it was issued.</summary>
    private static IReadOnlyList<EntrustmentDecisionDto> Decisions() =>
    [
        Decision(41, 5001, "PAED-001", "Take a history", inForce: true),
        Decision(42, 5006, "PAED-006", "Manage a sick neonate", inForce: false)
    ];

    private static EntrustmentDecisionDto Decision(int id, int epaId, string code, string title, bool inForce)
        => new(id, TraineeUserId, epaId, code, title, inForce, 3, "3a", 3, new DateOnly(2026, 7, 1), null, 30, "chair-1",
            "Consistent across the period.", EntrustmentDecisionStatus.Active, null, null, null, null, [])
        {
            TraineeName = "Thandi Mokoena"
        };

    private static CommitteeReviewDetailDto Review() => new(
        5,
        TraineeUserId,
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
    };

    private static IReadOnlyList<string> Texts(IEnumerable<IElement> elements) => elements.Select(Text).ToList();

    /// <summary>An element's text with Razor's source indentation collapsed to single spaces.</summary>
    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    /// <summary>The text of every paused mark (span.paused-mark, T355 C13) inside these elements.</summary>
    private static IReadOnlyList<string> MarkersIn(IEnumerable<IElement> elements)
        => elements.SelectMany(element => element.QuerySelectorAll("span.paused-mark")).Select(Text).ToList();

    private sealed class FakeSender(IReadOnlyDictionary<Type, object?> answers) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => answers.TryGetValue(request.GetType(), out var answer)
                ? Task.FromResult((TResponse)answer!)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

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
/// in use)", as My activities and the Inbox do (T231): the rating trajectory headings on My progress and on the committee
/// review page, the admin's entrustment decisions list, and My authorisations.
/// </summary>
/// <remarks>
/// Each marked label is compared with the EPA picker's own text (<see cref="EpaOptionLabel.For" />), not a copy of it, so a
/// change to the picker's wording that a page does not follow fails here. The marker is its own muted span, and an EPA in
/// force has none.
/// </remarks>
public sealed class EpaNoLongerInUseMarkTests : TestContext
{
    private const string TraineeUserId = "trainee-1";

    private static readonly string Deactivated = EpaOptionLabel.For("PAED-006", "Manage a sick neonate", inForce: false);
    private const string InForce = "PAED-001 — Take a history";

    [Fact]
    public void MyProgress_MarksATrajectoryWhoseEpaIsNoLongerInForce()
    {
        SignIn(WombatRoles.Trainee);
        Answer(new Dictionary<Type, object?>
        {
            [typeof(GetCurriculumProgressForTraineeQuery)] = null,
            [typeof(GetEpaTrajectoryForTraineeQuery)] = Trajectories(),
            [typeof(GetEntrustmentStandingForTraineeQuery)] = null,
            [typeof(GetMsfCoverageForTraineeQuery)] = null
        });

        var cut = RenderComponent<MyProgress>();
        cut.WaitForState(() => cut.Markup.Contains("Rating trajectory"));

        var headings = cut.FindAll("h2")
            .Single(heading => heading.TextContent.Trim() == "Rating trajectory")
            .ParentElement!
            .QuerySelectorAll(".detail-card h3");
        Texts(headings).Should().Equal(InForce, Deactivated);
        MarkersIn(headings).Should().Equal([EpaOptionLabel.NoLongerInUse], "only the EPA that is not in force is marked, and muted");
    }

    /// <summary>The committee reads the same chart from the same query, so it is told the same thing.</summary>
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

        var headings = cut.FindAll("article.detail-card--compact h4");
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

    /// <summary>The text of every muted span inside these elements.</summary>
    private static IReadOnlyList<string> MarkersIn(IEnumerable<IElement> elements)
        => elements.SelectMany(element => element.QuerySelectorAll("span.muted")).Select(Text).ToList();

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

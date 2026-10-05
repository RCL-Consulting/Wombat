using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.Features.Programme.ProgrammeCast;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// T358 (flow 06, C9): the registrar page's Committee reviews: one registrar's reviews, any state, that the caller may
/// open by the review page's own ladder (<c>CommitteeReviewReadAccess</c>), newest scheduled first.
/// </summary>
public sealed class ListReviewsForProgrammeTraineeQueryTests
{
    [Fact]
    public async Task ACommitteeMember_ReadsOnlyTheirPanelsReviews_ACoordinatorTheInstitutions()
    {
        var (db, _) = Build();
        await using var __ = db;
        AddPanel(db, 1, Kgk, "zulu");
        AddPanel(db, 2, Kgk, "naidoo");
        AddReview(db, 11, panelId: 1, Mahlangu, new DateOnly(2026, 6, 20), semester: 1);
        AddReview(db, 12, panelId: 2, Mahlangu, new DateOnly(2026, 11, 20));
        AddReview(db, 13, panelId: 1, Ndlovu, new DateOnly(2026, 11, 21));
        db.SaveChanges();
        var users = FakeUserDirectory.PanelMembersOf(db);

        var asMember = await ListAsync(db, users, CommitteeMember("zulu"));
        var asCoordinator = await ListAsync(db, users, Coordinator());

        asMember.Select(review => review.Id).Should().Equal(11);
        asCoordinator.Select(review => review.Id).Should().Equal(12, 11);
        asCoordinator.Should().OnlyContain(review => review.TraineeUserId == Mahlangu);
        asCoordinator[0].PeriodLabel.Should().Be("2026 S2");
    }

    [Fact]
    public async Task AnotherInstitutionsCoordinator_AndARegistrar_ReadNone()
    {
        var (db, _) = Build();
        await using var __ = db;
        AddPanel(db, 1, Kgk, "zulu");
        AddReview(db, 11, panelId: 1, Mahlangu, new DateOnly(2026, 11, 20));
        db.SaveChanges();
        var users = FakeUserDirectory.PanelMembersOf(db);

        (await ListAsync(db, users, Coordinator("other", OtherHospital))).Should().BeEmpty();
        (await ListAsync(db, users, TestPrincipals.InRoles([WombatRoles.CommitteeMember, WombatRoles.Trainee], "zulu", Kgk)))
            .Should().BeEmpty("a registrar on the committee reads no peer's review here (T185)");
    }

    private static Task<IReadOnlyList<CommitteeReviewListItemDto>> ListAsync(
        Wombat.Infrastructure.Persistence.ApplicationDbContext db,
        FakeUserDirectory users,
        System.Security.Claims.ClaimsPrincipal principal)
        => new ListReviewsForProgrammeTraineeQueryHandler(db, users).Handle(
            new ListReviewsForProgrammeTraineeQuery(principal, Mahlangu), CancellationToken.None);
}

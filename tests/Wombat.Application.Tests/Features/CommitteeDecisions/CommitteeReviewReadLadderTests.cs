using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Tests.Shared;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// One review, three queries, one ladder. ReviewDetail.razor asks for the review, the sampling
/// report and the staged entrustment decisions, and before this each had its own idea of who may
/// read a review: the detail query waived every Coordinator in the country, and the pending-decision
/// query took no caller at all — it handed back EPA codes, proposed levels and the committee's
/// written rationale to anyone who could name a review id. These tests hold the three to the same
/// answer. (T101 finding E)
/// </summary>
public sealed class CommitteeReviewReadLadderTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;

    [Fact]
    public async Task Coordinator_ReachesAReviewInTheirOwnInstitution()
    {
        await using var db = CreateDbContext();
        var reviewId = await SeedReviewAsync(db);

        var review = await new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, Principal("coord-1", WombatRoles.Coordinator, HostInstitution)),
            CancellationToken.None);

        review.TraineeUserId.Should().Be("trainee-1");
    }

    [Fact]
    public async Task Coordinator_DoesNotReachAnotherInstitutionsReview()
    {
        await using var db = CreateDbContext();
        var reviewId = await SeedReviewAsync(db);

        var act = () => new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, Principal("coord-2", WombatRoles.Coordinator, OtherInstitution)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task PendingDecisions_AreListedForAPanelMember()
    {
        await using var db = CreateDbContext();
        var reviewId = await SeedReviewAsync(db);
        await StagePendingAsync(db, reviewId);

        var pending = await new ListPendingEntrustmentDecisionsForReviewQueryHandler(db).Handle(
            new ListPendingEntrustmentDecisionsForReviewQuery(
                reviewId, Principal("member-1", WombatRoles.CommitteeMember, HostInstitution)),
            CancellationToken.None);

        pending.Should().ContainSingle().Which.EpaCode.Should().Be("EPA-07");
    }

    [Fact]
    public async Task PendingDecisions_AreRefusedToACallerWithNoClaimOnTheReview()
    {
        // The rationale on a staged decision is the committee's reasoning about a named trainee,
        // written before anyone has been told. A review id was the whole of the authorization.
        await using var db = CreateDbContext();
        var reviewId = await SeedReviewAsync(db);
        await StagePendingAsync(db, reviewId);

        var act = () => new ListPendingEntrustmentDecisionsForReviewQueryHandler(db).Handle(
            new ListPendingEntrustmentDecisionsForReviewQuery(reviewId, Principal("stranger-1")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task PendingDecisions_AreRefusedToACoordinatorFromAnotherInstitution()
    {
        await using var db = CreateDbContext();
        var reviewId = await SeedReviewAsync(db);
        await StagePendingAsync(db, reviewId);

        var act = () => new ListPendingEntrustmentDecisionsForReviewQueryHandler(db).Handle(
            new ListPendingEntrustmentDecisionsForReviewQuery(
                reviewId, Principal("coord-2", WombatRoles.Coordinator, OtherInstitution)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task PendingDecisions_AreRefusedToTheTraineeBeforeRatification()
    {
        // The trainee arm of the ladder is about timing as much as identity: a decision still being
        // drafted is not theirs to read, and this query is the draft.
        await using var db = CreateDbContext();
        var reviewId = await SeedReviewAsync(db);
        await StagePendingAsync(db, reviewId);

        var act = () => new ListPendingEntrustmentDecisionsForReviewQueryHandler(db).Handle(
            new ListPendingEntrustmentDecisionsForReviewQuery(
                reviewId, Principal("trainee-1", WombatRoles.Trainee, HostInstitution)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<int> SeedReviewAsync(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "Emergency triage", IsActive = true });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 3, ScaleId = 1, Order = 3, Label = "3a" });

        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "Paediatrics ARCP",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        };
        db.DecisionPanels.Add(panel);

        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = 30,
            PanelId = panel.Id,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();
        return review.Id;
    }

    private static async Task StagePendingAsync(ApplicationDbContext db, int reviewId)
    {
        db.Set<PendingEntrustmentDecision>().Add(PendingEntrustmentDecision.Stage(
            reviewId,
            epaId: 7,
            authorisedLevelId: 3,
            issuedOn: new DateOnly(2026, 4, 1),
            expiresOn: null,
            rationale: "Consistent independent practice across the review period.",
            evidenceItemIds: [1],
            actorUserId: "chair-1",
            utcNow: DateTime.UtcNow));
        await db.SaveChangesAsync();
    }

    private static ClaimsPrincipal Principal(string userId, string? role = null, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

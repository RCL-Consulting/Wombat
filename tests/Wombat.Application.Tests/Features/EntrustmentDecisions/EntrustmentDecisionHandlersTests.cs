using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

public sealed class EntrustmentDecisionHandlersTests
{
    private const int EvidenceOnEpa7 = 501;
    private const int EvidenceOnEpa8 = 502;

    [Fact]
    public async Task Ratify_OnlyByTheChair_IssuesTheStagedStar_AndSupersedesThePriorActiveOne()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);
        var prior = await SeedStarAsync(dbContext, review.Id, epaId: 7, levelId: 3, new DateOnly(2026, 1, 10), null);

        // Staged while the review is in progress: since T165 the staged STARs are part of the decision recorded next, and
        // are fixed with it.
        await StageAsync(dbContext, review.Id, epaId: 7, levelId: 4, new DateOnly(2026, 4, 1), null, "Level advanced.", EvidenceOnEpa7);
        await RecordDecisionAsync(dbContext, review.Id);

        var ratify = new RatifyCommitteeDecisionCommandHandler(dbContext);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => ratify.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("member-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None));

        await ratify.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var issued = await dbContext.Set<EntrustmentDecision>().SingleAsync(d => d.Id != prior.Id);
        issued.Status.Should().Be(EntrustmentDecisionStatus.Active);
        issued.AuthorisedLevelId.Should().Be(4);

        var superseded = await dbContext.Set<EntrustmentDecision>().SingleAsync(d => d.Id == prior.Id);
        superseded.Status.Should().Be(EntrustmentDecisionStatus.Superseded);
        superseded.SupersededByDecisionId.Should().Be(issued.Id);
    }

    [Fact]
    public async Task Stage_OnAReviewNotYetStarted_IsRefused_AfterTheChairIsAuthorised()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: false);

        var act = () => StageAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null, "Rationale.", EvidenceOnEpa7);

        // Since T165 a STAR is staged only while the review is in progress (StagedStars).
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(StagedStars.FixedWhenDecided);
    }

    [Fact]
    public async Task Revoke_AllowsInstitutionalAdminsAndIssuingChair()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1));

        var revokeHandler = new RevokeEntrustmentDecisionCommandHandler(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Arbitrary revocation.", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None));

        var revokedByAdmin = await revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Scope reduced by programme.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        revokedByAdmin.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
        revokedByAdmin.RevocationReason.Should().Be("Scope reduced by programme.");
        revokedByAdmin.RevokedByUserId.Should().Be("admin-1");
    }

    [Fact]
    public async Task StagePendingAndRatify_IssuesAtomically_EachStarWithTheEvidenceItWasStagedOn()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);

        // Staged while the review is in progress: since T165 the staged STARs are part of the decision recorded next, and
        // are fixed with it.
        await StageAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1), "EPA 7 rationale.", EvidenceOnEpa7);
        await StageAsync(dbContext, review.Id, 8, 4, new DateOnly(2026, 4, 1), null, "EPA 8 rationale.", EvidenceOnEpa7, EvidenceOnEpa8);

        (await dbContext.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(2);

        await RecordDecisionAsync(dbContext, review.Id);

        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext);
        await ratifyHandler.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        (await dbContext.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(0);

        var decisions = await dbContext.Set<EntrustmentDecision>().Include(d => d.EvidenceLinks).OrderBy(d => d.EpaId).ToListAsync();
        decisions.Should().HaveCount(2);
        decisions.Should().AllSatisfy(d =>
        {
            d.Status.Should().Be(EntrustmentDecisionStatus.Active);
            d.IssuedByCommitteeReviewId.Should().Be(review.Id);
            d.IssuedByChairUserId.Should().Be("chair-1");
        });
        decisions[0].EvidenceLinks.Select(link => link.CommitteeEvidenceId).Should().Equal(EvidenceOnEpa7);
        decisions[1].EvidenceLinks.Select(link => link.CommitteeEvidenceId).Should().BeEquivalentTo([EvidenceOnEpa7, EvidenceOnEpa8]);
    }

    [Fact]
    public async Task GetActiveDecisionsForTrainee_ReturnsOnlyActive()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var first = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        await SeedStarAsync(dbContext, review.Id, 8, 4, new DateOnly(2026, 4, 1), null);

        var revokeHandler = new RevokeEntrustmentDecisionCommandHandler(dbContext);
        await revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(first.Id, "Revoked.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        var queryHandler = new GetActiveDecisionsForTraineeQueryHandler(dbContext);
        var active = await queryHandler.Handle(new GetActiveDecisionsForTraineeQuery("trainee-1", CreatePrincipal("trainee-1", [WombatRoles.Trainee])), CancellationToken.None);

        active.Should().HaveCount(1);
        active[0].EpaId.Should().Be(8);
        active[0].EvidenceLinks.Should().ContainSingle().Which.CommitteeEvidenceId.Should().Be(EvidenceOnEpa8);
    }

    [Fact]
    public async Task ListExpiringDecisions_RespectsWindow()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var asOf = new DateOnly(2026, 4, 1);
        await SeedStarAsync(dbContext, review.Id, 7, 3, asOf.AddDays(-30), asOf.AddDays(10));
        await SeedStarAsync(dbContext, review.Id, 8, 4, asOf.AddDays(-30), asOf.AddDays(100));

        var handler = new ListExpiringDecisionsQueryHandler(dbContext);
        var expiring = await handler.Handle(new ListExpiringDecisionsQuery(30, asOf), CancellationToken.None);

        expiring.Should().HaveCount(1);
        expiring[0].EpaId.Should().Be(7);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Task<PendingEntrustmentDecisionDto> StageAsync(
        ApplicationDbContext dbContext, int reviewId, int epaId, int levelId, DateOnly issuedOn, DateOnly? expiresOn,
        string rationale, params int[] evidenceItemIds)
        => new StagePendingEntrustmentDecisionCommandHandler(dbContext).Handle(
            new StagePendingEntrustmentDecisionCommand(
                reviewId, null, epaId, levelId, issuedOn, expiresOn, rationale, evidenceItemIds,
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

    /// <summary>The panel's two members, each an active committee member at its institution (T165, PanelSeat).</summary>
    private static FakeUserDirectory Committee => FakeUserDirectory.CommitteeMembersAt(1, "chair-1", "member-1");

    /// <summary>Records the decision with a quorate sitting, the chair and the other member present (T165).</summary>
    private static Task RecordDecisionAsync(ApplicationDbContext dbContext, int reviewId)
        => new RecordCommitteeDecisionCommandHandler(dbContext, Committee).Handle(
            new RecordCommitteeDecisionCommand(reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null, ["chair-1", "member-1"],
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

    /// <summary>
    /// A STAR issued by the review, resting on its snapshot line about the same EPA: what a ratification writes, stored
    /// directly for the tests that only read or revoke one.
    /// </summary>
    private static async Task<EntrustmentDecision> SeedStarAsync(
        ApplicationDbContext dbContext, int reviewId, int epaId, int levelId, DateOnly issuedOn, DateOnly? expiresOn)
    {
        var line = await dbContext.Set<CommitteeEvidence>()
            .SingleAsync(item => item.Id == (epaId == 7 ? EvidenceOnEpa7 : EvidenceOnEpa8));
        var decision = EntrustmentDecision.Issue(
            "trainee-1", epaId, levelId, issuedOn, expiresOn, reviewId, "chair-1", $"EPA {epaId}.",
            [EntrustmentEvidenceLink.FromSnapshot(line)]);
        dbContext.Set<EntrustmentDecision>().Add(decision);
        await dbContext.SaveChangesAsync();
        return decision;
    }

    private static async Task<CommitteeReview> SeedRatifiedReviewAsync(ApplicationDbContext dbContext)
    {
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);
        await RecordDecisionAsync(dbContext, review.Id);
        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext);
        await ratifyHandler.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        return await dbContext.Set<CommitteeReview>().SingleAsync(r => r.Id == review.Id);
    }

    private static async Task<CommitteeReview> SeedReviewInStateAsync(ApplicationDbContext dbContext, bool startReview)
    {
        var institution = new Institution { Id = 1, Name = "Test Hospital", IsActive = true, CreatedOn = DateTime.UtcNow };
        var speciality = new Speciality { Id = 5, CollegeId = 1, Name = "General Medicine", IsActive = true };
        var subSpec = new SubSpeciality { Id = 9, SpecialityId = 5, Name = "Acute Care", IsActive = true };

        var scale = new EntrustmentScale { Id = 1, Name = "Standard 5-point" };
        var levels = Enumerable.Range(1, 5).Select(order => new EntrustmentLevel
        {
            Id = order,
            ScaleId = 1,
            Order = order,
            Label = $"Level {order}"
        }).ToList();

        var epa7 = new Epa { Id = 7, SubSpecialityId = 9, Code = "EPA-07", Title = "Emergency triage", IsActive = true };
        var epa8 = new Epa { Id = 8, SubSpecialityId = 9, Code = "EPA-08", Title = "Admission", IsActive = true };

        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "ARCP panel",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = 1,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        };

        var review = new CommitteeReview
        {
            Id = 30,
            Panel = panel,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        dbContext.Institutions.Add(institution);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpec);

        // The trainee trains at the test hospital, so its InstitutionalAdmin oversees them (T183), and it is the panel's
        // institution: a panel acts only on its own institution's trainees (T182). A STAR is granted only on an EPA of
        // the trainee's curriculum (T167), so the curriculum holds both EPAs.
        dbContext.Curricula.Add(new Curriculum { Id = 40, SubSpecialityId = 9, Name = "Acute Care", Version = "1" });
        dbContext.CurriculumItems.AddRange(
            new CurriculumItem { Id = 41, CurriculumId = 40, EpaId = 7, RequiredCount = 1, MinimumLevelOrder = 3 },
            new CurriculumItem { Id = 42, CurriculumId = 40, EpaId = 8, RequiredCount = 1, MinimumLevelOrder = 3 });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = 1, CurriculumId = 40,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1), IsActive = true
        });

        dbContext.EntrustmentScales.Add(scale);
        dbContext.EntrustmentLevels.AddRange(levels);
        dbContext.Epas.AddRange(epa7, epa8);
        dbContext.DecisionPanels.Add(panel);
        dbContext.CommitteeReviews.Add(review);
        await dbContext.SaveChangesAsync();

        if (startReview)
        {
            var startHandler = new StartCommitteeReviewCommandHandler(dbContext);
            await startHandler.Handle(
                new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
                CancellationToken.None);

            // The window held no activity, so the snapshot is written here: one line about each EPA, which a staged
            // decision names as the evidence it rests on (D38, T131).
            dbContext.Set<CommitteeEvidence>().AddRange(
                new CommitteeEvidence
                {
                    Id = EvidenceOnEpa7, ReviewId = review.Id, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 101,
                    EpaId = 7, EpaCode = "EPA-07", SourceLabel = "Mini-CEX #101", Summary = "State: completed.",
                    ObservedOn = new DateOnly(2026, 2, 1)
                },
                new CommitteeEvidence
                {
                    Id = EvidenceOnEpa8, ReviewId = review.Id, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 102,
                    EpaId = 8, EpaCode = "EPA-08", SourceLabel = "CbD #102", Summary = "State: completed.",
                    ObservedOn = new DateOnly(2026, 2, 2)
                });
            await dbContext.SaveChangesAsync();
        }

        return await dbContext.Set<CommitteeReview>().SingleAsync(r => r.Id == review.Id);
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string> roles, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(WombatClaimTypes.SpecialityId, "5"));
        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

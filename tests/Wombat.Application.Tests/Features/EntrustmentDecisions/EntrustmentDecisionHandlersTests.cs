using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

public sealed class EntrustmentDecisionHandlersTests
{
    [Fact]
    public async Task Issue_OnlyAllowsChairsAndAutoSupersedesPriorActive()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var issueHandler = new IssueEntrustmentDecisionCommandHandler(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => issueHandler.Handle(
            new IssueEntrustmentDecisionCommand(
                "trainee-1", 7, 3,
                new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1),
                review.Id, "Rationale.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("member-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None));

        var first = await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand(
                "trainee-1", 7, 3,
                new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1),
                review.Id, "First authorisation.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        first.Status.Should().Be(EntrustmentDecisionStatus.Active);

        var second = await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand(
                "trainee-1", 7, 4,
                new DateOnly(2026, 6, 1), new DateOnly(2027, 6, 1),
                review.Id, "Level advanced after additional evidence.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        second.Status.Should().Be(EntrustmentDecisionStatus.Active);

        var prior = await dbContext.Set<EntrustmentDecision>().SingleAsync(d => d.Id == first.Id);
        prior.Status.Should().Be(EntrustmentDecisionStatus.Superseded);
        prior.SupersededByDecisionId.Should().Be(second.Id);
    }

    [Fact]
    public async Task Issue_RejectsWhenReviewIsNotRatified()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: false);

        var issueHandler = new IssueEntrustmentDecisionCommandHandler(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => issueHandler.Handle(
            new IssueEntrustmentDecisionCommand(
                "trainee-1", 7, 3,
                new DateOnly(2026, 4, 1), null,
                review.Id, "Rationale.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None));

        exception.Message.Should().Contain("ratified", Exactly.Once());
    }

    [Fact]
    public async Task Revoke_AllowsInstitutionalAdminsAndIssuingChair()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var issueHandler = new IssueEntrustmentDecisionCommandHandler(dbContext);
        var issued = await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand(
                "trainee-1", 7, 3,
                new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1),
                review.Id, "First authorisation.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var revokeHandler = new RevokeEntrustmentDecisionCommandHandler(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Arbitrary revocation.", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None));

        var revokedByAdmin = await revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Scope reduced by programme.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin])),
            CancellationToken.None);

        revokedByAdmin.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
        revokedByAdmin.RevocationReason.Should().Be("Scope reduced by programme.");
        revokedByAdmin.RevokedByUserId.Should().Be("admin-1");
    }

    [Fact]
    public async Task StagePendingAndRatify_IssuesAtomically()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);

        var recordHandler = new RecordCommitteeDecisionCommandHandler(dbContext);
        await recordHandler.Handle(
            new RecordCommitteeDecisionCommand(review.Id, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null,
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var stageHandler = new StagePendingEntrustmentDecisionCommandHandler(dbContext);
        await stageHandler.Handle(
            new StagePendingEntrustmentDecisionCommand(
                review.Id, null, 7, 3,
                new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1),
                "EPA 7 rationale.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        await stageHandler.Handle(
            new StagePendingEntrustmentDecisionCommand(
                review.Id, null, 8, 4,
                new DateOnly(2026, 4, 1), null,
                "EPA 8 rationale.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        (await dbContext.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(2);

        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext);
        await ratifyHandler.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        (await dbContext.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(0);

        var decisions = await dbContext.Set<EntrustmentDecision>().ToListAsync();
        decisions.Should().HaveCount(2);
        decisions.Should().AllSatisfy(d =>
        {
            d.Status.Should().Be(EntrustmentDecisionStatus.Active);
            d.IssuedByCommitteeReviewId.Should().Be(review.Id);
            d.IssuedByChairUserId.Should().Be("chair-1");
        });
    }

    [Fact]
    public async Task GetActiveDecisionsForTrainee_ReturnsOnlyActive()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var issueHandler = new IssueEntrustmentDecisionCommandHandler(dbContext);
        var first = await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand("trainee-1", 7, 3, new DateOnly(2026, 4, 1), null, review.Id, "EPA 7.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand("trainee-1", 8, 4, new DateOnly(2026, 4, 1), null, review.Id, "EPA 8.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var revokeHandler = new RevokeEntrustmentDecisionCommandHandler(dbContext);
        await revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(first.Id, "Revoked.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin])),
            CancellationToken.None);

        var queryHandler = new GetActiveDecisionsForTraineeQueryHandler(dbContext);
        var active = await queryHandler.Handle(new GetActiveDecisionsForTraineeQuery("trainee-1"), CancellationToken.None);

        active.Should().HaveCount(1);
        active[0].EpaId.Should().Be(8);
    }

    [Fact]
    public async Task ListExpiringDecisions_RespectsWindow()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var asOf = new DateOnly(2026, 4, 1);
        var issueHandler = new IssueEntrustmentDecisionCommandHandler(dbContext);
        await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand("trainee-1", 7, 3, asOf.AddDays(-30), asOf.AddDays(10), review.Id, "Expires within window.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        await issueHandler.Handle(
            new IssueEntrustmentDecisionCommand("trainee-1", 8, 4, asOf.AddDays(-30), asOf.AddDays(100), review.Id, "Expires outside window.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

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

    private static async Task<CommitteeReview> SeedRatifiedReviewAsync(ApplicationDbContext dbContext)
    {
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);
        var recordHandler = new RecordCommitteeDecisionCommandHandler(dbContext);
        await recordHandler.Handle(
            new RecordCommitteeDecisionCommand(review.Id, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null,
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
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
        }

        return await dbContext.Set<CommitteeReview>().SingleAsync(r => r.Id == review.Id);
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(WombatClaimTypes.SpecialityId, "5"));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

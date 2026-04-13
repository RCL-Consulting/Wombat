using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

public sealed class CommitteeDecisionHandlersTests
{
    [Fact]
    public async Task StartReview_SnapshotsEvidenceAndDoesNotFollowLaterActivityChanges()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewAsync(dbContext);

        var startHandler = new StartCommitteeReviewCommandHandler(dbContext);
        var started = await startHandler.Handle(
            new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        started.State.Should().Be(CommitteeReviewState.InProgress);
        started.EvidenceItems.Should().NotBeEmpty();
        started.EvidenceItems.Should().ContainSingle(item => item.ActivityId == 100);
        started.EvidenceItems.Single(item => item.ActivityId == 100).Summary.Should().Contain("draft");

        var activity = await dbContext.Activities.SingleAsync(entity => entity.Id == 100);
        activity.CurrentState = "completed";
        activity.UpdatedOn = DateTime.UtcNow.AddDays(1);
        await dbContext.SaveChangesAsync();

        var refreshed = await new GetCommitteeReviewByIdQueryHandler(dbContext).Handle(
            new GetCommitteeReviewByIdQuery(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        refreshed.EvidenceItems.Single(item => item.ActivityId == 100).Summary.Should().Contain("draft");
        refreshed.EvidenceItems.Single(item => item.ActivityId == 100).Summary.Should().NotContain("completed");
    }

    [Fact]
    public async Task Authorization_RatifyAppealAndResolveRespectChairTraineeAndAppealBodyRules()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewAsync(dbContext);

        var startHandler = new StartCommitteeReviewCommandHandler(dbContext);
        await startHandler.Handle(new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])), CancellationToken.None);

        var recordHandler = new RecordCommitteeDecisionCommandHandler(dbContext);
        await recordHandler.Handle(
            new RecordCommitteeDecisionCommand(
                review.Id,
                CommitteeDecisionCategory.SatisfactoryWithObservations,
                "Reasonable progress with targeted conditions.",
                "Improve documentation quality.",
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            ratifyHandler.Handle(new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("member-1", [WombatRoles.CommitteeMember])), CancellationToken.None));

        await ratifyHandler.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var appealHandler = new LodgeAppealCommandHandler(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            appealHandler.Handle(new LodgeAppealCommand(review.Id, "Not my record.", CreatePrincipal("other-trainee", [WombatRoles.Trainee])), CancellationToken.None));

        await appealHandler.Handle(
            new LodgeAppealCommand(review.Id, "The conditions are too broad.", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None);

        var resolveHandler = new ResolveAppealCommandHandler(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            resolveHandler.Handle(
                new ResolveAppealCommand(review.Id, CommitteeAppealOutcome.Dismissed, null, null, null, CreatePrincipal("member-1", [WombatRoles.CommitteeMember])),
                CancellationToken.None));

        var resolved = await resolveHandler.Handle(
            new ResolveAppealCommand(
                review.Id,
                CommitteeAppealOutcome.Remitted,
                CommitteeDecisionCategory.SatisfactoryProgress,
                "Appeal body reduced the conditions after review.",
                null,
                CreatePrincipal("external-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        resolved.State.Should().Be(CommitteeReviewState.Final);
        resolved.Decisions.Should().HaveCount(2);
        resolved.Decisions[0].Category.Should().Be(CommitteeDecisionCategory.SatisfactoryProgress);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<CommitteeReview> SeedReviewAsync(ApplicationDbContext dbContext)
    {
        var activityType = new ActivityType
        {
            Id = 10,
            Key = "mini_cex",
            Name = "Mini-CEX",
            Scope = ActivityScope.Institution,
            ScopeId = 1,
            Version = 1,
            SchemaJson = "{}",
            WorkflowJson = "{}",
            CreditRulesJson = "{}",
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };

        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "General Medicine ARCP",
            Scope = DecisionPanelScope.Speciality,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
                new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
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

        var activity = new Activity
        {
            Id = 100,
            ActivityType = activityType,
            ActivityTypeId = activityType.Id,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = "draft",
            DataJson = """{ "title": "Observed consultation" }""",
            CreatedOn = new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc),
            UpdatedOn = new DateTime(2026, 2, 2, 8, 0, 0, DateTimeKind.Utc)
        };

        var template = new MsfTemplate
        {
            Id = 40,
            Name = "Annual MSF"
        };

        var msfCampaign = new MsfCampaign
        {
            Id = 50,
            SubjectUserId = "trainee-1",
            Template = template,
            TemplateId = template.Id,
            CreatedByUserId = "coord-1",
            CreatedOn = new DateTime(2026, 2, 3, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2026, 2, 3),
            ClosesOn = new DateOnly(2026, 2, 15),
            MinimumResponses = 4,
            MinimumCategoryResponses = 2,
            State = MsfCampaignState.Released,
            ReleasedOn = new DateTime(2026, 2, 20, 8, 0, 0, DateTimeKind.Utc),
            Responses =
            [
                new MsfResponse
                {
                    SubmittedOn = new DateTime(2026, 2, 10, 8, 0, 0, DateTimeKind.Utc)
                }
            ]
        };

        dbContext.ActivityTypes.Add(activityType);
        dbContext.DecisionPanels.Add(panel);
        dbContext.CommitteeReviews.Add(review);
        dbContext.Activities.Add(activity);
        dbContext.MsfTemplates.Add(template);
        dbContext.MsfCampaigns.Add(msfCampaign);
        await dbContext.SaveChangesAsync();

        return review;
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

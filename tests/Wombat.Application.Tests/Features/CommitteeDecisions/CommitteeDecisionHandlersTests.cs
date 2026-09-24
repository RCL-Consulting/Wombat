using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Common.Security;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;
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

    /// <summary>
    /// A released campaign's evidence row says which EPAs it stands behind, and that its per-EPA records
    /// exist. (T121)
    /// </summary>
    /// <remarks>
    /// The snapshot now contains BOTH the campaign and one ordinary <c>msf_cpsa</c> activity per covered
    /// EPA. That is deliberate — a panel wants the report and the per-EPA claims — but it is only readable
    /// if the parent names the children; otherwise it reads as one campaign and N unexplained siblings
    /// dated the same minute.
    /// </remarks>
    [Fact]
    public async Task StartReview_MsfEvidenceNamesTheEpasItStandsBehind()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewAsync(dbContext);

        dbContext.Epas.Add(new Epa { Id = 70, SubSpecialityId = 1, Code = "PAED-010", Title = "Leading a clinical team" });
        dbContext.Epas.Add(new Epa { Id = 71, SubSpecialityId = 1, Code = "PAED-011", Title = "Managing population health" });
        dbContext.MsfCampaignEpas.Add(new MsfCampaignEpa
        {
            CampaignId = 50,
            EpaId = 70,
            RecordedOn = new DateTime(2026, 2, 20, 8, 0, 0, DateTimeKind.Utc)
        });

        // Declared but never recorded: this EPA had left the trainee's curriculum by release day, so
        // the release dropped it. The snapshot must not claim an activity that was never written.
        dbContext.MsfCampaignEpas.Add(new MsfCampaignEpa { CampaignId = 50, EpaId = 71 });
        (await dbContext.MsfCampaigns.SingleAsync(campaign => campaign.Id == 50)).EvidenceRecordedOn =
            new DateTime(2026, 2, 20, 8, 0, 0, DateTimeKind.Utc);
        await dbContext.SaveChangesAsync();

        var started = await new StartCommitteeReviewCommandHandler(dbContext).Handle(
            new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var msfEvidence = started.EvidenceItems.Should().ContainSingle(item => item.MsfCampaignId == 50).Subject;
        msfEvidence.Summary.Should().Contain("Evidence recorded for PAED-010, one activity each.");
        msfEvidence.Summary.Should().Contain("Also declared PAED-011");
        msfEvidence.Summary.Should().NotContain("Evidence recorded for PAED-010, PAED-011");
    }

    /// <summary>
    /// A campaign that was never released is not evidence, however squarely it closes inside the review
    /// window. (T138)
    /// </summary>
    /// <remarks>
    /// Before release the trainee has not seen the report and it may still be withdrawn; a withdrawn one
    /// was deliberately retracted. The portfolio PDF and the trainee's own MSF list already read released
    /// campaigns only, and a panel is shown the same set. The released fixture campaign sits beside it in
    /// the same window, so the assertion is about state, not about the window. A draft or open campaign
    /// has no <c>ClosedOn</c> and so misses the window as well; the closed, under-review and
    /// withdrawn-after-close cases are the ones only the state rule keeps out.
    /// </remarks>
    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.UnderReview)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public async Task StartReview_LeavesOutAnMsfCampaignThatWasNeverReleased(MsfCampaignState state)
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewAsync(dbContext);

        dbContext.MsfCampaigns.Add(CreateCampaignClosingInWindow(51, state));
        await dbContext.SaveChangesAsync();

        var started = await new StartCommitteeReviewCommandHandler(dbContext).Handle(
            new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        started.EvidenceItems.Should().NotContain(item => item.MsfCampaignId == 51);
        started.EvidenceItems.Should().ContainSingle(item => item.MsfCampaignId == 50);
    }

    /// <summary>
    /// A released campaign closing inside the window is in the snapshot, and one that named no EPA says
    /// so. (T138)
    /// </summary>
    [Fact]
    public async Task StartReview_IncludesAReleasedMsfCampaignClosingInTheWindow()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewAsync(dbContext);

        dbContext.MsfCampaigns.Add(CreateCampaignClosingInWindow(51, MsfCampaignState.Released));
        await dbContext.SaveChangesAsync();

        var started = await new StartCommitteeReviewCommandHandler(dbContext).Handle(
            new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        var msfEvidence = started.EvidenceItems.Should().ContainSingle(item => item.MsfCampaignId == 51).Subject;
        msfEvidence.SourceType.Should().Be(CommitteeEvidenceSourceType.MsfCampaign);
        msfEvidence.Summary.Should().Contain("State: Released");
        msfEvidence.Summary.Should().Contain("closed 2026-03-10.");
        msfEvidence.Summary.Should().EndWith(" Names no EPA, so it recorded no evidence.");
    }

    /// <summary>
    /// A released campaign falls in the review window by the UTC day it actually closed, which is the day
    /// its per-EPA evidence activities are observed on, so the report and its claims reach the same panel.
    /// (T138)
    /// </summary>
    /// <remarks>
    /// Each case also adds the per-EPA activity a release would have written, observed on the closing day
    /// (<c>ReleaseMsfCampaign.EvidenceCompleteOn</c>), and asserts it lands on the same side of the window
    /// as its campaign. Windowing by <c>ClosesOn</c> split the first two: the auto-close job shuts a
    /// campaign the day after <c>ClosesOn</c> at the earliest, and a coordinator can close one early.
    /// </remarks>
    [Theory]
    // Scheduled for the window's last day, auto-closed the next morning: its evidence belongs to the next review.
    [InlineData("2026-03-31", "2026-04-01T01:00:00Z", false)]
    // Scheduled after the window, closed early inside it: its evidence is dated inside it.
    [InlineData("2026-04-10", "2026-03-28T09:00:00Z", true)]
    // The window's days are inclusive at both ends, in UTC.
    [InlineData("2026-03-31", "2026-03-31T23:30:00Z", true)]
    [InlineData("2026-01-10", "2026-01-01T00:00:00Z", true)]
    [InlineData("2025-12-31", "2025-12-31T23:59:00Z", false)]
    public async Task StartReview_WindowsAnMsfCampaignByTheDayItClosed(string closesOn, string closedOn, bool inSnapshot)
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewAsync(dbContext);

        var closedAt = DateTime.Parse(closedOn, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        var campaign = CreateCampaignClosingInWindow(51, MsfCampaignState.Released);
        campaign.ClosesOn = DateOnly.Parse(closesOn, CultureInfo.InvariantCulture);
        campaign.ClosedOn = closedAt;
        campaign.ReleasedOn = closedAt.AddDays(2);
        dbContext.MsfCampaigns.Add(campaign);

        dbContext.Activities.Add(new Activity
        {
            Id = 101,
            ActivityTypeId = 10,
            SchemaVersion = 1,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coord-1",
            CurrentState = "completed",
            DataJson = "{}",
            CreatedOn = closedAt.AddDays(2),
            ObservedOn = DateOnly.FromDateTime(closedAt),
            UpdatedOn = closedAt.AddDays(2)
        });
        await dbContext.SaveChangesAsync();

        var started = await new StartCommitteeReviewCommandHandler(dbContext).Handle(
            new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        started.EvidenceItems.Any(item => item.MsfCampaignId == 51).Should().Be(inSnapshot);
        started.EvidenceItems.Any(item => item.ActivityId == 101).Should().Be(
            inSnapshot, "the campaign's per-EPA evidence is observed on the day it closed, and must travel with it");
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
            // The panel's institution is the trainee's: a panel acts only on its own institution's trainees. (T182)
            InstitutionId = 1,
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
            // T119: production stamps this in ActivityService; a fixture that builds the
            // entity directly must set it, or it defaults to 0001-01-01.
            ObservedOn = DateOnly.FromDateTime(new DateTime(2026, 2, 1, 8, 0, 0, DateTimeKind.Utc)),
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
            // A released campaign was closed first: Release requires UnderReview, and Close stamps this.
            OpenedOn = new DateTime(2026, 2, 3, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = new DateTime(2026, 2, 16, 1, 0, 0, DateTimeKind.Utc),
            ReleasedOn = new DateTime(2026, 2, 20, 8, 0, 0, DateTimeKind.Utc),
            Responses =
            [
                new MsfResponse
                {
                    SubmittedOn = new DateTime(2026, 2, 10, 8, 0, 0, DateTimeKind.Utc)
                }
            ]
        };

        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = 1, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1)
        });
        dbContext.ActivityTypes.Add(activityType);
        dbContext.DecisionPanels.Add(panel);
        dbContext.CommitteeReviews.Add(review);
        dbContext.Activities.Add(activity);
        dbContext.MsfTemplates.Add(template);
        dbContext.MsfCampaigns.Add(msfCampaign);
        await dbContext.SaveChangesAsync();

        return review;
    }

    /// <summary>
    /// A second campaign for the fixture's trainee, on the fixture's template, closing on 2026-03-10 —
    /// inside the review window — with the timestamps its state implies. A withdrawn one was withdrawn
    /// from under review, after it closed: the case where only the state rule keeps it out of a snapshot.
    /// </summary>
    private static MsfCampaign CreateCampaignClosingInWindow(int id, MsfCampaignState state)
    {
        var createdOn = new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc);

        return new MsfCampaign
        {
            Id = id,
            SubjectUserId = "trainee-1",
            TemplateId = 40,
            CreatedByUserId = "coord-1",
            CreatedOn = createdOn,
            OpensOn = new DateOnly(2026, 3, 1),
            ClosesOn = new DateOnly(2026, 3, 10),
            MinimumResponses = 4,
            MinimumCategoryResponses = 2,
            State = state,
            OpenedOn = state == MsfCampaignState.Draft ? null : createdOn,
            ClosedOn = state is MsfCampaignState.Closed or MsfCampaignState.UnderReview or MsfCampaignState.Released
                    or MsfCampaignState.Withdrawn
                ? new DateTime(2026, 3, 10, 18, 0, 0, DateTimeKind.Utc)
                : null,
            ReleasedOn = state == MsfCampaignState.Released ? new DateTime(2026, 3, 12, 8, 0, 0, DateTimeKind.Utc) : null,
            WithdrawnOn = state == MsfCampaignState.Withdrawn ? new DateTime(2026, 3, 11, 8, 0, 0, DateTimeKind.Utc) : null
        };
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

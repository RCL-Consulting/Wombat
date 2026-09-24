using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// A review counts the MSF campaigns that closed in its window and are not in its snapshot: the ones still awaiting
/// release, and, once it has started, the ones released after it did. Both were unreleased at Start, so the snapshot
/// left them out, and no later snapshot will take them. (T173)
/// </summary>
public sealed class MsfCampaignsOutsideSnapshotTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int ReviewId = 30;
    private const int OverlappingReviewId = 31;

    /// <summary>A moment inside the review's window (2026-01-01 to 2026-03-31).</summary>
    private static readonly DateTime InWindow = new(2026, 3, 10, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task AReviewWhoseWindowHoldsACampaignUnderReview_CountsIt()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(new MsfCampaignsOutsideSnapshotDto(1, 0));
    }

    [Fact]
    public async Task BeforeStart_AReleasedCampaignInTheWindow_IsNotCounted()
    {
        // Start will take it; nothing else is on record.
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.Released));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(MsfCampaignsOutsideSnapshotDto.None);
    }

    [Fact]
    public async Task EveryCampaignAwaitingReleaseInTheWindow_IsCounted()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.AddRange(
            Campaign(51, MsfCampaignState.UnderReview),
            Campaign(52, MsfCampaignState.UnderReview, closedOn: new DateTime(2026, 1, 20, 9, 0, 0, DateTimeKind.Utc)),
            Campaign(53, MsfCampaignState.Released));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(new MsfCampaignsOutsideSnapshotDto(2, 0));
    }

    [Fact]
    public async Task AfterStart_ACampaignTheSnapshotTook_IsNotCounted()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.Released));
        await db.SaveChangesAsync();

        (await StartAsync(db)).EvidenceItems.Should().Contain(item => item.MsfCampaignId == 51, "the setup: Start took it");

        (await CountAsync(db, Chair)).Should().Be(MsfCampaignsOutsideSnapshotDto.None);
    }

    [Fact]
    public async Task AfterStart_ACampaignStillUnderReview_IsCountedAsAwaitingRelease()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();
        await StartAsync(db);

        (await CountAsync(db, Chair)).Should().Be(new MsfCampaignsOutsideSnapshotDto(1, 0));
    }

    /// <summary>
    /// Releasing a campaign after Start does not put it in the snapshot, so it must not drop out of the count: it moves
    /// from awaiting release to released after Start. Counting only the unreleased made the notice vanish on release
    /// while the campaign was still in no snapshot.
    /// </summary>
    [Fact]
    public async Task AfterStart_ACampaignReleasedSinceStarting_IsStillCounted()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        var campaign = Campaign(51, MsfCampaignState.UnderReview);
        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        await StartAsync(db);

        campaign.State = MsfCampaignState.Released;
        campaign.ReleasedOn = InWindow.AddDays(30);
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(new MsfCampaignsOutsideSnapshotDto(0, 1));
    }

    /// <summary>
    /// Membership is this review's snapshot, not any review's. A formative review over the same days can hold a
    /// campaign that the summative one does not.
    /// </summary>
    [Fact]
    public async Task AnotherReviewsSnapshot_DoesNotHideACampaignFromThisOne()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        await SeedOverlappingReviewAsync(db);
        var campaign = Campaign(51, MsfCampaignState.UnderReview);
        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();

        // This review starts while the campaign is under review; the overlapping one after it is released.
        await StartAsync(db);
        campaign.State = MsfCampaignState.Released;
        campaign.ReleasedOn = InWindow.AddDays(30);
        await db.SaveChangesAsync();
        (await StartAsync(db, OverlappingReviewId)).EvidenceItems
            .Should().Contain(item => item.MsfCampaignId == 51, "the setup: the other review's snapshot holds it");

        (await CountAsync(db, Chair)).Should().Be(new MsfCampaignsOutsideSnapshotDto(0, 1));
    }

    /// <summary>
    /// Only <see cref="MsfCampaignState.UnderReview" /> is awaiting release: it is the one state Release accepts. A
    /// withdrawn campaign was retracted, closed after it was under review, and will never be released; a draft or open
    /// one has not closed. <see cref="MsfCampaignState.Closed" /> is produced by nothing today, and Release refuses it.
    /// Before Start a released one is Start's to take. Each carries a close time inside the window, whatever its state
    /// implies, so that only the state rule can keep it out.
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.Released)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public async Task BeforeStart_ACampaignInAnyOtherState_IsNotCounted(MsfCampaignState state)
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(ClosedInWindow(Campaign(51, state)));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(MsfCampaignsOutsideSnapshotDto.None);
    }

    /// <summary>
    /// After Start, a released campaign the snapshot does not hold is counted (above); no other state is. Each campaign
    /// arrives after Start, so the snapshot holds none of them, and each closed inside the window.
    /// </summary>
    [Theory]
    [InlineData(MsfCampaignState.Draft)]
    [InlineData(MsfCampaignState.Open)]
    [InlineData(MsfCampaignState.Closed)]
    [InlineData(MsfCampaignState.Withdrawn)]
    public async Task AfterStart_ACampaignInAnyOtherState_IsNotCounted(MsfCampaignState state)
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        await StartAsync(db);
        db.MsfCampaigns.Add(ClosedInWindow(Campaign(51, state)));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(MsfCampaignsOutsideSnapshotDto.None);
    }

    [Fact]
    public async Task AnotherTraineesCampaign_IsNotCounted()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview, subjectUserId: "trainee-2"));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(MsfCampaignsOutsideSnapshotDto.None);
    }

    /// <summary>
    /// The notice counts exactly the campaigns the snapshot would take were they released: the same UTC close-day
    /// window, whatever the scheduled close date says. Each case is counted while under review, then released, and
    /// the review started; the campaign must be in that snapshot exactly when it was counted.
    /// </summary>
    [Theory]
    // Scheduled for the window's last day, auto-closed the next morning: it belongs to the next review.
    [InlineData("2026-03-31", "2026-04-01T01:00:00Z", false)]
    // Scheduled after the window, closed early inside it.
    [InlineData("2026-04-10", "2026-03-28T09:00:00Z", true)]
    // The window's days are inclusive at both ends, in UTC.
    [InlineData("2026-03-31", "2026-03-31T23:30:00Z", true)]
    [InlineData("2026-01-10", "2026-01-01T00:00:00Z", true)]
    [InlineData("2025-12-31", "2025-12-31T23:59:00Z", false)]
    public async Task TheWindowIsTheSnapshots(string closesOn, string closedOn, bool inWindow)
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        var campaign = Campaign(
            51,
            MsfCampaignState.UnderReview,
            closedOn: DateTime.Parse(closedOn, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        campaign.ClosesOn = DateOnly.Parse(closesOn, CultureInfo.InvariantCulture);
        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).AwaitingRelease.Should().Be(inWindow ? 1 : 0);

        campaign.State = MsfCampaignState.Released;
        campaign.ReleasedOn = campaign.ClosedOn!.Value.AddDays(2);
        await db.SaveChangesAsync();

        var started = await StartAsync(db);

        started.EvidenceItems.Any(item => item.MsfCampaignId == 51).Should().Be(
            inWindow, "a campaign counted as awaiting release must be one this review's snapshot would take once released");
    }

    [Fact]
    public async Task ACoordinatorOfThePanelsInstitution_IsTold()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        (await CountAsync(db, Principal("coord-1", [WombatRoles.Coordinator], HostInstitution))).AwaitingRelease.Should().Be(1);
    }

    [Fact]
    public async Task AnInstitutionalAdminOfThePanelsInstitution_IsTold()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        (await CountAsync(db, Principal("ia-1", [WombatRoles.InstitutionalAdmin], HostInstitution))).AwaitingRelease.Should().Be(1);
    }

    /// <summary>
    /// An External panel member from another institution reads the review, and its frozen snapshot, through panel
    /// membership. They are told what is missing too: answering them zero would say nothing is.
    /// </summary>
    [Fact]
    public async Task AnExternalPanelMemberFromAnotherInstitution_IsTold()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        (await CountAsync(db, Principal("external-1", [WombatRoles.CommitteeMember], OtherInstitution))).AwaitingRelease.Should().Be(1);
    }

    [Fact]
    public async Task ACoordinatorFromAnotherInstitution_IsRefused()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        var act = () => CountAsync(db, Principal("coord-2", [WombatRoles.Coordinator], OtherInstitution));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ACallerWithNoClaimOnTheReview_IsRefused()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        var act = () => CountAsync(db, Principal("member-9", [WombatRoles.CommitteeMember], HostInstitution));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    /// <summary>
    /// The subject is shown a campaign only once it is released (T113), and the notice is the panel's. The review
    /// ladder admits them to their own ratified review; the count tells them nothing, of either kind.
    /// </summary>
    [Fact]
    public async Task TheTraineeOnTheirOwnRatifiedReview_IsToldNothing()
    {
        await using var db = CreateDbContext();
        var review = await SeedReviewAsync(db);
        db.Entry(review).Property(entity => entity.State).CurrentValue = CommitteeReviewState.Ratified;
        db.MsfCampaigns.AddRange(Campaign(51, MsfCampaignState.UnderReview), Campaign(52, MsfCampaignState.Released));
        await db.SaveChangesAsync();

        (await CountAsync(db, Chair)).Should().Be(new MsfCampaignsOutsideSnapshotDto(1, 1), "the setup: both are outside it");
        (await CountAsync(db, Principal("trainee-1", [WombatRoles.Trainee], HostInstitution)))
            .Should().Be(MsfCampaignsOutsideSnapshotDto.None);
    }

    [Fact]
    public async Task TheTraineeBeforeRatification_IsRefusedByTheLadder()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        var act = () => CountAsync(db, Principal("trainee-1", [WombatRoles.Trainee], HostInstitution));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task AReviewThatDoesNotExist_IsNotFound()
    {
        await using var db = CreateDbContext();
        await SeedReviewAsync(db);

        var act = () => new CountMsfCampaignsOutsideSnapshotQueryHandler(db).Handle(
            new CountMsfCampaignsOutsideSnapshotQuery(999, Chair),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ClaimsPrincipal Chair => Principal("chair-1", [WombatRoles.CommitteeMember], HostInstitution);

    private static Task<MsfCampaignsOutsideSnapshotDto> CountAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new CountMsfCampaignsOutsideSnapshotQueryHandler(db).Handle(
            new CountMsfCampaignsOutsideSnapshotQuery(ReviewId, principal),
            CancellationToken.None);

    private static Task<CommitteeReviewDetailDto> StartAsync(ApplicationDbContext db, int reviewId = ReviewId)
        => new StartCommitteeReviewCommandHandler(db).Handle(
            new StartCommitteeReviewCommand(reviewId, Chair),
            CancellationToken.None);

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<CommitteeReview> SeedReviewAsync(ApplicationDbContext db)
    {
        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
                new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
            ]
        };
        db.DecisionPanels.Add(panel);
        db.MsfTemplates.Add(new MsfTemplate { Id = 40, Name = "Annual MSF" });
        // The trainee trains at the panel's institution: a panel acts only on its own institution's trainees. (T182)
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = HostInstitution, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1)
        });

        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = ReviewId,
            PanelId = panel.Id,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();
        return review;
    }

    /// <summary>A formative review of the same trainee over the same days, on the same panel.</summary>
    private static async Task SeedOverlappingReviewAsync(ApplicationDbContext db)
    {
        db.CommitteeReviews.Add(new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = OverlappingReviewId,
            PanelId = 20,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 2, 1),
            IsFormative = true
        });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// A campaign scheduled to close on 2026-03-10, inside the window, with the timestamps its state implies. Every
    /// state past Open was closed first (a withdrawn one from under review, the case where only the state rule keeps it
    /// out), so each carries <c>ClosedOn</c>; a draft or open one does not, unless <see cref="ClosedInWindow" /> says so.
    /// </summary>
    private static MsfCampaign Campaign(
        int id,
        MsfCampaignState state,
        DateTime? closedOn = null,
        string subjectUserId = "trainee-1")
    {
        var createdOn = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc);
        var closed = closedOn ?? InWindow;
        var hasClosed = state is MsfCampaignState.Closed or MsfCampaignState.UnderReview or MsfCampaignState.Released
            or MsfCampaignState.Withdrawn;

        return new MsfCampaign
        {
            Id = id,
            SubjectUserId = subjectUserId,
            TemplateId = 40,
            CreatedByUserId = "coord-1",
            CreatedOn = createdOn,
            OpensOn = new DateOnly(2026, 1, 5),
            ClosesOn = new DateOnly(2026, 3, 10),
            MinimumResponses = 4,
            MinimumCategoryResponses = 2,
            State = state,
            OpenedOn = state == MsfCampaignState.Draft ? null : createdOn,
            ClosedOn = hasClosed ? closed : null,
            ReleasedOn = state == MsfCampaignState.Released ? closed.AddDays(2) : null,
            WithdrawnOn = state == MsfCampaignState.Withdrawn ? closed.AddDays(1) : null
        };
    }

    /// <summary>
    /// Gives the campaign a close time inside the window whatever its state implies, so a test of the state rule is
    /// not passed by the date rule instead.
    /// </summary>
    private static MsfCampaign ClosedInWindow(MsfCampaign campaign)
    {
        campaign.ClosedOn = InWindow;
        return campaign;
    }

    private static ClaimsPrincipal Principal(string userId, IReadOnlyCollection<string> roles, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

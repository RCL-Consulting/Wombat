using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// A member of a panel starts its reviews, and reads them through their seat, only while they may sit on it: an active
/// committee member at the panel's institution who is not a trainee, and never the trainee under review
/// (<c>PanelSeat.SittingAt</c>, T237's one rule), read from the user store. (T279)
/// </summary>
/// <remarks>
/// <para>
/// Until T279 Start and the read ladder's panel arm (<c>CommitteeDecisionAuthorization.WorksOnPanel</c>) read only the
/// caller's claims, and a circuit's claims are frozen for its life. A member who had lost the CommitteeMember role, moved
/// to another institution or been deactivated could still read the panel's reviews and their frozen evidence, and start
/// one, which freezes the trainee's evidence snapshot and agenda. The chair's actions had been held to the rule since
/// T256, and the appeal body since T237.
/// </para>
/// <para>
/// Each member here signs in as they did before the change: their claims still say CommitteeMember at the panel's
/// institution, and only the user store says they may not sit. A refused Start is followed by the save the audit pipeline
/// makes from its catch and a cleared change tracker, and the store is read back through a second context: a check that
/// ran after a mutation would have the refusal commit it. Each control runs the same request past a member who may sit.
/// </para>
/// </remarks>
public sealed class PanelMemberMaySitTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int GeneralPaediatrics = 11;
    private const int PanelId = 10;
    private const int EpaId = 7;
    private const int LevelId = 3;

    private const string MemberUserId = "member-a";
    private const string TraineeUserId = "paeds-a";

    private const string NotAmongTheReviewsYouCanView = "The committee review could not be found among the reviews you can view.";
    private const string NotAmongTheReviewsYouCanStart = "The committee review could not be found among the reviews you can start.";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    /// <summary>Why the member may not sit on the panel now, as the user store says it.</summary>
    private static readonly string[] ConditionsWhereTheMemberMayNotSit =
    [
        "is no longer a committee member",
        "moved to another institution",
        "was deactivated",
        "was given the Trainee role since signing in",
        "was erased: the id names no account"
    ];

    public static TheoryData<string> MembersWhoMayNotSit
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var condition in ConditionsWhereTheMemberMayNotSit)
            {
                data.Add(condition);
            }

            return data;
        }
    }

    /// <summary>Every read of a review that climbs the one read ladder (<c>DemandReviewAccessAsync</c>).</summary>
    public static TheoryData<string> ReadsOfAReview => new()
    {
        "GetById", "Agenda", "MsfOutsideSnapshot", "Sampling", "PendingDecisions", "StarEpaOptions"
    };

    public static TheoryData<string, string> EveryReadOfEveryMemberWhoMayNotSit
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var read in new[] { "GetById", "Agenda", "MsfOutsideSnapshot", "Sampling", "PendingDecisions", "StarEpaOptions" })
            {
                foreach (var condition in ConditionsWhereTheMemberMayNotSit)
                {
                    data.Add(read, condition);
                }
            }

            return data;
        }
    }

    // ─── Start ───────────────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(MembersWhoMayNotSit))]
    public async Task AMemberWhoMayNotSitNow_IsRefusedStart_WithTheOneRefusal_AndNothingIsFrozen(string condition)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedScheduledReviewAsync(db);
        var before = await SnapshotAsync();

        var start = () => StartAsync(db, reviewId, Member(), DirectoryWhereTheMember(condition));

        (await start.Should().ThrowAsync<UnauthorizedAccessException>(condition))
            .Which.Message.Should().Be(NotAmongTheReviewsYouCanStart, condition);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().Equal(before, condition);
    }

    [Fact]
    public async Task TheControl_AMemberWhoMaySit_StartsTheReview_AndItsEvidenceIsFrozen()
    {
        // The same request, fixture and review as the refusals above, past a member the user store lets sit: it must run
        // to the end, so each refusal is the gate's, not a fixture Start could not run on.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedScheduledReviewAsync(db);

        await StartAsync(db, reviewId, Member(), DirectoryWhereTheMember("may sit"));

        db.ChangeTracker.Clear();
        await using var read = CreateDb();
        var review = await read.CommitteeReviews.SingleAsync(entity => entity.Id == reviewId);
        review.State.Should().Be(CommitteeReviewState.InProgress);
        review.StartedByUserId.Should().Be(MemberUserId);
    }

    [Fact]
    public async Task TheTraineeUnderReview_SeatedOnThePanelAfterLosingTrainee_NeitherStartsNorReadsTheirOwnReview()
    {
        // Seated as a member once they no longer held Trainee, which the panel's save allows, since it knows no review. The
        // store holds them as an active committee member at the panel's institution, so only the "never the trainee under
        // review" arm of the rule stands in the way.
        await using var db = await SeededDbAsync(extraMember: TraineeUserId);
        var reviewId = await SeedScheduledReviewAsync(db);
        var before = await SnapshotAsync();
        var formerTrainee = TestPrincipals.InRole(WombatRoles.CommitteeMember, TraineeUserId, InstitutionA);
        var users = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", MemberUserId, TraineeUserId);

        var start = () => StartAsync(db, reviewId, formerTrainee, users);
        var read = () => ReadAsync(db, "GetById", reviewId, formerTrainee, users);

        (await start.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(NotAmongTheReviewsYouCanStart);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await read.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(NotAmongTheReviewsYouCanView);
        (await SnapshotAsync()).Should().Equal(before);
    }

    // ─── Reading ─────────────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(EveryReadOfEveryMemberWhoMayNotSit))]
    public async Task AMemberWhoMayNotSitNow_ReadsNothingOfTheReview_AndIsGivenTheOneRefusal(string request, string condition)
    {
        // An in-progress review: its evidence is frozen, which is what a member who no longer belongs on the panel must
        // not read.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedStartedReviewAsync(db);
        var because = $"{request}: the member {condition}";

        var read = () => ReadAsync(db, request, reviewId, Member(), DirectoryWhereTheMember(condition));

        (await read.Should().ThrowAsync<UnauthorizedAccessException>(because))
            .Which.Message.Should().Be(NotAmongTheReviewsYouCanView, because);
    }

    [Theory]
    [MemberData(nameof(ReadsOfAReview))]
    public async Task TheControl_AMemberWhoMaySit_ReadsTheReview(string request)
    {
        await using var db = await SeededDbAsync();
        var reviewId = await SeedStartedReviewAsync(db);

        var read = () => ReadAsync(db, request, reviewId, Member(), DirectoryWhereTheMember("may sit"));

        await read.Should().NotThrowAsync(request);
    }

    [Fact]
    public async Task TheReviewsList_ListsTheReview_ToAMemberWhoMaySit_AndToNoMemberWhoMayNot()
    {
        // The list is the read ladder in its set form, so a row is listed exactly when its Open link opens (T218).
        await using var db = await SeededDbAsync();
        var reviewId = await SeedStartedReviewAsync(db);

        (await ListAsync(db, Member(), DirectoryWhereTheMember("may sit"))).Select(row => row.Id)
            .Should().Equal([reviewId], "the control: a member who may sit");

        foreach (var condition in ConditionsWhereTheMemberMayNotSit)
        {
            (await ListAsync(db, Member(), DirectoryWhereTheMember(condition))).Should().BeEmpty(condition);
        }
    }

    // ─── What the review page offers ─────────────────────────────────────────

    [Fact]
    public async Task AMemberWhoMayNotSit_ButReadsTheReviewAsAnInstitutionalAdmin_IsNotOfferedStart_AndIsRefusedIt()
    {
        // The page offers Start by the predicate the handler demands (CallerMayStart, WorksOnReview), so a member who reads
        // the review through another rung, here as an institutional administrator of the panel's institution, is not
        // offered what the handler would refuse.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedScheduledReviewAsync(db);
        var before = await SnapshotAsync();
        var administersA = TestPrincipals.InRoles([WombatRoles.CommitteeMember, WombatRoles.InstitutionalAdmin], MemberUserId, InstitutionA);

        foreach (var condition in ConditionsWhereTheMemberMayNotSit)
        {
            var detail = await new GetCommitteeReviewByIdQueryHandler(db, DirectoryWhereTheMember(condition)).Handle(
                new GetCommitteeReviewByIdQuery(reviewId, administersA), CancellationToken.None);
            var start = () => StartAsync(db, reviewId, administersA, DirectoryWhereTheMember(condition));

            detail.CallerMayStart.Should().BeFalse(condition);
            (await start.Should().ThrowAsync<UnauthorizedAccessException>(condition))
                .Which.Message.Should().Be(NotAmongTheReviewsYouCanStart, condition);
            await SaveAndClearAsAuditPipelineWouldAsync(db);
        }

        (await SnapshotAsync()).Should().Equal(before);

        // The control: the same caller, who may sit, is offered Start.
        var control = await new GetCommitteeReviewByIdQueryHandler(db, DirectoryWhereTheMember("may sit")).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, administersA), CancellationToken.None);
        control.CallerMayStart.Should().BeTrue();
    }

    [Fact]
    public async Task StartIsOfferedOnlyWhileTheReviewIsScheduled()
    {
        // Start takes a scheduled review alone, so the page is told nobody may start one in progress.
        await using var db = await SeededDbAsync();
        var reviewId = await SeedStartedReviewAsync(db);

        var detail = await new GetCommitteeReviewByIdQueryHandler(db, DirectoryWhereTheMember("may sit")).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, Member()), CancellationToken.None);

        detail.CallerMayStart.Should().BeFalse();
    }

    // ─── Requests ────────────────────────────────────────────────────────────

    /// <summary>
    /// The user store, in which the panel's chair and external member may sit, and the member as
    /// <paramref name="condition" /> says. The member's claims never change: they signed in before it did.
    /// </summary>
    private static FakeUserDirectory DirectoryWhereTheMember(string condition)
    {
        var directory = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "external-a")
            .WithTrainees(TraineeUserId);

        return condition switch
        {
            "may sit" => directory.WithCommitteeMembers(InstitutionA, MemberUserId),
            "is no longer a committee member" => directory.With(MemberRecord(InstitutionA, [WombatRoles.Assessor])),
            "moved to another institution" => directory.With(MemberRecord(InstitutionB, [WombatRoles.CommitteeMember])),
            "was deactivated" => directory.With(MemberRecord(InstitutionA, [WombatRoles.CommitteeMember]) with
            {
                IsLockedOut = true,
                IsDeactivated = true
            }),
            "was given the Trainee role since signing in" =>
                directory.With(MemberRecord(InstitutionA, [WombatRoles.CommitteeMember, WombatRoles.Trainee])),
            "was erased: the id names no account" => directory,
            _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, null)
        };

        static UserIdentityDetails MemberRecord(int institutionId, string[] roles)
            => new(MemberUserId, "member@test", "Priya", "Naidoo", institutionId, [], [], roles);
    }

    /// <summary>The member as they signed in: CommitteeMember at the panel's institution.</summary>
    private static ClaimsPrincipal Member() => TestPrincipals.InRole(WombatRoles.CommitteeMember, MemberUserId, InstitutionA);

    private static Task<CommitteeReviewDetailDto> StartAsync(
        ApplicationDbContext db, int reviewId, ClaimsPrincipal caller, IUserAdministrationService users)
        => new StartCommitteeReviewCommandHandler(db, users).Handle(
            new StartCommitteeReviewCommand(reviewId, caller), CancellationToken.None);

    private static async Task ReadAsync(
        ApplicationDbContext db, string request, int reviewId, ClaimsPrincipal caller, IUserAdministrationService users)
    {
        _ = request switch
        {
            "GetById" => (object)await new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
                new GetCommitteeReviewByIdQuery(reviewId, caller, new DateOnly(2027, 1, 8)), CancellationToken.None),
            "Agenda" => await new GetCommitteeAgendaQueryHandler(db, users).Handle(
                new GetCommitteeAgendaQuery(reviewId, caller, new DateOnly(2027, 1, 8)), CancellationToken.None),
            "MsfOutsideSnapshot" => await new CountMsfCampaignsOutsideSnapshotQueryHandler(db, users).Handle(
                new CountMsfCampaignsOutsideSnapshotQuery(reviewId, caller), CancellationToken.None),
            "Sampling" => await new GetSamplingConcentrationWarningsQueryHandler(db, users).Handle(
                new GetSamplingConcentrationWarningsQuery(reviewId, caller), CancellationToken.None),
            "PendingDecisions" => await new ListPendingEntrustmentDecisionsForReviewQueryHandler(db, users).Handle(
                new ListPendingEntrustmentDecisionsForReviewQuery(reviewId, caller), CancellationToken.None),
            "StarEpaOptions" => await new ListStarEpaOptionsForReviewQueryHandler(db, users).Handle(
                new ListStarEpaOptionsForReviewQuery(reviewId, caller), CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }

    private static Task<IReadOnlyList<CommitteeReviewListItemDto>> ListAsync(
        ApplicationDbContext db, ClaimsPrincipal caller, IUserAdministrationService users)
        => new ListReviewsForPanelQueryHandler(db, users).Handle(new ListReviewsForPanelQuery(caller), CancellationToken.None);

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>The panel's scheduled binding review of the paediatric trainee at A.</summary>
    private static async Task<int> SeedScheduledReviewAsync(ApplicationDbContext db)
    {
        var review = NewReview();
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return review.Id;
    }

    /// <summary>The same review, started by the chair: one frozen evidence line and one agenda line.</summary>
    private static async Task<int> SeedStartedReviewAsync(ApplicationDbContext db)
    {
        var review = NewReview();
        review.Start(
            [
                new CommitteeEvidence
                {
                    SourceType = CommitteeEvidenceSourceType.Activity,
                    ActivityId = 900,
                    EpaId = EpaId,
                    SourceLabel = "Mini-CEX #900",
                    Summary = "State: completed.",
                    ObservedOn = new DateOnly(2026, 6, 1)
                }
            ],
            [
                CommitteeAgendaLine.ForCadence(
                    1000, EpaId, "PAED-007", "Triage", isOpportunistic: false,
                    QuotaWindow.For(QuotaPeriod.Semester, new DateOnly(2026, 12, 31), new DateOnly(2024, 1, 15)),
                    new AcademicPeriod(2026, 2))
            ],
            "chair-a",
            new DateTime(2027, 1, 8, 9, 0, 0, DateTimeKind.Utc));

        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return review.Id;
    }

    private static CommitteeReview NewReview()
        => new()
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = TraineeUserId,
            PanelId = PanelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };

    private async Task<ApplicationDbContext> SeededDbAsync(string? extraMember = null)
    {
        var db = CreateDb();

        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true
        });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeUserId,
            InstitutionId = InstitutionA,
            CurriculumId = 100,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        });

        var members = new List<DecisionPanelMember>
        {
            new() { UserId = "chair-a", Role = DecisionPanelMemberRole.Chair },
            new() { UserId = MemberUserId, Role = DecisionPanelMemberRole.Member },
            new() { UserId = "external-a", Role = DecisionPanelMemberRole.External }
        };
        if (extraMember is not null)
        {
            members.Add(new DecisionPanelMember { UserId = extraMember, Role = DecisionPanelMemberRole.Member });
        }

        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelId,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionA,
            CreatedOn = DateTime.UtcNow,
            Members = members
        });

        db.EntrustmentScales.Add(new EntrustmentScale { Id = 1, Name = "v11.1 rungs" });
        db.EntrustmentLevels.Add(new EntrustmentLevel { Id = LevelId, ScaleId = 1, Order = 3, Label = "Indirect supervision" });
        db.Epas.Add(new Epa { Id = EpaId, SubSpecialityId = GeneralPaediatrics, Code = "PAED-007", Title = "Triage", IsActive = true });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1000, CurriculumId = 100, EpaId = EpaId, RequiredCount = 1, MinimumLevelOrder = 3
        });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>Everything Start writes: the review's state and who started it, its frozen evidence and its agenda.</summary>
    private async Task<IReadOnlyList<string>> SnapshotAsync()
    {
        await using var read = CreateDb();
        var rows = new List<string>();
        rows.AddRange(await read.CommitteeReviews.OrderBy(review => review.Id)
            .Select(review => $"review {review.Id}:{review.State}:{review.StartedOn}:{review.StartedByUserId}")
            .ToListAsync());
        rows.AddRange(await read.CommitteeEvidenceItems.OrderBy(line => line.Id)
            .Select(line => $"evidence {line.Id}:{line.ReviewId}:{line.SourceLabel}")
            .ToListAsync());
        rows.AddRange(await read.Set<CommitteeAgendaLine>().OrderBy(line => line.Id)
            .Select(line => $"line {line.Id}:{line.ReviewId}:{line.State}")
            .ToListAsync());
        rows.AddRange(await read.Set<PendingEntrustmentDecision>().OrderBy(pending => pending.Id)
            .Select(pending => $"pending {pending.Id}")
            .ToListAsync());
        return rows;
    }
}

using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T361 (flow 07, lane R): what the review sitting's page reads that it could not before. The trainee's profile id and
/// training year on the review (round 2 review 10, E6), the Start dialog's count of what Start would freeze (review 11),
/// and Start refused while the chair cannot act (Q11, E2).
/// </summary>
public sealed class ReviewSittingReadsTests
{
    private const int Host = 1;
    private const int Elsewhere = 2;
    private const int ReviewId = 30;
    private const int Paed001 = 7;
    private const string ChairName = "Thandi Zulu";

    /// <summary>Lerato Molefe's programme started on this day: year 4 at the end of 2026 S2.</summary>
    private static readonly DateOnly ProgrammeStart = new(2023, 2, 1);

    private static readonly DateOnly WindowFrom = new(2026, 7, 1);
    private static readonly DateOnly WindowTo = new(2026, 12, 31);

    // ---- the review's trainee: the profile id and the training year (review 10, E6) ----------------------------------

    [Fact]
    public async Task TheReview_CarriesTheTraineesPreferredProfile_WhileTheyTrainAtThePanelsInstitution()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var profileId = await db.TraineeProfiles.Where(profile => profile.UserId == "trainee-1").Select(profile => profile.Id).SingleAsync();

        var review = await ReadAsync(db, Chair);

        review.TraineeProfileId.Should().Be(profileId);
    }

    [Fact]
    public async Task TheReview_CarriesNoProfileId_OnceTheTraineeTrainsElsewhere()
    {
        // The preferred profile is the active one, at another institution: the registrar page would not open it for this
        // panel's staff (flow 06), so the review names none.
        await using var db = CreateDb();
        await SeedAsync(db);
        var old = await db.TraineeProfiles.SingleAsync(profile => profile.UserId == "trainee-1");
        old.IsActive = false;
        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = Elsewhere, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2026, 11, 1), ExpectedCompletionDate = new DateOnly(2030, 1, 1)
        });
        await db.SaveChangesAsync();

        var review = await ReadAsync(db, Administrator);

        review.TraineeProfileId.Should().BeNull();
        review.TrainingYear.Should().Be(4, "the year is read from the programme the review sat for, at this institution");
    }

    [Fact]
    public async Task TheTrainingYear_IsTheYearAtTheEndOfTheReviewsPeriod()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        (await ReadAsync(db, Chair)).TrainingYear.Should().Be(4);
    }

    [Fact]
    public async Task TheTrainingYear_OfAnEarlierReview_IsTheYearAtItsOwnPeriodsEnd_NotToday()
    {
        // 2025 S2 ends 2025-12-31, in the third year of a programme started on 2023-02-01.
        await using var db = CreateDb();
        await SeedAsync(db, from: new DateOnly(2025, 7, 1), to: new DateOnly(2025, 12, 31), academicYear: 2025);

        (await ReadAsync(db, Chair)).TrainingYear.Should().Be(3);
    }

    [Fact]
    public async Task TheTrainingYear_OfAnEndedProgramme_IsReadOnItsLastDay()
    {
        // The programme ended before the period did: the year it ended in, as an ended programme is read (T252).
        await using var db = CreateDb();
        await SeedAsync(db);
        var profile = await db.TraineeProfiles.SingleAsync(entity => entity.UserId == "trainee-1");
        profile.Complete(new DateOnly(2026, 1, 20), new DateOnly(2026, 10, 12));
        await db.SaveChangesAsync();

        (await ReadAsync(db, Chair)).TrainingYear.Should().Be(3);
    }

    [Fact]
    public async Task ATraineeWithNoProfile_HasNoProfileIdAndNoYear()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        db.TraineeProfiles.RemoveRange(db.TraineeProfiles);
        await db.SaveChangesAsync();

        var review = await ReadAsync(db, Administrator);

        review.TraineeProfileId.Should().BeNull();
        review.TrainingYear.Should().BeNull();
    }

    // ---- the Start dialog's count (review 11, round 3 item 11) ------------------------------------------------------

    [Fact]
    public async Task ThePreview_CountsWhatStartWouldFreeze_AndTheCampaignsItLeavesOut()
    {
        // Molefe at Act 4: 8 activities and the MSF campaign report, nothing left out.
        await using var db = CreateDb();
        await SeedAsync(db);
        await SeedEvidenceAsync(db, activities: 8);
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.Released));
        await db.SaveChangesAsync();

        var preview = await PreviewAsync(db, Chair);

        preview.Should().Be(new StartPreviewDto(9, 8, 1, WindowFrom, WindowTo, 0));
    }

    [Fact]
    public async Task ThePreview_CountsAClosedUnreleasedCampaign_AsLeftOut()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        await SeedEvidenceAsync(db, activities: 8);
        db.MsfCampaigns.AddRange(Campaign(51, MsfCampaignState.Released), Campaign(52, MsfCampaignState.UnderReview));
        await db.SaveChangesAsync();

        var preview = await PreviewAsync(db, Chair);

        preview!.ItemCount.Should().Be(9, "an unreleased campaign is not evidence (T138)");
        preview.CampaignsLeftOut.Should().Be(1);
    }

    [Fact]
    public async Task ThePreview_IsWhatStartThenFreezes()
    {
        // The same code counts and freezes: the activities in the window in every state, another trainee's never.
        await using var db = CreateDb();
        await SeedAsync(db);
        var type = await SeedEvidenceAsync(db, activities: 5);
        AddActivity(db, type, "draft", new DateOnly(2026, 8, 1));
        AddActivity(db, type, "completed", new DateOnly(2026, 6, 30));
        AddActivity(db, type, "completed", new DateOnly(2026, 8, 2), subject: "trainee-2");
        db.MsfCampaigns.Add(Campaign(51, MsfCampaignState.Released));
        await db.SaveChangesAsync();

        var preview = await PreviewAsync(db, Chair);
        var started = await StartAsync(db, Chair);

        preview!.ItemCount.Should().Be(started.EvidenceItems.Count).And.Be(7);
        preview.ActivityCount.Should().Be(started.EvidenceItems.Count(item => item.ActivityId is not null));
        preview.CampaignReportCount.Should().Be(started.EvidenceItems.Count(item => item.MsfCampaignId is not null));
    }

    [Fact]
    public async Task ThePreview_IsNull_OnceTheReviewHasStarted()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        await StartAsync(db, Chair);

        (await PreviewAsync(db, Chair)).Should().BeNull();
    }

    [Fact]
    public async Task ThePreview_IsNull_ForACallerWhoMayNotReadTheReview()
    {
        // An unknown id and one out of reach alike: nothing about the review is said (T194 item 1).
        await using var db = CreateDb();
        await SeedAsync(db);

        (await PreviewAsync(db, TestPrincipals.InRole(WombatRoles.Coordinator, "coordinator-b", Elsewhere))).Should().BeNull();
        (await PreviewAsync(db, Chair, reviewId: 999)).Should().BeNull();
    }

    // ---- Start while the chair cannot act (Q11, E2) -----------------------------------------------------------------

    [Fact]
    public async Task Start_IsRefused_WhileTheChairCannotAct_WithTheReviewsOwnSentence_AndStagesNothing()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        await SeedEvidenceAsync(db, activities: 2);
        await db.SaveChangesAsync();
        var users = ChairNoLongerSits();
        var coordinator = TestPrincipals.InRole(WombatRoles.Coordinator, "coordinator-a", Host);

        var shown = await new GetCommitteeReviewByIdQueryHandler(db, users).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, coordinator), CancellationToken.None);
        var act = () => new StartCommitteeReviewCommandHandler(db, users).Handle(
            new StartCommitteeReviewCommand(ReviewId, coordinator), CancellationToken.None);

        shown.CallerMayStart.Should().BeTrue("the setup: a coordinator of the institution may start it");
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be(PanelSeat.ChairCannotAct(ChairName)).And.Be(shown.ChairCannotAct);

        db.ChangeTracker.Clear();
        var review = await db.CommitteeReviews.Include(entity => entity.EvidenceItems).Include(entity => entity.AgendaLines)
            .SingleAsync(entity => entity.Id == ReviewId);
        review.State.Should().Be(CommitteeReviewState.Scheduled);
        review.EvidenceItems.Should().BeEmpty();
        review.AgendaLines.Should().BeEmpty();
    }

    [Fact]
    public async Task Start_ByAMember_IsRefusedToo_WhileTheChairCannotAct()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var act = () => new StartCommitteeReviewCommandHandler(db, ChairNoLongerSits()).Handle(
            new StartCommitteeReviewCommand(ReviewId, Member), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(PanelSeat.ChairCannotAct(ChairName));
    }

    [Fact]
    public async Task Start_ByAMember_Starts_WhileTheChairCanAct()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var started = await StartAsync(db, Member);

        started.State.Should().Be(CommitteeReviewState.InProgress);
    }

    // ---- fixture -----------------------------------------------------------------------------------------------------

    private static ClaimsPrincipal Chair => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-1", Host);

    private static ClaimsPrincipal Member => TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-1", Host);

    private static ClaimsPrincipal Administrator => TestPrincipals.Administrator("admin-1");

    /// <summary>The member may sit; the chair holds no CommitteeMember role any more.</summary>
    private static FakeUserDirectory ChairNoLongerSits()
        => new FakeUserDirectory(("chair-1", ChairName), ("member-1", "Priya Naidoo"))
            .WithCommitteeMembers(Host, "member-1")
            .WithTrainees("trainee-1");

    private static Task<CommitteeReviewDetailDto> ReadAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new GetCommitteeReviewByIdQuery(ReviewId, principal, new DateOnly(2026, 10, 12)), CancellationToken.None);

    private static Task<StartPreviewDto?> PreviewAsync(ApplicationDbContext db, ClaimsPrincipal principal, int reviewId = ReviewId)
        => new GetStartPreviewQueryHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new GetStartPreviewQuery(reviewId, principal), CancellationToken.None);

    private static Task<CommitteeReviewDetailDto> StartAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new StartCommitteeReviewCommandHandler(db, FakeUserDirectory.PanelMembersOf(db)).Handle(
            new StartCommitteeReviewCommand(ReviewId, principal), CancellationToken.None);

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task SeedAsync(ApplicationDbContext db, DateOnly? from = null, DateOnly? to = null, int academicYear = 2026)
    {
        db.Epas.Add(new Epa { Id = Paed001, SubSpecialityId = 1, Code = "PAED-001", Title = "Acute admission", IsActive = true });
        db.WbaTools.Add(new WbaTool { Id = 1, Key = "mini_cex", Name = "Mini-CEX" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "Paediatrics", Version = "v11.1" });
        db.MsfTemplates.Add(new MsfTemplate { Id = 40, Name = "Annual MSF" });
        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = Host, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = ProgrammeStart, ExpectedCompletionDate = new DateOnly(2027, 1, 31)
        });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = Host,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        });
        db.CommitteeReviews.Add(new CommitteeReview
        {
            AcademicYear = academicYear,
            Semester = 2,
            Id = ReviewId,
            PanelId = 20,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = from ?? WindowFrom,
            ReviewPeriodTo = to ?? WindowTo,
            ScheduledOn = new DateOnly(2026, 10, 5)
        });
        await db.SaveChangesAsync();
    }

    /// <summary>The shipped Mini-CEX type, and <paramref name="activities" /> completed ones in the window.</summary>
    private static async Task<ActivityType> SeedEvidenceAsync(ApplicationDbContext db, int activities)
    {
        var schema = ReadSeedFile("mini_cex_cpsa", "schema.json");
        var workflow = ReadSeedFile("mini_cex_cpsa", "workflow.json");
        var credit = ReadSeedFile("mini_cex_cpsa", "credit.json");
        var type = new ActivityType
        {
            Key = "mini_cex_cpsa",
            Name = "Mini-CEX (Paediatrics)",
            Version = 1,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = schema,
            WorkflowJson = workflow,
            CreditRulesJson = credit,
            WbaToolKey = "mini_cex"
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = schema,
            WorkflowJson = workflow,
            CreditRulesJson = credit,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);
        await db.SaveChangesAsync();

        for (var day = 0; day < activities; day++)
        {
            AddActivity(db, type, "completed", new DateOnly(2026, 8, 1).AddDays(day));
        }

        return type;
    }

    private static string ReadSeedFile(string key, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, fileName));

    private static void AddActivity(ApplicationDbContext db, ActivityType type, string state, DateOnly observedOn, string subject = "trainee-1")
    {
        var filed = observedOn.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        db.Activities.Add(new Activity
        {
            ActivityTypeId = type.Id,
            SchemaVersion = 1,
            InstitutionId = Host,
            SubjectUserId = subject,
            CreatedByUserId = subject,
            CurrentState = state,
            DataJson = """{ "epa_id": 7, "assessor_user_id": "assessor-a", "overall_level": 3 }""",
            EpaId = Paed001,
            CreatedOn = filed,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared,
            UpdatedOn = filed
        });
    }

    /// <summary>A campaign that closed in the window, 2026-09-10.</summary>
    private static MsfCampaign Campaign(int id, MsfCampaignState state)
    {
        var closed = new DateTime(2026, 9, 10, 18, 0, 0, DateTimeKind.Utc);
        return new MsfCampaign
        {
            Id = id,
            SubjectUserId = "trainee-1",
            TemplateId = 40,
            CreatedByUserId = "coord-1",
            CreatedOn = new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
            OpensOn = new DateOnly(2026, 8, 1),
            ClosesOn = new DateOnly(2026, 9, 10),
            MinimumResponses = 4,
            MinimumCategoryResponses = 2,
            State = state,
            OpenedOn = new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc),
            ClosedOn = closed,
            ReleasedOn = state == MsfCampaignState.Released ? closed.AddDays(2) : null
        };
    }
}

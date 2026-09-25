using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T075 / F-4A-1 scope guards on the committee-review scheduling + view surface.
/// An InstitutionalAdmin can schedule and view reviews for panels in their own
/// institution (the panel's own InstitutionId, whatever its scope: T091/T094) but not for
/// panels in another institution. Mirrors the panel-admin guards in PanelScopeGuardTests.
/// Which trainees may be put before a panel is CommitteeTraineeScopeTests (T182).
/// </summary>
public sealed class ReviewSchedulingScopeGuardTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int SpecialityInA = 4;

    [Fact]
    public async Task Schedule_InstitutionalAdmin_CanScheduleOnOwnInstitutionPanel()
    {
        await using var db = SeededDb();
        var panelId = await AddPanelAsync(db, DecisionPanelScope.Institution, institutionId: InstitutionA);
        var handler = new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db));

        var result = await handler.Handle(
            new ScheduleCommitteeReviewCommand(
                TraineeUserId: "trainee-1",
                PanelId: panelId,
                AcademicYear: 2026,
                Semester: 2,
                ReviewPeriodFrom: new DateOnly(2026, 1, 1),
                ReviewPeriodTo: new DateOnly(2026, 12, 31),
                ScheduledOn: new DateOnly(2027, 1, 8),
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.PanelId.Should().Be(panelId);
        result.TraineeUserId.Should().Be("trainee-1");
        result.State.Should().Be(CommitteeReviewState.Scheduled);
    }

    [Fact]
    public async Task Schedule_InstitutionalAdmin_CanScheduleOnOwnSpecialityPanel()
    {
        await using var db = SeededDb();
        var panelId = await AddPanelAsync(db, DecisionPanelScope.Speciality, institutionId: InstitutionA, specialityId: SpecialityInA);
        var handler = new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db));

        var result = await handler.Handle(
            new ScheduleCommitteeReviewCommand(
                TraineeUserId: "trainee-1",
                PanelId: panelId,
                AcademicYear: 2026,
                Semester: 2,
                ReviewPeriodFrom: new DateOnly(2026, 1, 1),
                ReviewPeriodTo: new DateOnly(2026, 12, 31),
                ScheduledOn: new DateOnly(2027, 1, 8),
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.PanelId.Should().Be(panelId);
    }

    [Fact]
    public async Task Schedule_InstitutionalAdmin_CannotScheduleOnOtherInstitutionPanel()
    {
        await using var db = SeededDb();
        var panelId = await AddPanelAsync(db, DecisionPanelScope.Institution, institutionId: InstitutionB);
        var handler = new ScheduleCommitteeReviewCommandHandler(db, FakeUserDirectory.TraineesOf(db));

        var act = () => handler.Handle(
            new ScheduleCommitteeReviewCommand(
                TraineeUserId: "trainee-1",
                PanelId: panelId,
                AcademicYear: 2026,
                Semester: 2,
                ReviewPeriodFrom: new DateOnly(2026, 1, 1),
                ReviewPeriodTo: new DateOnly(2026, 12, 31),
                ScheduledOn: new DateOnly(2027, 1, 8),
                Principal: TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*your institution*");
    }

    [Fact]
    public async Task List_InstitutionalAdmin_OnlySeesOwnInstitutionReviews()
    {
        await using var db = SeededDb();
        var panelA = await AddPanelAsync(db, DecisionPanelScope.Institution, institutionId: InstitutionA);
        var panelB = await AddPanelAsync(db, DecisionPanelScope.Institution, institutionId: InstitutionB);
        await AddReviewAsync(db, panelA, "trainee-a");
        await AddReviewAsync(db, panelB, "trainee-b");

        var handler = new ListReviewsForPanelQueryHandler(db, FakeUserDirectory.Empty);
        var result = await handler.Handle(
            new ListReviewsForPanelQuery(TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.Select(review => review.TraineeUserId).Should().BeEquivalentTo("trainee-a");
    }

    [Fact]
    public async Task GetById_InstitutionalAdmin_CanViewOwnInstitutionReview()
    {
        await using var db = SeededDb();
        var panelId = await AddPanelAsync(db, DecisionPanelScope.Institution, institutionId: InstitutionA);
        var reviewId = await AddReviewAsync(db, panelId, "trainee-a");

        var handler = new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty);
        var result = await handler.Handle(
            new GetCommitteeReviewByIdQuery(reviewId, TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        result.TraineeUserId.Should().Be("trainee-a");
    }

    [Fact]
    public async Task GetById_InstitutionalAdmin_RejectsOtherInstitutionReview()
    {
        await using var db = SeededDb();
        var panelId = await AddPanelAsync(db, DecisionPanelScope.Institution, institutionId: InstitutionB);
        var reviewId = await AddReviewAsync(db, panelId, "trainee-b");

        var handler = new GetCommitteeReviewByIdQueryHandler(db, FakeUserDirectory.Empty);
        var act = () => handler.Handle(
            new GetCommitteeReviewByIdQuery(reviewId, TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        // The one refusal an unknown id gets too (T194 item 1).
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("The committee review could not be found among the reviews you can view.");
    }

    private static async Task<int> AddPanelAsync(
        ApplicationDbContext db,
        DecisionPanelScope scope,
        int institutionId,
        int? specialityId = null)
    {
        var panel = new DecisionPanel
        {
            Name = $"Panel-{scope}-{institutionId}-{specialityId}",
            Scope = scope,
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            CreatedOn = DateTime.UtcNow
        };
        db.Set<DecisionPanel>().Add(panel);
        await db.SaveChangesAsync();
        return panel.Id;
    }

    private static async Task<int> AddReviewAsync(ApplicationDbContext db, int panelId, string traineeUserId)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = traineeUserId,
            PanelId = panelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };
        db.Set<CommitteeReview>().Add(review);
        await db.SaveChangesAsync();
        return review.Id;
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Set<Institution>().AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = SpecialityInA, CollegeId = InstitutionA, Name = "SpecA", IsActive = true });
        // trainee-1's programme is in SpecA, which the Speciality-scoped panels here cover: such a panel reviews only its
        // own speciality's trainees (T131, T194 item 2).
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 40, SpecialityId = SpecialityInA, Name = "SubSpecA", IsActive = true });
        db.Set<Curriculum>().Add(new Curriculum { Id = 1, SubSpecialityId = 40, Name = "SpecA programme", Version = "1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = InstitutionA, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1)
        });
        db.SaveChanges();
        return db;
    }
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T209 review: the day a programme ended (<see cref="TraineeProfile.EndedOn" />) is what D49 reads, so the admin pages
/// show it. T305: once it is recorded the profile is archived, and no save changes it.
/// </summary>
public sealed class TraineeProfileRecordedEndTests
{
    private const int InstitutionA = 1;
    private static readonly DateOnly Start = new(2023, 1, 15);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_OfAnEndedProfile_IsRefused_AndLeavesNothingToCommit(bool completed)
    {
        // T305: Mark complete says the profile is archived. Before T305 a completed or withdrawn trainee's profile could
        // still be saved, re-pinning its curriculum and re-dating its programme, and with them the graduate's progress,
        // standing and portfolio. It is read-only now; its refusal replaces T209's start-after-end check, which only an
        // ended profile could reach.
        await using var db = SeededDb();
        var profile = await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1);
        var end = new DateOnly(2026, 6, 15);
        if (completed)
        {
            profile.Complete(end, today: end);
        }
        else
        {
            profile.Deactivate(end, today: end);
        }

        await db.SaveChangesAsync();

        var act = () => Handler(db).Handle(
            new UpdateTraineeProfileCommand(1, 2, new DateOnly(2024, 2, 1), new DateOnly(2030, 1, 1), TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This programme ended on 2026-06-15; its record is archived and cannot be changed.");

        // What the audit pipeline does after a throw: save the request's context. Nothing may have changed.
        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the refusal comes before any write");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var stored = await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1);
        (stored.CurriculumId, stored.AdoptionId).Should().Be((1, 1));
        stored.ProgrammeStartDate.Should().Be(Start);
        stored.ExpectedCompletionDate.Should().Be(new DateOnly(2029, 12, 31));
    }

    [Fact]
    public async Task Update_OfARunningProgramme_IsNotHeldToAnyEnd()
    {
        await using var db = SeededDb();

        await Handler(db).Handle(
            new UpdateTraineeProfileCommand(1, 1, new DateOnly(2027, 1, 1), new DateOnly(2030, 1, 1), TestPrincipals.Administrator()),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        (await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1)).ProgrammeStartDate.Should().Be(new DateOnly(2027, 1, 1));
    }

    [Fact]
    public async Task TheProfilePageAndTheList_CarryTheDayAWithdrawnTraineeLeft()
    {
        await using var db = SeededDb();
        var left = new DateOnly(2026, 8, 20);
        (await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1)).Deactivate(left, today: left);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var page = await new GetTraineeProfileByIdQueryHandler(db, Users().Object).Handle(
            new GetTraineeProfileByIdQuery(1, TestPrincipals.Administrator()), CancellationToken.None);
        var listed = (await new ListTraineesForSpecialityQueryHandler(db, Users().Object).Handle(
            new ListTraineesForSpecialityQuery(TestPrincipals.Administrator()), CancellationToken.None)).Single();

        foreach (var dto in new[] { page, listed })
        {
            dto.IsActive.Should().BeFalse();
            dto.DeactivatedOn.Should().Be(left);
            dto.CompletedOn.Should().BeNull();
        }
    }

    [Fact]
    public async Task TheProfilePageAndTheList_CarryTheGraduationDay_AndNoWithdrawal()
    {
        await using var db = SeededDb();
        var graduated = new DateOnly(2026, 11, 20);
        (await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1)).Complete(graduated, today: graduated);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var page = await new GetTraineeProfileByIdQueryHandler(db, Users().Object).Handle(
            new GetTraineeProfileByIdQuery(1, TestPrincipals.Administrator()), CancellationToken.None);
        var listed = (await new ListTraineesForSpecialityQueryHandler(db, Users().Object).Handle(
            new ListTraineesForSpecialityQuery(TestPrincipals.Administrator()), CancellationToken.None)).Single();

        foreach (var dto in new[] { page, listed })
        {
            dto.CompletedOn.Should().Be(graduated);
            dto.DeactivatedOn.Should().BeNull();
        }
    }

    private static UpdateTraineeProfileCommandHandler Handler(ApplicationDbContext db)
        => new(db, Users().Object, new CreditApplier(db), new TraineeCreditLock(db));

    private static Mock<IUserAdministrationService> Users()
    {
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.GetByIdAsync("trainee-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails("trainee-1", "molefe@kgk", "Lerato", "Molefe", InstitutionA, [1], [1], [WombatRoles.Trainee]));
        return users;
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<Institution>().Add(new Institution { Id = InstitutionA, Name = "KGK", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = InstitutionA, Name = "Paediatrics", IsActive = true });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Set<Curriculum>().Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "FCPaed(SA) Part 1", Version = "2026.1" });
        db.Set<Curriculum>().Add(new Curriculum { Id = 2, SubSpecialityId = 1, Name = "FCPaed(SA) Part 1", Version = "2026.2" });
        db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
        {
            Id = 1,
            InstitutionId = InstitutionA,
            CurriculumId = 1,
            SubSpecialityId = 1,
            AdoptedOn = new DateOnly(2023, 1, 1),
            IsActive = true
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = InstitutionA,
            CurriculumId = 1,
            AdoptionId = 1,
            ProgrammeStartDate = Start,
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        });
        db.SaveChanges();
        return db;
    }
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

public sealed class GetCurriculumProgressForTraineeTests
{
    [Fact]
    public async Task ListsEveryCurriculumItem_IncludingThoseWithoutCredit()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        // Only PAED-001 has accrued credit; PAED-002 has none.
        db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            Id = 1, CurriculumItemId = 1, TraineeUserId = "trainee-1",
            CountsSoFar = 1, MinimumLevelReachedCount = 0, LastActivityId = 2,
            LastUpdated = new DateTime(2026, 2, 9, 8, 0, 0, DateTimeKind.Utc)
        });
        db.SaveChanges();

        var handler = new GetCurriculumProgressForTraineeQueryHandler(db);
        var result = await handler.Handle(
            new GetCurriculumProgressForTraineeQuery("trainee-1"), CancellationToken.None);

        result.Select(r => r.EpaCode).Should().Equal("PAED-001", "PAED-002");

        var paed002 = result.Single(r => r.EpaCode == "PAED-002");
        paed002.CompletedCount.Should().Be(0);
        paed002.MinimumLevelReachedCount.Should().Be(0);
        paed002.LastUpdated.Should().BeNull();
        paed002.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task SurfacesCreditedRow_WithVolumeAndLevelReachedSeparately()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        // T071 semantics: a below-min completion counts volume but not the level-reached counter.
        db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            Id = 1, CurriculumItemId = 1, TraineeUserId = "trainee-1",
            CountsSoFar = 1, MinimumLevelReachedCount = 0, LastActivityId = 2,
            LastUpdated = new DateTime(2026, 2, 9, 8, 0, 0, DateTimeKind.Utc)
        });
        db.SaveChanges();

        var handler = new GetCurriculumProgressForTraineeQueryHandler(db);
        var result = await handler.Handle(
            new GetCurriculumProgressForTraineeQuery("trainee-1"), CancellationToken.None);

        var paed001 = result.Single(r => r.EpaCode == "PAED-001");
        paed001.EpaTitle.Should().Be("Clerk an acute admission");
        paed001.CompletedCount.Should().Be(1);
        paed001.RequiredCount.Should().Be(30);
        paed001.MinimumLevelReachedCount.Should().Be(0);
        paed001.EffectiveMinimumLevelOrder.Should().Be(4);
        paed001.IsComplete.Should().BeFalse();
        paed001.LastUpdated.Should().Be(new DateTime(2026, 2, 9, 8, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task NoActiveProfile_ReturnsEmpty()
    {
        await using var db = CreateDb();
        SeedCurriculum(db);
        db.SaveChanges();

        var handler = new GetCurriculumProgressForTraineeQueryHandler(db);
        var result = await handler.Handle(
            new GetCurriculumProgressForTraineeQuery("trainee-without-profile"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedCurriculum(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "KGK" });
        db.Specialities.Add(new Speciality { Id = 1, InstitutionId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });

        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Clerk an acute admission" });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Manage a ward" });

        db.Curricula.Add(new Curriculum
        {
            Id = 1, SubSpecialityId = 1, Name = "FCPaed Part 1",
            Version = "2026.1", EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1, CurriculumId = 1, EpaId = 1, RequiredCount = 30,
            MinimumLevelOrder = 4, WindowMonths = 36
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 2, CurriculumId = 1, EpaId = 2, RequiredCount = 10,
            MinimumLevelOrder = 3, WindowMonths = 36
        });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = "trainee-1", CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2024, 1, 1),
            ExpectedCompletionDate = new DateOnly(2027, 1, 1),
            IsActive = true
        });
    }
}

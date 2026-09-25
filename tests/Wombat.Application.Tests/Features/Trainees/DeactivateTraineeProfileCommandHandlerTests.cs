using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T209: deactivating a trainee records their last day in the programme, which D49 reads. A refusal leaves nothing
/// behind: the audit pipeline saves the request's context even when the handler throws, so each refusal is proven by a
/// save and a cleared tracker before the row is read back.
/// </summary>
public sealed class DeactivateTraineeProfileCommandHandlerTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;

    /// <summary>
    /// 22:30 UTC on 31 March 2027, which is already 1 April in South Africa. Not near the real date, so a handler that
    /// read the system clock would fail.
    /// </summary>
    private static readonly DateTimeOffset Now = new(2027, 3, 31, 22, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_RecordsTheLastDay_AndDeactivates()
    {
        await using var db = SeededDb();

        await Handler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2026, 5, 31), TestPrincipals.Administrator()),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        var profile = await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1);
        profile.IsActive.Should().BeFalse();
        profile.DeactivatedOn.Should().Be(new DateOnly(2026, 5, 31));
        profile.CompletedOn.Should().BeNull();
        profile.EndedOn.Should().Be(new DateOnly(2026, 5, 31));
    }

    [Fact]
    public async Task Handle_RefusesADayBeforeTheProgrammeStart_AndLeavesNothingToCommit()
    {
        await using var db = SeededDb();

        var act = () => Handler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2022, 12, 31), TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*before the programme start*");
        await AssertStillActiveAfterTheAuditSave(db);
    }

    [Fact]
    public async Task Handle_RefusesADayAfterToday_AndLeavesNothingToCommit()
    {
        // T209 review: the profile ends now, so a later day (a resignation's notice date, say) would be an end that has
        // not happened, and D49 would hold the trainee to every period up to it. There is no way to correct it afterwards.
        await using var db = SeededDb();

        var act = () => Handler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2027, 4, 2), TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The deactivation date cannot be after today (2027-04-01).");
        await AssertStillActiveAfterTheAuditSave(db);
    }

    [Fact]
    public async Task Handle_TakesTodayFromTheSouthAfricanCalendar()
    {
        // 1 April is today in South Africa though it is still 31 March in UTC.
        await using var db = SeededDb();

        await Handler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2027, 4, 1), TestPrincipals.Administrator()),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        (await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1)).DeactivatedOn.Should().Be(new DateOnly(2027, 4, 1));
    }

    [Fact]
    public async Task Handle_RefusesAnInactiveProfile_AndKeepsItsRecordedEnd()
    {
        await using var db = SeededDb();
        (await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1)).Complete(new DateOnly(2026, 6, 15), today: new DateOnly(2026, 6, 15));
        await db.SaveChangesAsync();

        var act = () => Handler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2026, 7, 1), TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*active*");
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var profile = await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1);
        profile.DeactivatedOn.Should().BeNull();
        profile.EndedOn.Should().Be(new DateOnly(2026, 6, 15));
    }

    [Fact]
    public async Task Handle_RejectsOutOfInstitution_AndLeavesNothingToCommit()
    {
        await using var db = SeededDb();

        var act = () => Handler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2026, 5, 31), TestPrincipals.InstitutionalAdmin(InstitutionB)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await AssertStillActiveAfterTheAuditSave(db);
    }

    private static DeactivateTraineeProfileCommandHandler Handler(ApplicationDbContext db) => new(db, new FixedClock(Now));

    /// <summary>What the audit pipeline does after a throw: save the request's context. Nothing may have changed.</summary>
    private static async Task AssertStillActiveAfterTheAuditSave(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var profile = await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1);
        profile.IsActive.Should().BeTrue();
        profile.DeactivatedOn.Should().BeNull();
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
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = InstitutionA,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2023, 1, 15),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        });
        db.SaveChanges();
        return db;
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

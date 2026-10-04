using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.GetEpaProgressForTrainee;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// One EPA's page under My progress (T355, round 1 correction 2): one item of the caller's own curriculum, by the EPA's
/// id, read by the code My progress's rows are read by. In force or paused; null for anything that is not an item of
/// their curriculum, so the page says "Page not found". "Today" is pinned by <c>AsOf</c>, or by the clock.
/// </summary>
public sealed class GetEpaProgressForTraineeQueryTests
{
    /// <summary>Semester 2 of 2026.</summary>
    private static readonly DateOnly AsOf = new(2026, 9, 23);

    private const string Trainee = "trainee-1";
    private const int SemesterEpaId = 1;   // PAED-001, three per semester
    private const int YearEpaId = 2;       // PAED-002, one per academic year

    [Fact]
    public async Task OneEpa_ByItsId_IsTheRowMyProgressReads()
    {
        await using var db = CreateDb();
        Seed(db);
        AddRow(db, 1, 2026, 2, counts: 2, reached: 1, lastObservedOn: new DateOnly(2026, 9, 1));
        AddRow(db, 1, 2026, 1, counts: 3, reached: 3, lastObservedOn: new DateOnly(2026, 3, 1));
        db.SaveChanges();

        var page = await Read(db, SemesterEpaId);
        var row = (await new GetCurriculumProgressForTraineeQueryHandler(db).Handle(
                new GetCurriculumProgressForTraineeQuery(Trainee, TestPrincipals.Trainee(Trainee), AsOf), CancellationToken.None))!
            .Items.Single(item => item.EpaId == SemesterEpaId);

        page.Should().NotBeNull();
        page!.Item.Should().Be(row, "the page and My progress's row are read by the same code, so they cannot disagree");
        page.Item.Current.Count.Should().Be(2);
        page.Item.Previous!.Name.Should().Be("Semester 1, 2026");
        page.AsOf.Should().Be(AsOf);
        page.Ended.Should().BeNull();
        page.IsAfterTeachingYear.Should().BeFalse();
    }

    [Fact]
    public async Task TheTrainingYear_AndItsLevel_AreRead()
    {
        // Started 2024-01-01: on 2026-09-23 the trainee is in training year 3, whose level Annexure A's map names.
        await using var db = CreateDb();
        Seed(db);
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 7, Name = "CPSA Paediatric Entrustment Scale v11.1" });
        var labels = new[] { "1", "2", "3a", "3b", "4", "5" };
        for (var order = 1; order <= labels.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 100 + order, ScaleId = 7, Order = order, Label = labels[order - 1] });
        }

        var item = db.CurriculumItems.Local.Single(entry => entry.Id == 1);
        item.ScaleId = 7;
        item.MinimumLevelOrder = 6;
        item.MinimumLevelByStageJson = """{ "3": 5, "4": 6 }""";
        item.DecisionCadence = QuotaPeriod.Semester;
        db.SaveChanges();

        var page = await Read(db, SemesterEpaId);

        page!.TraineeStage.Should().Be(3);
        page.ProgrammeStartDate.Should().Be(new DateOnly(2024, 1, 1));
        page.Item.EffectiveMinimumLevelLabel.Should().Be("4", "training year 3's level on the College's ladder");
        page.Item.MinimumByTrainingYear.Should().BeTrue();
        page.Item.ExitLevelLabel.Should().Be("5");
        page.Item.DecisionCadence.Should().Be(QuotaPeriod.Semester);
    }

    [Fact]
    public async Task APausedEpa_IsStillRead_AndSaysItIsNotInForce()
    {
        await using var db = CreateDb();
        Seed(db);
        AddRow(db, 1, 2026, 2, counts: 3, reached: 3, lastObservedOn: new DateOnly(2026, 8, 12));
        db.SaveChanges();
        db.Epas.Single(epa => epa.Id == SemesterEpaId).Deactivate(DateTime.MinValue);
        db.SaveChanges();

        var page = await Read(db, SemesterEpaId);

        page.Should().NotBeNull("a paused EPA's page says why it is no target (D48)");
        page!.Item.EpaInForce.Should().BeFalse();
        page.Item.EpaCode.Should().Be("PAED-001");
    }

    [Fact]
    public async Task AnEpaNotOnTheCurriculum_AnotherInstitutionsLocalEpa_AndAnUnknownId_AreNotFound()
    {
        await using var db = CreateDb();
        Seed(db);
        db.Epas.Add(new Epa { Id = 9, SubSpecialityId = 1, Code = "PAED-009", Title = "On no curriculum of hers" });
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, OwningInstitutionId = 2, Code = "LOCAL-2", Title = "Someone else's extra" });
        db.CurriculumItems.Add(new CurriculumItem { Id = 3, CurriculumId = 1, EpaId = 3, OwningInstitutionId = 2, RequiredCount = 1, MinimumLevelOrder = 3, WindowMonths = 12 });
        db.SaveChanges();

        (await Read(db, 9)).Should().BeNull("PAED-009 is no item of her curriculum");
        (await Read(db, 3)).Should().BeNull("another institution's local item is never this trainee's");
        (await Read(db, 999)).Should().BeNull("an unknown id is not found, and confirms nothing");
    }

    [Fact]
    public async Task OurOwnInstitutionsLocalEpa_IsRead_WithItsOwner()
    {
        await using var db = CreateDb();
        Seed(db);
        db.Epas.Add(new Epa { Id = 4, SubSpecialityId = 1, OwningInstitutionId = 1, Code = "KGK-001", Title = "Running a paediatric outreach clinic" });
        db.CurriculumItems.Add(new CurriculumItem { Id = 4, CurriculumId = 1, EpaId = 4, OwningInstitutionId = 1, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12 });
        db.SaveChanges();

        var page = await Read(db, 4);

        page!.Item.OwningInstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        page.Item.IsPerSemester.Should().BeFalse();
    }

    [Fact]
    public async Task ACallerWithNoProfile_IsNotFound()
    {
        await using var db = CreateDb();
        Seed(db);
        db.SaveChanges();

        var page = await new GetEpaProgressForTraineeQueryHandler(db, TimeProvider.System).Handle(
            new GetEpaProgressForTraineeQuery(TestPrincipals.Trainee("someone-else"), SemesterEpaId, AsOf), CancellationToken.None);

        page.Should().BeNull("the page reads the caller's own record, and they hold none");
    }

    [Fact]
    public async Task AnEndedProgramme_IsReadAsOnItsLastDay_WithEveryPeriod()
    {
        // R5: an ended programme's EPA page shows its periods read-only, as My progress's record does (T252).
        await using var db = CreateDb();
        Seed(db);
        AddRow(db, 1, 2025, 2, counts: 3);
        AddRow(db, 1, 2026, 1, counts: 2);
        db.SaveChanges();
        db.Set<TraineeProfile>().Local.Single().Complete(new DateOnly(2026, 6, 30), today: new DateOnly(2026, 6, 30));
        db.SaveChanges();
        db.ChangeTracker.Clear();

        var page = await Read(db, SemesterEpaId);

        page!.Ended.Should().Be(new ProgrammeEndDto(Completed: true, EndedOn: new DateOnly(2026, 6, 30), Today: AsOf));
        page.AsOf.Should().Be(new DateOnly(2026, 6, 30));
        page.Item.Periods.Should().NotBeNull();
        page.Item.Periods!.Select(period => period.Name).Should().Equal(
            "Semester 1, 2026", "Semester 2, 2025", "Semester 1, 2025", "Semester 2, 2024", "Semester 1, 2024");
    }

    [Fact]
    public async Task Today_IsTheSouthAfricanDate_ByTheClock()
    {
        // 22:30 UTC on 30 November is already 1 December in South Africa: after the College's teaching year (D40, T325).
        await using var db = CreateDb();
        Seed(db);
        db.SaveChanges();
        var clock = new FixedClock(new DateTimeOffset(2026, 11, 30, 22, 30, 0, TimeSpan.Zero));

        var page = await new GetEpaProgressForTraineeQueryHandler(db, clock).Handle(
            new GetEpaProgressForTraineeQuery(TestPrincipals.Trainee(Trainee), SemesterEpaId), CancellationToken.None);

        page!.AsOf.Should().Be(new DateOnly(2026, 12, 1));
        page.IsAfterTeachingYear.Should().BeTrue();
    }

    private static Task<EpaProgressDto?> Read(ApplicationDbContext db, int epaId)
        => new GetEpaProgressForTraineeQueryHandler(db, TimeProvider.System).Handle(
            new GetEpaProgressForTraineeQuery(TestPrincipals.Trainee(Trainee), epaId, AsOf), CancellationToken.None);

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static void AddRow(
        ApplicationDbContext db, int curriculumItemId, int year, int semester, int counts, int reached = 0, DateOnly? lastObservedOn = null)
        => db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = curriculumItemId,
            TraineeUserId = Trainee,
            AcademicYear = year,
            Semester = semester,
            CountsSoFar = counts,
            MinimumLevelReachedCount = reached,
            LastObservedOn = lastObservedOn,
            LastObservedOnDeclared = lastObservedOn is not null,
            LastUpdated = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        });

    private static void Seed(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "Kgosi Kgari Teaching Hospital" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });

        db.Epas.Add(new Epa { Id = SemesterEpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Clerk an acute admission" });
        db.Epas.Add(new Epa { Id = YearEpaId, SubSpecialityId = 1, Code = "PAED-002", Title = "Manage a ward" });

        db.Curricula.Add(new Curriculum
        {
            Id = 1, SubSpecialityId = 1, Name = "FCPaed Part 1",
            Version = "2026.1", EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });

        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1, CurriculumId = 1, EpaId = SemesterEpaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4, WindowMonths = 36
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 2, CurriculumId = 1, EpaId = YearEpaId, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 3, WindowMonths = 36
        });

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = Trainee, InstitutionId = 1, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2024, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = true
        });
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

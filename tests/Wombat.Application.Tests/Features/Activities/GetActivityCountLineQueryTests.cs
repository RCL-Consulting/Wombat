using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Features.Activities.Queries.GetActivityCountLine;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;
using static Wombat.Application.Tests.TestHelpers.TraineeCounts;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// The completed card's count (T355, C5; E5; note 4): the registrar's own activity only, read by <c>EpaCountLines</c>, the
/// reader Home's Recent decisions shares. Anyone else, and an unknown id, reads null, so the answer never says the id
/// exists.
/// </summary>
public sealed class GetActivityCountLineQueryTests
{
    private const string Dlamini = "trainee-dlamini";
    private const string Molefe = "trainee-molefe";

    [Fact]
    public async Task HerOwnCompletedActivity_ReadsItsCountLine()
    {
        await using var db = Seeded();
        AddActivity(db, 1, Dlamini, Paed001, new DateOnly(2026, 9, 23));

        var line = await ReadAsync(db, TestPrincipals.Trainee(Dlamini, HostInstitution), 1);

        line.Should().NotBeNull();
        (line!.EpaId, line.EpaCode, line.IsCurrentWindow).Should().Be((Paed001, "PAED-001", true));
        (line.Window.Count, line.Window.Target).Should().Be((1, 3));
    }

    [Fact]
    public async Task AnotherTraineesActivity_AnAssessorsView_AndAnUnknownId_ReadNull()
    {
        await using var db = Seeded();
        AddActivity(db, 1, Dlamini, Paed001, new DateOnly(2026, 9, 23));
        AddActivity(db, 2, Molefe, Paed001, new DateOnly(2026, 9, 24));

        (await ReadAsync(db, TestPrincipals.Trainee(Dlamini, HostInstitution), 2)).Should().BeNull("it is Molefe's");
        (await ReadAsync(db, AssessorReads.CreatePrincipal("assessor-1"), 1)).Should().BeNull("the count is hers alone");
        (await ReadAsync(db, TestPrincipals.Administrator(), 1)).Should().BeNull("an administrator reads it, but is not its subject");
        (await ReadAsync(db, TestPrincipals.Trainee(Dlamini, HostInstitution), 999)).Should().BeNull();
    }

    [Fact]
    public async Task AnActivityAboutNoEpa_OrAPausedEpa_ReadsNull()
    {
        await using var db = Seeded();
        AddActivity(db, 1, Dlamini, epaId: null, new DateOnly(2026, 9, 23));
        AddActivity(db, 2, Dlamini, Paed012, new DateOnly(2026, 9, 26));

        (await ReadAsync(db, TestPrincipals.Trainee(Dlamini, HostInstitution), 1)).Should().BeNull();
        (await ReadAsync(db, TestPrincipals.Trainee(Dlamini, HostInstitution), 2)).Should().BeNull();
    }

    private static ApplicationDbContext Seeded()
    {
        var db = AssessorReads.CreateDb();
        SeedCurriculum(db);
        SeedTrainee(db, Dlamini, new DateOnly(2023, 1, 15), profileId: 1);
        SeedTrainee(db, Molefe, new DateOnly(2023, 1, 14), profileId: 2);
        Credit(db, Dlamini, Paed001, 2026, 2, 1);
        Credit(db, Molefe, Paed001, 2026, 2, 2);
        ShippedSeeds.AddType(db, 21, "mini_cex_cpsa", "Mini-CEX (Paediatrics)");
        db.SaveChanges();
        return db;
    }

    private static void AddActivity(ApplicationDbContext db, int id, string subject, int? epaId, DateOnly observedOn)
    {
        db.Activities.Add(new Activity
        {
            Id = id, ActivityTypeId = 21, SchemaVersion = 1, SubjectUserId = subject, CreatedByUserId = subject,
            CurrentState = "completed", DataJson = AssessorReads.NamesTheAssessor, EpaId = epaId, ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared, InstitutionId = HostInstitution,
            CreatedOn = TodayUtc.AddDays(-2), UpdatedOn = TodayUtc.AddDays(-1)
        });
        db.SaveChanges();
    }

    private static Task<EpaCountLineDto?> ReadAsync(ApplicationDbContext db, ClaimsPrincipal principal, int activityId)
        => new GetActivityCountLineQueryHandler(db, new AssessorReads.FixedClock(TodayUtc))
            .Handle(new GetActivityCountLineQuery(principal, activityId), CancellationToken.None);
}

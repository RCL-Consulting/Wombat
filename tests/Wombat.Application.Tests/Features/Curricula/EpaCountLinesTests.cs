using FluentAssertions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Infrastructure.Persistence;
using static Wombat.Application.Tests.TestHelpers.TraineeCounts;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// The count a decision made (T355, C5; E5; round 1, correction 5): the window the encounter counts towards, tallied from
/// today's progress rows, named when it is not the current one. One reader for Home's Recent decisions and the completed
/// card, so the two say the same count.
/// </summary>
public sealed class EpaCountLinesTests
{
    private const string Dlamini = "trainee-dlamini";

    [Fact]
    public async Task TheCurrentWindow_IsTalliedFromTodaysRows_AndMarkedCurrent()
    {
        await using var db = Seeded();
        Credit(db, Dlamini, Paed001, 2026, 2, 3);

        var line = (await ReadAsync(db, (Paed001, new DateOnly(2026, 9, 23))))[(Paed001, new DateOnly(2026, 9, 23))];

        line.EpaCode.Should().Be("PAED-001");
        line.QuotaPeriod.Should().Be(QuotaPeriod.Semester);
        line.IsCurrentWindow.Should().BeTrue();
        (line.Window.Name, line.Window.Count, line.Window.Target, line.Window.IsMet).Should().Be(("Semester 2, 2026", 3, 3, true));
    }

    [Fact]
    public async Task AnEncounterInAnEarlierWindow_CountsThere_AndTheWindowIsNotCurrent()
    {
        // E5: a Semester 1 encounter completed in Semester 2 counts towards Semester 1, read live.
        await using var db = Seeded();
        Credit(db, Dlamini, Paed001, 2026, 1, 1);
        Credit(db, Dlamini, Paed001, 2026, 2, 3);

        var key = (Paed001, new DateOnly(2026, 6, 20));
        var line = (await ReadAsync(db, key))[key];

        line.IsCurrentWindow.Should().BeFalse();
        (line.Window.Name, line.Window.Count, line.Window.Shortfall, line.Window.IsMet)
            .Should().Be(("Semester 1, 2026", 1, 2, false));
    }

    [Fact]
    public async Task AYearlyItem_CountsBothSemesters_OfTheAcademicYear()
    {
        await using var db = Seeded();
        Credit(db, Dlamini, Paed008, 2026, 1, 1);

        var key = (Paed008, new DateOnly(2026, 3, 1));
        var line = (await ReadAsync(db, key))[key];

        line.QuotaPeriod.Should().Be(QuotaPeriod.AcademicYear);
        line.IsCurrentWindow.Should().BeTrue("the 2026 academic year contains today");
        (line.Window.Name, line.Window.Count, line.Window.IsMet).Should().Be(("2026 academic year", 1, true));
    }

    [Fact]
    public async Task AWindowTheStartWaives_HoldsNoTarget_ButKeepsItsCount()
    {
        // D14: a registrar who started part-way through Semester 2, 2026 is held to no semester target until Semester 1, 2027.
        await using var db = Seeded();
        SeedTrainee(db, "trainee-late", new DateOnly(2026, 8, 18), profileId: 2);
        Credit(db, "trainee-late", Paed002, 2026, 2, 2);

        var key = (Paed002, new DateOnly(2026, 9, 1));
        var line = (await EpaCountLines.ReadAsync(db, "trainee-late", [key], Today, CancellationToken.None))[key];

        line.IsCurrentWindow.Should().BeTrue();
        line.Window.Applies.Should().BeFalse();
        line.Window.IsExempt.Should().BeTrue();
        line.Window.Count.Should().Be(2);
    }

    [Fact]
    public async Task APausedEpa_AnEpaOffHerCurriculum_AndAnotherInstitutionsOwnItem_HaveNoLine()
    {
        await using var db = Seeded();
        Credit(db, Dlamini, Paed012, 2026, 2, 1);

        var day = new DateOnly(2026, 9, 26);
        var lines = await ReadAsync(db, (Paed012, day), (OffCurriculum, day), (Oth001, day), (Kgk001, day));

        lines.Keys.Should().Equal([(Kgk001, day)], "only her own institution's item is hers; a paused EPA is no target (D48)");
    }

    [Fact]
    public async Task ADecemberEncounter_CountsTowardsSemesterTwo()
    {
        // D40: December counts into Semester 2 of its year.
        await using var db = Seeded();
        Credit(db, Dlamini, Paed001, 2026, 2, 2);

        var key = (Paed001, new DateOnly(2026, 12, 5));
        var line = (await EpaCountLines.ReadAsync(db, Dlamini, [key], new DateOnly(2026, 12, 10), CancellationToken.None))[key];

        line.Window.Name.Should().Be("Semester 2, 2026");
        line.IsCurrentWindow.Should().BeTrue();
        line.Window.Count.Should().Be(2);
    }

    [Fact]
    public async Task ATraineeWithNoProfile_HasNoLines()
    {
        await using var db = Seeded();

        var lines = await EpaCountLines.ReadAsync(
            db, "nobody", [(Paed001, new DateOnly(2026, 9, 23))], Today, CancellationToken.None);

        lines.Should().BeEmpty();
    }

    private static ApplicationDbContext Seeded()
    {
        var db = Wombat.Application.Tests.TestHelpers.AssessorReads.CreateDb();
        SeedCurriculum(db);
        // Training year 4 on 2026-10-03.
        SeedTrainee(db, Dlamini, new DateOnly(2023, 1, 15), profileId: 1);
        return db;
    }

    private static Task<IReadOnlyDictionary<(int EpaId, DateOnly ObservedOn), EpaCountLineDto>> ReadAsync(
        ApplicationDbContext db, params (int EpaId, DateOnly ObservedOn)[] keys)
        => EpaCountLines.ReadAsync(db, Dlamini, keys, Today, CancellationToken.None);
}

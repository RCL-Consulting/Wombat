using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T196 review: the query rule credit and the rebuild actually run (<see cref="CurriculumItemsInForce.InForceAt" />)
/// agrees with <see cref="Epa.InForceAt" /> at every moment, the pause's first instant included.
/// </summary>
/// <remarks>
/// The boundary was pinned only on <see cref="Epa.InForceAt" />, which no production code calls: turning the query's
/// <c>moment &lt; DeactivatedOn</c> into <c>&lt;=</c> passed every suite. The moment the pause begins is inside it, so a
/// completion stamped at that instant is paused, and a reactivation credits it.
/// </remarks>
public sealed class CurriculumItemsInForceAtTests
{
    private const int CurriculumId = 3000;
    private const int ActiveEpaId = 5000;
    private const int PausedEpaId = 5001;
    private const int UnstampedEpaId = 5002;

    private static readonly DateTime PausedFrom = new(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task TheMomentThePauseBegins_IsInsideIt_AndTheInstantBeforeIsNot()
    {
        await using var db = await SeededAsync();

        (await EpasInForceAt(db, PausedFrom)).Should().BeEquivalentTo([ActiveEpaId],
            "a completion credited at the instant the pause began is paused");
        (await EpasInForceAt(db, PausedFrom.AddTicks(-1))).Should().BeEquivalentTo([ActiveEpaId, PausedEpaId],
            "a completion credited the instant before was judged while the EPA was in force");
    }

    [Fact]
    public async Task TheQueryRule_AgreesWithTheDomainRule_AtEveryMoment()
    {
        await using var db = await SeededAsync();
        var epas = await db.Epas.AsNoTracking().ToListAsync();

        DateTime[] moments =
        [
            new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            PausedFrom.AddDays(-1),
            PausedFrom.AddTicks(-1),
            PausedFrom,
            PausedFrom.AddTicks(1),
            PausedFrom.AddDays(30)
        ];

        foreach (var moment in moments)
        {
            (await EpasInForceAt(db, moment)).Should().BeEquivalentTo(
                epas.Where(epa => epa.InForceAt(moment)).Select(epa => epa.Id),
                $"the two rules must agree at {moment:O}");
        }
    }

    private static async Task<List<int>> EpasInForceAt(ApplicationDbContext db, DateTime moment)
        => await db.CurriculumItems
            .Where(item => item.CurriculumId == CurriculumId)
            .InForceAt(moment)
            .Select(item => item.EpaId)
            .ToListAsync();

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var college = new College { Id = 1, Name = "CPSA", ShortCode = "CPSA" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = 1, College = college };
        var subSpeciality = new SubSpeciality { Id = 1, Name = "Paediatrics", SpecialityId = 1, Speciality = speciality };
        db.Colleges.Add(college);
        db.Specialities.Add(speciality);
        db.SubSpecialities.Add(subSpeciality);

        var paused = new Epa { Id = PausedEpaId, Code = "PAED-002", Title = "Paused", SubSpecialityId = 1 };
        paused.Deactivate(PausedFrom);

        db.Epas.AddRange(
            new Epa { Id = ActiveEpaId, Code = "PAED-001", Title = "In force", SubSpecialityId = 1 },
            paused,
            // Inactive with no recorded pause: the database refuses one (CK_Epas_DeactivatedOn), and both rules read it as
            // in force at no moment.
            new Epa { Id = UnstampedEpaId, Code = "PAED-003", Title = "Unstamped", SubSpecialityId = 1, IsActive = false });

        var curriculum = new Curriculum
        {
            Id = CurriculumId,
            SubSpecialityId = 1,
            SubSpeciality = subSpeciality,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };

        var itemId = 7000;
        foreach (var epaId in new[] { ActiveEpaId, PausedEpaId, UnstampedEpaId })
        {
            curriculum.Items.Add(new CurriculumItem
            {
                Id = itemId++, EpaId = epaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
                MinimumLevelOrder = 3, WindowMonths = 12
            });
        }

        db.Curricula.Add(curriculum);
        await db.SaveChangesAsync();

        return db;
    }
}

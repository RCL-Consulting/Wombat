using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T312 on a real PostgreSQL server: the migration gives every profile that had already ended the list it ended with, in
/// the shape <c>EndedCurriculumItem</c> reads back from <c>jsonb</c> (which re-renders what it stores), and the record
/// reads it.
/// </summary>
/// <remarks>
/// The migration's SQL runs nowhere else: the unit suites use EF InMemory, and on a fresh database no profile has ended
/// when it runs. So the profiles are written on the current schema, the migration is taken down (dropping the column, the
/// rows kept) and run up again, as it ran on the databases that held ended profiles. Each test works in a schema of its
/// own, as <c>ProgrammeEndCreditRacePostgresTests</c>.
/// </remarks>
public sealed class EndedCurriculumListPostgresTests : IAsyncLifetime
{
    private const string MigrationBeforeT312 = "20261023132146_T376_AssessorProfileTrainingOnly";
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    private static readonly DateOnly LastDay = new(2026, 9, 26);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheMigration_KeepsEachEndedProfilesList_FromTheCatalogue_AndTheRecordReadsIt()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int graduate, running, unrecorded, pausedBeforeEpaId, pausedAfterEpaId, institutionId, curriculumId;
            int pausedLateOnTheLastDayEpaId, pausedAtItsCloseEpaId, pausedAfterItsCloseEpaId, ownLocalItemId, otherLocalItemId;

            await using (var db = NewContext(schema))
            {
                institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
                curriculumId = await db.Curricula.Where(entity => entity.Name == PaediatricCurriculumName).Select(entity => entity.Id).SingleAsync();
                var epas = await db.CurriculumItems
                    .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null)
                    .OrderBy(item => item.Epa.Code)
                    .Select(item => item.Epa)
                    .ToListAsync();

                // One EPA paused before her last day (no target when the programme ended) and one after it (still on it).
                epas[0].Deactivate(new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc));
                epas[1].Deactivate(new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc));
                pausedBeforeEpaId = epas[0].Id;
                pausedAfterEpaId = epas[1].Id;

                // T312 review (R3): the close of the last day is South African midnight, 22:00 UTC (ProgrammeCalendar.EndOf),
                // and the list holds what was in force at that moment (InForceAt: the pause began strictly after it). A pause a
                // second before it, or at it, is off the list; one a second after it is on it.
                epas[2].Deactivate(new DateTime(2026, 9, 26, 21, 59, 59, DateTimeKind.Utc));
                epas[3].Deactivate(new DateTime(2026, 9, 26, 22, 0, 0, DateTimeKind.Utc));
                epas[4].Deactivate(new DateTime(2026, 9, 26, 22, 0, 1, DateTimeKind.Utc));
                pausedLateOnTheLastDayEpaId = epas[2].Id;
                pausedAtItsCloseEpaId = epas[3].Id;
                pausedAfterItsCloseEpaId = epas[4].Id;

                // An item of the graduate's own institution (on her list) and one of another institution's (never hers).
                var subSpecialityId = await db.Curricula.Where(entity => entity.Id == curriculumId)
                    .Select(entity => entity.SubSpecialityId).SingleAsync();
                var elsewhere = new Wombat.Domain.Institutions.Institution { Name = "Elsewhere T312", ShortCode = "ELS312" };
                db.Institutions.Add(elsewhere);
                await db.SaveChangesAsync();
                var ownEpa = new Wombat.Domain.Epas.Epa
                {
                    SubSpecialityId = subSpecialityId, OwningInstitutionId = institutionId, Code = "OWN-312", Title = "Our own"
                };
                var otherEpa = new Wombat.Domain.Epas.Epa
                {
                    SubSpecialityId = subSpecialityId, OwningInstitutionId = elsewhere.Id, Code = "ELS-312", Title = "Theirs"
                };
                db.Epas.AddRange(ownEpa, otherEpa);
                await db.SaveChangesAsync();
                var ownItem = LocalItem(curriculumId, ownEpa.Id, institutionId);
                var otherItem = LocalItem(curriculumId, otherEpa.Id, elsewhere.Id);
                db.CurriculumItems.AddRange(ownItem, otherItem);
                await db.SaveChangesAsync();
                (ownLocalItemId, otherLocalItemId) = (ownItem.Id, otherItem.Id);

                var ended = Profile("graduate-t312", institutionId, curriculumId);
                ended.Complete(LastDay, LastDay);
                var active = Profile("registrar-t312", institutionId, curriculumId);
                var noDay = Profile("withdrawn-t312", institutionId, curriculumId);
                noDay.IsActive = false;
                db.TraineeProfiles.AddRange(ended, active, noDay);
                await db.SaveChangesAsync();
                (graduate, running, unrecorded) = (ended.Id, active.Id, noDay.Id);
            }

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(MigrationBeforeT312);
            }

            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
                (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
            }

            await using (var db = NewContext(schema))
            {
                var expected = await db.CurriculumItems.AsNoTracking()
                    .Where(item => item.CurriculumId == curriculumId &&
                                   (item.OwningInstitutionId == null || item.OwningInstitutionId == institutionId) &&
                                   item.EpaId != pausedBeforeEpaId &&
                                   item.EpaId != pausedLateOnTheLastDayEpaId &&
                                   item.EpaId != pausedAtItsCloseEpaId)
                    .OrderBy(item => item.Id)
                    .ToListAsync();
                expected.Should().HaveCountGreaterThan(10, "guard: the v11.1 catalogue is seeded");

                var profiles = await db.TraineeProfiles.AsNoTracking()
                    .Where(entity => entity.Id == graduate || entity.Id == running || entity.Id == unrecorded)
                    .ToDictionaryAsync(entity => entity.Id);

                profiles[running].EndedCurriculumJson.Should().BeNull("a running programme has no list");
                profiles[unrecorded].EndedCurriculumJson.Should().BeNull("with no last day there is nothing to judge it on");

                var list = profiles[graduate].ReadEndedCurriculum()!;
                list.Should().BeEquivalentTo(
                    expected.Select(EndedCurriculumItem.Of),
                    options => options.WithStrictOrdering(),
                    "every item in force on the last day, with its target, as the commands keep it");
                list.Select(item => item.EpaId).Should().Contain(pausedAfterEpaId).And.NotContain(pausedBeforeEpaId);
                list.Select(item => item.EpaId).Should().Contain(pausedAfterItsCloseEpaId, "a pause at 22:00:01 UTC began after the last day")
                    .And.NotContain(pausedLateOnTheLastDayEpaId, "a pause at 21:59:59 UTC began on the last day")
                    .And.NotContain(pausedAtItsCloseEpaId, "at 22:00 UTC the EPA was no longer in force, as InForceAt judges it");
                list.Should().ContainSingle(item => item.CurriculumItemId == ownLocalItemId).Which.IsLocal.Should().BeTrue();
                list.Select(item => item.CurriculumItemId).Should().NotContain(otherLocalItemId);

                var record = (await TraineeQuotaProgressReader.ReadAsync(db, "graduate-t312", new DateOnly(2026, 10, 20), CancellationToken.None))!;
                record.Items.Select(item => item.CurriculumItemId).Should().BeEquivalentTo(expected.Select(item => item.Id));
                record.Items.Single(item => item.EpaId == pausedAfterEpaId).EpaInForce.Should().BeFalse();
                (await ProgrammeItems.ReadAsync(db, profiles[graduate], CancellationToken.None))
                    .Should().HaveCount(expected.Count);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Wombat.Domain.Curricula.CurriculumItem LocalItem(int curriculumId, int epaId, int institutionId) => new()
    {
        CurriculumId = curriculumId, EpaId = epaId, OwningInstitutionId = institutionId, RequiredCount = 1,
        QuotaPeriod = Wombat.Domain.Curricula.QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
    };

    private static TraineeProfile Profile(string userId, int institutionId, int curriculumId) => new()
    {
        UserId = userId,
        InstitutionId = institutionId,
        CurriculumId = curriculumId,
        ProgrammeStartDate = new DateOnly(2023, 1, 15),
        ExpectedCompletionDate = new DateOnly(2027, 1, 15),
        IsActive = true
    };

    private async Task<string> SeededSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}

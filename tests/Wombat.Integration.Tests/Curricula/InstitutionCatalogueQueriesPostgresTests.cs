using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T392 (flow 15) build review C3: the institution-state, adoptable and EPA queries on a real PostgreSQL server, against
/// the seeded CPSA paediatric catalogue. The unit suites run them on EF InMemory, which evaluates in memory what Npgsql has
/// to translate: <c>GetAdoptableCurricula</c>'s correlated item count inside a projection, <c>CurriculumInstitutionStates</c>'
/// <c>GroupBy</c> over the profiles, and <c>EpaReads.ListAsync</c>'s correlated owner lookup. Each is run once here, and its
/// answer checked, so a query EF cannot translate fails this suite rather than the page.
/// </summary>
/// <remarks>
/// The world: v11.1 (seeded, fifteen national items) and a 11.2 the College releases with one national item. The adopter
/// adopted 11.1, keeps an item of its own (LOC-A01) and two registrars on it, one active and one ended. Another institution
/// has an own EPA (LOC-B01) the adopter must not read. Schema helpers as <see cref="CurriculumAdminScopePostgresTests" />.
/// </remarks>
public sealed class InstitutionCatalogueQueriesPostgresTests : IAsyncLifetime
{
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_TheAdoptableVersions_ReadTheirCountsAndWhatAdoptingWouldSupersede()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                var mine = await new GetAdoptableCurriculaQueryHandler(db).Handle(
                    new GetAdoptableCurriculaQuery(InstitutionalAdmin(world.Adopter)), CancellationToken.None);
                var chosen = await new GetAdoptableCurriculaQueryHandler(db).Handle(
                    new GetAdoptableCurriculaQuery(Administrator(), world.Adopter), CancellationToken.None);

                var next = mine.Should().ContainSingle(row => row.Id == world.NextVersion).Subject;
                next.Relation.Should().Be(AdoptionRelation.Newer);
                next.NationalItemCount.Should().Be(1);
                next.AdoptedCurriculumId.Should().Be(world.CurriculumId);
                next.AdoptedVersion.Should().Be("11.1");
                next.AdoptedRegistrarCount.Should().Be(1, "the ended registrar is not still on it");
                next.OwnItemsNotCarried.Should().Equal("LOC-A01");
                mine.Should().NotContain(row => row.Id == world.CurriculumId, "an actively adopted version is no offer (Q7)");
                chosen.Select(row => row.Id).Should().BeEquivalentTo(mine.Select(row => row.Id),
                    "the Administrator reads what the institution's admin reads");
            }

            commands.Texts.Should().Contain(text => text.Contains("\"CurriculumItems\"", StringComparison.Ordinal),
                "the national item count is read in SQL");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task OnPostgres_TheCurriculaList_ReadsTheInstitutionsState_ForItsAdminAndForTheAdministratorsChoice()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            await using var db = NewContext(schema);
            var mine = await new GetCurriculaListQueryHandler(db).Handle(
                new GetCurriculaListQuery(InstitutionalAdmin(world.Adopter)), CancellationToken.None);
            var chosen = await new GetCurriculaListQueryHandler(db).Handle(
                new GetCurriculaListQuery(Administrator(), world.Adopter), CancellationToken.None);

            foreach (var list in new[] { mine, chosen })
            {
                var state = list.Should().ContainSingle(curriculum => curriculum.Id == world.CurriculumId).Subject.Institution!;
                state.InstitutionId.Should().Be(world.Adopter);
                state.IsActiveAdoption.Should().BeTrue();
                state.RegistrarCount.Should().Be(1, "active registrars only");
                state.NationalItemCount.Should().Be(15);
                state.OwnItemCount.Should().Be(1);
            }

            mine.Single(curriculum => curriculum.Id == world.CurriculumId).MayAddItem.Should().BeTrue("the institution's active adoption (F6)");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task OnPostgres_TheEpaListAndPage_ReadEachCallersEpas_WithTheirOwners()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                var mine = await new GetEpaListQueryHandler(db).Handle(new GetEpaListQuery(InstitutionalAdmin(world.Adopter)), CancellationToken.None);
                mine.Epas.Count(epa => epa.OwningInstitutionId is null).Should().Be(15, "the national EPAs of the discipline it adopted");
                mine.Epas.Where(epa => epa.OwningInstitutionId is not null).Select(epa => epa.Code).Should().Equal("LOC-A01");
                mine.InstitutionName.Should().Be("Adopting Academic");
                mine.MayCreate.Should().BeTrue();

                var all = await new GetEpaListQueryHandler(db).Handle(new GetEpaListQuery(Administrator()), CancellationToken.None);
                all.Epas.Single(epa => epa.Code == "LOC-B01").OwningInstitutionName.Should().Be("Other Academic",
                    "the Administrator reads whose own each EPA is");
            }

            commands.Texts.Where(text => text.Contains("FROM \"Epas\"", StringComparison.Ordinal))
                .Should().Contain(text => text.Contains("\"Institutions\"", StringComparison.Ordinal),
                    "the owner is a correlated lookup in the list's own statement");

            await using (var db = NewContext(schema))
            {
                var own = await new GetEpaPageQueryHandler(db).Handle(
                    new GetEpaPageQuery(world.AdopterEpa, InstitutionalAdmin(world.Adopter)), CancellationToken.None);
                own.Epa!.MayChange.Should().BeTrue();
                own.Disciplines.Select(discipline => discipline.SubSpecialityId).Should().Equal(world.SubSpecialityId);

                var foreign = await new GetEpaPageQueryHandler(db).Handle(
                    new GetEpaPageQuery(world.OtherEpa, InstitutionalAdmin(world.Adopter)), CancellationToken.None);
                foreign.Epa.Should().BeNull("another institution's own EPA is unavailable");

                var create = await new GetEpaPageQueryHandler(db).Handle(
                    new GetEpaPageQuery(null, InstitutionalAdmin(world.Adopter)), CancellationToken.None);
                create.CreatesOwn.Should().BeTrue();
                create.MayCreate.Should().BeTrue();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private sealed record World(int CurriculumId, int NextVersion, int SubSpecialityId, int Adopter, int Other, int AdopterEpa, int OtherEpa);

    private async Task<World> SeedWorldAsync(string schema)
    {
        await using var db = NewContext(schema);

        var curriculum = await db.Curricula
            .Where(entity => entity.Name == PaediatricCurriculumName)
            .Select(entity => new { entity.Id, entity.SubSpecialityId })
            .SingleAsync();
        var firstNational = await db.Set<CurriculumItem>()
            .Where(item => item.CurriculumId == curriculum.Id && item.OwningInstitutionId == null)
            .OrderBy(item => item.Id)
            .Select(item => item.EpaId)
            .FirstAsync();

        var adopter = new Institution { Name = "Adopting Academic", ShortCode = "T392-A" };
        var other = new Institution { Name = "Other Academic", ShortCode = "T392-B" };
        db.Institutions.AddRange(adopter, other);
        await db.SaveChangesAsync();

        var adopterEpa = new Epa { Code = "LOC-A01", Title = "The adopter's own", SubSpecialityId = curriculum.SubSpecialityId, OwningInstitutionId = adopter.Id };
        var otherEpa = new Epa { Code = "LOC-B01", Title = "The other's own", SubSpecialityId = curriculum.SubSpecialityId, OwningInstitutionId = other.Id };
        db.Epas.AddRange(adopterEpa, otherEpa);

        var next = new Curriculum
        {
            SubSpecialityId = curriculum.SubSpecialityId,
            Name = PaediatricCurriculumName,
            Version = "11.2",
            EffectiveFrom = new DateOnly(2027, 1, 1),
            IsActive = true
        };
        db.Curricula.Add(next);
        await db.SaveChangesAsync();

        db.Set<CurriculumItem>().AddRange(
            Item(curriculum.Id, adopterEpa.Id, adopter.Id),
            Item(next.Id, firstNational, owner: null));
        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption
        {
            InstitutionId = adopter.Id,
            CurriculumId = curriculum.Id,
            SubSpecialityId = curriculum.SubSpecialityId,
            AdoptedOn = new DateOnly(2026, 1, 1),
            IsActive = true
        });
        db.TraineeProfiles.AddRange(
            Profile("t392-active", adopter.Id, curriculum.Id, isActive: true),
            Profile("t392-ended", adopter.Id, curriculum.Id, isActive: false));
        await db.SaveChangesAsync();

        return new World(curriculum.Id, next.Id, curriculum.SubSpecialityId, adopter.Id, other.Id, adopterEpa.Id, otherEpa.Id);
    }

    private static CurriculumItem Item(int curriculumId, int epaId, int? owner)
        => new()
        {
            CurriculumId = curriculumId,
            EpaId = epaId,
            OwningInstitutionId = owner,
            RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        };

    private static TraineeProfile Profile(string userId, int institutionId, int curriculumId, bool isActive)
        => new()
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 15),
            ExpectedCompletionDate = new DateOnly(2029, 1, 14),
            IsActive = isActive
        };

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => Principal(
            new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
            new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture)));

    private static ClaimsPrincipal Administrator()
        => Principal(new Claim(ClaimTypes.Role, WombatRoles.Administrator));

    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "t392-admin"), .. claims],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

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

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>Every query EF sends through one context, so "in SQL" is asserted on the wire.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Texts.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}

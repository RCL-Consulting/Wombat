using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Configurations;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T223 on a real PostgreSQL server, against the seeded CPSA paediatric catalogue: an EPA is on a curriculum once for each
/// institution's trainees. Two institutions each hold an item of their own on one EPA; a second national item on an EPA, a
/// second item of one institution's own on an EPA, and a national item beside an institution's own on one EPA are each
/// refused, by the commands and, below them, by the database.
/// </summary>
/// <remarks>
/// <para>
/// Before T223 one unique index held one item per EPA per curriculum whoever owned it (T091), so one institution's own item
/// on a national EPA kept every other institution from it. Only PostgreSQL enforces the indexes and the exclusion
/// constraint, and only the migration writes the constraint, so these run here, on a migrated schema.
/// </para>
/// <para>
/// The schema helpers follow <see cref="CurriculumAdminScopePostgresTests" />: one schema per test, registered before it is
/// created, dropped in a <c>finally</c> and again from <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class CurriculumItemEpaPerOwnerPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    private const string NationalBesideLocal =
        "This curriculum already contains the selected EPA, as an institution's own item. A national item cannot share an EPA with an institution's own item: that institution's trainees would be measured against the EPA twice.";

    private const string LocalBesideNational =
        "This curriculum already contains the selected EPA, as a national item. An institution's own item adds an EPA to the national curriculum and cannot repeat one on it.";

    private const string AlreadyContains = "This curriculum already contains the selected EPA.";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task TwoInstitutions_EachAddAnItemOfTheirOwn_OnOneEpaOfTheCurriculum_AndTheCollegeCannotAddItBesideThem()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            await using (var db = NewContext(schema))
            {
                (await PickerAsync(db, world.CurriculumId, InstitutionalAdmin(world.InstitutionA))).Should().Contain("PAED-099");
                await AddAsync(db, world, world.Paed099, InstitutionalAdmin(world.InstitutionA));
            }

            await using (var db = NewContext(schema))
            {
                (await PickerAsync(db, world.CurriculumId, InstitutionalAdmin(world.InstitutionB))).Should().Contain("PAED-099",
                    "institution A's own item holds PAED-099 for A's trainees, not B's");
                await AddAsync(db, world, world.Paed099, InstitutionalAdmin(world.InstitutionB));
            }

            await using (var db = NewContext(schema))
            {
                (await db.Set<CurriculumItem>()
                        .Where(item => item.CurriculumId == world.CurriculumId && item.EpaId == world.Paed099)
                        .Select(item => item.OwningInstitutionId)
                        .ToListAsync())
                    .Should().BeEquivalentTo(new int?[] { world.InstitutionA, world.InstitutionB });

                (await PickerAsync(db, world.CurriculumId, InstitutionalAdmin(world.InstitutionA))).Should().NotContain("PAED-099",
                    "A holds it already");
                var second = () => AddAsync(db, world, world.Paed099, InstitutionalAdmin(world.InstitutionA));
                (await second.Should().ThrowAsync<InvalidOperationException>()).Which.Message
                    .Should().Be("This curriculum already contains the selected EPA.");
            }

            await using (var db = NewContext(schema))
            {
                var college = CollegeAdmin(world.CollegeId);
                (await PickerAsync(db, world.CurriculumId, college)).Should().NotContain("PAED-099",
                    "a national item beside A's and B's own would measure their trainees against PAED-099 twice");
                var national = () => AddAsync(db, world, world.Paed099, college);
                (await national.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(NationalBesideLocal);

                (await PickerAsync(db, world.CurriculumId, InstitutionalAdmin(world.InstitutionA))).Should().NotContain("PAED-001",
                    "the national item holds PAED-001 for every institution");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task TheDatabase_RefusesASecondItemOnAnEpaForTheSameTrainees_WhateverWritesIt()
    {
        // Below the commands, as a racing write would reach it. Each insert on a context of its own: a refused one stays
        // tracked, and the next save would try it again.
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            await InsertAsync(schema, Item(world.CurriculumId, world.Paed099, world.InstitutionA));

            await InsertAsync(schema, Item(world.CurriculumId, world.Paed099, world.InstitutionB));

            (await RefusalAsync(schema, Item(world.CurriculumId, world.Paed001, owner: null)))
                .Should().Be((PostgresErrorCodes.UniqueViolation, CurriculumItemConfiguration.NationalEpaIndexName),
                    "a national duplicate is still refused");

            (await RefusalAsync(schema, Item(world.CurriculumId, world.Paed099, world.InstitutionA)))
                .Should().Be((PostgresErrorCodes.UniqueViolation, CurriculumItemConfiguration.LocalEpaIndexName),
                    "one institution holds one item of its own on an EPA");

            (await RefusalAsync(schema, Item(world.CurriculumId, world.Paed099, owner: null)))
                .Should().Be((PostgresErrorCodes.ExclusionViolation, CurriculumItemConfiguration.EpaOncePerInstitutionConstraintName),
                    "a national item beside an institution's own on one EPA");

            (await RefusalAsync(schema, Item(world.CurriculumId, world.Paed001, world.InstitutionA)))
                .Should().Be((PostgresErrorCodes.ExclusionViolation, CurriculumItemConfiguration.EpaOncePerInstitutionConstraintName),
                    "an institution's own item beside a national one on one EPA");

            await using var db = NewContext(schema);
            (await db.Set<CurriculumItem>().CountAsync(item => item.CurriculumId == world.CurriculumId && item.EpaId == world.Paed099))
                .Should().Be(2, "A's and B's own items, and nothing refused");
            (await db.Set<CurriculumItem>().CountAsync(item => item.CurriculumId == world.CurriculumId && item.EpaId == world.Paed001))
                .Should().Be(1);

            // On another curriculum the same EPA is free: every rule is per curriculum.
            var otherCurriculum = new Curriculum
            {
                SubSpecialityId = world.SubSpecialityId,
                Name = PaediatricCurriculumName,
                Version = "T223",
                EffectiveFrom = new DateOnly(2027, 1, 1),
                IsActive = false
            };
            db.Curricula.Add(otherCurriculum);
            await db.SaveChangesAsync();
            await InsertAsync(schema, Item(otherCurriculum.Id, world.Paed099, owner: null));
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    public enum Adder
    {
        College,
        InstitutionA
    }

    public enum RacingItem
    {
        National,
        InstitutionAsOwn
    }

    [Theory]
    [InlineData(Adder.InstitutionA, RacingItem.National, LocalBesideNational)]
    [InlineData(Adder.College, RacingItem.InstitutionAsOwn, NationalBesideLocal)]
    [InlineData(Adder.InstitutionA, RacingItem.InstitutionAsOwn, AlreadyContains)]
    [InlineData(Adder.College, RacingItem.National, AlreadyContains)]
    public async Task AnAddThatLosesTheRaceForItsEpa_IsRefusedInTheChecksWords_AndSavesNothing(Adder adder, RacingItem racing, string message)
    {
        // T223 review. The command's own check passed, and another item took the EPA before its save: the College adding
        // PAED-099 nationally while institution A adds it as its own, say. The database refuses the loser (a unique index,
        // or the exclusion constraint), and before the review the page showed EF's "An error occurred while saving the
        // entity changes".
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);
            var racingOwner = racing == RacingItem.National ? (int?)null : world.InstitutionA;
            var principal = adder == Adder.College ? CollegeAdmin(world.CollegeId) : InstitutionalAdmin(world.InstitutionA);

            await using (var db = NewContext(schema, beforeFirstSave: () => InsertAsync(schema, Item(world.CurriculumId, world.Paed099, racingOwner))))
            {
                var add = () => AddAsync(db, world, world.Paed099, principal);
                var refusal = (await add.Should().ThrowAsync<InvalidOperationException>()).Which;
                refusal.Message.Should().Be(message);
                refusal.InnerException.Should().BeOfType<DbUpdateException>(
                    "the database's refusal stays underneath, so the audit pipeline discards the refused save (T201)");
            }

            await using var read = NewContext(schema);
            (await read.Set<CurriculumItem>()
                    .Where(item => item.CurriculumId == world.CurriculumId && item.EpaId == world.Paed099)
                    .Select(item => item.OwningInstitutionId)
                    .ToListAsync())
                .Should().Equal(new[] { racingOwner }, "the racing item, and nothing of the refused add");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task AnUpdateThatLosesTheRaceForItsEpa_IsRefusedInTheChecksWords_AndSavesNothing()
    {
        // Institution A moves its own item from PAED-098 to PAED-099, and the College adds PAED-099 nationally first.
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            int paed098;
            await using (var db = NewContext(schema))
            {
                var epa = new Epa { Code = "PAED-098", Title = "Another national EPA not on the curriculum", SubSpecialityId = world.SubSpecialityId };
                db.Epas.Add(epa);
                await db.SaveChangesAsync();
                paed098 = epa.Id;
            }

            var aItem = Item(world.CurriculumId, paed098, world.InstitutionA);
            await InsertAsync(schema, aItem);

            await using (var db = NewContext(schema, beforeFirstSave: () => InsertAsync(schema, Item(world.CurriculumId, world.Paed099, owner: null))))
            {
                var update = () => new UpdateCurriculumItemCommandHandler(db).Handle(
                    new UpdateCurriculumItemCommand(world.CurriculumId, aItem.Id, world.Paed099, 7, QuotaPeriod.AcademicYear, 3, 12,
                        null, null, null, null, null, false, InstitutionalAdmin(world.InstitutionA)),
                    CancellationToken.None);
                var refusal = (await update.Should().ThrowAsync<InvalidOperationException>()).Which;
                refusal.Message.Should().Be(LocalBesideNational);
                refusal.InnerException.Should().BeOfType<DbUpdateException>();
            }

            await using var read = NewContext(schema);
            var stored = await read.Set<CurriculumItem>().SingleAsync(item => item.Id == aItem.Id);
            stored.EpaId.Should().Be(paed098, "the refused move saved nothing");
            stored.RequiredCount.Should().Be(1);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private sealed record World(int CurriculumId, int SubSpecialityId, int CollegeId, int InstitutionA, int InstitutionB, int Paed001, int Paed099);

    /// <summary>
    /// Two institutions that each adopted the paediatric curriculum, and PAED-099, a national EPA of its sub-speciality that
    /// the College has not put on it. PAED-001 is on it as a national item, from the catalogue.
    /// </summary>
    private async Task<World> SeedWorldAsync(string schema)
    {
        await using var db = NewContext(schema);

        var curriculum = await db.Curricula
            .Where(entity => entity.Name == PaediatricCurriculumName)
            .Select(entity => new { entity.Id, entity.SubSpecialityId, entity.SubSpeciality.Speciality.CollegeId })
            .SingleAsync();
        var paed001 = await db.Epas.Where(epa => epa.Code == "PAED-001" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();

        var institutionA = new Institution { Name = "Institution A Academic", ShortCode = "T223-A" };
        var institutionB = new Institution { Name = "Institution B Academic", ShortCode = "T223-B" };
        db.Institutions.AddRange(institutionA, institutionB);

        var paed099 = new Epa { Code = "PAED-099", Title = "A national EPA not on the curriculum", SubSpecialityId = curriculum.SubSpecialityId };
        db.Epas.Add(paed099);
        await db.SaveChangesAsync();

        db.InstitutionCurriculumAdoptions.AddRange(
            Adoption(institutionA.Id, curriculum.Id, curriculum.SubSpecialityId),
            Adoption(institutionB.Id, curriculum.Id, curriculum.SubSpecialityId));
        await db.SaveChangesAsync();

        return new World(curriculum.Id, curriculum.SubSpecialityId, curriculum.CollegeId, institutionA.Id, institutionB.Id, paed001, paed099.Id);
    }

    private static InstitutionCurriculumAdoption Adoption(int institutionId, int curriculumId, int subSpecialityId)
        => new()
        {
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            SubSpecialityId = subSpecialityId,
            AdoptedOn = new DateOnly(2026, 1, 1),
            IsActive = true
        };

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

    private async Task InsertAsync(string schema, CurriculumItem item)
    {
        await using var db = NewContext(schema);
        db.Set<CurriculumItem>().Add(item);
        await db.SaveChangesAsync();
    }

    /// <summary>The SQL state and the constraint the server names when it refuses the insert.</summary>
    private async Task<(string SqlState, string? ConstraintName)> RefusalAsync(string schema, CurriculumItem item)
    {
        var insert = () => InsertAsync(schema, item);
        var refusal = (await insert.Should().ThrowAsync<DbUpdateException>()).Which.InnerException
            .Should().BeOfType<PostgresException>().Subject;
        return (refusal.SqlState, refusal.ConstraintName);
    }

    private static Task<CurriculumDto> AddAsync(ApplicationDbContext db, World world, int epaId, ClaimsPrincipal principal)
        => new AddCurriculumItemCommandHandler(db).Handle(
            new AddCurriculumItemCommand(world.CurriculumId, epaId, 1, QuotaPeriod.AcademicYear, 3, 12, null, null, null, null, null, false, principal),
            CancellationToken.None);

    private static async Task<IReadOnlyList<string>> PickerAsync(ApplicationDbContext db, int curriculumId, ClaimsPrincipal principal)
        => (await new ListCurriculumItemEpaOptionsQueryHandler(db).Handle(
                new ListCurriculumItemEpaOptionsQuery(curriculumId, null, principal), CancellationToken.None))
            .Select(epa => epa.Code)
            .ToList();

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => Principal(
            new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
            new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture)));

    private static ClaimsPrincipal CollegeAdmin(int collegeId)
        => Principal(
            new Claim(ClaimTypes.Role, WombatRoles.CollegeAdmin),
            new Claim(WombatClaimTypes.CollegeId, collegeId.ToString(CultureInfo.InvariantCulture)));

    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "t223-admin"), .. claims],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private async Task<string> SeededSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

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

    /// <summary>A new, empty schema, registered for dropping before it exists so that no failure can leak it.</summary>
    private async Task<string> CreateSchemaAsync()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{schema}\"";
        await command.ExecuteNonQueryAsync();

        return schema;
    }

    /// <summary>Drops every schema this test created. Called from the test's finally and again from DisposeAsync.</summary>
    private async Task DropSchemasAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas.ToList())
        {
            // Belt and braces: this class only ever drops a schema it named itself.
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    private ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (beforeFirstSave is not null)
        {
            options.AddInterceptors(new BeforeFirstSave(beforeFirstSave));
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>Runs the racing write once, after the command's checks and just before its first save reaches the database.</summary>
    private sealed class BeforeFirstSave(Func<Task> action) : SaveChangesInterceptor
    {
        private Func<Task>? _action = action;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref _action, null) is { } run)
            {
                await run();
            }

            return result;
        }
    }

    /// <summary>The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.</summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>MsfRespondEndpointFlowTests</c>.</summary>
    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }
}

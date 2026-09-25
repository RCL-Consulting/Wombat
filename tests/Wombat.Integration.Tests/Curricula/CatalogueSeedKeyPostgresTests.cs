using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T221 on a real PostgreSQL server: <see cref="PaediatricCatalogueSeeder" /> finds the College, its speciality and
/// sub-speciality, the v11.1 ladder, the fifteen EPAs and the curriculum by a seed key an administrator cannot edit, and
/// creates the catalogue only on a database that holds none of it.
/// </summary>
/// <remarks>
/// <para>
/// Before T221 each row was found by a value an administrator can edit, and created when the lookup missed. Renaming the
/// sub-speciality produced a second sub-speciality, fifteen duplicate EPAs and a second curriculum at the next boot;
/// changing CPSA's short code but not its name made the next boot insert a second College of the same name, which the
/// unique index on <c>Colleges.Name</c> refused, and startup stopped. Only Postgres enforces that index, so these run here.
/// </para>
/// <para>
/// The keys are typed from the task, not read from the seeder, so a key the seeder and the migration both changed would
/// still fail here. Each edit locates its row by the name it had before, as an administrator's edit page does, so the
/// same test fails against the pre-T221 seeder for the reason the defect names. The schema helpers follow
/// <see cref="WbaToolAllowListPostgresTests" />: one schema per test, registered before it is created, dropped in a
/// <c>finally</c> and again from <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class CatalogueSeedKeyPostgresTests : IAsyncLifetime
{
    private const string T221Migration = "20260925031417_T221_CatalogueSeedKeys";

    private const string CollegeKey = "cpsa";
    private const string SpecialityKey = "cpsa:paediatrics";
    private const string SubSpecialityKey = "cpsa:paediatrics:paediatrics";
    private const string ScaleKey = "cpsa:scale:v11.1";
    private const string CurriculumKey = "cpsa:paediatrics:curriculum:v11.1";

    private const string CollegeName = "College of Paediatricians of South Africa";
    private const string ScaleName = "CPSA Paediatric Entrustment Scale v11.1";
    private const string CurriculumName = "Paediatric EPA Curriculum";

    /// <summary>The fifteen EPA codes of CPSA EPA v11.1, PAED-001 to PAED-015.</summary>
    private static readonly string[] EpaCodes = Enumerable.Range(1, 15)
        .Select(number => $"PAED-{number.ToString("000", CultureInfo.InvariantCulture)}")
        .ToArray();

    private static string EpaKey(string code) => $"cpsa:paediatrics:epa:{code}";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    // ---- a fresh database ------------------------------------------------------------------------------------------------

    /// <summary>
    /// The create path: one boot seeds the whole catalogue, each row carrying its key and hanging off the right parent, and
    /// a second boot writes nothing and announces nothing.
    /// </summary>
    [Fact]
    public async Task AFreshDatabase_SeedsTheWholeCatalogueOnce_EveryRowCarryingItsSeedKey_AndASecondBootWritesNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            var firstBoot = await BootAsync(schema);
            firstBoot.Writes.Should().NotBeEmpty("guard: the write counter sees the first boot, so an empty second boot means something");
            firstBoot.Warnings.Should().BeEmpty();

            var keys = await SeedKeysAsync(schema);
            keys.Keys.Should().BeEquivalentTo(ExpectedKeys(), "every catalogue row carries its key, and no other row carries one");

            var collegeId = keys[("Colleges", CollegeKey)];
            var specialityId = keys[("Specialities", SpecialityKey)];
            var subSpecialityId = keys[("SubSpecialities", SubSpecialityKey)];
            var scaleId = keys[("EntrustmentScales", ScaleKey)];
            var curriculumId = keys[("Curricula", CurriculumKey)];

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Colleges" WHERE "Name" = $1""", CollegeName)).Should().Be(1);
            (await ScalarAsync<int>(schema, """SELECT "CollegeId" FROM "Specialities" WHERE "Id" = $1""", specialityId)).Should().Be(collegeId);
            (await ScalarAsync<int>(schema, """SELECT "SpecialityId" FROM "SubSpecialities" WHERE "Id" = $1""", subSpecialityId)).Should().Be(specialityId);
            (await ScalarAsync<int>(schema, """SELECT "DefaultEntrustmentScaleId" FROM "SubSpecialities" WHERE "Id" = $1""", subSpecialityId))
                .Should().Be(scaleId, "the sub-speciality is created defaulting to the ladder (T187)");
            (await ScalarAsync<int>(schema, """SELECT "SubSpecialityId" FROM "Curricula" WHERE "Id" = $1""", curriculumId)).Should().Be(subSpecialityId);

            (await QueryAsync(schema, """SELECT "Label" FROM "EntrustmentLevels" WHERE "ScaleId" = $1 ORDER BY "Order" """, reader => reader.GetString(0), scaleId))
                .Should().Equal("1", "2", "3a", "3b", "4", "5");

            // Numbered in catalogue order, as they were when each EPA and item had a save of its own.
            (await QueryAsync(schema, """SELECT "Code" FROM "Epas" WHERE "SubSpecialityId" = $1 AND "OwningInstitutionId" IS NULL ORDER BY "Id" """, reader => reader.GetString(0), subSpecialityId))
                .Should().Equal(EpaCodes);
            (await QueryAsync(
                    schema,
                    """
                    SELECT e."Code", ci."ScaleId" FROM "CurriculumItems" AS ci JOIN "Epas" AS e ON e."Id" = ci."EpaId"
                    WHERE ci."CurriculumId" = $1 ORDER BY ci."Id"
                    """,
                    reader => (reader.GetString(0), reader.GetInt32(1)),
                    curriculumId))
                .Should().Equal(EpaCodes.Select(code => (code, scaleId)), "one item per EPA, each pinned to the ladder (T174)");

            var census = await CensusAsync(schema);

            var secondBoot = await BootAsync(schema);
            secondBoot.Writes.Should().BeEmpty("a second boot finds every row by its key");
            secondBoot.Warnings.Should().BeEmpty();
            (await CensusAsync(schema)).Should().Be(census);
            (await SeedKeysAsync(schema)).Should().BeEquivalentTo(keys);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// One key names one row, and any number of rows an administrator made carry none. Seven tables since T229.
    /// </summary>
    [Fact]
    public async Task TheSeedKeyIndexes_AreUniqueWhereAKeyIsSet_OnEveryKeyedTable()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);

            (await QueryAsync(
                    schema,
                    """
                    SELECT tablename, indexdef FROM pg_indexes
                    WHERE schemaname = current_schema() AND indexname LIKE 'IX\_%\_SeedKey'
                    ORDER BY tablename
                    """,
                    reader => (Table: reader.GetString(0), Definition: reader.GetString(1))))
                .Should().SatisfyRespectively(
                    ExpectedIndexedTables().Select(table => new Action<(string Table, string Definition)>(index =>
                    {
                        index.Table.Should().Be(table);
                        index.Definition.Should().StartWith("CREATE UNIQUE INDEX").And.EndWith("WHERE (\"SeedKey\" IS NOT NULL)");
                    })).ToArray());

            var secondCpsa = () => ExecuteAsync(
                schema,
                """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive", "SeedKey") VALUES ('Another', 'ANOTHER', now(), TRUE, 'cpsa')""");
            (await secondCpsa.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);

            (await ExecuteAsync(
                    schema,
                    """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('An administrator''s College', 'ADMIN-1', now(), TRUE), ('Another administrator''s College', 'ADMIN-2', now(), TRUE)"""))
                .Should().Be(2, "rows an administrator makes carry no key, and nulls never collide");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The catalogue is created in one save, so a first boot that stops half way leaves none of it behind, and the next
    /// boot seeds it whole. Were it several saves, the rows before the failure would stay; every later boot would find
    /// them, announce the rest as missing and never create it.
    /// </summary>
    [Fact]
    public async Task AFirstBootThatFailsHalfWayThroughTheCatalogue_LeavesNoneOfIt_AndTheNextBootSeedsItWhole()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            await using (var db = NewContext(schema))
            {
                await new DataSeeder(db).SeedAsync();
            }

            var crash = new CrashOnInsertInto("Curricula");
            await using (var db = NewContext(schema, crash))
            {
                var firstBoot = () => new PaediatricCatalogueSeeder(db).SeedAsync();
                (await firstBoot.Should().ThrowAsync<Exception>())
                    .Where(thrown => Chain(thrown).OfType<SimulatedCrashException>().Any(), "the boot stopped where the test stopped it");
            }

            // What the curriculum's foreign keys need goes in earlier batches of the same save, so it reached the server.
            crash.SentBeforeTheCrash.Should().Contain(
                new[] { "Colleges", "EntrustmentScales", "Specialities", "SubSpecialities" },
                "guard: rows ahead of the curriculum reached the server before the crash, so their absence below is a rollback");

            (await SeedKeysAsync(schema)).Should().BeEmpty("the save that failed took every catalogue row with it");
            (await CatalogueCensusAsync(schema)).Should().Be(new Census(0, 0, 0, 0, 0, 0, 0, 0, 0));

            var nextBoot = await BootAsync(schema);

            nextBoot.Warnings.Should().BeEmpty("nothing of the catalogue is left to be found half made");
            (await SeedKeysAsync(schema)).Keys.Should().BeEquivalentTo(ExpectedKeys());
            (await CatalogueCensusAsync(schema)).Should().Be(new Census(1, 1, 1, 1, 6, 15, 1, 15, 0));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- renames between two boots ---------------------------------------------------------------------------------------

    /// <summary>
    /// An administrator's edit of each row, located as their edit page would find it: by what it was called.
    /// </summary>
    public static TheoryData<string, string> Renames => new()
    {
        {
            "the College's short code, its name kept (before T221, startup stopped on the unique College name)",
            """UPDATE "Colleges" SET "ShortCode" = 'CPSA-2027' WHERE "ShortCode" = 'CPSA'"""
        },
        {
            "the College's name and short code",
            """UPDATE "Colleges" SET "Name" = 'College of Paediatricians (SA)', "ShortCode" = 'CPAED' WHERE "ShortCode" = 'CPSA'"""
        },
        {
            "the speciality",
            """UPDATE "Specialities" SET "Name" = 'Paediatrics and Child Health' WHERE "Name" = 'Paediatrics'"""
        },
        {
            "the sub-speciality (before T221, a second sub-speciality, fifteen EPAs and a curriculum)",
            """UPDATE "SubSpecialities" SET "Name" = 'General Paediatrics' WHERE "Name" = 'Paediatrics'"""
        },
        {
            "the v11.1 ladder",
            """UPDATE "EntrustmentScales" SET "Name" = 'Paediatric entrustment ladder' WHERE "Name" = 'CPSA Paediatric Entrustment Scale v11.1'"""
        },
        {
            "an EPA's code",
            """UPDATE "Epas" SET "Code" = 'PAED-01' WHERE "Code" = 'PAED-001' AND "OwningInstitutionId" IS NULL"""
        },
        {
            "the curriculum's name and version",
            """UPDATE "Curricula" SET "Name" = 'Paediatrics EPA Curriculum', "Version" = '11.1-local' WHERE "Name" = 'Paediatric EPA Curriculum' AND "Version" = '11.1'"""
        },
    };

    [Theory]
    [MemberData(nameof(Renames))]
    public async Task ARenamedCatalogueRow_IsFoundByItsSeedKey_AndTheNextBootCreatesNothing(string row, string rename)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            var census = await CensusAsync(schema);
            var keys = await SeedKeysAsync(schema);

            (await ExecuteAsync(schema, rename)).Should().Be(1, "guard: the edit of {0} reaches exactly one row", row);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census, "renaming {0} creates nothing at the next boot", row);
            boot.Writes.Should().BeEmpty("the renamed row is found by its key, and nothing about it differs from the catalogue");
            boot.Warnings.Should().BeEmpty();
            (await SeedKeysAsync(schema)).Should().BeEquivalentTo(keys, "no command and no boot writes a seed key");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- a row that cannot be found by its key ---------------------------------------------------------------------------

    /// <summary>
    /// A row without its key, as a row an administrator renamed before the T221 migration stamped the keys would be.
    /// </summary>
    public static TheoryData<string, string, string> KeylessRows => new()
    {
        // The College loses its key and its short code, keeping its name: the pre-T221 crash, now with nothing to find it by.
        { "College", """UPDATE "Colleges" SET "SeedKey" = NULL, "ShortCode" = 'CPSA-OLD' WHERE "SeedKey" = 'cpsa'""", CollegeKey },
        { "speciality", """UPDATE "Specialities" SET "SeedKey" = NULL WHERE "SeedKey" = 'cpsa:paediatrics'""", SpecialityKey },
        { "sub-speciality", """UPDATE "SubSpecialities" SET "SeedKey" = NULL WHERE "SeedKey" = 'cpsa:paediatrics:paediatrics'""", SubSpecialityKey },
        { "entrustment scale", """UPDATE "EntrustmentScales" SET "SeedKey" = NULL WHERE "SeedKey" = 'cpsa:scale:v11.1'""", ScaleKey },
        { "EPA (PAED-007)", """UPDATE "Epas" SET "SeedKey" = NULL WHERE "SeedKey" = 'cpsa:paediatrics:epa:PAED-007'""", "cpsa:paediatrics:epa:PAED-007" },
        { "curriculum", """UPDATE "Curricula" SET "SeedKey" = NULL WHERE "SeedKey" = 'cpsa:paediatrics:curriculum:v11.1'""", CurriculumKey },
    };

    [Theory]
    [MemberData(nameof(KeylessRows))]
    public async Task AKeylessRow_OnADatabaseThatHoldsTheCatalogue_IsAnnouncedOnce_AndNotCreatedAgain(string row, string clearKey, string seedKey)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            (await ExecuteAsync(schema, clearKey)).Should().Be(1, "guard: exactly one row loses its key");
            var census = await CensusAsync(schema);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census, "a missing {0} is never created again once the catalogue exists", row);
            boot.Writes.Should().BeEmpty();

            var warning = boot.Warnings.Should().ContainSingle("the missing row is announced once, and everything that needs it is skipped quietly").Which;
            warning.Values["CatalogueRow"].Should().Be(row);
            warning.Values["SeedKey"].Should().Be(seedKey);
            warning.Message.Should().Contain(seedKey).And.Contain("Not created");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Without the ladder there is nothing to pin a recreated item to, so an item an administrator removed stays removed,
    /// and the checks that do not need the ladder still run.
    /// </summary>
    [Fact]
    public async Task WithTheLadderMissingByItsKey_ARemovedItemIsNotRecreated_AndTheOtherChecksStillRun()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);

            await ExecuteAsync(schema, """UPDATE "EntrustmentScales" SET "SeedKey" = NULL WHERE "Name" = $1""", ScaleName);
            (await ExecuteAsync(schema, $"""DELETE FROM "CurriculumItems" WHERE "EpaId" = ({EpaIdSql("PAED-009")})""")).Should().Be(1);
            (await ExecuteAsync(schema, $"""UPDATE "CurriculumItems" SET "RequiredCount" = 99 WHERE "EpaId" = ({EpaIdSql("PAED-001")})""")).Should().Be(1);
            var census = await CensusAsync(schema);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census, "no item is created without the ladder its minima are written on");
            boot.Warnings.Should().HaveCount(2);
            boot.Warnings.Should().ContainSingle(entry => Equals(entry.Values.GetValueOrDefault("SeedKey"), ScaleKey));
            boot.Warnings.Should().ContainSingle(entry => entry.Values.ContainsKey("ExpectedCount"))
                .Which.Values["EpaCode"].Should().Be("PAED-001", "the target check does not need the ladder, so it still runs");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A CollegeAdmin moves a catalogue EPA to another sub-speciality and removes its item. The EPA is still the
    /// catalogue's, found by its key, so no second PAED-003 is created; and no item is created for an EPA of another
    /// sub-speciality (T195).
    /// </summary>
    [Fact]
    public async Task AnEpaMovedToAnotherSubSpeciality_IsNotCreatedAgain_AndGetsNoNewItemInTheOldCurriculum()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);

            var neonatologyId = await ScalarAsync<int>(
                schema,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") SELECT "Id", 'Neonatology', TRUE FROM "Specialities" WHERE "Name" = 'Paediatrics' RETURNING "Id" """);
            var paed003 = await ScalarAsync<int>(schema, EpaIdSql("PAED-003"));
            (await ExecuteAsync(schema, """DELETE FROM "CurriculumItems" WHERE "EpaId" = $1""", paed003)).Should().Be(1);
            (await ExecuteAsync(schema, """UPDATE "Epas" SET "SubSpecialityId" = $1 WHERE "Id" = $2""", neonatologyId, paed003)).Should().Be(1);
            var census = await CensusAsync(schema);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census);
            var warning = boot.Warnings.Should().ContainSingle().Which;
            warning.Values["EpaCode"].Should().Be("PAED-003");
            warning.Values["EpaSubSpecialityId"].Should().Be(neonatologyId);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- a fresh database that already holds one of the catalogue's names -----------------------------------------------

    /// <summary>A row an administrator made, before the catalogue was ever seeded, that holds a name the catalogue needs.</summary>
    public static TheoryData<string, string> Collisions => new()
    {
        {
            "a College of the catalogue's name (before T221, startup stopped on the unique College name)",
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College of Paediatricians of South Africa', 'CPSA-X', now(), TRUE)"""
        },
        {
            "a College of the catalogue's short code (before T221, adopted by it)",
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Paediatric College', 'CPSA', now(), TRUE)"""
        },
        {
            "a scale of the v11.1 ladder's name (before T221, adopted by it)",
            """INSERT INTO "EntrustmentScales" ("Name") VALUES ('CPSA Paediatric Entrustment Scale v11.1')"""
        },
    };

    [Theory]
    [MemberData(nameof(Collisions))]
    public async Task AFreshDatabase_WhereARowItDidNotMakeHoldsACatalogueName_SeedsNothing_AndStartsUp(string row, string insert)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            (await ExecuteAsync(schema, insert)).Should().Be(1, "guard: {0} is there before the first boot", row);

            var boot = await BootAsync(schema);

            (await SeedKeysAsync(schema)).Should().BeEmpty("nothing of the catalogue is created beside a row it did not make, and that row is not adopted");
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "ActivityTypes" WHERE "Key" LIKE '%\_cpsa'""")).Should().Be(0, "a CPSA type has no speciality to be scoped to");
            boot.Warnings.Should().ContainSingle().Which.Values.Should().ContainKey("Collisions");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the T221 migration on a database that already holds the catalogue -----------------------------------------------

    /// <summary>
    /// A database as T221 finds dev: the catalogue as the pre-T221 seeder made it, beside a decoy for each clause of the
    /// stamp. The migration keys exactly the catalogue's rows, and the next boot finds them all and creates nothing.
    /// </summary>
    [Fact]
    public async Task Migration_OnAPopulatedDatabase_KeysExactlyTheCataloguesRows_AndTheNextBootCreatesNothing()
    {
        try
        {
            var fixture = await ArrangePreT221DatabaseAsync(subSpecialityName: "Paediatrics");

            await MigrateToLatestAsync(fixture.Schema);

            var keys = await SeedKeysAsync(fixture.Schema);
            keys.Should().BeEquivalentTo(fixture.CatalogueRows, "the stamp keys the catalogue's rows and no decoy");

            var census = await CatalogueCensusAsync(fixture.Schema);
            var boot = await BootAsync(fixture.Schema);

            (await CatalogueCensusAsync(fixture.Schema)).Should().Be(census, "every catalogue row is found by the key the migration gave it");
            boot.Warnings.Where(IsSeedKeyWarning).Should().BeEmpty();
            (await SeedKeysAsync(fixture.Schema)).Should().BeEquivalentTo(keys);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The case the stamp cannot help: the sub-speciality was renamed before T221, so the stamp finds the College and the
    /// speciality but not the sub-speciality, nor the EPAs and curriculum it finds through it. The next boot announces
    /// each and creates no second catalogue, which is what the pre-T221 seeder did at this point.
    /// </summary>
    [Fact]
    public async Task Migration_AfterASubSpecialityRenamedBeforeIt_KeysWhatItCanFind_AndTheNextBootCreatesNoSecondCatalogue()
    {
        try
        {
            var fixture = await ArrangePreT221DatabaseAsync(subSpecialityName: "General Paediatrics");

            await MigrateToLatestAsync(fixture.Schema);

            (await SeedKeysAsync(fixture.Schema)).Should().BeEquivalentTo(
                fixture.CatalogueRows,
                "the stamp guesses nothing: what it finds through the renamed sub-speciality stays without a key");

            var census = await CatalogueCensusAsync(fixture.Schema);
            var boot = await BootAsync(fixture.Schema);

            (await CatalogueCensusAsync(fixture.Schema)).Should().Be(census, "no second sub-speciality, EPA or curriculum");
            boot.Warnings.Where(IsSeedKeyWarning).Select(entry => (string)entry.Values["SeedKey"]!)
                .Should().BeEquivalentTo(new[] { SubSpecialityKey, CurriculumKey }.Concat(EpaCodes.Select(EpaKey)));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>Replayed, on a restored dump for instance, the stamp changes nothing already keyed.</summary>
    [Fact]
    public async Task TheStamp_ReplayedOnASeededDatabase_ChangesNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            (await ExecuteAsync(schema, """UPDATE "SubSpecialities" SET "Name" = 'General Paediatrics' WHERE "SeedKey" = 'cpsa:paediatrics:paediatrics'""")).Should().Be(1);
            var keys = await SeedKeysAsync(schema);

            var stamp = new T221_CatalogueSeedKeys().UpOperations.OfType<SqlOperation>().Should().ContainSingle().Which.Sql;
            (await ExecuteAsync(schema, stamp)).Should().Be(0);

            (await SeedKeysAsync(schema)).Should().BeEquivalentTo(keys);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Down_RemovesTheSeedKeyColumns()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(MigrationBeforeT221(db));
            }

            (await ScalarAsync<long>(
                    schema,
                    """SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = current_schema() AND column_name = 'SeedKey'"""))
                .Should().Be(0);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the populated database, before T221 -----------------------------------------------------------------------------

    private sealed record PreT221Fixture(string Schema, IReadOnlyDictionary<(string Table, string Key), int> CatalogueRows);

    /// <param name="subSpecialityName">
    /// "Paediatrics" as the seeder made it, or another name for a sub-speciality an administrator renamed before T221.
    /// </param>
    private async Task<PreT221Fixture> ArrangePreT221DatabaseAsync(string subSpecialityName)
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(MigrationBeforeT221(db));
            (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(T221Migration, "guard: the schema stops just before T221");
        }

        var rows = new Dictionary<(string Table, string Key), int>();

        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);

        var hospitalId = await InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Legacy Hospital', 'LEGACY', now(), TRUE) RETURNING "Id" """);

        // The catalogue, as the pre-T221 seeder made it.
        var cpsaId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ($1, 'CPSA', now(), TRUE) RETURNING "Id" """, CollegeName);
        var paediatricsId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, cpsaId);
        var ladderId = await InsertAsync(connection,
            """INSERT INTO "EntrustmentScales" ("Name") VALUES ($1) RETURNING "Id" """, ScaleName);
        string[] rungs = ["1", "2", "3a", "3b", "4", "5"];
        for (var order = 1; order <= rungs.Length; order++)
        {
            await InsertAsync(connection,
                """INSERT INTO "EntrustmentLevels" ("ScaleId", "Order", "Label") VALUES ($1, $2, $3) RETURNING "Id" """, ladderId, order, rungs[order - 1]);
        }

        var subSpecialityId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive", "DefaultEntrustmentScaleId") VALUES ($1, $2, TRUE, $3) RETURNING "Id" """,
            paediatricsId, subSpecialityName, ladderId);
        var epaIds = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var code in EpaCodes)
        {
            epaIds[code] = await InsertEpaAsync(connection, subSpecialityId, code, owningInstitutionId: null);
        }

        var curriculumId = await InsertCurriculumAsync(connection, subSpecialityId, CurriculumName, "11.1");
        foreach (var code in EpaCodes)
        {
            await InsertAsync(connection,
                """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "RequiredCount", "QuotaPeriod", "MinimumLevelOrder", "WindowMonths", "ScaleId") VALUES ($1, $2, 1, 1, 3, 12, $3) RETURNING "Id" """,
                curriculumId, epaIds[code], ladderId);
        }

        rows[("Colleges", CollegeKey)] = cpsaId;
        rows[("Specialities", SpecialityKey)] = paediatricsId;
        rows[("EntrustmentScales", ScaleKey)] = ladderId;
        rows[("SubSpecialities", SubSpecialityKey)] = subSpecialityId;
        rows[("Curricula", CurriculumKey)] = curriculumId;
        foreach (var code in EpaCodes)
        {
            rows[("Epas", EpaKey(code))] = epaIds[code];
        }

        // One decoy per clause of the stamp. None may be keyed.
        var neonatologyId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Neonatology', TRUE) RETURNING "Id" """, paediatricsId);
        var otherCollegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College of Physicians of South Africa', 'CPSA-PHYS', now(), TRUE) RETURNING "Id" """);
        var otherPaediatricsId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, otherCollegeId);
        var otherPaediatricsSubId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, otherPaediatricsId);
        await InsertAsync(connection, """INSERT INTO "EntrustmentScales" ("Name") VALUES ('CPSA Paediatric Entrustment Scale v11.0') RETURNING "Id" """);

        await InsertEpaAsync(connection, subSpecialityId, "PAED-016", owningInstitutionId: null);
        await InsertEpaAsync(connection, subSpecialityId, "PAED-001", owningInstitutionId: hospitalId);
        await InsertEpaAsync(connection, neonatologyId, "PAED-001", owningInstitutionId: null);
        await InsertEpaAsync(connection, otherPaediatricsSubId, "PAED-001", owningInstitutionId: null);

        await InsertCurriculumAsync(connection, subSpecialityId, CurriculumName, "11.2");
        await InsertCurriculumAsync(connection, subSpecialityId, "Paediatric Research Track", "11.1");
        await InsertCurriculumAsync(connection, neonatologyId, CurriculumName, "11.1");
        await InsertCurriculumAsync(connection, otherPaediatricsSubId, CurriculumName, "11.1");

        var keyed = subSpecialityName == "Paediatrics"
            ? rows
            : rows.Where(pair => pair.Key.Table is "Colleges" or "Specialities" or "EntrustmentScales").ToDictionary(pair => pair.Key, pair => pair.Value);
        return new PreT221Fixture(schema, keyed);
    }

    private static Task<int> InsertEpaAsync(NpgsqlConnection connection, int subSpecialityId, string code, int? owningInstitutionId)
        => InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, $3, $2, $2, 0, now(), TRUE) RETURNING "Id" """,
            subSpecialityId, code, (object?)owningInstitutionId ?? DBNull.Value);

    private static Task<int> InsertCurriculumAsync(NpgsqlConnection connection, int subSpecialityId, string name, string version)
        => InsertAsync(connection,
            """INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive") VALUES ($1, $2, $3, DATE '2026-01-01', TRUE) RETURNING "Id" """,
            subSpecialityId, name, version);

    // ---- expectations, census and boots ------------------------------------------------------------------------------------

    /// <summary>The tables with a seed key: T221's six, and institutions since T229, for DataSeeder's Demo Institution.</summary>
    private static string[] ExpectedIndexedTables() => ["Colleges", "Curricula", "EntrustmentScales", "Epas", "Institutions", "Specialities", "SubSpecialities"];

    private static IEnumerable<(string Table, string Key)> ExpectedKeys()
        => new[]
            {
                ("Colleges", CollegeKey),
                ("Specialities", SpecialityKey),
                ("SubSpecialities", SubSpecialityKey),
                ("EntrustmentScales", ScaleKey),
                ("Curricula", CurriculumKey)
            }
            .Concat(EpaCodes.Select(code => ("Epas", EpaKey(code))));

    /// <summary>The Id of the national EPA with this code, as SQL, for a statement to embed.</summary>
    private static string EpaIdSql(string code)
        => $"""SELECT "Id" FROM "Epas" WHERE "Code" = '{code}' AND "OwningInstitutionId" IS NULL""";

    private static bool IsSeedKeyWarning(CapturedLogEntry entry) => entry.Values.ContainsKey("SeedKey") || entry.Values.ContainsKey("Collisions");

    /// <summary>
    /// Every row that carries a seed key, as (table, key) to id, but for <see cref="DataSeeder" />'s demo rows. Those carry
    /// keys of their own since T229, each <c>demo</c> or starting <c>demo:</c>, and are <c>DemoSeedKeyPostgresTests</c>'
    /// concern: the seeder creates them at the first boot after a migration.
    /// </summary>
    private async Task<Dictionary<(string Table, string Key), int>> SeedKeysAsync(string schema)
    {
        var rows = await QueryAsync(
            schema,
            string.Join(
                " UNION ALL ",
                ExpectedIndexedTables().Select(table =>
                    $"""SELECT '{table}', "SeedKey", "Id" FROM "{table}" WHERE "SeedKey" IS NOT NULL AND "SeedKey" <> 'demo' AND "SeedKey" NOT LIKE 'demo:%'""")),
            reader => (Table: reader.GetString(0), Key: reader.GetString(1), Id: reader.GetInt32(2)));

        return rows.ToDictionary(row => (row.Table, row.Key), row => row.Id);
    }

    private sealed record Census(long Colleges, long Specialities, long SubSpecialities, long Scales, long Levels, long Epas, long Curricula, long Items, long ActivityTypes);

    private async Task<Census> CensusAsync(string schema)
        => new(
            await CountAsync(schema, "Colleges"),
            await CountAsync(schema, "Specialities"),
            await CountAsync(schema, "SubSpecialities"),
            await CountAsync(schema, "EntrustmentScales"),
            await CountAsync(schema, "EntrustmentLevels"),
            await CountAsync(schema, "Epas"),
            await CountAsync(schema, "Curricula"),
            await CountAsync(schema, "CurriculumItems"),
            await CountAsync(schema, "ActivityTypes"));

    /// <summary>
    /// The rows only this seeder would create, counted by the names it gives them. <see cref="DataSeeder" /> creates its
    /// demo tree on the first boot after a migration, so a whole-table census would move for reasons that are not T221's.
    /// Activity types are left out (zero): the seeder creates any CPSA type a database lacks, by design, and the
    /// fixture holds none.
    /// </summary>
    private async Task<Census> CatalogueCensusAsync(string schema)
        => new(
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Colleges" WHERE "Name" = $1 OR "ShortCode" = 'CPSA'""", CollegeName),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Specialities" WHERE "Name" = 'Paediatrics'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "SubSpecialities" WHERE "Name" LIKE '%Paediatrics'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "EntrustmentScales" WHERE "Name" LIKE 'CPSA%'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "EntrustmentLevels" AS l JOIN "EntrustmentScales" AS s ON s."Id" = l."ScaleId" WHERE s."Name" LIKE 'CPSA%'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Epas" WHERE "Code" LIKE 'PAED-%'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Curricula" WHERE "Name" = $1""", CurriculumName),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "CurriculumItems" AS ci JOIN "Curricula" AS c ON c."Id" = ci."CurriculumId" WHERE c."Name" = $1""", CurriculumName),
            ActivityTypes: 0);

    private Task<long> CountAsync(string schema, string table) => ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "{table}" """);

    private sealed record BootLog(IReadOnlyList<string> Writes, IReadOnlyList<CapturedLogEntry> Warnings);

    /// <summary>
    /// One startup's seeding, the way <c>Program.cs</c> runs it: <see cref="DataSeeder" />, then the catalogue, each in its
    /// own context. Every write statement and the catalogue seeder's warnings are captured.
    /// </summary>
    private async Task<BootLog> BootAsync(string schema)
    {
        var writes = new WriteCounter();
        var log = new CapturingLogger<PaediatricCatalogueSeeder>();

        await using (var db = NewContext(schema, writes))
        {
            await new DataSeeder(db).SeedAsync();
        }

        await using (var db = NewContext(schema, writes))
        {
            await new PaediatricCatalogueSeeder(db, log).SeedAsync();
        }

        return new BootLog(writes.Writes, log.Warnings.ToList());
    }

    /// <summary>
    /// The migration just before T221, read from the assembly rather than typed. A branch that merges a migration
    /// timestamped before T221 (T145 and T219 were two) moves it, and a typed id would then stop the schema short of
    /// that migration, leaving it, not T221, first in line.
    /// </summary>
    private static string MigrationBeforeT221(ApplicationDbContext db)
    {
        var migrations = db.Database.GetMigrations().Order(StringComparer.Ordinal).ToList();
        var index = migrations.IndexOf(T221Migration);
        index.Should().BePositive("guard: T221 is in the assembly, and is not its first migration");
        return migrations[index - 1];
    }

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();
        await MigrateToLatestAsync(schema);
        return schema;
    }

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T221Migration);
    }

    // ---- plumbing ----------------------------------------------------------------------------------------------------------

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, sql, values);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, sql, values);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    private async Task<List<T>> QueryAsync<T>(string schema, string sql, Func<NpgsqlDataReader, T> map, params object[] values)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = Command(connection, sql, values);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<T>();
        while (await reader.ReadAsync())
        {
            results.Add(map(reader));
        }

        return results;
    }

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return command;
    }

    /// <summary>
    /// Records every INSERT, UPDATE, DELETE or MERGE statement EF sends, so "a boot writes nothing" is asserted on what
    /// reached the server. A copy of <c>WbaToolAllowListPostgresTests</c>' counter; the fresh-database test asserts that
    /// the first boot IS seen, so an empty list is never vacuous.
    /// </summary>
    private sealed class WriteCounter : DbCommandInterceptor
    {
        private static readonly Regex WriteStatement = new(
            @"^\s*(INSERT|UPDATE|DELETE|MERGE)\b",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        public List<string> Writes { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Inspect(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Inspect(command);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            Inspect(command);
            return result;
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        private void Inspect(DbCommand command)
        {
            if (WriteStatement.IsMatch(command.CommandText))
            {
                Writes.Add(command.CommandText);
            }
        }
    }

    private static IEnumerable<Exception> Chain(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            yield return current;
        }
    }

    private sealed class SimulatedCrashException(string message) : Exception(message);

    /// <summary>
    /// Throws, before it reaches the server, the first command that inserts into <paramref name="table" />, and records
    /// which tables the commands before it inserted into.
    /// </summary>
    private sealed class CrashOnInsertInto(string table) : DbCommandInterceptor
    {
        private static readonly Regex InsertInto = new(
            @"^\s*INSERT INTO ""(?<table>[^""]+)""",
            RegexOptions.Multiline | RegexOptions.CultureInvariant);

        public HashSet<string> SentBeforeTheCrash { get; } = new(StringComparer.Ordinal);

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Inspect(command);
            return ValueTask.FromResult(result);
        }

        private void Inspect(DbCommand command)
        {
            var tables = InsertInto.Matches(command.CommandText).Select(match => match.Groups["table"].Value).ToList();
            if (tables.Contains(table, StringComparer.Ordinal))
            {
                throw new SimulatedCrashException($"Simulated crash on the insert into \"{table}\".");
            }

            SentBeforeTheCrash.UnionWith(tables);
        }
    }

    /// <summary>Keeps every log entry with its structured values. A copy of Wombat.Infrastructure.Tests' internal one.</summary>
    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<CapturedLogEntry> Entries { get; } = [];

        public IEnumerable<CapturedLogEntry> Warnings => Entries.Where(entry => entry.Level == LogLevel.Warning);

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var values = state is IReadOnlyList<KeyValuePair<string, object?>> pairs
                ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                : new Dictionary<string, object?>(StringComparer.Ordinal);

            Entries.Add(new CapturedLogEntry(logLevel, formatter(state, exception), values));
        }
    }

    private sealed record CapturedLogEntry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Values);
}

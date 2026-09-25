extern alias WombatWeb;

using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Persistence.Migrations;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T229 on a real PostgreSQL server: <see cref="DataSeeder" /> finds the Demo Institution, the Demo College, its speciality
/// and sub-speciality, the O-R Scale, the demo EPA and the IM Core curriculum by a seed key an administrator cannot edit, and
/// creates the demo data only on a database that holds none of it.
/// </summary>
/// <remarks>
/// <para>
/// Before T229 the College and the institution were found by their short codes, both editable. A changed <c>DEMO-C</c> stopped
/// startup on a <c>SingleAsync</c>; a changed <c>DEMO</c> made the next boot insert a second Demo College and Demo Institution,
/// which the unique name indexes refused, so startup stopped there. The scale, EPA and curriculum were found by name or code
/// and created again when the lookup missed. Only Postgres enforces those indexes, so these run here; the last test starts
/// the web app itself, twice, as a server restart does.
/// </para>
/// <para>
/// The keys are typed from the seeder's contract, not read from it, so a key the seeder and the migration both changed would
/// still fail here. Each edit locates its row by what it was called, as an administrator's edit page does, so the same test
/// fails against the pre-T229 seeder for the reason the defect names. The plumbing follows
/// <see cref="CatalogueSeedKeyPostgresTests" />: one schema per test, registered before it is created, dropped in a
/// <c>finally</c> and again from <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class DemoSeedKeyPostgresTests : IAsyncLifetime
{
    private const string T229Migration = "20260925070536_T229_DemoSeedKeys";

    private const string InstitutionKey = "demo";
    private const string CollegeKey = "demo";
    private const string SpecialityKey = "demo:general-medicine";
    private const string SubSpecialityKey = "demo:general-medicine:general-internal-medicine";
    private const string ScaleKey = "demo:scale:o-r";
    private const string EpaKey = "demo:general-medicine:epa:EPA-001";
    private const string CurriculumKey = "demo:general-medicine:curriculum:v2026.1";

    private static readonly string[] KeyedTables =
        ["Colleges", "Curricula", "EntrustmentScales", "Epas", "Institutions", "Specialities", "SubSpecialities"];

    private static readonly string[] OrScaleRungs =
        ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"];

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    // ---- a fresh database ------------------------------------------------------------------------------------------------

    /// <summary>
    /// The create path: one boot seeds the demo data once, each row carrying its key and hanging off the right parent, and a
    /// second boot writes nothing and announces nothing.
    /// </summary>
    [Fact]
    public async Task AFreshDatabase_SeedsTheDemoDataOnce_EveryRowCarryingItsSeedKey_AndASecondBootWritesNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            var firstBoot = await BootAsync(schema);
            firstBoot.Writes.Should().NotBeEmpty("guard: the write counter sees the first boot, so an empty second boot means something");
            firstBoot.Warnings.Should().BeEmpty();
            firstBoot.CatalogueWarnings.Should().BeEmpty();

            var keys = await DemoKeysAsync(schema);
            keys.Keys.Should().BeEquivalentTo(ExpectedKeys(), "every demo row carries its key");

            var institutionId = keys[("Institutions", InstitutionKey)];
            var collegeId = keys[("Colleges", CollegeKey)];
            var specialityId = keys[("Specialities", SpecialityKey)];
            var subSpecialityId = keys[("SubSpecialities", SubSpecialityKey)];
            var scaleId = keys[("EntrustmentScales", ScaleKey)];
            var epaId = keys[("Epas", EpaKey)];
            var curriculumId = keys[("Curricula", CurriculumKey)];

            (await QueryAsync(schema, """SELECT "Name", "ShortCode" FROM "Institutions" WHERE "Id" = $1""", reader => (reader.GetString(0), reader.GetString(1)), institutionId))
                .Should().Equal(("Demo Institution", "DEMO"));
            (await QueryAsync(schema, """SELECT "Name", "ShortCode" FROM "Colleges" WHERE "Id" = $1""", reader => (reader.GetString(0), reader.GetString(1)), collegeId))
                .Should().Equal(("Demo College", "DEMO-C"));
            (await ScalarAsync<int>(schema, """SELECT "CollegeId" FROM "Specialities" WHERE "Id" = $1""", specialityId)).Should().Be(collegeId);
            (await ScalarAsync<int>(schema, """SELECT "SpecialityId" FROM "SubSpecialities" WHERE "Id" = $1""", subSpecialityId)).Should().Be(specialityId);
            (await QueryAsync(schema, """SELECT "Label" FROM "EntrustmentLevels" WHERE "ScaleId" = $1 ORDER BY "Order" """, reader => reader.GetString(0), scaleId))
                .Should().Equal(OrScaleRungs);
            (await QueryAsync(
                    schema,
                    """SELECT "SubSpecialityId", "Code", "OwningInstitutionId" IS NULL FROM "Epas" WHERE "Id" = $1""",
                    reader => (reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2)),
                    epaId))
                .Should().Equal((subSpecialityId, "EPA-001", true));
            (await QueryAsync(
                    schema,
                    """SELECT "SubSpecialityId", "Name", "Version" FROM "Curricula" WHERE "Id" = $1""",
                    reader => (reader.GetInt32(0), reader.GetString(1), reader.GetString(2)),
                    curriculumId))
                .Should().Equal((subSpecialityId, "IM Core Curriculum", "2026.1"));
            (await QueryAsync(
                    schema,
                    """SELECT "EpaId", "ScaleId", "RequiredCount", "MinimumLevelOrder" FROM "CurriculumItems" WHERE "CurriculumId" = $1""",
                    reader => (reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3)),
                    curriculumId))
                .Should().Equal(new[] { (epaId, scaleId, 5, 4) }, "one item, five at Independent, pinned to the O-R Scale (T174)");
            (await GenericTypeScopesAsync(schema)).Should().Equal(new[] { specialityId }, "every generic type is scoped to the demo speciality");

            var census = await CensusAsync(schema);

            var secondBoot = await BootAsync(schema);
            secondBoot.Writes.Should().BeEmpty("a second boot finds every row by its key");
            secondBoot.Warnings.Should().BeEmpty();
            (await CensusAsync(schema)).Should().Be(census);
            (await DemoKeysAsync(schema)).Should().BeEquivalentTo(keys);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>One key names one institution, and any number of institutions an administrator made carry none.</summary>
    [Fact]
    public async Task TheInstitutionSeedKey_IsUniqueWhereItIsSet_AndNullForAnInstitutionAnAdministratorMakes()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);

            var secondDemo = () => ExecuteAsync(
                schema,
                """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive", "SeedKey") VALUES ('Another', 'ANOTHER', now(), TRUE, 'demo')""");
            (await secondDemo.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);

            (await ExecuteAsync(
                    schema,
                    """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Groote Schuur Hospital', 'GSH', now(), TRUE), ('Red Cross War Memorial Children''s Hospital', 'RXH', now(), TRUE)"""))
                .Should().Be(2, "institutions an administrator makes carry no key, and nulls never collide");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The demo data is created in one save, so a first boot that stops half way leaves none of it behind, and the next boot
    /// seeds it whole. Were it several saves, the rows before the failure would stay; every later boot would find them,
    /// announce the rest as missing and never create it.
    /// </summary>
    [Fact]
    public async Task AFirstBootThatFailsHalfWayThroughTheDemoData_LeavesNoneOfIt_AndTheNextBootSeedsItWhole()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            var crash = new CrashOnInsertInto("Curricula");
            await using (var db = NewContext(schema, crash))
            {
                var firstBoot = () => new DataSeeder(db).SeedAsync();
                (await firstBoot.Should().ThrowAsync<Exception>())
                    .Where(thrown => Chain(thrown).OfType<SimulatedCrashException>().Any(), "the boot stopped where the test stopped it");
            }

            // What the curriculum's foreign keys need goes in earlier batches of the same save, so it reached the server.
            crash.SentBeforeTheCrash.Should().Contain(
                new[] { "Colleges", "Institutions", "EntrustmentScales", "Specialities", "SubSpecialities" },
                "guard: rows ahead of the curriculum reached the server before the crash, so their absence below is a rollback");

            (await DemoKeysAsync(schema)).Should().BeEmpty("the save that failed took every demo row with it");
            (await DemoCensusAsync(schema)).Should().Be(new DemoCensus(0, 0, 0, 0, 0, 0, 0, 0, 0));

            var nextBoot = await BootAsync(schema);

            nextBoot.Warnings.Should().BeEmpty("nothing of the demo data is left to be found half made");
            (await DemoKeysAsync(schema)).Keys.Should().BeEquivalentTo(ExpectedKeys());
            (await DemoCensusAsync(schema)).Should().Be(new DemoCensus(1, 1, 1, 1, 1, 5, 1, 1, 1));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- edits between two boots -----------------------------------------------------------------------------------------

    /// <summary>
    /// An administrator's edit of each demo row, located as their edit page would find it: by what it was called.
    /// </summary>
    public static TheoryData<string, string> Edits => new()
    {
        {
            "the institution's short code, its name kept (before T229, a second Demo College and Demo Institution, refused by the unique names)",
            """UPDATE "Institutions" SET "ShortCode" = 'DEMO-2027' WHERE "ShortCode" = 'DEMO'"""
        },
        {
            "the institution's name and short code",
            """UPDATE "Institutions" SET "Name" = 'Groote Schuur Hospital', "ShortCode" = 'GSH' WHERE "ShortCode" = 'DEMO'"""
        },
        {
            "the College's short code, its name kept (before T229, a SingleAsync on DEMO-C stopped startup)",
            """UPDATE "Colleges" SET "ShortCode" = 'DEMO-C2' WHERE "ShortCode" = 'DEMO-C'"""
        },
        {
            "the College's name and short code",
            """UPDATE "Colleges" SET "Name" = 'College of Physicians of South Africa', "ShortCode" = 'CP-SA' WHERE "ShortCode" = 'DEMO-C'"""
        },
        {
            "the speciality",
            """UPDATE "Specialities" SET "Name" = 'Internal Medicine' WHERE "Name" = 'General Medicine'"""
        },
        {
            "the sub-speciality",
            """UPDATE "SubSpecialities" SET "Name" = 'Adult Internal Medicine' WHERE "Name" = 'General Internal Medicine'"""
        },
        {
            "the O-R Scale (before T229, a second O-R Scale)",
            """UPDATE "EntrustmentScales" SET "Name" = 'Observation to entrustment' WHERE "Name" = 'O-R Scale'"""
        },
        {
            "the demo EPA's code (before T229, a second EPA-001 with an item of its own)",
            """UPDATE "Epas" SET "Code" = 'IM-001' WHERE "Code" = 'EPA-001' AND "OwningInstitutionId" IS NULL"""
        },
        {
            "the curriculum's name and version (before T229, a second IM Core Curriculum)",
            """UPDATE "Curricula" SET "Name" = 'Internal Medicine Core', "Version" = '2027.1' WHERE "Name" = 'IM Core Curriculum' AND "Version" = '2026.1'"""
        },
    };

    [Theory]
    [MemberData(nameof(Edits))]
    public async Task AnEditedDemoRow_IsFoundByItsSeedKey_AndTheNextBootCreatesNothing(string row, string edit)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            var census = await CensusAsync(schema);
            var keys = await DemoKeysAsync(schema);

            (await ExecuteAsync(schema, edit)).Should().Be(1, "guard: the edit of {0} reaches exactly one row", row);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census, "editing {0} creates nothing at the next boot", row);
            boot.Writes.Should().BeEmpty("the edited row is found by its key, and nothing about it differs from what the seeder checks");
            boot.Warnings.Should().BeEmpty();
            boot.CatalogueWarnings.Should().BeEmpty();
            (await DemoKeysAsync(schema)).Should().BeEquivalentTo(keys, "no command and no boot writes a seed key");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- a row that cannot be found by its key ---------------------------------------------------------------------------

    /// <summary>
    /// A demo row without its key, as a row an administrator edited before the T229 migration stamped the keys would be.
    /// </summary>
    public static TheoryData<string, string, string> KeylessRows => new()
    {
        // The institution and the College each lose their key and their short code, keeping their name: the pre-T229 crash,
        // now with nothing to find the row by.
        { "institution", """UPDATE "Institutions" SET "SeedKey" = NULL, "ShortCode" = 'DEMO-OLD' WHERE "SeedKey" = 'demo'""", InstitutionKey },
        { "College", """UPDATE "Colleges" SET "SeedKey" = NULL, "ShortCode" = 'DEMO-C-OLD' WHERE "SeedKey" = 'demo'""", CollegeKey },
        { "speciality", """UPDATE "Specialities" SET "SeedKey" = NULL WHERE "SeedKey" = 'demo:general-medicine'""", SpecialityKey },
        { "sub-speciality", """UPDATE "SubSpecialities" SET "SeedKey" = NULL WHERE "SeedKey" = 'demo:general-medicine:general-internal-medicine'""", SubSpecialityKey },
        { "entrustment scale", """UPDATE "EntrustmentScales" SET "SeedKey" = NULL WHERE "SeedKey" = 'demo:scale:o-r'""", ScaleKey },
        { "EPA (EPA-001)", """UPDATE "Epas" SET "SeedKey" = NULL WHERE "SeedKey" = 'demo:general-medicine:epa:EPA-001'""", EpaKey },
        { "curriculum", """UPDATE "Curricula" SET "SeedKey" = NULL WHERE "SeedKey" = 'demo:general-medicine:curriculum:v2026.1'""", CurriculumKey },
    };

    [Theory]
    [MemberData(nameof(KeylessRows))]
    public async Task AKeylessDemoRow_OnADatabaseThatHoldsTheDemoData_IsAnnouncedOnce_AndNotCreatedAgain(string row, string clearKey, string seedKey)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            (await ExecuteAsync(schema, clearKey)).Should().Be(1, "guard: exactly one row loses its key");
            var census = await CensusAsync(schema);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census, "a missing {0} is never created again once the demo data exists", row);
            boot.Writes.Should().BeEmpty();

            var warning = boot.Warnings.Should().ContainSingle("the missing row is announced once, and everything that needs it is skipped quietly").Which;
            warning.Values["DemoRow"].Should().Be(row);
            warning.Values["SeedKey"].Should().Be(seedKey);
            warning.Message.Should().Contain(seedKey).And.Contain("Not created");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A CollegeAdmin moves the demo EPA to another sub-speciality and removes its item. The EPA is still the demo's, found by
    /// its key, so no second EPA-001 is created; and no item is created for an EPA of another sub-speciality (T195).
    /// </summary>
    [Fact]
    public async Task TheDemoEpaMovedToAnotherSubSpeciality_IsNotCreatedAgain_AndGetsNoNewItemInTheDemoCurriculum()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);

            var cardiologyId = await ScalarAsync<int>(
                schema,
                """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") SELECT "Id", 'Cardiology', TRUE FROM "Specialities" WHERE "SeedKey" = 'demo:general-medicine' RETURNING "Id" """);
            var epaId = await ScalarAsync<int>(schema, """SELECT "Id" FROM "Epas" WHERE "SeedKey" = 'demo:general-medicine:epa:EPA-001'""");
            (await ExecuteAsync(schema, """DELETE FROM "CurriculumItems" WHERE "EpaId" = $1""", epaId)).Should().Be(1);
            (await ExecuteAsync(schema, """UPDATE "Epas" SET "SubSpecialityId" = $1 WHERE "Id" = $2""", cardiologyId, epaId)).Should().Be(1);
            var census = await CensusAsync(schema);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census);
            boot.Writes.Should().BeEmpty();
            var warning = boot.Warnings.Should().ContainSingle().Which;
            warning.Values["EpaCode"].Should().Be("EPA-001");
            warning.Values["EpaSubSpecialityId"].Should().Be(cardiologyId);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// An institution's own EPA-001 in the demo sub-speciality, which the EPA code indexes allow beside the national one.
    /// Before T229 the seeder looked the demo EPA up by sub-speciality and code with <c>SingleOrDefaultAsync</c>, found two,
    /// and stopped startup.
    /// </summary>
    [Fact]
    public async Task AnInstitutionsOwnEpa001_InTheDemoSubSpeciality_NeitherStopsStartupNorIsTouched()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);

            (await ExecuteAsync(
                    schema,
                    """
                    INSERT INTO "Epas" ("SubSpecialityId", "OwningInstitutionId", "Code", "Title", "Category", "CreatedOn", "IsActive")
                    SELECT ss."Id", i."Id", 'EPA-001', 'A local EPA-001', 0, now(), TRUE
                    FROM "SubSpecialities" AS ss, "Institutions" AS i
                    WHERE ss."SeedKey" = 'demo:general-medicine:general-internal-medicine' AND i."SeedKey" = 'demo'
                    """))
                .Should().Be(1, "guard: the local EPA-001 exists beside the national one");
            var census = await CensusAsync(schema);

            var boot = await BootAsync(schema);

            (await CensusAsync(schema)).Should().Be(census);
            boot.Writes.Should().BeEmpty();
            boot.Warnings.Should().BeEmpty();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- a fresh database that already holds one of the demo data's names -----------------------------------------------

    /// <summary>A row an administrator made, before the demo data was ever seeded, that holds a name the demo data needs.</summary>
    public static TheoryData<string, string> Collisions => new()
    {
        {
            "a College of the demo's name (before T229, startup stopped on the unique College name)",
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demo College', 'DC-X', now(), TRUE)"""
        },
        {
            "a College of the demo's short code (before T229, startup stopped on the unique short code)",
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('A college for demonstrations', 'DEMO-C', now(), TRUE)"""
        },
        {
            "an institution of the demo's name (before T229, startup stopped on the unique institution name)",
            """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demo Institution', 'DI-X', now(), TRUE)"""
        },
        {
            "an institution of the demo's short code (before T229, adopted, then a SingleAsync on DEMO-C stopped startup)",
            """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demonstration Hospital', 'DEMO', now(), TRUE)"""
        },
        {
            "a scale of the O-R Scale's name (before T229, adopted by it)",
            """INSERT INTO "EntrustmentScales" ("Name") VALUES ('O-R Scale')"""
        },
    };

    [Theory]
    [MemberData(nameof(Collisions))]
    public async Task AFreshDatabase_WhereARowItDidNotMakeHoldsADemoName_SeedsNoDemoData_AndStartsUp(string row, string insert)
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            (await ExecuteAsync(schema, insert)).Should().Be(1, "guard: {0} is there before the first boot", row);

            var boot = await BootAsync(schema);

            (await DemoKeysAsync(schema)).Should().BeEmpty("nothing of the demo data is created beside a row it did not make, and that row is not adopted");
            (await GenericTypeScopesAsync(schema)).Should().BeEmpty("a generic type has no demo speciality to be scoped to");
            boot.Warnings.Should().ContainSingle().Which.Values.Should().ContainKey("Collisions");
            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Colleges" WHERE "SeedKey" = 'cpsa'"""))
                .Should().Be(1, "startup went on: the catalogue seeder ran after the demo seeder and seeded the catalogue");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the T229 migration on a database that already holds the demo data ----------------------------------------------

    /// <summary>
    /// A database as T229 finds dev: the demo data as the pre-T229 seeder made it, beside a decoy for each clause of the stamp.
    /// The migration keys exactly the demo rows, and the next boot finds them all and creates nothing.
    /// </summary>
    [Fact]
    public async Task Migration_OnAPopulatedDatabase_KeysExactlyTheDemoRows_AndTheNextBootCreatesNothing()
    {
        try
        {
            var fixture = await ArrangePreT229DatabaseAsync(institutionShortCode: "DEMO", collegeShortCode: "DEMO-C");

            await MigrateToLatestAsync(fixture.Schema);

            var keys = await DemoKeysAsync(fixture.Schema);
            keys.Should().BeEquivalentTo(fixture.KeyedRows, "the stamp keys the demo rows and no decoy");

            var census = await DemoCensusAsync(fixture.Schema);
            var boot = await BootAsync(fixture.Schema);

            (await DemoCensusAsync(fixture.Schema)).Should().Be(census, "every demo row is found by the key the migration gave it");
            boot.Warnings.Should().BeEmpty();
            (await GenericTypeScopesAsync(fixture.Schema)).Should().Equal(
                new[] { keys[("Specialities", SpecialityKey)] },
                "the generic types the boot creates are scoped to the demo speciality, not to its decoy under another College");
            (await DemoKeysAsync(fixture.Schema)).Should().BeEquivalentTo(keys);

            var secondBoot = await BootAsync(fixture.Schema);
            secondBoot.Writes.Should().BeEmpty("the second boot after the upgrade is a no-op");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>A short code an administrator changed before T229, so the stamp cannot find the row it names.</summary>
    public static TheoryData<string, string, string, string[]> ShortCodesChangedBeforeTheMigration => new()
    {
        {
            "DEMO (before T229, the next boot inserted a second Demo Institution, refused by the unique name)",
            "DEMO-2027",
            "DEMO-C",
            [InstitutionKey]
        },
        {
            // The speciality, sub-speciality, EPA and curriculum are found through the College, so the stamp guesses none.
            "DEMO-C (before T229, a SingleAsync on DEMO-C stopped startup)",
            "DEMO",
            "DEMO-C2",
            [CollegeKey, SpecialityKey, SubSpecialityKey, EpaKey, CurriculumKey]
        },
    };

    /// <summary>
    /// The case the stamp cannot help: a short code changed before T229. The stamp keys what it can find, and the next boot
    /// starts, announces each row it cannot find, and creates none of them, where the pre-T229 seeder stopped startup.
    /// </summary>
    [Theory]
    [MemberData(nameof(ShortCodesChangedBeforeTheMigration))]
    public async Task Migration_AfterAShortCodeChangedBeforeIt_KeysWhatItCanFind_AndTheNextBootStartsAndCreatesNothing(
        string changed,
        string institutionShortCode,
        string collegeShortCode,
        string[] unkeyed)
    {
        try
        {
            var fixture = await ArrangePreT229DatabaseAsync(institutionShortCode, collegeShortCode);

            await MigrateToLatestAsync(fixture.Schema);

            (await DemoKeysAsync(fixture.Schema)).Should().BeEquivalentTo(
                fixture.KeyedRows,
                "the stamp guesses nothing: with {0} changed, what it would find by it stays without a key",
                changed);

            var census = await DemoCensusAsync(fixture.Schema);
            var boot = await BootAsync(fixture.Schema);

            (await DemoCensusAsync(fixture.Schema)).Should().Be(census, "no second Demo Institution, College, speciality, EPA or curriculum");
            boot.Warnings.Where(entry => entry.Values.ContainsKey("SeedKey")).Select(entry => (string)entry.Values["SeedKey"]!)
                .Should().BeEquivalentTo(unkeyed);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Replayed, on a restored dump for instance, the stamp changes nothing already keyed, and gives no key to a row an
    /// administrator later made in a keyed row's old place, which would collide with the key's unique index.
    /// </summary>
    [Fact]
    public async Task TheStamp_ReplayedAfterEveryDemoRowWasEditedAndItsOldPlaceTaken_ChangesNothing()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            var keys = await DemoKeysAsync(schema);

            // Each keyed row edited out of the lookup that found it before T229, and a new row made that the lookup finds.
            (await ExecuteAsync(schema, """
                UPDATE "Institutions" SET "ShortCode" = 'DEMO-OLD' WHERE "SeedKey" = 'demo';
                INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demonstration Hospital', 'DEMO', now(), TRUE);
                UPDATE "Colleges" SET "ShortCode" = 'DEMO-C-OLD' WHERE "SeedKey" = 'demo';
                INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('A college for demonstrations', 'DEMO-C', now(), TRUE);
                UPDATE "Specialities" SET "CollegeId" = (SELECT "Id" FROM "Colleges" WHERE "ShortCode" = 'DEMO-C')
                    WHERE "SeedKey" = 'demo:general-medicine';
                INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive")
                    SELECT "Id", 'Family Medicine', TRUE FROM "Colleges" WHERE "SeedKey" = 'demo';
                INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive")
                    SELECT "Id", 'Family Practice', TRUE FROM "Specialities" WHERE "Name" = 'Family Medicine';
                UPDATE "EntrustmentScales" SET "Name" = 'Observation to entrustment' WHERE "SeedKey" = 'demo:scale:o-r';
                INSERT INTO "EntrustmentScales" ("Name") VALUES ('O-R Scale');
                UPDATE "Epas" SET "Code" = 'IM-001' WHERE "SeedKey" = 'demo:general-medicine:epa:EPA-001';
                INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "CreatedOn", "IsActive")
                    SELECT "Id", 'EPA-001', 'A new EPA-001', 0, now(), TRUE FROM "SubSpecialities" WHERE "SeedKey" = 'demo:general-medicine:general-internal-medicine';
                UPDATE "Curricula" SET "Name" = 'IM Core Curriculum (old)' WHERE "SeedKey" = 'demo:general-medicine:curriculum:v2026.1';
                INSERT INTO "Curricula" ("SubSpecialityId", "Name", "Version", "EffectiveFrom", "IsActive")
                    SELECT "Id", 'IM Core Curriculum', '2026.1', DATE '2026-01-01', TRUE FROM "SubSpecialities" WHERE "SeedKey" = 'demo:general-medicine:general-internal-medicine';
                """))
                .Should().Be(13, "guard: each of the thirteen statements reached its one row");

            (await ExecuteAsync(schema, DemoSeedKeyStampSql())).Should().Be(0);

            (await DemoKeysAsync(schema)).Should().BeEquivalentTo(keys);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Down_RemovesTheInstitutionSeedKeyColumn_AndLeavesT221s()
    {
        try
        {
            var schema = await MigratedSchemaAsync();

            await using (var db = NewContext(schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(MigrationBeforeT229(db));
            }

            (await QueryAsync(
                    schema,
                    """SELECT table_name FROM information_schema.columns WHERE table_schema = current_schema() AND column_name = 'SeedKey' ORDER BY table_name""",
                    reader => reader.GetString(0)))
                .Should().BeEquivalentTo(KeyedTables.Where(table => table != "Institutions"));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the web app, started twice -------------------------------------------------------------------------------------

    /// <summary>
    /// The task's own case, end to end: the web app starts on a fresh database, both demo short codes are changed, and it
    /// starts again. <c>Program.cs</c> migrates and runs every seeder at each start; the dev users are seeded from the same
    /// services, as a Development start does. Before T229 the second start threw: <c>DEMO</c> no longer found, the seeder
    /// inserted a second "Demo College", which the unique name refused (and <c>DevUserSeeder</c> threw on its own
    /// <c>SingleAsync</c> for <c>DEMO</c>).
    /// </summary>
    [Fact]
    public async Task TheWebApp_StartsAgainAfterBothDemoShortCodesChanged_WithItsDevUsers_AndCreatesNothing()
    {
        try
        {
            var schema = await _schemas.CreateAsync();

            await StartTheWebAppAsync(schema);
            var census = await CensusAsync(schema);
            var people = await PeopleCensusAsync(schema);
            people.Users.Should().BeGreaterThanOrEqualTo(7, "guard: the dev users were seeded at the first start");

            (await ExecuteAsync(schema, """UPDATE "Institutions" SET "ShortCode" = 'DEMO-2027' WHERE "ShortCode" = 'DEMO'""")).Should().Be(1);
            (await ExecuteAsync(schema, """UPDATE "Colleges" SET "ShortCode" = 'DEMO-C2' WHERE "ShortCode" = 'DEMO-C'""")).Should().Be(1);

            var restart = () => StartTheWebAppAsync(schema);
            await restart.Should().NotThrowAsync("the demo rows are found by their seed keys, whatever their short codes");

            (await CensusAsync(schema)).Should().Be(census, "the restart created no demo row");
            (await PeopleCensusAsync(schema)).Should().Be(people, "the restart created no dev user and no scope");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The dev-user seeder finds the paediatric curriculum by the catalogue's seed key, not by its name, so a renamed
    /// curriculum still admits the dev trainee to it and scopes the dev staff to Paediatrics. Before this it was found by
    /// the name "Paediatric EPA Curriculum": a rename put a newly seeded dev trainee on the demo IM curriculum, and gave the
    /// dev staff no paediatric scope, so the CPSA instruments were offered to nobody.
    /// </summary>
    [Fact]
    public async Task TheWebApp_OnADatabaseWhosePaediatricCurriculumWasRenamed_AdmitsTheDevTraineeToIt_AndScopesTheDevStaffToPaediatrics()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            await BootAsync(schema);
            (await ExecuteAsync(
                    schema,
                    """UPDATE "Curricula" SET "Name" = 'Paediatrics EPA Curriculum' WHERE "SeedKey" = 'cpsa:paediatrics:curriculum:v11.1' AND "Name" = 'Paediatric EPA Curriculum'"""))
                .Should().Be(1, "guard: the catalogue's curriculum was seeded under its own name, and is now renamed");
            (await CountAsync(schema, "AspNetUsers")).Should().Be(0, "guard: the dev users are seeded by the web app's start, after the rename");

            await StartTheWebAppAsync(schema);

            var paediatricCurriculumId = await ScalarAsync<int>(schema, """SELECT "Id" FROM "Curricula" WHERE "SeedKey" = 'cpsa:paediatrics:curriculum:v11.1'""");
            var paediatricSubSpecialityId = await ScalarAsync<int>(schema, """SELECT "Id" FROM "SubSpecialities" WHERE "SeedKey" = 'cpsa:paediatrics:paediatrics'""");
            (await ScalarAsync<int>(
                    schema,
                    """SELECT p."CurriculumId" FROM "TraineeProfiles" AS p JOIN "AspNetUsers" AS u ON u."Id" = p."UserId" WHERE u."Email" = 'trainee@wombat.local'"""))
                .Should().Be(paediatricCurriculumId, "the dev trainee is admitted to the paediatric curriculum, whatever it is now called");
            (await ScalarAsync<long>(
                    schema,
                    """SELECT COUNT(*) FROM "UserSubSpecialityScopes" AS s JOIN "AspNetUsers" AS u ON u."Id" = s."UserId" WHERE u."Email" = 'assessor@wombat.local' AND s."SubSpecialityId" = $1""",
                    paediatricSubSpecialityId))
                .Should().Be(1, "the dev assessor can complete the CPSA instruments");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Starts <c>Wombat.Web</c> on the schema (its <c>Program.cs</c> migrates and seeds before it serves anything), runs the
    /// dev-user seeder from its services, and stops it.
    /// </summary>
    private async Task StartTheWebAppAsync(string schema)
    {
        await using var factory = new DemoWebFactory(TestDatabase.SchemaConnectionString(schema));
        using var client = factory.CreateClient();

        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<DevUserSeeder>().SeedAsync();
    }

    private sealed record PeopleCensus(long Users, long SpecialityScopes, long SubSpecialityScopes, long TraineeProfiles);

    private async Task<PeopleCensus> PeopleCensusAsync(string schema)
        => new(
            await CountAsync(schema, "AspNetUsers"),
            await CountAsync(schema, "UserSpecialityScopes"),
            await CountAsync(schema, "UserSubSpecialityScopes"),
            await CountAsync(schema, "TraineeProfiles"));

    /// <summary>
    /// <c>Wombat.Web</c> as the server runs it, but for the database it is pointed at and the scheduler (removed, so no
    /// background job writes beside a census). The environment is neither Development, whose settings are the developer's
    /// own, nor Production, whose settings file is the server's.
    /// </summary>
    private sealed class DemoWebFactory(string connectionString) : WebApplicationFactory<WombatWeb::Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTest");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
            builder.ConfigureServices(services =>
                services.Remove(services.Single(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(ScheduledJobHost))));
        }
    }

    // ---- the populated database, before T229 -----------------------------------------------------------------------------

    /// <param name="Schema">The schema.</param>
    /// <param name="KeyedRows">The rows the stamp should key, as (table, key) to id.</param>
    private sealed record PreT229Fixture(string Schema, IReadOnlyDictionary<(string Table, string Key), int> KeyedRows);

    /// <summary>
    /// The demo data as the pre-T229 seeder made it, and a decoy for each clause of the stamp. The decoys that resemble a
    /// demo row under another College go in first, so each has a lower id than the row it resembles and a stamp that lost
    /// its College clause would pick it; the ones inside the demo discipline go in after, so a stamp that lost its
    /// <c>MIN</c> would reach them too.
    /// </summary>
    /// <param name="institutionShortCode"><c>DEMO</c> as the seeder made it, or a short code changed before T229.</param>
    /// <param name="collegeShortCode"><c>DEMO-C</c> as the seeder made it, or a short code changed before T229.</param>
    private async Task<PreT229Fixture> ArrangePreT229DatabaseAsync(string institutionShortCode, string collegeShortCode)
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(MigrationBeforeT229(db));
            (await db.Database.GetPendingMigrationsAsync()).First().Should().Be(T229Migration, "guard: the schema stops just before T229");
        }

        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);

        // Decoys under another College, first.
        var otherCollegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('College of Physicians of South Africa', 'CP-SA', now(), TRUE) RETURNING "Id" """);
        var otherSpecialityId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'General Medicine', TRUE) RETURNING "Id" """, otherCollegeId);
        var otherSubSpecialityId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'General Internal Medicine', TRUE) RETURNING "Id" """, otherSpecialityId);
        await InsertEpaAsync(connection, otherSubSpecialityId, "EPA-001", owningInstitutionId: null);
        await InsertCurriculumAsync(connection, otherSubSpecialityId, "IM Core Curriculum", "2026.1");
        var hospitalId = await InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Legacy Hospital', 'LEGACY', now(), TRUE) RETURNING "Id" """);
        await InsertScaleAsync(connection, "Another ladder", ["A", "B"]);

        // The demo data, as the pre-T229 seeder made it.
        var collegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demo College', $1, now(), TRUE) RETURNING "Id" """, collegeShortCode);
        var institutionId = await InsertAsync(connection,
            """INSERT INTO "Institutions" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('Demo Institution', $1, now(), TRUE) RETURNING "Id" """, institutionShortCode);
        var specialityId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'General Medicine', TRUE) RETURNING "Id" """, collegeId);
        var subSpecialityId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'General Internal Medicine', TRUE) RETURNING "Id" """, specialityId);
        var scaleId = await InsertScaleAsync(connection, "O-R Scale", OrScaleRungs);
        var epaId = await InsertEpaAsync(connection, subSpecialityId, "EPA-001", owningInstitutionId: null);
        var curriculumId = await InsertCurriculumAsync(connection, subSpecialityId, "IM Core Curriculum", "2026.1");
        await InsertAsync(connection,
            """INSERT INTO "CurriculumItems" ("CurriculumId", "EpaId", "RequiredCount", "QuotaPeriod", "MinimumLevelOrder", "WindowMonths", "ScaleId") VALUES ($1, $2, 5, 0, 4, 12, $3) RETURNING "Id" """,
            curriculumId, epaId, scaleId);

        // Decoys inside the demo discipline, after it.
        await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Family Medicine', TRUE) RETURNING "Id" """, collegeId);
        var cardiologyId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Cardiology', TRUE) RETURNING "Id" """, specialityId);
        await InsertEpaAsync(connection, subSpecialityId, "EPA-001", owningInstitutionId: hospitalId);
        await InsertEpaAsync(connection, cardiologyId, "EPA-001", owningInstitutionId: null);
        await InsertCurriculumAsync(connection, subSpecialityId, "IM Core Curriculum", "2026.2");
        await InsertCurriculumAsync(connection, subSpecialityId, "IM Research Track", "2026.1");
        await InsertCurriculumAsync(connection, cardiologyId, "IM Core Curriculum", "2026.1");

        var rows = new Dictionary<(string Table, string Key), int>
        {
            [("Institutions", InstitutionKey)] = institutionId,
            [("Colleges", CollegeKey)] = collegeId,
            [("Specialities", SpecialityKey)] = specialityId,
            [("SubSpecialities", SubSpecialityKey)] = subSpecialityId,
            [("EntrustmentScales", ScaleKey)] = scaleId,
            [("Epas", EpaKey)] = epaId,
            [("Curricula", CurriculumKey)] = curriculumId,
        };

        // What the stamp can find: the institution by DEMO, the College by DEMO-C and the rest through it, the scale by name.
        var keyed = rows
            .Where(pair => pair.Key.Table switch
            {
                "Institutions" => institutionShortCode == "DEMO",
                "EntrustmentScales" => true,
                _ => collegeShortCode == "DEMO-C",
            })
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return new PreT229Fixture(schema, keyed);
    }

    private static async Task<int> InsertScaleAsync(NpgsqlConnection connection, string name, IReadOnlyList<string> rungs)
    {
        var scaleId = await InsertAsync(connection, """INSERT INTO "EntrustmentScales" ("Name") VALUES ($1) RETURNING "Id" """, name);
        for (var order = 1; order <= rungs.Count; order++)
        {
            await InsertAsync(connection,
                """INSERT INTO "EntrustmentLevels" ("ScaleId", "Order", "Label") VALUES ($1, $2, $3) RETURNING "Id" """, scaleId, order, rungs[order - 1]);
        }

        return scaleId;
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

    private static IEnumerable<(string Table, string Key)> ExpectedKeys()
        =>
        [
            ("Institutions", InstitutionKey),
            ("Colleges", CollegeKey),
            ("Specialities", SpecialityKey),
            ("SubSpecialities", SubSpecialityKey),
            ("EntrustmentScales", ScaleKey),
            ("Epas", EpaKey),
            ("Curricula", CurriculumKey),
        ];

    /// <summary>The migration's statement, exactly as its <c>Up</c> hands it to <c>Sql()</c>.</summary>
    private static string DemoSeedKeyStampSql()
        => new T229_DemoSeedKeys().UpOperations.OfType<SqlOperation>().Should().ContainSingle().Which.Sql;

    /// <summary>Every row that carries one of the demo's seed keys (<c>demo</c>, or starting <c>demo:</c>), as (table, key) to id.</summary>
    private async Task<Dictionary<(string Table, string Key), int>> DemoKeysAsync(string schema)
    {
        var rows = await QueryAsync(
            schema,
            string.Join(
                " UNION ALL ",
                KeyedTables.Select(table =>
                    $"""SELECT '{table}', "SeedKey", "Id" FROM "{table}" WHERE "SeedKey" = 'demo' OR "SeedKey" LIKE 'demo:%'""")),
            reader => (Table: reader.GetString(0), Key: reader.GetString(1), Id: reader.GetInt32(2)));

        return rows.ToDictionary(row => (row.Table, row.Key), row => row.Id);
    }

    /// <summary>The scopes of the generic activity types <see cref="DataSeeder" /> creates, one entry per distinct scope.</summary>
    private async Task<List<int>> GenericTypeScopesAsync(string schema)
    {
        var genericKeys = ActivityTypeSeedCatalogue.For(ActivityTypeSeedSource.Generic).Select(entry => entry.Key).ToArray();
        return await QueryAsync(
            schema,
            """SELECT DISTINCT "ScopeId" FROM "ActivityTypes" WHERE "Key" = ANY($1)""",
            reader => reader.GetInt32(0),
            // One parameter, the whole array: a bare string[] would bind as the params array itself, one key per parameter.
            (object)genericKeys);
    }

    private sealed record Census(
        long Institutions,
        long Colleges,
        long Specialities,
        long SubSpecialities,
        long Scales,
        long Levels,
        long Epas,
        long Curricula,
        long Items,
        long ActivityTypes,
        long Procedures);

    private async Task<Census> CensusAsync(string schema)
        => new(
            await CountAsync(schema, "Institutions"),
            await CountAsync(schema, "Colleges"),
            await CountAsync(schema, "Specialities"),
            await CountAsync(schema, "SubSpecialities"),
            await CountAsync(schema, "EntrustmentScales"),
            await CountAsync(schema, "EntrustmentLevels"),
            await CountAsync(schema, "Epas"),
            await CountAsync(schema, "Curricula"),
            await CountAsync(schema, "CurriculumItems"),
            await CountAsync(schema, "ActivityTypes"),
            await CountAsync(schema, "ProcedureCatalogueEntries"));

    private sealed record DemoCensus(long Institutions, long Colleges, long Specialities, long SubSpecialities, long Scales, long Levels, long Epas, long Curricula, long Items);

    /// <summary>
    /// The rows only this seeder would create, counted by the names it gives them (decoys of the same name included, which
    /// no boot changes). The catalogue seeder creates its own rows at the first boot after a migration, so a whole-table
    /// census would move for reasons that are not T229's.
    /// </summary>
    private async Task<DemoCensus> DemoCensusAsync(string schema)
        => new(
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Institutions" WHERE "Name" = 'Demo Institution' OR "ShortCode" = 'DEMO'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Colleges" WHERE "Name" = 'Demo College' OR "ShortCode" = 'DEMO-C'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Specialities" WHERE "Name" = 'General Medicine'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "SubSpecialities" WHERE "Name" = 'General Internal Medicine'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "EntrustmentScales" WHERE "Name" = 'O-R Scale'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "EntrustmentLevels" AS l JOIN "EntrustmentScales" AS s ON s."Id" = l."ScaleId" WHERE s."Name" = 'O-R Scale'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Epas" WHERE "Code" = 'EPA-001'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Curricula" WHERE "Name" = 'IM Core Curriculum'"""),
            await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "CurriculumItems" AS ci JOIN "Curricula" AS c ON c."Id" = ci."CurriculumId" WHERE c."Name" = 'IM Core Curriculum'"""));

    private Task<long> CountAsync(string schema, string table) => ScalarAsync<long>(schema, $"""SELECT COUNT(*) FROM "{table}" """);

    /// <param name="Writes">Every write statement either seeder sent.</param>
    /// <param name="Warnings"><see cref="DataSeeder" />'s warnings.</param>
    /// <param name="CatalogueWarnings"><see cref="PaediatricCatalogueSeeder" />'s warnings.</param>
    private sealed record BootLog(IReadOnlyList<string> Writes, IReadOnlyList<CapturedLogEntry> Warnings, IReadOnlyList<CapturedLogEntry> CatalogueWarnings);

    /// <summary>
    /// One startup's seeding, the way <c>Program.cs</c> runs it: <see cref="DataSeeder" />, then the catalogue, each in its
    /// own context. Every write statement and both seeders' warnings are captured.
    /// </summary>
    private async Task<BootLog> BootAsync(string schema)
    {
        var writes = new WriteCounter();
        var log = new CapturingLogger<DataSeeder>();
        var catalogueLog = new CapturingLogger<PaediatricCatalogueSeeder>();

        await using (var db = NewContext(schema, writes))
        {
            await new DataSeeder(db, log).SeedAsync();
        }

        await using (var db = NewContext(schema, writes))
        {
            await new PaediatricCatalogueSeeder(db, catalogueLog).SeedAsync();
        }

        return new BootLog(writes.Writes, log.Warnings.ToList(), catalogueLog.Warnings.ToList());
    }

    /// <summary>
    /// The migration just before T229, read from the assembly rather than typed, so a branch that merges a migration
    /// timestamped before T229 still stops the schema just short of it.
    /// </summary>
    private static string MigrationBeforeT229(ApplicationDbContext db)
    {
        var migrations = db.Database.GetMigrations().Order(StringComparer.Ordinal).ToList();
        var index = migrations.IndexOf(T229Migration);
        index.Should().BePositive("guard: T229 is in the assembly, and is not its first migration");
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
        (await db.Database.GetAppliedMigrationsAsync()).Should().Contain(T229Migration);
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
    /// reached the server. A copy of <c>CatalogueSeedKeyPostgresTests</c>' counter; the fresh-database test asserts that the
    /// first boot IS seen, so an empty list is never vacuous.
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
    /// Throws, before it reaches the server, the first command that inserts into <paramref name="table" />, and records which
    /// tables the commands before it inserted into.
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

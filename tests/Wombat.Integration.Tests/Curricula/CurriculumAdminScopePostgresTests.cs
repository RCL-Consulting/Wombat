using System.Data.Common;
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

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T211 on a real PostgreSQL server, against the seeded CPSA paediatric catalogue: an InstitutionalAdmin opens the
/// curriculum their institution adopted, with the fifteen national items and their own, and not another institution's;
/// an institution that no longer adopts it opens it to keep the items of its own still on it; the College reads the
/// national items only; an institution with neither an adoption nor an item there opens nothing.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which evaluates <see cref="CurriculumAdminScope.Openable" />'s institution arms in
/// memory. On Npgsql they have to become correlated EXISTS subqueries, over the adoptions and over the curriculum's own
/// items, inside the curriculum query, and the item projection a collection read in the same statement, or EF throws.
/// Both are asserted on what reached the server.
/// </para>
/// <para>
/// The schema helpers follow <c>EntrustmentDecisionAdminScopePostgresTests</c>: each test builds its context on a schema
/// of its own, registered before it is created and dropped in a <c>finally</c>, with <see cref="DisposeAsync" /> as a
/// backstop.
/// </para>
/// </remarks>
public sealed class CurriculumAdminScopePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task EachAdmin_OnPostgres_OpensTheCurriculaTheirRoleAdmits_CutToTheItemsTheyRead()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                var adopted = await ReadAsync(db, world.CurriculumId, InstitutionalAdmin(world.Adopter));

                adopted.Should().NotBeNull("the institution adopted it, so its InstitutionalAdmin opens it");
                adopted!.CanEditCurriculum.Should().BeFalse();
                adopted.Items.Where(item => !item.IsLocal).Should().HaveCount(15).And.OnlyContain(item => !item.CanEdit,
                    "the fifteen national EPAs of v11.1 are the College's to change");
                adopted.Items.Where(item => item.IsLocal).Select(item => item.Id).Should().BeEquivalentTo([world.AdopterItem],
                    "the other institution's own item is not the adopter's to read");
                adopted.Items.Single(item => item.Id == world.AdopterItem).CanEdit.Should().BeTrue();
            }

            var reads = commands.Texts.Where(text => text.Contains("\"Curricula\"", StringComparison.Ordinal)).ToList();
            reads.Should().ContainSingle("the curriculum and its items are read in one statement");
            reads[0].Should().Contain("\"InstitutionCurriculumAdoptions\"", "the adoption is checked in SQL");
            reads[0].Should().Contain("EXISTS", "as a correlated EXISTS, not by reading the adoptions first");

            var otherCommands = new CommandLog();
            await using (var db = NewContext(schema, otherCommands))
            {
                var kept = await ReadAsync(db, world.CurriculumId, InstitutionalAdmin(world.Other));

                kept.Should().NotBeNull("the other institution adopts it no longer, but still has an item of its own on it");
                kept!.CanEditCurriculum.Should().BeFalse();
                kept.Items.Where(item => item.IsLocal).Select(item => item.Id).Should().BeEquivalentTo([world.OtherItem],
                    "the adopter's own item is not the other institution's to read");
                kept.Items.Should().HaveCount(16).And.OnlyContain(item => item.CanEdit == item.IsLocal);
            }

            var otherReads = otherCommands.Texts.Where(text => text.Contains("\"Curricula\"", StringComparison.Ordinal)).ToList();
            otherReads.Should().ContainSingle("the own-item arm is checked in the same statement");
            System.Text.RegularExpressions.Regex.Count(otherReads[0], "EXISTS").Should().Be(2,
                "each institution arm is its own correlated EXISTS: the adoptions, and the curriculum's own items");

            await using (var db = NewContext(schema))
            {
                (await ReadAsync(db, world.CurriculumId, InstitutionalAdmin(world.Stranger)))
                    .Should().BeNull("an institution with neither an adoption nor an item there reads it as not found");

                var list = await new GetCurriculaListQueryHandler(db).Handle(
                    new GetCurriculaListQuery(InstitutionalAdmin(world.Adopter)), CancellationToken.None);
                list.Select(curriculum => curriculum.Id).Should().Equal([world.CurriculumId]);
                list.Single().Items.Should().HaveCount(16, "the list shows the items the read shows");

                (await new GetCurriculaListQueryHandler(db).Handle(
                    new GetCurriculaListQuery(InstitutionalAdmin(world.Other)), CancellationToken.None))
                    .Select(curriculum => curriculum.Id).Should().Equal([world.CurriculumId]);

                (await new GetCurriculaListQueryHandler(db).Handle(
                    new GetCurriculaListQuery(InstitutionalAdmin(world.Stranger)), CancellationToken.None))
                    .Should().BeEmpty();

                var college = await ReadAsync(db, world.CurriculumId, CollegeAdmin(world.CollegeId));
                college!.CanEditCurriculum.Should().BeTrue();
                college.Items.Should().HaveCount(15).And.OnlyContain(item => !item.IsLocal && item.CanEdit,
                    "an institution's own items are neither the College's to change nor to read");

                var administrator = await ReadAsync(db, world.CurriculumId, Administrator());
                administrator!.Items.Should().HaveCount(17).And.OnlyContain(item => item.CanEdit);
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    /// <summary>
    /// T222 on PostgreSQL, against the seeded v11.1 curriculum, whose fifteen national EPAs are all items: each EPA picker
    /// leaves out what the curriculum holds, in the one statement that lists the EPAs, and an Administrator reads whose
    /// own each local item is, in the one statement that reads the curriculum.
    /// </summary>
    [Fact]
    public async Task OnPostgres_ThePickersLeaveOutWhatTheCurriculumHolds_AndAnAdministratorReadsEachLocalItemsOwner()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            // A second local EPA of the adopter's, not yet on the curriculum.
            await using (var db = NewContext(schema))
            {
                var subSpecialityId = await db.Curricula.Where(entity => entity.Id == world.CurriculumId).Select(entity => entity.SubSpecialityId).SingleAsync();
                db.Epas.Add(new Epa { Code = "LOC-A02", Title = "The adopter's second", SubSpecialityId = subSpecialityId, OwningInstitutionId = world.Adopter });
                await db.SaveChangesAsync();
            }

            var pickerCommands = new CommandLog();
            await using (var db = NewContext(schema, pickerCommands))
            {
                (await PickerAsync(db, world.CurriculumId, null, CollegeAdmin(world.CollegeId)))
                    .Should().BeEmpty("every national EPA of v11.1 is already an item, so the College has nothing to add");
                (await PickerAsync(db, world.CurriculumId, null, InstitutionalAdmin(world.Adopter)))
                    .Should().Equal(["LOC-A02"], "the national EPAs and LOC-A01 are on the curriculum; LOC-B01 is not the adopter's");
                (await PickerAsync(db, world.CurriculumId, world.AdopterItem, InstitutionalAdmin(world.Adopter)))
                    .Should().Equal(["LOC-A01", "LOC-A02"], "the edited item keeps its own EPA on offer");
            }

            var pickerReads = pickerCommands.Texts.Where(text => text.Contains("FROM \"Epas\"", StringComparison.Ordinal)).ToList();
            pickerReads.Should().HaveCount(3, "each picker lists its EPAs in one statement");
            pickerReads.Should().OnlyContain(text => text.Contains("\"CurriculumItems\"", StringComparison.Ordinal),
                "the EPAs the curriculum holds are left out in SQL, not after reading the items");

            var readCommands = new CommandLog();
            await using (var db = NewContext(schema, readCommands))
            {
                var administrator = (await ReadAsync(db, world.CurriculumId, Administrator()))!;
                administrator.Items.Where(item => item.IsLocal).ToDictionary(item => item.Id, item => item.OwningInstitutionName)
                    .Should().BeEquivalentTo(new Dictionary<int, string?>
                    {
                        [world.AdopterItem] = "Adopting Academic",
                        [world.OtherItem] = "Other Academic"
                    });
                administrator.Items.Where(item => !item.IsLocal).Should().HaveCount(15).And.OnlyContain(item => item.OwningInstitutionName == null);
            }

            var reads = readCommands.Texts.Where(text => text.Contains("\"Curricula\"", StringComparison.Ordinal)).ToList();
            reads.Should().ContainSingle("the owners' names are read with the curriculum and its items");
            reads[0].Should().Contain("\"Institutions\"", "each owner is a correlated lookup in the item projection");

            await using (var db = NewContext(schema))
            {
                (await ReadAsync(db, world.CurriculumId, InstitutionalAdmin(world.Adopter)))!.Items
                    .Should().OnlyContain(item => item.OwningInstitutionName == null, "every local item the adopter reads is its own");

                var saved = await new UpdateCurriculumItemCommandHandler(db).Handle(
                    new UpdateCurriculumItemCommand(world.CurriculumId, world.AdopterItem, world.AdopterEpa, 2, QuotaPeriod.AcademicYear, 3, 12,
                        null, null, null, null, null, false, Administrator()),
                    CancellationToken.None);
                saved.Items.Single(item => item.Id == world.AdopterItem).OwningInstitutionName
                    .Should().Be("Adopting Academic", "a command's returned curriculum names the owners as the read does");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private static async Task<IReadOnlyList<string>> PickerAsync(ApplicationDbContext db, int curriculumId, int? itemId, ClaimsPrincipal principal)
        => (await new ListCurriculumItemEpaOptionsQueryHandler(db).Handle(
                new ListCurriculumItemEpaOptionsQuery(curriculumId, itemId, principal), CancellationToken.None))
            .Select(epa => epa.Code)
            .ToList();

    private sealed record World(int CurriculumId, int CollegeId, int Adopter, int Other, int Stranger, int AdopterItem, int OtherItem, int AdopterEpa);

    /// <summary>
    /// Three institutions. The first adopts the paediatric curriculum and keeps an item of its own on it; the second keeps
    /// one there too (as it would from an adoption since superseded) and has adopted nothing; the third has neither.
    /// </summary>
    private async Task<World> SeedWorldAsync(string schema)
    {
        await using var db = NewContext(schema);

        var curriculum = await db.Curricula
            .Where(entity => entity.Name == PaediatricCurriculumName)
            .Select(entity => new { entity.Id, entity.SubSpecialityId, entity.SubSpeciality.Speciality.CollegeId })
            .SingleAsync();

        var adopter = new Institution { Name = "Adopting Academic", ShortCode = "T211-A" };
        var other = new Institution { Name = "Other Academic", ShortCode = "T211-B" };
        var stranger = new Institution { Name = "Stranger Academic", ShortCode = "T211-C" };
        db.Institutions.AddRange(adopter, other, stranger);
        await db.SaveChangesAsync();

        var adopterEpa = new Epa { Code = "LOC-A01", Title = "The adopter's own", SubSpecialityId = curriculum.SubSpecialityId, OwningInstitutionId = adopter.Id };
        var otherEpa = new Epa { Code = "LOC-B01", Title = "The other's own", SubSpecialityId = curriculum.SubSpecialityId, OwningInstitutionId = other.Id };
        db.Epas.AddRange(adopterEpa, otherEpa);
        await db.SaveChangesAsync();

        var adopterItem = LocalItem(curriculum.Id, adopterEpa.Id, adopter.Id);
        var otherItem = LocalItem(curriculum.Id, otherEpa.Id, other.Id);
        db.Set<CurriculumItem>().AddRange(adopterItem, otherItem);
        db.InstitutionCurriculumAdoptions.Add(new InstitutionCurriculumAdoption
        {
            InstitutionId = adopter.Id,
            CurriculumId = curriculum.Id,
            SubSpecialityId = curriculum.SubSpecialityId,
            AdoptedOn = new DateOnly(2026, 1, 1),
            IsActive = true
        });
        await db.SaveChangesAsync();

        return new World(curriculum.Id, curriculum.CollegeId, adopter.Id, other.Id, stranger.Id, adopterItem.Id, otherItem.Id, adopterEpa.Id);
    }

    private static CurriculumItem LocalItem(int curriculumId, int epaId, int institutionId)
        => new()
        {
            CurriculumId = curriculumId,
            EpaId = epaId,
            OwningInstitutionId = institutionId,
            RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 3,
            WindowMonths = 12
        };

    private static Task<CurriculumDto?> ReadAsync(ApplicationDbContext db, int curriculumId, ClaimsPrincipal principal)
        => new GetCurriculumByIdQueryHandler(db).Handle(new GetCurriculumByIdQuery(curriculumId, principal), CancellationToken.None);

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => Principal(
            new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
            new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture)));

    private static ClaimsPrincipal CollegeAdmin(int collegeId)
        => Principal(
            new Claim(ClaimTypes.Role, WombatRoles.CollegeAdmin),
            new Claim(WombatClaimTypes.CollegeId, collegeId.ToString(CultureInfo.InvariantCulture)));

    private static ClaimsPrincipal Administrator()
        => Principal(new Claim(ClaimTypes.Role, WombatRoles.Administrator));

    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "t211-admin"), .. claims],
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

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
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

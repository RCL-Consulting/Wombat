using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.EntrustmentDecisions;

/// <summary>
/// T183 on a real PostgreSQL server: the entrustment-decision admin list, narrowed to the caller's institution through
/// <see cref="TraineeScopeResolver.PreferredProfiles" /> in SQL and judged by <see cref="TraineeScopeResolver.IsAdministeredBy" />
/// over <see cref="TraineeScopeResolver.ResolveManyAsync" />; and a refused revoke, saved as the audit pipeline would.
/// </summary>
/// <remarks>
/// <para>
/// The unit suites run on EF InMemory, which evaluates the preferred-profile predicate and the resolver's id lists in
/// memory. On Npgsql the first has to become a correlated NOT EXISTS inside the decision query's EXISTS, and the second
/// an array parameter, or EF throws, or the list is filtered only after every decision in the country has been read.
/// Both are asserted on what reached the server.
/// </para>
/// <para>
/// The schema helpers follow <c>MsfCampaignScopePostgresTests</c>: each test builds its context on a schema of its own,
/// registered before it is created and dropped in a <c>finally</c>, with <see cref="DisposeAsync" /> as a backstop.
/// </para>
/// </remarks>
public sealed class EntrustmentDecisionAdminScopePostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task TheAdminList_OnPostgres_IsConfinedToTheCallersInstitutionInSql_AndToTheirSpeciality()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                (await ListAsync(db, Admin(WombatRoles.InstitutionalAdmin, world.Host))).Should().BeEquivalentTo([world.HostDecision]);
            }

            var listing = commands.Texts.Where(text => text.Contains("\"EntrustmentDecisions\"", StringComparison.Ordinal)).ToList();
            listing.Should().ContainSingle("the decisions are read in one query, not one per trainee");
            listing[0].Should().Contain("\"TraineeProfiles\"", "the institution narrowing runs in SQL");
            listing[0].Should().Contain("NOT EXISTS", "the preferred-profile predicate runs in SQL");
            commands.Texts.Should().HaveCount(4, "one query for the decisions, and three for the scopes of every trainee on it");

            await using (var db = NewContext(schema))
            {
                // The moved trainee's older profile is at the host; the preferred one, the highest id, is elsewhere.
                (await ListAsync(db, Admin(WombatRoles.InstitutionalAdmin, world.Elsewhere)))
                    .Should().BeEquivalentTo([world.ElsewhereDecision, world.MovedDecision]);
                (await ListAsync(db, Administrator()))
                    .Should().BeEquivalentTo([world.HostDecision, world.ElsewhereDecision, world.MovedDecision]);

                (await ListAsync(db, Admin(WombatRoles.SpecialityAdmin, world.Host, specialityId: world.SpecialityId)))
                    .Should().BeEquivalentTo([world.HostDecision]);
                (await ListAsync(db, Admin(WombatRoles.SpecialityAdmin, world.Host, specialityId: world.SpecialityId + 10_000)))
                    .Should().BeEmpty("a SpecialityAdmin of another speciality oversees none of these trainees");
                (await ListAsync(db, Admin(WombatRoles.SpecialityAdmin, world.Host, specialityId: world.SpecialityId + 10_000, alsoRole: WombatRoles.CommitteeMember)))
                    .Should().BeEmpty("a committee seat beside it reads these trainees, and does not make them theirs to administer");
                (await ListAsync(db, Admin(WombatRoles.SpecialityAdmin, world.Elsewhere, specialityId: world.SpecialityId)))
                    .Should().BeEquivalentTo([world.ElsewhereDecision, world.MovedDecision]);

                var scopes = await TraineeScopeResolver.ResolveManyAsync(
                    db, ["trainee-host", "trainee-else", "trainee-moved", "nobody"], CancellationToken.None);
                scopes.Keys.Should().BeEquivalentTo(["trainee-host", "trainee-else", "trainee-moved"]);
                scopes["trainee-moved"].InstitutionId.Should().Be(world.Elsewhere);
                scopes["trainee-host"].Should().Be(await TraineeScopeResolver.ResolveAsync(db, "trainee-host", CancellationToken.None));
                scopes["trainee-host"].SpecialityId.Should().Be(world.SpecialityId);
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task ARefusedRevoke_OnPostgres_LeavesTheDecisionActive_EvenAfterTheAuditPipelinesSave()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedWorldAsync(schema);

            await using (var db = NewContext(schema))
            {
                var act = () => RevokeAsync(db, world.HostDecision, Admin(WombatRoles.InstitutionalAdmin, world.Elsewhere));
                await act.Should().ThrowAsync<UnauthorizedAccessException>();

                // What AuditPipelineBehavior does from its catch.
                await db.SaveChangesAsync();
            }

            await using (var db = NewContext(schema))
            {
                var after = await db.EntrustmentDecisions.AsNoTracking().SingleAsync(decision => decision.Id == world.HostDecision);
                after.Status.Should().Be(EntrustmentDecisionStatus.Active);
                after.RevokedOn.Should().BeNull();
            }

            await using (var db = NewContext(schema))
            {
                await RevokeAsync(db, world.HostDecision, Admin(WombatRoles.InstitutionalAdmin, world.Host));
            }

            await using (var db = NewContext(schema))
            {
                var after = await db.EntrustmentDecisions.AsNoTracking().SingleAsync(decision => decision.Id == world.HostDecision);
                after.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
                after.RevokedByUserId.Should().Be($"admin-{world.Host}");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private sealed record World(
        int Host,
        int Elsewhere,
        int SpecialityId,
        int HostDecision,
        int ElsewhereDecision,
        int MovedDecision);

    /// <summary>
    /// A trainee at the demo institution, one at a second institution, and one with two past profiles (the demo
    /// institution first, then the other), each holding one active decision issued by a demo-institution panel.
    /// </summary>
    private async Task<World> SeedWorldAsync(string schema)
    {
        await using var db = NewContext(schema);

        var host = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        var institution = new Institution { Name = "Elsewhere General", ShortCode = "T183-ELSE" };
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();
        var elsewhere = institution.Id;

        var curriculum = await db.Curricula
            .OrderBy(entity => entity.Id)
            .Select(entity => new { entity.Id, entity.SubSpecialityId, entity.SubSpeciality.SpecialityId })
            .FirstAsync();

        var epa = await db.Epas.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
        var level = await db.EntrustmentLevels.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();

        AddProfile(db, "trainee-host", host, curriculum.Id, isActive: true);
        AddProfile(db, "trainee-else", elsewhere, curriculum.Id, isActive: true);
        AddProfile(db, "trainee-moved", host, curriculum.Id, isActive: false);
        await db.SaveChangesAsync();
        AddProfile(db, "trainee-moved", elsewhere, curriculum.Id, isActive: false);
        await db.SaveChangesAsync();

        var panel = new DecisionPanel
        {
            Name = "T183 CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = host,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }]
        };
        db.DecisionPanels.Add(panel);
        await db.SaveChangesAsync();

        var hostDecision = await AddDecisionAsync(db, panel.Id, "trainee-host", epa, level);
        var elsewhereDecision = await AddDecisionAsync(db, panel.Id, "trainee-else", epa, level);
        var movedDecision = await AddDecisionAsync(db, panel.Id, "trainee-moved", epa, level);

        return new World(host, elsewhere, curriculum.SpecialityId, hostDecision, elsewhereDecision, movedDecision);
    }

    private static async Task<int> AddDecisionAsync(ApplicationDbContext db, int panelId, string traineeUserId, int epaId, int levelId)
    {
        var review = new CommitteeReview
        {
            PanelId = panelId,
            TraineeUserId = traineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 1)
        };
        db.CommitteeReviews.Add(review);
        db.Entry(review).Property(entity => entity.State).CurrentValue = CommitteeReviewState.Ratified;
        await db.SaveChangesAsync();

        var decision = EntrustmentDecision.Issue(
            traineeUserId, epaId, levelId, new DateOnly(2026, 7, 1), expiresOn: null,
            review.Id, "chair-1", "Consistent across the period.", []);
        db.EntrustmentDecisions.Add(decision);
        await db.SaveChangesAsync();
        return decision.Id;
    }

    private static async Task<int[]> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => (await new ListEntrustmentDecisionsForAdminQueryHandler(db, FakeUserDirectory.Empty)
                .Handle(new ListEntrustmentDecisionsForAdminQuery(null, null, principal), CancellationToken.None))
            .Select(decision => decision.Id)
            .ToArray();

    private static Task<EntrustmentDecisionDto> RevokeAsync(ApplicationDbContext db, int decisionId, ClaimsPrincipal principal)
        => new RevokeEntrustmentDecisionCommandHandler(db).Handle(
            new RevokeEntrustmentDecisionCommand(decisionId, "Concern raised at the site.", principal), CancellationToken.None);

    private static void AddProfile(ApplicationDbContext db, string userId, int institutionId, int curriculumId, bool isActive)
        => db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = isActive
        });

    private static ClaimsPrincipal Admin(string role, int institutionId, int? specialityId = null, string? alsoRole = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"admin-{institutionId}"),
            new(ClaimTypes.Role, role),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
        };

        if (alsoRole is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, alsoRole));
        }

        if (specialityId is int speciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, speciality.ToString(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "IntegrationTest", ClaimTypes.Name, ClaimTypes.Role));
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
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

    /// <summary>Drops every schema this test created. Called from each test's finally and again from DisposeAsync.</summary>
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

    /// <summary>Every query EF sends through one context, so "in SQL, not per row" is asserted on the wire.</summary>
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

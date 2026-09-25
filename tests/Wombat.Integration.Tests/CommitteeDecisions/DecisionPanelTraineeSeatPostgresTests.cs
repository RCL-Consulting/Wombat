using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T237 on a real PostgreSQL server, through the real user store (<see cref="UserAdministrationService" />): a committee
/// member who also holds Trainee is offered by the panel picker for no seat, and a panel naming them is refused before
/// anything is written.
/// </summary>
/// <remarks>
/// <para>
/// Every unit test of the rule answers the role listing from <c>FakeUserDirectory</c>, which is written to answer as
/// the real listing does: a record carrying only the role it was listed by. This is the check that the real one does,
/// since the rule cannot read Trainee from a committee member's record and must read the Trainee listing instead
/// (<c>TraineeScopeResolver.HoldersAsync</c>).
/// </para>
/// <para>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class DecisionPanelTraineeSeatPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const string Chair = "chair-a";
    private const string Member = "member-a";
    private const string Representative = "rep-a";

    /// <summary>A trainee who sits on no committee: never asked about, so never read (T237 review).</summary>
    private const string OtherTrainee = "trainee-a";

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task OnPostgres_ACommitteeMemberWhoHoldsTrainee_IsOfferedForNoSeat_AndAPanelSeatingThemIsRefused_WithNothingWritten()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        try
        {
            await using var root = await MigratedServicesAsync(schema);

            int institutionId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();
                institutionId = institution.Id;

                NomineeSeed.AddUser(db, Chair, institutionId, WombatRoles.CommitteeMember);
                NomineeSeed.AddUser(db, Member, institutionId, WombatRoles.CommitteeMember);
                NomineeSeed.AddUser(db, Representative, institutionId, WombatRoles.CommitteeMember, WombatRoles.Trainee);
                NomineeSeed.AddUser(db, OtherTrainee, institutionId, WombatRoles.Trainee);
                await db.SaveChangesAsync();
            }

            await using var act = root.CreateAsyncScope();
            var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), context);
            var admin = InstitutionalAdmin(institutionId);

            // The real store answers who holds a role for exactly the people asked about, in one query: the rule asks it
            // of a panel's candidates, never of every trainee in the country.
            (await users.WhichHoldRoleAsync([Chair, Member, Representative, "nobody-at-all"], WombatRoles.Trainee))
                .Should().BeEquivalentTo([Representative], "only those asked about, and only those who hold the role");

            var candidates = await new ListPanelMemberCandidatesQueryHandler(users).Handle(
                new ListPanelMemberCandidatesQuery(admin), CancellationToken.None);

            candidates.Select(candidate => candidate.UserId).Should().BeEquivalentTo(
                [Chair, Member], "the picker every seat reads leaves out someone who holds Trainee");

            foreach (var seat in new[] { DecisionPanelMemberRole.Chair, DecisionPanelMemberRole.External })
            {
                DecisionPanelMemberInput[] members = seat == DecisionPanelMemberRole.Chair
                    ? [new(Representative, DecisionPanelMemberRole.Chair), new(Member, DecisionPanelMemberRole.Member)]
                    : [new(Chair, DecisionPanelMemberRole.Chair), new(Representative, DecisionPanelMemberRole.External)];

                var create = () => new CreateDecisionPanelCommandHandler(context, users).Handle(
                    new CreateDecisionPanelCommand("Hospital CCC", DecisionPanelScope.Institution, institutionId, null, members, admin),
                    CancellationToken.None);

                (await create.Should().ThrowAsync<InvalidOperationException>(seat.ToString()))
                    .Which.Message.Should().Be(PanelSeat.NotEligible);

                // As the audit pipeline would, from its catch: the refusal came before anything was added.
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.DecisionPanels.CountAsync()).Should().Be(0);
                (await db.Set<DecisionPanelMember>().CountAsync()).Should().Be(0);
            }

            // The control: the same panel without them is accepted, through the same real store.
            await new CreateDecisionPanelCommandHandler(context, users).Handle(
                new CreateDecisionPanelCommand(
                    "Hospital CCC", DecisionPanelScope.Institution, institutionId, null,
                    [new(Chair, DecisionPanelMemberRole.Chair), new(Member, DecisionPanelMemberRole.Member)], admin),
                CancellationToken.None);

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.Set<DecisionPanelMember>().Select(member => member.UserId).ToListAsync())
                    .Should().BeEquivalentTo([Chair, Member]);
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, $"inst-admin-{institutionId}"),
                new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "test"));

    private async Task<ServiceProvider> MigratedServicesAsync(string schema)
    {
        await using (var connection = new NpgsqlConnection(_baseConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE SCHEMA \"{schema}\"";
            await command.ExecuteNonQueryAsync();
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(SchemaConnectionString(schema)));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var root = services.BuildServiceProvider();

        await using (var migrationScope = root.CreateAsyncScope())
        {
            await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        }

        return root;
    }

    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString) { SearchPath = schema, Pooling = false }.ConnectionString;

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
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    /// <summary>The same resolution order as the other PostgreSQL tests.</summary>
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

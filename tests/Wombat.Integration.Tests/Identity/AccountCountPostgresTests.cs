using System.Data.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T400 (flow 18, K2, Q4, T14) on a real PostgreSQL server: Home's Accounts card, counted by
/// <see cref="UserAdministrationService.CountAccountsAsync" /> in one statement. An erased account is in neither figure;
/// an administrator's lock (stored as <c>infinity</c>) and a sign-in lockout still running are locked; a lapsed lockout is
/// not.
/// </summary>
/// <remarks>
/// Isolated the way <c>UserDisplayNamesPostgresTests</c> is: a migrated schema of its own, dropped in a finally and again
/// on dispose.
/// </remarks>
public sealed class AccountCountPostgresTests : IAsyncLifetime
{
    private static readonly DateTime Now = new(2026, 11, 10, 19, 39, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task CountAccounts_LeavesTheErasedOut_AndCountsEveryRunningLockout_InOneStatement()
    {
        var schema = await _schemas.CreateAsync();
        var commands = new CommandLog();

        try
        {
            await using var root = await MigratedServicesAsync(schema, commands);

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                NomineeSeed.AddUser(db, "active-1", institutionId: null);
                NomineeSeed.AddUser(db, "active-no-role", institutionId: null);
                NomineeSeed.AddUser(db, "admin-locked", institutionId: null, UserDeactivation.IndefiniteLockoutEnd);
                NomineeSeed.AddUser(db, "sign-in-lockout", institutionId: null, new DateTimeOffset(Now.AddMinutes(10)));
                NomineeSeed.AddUser(db, "lapsed-lockout", institutionId: null, new DateTimeOffset(Now.AddMinutes(-1)));
                // Erased: the account is kept with its names blanked and the indefinite lockout, and its erasure record
                // names it (ErasureExecutor). It is in neither figure.
                NomineeSeed.AddUser(db, "erased-1", institutionId: null, UserDeactivation.IndefiniteLockoutEnd);
                var request = DataRightsRequest.Create("erased-1", "Erased Person", DataRightsRequestType.Erasure, "Leaving.", Now);
                db.Set<DataRightsRequest>().Add(request);
                await db.SaveChangesAsync();
                db.Set<DataRightsErasureRecord>().Add(
                    DataRightsErasureRecord.Create(request.Id, "erased-1", "erased-person-7f3a", Now, "[]"));
                await db.SaveChangesAsync();
            }

            // Guard: the indefinite lock reached the server as infinity, the value the count must still read as running.
            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var stored = await db.Database
                    .SqlQueryRaw<string>("""SELECT "LockoutEnd"::text AS "Value" FROM "AspNetUsers" WHERE "Id" = 'admin-locked'""")
                    .SingleAsync();
                stored.Should().Be("infinity");
            }

            await using var act = root.CreateAsyncScope();
            var service = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
                act.ServiceProvider.GetRequiredService<ApplicationDbContext>());
            commands.Texts.Clear();

            var count = await service.CountAccountsAsync(Now);

            count.Should().Be(new Wombat.Application.Common.Interfaces.AccountCount(Accounts: 5, Locked: 2),
                "five accounts but the erased one; the administrator's lock and the running sign-in lockout");
            commands.Texts.Should().ContainSingle("one round trip, not a read of every user");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task CountAccounts_WithNoAccount_IsZero()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedServicesAsync(schema, new CommandLog());
            await using var act = root.CreateAsyncScope();
            var service = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
                act.ServiceProvider.GetRequiredService<ApplicationDbContext>());

            (await service.CountAccountsAsync(Now)).Should().Be(new Wombat.Application.Common.Interfaces.AccountCount(0, 0));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private async Task<ServiceProvider> MigratedServicesAsync(string schema, CommandLog commands)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .AddInterceptors(commands));
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

    /// <summary>Every statement that reached the server, to count the count's round trips.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}

using System.Data.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Users;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T142 on a real PostgreSQL server: the one name lookup behind every page that shows people,
/// <see cref="UserAdministrationService.GetDisplayNamesAsync" />, read through <see cref="UserDisplayNames" /> as the
/// pages' queries read it.
/// </summary>
/// <remarks>
/// <para>
/// Every unit test of those queries answers the lookup from <c>FakeUserDirectory</c>, so nothing else runs the real
/// query: that the id list translates to one server-side statement rather than a read of every user, that ids compare
/// by the column's collation, and what a user with no name on record comes back as.
/// </para>
/// <para>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>), registered
/// before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class UserDisplayNamesPostgresTests : IAsyncLifetime
{
    private const string Named = "user-named";
    private const string FirstNameOnly = "user-first-name-only";
    private const string Nameless = "user-nameless";
    private const string NotAskedAbout = "user-not-asked-about";
    private const string Nobody = "user-who-does-not-exist";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task GetDisplayNames_OnPostgres_NamesExactlyTheUsersAskedAbout_InOneStatement()
    {
        var schema = await _schemas.CreateAsync();
        var commands = new CommandLog();

        try
        {
            await using var root = await MigratedServicesAsync(schema, commands);

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Name(NomineeSeed.AddUser(db, Named, institutionId: null), "Thandi", "Nkosi");
                Name(NomineeSeed.AddUser(db, FirstNameOnly, institutionId: null), "Sipho", string.Empty);
                Name(NomineeSeed.AddUser(db, Nameless, institutionId: null), string.Empty, string.Empty);
                Name(NomineeSeed.AddUser(db, NotAskedAbout, institutionId: null), "Not", "Asked");
                await db.SaveChangesAsync();
            }

            await using var act = root.CreateAsyncScope();
            var service = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
                act.ServiceProvider.GetRequiredService<ApplicationDbContext>());
            string[] asked = [Named, FirstNameOnly, Nameless, Nobody, Named.ToUpperInvariant()];
            commands.Texts.Clear();

            var names = await service.GetDisplayNamesAsync(asked);

            names.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                [Named] = "Thandi Nkosi",
                [FirstNameOnly] = "Sipho",
                [Nameless] = string.Empty
            }, "a user who does not exist is left out, ids compare exactly, and nobody not asked about is read");

            var statement = commands.Texts.Should().ContainSingle("one round trip for every id, not one per user").Subject;
            statement.Should().Contain("FROM \"AspNetUsers\"").And.Contain("WHERE", "the ids are filtered on the server");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ThroughUserDisplayNames_OnPostgres_APageShowsTheName_OrTheIdWhereThereIsNoName()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            await using var root = await MigratedServicesAsync(schema, new CommandLog());

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Name(NomineeSeed.AddUser(db, Named, institutionId: null), "Thandi", "Nkosi");
                Name(NomineeSeed.AddUser(db, Nameless, institutionId: null), string.Empty, string.Empty);
                await db.SaveChangesAsync();
            }

            await using var act = root.CreateAsyncScope();
            var service = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
                act.ServiceProvider.GetRequiredService<ApplicationDbContext>());

            var names = await UserDisplayNames.ResolveAsync(service, [Named, Nameless, Nobody], CancellationToken.None);

            names.NameOf(Named).Should().Be("Thandi Nkosi");
            names.NameOf(Nameless).Should().Be(Nameless, "a blank cell would read as nobody");
            names.NameOf(Nobody).Should().Be(Nobody);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static void Name(WombatIdentityUser user, string firstName, string lastName)
    {
        user.FirstName = firstName;
        user.LastName = lastName;
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

    /// <summary>Every statement that reached the server, to count the lookup's round trips.</summary>
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

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}

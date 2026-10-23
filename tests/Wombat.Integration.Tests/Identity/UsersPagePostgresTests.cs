using System.Data.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T376, flow 12, lane U, on a real PostgreSQL server: one page of Users in SQL
/// (<see cref="UserAdministrationService.ListUsersPageAsync" />). S7's order is the database's, so it is pinned here, with
/// "du Plessis" and "van Rensburg"; S6's words, Role with No role and Status (K25) translate; and a page of 20 reads its
/// roles in one statement whatever its size, so K26's N+1 is gone.
/// </summary>
/// <remarks>Isolated as <c>UserDisplayNamesPostgresTests</c> is: a migrated schema of its own, dropped in a finally.</remarks>
public sealed class UsersPagePostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_TheOrder_TheWords_TheRoles_TheStatus_TheScope_AndOneStatementForThePagesRoles()
    {
        var schema = await _schemas.CreateAsync();
        var commands = new CommandLog();

        try
        {
            await using var root = await MigratedServicesAsync(schema, commands);
            int kgk;
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var institution = new Institution { Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true };
                var elsewhere = new Institution { Name = "Elsewhere", ShortCode = "ELS", IsActive = true };
                db.Institutions.AddRange(institution, elsewhere);
                await db.SaveChangesAsync();
                kgk = institution.Id;

                Person(db, "zulu", "Thandi", "Zulu", kgk, null, WombatRoles.CommitteeMember, WombatRoles.Assessor);
                Person(db, "vanrensburg", "John", "van Rensburg", kgk, null, WombatRoles.CommitteeMember);
                Person(db, "duplessis", "Pieter", "du Plessis", kgk, null);
                Person(db, "botha", "Sarah", "Botha", kgk, null, WombatRoles.CommitteeMember);
                Person(db, "patel", "Mohammed", "Patel", kgk, DateTimeOffset.MaxValue, WombatRoles.Assessor);
                Person(db, "smit", "Pieter", "Smit", kgk, DateTimeOffset.UtcNow.AddMinutes(10), WombatRoles.Coordinator);
                Person(db, "where", "Else", "Where", elsewhere.Id, null, WombatRoles.Coordinator);
                Person(db, "devadmin", "Demo", "Administrator", null, null, WombatRoles.Administrator);
                foreach (var n in Enumerable.Range(1, 25))
                {
                    Person(db, $"trainee-{n:00}", $"Reg{n:00}", "Trainee", kgk, null, WombatRoles.Trainee);
                }

                await db.SaveChangesAsync();
            }

            await using var act = root.CreateAsyncScope();
            var service = new UserAdministrationService(
                act.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(),
                act.ServiceProvider.GetRequiredService<ApplicationDbContext>());

            UserDirectoryFilter At(int? institution, int size = 100, string[]? words = null, string? role = null, bool noRole = false, UserDirectoryStatus? status = null, int page = 1)
                => new(institution, words ?? [], role, noRole, status, page, size);

            // S7: lower(LastName), lower(FirstName), Id: the particles sort by their letters, never by their missing capital.
            var all = await service.ListUsersPageAsync(At(kgk));
            all.Rows.Select(user => user.LastName).Where(name => name != "Trainee").Should().Equal(
                "Botha", "du Plessis", "Patel", "Smit", "van Rensburg", "Zulu");
            all.Rows.Where(user => user.LastName == "Trainee").Select(user => user.FirstName).Should().BeInAscendingOrder(
                StringComparer.Ordinal, "then by first name");

            // S6: each word, either order, part of a name or the address.
            foreach (var words in new[] { new[] { "thandi", "zulu" }, ["zulu", "thandi"], ["andi"], ["zulu@test.local"] })
            {
                (await service.ListUsersPageAsync(At(kgk, words: words))).Rows.Select(user => user.UserId).Should().Equal(["zulu"], string.Join(' ', words));
            }

            (await service.ListUsersPageAsync(At(kgk, words: ["thandi", "patel"]))).MatchCount.Should().Be(0);

            // Role, No role, Status (K25: Smit's brute-force lockout is Active; Patel's administrative lock is not).
            (await service.ListUsersPageAsync(At(kgk, role: WombatRoles.CommitteeMember))).Rows.Select(user => user.UserId)
                .Should().Equal("botha", "vanrensburg", "zulu");
            (await service.ListUsersPageAsync(At(kgk, noRole: true))).Rows.Select(user => user.UserId).Should().Equal("duplessis");
            var locked = await service.ListUsersPageAsync(At(kgk, status: UserDirectoryStatus.LockedOut));
            locked.Rows.Select(user => user.UserId).Should().Equal("patel");
            locked.Rows.Single().IsDeactivated.Should().BeTrue();
            (await service.ListUsersPageAsync(At(kgk, status: UserDirectoryStatus.Active))).Rows.Select(user => user.UserId)
                .Should().Contain("smit").And.NotContain("patel");

            // Scope: her institution only; every account, those with none included, for no institution.
            (await service.ListUsersPageAsync(At(kgk))).TotalCount.Should().Be(31);
            (await service.ListUsersPageAsync(At(null))).TotalCount.Should().Be(33);

            // A page of 20 of 31, its roles read in one statement, and the second page's rest.
            commands.Texts.Clear();
            var first = await service.ListUsersPageAsync(At(kgk, size: 20));
            first.Rows.Should().HaveCount(20);
            first.MatchCount.Should().Be(31);
            first.Rows.Single(user => user.UserId == "botha").Roles.Should().Equal(WombatRoles.CommitteeMember);
            first.Rows.Single(user => user.UserId == "duplessis").Roles.Should().BeEmpty();
            commands.Texts.Should().HaveCount(4, "two counts, the page with its scopes, and the page's roles: none per row (K26)");
            commands.Texts.Count(text => text.Contains("\"AspNetUserRoles\"")).Should().Be(1);

            var second = await service.ListUsersPageAsync(At(kgk, size: 20, page: 2));
            second.Rows.Should().HaveCount(11);
            second.Rows.Single(user => user.UserId == "zulu").Roles.Should().BeEquivalentTo([WombatRoles.CommitteeMember, WombatRoles.Assessor]);
            second.Page.Should().Be(2);
            (await service.ListUsersPageAsync(At(kgk, size: 20, page: 9))).Page.Should().Be(2, "past the last page, the last");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static void Person(
        ApplicationDbContext db, string id, string first, string last, int? institutionId, DateTimeOffset? lockoutEnd, params string[] roles)
    {
        var user = NomineeSeed.AddUser(db, id, institutionId, lockoutEnd, roles);
        user.FirstName = first;
        user.LastName = last;
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

    /// <summary>Every statement that reached the server.</summary>
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

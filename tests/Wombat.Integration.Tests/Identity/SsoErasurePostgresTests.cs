using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.DataRights;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T149: erasure removes every external login, on real PostgreSQL. The erasure executor runs raw SQL, so the in-memory
/// provider cannot run it. A linked provider subject identifies the person, and a live one is a way back in: SSO sign-in
/// finds the account by it.
/// </summary>
/// <remarks>
/// Isolated the way <c>WbaToolAllowListPostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>),
/// registered before it is created and dropped in a finally and again on dispose.
/// </remarks>
public sealed class SsoErasurePostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Erasure_RemovesEveryExternalLogin()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
            services.AddIdentity<WombatIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<ErasureExecutor>();

            await using var root = services.BuildServiceProvider();

            await using (var migrationScope = root.CreateAsyncScope())
            {
                await migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
            }

            string userId;
            await using (var arrange = root.CreateAsyncScope())
            {
                var users = arrange.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                var user = new WombatIdentityUser
                {
                    UserName = "naidoo@kgk.test",
                    Email = "naidoo@kgk.test",
                    FirstName = "Thandi",
                    LastName = "Naidoo",
                    InstitutionId = null
                };
                (await users.CreateAsync(user)).Succeeded.Should().BeTrue();
                (await users.AddLoginAsync(user, new UserLoginInfo("kgk", "idp-subject-1", "KGK sign-in"))).Succeeded.Should().BeTrue();
                (await users.AddLoginAsync(user, new UserLoginInfo("other", "idp-subject-2", "Other sign-in"))).Succeeded.Should().BeTrue();
                userId = user.Id;
            }

            await using (var act = root.CreateAsyncScope())
            {
                // The erasure record references its request, as the approval flow's request row would be there.
                var request = DataRightsRequest.Create(
                    userId, "Thandi Naidoo", DataRightsRequestType.Erasure, "Leaving the programme.", DateTime.UtcNow);
                var db = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Set<DataRightsRequest>().Add(request);
                await db.SaveChangesAsync();

                await act.ServiceProvider.GetRequiredService<ErasureExecutor>().ExecuteAsync(request, "salt-for-tests", CancellationToken.None);
            }

            await using (var assert = root.CreateAsyncScope())
            {
                var users = assert.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                var erased = await users.FindByIdAsync(userId);
                erased.Should().NotBeNull("erasure pseudonymises the account; it does not delete the row");
                (await users.GetLoginsAsync(erased!)).Should().BeEmpty();
                (await users.FindByLoginAsync("kgk", "idp-subject-1")).Should().BeNull("the provider subject no longer leads back in");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }
}

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T292: <see cref="DevUserSeeder" /> seeds <c>devadmin@wombat.local</c>, a global Administrator (no institution, no
/// College, no scope), so an agent can play the Administrator's steps on dev without the bootstrap credential. Built on the
/// real Identity stack over the in-memory store, with the password rules the app configures, and booted in the order
/// <c>Program.cs</c> seeds.
/// </summary>
public sealed class DevUserSeederTests : IDisposable
{
    private const string AdministratorEmail = "devadmin@wombat.local";

    private readonly ServiceProvider _root;

    public DevUserSeederTests()
    {
        // One name, so every boot's scope opens the same database, as every start of a server does.
        var databaseName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));

        // The password rules DependencyInjection.AddInfrastructure configures, so a dev password they refuse fails here.
        services.AddIdentity<WombatIdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 4;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<RoleSeeder>();
        services.AddScoped<DevUserSeeder>();

        _root = services.BuildServiceProvider();
    }

    public void Dispose() => _root.Dispose();

    [Fact]
    public async Task AFreshDevBoot_SeedsDevAdmin_AsAGlobalAdministratorOnly_AndASecondBootCreatesNothing()
    {
        await BootAsync(withDemoData: true);

        await using (var scope = _root.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var admin = await users.FindByEmailAsync(AdministratorEmail);

            admin.Should().NotBeNull();
            (await users.GetRolesAsync(admin!)).Should().Equal(WombatRoles.Administrator);
            admin!.InstitutionId.Should().BeNull("a global Administrator, as AdminSeeder makes one");
            admin.CollegeId.Should().BeNull();
            admin.FirstName.Should().Be("Demo");
            admin.LastName.Should().Be("Administrator");
            admin.EmailConfirmed.Should().BeTrue();
            (await users.CheckPasswordAsync(admin, "ChangeThisDevAdmin123!")).Should().BeTrue();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await dbContext.UserSpecialityScopes.AnyAsync(entry => entry.UserId == admin.Id)).Should().BeFalse();
            (await dbContext.UserSubSpecialityScopes.AnyAsync(entry => entry.UserId == admin.Id)).Should().BeFalse();
            (await dbContext.TraineeProfiles.AnyAsync(entry => entry.UserId == admin.Id)).Should().BeFalse();
        }

        var before = await CensusAsync();
        before.Users.Should().Be(8, "guard: the seven dev users before T292, and devadmin");

        await BootAsync(withDemoData: true);

        (await CensusAsync()).Should().Be(before, "a second boot creates no user, role, scope or profile");
    }

    /// <summary>
    /// An Administrator needs neither the demo curriculum nor the Demo Institution, so the seeder's early returns for a
    /// missing demo row skip the other dev users but not this one.
    /// </summary>
    [Fact]
    public async Task ADevBootWithoutTheDemoData_StillSeedsDevAdmin_AndNoOtherDevUser()
    {
        await BootAsync(withDemoData: false);

        await using var scope = _root.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await dbContext.Users.Select(user => user.Email).ToListAsync()).Should().Equal(AdministratorEmail);

        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        (await users.GetRolesAsync((await users.FindByEmailAsync(AdministratorEmail))!)).Should().Equal(WombatRoles.Administrator);
    }

    /// <summary>
    /// The seeding half of one Development start, in the order <c>Program.cs</c> runs it: roles, the demo data and the
    /// national catalogue, then the dev users. <c>AdminSeeder</c> is left out, as it is on a dev host with no bootstrap
    /// credential configured.
    /// </summary>
    private async Task BootAsync(bool withDemoData)
    {
        await using var scope = _root.CreateAsyncScope();
        var services = scope.ServiceProvider;

        await services.GetRequiredService<RoleSeeder>().SeedAsync();

        if (withDemoData)
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            await new DataSeeder(dbContext).SeedAsync();
            await new PaediatricCatalogueSeeder(dbContext).SeedAsync();
        }

        await services.GetRequiredService<DevUserSeeder>().SeedAsync();
    }

    private sealed record Census(int Users, int UserRoles, int SpecialityScopes, int SubSpecialityScopes, int TraineeProfiles);

    private async Task<Census> CensusAsync()
    {
        await using var scope = _root.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return new Census(
            await dbContext.Users.CountAsync(),
            await dbContext.UserRoles.CountAsync(),
            await dbContext.UserSpecialityScopes.CountAsync(),
            await dbContext.UserSubSpecialityScopes.CountAsync(),
            await dbContext.TraineeProfiles.CountAsync());
    }
}

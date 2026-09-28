using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// My account's How you sign in card (T339, flow 02; T286's list and removal), against a real Identity stack over the
/// in-memory store: which ways in an account has, and the removal of an institutional sign-in, which never removes the
/// last way in, not even when two tabs each remove one of two.
/// </summary>
public sealed class InstitutionalSignInMethodsTests : IDisposable
{
    private const string Password = "Correct-Horse-Battery-9!";

    private readonly ServiceProvider _root;

    public InstitutionalSignInMethodsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var database = Guid.NewGuid().ToString();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(database));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.Configure<SsoOptions>(options =>
        {
            options.Providers.Add(new SsoProviderOptions { Key = "kgk", DisplayName = "Kgosi Kgari Teaching Hospital", InstitutionId = 1 });
            options.Providers.Add(new SsoProviderOptions { Key = "marula", DisplayName = "Marula Test Hospital", InstitutionId = 1 });
        });
        services.AddScoped<UserAdministrationService>();
        _root = services.BuildServiceProvider();
    }

    public void Dispose() => _root.Dispose();

    // ---- the list ----

    [Fact]
    public async Task AnAccountWithAPassword_AndALink_HasBoth_EachSignInByItsConfiguredName()
    {
        var id = await CreateAsync(password: Password, logins: [("kgk", "subject-1", "stored name")]);

        var methods = await InScope(service => service.GetSignInMethodsAsync(id));

        methods!.HasLocalPassword.Should().BeTrue();
        methods.InstitutionalSignIns.Should().Equal(new InstitutionalSignIn("kgk", "Kgosi Kgari Teaching Hospital"));
    }

    [Fact]
    public async Task ASignInNoLongerConfigured_IsNamedAsItWasStored_ElseByItsKey()
    {
        var id = await CreateAsync(password: Password, logins: [("gone", "s-1", "Old Hospital"), ("bare", "s-2", null)]);

        var methods = await InScope(service => service.GetSignInMethodsAsync(id));

        methods!.InstitutionalSignIns.Should().BeEquivalentTo(
            [new InstitutionalSignIn("gone", "Old Hospital"), new InstitutionalSignIn("bare", "bare")]);
    }

    [Theory]
    [InlineData(true, false, "no password is stored")]
    [InlineData(false, true, "a password the account may not use signs nothing in (AllowLocalPassword false)")]
    public async Task APasswordThatCannotSignIn_IsNoLocalPassword(bool allowLocalPassword, bool storePassword, string because)
    {
        var id = await CreateAsync(password: storePassword ? Password : null, allowLocalPassword: allowLocalPassword,
            logins: [("kgk", "subject-1", null)]);

        var methods = await InScope(service => service.GetSignInMethodsAsync(id));

        methods!.HasLocalPassword.Should().BeFalse(because);
    }

    [Fact]
    public async Task AnAccountThatDoesNotExist_HasNoMethods()
        => (await InScope(service => service.GetSignInMethodsAsync("nobody"))).Should().BeNull();

    // ---- the removal ----

    [Fact]
    public async Task ARemoval_RemovesTheSignIn_AndChangesTheSecurityStamp()
    {
        var id = await CreateAsync(password: Password, logins: [("kgk", "subject-1", null)]);
        var stamp = await StampAsync(id);

        var removal = await InScope(service => service.RemoveInstitutionalSignInAsync(id, "kgk"));

        removal.Should().Be(InstitutionalSignInRemoval.Removed);
        (await LoginsAsync(id)).Should().BeEmpty();
        (await StampAsync(id)).Should().NotBe(stamp, "every other session of the account ends; the endpoint issues this one's cookie again");
    }

    [Fact]
    public async Task TheLastWayIn_IsNotRemoved()
    {
        var id = await CreateAsync(password: null, allowLocalPassword: false, logins: [("kgk", "subject-1", null)]);

        var removal = await InScope(service => service.RemoveInstitutionalSignInAsync(id, "kgk"));

        removal.Should().Be(InstitutionalSignInRemoval.LastWayIn);
        (await LoginsAsync(id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task APasswordTheAccountMayNotUse_IsNoWayIn()
    {
        var id = await CreateAsync(password: Password, allowLocalPassword: false, logins: [("kgk", "subject-1", null)]);

        (await InScope(service => service.RemoveInstitutionalSignInAsync(id, "kgk"))).Should().Be(InstitutionalSignInRemoval.LastWayIn);
        (await LoginsAsync(id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task WithNoPassword_OneOfTwoSignIns_IsRemoved_AndThenTheOtherIsTheLastWayIn()
    {
        var id = await CreateAsync(password: null, allowLocalPassword: false, logins: [("kgk", "s-1", null), ("marula", "s-2", null)]);

        (await InScope(service => service.RemoveInstitutionalSignInAsync(id, "marula"))).Should().Be(InstitutionalSignInRemoval.Removed);
        (await InScope(service => service.RemoveInstitutionalSignInAsync(id, "kgk"))).Should().Be(InstitutionalSignInRemoval.LastWayIn);
        (await LoginsAsync(id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task AProviderTheAccountHasNoSignInThrough_IsNotLinked_AndNothingChanges()
    {
        var id = await CreateAsync(password: Password, logins: [("kgk", "subject-1", null)]);
        var stamp = await StampAsync(id);

        (await InScope(service => service.RemoveInstitutionalSignInAsync(id, "marula"))).Should().Be(InstitutionalSignInRemoval.NotLinked);
        (await LoginsAsync(id)).Should().Equal("kgk");
        (await StampAsync(id)).Should().Be(stamp);
    }

    /// <summary>
    /// Two tabs of an account with no password and two sign-ins, each removing one. The second read the account (its
    /// session check) before the first's removal was saved. Its count of the logins comes after that save, so it sees the
    /// one left, which is the last way in. Had it counted first, the save would be refused on the concurrency stamp
    /// (<see cref="ARemovalRefusedOnTheConcurrencyStamp_IsAConflict_AndLeavesNothingStaged" />). Either way one is left.
    /// </summary>
    [Fact]
    public async Task TwoTabsRemovingTwoSignIns_LeaveOne()
    {
        var id = await CreateAsync(password: null, allowLocalPassword: false, logins: [("kgk", "s-1", null), ("marula", "s-2", null)]);

        await using var second = _root.CreateAsyncScope();
        (await second.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByIdAsync(id))
            .Should().NotBeNull("guard: the second tab's session check reads the account first");

        (await InScope(service => service.RemoveInstitutionalSignInAsync(id, "marula"))).Should().Be(InstitutionalSignInRemoval.Removed);
        (await second.ServiceProvider.GetRequiredService<UserAdministrationService>().RemoveInstitutionalSignInAsync(id, "kgk"))
            .Should().Be(InstitutionalSignInRemoval.LastWayIn);

        (await LoginsAsync(id)).Should().Equal("kgk");
    }

    [Fact]
    public async Task ARemovalRefusedOnTheConcurrencyStamp_IsAConflict_AndLeavesNothingStaged()
    {
        var id = await CreateAsync(password: Password, logins: [("kgk", "s-1", null), ("marula", "s-2", null)]);

        await using var second = _root.CreateAsyncScope();
        (await second.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByIdAsync(id))
            .Should().NotBeNull("guard: the second tab read the account before the first saved");

        (await InScope(service => service.RemoveInstitutionalSignInAsync(id, "marula"))).Should().Be(InstitutionalSignInRemoval.Removed);

        var service = second.ServiceProvider.GetRequiredService<UserAdministrationService>();
        (await service.RemoveInstitutionalSignInAsync(id, "kgk")).Should().Be(InstitutionalSignInRemoval.Conflict);

        var db = second.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "the audit pipeline saves the request's context for its failure row, and must not commit the refused removal");
        await db.SaveChangesAsync();
        (await LoginsAsync(id)).Should().Equal("kgk");
    }

    private async Task<T> InScope<T>(Func<UserAdministrationService, Task<T>> act)
    {
        await using var scope = _root.CreateAsyncScope();
        return await act(scope.ServiceProvider.GetRequiredService<UserAdministrationService>());
    }

    private async Task<string> CreateAsync(
        string? password,
        IReadOnlyList<(string Provider, string Subject, string? DisplayName)> logins,
        bool allowLocalPassword = true)
    {
        await using var scope = _root.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var email = $"{Guid.NewGuid():N}@example.test";
        var user = new WombatIdentityUser { UserName = email, Email = email, AllowLocalPassword = allowLocalPassword, InstitutionId = 1 };
        var created = password is null ? await users.CreateAsync(user) : await users.CreateAsync(user, password);
        created.Succeeded.Should().BeTrue(string.Join("; ", created.Errors.Select(error => error.Code)));
        foreach (var login in logins)
        {
            (await users.AddLoginAsync(user, new UserLoginInfo(login.Provider, login.Subject, login.DisplayName))).Succeeded.Should().BeTrue();
        }

        return user.Id;
    }

    private async Task<List<string>> LoginsAsync(string userId)
    {
        await using var scope = _root.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().UserLogins
            .Where(login => login.UserId == userId).Select(login => login.LoginProvider).OrderBy(provider => provider).ToListAsync();
    }

    private async Task<string?> StampAsync(string userId)
    {
        await using var scope = _root.CreateAsyncScope();
        return (await scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByIdAsync(userId))!.SecurityStamp;
    }
}

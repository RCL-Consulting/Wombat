using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T285: a registration Identity refuses is an <see cref="InvitationRefusedException" /> carrying Identity's codes, which
/// the register page describes in words of its own. Until T285 it was a bare exception whose message was Identity's
/// descriptions joined with "; ", which the register endpoint put in the address and the audit row kept: a taken user name
/// read "Username '…' is already taken.", quoting the address.
/// </summary>
public sealed class InvitedUserProvisionerTests : IDisposable
{
    private const int InstitutionId = 10;

    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public InvitedUserProvisionerTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // The password rules DependencyInjection.AddInfrastructure configures.
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
        services.AddScoped<InvitedUserProvisioner>();

        _root = services.BuildServiceProvider();
        _scope = _root.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }

    private InvitedUserProvisioner Provisioner => _scope.ServiceProvider.GetRequiredService<InvitedUserProvisioner>();

    private UserManager<WombatIdentityUser> Users => _scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();

    [Fact]
    public async Task APasswordIdentityRefuses_IsRefusedWithIdentitysCodes_NeverItsWords()
    {
        var act = () => ProvisionAsync("invitee@kgk.test", "short");

        var refusal = (await act.Should().ThrowAsync<InvitationRefusedException>()).Which;
        refusal.Reason.Should().Be(InvitationRefusal.AccountNotCreated);
        refusal.ErrorCodes.Should().Equal(
            nameof(IdentityErrorDescriber.PasswordTooShort),
            nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric),
            nameof(IdentityErrorDescriber.PasswordRequiresDigit),
            nameof(IdentityErrorDescriber.PasswordRequiresUpper));
        refusal.Message.Should().Be(
            "Identity refused the new account (PasswordTooShort, PasswordRequiresNonAlphanumeric, PasswordRequiresDigit, " +
            "PasswordRequiresUpper).",
            "the audit row keeps the codes, not Identity's descriptions");
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(0, "no account was created, nor left for a save to commit");
    }

    [Fact]
    public async Task AnAddressAnAccountHolds_IsRefusedAsAccountExists()
    {
        await CreateAsync("invitee@kgk.test", userName: "invitee@kgk.test");

        var act = () => ProvisionAsync("invitee@kgk.test", "Registered-Pa55word!");

        var refusal = (await act.Should().ThrowAsync<InvitationRefusedException>()).Which;
        refusal.Reason.Should().Be(InvitationRefusal.AccountExists);
        refusal.Message.Should().Be("A user with this email address already exists.");
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(1, "only the account that holds the address");
    }

    [Fact]
    public async Task AnAddressAnotherAccountHoldsAsItsUserName_IsRefusedAsAccountExists_NotInIdentitysWords()
    {
        // The email look-up misses it, the other account's email being another address; Identity's user-name check does not.
        await CreateAsync("someone-else@kgk.test", userName: "invitee@kgk.test");

        var act = () => ProvisionAsync("invitee@kgk.test", "Registered-Pa55word!");

        var refusal = (await act.Should().ThrowAsync<InvitationRefusedException>()).Which;
        refusal.Reason.Should().Be(InvitationRefusal.AccountExists);
        refusal.Message.Should().NotContain("invitee@kgk.test").And.NotContain("already taken");
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(1, "only the account that holds the address");
    }

    /// <summary>
    /// Identity would create this one: its rules do not ask for a unique email, and the other account's user name is
    /// another address. Only the provisioner's own look-up refuses it, and two accounts sharing an email would break the
    /// sign-in page's look-up for both.
    /// </summary>
    [Fact]
    public async Task AnAddressAnotherAccountHoldsAsItsEmailOnly_IsRefusedAsAccountExists_ThoughIdentityWouldCreateIt()
    {
        await CreateAsync("invitee@kgk.test", userName: "another-name@kgk.test");

        var act = () => ProvisionAsync("invitee@kgk.test", "Registered-Pa55word!");

        (await act.Should().ThrowAsync<InvitationRefusedException>()).Which.Reason.Should().Be(InvitationRefusal.AccountExists);
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(1, "only the account that holds the address");
    }

    /// <summary>
    /// Another registration takes the address after the provisioner's own check looked, so it is Identity's create that
    /// refuses it, with a code of its own. It reads as the check's refusal, never in Identity's words, which quote the
    /// address.
    /// </summary>
    [Fact]
    public async Task AnAddressTakenBetweenTheCheckAndTheCreate_IsRefusedAsAccountExists_NotInIdentitysWords()
    {
        Users.UserValidators.Add(new TakenAtCreate("invitee@kgk.test"));

        var act = () => ProvisionAsync("invitee@kgk.test", "Registered-Pa55word!");

        var refusal = (await act.Should().ThrowAsync<InvitationRefusedException>()).Which;
        refusal.Reason.Should().Be(InvitationRefusal.AccountExists);
        refusal.Message.Should().Be("A user with this email address already exists.");
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(0);
    }

    /// <summary>
    /// An address Identity will not take as a user name can never be registered, whatever the person enters. Until T285 it
    /// was refused in Identity's words, which quoted the address; now as a refusal of its own, before anything is created.
    /// </summary>
    [Fact]
    public async Task AnAddressIdentityWillNotTakeAsAUserName_IsRefusedAsNotAccepted_BeforeAnythingIsCreated()
    {
        var act = () => ProvisionAsync("o'brien@kgk.test", "Registered-Pa55word!");

        var refusal = (await act.Should().ThrowAsync<InvitationRefusedException>()).Which;
        refusal.Reason.Should().Be(InvitationRefusal.AddressNotAccepted);
        refusal.ErrorCodes.Should().BeEmpty();
        refusal.Message.Should().NotContain("o'brien");
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(0, "no account was created, nor left for a save to commit");
    }

    /// <summary>
    /// The invitation's preview asks this, so the register page offers its form only where a submit could succeed (T285
    /// review): an address an account holds, as its email or as its user name, is taken, and one Identity will not take as
    /// a user name is not accepted. Asking adds nothing to the context.
    /// </summary>
    [Fact]
    public async Task TheAddressStatus_IsTheProvisionersOwnTest_AndAddsNothing()
    {
        await CreateAsync("held@kgk.test", userName: "held@kgk.test");
        await CreateAsync("someone-else@kgk.test", userName: "held-as-user-name@kgk.test");
        await CreateAsync("held-as-email@kgk.test", userName: "yet-another-name@kgk.test");

        (await Provisioner.GetAddressStatusAsync("free@kgk.test")).Should().Be(InvitedAddressStatus.Available);
        (await Provisioner.GetAddressStatusAsync("held@kgk.test")).Should().Be(InvitedAddressStatus.Taken);
        (await Provisioner.GetAddressStatusAsync("HELD@kgk.test")).Should().Be(InvitedAddressStatus.Taken,
            "Identity matches the normalised address");
        (await Provisioner.GetAddressStatusAsync("held-as-user-name@kgk.test")).Should().Be(InvitedAddressStatus.Taken,
            "the email look-up misses it; Identity's own user-name check does not");
        (await Provisioner.GetAddressStatusAsync("held-as-email@kgk.test")).Should().Be(InvitedAddressStatus.Taken,
            "Identity's checks miss it, its rules not asking for a unique email; the email look-up does not");
        (await Provisioner.GetAddressStatusAsync("o'brien@kgk.test")).Should().Be(InvitedAddressStatus.NotAccepted);

        var dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "the account that would be created is judged, never added");
        (await AccountsAfterTheAuditRowsSaveAsync()).Should().Be(3);
    }

    /// <summary>
    /// The accounts stored once the context is saved, as the audit row a refused command writes saves it (the audit trap),
    /// then read back with nothing tracked.
    /// </summary>
    private async Task<int> AccountsAfterTheAuditRowsSaveAsync()
    {
        var dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
        return await dbContext.Users.AsNoTracking().CountAsync();
    }

    private Task<ProvisionedInvitationUser> ProvisionAsync(string email, string password)
        => Provisioner.ProvisionAsync(email, password, "Refilwe", "Dlamini", WombatRoles.Coordinator, InstitutionId, null, null, null);

    /// <summary>
    /// Refuses <paramref name="address" /> as Identity's own validator refuses a user name another account took, but only
    /// at the create, whose account carries a password: the account that took it did so after the check had looked.
    /// </summary>
    private sealed class TakenAtCreate(string address) : IUserValidator<WombatIdentityUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<WombatIdentityUser> manager, WombatIdentityUser user)
            => Task.FromResult(user.PasswordHash is not null
                               && string.Equals(user.UserName, address, StringComparison.OrdinalIgnoreCase)
                ? IdentityResult.Failed(new IdentityErrorDescriber().DuplicateUserName(address))
                : IdentityResult.Success);
    }

    private async Task CreateAsync(string email, string userName)
    {
        var created = await Users.CreateAsync(
            new WombatIdentityUser { UserName = userName, Email = email, FirstName = "Held", LastName = "Account", InstitutionId = InstitutionId },
            "Held-Account-Pa55word!");
        created.Succeeded.Should().BeTrue(string.Join("; ", created.Errors.Select(error => error.Description)));
    }
}

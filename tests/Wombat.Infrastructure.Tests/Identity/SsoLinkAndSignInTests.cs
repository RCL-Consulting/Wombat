using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Audit;
using Wombat.Application.Common.Options;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T149: linking an institutional sign-in to a local account, and SSO sign-in, against a real Identity stack
/// (UserManager, SignInManager, the lockout options the app configures) over the in-memory store.
/// </summary>
/// <remarks>
/// Before T149 the link endpoint took the provider, the subject id and the account's email from the form, checked the
/// password without counting failures, and SSO sign-in ignored lockout. So anyone holding any external session could
/// guess any account's password without limit and, on a hit, bind an external identity of their choosing to it.
/// </remarks>
public sealed class SsoLinkAndSignInTests : IDisposable
{
    private const string ProviderKey = "kgk";
    private const int ProviderInstitutionId = 10;
    private const int OtherInstitutionId = 20;
    private const string Password = "Correct-Horse-Battery-9!";

    /// <summary>A second provider of the same institution, which asserts verification by Entra's claim (T155).</summary>
    private const string EntraProviderKey = "kgk-entra";
    private const string EntraVerifiedClaim = "xms_edov";

    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;
    private readonly RecordingAuthenticationService _authentication = new();
    private readonly RecordingAuditWriter _audit = new();

    public SsoLinkAndSignInTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // The same lockout bounds DependencyInjection.AddInfrastructure configures.
        services.AddIdentity<WombatIdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthenticationService>(_authentication);
        // Saves the request's context as the real AuditWriter does, so a change left tracked is committed here as it
        // would be in the app: the audit trap (T155).
        services.AddScoped<IAuditWriter>(provider =>
            new FlushingAuditWriter(_audit, provider.GetRequiredService<ApplicationDbContext>()));
        services.Configure<SsoOptions>(options =>
        {
            options.Providers.Add(new SsoProviderOptions
            {
                Key = ProviderKey,
                DisplayName = "KGK sign-in",
                InstitutionId = ProviderInstitutionId,
                GroupsClaim = "groups"
            });
            options.Providers.Add(new SsoProviderOptions
            {
                Key = EntraProviderKey,
                DisplayName = "KGK Entra",
                InstitutionId = ProviderInstitutionId,
                GroupsClaim = "groups",
                EmailVerifiedClaim = EntraVerifiedClaim
            });
        });
        services.AddScoped<SsoGroupMapper>();
        services.AddScoped<ExternalLoginHandler>();
        services.AddScoped<UserAdministrationService>();

        _root = services.BuildServiceProvider();
        _scope = _root.CreateScope();
        _scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { RequestServices = _scope.ServiceProvider };
    }

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }

    private UserManager<WombatIdentityUser> Users => _scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();

    private ExternalLoginHandler Handler => _scope.ServiceProvider.GetRequiredService<ExternalLoginHandler>();

    private ApplicationDbContext Db => _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // ---- linking --------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ALinkWithTheRightPassword_BindsTheExternalLoginFromTheCookie_AndSignsIn()
    {
        var user = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);

        var result = await Handler.LinkAndSignInAsync(Verified("naidoo@kgk.test", subject: "idp-subject-1"), Password, "10.0.0.0", "test");

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        (await Users.GetLoginsAsync(user)).Should().ContainSingle()
            .Which.Should().Match<UserLoginInfo>(login => login.LoginProvider == ProviderKey && login.ProviderKey == "idp-subject-1");
        _authentication.SignedInUserIds.Should().Equal(user.Id);
        _audit.Actions.Should().Contain("SsoAccountLinked");
    }

    [Fact]
    public async Task ALinkTargetsTheAccountTheProvidersEmailNames_SoAnotherAccountCannotBeNamed()
    {
        // The cookie says the IdP authenticated "mallory"; nothing the caller sends can point the link at Naidoo.
        var naidoo = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);
        await CreateUserAsync("mallory@kgk.test", ProviderInstitutionId, password: "Mallorys-Own-Pass-1!");

        var result = await Handler.LinkAndSignInAsync(Verified("mallory@kgk.test", subject: "mallory-subject"), Password, null, null);

        result.Succeeded.Should().BeFalse("Naidoo's password is not Mallory's");
        (await Users.GetLoginsAsync(naidoo)).Should().BeEmpty();
        (await Users.GetAccessFailedCountAsync(naidoo)).Should().Be(0, "Naidoo's account was never the target");
        (await Users.GetAccessFailedCountAsync((await Users.FindByEmailAsync("mallory@kgk.test"))!))
            .Should().Be(1, "the guess was counted against the account the provider named, which is Mallory's");
    }

    [Fact]
    public async Task FiveWrongPasswords_LockTheAccount_AndALockedAccountCannotLinkEvenWithTheRightOne()
    {
        var user = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);
        var external = Verified("naidoo@kgk.test", subject: "idp-subject-1");

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var wrong = await Handler.LinkAndSignInAsync(external, "Wrong-Password-" + attempt, null, null);
            wrong.Succeeded.Should().BeFalse();
        }

        (await Users.IsLockedOutAsync(user)).Should().BeTrue("link guesses count toward the same lockout as the local login");

        var right = await Handler.LinkAndSignInAsync(external, Password, null, null);

        right.Succeeded.Should().BeFalse();
        right.ErrorMessage.Should().Be(ExternalLoginHandler.LinkLockedOutMessage);
        (await Users.GetLoginsAsync(user)).Should().BeEmpty();
        _authentication.SignedInUserIds.Should().BeEmpty();
        _audit.Actions.Should().Contain("SsoAccountLinkLockedOut");
    }

    [Fact]
    public async Task EveryLinkFailureButTheLockout_ReadsTheSame()
    {
        await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);
        await CreateUserAsync("elsewhere@kgk.test", OtherInstitutionId);

        var wrongPassword = await Handler.LinkAndSignInAsync(Verified("naidoo@kgk.test", "s1"), "Wrong-Password-1", null, null);
        var noAccount = await Handler.LinkAndSignInAsync(Verified("nobody@kgk.test", "s2"), Password, null, null);
        var otherInstitution = await Handler.LinkAndSignInAsync(Verified("elsewhere@kgk.test", "s3"), Password, null, null);

        new[] { wrongPassword, noAccount, otherInstitution }
            .Select(result => result.ErrorMessage)
            .Should().OnlyContain(message => message == ExternalLoginHandler.LinkRefusedMessage);
        _authentication.SignedInUserIds.Should().BeEmpty();
    }

    [Fact]
    public async Task AnExternalLoginAlreadyLinked_CannotBeLinkedAgain()
    {
        var first = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);
        (await Users.AddLoginAsync(first, new UserLoginInfo(ProviderKey, "idp-subject-1", "KGK sign-in"))).Succeeded.Should().BeTrue();
        await CreateUserAsync("second@kgk.test", ProviderInstitutionId);

        var result = await Handler.LinkAndSignInAsync(Verified("second@kgk.test", subject: "idp-subject-1"), Password, null, null);

        result.Succeeded.Should().BeFalse();
        _authentication.SignedInUserIds.Should().BeEmpty();
    }

    [Fact]
    public async Task AnSsoOnlyAccount_CannotBeLinked_AndTheAttemptCountsNothingAgainstIt()
    {
        // The local login refuses an SSO-only account before any password check. So does the link: otherwise anyone
        // with an IdP identity asserting the address could lock the owner out, five attempts at a time.
        var user = await CreateUserAsync("sso-only@kgk.test", ProviderInstitutionId);
        user.AllowLocalPassword = false;
        await Users.UpdateAsync(user);

        var result = await Handler.LinkAndSignInAsync(Verified("sso-only@kgk.test", "another-subject"), Password, null, null);

        result.ErrorMessage.Should().Be(ExternalLoginHandler.LinkRefusedMessage);
        (await Users.GetAccessFailedCountAsync(user)).Should().Be(0);
        (await Users.GetLoginsAsync(user)).Should().BeEmpty();
    }

    [Fact]
    public async Task AnAdministratorAccount_CannotBeLinked()
    {
        var admin = await CreateUserAsync("admin@kgk.test", ProviderInstitutionId);
        await GrantAsync(admin, "Administrator");

        var result = await Handler.LinkAndSignInAsync(Verified("admin@kgk.test", "idp-admin"), Password, null, null);

        result.ErrorMessage.Should().Be(ExternalLoginHandler.AdministratorMessage);
        (await Users.GetLoginsAsync(admin)).Should().BeEmpty();
        _authentication.SignedInUserIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ADeactivatedAccount_IsNotOfferedALink()
    {
        var user = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);
        await Users.SetLockoutEndDateAsync(user, UserDeactivation.IndefiniteLockoutEnd);

        var result = await Handler.HandleCallbackAsync(Verified("naidoo@kgk.test", "idp-subject-1"), null, null);

        result.RequiresLinking.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.LockedOutMessage);
    }

    [Fact]
    public async Task AVerifiedEmailNamingAnAccountOfTheInstitution_IsOfferedALink()
    {
        // The control for the unverified refusals below.
        await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);

        var result = await Handler.HandleCallbackAsync(Verified("naidoo@kgk.test", "idp-subject-1"), null, null);

        result.RequiresLinking.Should().BeTrue(result.ErrorMessage);
        result.Email.Should().Be("naidoo@kgk.test");
    }

    [Theory]
    [InlineData(ProviderInstitutionId, null)]
    [InlineData(ProviderInstitutionId, "false")]
    [InlineData(OtherInstitutionId, "false")]
    public async Task AnUnverifiedEmailAnAccountHolds_IsRefusedAsUnverified_ExactlyAsAnAddressNoOneHolds(
        int holdersInstitutionId, string? emailVerified)
    {
        // A link is a lasting credential, so it is offered only for a verified email (T155 review). An unverified email
        // goes on to provisioning, which must refuse it for being unverified before it asks who holds the address:
        // otherwise "in use" for a held address and "not verified" for any other would tell anyone who can type an
        // address at their provider which addresses have accounts, here or at another institution.
        var holder = await CreateUserAsync("held@kgk.test", holdersInstitutionId);

        var held = await Handler.HandleCallbackAsync(External("held@kgk.test", "idp-a", emailVerified: emailVerified), null, null);
        var free = await Handler.HandleCallbackAsync(External("free@kgk.test", "idp-b", emailVerified: emailVerified), null, null);

        held.Should().BeEquivalentTo(free, "the answer must not depend on whether an account holds the address");
        held.RequiresLinking.Should().BeFalse("an unverified email is not matched to an account");
        held.Succeeded.Should().BeFalse();
        held.ErrorMessage.Should().Be(ExternalLoginHandler.EmailNotVerifiedMessage);

        (await Users.GetLoginsAsync(holder)).Should().BeEmpty();
        (await Users.FindByEmailAsync("free@kgk.test")).Should().BeNull();
        (await Users.FindByLoginAsync(ProviderKey, "idp-a")).Should().BeNull();
        _authentication.SignedInUserIds.Should().BeEmpty();
        _audit.Entries.Where(entry => entry.Action == "SsoProvisioningRefused")
            .Select(entry => entry.ErrorMessage)
            .Should().Equal(ExternalLoginHandler.EmailNotVerifiedReason, ExternalLoginHandler.EmailNotVerifiedReason);
    }

    [Theory]
    [InlineData(Password)]
    [InlineData("Wrong-Password-1")]
    public async Task ALinkPostedWithAnUnverifiedEmail_IsRefusedBeforeAnyAccountIsLookedUpOrPasswordChecked(string password)
    {
        // The submit endpoint takes any external cookie the provider's handler set, without passing through the callback,
        // so it judges the email itself. Right password or wrong, nothing is linked and nothing counts against the account.
        var user = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);

        var result = await Handler.LinkAndSignInAsync(External("naidoo@kgk.test", "idp-subject-1"), password, null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.EmailNotVerifiedMessage);
        (await Users.GetLoginsAsync(user)).Should().BeEmpty();
        (await Users.GetAccessFailedCountAsync(user)).Should().Be(0, "no password was checked");
        _authentication.SignedInUserIds.Should().BeEmpty();
        _audit.Entries.Should().ContainSingle()
            .Which.Should().Match<AuditEntry>(entry =>
                entry.Action == "SsoAccountLinkFailed"
                && entry.ErrorMessage == ExternalLoginHandler.EmailNotVerifiedReason
                && entry.ActorUserId == null
                && entry.InstitutionId == null);
    }

    // ---- SSO sign-in ----------------------------------------------------------------------------------------------

    [Fact]
    public async Task ALinkedAccount_SignsInThroughTheProvider()
    {
        // The control for the two refusals below.
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");

        var result = await Handler.HandleCallbackAsync(External("naidoo@kgk.test", "idp-subject-1"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        _authentication.SignedInUserIds.Should().Equal(user.Id);
    }

    [Fact]
    public async Task AnAdministratorsLock_IsHonouredBySsoSignIn()
    {
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        await Users.SetLockoutEndDateAsync(user, UserDeactivation.IndefiniteLockoutEnd);

        var result = await Handler.HandleCallbackAsync(
            External("naidoo@kgk.test", "idp-subject-1", name: "Changed Name"), null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.LockedOutMessage);
        _authentication.SignedInUserIds.Should().BeEmpty();
        (await Users.FindByIdAsync(user.Id))!.FirstName.Should().NotBe("Changed", "a refused sign-in touches nothing");
        _audit.Actions.Should().Contain("SsoLoginRefused");
    }

    [Fact]
    public async Task ABruteForceLockout_DoesNotBlockSsoSignIn()
    {
        // The lockout protects the password, which SSO never checks. Honouring it here would let anyone keep an SSO user
        // out by typing five wrong passwords at the local login every fifteen minutes.
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        await Users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(15));
        (await Users.IsLockedOutAsync(user)).Should().BeTrue("guard: the account really is locked out right now");

        var result = await Handler.HandleCallbackAsync(External("naidoo@kgk.test", "idp-subject-1"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        _authentication.SignedInUserIds.Should().Equal(user.Id);
    }

    [Fact]
    public async Task AnAdministratorAccount_CannotSignInThroughSso()
    {
        // The role cannot be granted through SSO, so it is not reachable through SSO either: otherwise the institution's
        // identity-provider admins could sign in as a global Administrator.
        var admin = await LinkedUserAsync("admin@kgk.test", ProviderInstitutionId, "idp-admin");
        await GrantAsync(admin, "Administrator");

        var result = await Handler.HandleCallbackAsync(External("admin@kgk.test", "idp-admin"), null, null);

        result.ErrorMessage.Should().Be(ExternalLoginHandler.AdministratorMessage);
        _authentication.SignedInUserIds.Should().BeEmpty();
    }

    [Fact]
    public async Task MovingAUserToAnotherInstitution_DropsTheirExternalLogins_SoTheyAreNotStrandedByTheOldProvider()
    {
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");

        var administration = _scope.ServiceProvider.GetRequiredService<UserAdministrationService>();
        await administration.UpdateScopeAsync(user.Id, OtherInstitutionId, [], []);

        (await Users.FindByLoginAsync(ProviderKey, "idp-subject-1")).Should().BeNull();

        // A scope edit that keeps the institution leaves the links alone.
        var stays = await LinkedUserAsync("stays@kgk.test", ProviderInstitutionId, "idp-subject-2");
        await administration.UpdateScopeAsync(stays.Id, ProviderInstitutionId, [], []);
        (await Users.FindByLoginAsync(ProviderKey, "idp-subject-2")).Should().NotBeNull();
    }

    [Fact]
    public async Task AnAccountOfAnotherInstitution_CannotSignInThroughThisProvider()
    {
        // Its group mappings would otherwise hand the account this institution's roles.
        await LinkedUserAsync("moved@kgk.test", OtherInstitutionId, "idp-subject-9");

        var result = await Handler.HandleCallbackAsync(External("moved@kgk.test", "idp-subject-9"), null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.WrongInstitutionMessage);
        _authentication.SignedInUserIds.Should().BeEmpty();
    }

    // ---- the email a sign-in syncs, and the account it provisions (T155) -------------------------------------------

    [Theory]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("1", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("01", false)]
    [InlineData("yes", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void TheEmailIsVerified_OnlyWhenTheProviderAssertsTrue(string? claimValue, bool expected)
    {
        var principal = External("naidoo@kgk.test", "s1", emailVerified: claimValue).Principal;

        ExternalLoginHandler.EmailIsVerified(principal, "email_verified").Should().Be(expected);
    }

    [Fact]
    public void AVerificationClaimThatContradictsItself_IsUnverified()
    {
        var principal = External("naidoo@kgk.test", "s1", emailVerified: "true").Principal;
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("email_verified", "false"));

        ExternalLoginHandler.EmailIsVerified(principal, "email_verified").Should().BeFalse();
        ExternalLoginHandler.EmailIsVerified(principal, claimType: null).Should().BeFalse("no claim named, nothing verified");
    }

    [Fact]
    public async Task AVerifiedNewEmail_IsWrittenAsOneValidatedChange_ToBothColumnsAndTheirNormalisedForms()
    {
        // The control for every refusal below. The old address was never confirmed; the new one is, by the provider.
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        user.EmailConfirmed = false;
        (await Users.UpdateAsync(user)).Succeeded.Should().BeTrue();
        var stampBefore = user.SecurityStamp;

        var result = await Handler.HandleCallbackAsync(
            External("n.naidoo@kgk.test", "idp-subject-1", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        var stored = await StoredAsync(user.Id);
        stored.Email.Should().Be("n.naidoo@kgk.test");
        stored.UserName.Should().Be("n.naidoo@kgk.test");
        stored.NormalizedEmail.Should().Be("N.NAIDOO@KGK.TEST");
        stored.NormalizedUserName.Should().Be("N.NAIDOO@KGK.TEST");
        stored.EmailConfirmed.Should().BeTrue("the provider verified it");
        stored.SecurityStamp.Should().NotBe(stampBefore, "an email change rotates the stamp, as SetEmailAsync does");
        (await Users.FindByEmailAsync("n.naidoo@kgk.test"))!.Id.Should().Be(user.Id);
        (await Users.FindByEmailAsync("naidoo@kgk.test")).Should().BeNull();
        _audit.Actions.Should().Contain("SsoEmailChanged");
        _authentication.SignedInUserNames.Should().Equal("n.naidoo@kgk.test");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("false")]
    [InlineData("not-a-boolean")]
    public async Task AnUnverifiedEmailClaim_IsNeverWritten_AndTheSignInGoesAhead(string? emailVerified)
    {
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");

        var result = await Handler.HandleCallbackAsync(
            External("someone.else@kgk.test", "idp-subject-1", name: "Thandi Naidoo", emailVerified: emailVerified), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        _authentication.SignedInUserIds.Should().Equal(user.Id);
        var stored = await StoredAsync(user.Id);
        stored.Email.Should().Be("naidoo@kgk.test");
        stored.UserName.Should().Be("naidoo@kgk.test");
        stored.NormalizedEmail.Should().Be("NAIDOO@KGK.TEST");
        stored.NormalizedUserName.Should().Be("NAIDOO@KGK.TEST");
        stored.SecurityStamp.Should().Be(user.SecurityStamp);
        stored.FirstName.Should().Be("Thandi", "the name still syncs; only the email waits on verification");
        _audit.Actions.Should().NotContain("SsoEmailChanged");
    }

    [Fact]
    public async Task AVerifiedEmailNamingAnotherAccount_ChangesNeitherAccount_AndTheOwnerStillSignsIn()
    {
        // Before T155 Identity refused the duplicate user name, the refused values stayed tracked, and the group mapper's
        // save committed them: Naidoo took Venter's address, with the old normalised columns.
        var naidoo = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        var venter = await CreateUserAsync("venter@kgk.test", ProviderInstitutionId);

        var result = await Handler.HandleCallbackAsync(
            External("venter@kgk.test", "idp-subject-1", name: "Thandi Naidoo", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        _authentication.SignedInUserIds.Should().Equal(naidoo.Id);
        _authentication.SignedInUserNames.Should().Equal("naidoo@kgk.test");

        var storedNaidoo = await StoredAsync(naidoo.Id);
        storedNaidoo.Email.Should().Be("naidoo@kgk.test");
        storedNaidoo.UserName.Should().Be("naidoo@kgk.test");
        storedNaidoo.NormalizedEmail.Should().Be("NAIDOO@KGK.TEST");
        storedNaidoo.NormalizedUserName.Should().Be("NAIDOO@KGK.TEST");
        storedNaidoo.SecurityStamp.Should().Be(naidoo.SecurityStamp);
        storedNaidoo.FirstName.Should().Be("Thandi", "the rest of the sign-in goes ahead");

        var storedVenter = await StoredAsync(venter.Id);
        storedVenter.Email.Should().Be("venter@kgk.test");
        storedVenter.UserName.Should().Be("venter@kgk.test");
        storedVenter.SecurityStamp.Should().Be(venter.SecurityStamp);
        storedVenter.ConcurrencyStamp.Should().Be(venter.ConcurrencyStamp);

        (await Users.FindByEmailAsync("venter@kgk.test"))!.Id.Should().Be(venter.Id);
        (await Users.FindByEmailAsync("naidoo@kgk.test"))!.Id.Should().Be(naidoo.Id);
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoEmailSyncRefused")
            .Which.ErrorMessage.Should().Be(ExternalLoginHandler.EmailHeldByAnotherAccountReason);
    }

    [Fact]
    public async Task AnAddressAnotherAccountHoldsOnlyAsItsEmail_IsRefusedToo()
    {
        // Identity checks unique user names, not unique emails, so only the handler's own check stops this. Two accounts
        // with one email would break the sign-in page's lookup for both.
        var naidoo = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        var venter = await CreateUserAsync("venter@kgk.test", ProviderInstitutionId, userName: "venter-legacy");

        await Handler.HandleCallbackAsync(External("venter@kgk.test", "idp-subject-1", emailVerified: "true"), null, null);

        (await StoredAsync(naidoo.Id)).Email.Should().Be("naidoo@kgk.test");
        (await Users.FindByEmailAsync("venter@kgk.test"))!.Id.Should().Be(venter.Id, "one account still holds the address");
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoEmailSyncRefused")
            .Which.ErrorMessage.Should().Be(ExternalLoginHandler.EmailHeldByAnotherAccountReason);
    }

    [Fact]
    public async Task AnAddressAnotherAccountHoldsOnlyAsItsUserName_IsRefusedBeforeAnythingIsChanged()
    {
        // Identity would refuse this one too, but only after the values were set on the tracked account.
        var naidoo = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        await CreateUserAsync("venter.home@example.test", ProviderInstitutionId, userName: "venter@kgk.test");

        await Handler.HandleCallbackAsync(External("venter@kgk.test", "idp-subject-1", emailVerified: "true"), null, null);

        (await StoredAsync(naidoo.Id)).Email.Should().Be("naidoo@kgk.test");
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoEmailSyncRefused")
            .Which.ErrorMessage.Should().Be(ExternalLoginHandler.EmailHeldByAnotherAccountReason);
    }

    [Fact]
    public async Task AVerifiedEmailIdentityRefuses_LeavesNothingTracked_ForALaterSaveToCommit()
    {
        // The tracked-entity trap. UserManager sets the values before it validates them and does not undo them when it
        // refuses, and the audit writer's save and the group mapper's both flush whatever the context tracks. An
        // apostrophe is a valid address but not a valid user name.
        var naidoo = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");

        var result = await Handler.HandleCallbackAsync(
            External("o'naidoo@kgk.test", "idp-subject-1", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        Db.ChangeTracker.Entries<WombatIdentityUser>().Should().OnlyContain(entry => entry.State == EntityState.Unchanged);
        _authentication.SignedInUserNames.Should().Equal(["naidoo@kgk.test"], "the cookie is built from the reloaded account");

        var stored = await StoredAsync(naidoo.Id);
        stored.Email.Should().Be("naidoo@kgk.test");
        stored.UserName.Should().Be("naidoo@kgk.test");
        stored.EmailConfirmed.Should().BeTrue();
        stored.SecurityStamp.Should().Be(naidoo.SecurityStamp);
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoEmailSyncRefused")
            .Which.ErrorMessage.Should().Be(ExternalLoginHandler.EmailRefusedByValidationReason);
    }

    [Fact]
    public async Task ANameChangeIdentityRefuses_IsNotCommittedByTheRoleSyncsSave()
    {
        // The same trap on the name. The stored user name is one Identity no longer accepts, so any update of this
        // account is refused; the refused name must not ride out on the group mapper's save.
        var naidoo = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "idp-subject-1");
        var tracked = await Db.Users.SingleAsync(user => user.Id == naidoo.Id);
        tracked.UserName = "naidoo legacy";
        tracked.NormalizedUserName = "NAIDOO LEGACY";
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var result = await Handler.HandleCallbackAsync(
            External("naidoo@kgk.test", "idp-subject-1", name: "Changed Name"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        (await StoredAsync(naidoo.Id)).FirstName.Should().Be("Test");
    }

    [Theory]
    [InlineData("true")]
    [InlineData("1")]
    public async Task AProviderThatNamesAnotherVerificationClaim_IsJudgedByThatClaimAlone(string entraValue)
    {
        // Entra ID sends no email_verified; its xms_edov optional claim says the same thing, and the provider's
        // configuration names it. It is documented as a boolean and has also been seen as "1".
        var user = await LinkedUserAsync("naidoo@kgk.test", ProviderInstitutionId, "entra-subject-1", EntraProviderKey);

        await Handler.HandleCallbackAsync(External("n.naidoo@kgk.test", "entra-subject-1", emailVerified: "true",
            providerKey: EntraProviderKey), null, null);
        (await StoredAsync(user.Id)).Email.Should().Be("naidoo@kgk.test", "email_verified is not the claim this provider uses");

        await Handler.HandleCallbackAsync(External("n.naidoo@kgk.test", "entra-subject-1", emailVerified: entraValue,
            providerKey: EntraProviderKey, verifiedClaim: EntraVerifiedClaim), null, null);
        (await StoredAsync(user.Id)).Email.Should().Be("n.naidoo@kgk.test");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("false")]
    public async Task AnUnverifiedEmail_ProvisionsNoAccount(string? emailVerified)
    {
        var result = await Handler.HandleCallbackAsync(
            External("new.person@kgk.test", "idp-new", name: "New Person", emailVerified: emailVerified), null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.EmailNotVerifiedMessage);
        (await Users.FindByEmailAsync("new.person@kgk.test")).Should().BeNull();
        (await Users.FindByLoginAsync(ProviderKey, "idp-new")).Should().BeNull();
        _authentication.SignedInUserIds.Should().BeEmpty();
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoProvisioningRefused")
            .Which.Should().Match<AuditEntry>(entry =>
                entry.InstitutionId == ProviderInstitutionId && entry.ErrorMessage == ExternalLoginHandler.EmailNotVerifiedReason);
    }

    [Fact]
    public async Task AVerifiedEmail_ProvisionsAConfirmedSsoOnlyAccount()
    {
        // The control for the refusals around it.
        var roles = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        (await roles.CreateAsync(new IdentityRole("PendingTrainee"))).Succeeded.Should().BeTrue();

        var result = await Handler.HandleCallbackAsync(
            External("new.person@kgk.test", "idp-new", name: "New Person", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeTrue(result.ErrorMessage);
        var created = (await Users.FindByLoginAsync(ProviderKey, "idp-new"))!;
        created.Email.Should().Be("new.person@kgk.test");
        created.EmailConfirmed.Should().BeTrue();
        created.AllowLocalPassword.Should().BeFalse();
        created.InstitutionId.Should().Be(ProviderInstitutionId);
        _audit.Actions.Should().Contain("SsoFirstLogin");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("elsewhere-legacy")]
    public async Task AVerifiedEmailAnotherInstitutionsAccountHolds_ProvisionsNothing(string? holdersUserName)
    {
        // With a user name equal to the address Identity would refuse the duplicate, in its own words; with any other
        // user name it would create a second account with the same email.
        var holder = await CreateUserAsync("elsewhere@kgk.test", OtherInstitutionId, userName: holdersUserName);

        var result = await Handler.HandleCallbackAsync(
            External("elsewhere@kgk.test", "idp-new", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.EmailInUseMessage);
        (await Users.FindByEmailAsync("elsewhere@kgk.test"))!.Id.Should().Be(holder.Id);
        (await Users.FindByLoginAsync(ProviderKey, "idp-new")).Should().BeNull();
        _authentication.SignedInUserIds.Should().BeEmpty();
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoProvisioningRefused")
            .Which.ErrorMessage.Should().Be(ExternalLoginHandler.EmailInUseReason);
    }

    /// <summary>
    /// A verified address Identity's user-name rules refuse (they allow no apostrophe). The person was shown Identity's
    /// own description until T156, which quotes the address; now a fixed sentence, and the codes go to the audit row.
    /// </summary>
    [Fact]
    public async Task ANewAccountIdentityRefuses_IsRefusedInWombatsWords_NeverIdentitys()
    {
        const string address = "o'brien@kgk.test";

        var result = await Handler.HandleCallbackAsync(External(address, "idp-new", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.AccountNotCreatedMessage);
        result.ErrorMessage.Should().NotContain("o'brien").And.NotContain("Username");
        (await Users.FindByLoginAsync(ProviderKey, "idp-new")).Should().BeNull();
        (await Db.Users.AsNoTracking().CountAsync()).Should().Be(0, "no account was created");
        _authentication.SignedInUserIds.Should().BeEmpty();
        var refusal = _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoProvisioningRefused").Which;
        refusal.InstitutionId.Should().Be(ProviderInstitutionId);
        refusal.ErrorMessage.Should().StartWith(ExternalLoginHandler.AccountRefusedByValidationReason)
            .And.Contain(nameof(IdentityErrorDescriber.InvalidUserName))
            .And.NotContain("o'brien", "the row carries no address");
    }

    /// <summary>
    /// An address another sign-in takes between the in-use check and the create: Identity refuses the duplicate user
    /// name, and until T156 the person read "Username '…' is already taken." They are told what the in-use check tells.
    /// </summary>
    [Fact]
    public async Task AnAddressTakenAfterTheInUseCheck_IsToldItIsInUse_NotIdentitysUsernameTaken()
    {
        const string address = "raced@kgk.test";
        Users.UserValidators.Add(new TakenBetweenCheckAndCreate(address));

        var result = await Handler.HandleCallbackAsync(External(address, "idp-new", emailVerified: "true"), null, null);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.EmailInUseMessage);
        result.ErrorMessage.Should().NotContain("already taken").And.NotContain(address);
        (await Users.FindByLoginAsync(ProviderKey, "idp-new")).Should().BeNull();
        _authentication.SignedInUserIds.Should().BeEmpty();
        _audit.Entries.Should().ContainSingle(entry => entry.Action == "SsoProvisioningRefused")
            .Which.ErrorMessage.Should().Be(ExternalLoginHandler.EmailInUseReason);
    }

    // Erasure's removal of external logins is tested on PostgreSQL (SsoErasurePostgresTests): the erasure executor runs
    // raw SQL the in-memory provider cannot.

    // ---- helpers --------------------------------------------------------------------------------------------------

    private async Task<WombatIdentityUser> CreateUserAsync(
        string email, int institutionId, string password = Password, string? userName = null)
    {
        var user = new WombatIdentityUser
        {
            UserName = userName ?? email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = email,
            InstitutionId = institutionId
        };

        var created = await Users.CreateAsync(user, password);
        created.Succeeded.Should().BeTrue(string.Join("; ", created.Errors.Select(error => error.Description)));
        return user;
    }

    private async Task GrantAsync(WombatIdentityUser user, string role)
    {
        var roles = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync(role))
        {
            (await roles.CreateAsync(new IdentityRole(role))).Succeeded.Should().BeTrue();
        }

        (await Users.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
    }

    private async Task<WombatIdentityUser> LinkedUserAsync(
        string email, int institutionId, string subject, string providerKey = ProviderKey)
    {
        var user = await CreateUserAsync(email, institutionId);
        (await Users.AddLoginAsync(user, new UserLoginInfo(providerKey, subject, "KGK sign-in"))).Succeeded.Should().BeTrue();
        return user;
    }

    /// <summary>
    /// The external login the provider authenticated. <paramref name="emailVerified" /> is the raw value of the
    /// verification claim, <paramref name="verifiedClaim" /> (<c>email_verified</c> by default); null sends no such claim.
    /// </summary>
    private static ExternalLoginInfo External(
        string email,
        string subject,
        string? name = null,
        string? emailVerified = null,
        string providerKey = ProviderKey,
        string verifiedClaim = "email_verified")
    {
        var claims = new List<Claim> { new(ClaimTypes.Email, email), new(ClaimTypes.NameIdentifier, subject) };
        if (name is not null)
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        if (emailVerified is not null)
        {
            claims.Add(new Claim(verifiedClaim, emailVerified));
        }

        return new ExternalLoginInfo(new ClaimsPrincipal(new ClaimsIdentity(claims, providerKey)), providerKey, subject, "KGK sign-in");
    }

    /// <summary>An external login whose provider asserts the email as verified, as the link flow requires. (T155)</summary>
    private static ExternalLoginInfo Verified(string email, string subject) => External(email, subject, emailVerified: "true");

    /// <summary>The account as the database holds it: every pending change saved, then nothing tracked. (T155)</summary>
    private async Task<WombatIdentityUser> StoredAsync(string userId)
    {
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();
        return await Db.Users.AsNoTracking().SingleAsync(user => user.Id == userId);
    }

    private sealed class RecordingAuthenticationService : IAuthenticationService
    {
        public List<string> SignedInUserIds { get; } = [];

        /// <summary>The user name each cookie was built with.</summary>
        public List<string?> SignedInUserNames { get; } = [];

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            SignedInUserIds.Add(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "?");
            SignedInUserNames.Add(principal.FindFirstValue(ClaimTypes.Name));
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }

    /// <summary>
    /// Refuses <paramref name="address" /> as Identity's own validator refuses a user name another account took: the
    /// account that took it did so after the handler's in-use check had looked.
    /// </summary>
    private sealed class TakenBetweenCheckAndCreate(string address) : IUserValidator<WombatIdentityUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<WombatIdentityUser> manager, WombatIdentityUser user)
            => Task.FromResult(string.Equals(user.UserName, address, StringComparison.OrdinalIgnoreCase)
                ? IdentityResult.Failed(new IdentityErrorDescriber().DuplicateUserName(address))
                : IdentityResult.Success);
    }

    private sealed class RecordingAuditWriter
    {
        public List<AuditEntry> Entries { get; } = [];

        public IEnumerable<string> Actions => Entries.Select(entry => entry.Action);
    }

    /// <summary>Records the entry, then saves the context, flushing whatever it tracks, as <c>AuditWriter</c> does.</summary>
    private sealed class FlushingAuditWriter(RecordingAuditWriter log, ApplicationDbContext dbContext) : IAuditWriter
    {
        public async Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            log.Entries.Add(entry);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task WriteDiscardingPendingChangesAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            dbContext.ChangeTracker.Clear();
            await WriteAsync(entry, cancellationToken);
        }
    }
}

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
        services.AddSingleton<IAuditWriter>(_audit);
        services.Configure<SsoOptions>(options => options.Providers.Add(new SsoProviderOptions
        {
            Key = ProviderKey,
            DisplayName = "KGK sign-in",
            InstitutionId = ProviderInstitutionId,
            GroupsClaim = "groups"
        }));
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

    // ---- linking --------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ALinkWithTheRightPassword_BindsTheExternalLoginFromTheCookie_AndSignsIn()
    {
        var user = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);

        var result = await Handler.LinkAndSignInAsync(External("naidoo@kgk.test", subject: "idp-subject-1"), Password, "10.0.0.0", "test");

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

        var result = await Handler.LinkAndSignInAsync(External("mallory@kgk.test", subject: "mallory-subject"), Password, null, null);

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
        var external = External("naidoo@kgk.test", subject: "idp-subject-1");

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

        var wrongPassword = await Handler.LinkAndSignInAsync(External("naidoo@kgk.test", "s1"), "Wrong-Password-1", null, null);
        var noAccount = await Handler.LinkAndSignInAsync(External("nobody@kgk.test", "s2"), Password, null, null);
        var otherInstitution = await Handler.LinkAndSignInAsync(External("elsewhere@kgk.test", "s3"), Password, null, null);

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

        var result = await Handler.LinkAndSignInAsync(External("second@kgk.test", subject: "idp-subject-1"), Password, null, null);

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

        var result = await Handler.LinkAndSignInAsync(External("sso-only@kgk.test", "another-subject"), Password, null, null);

        result.ErrorMessage.Should().Be(ExternalLoginHandler.LinkRefusedMessage);
        (await Users.GetAccessFailedCountAsync(user)).Should().Be(0);
        (await Users.GetLoginsAsync(user)).Should().BeEmpty();
    }

    [Fact]
    public async Task AnAdministratorAccount_CannotBeLinked()
    {
        var admin = await CreateUserAsync("admin@kgk.test", ProviderInstitutionId);
        await GrantAsync(admin, "Administrator");

        var result = await Handler.LinkAndSignInAsync(External("admin@kgk.test", "idp-admin"), Password, null, null);

        result.ErrorMessage.Should().Be(ExternalLoginHandler.AdministratorMessage);
        (await Users.GetLoginsAsync(admin)).Should().BeEmpty();
        _authentication.SignedInUserIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ADeactivatedAccount_IsNotOfferedALink()
    {
        var user = await CreateUserAsync("naidoo@kgk.test", ProviderInstitutionId);
        await Users.SetLockoutEndDateAsync(user, UserDeactivation.IndefiniteLockoutEnd);

        var result = await Handler.HandleCallbackAsync(External("naidoo@kgk.test", "idp-subject-1"), null, null);

        result.RequiresLinking.Should().BeFalse();
        result.ErrorMessage.Should().Be(ExternalLoginHandler.LockedOutMessage);
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

    // Erasure's removal of external logins is tested on PostgreSQL (SsoErasurePostgresTests): the erasure executor runs
    // raw SQL the in-memory provider cannot.

    // ---- helpers --------------------------------------------------------------------------------------------------

    private async Task<WombatIdentityUser> CreateUserAsync(string email, int institutionId, string password = Password)
    {
        var user = new WombatIdentityUser
        {
            UserName = email,
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

    private async Task<WombatIdentityUser> LinkedUserAsync(string email, int institutionId, string subject)
    {
        var user = await CreateUserAsync(email, institutionId);
        (await Users.AddLoginAsync(user, new UserLoginInfo(ProviderKey, subject, "KGK sign-in"))).Succeeded.Should().BeTrue();
        return user;
    }

    private static ExternalLoginInfo External(string email, string subject, string? name = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.Email, email), new(ClaimTypes.NameIdentifier, subject) };
        if (name is not null)
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        return new ExternalLoginInfo(new ClaimsPrincipal(new ClaimsIdentity(claims, ProviderKey)), ProviderKey, subject, "KGK sign-in");
    }

    private sealed class RecordingAuthenticationService : IAuthenticationService
    {
        public List<string> SignedInUserIds { get; } = [];

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            SignedInUserIds.Add(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "?");
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }

    private sealed class RecordingAuditWriter : IAuditWriter
    {
        public List<string> Actions { get; } = [];

        public Task WriteAsync(AuditEntry entry, CancellationToken cancellationToken = default)
        {
            Actions.Add(entry.Action);
            return Task.CompletedTask;
        }

        public Task WriteDiscardingPendingChangesAsync(AuditEntry entry, CancellationToken cancellationToken = default)
            => WriteAsync(entry, cancellationToken);
    }
}

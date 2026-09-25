using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T279: a circuit's sign-in is checked against the account every minute, as the sign-in cookie is, and every change that
/// should end a session changes the account's security stamp, so the check signs it out.
/// </summary>
/// <remarks>
/// Every signed-in page is interactive, so after the first page a user's navigation never reaches an endpoint and the
/// cookie's check never runs for it. Until T279 the plain <c>ServerAuthenticationStateProvider</c> was registered, so a
/// circuit kept the principal it started with for as long as its tab was open: a lock, an erasure or a lost role never
/// reached it. Here the check is asked directly, over Identity as AddInfrastructure wires it and an in-memory store, after
/// each change an administrator makes through the app's own user service.
/// </remarks>
public sealed class SessionRevalidationTests
{
    [Fact]
    public void TheInterval_IsOneMinute_WithinTheOneToFiveMinutesT279Allows()
    {
        SessionRevalidation.Interval.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void TheCookiesCheck_AndTheCircuitsCheck_UseTheOneInterval()
    {
        var options = new SecurityStampValidatorOptions();
        SessionRevalidation.Configure(options);

        using var services = BuildServices();
        var provider = new SessionRevalidatingAuthenticationStateProvider(
            NullLoggerFactory.Instance, services.GetRequiredService<IServiceScopeFactory>());

        options.ValidationInterval.Should().Be(SessionRevalidation.Interval);
        provider.Interval.Should().Be(SessionRevalidation.Interval);
    }

    public static TheoryData<string> ChangesThatEndASession => new()
    {
        "an administrator locks the account",
        "an administrator removes a role",
        "an administrator adds a role",
        "the account moves to another institution",
        "the account's speciality scope changes",
        "an administrator resets the password",
        "the account is erased: its stamp changes",
        "the account is gone"
    };

    [Theory]
    [MemberData(nameof(ChangesThatEndASession))]
    public async Task AChangeToTheAccount_SignsTheCircuitOut_AtItsNextCheck(string change)
    {
        using var services = BuildServices();
        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        var (userId, principal) = await SignedInAssessorAsync(services);

        (await SessionRevalidatingAuthenticationStateProvider.IsCurrentAsync(scopes, principal))
            .Should().BeTrue($"guard: before {change}, the circuit's sign-in is current");

        await ChangeAsync(services, userId, change);

        (await SessionRevalidatingAuthenticationStateProvider.IsCurrentAsync(scopes, principal))
            .Should().BeFalse(change);
    }

    [Fact]
    public async Task ASaveThatChangesNeitherTheInstitutionNorTheScope_LeavesTheSessionAlone()
    {
        // An assessor profile saved with only its other fields changed calls the scope update with what is already there:
        // nothing a claim carries has changed, so nobody is signed out.
        using var services = BuildServices();
        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        var (userId, principal) = await SignedInAssessorAsync(services);

        await using (var scope = services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IUserAdministrationService>()
                .UpdateScopeAsync(userId, InstitutionA, [Speciality], []);
        }

        (await SessionRevalidatingAuthenticationStateProvider.IsCurrentAsync(scopes, principal)).Should().BeTrue();
    }

    [Fact]
    public async Task ANewSignIn_AfterTheChange_IsCurrent()
    {
        // The check ends the old sign-in, not the account: a principal built after the change carries the new stamp.
        using var services = BuildServices();
        var scopes = services.GetRequiredService<IServiceScopeFactory>();
        var (userId, _) = await SignedInAssessorAsync(services);

        await ChangeAsync(services, userId, "an administrator removes a role");
        var fresh = await PrincipalOfAsync(services, userId);

        (await SessionRevalidatingAuthenticationStateProvider.IsCurrentAsync(scopes, fresh)).Should().BeTrue();
    }

    // ---- the circuit's own loop (the T279 review) ----------------------------------------------------------------------
    //
    // The tests above ask IsCurrentAsync, the check, directly. These run the provider as a circuit runs it: its auth state
    // set, its revalidation loop waiting its interval (a few milliseconds here, the one thing the tests change) and asking
    // its override, and the circuit signed out by the base class when the override says no. So an override that did not
    // ask the check, or a loop that was never started, fails here.

    [Fact]
    public async Task AnOpenCircuit_IsSignedOut_WhenItsAccountChanges()
    {
        using var services = BuildServices();
        var (userId, principal) = await SignedInAssessorAsync(services);
        using var provider = CircuitProvider(services.GetRequiredService<IServiceScopeFactory>());
        var signedOut = SignedOutSignal(provider);

        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));
        await ChangeAsync(services, userId, "an administrator removes a role");

        var state = await signedOut.WaitAsync(TimeSpan.FromSeconds(30));
        state.User.Identities.Should().NotContain(identity => identity.IsAuthenticated, "the circuit is signed out");
        (await provider.GetAuthenticationStateAsync()).User.Identities.Should().NotContain(identity => identity.IsAuthenticated);
    }

    [Fact]
    public async Task AnOpenCircuit_WhoseAccountNobodyChanged_StaysSignedIn_CheckAfterCheck()
    {
        // The control: the same loop and interval, and nothing changed. Five checks pass, and the circuit keeps its sign-in.
        using var services = BuildServices();
        var (_, principal) = await SignedInAssessorAsync(services);
        var scopes = new CountingScopeFactory(services.GetRequiredService<IServiceScopeFactory>());
        using var provider = CircuitProvider(scopes);
        var signedOut = SignedOutSignal(provider);

        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));
        await WaitUntilAsync(() => scopes.Created >= 5);

        signedOut.IsCompleted.Should().BeFalse("no check failed");
        (await provider.GetAuthenticationStateAsync()).User.Should().BeSameAs(principal);
    }

    [Fact]
    public async Task AFaultInTheCheck_IsAskedAgain_AndOnlyTheThirdInARowSignsTheCircuitOut()
    {
        // A fault is not a verdict: the database restarting must not send every open tab to the sign-in page at once, as the
        // base class's rule (any exception signs out) did. Three in a row still do: a check that never succeeds ends the
        // session.
        using var services = BuildServices();
        var (_, principal) = await SignedInAssessorAsync(services);
        var scopes = new CountingScopeFactory(services.GetRequiredService<IServiceScopeFactory>()) { Faulting = true };
        using var provider = CircuitProvider(scopes);
        var signedOut = SignedOutSignal(provider);

        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));
        await signedOut.WaitAsync(TimeSpan.FromSeconds(30));

        SessionRevalidatingAuthenticationStateProvider.FaultsBeforeSignOut.Should().Be(3);
        scopes.Created.Should().Be(
            SessionRevalidatingAuthenticationStateProvider.FaultsBeforeSignOut,
            "the circuit was signed out at the third fault, not the first");
    }

    [Fact]
    public async Task ACheckThatCompletes_StartsTheCountOfFaultsAgain()
    {
        using var services = BuildServices();
        var (_, principal) = await SignedInAssessorAsync(services);
        var scopes = new CountingScopeFactory(services.GetRequiredService<IServiceScopeFactory>());
        using var provider = CircuitProvider(scopes);

        scopes.Faulting = true;
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeTrue("the first fault is asked again");
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeTrue("so is the second");

        scopes.Faulting = false;
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeTrue("the check completes, and passes");

        scopes.Faulting = true;
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeTrue("the count started again");
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeTrue();
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeFalse("the third fault in a row");
    }

    [Fact]
    public async Task ACheckThatCompletesAndFails_SignsOutAtOnce_WhateverFaultsCameBefore()
    {
        using var services = BuildServices();
        var (userId, principal) = await SignedInAssessorAsync(services);
        var scopes = new CountingScopeFactory(services.GetRequiredService<IServiceScopeFactory>());
        using var provider = CircuitProvider(scopes);

        scopes.Faulting = true;
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeTrue();

        scopes.Faulting = false;
        await ChangeAsync(services, userId, "an administrator locks the account");
        (await provider.CheckAsync(principal, CancellationToken.None)).Should().BeFalse(
            "a stamp that no longer matches is a verdict");
    }

    /// <summary>The provider as a circuit gets it, checked every few milliseconds instead of every minute.</summary>
    private static SessionRevalidatingAuthenticationStateProvider CircuitProvider(IServiceScopeFactory scopes)
        => new(NullLoggerFactory.Instance, scopes, TimeSpan.FromMilliseconds(20));

    /// <summary>Completes with the first auth state the provider announces that signs nobody in.</summary>
    private static Task<AuthenticationState> SignedOutSignal(AuthenticationStateProvider provider)
    {
        var signal = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.AuthenticationStateChanged += async task =>
        {
            var state = await task;
            if (!state.User.Identities.Any(identity => identity.IsAuthenticated))
            {
                signal.TrySetResult(state);
            }
        };
        return signal.Task;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (!condition())
        {
            DateTime.UtcNow.Should().BeBefore(deadline, "the loop should have checked by now");
            await Task.Delay(10);
        }
    }

    /// <summary>The app's scopes, counted, and failing as the database would while <see cref="Faulting" /> is on.</summary>
    private sealed class CountingScopeFactory(IServiceScopeFactory inner) : IServiceScopeFactory
    {
        private int _created;
        private volatile bool _faulting;

        public int Created => Volatile.Read(ref _created);

        public bool Faulting
        {
            get => _faulting;
            set => _faulting = value;
        }

        public IServiceScope CreateScope()
        {
            Interlocked.Increment(ref _created);
            if (_faulting)
            {
                throw new InvalidOperationException("A fault the test put here: the database has gone.");
            }

            return inner.CreateScope();
        }
    }

    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Speciality = 5;
    private const string Password = "Assessor-Pa55word!";

    private static async Task ChangeAsync(ServiceProvider services, string userId, string change)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserAdministrationService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();

        switch (change)
        {
            case "an administrator locks the account":
                await users.SetLockoutAsync(userId, locked: true);
                break;
            case "an administrator removes a role":
                await users.RemoveRoleAsync(userId, WombatRoles.Assessor);
                break;
            case "an administrator adds a role":
                await users.AddRoleAsync(userId, WombatRoles.CommitteeMember);
                break;
            case "the account moves to another institution":
                await users.UpdateScopeAsync(userId, InstitutionB, [Speciality], []);
                break;
            case "the account's speciality scope changes":
                await users.UpdateScopeAsync(userId, InstitutionA, [], []);
                break;
            case "an administrator resets the password":
                await users.ResetPasswordAsync(userId, "Replaced-Pa55word!");
                break;
            case "the account is erased: its stamp changes":
                // What ErasureExecutor does to the account: a new stamp, among the rest.
                var erased = await userManager.FindByIdAsync(userId);
                erased!.SecurityStamp = Guid.NewGuid().ToString();
                (await userManager.UpdateAsync(erased)).Succeeded.Should().BeTrue();
                break;
            case "the account is gone":
                var gone = await userManager.FindByIdAsync(userId);
                (await userManager.DeleteAsync(gone!)).Succeeded.Should().BeTrue();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }

    /// <summary>An assessor at A with one speciality scope, and the principal a sign-in would have built for them.</summary>
    private static async Task<(string UserId, ClaimsPrincipal Principal)> SignedInAssessorAsync(ServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { WombatRoles.Assessor, WombatRoles.CommitteeMember })
        {
            (await roles.CreateAsync(new IdentityRole(role))).Succeeded.Should().BeTrue();
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var user = new WombatIdentityUser
        {
            UserName = "assessor@example.test",
            Email = "assessor@example.test",
            FirstName = "Anna",
            LastName = "Assessor",
            InstitutionId = InstitutionA
        };
        (await userManager.CreateAsync(user, Password)).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(user, WombatRoles.Assessor)).Succeeded.Should().BeTrue();
        await scope.ServiceProvider.GetRequiredService<IUserAdministrationService>()
            .UpdateScopeAsync(user.Id, InstitutionA, [Speciality], []);

        return (user.Id, await PrincipalOfAsync(services, user.Id));
    }

    private static async Task<ClaimsPrincipal> PrincipalOfAsync(ServiceProvider services, string userId)
    {
        await using var scope = services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var factory = scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<WombatIdentityUser>>();
        return await factory.CreateAsync((await userManager.FindByIdAsync(userId))!);
    }

    /// <summary>Identity as AddInfrastructure wires it, with the app's user service, over an in-memory store.</summary>
    private static ServiceProvider BuildServices()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        return services.BuildServiceProvider();
    }
}

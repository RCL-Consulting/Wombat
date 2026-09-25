using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// A circuit's sign-in, checked against the account every <see cref="SessionRevalidation.Interval" />, as the sign-in
/// cookie is checked between requests: once the account's security stamp is no longer the one the circuit signed in with,
/// or the account is gone, the circuit is signed out. (T279)
/// </summary>
/// <remarks>
/// <para>
/// Every signed-in page is interactive (App.razor), so after the first page a user's navigation never reaches an endpoint,
/// and the cookie's own check (<see cref="SessionRevalidation.Configure" />) never runs for it. A circuit took its
/// principal from the request that opened it and kept it for as long as its tab stayed open: until T279 the plain
/// <see cref="ServerAuthenticationStateProvider" /> was registered and nothing checked it again, so a lock, an erasure or a
/// change of roles never reached a tab already open.
/// </para>
/// <para>
/// The check is the cookie's own (<see cref="SignInManager{TUser}.ValidateSecurityStampAsync(ClaimsPrincipal)" />), so a
/// circuit and a request answer the same question, and every change that should end a session changes the stamp
/// (<see cref="SessionRevalidation" />). A circuit that fails it becomes anonymous, every request its components send
/// carries a principal that no gate admits, and the tab leaves the circuit by a full page load through
/// <see cref="SessionEnd" />, which ends the browser's session too and shows the sign-in page (<see cref="EndedSessionExit" />,
/// the T279 review).
/// </para>
/// <para>
/// <b>A fault is not a verdict</b> (the T279 review). A check that throws (the database restarting, say) neither confirms
/// the sign-in nor ends it: it is logged and asked again at the next interval, and only
/// <see cref="FaultsBeforeSignOut" /> faults in a row sign the circuit out. The base class signed every open tab out at the
/// first fault, so a PostgreSQL restart would have sent every user to the sign-in page at once, losing whatever each had
/// typed. The cookie's check, on a request, answers a fault with an error page rather than a sign-out, so neither half
/// ends a session on a fault alone. What a tolerated fault leaves open is small: the circuit keeps the claims it had for
/// at most two more intervals, while the store it would act on is the one that is failing.
/// </para>
/// <para>
/// Each check runs in a scope of its own, so no database context outlives it inside the long-lived circuit.
/// </para>
/// </remarks>
public sealed class SessionRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    /// <summary>How many checks in a row may fault before the circuit is signed out: the third fault ends it.</summary>
    public const int FaultsBeforeSignOut = 3;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly TimeSpan _interval;
    private int _consecutiveFaults;

    /// <summary>The provider the app registers: checked every <see cref="SessionRevalidation.Interval" />.</summary>
    public SessionRevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        : this(loggerFactory, scopeFactory, SessionRevalidation.Interval)
    {
    }

    /// <summary>A provider checked every <paramref name="interval" />: for the tests, which cannot wait a minute.</summary>
    internal SessionRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        TimeSpan interval)
        : base(loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        _scopeFactory = scopeFactory;
        _logger = loggerFactory.CreateLogger<SessionRevalidatingAuthenticationStateProvider>();
        _interval = interval;
    }

    /// <inheritdoc />
    protected override TimeSpan RevalidationInterval => _interval;

    /// <summary>How often this circuit's sign-in is checked: <see cref="RevalidationInterval" />, for the tests.</summary>
    internal TimeSpan Interval => RevalidationInterval;

    /// <inheritdoc />
    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
        => CheckAsync(authenticationState.User, cancellationToken);

    /// <summary>
    /// The check made at each interval: whether <paramref name="user" /> is still current (<see cref="IsCurrentAsync" />),
    /// with a fault answered as still current until it is the <see cref="FaultsBeforeSignOut" />th in a row. A check that
    /// completes, either way, starts the count again.
    /// </summary>
    internal async Task<bool> CheckAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        try
        {
            var current = await IsCurrentAsync(_scopeFactory, user);
            _consecutiveFaults = 0;
            return current;
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _consecutiveFaults++;
            if (_consecutiveFaults >= FaultsBeforeSignOut)
            {
                _logger.LogError(
                    exception,
                    "A circuit's sign-in could not be checked {Faults} times in a row; the circuit is signed out.",
                    _consecutiveFaults);
                return false;
            }

            _logger.LogWarning(
                exception,
                "A circuit's sign-in could not be checked ({Faults} of {Allowed} faults in a row); it is checked again at the next interval.",
                _consecutiveFaults,
                FaultsBeforeSignOut);
            return true;
        }
    }

    /// <summary>
    /// Whether <paramref name="user" /> still signs in the account it names: the account exists, and its security stamp is
    /// the one the principal carries. The cookie's check, in a scope of its own.
    /// </summary>
    public static async Task<bool> IsCurrentAsync(IServiceScopeFactory scopeFactory, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(user);

        await using var scope = scopeFactory.CreateAsyncScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<WombatIdentityUser>>();
        return await signInManager.ValidateSecurityStampAsync(user) is not null;
    }
}

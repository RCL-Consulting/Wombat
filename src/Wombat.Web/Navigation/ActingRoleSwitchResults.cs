using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Wombat.Web.Navigation;

/// <summary>
/// The one-time word a switch leaves for the page it lands on (T335, flow 01; R2-Rules § 1): "You are now acting as …".
/// The switch endpoint writes it as a short-lived cookie; <c>App.razor</c> takes it, once, as it renders that page.
/// </summary>
/// <remarks>
/// <para>
/// A cookie because the switch is a redirect: the page that says what happened is the next request's. It is HttpOnly and
/// SameSite=Lax, protected with Data Protection so it cannot be written by hand, bound to the account that switched so no
/// one else signing in on the browser is told it, and good for <see cref="Lifetime" /> only.
/// </para>
/// <para>
/// Taken once. The first page rendered with it deletes it, and its nonce is remembered for as long as the cookie could
/// have lived, so the same value sent again (a reload of a stale request, a copied cookie) says nothing. The nonces are
/// held in memory: one server, and a restart forgets at most two minutes of them.
/// </para>
/// <para>
/// A forged, expired, replayed or someone else's value is refused silently: the page shows nothing, as it does with no
/// cookie at all.
/// </para>
/// <para>
/// <b>Said by one circuit</b> (the review of the t335 branch). The word reaches the page's circuit as a parameter of
/// <c>Routes</c>, written into the page's descriptor. <c>Blazor.resumeCircuit()</c> starts a new circuit from that
/// descriptor, the word included, so a paused or evicted page, resumed, said it again. The first circuit to render it
/// says it, and its nonce is remembered here (<see cref="SayInCircuit" />); any later circuit given the same word says
/// nothing. The prerender, which is no circuit, says it as before. Remembered for <see cref="SaidRetention" />, longer
/// than a circuit's state is kept to be resumed.
/// </para>
/// </remarks>
public sealed class ActingRoleSwitchResults
{
    /// <summary>The cookie's name.</summary>
    public const string CookieName = "wombat_role_switched";

    /// <summary>How long the word waits for its page: the redirect is followed at once, so this is generous.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    /// <summary>
    /// How long a word a circuit has said is remembered: the eight hours a sign-in lasts (the application cookie's
    /// <c>ExpireTimeSpan</c>), more than the two hours a circuit's state is kept to be resumed
    /// (<c>CircuitOptions.PersistedCircuitInMemoryRetentionPeriod</c>'s default). A switch is a person's click, so what is
    /// held is a few words an hour.
    /// </summary>
    public static readonly TimeSpan SaidRetention = TimeSpan.FromHours(8);

    private const string Purpose = "Wombat.Web.Navigation.ActingRoleSwitchResults.v1";

    private readonly ITimeLimitedDataProtector _protector;
    private readonly TimeProvider _time;
    private readonly ConcurrentDictionary<string, DateTimeOffset> _taken = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DateTimeOffset> _said = new(StringComparer.Ordinal);

    public ActingRoleSwitchResults(IDataProtectionProvider dataProtection, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(dataProtection);
        ArgumentNullException.ThrowIfNull(time);

        _protector = dataProtection.CreateProtector(Purpose).ToTimeLimitedDataProtector();
        _time = time;
    }

    /// <summary>Leaves the word for the next page: <paramref name="userId" /> switched from <paramref name="from" /> to <paramref name="to" />.</summary>
    public void Issue(HttpContext context, string userId, string? from, string to)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(to);

        var payload = JsonSerializer.Serialize(new Payload(userId, from, to, Convert.ToHexString(RandomNumberGenerator.GetBytes(16))));
        var value = _protector.Protect(payload, _time.GetUtcNow() + Lifetime);

        context.Response.Cookies.Append(CookieName, value, CookieOptions(context, Lifetime));
    }

    /// <summary>
    /// The word left for this request's signed-in user, taken: the cookie is deleted, and its value says nothing again.
    /// Null when there is none, or when it is forged, expired, already taken or someone else's.
    /// </summary>
    public ActingRoleSwitchResult? Take(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Cookies.TryGetValue(CookieName, out var value))
        {
            return null;
        }

        Forget(context);

        Payload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<Payload>(_protector.Unprotect(value, out _));
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            return null;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (payload is null
            || string.IsNullOrEmpty(payload.Nonce)
            || string.IsNullOrEmpty(payload.To)
            || userId is null
            || !string.Equals(payload.UserId, userId, StringComparison.Ordinal))
        {
            return null;
        }

        var now = _time.GetUtcNow();
        ForgetExpired(_taken, now);

        return _taken.TryAdd(payload.Nonce, now + Lifetime)
            ? new ActingRoleSwitchResult(payload.From, payload.To, payload.Nonce)
            : null;
    }

    /// <summary>
    /// Whether a circuit may say <paramref name="result" />: true for the first circuit to ask, false for every later one,
    /// which is a circuit resumed from the page's descriptor and given the same word again. A result with no nonce has
    /// nothing to remember it by, and is said.
    /// </summary>
    public bool SayInCircuit(ActingRoleSwitchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (string.IsNullOrEmpty(result.Nonce))
        {
            return true;
        }

        var now = _time.GetUtcNow();
        ForgetExpired(_said, now);
        return _said.TryAdd(result.Nonce, now + SaidRetention);
    }

    /// <summary>Deletes the cookie, if the response can still say so: at sign-out, and once it is taken.</summary>
    public static void Forget(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Response.HasStarted && context.Request.Cookies.ContainsKey(CookieName))
        {
            context.Response.Cookies.Delete(CookieName, CookieOptions(context, maxAge: null));
        }
    }

    private static void ForgetExpired(ConcurrentDictionary<string, DateTimeOffset> nonces, DateTimeOffset now)
    {
        foreach (var (nonce, until) in nonces)
        {
            if (until <= now)
            {
                nonces.TryRemove(nonce, out _);
            }
        }
    }

    private static CookieOptions CookieOptions(HttpContext context, TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = context.Request.IsHttps,
        Path = "/",
        IsEssential = true,
        MaxAge = maxAge
    };

    private sealed record Payload(string UserId, string? From, string To, string Nonce);
}

using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Wombat.Web.Security;

/// <summary>
/// What the register page holds for the browser that opened an invitation's link: the token, and after a refusal the
/// refusal codes and the two names, in one Data Protection cookie for <see cref="Lifetime" /> from when the link was opened
/// (T373, flow 11, S1, S2; T315, T341).
/// </summary>
/// <remarks>
/// <para>
/// Until T373 the token stood in the page's address (<c>/account/register?token=…</c>), which a script took out of the
/// address bar after the page loaded, and a refused registration came back to <c>?token=…&amp;error=…</c>: the token in
/// the browser's history and in every refused redirect, and the names dropped. Now the mail's link opens
/// <see cref="RegisterOutcome.StartPath" />, which keeps the token here and answers 303 to the page with no query, so the
/// token leaves the address bar by a redirect, and no redirect ever holds a name, a code or a token.
/// </para>
/// <para>
/// HttpOnly, SameSite=Lax, on the register paths only (<see cref="RegisterOutcome.PagePath" />, which covers the start
/// and the submit beneath it), Secure as the request's scheme (as <c>ActingRoleSwitchResults</c>, so the http replay
/// registers), and protected, so it cannot be written by hand. Never a password. Deleted when the registration succeeds.
/// A refusal rewrites it with the same end, so the 30 minutes run from when the link was opened, as G14's words say.
/// </para>
/// <para>
/// A forged, lapsed or unreadable value reads as no cookie. A submit that arrives with none (the 30 minutes passed with
/// the form open) leaves <see cref="RegisterState.TimedOut" /> in its place, so the page says the time has passed (G16)
/// rather than that the link is incomplete (G13), without a code in the address.
/// </para>
/// </remarks>
public sealed class RegisterCookie
{
    /// <summary>The cookie's name.</summary>
    public const string CookieName = "wombat_register";

    /// <summary>How long the page holds an invitation for, from when its link was opened (S1; G14 and G16 say it).</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    /// <summary>
    /// The longest token the start path holds (T373 fix pass, B7). A token is 43 characters; one far longer would make a
    /// cookie the browser drops, and the person would read "Invitation link incomplete" with no reason why, so it is read
    /// as no token at once.
    /// </summary>
    public const int LongestToken = 256;

    private const string Purpose = "Wombat.Web.Security.RegisterCookie.v1";

    private readonly IDataProtector _protector;
    private readonly TimeProvider _time;

    public RegisterCookie(IDataProtectionProvider dataProtection, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(dataProtection);
        ArgumentNullException.ThrowIfNull(time);

        // Not a time-limited protector: its clock is the system's, and the end is the payload's own, judged by _time, so a
        // test can pin it.
        _protector = dataProtection.CreateProtector(Purpose);
        _time = time;
    }

    /// <summary>Whether <paramref name="token" /> is one the start path holds: not blank, and no longer than <see cref="LongestToken" />.</summary>
    public static bool CanHold(string? token) => !string.IsNullOrWhiteSpace(token) && token.Length <= LongestToken;

    /// <summary>
    /// The form's binding to the invitation it shows (T373 fix pass, B1): the first 16 hex characters of the token's
    /// SHA-256. The page writes it in a hidden field and the submit compares it with the cookie's token, so a form shown for
    /// one invitation never registers another whose link this browser opened since (the one cookie holds the last link
    /// opened). It is not the token, and the token cannot be had from it.
    /// </summary>
    public static string Fingerprint(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)))[..16];
    }

    /// <summary>Whether <paramref name="sent" />, the form's binding, is <paramref name="token" />'s; a form without one is not.</summary>
    public static bool Fingerprints(string token, string? sent)
        => !string.IsNullOrEmpty(sent)
           && CryptographicOperations.FixedTimeEquals(
               System.Text.Encoding.ASCII.GetBytes(Fingerprint(token)),
               System.Text.Encoding.ASCII.GetBytes(sent));

    /// <summary>Holds <paramref name="token" /> for <see cref="Lifetime" /> from now, with no refusal and no names.</summary>
    public void Open(HttpContext context, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        Write(context, new RegisterState(token, [], null, null, TimedOut: false, Until: _time.GetUtcNow() + Lifetime));
    }

    /// <summary>Says, for the next page, that a submit arrived after the invitation's time had passed (G16).</summary>
    public void TimeOut(HttpContext context)
        => Write(context, new RegisterState(null, [], null, null, TimedOut: true, Until: _time.GetUtcNow() + Lifetime));

    /// <summary>Writes <paramref name="state" />, good until its own end; a state already past its end deletes the cookie.</summary>
    public void Write(HttpContext context, RegisterState state)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(state);

        var left = state.Until - _time.GetUtcNow();
        if (left <= TimeSpan.Zero)
        {
            Delete(context);
            return;
        }

        var value = _protector.Protect(JsonSerializer.Serialize(state));
        context.Response.Cookies.Append(CookieName, value, Options(context, left));
    }

    /// <summary>What the cookie holds; null when there is none, or it is forged, lapsed or unreadable.</summary>
    public RegisterState? Read(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Cookies.TryGetValue(CookieName, out var value) || string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            var state = JsonSerializer.Deserialize<RegisterState>(_protector.Unprotect(value));
            return state is not null && state.Until > _time.GetUtcNow() && (state.TimedOut || !string.IsNullOrWhiteSpace(state.Token))
                ? state with { Codes = state.Codes ?? [] }
                : null;
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            return null;
        }
    }

    /// <summary>Deletes the cookie, if the response can still say so: once the registration succeeds.</summary>
    public static void Delete(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Response.HasStarted)
        {
            context.Response.Cookies.Delete(CookieName, Options(context, maxAge: null));
        }
    }

    private static CookieOptions Options(HttpContext context, TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = context.Request.IsHttps,
        Path = RegisterOutcome.PagePath,
        IsEssential = true,
        MaxAge = maxAge
    };
}

/// <summary>What <see cref="RegisterCookie" /> holds.</summary>
/// <param name="Token">The invitation's token; null only in the timed-out state.</param>
/// <param name="Codes">The codes of the refusal the last submit met (<see cref="RegisterOutcome" />); empty when none.</param>
/// <param name="FirstName">The first name the refused submit carried, so the form keeps it (T317's registration half).</param>
/// <param name="LastName">The last name it carried.</param>
/// <param name="TimedOut">A submit arrived with no cookie: the invitation's 30 minutes had passed (G16).</param>
/// <param name="Until">When the cookie lapses: 30 minutes from when the link was opened, kept through a refusal.</param>
public sealed record RegisterState(
    string? Token,
    IReadOnlyList<string> Codes,
    string? FirstName,
    string? LastName,
    bool TimedOut,
    DateTimeOffset Until);

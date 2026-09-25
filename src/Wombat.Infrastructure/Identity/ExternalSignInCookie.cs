using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Identity's external cookie (<see cref="IdentityConstants.ExternalScheme" />): an institutional sign-in the provider has
/// completed, held until the callback signs its account in, or until the person links it to their account with their
/// password (T149). (T156)
/// </summary>
/// <remarks>
/// <para>
/// <b>It does not slide.</b> Identity gives it five minutes, and cookie authentication slides by default: every request
/// that reads it past the halfway mark issued it again for five more. The link page reads it as it loads, so reloading
/// the page kept a provider's sign-in alive for as long as someone kept at it, however long ago the provider had
/// authenticated them, and after the provider had revoked them. It now ends five minutes after the provider's callback.
/// </para>
/// <para>
/// <b>Its name carries the <c>__Host-</c> prefix</b>, so a browser takes it only from this host, over a secure
/// connection, for the whole site, with no <c>Domain</c>. Another <c>rcl.co.za</c> subdomain can set a cookie for the
/// parent domain that every subdomain receives. Unprefixed, it could plant an external sign-in under this cookie's name
/// in a visitor's browser, one it obtained from Wombat through a provider account it controls, and the callback and the
/// link page would then act on that identity as the visitor's. A prefixed cookie cannot be set that way, and the
/// unprefixed name is no longer read.
/// </para>
/// <para>
/// The prefix requires the <c>Secure</c> attribute, so the cookie is always issued Secure. Behind Caddy the request is
/// HTTPS anyway (forwarded headers). A browser treats <c>http://localhost</c> as a secure origin, so development over
/// plain HTTP keeps working: Chrome kept a <c>__Host-</c> Secure cookie set by <c>http://localhost</c> and sent it back
/// (observed 2026-09-25). The provider's correlation and nonce cookies were already Secure always, so SSO in development
/// relied on this before.
/// </para>
/// </remarks>
public static class ExternalSignInCookie
{
    /// <summary>The cookie's name: Identity's, with the <c>__Host-</c> prefix.</summary>
    public static readonly string Name = "__Host-" + IdentityConstants.ExternalScheme;

    /// <summary>How long it lasts from the provider's callback: Identity's own five minutes, kept.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    public static void Configure(CookieAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Cookie.Name = Name;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Path = "/";
        options.Cookie.Domain = null;
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = Lifetime;
        options.SlidingExpiration = false;
    }
}

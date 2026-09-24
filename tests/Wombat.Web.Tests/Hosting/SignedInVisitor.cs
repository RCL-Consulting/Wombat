using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// Signs every request to an <see cref="AppTestHost" /> in, as Identity's cookie does for a user who has signed in.
/// </summary>
/// <remarks>
/// Pass <see cref="Register" /> to <see cref="AppTestHost.StartAsync" />. It becomes the scheme requests are
/// authenticated by; the host's cookie scheme still answers a challenge, so nothing else about the host changes. The
/// user holds no role and no scope, so it reaches only a page that asks for nothing beyond a signed-in user.
/// </remarks>
internal sealed class SignedInVisitor(
    IOptionsMonitor<SignedInVisitor.VisitorOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<SignedInVisitor.VisitorOptions>(options, logger, encoder)
{
    private const string SchemeName = nameof(SignedInVisitor);

    /// <summary>A user with one identity, signed in: what Identity's cookie gives.</summary>
    public static void Register(IServiceCollection services) => Register(services, anonymousIdentityFirst: false);

    /// <summary>
    /// A user whose first identity is anonymous and whose second is signed in. The fallback policy counts them as signed
    /// in, since it asks whether any identity is, although <see cref="ClaimsPrincipal.Identity" />, the first, is not.
    /// </summary>
    public static void RegisterBehindAnAnonymousIdentity(IServiceCollection services)
        => Register(services, anonymousIdentityFirst: true);

    private static void Register(IServiceCollection services, bool anonymousIdentityFirst)
    {
        services.AddAuthentication().AddScheme<VisitorOptions, SignedInVisitor>(
            SchemeName, options => options.AnonymousIdentityFirst = anonymousIdentityFirst);
        services.Configure<AuthenticationOptions>(options => options.DefaultAuthenticateScheme = SchemeName);
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var signedIn = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "signed-in-visitor"), new Claim(ClaimTypes.Name, "Signed-in visitor")],
            SchemeName);
        ClaimsIdentity[] identities = Options.AnonymousIdentityFirst ? [new ClaimsIdentity(), signedIn] : [signedIn];
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identities), SchemeName)));
    }

    internal sealed class VisitorOptions : AuthenticationSchemeOptions
    {
        public bool AnonymousIdentityFirst { get; set; }
    }
}

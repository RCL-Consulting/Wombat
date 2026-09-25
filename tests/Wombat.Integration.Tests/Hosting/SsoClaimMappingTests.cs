using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Integration.Tests.MultiSourceFeedback;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T155: the OIDC registration in <c>Wombat.Web</c>'s <c>Program.cs</c> keeps each provider's email-verification claim
/// from the userinfo response, so the external login carries it however the provider sends it.
/// </summary>
/// <remarks>
/// The ID token's claims all reach the external login. From the userinfo response the handler keeps only the claims its
/// <see cref="OpenIdConnectOptions.ClaimActions" /> name, and the defaults name <c>email</c> but not <c>email_verified</c>.
/// Without the mapping, a provider that asserts verification only there would never be seen to verify an address: SSO
/// would sync no email and provision no account for it. The actions are run here on a userinfo document as the handler
/// runs them.
/// </remarks>
public sealed class SsoClaimMappingTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string Issuer = "https://idp.kgk.test";

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public SsoClaimMappingTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task EachProvidersVerificationClaim_IsKeptFromTheUserinfoResponse()
    {
        await using var app = _host.Factory.WithWebHostBuilder(builder =>
        {
            Provider(builder, 0, "kgk", verifiedClaim: null);
            Provider(builder, 1, "kgk-entra", verifiedClaim: "xms_edov");
        });

        var options = app.Services.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>();
        using var userinfo = JsonDocument.Parse(
            """{ "sub": "idp-subject-1", "email": "naidoo@kgk.test", "email_verified": true, "xms_edov": true }""");

        var standard = Mapped(options.Get("kgk"), userinfo);
        standard.FindFirst("email_verified").Should().NotBeNull("the default claim, OIDC Core § 5.1");
        bool.Parse(standard.FindFirst("email_verified")!.Value).Should().BeTrue();
        standard.HasClaim(claim => claim.Type == "xms_edov").Should().BeFalse("only the claim the provider is configured with");

        var entra = Mapped(options.Get("kgk-entra"), userinfo);
        entra.FindFirst("xms_edov").Should().NotBeNull("the claim this provider's configuration names");
        bool.Parse(entra.FindFirst("xms_edov")!.Value).Should().BeTrue();
        entra.HasClaim(claim => claim.Type == "email_verified").Should().BeFalse();
    }

    private static void Provider(IWebHostBuilder builder, int index, string key, string? verifiedClaim)
    {
        var prefix = $"Sso:Providers:{index}:";
        builder.UseSetting(prefix + "Key", key);
        builder.UseSetting(prefix + "DisplayName", key);
        builder.UseSetting(prefix + "InstitutionId", "1");
        builder.UseSetting(prefix + "Authority", Issuer);
        builder.UseSetting(prefix + "ClientId", "wombat");
        builder.UseSetting(prefix + "ClientSecret", "not-a-secret");
        if (verifiedClaim is not null)
        {
            builder.UseSetting(prefix + "EmailVerifiedClaim", verifiedClaim);
        }
    }

    /// <summary>What the handler keeps from a userinfo response: each claim action run on it, into the identity.</summary>
    private static ClaimsIdentity Mapped(OpenIdConnectOptions options, JsonDocument userinfo)
    {
        var identity = new ClaimsIdentity("oidc");
        foreach (var action in options.ClaimActions)
        {
            action.Run(userinfo.RootElement, identity, Issuer);
        }

        return identity;
    }
}

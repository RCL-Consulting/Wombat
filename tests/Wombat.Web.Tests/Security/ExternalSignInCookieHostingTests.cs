using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T156: the external cookie as a browser receives it, and as the cookie handler reads it back. It holds a provider's
/// sign-in until its account is signed in or linked, and until T156 it slid: the link page reads it as it loads, so
/// reloading the page kept the sign-in alive, past its revocation at the provider.
/// </summary>
/// <remarks>
/// A host of its own, on a clock the test moves, with the one cookie scheme configured by
/// <see cref="ExternalSignInCookie.Configure" />, as AddInfrastructure configures Identity's (which
/// <c>ExternalSignInCookieTests</c> checks in the infrastructure suite).
/// </remarks>
public sealed class ExternalSignInCookieHostingTests
{
    private const string Subject = "provider-subject-1";

    [Fact]
    public async Task TheCookie_IsHostOnlyAndSecure_AndEndsFiveMinutesAfterTheCallback_HoweverOftenItIsRead()
    {
        var clock = new MovableClock();
        await using var app = await StartAsync(clock);
        using var client = app.GetTestClient();

        using var callback = await client.GetAsync("/callback");
        var issued = SetCookies(callback).Should().ContainSingle().Which;
        issued.Should().StartWith("__Host-Identity.External=");
        var attributes = issued.Split(';').Skip(1).Select(part => part.Trim().ToLowerInvariant()).ToList();
        attributes.Should().Contain("secure", "a browser drops a __Host- cookie without it");
        attributes.Should().Contain("path=/");
        attributes.Should().NotContain(attribute => attribute.StartsWith("domain=", StringComparison.Ordinal));
        var cookie = issued.Split(';')[0];

        // Read every minute, as reloading the link page reads it: past the halfway mark it is not issued again.
        for (var minute = 1; minute <= 4; minute++)
        {
            clock.MoveForward(TimeSpan.FromMinutes(1));
            using var read = await ReadAsync(client, cookie);
            (await read.Content.ReadAsStringAsync()).Should().Be(Subject, $"minute {minute}: still inside its five minutes");
            SetCookies(read).Should().BeEmpty($"minute {minute}: the cookie does not slide");
        }

        clock.MoveForward(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        using var expired = await ReadAsync(client, cookie);
        (await expired.Content.ReadAsStringAsync()).Should().Be("nobody", "five minutes after the callback, however often it was read");
    }

    private static async Task<WebApplication> StartAsync(TimeProvider clock)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        builder.Services.AddAuthentication()
            .AddCookie(IdentityConstants.ExternalScheme, ExternalSignInCookie.Configure);
        builder.Services.PostConfigure<CookieAuthenticationOptions>(
            IdentityConstants.ExternalScheme, options => options.TimeProvider = clock);

        var app = builder.Build();

        // What the provider's handler does at its callback: the sign-in it completed goes into the external cookie.
        app.MapGet("/callback", (HttpContext context) => context.SignInAsync(
            IdentityConstants.ExternalScheme,
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, Subject)], "oidc"))));

        // What the link page does as it loads: reads the sign-in back.
        app.MapGet("/link", async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
            await context.Response.WriteAsync(result.Succeeded ? result.Principal!.Identity!.Name! : "nobody");
        });

        await app.StartAsync();
        return app;
    }

    private static Task<HttpResponseMessage> ReadAsync(HttpClient client, string cookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/link");
        request.Headers.Add("Cookie", cookie);
        return client.SendAsync(request);
    }

    private static IReadOnlyList<string> SetCookies(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies.ToList() : [];

    private sealed class MovableClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);

        public void MoveForward(TimeSpan by) => _now += by;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}

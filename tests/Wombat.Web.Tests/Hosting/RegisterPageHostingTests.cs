using System.Net;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// Complete registration as a browser receives it (T373, flow 11): static for a visitor signed in or not, so neither opens
/// a circuit; one h1 for the form and for each card, which is also the tab's title; and no script of the page's own, its
/// toggles being wombat.js's (G17) and the token gone by the start path's redirect, not by a script after load (T315).
/// </summary>
/// <remarks>
/// The start path and the submit are Program.cs's own endpoints, which this host does not map: the integration suite runs
/// them (<c>Hosting/AccountRefusalFlowTests</c>).
/// </remarks>
public sealed class RegisterPageHostingTests
{
    private const string Token = "invitation-token";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ThePage_IsStatic_SignedInOrNot_WithOneHeading_ThatIsTheTabsTitle(bool signedIn)
    {
        await using var host = await StartAsync(signedIn);

        var (response, html, document) = await LoadAsync(host, withCookie: true);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ServerComponentMarker.IsIn(html).Should().BeFalse("Register is static, signed in (G18) or not: no circuit");
        var heading = signedIn ? "Signed in as someone else" : "Complete registration";
        document.QuerySelectorAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be(heading);
        document.Title.Should().Be($"{heading} · Wombat");
        document.QuerySelector("nav").Should().BeNull("the auth card: no navigation");
    }

    [Fact]
    public async Task WithNoCookie_ThePageIsTheIncompleteLinksCard()
    {
        await using var host = await StartAsync(signedIn: false);

        var (_, _, document) = await LoadAsync(host, withCookie: false);

        document.QuerySelectorAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Invitation link incomplete");
        document.Title.Should().Be("Invitation link incomplete · Wombat");
        document.QuerySelector("form").Should().BeNull();
    }

    [Fact]
    public async Task ThePage_RunsNoScriptOfItsOwn()
    {
        await using var host = await StartAsync(signedIn: false);

        var (_, html, document) = await LoadAsync(host, withCookie: true);

        document.QuerySelectorAll("script:not([src])").Select(script => script.GetAttribute("type"))
            .Should().OnlyContain(type => type == "importmap", "App's import map is the one inline script, nonce-backed");
        var (_, _, other) = await host.LoadAsync(AppTestHost.AnonymousPage);
        document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src"))
            .Should().Equal(other.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")),
                "App's own scripts, as on every page, and none of the page's");
        html.Should().NotContain("onclick").And.NotContain("clearInvitationTokenFromUrl");
        document.QuerySelectorAll("[style]").Should().BeEmpty("S20: no inline style");
        document.QuerySelector("form[action='/account/register/submit']").Should().NotBeNull();
    }

    private static Task<AppTestHost> StartAsync(bool signedIn)
        => AppTestHost.StartAsync(services =>
        {
            services.AddSingleton<IScopedSender>(new PreviewSender());
            services.AddSingleton<RegisterCookie>();
            services.AddLogging();
            if (signedIn)
            {
                SignedInVisitor.Register(services);
            }
        });

    private static async Task<(HttpResponseMessage Response, string Html, AngleSharp.Html.Dom.IHtmlDocument Document)> LoadAsync(
        AppTestHost host, bool withCookie)
    {
        if (withCookie)
        {
            var writer = new DefaultHttpContext();
            host.Services.GetRequiredService<RegisterCookie>().Open(writer, Token);
            host.Client.DefaultRequestHeaders.Add(
                "Cookie", $"{RegisterCookie.CookieName}={RegisterCookieValues.ValueIn(writer.Response.Headers.SetCookie.ToString())}");
        }

        return await host.LoadAsync(RegisterOutcome.PagePath);
    }

    private sealed class PreviewSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetInvitationPreviewQuery { Token: Token }
                ? Task.FromResult((TResponse)(object)new InvitationPreviewDto(
                    "smit@kgk.wombat.local", WombatRoles.Coordinator, "Kgosi Kgari Teaching Hospital", null, null, null,
                    new DateOnly(2026, 11, 2)))
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}

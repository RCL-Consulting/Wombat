using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// Every stylesheet and script <c>App.razor</c> links is addressed by its content hash, so a deploy that changes one
/// changes its URL and no browser pairs a new page with an old copy (T175).
/// </summary>
/// <remarks>
/// Before T175, <c>app.css</c> was linked as <c>/app.css</c>. <c>UseStaticFiles</c> answers that path with no
/// Cache-Control, so browsers cached it heuristically, and T125's browser check rendered its new per-year editor with
/// the old stylesheet: selects about 1000px wide until a forced re-fetch.
/// </remarks>
public sealed partial class AppAssetUrlTests
{
    /// <summary>The first-party stylesheets and scripts App.razor links, by the label the manifest files them under.</summary>
    private static readonly string[] LinkedAssets =
    [
        "app.css",
        "Wombat.Web.styles.css",
        "wombat.js",
        "js/dialog.js",
        "js/file-download.js",
        "_framework/blazor.web.js",
    ];

    [Fact]
    public async Task EveryStylesheetAndScript_IsLinkedByItsContentHashedUrl_WhichIsCachedAsImmutable()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);
        var manifest = StaticAssetsManifest.Load();

        var urls = document.QuerySelectorAll("link[rel=stylesheet]").Select(link => link.GetAttribute("href"))
            .Concat(document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")))
            .Select(url => url!.TrimStart('/'))
            .ToList();

        urls.Should().NotBeEmpty();
        foreach (var url in urls)
        {
            manifest.Should().ContainKey(url, $"'{url}' must be a route MapStaticAssets serves");
            var route = manifest[url];
            route.Label.Should().NotBeNull(
                $"'{url}' is the bare, unhashed route; link it through @Assets[\"{url}\"] so its URL carries the hash");
            route.CacheControl.Should().Contain("immutable", $"'{url}' is content-addressed, so it can be cached forever");
        }

        urls.Select(url => manifest[url].Label).Should().Contain(
            LinkedAssets, "every first-party stylesheet and script is still linked, each through its hashed URL");
    }

    [Fact]
    public async Task TheHashedStylesheet_IsServedAsCss_WithAYearLongImmutableCache()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);

        var href = document.QuerySelectorAll("link[rel=stylesheet]")
            .Select(link => link.GetAttribute("href")!)
            .Should().ContainSingle(url => HashedAppCss().IsMatch(url), "app.css is linked once, by its hashed URL")
            .Which;

        using var response = await host.Client.GetAsync(href);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/css");
        response.Headers.CacheControl!.ToString().Should().Be("max-age=31536000, immutable");
        (await response.Content.ReadAsStringAsync()).Should().Contain("--primary-color", "it is the design system's app.css");
    }

    /// <summary>
    /// The hashed routes are endpoints, so the fallback policy (a signed-in user on every endpoint) covers them unless they
    /// opt out. Without the opt-out each one redirects to the sign-in page, and the sign-in page itself renders with no
    /// stylesheet and no script. The import map's modules are included: <c>ReconnectModal</c>'s was the first to break so.
    /// </summary>
    [Fact]
    public async Task EveryStylesheetScriptAndModule_ReachesAVisitorWhoHasNotSignedIn()
    {
        await using var host = await AppTestHost.StartAsync();

        // The host carries the real fallback policy, so a 200 below is the opt-out's doing and not a missing policy's.
        var fallback = await host.Services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();
        fallback.Should().NotBeNull("every endpoint that does not opt out requires a signed-in user");
        fallback!.Requirements.Should().ContainSingle().Which.Should().BeOfType<DenyAnonymousAuthorizationRequirement>();

        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);
        var importMap = document.QuerySelectorAll("script")
            .Should().ContainSingle(script => script.GetAttribute("type") == "importmap").Which.TextContent;
        using var imports = JsonDocument.Parse(importMap);
        var modules = imports.RootElement.GetProperty("imports").EnumerateObject()
            .Select(entry => entry.Value.GetString()!)
            .ToList();
        modules.Should().NotBeEmpty("the import map names ReconnectModal's module");

        var urls = document.QuerySelectorAll("link[rel=stylesheet]").Select(link => link.GetAttribute("href")!)
            .Concat(document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")!))
            .Concat(modules)
            .ToList();

        foreach (var url in urls)
        {
            using var response = await host.Client.GetAsync(url);
            response.StatusCode.Should().Be(
                HttpStatusCode.OK,
                $"'{url}' is on the sign-in page, so it must reach a visitor who has not signed in; it was answered with " +
                $"{(int)response.StatusCode} to '{response.Headers.Location}'");
            response.Content.Headers.ContentType?.MediaType.Should().BeOneOf(
                ["text/css", "text/javascript"], $"'{url}' is a stylesheet or a script, not the sign-in page");
            (await response.Content.ReadAsByteArrayAsync()).Should().NotBeEmpty($"'{url}' is served whole");
        }
    }

    /// <summary>
    /// Linking the scripts through their hashed URLs keeps the CSP's <c>script-src 'self' 'nonce-…'</c> true (T097): each
    /// script is same-origin, and the one inline script, the import map, carries this response's nonce.
    /// </summary>
    [Fact]
    public async Task EveryScript_IsSameOriginOrCarriesTheResponsesNonce()
    {
        await using var host = await AppTestHost.StartAsync();
        var (response, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);

        // The framework adds a second, frame-ancestors-only policy of its own to a response that renders interactive
        // server components. A browser enforces both; the one that governs scripts is Wombat's.
        var policy = response.Headers.GetValues("Content-Security-Policy")
            .Should().ContainSingle(value => value.Contains("script-src")).Which;
        var nonce = CspNonce().Match(policy).Groups["nonce"].Value;
        nonce.Should().NotBeEmpty("script-src is nonce-backed");

        var origin = host.Client.BaseAddress!;
        var scripts = document.QuerySelectorAll("script").ToList();
        scripts.Should().Contain(script => script.GetAttribute("type") == "importmap");
        foreach (var script in scripts)
        {
            if (script.GetAttribute("src") is { } src)
            {
                new Uri(origin, src).Authority.Should().Be(origin.Authority, $"'{src}' is served from this origin");
            }
            else
            {
                script.GetAttribute("nonce").Should().Be(nonce, "an inline script runs only with this response's nonce");
            }
        }
    }

    [GeneratedRegex(@"^app\.[a-z0-9]+\.css$")]
    private static partial Regex HashedAppCss();

    [GeneratedRegex(@"script-src 'self' 'nonce-(?<nonce>[^']+)'")]
    private static partial Regex CspNonce();
}

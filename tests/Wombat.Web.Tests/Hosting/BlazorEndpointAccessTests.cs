using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// What a visitor who has not signed in may reach of Blazor's own endpoints, and what their pages ask of it (T181).
/// </summary>
/// <remarks>
/// <para>
/// The sign-in pages logged <c>Unexpected token '&lt;', "&lt;!DOCTYPE "... is not valid JSON</c> on every load. They
/// were interactive, so <c>blazor.web.js</c> set out to open a circuit, and its first step, a fetch of
/// <c>/_blazor/initializers</c>, met the fallback policy: a 302 to the sign-in page, whose HTML it parsed as JSON.
/// </para>
/// <para>
/// Opening that endpoint alone would only have moved the error: the next step, negotiating the circuit, meets the same
/// policy. The circuit is not opened, because inside one the fallback policy no longer guards a page, so a signed-out
/// visitor's pages are static instead and never ask for one. The host maps <c>MapWombatRazorComponents</c> under the real
/// fallback policy, as <c>Program.cs</c> does.
/// </para>
/// </remarks>
public sealed partial class BlazorEndpointAccessTests
{
    private const string SignInPath = "/account/login";

    /// <summary>The pages a visitor lands on before signing in, each rendering the whole of <c>App</c>.</summary>
    public static TheoryData<string> SignInPages => new() { SignInPath, AppTestHost.AnonymousPage };

    /// <summary>The shapes of signed-in user the fallback policy accepts, by name (<see cref="SignIn" />).</summary>
    public static TheoryData<string> SignedInUsers => new() { OneIdentity, BehindAnAnonymousIdentity };

    private const string OneIdentity = "one identity, signed in";
    private const string BehindAnAnonymousIdentity = "an anonymous identity, then a signed-in one";

    [Fact]
    public async Task TheInitializersList_AnswersAVisitorWhoHasNotSignedIn_WithJson()
    {
        await using var host = await AppTestHost.StartAsync();
        await ShouldCarryTheFallbackPolicy(host);

        // The address blazor.web.js fetches: "_blazor/initializers" against the page's <base href="/">.
        using var response = await host.Client.GetAsync("/_blazor/initializers");

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"the list is fetched on a page the visitor reaches before signing in; it was answered with " +
            $"{(int)response.StatusCode} to '{response.Headers.Location}'");
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json", "the script parses it as JSON");

        using var list = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        list.RootElement.ValueKind.Should().Be(JsonValueKind.Array, "the script maps it as a list of module names");
        list.RootElement.EnumerateArray().Should().OnlyContain(name => name.ValueKind == JsonValueKind.String);
    }

    /// <summary>
    /// The render mode maps four endpoints, and the circuit is three of them. A new one fails the first assertion, so it
    /// is judged here rather than left to the fallback policy's default.
    /// </summary>
    [Fact]
    public async Task OfBlazorsEndpoints_OnlyTheInitializersList_IsOpenToAVisitorWhoHasNotSignedIn()
    {
        await using var host = await AppTestHost.StartAsync();

        var endpoints = host.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => Route(endpoint).StartsWith("_blazor", StringComparison.Ordinal))
            .ToList();

        endpoints.Select(Route).Should().BeEquivalentTo(
            ["_blazor", "_blazor/negotiate", "_blazor/disconnect", RazorComponentEndpoints.InitializersRoute],
            "the interactive server render mode maps the hub, its negotiation, its disconnect beacon and the initializers");
        endpoints.Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null).Select(Route)
            .Should().Equal(
                [RazorComponentEndpoints.InitializersRoute],
                "the circuit stays behind the fallback policy: inside one, navigation never meets an endpoint's policy");
        endpoints.Should().OnlyContain(
            endpoint => endpoint.Metadata.GetMetadata<IAuthorizeData>() == null
                && endpoint.Metadata.GetMetadata<AuthorizationPolicy>() == null
                && endpoint.Metadata.GetMetadata<IAuthorizationRequirementData>() == null,
            "none carries a policy of its own, in any of the three forms the authorization middleware combines, so the " +
            "fallback policy is what closes the circuit to a visitor who has not signed in, and all that is asked of a " +
            "signed-in user");
    }

    /// <summary>
    /// The circuit's endpoints, as the script calls them. Negotiation carries the framework's no-redirect marker, so the
    /// cookie scheme answers it 401; the disconnect beacon, a plain endpoint, is sent to the sign-in page.
    /// </summary>
    [Theory]
    [InlineData("POST", "/_blazor/negotiate?negotiateVersion=1", HttpStatusCode.Unauthorized)]
    [InlineData("GET", "/_blazor?id=circuit-connection", HttpStatusCode.Unauthorized)]
    [InlineData("POST", "/_blazor/disconnect/", HttpStatusCode.Found)]
    public async Task TheCircuit_RefusesAVisitorWhoHasNotSignedIn(string method, string path, HttpStatusCode refusal)
    {
        await using var host = await AppTestHost.StartAsync();

        using var response = await host.Client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        response.StatusCode.Should().Be(refusal, $"{method} {path} is part of the circuit, which is a signed-in user's");
        if (refusal == HttpStatusCode.Found)
        {
            response.Headers.Location!.AbsolutePath.Should().Be(SignInPath);
        }
    }

    /// <summary>
    /// The circuit still opens for a signed-in user: negotiation answers them with a connection token. Every other case
    /// on the circuit is signed out, so without this one a policy that closed it to everyone would pass them all, and
    /// every signed-in page would lose its interactivity. Both shapes of signed-in user the fallback policy accepts are
    /// accepted here.
    /// </summary>
    [Theory]
    [MemberData(nameof(SignedInUsers))]
    public async Task TheCircuit_Negotiates_WithASignedInUser(string user)
    {
        await using var host = await AppTestHost.StartAsync(SignIn(user));

        using var response = await host.Client.PostAsync("/_blazor/negotiate?negotiateVersion=1", content: null);

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"a signed-in user's pages are interactive, and their circuit starts here; it was answered with " +
            $"{(int)response.StatusCode} to '{response.Headers.Location}'");
        using var negotiation = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        negotiation.RootElement.GetProperty("connectionToken").GetString().Should().NotBeNullOrEmpty(
            "the script connects to the hub with the token negotiation hands it");
    }

    /// <summary>
    /// The page is static, so the script finds no interactive root and never asks for a circuit, and there is no refusal
    /// for the console to log. Before T181 each of these pages carried two roots, the head outlet and the router.
    /// </summary>
    [Theory]
    [MemberData(nameof(SignInPages))]
    public async Task ASignInPage_GivesAVisitorWhoHasNotSignedIn_NoInteractiveRoot_SoTheScriptAsksForNoCircuit(string path)
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, html, document) = await host.LoadAsync(path);

        ServerComponentMarker().IsMatch(html).Should().BeFalse(
            "an interactive server root would have blazor.web.js open a circuit, which the hub refuses a visitor who " +
            "has not signed in");
        document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")!)
            .Should().Contain(src => src.Contains("blazor.web", StringComparison.Ordinal),
                "the script still loads: it runs enhanced navigation and form posts on a static page");
    }

    /// <summary>
    /// The same page, signed in: the head outlet and the router are each an interactive server root. "Signed in" is what
    /// the hub's fallback policy accepts, any identity authenticated, so a user whose first identity is anonymous gets
    /// the circuit <see cref="TheCircuit_Negotiates_WithASignedInUser" /> shows the hub gives them.
    /// </summary>
    [Theory]
    [MemberData(nameof(SignedInUsers))]
    public async Task TheSamePage_SignedIn_IsInteractive(string user)
    {
        await using var host = await AppTestHost.StartAsync(SignIn(user));

        var (_, html, _) = await host.LoadAsync(AppTestHost.AnonymousPage);

        ServerComponentMarker().Matches(html).Should().HaveCount(
            2, "a signed-in user's every page is interactive: the head outlet and the router run in their circuit");
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/admin/users")]
    public async Task APageThatRequiresSignIn_StillSendsAVisitorWhoHasNotSignedIn_ToTheSignInPage(string path)
    {
        await using var host = await AppTestHost.StartAsync();

        using var response = await host.Client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Found, $"'{path}' is for a signed-in user");
        response.Headers.Location!.PathAndQuery.Should().Be($"{SignInPath}?ReturnUrl={Uri.EscapeDataString(path)}");
    }

    /// <summary>A 200 from a Blazor endpoint is only an opt-out's doing if the host carries the real fallback policy.</summary>
    private static async Task ShouldCarryTheFallbackPolicy(AppTestHost host)
    {
        var fallback = await host.Services.GetRequiredService<IAuthorizationPolicyProvider>().GetFallbackPolicyAsync();
        fallback.Should().NotBeNull("every endpoint that does not opt out requires a signed-in user");
        fallback!.Requirements.Should().ContainSingle().Which.Should().BeOfType<DenyAnonymousAuthorizationRequirement>();
    }

    private static Action<IServiceCollection> SignIn(string user) => user switch
    {
        OneIdentity => SignedInVisitor.Register,
        BehindAnAnonymousIdentity => SignedInVisitor.RegisterBehindAnAnonymousIdentity,
        _ => throw new ArgumentOutOfRangeException(nameof(user), user, null),
    };

    private static string Route(RouteEndpoint endpoint) => endpoint.RoutePattern.RawText!.Trim('/');

    [GeneratedRegex("""<!--Blazor:\{[^>]*"type":"server""")]
    private static partial Regex ServerComponentMarker();
}

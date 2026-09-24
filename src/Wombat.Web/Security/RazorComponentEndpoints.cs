using Microsoft.AspNetCore.Authorization;
using Wombat.Web.Components;

namespace Wombat.Web.Security;

/// <summary>
/// The Razor-components endpoints: every routable page, and the interactive server render mode's own endpoints under
/// <c>/_blazor</c>, of which one, the JS-initializers list, is open to a visitor who has not signed in (T181).
/// </summary>
/// <remarks>
/// <para>
/// <c>AuthorizationPolicies</c> sets a fallback policy of <c>RequireAuthenticatedUser</c>, and the render mode's endpoints
/// declare no policy of their own, so each one requires a signed-in user unless it opts out. Four are mapped:
/// <c>/_blazor/initializers/</c>, <c>/_blazor/negotiate</c>, <c>/_blazor</c> (the hub) and <c>/_blazor/disconnect/</c>.
/// </para>
/// <para>
/// <c>blazor.web.js</c> fetches the initializers list as the first step of opening a circuit and parses it as JSON.
/// Under the fallback policy a visitor who had not signed in got a 302 to the sign-in page instead, so the script parsed
/// that page's HTML and logged <c>Unexpected token '&lt;'</c> on every sign-in page. The list names the libraries'
/// JavaScript initializer modules, which are static assets any visitor can already fetch, so opening it discloses
/// nothing.
/// </para>
/// <para>
/// The circuit itself stays closed. Inside a circuit, navigation never reaches an endpoint, so the fallback policy no
/// longer guards a page that relies on it rather than on <c>[Authorize]</c>; an anonymous circuit would render such a
/// page to anyone. Opening the list alone would only have moved the sign-in pages' console error to the next step, a
/// refused negotiation, so <c>App.razor</c> gives a visitor who has not signed in no interactive root at all, and the
/// script never sets out to open a circuit for them. That is what keeps their console clean; the list is open as well
/// because a JSON endpoint that answers with the sign-in page is wrong for whoever does fetch it.
/// </para>
/// <para>
/// <c>Hosting/BlazorEndpointAccessTests</c> maps this method under the real fallback policy and fails if a second
/// <c>/_blazor</c> endpoint opens, or the initializers endpoint closes.
/// </para>
/// </remarks>
internal static class RazorComponentEndpoints
{
    /// <summary>The initializers endpoint's route, as the framework maps it, without its slashes.</summary>
    internal const string InitializersRoute = "_blazor/initializers";

    public static RazorComponentsEndpointConventionBuilder MapWombatRazorComponents(this IEndpointRouteBuilder endpoints)
    {
        var components = endpoints.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // A convention on the Razor-components builder reaches the render mode's endpoints too; this one names only the
        // initializers route, so the pages and the circuit's endpoints keep the fallback policy.
        components.Add(endpoint =>
        {
            if (endpoint is RouteEndpointBuilder route
                && string.Equals(route.RoutePattern.RawText?.Trim('/'), InitializersRoute, StringComparison.Ordinal))
            {
                endpoint.Metadata.Add(new AllowAnonymousAttribute());
            }
        });

        return components;
    }
}

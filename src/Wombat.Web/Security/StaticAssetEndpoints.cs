using Microsoft.AspNetCore.StaticAssets;

namespace Wombat.Web.Security;

/// <summary>
/// The fingerprinted static-asset endpoints, open to a visitor who has not signed in.
/// </summary>
/// <remarks>
/// <para>
/// <c>AuthorizationPolicies</c> sets a fallback policy of <c>RequireAuthenticatedUser</c>, which every endpoint
/// inherits unless it opts out, and the hashed routes <c>MapStaticAssets</c> registers are endpoints. Without the
/// opt-out, an anonymous request for one redirects to <c>/account/login</c> and the browser receives HTML in its place.
/// Since T175 that is every stylesheet and script <c>App.razor</c> links (<c>app.&lt;hash&gt;.css</c> and the rest,
/// through <c>@Assets</c>), so the sign-in page would render unstyled and scriptless; before it, the case was
/// <c>ReconnectModal.&lt;hash&gt;.razor.js</c>, referenced from the import map, whose subresource-integrity check failed.
/// Bare paths (the icons, the web manifest) are unaffected because <c>UseStaticFiles</c> answers them before
/// authorization runs, which is what made this asymmetric and easy to miss.
/// </para>
/// <para>
/// It is a method of its own so that <c>Hosting/AppAssetUrlTests</c> maps exactly what <c>Program.cs</c> maps, under the
/// real fallback policy, and fails if the opt-out goes.
/// </para>
/// </remarks>
internal static class StaticAssetEndpoints
{
    public static StaticAssetsEndpointConventionBuilder MapWombatStaticAssets(this IEndpointRouteBuilder endpoints)
        => endpoints.MapStaticAssets().AllowAnonymous();
}

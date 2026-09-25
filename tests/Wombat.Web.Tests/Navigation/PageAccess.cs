using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Components.Layout;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// Which page a link opens, and whether a role is let in, judged as the router and the endpoint's authorization judge
/// them. T178 wrote these for the nav; T261 shares them with the role dashboards, so both hold every link they offer to
/// the same rule: it opens a page that exists and admits the role offering it.
/// </summary>
internal static class PageAccess
{
    // The app's own authorization: its policies (AddWombatAuthorization), the default policy for a bare [Authorize], and
    // the fallback policy for a page that declares nothing, as the endpoint's authorization applies them.
    private static readonly ServiceProvider Authorization = new ServiceCollection()
        .AddLogging()
        .AddWombatAuthorization()
        .BuildServiceProvider();

    /// <summary>
    /// The routed page an href opens, or null when none answers it. A literal route wins over a parameterised one, as in
    /// the router: /activities/new is not /activities/{Id}.
    /// </summary>
    public static Type? PageFor(string href)
    {
        var path = "/" + href.TrimStart('/');
        var pages = typeof(NavMenu).Assembly.GetTypes()
            .SelectMany(t => t.GetCustomAttributes<RouteAttribute>().Select(r => (Page: t, r.Template)))
            .ToList();

        return pages.FirstOrDefault(p => string.Equals(p.Template, path, StringComparison.OrdinalIgnoreCase)).Page
            ?? pages.FirstOrDefault(p => Matches(p.Template, path)).Page;
    }

    /// <summary>Null when the role gets in; otherwise why not.</summary>
    public static Task<string?> RefusalOf(Type page, string role) => RefusalOf(page, PrincipalFor(role), $"a {role}");

    /// <summary>Null when this holder gets in; otherwise why not. For a holder no single role describes (T252).</summary>
    public static Task<string?> RefusalOf(Type page, ClaimsPrincipal principal) => RefusalOf(page, principal, "this holder");

    private static async Task<string?> RefusalOf(Type page, ClaimsPrincipal principal, string who)
    {
        if (page.GetCustomAttributes<AllowAnonymousAttribute>(inherit: true).Any())
        {
            return null;
        }

        var policies = Authorization.GetRequiredService<IAuthorizationPolicyProvider>();
        var declared = page.GetCustomAttributes(inherit: true).OfType<IAuthorizeData>().ToList();
        var policy = declared.Count == 0
            ? await policies.GetFallbackPolicyAsync()
            : await AuthorizationPolicy.CombineAsync(policies, declared);
        if (policy is null)
        {
            return null;
        }

        var result = await Authorization.GetRequiredService<IAuthorizationService>().AuthorizeAsync(principal, policy);
        return result.Succeeded
            ? null
            : $"{page.Name} refuses {who}: {string.Join("; ", result.Failure!.FailedRequirements)}";
    }

    /// <summary>
    /// A signed-in holder of the role, carrying the scope claims every holder of it carries (InvitationRules.ValidateScope)
    /// and no more, so a page gated on a scope claim is judged as it would be for the least-scoped holder.
    /// </summary>
    public static ClaimsPrincipal PrincipalFor(string role)
    {
        string[] scopes = role switch
        {
            WombatRoles.CollegeAdmin => [WombatClaims.CollegeId],
            WombatRoles.InstitutionalAdmin or WombatRoles.Coordinator or WombatRoles.CommitteeMember => [WombatClaims.InstitutionId],
            WombatRoles.SpecialityAdmin => [WombatClaims.InstitutionId, WombatClaims.SpecialityId],
            WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee
                => [WombatClaims.InstitutionId, WombatClaims.SpecialityId, WombatClaims.SubSpecialityId],
            _ => []
        };

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "user"), new(ClaimTypes.Role, role) };
        claims.AddRange(scopes.Select(scope => new Claim(scope, "1")));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }

    /// <summary>The directory holding Wombat.sln, for a test that reads a document or a source file.</summary>
    public static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    private static bool Matches(string template, string path)
    {
        var want = template.Trim('/').Split('/');
        var got = path.Trim('/').Split('/');

        return want.Length == got.Length
            && want.Zip(got).All(s => s.First.StartsWith('{') || string.Equals(s.First, s.Second, StringComparison.OrdinalIgnoreCase));
    }
}

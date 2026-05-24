using System.Security.Claims;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;

namespace Wombat.Application.Tests.TestHelpers;

/// <summary>
/// Synthetic principals for scope-guard tests. T056 made handlers principal-aware;
/// these helpers keep tests terse without leaking ASP.NET Identity infrastructure.
/// </summary>
internal static class TestPrincipals
{
    public static ClaimsPrincipal Administrator(string userId = "admin-user")
        => Build(userId, new[] { WombatRoles.Administrator }, institutionId: null);

    public static ClaimsPrincipal InstitutionalAdmin(int institutionId, string userId = "inst-admin-user")
        => Build(userId, new[] { WombatRoles.InstitutionalAdmin }, institutionId);

    public static ClaimsPrincipal Anonymous() => new();

    private static ClaimsPrincipal Build(string userId, IEnumerable<string> roles, int? institutionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

using System.Security.Claims;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;

namespace Wombat.Application.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetInstitutionId(this ClaimsPrincipal principal)
        => principal.GetSingleIntClaim(WombatClaimTypes.InstitutionId);

    public static bool IsAdministrator(this ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Administrator);

    public static bool IsInstitutionalAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.InstitutionalAdmin);

    /// <summary>
    /// True if the caller is a global Administrator, or an InstitutionalAdmin whose
    /// institution-id claim matches the target institution. Used by handlers reachable
    /// from the AdministratorOrInstitutionalAdmin policy as a second line of defence.
    /// </summary>
    public static bool CanAccessInstitution(this ClaimsPrincipal principal, int institutionId)
    {
        if (principal.IsAdministrator())
        {
            return true;
        }

        if (!principal.IsInstitutionalAdmin())
        {
            return false;
        }

        var scopedInstitutionId = principal.GetInstitutionId();
        return scopedInstitutionId.HasValue && scopedInstitutionId.Value == institutionId;
    }

    public static IReadOnlyCollection<int> GetSpecialityIds(this ClaimsPrincipal principal)
        => principal.GetIntClaims(WombatClaimTypes.SpecialityId);

    public static IReadOnlyCollection<int> GetSubSpecialityIds(this ClaimsPrincipal principal)
        => principal.GetIntClaims(WombatClaimTypes.SubSpecialityId);

    public static bool IsInSpeciality(this ClaimsPrincipal principal, int specialityId)
        => principal.GetSpecialityIds().Contains(specialityId);

    public static bool IsInSubSpeciality(this ClaimsPrincipal principal, int subSpecialityId)
        => principal.GetSubSpecialityIds().Contains(subSpecialityId);

    public static bool IsInRole(this ClaimsPrincipal principal, string role)
        => principal.Claims.Any(claim =>
            claim.Type == ClaimTypes.Role &&
            string.Equals(claim.Value, role, StringComparison.Ordinal));

    private static int? GetSingleIntClaim(this ClaimsPrincipal principal, string claimType)
    {
        var value = principal.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value;
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static IReadOnlyCollection<int> GetIntClaims(this ClaimsPrincipal principal, string claimType)
        => principal.Claims
            .Where(claim => claim.Type == claimType)
            .Select(claim => int.TryParse(claim.Value, out var parsed) ? parsed : (int?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .Distinct()
            .ToArray();
}

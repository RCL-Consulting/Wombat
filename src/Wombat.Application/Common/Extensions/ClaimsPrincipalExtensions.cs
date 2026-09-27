using System.Security.Claims;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;

namespace Wombat.Application.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetInstitutionId(this ClaimsPrincipal principal)
        => principal.GetSingleIntClaim(WombatClaimTypes.InstitutionId);

    public static int? GetCollegeId(this ClaimsPrincipal principal)
        => principal.GetSingleIntClaim(WombatClaimTypes.CollegeId);

    /// <summary>
    /// The signed-in user's id, from the NameIdentifier claim; a caller without one is refused before anything is read
    /// or written. For the requests that act on the caller's own account and nobody else's. (T185)
    /// </summary>
    /// <remarks>
    /// <c>CommitteeDecisionAuthorization.GetRequiredUserId</c> and <c>EntrustmentDecisionAuthorization.GetRequiredUserId</c>
    /// are older copies of this, which accept a blank id.
    /// </remarks>
    public static string GetRequiredUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? throw new UnauthorizedAccessException("The current user identifier is missing.")
            : userId;
    }

    /// <summary>
    /// The person's name as the shell shows it (<see cref="WombatClaimTypes.DisplayName" />, issued at sign-in: their name,
    /// or their email when the account has none). A principal from a cookie issued before the claim existed is named by
    /// its sign-in name. Null for one with neither. (T335, flow 01)
    /// </summary>
    public static string? GetDisplayName(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var displayName = principal.FindFirst(WombatClaimTypes.DisplayName)?.Value;
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var name = principal.Identity?.Name;
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    public static bool IsAdministrator(this ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Administrator);

    public static bool IsCollegeAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.CollegeAdmin);

    public static bool IsInstitutionalAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.InstitutionalAdmin);

    /// <summary>
    /// True if the caller is a global Administrator, or a CollegeAdmin whose college-id claim
    /// matches the target college. Used by national-catalogue handlers reachable from the
    /// AdministratorOrCollegeAdmin policy as a second line of defence (mirrors
    /// <see cref="CanAccessInstitution"/>). (T091)
    /// </summary>
    public static bool CanAccessCollege(this ClaimsPrincipal principal, int collegeId)
    {
        if (principal.IsAdministrator())
        {
            return true;
        }

        if (!principal.IsCollegeAdmin())
        {
            return false;
        }

        var scopedCollegeId = principal.GetCollegeId();
        return scopedCollegeId.HasValue && scopedCollegeId.Value == collegeId;
    }

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

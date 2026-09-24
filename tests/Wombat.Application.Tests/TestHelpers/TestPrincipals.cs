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
        => Build(userId, new[] { WombatRoles.Administrator }, institutionId: null, collegeId: null);

    public static ClaimsPrincipal InstitutionalAdmin(int institutionId, string userId = "inst-admin-user")
        => Build(userId, new[] { WombatRoles.InstitutionalAdmin }, institutionId, collegeId: null);

    public static ClaimsPrincipal CollegeAdmin(int collegeId, string userId = "college-admin-user")
        => Build(userId, new[] { WombatRoles.CollegeAdmin }, institutionId: null, collegeId);

    /// <summary>A trainee. Every signed-in user carries an institution claim, trainees included. (T113)</summary>
    public static ClaimsPrincipal Trainee(string userId, int? institutionId = null)
        => Build(userId, new[] { WombatRoles.Trainee }, institutionId, collegeId: null);

    public static ClaimsPrincipal Coordinator(int institutionId, string userId = "coordinator-user")
        => Build(userId, new[] { WombatRoles.Coordinator }, institutionId, collegeId: null);

    /// <summary>
    /// Any single role, with the scope claims a real sign-in would carry for it: speciality and sub-speciality ids are
    /// national, so a scoped admin is only meaningful together with an institution. (T113)
    /// </summary>
    public static ClaimsPrincipal InRole(
        string role,
        string userId,
        int? institutionId,
        int? specialityId = null,
        int? subSpecialityId = null)
        => Build(userId, new[] { role }, institutionId, collegeId: null, specialityId, subSpecialityId);

    public static ClaimsPrincipal Anonymous() => new();

    private static ClaimsPrincipal Build(
        string userId,
        IEnumerable<string> roles,
        int? institutionId,
        int? collegeId,
        int? specialityId = null,
        int? subSpecialityId = null)
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

        if (collegeId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.CollegeId, collegeId.Value.ToString()));
        }

        if (specialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, specialityId.Value.ToString()));
        }

        if (subSpecialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpecialityId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}

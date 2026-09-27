using Microsoft.AspNetCore.Identity;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Identity;

public class WombatIdentityUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? InstitutionId { get; set; }

    /// <summary>
    /// The national College a <c>CollegeAdmin</c> is scoped to. Null for every other role.
    /// Surfaced as the <see cref="WombatClaims.CollegeId"/> claim at login so the national-catalogue
    /// handlers can scope by college. (T093)
    /// </summary>
    public int? CollegeId { get; set; }

    public bool OptOutOfOptionalProcessing { get; set; }
    public bool OptOutOfDigestEmails { get; set; }

    /// <summary>
    /// When false, the user cannot set a password and the local login form refuses them.
    /// Defaults to true for invitation-provisioned users, false for SSO-provisioned users.
    /// </summary>
    public bool AllowLocalPassword { get; set; } = true;

    /// <summary>
    /// The role whose frame (navigation, landing, head) the person last chose, by its key; null until they first switch.
    /// A view preference only: access is the union of the roles held, whatever this says. Stored with the account, not in
    /// the browser, so it follows the person across sign-ins and the next person on the same browser gets their own
    /// (T335, flow 01, D1; W-010; T317). Written only by the switch endpoint, and carried into the sign-in cookie as the
    /// <see cref="WombatClaims.ActingRole"/> claim; a stored role the person no longer holds is ignored where it is read.
    /// </summary>
    public string? ActingRole { get; set; }

    public WombatUser ToDomainUser()
        => new()
        {
            UserName = UserName ?? string.Empty,
            Email = Email ?? string.Empty,
            FirstName = FirstName,
            LastName = LastName,
            InstitutionId = InstitutionId,
            SpecialityIds = SpecialityScopes.Select(scope => scope.SpecialityId).ToArray(),
            SubSpecialityIds = SubSpecialityScopes.Select(scope => scope.SubSpecialityId).ToArray()
        };

    public ICollection<WombatIdentityUserSpecialityScope> SpecialityScopes { get; set; } = [];
    public ICollection<WombatIdentityUserSubSpecialityScope> SubSpecialityScopes { get; set; } = [];
}

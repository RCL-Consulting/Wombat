using Microsoft.AspNetCore.Identity;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Identity;

public class WombatIdentityUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? InstitutionId { get; set; }
    public bool OptOutOfOptionalProcessing { get; set; }
    public bool OptOutOfDigestEmails { get; set; }

    /// <summary>
    /// When false, the user cannot set a password and the local login form refuses them.
    /// Defaults to true for invitation-provisioned users, false for SSO-provisioned users.
    /// </summary>
    public bool AllowLocalPassword { get; set; } = true;

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

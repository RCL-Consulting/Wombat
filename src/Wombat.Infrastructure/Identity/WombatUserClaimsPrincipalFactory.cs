using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Wombat.Infrastructure.Identity;

public sealed class WombatUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<WombatIdentityUser, IdentityRole>
{
    public WombatUserClaimsPrincipalFactory(
        UserManager<WombatIdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(WombatIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (user.InstitutionId.HasValue)
        {
            identity.AddClaim(new Claim(WombatClaims.InstitutionId, user.InstitutionId.Value.ToString()));
        }

        foreach (var specialityId in user.SpecialityScopes.Select(scope => scope.SpecialityId).Distinct())
        {
            identity.AddClaim(new Claim(WombatClaims.SpecialityId, specialityId.ToString()));
        }

        foreach (var subSpecialityId in user.SubSpecialityScopes.Select(scope => scope.SubSpecialityId).Distinct())
        {
            identity.AddClaim(new Claim(WombatClaims.SubSpecialityId, subSpecialityId.ToString()));
        }

        return identity;
    }
}
